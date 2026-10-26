using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BongoBro
{
    public class MainForm : Form
    {
        const int WM_KEYDOWN = 0x100, WM_KEYUP = 0x101, WM_SYSKEYDOWN = 0x104, WM_SYSKEYUP = 0x105;
        const int WM_MOUSEMOVE = 0x200, WM_LBUTTONDOWN = 0x201, WM_LBUTTONUP = 0x202, WM_RBUTTONDOWN = 0x204, WM_RBUTTONUP = 0x205,
                  WM_MBUTTONDOWN = 0x207, WM_MBUTTONUP = 0x208, WM_MOUSEWHEEL = 0x20A;

        public const float MinScale = 0.15f, MaxScale = 1.5f, MinCounterScale = 0.5f, MaxCounterScale = 2f;

        readonly Pose pose = new Pose();
        readonly Random rnd = new Random();
        readonly Dictionary<int, bool> held = new Dictionary<int, bool>();
        Bitmap canvas;
        float scale;
        readonly Timer timer;
        NotifyIcon tray;
        ContextMenuStrip menu;
        ToolStripMenuItem miBox, miAch, miUpdate, miChar;
        UpdateForm upd;
        int lastUpdateCheck;
        string notifiedVersion, balloonKind;
        MenuSlider charSlider, counterSlider;
        CounterForm counter;
        Native.HookProc kbProc, msProc;
        IntPtr kbHook = IntPtr.Zero, msHook = IntPtr.Zero;
        bool nextRight, mouseDown, shown = true;
        int leftPress = -100000, rightPress = -100000, lastMouse = -100000, lastKey = -100000;
        int blinkUntil, nextBlink, boxFxUntil = -100000, lastSave;
        string lastSig = "", fxText = "";
        readonly Queue<int> keyTimes = new Queue<int>();
        int lastDayCheck;
        AchievementsForm ach;
        LeaderboardForm lb;
        int lastLbSubmit = Environment.TickCount - Leaderboard.SubmitEveryMs + 20000; // first report ~20 s after start
        ShopForm shop;
        LootForm loot;

        public MainForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "BongoBro";
            if (Program.AppIcon != null) Icon = Program.AppIcon;
            TopMost = Game.S.TopMost;
            if (Game.S.Scale < MinScale) Game.S.Scale = MinScale;
            if (Game.S.Scale > MaxScale) Game.S.Scale = MaxScale;
            scale = Game.S.Scale * Ui.Dpi;
            int sz = CanvasPx();
            Size = new Size(sz, sz);
            Point loc = new Point(Game.S.X, Game.S.Y);
            if (Game.S.X == int.MinValue || !OnScreen(loc, sz)) loc = DefaultLocation(sz);
            Location = loc;
            BuildMenu();
            BuildTray();
            timer = new Timer();
            timer.Interval = 33;
            timer.Tick += OnTick;
            nextBlink = Environment.TickCount + 3000;
            lastSave = Environment.TickCount;
        }

        int CanvasPx() { return (int)Math.Ceiling(360 * scale); }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80000 | 0x80 | 0x08000000; // layered | toolwindow | noactivate
                return cp;
            }
        }

        protected override bool ShowWithoutActivation { get { return true; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            kbProc = KbHook;
            msProc = MsHook;
            IntPtr mod = Native.GetModuleHandle(null);
            kbHook = Native.SetWindowsHookEx(13, kbProc, mod, 0);
            msHook = Native.SetWindowsHookEx(14, msProc, mod, 0);
            Game.Unlocked += OnUnlocked;
            Game.TouchDay();
            Game.CheckAchievements();
            lastDayCheck = Environment.TickCount;
            counter = new CounterForm(this);
            PositionCounter();
            counter.Show();
            LocationChanged += delegate
            {
                Game.S.X = Left; Game.S.Y = Top; Game.Dirty = true;
                PositionCounter();
            };
            Render();
            timer.Start();
            if (Program.Updated) Balloon("loot", "BongoBro обновлён", "Установлена версия " + Program.Version + ". Прогресс на месте.");
            CheckForUpdates();
            // test hook: exit the way the updater does, to check shutdown is clean
            int exitAfter;
            if (int.TryParse(Environment.GetEnvironmentVariable("BONGO_SELFTEST_EXIT"), out exitAfter))
            {
                Timer quit = new Timer();
                quit.Interval = exitAfter;
                quit.Tick += delegate { quit.Stop(); Application.Exit(); };
                quit.Start();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            timer.Stop();
            if (kbHook != IntPtr.Zero) Native.UnhookWindowsHookEx(kbHook);
            if (msHook != IntPtr.Zero) Native.UnhookWindowsHookEx(msHook);
            kbHook = msHook = IntPtr.Zero;
            Game.Save();
            tray.Visible = false;
            tray.Dispose();
            // Do not close other forms here: Application.Exit() enumerates the open forms while
            // raising FormClosing, and closing one of them now throws "collection was modified".
            if (counter != null) counter.Hide();
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (counter != null) counter.Dispose();
            base.OnFormClosed(e);
        }

        // ---------------- input hooks ----------------
        IntPtr KbHook(int code, IntPtr w, IntPtr l)
        {
            if (code >= 0)
            {
                int msg = w.ToInt32();
                int vk = Marshal.ReadInt32(l);
                if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                {
                    if (!held.ContainsKey(vk))
                    {
                        bool right = nextRight;
                        nextRight = !nextRight;
                        held[vk] = right;
                        int now = Environment.TickCount;
                        if (right) rightPress = now; else leftPress = now;
                        lastKey = now;
                        pose.LitKey = (vk * 7 + rnd.Next(3)) % Art.KeyCount;
                        if (Game.AddKey()) OnBoxEarned();
                        keyTimes.Enqueue(now);
                        while (now - keyTimes.Peek() > 60000) keyTimes.Dequeue();
                        Game.ReportKpm(keyTimes.Count);
                    }
                }
                else if (msg == WM_KEYUP || msg == WM_SYSKEYUP) held.Remove(vk);
            }
            return Native.CallNextHookEx(kbHook, code, w, l);
        }

        IntPtr MsHook(int code, IntPtr w, IntPtr l)
        {
            if (code >= 0)
            {
                int msg = w.ToInt32();
                switch (msg)
                {
                    case WM_LBUTTONDOWN:
                    case WM_RBUTTONDOWN:
                    case WM_MBUTTONDOWN:
                        mouseDown = true;
                        lastMouse = Environment.TickCount;
                        Game.AddClick();
                        break;
                    case WM_LBUTTONUP:
                    case WM_RBUTTONUP:
                    case WM_MBUTTONUP:
                        mouseDown = false;
                        lastMouse = Environment.TickCount;
                        break;
                    case WM_MOUSEMOVE:
                    case WM_MOUSEWHEEL:
                        lastMouse = Environment.TickCount;
                        break;
                }
            }
            return Native.CallNextHookEx(msHook, code, w, l);
        }

        void OnBoxEarned()
        {
            fxText = "+1 ЛУТБОКС!";
            boxFxUntil = Environment.TickCount + 3000;
            Balloon("loot", "Новый лутбокс!", "Ещё 2000 нажатий позади. Открой лутбокс в меню персонажа.");
        }

        void OnUnlocked(Achievement a)
        {
            fxText = "ДОСТИЖЕНИЕ!";
            boxFxUntil = Environment.TickCount + 3500;
            Balloon("loot", "Достижение: " + a.Name, a.Desc + "\nНаграда: " + a.RewardText());
        }

        // kind decides what a click on the balloon opens
        void Balloon(string kind, string title, string text)
        {
            balloonKind = kind;
            try { tray.ShowBalloonTip(5000, title, text, ToolTipIcon.Info); } catch { }
        }

        void CheckForUpdates()
        {
            lastUpdateCheck = Environment.TickCount;
            Updater.CheckAsync(this, ok =>
            {
                if (!ok || !Updater.Available || notifiedVersion == Updater.LatestVersion) return;
                notifiedVersion = Updater.LatestVersion;
                Balloon("update", "Доступно обновление " + Updater.LatestVersion, "Нажми, чтобы обновить BongoBro. Прогресс сохранится.");
            });
        }

        public void OpenUpdate()
        {
            if (upd == null || upd.IsDisposed) upd = new UpdateForm();
            ShowForm(upd);
        }

        public void OpenLeaderboard()
        {
            if (lb == null || lb.IsDisposed) lb = new LeaderboardForm();
            ShowForm(lb);
        }

        public void OpenAchievements()
        {
            if (ach == null || ach.IsDisposed) ach = new AchievementsForm();
            ShowForm(ach);
        }

        // ---------------- loop ----------------
        void OnTick(object sender, EventArgs e)
        {
            int now = Environment.TickCount;
            if (held.Count > 0 && now - lastKey > 400)
            {
                List<int> stale = null;
                foreach (int vk in held.Keys)
                    if ((Native.GetAsyncKeyState(vk) & 0x8000) == 0)
                    {
                        if (stale == null) stale = new List<int>();
                        stale.Add(vk);
                    }
                if (stale != null) foreach (int vk in stale) held.Remove(vk);
            }
            bool lh = false, rh = false;
            foreach (bool r in held.Values) { if (r) rh = true; else lh = true; }
            pose.LeftDown = lh || now - leftPress < 90;
            pose.RightDown = rh || now - rightPress < 90;
            // his right hand (viewer's left) is the mouse hand
            pose.OnMouse = !pose.LeftDown && now - leftPress > 250 && now - lastMouse < 1500;
            pose.MouseDown = mouseDown;
            if (held.Count == 0 && now - lastKey > 140) pose.LitKey = -1;
            if (now - nextBlink >= 0) { blinkUntil = now + 150; nextBlink = now + 2200 + rnd.Next(4000); }
            pose.Blink = now - blinkUntil < 0;
            Point c = Cursor.Position;
            Rectangle sb = Screen.FromPoint(c).Bounds;
            pose.MouseDX = (float)Math.Round(((c.X - sb.Left) / (float)sb.Width - 0.5f) * 10f);
            pose.MouseDY = (float)Math.Round(((c.Y - sb.Top) / (float)sb.Height - 0.5f) * 8f);
            pose.Hat = Game.S.Hat;
            pose.Glasses = Game.S.Glasses;
            pose.Gloves = Game.S.Gloves;
            pose.Face = Game.S.Face;
            pose.Theme = Game.S.Theme;
            pose.Character = Game.S.Character;
            pose.T = Art.Now;

            bool anim = Art.IsAnimated(pose);
            string sig = string.Concat(new object[] {
                pose.LeftDown, pose.RightDown, pose.OnMouse, pose.MouseDown, pose.Blink, pose.LitKey,
                pose.MouseDX, pose.MouseDY, pose.Hat, pose.Glasses, pose.Gloves, pose.Face, pose.Theme, pose.Character, scale });
            if (shown && (anim || sig != lastSig))
            {
                lastSig = sig;
                Render();
            }
            Item th = Items.Get(Game.S.Theme);
            if (counter != null) counter.Tick(boxFxUntil, fxText, pose.T, th != null && th.Rarity == Rarity.Immortal);
            if (now - lastDayCheck > 60000) { lastDayCheck = now; Game.TouchDay(); }
            if (now - lastUpdateCheck > 6 * 3600 * 1000) CheckForUpdates();
            if (Game.S.Nick != null && now - lastLbSubmit > Leaderboard.SubmitEveryMs && Game.S.Keys != Leaderboard.LastSubmittedKeys)
            {
                lastLbSubmit = now;
                Leaderboard.SubmitAsync(this, Game.S.Nick, err => { });
            }
            if (Game.Dirty && now - lastSave > 15000)
            {
                Game.Save();
                lastSave = now;
            }
        }

        void Render()
        {
            int px = CanvasPx();
            if (canvas == null || canvas.Width != px)
            {
                if (canvas != null) canvas.Dispose();
                canvas = new Bitmap(px, px, PixelFormat.Format32bppArgb);
                Size = new Size(px, px);
            }
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.ScaleTransform(scale, scale);
                Art.DrawCharacter(g, pose);
            }
            Native.SetLayeredBitmap(this, canvas);
        }

        // ---------------- counter placement ----------------
        // The counter is anchored to the top of his head, so it stays next to him at any size.
        Point HeadAnchor() { return new Point(Left + Width / 2, Top + (int)(Height * 0.25f)); }

        void PositionCounter()
        {
            if (counter == null) return;
            Point a = HeadAnchor();
            int dx = Game.S.CounterDX, dy = Game.S.CounterDY;
            if (dx == int.MinValue || dy == int.MinValue)
            {
                Size cs = counter.PxSize();
                dx = -(int)(Width * 0.16f) - cs.Width;
                dy = (int)(Height * 0.12f) - cs.Height;
            }
            counter.PlaceAt(new Point(a.X + dx, a.Y + dy));
        }

        // Called when the user drags the counter (or it is resized): remember its offset.
        public void CounterDragged(Point loc)
        {
            Point a = HeadAnchor();
            Game.S.CounterDX = loc.X - a.X;
            Game.S.CounterDY = loc.Y - a.Y;
            Game.Dirty = true;
        }

        // ---------------- window behaviour ----------------
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left) Native.DragWindow(this);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Right) ShowMenu();
        }

        public void ShowMenu() { menu.Show(Cursor.Position); }

        static bool OnScreen(Point loc, int sz)
        {
            Rectangle r = new Rectangle(loc.X + sz / 4, loc.Y + sz / 4, sz / 2, sz / 2);
            foreach (Screen s in Screen.AllScreens) if (s.WorkingArea.IntersectsWith(r)) return true;
            return false;
        }

        static Point DefaultLocation(int sz)
        {
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            return new Point(wa.Right - sz - 16, wa.Bottom - sz);
        }

        // Resizes around the bottom-right corner (the desk stays where it was).
        void SetScale(float s)
        {
            int oldPx = CanvasPx();
            int right = Left + oldPx, bottom = Top + oldPx;
            Game.S.Scale = s;
            scale = s * Ui.Dpi;
            int px = CanvasPx();
            Location = new Point(right - px, bottom - px);
            Game.Dirty = true;
            Render();
            PositionCounter();
        }

        void ResetPosition()
        {
            Game.S.CounterDX = Game.S.CounterDY = int.MinValue;
            Location = DefaultLocation(CanvasPx());
            PositionCounter();
            Render();
        }

        void SetShown(bool v)
        {
            shown = v;
            if (v) { Show(); Render(); } else Hide();
        }

        public static void SetCharacter(string id)
        {
            Game.S.Character = Characters.Normalize(id);
            Game.Save();
        }

        public void OpenLoot()
        {
            if (loot == null || loot.IsDisposed) loot = new LootForm();
            ShowForm(loot);
        }

        public void OpenShop()
        {
            if (shop == null || shop.IsDisposed) shop = new ShopForm();
            ShowForm(shop);
        }

        static void ShowForm(Form f)
        {
            f.Show();
            if (f.WindowState == FormWindowState.Minimized) f.WindowState = FormWindowState.Normal;
            f.Activate();
        }

        // ---------------- menu & tray ----------------
        void BuildMenu()
        {
            int ip = (int)(20 * Ui.Dpi);
            menu = new ContextMenuStrip();
            menu.Renderer = new MenuRenderer();
            menu.Font = new Font("Segoe UI", 10f);
            menu.ImageScalingSize = new Size(ip, ip);
            menu.BackColor = Ui.Panel;
            menu.ForeColor = Ui.Fg;
            menu.Padding = new Padding(2, 4, 2, 4);

            HeaderItem header = new HeaderItem();
            header.Click += delegate { OpenShop(); };
            miBox = new ToolStripMenuItem("Открыть лутбокс", Ui.MenuIcon("gift", ip), delegate { OpenLoot(); });
            ToolStripMenuItem miShop = new ToolStripMenuItem("Гардероб и коллекция", Ui.MenuIcon("shirt", ip), delegate { OpenShop(); });
            miAch = new ToolStripMenuItem("Достижения", Ui.MenuIcon("trophy", ip), delegate { OpenAchievements(); });
            ToolStripMenuItem miBoard = new ToolStripMenuItem("Лидерборд", Ui.MenuIcon("podium", ip), delegate { OpenLeaderboard(); });
            miChar = new ToolStripMenuItem("Персонаж", Ui.MenuIcon("user", ip));
            foreach (string id in Characters.Ids)
            {
                string who = id;
                ToolStripMenuItem it = new ToolStripMenuItem(Characters.Name(who));
                it.Tag = who;
                it.Click += delegate { SetCharacter(who); };
                miChar.DropDownItems.Add(it);
            }
            miChar.DropDown.Renderer = menu.Renderer;
            miChar.DropDown.BackColor = Ui.Panel;
            miChar.DropDown.Font = menu.Font;
            ((ToolStripDropDownMenu)miChar.DropDown).ShowImageMargin = false;
            ((ToolStripDropDownMenu)miChar.DropDown).ShowCheckMargin = true;
            miChar.DropDownOpening += delegate
            {
                foreach (ToolStripItem it in miChar.DropDownItems)
                {
                    ToolStripMenuItem mi = it as ToolStripMenuItem;
                    if (mi != null) mi.Checked = (string)mi.Tag == Game.S.Character;
                }
            };

            charSlider = new MenuSlider("Размер персонажа", MinScale, MaxScale, Game.S.Scale);
            charSlider.ValueChanged += SetScale;
            charSlider.Committed += Game.Save;
            counterSlider = new MenuSlider("Размер счётчика", MinCounterScale, MaxCounterScale, Game.S.CounterScale);
            counterSlider.ValueChanged += v => counter.SetScale(v);
            counterSlider.Committed += Game.Save;

            ToggleItem tAuto = new ToggleItem("Автозапуск с Windows", Ui.MenuIcon("power", ip), AutoStart.IsOn, v =>
            {
                try { AutoStart.Set(v); }
                catch (Exception ex) { MessageBox.Show("Не удалось изменить автозапуск: " + ex.Message, "BongoBro"); }
            });
            ToggleItem tTop = new ToggleItem("Поверх всех окон", Ui.MenuIcon("pin", ip), () => Game.S.TopMost, v =>
            {
                Game.S.TopMost = v;
                TopMost = v;
                counter.TopMost = v;
                Game.Save();
            });
            ToggleItem tCounter = new ToggleItem("Показывать счётчик", Ui.MenuIcon("counter", ip), () => Game.S.ShowCounter, v => { Game.S.ShowCounter = v; Game.Save(); });
            ToggleItem tShown = new ToggleItem("Показывать персонажа", Ui.MenuIcon("user", ip), () => shown, SetShown);
            miUpdate = new ToolStripMenuItem("Проверить обновления", Ui.MenuIcon("update", ip), delegate { OpenUpdate(); });
            ToolStripMenuItem miReset = new ToolStripMenuItem("Вернуть в угол экрана", Ui.MenuIcon("reset", ip), delegate { SetShown(true); ResetPosition(); });
            ToolStripMenuItem miExit = new ToolStripMenuItem("Выход", Ui.MenuIcon("exit", ip), delegate { Close(); });

            menu.Items.AddRange(new ToolStripItem[] {
                header, new ToolStripSeparator(), miChar, miBox, miShop, miAch, miBoard, new ToolStripSeparator(),
                charSlider.Host, counterSlider.Host, new ToolStripSeparator(),
                tAuto, tTop, tCounter, tShown, miReset, new ToolStripSeparator(), miUpdate, miExit });

            menu.Opening += delegate
            {
                int n = Game.S.Boxes, gold = Game.S.GoldBoxes;
                miBox.Text = n + gold == 0 ? "Лутбоксы"
                    : "Открыть лутбокс  (" + n + (gold > 0 ? " + " + gold + " зол." : "") + ")";
                miAch.Text = "Достижения  (" + Game.S.Achieved.Count + "/" + Achievement.All.Count + ")";
                miChar.Text = "Персонаж: " + Characters.Name(Game.S.Character);
                miUpdate.Text = Updater.Available ? "Обновить до версии " + Updater.LatestVersion : "Проверить обновления  (v" + Program.Version + ")";
                charSlider.SetValue(Game.S.Scale);
                counterSlider.SetValue(Game.S.CounterScale);
            };
            menu.Opened += delegate
            {
                using (GraphicsPath p = Art.RR(new RectangleF(0, 0, menu.Width, menu.Height), 10 * Ui.Dpi)) menu.Region = new Region(p);
            };
        }

        void BuildTray()
        {
            tray = new NotifyIcon();
            tray.Icon = Program.TrayIcon ?? SystemIcons.Application;
            tray.Text = "BongoBro";
            tray.ContextMenuStrip = menu;
            tray.Visible = true;
            tray.DoubleClick += delegate { SetShown(!shown); };
            tray.BalloonTipClicked += delegate { if (balloonKind == "update") OpenUpdate(); else OpenLoot(); };
        }
    }
}

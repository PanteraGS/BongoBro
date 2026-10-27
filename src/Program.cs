using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace BongoBro
{
    static partial class Program
    {
        public static Icon AppIcon, TrayIcon;
        public static Bitmap Logo;
        public static MainForm MainWin;
        public static bool Updated;

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 2 && args[0] == "--export-icon") { IconMaker.ExportIco(args[1]); return; }
            if (args.Length == 2 && args[0] == "--preview") { IconMaker.ExportPreview(args[1]); return; }
            if (args.Length == 2 && args[0] == "--preview2") { IconMaker.ExportFeatures(args[1]); return; }
            if (args.Length == 3 && args[0] == "--shot") { Screenshot(args[1], args[2]); return; }
            if (args.Length == 1 && args[0] == "--update") { UpdateHeadless(); return; }

            // After a self-update the old process may still be closing: wait for it instead of refusing to start.
            Updated = args.Length == 1 && args[0] == "--updated";
            using (Mutex m = new Mutex(false, "BongoBro.SingleInstance.v1"))
            {
                bool owned;
                try
                {
                    owned = m.WaitOne(Updated ? 5000 : 0);
                    if (!owned && Updated)
                    {
                        // Versions up to 1.1.0 could hang on an error dialog while exiting after an update.
                        // Its progress is already saved, so end that old process and carry on.
                        Updater.KillOldProcess();
                        owned = m.WaitOne(10000);
                    }
                }
                catch (AbandonedMutexException) { owned = true; }
                if (!owned)
                {
                    MessageBox.Show("BongoBro уже запущен: ищи его на экране или в трее.", "BongoBro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                Updater.CleanupOld();
                try { Native.SetProcessDPIAware(); } catch { }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero)) Ui.Dpi = g.DpiX / 96f;
                Game.Load();
                if (AutoStart.IsOn()) { try { AutoStart.Set(true); } catch { } }
                Logo = IconMaker.Render(128);
                AppIcon = IconMaker.MakeIcon(64);
                TrayIcon = IconMaker.MakeIcon(32);
                MainWin = new MainForm();
                Application.Run(MainWin);
                m.ReleaseMutex();
            }
        }
    }

    static partial class Program
    {
        // "BongoBro.exe --update": update without UI (the app restarts itself if a newer version exists).
        static void UpdateHeadless()
        {
            Form host = new Form();
            host.ShowInTaskbar = false;
            host.Opacity = 0;
            host.Load += delegate
            {
                Updater.CheckAsync(host, ok =>
                {
                    if (ok && Updater.Available) Updater.DownloadAndRestart(p => { }, msg => Application.Exit());
                    else Application.Exit();
                });
            };
            Application.Run(host);
        }

        static ShopForm SetsShop(bool personal = false)
        {
            ShopForm f = new ShopForm();
            if (personal) f.ShowPersonalSets(); else f.ShowSetsTab();
            return f;
        }

        // Debug helper: renders one window off-screen to a PNG, using the real save read-only.
        static void Screenshot(string which, string path)
        {
            Application.EnableVisualStyles();
            Game.S = SaveData.Load();
            if (which == "loot-gold") { Game.S.GoldBoxes = 2; which = "loot"; }
            if (Characters.Preview != null) Game.S.Character = Characters.Preview;
            Form f = which == "loot" ? (Form)new LootForm() : which == "shop" ? (Form)new ShopForm()
                : which == "update" ? (Form)new UpdateForm() : which == "board" ? (Form)new LeaderboardForm()
                : which == "dev" ? (Form)new DevForm(null) : which == "sets" ? (Form)SetsShop() : which == "psets" ? (Form)SetsShop(true) : new AchievementsForm();
            f.StartPosition = FormStartPosition.Manual;
            f.Location = new Point(-5000, -5000);
            f.ShowInTaskbar = false;
            f.Show();
            // the update window asks GitHub asynchronously: give it a few seconds
            for (int i = 0; i < (which == "update" || which == "board" ? 120 : 1); i++) { Application.DoEvents(); Thread.Sleep(50); }
            using (Bitmap b = new Bitmap(f.Width, f.Height))
            {
                f.DrawToBitmap(b, new Rectangle(0, 0, f.Width, f.Height));
                b.Save(path, ImageFormat.Png);
            }
            f.Close();
        }
    }

    static class IconMaker
    {
        public static Bitmap Render(int px)
        {
            Bitmap b = new Bitmap(px, px, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.ScaleTransform(px / 100f, px / 100f);
                using (GraphicsPath c = new GraphicsPath())
                {
                    c.AddEllipse(1, 1, 98, 98);
                    using (LinearGradientBrush br = new LinearGradientBrush(new RectangleF(0, 0, 100, 100), Color.FromArgb(160, 130, 255), Color.FromArgb(76, 44, 190), 90f)) g.FillPath(br, c);
                    GraphicsState st = g.Save();
                    g.SetClip(c, CombineMode.Intersect);
                    Pose p = new Pose();
                    Art.DrawBust(g, new RectangleF(96, 46, 168, 168), new RectangleF(0, 6, 100, 100), p);
                    g.Restore(st);
                }
            }
            return b;
        }

        public static Icon MakeIcon(int px)
        {
            using (Bitmap b = Render(px)) return Icon.FromHandle(b.GetHicon());
        }

        public static void ExportIco(string path)
        {
            int[] sizes = { 256, 64, 48, 32, 16 };
            List<byte[]> pngs = new List<byte[]>();
            foreach (int s in sizes)
                using (Bitmap b = Render(s))
                using (MemoryStream ms = new MemoryStream())
                {
                    b.Save(ms, ImageFormat.Png);
                    pngs.Add(ms.ToArray());
                }
            using (BinaryWriter w = new BinaryWriter(File.Create(path)))
            {
                w.Write((short)0); w.Write((short)1); w.Write((short)sizes.Length);
                int offset = 6 + 16 * sizes.Length;
                for (int i = 0; i < sizes.Length; i++)
                {
                    byte d = (byte)(sizes[i] >= 256 ? 0 : sizes[i]);
                    w.Write(d); w.Write(d); w.Write((byte)0); w.Write((byte)0);
                    w.Write((short)1); w.Write((short)32);
                    w.Write(pngs[i].Length); w.Write(offset);
                    offset += pngs[i].Length;
                }
                foreach (byte[] png in pngs) w.Write(png);
            }
        }

        // Debug helper: moods, gear sets, day phases and pets on one sheet.
        public static void ExportFeatures(string path)
        {
            Game.S = new SaveData();
            using (Bitmap b = new Bitmap(1440, 1220, PixelFormat.Format32bppArgb))
            using (Graphics g = Graphics.FromImage(b))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                g.Clear(Ui.Bg);
                Mood[] moods = { Mood.Normal, Mood.Yawn, Mood.Sleep, Mood.Surprised, Mood.Focused, Mood.Happy };
                string[] mn = { "Обычный", "Зевает", "Спит", "Удивлён", "Сосредоточен", "Радуется" };
                for (int i = 0; i < moods.Length; i++)
                {
                    Pose p = new Pose();
                    p.T = 1.1f + i; p.Mood = moods[i]; p.Pet = "pet_cat"; p.RightDown = moods[i] == Mood.Focused;
                    Character(g, new Rectangle(10 + i * 238, 10, 228, 228), p, mn[i]);
                }
                string[] sets = { "set_samurai", "set_pirate", "set_astro" };
                for (int i = 0; i < 3; i++)
                {
                    Pose p = new Pose();
                    p.T = 2.3f + i; p.PetHop = 0.6f; p.LeftDown = true; p.LitKey = 12;
                    if (i == 1) p.Character = Characters.Vlad;
                    foreach (Item it in GearSet.All[i].Pieces())
                        switch (it.Slot)
                        {
                            case Slot.Hat: p.Hat = it.Id; break;
                            case Slot.Glasses: p.Glasses = it.Id; break;
                            case Slot.Gloves: p.Gloves = it.Id; break;
                            case Slot.Face: p.Face = it.Id; break;
                            case Slot.Theme: p.Theme = it.Id; break;
                            case Slot.Pet: p.Pet = it.Id; break;
                        }
                    Character(g, new Rectangle(10 + i * 238, 250, 228, 228), p, "Сет: " + GearSet.All[i].Name);
                }
                DayPhase[] days = { DayPhase.Morning, DayPhase.Evening, DayPhase.Night };
                string[] dn = { "Утро", "Вечер", "Ночь" };
                for (int i = 0; i < 3; i++)
                {
                    Pose p = new Pose();
                    p.T = 3.3f + i; p.Day = days[i]; p.Pet = i == 2 ? "pet_owl" : "pet_corgi"; p.Mood = i == 2 ? Mood.Sleep : Mood.Normal;
                    Character(g, new Rectangle(724 + i * 238, 250, 228, 228), p, dn[i]);
                }
                int n = 0;
                // personal sets on their owners, calm and in the middle of a key press
                for (int i = 0; i < 6; i++)
                {
                    GearSet set = GearSet.All[3 + i % 3];
                    Pose p = new Pose();
                    p.T = 4.1f + i; p.Character = set.Owner;
                    if (i >= 3) { p.PetHop = 0.55f; p.LeftDown = true; p.LitKey = 20; }
                    foreach (Item it in set.Pieces())
                        switch (it.Slot)
                        {
                            case Slot.Hat: p.Hat = it.Id; break;
                            case Slot.Glasses: p.Glasses = it.Id; break;
                            case Slot.Gloves: p.Gloves = it.Id; break;
                            case Slot.Face: p.Face = it.Id; break;
                            case Slot.Theme: p.Theme = it.Id; break;
                            case Slot.Pet: p.Pet = it.Id; break;
                        }
                    Character(g, new Rectangle(10 + i * 238, 490, 228, 228), p, set.Name + (i >= 3 ? " (нажатие)" : ""));
                }
                foreach (Item it in Items.OfSlot(Slot.Pet))
                {
                    Ui.ItemCard(g, new RectangleF(10 + (n % 12) * 118, 740 + (n / 12) * 156, 110, 148), it, 1.4f + n * 0.3f, false, 0, false);
                    n++;
                }
                b.Save(path, ImageFormat.Png);
            }
        }

        static void Character(Graphics g, Rectangle r, Pose p, string label)
        {
            using (Bitmap c = new Bitmap(r.Width, r.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics cg = Graphics.FromImage(c))
                {
                    cg.SmoothingMode = SmoothingMode.AntiAlias;
                    Ui.FillRR(cg, new RectangleF(0, 0, r.Width, r.Height), 12, Ui.Panel);
                    Art.DrawCharacterIn(cg, new RectangleF(0, 0, r.Width, r.Height), p);
                }
                using (ImageAttributes tint = MainForm.DayTint(p.Day))
                    g.DrawImage(c, r, 0, 0, c.Width, c.Height, GraphicsUnit.Pixel, tint);
            }
            Ui.TextL(g, label, Ui.F(14, true), Color.White, r.X + 8, r.Y + 6);
        }

        // Debug helper: renders a sheet with the character and every item.
        public static void ExportPreview(string path)
        {
            Game.S = new SaveData();
            if (Characters.Preview != null) Game.S.Character = Characters.Preview;
            using (Bitmap b = new Bitmap(1440, 2150, PixelFormat.Format32bppArgb))
            using (Graphics g = Graphics.FromImage(b))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                g.Clear(Ui.Bg);
                string[][] looks = {
                    new string[] { null, null, null, null, null },
                    new string[] { "hat_halo", "gl_galaxy", "gv_plasma", "fc_neon", "th_rgb" },
                    new string[] { "hat_firecrown", null, "gv_infinity", "fc_fireeyes", "th_cosmos" },
                    new string[] { "hat_catears", null, "gv_paw", "fc_cat", "th_sakura" },
                    new string[] { "hat_viking", "gl_steampunk", "gv_knight", "fc_mustache", "th_marble" },
                    new string[] { "hat_cowboy", null, "gv_boxing", "fc_grin", "th_cyber" } };
                for (int i = 0; i < looks.Length; i++)
                {
                    Pose p = new Pose();
                    if (Characters.Preview != null) p.Character = Characters.Preview;
                    p.T = 0.7f + i;
                    p.Hat = looks[i][0]; p.Glasses = looks[i][1]; p.Gloves = looks[i][2]; p.Face = looks[i][3]; p.Theme = looks[i][4];
                    if (i % 2 == 0) { p.LeftDown = true; p.LitKey = 14; } else { p.OnMouse = true; p.MouseDown = true; }
                    RectangleF dst = new RectangleF(10 + (i % 2) * 210, 10 + (i / 2) * 210, 210, 210);
                    Art.DrawCounter(g, new RectangleF(dst.X + 4, dst.Y + 4, 90, 24), Art.GetTheme(p.Theme), "12 345", 0.6f, p.T);
                    Art.DrawCharacterIn(g, dst, p);
                }
                for (int i = 0; i < Items.All.Count; i++)
                {
                    int col = i % 7, row = i / 7;
                    Ui.ItemCard(g, new RectangleF(440 + col * 140, 10 + row * 126, 92, 118), Items.All[i], 1.7f + i * 0.3f, false, 0, false);
                }
                b.Save(path, ImageFormat.Png);
            }
        }
    }
}

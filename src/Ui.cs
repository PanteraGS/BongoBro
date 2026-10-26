using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.Windows.Forms;

namespace BongoBro
{
    public static class Ui
    {
        public static float Dpi = 1f;
        public static readonly Color Bg = Color.FromArgb(20, 21, 31);
        public static readonly Color Panel = Color.FromArgb(30, 32, 47);
        public static readonly Color Panel2 = Color.FromArgb(42, 45, 68);
        public static readonly Color Border = Color.FromArgb(60, 64, 98);
        public static readonly Color Fg = Color.FromArgb(241, 242, 248);
        public static readonly Color Sub = Color.FromArgb(150, 156, 184);
        public static readonly Color Accent = Color.FromArgb(124, 92, 255);
        public static readonly Color Gold = Color.FromArgb(255, 176, 32);
        public static readonly Color Good = Color.FromArgb(34, 197, 94);
        public static readonly Color Bad = Color.FromArgb(239, 68, 68);

        static readonly Dictionary<string, Font> fonts = new Dictionary<string, Font>();
        static readonly NumberFormatInfo nfi;

        static Ui()
        {
            nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
        }

        public static Font F(float px, bool bold)
        {
            string k = px.ToString(CultureInfo.InvariantCulture) + (bold ? "b" : "r");
            Font f;
            if (!fonts.TryGetValue(k, out f))
            {
                f = new Font("Segoe UI", px, bold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel);
                fonts[k] = f;
            }
            return f;
        }

        public static string Num(long n) { return n.ToString("#,0", nfi); }

        public static void FillRR(Graphics g, RectangleF r, float rad, Color c)
        {
            using (GraphicsPath p = Art.RR(r, rad)) using (SolidBrush b = new SolidBrush(c)) g.FillPath(b, p);
        }

        public static void TextC(Graphics g, string s, Font f, Color c, RectangleF r)
        {
            using (StringFormat sf = new StringFormat())
            using (SolidBrush b = new SolidBrush(c))
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                sf.Trimming = StringTrimming.EllipsisCharacter;
                g.DrawString(s, f, b, r, sf);
            }
        }

        public static void TextL(Graphics g, string s, Font f, Color c, float x, float y)
        {
            using (SolidBrush b = new SolidBrush(c)) g.DrawString(s, f, b, x, y);
        }

        public static void TextR(Graphics g, string s, Font f, Color c, RectangleF r)
        {
            using (StringFormat sf = new StringFormat())
            using (SolidBrush b = new SolidBrush(c))
            {
                sf.Alignment = StringAlignment.Far;
                sf.LineAlignment = StringAlignment.Center;
                sf.Trimming = StringTrimming.EllipsisCharacter;
                sf.FormatFlags = StringFormatFlags.NoWrap;
                g.DrawString(s, f, b, r, sf);
            }
        }

        public static void Progress(Graphics g, RectangleF r, float k, Color c)
        {
            FillRR(g, r, r.Height / 2, Color.FromArgb(56, 60, 90));
            if (k <= 0) return;
            RectangleF f = new RectangleF(r.X, r.Y, Math.Max(r.Height, r.Width * Math.Min(1f, k)), r.Height);
            using (GraphicsPath p = Art.RR(f, r.Height / 2))
            using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(f.X - 1, f.Y, f.Width + 2, f.Height), c, Art.Mix(c, Color.White, 0.35f), 0f))
                g.FillPath(b, p);
        }

        public static void ItemCard(Graphics g, RectangleF r, Item it, float t, bool dim, float footer, bool hover)
        {
            Color rc = Items.RarityColor(it.Rarity);
            float sk = r.Width / 120f;
            using (GraphicsPath p = Art.RR(r, 12 * sk))
            {
                if (it.Rarity == Rarity.Immortal)
                    Art.GlowPath(g, p, Art.Hsv(38 + 18 * Art.Sin(t * 1.3f), 0.85f, 1f), 0.7f + 0.3f * Art.Sin(t * 3));
                else if (it.Rarity == Rarity.Legendary)
                    Art.GlowPath(g, p, rc, 0.45f);
                using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(r.X, r.Y - 1, r.Width, r.Height + 2), Panel2, Art.Mix(Panel, rc, 0.32f), 90f)) g.FillPath(b, p);
                GraphicsState st = g.Save();
                g.SetClip(p, CombineMode.Intersect);
                using (SolidBrush top = new SolidBrush(rc)) g.FillRectangle(top, r.X, r.Y, r.Width, 4 * sk);
                g.Restore(st);
                using (Pen pen = new Pen(Art.A(hover ? 255 : 140, rc), hover ? 2.2f : 1.3f)) g.DrawPath(pen, p);
            }
            RectangleF pv = new RectangleF(r.X + 6 * sk, r.Y + 6 * sk, r.Width - 12 * sk, (r.Height - footer) * 0.58f);
            Art.Glow(g, pv.X + pv.Width / 2, pv.Y + pv.Height / 2, pv.Width * 0.55f, pv.Height * 0.6f, rc, 80);
            GraphicsState cs = g.Save();
            g.SetClip(pv, CombineMode.Intersect);
            Art.DrawItemPreview(g, it, pv, t);
            g.Restore(cs);
            if (dim)
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(120, 20, 21, 31))) g.FillRectangle(b, pv);
                Lock(g, pv.Right - 12 * sk, pv.Y + 12 * sk, 7 * sk);
            }
            float ty = pv.Bottom + 4 * sk;
            TextC(g, it.Name, F(12.5f * sk, true), Fg, new RectangleF(r.X + 4, ty, r.Width - 8, 34 * sk));
            TextC(g, Items.RarityName(it.Rarity), F(10.5f * sk, true), rc, new RectangleF(r.X, ty + 33 * sk, r.Width, 16 * sk));
            if (it.Rarity == Rarity.Immortal)
            {
                for (int k = 0; k < 3; k++)
                {
                    float ph = (t * 0.35f + k / 3f) % 1f;
                    float per = 2 * (r.Width + r.Height), d = ph * per;
                    float x, y;
                    if (d < r.Width) { x = r.X + d; y = r.Y; }
                    else if (d < r.Width + r.Height) { x = r.Right; y = r.Y + d - r.Width; }
                    else if (d < 2 * r.Width + r.Height) { x = r.Right - (d - r.Width - r.Height); y = r.Bottom; }
                    else { x = r.X; y = r.Bottom - (d - 2 * r.Width - r.Height); }
                    Art.Sparkle(g, x, y, (3 + 2 * Art.Sin(t * 6 + k)) * sk, Color.FromArgb(255, 210, 120));
                }
            }
        }

        static void Lock(Graphics g, float cx, float cy, float s)
        {
            using (Pen p = new Pen(Sub, s * 0.3f)) g.DrawArc(p, cx - s * 0.5f, cy - s * 1.1f, s, s * 1.2f, 180, 180);
            FillRR(g, new RectangleF(cx - s * 0.75f, cy - s * 0.45f, s * 1.5f, s * 1.2f), s * 0.25f, Sub);
        }

        public static Bitmap MenuIcon(string kind, int px)
        {
            Bitmap b = new Bitmap(px, px, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.ScaleTransform(px / 20f, px / 20f);
                Color c = Fg;
                using (Pen p = Art.P(c, 1.7f))
                {
                    switch (kind)
                    {
                        case "gift": Art.DrawLootBox(g, 10, 11.5f, 17, 0, false); break;
                        case "shirt":
                            using (GraphicsPath s = new GraphicsPath())
                            {
                                s.AddPolygon(new PointF[] { new PointF(6.5f, 3), new PointF(13.5f, 3), new PointF(19, 7), new PointF(16.5f, 10.5f), new PointF(15, 9.5f), new PointF(15, 18), new PointF(5, 18), new PointF(5, 9.5f), new PointF(3.5f, 10.5f), new PointF(1, 7) });
                                using (LinearGradientBrush br = new LinearGradientBrush(new RectangleF(0, 2, 20, 17), Color.FromArgb(190, 170, 255), Accent, 90f)) g.FillPath(br, s);
                            }
                            break;
                        case "trophy":
                            using (GraphicsPath cup = new GraphicsPath())
                            {
                                cup.AddBezier(4.5f, 2.5f, 4.5f, 10, 7, 12, 10, 12);
                                cup.AddBezier(10, 12, 13, 12, 15.5f, 10, 15.5f, 2.5f);
                                cup.CloseFigure();
                                using (LinearGradientBrush br = new LinearGradientBrush(new RectangleF(4, 2, 12, 11), Color.FromArgb(255, 236, 140), Gold, 90f)) g.FillPath(br, cup);
                            }
                            using (Pen hp = Art.P(Gold, 1.5f)) { g.DrawArc(hp, 1.5f, 3.5f, 5, 5, 90, 180); g.DrawArc(hp, 13.5f, 3.5f, 5, 5, 270, 180); }
                            using (SolidBrush br = new SolidBrush(Gold)) { g.FillRectangle(br, 9, 12, 2, 3); g.FillRectangle(br, 6, 15, 8, 2.5f); }
                            break;
                        case "podium":
                            using (SolidBrush b1 = new SolidBrush(Gold)) g.FillRectangle(b1, 7.5f, 4, 5, 13.5f);
                            using (SolidBrush b2 = new SolidBrush(Color.FromArgb(200, 208, 222))) g.FillRectangle(b2, 2, 8, 5, 9.5f);
                            using (SolidBrush b3 = new SolidBrush(Color.FromArgb(214, 136, 74))) g.FillRectangle(b3, 13, 10.5f, 5, 7);
                            break;
                        case "update":
                            using (SolidBrush br = new SolidBrush(Good)) g.FillEllipse(br, 2, 2, 16, 16);
                            using (Pen w = Art.P(Color.White, 2f))
                            {
                                g.DrawLine(w, 10, 14.5f, 10, 6);
                                g.DrawLines(w, new PointF[] { new PointF(6.5f, 9), new PointF(10, 5.5f), new PointF(13.5f, 9) });
                            }
                            break;
                        case "power":
                            g.DrawArc(p, 4, 4.5f, 12, 12, -55, 290);
                            g.DrawLine(p, 10, 2.5f, 10, 9);
                            break;
                        case "pin":
                            g.DrawRectangle(p, 2.5f, 2.5f, 10, 10);
                            using (SolidBrush br = new SolidBrush(Accent)) g.FillRectangle(br, 7.5f, 7.5f, 10, 10);
                            break;
                        case "counter":
                            using (GraphicsPath k = Art.RR(new RectangleF(1.5f, 5, 17, 11), 2.5f)) g.DrawPath(p, k);
                            using (SolidBrush br = new SolidBrush(c))
                            {
                                for (int i = 0; i < 4; i++) g.FillRectangle(br, 4 + i * 3.4f, 7.5f, 2, 2);
                                g.FillRectangle(br, 6, 11.5f, 8, 2);
                            }
                            break;
                        case "user":
                            g.DrawEllipse(p, 6.5f, 2.5f, 7, 7);
                            g.DrawArc(p, 3, 11, 14, 12, 180, 180);
                            break;
                        case "size":
                            g.DrawLine(p, 4, 16, 16, 4);
                            g.DrawLines(p, new PointF[] { new PointF(10, 4), new PointF(16, 4), new PointF(16, 10) });
                            g.DrawLines(p, new PointF[] { new PointF(4, 10), new PointF(4, 16), new PointF(10, 16) });
                            break;
                        case "reset":
                            g.DrawArc(p, 3.5f, 3.5f, 13, 13, -30, 290);
                            g.DrawLines(p, new PointF[] { new PointF(15.5f, 1.5f), new PointF(15.5f, 6.5f), new PointF(10.5f, 6.5f) });
                            break;
                        case "exit":
                            using (Pen r = Art.P(Bad, 2f)) { g.DrawLine(r, 5, 5, 15, 15); g.DrawLine(r, 15, 5, 5, 15); }
                            break;
                    }
                }
            }
            return b;
        }
    }

    public class Hit
    {
        public RectangleF R;
        public Action A;
        public Hit(RectangleF r, Action a) { R = r; A = a; }
    }

    // Borderless, custom-painted window. Layout is done in logical (96 dpi) units.
    public class CustomForm : Form
    {
        protected float S;
        protected int W, H;
        protected readonly List<Hit> hits = new List<Hit>();
        protected PointF mouse = new PointF(-1000, -1000);
        readonly Timer anim;

        public CustomForm(int w, int h, string title)
        {
            W = w; H = h; S = Ui.Dpi;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.None;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            ClientSize = new Size((int)(w * S), (int)(h * S));
            BackColor = Ui.Bg;
            Text = title;
            if (Program.AppIcon != null) Icon = Program.AppIcon;
            KeyPreview = true;
            anim = new Timer();
            anim.Interval = 30;
            anim.Tick += delegate { Invalidate(); };
            anim.Start();
        }

        protected override CreateParams CreateParams
        {
            get { CreateParams cp = base.CreateParams; cp.ClassStyle |= 0x20000; return cp; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            using (GraphicsPath p = Art.RR(new RectangleF(0, 0, ClientSize.Width, ClientSize.Height), 14 * S)) Region = new Region(p);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            anim.Stop();
            anim.Dispose();
            base.OnFormClosed(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.Clear(Ui.Bg);
            g.ScaleTransform(S, S);
            hits.Clear();
            PaintUi(g, Art.Now);
            using (GraphicsPath p = Art.RR(new RectangleF(0.75f, 0.75f, W - 1.5f, H - 1.5f), 14))
            using (Pen pen = new Pen(Ui.Border, 1.5f)) g.DrawPath(pen, p);
        }

        protected virtual void PaintUi(Graphics g, float t) { }

        protected bool Hover(RectangleF r) { return r.Contains(mouse); }
        protected void AddHit(RectangleF r, Action a) { hits.Add(new Hit(r, a)); }

        protected void TitleBar(Graphics g, string title)
        {
            using (SolidBrush b = new SolidBrush(Ui.Panel)) g.FillRectangle(b, 0, 0, W, 50);
            using (Pen p = new Pen(Ui.Border)) g.DrawLine(p, 0, 50, W, 50);
            if (Program.Logo != null) g.DrawImage(Program.Logo, 14, 9, 32, 32);
            Ui.TextL(g, title, Ui.F(17, true), Ui.Fg, 54, 13);
            RectangleF x = new RectangleF(W - 46, 9, 34, 32);
            bool hv = Hover(x);
            if (hv) Ui.FillRR(g, x, 8, Ui.Bad);
            using (Pen p = Art.P(hv ? Color.White : Ui.Sub, 2f))
            {
                g.DrawLine(p, x.X + 12, x.Y + 11, x.Right - 12, x.Bottom - 11);
                g.DrawLine(p, x.Right - 12, x.Y + 11, x.X + 12, x.Bottom - 11);
            }
            AddHit(x, Close);
        }

        protected void Button(Graphics g, RectangleF r, string text, Color c, bool filled, bool enabled, Action a)
        {
            bool hv = enabled && Hover(r);
            Color tc;
            using (GraphicsPath p = Art.RR(r, Math.Min(12, r.Height / 2.4f)))
            {
                if (!enabled)
                {
                    using (SolidBrush b = new SolidBrush(Ui.Panel2)) g.FillPath(b, p);
                    tc = Ui.Sub;
                }
                else if (filled)
                {
                    Color top = hv ? Art.Mix(c, Color.White, 0.25f) : Art.Mix(c, Color.White, 0.1f);
                    Color bot = hv ? c : Art.Mix(c, Color.Black, 0.15f);
                    if (hv) Art.GlowPath(g, p, c, 0.5f);
                    using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(r.X, r.Y - 1, r.Width, r.Height + 2), top, bot, 90f)) g.FillPath(b, p);
                    tc = Color.White;
                }
                else
                {
                    using (SolidBrush b = new SolidBrush(Art.A(hv ? 70 : 28, c))) g.FillPath(b, p);
                    using (Pen pen = new Pen(c, 1.5f)) g.DrawPath(pen, p);
                    tc = Art.Mix(c, Color.White, 0.45f);
                }
            }
            Ui.TextC(g, text, Ui.F(Math.Min(15, r.Height * 0.42f), true), tc, r);
            if (enabled && a != null) AddHit(r, a);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            mouse = new PointF(e.X / S, e.Y / S);
            bool any = false;
            foreach (Hit h in hits) if (h.R.Contains(mouse)) { any = true; break; }
            Cursor = any ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            mouse = new PointF(-1000, -1000);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            PointF m = new PointF(e.X / S, e.Y / S);
            for (int i = hits.Count - 1; i >= 0; i--)
                if (hits[i].R.Contains(m)) { hits[i].A(); Invalidate(); return; }
            if (m.Y < 50) Native.DragWindow(this);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape) Close();
        }
    }

    // ---------------- context menu ----------------
    class MenuColors : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground { get { return Ui.Panel; } }
        public override Color ImageMarginGradientBegin { get { return Ui.Panel; } }
        public override Color ImageMarginGradientMiddle { get { return Ui.Panel; } }
        public override Color ImageMarginGradientEnd { get { return Ui.Panel; } }
        public override Color MenuBorder { get { return Ui.Border; } }
        public override Color MenuItemBorder { get { return Color.Transparent; } }
        public override Color MenuItemSelected { get { return Ui.Panel2; } }
        public override Color SeparatorDark { get { return Ui.Border; } }
        public override Color SeparatorLight { get { return Ui.Panel; } }
    }

    public class ToggleItem : ToolStripMenuItem
    {
        public readonly Func<bool> Get;
        readonly Action<bool> set;
        public ToggleItem(string text, Image img, Func<bool> get, Action<bool> set) : base(text, img)
        {
            Get = get;
            this.set = set;
            ShortcutKeyDisplayString = "            ";
        }
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            set(!Get());
        }
    }

    public class MenuRenderer : ToolStripProfessionalRenderer
    {
        public MenuRenderer() : base(new MenuColors()) { RoundedEdges = false; }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (SolidBrush b = new SolidBrush(Ui.Panel)) e.Graphics.FillRectangle(b, e.AffectedBounds);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath p = Art.RR(new RectangleF(0.5f, 0.5f, e.ToolStrip.Width - 1.5f, e.ToolStrip.Height - 1.5f), 10 * Ui.Dpi))
            using (Pen pen = new Pen(Ui.Border, 1.5f)) g.DrawPath(pen, p);
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e) { }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            ToolStripItem it = e.Item;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float d = Ui.Dpi;
            if (it.Selected && it.Enabled)
            {
                RectangleF r = new RectangleF(5 * d, 1, it.Width - 10 * d, it.Height - 2);
                using (GraphicsPath p = Art.RR(r, 7 * d))
                using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(r.X - 1, r.Y, r.Width + 2, r.Height), Ui.Accent, Color.FromArgb(88, 64, 214), 0f))
                    g.FillPath(b, p);
            }
            ToggleItem ti = it as ToggleItem;
            if (ti != null)
            {
                bool on = ti.Get();
                RectangleF sw = new RectangleF(it.Width - 50 * d, (it.Height - 18 * d) / 2, 34 * d, 18 * d);
                Color bg = on ? (it.Selected ? Color.White : Ui.Good) : Color.FromArgb(70, 74, 106);
                Ui.FillRR(g, sw, sw.Height / 2, bg);
                float k = 14 * d;
                float kx = on ? sw.Right - k - 2 * d : sw.X + 2 * d;
                using (SolidBrush b = new SolidBrush(on && it.Selected ? Ui.Accent : Color.White)) g.FillEllipse(b, kx, sw.Y + 2 * d, k, k);
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? Ui.Fg : Ui.Sub;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            int y = e.Item.Height / 2;
            using (Pen p = new Pen(Ui.Border)) e.Graphics.DrawLine(p, 12, y, e.Item.Width - 12, y);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = Ui.Fg;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = e.ImageRectangle;
            float s = Math.Min(r.Width, r.Height) * 0.8f;
            float cx = r.X + r.Width / 2f, cy = r.Y + r.Height / 2f;
            using (SolidBrush b = new SolidBrush(Ui.Accent)) g.FillEllipse(b, cx - s / 2, cy - s / 2, s, s);
            using (SolidBrush b = new SolidBrush(Color.White)) g.FillEllipse(b, cx - s / 5, cy - s / 5, s * 0.4f, s * 0.4f);
        }
    }

    public class HeaderItem : ToolStripMenuItem
    {
        public override Size GetPreferredSize(Size constrainingSize)
        {
            return new Size((int)(300 * Ui.Dpi), (int)(96 * Ui.Dpi));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.ScaleTransform(Ui.Dpi, Ui.Dpi);
            float w = Width / Ui.Dpi;
            RectangleF card = new RectangleF(8, 6, w - 16, 84);
            using (GraphicsPath p = Art.RR(card, 10))
            using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(card.X - 1, card.Y, card.Width + 2, card.Height), Color.FromArgb(92, 64, 220), Color.FromArgb(46, 36, 110), 0f))
            {
                g.FillPath(b, p);
                if (Selected) using (Pen pen = new Pen(Color.FromArgb(180, 255, 255, 255), 1.5f)) g.DrawPath(pen, p);
            }
            if (Program.Logo != null) g.DrawImage(Program.Logo, 16, 14, 56, 56);
            SaveData s = Game.S;
            Ui.TextL(g, "BongoBro", Ui.F(17, true), Color.White, 80, 11);
            Ui.TextR(g, "коллекция " + s.Owned.Count + "/" + Items.All.Count, Ui.F(12, true), Color.FromArgb(255, 220, 120), new RectangleF(w - 160, 13, 146, 22));
            Ui.TextL(g, "Нажатий: " + Ui.Num(s.Keys) + "   Кликов: " + Ui.Num(s.Clicks), Ui.F(12, false), Color.FromArgb(220, 214, 255), 80, 36);
            long left = Game.BoxEvery - s.Keys % Game.BoxEvery;
            Ui.Progress(g, new RectangleF(80, 58, w - 104, 7), 1f - left / (float)Game.BoxEvery, Ui.Gold);
            Ui.TextL(g, "до лутбокса: " + Ui.Num(left), Ui.F(11, false), Color.FromArgb(190, 184, 230), 80, 67);
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;

namespace BongoBro
{
    // Key counter + loot box badge in its own transparent window, so it can be scaled
    // independently of the character. It follows the character when he is dragged.
    public class CounterForm : Form
    {
        // layout in base units
        const float BaseW = 222, BaseH = 88;
        static readonly RectangleF PillRect = new RectangleF(4, 42, 150, 40);
        static readonly RectangleF GiftRect = new RectangleF(158, 30, 60, 56);
        static readonly FontFamily Segoe = new FontFamily("Segoe UI");

        readonly MainForm owner;
        Bitmap canvas;
        float scale;
        bool placing;
        string lastSig = "";

        public CounterForm(MainForm owner)
        {
            this.owner = owner;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "BongoBro counter";
            TopMost = Game.S.TopMost;
            scale = Game.S.CounterScale * Ui.Dpi;
            Size = PxSize();
        }

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

        public Size PxSize() { return new Size((int)Math.Ceiling(BaseW * scale), (int)Math.Ceiling(BaseH * scale)); }

        // Moves the window without treating it as a user drag.
        public void PlaceAt(Point p)
        {
            placing = true;
            Location = p;
            placing = false;
            if (IsHandleCreated) Render();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            if (!placing && Visible) owner.CounterDragged(Location);
        }

        // Keeps the pill's bottom-left corner in place while resizing.
        public void SetScale(float s)
        {
            Game.S.CounterScale = s;
            int left = Left + (int)(PillRect.X * scale), bottom = Top + (int)(PillRect.Bottom * scale);
            scale = s * Ui.Dpi;
            placing = true;
            Location = new Point(left - (int)(PillRect.X * scale), bottom - (int)(PillRect.Bottom * scale));
            placing = false;
            owner.CounterDragged(Location);
            Render();
        }

        public void Tick(int boxFxUntil, string fxText, float t, bool themeAnimated)
        {
            int now = Environment.TickCount;
            bool fx = now - boxFxUntil < 0;
            string sig = string.Concat(new object[] { Game.S.ShowCounter, Game.S.Keys, Game.S.Boxes, Game.S.GoldBoxes, Game.S.Theme, scale });
            if (fx || themeAnimated || sig != lastSig)
            {
                lastSig = sig;
                Render(boxFxUntil, fxText, t);
            }
        }

        void Render() { Render(-100000, "", Art.Now); }

        void Render(int boxFxUntil, string fxText, float t)
        {
            if (!IsHandleCreated) return;
            Size sz = PxSize();
            if (canvas == null || canvas.Width != sz.Width || canvas.Height != sz.Height)
            {
                if (canvas != null) canvas.Dispose();
                canvas = new Bitmap(sz.Width, sz.Height, PixelFormat.Format32bppArgb);
                Size = sz;
            }
            int now = Environment.TickCount;
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.ScaleTransform(scale, scale);
                if (Game.S.ShowCounter)
                {
                    float k = (Game.S.Keys % Game.BoxEvery) / (float)Game.BoxEvery;
                    Art.DrawCounter(g, PillRect, Art.GetTheme(Game.S.Theme), Ui.Num(Game.S.Keys), k, t);
                }
                bool fx = now - boxFxUntil < 0;
                int total = Game.S.Boxes + Game.S.GoldBoxes;
                if (total > 0)
                {
                    float bounce = fx ? Math.Abs(Art.Sin(t * 9)) * 7 : 0;
                    Art.DrawLootBox(g, 186, 62 - bounce, 40, t, fx, Game.S.GoldBoxes > 0);
                    using (SolidBrush b = new SolidBrush(Ui.Bad)) g.FillEllipse(b, 198, 32, 19, 19);
                    using (Pen pen = new Pen(Color.White, 1.6f)) g.DrawEllipse(pen, 198, 32, 19, 19);
                    Ui.TextC(g, total > 99 ? "99" : total.ToString(), Ui.F(11, true), Color.White, new RectangleF(197, 32, 21, 19));
                }
                if (fx)
                {
                    float a = (boxFxUntil - now) / 3000f;
                    float y = 2 + (1 - a) * 6;
                    using (GraphicsPath tp = new GraphicsPath())
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        tp.AddString(fxText, Segoe, (int)FontStyle.Bold, 24f, new RectangleF(0, y, BaseW, 36), sf);
                        using (Pen o = Art.P(Art.A((int)(230 * a), Color.FromArgb(40, 16, 80)), 5f)) g.DrawPath(o, tp);
                        using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(0, y, BaseW, 34), Art.A((int)(255 * a), Color.FromArgb(255, 236, 140)), Art.A((int)(255 * a), Color.FromArgb(255, 160, 30)), 90f)) g.FillPath(b, tp);
                    }
                }
            }
            Native.SetLayeredBitmap(this, canvas);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            PointF p = new PointF(e.X / scale, e.Y / scale);
            if (Game.S.Boxes + Game.S.GoldBoxes > 0 && GiftRect.Contains(p)) { owner.OpenLoot(); return; }
            Native.DragWindow(this);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Right) owner.ShowMenu();
        }
    }
}

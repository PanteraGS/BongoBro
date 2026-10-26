using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace BongoBro
{
    // Owner-drawn slider hosted inside the context menu.
    public class MenuSlider : Control
    {
        public readonly ToolStripControlHost Host;
        public event Action<float> ValueChanged;
        public event Action Committed;

        readonly string label;
        readonly float min, max;
        float value;
        bool drag, hover;

        public MenuSlider(string label, float min, float max, float value)
        {
            this.label = label;
            this.min = min;
            this.max = max;
            this.value = Clamp(value);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Ui.Panel;
            Cursor = Cursors.Hand;
            Size = new Size((int)(290 * Ui.Dpi), (int)(52 * Ui.Dpi));
            Host = new ToolStripControlHost(this);
            Host.AutoSize = false;
            Host.Size = Size;
            Host.Margin = Padding.Empty;
            Host.Padding = Padding.Empty;
            Host.BackColor = Ui.Panel;
        }

        float Clamp(float v) { return v < min ? min : v > max ? max : v; }

        public void SetValue(float v)
        {
            value = Clamp(v);
            Invalidate();
        }

        RectangleF Track()
        {
            float d = Ui.Dpi;
            return new RectangleF(18 * d, 34 * d, Width - 36 * d, 6 * d);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.Clear(Ui.Panel);
            float d = Ui.Dpi;
            Ui.TextL(g, label, Ui.F(13 * d, false), Ui.Fg, 16 * d, 6 * d);
            Ui.TextR(g, (int)Math.Round(value * 100) + "%", Ui.F(13 * d, true), Color.FromArgb(196, 178, 255), new RectangleF(Width / 2f, 5 * d, Width / 2f - 16 * d, 20 * d));
            RectangleF tr = Track();
            float k = (value - min) / (max - min);
            Ui.Progress(g, tr, k, Ui.Accent);
            float kx = tr.X + tr.Width * k, ky = tr.Y + tr.Height / 2, r = (drag || hover ? 9f : 7.5f) * d;
            if (drag || hover) Art.Glow(g, kx, ky, r * 2.2f, r * 2.2f, Ui.Accent, 130);
            using (SolidBrush b = new SolidBrush(Color.White)) g.FillEllipse(b, kx - r, ky - r, r * 2, r * 2);
            using (Pen p = new Pen(Ui.Accent, 2.5f * d)) g.DrawEllipse(p, kx - r, ky - r, r * 2, r * 2);
        }

        void SetFromX(int x)
        {
            RectangleF tr = Track();
            float v = Clamp(min + (x - tr.X) / tr.Width * (max - min));
            v = (float)Math.Round(v * 100) / 100f;
            if (Math.Abs(v - value) < 0.001f) return;
            value = v;
            Invalidate();
            if (ValueChanged != null) ValueChanged(v);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            drag = true;
            Capture = true;
            SetFromX(e.X);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (drag) SetFromX(e.X);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!drag) return;
            drag = false;
            Capture = false;
            Invalidate();
            if (Committed != null) Committed();
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); hover = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hover = false; Invalidate(); }
    }
}

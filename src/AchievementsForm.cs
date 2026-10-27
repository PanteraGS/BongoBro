using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BongoBro
{
    public class AchievementsForm : CustomForm
    {
        // two columns of achievements
        const int PerColumn = 11;

        public AchievementsForm() : base(1240, 652, "BongoBro: достижения") { }

        protected override void PaintUi(Graphics g, float t)
        {
            SaveData s = Game.S;
            TitleBar(g, "ДОСТИЖЕНИЯ");
            Ui.TextR(g, "Выполнено: " + s.Achieved.Count + " / " + Achievement.All.Count, Ui.F(15, true), Color.FromArgb(255, 220, 120), new RectangleF(W - 330, 10, 270, 30));
            float cw = (W - 48) / 2f;
            for (int i = 0; i < Achievement.All.Count; i++)
            {
                Achievement a = Achievement.All[i];
                RectangleF r = new RectangleF(16 + (i / PerColumn) * (cw + 16), 62 + (i % PerColumn) * 53, cw, 47);
                bool done = s.Achieved.Contains(a.Id);
                long cur = Math.Min(a.Progress(s), a.Target);
                Color accent = a.Gold ? Ui.Gold : Ui.Accent;

                using (GraphicsPath p = Art.RR(r, 12))
                {
                    if (done && a.Gold) Art.GlowPath(g, p, Ui.Gold, 0.45f + 0.15f * Art.Sin(t * 3 + i));
                    Color bg = done ? Art.Mix(Ui.Panel, a.Gold ? Ui.Gold : Ui.Good, 0.16f) : Ui.Panel;
                    using (SolidBrush b = new SolidBrush(bg)) g.FillPath(b, p);
                    using (Pen pen = new Pen(done ? Art.A(200, a.Gold ? Ui.Gold : Ui.Good) : Ui.Border, 1.4f)) g.DrawPath(pen, p);
                }

                Trophy(g, r.X + 28, r.Y + 24, done ? (a.Gold ? Ui.Gold : Color.FromArgb(150, 220, 255)) : Color.FromArgb(90, 96, 124), done);
                Ui.TextL(g, a.Name, Ui.F(14, true), done ? Color.White : Ui.Fg, r.X + 54, r.Y + 4);
                Ui.TextL(g, a.Desc, Ui.F(11.5f, false), Ui.Sub, r.X + 54, r.Y + 25);

                // progress
                RectangleF bar = new RectangleF(r.X + 330, r.Y + 28, 112, 7);
                Ui.Progress(g, bar, cur / (float)a.Target, done ? Ui.Good : accent);
                string prog = done ? "Готово ✓" : Ui.Num(cur) + " / " + Ui.Num(a.Target);
                Ui.TextC(g, prog, Ui.F(11.5f, true), done ? Ui.Good : Ui.Fg, new RectangleF(bar.X - 10, r.Y + 5, bar.Width + 20, 20));

                // reward
                float bx = r.Right - 136;
                Art.DrawLootBox(g, bx + 14, r.Y + 26, 26, t, false, a.Gold);
                Ui.TextL(g, a.Gold ? "Золотой" : a.RewardText(), Ui.F(12, true), a.Gold ? Ui.Gold : Color.FromArgb(200, 184, 255), bx + 36, r.Y + 14);
            }
        }

        static void Trophy(Graphics g, float cx, float cy, Color c, bool done)
        {
            if (done) Art.Glow(g, cx, cy, 22, 22, c, 90);
            using (GraphicsPath cup = new GraphicsPath())
            {
                cup.AddBezier(cx - 11, cy - 13, cx - 11, cy + 2, cx - 6, cy + 5, cx, cy + 5);
                cup.AddBezier(cx, cy + 5, cx + 6, cy + 5, cx + 11, cy + 2, cx + 11, cy - 13);
                cup.CloseFigure();
                using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(cx - 12, cy - 14, 24, 20), Art.Mix(c, Color.White, 0.35f), c, 90f)) g.FillPath(b, cup);
                using (Pen p = Art.P(Art.Mix(c, Color.Black, 0.45f), 1.5f)) g.DrawPath(p, cup);
            }
            using (Pen h = Art.P(Art.Mix(c, Color.Black, 0.2f), 2.2f))
            {
                g.DrawArc(h, cx - 17, cy - 11, 8, 9, 90, 180);
                g.DrawArc(h, cx + 9, cy - 11, 8, 9, 270, 180);
            }
            using (SolidBrush b = new SolidBrush(Art.Mix(c, Color.Black, 0.15f)))
            {
                g.FillRectangle(b, cx - 2, cy + 5, 4, 5);
                g.FillRectangle(b, cx - 8, cy + 10, 16, 4);
            }
            if (done)
                using (GraphicsPath st = Art.Star(cx, cy - 5, 4.5f, 2f, 5, -90))
                using (SolidBrush w = new SolidBrush(Color.FromArgb(230, 255, 255, 255))) g.FillPath(w, st);
        }
    }
}

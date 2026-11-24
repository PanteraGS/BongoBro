using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BongoBro
{
    // Today's three quests, their progress and rewards.
    public class QuestsForm : CustomForm
    {
        public QuestsForm() : base(680, 472, "BongoBro: задания") { }

        protected override void PaintUi(Graphics g, float t)
        {
            SaveData s = Game.S;
            Quests.Refresh(s);
            TitleBar(g, "ЗАДАНИЯ ДНЯ");
            TimeSpan left = Quests.ResetAt - DateTime.Now;
            Ui.TextR(g, "Новые через " + (int)left.TotalHours + " ч " + left.Minutes + " мин", Ui.F(13, true), Color.FromArgb(255, 220, 120), new RectangleF(W - 330, 12, 270, 30));

            int done = 0;
            for (int i = 0; i < Quests.Today.Count; i++)
            {
                Quest q = Quests.Today[i];
                bool ok = Quests.IsDone(s, q);
                if (ok) done++;
                long cur = Math.Min(q.Progress(s), q.Target);
                RectangleF r = new RectangleF(16, 64 + i * 86, W - 32, 76);
                using (GraphicsPath p = Art.RR(r, 12))
                {
                    using (SolidBrush b = new SolidBrush(ok ? Art.Mix(Ui.Panel, Ui.Good, 0.16f) : Ui.Panel)) g.FillPath(b, p);
                    using (Pen pen = new Pen(ok ? Art.A(200, Ui.Good) : Ui.Border, 1.4f)) g.DrawPath(pen, p);
                }
                // check mark circle
                RectangleF c = new RectangleF(r.X + 16, r.Y + 22, 32, 32);
                using (SolidBrush b = new SolidBrush(ok ? Ui.Good : Art.Mix(Ui.Panel, Color.White, 0.08f))) g.FillEllipse(b, c);
                Ui.TextC(g, ok ? "✓" : (i + 1).ToString(), Ui.F(15, true), ok ? Color.White : Ui.Sub, c);

                Ui.TextL(g, q.Text, Ui.F(16, true), ok ? Color.White : Ui.Fg, r.X + 62, r.Y + 12);
                RectangleF bar = new RectangleF(r.X + 64, r.Y + 50, 300, 8);
                Ui.Progress(g, bar, cur / (float)q.Target, ok ? Ui.Good : Ui.Accent);
                Ui.TextL(g, ok ? "Готово" : Ui.Num(cur) + " / " + Ui.Num(q.Target), Ui.F(12, true), ok ? Ui.Good : Ui.Sub, bar.Right + 12, r.Y + 44);

                float bx = r.Right - 150;
                Art.DrawLootBox(g, bx + 18, r.Y + 40, 34, t, !ok, false);
                Ui.TextL(g, "1 лутбокс", Ui.F(13, true), Color.FromArgb(200, 184, 255), bx + 44, r.Y + 28);
            }

            // bonus for finishing all three
            RectangleF br = new RectangleF(16, 330, W - 32, 80);
            bool all = done == Quests.Count;
            using (GraphicsPath p = Art.RR(br, 12))
            {
                if (all) Art.GlowPath(g, p, Ui.Gold, 0.4f + 0.15f * Art.Sin(t * 3));
                using (SolidBrush b = new SolidBrush(Art.Mix(Ui.Panel, Ui.Gold, all ? 0.18f : 0.07f))) g.FillPath(b, p);
                using (Pen pen = new Pen(Art.A(all ? 220 : 120, Ui.Gold), 1.4f)) g.DrawPath(pen, p);
            }
            Ui.TextL(g, all ? "Все задания выполнены!" : "Выполни все три задания", Ui.F(16, true), all ? Ui.Gold : Ui.Fg, br.X + 20, br.Y + 14);
            Ui.TextL(g, all ? "Бонус получен. Новые задания появятся в полночь." : "и получи ещё +" + Quests.BonusBoxes + " лутбокс сверху", Ui.F(12.5f, false), Ui.Sub, br.X + 20, br.Y + 44);
            for (int i = 0; i < Quests.Count; i++)
                using (SolidBrush b = new SolidBrush(i < done ? Ui.Gold : Art.Mix(Ui.Panel, Color.White, 0.12f)))
                    g.FillEllipse(b, br.Right - 210 + i * 24, br.Y + 32, 16, 16);
            Art.DrawLootBox(g, br.Right - 100, br.Y + 44, 44, t, all, false);
            Ui.TextC(g, "+" + Quests.BonusBoxes, Ui.F(15, true), Ui.Gold, new RectangleF(br.Right - 60, br.Y + 28, 40, 24));

            // daily streak: a box for every day in a row, a gold one every 7th
            RectangleF sr = new RectangleF(16, 420, W - 32, 42);
            using (GraphicsPath p = Art.RR(sr, 12))
            {
                using (SolidBrush b = new SolidBrush(Art.Mix(Ui.Panel, Ui.Accent, 0.08f))) g.FillPath(b, p);
                using (Pen pen = new Pen(Ui.Border, 1.4f)) g.DrawPath(pen, p);
            }
            Ui.TextL(g, "Стрик: " + s.Streak + " " + MainForm.StreakWord(s.Streak), Ui.F(14, true), Ui.Fg, sr.X + 18, sr.Y + 4);
            Ui.TextL(g, "Заходи каждый день: +1 лутбокс, а каждый 7-й день — ещё и золотой", Ui.F(11.5f, false), Ui.Sub, sr.X + 18, sr.Y + 24);
            int cyc = s.Streak % 7;
            for (int i = 1; i <= 7; i++)
            {
                bool on = i <= cyc || s.Streak > 0 && cyc == 0;
                Color c = i == 7 ? (on ? Ui.Gold : Art.Mix(Ui.Panel, Ui.Gold, 0.35f)) : on ? Ui.Accent : Art.Mix(Ui.Panel, Color.White, 0.12f);
                using (SolidBrush b = new SolidBrush(c)) g.FillEllipse(b, sr.Right - 196 + i * 24, sr.Y + 13, 15, 15);
            }
        }
    }
}

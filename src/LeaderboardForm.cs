using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BongoBro
{
    public class LeaderboardForm : CustomForm
    {
        enum St { Setup, Joining, Loading, Board, Offline }

        const int Rows = 10;
        static readonly Color[] Medals = { Color.FromArgb(255, 196, 40), Color.FromArgb(200, 208, 222), Color.FromArgb(214, 136, 74) };

        St st;
        Leaderboard.Board board;
        string error;
        readonly TextBox nickBox;

        public LeaderboardForm() : base(560, 660, "BongoBro: лидерборд")
        {
            nickBox = new TextBox();
            nickBox.BorderStyle = BorderStyle.None;
            nickBox.BackColor = Ui.Panel2;
            nickBox.ForeColor = Color.White;
            nickBox.Font = new Font("Segoe UI", 18 * S, FontStyle.Bold, GraphicsUnit.Pixel);
            nickBox.MaxLength = 16;
            nickBox.Location = new Point((int)(46 * S), (int)(206 * S));
            nickBox.Width = (int)(468 * S);
            nickBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Join(); } };
            Controls.Add(nickBox);

            if (Game.S.Nick == null) SetState(St.Setup);
            else
            {
                // send fresh numbers first, then show the table
                SetState(St.Loading);
                Leaderboard.SubmitAsync(this, Game.S.Nick, err => Reload());
            }
        }

        void SetState(St s)
        {
            st = s;
            nickBox.Visible = s == St.Setup || s == St.Joining;
            nickBox.Enabled = s == St.Setup;
            if (s == St.Setup)
            {
                if (nickBox.Text.Length == 0 && Game.S.Nick != null) nickBox.Text = Game.S.Nick;
                nickBox.Focus();
            }
        }

        void Reload()
        {
            SetState(St.Loading);
            Leaderboard.FetchAsync(this, b => { board = b; SetState(b == null ? St.Offline : St.Board); });
        }

        void Join()
        {
            string name = nickBox.Text.Trim();
            if (name.Length < 2) { error = Leaderboard.ErrorText("bad_name"); return; }
            error = null;
            SetState(St.Joining);
            Leaderboard.SubmitAsync(this, name, err =>
            {
                if (err != null) { error = Leaderboard.ErrorText(err); SetState(St.Setup); return; }
                Game.S.Nick = name;
                Game.Save();
                Reload();
            });
        }

        protected override void PaintUi(Graphics g, float t)
        {
            TitleBar(g, "ЛИДЕРБОРД");
            Ui.TextL(g, "Нажатия клавиш  •  таблица обновляется каждые 12 часов", Ui.F(13, false), Ui.Sub, 24, 60);
            switch (st)
            {
                case St.Setup:
                case St.Joining:
                    PaintSetup(g, t);
                    break;
                case St.Loading:
                    Ui.TextC(g, "Загружаем" + new string('.', 1 + (int)(t * 3) % 3), Ui.F(20, true), Ui.Fg, new RectangleF(0, 280, W, 40));
                    break;
                case St.Offline:
                    Ui.TextC(g, "Нет связи с сервером", Ui.F(20, true), Ui.Bad, new RectangleF(0, 260, W, 34));
                    Ui.TextC(g, "Проверь интернет и попробуй ещё раз.", Ui.F(13, false), Ui.Sub, new RectangleF(0, 296, W, 24));
                    Button(g, new RectangleF(W / 2 - 100, 340, 200, 44), "Повторить", Ui.Accent, false, true, Reload);
                    break;
                case St.Board:
                    PaintBoard(g, t);
                    break;
            }
        }

        void PaintSetup(Graphics g, float t)
        {
            Art.DrawLootBox(g, W / 2, 128, 60, t, true, true);
            Ui.TextC(g, Game.S.Nick == null ? "Придумай ник для таблицы" : "Новый ник", Ui.F(18, true), Ui.Fg, new RectangleF(0, 160, W, 30));
            Ui.FillRR(g, new RectangleF(32, 198, W - 64, 42), 10, Ui.Panel2);
            using (GraphicsPath p = Art.RR(new RectangleF(32, 198, W - 64, 42), 10))
            using (Pen pen = new Pen(Ui.Accent, 1.6f)) g.DrawPath(pen, p);
            Ui.TextC(g, "2–16 символов. В таблицу уходят только ник, персонаж и число нажатий.", Ui.F(12, false), Ui.Sub, new RectangleF(0, 248, W, 22));
            if (error != null) Ui.TextC(g, error, Ui.F(13, true), Ui.Bad, new RectangleF(0, 276, W, 24));
            bool busy = st == St.Joining;
            Button(g, new RectangleF(W / 2 - 110, 316, 220, 48), busy ? "Отправляем..." : "Участвовать", Ui.Gold, true, !busy, Join);
            if (Game.S.Nick != null && !busy) Button(g, new RectangleF(W / 2 - 110, 376, 220, 40), "Отмена", Ui.Sub, false, true, Reload);
        }

        void PaintBoard(Graphics g, float t)
        {
            TimeSpan left = board.Next - DateTime.Now;
            if (left < TimeSpan.Zero) left = TimeSpan.Zero;
            Ui.TextL(g, "Обновлено " + board.Updated.ToString("dd.MM HH:mm") + "  •  следующее через " + (int)left.TotalHours + " ч " + left.Minutes + " мин",
                Ui.F(12.5f, true), Color.FromArgb(200, 184, 255), 24, 82);
            Ui.TextR(g, "Игроков: " + board.Players, Ui.F(12.5f, true), Ui.Sub, new RectangleF(W - 200, 80, 176, 22));

            if (board.Top.Count == 0)
            {
                Ui.TextC(g, "Пока никого нет — стань первым!", Ui.F(18, true), Ui.Fg, new RectangleF(0, 260, W, 34));
            }
            bool meShown = false;
            int n = Math.Min(board.Top.Count, Rows);
            for (int i = 0; i < n; i++)
            {
                Leaderboard.Entry e = board.Top[i];
                if (e.Me) meShown = true;
                Row(g, new RectangleF(16, 112 + i * 44, W - 32, 38), e, t);
            }
            if (board.Me != null && !meShown)
            {
                Ui.TextC(g, "•  •  •", Ui.F(12, true), Ui.Sub, new RectangleF(0, 112 + n * 44, W, 14));
                Row(g, new RectangleF(16, 128 + n * 44, W - 32, 38), board.Me, t);
            }
            string mine = board.Me != null
                ? "Сейчас у тебя " + Ui.Num(Game.S.Keys) + " нажатий — в таблице они обновятся в " + board.Next.ToString("HH:mm")
                : "Ты попадёшь в таблицу при следующем обновлении, в " + board.Next.ToString("HH:mm");
            Ui.TextC(g, mine, Ui.F(12, false), Ui.Sub, new RectangleF(0, 568, W, 22));
            Button(g, new RectangleF(W / 2 - 210, 600, 200, 40), "Обновить", Ui.Accent, false, true, Reload);
            Button(g, new RectangleF(W / 2 + 10, 600, 200, 40), "Сменить ник", Ui.Sub, false, true, () => { error = null; SetState(St.Setup); });
        }

        void Row(Graphics g, RectangleF r, Leaderboard.Entry e, float t)
        {
            using (GraphicsPath p = Art.RR(r, 10))
            {
                Color bg = e.Me ? Art.Mix(Ui.Panel, Ui.Accent, 0.35f) : Ui.Panel;
                using (SolidBrush b = new SolidBrush(bg)) g.FillPath(b, p);
                if (e.Me) using (Pen pen = new Pen(Ui.Accent, 1.6f)) g.DrawPath(pen, p);
            }
            float cy = r.Y + r.Height / 2;
            if (e.Rank <= 3)
            {
                Color m = Medals[e.Rank - 1];
                Art.Glow(g, r.X + 24, cy, 18, 18, m, 90);
                using (SolidBrush b = new SolidBrush(m)) g.FillEllipse(b, r.X + 12, cy - 12, 24, 24);
                Ui.TextC(g, e.Rank.ToString(), Ui.F(13, true), Color.FromArgb(60, 40, 10), new RectangleF(r.X + 12, cy - 12, 24, 24));
            }
            else Ui.TextC(g, e.Rank.ToString(), Ui.F(14, true), Ui.Sub, new RectangleF(r.X + 4, cy - 12, 40, 24));

            Pose face = new Pose();
            face.T = t;
            face.Character = Characters.Normalize(e.Char);
            Art.DrawBust(g, new RectangleF(96, 40, 168, 168), new RectangleF(r.X + 46, r.Y + 2, 34, 34), face);

            Ui.TextL(g, e.Name + (e.Me ? "  (ты)" : ""), Ui.F(15, true), e.Me ? Color.White : Ui.Fg, r.X + 88, r.Y + 8);
            Ui.TextR(g, Ui.Num(e.Keys), Ui.F(15, true), e.Rank <= 3 ? Medals[e.Rank - 1] : Ui.Fg, new RectangleF(r.Right - 200, r.Y + 7, 186, 24));
        }
    }
}

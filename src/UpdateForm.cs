using System;
using System.Drawing;

namespace BongoBro
{
    public class UpdateForm : CustomForm
    {
        enum St { Checking, UpToDate, Available, Downloading, Error, Offline }

        St st;
        int percent;
        string error;

        public UpdateForm() : base(500, 340, "BongoBro: обновление")
        {
            st = Updater.Available ? St.Available : St.Checking;
            if (st == St.Checking) Check();
        }

        void Check()
        {
            st = St.Checking;
            Updater.CheckAsync(this, ok => { st = !ok ? St.Offline : Updater.Available ? St.Available : St.UpToDate; });
        }

        protected override void PaintUi(Graphics g, float t)
        {
            TitleBar(g, "ОБНОВЛЕНИЕ");
            Ui.TextL(g, "Установлена версия " + Program.Version, Ui.F(13, false), Ui.Sub, 24, 64);
            RectangleF main = new RectangleF(24, 92, W - 48, 30);
            RectangleF body = new RectangleF(24, 130, W - 48, 130);
            RectangleF btn = new RectangleF(W / 2 - 120, 276, 240, 46);
            switch (st)
            {
                case St.Checking:
                    Ui.TextL(g, "Проверяем GitHub" + new string('.', 1 + (int)(t * 3) % 3), Ui.F(20, true), Ui.Fg, main.X, main.Y);
                    break;
                case St.UpToDate:
                    Ui.TextL(g, "У тебя последняя версия ✓", Ui.F(20, true), Ui.Good, main.X, main.Y);
                    Button(g, btn, "Проверить ещё раз", Ui.Accent, false, true, Check);
                    break;
                case St.Offline:
                    Ui.TextL(g, "Не удалось связаться с GitHub", Ui.F(20, true), Ui.Bad, main.X, main.Y);
                    Ui.TextL(g, "Проверь интернет и попробуй ещё раз.", Ui.F(13, false), Ui.Sub, body.X, body.Y);
                    Button(g, btn, "Повторить", Ui.Accent, false, true, Check);
                    break;
                case St.Available:
                case St.Error:
                    Ui.TextL(g, "Доступна версия " + Updater.LatestVersion, Ui.F(20, true), Ui.Gold, main.X, main.Y);
                    if (st == St.Error)
                        Ui.TextC(g, error, Ui.F(13, true), Ui.Bad, body);
                    else
                        DrawNotes(g, body);
                    Button(g, btn, st == St.Error ? "Попробовать снова" : "ОБНОВИТЬ", Ui.Gold, true, true, Start);
                    break;
                case St.Downloading:
                    Ui.TextL(g, "Скачиваем версию " + Updater.LatestVersion, Ui.F(20, true), Ui.Fg, main.X, main.Y);
                    Ui.Progress(g, new RectangleF(24, 180, W - 48, 12), percent / 100f, Ui.Gold);
                    Ui.TextC(g, percent + "%  •  после загрузки BongoBro перезапустится сам", Ui.F(13, false), Ui.Sub, new RectangleF(24, 200, W - 48, 24));
                    break;
            }
        }

        void DrawNotes(Graphics g, RectangleF r)
        {
            Ui.FillRR(g, r, 10, Ui.Panel);
            string notes = string.IsNullOrEmpty(Updater.Notes) ? "Что нового: описание не указано." : Updater.Notes.Trim();
            using (StringFormat sf = new StringFormat())
            using (SolidBrush b = new SolidBrush(Ui.Fg))
            {
                sf.Trimming = StringTrimming.EllipsisWord;
                g.DrawString(notes, Ui.F(13, false), b, new RectangleF(r.X + 12, r.Y + 10, r.Width - 24, r.Height - 20), sf);
            }
            Ui.TextL(g, "Прогресс, коллекция и достижения сохранятся.", Ui.F(12, false), Ui.Sub, r.X, r.Bottom + 4);
        }

        void Start()
        {
            st = St.Downloading;
            percent = 0;
            Updater.DownloadAndRestart(p => { percent = p; }, msg => { error = msg; st = St.Error; });
        }
    }
}

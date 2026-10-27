using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BongoBro
{
    // Asks for the developer code. Only its hash is shipped (see Game.CheckDevCode).
    public class DevCodeForm : CustomForm
    {
        readonly MainForm owner;
        readonly TextBox box;
        string error;

        public DevCodeForm(MainForm owner) : base(420, 250, "BongoBro: код разработчика")
        {
            this.owner = owner;
            box = new TextBox();
            box.BorderStyle = BorderStyle.None;
            box.BackColor = Ui.Panel2;
            box.ForeColor = Color.White;
            box.Font = new Font("Segoe UI", 18 * S, FontStyle.Bold, GraphicsUnit.Pixel);
            box.UseSystemPasswordChar = true;
            box.Location = new Point((int)(46 * S), (int)(108 * S));
            box.Width = (int)(328 * S);
            box.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; TryEnter(); } };
            Controls.Add(box);
            Shown += delegate { box.Focus(); };
        }

        void TryEnter()
        {
            if (!Game.CheckDevCode(box.Text)) { error = "Неверный код"; box.SelectAll(); return; }
            Game.EnterDev();
            Close();
            owner.OpenDev();
        }

        protected override void PaintUi(Graphics g, float t)
        {
            TitleBar(g, "КОД РАЗРАБОТЧИКА");
            Ui.TextL(g, "Введи код, чтобы открыть панель проверки", Ui.F(13, false), Ui.Sub, 32, 70);
            Ui.FillRR(g, new RectangleF(32, 100, W - 64, 42), 10, Ui.Panel2);
            using (GraphicsPath p = Art.RR(new RectangleF(32, 100, W - 64, 42), 10))
            using (Pen pen = new Pen(error != null ? Ui.Bad : Ui.Accent, 1.6f)) g.DrawPath(pen, p);
            if (error != null) Ui.TextL(g, error, Ui.F(13, true), Ui.Bad, 32, 150);
            Button(g, new RectangleF(W / 2 - 90, 186, 180, 44), "Войти", Ui.Accent, true, true, TryEnter);
        }
    }

    // Test panel: everything here happens in a sandbox and is thrown away on exit.
    public class DevForm : CustomForm
    {
        readonly MainForm owner;

        public DevForm(MainForm owner) : base(700, 690, "BongoBro: режим разработчика")
        {
            this.owner = owner;
        }

        float y;

        void Section(Graphics g, string title)
        {
            y += 10;
            Ui.TextL(g, title, Ui.F(14, true), Color.FromArgb(200, 184, 255), 24, y);
            y += 26;
        }

        // lays out a row of equal buttons; `active` marks the selected one
        void Row(Graphics g, string[] labels, Action<int> click, int active)
        {
            float gap = 8, w = (W - 48 - gap * (labels.Length - 1)) / labels.Length;
            for (int i = 0; i < labels.Length; i++)
            {
                int k = i;
                Button(g, new RectangleF(24 + i * (w + gap), y, w, 34), labels[i], Ui.Accent, i == active, true, () => click(k));
            }
            y += 42;
        }

        protected override void PaintUi(Graphics g, float t)
        {
            TitleBar(g, "РЕЖИМ РАЗРАБОТЧИКА");
            Ui.FillRR(g, new RectangleF(16, 58, W - 32, 30), 8, Color.FromArgb(60, 239, 68, 68));
            Ui.TextC(g, "Песочница: изменения не сохраняются и не попадают в лидерборд", Ui.F(12.5f, true), Color.FromArgb(255, 190, 190), new RectangleF(16, 58, W - 32, 30));
            y = 92;
            SaveData s = Game.S;

            Section(g, "Ресурсы   (лутбоксов " + s.Boxes + ", золотых " + s.GoldBoxes + ", нажатий " + Ui.Num(s.Keys) + ")");
            Row(g, new[] { "+10 лутбоксов", "+3 золотых", "До лутбокса 10 нажатий", "Все предметы" }, i =>
            {
                if (i == 0) s.Boxes += 10;
                else if (i == 1) s.GoldBoxes += 3;
                else if (i == 2) s.Keys += Game.BoxEvery - 10 - s.Keys % Game.BoxEvery;
                else foreach (Item it in Items.All) s.Owned.Add(it.Id);
            }, -1);

            Section(g, "Настроение");
            Mood[] moods = { Mood.Normal, Mood.Yawn, Mood.Sleep, Mood.Surprised, Mood.Focused, Mood.Happy };
            string[] moodNames = { "Обычное", "Зевает", "Спит", "Удивлён", "Сосредоточен", "Радуется" };
            int mi = Game.DevMood.HasValue ? Array.IndexOf(moods, Game.DevMood.Value) + 1 : 0;
            string[] moodRow = new string[moods.Length + 1];
            moodRow[0] = "Авто";
            Array.Copy(moodNames, 0, moodRow, 1, moodNames.Length);
            Row(g, moodRow, i => { Game.DevMood = i == 0 ? (Mood?)null : moods[i - 1]; }, mi);

            Section(g, "Время суток");
            DayPhase[] days = { DayPhase.Morning, DayPhase.Day, DayPhase.Evening, DayPhase.Night };
            int di = Game.DevDay.HasValue ? Array.IndexOf(days, Game.DevDay.Value) + 1 : 0;
            Row(g, new[] { "Авто", "Утро", "День", "Вечер", "Ночь" }, i => { Game.DevDay = i == 0 ? (DayPhase?)null : days[i - 1]; }, di);

            Section(g, "Сеты (выдать и надеть)");
            Row(g, new[] { "Самурай", "Пират", "Космонавт", "Снять всё" }, i =>
            {
                if (i == 3) { foreach (Slot sl in Items.Slots) s.SetEquipped(sl, null); return; }
                foreach (Item it in GearSet.All[i].Pieces()) { s.Owned.Add(it.Id); s.SetEquipped(it.Slot, it.Id); }
            }, -1);
            // personal sets also switch to their character, so the bonus shows
            Row(g, new[] { "Гриль-мастер (Юра)", "Бахча (Влад)", "Пивной фестиваль (Стасян)" }, i =>
            {
                GearSet set = GearSet.All[3 + i];
                foreach (Item it in set.Pieces()) { s.Owned.Add(it.Id); s.SetEquipped(it.Slot, it.Id); }
                MainForm.SetCharacter(set.Owner);
            }, -1);

            Section(g, "Эффекты");
            Row(g, new[] { "+1 лутбокс (эффект)", "Достижение", "Прыжок питомца", "Открыть лутбоксы" }, i =>
            {
                if (i == 0) { s.Boxes++; owner.TestLootFx(); }
                else if (i == 1) owner.TestAchFx();
                else if (i == 2) owner.TestPetHop();
                else owner.OpenLoot();
            }, -1);

            Section(g, "Звуки   (громкость " + (int)Math.Round(s.Volume * 100) + "%)");
            string[] snd = { "bongo_l0", "mech_l0", "soft_l0", "chip_l0", "ev_box", "ev_ach", "ev_item", "ev_rare" };
            Row(g, new[] { "Бонго", "Механика", "Мягкий", "8-бит", "Лутбокс", "Достиж.", "Предмет", "Редкий" }, i => Sound.Preview(snd[i]), -1);

            Button(g, new RectangleF(W / 2 - 160, H - 58, 320, 42), "Выйти из режима разработчика", Ui.Bad, true, true, () =>
            {
                Game.ExitDev();
                Close();
            });
        }
    }
}

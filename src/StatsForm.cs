using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BongoBro
{
    // Totals, key presses per day for two weeks and a heat map of the keyboard.
    // Only the number of presses per key is stored, never what was typed.
    public class StatsForm : CustomForm
    {
        class KeyCap
        {
            public string Label, Ru;
            public int Vk;
            public float Width;
            public KeyCap(string label, string ru, int vk, float width) { Label = label; Ru = ru; Vk = vk; Width = width; }
        }

        static readonly KeyCap[][] Caps = BuildLayout();

        static KeyCap[][] BuildLayout()
        {
            List<KeyCap[]> rows = new List<KeyCap[]>();
            List<KeyCap> r = new List<KeyCap>();
            r.Add(new KeyCap("`", "Ё", 0xC0, 1));
            for (int i = 1; i <= 10; i++) r.Add(new KeyCap((i % 10).ToString(), "", 0x30 + i % 10, 1));
            r.Add(new KeyCap("-", "", 0xBD, 1));
            r.Add(new KeyCap("=", "", 0xBB, 1));
            r.Add(new KeyCap("Backspace", "", 0x08, 2));
            rows.Add(r.ToArray());
            rows.Add(Letters("Tab", 0x09, 1.5f, "QWERTYUIOP", "ЙЦУКЕНГШЩЗ", new[] { "[", "]", "\\" }, new[] { "Х", "Ъ", "" }, new[] { 0xDB, 0xDD, 0xDC }, 1.5f));
            rows.Add(Letters("Caps", 0x14, 1.75f, "ASDFGHJKL", "ФЫВАПРОЛД", new[] { ";", "'", "Enter" }, new[] { "Ж", "Э", "" }, new[] { 0xBA, 0xDE, 0x0D }, 2.25f));
            rows.Add(Letters("Shift", 0xA0, 2.25f, "ZXCVBNM", "ЯЧСМИТЬ", new[] { ",", ".", "/", "Shift" }, new[] { "Б", "Ю", "", "" }, new[] { 0xBC, 0xBE, 0xBF, 0xA1 }, 2.75f));
            rows.Add(new[] {
                new KeyCap("Ctrl", "", 0xA2, 1.5f), new KeyCap("Win", "", 0x5B, 1.25f), new KeyCap("Alt", "", 0xA4, 1.25f),
                new KeyCap("Пробел", "", 0x20, 7f), new KeyCap("Alt", "", 0xA5, 1.25f), new KeyCap("Menu", "", 0x5D, 1.25f), new KeyCap("Ctrl", "", 0xA3, 1.5f) });
            return rows.ToArray();
        }

        // a row that starts with a wide key, then letters, then punctuation; the last extra key gets lastWidth
        static KeyCap[] Letters(string first, int firstVk, float firstW, string en, string ru, string[] extra, string[] extraRu, int[] extraVk, float lastWidth)
        {
            List<KeyCap> r = new List<KeyCap>();
            r.Add(new KeyCap(first, "", firstVk, firstW));
            for (int i = 0; i < en.Length; i++) r.Add(new KeyCap(en[i].ToString(), ru[i].ToString(), en[i], 1));
            for (int i = 0; i < extra.Length; i++) r.Add(new KeyCap(extra[i], extraRu[i], extraVk[i], i == extra.Length - 1 ? lastWidth : 1));
            return r.ToArray();
        }

        public StatsForm() : base(1000, 668, "BongoBro: статистика") { }

        protected override void PaintUi(Graphics g, float t)
        {
            SaveData s = Game.S;
            TitleBar(g, "СТАТИСТИКА");
            Tiles(g, s);
            Chart(g, s);
            Heatmap(g, s);
        }

        void Tiles(Graphics g, SaveData s)
        {
            long best = Math.Max(s.KeysYura, Math.Max(s.KeysVlad, s.KeysStas));
            string fav = best == 0 ? "—" : best == s.KeysStas ? Characters.Name(Characters.Stas) : best == s.KeysVlad ? Characters.Name(Characters.Vlad) : Characters.Name(Characters.Yura);
            string[][] tiles = {
                new[] { "Всего нажатий", Ui.Num(s.Keys) },
                new[] { "Кликов мышью", Ui.Num(s.Clicks) },
                new[] { "Рекорд скорости", s.BestKpm + " / мин" },
                new[] { "Дней подряд", s.Streak.ToString() },
                new[] { "Открыто лутбоксов", s.Opened.ToString() },
                new[] { "Любимый персонаж", fav } };
            float w = (W - 32 - 10 * (tiles.Length - 1)) / (float)tiles.Length;
            for (int i = 0; i < tiles.Length; i++)
            {
                RectangleF r = new RectangleF(16 + i * (w + 10), 60, w, 68);
                Ui.FillRR(g, r, 12, Ui.Panel);
                Ui.TextC(g, tiles[i][0], Ui.F(12, false), Ui.Sub, new RectangleF(r.X, r.Y + 8, r.Width, 20));
                Ui.TextC(g, tiles[i][1], Ui.F(20, true), Color.White, new RectangleF(r.X, r.Y + 30, r.Width, 30));
            }
        }

        void Chart(Graphics g, SaveData s)
        {
            const int days = 14;
            Ui.TextL(g, "Нажатия за 2 недели", Ui.F(15, true), Ui.Fg, 20, 140);
            RectangleF area = new RectangleF(16, 166, W - 32, 176);
            Ui.FillRR(g, area, 12, Ui.Panel);
            long max = 1;
            long[] v = new long[days];
            for (int i = 0; i < days; i++)
            {
                v[i] = Stats.KeysOn(s, DateTime.Today.AddDays(i - days + 1));
                max = Math.Max(max, v[i]);
            }
            float slot = (area.Width - 24) / days, top = area.Y + 26, bottom = area.Bottom - 26;
            for (int i = 0; i < days; i++)
            {
                DateTime d = DateTime.Today.AddDays(i - days + 1);
                float h = (bottom - top) * v[i] / max;
                RectangleF bar = new RectangleF(area.X + 12 + i * slot + slot * 0.18f, bottom - h, slot * 0.64f, Math.Max(h, 2));
                bool today = i == days - 1;
                Ui.FillRR(g, bar, Math.Min(6, bar.Width / 2), v[i] == 0 ? Art.Mix(Ui.Panel, Color.White, 0.08f) : today ? Ui.Gold : Ui.Accent);
                if (v[i] > 0) Ui.TextC(g, Short(v[i]), Ui.F(11, true), today ? Ui.Gold : Ui.Fg, new RectangleF(bar.X - 10, bar.Y - 18, bar.Width + 20, 16));
                Ui.TextC(g, today ? "сегодня" : d.ToString("dd.MM"), Ui.F(10.5f, today), today ? Ui.Gold : Ui.Sub, new RectangleF(bar.X - 14, bottom + 4, bar.Width + 28, 16));
            }
        }

        static string Short(long n)
        {
            if (n >= 10000) return (n / 1000) + "k";
            if (n >= 1000) return (n / 1000.0).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "k";
            return n.ToString();
        }

        void Heatmap(Graphics g, SaveData s)
        {
            long max = 1;
            KeyCap top = null;
            foreach (KeyCap[] row in Caps)
                foreach (KeyCap k in row)
                {
                    long n = Count(s, k.Vk);
                    if (n > max) { max = n; top = k; }
                }
            Ui.TextL(g, "Какие клавиши ты жмёшь чаще всего", Ui.F(15, true), Ui.Fg, 20, 352);
            if (top != null)
                Ui.TextR(g, "Любимая клавиша: " + top.Label + " (" + Ui.Num(max) + ")", Ui.F(13, true), Ui.Gold, new RectangleF(W - 420, 354, 400, 22));
            float unit = (W - 32 - 24) / 15f, kh = 40, gap = 4;
            RectangleF area = new RectangleF(16, 380, W - 32, Caps.Length * (kh + gap) + 20);
            Ui.FillRR(g, area, 12, Ui.Panel);
            KeyCap hovered = null;
            for (int ri = 0; ri < Caps.Length; ri++)
            {
                float x = area.X + 12;
                foreach (KeyCap k in Caps[ri])
                {
                    RectangleF r = new RectangleF(x + gap / 2, area.Y + 10 + ri * (kh + gap), k.Width * unit - gap, kh);
                    x += k.Width * unit;
                    long n = Count(s, k.Vk);
                    float heat = (float)Math.Sqrt(n / (double)max);
                    Color c = n == 0 ? Ui.Panel2 : heat < 0.6f ? Art.Mix(Ui.Panel2, Ui.Accent, heat / 0.6f) : Art.Mix(Ui.Accent, Ui.Gold, (heat - 0.6f) / 0.4f);
                    bool hover = Hover(r);
                    if (hover) hovered = k;
                    Ui.FillRR(g, r, 6, c);
                    if (hover) using (GraphicsPath p = Art.RR(r, 6)) using (Pen pen = new Pen(Color.White, 1.6f)) g.DrawPath(pen, p);
                    Color txt = heat > 0.75f ? Color.FromArgb(40, 24, 0) : Color.White;
                    if (k.Ru.Length > 0)
                    {
                        Ui.TextL(g, k.Label, Ui.F(12, true), txt, r.X + 6, r.Y + 3);
                        Ui.TextR(g, k.Ru, Ui.F(11, false), Art.A(170, txt), new RectangleF(r.X, r.Bottom - 20, r.Width - 6, 18));
                    }
                    else Ui.TextC(g, k.Label, Ui.F(k.Label.Length > 2 ? 11 : 13, true), txt, r);
                }
            }
            string foot = hovered != null
                ? hovered.Label + (hovered.Ru.Length > 0 ? " / " + hovered.Ru : "") + ": " + Ui.Num(Count(s, hovered.Vk)) + " нажатий"
                : "Наведи на клавишу, чтобы увидеть число нажатий. Сохраняется только количество, а не то, что ты набирал.";
            Ui.TextC(g, foot, Ui.F(12, hovered != null), hovered != null ? Ui.Fg : Ui.Sub, new RectangleF(16, area.Bottom + 6, W - 32, 20));
        }

        static long Count(SaveData s, int vk)
        {
            long n;
            return s.KeyCounts.TryGetValue(vk, out n) ? n : 0;
        }
    }
}

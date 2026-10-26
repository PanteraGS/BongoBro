using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BongoBro
{
    public class LootForm : CustomForm
    {
        enum St { Idle, Rolling, Result }

        const int RollMs = 5200;
        const int WinIndex = 46;
        const float Stride = 132f;

        St st = St.Idle;
        readonly List<Item> strip = new List<Item>();
        Item result;
        bool resultNew, rollGold;
        int rollStart, shownAt;
        float target;

        public LootForm() : base(720, 480, "BongoBro: лутбоксы") { }

        protected override void PaintUi(Graphics g, float t)
        {
            int now = Environment.TickCount;
            Art.Glow(g, 360, 250, 340, 200, rollGold && st != St.Idle ? Ui.Gold : Ui.Accent, 34);
            TitleBar(g, "ЛУТБОКСЫ");
            if (st == St.Idle) Idle(g, t);
            else if (st == St.Rolling) Rolling(g, t, now);
            else ShowResult(g, t, now);
        }

        void Idle(Graphics g, float t)
        {
            int boxes = Game.S.Boxes, gold = Game.S.GoldBoxes;
            long left = Game.BoxEvery - Game.S.Keys % Game.BoxEvery;
            if (gold > 0)
            {
                // two boxes side by side
                BoxColumn(g, t, 200, "Обычных: " + boxes, boxes, false, () => StartRoll(false));
                BoxColumn(g, t, 520, "Золотых: " + gold, gold, true, () => StartRoll(true));
                Ui.TextC(g, "Золотой лутбокс: только IMMORTAL", Ui.F(13, true), Ui.Gold, new RectangleF(360, 418, 320, 22));
                Ui.TextC(g, "До следующего: " + Ui.Num(left) + " нажатий", Ui.F(13, false), Ui.Sub, new RectangleF(40, 418, 320, 22));
                return;
            }
            float bob = boxes > 0 ? Art.Sin(t * 2.6f) * 6 : 0;
            Art.DrawLootBox(g, 360, 190 + bob, 150, t, boxes > 0);
            Ui.TextC(g, boxes > 0 ? "Доступно лутбоксов: " + boxes : "Лутбоксов пока нет", Ui.F(22, true), Ui.Fg, new RectangleF(0, 280, W, 34));
            Ui.TextC(g, "До следующего лутбокса: " + Ui.Num(left) + " нажатий", Ui.F(14, false), Ui.Sub, new RectangleF(0, 314, W, 22));
            Ui.Progress(g, new RectangleF(210, 342, 300, 8), 1f - left / (float)Game.BoxEvery, Ui.Gold);
            Button(g, new RectangleF(250, 366, 220, 50), "ОТКРЫТЬ", Ui.Accent, true, boxes > 0, () => StartRoll(false));
            for (int i = 0; i < 4; i++)
            {
                Rarity r = (Rarity)i;
                float x = 104 + i * 128;
                using (SolidBrush b = new SolidBrush(Items.RarityColor(r))) g.FillEllipse(b, x, 441, 10, 10);
                string pct = (Items.Chances[i] * 100).ToString("0") + "%";
                Ui.TextL(g, Items.RarityName(r) + " " + pct, Ui.F(12, true), Art.Mix(Items.RarityColor(r), Color.White, 0.2f), x + 14, 437);
            }
        }

        void BoxColumn(Graphics g, float t, float cx, string label, int count, bool gold, Action open)
        {
            float bob = count > 0 ? Art.Sin(t * 2.6f + (gold ? 1.5f : 0)) * 6 : 0;
            Art.DrawLootBox(g, cx, 190 + bob, 130, t, count > 0, gold);
            Ui.TextC(g, label, Ui.F(20, true), gold ? Ui.Gold : Ui.Fg, new RectangleF(cx - 150, 284, 300, 32));
            Button(g, new RectangleF(cx - 100, 340, 200, 50), gold ? "ОТКРЫТЬ ЗОЛОТОЙ" : "ОТКРЫТЬ", gold ? Ui.Gold : Ui.Accent, true, count > 0, open);
        }

        void StartRoll(bool gold)
        {
            if (st == St.Rolling) return;
            if (gold ? Game.S.GoldBoxes <= 0 : Game.S.Boxes <= 0) return;
            rollGold = gold;
            result = gold ? Game.OpenGoldBox(out resultNew) : Game.OpenBox(out resultNew);
            strip.Clear();
            List<Item> imm = Items.OfRarity(Rarity.Immortal);
            for (int i = 0; i < WinIndex + 6; i++)
            {
                if (i == WinIndex) strip.Add(result);
                else strip.Add(gold ? imm[Game.Rnd.Next(imm.Count)] : Items.Roll(Game.Rnd));
            }
            // tease: a shiny immortal right next to the winner
            if (!gold && result.Rarity != Rarity.Immortal && Game.Rnd.Next(3) == 0)
                strip[WinIndex + (Game.Rnd.Next(2) == 0 ? 1 : -1)] = imm[Game.Rnd.Next(imm.Count)];
            float jitter = (float)(Game.Rnd.NextDouble() * 2 - 1) * 48;
            target = (WinIndex - 3) * Stride + jitter;
            rollStart = Environment.TickCount;
            st = St.Rolling;
        }

        void Rolling(Graphics g, float t, int now)
        {
            float p = Math.Min(1f, (now - rollStart) / (float)RollMs);
            float e = 1 - (float)Math.Pow(1 - p, 4);
            float off = target * e;
            RectangleF area = new RectangleF(16, 110, W - 32, 210);
            Ui.FillRR(g, area, 14, Ui.Panel);
            GraphicsState gs = g.Save();
            g.SetClip(area, CombineMode.Intersect);
            for (int i = 0; i < strip.Count; i++)
            {
                float cx = 360 + (i - 3) * Stride - off;
                if (cx < -80 || cx > W + 80) continue;
                Ui.ItemCard(g, new RectangleF(cx - 60, 135, 120, 160), strip[i], t, false, 0, false);
            }
            using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(15, 110, 141, 210), Ui.Panel, Art.A(0, Ui.Panel), 0f)) g.FillRectangle(b, 16, 110, 140, 210);
            using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(W - 157, 110, 141, 210), Art.A(0, Ui.Panel), Ui.Panel, 0f)) g.FillRectangle(b, W - 156, 110, 140, 210);
            g.Restore(gs);
            using (Pen pen = new Pen(Ui.Gold, 3f)) g.DrawLine(pen, 360, 118, 360, 312);
            using (SolidBrush b = new SolidBrush(Ui.Gold))
            {
                g.FillPolygon(b, new PointF[] { new PointF(349, 110), new PointF(371, 110), new PointF(360, 124) });
                g.FillPolygon(b, new PointF[] { new PointF(349, 320), new PointF(371, 320), new PointF(360, 306) });
            }
            string dots = new string('.', 1 + (int)(t * 3) % 3);
            Ui.TextC(g, (rollGold ? "Открываем золотой" : "Открываем") + dots, Ui.F(20, true), rollGold ? Ui.Gold : Ui.Fg, new RectangleF(0, 340, W, 34));
            Ui.TextC(g, rollGold ? "Только IMMORTAL" : "Common 62%  •  Rare 26%  •  Legendary 10%  •  Immortal 2%", Ui.F(13, false), Ui.Sub, new RectangleF(0, 380, W, 24));
            if (now - rollStart > RollMs + 350)
            {
                st = St.Result;
                shownAt = now;
            }
        }

        static float EaseOutBack(float k)
        {
            const float c1 = 1.70158f, c3 = c1 + 1;
            float x = k - 1;
            return 1 + c3 * x * x * x + c1 * x * x;
        }

        void ShowResult(Graphics g, float t, int now)
        {
            float k = Math.Min(1f, (now - shownAt) / 380f);
            float sc = EaseOutBack(k);
            Color rc = Items.RarityColor(result.Rarity);
            if (result.Rarity >= Rarity.Rare)
            {
                int ra = result.Rarity == Rarity.Immortal ? 60 : result.Rarity == Rarity.Legendary ? 45 : 26;
                using (SolidBrush rb = new SolidBrush(Art.A(ra, rc)))
                    for (int i = 0; i < 14; i++)
                    {
                        double a = (t * 22 + i * 360.0 / 14) * Math.PI / 180;
                        g.FillPolygon(rb, new PointF[] {
                            new PointF(360, 220),
                            new PointF(360 + Art.Cos(a - 0.07) * 420, 220 + Art.Sin(a - 0.07) * 420),
                            new PointF(360 + Art.Cos(a + 0.07) * 420, 220 + Art.Sin(a + 0.07) * 420) });
                    }
                TitleBar(g, "ЛУТБОКСЫ");
            }
            Art.Glow(g, 360, 220, 210, 170, rc, 100);
            GraphicsState gs = g.Save();
            g.TranslateTransform(360, 220);
            g.ScaleTransform(sc, sc);
            g.TranslateTransform(-360, -220);
            Ui.ItemCard(g, new RectangleF(250, 72, 220, 292), result, t, false, 0, false);
            g.Restore(gs);
            string msg = resultNew ? "НОВЫЙ ПРЕДМЕТ!  " + Items.SlotName(result.Slot)
                : rollGold ? "Все immortal уже собраны!" : "Коллекция уже собрана целиком!";
            Ui.TextC(g, msg, Ui.F(21, true), resultNew ? Art.Mix(rc, Color.White, 0.2f) : Ui.Gold, new RectangleF(0, 370, W, 32));
            if (k < 1) return;
            bool equipped = Game.S.GetEquipped(result.Slot) == result.Id;
            Item it = result;
            bool more = rollGold ? Game.S.GoldBoxes > 0 : Game.S.Boxes > 0;
            Button(g, new RectangleF(140, 414, 140, 44), equipped ? "Надето ✓" : "Надеть", Ui.Good, true, !equipped, () => Game.Equip(it));
            Button(g, new RectangleF(290, 414, 140, 44), "Ещё (" + (rollGold ? Game.S.GoldBoxes : Game.S.Boxes) + ")", rollGold ? Ui.Gold : Ui.Accent, true, more, () => StartRoll(rollGold));
            Button(g, new RectangleF(440, 414, 140, 44), "Готово", Ui.Sub, false, true, () => { st = St.Idle; });
        }
    }
}

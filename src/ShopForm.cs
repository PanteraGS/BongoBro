using System;
using System.Collections.Generic;
using System.Drawing;

namespace BongoBro
{
    // Wardrobe: every item comes only from loot boxes.
    public class ShopForm : CustomForm
    {
        Slot tab = Slot.Hat;

        public ShopForm() : base(1160, 640, "BongoBro: гардероб") { }

        protected override void PaintUi(Graphics g, float t)
        {
            SaveData s = Game.S;
            TitleBar(g, "ГАРДЕРОБ");
            Ui.TextR(g, "Коллекция: " + s.Owned.Count + " / " + Items.All.Count, Ui.F(15, true), Color.FromArgb(255, 220, 120), new RectangleF(W - 330, 10, 270, 30));

            // left: character preview
            Ui.FillRR(g, new RectangleF(16, 62, 262, 562), 14, Ui.Panel);
            Art.Glow(g, 147, 196, 130, 120, Ui.Accent, 70);
            Pose p = Pose.FromSave(s, t);
            int ph = (int)(t * 4) % 4;
            p.LeftDown = ph == 0;
            p.RightDown = ph == 2;
            p.LitKey = ph == 0 ? 14 : ph == 2 ? 21 : -1;
            p.Blink = t % 4f < 0.13f;
            Art.DrawCharacterIn(g, new RectangleF(22, 66, 250, 250), p);

            float y = 324;
            foreach (Slot sl in Items.Slots)
            {
                Item it = Items.Get(s.GetEquipped(sl));
                Ui.TextL(g, Items.SlotLabel(sl), Ui.F(13, false), Ui.Sub, 32, y + 4);
                Ui.TextR(g, it != null ? it.Name : "—", Ui.F(13, true), it != null ? Items.RarityColor(it.Rarity) : Ui.Sub, new RectangleF(104, y, 162, 26));
                y += 28;
            }
            Ui.TextL(g, "Лутбоксов: " + s.Boxes + "   Открыто: " + s.Opened, Ui.F(13, false), Ui.Fg, 32, 470);
            long left = Game.BoxEvery - s.Keys % Game.BoxEvery;
            Ui.Progress(g, new RectangleF(32, 494, 230, 6), 1f - left / (float)Game.BoxEvery, Ui.Gold);
            Button(g, new RectangleF(30, 514, 234, 44), s.Boxes + s.GoldBoxes > 0 ? "Открыть лутбоксы (" + (s.Boxes + s.GoldBoxes) + ")" : "Лутбоксы", Ui.Accent, true, true, () => Program.MainWin.OpenLoot());
            bool anyEq = false;
            foreach (Slot sl in Items.Slots) if (s.GetEquipped(sl) != null) anyEq = true;
            Button(g, new RectangleF(30, 570, 234, 38), "Снять всё", Ui.Sub, false, anyEq, () =>
            {
                foreach (Slot sl in Items.Slots) Game.S.SetEquipped(sl, null);
                Game.Save();
            });

            // tabs
            for (int i = 0; i < Items.Slots.Length; i++)
            {
                Slot sl = Items.Slots[i];
                List<Item> all = Items.OfSlot(sl);
                int owned = 0;
                foreach (Item it in all) if (s.Owned.Contains(it.Id)) owned++;
                RectangleF r = new RectangleF(292 + i * 172.8f, 62, 164.8f, 38);
                Button(g, r, Items.SlotName(sl) + "  " + owned + "/" + all.Count, Ui.Accent, sl == tab, true, () => { tab = sl; });
            }

            // grid 7 x 2
            List<Item> list = Items.OfSlot(tab);
            for (int i = 0; i < list.Count; i++)
            {
                Item it = list[i];
                RectangleF r = new RectangleF(292 + (i % 7) * 123.8f, 112 + (i / 7) * 242, 113, 232);
                bool owned = s.Owned.Contains(it.Id);
                bool eq = s.GetEquipped(it.Slot) == it.Id;
                Ui.ItemCard(g, r, it, t + i * 0.37f, !owned, 40, Hover(r));
                RectangleF b = new RectangleF(r.X + 8, r.Bottom - 36, r.Width - 16, 28);
                if (eq) Button(g, b, "Надето ✓", Ui.Good, true, true, () => Game.Unequip(it.Slot));
                else if (owned) Button(g, b, "Надеть", Ui.Accent, false, true, () => Game.Equip(it));
                else Button(g, b, "Из лутбокса", Ui.Sub, false, false, null);
            }
            Ui.TextC(g, "Каждые 2000 нажатий клавиш = лутбокс  •  новые вещи выпадают без повторов, пока коллекция не собрана", Ui.F(12, false), Ui.Sub, new RectangleF(292, 600, 856, 24));
        }
    }
}

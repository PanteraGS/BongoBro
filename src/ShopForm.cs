using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BongoBro
{
    // Wardrobe: every item comes only from loot boxes.
    public class ShopForm : CustomForm
    {
        // index into Items.Slots, or SetsTab for the gear sets page
        const int SetsTab = -1;
        int tab = 0;
        bool personal;   // sets page: personal sets instead of the common ones

        public void ShowSetsTab() { tab = SetsTab; }
        public void ShowPersonalSets() { tab = SetsTab; personal = true; }

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
            p.PetHop = ph == 0 ? 1 - (t * 4 % 1) : 0;
            p.Blink = t % 4f < 0.13f;
            Art.DrawCharacterIn(g, new RectangleF(22, 98, 250, 222), p);
            for (int i = 0; i < Characters.Ids.Length; i++)
            {
                string id = Characters.Ids[i];
                float bw = (238 - 6 * (Characters.Ids.Length - 1)) / (float)Characters.Ids.Length;
                RectangleF r = new RectangleF(28 + i * (bw + 6), 70, bw, 30);
                Button(g, r, Characters.Name(id), Ui.Accent, s.Character == id, true, () => MainForm.SetCharacter(id));
            }

            float y = 324;
            foreach (Slot sl in Items.Slots)
            {
                Item it = Items.Get(s.GetEquipped(sl));
                Ui.TextL(g, Items.SlotLabel(sl), Ui.F(13, false), Ui.Sub, 32, y + 3);
                Ui.TextR(g, it != null ? it.Name : "—", Ui.F(13, true), it != null ? Items.RarityColor(it.Rarity) : Ui.Sub, new RectangleF(104, y, 162, 24));
                y += 24;
            }
            Ui.TextL(g, "Лутбоксов: " + (s.Boxes + s.GoldBoxes) + "   Открыто: " + s.Opened, Ui.F(13, false), Ui.Fg, 32, 474);
            long left = Game.BoxEvery - s.Keys % Game.BoxEvery;
            Ui.Progress(g, new RectangleF(32, 498, 230, 6), 1f - left / (float)Game.BoxEvery, Ui.Gold);
            Button(g, new RectangleF(30, 514, 234, 44), s.Boxes + s.GoldBoxes > 0 ? "Открыть лутбоксы (" + (s.Boxes + s.GoldBoxes) + ")" : "Лутбоксы", Ui.Accent, true, true, () => Program.MainWin.OpenLoot());
            bool anyEq = false;
            foreach (Slot sl in Items.Slots) if (s.GetEquipped(sl) != null) anyEq = true;
            Button(g, new RectangleF(30, 570, 234, 38), "Снять всё", Ui.Sub, false, anyEq, () =>
            {
                foreach (Slot sl in Items.Slots) Game.S.SetEquipped(sl, null);
                Game.Save();
            });

            // tabs: one per slot plus the gear sets
            int tabs = Items.Slots.Length + 1;
            float tw = (856 - 8 * (tabs - 1)) / (float)tabs;
            for (int i = 0; i < tabs; i++)
            {
                int idx = i < Items.Slots.Length ? i : SetsTab;
                string label;
                if (idx == SetsTab) label = "Сеты";
                else
                {
                    List<Item> all = Items.OfSlotBase(Items.Slots[i]);
                    int owned = 0;
                    foreach (Item it in all) if (s.Owned.Contains(it.Id)) owned++;
                    label = Items.SlotName(Items.Slots[i]) + " " + owned + "/" + all.Count;
                }
                Button(g, new RectangleF(292 + i * (tw + 8), 62, tw, 38), label, idx == SetsTab ? Ui.Gold : Ui.Accent, tab == idx, true, () => { tab = idx; });
            }

            if (tab == SetsTab) PaintSets(g, t, s);
            else PaintGrid(g, t, s, Items.OfSlotBase(Items.Slots[tab]));
            Ui.TextC(g, "Каждые 2000 нажатий клавиш = лутбокс  •  новые вещи выпадают без повторов, пока коллекция не собрана", Ui.F(12, false), Ui.Sub, new RectangleF(292, 600, 856, 24));
        }

        void PaintGrid(Graphics g, float t, SaveData s, List<Item> list)
        {
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
        }

        void PaintSets(Graphics g, float t, SaveData s)
        {
            // common sets and personal (one per character) sets on two sub-pages
            Button(g, new RectangleF(292, 110, 200, 30), "Общие сеты", Ui.Gold, !personal, true, () => { personal = false; });
            Button(g, new RectangleF(500, 110, 200, 30), "Личные сеты", Ui.Gold, personal, true, () => { personal = true; });
            List<GearSet> shown = new List<GearSet>();
            foreach (GearSet gs in GearSet.All) if ((gs.Owner != null) == personal) shown.Add(gs);
            for (int i = 0; i < shown.Count; i++)
            {
                GearSet set = shown[i];
                List<Item> pieces = set.Pieces();
                RectangleF row = new RectangleF(292, 148 + i * 150, 856, 144);
                Color rc = Items.RarityColor(set.Rarity);
                bool worn = set.IsWorn(s.GetEquipped);
                bool mine = set.BonusFor(s.Character);
                int have = set.OwnedCount(s);
                using (GraphicsPath p = Art.RR(row, 14))
                {
                    if (worn) Art.GlowPath(g, p, Ui.Gold, 0.5f + 0.2f * Art.Sin(t * 3 + i));
                    using (SolidBrush b = new SolidBrush(Art.Mix(Ui.Panel, rc, 0.12f))) g.FillPath(b, p);
                    using (Pen pen = new Pen(worn ? Ui.Gold : Art.A(160, rc), 1.5f)) g.DrawPath(pen, p);
                }
                Ui.TextL(g, set.Name, Ui.F(22, true), Color.White, row.X + 18, row.Y + 8);
                string kind = set.Owner != null ? "ЛИЧНЫЙ СЕТ: " + Characters.Name(set.Owner).ToUpper() : "СЕТ";
                Ui.TextL(g, kind + "  •  " + Items.RarityName(set.Rarity), Ui.F(12, true), rc, row.X + 20, row.Y + 40);
                using (StringFormat sf = new StringFormat())
                using (SolidBrush b = new SolidBrush(Ui.Sub))
                    g.DrawString("Бонус: " + set.Bonus, Ui.F(13, false), b, new RectangleF(row.X + 20, row.Y + 58, 300, 40), sf);
                Ui.Progress(g, new RectangleF(row.X + 20, row.Y + 100, 180, 6), have / (float)pieces.Count, worn ? Ui.Good : rc);
                Ui.TextL(g, "Собрано " + have + "/" + pieces.Count, Ui.F(12, true), Ui.Fg, row.X + 208, row.Y + 93);
                RectangleF btn = new RectangleF(row.X + 20, row.Y + 112, 240, 28);
                if (worn && mine) Button(g, btn, "Бонус активен ✓", Ui.Good, true, true, () => { foreach (Item it in pieces) Game.Unequip(it.Slot); });
                else if (worn) Button(g, btn, "Бонус только у " + Characters.NameWho(set.Owner), Ui.Sub, true, true, () => MainForm.SetCharacter(set.Owner));
                else Button(g, btn, have == pieces.Count ? "Надеть сет" : "Собери все 4 вещи", Ui.Gold, true, have == pieces.Count, () => { foreach (Item it in pieces) Game.Equip(it); });

                for (int k = 0; k < pieces.Count; k++)
                {
                    Item it = pieces[k];
                    RectangleF r = new RectangleF(row.X + 336 + k * 130, row.Y + 2, 120, 140);
                    bool owned = s.Owned.Contains(it.Id);
                    bool eq = s.GetEquipped(it.Slot) == it.Id;
                    Ui.ItemCard(g, r, it, t + k * 0.4f, !owned, 0, Hover(r) && owned);
                    if (eq) Ui.TextC(g, "✓", Ui.F(18, true), Ui.Good, new RectangleF(r.X + 4, r.Y + 6, 24, 24));
                    if (owned) AddHit(r, () => { if (eq) Game.Unequip(it.Slot); else Game.Equip(it); });
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BongoBro
{
    // Wardrobe: every item comes only from loot boxes.
    public class ShopForm : CustomForm
    {
        // index into Items.Slots, or SetsTab / NewYearTab / ExchangeTab for the extra pages
        const int SetsTab = -1, NewYearTab = -2, ExchangeTab = -3;
        int tab = 0;
        bool personal;   // sets page: personal sets instead of the common ones
        int exchangePage, gridPage;
        static readonly Color ShardColor = Color.FromArgb(150, 225, 255);

        public void ShowSetsTab() { tab = SetsTab; }
        public void ShowPersonalSets() { tab = SetsTab; personal = true; }
        public void ShowNewYearTab() { tab = NewYearTab; }
        public void ShowExchangeTab() { tab = ExchangeTab; }
        public void ShowSlotTab(Slot sl) { tab = Array.IndexOf(Items.Slots, sl); }

        public ShopForm() : base(1160, 640, "BongoBro: гардероб") { }

        protected override void PaintUi(Graphics g, float t)
        {
            SaveData s = Game.S;
            TitleBar(g, "ГАРДЕРОБ");
            Ui.TextR(g, "Коллекция: " + s.Owned.Count + " / " + Items.All.Count + "   Осколки: " + s.Shards + " ◆", Ui.F(15, true), Color.FromArgb(255, 220, 120), new RectangleF(W - 480, 10, 420, 30));

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
            Ui.TextL(g, "Лутбоксов: " + Game.TotalBoxes + "   Открыто: " + s.Opened, Ui.F(13, false), Ui.Fg, 32, 474);
            long left = Game.BoxEvery - s.Keys % Game.BoxEvery;
            Ui.Progress(g, new RectangleF(32, 498, 230, 6), 1f - left / (float)Game.BoxEvery, Ui.Gold);
            Button(g, new RectangleF(30, 514, 234, 44), Game.TotalBoxes > 0 ? "Открыть лутбоксы (" + Game.TotalBoxes + ")" : "Лутбоксы", Ui.Accent, true, true, () => Program.MainWin.OpenLoot());
            bool anyEq = false;
            foreach (Slot sl in Items.Slots) if (s.GetEquipped(sl) != null) anyEq = true;
            Button(g, new RectangleF(30, 570, 234, 38), "Снять всё", Ui.Sub, false, anyEq, () =>
            {
                foreach (Slot sl in Items.Slots) Game.S.SetEquipped(sl, null);
                Game.Save();
            });

            // tabs: one per slot, the gear sets, the New Year collection and the shard exchange
            int tabs = Items.Slots.Length + 3;
            float tw = (856 - 8 * (tabs - 1)) / (float)tabs;
            for (int i = 0; i < tabs; i++)
            {
                int idx = i < Items.Slots.Length ? i : i == Items.Slots.Length ? SetsTab : i == Items.Slots.Length + 1 ? NewYearTab : ExchangeTab;
                string label;
                if (idx == SetsTab) label = "Сеты";
                else if (idx == ExchangeTab) label = "Обмен";
                else if (idx == NewYearTab)
                {
                    int owned = 0;
                    foreach (Item it in Items.NewYearItems()) if (s.Owned.Contains(it.Id)) owned++;
                    label = "Новый год " + owned + "/" + Items.NewYearItems().Count;
                }
                else
                {
                    List<Item> all = Items.OfSlotBase(Items.Slots[i]);
                    int owned = 0;
                    foreach (Item it in all) if (s.Owned.Contains(it.Id)) owned++;
                    label = Items.SlotName(Items.Slots[i]) + " " + owned + "/" + all.Count;
                }
                Color tc = idx == SetsTab ? Ui.Gold : idx == NewYearTab ? Color.FromArgb(236, 64, 70) : idx == ExchangeTab ? ShardColor : Ui.Accent;
                Button(g, new RectangleF(292 + i * (tw + 8), 62, tw, 38), label, tc, tab == idx, true, () => { tab = idx; gridPage = 0; });
            }

            if (tab == SetsTab) PaintSets(g, t, s);
            else if (tab == NewYearTab) PaintGrid(g, t, s, Items.NewYearItems());
            else if (tab == ExchangeTab) PaintExchange(g, t, s);
            else PaintGrid(g, t, s, Items.OfSlotBase(Items.Slots[tab]));
            string foot = tab == NewYearTab
                ? "Новогодние вещи выпадают только из новогодних лутбоксов " + NewYear.Period + (NewYear.Active ? "  •  событие идёт сейчас!" : "")
                : tab == ExchangeTab
                ? "Повторы из лутбоксов превращаются в осколки — здесь они обмениваются на любую недостающую вещь"
                : "Каждые 2000 нажатий клавиш = лутбокс  •  новые вещи выпадают без повторов, пока коллекция не собрана  •  повторы дают осколки";
            Ui.TextC(g, foot, Ui.F(12, false), tab == NewYearTab ? Color.FromArgb(255, 160, 160) : tab == ExchangeTab ? ShardColor : Ui.Sub, new RectangleF(292, 600, 856, 24));
        }

        // every item the player does not own yet (except the New Year collection), cheapest first
        void PaintExchange(Graphics g, float t, SaveData s)
        {
            List<Item> missing = new List<Item>();
            foreach (Item it in Items.All) if (!it.NewYear && !s.Owned.Contains(it.Id)) missing.Add(it);
            missing.Sort(delegate (Item a, Item b) { return a.Rarity != b.Rarity ? a.Rarity.CompareTo(b.Rarity) : a.Slot.CompareTo(b.Slot); });
            Ui.TextL(g, "Осколки: " + s.Shards + " ◆", Ui.F(17, true), ShardColor, 296, 110);
            if (missing.Count == 0)
            {
                Ui.TextC(g, "Коллекция собрана целиком — обменивать нечего!", Ui.F(17, true), Ui.Fg, new RectangleF(292, 300, 856, 30));
                Ui.TextC(g, "Дубликаты всё равно приносят осколки: пригодятся, когда выйдут новые вещи.", Ui.F(13, false), Ui.Sub, new RectangleF(292, 334, 856, 24));
                return;
            }
            const int perPage = 14;
            int pages = (missing.Count + perPage - 1) / perPage;
            if (exchangePage >= pages) exchangePage = pages - 1;
            if (exchangePage < 0) exchangePage = 0;
            if (pages > 1)
            {
                Button(g, new RectangleF(W - 212, 106, 30, 28), "◀", Ui.Sub, false, exchangePage > 0, () => exchangePage--);
                Ui.TextC(g, exchangePage + 1 + "/" + pages, Ui.F(13, true), Ui.Fg, new RectangleF(W - 180, 106, 54, 28));
                Button(g, new RectangleF(W - 124, 106, 30, 28), "▶", Ui.Sub, false, exchangePage < pages - 1, () => exchangePage++);
            }
            int shown = Math.Min(missing.Count - exchangePage * perPage, perPage);
            for (int k = 0; k < shown; k++)
            {
                Item it = missing[exchangePage * perPage + k];
                RectangleF r = new RectangleF(292 + (k % 7) * 123.8f, 144 + (k / 7) * 228, 113, 222);
                Ui.ItemCard(g, r, it, t + k * 0.37f, false, 40, Hover(r));
                int price = Items.ShardPrice(it.Rarity);
                bool afford = s.Shards >= price;
                Button(g, new RectangleF(r.X + 8, r.Bottom - 36, r.Width - 16, 28), price + " ◆", afford ? ShardColor : Ui.Sub, afford, afford, () => Game.BuyWithShards(it));
            }
        }

        void PaintGrid(Graphics g, float t, SaveData s, List<Item> list)
        {
            const int perPage = 14;
            int pages = (list.Count + perPage - 1) / perPage;
            if (gridPage >= pages) gridPage = pages - 1;
            if (gridPage < 0) gridPage = 0;
            int shown = Math.Min(list.Count - gridPage * perPage, perPage);
            for (int k = 0; k < shown; k++)
            {
                Item it = list[gridPage * perPage + k];
                RectangleF r = new RectangleF(292 + (k % 7) * 123.8f, 112 + (k / 7) * 242, 113, 232);
                bool owned = s.Owned.Contains(it.Id);
                bool eq = s.GetEquipped(it.Slot) == it.Id;
                Ui.ItemCard(g, r, it, t + k * 0.37f, !owned, 40, Hover(r));
                RectangleF b = new RectangleF(r.X + 8, r.Bottom - 36, r.Width - 16, 28);
                if (eq) Button(g, b, "Надето ✓", Ui.Good, true, true, () => Game.Unequip(it.Slot));
                else if (owned) Button(g, b, "Надеть", Ui.Accent, false, true, () => Game.Equip(it));
                else Button(g, b, it.NewYear ? "Новогодний" : "Из лутбокса", Ui.Sub, false, false, null);
                if (it.Slot == Slot.Pet && owned) PetBadge(g, r, s, it.Id);
            }
            if (pages > 1)
            {
                Button(g, new RectangleF(W - 196, 594, 30, 28), "◀", Ui.Sub, false, gridPage > 0, () => gridPage--);
                Ui.TextC(g, gridPage + 1 + "/" + pages, Ui.F(13, true), Ui.Fg, new RectangleF(W - 164, 594, 54, 28));
                Button(g, new RectangleF(W - 108, 594, 30, 28), "▶", Ui.Sub, false, gridPage < pages - 1, () => gridPage++);
            }
        }

        // pet level and experience towards the next one
        static void PetBadge(Graphics g, RectangleF r, SaveData s, string pet)
        {
            long xp = PetLevels.Xp(s, pet);
            int lvl = PetLevels.Level(xp);
            Color c = lvl >= 3 ? Ui.Gold : lvl == 2 ? Color.FromArgb(120, 200, 255) : Ui.Sub;
            string label = lvl + " · " + PetLevels.Name(lvl);
            float tw = g.MeasureString(label, Ui.F(10.5f, true)).Width + 10;
            RectangleF tag = new RectangleF(r.X + 6, r.Y + 6, tw, 20);
            Ui.FillRR(g, tag, 6, Art.A(200, Art.Mix(Ui.Bg, c, 0.3f)));
            Ui.TextC(g, label, Ui.F(10.5f, true), c, tag);
            XpPill(g, new RectangleF(r.X + 8, r.Bottom - 55, r.Width - 16, 15), xp, c);
        }

        // experience bar with "have / need" written inside, "Максимум" at the top stage
        static void XpPill(Graphics g, RectangleF r, long xp, Color c)
        {
            float k = PetLevels.Progress(xp);
            Ui.FillRR(g, r, r.Height / 2, Art.A(170, Ui.Bg));
            if (k > 0)
            {
                RectangleF fill = new RectangleF(r.X, r.Y, Math.Max(r.Height, r.Width * k), r.Height);
                Ui.FillRR(g, fill, r.Height / 2, Art.A(150, c));
            }
            string txt = PetLevels.Level(xp) >= 3 ? "Максимум" : Ui.Num(xp) + " / " + Ui.Num(xp + PetLevels.ToNext(xp));
            Ui.TextC(g, txt, Ui.F(8.5f, true), Color.White, new RectangleF(r.X, r.Y, r.Width, r.Height + 1));
        }

        // compact version for the small set cards: level tag in the corner and a thin bar along the bottom
        static void PetBadgeSmall(Graphics g, RectangleF r, SaveData s, string pet)
        {
            long xp = PetLevels.Xp(s, pet);
            int lvl = PetLevels.Level(xp);
            Color c = lvl >= 3 ? Ui.Gold : lvl == 2 ? Color.FromArgb(120, 200, 255) : Ui.Sub;
            RectangleF tag = new RectangleF(r.Right - 50, r.Y + 8, 44, 18);
            Ui.FillRR(g, tag, 6, Art.A(200, Art.Mix(Ui.Bg, c, 0.3f)));
            Ui.TextC(g, "Ур. " + lvl, Ui.F(10.5f, true), c, tag);
            XpPill(g, new RectangleF(r.X + 10, r.Y + 72, r.Width - 20, 14), xp, c);
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
                    if (it.Slot == Slot.Pet && owned) PetBadgeSmall(g, r, s, it.Id);
                    if (owned) AddHit(r, () => { if (eq) Game.Unequip(it.Slot); else Game.Equip(it); });
                }
            }
        }
    }
}

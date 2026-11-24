using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace BongoBro
{
    public enum Rarity { Common = 0, Rare = 1, Legendary = 2, Immortal = 3 }
    public enum Slot { Hat = 0, Glasses = 1, Gloves = 2, Face = 3, Theme = 4, Pet = 5 }

    public class Item
    {
        public string Id;
        public string Name;
        public Slot Slot;
        public Rarity Rarity;
        public string Set;   // id of the gear set this item belongs to, or null
        public bool NewYear; // drops only from New Year boxes
        public Item(string id, string name, Slot slot, Rarity rarity) { Id = id; Name = name; Slot = slot; Rarity = rarity; }
    }

    public static class Items
    {
        public static readonly List<Item> All = new List<Item>();
        public static readonly double[] Chances = { 0.62, 0.26, 0.10, 0.02 };
        public static readonly Slot[] Slots = { Slot.Hat, Slot.Glasses, Slot.Gloves, Slot.Face, Slot.Theme, Slot.Pet };

        // A duplicate from a loot box turns into shards; shards buy specific items on the exchange tab.
        public static int ShardValue(Rarity r)
        {
            switch (r)
            {
                case Rarity.Rare: return 25;
                case Rarity.Legendary: return 60;
                case Rarity.Immortal: return 150;
                default: return 10;
            }
        }

        public static int ShardPrice(Rarity r)
        {
            switch (r)
            {
                case Rarity.Rare: return 300;
                case Rarity.Legendary: return 800;
                case Rarity.Immortal: return 2000;
                default: return 100;
            }
        }

        static Items()
        {
            Group(Slot.Hat,
                new[] { "hat_beanie", "Серая бини", "hat_cap", "Красная кепка", "hat_bucket", "Панама", "hat_headband", "Спортивная повязка", "hat_flower", "Цветочек", "hat_bandana", "Бандана" },
                new[] { "hat_pompom", "Шапка с помпоном", "hat_ushanka", "Ушанка", "hat_catears", "Кошачьи ушки", "hat_cowboy", "Ковбойская шляпа", "hat_headphones", "Наушники" },
                new[] { "hat_tophat", "Цилиндр джентльмена", "hat_crown", "Золотая корона", "hat_viking", "Шлем викинга", "hat_wizard", "Шляпа волшебника" },
                new[] { "hat_firecrown", "Корона Инферно", "hat_halo", "Божественный нимб", "hat_astral", "Астральная корона" });
            Group(Slot.Glasses,
                new[] { "gl_round", "Круглые очки", "gl_square", "Очки хипстера", "gl_hearts", "Очки-сердечки", "gl_cateye", "Кошачий глаз", "gl_beach", "Пляжные очки", "gl_sport", "Спортивные очки" },
                new[] { "gl_aviator", "Авиаторы", "gl_3d", "3D-очки", "gl_ski", "Лыжная маска", "gl_shutter", "Неоновые шторки", "gl_star", "Очки-звёзды" },
                new[] { "gl_pixel", "Deal With It", "gl_monocle", "Монокль", "gl_steampunk", "Стимпанк-гогглы", "gl_vr", "VR-шлем" },
                new[] { "gl_laser", "Лазерный визор", "gl_galaxy", "Очки Галактики", "gl_prism", "Радужный визор" });
            Group(Slot.Gloves,
                new[] { "gv_knit", "Вязаные перчатки", "gv_garden", "Садовые перчатки", "gv_rubber", "Резиновые перчатки", "gv_medical", "Медицинские перчатки", "gv_striped", "Полосатые перчатки", "gv_work", "Рабочие перчатки" },
                new[] { "gv_leather", "Кожаные перчатки", "gv_fingerless", "Митенки", "gv_boxing", "Боксёрские перчатки", "gv_paw", "Кошачьи лапки", "gv_ninja", "Обмотки ниндзя" },
                new[] { "gv_mickey", "Мультяшные перчатки", "gv_gold", "Золотые перчатки", "gv_knight", "Рыцарские латы", "gv_diamond", "Алмазные перчатки" },
                new[] { "gv_infinity", "Перчатка Бесконечности", "gv_plasma", "Плазменные перчатки", "gv_stardust", "Перчатки звёздной пыли" });
            Group(Slot.Face,
                new[] { "fc_blush", "Румянец", "fc_freckles", "Веснушки", "fc_wink", "Подмигивание", "fc_grin", "Улыбка до ушей", "fc_tongue", "Дразнилка", "fc_bandaid", "Пластырь" },
                new[] { "fc_mustache", "Усы джентльмена", "fc_cat", "Кошачий грим", "fc_hearts", "Влюблённый взгляд", "fc_warpaint", "Боевая раскраска", "fc_unibrow", "Монобровь" },
                new[] { "fc_clown", "Клоун", "fc_skull", "Сахарный череп", "fc_stars", "Звёздный взгляд", "fc_hypno", "Гипноз" },
                new[] { "fc_fireeyes", "Пламенный взгляд", "fc_neon", "Неоновый грим", "fc_void", "Взгляд Бездны" });
            Group(Slot.Theme,
                new[] { "th_snow", "Белоснежка", "th_mint", "Мятный", "th_peach", "Персик", "th_graphite", "Графит", "th_lavender", "Лаванда", "th_choco", "Шоколад" },
                new[] { "th_retro", "Ретро", "th_sakura", "Сакура", "th_ocean", "Океан", "th_matcha", "Матча", "th_lime", "Кислотный лайм" },
                new[] { "th_marble", "Золото и мрамор", "th_cyber", "Киберпанк", "th_candy", "Конфетка", "th_sunset", "Закат" },
                new[] { "th_rgb", "RGB Геймер", "th_cosmos", "Космос", "th_aurora", "Северное сияние" });
            Group(Slot.Pet,
                new[] { "pet_cat", "Котик", "pet_hamster", "Хомяк", "pet_duck", "Утёнок", "pet_frog", "Лягушонок", "pet_penguin", "Пингвинёнок" },
                new[] { "pet_fox", "Лисёнок", "pet_panda", "Панда", "pet_owl", "Совёнок", "pet_axolotl", "Аксолотль", "pet_turtle", "Черепашка" },
                new[] { "pet_corgi", "Корги", "pet_melon", "Мини-арбуз", "pet_robot", "Робот", "pet_phoenix", "Феникс" },
                new[] { "pet_dragon", "Дракончик", "pet_unicorn", "Единорожек" });

            // gear sets: 4 pieces each, a bonus effect when all are worn
            SetPiece("set_samurai", Rarity.Common, Slot.Hat, "set_samurai_band", "Хатимаки");
            SetPiece("set_samurai", Rarity.Common, Slot.Face, "set_samurai_paint", "Грим самурая");
            SetPiece("set_samurai", Rarity.Common, Slot.Gloves, "set_samurai_gloves", "Наручи самурая");
            SetPiece("set_samurai", Rarity.Common, Slot.Pet, "set_samurai_shiba", "Сиба-ину");
            SetPiece("set_pirate", Rarity.Rare, Slot.Hat, "set_pirate_hat", "Треуголка капитана");
            SetPiece("set_pirate", Rarity.Rare, Slot.Glasses, "set_pirate_patch", "Повязка на глаз");
            SetPiece("set_pirate", Rarity.Rare, Slot.Gloves, "set_pirate_gloves", "Перчатки корсара");
            SetPiece("set_pirate", Rarity.Rare, Slot.Pet, "set_pirate_parrot", "Попугай Кеша");
            SetPiece("set_astro", Rarity.Legendary, Slot.Hat, "set_astro_helmet", "Шлем космонавта");
            SetPiece("set_astro", Rarity.Legendary, Slot.Gloves, "set_astro_gloves", "Перчатки скафандра");
            SetPiece("set_astro", Rarity.Legendary, Slot.Theme, "set_astro_style", "Пульт ЦУПа");
            SetPiece("set_astro", Rarity.Legendary, Slot.Pet, "set_astro_alien", "Инопланетянин");
            // personal sets: anyone can wear the pieces, the bonus shows only on their character
            SetPiece("set_sausage", Rarity.Rare, Slot.Hat, "set_sausage_hat", "Шапка-хот-дог");
            SetPiece("set_sausage", Rarity.Rare, Slot.Gloves, "set_sausage_gloves", "Прихватки гриль-мастера");
            SetPiece("set_sausage", Rarity.Rare, Slot.Theme, "set_sausage_style", "Кетчуп и горчица");
            SetPiece("set_sausage", Rarity.Rare, Slot.Pet, "set_sausage_dachshund", "Такса-сосиска");
            SetPiece("set_melon", Rarity.Rare, Slot.Hat, "set_melon_hat", "Арбузный шлем");
            SetPiece("set_melon", Rarity.Rare, Slot.Glasses, "set_melon_glasses", "Очки-дольки");
            SetPiece("set_melon", Rarity.Rare, Slot.Gloves, "set_melon_gloves", "Арбузные перчатки");
            SetPiece("set_melon", Rarity.Rare, Slot.Theme, "set_melon_style", "Сочный арбуз");
            SetPiece("set_beer", Rarity.Rare, Slot.Hat, "set_beer_hat", "Пивная каска");
            SetPiece("set_beer", Rarity.Rare, Slot.Face, "set_beer_face", "Пенные усы");
            SetPiece("set_beer", Rarity.Rare, Slot.Gloves, "set_beer_gloves", "Пенные перчатки");
            SetPiece("set_beer", Rarity.Rare, Slot.Pet, "set_beer_hop", "Хмелёк");

            // New Year collection: only from New Year boxes, which drop from 15 December to 15 January
            NewYearItem(Rarity.Common, Slot.Hat, "ny_santa", "Шапка Деда Мороза");
            NewYearItem(Rarity.Common, Slot.Gloves, "ny_mittens", "Варежки со снежинками");
            NewYearItem(Rarity.Common, Slot.Face, "ny_frost", "Морозный румянец");
            NewYearItem(Rarity.Rare, Slot.Hat, "ny_antlers", "Оленьи рога");
            NewYearItem(Rarity.Rare, Slot.Glasses, "ny_snowflake", "Очки-снежинки");
            NewYearItem(Rarity.Rare, Slot.Face, "ny_scarf", "Новогодний шарф");
            NewYearItem(Rarity.Legendary, Slot.Theme, "ny_style", "Ёлочные игрушки");
            NewYearItem(Rarity.Legendary, Slot.Pet, "ny_snowman", "Снеговик");
            NewYearItem(Rarity.Immortal, Slot.Hat, "ny_tree", "Ёлка с гирляндой");
        }

        static void NewYearItem(Rarity r, Slot s, string id, string name)
        {
            Item it = new Item(id, name, s, r);
            it.NewYear = true;
            All.Add(it);
        }

        public static List<Item> NewYearItems()
        {
            List<Item> l = new List<Item>();
            foreach (Item it in All) if (it.NewYear) l.Add(it);
            return l;
        }

        // New Year box: a New Year item by the usual rarity odds, unowned ones first;
        // once the whole collection is owned it gives an ordinary item instead
        public static Item RollNewYear(Random rnd, HashSet<string> owned)
        {
            int r0 = (int)RollRarity(rnd);
            List<int> order = new List<int>();
            order.Add(r0);
            for (int r = r0 - 1; r >= 0; r--) order.Add(r);
            for (int r = r0 + 1; r <= 3; r++) order.Add(r);
            foreach (int r in order)
            {
                List<Item> pool = new List<Item>();
                foreach (Item it in NewYearItems()) if ((int)it.Rarity == r && !owned.Contains(it.Id)) pool.Add(it);
                if (pool.Count > 0) return pool[rnd.Next(pool.Count)];
            }
            return RollNew(rnd, owned);
        }

        static void SetPiece(string set, Rarity r, Slot s, string id, string name)
        {
            Item it = new Item(id, name, s, r);
            it.Set = set;
            All.Add(it);
        }

        // items shown in a slot tab of the wardrobe (set pieces and New Year items have their own tabs)
        public static List<Item> OfSlotBase(Slot s)
        {
            List<Item> l = new List<Item>();
            foreach (Item it in All) if (it.Slot == s && it.Set == null && !it.NewYear) l.Add(it);
            return l;
        }

        static void Group(Slot s, params string[][] tiers)
        {
            for (int r = 0; r < tiers.Length; r++)
                for (int i = 0; i + 1 < tiers[r].Length; i += 2)
                    All.Add(new Item(tiers[r][i], tiers[r][i + 1], s, (Rarity)r));
        }

        public static Item Get(string id)
        {
            if (id == null) return null;
            foreach (Item it in All) if (it.Id == id) return it;
            return null;
        }

        public static List<Item> OfSlot(Slot s)
        {
            List<Item> l = new List<Item>();
            foreach (Item it in All) if (it.Slot == s) l.Add(it);
            return l;
        }

        // the regular loot pool (New Year items are left out)
        public static List<Item> OfRarity(Rarity r)
        {
            List<Item> l = new List<Item>();
            foreach (Item it in All) if (it.Rarity == r && !it.NewYear) l.Add(it);
            return l;
        }

        public static Rarity RollRarity(Random rnd)
        {
            double x = rnd.NextDouble(), acc = 0;
            for (int i = 0; i < Chances.Length; i++)
            {
                acc += Chances[i];
                if (x < acc) return (Rarity)i;
            }
            return Rarity.Common;
        }

        // Any item, for the roulette filler.
        public static Item Roll(Random rnd)
        {
            List<Item> pool = OfRarity(RollRarity(rnd));
            return pool[rnd.Next(pool.Count)];
        }

        // Prefers items the player does not own yet. Falls back to lower, then higher rarities
        // once a whole rarity tier is collected; returns a duplicate only when everything is owned.
        public static Item RollNew(Random rnd, HashSet<string> owned)
        {
            int r0 = (int)RollRarity(rnd);
            List<int> order = new List<int>();
            order.Add(r0);
            for (int r = r0 - 1; r >= 0; r--) order.Add(r);
            for (int r = r0 + 1; r <= 3; r++) order.Add(r);
            foreach (int r in order)
            {
                List<Item> pool = new List<Item>();
                foreach (Item it in OfRarity((Rarity)r)) if (!owned.Contains(it.Id)) pool.Add(it);
                if (pool.Count > 0) return pool[rnd.Next(pool.Count)];
            }
            List<Item> any = OfRarity((Rarity)r0);
            return any[rnd.Next(any.Count)];
        }

        public static string RarityName(Rarity r)
        {
            switch (r)
            {
                case Rarity.Rare: return "RARE";
                case Rarity.Legendary: return "LEGENDARY";
                case Rarity.Immortal: return "IMMORTAL";
                default: return "COMMON";
            }
        }

        public static Color RarityColor(Rarity r)
        {
            switch (r)
            {
                case Rarity.Rare: return Color.FromArgb(75, 139, 255);
                case Rarity.Legendary: return Color.FromArgb(182, 92, 255);
                case Rarity.Immortal: return Color.FromArgb(255, 176, 32);
                default: return Color.FromArgb(176, 184, 196);
            }
        }

        public static string SlotName(Slot s)
        {
            switch (s)
            {
                case Slot.Glasses: return "Очки";
                case Slot.Gloves: return "Перчатки";
                case Slot.Face: return "Лицо";
                case Slot.Theme: return "Стиль";
                case Slot.Pet: return "Питомцы";
                default: return "Шапки";
            }
        }

        public static string SlotLabel(Slot s)
        {
            switch (s)
            {
                case Slot.Glasses: return "Очки";
                case Slot.Gloves: return "Перчатки";
                case Slot.Face: return "Лицо";
                case Slot.Theme: return "Стиль";
                case Slot.Pet: return "Питомец";
                default: return "Шапка";
            }
        }
    }

    public static class Characters
    {
        // Ids are stored in the save file: never rename them.
        public const string Yura = "yura", Vlad = "vlad", Stas = "stas";
        public static readonly string[] Ids = { Yura, Vlad, Stas };

        public static string Name(string id)
        {
            return id == Vlad ? "Влад" : id == Stas ? "Стасян" : "Юра";
        }

        // accusative: "за Юру"
        public static string NameWhom(string id)
        {
            return id == Vlad ? "Влада" : id == Stas ? "Стасяна" : "Юру";
        }

        // genitive: "только у Юры"
        public static string NameWho(string id)
        {
            return id == Vlad ? "Влада" : id == Stas ? "Стасяна" : "Юры";
        }

        // debug renders: BONGO_PREVIEW_CHAR=vlad/stas draws that character
        public static string Preview
        {
            get
            {
                string id = Environment.GetEnvironmentVariable("BONGO_PREVIEW_CHAR");
                return Array.IndexOf(Ids, id) >= 0 ? id : null;
            }
        }

        public static string Normalize(string id)
        {
            return Array.IndexOf(Ids, id) >= 0 ? id : Yura;
        }
    }

    public class SaveData
    {
        public string Character = Characters.Yura;
        // leaderboard identity: random id + secret token proving the entry is ours; Nick = null means not taking part
        public string PlayerId, PlayerToken, Nick;
        // sound: key sound mode (see Sound.Modes), volume 0..1, loot/achievement jingles; day phases on/off
        public string SoundMode = "off";
        public float Volume = 0.5f;
        public bool EventSounds = true, DayPhases = true;
        public long Keys, Clicks;
        public int Boxes, Opened, GoldBoxes;
        // shards: the exchange currency duplicates turn into; ShardsSpent feeds the achievement
        public int Shards;
        public long ShardsSpent;
        public HashSet<string> Owned = new HashSet<string>();
        public HashSet<string> Achieved = new HashSet<string>();
        // best keys-per-minute, current daily streak and the last day the app ran (yyyy-MM-dd)
        public int BestKpm, Streak;
        // achievement counters: night keys, immortals from regular boxes, best leaderboard place, keys per character
        public long NightKeys, KeysYura, KeysVlad, KeysStas;
        public int LuckyImm, BestRank;
        // New Year boxes and how many boxes were earned during the event (every 5th is a New Year one)
        public int NyBoxes, NyCount;
        // daily quests: the day they belong to, today's counters and the bitmask of quests already rewarded
        public string QDay;
        public long QKeys, QClicks, QCharKeys;
        public int QKpm, QOpened, QDone;
        // pet experience per pet id, keys per day (yyyy-MM-dd) and presses per virtual-key code
        public Dictionary<string, long> PetXp = new Dictionary<string, long>();
        public Dictionary<string, long> History = new Dictionary<string, long>();
        public Dictionary<int, long> KeyCounts = new Dictionary<int, long>();
        public string LastDay;
        public string Hat, Glasses, Gloves, Face, Theme, Pet;
        public int X = int.MinValue, Y = int.MinValue;
        public float Scale = 0.8f, CounterScale = 1f;
        // counter position relative to the character's head, physical pixels; MinValue = automatic
        public int CounterDX = int.MinValue, CounterDY = int.MinValue;
        public bool TopMost = true, ShowCounter = true;

        public static string Dir
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BongoBro"); }
        }
        public static string FilePath { get { return Path.Combine(Dir, "save.dat"); } }

        public string GetEquipped(Slot s)
        {
            switch (s)
            {
                case Slot.Hat: return Hat;
                case Slot.Glasses: return Glasses;
                case Slot.Gloves: return Gloves;
                case Slot.Face: return Face;
                case Slot.Pet: return Pet;
                default: return Theme;
            }
        }

        public void SetEquipped(Slot s, string id)
        {
            switch (s)
            {
                case Slot.Hat: Hat = id; break;
                case Slot.Glasses: Glasses = id; break;
                case Slot.Gloves: Gloves = id; break;
                case Slot.Face: Face = id; break;
                case Slot.Pet: Pet = id; break;
                default: Theme = id; break;
            }
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Dir);
                CultureInfo ci = CultureInfo.InvariantCulture;
                StringBuilder sb = new StringBuilder();
                sb.Append("keys=").Append(Keys.ToString(ci)).Append('\n');
                sb.Append("clicks=").Append(Clicks.ToString(ci)).Append('\n');
                sb.Append("boxes=").Append(Boxes.ToString(ci)).Append('\n');
                sb.Append("opened=").Append(Opened.ToString(ci)).Append('\n');
                sb.Append("owned=").Append(string.Join(",", new List<string>(Owned).ToArray())).Append('\n');
                sb.Append("hat=").Append(Hat ?? "").Append('\n');
                sb.Append("glasses=").Append(Glasses ?? "").Append('\n');
                sb.Append("gloves=").Append(Gloves ?? "").Append('\n');
                sb.Append("face=").Append(Face ?? "").Append('\n');
                sb.Append("theme=").Append(Theme ?? "").Append('\n');
                sb.Append("pet=").Append(Pet ?? "").Append('\n');
                sb.Append("x=").Append(X.ToString(ci)).Append('\n');
                sb.Append("y=").Append(Y.ToString(ci)).Append('\n');
                sb.Append("scale=").Append(Scale.ToString(ci)).Append('\n');
                sb.Append("cscale=").Append(CounterScale.ToString(ci)).Append('\n');
                sb.Append("cdx=").Append(CounterDX.ToString(ci)).Append('\n');
                sb.Append("cdy=").Append(CounterDY.ToString(ci)).Append('\n');
                sb.Append("topmost=").Append(TopMost ? "1" : "0").Append('\n');
                sb.Append("counter=").Append(ShowCounter ? "1" : "0").Append('\n');
                sb.Append("gold=").Append(GoldBoxes.ToString(ci)).Append('\n');
                sb.Append("shards=").Append(Shards.ToString(ci)).Append('\n');
                sb.Append("spent=").Append(ShardsSpent.ToString(ci)).Append('\n');
                sb.Append("achieved=").Append(string.Join(",", new List<string>(Achieved).ToArray())).Append('\n');
                sb.Append("bestkpm=").Append(BestKpm.ToString(ci)).Append('\n');
                sb.Append("streak=").Append(Streak.ToString(ci)).Append('\n');
                sb.Append("lastday=").Append(LastDay ?? "").Append('\n');
                sb.Append("char=").Append(Character).Append('\n');
                sb.Append("pid=").Append(PlayerId ?? "").Append('\n');
                sb.Append("ptoken=").Append(PlayerToken ?? "").Append('\n');
                sb.Append("nick=").Append(Nick ?? "").Append('\n');
                sb.Append("sound=").Append(SoundMode).Append('\n');
                sb.Append("volume=").Append(Volume.ToString(ci)).Append('\n');
                sb.Append("evsound=").Append(EventSounds ? "1" : "0").Append('\n');
                sb.Append("dayphases=").Append(DayPhases ? "1" : "0").Append('\n');
                sb.Append("nightkeys=").Append(NightKeys.ToString(ci)).Append('\n');
                sb.Append("lucky=").Append(LuckyImm.ToString(ci)).Append('\n');
                sb.Append("bestrank=").Append(BestRank.ToString(ci)).Append('\n');
                sb.Append("kyura=").Append(KeysYura.ToString(ci)).Append('\n');
                sb.Append("kvlad=").Append(KeysVlad.ToString(ci)).Append('\n');
                sb.Append("kstas=").Append(KeysStas.ToString(ci)).Append('\n');
                sb.Append("nybox=").Append(NyBoxes.ToString(ci)).Append('\n');
                sb.Append("nycount=").Append(NyCount.ToString(ci)).Append('\n');
                sb.Append("qday=").Append(QDay ?? "").Append('\n');
                sb.Append("qkeys=").Append(QKeys.ToString(ci)).Append('\n');
                sb.Append("qclicks=").Append(QClicks.ToString(ci)).Append('\n');
                sb.Append("qchar=").Append(QCharKeys.ToString(ci)).Append('\n');
                sb.Append("qkpm=").Append(QKpm.ToString(ci)).Append('\n');
                sb.Append("qopen=").Append(QOpened.ToString(ci)).Append('\n');
                sb.Append("qdone=").Append(QDone.ToString(ci)).Append('\n');
                sb.Append("petxp=").Append(JoinMap(PetXp)).Append('\n');
                // only the last 60 days of history are kept
                Dictionary<string, long> hist = new Dictionary<string, long>();
                string oldest = DateTime.Today.AddDays(-60).ToString("yyyy-MM-dd", ci);
                foreach (KeyValuePair<string, long> kv in History) if (string.CompareOrdinal(kv.Key, oldest) >= 0) hist[kv.Key] = kv.Value;
                sb.Append("hist=").Append(JoinMap(hist)).Append('\n');
                Dictionary<string, long> vk = new Dictionary<string, long>();
                foreach (KeyValuePair<int, long> kv in KeyCounts) vk[kv.Key.ToString(ci)] = kv.Value;
                sb.Append("vk=").Append(JoinMap(vk)).Append('\n');
                // write to disk for real before swapping files, so a power cut cannot leave an empty save
                string tmp = FilePath + ".tmp";
                byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
                using (FileStream fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
                {
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush(true);
                }
                if (File.Exists(FilePath)) File.Replace(tmp, FilePath, FilePath + ".bak", true);
                else File.Move(tmp, FilePath);
                DailyBackup();
            }
            catch { }
        }

        public static string BackupDir { get { return Path.Combine(Dir, "backups"); } }

        // one copy per day in backups\, the last 14 days are kept
        void DailyBackup()
        {
            if (Keys <= 0) return;
            Directory.CreateDirectory(BackupDir);
            string today = Path.Combine(BackupDir, "save-" + DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".dat");
            // during the day keep refreshing today's copy, but never replace it with a smaller save
            SaveData existing = File.Exists(today) ? TryLoad(today) : null;
            if (existing == null || existing.Keys <= Keys) File.Copy(FilePath, today, true);
            string[] all = Directory.GetFiles(BackupDir, "save-*.dat");
            Array.Sort(all, StringComparer.Ordinal);
            for (int i = 0; i < all.Length - 14; i++) File.Delete(all[i]);
        }

        // "key:value,key:value" maps in the save file
        static string JoinMap(Dictionary<string, long> m)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, long> kv in m)
            {
                if (sb.Length > 0) sb.Append(',');
                sb.Append(kv.Key).Append(':').Append(kv.Value.ToString(CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        static Dictionary<string, long> SplitMap(string v)
        {
            Dictionary<string, long> m = new Dictionary<string, long>();
            foreach (string part in v.Split(','))
            {
                int i = part.LastIndexOf(':');
                long n;
                if (i > 0 && long.TryParse(part.Substring(i + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out n)) m[part.Substring(0, i).Trim()] = n;
            }
            return m;
        }

        // Falls back to save.dat.bak and then to the newest daily backup. A save that could not be read
        // is copied aside first, so starting over never destroys it.
        public static SaveData Load()
        {
            SaveData d = TryLoad(FilePath);
            if (d != null) return d;
            KeepBroken(FilePath);
            d = TryLoad(FilePath + ".bak");
            if (d != null) return d;
            KeepBroken(FilePath + ".bak");
            try
            {
                if (Directory.Exists(BackupDir))
                {
                    string[] all = Directory.GetFiles(BackupDir, "save-*.dat");
                    Array.Sort(all, StringComparer.Ordinal);
                    for (int i = all.Length - 1; i >= 0; i--)
                    {
                        d = TryLoad(all[i]);
                        if (d != null) return d;
                    }
                }
            }
            catch { }
            return new SaveData();
        }

        static void KeepBroken(string path)
        {
            try
            {
                if (File.Exists(path) && new FileInfo(path).Length > 0)
                    File.Copy(path, path + ".broken-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture), true);
            }
            catch { }
        }

        static SaveData TryLoad(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                CultureInfo ci = CultureInfo.InvariantCulture;
                SaveData d = new SaveData();
                bool valid = false;
                foreach (string raw in File.ReadAllLines(path, Encoding.UTF8))
                {
                    int i = raw.IndexOf('=');
                    if (i <= 0) continue;
                    string k = raw.Substring(0, i).Trim(), v = raw.Substring(i + 1).Trim();
                    string id = v.Length > 0 ? v : null;
                    switch (k)
                    {
                        case "keys": valid = long.TryParse(v, NumberStyles.Integer, ci, out d.Keys); break;
                        case "clicks": long.TryParse(v, NumberStyles.Integer, ci, out d.Clicks); break;
                        case "boxes": int.TryParse(v, NumberStyles.Integer, ci, out d.Boxes); break;
                        case "opened": int.TryParse(v, NumberStyles.Integer, ci, out d.Opened); break;
                        case "owned":
                            foreach (string o in v.Split(','))
                                if (Items.Get(o.Trim()) != null) d.Owned.Add(o.Trim());
                            break;
                        case "hat": d.Hat = id; break;
                        case "glasses": d.Glasses = id; break;
                        case "gloves": d.Gloves = id; break;
                        case "face": d.Face = id; break;
                        case "theme": d.Theme = id; break;
                        case "pet": d.Pet = id; break;
                        case "x": int.TryParse(v, NumberStyles.Integer, ci, out d.X); break;
                        case "y": int.TryParse(v, NumberStyles.Integer, ci, out d.Y); break;
                        case "scale": float.TryParse(v, NumberStyles.Float, ci, out d.Scale); break;
                        case "cscale": float.TryParse(v, NumberStyles.Float, ci, out d.CounterScale); break;
                        case "cdx": int.TryParse(v, NumberStyles.Integer, ci, out d.CounterDX); break;
                        case "cdy": int.TryParse(v, NumberStyles.Integer, ci, out d.CounterDY); break;
                        case "topmost": d.TopMost = v == "1"; break;
                        case "counter": d.ShowCounter = v == "1"; break;
                        case "gold": int.TryParse(v, NumberStyles.Integer, ci, out d.GoldBoxes); break;
                        case "shards": int.TryParse(v, NumberStyles.Integer, ci, out d.Shards); break;
                        case "spent": long.TryParse(v, NumberStyles.Integer, ci, out d.ShardsSpent); break;
                        case "achieved":
                            foreach (string a in v.Split(',')) if (a.Trim().Length > 0) d.Achieved.Add(a.Trim());
                            break;
                        case "bestkpm": int.TryParse(v, NumberStyles.Integer, ci, out d.BestKpm); break;
                        case "streak": int.TryParse(v, NumberStyles.Integer, ci, out d.Streak); break;
                        case "lastday": d.LastDay = id; break;
                        case "char": d.Character = Characters.Normalize(v); break;
                        case "pid": d.PlayerId = id; break;
                        case "ptoken": d.PlayerToken = id; break;
                        case "nick": d.Nick = id; break;
                        case "sound": if (Array.IndexOf(Sound.Modes, v) >= 0) d.SoundMode = v; break;
                        case "volume": float.TryParse(v, NumberStyles.Float, ci, out d.Volume); d.Volume = Math.Max(0f, Math.Min(1f, d.Volume)); break;
                        case "evsound": d.EventSounds = v == "1"; break;
                        case "dayphases": d.DayPhases = v == "1"; break;
                        case "nightkeys": long.TryParse(v, NumberStyles.Integer, ci, out d.NightKeys); break;
                        case "lucky": int.TryParse(v, NumberStyles.Integer, ci, out d.LuckyImm); break;
                        case "bestrank": int.TryParse(v, NumberStyles.Integer, ci, out d.BestRank); break;
                        case "kyura": long.TryParse(v, NumberStyles.Integer, ci, out d.KeysYura); break;
                        case "kvlad": long.TryParse(v, NumberStyles.Integer, ci, out d.KeysVlad); break;
                        case "kstas": long.TryParse(v, NumberStyles.Integer, ci, out d.KeysStas); break;
                        case "nybox": int.TryParse(v, NumberStyles.Integer, ci, out d.NyBoxes); break;
                        case "nycount": int.TryParse(v, NumberStyles.Integer, ci, out d.NyCount); break;
                        case "qday": d.QDay = id; break;
                        case "qkeys": long.TryParse(v, NumberStyles.Integer, ci, out d.QKeys); break;
                        case "qclicks": long.TryParse(v, NumberStyles.Integer, ci, out d.QClicks); break;
                        case "qchar": long.TryParse(v, NumberStyles.Integer, ci, out d.QCharKeys); break;
                        case "qkpm": int.TryParse(v, NumberStyles.Integer, ci, out d.QKpm); break;
                        case "qopen": int.TryParse(v, NumberStyles.Integer, ci, out d.QOpened); break;
                        case "qdone": int.TryParse(v, NumberStyles.Integer, ci, out d.QDone); break;
                        case "petxp": d.PetXp = SplitMap(v); break;
                        case "hist": d.History = SplitMap(v); break;
                        case "vk":
                            foreach (KeyValuePair<string, long> kv in SplitMap(v))
                            {
                                int code;
                                if (int.TryParse(kv.Key, NumberStyles.Integer, ci, out code)) d.KeyCounts[code] = kv.Value;
                            }
                            break;
                    }
                }
                // an empty or zero-filled file (e.g. after a power cut) is not a save
                if (!valid) return null;
                if (d.Scale < 0.15f || d.Scale > 2f) d.Scale = 0.8f;
                if (d.CounterScale < 0.3f || d.CounterScale > 2.5f) d.CounterScale = 1f;
                return d;
            }
            catch { return null; }
        }
    }

    public static class Game
    {
        public const int BoxEvery = 2000;
        public static SaveData S;
        public static readonly Random Rnd = new Random();
        public static bool Dirty;

        public static void Load()
        {
            S = SaveData.Load();
            foreach (Slot s in Items.Slots)
            {
                string id = S.GetEquipped(s);
                Item it = Items.Get(id);
                if (id != null && (it == null || it.Slot != s || !S.Owned.Contains(id))) S.SetEquipped(s, null);
            }
        }

        public static void Save()
        {
            // developer mode is a sandbox: nothing reaches the real save file
            if (!DevMode) S.Save();
            Dirty = false;
        }

        // ---------------- developer mode ----------------
        // Only the SHA-256 of "BongoBro.dev:" + code is shipped, the code itself is not in the source.
        const string DevCodeHash = "f1b1bd751ae82d3ed357bc9feac82da87a25adc71491c4eb1a27be8de27e2dbb";
        public static bool DevMode;
        public static Mood? DevMood;
        public static DayPhase? DevDay;

        public static bool CheckDevCode(string code)
        {
            string s = "BongoBro.dev:" + (code ?? "").Trim().ToUpperInvariant();
            using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] h = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in h) sb.Append(b.ToString("x2"));
                return sb.ToString() == DevCodeHash;
            }
        }

        public static void EnterDev()
        {
            if (DevMode) return;
            Save();          // keep the real progress safe first
            DevMode = true;
        }

        // drops every sandbox change and reloads the real save
        public static void ExitDev()
        {
            if (!DevMode) return;
            DevMode = false;
            DevMood = null;
            DevDay = null;
            NewYear.DevOverride = null;
            Load();
        }

        // Raised for every achievement unlocked (its reward is already granted).
        public static event Action<Achievement> Unlocked;

        public static int TotalBoxes { get { return S.Boxes + S.GoldBoxes + S.NyBoxes; } }

        // true when the last box earned by key presses was a New Year one
        public static bool LastBoxNewYear;

        // returns true when a new loot box was earned
        public static bool AddKey(int vk)
        {
            S.Keys++;
            if (S.Character == Characters.Vlad) S.KeysVlad++;
            else if (S.Character == Characters.Stas) S.KeysStas++;
            else S.KeysYura++;
            if (DateTime.Now.Hour < 5) S.NightKeys++;
            Stats.CountKey(S, vk);
            PetLevels.AddXp(S);
            Quests.OnKey(S);
            Dirty = true;
            bool box = false;
            if (S.Keys % BoxEvery == 0)
            {
                // during the New Year event every 5th box is a New Year one
                LastBoxNewYear = NewYear.Active && ++S.NyCount % 5 == 0;
                if (LastBoxNewYear) S.NyBoxes++; else S.Boxes++;
                Save();
                box = true;
            }
            CheckAchievements();
            return box;
        }

        public static void AddClick() { S.Clicks++; Quests.OnClick(S); Dirty = true; CheckAchievements(); }

        // the top-3 achievement counts only once the table has enough players
        public const int TopMinPlayers = 5;

        public static void ReportRank(int rank, int players)
        {
            if (rank <= 0 || players < TopMinPlayers || (S.BestRank > 0 && rank >= S.BestRank)) return;
            S.BestRank = rank;
            Save();
            CheckAchievements();
        }

        public static void ReportKpm(int kpm)
        {
            Quests.OnKpm(S, kpm);
            if (kpm <= S.BestKpm) return;
            S.BestKpm = kpm;
            Dirty = true;
            CheckAchievements();
        }

        // Raised on every new day of the streak with the reward granted: days in a row, boxes, gold boxes.
        public static event Action<int, int, int> StreakDay;

        // Counts consecutive calendar days the app was running.
        public static void TouchDay()
        {
            DateTime today = DateTime.Today;
            string key = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (S.LastDay == key) return;
            string yesterday = today.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            S.Streak = S.LastDay == yesterday ? S.Streak + 1 : 1;
            S.LastDay = key;
            Quests.Refresh(S);
            // coming back the next day pays: a box per day of the streak, a gold one every 7th
            int boxes = 0, gold = 0;
            if (S.Streak >= 2)
            {
                boxes = 1;
                if (S.Streak % 7 == 0) gold = 1;
                S.Boxes += boxes;
                S.GoldBoxes += gold;
            }
            Save();
            CheckAchievements();
            if (boxes + gold > 0 && StreakDay != null) StreakDay(S.Streak, boxes, gold);
        }

        public static void CheckAchievements()
        {
            foreach (Achievement a in Achievement.All)
            {
                if (S.Achieved.Contains(a.Id) || a.Progress(S) < a.Target) continue;
                S.Achieved.Add(a.Id);
                if (a.Gold) S.GoldBoxes++; else S.Boxes += a.Boxes;
                Save();
                if (Unlocked != null) Unlocked(a);
            }
        }

        public static Item OpenBox(out bool isNew)
        {
            S.Boxes--;
            S.Opened++;
            Item it = Items.RollNew(Rnd, S.Owned);
            isNew = S.Owned.Add(it.Id);
            if (it.Rarity == Rarity.Immortal) S.LuckyImm++;
            if (!isNew) S.Shards += Items.ShardValue(it.Rarity);
            Quests.OnBoxOpened(S);
            Save();
            CheckAchievements();
            return it;
        }

        // Golden box: immortal items only, new ones first.
        public static Item OpenGoldBox(out bool isNew)
        {
            S.GoldBoxes--;
            S.Opened++;
            List<Item> imm = Items.OfRarity(Rarity.Immortal), fresh = new List<Item>();
            foreach (Item i in imm) if (!S.Owned.Contains(i.Id)) fresh.Add(i);
            List<Item> pool = fresh.Count > 0 ? fresh : imm;
            Item it = pool[Rnd.Next(pool.Count)];
            isNew = S.Owned.Add(it.Id);
            if (!isNew) S.Shards += Items.ShardValue(it.Rarity);
            Quests.OnBoxOpened(S);
            Save();
            CheckAchievements();
            return it;
        }

        public static Item OpenNewYearBox(out bool isNew)
        {
            S.NyBoxes--;
            S.Opened++;
            Item it = Items.RollNewYear(Rnd, S.Owned);
            isNew = S.Owned.Add(it.Id);
            if (!isNew) S.Shards += Items.ShardValue(it.Rarity);
            Quests.OnBoxOpened(S);
            Save();
            CheckAchievements();
            return it;
        }

        // The exchange: a duplicate-free way to get a specific item (New Year collection excluded).
        public static bool BuyWithShards(Item it)
        {
            if (it == null || it.NewYear || S.Owned.Contains(it.Id)) return false;
            int price = Items.ShardPrice(it.Rarity);
            if (S.Shards < price) return false;
            S.Shards -= price;
            S.ShardsSpent += price;
            S.Owned.Add(it.Id);
            Save();
            CheckAchievements();
            return true;
        }

        public static void Equip(Item it)
        {
            if (!S.Owned.Contains(it.Id)) return;
            S.SetEquipped(it.Slot, it.Id);
            Save();
            CheckAchievements();
        }

        public static void Unequip(Slot s) { S.SetEquipped(s, null); Save(); }
    }

    public class GearSet
    {
        public string Id, Name, Bonus;
        public Rarity Rarity;

        public string Owner;   // character id of a personal set, or null when the bonus works for everyone

        GearSet(string id, string name, Rarity r, string bonus, string owner = null) { Id = id; Name = name; Rarity = r; Bonus = bonus; Owner = owner; }

        // Ids are stored as Item.Set: never rename them.
        public static readonly List<GearSet> All = new List<GearSet>
        {
            new GearSet("set_samurai", "Самурай", Rarity.Common, "Красное солнце и падающая сакура"),
            new GearSet("set_pirate", "Пират", Rarity.Rare, "Пиратский флаг и золотые монеты на нажатиях"),
            new GearSet("set_astro", "Космонавт", Rarity.Legendary, "Орбита с планетой, звёзды и невесомость"),
            new GearSet("set_sausage", "Гриль-мастер", Rarity.Rare, "Жар гриля, дымок и сосиски на нажатиях", Characters.Yura),
            new GearSet("set_melon", "Бахча", Rarity.Rare, "Арбузный дождь и семечки на нажатиях", Characters.Vlad),
            new GearSet("set_beer", "Пивной фестиваль", Rarity.Rare, "Флажки, пузырьки и брызги пены на нажатиях", Characters.Stas),
        };

        // a personal set's bonus works only on its own character
        public bool BonusFor(string character) { return Owner == null || Owner == character; }

        public List<Item> Pieces()
        {
            List<Item> l = new List<Item>();
            foreach (Item it in Items.All) if (it.Set == Id) l.Add(it);
            return l;
        }

        public int OwnedCount(SaveData s)
        {
            int n = 0;
            foreach (Item it in Pieces()) if (s.Owned.Contains(it.Id)) n++;
            return n;
        }

        public bool IsWorn(Func<Slot, string> equipped)
        {
            foreach (Item it in Pieces()) if (equipped(it.Slot) != it.Id) return false;
            return true;
        }

        // the set whose every piece is equipped right now, or null
        public static string Active(Func<Slot, string> equipped, string character)
        {
            foreach (GearSet g in All) if (g.BonusFor(character) && g.IsWorn(equipped)) return g.Id;
            return null;
        }
    }

    public class Achievement
    {
        public string Id, Name, Desc;
        public long Target;
        public int Boxes;   // regular loot boxes granted
        public bool Gold;   // grants one golden (immortal-only) loot box instead
        public Func<SaveData, long> Progress;

        Achievement(string id, string name, string desc, long target, int boxes, bool gold, Func<SaveData, long> progress)
        {
            Id = id; Name = name; Desc = desc; Target = target; Boxes = boxes; Gold = gold; Progress = progress;
        }

        // Ids are stored in the save file: never rename or remove them.
        public static readonly List<Achievement> All = new List<Achievement>
        {
            new Achievement("ach_keys1k", "Разминка", "Нажать 1 000 клавиш", 1000, 1, false, s => s.Keys),
            new Achievement("ach_clicks", "Кликер", "Кликнуть мышью 5 000 раз", 5000, 1, false, s => s.Clicks),
            new Achievement("ach_fashion", "Модник", "Надеть вещи во все 5 слотов сразу", 5, 1, false, EquippedSlots),
            new Achievement("ach_speed300", "Скоропечатник", "Нажать 300 клавиш за одну минуту", 300, 2, false, s => s.BestKpm),
            new Achievement("ach_streak7", "Постоянство", "Запускать BongoBro 7 дней подряд", 7, 2, false, s => s.Streak),
            new Achievement("ach_collect25", "Коллекционер", "Собрать 25 разных предметов", 25, 2, false, s => s.Owned.Count),
            new Achievement("ach_keys50k", "Марафонец", "Нажать 50 000 клавиш", 50000, 3, false, s => s.Keys),
            new Achievement("ach_keys250k", "Легенда клавиатуры", "Нажать 250 000 клавиш", 250000, 0, true, s => s.Keys),
            new Achievement("ach_legendary", "Королевская коллекция", "Собрать все " + Items.OfRarity(Rarity.Legendary).Count + " legendary-предметов", Items.OfRarity(Rarity.Legendary).Count, 0, true, OwnedLegendary),
            new Achievement("ach_speed550", "Бог скорости", "Нажать 550 клавиш за одну минуту", 550, 0, true, s => s.BestKpm),
            new Achievement("ach_owl", "Сова", "Нажать 500 клавиш между 0:00 и 5:00", 500, 1, false, s => s.NightKeys),
            new Achievement("ach_zoo", "Зоопарк", "Собрать 10 питомцев", 10, 2, false, OwnedPets),
            new Achievement("ach_setfull", "Полный комплект", "Собрать все 4 вещи любого сета", 4, 2, false, BestSet),
            new Achievement("ach_lucky", "Счастливчик", "Выбить immortal из обычного лутбокса", 1, 2, false, s => s.LuckyImm),
            new Achievement("ach_yura", "Сосисочник", "Нажать 10 000 клавиш за Юру", 10000, 1, false, s => s.KeysYura),
            new Achievement("ach_vlad", "Арбузный сок", "Нажать 10 000 клавиш за Влада", 10000, 1, false, s => s.KeysVlad),
            new Achievement("ach_stas", "Стасян одобряет", "Нажать 10 000 клавиш за Стасяна", 10000, 1, false, s => s.KeysStas),
            new Achievement("ach_gang", "Вся банда", "Нажать по 1 000 клавиш за каждого персонажа", 1000, 2, false, s => Math.Min(s.KeysYura, Math.Min(s.KeysVlad, s.KeysStas))),
            new Achievement("ach_streak30", "Железная воля", "Запускать BongoBro 30 дней подряд", 30, 3, false, s => s.Streak),
            new Achievement("ach_million", "Миллионер", "Нажать 1 000 000 клавиш", 1000000, 0, true, s => s.Keys),
            new Achievement("ach_top3", "Пьедестал", "Попасть в топ-3 лидерборда (от " + Game.TopMinPlayers + " игроков)", 1, 0, true, s => s.BestRank > 0 && s.BestRank <= 3 ? 1 : 0),
            new Achievement("ach_shards", "Копилка", "Накопить 500 осколков за дубликаты", 500, 1, false, s => s.Shards),
            new Achievement("ach_spender", "Транжира", "Потратить 1 000 осколков на обмен", 1000, 2, false, s => s.ShardsSpent),
            new Achievement("ach_collect50", "Коллекционер II", "Собрать 50 разных предметов", 50, 2, false, s => s.Owned.Count),
            new Achievement("ach_streak100", "Сто дней без перерыва", "Запускать BongoBro 100 дней подряд", 100, 0, true, s => s.Streak),
        };

        static long OwnedPets(SaveData s)
        {
            int n = 0;
            foreach (Item it in Items.OfSlot(Slot.Pet)) if (s.Owned.Contains(it.Id)) n++;
            return n;
        }

        static long BestSet(SaveData s)
        {
            int best = 0;
            foreach (GearSet g in GearSet.All) best = Math.Max(best, g.OwnedCount(s));
            return best;
        }

        static long EquippedSlots(SaveData s)
        {
            int n = 0;
            foreach (Slot sl in Items.Slots) if (s.GetEquipped(sl) != null) n++;
            return n;
        }

        static long OwnedLegendary(SaveData s)
        {
            int n = 0;
            foreach (Item it in Items.OfRarity(Rarity.Legendary)) if (s.Owned.Contains(it.Id)) n++;
            return n;
        }

        public string RewardText()
        {
            if (Gold) return "Золотой лутбокс";
            return Boxes == 1 ? "1 лутбокс" : Boxes + " лутбокса";
        }
    }

    public static class AutoStart
    {
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string Name = "BongoBro";

        public static bool IsOn()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(RunKey))
                    return k != null && k.GetValue(Name) != null;
            }
            catch { return false; }
        }

        public static void Set(bool on)
        {
            using (RegistryKey k = Registry.CurrentUser.CreateSubKey(RunKey))
            {
                if (on) k.SetValue(Name, "\"" + Application.ExecutablePath + "\"");
                else k.DeleteValue(Name, false);
            }
        }
    }
}

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
    public enum Slot { Hat = 0, Glasses = 1, Gloves = 2, Face = 3, Theme = 4 }

    public class Item
    {
        public string Id;
        public string Name;
        public Slot Slot;
        public Rarity Rarity;
        public Item(string id, string name, Slot slot, Rarity rarity) { Id = id; Name = name; Slot = slot; Rarity = rarity; }
    }

    public static class Items
    {
        public static readonly List<Item> All = new List<Item>();
        public static readonly double[] Chances = { 0.62, 0.26, 0.10, 0.02 };
        public static readonly Slot[] Slots = { Slot.Hat, Slot.Glasses, Slot.Gloves, Slot.Face, Slot.Theme };

        static Items()
        {
            Group(Slot.Hat,
                new[] { "hat_beanie", "Серая бини", "hat_cap", "Красная кепка", "hat_bucket", "Панама", "hat_headband", "Спортивная повязка", "hat_flower", "Цветочек" },
                new[] { "hat_pompom", "Шапка с помпоном", "hat_ushanka", "Ушанка", "hat_catears", "Кошачьи ушки", "hat_cowboy", "Ковбойская шляпа" },
                new[] { "hat_tophat", "Цилиндр джентльмена", "hat_crown", "Золотая корона", "hat_viking", "Шлем викинга" },
                new[] { "hat_firecrown", "Корона Инферно", "hat_halo", "Божественный нимб" });
            Group(Slot.Glasses,
                new[] { "gl_round", "Круглые очки", "gl_square", "Очки хипстера", "gl_hearts", "Очки-сердечки", "gl_cateye", "Кошачий глаз", "gl_beach", "Пляжные очки" },
                new[] { "gl_aviator", "Авиаторы", "gl_3d", "3D-очки", "gl_ski", "Лыжная маска", "gl_shutter", "Неоновые шторки" },
                new[] { "gl_pixel", "Deal With It", "gl_monocle", "Монокль", "gl_steampunk", "Стимпанк-гогглы" },
                new[] { "gl_laser", "Лазерный визор", "gl_galaxy", "Очки Галактики" });
            Group(Slot.Gloves,
                new[] { "gv_knit", "Вязаные перчатки", "gv_garden", "Садовые перчатки", "gv_rubber", "Резиновые перчатки", "gv_medical", "Медицинские перчатки", "gv_striped", "Полосатые перчатки" },
                new[] { "gv_leather", "Кожаные перчатки", "gv_fingerless", "Митенки", "gv_boxing", "Боксёрские перчатки", "gv_paw", "Кошачьи лапки" },
                new[] { "gv_mickey", "Мультяшные перчатки", "gv_gold", "Золотые перчатки", "gv_knight", "Рыцарские латы" },
                new[] { "gv_infinity", "Перчатка Бесконечности", "gv_plasma", "Плазменные перчатки" });
            Group(Slot.Face,
                new[] { "fc_blush", "Румянец", "fc_freckles", "Веснушки", "fc_wink", "Подмигивание", "fc_grin", "Улыбка до ушей", "fc_tongue", "Дразнилка" },
                new[] { "fc_mustache", "Усы джентльмена", "fc_cat", "Кошачий грим", "fc_hearts", "Влюблённый взгляд", "fc_warpaint", "Боевая раскраска" },
                new[] { "fc_clown", "Клоун", "fc_skull", "Сахарный череп", "fc_stars", "Звёздный взгляд" },
                new[] { "fc_fireeyes", "Пламенный взгляд", "fc_neon", "Неоновый грим" });
            Group(Slot.Theme,
                new[] { "th_snow", "Белоснежка", "th_mint", "Мятный", "th_peach", "Персик", "th_graphite", "Графит", "th_lavender", "Лаванда" },
                new[] { "th_retro", "Ретро", "th_sakura", "Сакура", "th_ocean", "Океан", "th_matcha", "Матча" },
                new[] { "th_marble", "Золото и мрамор", "th_cyber", "Киберпанк", "th_candy", "Конфетка" },
                new[] { "th_rgb", "RGB Геймер", "th_cosmos", "Космос" });
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

        public static List<Item> OfRarity(Rarity r)
        {
            List<Item> l = new List<Item>();
            foreach (Item it in All) if (it.Rarity == r) l.Add(it);
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
                default: return "Шапка";
            }
        }
    }

    public static class Characters
    {
        // Ids are stored in the save file: never rename them.
        public const string Yura = "yura", Vlad = "vlad";
        public static readonly string[] Ids = { Yura, Vlad };

        public static string Name(string id)
        {
            return id == Vlad ? "Влад" : "Юра";
        }

        public static string Normalize(string id)
        {
            return Array.IndexOf(Ids, id) >= 0 ? id : Yura;
        }
    }

    public class SaveData
    {
        public string Character = Characters.Yura;
        public long Keys, Clicks;
        public int Boxes, Opened, GoldBoxes;
        public HashSet<string> Owned = new HashSet<string>();
        public HashSet<string> Achieved = new HashSet<string>();
        // best keys-per-minute, current daily streak and the last day the app ran (yyyy-MM-dd)
        public int BestKpm, Streak;
        public string LastDay;
        public string Hat, Glasses, Gloves, Face, Theme;
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
                sb.Append("x=").Append(X.ToString(ci)).Append('\n');
                sb.Append("y=").Append(Y.ToString(ci)).Append('\n');
                sb.Append("scale=").Append(Scale.ToString(ci)).Append('\n');
                sb.Append("cscale=").Append(CounterScale.ToString(ci)).Append('\n');
                sb.Append("cdx=").Append(CounterDX.ToString(ci)).Append('\n');
                sb.Append("cdy=").Append(CounterDY.ToString(ci)).Append('\n');
                sb.Append("topmost=").Append(TopMost ? "1" : "0").Append('\n');
                sb.Append("counter=").Append(ShowCounter ? "1" : "0").Append('\n');
                sb.Append("gold=").Append(GoldBoxes.ToString(ci)).Append('\n');
                sb.Append("achieved=").Append(string.Join(",", new List<string>(Achieved).ToArray())).Append('\n');
                sb.Append("bestkpm=").Append(BestKpm.ToString(ci)).Append('\n');
                sb.Append("streak=").Append(Streak.ToString(ci)).Append('\n');
                sb.Append("lastday=").Append(LastDay ?? "").Append('\n');
                sb.Append("char=").Append(Character).Append('\n');
                string tmp = FilePath + ".tmp";
                File.WriteAllText(tmp, sb.ToString(), Encoding.UTF8);
                if (File.Exists(FilePath))
                {
                    File.Copy(FilePath, FilePath + ".bak", true);
                    File.Delete(FilePath);
                }
                File.Move(tmp, FilePath);
            }
            catch { }
        }

        public static SaveData Load()
        {
            SaveData d = TryLoad(FilePath);
            if (d == null) d = TryLoad(FilePath + ".bak");
            return d ?? new SaveData();
        }

        static SaveData TryLoad(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                CultureInfo ci = CultureInfo.InvariantCulture;
                SaveData d = new SaveData();
                foreach (string raw in File.ReadAllLines(path, Encoding.UTF8))
                {
                    int i = raw.IndexOf('=');
                    if (i <= 0) continue;
                    string k = raw.Substring(0, i).Trim(), v = raw.Substring(i + 1).Trim();
                    string id = v.Length > 0 ? v : null;
                    switch (k)
                    {
                        case "keys": long.TryParse(v, NumberStyles.Integer, ci, out d.Keys); break;
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
                        case "x": int.TryParse(v, NumberStyles.Integer, ci, out d.X); break;
                        case "y": int.TryParse(v, NumberStyles.Integer, ci, out d.Y); break;
                        case "scale": float.TryParse(v, NumberStyles.Float, ci, out d.Scale); break;
                        case "cscale": float.TryParse(v, NumberStyles.Float, ci, out d.CounterScale); break;
                        case "cdx": int.TryParse(v, NumberStyles.Integer, ci, out d.CounterDX); break;
                        case "cdy": int.TryParse(v, NumberStyles.Integer, ci, out d.CounterDY); break;
                        case "topmost": d.TopMost = v == "1"; break;
                        case "counter": d.ShowCounter = v == "1"; break;
                        case "gold": int.TryParse(v, NumberStyles.Integer, ci, out d.GoldBoxes); break;
                        case "achieved":
                            foreach (string a in v.Split(',')) if (a.Trim().Length > 0) d.Achieved.Add(a.Trim());
                            break;
                        case "bestkpm": int.TryParse(v, NumberStyles.Integer, ci, out d.BestKpm); break;
                        case "streak": int.TryParse(v, NumberStyles.Integer, ci, out d.Streak); break;
                        case "lastday": d.LastDay = id; break;
                        case "char": d.Character = Characters.Normalize(v); break;
                    }
                }
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

        public static void Save() { S.Save(); Dirty = false; }

        // Raised for every achievement unlocked (its reward is already granted).
        public static event Action<Achievement> Unlocked;

        // returns true when a new loot box was earned
        public static bool AddKey()
        {
            S.Keys++;
            Dirty = true;
            bool box = false;
            if (S.Keys % BoxEvery == 0) { S.Boxes++; Save(); box = true; }
            CheckAchievements();
            return box;
        }

        public static void AddClick() { S.Clicks++; Dirty = true; CheckAchievements(); }

        public static void ReportKpm(int kpm)
        {
            if (kpm <= S.BestKpm) return;
            S.BestKpm = kpm;
            Dirty = true;
            CheckAchievements();
        }

        // Counts consecutive calendar days the app was running.
        public static void TouchDay()
        {
            DateTime today = DateTime.Today;
            string key = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (S.LastDay == key) return;
            string yesterday = today.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            S.Streak = S.LastDay == yesterday ? S.Streak + 1 : 1;
            S.LastDay = key;
            Save();
            CheckAchievements();
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
            Save();
            CheckAchievements();
            return it;
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
            new Achievement("ach_legendary", "Королевская коллекция", "Собрать все 15 legendary-предметов", 15, 0, true, OwnedLegendary),
            new Achievement("ach_speed550", "Бог скорости", "Нажать 550 клавиш за одну минуту", 550, 0, true, s => s.BestKpm),
        };

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

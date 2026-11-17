using System;
using System.Collections.Generic;
using System.Globalization;

namespace BongoBro
{
    // The New Year event runs from 15 December to 15 January.
    public static class NewYear
    {
        public static bool? DevOverride;   // developer mode can force it on or off

        public static bool Active
        {
            get
            {
                if (DevOverride.HasValue) return DevOverride.Value;
                DateTime d = DateTime.Today;
                return (d.Month == 12 && d.Day >= 15) || (d.Month == 1 && d.Day <= 15);
            }
        }

        public const string Period = "с 15 декабря по 15 января";
    }

    public class Quest
    {
        public int Index;
        public string Text;
        public long Target;
        public Func<SaveData, long> Progress;
    }

    // Three random quests a day, the same for everybody on that date. Each one gives a loot box,
    // finishing all three gives one more.
    public static class Quests
    {
        public const int Count = 3, BonusBoxes = 1;

        // raised when a quest is finished (its reward is already granted); allDone = it was the last one
        public static event Action<Quest, bool> Completed;

        static string builtFor;
        static List<Quest> today = new List<Quest>();
        static string todayChar;

        static string TodayKey { get { return DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); } }

        public static List<Quest> Today { get { Build(); return today; } }

        // character of today's "play as" quest, or null
        public static string Character { get { Build(); return todayChar; } }

        public static DateTime ResetAt { get { return DateTime.Today.AddDays(1); } }

        static void Build()
        {
            string key = TodayKey;
            if (builtFor == key) return;
            builtFor = key;
            today = new List<Quest>();
            todayChar = null;
            DateTime d = DateTime.Today;
            Random rnd = new Random(d.Year * 10000 + d.Month * 100 + d.Day);
            List<int> kinds = new List<int> { 0, 1, 2, 3, 4 };
            for (int i = 0; i < Count; i++)
            {
                int k = kinds[rnd.Next(kinds.Count)];
                kinds.Remove(k);
                Quest q = new Quest();
                q.Index = i;
                switch (k)
                {
                    case 0:
                        q.Target = new[] { 3000, 5000, 8000 }[rnd.Next(3)];
                        q.Text = "Нажать " + Ui.Num(q.Target) + " клавиш";
                        q.Progress = s => s.QKeys;
                        break;
                    case 1:
                        q.Target = new[] { 300, 600, 1000 }[rnd.Next(3)];
                        q.Text = "Кликнуть мышью " + Ui.Num(q.Target) + " раз";
                        q.Progress = s => s.QClicks;
                        break;
                    case 2:
                        todayChar = Characters.Ids[rnd.Next(Characters.Ids.Length)];
                        q.Target = 1000;
                        q.Text = "Нажать 1 000 клавиш за " + Characters.NameWhom(todayChar);
                        q.Progress = s => s.QCharKeys;
                        break;
                    case 3:
                        q.Target = new[] { 150, 200, 250 }[rnd.Next(3)];
                        q.Text = "Разогнаться до " + q.Target + " нажатий в минуту";
                        q.Progress = s => s.QKpm;
                        break;
                    default:
                        q.Target = 1 + rnd.Next(2);
                        q.Text = q.Target == 1 ? "Открыть лутбокс" : "Открыть " + q.Target + " лутбокса";
                        q.Progress = s => s.QOpened;
                        break;
                }
                today.Add(q);
            }
        }

        public static bool IsDone(SaveData s, Quest q) { return (s.QDone & (1 << q.Index)) != 0; }

        public static int DoneCount(SaveData s)
        {
            Refresh(s);
            int n = 0;
            foreach (Quest q in Today) if (IsDone(s, q)) n++;
            return n;
        }

        // starts a new set of quests when the date changes
        public static void Refresh(SaveData s)
        {
            Build();
            if (s.QDay == builtFor) return;
            s.QDay = builtFor;
            s.QKeys = s.QClicks = s.QCharKeys = 0;
            s.QKpm = s.QOpened = s.QDone = 0;
            Game.Dirty = true;
        }

        public static void OnKey(SaveData s)
        {
            Refresh(s);
            s.QKeys++;
            if (s.Character == todayChar) s.QCharKeys++;
            Check(s);
        }

        public static void OnClick(SaveData s) { Refresh(s); s.QClicks++; Check(s); }

        public static void OnKpm(SaveData s, int kpm)
        {
            Refresh(s);
            if (kpm <= s.QKpm) return;
            s.QKpm = kpm;
            Check(s);
        }

        public static void OnBoxOpened(SaveData s) { Refresh(s); s.QOpened++; Check(s); }

        static void Check(SaveData s)
        {
            foreach (Quest q in today)
            {
                if (IsDone(s, q) || q.Progress(s) < q.Target) continue;
                s.QDone |= 1 << q.Index;
                s.Boxes++;
                bool all = (s.QDone & ((1 << Count) - 1)) == (1 << Count) - 1;
                if (all) s.Boxes += BonusBoxes;
                Game.Save();
                if (Completed != null) Completed(q, all);
            }
        }

        // developer mode: finish every quest of the day
        public static void CompleteAll(SaveData s)
        {
            Refresh(s);
            s.QKeys = s.QClicks = s.QCharKeys = 1000000;
            s.QKpm = 10000;
            s.QOpened = 100;
            Check(s);
        }
    }

    // Pets grow while they are worn: one experience point per key press.
    public static class PetLevels
    {
        public const int Max = 3;
        static readonly long[] Need = { 0, 5000, 20000 };
        static readonly string[] Names = { "Малыш", "Подросток", "Взрослый" };

        public static event Action<string, int> LevelUp;

        public static long Xp(SaveData s, string pet)
        {
            long x;
            return pet != null && s.PetXp.TryGetValue(pet, out x) ? x : 0;
        }

        public static int Level(long xp)
        {
            int l = 1;
            for (int i = 1; i < Need.Length; i++) if (xp >= Need[i]) l = i + 1;
            return l;
        }

        public static int LevelOf(SaveData s, string pet) { return Level(Xp(s, pet)); }

        public static string Name(int level) { return Names[Math.Max(1, Math.Min(Max, level)) - 1]; }

        // 0..1 towards the next level, 1 at the top level
        public static float Progress(long xp)
        {
            int l = Level(xp);
            if (l >= Max) return 1f;
            return (xp - Need[l - 1]) / (float)(Need[l] - Need[l - 1]);
        }

        public static long ToNext(long xp)
        {
            int l = Level(xp);
            return l >= Max ? 0 : Need[l] - xp;
        }

        public static void AddXp(SaveData s)
        {
            if (s.Pet == null) return;
            long x = Xp(s, s.Pet);
            int before = Level(x);
            s.PetXp[s.Pet] = x + 1;
            int after = Level(x + 1);
            if (after > before && LevelUp != null) LevelUp(s.Pet, after);
        }

        // developer mode helper
        public static void Give(SaveData s, string pet, long xp)
        {
            if (pet == null) return;
            long x = Xp(s, pet);
            int before = Level(x);
            s.PetXp[pet] = x + xp;
            if (Level(x + xp) > before && LevelUp != null) LevelUp(pet, Level(x + xp));
        }
    }

    // Keys per day and per key, for the statistics window. Only counts are kept, never the order of keys.
    public static class Stats
    {
        static DateTime day;
        static string dayKey;

        public static string DayKey(DateTime d) { return d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); }

        public static void CountKey(SaveData s, int vk)
        {
            if (DateTime.Today != day) { day = DateTime.Today; dayKey = DayKey(day); }
            long n;
            s.History.TryGetValue(dayKey, out n);
            s.History[dayKey] = n + 1;
            s.KeyCounts.TryGetValue(vk, out n);
            s.KeyCounts[vk] = n + 1;
        }

        public static long KeysOn(SaveData s, DateTime d)
        {
            long n;
            return s.History.TryGetValue(DayKey(d), out n) ? n : 0;
        }
    }
}

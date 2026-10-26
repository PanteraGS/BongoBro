using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace BongoBro
{
    // Talks to the leaderboard Worker (server/ folder). Only the nickname, the character
    // and the total key presses leave the computer; the id/token pair proves ownership.
    public static class Leaderboard
    {
        public const string BaseUrl = "https://bongobro-leaderboard.bongobro-leaderboard.workers.dev";
        public const int SubmitEveryMs = 3600 * 1000;

        public class Entry
        {
            public int Rank;
            public string Name, Char;
            public long Keys;
            public bool Me;
        }

        public class Board
        {
            public DateTime Updated, Next;
            public int Players;
            public List<Entry> Top = new List<Entry>();
            public Entry Me;
        }

        public static long LastSubmittedKeys = -1;

        public static void EnsureIdentity(SaveData s)
        {
            if (s.PlayerId != null && s.PlayerToken != null) return;
            s.PlayerId = RandomHex(16);
            s.PlayerToken = RandomHex(32);
            Game.Save();
        }

        static string RandomHex(int bytes)
        {
            byte[] b = new byte[bytes];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider()) rng.GetBytes(b);
            StringBuilder sb = new StringBuilder();
            foreach (byte x in b) sb.Append(x.ToString("x2"));
            return sb.ToString();
        }

        static WebClient Client()
        {
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            wc.Headers[HttpRequestHeader.UserAgent] = "BongoBro/" + Program.Version;
            return wc;
        }

        // done(null) on success, otherwise an error code from the server or "offline".
        public static void SubmitAsync(Control ui, string name, Action<string> done)
        {
            // sandbox numbers from developer mode must never reach the real table
            if (Game.DevMode) { Post(ui, () => done("dev_mode")); return; }
            SaveData s = Game.S;
            EnsureIdentity(s);
            Dictionary<string, object> body = new Dictionary<string, object>();
            body["id"] = s.PlayerId;
            body["token"] = s.PlayerToken;
            body["name"] = name;
            body["keys"] = s.Keys;
            body["char"] = s.Character;
            string payload = new JavaScriptSerializer().Serialize(body);
            long keys = s.Keys;
            ThreadPool.QueueUserWorkItem(delegate
            {
                string err = null;
                try
                {
                    using (WebClient wc = Client())
                    {
                        wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                        wc.UploadString(BaseUrl + "/submit", "POST", payload);
                    }
                }
                catch (WebException ex) { err = ErrorCode(ex); }
                catch { err = "offline"; }
                if (err == null) LastSubmittedKeys = keys;
                Post(ui, () => done(err));
            });
        }

        public static void FetchAsync(Control ui, Action<Board> done)
        {
            string id = Game.S.PlayerId;
            ThreadPool.QueueUserWorkItem(delegate
            {
                Board b = null;
                try
                {
                    using (WebClient wc = Client())
                    {
                        string json = wc.DownloadString(BaseUrl + "/leaderboard" + (id != null ? "?id=" + id : ""));
                        b = Parse(json);
                    }
                }
                catch { }
                Post(ui, () => done(b));
            });
        }

        static void Post(Control ui, Action a)
        {
            try { if (!ui.IsDisposed) ui.BeginInvoke((MethodInvoker)delegate { a(); }); } catch { }
        }

        static string ErrorCode(WebException ex)
        {
            try
            {
                HttpWebResponse r = ex.Response as HttpWebResponse;
                if (r == null) return "offline";
                using (StreamReader rd = new StreamReader(r.GetResponseStream(), Encoding.UTF8))
                {
                    Dictionary<string, object> d = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(rd.ReadToEnd());
                    object e;
                    return d != null && d.TryGetValue("error", out e) ? Convert.ToString(e) : "http_" + (int)r.StatusCode;
                }
            }
            catch { return "offline"; }
        }

        static Board Parse(string json)
        {
            Dictionary<string, object> d = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(json);
            Board b = new Board();
            b.Updated = FromUnixMs(d["updated"]);
            b.Next = FromUnixMs(d["next"]);
            b.Players = Convert.ToInt32(d["players"]);
            ArrayList top = d["top"] as ArrayList;
            if (top != null) foreach (object o in top) b.Top.Add(ToEntry(o as Dictionary<string, object>));
            b.Me = d["me"] != null ? ToEntry(d["me"] as Dictionary<string, object>) : null;
            return b;
        }

        static Entry ToEntry(Dictionary<string, object> o)
        {
            Entry e = new Entry();
            e.Rank = Convert.ToInt32(o["rank"]);
            e.Name = Convert.ToString(o["name"]);
            e.Char = Convert.ToString(o["char"]);
            e.Keys = Convert.ToInt64(o["keys"]);
            e.Me = Convert.ToBoolean(o["me"]);
            return e;
        }

        static DateTime FromUnixMs(object v)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToDouble(v)).ToLocalTime();
        }

        public static string ErrorText(string code)
        {
            switch (code)
            {
                case "name_taken": return "Этот ник уже занят, выбери другой.";
                case "bad_name": return "Ник: 2–16 символов, буквы, цифры, пробел, _ . -";
                case "offline": return "Нет связи с сервером. Проверь интернет.";
                case "too_often": return "Слишком часто, попробуй через минуту.";
                case "dev_mode": return "В режиме разработчика таблица только для просмотра.";
                default: return "Ошибка сервера: " + code;
            }
        }
    }
}

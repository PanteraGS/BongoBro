using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace BongoBro
{
    // Checks GitHub Releases for a newer BongoBro.exe and swaps the running exe for it.
    // A running exe cannot be deleted but can be renamed, so the old file is moved aside
    // as BongoBro.old.exe and removed on the next start.
    public static class Updater
    {
        public const string Repo = "PanteraGS/BongoBro";
        const string AssetName = "BongoBro.exe";

        public static string LatestVersion, Notes;
        static string downloadUrl;
        static WebClient download;

        public static bool Available
        {
            get { return LatestVersion != null && downloadUrl != null && Compare(LatestVersion, Program.Version) > 0; }
        }

        public static string OldExePath
        {
            get { return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "BongoBro.old.exe"); }
        }

        // Ends a previous instance still running from the renamed BongoBro.old.exe.
        public static void KillOldProcess()
        {
            int me = Process.GetCurrentProcess().Id;
            List<Process> procs = new List<Process>(Process.GetProcessesByName("BongoBro.old"));
            procs.AddRange(Process.GetProcessesByName("BongoBro"));
            foreach (Process p in procs)
            {
                try
                {
                    string path = p.MainModule.FileName;
                    bool ours = string.Equals(path, OldExePath, StringComparison.OrdinalIgnoreCase)
                             || string.Equals(path, Application.ExecutablePath, StringComparison.OrdinalIgnoreCase);
                    if (p.Id != me && ours)
                    {
                        p.Kill();
                        p.WaitForExit(3000);
                    }
                }
                catch { }
                finally { p.Dispose(); }
            }
        }

        public static void CleanupOld()
        {
            try { if (File.Exists(OldExePath)) File.Delete(OldExePath); } catch { }
        }

        static void EnableTls()
        {
            // GitHub requires TLS 1.2, which .NET 4 does not enable by default.
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
        }

        // Queries the latest release on a background thread; done(ok) runs on the UI thread.
        public static void CheckAsync(Control ui, Action<bool> done)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                bool ok = false;
                string ver = null, notes = null, url = null;
                try
                {
                    EnableTls();
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers[HttpRequestHeader.UserAgent] = "BongoBro/" + Program.Version;
                        wc.Headers[HttpRequestHeader.Accept] = "application/vnd.github+json";
                        wc.Encoding = Encoding.UTF8;
                        string json = wc.DownloadString("https://api.github.com/repos/" + Repo + "/releases/latest");
                        Dictionary<string, object> rel = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(json);
                        ver = (rel["tag_name"] as string ?? "").TrimStart('v', 'V');
                        notes = rel.ContainsKey("body") ? rel["body"] as string : null;
                        ArrayList assets = rel["assets"] as ArrayList;
                        if (assets != null)
                            foreach (object o in assets)
                            {
                                Dictionary<string, object> a = o as Dictionary<string, object>;
                                if (a != null && string.Equals(a["name"] as string, AssetName, StringComparison.OrdinalIgnoreCase))
                                    url = a["browser_download_url"] as string;
                            }
                        ok = true;
                    }
                }
                catch { }
                try
                {
                    ui.BeginInvoke((MethodInvoker)delegate
                    {
                        if (ok) { LatestVersion = ver; Notes = notes; downloadUrl = url; }
                        done(ok);
                    });
                }
                catch { }
            });
        }

        // Downloads the new exe, swaps it in and restarts. progress(0..100) and fail(message) run on the UI thread.
        public static void DownloadAndRestart(Action<int> progress, Action<string> fail)
        {
            if (download != null || downloadUrl == null) return;
            string exe = Application.ExecutablePath;
            string tmp = exe + ".download";
            try { if (File.Exists(tmp)) File.Delete(tmp); }
            catch (Exception ex) { fail("Не удалось подготовить папку: " + ex.Message); return; }

            EnableTls();
            download = new WebClient();
            download.Headers[HttpRequestHeader.UserAgent] = "BongoBro/" + Program.Version;
            download.DownloadProgressChanged += (s, e) => progress(e.ProgressPercentage);
            download.DownloadFileCompleted += (s, e) =>
            {
                download.Dispose();
                download = null;
                if (e.Error != null || e.Cancelled) { fail("Не удалось скачать: " + (e.Error != null ? e.Error.Message : "отменено")); return; }
                string err = Swap(exe, tmp);
                if (err != null) { fail(err); return; }
                if (Game.S != null) Game.Save();
                try { Process.Start(exe, "--updated"); } catch (Exception ex) { fail("Обновление установлено, но не удалось перезапустить: " + ex.Message); return; }
                // close the main window (the normal exit path) rather than Application.Exit()
                if (Program.MainWin != null && !Program.MainWin.IsDisposed) Program.MainWin.Close();
                else Application.Exit();
            };
            download.DownloadFileAsync(new Uri(downloadUrl), tmp);
        }

        // Returns null on success or an error message; rolls back if the swap fails halfway.
        static string Swap(string exe, string tmp)
        {
            try
            {
                FileInfo fi = new FileInfo(tmp);
                byte[] head = new byte[2];
                using (FileStream fs = File.OpenRead(tmp)) fs.Read(head, 0, 2);
                if (fi.Length < 50 * 1024 || head[0] != 'M' || head[1] != 'Z') return "Скачанный файл повреждён, обновление отменено.";
            }
            catch (Exception ex) { return "Не удалось проверить скачанный файл: " + ex.Message; }

            CleanupOld();
            try { File.Move(exe, OldExePath); }
            catch (Exception ex) { return "Нет доступа к папке программы (" + ex.Message + "). Перенеси BongoBro.exe, например, в Документы."; }
            try { File.Move(tmp, exe); }
            catch (Exception ex)
            {
                try { File.Move(OldExePath, exe); } catch { }
                return "Не удалось заменить файл: " + ex.Message;
            }
            return null;
        }

        public static int Compare(string a, string b)
        {
            string[] x = a.Split('.'), y = b.Split('.');
            for (int i = 0; i < Math.Max(x.Length, y.Length); i++)
            {
                int u = 0, v = 0;
                if (i < x.Length) int.TryParse(x[i], out u);
                if (i < y.Length) int.TryParse(y[i], out v);
                if (u != v) return u.CompareTo(v);
            }
            return 0;
        }
    }
}

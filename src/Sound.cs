using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace BongoBro
{
    // Tiny built-in synthesizer: every sound is generated as a WAV in memory (volume baked in)
    // and played asynchronously through winmm's PlaySound. No sound files ship with the app.
    public static class Sound
    {
        public static readonly string[] Modes = { "off", "bongo", "mech", "soft", "chip" };

        public static string ModeName(string m)
        {
            switch (m)
            {
                case "bongo": return "Бонго";
                case "mech": return "Механика";
                case "soft": return "Мягкий";
                case "chip": return "8-бит";
                default: return "Без звука клавиш";
            }
        }

        [DllImport("winmm.dll", SetLastError = true)]
        static extern bool PlaySound(IntPtr pszSound, IntPtr hmod, uint fdwSound);
        const uint SND_ASYNC = 0x1, SND_NODEFAULT = 0x2, SND_MEMORY = 0x4;

        const int Rate = 22050;
        static readonly Dictionary<string, GCHandle> cache = new Dictionary<string, GCHandle>();
        static float cachedVolume = -1;
        static readonly Random rnd = new Random();
        static int variant;

        public static void Key(bool right)
        {
            string m = Game.S.SoundMode;
            if (m == "off") return;
            variant = (variant + 1) % 3;
            Play(m + (right ? "_r" : "_l") + variant);
        }

        public static void Click()
        {
            string m = Game.S.SoundMode;
            if (m == "off") return;
            Play(m + "_click");
        }

        // "box" (loot box earned), "ach" (achievement), "item" (item revealed), "rare" (legendary/immortal revealed)
        public static void Event(string kind)
        {
            if (!Game.S.EventSounds) return;
            Play("ev_" + kind);
        }

        // plays a sound regardless of the current mode (developer panel, menu preview)
        public static void Preview(string name) { Play(name); }

        static void Play(string name)
        {
            float vol = Game.S.Volume;
            if (vol <= 0.001f) return;
            try
            {
                if (Math.Abs(vol - cachedVolume) > 0.001f) ClearCache(vol);
                GCHandle h;
                if (!cache.TryGetValue(name, out h))
                {
                    byte[] wav = Wav(Synth(name), vol);
                    h = GCHandle.Alloc(wav, GCHandleType.Pinned);   // must stay put while it plays
                    cache[name] = h;
                }
                PlaySound(h.AddrOfPinnedObject(), IntPtr.Zero, SND_ASYNC | SND_MEMORY | SND_NODEFAULT);
            }
            catch { }
        }

        static void ClearCache(float vol)
        {
            PlaySound(IntPtr.Zero, IntPtr.Zero, 0);   // stop anything still reading the old buffers
            foreach (GCHandle h in cache.Values) h.Free();
            cache.Clear();
            cachedVolume = vol;
        }

        // ---------------- synthesis ----------------
        static float[] Buf(double seconds) { return new float[(int)(Rate * seconds)]; }

        static float[] Synth(string name)
        {
            if (name.StartsWith("ev_")) return EventSynth(name.Substring(3));
            int us = name.IndexOf('_');
            string mode = name.Substring(0, us), what = name.Substring(us + 1);
            bool click = what == "click";
            bool right = what.StartsWith("r");
            int v = click ? 0 : what[1] - '0';
            double detune = 1 + (v - 1) * 0.04;   // three slightly different variants per key
            switch (mode)
            {
                case "bongo": return Bongo(click ? 330 : (right ? 230 : 165) * detune);
                case "mech": return Mech(click ? 1.4 : detune);
                case "soft": return Pop(click ? 900 : (right ? 620 : 520) * detune);
                default: return Chip(click ? 1320 : (right ? 880 : 660) * detune);
            }
        }

        static float[] Bongo(double f0)
        {
            float[] b = Buf(0.2);
            double ph = 0;
            for (int i = 0; i < b.Length; i++)
            {
                double t = i / (double)Rate;
                double f = f0 * (1 + 0.6 * Math.Exp(-t * 60));   // quick pitch drop like a hand drum
                ph += 2 * Math.PI * f / Rate;
                double body = Math.Sin(ph) * Math.Exp(-t * 22);
                double slap = (rnd.NextDouble() * 2 - 1) * Math.Exp(-t * 300) * 0.5;
                b[i] = (float)(body * 0.9 + slap);
            }
            return b;
        }

        static float[] Mech(double tone)
        {
            float[] b = Buf(0.07);
            double prev = 0;
            for (int i = 0; i < b.Length; i++)
            {
                double t = i / (double)Rate;
                double n = rnd.NextDouble() * 2 - 1;
                double hp = n - prev; prev = n;                          // crude high-pass: crisp click
                double click = hp * Math.Exp(-t * 160) * 0.6;
                double ping = Math.Sin(2 * Math.PI * 2400 * tone * t) * Math.Exp(-t * 500) * 0.5;
                double thock = Math.Sin(2 * Math.PI * 280 * tone * t) * Math.Exp(-t * 90) * (t > 0.012 ? 0.6 : 0);
                b[i] = (float)(click + ping + thock);
            }
            return b;
        }

        static float[] Pop(double f0)
        {
            float[] b = Buf(0.11);
            double ph = 0;
            for (int i = 0; i < b.Length; i++)
            {
                double t = i / (double)Rate;
                ph += 2 * Math.PI * f0 * (1 + 0.5 * t * 8) / Rate;
                double env = Math.Min(1, t / 0.003) * Math.Exp(-t * 40);
                b[i] = (float)(Math.Sin(ph) * env * 0.8);
            }
            return b;
        }

        static float[] Chip(double f)
        {
            float[] b = Buf(0.05);
            for (int i = 0; i < b.Length; i++)
            {
                double t = i / (double)Rate;
                double sq = Math.Sin(2 * Math.PI * f * t) >= 0 ? 1 : -1;
                b[i] = (float)(sq * 0.35 * (1 - t / 0.05));
            }
            return b;
        }

        static float[] EventSynth(string kind)
        {
            double[] notes; double len;
            switch (kind)
            {
                case "ach": notes = new double[] { 392, 523, 659, 784, 1047 }; len = 0.11; break;
                case "rare": notes = new double[] { 523, 659, 784, 1047, 1319, 1568 }; len = 0.07; break;
                case "item": notes = new double[] { 659, 880 }; len = 0.08; break;
                default: notes = new double[] { 523, 659, 784, 1047 }; len = 0.08; break;   // box
            }
            float[] b = Buf(len * notes.Length + 0.35);
            for (int n = 0; n < notes.Length; n++)
            {
                int start = (int)(n * len * Rate);
                bool last = n == notes.Length - 1;
                double dur = last ? len + 0.35 : len * 1.6;
                for (int i = 0; i < (int)(dur * Rate) && start + i < b.Length; i++)
                {
                    double t = i / (double)Rate;
                    double env = Math.Min(1, t / 0.004) * Math.Exp(-t * (last ? 7 : 18));
                    double tri = 2 / Math.PI * Math.Asin(Math.Sin(2 * Math.PI * notes[n] * t));
                    double shimmer = Math.Sin(2 * Math.PI * notes[n] * 2 * t) * 0.25;
                    b[start + i] += (float)((tri + shimmer) * env * 0.45);
                }
            }
            return b;
        }

        // 16-bit mono PCM WAV with the volume applied
        static byte[] Wav(float[] samples, float volume)
        {
            float gain = 0.9f * volume * volume;   // perceptual-ish curve for the slider
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter w = new BinaryWriter(ms))
            {
                int dataLen = samples.Length * 2;
                w.Write(new char[] { 'R', 'I', 'F', 'F' }); w.Write(36 + dataLen);
                w.Write(new char[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
                w.Write(16); w.Write((short)1); w.Write((short)1); w.Write(Rate); w.Write(Rate * 2); w.Write((short)2); w.Write((short)16);
                w.Write(new char[] { 'd', 'a', 't', 'a' }); w.Write(dataLen);
                foreach (float s in samples)
                {
                    float v = Math.Max(-1f, Math.Min(1f, s * gain));
                    w.Write((short)(v * 32000));
                }
                w.Flush();
                return ms.ToArray();
            }
        }
    }
}

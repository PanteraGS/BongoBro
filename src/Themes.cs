using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BongoBro
{
    public enum ThemeFx { None, Retro, Sakura, Ocean, Marble, Cyber, Candy, Rgb, Cosmos }

    // Colors of the keyboard, mouse and key counter ("Стиль" slot).
    public class Theme
    {
        public Color KbTop, KbBot, KbFront, KbInk, Key, KeyShade, Lit, LitShade;
        public Color MouseTop, MouseBot, PadTop, PadBot, PillBg, PillBorder, PillText, Bar;
        public ThemeFx Fx;

        public Theme(ThemeFx fx, int kbTop, int kbBot, int kbFront, int key, int keyShade, int lit, int mouse, int padTop, int padBot, int pillBg, int pillBorder, int pillText, int bar)
        {
            Fx = fx;
            KbTop = C(kbTop); KbBot = C(kbBot); KbFront = C(kbFront); KbInk = Art.Mix(KbFront, Color.Black, 0.5f);
            Key = C(key); KeyShade = C(keyShade); Lit = C(lit); LitShade = Art.Mix(Lit, Color.Black, 0.25f);
            MouseTop = Art.Mix(C(mouse), Color.White, 0.25f); MouseBot = Art.Mix(C(mouse), Color.Black, 0.12f);
            PadTop = C(padTop); PadBot = C(padBot);
            PillBg = Color.FromArgb(225, C(pillBg)); PillBorder = C(pillBorder); PillText = C(pillText); Bar = C(bar);
        }

        static Color C(int rgb) { return Color.FromArgb(255, (rgb >> 16) & 255, (rgb >> 8) & 255, rgb & 255); }
    }

    public static partial class Art
    {
        public static readonly Theme DefaultTheme = new Theme(ThemeFx.None, 0x3E4256, 0x282B38, 0x161820, 0xF0F2F8, 0x8C92A8, 0xFFD166, 0xEEF0F6, 0x3E5084, 0x222C50, 0x161824, 0x7C5CFF, 0xFFFFFF, 0xFFB020);

        static readonly Dictionary<string, Theme> themes = new Dictionary<string, Theme>
        {
            { "th_snow",     new Theme(ThemeFx.None,   0xF4F6FA, 0xD6DAE4, 0xBEC4D2, 0xFFFFFF, 0xC4CAD8, 0x82C8FF, 0xFFFFFF, 0xDCE2EE, 0xBEC6D6, 0xF4F6FC, 0xB4BED2, 0x283040, 0x5AAAFF) },
            { "th_mint",     new Theme(ThemeFx.None,   0x96E6C8, 0x60C4A0, 0x3C9676, 0xFAFFFC, 0x96D2BE, 0xFF8CAA, 0xD2FAEC, 0x3CAA8C, 0x28826C, 0x184638, 0x96E6C8, 0xFFFFFF, 0x96FFD2) },
            { "th_peach",    new Theme(ThemeFx.None,   0xFFC8AA, 0xF0A082, 0xC87860, 0xFFF6EE, 0xE2BAAA, 0xFF6E5A, 0xFFE6D6, 0xFFA08C, 0xDC786E, 0x6E3228, 0xFFBEA0, 0xFFF0E6, 0xFF8C6E) },
            { "th_graphite", new Theme(ThemeFx.None,   0x28282C, 0x16161A, 0x0A0A0C, 0x34363C, 0x141418, 0xF0F0F0, 0x3C3E46, 0x1E1E22, 0x101012, 0x0A0A0C, 0x5A5A64, 0xE6E6EB, 0xC8C8D2) },
            { "th_lavender", new Theme(ThemeFx.None,   0xC8B4FA, 0xA088E6, 0x6E5AB4, 0xFAF6FF, 0xC4B4E8, 0xFFDC6E, 0xECE2FF, 0x9678DC, 0x6E56BE, 0x3C286E, 0xC8B4FA, 0xFFFFFF, 0xDCBEFF) },
            { "th_retro",    new Theme(ThemeFx.Retro,  0xE8DCBE, 0xCEC0A0, 0xA09274, 0xF6EEDA, 0xBEB092, 0xFF963C, 0xEDE3C9, 0x7A5A3C, 0x543C26, 0x3C3228, 0xE8783C, 0xF6E8C8, 0xFF963C) },
            { "th_sakura",   new Theme(ThemeFx.Sakura, 0xFFD6E2, 0xF0AAC4, 0xC87896, 0xFFFAFC, 0xE8B8CA, 0xFF5A96, 0xFFE8F0, 0xFFE4EE, 0xF4BCD0, 0xFFF0F6, 0xFF8CB4, 0x8C2850, 0xFF6EA0) },
            { "th_ocean",    new Theme(ThemeFx.Ocean,  0x2878C8, 0x144696, 0x0C2E64, 0xE6FAFF, 0x6EA0C8, 0xFFE678, 0xC8ECFF, 0x145AA0, 0x0A326E, 0x0A2850, 0x50D2FF, 0xFFFFFF, 0x50DCFF) },
            { "th_matcha",   new Theme(ThemeFx.None,   0xAAC878, 0x78A050, 0x50703A, 0xF6F4E2, 0xB4BE96, 0x8C5A3C, 0xF0ECD2, 0x3C5A2C, 0x263C1C, 0x283C1E, 0xAAC878, 0xF0F4DC, 0xC8E68C) },
            { "th_marble",   new Theme(ThemeFx.Marble, 0xFFE282, 0xC88C1E, 0x8C5A14, 0xFAFAF8, 0xBEBCB8, 0xFFC832, 0xFAFAF6, 0x1E1A24, 0x0C0A10, 0x14101A, 0xFFC850, 0xFFE28C, 0xFFC850) },
            { "th_cyber",    new Theme(ThemeFx.Cyber,  0x202024, 0x0A0A0E, 0xFCE628, 0xFCE628, 0xAA9600, 0x00F0FF, 0xFCE628, 0x101014, 0x050508, 0x08080C, 0x00F0FF, 0xFCE628, 0x00F0FF) },
            { "th_candy",    new Theme(ThemeFx.Candy,  0xFFF0F8, 0xF8D2E6, 0xE6A0C8, 0xFFFFFF, 0xD2AAC8, 0xFFFFFF, 0xFFDCEC, 0xFFC8E0, 0xC8E6FF, 0xFFE6F4, 0xFF8CC8, 0xB43C82, 0xFF8CC8) },
            { "th_rgb",      new Theme(ThemeFx.Rgb,    0x18181C, 0x08080A, 0x000000, 0x26262C, 0x0C0C10, 0xFFFFFF, 0x1C1C22, 0x101014, 0x050506, 0x08080A, 0xFF0080, 0xFFFFFF, 0xFF0080) },
            { "th_cosmos",   new Theme(ThemeFx.Cosmos, 0x2A1050, 0x080418, 0x05020E, 0x2E2864, 0x120E30, 0xC8F0FF, 0x1A1238, 0x3C1C6E, 0x0A0620, 0x1A0C3C, 0xFF6EDC, 0xFFFFFF, 0x78E6FF) },
        };

        public static Theme GetTheme(string id)
        {
            Theme th;
            if (id != null && themes.TryGetValue(id, out th)) return th;
            return DefaultTheme;
        }

        // The keyboard faces him, not the viewer: the space-bar row is the far (top) row on screen.
        const int SpaceRow = 0;
        static readonly float[][] KeyRows = {
            new float[] { 1, 1, 1.5f, 5, 1.5f, 1, 1 },
            new float[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            new float[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            new float[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        public const int KeyCount = 43;

        static readonly Color[] CandyKeys = {
            Color.FromArgb(255, 182, 210), Color.FromArgb(178, 236, 214), Color.FromArgb(255, 238, 160),
            Color.FromArgb(178, 214, 255), Color.FromArgb(220, 196, 255) };

        static float Lerp(float a, float b, float k) { return a + (b - a) * k; }

        static void KeyColors(Theme th, int idx, int row, float xk, float wu, float t, bool lit, out Color top, out Color shade)
        {
            if (lit)
            {
                top = th.Lit; shade = th.LitShade;
                return;
            }
            switch (th.Fx)
            {
                case ThemeFx.Rgb:
                    top = Hsv(t * 140 - xk * 320 - row * 30, 0.75f, 1f);
                    shade = Hsv(t * 140 - xk * 320 - row * 30, 0.85f, 0.45f);
                    return;
                case ThemeFx.Candy:
                    top = CandyKeys[(idx * 3 + row) % CandyKeys.Length];
                    shade = Mix(top, Color.FromArgb(120, 60, 110), 0.35f);
                    return;
                case ThemeFx.Ocean:
                    top = Mix(Color.FromArgb(232, 250, 255), Color.FromArgb(140, 214, 250), row / 3f);
                    shade = Mix(top, Color.FromArgb(20, 70, 150), 0.5f);
                    return;
                case ThemeFx.Retro:
                    // his Esc: the far-left key from his side = bottom-right on screen
                    if (idx == KeyCount - 1) { top = Color.FromArgb(232, 120, 60); shade = Color.FromArgb(160, 70, 30); return; }
                    if (xk < 0.07f || xk > 0.93f || (row == SpaceRow && wu != 5))
                    {
                        top = Color.FromArgb(128, 126, 132); shade = Color.FromArgb(78, 76, 84);
                        return;
                    }
                    break;
                case ThemeFx.Cosmos:
                    top = Mix(th.Key, Hsv(250 + 40 * Sin(t * 2 + idx), 0.6f, 0.6f), 0.35f);
                    shade = th.KeyShade;
                    return;
            }
            top = th.Key; shade = th.KeyShade;
        }

        static void DrawDesk(Graphics g, Pose p)
        {
            Theme th = GetTheme(p.Theme);
            float t = p.T;
            Glow(g, 170, 346, 160, 16, Color.Black, 80);
            PointF[] body = { new PointF(46, 268), new PointF(284, 268), new PointF(302, 338), new PointF(28, 338) };
            if (th.Fx == ThemeFx.Rgb || th.Fx == ThemeFx.Cyber || th.Fx == ThemeFx.Cosmos)
                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddPolygon(new PointF[] { body[0], body[1], new PointF(302, 348), new PointF(28, 348) });
                    Color gc = th.Fx == ThemeFx.Rgb ? Hsv(t * 100, 0.9f, 1f) : th.Fx == ThemeFx.Cyber ? Color.FromArgb(0, 240, 255) : Color.FromArgb(255, 110, 220);
                    GlowPath(g, gp, gc, th.Fx == ThemeFx.Cyber ? 0.6f : 0.8f + 0.2f * Sin(t * 3));
                }
            using (GraphicsPath front = new GraphicsPath())
            {
                front.AddPolygon(new PointF[] { new PointF(28, 338), new PointF(302, 338), new PointF(300, 348), new PointF(30, 348) });
                Fill(g, th.KbFront, front);
                if (th.Fx == ThemeFx.Cyber)
                {
                    GraphicsState st = g.Save();
                    g.SetClip(front, CombineMode.Intersect);
                    using (SolidBrush bk = new SolidBrush(Color.FromArgb(20, 20, 20)))
                        for (float x = 20; x < 310; x += 14) g.FillPolygon(bk, new PointF[] { new PointF(x, 348), new PointF(x + 7, 348), new PointF(x + 15, 338), new PointF(x + 8, 338) });
                    g.Restore(st);
                }
                Stroke(g, th.KbInk, 3f, front);
            }
            using (GraphicsPath kb = new GraphicsPath())
            {
                kb.AddPolygon(body);
                Grad(g, kb, th.KbTop, th.KbBot);
                GraphicsState st = g.Save();
                g.SetClip(kb, CombineMode.Intersect);
                if (th.Fx == ThemeFx.Cosmos) StarField(g, new RectangleF(28, 268, 274, 70), t, 3);
                if (th.Fx == ThemeFx.Marble) Glow(g, 120, 272, 70, 10, Color.White, 150);
                g.Restore(st);
                Stroke(g, th.Fx == ThemeFx.Cyber ? Color.FromArgb(0, 240, 255) : th.KbInk, 3f, kb);
            }
            if (th.Fx == ThemeFx.Sakura)
            {
                Petal(g, 40, 330, 30); Petal(g, 290, 274, -20); Petal(g, 52, 276, 70);
            }
            int idx = 0;
            for (int r = 0; r < 4; r++)
            {
                float y0 = 274 + r * 15.6f, y1 = y0 + 12.4f;
                float lx0 = Lerp(46, 28, (y0 - 268) / 70f) + 7, rx0 = Lerp(284, 302, (y0 - 268) / 70f) - 7;
                float lx1 = Lerp(46, 28, (y1 - 268) / 70f) + 7, rx1 = Lerp(284, 302, (y1 - 268) / 70f) - 7;
                float u = 0, gap = 1.3f;
                foreach (float wu in KeyRows[r])
                {
                    float a = u / 12f, b = (u + wu) / 12f;
                    bool on = idx == p.LitKey;
                    float dy = on ? 1.6f : 0;
                    PointF[] q = {
                        new PointF(Lerp(lx0, rx0, a) + gap, y0 + dy), new PointF(Lerp(lx0, rx0, b) - gap, y0 + dy),
                        new PointF(Lerp(lx1, rx1, b) - gap, y1 + dy), new PointF(Lerp(lx1, rx1, a) + gap, y1 + dy) };
                    PointF[] s = new PointF[4];
                    for (int i = 0; i < 4; i++) s[i] = new PointF(q[i].X, q[i].Y + 2.4f - dy);
                    Color top, shade;
                    KeyColors(th, idx, r, (a + b) / 2, wu, t, on, out top, out shade);
                    float kcx = (q[0].X + q[1].X) / 2, kcy = (q[0].Y + q[2].Y) / 2;
                    if (th.Fx == ThemeFx.Rgb && !on) Glow(g, kcx, kcy, 13, 10, top, 70);
                    using (SolidBrush sb = new SolidBrush(shade)) g.FillPolygon(sb, s);
                    using (SolidBrush tb = new SolidBrush(top)) g.FillPolygon(tb, q);
                    if (th.Fx == ThemeFx.Marble && idx % 3 != 1)
                        Line(g, Color.FromArgb(90, 120, 120, 130), 0.8f, q[0].X + 2 + (idx % 4), q[0].Y + 1, q[2].X - 3, q[2].Y - 2 - (idx % 3));
                    if (th.Fx == ThemeFx.Cosmos)
                        using (Pen ep = new Pen(Color.FromArgb(150, Hsv(190 + 60 * Sin(t * 2 + idx * 0.4f), 0.6f, 1f)), 1f)) g.DrawPolygon(ep, q);
                    if (on) Glow(g, kcx, kcy, 14, 10, Mix(th.Lit, Color.White, 0.3f), 140);
                    idx++;
                    u += wu;
                }
            }
            if (th.Fx == ThemeFx.Sakura) Petal(g, 200, 300, 40);
            DrawMouse(g, p, th);
        }

        static void Petal(Graphics g, float x, float y, float rot)
        {
            GraphicsState st = g.Save();
            g.TranslateTransform(x, y);
            g.RotateTransform(rot);
            using (GraphicsPath pp = new GraphicsPath())
            {
                pp.AddBezier(0, -5, 4, -5, 5, 2, 0, 5);
                pp.AddBezier(0, 5, -5, 2, -4, -5, 0, -5);
                Fill(g, Color.FromArgb(230, 255, 160, 196), pp);
                Stroke(g, Color.FromArgb(200, 220, 100, 150), 0.8f, pp);
            }
            g.Restore(st);
        }

        static void StarField(Graphics g, RectangleF r, float t, int seed)
        {
            Random rnd = new Random(seed);
            for (int i = 0; i < 28; i++)
            {
                float x = r.X + (float)rnd.NextDouble() * r.Width, y = r.Y + (float)rnd.NextDouble() * r.Height;
                float tw = 0.5f + 0.5f * Sin(t * (2 + rnd.NextDouble() * 3) + i);
                float s = 0.8f + (float)rnd.NextDouble() * 1.4f;
                FillEl(g, A((int)(80 + 175 * tw), Color.White), x - s / 2, y - s / 2, s, s);
            }
            Glow(g, r.X + r.Width * 0.3f, r.Y + r.Height * 0.5f, r.Width * 0.25f, r.Height * 0.6f, Color.FromArgb(255, 90, 200), 60);
            Glow(g, r.X + r.Width * 0.75f, r.Y + r.Height * 0.4f, r.Width * 0.2f, r.Height * 0.6f, Color.FromArgb(80, 200, 255), 50);
        }

        static void DrawMouse(Graphics g, Pose p, Theme th)
        {
            float t = p.T;
            RectangleF padR = new RectangleF(4, 282, 60, 54);
            using (GraphicsPath pad = RR(padR, 14))
            {
                if (th.Fx == ThemeFx.Rgb) GlowPath(g, pad, Hsv(t * 100 + 180, 0.9f, 1f), 0.8f);
                if (th.Fx == ThemeFx.Cyber) GlowPath(g, pad, Color.FromArgb(0, 240, 255), 0.5f);
                Grad(g, pad, th.PadTop, th.PadBot);
                GraphicsState st = g.Save();
                g.SetClip(pad, CombineMode.Intersect);
                if (th.Fx == ThemeFx.Cosmos) StarField(g, padR, t, 9);
                if (th.Fx == ThemeFx.Candy)
                    using (SolidBrush sb = new SolidBrush(Color.FromArgb(120, 255, 255, 255)))
                        for (float x = -12; x < 68; x += 14) g.FillPolygon(sb, new PointF[] { new PointF(x, 336), new PointF(x + 6, 336), new PointF(x + 30, 282), new PointF(x + 24, 282) });
                if (th.Fx == ThemeFx.Ocean)
                    using (Pen w = P(Color.FromArgb(90, 160, 220, 255), 1.4f))
                        for (int i = 0; i < 3; i++) g.DrawBezier(w, 4, 296 + i * 14, 18, 290 + i * 14, 34, 302 + i * 14, 64, 294 + i * 14);
                g.Restore(st);
                Color edge = th.Fx == ThemeFx.Marble ? Color.FromArgb(255, 200, 80) : th.Fx == ThemeFx.Rgb ? Hsv(t * 100 + 180, 0.9f, 1f) : Mix(th.PadBot, Color.Black, 0.45f);
                Stroke(g, edge, 2.5f, pad);
            }
            if (th.Fx == ThemeFx.Sakura) Petal(g, 14, 328, 30);

            // wireless mouse with its buttons pointing away from him (towards the viewer)
            GraphicsState ms = g.Save();
            g.TranslateTransform(MouseX(p), 309 + MouseYOff(p));
            g.RotateTransform(180);
            using (GraphicsPath m = RR(new RectangleF(-12, -17, 24, 34), 11))
            {
                Grad(g, m, th.MouseBot, th.MouseTop);
                GraphicsState st = g.Save();
                g.SetClip(m, CombineMode.Intersect);
                if (p.MouseDown)
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(110, 40, 44, 70))) g.FillRectangle(b, -12, -17, 12, 14);
                if (th.Fx == ThemeFx.Cosmos) StarField(g, new RectangleF(-12, -17, 24, 34), t, 5);
                if (th.Fx == ThemeFx.Candy)
                    using (SolidBrush sb = new SolidBrush(Color.FromArgb(150, 255, 140, 200))) g.FillRectangle(sb, -12, 2, 24, 4);
                FillEl(g, A(140, Color.White), 3, 3, 5, 10);
                g.Restore(st);
                Color ink = th.Fx == ThemeFx.Marble ? Color.FromArgb(200, 150, 40) : Color.FromArgb(40, 42, 52);
                Stroke(g, ink, 2.2f, m);
            }
            Line(g, Color.FromArgb(120, 124, 140), 1.4f, 0, -17, 0, -4);
            using (GraphicsPath w = RR(new RectangleF(-2, -13, 4, 7), 2)) Fill(g, Color.FromArgb(90, 96, 110), w);
            if (th.Fx == ThemeFx.Rgb || th.Fx == ThemeFx.Cyber || th.Fx == ThemeFx.Cosmos)
            {
                Color lc = th.Fx == ThemeFx.Rgb ? Hsv(t * 140, 0.8f, 1f) : th.Fx == ThemeFx.Cyber ? Color.FromArgb(0, 240, 255) : Color.FromArgb(255, 120, 230);
                Glow(g, 0, 7, 9, 9, lc, 220);
                FillEl(g, Mix(lc, Color.White, 0.5f), -2.5f, 4.5f, 5, 5);
            }
            if (th.Fx == ThemeFx.Marble) Line(g, Color.FromArgb(220, 170, 50), 1.6f, -9, 8, 9, 8);
            g.Restore(ms);
        }

        // Key counter pill, styled by the theme.
        public static void DrawCounter(Graphics g, RectangleF r, Theme th, string text, float progress, float t)
        {
            using (GraphicsPath p = RR(r, r.Height * 0.36f))
            {
                if (th.Fx == ThemeFx.Rgb) GlowPath(g, p, Hsv(t * 100, 0.9f, 1f), 0.8f);
                else if (th.Fx == ThemeFx.Cosmos) GlowPath(g, p, Color.FromArgb(255, 110, 220), 0.6f + 0.3f * Sin(t * 3));
                else if (th.Fx == ThemeFx.Cyber) GlowPath(g, p, th.PillBorder, 0.5f);
                else if (th.Fx == ThemeFx.Marble) GlowPath(g, p, th.PillBorder, 0.3f);
                Color bg2 = th.Fx == ThemeFx.Cosmos ? Color.FromArgb(235, 60, 20, 90) : th.Fx == ThemeFx.Candy ? Color.FromArgb(235, 214, 236, 255) : Mix(th.PillBg, Color.Black, 0.15f);
                using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(r.X, r.Y - 1, r.Width, r.Height + 2), th.PillBg, bg2, th.Fx == ThemeFx.Candy ? 0f : 90f)) g.FillPath(b, p);
                if (th.Fx == ThemeFx.Cosmos)
                {
                    GraphicsState st = g.Save();
                    g.SetClip(p, CombineMode.Intersect);
                    StarField(g, r, t, 7);
                    g.Restore(st);
                }
                if (th.Fx == ThemeFx.Rgb)
                    using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(r.X - 1, r.Y, r.Width + 2, r.Height), Hsv(t * 100, 0.9f, 1f), Hsv(t * 100 + 160, 0.9f, 1f), 0f))
                    using (Pen pen = new Pen(b, 2.2f)) g.DrawPath(pen, p);
                else using (Pen pen = new Pen(th.PillBorder, 1.8f)) g.DrawPath(pen, p);
            }
            float k = r.Height / 40f;
            using (GraphicsPath kb = RR(new RectangleF(r.X + 9 * k, r.Y + 9 * k, 22 * k, 14 * k), 3 * k))
            {
                Fill(g, Mix(th.PillText, th.PillBg, 0.1f), kb);
                using (SolidBrush d = new SolidBrush(th.PillBg))
                {
                    for (int i = 0; i < 4; i++) g.FillRectangle(d, r.X + (12 + i * 4.5f) * k, r.Y + 12 * k, 3 * k, 3 * k);
                    g.FillRectangle(d, r.X + 14 * k, r.Y + 17.5f * k, 12 * k, 2.5f * k);
                }
            }
            Ui.TextL(g, text, Ui.F(17 * k, true), th.PillText, r.X + 34 * k, r.Y);
            RectangleF bar = new RectangleF(r.X + 9 * k, r.Bottom - 11 * k, r.Width - 17 * k, 5 * k);
            Ui.FillRR(g, bar, bar.Height / 2, A(120, Mix(th.PillBg, th.PillText, 0.3f)));
            if (progress > 0)
            {
                RectangleF f = new RectangleF(bar.X, bar.Y, Math.Max(bar.Height, bar.Width * Math.Min(1f, progress)), bar.Height);
                Color c1 = th.Bar, c2 = Mix(th.Bar, Color.White, 0.4f);
                if (th.Fx == ThemeFx.Rgb) { c1 = Hsv(t * 120, 0.9f, 1f); c2 = Hsv(t * 120 + 120, 0.9f, 1f); }
                if (th.Fx == ThemeFx.Cosmos) { c1 = Color.FromArgb(255, 110, 220); c2 = th.Bar; }
                using (GraphicsPath fp = RR(f, f.Height / 2))
                using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(f.X - 1, f.Y, f.Width + 2, f.Height), c1, c2, 0f))
                    g.FillPath(b, fp);
            }
        }

        public static void DrawThemePreview(Graphics g, string id, RectangleF dst, float t)
        {
            GraphicsState st = Fit(g, new RectangleF(0, 196, 312, 158), dst);
            Pose p = new Pose();
            p.T = t;
            p.Theme = id;
            p.LitKey = (int)(t * 3) % KeyCount;
            DrawCounter(g, new RectangleF(72, 204, 168, 42), GetTheme(id), "12 345", 0.6f, t);
            DrawDesk(g, p);
            g.Restore(st);
        }
    }
}

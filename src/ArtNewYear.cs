using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BongoBro
{
    // New Year event: the scene (garland and snow) and the New Year collection.
    public static partial class Art
    {
        static readonly Color SantaRed = Color.FromArgb(214, 32, 44);
        static readonly Color SantaRedDark = Color.FromArgb(140, 14, 24);
        static readonly Color Fur = Color.FromArgb(250, 250, 246);
        static readonly Color FurShade = Color.FromArgb(206, 212, 222);
        static readonly Color Pine = Color.FromArgb(40, 150, 70);
        static readonly Color PineDark = Color.FromArgb(18, 92, 44);
        static readonly Color[] Bulbs = {
            Color.FromArgb(255, 70, 80), Color.FromArgb(255, 210, 60), Color.FromArgb(80, 200, 255),
            Color.FromArgb(120, 230, 110), Color.FromArgb(255, 120, 220) };

        public static void Snowflake(Graphics g, float cx, float cy, float r, Color c, float w)
        {
            using (Pen p = P(c, w))
                for (int i = 0; i < 6; i++)
                {
                    double a = i * Math.PI / 3;
                    float ex = cx + Cos(a) * r, ey = cy + Sin(a) * r;
                    g.DrawLine(p, cx, cy, ex, ey);
                    float mx = cx + Cos(a) * r * 0.6f, my = cy + Sin(a) * r * 0.6f;
                    g.DrawLine(p, mx, my, mx + Cos(a + 0.8) * r * 0.3f, my + Sin(a + 0.8) * r * 0.3f);
                    g.DrawLine(p, mx, my, mx + Cos(a - 0.8) * r * 0.3f, my + Sin(a - 0.8) * r * 0.3f);
                }
        }

        // ======================= scene =======================
        // a garland of blinking bulbs along the top of the window
        static void NewYearBack(Graphics g, float t)
        {
            PointF[] wire = new PointF[9];
            for (int i = 0; i < wire.Length; i++)
                wire[i] = new PointF(6 + i * 43.5f, 14 + (i % 2 == 0 ? 0 : 16));
            using (Pen w = P(Color.FromArgb(40, 90, 50), 2f)) g.DrawCurve(w, wire, 0.6f);
            for (int i = 0; i < 16; i++)
            {
                float x = 12 + i * 22.4f;
                float y = YOnWire(x);
                Color c = Bulbs[i % Bulbs.Length];
                bool on = ((int)(t * 2.2f) + i) % 3 != 0;
                if (on) Glow(g, x, y + 7, 9, 9, c, 150);
                using (GraphicsPath b = new GraphicsPath())
                {
                    b.AddEllipse(x - 3.5f, y + 2, 7, 10);
                    Fill(g, on ? Mix(c, Color.White, 0.25f) : Mix(c, Color.Black, 0.45f), b);
                }
                FillEl(g, Color.FromArgb(70, 70, 76), x - 2.5f, y, 5, 3.5f);
            }
        }

        // the garland wire sags between the hooks
        static float YOnWire(float x)
        {
            float seg = (x - 6) / 43.5f;
            float frac = seg - (float)Math.Floor(seg);
            int k = (int)Math.Floor(seg);
            float a = k % 2 == 0 ? 14 : 30, b = k % 2 == 0 ? 30 : 14;
            float s = (1 - (float)Math.Cos(frac * Math.PI)) / 2;
            return a + (b - a) * s;
        }

        static void Snowfall(Graphics g, float t)
        {
            for (int i = 0; i < 28; i++)
            {
                float speed = 0.06f + (i % 5) * 0.012f;
                float ph = (t * speed + i * 0.137f) % 1f;
                float x = (i * 53 + 17) % 360 + Sin(t * 0.9 + i) * 10;
                float y = -10 + ph * 380;
                float r = 1.6f + (i % 4) * 0.8f;
                int a = (int)(220 * Math.Min(1f, (1 - ph) * 4));
                if (i % 7 == 0) Snowflake(g, x, y, r * 2.2f, A(a, Color.White), 1.1f);
                else FillEl(g, A(a, Color.White), x - r, y - r, r * 2, r * 2);
            }
        }

        // ======================= hats =======================
        static void SantaHat(Graphics g)
        {
            using (GraphicsPath cone = new GraphicsPath())
            {
                cone.AddBezier(108, 96, 114, 52, 150, 26, 196, 30);
                cone.AddBezier(196, 30, 232, 34, 258, 52, 266, 78);
                cone.AddBezier(266, 78, 250, 64, 236, 62, 228, 72);
                cone.AddBezier(228, 72, 240, 82, 248, 92, 252, 98);
                cone.CloseFigure();
                Grad(g, cone, Color.FromArgb(240, 60, 66), SantaRedDark);
                Glow(g, 156, 52, 26, 12, Color.White, 70);
                Stroke(g, SantaRedDark, 2.2f, cone);
            }
            FurBall(g, 266, 82, 13);
            using (GraphicsPath band = RR(new RectangleF(98, 84, 164, 26), 13)) FurShape(g, band);
        }

        static void FurShape(Graphics g, GraphicsPath p)
        {
            Grad(g, p, Fur, FurShade);
            RectangleF r = p.GetBounds();
            GraphicsState st = g.Save();
            g.SetClip(p, CombineMode.Intersect);
            Random rnd = new Random((int)r.X * 7 + (int)r.Y);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(120, 190, 198, 212)))
                for (int i = 0; i < (int)(r.Width * r.Height / 40); i++)
                    g.FillEllipse(b, r.X + (float)rnd.NextDouble() * r.Width, r.Y + (float)rnd.NextDouble() * r.Height, 4, 3);
            g.Restore(st);
            Stroke(g, Color.FromArgb(160, 168, 182), 1.6f, p);
        }

        static void FurBall(Graphics g, float cx, float cy, float r)
        {
            using (GraphicsPath b = new GraphicsPath()) { b.AddEllipse(cx - r, cy - r, r * 2, r * 2); FurShape(g, b); }
        }

        static void Antlers(Graphics g)
        {
            Color c = Color.FromArgb(150, 96, 52), d = Color.FromArgb(86, 50, 22);
            using (Pen o = P(d, 9f))
            using (Pen f = P(c, 6f))
                for (int k = 0; k < 2; k++)
                {
                    float s = k == 0 ? -1 : 1;
                    PointF[][] lines = {
                        new[] { new PointF(180 + 46 * s, 60), new PointF(180 + 62 * s, 30), new PointF(180 + 58 * s, 6) },
                        new[] { new PointF(180 + 58 * s, 40), new PointF(180 + 80 * s, 30), new PointF(180 + 88 * s, 12) },
                        new[] { new PointF(180 + 61 * s, 22), new PointF(180 + 44 * s, 12), new PointF(180 + 40 * s, 0) } };
                    foreach (PointF[] l in lines) g.DrawCurve(o, l);
                    foreach (PointF[] l in lines) g.DrawCurve(f, l);
                }
            // headband
            Bez(g, Color.FromArgb(70, 40, 20), 6f, 112, 104, 124, 44, 236, 44, 248, 104);
            Bez(g, Color.FromArgb(120, 70, 36), 3f, 112, 104, 124, 44, 236, 44, 248, 104);
            FillEl(g, Color.FromArgb(255, 70, 80), 172, 40, 16, 12);
        }

        // immortal: a small fir tree with a glowing star and running lights
        static void TreeHat(Graphics g, float t)
        {
            float pulse = 0.6f + 0.4f * Sin(t * 3);
            Glow(g, 180, 4, 30, 30, Color.FromArgb(255, 220, 90), (int)(170 * pulse));
            for (int tier = 2; tier >= 0; tier--)
            {
                float top = 6 + tier * 30, bot = top + 44, half = 46 + tier * 18;
                using (GraphicsPath tr = new GraphicsPath())
                {
                    tr.AddLine(180, top, 180 + half, bot);
                    tr.AddBezier(180 + half, bot, 180 + half * 0.4f, bot + 7, 180 - half * 0.4f, bot + 7, 180 - half, bot);
                    tr.CloseFigure();
                    Grad(g, tr, Color.FromArgb(70, 180, 90), PineDark);
                    Stroke(g, Color.FromArgb(10, 60, 26), 2f, tr);
                }
            }
            // running garland lights
            for (int i = 0; i < 14; i++)
            {
                float tier = i / 5, k = (i % 5) / 4f;
                float y = 40 + tier * 30 + k * 6, half = 34 + tier * 18;
                float x = 180 - half + k * half * 2;
                Color c = Bulbs[i % Bulbs.Length];
                bool on = ((int)(t * 6) + i) % 4 != 0;
                if (on) Glow(g, x, y, 7, 7, c, 170);
                FillEl(g, on ? Mix(c, Color.White, 0.3f) : Mix(c, Color.Black, 0.4f), x - 3, y - 3, 6, 6);
            }
            using (GraphicsPath st = Star(180, 2, 12, 5, 5, -90 + t * 20))
            {
                Grad(g, st, Color.FromArgb(255, 250, 200), Color.FromArgb(255, 180, 30));
                Stroke(g, Color.FromArgb(170, 110, 10), 1.4f, st);
            }
        }

        // ======================= glasses =======================
        static void SnowflakeGlasses(Graphics g)
        {
            Color ice = Color.FromArgb(150, 210, 255), deep = Color.FromArgb(60, 130, 200);
            foreach (float cx in new float[] { EyeL, EyeR })
            {
                using (GraphicsPath lens = new GraphicsPath())
                {
                    lens.AddEllipse(cx - 18, EyeY - 17, 36, 34);
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(70, 200, 235, 255))) g.FillPath(b, lens);
                }
                Snowflake(g, cx, EyeY, 24, deep, 4.4f);
                Snowflake(g, cx, EyeY, 24, ice, 2.4f);
                Line(g, A(170, Color.White), 1.6f, cx - 10, EyeY + 6, cx - 4, EyeY - 8);
            }
            Line(g, deep, 2.6f, EyeL + 18, EyeY - 2, EyeR - 18, EyeY - 2);
            Line(g, deep, 2.6f, EyeL - 22, EyeY - 2, 114, EyeY - 6);
            Line(g, deep, 2.6f, EyeR + 22, EyeY - 2, 246, EyeY - 6);
        }

        // ======================= gloves =======================
        static void Mittens(Graphics g, float tx)
        {
            using (LinearGradientBrush b = VBrush(Color.FromArgb(240, 60, 66), SantaRedDark)) HandShape(g, tx, b, SantaRedDark, 2.4f);
            Snowflake(g, 0, -3, 7, Color.FromArgb(240, 255, 255, 255), 1.4f);
            using (Pen k = P(A(70, Color.White), 1f))
                for (int i = -2; i <= 2; i++) g.DrawLine(k, i * 6, -15, i * 6, -11);
            using (GraphicsPath cf = RR(new RectangleF(-17, 7, 34, 12), 6)) FurShape(g, cf);
        }

        // ======================= face =======================
        static void FrostFace(Graphics g, float t)
        {
            Glow(g, 140, 170, 15, 10, Color.FromArgb(150, 200, 255), 150);
            Glow(g, 220, 170, 15, 10, Color.FromArgb(150, 200, 255), 150);
            Glow(g, 142, 168, 9, 6, Color.FromArgb(255, 140, 160), 120);
            Glow(g, 218, 168, 9, 6, Color.FromArgb(255, 140, 160), 120);
            Snowflake(g, 224, 172, 7, Color.FromArgb(230, 255, 255, 255), 1.4f);
            Sparkle(g, 132, 160, 3 + 1.5f * Sin(t * 4), Color.FromArgb(220, 240, 255));
        }

        static void Scarf(Graphics g)
        {
            using (GraphicsPath tail = new GraphicsPath())
            {
                tail.AddPolygon(new PointF[] { new PointF(206, 214), new PointF(224, 216), new PointF(232, 268), new PointF(212, 270) });
                StripeFill(g, tail, true);
            }
            using (GraphicsPath band = new GraphicsPath())
            {
                band.AddBezier(132, 200, 150, 222, 210, 222, 228, 200);
                band.AddLine(228, 200, 232, 216);
                band.AddBezier(232, 216, 210, 240, 150, 240, 128, 216);
                band.CloseFigure();
                StripeFill(g, band, false);
            }
        }

        static void StripeFill(Graphics g, GraphicsPath p, bool vertical)
        {
            Fill(g, SantaRed, p);
            RectangleF r = p.GetBounds();
            GraphicsState st = g.Save();
            g.SetClip(p, CombineMode.Intersect);
            using (SolidBrush w = new SolidBrush(Fur))
            using (SolidBrush gr = new SolidBrush(Pine))
                for (int i = 0; i < 12; i++)
                {
                    if (vertical) g.FillRectangle(i % 2 == 0 ? w : gr, r.X, r.Y + 8 + i * 14, r.Width, 5);
                    else g.FillRectangle(i % 2 == 0 ? w : gr, r.X + 8 + i * 16, r.Y, 5, r.Height);
                }
            g.Restore(st);
            Stroke(g, SantaRedDark, 1.8f, p);
        }

        // ======================= pet =======================
        static void Snowman(Graphics g, float t, bool sleep)
        {
            float wave = sleep ? 0 : Sin(t * 3) * 10;
            using (Pen arm = P(Color.FromArgb(110, 70, 36), 2.2f))
            {
                g.DrawLine(arm, -12, -22, -26, -32 - wave * 0.4f);
                g.DrawLine(arm, 12, -22, 26, -30);
            }
            Blob(g, new RectangleF(-17, -28, 34, 28), Color.White);
            Blob(g, new RectangleF(-12, -48, 24, 22), Color.White);
            // hat
            using (GraphicsPath h = RR(new RectangleF(-8, -60, 16, 13), 2)) { Fill(g, Color.FromArgb(40, 40, 46), h); Stroke(g, PetInk, 1.4f, h); }
            Line(g, Color.FromArgb(40, 40, 46), 3f, -12, -47, 12, -47);
            Line(g, SantaRed, 2.4f, -8, -51, 8, -51);
            // face
            PetEyes(g, -4, 4, -40, sleep, 1.8f);
            using (GraphicsPath nose = new GraphicsPath())
            {
                nose.AddPolygon(new PointF[] { new PointF(-1, -37), new PointF(-1, -33), new PointF(10, -36) });
                Fill(g, Color.FromArgb(255, 140, 40), nose);
            }
            Cheeks(g, -7, 7, -33);
            // scarf and buttons
            using (GraphicsPath sc = RR(new RectangleF(-11, -29, 22, 6), 3)) Fill(g, SantaRed, sc);
            Line(g, SantaRed, 4f, 6, -26, 9, -16);
            FillEl(g, PetInk, -2, -18, 4, 4);
            FillEl(g, PetInk, -2, -10, 4, 4);
        }
    }
}

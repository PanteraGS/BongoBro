using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BongoBro
{
    // Personal gear sets: "Гриль-мастер" (Yura, sausages), "Бахча" (Vlad, watermelons)
    // and "Пивной фестиваль" (Stasyan, beer). Their bonus shows only on the owner.
    public static partial class Art
    {
        static readonly Color Sausage = Color.FromArgb(206, 92, 64);
        static readonly Color SausageDark = Color.FromArgb(140, 50, 34);
        static readonly Color Bun = Color.FromArgb(236, 178, 100);
        static readonly Color BunDark = Color.FromArgb(190, 120, 52);
        static readonly Color Mustard = Color.FromArgb(255, 210, 50);
        static readonly Color Rind = Color.FromArgb(70, 150, 60);
        static readonly Color RindDark = Color.FromArgb(34, 96, 36);
        static readonly Color MelonRed = Color.FromArgb(240, 70, 82);
        static readonly Color MelonRedDark = Color.FromArgb(190, 34, 50);

        // ======================= Гриль-мастер =======================
        static void SausageHat(Graphics g)
        {
            using (GraphicsPath back = new GraphicsPath())
            {
                back.AddEllipse(108, 26, 144, 38);
                Grad(g, back, Mix(Bun, Color.White, 0.2f), BunDark);
                Stroke(g, Ink, 2f, back);
            }
            SausageShape(g, new RectangleF(86, 30, 188, 28), 0);
            using (GraphicsPath front = RR(new RectangleF(100, 48, 160, 58), 28))
            {
                Grad(g, front, Mix(Bun, Color.White, 0.3f), BunDark);
                GraphicsState st = g.Save();
                g.SetClip(front, CombineMode.Intersect);
                Glow(g, 150, 60, 34, 8, Color.White, 90);
                // sesame seeds
                Random rnd = new Random(5);
                using (SolidBrush b = new SolidBrush(Color.FromArgb(255, 244, 214)))
                    for (int i = 0; i < 14; i++)
                    {
                        float x = 114 + (float)rnd.NextDouble() * 132, y = 56 + (float)rnd.NextDouble() * 24;
                        g.FillEllipse(b, x, y, 4.5f, 2.6f);
                    }
                g.Restore(st);
                Stroke(g, Ink, 2.2f, front);
            }
        }

        // a sausage lying in r, rotated by ang degrees around its center, with grill marks and mustard
        static void SausageShape(Graphics g, RectangleF r, float ang)
        {
            GraphicsState st = g.Save();
            g.TranslateTransform(r.X + r.Width / 2, r.Y + r.Height / 2);
            g.RotateTransform(ang);
            RectangleF l = new RectangleF(-r.Width / 2, -r.Height / 2, r.Width, r.Height);
            using (GraphicsPath s = RR(l, r.Height / 2))
            {
                Grad(g, s, Mix(Sausage, Color.White, 0.2f), SausageDark);
                GraphicsState c = g.Save();
                g.SetClip(s, CombineMode.Intersect);
                using (Pen mark = P(A(150, Color.FromArgb(90, 30, 20)), Math.Max(1.4f, r.Height * 0.12f)))
                    for (float x = l.X + r.Height * 0.8f; x < l.Right - r.Height * 0.6f; x += r.Height * 0.9f)
                        g.DrawLine(mark, x, l.Y + 2, x - r.Height * 0.4f, l.Bottom - 2);
                Line(g, A(110, Color.White), Math.Max(1.2f, r.Height * 0.12f), l.X + r.Height * 0.5f, l.Y + r.Height * 0.28f, l.Right - r.Height * 0.5f, l.Y + r.Height * 0.28f);
                g.Restore(c);
                Stroke(g, SausageDark, 1.8f, s);
            }
            // mustard zig-zag
            int n = Math.Max(3, (int)(r.Width / (r.Height * 0.7f)));
            PointF[] z = new PointF[n];
            for (int i = 0; i < n; i++)
                z[i] = new PointF(l.X + r.Height * 0.5f + i * (r.Width - r.Height) / (n - 1), i % 2 == 0 ? -r.Height * 0.12f : r.Height * 0.12f);
            using (Pen m = P(Mustard, Math.Max(1.6f, r.Height * 0.16f))) g.DrawLines(m, z);
            g.Restore(st);
        }

        // red checkered oven mitts
        static void SausageGloves(Graphics g, float tx)
        {
            Color d = Color.FromArgb(110, 20, 20);
            using (LinearGradientBrush b = VBrush(Color.FromArgb(236, 70, 64), Color.FromArgb(176, 34, 34))) HandShape(g, tx, b, d, 2.4f);
            using (GraphicsPath pp = PalmPath())
            {
                GraphicsState st = g.Save();
                g.SetClip(pp, CombineMode.Intersect);
                using (Pen w = P(A(150, Color.White), 3f))
                    for (int i = -3; i <= 3; i++) { g.DrawLine(w, i * 7, -20, i * 7, 20); g.DrawLine(w, -20, i * 7, 20, i * 7); }
                g.Restore(st);
            }
            FillEl(g, A(130, Color.White), -9, -12, 9, 5);
            Cuff(g, Color.FromArgb(250, 246, 236), d, 11);
        }

        // a dachshund lying in a hot-dog bun
        static void Dachshund(Graphics g, float t, bool sleep)
        {
            Color fur = Color.FromArgb(170, 92, 48), dark = Color.FromArgb(104, 52, 24);
            float wag = sleep ? 0 : Sin(t * 8) * 4;
            using (GraphicsPath back = new GraphicsPath()) { back.AddEllipse(-29, -20, 56, 16); Grad(g, back, Bun, BunDark); Stroke(g, PetInk, 1.8f, back); }
            using (Pen tail = P(PetInk, 5)) g.DrawBezier(tail, -20, -20, -26, -24, -30, -30 + wag, -32, -34 + wag);
            using (Pen tail = P(fur, 3)) g.DrawBezier(tail, -20, -20, -26, -24, -30, -30 + wag, -32, -34 + wag);
            using (GraphicsPath body = RR(new RectangleF(-24, -28, 40, 16), 8))
            {
                Grad(g, body, Mix(fur, Color.White, 0.2f), fur);
                Stroke(g, PetInk, 2f, body);
            }
            using (Pen m = P(Mustard, 2.4f)) g.DrawLines(m, new PointF[] { new PointF(-18, -22), new PointF(-12, -25), new PointF(-6, -21), new PointF(0, -25), new PointF(6, -21) });
            // head and long ear
            Blob(g, new RectangleF(6, -44, 22, 20), fur);
            using (GraphicsPath snout = new GraphicsPath()) { snout.AddEllipse(20, -36, 14, 10); Grad(g, snout, Mix(fur, Color.White, 0.25f), fur); Stroke(g, PetInk, 1.6f, snout); }
            FillEl(g, PetInk, 30, -34, 5, 4);
            using (GraphicsPath ear = new GraphicsPath())
            {
                ear.AddEllipse(5, -40, 9, 18);
                Fill(g, dark, ear);
                Stroke(g, PetInk, 1.4f, ear);
            }
            PetEyes(g, 18, 18, -36, sleep, 2.2f);
            Cheeks(g, 24, 24, -30);
            using (GraphicsPath front = RR(new RectangleF(-30, -14, 60, 14), 7))
            {
                Grad(g, front, Mix(Bun, Color.White, 0.25f), BunDark);
                Stroke(g, PetInk, 1.8f, front);
            }
        }

        // ======================= Бахча =======================
        static void MelonHat(Graphics g)
        {
            using (GraphicsPath d = new GraphicsPath())
            {
                d.AddBezier(102, 106, 96, 50, 144, 26, 180, 26);
                d.AddBezier(180, 26, 216, 26, 264, 50, 258, 106);
                d.AddBezier(258, 106, 220, 94, 140, 94, 102, 106);
                d.CloseFigure();
                Grad(g, d, Color.FromArgb(120, 196, 90), RindDark);
                GraphicsState st = g.Save();
                g.SetClip(d, CombineMode.Intersect);
                using (Pen s = P(A(210, Color.FromArgb(30, 92, 34)), 9f))
                    for (int i = -3; i <= 3; i++)
                        g.DrawBezier(s, 180 + i * 6, 22, 180 + i * 18 + 4, 50, 180 + i * 26 - 4, 80, 180 + i * 30, 108);
                Glow(g, 150, 46, 24, 12, Color.White, 90);
                g.Restore(st);
                Stroke(g, Ink, 2.2f, d);
            }
            // the cut edge: white rind, then the red flesh with seeds
            Bez(g, Color.FromArgb(240, 248, 220), 8f, 102, 104, 140, 92, 220, 92, 258, 104);
            Bez(g, MelonRed, 5f, 104, 108, 140, 97, 220, 97, 256, 108);
            Bez(g, Ink, 1.6f, 102, 111, 140, 100, 220, 100, 258, 111);
            for (int i = 0; i < 7; i++) FillEl(g, Color.FromArgb(30, 20, 20), 120 + i * 20, 97 + Math.Abs(i - 3) * 1.6f, 3, 4);
        }

        static void MelonGlasses(Graphics g)
        {
            foreach (float cx in new float[] { EyeL, EyeR })
            {
                RectangleF r = new RectangleF(cx - 21, EyeY - 24, 42, 44);
                using (GraphicsPath s = new GraphicsPath())
                {
                    s.AddArc(r, 0, 180);
                    s.CloseFigure();
                    Grad(g, s, Color.FromArgb(200, 255, 120, 128), Color.FromArgb(220, 210, 40, 60));
                    using (Pen rind = P(Rind, 5f)) g.DrawArc(rind, r.X + 1, r.Y + 1, r.Width - 2, r.Height - 2, 5, 170);
                    using (Pen white = P(Color.FromArgb(236, 250, 214), 2f)) g.DrawArc(white, r.X + 4, r.Y + 4, r.Width - 8, r.Height - 8, 8, 164);
                    Line(g, RindDark, 2.6f, r.X, r.Y + r.Height / 2, r.Right, r.Y + r.Height / 2);
                }
                foreach (PointF sd in new PointF[] { new PointF(cx - 9, EyeY + 6), new PointF(cx + 8, EyeY + 7), new PointF(cx, EyeY + 13) })
                    FillEl(g, Color.FromArgb(30, 20, 20), sd.X - 1.6f, sd.Y - 2.2f, 3.2f, 4.4f);
                Line(g, A(150, Color.White), 1.6f, cx - 12, EyeY + 2, cx - 6, EyeY + 10);
            }
            Bez(g, RindDark, 2.6f, EyeL + 21, EyeY - 2, EyeL + 24, EyeY - 6, EyeR - 24, EyeY - 6, EyeR - 21, EyeY - 2);
            Line(g, RindDark, 2.6f, EyeL - 21, EyeY - 2, 112, EyeY - 6);
            Line(g, RindDark, 2.6f, EyeR + 21, EyeY - 2, 248, EyeY - 6);
        }

        static void MelonGloves(Graphics g, float tx)
        {
            using (LinearGradientBrush b = VBrush(Color.FromArgb(130, 204, 96), Rind)) HandShape(g, tx, b, RindDark, 2.4f);
            using (GraphicsPath pp = PalmPath())
            {
                GraphicsState st = g.Save();
                g.SetClip(pp, CombineMode.Intersect);
                using (Pen s = P(A(200, Color.FromArgb(30, 92, 34)), 4f))
                    for (int i = -2; i <= 2; i++) g.DrawBezier(s, i * 8, -18, i * 8 + 4, -8, i * 8 - 4, 4, i * 8, 16);
                g.Restore(st);
            }
            FillEl(g, A(130, Color.White), -9, -12, 9, 5);
            Cuff(g, MelonRed, MelonRedDark, 10);
            for (int i = 0; i < 4; i++) FillEl(g, Color.FromArgb(30, 20, 20), -10 + i * 6.5f, 10.5f, 2.4f, 3.2f);
        }

        // a slice of watermelon, point up, centred on (x, y)
        static void MelonSlice(Graphics g, float x, float y, float ang, float s)
        {
            GraphicsState st = g.Save();
            g.TranslateTransform(x, y);
            g.RotateTransform(ang);
            g.ScaleTransform(s, s);
            using (GraphicsPath w = new GraphicsPath())
            {
                w.AddLine(0, -10, 9, 6);
                w.AddBezier(9, 6, 4, 9, -4, 9, -9, 6);
                w.CloseFigure();
                Fill(g, MelonRed, w);
            }
            using (Pen r = P(Rind, 3f)) g.DrawBezier(r, 10, 7, 4, 10.5f, -4, 10.5f, -10, 7);
            FillEl(g, Color.FromArgb(30, 20, 20), -3, 0, 2, 3);
            FillEl(g, Color.FromArgb(30, 20, 20), 2, -3, 2, 3);
            g.Restore(st);
        }

        // ======================= Пивной фестиваль =======================
        static void BeerHat(Graphics g, float t)
        {
            // straws from the cans down to the mouth (drawn first, under the helmet)
            using (Pen o = P(A(200, Color.FromArgb(150, 70, 60)), 4f))
            using (Pen f = P(A(230, Color.FromArgb(240, 90, 80)), 2.4f))
                for (int k = 0; k < 2; k++)
                {
                    float s = k == 0 ? 1 : -1;
                    g.DrawBezier(o, 180 - 84 * s, 86, 180 - 92 * s, 130, 180 - 46 * s, 170, 180 - 14 * s, 186);
                    g.DrawBezier(f, 180 - 84 * s, 86, 180 - 92 * s, 130, 180 - 46 * s, 170, 180 - 14 * s, 186);
                }
            using (GraphicsPath d = new GraphicsPath())
            {
                d.AddBezier(106, 100, 102, 52, 144, 30, 180, 30);
                d.AddBezier(180, 30, 216, 30, 258, 52, 254, 100);
                d.CloseFigure();
                Grad(g, d, Color.FromArgb(255, 226, 90), Color.FromArgb(226, 160, 20));
                Glow(g, 150, 50, 22, 12, Color.White, 110);
                Stroke(g, Color.FromArgb(120, 76, 10), 2.2f, d);
            }
            Line(g, Color.FromArgb(200, 140, 20), 4f, 180, 32, 180, 98);
            using (GraphicsPath brim = RR(new RectangleF(96, 94, 168, 14), 7))
            {
                Grad(g, brim, Color.FromArgb(255, 220, 80), Color.FromArgb(206, 140, 14));
                Stroke(g, Color.FromArgb(120, 76, 10), 2f, brim);
            }
            // two beer cans in holders on the sides
            foreach (float cx in new float[] { 96, 264 })
            {
                using (GraphicsPath can = RR(new RectangleF(cx - 12, 46, 24, 40), 5))
                {
                    Grad(g, can, Color.FromArgb(250, 250, 250), Color.FromArgb(176, 182, 192));
                    GraphicsState st = g.Save();
                    g.SetClip(can, CombineMode.Intersect);
                    using (SolidBrush lb = new SolidBrush(Beer)) g.FillRectangle(lb, cx - 12, 56, 24, 18);
                    using (SolidBrush fb = new SolidBrush(Foam)) g.FillRectangle(fb, cx - 12, 56, 24, 4);
                    Line(g, A(150, Color.White), 2.4f, cx - 7, 48, cx - 7, 84);
                    g.Restore(st);
                    Stroke(g, Color.FromArgb(80, 84, 94), 1.8f, can);
                }
                Line(g, Color.FromArgb(120, 76, 10), 3f, cx - 13, 70, cx + 13, 70);
            }
        }

        // a foam mustache on the upper lip
        static void BeerFoamMustache(Graphics g)
        {
            float[][] puffs = {
                new float[] { 156, 176, 13 }, new float[] { 166, 172, 15 }, new float[] { 180, 171, 16 },
                new float[] { 194, 172, 15 }, new float[] { 204, 176, 13 }, new float[] { 173, 180, 12 }, new float[] { 188, 180, 12 } };
            foreach (float[] p in puffs) FillEl(g, Color.FromArgb(200, 186, 150), p[0] - p[2] / 2 - 1, p[1] - p[2] / 2 - 1, p[2] + 2, p[2] + 2);
            foreach (float[] p in puffs)
            {
                using (GraphicsPath c = new GraphicsPath())
                {
                    c.AddEllipse(p[0] - p[2] / 2, p[1] - p[2] / 2, p[2], p[2]);
                    Grad(g, c, Color.White, FoamShade);
                }
            }
            FillEl(g, A(200, Color.White), 166, 167, 6, 4);
            using (GraphicsPath drip = new GraphicsPath())
            {
                drip.AddBezier(196, 180, 196, 186, 197, 190, 199, 192);
                drip.AddBezier(199, 192, 202, 190, 202, 186, 202, 180);
                drip.CloseFigure();
                Fill(g, Foam, drip);
            }
        }

        // white foam gloves with an amber beer cuff
        static void BeerGloves(Graphics g, float tx)
        {
            using (LinearGradientBrush b = VBrush(Color.White, FoamShade)) HandShape(g, tx, b, Color.FromArgb(170, 150, 110), 2.2f);
            using (SolidBrush pore = new SolidBrush(A(90, Color.FromArgb(200, 186, 150))))
            {
                g.FillEllipse(pore, -8, -6, 3, 3); g.FillEllipse(pore, 4, -10, 2.5f, 2.5f);
                g.FillEllipse(pore, 6, 2, 3, 3); g.FillEllipse(pore, -3, 4, 2, 2);
            }
            FingerLines(g, A(150, Color.FromArgb(190, 170, 130)), 1.4f);
            Cuff(g, Beer, BeerDeep, 11);
            using (Pen ring = P(A(200, Color.FromArgb(255, 236, 170)), 1f))
            {
                g.DrawEllipse(ring, -10, 10, 3, 3); g.DrawEllipse(ring, 1, 12, 4, 4); g.DrawEllipse(ring, 8, 9, 3, 3);
            }
        }

        // a cheerful hop cone
        static void HopCone(Graphics g, float t, bool sleep)
        {
            Color c = Color.FromArgb(150, 204, 80), d = Color.FromArgb(80, 140, 40);
            float sway = sleep ? 0 : Sin(t * 2.4) * 5;
            using (Pen stem = P(Color.FromArgb(90, 120, 40), 2.4f)) g.DrawBezier(stem, 0, -46, 0, -52, 4, -56, 6 + sway * 0.3f, -58);
            using (GraphicsPath leaf = new GraphicsPath())
            {
                leaf.AddBezier(2, -52, 8 + sway, -64, 22 + sway, -60, 20 + sway, -52);
                leaf.AddBezier(20 + sway, -52, 14, -48, 8, -48, 2, -52);
                Grad(g, leaf, Color.FromArgb(120, 190, 70), d);
                Stroke(g, PetInk, 1.4f, leaf);
            }
            using (GraphicsPath body = new GraphicsPath())
            {
                body.AddBezier(0, -48, 18, -46, 20, -16, 0, 0);
                body.AddBezier(0, 0, -20, -16, -18, -46, 0, -48);
                Grad(g, body, Mix(c, Color.White, 0.2f), d);
                GraphicsState st = g.Save();
                g.SetClip(body, CombineMode.Intersect);
                using (Pen sc = P(A(170, d), 1.6f))
                    for (int row = 0; row < 5; row++)
                        for (int k = -2; k <= 2; k++)
                        {
                            float x = k * 8 + (row % 2) * 4, y = -40 + row * 8;
                            g.DrawArc(sc, x - 6, y - 4, 12, 9, 20, 140);
                        }
                g.Restore(st);
                Stroke(g, PetInk, 2f, body);
            }
            PetEyes(g, -5, 5, -26, sleep, 2.3f);
            Cheeks(g, -9, 9, -20);
            SmallMouth(g, 0, -20);
        }

        // ======================= personal set bonuses =======================
        static void PersonalBonusBack(Graphics g, Pose p, string set, float t)
        {
            if (set == "set_sausage")
            {
                // heat of the grill below and smoke drifting up
                Glow(g, 180, 330, 200, 80, Color.FromArgb(255, 110, 40), 110);
                for (int i = 0; i < 8; i++)
                {
                    float ph = (t * 0.18f + i / 8f) % 1f;
                    float x = 40 + (i * 47) % 280 + Sin(t * 1.2 + i) * 12, y = 250 - ph * 240;
                    Glow(g, x, y, 14 + ph * 22, 10 + ph * 16, Color.FromArgb(200, 200, 210), (int)(110 * (1 - ph)));
                }
            }
            else if (set == "set_melon")
            {
                // a huge watermelon slice rises behind him like a sun
                using (GraphicsPath s = new GraphicsPath())
                {
                    s.AddPie(30, 10, 300, 300, 180, 180);
                    Fill(g, Color.FromArgb(190, 240, 80, 92), s);
                }
                using (Pen w = P(Color.FromArgb(200, 240, 250, 220), 8f)) g.DrawArc(w, 34, 14, 292, 292, 180, 180);
                using (Pen r = P(Color.FromArgb(220, 70, 150, 60), 10f)) g.DrawArc(r, 26, 6, 308, 308, 180, 180);
                Random rnd = new Random(3);
                for (int i = 0; i < 14; i++)
                {
                    double a = Math.PI + rnd.NextDouble() * Math.PI;
                    float rr = 40 + (float)rnd.NextDouble() * 100;
                    FillEl(g, Color.FromArgb(200, 30, 20, 20), 180 + (float)Math.Cos(a) * rr, 160 + (float)Math.Sin(a) * rr, 5, 7);
                }
            }
            else if (set == "set_beer")
            {
                // bubbles rising through the background
                for (int i = 0; i < 16; i++)
                {
                    float ph = (t * 0.22f + i / 16f) % 1f;
                    float x = 14 + (i * 61) % 332 + Sin(t * 2 + i) * 6, y = 330 - ph * 330, d = 5 + (i % 4) * 3;
                    using (Pen ring = P(Color.FromArgb((int)(200 * (1 - ph * 0.6f)), 255, 220, 120), 1.6f)) g.DrawEllipse(ring, x, y, d, d);
                    FillEl(g, A((int)(170 * (1 - ph)), Color.White), x + d * 0.2f, y + d * 0.15f, d * 0.35f, d * 0.3f);
                }
                // Bavarian pennant garland
                PointF[] rope = new PointF[11];
                for (int i = 0; i < rope.Length; i++)
                {
                    float x = 10 + i * 34;
                    rope[i] = new PointF(x, 24 + (float)Math.Sin(i / 10.0 * Math.PI) * 22 + Sin(t * 1.5 + i * 0.4) * 1.5f);
                }
                using (Pen r = P(Color.FromArgb(120, 90, 60), 1.6f)) g.DrawCurve(r, rope);
                for (int i = 0; i + 1 < rope.Length; i++)
                {
                    PointF a = rope[i], b = rope[i + 1];
                    PointF tip = new PointF((a.X + b.X) / 2, (a.Y + b.Y) / 2 + 22);
                    using (GraphicsPath f = new GraphicsPath())
                    {
                        f.AddPolygon(new PointF[] { new PointF(a.X + 3, a.Y), new PointF(b.X - 3, b.Y), tip });
                        Fill(g, i % 2 == 0 ? Color.FromArgb(40, 120, 220) : Color.White, f);
                        Stroke(g, Color.FromArgb(90, 70, 50), 1f, f);
                    }
                }
            }
        }

        static void PersonalBonusFront(Graphics g, Pose p, string set, float t)
        {
            float k = 1 - p.PetHop;
            int a = (int)(255 * p.PetHop);
            if (set == "set_sausage" && p.PetHop > 0)
            {
                // a sausage on a fork jumps out of the grill on every key press
                float y = 262 - Sin(k * Math.PI) * 80;
                GraphicsState st = g.Save();
                g.TranslateTransform(56, y);
                g.RotateTransform(-20 + k * 40);
                using (Pen fork = P(A(a, Color.FromArgb(190, 196, 206)), 2.4f))
                {
                    g.DrawLine(fork, 0, 6, 0, 34);
                    g.DrawLine(fork, -4, 6, -4, -2); g.DrawLine(fork, 4, 6, 4, -2); g.DrawLine(fork, -4, 6, 4, 6);
                }
                g.Restore(st);
                if (a > 40) SausageShape(g, new RectangleF(36, y - 14, 40, 13), -20 + k * 40);
                for (int i = 0; i < 4; i++)
                    Sparkle(g, 30 + i * 18 + k * (i - 1.5f) * 14, 270 - k * (40 + i * 8), 2.5f, A(a, Color.FromArgb(255, 170, 60)));
            }
            else if (set == "set_melon")
            {
                // watermelon slices drift down
                for (int i = 0; i < 9; i++)
                {
                    float ph = (t * 0.1f + i / 9f) % 1f;
                    float x = (i * 83) % 360 + Sin(t * 1.1 + i) * 14;
                    MelonSlice(g, x, -14 + ph * 380, t * 70 + i * 50, 0.9f + (i % 3) * 0.2f);
                }
                if (p.PetHop > 0)
                    for (int i = 0; i < 5; i++)
                    {
                        float x = 150 + i * 15 + k * (i - 2) * 26, y = 272 - Sin(k * Math.PI) * (40 + i % 2 * 14);
                        FillEl(g, A(a, Color.FromArgb(30, 20, 20)), x - 2, y - 3, 4, 6);
                    }
            }
            else if (set == "set_beer" && p.PetHop > 0)
            {
                // foam splashes over the keyboard
                for (int i = 0; i < 6; i++)
                {
                    float x = 140 + i * 16 + k * (i - 2.5f) * 22, y = 272 - Sin(k * Math.PI) * (36 + (i % 3) * 10), d = 6 + (i % 3) * 3;
                    FillEl(g, A(a, Foam), x - d / 2, y - d / 2, d, d);
                    using (Pen o = P(A(a / 2, Color.FromArgb(214, 170, 90)), 1f)) g.DrawEllipse(o, x - d / 2, y - d / 2, d, d);
                }
            }
        }
    }
}

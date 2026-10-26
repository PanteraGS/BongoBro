using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BongoBro
{
    // Pets sit on the desk to the right of the keyboard. Local coordinates: (0,0) is the
    // middle of the pet's feet, up is negative y, a pet is roughly 46 units tall.
    public static partial class Art
    {
        public static readonly PointF PetSpot = new PointF(324, 274);

        public static void DrawPet(Graphics g, Pose p)
        {
            if (p.Pet == null) return;
            GraphicsState st = g.Save();
            float hop = p.Mood == Mood.Sleep ? 0 : p.PetHop * 9;
            float breathe = 1 + 0.03f * Sin(p.T * 2.2);
            g.TranslateTransform(PetSpot.X, PetSpot.Y - hop);
            Glow(g, 0, 2 + hop, 32, 6, Color.Black, 70);
            g.ScaleTransform(1.35f, 1.35f * breathe);
            bool sleep = p.Mood == Mood.Sleep;
            DrawPetBody(g, p.Pet, p.T, sleep);
            g.Restore(st);
            if (sleep) Zzz(g, PetSpot.X + 20, PetSpot.Y - 66, p.T, 0.8f);
        }

        public static void DrawPetPreview(Graphics g, string id, RectangleF dst, float t)
        {
            GraphicsState st = Fit(g, new RectangleF(-34, -62, 68, 70), dst);
            DrawPetBody(g, id, t, false);
            g.Restore(st);
        }

        static void DrawPetBody(Graphics g, string id, float t, bool sleep)
        {
            switch (id)
            {
                case "pet_cat": Cat(g, Color.FromArgb(158, 164, 178), t, sleep); break;
                case "pet_hamster": Hamster(g, sleep); break;
                case "pet_duck": Duck(g, sleep); break;
                case "pet_frog": Frog(g, sleep); break;
                case "pet_penguin": Penguin(g, sleep); break;
                case "pet_fox": Fox(g, t, sleep); break;
                case "pet_panda": Panda(g, sleep); break;
                case "pet_owl": Owl(g, t, sleep); break;
                case "pet_axolotl": Axolotl(g, t, sleep); break;
                case "pet_corgi": Dog(g, Color.FromArgb(232, 150, 70), false, t, sleep); break;
                case "pet_melon": MiniMelon(g, sleep); break;
                case "pet_robot": Robot(g, t, sleep); break;
                case "pet_dragon": Dragon(g, t, sleep); break;
                case "pet_unicorn": Unicorn(g, t, sleep); break;
                case "set_samurai_shiba": Dog(g, Color.FromArgb(222, 150, 84), true, t, sleep); break;
                case "set_pirate_parrot": Parrot(g, t, sleep); break;
                case "set_astro_alien": Alien(g, t, sleep); break;
            }
        }

        // ---------- shared bits ----------
        static readonly Color PetInk = Color.FromArgb(52, 38, 34);

        static void Blob(Graphics g, RectangleF r, Color c)
        {
            using (GraphicsPath p = new GraphicsPath())
            {
                p.AddEllipse(r);
                Grad(g, p, Mix(c, Color.White, 0.25f), Mix(c, Color.Black, 0.08f));
                Stroke(g, PetInk, 2f, p);
            }
        }

        static void Tri(Graphics g, Color c, params PointF[] pts)
        {
            using (GraphicsPath p = new GraphicsPath())
            {
                p.AddPolygon(pts);
                Fill(g, c, p);
                Stroke(g, PetInk, 1.8f, p);
            }
        }

        static void PetEyes(Graphics g, float lx, float rx, float y, bool sleep, float size)
        {
            if (sleep)
            {
                Bez(g, PetInk, 1.8f, lx - size, y, lx - size * 0.4f, y + size * 0.8f, lx + size * 0.4f, y + size * 0.8f, lx + size, y);
                Bez(g, PetInk, 1.8f, rx - size, y, rx - size * 0.4f, y + size * 0.8f, rx + size * 0.4f, y + size * 0.8f, rx + size, y);
                return;
            }
            foreach (float x in new float[] { lx, rx })
            {
                FillEl(g, PetInk, x - size, y - size * 1.15f, size * 2, size * 2.3f);
                FillEl(g, Color.White, x - size * 0.55f, y - size * 0.85f, size * 0.8f, size * 0.8f);
            }
        }

        static void Cheeks(Graphics g, float lx, float rx, float y)
        {
            Glow(g, lx, y, 5, 3, Color.FromArgb(255, 130, 150), 140);
            Glow(g, rx, y, 5, 3, Color.FromArgb(255, 130, 150), 140);
        }

        static void SmallMouth(Graphics g, float x, float y)
        {
            Bez(g, PetInk, 1.5f, x - 3, y, x - 1.5f, y + 2, x, y + 2, x, y);
            Bez(g, PetInk, 1.5f, x, y, x, y + 2, x + 1.5f, y + 2, x + 3, y);
        }

        // ---------- pets ----------
        static void Cat(Graphics g, Color c, float t, bool sleep)
        {
            float sway = sleep ? 0 : Sin(t * 2.5) * 4;
            using (Pen tail = P(PetInk, 7)) g.DrawBezier(tail, 12, -6, 26, -8, 24 + sway, -22, 18 + sway, -30);
            using (Pen tail = P(c, 4.5f)) g.DrawBezier(tail, 12, -6, 26, -8, 24 + sway, -22, 18 + sway, -30);
            Blob(g, new RectangleF(-15, -24, 30, 24), c);
            Blob(g, new RectangleF(-16, -46, 32, 27), c);
            Tri(g, c, new PointF(-15, -38), new PointF(-12, -54), new PointF(-3, -44));
            Tri(g, c, new PointF(15, -38), new PointF(12, -54), new PointF(3, -44));
            Tri(g, Color.FromArgb(255, 170, 190), new PointF(-11, -42), new PointF(-10.5f, -50), new PointF(-6, -44));
            Tri(g, Color.FromArgb(255, 170, 190), new PointF(11, -42), new PointF(10.5f, -50), new PointF(6, -44));
            PetEyes(g, -6, 6, -33, sleep, 2.6f);
            FillEl(g, Color.FromArgb(255, 140, 160), -1.5f, -29, 3, 2.2f);
            SmallMouth(g, 0, -27);
            Cheeks(g, -10, 10, -27);
            using (Pen w = P(A(160, PetInk), 0.9f))
            {
                g.DrawLine(w, -9, -28, -19, -30); g.DrawLine(w, -9, -26, -19, -25);
                g.DrawLine(w, 9, -28, 19, -30); g.DrawLine(w, 9, -26, 19, -25);
            }
        }

        static void Hamster(Graphics g, bool sleep)
        {
            Color c = Color.FromArgb(232, 176, 112);
            FillEl(g, c, -15, -40, 9, 9); FillEl(g, c, 6, -40, 9, 9);
            Blob(g, new RectangleF(-19, -38, 38, 38), c);
            using (GraphicsPath b = new GraphicsPath()) { b.AddEllipse(-11, -22, 22, 20); Fill(g, Color.FromArgb(255, 244, 226), b); }
            FillEl(g, Color.FromArgb(255, 170, 170), -12.5f, -38.5f, 5, 5); FillEl(g, Color.FromArgb(255, 170, 170), 7.5f, -38.5f, 5, 5);
            PetEyes(g, -6, 6, -27, sleep, 2.4f);
            FillEl(g, Color.FromArgb(240, 120, 140), -1.5f, -23, 3, 2);
            SmallMouth(g, 0, -21);
            Glow(g, -12, -20, 6, 4, Color.FromArgb(255, 140, 150), 130);
            Glow(g, 12, -20, 6, 4, Color.FromArgb(255, 140, 150), 130);
            FillEl(g, Color.FromArgb(255, 210, 190), -8, -6, 6, 4); FillEl(g, Color.FromArgb(255, 210, 190), 2, -6, 6, 4);
        }

        static void Duck(Graphics g, bool sleep)
        {
            Color y = Color.FromArgb(255, 220, 70);
            FillEl(g, Color.FromArgb(255, 150, 40), -9, -3, 8, 4); FillEl(g, Color.FromArgb(255, 150, 40), 1, -3, 8, 4);
            Blob(g, new RectangleF(-17, -24, 32, 24), y);
            Blob(g, new RectangleF(-11, -44, 24, 22), y);
            Bez(g, Mix(y, Color.Black, 0.25f), 2f, -12, -14, -6, -18, 2, -16, 6, -10);
            using (GraphicsPath bk = new GraphicsPath())
            {
                bk.AddEllipse(8, -34, 12, 7);
                Fill(g, Color.FromArgb(255, 150, 40), bk);
                Stroke(g, PetInk, 1.6f, bk);
            }
            PetEyes(g, -2, 6, -35, sleep, 2.1f);
            Cheeks(g, -5, 9, -29);
            Tri(g, y, new PointF(0, -44), new PointF(2, -50), new PointF(4, -43));
        }

        static void Frog(Graphics g, bool sleep)
        {
            Color c = Color.FromArgb(110, 196, 90);
            Blob(g, new RectangleF(-20, -28, 40, 28), c);
            using (GraphicsPath b = new GraphicsPath()) { b.AddEllipse(-12, -16, 24, 15); Fill(g, Color.FromArgb(214, 240, 170), b); }
            Blob(g, new RectangleF(-17, -40, 14, 14), c);
            Blob(g, new RectangleF(3, -40, 14, 14), c);
            if (sleep) PetEyes(g, -10, 10, -33, true, 3.5f);
            else
                foreach (float x in new float[] { -10, 10 })
                {
                    FillEl(g, Color.White, x - 4.5f, -37.5f, 9, 9);
                    FillEl(g, PetInk, x - 2.4f, -35.5f, 4.8f, 5.6f);
                    FillEl(g, Color.White, x - 1.6f, -35, 1.6f, 1.6f);
                }
            Bez(g, PetInk, 1.8f, -9, -22, -4, -18, 4, -18, 9, -22);
            Cheeks(g, -13, 13, -20);
        }

        static void Penguin(Graphics g, bool sleep)
        {
            FillEl(g, Color.FromArgb(255, 150, 40), -10, -3, 9, 4); FillEl(g, Color.FromArgb(255, 150, 40), 1, -3, 9, 4);
            Blob(g, new RectangleF(-16, -46, 32, 45), Color.FromArgb(46, 50, 64));
            using (GraphicsPath b = new GraphicsPath())
            {
                b.AddEllipse(-11, -36, 22, 33);
                Fill(g, Color.FromArgb(250, 250, 252), b);
            }
            PetEyes(g, -5, 5, -31, sleep, 2.3f);
            Tri(g, Color.FromArgb(255, 160, 50), new PointF(-3, -27), new PointF(3, -27), new PointF(0, -22));
            Cheeks(g, -8, 8, -25);
            Bez(g, Color.FromArgb(30, 32, 42), 4f, -16, -30, -22, -22, -22, -14, -18, -10);
            Bez(g, Color.FromArgb(30, 32, 42), 4f, 16, -30, 22, -22, 22, -14, 18, -10);
        }

        static void Fox(Graphics g, float t, bool sleep)
        {
            Color o = Color.FromArgb(236, 128, 52);
            float sway = sleep ? 0 : Sin(t * 2) * 3;
            using (GraphicsPath tail = new GraphicsPath())
            {
                tail.AddBezier(10, -6, 26, -2, 30 + sway, -18, 22 + sway, -32);
                tail.AddBezier(22 + sway, -32, 18, -18, 16, -12, 8, -12);
                tail.CloseFigure();
                Fill(g, o, tail); Stroke(g, PetInk, 1.8f, tail);
            }
            FillEl(g, Color.White, 18 + sway, -35, 8, 8);
            Blob(g, new RectangleF(-14, -24, 28, 24), o);
            Tri(g, o, new PointF(-15, -36), new PointF(-14, -56), new PointF(-3, -42));
            Tri(g, o, new PointF(15, -36), new PointF(14, -56), new PointF(3, -42));
            Blob(g, new RectangleF(-16, -46, 32, 26), o);
            using (GraphicsPath m = new GraphicsPath())
            {
                m.AddBezier(-12, -32, -8, -22, 8, -22, 12, -32);
                m.AddBezier(12, -32, 6, -28, -6, -28, -12, -32);
                Fill(g, Color.FromArgb(255, 248, 238), m);
            }
            PetEyes(g, -6, 6, -35, sleep, 2.4f);
            FillEl(g, PetInk, -2, -28, 4, 3);
        }

        static void Panda(Graphics g, bool sleep)
        {
            Color k = Color.FromArgb(36, 38, 46);
            FillEl(g, k, -17, -48, 11, 11); FillEl(g, k, 6, -48, 11, 11);
            Blob(g, new RectangleF(-17, -26, 34, 26), Color.FromArgb(246, 246, 250));
            FillEl(g, k, -17, -20, 9, 14); FillEl(g, k, 8, -20, 9, 14);
            Blob(g, new RectangleF(-17, -46, 34, 28), Color.FromArgb(250, 250, 252));
            FillEl(g, k, -12, -37, 9, 11); FillEl(g, k, 3, -37, 9, 11);
            if (sleep) { Bez(g, Color.White, 1.6f, -10, -32, -8, -30, -6, -30, -4, -32); Bez(g, Color.White, 1.6f, 4, -32, 6, -30, 8, -30, 10, -32); }
            else { FillEl(g, Color.White, -9, -34, 3.5f, 3.5f); FillEl(g, Color.White, 6, -34, 3.5f, 3.5f); }
            FillEl(g, k, -2, -27, 4, 3);
            SmallMouth(g, 0, -24);
            Cheeks(g, -11, 11, -24);
        }

        static void Owl(Graphics g, float t, bool sleep)
        {
            Color c = Color.FromArgb(150, 104, 70);
            Tri(g, c, new PointF(-15, -38), new PointF(-14, -50), new PointF(-7, -42));
            Tri(g, c, new PointF(15, -38), new PointF(14, -50), new PointF(7, -42));
            Blob(g, new RectangleF(-17, -46, 34, 46), c);
            using (GraphicsPath b = new GraphicsPath()) { b.AddEllipse(-10, -22, 20, 19); Fill(g, Color.FromArgb(232, 206, 170), b); }
            using (Pen v = P(Mix(c, Color.Black, 0.2f), 1.2f))
                for (int i = 0; i < 3; i++) { float y = -18 + i * 5; g.DrawLines(v, new PointF[] { new PointF(-4, y), new PointF(0, y + 2), new PointF(4, y) }); }
            foreach (float x in new float[] { -7, 7 })
            {
                FillEl(g, Color.FromArgb(250, 244, 230), x - 7, -40, 14, 14);
                using (Pen o = P(Mix(c, Color.Black, 0.3f), 1.4f)) g.DrawEllipse(o, x - 7, -40, 14, 14);
            }
            if (sleep) PetEyes(g, -7, 7, -34, true, 3.5f);
            else
            {
                float look = Sin(t * 0.7) * 1.5f;
                foreach (float x in new float[] { -7, 7 })
                {
                    FillEl(g, PetInk, x - 3.6f + look, -36.6f, 7.2f, 7.2f);
                    FillEl(g, Color.White, x - 2 + look, -35.5f, 2.2f, 2.2f);
                }
            }
            Tri(g, Color.FromArgb(255, 180, 60), new PointF(-2.5f, -28), new PointF(2.5f, -28), new PointF(0, -23));
        }

        static void Axolotl(Graphics g, float t, bool sleep)
        {
            Color c = Color.FromArgb(255, 176, 200), f = Color.FromArgb(236, 98, 140);
            float w = sleep ? 0 : Sin(t * 3) * 2;
            for (int k = 0; k < 2; k++)
            {
                float s = k == 0 ? -1 : 1;
                for (int i = 0; i < 3; i++)
                    using (Pen fr = P(f, 3.6f)) g.DrawLine(fr, s * 13, -34 + i * 5, s * (22 + i % 2 * 2) + s * w, -42 + i * 6);
            }
            Blob(g, new RectangleF(-14, -22, 28, 22), c);
            Blob(g, new RectangleF(-16, -44, 32, 26), c);
            PetEyes(g, -7, 7, -32, sleep, 2.2f);
            Bez(g, PetInk, 1.6f, -6, -26, -3, -23, 3, -23, 6, -26);
            Cheeks(g, -10, 10, -27);
            FillEl(g, Mix(c, Color.White, 0.4f), -8, -18, 16, 12);
        }

        static void Dog(Graphics g, Color c, bool shiba, float t, bool sleep)
        {
            float wag = sleep ? 0 : Sin(t * 9) * 6;
            if (shiba)
            {
                using (Pen tl = P(PetInk, 8)) g.DrawArc(tl, 8, -26, 14, 14, 180 + wag, 260);
                using (Pen tl = P(c, 5)) g.DrawArc(tl, 8, -26, 14, 14, 180 + wag, 260);
            }
            else
            {
                using (Pen tl = P(PetInk, 6)) g.DrawLine(tl, 13, -14, 20, -22 + wag * 0.4f);
                using (Pen tl = P(c, 3.5f)) g.DrawLine(tl, 13, -14, 20, -22 + wag * 0.4f);
            }
            Blob(g, new RectangleF(-17, -24, 34, 22), c);
            FillEl(g, c, -14, -6, 7, 6); FillEl(g, c, 7, -6, 7, 6);
            using (GraphicsPath b = new GraphicsPath()) { b.AddEllipse(-8, -18, 16, 14); Fill(g, Color.FromArgb(255, 248, 238), b); }
            Tri(g, c, new PointF(-15, -36), new PointF(-14, shiba ? -52 : -56), new PointF(-4, -42));
            Tri(g, c, new PointF(15, -36), new PointF(14, shiba ? -52 : -56), new PointF(4, -42));
            Blob(g, new RectangleF(-16, -46, 32, 26), c);
            using (GraphicsPath m = new GraphicsPath())
            {
                m.AddEllipse(-9, -34, 18, 13);
                Fill(g, Color.FromArgb(255, 248, 238), m);
            }
            PetEyes(g, -6, 6, -37, sleep, 2.3f);
            FillEl(g, PetInk, -2.5f, -31, 5, 3.6f);
            if (!sleep && !shiba)
                using (GraphicsPath tg = RR(new RectangleF(-2.5f, -26, 5, 6), 2.5f)) Fill(g, Color.FromArgb(255, 120, 140), tg);
            else SmallMouth(g, 0, -26);
            Cheeks(g, -10, 10, -29);
            if (shiba)
                using (GraphicsPath sc = new GraphicsPath())
                {
                    // red bandana of the samurai set
                    sc.AddPolygon(new PointF[] { new PointF(-12, -21), new PointF(12, -21), new PointF(0, -13) });
                    Fill(g, Color.FromArgb(214, 40, 48), sc);
                    Stroke(g, PetInk, 1.2f, sc);
                }
        }

        static void MiniMelon(Graphics g, bool sleep)
        {
            using (GraphicsPath m = new GraphicsPath())
            {
                m.AddEllipse(-19, -38, 38, 37);
                Grad(g, m, Color.FromArgb(140, 206, 96), Color.FromArgb(56, 128, 52));
                GraphicsState st = g.Save();
                g.SetClip(m, CombineMode.Intersect);
                using (Pen s = P(Color.FromArgb(200, 32, 94, 36), 4f))
                    for (int i = -2; i <= 2; i++) g.DrawBezier(s, i * 8, -38, i * 10, -26, i * 10, -12, i * 8, -1);
                Glow(g, 0, -18, 13, 10, Color.FromArgb(190, 230, 140), 160);
                g.Restore(st);
                Stroke(g, Color.FromArgb(26, 62, 28), 2f, m);
            }
            using (GraphicsPath leaf = new GraphicsPath())
            {
                leaf.AddBezier(0, -38, 4, -48, 12, -48, 14, -44);
                leaf.AddBezier(14, -44, 8, -40, 4, -40, 0, -38);
                Fill(g, Color.FromArgb(110, 190, 70), leaf);
                Stroke(g, Color.FromArgb(26, 62, 28), 1.4f, leaf);
            }
            PetEyes(g, -6, 6, -21, sleep, 2.3f);
            SmallMouth(g, 0, -15);
            Cheeks(g, -10, 10, -15);
        }

        static void Robot(Graphics g, float t, bool sleep)
        {
            Color m = Color.FromArgb(176, 186, 204);
            using (GraphicsPath b = RR(new RectangleF(-13, -22, 26, 21), 5)) { Grad(g, b, Mix(m, Color.White, 0.3f), m); Stroke(g, PetInk, 1.8f, b); }
            FillEl(g, Color.FromArgb(255, 90, 90), -3, -16, 6, 6);
            Line(g, PetInk, 2f, 0, -46, 0, -52);
            bool on = !sleep && (int)(t * 2) % 2 == 0;
            Glow(g, 0, -54, 6, 6, Color.FromArgb(255, 80, 80), on ? 200 : 40);
            FillEl(g, on ? Color.FromArgb(255, 110, 110) : Color.FromArgb(150, 80, 80), -2.5f, -56.5f, 5, 5);
            using (GraphicsPath h = RR(new RectangleF(-17, -47, 34, 26), 7)) { Grad(g, h, Mix(m, Color.White, 0.35f), m); Stroke(g, PetInk, 1.8f, h); }
            using (GraphicsPath sc = RR(new RectangleF(-12, -42, 24, 16), 4)) Fill(g, Color.FromArgb(22, 30, 46), sc);
            Color led = Color.FromArgb(90, 230, 255);
            if (sleep) { Line(g, led, 2f, -8, -34, -3, -34); Line(g, led, 2f, 3, -34, 8, -34); }
            else
            {
                Glow(g, -5, -35, 5, 5, led, 160); Glow(g, 5, -35, 5, 5, led, 160);
                FillEl(g, led, -7, -37, 4, 4); FillEl(g, led, 3, -37, 4, 4);
                Bez(g, led, 1.4f, -4, -30, -2, -28, 2, -28, 4, -30);
            }
            FillEl(g, m, -19, -38, 4, 8); FillEl(g, m, 15, -38, 4, 8);
        }

        static void Dragon(Graphics g, float t, bool sleep)
        {
            Color c = Color.FromArgb(120, 92, 226), belly = Color.FromArgb(255, 214, 140);
            float pulse = 0.7f + 0.3f * Sin(t * 3);
            Glow(g, 0, -24, 32, 30, Color.FromArgb(170, 120, 255), (int)(110 * pulse));
            float flap = sleep ? 0 : Sin(t * 6) * 6;
            for (int k = 0; k < 2; k++)
            {
                float s = k == 0 ? -1 : 1;
                using (GraphicsPath w = new GraphicsPath())
                {
                    w.AddPolygon(new PointF[] { new PointF(s * 8, -26), new PointF(s * 26, -44 - flap), new PointF(s * 24, -30), new PointF(s * 30, -26 - flap * 0.5f), new PointF(s * 12, -16) });
                    Grad(g, w, Color.FromArgb(160, 130, 255), Color.FromArgb(84, 60, 180));
                    Stroke(g, PetInk, 1.6f, w);
                }
            }
            using (Pen tl = P(PetInk, 7)) g.DrawBezier(tl, 10, -6, 24, -4, 26, -14, 30, -16);
            using (Pen tl = P(c, 4.5f)) g.DrawBezier(tl, 10, -6, 24, -4, 26, -14, 30, -16);
            Blob(g, new RectangleF(-14, -26, 28, 25), c);
            using (GraphicsPath b = new GraphicsPath()) { b.AddEllipse(-8, -20, 16, 17); Fill(g, belly, b); }
            Tri(g, Color.FromArgb(255, 236, 170), new PointF(-11, -42), new PointF(-14, -56), new PointF(-5, -46));
            Tri(g, Color.FromArgb(255, 236, 170), new PointF(11, -42), new PointF(14, -56), new PointF(5, -46));
            Blob(g, new RectangleF(-16, -48, 32, 26), c);
            PetEyes(g, -6, 6, -36, sleep, 2.5f);
            FillEl(g, PetInk, -4, -30, 2, 1.6f); FillEl(g, PetInk, 2, -30, 2, 1.6f);
            Cheeks(g, -10, 10, -29);
            // a small fire puff every few seconds
            float ph = (t * 0.4f) % 1f;
            if (!sleep && ph > 0.75f)
            {
                float k2 = (ph - 0.75f) / 0.25f;
                GraphicsState st = g.Save();
                g.TranslateTransform(-14, -28);
                g.RotateTransform(-90);
                Flame(g, 0, 0, 22 * Sin(k2 * Math.PI), 5, t, 41, false);
                Flame(g, 0, 0, 12 * Sin(k2 * Math.PI), 2.5f, t, 42, true);
                g.Restore(st);
            }
        }

        static void Unicorn(Graphics g, float t, bool sleep)
        {
            float hue = t * 70;
            Glow(g, 0, -26, 32, 30, Hsv(hue, 0.6f, 1f), 110);
            Color c = Color.FromArgb(252, 250, 255);
            Blob(g, new RectangleF(-16, -24, 32, 23), c);
            FillEl(g, c, -14, -6, 6, 6); FillEl(g, c, 8, -6, 6, 6);
            for (int i = 0; i < 4; i++)
                using (Pen m = P(Hsv(hue + i * 60, 0.75f, 1f), 4.5f)) g.DrawBezier(m, 8 - i * 4, -50 + i * 2, 18 - i * 2, -46 + i * 4, 18 - i * 2, -34 + i * 3, 12 - i, -24 + i);
            Blob(g, new RectangleF(-16, -46, 30, 26), c);
            using (GraphicsPath h = new GraphicsPath())
            {
                h.AddPolygon(new PointF[] { new PointF(-6, -44), new PointF(-1, -64), new PointF(2, -44) });
                Grad(g, h, Color.FromArgb(255, 240, 150), Color.FromArgb(230, 170, 40));
                Stroke(g, Color.FromArgb(150, 100, 20), 1.2f, h);
            }
            Glow(g, -2, -58, 7, 7, Color.FromArgb(255, 230, 140), (int)(150 + 100 * Sin(t * 4)));
            Tri(g, c, new PointF(4, -42), new PointF(8, -52), new PointF(10, -42));
            PetEyes(g, -8, 3, -34, sleep, 2.3f);
            FillEl(g, Color.FromArgb(255, 180, 200), -14, -30, 7, 5);
            Cheeks(g, -8, 6, -28);
            float sp = (t * 0.6f) % 1f;
            Sparkle(g, -20 + sp * 40, -50 + Sin(sp * Math.PI * 2) * 6, 4 * Sin(sp * Math.PI), Hsv(hue + 120, 0.5f, 1f));
        }

        static void Parrot(Graphics g, float t, bool sleep)
        {
            Color green = Color.FromArgb(70, 190, 90), red = Color.FromArgb(226, 56, 60);
            using (GraphicsPath tl = new GraphicsPath())
            {
                tl.AddPolygon(new PointF[] { new PointF(-4, -6), new PointF(4, -6), new PointF(8, 8), new PointF(-2, 6) });
                Fill(g, Color.FromArgb(60, 120, 220), tl);
                Stroke(g, PetInk, 1.4f, tl);
            }
            Blob(g, new RectangleF(-12, -30, 24, 28), green);
            Bez(g, Mix(green, Color.Black, 0.3f), 2f, 8, -24, 12, -16, 10, -8, 6, -4);
            Blob(g, new RectangleF(-12, -48, 24, 22), red);
            using (GraphicsPath bk = new GraphicsPath())
            {
                bk.AddBezier(-12, -40, -20, -40, -20, -32, -14, -30);
                bk.AddLine(-14, -30, -10, -36);
                bk.CloseFigure();
                Fill(g, Color.FromArgb(255, 210, 80), bk);
                Stroke(g, PetInk, 1.4f, bk);
            }
            if (sleep) PetEyes(g, -2, -2, -40, true, 2.2f);
            else { FillEl(g, Color.White, -6, -44, 7, 7); FillEl(g, PetInk, -4, -42, 3.4f, 3.4f); }
            // tiny pirate hat
            using (GraphicsPath hat = new GraphicsPath())
            {
                hat.AddBezier(-12, -48, -6, -58, 6, -58, 12, -48);
                hat.AddBezier(12, -48, 6, -50, -6, -50, -12, -48);
                Fill(g, Color.FromArgb(30, 30, 36), hat);
            }
            FillEl(g, Color.White, -1.5f, -54, 3, 3);
            Line(g, Color.FromArgb(255, 210, 80), 1.6f, -6, 0, -6, -3);
            Line(g, Color.FromArgb(255, 210, 80), 1.6f, 4, 0, 4, -3);
        }

        static void Alien(Graphics g, float t, bool sleep)
        {
            Color c = Color.FromArgb(150, 230, 110);
            float bob = sleep ? 0 : Sin(t * 2) * 2;
            g.TranslateTransform(0, bob);
            Blob(g, new RectangleF(-11, -20, 22, 20), c);
            FillEl(g, Color.FromArgb(210, 216, 230), -14, -4, 28, 6);
            for (int k = 0; k < 2; k++)
            {
                float s = k == 0 ? -1 : 1;
                Bez(g, PetInk, 1.6f, s * 5, -44, s * 8, -52, s * 12, -54, s * 14, -56);
                Glow(g, s * 14, -57, 4, 4, Color.FromArgb(160, 255, 140), 200);
                FillEl(g, Color.FromArgb(200, 255, 170), s * 14 - 2.2f, -59.2f, 4.4f, 4.4f);
            }
            Blob(g, new RectangleF(-15, -46, 30, 28), c);
            if (sleep) PetEyes(g, -6, 6, -33, true, 3.5f);
            else
                foreach (float x in new float[] { -6, 6 })
                {
                    using (GraphicsPath e = new GraphicsPath())
                    {
                        e.AddEllipse(x - 5, -38, 10, 12);
                        Fill(g, Color.FromArgb(20, 24, 36), e);
                    }
                    FillEl(g, Color.White, x - 3, -36, 3, 3);
                }
            SmallMouth(g, 0, -24);
            // glass bubble helmet
            using (GraphicsPath h = new GraphicsPath())
            {
                h.AddEllipse(-19, -52, 38, 38);
                using (SolidBrush b = new SolidBrush(Color.FromArgb(50, 190, 230, 255))) g.FillPath(b, h);
                using (Pen p = P(Color.FromArgb(180, 220, 240, 255), 1.6f)) g.DrawPath(p, h);
            }
            using (Pen hl = P(Color.FromArgb(200, 255, 255, 255), 2.2f)) g.DrawArc(hl, -14, -48, 28, 28, 200, 50);
            g.TranslateTransform(0, -bob);
        }

        // floating "Z z z" for sleeping characters and pets
        static void Zzz(Graphics g, float x, float y, float t, float scale)
        {
            for (int i = 0; i < 3; i++)
            {
                float ph = (t * 0.45f + i / 3f) % 1f;
                float sz = (10 + i * 3) * scale * (0.6f + ph * 0.6f);
                int a = (int)(255 * Math.Min(1, (1 - ph) * 1.6f));
                using (Font f = new Font("Segoe UI", Math.Max(1f, sz), FontStyle.Bold, GraphicsUnit.Pixel))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(a, 140, 170, 255)))
                    g.DrawString("z", f, b, x + ph * 18 * scale + i * 4 * scale, y - ph * 30 * scale);
            }
        }
    }
}

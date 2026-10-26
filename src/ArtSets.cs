using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BongoBro
{
    // Gear set pieces, set bonus effects, day-phase props and mood overlays.
    public static partial class Art
    {
        // ======================= set pieces =======================
        static void SamuraiBand(Graphics g)
        {
            Color ink = Color.FromArgb(120, 110, 100);
            using (GraphicsPath tail = new GraphicsPath())
            {
                tail.AddBezier(244, 104, 258, 112, 264, 128, 272, 142);
                tail.AddLine(272, 142, 262, 144);
                tail.AddBezier(262, 144, 256, 130, 250, 118, 240, 112);
                tail.CloseFigure();
                Fill(g, Color.FromArgb(248, 246, 240), tail);
                Stroke(g, ink, 1.6f, tail);
            }
            using (GraphicsPath b = new GraphicsPath())
            {
                b.AddBezier(110, 100, 140, 82, 220, 82, 250, 100);
                b.AddLine(250, 100, 250, 117);
                b.AddBezier(250, 117, 220, 99, 140, 99, 110, 117);
                b.CloseFigure();
                Grad(g, b, Color.White, Color.FromArgb(226, 222, 214));
                Stroke(g, ink, 2f, b);
            }
            Glow(g, 180, 99, 12, 9, Color.FromArgb(255, 60, 50), 120);
            FillEl(g, Color.FromArgb(214, 30, 40), 172, 91.5f, 16, 15);
        }

        static void SamuraiPaint(Graphics g)
        {
            Color red = Color.FromArgb(220, 210, 30, 40);
            foreach (int k in new int[] { 0, 1 })
            {
                float s = k == 0 ? -1 : 1, cx = k == 0 ? EyeL : EyeR;
                using (GraphicsPath p = new GraphicsPath())
                {
                    p.AddBezier(cx + 10 * s, EyeY + 2, cx + 20 * s, EyeY + 10, cx + 22 * s, EyeY + 26, cx + 16 * s, EyeY + 40);
                    p.AddBezier(cx + 16 * s, EyeY + 40, cx + 14 * s, EyeY + 26, cx + 12 * s, EyeY + 14, cx + 4 * s, EyeY + 6);
                    p.CloseFigure();
                    Fill(g, red, p);
                }
                Line(g, Color.FromArgb(230, 30, 20, 20), 2.4f, cx + 13 * s, EyeY - 2, cx + 22 * s, EyeY - 7);
            }
        }

        static void SamuraiGloves(Graphics g, float tx)
        {
            Color d = Color.FromArgb(70, 12, 16);
            using (LinearGradientBrush b = VBrush(Color.FromArgb(196, 52, 56), Color.FromArgb(128, 24, 30))) HandShape(g, tx, b, d, 2.4f);
            using (Pen gold = P(Color.FromArgb(230, 186, 80), 1.6f))
            {
                g.DrawBezier(gold, -15, -6, -6, -9, 6, -9, 15, -6);
                g.DrawBezier(gold, -15, 2, -6, -1, 6, -1, 15, 2);
            }
            FillEl(g, A(140, Color.White), -9, -12, 9, 5);
            FingerLines(g, d, 1.5f);
            Cuff(g, Color.FromArgb(30, 26, 30), Color.FromArgb(230, 186, 80), 11);
        }

        static void PirateHat(Graphics g)
        {
            Color black = Color.FromArgb(28, 26, 32), gold = Color.FromArgb(226, 180, 70);
            using (GraphicsPath crown = new GraphicsPath())
            {
                crown.AddBezier(128, 96, 124, 56, 150, 38, 180, 38);
                crown.AddBezier(180, 38, 210, 38, 236, 56, 232, 96);
                crown.CloseFigure();
                Grad(g, crown, Color.FromArgb(60, 56, 66), black);
                Stroke(g, Color.FromArgb(8, 8, 10), 2.2f, crown);
            }
            using (GraphicsPath brim = new GraphicsPath())
            {
                brim.AddBezier(98, 84, 124, 102, 150, 108, 180, 122);
                brim.AddBezier(180, 122, 210, 108, 236, 102, 262, 84);
                brim.AddBezier(262, 84, 236, 78, 210, 92, 180, 96);
                brim.AddBezier(180, 96, 150, 92, 124, 78, 98, 84);
                brim.CloseFigure();
                Grad(g, brim, Color.FromArgb(56, 52, 62), black);
                using (Pen tr = P(gold, 3f)) g.DrawPath(tr, brim);
            }
            // skull and crossbones
            using (Pen bone = P(Color.FromArgb(246, 244, 236), 3f)) { g.DrawLine(bone, 168, 82, 192, 62); g.DrawLine(bone, 168, 62, 192, 82); }
            FillEl(g, Color.FromArgb(246, 244, 236), 171, 56, 18, 16);
            FillEl(g, black, 175, 61, 4, 4); FillEl(g, black, 181, 61, 4, 4);
        }

        static void PirateEyepatch(Graphics g)
        {
            Color k = Color.FromArgb(24, 22, 26);
            Line(g, k, 3f, 118, 112, 246, 150);
            Line(g, k, 3f, 222, 144, 248, 146);
            using (GraphicsPath p = new GraphicsPath())
            {
                p.AddEllipse(191, 132, 30, 27);
                Grad(g, p, Color.FromArgb(56, 54, 60), k);
                Stroke(g, Color.FromArgb(8, 8, 10), 1.6f, p);
            }
            Bez(g, A(120, Color.White), 1.6f, 196, 140, 199, 136, 204, 135, 208, 135);
        }

        static void PirateGloves(Graphics g, float tx)
        {
            Color c = Color.FromArgb(124, 80, 44), d = Color.FromArgb(56, 32, 16);
            using (LinearGradientBrush b = VBrush(Color.FromArgb(166, 114, 70), c)) HandShape(g, tx, b, d, 2.4f);
            FingerLines(g, d, 1.5f);
            using (GraphicsPath cf = new GraphicsPath())
            {
                cf.AddPolygon(new PointF[] { new PointF(-16, 6), new PointF(16, 6), new PointF(22, 22), new PointF(-22, 22) });
                Grad(g, cf, Color.FromArgb(104, 64, 34), Color.FromArgb(70, 40, 20));
                Stroke(g, d, 1.8f, cf);
            }
            FillEl(g, Color.FromArgb(236, 196, 90), -8, 11, 5, 5);
            FillEl(g, Color.FromArgb(236, 196, 90), 3, 11, 5, 5);
        }

        static void AstroHelmet(Graphics g, float t)
        {
            using (GraphicsPath ring = RR(new RectangleF(118, 204, 124, 24), 10))
            {
                Grad(g, ring, Color.FromArgb(240, 242, 248), Color.FromArgb(170, 176, 190));
                Stroke(g, Color.FromArgb(70, 74, 86), 2f, ring);
            }
            for (int i = 0; i < 6; i++) FillEl(g, Color.FromArgb(120, 126, 140), 128 + i * 20, 213, 4, 4);
            using (GraphicsPath flag = RR(new RectangleF(208, 209, 16, 10), 1))
            {
                Fill(g, Color.White, flag);
                using (SolidBrush b = new SolidBrush(Color.FromArgb(40, 80, 200))) g.FillRectangle(b, 208, 212.4f, 16, 3.4f);
                using (SolidBrush r = new SolidBrush(Color.FromArgb(214, 40, 48))) g.FillRectangle(r, 208, 215.8f, 16, 3.2f);
            }
            using (GraphicsPath bub = new GraphicsPath())
            {
                bub.AddEllipse(88, 30, 184, 190);
                using (SolidBrush b = new SolidBrush(Color.FromArgb(44, 170, 220, 255))) g.FillPath(b, bub);
                using (Pen p = P(Color.FromArgb(220, 230, 244, 255), 3f)) g.DrawPath(p, bub);
            }
            using (Pen hl = P(Color.FromArgb(170, 255, 255, 255), 6f)) g.DrawArc(hl, 102, 44, 156, 162, 200, 50);
            using (Pen hl = P(Color.FromArgb(120, 255, 255, 255), 3f)) g.DrawArc(hl, 112, 54, 136, 142, 262, 14);
            Line(g, Color.FromArgb(120, 126, 140), 2.4f, 244, 50, 254, 22);
            bool on = (int)(t * 1.5) % 2 == 0;
            Glow(g, 255, 20, 8, 8, Color.FromArgb(255, 70, 70), on ? 200 : 50);
            FillEl(g, on ? Color.FromArgb(255, 110, 110) : Color.FromArgb(150, 70, 70), 251.5f, 16.5f, 7, 7);
        }

        static void AstroGloves(Graphics g, float tx)
        {
            Color d = Color.FromArgb(90, 96, 110);
            using (GraphicsPath th = new GraphicsPath()) { th.AddEllipse(tx * 1.05f - 7, -5, 14, 17); Grad(g, th, Color.White, Color.FromArgb(200, 204, 214)); Stroke(g, d, 2.2f, th); }
            using (GraphicsPath pp = new GraphicsPath()) { pp.AddEllipse(-19, -18, 38, 33); Grad(g, pp, Color.White, Color.FromArgb(196, 200, 212)); Stroke(g, d, 2.2f, pp); }
            FingerLines(g, A(160, d), 1.5f);
            using (GraphicsPath patch = RR(new RectangleF(-6, -4, 12, 9), 2)) Fill(g, Color.FromArgb(255, 138, 42), patch);
            using (GraphicsPath cf = RR(new RectangleF(-18, 6, 36, 15), 5))
            {
                Grad(g, cf, Color.FromArgb(70, 120, 220), Color.FromArgb(40, 70, 160));
                Stroke(g, Color.FromArgb(20, 36, 90), 1.8f, cf);
            }
            Line(g, Color.FromArgb(200, 210, 230), 1.6f, -16, 10, 16, 10);
        }

        // ======================= set bonus effects (behind the character) =======================
        static void SetBonusBack(Graphics g, Pose p)
        {
            string set = p.ActiveSet;
            float t = p.T;
            if (set == "set_samurai")
            {
                Glow(g, 180, 124, 150, 150, Color.FromArgb(255, 120, 90), 90);
                using (GraphicsPath sun = new GraphicsPath())
                {
                    sun.AddEllipse(84, 28, 192, 192);
                    using (PathGradientBrush b = new PathGradientBrush(sun))
                    {
                        b.CenterColor = Color.FromArgb(235, 255, 96, 72);
                        b.SurroundColors = new Color[] { Color.FromArgb(225, 214, 34, 44) };
                        g.FillPath(b, sun);
                    }
                }
            }
            else if (set == "set_pirate")
            {
                float wave = Sin(t * 3) * 6;
                Line(g, Color.FromArgb(96, 64, 36), 5f, 296, 22, 296, 262);
                FillEl(g, Color.FromArgb(230, 190, 80), 291.5f, 15, 9, 9);
                using (GraphicsPath f = new GraphicsPath())
                {
                    f.AddLine(296, 30, 296, 82);
                    f.AddBezier(296, 82, 316, 82 + wave, 336, 78 - wave, 356, 84 + wave);
                    f.AddLine(356, 84 + wave, 356, 32 + wave);
                    f.AddBezier(356, 32 + wave, 336, 26 - wave, 316, 30 + wave, 296, 30);
                    f.CloseFigure();
                    Fill(g, Color.FromArgb(26, 24, 30), f);
                }
                using (Pen bone = P(Color.FromArgb(240, 238, 230), 2.4f)) { g.DrawLine(bone, 316, 66 + wave * 0.3f, 336, 50); g.DrawLine(bone, 316, 50, 336, 66 + wave * 0.3f); }
                FillEl(g, Color.FromArgb(240, 238, 230), 319, 41 + wave * 0.2f, 14, 13);
            }
            else if (set == "set_astro")
            {
                StarField(g, new RectangleF(0, 0, 360, 230), t, 21);
                float a = t * 0.6f;
                float px = 180 + Cos(a) * 150, py = 112 + Sin(a) * 34;
                if (Sin(a) < 0) Planet(g, px, py, t);
            }
        }

        // effects drawn in front of the character
        static void SetBonusFront(Graphics g, Pose p)
        {
            string set = p.ActiveSet;
            float t = p.T;
            if (set == "set_samurai")
            {
                for (int i = 0; i < 12; i++)
                {
                    float ph = (t * 0.12f + i / 12f) % 1f;
                    float x = (i * 67) % 360 + Sin(t * 1.3 + i) * 14;
                    Petal(g, x, -10 + ph * 370, t * 90 + i * 40);
                }
            }
            else if (set == "set_pirate" && p.PetHop > 0)
            {
                // a few gold coins pop out of the keyboard on every key press
                float k = 1 - p.PetHop;
                for (int i = 0; i < 3; i++)
                {
                    float x = 150 + i * 30 + k * (i - 1) * 20, y = 270 - Sin(k * Math.PI) * 50 - i * 6;
                    int a = (int)(255 * p.PetHop);
                    FillEl(g, A(a, Color.FromArgb(255, 210, 70)), x - 6, y - 6, 12, 12);
                    using (Pen o = P(A(a, Color.FromArgb(170, 120, 20)), 1.4f)) g.DrawEllipse(o, x - 6, y - 6, 12, 12);
                    FillEl(g, A(a, Color.White), x - 3, y - 4, 3, 3);
                }
            }
            else if (set == "set_astro")
            {
                float a = t * 0.6f;
                if (Sin(a) >= 0) Planet(g, 180 + Cos(a) * 150, 112 + Sin(a) * 34, t);
            }
        }

        static void Planet(Graphics g, float x, float y, float t)
        {
            Glow(g, x, y, 26, 26, Color.FromArgb(120, 180, 255), 80);
            using (GraphicsPath pl = new GraphicsPath())
            {
                pl.AddEllipse(x - 15, y - 15, 30, 30);
                using (PathGradientBrush b = new PathGradientBrush(pl))
                {
                    b.CenterPoint = new PointF(x - 5, y - 6);
                    b.CenterColor = Color.FromArgb(255, 200, 150);
                    b.SurroundColors = new Color[] { Color.FromArgb(200, 90, 60) };
                    g.FillPath(b, pl);
                }
            }
            using (Pen r = P(Color.FromArgb(220, 255, 230, 170), 3f)) g.DrawEllipse(r, x - 26, y - 6, 52, 12);
        }

        // ======================= day phases =======================
        static void DayBack(Graphics g, Pose p)
        {
            float t = p.T;
            switch (p.Day)
            {
                case DayPhase.Evening:
                    Glow(g, 180, 220, 190, 150, Color.FromArgb(255, 140, 60), 90);
                    break;
                case DayPhase.Night:
                    Glow(g, 60, 62, 40, 40, Color.FromArgb(220, 230, 255), 70);
                    using (GraphicsPath moon = new GraphicsPath())
                    using (GraphicsPath cut = new GraphicsPath())
                    {
                        moon.AddEllipse(42, 44, 36, 36);
                        cut.AddEllipse(54, 38, 34, 34);
                        using (Region r = new Region(moon))
                        {
                            r.Exclude(cut);
                            using (SolidBrush b = new SolidBrush(Color.FromArgb(255, 250, 220))) g.FillRegion(b, r);
                        }
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        float x = 20 + (i * 53) % 110, y = 20 + (i * 37) % 70;
                        Sparkle(g, x, y, 2 + 1.5f * (0.5f + 0.5f * Sin(t * 2 + i)), Color.FromArgb(200, 210, 255));
                    }
                    break;
                case DayPhase.Morning:
                    for (int i = 0; i < 7; i++)
                    {
                        double a = (200 + i * 12) * Math.PI / 180;
                        using (SolidBrush b = new SolidBrush(Color.FromArgb(28, 255, 220, 140)))
                            g.FillPolygon(b, new PointF[] { new PointF(360, 0), new PointF(360 + Cos(a - 0.03) * 420, Sin(a - 0.03) * 420), new PointF(360 + Cos(a + 0.03) * 420, Sin(a + 0.03) * 420) });
                    }
                    break;
            }
        }

        // a mug on the desk, coffee in the morning and tea at night
        static void DayFront(Graphics g, Pose p)
        {
            if (p.Day != DayPhase.Morning && p.Day != DayPhase.Night) return;
            float x = 36, y = 276, t = p.T;
            Color mug = p.Day == DayPhase.Morning ? Color.FromArgb(236, 90, 70) : Color.FromArgb(120, 170, 230);
            using (Pen h = P(PetInk, 6)) g.DrawArc(h, x + 8, y - 20, 12, 12, -90, 180);
            using (Pen h = P(mug, 3.5f)) g.DrawArc(h, x + 8, y - 20, 12, 12, -90, 180);
            using (GraphicsPath m = RR(new RectangleF(x - 12, y - 24, 24, 24), 5))
            {
                Grad(g, m, Mix(mug, Color.White, 0.25f), mug);
                Stroke(g, PetInk, 2f, m);
            }
            FillEl(g, p.Day == DayPhase.Morning ? Color.FromArgb(110, 66, 40) : Color.FromArgb(200, 150, 70), x - 10, y - 26, 20, 5);
            using (Pen s = P(Color.FromArgb(150, 255, 255, 255), 2f))
                for (int i = 0; i < 2; i++)
                {
                    float o = Sin(t * 2 + i * 2) * 3;
                    g.DrawBezier(s, x - 4 + i * 8, y - 28, x - 8 + i * 8 + o, y - 34, x + i * 8 - o, y - 40, x - 4 + i * 8, y - 46);
                }
        }

        // ======================= mood overlays (head coordinates) =======================
        static void MoodOverlay(Graphics g, Pose p)
        {
            float t = p.T;
            switch (p.Mood)
            {
                case Mood.Sleep:
                    Zzz(g, 236, 96, t, 1.3f);
                    break;
                case Mood.Surprised:
                    using (Font f = new Font("Segoe UI", 40, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (GraphicsPath tp = new GraphicsPath())
                    {
                        tp.AddString("!", f.FontFamily, (int)FontStyle.Bold, 44, new PointF(244, 28), StringFormat.GenericDefault);
                        using (Pen o = P(Color.FromArgb(120, 70, 0), 4f)) g.DrawPath(o, tp);
                        Fill(g, Color.FromArgb(255, 210, 50), tp);
                    }
                    break;
                case Mood.Focused:
                    using (GraphicsPath d = new GraphicsPath())
                    {
                        d.AddBezier(238, 104, 232, 114, 232, 122, 238, 124);
                        d.AddBezier(238, 124, 244, 122, 244, 114, 238, 104);
                        Grad(g, d, Color.FromArgb(220, 240, 255), Color.FromArgb(120, 190, 250));
                        Stroke(g, Color.FromArgb(60, 120, 190), 1.4f, d);
                    }
                    break;
                case Mood.Happy:
                    Sparkle(g, 116, 96, 6 + 2 * Sin(t * 6), Color.FromArgb(255, 230, 120));
                    Sparkle(g, 246, 120, 5 + 2 * Sin(t * 6 + 2), Color.FromArgb(255, 230, 120));
                    break;
            }
        }

        // motion lines next to the hands when typing fast
        static void SpeedLines(Graphics g, Pose p)
        {
            if (p.Mood != Mood.Focused) return;
            using (Pen s = P(Color.FromArgb(150, 255, 255, 255), 2.2f))
            {
                float o = (p.T * 40) % 8;
                g.DrawLine(s, 70, 246 + o, 84, 252 + o);
                g.DrawLine(s, 66, 262 + o, 80, 266 + o);
                g.DrawLine(s, 290, 246 + o, 276, 252 + o);
                g.DrawLine(s, 294, 262 + o, 280, 266 + o);
            }
        }
    }
}

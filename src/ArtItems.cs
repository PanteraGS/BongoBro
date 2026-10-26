using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BongoBro
{
    // Hats, glasses and gloves.
    public static partial class Art
    {
        // ======================= GLOVES =======================
        static bool DrawGlove(Graphics g, string id, float tx, float t, float seed)
        {
            switch (id)
            {
                case "gv_knit": GloveKnit(g, tx); return true;
                case "gv_garden": GloveGarden(g, tx); return true;
                case "gv_rubber": GloveSimple(g, tx, Color.FromArgb(255, 236, 120), Color.FromArgb(250, 200, 30), Color.FromArgb(170, 120, 0), true); return true;
                case "gv_medical": GloveSimple(g, tx, Color.FromArgb(170, 214, 255), Color.FromArgb(100, 160, 236), Color.FromArgb(40, 90, 170), false); return true;
                case "gv_striped": GloveStriped(g, tx); return true;
                case "gv_leather": GloveLeather(g, tx); return true;
                case "gv_fingerless": GloveFingerless(g, tx); return true;
                case "gv_boxing": GloveBoxing(g, tx); return true;
                case "gv_paw": GlovePaw(g, tx); return true;
                case "gv_mickey": GloveMickey(g, tx); return true;
                case "gv_gold": GloveGold(g, tx); return true;
                case "gv_knight": GloveKnight(g, tx); return true;
                case "gv_infinity": GloveInfinity(g, tx, t); return true;
                case "gv_plasma": GlovePlasma(g, tx, t, seed); return true;
            }
            return false;
        }

        static void GloveKnit(Graphics g, float tx)
        {
            Color c = Color.FromArgb(70, 170, 90), d = Color.FromArgb(34, 110, 52);
            using (LinearGradientBrush b = VBrush(Mix(c, Color.White, 0.2f), c)) HandShape(g, tx, b, d, 2.4f);
            using (GraphicsPath pp = PalmPath())
            {
                GraphicsState st = g.Save();
                g.SetClip(pp, CombineMode.Intersect);
                using (Pen k = P(A(150, d), 1.3f))
                    for (float yy = -12; yy < 14; yy += 6)
                        for (float xx = -14; xx < 16; xx += 6)
                        {
                            g.DrawLine(k, xx - 1.5f, yy - 1.5f, xx, yy);
                            g.DrawLine(k, xx, yy, xx + 1.5f, yy - 1.5f);
                        }
                g.Restore(st);
            }
            FingerLines(g, d, 1.8f);
            Cuff(g, Color.FromArgb(240, 240, 240), d, 11);
            Line(g, c, 2.4f, -14, 12.5f, 14, 12.5f);
        }

        static void GloveGarden(Graphics g, float tx)
        {
            Color c = Color.FromArgb(236, 220, 172), d = Color.FromArgb(150, 120, 70);
            using (LinearGradientBrush b = VBrush(Mix(c, Color.White, 0.3f), c)) HandShape(g, tx, b, d, 2.4f);
            using (SolidBrush dot = new SolidBrush(Color.FromArgb(238, 128, 40)))
            {
                float[][] pts = { new float[] { -9, -4 }, new float[] { 0, 2 }, new float[] { 8, -5 }, new float[] { -5, 7 }, new float[] { 7, 6 }, new float[] { -1, -9 } };
                foreach (float[] q in pts) g.FillEllipse(dot, q[0] - 2, q[1] - 2, 4, 4);
            }
            FingerLines(g, d, 1.6f);
            Cuff(g, Color.FromArgb(196, 102, 43), Color.FromArgb(120, 56, 20), 11);
        }

        static void GloveSimple(Graphics g, float tx, Color light, Color c, Color d, bool longCuff)
        {
            using (LinearGradientBrush b = VBrush(light, c)) HandShape(g, tx, b, d, 2.3f);
            FillEl(g, A(170, Color.White), -10, -12, 11, 6);
            FillEl(g, A(110, Color.White), 6, -2, 4, 7);
            FingerLines(g, A(180, d), 1.5f);
            if (longCuff)
            {
                using (GraphicsPath cp = RR(new RectangleF(-17, 5, 34, 17), 5))
                {
                    using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(-17, 4, 34, 19), light, c, 90f)) g.FillPath(b, cp);
                    Stroke(g, d, 2f, cp);
                }
                Line(g, d, 1.5f, -15, 9, 15, 9);
            }
            else
            {
                Cuff(g, light, d, 9);
                Line(g, A(150, d), 1.2f, -14, 11.5f, 14, 11.5f);
            }
        }

        static void GloveStriped(Graphics g, float tx)
        {
            Color ink = Color.FromArgb(90, 30, 60);
            using (SolidBrush w = new SolidBrush(Color.FromArgb(252, 250, 246))) HandShape(g, tx, w, ink, 2.4f);
            using (GraphicsPath pp = PalmPath())
            using (GraphicsPath th = ThumbPath(tx))
            using (Region rg = new Region(pp))
            {
                rg.Union(th);
                GraphicsState st = g.Save();
                g.SetClip(rg, CombineMode.Intersect);
                Color[] cs = { Color.FromArgb(240, 80, 120), Color.FromArgb(90, 200, 220), Color.FromArgb(255, 200, 60) };
                for (int i = 0; i < 7; i++)
                    using (SolidBrush b = new SolidBrush(cs[i % 3])) g.FillRectangle(b, -24, -18 + i * 6, 48, 3.2f);
                g.Restore(st);
            }
            using (GraphicsPath th = ThumbPath(tx)) Stroke(g, ink, 2.4f, th);
            using (GraphicsPath pp = PalmPath()) Stroke(g, ink, 2.4f, pp);
            FingerLines(g, ink, 1.5f);
            Cuff(g, Color.FromArgb(240, 80, 120), ink, 11);
        }

        static void GloveLeather(Graphics g, float tx)
        {
            Color c = Color.FromArgb(70, 44, 32), d = Color.FromArgb(24, 14, 10);
            using (LinearGradientBrush b = VBrush(Color.FromArgb(110, 72, 52), c)) HandShape(g, tx, b, d, 2.4f);
            FillEl(g, A(70, Color.White), -9, -11, 11, 6);
            using (Pen st = new Pen(Color.FromArgb(190, 150, 110), 1.1f))
            {
                st.DashStyle = DashStyle.Dash;
                g.DrawLine(st, -8, -14, -7, -4);
                g.DrawLine(st, 0, -15, 0, -5);
                g.DrawLine(st, 8, -14, 7, -4);
            }
            Cuff(g, Color.FromArgb(50, 30, 22), d, 11);
            FillEl(g, Color.FromArgb(220, 180, 70), 8, 10, 5, 5);
        }

        static void GloveFingerless(Graphics g, float tx)
        {
            PlainHand(g, tx);
            Color c = Color.FromArgb(96, 104, 120), d = Color.FromArgb(40, 44, 54);
            GraphicsState st = g.Save();
            g.SetClip(new RectangleF(-30, -6, 60, 40), CombineMode.Intersect);
            using (LinearGradientBrush b = VBrush(Mix(c, Color.White, 0.2f), c)) HandShape(g, tx, b, d, 2.4f);
            g.Restore(st);
            Line(g, d, 2f, -16, -6, 16, -6);
            using (Pen r = P(A(120, d), 1.2f)) { g.DrawLine(r, -8, -6, -8, -2); g.DrawLine(r, 0, -6, 0, -2); g.DrawLine(r, 8, -6, 8, -2); }
            Cuff(g, Color.FromArgb(60, 66, 80), d, 11);
        }

        static void GloveBoxing(Graphics g, float tx)
        {
            Color d = Color.FromArgb(110, 10, 20);
            using (LinearGradientBrush b = VBrush(Color.FromArgb(255, 110, 100), Color.FromArgb(200, 30, 40)))
            {
                using (GraphicsPath th = new GraphicsPath()) { th.AddEllipse(tx * 1.05f - 7, -6, 14, 17); g.FillPath(b, th); Stroke(g, d, 2.4f, th); }
                using (GraphicsPath pp = new GraphicsPath()) { pp.AddEllipse(-19, -20, 38, 34); g.FillPath(b, pp); Stroke(g, d, 2.4f, pp); }
            }
            FillEl(g, A(170, Color.White), -11, -15, 13, 7);
            Bez(g, A(140, d), 1.6f, -12, -2, -4, 3, 4, 3, 12, -2);
            using (GraphicsPath cp = RR(new RectangleF(-17, 7, 34, 15), 5))
            {
                Fill(g, Color.FromArgb(250, 250, 250), cp);
                Stroke(g, d, 2f, cp);
            }
            using (Pen lc = P(Color.FromArgb(200, 30, 40), 1.4f))
                for (int i = 0; i < 3; i++) { g.DrawLine(lc, -4, 9 + i * 4, 4, 12 + i * 4); g.DrawLine(lc, 4, 9 + i * 4, -4, 12 + i * 4); }
        }

        static void GlovePaw(Graphics g, float tx)
        {
            Color ink = Color.FromArgb(120, 110, 120);
            using (LinearGradientBrush b = VBrush(Color.White, Color.FromArgb(222, 216, 222))) HandShape(g, tx, b, ink, 2.4f);
            using (Pen f = P(A(150, ink), 1.3f))
            {
                g.DrawLine(f, -16, -6, -19, -8); g.DrawLine(f, -16, 2, -19, 2);
                g.DrawLine(f, 16, -6, 19, -8); g.DrawLine(f, 16, 2, 19, 2);
            }
            Color pad = Color.FromArgb(255, 160, 184);
            using (GraphicsPath main = Heart(0, 5, 7)) { Fill(g, pad, main); }
            FillEl(g, pad, -12, -12, 7, 6);
            FillEl(g, pad, -3.5f, -15, 7, 6);
            FillEl(g, pad, 5, -12, 7, 6);
            FillEl(g, A(150, Color.White), -2, 1, 3, 2);
            Cuff(g, Color.FromArgb(240, 236, 240), ink, 11);
        }

        static void GloveMickey(Graphics g, float tx)
        {
            using (LinearGradientBrush b = VBrush(Color.White, Color.FromArgb(226, 228, 236))) HandShape(g, tx, b, Color.FromArgb(30, 30, 34), 2.6f);
            using (Pen l = P(Color.FromArgb(30, 30, 34), 2.2f))
            {
                g.DrawLine(l, -5, -3, -5, 6);
                g.DrawLine(l, 0, -4, 0, 7);
                g.DrawLine(l, 5, -3, 5, 6);
            }
            FingerLines(g, Color.FromArgb(150, 150, 160), 1.5f);
            using (GraphicsPath cp = RR(new RectangleF(-19, 6, 38, 15), 7))
            {
                using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(-19, 5, 38, 17), Color.White, Color.FromArgb(210, 212, 222), 90f)) g.FillPath(b, cp);
                Stroke(g, Color.FromArgb(30, 30, 34), 2.4f, cp);
            }
        }

        static void GloveGold(Graphics g, float tx)
        {
            Color d = Color.FromArgb(140, 92, 18);
            using (LinearGradientBrush b = VBrush(Color.FromArgb(255, 236, 140), Color.FromArgb(214, 150, 30))) HandShape(g, tx, b, d, 2.4f);
            FillEl(g, A(150, Color.White), -10, -12, 10, 6);
            FingerLines(g, Color.FromArgb(170, 112, 20), 1.8f);
            using (GraphicsPath cp = RR(new RectangleF(-16, 7, 32, 11), 5))
            {
                using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(-16, 6, 32, 13), Color.FromArgb(255, 220, 110), Color.FromArgb(180, 120, 20), 90f)) g.FillPath(b, cp);
                Stroke(g, d, 2f, cp);
            }
            Sparkle(g, 10, -10, 4, Color.FromArgb(255, 230, 140));
        }

        static void GloveKnight(Graphics g, float tx)
        {
            Color ink = Color.FromArgb(60, 64, 76);
            using (LinearGradientBrush b = VBrush(Color.FromArgb(246, 248, 252), Color.FromArgb(140, 148, 162))) HandShape(g, tx, b, ink, 2.4f);
            using (Pen pl = P(A(200, ink), 1.5f))
            {
                g.DrawBezier(pl, -15, -9, -6, -12, 6, -12, 15, -9);
                g.DrawBezier(pl, -16, -3, -6, -6, 6, -6, 16, -3);
                g.DrawBezier(pl, -16, 3, -6, 0, 6, 0, 16, 3);
            }
            FingerLines(g, A(160, ink), 1.3f);
            for (int i = -1; i <= 1; i++) FillEl(g, Color.FromArgb(220, 180, 70), i * 8 - 1.5f, -1.5f, 3, 3);
            FillEl(g, A(200, Color.White), -10, -14, 9, 4);
            using (GraphicsPath cf = new GraphicsPath())
            {
                cf.AddPolygon(new PointF[] { new PointF(-17, 6), new PointF(17, 6), new PointF(21, 21), new PointF(-21, 21) });
                using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(-22, 5, 44, 17), Color.FromArgb(230, 234, 242), Color.FromArgb(130, 138, 152), 90f)) g.FillPath(b, cf);
                Stroke(g, ink, 2f, cf);
            }
            Line(g, Color.FromArgb(220, 180, 70), 2f, -18, 18, 18, 18);
        }

        static readonly Color[] Stones = {
            Color.FromArgb(168, 85, 247), Color.FromArgb(59, 130, 246), Color.FromArgb(239, 68, 68),
            Color.FromArgb(249, 115, 22), Color.FromArgb(34, 197, 94), Color.FromArgb(250, 204, 21) };

        static void GloveInfinity(Graphics g, float tx, float t)
        {
            float pulse = 0.75f + 0.25f * Sin(t * 3);
            Glow(g, 0, 0, 36, 34, Color.FromArgb(255, 200, 80), (int)(120 * pulse));
            Color d = Color.FromArgb(120, 74, 10);
            using (LinearGradientBrush b = VBrush(Color.FromArgb(255, 226, 120), Color.FromArgb(196, 128, 20))) HandShape(g, tx, b, d, 2.4f);
            using (Pen pl = P(A(170, d), 1.4f))
            {
                g.DrawBezier(pl, -15, -4, -6, -7, 6, -7, 15, -4);
                g.DrawBezier(pl, -14, 8, -6, 6, 6, 6, 14, 8);
            }
            FingerLines(g, d, 1.6f);
            float[][] pos = { new float[] { -11, -10, 3 }, new float[] { -4, -12.5f, 3 }, new float[] { 4, -12.5f, 3 }, new float[] { 11, -10, 3 }, new float[] { 0, 1.5f, 5.5f }, new float[] { tx, 5, 3 } };
            for (int i = 0; i < 6; i++)
            {
                float gp = 0.55f + 0.45f * Sin(t * 4 + i * 1.05f);
                Glow(g, pos[i][0], pos[i][1], pos[i][2] * 3.2f, pos[i][2] * 3.2f, Stones[i], (int)(200 * gp));
                Gem(g, pos[i][0], pos[i][1], pos[i][2], Stones[i]);
            }
            using (GraphicsPath cp = RR(new RectangleF(-17, 7, 34, 12), 4))
            {
                using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(-17, 6, 34, 14), Color.FromArgb(255, 220, 110), Color.FromArgb(170, 110, 18), 90f)) g.FillPath(b, cp);
                Stroke(g, d, 2f, cp);
            }
            float sp = (t * 0.8f) % 1f;
            Sparkle(g, -16 + sp * 30, -20 + Sin(t * 2) * 4, 4.5f * Sin(sp * Math.PI), Color.FromArgb(255, 220, 120));
        }

        static void GlovePlasma(Graphics g, float tx, float t, float seed)
        {
            float pulse = 0.65f + 0.35f * Sin(t * 5 + seed);
            Color cy = Color.FromArgb(60, 230, 255);
            Glow(g, 0, 0, 38, 36, cy, (int)(110 * pulse));
            using (GraphicsPath pp = PalmPath()) GlowPath(g, pp, cy, pulse);
            using (LinearGradientBrush b = VBrush(Color.FromArgb(24, 40, 84), Color.FromArgb(8, 14, 34))) HandShape(g, tx, b, Color.FromArgb(150, 245, 255), 2f);
            using (Pen c = P(A(200, cy), 1.4f))
            {
                g.DrawLine(c, -8, -13, -8, -2); g.DrawLine(c, -8, -2, -2, 4);
                g.DrawLine(c, 0, -14, 0, 6);
                g.DrawLine(c, 8, -13, 8, -2); g.DrawLine(c, 8, -2, 2, 4);
            }
            FillEl(g, Color.White, -2.5f, 3.5f, 5, 5);
            Glow(g, 0, 6, 8, 8, cy, 220);
            Cuff(g, Color.FromArgb(14, 24, 52), Color.FromArgb(120, 230, 255), 11);
            Random rr = new Random((int)(t * 14) * 7 + (seed > 0 ? 3 : 11));
            for (int a = 0; a < 3; a++)
            {
                double ang = rr.NextDouble() * Math.PI * 2;
                PointF[] pts = new PointF[6];
                for (int i = 0; i < 6; i++)
                {
                    double rad = 15 + i * 3.6, jit = (rr.NextDouble() - 0.5) * 0.5;
                    pts[i] = new PointF(Cos(ang + jit) * (float)rad, Sin(ang + jit) * (float)rad);
                }
                using (Pen gp = P(A(90, cy), 4f)) g.DrawLines(gp, pts);
                using (Pen wp = P(Color.FromArgb(230, 230, 255, 255), 1.3f)) g.DrawLines(wp, pts);
            }
        }

        // ======================= GLASSES =======================
        public static void DrawGlasses(Graphics g, string id, float t)
        {
            if (id == null) return;
            switch (id)
            {
                case "gl_round": GlRound(g); break;
                case "gl_square": GlSquare(g); break;
                case "gl_hearts": GlHearts(g); break;
                case "gl_cateye": GlCatEye(g); break;
                case "gl_beach": GlBeach(g); break;
                case "gl_aviator": GlAviator(g); break;
                case "gl_3d": Gl3d(g); break;
                case "gl_ski": GlSki(g); break;
                case "gl_shutter": GlShutter(g); break;
                case "gl_pixel": GlPixel(g); break;
                case "gl_monocle": GlMonocle(g); break;
                case "gl_steampunk": GlSteampunk(g); break;
                case "gl_laser": GlLaser(g, t); break;
                case "gl_galaxy": GlGalaxy(g, t); break;
            }
        }

        static void Temples(Graphics g, Color c, float w, float lx, float rx, float y)
        {
            using (Pen p = P(c, w)) { g.DrawLine(p, lx, y, 116, y - 3); g.DrawLine(p, rx, y, 244, y - 3); }
        }

        static void Glare(Graphics g, float cx, float cy, float r)
        {
            using (Pen p = P(A(170, Color.White), 2f)) g.DrawArc(p, cx - r * 0.65f, cy - r * 0.65f, r * 1.3f, r * 1.3f, 200, 60);
        }

        static void GlRound(Graphics g)
        {
            Color fr = Color.FromArgb(40, 40, 44);
            FillEl(g, A(60, Color.FromArgb(200, 230, 255)), 136, 126, 36, 36);
            FillEl(g, A(60, Color.FromArgb(200, 230, 255)), 188, 126, 36, 36);
            Temples(g, fr, 3f, 136, 224, 142);
            using (Pen p = P(fr, 3.5f))
            {
                g.DrawEllipse(p, 136, 126, 36, 36);
                g.DrawEllipse(p, 188, 126, 36, 36);
                g.DrawBezier(p, 172, 141, 176, 136, 184, 136, 188, 141);
            }
            Glare(g, 154, 144, 18); Glare(g, 206, 144, 18);
        }

        static void GlSquare(Graphics g)
        {
            Color fr = Color.FromArgb(22, 22, 24);
            Temples(g, fr, 4f, 134, 226, 139);
            foreach (float x in new float[] { 133, 185 })
                using (GraphicsPath l = RR(new RectangleF(x, 129, 42, 31), 7))
                {
                    Fill(g, A(55, Color.FromArgb(200, 230, 255)), l);
                    Stroke(g, fr, 5f, l);
                }
            Line(g, fr, 4f, 175, 139, 185, 139);
            using (Pen p = P(A(160, Color.White), 2f)) { g.DrawLine(p, 140, 152, 150, 135); g.DrawLine(p, 192, 152, 202, 135); }
        }

        static void GlHearts(Graphics g)
        {
            Color fr = Color.FromArgb(190, 20, 80);
            Temples(g, fr, 3f, 138, 222, 140);
            foreach (float cx in new float[] { 154, 206 })
                using (GraphicsPath h = Heart(cx, 146, 18))
                {
                    using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(cx - 24, 126, 48, 40), Color.FromArgb(225, 255, 110, 160), Color.FromArgb(225, 220, 30, 90), 90f)) g.FillPath(b, h);
                    Stroke(g, fr, 3.2f, h);
                    FillEl(g, A(200, Color.White), cx - 12, 136, 7, 5);
                }
            Bez(g, fr, 3f, 173, 140, 177, 135, 183, 135, 187, 140);
        }

        static GraphicsPath CatEyeLens(float cx, float cy, float s)
        {
            GraphicsPath p = new GraphicsPath();
            p.AddBezier(cx + 17 * s, cy - 6, cx + 8 * s, cy - 13, cx - 10 * s, cy - 14, cx - 25 * s, cy - 19);
            p.AddBezier(cx - 25 * s, cy - 19, cx - 21 * s, cy - 4, cx - 14 * s, cy + 13, cx, cy + 13);
            p.AddBezier(cx, cy + 13, cx + 12 * s, cy + 13, cx + 19 * s, cy + 4, cx + 17 * s, cy - 6);
            p.CloseFigure();
            return p;
        }

        static void GlCatEye(Graphics g)
        {
            Color fr = Color.FromArgb(120, 20, 60);
            Temples(g, fr, 3f, 132, 228, 136);
            for (int k = 0; k < 2; k++)
            {
                float cx = k == 0 ? 154 : 206, s = k == 0 ? 1 : -1;
                using (GraphicsPath l = CatEyeLens(cx, 145, s))
                {
                    Fill(g, A(55, Color.FromArgb(255, 220, 240)), l);
                    Stroke(g, fr, 4.2f, l);
                }
                FillEl(g, Color.White, cx - 23 * s - 2, 125, 3.5f, 3.5f);
                FillEl(g, Color.White, cx - 19 * s - 1.5f, 129, 2.5f, 2.5f);
            }
            Bez(g, fr, 3f, 171, 140, 176, 136, 184, 136, 189, 140);
        }

        static void GlBeach(Graphics g)
        {
            Color fr = Color.FromArgb(30, 196, 200);
            Temples(g, fr, 3.5f, 135, 225, 142);
            foreach (float cx in new float[] { 154, 206 })
                using (GraphicsPath l = new GraphicsPath())
                {
                    l.AddEllipse(cx - 19, 126, 38, 38);
                    using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(cx - 20, 125, 40, 40), Color.FromArgb(215, 255, 160, 60), Color.FromArgb(215, 180, 50, 200), 90f)) g.FillPath(b, l);
                    Stroke(g, fr, 4.5f, l);
                    Glare(g, cx, 145, 19);
                }
            Bez(g, fr, 3.5f, 173, 141, 177, 136, 183, 136, 187, 141);
        }

        static GraphicsPath Teardrop(float cx, float cy)
        {
            GraphicsPath p = new GraphicsPath();
            p.AddBezier(cx - 20, cy - 8, cx - 12, cy - 14, cx + 12, cy - 14, cx + 20, cy - 8);
            p.AddBezier(cx + 20, cy - 8, cx + 22, cy + 10, cx + 10, cy + 19, cx, cy + 19);
            p.AddBezier(cx, cy + 19, cx - 12, cy + 19, cx - 22, cy + 8, cx - 20, cy - 8);
            p.CloseFigure();
            return p;
        }

        static void GlAviator(Graphics g)
        {
            Color gold = Color.FromArgb(201, 162, 39);
            Temples(g, gold, 2.2f, 134, 226, 137);
            foreach (float cx in new float[] { 154, 206 })
                using (GraphicsPath l = Teardrop(cx, 144))
                {
                    using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(cx - 23, 129, 46, 36), Color.FromArgb(240, 50, 34, 22), Color.FromArgb(215, 150, 104, 52), 90f)) g.FillPath(b, l);
                    Stroke(g, gold, 2.4f, l);
                    Line(g, A(130, Color.White), 2.2f, cx - 12, 149, cx - 4, 137);
                }
            Line(g, gold, 2.2f, 174, 136, 186, 136);
            Bez(g, gold, 2.2f, 174, 142, 178, 139, 182, 139, 186, 142);
        }

        static void Gl3d(Graphics g)
        {
            Temples(g, Color.FromArgb(235, 235, 235), 3.5f, 128, 232, 135);
            using (GraphicsPath f = RR(new RectangleF(126, 127, 108, 35), 6))
            {
                Fill(g, Color.FromArgb(248, 248, 248), f);
                Stroke(g, Color.FromArgb(140, 140, 150), 2f, f);
            }
            using (GraphicsPath l = RR(new RectangleF(132, 132, 40, 25), 4)) Fill(g, Color.FromArgb(205, 230, 40, 50), l);
            using (GraphicsPath r = RR(new RectangleF(188, 132, 40, 25), 4)) Fill(g, Color.FromArgb(205, 40, 200, 230), r);
            using (Pen p = P(Color.FromArgb(140, 140, 150), 2f)) g.DrawArc(p, 173, 150, 14, 14, 180, 180);
            using (Pen p = P(A(150, Color.White), 2f)) { g.DrawLine(p, 137, 153, 145, 136); g.DrawLine(p, 193, 153, 201, 136); }
        }

        static void GlSki(Graphics g)
        {
            Line(g, Color.FromArgb(40, 120, 220), 12f, 108, 146, 252, 146);
            Line(g, Color.FromArgb(255, 255, 255), 2f, 108, 146, 124, 146);
            Line(g, Color.FromArgb(255, 255, 255), 2f, 236, 146, 252, 146);
            using (GraphicsPath f = RR(new RectangleF(121, 124, 118, 44), 21))
            {
                Fill(g, Color.FromArgb(30, 30, 36), f);
                Stroke(g, Color.FromArgb(8, 8, 10), 2.4f, f);
            }
            using (GraphicsPath l = RR(new RectangleF(127, 129, 106, 34), 16))
            {
                using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(126, 128, 108, 36), Color.Orange, Color.Blue, 0f))
                {
                    ColorBlend cb = new ColorBlend();
                    cb.Colors = new Color[] { Color.FromArgb(255, 180, 40), Color.FromArgb(230, 70, 160), Color.FromArgb(70, 110, 240) };
                    cb.Positions = new float[] { 0, 0.5f, 1 };
                    b.InterpolationColors = cb;
                    g.FillPath(b, l);
                }
                GraphicsState st = g.Save();
                g.SetClip(l, CombineMode.Intersect);
                using (SolidBrush hl = new SolidBrush(Color.FromArgb(90, 255, 255, 255)))
                {
                    g.FillPolygon(hl, new PointF[] { new PointF(140, 129), new PointF(160, 129), new PointF(140, 163), new PointF(120, 163) });
                    g.FillPolygon(hl, new PointF[] { new PointF(166, 129), new PointF(172, 129), new PointF(152, 163), new PointF(146, 163) });
                }
                g.Restore(st);
                Stroke(g, Color.FromArgb(240, 240, 250), 1.4f, l);
            }
            using (Pen p = P(Color.FromArgb(30, 30, 36), 3f)) g.DrawArc(p, 170, 156, 20, 16, 180, 180);
        }

        static void GlShutter(Graphics g)
        {
            Color fr = Color.FromArgb(255, 60, 170);
            Temples(g, fr, 3f, 133, 227, 139);
            foreach (float cx in new float[] { 154, 206 })
            {
                RectangleF r = new RectangleF(cx - 21, 130, 42, 31);
                using (GraphicsPath l = RR(r, 6))
                {
                    Fill(g, A(40, fr), l);
                    using (Pen sl = P(fr, 2.8f))
                        for (int i = 1; i < 6; i++) g.DrawLine(sl, r.X + 2, r.Y + i * 5.2f, r.Right - 2, r.Y + i * 5.2f);
                    Stroke(g, fr, 4f, l);
                }
            }
            Line(g, fr, 3.5f, 175, 140, 185, 140);
        }

        static readonly string[] PixelRows = {
            "######################",
            ".#W#######..#W#######.",
            ".##W######..##W######.",
            "..#######....#######..",
            "...#####......#####..." };

        static void GlPixel(Graphics g)
        {
            using (Pen p = new Pen(Color.FromArgb(18, 18, 18), 4f)) { g.DrawLine(p, 126, 133, 114, 137); g.DrawLine(p, 234, 133, 246, 137); }
            using (SolidBrush bk = new SolidBrush(Color.FromArgb(18, 18, 18)))
            using (SolidBrush wh = new SolidBrush(Color.White))
            {
                for (int r = 0; r < PixelRows.Length; r++)
                    for (int c = 0; c < PixelRows[r].Length; c++)
                    {
                        char ch = PixelRows[r][c];
                        if (ch == '.') continue;
                        g.FillRectangle(ch == 'W' ? wh : bk, 125 + c * 5, 131 + r * 5, 5.4f, 5.4f);
                    }
            }
        }

        static void GlMonocle(Graphics g)
        {
            using (Pen ch = new Pen(Color.FromArgb(212, 168, 50), 2.4f))
            {
                ch.DashStyle = DashStyle.Dot;
                g.DrawBezier(ch, 222, 157, 238, 190, 228, 226, 206, 252);
            }
            FillEl(g, A(55, Color.FromArgb(220, 240, 255)), 187, 125, 38, 38);
            using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(184, 122, 44, 44), Color.FromArgb(255, 234, 140), Color.FromArgb(170, 112, 20), 45f))
            using (Pen p = new Pen(b, 4.5f)) g.DrawEllipse(p, 187, 125, 38, 38);
            using (Pen p = P(Color.FromArgb(110, 70, 10), 1f)) g.DrawEllipse(p, 184.8f, 122.8f, 42.4f, 42.4f);
            Glare(g, 206, 144, 19);
        }

        static void GlSteampunk(Graphics g)
        {
            Line(g, Color.FromArgb(110, 66, 40), 12f, 108, 146, 252, 146);
            using (Pen st = new Pen(Color.FromArgb(220, 170, 110), 1.2f))
            {
                st.DashStyle = DashStyle.Dash;
                g.DrawLine(st, 108, 142, 132, 142); g.DrawLine(st, 228, 142, 252, 142);
                g.DrawLine(st, 108, 150, 132, 150); g.DrawLine(st, 228, 150, 252, 150);
            }
            Line(g, Color.FromArgb(150, 100, 34), 5f, 172, 145, 188, 145);
            foreach (float cx in new float[] { 154, 206 })
            {
                using (GraphicsPath ring = new GraphicsPath())
                {
                    ring.AddEllipse(cx - 21, 124, 42, 42);
                    using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(cx - 22, 123, 44, 44), Color.FromArgb(240, 196, 110), Color.FromArgb(130, 80, 24), 45f)) g.FillPath(b, ring);
                    Stroke(g, Color.FromArgb(80, 46, 12), 2f, ring);
                }
                using (GraphicsPath lens = new GraphicsPath())
                {
                    lens.AddEllipse(cx - 14, 131, 28, 28);
                    using (PathGradientBrush b = new PathGradientBrush(lens))
                    {
                        b.CenterColor = Color.FromArgb(220, 150, 240, 220);
                        b.SurroundColors = new Color[] { Color.FromArgb(230, 20, 100, 100) };
                        g.FillPath(b, lens);
                    }
                    Stroke(g, Color.FromArgb(80, 46, 12), 1.6f, lens);
                }
                for (int i = 0; i < 8; i++)
                {
                    double a = i * Math.PI / 4;
                    FillEl(g, Color.FromArgb(90, 56, 20), cx + Cos(a) * 17.5f - 1.4f, 145 + Sin(a) * 17.5f - 1.4f, 2.8f, 2.8f);
                }
                Glare(g, cx, 145, 13);
            }
            using (GraphicsPath gear = Star(238, 124, 10, 7, 8, 0))
            {
                Grad(g, gear, Color.FromArgb(240, 196, 110), Color.FromArgb(140, 90, 30));
                Stroke(g, Color.FromArgb(80, 46, 12), 1.5f, gear);
            }
            FillEl(g, Color.FromArgb(80, 46, 12), 235, 121, 6, 6);
        }

        static void GlLaser(Graphics g, float t)
        {
            float pulse = 0.7f + 0.3f * Sin(t * 5);
            Color red = Color.FromArgb(255, 40, 40);
            using (GraphicsPath v = new GraphicsPath())
            {
                v.AddBezier(122, 133, 150, 124, 210, 124, 238, 133);
                v.AddLine(238, 133, 236, 157);
                v.AddBezier(236, 157, 210, 163, 150, 163, 124, 157);
                v.CloseFigure();
                Glow(g, 180, 146, 92, 36, red, (int)(100 * pulse));
                GlowPath(g, v, red, pulse);
                Grad(g, v, Color.FromArgb(150, 6, 24), Color.FromArgb(50, 0, 8));
                GraphicsState st = g.Save();
                g.SetClip(v, CombineMode.Intersect);
                using (Pen hl = P(Color.FromArgb(150, 255, 90, 90), 1.4f)) { g.DrawLine(hl, 120, 139, 240, 139); g.DrawLine(hl, 120, 150, 240, 150); }
                float sx = 130 + (Sin(t * 2.2f) * 0.5f + 0.5f) * 100;
                Glow(g, sx, 144, 18, 26, Color.FromArgb(255, 140, 140), 230);
                Line(g, Color.FromArgb(255, 235, 235), 2.2f, sx, 123, sx, 164);
                g.Restore(st);
                Stroke(g, Color.FromArgb(255, 110, 110), 2f, v);
            }
            using (GraphicsPath l = RR(new RectangleF(113, 135, 12, 18), 3)) { Fill(g, Color.FromArgb(70, 74, 84), l); Stroke(g, Color.FromArgb(20, 20, 24), 1.5f, l); }
            using (GraphicsPath r = RR(new RectangleF(235, 135, 12, 18), 3)) { Fill(g, Color.FromArgb(70, 74, 84), r); Stroke(g, Color.FromArgb(20, 20, 24), 1.5f, r); }
        }

        static void GlGalaxy(Graphics g, float t)
        {
            float pulse = 0.7f + 0.3f * Sin(t * 3);
            float hue = t * 60;
            Color frame = Hsv(hue + 60, 0.55f, 1f);
            Temples(g, Color.FromArgb(40, 30, 60), 3f, 135, 225, 142);
            for (int k = 0; k < 2; k++)
            {
                float cx = k == 0 ? 154 : 206, cy = 144;
                Glow(g, cx, cy, 32, 32, Hsv(hue + k * 40, 0.8f, 1f), (int)(150 * pulse));
                using (GraphicsPath l = new GraphicsPath())
                {
                    l.AddEllipse(cx - 19, cy - 19, 38, 38);
                    using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(cx - 20, cy - 20, 40, 40), Hsv(hue, 0.85f, 0.45f), Hsv(hue + 140, 0.9f, 0.75f), (t * 90 + k * 60) % 360))
                        g.FillPath(b, l);
                    GraphicsState st = g.Save();
                    g.SetClip(l, CombineMode.Intersect);
                    Glow(g, cx + Sin(t * 1.3 + k) * 6, cy + Cos(t * 1.1 + k) * 6, 13, 13, Hsv(hue + 240, 0.7f, 1f), 200);
                    for (int s = 0; s < 8; s++)
                    {
                        double ang = t * 0.9 + s * 0.8 + k;
                        float rad = 3 + (s * 5) % 15;
                        float sz = 1.2f + (s % 3) * 0.6f;
                        int a = (int)(160 + 95 * Sin(t * 6 + s * 2));
                        FillEl(g, A(a, Color.White), cx + Cos(ang) * rad - sz / 2, cy + Sin(ang) * rad - sz / 2, sz, sz);
                    }
                    g.Restore(st);
                    GlowPath(g, l, frame, pulse * 0.8f);
                    Stroke(g, frame, 3f, l);
                }
            }
            Bez(g, frame, 3f, 173, 141, 177, 136, 183, 136, 187, 141);
            float sp = (t * 0.5f) % 1f;
            Sparkle(g, 128 + sp * 104, 119 + Sin(sp * Math.PI * 2) * 4, 5 * Sin(sp * Math.PI), Hsv(hue, 0.5f, 1f));
        }

        // ======================= HATS =======================
        public static void DrawHat(Graphics g, string id, float t)
        {
            if (id == null) return;
            switch (id)
            {
                case "hat_beanie": Beanie(g, Color.FromArgb(128, 136, 150), Color.FromArgb(92, 100, 114), false); break;
                case "hat_cap": Cap(g); break;
                case "hat_bucket": Bucket(g); break;
                case "hat_headband": Headband(g); break;
                case "hat_flower": FlowerClip(g); break;
                case "hat_pompom": Beanie(g, Color.FromArgb(56, 116, 224), Color.FromArgb(34, 80, 176), true); break;
                case "hat_ushanka": Ushanka(g); break;
                case "hat_catears": CatEars(g); break;
                case "hat_cowboy": Cowboy(g); break;
                case "hat_tophat": TopHat(g); break;
                case "hat_crown": Crown(g); break;
                case "hat_viking": Viking(g); break;
                case "hat_firecrown": FireCrown(g, t); break;
                case "hat_halo": Halo(g, t); break;
            }
        }

        static void Beanie(Graphics g, Color c, Color dark, bool pompom)
        {
            using (GraphicsPath d = new GraphicsPath())
            {
                d.AddBezier(104, 104, 98, 50, 146, 28, 180, 28);
                d.AddBezier(180, 28, 214, 28, 262, 50, 256, 104);
                d.CloseFigure();
                Grad(g, d, Mix(c, Color.White, 0.15f), c);
                GraphicsState st = g.Save();
                g.SetClip(d, CombineMode.Intersect);
                if (pompom)
                    using (SolidBrush w = new SolidBrush(Color.FromArgb(245, 245, 250)))
                    {
                        g.FillRectangle(w, 98, 56, 164, 8);
                        g.FillRectangle(w, 98, 72, 164, 6);
                    }
                using (Pen k = P(A(70, dark), 1.6f))
                    for (int i = -4; i <= 4; i++)
                        g.DrawBezier(k, 180 + i * 6, 30, 180 + i * 10, 50, 180 + i * 15, 76, 180 + i * 17, 100);
                Glow(g, 150, 50, 26, 14, Color.White, 60);
                g.Restore(st);
                Stroke(g, Mix(dark, Color.Black, 0.4f), 2.6f, d);
            }
            using (GraphicsPath cp = RR(new RectangleF(100, 90, 160, 28), 12))
            {
                Fill(g, pompom ? Color.FromArgb(240, 240, 246) : dark, cp);
                GraphicsState st = g.Save();
                g.SetClip(cp, CombineMode.Intersect);
                using (Pen r = P(A(90, pompom ? Color.FromArgb(150, 160, 190) : Mix(dark, Color.Black, 0.4f)), 2f))
                    for (float x = 106; x < 258; x += 7) g.DrawLine(r, x, 92, x, 116);
                g.Restore(st);
                Stroke(g, Mix(dark, Color.Black, 0.4f), 2.6f, cp);
            }
            if (pompom)
            {
                using (SolidBrush w = new SolidBrush(Color.FromArgb(250, 250, 255)))
                using (Pen o = P(Color.FromArgb(150, 160, 190), 1.6f))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        double a = i * Math.PI / 5;
                        g.DrawEllipse(o, 180 + Cos(a) * 9 - 7, 22 + Sin(a) * 9 - 7, 14, 14);
                    }
                    for (int i = 0; i < 10; i++)
                    {
                        double a = i * Math.PI / 5;
                        g.FillEllipse(w, 180 + Cos(a) * 9 - 7, 22 + Sin(a) * 9 - 7, 14, 14);
                    }
                    g.FillEllipse(w, 168, 10, 24, 24);
                }
            }
        }

        static void Cap(Graphics g)
        {
            Color red = Color.FromArgb(220, 64, 64), dark = Color.FromArgb(150, 32, 38);
            using (GraphicsPath d = new GraphicsPath())
            {
                d.AddBezier(110, 106, 106, 54, 146, 34, 180, 34);
                d.AddBezier(180, 34, 214, 34, 254, 54, 250, 106);
                d.CloseFigure();
                Grad(g, d, Color.FromArgb(240, 100, 100), red);
                using (Pen s = P(A(130, dark), 1.8f))
                {
                    g.DrawLine(s, 180, 36, 180, 104);
                    g.DrawBezier(s, 176, 36, 150, 48, 140, 76, 136, 104);
                    g.DrawBezier(s, 184, 36, 210, 48, 220, 76, 224, 104);
                }
                Stroke(g, Color.FromArgb(90, 18, 22), 2.6f, d);
            }
            FillEl(g, Color.White, 165, 54, 30, 30);
            using (Pen o = P(Color.FromArgb(90, 18, 22), 1.6f)) g.DrawEllipse(o, 165, 54, 30, 30);
            using (GraphicsPath s = Star(180, 69, 10, 4.5f, 5, -90)) Fill(g, red, s);
            FillEl(g, dark, 174, 29, 12, 8);
            using (GraphicsPath v = new GraphicsPath())
            {
                v.AddBezier(106, 98, 130, 122, 230, 122, 254, 98);
                v.AddBezier(254, 98, 224, 108, 136, 108, 106, 98);
                v.CloseFigure();
                Fill(g, dark, v);
                Stroke(g, Color.FromArgb(90, 18, 22), 2.4f, v);
            }
        }

        static void Bucket(Graphics g)
        {
            Color c = Color.FromArgb(232, 212, 156), d = Color.FromArgb(160, 132, 80), ink = Color.FromArgb(96, 74, 36);
            using (GraphicsPath cr = new GraphicsPath())
            {
                cr.AddBezier(122, 100, 118, 56, 140, 36, 180, 36);
                cr.AddBezier(180, 36, 220, 36, 242, 56, 238, 100);
                cr.CloseFigure();
                Grad(g, cr, Mix(c, Color.White, 0.3f), c);
                GraphicsState st = g.Save();
                g.SetClip(cr, CombineMode.Intersect);
                using (SolidBrush b = new SolidBrush(d)) g.FillRectangle(b, 110, 80, 140, 12);
                using (Pen s = new Pen(A(160, ink), 1.1f))
                {
                    s.DashStyle = DashStyle.Dash;
                    g.DrawBezier(s, 132, 74, 150, 68, 210, 68, 228, 74);
                    g.DrawBezier(s, 138, 58, 156, 52, 204, 52, 222, 58);
                }
                g.Restore(st);
                Stroke(g, ink, 2.4f, cr);
            }
            using (GraphicsPath br = new GraphicsPath())
            {
                br.AddBezier(98, 110, 120, 86, 240, 86, 262, 110);
                br.AddBezier(262, 110, 250, 124, 110, 124, 98, 110);
                br.CloseFigure();
                Grad(g, br, Mix(c, Color.White, 0.15f), Mix(c, Color.Black, 0.1f));
                using (Pen s = new Pen(A(160, ink), 1.1f))
                {
                    s.DashStyle = DashStyle.Dash;
                    g.DrawBezier(s, 106, 110, 124, 94, 236, 94, 254, 110);
                }
                Stroke(g, ink, 2.4f, br);
            }
            using (GraphicsPath lf = new GraphicsPath())
            {
                lf.AddBezier(214, 84, 222, 74, 234, 74, 238, 80);
                lf.AddBezier(238, 80, 232, 88, 220, 88, 214, 84);
                Fill(g, Color.FromArgb(110, 180, 90), lf);
                Stroke(g, Color.FromArgb(50, 100, 40), 1.4f, lf);
            }
        }

        static void Headband(Graphics g)
        {
            Color red = Color.FromArgb(230, 50, 60), ink = Color.FromArgb(120, 20, 30);
            using (GraphicsPath tail = new GraphicsPath())
            {
                tail.AddBezier(244, 102, 256, 110, 262, 124, 268, 136);
                tail.AddLine(268, 136, 260, 138);
                tail.AddBezier(260, 138, 256, 126, 250, 114, 242, 110);
                tail.CloseFigure();
                Fill(g, Color.FromArgb(250, 250, 250), tail);
                Stroke(g, ink, 1.8f, tail);
            }
            using (GraphicsPath b = new GraphicsPath())
            {
                b.AddBezier(110, 100, 140, 82, 220, 82, 250, 100);
                b.AddLine(250, 100, 250, 116);
                b.AddBezier(250, 116, 220, 98, 140, 98, 110, 116);
                b.CloseFigure();
                Fill(g, Color.FromArgb(252, 252, 252), b);
                GraphicsState st = g.Save();
                g.SetClip(b, CombineMode.Intersect);
                Bez(g, red, 4f, 108, 108, 140, 90, 220, 90, 252, 108);
                g.Restore(st);
                Stroke(g, ink, 2.2f, b);
            }
            using (GraphicsPath s = Star(180, 92, 5, 2.2f, 5, -90)) Fill(g, Color.FromArgb(40, 40, 60), s);
        }

        static void FlowerClip(Graphics g)
        {
            float cx = 228, cy = 92;
            using (GraphicsPath lf = new GraphicsPath())
            {
                lf.AddBezier(212, 106, 206, 96, 210, 88, 220, 90);
                lf.AddBezier(220, 90, 222, 100, 220, 104, 212, 106);
                Fill(g, Color.FromArgb(110, 190, 90), lf);
                Stroke(g, Color.FromArgb(50, 110, 40), 1.4f, lf);
            }
            for (int i = 0; i < 5; i++)
            {
                double a = i * Math.PI * 2 / 5 - Math.PI / 2;
                float px = cx + Cos(a) * 9, py = cy + Sin(a) * 9;
                GradEl(g, new RectangleF(px - 7.5f, py - 7.5f, 15, 15), Color.FromArgb(255, 200, 220), Color.FromArgb(250, 120, 170));
                using (Pen o = P(Color.FromArgb(200, 60, 110), 1.4f)) g.DrawEllipse(o, px - 7.5f, py - 7.5f, 15, 15);
            }
            GradEl(g, new RectangleF(cx - 5.5f, cy - 5.5f, 11, 11), Color.FromArgb(255, 240, 120), Color.FromArgb(240, 180, 30));
            FillEl(g, A(200, Color.White), cx - 3, cy - 3.5f, 3, 2.5f);
        }

        static void Ushanka(Graphics g)
        {
            Color fur = Color.FromArgb(222, 194, 152), furD = Color.FromArgb(160, 128, 88), body = Color.FromArgb(100, 66, 42);
            foreach (float x in new float[] { 95, 231 })
                using (GraphicsPath f = RR(new RectangleF(x, 94, 34, 78), 14))
                {
                    Grad(g, f, Mix(fur, Color.White, 0.2f), fur);
                    using (Pen s = P(A(140, furD), 1.4f))
                        for (int i = 0; i < 7; i++) g.DrawLine(s, x + 6 + (i * 7) % 22, 104 + i * 9, x + 9 + (i * 7) % 22, 110 + i * 9);
                    Stroke(g, furD, 2.4f, f);
                }
            using (GraphicsPath d = new GraphicsPath())
            {
                d.AddBezier(108, 100, 102, 48, 146, 28, 180, 28);
                d.AddBezier(180, 28, 214, 28, 258, 48, 252, 100);
                d.CloseFigure();
                Grad(g, d, Mix(body, Color.White, 0.15f), body);
                Stroke(g, Color.FromArgb(50, 30, 18), 2.6f, d);
            }
            using (GraphicsPath b = RR(new RectangleF(98, 74, 164, 40), 18))
            {
                Grad(g, b, Mix(fur, Color.White, 0.25f), fur);
                using (Pen s = P(A(150, furD), 1.4f))
                    for (int i = 0; i < 22; i++)
                    {
                        float x = 106 + i * 7f, y = 80 + (i * 13) % 26;
                        g.DrawLine(s, x, y, x + 2, y + 5);
                    }
                Stroke(g, furD, 2.6f, b);
            }
            using (GraphicsPath s = Star(180, 94, 11, 4.5f, 5, -90))
            {
                Fill(g, Color.FromArgb(220, 40, 40), s);
                Stroke(g, Color.FromArgb(120, 10, 10), 1.6f, s);
            }
        }

        static void CatEars(Graphics g)
        {
            Color fur = Color.FromArgb(58, 54, 66), ink = Color.FromArgb(20, 18, 24), pink = Color.FromArgb(255, 170, 196);
            for (int k = 0; k < 2; k++)
            {
                float s = k == 0 ? 1 : -1;
                using (GraphicsPath e = new GraphicsPath())
                {
                    e.AddPolygon(new PointF[] { new PointF(180 - 54 * s, 78), new PointF(180 - 44 * s, 24), new PointF(180 - 14 * s, 58) });
                    Grad(g, e, Mix(fur, Color.White, 0.2f), fur);
                    using (Pen o = P(ink, 2.6f)) g.DrawPath(o, e);
                }
                using (GraphicsPath i = new GraphicsPath())
                {
                    i.AddPolygon(new PointF[] { new PointF(180 - 47 * s, 70), new PointF(180 - 42 * s, 38), new PointF(180 - 22 * s, 60) });
                    Fill(g, pink, i);
                }
            }
            Bez(g, ink, 7f, 116, 102, 126, 44, 234, 44, 244, 102);
            Bez(g, Color.FromArgb(90, 86, 100), 2f, 120, 98, 130, 46, 230, 46, 240, 98);
        }

        static void Cowboy(Graphics g)
        {
            Color c = Color.FromArgb(160, 104, 58), ink = Color.FromArgb(60, 34, 14);
            using (GraphicsPath cr = new GraphicsPath())
            {
                cr.AddBezier(132, 100, 128, 60, 134, 32, 156, 28);
                cr.AddBezier(156, 28, 168, 40, 192, 40, 204, 28);
                cr.AddBezier(204, 28, 226, 32, 232, 60, 228, 100);
                cr.AddBezier(228, 100, 200, 106, 160, 106, 132, 100);
                cr.CloseFigure();
                Grad(g, cr, Mix(c, Color.White, 0.2f), c);
                GraphicsState st = g.Save();
                g.SetClip(cr, CombineMode.Intersect);
                using (SolidBrush b = new SolidBrush(Color.FromArgb(70, 40, 20))) g.FillRectangle(b, 120, 82, 140, 12);
                Bez(g, A(100, ink), 1.6f, 180, 40, 178, 56, 180, 70, 180, 82);
                g.Restore(st);
                Stroke(g, ink, 2.4f, cr);
            }
            using (GraphicsPath s = Star(180, 88, 6, 2.6f, 5, -90)) { Fill(g, Color.FromArgb(255, 210, 80), s); Stroke(g, Color.FromArgb(140, 90, 10), 1f, s); }
            using (GraphicsPath br = new GraphicsPath())
            {
                br.AddBezier(84, 80, 98, 116, 262, 116, 276, 80);
                br.AddBezier(276, 80, 256, 100, 104, 100, 84, 80);
                br.CloseFigure();
                Grad(g, br, Mix(c, Color.White, 0.1f), Mix(c, Color.Black, 0.2f));
                Stroke(g, ink, 2.4f, br);
            }
        }

        static void TopHat(Graphics g)
        {
            using (GraphicsPath brim = new GraphicsPath())
            {
                brim.AddEllipse(106, 92, 148, 22);
                Fill(g, Color.FromArgb(26, 26, 30), brim);
                Stroke(g, Color.FromArgb(6, 6, 8), 2.4f, brim);
            }
            using (GraphicsPath c = new GraphicsPath())
            {
                c.AddLine(136, 100, 132, 16);
                c.AddBezier(132, 16, 150, 10, 210, 10, 228, 16);
                c.AddLine(228, 16, 224, 100);
                c.AddBezier(224, 100, 200, 106, 160, 106, 136, 100);
                c.CloseFigure();
                using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(130, 8, 100, 100), Color.FromArgb(66, 66, 76), Color.FromArgb(16, 16, 20), 0f)) g.FillPath(b, c);
                GraphicsState st = g.Save();
                g.SetClip(c, CombineMode.Intersect);
                using (SolidBrush band = new SolidBrush(Color.FromArgb(180, 34, 46))) g.FillRectangle(band, 120, 78, 130, 16);
                using (SolidBrush hl = new SolidBrush(Color.FromArgb(34, 255, 255, 255))) g.FillRectangle(hl, 146, 14, 9, 90);
                g.Restore(st);
                Stroke(g, Color.FromArgb(6, 6, 8), 2.4f, c);
            }
            FillEl(g, Color.FromArgb(70, 70, 82), 132, 9, 96, 13);
            using (Pen o = P(Color.FromArgb(6, 6, 8), 2f)) g.DrawEllipse(o, 132, 9, 96, 13);
            using (GraphicsPath card = RR(new RectangleF(204, 62, 13, 18), 2))
            {
                Fill(g, Color.White, card);
                Stroke(g, Color.FromArgb(60, 60, 60), 1f, card);
            }
            using (GraphicsPath h = Heart(210.5f, 71, 3.6f)) Fill(g, Color.FromArgb(200, 30, 40), h);
        }

        static GraphicsPath CrownPath()
        {
            GraphicsPath c = new GraphicsPath();
            c.AddLines(new PointF[] {
                new PointF(128, 106), new PointF(126, 66), new PointF(146, 84), new PointF(162, 46), new PointF(180, 76),
                new PointF(198, 46), new PointF(214, 84), new PointF(234, 66), new PointF(232, 106) });
            c.AddBezier(232, 106, 200, 112, 160, 112, 128, 106);
            c.CloseFigure();
            return c;
        }

        static void Crown(Graphics g)
        {
            using (GraphicsPath c = CrownPath())
            {
                Grad(g, c, Color.FromArgb(255, 234, 130), Color.FromArgb(214, 148, 28));
                GraphicsState st = g.Save();
                g.SetClip(c, CombineMode.Intersect);
                using (SolidBrush hl = new SolidBrush(Color.FromArgb(90, 255, 255, 255))) g.FillRectangle(hl, 120, 88, 120, 5);
                g.Restore(st);
                Stroke(g, Color.FromArgb(130, 84, 16), 2.4f, c);
            }
            foreach (PointF b in new PointF[] { new PointF(126, 63), new PointF(162, 43), new PointF(198, 43), new PointF(234, 63) })
                Gem(g, b.X, b.Y, 4.5f, Color.FromArgb(255, 220, 100));
            Gem(g, 180, 96, 8, Color.FromArgb(230, 40, 60));
            Gem(g, 151, 97, 5.5f, Color.FromArgb(50, 120, 240));
            Gem(g, 209, 97, 5.5f, Color.FromArgb(40, 190, 90));
            Sparkle(g, 222, 52, 6, Color.FromArgb(255, 230, 140));
        }

        static void Viking(Graphics g)
        {
            Color ivory = Color.FromArgb(250, 240, 214), ivD = Color.FromArgb(196, 172, 126), ink = Color.FromArgb(90, 70, 40);
            for (int k = 0; k < 2; k++)
            {
                float s = k == 0 ? 1 : -1;
                using (GraphicsPath h = new GraphicsPath())
                {
                    h.AddBezier(180 - 62 * s, 88, 180 - 86 * s, 80, 180 - 98 * s, 56, 180 - 92 * s, 24);
                    h.AddBezier(180 - 92 * s, 24, 180 - 82 * s, 46, 180 - 68 * s, 58, 180 - 46 * s, 66);
                    h.CloseFigure();
                    using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(180 - 100, 20, 200, 72), ivory, ivD, 90f)) g.FillPath(b, h);
                    Stroke(g, ink, 2.2f, h);
                }
                Bez(g, A(170, ink), 1.6f, 180 - 70 * s, 70, 180 - 76 * s, 76, 180 - 78 * s, 80, 180 - 74 * s, 84);
                Bez(g, A(170, ink), 1.6f, 180 - 80 * s, 58, 180 - 86 * s, 62, 180 - 88 * s, 66, 180 - 86 * s, 72);
            }
            using (GraphicsPath d = new GraphicsPath())
            {
                d.AddBezier(110, 104, 104, 48, 146, 30, 180, 30);
                d.AddBezier(180, 30, 214, 30, 256, 48, 250, 104);
                d.CloseFigure();
                Grad(g, d, Color.FromArgb(226, 232, 242), Color.FromArgb(124, 132, 148));
                GraphicsState st = g.Save();
                g.SetClip(d, CombineMode.Intersect);
                Glow(g, 150, 54, 28, 16, Color.White, 120);
                using (SolidBrush b = new SolidBrush(Color.FromArgb(190, 140, 50))) g.FillRectangle(b, 175, 28, 10, 70);
                g.Restore(st);
                Stroke(g, Color.FromArgb(56, 60, 70), 2.6f, d);
            }
            using (GraphicsPath band = RR(new RectangleF(104, 90, 152, 18), 7))
            {
                Grad(g, band, Color.FromArgb(230, 180, 80), Color.FromArgb(150, 100, 30));
                Stroke(g, Color.FromArgb(90, 56, 14), 2f, band);
            }
            for (float x = 114; x < 250; x += 16) FillEl(g, Color.FromArgb(255, 230, 150), x - 2, 97, 4, 4);
            using (GraphicsPath nose = RR(new RectangleF(176, 104, 8, 22), 3))
            {
                Grad(g, nose, Color.FromArgb(220, 226, 236), Color.FromArgb(130, 138, 152));
                Stroke(g, Color.FromArgb(56, 60, 70), 1.6f, nose);
            }
        }

        static void Flame(Graphics g, float x, float by, float h, float w, float t, int seed, bool core)
        {
            float f = Sin(t * 11 + seed * 1.7) * 0.12f + Sin(t * 17 + seed) * 0.08f;
            float hh = h * (1 + f);
            float sway = Sin(t * 6 + seed) * w * 0.4f;
            using (GraphicsPath fp = new GraphicsPath())
            {
                fp.AddBezier(x - w, by, x - w * 1.2f, by - hh * 0.45f, x - w * 0.3f + sway * 0.5f, by - hh * 0.6f, x + sway, by - hh);
                fp.AddBezier(x + sway, by - hh, x + w * 0.4f + sway * 0.5f, by - hh * 0.55f, x + w * 1.2f, by - hh * 0.4f, x + w, by);
                fp.CloseFigure();
                using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(x - w * 1.5f, by - hh - 1, w * 3f, hh + 2), Color.Red, Color.Yellow, 90f))
                {
                    ColorBlend cb = new ColorBlend();
                    if (core)
                    {
                        cb.Colors = new Color[] { Color.FromArgb(0, 255, 240, 160), Color.FromArgb(230, 255, 240, 160), Color.FromArgb(255, 255, 255, 230) };
                        cb.Positions = new float[] { 0, 0.5f, 1 };
                    }
                    else
                    {
                        cb.Colors = new Color[] { Color.FromArgb(0, 255, 40, 0), Color.FromArgb(230, 255, 90, 10), Color.FromArgb(255, 255, 200, 50) };
                        cb.Positions = new float[] { 0, 0.4f, 1 };
                    }
                    b.InterpolationColors = cb;
                    g.FillPath(b, fp);
                }
            }
        }

        static void FireCrown(Graphics g, float t)
        {
            float pulse = 0.7f + 0.3f * Sin(t * 4);
            Glow(g, 180, 62, 96, 64, Color.FromArgb(255, 110, 20), (int)(120 * pulse));
            float[][] fl = { new float[] { 126, 70, 34, 9 }, new float[] { 162, 52, 46, 11 }, new float[] { 198, 52, 46, 11 }, new float[] { 234, 70, 34, 9 }, new float[] { 180, 82, 32, 10 } };
            for (int i = 0; i < fl.Length; i++) Flame(g, fl[i][0], fl[i][1], fl[i][2], fl[i][3], t, i + 1, false);
            for (int i = 0; i < fl.Length; i++) Flame(g, fl[i][0], fl[i][1], fl[i][2] * 0.55f, fl[i][3] * 0.5f, t, i + 7, true);
            using (GraphicsPath c = CrownPath())
            {
                GlowPath(g, c, Color.FromArgb(255, 120, 30), pulse);
                Grad(g, c, Color.FromArgb(96, 40, 116), Color.FromArgb(26, 10, 36));
                GraphicsState st = g.Save();
                g.SetClip(c, CombineMode.Intersect);
                using (Pen lava = P(Color.FromArgb((int)(200 * pulse), 255, 130, 30), 1.8f))
                {
                    g.DrawLines(lava, new PointF[] { new PointF(134, 104), new PointF(140, 92), new PointF(136, 80) });
                    g.DrawLines(lava, new PointF[] { new PointF(224, 104), new PointF(218, 94), new PointF(224, 82) });
                    g.DrawLines(lava, new PointF[] { new PointF(166, 100), new PointF(170, 84), new PointF(164, 70) });
                    g.DrawLines(lava, new PointF[] { new PointF(194, 100), new PointF(190, 84), new PointF(196, 70) });
                }
                g.Restore(st);
                Stroke(g, Color.FromArgb(255, 150, 50), 2.2f, c);
            }
            Glow(g, 180, 96, 22, 22, Color.FromArgb(255, 80, 0), (int)(220 * pulse));
            Gem(g, 180, 96, 8, Color.FromArgb(255, 90, 20));
            Gem(g, 151, 97, 5f, Color.FromArgb(200, 60, 255));
            Gem(g, 209, 97, 5f, Color.FromArgb(200, 60, 255));
            for (int i = 0; i < 12; i++)
            {
                float ph = (t * 0.55f + i / 12f) % 1f;
                float x = 130 + (i * 53) % 100 + Sin(t * 3 + i) * 5;
                float y = 84 - ph * 90;
                float r = 2.4f * (1 - ph * 0.5f);
                Glow(g, x, y, r * 3, r * 3, Color.FromArgb(255, 140, 30), (int)(160 * (1 - ph)));
                FillEl(g, A((int)(255 * (1 - ph)), Color.FromArgb(255, 220, 120)), x - r, y - r, r * 2, r * 2);
            }
        }

        static void HaloBack(Graphics g, float t)
        {
            float y = 24 + Sin(t * 2) * 4;
            float pulse = 0.75f + 0.25f * Sin(t * 3);
            using (SolidBrush rb = new SolidBrush(Color.FromArgb((int)(22 * pulse), 255, 236, 160)))
                for (int i = 0; i < 12; i++)
                {
                    double a = (t * 20 + i * 30) * Math.PI / 180;
                    PointF[] tri = {
                        new PointF(180, y),
                        new PointF(180 + Cos(a - 0.08) * 140, y + Sin(a - 0.08) * 110),
                        new PointF(180 + Cos(a + 0.08) * 140, y + Sin(a + 0.08) * 110) };
                    g.FillPolygon(rb, tri);
                }
            Glow(g, 180, y + 30, 120, 110, Color.FromArgb(255, 236, 170), (int)(70 * pulse));
        }

        static void Halo(Graphics g, float t)
        {
            float y = 24 + Sin(t * 2) * 4;
            float pulse = 0.75f + 0.25f * Sin(t * 3);
            Glow(g, 180, y, 88, 36, Color.FromArgb(255, 220, 120), (int)(140 * pulse));
            RectangleF ring = new RectangleF(128, y - 11, 104, 22);
            float[] w = { 16, 10, 6, 2.5f };
            Color[] c = { A((int)(50 * pulse), Color.FromArgb(255, 210, 90)), A((int)(100 * pulse), Color.FromArgb(255, 220, 110)), Color.FromArgb(255, 238, 160), Color.White };
            for (int i = 0; i < 4; i++) using (Pen p = new Pen(c[i], w[i])) g.DrawEllipse(p, ring);
            for (int k = 0; k < 4; k++)
            {
                double a = t * 1.6 + k * Math.PI / 2;
                Sparkle(g, 180 + Cos(a) * 52, y + Sin(a) * 11, 4.5f + 2 * Sin(t * 5 + k), Color.FromArgb(255, 230, 140));
            }
        }
    }
}

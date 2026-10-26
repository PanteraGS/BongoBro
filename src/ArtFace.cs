using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BongoBro
{
    // Eyes, brows, nose, mouth and the "Face" item slot (expressions & make-up).
    public static partial class Art
    {
        const float EyeY = 146, EyeL = 154, EyeR = 206;
        const int EyeNormal = 0, EyeClosed = 1, EyeHappy = 2, EyeHeart = 3, EyeStar = 4, EyeFire = 5, EyeNeon = 6;

        static readonly PointF[] Freckles = {
            new PointF(140, 162), new PointF(146, 166), new PointF(136, 168), new PointF(151, 161), new PointF(143, 172),
            new PointF(220, 162), new PointF(214, 166), new PointF(224, 168), new PointF(209, 161), new PointF(217, 172),
            new PointF(176, 158), new PointF(185, 157), new PointF(180, 162) };

        // drawn inside the face clip, under the features
        static void FaceUnder(Graphics g, Pose p)
        {
            string f = p.Face;
            float t = p.T;
            if (f == "fc_blush" || f == "fc_hearts" || f == "fc_grin" || f == "fc_wink") Blush(g, f == "fc_blush" || f == "fc_hearts");
            switch (f)
            {
                case "fc_freckles":
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(170, 186, 112, 74)))
                        for (int i = 0; i < Freckles.Length; i++)
                        {
                            float s = 2f + (i % 3) * 0.5f;
                            g.FillEllipse(b, Freckles[i].X - s / 2, Freckles[i].Y - s / 2, s, s);
                        }
                    break;
                case "fc_warpaint":
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(225, 40, 70, 170)))
                        for (int k = 0; k < 2; k++)
                        {
                            float s = k == 0 ? 1 : -1, c = k == 0 ? EyeL : EyeR;
                            g.FillPolygon(b, new PointF[] { new PointF(c - 18 * s, 164), new PointF(c + 8 * s, 162), new PointF(c + 6 * s, 167), new PointF(c - 20 * s, 169) });
                            g.FillPolygon(b, new PointF[] { new PointF(c - 16 * s, 173), new PointF(c + 4 * s, 171), new PointF(c + 2 * s, 176), new PointF(c - 18 * s, 178) });
                        }
                    break;
                case "fc_clown":
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(200, 80, 170, 255)))
                        foreach (float c in new float[] { EyeL, EyeR })
                            g.FillPolygon(b, new PointF[] { new PointF(c, 118), new PointF(c + 11, 146), new PointF(c, 178), new PointF(c - 11, 146) });
                    FillEl(g, Color.FromArgb(200, 240, 60, 70), 120, 168, 20, 18);
                    FillEl(g, Color.FromArgb(200, 240, 60, 70), 220, 168, 20, 18);
                    Bez(g, Color.FromArgb(220, 230, 40, 50), 8f, 156, 176, 166, 208, 194, 208, 204, 176);
                    break;
                case "fc_skull":
                    using (SolidBrush w = new SolidBrush(Color.FromArgb(185, 255, 255, 255))) g.FillRectangle(w, 100, 60, 160, 160);
                    foreach (float c in new float[] { EyeL, EyeR })
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            double a = i * Math.PI / 5;
                            Color pc = i % 2 == 0 ? Color.FromArgb(255, 90, 160) : Color.FromArgb(40, 200, 190);
                            FillEl(g, pc, c + Cos(a) * 22 - 4.5f, EyeY + Sin(a) * 22 - 4.5f, 9, 9);
                        }
                        FillEl(g, Color.FromArgb(235, 50, 30, 60), c - 19, EyeY - 19, 38, 38);
                    }
                    using (Pen sw = P(Color.FromArgb(200, 140, 60, 200), 1.8f))
                    {
                        g.DrawBezier(sw, 128, 186, 134, 178, 142, 184, 138, 190);
                        g.DrawBezier(sw, 232, 186, 226, 178, 218, 184, 222, 190);
                    }
                    for (int i = 0; i < 5; i++) FillEl(g, Color.FromArgb(200, 255, 90, 160), 168 + i * 6 - 1.5f, 203, 3, 3);
                    break;
                case "fc_neon":
                    {
                        Color c = Hsv(t * 90, 0.8f, 1f);
                        float pulse = 0.7f + 0.3f * Sin(t * 4);
                        using (GraphicsPath n = new GraphicsPath())
                        {
                            n.StartFigure(); n.AddLines(new PointF[] { new PointF(124, 160), new PointF(138, 168), new PointF(138, 182), new PointF(150, 189) });
                            n.StartFigure(); n.AddLines(new PointF[] { new PointF(236, 160), new PointF(222, 168), new PointF(222, 182), new PointF(210, 189) });
                            n.StartFigure(); n.AddLines(new PointF[] { new PointF(166, 205), new PointF(180, 198), new PointF(194, 205) });
                            GlowPath(g, n, c, pulse * 0.4f);
                            Stroke(g, Mix(c, Color.White, 0.5f), 1.8f, n);
                        }
                        foreach (PointF d in new PointF[] { new PointF(150, 189), new PointF(210, 189), new PointF(180, 198) })
                            FillEl(g, Color.White, d.X - 2, d.Y - 2, 4, 4);
                        break;
                    }
                case "fc_fireeyes":
                    Glow(g, EyeL, EyeY + 6, 28, 24, Color.FromArgb(255, 110, 20), 90);
                    Glow(g, EyeR, EyeY + 6, 28, 24, Color.FromArgb(255, 110, 20), 90);
                    break;
            }
        }

        static void Blush(Graphics g, bool strong)
        {
            int a = strong ? 175 : 105;
            Color c = Color.FromArgb(255, 128, 140);
            Glow(g, 134, 174, strong ? 21 : 17, strong ? 11 : 8.5f, c, a);
            Glow(g, 226, 174, strong ? 21 : 17, strong ? 11 : 8.5f, c, a);
            using (Pen h = P(Color.FromArgb(strong ? 200 : 120, 232, 104, 116), 1.4f))
                for (int i = 0; i < 3; i++)
                {
                    float x = 127 + i * 6;
                    g.DrawLine(h, x, 177, x + 4, 170);
                    g.DrawLine(h, x + 92, 177, x + 96, 170);
                }
        }

        static void DrawEyes(Graphics g, Pose p)
        {
            int mode = EyeNormal;
            switch (p.Face)
            {
                case "fc_hearts": mode = EyeHeart; break;
                case "fc_stars": mode = EyeStar; break;
                case "fc_fireeyes": mode = EyeFire; break;
                case "fc_neon": mode = EyeNeon; break;
            }
            int left = mode, right = mode;
            if (p.Face == "fc_wink") right = EyeHappy;
            if (p.Face == "fc_grin") left = right = EyeHappy;
            if (p.Blink)
            {
                if (left != EyeHappy) left = EyeClosed;
                if (right != EyeHappy) right = EyeClosed;
            }
            DrawEye(g, EyeL, EyeY, -1, left, p.T);
            DrawEye(g, EyeR, EyeY, 1, right, p.T);
        }

        static void DrawEye(Graphics g, float cx, float cy, float outer, int mode, float t)
        {
            Color lash = Color.FromArgb(48, 32, 26);
            if (mode == EyeClosed)
            {
                Bez(g, lash, 3f, cx - 13, cy + 1, cx - 6, cy + 4, cx + 6, cy + 4, cx + 13, cy);
                return;
            }
            if (mode == EyeHappy)
            {
                Bez(g, lash, 3.2f, cx - 12, cy + 4, cx - 6, cy - 4, cx + 6, cy - 4, cx + 12, cy + 4);
                return;
            }
            Color top = Color.FromArgb(70, 100, 130), bot = Color.FromArgb(150, 182, 206), pupil = Color.FromArgb(24, 30, 40);
            if (mode == EyeFire) { top = Color.FromArgb(170, 30, 0); bot = Color.FromArgb(255, 220, 80); pupil = Color.FromArgb(90, 10, 0); }
            if (mode == EyeNeon) { top = Hsv(t * 90, 0.9f, 0.45f); bot = Hsv(t * 90 + 50, 0.6f, 1f); pupil = Hsv(t * 90, 0.9f, 0.2f); }
            if (mode == EyeFire || mode == EyeNeon)
            {
                float pulse = 0.7f + 0.3f * Sin(t * 5);
                Glow(g, cx, cy, 24, 22, mode == EyeFire ? Color.FromArgb(255, 140, 20) : Hsv(t * 90, 0.8f, 1f), (int)((mode == EyeFire ? 170 : 90) * pulse));
            }
            // narrow almond eye with a heavy upper lid
            float o = outer;
            using (GraphicsPath e = new GraphicsPath())
            {
                e.AddBezier(cx - 14 * o, cy + 1, cx - 8 * o, cy - 8, cx + 7 * o, cy - 8, cx + 14 * o, cy - 2);
                e.AddBezier(cx + 14 * o, cy - 2, cx + 7 * o, cy + 7, cx - 7 * o, cy + 7, cx - 14 * o, cy + 1);
                e.CloseFigure();
                Fill(g, Color.FromArgb(250, 248, 246), e);
                GraphicsState st = g.Save();
                g.SetClip(e, CombineMode.Intersect);
                if (mode == EyeHeart)
                {
                    using (GraphicsPath h = Heart(cx, cy, 7.5f))
                    {
                        Grad(g, h, Color.FromArgb(255, 120, 160), Color.FromArgb(214, 30, 80));
                        Stroke(g, Color.FromArgb(140, 10, 50), 1.2f, h);
                    }
                }
                else
                {
                    using (GraphicsPath ir = new GraphicsPath())
                    {
                        ir.AddEllipse(cx - 7.5f, cy - 7, 15, 15);
                        Grad(g, ir, top, bot);
                        Stroke(g, Mix(top, Color.Black, 0.4f), 1.2f, ir);
                    }
                    if (mode == EyeStar)
                        using (GraphicsPath s = Star(cx, cy + 0.5f, 6, 2.6f, 5, -90))
                        {
                            Grad(g, s, Color.FromArgb(255, 246, 150), Color.FromArgb(255, 190, 30));
                            Stroke(g, Color.FromArgb(170, 110, 0), 0.8f, s);
                        }
                    else FillEl(g, pupil, cx - 3.4f, cy - 3, 6.8f, 6.8f);
                }
                FillEl(g, A(235, Color.White), cx - 4, cy - 4.5f, 3.4f, 3.4f);
                using (LinearGradientBrush sh = new LinearGradientBrush(new RectangleF(cx - 15, cy - 9, 30, 8), Color.FromArgb(110, 120, 90, 80), Color.FromArgb(0, 120, 90, 80), 90f))
                    g.FillRectangle(sh, cx - 15, cy - 9, 30, 7);
                g.Restore(st);
            }
            Bez(g, lash, 3f, cx - 14.5f * o, cy + 1, cx - 8 * o, cy - 8.5f, cx + 7 * o, cy - 8.5f, cx + 14.5f * o, cy - 2.5f);
            Bez(g, A(170, SkinDeep), 1.5f, cx - 11 * o, cy - 9, cx - 5 * o, cy - 13, cx + 5 * o, cy - 13, cx + 12 * o, cy - 9);
            Bez(g, A(110, SkinDeep), 1.3f, cx - 10 * o, cy + 7, cx - 4 * o, cy + 9, cx + 4 * o, cy + 9, cx + 10 * o, cy + 6);
        }

        static void DrawBrows(Graphics g, Pose p)
        {
            bool angry = p.Face == "fc_warpaint" || p.Face == "fc_fireeyes";
            float dy = p.Face == "fc_grin" || p.Face == "fc_stars" ? -3 : 0;
            for (int k = 0; k < 2; k++)
            {
                float s = k == 0 ? 1 : -1;
                // thick, almost straight brow: inner end lower and fuller
                float inY = angry ? 132 : 129, outY = angry ? 122 : 126;
                using (GraphicsPath b = new GraphicsPath())
                {
                    b.AddBezier(180 - 12 * s, inY + dy, 180 - 22 * s, inY - 4 + dy, 180 - 34 * s, outY - 3 + dy, 180 - 44 * s, outY + 1 + dy);
                    b.AddBezier(180 - 44 * s, outY + 1 + dy, 180 - 34 * s, outY + 1 + dy, 180 - 22 * s, inY + 1 + dy, 180 - 12 * s, inY + 4.5f + dy);
                    b.CloseFigure();
                    Fill(g, HairDark, b);
                    Stroke(g, HairDark, 1.6f, b);
                }
            }
        }

        static void DrawNose(Graphics g, Pose p)
        {
            switch (p.Face)
            {
                case "fc_clown":
                    GradEl(g, new RectangleF(171, 157, 20, 20), Color.FromArgb(255, 100, 100), Color.FromArgb(196, 18, 30));
                    using (Pen o = P(Color.FromArgb(120, 10, 20), 1.6f)) g.DrawEllipse(o, 171, 157, 20, 20);
                    FillEl(g, A(220, Color.White), 175, 160, 6, 4);
                    return;
                case "fc_cat":
                    using (GraphicsPath n = new GraphicsPath())
                    {
                        n.AddPolygon(new PointF[] { new PointF(175, 164), new PointF(187, 164), new PointF(181, 171) });
                        Fill(g, Color.FromArgb(255, 140, 170), n);
                        Stroke(g, Color.FromArgb(160, 60, 90), 1.4f, n);
                    }
                    return;
                case "fc_skull":
                    using (GraphicsPath h = Heart(181, 166, 6)) Fill(g, Color.FromArgb(50, 30, 60), h);
                    return;
            }
            Glow(g, 186, 156, 6, 14, SkinShade, 160);
            Bez(g, SkinDeep, 2.2f, 184, 138, 186, 152, 190, 160, 188, 167);
            Bez(g, SkinDeep, 2.2f, 172, 168, 176, 173, 184, 174, 190, 169);
            FillEl(g, A(150, SkinDeep), 174, 168, 4, 2.6f);
            FillEl(g, A(150, SkinDeep), 183, 168, 4, 2.6f);
            FillEl(g, A(170, Color.White), 178, 150, 3, 9);
        }

        static void DrawMouth(Graphics g, Pose p)
        {
            Color line = Color.FromArgb(150, 70, 62);
            switch (p.Face)
            {
                case "fc_grin":
                case "fc_clown":
                    using (GraphicsPath m = new GraphicsPath())
                    {
                        m.AddBezier(161, 180, 172, 184, 190, 184, 201, 180);
                        m.AddBezier(201, 180, 199, 202, 163, 202, 161, 180);
                        m.CloseFigure();
                        Fill(g, Color.FromArgb(110, 28, 40), m);
                        GraphicsState st = g.Save();
                        g.SetClip(m, CombineMode.Intersect);
                        FillEl(g, Color.FromArgb(255, 120, 132), 168, 190, 26, 16);
                        using (SolidBrush w = new SolidBrush(Color.White)) g.FillRectangle(w, 160, 178, 42, 6);
                        g.Restore(st);
                        Stroke(g, Ink, 2.4f, m);
                    }
                    return;
                case "fc_hearts":
                case "fc_stars":
                case "fc_blush":
                    using (GraphicsPath m = new GraphicsPath())
                    {
                        m.AddBezier(170, 182, 176, 184, 186, 184, 192, 182);
                        m.AddBezier(192, 182, 190, 195, 172, 195, 170, 182);
                        m.CloseFigure();
                        Fill(g, Color.FromArgb(140, 40, 52), m);
                        GraphicsState st = g.Save();
                        g.SetClip(m, CombineMode.Intersect);
                        FillEl(g, Color.FromArgb(255, 130, 140), 174, 188, 14, 9);
                        g.Restore(st);
                        Stroke(g, Ink, 2.2f, m);
                    }
                    return;
                case "fc_tongue":
                    using (GraphicsPath tg = RR(new RectangleF(176, 184, 13, 14), 6))
                    {
                        Grad(g, tg, Color.FromArgb(255, 140, 156), Color.FromArgb(230, 80, 104));
                        Stroke(g, Color.FromArgb(150, 40, 60), 1.6f, tg);
                    }
                    Line(g, Color.FromArgb(180, 60, 80), 1.2f, 182.5f, 188, 182.5f, 194);
                    Bez(g, line, 2.6f, 166, 183, 174, 188, 188, 188, 196, 182);
                    return;
                case "fc_cat":
                    Bez(g, line, 2.4f, 170, 181, 172, 188, 179, 189, 181, 183);
                    Bez(g, line, 2.4f, 181, 183, 183, 189, 190, 188, 192, 181);
                    return;
                case "fc_skull":
                    Line(g, Color.FromArgb(50, 30, 60), 2.4f, 158, 186, 204, 186);
                    for (float x = 162; x < 204; x += 6) Line(g, Color.FromArgb(50, 30, 60), 1.6f, x, 182, x, 190);
                    return;
                case "fc_warpaint":
                case "fc_fireeyes":
                    Bez(g, line, 2.6f, 168, 187, 176, 184, 186, 184, 194, 187);
                    return;
            }
            // calm, slightly lopsided closed mouth
            Line(g, A(90, SkinDeep), 1.4f, 180, 176, 180, 181);
            Bez(g, Color.FromArgb(140, 88, 78), 2.6f, 164, 188, 172, 190, 188, 190, 197, 186);
            Bez(g, A(120, SkinDeep), 1.6f, 171, 195, 177, 197.5f, 184, 197.5f, 190, 195);
        }

        // drawn on top of the face features
        static void FaceOver(Graphics g, Pose p)
        {
            float t = p.T;
            switch (p.Face)
            {
                case "fc_mustache":
                    for (int k = 0; k < 2; k++)
                    {
                        float s = k == 0 ? 1 : -1;
                        using (GraphicsPath m = new GraphicsPath())
                        {
                            m.AddBezier(180, 175, 172 - 0 * s, 171, 162, 172, 154, 178);
                            m.AddBezier(154, 178, 148, 183, 140, 181, 141, 173);
                            m.AddBezier(141, 173, 143, 184, 152, 189, 162, 185);
                            m.AddBezier(162, 185, 170, 183, 176, 182, 180, 181);
                            m.CloseFigure();
                            if (k == 1) using (Matrix mx = new Matrix(-1, 0, 0, 1, 360, 0)) m.Transform(mx);
                            Grad(g, m, Color.FromArgb(150, 104, 62), Color.FromArgb(90, 58, 30));
                            Stroke(g, Color.FromArgb(60, 36, 18), 1.6f, m);
                        }
                    }
                    break;
                case "fc_cat":
                    using (Pen w = P(Color.FromArgb(90, 64, 64), 1.6f))
                        for (int k = 0; k < 2; k++)
                        {
                            float s = k == 0 ? 1 : -1;
                            g.DrawLine(w, 180 - 32 * s, 172, 180 - 58 * s, 166);
                            g.DrawLine(w, 180 - 32 * s, 177, 180 - 60 * s, 178);
                            g.DrawLine(w, 180 - 32 * s, 182, 180 - 56 * s, 190);
                        }
                    break;
                case "fc_hearts":
                    foreach (float[] h in new float[][] { new float[] { 236, 112, 7 }, new float[] { 250, 94, 4.5f }, new float[] { 124, 104, 4 } })
                        using (GraphicsPath hp = Heart(h[0], h[1], h[2]))
                        {
                            Grad(g, hp, Color.FromArgb(255, 130, 170), Color.FromArgb(230, 40, 100));
                            Stroke(g, Color.FromArgb(150, 20, 60), 1.2f, hp);
                        }
                    break;
                case "fc_stars":
                    Sparkle(g, 128, 124, 6, Color.FromArgb(255, 220, 90));
                    Sparkle(g, 238, 132, 5, Color.FromArgb(255, 220, 90));
                    Sparkle(g, 232, 194, 3.5f, Color.FromArgb(255, 220, 90));
                    break;
                case "fc_wink":
                    Sparkle(g, 230, 128, 5, Color.FromArgb(255, 220, 120));
                    break;
                case "fc_fireeyes":
                    for (int k = 0; k < 2; k++)
                    {
                        float outer = k == 0 ? -1 : 1, cx = k == 0 ? EyeL : EyeR;
                        GraphicsState st = g.Save();
                        g.TranslateTransform(cx + 12 * outer, EyeY - 6);
                        g.RotateTransform(68 * outer);
                        Flame(g, 0, 0, 30, 6.5f, t, 20 + k, false);
                        Flame(g, 0, 0, 17, 3.4f, t, 30 + k, true);
                        g.Restore(st);
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        float ph = (t * 0.8f + i / 6f) % 1f;
                        float side = i % 2 == 0 ? -1 : 1;
                        float x = 180 + side * (40 + ph * 40), y = EyeY - 8 - ph * 34;
                        FillEl(g, A((int)(255 * (1 - ph)), Color.FromArgb(255, 200, 90)), x - 1.5f, y - 1.5f, 3, 3);
                    }
                    break;
                case "fc_neon":
                    {
                        float ph = (t * 0.6f) % 1f;
                        Sparkle(g, 124 + ph * 112, 150 + Sin(ph * Math.PI * 2) * 30, 4 * Sin(ph * Math.PI), Hsv(t * 90 + 120, 0.6f, 1f));
                        break;
                    }
            }
        }
    }
}

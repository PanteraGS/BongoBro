using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BongoBro
{
    // Stasyan: long wavy hair, round wire glasses, mustache with a chin tuft,
    // and his "glass of beer" suit: foam on the shoulders, beer with bubbles below.
    public static partial class Art
    {
        static readonly Color Beer = Color.FromArgb(240, 178, 34);
        static readonly Color BeerDeep = Color.FromArgb(206, 128, 16);
        static readonly Color BeerLight = Color.FromArgb(255, 216, 96);
        static readonly Color Foam = Color.FromArgb(252, 250, 242);
        static readonly Color FoamShade = Color.FromArgb(226, 220, 204);

        static void DrawBodyStas(Graphics g, Pose p)
        {
            using (GraphicsPath tp = TorsoPath())
            {
                BeerFill(g, tp, 254, 1);
                Stroke(g, JacketInk, 3f, tp);
            }
            // white t-shirt between the open lapels
            using (GraphicsPath ts = new GraphicsPath())
            {
                ts.AddPolygon(new PointF[] { new PointF(146, 238), new PointF(214, 238), new PointF(206, 336), new PointF(154, 336) });
                Grad(g, ts, Color.White, Color.FromArgb(226, 226, 222));
                Stroke(g, A(120, Ink), 1.6f, ts);
            }
            // lapels: foam at the top turning into beer, like the jacket
            for (int k = 0; k < 2; k++)
            {
                float s = k == 0 ? 1 : -1;
                using (GraphicsPath lp = new GraphicsPath())
                {
                    lp.AddPolygon(new PointF[] {
                        new PointF(180 - 36 * s, 236), new PointF(180 - 24 * s, 240), new PointF(180 - 30 * s, 300),
                        new PointF(180 - 26 * s, 336), new PointF(180 - 46 * s, 336), new PointF(180 - 52 * s, 280) });
                    BeerFill(g, lp, 262, 3 + k);
                    Stroke(g, JacketInk, 2f, lp);
                }
            }
            DrawNeck(g);
            // t-shirt collar
            Bez(g, Color.FromArgb(214, 214, 210), 5f, 156, 238, 164, 252, 196, 252, 204, 238);
            DrawBeerTie(g);
        }

        // Fills a shape with foam above foamY (with drips) and bubbly beer below.
        static void BeerFill(Graphics g, GraphicsPath shape, float foamY, int seed)
        {
            RectangleF r = shape.GetBounds();
            GraphicsState st = g.Save();
            g.SetClip(shape, CombineMode.Intersect);
            using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(r.X, foamY - 10, r.Width, r.Bottom - foamY + 12), BeerLight, BeerDeep, 90f))
                g.FillRectangle(b, r.X - 2, r.Y - 2, r.Width + 4, r.Height + 4);
            // a few deeper amber streaks and the bubbles rising in the beer
            Glow(g, r.X + r.Width * 0.25f, r.Bottom, r.Width * 0.2f, 30, BeerDeep, 120);
            Glow(g, r.X + r.Width * 0.75f, r.Bottom, r.Width * 0.2f, 30, BeerDeep, 120);
            Random rnd = new Random(seed * 31 + 7);
            int n = (int)(r.Width * (r.Bottom - foamY) / 140);
            using (Pen ring = P(A(170, Color.FromArgb(255, 236, 170)), 1.1f))
            using (SolidBrush shine = new SolidBrush(A(200, Color.White)))
                for (int i = 0; i < n; i++)
                {
                    float d = 2.5f + (float)rnd.NextDouble() * 4.5f;
                    float x = r.X + (float)rnd.NextDouble() * r.Width, y = foamY + 6 + (float)rnd.NextDouble() * (r.Bottom - foamY);
                    g.DrawEllipse(ring, x, y, d, d);
                    g.FillEllipse(shine, x + d * 0.2f, y + d * 0.15f, d * 0.35f, d * 0.3f);
                }
            // foam head with a wavy, dripping lower edge
            using (GraphicsPath f = new GraphicsPath())
            {
                float x0 = r.X - 4, x1 = r.Right + 4;
                f.AddLine(x0, r.Y - 4, x1, r.Y - 4);
                f.AddLine(x1, r.Y - 4, x1, foamY);
                float step = 14;
                for (float x = x1; x > x0; x -= step)
                {
                    float drip = rnd.Next(4) == 0 ? 10 + (float)rnd.NextDouble() * 10 : 3 + (float)rnd.NextDouble() * 4;
                    f.AddBezier(x, foamY, x - 2, foamY + drip, x - step + 2, foamY + drip, x - step, foamY);
                }
                f.AddLine(x0, foamY, x0, r.Y - 4);
                f.CloseFigure();
                using (LinearGradientBrush fb = new LinearGradientBrush(new RectangleF(r.X, r.Y - 4, r.Width, foamY - r.Y + 24), Foam, FoamShade, 90f))
                    g.FillPath(fb, f);
                using (Pen e = P(A(120, Color.FromArgb(214, 170, 90)), 1.4f)) g.DrawPath(e, f);
                // foam pores
                using (SolidBrush pore = new SolidBrush(A(70, Color.FromArgb(200, 186, 150))))
                    for (int i = 0; i < n / 2 + 4; i++)
                    {
                        float d = 1.5f + (float)rnd.NextDouble() * 2.5f;
                        g.FillEllipse(pore, r.X + (float)rnd.NextDouble() * r.Width, r.Y + (float)rnd.NextDouble() * (foamY - r.Y), d, d);
                    }
            }
            g.Restore(st);
        }

        // white tie whose lower half is a glass of beer
        static void DrawBeerTie(Graphics g)
        {
            using (GraphicsPath knot = new GraphicsPath())
            {
                knot.AddPolygon(new PointF[] { new PointF(172, 242), new PointF(188, 242), new PointF(185, 256), new PointF(175, 256) });
                Grad(g, knot, Color.White, Color.FromArgb(218, 218, 214));
                Stroke(g, A(170, Ink), 1.6f, knot);
            }
            using (GraphicsPath tie = new GraphicsPath())
            {
                tie.AddPolygon(new PointF[] { new PointF(175, 256), new PointF(185, 256), new PointF(193, 320), new PointF(180, 334), new PointF(167, 320) });
                BeerFill(g, tie, 272, 9);
                Stroke(g, A(170, Ink), 1.6f, tie);
            }
        }

        static void DrawHeadStas(Graphics g, Pose p)
        {
            float t = p.T;
            if (p.Hat == "hat_halo") HaloBack(g, t);
            StasBackHair(g);
            using (GraphicsPath face = FacePath())
            {
                Fill(g, Skin, face);
                GraphicsState st = g.Save();
                g.SetClip(face, CombineMode.Intersect);
                Glow(g, 114, 156, 20, 60, SkinShade, 190);
                Glow(g, 246, 156, 20, 60, SkinShade, 190);
                Glow(g, 150, 168, 18, 12, SkinShade, 110);
                Glow(g, 210, 168, 18, 12, SkinShade, 110);
                Glow(g, 180, 204, 46, 16, Color.FromArgb(150, 116, 92), 50);
                FaceUnder(g, p);
                g.Restore(st);
                Stroke(g, Ink, 2.6f, face);
            }
            DrawEyes(g, p);
            DrawNose(g, p);
            DrawMouth(g, p);
            StasMustache(g);
            FaceOver(g, p);
            StasFrontHair(g);
            DrawBrows(g, p);
            if (p.Glasses == null) StasGlasses(g);
            else DrawGlasses(g, p.Glasses, t);
            DrawHat(g, p.Hat, t);
            MoodOverlay(g, p);
        }

        // long wavy hair falling behind the face down to the shoulders
        static void StasBackHair(Graphics g)
        {
            using (GraphicsPath h = new GraphicsPath())
            {
                h.AddBezier(180, 40, 120, 38, 94, 80, 98, 130);
                h.AddBezier(98, 130, 100, 160, 92, 180, 100, 204);
                h.AddBezier(100, 204, 106, 222, 128, 228, 140, 216);
                h.AddBezier(140, 216, 148, 208, 150, 200, 150, 192);
                h.AddLine(150, 192, 210, 192);
                h.AddBezier(210, 192, 210, 200, 212, 208, 220, 216);
                h.AddBezier(220, 216, 232, 228, 254, 222, 260, 204);
                h.AddBezier(260, 204, 268, 180, 260, 160, 262, 130);
                h.AddBezier(262, 130, 266, 80, 240, 38, 180, 40);
                h.CloseFigure();
                Grad(g, h, HairC, HairDark);
                GraphicsState st = g.Save();
                g.SetClip(h, CombineMode.Intersect);
                using (Pen s = P(A(150, HairDark), 2f))
                using (Pen hl = P(A(110, HairLight), 2.4f))
                    for (int k = 0; k < 2; k++)
                    {
                        float d = k == 0 ? 1 : -1;
                        for (int i = 0; i < 4; i++)
                        {
                            float x = 180 - (70 - i * 7) * d;
                            g.DrawBezier(s, x, 120, x - 10 * d, 150, x + 8 * d, 180, x - 4 * d, 218);
                            g.DrawBezier(hl, x + 3 * d, 124, x - 7 * d, 154, x + 11 * d, 184, x - 1 * d, 214);
                        }
                    }
                g.Restore(st);
                using (Pen o = new Pen(HairDark, 2.4f)) { o.LineJoin = LineJoin.Round; g.DrawPath(o, h); }
            }
        }

        // middle part, curtains framing the face and covering the ears
        static void StasFrontHair(Graphics g)
        {
            using (GraphicsPath h = new GraphicsPath())
            {
                h.AddBezier(180, 46, 146, 44, 112, 62, 108, 104);
                h.AddBezier(108, 104, 104, 136, 110, 166, 118, 188);
                h.AddBezier(118, 188, 126, 180, 130, 172, 130, 164);
                h.AddBezier(130, 164, 128, 168, 130, 172, 130, 174);
                h.AddBezier(130, 174, 124, 150, 124, 124, 136, 106);
                h.AddBezier(136, 106, 146, 90, 164, 80, 180, 78);
                h.AddBezier(180, 78, 196, 80, 214, 90, 224, 106);
                h.AddBezier(224, 106, 236, 124, 236, 150, 230, 174);
                h.AddBezier(230, 174, 230, 172, 230, 168, 230, 164);
                h.AddBezier(230, 164, 230, 172, 234, 180, 242, 188);
                h.AddBezier(242, 188, 250, 166, 256, 136, 252, 104);
                h.AddBezier(252, 104, 248, 62, 214, 44, 180, 46);
                h.CloseFigure();
                Grad(g, h, HairLight, HairC);
                GraphicsState st = g.Save();
                g.SetClip(h, CombineMode.Intersect);
                using (Pen s = P(A(160, HairDark), 1.8f))
                using (Pen hl = P(A(130, Color.FromArgb(222, 184, 136)), 3f))
                    for (int k = 0; k < 2; k++)
                    {
                        float d = k == 0 ? 1 : -1;
                        g.DrawBezier(s, 178 - 2 * d, 52, 160 - 4 * d, 56, 132 + 0 * d, 70, 120 + 0 * d, 100);
                        g.DrawBezier(s, 178 - 2 * d, 62, 158 - 2 * d, 66, 140, 78, 128, 104);
                        g.DrawBezier(s, 118, 110, 112, 136, 118, 160, 119, 180);
                        g.DrawBezier(s, 128, 112, 120, 136, 126, 152, 125, 170);
                        g.DrawBezier(hl, 172, 52, 150, 56, 128, 72, 118, 106);
                        // mirror for the right curtain
                        g.TranslateTransform(360, 0);
                        g.ScaleTransform(-1, 1);
                    }
                g.Restore(st);
                Line(g, A(200, HairDark), 2f, 180, 46, 180, 78);
                using (Pen o = new Pen(HairDark, 2.4f)) { o.LineJoin = LineJoin.Round; g.DrawPath(o, h); }
            }
        }

        // thin mustache with turned-down ends and a small tuft on the chin
        static void StasMustache(Graphics g)
        {
            Color c = Color.FromArgb(235, 96, 64, 40);
            using (GraphicsPath m = new GraphicsPath())
            {
                m.AddBezier(154, 188, 162, 172, 198, 172, 206, 188);
                m.AddBezier(206, 188, 196, 179, 164, 179, 154, 188);
                m.CloseFigure();
                using (SolidBrush b = new SolidBrush(c)) g.FillPath(b, m);
            }
            using (GraphicsPath tuft = new GraphicsPath())
            {
                tuft.AddBezier(170, 199, 170, 212, 176, 216, 180, 216);
                tuft.AddBezier(180, 216, 184, 216, 190, 212, 190, 199);
                tuft.AddBezier(190, 199, 184, 203, 176, 203, 170, 199);
                tuft.CloseFigure();
                using (SolidBrush b = new SolidBrush(A(220, c))) g.FillPath(b, tuft);
            }
            using (Pen h = P(A(120, Color.FromArgb(150, 110, 76)), 1f))
                for (int i = 0; i < 5; i++) g.DrawLine(h, 173 + i * 3.5f, 203, 174 + i * 3.2f, 212);
        }

        // his own round wire-rim glasses
        static void StasGlasses(Graphics g)
        {
            Color fr = Color.FromArgb(150, 120, 70);
            using (Pen rim = P(fr, 2.2f))
            using (Pen dark = P(A(160, Color.FromArgb(70, 54, 30)), 0.8f))
                foreach (float cx in new float[] { EyeL, EyeR })
                {
                    RectangleF r = new RectangleF(cx - 18, EyeY - 17, 36, 34);
                    using (SolidBrush lens = new SolidBrush(Color.FromArgb(34, 200, 230, 255))) g.FillEllipse(lens, r);
                    g.DrawEllipse(rim, r);
                    g.DrawEllipse(dark, r.X - 1.2f, r.Y - 1.2f, r.Width + 2.4f, r.Height + 2.4f);
                    Line(g, A(150, Color.White), 1.6f, r.X + 8, r.Bottom - 9, r.X + 15, r.Y + 8);
                }
            Bez(g, fr, 2f, EyeL + 18, EyeY - 2, EyeL + 22, EyeY - 7, EyeR - 22, EyeY - 7, EyeR - 18, EyeY - 2);
            Line(g, fr, 2f, EyeL - 18, EyeY - 2, 128, EyeY - 5);
            Line(g, fr, 2f, EyeR + 18, EyeY - 2, 232, EyeY - 5);
        }
    }
}

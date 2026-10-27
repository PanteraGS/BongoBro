using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BongoBro
{
    // Character palettes and Vlad: a watermelon with Vlad's hair, glasses and beard.
    public static partial class Art
    {
        static string paletteFor;

        // Skin, hair and shirt colors are shared by the body, hands and face features.
        public static void UseCharacter(string id)
        {
            id = Characters.Normalize(id);
            if (paletteFor == id) return;
            paletteFor = id;
            if (id == Characters.Vlad)
            {
                Skin = Color.FromArgb(150, 204, 96);       // light rind green: hands, neck
                SkinShade = Color.FromArgb(112, 172, 70);
                SkinDeep = Color.FromArgb(52, 112, 44);    // nose and creases
                Ink = Color.FromArgb(26, 62, 28);
                HairC = Color.FromArgb(118, 86, 58);
                HairDark = Color.FromArgb(70, 48, 30);
                HairLight = Color.FromArgb(166, 126, 88);
                Shirt = Color.FromArgb(52, 74, 152);
                ShirtHi = Color.FromArgb(92, 118, 200);
            }
            else if (id == Characters.Stas)
            {
                Skin = Color.FromArgb(244, 212, 190);
                SkinShade = Color.FromArgb(228, 182, 158);
                SkinDeep = Color.FromArgb(192, 136, 110);
                Ink = Color.FromArgb(62, 40, 32);
                HairC = Color.FromArgb(118, 80, 48);
                HairDark = Color.FromArgb(64, 40, 22);
                HairLight = Color.FromArgb(168, 122, 78);
                Shirt = Color.FromArgb(232, 168, 30);       // beer-print jacket sleeves
                ShirtHi = Color.FromArgb(255, 212, 92);
            }
            else
            {
                Skin = Color.FromArgb(246, 210, 184);
                SkinShade = Color.FromArgb(232, 180, 152);
                SkinDeep = Color.FromArgb(196, 132, 106);
                Ink = Color.FromArgb(62, 40, 32);
                HairC = Color.FromArgb(132, 94, 60);
                HairDark = Color.FromArgb(78, 54, 34);
                HairLight = Color.FromArgb(176, 132, 88);
                Shirt = Color.FromArgb(44, 46, 52);
                ShirtHi = Color.FromArgb(78, 80, 90);
            }
        }

        static readonly RectangleF Melon = new RectangleF(98, 54, 164, 168);

        static void DrawHeadVlad(Graphics g, Pose p)
        {
            float t = p.T;
            if (p.Hat == "hat_halo") HaloBack(g, t);
            using (GraphicsPath m = new GraphicsPath())
            {
                m.AddEllipse(Melon);
                using (PathGradientBrush b = new PathGradientBrush(m))
                {
                    b.CenterPoint = new PointF(160, 112);
                    b.CenterColor = Color.FromArgb(150, 210, 96);
                    b.SurroundColors = new Color[] { Color.FromArgb(52, 128, 48) };
                    g.FillPath(b, m);
                }
                GraphicsState st = g.Save();
                g.SetClip(m, CombineMode.Intersect);
                MelonStripes(g);
                Glow(g, 180, 152, 62, 52, Color.FromArgb(176, 226, 120), 120);
                Glow(g, 138, 92, 26, 16, Color.White, 110);
                Glow(g, 180, 222, 90, 30, Color.FromArgb(16, 60, 20), 120);
                Beard(g);
                FaceUnder(g, p);
                g.Restore(st);
                Stroke(g, Ink, 3f, m);
            }
            BittenSlice(g);
            DrawEyes(g, p);
            DrawNose(g, p);
            DrawMouth(g, p);
            Mustache(g);
            FaceOver(g, p);
            VladHair(g);
            DrawBrows(g, p);
            if (p.Glasses == null) VladGlasses(g);
            else DrawGlasses(g, p.Glasses, t);
            DrawHat(g, p.Hat, t);
            MoodOverlay(g, p);
        }

        // dark zig-zag stripes that follow the curve of the melon
        static void MelonStripes(Graphics g)
        {
            float cx = Melon.X + Melon.Width / 2, cy = Melon.Y + Melon.Height / 2, ry = Melon.Height / 2;
            using (Pen p = P(Color.FromArgb(200, 34, 98, 38), 10f))
                for (int i = -3; i <= 3; i++)
                {
                    PointF[] pts = new PointF[15];
                    for (int k = 0; k < pts.Length; k++)
                    {
                        float y = Melon.Y + k * Melon.Height / (pts.Length - 1);
                        float e = (float)Math.Sqrt(Math.Max(0, 1 - Math.Pow((y - cy) / ry, 2)));
                        float wave = (float)Math.Sin(k * 1.3 + i) * 2.2f * e;
                        pts[k] = new PointF(cx + i * 30 * e + wave, y);
                    }
                    g.DrawCurve(p, pts, 0.5f);
                }
            // lighter "face" area so the features read clearly over the stripes
            using (GraphicsPath f = new GraphicsPath())
            {
                f.AddEllipse(116, 104, 128, 104);
                using (PathGradientBrush b = new PathGradientBrush(f))
                {
                    b.CenterColor = Color.FromArgb(235, 176, 222, 120);
                    b.SurroundColors = new Color[] { Color.FromArgb(0, 176, 222, 120) };
                    b.FocusScales = new PointF(0.55f, 0.5f);
                    g.FillPath(b, f);
                }
            }
        }

        // short full beard on the lower half, leaving the mouth open
        static void Beard(Graphics g)
        {
            using (GraphicsPath b = new GraphicsPath())
            {
                b.AddBezier(104, 160, 120, 182, 146, 184, 180, 182);
                b.AddBezier(180, 182, 214, 184, 240, 182, 256, 160);
                b.AddLine(256, 160, 262, 230);
                b.AddLine(262, 230, 98, 230);
                b.CloseFigure();
                using (Region r = new Region(b))
                using (GraphicsPath mouth = new GraphicsPath())
                {
                    mouth.AddEllipse(162, 183, 38, 14);
                    r.Exclude(mouth);
                    using (SolidBrush br = new SolidBrush(Color.FromArgb(215, 120, 88, 58))) g.FillRegion(br, r);
                    GraphicsState st = g.Save();
                    g.SetClip(r, CombineMode.Intersect);
                    Random rnd = new Random(11);
                    using (Pen h = P(Color.FromArgb(150, 62, 42, 28), 1.4f))
                    using (Pen l = P(Color.FromArgb(90, 160, 122, 90), 1.2f))
                        for (int i = 0; i < 140; i++)
                        {
                            float x = 104 + (float)rnd.NextDouble() * 152, y = 162 + (float)rnd.NextDouble() * 64;
                            g.DrawLine(i % 3 == 0 ? l : h, x, y, x + 1.2f, y + 3.5f);
                        }
                    Glow(g, 180, 214, 60, 16, Color.FromArgb(40, 26, 16), 110);
                    g.Restore(st);
                }
            }
        }

        static void Mustache(Graphics g)
        {
            using (GraphicsPath m = new GraphicsPath())
            {
                m.AddBezier(160, 183, 166, 172, 194, 172, 200, 183);
                m.AddBezier(200, 183, 192, 178, 168, 178, 160, 183);
                m.CloseFigure();
                using (SolidBrush b = new SolidBrush(Color.FromArgb(240, 88, 62, 42))) g.FillPath(b, m);
            }
        }

        // a bitten-off wedge on the upper right, showing red flesh and seeds
        static void BittenSlice(Graphics g)
        {
            using (GraphicsPath w = new GraphicsPath())
            {
                w.AddBezier(236, 86, 246, 96, 254, 108, 258, 122);
                w.AddBezier(258, 122, 250, 120, 242, 121, 234, 124);
                w.AddBezier(234, 124, 238, 112, 238, 98, 236, 86);
                w.CloseFigure();
                Grad(g, w, Color.FromArgb(255, 110, 120), Color.FromArgb(214, 40, 56));
                Stroke(g, Color.FromArgb(236, 248, 214), 2.6f, w);
                Stroke(g, Ink, 1.4f, w);
            }
            foreach (PointF s in new PointF[] { new PointF(242, 104), new PointF(248, 114), new PointF(240, 116) })
            {
                FillEl(g, Color.FromArgb(30, 20, 20), s.X - 1.6f, s.Y - 2.4f, 3.2f, 4.8f);
                FillEl(g, A(160, Color.White), s.X - 0.8f, s.Y - 2f, 1.2f, 1.4f);
            }
        }

        // tousled, side-parted hair swept from his right to his left, with a fringe over the forehead
        static void VladHair(Graphics g)
        {
            PointF[] fringe = {
                new PointF(246, 98), new PointF(234, 92), new PointF(222, 100), new PointF(212, 88), new PointF(198, 102),
                new PointF(188, 90), new PointF(170, 106), new PointF(162, 92), new PointF(144, 110), new PointF(138, 98),
                new PointF(122, 114) };
            using (GraphicsPath h = new GraphicsPath())
            {
                h.AddBezier(116, 116, 102, 76, 132, 38, 182, 34);
                h.AddBezier(182, 34, 232, 30, 262, 58, 250, 102);
                h.AddLine(250, 102, fringe[0].X, fringe[0].Y);
                for (int i = 0; i + 1 < fringe.Length; i++)
                {
                    PointF a = fringe[i], b = fringe[i + 1];
                    h.AddBezier(a.X, a.Y, a.X - 4, a.Y + 2, b.X + 6, b.Y - 4, b.X, b.Y);
                }
                h.AddLine(fringe[fringe.Length - 1].X, fringe[fringe.Length - 1].Y, 116, 116);
                h.CloseFigure();
                Grad(g, h, HairLight, HairC);
                GraphicsState st = g.Save();
                g.SetClip(h, CombineMode.Intersect);
                using (Pen s = P(A(150, HairDark), 1.8f))
                {
                    g.DrawBezier(s, 136, 56, 160, 50, 196, 56, 226, 76);
                    g.DrawBezier(s, 130, 70, 156, 62, 192, 70, 216, 90);
                    g.DrawBezier(s, 124, 88, 146, 80, 170, 88, 188, 100);
                    g.DrawBezier(s, 150, 40, 170, 38, 200, 42, 236, 62);
                }
                using (Pen hl = P(A(130, Color.FromArgb(220, 186, 140)), 4f))
                    g.DrawBezier(hl, 142, 50, 166, 44, 196, 48, 220, 62);
                g.Restore(st);
                using (Pen o = new Pen(HairDark, 2.6f)) { o.LineJoin = LineJoin.Round; g.DrawPath(o, h); }
            }
        }

        // his own thin rectangular glasses (semi-rimless: dark top bar, thin lower rim)
        static void VladGlasses(Graphics g)
        {
            Color fr = Color.FromArgb(44, 40, 46);
            using (Pen thin = P(Color.FromArgb(150, 90, 90, 100), 1.4f))
            using (Pen top = P(fr, 3.4f))
                foreach (float x in new float[] { 132, 186 })
                {
                    RectangleF r = new RectangleF(x, 133, 42, 26);
                    FillEl(g, A(40, Color.FromArgb(220, 240, 255)), r.X, r.Y, r.Width, r.Height);
                    using (GraphicsPath l = RR(r, 7))
                    {
                        using (SolidBrush lens = new SolidBrush(Color.FromArgb(36, 200, 230, 255))) g.FillPath(lens, l);
                        g.DrawPath(thin, l);
                    }
                    g.DrawBezier(top, r.X + 1, r.Y + 6, r.X + 4, r.Y - 1, r.Right - 4, r.Y - 1, r.Right - 1, r.Y + 6);
                    Line(g, A(150, Color.White), 1.8f, r.X + 7, r.Bottom - 6, r.X + 15, r.Y + 6);
                }
            Bez(g, fr, 2.6f, 174, 138, 177, 134, 183, 134, 186, 138);
            Line(g, fr, 3f, 132, 139, 104, 136);
            Line(g, fr, 3f, 228, 139, 256, 136);
        }

        // thin chain necklace over the t-shirt collar
        static void DrawChain(Graphics g)
        {
            using (Pen c = new Pen(Color.FromArgb(214, 218, 226), 1.6f))
            {
                c.DashStyle = DashStyle.Dot;
                g.DrawBezier(c, 160, 232, 166, 262, 196, 262, 202, 232);
            }
        }
    }
}

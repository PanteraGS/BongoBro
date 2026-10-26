using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BongoBro
{
    public enum Mood { Normal, Yawn, Sleep, Surprised, Focused, Happy }
    public enum DayPhase { Day, Morning, Evening, Night }

    public class Pose
    {
        public bool LeftDown, RightDown, OnMouse, MouseDown, Blink;
        public int LitKey = -1;
        public float MouseDX, MouseDY, T;
        public string Hat, Glasses, Gloves, Face, Theme, Pet;
        public string Character = Characters.Yura;
        public Mood Mood;
        public DayPhase Day;
        public float PetHop;   // 0..1, the pet jumps on key presses

        public string Equipped(Slot s)
        {
            switch (s)
            {
                case Slot.Hat: return Hat;
                case Slot.Glasses: return Glasses;
                case Slot.Gloves: return Gloves;
                case Slot.Face: return Face;
                case Slot.Pet: return Pet;
                default: return Theme;
            }
        }

        public string ActiveSet { get { return GearSet.Active(Equipped); } }

        public static Pose FromSave(SaveData s, float t)
        {
            Pose p = new Pose();
            p.T = t;
            p.Hat = s.Hat; p.Glasses = s.Glasses; p.Gloves = s.Gloves; p.Face = s.Face; p.Theme = s.Theme; p.Pet = s.Pet;
            p.Character = s.Character;
            return p;
        }
    }

    // All character art is drawn in a 360x360 base canvas.
    public static partial class Art
    {
        static readonly Stopwatch clock = Stopwatch.StartNew();
        public static float Now { get { return (float)(clock.Elapsed.TotalSeconds % 3600.0); } }

        public static Color Skin = Color.FromArgb(246, 210, 184);
        public static Color SkinShade = Color.FromArgb(232, 180, 152);
        public static Color SkinDeep = Color.FromArgb(196, 132, 106);
        public static Color Ink = Color.FromArgb(62, 40, 32);
        public static Color HairC = Color.FromArgb(132, 94, 60);
        public static Color HairDark = Color.FromArgb(78, 54, 34);
        public static Color HairLight = Color.FromArgb(176, 132, 88);
        public static readonly Color Jacket = Color.FromArgb(40, 42, 48);
        public static readonly Color JacketHi = Color.FromArgb(70, 72, 82);
        public static readonly Color JacketInk = Color.FromArgb(14, 15, 20);

        // overall character size inside the canvas, and the cartoon head enlargement
        const float SceneScale = 0.86f, HeadScale = 1.04f;

        static readonly PointF[] stubble;

        static Art()
        {
            Random r = new Random(42);
            List<PointF> pts = new List<PointF>();
            while (pts.Count < 110)
            {
                float x = 132 + (float)r.NextDouble() * 96, y = 172 + (float)r.NextDouble() * 44;
                float dx = (x - 180) / 44f, dy = (y - 204) / 16f;
                bool chin = dx * dx + dy * dy < 1f && y > 196;
                float ux = (x - 180) / 18f, uy = (y - 180) / 4f;
                if (chin || ux * ux + uy * uy < 1f) pts.Add(new PointF(x, y));
            }
            stubble = pts.ToArray();
        }

        // ---------------- helpers ----------------
        public static float Sin(double v) { return (float)Math.Sin(v); }
        public static float Cos(double v) { return (float)Math.Cos(v); }
        static int Cl(double v) { return v < 0 ? 0 : v > 255 ? 255 : (int)v; }
        public static Color A(int a, Color c) { return Color.FromArgb(Cl(a), c.R, c.G, c.B); }
        public static Color Mix(Color a, Color b, float k)
        {
            return Color.FromArgb(Cl(a.A + (b.A - a.A) * k), Cl(a.R + (b.R - a.R) * k), Cl(a.G + (b.G - a.G) * k), Cl(a.B + (b.B - a.B) * k));
        }
        public static Color Hsv(float h, float s, float v)
        {
            h = h % 360f; if (h < 0) h += 360f;
            float c = v * s, x = c * (1 - Math.Abs((h / 60f) % 2 - 1)), m = v - c, r = 0, g = 0, b = 0;
            if (h < 60) { r = c; g = x; }
            else if (h < 120) { r = x; g = c; }
            else if (h < 180) { g = c; b = x; }
            else if (h < 240) { g = x; b = c; }
            else if (h < 300) { r = x; b = c; }
            else { r = c; b = x; }
            return Color.FromArgb(Cl((r + m) * 255), Cl((g + m) * 255), Cl((b + m) * 255));
        }

        public static Pen P(Color c, float w)
        {
            Pen p = new Pen(c, w);
            p.StartCap = LineCap.Round; p.EndCap = LineCap.Round; p.LineJoin = LineJoin.Round;
            return p;
        }

        public static GraphicsPath RR(RectangleF r, float rad)
        {
            GraphicsPath p = new GraphicsPath();
            float d = Math.Min(rad * 2, Math.Min(r.Width, r.Height));
            if (d < 0.5f) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static GraphicsPath Heart(float cx, float cy, float s)
        {
            GraphicsPath p = new GraphicsPath();
            p.AddBezier(cx, cy + s * 0.9f, cx - s * 1.3f, cy + s * 0.1f, cx - s * 0.9f, cy - s * 1.0f, cx, cy - s * 0.45f);
            p.AddBezier(cx, cy - s * 0.45f, cx + s * 0.9f, cy - s * 1.0f, cx + s * 1.3f, cy + s * 0.1f, cx, cy + s * 0.9f);
            p.CloseFigure();
            return p;
        }

        static void Fill(Graphics g, Color c, GraphicsPath p) { using (SolidBrush b = new SolidBrush(c)) g.FillPath(b, p); }
        static void Stroke(Graphics g, Color c, float w, GraphicsPath p) { using (Pen pen = P(c, w)) g.DrawPath(pen, p); }
        static void FillEl(Graphics g, Color c, float x, float y, float w, float h) { using (SolidBrush b = new SolidBrush(c)) g.FillEllipse(b, x, y, w, h); }
        static void Line(Graphics g, Color c, float w, float x1, float y1, float x2, float y2) { using (Pen p = P(c, w)) g.DrawLine(p, x1, y1, x2, y2); }
        static void Bez(Graphics g, Color c, float w, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            using (Pen p = P(c, w)) g.DrawBezier(p, x1, y1, x2, y2, x3, y3, x4, y4);
        }
        static void Grad(Graphics g, GraphicsPath p, Color top, Color bottom)
        {
            RectangleF r = p.GetBounds();
            r.Inflate(1, 1);
            using (LinearGradientBrush b = new LinearGradientBrush(r, top, bottom, 90f)) g.FillPath(b, p);
        }
        static void GradEl(Graphics g, RectangleF r, Color top, Color bottom)
        {
            using (GraphicsPath p = new GraphicsPath()) { p.AddEllipse(r); Grad(g, p, top, bottom); }
        }

        public static void Glow(Graphics g, float cx, float cy, float rx, float ry, Color c, int alpha)
        {
            if (rx < 0.5f || ry < 0.5f || alpha <= 0) return;
            using (GraphicsPath p = new GraphicsPath())
            {
                p.AddEllipse(cx - rx, cy - ry, rx * 2, ry * 2);
                using (PathGradientBrush b = new PathGradientBrush(p))
                {
                    b.CenterColor = A(alpha, c);
                    b.SurroundColors = new Color[] { A(0, c) };
                    g.FillPath(b, p);
                }
            }
        }

        public static void GlowPath(Graphics g, GraphicsPath path, Color c, float k)
        {
            float[] w = { 18, 12, 7, 3.5f };
            int[] a = { 18, 34, 60, 110 };
            for (int i = 0; i < w.Length; i++)
                using (Pen p = P(A((int)(a[i] * k), c), w[i])) g.DrawPath(p, path);
        }

        public static GraphicsPath Star(float cx, float cy, float r, float ri, int n, float rot)
        {
            PointF[] pts = new PointF[n * 2];
            for (int i = 0; i < n * 2; i++)
            {
                double ang = (rot + i * 180.0 / n) * Math.PI / 180;
                float rr = (i % 2 == 0) ? r : ri;
                pts[i] = new PointF(cx + Cos(ang) * rr, cy + Sin(ang) * rr);
            }
            GraphicsPath p = new GraphicsPath();
            p.AddPolygon(pts);
            return p;
        }

        public static void Sparkle(Graphics g, float cx, float cy, float r, Color c)
        {
            if (r <= 0.3f) return;
            Glow(g, cx, cy, r * 2.2f, r * 2.2f, c, 130);
            using (GraphicsPath s = Star(cx, cy, r, r * 0.26f, 4, -90))
                Fill(g, Color.FromArgb(245, 255, 255, 255), s);
        }

        static void Gem(Graphics g, float cx, float cy, float r, Color c)
        {
            using (GraphicsPath p = new GraphicsPath())
            {
                p.AddEllipse(cx - r, cy - r * 0.85f, r * 2, r * 1.7f);
                using (PathGradientBrush b = new PathGradientBrush(p))
                {
                    b.CenterPoint = new PointF(cx - r * 0.25f, cy - r * 0.25f);
                    b.CenterColor = Mix(c, Color.White, 0.55f);
                    b.SurroundColors = new Color[] { Mix(c, Color.Black, 0.3f) };
                    g.FillPath(b, p);
                }
                Stroke(g, Mix(c, Color.Black, 0.6f), Math.Max(1f, r * 0.18f), p);
            }
            FillEl(g, A(210, Color.White), cx - r * 0.55f, cy - r * 0.55f, r * 0.55f, r * 0.42f);
        }

        public static bool IsAnimated(Pose p)
        {
            return IsImm(p.Hat) || IsImm(p.Glasses) || IsImm(p.Gloves) || IsImm(p.Face) || IsImm(p.Theme);
        }
        static bool IsImm(string id)
        {
            Item it = Items.Get(id);
            return it != null && it.Rarity == Rarity.Immortal;
        }

        public static GraphicsState Fit(Graphics g, RectangleF src, RectangleF dst)
        {
            GraphicsState st = g.Save();
            float s = Math.Min(dst.Width / src.Width, dst.Height / src.Height);
            g.TranslateTransform(dst.X + (dst.Width - src.Width * s) / 2, dst.Y + (dst.Height - src.Height * s) / 2);
            g.ScaleTransform(s, s);
            g.TranslateTransform(-src.X, -src.Y);
            return st;
        }

        static void ScaleAround(Graphics g, float cx, float cy, float s)
        {
            g.TranslateTransform(cx, cy);
            g.ScaleTransform(s, s);
            g.TranslateTransform(-cx, -cy);
        }

        // ---------------- character ----------------
        public static void DrawCharacter(Graphics g, Pose p)
        {
            UseCharacter(p.Character);
            GraphicsState root = g.Save();
            DayBack(g, p);
            SetBonusBack(g, p);
            ScaleAround(g, 180, 356, SceneScale);
            // weightless float while the astronaut set is worn
            if (p.ActiveSet == "set_astro") g.TranslateTransform(0, Sin(p.T * 1.5) * 5 - 3);
            DrawBody(g, p);
            bool hit = p.LeftDown || p.RightDown || (p.OnMouse && p.MouseDown);
            GraphicsState st = g.Save();
            if (hit && p.Mood != Mood.Sleep) g.TranslateTransform(0, 2.5f);
            if (p.Mood == Mood.Sleep)
            {
                // head droops to one side while asleep
                g.TranslateTransform(180, 214);
                g.RotateTransform(7);
                g.TranslateTransform(-180, -210);
            }
            ScaleAround(g, 180, 216, HeadScale);
            DrawHead(g, p);
            g.Restore(st);
            DrawDesk(g, p);
            DayFront(g, p);
            DrawPet(g, p);
            DrawArms(g, p);
            SpeedLines(g, p);
            g.Restore(root);
            SetBonusFront(g, p);
        }

        public static void DrawCharacterIn(Graphics g, RectangleF dst, Pose p)
        {
            GraphicsState st = Fit(g, new RectangleF(0, 0, 360, 360), dst);
            DrawCharacter(g, p);
            g.Restore(st);
        }

        public static void DrawBust(Graphics g, RectangleF src, RectangleF dst, Pose p)
        {
            UseCharacter(p.Character);
            GraphicsState st = Fit(g, src, dst);
            DrawBody(g, p);
            ScaleAround(g, 180, 216, HeadScale);
            DrawHead(g, p);
            g.Restore(st);
        }

        static Color Shirt = Color.FromArgb(44, 46, 52);
        static Color ShirtHi = Color.FromArgb(78, 80, 90);

        static void DrawBody(Graphics g, Pose p)
        {
            // oversized black t-shirt with dropped shoulders
            using (GraphicsPath tp = new GraphicsPath())
            {
                tp.AddBezier(50, 336, 50, 290, 70, 252, 140, 238);
                tp.AddLine(140, 238, 220, 238);
                tp.AddBezier(220, 238, 290, 252, 310, 290, 310, 336);
                tp.CloseFigure();
                Grad(g, tp, ShirtHi, Shirt);
                GraphicsState st = g.Save();
                g.SetClip(tp, CombineMode.Intersect);
                Glow(g, 92, 262, 34, 20, Color.White, 34);
                Glow(g, 268, 262, 34, 20, Color.White, 34);
                using (Pen fold = P(A(150, JacketInk), 2f))
                using (Pen hl = P(A(34, Color.White), 2f))
                {
                    g.DrawBezier(fold, 96, 252, 102, 270, 104, 290, 100, 320);
                    g.DrawBezier(fold, 264, 252, 258, 270, 256, 290, 260, 320);
                    g.DrawBezier(fold, 132, 268, 140, 280, 150, 284, 162, 286);
                    g.DrawBezier(hl, 132, 264, 140, 276, 150, 280, 162, 282);
                    g.DrawBezier(fold, 228, 270, 220, 282, 210, 286, 198, 288);
                    g.DrawBezier(hl, 228, 266, 220, 278, 210, 282, 198, 284);
                }
                g.Restore(st);
                Stroke(g, JacketInk, 3f, tp);
            }
            // neck
            using (GraphicsPath np = new GraphicsPath())
            {
                np.AddLine(162, 184, 160, 238);
                np.AddBezier(160, 238, 168, 252, 192, 252, 200, 238);
                np.AddLine(200, 238, 198, 184);
                np.CloseFigure();
                Fill(g, Skin, np);
                GraphicsState st = g.Save();
                g.SetClip(np, CombineMode.Intersect);
                Glow(g, 180, 206, 36, 20, SkinDeep, 170);
                Glow(g, 162, 230, 8, 20, SkinShade, 120);
                Glow(g, 198, 230, 8, 20, SkinShade, 120);
                g.Restore(st);
                Bez(g, A(170, SkinDeep), 1.8f, 176, 222, 178, 227, 182, 227, 184, 222);
                Line(g, Ink, 2.2f, 162, 196, 160, 238);
                Line(g, Ink, 2.2f, 198, 196, 200, 238);
            }
            // crew neck rib
            Bez(g, Mix(Shirt, Color.Black, 0.4f), 7f, 152, 236, 162, 258, 198, 258, 208, 236);
            Bez(g, A(60, Color.White), 1.4f, 156, 238, 166, 255, 194, 255, 204, 238);
            if (p.Character == Characters.Vlad) DrawChain(g);
        }

        static GraphicsPath FacePath()
        {
            GraphicsPath f = new GraphicsPath();
            f.AddBezier(118, 100, 116, 140, 120, 168, 133, 187);
            f.AddBezier(133, 187, 146, 204, 162, 214, 180, 215);
            f.AddBezier(180, 215, 198, 214, 214, 204, 227, 187);
            f.AddBezier(227, 187, 240, 168, 244, 140, 242, 100);
            f.AddBezier(242, 100, 240, 58, 120, 58, 118, 100);
            f.CloseFigure();
            return f;
        }

        public static void DrawHead(Graphics g, Pose p)
        {
            UseCharacter(p.Character);
            if (p.Character == Characters.Vlad) { DrawHeadVlad(g, p); return; }
            float t = p.T;
            if (p.Hat == "hat_halo") HaloBack(g, t);
            using (GraphicsPath bh = new GraphicsPath())
            {
                bh.AddEllipse(110, 50, 140, 104);
                using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(110, 49, 140, 106), HairDark, A(0, HairDark), 90f)) g.FillPath(b, bh);
            }
            DrawEar(g, 111, 150, false);
            DrawEar(g, 249, 150, true);
            using (GraphicsPath face = FacePath())
            {
                Fill(g, Skin, face);
                GraphicsState st = g.Save();
                g.SetClip(face, CombineMode.Intersect);
                Glow(g, 114, 156, 20, 60, SkinShade, 190);
                Glow(g, 246, 156, 20, 60, SkinShade, 190);
                Glow(g, 180, 104, 70, 14, SkinDeep, 70);
                Glow(g, 150, 168, 18, 12, SkinShade, 110);
                Glow(g, 210, 168, 18, 12, SkinShade, 110);
                // five o'clock shadow
                Color stub = Color.FromArgb(120, 96, 80);
                Glow(g, 180, 204, 50, 20, stub, 80);
                Glow(g, 180, 180, 22, 6, stub, 75);
                Glow(g, 138, 192, 14, 18, stub, 45);
                Glow(g, 222, 192, 14, 18, stub, 45);
                using (SolidBrush sb = new SolidBrush(Color.FromArgb(80, 96, 70, 52)))
                    foreach (PointF d in stubble) g.FillEllipse(sb, d.X, d.Y, 1.6f, 1.6f);
                FaceUnder(g, p);
                g.Restore(st);
                Stroke(g, Ink, 2.6f, face);
            }
            DrawEyes(g, p);
            DrawNose(g, p);
            DrawMouth(g, p);
            FaceOver(g, p);
            DrawFrontHair(g);
            DrawBrows(g, p);
            DrawGlasses(g, p.Glasses, t);
            DrawHat(g, p.Hat, t);
            MoodOverlay(g, p);
        }

        static void DrawEar(Graphics g, float cx, float cy, bool right)
        {
            float d = right ? -1 : 1;
            using (GraphicsPath e = new GraphicsPath())
            {
                e.AddBezier(cx + 8 * d, cy - 16, cx - 8 * d, cy - 26, cx - 16 * d, cy - 4, cx - 10 * d, cy + 10);
                e.AddBezier(cx - 10 * d, cy + 10, cx - 6 * d, cy + 20, cx + 4 * d, cy + 22, cx + 8 * d, cy + 14);
                e.CloseFigure();
                Fill(g, Skin, e);
                Stroke(g, Ink, 2.4f, e);
            }
            Glow(g, cx - 2 * d, cy, 6, 12, Color.FromArgb(240, 150, 140), 100);
            Bez(g, SkinDeep, 2f, cx + 4 * d, cy - 12, cx - 8 * d, cy - 16, cx - 10 * d, cy + 2, cx - 2 * d, cy + 8);
            Bez(g, A(160, SkinDeep), 1.6f, cx + 2 * d, cy - 4, cx - 3 * d, cy - 3, cx - 3 * d, cy + 4, cx + 1 * d, cy + 6);
        }

        // short crew cut: spiky top, faded sides
        static void DrawFrontHair(Graphics g)
        {
            foreach (int k in new int[] { 0, 1 })
            {
                float s = k == 0 ? 1 : -1;
                using (GraphicsPath side = new GraphicsPath())
                {
                    side.AddPolygon(new PointF[] { new PointF(180 - 66 * s, 92), new PointF(180 - 54 * s, 92), new PointF(180 - 56 * s, 134), new PointF(180 - 64 * s, 142) });
                    using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(100, 90, 160, 54), A(200, HairDark), A(30, HairDark), 90f)) g.FillPath(b, side);
                }
            }
            PointF[] top = {
                new PointF(116, 120), new PointF(113, 94), new PointF(118, 76), new PointF(126, 70), new PointF(131, 60),
                new PointF(140, 62), new PointF(147, 52), new PointF(157, 56), new PointF(167, 47), new PointF(177, 53),
                new PointF(188, 46), new PointF(197, 52), new PointF(208, 48), new PointF(217, 55), new PointF(226, 55),
                new PointF(233, 64), new PointF(242, 74), new PointF(247, 94), new PointF(244, 120),
                new PointF(238, 106), new PointF(230, 108), new PointF(224, 101), new PointF(212, 105), new PointF(204, 98),
                new PointF(192, 103), new PointF(184, 97), new PointF(172, 102), new PointF(164, 97), new PointF(152, 103),
                new PointF(144, 99), new PointF(134, 105), new PointF(126, 102), new PointF(120, 110) };
            using (GraphicsPath hp = new GraphicsPath())
            {
                hp.AddLines(top);
                hp.CloseFigure();
                Grad(g, hp, HairLight, HairC);
                GraphicsState st = g.Save();
                g.SetClip(hp, CombineMode.Intersect);
                Glow(g, 180, 100, 70, 10, HairDark, 120);
                using (Pen s = P(A(150, HairDark), 1.6f))
                {
                    g.DrawLine(s, 140, 66, 136, 92);
                    g.DrawLine(s, 160, 60, 158, 88);
                    g.DrawLine(s, 182, 56, 182, 86);
                    g.DrawLine(s, 204, 56, 206, 88);
                    g.DrawLine(s, 224, 64, 228, 92);
                }
                using (Pen hl = P(A(120, Color.FromArgb(230, 196, 150)), 3f))
                {
                    g.DrawLine(hl, 150, 58, 148, 74);
                    g.DrawLine(hl, 172, 50, 172, 68);
                    g.DrawLine(hl, 194, 50, 195, 68);
                }
                g.Restore(st);
                using (Pen o = new Pen(HairDark, 2.4f)) { o.LineJoin = LineJoin.Round; g.DrawPath(o, hp); }
            }
        }

        // ---------------- arms & hands ----------------
        static void DrawArms(Graphics g, Pose p)
        {
            // He faces the viewer, so his right hand (and the mouse) is on the viewer's left
            // and the watch sits on his left wrist, on the viewer's right.
            PointF ls = new PointF(94, 266), rs = new PointF(266, 266);
            PointF lh;
            bool lDown;
            if (p.OnMouse) { lh = new PointF(MouseX(p), (p.MouseDown ? 304 : 292) + MouseYOff(p)); lDown = p.MouseDown; }
            else { lh = p.LeftDown ? new PointF(122, 298) : new PointF(112, 246); lDown = p.LeftDown; }
            PointF rh = p.RightDown ? new PointF(238, 298) : new PointF(248, 246);
            Arm(g, ls, lh, p, lDown, false);
            Arm(g, rs, rh, p, p.RightDown, p.Character != Characters.Vlad);
        }

        // Cursor movement is mirrored: his "right" and "forward" point at the viewer's left and down.
        static float MouseX(Pose p) { return 34 - p.MouseDX; }
        static float MouseYOff(Pose p) { return -p.MouseDY; }

        static void Arm(Graphics g, PointF s, PointF h, Pose p, bool down, bool watch)
        {
            double dx = s.X - h.X, dy = s.Y - h.Y, len = Math.Sqrt(dx * dx + dy * dy);
            if (len < 1) len = 1;
            float ux = (float)(dx / len), uy = (float)(dy / len);
            PointF wrist = new PointF(h.X + ux * 12, h.Y + uy * 12);
            PointF sh = new PointF(s.X + ux * 8, s.Y + uy * 8);
            float sl = Math.Min((float)len * 0.55f, 26);
            PointF hem = new PointF(sh.X - ux * sl, sh.Y - uy * sl);
            using (Pen o = P(Ink, 24)) g.DrawLine(o, sh, wrist);
            using (Pen f = P(Skin, 19.5f)) g.DrawLine(f, sh, wrist);
            using (Pen sd = P(A(110, SkinDeep), 5)) g.DrawLine(sd, sh.X + uy * 6, sh.Y - ux * 6, wrist.X + uy * 6, wrist.Y - ux * 6);
            using (Pen o = P(JacketInk, 38)) g.DrawLine(o, sh, hem);
            using (Pen f = P(Shirt, 33)) g.DrawLine(f, sh, hem);
            using (Pen hl = P(A(40, Color.White), 5)) g.DrawLine(hl, sh.X, sh.Y - 9, hem.X, hem.Y - 9);
            double angR = Math.Atan2(dy, dx) - Math.PI / 2;
            float sign = Math.Cos(angR) * (180 - h.X) >= 0 ? 1 : -1;
            DrawHand(g, h.X, h.Y, (float)(angR * 180 / Math.PI), p.Gloves, sign, down, p.T, watch);
        }

        public static void DrawGlovePreview(Graphics g, string glove, RectangleF dst, float t)
        {
            GraphicsState st = Fit(g, new RectangleF(-36, -36, 72, 78), dst);
            using (Pen o = P(Ink, 24)) g.DrawLine(o, 0, 70, 0, 12);
            using (Pen f = P(Skin, 19.5f)) g.DrawLine(f, 0, 70, 0, 12);
            DrawHand(g, 0, 0, 0, glove, 1, false, t, false);
            g.Restore(st);
        }

        public static void DrawHand(Graphics g, float x, float y, float ang, string glove, float thumbSign, bool down, float t, bool watch)
        {
            GraphicsState st = g.Save();
            g.TranslateTransform(x, y);
            g.RotateTransform(ang);
            if (down) g.ScaleTransform(1.06f, 0.86f);
            float tx = 14 * thumbSign;
            if (!DrawGlove(g, glove, tx, t, thumbSign)) PlainHand(g, tx);
            if (watch)
            {
                using (GraphicsPath band = RR(new RectangleF(-11, 17, 22, 9), 3)) { Fill(g, Color.FromArgb(24, 24, 28), band); Stroke(g, Color.FromArgb(8, 8, 10), 1.4f, band); }
                using (GraphicsPath face = RR(new RectangleF(-7, 15, 14, 13), 3.5f))
                {
                    Fill(g, Color.FromArgb(34, 36, 42), face);
                    Stroke(g, Color.FromArgb(8, 8, 10), 1.6f, face);
                }
                using (GraphicsPath scr = RR(new RectangleF(-5, 17, 10, 9), 2.5f)) Fill(g, Color.FromArgb(18, 22, 34), scr);
                Line(g, Color.FromArgb(120, 220, 255), 1.2f, -2.5f, 21.5f, 2.5f, 21.5f);
                FillEl(g, A(90, Color.White), -4, 17.5f, 3, 2);
            }
            g.Restore(st);
        }

        static GraphicsPath PalmPath() { GraphicsPath p = new GraphicsPath(); p.AddEllipse(-17, -16, 34, 31); return p; }
        static GraphicsPath ThumbPath(float tx) { GraphicsPath p = new GraphicsPath(); p.AddEllipse(tx - 6, -3, 12, 15); return p; }

        static void HandShape(Graphics g, float tx, Brush b, Color outline, float ow)
        {
            using (GraphicsPath th = ThumbPath(tx)) { g.FillPath(b, th); Stroke(g, outline, ow, th); }
            using (GraphicsPath pp = PalmPath()) { g.FillPath(b, pp); Stroke(g, outline, ow, pp); }
        }

        static void FingerLines(Graphics g, Color c, float w)
        {
            using (Pen p = P(c, w))
            {
                g.DrawLine(p, -8, -15, -7, -8);
                g.DrawLine(p, 0, -16.5f, 0, -9);
                g.DrawLine(p, 8, -15, 7, -8);
            }
        }

        static void Cuff(Graphics g, Color c, Color outline, float h)
        {
            using (GraphicsPath cp = RR(new RectangleF(-16, 7, 32, h), 5)) { Fill(g, c, cp); Stroke(g, outline, 2f, cp); }
        }

        static void PlainHand(Graphics g, float tx)
        {
            using (LinearGradientBrush b = VBrush(Color.FromArgb(255, 234, 214), Skin)) HandShape(g, tx, b, Ink, 2.4f);
            Glow(g, 0, 4, 9, 6, Color.FromArgb(255, 170, 160), 70);
            FingerLines(g, SkinDeep, 1.7f);
            FillEl(g, A(150, Color.White), -9, -11, 8, 5);
        }

        static LinearGradientBrush VBrush(Color a, Color b)
        {
            return new LinearGradientBrush(new RectangleF(-24, -20, 48, 40), a, b, 90f);
        }

        // ---------------- previews / misc ----------------
        public static void DrawItemPreview(Graphics g, Item it, RectangleF dst, float t)
        {
            // previews show the item on the currently selected character
            string who = Game.S != null ? Game.S.Character : Characters.Yura;
            UseCharacter(who);
            switch (it.Slot)
            {
                case Slot.Gloves: DrawGlovePreview(g, it.Id, dst, t); return;
                case Slot.Theme: DrawThemePreview(g, it.Id, dst, t); return;
                case Slot.Pet: DrawPetPreview(g, it.Id, dst, t); return;
            }
            Pose p = new Pose();
            p.T = t;
            p.Character = who;
            RectangleF src;
            if (it.Slot == Slot.Hat) { p.Hat = it.Id; src = new RectangleF(92, -6, 176, 164); }
            else if (it.Slot == Slot.Glasses) { p.Glasses = it.Id; src = new RectangleF(108, 92, 144, 108); }
            else { p.Face = it.Id; src = new RectangleF(100, 88, 160, 136); }
            GraphicsState st = Fit(g, src, dst);
            DrawHead(g, p);
            g.Restore(st);
        }

        public static void DrawLootBox(Graphics g, float cx, float cy, float s, float t, bool active)
        {
            DrawLootBox(g, cx, cy, s, t, active, false);
        }

        // Regular box: purple with a gold ribbon. Golden box: gold with a purple ribbon, always shining.
        public static void DrawLootBox(Graphics g, float cx, float cy, float s, float t, bool active, bool gold)
        {
            Color bodyTop = gold ? Color.FromArgb(255, 222, 110) : Color.FromArgb(139, 92, 246);
            Color bodyBot = gold ? Color.FromArgb(200, 130, 18) : Color.FromArgb(76, 29, 149);
            Color lidTop = gold ? Color.FromArgb(255, 240, 160) : Color.FromArgb(180, 150, 255);
            Color lidBot = gold ? Color.FromArgb(222, 156, 30) : Color.FromArgb(112, 50, 220);
            Color ink = gold ? Color.FromArgb(110, 66, 8) : Color.FromArgb(34, 14, 66);
            Color ribA = gold ? Color.FromArgb(190, 120, 255) : Color.FromArgb(255, 230, 120);
            Color ribB = gold ? Color.FromArgb(110, 40, 200) : Color.FromArgb(214, 150, 30);
            Color ribInk = gold ? Color.FromArgb(60, 16, 110) : Color.FromArgb(130, 84, 16);
            Color glow = gold ? Color.FromArgb(255, 200, 80) : Color.FromArgb(170, 120, 255);
            if (active || gold)
            {
                float pulse = 0.7f + 0.3f * Sin(t * 3);
                Glow(g, cx, cy, s * 0.9f, s * 0.8f, glow, (int)((gold ? 190 : 160) * pulse));
            }
            float bw = s * 0.78f, bh = s * 0.5f, bx = cx - bw / 2, by = cy - bh * 0.2f;
            float lw = Math.Max(1f, s * 0.022f);
            using (GraphicsPath body = RR(new RectangleF(bx, by, bw, bh), s * 0.05f))
            {
                Grad(g, body, bodyTop, bodyBot);
                Stroke(g, ink, lw, body);
            }
            RectangleF lidR = new RectangleF(cx - bw * 0.56f, by - s * 0.17f, bw * 1.12f, s * 0.19f);
            using (GraphicsPath lid = RR(lidR, s * 0.05f))
            {
                Grad(g, lid, lidTop, lidBot);
                Stroke(g, ink, lw, lid);
            }
            RectangleF rib = new RectangleF(cx - s * 0.07f, lidR.Y, s * 0.14f, by + bh - lidR.Y);
            using (GraphicsPath rp = RR(rib, s * 0.01f))
            {
                using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(rib.X - 1, rib.Y, rib.Width + 2, rib.Height), ribA, ribB, 0f)) g.FillPath(b, rp);
                Stroke(g, ribInk, lw * 0.8f, rp);
            }
            using (GraphicsPath l1 = new GraphicsPath())
            using (GraphicsPath l2 = new GraphicsPath())
            {
                l1.AddEllipse(cx - s * 0.3f, lidR.Y - s * 0.2f, s * 0.3f, s * 0.21f);
                l2.AddEllipse(cx, lidR.Y - s * 0.2f, s * 0.3f, s * 0.21f);
                foreach (GraphicsPath l in new GraphicsPath[] { l1, l2 })
                {
                    Grad(g, l, Mix(ribA, Color.White, 0.2f), ribB);
                    Stroke(g, ribInk, lw * 0.8f, l);
                }
            }
            FillEl(g, Mix(ribA, Color.White, 0.1f), cx - s * 0.075f, lidR.Y - s * 0.12f, s * 0.15f, s * 0.15f);
            using (Pen o = P(ribInk, lw * 0.8f)) g.DrawEllipse(o, cx - s * 0.075f, lidR.Y - s * 0.12f, s * 0.15f, s * 0.15f);
            using (SolidBrush hl = new SolidBrush(Color.FromArgb(gold ? 110 : 70, 255, 255, 255))) g.FillRectangle(hl, bx + bw * 0.08f, by + bh * 0.12f, bw * 0.08f, bh * 0.7f);
            if (gold)
                using (GraphicsPath st = Star(cx - bw * 0.3f, by + bh * 0.55f, s * 0.07f, s * 0.03f, 5, -90))
                    Fill(g, Color.FromArgb(255, 255, 250, 220), st);
            if (active || gold)
                for (int k = 0; k < (gold ? 5 : 3); k++)
                {
                    float ph = (t * 0.7f + k / (gold ? 5f : 3f)) % 1f;
                    double a = k * 2.1 + t * 0.4;
                    Sparkle(g, cx + Cos(a) * s * 0.55f, cy + Sin(a) * s * 0.4f - ph * s * 0.1f, s * 0.06f * Sin(ph * Math.PI), gold ? Color.FromArgb(255, 230, 140) : Color.FromArgb(220, 190, 255));
                }
        }
    }
}

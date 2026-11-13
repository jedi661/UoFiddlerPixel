/***************************************************************************
 *
 * $Author: Nikodemus
 *
 * "THE WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a Wine in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.UoStudio
{
    /// <summary>
    /// Button icons for the UO Studio tools, drawn at runtime (no resource files needed).
    /// </summary>
    public static class StudioIcons
    {
        private const int Size = 48;

        private static Bitmap Create(Action<Graphics> draw)
        {
            var bmp = new Bitmap(Size, Size);
            using Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            using (var bg = new LinearGradientBrush(new Rectangle(0, 0, Size, Size), Color.FromArgb(58, 64, 84), Color.FromArgb(28, 30, 40), 90f))
            using (GraphicsPath p = Rounded(new Rectangle(1, 1, Size - 3, Size - 3), 8))
            {
                g.FillPath(bg, p);
                using var border = new Pen(Color.FromArgb(120, 140, 170), 1.5f);
                g.DrawPath(border, p);
            }
            draw(g);
            return bmp;
        }

        private static GraphicsPath Rounded(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = radius * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        /// <summary>Pixel grid with a diamond (pixel art / isometric converter).</summary>
        public static Bitmap PixelArt() => Create(g =>
        {
            g.SmoothingMode = SmoothingMode.None;
            Color[] cols = { Color.FromArgb(240, 200, 80), Color.FromArgb(220, 120, 50), Color.FromArgb(120, 200, 90), Color.FromArgb(80, 150, 230) };
            for (int y = 0; y < 6; y++)
            {
                for (int x = 0; x < 6; x++)
                {
                    int dx = Math.Abs(x - 2) + Math.Abs(y - 2);
                    if (dx > 3 || x > 5 || y > 5)
                    {
                        continue;
                    }
                    using var b = new SolidBrush(cols[(x + y) % cols.Length]);
                    g.FillRectangle(b, 7 + (x * 6), 7 + (y * 6), 5, 5);
                }
            }
            using var pen = new Pen(Color.White, 2f);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawPolygon(pen, new[] { new Point(34, 26), new Point(43, 34), new Point(34, 42), new Point(25, 34) });
        });

        /// <summary>Film strip with a running figure and a mirror arrow (animation studio).</summary>
        public static Bitmap Animation() => Create(g =>
        {
            using (var strip = new SolidBrush(Color.FromArgb(30, 30, 30)))
            {
                g.FillRectangle(strip, 6, 8, 36, 22);
            }
            using (var hole = new SolidBrush(Color.FromArgb(200, 200, 200)))
            {
                for (int i = 0; i < 6; i++)
                {
                    g.FillRectangle(hole, 8 + (i * 6), 9, 3, 3);
                    g.FillRectangle(hole, 8 + (i * 6), 26, 3, 3);
                }
            }
            using (var fig = new Pen(Color.FromArgb(255, 210, 90), 2f))
            {
                g.DrawEllipse(fig, 21, 13, 5, 5);
                g.DrawLine(fig, 23, 18, 23, 23);
                g.DrawLine(fig, 23, 23, 19, 26);
                g.DrawLine(fig, 23, 23, 27, 26);
                g.DrawLine(fig, 19, 20, 28, 19);
            }
            using var arrow = new Pen(Color.FromArgb(120, 200, 255), 2.5f) { StartCap = LineCap.ArrowAnchor, EndCap = LineCap.ArrowAnchor };
            g.DrawLine(arrow, 10, 38, 38, 38);
        });

        /// <summary>Shirt / armour on a paperdoll (equipment generator).</summary>
        public static Bitmap Equipment() => Create(g =>
        {
            var shirt = new[]
            {
                new Point(16, 9), new Point(21, 12), new Point(27, 12), new Point(32, 9), new Point(40, 16),
                new Point(35, 21), new Point(33, 19), new Point(33, 40), new Point(15, 40), new Point(15, 19),
                new Point(13, 21), new Point(8, 16)
            };
            using (var b = new LinearGradientBrush(new Rectangle(8, 9, 32, 31), Color.FromArgb(200, 210, 225), Color.FromArgb(110, 120, 140), 90f))
            {
                g.FillPolygon(b, shirt);
            }
            using (var pen = new Pen(Color.FromArgb(40, 40, 50), 1.5f))
            {
                g.DrawPolygon(pen, shirt);
                g.DrawLine(pen, 24, 14, 24, 39);
            }
            using var gold = new SolidBrush(Color.FromArgb(240, 200, 80));
            g.FillEllipse(gold, 21, 22, 6, 6);
        });

        /// <summary>Waveform with a speaker (sound studio).</summary>
        public static Bitmap Sound() => Create(g =>
        {
            using (var b = new SolidBrush(Color.FromArgb(230, 230, 240)))
            {
                g.FillPolygon(b, new[] { new Point(7, 19), new Point(13, 19), new Point(20, 12), new Point(20, 36), new Point(13, 29), new Point(7, 29) });
            }
            using var wave = new Pen(Color.FromArgb(90, 220, 140), 2f);
            int[] h = { 4, 10, 16, 8, 13, 5, 9 };
            for (int i = 0; i < h.Length; i++)
            {
                int x = 24 + (i * 3);
                g.DrawLine(wave, x, 24 - (h[i] / 2), x, 24 + (h[i] / 2));
            }
        });

        /// <summary>Magic sparkle / flame (spell and effect editor).</summary>
        public static Bitmap Effect() => Create(g =>
        {
            using (var glow = new GraphicsPath())
            {
                glow.AddEllipse(8, 8, 32, 32);
                using var pb = new PathGradientBrush(glow)
                {
                    CenterColor = Color.FromArgb(230, 255, 230, 140),
                    SurroundColors = new[] { Color.FromArgb(0, 255, 120, 40) }
                };
                g.FillPath(pb, glow);
            }
            var star = new PointF[10];
            for (int i = 0; i < 10; i++)
            {
                double a = (Math.PI / 5 * i) - (Math.PI / 2);
                float r = i % 2 == 0 ? 15f : 6f;
                star[i] = new PointF(24 + (float)(Math.Cos(a) * r), 24 + (float)(Math.Sin(a) * r));
            }
            using (var b = new SolidBrush(Color.FromArgb(255, 250, 220)))
            {
                g.FillPolygon(b, star);
            }
            using var small = new SolidBrush(Color.FromArgb(150, 210, 255));
            g.FillEllipse(small, 37, 9, 4, 4);
            g.FillEllipse(small, 9, 36, 3, 3);
            g.FillEllipse(small, 38, 37, 3, 3);
        });
    }
}

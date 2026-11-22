/***************************************************************************
 *
 *  * Dragon Map Studio – drawn tool icons for the tool palettes (no client files needed)
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls
{
    /// <summary>Simple vector pictures (24×24) for the tools: brush, bucket, pipette, select, coast …</summary>
    public static class ToolIcons
    {
        public const int Size = 24;

        private static readonly Dictionary<string, Bitmap> Cache = new Dictionary<string, Bitmap>();

        private static readonly Color Ink = Color.FromArgb(30, 41, 59);
        private static readonly Color Sand = Color.FromArgb(234, 196, 120);
        private static readonly Color Sea = Color.FromArgb(37, 99, 235);
        private static readonly Color Grass = Color.FromArgb(22, 128, 61);
        private static readonly Color Rock = Color.FromArgb(120, 113, 108);
        private static readonly Color Cave = Color.FromArgb(180, 83, 9);
        private static readonly Color Red = Color.FromArgb(220, 38, 38);

        /// <summary>Picture of a tool by key; unknown keys get the first letter in a circle.</summary>
        public static Bitmap Get(string key)
        {
            if (Cache.TryGetValue(key, out Bitmap b))
            {
                return b;
            }
            b = new Bitmap(Size, Size);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                Draw(g, key);
            }
            Cache[key] = b;
            return b;
        }

        private static void Draw(Graphics g, string key)
        {
            using var ink = new Pen(Ink, 1.6f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
            using var inkBrush = new SolidBrush(Ink);
            switch (key)
            {
                case "brush":
                    Brush(g, Color.FromArgb(124, 58, 237));
                    break;
                case "eraser":
                    {
                        var pts = new[] { new PointF(4, 16), new PointF(13, 6), new PointF(20, 13), new PointF(12, 21), new PointF(7, 21) };
                        using var pink = new SolidBrush(Color.FromArgb(244, 114, 182));
                        g.FillPolygon(pink, pts);
                        g.DrawPolygon(ink, pts);
                        g.DrawLine(ink, 8.5f, 11, 15.5f, 18);
                        g.DrawLine(ink, 12, 21, 21, 21);
                        break;
                    }
                case "spray":
                    {
                        using var can = new SolidBrush(Color.FromArgb(14, 165, 233));
                        g.FillRectangle(can, 4, 10, 8, 12);
                        g.DrawRectangle(ink, 4, 10, 8, 12);
                        g.FillRectangle(inkBrush, 6, 6, 4, 4);
                        var rnd = new Random(3);
                        for (int i = 0; i < 14; i++)
                        {
                            float x = 14 + (float)rnd.NextDouble() * 8, y = 2 + (float)rnd.NextDouble() * 10;
                            g.FillEllipse(can, x, y, 1.8f, 1.8f);
                        }
                        break;
                    }
                case "line":
                    g.DrawLine(new Pen(Ink, 2.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round }, 4, 20, 20, 4);
                    break;
                case "rect":
                    g.DrawRectangle(new Pen(Ink, 2f), 4, 6, 16, 12);
                    break;
                case "rectfill":
                    using (var fill = new SolidBrush(Color.FromArgb(124, 58, 237)))
                    {
                        g.FillRectangle(fill, 4, 6, 16, 12);
                    }
                    g.DrawRectangle(ink, 4, 6, 16, 12);
                    break;
                case "ellipse":
                    g.DrawEllipse(new Pen(Ink, 2f), 3, 5, 18, 14);
                    break;
                case "ellipsefill":
                    using (var fill = new SolidBrush(Color.FromArgb(124, 58, 237)))
                    {
                        g.FillEllipse(fill, 3, 5, 18, 14);
                    }
                    g.DrawEllipse(ink, 3, 5, 18, 14);
                    break;
                case "fill":
                    {
                        // tilted bucket with a drop
                        var bucket = new[] { new PointF(5, 10), new PointF(13, 3), new PointF(20, 10), new PointF(12, 18) };
                        using var body = new SolidBrush(Color.FromArgb(203, 213, 225));
                        g.FillPolygon(body, bucket);
                        g.DrawPolygon(ink, bucket);
                        using var paint = new SolidBrush(Color.FromArgb(124, 58, 237));
                        g.FillPolygon(paint, new[] { new PointF(5, 10), new PointF(20, 10), new PointF(12, 18) });
                        g.FillEllipse(paint, 17, 15, 5, 6);
                        g.FillPolygon(paint, new[] { new PointF(19.5f, 11), new PointF(17.2f, 16.5f), new PointF(21.8f, 16.5f) });
                        break;
                    }
                case "picker":
                    {
                        using var thick = new Pen(Ink, 3f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                        g.DrawLine(thick, 15, 9, 6, 18);
                        g.FillEllipse(inkBrush, 14, 3, 7, 7);
                        using var drop = new SolidBrush(Color.FromArgb(124, 58, 237));
                        g.FillEllipse(drop, 3, 18, 4, 4);
                        break;
                    }
                case "raise":
                    Mountain(g, Grass);
                    Arrow(g, true, Color.FromArgb(22, 163, 74));
                    break;
                case "lower":
                    Mountain(g, Grass);
                    Arrow(g, false, Red);
                    break;
                case "select":
                    using (var dash = new Pen(Ink, 1.6f) { DashStyle = DashStyle.Dash })
                    {
                        g.DrawRectangle(dash, 4, 5, 16, 14);
                    }
                    break;
                case "pan":
                    {
                        using var p = new Pen(Ink, 1.8f) { StartCap = LineCap.ArrowAnchor, EndCap = LineCap.ArrowAnchor };
                        g.DrawLine(p, 12, 3, 12, 21);
                        g.DrawLine(p, 3, 12, 21, 12);
                        break;
                    }
                case "cave":
                case "caveentrance":
                    {
                        Mountain(g, Rock);
                        using var dark = new SolidBrush(Color.FromArgb(20, 20, 20));
                        g.FillPie(dark, 8, 12, 8, 14, 180, 180);
                        g.FillRectangle(dark, 8, 19, 8, 3);
                        if (key == "caveentrance")
                        {
                            using var mouth = new Pen(Cave, 1.6f);
                            g.DrawArc(mouth, 7, 11, 10, 16, 180, 180);
                        }
                        break;
                    }
                case "cavestamp":
                    {
                        Mountain(g, Rock);
                        using var dark = new SolidBrush(Color.FromArgb(20, 20, 20));
                        g.FillPie(dark, 8, 12, 8, 14, 180, 180);
                        g.FillRectangle(dark, 8, 19, 8, 3);
                        Stamp(g, Cave);
                        break;
                    }
                case "pattern":
                    {
                        using var a = new SolidBrush(Color.FromArgb(234, 179, 8));
                        using var b2 = new SolidBrush(Color.FromArgb(161, 98, 7));
                        for (int y = 0; y < 3; y++)
                        {
                            for (int x = 0; x < 3; x++)
                            {
                                g.FillRectangle((x + y) % 2 == 0 ? a : b2, 2 + x * 5, 12 + y * 3.5f, 5, 3.5f);
                            }
                        }
                        Brush(g, Color.FromArgb(126, 34, 206));
                        break;
                    }
                case "place":
                    {
                        // little crate
                        var top = new[] { new PointF(12, 3), new PointF(21, 8), new PointF(12, 13), new PointF(3, 8) };
                        using var lightB = new SolidBrush(Color.FromArgb(217, 160, 102));
                        using var darkB = new SolidBrush(Color.FromArgb(160, 100, 50));
                        g.FillPolygon(lightB, top);
                        g.FillPolygon(darkB, new[] { new PointF(3, 8), new PointF(12, 13), new PointF(12, 22), new PointF(3, 17) });
                        g.FillPolygon(lightB, new[] { new PointF(21, 8), new PointF(12, 13), new PointF(12, 22), new PointF(21, 17) });
                        g.DrawPolygon(ink, new[] { new PointF(12, 3), new PointF(21, 8), new PointF(21, 17), new PointF(12, 22), new PointF(3, 17), new PointF(3, 8) });
                        g.DrawLine(ink, 3, 8, 12, 13);
                        g.DrawLine(ink, 21, 8, 12, 13);
                        g.DrawLine(ink, 12, 13, 12, 22);
                        break;
                    }
                case "multi":
                    {
                        using var wall = new SolidBrush(Color.FromArgb(226, 232, 240));
                        using var roof = new SolidBrush(Color.FromArgb(185, 28, 28));
                        g.FillRectangle(wall, 5, 11, 14, 10);
                        g.DrawRectangle(ink, 5, 11, 14, 10);
                        g.FillPolygon(roof, new[] { new PointF(3, 12), new PointF(12, 3), new PointF(21, 12) });
                        g.DrawPolygon(ink, new[] { new PointF(3, 12), new PointF(12, 3), new PointF(21, 12) });
                        g.FillRectangle(inkBrush, 10, 15, 4, 6);
                        break;
                    }
                case "template":
                    {
                        using var paper = new SolidBrush(Color.FromArgb(219, 234, 254));
                        g.FillRectangle(paper, 3, 3, 18, 18);
                        using var blue = new Pen(Sea, 1.4f);
                        g.DrawRectangle(blue, 3, 3, 18, 18);
                        g.DrawRectangle(blue, 6, 7, 7, 6);
                        g.DrawLine(blue, 6, 16, 18, 16);
                        g.DrawLine(blue, 15, 7, 18, 7);
                        g.DrawLine(blue, 15, 10, 18, 10);
                        break;
                    }
                case "delete":
                    using (var x = new Pen(Red, 3f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    {
                        g.DrawLine(x, 6, 6, 18, 18);
                        g.DrawLine(x, 18, 6, 6, 18);
                    }
                    break;
                case "hue":
                    {
                        using var path = new GraphicsPath();
                        path.AddBezier(12, 2, 6, 10, 4, 14, 6, 18);
                        path.AddArc(5, 10, 14, 12, 160, -140);
                        path.AddBezier(18, 18, 20, 14, 18, 10, 12, 2);
                        using var lg = new LinearGradientBrush(new Rectangle(4, 2, 16, 20), Color.FromArgb(236, 72, 153), Color.FromArgb(59, 130, 246), 45f);
                        g.FillPath(lg, path);
                        g.DrawPath(ink, path);
                        break;
                    }
                case "landset":
                    Diamond(g, Grass, 12);
                    break;
                case "setheight":
                    Diamond(g, Grass, 15);
                    using (var eq = new Pen(Ink, 2f))
                    {
                        g.DrawLine(eq, 8, 5, 16, 5);
                        g.DrawLine(eq, 8, 9, 16, 9);
                    }
                    break;
                case "smooth":
                    {
                        using var p = new Pen(Grass, 2.4f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                        g.DrawBezier(p, 2, 16, 8, 6, 15, 18, 22, 9);
                        using var p2 = new Pen(Color.FromArgb(120, Ink), 1.2f) { DashStyle = DashStyle.Dot };
                        g.DrawLine(p2, 2, 13, 22, 13);
                        break;
                    }
                case "coast":
                    Coast(g);
                    break;
                case "coastsmooth":
                    Coast(g);
                    Sparkle(g, 18, 5, Color.FromArgb(250, 204, 21));
                    break;
                case "water":
                    {
                        using var drop = new SolidBrush(Sea);
                        g.FillEllipse(drop, 5, 9, 9, 10);
                        g.FillPolygon(drop, new[] { new PointF(9.5f, 3), new PointF(5.4f, 12), new PointF(13.6f, 12) });
                        g.FillEllipse(drop, 14, 13, 6, 7);
                        g.FillPolygon(drop, new[] { new PointF(17, 9), new PointF(14.3f, 15), new PointF(19.7f, 15) });
                        break;
                    }
                case "area":
                    using (var dash = new Pen(Color.FromArgb(8, 145, 178), 1.8f) { DashStyle = DashStyle.Dash })
                    {
                        g.DrawPolygon(dash, new[] { new PointF(12, 3), new PointF(22, 12), new PointF(12, 21), new PointF(2, 12) });
                    }
                    break;
                case "mountain":
                    Mountain(g, Rock);
                    using (var snow = new SolidBrush(Color.White))
                    {
                        g.FillPolygon(snow, new[] { new PointF(12, 4), new PointF(9.5f, 9), new PointF(14.5f, 9) });
                    }
                    break;
                case "heightkeep":
                    Diamond(g, Color.FromArgb(161, 98, 7), 14);
                    using (var p = new Pen(Color.FromArgb(8, 145, 178), 2f) { StartCap = LineCap.Round, EndCap = LineCap.ArrowAnchor })
                    {
                        g.DrawLine(p, 12, 12, 12, 2);
                    }
                    break;
                default:
                    {
                        using var c = new SolidBrush(Color.FromArgb(71, 85, 105));
                        g.FillEllipse(c, 2, 2, 20, 20);
                        using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
                        using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString(key.Length > 0 ? key.Substring(0, 1).ToUpperInvariant() : "?", font, Brushes.White, new RectangleF(0, 0, Size, Size), sf);
                        break;
                    }
            }
        }

        private static void Brush(Graphics g, Color paint)
        {
            using var handle = new Pen(Color.FromArgb(146, 64, 14), 3f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawLine(handle, 20, 3, 12, 12);
            using var ferrule = new Pen(Color.FromArgb(148, 163, 184), 3.4f);
            g.DrawLine(ferrule, 12.5f, 11.5f, 10.5f, 13.5f);
            using var tip = new SolidBrush(paint);
            g.FillPolygon(tip, new[] { new PointF(9, 12), new PointF(12, 15), new PointF(7, 21), new PointF(3, 21), new PointF(4, 17) });
        }

        private static void Mountain(Graphics g, Color c)
        {
            using var b = new SolidBrush(c);
            using var pen = new Pen(Ink, 1.2f) { LineJoin = LineJoin.Round };
            var pts = new[] { new PointF(1, 21), new PointF(8, 9), new PointF(11, 12), new PointF(12, 4), new PointF(23, 21) };
            g.FillPolygon(b, pts);
            g.DrawPolygon(pen, pts);
        }

        private static void Arrow(Graphics g, bool up, Color c)
        {
            using var p = new Pen(c, 2.6f) { EndCap = LineCap.ArrowAnchor, StartCap = LineCap.Round };
            if (up)
            {
                g.DrawLine(p, 19, 13, 19, 2);
            }
            else
            {
                g.DrawLine(p, 19, 2, 19, 13);
            }
        }

        private static void Diamond(Graphics g, Color c, float cy)
        {
            using var b = new SolidBrush(c);
            using var pen = new Pen(Ink, 1.2f);
            var pts = new[] { new PointF(12, cy - 6), new PointF(22, cy), new PointF(12, cy + 6), new PointF(2, cy) };
            g.FillPolygon(b, pts);
            g.DrawPolygon(pen, pts);
        }

        private static void Coast(Graphics g)
        {
            using var sand = new SolidBrush(Sand);
            using var sea = new SolidBrush(Sea);
            g.FillRectangle(sea, 2, 2, 20, 20);
            g.FillPolygon(sand, new[] { new PointF(2, 2), new PointF(14, 2), new PointF(10, 9), new PointF(12, 15), new PointF(6, 22), new PointF(2, 22) });
            using var foam = new Pen(Color.White, 1.4f);
            g.DrawBezier(foam, 14, 3, 11, 8, 14, 13, 8, 21);
            using var pen = new Pen(Ink, 1f);
            g.DrawRectangle(pen, 2, 2, 20, 20);
        }

        private static void Stamp(Graphics g, Color c)
        {
            using var b = new SolidBrush(c);
            g.FillRectangle(b, 14, 15, 9, 3);
            g.FillRectangle(b, 17, 9, 3, 6);
        }

        private static void Sparkle(Graphics g, float x, float y, Color c)
        {
            using var b = new SolidBrush(c);
            g.FillPolygon(b, new[]
            {
                new PointF(x, y - 5), new PointF(x + 1.3f, y - 1.3f), new PointF(x + 5, y), new PointF(x + 1.3f, y + 1.3f),
                new PointF(x, y + 5), new PointF(x - 1.3f, y + 1.3f), new PointF(x - 5, y), new PointF(x - 1.3f, y - 1.3f)
            });
        }
    }
}

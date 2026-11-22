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
                case "scatter":
                    {
                        Tree(g, 7, 13, 1f);
                        Tree(g, 16, 9, 0.8f);
                        Tree(g, 15, 18, 0.7f);
                        break;
                    }
                case "huebrush":
                    {
                        using var lg = new LinearGradientBrush(new Rectangle(2, 12, 20, 10), Color.FromArgb(236, 72, 153), Color.FromArgb(59, 130, 246), 0f);
                        g.FillEllipse(lg, 2, 13, 14, 9);
                        Brush(g, Color.FromArgb(236, 72, 153));
                        break;
                    }
                case "wall":
                    {
                        using var stone = new SolidBrush(Color.FromArgb(168, 162, 158));
                        g.FillRectangle(stone, 2, 7, 20, 13);
                        using var mortar = new Pen(Color.FromArgb(87, 83, 78), 1.2f);
                        g.DrawRectangle(mortar, 2, 7, 20, 13);
                        g.DrawLine(mortar, 2, 11.5f, 22, 11.5f);
                        g.DrawLine(mortar, 2, 16, 22, 16);
                        foreach (float x in new[] { 8f, 15f })
                        {
                            g.DrawLine(mortar, x, 7, x, 11.5f);
                            g.DrawLine(mortar, x, 16, x, 20);
                        }
                        g.DrawLine(mortar, 11.5f, 11.5f, 11.5f, 16);
                        g.DrawLine(mortar, 18.5f, 11.5f, 18.5f, 16);
                        g.DrawLine(mortar, 4.5f, 11.5f, 4.5f, 16);
                        break;
                    }
                case "river":
                    {
                        using var grass = new SolidBrush(Color.FromArgb(134, 194, 120));
                        g.FillRectangle(grass, 2, 2, 20, 20);
                        using var water = new Pen(Sea, 5f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                        g.DrawBezier(water, 4, 2, 2, 10, 20, 12, 18, 22);
                        using var light = new Pen(Color.FromArgb(147, 197, 253), 1.2f);
                        g.DrawBezier(light, 5, 4, 4, 10, 18, 12, 17, 20);
                        break;
                    }
                case "road":
                    {
                        using var grass = new SolidBrush(Color.FromArgb(134, 194, 120));
                        g.FillRectangle(grass, 2, 2, 20, 20);
                        using var road = new Pen(Color.FromArgb(161, 120, 70), 6f) { StartCap = LineCap.Flat, EndCap = LineCap.Flat };
                        g.DrawBezier(road, 3, 21, 8, 10, 16, 16, 21, 3);
                        using var dash = new Pen(Color.FromArgb(250, 240, 200), 1f) { DashStyle = DashStyle.Dash };
                        g.DrawBezier(dash, 3, 21, 8, 10, 16, 16, 21, 3);
                        using var dot = new SolidBrush(Red);
                        g.FillEllipse(dot, 1, 18, 5, 5);
                        g.FillEllipse(dot, 18, 1, 5, 5);
                        break;
                    }
                case "ramp":
                    {
                        using var b = new SolidBrush(Color.FromArgb(161, 120, 70));
                        var pts = new[] { new PointF(2, 21), new PointF(22, 21), new PointF(22, 6) };
                        g.FillPolygon(b, pts);
                        g.DrawPolygon(new Pen(Ink, 1.2f), pts);
                        Arrow(g, true, Color.FromArgb(22, 163, 74));
                        break;
                    }
                case "plateau":
                    {
                        using var b = new SolidBrush(Grass);
                        var pts = new[] { new PointF(1, 21), new PointF(6, 9), new PointF(18, 9), new PointF(23, 21) };
                        g.FillPolygon(b, pts);
                        g.DrawPolygon(new Pen(Ink, 1.2f), pts);
                        using var top = new Pen(Color.FromArgb(250, 204, 21), 2f);
                        g.DrawLine(top, 6, 9, 18, 9);
                        break;
                    }
                case "noise":
                    {
                        using var p = new Pen(Grass, 2f) { LineJoin = LineJoin.Round };
                        g.DrawLines(p, new[] { new PointF(1, 16), new PointF(4, 12), new PointF(7, 15), new PointF(10, 9), new PointF(13, 14), new PointF(16, 10), new PointF(19, 15), new PointF(23, 11) });
                        g.DrawLine(new Pen(Color.FromArgb(120, Ink), 1f), 1, 20, 23, 20);
                        break;
                    }
                case "variants":
                    {
                        Color[] c = { Color.FromArgb(22, 128, 61), Color.FromArgb(34, 160, 80), Color.FromArgb(21, 110, 50), Color.FromArgb(50, 140, 70) };
                        for (int y = 0; y < 3; y++)
                        {
                            for (int x = 0; x < 3; x++)
                            {
                                using var b = new SolidBrush(c[(x * 7 + y * 5) % 4]);
                                g.FillRectangle(b, 3 + x * 6, 3 + y * 6, 6, 6);
                            }
                        }
                        g.DrawRectangle(new Pen(Ink, 1f), 3, 3, 18, 18);
                        break;
                    }
                case "transitionbrush":
                    {
                        using var lg = new LinearGradientBrush(new Rectangle(2, 2, 20, 20), Color.FromArgb(22, 128, 61), Sand, 45f);
                        g.FillRectangle(lg, 2, 2, 20, 20);
                        g.DrawRectangle(new Pen(Ink, 1f), 2, 2, 20, 20);
                        Sparkle(g, 12, 12, Color.White);
                        break;
                    }
                case "lasso":
                    {
                        using var p = new Pen(Color.FromArgb(8, 145, 178), 1.8f) { DashStyle = DashStyle.Dash };
                        g.DrawClosedCurve(p, new[] { new PointF(4, 8), new PointF(12, 3), new PointF(21, 7), new PointF(18, 15), new PointF(8, 16) });
                        using var rope = new Pen(Ink, 1.4f);
                        g.DrawBezier(rope, 8, 16, 6, 19, 9, 21, 5, 23);
                        break;
                    }
                case "magicwand":
                    {
                        using var stick = new Pen(Ink, 2.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                        g.DrawLine(stick, 4, 21, 15, 10);
                        Sparkle(g, 17, 7, Color.FromArgb(250, 204, 21));
                        Sparkle(g, 9, 5, Color.FromArgb(8, 145, 178));
                        break;
                    }
                case "measure":
                    {
                        using var b = new SolidBrush(Color.FromArgb(250, 204, 21));
                        var pts = new[] { new PointF(2, 16), new PointF(16, 2), new PointF(22, 8), new PointF(8, 22) };
                        g.FillPolygon(b, pts);
                        g.DrawPolygon(new Pen(Ink, 1.2f), pts);
                        using var tick = new Pen(Ink, 1f);
                        for (int k = 1; k < 6; k++)
                        {
                            float x = 2 + k * 2.4f, y = 16 - k * 2.4f;
                            g.DrawLine(tick, x, y, x + (k % 2 == 0 ? 3 : 2), y + (k % 2 == 0 ? 3 : 2));
                        }
                        break;
                    }
                case "halfcircle":
                    using (var fill = new SolidBrush(Color.FromArgb(124, 58, 237)))
                    {
                        g.FillPie(fill, 2, 5, 20, 20, 180, 180);
                    }
                    g.DrawPie(ink, 2, 5, 20, 20, 180, 180);
                    break;
                case "triangle":
                    {
                        var pts = new[] { new PointF(12, 3), new PointF(22, 20), new PointF(2, 20) };
                        using var fill = new SolidBrush(Color.FromArgb(124, 58, 237));
                        g.FillPolygon(fill, pts);
                        g.DrawPolygon(ink, pts);
                        break;
                    }
                case "slopeline":
                    {
                        using var lg = new LinearGradientBrush(new Rectangle(2, 2, 20, 20), Color.FromArgb(22, 128, 61), Color.FromArgb(161, 98, 7), 45f);
                        using var p = new Pen(lg, 5f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                        g.DrawLine(p, 4, 20, 20, 4);
                        using var a = new Pen(Ink, 1.4f) { EndCap = LineCap.ArrowAnchor };
                        g.DrawLine(a, 6, 22, 21, 7);
                        break;
                    }
                case "course":
                    {
                        using var p = new Pen(Sea, 3.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                        g.DrawBezier(p, 2, 4, 20, 2, 2, 22, 22, 20);
                        using var dot = new SolidBrush(Red);
                        g.FillEllipse(dot, 0, 2, 5, 5);
                        g.FillEllipse(dot, 19, 17, 5, 5);
                        break;
                    }
                case "dungeon":
                    {
                        using var rock = new SolidBrush(Color.FromArgb(68, 64, 60));
                        g.FillRectangle(rock, 2, 2, 20, 20);
                        using var floor = new SolidBrush(Color.FromArgb(214, 211, 209));
                        g.FillRectangle(floor, 4, 4, 6, 6);
                        g.FillRectangle(floor, 13, 12, 7, 7);
                        g.FillRectangle(floor, 6, 9, 2, 6);
                        g.FillRectangle(floor, 6, 14, 8, 2);
                        break;
                    }
                case "bridge":
                    {
                        using var sea = new SolidBrush(Sea);
                        g.FillRectangle(sea, 2, 14, 20, 8);
                        using var wood = new Pen(Color.FromArgb(146, 64, 14), 3f);
                        g.DrawArc(wood, 2, 8, 20, 14, 180, 180);
                        using var rail = new Pen(Color.FromArgb(120, 53, 15), 1.4f);
                        g.DrawArc(rail, 2, 4, 20, 14, 180, 180);
                        for (int x = 5; x <= 19; x += 7)
                        {
                            g.DrawLine(rail, x, 9, x, 13);
                        }
                        break;
                    }
                case "cellar":
                    {
                        using var grass = new SolidBrush(Grass);
                        g.FillRectangle(grass, 1, 3, 22, 4);
                        using var dark = new SolidBrush(Color.FromArgb(41, 37, 36));
                        g.FillRectangle(dark, 4, 7, 16, 15);
                        using var step = new Pen(Color.FromArgb(214, 211, 209), 1.6f);
                        for (int k = 0; k < 4; k++)
                        {
                            g.DrawLine(step, 6 + k * 3, 9 + k * 3, 10 + k * 3, 9 + k * 3);
                            g.DrawLine(step, 10 + k * 3, 9 + k * 3, 10 + k * 3, 12 + k * 3);
                        }
                        break;
                    }
                case "undermap":
                    {
                        using var grass = new SolidBrush(Color.FromArgb(140, Grass));
                        g.FillRectangle(grass, 1, 3, 22, 6);
                        using var p = new Pen(Color.FromArgb(126, 34, 206), 2.4f) { EndCap = LineCap.ArrowAnchor };
                        g.DrawLine(p, 12, 4, 12, 21);
                        using var dash = new Pen(Ink, 1f) { DashStyle = DashStyle.Dash };
                        g.DrawLine(dash, 1, 17, 23, 17);
                        break;
                    }
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

        private static void Tree(Graphics g, float x, float y, float k)
        {
            using var trunk = new SolidBrush(Color.FromArgb(120, 72, 30));
            g.FillRectangle(trunk, x - 1 * k, y + 2 * k, 2 * k, 6 * k);
            using var leaves = new SolidBrush(Grass);
            g.FillEllipse(leaves, x - 5 * k, y - 6 * k, 10 * k, 10 * k);
            using var pen = new Pen(Color.FromArgb(20, 83, 45), 1f);
            g.DrawEllipse(pen, x - 5 * k, y - 6 * k, 10 * k, 10 * k);
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

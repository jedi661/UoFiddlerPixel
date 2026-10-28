/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace UoFiddler.Controls.MapViewer.Editor.Tools
{
    public enum ShapeKind
    {
        Rectangle,
        Ellipse,
        Triangle,
        Line
    }

    /// <summary>
    /// Cells of lines, curves and shapes (map coordinates). The result can be used with every tool.
    /// </summary>
    public static class Shapes
    {
        /// <summary>Straight line (Bresenham), both ends included</summary>
        public static List<Point> Line(Point a, Point b)
        {
            var result = new List<Point>();
            int x0 = a.X, y0 = a.Y, x1 = b.X, y1 = b.Y;
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            while (true)
            {
                result.Add(new Point(x0, y0));
                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                int e2 = 2 * err;
                if (e2 >= dy)
                {
                    err += dy;
                    x0 += sx;
                }

                if (e2 <= dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }

            return result;
        }

        /// <summary>Lines through all points</summary>
        public static List<Point> Polyline(IReadOnlyList<Point> points)
        {
            var result = new List<Point>();
            if (points.Count == 1)
            {
                result.Add(points[0]);
            }

            for (int i = 1; i < points.Count; i++)
            {
                List<Point> part = Line(points[i - 1], points[i]);
                result.AddRange(i == 1 ? part : part.Skip(1));
            }

            return result;
        }

        /// <summary>Smooth curve through all points (Catmull-Rom), as a gap free chain of cells</summary>
        public static List<Point> Curve(IReadOnlyList<Point> points)
        {
            if (points.Count < 3)
            {
                return Polyline(points);
            }

            var samples = new List<Point>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                PointF p0 = points[Math.Max(0, i - 1)], p1 = points[i], p2 = points[i + 1], p3 = points[Math.Min(points.Count - 1, i + 2)];
                double length = Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
                int steps = Math.Max(2, (int)(length * 3));
                for (int s = 0; s < steps; s++)
                {
                    double t = s / (double)steps, t2 = t * t, t3 = t2 * t;
                    double x = 0.5 * ((2 * p1.X) + ((-p0.X + p2.X) * t) + (((2 * p0.X) - (5 * p1.X) + (4 * p2.X) - p3.X) * t2) + ((-p0.X + (3 * p1.X) - (3 * p2.X) + p3.X) * t3));
                    double y = 0.5 * ((2 * p1.Y) + ((-p0.Y + p2.Y) * t) + (((2 * p0.Y) - (5 * p1.Y) + (4 * p2.Y) - p3.Y) * t2) + ((-p0.Y + (3 * p1.Y) - (3 * p2.Y) + p3.Y) * t3));
                    samples.Add(new Point((int)Math.Round(x), (int)Math.Round(y)));
                }
            }

            samples.Add(points[points.Count - 1]);

            // connect the samples without gaps
            var result = new List<Point>();
            for (int i = 0; i < samples.Count; i++)
            {
                if (i == 0)
                {
                    result.Add(samples[0]);
                    continue;
                }

                if (samples[i] == samples[i - 1])
                {
                    continue;
                }

                result.AddRange(Line(samples[i - 1], samples[i]).Skip(1));
            }

            return result;
        }

        /// <summary>Cells of a shape in the box between two corners</summary>
        public static List<Point> Shape(ShapeKind kind, Point a, Point b, bool filled)
        {
            int x0 = Math.Min(a.X, b.X), x1 = Math.Max(a.X, b.X), y0 = Math.Min(a.Y, b.Y), y1 = Math.Max(a.Y, b.Y);
            switch (kind)
            {
                case ShapeKind.Line:
                    return Line(a, b);
                case ShapeKind.Rectangle:
                {
                    var result = new List<Point>();
                    for (int y = y0; y <= y1; y++)
                    {
                        for (int x = x0; x <= x1; x++)
                        {
                            if (filled || x == x0 || x == x1 || y == y0 || y == y1)
                            {
                                result.Add(new Point(x, y));
                            }
                        }
                    }

                    return result;
                }

                case ShapeKind.Ellipse:
                {
                    double cx = (x0 + x1) / 2.0, cy = (y0 + y1) / 2.0;
                    double rx = Math.Max(0.5, (x1 - x0 + 1) / 2.0), ry = Math.Max(0.5, (y1 - y0 + 1) / 2.0);
                    var inside = new HashSet<Point>();
                    for (int y = y0; y <= y1; y++)
                    {
                        for (int x = x0; x <= x1; x++)
                        {
                            double dx = (x - cx) / rx, dy = (y - cy) / ry;
                            if ((dx * dx) + (dy * dy) <= 1.0)
                            {
                                inside.Add(new Point(x, y));
                            }
                        }
                    }

                    return filled ? inside.ToList() : Outline(inside);
                }

                default:
                {
                    // triangle: tip in the middle of the upper edge, base = lower edge
                    var tip = new PointF((x0 + x1) / 2f, y0);
                    var left = new PointF(x0, y1);
                    var right = new PointF(x1, y1);
                    var inside = new HashSet<Point>();
                    for (int y = y0; y <= y1; y++)
                    {
                        for (int x = x0; x <= x1; x++)
                        {
                            if (InTriangle(new PointF(x, y), tip, left, right))
                            {
                                inside.Add(new Point(x, y));
                            }
                        }
                    }

                    inside.Add(Point.Round(tip));
                    return filled ? inside.ToList() : Outline(inside);
                }
            }
        }

        private static bool InTriangle(PointF p, PointF a, PointF b, PointF c)
        {
            float Sign(PointF p1, PointF p2, PointF p3) => ((p1.X - p3.X) * (p2.Y - p3.Y)) - ((p2.X - p3.X) * (p1.Y - p3.Y));
            const float tolerance = 0.5f;
            float d1 = Sign(p, a, b), d2 = Sign(p, b, c), d3 = Sign(p, c, a);
            bool neg = d1 < -tolerance || d2 < -tolerance || d3 < -tolerance;
            bool pos = d1 > tolerance || d2 > tolerance || d3 > tolerance;
            return !(neg && pos);
        }

        /// <summary>Cells of a filled area that touch a cell outside of it</summary>
        public static List<Point> Outline(HashSet<Point> area)
        {
            return area.Where(p => !area.Contains(new Point(p.X + 1, p.Y)) || !area.Contains(new Point(p.X - 1, p.Y)) ||
                                   !area.Contains(new Point(p.X, p.Y + 1)) || !area.Contains(new Point(p.X, p.Y - 1))).ToList();
        }

        /// <summary>Makes a line as thick as the brush (union of the brush cells at every point)</summary>
        public static List<Point> Thicken(IEnumerable<Point> cells, int size, BrushShape shape, int mapWidth, int mapHeight)
        {
            var result = new HashSet<Point>();
            foreach (Point p in cells)
            {
                if (size <= 1)
                {
                    if (p.X >= 0 && p.Y >= 0 && p.X < mapWidth && p.Y < mapHeight)
                    {
                        result.Add(p);
                    }

                    continue;
                }

                result.UnionWith(MapBrush.Cells(p.X, p.Y, size, shape, mapWidth, mapHeight));
            }

            return result.ToList();
        }
    }
}

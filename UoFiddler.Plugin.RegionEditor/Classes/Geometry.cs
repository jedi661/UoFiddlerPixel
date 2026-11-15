/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Geometry of the region editor: cell masks, polygons, circles, flood fill
 * and the conversion of any shape into as few rectangles as possible
 * (server formats only know rectangles).
 *
 * A cell (x, y) belongs to a shape when its center (x + 0.5, y + 0.5) is inside.
 *
 ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace UoFiddler.Plugin.RegionEditor.Classes
{
    /// <summary>Set of map cells inside a bounding rectangle</summary>
    public sealed class CellMask
    {
        /// <summary>largest mask (cells) that is created, protects against huge allocations</summary>
        public const long MaxCells = 64L * 1024 * 1024;

        private readonly BitArray _bits;

        public CellMask(Rectangle bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                bounds = new Rectangle(bounds.X, bounds.Y, 0, 0);
            }

            if ((long)bounds.Width * bounds.Height > MaxCells)
            {
                throw new ArgumentException("Area too large");
            }

            Bounds = bounds;
            _bits = new BitArray(Math.Max(1, bounds.Width * bounds.Height));
        }

        public Rectangle Bounds { get; }

        public int Count { get; private set; }

        public bool Get(int x, int y)
        {
            if (x < Bounds.X || y < Bounds.Y || x >= Bounds.Right || y >= Bounds.Bottom)
            {
                return false;
            }

            return _bits[((y - Bounds.Y) * Bounds.Width) + (x - Bounds.X)];
        }

        public void Set(int x, int y, bool value)
        {
            if (x < Bounds.X || y < Bounds.Y || x >= Bounds.Right || y >= Bounds.Bottom)
            {
                return;
            }

            int i = ((y - Bounds.Y) * Bounds.Width) + (x - Bounds.X);
            if (_bits[i] != value)
            {
                _bits[i] = value;
                Count += value ? 1 : -1;
            }
        }

        public void SetRect(Rectangle r, bool value)
        {
            r.Intersect(Bounds);
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    Set(x, y, value);
                }
            }
        }

        /// <summary>runs [x1, x2) of set cells in a row</summary>
        public IEnumerable<(int X1, int X2)> Runs(int y)
        {
            int x = Bounds.X;
            while (x < Bounds.Right)
            {
                while (x < Bounds.Right && !Get(x, y))
                {
                    x++;
                }

                if (x >= Bounds.Right)
                {
                    yield break;
                }

                int start = x;
                while (x < Bounds.Right && Get(x, y))
                {
                    x++;
                }

                yield return (start, x);
            }
        }
    }

    public static class Geometry
    {
        #region Polygons

        public static bool PointInPolygon(IList<Point> poly, double px, double py)
        {
            bool inside = false;
            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                double xi = poly[i].X, yi = poly[i].Y, xj = poly[j].X, yj = poly[j].Y;
                if ((yi > py) != (yj > py) && px < ((xj - xi) * (py - yi) / (yj - yi)) + xi)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        /// <summary>area (shoelace formula, absolute)</summary>
        public static double PolygonArea(IList<Point> poly)
        {
            double sum = 0;
            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                sum += ((double)poly[j].X * poly[i].Y) - ((double)poly[i].X * poly[j].Y);
            }

            return Math.Abs(sum / 2);
        }

        /// <summary>runs [x1, x2) of the cells of a row whose centers are inside the polygon</summary>
        public static List<(int X1, int X2)> PolygonRuns(IList<Point> poly, int y)
        {
            var result = new List<(int, int)>();
            if (poly.Count < 3)
            {
                return result;
            }

            double py = y + 0.5;
            var xs = new List<double>();
            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                double yi = poly[i].Y, yj = poly[j].Y;
                if ((yi > py) != (yj > py))
                {
                    xs.Add(poly[i].X + ((py - yi) * (poly[j].X - poly[i].X) / (yj - yi)));
                }
            }

            xs.Sort();
            for (int k = 0; k + 1 < xs.Count; k += 2)
            {
                // cell x is inside when x + 0.5 is in [a, b)
                int x1 = (int)Math.Ceiling(xs[k] - 0.5);
                int x2 = (int)Math.Ceiling(xs[k + 1] - 0.5);
                if (x2 > x1)
                {
                    result.Add((x1, x2));
                }
            }

            return result;
        }

        /// <summary>polygon as rectangles (rows of equal runs are merged)</summary>
        public static List<RegionRect> PolygonToRects(IList<Point> poly)
        {
            if (poly.Count < 3)
            {
                return new List<RegionRect>();
            }

            int y1 = poly.Min(p => p.Y), y2 = poly.Max(p => p.Y);
            return RunsToRects(y1, y2, y => PolygonRuns(poly, y));
        }

        /// <summary>true when the polygon is a simple axis aligned rectangle</summary>
        public static bool IsRectangle(IList<Point> poly, out Rectangle rect)
        {
            rect = Rectangle.Empty;
            List<Point> pts = poly.Distinct().ToList();
            if (pts.Count != 4)
            {
                return false;
            }

            int x1 = pts.Min(p => p.X), x2 = pts.Max(p => p.X), y1 = pts.Min(p => p.Y), y2 = pts.Max(p => p.Y);
            if (pts.All(p => (p.X == x1 || p.X == x2) && (p.Y == y1 || p.Y == y2)) && x2 > x1 && y2 > y1)
            {
                rect = Rectangle.FromLTRB(x1, y1, x2, y2);
                return true;
            }

            return false;
        }

        #endregion

        #region Runs / masks -> rectangles

        /// <summary>
        /// Converts row runs into rectangles: a run that continues with the same x range
        /// in the next row extends the rectangle downwards.
        /// </summary>
        public static List<RegionRect> RunsToRects(int yStart, int yEnd, Func<int, IEnumerable<(int X1, int X2)>> runsOfRow)
        {
            var done = new List<RegionRect>();
            var open = new Dictionary<(int, int), RegionRect>();
            for (int y = yStart; y <= yEnd; y++)
            {
                var row = new HashSet<(int, int)>(runsOfRow(y));
                foreach (var key in open.Keys.ToList())
                {
                    if (!row.Contains(key))
                    {
                        done.Add(open[key]);
                        open.Remove(key);
                    }
                }

                foreach (var run in row)
                {
                    if (open.TryGetValue(run, out RegionRect r))
                    {
                        r.Height++;
                    }
                    else
                    {
                        open[run] = new RegionRect { X = run.Item1, Y = y, Width = run.Item2 - run.Item1, Height = 1 };
                    }
                }
            }

            done.AddRange(open.Values);
            return done.OrderBy(r => r.Y).ThenBy(r => r.X).ToList();
        }

        public static List<RegionRect> MaskToRects(CellMask mask)
        {
            if (mask.Count == 0)
            {
                return new List<RegionRect>();
            }

            return RunsToRects(mask.Bounds.Top, mask.Bounds.Bottom - 1, mask.Runs);
        }

        public static CellMask RectsToMask(IEnumerable<RegionRect> rects)
        {
            List<RegionRect> list = rects.Where(r => r.Width > 0 && r.Height > 0).ToList();
            Rectangle bounds = Rectangle.Empty;
            foreach (RegionRect r in list)
            {
                bounds = bounds.IsEmpty ? r.Bounds : Rectangle.Union(bounds, r.Bounds);
            }

            var mask = new CellMask(bounds);
            foreach (RegionRect r in list)
            {
                mask.SetRect(r.Bounds, true);
            }

            return mask;
        }

        /// <summary>
        /// Merges overlapping/adjacent rectangles into a minimal set (only rectangles with the
        /// same z range and expansion are merged). Returns the input when the area is too large.
        /// </summary>
        public static List<RegionRect> Optimize(IEnumerable<RegionRect> rects)
        {
            var result = new List<RegionRect>();
            foreach (var group in rects.GroupBy(r => (r.ZMin, r.ZMax, r.Expansion ?? string.Empty)))
            {
                List<RegionRect> list = group.ToList();
                try
                {
                    List<RegionRect> merged = MaskToRects(RectsToMask(list));
                    if (merged.Count > list.Count)
                    {
                        merged = list.Select(r => r.Clone()).ToList();
                    }

                    foreach (RegionRect r in merged)
                    {
                        r.ZMin = group.Key.ZMin;
                        r.ZMax = group.Key.ZMax;
                        r.Expansion = string.IsNullOrEmpty(group.Key.Item3) ? null : group.Key.Item3;
                    }

                    result.AddRange(merged);
                }
                catch (ArgumentException)
                {
                    result.AddRange(list.Select(r => r.Clone()));
                }
            }

            return result;
        }

        /// <summary>rectangles minus a cut-out rectangle</summary>
        public static List<RegionRect> Subtract(IEnumerable<RegionRect> rects, Rectangle cut)
        {
            var result = new List<RegionRect>();
            foreach (RegionRect r in rects)
            {
                Rectangle a = r.Bounds;
                if (!a.IntersectsWith(cut))
                {
                    result.Add(r.Clone());
                    continue;
                }

                Rectangle i = Rectangle.Intersect(a, cut);
                void Add(Rectangle p)
                {
                    if (p.Width > 0 && p.Height > 0)
                    {
                        RegionRect n = r.Clone();
                        n.X = p.X;
                        n.Y = p.Y;
                        n.Width = p.Width;
                        n.Height = p.Height;
                        result.Add(n);
                    }
                }

                Add(Rectangle.FromLTRB(a.Left, a.Top, a.Right, i.Top));
                Add(Rectangle.FromLTRB(a.Left, i.Bottom, a.Right, a.Bottom));
                Add(Rectangle.FromLTRB(a.Left, i.Top, i.Left, i.Bottom));
                Add(Rectangle.FromLTRB(i.Right, i.Top, a.Right, i.Bottom));
            }

            return result;
        }

        #endregion

        #region Circle / flood fill

        /// <summary>cells of a circle (center cell, radius in cells) as rectangles</summary>
        public static List<RegionRect> CircleToRects(Point center, int radius)
        {
            radius = Math.Max(0, radius);
            double r2 = (radius + 0.5) * (radius + 0.5);
            return RunsToRects(center.Y - radius, center.Y + radius, y =>
            {
                double dy = y - center.Y;
                double rest = r2 - (dy * dy);
                if (rest < 0)
                {
                    return Array.Empty<(int, int)>();
                }

                int dx = (int)Math.Floor(Math.Sqrt(rest));
                return new[] { (center.X - dx, center.X + dx + 1) };
            });
        }

        /// <summary>
        /// Flood fill (4 neighbours) from a start cell over all cells for which <paramref name="match"/> is true.
        /// Stops at <paramref name="limit"/> (returns null when the area is larger than the limit).
        /// </summary>
        public static CellMask FloodFill(Point start, Rectangle area, Func<int, int, bool> match, int maxCells)
        {
            if (!area.Contains(start) || !match(start.X, start.Y))
            {
                return null;
            }

            var mask = new CellMask(area);
            var stack = new Stack<Point>();
            stack.Push(start);
            mask.Set(start.X, start.Y, true);
            while (stack.Count > 0)
            {
                Point p = stack.Pop();
                Visit(p.X + 1, p.Y);
                Visit(p.X - 1, p.Y);
                Visit(p.X, p.Y + 1);
                Visit(p.X, p.Y - 1);
                if (mask.Count > maxCells)
                {
                    return null;
                }
            }

            return mask;

            void Visit(int x, int y)
            {
                if (x < area.Left || y < area.Top || x >= area.Right || y >= area.Bottom || mask.Get(x, y) || !match(x, y))
                {
                    return;
                }

                mask.Set(x, y, true);
                stack.Push(new Point(x, y));
            }
        }

        #endregion

        #region Helpers

        public static Rectangle Normalize(Point a, Point b)
        {
            return Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X) + 1, Math.Max(a.Y, b.Y) + 1);
        }

        public static int Snap(int value, int grid) => grid <= 1 ? value : (int)Math.Round(value / (double)grid) * grid;

        public static Point Snap(Point p, int grid) => new Point(Snap(p.X, grid), Snap(p.Y, grid));

        /// <summary>overlapping area of two rectangle sets (cells)</summary>
        public static long OverlapArea(IEnumerable<RegionRect> a, IEnumerable<RegionRect> b)
        {
            long sum = 0;
            List<RegionRect> listB = b.ToList();
            foreach (RegionRect ra in a)
            {
                foreach (RegionRect rb in listB)
                {
                    Rectangle i = Rectangle.Intersect(ra.Bounds, rb.Bounds);
                    if (i.Width > 0 && i.Height > 0)
                    {
                        sum += (long)i.Width * i.Height;
                    }
                }
            }

            return sum;
        }

        /// <summary>true when every cell of <paramref name="inner"/> lies in one of the <paramref name="outer"/> rectangles</summary>
        public static bool Covers(IEnumerable<RegionRect> outer, IEnumerable<RegionRect> inner)
        {
            List<RegionRect> o = outer.ToList();
            foreach (RegionRect r in inner)
            {
                List<RegionRect> rest = new List<RegionRect> { r.Clone() };
                foreach (RegionRect cut in o)
                {
                    rest = Subtract(rest, cut.Bounds);
                    if (rest.Count == 0)
                    {
                        break;
                    }
                }

                if (rest.Count > 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>all rectangles of a region including the converted polygons</summary>
        public static List<RegionRect> AllRects(RegionNode region)
        {
            var list = region.Rects.Select(r => r.Clone()).ToList();
            foreach (RegionPolygon p in region.Polygons)
            {
                foreach (RegionRect r in PolygonToRects(p.Points))
                {
                    r.ZMin = p.ZMin;
                    r.ZMax = p.ZMax;
                    list.Add(r);
                }
            }

            return list;
        }

        #endregion
    }
}

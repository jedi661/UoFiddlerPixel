/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using UoFiddler.Plugin.Enhanced.EcUltima.Imaging;
using UoFiddler.Plugin.Enhanced.EcUltima.Maps;

namespace UoFiddler.Plugin.Enhanced.Classes
{
    public sealed class EcMapRenderOptions
    {
        public bool Legacy { get; set; }
        public bool ShowLand { get; set; } = true;
        public bool ShowStatics { get; set; } = true;

        /// <summary>Draw flat land tiles with the land art of the classic client loaded in UoFiddler.</summary>
        public bool LandArt { get; set; } = true;

        public bool Grid { get; set; }
        public int MaxZ { get; set; } = 127;
        public Color Background { get; set; } = Color.FromArgb(18, 18, 22);
    }

    /// <summary>A drawn static: map position, data, area on the image.</summary>
    public sealed class EcPlacedStatic
    {
        public int X;
        public int Y;
        public EcStatic Static;
        public EcItemSprite Sprite;
        public Rectangle Rect;
    }

    public sealed class EcMapRenderResult
    {
        public Bitmap Image;
        public List<EcPlacedStatic> Statics = new List<EcPlacedStatic>();

        /// <summary>Image position of the top corner of the tile (area.X, area.Y) at z 0.</summary>
        public Point Origin;

        public int TileHalf;
        public int ZStep;
        public Rectangle Area;
    }

    /// <summary>
    /// Isometric view of a map area of the Enhanced Client: land as terrain with the heights of the four corners
    /// (radar colors with light, or the classic land art for flat tiles) and the statics with the EC images
    /// (<see cref="EcItemSprites"/>), drawn in the order of the client (x + y, then z).
    /// </summary>
    public static class EcMapRenderer
    {
        /// <summary>Biggest image side in pixels (memory).</summary>
        public const int MaxImageSide = 9000;

        public static EcMapRenderResult Render(EcFacet facet, Rectangle area, byte[] radarDds, EcItemSprites sprites, EcMapRenderOptions options)
        {
            area = facet.Clip(area);
            int tile = EcItemSprites.TileWidth(options.Legacy);
            int half = tile / 2;
            int zStep = EcItemSprites.ZStep(options.Legacy);
            var result = new EcMapRenderResult { TileHalf = half, ZStep = zStep, Area = area };
            if (area.Width <= 0 || area.Height <= 0)
            {
                return result;
            }

            // heights incl. one more row / column for the corners
            int w = area.Width, h = area.Height;
            var z = new int[w + 1, h + 1];
            var tiles = new EcMapTile[w, h];
            for (int x = 0; x <= w; x++)
            {
                for (int y = 0; y <= h; y++)
                {
                    EcMapTile t = facet.GetTile(Math.Min(area.X + x, facet.Width - 1), Math.Min(area.Y + y, facet.Height - 1));
                    z[x, y] = t?.Z ?? 0;
                    if (x < w && y < h)
                    {
                        tiles[x, y] = t;
                    }
                }
            }

            byte[] colors = null;
            if (radarDds != null)
            {
                try
                {
                    colors = DdsImage.DecodeRegion(radarDds, area.X, area.Y, w, h).Pixels;
                }
                catch (Exception)
                {
                    colors = null;
                }
            }

            // drawables in client order
            var drawables = new List<(int Depth, int Priority, int Order, Action<Graphics, Point> Draw, Rectangle Bounds)>();
            int order = 0;
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            void Bound(Rectangle r)
            {
                minX = Math.Min(minX, r.Left);
                minY = Math.Min(minY, r.Top);
                maxX = Math.Max(maxX, r.Right);
                maxY = Math.Max(maxY, r.Bottom);
            }

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    EcMapTile t = tiles[x, y];
                    if (t == null)
                    {
                        continue;
                    }
                    int cx = (x - y) * half;
                    int top = (x + y) * half;
                    int zt = z[x, y], zr = z[x + 1, y], zb = z[x + 1, y + 1], zl = z[x, y + 1];
                    Point[] land =
                    {
                        new Point(cx, top - (zt * zStep)),
                        new Point(cx + half, top + half - (zr * zStep)),
                        new Point(cx, top + tile - (zb * zStep)),
                        new Point(cx - half, top + half - (zl * zStep))
                    };
                    var landBounds = Rectangle.FromLTRB(cx - half, land.Min(p => p.Y), cx + half, land.Max(p => p.Y));
                    Bound(landBounds);

                    if (options.ShowLand)
                    {
                        Color baseColor = TileColor(colors, x, y, w, t);
                        // light from the upper left: brighter when the land rises to the left / top
                        double light = 1.0 + ((zl - zr + zt - zb) * 0.035);
                        Color c = Shade(baseColor, light);
                        bool flat = zt == zr && zr == zb && zb == zl;
                        int landId = t.LandId;
                        int zMin = Math.Min(Math.Min(zt, zr), Math.Min(zb, zl));
                        drawables.Add(((x + y), (zMin * 4) - 2, order++, (g, o) =>
                        {
                            Point[] poly = land.Select(p => new Point(p.X + o.X, p.Y + o.Y)).ToArray();
                            Bitmap art = flat && options.LandArt ? LandArt(landId) : null;
                            if (art != null)
                            {
                                g.DrawImage(art, new Rectangle(poly[3].X, poly[0].Y, tile, tile));
                            }
                            else
                            {
                                using (var brush = new SolidBrush(c))
                                {
                                    g.FillPolygon(brush, poly);
                                }
                            }
                            if (options.Grid)
                            {
                                using (var pen = new Pen(Color.FromArgb(70, Color.White)))
                                {
                                    g.DrawPolygon(pen, poly);
                                }
                            }
                        }, landBounds));
                    }

                    if (!options.ShowStatics)
                    {
                        continue;
                    }
                    for (int i = 0; i < t.Statics.Count; i++)
                    {
                        EcStatic st = t.Statics[i];
                        if (st.Z > options.MaxZ)
                        {
                            continue;
                        }
                        EcItemSprite sprite = sprites.Get(st.ItemId, options.Legacy);
                        int left = cx - half;
                        int bottom = top + tile - (st.Z * zStep);
                        Rectangle rect = sprite.Image != null
                            ? new Rectangle(left + sprite.OffsetX, bottom + sprite.OffsetY, sprite.Image.Width, sprite.Image.Height)
                            : new Rectangle(left, bottom - tile, tile, tile);
                        Bound(rect);
                        int threshold = (sprite.Height > 0 ? 1 : 0) + (sprite.Background ? 0 : 1);
                        var placed = new EcPlacedStatic { X = area.X + x, Y = area.Y + y, Static = st, Sprite = sprite, Rect = rect };
                        result.Statics.Add(placed);
                        drawables.Add(((x + y), ((st.Z + threshold) * 4) + threshold, order++, (g, o) =>
                        {
                            if (sprite.Image != null)
                            {
                                g.DrawImageUnscaled(sprite.Image, rect.X + o.X, rect.Y + o.Y);
                            }
                            else
                            {
                                g.DrawRectangle(Pens.Red, rect.X + o.X, rect.Y + o.Y, rect.Width, rect.Height);
                            }
                        }, rect));
                    }
                }
            }

            if (minX == int.MaxValue)
            {
                return result;
            }

            const int margin = 8;
            int width = Math.Min(MaxImageSide, maxX - minX + (2 * margin));
            int height = Math.Min(MaxImageSide, maxY - minY + (2 * margin));
            var offset = new Point(margin - minX, margin - minY);
            var canvas = new Bitmap(Math.Max(1, width), Math.Max(1, height), PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(options.Background);
                g.SmoothingMode = SmoothingMode.None;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                foreach (var d in drawables.OrderBy(d => d.Depth).ThenBy(d => d.Priority).ThenBy(d => d.Order))
                {
                    d.Draw(g, offset);
                }
            }

            foreach (EcPlacedStatic p in result.Statics)
            {
                p.Rect.Offset(offset);
            }
            // draw order = hit test order (last drawn is on top)
            result.Statics = result.Statics.OrderBy(p => (p.X - area.X) + (p.Y - area.Y)).ThenBy(p => p.Static.Z).ToList();
            result.Image = canvas;
            result.Origin = offset;
            return result;
        }

        /// <summary>Topmost static under an image pixel (transparent pixels don't count).</summary>
        public static EcPlacedStatic HitTest(EcMapRenderResult result, Point p)
        {
            if (result?.Statics == null)
            {
                return null;
            }
            for (int i = result.Statics.Count - 1; i >= 0; i--)
            {
                EcPlacedStatic s = result.Statics[i];
                if (!s.Rect.Contains(p))
                {
                    continue;
                }
                if (s.Sprite.Image == null || s.Sprite.Image.GetPixel(p.X - s.Rect.X, p.Y - s.Rect.Y).A > 16)
                {
                    return s;
                }
            }
            return null;
        }

        /// <summary>Map tile under an image pixel (land at z 0 – good enough to pick a tile).</summary>
        public static Point TileAt(EcMapRenderResult result, Point p, EcFacet facet)
        {
            if (result == null || result.TileHalf == 0)
            {
                return new Point(-1, -1);
            }
            double px = p.X - result.Origin.X, py = p.Y - result.Origin.Y;
            // iso: px = (x - y) * half, py = (x + y) * half (+ half to the tile center) - z * step
            int half = result.TileHalf;
            for (int pass = 0; pass < 2; pass++)
            {
                double fx = ((px / half) + ((py - half) / half)) / 2.0;
                double fy = (((py - half) / half) - (px / half)) / 2.0;
                int tx = (int)Math.Round(fx), ty = (int)Math.Round(fy);
                int mx = result.Area.X + tx, my = result.Area.Y + ty;
                if (pass == 0 && mx >= 0 && my >= 0 && mx < facet.Width && my < facet.Height)
                {
                    // correct the height once
                    int z = facet.GetTile(mx, my)?.Z ?? 0;
                    py += z * result.ZStep;
                    continue;
                }
                return new Point(mx, my);
            }
            return new Point(-1, -1);
        }

        private static Color TileColor(byte[] colors, int x, int y, int width, EcMapTile tile)
        {
            if (colors != null)
            {
                int o = ((y * width) + x) * 4;
                if (o + 2 < colors.Length)
                {
                    return Color.FromArgb(colors[o + 2], colors[o + 1], colors[o]);
                }
            }
            int h = (int)((tile.LandId * 2654435761u) >> 8);
            return Color.FromArgb(60 + (h & 0x7F), 60 + ((h >> 7) & 0x7F), 60 + ((h >> 14) & 0x7F));
        }

        private static Color Shade(Color c, double f)
        {
            int Clamp(double v) => Math.Max(0, Math.Min(255, (int)v));
            return Color.FromArgb(Clamp(c.R * f), Clamp(c.G * f), Clamp(c.B * f));
        }

        private static readonly Dictionary<int, Bitmap> _landArt = new Dictionary<int, Bitmap>();

        /// <summary>Land tile of the classic client loaded in UoFiddler (null if there is none).</summary>
        private static Bitmap LandArt(int id)
        {
            lock (_landArt)
            {
                if (_landArt.TryGetValue(id, out Bitmap b))
                {
                    return b;
                }
                try
                {
                    Bitmap art = Ultima.Art.GetLand(id);
                    b = art != null ? new Bitmap(art) : null;
                }
                catch (Exception)
                {
                    b = null;
                }
                _landArt[id] = b;
                return b;
            }
        }
    }
}

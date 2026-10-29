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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using Ultima;

namespace UoFiddler.Controls.MultiMapImages
{
    /// <summary>Look of the generated image</summary>
    public enum MapImageStyle
    {
        /// <summary>Hand drawn ink map: coast lines, mountains, trees, houses, dotted roads</summary>
        InkMap,
        /// <summary>Radar colors of the map / colors of the image</summary>
        Colors,
        Grayscale,
        /// <summary>Dithered black and white (like the MultiMap.rle of the client)</summary>
        BlackWhite,
        /// <summary>Outlines only (edge detection)</summary>
        Sketch
    }

    public sealed class MapImageSettings
    {
        public MapImageStyle Style { get; set; } = MapImageStyle.InkMap;
        public int OutputWidth { get; set; } = 2560;
        public int OutputHeight { get; set; } = 2048;
        /// <summary>True: height follows the aspect ratio of the source</summary>
        public bool KeepAspect { get; set; } = true;
        public bool Statics { get; set; } = true;
        /// <summary>Size factor of symbols, coast hairs and ripples (1 = normal)</summary>
        public float SymbolScale { get; set; } = 1f;
        public int Seed { get; set; } = 1;
        public bool Parchment { get; set; }
        public bool CoastRipples { get; set; } = true;
        public bool Frame { get; set; }
        /// <summary>Grid every n tiles (map) or pixels (image). 0 = no grid</summary>
        public int GridSize { get; set; }
        /// <summary>Area of the map in tiles. Empty = whole map</summary>
        public Rectangle Area { get; set; } = Rectangle.Empty;
        /// <summary>Ink density 0.25..2 (dots, hatching)</summary>
        public float Density { get; set; } = 1f;
    }

    /// <summary>
    /// Creates images of maps (built-in, registered or external) or of image files in different styles.
    /// </summary>
    public static class MapImageRenderer
    {
        private const long MaxPixels = 64L * 1024 * 1024;

        #region Terrain classes

        private enum T : byte
        {
            Plain = 0,
            Water,
            Sand,
            Forest,
            Mountain,
            Swamp,
            Snow,
            Lava,
            Road,
            Dirt,
            Cave,
            Count
        }

        // overlay of statics at a pixel
        private const byte OverNone = 0;
        private const byte OverTree = 1;
        private const byte OverBuilding = 2;
        private const byte OverRock = 3;

        private sealed class TerrainGrid
        {
            public int Width;
            public int Height;
            public byte[] Class;
            public byte[] Overlay;
            public sbyte[] Z;
            /// <summary>tiles per pixel (1 / scale) - 0 for images</summary>
            public double TilesPerPixel;
            public Rectangle Area;
        }

        private static byte[] _landClass;
        private static byte[] _itemClass;
        private static readonly object _tableLock = new object();

        private static void BuildTables()
        {
            lock (_tableLock)
            {
                if (_landClass != null)
                {
                    return;
                }

                var land = new byte[0x4000];
                LandData[] landTable = TileData.LandTable;
                for (int i = 0; i < land.Length; i++)
                {
                    if (landTable == null || i >= landTable.Length)
                    {
                        land[i] = (byte)T.Plain;
                        continue;
                    }

                    LandData d = landTable[i];
                    land[i] = (byte)ClassifyLand(d.Name, d.Flags);
                }

                ItemData[] itemTable = TileData.ItemTable;
                int itemCount = itemTable?.Length ?? 0;
                var items = new byte[Math.Max(itemCount, 1)];
                for (int i = 0; i < itemCount; i++)
                {
                    ItemData d = itemTable[i];
                    items[i] = ClassifyItem(d.Name, d.Flags);
                }

                _itemClass = items;
                _landClass = land;
            }
        }

        /// <summary>The tile data may change (other client folder) - classes are built again</summary>
        public static void ResetTables()
        {
            lock (_tableLock)
            {
                _landClass = null;
                _itemClass = null;
            }
        }

        private static T ClassifyLand(string name, TileFlag flags)
        {
            string n = (name ?? string.Empty).ToLowerInvariant();
            if ((flags & TileFlag.Wet) != 0 || n.Contains("water") || n.Contains("void") || n.Contains("nodraw"))
            {
                return T.Water;
            }

            if (n.Contains("lava"))
            {
                return T.Lava;
            }

            if (n.Contains("swamp") || n.Contains("marsh") || n.Contains("bog"))
            {
                return T.Swamp;
            }

            if (n.Contains("snow") || n.Contains("ice") || n.Contains("frozen"))
            {
                return T.Snow;
            }

            if (n.Contains("sand") || n.Contains("desert") || n.Contains("beach") || n.Contains("dune"))
            {
                return T.Sand;
            }

            if (n.Contains("cobble") || n.Contains("brick") || n.Contains("paved") || n.Contains("flagstone") ||
                n.Contains("road") || n.Contains("street") || n.Contains("marble") || n.Contains("tile"))
            {
                return T.Road;
            }

            if (n.Contains("mountain") || n.Contains("rock") || n.Contains("cliff") || n.Contains("stone") ||
                n.Contains("hill") || n.Contains("crag"))
            {
                return T.Mountain;
            }

            if (n.Contains("cave"))
            {
                return T.Cave;
            }

            if (n.Contains("forest") || n.Contains("jungle") || n.Contains("leaves") || n.Contains("tree"))
            {
                return T.Forest;
            }

            if (n.Contains("dirt") || n.Contains("mud") || n.Contains("furrow") || n.Contains("earth") ||
                n.Contains("wood") || n.Contains("floor"))
            {
                return T.Dirt;
            }

            return T.Plain;
        }

        private static byte ClassifyItem(string name, TileFlag flags)
        {
            string n = (name ?? string.Empty).ToLowerInvariant();
            if ((flags & (TileFlag.Wall | TileFlag.Roof | TileFlag.Door | TileFlag.Window)) != 0 &&
                !n.Contains("rock") && !n.Contains("cave") && !n.Contains("mountain"))
            {
                return OverBuilding;
            }

            if (n.Contains("tree") || n.Contains("trunk") || n.Contains("leaves") || n.Contains("cypress") ||
                n.Contains("palm") || n.Contains("yew") || n.Contains("cedar") || n.Contains("pine") || n.Contains("oak") ||
                n.Contains("willow") || n.Contains("bush") || n.Contains("shrub") || (flags & TileFlag.Foliage) != 0)
            {
                return OverTree;
            }

            if (n.Contains("rock") || n.Contains("boulder") || n.Contains("cave wall") || n.Contains("mountain"))
            {
                return OverRock;
            }

            if ((flags & TileFlag.Wet) != 0 && n.Contains("water"))
            {
                return 4; // water static
            }

            return OverNone;
        }

        #endregion

        #region Public API

        /// <summary>Output size for a source of the given size</summary>
        public static Size GetOutputSize(MapImageSettings s, int sourceWidth, int sourceHeight)
        {
            int w = Math.Max(16, s.OutputWidth);
            int h = s.KeepAspect
                ? Math.Max(16, (int)Math.Round((double)w * sourceHeight / Math.Max(1, sourceWidth)))
                : Math.Max(16, s.OutputHeight);

            if ((long)w * h > MaxPixels)
            {
                double f = Math.Sqrt((double)MaxPixels / ((long)w * h));
                w = (int)(w * f);
                h = (int)(h * f);
            }

            return new Size(w, h);
        }

        /// <summary>Creates an image of a map</summary>
        public static Bitmap RenderMap(Map map, MapImageSettings s, IProgress<int> progress, CancellationToken token)
        {
            Rectangle area = s.Area;
            if (area.IsEmpty || area.Width <= 0 || area.Height <= 0)
            {
                area = new Rectangle(0, 0, map.Width, map.Height);
            }

            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            if (area.Width < 8 || area.Height < 8)
            {
                throw new ArgumentException("The area is too small.");
            }

            Size size = GetOutputSize(s, area.Width, area.Height);
            Bitmap result;
            if (s.Style == MapImageStyle.InkMap)
            {
                TerrainGrid grid = BuildGridFromMap(map, area, size, s, progress, token);
                result = RenderInk(grid, s, progress, token);
            }
            else
            {
                using (Bitmap colors = RenderMapColors(map, area, size, s.Statics, progress, token))
                {
                    result = ApplyStyle(colors, s, progress, token);
                }
            }

            if (s.GridSize > 0)
            {
                DrawGrid(result, s, area, area.Width / (double)size.Width);
            }

            if (s.Frame)
            {
                DrawFrame(result, s);
            }

            progress?.Report(100);
            return result;
        }

        /// <summary>Creates an image of an image file / bitmap</summary>
        public static Bitmap RenderImage(Bitmap source, MapImageSettings s, IProgress<int> progress, CancellationToken token)
        {
            Size size = GetOutputSize(s, source.Width, source.Height);
            Bitmap result;
            using (Bitmap scaled = Scale(source, size))
            {
                progress?.Report(10);
                if (s.Style == MapImageStyle.InkMap)
                {
                    TerrainGrid grid = BuildGridFromColors(scaled, token);
                    progress?.Report(30);
                    result = RenderInk(grid, s, progress, token);
                }
                else
                {
                    result = ApplyStyle(scaled, s, progress, token);
                }
            }

            if (s.GridSize > 0)
            {
                DrawGrid(result, s, new Rectangle(0, 0, source.Width, source.Height), source.Width / (double)size.Width);
            }

            if (s.Frame)
            {
                DrawFrame(result, s);
            }

            progress?.Report(100);
            return result;
        }

        #endregion

        #region Sources

        private static TerrainGrid BuildGridFromMap(Map map, Rectangle area, Size size, MapImageSettings s,
            IProgress<int> progress, CancellationToken token)
        {
            BuildTables();
            byte[] landClass = _landClass;
            byte[] itemClass = _itemClass;

            int w = size.Width, h = size.Height;
            var grid = new TerrainGrid
            {
                Width = w,
                Height = h,
                Class = new byte[w * h],
                Overlay = new byte[w * h],
                Z = new sbyte[w * h],
                TilesPerPixel = area.Width / (double)w,
                Area = area
            };

            TileMatrix tiles = map.Tiles;
            double sx = area.Width / (double)w;
            double sy = area.Height / (double)h;
            try
            {
                for (int py = 0; py < h; py++)
                {
                    if ((py & 15) == 0)
                    {
                        token.ThrowIfCancellationRequested();
                        progress?.Report(py * 45 / h);
                    }

                    int ty = area.Y + Math.Min(area.Height - 1, (int)((py + 0.5) * sy));
                    int row = py * w;
                    for (int px = 0; px < w; px++)
                    {
                        int tx = area.X + Math.Min(area.Width - 1, (int)((px + 0.5) * sx));
                        Tile land = tiles.GetLandTile(tx, ty);
                        int id = land.Id & 0x3FFF;
                        byte cls = landClass[id];
                        sbyte z = land.Z;
                        byte over = OverNone;

                        if (s.Statics)
                        {
                            HuedTile[] statics = tiles.GetStaticTiles(tx, ty);
                            if (statics != null)
                            {
                                for (int i = 0; i < statics.Length; i++)
                                {
                                    int sid = statics[i].Id;
                                    if (sid >= itemClass.Length)
                                    {
                                        continue;
                                    }

                                    byte ic = itemClass[sid];
                                    if (ic == 4)
                                    {
                                        cls = (byte)T.Water;
                                    }
                                    else if (ic == OverBuilding)
                                    {
                                        over = OverBuilding;
                                    }
                                    else if (ic != OverNone && over == OverNone)
                                    {
                                        over = ic;
                                    }
                                }
                            }
                        }

                        grid.Class[row + px] = cls;
                        grid.Overlay[row + px] = over;
                        grid.Z[row + px] = z;
                    }
                }
            }
            finally
            {
                tiles.CloseStreams();
            }

            return grid;
        }

        private static TerrainGrid BuildGridFromColors(Bitmap bmp, CancellationToken token)
        {
            int w = bmp.Width, h = bmp.Height;
            var grid = new TerrainGrid
            {
                Width = w,
                Height = h,
                Class = new byte[w * h],
                Overlay = new byte[w * h],
                Z = new sbyte[w * h]
            };

            int[] px = GetPixels(bmp);
            for (int y = 0; y < h; y++)
            {
                if ((y & 31) == 0)
                {
                    token.ThrowIfCancellationRequested();
                }

                for (int x = 0; x < w; x++)
                {
                    int c = px[y * w + x];
                    int r = (c >> 16) & 0xFF, g = (c >> 8) & 0xFF, b = c & 0xFF;
                    T cls = ClassifyColor(r, g, b, out byte over);
                    grid.Class[y * w + x] = (byte)cls;
                    grid.Overlay[y * w + x] = over;
                    // brightness as pseudo height for hill hatching
                    grid.Z[y * w + x] = (sbyte)(((r * 3 + g * 6 + b) / 10) / 4 - 32);
                }
            }

            return grid;
        }

        private static T ClassifyColor(int r, int g, int b, out byte over)
        {
            over = OverNone;
            int max = Math.Max(r, Math.Max(g, b));
            int min = Math.Min(r, Math.Min(g, b));
            float v = max / 255f;
            float sat = max == 0 ? 0 : (max - min) / (float)max;
            float hue = 0;
            if (max != min)
            {
                if (max == r)
                {
                    hue = 60f * ((g - b) / (float)(max - min));
                }
                else if (max == g)
                {
                    hue = 60f * (2 + (b - r) / (float)(max - min));
                }
                else
                {
                    hue = 60f * (4 + (r - g) / (float)(max - min));
                }

                if (hue < 0)
                {
                    hue += 360;
                }
            }

            if (v < 0.06f)
            {
                return T.Water; // black = void / outside
            }

            if (sat > 0.25f && hue >= 175 && hue <= 260)
            {
                return T.Water;
            }

            if (sat > 0.55f && v > 0.5f && (hue < 15 || hue > 340))
            {
                return T.Lava;
            }

            if (v > 0.88f && sat < 0.12f)
            {
                return T.Snow;
            }

            if (sat < 0.16f)
            {
                return v < 0.75f ? T.Mountain : T.Road;
            }

            if (hue >= 30 && hue <= 65 && v > 0.55f)
            {
                return T.Sand;
            }

            if (hue >= 65 && hue <= 170)
            {
                if (v < 0.42f)
                {
                    over = OverTree;
                    return T.Forest;
                }

                if (sat < 0.35f && v < 0.55f)
                {
                    return T.Swamp;
                }

                return T.Plain;
            }

            if (hue >= 10 && hue < 45)
            {
                return v < 0.45f ? T.Mountain : T.Dirt;
            }

            return T.Plain;
        }

        /// <summary>Radar colors of the area, scaled to the output size</summary>
        private static Bitmap RenderMapColors(Map map, Rectangle area, Size size, bool statics, IProgress<int> progress,
            CancellationToken token)
        {
            // GetImage works with 8x8 blocks
            int bx = area.X >> 3, by = area.Y >> 3;
            int bw = Math.Max(1, (area.Right + 7 >> 3) - bx);
            int bh = Math.Max(1, (area.Bottom + 7 >> 3) - by);
            bw = Math.Min(bw, (map.Width >> 3) - bx);
            bh = Math.Min(bh, (map.Height >> 3) - by);

            var result = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(Color.Black);
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                // render in strips, so huge maps don't need one giant bitmap
                const int stripBlocks = 64;
                double scaleX = size.Width / (double)area.Width;
                double scaleY = size.Height / (double)area.Height;
                for (int sy = 0; sy < bh; sy += stripBlocks)
                {
                    token.ThrowIfCancellationRequested();
                    int h = Math.Min(stripBlocks, bh - sy);
                    using (Bitmap strip = map.GetImage(bx, by + sy, bw, h, statics))
                    {
                        float destX = (float)(((bx << 3) - area.X) * scaleX);
                        float destY = (float)((((by + sy) << 3) - area.Y) * scaleY);
                        float destW = (float)((bw << 3) * scaleX);
                        float destH = (float)((h << 3) * scaleY);
                        g.DrawImage(strip, new RectangleF(destX, destY, destW, destH),
                            new RectangleF(0, 0, strip.Width, strip.Height), GraphicsUnit.Pixel);
                    }

                    progress?.Report((sy + h) * 60 / bh);
                }
            }

            return result;
        }

        private static Bitmap Scale(Bitmap source, Size size)
        {
            var result = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(Color.White);
                g.InterpolationMode = size.Width < source.Width ? InterpolationMode.HighQualityBicubic : InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(source, new Rectangle(0, 0, size.Width, size.Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel);
            }

            return result;
        }

        #endregion

        #region Pixel helpers

        private static int[] GetPixels(Bitmap bmp)
        {
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                var px = new int[bmp.Width * bmp.Height];
                for (int y = 0; y < bmp.Height; y++)
                {
                    Marshal.Copy(data.Scan0 + y * data.Stride, px, y * bmp.Width, bmp.Width);
                }

                return px;
            }
            finally
            {
                bmp.UnlockBits(data);
            }
        }

        private static Bitmap FromPixels(int[] px, int w, int h)
        {
            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            var data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < h; y++)
                {
                    Marshal.Copy(px, y * w, data.Scan0 + y * data.Stride, w);
                }
            }
            finally
            {
                bmp.UnlockBits(data);
            }

            return bmp;
        }

        private static uint Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + seed * 144269527);
                h = (h ^ (h >> 13)) * 1274126177u;
                return h ^ (h >> 16);
            }
        }

        private static double Rand(int x, int y, int seed)
        {
            return (Hash(x, y, seed) & 0xFFFFFF) / 16777216.0;
        }

        private static Color PaperColor(MapImageSettings s) => s.Parchment ? Color.FromArgb(241, 228, 195) : Color.White;

        private static Color InkColor(MapImageSettings s) => s.Parchment ? Color.FromArgb(48, 34, 22) : Color.Black;

        #endregion

        #region Ink map

        private static Bitmap RenderInk(TerrainGrid grid, MapImageSettings s, IProgress<int> progress, CancellationToken token)
        {
            int w = grid.Width, h = grid.Height;
            int seed = s.Seed;
            float sc = Math.Max(0.3f, s.SymbolScale);
            float density = Math.Max(0.1f, s.Density);

            byte[] cls = ModeFilter(grid.Class, w, h);
            token.ThrowIfCancellationRequested();
            progress?.Report(50);

            // distance of water pixels to the coast (chamfer 3/4)
            int[] dist = WaterDistance(cls, w, h, (int)(30 * sc) * 3);
            token.ThrowIfCancellationRequested();
            progress?.Report(55);

            var ink = new byte[w * h];
            double tilesPerPixel = grid.TilesPerPixel > 0 ? grid.TilesPerPixel : 1;
            int rippleBand = Math.Max(3, (int)Math.Round(4 * sc)) * 3;
            int rippleBand2 = Math.Max(5, (int)Math.Round(8 * sc)) * 3;

            for (int y = 0; y < h; y++)
            {
                if ((y & 31) == 0)
                {
                    token.ThrowIfCancellationRequested();
                }

                int row = y * w;
                for (int x = 0; x < w; x++)
                {
                    int i = row + x;
                    var c = (T)cls[i];
                    double r = Rand(x, y, seed);

                    if (c == T.Water)
                    {
                        int d = dist[i];
                        if (d <= 3)
                        {
                            ink[i] = 1; // coast line
                        }
                        else if (s.CoastRipples && d >= rippleBand && d < rippleBand + 3 && r < 0.75)
                        {
                            ink[i] = 1;
                        }
                        else if (s.CoastRipples && d >= rippleBand2 && d < rippleBand2 + 3 && ((x + y) % 3) == 0)
                        {
                            ink[i] = 1;
                        }

                        continue;
                    }

                    // borders between regions
                    if (x + 1 < w && IsRegionBorder(c, (T)cls[i + 1]) && r < 0.8 ||
                        y + 1 < h && IsRegionBorder(c, (T)cls[i + w]) && r < 0.8)
                    {
                        ink[i] = 1;
                        continue;
                    }

                    switch (c)
                    {
                        case T.Sand:
                            if (r < 0.09 * density)
                            {
                                ink[i] = 1;
                            }

                            break;
                        case T.Dirt:
                            if (r < 0.025 * density)
                            {
                                ink[i] = 1;
                            }

                            break;
                        case T.Cave:
                            if (r < 0.06 * density)
                            {
                                ink[i] = 1;
                            }

                            break;
                        case T.Snow:
                            if (r < 0.01 * density)
                            {
                                ink[i] = 1;
                            }

                            break;
                        case T.Plain:
                            if (r < 0.004 * density)
                            {
                                ink[i] = 1;
                            }

                            break;
                        case T.Swamp:
                        {
                            int step = Math.Max(3, (int)(4 * sc));
                            if (y % step == 0 && ((x + (y / step) * 3) % (step + 2)) < step - 1 && r < 0.8)
                            {
                                ink[i] = 1;
                            }

                            break;
                        }
                        case T.Lava:
                            if ((x + y) % 3 == 0 || (x - y + 3000) % 6 == 0)
                            {
                                ink[i] = 1;
                            }

                            break;
                        case T.Road:
                        {
                            // dotted path: only the border pixels of road areas, every 2nd pixel
                            bool edge = x == 0 || y == 0 || x == w - 1 || y == h - 1 ||
                                        cls[i - 1] != (byte)T.Road || cls[i + 1] != (byte)T.Road ||
                                        cls[i - w] != (byte)T.Road || cls[i + w] != (byte)T.Road;
                            if (edge && ((x + y) & 1) == 0)
                            {
                                ink[i] = 1;
                            }

                            break;
                        }
                    }

                    // hill hatching from the height
                    if (c != T.Mountain && x > 0 && y > 0 && x < w - 1 && y < h - 1)
                    {
                        int dz = Math.Abs(grid.Z[i + 1] - grid.Z[i - 1]) + Math.Abs(grid.Z[i + w] - grid.Z[i - w]);
                        double slope = dz / (2.0 * tilesPerPixel);
                        if (slope > 3.5 && ((x - y + 4096) % Math.Max(3, (int)(4 * sc))) == 0 && r < 0.7 * density)
                        {
                            ink[i] = 1;
                        }
                    }
                }
            }

            progress?.Report(65);

            if (s.CoastRipples)
            {
                DrawCoastHairs(ink, cls, dist, w, h, seed, sc);
            }

            token.ThrowIfCancellationRequested();
            progress?.Report(70);

            // paper + ink into pixels
            Color paper = PaperColor(s);
            Color inkColor = InkColor(s);
            var px = new int[w * h];
            int inkArgb = inkColor.ToArgb();
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    if (ink[i] != 0)
                    {
                        px[i] = inkArgb;
                    }
                    else
                    {
                        px[i] = s.Parchment ? ParchmentPixel(x, y, w, h, paper, seed) : paper.ToArgb();
                    }
                }
            }

            Bitmap bmp = FromPixels(px, w, h);
            progress?.Report(75);

            DrawSymbols(bmp, grid, cls, s, token);
            progress?.Report(98);
            return bmp;
        }

        private static bool IsRegionBorder(T a, T b)
        {
            if (a == b || b == T.Water)
            {
                return false;
            }

            if (a == T.Road || b == T.Road || a == T.Mountain || b == T.Mountain)
            {
                return false;
            }

            if ((a == T.Plain && b == T.Dirt) || (a == T.Dirt && b == T.Plain))
            {
                return false;
            }

            return true;
        }

        private static int ParchmentPixel(int x, int y, int w, int h, Color paper, int seed)
        {
            // noise + darker edges
            double n = (Rand(x >> 2, y >> 2, seed + 77) - 0.5) * 14 + (Rand(x, y, seed + 78) - 0.5) * 6;
            double dx = (x - w / 2.0) / (w / 2.0), dy = (y - h / 2.0) / (h / 2.0);
            double v = Math.Max(0, Math.Sqrt(dx * dx + dy * dy) - 0.6) * 55;
            int r = Clamp((int)(paper.R + n - v)), g = Clamp((int)(paper.G + n - v * 1.1)), b = Clamp((int)(paper.B + n - v * 1.3));
            return (255 << 24) | (r << 16) | (g << 8) | b;
        }

        private static int Clamp(int v) => v < 0 ? 0 : v > 255 ? 255 : v;

        /// <summary>3x3 majority filter, removes single pixels</summary>
        private static byte[] ModeFilter(byte[] src, int w, int h)
        {
            var dst = new byte[src.Length];
            var counts = new int[(int)T.Count];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Array.Clear(counts, 0, counts.Length);
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int yy = y + dy;
                        if (yy < 0 || yy >= h)
                        {
                            continue;
                        }

                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int xx = x + dx;
                            if (xx < 0 || xx >= w)
                            {
                                continue;
                            }

                            byte c = src[yy * w + xx];
                            if (c < counts.Length)
                            {
                                counts[c]++;
                            }
                        }
                    }

                    byte own = src[y * w + x];
                    byte best = own;
                    int bestCount = own < counts.Length ? counts[own] : 0;
                    for (int c = 0; c < counts.Length; c++)
                    {
                        if (counts[c] > bestCount)
                        {
                            best = (byte)c;
                            bestCount = counts[c];
                        }
                    }

                    // keep roads thin: don't remove them
                    dst[y * w + x] = own == (byte)T.Road && counts[(int)T.Road] >= 2 ? own : best;
                }
            }

            return dst;
        }

        /// <summary>Chamfer distance (3 = one pixel) of water pixels to the next land pixel. Land = 0.</summary>
        private static int[] WaterDistance(byte[] cls, int w, int h, int max)
        {
            var d = new int[w * h];
            for (int i = 0; i < d.Length; i++)
            {
                d[i] = cls[i] == (byte)T.Water ? max : 0;
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    if (d[i] == 0)
                    {
                        continue;
                    }

                    int v = d[i];
                    if (x > 0)
                    {
                        v = Math.Min(v, d[i - 1] + 3);
                    }

                    if (y > 0)
                    {
                        v = Math.Min(v, d[i - w] + 3);
                        if (x > 0)
                        {
                            v = Math.Min(v, d[i - w - 1] + 4);
                        }

                        if (x < w - 1)
                        {
                            v = Math.Min(v, d[i - w + 1] + 4);
                        }
                    }

                    d[i] = v;
                }
            }

            for (int y = h - 1; y >= 0; y--)
            {
                for (int x = w - 1; x >= 0; x--)
                {
                    int i = y * w + x;
                    if (d[i] == 0)
                    {
                        continue;
                    }

                    int v = d[i];
                    if (x < w - 1)
                    {
                        v = Math.Min(v, d[i + 1] + 3);
                    }

                    if (y < h - 1)
                    {
                        v = Math.Min(v, d[i + w] + 3);
                        if (x < w - 1)
                        {
                            v = Math.Min(v, d[i + w + 1] + 4);
                        }

                        if (x > 0)
                        {
                            v = Math.Min(v, d[i + w - 1] + 4);
                        }
                    }

                    d[i] = v;
                }
            }

            return d;
        }

        /// <summary>Short strokes from the coast into the water (the "hairy" coast of drawn maps)</summary>
        private static void DrawCoastHairs(byte[] ink, byte[] cls, int[] dist, int w, int h, int seed, float sc)
        {
            int maxLen = Math.Max(2, (int)Math.Round(4 * sc));
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    int i = y * w + x;
                    if (cls[i] != (byte)T.Water || dist[i] > 3)
                    {
                        continue;
                    }

                    if (Rand(x, y, seed + 5) > 0.3)
                    {
                        continue;
                    }

                    // direction away from the land = gradient of the distance
                    double gx = dist[i + 1] - dist[i - 1];
                    double gy = dist[i + w] - dist[i - w];
                    double len = Math.Sqrt(gx * gx + gy * gy);
                    if (len < 0.5)
                    {
                        continue;
                    }

                    gx /= len;
                    gy /= len;
                    int l = 2 + (int)(Rand(x, y, seed + 6) * (maxLen - 1));
                    for (int k = 1; k <= l; k++)
                    {
                        int xx = x + (int)Math.Round(gx * k);
                        int yy = y + (int)Math.Round(gy * k);
                        if (xx < 0 || yy < 0 || xx >= w || yy >= h)
                        {
                            break;
                        }

                        int j = yy * w + xx;
                        if (cls[j] != (byte)T.Water)
                        {
                            break;
                        }

                        ink[j] = 1;
                    }
                }
            }
        }

        private struct Symbol
        {
            public MapSymbol Kind;
            public float X;
            public float Y;
            public float Size;
            public int Variant;
        }

        private static void DrawSymbols(Bitmap bmp, TerrainGrid grid, byte[] cls, MapImageSettings s, CancellationToken token)
        {
            int w = grid.Width, h = grid.Height;
            int seed = s.Seed;
            float sc = Math.Max(0.3f, s.SymbolScale);
            int cell = Math.Max(4, (int)Math.Round(9 * sc));
            var symbols = new List<Symbol>();
            var counts = new int[(int)T.Count];

            for (int cy = 0; cy < h; cy += cell)
            {
                token.ThrowIfCancellationRequested();
                for (int cx = 0; cx < w; cx += cell)
                {
                    Array.Clear(counts, 0, counts.Length);
                    int trees = 0, buildings = 0, rocks = 0, total = 0;
                    int ex = Math.Min(w, cx + cell), ey = Math.Min(h, cy + cell);
                    for (int y = cy; y < ey; y++)
                    {
                        for (int x = cx; x < ex; x++)
                        {
                            int i = y * w + x;
                            counts[cls[i]]++;
                            total++;
                            switch (grid.Overlay[i])
                            {
                                case OverTree:
                                    trees++;
                                    break;
                                case OverBuilding:
                                    buildings++;
                                    break;
                                case OverRock:
                                    rocks++;
                                    break;
                            }
                        }
                    }

                    if (total == 0 || counts[(int)T.Water] > total / 3)
                    {
                        continue;
                    }

                    double r = Rand(cx, cy, seed + 11);
                    float jx = (float)((Rand(cx, cy, seed + 12) - 0.5) * cell * 0.6);
                    float jy = (float)((Rand(cx, cy, seed + 13) - 0.5) * cell * 0.6);
                    float x0 = cx + cell / 2f + jx, y0 = cy + cell / 2f + jy;
                    int variant = (int)(Hash(cx, cy, seed + 14) & 0xFF);

                    double fb = buildings / (double)total;
                    double ft = trees / (double)total;
                    double fm = counts[(int)T.Mountain] / (double)total;
                    double ff = counts[(int)T.Forest] / (double)total;
                    bool snowy = counts[(int)T.Snow] > total / 3;

                    if (fb >= 0.2)
                    {
                        symbols.Add(new Symbol { Kind = MapSymbol.House, X = cx + cell / 2f, Y = cy + cell * 0.8f, Size = cell * 0.95f, Variant = variant });
                    }
                    else if (fm >= 0.45)
                    {
                        if (r < 0.85)
                        {
                            symbols.Add(new Symbol { Kind = MapSymbol.Mountain, X = x0, Y = y0 + cell * 0.4f, Size = cell * (1.4f + (float)Rand(cx, cy, seed + 15) * 0.8f), Variant = variant });
                        }
                    }
                    else if (ft >= 0.18 || ff >= 0.5)
                    {
                        if (r < 0.9)
                        {
                            symbols.Add(new Symbol
                            {
                                Kind = snowy || fm > 0.15 ? MapSymbol.Conifer : MapSymbol.Tree,
                                X = x0,
                                Y = y0 + cell * 0.35f,
                                Size = cell * (0.85f + (float)Rand(cx, cy, seed + 16) * 0.35f),
                                Variant = variant
                            });
                        }
                    }
                    else if (ft >= 0.04 && r < 0.5)
                    {
                        symbols.Add(new Symbol { Kind = MapSymbol.Bush, X = x0, Y = y0 + cell * 0.3f, Size = cell * 0.6f, Variant = variant });
                    }
                    else if (rocks > total / 5 && r < 0.5)
                    {
                        symbols.Add(new Symbol { Kind = MapSymbol.Rock, X = x0, Y = y0 + cell * 0.3f, Size = cell * 0.6f, Variant = variant });
                    }
                    else if ((counts[(int)T.Plain] > total / 2 || counts[(int)T.Swamp] > total / 2) && r < 0.06 * s.Density)
                    {
                        symbols.Add(new Symbol { Kind = MapSymbol.Tuft, X = x0, Y = y0 + cell * 0.25f, Size = cell * 0.5f, Variant = variant });
                    }
                }
            }

            // back to front
            symbols.Sort((a, b) => a.Y.CompareTo(b.Y));

            using (Graphics g = Graphics.FromImage(bmp))
            using (var painter = new MapSymbolPainter(InkColor(s), PaperColor(s), Math.Max(1f, 0.75f * sc)))
            {
                g.SmoothingMode = s.Parchment ? SmoothingMode.AntiAlias : SmoothingMode.None;
                foreach (Symbol sym in symbols)
                {
                    painter.Draw(g, sym.Kind, sym.X, sym.Y, sym.Size, sym.Variant);
                }
            }
        }

        #endregion

        #region Other styles

        private static Bitmap ApplyStyle(Bitmap colors, MapImageSettings s, IProgress<int> progress, CancellationToken token)
        {
            int w = colors.Width, h = colors.Height;
            switch (s.Style)
            {
                case MapImageStyle.Colors:
                    return new Bitmap(colors);
                case MapImageStyle.Grayscale:
                {
                    int[] px = GetPixels(colors);
                    for (int i = 0; i < px.Length; i++)
                    {
                        int l = Luma(px[i]);
                        px[i] = (255 << 24) | (l << 16) | (l << 8) | l;
                    }

                    return FromPixels(px, w, h);
                }
                case MapImageStyle.BlackWhite:
                    return Dither(colors, s, token);
                case MapImageStyle.Sketch:
                    return Sketch(colors, s, token);
                default:
                    return new Bitmap(colors);
            }
        }

        private static int Luma(int c)
        {
            int r = (c >> 16) & 0xFF, g = (c >> 8) & 0xFF, b = c & 0xFF;
            return (r * 299 + g * 587 + b * 114) / 1000;
        }

        /// <summary>Floyd-Steinberg dithering to pure black and white</summary>
        private static Bitmap Dither(Bitmap colors, MapImageSettings s, CancellationToken token)
        {
            int w = colors.Width, h = colors.Height;
            int[] px = GetPixels(colors);
            var lum = new float[w * h];
            for (int i = 0; i < px.Length; i++)
            {
                lum[i] = Luma(px[i]);
            }

            int ink = InkColor(s).ToArgb(), paper = PaperColor(s).ToArgb();
            for (int y = 0; y < h; y++)
            {
                if ((y & 31) == 0)
                {
                    token.ThrowIfCancellationRequested();
                }

                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    float old = lum[i];
                    float neu = old < 128 ? 0 : 255;
                    px[i] = neu == 0 ? ink : paper;
                    float err = old - neu;
                    if (x + 1 < w)
                    {
                        lum[i + 1] += err * 7 / 16f;
                    }

                    if (y + 1 < h)
                    {
                        if (x > 0)
                        {
                            lum[i + w - 1] += err * 3 / 16f;
                        }

                        lum[i + w] += err * 5 / 16f;
                        if (x + 1 < w)
                        {
                            lum[i + w + 1] += err * 1 / 16f;
                        }
                    }
                }
            }

            return FromPixels(px, w, h);
        }

        /// <summary>Outlines by Sobel edge detection</summary>
        private static Bitmap Sketch(Bitmap colors, MapImageSettings s, CancellationToken token)
        {
            int w = colors.Width, h = colors.Height;
            int[] src = GetPixels(colors);
            var lum = new int[w * h];
            for (int i = 0; i < src.Length; i++)
            {
                lum[i] = Luma(src[i]);
            }

            int ink = InkColor(s).ToArgb();
            Color paperColor = PaperColor(s);
            var px = new int[w * h];
            double threshold = 90 / Math.Max(0.25, s.Density);
            for (int y = 0; y < h; y++)
            {
                if ((y & 31) == 0)
                {
                    token.ThrowIfCancellationRequested();
                }

                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    double mag = 0;
                    if (x > 0 && y > 0 && x < w - 1 && y < h - 1)
                    {
                        int gx = -lum[i - w - 1] - 2 * lum[i - 1] - lum[i + w - 1] + lum[i - w + 1] + 2 * lum[i + 1] + lum[i + w + 1];
                        int gy = -lum[i - w - 1] - 2 * lum[i - w] - lum[i - w + 1] + lum[i + w - 1] + 2 * lum[i + w] + lum[i + w + 1];
                        mag = Math.Sqrt(gx * gx + gy * gy);
                    }

                    px[i] = mag > threshold
                        ? ink
                        : s.Parchment ? ParchmentPixel(x, y, w, h, paperColor, s.Seed) : paperColor.ToArgb();
                }
            }

            return FromPixels(px, w, h);
        }

        #endregion

        #region Grid and frame

        private static void DrawGrid(Bitmap bmp, MapImageSettings s, Rectangle area, double unitsPerPixel)
        {
            if (unitsPerPixel <= 0)
            {
                return;
            }

            int step = s.GridSize;
            using (Graphics g = Graphics.FromImage(bmp))
            using (var pen = new Pen(Color.FromArgb(110, InkColor(s)), 1f) { DashStyle = DashStyle.Dot })
            using (var font = new Font("Arial", Math.Max(7f, 7f * s.SymbolScale)))
            using (var brush = new SolidBrush(InkColor(s)))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                int first = (area.X + step - 1) / step * step;
                for (int u = first; u < area.Right; u += step)
                {
                    float x = (float)((u - area.X) / unitsPerPixel);
                    g.DrawLine(pen, x, 0, x, bmp.Height);
                    g.DrawString(u.ToString(), font, brush, x + 2, 2);
                }

                first = (area.Y + step - 1) / step * step;
                for (int u = first; u < area.Bottom; u += step)
                {
                    float y = (float)((u - area.Y) / unitsPerPixel);
                    g.DrawLine(pen, 0, y, bmp.Width, y);
                    g.DrawString(u.ToString(), font, brush, 2, y + 2);
                }
            }
        }

        private static void DrawFrame(Bitmap bmp, MapImageSettings s)
        {
            using (Graphics g = Graphics.FromImage(bmp))
            using (var thick = new Pen(InkColor(s), Math.Max(2f, 3f * s.SymbolScale)))
            using (var thin = new Pen(InkColor(s), 1f))
            {
                float m = Math.Max(3f, 4f * s.SymbolScale);
                g.DrawRectangle(thick, m, m, bmp.Width - 2 * m - 1, bmp.Height - 2 * m - 1);
                float m2 = m + Math.Max(4f, 5f * s.SymbolScale);
                g.DrawRectangle(thin, m2, m2, bmp.Width - 2 * m2 - 1, bmp.Height - 2 * m2 - 1);
            }
        }

        #endregion
    }
}

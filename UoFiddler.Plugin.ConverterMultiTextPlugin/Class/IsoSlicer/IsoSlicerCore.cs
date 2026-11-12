using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextureCutter;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.IsoSlicer
{
    internal enum SliceMode
    {
        /// <summary>Plain rectangles, row by row (old "Straight view").</summary>
        Straight,

        /// <summary>UO land tile diamonds, every second row shifted by half a tile (old "Grid view").</summary>
        IsoLand
    }

    internal enum OutsideFill { Black, Transparent, Color }

    internal sealed class SliceSettings
    {
        public SliceMode Mode = SliceMode.IsoLand;
        public int TileWidth = 44;
        public int TileHeight = 44;
        public int GridX;          // shifts the whole grid
        public int GridY;
        public OutsideFill Outside = OutsideFill.Black;
        public Color FillColor = Color.Black;
        public bool SkipEmpty = true;
    }

    /// <summary>One piece of the grid.</summary>
    internal sealed class SliceCell
    {
        public int Index;        // running number (before skipping empty ones)
        public int Column;
        public int Row;
        public Rectangle Bounds; // in image pixels (can reach outside the image)
        public Point Map;        // map position relative to the first tile (iso: UO map x / y, straight: column / row)
        public bool Empty;
        public Bitmap Image;     // the cut tile
        public int FileNumber = -1;
    }

    /// <summary>Cuts an image into rectangles or UO land tile diamonds and writes the results.</summary>
    internal static class IsoSlicerCore
    {
        // ------------------------------------------------------------------ geometry

        /// <summary>
        /// Half width of the diamond in row y. For 44 × 44: rows 0 … 21 are 2 … 44 pixels wide, rows 22 … 43 44 … 2 –
        /// exactly the 1012 pixels of a UO land tile.
        /// </summary>
        public static int DiamondHalfWidth(int y, int w, int h)
        {
            int half = h / 2;
            int k = Math.Min(y, h - 1 - y) + 1;
            return Math.Max(1, (int)Math.Round(k * (w / 2.0) / Math.Max(1, half)));
        }

        public static bool InDiamond(int x, int y, int w, int h)
        {
            int hw = DiamondHalfWidth(y, w, h);
            return x >= w / 2 - hw && x < w / 2 + hw;
        }

        /// <summary>All cells that touch the image.</summary>
        public static List<SliceCell> Layout(Size image, SliceSettings s)
        {
            var cells = new List<SliceCell>();
            int w = Math.Max(2, s.TileWidth), h = Math.Max(2, s.TileHeight);
            if (s.Mode == SliceMode.Straight)
            {
                int x0 = Mod(s.GridX, w) - (Mod(s.GridX, w) > 0 ? w : 0);
                int y0 = Mod(s.GridY, h) - (Mod(s.GridY, h) > 0 ? h : 0);
                int row = 0;
                for (int y = y0; y < image.Height; y += h, row++)
                {
                    int col = 0;
                    for (int x = x0; x < image.Width; x += w, col++)
                    {
                        cells.Add(new SliceCell { Index = cells.Count, Column = col, Row = row, Bounds = new Rectangle(x, y, w, h), Map = new Point(col, row) });
                    }
                }
                return cells;
            }

            // Iso: the diamond centers lie on a grid of w × h/2, every second row shifted by w/2.
            int hy = h / 2;
            int ox = Mod(s.GridX, w), oy = Mod(s.GridY, hy);
            int firstRow = -1 - (oy > 0 ? 1 : 0);
            int rows = (image.Height + h) / hy + 2;
            for (int r = firstRow, row = 0; r < firstRow + rows; r++, row++)
            {
                int cy = r * hy + oy;                      // center y
                bool odd = Mod(r, 2) == 1;
                for (int c = -1, col = 0; ; c++, col++)
                {
                    int cx = c * w + ox + (odd ? w / 2 : 0); // center x
                    var bounds = new Rectangle(cx - w / 2, cy - hy, w, h);
                    if (bounds.Left >= image.Width)
                    {
                        break;
                    }
                    if (bounds.Right <= 0 || bounds.Bottom <= 0 || bounds.Top >= image.Height)
                    {
                        continue;
                    }
                    // screen (cx, cy) = ((mx - my) * w/2, (mx + my) * h/2) → map position
                    int sx = 2 * c + (odd ? 1 : 0), sy = r;  // mx - my, mx + my
                    cells.Add(new SliceCell { Index = cells.Count, Column = col, Row = row, Bounds = bounds, Map = new Point((sx + sy) / 2, (sy - sx) / 2) });
                }
            }
            // map positions relative to the smallest one
            if (cells.Count > 0)
            {
                int mx = cells.Min(c => c.Map.X), my = cells.Min(c => c.Map.Y);
                foreach (SliceCell c in cells)
                {
                    c.Map = new Point(c.Map.X - mx, c.Map.Y - my);
                }
            }
            return cells;
        }

        private static int Mod(int a, int m) => ((a % m) + m) % m;

        /// <summary>Outline of a cell in image coordinates (rectangle or diamond).</summary>
        public static PointF[] Outline(SliceCell c, SliceMode mode)
        {
            RectangleF b = c.Bounds;
            if (mode == SliceMode.Straight)
            {
                return new[] { new PointF(b.Left, b.Top), new PointF(b.Right, b.Top), new PointF(b.Right, b.Bottom), new PointF(b.Left, b.Bottom) };
            }
            float cx = b.Left + b.Width / 2f, cy = b.Top + b.Height / 2f;
            return new[] { new PointF(cx, b.Top), new PointF(b.Right, cy), new PointF(cx, b.Bottom), new PointF(b.Left, cy) };
        }

        public static SliceCell HitTest(List<SliceCell> cells, SliceMode mode, Point p)
        {
            foreach (SliceCell c in cells)
            {
                if (!c.Bounds.Contains(p))
                {
                    continue;
                }
                if (mode == SliceMode.Straight || InDiamond(p.X - c.Bounds.X, p.Y - c.Bounds.Y, c.Bounds.Width, c.Bounds.Height))
                {
                    return c;
                }
            }
            return null;
        }

        // ------------------------------------------------------------------ cutting

        /// <summary>Cuts every cell (fast, on pixel arrays). Cells that only hold outside / transparent / fill pixels are marked Empty.</summary>
        public static void Cut(Bitmap source, List<SliceCell> cells, SliceSettings s)
        {
            int iw = source.Width, ih = source.Height;
            int[] src = CutterImaging.GetPixels(source);
            int fill = s.Outside == OutsideFill.Transparent ? 0 : s.Outside == OutsideFill.Color ? s.FillColor.ToArgb() : unchecked((int)0xFF000000);
            foreach (SliceCell c in cells)
            {
                c.Image?.Dispose();
                int w = c.Bounds.Width, h = c.Bounds.Height;
                var px = new int[w * h];
                bool any = false;
                for (int y = 0; y < h; y++)
                {
                    int sy = c.Bounds.Y + y;
                    int hw = s.Mode == SliceMode.IsoLand ? DiamondHalfWidth(y, w, h) : w;
                    int xFrom = s.Mode == SliceMode.IsoLand ? w / 2 - hw : 0, xTo = s.Mode == SliceMode.IsoLand ? w / 2 + hw : w;
                    for (int x = 0; x < w; x++)
                    {
                        int i = y * w + x;
                        if (x < xFrom || x >= xTo)
                        {
                            // outside the diamond: black = transparent for UO land tiles
                            px[i] = s.Outside == OutsideFill.Transparent ? 0 : unchecked((int)0xFF000000);
                            continue;
                        }
                        int sx = c.Bounds.X + x;
                        if (sx < 0 || sy < 0 || sx >= iw || sy >= ih)
                        {
                            px[i] = fill;
                            continue;
                        }
                        int p = src[sy * iw + sx];
                        if (((p >> 24) & 0xFF) < 128)
                        {
                            px[i] = fill;
                            continue;
                        }
                        px[i] = p;
                        if ((p & 0xFFFFFF) != (fill & 0xFFFFFF) || s.Outside == OutsideFill.Transparent)
                        {
                            any = true;
                        }
                    }
                }
                var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                CutterImaging.SetPixels(bmp, px);
                c.Image = bmp;
                c.Empty = !any;
            }
        }

        /// <summary>Average brightness 0 … 1 of the visible pixels of a cell (for heights).</summary>
        public static double Brightness(SliceCell c)
        {
            if (c.Image == null)
            {
                return 0;
            }
            int[] px = CutterImaging.GetPixels(c.Image);
            long sum = 0;
            int n = 0;
            foreach (int p in px)
            {
                if (((p >> 24) & 0xFF) < 128 || (p & 0xFFFFFF) == 0)
                {
                    continue;
                }
                sum += ((p >> 16) & 0xFF) * 299 + ((p >> 8) & 0xFF) * 587 + (p & 0xFF) * 114;
                n++;
            }
            return n == 0 ? 0 : sum / (n * 255000.0);
        }

        // ------------------------------------------------------------------ views

        /// <summary>The cut tiles put back together; gap spreads them apart (exploded view) to show every cut.</summary>
        public static Bitmap Assemble(List<SliceCell> cells, SliceMode mode, int gap, bool outlines, bool numbers, bool hideEmpty)
        {
            List<SliceCell> use = cells.Where(c => c.Image != null && (!hideEmpty || !c.Empty)).ToList();
            if (use.Count == 0)
            {
                return null;
            }
            Point Pos(SliceCell c)
            {
                if (mode == SliceMode.Straight)
                {
                    return new Point(c.Bounds.X + c.Column * gap, c.Bounds.Y + c.Row * gap);
                }
                // iso: spread along the diamond grid
                return new Point(c.Bounds.X + (c.Map.X - c.Map.Y) * gap, c.Bounds.Y + (c.Map.X + c.Map.Y) * gap / 2);
            }
            int minX = use.Min(c => Pos(c).X), minY = use.Min(c => Pos(c).Y);
            int maxX = use.Max(c => Pos(c).X + c.Bounds.Width), maxY = use.Max(c => Pos(c).Y + c.Bounds.Height);
            var bmp = new Bitmap(maxX - minX + 2, maxY - minY + 2, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            using (var pen = new Pen(Color.FromArgb(150, 0, 255, 0)))
            using (var font = new Font("Segoe UI", 7f))
            {
                g.Clear(Color.Transparent);
                foreach (SliceCell c in use)
                {
                    Point p = Pos(c);
                    p.Offset(-minX, -minY);
                    if (mode == SliceMode.IsoLand)
                    {
                        // black outside the diamond is transparent in UO – draw only the diamond
                        using (var copy = MaskOutside(c.Image))
                        {
                            g.DrawImageUnscaled(copy, p);
                        }
                    }
                    else
                    {
                        g.DrawImageUnscaled(c.Image, p);
                    }
                    if (outlines)
                    {
                        PointF[] o = Outline(c, mode).Select(v => new PointF(v.X - c.Bounds.X + p.X, v.Y - c.Bounds.Y + p.Y)).ToArray();
                        g.DrawPolygon(pen, o);
                    }
                    if (numbers)
                    {
                        string t = (c.FileNumber >= 0 ? c.FileNumber : c.Index).ToString();
                        g.DrawString(t, font, Brushes.Black, p.X + c.Bounds.Width / 2f - 6, p.Y + c.Bounds.Height / 2f - 5);
                        g.DrawString(t, font, Brushes.Yellow, p.X + c.Bounds.Width / 2f - 7, p.Y + c.Bounds.Height / 2f - 6);
                    }
                }
            }
            return bmp;
        }

        private static Bitmap MaskOutside(Bitmap tile)
        {
            int w = tile.Width, h = tile.Height;
            int[] px = CutterImaging.GetPixels(tile);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (!InDiamond(x, y, w, h))
                    {
                        px[y * w + x] = 0;
                    }
                }
            }
            var b = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            CutterImaging.SetPixels(b, px);
            return b;
        }

        public enum HeightSource { Flat, Brightness, InverseBrightness, Hills }

        /// <summary>Builds a terrain (map) from the tiles for the 2D / 3D map view of the texture cutter renderer.</summary>
        public static TerrainScene BuildScene(List<SliceCell> cells, SliceMode mode, HeightSource heights, int maxZ, bool hideEmpty, out List<TerrainKind> kinds, out Dictionary<Point, SliceCell> byMap)
        {
            kinds = new List<TerrainKind>();
            byMap = new Dictionary<Point, SliceCell>();
            List<SliceCell> use = cells.Where(c => c.Image != null && (!hideEmpty || !c.Empty)).ToList();
            int size = use.Count == 0 ? 1 : Math.Max(use.Max(c => c.Map.X), use.Max(c => c.Map.Y)) + 2;
            size = Math.Min(size, 160);
            var scene = new TerrainScene(size);
            for (int i = 0; i < scene.Kind.Length; i++)
            {
                scene.Kind[i] = -1;
            }
            var rnd = new Random(7);
            foreach (SliceCell c in use)
            {
                if (!scene.Inside(c.Map.X, c.Map.Y))
                {
                    continue;
                }
                var kind = new TerrainKind { Name = c.Index.ToString() };
                if (mode == SliceMode.IsoLand && c.Image.Width == TextureImaging.LandTileSize && c.Image.Height == TextureImaging.LandTileSize)
                {
                    kind.SetImages(null, new Bitmap(c.Image));
                }
                else
                {
                    kind.SetImages(new Bitmap(c.Image), null);
                }
                scene.Kind[c.Map.Y * size + c.Map.X] = kinds.Count;
                kinds.Add(kind);
                byMap[c.Map] = c;
                double b = Brightness(c);
                int z = heights switch
                {
                    HeightSource.Brightness => (int)Math.Round(b * maxZ),
                    HeightSource.InverseBrightness => (int)Math.Round((1 - b) * maxZ),
                    HeightSource.Hills => (int)Math.Round((Math.Sin(c.Map.X * 0.45) + Math.Cos(c.Map.Y * 0.38) + 2) / 4 * maxZ),
                    _ => 0
                };
                scene.Z[c.Map.Y * size + c.Map.X] = z;
            }
            return scene;
        }

        // ------------------------------------------------------------------ output

        public sealed class OutputSettings
        {
            public string Folder;
            public string FileNameFormat = "{0}";
            public int StartNumber;
            public ImageFormat Format = ImageFormat.Bmp;
            public string Extension = "bmp";
            public bool Html = true;
            public bool Csv = true;
            public bool GridImage = true;
        }

        /// <summary>
        /// Writes the tiles. File name format: {0} = number, {1} = column, {2} = row, {3} = map x, {4} = map y.
        /// Returns the number of written tiles.
        /// </summary>
        public static int Save(List<SliceCell> cells, SliceSettings s, OutputSettings o)
        {
            Directory.CreateDirectory(o.Folder);
            int n = o.StartNumber, written = 0;
            foreach (SliceCell c in cells)
            {
                c.FileNumber = -1;
                if (c.Image == null || (s.SkipEmpty && c.Empty))
                {
                    continue;
                }
                c.FileNumber = n;
                string name = FileName(c, o);
                c.Image.Save(Path.Combine(o.Folder, name), o.Format);
                n++;
                written++;
            }
            List<SliceCell> saved = cells.Where(c => c.FileNumber >= 0).ToList();
            if (o.Html)
            {
                File.WriteAllText(Path.Combine(o.Folder, "layout.html"), Html(saved, s, o));
            }
            if (o.Csv)
            {
                var sb = new StringBuilder("number;file;column;row;x;y;width;height;mapX;mapY\r\n");
                foreach (SliceCell c in saved)
                {
                    sb.Append($"{c.FileNumber};{FileName(c, o)};{c.Column};{c.Row};{c.Bounds.X};{c.Bounds.Y};{c.Bounds.Width};{c.Bounds.Height};{c.Map.X};{c.Map.Y}\r\n");
                }
                File.WriteAllText(Path.Combine(o.Folder, "layout.csv"), sb.ToString());
            }
            if (o.GridImage)
            {
                using (Bitmap grid = GridTile(s))
                {
                    grid.Save(Path.Combine(o.Folder, "grid.png"), ImageFormat.Png);
                }
            }
            return written;
        }

        public static string FileName(SliceCell c, OutputSettings o)
        {
            string name;
            try
            {
                name = string.Format(o.FileNameFormat, c.FileNumber, c.Column, c.Row, c.Map.X, c.Map.Y);
            }
            catch (FormatException)
            {
                name = c.FileNumber.ToString();
            }
            foreach (char ch in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(ch, '_');
            }
            return name + "." + o.Extension;
        }

        /// <summary>The page shows the tiles exactly at their place (the old iso page doubled the positions).</summary>
        private static string Html(List<SliceCell> cells, SliceSettings s, OutputSettings o)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset=\"utf-8\"><title>Slices</title><style>body{margin:0;background:#202020} img{position:absolute}" +
                          (s.Mode == SliceMode.IsoLand ? " img{clip-path:polygon(50% 0,100% 50%,50% 100%,0 50%)}" : string.Empty) + "</style></head><body>");
            if (cells.Count > 0)
            {
                int minX = cells.Min(c => c.Bounds.X), minY = cells.Min(c => c.Bounds.Y);
                foreach (SliceCell c in cells)
                {
                    string f = FileName(c, o);
                    sb.AppendLine($"<img src=\"{f}\" title=\"{f} – column {c.Column}, row {c.Row}, map {c.Map.X}/{c.Map.Y}\" style=\"left:{c.Bounds.X - minX}px;top:{c.Bounds.Y - minY}px\">");
                }
            }
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        /// <summary>One tile with the cut line (green), as reference overlay.</summary>
        public static Bitmap GridTile(SliceSettings s)
        {
            int w = Math.Max(2, s.TileWidth), h = Math.Max(2, s.TileHeight);
            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            using (var pen = new Pen(Color.FromArgb(160, 0, 255, 0)))
            {
                g.Clear(Color.Transparent);
                if (s.Mode == SliceMode.Straight)
                {
                    g.DrawRectangle(pen, 0, 0, w - 1, h - 1);
                }
                else
                {
                    g.DrawPolygon(pen, new[] { new PointF(w / 2f, 0), new PointF(w - 1, h / 2f), new PointF(w / 2f, h - 1), new PointF(0, h / 2f) });
                }
            }
            return bmp;
        }

        // ------------------------------------------------------------------ image tools

        public static Bitmap Scale(Bitmap src, int width, int height)
        {
            var bmp = new Bitmap(Math.Max(1, width), Math.Max(1, height), PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingMode = CompositingMode.SourceCopy;
                using (var a = new ImageAttributes())
                {
                    a.SetWrapMode(WrapMode.TileFlipXY);
                    g.DrawImage(src, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, a);
                }
            }
            return bmp;
        }
    }
}

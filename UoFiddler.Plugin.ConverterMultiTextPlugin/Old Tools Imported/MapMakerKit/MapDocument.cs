// /***************************************************************************
//  *
//  * $Author: Turley
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit
{
    /// <summary>
    /// A map in "image form": one byte per tile for the terrain group (Terrain.bmp)
    /// and one byte per tile for the altitude key (Altitude.bmp).
    /// </summary>
    internal sealed class MapDocument
    {
        public int Width { get; }
        public int Height { get; }

        /// <summary>Terrain group ID per tile, row major (index = y * Width + x).</summary>
        public byte[] Terrain { get; }

        /// <summary>Altitude palette key per tile, row major.</summary>
        public byte[] Altitude { get; }

        /// <summary>Raised whenever the content changed (editor, height tools ...).</summary>
        public event EventHandler Changed;

        public MapDocument(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("Map size must be greater than 0.");
            }

            Width = width;
            Height = height;
            Terrain = new byte[checked(width * height)];
            Altitude = new byte[checked(width * height)];
        }

        public static MapDocument CreateBlank(int width, int height, byte terrain, byte altitudeKey)
        {
            MapDocument doc = new MapDocument(width, height);
            doc.Fill(terrain, altitudeKey);
            return doc;
        }

        public void Fill(byte terrain, byte altitudeKey)
        {
            Array.Fill(Terrain, terrain);
            Array.Fill(Altitude, altitudeKey);
        }

        public bool Contains(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        public int Index(int x, int y) => (y * Width) + x;

        public MapDocument Clone()
        {
            MapDocument copy = new MapDocument(Width, Height);
            Buffer.BlockCopy(Terrain, 0, copy.Terrain, 0, Terrain.Length);
            Buffer.BlockCopy(Altitude, 0, copy.Altitude, 0, Altitude.Length);
            return copy;
        }

        public void RaiseChanged() => Changed?.Invoke(this, EventArgs.Empty);

        /// <summary>UO maps are stored in 8x8 blocks, so both sides must be multiples of 8.</summary>
        public static bool IsValidUoSize(int width, int height, out string error)
        {
            error = null;
            if (width < 8 || height < 8)
            {
                error = "Width and height must be at least 8.";
            }
            else if (width % 8 != 0 || height % 8 != 0)
            {
                error = "Width and height must be multiples of 8 (UO stores maps in 8x8 blocks).";
            }
            else if (width > 32760 || height > 32760)
            {
                error = "Width and height must be smaller than 32768.";
            }

            return error == null;
        }

        // ------------------------------------------------------------------ IO

        public static string TerrainPath(string folder, string terrainFile) => Path.Combine(folder ?? string.Empty, terrainFile ?? "Terrain.bmp");

        public void Save(string terrainPath, string altitudePath, TerrainPalette terrainPalette, AltitudePalette altitudePalette)
        {
            if (!string.IsNullOrEmpty(terrainPath))
            {
                WriteIndexed(terrainPath, Width, Height, Terrain, terrainPalette.CreateColorPalette());
            }

            if (!string.IsNullOrEmpty(altitudePath))
            {
                WriteIndexed(altitudePath, Width, Height, Altitude, altitudePalette.CreateColorPalette());
            }
        }

        /// <summary>
        /// Loads Terrain.bmp + Altitude.bmp. 8 bit indexed images are read as-is (palette index = ID).
        /// Other formats (e.g. a 24 bit export from an image editor) are converted by nearest colour.
        /// A missing altitude image is rebuilt from the terrain defaults.
        /// </summary>
        public static MapDocument Load(string terrainPath, string altitudePath, TerrainPalette terrainPalette, AltitudePalette altitudePalette, Action<string> log = null)
        {
            byte[] terrain;
            int width;
            int height;
            using (Bitmap bmp = LoadBitmapUnlocked(terrainPath))
            {
                width = bmp.Width;
                height = bmp.Height;
                terrain = ReadIndices(bmp, color => terrainPalette.Nearest(color), out bool converted);
                if (converted)
                {
                    log?.Invoke("Terrain image is not 8 bit indexed - colours were matched to the nearest terrain.");
                }
            }

            MapDocument doc = new MapDocument(width, height);
            Buffer.BlockCopy(terrain, 0, doc.Terrain, 0, terrain.Length);

            if (!string.IsNullOrEmpty(altitudePath) && File.Exists(altitudePath))
            {
                using (Bitmap bmp = LoadBitmapUnlocked(altitudePath))
                {
                    if (bmp.Width != width || bmp.Height != height)
                    {
                        log?.Invoke(string.Format("Altitude image size {0}x{1} does not match terrain {2}x{3} - altitude rebuilt from terrain.", bmp.Width, bmp.Height, width, height));
                        HeightTools.ResetFromTerrain(doc, terrainPalette, altitudePalette, null);
                    }
                    else
                    {
                        byte[] alt = ReadIndices(bmp, color => NearestAltitude(altitudePalette, color), out bool converted);
                        if (converted)
                        {
                            log?.Invoke("Altitude image is not 8 bit indexed - colours were matched to the nearest altitude.");
                        }

                        Buffer.BlockCopy(alt, 0, doc.Altitude, 0, alt.Length);
                    }
                }

                int fixedKeys = 0;
                for (int i = 0; i < doc.Altitude.Length; i++)
                {
                    if (!altitudePalette.Defined[doc.Altitude[i]])
                    {
                        doc.Altitude[i] = altitudePalette.BaseKeyFor(terrainPalette, doc.Terrain[i]);
                        fixedKeys++;
                    }
                }

                if (fixedKeys > 0)
                {
                    log?.Invoke(string.Format("{0} altitude pixels used undefined palette entries and were reset to the terrain default.", fixedKeys));
                }
            }
            else
            {
                log?.Invoke("No altitude image found - altitude created from terrain defaults.");
                HeightTools.ResetFromTerrain(doc, terrainPalette, altitudePalette, null);
            }

            return doc;
        }

        private static byte NearestAltitude(AltitudePalette palette, Color color)
        {
            int best = palette.DefaultLandKey;
            long bestDist = long.MaxValue;
            for (int i = 0; i < 256; i++)
            {
                if (!palette.Defined[i])
                {
                    continue;
                }

                Color c = palette.Colors[i];
                long dr = c.R - color.R;
                long dg = c.G - color.G;
                long db = c.B - color.B;
                long dist = (dr * dr) + (dg * dg) + (db * db);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = i;
                }
            }

            return (byte)best;
        }

        /// <summary>Loads a bitmap without keeping the file locked.</summary>
        public static Bitmap LoadBitmapUnlocked(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            using (MemoryStream ms = new MemoryStream(bytes))
            using (Image img = Image.FromStream(ms))
            {
                if (img.PixelFormat == PixelFormat.Format8bppIndexed && img is Bitmap source)
                {
                    // keep the indexed format (palette index = ID); copy so it no longer depends on the stream
                    return CloneIndexed(source);
                }

                return new Bitmap(img);
            }
        }

        private static Bitmap CloneIndexed(Bitmap source)
        {
            Rectangle rect = new Rectangle(0, 0, source.Width, source.Height);
            Bitmap target = new Bitmap(source.Width, source.Height, PixelFormat.Format8bppIndexed);
            target.Palette = source.Palette;
            BitmapData src = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            BitmapData dst = target.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            try
            {
                byte[] row = new byte[source.Width];
                for (int y = 0; y < source.Height; y++)
                {
                    Marshal.Copy(IntPtr.Add(src.Scan0, y * src.Stride), row, 0, row.Length);
                    Marshal.Copy(row, 0, IntPtr.Add(dst.Scan0, y * dst.Stride), row.Length);
                }
            }
            finally
            {
                source.UnlockBits(src);
                target.UnlockBits(dst);
            }

            return target;
        }

        /// <summary>
        /// Reads one byte per pixel. Respects the bitmap stride (the old code assumed stride == width).
        /// </summary>
        public static byte[] ReadIndices(Bitmap bmp, Func<Color, byte> colorToIndex, out bool converted)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            byte[] result = new byte[checked(width * height)];
            Rectangle rect = new Rectangle(0, 0, width, height);

            if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                converted = false;
                BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
                try
                {
                    for (int y = 0; y < height; y++)
                    {
                        Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), result, y * width, width);
                    }
                }
                finally
                {
                    bmp.UnlockBits(data);
                }

                return result;
            }

            converted = true;
            BitmapData argb = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                int[] row = new int[width];
                int lastArgb = 0;
                byte lastIndex = 0;
                bool hasLast = false;
                for (int y = 0; y < height; y++)
                {
                    Marshal.Copy(IntPtr.Add(argb.Scan0, y * argb.Stride), row, 0, width);
                    for (int x = 0; x < width; x++)
                    {
                        int value = row[x];
                        if (!hasLast || value != lastArgb)
                        {
                            lastIndex = colorToIndex(Color.FromArgb(value));
                            lastArgb = value;
                            hasLast = true;
                        }

                        result[(y * width) + x] = lastIndex;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(argb);
            }

            return result;
        }

        public static void WriteIndexed(string path, int width, int height, byte[] indices, ColorPalette palette)
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (Bitmap bmp = CreateIndexedBitmap(width, height, indices, palette))
            {
                bmp.Save(path, ImageFormat.Bmp);
            }
        }

        public static Bitmap CreateIndexedBitmap(int width, int height, byte[] indices, ColorPalette palette)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            try
            {
                bmp.Palette = palette;
                Rectangle rect = new Rectangle(0, 0, width, height);
                BitmapData data = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                try
                {
                    for (int y = 0; y < height; y++)
                    {
                        Marshal.Copy(indices, y * width, IntPtr.Add(data.Scan0, y * data.Stride), width);
                    }
                }
                finally
                {
                    bmp.UnlockBits(data);
                }

                return bmp;
            }
            catch
            {
                bmp.Dispose();
                throw;
            }
        }

        // ------------------------------------------------------------------ rendering

        public enum RenderMode
        {
            Terrain,
            Altitude,
            Relief
        }

        /// <summary>Renders a down-scaled preview (32 bit) that fits into maxWidth x maxHeight.</summary>
        public Bitmap RenderPreview(int maxWidth, int maxHeight, RenderMode mode, TerrainPalette terrainPalette, AltitudePalette altitudePalette)
        {
            double scale = Math.Min((double)maxWidth / Width, (double)maxHeight / Height);
            if (scale > 1)
            {
                scale = Math.Floor(scale);
            }

            int w = Math.Max(1, (int)(Width * scale));
            int h = Math.Max(1, (int)(Height * scale));
            Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                int[] row = new int[w];
                for (int y = 0; y < h; y++)
                {
                    int sy = Math.Min(Height - 1, (int)(y / scale));
                    for (int x = 0; x < w; x++)
                    {
                        int sx = Math.Min(Width - 1, (int)(x / scale));
                        row[x] = PixelColor(sx, sy, mode, terrainPalette, altitudePalette);
                    }

                    Marshal.Copy(row, 0, IntPtr.Add(data.Scan0, y * data.Stride), w);
                }
            }
            finally
            {
                bmp.UnlockBits(data);
            }

            return bmp;
        }

        public int PixelColor(int x, int y, RenderMode mode, TerrainPalette terrainPalette, AltitudePalette altitudePalette)
        {
            int i = (y * Width) + x;
            switch (mode)
            {
                case RenderMode.Altitude:
                    return altitudePalette.Argb[Altitude[i]];
                case RenderMode.Relief:
                    {
                        int c = terrainPalette.Argb[Terrain[i]];
                        int here = altitudePalette.Altitude[Altitude[i]];
                        int nx = x > 0 ? altitudePalette.Altitude[Altitude[i - 1]] : here;
                        int ny = y > 0 ? altitudePalette.Altitude[Altitude[i - Width]] : here;
                        int slope = ((here - nx) + (here - ny)) * 6;
                        return Shade(c, slope);
                    }

                default:
                    return terrainPalette.Argb[Terrain[i]];
            }
        }

        public static int Shade(int argb, int amount)
        {
            if (amount == 0)
            {
                return argb;
            }

            if (amount > 120)
            {
                amount = 120;
            }
            else if (amount < -120)
            {
                amount = -120;
            }

            int r = Clamp(((argb >> 16) & 0xFF) + amount);
            int g = Clamp(((argb >> 8) & 0xFF) + amount);
            int b = Clamp((argb & 0xFF) + amount);
            return unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
        }

        private static int Clamp(int v) => v < 0 ? 0 : (v > 255 ? 255 : v);
    }

    internal static class AltitudePaletteExtensions
    {
        /// <summary>Default altitude key for a terrain (the "Base" attribute of Terrain.xml).</summary>
        public static byte BaseKeyFor(this AltitudePalette altitudes, TerrainPalette terrains, byte terrain)
        {
            byte key = terrains.BaseAltitudeKey[terrain];
            return altitudes.IsValid(key) ? key : altitudes.DefaultLandKey;
        }
    }
}

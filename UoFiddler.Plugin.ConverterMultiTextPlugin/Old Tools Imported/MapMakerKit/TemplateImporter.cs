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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit
{
    public enum TemplateScaleMode
    {
        /// <summary>Stretch to the full map size.</summary>
        Stretch,

        /// <summary>Keep the aspect ratio and center it, rest = fill terrain.</summary>
        Fit,

        /// <summary>Repeat the image over the whole map.</summary>
        Tile,

        /// <summary>1:1 at the top left corner.</summary>
        Original
    }

    /// <summary>
    /// Turns images from a template folder into map data:
    /// - terrain colour images (colours of Terrain.xml, other colours are matched to the nearest terrain)
    /// - grey scale height maps (dark = deep water, bright = mountains) for the generator
    /// - sketches for the generator
    /// </summary>
    internal static class TemplateImporter
    {
        public static readonly string[] Extensions = { ".png", ".bmp", ".jpg", ".jpeg", ".gif", ".tif", ".tiff" };

        /// <summary>Terrains used when "natural terrains only" is selected (good for photos / drawings).</summary>
        public static readonly int[] NaturalTerrains =
        {
            TerrainIds.Grass, TerrainIds.Sand, TerrainIds.DeepWater, TerrainIds.ShallowWater, TerrainIds.LargeDirt,
            TerrainIds.SmallDirt, TerrainIds.Jungle, TerrainIds.Forest, TerrainIds.Snow, TerrainIds.Lava,
            TerrainIds.Rock, TerrainIds.Swamp, TerrainIds.Beach, TerrainIds.Blank, TerrainIds.DungeonCave
        };

        public static List<string> ListImages(string folder)
        {
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                return new List<string>();
            }

            return Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
                .Where(f => Extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static Bitmap LoadThumbnail(string path, int size)
        {
            using (Bitmap source = MapDocument.LoadBitmapUnlocked(path))
            {
                Bitmap thumb = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(thumb))
                {
                    g.Clear(Color.FromArgb(40, 40, 40));
                    double scale = Math.Min((double)size / source.Width, (double)size / source.Height);
                    int w = Math.Max(1, (int)(source.Width * scale));
                    int h = Math.Max(1, (int)(source.Height * scale));
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(source, new Rectangle((size - w) / 2, (size - h) / 2, w, h));
                }

                return thumb;
            }
        }

        /// <summary>
        /// Converts an image to terrain IDs (its own size). 8 bit images that already use the terrain palette keep
        /// their indices, everything else is matched by colour. Fully transparent pixels become "No Draw" (0).
        /// </summary>
        public static byte[] ImageToTerrain(Bitmap image, TerrainPalette palette, bool naturalOnly)
        {
            bool[] allowed = null;
            if (naturalOnly)
            {
                allowed = new bool[256];
                foreach (int id in NaturalTerrains)
                {
                    allowed[id] = true;
                }
            }

            if (image.PixelFormat == PixelFormat.Format8bppIndexed && !naturalOnly && PaletteMatches(image.Palette, palette))
            {
                return MapDocument.ReadIndices(image, c => palette.Nearest(c), out _);
            }

            Dictionary<int, byte> cache = new Dictionary<int, byte>();
            using (Bitmap argb = ToArgb(image))
            {
                return MapDocument.ReadIndices(argb, c =>
                {
                    if (c.A < 128)
                    {
                        return TerrainIds.NoDraw;
                    }

                    int key = c.ToArgb();
                    if (!cache.TryGetValue(key, out byte id))
                    {
                        id = palette.Nearest(c, allowed);
                        cache[key] = id;
                    }

                    return id;
                }, out _);
            }
        }

        /// <summary>Converts an image to grey values 0..255 (its own size).</summary>
        public static byte[] ImageToGrey(Bitmap image)
        {
            using (Bitmap argb = ToArgb(image))
            {
                int w = argb.Width;
                int h = argb.Height;
                byte[] result = new byte[w * h];
                BitmapData data = argb.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    int[] row = new int[w];
                    for (int y = 0; y < h; y++)
                    {
                        Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), row, 0, w);
                        for (int x = 0; x < w; x++)
                        {
                            int c = row[x];
                            int r = (c >> 16) & 0xFF;
                            int g = (c >> 8) & 0xFF;
                            int b = c & 0xFF;
                            result[(y * w) + x] = (byte)(((r * 299) + (g * 587) + (b * 114)) / 1000);
                        }
                    }
                }
                finally
                {
                    argb.UnlockBits(data);
                }

                return result;
            }
        }

        /// <summary>Scales terrain IDs (nearest neighbour) to the map size.</summary>
        public static byte[] Resample(byte[] source, int sw, int sh, int tw, int th, TemplateScaleMode mode, byte fill)
        {
            byte[] result = new byte[tw * th];
            switch (mode)
            {
                case TemplateScaleMode.Stretch:
                    for (int y = 0; y < th; y++)
                    {
                        int sy = Math.Min(sh - 1, (int)((long)y * sh / th));
                        for (int x = 0; x < tw; x++)
                        {
                            int sx = Math.Min(sw - 1, (int)((long)x * sw / tw));
                            result[(y * tw) + x] = source[(sy * sw) + sx];
                        }
                    }

                    break;

                case TemplateScaleMode.Fit:
                    {
                        Array.Fill(result, fill);
                        double scale = Math.Min((double)tw / sw, (double)th / sh);
                        int w = Math.Max(1, (int)(sw * scale));
                        int h = Math.Max(1, (int)(sh * scale));
                        int ox = (tw - w) / 2;
                        int oy = (th - h) / 2;
                        for (int y = 0; y < h; y++)
                        {
                            int sy = Math.Min(sh - 1, (int)(y / scale));
                            for (int x = 0; x < w; x++)
                            {
                                int sx = Math.Min(sw - 1, (int)(x / scale));
                                result[((oy + y) * tw) + ox + x] = source[(sy * sw) + sx];
                            }
                        }

                        break;
                    }

                case TemplateScaleMode.Tile:
                    for (int y = 0; y < th; y++)
                    {
                        for (int x = 0; x < tw; x++)
                        {
                            result[(y * tw) + x] = source[((y % sh) * sw) + (x % sw)];
                        }
                    }

                    break;

                default:
                    Array.Fill(result, fill);
                    for (int y = 0; y < Math.Min(sh, th); y++)
                    {
                        Buffer.BlockCopy(source, y * sw, result, y * tw, Math.Min(sw, tw));
                    }

                    break;
            }

            return result;
        }

        /// <summary>Template -> complete map document (terrain + default altitudes).</summary>
        public static MapDocument ToDocument(Bitmap image, int width, int height, TemplateScaleMode mode, bool naturalOnly, byte fill, TerrainPalette terrains, AltitudePalette altitudes)
        {
            byte[] ids = ImageToTerrain(image, terrains, naturalOnly);
            byte[] scaled = Resample(ids, image.Width, image.Height, width, height, mode, fill);
            MapDocument doc = new MapDocument(width, height);
            for (int i = 0; i < scaled.Length; i++)
            {
                byte t = scaled[i] == TerrainIds.NoDraw ? fill : scaled[i];
                doc.Terrain[i] = t;
                doc.Altitude[i] = altitudes.BaseKeyFor(terrains, t);
            }

            return doc;
        }

        private static bool PaletteMatches(ColorPalette imagePalette, TerrainPalette palette)
        {
            int matches = 0;
            int defined = 0;
            for (int i = 0; i < 256 && i < imagePalette.Entries.Length; i++)
            {
                if (!palette.Defined[i])
                {
                    continue;
                }

                defined++;
                if ((imagePalette.Entries[i].ToArgb() & 0xFFFFFF) == (palette.Colors[i].ToArgb() & 0xFFFFFF))
                {
                    matches++;
                }
            }

            return defined > 0 && matches >= defined * 0.9;
        }

        private static Bitmap ToArgb(Bitmap image)
        {
            Bitmap argb = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(argb))
            {
                g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));
            }

            return argb;
        }
    }
}

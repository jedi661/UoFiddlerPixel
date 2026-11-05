// /***************************************************************************
//  *
//  * Dragon Map Studio – land outline from an image (country silhouette, sketch …)
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public enum OutlineFit
    {
        /// <summary>Whole image visible, keeps the aspect ratio.</summary>
        Fit,
        /// <summary>Fills the map, keeps the aspect ratio (cuts off).</summary>
        Fill,
        /// <summary>Stretches to the map size.</summary>
        Stretch,
        /// <summary>1 pixel = 1 tile, centred.</summary>
        Center
    }

    public enum OutlineAlpha
    {
        /// <summary>Transparent pixels decide when the image has transparency, otherwise the brightness.</summary>
        Auto,
        Brightness,
        Transparency
    }

    public enum OutlineApplyMode
    {
        /// <summary>Land colour inside, water colour outside.</summary>
        LandAndWater,
        /// <summary>Only the land is painted, the rest of the map stays.</summary>
        LandOnly,
        /// <summary>Only the outline (coast line) is drawn.</summary>
        OutlineOnly,
        /// <summary>Land, water and the coast line in its own colour.</summary>
        LandWaterAndOutline
    }

    public sealed class OutlineSettings
    {
        [Category("1 Bild"), DisplayName("Bilddatei"), Description("Umriss eines Landes / Kontinents (PNG mit Transparenz, Schwarz-Weiß-Bild, Karte …).")]
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string ImagePath { get; set; } = string.Empty;

        [Category("1 Bild"), DisplayName("Erkennung"), Description("Auto: Transparenz wenn vorhanden, sonst Helligkeit.")]
        public OutlineAlpha Detection { get; set; } = OutlineAlpha.Auto;

        [Category("1 Bild"), DisplayName("Land ist dunkel"), Description("An: dunkle Pixel = Land (typische Silhouette auf weißem Grund).")]
        public bool LandIsDark { get; set; } = true;

        [Category("1 Bild"), DisplayName("Schwelle 0..255")]
        public int Threshold { get; set; } = 128;

        [Category("1 Bild"), DisplayName("Umkehren")]
        public bool Invert { get; set; }

        [Category("2 Platzierung"), DisplayName("Einpassen")]
        public OutlineFit Fit { get; set; } = OutlineFit.Fit;

        [Category("2 Platzierung"), DisplayName("Rand %"), Description("Freier Rand (Wasser) um das Bild.")]
        public double MarginPercent { get; set; } = 6;

        [Category("2 Platzierung"), DisplayName("Glätten (Tiles)"), Description("Rundet treppige Kanten ab (0 = aus).")]
        public int Smooth { get; set; } = 2;

        [Category("3 Anwenden"), DisplayName("Modus")]
        public OutlineApplyMode Mode { get; set; } = OutlineApplyMode.LandAndWater;

        [Category("3 Anwenden"), DisplayName("Land-Farbe (Index)"), Description("Palettenindex dezimal (IMod13: 0 = Gras z0). Tipp: in der Farbtabelle klicken und 'Als Land-Farbe' wählen.")]
        public int LandIndex { get; set; } = 0x00;

        [Category("3 Anwenden"), DisplayName("Wasser-Farbe (Index)"), Description("IMod13: 32 (0x20) = Wasser -5.")]
        public int WaterIndex { get; set; } = 0x20;

        [Category("3 Anwenden"), DisplayName("Umriss-Farbe (Index)"), Description("Farbe der Küstenlinie (IMod13: 0x50.. Sand).")]
        public int OutlineIndex { get; set; } = 0x50;

        [Category("3 Anwenden"), DisplayName("Umriss-Dicke (Tiles)")]
        public int OutlineThickness { get; set; } = 2;

        public OutlineSettings Clone() => (OutlineSettings)MemberwiseClone();
    }

    /// <summary>Turns an image into a land mask in map size and paints it into an <see cref="IndexedMap"/>.</summary>
    public static class OutlineImporter
    {
        /// <summary>Returns a mask (0 = water, 1 = land) with the size of the map.</summary>
        public static float[] CreateMask(Bitmap image, int mapW, int mapH, OutlineSettings s)
        {
            int iw = image.Width, ih = image.Height;
            var argb = new int[iw * ih];
            BitmapData bd = image.LockBits(new Rectangle(0, 0, iw, ih), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < ih; y++)
                {
                    Marshal.Copy(bd.Scan0 + y * bd.Stride, argb, y * iw, iw);
                }
            }
            finally
            {
                image.UnlockBits(bd);
            }

            bool useAlpha = s.Detection == OutlineAlpha.Transparency;
            if (s.Detection == OutlineAlpha.Auto)
            {
                int transparent = 0;
                for (int i = 0; i < argb.Length; i += 7)
                {
                    if (((argb[i] >> 24) & 0xFF) < 128)
                    {
                        transparent++;
                    }
                }
                useAlpha = transparent > argb.Length / 7 / 100; // more than 1 % transparent
            }

            RectangleF dest = DestRect(iw, ih, mapW, mapH, s);
            var mask = new float[mapW * mapH];
            for (int y = 0; y < mapH; y++)
            {
                double v = (y + 0.5 - dest.Y) / dest.Height;
                if (v < 0 || v >= 1)
                {
                    continue;
                }
                int sy = Math.Min(ih - 1, (int)(v * ih));
                for (int x = 0; x < mapW; x++)
                {
                    double u = (x + 0.5 - dest.X) / dest.Width;
                    if (u < 0 || u >= 1)
                    {
                        continue;
                    }
                    int sx = Math.Min(iw - 1, (int)(u * iw));
                    int c = argb[sy * iw + sx];
                    int a = (c >> 24) & 0xFF;
                    bool land;
                    if (useAlpha)
                    {
                        land = a >= 128;
                    }
                    else
                    {
                        int r = (c >> 16) & 0xFF, g = (c >> 8) & 0xFF, b = c & 0xFF;
                        int lum = (r * 299 + g * 587 + b * 114) / 1000;
                        if (a < 128)
                        {
                            lum = 255; // transparent = background
                        }
                        land = s.LandIsDark ? lum < s.Threshold : lum >= s.Threshold;
                    }
                    if (s.Invert)
                    {
                        land = !land;
                    }
                    mask[y * mapW + x] = land ? 1f : 0f;
                }
            }

            if (s.Smooth > 0)
            {
                BoxBlur(mask, mapW, mapH, s.Smooth);
                for (int i = 0; i < mask.Length; i++)
                {
                    mask[i] = mask[i] >= 0.5f ? 1f : 0f;
                }
            }
            return mask;
        }

        private static RectangleF DestRect(int iw, int ih, int mapW, int mapH, OutlineSettings s)
        {
            float margin = (float)(Math.Clamp(s.MarginPercent, 0, 45) / 100.0);
            float availW = mapW * (1 - 2 * margin), availH = mapH * (1 - 2 * margin);
            float w, h;
            switch (s.Fit)
            {
                case OutlineFit.Stretch:
                    w = availW;
                    h = availH;
                    break;
                case OutlineFit.Center:
                    w = iw;
                    h = ih;
                    break;
                case OutlineFit.Fill:
                    {
                        float k = Math.Max(availW / iw, availH / ih);
                        w = iw * k;
                        h = ih * k;
                        break;
                    }
                default:
                    {
                        float k = Math.Min(availW / iw, availH / ih);
                        w = iw * k;
                        h = ih * k;
                        break;
                    }
            }
            return new RectangleF((mapW - w) / 2f, (mapH - h) / 2f, w, h);
        }

        /// <summary>Paints the mask into the map (records undo through <paramref name="history"/>).</summary>
        public static void Apply(IndexedMap map, float[] mask, OutlineSettings s, MapHistory history)
        {
            history?.Begin("Umriss anwenden");
            history?.TouchAll();
            ushort land = (ushort)Math.Clamp(s.LandIndex, 0, map.MaxIndex);
            ushort water = (ushort)Math.Clamp(s.WaterIndex, 0, map.MaxIndex);
            ushort outline = (ushort)Math.Clamp(s.OutlineIndex, 0, map.MaxIndex);
            int w = map.Width, h = map.Height;

            if (s.Mode != OutlineApplyMode.OutlineOnly)
            {
                for (int i = 0; i < mask.Length; i++)
                {
                    if (mask[i] >= 0.5f)
                    {
                        map.Pixels[i] = land;
                    }
                    else if (s.Mode != OutlineApplyMode.LandOnly)
                    {
                        map.Pixels[i] = water;
                    }
                }
            }

            if (s.Mode == OutlineApplyMode.OutlineOnly || s.Mode == OutlineApplyMode.LandWaterAndOutline)
            {
                bool[] edge = EdgeOf(mask, w, h, Math.Max(1, s.OutlineThickness));
                for (int i = 0; i < edge.Length; i++)
                {
                    if (edge[i])
                    {
                        map.Pixels[i] = outline;
                    }
                }
            }
            history?.End();
        }

        /// <summary>Land cells next to water, grown inwards to the wanted thickness.</summary>
        public static bool[] EdgeOf(float[] mask, int w, int h, int thickness)
        {
            var edge = new bool[w * h];
            var dist = new int[w * h];
            Array.Fill(dist, int.MaxValue);
            var queue = new System.Collections.Generic.Queue<int>();
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    if (mask[i] < 0.5f)
                    {
                        continue;
                    }
                    bool border = (x > 0 && mask[i - 1] < 0.5f) || (x < w - 1 && mask[i + 1] < 0.5f) ||
                                  (y > 0 && mask[i - w] < 0.5f) || (y < h - 1 && mask[i + w] < 0.5f);
                    if (border)
                    {
                        dist[i] = 1;
                        queue.Enqueue(i);
                    }
                }
            }
            while (queue.Count > 0)
            {
                int i = queue.Dequeue();
                edge[i] = true;
                if (dist[i] >= thickness)
                {
                    continue;
                }
                int x = i % w, y = i / w;
                void Visit(int j)
                {
                    if (mask[j] >= 0.5f && dist[j] == int.MaxValue)
                    {
                        dist[j] = dist[i] + 1;
                        queue.Enqueue(j);
                    }
                }
                if (x > 0) { Visit(i - 1); }
                if (x < w - 1) { Visit(i + 1); }
                if (y > 0) { Visit(i - w); }
                if (y < h - 1) { Visit(i + w); }
            }
            return edge;
        }

        /// <summary>Separable box blur (in place), radius in tiles.</summary>
        public static void BoxBlur(float[] data, int w, int h, int radius)
        {
            if (radius <= 0)
            {
                return;
            }
            var tmp = new float[data.Length];
            float norm = 1f / (2 * radius + 1);
            System.Threading.Tasks.Parallel.For(0, h, y =>
            {
                int o = y * w;
                float sum = 0;
                for (int k = -radius; k <= radius; k++)
                {
                    sum += data[o + Math.Clamp(k, 0, w - 1)];
                }
                for (int x = 0; x < w; x++)
                {
                    tmp[o + x] = sum * norm;
                    sum += data[o + Math.Min(w - 1, x + radius + 1)] - data[o + Math.Max(0, x - radius)];
                }
            });
            System.Threading.Tasks.Parallel.For(0, w, x =>
            {
                float sum = 0;
                for (int k = -radius; k <= radius; k++)
                {
                    sum += tmp[Math.Clamp(k, 0, h - 1) * w + x];
                }
                for (int y = 0; y < h; y++)
                {
                    data[y * w + x] = sum * norm;
                    sum += tmp[Math.Min(h - 1, y + radius + 1) * w + x] - tmp[Math.Max(0, y - radius) * w + x];
                }
            });
        }

        /// <summary>Mask from the current map: every cell that is not water (group) counts as land.</summary>
        public static float[] MaskFromMap(IndexedMap map, DragonRuleSet rules, int waterGroup)
        {
            var isWater = new bool[65536];
            foreach (DragonPaletteEntry e in rules.Palette)
            {
                isWater[e.Index] = e.Group == waterGroup;
            }
            var mask = new float[map.Pixels.Length];
            for (int i = 0; i < mask.Length; i++)
            {
                mask[i] = isWater[map.Pixels[i]] ? 0f : 1f;
            }
            return mask;
        }
    }
}

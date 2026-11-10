using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AltitudeTool
{
    /// <summary>Ways to color the height map.</summary>
    internal enum HeightColorMode
    {
        GrayUo,       // z + 128 like the original tool
        GrayStretch,  // min … max of the map stretched to black … white
        Terrain,      // water, beach, meadow, hills, mountains, snow
        Hillshade,    // light from north-west
        TerrainShade  // terrain colors + hillshade
    }

    /// <summary>Editing operations on a region of a height map.</summary>
    internal static class HeightOps
    {
        public static int Offset(HeightMap m, Rectangle r, int offset)
        {
            return ForEach(m, r, z => HeightMap.Clamp(z + offset));
        }

        public static int SetTo(HeightMap m, Rectangle r, int value)
        {
            sbyte v = HeightMap.Clamp(value);
            return ForEach(m, r, _ => v);
        }

        public static int ClampRange(HeightMap m, Rectangle r, int min, int max)
        {
            return ForEach(m, r, z => HeightMap.Clamp(Math.Max(min, Math.Min(max, z))));
        }

        /// <summary>Scales the height around a base altitude (factor 2 = twice as steep, 0.5 = flatter).</summary>
        public static int Scale(HeightMap m, Rectangle r, double factor, int baseZ)
        {
            return ForEach(m, r, z => HeightMap.Clamp((int)Math.Round(baseZ + (z - baseZ) * factor)));
        }

        /// <summary>Terraces: rounds to steps of <paramref name="step"/> (UO likes steps of 5 for stairs and cliffs).</summary>
        public static int Terrace(HeightMap m, Rectangle r, int step)
        {
            if (step <= 1)
            {
                return 0;
            }
            return ForEach(m, r, z => HeightMap.Clamp((int)Math.Round(z / (double)step) * step));
        }

        /// <summary>Box blur with radius; repeated <paramref name="passes"/> times. Only tiles inside r change, neighbours outside are read.</summary>
        public static int Smooth(HeightMap m, Rectangle r, int radius, int passes, Func<int, int, bool> include = null)
        {
            int changed = 0;
            radius = Math.Max(1, radius);
            for (int p = 0; p < Math.Max(1, passes); p++)
            {
                // read a frame around the region so the border blends with the neighbours
                Rectangle read = Rectangle.Intersect(Rectangle.Inflate(r, radius, radius), m.Bounds);
                sbyte[] src = m.CopyRegion(read);
                int w = read.Width, h = read.Height;
                // summed area table
                var sat = new long[(w + 1) * (h + 1)];
                for (int y = 0; y < h; y++)
                {
                    long rowSum = 0;
                    for (int x = 0; x < w; x++)
                    {
                        rowSum += src[y * w + x];
                        sat[(y + 1) * (w + 1) + x + 1] = sat[y * (w + 1) + x + 1] + rowSum;
                    }
                }
                for (int y = r.Top; y < r.Bottom; y++)
                {
                    for (int x = r.Left; x < r.Right; x++)
                    {
                        if (include != null && !include(x, y))
                        {
                            continue;
                        }
                        int x0 = Math.Max(read.Left, x - radius) - read.X, x1 = Math.Min(read.Right - 1, x + radius) - read.X;
                        int y0 = Math.Max(read.Top, y - radius) - read.Y, y1 = Math.Min(read.Bottom - 1, y + radius) - read.Y;
                        long sum = sat[(y1 + 1) * (w + 1) + x1 + 1] - sat[y0 * (w + 1) + x1 + 1] - sat[(y1 + 1) * (w + 1) + x0] + sat[y0 * (w + 1) + x0];
                        int n = (x1 - x0 + 1) * (y1 - y0 + 1);
                        sbyte v = HeightMap.Clamp((int)Math.Round(sum / (double)n));
                        int idx = y * m.Width + x;
                        if (m.Z[idx] != v)
                        {
                            m.Z[idx] = v;
                            changed++;
                        }
                    }
                }
            }
            return changed;
        }

        /// <summary>Adds smooth random hills (value noise) with an amplitude in z and a feature size in tiles.</summary>
        public static int Noise(HeightMap m, Rectangle r, int amplitude, int size, int seed)
        {
            size = Math.Max(2, size);
            return ForEachXY(m, r, (x, y, z) =>
            {
                double n = ValueNoise(x / (double)size, y / (double)size, seed) * 0.65 + ValueNoise(x * 2.0 / size, y * 2.0 / size, seed + 17) * 0.35;
                return HeightMap.Clamp(z + (int)Math.Round((n * 2 - 1) * amplitude));
            });
        }

        private static double ValueNoise(double x, double y, int seed)
        {
            int xi = (int)Math.Floor(x), yi = (int)Math.Floor(y);
            double fx = x - xi, fy = y - yi;
            fx = fx * fx * (3 - 2 * fx);
            fy = fy * fy * (3 - 2 * fy);
            double a = Hash(xi, yi, seed), b = Hash(xi + 1, yi, seed), c = Hash(xi, yi + 1, seed), d = Hash(xi + 1, yi + 1, seed);
            return a + (b - a) * fx + (c - a) * fy + (a - b - c + d) * fx * fy;
        }

        private static double Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + seed * 144269504);
                h = (h ^ (h >> 13)) * 1274126177;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / (double)0xFFFFFF;
            }
        }

        /// <summary>Round brush: strength 1 … 10, mode 0 raise, 1 lower, 2 smooth, 3 flatten to targetZ. Soft falloff to the edge.</summary>
        public static Rectangle Brush(HeightMap m, int cx, int cy, int radius, int strength, int mode, int targetZ)
        {
            var r = Rectangle.Intersect(new Rectangle(cx - radius, cy - radius, radius * 2 + 1, radius * 2 + 1), m.Bounds);
            if (r.IsEmpty)
            {
                return r;
            }
            if (mode == 2)
            {
                Smooth(m, r, Math.Max(1, radius / 3), 1, (x, y) => (x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius);
                return r;
            }
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    double d = Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (d > radius)
                    {
                        continue;
                    }
                    double fall = radius == 0 ? 1 : 1 - d / (radius + 1);
                    int idx = y * m.Width + x;
                    int z = m.Z[idx];
                    switch (mode)
                    {
                        case 0:
                            z += (int)Math.Max(1, Math.Round(strength * fall));
                            break;
                        case 1:
                            z -= (int)Math.Max(1, Math.Round(strength * fall));
                            break;
                        default:
                            double t = Math.Min(1, strength / 10.0 * fall * 1.5);
                            z = (int)Math.Round(z + (targetZ - z) * t);
                            break;
                    }
                    m.Z[idx] = HeightMap.Clamp(z);
                }
            }
            return r;
        }

        private static int ForEach(HeightMap m, Rectangle r, Func<int, sbyte> f)
        {
            int changed = 0;
            for (int y = r.Top; y < r.Bottom; y++)
            {
                int row = y * m.Width;
                for (int x = r.Left; x < r.Right; x++)
                {
                    sbyte v = f(m.Z[row + x]);
                    if (m.Z[row + x] != v)
                    {
                        m.Z[row + x] = v;
                        changed++;
                    }
                }
            }
            return changed;
        }

        private static int ForEachXY(HeightMap m, Rectangle r, Func<int, int, int, sbyte> f)
        {
            int changed = 0;
            for (int y = r.Top; y < r.Bottom; y++)
            {
                int row = y * m.Width;
                for (int x = r.Left; x < r.Right; x++)
                {
                    sbyte v = f(x, y, m.Z[row + x]);
                    if (m.Z[row + x] != v)
                    {
                        m.Z[row + x] = v;
                        changed++;
                    }
                }
            }
            return changed;
        }

        // ------------------------------------------------------------------ rendering

        /// <summary>Color of an altitude (ARGB) for the color modes without shading.</summary>
        public static int[] MakePalette(HeightColorMode mode, int min, int max)
        {
            var pal = new int[256];
            for (int i = 0; i < 256; i++)
            {
                int z = i - 128;
                Color c;
                switch (mode)
                {
                    case HeightColorMode.GrayStretch:
                    {
                        int g = max > min ? (int)Math.Round(Math.Max(0, Math.Min(1, (z - min) / (double)(max - min))) * 255) : 128;
                        c = Color.FromArgb(g, g, g);
                        break;
                    }
                    case HeightColorMode.Terrain:
                    case HeightColorMode.TerrainShade:
                        c = TerrainColor(z);
                        break;
                    default:
                        c = Color.FromArgb(i, i, i);
                        break;
                }
                pal[i] = c.ToArgb();
            }
            return pal;
        }

        private static Color TerrainColor(int z)
        {
            (int Z, Color C)[] stops =
            {
                (-128, Color.FromArgb(5, 10, 40)),
                (-20, Color.FromArgb(20, 50, 120)),
                (-6, Color.FromArgb(50, 110, 190)),
                (-5, Color.FromArgb(215, 200, 140)),
                (0, Color.FromArgb(200, 190, 130)),
                (2, Color.FromArgb(90, 150, 60)),
                (15, Color.FromArgb(60, 120, 40)),
                (30, Color.FromArgb(120, 110, 60)),
                (50, Color.FromArgb(130, 100, 70)),
                (75, Color.FromArgb(140, 135, 130)),
                (100, Color.FromArgb(235, 235, 240)),
                (127, Color.White)
            };
            for (int i = 1; i < stops.Length; i++)
            {
                if (z <= stops[i].Z)
                {
                    double t = (z - stops[i - 1].Z) / (double)Math.Max(1, stops[i].Z - stops[i - 1].Z);
                    Color a = stops[i - 1].C, b = stops[i].C;
                    return Color.FromArgb((int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
                }
            }
            return Color.White;
        }

        /// <summary>
        /// Renders tiles into a 32 bit bitmap. view = map area shown; output size = bitmap size (nearest neighbour sampling).
        /// contour &gt; 0 draws a dark line every contour z steps; original != null tints changed tiles.
        /// </summary>
        public static void Render(HeightMap m, RectangleF view, Bitmap target, HeightColorMode mode, int[] palette, int contour, HeightMap original)
        {
            int w = target.Width, h = target.Height;
            BitmapData bd = target.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                var row = new int[w];
                var xs = new int[w];
                for (int x = 0; x < w; x++)
                {
                    xs[x] = (int)Math.Floor(view.X + (x + 0.5) * view.Width / w);
                }
                bool shade = mode == HeightColorMode.Hillshade || mode == HeightColorMode.TerrainShade;
                for (int y = 0; y < h; y++)
                {
                    int my = (int)Math.Floor(view.Y + (y + 0.5) * view.Height / h);
                    for (int x = 0; x < w; x++)
                    {
                        int mx = xs[x];
                        if (mx < 0 || my < 0 || mx >= m.Width || my >= m.Height)
                        {
                            row[x] = unchecked((int)0xFF1E1E1E);
                            continue;
                        }
                        int idx = my * m.Width + mx;
                        int z = m.Z[idx];
                        int c = palette[z + 128];
                        if (shade)
                        {
                            int zl = mx > 0 ? m.Z[idx - 1] : z;
                            int zu = my > 0 ? m.Z[idx - m.Width] : z;
                            double light = 0.5 + ((z - zl) + (z - zu)) * 0.06;
                            light = Math.Max(0.15, Math.Min(1.35, light + 0.35));
                            if (mode == HeightColorMode.Hillshade)
                            {
                                int g = (int)Math.Max(0, Math.Min(255, 150 * light));
                                c = unchecked((int)0xFF000000) | (g << 16) | (g << 8) | g;
                            }
                            else
                            {
                                c = Scale(c, light);
                            }
                        }
                        if (contour > 0)
                        {
                            int zl = mx > 0 ? m.Z[idx - 1] : z;
                            int zu = my > 0 ? m.Z[idx - m.Width] : z;
                            if (FloorDiv(z, contour) != FloorDiv(zl, contour) || FloorDiv(z, contour) != FloorDiv(zu, contour))
                            {
                                c = Scale(c, 0.45);
                            }
                        }
                        if (original != null && original.Z[idx] != z)
                        {
                            int r = (c >> 16) & 0xFF, g = (c >> 8) & 0xFF, b = c & 0xFF;
                            c = unchecked((int)0xFF000000) | (Math.Min(255, r / 2 + 128) << 16) | ((g / 2) << 8) | (b / 2 + 60);
                        }
                        row[x] = c;
                    }
                    Marshal.Copy(row, 0, bd.Scan0 + y * bd.Stride, w);
                }
            }
            finally
            {
                target.UnlockBits(bd);
            }
        }

        private static int FloorDiv(int a, int b) => (int)Math.Floor(a / (double)b);

        private static int Scale(int c, double f)
        {
            int r = (int)Math.Min(255, ((c >> 16) & 0xFF) * f), g = (int)Math.Min(255, ((c >> 8) & 0xFF) * f), b = (int)Math.Min(255, (c & 0xFF) * f);
            return unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
        }
    }
}

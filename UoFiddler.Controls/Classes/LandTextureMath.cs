using System;
using System.Collections.Generic;

namespace UoFiddler.Controls.Classes
{
    /// <summary>What an image contains (for the search of empty / placeholder land tiles and textures).</summary>
    public enum TerrainImageKind
    {
        /// <summary>No visible pixel (all transparent or pure black).</summary>
        Empty = 0,

        /// <summary>One single colour (placeholders like the pink square).</summary>
        SingleColour = 1,

        /// <summary>2 - 4 colours (often a placeholder or a "NO DRAW" marker).</summary>
        FewColours = 2,

        /// <summary>A real picture.</summary>
        Normal = 3
    }

    /// <summary>
    /// Pixel math for land tiles (44x44 diamond) and textures (64x64 / 128x128) on ARGB1555 arrays (no GDI).
    ///
    /// HOW THE CLIENT CONNECTS THEM: a land tile has a texture id in the land tiledata. Flat land is drawn with the
    /// 44x44 art; land with different heights is drawn stretched with the TEXTURE. The texture is laid onto the
    /// diamond turned by 45 degrees:
    ///     texture (0,0) top left     -> top corner of the diamond    (22, 0)
    ///     texture (1,0) top right    -> right corner                 (44, 22)
    ///     texture (0,1) bottom left  -> left corner                  (0, 22)
    ///     texture (1,1) bottom right -> bottom corner                (22, 44)
    /// so the texture's x axis runs along the map x axis (screen down-right) and its y axis along map y (down-left).
    /// The art of the land tile should be the same picture as its texture seen this way - otherwise flat and stretched
    /// ground look different.
    /// </summary>
    public static class LandTextureMath
    {
        public const int LandSize = 44;

        /// <summary>First x and number of pixels of a diamond row (same as art.mul land encoding).</summary>
        public static void LandRow(int y, out int startX, out int count)
        {
            if (y < 22)
            {
                startX = 21 - y;
                count = 2 + (2 * y);
            }
            else
            {
                startX = y - 22;
                count = 44 - (2 * (y - 22));
            }
        }

        public static bool InsideLand(int x, int y)
        {
            if (y < 0 || y >= LandSize)
            {
                return false;
            }

            LandRow(y, out int start, out int count);
            return x >= start && x < start + count;
        }

        /// <summary>Land tile (44x44, outside the diamond = 0) from a square texture (client orientation, bilinear).</summary>
        public static ushort[] LandFromTexture(ushort[] texture, int size)
        {
            var dst = new ushort[LandSize * LandSize];
            for (int y = 0; y < LandSize; ++y)
            {
                LandRow(y, out int startX, out int count);
                for (int x = startX; x < startX + count; ++x)
                {
                    double cx = x + 0.5 - 22.0;
                    double cy = y + 0.5 - 22.0;
                    double u = ((cx + cy) / 44.0) + 0.5;
                    double v = ((cy - cx) / 44.0) + 0.5;
                    dst[(y * LandSize) + x] = (ushort)(SampleSquare(texture, size, (u * size) - 0.5, (v * size) - 0.5) | 0x8000);
                }
            }

            return dst;
        }

        /// <summary>Texture (size x size) from a land tile - the inverse of <see cref="LandFromTexture"/>.</summary>
        public static ushort[] TextureFromLand(ushort[] land, int size)
        {
            var dst = new ushort[size * size];
            for (int j = 0; j < size; ++j)
            {
                for (int i = 0; i < size; ++i)
                {
                    double u = (i + 0.5) / size;
                    double v = (j + 0.5) / size;
                    double cx = (u - v) * 22.0;
                    double cy = ((u + v) * 22.0) - 22.0;
                    dst[(j * size) + i] = (ushort)(SampleLand(land, cx + 22.0 - 0.5, cy + 22.0 - 0.5) | 0x8000);
                }
            }

            return dst;
        }

        /// <summary>How similar two land tiles are inside the diamond, 0 - 100 %.</summary>
        public static double MatchPercent(ushort[] a, ushort[] b)
        {
            if (a == null || b == null || a.Length < LandSize * LandSize || b.Length < LandSize * LandSize)
            {
                return 0;
            }

            double diff = 0;
            int n = 0;
            for (int y = 0; y < LandSize; ++y)
            {
                LandRow(y, out int start, out int count);
                for (int x = start; x < start + count; ++x)
                {
                    ushort p = a[(y * LandSize) + x], q = b[(y * LandSize) + x];
                    diff += Math.Abs(((p >> 10) & 31) - ((q >> 10) & 31)) + Math.Abs(((p >> 5) & 31) - ((q >> 5) & 31)) + Math.Abs((p & 31) - (q & 31));
                    ++n;
                }
            }

            return n == 0 ? 0 : 100.0 * (1.0 - (diff / (n * 3.0 * 31.0)));
        }

        /// <summary>
        /// Best orientation of a texture for a land tile: tries the 8 turns / mirrors and returns the one whose land
        /// tile looks most like <paramref name="land"/> (0 = as it is, 1-3 = turned 90/180/270 clockwise,
        /// 4-7 = mirrored left/right and then turned).
        /// </summary>
        public static int BestOrientation(ushort[] texture, int size, ushort[] land, out double bestMatch)
        {
            int best = 0;
            bestMatch = -1;
            for (int o = 0; o < 8; ++o)
            {
                double m = MatchPercent(LandFromTexture(Orient(texture, size, o), size), land);
                if (m > bestMatch + 0.01)
                {
                    bestMatch = m;
                    best = o;
                }
            }

            return best;
        }

        /// <summary>Orientation 0-7 (see <see cref="BestOrientation"/>).</summary>
        public static ushort[] Orient(ushort[] px, int size, int orientation)
        {
            ushort[] r = orientation >= 4 ? Mirror(px, size, true) : (ushort[])px.Clone();
            for (int i = 0; i < orientation % 4; ++i)
            {
                r = Rotate90(r, size, true);
            }

            return r;
        }

        public static ushort[] Rotate90(ushort[] px, int size, bool clockwise)
        {
            var dst = new ushort[px.Length];
            for (int y = 0; y < size; ++y)
            {
                for (int x = 0; x < size; ++x)
                {
                    int nx = clockwise ? size - 1 - y : y;
                    int ny = clockwise ? x : size - 1 - x;
                    dst[(ny * size) + nx] = px[(y * size) + x];
                }
            }

            return dst;
        }

        public static ushort[] Mirror(ushort[] px, int size, bool horizontal)
        {
            var dst = new ushort[px.Length];
            for (int y = 0; y < size; ++y)
            {
                for (int x = 0; x < size; ++x)
                {
                    int nx = horizontal ? size - 1 - x : x;
                    int ny = horizontal ? y : size - 1 - y;
                    dst[(ny * size) + nx] = px[(y * size) + x];
                }
            }

            return dst;
        }

        /// <summary>Cuts a land tile to the diamond (pixels outside = 0, inside opaque).</summary>
        public static ushort[] CutToDiamond(ushort[] px)
        {
            var dst = new ushort[LandSize * LandSize];
            for (int y = 0; y < LandSize; ++y)
            {
                LandRow(y, out int start, out int count);
                for (int x = start; x < start + count; ++x)
                {
                    dst[(y * LandSize) + x] = (ushort)(px[(y * LandSize) + x] | 0x8000);
                }
            }

            return dst;
        }

        /// <summary>
        /// Empty / one colour / few colours / normal. For land tiles only the diamond counts. Pure black (0x0000 or
        /// 0x8000) counts as "nothing".
        /// </summary>
        public static TerrainImageKind Classify(ushort[] px, int width, int height, bool land, out int colours)
        {
            var set = new HashSet<ushort>();
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    if (land && !InsideLand(x, y))
                    {
                        continue;
                    }

                    ushort p = (ushort)(px[(y * width) + x] & 0x7FFF);
                    if (p != 0)
                    {
                        set.Add(p);
                        if (set.Count > 4)
                        {
                            colours = set.Count;
                            return TerrainImageKind.Normal;
                        }
                    }
                }
            }

            colours = set.Count;
            return set.Count == 0 ? TerrainImageKind.Empty : set.Count == 1 ? TerrainImageKind.SingleColour : TerrainImageKind.FewColours;
        }

        /// <summary>Average visible colour as 15-bit value (for the radar colour).</summary>
        public static ushort Average(ushort[] px)
        {
            long r = 0, g = 0, b = 0, n = 0;
            foreach (ushort p in px)
            {
                if ((p & 0x7FFF) == 0)
                {
                    continue;
                }

                r += (p >> 10) & 31;
                g += (p >> 5) & 31;
                b += p & 31;
                ++n;
            }

            return n == 0 ? (ushort)0 : (ushort)(((r / n) << 10) | ((g / n) << 5) | (b / n));
        }

        private static ushort SampleSquare(ushort[] src, int size, double fx, double fy)
        {
            fx = Math.Clamp(fx, 0, size - 1);
            fy = Math.Clamp(fy, 0, size - 1);
            int x0 = (int)fx, y0 = (int)fy;
            int x1 = Math.Min(x0 + 1, size - 1), y1 = Math.Min(y0 + 1, size - 1);
            double tx = fx - x0, ty = fy - y0;
            return Mix(src[(y0 * size) + x0], src[(y0 * size) + x1], src[(y1 * size) + x0], src[(y1 * size) + x1], tx, ty);
        }

        /// <summary>Bilinear inside the diamond; neighbours outside are left out (weights renormalised).</summary>
        private static ushort SampleLand(ushort[] land, double fx, double fy)
        {
            int x0 = (int)Math.Floor(fx), y0 = (int)Math.Floor(fy);
            double tx = fx - x0, ty = fy - y0;
            double r = 0, g = 0, b = 0, w = 0;
            for (int dy = 0; dy <= 1; ++dy)
            {
                for (int dx = 0; dx <= 1; ++dx)
                {
                    int x = x0 + dx, y = y0 + dy;
                    if (!InsideLand(x, y))
                    {
                        continue;
                    }

                    double weight = (dx == 0 ? 1 - tx : tx) * (dy == 0 ? 1 - ty : ty);
                    if (weight <= 0)
                    {
                        continue;
                    }

                    ushort p = land[(y * LandSize) + x];
                    r += ((p >> 10) & 31) * weight;
                    g += ((p >> 5) & 31) * weight;
                    b += (p & 31) * weight;
                    w += weight;
                }
            }

            if (w <= 0)
            {
                // outside: nearest pixel of the diamond towards the centre
                double cx = 21.5, cy = 21.5;
                for (int step = 1; step <= 44; ++step)
                {
                    double t = step / 44.0;
                    int x = (int)Math.Round(fx + ((cx - fx) * t)), y = (int)Math.Round(fy + ((cy - fy) * t));
                    if (InsideLand(x, y))
                    {
                        return (ushort)(land[(y * LandSize) + x] & 0x7FFF);
                    }
                }

                return 0;
            }

            return (ushort)(((int)Math.Round(r / w) << 10) | ((int)Math.Round(g / w) << 5) | (int)Math.Round(b / w));
        }

        private static ushort Mix(ushort c00, ushort c10, ushort c01, ushort c11, double tx, double ty)
        {
            int Ch(int s)
            {
                double a = (c00 >> s) & 31, b = (c10 >> s) & 31, c = (c01 >> s) & 31, d = (c11 >> s) & 31;
                double top = a + ((b - a) * tx), bottom = c + ((d - c) * tx);
                return (int)Math.Round(top + ((bottom - top) * ty));
            }

            return (ushort)((Ch(10) << 10) | (Ch(5) << 5) | Ch(0));
        }
    }
}

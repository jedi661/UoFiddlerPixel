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

namespace UoFiddler.Plugin.Enhanced.EcUltima.Art
{
    /// <summary>How classic art is enlarged to the EC size (1.5 times).</summary>
    public enum ArtScaleMode
    {
        /// <summary>Scale3x (edge aware, keeps the pixel art look without blur), then halved – clean edges.</summary>
        Smooth = 0,

        /// <summary>Nearest pixel – hard, every pixel stays its color (1 or 2 pixels wide).</summary>
        Sharp = 1,

        /// <summary>Bilinear – soft, slightly blurred.</summary>
        Soft = 2
    }

    /// <summary>
    /// Enlarges BGRA images (classic art → EC size 1.5 x). Transparent pixels (alpha 0) stay transparent,
    /// edges are not mixed with the transparent color.
    /// </summary>
    public static class EcArtScaler
    {
        public const double EcScale = 1.5;

        /// <summary>Size of the EC image for a classic size.</summary>
        public static int ScaledSize(int size) => Math.Max(1, (int)Math.Round(size * EcScale, MidpointRounding.AwayFromZero));

        public static byte[] Scale(byte[] bgra, int width, int height, ArtScaleMode mode, out int newWidth, out int newHeight)
        {
            newWidth = ScaledSize(width);
            newHeight = ScaledSize(height);
            switch (mode)
            {
                case ArtScaleMode.Sharp:
                    return Nearest(bgra, width, height, newWidth, newHeight);
                case ArtScaleMode.Soft:
                    return Bilinear(bgra, width, height, newWidth, newHeight);
                default:
                    byte[] x3 = Scale3x(bgra, width, height);
                    return Downsample(x3, width * 3, height * 3, newWidth, newHeight);
            }
        }

        private static uint Pixel(byte[] p, int w, int h, int x, int y)
        {
            x = Math.Max(0, Math.Min(w - 1, x));
            y = Math.Max(0, Math.Min(h - 1, y));
            return BitConverter.ToUInt32(p, ((y * w) + x) * 4);
        }

        /// <summary>Scale3x (AdvMAME3x): every pixel becomes 3 x 3, edges follow the neighbours.</summary>
        public static byte[] Scale3x(byte[] src, int w, int h)
        {
            int w3 = w * 3;
            var dst = new byte[w3 * h * 3 * 4];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    uint a = Pixel(src, w, h, x - 1, y - 1), b = Pixel(src, w, h, x, y - 1), c = Pixel(src, w, h, x + 1, y - 1);
                    uint d = Pixel(src, w, h, x - 1, y), e = Pixel(src, w, h, x, y), f = Pixel(src, w, h, x + 1, y);
                    uint g = Pixel(src, w, h, x - 1, y + 1), hh = Pixel(src, w, h, x, y + 1), i = Pixel(src, w, h, x + 1, y + 1);

                    uint e0 = e, e1 = e, e2 = e, e3 = e, e4 = e, e5 = e, e6 = e, e7 = e, e8 = e;
                    if (b != hh && d != f)
                    {
                        e0 = d == b ? d : e;
                        e1 = (d == b && e != c) || (b == f && e != a) ? b : e;
                        e2 = b == f ? f : e;
                        e3 = (d == b && e != g) || (d == hh && e != a) ? d : e;
                        e5 = (b == f && e != i) || (hh == f && e != c) ? f : e;
                        e6 = d == hh ? d : e;
                        e7 = (d == hh && e != i) || (hh == f && e != g) ? hh : e;
                        e8 = hh == f ? f : e;
                    }
                    int ox = x * 3, oy = y * 3;
                    Put(dst, w3, ox, oy, e0); Put(dst, w3, ox + 1, oy, e1); Put(dst, w3, ox + 2, oy, e2);
                    Put(dst, w3, ox, oy + 1, e3); Put(dst, w3, ox + 1, oy + 1, e4); Put(dst, w3, ox + 2, oy + 1, e5);
                    Put(dst, w3, ox, oy + 2, e6); Put(dst, w3, ox + 1, oy + 2, e7); Put(dst, w3, ox + 2, oy + 2, e8);
                }
            }
            return dst;
        }

        private static void Put(byte[] p, int w, int x, int y, uint v)
        {
            int o = ((y * w) + x) * 4;
            p[o] = (byte)v;
            p[o + 1] = (byte)(v >> 8);
            p[o + 2] = (byte)(v >> 16);
            p[o + 3] = (byte)(v >> 24);
        }

        /// <summary>Area average (alpha weighted, so transparent pixels don't darken the edges).</summary>
        public static byte[] Downsample(byte[] src, int w, int h, int nw, int nh)
        {
            var dst = new byte[nw * nh * 4];
            double sx = (double)w / nw, sy = (double)h / nh;
            for (int y = 0; y < nh; y++)
            {
                double y0 = y * sy, y1 = (y + 1) * sy;
                for (int x = 0; x < nw; x++)
                {
                    double x0 = x * sx, x1 = (x + 1) * sx;
                    double r = 0, g = 0, b = 0, a = 0, area = 0;
                    for (int yy = (int)y0; yy < Math.Min(h, (int)Math.Ceiling(y1)); yy++)
                    {
                        double wy = Math.Min(y1, yy + 1) - Math.Max(y0, yy);
                        for (int xx = (int)x0; xx < Math.Min(w, (int)Math.Ceiling(x1)); xx++)
                        {
                            double wx = Math.Min(x1, xx + 1) - Math.Max(x0, xx);
                            double weight = wx * wy;
                            int o = ((yy * w) + xx) * 4;
                            double pa = src[o + 3] / 255.0 * weight;
                            b += src[o] * pa;
                            g += src[o + 1] * pa;
                            r += src[o + 2] * pa;
                            a += pa;
                            area += weight;
                        }
                    }
                    int d = ((y * nw) + x) * 4;
                    double alpha = area > 0 ? a / area : 0;
                    // classic art has only opaque / transparent pixels: keep it that way (no half transparent rims)
                    if (alpha >= 0.5 && a > 0)
                    {
                        dst[d] = (byte)Math.Round(b / a);
                        dst[d + 1] = (byte)Math.Round(g / a);
                        dst[d + 2] = (byte)Math.Round(r / a);
                        dst[d + 3] = 255;
                    }
                }
            }
            return dst;
        }

        public static byte[] Nearest(byte[] src, int w, int h, int nw, int nh)
        {
            var dst = new byte[nw * nh * 4];
            for (int y = 0; y < nh; y++)
            {
                int sy = Math.Min(h - 1, y * h / nh);
                for (int x = 0; x < nw; x++)
                {
                    int sx = Math.Min(w - 1, x * w / nw);
                    Buffer.BlockCopy(src, ((sy * w) + sx) * 4, dst, ((y * nw) + x) * 4, 4);
                }
            }
            return dst;
        }

        public static byte[] Bilinear(byte[] src, int w, int h, int nw, int nh)
        {
            var dst = new byte[nw * nh * 4];
            for (int y = 0; y < nh; y++)
            {
                double fy = ((y + 0.5) * h / nh) - 0.5;
                int y0 = (int)Math.Floor(fy);
                double ty = fy - y0;
                for (int x = 0; x < nw; x++)
                {
                    double fx = ((x + 0.5) * w / nw) - 0.5;
                    int x0 = (int)Math.Floor(fx);
                    double tx = fx - x0;
                    double r = 0, g = 0, b = 0, a = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        int xx = x0 + (k & 1), yy = y0 + (k >> 1);
                        double weight = ((k & 1) == 0 ? 1 - tx : tx) * ((k >> 1) == 0 ? 1 - ty : ty);
                        uint p = Pixel(src, w, h, xx, yy);
                        double pa = (p >> 24) / 255.0 * weight;
                        b += (p & 0xFF) * pa;
                        g += ((p >> 8) & 0xFF) * pa;
                        r += ((p >> 16) & 0xFF) * pa;
                        a += pa;
                    }
                    int d = ((y * nw) + x) * 4;
                    if (a >= 0.5)
                    {
                        dst[d] = (byte)Math.Round(b / a);
                        dst[d + 1] = (byte)Math.Round(g / a);
                        dst[d + 2] = (byte)Math.Round(r / a);
                        dst[d + 3] = 255;
                    }
                }
            }
            return dst;
        }

        /// <summary>Puts the image top left into a power of two sized image (like the client textures); returns the new size.</summary>
        public static byte[] PadPowerOfTwo(byte[] bgra, int w, int h, out int pw, out int ph)
        {
            pw = NextPow2(Math.Max(w, 4));
            ph = NextPow2(Math.Max(h, 4));
            var dst = new byte[pw * ph * 4];
            for (int y = 0; y < h; y++)
            {
                Buffer.BlockCopy(bgra, y * w * 4, dst, y * pw * 4, w * 4);
            }
            return dst;
        }

        public static int NextPow2(int v)
        {
            int p = 1;
            while (p < v)
            {
                p <<= 1;
            }
            return p;
        }

        /// <summary>Smallest rectangle with visible pixels (x, y, w, h); w = 0 when everything is transparent.</summary>
        public static (int X, int Y, int W, int H) UsedArea(byte[] bgra, int w, int h)
        {
            int minX = w, minY = h, maxX = -1, maxY = -1;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (bgra[(((y * w) + x) * 4) + 3] != 0)
                    {
                        minX = Math.Min(minX, x);
                        maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y);
                        maxY = Math.Max(maxY, y);
                    }
                }
            }
            return maxX < 0 ? (0, 0, 0, 0) : (minX, minY, maxX - minX + 1, maxY - minY + 1);
        }
    }
}

/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Animations
{
    /// <summary>
    /// One frame of an Enhanced Client animation. Pixels are palette indices with a coverage
    /// (16 = opaque, 1..15 = partly covered edge pixel, 0 = transparent).
    /// </summary>
    public sealed class EcAnimationFrame
    {
        /// <summary>Action id (the same for all frames of a file).</summary>
        public ushort ActionId { get; set; }

        /// <summary>1 based position in the grid of 5 directions x frames per direction.</summary>
        public ushort Number { get; set; }

        /// <summary>Bounding box relative to the tile center (X2/Y2 exclusive).</summary>
        public short X1 { get; set; }
        public short Y1 { get; set; }
        public short X2 { get; set; }
        public short Y2 { get; set; }

        public int Width => X2 - X1;
        public int Height => Y2 - Y1;

        /// <summary>Palette index per pixel (row by row).</summary>
        public byte[] Indices { get; set; } = Array.Empty<byte>();

        /// <summary>Coverage per pixel: 0 transparent, 1..15 edge, 16 opaque.</summary>
        public byte[] Coverage { get; set; } = Array.Empty<byte>();

        /// <summary>A frame entry without pixel data (offset 0 in the file).</summary>
        public bool IsEmpty => Indices.Length == 0;
    }

    /// <summary>
    /// Enhanced Client animation of one body and action ("AMOU" format, AnimationFrame1-6.uop,
    /// "build/animationframe/{body:D6}/{action:D2}.bin"; also Paperdoll.uop).
    ///
    /// Layout: "AMOU", uint version, uint length, uint body id, short x1, y1, x2, y2 (bounds of all frames),
    /// uint color count, uint color table offset (40), uint frame count, uint frame table offset,
    /// colors (R, G, B, A bytes), frame table (16 bytes each: ushort action, ushort number, short x1, y1, x2, y2,
    /// uint data offset relative to the table entry), pixel data.
    ///
    /// Pixel data is one stream over all rows: a byte below 128 skips that many transparent pixels; a byte n >= 128
    /// starts a packet with n - 128 opaque pixels, followed by a byte with two coverage values (high nibble: one edge
    /// pixel before the opaque pixels, low nibble: one edge pixel after them), then the palette indices. Packets never
    /// go past the end of a row. Writing reproduces the original files byte by byte.
    /// </summary>
    public sealed class EcAnimation
    {
        public const uint Magic = 0x554F4D41; // "AMOU"

        public uint Version { get; set; } = 1;
        public uint BodyId { get; set; }
        public short X1 { get; set; }
        public short Y1 { get; set; }
        public short X2 { get; set; }
        public short Y2 { get; set; }

        /// <summary>Palette as R, G, B, A (4 bytes per color).</summary>
        public List<byte[]> Colors { get; } = new List<byte[]>();

        public List<EcAnimationFrame> Frames { get; } = new List<EcAnimationFrame>();

        public ushort ActionId => Frames.Count > 0 ? Frames[0].ActionId : (ushort)0;

        public static bool IsAnimation(byte[] data) => data != null && data.Length >= 40 && BitConverter.ToUInt32(data, 0) == Magic;

        public static EcAnimation Read(byte[] data)
        {
            if (!IsAnimation(data))
            {
                throw new InvalidDataException("No AMOU animation.");
            }

            var a = new EcAnimation
            {
                Version = BitConverter.ToUInt32(data, 4),
                BodyId = BitConverter.ToUInt32(data, 12),
                X1 = BitConverter.ToInt16(data, 16),
                Y1 = BitConverter.ToInt16(data, 18),
                X2 = BitConverter.ToInt16(data, 20),
                Y2 = BitConverter.ToInt16(data, 22)
            };
            int colorCount = BitConverter.ToInt32(data, 24);
            int colorOffset = BitConverter.ToInt32(data, 28);
            int frameCount = BitConverter.ToInt32(data, 32);
            int frameOffset = BitConverter.ToInt32(data, 36);

            for (int i = 0; i < colorCount; i++)
            {
                var c = new byte[4];
                Buffer.BlockCopy(data, colorOffset + (i * 4), c, 0, 4);
                a.Colors.Add(c);
            }

            for (int i = 0; i < frameCount; i++)
            {
                int p = frameOffset + (i * 16);
                var frame = new EcAnimationFrame
                {
                    ActionId = BitConverter.ToUInt16(data, p),
                    Number = BitConverter.ToUInt16(data, p + 2),
                    X1 = BitConverter.ToInt16(data, p + 4),
                    Y1 = BitConverter.ToInt16(data, p + 6),
                    X2 = BitConverter.ToInt16(data, p + 8),
                    Y2 = BitConverter.ToInt16(data, p + 10)
                };
                uint relative = BitConverter.ToUInt32(data, p + 12);
                if (relative != 0)
                {
                    DecodePixels(data, p + (int)relative, frame);
                }
                a.Frames.Add(frame);
            }
            return a;
        }

        private static void DecodePixels(byte[] data, int position, EcAnimationFrame frame)
        {
            int count = Math.Max(0, frame.Width) * Math.Max(0, frame.Height);
            frame.Indices = new byte[count];
            frame.Coverage = new byte[count];
            int p = 0;
            int i = position;
            while (p < count)
            {
                byte b = data[i++];
                if (b < 128)
                {
                    p += b;
                    continue;
                }

                byte nibbles = data[i++];
                int before = nibbles >> 4;
                int after = nibbles & 15;
                if (before > 0)
                {
                    Set(frame, p++, data[i++], (byte)before);
                }
                for (int k = 0; k < b - 128; k++)
                {
                    Set(frame, p++, data[i++], 16);
                }
                if (after > 0)
                {
                    Set(frame, p++, data[i++], (byte)after);
                }
            }
        }

        private static void Set(EcAnimationFrame frame, int p, byte index, byte coverage)
        {
            if (p < frame.Indices.Length)
            {
                frame.Indices[p] = index;
                frame.Coverage[p] = coverage;
            }
        }

        public byte[] Write()
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(new byte[40]);
                int colorOffset = (int)ms.Position;
                foreach (byte[] c in Colors)
                {
                    w.Write(c, 0, 4);
                }

                int frameOffset = (int)ms.Position;
                w.Write(new byte[16 * Frames.Count]);

                var offsets = new uint[Frames.Count];
                for (int i = 0; i < Frames.Count; i++)
                {
                    if (Frames[i].IsEmpty)
                    {
                        continue;
                    }
                    long position = ms.Position;
                    w.Write(EncodePixels(Frames[i]));
                    offsets[i] = (uint)(position - (frameOffset + (i * 16)));
                }
                long length = ms.Position;

                ms.Seek(frameOffset, SeekOrigin.Begin);
                for (int i = 0; i < Frames.Count; i++)
                {
                    EcAnimationFrame f = Frames[i];
                    w.Write(f.ActionId);
                    w.Write(f.Number);
                    w.Write(f.X1);
                    w.Write(f.Y1);
                    w.Write(f.X2);
                    w.Write(f.Y2);
                    w.Write(offsets[i]);
                }

                ms.Seek(0, SeekOrigin.Begin);
                w.Write(Magic);
                w.Write(Version);
                w.Write((uint)length);
                w.Write(BodyId);
                w.Write(X1);
                w.Write(Y1);
                w.Write(X2);
                w.Write(Y2);
                w.Write((uint)Colors.Count);
                w.Write((uint)colorOffset);
                w.Write((uint)Frames.Count);
                w.Write((uint)frameOffset);
                w.Flush();
                return ms.ToArray();
            }
        }

        private static byte[] EncodePixels(EcAnimationFrame frame)
        {
            int width = Math.Max(1, frame.Width);
            int count = frame.Indices.Length;
            var output = new List<byte>(count);
            var full = new List<byte>(127);
            int p = 0;

            while (p < count)
            {
                if (frame.Coverage[p] == 0)
                {
                    int q = p;
                    while (q < count && frame.Coverage[q] == 0)
                    {
                        q++;
                    }
                    int run = q - p;
                    while (run > 0)
                    {
                        int k = Math.Min(run, 127);
                        output.Add((byte)k);
                        run -= k;
                    }
                    p = q;
                    continue;
                }

                int rowEnd = ((p / width) + 1) * width;
                int before = 0, after = 0;
                byte first = 0, last = 0;
                full.Clear();

                if (frame.Coverage[p] < 16)
                {
                    if (p == rowEnd - 1)
                    {
                        // a single edge pixel at the end of a row is written as "after"
                        output.Add(128);
                        output.Add(frame.Coverage[p]);
                        output.Add(frame.Indices[p]);
                        p++;
                        continue;
                    }
                    before = frame.Coverage[p];
                    first = frame.Indices[p];
                    p++;
                }

                while (p < rowEnd && frame.Coverage[p] == 16 && full.Count < 127)
                {
                    full.Add(frame.Indices[p]);
                    p++;
                }

                if (full.Count < 127 && p < rowEnd && frame.Coverage[p] > 0 && frame.Coverage[p] < 16)
                {
                    after = frame.Coverage[p];
                    last = frame.Indices[p];
                    p++;
                }

                output.Add((byte)(128 + full.Count));
                output.Add((byte)((before << 4) | after));
                if (before > 0)
                {
                    output.Add(first);
                }
                output.AddRange(full);
                if (after > 0)
                {
                    output.Add(last);
                }
            }

            return output.ToArray();
        }

        // ------------------------------------------------------------------ images

        /// <summary>
        /// Frame as BGRA pixels (edge pixels get alpha = coverage * 16).
        /// </summary>
        public byte[] ToBgra(EcAnimationFrame frame)
        {
            var bgra = new byte[frame.Indices.Length * 4];
            for (int i = 0; i < frame.Indices.Length; i++)
            {
                int coverage = frame.Coverage[i];
                if (coverage == 0)
                {
                    continue;
                }
                byte[] c = frame.Indices[i] < Colors.Count ? Colors[frame.Indices[i]] : new byte[] { 255, 0, 255, 255 };
                bgra[(i * 4) + 0] = c[2];
                bgra[(i * 4) + 1] = c[1];
                bgra[(i * 4) + 2] = c[0];
                bgra[(i * 4) + 3] = coverage >= 16 ? (byte)255 : (byte)(coverage * 16);
            }
            return bgra;
        }

        /// <summary>
        /// Replaces the pixels of a frame with an image (BGRA). Colors are mapped to the nearest palette entry;
        /// alpha becomes the coverage (0 = transparent, 255 = opaque, in between = edge pixel).
        /// The frame bounds keep their position; the size is taken from the image.
        /// </summary>
        public void SetFrameImage(EcAnimationFrame frame, byte[] bgra, int width, int height, short? x1 = null, short? y1 = null)
        {
            if (Colors.Count == 0)
            {
                BuildPalette(new[] { bgra });
            }

            frame.X1 = x1 ?? frame.X1;
            frame.Y1 = y1 ?? frame.Y1;
            frame.X2 = (short)(frame.X1 + width);
            frame.Y2 = (short)(frame.Y1 + height);
            frame.Indices = new byte[width * height];
            frame.Coverage = new byte[width * height];

            var cache = new Dictionary<int, byte>();
            for (int i = 0; i < width * height; i++)
            {
                byte a = bgra[(i * 4) + 3];
                if (a < 8)
                {
                    continue;
                }
                int key = (bgra[(i * 4) + 2] << 16) | (bgra[(i * 4) + 1] << 8) | bgra[i * 4];
                if (!cache.TryGetValue(key, out byte index))
                {
                    index = NearestColor(bgra[(i * 4) + 2], bgra[(i * 4) + 1], bgra[i * 4]);
                    cache[key] = index;
                }
                frame.Indices[i] = index;
                frame.Coverage[i] = a >= 248 ? (byte)16 : (byte)Math.Max(1, Math.Min(15, (a + 8) / 16));
            }

            UpdateBounds();
        }

        /// <summary>
        /// New palette (up to 256 colors, median cut) for the given BGRA images. Existing frames keep their indices,
        /// so only use this for animations whose frames are all replaced.
        /// </summary>
        public void BuildPalette(IEnumerable<byte[]> images)
        {
            var colors = new List<int>();
            foreach (byte[] img in images)
            {
                for (int i = 0; i + 3 < img.Length; i += 4)
                {
                    if (img[i + 3] >= 8)
                    {
                        colors.Add((img[i + 2] << 16) | (img[i + 1] << 8) | img[i]);
                    }
                }
            }

            Colors.Clear();
            foreach (int c in MedianCut(colors.Distinct().ToList(), 256))
            {
                Colors.Add(new[] { (byte)(c >> 16), (byte)(c >> 8), (byte)c, (byte)255 });
            }
            if (Colors.Count == 0)
            {
                Colors.Add(new byte[] { 0, 0, 0, 255 });
            }
        }

        private static List<int> MedianCut(List<int> colors, int maxColors)
        {
            if (colors.Count <= maxColors)
            {
                return colors;
            }

            var boxes = new List<List<int>> { colors };
            while (boxes.Count < maxColors)
            {
                // split the box with the largest range
                int bestBox = -1, bestRange = -1, bestChannel = 0;
                for (int b = 0; b < boxes.Count; b++)
                {
                    if (boxes[b].Count < 2)
                    {
                        continue;
                    }
                    for (int ch = 0; ch < 3; ch++)
                    {
                        int shift = 16 - (ch * 8);
                        int min = 255, max = 0;
                        foreach (int c in boxes[b])
                        {
                            int v = (c >> shift) & 255;
                            min = Math.Min(min, v);
                            max = Math.Max(max, v);
                        }
                        if (max - min > bestRange)
                        {
                            bestRange = max - min;
                            bestBox = b;
                            bestChannel = shift;
                        }
                    }
                }
                if (bestBox < 0)
                {
                    break;
                }

                List<int> box = boxes[bestBox].OrderBy(c => (c >> bestChannel) & 255).ToList();
                boxes[bestBox] = box.GetRange(0, box.Count / 2);
                boxes.Add(box.GetRange(box.Count / 2, box.Count - (box.Count / 2)));
            }

            return boxes.Where(b => b.Count > 0).Select(b =>
            {
                long r = 0, g = 0, bl = 0;
                foreach (int c in b)
                {
                    r += (c >> 16) & 255;
                    g += (c >> 8) & 255;
                    bl += c & 255;
                }
                return (int)(((r / b.Count) << 16) | ((g / b.Count) << 8) | (bl / b.Count));
            }).ToList();
        }

        private byte NearestColor(int r, int g, int b)
        {
            int best = 0, bestDistance = int.MaxValue;
            for (int i = 0; i < Colors.Count && i < 256; i++)
            {
                int dr = Colors[i][0] - r, dg = Colors[i][1] - g, db = Colors[i][2] - b;
                int d = (dr * dr * 3) + (dg * dg * 4) + (db * db * 2);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = i;
                    if (d == 0)
                    {
                        break;
                    }
                }
            }
            return (byte)best;
        }

        /// <summary>
        /// Sets the animation bounds to the union of all frame bounds.
        /// </summary>
        public void UpdateBounds()
        {
            List<EcAnimationFrame> frames = Frames.Where(f => !f.IsEmpty).ToList();
            if (frames.Count == 0)
            {
                return;
            }
            X1 = frames.Min(f => f.X1);
            Y1 = frames.Min(f => f.Y1);
            X2 = frames.Max(f => f.X2);
            Y2 = frames.Max(f => f.Y2);
        }

        /// <summary>
        /// Suggested number of frames per direction (the frame numbers fill a grid of 5 directions).
        /// </summary>
        public int SuggestedFramesPerDirection()
        {
            int max = Frames.Count == 0 ? 0 : Frames.Max(f => f.Number);
            return Math.Max(1, (max + 4) / 5);
        }
    }
}

// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * "THE WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class
{
    // ════════════════════════════════════════════════════════════════════
    //  VD file layout (same format UoFiddler / UOEC Super Viewer use):
    //
    //    short  6                 file id
    //    short  animType          0 = High (22 actions), 1 = Low (13), 2 = People (35)
    //    Entry  [actions * 5]     int lookup, int length, int extra (-1 = empty)
    //    data   per entry:        ushort[256] palette (15 bit, XOR 0x8000 on read)
    //                             int frameCount, int[frameCount] offsets (relative)
    //                             frames: short cx, short cy, ushort w, ushort h,
    //                                     RLE chunks..., 0x7FFF7FFF
    // ════════════════════════════════════════════════════════════════════

    /// <summary>Animation group stored in a VD file.</summary>
    public enum VdAnimType
    {
        High = 0,
        Low = 1,
        People = 2
    }

    /// <summary>One frame: 32-bit image plus its center point (UO anim center).</summary>
    public sealed class VdFrame : IDisposable
    {
        public Bitmap Image { get; private set; }
        public int CenterX { get; set; }
        public int CenterY { get; set; }

        public int Width => Image?.Width ?? 0;
        public int Height => Image?.Height ?? 0;

        /// <summary>Takes ownership of <paramref name="image"/> (converted to 32bpp ARGB).</summary>
        public VdFrame(Bitmap image, int centerX, int centerY)
        {
            Image = VdAnimationFile.To32Bpp(image ?? new Bitmap(1, 1, PixelFormat.Format32bppArgb), image != null);
            CenterX = centerX;
            CenterY = centerY;
        }

        public VdFrame Clone()
        {
            return new VdFrame(new Bitmap(Image), CenterX, CenterY);
        }

        /// <summary>Replaces the image. The old one is disposed, the new one is owned.</summary>
        public void SetImage(Bitmap image)
        {
            Bitmap old = Image;
            Image = VdAnimationFile.To32Bpp(image, true);
            if (!ReferenceEquals(old, Image))
            {
                old?.Dispose();
            }
        }

        /// <summary>Mirror left/right around the center point (like the client does for directions 5-7).</summary>
        public void MirrorHorizontal()
        {
            Image.RotateFlip(RotateFlipType.RotateNoneFlipX);
            CenterX = Width - CenterX;
        }

        /// <summary>Mirror top/bottom inside the frame bounds.</summary>
        public void MirrorVertical()
        {
            Image.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }

        /// <summary>Left/top draw position of this frame for a given origin.</summary>
        public Point GetDrawPosition(Point origin, bool mirrored)
        {
            int x = mirrored ? origin.X - (Width - CenterX) : origin.X - CenterX;
            int y = origin.Y - CenterY - Height;
            return new Point(x, y);
        }

        public void Dispose()
        {
            Image?.Dispose();
            Image = null;
        }
    }

    /// <summary>All frames of one action in one stored direction (0-4).</summary>
    public sealed class VdEntry : IDisposable
    {
        public List<VdFrame> Frames { get; } = new List<VdFrame>();

        /// <summary>Third int of the index entry, kept as-is.</summary>
        public int Extra { get; set; }

        /// <summary>Palette read from the file. Reused on save if all colors still fit, so unchanged entries stay byte-identical.</summary>
        public ushort[] OriginalPalette { get; set; }

        public bool IsEmpty => Frames.Count == 0;

        public VdEntry Clone()
        {
            var e = new VdEntry { Extra = Extra, OriginalPalette = (ushort[])OriginalPalette?.Clone() };
            foreach (VdFrame f in Frames)
            {
                e.Frames.Add(f.Clone());
            }
            return e;
        }

        public void Clear()
        {
            foreach (VdFrame f in Frames)
            {
                f.Dispose();
            }
            Frames.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }

    /// <summary>Complete in-memory VD file. Load, edit and save.</summary>
    public sealed class VdAnimationFile : IDisposable
    {
        public const int DirectionCount = 5;
        private const int FileId = 6;
        private const int PaletteSize = 256;
        private const int DoubleXor = (0x200 << 22) | (0x200 << 12);
        private const int EndOfFrame = 0x7FFF7FFF;

        public VdAnimType AnimType { get; private set; }
        public int ActionCount => GetActionCount(AnimType);

        /// <summary>[action, direction]</summary>
        public VdEntry[,] Entries { get; private set; }

        /// <summary>Number of entries that had more than 256 colors and were reduced on the last save.</summary>
        public int LastSaveQuantizedEntries { get; private set; }

        public VdAnimationFile(VdAnimType type)
        {
            AnimType = type;
            Entries = new VdEntry[ActionCount, DirectionCount];
            for (int a = 0; a < ActionCount; a++)
            {
                for (int d = 0; d < DirectionCount; d++)
                {
                    Entries[a, d] = new VdEntry();
                }
            }
        }

        public static int GetActionCount(VdAnimType type)
        {
            switch (type)
            {
                case VdAnimType.High: return 22;
                case VdAnimType.Low: return 13;
                default: return 35;
            }
        }

        public VdEntry GetEntry(int action, int dir)
        {
            if (action < 0 || action >= ActionCount || dir < 0 || dir >= DirectionCount)
            {
                return null;
            }
            return Entries[action, dir];
        }

        public bool IsActionEmpty(int action)
        {
            for (int d = 0; d < DirectionCount; d++)
            {
                if (!Entries[action, d].IsEmpty)
                {
                    return false;
                }
            }
            return true;
        }

        public int TotalFrames
        {
            get
            {
                int n = 0;
                foreach (VdEntry e in Entries)
                {
                    n += e.Frames.Count;
                }
                return n;
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  Load
        // ════════════════════════════════════════════════════════════════

        public static VdAnimationFile Load(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                if (data.Length < 4)
                {
                    throw new InvalidDataException("Datei ist zu klein für eine VD-Datei.");
                }

                short id = br.ReadInt16();
                if (id != FileId)
                {
                    throw new InvalidDataException($"Keine gültige VD-Datei (Kennung {id}, erwartet {FileId}).");
                }

                short typeValue = br.ReadInt16();
                VdAnimType type;
                if (typeValue >= 0 && typeValue <= 2)
                {
                    type = (VdAnimType)typeValue;
                }
                else
                {
                    type = GuessType(br, data.Length);
                }

                var file = new VdAnimationFile(type);
                int count = file.ActionCount * DirectionCount;

                if (4 + count * 12 > data.Length)
                {
                    throw new InvalidDataException("Index-Tabelle ist größer als die Datei. Datei beschädigt oder falscher Typ.");
                }

                var lookups = new int[count];
                var lengths = new int[count];
                var extras = new int[count];
                for (int i = 0; i < count; i++)
                {
                    lookups[i] = br.ReadInt32();
                    lengths[i] = br.ReadInt32();
                    extras[i] = br.ReadInt32();
                }

                for (int i = 0; i < count; i++)
                {
                    VdEntry entry = file.Entries[i / DirectionCount, i % DirectionCount];
                    entry.Extra = extras[i] == -1 ? 0 : extras[i];

                    if (lookups[i] <= 0 || lengths[i] <= 0 || lookups[i] >= data.Length)
                    {
                        continue;
                    }

                    try
                    {
                        ReadEntry(br, lookups[i], entry);
                    }
                    catch (Exception ex) when (ex is EndOfStreamException || ex is IOException || ex is ArgumentException)
                    {
                        // a broken entry should not kill the whole file
                        entry.Clear();
                    }
                }

                return file;
            }
        }

        /// <summary>For files with an unknown type value: pick the action count whose index table looks valid.</summary>
        private static VdAnimType GuessType(BinaryReader br, long fileLength)
        {
            foreach (VdAnimType t in new[] { VdAnimType.People, VdAnimType.High, VdAnimType.Low })
            {
                int count = GetActionCount(t) * DirectionCount;
                long tableEnd = 4 + count * 12L;
                if (tableEnd > fileLength)
                {
                    continue;
                }

                br.BaseStream.Seek(4, SeekOrigin.Begin);
                bool ok = true;
                long firstData = long.MaxValue;
                for (int i = 0; i < count && ok; i++)
                {
                    int lookup = br.ReadInt32();
                    int length = br.ReadInt32();
                    br.ReadInt32();
                    if (lookup > 0 && length > 0)
                    {
                        if (lookup < tableEnd || lookup + (long)length > fileLength)
                        {
                            ok = false;
                        }
                        firstData = Math.Min(firstData, lookup);
                    }
                }

                // the first data block starts right behind the index table
                if (ok && (firstData == long.MaxValue || firstData == tableEnd))
                {
                    br.BaseStream.Seek(4, SeekOrigin.Begin);
                    return t;
                }
            }

            throw new InvalidDataException("Unbekannter VD-Animationstyp.");
        }

        private static void ReadEntry(BinaryReader br, int lookup, VdEntry entry)
        {
            br.BaseStream.Seek(lookup, SeekOrigin.Begin);

            var palette = new ushort[PaletteSize];
            for (int p = 0; p < PaletteSize; p++)
            {
                palette[p] = (ushort)(br.ReadUInt16() ^ 0x8000);
            }
            entry.OriginalPalette = palette;

            long start = br.BaseStream.Position;
            int frameCount = br.ReadInt32();
            if (frameCount < 0 || frameCount > 4096)
            {
                throw new InvalidDataException("Ungültige Frame-Anzahl.");
            }

            var offsets = new int[frameCount];
            for (int f = 0; f < frameCount; f++)
            {
                offsets[f] = br.ReadInt32();
            }

            for (int f = 0; f < frameCount; f++)
            {
                br.BaseStream.Seek(start + offsets[f], SeekOrigin.Begin);
                entry.Frames.Add(ReadFrame(br, palette));
            }
        }

        private static VdFrame ReadFrame(BinaryReader br, ushort[] palette)
        {
            int cx = br.ReadInt16();
            int cy = br.ReadInt16();
            int w = br.ReadUInt16();
            int h = br.ReadUInt16();

            if (w == 0 || h == 0)
            {
                return new VdFrame(null, cx, cy);
            }

            var pixels = new int[w * h];
            int xBase = cx - 0x200;
            int yBase = cy + h - 0x200;

            int header;
            while ((header = br.ReadInt32()) != EndOfFrame)
            {
                header ^= DoubleXor;
                int run = header & 0xFFF;
                int offY = (header >> 12) & 0x3FF;
                int offX = (header >> 22) & 0x3FF;

                int x = Wrap(xBase + offX, w);
                int y = Wrap(yBase + offY, h);

                for (int i = 0; i < run; i++)
                {
                    byte idx = br.ReadByte();
                    int px = x + i;
                    if (y >= 0 && y < h && px >= 0 && px < w)
                    {
                        pixels[y * w + px] = Color1555To32(palette[idx]);
                    }
                }
            }

            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            for (int y = 0; y < h; y++)
            {
                Marshal.Copy(pixels, y * w, bd.Scan0 + y * bd.Stride, w);
            }
            bmp.UnlockBits(bd);

            return new VdFrame(bmp, cx, cy);
        }

        /// <summary>The 10-bit offsets wrap at 1024; bring the coordinate back into the frame.</summary>
        private static int Wrap(int v, int size)
        {
            if (v < 0 && v + 1024 < size)
            {
                return v + 1024;
            }
            if (v >= size && v - 1024 >= 0)
            {
                return v - 1024;
            }
            return v;
        }

        // ════════════════════════════════════════════════════════════════
        //  Save
        // ════════════════════════════════════════════════════════════════

        public void Save(string path)
        {
            LastSaveQuantizedEntries = 0;
            int count = ActionCount * DirectionCount;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((short)FileId);
                bw.Write((short)AnimType);

                long indexPos = ms.Position;
                // reserve index table
                bw.Write(new byte[count * 12]);

                var lookups = new int[count];
                var lengths = new int[count];
                var extras = new int[count];

                for (int i = 0; i < count; i++)
                {
                    VdEntry entry = Entries[i / DirectionCount, i % DirectionCount];
                    if (entry.IsEmpty)
                    {
                        lookups[i] = -1;
                        lengths[i] = -1;
                        extras[i] = -1;
                        continue;
                    }

                    long start = ms.Position;
                    WriteEntry(bw, entry);
                    lookups[i] = (int)start;
                    lengths[i] = (int)(ms.Position - start);
                    extras[i] = entry.Extra;
                }

                ms.Position = indexPos;
                for (int i = 0; i < count; i++)
                {
                    bw.Write(lookups[i]);
                    bw.Write(lengths[i]);
                    bw.Write(extras[i]);
                }

                bw.Flush();

                // only touch the target file once everything is encoded
                File.WriteAllBytes(path, ms.ToArray());
            }
        }

        private void WriteEntry(BinaryWriter bw, VdEntry entry)
        {
            // collect all 15-bit colors with their usage
            var usage = new Dictionary<ushort, int>();
            var frameColors = new List<ushort[]>(entry.Frames.Count);
            foreach (VdFrame frame in entry.Frames)
            {
                ushort[] px = GetPixels15(frame.Image);
                frameColors.Add(px);
                foreach (ushort c in px)
                {
                    if (c == Transparent)
                    {
                        continue;
                    }
                    usage.TryGetValue(c, out int n);
                    usage[c] = n + 1;
                }
            }

            ushort[] palette;
            Dictionary<ushort, byte> map;

            if (entry.OriginalPalette != null && usage.Keys.All(c => Array.IndexOf(entry.OriginalPalette, (ushort)(c | 0x8000)) >= 0))
            {
                // unchanged colors: keep the original palette order
                palette = entry.OriginalPalette;
                map = new Dictionary<ushort, byte>();
                foreach (ushort c in usage.Keys)
                {
                    map[c] = (byte)Array.IndexOf(palette, (ushort)(c | 0x8000));
                }
            }
            else if (usage.Count <= PaletteSize)
            {
                palette = new ushort[PaletteSize];
                map = new Dictionary<ushort, byte>();
                int i = 0;
                foreach (ushort c in usage.Keys.OrderByDescending(k => usage[k]))
                {
                    palette[i] = (ushort)(c | 0x8000);
                    map[c] = (byte)i;
                    i++;
                }
            }
            else
            {
                LastSaveQuantizedEntries++;
                List<ushort> reduced = MedianCut(usage, PaletteSize);
                palette = new ushort[PaletteSize];
                for (int i = 0; i < reduced.Count; i++)
                {
                    palette[i] = (ushort)(reduced[i] | 0x8000);
                }
                map = new Dictionary<ushort, byte>();
                foreach (ushort c in usage.Keys)
                {
                    map[c] = NearestIndex(reduced, c);
                }
            }

            for (int p = 0; p < PaletteSize; p++)
            {
                bw.Write((ushort)(palette[p] ^ 0x8000));
            }

            long start = bw.BaseStream.Position;
            bw.Write(entry.Frames.Count);
            long offsetTable = bw.BaseStream.Position;
            bw.Write(new byte[4 * entry.Frames.Count]);

            var offsets = new int[entry.Frames.Count];
            for (int f = 0; f < entry.Frames.Count; f++)
            {
                offsets[f] = (int)(bw.BaseStream.Position - start);
                WriteFrame(bw, entry.Frames[f], frameColors[f], map);
            }

            long end = bw.BaseStream.Position;
            bw.BaseStream.Position = offsetTable;
            foreach (int o in offsets)
            {
                bw.Write(o);
            }
            bw.BaseStream.Position = end;
        }

        private static void WriteFrame(BinaryWriter bw, VdFrame frame, ushort[] px, Dictionary<ushort, byte> map)
        {
            int w = frame.Width;
            int h = frame.Height;
            int cx = frame.CenterX;
            int cy = frame.CenterY;

            bw.Write((short)cx);
            bw.Write((short)cy);
            bw.Write((ushort)w);
            bw.Write((ushort)h);

            for (int y = 0; y < h; y++)
            {
                int x = 0;
                while (x < w)
                {
                    while (x < w && px[y * w + x] == Transparent)
                    {
                        x++;
                    }
                    if (x >= w)
                    {
                        break;
                    }

                    int runStart = x;
                    while (x < w && px[y * w + x] != Transparent && x - runStart < 0xFFF)
                    {
                        x++;
                    }
                    int run = x - runStart;

                    int offX = runStart - cx + 0x200;
                    int offY = y - cy - h + 0x200;
                    // the client reads both offsets as signed 10 bit values relative to the center
                    if (offX < 0 || offX > 0x3FF || offY < 0 || offY > 0x3FF)
                    {
                        throw new InvalidOperationException(
                            $"Frame {w}x{h} mit Center ({cx},{cy}) liegt außerhalb des VD-Bereichs (max. ±512 Pixel vom Center).");
                    }

                    int header = run | ((offY & 0x3FF) << 12) | ((offX & 0x3FF) << 22);
                    bw.Write(header ^ DoubleXor);

                    for (int i = runStart; i < runStart + run; i++)
                    {
                        bw.Write(map[px[y * w + i]]);
                    }
                }
            }

            bw.Write(EndOfFrame);
        }

        // ════════════════════════════════════════════════════════════════
        //  Color helpers
        // ════════════════════════════════════════════════════════════════

        /// <summary>Marker for "not drawn". Real colors are 15 bit, so 0xFFFF never collides.</summary>
        private const ushort Transparent = 0xFFFF;

        private static int Expand5(int v) => (v << 3) | (v >> 2);

        public static int Color1555To32(ushort c)
        {
            int r = Expand5((c >> 10) & 0x1F);
            int g = Expand5((c >> 5) & 0x1F);
            int b = Expand5(c & 0x1F);
            return unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
        }

        /// <summary>Reads a 32-bit image as 15-bit colors. Pixels with alpha &lt; 128 are transparent.</summary>
        private static ushort[] GetPixels15(Bitmap bmp)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            var result = new ushort[w * h];
            var row = new int[w];

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < h; y++)
                {
                    Marshal.Copy(bd.Scan0 + y * bd.Stride, row, 0, w);
                    for (int x = 0; x < w; x++)
                    {
                        int argb = row[x];
                        int a = (argb >> 24) & 0xFF;
                        if (a < 128)
                        {
                            result[y * w + x] = Transparent;
                            continue;
                        }
                        int r = (argb >> 19) & 0x1F;
                        int g = (argb >> 11) & 0x1F;
                        int b = (argb >> 3) & 0x1F;
                        result[y * w + x] = (ushort)((r << 10) | (g << 5) | b);
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }

            return result;
        }

        private static byte NearestIndex(List<ushort> palette, ushort c)
        {
            int r = (c >> 10) & 0x1F, g = (c >> 5) & 0x1F, b = c & 0x1F;
            int best = 0;
            int bestDist = int.MaxValue;
            for (int i = 0; i < palette.Count; i++)
            {
                ushort p = palette[i];
                int dr = ((p >> 10) & 0x1F) - r;
                int dg = ((p >> 5) & 0x1F) - g;
                int db = (p & 0x1F) - b;
                int dist = dr * dr * 3 + dg * dg * 4 + db * db * 2;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = i;
                    if (dist == 0)
                    {
                        break;
                    }
                }
            }
            return (byte)best;
        }

        /// <summary>Reduces a weighted color set to at most <paramref name="max"/> colors.</summary>
        private static List<ushort> MedianCut(Dictionary<ushort, int> usage, int max)
        {
            var boxes = new List<List<KeyValuePair<ushort, int>>> { usage.ToList() };

            while (boxes.Count < max)
            {
                // split the box with the widest channel range
                int bestBox = -1, bestChannel = 0, bestRange = 0;
                for (int i = 0; i < boxes.Count; i++)
                {
                    if (boxes[i].Count < 2)
                    {
                        continue;
                    }
                    for (int ch = 0; ch < 3; ch++)
                    {
                        int shift = ch == 0 ? 10 : ch == 1 ? 5 : 0;
                        int min = 31, mx = 0;
                        foreach (var kv in boxes[i])
                        {
                            int v = (kv.Key >> shift) & 0x1F;
                            if (v < min) min = v;
                            if (v > mx) mx = v;
                        }
                        if (mx - min > bestRange)
                        {
                            bestRange = mx - min;
                            bestBox = i;
                            bestChannel = shift;
                        }
                    }
                }

                if (bestBox < 0)
                {
                    break;
                }

                int s = bestChannel;
                var sorted = boxes[bestBox].OrderBy(kv => (kv.Key >> s) & 0x1F).ToList();
                long total = sorted.Sum(kv => (long)kv.Value);
                long acc = 0;
                int cut = 1;
                for (int i = 0; i < sorted.Count - 1; i++)
                {
                    acc += sorted[i].Value;
                    cut = i + 1;
                    if (acc * 2 >= total)
                    {
                        break;
                    }
                }

                boxes[bestBox] = sorted.Take(cut).ToList();
                boxes.Add(sorted.Skip(cut).ToList());
            }

            var result = new List<ushort>(boxes.Count);
            foreach (var box in boxes)
            {
                long w = 0, r = 0, g = 0, b = 0;
                foreach (var kv in box)
                {
                    w += kv.Value;
                    r += ((kv.Key >> 10) & 0x1F) * (long)kv.Value;
                    g += ((kv.Key >> 5) & 0x1F) * (long)kv.Value;
                    b += (kv.Key & 0x1F) * (long)kv.Value;
                }
                if (w == 0)
                {
                    continue;
                }
                result.Add((ushort)(((r / w) << 10) | ((g / w) << 5) | (b / w)));
            }
            return result;
        }

        /// <summary>Returns a 32bpp ARGB bitmap. Disposes the source if it had to be converted and <paramref name="owns"/> is true.</summary>
        public static Bitmap To32Bpp(Bitmap src, bool owns)
        {
            if (src.PixelFormat == PixelFormat.Format32bppArgb)
            {
                return src;
            }

            var dst = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(dst))
            {
                g.DrawImage(src, new Rectangle(0, 0, src.Width, src.Height));
            }
            if (owns)
            {
                src.Dispose();
            }
            return dst;
        }

        /// <summary>Cuts away transparent borders. Returns the bounds used (or null if the image is empty).</summary>
        public static Rectangle? GetOpaqueBounds(Bitmap bmp)
        {
            int w = bmp.Width, h = bmp.Height;
            int minX = w, minY = h, maxX = -1, maxY = -1;
            var row = new int[w];
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < h; y++)
                {
                    Marshal.Copy(bd.Scan0 + y * bd.Stride, row, 0, w);
                    for (int x = 0; x < w; x++)
                    {
                        if (((row[x] >> 24) & 0xFF) >= 128)
                        {
                            if (x < minX) minX = x;
                            if (x > maxX) maxX = x;
                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                        }
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }

            if (maxX < 0)
            {
                return null;
            }
            return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        // ════════════════════════════════════════════════════════════════
        //  Action names
        // ════════════════════════════════════════════════════════════════

        private static readonly string[] HighNames =
        {
            "Walk", "Idle", "Die1", "Die2", "Attack1", "Attack2", "Attack3",
            "AttackBow", "AttackCrossBow", "AttackThrow", "GetHit", "Pillage",
            "Stomp", "Cast2", "Cast3", "BlockRight", "BlockLeft", "Idle",
            "Fidget", "Fly", "TakeOff", "GetHitInAir"
        };

        private static readonly string[] LowNames =
        {
            "Walk", "Run", "Idle", "Eat", "Alert", "Attack1", "Attack2", "GetHit",
            "Die1", "Idle", "Fidget", "LieDown", "Die2"
        };

        // People names taken from the UOEC Super Viewer (CharActions 0-34)
        private static readonly string[] PeopleNames =
        {
            "Walk", "Walk (With Weapon)", "Run", "Run (With Weapon)",
            "Idle", "Idle (With Weapon)", "Fidget", "Idle - Combat (1H Weapon)",
            "Idle - Combat (2H Weapon)", "Slash Attack (1H Weapon)", "Pierce Attack (1H Weapon)", "Bash Attack (1H Weapon)",
            "Bash Attack (2H Weapon)", "Slash Attack (2H Weapon)", "Pierce Attack (2H Weapon)", "Combat Walk (2H Weapon)",
            "Spell 1", "Spell 2", "Bow Attack", "Crossbow Attack",
            "Get Hit", "Die Backward", "Die Forward", "Walk Mounted",
            "Run Mounted", "Idle Mounted", "Bash Attack Mounted", "Bow Attack Mounted",
            "Crossbow Attack Mounted", "Slash Attack Mounted", "Shield Block", "Punch",
            "Bowing", "Salute (Armed)", "Drinking"
        };

        public string GetActionName(int action)
        {
            string[] names = AnimType == VdAnimType.High ? HighNames : AnimType == VdAnimType.Low ? LowNames : PeopleNames;
            return action >= 0 && action < names.Length ? names[action] : "Action " + action;
        }

        public void Dispose()
        {
            if (Entries == null)
            {
                return;
            }
            foreach (VdEntry e in Entries)
            {
                e.Dispose();
            }
            Entries = null;
        }
    }
}

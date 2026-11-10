using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AltitudeTool
{
    /// <summary>
    /// Land altitudes of a UO map (one sbyte per tile) – the C# replacement of AltitudeTool.exe (Andreas Schneider, 2007).
    /// map*.mul layout: blocks of 8 × 8 tiles, column by column (block index = bx * blockHeight + by),
    /// every block = 4 byte header + 64 × (ushort tile id, sbyte z), cells row by row.
    /// The heightmap image of the original tool: 8 bit gray, 1 pixel per tile, gray = z + 128.
    /// </summary>
    internal sealed class HeightMap
    {
        public const int BlockBytes = 196;
        public const int StaticBytes = 7;
        public const int StaIdxBytes = 12;

        public HeightMap(int width, int height)
        {
            Width = width;
            Height = height;
            Z = new sbyte[width * height];
        }

        public int Width { get; }
        public int Height { get; }
        public int BlockWidth => Width >> 3;
        public int BlockHeight => Height >> 3;

        /// <summary>Altitudes, row by row (index = y * Width + x).</summary>
        public sbyte[] Z { get; }

        /// <summary>Tile ids (only when read from a map, used to write a new map file).</summary>
        public ushort[] Ids { get; set; }

        public sbyte this[int x, int y]
        {
            get => Z[y * Width + x];
            set => Z[y * Width + x] = value;
        }

        public Rectangle Bounds => new Rectangle(0, 0, Width, Height);

        public HeightMap Clone()
        {
            var c = new HeightMap(Width, Height) { Ids = Ids };
            Buffer.BlockCopy(Z, 0, c.Z, 0, Z.Length);
            return c;
        }

        public sbyte[] CopyRegion(Rectangle r)
        {
            var data = new sbyte[r.Width * r.Height];
            for (int y = 0; y < r.Height; y++)
            {
                Buffer.BlockCopy(Z, (r.Y + y) * Width + r.X, data, y * r.Width, r.Width);
            }
            return data;
        }

        public void PasteRegion(Rectangle r, sbyte[] data)
        {
            for (int y = 0; y < r.Height; y++)
            {
                Buffer.BlockCopy(data, y * r.Width, Z, (r.Y + y) * Width + r.X, r.Width);
            }
        }

        public static sbyte Clamp(int z) => (sbyte)(z < -128 ? -128 : z > 127 ? 127 : z);

        // ------------------------------------------------------------------ map sizes

        /// <summary>Usual map sizes in tiles.</summary>
        public static readonly (string Name, int Width, int Height)[] KnownMaps =
        {
            ("Felucca / Trammel (ML+)", 7168, 4096),
            ("Felucca / Trammel (old)", 6144, 4096),
            ("Ilshenar", 2304, 1600),
            ("Malas", 2560, 2048),
            ("Tokuno", 1448, 1448),
            ("Ter Mur", 1280, 4096)
        };

        /// <summary>Guesses the map height in blocks from the size of a map*.mul (0 = unknown).</summary>
        public static int GuessBlockHeight(long fileLength, out string name)
        {
            name = null;
            if (fileLength <= 0 || fileLength % BlockBytes != 0)
            {
                return 0;
            }
            long blocks = fileLength / BlockBytes;
            foreach ((string n, int w, int h) in KnownMaps)
            {
                if ((long)(w >> 3) * (h >> 3) == blocks)
                {
                    name = n;
                    return h >> 3;
                }
            }
            foreach (int h in new[] { 512, 256, 200, 181 })
            {
                if (blocks % h == 0)
                {
                    name = "custom";
                    return h;
                }
            }
            return 0;
        }

        // ------------------------------------------------------------------ reading

        /// <summary>Reads the altitudes (and tile ids) of a map*.mul. blockHeight = map height in blocks (like the original "mapheight").</summary>
        public static HeightMap ReadMul(string path, int blockHeight, IProgress<int> progress, CancellationToken token)
        {
            long length = new FileInfo(path).Length;
            if (blockHeight <= 0)
            {
                throw new ArgumentException("The map height in blocks must be greater than 0.");
            }
            long blocks = length / BlockBytes;
            if (blocks == 0 || blocks % blockHeight != 0)
            {
                throw new InvalidDataException($"The file has {blocks} blocks – that is not a multiple of the map height {blockHeight} blocks. Wrong height or not a map*.mul?");
            }
            int blockWidth = (int)(blocks / blockHeight);
            var map = new HeightMap(blockWidth * 8, blockHeight * 8) { Ids = new ushort[blockWidth * 8 * blockHeight * 8] };
            var column = new byte[BlockBytes * blockHeight];
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1 << 20))
            {
                for (int bx = 0; bx < blockWidth; bx++)
                {
                    token.ThrowIfCancellationRequested();
                    ReadFully(fs, column);
                    for (int by = 0; by < blockHeight; by++)
                    {
                        int o = by * BlockBytes + 4;
                        for (int cy = 0; cy < 8; cy++)
                        {
                            int row = (by * 8 + cy) * map.Width + bx * 8;
                            for (int cx = 0; cx < 8; cx++, o += 3)
                            {
                                map.Ids[row + cx] = (ushort)(column[o] | (column[o + 1] << 8));
                                map.Z[row + cx] = (sbyte)column[o + 2];
                            }
                        }
                    }
                    progress?.Report(bx * 100 / blockWidth);
                }
            }
            return map;
        }

        /// <summary>Reads a map that UoFiddler has loaded (works for map*.mul and map*LegacyMUL.uop).</summary>
        public static HeightMap ReadLoaded(Ultima.Map source, IProgress<int> progress, CancellationToken token)
        {
            Ultima.TileMatrix matrix = source.Tiles;
            int bw = matrix.BlockWidth, bh = matrix.BlockHeight;
            var map = new HeightMap(bw * 8, bh * 8) { Ids = new ushort[bw * 8 * bh * 8] };
            for (int bx = 0; bx < bw; bx++)
            {
                token.ThrowIfCancellationRequested();
                for (int by = 0; by < bh; by++)
                {
                    Ultima.Tile[] tiles = matrix.ReadLandBlockUncached(bx, by);
                    for (int i = 0; i < 64 && i < tiles.Length; i++)
                    {
                        int idx = (by * 8 + (i >> 3)) * map.Width + bx * 8 + (i & 7);
                        map.Ids[idx] = tiles[i].Id;
                        map.Z[idx] = tiles[i].Z;
                    }
                }
                progress?.Report(bx * 100 / bw);
            }
            return map;
        }

        private static void ReadFully(Stream s, byte[] buffer)
        {
            int read = 0;
            while (read < buffer.Length)
            {
                int n = s.Read(buffer, read, buffer.Length - read);
                if (n <= 0)
                {
                    throw new EndOfStreamException();
                }
                read += n;
            }
        }

        // ------------------------------------------------------------------ writing

        /// <summary>
        /// Writes the altitudes into a map file. With <paramref name="patchSource"/> the source map*.mul is copied and only the
        /// z bytes are changed (headers and ids stay exactly as they are); otherwise a new file is written from the ids in memory.
        /// Returns the number of changed tiles.
        /// </summary>
        public int WriteMap(string target, string patchSource, IProgress<int> progress, CancellationToken token)
        {
            int changed = 0;
            int bw = BlockWidth, bh = BlockHeight;
            if (!string.IsNullOrEmpty(patchSource) && !string.Equals(Path.GetFullPath(patchSource), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(patchSource, target, true);
            }
            bool patch = !string.IsNullOrEmpty(patchSource);
            if (!patch && Ids == null)
            {
                throw new InvalidOperationException("No tile ids – a new map file cannot be written.");
            }
            using (var fs = new FileStream(target, patch ? FileMode.Open : FileMode.Create, patch ? FileAccess.ReadWrite : FileAccess.Write, FileShare.None, 1 << 20))
            {
                if (patch && fs.Length < (long)bw * bh * BlockBytes)
                {
                    throw new InvalidDataException("The map file is smaller than the height map.");
                }
                var column = new byte[BlockBytes * bh];
                for (int bx = 0; bx < bw; bx++)
                {
                    token.ThrowIfCancellationRequested();
                    long start = (long)bx * bh * BlockBytes;
                    if (patch)
                    {
                        fs.Position = start;
                        ReadFully(fs, column);
                    }
                    for (int by = 0; by < bh; by++)
                    {
                        int o = by * BlockBytes;
                        if (!patch)
                        {
                            column[o] = column[o + 1] = column[o + 2] = column[o + 3] = 0;
                        }
                        o += 4;
                        for (int cy = 0; cy < 8; cy++)
                        {
                            int row = (by * 8 + cy) * Width + bx * 8;
                            for (int cx = 0; cx < 8; cx++, o += 3)
                            {
                                if (!patch)
                                {
                                    ushort id = Ids[row + cx];
                                    column[o] = (byte)id;
                                    column[o + 1] = (byte)(id >> 8);
                                }
                                byte z = (byte)Z[row + cx];
                                if (column[o + 2] != z)
                                {
                                    changed++;
                                }
                                column[o + 2] = z;
                            }
                        }
                    }
                    fs.Position = start;
                    fs.Write(column, 0, column.Length);
                    progress?.Report(bx * 100 / bw);
                }
            }
            return changed;
        }

        /// <summary>
        /// Moves the statics with the land: every static gets z + (new land z − old land z) of its cell, clamped to −128 … 127
        /// (like "apply" / "modify" of the original tool). The statics file is copied to <paramref name="targetStatics"/> and patched.
        /// Returns the number of moved statics.
        /// </summary>
        public int ShiftStatics(HeightMap original, string staidx, string statics, string targetStatics, IProgress<int> progress, CancellationToken token)
        {
            if (!string.Equals(Path.GetFullPath(statics), Path.GetFullPath(targetStatics), StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(statics, targetStatics, true);
            }
            int moved = 0;
            int bw = BlockWidth, bh = BlockHeight;
            byte[] index = File.ReadAllBytes(staidx);
            using (var fs = new FileStream(targetStatics, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 1 << 16))
            {
                var buffer = new byte[StaticBytes * 1024];
                for (int bx = 0; bx < bw; bx++)
                {
                    token.ThrowIfCancellationRequested();
                    for (int by = 0; by < bh; by++)
                    {
                        long entry = ((long)bx * bh + by) * StaIdxBytes;
                        if (entry + 8 > index.Length)
                        {
                            continue;
                        }
                        int lookup = BitConverter.ToInt32(index, (int)entry);
                        int length = BitConverter.ToInt32(index, (int)entry + 4);
                        if (lookup < 0 || length <= 0 || lookup + (long)length > fs.Length)
                        {
                            continue;
                        }
                        bool any = false;
                        for (int i = 0; i < 64 && !any; i++)
                        {
                            int idx = (by * 8 + (i >> 3)) * Width + bx * 8 + (i & 7);
                            any = Z[idx] != original.Z[idx];
                        }
                        if (!any)
                        {
                            continue;
                        }
                        int count = length / StaticBytes;
                        if (buffer.Length < count * StaticBytes)
                        {
                            buffer = new byte[count * StaticBytes];
                        }
                        fs.Position = lookup;
                        ReadFully(fs, new ArraySegment<byte>(buffer, 0, count * StaticBytes));
                        bool dirty = false;
                        for (int s = 0; s < count; s++)
                        {
                            int o = s * StaticBytes;
                            int x = buffer[o + 2] & 7, y = buffer[o + 3] & 7;
                            int idx = (by * 8 + y) * Width + bx * 8 + x;
                            int delta = Z[idx] - original.Z[idx];
                            if (delta == 0)
                            {
                                continue;
                            }
                            buffer[o + 4] = (byte)Clamp((sbyte)buffer[o + 4] + delta);
                            dirty = true;
                            moved++;
                        }
                        if (dirty)
                        {
                            fs.Position = lookup;
                            fs.Write(buffer, 0, count * StaticBytes);
                        }
                    }
                    progress?.Report(bx * 100 / bw);
                }
            }
            return moved;
        }

        private static void ReadFully(Stream s, ArraySegment<byte> seg)
        {
            int read = 0;
            while (read < seg.Count)
            {
                int n = s.Read(seg.Array, seg.Offset + read, seg.Count - read);
                if (n <= 0)
                {
                    throw new EndOfStreamException();
                }
                read += n;
            }
        }

        // ------------------------------------------------------------------ images

        /// <summary>8 bit gray image like the original tool (gray = z + 128) of a region.</summary>
        public Bitmap ToGrayBitmap(Rectangle r)
        {
            var bmp = new Bitmap(r.Width, r.Height, PixelFormat.Format8bppIndexed);
            ColorPalette pal = bmp.Palette;
            for (int i = 0; i < 256; i++)
            {
                pal.Entries[i] = Color.FromArgb(i, i, i);
            }
            bmp.Palette = pal;
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, r.Width, r.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            try
            {
                var row = new byte[r.Width];
                for (int y = 0; y < r.Height; y++)
                {
                    int src = (r.Y + y) * Width + r.X;
                    for (int x = 0; x < r.Width; x++)
                    {
                        row[x] = (byte)(Z[src + x] + 128);
                    }
                    Marshal.Copy(row, 0, bd.Scan0 + y * bd.Stride, r.Width);
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
            return bmp;
        }

        /// <summary>Raw 8 bit file (gray = z + 128, no header) – for terrain programs.</summary>
        public void SaveRaw(string file, Rectangle r)
        {
            var data = new byte[r.Width * r.Height];
            for (int y = 0; y < r.Height; y++)
            {
                int src = (r.Y + y) * Width + r.X;
                for (int x = 0; x < r.Width; x++)
                {
                    data[y * r.Width + x] = (byte)(Z[src + x] + 128);
                }
            }
            File.WriteAllBytes(file, data);
        }

        /// <summary>
        /// Reads a height image: z = gray − 128 (gray of 8 bit palettes, otherwise the luminance). Pixels with alpha 0 give null
        /// entries in <paramref name="skip"/> (keep the map there).
        /// </summary>
        public static sbyte[] ReadHeightImage(string file, out int width, out int height, out bool[] skip)
        {
            Bitmap bmp;
            using (var ms = new MemoryStream(File.ReadAllBytes(file)))
            using (var tmp = new Bitmap(ms))
            {
                bmp = new Bitmap(tmp.Width, tmp.Height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    g.DrawImage(tmp, new Rectangle(0, 0, tmp.Width, tmp.Height));
                }
            }
            using (bmp)
            {
                width = bmp.Width;
                height = bmp.Height;
                var z = new sbyte[width * height];
                skip = new bool[width * height];
                bool anySkip = false;
                BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    var row = new int[width];
                    for (int y = 0; y < height; y++)
                    {
                        Marshal.Copy(bd.Scan0 + y * bd.Stride, row, 0, width);
                        for (int x = 0; x < width; x++)
                        {
                            int p = row[x];
                            int a = (p >> 24) & 0xFF, r = (p >> 16) & 0xFF, g = (p >> 8) & 0xFF, b = p & 0xFF;
                            int gray = r == g && g == b ? r : (r * 299 + g * 587 + b * 114 + 500) / 1000;
                            z[y * width + x] = (sbyte)(gray - 128);
                            if (a < 128)
                            {
                                skip[y * width + x] = true;
                                anySkip = true;
                            }
                        }
                    }
                }
                finally
                {
                    bmp.UnlockBits(bd);
                }
                if (!anySkip)
                {
                    skip = null;
                }
                return z;
            }
        }

        /// <summary>Puts an image (from <see cref="ReadHeightImage"/>) into the map at x, y. Returns the number of changed tiles.</summary>
        public int Import(sbyte[] z, int w, int h, bool[] skip, int atX, int atY, bool relative)
        {
            int changed = 0;
            for (int y = 0; y < h; y++)
            {
                int my = atY + y;
                if (my < 0 || my >= Height)
                {
                    continue;
                }
                for (int x = 0; x < w; x++)
                {
                    int mx = atX + x;
                    int i = y * w + x;
                    if (mx < 0 || mx >= Width || (skip != null && skip[i]))
                    {
                        continue;
                    }
                    int idx = my * Width + mx;
                    sbyte v = relative ? Clamp(Z[idx] + z[i]) : z[i];
                    if (Z[idx] != v)
                    {
                        Z[idx] = v;
                        changed++;
                    }
                }
            }
            return changed;
        }

        // ------------------------------------------------------------------ statistics

        public int[] Histogram(Rectangle r, out int min, out int max, out double average)
        {
            var hist = new int[256];
            min = 127;
            max = -128;
            long sum = 0;
            for (int y = r.Top; y < r.Bottom; y++)
            {
                int row = y * Width;
                for (int x = r.Left; x < r.Right; x++)
                {
                    int z = Z[row + x];
                    hist[z + 128]++;
                    sum += z;
                    if (z < min)
                    {
                        min = z;
                    }
                    if (z > max)
                    {
                        max = z;
                    }
                }
            }
            long n = (long)r.Width * r.Height;
            average = n > 0 ? sum / (double)n : 0;
            return hist;
        }

        public int CountDifferences(HeightMap other)
        {
            int n = 0;
            for (int i = 0; i < Z.Length; i++)
            {
                if (Z[i] != other.Z[i])
                {
                    n++;
                }
            }
            return n;
        }

        public List<Rectangle> ChangedBlocks(HeightMap original)
        {
            var list = new List<Rectangle>();
            for (int bx = 0; bx < BlockWidth; bx++)
            {
                for (int by = 0; by < BlockHeight; by++)
                {
                    for (int i = 0; i < 64; i++)
                    {
                        int idx = (by * 8 + (i >> 3)) * Width + bx * 8 + (i & 7);
                        if (Z[idx] != original.Z[idx])
                        {
                            list.Add(new Rectangle(bx * 8, by * 8, 8, 8));
                            break;
                        }
                    }
                }
            }
            return list;
        }
    }
}

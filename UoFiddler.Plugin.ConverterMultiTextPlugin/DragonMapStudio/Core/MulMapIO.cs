// /***************************************************************************
//  *
//  * Dragon Map Studio – map / statics files (MUL + UOP) and MUL -> BMP (Dragon_m2b)
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UoFiddler.Plugin.UopPacker.Classes;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>Known client map sizes (for automatic size detection).</summary>
    public sealed class MapSizePreset
    {
        public MapSizePreset(string name, int width, int height)
        {
            Name = name;
            Width = width;
            Height = height;
        }

        public string Name { get; }
        public int Width { get; }
        public int Height { get; }

        public long MulSize => (long)(Width / 8) * (Height / 8) * 196;

        public override string ToString() => $"{Name} ({Width}×{Height})";

        public static readonly MapSizePreset[] All =
        {
            new MapSizePreset("Felucca / Trammel (ML)", 7168, 4096),
            new MapSizePreset("Felucca / Trammel (alt)", 6144, 4096),
            new MapSizePreset("Ilshenar", 2304, 1600),
            new MapSizePreset("Malas", 2560, 2048),
            new MapSizePreset("Tokuno", 1448, 1448),
            new MapSizePreset("Ter Mur", 1280, 4096),
            new MapSizePreset("Test 1024", 1024, 1024),
            new MapSizePreset("Test 512", 512, 512)
        };
    }

    public static class MulMapIO
    {
        private const int BlockSize = 196;

        /// <summary>map#.mul: blocks column by column (x major), each 4 byte header + 64 × (ushort id, sbyte z).</summary>
        public static void WriteMap(string path, int width, int height, ushort[] tiles, sbyte[] z)
        {
            int bw = width / 8, bh = height / 8;
            string tmp = path + ".tmp";
            using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20))
            {
                var block = new byte[BlockSize];
                for (int bx = 0; bx < bw; bx++)
                {
                    for (int by = 0; by < bh; by++)
                    {
                        int o = 4;
                        block[0] = block[1] = block[2] = block[3] = 0;
                        for (int cy = 0; cy < 8; cy++)
                        {
                            int row = (by * 8 + cy) * width + bx * 8;
                            for (int cx = 0; cx < 8; cx++)
                            {
                                ushort id = tiles[row + cx];
                                block[o++] = (byte)id;
                                block[o++] = (byte)(id >> 8);
                                block[o++] = (byte)z[row + cx];
                            }
                        }
                        fs.Write(block, 0, BlockSize);
                    }
                }
            }
            ReplaceFile(tmp, path);
        }

        public static void WriteStatics(string idxPath, string staticsPath, int bw, int bh, List<StaticEntry> statics)
        {
            int blocks = bw * bh;
            var counts = new int[blocks];
            foreach (StaticEntry s in statics)
            {
                counts[(s.X >> 3) * bh + (s.Y >> 3)]++;
            }
            var start = new int[blocks + 1];
            for (int b = 0; b < blocks; b++)
            {
                start[b + 1] = start[b] + counts[b];
            }
            var sorted = new StaticEntry[statics.Count];
            var pos = (int[])start.Clone();
            foreach (StaticEntry s in statics)
            {
                sorted[pos[(s.X >> 3) * bh + (s.Y >> 3)]++] = s;
            }

            string tmpIdx = idxPath + ".tmp", tmpSta = staticsPath + ".tmp";
            using (var idx = new BinaryWriter(new FileStream(tmpIdx, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20)))
            using (var sta = new BinaryWriter(new FileStream(tmpSta, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20)))
            {
                for (int b = 0; b < blocks; b++)
                {
                    int count = counts[b];
                    if (count == 0)
                    {
                        idx.Write(-1);
                        idx.Write(-1);
                        idx.Write(0);
                        continue;
                    }
                    idx.Write((int)sta.BaseStream.Position);
                    idx.Write(count * 7);
                    idx.Write(0);
                    for (int k = start[b]; k < start[b + 1]; k++)
                    {
                        StaticEntry s = sorted[k];
                        sta.Write(s.Id);
                        sta.Write((byte)(s.X & 7));
                        sta.Write((byte)(s.Y & 7));
                        sta.Write(s.Z);
                        sta.Write(s.Hue);
                    }
                }
            }
            ReplaceFile(tmpIdx, idxPath);
            ReplaceFile(tmpSta, staticsPath);
        }

        /// <summary>Reads staidx#.mul / statics#.mul into a list (absolute coordinates).</summary>
        public static List<StaticEntry> ReadStatics(string idxPath, string staticsPath, int width, int height)
        {
            var list = new List<StaticEntry>();
            int bw = width / 8, bh = height / 8;
            using var idx = new BinaryReader(new FileStream(idxPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16));
            byte[] sta = File.ReadAllBytes(staticsPath);
            long blocks = Math.Min((long)bw * bh, idx.BaseStream.Length / 12);
            for (long b = 0; b < blocks; b++)
            {
                int offset = idx.ReadInt32();
                int length = idx.ReadInt32();
                idx.ReadInt32();
                if (offset < 0 || length <= 0 || offset + (long)length > sta.Length)
                {
                    continue;
                }
                int bx = (int)(b / bh), by = (int)(b % bh);
                for (int o = offset; o + 7 <= offset + length; o += 7)
                {
                    list.Add(new StaticEntry
                    {
                        Id = (ushort)(sta[o] | (sta[o + 1] << 8)),
                        X = bx * 8 + (sta[o + 2] & 7),
                        Y = by * 8 + (sta[o + 3] & 7),
                        Z = (sbyte)sta[o + 4],
                        Hue = (ushort)(sta[o + 5] | (sta[o + 6] << 8))
                    });
                }
            }
            return list;
        }

        /// <summary>Packs map#.mul into map#LegacyMUL.uop (UoFiddler UOP packer).</summary>
        public static void WriteUop(string mulPath, string uopPath, int mapNumber)
        {
            ConversionResult r = LegacyMulFileConverter.ToUop(mulPath, null, uopPath, FileType.MapLegacyMul, mapNumber);
            if (r.Warnings.Count > 0)
            {
                throw new IOException("UOP: " + string.Join("; ", r.Warnings));
            }
        }

        /// <summary>Reads map#.mul or map#LegacyMUL.uop.</summary>
        public static void ReadMap(string path, int width, int height, out ushort[] tiles, out sbyte[] z, int mapNumberForUop = -1)
        {
            string mul = path;
            string temp = null;
            if (path.EndsWith(".uop", StringComparison.OrdinalIgnoreCase))
            {
                int num = mapNumberForUop >= 0 ? mapNumberForUop : GuessMapNumber(path);
                temp = Path.Combine(Path.GetTempPath(), $"dms_map{num}_{Guid.NewGuid():N}.mul");
                LegacyMulFileConverter.FromUop(path, temp, null, FileType.MapLegacyMul, num);
                mul = temp;
            }
            try
            {
                int bw = width / 8, bh = height / 8;
                tiles = new ushort[width * height];
                z = new sbyte[width * height];
                using var fs = new FileStream(mul, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20);
                long blocks = Math.Min((long)bw * bh, fs.Length / BlockSize);
                var block = new byte[BlockSize];
                for (long b = 0; b < blocks; b++)
                {
                    if (fs.Read(block, 0, BlockSize) != BlockSize)
                    {
                        break;
                    }
                    int bx = (int)(b / bh), by = (int)(b % bh);
                    int o = 4;
                    for (int cy = 0; cy < 8; cy++)
                    {
                        int row = (by * 8 + cy) * width + bx * 8;
                        for (int cx = 0; cx < 8; cx++)
                        {
                            tiles[row + cx] = (ushort)(block[o] | (block[o + 1] << 8));
                            z[row + cx] = (sbyte)block[o + 2];
                            o += 3;
                        }
                    }
                }
            }
            finally
            {
                if (temp != null && File.Exists(temp))
                {
                    File.Delete(temp);
                }
            }
        }

        public static int GuessMapNumber(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            foreach (char c in name.Replace("map", string.Empty))
            {
                if (char.IsDigit(c))
                {
                    return c - '0';
                }
            }
            return 0;
        }

        /// <summary>
        /// Size of a map file: known client sizes, the preferred size (e.g. the map open in the editor),
        /// a square map, or a common height. Works for MUL and UOP (null if nothing fits).
        /// </summary>
        public static MapSizePreset GuessSize(string path, int preferWidth = 0, int preferHeight = 0, int mapNumber = -1)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            long blocks = BlockCount(path, mapNumber);
            return FromBlocks(blocks, preferWidth, preferHeight);
        }

        /// <summary>Number of 8×8 blocks in map#.mul / map#LegacyMUL.uop.</summary>
        public static long BlockCount(string path, int mapNumber = -1)
        {
            if (!path.EndsWith(".uop", StringComparison.OrdinalIgnoreCase))
            {
                return new FileInfo(path).Length / BlockSize;
            }
            string temp = Path.Combine(Path.GetTempPath(), $"dms_size_{Guid.NewGuid():N}.mul");
            try
            {
                LegacyMulFileConverter.FromUop(path, temp, null, FileType.MapLegacyMul, mapNumber >= 0 ? mapNumber : GuessMapNumber(path));
                return new FileInfo(temp).Length / BlockSize;
            }
            catch (Exception)
            {
                return 0;
            }
            finally
            {
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }
            }
        }

        public static MapSizePreset FromBlocks(long blocks, int preferWidth = 0, int preferHeight = 0)
        {
            if (blocks <= 0)
            {
                return null;
            }
            MapSizePreset preset = MapSizePreset.All.FirstOrDefault(p => (long)(p.Width / 8) * (p.Height / 8) == blocks);
            if (preferWidth > 0 && preferHeight > 0 && (long)(preferWidth / 8) * (preferHeight / 8) == blocks)
            {
                return new MapSizePreset("wie Editor-Karte", preferWidth / 8 * 8, preferHeight / 8 * 8);
            }
            if (preset != null)
            {
                return preset;
            }
            long side = (long)Math.Round(Math.Sqrt(blocks));
            if (side * side == blocks)
            {
                return new MapSizePreset("quadratisch", (int)side * 8, (int)side * 8);
            }
            foreach (int h in new[] { 4096, 2048, 1600, 1448, 1024, 512, 256 })
            {
                int bh = h / 8;
                if (blocks % bh == 0 && blocks / bh * 8 >= h / 2)
                {
                    return new MapSizePreset("aus Dateigröße", (int)(blocks / bh * 8), h);
                }
            }
            return null;
        }

        private static void ReplaceFile(string tmp, string target)
        {
            if (File.Exists(target))
            {
                File.Delete(target);
            }
            File.Move(tmp, target);
        }
    }

    /// <summary>
    /// map#.mul -> 8-bit Dragon BMP (like Dragon_m2b): every land tile is looked up in maptrans
    /// (and in the map transitions for border tiles), the altitude chooses the colour of the group.
    /// </summary>
    public static class MapToBitmapConverter
    {
        public static IndexedMap Convert(DragonRuleSet rules, ushort[] tiles, sbyte[] z, int width, int height, byte unknownIndex,
            out long unknownCount, IProgress<int> progress, CancellationToken cancel)
        {
            var groupOfTile = new Dictionary<ushort, int>();
            foreach (DragonPaletteEntry e in rules.Palette.Where(p => p.Defined && p.Group >= 0))
            {
                foreach (ushort id in e.GetTileIds())
                {
                    groupOfTile.TryAdd(id, e.Group);
                }
            }
            foreach (TransitionRule r in rules.Transitions.Where(t => t.Layer == TransitionLayer.Map && t.GroupA >= 0))
            {
                TransitionFile f = rules.FindTransitionFile(r.File);
                if (f == null)
                {
                    continue;
                }
                foreach (TransitionPattern p in f.Patterns)
                {
                    foreach (ushort id in p.GetTileIds())
                    {
                        groupOfTile.TryAdd(id, r.GroupA);
                    }
                }
            }

            var lookup = new Dictionary<int, int[]>();
            int[] AltTable(int group)
            {
                if (!lookup.TryGetValue(group, out int[] t))
                {
                    t = new int[256];
                    for (int a = -128; a < 128; a++)
                    {
                        t[a + 128] = rules.NearestIndex(group, a);
                    }
                    lookup[group] = t;
                }
                return t;
            }

            var map = new IndexedMap(width, height);
            unknownCount = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;
                    if (groupOfTile.TryGetValue(tiles[i], out int g))
                    {
                        int idx = AltTable(g)[z[i] + 128];
                        if (idx >= 0)
                        {
                            map.Pixels[i] = (byte)idx;
                            continue;
                        }
                    }
                    map.Pixels[i] = unknownIndex;
                    unknownCount++;
                }
                if ((y & 63) == 0)
                {
                    cancel.ThrowIfCancellationRequested();
                    progress?.Report(y * 100 / height);
                }
            }
            return map;
        }
    }
}

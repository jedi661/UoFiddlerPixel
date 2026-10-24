// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using Microsoft.VisualBasic;
using Transition;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit
{
    internal sealed class CompileOptions
    {
        public string OutputFolder { get; set; }
        public int MapNumber { get; set; }
        public bool RandomStatics { get; set; } = true;

        /// <summary>Load static XML files from &lt;ImportFolder&gt; (format of "Import Files").</summary>
        public bool ImportStatics { get; set; } = true;
        public string ImportFolder { get; set; }

        /// <summary>Seed for the random altitude variation; 0 = different every time.</summary>
        public int Seed { get; set; }
    }

    /// <summary>
    /// Builds map#.mul, staidx#.mul and statics#.mul from Terrain.bmp + Altitude.bmp
    /// (the former "Ultima Online Map Making Utility" / Make()).
    /// </summary>
    internal sealed class UoMapCompiler
    {
        private readonly TerrainPalette _terrain;
        private readonly AltitudePalette _altitude;

        public UoMapCompiler(TerrainPalette terrain, AltitudePalette altitude)
        {
            _terrain = terrain;
            _altitude = altitude;
        }

        /// <summary>Loads all transition XML files. Key = the 9 terrain groups of a 3x3 area.</summary>
        public static TransitionSet LoadTransitions(string folder, Action<string> log)
        {
            Dictionary<TransitionKey, Transition.Transition> result = new Dictionary<TransitionKey, Transition.Transition>();
            if (!Directory.Exists(folder))
            {
                log?.Invoke("Transition folder not found: " + folder);
                return new TransitionSet(result);
            }

            TransitionTable table = new TransitionTable { ShowMessages = false };
            table.MassLoad(folder);
            foreach (string error in table.LoadErrors.Take(10))
            {
                log?.Invoke("Warning: transition file skipped - " + error);
            }

            if (table.LoadErrors.Count > 10)
            {
                log?.Invoke(string.Format("Warning: ... and {0} more transition files could not be read.", table.LoadErrors.Count - 10));
            }

            int invalid = 0;
            foreach (DictionaryEntry entry in table.GetTransitionTable)
            {
                if (entry.Key is string key && TransitionKey.TryParse(key, out TransitionKey parsed) && entry.Value is Transition.Transition transition)
                {
                    result[parsed] = transition;
                }
                else
                {
                    invalid++;
                }
            }

            log?.Invoke(string.Format("{0} transitions loaded{1}.", result.Count, invalid > 0 ? string.Format(" ({0} with an invalid hash key skipped)", invalid) : string.Empty));
            return new TransitionSet(result);
        }

        public void Compile(MapDocument doc, CompileOptions options, IProgress<int> progress, Action<string> log, CancellationToken ct)
        {
            log = log ?? (_ => { });
            if (!MapDocument.IsValidUoSize(doc.Width, doc.Height, out string sizeError))
            {
                throw new InvalidOperationException(sizeError);
            }

            Stopwatch watch = Stopwatch.StartNew();
            int width = doc.Width;
            int height = doc.Height;
            int maxX = width - 1;
            int maxY = height - 1;
            int blocksX = width / 8;
            int blocksY = height / 8;
            int n = width * height;

            Random random = options.Seed == 0 ? new Random() : new Random(options.Seed);
            VBMath.Randomize(options.Seed == 0 ? Environment.TickCount : options.Seed); // used by the Transition DLL

            // ---------------------------------------------------------------- prepare
            log("Preparing map data ...");
            byte[] group = doc.Terrain;
            short[] alt = new short[n];
            short[] tile = new short[n];
            int invalidAltitude = 0;
            int undefinedTerrain = 0;
            for (int i = 0; i < n; i++)
            {
                byte key = doc.Altitude[i];
                if (!_altitude.Defined[key])
                {
                    key = _altitude.BaseKeyFor(_terrain, group[i]);
                    invalidAltitude++;
                }

                alt[i] = _altitude.Altitude[key];
                if (!_terrain.Defined[group[i]])
                {
                    undefinedTerrain++;
                }
            }

            if (invalidAltitude > 0)
            {
                log(string.Format("Warning: {0} altitude pixels used undefined palette entries - terrain default used.", invalidAltitude));
            }

            if (undefinedTerrain > 0)
            {
                log(string.Format("Warning: {0} pixels use terrain groups that are not in Terrain.xml - they get tile 0.", undefinedTerrain));
            }

            log("Loading transition tables ...");
            TransitionSet transitions = LoadTransitions(MapDataPaths.TransitionsDir, log);
            ct.ThrowIfCancellationRequested();

            Microsoft.VisualBasic.Collection[,] statics = new Microsoft.VisualBasic.Collection[blocksX, blocksY];
            for (int bx = 0; bx < blocksX; bx++)
            {
                for (int by = 0; by < blocksY; by++)
                {
                    statics[bx, by] = new Microsoft.VisualBasic.Collection();
                }
            }

            progress?.Report(5);

            // ---------------------------------------------------------------- pass 1: transitions
            log("Applying transition tables ...");
            for (int m = 0; m <= maxX; m++)
            {
                ct.ThrowIfCancellationRequested();
                int left = m == 0 ? maxX : m - 1;
                int right = m == maxX ? 0 : m + 1;
                for (int y = 0; y <= maxY; y++)
                {
                    int up = y == 0 ? maxY : y - 1;
                    int down = y == maxY ? 0 : y + 1;
                    int i = (y * width) + m;
                    byte g = group[i];

                    TransitionKey key = new TransitionKey(
                        group[(up * width) + left], group[(up * width) + m], group[(up * width) + right],
                        group[(y * width) + left], g, group[(y * width) + right],
                        group[(down * width) + left], group[(down * width) + m], group[(down * width) + right]);

                    short cellAltitude = alt[i];
                    try
                    {
                        if (!transitions.TryGet(key, out Transition.Transition transition))
                        {
                            // no transition: plain terrain tile, altitude from the altitude image
                            // (the old code overwrote it with the altitude of the last transition cell)
                            tile[i] = _terrain.TileIds[g];
                        }
                        else
                        {
                            MapTile mapTile = transition.GetRandomMapTile();
                            if (mapTile == null)
                            {
                                // transition with statics only: keep the altitude
                                // (the old code added the terrain's altitude *palette key* (e.g. 88) as height)
                                tile[i] = _terrain.TileIds[g];
                            }
                            else
                            {
                                tile[i] = mapTile.TileID;
                                alt[i] = (short)(alt[i] + mapTile.AltIDMod);
                            }

                            transition.GetRandomStaticTiles((short)m, (short)y, cellAltitude, statics, options.RandomStatics);
                        }

                        if (g == TerrainIds.TestPattern)
                        {
                            tile[i] = 1078;
                            alt[i] = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(string.Format("Location X:{0}, Y:{1}, Z:{2} HashKey:{3}: {4}", m, y, cellAltitude, key, ex.Message), ex);
                    }
                }

                if ((m & 31) == 0)
                {
                    progress?.Report(5 + (int)(45L * m / Math.Max(1, maxX)));
                }
            }

            log(string.Format("  done after {0:0.0} s.", watch.Elapsed.TotalSeconds));

            // ---------------------------------------------------------------- pass 2: rough edges, mountains, random altitude
            log("Second pass (rough edges, mountains, random altitude) ...");
            RoughEdge roughEdge = new RoughEdge(random);
            foreach (string error in roughEdge.Errors)
            {
                log("Warning: " + error);
            }

            short[] around = new short[9];
            for (int o = 0; o <= maxX; o++)
            {
                ct.ThrowIfCancellationRequested();
                int left = o == 0 ? maxX : o - 1;
                int right = o == maxX ? 0 : o + 1;
                for (int p = 0; p <= maxY; p++)
                {
                    int up = p == 0 ? maxY : p - 1;
                    int down = p == maxY ? 0 : p + 1;
                    int i = (p * width) + o;

                    int change = roughEdge.CheckCorner(tile[(up * width) + left])
                                 + roughEdge.CheckLeft(tile[(p * width) + left])
                                 + roughEdge.CheckTop(tile[(up * width) + o]);
                    alt[i] = (short)(alt[i] + change);

                    if (group[i] == TerrainIds.MountainCave)
                    {
                        around[0] = alt[(up * width) + left];
                        around[1] = alt[(up * width) + o];
                        around[2] = alt[(up * width) + right];
                        around[3] = alt[(p * width) + left];
                        around[4] = alt[i];
                        around[5] = alt[(p * width) + right];
                        around[6] = alt[(down * width) + left];
                        around[7] = alt[(down * width) + o];
                        around[8] = alt[(down * width) + right];
                        Array.Sort(around);
                        alt[i] = (short)(around[8] + RandomStep(random));
                    }

                    if (_terrain.RandomAltitude[group[i]])
                    {
                        alt[i] = (short)(alt[i] + RandomStep(random));
                    }
                }

                if ((o & 31) == 0)
                {
                    progress?.Report(50 + (int)(25L * o / Math.Max(1, maxX)));
                }
            }

            // ---------------------------------------------------------------- import statics
            if (options.ImportStatics)
            {
                string folder = options.ImportFolder;
                if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                {
                    log("No 'Import Files' folder found - no static XML files imported.");
                }
                else
                {
                    ImportStatics(folder, statics, width, height, log);
                }
            }

            ct.ThrowIfCancellationRequested();

            // ---------------------------------------------------------------- write mul files
            string outFolder = MapDataPaths.EnsureDirectory(options.OutputFolder);
            int mapNumber = options.MapNumber;
            string mapPath = Path.Combine(outFolder, string.Format("Map{0}.mul", mapNumber));
            string idxPath = Path.Combine(outFolder, string.Format("StaIdx{0}.mul", mapNumber));
            string staticsPath = Path.Combine(outFolder, string.Format("Statics{0}.mul", mapNumber));

            log("Writing " + mapPath);
            using (FileStream stream = new FileStream(mapPath, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                for (int bx = 0; bx < blocksX; bx++)
                {
                    for (int by = 0; by < blocksY; by++)
                    {
                        writer.Write(1); // block header
                        for (int cy = 0; cy < 8; cy++)
                        {
                            int row = ((by * 8) + cy) * width;
                            for (int cx = 0; cx < 8; cx++)
                            {
                                int i = row + (bx * 8) + cx;
                                writer.Write((ushort)tile[i]);
                                writer.Write((sbyte)Math.Clamp((int)alt[i], -127, 127));
                            }
                        }
                    }

                    if ((bx & 15) == 0)
                    {
                        progress?.Report(75 + (int)(15L * bx / Math.Max(1, blocksX)));
                    }
                }
            }

            log("Writing " + idxPath);
            log("Writing " + staticsPath);
            long staticCount = 0;
            using (FileStream idxStream = new FileStream(idxPath, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16))
            using (FileStream staStream = new FileStream(staticsPath, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16))
            using (BinaryWriter idx = new BinaryWriter(idxStream))
            using (BinaryWriter sta = new BinaryWriter(staStream))
            {
                for (int bx = 0; bx < blocksX; bx++)
                {
                    for (int by = 0; by < blocksY; by++)
                    {
                        int position = (int)sta.BaseStream.Position;
                        int length = 0;
                        foreach (object item in statics[bx, by])
                        {
                            if (item is StaticCell cell)
                            {
                                cell.Write(sta);
                                length += 7;
                                staticCount++;
                            }
                        }

                        idx.Write(length == 0 ? -1 : position);
                        idx.Write(length);
                        idx.Write(1); // extra
                    }

                    if ((bx & 15) == 0)
                    {
                        progress?.Report(90 + (int)(10L * bx / Math.Max(1, blocksX)));
                    }
                }
            }

            progress?.Report(100);
            log(string.Format("Done: {0}x{1}, {2} statics, {3:0.0} s.", width, height, staticCount, watch.Elapsed.TotalSeconds));
        }

        /// <summary>0 => -4, 1-2 => -2, 3-7 => 0, 8-9 => +2, 10 => +4 (the old float comparison hit almost nothing).</summary>
        private static int RandomStep(Random random)
        {
            int r = random.Next(11);
            if (r == 0)
            {
                return -4;
            }

            if (r <= 2)
            {
                return -2;
            }

            if (r <= 7)
            {
                return 0;
            }

            return r <= 9 ? 2 : 4;
        }

        private static void ImportStatics(string folder, Microsoft.VisualBasic.Collection[,] statics, int width, int height, Action<string> log)
        {
            int imported = 0;
            int skipped = 0;
            foreach (string file in Directory.EnumerateFiles(folder, "*.xml", SearchOption.AllDirectories))
            {
                try
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(file);
                    XmlNode root = xml.SelectSingleNode("//Static_Tiles");
                    if (root == null)
                    {
                        log("  skipped (no <Static_Tiles>): " + file);
                        continue;
                    }

                    foreach (XmlElement element in root.SelectNodes("Tile"))
                    {
                        int tileId = XmlConvert.ToInt32(element.GetAttribute("TileID"));
                        int x = XmlConvert.ToInt32(element.GetAttribute("X"));
                        int y = XmlConvert.ToInt32(element.GetAttribute("Y"));
                        int z = XmlConvert.ToInt32(element.GetAttribute("Z"));
                        string hueText = element.GetAttribute("Hue");
                        int hue = string.IsNullOrEmpty(hueText) ? 0 : XmlConvert.ToInt32(hueText);
                        if (x < 0 || y < 0 || x >= width || y >= height)
                        {
                            skipped++;
                            continue;
                        }

                        statics[x >> 3, y >> 3].Add(new StaticCell(tileId, (byte)(x % 8), (byte)(y % 8), z, hue), null, null, null);
                        imported++;
                    }
                }
                catch (Exception ex)
                {
                    log(string.Format("  error in {0}: {1}", file, ex.Message));
                }
            }

            log(string.Format("Imported {0} statics from XML files{1}.", imported, skipped > 0 ? string.Format(" ({0} outside of the map skipped)", skipped) : string.Empty));
        }

        // ================================================================ transition check

        /// <summary>
        /// Lists terrain combinations that occur in the map but have no transition.
        /// Those places get hard edges in the client.
        /// </summary>
        public static List<string> CheckTransitions(MapDocument doc, TerrainPalette terrains, TransitionSet transitions, int maxResults, CancellationToken ct)
        {
            // key = sorted distinct terrain groups of the 3x3 area (2 = border, 3+ = junction)
            Dictionary<string, int> missing = new Dictionary<string, int>();
            Dictionary<string, string> names = new Dictionary<string, string>();
            SortedSet<byte> groups = new SortedSet<byte>();
            for (int x = 0; x < doc.Width; x++)
            {
                ct.ThrowIfCancellationRequested();
                for (int y = 0; y < doc.Height; y++)
                {
                    TransitionKey key = TransitionRepair.KeyAt(doc, x, y);
                    if (transitions.IsOk(key))
                    {
                        continue;
                    }

                    groups.Clear();
                    for (int k = 0; k < 9; k++)
                    {
                        groups.Add(key[k]);
                    }

                    string id = string.Join(",", groups);
                    if (!missing.TryGetValue(id, out int count))
                    {
                        names[id] = string.Join("  +  ", groups.Select(g => terrains.DisplayName(g)));
                    }

                    missing[id] = count + 1;
                }
            }

            return missing.OrderByDescending(p => p.Value)
                .Take(maxResults)
                .Select(p => string.Format("{0,8} tiles: {1}", p.Value, names[p.Key]))
                .ToList();
        }
    }

    /// <summary>The 9 terrain groups of a 3x3 area, packed (no string per tile like the old "{0:X2}..." key).</summary>
    internal readonly struct TransitionKey : IEquatable<TransitionKey>
    {
        private readonly ulong _low;  // groups 0..7
        private readonly byte _high;  // group 8

        public TransitionKey(byte a, byte b, byte c, byte d, byte e, byte f, byte g, byte h, byte i)
        {
            _low = a | ((ulong)b << 8) | ((ulong)c << 16) | ((ulong)d << 24) | ((ulong)e << 32) | ((ulong)f << 40) | ((ulong)g << 48) | ((ulong)h << 56);
            _high = i;
        }

        public byte this[int index] => index == 8 ? _high : (byte)(_low >> (index * 8));

        public bool IsUniform
        {
            get
            {
                byte first = this[0];
                for (int k = 1; k < 9; k++)
                {
                    if (this[k] != first)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public byte FirstDifferent(byte value)
        {
            for (int k = 0; k < 9; k++)
            {
                if (this[k] != value)
                {
                    return this[k];
                }
            }

            return value;
        }

        public static bool TryParse(string text, out TransitionKey key)
        {
            key = default;
            if (text == null || text.Length != 18)
            {
                return false;
            }

            byte[] parts = new byte[9];
            for (int k = 0; k < 9; k++)
            {
                if (!byte.TryParse(text.Substring(k * 2, 2), System.Globalization.NumberStyles.HexNumber, null, out parts[k]))
                {
                    return false;
                }
            }

            key = new TransitionKey(parts[0], parts[1], parts[2], parts[3], parts[4], parts[5], parts[6], parts[7], parts[8]);
            return true;
        }

        public bool Equals(TransitionKey other) => _low == other._low && _high == other._high;

        public override bool Equals(object obj) => obj is TransitionKey other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(_low, _high);

        public override string ToString()
        {
            char[] chars = new char[18];
            const string hex = "0123456789ABCDEF";
            for (int k = 0; k < 9; k++)
            {
                byte v = this[k];
                chars[k * 2] = hex[v >> 4];
                chars[(k * 2) + 1] = hex[v & 15];
            }

            return new string(chars);
        }
    }
}

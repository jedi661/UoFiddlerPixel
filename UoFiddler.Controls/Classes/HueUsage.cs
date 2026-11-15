/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Which hue is used for what – read from the client files only:
 *
 *  statics*.mul    every static on the maps has a hue (0 = none, otherwise hue number = index + 1)
 *  body.def        <body> {<drawn body>} <hue>        – body is drawn with another animation and a hue
 *  corpse.def      <body> {<corpse body>} <hue>       – corpse of a body
 *  gump.def        <gump> {<drawn gump>} <hue>        – paperdoll gump replaced by another one with a hue
 *  equipconv.def   <body> <equipment> <convert to> <gump> <hue>
 *
 * Not hued by the client (so no usage): land tiles (map*.mul), textures, multis, art/anim/gump files themselves.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ultima;

namespace UoFiddler.Controls.Classes
{
    public enum HueUsageKind
    {
        Static,
        BodyDef,
        CorpseDef,
        GumpDef,
        EquipConv
    }

    /// <summary>Kind of graphic that is used for the preview</summary>
    public enum HuePreviewKind
    {
        Item,
        Animation,
        Gump
    }

    public sealed class HueUsageEntry
    {
        public HueUsageKind Kind { get; set; }

        /// <summary>graphic shown for this usage (item id, body or gump)</summary>
        public HuePreviewKind PreviewKind { get; set; }
        public int PreviewId { get; set; }

        /// <summary>number of statics (only for Kind = Static)</summary>
        public int Count { get; set; }

        /// <summary>example positions "Felucca 1234,567,0"</summary>
        public List<string> Samples { get; } = new List<string>();

        public string Text { get; set; }

        public override string ToString() => Text;
    }

    public static class HueUsageIndex
    {
        private const int MaxSamples = 4;
        private static Dictionary<int, List<HueUsageEntry>> _byHue = new Dictionary<int, List<HueUsageEntry>>();
        private static Task _task;
        private static int _version;

        /// <summary>Raised (on a worker thread) when the index is ready</summary>
        public static event EventHandler Built;

        public static bool IsBuilt { get; private set; }
        public static bool IsBuilding => _task != null && !_task.IsCompleted;

        /// <summary>What was scanned ("3 maps, 1.234.567 statics …")</summary>
        public static string Summary { get; private set; } = "not scanned yet";

        public static string LastError { get; private set; }

        /// <summary>Usages of a hue (file index), most important first</summary>
        public static IReadOnlyList<HueUsageEntry> Get(int hueIndex)
        {
            return _byHue.TryGetValue(hueIndex, out List<HueUsageEntry> list) ? list : (IReadOnlyList<HueUsageEntry>)Array.Empty<HueUsageEntry>();
        }

        public static bool IsUsed(int hueIndex) => _byHue.ContainsKey(hueIndex);

        public static int UsedCount => _byHue.Count;

        /// <summary>Drops the index (files changed)</summary>
        public static void Invalidate()
        {
            _version++;
            IsBuilt = false;
            _byHue = new Dictionary<int, List<HueUsageEntry>>();
            Summary = "not scanned yet";
        }

        /// <summary>
        /// Starts the scan in the background (call on the UI thread: the map paths are read here).
        /// Returns the running task if a scan is already running.
        /// </summary>
        public static Task BuildAsync()
        {
            if (IsBuilding)
            {
                return _task;
            }

            int version = ++_version;
            List<MapSource> maps = CollectMaps();
            string bodyDef = Files.GetFilePath("body.def");
            string corpseDef = Files.GetFilePath("corpse.def");
            string gumpDef = Files.GetFilePath("gump.def");
            string equipConv = Files.GetFilePath("equipconv.def");
            int hueCount = Hues.List?.Length ?? 3000;

            _task = Task.Run(() =>
            {
                var result = new Dictionary<int, List<HueUsageEntry>>();
                var statics = new Dictionary<long, HueUsageEntry>();
                long staticCount = 0, huedCount = 0;
                var errors = new List<string>();

                foreach (MapSource map in maps)
                {
                    try
                    {
                        ScanStatics(map, hueCount, statics, ref staticCount, ref huedCount);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{map.Name}: {ex.Message}");
                    }
                }

                foreach (KeyValuePair<long, HueUsageEntry> pair in statics)
                {
                    int hue = (int)(pair.Key >> 16);
                    HueUsageEntry e = pair.Value;
                    string name = ItemName(e.PreviewId);
                    e.Text = $"Item 0x{e.PreviewId:X4}{(name.Length > 0 ? $" \"{name}\"" : string.Empty)} – {e.Count}× on the maps" +
                             (e.Samples.Count > 0 ? $"  ({string.Join("; ", e.Samples)}{(e.Count > e.Samples.Count ? "; …" : string.Empty)})" : string.Empty);
                    Add(result, hue, e);
                }

                int defs = 0;
                defs += ReadBraceDef(bodyDef, "body.def", HueUsageKind.BodyDef, HuePreviewKind.Animation, hueCount, result,
                    (orig, drawn) => $"body.def: body {orig} is drawn as animation {drawn} with this hue");
                defs += ReadBraceDef(corpseDef, "corpse.def", HueUsageKind.CorpseDef, HuePreviewKind.Animation, hueCount, result,
                    (orig, drawn) => $"corpse.def: corpse of body {orig} uses animation {drawn} with this hue");
                defs += ReadBraceDef(gumpDef, "gump.def", HueUsageKind.GumpDef, HuePreviewKind.Gump, hueCount, result,
                    (orig, drawn) => $"gump.def: gump {orig} (0x{orig:X}) is shown as gump {drawn} (0x{drawn:X}) with this hue");
                defs += ReadEquipConv(equipConv, hueCount, result);

                // statics: most used first, then the definition files
                foreach (List<HueUsageEntry> list in result.Values)
                {
                    list.Sort((a, b) => a.Kind != b.Kind ? a.Kind.CompareTo(b.Kind) : b.Count.CompareTo(a.Count));
                }

                if (version != _version)
                {
                    return; // invalidated meanwhile
                }

                _byHue = result;
                LastError = errors.Count == 0 ? null : string.Join("\n", errors);
                Summary = $"{maps.Count} map file(s): {staticCount:N0} statics, {huedCount:N0} with a hue; {defs} entries in the .def files; " +
                          $"{result.Count} hues are used";
                IsBuilt = true;
                Built?.Invoke(null, EventArgs.Empty);
            });

            return _task;
        }

        #region Statics

        private sealed class MapSource
        {
            public string Name;
            public string IndexPath;
            public string StaticsPath;
            public int BlockHeight;
        }

        private static List<MapSource> CollectMaps()
        {
            var list = new List<MapSource>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Map map in Map.AllMaps())
            {
                try
                {
                    TileMatrix tiles = map.Tiles;
                    string idx = tiles.IndexPath, mul = tiles.StaticsPath;
                    if (idx == null || mul == null || !File.Exists(idx) || !File.Exists(mul))
                    {
                        continue;
                    }

                    // Felucca and Trammel often use the same files
                    if (!seen.Add(Path.GetFullPath(mul)))
                    {
                        continue;
                    }

                    string name;
                    try
                    {
                        name = MapRegistry.GetName(map.MapId);
                    }
                    catch (Exception)
                    {
                        name = $"Map {map.MapId}";
                    }

                    list.Add(new MapSource { Name = name, IndexPath = idx, StaticsPath = mul, BlockHeight = Math.Max(1, map.Height >> 3) });
                }
                catch (Exception)
                {
                    // map without files
                }
            }

            return list;
        }

        private static void ScanStatics(MapSource map, int hueCount, Dictionary<long, HueUsageEntry> statics, ref long staticCount, ref long huedCount)
        {
            byte[] index = File.ReadAllBytes(map.IndexPath);
            using (var fs = new FileStream(map.StaticsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1 << 16))
            using (var reader = new BinaryReader(fs))
            {
                long length = fs.Length;
                int blocks = index.Length / 12;
                var buffer = new byte[0];
                for (int block = 0; block < blocks; block++)
                {
                    int lookup = BitConverter.ToInt32(index, block * 12);
                    int size = BitConverter.ToInt32(index, (block * 12) + 4);
                    if (lookup < 0 || size <= 0 || lookup + (long)size > length)
                    {
                        continue;
                    }

                    if (buffer.Length < size)
                    {
                        buffer = new byte[size];
                    }

                    fs.Seek(lookup, SeekOrigin.Begin);
                    int read = fs.Read(buffer, 0, size);
                    int bx = block / map.BlockHeight, by = block % map.BlockHeight;

                    for (int p = 0; p + 7 <= read; p += 7)
                    {
                        staticCount++;
                        short hue = BitConverter.ToInt16(buffer, p + 5);
                        if (hue == 0)
                        {
                            continue;
                        }

                        int hueIndex = (hue & 0x3FFF) - 1;
                        if (hueIndex < 0 || hueIndex >= hueCount)
                        {
                            continue;
                        }

                        huedCount++;
                        ushort id = BitConverter.ToUInt16(buffer, p);
                        long key = ((long)hueIndex << 16) | id;
                        if (!statics.TryGetValue(key, out HueUsageEntry entry))
                        {
                            entry = new HueUsageEntry { Kind = HueUsageKind.Static, PreviewKind = HuePreviewKind.Item, PreviewId = id };
                            statics[key] = entry;
                        }

                        entry.Count++;
                        if (entry.Samples.Count < MaxSamples)
                        {
                            int x = (bx << 3) + buffer[p + 2], y = (by << 3) + buffer[p + 3];
                            entry.Samples.Add($"{map.Name} {x},{y},{(sbyte)buffer[p + 4]}");
                        }
                    }
                }
            }
        }

        private static string ItemName(int id)
        {
            try
            {
                return TileData.ItemTable != null && id < TileData.ItemTable.Length ? (TileData.ItemTable[id].Name ?? string.Empty).Trim() : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        #endregion

        #region .def files

        private static void Add(Dictionary<int, List<HueUsageEntry>> result, int hueIndex, HueUsageEntry entry)
        {
            if (!result.TryGetValue(hueIndex, out List<HueUsageEntry> list))
            {
                list = new List<HueUsageEntry>();
                result[hueIndex] = list;
            }

            list.Add(entry);
        }

        private static bool TryInt(string text, out int value)
        {
            text = (text ?? string.Empty).Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
            }

            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>"orig {new[, more]} hue" files (body.def, corpse.def, gump.def)</summary>
        private static int ReadBraceDef(string path, string fileName, HueUsageKind kind, HuePreviewKind preview, int hueCount,
            Dictionary<int, List<HueUsageEntry>> result, Func<int, int, string> text)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return 0;
            }

            int count = 0;
            foreach (string raw in File.ReadLines(path))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("//"))
                {
                    continue;
                }

                int open = line.IndexOf('{'), close = line.IndexOf('}');
                if (open < 0 || close < open)
                {
                    continue;
                }

                string drawnText = line.Substring(open + 1, close - open - 1).Split(',')[0];
                string hueText = line.Substring(close + 1).Trim().Split(new[] { ' ', '\t', '#' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (!TryInt(line.Substring(0, open), out int orig) || !TryInt(drawnText, out int drawn) || !TryInt(hueText, out int hue) || hue <= 0)
                {
                    continue;
                }

                int hueIndex = (hue & 0x3FFF) - 1;
                if (hueIndex < 0 || hueIndex >= hueCount)
                {
                    continue;
                }

                Add(result, hueIndex, new HueUsageEntry { Kind = kind, PreviewKind = preview, PreviewId = drawn, Text = text(orig, drawn) });
                count++;
            }

            return count;
        }

        /// <summary>equipconv.def: bodyType equipment convertTo gump hue</summary>
        private static int ReadEquipConv(string path, int hueCount, Dictionary<int, List<HueUsageEntry>> result)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return 0;
            }

            int count = 0;
            foreach (string raw in File.ReadLines(path))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("\"") || line.StartsWith("//"))
                {
                    continue;
                }

                string[] split = Regex.Split(line, @"\s+");
                if (split.Length < 5 || !TryInt(split[0], out int body) || !TryInt(split[1], out int equip) ||
                    !TryInt(split[2], out int convert) || !TryInt(split[3], out int gump) || !TryInt(split[4], out int hue) || hue <= 0)
                {
                    continue;
                }

                int hueIndex = (hue & 0x3FFF) - 1;
                if (hueIndex < 0 || hueIndex >= hueCount)
                {
                    continue;
                }

                int gumpId = gump == 0 ? equip + 50000 : gump == -1 ? convert + 50000 : gump;
                Add(result, hueIndex, new HueUsageEntry
                {
                    Kind = HueUsageKind.EquipConv,
                    PreviewKind = HuePreviewKind.Animation,
                    PreviewId = convert > 0 ? convert : equip,
                    Text = $"equipconv.def: on body {body} equipment {equip} is drawn as animation {convert}, paperdoll gump {gumpId} – with this hue"
                });
                count++;
            }

            return count;
        }

        #endregion
    }
}

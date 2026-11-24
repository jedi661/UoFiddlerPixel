/***************************************************************************
 *
 *  * Dragon Map Studio – direction table ("Richtungen festlegen"): groups of four graphics (items or land tiles)
 *  *  for north, east, south and west, put in by hand. What is in the table wins over the automatic recognition
 *  *  (slope of stairs from the art, walls by their outline) – for building stairs, turning (R / 90°), the motif kit
 *  *  and turning land.
 *  *
 *  *  For a stair group the direction means "goes up towards". Own groups: SettingsFolder\richtungen.json,
 *  *  shipped ones (project): DragonData\Richtungen\*.json – an own group with the same name wins.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public enum DirectionKind
    {
        Item,
        Land
    }

    public sealed class DirectionGroup
    {
        public string Name { get; set; } = "Neue Gruppe";

        public DirectionKind Kind { get; set; } = DirectionKind.Item;

        /// <summary>Stairs: the direction means "goes up towards" (used for building stairs and finding stairs in motifs).</summary>
        public bool Stair { get; set; }

        /// <summary>Graphic per direction (index = Compass: north, east, south, west); 0 = none.</summary>
        public int[] Ids { get; set; } = new int[4];

        public string Note { get; set; } = string.Empty;

        [JsonIgnore]
        public bool Shipped { get; set; }

        public int this[Compass c]
        {
            get => Ids != null && Ids.Length == 4 ? Ids[(int)c] : 0;
            set
            {
                if (Ids == null || Ids.Length != 4)
                {
                    Ids = new int[4];
                }
                Ids[(int)c] = value;
            }
        }

        public override string ToString() => $"{Name} ({(Kind == DirectionKind.Land ? "Land" : "Item")}{(Stair ? ", Treppe" : string.Empty)}): " +
                                             string.Join(" ", new[] { Compass.North, Compass.East, Compass.South, Compass.West }.Select(c => this[c] == 0 ? "–" : $"{this[c]:x4}"));
    }

    public static class DirectionGroups
    {
        private static List<DirectionGroup> _all;
        private static Dictionary<(DirectionKind, int), (DirectionGroup g, Compass c)> _index;
        private static readonly object Lock = new object();
        private static readonly JsonSerializerOptions JsonOpt = new JsonSerializerOptions { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };

        public static string FilePath => Path.Combine(StudioSettings.SettingsFolder, "richtungen.json");

        public static string ShippedFolder => StudioPaths.DataFolder == null ? null : Path.Combine(StudioPaths.DataFolder, "Richtungen");

        /// <summary>Raised after the table changed (caches of stairs / turning are cleared).</summary>
        public static event Action Changed;

        /// <summary>All groups (shipped + own; own ones with the same name win).</summary>
        public static List<DirectionGroup> All
        {
            get
            {
                lock (Lock)
                {
                    if (_all == null)
                    {
                        Load();
                    }
                    return _all;
                }
            }
        }

        private static List<DirectionGroup> ReadFile(string file)
        {
            try
            {
                return JsonSerializer.Deserialize<List<DirectionGroup>>(File.ReadAllText(file), JsonOpt) ?? new List<DirectionGroup>();
            }
            catch (Exception)
            {
                return new List<DirectionGroup>();
            }
        }

        private static void Load()
        {
            var byName = new Dictionary<string, DirectionGroup>(StringComparer.OrdinalIgnoreCase);
            if (ShippedFolder != null && Directory.Exists(ShippedFolder))
            {
                foreach (string f in Directory.GetFiles(ShippedFolder, "*.json"))
                {
                    foreach (DirectionGroup g in ReadFile(f))
                    {
                        g.Shipped = true;
                        byName[g.Name] = g;
                    }
                }
            }
            if (File.Exists(FilePath))
            {
                foreach (DirectionGroup g in ReadFile(FilePath))
                {
                    g.Shipped = false;
                    byName[g.Name] = g;
                }
            }
            _all = byName.Values.ToList();
            BuildIndex();
        }

        private static void BuildIndex()
        {
            _index = new Dictionary<(DirectionKind, int), (DirectionGroup, Compass)>();
            foreach (DirectionGroup g in _all)
            {
                foreach (Compass c in new[] { Compass.North, Compass.East, Compass.South, Compass.West })
                {
                    int id = g[c];
                    if (id != 0 && !_index.ContainsKey((g.Kind, id)))
                    {
                        _index[(g.Kind, id)] = (g, c);
                    }
                }
            }
        }

        /// <summary>Saves the given groups as the own table (shipped ones that were not changed are left out).</summary>
        public static void Save(List<DirectionGroup> groups)
        {
            lock (Lock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllText(FilePath, JsonSerializer.Serialize(groups.Where(g => !g.Shipped).ToList(), JsonOpt));
                _all = groups.ToList();
                BuildIndex();
            }
            Changed?.Invoke();
        }

        public static void Reload()
        {
            lock (Lock)
            {
                _all = null;
            }
            Changed?.Invoke();
        }

        /// <summary>Writes groups to a file (e.g. DragonData\Richtungen\meine.json of the project).</summary>
        public static void Export(IEnumerable<DirectionGroup> groups, string file) => File.WriteAllText(file, JsonSerializer.Serialize(groups.ToList(), JsonOpt));

        /// <summary>Reads groups from a file (they become own groups; same name = replaced).</summary>
        public static List<DirectionGroup> ImportFile(string file) => ReadFile(file);

        /// <summary>Direction of a graphic in the table (null = not in the table).</summary>
        public static Compass? DirOf(int id, DirectionKind kind = DirectionKind.Item)
        {
            List<DirectionGroup> unused = All;
            lock (Lock)
            {
                return _index != null && _index.TryGetValue((kind, id), out (DirectionGroup g, Compass c) e) ? e.c : (Compass?)null;
            }
        }

        /// <summary>"Goes up towards" of a stair piece from the table (only groups marked as stairs).</summary>
        public static Compass? StairUp(int id)
        {
            List<DirectionGroup> unused = All;
            lock (Lock)
            {
                return _index != null && _index.TryGetValue((DirectionKind.Item, id), out (DirectionGroup g, Compass c) e) && e.g.Stair ? e.c : (Compass?)null;
            }
        }

        /// <summary>
        /// The graphic after <paramref name="quarterTurns"/> clockwise quarter turns, from the table; −1 = not in the table.
        /// An empty slot takes the opposite one (walls look the same from both sides), else the graphic stays.
        /// </summary>
        public static int Turned(int id, int quarterTurns, DirectionKind kind = DirectionKind.Item)
        {
            List<DirectionGroup> unused = All;
            DirectionGroup g;
            Compass c;
            lock (Lock)
            {
                if (_index == null || !_index.TryGetValue((kind, id), out (DirectionGroup g, Compass c) e))
                {
                    return -1;
                }
                g = e.g;
                c = e.c;
            }
            var to = (Compass)((((int)c + quarterTurns) % 4 + 4) % 4);
            if (g[to] != 0)
            {
                return g[to];
            }
            var opposite = (Compass)(((int)to + 2) % 4);
            return g[opposite] != 0 ? g[opposite] : id;
        }
    }
}

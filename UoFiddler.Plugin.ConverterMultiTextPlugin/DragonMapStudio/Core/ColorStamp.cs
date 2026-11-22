/***************************************************************************
 *
 *  * Dragon Map Studio – colour stamps for the 2D map editor
 *  *
 *  * A stamp is a cut-out of the colour map (houses floors, roads, a cave
 *  * entrance, a hill …). Every pixel remembers its colour, its group and its
 *  * altitude relative to the ground of the stamp. When it is placed with
 *  * "Höhen anpassen", every pixel gets the colour of the same group whose
 *  * altitude is closest to (ground under the stamp + relative altitude) –
 *  * so a stamp built at height 0 also fits on a plateau at height 20.
 *  * Colours you defined yourself work the same way (only their group and
 *  * altitude matter).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public sealed class ColorStamp
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>Colour address of every pixel (row by row).</summary>
        public ushort[] Pixels { get; set; } = Array.Empty<ushort>();

        /// <summary>Group of every pixel (-1 = colour without group).</summary>
        public int[] Groups { get; set; } = Array.Empty<int>();

        /// <summary>Altitude of every pixel minus <see cref="BaseAltitude"/>.</summary>
        public int[] RelativeAltitudes { get; set; } = Array.Empty<int>();

        /// <summary>Ground of the stamp: median altitude of its border (water left out).</summary>
        public int BaseAltitude { get; set; }

        /// <summary>Statics of the stamp (X / Y inside the stamp, Dz above the land) – placed into the statics layer of the map.</summary>
        public List<LayerStatic> Statics { get; set; } = new List<LayerStatic>();

        /// <summary>Built land tiles of the area (only stamps made in the CGI view) – needed to learn rules from the stamp later.</summary>
        public ushort[] LandTiles { get; set; }

        /// <summary>Built land heights of the area (only stamps made in the CGI view).</summary>
        public sbyte[] LandZ { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public bool CanLearn => LandTiles != null && LandZ != null && LandTiles.Length == Width * Height && LandZ.Length == Width * Height;

        /// <summary>File the stamp was loaded from.</summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string File { get; set; } = string.Empty;

        public override string ToString() => string.IsNullOrEmpty(Category) ? $"{Name}  ({Width}×{Height})" : $"{Name}  ({Width}×{Height}) – {Category}";
    }

    public static class ColorStampTools
    {
        public const string Extension = ".dmstamp.json";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = false, IncludeFields = true };

        /// <summary>Median altitude of the border cells of <paramref name="r"/> (water group left out when possible).</summary>
        public static int GroundAltitude(IndexedMap map, System.Drawing.Rectangle r, DragonRuleSet rules, int waterGroup)
        {
            var all = new List<int>();
            var land = new List<int>();
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    if (!map.InBounds(x, y) || (x != r.Left && x != r.Right - 1 && y != r.Top && y != r.Bottom - 1))
                    {
                        continue;
                    }
                    ushort i = map[x, y];
                    if (i >= rules.Palette.Count)
                    {
                        continue;
                    }
                    DragonPaletteEntry e = rules.Palette[i];
                    all.Add(e.Altitude);
                    if (e.Group != waterGroup)
                    {
                        land.Add(e.Altitude);
                    }
                }
            }
            List<int> use = land.Count > 0 ? land : all;
            if (use.Count == 0)
            {
                return 0;
            }
            use.Sort();
            return use[use.Count / 2];
        }

        public static ColorStamp FromMap(IndexedMap cut, DragonRuleSet rules, int waterGroup, string name, string category)
        {
            var s = new ColorStamp { Name = name, Category = category, Width = cut.Width, Height = cut.Height };
            s.BaseAltitude = GroundAltitude(cut, new System.Drawing.Rectangle(0, 0, cut.Width, cut.Height), rules, waterGroup);
            int n = cut.Width * cut.Height;
            s.Pixels = (ushort[])cut.Pixels.Clone();
            s.Groups = new int[n];
            s.RelativeAltitudes = new int[n];
            for (int i = 0; i < n; i++)
            {
                ushort c = cut.Pixels[i];
                DragonPaletteEntry e = c < rules.Palette.Count ? rules.Palette[c] : null;
                s.Groups[i] = e != null && e.Defined ? e.Group : -1;
                s.RelativeAltitudes[i] = (e?.Altitude ?? 0) - s.BaseAltitude;
            }
            return s;
        }

        /// <summary>
        /// The pixels to write. <paramref name="adapt"/>: every pixel the colour of its group nearest to
        /// <paramref name="targetBase"/> + relative altitude (colours without group stay).
        /// </summary>
        public static IndexedMap Realize(ColorStamp s, DragonRuleSet rules, int targetBase, bool adapt, int bits)
        {
            var map = new IndexedMap(s.Width, s.Height, 0, bits);
            var byGroup = new Dictionary<int, List<DragonPaletteEntry>>();
            for (int i = 0; i < s.Pixels.Length; i++)
            {
                ushort c = s.Pixels[i];
                int g = i < s.Groups.Length ? s.Groups[i] : -1;
                if (adapt && g >= 0)
                {
                    if (!byGroup.TryGetValue(g, out List<DragonPaletteEntry> entries))
                    {
                        byGroup[g] = entries = rules.EntriesOfGroup(g).Where(e => e.Defined).ToList();
                    }
                    if (entries.Count > 0)
                    {
                        int want = targetBase + (i < s.RelativeAltitudes.Length ? s.RelativeAltitudes[i] : 0);
                        c = (ushort)entries.OrderBy(e => Math.Abs(e.Altitude - want)).ThenBy(e => e.Index == s.Pixels[i] ? 0 : 1).First().Index;
                    }
                }
                map.Pixels[i] = c;
            }
            return map;
        }

        public static string Save(ColorStamp s, string folder)
        {
            Directory.CreateDirectory(folder);
            string safe = string.Concat((string.IsNullOrWhiteSpace(s.Name) ? "Stempel" : s.Name).Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
            string path = Path.Combine(folder, safe + Extension);
            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(s, JsonOptions));
            s.File = path;
            return path;
        }

        public static List<ColorStamp> LoadAll(string folder, List<string> errors)
        {
            var list = new List<ColorStamp>();
            if (!Directory.Exists(folder))
            {
                return list;
            }
            foreach (string f in Directory.GetFiles(folder, "*" + Extension).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    ColorStamp s = JsonSerializer.Deserialize<ColorStamp>(System.IO.File.ReadAllText(f), JsonOptions);
                    if (s != null)
                    {
                        s.Statics ??= new List<LayerStatic>();
                    }
                    if (s != null && s.Width > 0 && s.Height > 0 && s.Pixels.Length == s.Width * s.Height)
                    {
                        s.File = f;
                        list.Add(s);
                    }
                }
                catch (Exception ex)
                {
                    errors?.Add($"{Path.GetFileName(f)}: {ex.Message}");
                }
            }
            return list;
        }

        /// <summary>Brush catalogue: a readable category from the group / colour name.</summary>
        public static string CategoryOf(DragonPaletteEntry e, string groupName)
        {
            string t = ((e.Category ?? string.Empty) + " " + (groupName ?? string.Empty) + " " + (e.Name ?? string.Empty)).ToLowerInvariant();
            bool Has(params string[] words) => words.Any(w => t.Contains(w));
            if (Has("cave", "höhle", "dungeon", "black", "lava", "void"))
            {
                return "Höhle & Dungeon";
            }
            if (Has("floor", "marble", "plank", "carpet", "boden", "tile ", "flagstone", "parkett"))
            {
                return "Hausböden";
            }
            if (Has("road", "street", "cobble", "path", "weg", "straße", "strasse", "pave", "brick", "dirt", "erde"))
            {
                return "Wege & Straßen";
            }
            if (Has("water", "wasser", "sea", "ocean", "sand", "beach", "strand", "coast", "küste", "swamp", "sumpf", "shore"))
            {
                return "Wasser & Küste";
            }
            if (Has("mountain", "berg", "rock", "fels", "snow", "schnee", "hill"))
            {
                return "Berg & Fels";
            }
            if (Has("grass", "gras", "forest", "wald", "jungle", "dschungel", "wood", "tree", "furrow", "farm", "field", "savannah", "waste"))
            {
                return "Gras & Wald";
            }
            return "Sonstiges";
        }

        public static readonly string[] Categories = { "Alle", "Gras & Wald", "Wasser & Küste", "Berg & Fels", "Wege & Straßen", "Hausböden", "Höhle & Dungeon", "Sonstiges" };
    }
}

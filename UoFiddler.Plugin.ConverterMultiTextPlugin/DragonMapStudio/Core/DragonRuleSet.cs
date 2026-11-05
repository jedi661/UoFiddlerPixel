// /***************************************************************************
//  *
//  * Dragon Map Studio – complete rule set (everything Dragon knows + extensions)
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>
    /// Holds all Dragon information in one object: palette (256 colours), groups, map / static transitions,
    /// items, unevenly, fix table and own extensions (colour aliases, textures, notes).
    /// Can be read from / written to the original Dragon script folder and to one JSON file.
    /// </summary>
    public sealed class DragonRuleSet
    {
        public const int FormatVersion = 1;

        public int Version { get; set; } = FormatVersion;
        public string Name { get; set; } = "Dragon IMod13";
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Colour addresses: 8 = classic Dragon (256 colours, 2-digit hex in the scripts), 16 = extended
        /// (up to 65 536 colours, 4-digit hex). 8-bit scripts and maps can always be read.
        /// </summary>
        public int ColorBits { get; set; } = 8;

        /// <summary>Highest number of colours of the format.</summary>
        [JsonIgnore]
        public int MaxColors => ColorBits == 16 ? 65536 : 256;

        /// <summary>Highest group number (groups.txt); 8 bit: ff = "any" in the scripts.</summary>
        [JsonIgnore]
        public int MaxGroup => ColorBits == 16 ? 0x7FFF : 0xFE;

        /// <summary>Palette index = position. 8 bit: 256 entries; 16 bit: grows in pages of 256.</summary>
        public List<DragonPaletteEntry> Palette { get; set; } = CreateEmptyPalette();

        public List<DragonGroup> Groups { get; set; } = new List<DragonGroup>();
        public List<TransitionRule> Transitions { get; set; } = new List<TransitionRule>();
        public List<TransitionFile> TransitionFiles { get; set; } = new List<TransitionFile>();
        public List<ItemRule> ItemRules { get; set; } = new List<ItemRule>();
        public List<ItemFile> ItemFiles { get; set; } = new List<ItemFile>();
        public List<UnevenRule> UnevenRules { get; set; } = new List<UnevenRule>();
        public List<FixEntry> Fixes { get; set; } = new List<FixEntry>();
        public List<ColorAlias> ColorAliases { get; set; } = new List<ColorAlias>();

        /// <summary>Header comments of the list files (kept for the export).</summary>
        public Dictionary<string, string> FileHeaders { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Problems found while loading (missing files, broken lines …).</summary>
        [JsonIgnore]
        public List<string> Warnings { get; } = new List<string>();

        public static List<DragonPaletteEntry> CreateEmptyPalette(int count = 256)
        {
            var list = new List<DragonPaletteEntry>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(new DragonPaletteEntry { Index = i, Name = "frei" });
            }
            return list;
        }

        /// <summary>Makes sure the palette has the right size (256 or a multiple of 256) with correct indices.</summary>
        public void Normalize()
        {
            Palette ??= CreateEmptyPalette();
            int highest = Palette.Where(e => e != null && e.Index >= 0).Select(e => e.Index).DefaultIfEmpty(0).Max();
            if (highest > 0xFF)
            {
                ColorBits = 16;
            }
            ColorBits = ColorBits == 16 ? 16 : 8;
            int size = ColorBits == 16 ? Math.Min(65536, Math.Max(256, (highest / 256 + 1) * 256)) : 256;
            var fixedList = CreateEmptyPalette(size);
            foreach (DragonPaletteEntry e in Palette)
            {
                if (e != null && e.Index >= 0 && e.Index < size)
                {
                    fixedList[e.Index] = e;
                }
            }
            Palette = fixedList;
            Groups ??= new List<DragonGroup>();
            Transitions ??= new List<TransitionRule>();
            TransitionFiles ??= new List<TransitionFile>();
            ItemRules ??= new List<ItemRule>();
            ItemFiles ??= new List<ItemFile>();
            UnevenRules ??= new List<UnevenRule>();
            Fixes ??= new List<FixEntry>();
            ColorAliases ??= new List<ColorAlias>();
            FileHeaders ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Switches the rule set to 16-bit colour addresses (nothing is lost; the export writes 4-digit hex).</summary>
        public void ConvertTo16Bit()
        {
            ColorBits = 16;
        }

        /// <summary>16 bit: makes the palette at least <paramref name="count"/> entries long (pages of 256).</summary>
        public void EnsurePaletteSize(int count)
        {
            count = Math.Min(MaxColors, (Math.Max(count, 1) + 255) / 256 * 256);
            while (Palette.Count < count)
            {
                Palette.Add(new DragonPaletteEntry { Index = Palette.Count, Name = "frei" });
            }
        }

        /// <summary>All group numbers used by groups.txt, the palette and the rules.</summary>
        public SortedSet<int> AllGroupIds()
        {
            var ids = new SortedSet<int>(Groups.Select(g => g.Id));
            foreach (DragonPaletteEntry p in Palette)
            {
                if (p.Group >= 0)
                {
                    ids.Add(p.Group);
                }
            }
            return ids;
        }

        /// <summary>Hex text of a colour address: 2 digits (8 bit) or 4 digits (16 bit).</summary>
        public string HexIndex(int index) => ColorBits == 16 ? index.ToString("x4") : index.ToString("x2");

        /// <summary>Hex text of a group: 2 digits (8 bit) or 4 digits (16 bit).</summary>
        public string HexGroup(int group) => ColorBits == 16 ? group.ToString("x4") : group.ToString("x2");

        public Color[] GetColors()
        {
            var colors = new Color[Palette.Count];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Palette[i].Color;
            }
            return colors;
        }

        public DragonGroup FindGroup(int id) => Groups.FirstOrDefault(g => g.Id == id);

        public string GroupName(int id)
        {
            if (id < 0)
            {
                return "(alle anderen)";
            }
            DragonGroup g = FindGroup(id);
            return g != null ? g.Name : $"Gruppe {id:x2}";
        }

        public TransitionFile FindTransitionFile(string path)
        {
            return TransitionFiles.FirstOrDefault(f => string.Equals(f.Path, NormalizePath(path), StringComparison.OrdinalIgnoreCase));
        }

        public ItemFile FindItemFile(string path)
        {
            return ItemFiles.FirstOrDefault(f => string.Equals(f.Path, NormalizePath(path), StringComparison.OrdinalIgnoreCase));
        }

        public static string NormalizePath(string path) => (path ?? string.Empty).Replace('\\', '/').Trim();

        /// <summary>Palette indices of a group sorted by altitude (used by the height tools and the generator).</summary>
        public List<DragonPaletteEntry> EntriesOfGroup(int group)
        {
            return Palette.Where(p => p.Defined && p.Group == group).OrderBy(p => p.Altitude).ThenBy(p => p.Index).ToList();
        }

        /// <summary>The defined entry of a group whose altitude is closest to <paramref name="z"/>; -1 if the group has none.</summary>
        public int NearestIndex(int group, int z)
        {
            int best = -1;
            int bestDist = int.MaxValue;
            foreach (DragonPaletteEntry p in Palette)
            {
                if (!p.Defined || p.Group != group)
                {
                    continue;
                }
                int d = Math.Abs(p.Altitude - z);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = p.Index;
                }
            }
            return best;
        }

        /// <summary>For each index: the index of the same group with the next higher / lower altitude.</summary>
        public void BuildHeightLookups(out ushort[] raise, out ushort[] lower)
        {
            raise = new ushort[Palette.Count];
            lower = new ushort[Palette.Count];
            for (int i = 0; i < raise.Length; i++)
            {
                raise[i] = (ushort)i;
                lower[i] = (ushort)i;
            }
            foreach (IGrouping<int, DragonPaletteEntry> grp in Palette.Where(p => p.Defined && p.Group >= 0).GroupBy(p => p.Group))
            {
                List<DragonPaletteEntry> sorted = grp.OrderBy(p => p.Altitude).ThenBy(p => p.Index).ToList();
                for (int k = 0; k < sorted.Count; k++)
                {
                    raise[sorted[k].Index] = (ushort)sorted[Math.Min(k + 1, sorted.Count - 1)].Index;
                    lower[sorted[k].Index] = (ushort)sorted[Math.Max(k - 1, 0)].Index;
                }
            }
        }

        /// <summary>Key of a group pair (both groups up to 7FFF).</summary>
        public static int PairKey(int a, int b) => (a << 16) | (b & 0xFFFF);

        /// <summary>Group pairs (<see cref="PairKey"/>, both directions) that have an active map transition.</summary>
        public HashSet<int> MapTransitionPairs()
        {
            var pairs = new HashSet<int>();
            int[] groups = AllGroupIds().ToArray();
            foreach (TransitionRule r in Transitions)
            {
                if (!r.Enabled || r.Layer != TransitionLayer.Map || FindTransitionFile(r.File) == null)
                {
                    continue;
                }
                foreach (int x in groups)
                {
                    int a = r.GroupA >= 0 ? r.GroupA : x;
                    int b = r.GroupB >= 0 ? r.GroupB : x;
                    if (a != b)
                    {
                        pairs.Add(PairKey(a, b));
                        pairs.Add(PairKey(b, a));
                    }
                    if (r.GroupA >= 0 && r.GroupB >= 0)
                    {
                        break;
                    }
                }
            }
            return pairs;
        }

        /// <summary>
        /// Activates the IMod13 cave entrance transition (map/cave-enti2mountain.txt: entrance group next to mountain)
        /// when it is not in betweentrans yet. Returns true when a rule was added.
        /// </summary>
        public bool EnsureCaveRule(int caveGroup, int mountainGroup)
        {
            if (Transitions.Any(t => t.Layer == TransitionLayer.Map && t.GroupA == caveGroup))
            {
                return false;
            }
            TransitionFile file = FindTransitionFile("map/cave-enti2mountain.txt") ?? FindTransitionFile("map/cave-ent2mountain.txt");
            if (file == null)
            {
                return false;
            }
            Transitions.Add(new TransitionRule
            {
                Layer = TransitionLayer.Map,
                ScriptSet = 1,
                GroupA = caveGroup,
                GroupB = mountainGroup,
                File = file.Path,
                Comment = "Höhleneingang (vom Dragon Map Studio aktiviert)"
            });
            return true;
        }

        /// <summary>First free (undefined) palette index or -1.</summary>
        public int FirstFreeIndex()
        {
            foreach (DragonPaletteEntry p in Palette)
            {
                if (!p.Defined && p.Group < 0)
                {
                    return p.Index;
                }
            }
            foreach (DragonPaletteEntry p in Palette)
            {
                if (!p.Defined)
                {
                    return p.Index;
                }
            }
            if (ColorBits == 16 && Palette.Count < MaxColors)
            {
                int next = Palette.Count;
                EnsurePaletteSize(next + 1);
                return next;
            }
            return -1;
        }

        // ------------------------------------------------------------------ JSON

        private static JsonSerializerOptions JsonOptions => new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        public void SaveJson(string path)
        {
            string dir = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(this, JsonOptions));
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(tmp, path);
        }

        public static DragonRuleSet LoadJson(string path)
        {
            DragonRuleSet set = JsonSerializer.Deserialize<DragonRuleSet>(File.ReadAllText(path), JsonOptions) ?? new DragonRuleSet();
            set.Normalize();
            return set;
        }

        public DragonRuleSet Clone()
        {
            DragonRuleSet copy = JsonSerializer.Deserialize<DragonRuleSet>(JsonSerializer.Serialize(this, JsonOptions), JsonOptions);
            copy.Normalize();
            copy.Warnings.AddRange(Warnings);
            return copy;
        }

        /// <summary>Checks the rule set for typical mistakes. Returns readable messages.</summary>
        public List<string> Validate()
        {
            var problems = new List<string>();
            var groupIds = new HashSet<int>(Groups.Select(g => g.Id));
            foreach (DragonPaletteEntry p in Palette.Where(p => p.Defined))
            {
                if (p.GetTileIds().Length == 0)
                {
                    problems.Add($"Farbe {p.HexIndex}: keine Tile-IDs.");
                }
                if (p.Group < 0)
                {
                    problems.Add($"Farbe {p.HexIndex}: keine Gruppe (groups.txt).");
                }
                else if (!groupIds.Contains(p.Group))
                {
                    problems.Add($"Farbe {p.HexIndex}: Gruppe {p.Group:x2} hat keinen Namen.");
                }
                if (p.Altitude < -128 || p.Altitude > 127)
                {
                    problems.Add($"Farbe {p.HexIndex}: Höhe {p.Altitude} außerhalb -128..127.");
                }
            }
            foreach (TransitionRule r in Transitions)
            {
                if (FindTransitionFile(r.File) == null)
                {
                    problems.Add($"Übergang {r}: Datei fehlt.");
                }
            }
            foreach (ItemRule r in ItemRules)
            {
                if (FindItemFile(r.File) == null)
                {
                    problems.Add($"Items Gruppe {r.Group:x2}: Datei {r.File} fehlt.");
                }
            }
            return problems;
        }
    }
}

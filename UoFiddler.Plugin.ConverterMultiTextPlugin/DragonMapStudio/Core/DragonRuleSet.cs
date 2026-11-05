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

        /// <summary>Always 256 entries (palette index = position).</summary>
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

        public static List<DragonPaletteEntry> CreateEmptyPalette()
        {
            var list = new List<DragonPaletteEntry>(256);
            for (int i = 0; i < 256; i++)
            {
                list.Add(new DragonPaletteEntry { Index = i, Name = "frei" });
            }
            return list;
        }

        /// <summary>Makes sure there are exactly 256 entries with correct indices.</summary>
        public void Normalize()
        {
            Palette ??= CreateEmptyPalette();
            var fixedList = CreateEmptyPalette();
            foreach (DragonPaletteEntry e in Palette)
            {
                if (e != null && e.Index >= 0 && e.Index < 256)
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

        public Color[] GetColors()
        {
            var colors = new Color[256];
            for (int i = 0; i < 256; i++)
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
        public void BuildHeightLookups(out byte[] raise, out byte[] lower)
        {
            raise = new byte[256];
            lower = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                raise[i] = (byte)i;
                lower[i] = (byte)i;
            }
            foreach (IGrouping<int, DragonPaletteEntry> grp in Palette.Where(p => p.Defined && p.Group >= 0).GroupBy(p => p.Group))
            {
                List<DragonPaletteEntry> sorted = grp.OrderBy(p => p.Altitude).ThenBy(p => p.Index).ToList();
                for (int k = 0; k < sorted.Count; k++)
                {
                    raise[sorted[k].Index] = (byte)sorted[Math.Min(k + 1, sorted.Count - 1)].Index;
                    lower[sorted[k].Index] = (byte)sorted[Math.Max(k - 1, 0)].Index;
                }
            }
        }

        /// <summary>Group pairs (a &lt;&lt; 8 | b, both directions) that have an active map transition.</summary>
        public HashSet<int> MapTransitionPairs()
        {
            var pairs = new HashSet<int>();
            foreach (TransitionRule r in Transitions)
            {
                if (!r.Enabled || r.Layer != TransitionLayer.Map || FindTransitionFile(r.File) == null)
                {
                    continue;
                }
                for (int x = 0; x < 256; x++)
                {
                    int a = r.GroupA >= 0 ? r.GroupA : x;
                    int b = r.GroupB >= 0 ? r.GroupB : x;
                    if (a != b)
                    {
                        pairs.Add((a << 8) | b);
                        pairs.Add((b << 8) | a);
                    }
                    if (r.GroupA >= 0 && r.GroupB >= 0)
                    {
                        break;
                    }
                }
            }
            return pairs;
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

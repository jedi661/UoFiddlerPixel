// /***************************************************************************
//  *
//  * Dragon Map Studio – checks and helpers for the rule set (duplicates, repair of hints, examples)
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

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public static class RuleSetTools
    {
        /// <summary>Defined colours that share the same RGB value (a BMP pixel could not tell them apart).</summary>
        public static List<List<DragonPaletteEntry>> DuplicateColors(DragonRuleSet rules)
        {
            // without a palette (all colours the same) there is nothing to compare
            if (rules.Palette.Where(p => p.Defined).Select(p => (p.R << 16) | (p.G << 8) | p.B).Distinct().Count() < 2)
            {
                return new List<List<DragonPaletteEntry>>();
            }
            return rules.Palette.Where(p => p.Defined)
                .GroupBy(p => (p.R << 16) | (p.G << 8) | p.B)
                .Where(g => g.Count() > 1)
                .Select(g => g.ToList())
                .ToList();
        }

        /// <summary>Other defined colours with the same RGB value as <paramref name="rgb"/> (except <paramref name="self"/>).</summary>
        public static List<DragonPaletteEntry> SameColor(DragonRuleSet rules, int rgb, int self)
        {
            return rules.Palette.Where(p => p.Defined && p.Index != self && ((p.R << 16) | (p.G << 8) | p.B) == rgb).ToList();
        }

        /// <summary>A colour that no defined entry uses yet, close to the wanted one.</summary>
        public static System.Drawing.Color UniqueColor(DragonRuleSet rules, System.Drawing.Color wanted, int self)
        {
            var used = new HashSet<int>(rules.Palette.Where(p => p.Defined && p.Index != self).Select(p => (p.R << 16) | (p.G << 8) | p.B));
            for (int step = 0; step < 4096; step++)
            {
                int d = (step + 1) / 2 * (step % 2 == 0 ? 1 : -1);
                int r = Math.Clamp(wanted.R + d, 0, 255), g = Math.Clamp(wanted.G + (step % 3 == 0 ? d : 0), 0, 255), b = Math.Clamp(wanted.B + (step % 5 == 0 ? -d : 0), 0, 255);
                if (!used.Contains((r << 16) | (g << 8) | b))
                {
                    return System.Drawing.Color.FromArgb(r, g, b);
                }
            }
            return wanted;
        }

        /// <summary>Next free group number.</summary>
        public static int FreeGroupId(DragonRuleSet rules)
        {
            SortedSet<int> used = rules.AllGroupIds();
            for (int g = 0; g <= rules.MaxGroup; g++)
            {
                if (!used.Contains(g))
                {
                    return g;
                }
            }
            return -1;
        }

        /// <summary>
        /// Repairs the typical hints: colours without group get the group of a colour with the same tiles
        /// (or of their maptrans section), rules with a missing file point to a similarly named existing file
        /// or are switched off. Returns what was done.
        /// </summary>
        public static List<string> FixHints(DragonRuleSet rules)
        {
            var done = new List<string>();
            foreach (DragonPaletteEntry e in rules.Palette.Where(p => p.Defined && p.Group < 0))
            {
                string tiles = string.Join(" ", e.GetTileIds().OrderBy(t => t));
                DragonPaletteEntry same = rules.Palette.FirstOrDefault(p => p.Defined && p.Group >= 0 && string.Join(" ", p.GetTileIds().OrderBy(t => t)) == tiles)
                                          ?? rules.Palette.FirstOrDefault(p => p.Defined && p.Group >= 0 && !string.IsNullOrEmpty(e.Comment) && p.Comment == e.Comment);
                if (same != null)
                {
                    e.Group = same.Group;
                    done.Add(L.Pick($"Farbe {rules.HexIndex(e.Index)}: Gruppe {rules.HexGroup(same.Group)} ({rules.GroupName(same.Group)}) gesetzt – gleiche Tiles wie {rules.HexIndex(same.Index)}.",
                        $"Colour {rules.HexIndex(e.Index)}: group {rules.HexGroup(same.Group)} ({rules.GroupName(same.Group)}) set – same tiles as {rules.HexIndex(same.Index)}.",
                        $"Couleur {rules.HexIndex(e.Index)} : groupe {rules.HexGroup(same.Group)} ({rules.GroupName(same.Group)}) – mêmes tuiles que {rules.HexIndex(same.Index)}."));
                }
            }
            foreach (TransitionRule r in rules.Transitions.Where(t => t.Enabled && rules.FindTransitionFile(t.File) == null))
            {
                string folder = Path.GetDirectoryName(DragonRuleSet.NormalizePath(r.File))?.Replace('\\', '/') ?? string.Empty;
                string name = Path.GetFileNameWithoutExtension(r.File).ToLowerInvariant();
                TransitionFile similar = rules.TransitionFiles
                    .Where(f => string.Equals(Path.GetDirectoryName(f.Path)?.Replace('\\', '/') ?? string.Empty, folder, StringComparison.OrdinalIgnoreCase))
                    .Select(f => (f, d: Distance(Path.GetFileNameWithoutExtension(f.Path).ToLowerInvariant(), name)))
                    .Where(x => x.d <= 2)
                    .OrderBy(x => x.d)
                    .Select(x => x.f)
                    .FirstOrDefault();
                if (similar != null && !rules.Transitions.Any(t => t != r && t.Layer == r.Layer && t.GroupA == r.GroupA && t.GroupB == r.GroupB && t.File == similar.Path))
                {
                    done.Add(L.Pick($"Übergang {r}: Datei fehlt → ähnliche Datei {similar.Path} benutzt.",
                        $"Transition {r}: file missing → similar file {similar.Path} used.",
                        $"Transition {r} : fichier manquant → fichier semblable {similar.Path} utilisé."));
                    r.File = similar.Path;
                }
                else
                {
                    r.Enabled = false;
                    done.Add(L.Pick($"Übergang {r}: Datei fehlt → Regel ausgeschaltet.",
                        $"Transition {r}: file missing → rule switched off.",
                        $"Transition {r} : fichier manquant → règle désactivée."));
                }
            }
            foreach (ItemRule r in rules.ItemRules.Where(t => t.Enabled && rules.FindItemFile(t.File) == null))
            {
                r.Enabled = false;
                done.Add(L.Pick($"Items Gruppe {rules.HexGroup(r.Group)}: Datei {r.File} fehlt → ausgeschaltet.",
                    $"Items group {rules.HexGroup(r.Group)}: file {r.File} missing → switched off.",
                    $"Objets groupe {rules.HexGroup(r.Group)} : fichier {r.File} manquant → désactivé."));
            }
            if (done.Count > 0)
            {
                rules.Warnings.Clear();
            }
            return done;
        }

        private static int Distance(string a, string b)
        {
            var d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++)
            {
                d[i, 0] = i;
            }
            for (int j = 0; j <= b.Length; j++)
            {
                d[0, j] = j;
            }
            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
                }
            }
            return d[a.Length, b.Length];
        }

        /// <summary>
        /// The 12 standard shapes of a transition (A = own group, B = neighbour; order NW N NE E SE S SW W):
        /// 4 edges, 4 outer corners (only the diagonal is B), 4 inner corners (two edges).
        /// </summary>
        public static readonly (string pattern, string de, string en)[] StandardPatterns =
        {
            ("BBBAAAAA", "Kante Norden", "edge north"),
            ("AABBBAAA", "Kante Osten", "edge east"),
            ("AAAABBBA", "Kante Süden", "edge south"),
            ("BAAAAABB", "Kante Westen", "edge west"),
            ("BAAAAAAA", "Außenecke NW", "outer corner NW"),
            ("AABAAAAA", "Außenecke NO", "outer corner NE"),
            ("AAAABAAA", "Außenecke SO", "outer corner SE"),
            ("AAAAAABA", "Außenecke SW", "outer corner SW"),
            ("BBBBBAAA", "Innenecke N+O", "inner corner N+E"),
            ("AABBBBBA", "Innenecke O+S", "inner corner E+S"),
            ("BAAABBBB", "Innenecke S+W", "inner corner S+W"),
            ("BBBAAABB", "Innenecke W+N", "inner corner W+N")
        };

        /// <summary>Example transition file with the 12 standard shapes; tiles = placeholders to be replaced.</summary>
        public static TransitionFile ExampleTransitionFile(DragonRuleSet rules, string path, int groupA, TransitionLayer layer)
        {
            ushort placeholder = layer == TransitionLayer.Map
                ? rules.EntriesOfGroup(groupA).SelectMany(e => e.GetTileIds()).FirstOrDefault()
                : (ushort)0x0001;
            var file = new TransitionFile
            {
                Path = DragonRuleSet.NormalizePath(path),
                HeaderComment = "Dragon Map Studio – Beispiel / example: 12 Standard-Formen. Tiles ersetzen (Katalog) / replace the tiles."
            };
            foreach ((string pattern, string de, string en) in StandardPatterns)
            {
                file.Patterns.Add(new TransitionPattern
                {
                    Pattern = pattern,
                    Tiles = placeholder.ToString("x4"),
                    Z1 = 0,
                    Z2 = 0,
                    Comment = L.Pick(de, en, en)
                });
            }
            return file;
        }

        /// <summary>Static ids placed by the item rules of a group (for the preview), at most <paramref name="max"/>.</summary>
        public static List<ushort> ItemIdsOfGroup(DragonRuleSet rules, int group, int max = 24)
        {
            var ids = new List<ushort>();
            foreach (ItemRule r in rules.ItemRules.Where(r => r.Enabled && r.Group == group))
            {
                ItemFile f = rules.FindItemFile(r.File);
                if (f == null)
                {
                    continue;
                }
                foreach (ItemGroupDef g in f.Groups)
                {
                    foreach (ItemPart p in g.Parts)
                    {
                        if (!ids.Contains(p.Id))
                        {
                            ids.Add(p.Id);
                            if (ids.Count >= max)
                            {
                                return ids;
                            }
                        }
                    }
                }
            }
            return ids;
        }

        /// <summary>Static ids of the static transitions that start at a group (e.g. shore waves), at most <paramref name="max"/>.</summary>
        public static List<ushort> TransitionStaticIdsOfGroup(DragonRuleSet rules, int group, int max = 12)
        {
            var ids = new List<ushort>();
            foreach (TransitionRule r in rules.Transitions.Where(t => t.Enabled && t.Layer == TransitionLayer.Static && t.GroupA == group))
            {
                TransitionFile f = rules.FindTransitionFile(r.File);
                foreach (ushort id in f?.Patterns.SelectMany(p => p.GetTileIds()) ?? Enumerable.Empty<ushort>())
                {
                    if (!ids.Contains(id))
                    {
                        ids.Add(id);
                        if (ids.Count >= max)
                        {
                            return ids;
                        }
                    }
                }
            }
            return ids;
        }
    }
}

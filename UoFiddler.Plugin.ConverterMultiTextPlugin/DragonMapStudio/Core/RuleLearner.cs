/***************************************************************************
 *
 *  * Dragon Map Studio – learn Dragon rules / scripts from a built area
 *  *
 *  * Input: land tiles + heights of a CGI area (or of a stamp made from one), the
 *  * colour of every cell (-> group) and the statics. Learned, like a person who
 *  * reads the original map:
 *  *   - map transitions:    cell of group A with neighbours of group B (8-neighbour
 *  *                         pattern, A/B) -> the tile that lies there, height offset
 *  *                         -> map/lernen_AA2BB.txt + rule A -> B (fills patterns the
 *  *                         existing rule does not have: it is used after it)
 *  *   - items:              statics standing inside a group (all 8 neighbours the same
 *  *                         group) -> item groups with frequency + rule with the density
 *  *                         -> items/lernen_AA.txt + items.txt line
 *  *   - static transitions: statics standing on cells at a group border -> pattern ->
 *  *                         static ids and height -> statics/lernen_AA2BB.txt + rule
 *  * Nothing is applied before the user ticked it (LearnedItem.Accept).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public sealed class LearnOptions
    {
        public bool MapTransitions { get; set; } = true;
        public bool Items { get; set; } = true;
        public bool StaticTransitions { get; set; } = true;

        /// <summary>A pattern / item group needs at least this many examples.</summary>
        public int MinCount { get; set; } = 2;

        /// <summary>Name part of the new files (e.g. the stamp name).</summary>
        public string Name { get; set; } = "lernen";
    }

    public enum LearnedKind
    {
        MapTransition,
        Items,
        StaticTransition
    }

    /// <summary>One thing that can be added to the rule set (file + rule).</summary>
    public sealed class LearnedItem
    {
        public LearnedKind Kind { get; set; }
        public bool Accept { get; set; } = true;
        public string Description { get; set; } = string.Empty;
        public TransitionFile TransitionFile { get; set; }
        public TransitionRule Rule { get; set; }
        public ItemFile ItemFile { get; set; }
        public ItemRule ItemRule { get; set; }

        public override string ToString() => Description;
    }

    public static class RuleLearner
    {
        private static readonly int[] NbDx = { -1, 0, 1, 1, 1, 0, -1, -1 };
        private static readonly int[] NbDy = { -1, -1, -1, 0, 1, 1, 1, 0 };

        private static string SafeName(string name)
        {
            string s = new string((name ?? "lernen").ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray()).Trim('_');
            return s.Length == 0 ? "lernen" : s.Length > 24 ? s.Substring(0, 24) : s;
        }

        /// <param name="tiles">Land tile of every cell (w × h).</param>
        /// <param name="z">Land height of every cell.</param>
        /// <param name="colours">Colour address of every cell (its group is used).</param>
        /// <param name="statics">Statics (X / Y inside the area, Dz = height above the land of the cell, or absolute).</param>
        public static List<LearnedItem> Learn(int w, int h, ushort[] tiles, sbyte[] z, ushort[] colours, IReadOnlyList<LayerStatic> statics,
            DragonRuleSet rules, LearnOptions o, List<string> log)
        {
            var result = new List<LearnedItem>();
            string name = SafeName(o.Name);
            var group = new int[w * h];
            var altitude = new int[w * h];
            for (int i = 0; i < group.Length; i++)
            {
                DragonPaletteEntry e = colours[i] < rules.Palette.Count ? rules.Palette[colours[i]] : null;
                group[i] = e != null && e.Defined ? e.Group : -1;
                altitude[i] = e?.Altitude ?? 0;
            }
            int G(int x, int y, int own) => x < 0 || y < 0 || x >= w || y >= h || group[y * w + x] < 0 ? own : group[y * w + x];
            var plainTiles = new Dictionary<int, HashSet<ushort>>();
            foreach (IGrouping<int, DragonPaletteEntry> g in rules.Palette.Where(p => p.Defined && p.Group >= 0).GroupBy(p => p.Group))
            {
                plainTiles[g.Key] = new HashSet<ushort>(g.SelectMany(e => e.GetTileIds()));
            }

            // the foreign group with the most neighbours and the A/B mask for it (other foreign groups count as A, like the compiler)
            (int b, byte mask) Border(int x, int y, int a)
            {
                var count = new Dictionary<int, int>();
                for (int k = 0; k < 8; k++)
                {
                    int ng = G(x + NbDx[k], y + NbDy[k], a);
                    if (ng != a)
                    {
                        count[ng] = count.TryGetValue(ng, out int c) ? c + 1 : 1;
                    }
                }
                if (count.Count == 0)
                {
                    return (-1, 0);
                }
                int bg = count.OrderByDescending(kv => kv.Value).First().Key;
                byte m = 0;
                for (int k = 0; k < 8; k++)
                {
                    if (G(x + NbDx[k], y + NbDy[k], a) == bg)
                    {
                        m |= (byte)(1 << k);
                    }
                }
                return (bg, m);
            }

            // statics per cell
            var cellStatics = new Dictionary<int, List<LayerStatic>>();
            foreach (LayerStatic s in statics ?? Array.Empty<LayerStatic>())
            {
                if (s.X < 0 || s.Y < 0 || s.X >= w || s.Y >= h)
                {
                    continue;
                }
                int i = s.Y * w + s.X;
                if (!cellStatics.TryGetValue(i, out List<LayerStatic> l))
                {
                    cellStatics[i] = l = new List<LayerStatic>();
                }
                l.Add(s);
            }
            int DzOf(LayerStatic s) => s.Absolute ? s.Dz - z[s.Y * w + s.X] : s.Dz;

            var mapSamples = new Dictionary<(int a, int b), Dictionary<byte, (Dictionary<ushort, int> tiles, List<int> dz)>>();
            var statSamples = new Dictionary<(int a, int b), Dictionary<byte, (Dictionary<ushort, int> ids, List<int> dz)>>();
            var itemSets = new Dictionary<int, Dictionary<string, (int count, List<ItemPart> parts)>>();
            var interiorCells = new Dictionary<int, int>();
            var interiorWithStatics = new Dictionary<int, int>();

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    int a = group[i];
                    if (a < 0)
                    {
                        continue;
                    }
                    (int b, byte mask) = Border(x, y, a);
                    cellStatics.TryGetValue(i, out List<LayerStatic> here);
                    if (b < 0)
                    {
                        // inside the group: items
                        interiorCells[a] = interiorCells.TryGetValue(a, out int ic) ? ic + 1 : 1;
                        if (o.Items && here != null && here.Count > 0)
                        {
                            interiorWithStatics[a] = interiorWithStatics.TryGetValue(a, out int iw) ? iw + 1 : 1;
                            List<ItemPart> parts = here.OrderBy(s => DzOf(s)).ThenBy(s => s.Id)
                                                       .Select(s => new ItemPart { Id = s.Id, Z = DzOf(s), Hue = s.Hue }).ToList();
                            string key = string.Join(";", parts.Select(p => $"{p.Id:x4}/{p.Z}/{p.Hue:x}"));
                            if (!itemSets.TryGetValue(a, out Dictionary<string, (int, List<ItemPart>)> sets))
                            {
                                itemSets[a] = sets = new Dictionary<string, (int, List<ItemPart>)>();
                            }
                            sets[key] = sets.TryGetValue(key, out (int count, List<ItemPart> parts) old) ? (old.count + 1, old.parts) : (1, parts);
                        }
                        continue;
                    }
                    // border cell
                    if (o.MapTransitions && !(plainTiles.TryGetValue(a, out HashSet<ushort> plain) && plain.Contains(tiles[i])))
                    {
                        if (!mapSamples.TryGetValue((a, b), out var masks))
                        {
                            mapSamples[(a, b)] = masks = new Dictionary<byte, (Dictionary<ushort, int>, List<int>)>();
                        }
                        if (!masks.TryGetValue(mask, out var sample))
                        {
                            masks[mask] = sample = (new Dictionary<ushort, int>(), new List<int>());
                        }
                        sample.tiles[tiles[i]] = sample.tiles.TryGetValue(tiles[i], out int tc) ? tc + 1 : 1;
                        sample.dz.Add(z[i] - altitude[i]);
                    }
                    if (o.StaticTransitions && here != null)
                    {
                        // height base like the compiler (StaticZ = SourceLowest): lowest land of group A around the cell
                        int lowest = int.MaxValue;
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int nx = x + dx, ny = y + dy;
                                if (nx >= 0 && ny >= 0 && nx < w && ny < h && group[ny * w + nx] == a)
                                {
                                    lowest = Math.Min(lowest, z[ny * w + nx]);
                                }
                            }
                        }
                        if (lowest == int.MaxValue)
                        {
                            lowest = z[i];
                        }
                        if (!statSamples.TryGetValue((a, b), out var smasks))
                        {
                            statSamples[(a, b)] = smasks = new Dictionary<byte, (Dictionary<ushort, int>, List<int>)>();
                        }
                        if (!smasks.TryGetValue(mask, out var ss))
                        {
                            smasks[mask] = ss = (new Dictionary<ushort, int>(), new List<int>());
                        }
                        foreach (LayerStatic s in here)
                        {
                            ss.ids[s.Id] = ss.ids.TryGetValue(s.Id, out int c) ? c + 1 : 1;
                            ss.dz.Add(z[i] + DzOf(s) - lowest);
                        }
                    }
                }
            }

            static int Median(List<int> l)
            {
                if (l.Count == 0)
                {
                    return 0;
                }
                var c = l.OrderBy(v => v).ToList();
                return c[c.Count / 2];
            }

            // ------------------------------------------------ map transitions
            foreach (var kv in mapSamples.OrderBy(k => k.Key.a).ThenBy(k => k.Key.b))
            {
                var file = new TransitionFile
                {
                    Path = $"map/lernen_{name}_{rules.HexGroup(kv.Key.a)}_zu_{rules.HexGroup(kv.Key.b)}.txt",
                    HeaderComment = $"// Dragon Map Studio: gelernt aus '{o.Name}' – Zelle Gruppe {rules.HexGroup(kv.Key.a)} neben {rules.HexGroup(kv.Key.b)}"
                };
                int examples = 0;
                foreach (var m in kv.Value.OrderBy(m => m.Key))
                {
                    int n = m.Value.tiles.Values.Sum();
                    if (n < o.MinCount)
                    {
                        continue;
                    }
                    examples += n;
                    int dz = Median(m.Value.dz);
                    // only the tiles that really belong to this pattern: at least 15 % of the examples (the rest came from other rules)
                    int max = m.Value.tiles.Values.Max();
                    var keep = m.Value.tiles.Where(t => t.Value * 100 >= n * 15 || t.Value == max).OrderByDescending(t => t.Value).Select(t => t.Key.ToString("x4"));
                    file.Patterns.Add(new TransitionPattern
                    {
                        Pattern = TransitionPattern.MaskToPattern(m.Key),
                        Tiles = string.Join(" ", keep),
                        Z1 = dz,
                        Z2 = dz,
                        Comment = $"{n}×"
                    });
                }
                if (file.Patterns.Count == 0)
                {
                    continue;
                }
                bool exists = rules.Transitions.Any(t => t.Layer == TransitionLayer.Map && t.GroupA == kv.Key.a && t.GroupB == kv.Key.b);
                result.Add(new LearnedItem
                {
                    Kind = LearnedKind.MapTransition,
                    TransitionFile = file,
                    Rule = new TransitionRule { Layer = TransitionLayer.Map, ScriptSet = 1, GroupA = kv.Key.a, GroupB = kv.Key.b, File = file.Path, Comment = "gelernt: " + o.Name },
                    Description = $"Map-Übergang {rules.HexGroup(kv.Key.a)} {rules.GroupName(kv.Key.a)} → {rules.HexGroup(kv.Key.b)} {rules.GroupName(kv.Key.b)}: " +
                                  $"{file.Patterns.Count} Muster aus {examples} Zellen → {file.Path}" + (exists ? " (ergänzt die vorhandene Regel)" : string.Empty)
                });
            }

            // ------------------------------------------------ items
            foreach (var kv in itemSets.OrderBy(k => k.Key))
            {
                int a = kv.Key;
                var sets = kv.Value.Where(s => s.Value.count >= o.MinCount).OrderByDescending(s => s.Value.count).ToList();
                if (sets.Count == 0)
                {
                    continue;
                }
                int inner = interiorCells.TryGetValue(a, out int ic) ? ic : 0;
                int withStatics = sets.Sum(s => s.Value.count);
                int chance = Math.Clamp((int)Math.Round(1000.0 * withStatics / Math.Max(1, inner)), 1, 1000);
                var file = new ItemFile
                {
                    Path = $"items/lernen_{name}_{rules.HexGroup(a)}.txt",
                    HeaderComment = $"// Dragon Map Studio: gelernt aus '{o.Name}' – Statics auf Gruppe {rules.HexGroup(a)} {rules.GroupName(a)}"
                };
                foreach (var s in sets)
                {
                    file.Groups.Add(new ItemGroupDef { Frequency = s.Value.count, Parts = s.Value.parts, Comment = $"{s.Value.count}×" });
                }
                result.Add(new LearnedItem
                {
                    Kind = LearnedKind.Items,
                    ItemFile = file,
                    ItemRule = new ItemRule { Group = a, Chance = chance, File = file.Path },
                    Description = $"Items auf {rules.HexGroup(a)} {rules.GroupName(a)}: {sets.Count} Arten, {withStatics} von {inner} Zellen " +
                                  $"(Chance {chance} ‰) → {file.Path}"
                });
            }

            // ------------------------------------------------ static transitions
            foreach (var kv in statSamples.OrderBy(k => k.Key.a).ThenBy(k => k.Key.b))
            {
                var file = new TransitionFile
                {
                    Path = $"statics/lernen_{name}_{rules.HexGroup(kv.Key.a)}_zu_{rules.HexGroup(kv.Key.b)}.txt",
                    HeaderComment = $"// Dragon Map Studio: gelernt aus '{o.Name}' – Statics am Rand {rules.HexGroup(kv.Key.a)} → {rules.HexGroup(kv.Key.b)}"
                };
                int examples = 0;
                foreach (var m in kv.Value.OrderBy(m => m.Key))
                {
                    int n = m.Value.ids.Values.Sum();
                    if (n < o.MinCount)
                    {
                        continue;
                    }
                    examples += n;
                    int dz = Median(m.Value.dz);
                    file.Patterns.Add(new TransitionPattern
                    {
                        Pattern = TransitionPattern.MaskToPattern(m.Key),
                        Tiles = string.Join(" ", m.Value.ids.OrderByDescending(t => t.Value).Take(12).Select(t => t.Key.ToString("x4"))),
                        Z1 = dz,
                        Z2 = dz,
                        Comment = $"{n}×"
                    });
                }
                if (file.Patterns.Count == 0)
                {
                    continue;
                }
                result.Add(new LearnedItem
                {
                    Kind = LearnedKind.StaticTransition,
                    TransitionFile = file,
                    Rule = new TransitionRule { Layer = TransitionLayer.Static, ScriptSet = 1, GroupA = kv.Key.a, GroupB = kv.Key.b, File = file.Path, Comment = "gelernt: " + o.Name },
                    Description = $"Static-Übergang {rules.HexGroup(kv.Key.a)} {rules.GroupName(kv.Key.a)} → {rules.HexGroup(kv.Key.b)} {rules.GroupName(kv.Key.b)}: " +
                                  $"{file.Patterns.Count} Muster aus {examples} Statics → {file.Path}"
                });
            }
            log?.Add($"Gelernt aus '{o.Name}' ({w}×{h}): {result.Count(r => r.Kind == LearnedKind.MapTransition)} Map-Übergänge, " +
                     $"{result.Count(r => r.Kind == LearnedKind.Items)} Item-Dateien, {result.Count(r => r.Kind == LearnedKind.StaticTransition)} Static-Übergänge.");
            return result;
        }

        /// <summary>Adds the accepted files and rules to the rule set (files with the same path are replaced). Returns log lines.</summary>
        public static List<string> Apply(DragonRuleSet rules, IEnumerable<LearnedItem> items)
        {
            var log = new List<string>();
            foreach (LearnedItem it in items.Where(i => i.Accept))
            {
                if (it.TransitionFile != null)
                {
                    rules.TransitionFiles.RemoveAll(f => string.Equals(f.Path, it.TransitionFile.Path, StringComparison.OrdinalIgnoreCase));
                    rules.TransitionFiles.Add(it.TransitionFile);
                    if (!rules.Transitions.Any(t => t.Layer == it.Rule.Layer && t.GroupA == it.Rule.GroupA && t.GroupB == it.Rule.GroupB &&
                                                    string.Equals(t.File, it.Rule.File, StringComparison.OrdinalIgnoreCase)))
                    {
                        // right after the existing rules of the pair (they keep priority, the learned file fills the missing patterns);
                        // a new pair goes before the other rules of its cell group, so a general "A next to any group" rule does not win
                        int last = rules.Transitions.FindLastIndex(t => t.Layer == it.Rule.Layer && t.GroupA == it.Rule.GroupA && t.GroupB == it.Rule.GroupB);
                        int firstOfA = rules.Transitions.FindIndex(t => t.Layer == it.Rule.Layer && t.GroupA == it.Rule.GroupA);
                        int at = last >= 0 ? last + 1 : firstOfA >= 0 ? firstOfA : rules.Transitions.Count;
                        rules.Transitions.Insert(at, it.Rule);
                    }
                }
                if (it.ItemFile != null)
                {
                    rules.ItemFiles.RemoveAll(f => string.Equals(f.Path, it.ItemFile.Path, StringComparison.OrdinalIgnoreCase));
                    rules.ItemFiles.Add(it.ItemFile);
                    if (!rules.ItemRules.Any(r => r.Group == it.ItemRule.Group && string.Equals(r.File, it.ItemRule.File, StringComparison.OrdinalIgnoreCase)))
                    {
                        rules.ItemRules.Add(it.ItemRule);
                    }
                }
                log.Add("übernommen: " + it.Description);
            }
            return log;
        }

        public static string Report(IEnumerable<LearnedItem> items, DragonRuleSet rules)
        {
            var sb = new StringBuilder();
            foreach (LearnedItem it in items)
            {
                sb.AppendLine((it.Accept ? "[x] " : "[ ] ") + it.Description);
                if (it.TransitionFile != null)
                {
                    foreach (TransitionPattern p in it.TransitionFile.Patterns.Take(60))
                    {
                        sb.AppendLine($"      {p.Pattern} {p.Tiles} {p.Z1:000} {p.Z2:000}   // {p.Comment}");
                    }
                }
                if (it.ItemFile != null)
                {
                    sb.AppendLine($"      items.txt: {rules.HexGroup(it.ItemRule.Group)} {it.ItemRule.Chance:0000} … {it.ItemFile.Path}");
                    foreach (ItemGroupDef g in it.ItemFile.Groups.Take(30))
                    {
                        sb.AppendLine($"      {g.Frequency:0000} {g.PartsText}");
                    }
                }
            }
            return sb.ToString();
        }
    }
}

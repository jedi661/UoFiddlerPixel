/***************************************************************************
 *
 *  * Dragon Map Studio – built map (land tiles + heights + statics) back to colours
 *  *
 *  * For every land cell the colour of the colour table is searched that builds
 *  * this tile on this height:
 *  *   1. a colour whose tiles contain the tile, altitude close to the height,
 *  *   2. the tile is a transition tile of a rule -> the cell group of the rule
 *  *      (the compiler makes the transition again),
 *  *   3. otherwise: suggestion for a new colour (tiledata name, radar colour,
 *  *      group of a colour with the same tile name or a new group).
 *  * What happens with a missing colour: suggest + confirm, create automatically,
 *  * or take the most similar existing colour (same radar colour, height).
 *  * Statics: own statics layer (1:1), land only, or a Dragon item rule.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public enum StaticsMode
    {
        [Description("Statics-Ebene: 1:1 mit Höhe über dem Land (empfohlen)")]
        Layer,

        [Description("Nur Land: Statics weglassen")]
        LandOnly,

        [Description("Dragon-Item-Regel: Statics als Item-Gruppe (zufällig gesetzt)")]
        ItemRule
    }

    public enum MissingColorMode
    {
        [Description("Vorschlag + Bestätigen (empfohlen)")]
        Suggest,

        [Description("Fehlende Farben automatisch anlegen")]
        AutoCreate,

        [Description("Nur vorhandene Farben (ähnlichste)")]
        ExistingOnly
    }

    public sealed class ColorSuggestion
    {
        public ushort Tile { get; set; }
        public int Altitude { get; set; }
        public int Count { get; set; }
        public string TileName { get; set; } = string.Empty;
        public Color Radar { get; set; }
        public int Group { get; set; } = -1;
        public string GroupName { get; set; } = string.Empty;
        public bool NewGroup { get; set; }
        public bool Accept { get; set; } = true;
        public int CreatedIndex { get; set; } = -1;

        public override string ToString() => $"{Tile:x4} Z{Altitude} {TileName} ×{Count} → {(NewGroup ? "neue Gruppe " : "Gruppe ")}{GroupName}";
    }

    public sealed class CgiConversion
    {
        public Rectangle Area;
        public ushort[] Colours;
        public int Resolved;
        public int ByTile;
        public int ByTransition;
        public int BySimilar;
        public List<ColorSuggestion> Suggestions = new List<ColorSuggestion>();
        public List<LayerStatic> Statics = new List<LayerStatic>();
        public List<string> Log = new List<string>();

        /// <summary>Cells still without colour (index 0xFFFF) – filled by <see cref="CgiToColor.Finish"/>.</summary>
        public int Unresolved => Colours.Count(c => c == ushort.MaxValue);
    }

    public static class CgiToColor
    {
        /// <summary>Altitude of a colour may differ this much from the land height (transitions add a few Z).</summary>
        public const int Tolerance = 4;

        /// <summary>Suggested colours are made for heights in steps of this size (fewer new colours).</summary>
        public const int AltitudeStep = 5;

        private sealed class Index
        {
            public readonly Dictionary<ushort, List<DragonPaletteEntry>> ByTile = new Dictionary<ushort, List<DragonPaletteEntry>>();
            public readonly Dictionary<int, List<DragonPaletteEntry>> ByGroup = new Dictionary<int, List<DragonPaletteEntry>>();
            public readonly Dictionary<string, int> GroupByTileName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            public readonly List<(DragonPaletteEntry e, Color radar)> Radar = new List<(DragonPaletteEntry, Color)>();
        }

        private static Index BuildIndex(DragonRuleSet rules)
        {
            var ix = new Index();
            foreach (DragonPaletteEntry e in rules.Palette.Where(p => p.Defined && p.Group >= 0))
            {
                ushort[] tiles = e.GetTileIds();
                foreach (ushort t in tiles.Distinct())
                {
                    if (!ix.ByTile.TryGetValue(t, out List<DragonPaletteEntry> l))
                    {
                        ix.ByTile[t] = l = new List<DragonPaletteEntry>();
                    }
                    l.Add(e);
                    string name = SafeName(t);
                    if (name.Length > 0)
                    {
                        ix.GroupByTileName.TryAdd(name, e.Group);
                    }
                }
                if (!ix.ByGroup.TryGetValue(e.Group, out List<DragonPaletteEntry> g))
                {
                    ix.ByGroup[e.Group] = g = new List<DragonPaletteEntry>();
                }
                g.Add(e);
                if (tiles.Length > 0)
                {
                    ix.Radar.Add((e, SafeRadar(tiles[0])));
                }
            }
            return ix;
        }

        private static string SafeName(ushort id)
        {
            try
            {
                return (UoArtCache.LandName(id) ?? string.Empty).Trim();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static Color SafeRadar(ushort id)
        {
            try
            {
                return UoArtCache.LandColor(id);
            }
            catch (Exception)
            {
                return Color.Gray;
            }
        }

        private static DragonPaletteEntry Nearest(List<DragonPaletteEntry> l, int z) => l.OrderBy(e => Math.Abs(e.Altitude - z)).First();

        /// <summary>
        /// Colours for every cell of <paramref name="area"/> (still 0xFFFF where no colour fits), the suggestions for the missing ones
        /// and the statics (relative to the area).
        /// </summary>
        public static CgiConversion Analyze(MapData map, Rectangle area, DragonRuleSet rules, LandTransitionEngine transitions, bool withStatics,
            IDictionary<ushort, int> extraTileGroups = null)
        {
            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            var conv = new CgiConversion { Area = area, Colours = new ushort[area.Width * area.Height] };
            Index ix = BuildIndex(rules);
            var cache = new Dictionary<int, int>();
            var missing = new Dictionary<int, ColorSuggestion>();
            for (int y = 0; y < area.Height; y++)
            {
                for (int x = 0; x < area.Width; x++)
                {
                    ushort tile = map.LandId(area.X + x, area.Y + y);
                    int z = map.LandZ(area.X + x, area.Y + y);
                    int key = (tile << 8) | (z & 0xFF);
                    if (!cache.TryGetValue(key, out int colour))
                    {
                        colour = Match(ix, transitions, tile, z, out int how);
                        if (colour < 0 && extraTileGroups != null && extraTileGroups.TryGetValue(tile, out int eg) && ix.ByGroup.TryGetValue(eg, out List<DragonPaletteEntry> el))
                        {
                            // tiles the compiler makes itself (e.g. sea floor of the coast profile) -> their group
                            colour = Nearest(el, z).Index;
                            how = 2;
                        }
                        cache[key] = colour;
                        cache[key | 0x4000_0000] = how;
                    }
                    int i = y * area.Width + x;
                    if (colour >= 0)
                    {
                        conv.Colours[i] = (ushort)colour;
                        conv.Resolved++;
                        switch (cache[key | 0x4000_0000])
                        {
                            case 1: conv.ByTile++; break;
                            case 2: conv.ByTransition++; break;
                        }
                        continue;
                    }
                    conv.Colours[i] = ushort.MaxValue;
                    int alt = (int)Math.Round(z / (double)AltitudeStep) * AltitudeStep;
                    int mk = (tile << 8) | (alt & 0xFF);
                    if (!missing.TryGetValue(mk, out ColorSuggestion s))
                    {
                        string name = SafeName(tile);
                        int group = ix.GroupByTileName.TryGetValue(name, out int g) ? g : -1;
                        missing[mk] = s = new ColorSuggestion
                        {
                            Tile = tile,
                            Altitude = alt,
                            TileName = name.Length > 0 ? name : $"Tile {tile:x4}",
                            Radar = SafeRadar(tile),
                            Group = group,
                            GroupName = group >= 0 ? rules.GroupName(group) : (name.Length > 0 ? name : $"Tile {tile:x4}"),
                            NewGroup = group < 0
                        };
                    }
                    s.Count++;
                }
            }
            conv.Suggestions = missing.Values.OrderByDescending(s => s.Count).ToList();
            if (withStatics)
            {
                for (int y = area.Top; y < area.Bottom; y++)
                {
                    for (int x = area.Left; x < area.Right; x++)
                    {
                        int land = map.LandZ(x, y);
                        foreach (StaticItem st in map.StaticsAt(x, y))
                        {
                            conv.Statics.Add(new LayerStatic { X = x - area.X, Y = y - area.Y, Dz = st.Z - land, Id = st.Id, Hue = st.Hue });
                        }
                    }
                }
            }
            conv.Log.Add("Hinweis: Gruppen mit denselben Tiles (z. B. Gras / Wald, Sand / Wüste) sind am Land nicht zu unterscheiden – es wird die erste passende Farbe genommen, gebaut ergibt sie dasselbe Tile.");
            conv.Log.Add($"Bereich {area.Width}×{area.Height}: {conv.ByTile:N0} Zellen über das Tile, {conv.ByTransition:N0} über Übergangs-Regeln, " +
                         $"{conv.Colours.Length - conv.Resolved:N0} ohne passende Farbe ({conv.Suggestions.Count} Vorschläge), {conv.Statics.Count:N0} Statics.");
            return conv;
        }

        /// <param name="how">1 = colour with this tile, 2 = transition tile of a group.</param>
        private static int Match(Index ix, LandTransitionEngine transitions, ushort tile, int z, out int how)
        {
            how = 0;
            if (ix.ByTile.TryGetValue(tile, out List<DragonPaletteEntry> l))
            {
                DragonPaletteEntry e = Nearest(l, z);
                if (Math.Abs(e.Altitude - z) <= Tolerance)
                {
                    how = 1;
                    return e.Index;
                }
            }
            int group = transitions?.GroupOf(tile) ?? -1;
            if (group >= 0 && transitions.IsTransitionTile(tile) && ix.ByGroup.TryGetValue(group, out List<DragonPaletteEntry> g))
            {
                // transition tiles get their height offset from the rule (+5…15 at mountain edges): a bigger tolerance
                DragonPaletteEntry e = Nearest(g, z);
                if (Math.Abs(e.Altitude - z) <= Tolerance + 12)
                {
                    how = 2;
                    return e.Index;
                }
            }
            return -1;
        }

        /// <summary>Creates the accepted suggestions as new colours (and new groups). Returns the log lines.</summary>
        public static List<string> CreateColours(DragonRuleSet rules, IEnumerable<ColorSuggestion> suggestions)
        {
            var log = new List<string>();
            var newGroups = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (ColorSuggestion s in suggestions.Where(s => s.Accept))
            {
                int group = s.Group;
                if (s.NewGroup || group < 0)
                {
                    if (!newGroups.TryGetValue(s.GroupName, out group))
                    {
                        group = RuleSetTools.FreeGroupId(rules);
                        if (group < 0)
                        {
                            log.Add("Keine freie Gruppe mehr – " + s);
                            continue;
                        }
                        rules.Groups.Add(new DragonGroup { Id = group, Name = s.GroupName, Comment = "aus der CGI-Ansicht übernommen" });
                        newGroups[s.GroupName] = group;
                        log.Add($"Neue Gruppe {rules.HexGroup(group)} '{s.GroupName}'.");
                    }
                }
                int index = rules.FirstFreeIndex();
                if (index < 0)
                {
                    log.Add("Keine freie Farbadresse mehr (16 Bit nutzen) – " + s);
                    break;
                }
                DragonPaletteEntry e = rules.Palette[index];
                e.Defined = true;
                e.SetColor(RuleSetTools.UniqueColor(rules, s.Radar, index));
                e.Name = $"{s.TileName} Z{s.Altitude}";
                e.Category = "Aus CGI";
                e.Group = group;
                e.Altitude = s.Altitude;
                e.Tiles = s.Tile.ToString("x4");
                e.Comment = $"Dragon Map Studio: {s.Count} Zellen";
                s.CreatedIndex = index;
                log.Add($"Farbe {rules.HexIndex(index)} '{e.Name}' (Gruppe {rules.HexGroup(group)}, Tile {s.Tile:x4}, Z {s.Altitude}) angelegt.");
            }
            return log;
        }

        /// <summary>
        /// Fills the cells that are still open: with a created colour of the same tile, otherwise with the existing colour
        /// whose first tile has the closest radar colour (and the closest altitude).
        /// </summary>
        public static void Finish(CgiConversion conv, MapData map, DragonRuleSet rules, LandTransitionEngine transitions)
        {
            Index ix = BuildIndex(rules);
            var cache = new Dictionary<int, int>();
            for (int y = 0; y < conv.Area.Height; y++)
            {
                for (int x = 0; x < conv.Area.Width; x++)
                {
                    int i = y * conv.Area.Width + x;
                    if (conv.Colours[i] != ushort.MaxValue)
                    {
                        continue;
                    }
                    ushort tile = map.LandId(conv.Area.X + x, conv.Area.Y + y);
                    int z = map.LandZ(conv.Area.X + x, conv.Area.Y + y);
                    int key = (tile << 8) | (z & 0xFF);
                    if (!cache.TryGetValue(key, out int c))
                    {
                        // a colour created for this tile (any height)?
                        if (ix.ByTile.TryGetValue(tile, out List<DragonPaletteEntry> l))
                        {
                            c = Nearest(l, z).Index;
                        }
                        else if (ix.Radar.Count > 0)
                        {
                            Color want = SafeRadar(tile);
                            c = ix.Radar.OrderBy(r => ColorDistance(r.radar, want) * 4 + Math.Abs(r.e.Altitude - z)).First().e.Index;
                            conv.BySimilar++;
                        }
                        else
                        {
                            c = 0;
                        }
                        cache[key] = c;
                    }
                    conv.Colours[i] = (ushort)c;
                }
            }
        }

        /// <summary>Land tiles and heights of an area of a built map.</summary>
        public static (ushort[] tiles, sbyte[] z) Cut(MapData map, Rectangle a)
        {
            var t = new ushort[a.Width * a.Height];
            var z = new sbyte[a.Width * a.Height];
            for (int y = 0; y < a.Height; y++)
            {
                for (int x = 0; x < a.Width; x++)
                {
                    t[y * a.Width + x] = map.LandId(a.X + x, a.Y + y);
                    z[y * a.Width + x] = (sbyte)map.LandZ(a.X + x, a.Y + y);
                }
            }
            return (t, z);
        }

        private static int ColorDistance(Color a, Color b) => Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B);

        /// <summary>The finished conversion as a colour stamp; statics as layer, left out or as a new Dragon item rule.</summary>
        public static ColorStamp ToStamp(CgiConversion conv, DragonRuleSet rules, int waterGroup, string name, string category, StaticsMode statics, List<string> log,
            MapData source = null)
        {
            var cut = new IndexedMap(conv.Area.Width, conv.Area.Height, conv.Colours, rules.ColorBits);
            ColorStamp stamp = ColorStampTools.FromMap(cut, rules, waterGroup, name, category);
            if (source != null)
            {
                // the built tiles go with the stamp: rules can be learned from it later
                (stamp.LandTiles, stamp.LandZ) = Cut(source, conv.Area);
            }
            if (statics == StaticsMode.Layer)
            {
                stamp.Statics = conv.Statics.ToList();
            }
            else if (statics == StaticsMode.ItemRule && conv.Statics.Count > 0)
            {
                CreateItemRule(conv, cut, rules, name, log);
            }
            return stamp;
        }

        /// <summary>
        /// Statics as one item group (all statics relative to the middle of the area) in items/stamp_name.txt and a rule
        /// for the most frequent group of the area (Chance 1 = rare).
        /// </summary>
        private static void CreateItemRule(CgiConversion conv, IndexedMap cut, DragonRuleSet rules, string name, List<string> log)
        {
            string safe = string.Concat(name.Select(ch => char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : '_'));
            string path = $"items/stamp_{safe}.txt";
            int cx = conv.Area.Width / 2, cy = conv.Area.Height / 2;
            var def = new ItemGroupDef { Frequency = 1, Comment = $"{name} (aus der CGI-Ansicht)" };
            foreach (LayerStatic s in conv.Statics)
            {
                def.Parts.Add(new ItemPart { Id = s.Id, Z = s.Dz, Hue = s.Hue, Dx = s.X - cx, Dy = s.Y - cy });
            }
            ItemFile file = rules.ItemFiles.FirstOrDefault(f => string.Equals(f.Path, path, StringComparison.OrdinalIgnoreCase));
            if (file == null)
            {
                file = new ItemFile { Path = path, HeaderComment = "// Dragon Map Studio: Stempel " + name };
                rules.ItemFiles.Add(file);
            }
            file.Groups.Add(def);
            int group = cut.Pixels.Where(p => p < rules.Palette.Count && rules.Palette[p].Group >= 0)
                              .GroupBy(p => rules.Palette[p].Group).OrderByDescending(g => g.Count()).Select(g => g.Key).DefaultIfEmpty(-1).First();
            if (group >= 0 && !rules.ItemRules.Any(r => r.Group == group && string.Equals(r.File, path, StringComparison.OrdinalIgnoreCase)))
            {
                rules.ItemRules.Add(new ItemRule { Group = group, Chance = 1, File = path });
            }
            log?.Add($"Item-Regel: {conv.Statics.Count} Statics als Gruppe in {path}" + (group >= 0 ? $", Regel für Gruppe {rules.HexGroup(group)} (Chance 1)." : "."));
        }

        /// <summary>Report of a conversion (for the log file).</summary>
        public static string Report(CgiConversion conv, DragonRuleSet rules, string source)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Dragon Map Studio – Umwandlung CGI-Karte → Farbkarte");
            sb.AppendLine($"Karte: {source}");
            conv.Log.ForEach(l => sb.AppendLine(l));
            sb.AppendLine($"Ähnlichste Farbe genommen: {conv.BySimilar:N0} Tile/Höhe-Kombinationen");
            if (conv.Suggestions.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Tiles ohne passende Farbe (Tile, Höhe, Anzahl, TileData-Name, Radar-Farbe, Gruppe, angelegt):");
                foreach (ColorSuggestion s in conv.Suggestions)
                {
                    sb.AppendLine($"  {s.Tile:x4}  Z{s.Altitude,4}  ×{s.Count,7}  {s.TileName,-24} #{s.Radar.R:x2}{s.Radar.G:x2}{s.Radar.B:x2}  " +
                                  $"{(s.NewGroup ? "neu: " : rules.HexGroup(s.Group) + " ")}{s.GroupName,-20} {(s.CreatedIndex >= 0 ? "→ " + rules.HexIndex(s.CreatedIndex) : s.Accept ? "-" : "abgelehnt")}");
                }
            }
            return sb.ToString();
        }

        // ================================================================== template (multi) -> colour stamp

        /// <summary>
        /// A template of the template library (UoFiddler multi, UO Architect, own template) as colour stamp:
        /// statics with their height above the ground, land tiles (if the template has some) as colours, otherwise
        /// <paramref name="floorColour"/> under the statics or transparent (the map keeps its colour).
        /// </summary>
        public static ColorStamp TemplateToStamp(MapTemplate t, DragonRuleSet rules, LandTransitionEngine transitions, int waterGroup, int floorColour,
            bool floorEverywhere, MissingColorMode missing, List<string> log)
        {
            Rectangle b = t.Bounds;
            if (b.IsEmpty)
            {
                return null;
            }
            int w = b.Width, h = b.Height;
            var colours = new ushort[w * h];
            Array.Fill(colours, ColorStampTools.Transparent);
            var landZ = new int[w * h];
            if (t.HasLand)
            {
                // land of the template as a small built map -> colours like a CGI area
                int mw = (w + 7) / 8 * 8, mh = (h + 7) / 8 * 8;
                var tiles = new ushort[mw * mh];
                var z = new sbyte[mw * mh];
                for (int ly = 0; ly < t.LandHeight; ly++)
                {
                    for (int lx = 0; lx < t.LandWidth; lx++)
                    {
                        int x = t.LandX + lx - b.X, y = t.LandY + ly - b.Y;
                        int k = ly * t.LandWidth + lx;
                        tiles[y * mw + x] = t.LandTiles[k];
                        z[y * mw + x] = (sbyte)Math.Clamp(t.LandZ != null && k < t.LandZ.Length ? t.LandZ[k] : 0, -128, 127);
                        landZ[y * w + x] = z[y * mw + x];
                    }
                }
                var map = new MapData(mw, mh, tiles, z);
                var area = new Rectangle(t.LandX - b.X, t.LandY - b.Y, t.LandWidth, t.LandHeight);
                CgiConversion conv = Analyze(map, area, rules, transitions, false);
                if (missing == MissingColorMode.AutoCreate && conv.Suggestions.Count > 0)
                {
                    log?.AddRange(CreateColours(rules, conv.Suggestions));
                }
                Finish(conv, map, rules, transitions);
                for (int y = 0; y < area.Height; y++)
                {
                    for (int x = 0; x < area.Width; x++)
                    {
                        colours[(area.Y + y) * w + area.X + x] = conv.Colours[y * area.Width + x];
                    }
                }
            }
            if (floorColour >= 0 && floorColour < rules.Palette.Count)
            {
                var occupied = new HashSet<int>(t.Statics.Select(s => (s.Y - b.Y) * w + s.X - b.X));
                for (int i = 0; i < colours.Length; i++)
                {
                    if (colours[i] == ColorStampTools.Transparent && (floorEverywhere || occupied.Contains(i)))
                    {
                        colours[i] = (ushort)floorColour;
                    }
                }
            }
            var cut = new IndexedMap(w, h, colours, rules.ColorBits);
            ColorStamp stamp = ColorStampTools.FromMap(cut, rules, waterGroup, t.Name, t.Category);
            if (!t.HasLand && floorColour >= 0 && floorColour < rules.Palette.Count)
            {
                // the stamp stands on the floor colour: its altitude is the ground
                stamp.BaseAltitude = rules.Palette[floorColour].Altitude;
                for (int i = 0; i < colours.Length; i++)
                {
                    stamp.RelativeAltitudes[i] = colours[i] < rules.Palette.Count ? rules.Palette[colours[i]].Altitude - stamp.BaseAltitude : 0;
                }
            }
            foreach (TemplateStatic s in t.Statics)
            {
                int x = s.X - b.X, y = s.Y - b.Y;
                // template Z is relative to the ground at the anchor; above the land of the cell when the template has land
                stamp.Statics.Add(new LayerStatic { X = x, Y = y, Dz = s.Z - (t.HasLand ? landZ[y * w + x] : 0), Id = s.Id, Hue = s.Hue });
            }
            return stamp;
        }
    }
}

// /***************************************************************************
//  *
//  * Dragon Map Studio – checks and repairs the painted map before the build
//  * (rules from the Dragon map making tutorial by Ryandor, 2002)
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>Options of the repair (PropertyGrid; also part of the build options).</summary>
    public sealed class MapFixOptions
    {
        [Category("Reparatur"), DisplayName("Einzel-Tiles / 1er-Streifen entfernen"),
         Description("Dragon braucht mindestens 2 Tiles nebeneinander für einen Übergang. Einzelne Pixel und 1 Tile breite Streifen werden zur häufigsten Nachbar-Gruppe.")]
        public bool RemoveThin { get; set; } = true;

        [Category("Reparatur"), DisplayName("Brücken-Gruppen einsetzen"),
         Description("Wo zwei Gruppen ohne Übergangsregel aneinander stoßen, wird eine Gruppe dazwischen gesetzt, die zu beiden Übergänge hat.")]
        public bool BridgeMissing { get; set; } = true;

        [Category("Reparatur"), DisplayName("Umgebungs-Regeln"),
         Description("Gruppe:Umgebung:Breite, mehrere mit ';'. Standard '18:00:2' = Sumpf immer mit 2 Tiles Gras umgeben (Dragon-Tutorial).")]
        public string SurroundRules { get; set; } = "18:00:2";

        [Category("Reparatur"), DisplayName("Berg-Profil"),
         Description("Berge steigen vom umgebenden Boden aus nach innen an (Dragon-Tutorial: Rock 20, 30, 50, 60, 65, 70 über Gras 0). Ein Berg liegt nie tiefer als das Gras daneben.")]
        public bool MountainProfile { get; set; } = true;

        [Category("Reparatur"), DisplayName("Berg-Profil Höhen"), Description("Höhe ÜBER dem Boden je Ring vom Rand aus (Leerzeichen getrennt). Jeder Ring liegt zwischen der Hälfte und diesem Wert.")]
        public string MountainHeights { get; set; } = "20 30 50 60 65 70 80 90";

        [Category("Gruppen"), DisplayName("Nicht verändern"), Description("Gruppen (hex), die die Reparatur nie ändert, z. B. 1d = unsichtbarer Höhleneingang.")]
        public string SkipGroups { get; set; } = "1d";

        [Category("Reparatur"), DisplayName("Steile Sprünge glätten"), Description("Höhenunterschied zwischen zwei Nachbar-Tiles begrenzen (gegen Stacheln und Klippen).")]
        public bool LimitSteps { get; set; } = true;

        [Category("Reparatur"), DisplayName("Max. Sprung Land"), Description("Größter Höhenunterschied zwischen Nachbarn (außer Berg/Berg).")]
        public int MaxStep { get; set; } = 10;

        [Category("Reparatur"), DisplayName("Max. Sprung Berg"), Description("Größter Höhenunterschied zwischen zwei Berg-Tiles.")]
        public int MaxMountainStep { get; set; } = 20;

        [Category("Gruppen"), DisplayName("Wasser-Gruppe"), Description("hex 02 = IMod13 Water (dezimal eingeben: 2).")]
        public int WaterGroup { get; set; } = 0x02;

        [Category("Gruppen"), DisplayName("Berg-Gruppe"), Description("IMod13: 6 = mountain/rock.")]
        public int MountainGroup { get; set; } = 0x06;

        [Category("Gruppen"), DisplayName("Ersatz-Gruppe"), Description("Bevorzugte Brücken-Gruppe (IMod13: 0 = Gras).")]
        public int FallbackGroup { get; set; } = 0x00;

        [Category("Gruppen"), DisplayName("GrassBump-Prüfung"), Description("Gruppe:Nachbar – Gruppe muss tiefer liegen als der Nachbar (Tutorial 'Grass cliffs': 19:00).")]
        public string LowerThanRules { get; set; } = "19:00";

        public MapFixOptions Clone() => (MapFixOptions)MemberwiseClone();

        public override string ToString() => "Reparatur-Einstellungen";
    }

    /// <summary>One kind of problem / repair with count and example coordinates.</summary>
    public sealed class MapFixEntry
    {
        public string Key;
        public string TitleDe;
        public string TitleEn;
        public string HintDe;
        public string HintEn;
        public long Count;
        public bool Fixed;
        public readonly List<Point> Samples = new List<Point>();

        public void Add(int x, int y)
        {
            Count++;
            if (Samples.Count < 15)
            {
                Samples.Add(new Point(x, y));
            }
        }
    }

    public sealed class MapFixReport
    {
        public readonly List<MapFixEntry> Entries = new List<MapFixEntry>();
        public bool CheckOnly;
        public int Width;
        public int Height;

        public MapFixEntry Get(string key, string de, string en, string hintDe, string hintEn, bool fixes)
        {
            MapFixEntry e = Entries.FirstOrDefault(x => x.Key == key);
            if (e == null)
            {
                e = new MapFixEntry { Key = key, TitleDe = de, TitleEn = en, HintDe = hintDe, HintEn = hintEn, Fixed = fixes };
                Entries.Add(e);
            }
            return e;
        }

        public long Total => Entries.Sum(e => e.Count);

        public string Summary()
        {
            if (Entries.Count == 0 || Total == 0)
            {
                return CheckOnly ? "Prüfung: keine Probleme gefunden." : "Reparatur: nichts zu tun.";
            }
            string fixedPart = string.Join(", ", Entries.Where(e => e.Count > 0 && e.Fixed).Select(e => $"{e.TitleDe} {e.Count:N0}"));
            string notes = string.Join(", ", Entries.Where(e => e.Count > 0 && !e.Fixed).Select(e => $"{e.TitleDe} {e.Count:N0}"));
            string head = CheckOnly ? "Prüfung – behebbar: " : "Reparatur – geändert: ";
            return head + (fixedPart.Length > 0 ? fixedPart : "-") + (notes.Length > 0 ? " | Hinweise (von Hand lösen): " + notes : string.Empty);
        }
    }

    /// <summary>
    /// Repairs typical Dragon problems on the 8-bit map:
    /// single tiles / 1 tile strips (no transition possible), group pairs without transition, swamp without grass ring,
    /// mountains that rise too steeply, big height jumps. <see cref="Check"/> only reports (on a copy).
    /// </summary>
    public static class MapFixer
    {
        private static readonly int[] Dx4 = { -1, 1, 0, 0 };
        private static readonly int[] Dy4 = { 0, 0, -1, 1 };
        private static readonly int[] Dx8 = { -1, 1, 0, 0, -1, 1, -1, 1 };
        private static readonly int[] Dy8 = { 0, 0, -1, 1, -1, -1, 1, 1 };

        /// <summary>Reports what <see cref="Fix"/> would change plus things that can only be checked (map unchanged).</summary>
        public static MapFixReport Check(IndexedMap map, DragonRuleSet rules, MapFixOptions o, CancellationToken cancel)
        {
            MapFixReport r = Fix(map.Clone(), rules, o, null, cancel);
            r.CheckOnly = true;
            return r;
        }

        /// <summary>Repairs the map in place (undo through <paramref name="history"/> when given).</summary>
        public static MapFixReport Fix(IndexedMap map, DragonRuleSet rules, MapFixOptions o, MapHistory history, CancellationToken cancel)
        {
            var report = new MapFixReport { Width = map.Width, Height = map.Height };
            history?.Begin("Karte reparieren");
            history?.TouchAll();
            var ctx = new Context(map, rules);
            foreach (string t in (o.SkipGroups ?? string.Empty).Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (HexList.TryParseHexAddress(t, out int sg))
                {
                    ctx.Skip[sg] = true;
                }
            }

            CheckUndefined(ctx, report);
            if (o.RemoveThin)
            {
                RemoveThin(ctx, report, cancel);
            }
            foreach ((int g, int s, int width) in ParseSurround(o.SurroundRules))
            {
                Surround(ctx, report, g, s, width, o.WaterGroup, cancel);
            }
            if (o.BridgeMissing)
            {
                BridgeMissing(ctx, report, o.FallbackGroup, o.WaterGroup, cancel);
                if (o.RemoveThin)
                {
                    RemoveThin(ctx, report, cancel); // again, only where it does not break a bridge
                }
            }
            if (o.MountainProfile)
            {
                MountainProfile(ctx, report, o.MountainGroup, ParseInts(o.MountainHeights), o.WaterGroup, cancel);
            }
            if (o.LimitSteps)
            {
                LimitSteps(ctx, report, o, cancel);
                if (o.MountainProfile)
                {
                    // smoothing must not push a mountain under its ground again
                    MountainProfile(ctx, report, o.MountainGroup, ParseInts(o.MountainHeights), o.WaterGroup, cancel);
                }
            }
            foreach ((int g, int n) in ParsePairs(o.LowerThanRules))
            {
                CheckLowerThan(ctx, report, g, n);
            }
            CheckRemainingPairs(ctx, report, o.WaterGroup);

            history?.End();
            return report;
        }

        /// <summary>Only the bridge groups (used by the generator).</summary>
        public static void BridgeOnly(IndexedMap map, DragonRuleSet rules, int fallbackGroup, int waterGroup, IEnumerable<int> preferred, CancellationToken cancel)
        {
            var ctx = new Context(map, rules);
            BridgeMissing(ctx, new MapFixReport(), fallbackGroup, waterGroup, cancel, preferred);
        }

        // ------------------------------------------------------------------ helpers

        private sealed class Context
        {
            public readonly IndexedMap Map;
            public readonly DragonRuleSet Rules;
            public readonly int[] GroupOf = new int[65536];
            public readonly int[] AltOf = new int[65536];
            public readonly bool[] Defined = new bool[65536];
            public readonly bool[] Skip = new bool[65537];
            public readonly HashSet<int> Pairs;
            private readonly Dictionary<int, List<DragonPaletteEntry>> _entries = new Dictionary<int, List<DragonPaletteEntry>>();

            public Context(IndexedMap map, DragonRuleSet rules)
            {
                Map = map;
                Rules = rules;
                Array.Fill(GroupOf, -1);
                foreach (DragonPaletteEntry e in rules.Palette)
                {
                    GroupOf[e.Index] = e.Group;
                    AltOf[e.Index] = e.Altitude;
                    Defined[e.Index] = e.Defined;
                }
                Pairs = rules.MapTransitionPairs();
            }

            public int W => Map.Width;
            public int H => Map.Height;
            public int Group(int i) => GroupOf[Map.Pixels[i]];

            public bool IsSkip(int group) => group >= 0 && group < Skip.Length && Skip[group];
            public int Alt(int i) => AltOf[Map.Pixels[i]];

            public List<DragonPaletteEntry> Entries(int group)
            {
                if (!_entries.TryGetValue(group, out List<DragonPaletteEntry> list))
                {
                    _entries[group] = list = Rules.EntriesOfGroup(group);
                }
                return list;
            }

            public bool HasGroup(int group) => Entries(group).Count > 0;

            private readonly Dictionary<long, int> _nearest = new Dictionary<long, int>();
            private readonly Dictionary<long, int> _below = new Dictionary<long, int>();

            /// <summary>Colour of the group nearest to the altitude.</summary>
            public ushort Nearest(int group, int alt)
            {
                long key = ((long)group << 32) | (uint)alt;
                if (!_nearest.TryGetValue(key, out int idx))
                {
                    DragonPaletteEntry best = Entries(group).OrderBy(e => Math.Abs(e.Altitude - alt)).FirstOrDefault();
                    _nearest[key] = idx = best?.Index ?? 0;
                }
                return (ushort)idx;
            }

            /// <summary>Highest colour of the group with altitude &lt;= max (or the lowest one).</summary>
            public int Below(int group, int max)
            {
                long key = ((long)group << 32) | (uint)max;
                if (!_below.TryGetValue(key, out int idx))
                {
                    List<DragonPaletteEntry> list = Entries(group);
                    DragonPaletteEntry best = list.Where(e => e.Altitude <= max).OrderByDescending(e => e.Altitude).FirstOrDefault() ?? list.FirstOrDefault();
                    _below[key] = idx = best?.Index ?? -1;
                }
                return idx;
            }

            /// <summary>Lowest colour of the group with altitude &gt;= min (or the highest one).</summary>
            public int Above(int group, int min)
            {
                List<DragonPaletteEntry> list = Entries(group);
                DragonPaletteEntry best = list.Where(e => e.Altitude >= min).OrderBy(e => e.Altitude).FirstOrDefault() ?? list.LastOrDefault();
                return best?.Index ?? -1;
            }

            public bool HasRule(int a, int b) => a == b || Pairs.Contains(DragonRuleSet.PairKey(a, b));
        }

        private static void CheckUndefined(Context c, MapFixReport r)
        {
            MapFixEntry e = r.Get("undefined", "Farben ohne maptrans-Eintrag", "Colours without maptrans entry",
                "Diese Farbe ist in maptrans.txt nicht definiert – mit einer definierten Farbe übermalen oder die Farbe definieren.",
                "This colour is not defined in maptrans.txt – paint over it with a defined colour or define the colour.", false);
            for (int i = 0; i < c.Map.Pixels.Length; i++)
            {
                if (!c.Defined[c.Map.Pixels[i]])
                {
                    e.Add(i % c.W, i / c.W);
                }
            }
        }

        private static void RemoveThin(Context c, MapFixReport r, CancellationToken cancel)
        {
            MapFixEntry e = r.Get("thin", "Einzel-Tiles / 1-Tile-Streifen", "Single tiles / 1 tile strips",
                "Dragon braucht mindestens 2 Tiles nebeneinander (Tutorial: 'contract selection by at least 2'). Fläche verbreitern oder entfernen.",
                "Dragon needs at least 2 tiles side by side (tutorial: 'contract selection by at least 2'). Make the area wider or remove it.", true);
            int w = c.W, h = c.H;
            ushort[] px = c.Map.Pixels;
            for (int pass = 0; pass < 3; pass++)
            {
                var copy = (ushort[])px.Clone();
                int changed = 0;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int i = y * w + x;
                        int g = c.GroupOf[copy[i]];
                        if (c.IsSkip(g))
                        {
                            continue;
                        }
                        bool Same(int nx, int ny) => nx < 0 || ny < 0 || nx >= w || ny >= h || c.GroupOf[copy[ny * w + nx]] == g;
                        bool thinX = !Same(x - 1, y) && !Same(x + 1, y);
                        bool thinY = !Same(x, y - 1) && !Same(x, y + 1);
                        if (!thinX && !thinY)
                        {
                            continue;
                        }
                        // most frequent other group of the 8 neighbours that has transitions to all neighbours
                        var count = new Dictionary<int, int>();
                        var colourOf = new Dictionary<int, ushort>();
                        var neighbourGroups = new List<int>(8);
                        for (int k = 0; k < 8; k++)
                        {
                            int nx = x + Dx8[k], ny = y + Dy8[k];
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                            {
                                continue;
                            }
                            ushort nc = copy[ny * w + nx];
                            int ng = c.GroupOf[nc];
                            if (ng < 0)
                            {
                                continue;
                            }
                            neighbourGroups.Add(ng);
                            if (ng == g)
                            {
                                continue;
                            }
                            count.TryGetValue(ng, out int n);
                            count[ng] = n + 1;
                            colourOf.TryAdd(ng, nc);
                        }
                        int bestIdx = -1;
                        foreach (KeyValuePair<int, int> kv in count.OrderByDescending(k => k.Value))
                        {
                            if (neighbourGroups.All(ng => c.HasRule(kv.Key, ng)))
                            {
                                bestIdx = colourOf[kv.Key];
                                break;
                            }
                        }
                        if (bestIdx >= 0)
                        {
                            px[i] = (ushort)bestIdx;
                            e.Add(x, y);
                            changed++;
                        }
                    }
                    if ((y & 63) == 0)
                    {
                        cancel.ThrowIfCancellationRequested();
                    }
                }
                if (changed == 0)
                {
                    break;
                }
            }
        }

        private static IEnumerable<(int g, int s, int width)> ParseSurround(string text)
        {
            foreach (string part in (text ?? string.Empty).Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] t = part.Trim().Split(':');
                if (t.Length >= 2 && HexList.TryParseHexAddress(t[0].Trim(), out int g) && HexList.TryParseHexAddress(t[1].Trim(), out int s))
                {
                    int width = t.Length > 2 && int.TryParse(t[2].Trim(), out int wv) ? Math.Clamp(wv, 1, 8) : 2;
                    yield return (g, s, width);
                }
            }
        }

        private static IEnumerable<(int g, int n)> ParsePairs(string text)
        {
            foreach (string part in (text ?? string.Empty).Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] t = part.Trim().Split(':');
                if (t.Length >= 2 && HexList.TryParseHexAddress(t[0].Trim(), out int g) && HexList.TryParseHexAddress(t[1].Trim(), out int n))
                {
                    yield return (g, n);
                }
            }
        }

        private static List<int> ParseInts(string text)
        {
            var list = new List<int>();
            foreach (string t in (text ?? string.Empty).Split(new[] { ' ', ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                {
                    list.Add(v);
                }
            }
            return list;
        }

        /// <summary>Every cell within <paramref name="width"/> tiles of group g that is not g / s becomes s (e.g. grass around swamp).</summary>
        private static void Surround(Context c, MapFixReport r, int g, int s, int width, int water, CancellationToken cancel)
        {
            if (!c.HasGroup(s))
            {
                return;
            }
            MapFixEntry e = r.Get($"surround_{c.Rules.HexGroup(g)}_{c.Rules.HexGroup(s)}", $"{c.Rules.GroupName(g)} ohne {width} Tiles {c.Rules.GroupName(s)} drumherum",
                $"{c.Rules.GroupName(g)} without {width} tiles of {c.Rules.GroupName(s)} around it",
                $"Dragon hat für {c.Rules.GroupName(g)} nur Übergänge zu {c.Rules.GroupName(s)} – mindestens {width} Tiles {c.Rules.GroupName(s)} drumherum malen.",
                $"Dragon only has transitions from {c.Rules.GroupName(g)} to {c.Rules.GroupName(s)} – paint at least {width} tiles of {c.Rules.GroupName(s)} around it.", true);
            int w = c.W, h = c.H;
            var dist = new int[w * h];
            Array.Fill(dist, int.MaxValue);
            var queue = new Queue<int>();
            for (int i = 0; i < dist.Length; i++)
            {
                if (c.Group(i) == g)
                {
                    dist[i] = 0;
                    queue.Enqueue(i);
                }
            }
            if (queue.Count == 0)
            {
                return;
            }
            while (queue.Count > 0)
            {
                int i = queue.Dequeue();
                if (dist[i] >= width)
                {
                    continue;
                }
                int x = i % w, y = i / w;
                for (int k = 0; k < 8; k++)
                {
                    int nx = x + Dx8[k], ny = y + Dy8[k];
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                    {
                        continue;
                    }
                    int j = ny * w + nx;
                    if (dist[j] != int.MaxValue)
                    {
                        continue;
                    }
                    dist[j] = dist[i] + 1;
                    int ng = c.Group(j);
                    if (ng != g && ng != s && ng >= 0 && !c.IsSkip(ng))
                    {
                        c.Map.Pixels[j] = c.Nearest(s, ng == water ? 0 : c.Alt(j));
                        e.Add(nx, ny);
                    }
                    queue.Enqueue(j);
                }
            }
            cancel.ThrowIfCancellationRequested();
        }

        private static void BridgeMissing(Context c, MapFixReport r, int fallback, int water, CancellationToken cancel, IEnumerable<int> preferred = null)
        {
            MapFixEntry e = r.Get("bridge", "Gruppen ohne Übergang (Brücke eingesetzt)", "Groups without transition (bridge inserted)",
                "Für diese zwei Gruppen gibt es keine Übergangsdatei. Eine Gruppe mit Übergängen zu beiden wurde dazwischen gesetzt.",
                "There is no transition file for these two groups. A group with transitions to both was put in between.", true);
            var candidates = new List<int> { fallback };
            if (preferred != null)
            {
                candidates.AddRange(preferred);
            }
            candidates.AddRange(c.Rules.AllGroupIds().Where(g => g != water && c.HasGroup(g)));
            candidates = candidates.Distinct().ToList();
            var bridges = new Dictionary<int, int>();
            int Bridge(int g, int n)
            {
                int key = DragonRuleSet.PairKey(g, n);
                if (!bridges.TryGetValue(key, out int b))
                {
                    b = candidates.FirstOrDefault(x => x != g && x != n && c.HasGroup(x) && c.HasRule(x, g) && c.HasRule(x, n), -1);
                    bridges[key] = b;
                }
                return b;
            }
            int w = c.W, h = c.H;
            for (int pass = 0; pass < 2; pass++)
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int i = y * w + x;
                        int g = c.Group(i);
                        if (g < 0 || g == water || c.IsSkip(g))
                        {
                            continue;
                        }
                        for (int k = 0; k < 8; k++)
                        {
                            int nx = x + Dx8[k], ny = y + Dy8[k];
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                            {
                                continue;
                            }
                            int n = c.Group(ny * w + nx);
                            if (n < 0 || n == water || c.IsSkip(n) || c.HasRule(g, n))
                            {
                                continue;
                            }
                            int b = Bridge(g, n);
                            if (b >= 0)
                            {
                                c.Map.Pixels[i] = c.Nearest(b, c.Alt(i));
                                e.Add(x, y);
                            }
                            break;
                        }
                    }
                    if ((y & 63) == 0)
                    {
                        cancel.ThrowIfCancellationRequested();
                    }
                }
            }
        }

        /// <summary>
        /// Mountains rise from the surrounding ground: ring n (distance n to the border) lies between
        /// ground + h[n]/2 and ground + h[n]. The ground is the height of the nearest non-mountain land (water = 0).
        /// </summary>
        private static void MountainProfile(Context c, MapFixReport r, int mountain, List<int> heights, int water, CancellationToken cancel)
        {
            if (heights.Count == 0 || !c.HasGroup(mountain))
            {
                return;
            }
            MapFixEntry e = r.Get("mountain", "Berg-Höhe an den Boden angepasst", "Mountain height adapted to the ground",
                "Tutorial: Berge in Ringen vom Boden aus nach oben malen – Rand Rock 20, dann 30, 50, 60, 65, 70 (über Gras 0). Ein Berg darf nie tiefer liegen als das Gras daneben.",
                "Tutorial: paint mountains in rings rising from the ground – border Rock 20, then 30, 50, 60, 65, 70 (above grass 0). A mountain must never be lower than the grass next to it.", true);
            int w = c.W, h = c.H;
            var dist = new int[w * h];
            var ground = new int[w * h];
            Array.Fill(dist, int.MaxValue);
            var queue = new Queue<int>();
            for (int i = 0; i < dist.Length; i++)
            {
                int g = c.Group(i);
                if (g != mountain)
                {
                    dist[i] = 0;
                    ground[i] = g == water ? 0 : c.Alt(i);
                    queue.Enqueue(i);
                }
            }
            while (queue.Count > 0)
            {
                int i = queue.Dequeue();
                int x = i % w, y = i / w;
                for (int k = 0; k < 4; k++)
                {
                    int nx = x + Dx4[k], ny = y + Dy4[k];
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                    {
                        continue;
                    }
                    int j = ny * w + nx;
                    if (dist[j] == int.MaxValue)
                    {
                        dist[j] = dist[i] + 1;
                        ground[j] = ground[i];
                        queue.Enqueue(j);
                    }
                    else if (dist[j] == dist[i] + 1 && dist[i] == 0)
                    {
                        ground[j] = Math.Max(ground[j], ground[i]); // highest ground along the border
                    }
                }
            }
            int last = heights[heights.Count - 1];
            for (int i = 0; i < dist.Length; i++)
            {
                if (dist[i] == 0 || dist[i] == int.MaxValue)
                {
                    continue;
                }
                int ring = Math.Min(dist[i], heights.Count) - 1;
                int max = Math.Clamp(ground[i] + heights[ring], -128, 127);
                int min = Math.Clamp(ground[i] + Math.Max(5, heights[ring] / 2), -128, 127);
                int z = c.Alt(i);
                int target = z > max ? max : z < min ? min : z;
                if (target == z)
                {
                    continue;
                }
                int idx = z > max ? c.Below(mountain, target) : c.Above(mountain, target);
                if (idx >= 0 && idx != c.Map.Pixels[i])
                {
                    c.Map.Pixels[i] = (ushort)idx;
                    e.Add(i % w, i / w);
                }
            }
            cancel.ThrowIfCancellationRequested();
        }

        private static void LimitSteps(Context c, MapFixReport r, MapFixOptions o, CancellationToken cancel)
        {
            MapFixEntry e = r.Get("steps", "Zu große Höhensprünge (geglättet)", "Too big height jumps (smoothed)",
                "Höhen in Stufen malen (Tutorial: 5, 10, 15, 20 …), keine großen Sprünge zwischen Nachbarn.",
                "Paint heights in steps (tutorial: 5, 10, 15, 20 …), no big jumps between neighbours.", true);
            int w = c.W, h = c.H;
            ushort[] px = c.Map.Pixels;
            for (int pass = 0; pass < 6; pass++)
            {
                int changed = 0;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int i = y * w + x;
                        int g = c.Group(i);
                        if (g < 0 || g == o.WaterGroup || c.IsSkip(g))
                        {
                            continue;
                        }
                        bool isMountain = g == o.MountainGroup;
                        int z = c.Alt(i);
                        int limit = int.MaxValue;
                        for (int k = 0; k < 4; k++)
                        {
                            int nx = x + Dx4[k], ny = y + Dy4[k];
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                            {
                                continue;
                            }
                            int j = ny * w + nx;
                            int ng = c.Group(j);
                            if (ng < 0 || ng == o.WaterGroup || (ng == o.MountainGroup) != isMountain)
                            {
                                continue; // land next to a mountain may be lower; mountains only compare with mountains
                            }
                            int step = isMountain ? o.MaxMountainStep : o.MaxStep;
                            limit = Math.Min(limit, c.Alt(j) + step);
                        }
                        if (limit == int.MaxValue || z <= limit)
                        {
                            continue;
                        }
                        int idx = c.Below(g, limit);
                        if (idx >= 0 && c.AltOf[idx] < z)
                        {
                            px[i] = (ushort)idx;
                            changed++;
                            if (pass == 0)
                            {
                                e.Add(x, y);
                            }
                            else
                            {
                                e.Count++;
                            }
                        }
                    }
                    if ((y & 63) == 0)
                    {
                        cancel.ThrowIfCancellationRequested();
                    }
                }
                if (changed == 0)
                {
                    break;
                }
            }
        }

        private static void CheckLowerThan(Context c, MapFixReport r, int g, int n)
        {
            if (!c.HasGroup(g) || !c.HasGroup(n))
            {
                return;
            }
            MapFixEntry e = r.Get($"lower_{c.Rules.HexGroup(g)}_{c.Rules.HexGroup(n)}", $"{c.Rules.GroupName(g)} nicht tiefer als {c.Rules.GroupName(n)}",
                $"{c.Rules.GroupName(g)} not lower than {c.Rules.GroupName(n)}",
                $"Tutorial 'Grass cliffs': {c.Rules.GroupName(g)} muss immer eine kleinere Höhe haben als angrenzendes {c.Rules.GroupName(n)}.",
                $"Tutorial 'grass cliffs': {c.Rules.GroupName(g)} must always have a lower z than the adjacent {c.Rules.GroupName(n)}.", false);
            int w = c.W, h = c.H;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    if (c.Group(i) != g)
                    {
                        continue;
                    }
                    for (int k = 0; k < 4; k++)
                    {
                        int nx = x + Dx4[k], ny = y + Dy4[k];
                        if (nx >= 0 && ny >= 0 && nx < w && ny < h && c.Group(ny * w + nx) == n && c.Alt(ny * w + nx) <= c.Alt(i))
                        {
                            e.Add(x, y);
                            break;
                        }
                    }
                }
            }
        }

        private static void CheckRemainingPairs(Context c, MapFixReport r, int water)
        {
            int w = c.W, h = c.H;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    int g = c.Group(i);
                    if (g < 0)
                    {
                        continue;
                    }
                    for (int k = 0; k < 2; k++)
                    {
                        int nx = x + (k == 0 ? 1 : 0), ny = y + (k == 1 ? 1 : 0);
                        if (nx >= w || ny >= h)
                        {
                            continue;
                        }
                        int n = c.Group(ny * w + nx);
                        if (n < 0 || n == g || c.HasRule(g, n))
                        {
                            continue;
                        }
                        int a = Math.Min(g, n), b = Math.Max(g, n);
                        r.Get($"nopair_{c.Rules.HexGroup(a)}_{c.Rules.HexGroup(b)}", $"Kein Übergang {c.Rules.GroupName(a)} / {c.Rules.GroupName(b)}",
                            $"No transition {c.Rules.GroupName(a)} / {c.Rules.GroupName(b)}",
                            "Keine Übergangsdatei für dieses Paar und keine Brücken-Gruppe gefunden – Übergangsregel anlegen oder anders malen.",
                            "No transition file for this pair and no bridge group – add a transition rule or paint differently.", false).Add(x, y);
                    }
                }
            }
        }
    }

    /// <summary>Writes the bilingual log files (German / English) that can be sent for analysis.</summary>
    public static class BuildLog
    {
        private static void Header(StringBuilder sb, string titleDe, string titleEn, DragonRuleSet rules)
        {
            sb.AppendLine("==========================================================================");
            sb.AppendLine($" Dragon Map Studio – {titleDe} / {titleEn}");
            sb.AppendLine($" {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}");
            sb.AppendLine($" Regelsatz / rule set: {rules.Name}  ({rules.Palette.Count(p => p.Defined)} Farben/colours, {rules.Transitions.Count} Übergänge/transitions)");
            sb.AppendLine("==========================================================================");
            sb.AppendLine();
        }

        private static void Section(StringBuilder sb, string de, string en)
        {
            sb.AppendLine();
            sb.AppendLine($"--- {de} / {en} " + new string('-', Math.Max(3, 66 - de.Length - en.Length)));
        }

        private static void Entries(StringBuilder sb, MapFixReport report)
        {
            foreach (MapFixEntry e in report.Entries.Where(e => e.Count > 0).OrderByDescending(e => e.Count))
            {
                string state = e.Fixed ? (report.CheckOnly ? "behebbar / fixable" : "behoben / fixed") : "nur Hinweis / report only";
                sb.AppendLine($"[{e.Key}] {e.TitleDe} / {e.TitleEn}: {e.Count:N0}  ({state})");
                sb.AppendLine($"   DE: {e.HintDe}");
                sb.AppendLine($"   EN: {e.HintEn}");
                sb.AppendLine("   Beispiele / examples (x,y): " + string.Join(" ", e.Samples.Select(p => $"({p.X},{p.Y})")));
            }
        }

        public static string Create(DragonRuleSet rules, BuildOptions o, BuildResult r)
        {
            var sb = new StringBuilder();
            Header(sb, "Build-Log", "build log", rules);
            Section(sb, "Zusammenfassung", "Summary");
            sb.AppendLine($"Karte / map: {r.Width}x{r.Height}, Nummer / number {o.MapNumber}, Seed {o.Seed}, Dauer / duration {r.Duration.TotalSeconds:0.0} s");
            sb.AppendLine($"Map-Übergänge / map transitions: {r.MapTransitions:N0}");
            sb.AppendLine($"Rand-Tiles ohne Übergang / border tiles without transition: {r.UnmatchedEdges:N0}");
            sb.AppendLine($"Static-Übergänge / static transitions: {r.StaticTransitionStatics:N0}");
            sb.AppendLine($"Items: {r.Items:N0} ({r.ItemStatics:N0} Statics)");
            sb.AppendLine($"Doppelte Statics entfernt / duplicate statics removed: {r.DuplicatesRemoved:N0}");
            sb.AppendLine($"Unebenheit / unevenly: {r.UnevenCells:N0}, Fixes: {r.Fixes:N0}, übernommen / merged: {r.MergedStatics:N0}, unbekannte Farben / unknown colours: {r.UnknownColors:N0}");
            Section(sb, "Optionen", "Options");
            sb.AppendLine($"StaticMode={o.StaticMode}, MapTransitionZ={o.MapTransitionZ}, StaticZ={o.StaticZ}, SmoothCoast={o.SmoothCoast}, ItemChanceBase={o.ItemChanceBase}, RemoveDuplicateStatics={o.RemoveDuplicateStatics}, AutoFix={o.AutoFixBeforeBuild}");
            if (r.FixReport != null)
            {
                Section(sb, "Automatische Reparatur", "Automatic repair");
                Entries(sb, r.FixReport);
            }
            if (r.Warnings.Count > 0)
            {
                Section(sb, "Warnungen", "Warnings");
                r.Warnings.ForEach(w => sb.AppendLine(w));
            }
            Section(sb, "Fehlende Übergänge", "Missing transitions");
            sb.AppendLine("DE: 'Muster fehlt' = Regel vorhanden, Muster (NW N NO O SO S SW W; A = eigene Gruppe, B = Nachbar) in der Datei ergänzen.");
            sb.AppendLine("    'Regel fehlt'  = keine Übergangsdatei für diese Gruppen → Regel anlegen oder 'Brücken-Gruppen' reparieren lassen.");
            sb.AppendLine("EN: 'Muster fehlt' = rule exists, add the pattern (NW N NE E SE S SW W; A = own group, B = neighbour) to the file.");
            sb.AppendLine("    'Regel fehlt'  = no transition file for these groups → add a rule or let the repair insert bridge groups.");
            foreach (KeyValuePair<string, int> kv in r.MissingTransitions.OrderByDescending(k => k.Value).Take(300))
            {
                string samples = r.MissingSamples.TryGetValue(kv.Key, out var list) ? string.Join(" ", list.Select(p => $"({p.X},{p.Y})")) : string.Empty;
                sb.AppendLine($"{kv.Key} : {kv.Value}   {samples}");
            }
            AppendRuleWarnings(sb, rules);
            return sb.ToString();
        }

        public static string CreateCheck(DragonRuleSet rules, MapFixReport report, string mapName)
        {
            var sb = new StringBuilder();
            Header(sb, report.CheckOnly ? "Karten-Prüfung" : "Karten-Reparatur", report.CheckOnly ? "map check" : "map repair", rules);
            sb.AppendLine($"Karte / map: {mapName} ({report.Width}x{report.Height})");
            sb.AppendLine(report.Summary());
            Section(sb, "Ergebnisse", "Results");
            Entries(sb, report);
            AppendRuleWarnings(sb, rules);
            return sb.ToString();
        }

        public static string CreateProblems(DragonRuleSet rules, IEnumerable<MapProblem> problems, string source)
        {
            var sb = new StringBuilder();
            Header(sb, "CGI-Prüfung", "CGI check", rules);
            sb.AppendLine($"Quelle / source: {source}");
            foreach (IGrouping<MapProblemKind, MapProblem> g in problems.GroupBy(p => p.Kind))
            {
                Section(sb, g.Key.ToString(), $"{g.Count()}");
                foreach (MapProblem p in g.Take(500))
                {
                    sb.AppendLine(p.ToString());
                }
            }
            return sb.ToString();
        }

        private static void AppendRuleWarnings(StringBuilder sb, DragonRuleSet rules)
        {
            List<string> v = rules.Validate();
            if (rules.Warnings.Count + v.Count == 0)
            {
                return;
            }
            Section(sb, "Hinweise zu den Regeln", "Rule set notes");
            rules.Warnings.ForEach(w => sb.AppendLine(w));
            v.ForEach(w => sb.AppendLine(w));
        }
    }
}

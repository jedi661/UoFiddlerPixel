// /***************************************************************************
//  *
//  * Dragon Map Studio – BMP -> map#.mul / staidx#.mul / statics#.mul (+ UOP)
//  * Replaces Dragon.exe and Dragonsp.exe.
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
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms.Design;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public enum StaticTileMode
    {
        /// <summary>All statics of a pattern line are placed on the tile (multi part borders).</summary>
        PlaceAll,
        /// <summary>One of the statics is chosen randomly.</summary>
        RandomOne
    }

    /// <summary>Options of the MUL build (PropertyGrid).</summary>
    public sealed class BuildOptions
    {
        [Category("1 Ausgabe"), DisplayName("Ausgabe-Ordner"), Description("Hier werden map#.mul, staidx#.mul, statics#.mul (und map#LegacyMUL.uop) gespeichert.")]
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        public string OutputFolder { get; set; } = string.Empty;

        [Category("1 Ausgabe"), DisplayName("Karten-Nummer"), Description("0 = Felucca, 1 = Trammel, 2 = Ilshenar, 3 = Malas, 4 = Tokuno, 5 = Ter Mur …")]
        public int MapNumber { get; set; }

        [Category("1 Ausgabe"), DisplayName("map#.mul schreiben")]
        public bool WriteMul { get; set; } = true;

        [Category("1 Ausgabe"), DisplayName("map#LegacyMUL.uop schreiben"), Description("Für neuere Clients (ab 7.0.24). Benutzt den UOP-Packer von UoFiddler.")]
        public bool WriteUop { get; set; }

        [Category("1 Ausgabe"), DisplayName("Statics schreiben"), Description("staidx#.mul und statics#.mul.")]
        public bool WriteStatics { get; set; } = true;

        [Category("1 Ausgabe"), DisplayName("WSC-Datei (UOX3) schreiben"), Description("Statics zusätzlich als Text (wie Dragon 'UOX Static File').")]
        public bool WriteWsc { get; set; }

        [Category("1 Ausgabe"), DisplayName("Bericht schreiben"), Description("dragon_log.txt: Statistik, Reparaturen, fehlende Übergänge mit Koordinaten (zum Weiterschicken).")]
        public bool WriteReport { get; set; } = true;

        [Category("2 Schritte"), DisplayName("Zufalls-Seed"), Description("Gleiche Zahl = gleiche Tiles / Items.")]
        public int Seed { get; set; } = 1;

        [Category("2 Schritte"), DisplayName("Unebenheit (unevenly.txt)")]
        public bool ApplyUneven { get; set; } = true;

        [Category("2 Schritte"), DisplayName("Map-Übergänge")]
        public bool ApplyMapTransitions { get; set; } = true;

        [Category("2 Schritte"), DisplayName("Static-Übergänge")]
        public bool ApplyStaticTransitions { get; set; } = true;

        [Category("2 Schritte"), DisplayName("Items (items.txt)")]
        public bool ApplyItems { get; set; } = true;

        [Category("2 Schritte"), DisplayName("Fix-Tabelle")]
        public bool ApplyFixes { get; set; } = true;

        [Category("3 Details"), DisplayName("Items: Chance-Basis"), Description("Chance in items.txt geteilt durch diesen Wert (1000 = Promille).")]
        public int ItemChanceBase { get; set; } = 1000;

        [Category("3 Details"), DisplayName("Items nicht überlappen")]
        public bool ItemOverlapCheck { get; set; } = true;

        [Category("3 Details"), DisplayName("Items nur auf gleicher Gruppe"), Description("Mehrteilige Items nur, wenn alle Teile auf der Gruppe liegen.")]
        public bool ItemSameGroup { get; set; } = true;

        [Category("3 Details"), DisplayName("Keine Items auf Übergangs-Statics")]
        public bool ItemsAvoidTransitionStatics { get; set; } = true;

        [Category("3 Details"), DisplayName("Statics mehrerer IDs"), Description("RandomOne = eine ID zufällig (wie Dragon: 'itemid1 to x will be used randomly'), PlaceAll = alle IDs einer Zeile setzen.")]
        public StaticTileMode StaticMode { get; set; } = StaticTileMode.RandomOne;

        [Category("3 Details"), DisplayName("Übergang: Höhe Z1..Z2"), Description("Dragon-Tutorial: neue Höhe = alte Höhe + Zufallszahl zwischen Z1 und Z2 (z. B. '005 007' → +5..+7). Aus = Höhe bleibt.")]
        public bool MapTransitionZ { get; set; } = true;

        [Category("3 Details"), DisplayName("Static-Z von den Nachbarn"), Description("Dragon-Tutorial: Static-Höhe = höchste Höhe der umliegenden Tiles der Quellgruppe + Z1..Z2. Aus = Höhe der Zelle.")]
        public bool StaticZFromNeighbours { get; set; } = true;

        [Category("3 Details"), DisplayName("Doppelte Statics entfernen"), Description("Gleiche ID an gleicher Stelle und Höhe nur einmal schreiben.")]
        public bool RemoveDuplicateStatics { get; set; } = true;

        [Category("5 Reparatur"), DisplayName("Vor dem Build reparieren"), Description("Repariert eine Kopie der Karte vor dem Build (die Karte im Editor bleibt unverändert). Einstellungen unten.")]
        public bool AutoFixBeforeBuild { get; set; }

        [Category("5 Reparatur"), DisplayName("Reparatur-Einstellungen"), TypeConverter(typeof(ExpandableObjectConverter))]
        public MapFixOptions Fix { get; set; } = new MapFixOptions();

        [Category("3 Details"), DisplayName("Unbekannte Farbe: Tile"), Description("Tile für Farben ohne maptrans-Eintrag (hex, 0244 = schwarz/nodraw).")]
        public string UnknownTile { get; set; } = "0244";

        [Category("4 Bestehende Statics"), DisplayName("Statics übernehmen"), Description("Statics aus vorhandenen Dateien dazu mischen (wie Dragonsp).")]
        public bool MergeExistingStatics { get; set; }

        [Category("4 Bestehende Statics"), DisplayName("staidx#.mul (alt)")]
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string MergeStaidxFile { get; set; } = string.Empty;

        [Category("4 Bestehende Statics"), DisplayName("statics#.mul (alt)")]
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string MergeStaticsFile { get; set; } = string.Empty;

        public BuildOptions Clone()
        {
            var c = (BuildOptions)MemberwiseClone();
            c.Fix = (Fix ?? new MapFixOptions()).Clone();
            return c;
        }
    }

    public struct StaticEntry
    {
        public ushort Id;
        public int X;
        public int Y;
        public sbyte Z;
        public ushort Hue;
    }

    /// <summary>Result of a build (statistics for the log and report).</summary>
    public sealed class BuildResult
    {
        public int Width;
        public int Height;
        public long MapTransitions;
        public long StaticTransitionStatics;
        public long Items;
        public long ItemStatics;
        public long UnevenCells;
        public long Fixes;
        public long MergedStatics;
        public long UnknownColors;
        public long UnmatchedEdges;
        public long DuplicatesRemoved;

        /// <summary>Up to 10 example coordinates per entry of <see cref="MissingTransitions"/>.</summary>
        public readonly Dictionary<string, List<System.Drawing.Point>> MissingSamples = new Dictionary<string, List<System.Drawing.Point>>();

        /// <summary>Report of the automatic repair (when on).</summary>
        public MapFixReport FixReport;
        public readonly List<string> Files = new List<string>();
        public readonly List<string> Warnings = new List<string>();

        /// <summary>Edge situations without a transition: "A->B pattern" -> count.</summary>
        public readonly Dictionary<string, int> MissingTransitions = new Dictionary<string, int>();

        public TimeSpan Duration;
    }

    /// <summary>
    /// The Dragon algorithm: colour -> group / altitude / random tile, unevenly, map transitions by 8-neighbour patterns,
    /// static transitions, random items, fix table; writes the client files.
    /// </summary>
    public sealed class DragonCompiler
    {
        private sealed class CompiledRule
        {
            public TransitionRule Rule;
            public Dictionary<byte, TransitionPattern> Patterns;
        }

        private static readonly int[] Dx = { -1, 0, 1, 1, 1, 0, -1, -1 };
        private static readonly int[] Dy = { -1, -1, -1, 0, 1, 1, 1, 0 };

        private readonly DragonRuleSet _rules;
        private readonly BuildOptions _o;
        private readonly Action<string> _log;

        public DragonCompiler(DragonRuleSet rules, BuildOptions options, Action<string> log)
        {
            _rules = rules;
            _o = options;
            _log = log ?? (_ => { });
        }

        // working data
        private int _w, _h;
        private byte[] _group;
        private ushort[] _tile;
        private sbyte[] _z;
        private bool[] _occupied;
        private List<StaticEntry> _statics;
        private Random _rnd;

        public BuildResult Build(IndexedMap source, IProgress<int> progress, CancellationToken cancel)
        {
            DateTime start = DateTime.Now;
            var result = new BuildResult();
            if (_o.AutoFixBeforeBuild)
            {
                source = source.Clone();
                result.FixReport = MapFixer.Fix(source, _rules, _o.Fix ?? new MapFixOptions(), null, cancel);
                _log(result.FixReport.Summary());
            }
            IndexedMap map = PadToBlocks(source, result);
            _w = map.Width;
            _h = map.Height;
            result.Width = _w;
            result.Height = _h;
            int n = _w * _h;
            _rnd = new Random(_o.Seed);
            _group = new byte[n];
            _tile = new ushort[n];
            _z = new sbyte[n];
            _occupied = new bool[n];
            _statics = new List<StaticEntry>(n / 16);

            _log($"Karte {_w}×{_h} ({_w / 8}×{_h / 8} Blöcke), Seed {_o.Seed}");

            // 1) colours -> group, altitude, tile
            ushort unknown = HexList.ParseHex(_o.UnknownTile);
            var groupOf = new byte[256];
            var defined = new bool[256];
            var altOf = new int[256];
            var tilesOf = new ushort[256][];
            foreach (DragonPaletteEntry e in _rules.Palette)
            {
                groupOf[e.Index] = (byte)(e.Group < 0 ? 0xFF : e.Group);
                ushort[] ids = e.GetTileIds();
                defined[e.Index] = e.Defined && ids.Length > 0;
                altOf[e.Index] = e.Altitude;
                tilesOf[e.Index] = ids;
            }
            var unknownSeen = new bool[256];
            for (int i = 0; i < n; i++)
            {
                byte c = map.Pixels[i];
                _group[i] = groupOf[c];
                if (defined[c])
                {
                    ushort[] ids = tilesOf[c];
                    _tile[i] = ids.Length == 1 ? ids[0] : ids[_rnd.Next(ids.Length)];
                    _z[i] = ClampZ(altOf[c]);
                }
                else
                {
                    _tile[i] = unknown;
                    _z[i] = 0;
                    result.UnknownColors++;
                    unknownSeen[c] = true;
                }
                if ((i & 0xFFFFF) == 0)
                {
                    cancel.ThrowIfCancellationRequested();
                    progress?.Report((int)(i * 10L / n));
                }
            }
            if (result.UnknownColors > 0)
            {
                string list = string.Join(", ", Enumerable.Range(0, 256).Where(k => unknownSeen[k]).Select(k => k.ToString("x2")));
                result.Warnings.Add($"{result.UnknownColors:N0} Pixel mit Farben ohne maptrans-Eintrag ({list}) – Tile {unknown:x4} gesetzt.");
            }

            // 2) unevenly
            if (_o.ApplyUneven)
            {
                ApplyUneven(result, cancel);
            }
            progress?.Report(15);

            // 3) map transitions
            if (_o.ApplyMapTransitions)
            {
                ApplyTransitions(TransitionLayer.Map, result, progress, 15, 45, cancel);
            }

            // 4) static transitions
            if (_o.ApplyStaticTransitions && _o.WriteStatics)
            {
                ApplyTransitions(TransitionLayer.Static, result, progress, 45, 60, cancel);
            }

            // 5) items
            if (_o.ApplyItems && _o.WriteStatics)
            {
                ApplyItems(result, progress, cancel);
            }
            progress?.Report(80);

            // 6) fixes
            if (_o.ApplyFixes)
            {
                foreach (FixEntry f in _rules.Fixes)
                {
                    if (f.X >= 0 && f.Y >= 0 && f.X < _w && f.Y < _h)
                    {
                        int i = f.Y * _w + f.X;
                        _tile[i] = f.TileId;
                        _z[i] = ClampZ(f.Z);
                        result.Fixes++;
                    }
                }
            }

            // 7) merge old statics
            if (_o.MergeExistingStatics && _o.WriteStatics)
            {
                result.MergedStatics = MergeExisting(result);
            }

            cancel.ThrowIfCancellationRequested();
            if (_o.RemoveDuplicateStatics)
            {
                var seen = new HashSet<(int, int, sbyte, ushort, ushort)>();
                int before = _statics.Count;
                _statics = _statics.Where(st => seen.Add((st.X, st.Y, st.Z, st.Id, st.Hue))).ToList();
                result.DuplicatesRemoved = before - _statics.Count;
                if (result.DuplicatesRemoved > 0)
                {
                    _log($"Doppelte Statics entfernt: {result.DuplicatesRemoved:N0}");
                }
            }
            if (!_inMemory)
            {
                WriteFiles(result, progress, cancel);
            }
            progress?.Report(100);
            result.Duration = DateTime.Now - start;
            if (_o.WriteReport && !_inMemory)
            {
                WriteReport(result);
            }
            return result;
        }

        private bool _inMemory;

        /// <summary>Runs all steps without writing files (for the CGI view). Statics are always created.</summary>
        public MapData BuildInMemory(IndexedMap source, IProgress<int> progress, CancellationToken cancel)
        {
            _inMemory = true;
            bool writeStatics = _o.WriteStatics;
            _o.WriteStatics = true;
            try
            {
                BuildResult result = Build(source, progress, cancel);
                var data = new MapData(_w, _h, _tile, _z) { Source = "Editor-Karte (im Speicher gebaut)", MapNumber = _o.MapNumber, Report = result };
                data.Statics.AddRange(_statics);
                return data;
            }
            finally
            {
                _o.WriteStatics = writeStatics;
                _inMemory = false;
            }
        }

        private IndexedMap PadToBlocks(IndexedMap source, BuildResult result)
        {
            int w = (source.Width + 7) / 8 * 8, h = (source.Height + 7) / 8 * 8;
            if (w == source.Width && h == source.Height)
            {
                return source;
            }
            result.Warnings.Add($"Größe {source.Width}×{source.Height} ist nicht durch 8 teilbar – auf {w}×{h} aufgefüllt (Randfarbe wiederholt).");
            var padded = new IndexedMap(w, h);
            for (int y = 0; y < h; y++)
            {
                int sy = Math.Min(y, source.Height - 1);
                for (int x = 0; x < w; x++)
                {
                    padded.Pixels[y * w + x] = source.Pixels[sy * source.Width + Math.Min(x, source.Width - 1)];
                }
            }
            return padded;
        }

        private static sbyte ClampZ(int z) => (sbyte)Math.Clamp(z, -128, 127);

        private void ApplyUneven(BuildResult result, CancellationToken cancel)
        {
            foreach (UnevenRule r in _rules.UnevenRules.Where(r => r.Enabled && r.Amount != 0 && r.Frequency > 0))
            {
                int x0 = Math.Max(0, r.Left), y0 = Math.Max(0, r.Top);
                int x1 = Math.Min(_w - 1, r.Right), y1 = Math.Min(_h - 1, r.Bottom);
                for (int y = y0; y <= y1; y++)
                {
                    for (int x = x0; x <= x1; x++)
                    {
                        int i = y * _w + x;
                        if (_group[i] == r.Group && _rnd.Next(1000) < r.Frequency)
                        {
                            _z[i] = ClampZ(_z[i] + _rnd.Next(Math.Abs(r.Amount) + 1) * Math.Sign(r.Amount));
                            result.UnevenCells++;
                        }
                    }
                    cancel.ThrowIfCancellationRequested();
                }
            }
        }

        private List<CompiledRule> Compile(TransitionLayer layer)
        {
            var list = new List<CompiledRule>();
            foreach (TransitionRule r in _rules.Transitions.Where(t => t.Enabled && t.Layer == layer))
            {
                TransitionFile f = _rules.FindTransitionFile(r.File);
                if (f == null)
                {
                    continue;
                }
                var dict = new Dictionary<byte, TransitionPattern>();
                foreach (TransitionPattern p in f.Patterns)
                {
                    if (p.GetTileIds().Length > 0)
                    {
                        dict.TryAdd(p.Mask, p);
                    }
                }
                if (dict.Count > 0)
                {
                    list.Add(new CompiledRule { Rule = r, Patterns = dict });
                }
            }
            return list;
        }

        private void ApplyTransitions(TransitionLayer layer, BuildResult result, IProgress<int> progress, int p0, int p1, CancellationToken cancel)
        {
            List<CompiledRule> all = Compile(layer);
            if (all.Count == 0)
            {
                return;
            }
            // candidate rules per cell group (keeps the file order)
            var byGroup = new List<CompiledRule>[256];
            for (int g = 0; g < 256; g++)
            {
                var l = all.Where(c => c.Rule.GroupA == g || (c.Rule.GroupA < 0 && c.Rule.GroupB != g)).ToList();
                byGroup[g] = l.Count > 0 ? l : null;
            }

            bool isMap = layer == TransitionLayer.Map;
            // group pairs with a rule: directed (cell group -> neighbour group) and in any direction
            var directed = new HashSet<int>();
            var anyDirection = new HashSet<int>();
            foreach (CompiledRule cr in all)
            {
                int ga = cr.Rule.GroupA, gb = cr.Rule.GroupB;
                for (int x = 0; x < 256; x++)
                {
                    if (ga >= 0 && x != ga)
                    {
                        continue;
                    }
                    for (int y = 0; y < 256; y++)
                    {
                        if (x != y && (gb < 0 || y == gb) && (ga >= 0 || x != gb))
                        {
                            directed.Add((x << 8) | y);
                            anyDirection.Add((x << 8) | y);
                            anyDirection.Add((y << 8) | x);
                        }
                    }
                }
            }
            var ng = new byte[8];
            var newTile = isMap ? (ushort[])_tile.Clone() : null;
            var newZ = isMap ? (sbyte[])_z.Clone() : null;
            long applied = 0;

            for (int y = 0; y < _h; y++)
            {
                for (int x = 0; x < _w; x++)
                {
                    int i = y * _w + x;
                    byte g = _group[i];
                    List<CompiledRule> rules = byGroup[g];
                    if (rules == null)
                    {
                        continue;
                    }
                    bool edge = false;
                    for (int k = 0; k < 8; k++)
                    {
                        int nx = x + Dx[k], ny = y + Dy[k];
                        ng[k] = nx < 0 || ny < 0 || nx >= _w || ny >= _h ? g : _group[ny * _w + nx];
                        edge |= ng[k] != g;
                    }
                    if (!edge)
                    {
                        continue;
                    }

                    bool matched = false;
                    foreach (CompiledRule cr in rules)
                    {
                        int b = cr.Rule.GroupB;
                        byte mask = 0;
                        for (int k = 0; k < 8; k++)
                        {
                            bool isB = b < 0 ? ng[k] != g : ng[k] == b;
                            if (isB)
                            {
                                mask |= (byte)(1 << k);
                            }
                        }
                        if (mask == 0 || !cr.Patterns.TryGetValue(mask, out TransitionPattern p))
                        {
                            continue;
                        }
                        ushort[] ids = p.GetTileIds();
                        if (isMap)
                        {
                            newTile[i] = ids.Length == 1 ? ids[0] : ids[_rnd.Next(ids.Length)];
                            if (_o.MapTransitionZ)
                            {
                                newZ[i] = ClampZ(_z[i] + RandomZ(p));
                            }
                            applied++;
                            matched = true;
                            break; // one map tile per cell
                        }

                        int baseZ = _o.StaticZFromNeighbours ? SourceGroupTopZ(x, y, cr.Rule) : _z[i];
                        if (_o.StaticMode == StaticTileMode.RandomOne)
                        {
                            AddStatic(ids[_rnd.Next(ids.Length)], x, y, baseZ + RandomZ(p), 0);
                            applied++;
                        }
                        else
                        {
                            int dz = RandomZ(p);
                            foreach (ushort id in ids)
                            {
                                AddStatic(id, x, y, baseZ + dz, 0);
                                applied++;
                            }
                        }
                        if (_o.ItemsAvoidTransitionStatics)
                        {
                            _occupied[i] = true;
                        }
                        matched = true;
                    }

                    if (isMap && !matched)
                    {
                        // which neighbour group is the problem?
                        byte other = 0xFF;
                        byte mask = 0;
                        for (int k = 0; k < 8; k++)
                        {
                            if (ng[k] != g)
                            {
                                if (other == 0xFF)
                                {
                                    other = ng[k];
                                }
                                mask |= (byte)(1 << k);
                            }
                        }
                        string key = null;
                        if (directed.Contains((g << 8) | other))
                        {
                            key = $"Muster fehlt  {g:x2}->{other:x2} {TransitionPattern.MaskToPattern(mask)}";
                        }
                        else if (!anyDirection.Contains((g << 8) | other))
                        {
                            key = $"Regel fehlt   {g:x2}<>{other:x2} ({_rules.GroupName(g)} / {_rules.GroupName(other)})";
                        }
                        if (key != null)
                        {
                            result.UnmatchedEdges++;
                            if (result.MissingTransitions.Count < 5000 || result.MissingTransitions.ContainsKey(key))
                            {
                                result.MissingTransitions.TryGetValue(key, out int count);
                                result.MissingTransitions[key] = count + 1;
                                if (!result.MissingSamples.TryGetValue(key, out var samples))
                                {
                                    result.MissingSamples[key] = samples = new List<System.Drawing.Point>();
                                }
                                if (samples.Count < 10)
                                {
                                    samples.Add(new System.Drawing.Point(x, y));
                                }
                            }
                        }
                    }
                }
                if ((y & 31) == 0)
                {
                    cancel.ThrowIfCancellationRequested();
                    progress?.Report(p0 + (p1 - p0) * y / _h);
                }
            }

            if (isMap)
            {
                _tile = newTile;
                _z = newZ;
                result.MapTransitions = applied;
                _log($"Map-Übergänge: {applied:N0} Tiles, {result.UnmatchedEdges:N0} Rand-Tiles ohne passenden Übergang (siehe dragon_log.txt).");
            }
            else
            {
                result.StaticTransitionStatics = applied;
                _log($"Static-Übergänge: {applied:N0} Statics.");
            }
        }

        /// <summary>Random height change between Z1 and Z2 (both included).</summary>
        private int RandomZ(TransitionPattern p)
        {
            int a = Math.Min(p.Z1, p.Z2), b = Math.Max(p.Z1, p.Z2);
            return a == b ? a : _rnd.Next(a, b + 1);
        }

        /// <summary>Highest land z of the cell and its 8 neighbours that belong to the source group of the rule.</summary>
        private int SourceGroupTopZ(int x, int y, TransitionRule rule)
        {
            int source = rule.GroupA >= 0 ? rule.GroupA : rule.GroupB;
            int best = int.MinValue;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= _w || ny >= _h)
                    {
                        continue;
                    }
                    int i = ny * _w + nx;
                    if (_group[i] == source)
                    {
                        best = Math.Max(best, _z[i]);
                    }
                }
            }
            return best == int.MinValue ? _z[y * _w + x] : best;
        }

        private void AddStatic(ushort id, int x, int y, int z, ushort hue)
        {
            _statics.Add(new StaticEntry { Id = id, X = x, Y = y, Z = ClampZ(z), Hue = hue });
        }

        private void ApplyItems(BuildResult result, IProgress<int> progress, CancellationToken cancel)
        {
            var byGroup = new List<(ItemRule rule, ItemFile file, int total)>[256];
            foreach (ItemRule r in _rules.ItemRules.Where(r => r.Enabled && r.Chance > 0))
            {
                ItemFile f = _rules.FindItemFile(r.File);
                if (f == null || f.Groups.Count == 0)
                {
                    continue;
                }
                int total = f.Groups.Sum(g => Math.Max(0, g.Frequency));
                if (total <= 0)
                {
                    continue;
                }
                int gi = r.Group & 0xFF;
                (byGroup[gi] ??= new List<(ItemRule, ItemFile, int)>()).Add((r, f, total));
            }
            int chanceBase = Math.Max(1, _o.ItemChanceBase);

            for (int y = 0; y < _h; y++)
            {
                for (int x = 0; x < _w; x++)
                {
                    int i = y * _w + x;
                    if (_occupied[i])
                    {
                        continue;
                    }
                    var list = byGroup[_group[i]];
                    if (list == null)
                    {
                        continue;
                    }
                    foreach ((ItemRule rule, ItemFile file, int total) in list)
                    {
                        if (x < rule.Left || x > rule.Right || y < rule.Top || y > rule.Bottom)
                        {
                            continue;
                        }
                        if (_rnd.Next(chanceBase) >= rule.Chance)
                        {
                            continue;
                        }
                        ItemGroupDef def = Pick(file, total);
                        if (def != null && TryPlace(def, x, y, _group[i]))
                        {
                            result.Items++;
                            result.ItemStatics += def.Parts.Count;
                            break;
                        }
                    }
                }
                if ((y & 31) == 0)
                {
                    cancel.ThrowIfCancellationRequested();
                    progress?.Report(60 + 20 * y / _h);
                }
            }
            _log($"Items: {result.Items:N0} Objekte mit {result.ItemStatics:N0} Statics.");
        }

        private ItemGroupDef Pick(ItemFile file, int total)
        {
            int r = _rnd.Next(total);
            foreach (ItemGroupDef g in file.Groups)
            {
                r -= Math.Max(0, g.Frequency);
                if (r < 0)
                {
                    return g;
                }
            }
            return null;
        }

        private bool TryPlace(ItemGroupDef def, int x, int y, byte group)
        {
            foreach (ItemPart p in def.Parts)
            {
                int tx = x + p.Dx, ty = y + p.Dy;
                if (tx < 0 || ty < 0 || tx >= _w || ty >= _h)
                {
                    return false;
                }
                int ti = ty * _w + tx;
                if (_o.ItemSameGroup && _group[ti] != group)
                {
                    return false;
                }
                if (_o.ItemOverlapCheck && _occupied[ti])
                {
                    return false;
                }
            }
            foreach (ItemPart p in def.Parts)
            {
                int tx = x + p.Dx, ty = y + p.Dy;
                int ti = ty * _w + tx;
                AddStatic(p.Id, tx, ty, _z[ti] + p.Z, p.Hue);
                _occupied[ti] = true;
            }
            return true;
        }

        private long MergeExisting(BuildResult result)
        {
            if (!File.Exists(_o.MergeStaidxFile) || !File.Exists(_o.MergeStaticsFile))
            {
                result.Warnings.Add("Statics übernehmen: staidx / statics Datei nicht gefunden.");
                return 0;
            }
            long count = 0;
            int bw = _w / 8, bh = _h / 8;
            using var idx = new BinaryReader(File.OpenRead(_o.MergeStaidxFile));
            using var sta = new BinaryReader(File.OpenRead(_o.MergeStaticsFile));
            long blocks = Math.Min((long)bw * bh, idx.BaseStream.Length / 12);
            for (long b = 0; b < blocks; b++)
            {
                int offset = idx.ReadInt32();
                int length = idx.ReadInt32();
                idx.ReadInt32();
                if (offset < 0 || length <= 0 || offset + (long)length > sta.BaseStream.Length)
                {
                    continue;
                }
                int bx = (int)(b / bh), by = (int)(b % bh);
                sta.BaseStream.Seek(offset, SeekOrigin.Begin);
                for (int k = 0; k < length / 7; k++)
                {
                    ushort id = sta.ReadUInt16();
                    byte x = sta.ReadByte();
                    byte y = sta.ReadByte();
                    sbyte z = sta.ReadSByte();
                    ushort hue = sta.ReadUInt16();
                    _statics.Add(new StaticEntry { Id = id, X = bx * 8 + (x & 7), Y = by * 8 + (y & 7), Z = z, Hue = hue });
                    count++;
                }
            }
            _log($"Bestehende Statics übernommen: {count:N0}");
            return count;
        }

        // ------------------------------------------------------------------ output

        private void WriteFiles(BuildResult result, IProgress<int> progress, CancellationToken cancel)
        {
            string folder = string.IsNullOrWhiteSpace(_o.OutputFolder) ? Environment.CurrentDirectory : _o.OutputFolder;
            Directory.CreateDirectory(folder);
            int bw = _w / 8, bh = _h / 8;
            int num = _o.MapNumber;

            string mapPath = Path.Combine(folder, $"map{num}.mul");
            if (_o.WriteMul || _o.WriteUop)
            {
                string target = _o.WriteMul ? mapPath : Path.Combine(folder, $"map{num}.mul.tmp_for_uop");
                MulMapIO.WriteMap(target, _w, _h, _tile, _z);
                if (_o.WriteMul)
                {
                    result.Files.Add(mapPath);
                }
                _log($"geschrieben: {target}");
                progress?.Report(88);

                if (_o.WriteUop)
                {
                    string uop = Path.Combine(folder, $"map{num}LegacyMUL.uop");
                    MulMapIO.WriteUop(target, uop, num);
                    result.Files.Add(uop);
                    _log($"geschrieben: {uop}");
                    if (!_o.WriteMul)
                    {
                        File.Delete(target);
                    }
                }
            }
            cancel.ThrowIfCancellationRequested();

            if (_o.WriteStatics)
            {
                string idxPath = Path.Combine(folder, $"staidx{num}.mul");
                string staPath = Path.Combine(folder, $"statics{num}.mul");
                MulMapIO.WriteStatics(idxPath, staPath, bw, bh, _statics);
                result.Files.Add(idxPath);
                result.Files.Add(staPath);
                _log($"geschrieben: {idxPath}, {staPath} ({_statics.Count:N0} Statics)");
                if (_o.WriteWsc)
                {
                    string wsc = Path.Combine(folder, $"statics{num}.wsc");
                    WriteWsc(wsc, _statics);
                    result.Files.Add(wsc);
                }
            }
            progress?.Report(98);
        }

        private static void WriteWsc(string path, List<StaticEntry> statics)
        {
            using var sw = new StreamWriter(path, false, Encoding.ASCII);
            int serial = 0x40000000;
            int n = 0;
            foreach (StaticEntry s in statics)
            {
                sw.WriteLine($"SECTION WORLDITEM {n++}");
                sw.WriteLine("{");
                sw.WriteLine($"SERIAL {serial++}");
                sw.WriteLine("NAME #");
                sw.WriteLine($"ID {s.Id}");
                sw.WriteLine($"X {s.X}");
                sw.WriteLine($"Y {s.Y}");
                sw.WriteLine($"Z {s.Z}");
                sw.WriteLine($"COLOR {s.Hue}");
                sw.WriteLine("CONT -1");
                sw.WriteLine("TYPE 255");
                sw.WriteLine("AMOUNT 1");
                sw.WriteLine("}");
                sw.WriteLine();
            }
        }

        private void WriteReport(BuildResult r)
        {
            try
            {
                string folder = string.IsNullOrWhiteSpace(_o.OutputFolder) ? Environment.CurrentDirectory : _o.OutputFolder;
                string path = Path.Combine(folder, "dragon_log.txt");
                File.WriteAllText(path, BuildLog.Create(_rules, _o, r));
                r.Files.Add(path);
            }
            catch (Exception ex)
            {
                _log("Log konnte nicht geschrieben werden / log could not be written: " + ex.Message);
            }
        }
    }
}

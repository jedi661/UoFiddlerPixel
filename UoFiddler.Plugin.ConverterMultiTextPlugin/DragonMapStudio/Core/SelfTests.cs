/***************************************************************************
 *
 *  * Dragon Map Studio – automatic self tests (test assistant)
 *  *
 *  *  Every test builds on a small map in memory (nothing of the real map changes) and logs the values it found –
 *  *  with the real client files of the user (tiledata, art), which the developer does not have. The log goes
 *  *  into the test report.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using Ultima;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public enum TestStatus
    {
        Ok,
        Warn,
        Fail,
        Info
    }

    /// <summary>A test map to draw as a picture (with the client art) for the report.</summary>
    public sealed class TestPicture
    {
        public string Title;
        public MapData Map;
        public Rectangle Area;
    }

    public sealed class TestResult
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public List<TestPicture> Pictures { get; set; } = new List<TestPicture>();

        public string Area { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TestStatus Status { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public double Milliseconds { get; set; }

        public override string ToString() => $"[{Status}] {Area} – {Name}: {Summary}";
    }

    public static class SelfTests
    {
        private sealed class Ctx
        {
            public readonly StringBuilder Log = new StringBuilder();
            public readonly List<TestPicture> Pictures = new List<TestPicture>();

            /// <summary>Keeps the test map for a picture (a copy of the area, so later changes of the test do not show).</summary>
            public void Picture(string title, MapData map, Rectangle area)
            {
                area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
                Rectangle big = Rectangle.Inflate(area, 4, 4);
                big.Intersect(new Rectangle(0, 0, map.Width, map.Height));
                MapData copy = map.CopyRegion(big, out Point o);
                Pictures.Add(new TestPicture { Title = title, Map = copy, Area = new Rectangle(area.X - o.X, area.Y - o.Y, area.Width, area.Height) });
            }
            public TestStatus Status = TestStatus.Ok;
            public string Summary = string.Empty;

            public void Line(string s) => Log.AppendLine(s);

            public void Warn(string s)
            {
                Line("⚠ " + s);
                if (Status == TestStatus.Ok)
                {
                    Status = TestStatus.Warn;
                }
            }

            public void Fail(string s)
            {
                Line("✖ " + s);
                Status = TestStatus.Fail;
            }

            public void Check(bool ok, string what)
            {
                if (ok)
                {
                    Line("✔ " + what);
                }
                else
                {
                    Fail(what);
                }
            }
        }

        /// <summary>All tests: (area, name, test). The brush settings of the studio give the chosen sets.</summary>
        public static List<(string area, string name, Action<object> run)> List(CgiBrushSettings b)
        {
            var list = new List<(string, string, Action<object>)>
            {
                ("Client", "Client-Dateien (TileData, Art, Hues)", c => Client((Ctx)c)),
                ("Brücke", "Teile aus der TileData (Bretter, Geländer, Pfosten, Pfeiler)", c => BridgePieces((Ctx)c)),
                ("Brücke", "Richtung der Geländer (Bild-Erkennung) und Gegenstück", c => RailDirections((Ctx)c)),
                ("Brücke", "Brücke West-Ost und Nord-Süd bauen", c => BridgeBuild((Ctx)c, b)),
                ("Brücke", "Bogen aus Stufen", c => BridgeArchStairs((Ctx)c, b)),
                ("Treppe", "Treppen-Gruppe aus der TileData", c => AutoStairs((Ctx)c)),
                ("Treppe", "Treppenhaus: Höhe, Absatz, gefüllt, alle Wenden", c => StaircaseShapes((Ctx)c, b)),
                ("Treppe", "Treppe lernen (bauen → lernen → vergleichen)", c => StairLearn((Ctx)c, b)),
                ("Drehen", "Drehen 90°: gerichtete Teile tauschen", c => RotateSwap((Ctx)c, b)),
                ("Wand", "Wand Punkt zu Punkt, Dicke, Ecken", c => WallPath((Ctx)c, b)),
                ("Fluss", "Fluss-Lauf: Breite, Mitte bleibt Wasser, Grund wie Küste", c => River((Ctx)c, b)),
                ("Küste", "Küste glätten: Zacken vorher / nachher", c => Coast((Ctx)c)),
                ("Unebenheit", "Muster: Wertebereich", c => Noise((Ctx)c)),
                ("Dungeon", "Grundriss, Stil, 2 Etagen mit Treppe", c => Dungeon((Ctx)c, b)),
                ("Werkstatt", "Vorlage ausschneiden und wieder einsetzen", c => Piece((Ctx)c)),
                ("Katalog", "Eingebaute Kategorien (Anzahl, Beispiele)", c => Categories((Ctx)c)),
                ("Einstellungen", "Pinsel-Einstellungen als JSON speichern / laden", c => SettingsJson((Ctx)c, b))
            };
            return list;
        }

        public static TestResult Run(string area, string name, Action<object> test)
        {
            var c = new Ctx();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                test(c);
            }
            catch (Exception ex)
            {
                c.Fail("Ausnahme: " + ex.GetType().Name + ": " + ex.Message);
                c.Line(ex.StackTrace ?? string.Empty);
            }
            sw.Stop();
            string first = c.Log.ToString().Split('\n').FirstOrDefault(l => l.StartsWith("✖") || l.StartsWith("⚠")) ?? "alles in Ordnung";
            return new TestResult
            {
                Area = area, Name = name, Status = c.Status, Details = c.Log.ToString(), Milliseconds = sw.Elapsed.TotalMilliseconds,
                Summary = c.Summary.Length > 0 ? c.Summary : first.Trim(),
                Pictures = c.Pictures
            };
        }

        // ------------------------------------------------------------------ helpers

        private static MapData Map(int w, int h, ushort land, int z) =>
            new MapData(w, h, Enumerable.Repeat(land, w * h).ToArray(), Enumerable.Repeat((sbyte)z, w * h).ToArray());

        private static string Item(int id) => $"{id:x4} '{UoArtCache.ItemName(id)}' h{UoArtCache.ItemHeight(id)} dir {Dir(id)}";

        private static string Dir(int id)
        {
            try
            {
                int o = BridgeKit.Orientation(id);
                return o > 0 ? "W-O" : o < 0 ? "N-S" : "-";
            }
            catch (Exception)
            {
                return "?";
            }
        }

        private static bool ClientLoaded()
        {
            try
            {
                return TileData.ItemTable != null && TileData.ItemTable.Length > 0 && UoArtCache.ArtAvailableCached;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static StairSet ChosenStairs(CgiBrushSettings b, out bool fake)
        {
            StairSet s = b.StairSets.FirstOrDefault(x => x.Name == b.StairSet);
            static bool Full(StairSet x) => x != null && HexList.Parse(x.DownNorth + " " + x.DownEast + " " + x.DownSouth + " " + x.DownWest).Length >= 4;
            if (!Full(s) && ClientLoaded())
            {
                try
                {
                    s = BridgeKit.AutoStairs(); // pictures with real art even without a chosen set
                }
                catch (Exception)
                {
                    s = null;
                }
            }
            fake = !Full(s);
            return fake ? new StairSet { Name = "Test", DownNorth = "7001", DownEast = "7002", DownSouth = "7003", DownWest = "7004", Block = "7010", Landing = "7020" } : s;
        }

        // ------------------------------------------------------------------ tests

        private static void Client(Ctx c)
        {
            bool ok = ClientLoaded();
            c.Line($"TileData Items: {(TileData.ItemTable?.Length ?? 0)}, Land: {(TileData.LandTable?.Length ?? 0)}");
            c.Line($"Art geladen: {UoArtCache.ArtAvailableCached}, höchste Item-ID: {UoArtCache.MaxItemId:x4}");
            try
            {
                c.Line($"Hues: {Hues.List?.Length ?? 0}");
            }
            catch (Exception)
            {
                c.Line("Hues: nicht geladen");
            }
            if (!ok)
            {
                c.Fail("Client-Dateien nicht geladen – die Tests mit TileData / Art sagen dann wenig.");
            }
            c.Summary = ok ? "Client-Dateien geladen" : "Client-Dateien fehlen";
        }

        private static void BridgePieces(Ctx c)
        {
            foreach (BridgeKit.Role r in Enum.GetValues(typeof(BridgeKit.Role)))
            {
                List<int> f = BridgeKit.Find(r, 12);
                c.Line($"{BridgeKit.RoleName(r)}: {f.Count} gefunden (beste zuerst)");
                foreach (int id in f)
                {
                    c.Line("   " + Item(id));
                }
            }
            BridgeGenSettings g = BridgeKit.AutoPieces();
            c.Line($"Automatisch gewählt: Bretter {g.Deck}, Geländer W-O {g.RailX}, N-S {g.RailY}, Pfosten {g.Post}, Pfeiler {g.Pillar}");
            if (string.IsNullOrEmpty(g.Deck))
            {
                c.Fail("Keine Bretter gefunden.");
            }
            if (string.IsNullOrEmpty(g.RailX) || string.IsNullOrEmpty(g.RailY))
            {
                c.Warn("Geländer für eine Richtung fehlt.");
            }
            foreach (ushort id in HexList.Parse(g.RailX))
            {
                if (BridgeKit.Orientation(id) <= 0)
                {
                    c.Warn($"Geländer W-O {id:x4} hat laut Bild nicht die Richtung W-O.");
                }
            }
            foreach (ushort id in HexList.Parse(g.RailY))
            {
                if (BridgeKit.Orientation(id) >= 0)
                {
                    c.Warn($"Geländer N-S {id:x4} hat laut Bild nicht die Richtung N-S.");
                }
            }
            c.Summary = $"Bretter {g.Deck}, Geländer {g.RailX} / {g.RailY}, Pfosten {g.Post}, Pfeiler {g.Pillar}";
        }

        private static void RailDirections(Ctx c)
        {
            int pairs = 0, single = 0;
            foreach (int id in BridgeKit.Find(BridgeKit.Role.Rail, 40))
            {
                ushort t = BridgeKit.Turned((ushort)id);
                c.Line($"{Item(id)}  →  Gegenstück {(t == id ? "keins" : Item(t))}");
                if (BridgeKit.Orientation(id) != 0)
                {
                    if (t != id)
                    {
                        pairs++;
                    }
                    else
                    {
                        single++;
                    }
                }
            }
            if (pairs == 0)
            {
                c.Warn("Kein Geländer mit Gegenstück gefunden – Drehen und 'andere Richtung ableiten' können nicht tauschen.");
            }
            c.Summary = $"{pairs} Geländer mit Gegenstück, {single} ohne";
        }

        private static void BridgeBuild(Ctx c, CgiBrushSettings b)
        {
            BridgeGenSettings g = BridgeKit.AutoPieces();
            if (string.IsNullOrEmpty(g.Deck))
            {
                g.Deck = "7100";
                g.RailX = "7101";
                g.RailY = "7102";
                g.Post = "7103";
                c.Warn("Keine Teile in der TileData – Test mit Platzhalter-IDs.");
            }
            g.Width = 3;
            BridgeTemplate t = BridgeKit.Generate(g, "Test");
            foreach (bool alongX in new[] { true, false })
            {
                MapData m = Map(64, 64, 0x00A8, -5);
                var a = alongX ? new Point(10, 32) : new Point(32, 10);
                var e = alongX ? new Point(40, 32) : new Point(32, 40);
                List<StaticChange> ch = BridgeKit.Build(m, a, e, t, new BridgeSettings { Width = 3, Arch = 4, Railings = true, Stairs = false }, new Random(1));
                var st = ch.Select(x => x.After).ToList();
                int decks = st.Count(s => HexList.Parse(g.Deck + " " + g.DeckY).Contains(s.Id));
                c.Line($"{(alongX ? "West-Ost" : "Nord-Süd")}: {st.Count} Statics, Bretter {decks}, z {st.Min(s => s.Z)}…{st.Max(s => s.Z)}");
                foreach (IGrouping<ushort, StaticItem> grp in st.GroupBy(s => s.Id).OrderByDescending(x => x.Count()))
                {
                    c.Line($"   {grp.Count()} × {Item(grp.Key)}");
                }
                c.Check(decks == 31 * 3, $"Bretter = Länge × Breite (31 × 3 = 93, gebaut {decks})");
                ushort[] want = HexList.Parse(alongX ? g.Deck : (string.IsNullOrEmpty(g.DeckY) ? g.Deck : g.DeckY));
                int wrongDir = st.Count(s => HexList.Parse(g.Deck + " " + g.DeckY).Contains(s.Id) && !want.Contains(s.Id));
                c.Check(wrongDir == 0, $"nur Bretter der Richtung {(alongX ? "West-Ost" : "Nord-Süd")} ({string.Join(" ", want.Select(i => i.ToString("x4")))}), falsche: {wrongDir}");
                m.Apply(ch);
                c.Picture($"Brücke {(alongX ? "West-Ost" : "Nord-Süd")}", m, alongX ? new Rectangle(8, 28, 35, 9) : new Rectangle(28, 8, 9, 35));
                ushort[] rail = HexList.Parse(alongX ? g.RailX : g.RailY);
                if (rail.Length > 0)
                {
                    c.Check(st.Count(s => s.Id == rail[0]) >= 60, $"Geländer {rail[0]:x4} auf beiden Seiten (≥ 60)");
                }
            }
        }

        private static void BridgeArchStairs(Ctx c, CgiBrushSettings b)
        {
            StairSet set = ChosenStairs(b, out bool fake);
            c.Line(fake ? "Kein vollständiges Treppen-Set gewählt – Platzhalter-Stufen." : $"Treppen-Set '{set.Name}': N {set.DownNorth}, O {set.DownEast}, S {set.DownSouth}, W {set.DownWest}");
            var g = new BridgeGenSettings { Deck = "7100", RailX = "7101", RailY = "7102", Width = 3 };
            BridgeTemplate t = BridgeKit.Generate(g, "Test");
            BridgeGenSettings real = ClientLoaded() ? BridgeKit.AutoPieces() : null;
            t.StairWest = set.DownWest;
            t.StairEast = set.DownEast;
            MapData m = Map(64, 64, 0x00A8, -5);
            List<StaticChange> ch = BridgeKit.Build(m, new Point(10, 32), new Point(40, 32), t, new BridgeSettings { Width = 3, Arch = 20, ArchStairs = true, Stairs = false }, new Random(1));
            var middle = ch.Select(x => x.After).Where(s => s.Y == 32 && !HexList.Parse("7101 7102").Contains(s.Id)).OrderBy(s => s.X).ToList();
            c.Line("Mitte der Brücke (x: id@z): " + string.Join(" ", middle.Select(s => $"{s.X}:{s.Id:x4}@{s.Z}")));
            int steps = middle.Count(s => s.Id != 0x7100);
            c.Check(steps == 8, $"4 Stufen hinauf + 4 hinunter (gefunden {steps})");
            // the water lies on −5: deck on −5 + 20 = 15
            c.Check(middle.Where(s => s.Id == 0x7100).All(s => s.Z == 15), "Bretter oben flach (Ufer −5 + Bogen 20 = 15)");
            if (real != null && !string.IsNullOrEmpty(real.Deck))
            {
                // the same with the real pieces, for the picture
                real.Width = 3;
                BridgeTemplate rt = BridgeKit.Generate(real, "Echt");
                rt.StairWest = set.DownWest;
                rt.StairEast = set.DownEast;
                MapData m2 = Map(64, 64, 0x00A8, -5);
                m2.Apply(BridgeKit.Build(m2, new Point(10, 32), new Point(40, 32), rt, new BridgeSettings { Width = 3, Arch = 20, ArchStairs = true, Stairs = false }, new Random(1)));
                c.Picture("Bogen aus Stufen (echte Teile)", m2, new Rectangle(8, 28, 35, 9));
            }
        }

        private static void AutoStairs(Ctx c)
        {
            List<int> f = BridgeKit.Find(BridgeKit.Role.Stairs, 30);
            c.Line($"{f.Count} Treppen-Teile (beste zuerst):");
            foreach (int id in f)
            {
                ItemData d = TileData.ItemTable[id];
                c.Line($"   {Item(id)}  Flags: {d.Flags}");
            }
            StairSet s = BridgeKit.AutoStairs(out string seen);
            c.Line("Neigung aus dem Bild (Oberkante links / Mitte / rechts):");
            c.Line(seen);
            c.Line($"Automatische Gruppe '{s.Name}': hinunter N {s.DownNorth}, O {s.DownEast}, S {s.DownSouth}, W {s.DownWest}");
            if (string.IsNullOrEmpty(s.DownNorth))
            {
                c.Warn("Keine Vierer-Gruppe gefunden – Treppe lernen benutzen.");
            }
            c.Summary = string.IsNullOrEmpty(s.DownNorth) ? "keine Gruppe" : $"{s.Name}: hinunter N {s.DownNorth}, O {s.DownEast}, S {s.DownSouth}, W {s.DownWest}";
            if (s.Name.Contains("geraten"))
            {
                c.Warn("Richtung der Stufen nicht aus dem Bild erkannt – bitte 'Treppe aus Auswahl lernen'.");
            }
        }

        private static void StaircaseShapes(Ctx c, CgiBrushSettings b)
        {
            StairSet set = ChosenStairs(b, out bool fake);
            c.Line(fake ? "Platzhalter-Treppe (kein vollständiges Set gewählt)." : $"Treppen-Set '{set.Name}': N {set.DownNorth} O {set.DownEast} S {set.DownSouth} W {set.DownWest} Block {set.Block} Absatz {set.Landing}");
            if (!fake)
            {
                StairSet check = BridgeKit.FixStairs(set, out string rep);
                c.Line("Prüfung der Neigung aus dem Bild:");
                c.Line(rep);
                if (check.DownNorth != set.DownNorth || check.DownEast != set.DownEast || check.DownSouth != set.DownSouth || check.DownWest != set.DownWest)
                {
                    c.Warn("Laut Bild passen die Stufen des Sets nicht zu ihren Richtungen – Knopf 'Richtungen aus Bild' beim Werkzeug Treppe.");
                }
            }
            StairPicture(c, set);
            foreach (StairTurn turn in Enum.GetValues(typeof(StairTurn)))
            {
                foreach (int width in new[] { 1, 2 })
                {
                    MapData m = Map(64, 64, 3, 0);
                    var o = new StaircaseSettings { Up = Compass.North, Floors = 2, Steps = 4, StepHeight = 5, Width = width, Turn = turn };
                    var cells = new Dictionary<Point, int>();
                    List<StaticChange> ch = Staircase.Build(m, new Point(30, 40), 0, o, set, cells);
                    m.Apply(ch);
                    int top = cells.Values.Max();
                    // solid: every used cell has a piece on every 5 z from 0 up to its top
                    int holes = 0;
                    foreach (KeyValuePair<Point, int> kv in cells)
                    {
                        var zs = new HashSet<int>(m.StaticsAt(kv.Key.X, kv.Key.Y).Select(s => s.Z));
                        for (int z = 0; z < kv.Value; z += 5)
                        {
                            if (!zs.Contains(z))
                            {
                                holes++;
                            }
                        }
                    }
                    bool overlap = ch.GroupBy(x => (x.After.X, x.After.Y, x.After.Z)).Any(g => g.Count() > 1);
                    c.Line($"{turn}, Breite {width}: {ch.Count} Statics, {cells.Count} Felder, oben z {top}, Löcher {holes}, doppelt {overlap}");
                    c.Check(top == 40, $"{turn} / {width}: 2 Etagen = 40 Z (ist {top})");
                    c.Check(holes == 0, $"{turn} / {width}: bis unten gefüllt");
                }
            }
        }

        private static void StairPicture(Ctx c, StairSet set)
        {
            MapData m = Map(64, 64, 3, 0);
            int i = 0;
            foreach (StairTurn turn in Enum.GetValues(typeof(StairTurn)))
            {
                var o = new StaircaseSettings { Up = Compass.West, Floors = 2, Steps = 4, Width = 1, Turn = turn };
                m.Apply(Staircase.Build(m, new Point(20 + (i % 2) * 24, 14 + (i / 2) * 26), 0, o, set));
                i++;
            }
            string[] names = { "gerade", "rechts herum", "links herum", "zurück" };
            for (int k = 0; k < 4; k++)
            {
                int x = 20 + (k % 2) * 24, y = 14 + (k / 2) * 26;
                c.Picture($"Treppenhaus {names[k]} (2 Etagen, hinauf nach West)", m, new Rectangle(x - 12, y - 6, 16, 16));
            }
        }

        private static void StairLearn(Ctx c, CgiBrushSettings b)
        {
            StairSet set = ChosenStairs(b, out _);
            MapData m = Map(80, 80, 3, 0);
            // one flight in every direction
            int n = 0;
            foreach (Compass up in new[] { Compass.North, Compass.East, Compass.South, Compass.West })
            {
                var o = new StaircaseSettings { Up = up, Floors = 1, Steps = 4, Width = 1, TopLanding = true };
                m.Apply(Staircase.Build(m, new Point(15 + n * 16, 40), 0, o, set));
                n++;
            }
            string[] dirs = { "Nord", "Ost", "Süd", "West" };
            for (int k = 0; k < 4; k++)
            {
                c.Picture($"Treppe hinauf nach {dirs[k]} (Klick bei {15 + k * 16}, 40)", m, new Rectangle(15 + k * 16 - 6, 34, 13, 13));
            }
            StairSet learned = Staircase.Learn(m.AllStatics().ToList(), "gelernt", out string report);
            c.Line(report);
            c.Check(learned.DownNorth == set.DownNorth.Split(' ')[0], $"hinunter Nord {learned.DownNorth} = {set.DownNorth}");
            c.Check(learned.DownEast == set.DownEast.Split(' ')[0], $"hinunter Ost {learned.DownEast} = {set.DownEast}");
            c.Check(learned.DownSouth == set.DownSouth.Split(' ')[0], $"hinunter Süd {learned.DownSouth} = {set.DownSouth}");
            c.Check(learned.DownWest == set.DownWest.Split(' ')[0], $"hinunter West {learned.DownWest} = {set.DownWest}");
        }

        private static void RotateSwap(Ctx c, CgiBrushSettings b)
        {
            var ids = new List<ushort>();
            WallSet w = b.WallSets.FirstOrDefault(x => x.Name == b.WallSet);
            if (w != null)
            {
                ids.AddRange(HexList.Parse(w.AlongX + " " + w.AlongY));
                c.Line($"Wand-Set '{w.Name}': X {w.AlongX}, Y {w.AlongY}, Pfosten {w.Post}, Ecke {w.Corner}");
            }
            ids.AddRange(BridgeKit.Find(BridgeKit.Role.Rail, 10).Select(i => (ushort)i));
            ids.AddRange(BridgeKit.Find(BridgeKit.Role.Wall, 10).Select(i => (ushort)i));
            int swapped = 0, kept = 0;
            foreach (ushort id in ids.Distinct())
            {
                ushort t = CgiBrushes.TurnedId(id, 1);
                bool good = t != id && BridgeKit.Orientation(t) == -BridgeKit.Orientation(id);
                c.Line($"{Item(id)} → {(t == id ? "bleibt" : Item(t))}{(t != id && !good ? "  ⚠ Richtung passt nicht" : string.Empty)}");
                if (t != id)
                {
                    swapped++;
                }
                else
                {
                    kept++;
                }
            }
            MapData m = Map(32, 32, 3, 0);
            ushort dirId = ids.FirstOrDefault(i => BridgeKit.Orientation(i) > 0);
            if (dirId != 0)
            {
                var wall = new List<StaticChange>();
                for (int x = 8; x < 16; x++)
                {
                    wall.Add(MapData.Add(new StaticItem { Id = dirId, X = x, Y = 12 }));
                }
                m.Apply(wall);
                c.Picture($"Vor dem Drehen: {dirId:x4} West-Ost", m, new Rectangle(4, 4, 20, 20));
                m.Apply(CgiBrushes.TransformStatics(m, m.AllStatics().ToList(), 1, false, false));
                c.Line("Nach Drehen: " + string.Join(", ", m.AllStatics().Select(s => $"{s.Id:x4} ({s.X},{s.Y})")));
                c.Picture("Nach Drehen 90°", m, new Rectangle(4, 4, 20, 20));
                c.Check(m.AllStatics().All(s => s.Id != dirId), $"nach dem Drehen ist kein {dirId:x4} mehr übrig (getauscht)");
            }
            if (swapped == 0)
            {
                c.Warn("Kein Teil wurde getauscht.");
            }
            c.Summary = $"{swapped} getauscht, {kept} bleiben";
        }

        private static void WallPath(Ctx c, CgiBrushSettings b)
        {
            WallSet set = b.WallSets.FirstOrDefault(x => x.Name == b.WallSet);
            if (set != null)
            {
                c.Line($"Dein Wand-Set '{set.Name}': X '{set.AlongX}', Y '{set.AlongY}', Pfosten '{set.Post}', Ecke '{set.Corner}'");
            }
            bool Bad(string ids) => HexList.Parse(ids).Length == 0 || HexList.Parse(ids).Any(i => i <= 2);
            if (set == null || Bad(set.AlongX) || Bad(set.AlongY))
            {
                if (set != null)
                {
                    c.Warn($"Dein Wand-Set '{set.Name}' ist unvollständig (X oder Y fehlt oder ist 0000–0002 = nodraw) – Knopf 'Wand-Set aus TileData' beim Werkzeug Wand.");
                }
                WallSet auto = ClientLoaded() ? BridgeKit.AutoWall("stone") ?? BridgeKit.AutoWall() : null;
                set = auto ?? new WallSet { Name = "Test", AlongX = "7201", AlongY = "7202", Post = "7203", Corner = "7204" };
                c.Line(auto != null ? $"Test mit Wand-Set aus der TileData: {auto.Name} X {auto.AlongX} Y {auto.AlongY} Pfosten {auto.Post}" : "Test mit Platzhaltern.");
            }
            MapData m = Map(48, 48, 3, 0);
            var pts = new List<Point> { new Point(5, 5), new Point(20, 5), new Point(20, 20), new Point(10, 25) };
            List<StaticChange> ch = CgiBrushes.WallPath(m, pts, set, new WallSettings { Thickness = 2, PostsAtEnds = true }, new Random(1));
            m.Apply(ch);
            var st = m.AllStatics().ToList();
            c.Line($"{st.Count} Teile, doppelt: {st.Count - st.Select(s => (s.X, s.Y, s.Z)).Distinct().Count()}");
            foreach (IGrouping<ushort, StaticItem> g in st.GroupBy(s => s.Id))
            {
                c.Line($"   {g.Count()} × {Item(g.Key)}");
            }
            var sb = new StringBuilder();
            for (int y = 3; y < 28; y++)
            {
                for (int x = 3; x < 24; x++)
                {
                    StaticItem s = m.StaticsAt(x, y).FirstOrDefault();
                    sb.Append(s == null ? '.' : HexList.Parse(set.AlongX).Contains(s.Id) ? '-' : HexList.Parse(set.AlongY).Contains(s.Id) ? '|' :
                        HexList.Parse(set.Corner).Contains(s.Id) ? '+' : 'o');
                }
                sb.AppendLine();
            }
            c.Line("Plan (- W-O, | N-S, + Ecke, o Pfosten):");
            c.Line(sb.ToString());
            c.Check(st.Count == st.Select(s => (s.X, s.Y, s.Z)).Distinct().Count(), "kein Teil doppelt");
            c.Picture("Wand Punkt zu Punkt, Dicke 2", m, new Rectangle(3, 3, 22, 25));
        }

        private static void River(Ctx c, CgiBrushSettings b)
        {
            foreach ((int w0, int w1, int bed) in new[] { (1, 1, 0), (3, 6, 0), (4, 4, 8) })
            {
                MapData m = Map(96, 64, 3, 10);
                var cs = new CourseSettings { Width = w0, EndWidth = w1, Meander = 20 };
                var rs = new RiverSettings { BedDepth = bed, BedTiles = bed > 0 ? "0016" : string.Empty };
                var core = new HashSet<int>();
                m.BeginLand();
                foreach ((float x, float y, float t) in Builders.Course(new PointF(5, 32), new PointF(90, 32), cs, new Random(3)))
                {
                    CgiBrushes.River(m, (int)x, (int)y, cs.WidthAt(t, 1) / 2.0, 5, rs, (a, d) => 10, new Random(1), core);
                }
                int water = m.Tiles.Count(v => v >= 0xA8 && v <= 0xAB || v == 0x16);
                // like transitions that turn all water into shore tiles
                for (int i = 0; i < m.Tiles.Length; i++)
                {
                    if (m.Tiles[i] >= 0xA8 && m.Tiles[i] <= 0xAB || m.Tiles[i] == 0x16)
                    {
                        m.SetLand(i % m.Width, i / m.Width, 0x71, m.Z[i]);
                    }
                }
                List<StaticChange> ws = CgiBrushes.RestoreWater(m, core, rs, new Random(1));
                m.ApplyStatics(ws);
                m.EndLand();
                int after = m.Tiles.Count(v => v >= 0xA8 && v <= 0xAB || v == 0x16);
                int x0 = 12, x1 = 85;
                string Cross(int x) => string.Concat(Enumerable.Range(20, 24).Select(y => m.Tiles[y * m.Width + x] == 0x71 ? '.' : m.Tiles[y * m.Width + x] == 3 ? ' ' : '~'));
                c.Line($"Breite {w0}→{w1}, Grund {bed}: Wasser {water}, nach Übergängen {after}, Wasser-Statics {ws.Count}{(ws.Count > 0 ? $" auf z {ws[0].After.Z}" : string.Empty)}");
                c.Line($"   Querschnitt Anfang [{Cross(x0)}]  Ende [{Cross(x1)}]");
                c.Check(after > 0, $"Mitte bleibt Wasser (Breite {w0}→{w1})");
                c.Picture($"Fluss {w0}→{w1}" + (bed > 0 ? $", Grund {bed} + Wasser-Statics" : string.Empty), m, new Rectangle(2, 18, 92, 28));
                if (bed > 0)
                {
                    c.Check(ws.Count > 0 && ws[0].After.Z == 5, "Grund wie Küste: Wasser-Statics auf dem Spiegel (z 5)");
                }
            }
        }

        private static void Coast(Ctx c)
        {
            // a ragged coast: land 0 left, sea right, with teeth
            MapData m = Map(64, 64, 0x0003, 0);
            var rnd = new Random(5);
            for (int y = 0; y < 64; y++)
            {
                int edge = 30 + (y % 3 == 0 ? rnd.Next(-3, 4) : 0);
                for (int x = edge; x < 64; x++)
                {
                    m.Tiles[y * 64 + x] = 0x00A8;
                    m.Z[y * 64 + x] = -15;
                }
                m.Z[y * 64 + edge - 1] = -3;
            }
            var coast = new CoastSettings();
            var o = new CoastSmoothSettings();
            var area = new Rectangle(0, 0, 64, 64);
            int before = CoastSmoother.FindErrors(m, area, coast, o).Count;
            c.Picture("Küste vorher", m, new Rectangle(16, 16, 30, 30));
            CoastEdit e = CoastSmoother.Smooth(m, area, coast, o, new Random(1));
            e.ApplyTo(m);
            int after = CoastSmoother.FindErrors(m, area, coast, o).Count;
            c.Line($"Zacken vorher {before}, nachher {after}, geänderte Land-Tiles {e.Land.Count}, Statics {e.Statics.Count}");
            c.Check(after < before || before == 0, "weniger Zacken nach dem Glätten");
            c.Picture("Küste nachher", m, new Rectangle(16, 16, 30, 30));
        }

        private static void Noise(Ctx c)
        {
            foreach (NoisePattern p in Enum.GetValues(typeof(NoisePattern)))
            {
                var o = new NoiseSettings { Pattern = p, Scale = 6, Detail = 2, Amplitude = 4 };
                var v = Enumerable.Range(0, 900).Select(i => CgiBrushes.NoiseValue(i % 30, i / 30, o)).ToList();
                c.Line($"{p}: min {v.Min():0.00} max {v.Max():0.00} Mittel {v.Average():0.00}");
                c.Check(v.Min() >= -1.001 && v.Max() <= 1.001, $"{p} bleibt in −1 … 1");
            }
        }

        private static void Dungeon(Ctx c, CgiBrushSettings b)
        {
            MapData m = Map(96, 80, 3, 0);
            var area = new Rectangle(8, 8, 72, 56);
            var o = new DungeonSettings { Layout = DungeonLayout.Grid, RoomMin = 6, RoomMax = 14, DoorWidth = 2 };
            var plans = new List<DungeonPlan> { Builders.Plan(area.Width, area.Height, o, new Random(4)), Builders.Plan(area.Width, area.Height, o, new Random(9)) };
            DungeonLook look = Workshop.Classic();
            look.FurniturePercent = 20;
            look.Furniture = new List<string> { "0b2c@0", "0b5d@0" };
            m.BeginLand();
            (List<StaticChange> s0, List<LandChange> l0) = Workshop.Clear(m, area, true, true, 0x0002);
            m.ApplyStatics(s0);
            foreach (LandChange l in l0)
            {
                m.SetLand(l.Index % m.Width, l.Index / m.Width, l.NewId, l.NewZ);
            }
            m.ApplyStatics(DungeonStyleTools.Build(m, area, plans[1], look, -20, true, false, false, new Random(2)));
            m.ApplyStatics(DungeonStyleTools.Build(m, area, plans[0], look, 0, false, true, false, new Random(2)));
            StairSet set = ChosenStairs(b, out bool fake);
            int stairs = Workshop.ConnectFloors(m, area.Location, plans, 0, 20, true, set, out List<Point> places);
            m.EndLand();
            var st = m.AllStatics().ToList();
            c.Line($"Räume oben {plans[0].Rooms.Count}, unten {plans[1].Rooms.Count}; Statics {st.Count}, oben (z ≥ 0) {st.Count(s => s.Z >= 0)}, unten {st.Count(s => s.Z < 0)}");
            c.Line($"Einrichtung gesetzt: {st.Count(s => s.Id == 0x0b2c || s.Id == 0x0b5d)}");
            c.Line($"Treppen zwischen den Etagen: {stairs} {(fake ? "(Platzhalter-Treppe)" : $"(Set '{set.Name}')")} bei " + string.Join(", ", places.Select(p => $"({p.X}, {p.Y})")));
            c.Check(plans[0].Rooms.Count > 0 && plans[1].Rooms.Count > 0, "beide Etagen haben Räume");
            c.Picture("Dungeon (obere Etage)", m, area);
            if (places.Count > 0)
            {
                c.Picture("Treppe zwischen den Etagen (Ausschnitt)", m, new Rectangle(places[0].X - 4, places[0].Y - 5, 14, 10));
            }
            c.Check(stairs == 1, "eine Treppe verbindet die Etagen");
            foreach (Point p in places)
            {
                c.Line($"   Loch oben: Land {m.LandId(p.X, p.Y):x4} z {m.LandZ(p.X, p.Y)}, Statics dort: " + string.Join(" ", m.StaticsAt(p.X, p.Y).Select(s => $"{s.Id:x4}@{s.Z}")));
                c.Check(m.LandId(p.X, p.Y) == 0x0002, "über der Treppe ist ein Loch (NoDraw)");
            }
        }

        private static void Piece(Ctx c)
        {
            MapData m = Map(48, 48, 3, 5);
            m.Apply(new List<StaticChange> { MapData.Add(new StaticItem { Id = 0x0241, X = 12, Y = 12, Z = 5 }), MapData.Add(new StaticItem { Id = 0x0242, X = 14, Y = 13, Z = 25 }) });
            WorkshopPiece p = WorkshopPiece.From(m, new Rectangle(10, 10, 8, 8), "Test", "Test");
            string json = JsonSerializer.Serialize(p);
            WorkshopPiece back = JsonSerializer.Deserialize<WorkshopPiece>(json);
            MapData m2 = Map(48, 48, 3, -20);
            m2.BeginLand();
            back.WriteTo(m2, new Point(20, 20), -20);
            m2.EndLand();
            var st = m2.AllStatics().OrderBy(s => s.Z).ToList();
            c.Line($"Vorlage {p.Width}×{p.Height}, Boden {p.Ground}, {p.Statics.Count} Statics, JSON {json.Length} Zeichen");
            c.Line("Eingesetzt auf z −20: " + string.Join(", ", st.Select(s => $"{s.Id:x4} ({s.X},{s.Y},{s.Z})")));
            c.Check(st.Count == 2 && st[0].Z == -20 && st[1].Z == 0, "Höhen relativ zum Boden bleiben (5 → −20, 25 → 0)");
        }

        private static void Categories(Ctx c)
        {
            if (!ClientLoaded())
            {
                c.Warn("TileData nicht geladen.");
                return;
            }
            int max = Math.Min(TileData.ItemTable.Length - 1, UoArtCache.MaxItemId);
            foreach (string cat in ArtCategories.StaticAuto)
            {
                var hits = Enumerable.Range(1, max).Where(id => ArtCategories.StaticFits(id, cat)).ToList();
                c.Line($"◆ {cat}: {hits.Count}  z. B. " + string.Join(", ", hits.Take(5).Select(i => $"{i:x4} {UoArtCache.ItemName(i)}")));
            }
            foreach (string cat in ArtCategories.LandAuto)
            {
                var hits = Enumerable.Range(0, Math.Min(TileData.LandTable.Length, 0x4000)).Where(id => ArtCategories.LandFits(id, cat)).ToList();
                c.Line($"◆ Land {cat}: {hits.Count}  z. B. " + string.Join(", ", hits.Take(5).Select(i => $"{i:x4} {UoArtCache.LandName(i)}")));
            }
            c.Line("Eigene Kategorien: " + string.Join(", ", ArtCategories.Own.Select(o => $"{o.Kind} '{o.Name}' ({o.IdList().Count})")));
        }

        private static void SettingsJson(Ctx c, CgiBrushSettings b)
        {
            string a = JsonSerializer.Serialize(b);
            CgiBrushSettings back = JsonSerializer.Deserialize<CgiBrushSettings>(a);
            back.Normalize();
            string a2 = JsonSerializer.Serialize(back);
            c.Line($"JSON {a.Length} Zeichen; Wand-Sets {b.WallSets.Count}, Treppen-Sets {b.StairSets.Count}, Brücken-Sets {b.BridgeSets.Count}, Weg-Vorlagen {b.RoadPresets.Count}, Varianten {b.VariantSets.Count}, Lauf-Arten {b.CoursePresets.Count}, Favoriten-Hues {b.FavoriteHues.Count}");
            c.Line($"Gewählt: Wand '{b.WallSet}', Treppe '{b.StairSet}', Brücken-Plan '{b.BridgeTemplate}', Streu '{b.ScatterSet}'");
            c.Check(a == a2, "Speichern und Laden ergibt dieselben Einstellungen");
        }
    }
}

/***************************************************************************
 *
 *  * Dragon Map Studio – builders (pure logic): dungeon generator, cellar under the map, bridge, natural course
 *  *
 *  *  Dungeon: random rooms that do not overlap, connected by a minimum spanning tree of their centres plus some
 *  *  extra connections (loops), L-shaped corridors – the classic room-and-corridor generator of roguelikes.
 *  *  Built as cave (land: floor tiles, walls = raised rock), as statics (floor + wall pieces by direction) or
 *  *  under the map (land becomes the invisible NoDraw tile 0002 at the dungeon floor, statics below).
 *  *
 *  *  Cellar: the land of the area becomes NoDraw at ground − depth, floor and walls as statics, stairs from the
 *  *  ground down (north / east / south / west), optional floor above (house floor).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public enum Compass
    {
        [Description("Nord")]
        North,

        [Description("Ost")]
        East,

        [Description("Süd")]
        South,

        [Description("West")]
        West
    }

    public enum DungeonLayout
    {
        [Description("Räume + Gänge (verstreut)")]
        Corridors,

        [Description("Gitter: Räume dicht an dicht mit Türen (wie Deceit)")]
        Grid
    }

    public enum DungeonStyle
    {
        [Description("Bergwall (Land: Boden-Tiles, Wände = Fels) – nur Werkzeug Bergwall")]
        Cave,

        [Description("Gebaut (Boden-Textur + Wand-Statics)")]
        Built,

        [Description("Unter der Karte (NoDraw-Land, Statics darunter)")]
        UnderMap
    }

    /// <summary>Stairs for every direction you walk down (hex, several = random per step).</summary>
    public sealed class StairSet
    {
        public string Name { get; set; } = "Treppe";

        [Description("Stufe, wenn man nach Norden hinuntergeht (hex).")]
        public string DownNorth { get; set; } = string.Empty;

        [Description("Stufe, wenn man nach Osten hinuntergeht (hex).")]
        public string DownEast { get; set; } = string.Empty;

        [Description("Stufe, wenn man nach Süden hinuntergeht (hex).")]
        public string DownSouth { get; set; } = string.Empty;

        [Description("Stufe, wenn man nach Westen hinuntergeht (hex).")]
        public string DownWest { get; set; } = string.Empty;

        public string For(Compass c) => c switch
        {
            Compass.North => DownNorth,
            Compass.East => DownEast,
            Compass.South => DownSouth,
            _ => DownWest
        };

        public override string ToString() => Name;
    }

    /// <summary>Bridge pieces: deck and railing for a bridge from west to east (X) or north to south (Y), posts.</summary>
    public sealed class BridgeSet
    {
        public string Name { get; set; } = "Holzbrücke";

        [Description("Bodenbretter einer Brücke West-Ost (hex, mehrere = zufällig).")]
        public string DeckX { get; set; } = string.Empty;

        [Description("Bodenbretter einer Brücke Nord-Süd (hex).")]
        public string DeckY { get; set; } = string.Empty;

        [Description("Geländer an den Seiten einer Brücke West-Ost (hex), leer = keins.")]
        public string RailX { get; set; } = string.Empty;

        [Description("Geländer an den Seiten einer Brücke Nord-Süd (hex), leer = keins.")]
        public string RailY { get; set; } = string.Empty;

        [Description("Pfosten an den Enden des Geländers (hex), leer = keine.")]
        public string Post { get; set; } = string.Empty;

        [Description("Pfeiler unter der Brücke bis zum Wasser (hex), leer = keine.")]
        public string Pillar { get; set; } = string.Empty;

        public override string ToString() => Name;
    }

    public sealed class BridgeSettings
    {
        [Category("Brücke"), DisplayName("Breite (Tiles)")]
        public int Width { get; set; } = 3;

        [Category("Brücke"), DisplayName("Z +"), Description("Über der höheren Seite der beiden Enden.")]
        public int ZOffset { get; set; }

        [Category("Brücke"), DisplayName("Bogen (Z)"), Description("So viel höher liegt die Mitte (0 = flach).")]
        public int Arch { get; set; } = 4;

        [Category("Brücke"), DisplayName("Geländer")]
        public bool Railings { get; set; } = true;

        [Category("Brücke"), DisplayName("Treppen an den Enden"), Description("Liegt die Brücke höher als das Ufer, kommen Stufen davor (Treppe aus dem Brücken-Plan).")]
        public bool Stairs { get; set; } = true;

        [Category("Brücke"), DisplayName("Pfeiler alle … Tiles"), Description("Für Sets ohne Plan: Stütze unter der Brücke (Pfeiler-ID im Brücken-Designer).")]
        public int PillarEvery { get; set; } = 6;
    }

    public enum CellarLand
    {
        [Description("Nur über der Treppe ein Loch (NoDraw auf Kartenhöhe)")]
        HoleOverStairs,

        [Description("Ganze Fläche NoDraw auf Kartenhöhe")]
        WholeAreaNoDraw,

        [Description("Land bleibt (Löcher selbst setzen)")]
        Keep,

        [Description("Ganze Fläche NoDraw auf Kellerhöhe (alt)")]
        LowerToCellar
    }

    public sealed class CellarSettings
    {
        [Category("Keller"), DisplayName("Land / Loch"), Description("Was mit dem Land passiert. Das Loch liegt immer auf der Höhe der Karte (keine Löcher in die Tiefe) – meist nur über der Treppe (ca. 3 Felder).")]
        public CellarLand Land { get; set; } = CellarLand.HoleOverStairs;

        [Category("Keller"), DisplayName("Tiefe (Z)"), Description("So tief unter dem Boden liegt der Kellerboden (UO: 20 = ein Stockwerk).")]
        public int Depth { get; set; } = 20;

        [Category("Keller"), DisplayName("Stufen-Höhe"), Description("Z je Treppenstufe (UO-Treppen: 5).")]
        public int StepHeight { get; set; } = 5;

        [Category("Keller"), DisplayName("Treppe hinunter nach"), Description("In diese Richtung geht man die Treppe hinunter. Die Treppe liegt an der gegenüberliegenden Innenkante.")]
        public Compass Direction { get; set; } = Compass.North;

        [Category("Keller"), DisplayName("Treppen-Breite")]
        public int StairWidth { get; set; } = 2;

        [Category("Keller"), DisplayName("Kellerboden (hex)"), Description("Statics des Kellerbodens (mehrere = zufällig), leer = keiner.")]
        public string FloorIds { get; set; } = string.Empty;

        [Category("Keller"), DisplayName("Boden oben (hex)"), Description("Statics des Bodens darüber auf Bodenhöhe (Hausboden), außer über der Treppe. Leer = keiner (z. B. wenn ein Haus darauf kommt).")]
        public string UpperFloorIds { get; set; } = string.Empty;

        [Category("Keller"), DisplayName("Wände"), Description("Wand-Set (Reiter Pinsel → Wand) für die Kellerwände am Rand, leer = keine.")]
        public string WallSet { get; set; } = string.Empty;
    }

    public sealed class DungeonSettings
    {
        [Category("Dungeon"), DisplayName("Art")]
        public DungeonStyle Style { get; set; } = DungeonStyle.Built;

        [Category("Dungeon"), DisplayName("Gelernter Stil"), Description("Name eines Dungeon-Stils, der aus einer echten Karte gelernt wurde (Reiter Pinsel → Dungeon → 'Stil aus Bereich lernen'). Leer = Wand-Set + Boden-Statics unten.")]
        public string Look { get; set; } = string.Empty;

        [Category("Dungeon"), DisplayName("Leere mit Void füllen"), Description("Das ganze Rechteck außerhalb der Gänge wird Void-Land (schwarz) wie in den Original-Dungeons.")]
        public bool FillVoid { get; set; } = true;

        [Category("Dungeon"), DisplayName("Grundriss"), Description("Räume + Gänge = Räume verstreut, mit Gängen verbunden. Gitter = der Bereich wird in Räume geteilt (gemeinsame Wände, Türen) wie in Deceit.")]
        public DungeonLayout Layout { get; set; } = DungeonLayout.Grid;

        [Category("Dungeon"), DisplayName("Wand-Dicke (Gitter)"), Description("1 = eine Wand-Zeile auf Boden-Land zwischen den Räumen (wie das Original), 2 = zwei Zeilen Leere (jeder Raum hat eigene Wände).")]
        public int WallThickness { get; set; } = 1;

        [Category("Dungeon"), DisplayName("Tür-Breite")]
        public int DoorWidth { get; set; } = 2;

        [Category("Dungeon"), DisplayName("Räume"), Description("Grundriss Räume + Gänge: so viele Räume.")]
        public int Rooms { get; set; } = 8;

        [Category("Dungeon"), DisplayName("Raum kleinste Seite")]
        public int RoomMin { get; set; } = 6;

        [Category("Dungeon"), DisplayName("Raum größte Seite")]
        public int RoomMax { get; set; } = 16;

        [Category("Dungeon"), DisplayName("Gang-Breite")]
        public int CorridorWidth { get; set; } = 2;

        [Category("Dungeon"), DisplayName("Schleifen (%)"), Description("Zusätzliche Verbindungen zwischen nahen Räumen (0 = nur ein Weg zu jedem Raum).")]
        public int Loops { get; set; } = 15;

        [Category("Dungeon"), DisplayName("Höhlig (%)"), Description("Ränder der Räume unregelmäßig wie eine Höhle (0 = gerade Wände wie in gebauten Dungeons; der Bergwall nimmt mindestens 30).")]
        public int Rough { get; set; }

        [Category("Dungeon"), DisplayName("Zufalls-Zahl"), Description("Gleiche Zahl = gleicher Dungeon. 0 = jedes Mal neu.")]
        public int Seed { get; set; }

        [Category("Höhen"), DisplayName("Boden-Z fest"), Description("An = 'Boden-Z' benutzen. Aus = tiefste Bodenhöhe im Bereich (Höhle im Berg) bzw. Boden − Tiefe (unter der Karte).")]
        public bool FixedFloor { get; set; }

        [Category("Höhen"), DisplayName("Boden-Z")]
        public int FloorZ { get; set; }

        [Category("Höhen"), DisplayName("Tiefe unter der Karte"), Description("Art 'Unter der Karte': so tief unter dem Boden liegt der Dungeon-Boden.")]
        public int Depth { get; set; } = 20;

        [Category("Höhen"), DisplayName("Wand-Höhe (Höhle)"), Description("Art Höhle: so hoch über dem Boden werden die Fels-Wände (mindestens).")]
        public int WallHeight { get; set; } = 20;

        [Category("Tiles"), DisplayName("Boden-Land (hex)"), Description("Art Höhle: Land-Tiles des Bodens. Leer = die Höhlen-Tiles aus dem Reiter Höhle / Weg.")]
        public string FloorLand { get; set; } = string.Empty;

        [Category("Tiles"), DisplayName("Wand-Land (hex)"), Description("Art Höhle: Land-Tiles der Wände. Leer = Berg-Tiles.")]
        public string WallLand { get; set; } = string.Empty;

        [Category("Tiles"), DisplayName("Boden-Statics (hex)"), Description("Art Gebaut / Unter der Karte: Statics des Bodens (mehrere = zufällig).")]
        public string FloorStatics { get; set; } = string.Empty;

        [Category("Tiles"), DisplayName("Wände (Wand-Set)"), Description("Art Gebaut / Unter der Karte: Wand-Set aus dem Reiter Pinsel → Wand.")]
        public string WallSet { get; set; } = string.Empty;

        [Category("Tiles"), DisplayName("Alte Statics entfernen"), Description("Statics im Dungeon-Bereich vorher entfernen.")]
        public bool ClearStatics { get; set; } = true;

        [Category("Karten-Editor"), DisplayName("Fels ganz füllen"), Description("Karten-Editor: der ganze Rest des Rechtecks wird Farbe 2 (sonst nur ein Ring Wand um Räume und Gänge).")]
        public bool FillRock2D { get; set; }
    }

    public enum CourseKind
    {
        [Description("Fluss (Wasser, Ufer)")]
        River,

        [Description("Berg-Kamm (Fels, Hang)")]
        Mountain,

        [Description("Weg / Straße")]
        Road,

        [Description("Eigene Farben")]
        Own,

        [Description("Muster-Stempel (Land + Statics)")]
        Stamp
    }

    /// <summary>
    /// Kind of the natural course in the map editor: which colours (core, edge, outer edge) and how wide.
    /// Colour −1 = colour 1 of the editor; edge −1 = none.
    /// </summary>
    public sealed class CoursePreset
    {
        [Category("1 Art"), DisplayName("Name")]
        public string Name { get; set; } = "Fluss";

        [Category("1 Art"), DisplayName("Art")]
        public CourseKind Kind { get; set; } = CourseKind.River;

        [Category("2 Farben"), DisplayName("Kern-Farbe"), Description("Index der Farbe in der Mitte (Wasser, Fels, Weg). −1 = Farbe 1 des Editors. Knopf 'Farbe 1 → Kern'.")]
        public int Core { get; set; } = -1;

        [Category("2 Farben"), DisplayName("Rand-Farbe"), Description("Farbe am Rand (Ufer-Sand, Hang, Bankett). −1 = kein Rand.")]
        public int Edge { get; set; } = -1;

        [Category("2 Farben"), DisplayName("Rand-Breite (Tiles)"), Description("So breit ist der Rand auf jeder Seite.")]
        public int EdgeWidth { get; set; } = 2;

        [Category("2 Farben"), DisplayName("Außen-Farbe"), Description("Zweiter, äußerer Rand (z. B. Gras-Höhe 5 um den Berg-Hang). −1 = keiner.")]
        public int Edge2 { get; set; } = -1;

        [Category("2 Farben"), DisplayName("Außen-Breite (Tiles)")]
        public int Edge2Width { get; set; } = 2;

        [Category("3 Lauf"), DisplayName("Breite am Anfang (Tiles)"), Description("Breite des Kerns an der Quelle. 0 = Pinselgröße.")]
        public int Width { get; set; } = 3;

        [Category("3 Lauf"), DisplayName("Breite am Ende (Tiles)"), Description("0 = wie am Anfang.")]
        public int EndWidth { get; set; } = 6;

        [Category("3 Lauf"), DisplayName("Schlängeln (%)"), Description("0 = gerade, 25 = Fluss, 10 = Weg.")]
        public int Meander { get; set; } = 25;

        [Category("3 Lauf"), DisplayName("Bögen je 20 Tiles")]
        public double Bends { get; set; } = 1.5;

        public override string ToString() => Name;

        public CourseSettings ToCourse(int seed) => new CourseSettings { Width = Width, EndWidth = EndWidth, Widen = false, Meander = Meander, Bends = Bends, Seed = seed };

        /// <summary>The built-in kinds (colours are found by name in the palette when the editor opens them).</summary>
        public static List<CoursePreset> Defaults() => new List<CoursePreset>
        {
            new CoursePreset { Name = "Fluss", Kind = CourseKind.River, Width = 3, EndWidth = 7, Meander = 25, EdgeWidth = 1 },
            new CoursePreset { Name = "Bach", Kind = CourseKind.River, Width = 1, EndWidth = 2, Meander = 30, Bends = 2.5, EdgeWidth = 1 },
            new CoursePreset { Name = "Berg-Kamm", Kind = CourseKind.Mountain, Width = 2, EndWidth = 2, Meander = 15, EdgeWidth = 2, Edge2Width = 2 },
            new CoursePreset { Name = "Weg", Kind = CourseKind.Road, Width = 2, EndWidth = 2, Meander = 10, Bends = 1, EdgeWidth = 0 },
            new CoursePreset { Name = "Eigene", Kind = CourseKind.Own, Width = 3, EndWidth = 3, Meander = 20 },
            new CoursePreset { Name = "Stempel", Kind = CourseKind.Stamp, Width = 3, EndWidth = 3, Meander = 15 }
        };
    }

    public sealed class CourseSettings
    {
        [Category("Lauf"), DisplayName("Schlängeln (%)"), Description("Wie weit der Lauf seitlich ausschwingt (0 = gerade, 25 = Fluss, 10 = Weg).")]
        public int Meander { get; set; } = 25;

        [Category("Lauf"), DisplayName("Bögen je 20 Tiles")]
        public double Bends { get; set; } = 1.5;

        [Category("Lauf"), DisplayName("Breite am Anfang (Tiles)"), Description("Breite des Wassers an der Quelle. 0 = Pinselgröße.")]
        public int Width { get; set; } = 3;

        [Category("Lauf"), DisplayName("Breite am Ende (Tiles)"), Description("Breite an der Mündung. 0 = wie am Anfang (oder automatisch breiter, wenn 'Breite wächst' an ist).")]
        public int EndWidth { get; set; } = 5;

        [Category("Lauf"), DisplayName("Breite wächst"), Description("Nur wenn 'Breite am Ende' 0 ist: schmal am Anfang (Quelle), breiter zum Ende (Mündung).")]
        public bool Widen { get; set; } = true;

        /// <summary>Width in tiles at 0..1 along the course (brush = width when nothing is set).</summary>
        public double WidthAt(double t, int brushWidth)
        {
            double w0 = Width > 0 ? Width : Math.Max(1, brushWidth);
            double w1 = EndWidth > 0 ? EndWidth : Widen ? w0 * 1.6 : w0;
            return w0 + (w1 - w0) * Math.Clamp(t, 0, 1);
        }

        [Category("Lauf"), DisplayName("Zufalls-Zahl"), Description("0 = jedes Mal ein anderer Lauf.")]
        public int Seed { get; set; }
    }

    /// <summary>Cells of a generated dungeon.</summary>
    public sealed class DungeonPlan
    {
        public const byte Solid = 0;
        public const byte Room = 1;
        public const byte Corridor = 2;

        /// <summary>Wall line on floor land between two rooms (grid layout, wall thickness 1).</summary>
        public const byte Inner = 3;

        public int W;
        public int H;
        public byte[] Cells;
        public List<Rectangle> Rooms = new List<Rectangle>();

        /// <summary>Open floor (room or corridor) – inner wall lines are not.</summary>
        public bool Floor(int x, int y) => x >= 0 && y >= 0 && x < W && y < H && (Cells[y * W + x] == Room || Cells[y * W + x] == Corridor);

        /// <summary>Floor land: open floor or an inner wall line (the original puts those walls on floor land).</summary>
        public bool LandFloor(int x, int y) => x >= 0 && y >= 0 && x < W && y < H && Cells[y * W + x] != Solid;

        public bool IsInner(int x, int y) => x >= 0 && y >= 0 && x < W && y < H && Cells[y * W + x] == Inner;

        /// <summary>Solid cell next to a floor cell (8 neighbours).</summary>
        public bool Wall(int x, int y)
        {
            if (Floor(x, y))
            {
                return false;
            }
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if ((dx != 0 || dy != 0) && Floor(x + dx, y + dy))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public int FloorCount => Cells.Count(c => c == Room || c == Corridor);
    }

    public static class Builders
    {
        // ------------------------------------------------------------------ dungeon generator

        public static DungeonPlan Plan(int w, int h, DungeonSettings o, Random rnd)
        {
            if (o.Layout == DungeonLayout.Grid)
            {
                return PlanGrid(w, h, o, rnd);
            }
            var plan = new DungeonPlan { W = w, H = h, Cells = new byte[w * h] };
            int min = Math.Max(2, o.RoomMin), max = Math.Max(min, o.RoomMax);
            // rooms: random places, no overlap (one cell of rock between them), at least one cell from the border
            for (int attempt = 0; attempt < Math.Max(1, o.Rooms) * 40 && plan.Rooms.Count < Math.Max(1, o.Rooms); attempt++)
            {
                int rw = rnd.Next(min, max + 1), rh = rnd.Next(min, max + 1);
                if (rw > w - 2 || rh > h - 2)
                {
                    rw = Math.Min(rw, w - 2);
                    rh = Math.Min(rh, h - 2);
                    if (rw < 2 || rh < 2)
                    {
                        break;
                    }
                }
                var r = new Rectangle(rnd.Next(1, w - rw), rnd.Next(1, h - rh), rw, rh);
                if (plan.Rooms.Any(o2 => Rectangle.Inflate(o2, 2, 2).IntersectsWith(r)))
                {
                    continue;
                }
                plan.Rooms.Add(r);
            }
            foreach (Rectangle r in plan.Rooms)
            {
                for (int y = r.Top; y < r.Bottom; y++)
                {
                    for (int x = r.Left; x < r.Right; x++)
                    {
                        // rough cave edges: the outermost ring loses some cells
                        bool edge = x == r.Left || y == r.Top || x == r.Right - 1 || y == r.Bottom - 1;
                        if (edge && rnd.Next(100) < o.Rough / 2)
                        {
                            continue;
                        }
                        plan.Cells[y * w + x] = DungeonPlan.Room;
                    }
                }
            }
            // connections: minimum spanning tree (Prim) over the room centres + some loops
            int n = plan.Rooms.Count;
            Point C(int i) => new Point(plan.Rooms[i].X + plan.Rooms[i].Width / 2, plan.Rooms[i].Y + plan.Rooms[i].Height / 2);
            double D(int a, int b) => Math.Sqrt(Math.Pow(C(a).X - C(b).X, 2) + Math.Pow(C(a).Y - C(b).Y, 2));
            var inTree = new HashSet<int> { 0 };
            var edges = new List<(int a, int b)>();
            while (n > 0 && inTree.Count < n)
            {
                (int a, int b, double d) best = (-1, -1, double.MaxValue);
                foreach (int a in inTree)
                {
                    for (int b = 0; b < n; b++)
                    {
                        if (!inTree.Contains(b) && D(a, b) < best.d)
                        {
                            best = (a, b, D(a, b));
                        }
                    }
                }
                inTree.Add(best.b);
                edges.Add((best.a, best.b));
            }
            for (int a = 0; a < n; a++)
            {
                // a loop to the nearest room that is not connected directly yet
                int b = Enumerable.Range(0, n).Where(i => i != a && !edges.Contains((a, i)) && !edges.Contains((i, a))).OrderBy(i => D(a, i)).DefaultIfEmpty(-1).First();
                if (b >= 0 && rnd.Next(100) < o.Loops)
                {
                    edges.Add((a, b));
                }
            }
            int cw = Math.Max(1, o.CorridorWidth);
            foreach ((int a, int b) in edges)
            {
                Point p = C(a), q = C(b);
                bool horizontalFirst = rnd.Next(2) == 0;
                Point corner = horizontalFirst ? new Point(q.X, p.Y) : new Point(p.X, q.Y);
                Carve(plan, p, corner, cw);
                Carve(plan, corner, q, cw);
            }
            return plan;
        }

        /// <summary>
        /// Grid layout like Deceit: the area is split again and again (binary space partition) into rooms; between two halves
        /// a wall line (thickness 1 on floor land, or 2 cells of void) with a door of the door width – every room is reachable.
        /// </summary>
        private static DungeonPlan PlanGrid(int w, int h, DungeonSettings o, Random rnd)
        {
            var plan = new DungeonPlan { W = w, H = h, Cells = new byte[w * h] };
            int min = Math.Max(2, o.RoomMin), max = Math.Max(min + 1, o.RoomMax), t = Math.Clamp(o.WallThickness, 1, 3), door = Math.Max(1, o.DoorWidth);
            byte line = t == 1 ? DungeonPlan.Inner : DungeonPlan.Solid;
            void Set(int x, int y, byte v)
            {
                if (x >= 1 && y >= 1 && x < w - 1 && y < h - 1)
                {
                    plan.Cells[y * w + x] = v;
                }
            }
            byte Get(int x, int y) => x < 0 || y < 0 || x >= w || y >= h ? DungeonPlan.Solid : plan.Cells[y * w + x];
            void Split(Rectangle r, int depth)
            {
                bool vertical = r.Width > r.Height || (r.Width == r.Height && rnd.Next(2) == 0); // vertical = wall line from north to south
                int size = vertical ? r.Width : r.Height;
                bool can = size >= 2 * min + t;
                bool want = size > max || (depth < 2 && rnd.Next(100) < 25 && size >= 2 * min + t);
                if (!can || !want)
                {
                    for (int y = r.Top; y < r.Bottom; y++)
                    {
                        for (int x = r.Left; x < r.Right; x++)
                        {
                            Set(x, y, DungeonPlan.Room);
                        }
                    }
                    plan.Rooms.Add(r);
                    return;
                }
                int p = rnd.Next(min, size - min - t + 1);
                Rectangle a, b;
                if (vertical)
                {
                    a = new Rectangle(r.X, r.Y, p, r.Height);
                    b = new Rectangle(r.X + p + t, r.Y, r.Width - p - t, r.Height);
                }
                else
                {
                    a = new Rectangle(r.X, r.Y, r.Width, p);
                    b = new Rectangle(r.X, r.Y + p + t, r.Width, r.Height - p - t);
                }
                Split(a, depth + 1);
                Split(b, depth + 1);
                // the wall line between the halves
                for (int k = 0; k < t; k++)
                {
                    for (int q = 0; q < (vertical ? r.Height : r.Width); q++)
                    {
                        if (vertical)
                        {
                            Set(r.X + p + k, r.Y + q, line);
                        }
                        else
                        {
                            Set(r.X + q, r.Y + p + k, line);
                        }
                    }
                }
                // doors: where both sides of the line are open floor (one, sometimes a second one = loop)
                var places = new List<int>();
                int len = vertical ? r.Height : r.Width;
                for (int q = 0; q + door <= len; q++)
                {
                    bool ok = true;
                    for (int d = 0; d < door && ok; d++)
                    {
                        int qq = q + d;
                        bool before = vertical ? Get(r.X + p - 1, r.Y + qq) == DungeonPlan.Room || Get(r.X + p - 1, r.Y + qq) == DungeonPlan.Corridor
                                               : Get(r.X + qq, r.Y + p - 1) == DungeonPlan.Room || Get(r.X + qq, r.Y + p - 1) == DungeonPlan.Corridor;
                        bool after = vertical ? Get(r.X + p + t, r.Y + qq) == DungeonPlan.Room || Get(r.X + p + t, r.Y + qq) == DungeonPlan.Corridor
                                              : Get(r.X + qq, r.Y + p + t) == DungeonPlan.Room || Get(r.X + qq, r.Y + p + t) == DungeonPlan.Corridor;
                        ok = before && after;
                    }
                    if (ok)
                    {
                        places.Add(q);
                    }
                }
                int doors = places.Count == 0 ? 0 : 1 + (rnd.Next(100) < o.Loops && places.Count > door * 3 ? 1 : 0);
                var used = new List<int>();
                for (int n = 0; n < doors; n++)
                {
                    int q = places[rnd.Next(places.Count)];
                    if (used.Any(u => Math.Abs(u - q) < door + 2))
                    {
                        continue;
                    }
                    used.Add(q);
                    for (int d = 0; d < door; d++)
                    {
                        for (int k = 0; k < t; k++)
                        {
                            if (vertical)
                            {
                                Set(r.X + p + k, r.Y + q + d, DungeonPlan.Corridor);
                            }
                            else
                            {
                                Set(r.X + q + d, r.Y + p + k, DungeonPlan.Corridor);
                            }
                        }
                    }
                }
            }
            Split(new Rectangle(1, 1, w - 2, h - 2), 0);
            return plan;
        }

        private static void Carve(DungeonPlan plan, Point a, Point b, int width)
        {
            int x0 = Math.Min(a.X, b.X), x1 = Math.Max(a.X, b.X), y0 = Math.Min(a.Y, b.Y), y1 = Math.Max(a.Y, b.Y);
            int lo = -(width - 1) / 2, hi = width / 2;
            for (int y = y0 + (a.X == b.X ? 0 : lo); y <= y1 + (a.X == b.X ? 0 : hi); y++)
            {
                for (int x = x0 + (a.Y == b.Y ? 0 : lo); x <= x1 + (a.Y == b.Y ? 0 : hi); x++)
                {
                    if (x >= 1 && y >= 1 && x < plan.W - 1 && y < plan.H - 1 && plan.Cells[y * plan.W + x] == DungeonPlan.Solid)
                    {
                        plan.Cells[y * plan.W + x] = DungeonPlan.Corridor;
                    }
                }
            }
        }

        /// <summary>Wall piece for a wall cell: along X when the floor is north / south of it, along Y when west / east, else a post.</summary>
        private static int WallKind(Func<int, int, bool> floor, int x, int y)
        {
            bool ns = floor(x, y - 1) || floor(x, y + 1);
            bool ew = floor(x - 1, y) || floor(x + 1, y);
            return ns && !ew ? 0 : ew && !ns ? 1 : 2;
        }

        private static ushort Pick(ushort[] ids, Random rnd) => ids.Length == 0 ? (ushort)0 : ids[rnd.Next(ids.Length)];

        /// <summary>Builds a dungeon plan into the map at the top left of the area (CGI view, inside BeginLand / EndLand). Returns the static changes.</summary>
        public static List<StaticChange> BuildDungeon(MapData map, Rectangle area, DungeonPlan plan, DungeonSettings o, WallSet walls, WallSettings wallOptions,
            ushort[] caveFloor, ushort[] rock, Random rnd)
        {
            var statics = new List<StaticChange>();
            bool Floor(int x, int y) => plan.Floor(x - area.X, y - area.Y);
            int minGround = int.MaxValue, maxGround = int.MinValue;
            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    if (map.InBounds(x, y))
                    {
                        minGround = Math.Min(minGround, map.LandZ(x, y));
                        maxGround = Math.Max(maxGround, map.LandZ(x, y));
                    }
                }
            }
            if (minGround == int.MaxValue)
            {
                return statics;
            }
            int floorZ = o.FixedFloor ? o.FloorZ : o.Style == DungeonStyle.UnderMap ? minGround - Math.Max(1, o.Depth) : minGround;
            ushort[] floorLand = HexList.Parse(o.FloorLand);
            if (floorLand.Length == 0)
            {
                floorLand = caveFloor ?? Array.Empty<ushort>();
            }
            ushort[] wallLand = HexList.Parse(o.WallLand);
            if (wallLand.Length == 0)
            {
                wallLand = rock ?? Array.Empty<ushort>();
            }
            ushort[] floorStatics = HexList.Parse(o.FloorStatics);
            ushort[][] pieces = walls == null ? null : new[] { HexList.Parse(walls.AlongX), HexList.Parse(walls.AlongY), HexList.Parse(walls.Post) };
            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    if (!map.InBounds(x, y))
                    {
                        continue;
                    }
                    bool floor = Floor(x, y), wall = !floor && plan.Wall(x - area.X, y - area.Y);
                    if (!floor && !wall)
                    {
                        continue; // rock / ground stays
                    }
                    if (o.ClearStatics)
                    {
                        statics.AddRange(map.StaticsAt(x, y).Select(MapData.Remove));
                    }
                    ushort id = map.LandId(x, y);
                    switch (o.Style)
                    {
                        case DungeonStyle.Cave:
                            if (floor)
                            {
                                map.SetLand(x, y, floorLand.Length > 0 ? Pick(floorLand, rnd) : id, floorZ);
                            }
                            else
                            {
                                int z = Math.Max(map.LandZ(x, y), floorZ + o.WallHeight);
                                map.SetLand(x, y, wallLand.Length > 0 ? Pick(wallLand, rnd) : id, z);
                            }
                            break;
                        default:
                            if (o.Style == DungeonStyle.UnderMap)
                            {
                                // the land above becomes invisible (NoDraw 0002) at the dungeon floor – the client draws the statics below
                                map.SetLand(x, y, 0x0002, floorZ);
                            }
                            else if (floor)
                            {
                                map.SetLand(x, y, id, floorZ);
                            }
                            if (floor && floorStatics.Length > 0)
                            {
                                statics.Add(MapData.Add(new StaticItem { Id = Pick(floorStatics, rnd), X = x, Y = y, Z = floorZ }));
                            }
                            if (wall && pieces != null)
                            {
                                ushort[] use = pieces[WallKind(Floor, x, y)];
                                if (use.Length == 0)
                                {
                                    use = pieces[0].Length > 0 ? pieces[0] : pieces[1].Length > 0 ? pieces[1] : pieces[2];
                                }
                                for (int l = 0; use.Length > 0 && l < Math.Max(1, wallOptions?.Levels ?? 1); l++)
                                {
                                    statics.Add(MapData.Add(new StaticItem { Id = Pick(use, rnd), X = x, Y = y, Z = Math.Clamp(floorZ + l * (wallOptions?.LevelHeight ?? 20), -128, 127) }));
                                }
                            }
                            break;
                    }
                }
            }
            return statics;
        }

        /// <summary>Colours of a dungeon plan in the colour map: floor = colour 1, walls (or all rock) = colour 2.</summary>
        public static void PaintDungeon(IndexedMap map, Rectangle area, DungeonPlan plan, ushort floor, ushort wall, bool fillRock)
        {
            for (int y = 0; y < plan.H; y++)
            {
                for (int x = 0; x < plan.W; x++)
                {
                    int mx = area.X + x, my = area.Y + y;
                    if (!map.InBounds(mx, my))
                    {
                        continue;
                    }
                    if (plan.Floor(x, y))
                    {
                        map.Pixels[my * map.Width + mx] = floor;
                    }
                    else if (fillRock || plan.Wall(x, y))
                    {
                        map.Pixels[my * map.Width + mx] = wall;
                    }
                }
            }
        }

        // ------------------------------------------------------------------ cellar

        /// <summary>
        /// Cellar under the area: land → NoDraw at ground − depth, floor, walls on the border, stairs, floor above.
        /// Call inside BeginLand / EndLand; returns the static changes.
        /// </summary>
        public static List<StaticChange> BuildCellar(MapData map, Rectangle area, CellarSettings o, WallSet walls, StairSet stairs, Random rnd)
        {
            var statics = new List<StaticChange>();
            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            if (area.Width < 3 || area.Height < 3)
            {
                return statics;
            }
            int ground = int.MinValue;
            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    ground = Math.Max(ground, map.LandZ(x, y));
                }
            }
            int cz = ground - Math.Max(1, o.Depth);
            int steps = Math.Max(1, (int)Math.Ceiling(o.Depth / (double)Math.Max(1, o.StepHeight)));
            var inner = Rectangle.Inflate(area, -1, -1);
            // the stairs lie at the inner edge opposite to the walking direction
            (int dx, int dy) = o.Direction switch
            {
                Compass.North => (0, -1),
                Compass.East => (1, 0),
                Compass.South => (0, 1),
                _ => (-1, 0)
            };
            var stairCells = new Dictionary<(int x, int y), int>();
            int width = Math.Max(1, o.StairWidth);
            for (int w = 0; w < width; w++)
            {
                for (int i = 1; i <= steps; i++)
                {
                    int x, y;
                    switch (o.Direction)
                    {
                        case Compass.North:
                            x = inner.Left + w;
                            y = inner.Bottom - i;
                            break;
                        case Compass.South:
                            x = inner.Left + w;
                            y = inner.Top + i - 1;
                            break;
                        case Compass.East:
                            x = inner.Left + i - 1;
                            y = inner.Top + w;
                            break;
                        default:
                            x = inner.Right - i;
                            y = inner.Top + w;
                            break;
                    }
                    if (inner.Contains(x, y))
                    {
                        stairCells[(x, y)] = i;
                    }
                }
            }
            ushort[] floor = HexList.Parse(o.FloorIds), upper = HexList.Parse(o.UpperFloorIds), stair = HexList.Parse(stairs?.For(o.Direction) ?? string.Empty);
            ushort[][] pieces = walls == null ? null : new[] { HexList.Parse(walls.AlongX), HexList.Parse(walls.AlongY), HexList.Parse(walls.Post) };
            bool Inner(int x, int y) => inner.Contains(x, y);
            int levels = Math.Max(1, (int)Math.Ceiling(o.Depth / 20.0));
            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    bool overStairs = stairCells.ContainsKey((x, y));
                    switch (o.Land)
                    {
                        case CellarLand.LowerToCellar:
                            map.SetLand(x, y, 0x0002, cz);
                            break;
                        case CellarLand.WholeAreaNoDraw:
                            map.SetLand(x, y, 0x0002, map.LandZ(x, y));
                            break;
                        case CellarLand.HoleOverStairs:
                            if (overStairs)
                            {
                                map.SetLand(x, y, 0x0002, map.LandZ(x, y));
                            }
                            break;
                    }
                    bool border = !Inner(x, y);
                    if (border)
                    {
                        if (pieces != null)
                        {
                            ushort[] use = pieces[WallKind(Inner, x, y)];
                            if (use.Length == 0)
                            {
                                use = pieces[2].Length > 0 ? pieces[2] : pieces[0].Length > 0 ? pieces[0] : pieces[1];
                            }
                            for (int l = 0; use.Length > 0 && l < levels; l++)
                            {
                                statics.Add(MapData.Add(new StaticItem { Id = Pick(use, rnd), X = x, Y = y, Z = Math.Clamp(cz + l * 20, -128, 127) }));
                            }
                        }
                        continue;
                    }
                    if (floor.Length > 0)
                    {
                        statics.Add(MapData.Add(new StaticItem { Id = Pick(floor, rnd), X = x, Y = y, Z = cz }));
                    }
                    if (stairCells.TryGetValue((x, y), out int step))
                    {
                        if (stair.Length > 0)
                        {
                            statics.Add(MapData.Add(new StaticItem { Id = Pick(stair, rnd), X = x, Y = y, Z = Math.Clamp(ground - o.StepHeight * step, -128, 127) }));
                        }
                    }
                    else if (upper.Length > 0)
                    {
                        statics.Add(MapData.Add(new StaticItem { Id = Pick(upper, rnd), X = x, Y = y, Z = ground }));
                    }
                }
            }
            return statics;
        }

        // ------------------------------------------------------------------ stairs

        /// <summary>
        /// Stairs from <paramref name="at"/> downwards in direction <paramref name="down"/>: the first step lies just below
        /// <paramref name="topZ"/>, every step 5 lower, filled down to the bottom so there are no holes under the steps.
        /// </summary>
        public static List<StaticChange> BuildStairs(MapData map, Point at, int topZ, Compass down, int steps, int width, StairSet set)
        {
            var statics = new List<StaticChange>();
            ushort[] ids = set == null ? Array.Empty<ushort>() : HexList.Parse(set.For(down));
            if (ids.Length == 0)
            {
                return statics;
            }
            int dx = down == Compass.East ? 1 : down == Compass.West ? -1 : 0;
            int dy = down == Compass.South ? 1 : down == Compass.North ? -1 : 0;
            int px = dy != 0 ? 1 : 0, py = dx != 0 ? 1 : 0; // across
            int n = Math.Clamp(steps, 1, 40);
            int bottom = topZ - 5 * n;
            for (int k = 0; k < n; k++)
            {
                for (int w = 0; w < Math.Max(1, width); w++)
                {
                    int x = at.X + dx * k + px * w, y = at.Y + dy * k + py * w;
                    if (!map.InBounds(x, y))
                    {
                        continue;
                    }
                    for (int z = topZ - 5 * (k + 1); z >= bottom; z -= 5)
                    {
                        statics.Add(MapData.Add(new StaticItem { Id = ids[0], X = x, Y = y, Z = Math.Clamp(z, -128, 127) }));
                    }
                }
            }
            return statics;
        }

        // ------------------------------------------------------------------ bridge

        /// <summary>Bridge from a to b (snapped to west – east or north – south): deck, railings on both sides, posts, arch.</summary>
        public static List<StaticChange> BuildBridge(MapData map, Point a, Point b, BridgeSet set, BridgeSettings o, Random rnd)
        {
            var statics = new List<StaticChange>();
            if (set == null)
            {
                return statics;
            }
            bool alongX = Math.Abs(b.X - a.X) >= Math.Abs(b.Y - a.Y);
            var end = alongX ? new Point(b.X, a.Y) : new Point(a.X, b.Y);
            int n = Math.Max(Math.Abs(end.X - a.X), Math.Abs(end.Y - a.Y));
            int sx = Math.Sign(end.X - a.X), sy = Math.Sign(end.Y - a.Y);
            int baseZ = Math.Max(map.LandZ(a.X, a.Y), map.LandZ(end.X, end.Y)) + o.ZOffset;
            ushort[] deck = HexList.Parse(alongX ? set.DeckX : set.DeckY);
            ushort[] rail = HexList.Parse(alongX ? set.RailX : set.RailY);
            ushort[] post = HexList.Parse(set.Post);
            int width = Math.Max(1, o.Width);
            int lo = -(width - 1) / 2, hi = width / 2;
            for (int k = 0; k <= n; k++)
            {
                double t = n == 0 ? 0 : k / (double)n;
                int z = baseZ + (int)Math.Round(o.Arch * Math.Sin(Math.PI * t));
                for (int w = lo; w <= hi; w++)
                {
                    int x = a.X + sx * k + (alongX ? 0 : w), y = a.Y + sy * k + (alongX ? w : 0);
                    if (!map.InBounds(x, y))
                    {
                        continue;
                    }
                    if (deck.Length > 0)
                    {
                        statics.Add(MapData.Add(new StaticItem { Id = Pick(deck, rnd), X = x, Y = y, Z = Math.Clamp(z, -128, 127) }));
                    }
                    bool side = w == lo || w == hi;
                    if (o.Railings && side && width >= 2)
                    {
                        bool atEnd = k == 0 || k == n;
                        ushort[] use = atEnd && post.Length > 0 ? post : rail;
                        if (use.Length > 0)
                        {
                            statics.Add(MapData.Add(new StaticItem { Id = Pick(use, rnd), X = x, Y = y, Z = Math.Clamp(z, -128, 127) }));
                        }
                    }
                }
            }
            return statics;
        }

        // ------------------------------------------------------------------ natural course

        /// <summary>
        /// Natural course from a to b: the straight line swings sideways with a few random bends (strongest in the middle,
        /// 0 at both ends). Points about half a tile apart; the third value is 0..1 along the course.
        /// </summary>
        public static List<(float x, float y, float t)> Course(PointF a, PointF b, CourseSettings o, Random rnd)
        {
            var result = new List<(float, float, float)>();
            double dx = b.X - a.X, dy = b.Y - a.Y, len = Math.Sqrt(dx * dx + dy * dy);
            if (len < 0.5)
            {
                result.Add((a.X, a.Y, 0));
                return result;
            }
            double nx = -dy / len, ny = dx / len;
            double amp = len * Math.Clamp(o.Meander, 0, 100) / 100.0 * 0.5;
            double bends = Math.Max(0.5, o.Bends * len / 20.0);
            // three sine waves with random phases: looks less regular than one
            double p1 = rnd.NextDouble() * Math.PI * 2, p2 = rnd.NextDouble() * Math.PI * 2, p3 = rnd.NextDouble() * Math.PI * 2;
            double s1 = rnd.NextDouble() < 0.5 ? -1 : 1;
            int steps = Math.Max(2, (int)Math.Ceiling(len * 2));
            for (int i = 0; i <= steps; i++)
            {
                double t = i / (double)steps;
                double envelope = Math.Sin(Math.PI * t);
                double wave = 0.6 * Math.Sin(Math.PI * bends * t + p1) * s1 + 0.3 * Math.Sin(Math.PI * bends * 2.1 * t + p2) + 0.1 * Math.Sin(Math.PI * bends * 4.3 * t + p3);
                double off = amp * envelope * wave;
                result.Add(((float)(a.X + dx * t + nx * off), (float)(a.Y + dy * t + ny * off), (float)t));
            }
            return result;
        }
    }
}

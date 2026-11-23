/***************************************************************************
 *
 *  * Dragon Map Studio – staircases like in the client
 *  *
 *  *  A flight = 4 stair pieces in one direction, each 5 z higher (4 × 5 = 20 z = one floor). After every flight one flat
 *  *  field (landing), then the next flight: straight on, turning left / right (L, several floors = spiral) or back (U).
 *  *  Everything is filled down to the bottom, so the staircase stands solid (no holes under the steps).
 *  *  The pieces of a stair set can be learned from a selection of a stair built by hand.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public enum StairTurn
    {
        [Description("Gerade weiter")]
        Straight,

        [Description("Rechts herum (L, mehrere Etagen = Wendel)")]
        Right,

        [Description("Links herum (L, mehrere Etagen = Wendel)")]
        Left,

        [Description("Zurück (U, neben dem Lauf)")]
        Back
    }

    public sealed class StaircaseSettings
    {
        [Category("Treppe"), DisplayName("Richtung (hinauf)"), Description("In diese Richtung geht man die erste Treppe hinauf. Im Werkzeug dreht R.")]
        public Compass Up { get; set; } = Compass.North;

        [Category("Treppe"), DisplayName("Etagen"), Description("So viele Läufe übereinander (je Lauf eine Etage = Stufen × Stufen-Höhe).")]
        public int Floors { get; set; } = 1;

        [Category("Treppe"), DisplayName("Stufen je Lauf"), Description("Original: 4 Stufen × 5 Z = 20 Z = eine Etage.")]
        public int Steps { get; set; } = 4;

        [Category("Treppe"), DisplayName("Stufen-Höhe (Z)"), Description("Nur wenn 'Höhen aus der TileData' aus ist.")]
        public int StepHeight { get; set; } = 5;

        [Category("Treppe"), DisplayName("Höhen aus der TileData"), Description("An = Stufen-, Block- und Absatz-Höhe stehen in der TileData (normal 5) und werden von dort gelesen. Aus = 'Stufen-Höhe'.")]
        public bool HeightFromTileData { get; set; } = true;

        [Category("Treppe"), DisplayName("Breite (Tiles)")]
        public int Width { get; set; } = 1;

        [Category("Treppe"), DisplayName("Wende nach jedem Lauf")]
        public StairTurn Turn { get; set; } = StairTurn.Right;

        [Category("Treppe"), DisplayName("Absatz oben"), Description("Nach dem letzten Lauf noch ein flaches Feld (zum Aussteigen).")]
        public bool TopLanding { get; set; } = true;

        [Category("Treppe"), DisplayName("Nach unten bauen"), Description("An = der Klick ist das obere Ende (Keller: von der Karte hinunter). Aus = der Klick ist der Fuß.")]
        public bool Downwards { get; set; }

        [Category("Treppe"), DisplayName("Bis zum Boden füllen"), Description("Unter Stufen und Absätzen Blöcke bis zum Fuß – massiv wie im Original.")]
        public bool Fill { get; set; } = true;

        [Category("Modell"), DisplayName("Modell"), Description("Ein Treppen-Modell aus dem Treppen-Designer ('Kategorie / Name'). Leer = Treppe aus dem Treppen-Set erzeugen.")]
        public string Model { get; set; } = string.Empty;

        [Category("Modell"), DisplayName("Modell mehrmals"), Description("So oft wird das Modell hintereinander gesetzt (jedes Mal um seinen Anstieg höher).")]
        public int Repeats { get; set; } = 1;
    }

    public static class Staircase
    {
        public static Point Step(Compass c) => c switch
        {
            Compass.North => new Point(0, -1),
            Compass.East => new Point(1, 0),
            Compass.South => new Point(0, 1),
            _ => new Point(-1, 0)
        };

        public static Compass Opposite(Compass c) => (Compass)(((int)c + 2) % 4);

        public static Compass RightOf(Compass c) => (Compass)(((int)c + 1) % 4);

        public static Compass LeftOf(Compass c) => (Compass)(((int)c + 3) % 4);

        private static Point Add(Point p, Point d, int k) => new Point(p.X + d.X * k, p.Y + d.Y * k);

        /// <summary>
        /// A staircase from <paramref name="at"/> (first step) with its foot on <paramref name="footZ"/>.
        /// Returns the statics; <paramref name="cells"/> gets every used tile with its top height (for previews / holes).
        /// </summary>
        public static List<StaticChange> Build(MapData map, Point at, int footZ, StaircaseSettings o, StairSet set, Dictionary<Point, int> cells = null)
        {
            var result = new List<StaticChange>();
            if (set == null)
            {
                return result;
            }
            int steps = Math.Clamp(o.Steps, 1, 20), h = Math.Clamp(o.StepHeight, 1, 20), w = Math.Clamp(o.Width, 1, 8), floors = Math.Clamp(o.Floors, 1, 10);
            ushort block = HexList.Parse(set.Block).FirstOrDefault();
            ushort landing = HexList.Parse(set.Landing).FirstOrDefault();
            // heights of the pieces from the tiledata (a step, a block: normally 5)
            int HeightOf(ushort id)
            {
                if (!o.HeightFromTileData || id == 0)
                {
                    return h;
                }
                int th = UoArtCache.ItemHeight(id);
                return th >= 1 && th <= 30 ? th : h;
            }
            if (o.HeightFromTileData)
            {
                ushort first = HexList.Parse(set.For(Staircase.Opposite(o.Up))).FirstOrDefault();
                h = HeightOf(first);
            }
            int bottom = o.Downwards ? footZ - floors * steps * h : footZ;
            var placed = new HashSet<(int, int, int)>();
            void Put(ushort id, int x, int y, int z)
            {
                if (id == 0 || !map.InBounds(x, y) || !placed.Add((x, y, z)))
                {
                    return;
                }
                result.Add(MapData.Add(new StaticItem { Id = id, X = x, Y = y, Z = Math.Clamp(z, -128, 127) }));
            }
            void Column(Point p, ushort top, ushort fill, int topZ)
            {
                // the piece with its top on topZ (its own tiledata height), below it blocks stacked from the bottom up
                int ht = HeightOf(top);
                Put(top, p.X, p.Y, topZ - ht);
                if (o.Fill)
                {
                    int hb = Math.Max(1, HeightOf(fill));
                    int z = bottom;
                    for (int i = 0; i < 40 && z + hb <= topZ - ht; i++, z += hb)
                    {
                        Put(fill, p.X, p.Y, z);
                    }
                    if (z < topZ - ht)
                    {
                        Put(fill, p.X, p.Y, topZ - ht - hb); // closes a gap when the heights do not add up
                    }
                }
                if (cells != null)
                {
                    cells[p] = topZ;
                }
            }

            Compass dir = o.Up;
            Point pos = at;
            int baseZ = bottom;
            for (int f = 0; f < floors; f++)
            {
                Point d = Step(dir), side = Step(RightOf(dir));
                ushort stair = HexList.Parse(set.For(Opposite(dir))).FirstOrDefault();
                ushort fill = block != 0 ? block : stair;
                for (int k = 0; k < steps; k++)
                {
                    for (int c = 0; c < w; c++)
                    {
                        Column(Add(Add(pos, d, k), side, c), stair, fill, baseZ + (k + 1) * h);
                    }
                }
                int top = baseZ + steps * h;
                Point land = Add(pos, d, steps);
                ushort landTop = landing != 0 ? landing : fill;
                bool last = f == floors - 1;
                if (last && !o.TopLanding)
                {
                    break;
                }
                StairTurn turn = last ? StairTurn.Straight : o.Turn;
                switch (turn)
                {
                    case StairTurn.Straight:
                        for (int c = 0; c < w; c++)
                        {
                            Column(Add(land, side, c), landTop, fill, top);
                        }
                        pos = Add(land, d, 1);
                        break;
                    case StairTurn.Right:
                        for (int a = 0; a < w; a++)
                        {
                            for (int c = 0; c < w; c++)
                            {
                                Column(Add(Add(land, d, a), side, c), landTop, fill, top);
                            }
                        }
                        pos = Add(Add(land, d, w - 1), side, w);
                        dir = RightOf(dir);
                        break;
                    case StairTurn.Left:
                        for (int a = 0; a < w; a++)
                        {
                            for (int c = 0; c < w; c++)
                            {
                                Column(Add(Add(land, d, a), side, c), landTop, fill, top);
                            }
                        }
                        pos = Add(land, side, -1);
                        dir = LeftOf(dir);
                        break;
                    default:
                        for (int c = 0; c < 2 * w; c++)
                        {
                            Column(Add(land, side, c), landTop, fill, top);
                        }
                        pos = Add(Add(land, d, -1), side, 2 * w - 1);
                        dir = Opposite(dir);
                        break;
                }
                baseZ = top;
            }
            return result;
        }

        /// <summary>
        /// Learns a stair set from statics of a stair built by hand: a piece with a piece one step higher next to it
        /// is a stair going up in that direction; pieces under others are the filling block, a flat top piece the landing.
        /// </summary>
        public static StairSet Learn(IReadOnlyCollection<StaticItem> sel, string name, out string report)
        {
            var set = new StairSet { Name = name };
            var at = sel.GroupBy(s => (s.X, s.Y)).ToDictionary(g => g.Key, g => g.OrderBy(s => s.Z).ToList());
            int TopZ((int, int) p) => at.TryGetValue(p, out List<StaticItem> l) ? l[l.Count - 1].Z : int.MinValue;
            var votes = new Dictionary<Compass, Dictionary<ushort, int>>();
            var under = new Dictionary<ushort, int>();
            foreach (KeyValuePair<(int X, int Y), List<StaticItem>> cell in at)
            {
                List<StaticItem> stack = cell.Value;
                StaticItem top = stack[stack.Count - 1];
                foreach (StaticItem s in stack.Take(stack.Count - 1))
                {
                    under[s.Id] = under.TryGetValue(s.Id, out int n) ? n + 1 : 1;
                }
                foreach (Compass c in new[] { Compass.North, Compass.East, Compass.South, Compass.West })
                {
                    Point d = Step(c);
                    int nz = TopZ((cell.Key.X + d.X, cell.Key.Y + d.Y));
                    int back = TopZ((cell.Key.X - d.X, cell.Key.Y - d.Y));
                    // one step higher ahead, one lower (or nothing) behind: this piece goes up towards c
                    if (nz > top.Z && nz - top.Z <= 10 && (back == int.MinValue || back < top.Z))
                    {
                        Compass down = Opposite(c);
                        if (!votes.TryGetValue(down, out Dictionary<ushort, int> v))
                        {
                            votes[down] = v = new Dictionary<ushort, int>();
                        }
                        v[top.Id] = v.TryGetValue(top.Id, out int k) ? k + 1 : 1;
                    }
                }
            }
            var lines = new List<string>();
            foreach (KeyValuePair<Compass, Dictionary<ushort, int>> v in votes)
            {
                ushort id = v.Value.OrderByDescending(p => p.Value).First().Key;
                string hex = id.ToString("x4");
                switch (v.Key)
                {
                    case Compass.North:
                        set.DownNorth = hex;
                        break;
                    case Compass.East:
                        set.DownEast = hex;
                        break;
                    case Compass.South:
                        set.DownSouth = hex;
                        break;
                    default:
                        set.DownWest = hex;
                        break;
                }
                lines.Add($"hinunter {v.Key}: {hex} {UoArtCache.ItemName(id)}");
            }
            var stairIds = new HashSet<string>(new[] { set.DownNorth, set.DownEast, set.DownSouth, set.DownWest });
            if (under.Count > 0)
            {
                ushort b = under.OrderByDescending(p => p.Value).First().Key;
                set.Block = b.ToString("x4");
                lines.Add($"Füll-Block: {set.Block} {UoArtCache.ItemName(b)}");
            }
            // landing: a top piece that is no stair
            var tops = at.Values.Select(l => l[l.Count - 1].Id).Where(i => !stairIds.Contains(i.ToString("x4"))).GroupBy(i => i).OrderByDescending(g => g.Count()).FirstOrDefault();
            if (tops != null)
            {
                set.Landing = tops.Key.ToString("x4");
                lines.Add($"Absatz: {set.Landing} {UoArtCache.ItemName(tops.Key)}");
            }
            int missing = new[] { set.DownNorth, set.DownEast, set.DownSouth, set.DownWest }.Count(string.IsNullOrEmpty);
            if (missing > 0)
            {
                lines.Add($"{missing} Richtung(en) fehlen – eine Treppe in jede Richtung bauen und mit auswählen, oder von Hand eintragen.");
            }
            report = string.Join(Environment.NewLine, lines);
            return set;
        }
    }
}

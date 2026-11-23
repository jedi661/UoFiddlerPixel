/***************************************************************************
 *
 *  * Dragon Map Studio – dungeon workshop (pure logic): clear, move, raise a part of a working copy,
 *  * ground level of a place, the built-in style "Klassisch (map0-Log)"
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public static class Workshop
    {
        /// <summary>Most common land height in the rectangle (the height of the map at this place, e.g. 0 for a town).</summary>
        public static int GroundLevel(MapData map, Rectangle r)
        {
            var count = new Dictionary<int, int>();
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    if (map.InBounds(x, y))
                    {
                        int z = map.LandZ(x, y);
                        count[z] = count.TryGetValue(z, out int c) ? c + 1 : 1;
                    }
                }
            }
            return count.Count == 0 ? 0 : count.OrderByDescending(kv => kv.Value).ThenBy(kv => Math.Abs(kv.Key)).First().Key;
        }

        /// <summary>
        /// Empties the rectangle: statics away and / or the land becomes the void tile (0002 NoDraw, 0244 black …)
        /// on its own height – so no holes into the depth appear.
        /// </summary>
        public static (List<StaticChange> statics, List<LandChange> land) Clear(MapData map, Rectangle r, bool statics, bool land, ushort voidTile)
        {
            var s = new List<StaticChange>();
            var l = new List<LandChange>();
            r.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    if (statics)
                    {
                        s.AddRange(map.StaticsAt(x, y).Select(MapData.Remove));
                    }
                    int i = y * map.Width + x;
                    if (land && map.Tiles[i] != voidTile)
                    {
                        l.Add(new LandChange { Index = i, OldId = map.Tiles[i], OldZ = map.Z[i], NewId = voidTile, NewZ = map.Z[i] });
                    }
                }
            }
            return (s, l);
        }

        /// <summary>Moves land and statics of the rectangle by (dx, dy); the cells left behind become the void tile.</summary>
        public static (List<StaticChange> statics, List<LandChange> land, Rectangle moved) Shift(MapData map, Rectangle r, int dx, int dy, ushort voidTile)
        {
            var s = new List<StaticChange>();
            r.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            var target = new Rectangle(r.X + dx, r.Y + dy, r.Width, r.Height);
            target.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            var newTiles = new Dictionary<int, (ushort id, sbyte z)>();
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    int i = y * map.Width + x;
                    if (!newTiles.ContainsKey(i))
                    {
                        newTiles[i] = (voidTile, map.Z[i]);
                    }
                }
            }
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (!map.InBounds(nx, ny))
                    {
                        continue;
                    }
                    newTiles[ny * map.Width + nx] = (map.Tiles[y * map.Width + x], map.Z[y * map.Width + x]);
                    foreach (StaticItem st in map.StaticsAt(x, y).ToList())
                    {
                        s.Add(MapData.Modify(st, t =>
                        {
                            t.X = nx;
                            t.Y = ny;
                        }));
                    }
                }
            }
            // statics already standing at the target outside the old rectangle are removed (the moved part replaces them)
            for (int y = target.Top; y < target.Bottom; y++)
            {
                for (int x = target.Left; x < target.Right; x++)
                {
                    if (!r.Contains(x, y))
                    {
                        s.AddRange(map.StaticsAt(x, y).Select(MapData.Remove));
                    }
                }
            }
            var l = newTiles.Where(kv => map.Tiles[kv.Key] != kv.Value.id || map.Z[kv.Key] != kv.Value.z)
                            .Select(kv => new LandChange { Index = kv.Key, OldId = map.Tiles[kv.Key], OldZ = map.Z[kv.Key], NewId = kv.Value.id, NewZ = kv.Value.z })
                            .ToList();
            return (s, l, target);
        }

        /// <summary>Raises / lowers everything in the rectangle: all statics, and the land that is not void.</summary>
        public static (List<StaticChange> statics, List<LandChange> land) Raise(MapData map, Rectangle r, int dz, HashSet<ushort> voidTiles)
        {
            var s = new List<StaticChange>();
            var l = new List<LandChange>();
            r.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    foreach (StaticItem st in map.StaticsAt(x, y))
                    {
                        s.Add(MapData.Modify(st, t => t.Z = Math.Clamp(t.Z + dz, -128, 127)));
                    }
                    int i = y * map.Width + x;
                    if (!voidTiles.Contains(map.Tiles[i]))
                    {
                        l.Add(new LandChange { Index = i, OldId = map.Tiles[i], OldZ = map.Z[i], NewId = map.Tiles[i], NewZ = (sbyte)Math.Clamp(map.Z[i] + dz, -128, 127) });
                    }
                }
            }
            return (s, l);
        }

        /// <summary>
        /// Built-in dungeon style from the analysis of map0 (6035, 25): cave floor 0245–0249, void 0244, walls 0241 / 0242 / 0243
        /// after the UO rule (north / west walls on the void outside, south / east walls on the last floor cell), garbage and blood.
        /// </summary>
        /// <summary>
        /// Stairs between dungeon floors (plans[0] = top on <paramref name="level"/>, every next one <paramref name="fh"/> lower):
        /// in a row open on both floors a flight down from the upper floor, the upper floor above the steps becomes a hole
        /// (NoDraw land when the top floor lies on the land, floor statics away). Returns the number of stairs; places = their foot cells.
        /// </summary>
        public static int ConnectFloors(MapData map, Point origin, List<DungeonPlan> plans, int level, int fh, bool topOnLand, StairSet set, out List<Point> places)
        {
            places = new List<Point>();
            int made = 0;
            var floorIds = new HashSet<ushort> { 0x053d, 0x053e };
            for (int f = 0; f < plans.Count - 1; f++)
            {
                DungeonPlan up = plans[f], down = plans[f + 1];
                Point? at = null;
                // a row of cells along X (step before + 4 steps + landing + a free cell), open on both floors, with a free row beside
                for (int y = 1; y < up.H - 1 && at == null; y++)
                {
                    for (int x = 1; x < up.W - 7 && at == null; x++)
                    {
                        bool ok = true;
                        for (int k = -1; k < 6 && ok; k++)
                        {
                            ok = up.Floor(x + k, y) && down.Floor(x + k, y) && down.Floor(x + k, y - 1) && up.Floor(x + k, y - 1);
                        }
                        if (ok)
                        {
                            at = new Point(x, y);
                        }
                    }
                }
                if (at == null)
                {
                    continue;
                }
                int topZ = level - f * fh;
                var p = new Point(origin.X + at.Value.X, origin.Y + at.Value.Y);
                var o = new StaircaseSettings { Up = Compass.East, Floors = 1, Steps = Math.Max(1, fh / 5), StepHeight = 5, Width = 1, Downwards = true, TopLanding = true, Fill = true };
                List<StaticChange> st = Staircase.Build(map, p, topZ, o, set);
                var holes = new List<StaticChange>();
                for (int k = 0; k < o.Steps; k++)
                {
                    int x = p.X + k, y = p.Y;
                    if (f == 0 && topOnLand)
                    {
                        map.SetLand(x, y, 0x0002, map.LandZ(x, y));
                    }
                    holes.AddRange(map.StaticsAt(x, y).Where(s => s.Z == topZ && floorIds.Contains(s.Id)).ToList().Select(MapData.Remove));
                }
                map.ApplyStatics(holes);
                map.ApplyStatics(st);
                places.Add(p);
                made++;
            }
            return made;
        }

        public static DungeonLook Classic()
        {
            WallRule R(bool onFloor, int mask, int cells, int filled, params string[] stacks) =>
                new WallRule { OnFloor = onFloor, Mask = mask, Cells = cells, Filled = filled, Stacks = stacks.ToList() };
            return new DungeonLook
            {
                Name = "Klassisch (map0-Log)",
                Source = "map0.mul (6035, 25, 91×78) – Analyse des Besitzers",
                VoidLand = "0244",
                FloorLand = "0245 0246 0247 0248 0249",
                DecorPercent = 4,
                Decor = new List<string> { "10ee@0", "10ef@0", "10f0@0", "10f1@0", "10f2@0", "10f3@0", "122a@0", "122f@0", "10ee@0", "10ef@0" },
                Walls = new List<WallRule>
                {
                    // void cells: floor east of it = west wall 0242, floor south = north wall 0241, floor north / west: nothing
                    R(false, 2, 118, 113, "0242@0"),
                    R(false, 4, 116, 103, "0241@0"),
                    R(false, 1, 111, 0),
                    R(false, 8, 125, 14, "0242@0"),
                    R(false, 3, 7, 7, "0242@0"),
                    R(false, 6, 13, 11, "0241@0", "0241@0", "0242@0"),
                    R(false, 12, 7, 7, "0241@0"),
                    R(false, 9, 4, 1, "0242@0"),
                    R(false, 16, 6, 1, "0242@0"),
                    R(false, 32, 15, 6, "0243@0"),
                    // floor cells at the edge: void south = south wall 0241, void east = east wall 0242
                    R(true, 11, 124, 114, "0241@0"),
                    R(true, 13, 122, 110, "0242@0"),
                    R(true, 3, 8, 5, "0241@0"),
                    R(true, 9, 6, 5, "0243@0"),
                    R(true, 12, 9, 9, "0242@0"),
                    R(true, 6, 12, 6, "0241@0"),
                    R(true, 7, 117, 10, "0242@0"),
                    R(true, 14, 115, 12, "0241@0"),
                    // inner walls between rooms (on floor land)
                    R(true, WallRule.InnerAlongX, 10, 10, "0241@0"),
                    R(true, WallRule.InnerAlongY, 10, 10, "0242@0"),
                    R(true, WallRule.InnerPost, 10, 10, "0243@0")
                }
            };
        }
    }
}

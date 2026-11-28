/***************************************************************************
 *
 *  * Dragon Map Studio – town tools of the region window: find the buildings of an area (walls, roofs, floors that
 *  *  touch each other) and rearrange them ("Stadt neu ordnen"): every building is taken out and set again in rows
 *  *  with a gap, optionally with its floor tiles, the ground below levelled, free land filled with a road tile.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>A building found in an area.</summary>
    public sealed class TownBuilding
    {
        public int Number { get; set; }

        /// <summary>Tiles of the building (including the inside).</summary>
        public Rectangle Bounds { get; set; }

        public List<StaticItem> Items { get; set; } = new List<StaticItem>();

        /// <summary>Land height in the middle of the building (its "ground").</summary>
        public int GroundZ { get; set; }

        public override string ToString() => $"{L.T("Gebäude")} {Number} – {Bounds.Width}×{Bounds.Height}, {Items.Count} Statics, ({Bounds.X}, {Bounds.Y})";

        /// <summary>The building as a template (statics + land, anchor = middle) – for pictures and saving.</summary>
        public MapTemplate ToTemplate(MapData map, bool withLand)
        {
            int ax = Bounds.X + Bounds.Width / 2, ay = Bounds.Y + Bounds.Height / 2;
            var t = new MapTemplate
            {
                Name = ToString(), OriginX = ax, OriginY = ay, OriginZ = GroundZ,
                Statics = Items.Select(s => new TemplateStatic { Id = s.Id, Hue = s.Hue, X = s.X - ax, Y = s.Y - ay, Z = s.Z - GroundZ }).ToList()
            };
            if (withLand)
            {
                t.LandX = Bounds.X - ax;
                t.LandY = Bounds.Y - ay;
                t.LandWidth = Bounds.Width;
                t.LandHeight = Bounds.Height;
                t.LandTiles = new ushort[Bounds.Width * Bounds.Height];
                t.LandZ = new int[Bounds.Width * Bounds.Height];
                for (int y = 0; y < Bounds.Height; y++)
                {
                    for (int x = 0; x < Bounds.Width; x++)
                    {
                        t.LandTiles[y * Bounds.Width + x] = map.LandId(Bounds.X + x, Bounds.Y + y);
                        t.LandZ[y * Bounds.Width + x] = map.LandZ(Bounds.X + x, Bounds.Y + y) - GroundZ;
                    }
                }
            }
            return t;
        }
    }

    public enum TownOrder
    {
        [Description("Wie vorher (von oben links)")]
        AsBefore,

        [Description("Nach Größe (große zuerst)")]
        BySize,

        [Description("Zufällig")]
        Random
    }

    public sealed class TownLayoutSettings
    {
        [DisplayName("Abstand (Tiles)"), Description("Freie Tiles zwischen zwei Gebäuden (Straße / Weg).")]
        public int Gap { get; set; } = 3;

        [DisplayName("Reihenfolge"), Description("In welcher Reihenfolge die Gebäude in die Reihen gesetzt werden.")]
        public TownOrder Order { get; set; } = TownOrder.AsBefore;

        [DisplayName("Boden mitnehmen"), Description("Die Land-Tiles unter dem Gebäude (Fußboden, Pflaster) wandern mit.")]
        public bool MoveLand { get; set; } = true;

        [DisplayName("Boden begradigen"), Description("Das Land unter jedem Gebäude (+1 Rand) auf seine alte Bodenhöhe setzen – nichts steckt im Boden.")]
        public bool Flatten { get; set; } = true;

        [DisplayName("Freiflächen-Tile (hex, 0 = aus)"), Description("Alle freien Tiles im Ziel-Bereich bekommen dieses Land-Tile (Straße, Pflaster). 0 = Land bleibt.")]
        public string RoadTile { get; set; } = "0";

        [DisplayName("Zufalls-Zahl"), Description("Für 'Zufällig'. 0 = jedes Mal anders.")]
        public int Seed { get; set; }
    }

    public static class TownTools
    {
        /// <summary>Is a static part of a building (wall, door, window, roof, floor)?</summary>
        public static Func<ushort, bool> IsBuildingPart { get; set; } = id =>
        {
            StaticKind k = UoArtCache.ItemKind(id);
            return k == StaticKind.Wall || k == StaticKind.Roof || k == StaticKind.Floor;
        };

        /// <summary>
        /// The buildings of an area: tiles with wall / roof / floor statics that touch (8 neighbours) form one building;
        /// all statics inside its rectangle belong to it. Groups smaller than <paramref name="minTiles"/> are left out.
        /// </summary>
        public static List<TownBuilding> FindBuildings(MapData map, Rectangle area, int minTiles = 6)
        {
            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            var part = new HashSet<Point>();
            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    if (map.StaticsAt(x, y).Any(s => IsBuildingPart(s.Id)))
                    {
                        part.Add(new Point(x, y));
                    }
                }
            }
            var result = new List<TownBuilding>();
            var seen = new HashSet<Point>();
            foreach (Point start in part.OrderBy(p => p.Y).ThenBy(p => p.X))
            {
                if (!seen.Add(start))
                {
                    continue;
                }
                var group = new List<Point> { start };
                var queue = new Queue<Point>();
                queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    Point p = queue.Dequeue();
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            var n = new Point(p.X + dx, p.Y + dy);
                            if (part.Contains(n) && seen.Add(n))
                            {
                                group.Add(n);
                                queue.Enqueue(n);
                            }
                        }
                    }
                }
                if (group.Count < minTiles)
                {
                    continue;
                }
                var r = Rectangle.FromLTRB(group.Min(p => p.X), group.Min(p => p.Y), group.Max(p => p.X) + 1, group.Max(p => p.Y) + 1);
                var items = new List<StaticItem>();
                for (int y = r.Top; y < r.Bottom; y++)
                {
                    for (int x = r.Left; x < r.Right; x++)
                    {
                        items.AddRange(map.StaticsAt(x, y));
                    }
                }
                result.Add(new TownBuilding { Bounds = r, Items = items, GroundZ = map.LandZ(r.X + r.Width / 2, r.Y + r.Height / 2) });
            }
            // a building inside the rectangle of another (e.g. a shed in a yard) stays its own; overlapping items are kept once
            var taken = new HashSet<StaticItem>();
            foreach (TownBuilding b in result.OrderByDescending(b => b.Bounds.Width * b.Bounds.Height))
            {
                b.Items = b.Items.Where(taken.Add).ToList();
            }
            result = result.Where(b => b.Items.Count > 0).OrderBy(b => b.Bounds.Y).ThenBy(b => b.Bounds.X).ToList();
            for (int i = 0; i < result.Count; i++)
            {
                result[i].Number = i + 1;
            }
            return result;
        }

        /// <summary>
        /// Rearranges the buildings in rows inside <paramref name="target"/> (one undo step for the caller).
        /// Buildings that do not fit stay where they were. <paramref name="placedCount"/> = how many were moved.
        /// </summary>
        public static (List<StaticChange> statics, List<LandChange> land) Rearrange(MapData map, List<TownBuilding> buildings, Rectangle target, TownLayoutSettings o, out int placedCount, out List<string> log)
        {
            log = new List<string>();
            var statics = new List<StaticChange>();
            var land = new Dictionary<int, LandChange>();
            placedCount = 0;
            target.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            if (buildings.Count == 0 || target.IsEmpty)
            {
                return (statics, land.Values.ToList());
            }
            int gap = Math.Max(0, o.Gap);
            IEnumerable<TownBuilding> order = o.Order switch
            {
                TownOrder.BySize => buildings.OrderByDescending(b => b.Bounds.Width * b.Bounds.Height),
                _ => buildings.OrderBy(b => b.Bounds.Y).ThenBy(b => b.Bounds.X)
            };
            if (o.Order == TownOrder.Random)
            {
                var rnd = new Random(o.Seed != 0 ? o.Seed : Environment.TickCount);
                order = buildings.OrderBy(_ => rnd.Next()).ToList();
            }

            void SetLand(int x, int y, ushort id, int z)
            {
                if (!map.InBounds(x, y))
                {
                    return;
                }
                int i = y * map.Width + x;
                bool had = land.TryGetValue(i, out LandChange c);
                ushort oldId = had ? c.OldId : map.Tiles[i];
                sbyte oldZ = had ? c.OldZ : map.Z[i];
                land[i] = new LandChange { Index = i, OldId = oldId, OldZ = oldZ, NewId = id, NewZ = (sbyte)Math.Clamp(z, -128, 127) };
            }

            // shelf packing: left to right, a new row when the next one does not fit
            int cx = target.X + gap / 2, cy = target.Y + gap / 2, rowH = 0;
            var footprints = new List<Rectangle>();
            var plan = new List<(TownBuilding b, Point at)>();
            foreach (TownBuilding b in order)
            {
                int w = b.Bounds.Width, h = b.Bounds.Height;
                if (cx + w > target.Right)
                {
                    cx = target.X + gap / 2;
                    cy += rowH + gap;
                    rowH = 0;
                }
                if (cx + w > target.Right || cy + h > target.Bottom)
                {
                    log.Add($"{b}: " + L.T("passt nicht mehr in den Bereich – bleibt stehen"));
                    continue;
                }
                plan.Add((b, new Point(cx, cy)));
                footprints.Add(new Rectangle(cx, cy, w, h));
                cx += w + gap;
                rowH = Math.Max(rowH, h);
            }

            // take the moved buildings out
            foreach ((TownBuilding b, _) in plan)
            {
                statics.AddRange(b.Items.Select(MapData.Remove));
            }
            // the land the buildings take with them (read before anything changes)
            var carried = plan.ToDictionary(p => p.b, p => o.MoveLand
                ? Enumerable.Range(0, p.b.Bounds.Width * p.b.Bounds.Height).Select(k => map.LandId(p.b.Bounds.X + k % p.b.Bounds.Width, p.b.Bounds.Y + k / p.b.Bounds.Width)).ToArray()
                : null);

            ushort road = HexList.Parse(o.RoadTile).FirstOrDefault();
            if (road != 0)
            {
                for (int y = target.Top; y < target.Bottom; y++)
                {
                    for (int x = target.Left; x < target.Right; x++)
                    {
                        if (!footprints.Any(f => f.Contains(x, y)))
                        {
                            SetLand(x, y, road, map.LandZ(x, y));
                        }
                    }
                }
            }
            foreach ((TownBuilding b, Point at) in plan)
            {
                Rectangle r = b.Bounds;
                int newGround = o.Flatten ? b.GroundZ : map.LandZ(at.X + r.Width / 2, at.Y + r.Height / 2);
                int dz = newGround - b.GroundZ;
                ushort[] carry = carried[b];
                int margin = o.Flatten ? 1 : 0;
                for (int y = -margin; y < r.Height + margin; y++)
                {
                    for (int x = -margin; x < r.Width + margin; x++)
                    {
                        int mx = at.X + x, my = at.Y + y;
                        bool inside = x >= 0 && y >= 0 && x < r.Width && y < r.Height;
                        ushort id = inside && carry != null ? carry[y * r.Width + x] : (land.TryGetValue(my * map.Width + mx, out LandChange lc) ? lc.NewId : map.LandId(mx, my));
                        int z = o.Flatten ? newGround : (land.TryGetValue(my * map.Width + mx, out LandChange lz) ? lz.NewZ : map.LandZ(mx, my));
                        if (inside || o.Flatten)
                        {
                            SetLand(mx, my, id, z);
                        }
                    }
                }
                foreach (StaticItem s in b.Items)
                {
                    statics.Add(MapData.Add(new StaticItem { Id = s.Id, Hue = s.Hue, X = at.X + s.X - r.X, Y = at.Y + s.Y - r.Y, Z = Math.Clamp(s.Z + dz, -128, 127) }));
                }
                log.Add($"{b} → ({at.X}, {at.Y})");
                placedCount++;
            }
            return (statics, land.Values.Where(c => c.NewId != c.OldId || c.NewZ != c.OldZ).ToList());
        }
    }
}

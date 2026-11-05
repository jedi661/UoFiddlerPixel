// /***************************************************************************
//  *
//  * Dragon Map Studio – coast and water tools for the CGI view (brush and marked area)
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>Values of the coast / water tools (defaults = the original map0).</summary>
    public sealed class CoastSettings
    {
        /// <summary>Land z by distance to the water (1, 2, 3 …).</summary>
        public string LandHeights { get; set; } = "-3 -1 0";

        /// <summary>Land higher than this stays (cliff coast).</summary>
        public int MaxLandZ { get; set; } = 10;

        public int FloorZ { get; set; } = -15;
        public int WaterWidth { get; set; } = 6;
        public int SurfaceZ { get; set; } = -5;
        public string FloorTiles { get; set; } = "0064";
        public string WaterStaticIds { get; set; } = "1797 1798 1799 179a 179b 179c";

        /// <summary>Water brush: set the sea floor tile (at <see cref="FloorZ"/>) below the painted water.</summary>
        public bool SetFloor { get; set; } = true;

        public static CoastSettings From(BuildOptions o)
        {
            return new CoastSettings
            {
                LandHeights = o.CoastLandHeights,
                MaxLandZ = o.CoastMaxLandZ,
                FloorZ = o.CoastWaterZ,
                WaterWidth = o.CoastWaterWidth,
                SurfaceZ = o.WaterSurfaceZ,
                FloorTiles = o.SeaFloorTiles,
                WaterStaticIds = o.CoastWaterStaticIds
            };
        }

        public List<int> Heights()
        {
            var list = new List<int>();
            foreach (string t in (LandHeights ?? string.Empty).Split(new[] { ' ', ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                {
                    list.Add(v);
                }
            }
            return list;
        }
    }

    /// <summary>Changes found by a coast / water tool, applied by the caller (one undo step).</summary>
    public sealed class CoastEdit
    {
        public readonly List<(int x, int y, ushort id, int z)> Land = new List<(int, int, ushort, int)>();
        public readonly List<StaticChange> Statics = new List<StaticChange>();

        public int Count => Land.Count + Statics.Count;

        /// <summary>Applies the land as part of a running stroke (BeginLand / EndLand) or as its own step.</summary>
        public void ApplyTo(MapData map)
        {
            if (map.LandStrokeActive)
            {
                foreach (var l in Land)
                {
                    map.SetLand(l.x, l.y, l.id, l.z);
                }
                map.ApplyStatics(Statics);
                return;
            }
            var land = Land.Select(l =>
            {
                int i = l.y * map.Width + l.x;
                return new LandChange { Index = i, OldId = map.Tiles[i], OldZ = map.Z[i], NewId = l.id, NewZ = (sbyte)Math.Clamp(l.z, -128, 127) };
            }).Where(c => c.OldId != c.NewId || c.OldZ != c.NewZ).ToList();
            map.Apply(Statics, land);
        }
    }

    /// <summary>
    /// Coast like the original map (sea floor at -15 with water statics at -5, beach -3 / -1 / 0) for a part of a finished map,
    /// and the water brush (water statics, transparent when tiledata says Translucent).
    /// </summary>
    public static class CoastShaper
    {
        /// <summary>Water cell: wet land tile, sea floor tile, or a water static above the ground.</summary>
        public static bool IsWater(MapData map, int x, int y, HashSet<ushort> floor)
        {
            ushort id = map.LandId(x, y);
            if (floor.Contains(id) || UoArtCache.LandIsWet(id))
            {
                return true;
            }
            int z = map.LandZ(x, y);
            return map.StaticsAt(x, y).Any(s => s.Z > z && UoArtCache.ItemIsWet(s.Id));
        }

        /// <summary>Reshapes the coast inside <paramref name="area"/> (distances are measured beyond it).</summary>
        public static CoastEdit Shape(MapData map, Rectangle area, CoastSettings o, Random rnd)
        {
            var edit = new CoastEdit();
            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            if (area.IsEmpty)
            {
                return edit;
            }
            List<int> heights = o.Heights();
            int maxDist = Math.Max(heights.Count, Math.Max(0, o.WaterWidth));
            Rectangle work = Rectangle.Inflate(area, maxDist + 1, maxDist + 1);
            work.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            int w = work.Width, h = work.Height;
            var floor = new HashSet<ushort>(HexList.Parse(o.FloorTiles));
            ushort[] floorIds = floor.ToArray();
            ushort[] waterIds = HexList.Parse(o.WaterStaticIds);

            var water = new bool[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    water[y * w + x] = IsWater(map, work.X + x, work.Y + y, floor);
                }
            }
            int[] toWater = Distances(water, w, h, true, maxDist);
            int[] toLand = Distances(water, w, h, false, maxDist);

            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    int i = (y - work.Y) * w + x - work.X;
                    int z = map.LandZ(x, y);
                    ushort id = map.LandId(x, y);
                    List<StaticItem> statics = map.StaticsAt(x, y).ToList();
                    if (water[i])
                    {
                        int dl = toLand[i];
                        if (o.WaterWidth <= 0 || dl < 1 || dl > o.WaterWidth)
                        {
                            continue; // open water stays
                        }
                        bool rim = dl == o.WaterWidth;
                        int nz = rim ? o.SurfaceZ : o.FloorZ;
                        ushort nid = floorIds.Length > 0 && !floor.Contains(id) ? floorIds[rnd.Next(floorIds.Length)] : id;
                        if (nz != z || nid != id)
                        {
                            edit.Land.Add((x, y, nid, nz));
                        }
                        bool hasWater = false;
                        foreach (StaticItem s in statics.Where(s => UoArtCache.ItemIsWet(s.Id)))
                        {
                            hasWater = true;
                            if (s.Z != o.SurfaceZ)
                            {
                                edit.Statics.Add(MapData.Modify(s, t => t.Z = o.SurfaceZ));
                            }
                        }
                        if (!hasWater && waterIds.Length > 0)
                        {
                            edit.Statics.Add(MapData.Add(new StaticItem { Id = waterIds[rnd.Next(waterIds.Length)], X = x, Y = y, Z = o.SurfaceZ }));
                        }
                        continue;
                    }

                    int d = toWater[i];
                    if (d < 1 || d > heights.Count || z > o.MaxLandZ)
                    {
                        continue;
                    }
                    int cap = heights[d - 1];
                    int target = d == 1 || z < cap ? cap : cap + (z - cap) / 2;
                    if (target == z)
                    {
                        continue;
                    }
                    edit.Land.Add((x, y, id, target));
                    int dz = target - z;
                    foreach (StaticItem s in statics)
                    {
                        // shore statics stay at the water surface, everything else moves with the ground
                        int nz = UoArtCache.ItemIsWet(s.Id) ? o.SurfaceZ : Math.Clamp(s.Z + dz, -128, 127);
                        if (nz != s.Z)
                        {
                            edit.Statics.Add(MapData.Modify(s, t => t.Z = nz));
                        }
                    }
                }
            }
            return edit;
        }

        /// <summary>
        /// Water brush: a water static on every cell (random from the list) at the surface height, the sea floor below
        /// (when <see cref="CoastSettings.SetFloor"/>). With <paramref name="erase"/> the water statics are removed.
        /// </summary>
        public static CoastEdit Water(MapData map, Rectangle area, CoastSettings o, Random rnd, bool erase)
        {
            var edit = new CoastEdit();
            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            ushort[] waterIds = HexList.Parse(o.WaterStaticIds);
            ushort[] floorIds = HexList.Parse(o.FloorTiles);
            var waterSet = new HashSet<ushort>(waterIds);
            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    List<StaticItem> wet = map.StaticsAt(x, y).Where(s => waterSet.Contains(s.Id) || UoArtCache.ItemIsWet(s.Id)).ToList();
                    if (erase)
                    {
                        edit.Statics.AddRange(wet.Select(MapData.Remove));
                        continue;
                    }
                    if (wet.Count == 0 && waterIds.Length > 0)
                    {
                        edit.Statics.Add(MapData.Add(new StaticItem { Id = waterIds[rnd.Next(waterIds.Length)], X = x, Y = y, Z = o.SurfaceZ }));
                    }
                    if (o.SetFloor && floorIds.Length > 0)
                    {
                        ushort id = map.LandId(x, y);
                        ushort nid = Array.IndexOf(floorIds, id) >= 0 ? id : floorIds[rnd.Next(floorIds.Length)];
                        if (nid != id || map.LandZ(x, y) != o.FloorZ)
                        {
                            edit.Land.Add((x, y, nid, o.FloorZ));
                        }
                    }
                }
            }
            return edit;
        }

        /// <summary>8-neighbour distance of every cell to the nearest water (toWater) or land cell, capped.</summary>
        private static int[] Distances(bool[] water, int w, int h, bool toWater, int max)
        {
            var dist = new int[w * h];
            Array.Fill(dist, int.MaxValue);
            var q = new Queue<int>();
            for (int i = 0; i < dist.Length; i++)
            {
                if (water[i] == toWater)
                {
                    dist[i] = 0;
                    q.Enqueue(i);
                }
            }
            while (q.Count > 0)
            {
                int i = q.Dequeue();
                if (dist[i] >= max)
                {
                    continue;
                }
                int x = i % w, y = i / w;
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                        {
                            continue;
                        }
                        int j = ny * w + nx;
                        if (dist[j] == int.MaxValue)
                        {
                            dist[j] = dist[i] + 1;
                            q.Enqueue(j);
                        }
                    }
                }
            }
            return dist;
        }
    }
}

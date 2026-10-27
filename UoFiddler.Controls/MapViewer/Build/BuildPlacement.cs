/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Ultima;

namespace UoFiddler.Controls.MapViewer.Build
{
    /// <summary>Which point of the template is placed on the target position</summary>
    public enum PlacementAnchor
    {
        Origin,
        UpperLeft,
        Center
    }

    /// <summary>How the base z of a placement is found</summary>
    public enum PlacementHeight
    {
        /// <summary>land z at the anchor + offset</summary>
        GroundAtAnchor,
        GroundLowest,
        GroundHighest,
        GroundAverage,

        /// <summary>the value is the absolute base z</summary>
        Fixed
    }

    /// <summary>Which existing statics below a building are removed</summary>
    public enum ObstacleFilter
    {
        All,
        Plants,
        None
    }

    /// <summary>
    /// A template placed on a map position (only in memory until it is applied to a <see cref="RegionEditSession"/>)
    /// </summary>
    public sealed class BuildPlacement
    {
        private static readonly string[] _plantWords =
        {
            "tree", "bush", "plant", "flower", "grass", "fern", "vine", "shrub", "weed", "mushroom", "cactus", "leaves", "reed", "log", "stump", "rock", "boulder"
        };

        public BuildPlacement(MultiTemplate template, Map map)
        {
            Template = template;
            Map = map;
        }

        public MultiTemplate Template { get; }
        public Map Map { get; }

        /// <summary>anchor position on the map</summary>
        public int X { get; set; }

        public int Y { get; set; }

        public PlacementAnchor Anchor { get; set; } = PlacementAnchor.Center;
        public PlacementHeight HeightMode { get; set; } = PlacementHeight.GroundAtAnchor;

        /// <summary>offset to the ground (ground modes) or absolute base z (<see cref="PlacementHeight.Fixed"/>)</summary>
        public int ZValue { get; set; }

        /// <summary>include parts with flag 0 (invisible in multi.mul)</summary>
        public bool IncludeInvisible { get; set; }

        public int BaseZ { get; private set; }
        public int OffsetX { get; private set; }
        public int OffsetY { get; private set; }
        public int GroundMin { get; private set; }
        public int GroundMax { get; private set; }
        public int GroundAverage { get; private set; }
        public Rectangle Bounds { get; private set; }
        public HashSet<Point> Footprint { get; } = new HashSet<Point>();
        public List<MapStatic> Statics { get; } = new List<MapStatic>();
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();

        public bool IsValid => Errors.Count == 0 && Statics.Count > 0;

        /// <summary>Recalculates base z, footprint, statics and the checks</summary>
        public void Update(Rectangle? viewArea = null)
        {
            Errors.Clear();
            Warnings.Clear();
            Footprint.Clear();
            Statics.Clear();

            List<MultiComponent> parts = Template.Components.Where(c => IncludeInvisible || c.IsVisible).ToList();
            if (parts.Count == 0)
            {
                Errors.Add("The template has no (visible) parts.");
                Bounds = Rectangle.Empty;
                return;
            }

            int minX = parts.Min(c => c.X), maxX = parts.Max(c => c.X);
            int minY = parts.Min(c => c.Y), maxY = parts.Max(c => c.Y);
            (OffsetX, OffsetY) = Anchor switch
            {
                PlacementAnchor.UpperLeft => (-minX, -minY),
                PlacementAnchor.Center => (-((minX + maxX) / 2), -((minY + maxY) / 2)),
                _ => (0, 0)
            };

            int outside = 0;
            foreach (MultiComponent c in parts)
            {
                var p = new Point(X + OffsetX + c.X, Y + OffsetY + c.Y);
                if (p.X < 0 || p.Y < 0 || p.X >= Map.Width || p.Y >= Map.Height)
                {
                    outside++;
                }
                else
                {
                    Footprint.Add(p);
                }
            }

            Bounds = Rectangle.FromLTRB(X + OffsetX + minX, Y + OffsetY + minY, X + OffsetX + maxX + 1, Y + OffsetY + maxY + 1);

            // ground below the footprint
            int gMin = int.MaxValue, gMax = int.MinValue;
            long sum = 0;
            foreach (Point p in Footprint)
            {
                int z = Map.Tiles.GetLandTile(p.X, p.Y).Z;
                gMin = Math.Min(gMin, z);
                gMax = Math.Max(gMax, z);
                sum += z;
            }

            if (Footprint.Count == 0)
            {
                gMin = gMax = 0;
            }

            GroundMin = gMin;
            GroundMax = gMax;
            GroundAverage = Footprint.Count == 0 ? 0 : (int)Math.Round(sum / (double)Footprint.Count);

            int anchorGround = X >= 0 && Y >= 0 && X < Map.Width && Y < Map.Height ? Map.Tiles.GetLandTile(X, Y).Z : 0;
            BaseZ = HeightMode switch
            {
                PlacementHeight.Fixed => ZValue,
                PlacementHeight.GroundLowest => GroundMin + ZValue,
                PlacementHeight.GroundHighest => GroundMax + ZValue,
                PlacementHeight.GroundAverage => GroundAverage + ZValue,
                _ => anchorGround + ZValue
            };

            int clamped = 0;
            foreach (MultiComponent c in parts)
            {
                int x = X + OffsetX + c.X, y = Y + OffsetY + c.Y;
                if (x < 0 || y < 0 || x >= Map.Width || y >= Map.Height)
                {
                    continue;
                }

                int z = BaseZ + c.Z;
                if (z < sbyte.MinValue || z > sbyte.MaxValue)
                {
                    clamped++;
                    z = Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, z));
                }

                Statics.Add(new MapStatic(x, y, c.ItemId, (sbyte)z, (short)c.Hue));
            }

            if (outside > 0)
            {
                Errors.Add($"{outside} parts are outside of the map.");
            }

            if (clamped > 0)
            {
                Errors.Add($"{clamped} parts are outside of z -128..127 (template z {Template.MinZ}..{Template.MaxZ}, base z {BaseZ}). Lower the base z or use \"Normalize Z\".");
            }

            if (GroundMax - GroundMin > 10)
            {
                Warnings.Add($"Uneven ground: z {GroundMin}..{GroundMax}. Flatten the ground or add a foundation.");
            }

            if (BaseZ < GroundMax - 2)
            {
                Warnings.Add($"Base z {BaseZ} is below the highest ground point {GroundMax}, parts can disappear in the ground.");
            }

            if (viewArea.HasValue && !viewArea.Value.Contains(Bounds))
            {
                Warnings.Add("The building is partly outside of the shown region (it is placed anyway).");
            }
        }

        /// <summary>
        /// Existing statics (map + changes in memory) on the footprint
        /// </summary>
        public List<MapStatic> FindObstacles(RegionEditSession session)
        {
            if (Bounds.IsEmpty)
            {
                return new List<MapStatic>();
            }

            return session.GetStaticsInArea(Bounds.Left, Bounds.Top, Bounds.Right - 1, Bounds.Bottom - 1)
                .Where(s => Footprint.Contains(new Point(s.X, s.Y)))
                .ToList();
        }

        /// <summary>
        /// True if an item looks like a plant/tree/rock (TileData foliage flag or name)
        /// </summary>
        public static bool IsPlant(ushort id)
        {
            if (TileData.ItemTable == null || id >= TileData.ItemTable.Length)
            {
                return false;
            }

            ItemData data = TileData.ItemTable[id];
            if ((data.Flags & TileFlag.Foliage) != 0)
            {
                return true;
            }

            string name = data.Name?.ToLowerInvariant() ?? string.Empty;
            return _plantWords.Any(w => name.Contains(w));
        }

        public static bool MatchesFilter(MapStatic s, ObstacleFilter filter)
        {
            return filter switch
            {
                ObstacleFilter.All => true,
                ObstacleFilter.Plants => IsPlant(s.Id),
                _ => false
            };
        }

        /// <summary>
        /// Applies the placement to the session as one undo step: removes the given obstacles,
        /// flattens the ground (optional) and adds the statics. Returns the number of added statics.
        /// </summary>
        public int Apply(RegionEditSession session, IEnumerable<MapStatic> removeObstacles, bool flattenGround)
        {
            using (session.Batch($"Place {Template.Name}"))
            {
                if (removeObstacles != null)
                {
                    session.RemoveStatics(removeObstacles, $"Place {Template.Name}");
                }

                if (flattenGround)
                {
                    session.FlattenLand(Footprint.Select(p => (p.X, p.Y)), BaseZ);
                }

                return session.AddStatics(Statics, $"Place {Template.Name}");
            }
        }

        /// <summary>
        /// Takes statics of a map area as new template (relative to the upper left corner and the given base z)
        /// </summary>
        public static MultiTemplate FromArea(RegionEditSession session, Rectangle area, int minZ, int maxZ, int baseZ, string name)
        {
            var t = new MultiTemplate { Name = name, Source = "Map area" };
            foreach (MapStatic s in session.GetStaticsInArea(area.Left, area.Top, area.Right - 1, area.Bottom - 1))
            {
                if (s.Z < minZ || s.Z > maxZ)
                {
                    continue;
                }

                t.Components.Add(new MultiComponent(s.Id, s.X - area.Left, s.Y - area.Top, s.Z - baseZ, (ushort)s.Hue, 1));
            }

            return t;
        }
    }
}

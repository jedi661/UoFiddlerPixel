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
using UoFiddler.Controls.MapViewer.Build;

namespace UoFiddler.Controls.MapViewer.Editor
{
    /// <summary>How the height of a pasted clip is adapted to the target</summary>
    public enum ClipHeightMode
    {
        /// <summary>z as on the source map (+ offset)</summary>
        Keep,

        /// <summary>moved so that the border fits the target ground on average (+ offset)</summary>
        MatchEdges
    }

    public sealed class ClipPasteOptions
    {
        public bool IncludeLand { get; set; } = true;
        public bool IncludeStatics { get; set; } = true;
        public ClipHeightMode HeightMode { get; set; } = ClipHeightMode.MatchEdges;

        /// <summary>additional z offset</summary>
        public int ZOffset { get; set; }

        /// <summary>cells at the border that blend from the target height to the clip height (0 = hard edge)</summary>
        public int BlendWidth { get; set; } = 3;

        /// <summary>statics keep their height above the ground (follow a changed ground)</summary>
        public bool StaticsFollowGround { get; set; } = true;

        /// <summary>existing statics of the target area that are removed</summary>
        public ObstacleFilter RemoveExisting { get; set; } = ObstacleFilter.All;

        /// <summary>item ids of the clip that are not pasted</summary>
        public HashSet<ushort> ExcludedIds { get; set; } = new HashSet<ushort>();

        public int MinZ { get; set; } = sbyte.MinValue;
        public int MaxZ { get; set; } = sbyte.MaxValue;

        /// <summary>height jumps at the border above this are reported</summary>
        public int EdgeThreshold { get; set; } = 10;

        public ClipPasteOptions Clone()
        {
            ClipPasteOptions copy = (ClipPasteOptions)MemberwiseClone();
            copy.ExcludedIds = new HashSet<ushort>(ExcludedIds);
            return copy;
        }
    }

    /// <summary>Everything that would be changed when pasting a clip at a position</summary>
    public sealed class ClipPasteResult
    {
        public Rectangle TargetArea { get; set; }
        public int AppliedOffset { get; set; }
        public List<(int X, int Y, ushort Id, sbyte Z)> Land { get; } = new List<(int, int, ushort, sbyte)>();
        public List<MapStatic> Statics { get; } = new List<MapStatic>();
        public List<MapStatic> RemovedStatics { get; } = new List<MapStatic>();

        /// <summary>border cells with a height jump to the target ground</summary>
        public List<Point> EdgeProblems { get; } = new List<Point>();

        public int MaxEdgeJump { get; set; }

        /// <summary>parts outside of z -128..127 (were clamped)</summary>
        public int Clamped { get; set; }

        /// <summary>cells outside of the target map (skipped)</summary>
        public int Outside { get; set; }
    }

    /// <summary>
    /// Calculates and applies the paste of a <see cref="MapClip"/> on a target map
    /// </summary>
    public static class ClipPaster
    {
        /// <summary>
        /// The z offset that is used for a position (fast: reads only the border cells; for the ghost image while moving)
        /// </summary>
        public static int EstimateOffset(MapClip clip, Map target, int tx, int ty, ClipPasteOptions o)
        {
            return o.HeightMode == ClipHeightMode.MatchEdges ? o.ZOffset + EdgeDifference(clip, target, tx, ty) : o.ZOffset;
        }

        private static int EdgeDifference(MapClip clip, Map target, int tx, int ty)
        {
            TileMatrix tiles = target.Tiles;
            long sum = 0;
            int count = 0;
            int w = clip.Width, h = clip.Height;
            for (int y = 0; y < h; y++)
            {
                bool edgeRow = y == 0 || y == h - 1;
                for (int x = 0; x < w; x += edgeRow ? 1 : Math.Max(1, w - 1))
                {
                    int mx = tx + x, my = ty + y;
                    if (mx < 0 || my < 0 || mx >= target.Width || my >= target.Height)
                    {
                        continue;
                    }

                    sum += tiles.GetLandTile(mx, my).Z - clip.GetLandZ(x, y);
                    count++;
                }
            }

            return count == 0 ? 0 : (int)Math.Round(sum / (double)count);
        }

        public static ClipPasteResult Compute(MapClip clip, Map target, int tx, int ty, ClipPasteOptions o, RegionEditSession session)
        {
            var result = new ClipPasteResult();
            TileMatrix tiles = target.Tiles;
            int w = clip.Width, h = clip.Height;

            var targetZ = new int?[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int mx = tx + x, my = ty + y;
                    if (mx >= 0 && my >= 0 && mx < target.Width && my < target.Height)
                    {
                        targetZ[(y * w) + x] = tiles.GetLandTile(mx, my).Z;
                    }
                    else
                    {
                        result.Outside++;
                    }
                }
            }

            // offset: average difference at the border (only cells on the map)
            int offset = o.ZOffset;
            if (o.HeightMode == ClipHeightMode.MatchEdges)
            {
                offset += EdgeDifference(clip, target, tx, ty);
            }

            result.AppliedOffset = offset;
            result.TargetArea = Rectangle.Intersect(new Rectangle(tx, ty, w, h), new Rectangle(0, 0, target.Width, target.Height));

            // new ground height of every cell
            var newZ = new int[w * h];
            int blend = Math.Max(0, o.BlendWidth);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int i = (y * w) + x;
                    if (!targetZ[i].HasValue)
                    {
                        continue;
                    }

                    if (!o.IncludeLand)
                    {
                        newZ[i] = targetZ[i].Value;
                        continue;
                    }

                    int z = clip.GetLandZ(x, y) + offset;
                    int d = Math.Min(Math.Min(x, y), Math.Min(w - 1 - x, h - 1 - y));
                    if (blend > 0 && d < blend)
                    {
                        double t = (d + 1) / (double)(blend + 1);
                        z = (int)Math.Round(targetZ[i].Value + ((z - targetZ[i].Value) * t));
                    }

                    if (z < sbyte.MinValue || z > sbyte.MaxValue)
                    {
                        result.Clamped++;
                        z = Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, z));
                    }

                    newZ[i] = z;
                    result.Land.Add((tx + x, ty + y, clip.GetLandId(x, y), (sbyte)z));
                }
            }

            // statics
            if (o.IncludeStatics)
            {
                foreach (ClipStatic s in clip.Statics)
                {
                    if (s.X < 0 || s.Y < 0 || s.X >= w || s.Y >= h || o.ExcludedIds.Contains(s.Id) || s.Z < o.MinZ || s.Z > o.MaxZ)
                    {
                        continue;
                    }

                    int i = (s.Y * w) + s.X;
                    if (!targetZ[i].HasValue)
                    {
                        continue;
                    }

                    int z = o.StaticsFollowGround
                        ? s.Z - clip.GetLandZ(s.X, s.Y) + newZ[i]
                        : s.Z + offset;

                    if (z < sbyte.MinValue || z > sbyte.MaxValue)
                    {
                        result.Clamped++;
                        z = Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, z));
                    }

                    result.Statics.Add(new MapStatic(tx + s.X, ty + s.Y, s.Id, (sbyte)z, s.Hue));
                }
            }

            // statics of the target area that are removed
            if (o.RemoveExisting != ObstacleFilter.None && session != null && !result.TargetArea.IsEmpty)
            {
                Rectangle a = result.TargetArea;
                result.RemovedStatics.AddRange(session.GetStaticsInArea(a.Left, a.Top, a.Right - 1, a.Bottom - 1)
                    .Where(s => BuildPlacement.MatchesFilter(s, o.RemoveExisting)));
            }

            // height jumps at the border to the cells outside of the clip
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (x != 0 && y != 0 && x != w - 1 && y != h - 1)
                    {
                        continue;
                    }

                    int i = (y * w) + x;
                    if (!targetZ[i].HasValue)
                    {
                        continue;
                    }

                    int jump = 0;
                    foreach ((int dx, int dy) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx >= 0 && ny >= 0 && nx < w && ny < h)
                        {
                            continue; // inside of the clip
                        }

                        int mx = tx + nx, my = ty + ny;
                        if (mx < 0 || my < 0 || mx >= target.Width || my >= target.Height)
                        {
                            continue;
                        }

                        jump = Math.Max(jump, Math.Abs(newZ[i] - tiles.GetLandTile(mx, my).Z));
                    }

                    result.MaxEdgeJump = Math.Max(result.MaxEdgeJump, jump);
                    if (jump > o.EdgeThreshold)
                    {
                        result.EdgeProblems.Add(new Point(tx + x, ty + y));
                    }
                }
            }

            tiles.CloseStreams();
            return result;
        }

        /// <summary>
        /// Applies a calculated paste as one undo step
        /// </summary>
        public static void Apply(ClipPasteResult result, RegionEditSession session, string name)
        {
            using (session.Batch(name))
            {
                if (result.RemovedStatics.Count > 0)
                {
                    session.RemoveStatics(result.RemovedStatics, name);
                }

                if (result.Land.Count > 0)
                {
                    session.SetLandTiles(result.Land, name);
                }

                if (result.Statics.Count > 0)
                {
                    session.AddStatics(result.Statics, name);
                }
            }
        }
    }
}

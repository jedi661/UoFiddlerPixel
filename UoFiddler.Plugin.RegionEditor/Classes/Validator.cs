/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Checks the regions of a project and fixes simple problems automatically.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace UoFiddler.Plugin.RegionEditor.Classes
{
    public enum IssueSeverity
    {
        Error,
        Warning,
        Info
    }

    /// <summary>Access to the map (size, water) – implemented with the Ultima SDK by the UI, replaceable in tests</summary>
    public interface IMapInfo
    {
        /// <summary>size of a map in cells, Size.Empty = unknown</summary>
        Size GetSize(int mapId);

        /// <summary>true when the land tile (and no walkable static) at x/y is water</summary>
        bool IsWater(int mapId, int x, int y);
    }

    public sealed class RegionIssue
    {
        public IssueSeverity Severity { get; set; }
        public string RegionId { get; set; }
        public string RegionName { get; set; }
        public int MapId { get; set; }

        /// <summary>key of the text in <see cref="Loc"/></summary>
        public string Key { get; set; }
        public object[] Args { get; set; } = Array.Empty<object>();

        /// <summary>cell to show on the map</summary>
        public Point? Location { get; set; }

        /// <summary>automatic fix (null = none); returns true when something was changed</summary>
        public Func<bool> Fix { get; set; }

        public string Message => Loc.F(Key, Args);

        public override string ToString() => $"{Severity}: {RegionName}: {Message}";
    }

    public static class Validator
    {
        public static List<RegionIssue> Check(RegionProject project, IMapInfo map)
        {
            var issues = new List<RegionIssue>();
            foreach (FacetRegions facet in project.Facets)
            {
                Size size = map?.GetSize(facet.MapId) ?? Size.Empty;
                var names = new Dictionary<string, RegionNode>(StringComparer.OrdinalIgnoreCase);
                foreach (RegionNode top in facet.Regions)
                {
                    CheckRegion(issues, facet, top, null, size, map, names);
                }

                CheckOverlaps(issues, facet);
            }

            return issues
                .OrderBy(i => i.Severity)
                .ThenBy(i => i.MapId)
                .ThenBy(i => i.RegionName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static RegionIssue Add(List<RegionIssue> list, IssueSeverity severity, FacetRegions facet, RegionNode region, string key, params object[] args)
        {
            var issue = new RegionIssue
            {
                Severity = severity,
                RegionId = region?.Id,
                RegionName = region?.DisplayName ?? facet.Name,
                MapId = facet.MapId,
                Key = key,
                Args = args ?? Array.Empty<object>(),
                Location = region == null || region.Bounds.IsEmpty ? (Point?)null : Center(region.Bounds)
            };
            list.Add(issue);
            return issue;
        }

        private static Point Center(Rectangle r) => new Point(r.X + (r.Width / 2), r.Y + (r.Height / 2));

        private static void CheckRegion(List<RegionIssue> issues, FacetRegions facet, RegionNode region, RegionNode parent, Size size,
            IMapInfo map, Dictionary<string, RegionNode> names)
        {
            // ── name ──
            if (string.IsNullOrWhiteSpace(region.Name))
            {
                RegionNode r = region;
                Add(issues, IssueSeverity.Error, facet, region, "chk.noname").Fix = () =>
                {
                    r.Name = $"{r.DisplayType}_{r.Id.Substring(0, 6)}";
                    return true;
                };
            }
            else if (names.TryGetValue(region.Name.Trim(), out RegionNode other) && other != region)
            {
                RegionNode r = region;
                Add(issues, IssueSeverity.Warning, facet, region, "chk.dupname", region.Name).Fix = () =>
                {
                    int n = 2;
                    string baseName = r.Name.Trim();
                    while (names.ContainsKey($"{baseName} {n}"))
                    {
                        n++;
                    }

                    r.Name = $"{baseName} {n}";
                    return true;
                };
            }
            else
            {
                names[region.Name.Trim()] = region;
            }

            if (!string.IsNullOrEmpty(region.Name) && (region.Name.Contains('"') || region.Name.Contains('<') || region.Name.Contains('&')))
            {
                Add(issues, IssueSeverity.Info, facet, region, "chk.namechars");
            }

            // ── area ──
            if (region.Rects.Count == 0 && region.Polygons.Count == 0)
            {
                Add(issues, region.Children.Count > 0 ? IssueSeverity.Info : IssueSeverity.Warning, facet, region, "chk.noarea");
            }

            foreach (RegionRect rect in region.Rects.ToList())
            {
                RegionRect r = rect;
                RegionNode owner = region;
                if (rect.Width <= 0 || rect.Height <= 0)
                {
                    Add(issues, IssueSeverity.Error, facet, region, "chk.zerosize", rect.ToString()).Fix = () =>
                    {
                        if (r.Width == 0 || r.Height == 0)
                        {
                            return owner.Rects.Remove(r);
                        }

                        r.Normalize();
                        return true;
                    };
                    continue;
                }

                if (!size.IsEmpty && (rect.X < 0 || rect.Y < 0 || rect.X + rect.Width > size.Width || rect.Y + rect.Height > size.Height))
                {
                    Size s = size;
                    Add(issues, IssueSeverity.Error, facet, region, "chk.outside", rect.ToString(), size.Width, size.Height).Fix = () =>
                    {
                        Rectangle clipped = Rectangle.Intersect(r.Bounds, new Rectangle(0, 0, s.Width, s.Height));
                        if (clipped.Width <= 0 || clipped.Height <= 0)
                        {
                            return owner.Rects.Remove(r);
                        }

                        r.X = clipped.X;
                        r.Y = clipped.Y;
                        r.Width = clipped.Width;
                        r.Height = clipped.Height;
                        return true;
                    };
                }

                if (rect.ZMin.HasValue && rect.ZMax.HasValue && rect.ZMin > rect.ZMax)
                {
                    Add(issues, IssueSeverity.Error, facet, region, "chk.zorder", rect.ToString()).Fix = () =>
                    {
                        (r.ZMin, r.ZMax) = (r.ZMax, r.ZMin);
                        return true;
                    };
                }
            }

            foreach (RegionPolygon poly in region.Polygons)
            {
                if (poly.Points.Count < 3)
                {
                    RegionPolygon p = poly;
                    RegionNode owner = region;
                    Add(issues, IssueSeverity.Error, facet, region, "chk.polypoints").Fix = () => owner.Polygons.Remove(p);
                }
            }

            // identical rectangles
            var seen = new HashSet<(int, int, int, int, int?, int?)>();
            foreach (RegionRect rect in region.Rects.ToList())
            {
                if (!seen.Add((rect.X, rect.Y, rect.Width, rect.Height, rect.ZMin, rect.ZMax)))
                {
                    RegionRect r = rect;
                    RegionNode owner = region;
                    Add(issues, IssueSeverity.Warning, facet, region, "chk.duprect", rect.ToString()).Fix = () => owner.Rects.Remove(r);
                }
            }

            if (region.Rects.Count > 1)
            {
                List<RegionRect> optimized = Geometry.Optimize(region.Rects);
                if (optimized.Count < region.Rects.Count)
                {
                    RegionNode owner = region;
                    Add(issues, IssueSeverity.Info, facet, region, "chk.optimize", region.Rects.Count, optimized.Count).Fix = () =>
                    {
                        owner.Rects = Geometry.Optimize(owner.Rects);
                        return true;
                    };
                }
            }

            if (region.ZMin.HasValue && region.ZMax.HasValue && region.ZMin > region.ZMax)
            {
                RegionNode owner = region;
                Add(issues, IssueSeverity.Error, facet, region, "chk.zorder", "zrange").Fix = () =>
                {
                    (owner.ZMin, owner.ZMax) = (owner.ZMax, owner.ZMin);
                    return true;
                };
            }

            // ── child outside the parent ──
            if (parent != null && (region.Rects.Count > 0 || region.Polygons.Count > 0))
            {
                List<RegionRect> parentRects = Geometry.AllRects(parent);
                if (parentRects.Count > 0 && !Geometry.Covers(parentRects, Geometry.AllRects(region)))
                {
                    Add(issues, IssueSeverity.Warning, facet, region, "chk.childoutside", parent.DisplayName);
                }
            }

            // ── priority / values ──
            if (region.Priority < 0 || region.Priority > 255)
            {
                RegionNode owner = region;
                Add(issues, IssueSeverity.Warning, facet, region, "chk.priority", region.Priority).Fix = () =>
                {
                    owner.Priority = Math.Max(0, Math.Min(255, owner.Priority));
                    return true;
                };
            }

            if (!string.IsNullOrEmpty(region.Music) && !RegionCatalog.IsKnownMusic(region.Music))
            {
                Add(issues, IssueSeverity.Warning, facet, region, "chk.music", region.Music);
            }

            if (!string.IsNullOrEmpty(region.Type) && !RegionCatalog.Types.Contains(region.Type))
            {
                Add(issues, IssueSeverity.Info, facet, region, "chk.type", region.Type);
            }

            // ── go location / entrance ──
            if (region.Go != null && (region.Rects.Count > 0 || region.Polygons.Count > 0) && !region.Contains(region.Go.X, region.Go.Y))
            {
                RegionNode owner = region;
                Add(issues, IssueSeverity.Warning, facet, region, "chk.gooutside", region.Go.ToString()).Fix = () =>
                {
                    Point c = Center(owner.Rects.Count > 0 ? owner.Rects[0].Bounds : owner.Bounds);
                    owner.Go.X = c.X;
                    owner.Go.Y = c.Y;
                    return true;
                };
            }

            if (region.Go != null && map != null && SafeWater(map, facet.MapId, region.Go.X, region.Go.Y))
            {
                Add(issues, IssueSeverity.Warning, facet, region, "chk.gowater", region.Go.ToString());
            }

            if (region.Entrance != null)
            {
                int mapId = string.IsNullOrEmpty(region.Entrance.Map) ? facet.MapId : RegionCatalog.FacetMapId(region.Entrance.Map);
                if (mapId >= 0 && map != null && SafeWater(map, mapId, region.Entrance.X, region.Entrance.Y))
                {
                    Add(issues, IssueSeverity.Warning, facet, region, "chk.entrancewater", region.Entrance.ToString());
                }

                if (!string.Equals(region.Type, "DungeonRegion", StringComparison.OrdinalIgnoreCase))
                {
                    Add(issues, IssueSeverity.Info, facet, region, "chk.entrancetype");
                }
            }
            else if (string.Equals(region.Type, "DungeonRegion", StringComparison.OrdinalIgnoreCase) && parent == null)
            {
                Add(issues, IssueSeverity.Info, facet, region, "chk.noentrance");
            }

            if (region.Polygons.Count > 0)
            {
                Add(issues, IssueSeverity.Info, facet, region, "chk.polyconvert", region.Polygons.Count);
            }

            foreach (RegionNode child in region.Children)
            {
                CheckRegion(issues, facet, child, region, size, map, names);
            }
        }

        private static bool SafeWater(IMapInfo map, int mapId, int x, int y)
        {
            try
            {
                return map.IsWater(mapId, x, y);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>top level regions with the same priority that overlap (the server picks one of them at random)</summary>
        private static void CheckOverlaps(List<RegionIssue> issues, FacetRegions facet)
        {
            List<RegionNode> top = facet.Regions.Where(r => !r.Hidden).ToList();
            var rects = top.ToDictionary(r => r.Id, Geometry.AllRects);
            for (int i = 0; i < top.Count; i++)
            {
                for (int j = i + 1; j < top.Count; j++)
                {
                    if (top[i].Priority != top[j].Priority || !top[i].Bounds.IntersectsWith(top[j].Bounds))
                    {
                        continue;
                    }

                    long area = Geometry.OverlapArea(rects[top[i].Id], rects[top[j].Id]);
                    if (area > 0)
                    {
                        Add(issues, IssueSeverity.Warning, facet, top[j], "chk.overlap", top[i].DisplayName, area, top[j].Priority);
                    }
                }
            }
        }

        /// <summary>Runs all automatic fixes, returns the number of fixed issues</summary>
        public static int FixAll(IEnumerable<RegionIssue> issues)
        {
            int count = 0;
            foreach (RegionIssue issue in issues.Where(i => i.Fix != null))
            {
                try
                {
                    if (issue.Fix())
                    {
                        count++;
                    }
                }
                catch (Exception)
                {
                    // the region was changed by another fix – ignore
                }
            }

            return count;
        }
    }
}

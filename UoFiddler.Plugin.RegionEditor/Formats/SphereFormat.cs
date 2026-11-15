/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Sphere: maps\mapN\mapN_areas.scp (AREADEF) and mapN_rooms.scp (ROOMDEF)
 *
 *  [AREADEF a_britain]
 *  NAME=Britain
 *  GROUP=Britain
 *  EVENTS=r_default
 *  FLAGS=REGION_FLAG_GUARDED|REGION_FLAG_NOBUILDING
 *  P=1495,1629,10,0                 (x, y, z, map)
 *  RECT=1416,1498,1740,1777,0       (x1, y1, x2, y2, map – x2/y2 = end, not included)
 *
 * Sphere has no nested regions: children are written as own AREADEFs.
 * All other keys (TAG.*, RESOURCES …) are kept in the region properties.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UoFiddler.Plugin.RegionEditor.Classes;

namespace UoFiddler.Plugin.RegionEditor.Formats
{
    public static class SphereFormat
    {
        public const string KeyDefName = "DEFNAME";
        public const string KeyKind = "SPHERE_KIND";
        public const string KeyFlags = "FLAGS";
        public const string KeyEvents = "EVENTS";
        public const string KeyGroup = "GROUP";

        public const string FlagGuarded = "REGION_FLAG_GUARDED";
        public const string FlagNoBuilding = "REGION_FLAG_NOBUILDING";
        public const string FlagUnderground = "REGION_FLAG_UNDERGROUND";
        public const string FlagSafe = "REGION_FLAG_SAFE";
        public const string FlagNoPvP = "REGION_ANTIMAGIC_DAMAGE";

        /// <summary>keys handled by the importer itself (the others are kept as properties)</summary>
        private static readonly HashSet<string> _handled = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "NAME", "P", "RECT" };

        #region Import

        public static RegionProject Load(string fileName)
        {
            RegionProject project = Parse(File.ReadAllText(fileName), MapFromFileName(fileName));
            project.SourceFile = fileName;
            project.SourceFormat = "sphere";
            return project;
        }

        /// <summary>map number from "map0_areas.scp" or a "map2" folder, 0 when unknown</summary>
        public static int MapFromFileName(string fileName)
        {
            Match m = Regex.Match(fileName ?? string.Empty, @"map(\d+)", RegexOptions.IgnoreCase | RegexOptions.RightToLeft);
            return m.Success ? int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : 0;
        }

        public static RegionProject Parse(string text, int defaultMap)
        {
            var project = new RegionProject();
            RegionNode current = null;
            int currentMap = defaultMap;
            bool mapFromRect = false;

            void Finish()
            {
                if (current == null)
                {
                    return;
                }

                FacetRegions facet = project.GetFacet(currentMap, RegionCatalog.FacetName(currentMap, null), true);
                facet.Regions.Add(current);
                current = null;
            }

            foreach (string raw in text.Replace("\r\n", "\n").Split('\n'))
            {
                string line = StripComment(raw).Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                if (line.StartsWith("[", StringComparison.Ordinal))
                {
                    Finish();
                    Match header = Regex.Match(line, @"^\[\s*(AREADEF|ROOMDEF|AREA|ROOM)\s*([^\]]*)\]", RegexOptions.IgnoreCase);
                    if (!header.Success)
                    {
                        continue;
                    }

                    string kind = header.Groups[1].Value.ToUpperInvariant();
                    string defName = header.Groups[2].Value.Trim();
                    current = new RegionNode { Name = defName, Priority = 50 };
                    current.Properties[KeyKind] = kind.StartsWith("ROOM", StringComparison.Ordinal) ? "ROOMDEF" : "AREADEF";
                    if (kind.EndsWith("DEF", StringComparison.Ordinal))
                    {
                        current.Properties[KeyDefName] = defName;
                    }

                    currentMap = defaultMap;
                    mapFromRect = false;
                    continue;
                }

                if (current == null)
                {
                    continue;
                }

                int eq = line.IndexOf('=');
                string key = (eq < 0 ? line : line.Substring(0, eq)).Trim();
                string value = eq < 0 ? string.Empty : line.Substring(eq + 1).Trim();

                switch (key.ToUpperInvariant())
                {
                    case "NAME":
                        current.Name = value;
                        break;
                    case "P":
                        {
                            int[] v = Numbers(value);
                            if (v.Length >= 2)
                            {
                                current.Go = new RegionPoint { X = v[0], Y = v[1], Z = v.Length > 2 ? v[2] : (int?)null };
                            }

                            if (v.Length > 3 && !mapFromRect)
                            {
                                currentMap = v[3];
                            }

                            break;
                        }
                    case "RECT":
                        {
                            int[] v = Numbers(value);
                            if (v.Length >= 4)
                            {
                                current.Rects.Add(new RegionRect { X = v[0], Y = v[1], Width = v[2] - v[0], Height = v[3] - v[1] });
                                if (v.Length > 4)
                                {
                                    currentMap = v[4];
                                    mapFromRect = true;
                                }
                            }

                            break;
                        }
                    default:
                        {
                            string upper = key.ToUpperInvariant();
                            current.Properties[upper] = upper == KeyEvents && current.Properties.TryGetValue(upper, out string old) && old.Length > 0
                                ? old + "," + value
                                : value;
                            break;
                        }
                }
            }

            Finish();

            foreach (RegionNode region in project.AllRegions())
            {
                region.Type = TypeFromFlags(region);
            }

            return project;
        }

        private static string StripComment(string line)
        {
            int i = line.IndexOf("//", StringComparison.Ordinal);
            return i < 0 ? line : line.Substring(0, i);
        }

        private static int[] Numbers(string value)
        {
            return value.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : 0)
                .ToArray();
        }

        private static HashSet<string> Flags(RegionNode region)
        {
            region.Properties.TryGetValue(KeyFlags, out string flags);
            return new HashSet<string>((flags ?? string.Empty).Split(new[] { '|', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim().ToUpperInvariant()));
        }

        /// <summary>region type of the editor for the Sphere flags (shown colour, ServUO export)</summary>
        public static string TypeFromFlags(RegionNode region)
        {
            HashSet<string> flags = Flags(region);
            if (region.Properties.TryGetValue(KeyKind, out string kind) && kind == "ROOMDEF")
            {
                return flags.Contains(FlagGuarded) ? "TownRegion" : "Region";
            }

            if (flags.Contains(FlagUnderground))
            {
                return "DungeonRegion";
            }

            if (flags.Contains(FlagGuarded))
            {
                return "TownRegion";
            }

            if (flags.Contains(FlagNoBuilding))
            {
                return "NoHousingRegion";
            }

            return string.Empty;
        }

        #endregion

        #region Export

        public sealed class Options
        {
            /// <summary>only regions of this map (-1 = all maps into one file)</summary>
            public int MapId { get; set; } = -1;

            public bool SkipHidden { get; set; }

            /// <summary>EVENTS for new regions without own EVENTS (empty = none)</summary>
            public string DefaultEvents { get; set; } = string.Empty;
        }

        public static string ToText(RegionProject project, Options options)
        {
            var sb = new StringBuilder();
            sb.AppendLine("//****************************************************************************");
            sb.AppendLine("// Regions – written by the UoFiddler region editor");
            sb.AppendLine("// RECT=x1,y1,x2,y2,map (x2/y2 = end), P=x,y,z,map");
            sb.AppendLine("//****************************************************************************");
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (FacetRegions facet in project.Facets.Where(f => options.MapId < 0 || f.MapId == options.MapId).OrderBy(f => f.MapId))
            {
                sb.AppendLine();
                sb.AppendLine($"// {facet.Name} (map {facet.MapId})");
                foreach (RegionNode top in facet.Regions)
                {
                    WriteRegion(sb, facet, top, options, usedNames, null);
                }
            }

            sb.AppendLine();
            sb.AppendLine("[EOF]");
            return sb.ToString();
        }

        private static void WriteRegion(StringBuilder sb, FacetRegions facet, RegionNode region, Options options, HashSet<string> usedNames, RegionNode parent)
        {
            if (options.SkipHidden && region.Hidden)
            {
                return;
            }

            List<RegionRect> rects = Geometry.AllRects(region);
            if (rects.Count > 0)
            {
                string kind = region.Properties.TryGetValue(KeyKind, out string k) && k == "ROOMDEF" ? "ROOMDEF" : "AREADEF";
                string defName = region.Properties.TryGetValue(KeyDefName, out string d) && !string.IsNullOrWhiteSpace(d) ? d : DefName(region.Name);
                string unique = defName;
                for (int n = 2; !usedNames.Add(unique); n++)
                {
                    unique = $"{defName}_{n}";
                }

                if (!string.IsNullOrWhiteSpace(region.LeadingComment))
                {
                    foreach (string line in region.LeadingComment.Split('\n'))
                    {
                        sb.AppendLine("// " + line.Trim());
                    }
                }

                sb.AppendLine();
                sb.AppendLine($"[{kind} {unique}]");

                string events = Get(region, KeyEvents) ?? (string.IsNullOrEmpty(options.DefaultEvents) ? null : options.DefaultEvents);
                if (!string.IsNullOrEmpty(events))
                {
                    sb.AppendLine($"EVENTS={events}");
                }

                sb.AppendLine($"NAME={region.Name}");
                string group = Get(region, KeyGroup) ?? parent?.Name;
                if (!string.IsNullOrEmpty(group))
                {
                    sb.AppendLine($"GROUP={group}");
                }

                sb.AppendLine($"FLAGS={FlagsFor(region)}");

                foreach (KeyValuePair<string, string> p in region.Properties.Where(p => !IsInternal(p.Key)))
                {
                    sb.AppendLine($"{p.Key}={p.Value}");
                }

                RegionPoint go = region.Go;
                if (go == null)
                {
                    RegionRect first = rects[0];
                    go = new RegionPoint { X = first.X + (first.Width / 2), Y = first.Y + (first.Height / 2), Z = 0 };
                }

                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "P={0},{1},{2},{3}", go.X, go.Y, go.Z ?? 0, facet.MapId));
                foreach (RegionRect r in rects)
                {
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "RECT={0},{1},{2},{3},{4}", r.X, r.Y, r.X + r.Width, r.Y + r.Height, facet.MapId));
                }
            }

            foreach (RegionNode child in region.Children)
            {
                WriteRegion(sb, facet, child, options, usedNames, region);
            }
        }

        private static bool IsInternal(string key) =>
            key == KeyDefName || key == KeyKind || key == KeyFlags || key == KeyEvents || key == KeyGroup || _handled.Contains(key) ||
            key.StartsWith("POL.", StringComparison.Ordinal);

        private static string Get(RegionNode region, string key) =>
            region.Properties.TryGetValue(key, out string v) && !string.IsNullOrWhiteSpace(v) ? v : null;

        /// <summary>FLAGS: imported flags are kept, the type adds the matching flags</summary>
        public static string FlagsFor(RegionNode region)
        {
            HashSet<string> flags = Flags(region);
            switch ((region.Type ?? string.Empty).ToLowerInvariant())
            {
                case "townregion":
                case "guardedregion":
                    if (!region.GuardsDisabled)
                    {
                        flags.Add(FlagGuarded);
                    }

                    flags.Add(FlagNoBuilding);
                    break;
                case "dungeonregion":
                    flags.Add(FlagUnderground);
                    flags.Add(FlagNoBuilding);
                    break;
                case "nohousingregion":
                case "jail":
                    flags.Add(FlagNoBuilding);
                    break;
            }

            if (region.GuardsDisabled)
            {
                flags.Remove(FlagGuarded);
            }

            return string.Join("|", flags.OrderBy(f => f, StringComparer.Ordinal));
        }

        /// <summary>"Britain Graveyard" -> "a_britain_graveyard"</summary>
        public static string DefName(string name)
        {
            string s = Regex.Replace((name ?? "region").ToLowerInvariant(), "[^a-z0-9]+", "_").Trim('_');
            return "a_" + (s.Length == 0 ? "region" : s);
        }

        public static string Save(RegionProject project, string fileName, Options options)
        {
            string backup = FileHelper.Backup(fileName);
            FileHelper.WriteAllText(fileName, ToText(project, options));
            return backup;
        }

        #endregion
    }
}

/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * POL: regions\regions.cfg (all in one), justice.cfg (guards), music.cfg (midi)
 *
 *  Region Moonglow
 *  {
 *      Range       4379 1045 4497 1182      (x-west y-north x-east y-south, corners included)
 *      Realm       britannia
 *      Guarded     1
 *      EnterText   You have entered the protection of the town guards.
 *      LeaveText   You have left the protection of the town guards.
 *      midi        48
 *  }
 *
 * POL has no nested regions: children are written as own blocks.
 * Other keys (NoCast, NoCombat, Light, Weather…) are kept in the region properties ("POL.<key>").
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
    public enum PolFile
    {
        /// <summary>regions.cfg with all keys</summary>
        Regions,

        /// <summary>justice.cfg: Guarded, EnterText, LeaveText, NoCombat …</summary>
        Justice,

        /// <summary>music.cfg: midi</summary>
        Music
    }

    public static class PolFormat
    {
        public const string Prefix = "POL.";
        public const string KeyBlock = "POL.BLOCK";

        /// <summary>realm names of the built in maps (map id -> realm)</summary>
        public static readonly string[] Realms = { "britannia", "britannia_alt", "ilshenar", "malas", "tokuno", "termur" };

        private static readonly string[] _justiceKeys = { "EnterText", "LeaveText", "NoCombat", "NoCast", "EnterScript", "LeaveScript" };

        public static string RealmName(int mapId) => mapId >= 0 && mapId < Realms.Length ? Realms[mapId] : $"realm{mapId}";

        public static int RealmMapId(string realm)
        {
            int i = Array.FindIndex(Realms, r => r.Equals(realm?.Trim(), StringComparison.OrdinalIgnoreCase));
            return i;
        }

        #region Import

        public static RegionProject Load(string fileName)
        {
            RegionProject project = Parse(File.ReadAllText(fileName));
            project.SourceFile = fileName;
            project.SourceFormat = "pol";
            return project;
        }

        /// <summary>Loads several files of a POL regions folder and joins blocks with the same name</summary>
        public static RegionProject LoadFolder(string folder)
        {
            var project = new RegionProject { SourceFile = folder, SourceFormat = "pol" };
            foreach (string name in new[] { "regions.cfg", "justice.cfg", "music.cfg" })
            {
                string path = Path.Combine(folder, name);
                if (File.Exists(path))
                {
                    MergeInto(project, Parse(File.ReadAllText(path)));
                }
            }

            return project;
        }

        private static void MergeInto(RegionProject target, RegionProject source)
        {
            foreach (FacetRegions facet in source.Facets)
            {
                FacetRegions t = target.GetFacet(facet.MapId, facet.Name, true);
                foreach (RegionNode region in facet.Regions)
                {
                    RegionNode same = t.Regions.FirstOrDefault(r => r.Name.Equals(region.Name, StringComparison.OrdinalIgnoreCase));
                    if (same == null)
                    {
                        t.Regions.Add(region);
                        continue;
                    }

                    // same region from another file: add the values, keep the rectangles of the first file
                    foreach (RegionRect r in region.Rects.Where(r => !same.Rects.Any(s => s.Bounds == r.Bounds)))
                    {
                        same.Rects.Add(r);
                    }

                    foreach (KeyValuePair<string, string> p in region.Properties.Where(p => p.Key != KeyBlock))
                    {
                        same.Properties[p.Key] = p.Value;
                    }

                    same.Music = same.Music ?? region.Music;
                    if (!string.IsNullOrEmpty(region.Type) && string.IsNullOrEmpty(same.Type))
                    {
                        same.Type = region.Type;
                    }
                }
            }
        }

        public static RegionProject Parse(string text)
        {
            var project = new RegionProject();
            RegionNode current = null;
            string realm = null;
            bool inBlock = false;
            string pendingHeader = null;

            foreach (string raw in text.Replace("\r\n", "\n").Split('\n'))
            {
                string line = StripComment(raw).Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                if (!inBlock)
                {
                    if (line == "{" && pendingHeader != null)
                    {
                        inBlock = true;
                        current = NewRegion(pendingHeader);
                        realm = null;
                        pendingHeader = null;
                        continue;
                    }

                    Match header = Regex.Match(line, @"^(Region|JusticeRegion|MusicRegion)\s+(.+?)\s*(\{)?$", RegexOptions.IgnoreCase);
                    if (header.Success)
                    {
                        string h = header.Groups[1].Value + " " + header.Groups[2].Value;
                        if (header.Groups[3].Success)
                        {
                            inBlock = true;
                            current = NewRegion(h);
                            realm = null;
                        }
                        else
                        {
                            pendingHeader = h;
                        }
                    }

                    continue;
                }

                if (line == "}")
                {
                    int mapId = RealmMapId(realm ?? "britannia");
                    if (mapId < 0)
                    {
                        mapId = 100 + Math.Abs((realm ?? string.Empty).GetHashCode() % 100);
                    }

                    FacetRegions facet = project.GetFacet(mapId, RegionCatalog.FacetName(mapId, realm), true);
                    facet.Regions.Add(current);
                    current = null;
                    inBlock = false;
                    continue;
                }

                Match kv = Regex.Match(line, @"^(\S+)\s*(.*)$");
                string key = kv.Groups[1].Value;
                string value = kv.Groups[2].Value.Trim();
                switch (key.ToLowerInvariant())
                {
                    case "range":
                        {
                            int[] v = value.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : 0).ToArray();
                            if (v.Length >= 4)
                            {
                                int x1 = Math.Min(v[0], v[2]), x2 = Math.Max(v[0], v[2]), y1 = Math.Min(v[1], v[3]), y2 = Math.Max(v[1], v[3]);
                                current.Rects.Add(new RegionRect { X = x1, Y = y1, Width = x2 - x1 + 1, Height = y2 - y1 + 1 });
                            }

                            break;
                        }
                    case "realm":
                        realm = value;
                        break;
                    case "guarded":
                        current.Properties[Prefix + "Guarded"] = value;
                        if (value == "1" && string.IsNullOrEmpty(current.Type))
                        {
                            current.Type = "GuardedRegion";
                        }

                        break;
                    case "midi":
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int midi))
                        {
                            current.Music = RegionCatalog.MusicName(midi) ?? value;
                        }

                        break;
                    case "type":
                        current.Properties[Prefix + "Type"] = value;
                        if (value.Equals("city", StringComparison.OrdinalIgnoreCase) || value.Equals("town", StringComparison.OrdinalIgnoreCase))
                        {
                            current.Type = "TownRegion";
                        }
                        else if (value.Equals("dungeon", StringComparison.OrdinalIgnoreCase))
                        {
                            current.Type = "DungeonRegion";
                        }

                        break;
                    default:
                        current.Properties[Prefix + key] = value;
                        break;
                }
            }

            return project;
        }

        private static RegionNode NewRegion(string header)
        {
            int space = header.IndexOf(' ');
            var region = new RegionNode { Name = header.Substring(space + 1).Trim(), Priority = 50 };
            region.Properties[KeyBlock] = header.Substring(0, space);
            return region;
        }

        private static string StripComment(string line)
        {
            int i = line.IndexOf("//", StringComparison.Ordinal);
            if (i >= 0)
            {
                line = line.Substring(0, i);
            }

            string t = line.TrimStart();
            return t.StartsWith("#", StringComparison.Ordinal) ? string.Empty : line;
        }

        #endregion

        #region Export

        public sealed class Options
        {
            public PolFile File { get; set; } = PolFile.Regions;
            public bool SkipHidden { get; set; }

            /// <summary>EnterText/LeaveText for guarded regions without own texts</summary>
            public bool GuardTexts { get; set; } = true;
        }

        public static string ToText(RegionProject project, Options options)
        {
            var sb = new StringBuilder();
            sb.AppendLine("##########################################################################");
            sb.AppendLine($"# {FileName(options.File)} – written by the UoFiddler region editor");
            sb.AppendLine("# Range x-west y-north x-east y-south (corners included)");
            sb.AppendLine("##########################################################################");

            foreach (FacetRegions facet in project.Facets.OrderBy(f => f.MapId))
            {
                foreach (RegionNode top in facet.Regions)
                {
                    WriteRegion(sb, facet, top, options);
                }
            }

            return sb.ToString();
        }

        public static string FileName(PolFile file) => file == PolFile.Justice ? "justice.cfg" : file == PolFile.Music ? "music.cfg" : "regions.cfg";

        private static void WriteRegion(StringBuilder sb, FacetRegions facet, RegionNode region, Options options)
        {
            if (options.SkipHidden && region.Hidden)
            {
                return;
            }

            List<RegionRect> rects = Geometry.AllRects(region);
            bool guarded = IsGuarded(region);
            int music = RegionCatalog.MusicId(region.Music);
            bool write = rects.Count > 0 &&
                         (options.File == PolFile.Regions || (options.File == PolFile.Justice) || (options.File == PolFile.Music && music >= 0));
            if (write)
            {
                sb.AppendLine();
                if (!string.IsNullOrWhiteSpace(region.LeadingComment))
                {
                    foreach (string line in region.LeadingComment.Split('\n'))
                    {
                        sb.AppendLine("# " + line.Trim());
                    }
                }

                sb.AppendLine($"Region {Clean(region.Name)}");
                sb.AppendLine("{");
                foreach (RegionRect r in rects)
                {
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "\tRange\t\t{0} {1} {2} {3}", r.X, r.Y, r.X + r.Width - 1, r.Y + r.Height - 1));
                }

                sb.AppendLine($"\tRealm\t\t{RealmName(facet.MapId)}");

                if (options.File != PolFile.Music)
                {
                    sb.AppendLine($"\tGuarded\t\t{(guarded ? 1 : 0)}");
                    string enter = Prop(region, "EnterText");
                    string leave = Prop(region, "LeaveText");
                    if (guarded && options.GuardTexts)
                    {
                        enter = enter ?? "You have entered the protection of the town guards.";
                        leave = leave ?? "You have left the protection of the town guards.";
                    }

                    if (enter != null)
                    {
                        sb.AppendLine($"\tEnterText\t{enter}");
                    }

                    if (leave != null)
                    {
                        sb.AppendLine($"\tLeaveText\t{leave}");
                    }
                }

                if (options.File != PolFile.Justice && music >= 0)
                {
                    sb.AppendLine($"\tmidi\t\t{music}");
                }

                foreach (KeyValuePair<string, string> p in region.Properties.Where(p => p.Key.StartsWith(Prefix, StringComparison.Ordinal)))
                {
                    string key = p.Key.Substring(Prefix.Length);
                    if (key == "BLOCK" || key.Equals("Guarded", StringComparison.OrdinalIgnoreCase) ||
                        key.Equals("EnterText", StringComparison.OrdinalIgnoreCase) || key.Equals("LeaveText", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    bool justiceKey = _justiceKeys.Contains(key, StringComparer.OrdinalIgnoreCase);
                    if (options.File == PolFile.Regions || (options.File == PolFile.Justice && justiceKey))
                    {
                        sb.AppendLine($"\t{key}\t\t{p.Value}");
                    }
                }

                if (options.File == PolFile.Regions && !region.Properties.ContainsKey(Prefix + "Type") && IsTown(region))
                {
                    sb.AppendLine("\tType\t\tcity");
                }

                sb.AppendLine("}");
            }

            foreach (RegionNode child in region.Children)
            {
                WriteRegion(sb, facet, child, options);
            }
        }

        private static string Prop(RegionNode region, string key) =>
            region.Properties.TryGetValue(Prefix + key, out string v) && !string.IsNullOrWhiteSpace(v) ? v : null;

        private static bool IsTown(RegionNode region) => string.Equals(region.Type, "TownRegion", StringComparison.OrdinalIgnoreCase);

        public static bool IsGuarded(RegionNode region)
        {
            if (region.GuardsDisabled)
            {
                return false;
            }

            string g = Prop(region, "Guarded");
            if (g != null)
            {
                return g == "1";
            }

            return IsTown(region) || string.Equals(region.Type, "GuardedRegion", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>the region name goes up to the end of the header line – no tabs, braces or line breaks</summary>
        private static string Clean(string name) => Regex.Replace(string.IsNullOrWhiteSpace(name) ? "Region" : name.Trim(), @"[\t\r\n{}]", " ");

        public static string Save(RegionProject project, string fileName, Options options)
        {
            string backup = FileHelper.Backup(fileName);
            FileHelper.WriteAllText(fileName, ToText(project, options));
            return backup;
        }

        #endregion
    }
}

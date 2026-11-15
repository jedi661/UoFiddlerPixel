/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Own formats of the region editor:
 *  - project (.uoregions, JSON) – everything incl. polygons, colours and notes
 *  - CSV (one line per rectangle, import and export)
 *  - text overview
 *  - HTML overview (tables and a sketch of every facet, optional map images)
 *  - C# class skeletons for region types that ServUO does not know
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using UoFiddler.Plugin.RegionEditor.Classes;

namespace UoFiddler.Plugin.RegionEditor.Formats
{
    public static class ProjectFormat
    {
        public const string Extension = ".uoregions";

        public static RegionProject Load(string fileName)
        {
            RegionProject project = JsonSerializer.Deserialize<RegionProject>(File.ReadAllText(fileName), ProjectJson.Options) ?? new RegionProject();
            project.Facets = project.Facets ?? new List<FacetRegions>();
            foreach (RegionNode r in project.AllRegions().ToList())
            {
                Repair(r);
            }

            return project;
        }

        /// <summary>lists that are missing in hand written files</summary>
        private static void Repair(RegionNode r)
        {
            r.Id = string.IsNullOrEmpty(r.Id) ? Guid.NewGuid().ToString("N") : r.Id;
            r.Rects = r.Rects ?? new List<RegionRect>();
            r.Polygons = r.Polygons ?? new List<RegionPolygon>();
            r.Children = r.Children ?? new List<RegionNode>();
            r.Properties = r.Properties ?? new Dictionary<string, string>();
            r.ExtraXml = r.ExtraXml ?? new List<string>();
            r.ExtraAttributes = r.ExtraAttributes ?? new Dictionary<string, string>();
        }

        public static void Save(RegionProject project, string fileName)
        {
            FileHelper.WriteAllText(fileName, JsonSerializer.Serialize(project, ProjectJson.Options));
        }
    }

    public static class CsvFormat
    {
        private static readonly string[] _columns =
        {
            "Facet", "MapId", "Region", "Parent", "Type", "Priority", "Music", "X", "Y", "Width", "Height", "ZMin", "ZMax", "GoX", "GoY", "GoZ"
        };

        public static string ToText(RegionProject project, char separator = ';')
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(separator.ToString(), _columns));
            foreach (FacetRegions facet in project.Facets.OrderBy(f => f.MapId))
            {
                foreach (RegionNode top in facet.Regions)
                {
                    Write(sb, facet, top, null, separator);
                }
            }

            return sb.ToString();
        }

        private static void Write(StringBuilder sb, FacetRegions facet, RegionNode region, RegionNode parent, char sep)
        {
            List<RegionRect> rects = Geometry.AllRects(region);
            if (rects.Count == 0)
            {
                rects.Add(null);
            }

            foreach (RegionRect r in rects)
            {
                string[] v =
                {
                    facet.Name, I(facet.MapId), region.Name, parent?.Name ?? string.Empty, region.Type, I(region.Priority), region.Music ?? string.Empty,
                    I(r?.X), I(r?.Y), I(r?.Width), I(r?.Height), I(r?.ZMin ?? region.ZMin), I(r?.ZMax ?? region.ZMax),
                    I(region.Go?.X), I(region.Go?.Y), I(region.Go?.Z)
                };
                sb.AppendLine(string.Join(sep.ToString(), v.Select(x => Quote(x, sep))));
            }

            foreach (RegionNode child in region.Children)
            {
                Write(sb, facet, child, region, sep);
            }
        }

        private static string I(int? v) => v?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

        private static string Quote(string s, char sep)
        {
            s = s ?? string.Empty;
            return s.IndexOf(sep) >= 0 || s.Contains('"') || s.Contains('\n') ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
        }

        /// <summary>Reads a CSV written by <see cref="ToText"/> (also with ',' as separator)</summary>
        public static RegionProject Parse(string text)
        {
            var project = new RegionProject();
            List<string[]> rows = ReadRows(text);
            if (rows.Count == 0)
            {
                return project;
            }

            string[] header = rows[0];
            int Col(string name) => Array.FindIndex(header, h => h.Trim().Equals(name, StringComparison.OrdinalIgnoreCase));
            int cFacet = Col("Facet"), cMap = Col("MapId"), cName = Col("Region"), cParent = Col("Parent"), cType = Col("Type"), cPrio = Col("Priority"),
                cMusic = Col("Music"), cX = Col("X"), cY = Col("Y"), cW = Col("Width"), cH = Col("Height"), cZ1 = Col("ZMin"), cZ2 = Col("ZMax"),
                cGx = Col("GoX"), cGy = Col("GoY"), cGz = Col("GoZ");
            if (cName < 0 || cX < 0)
            {
                throw new InvalidDataException("CSV: columns Region and X are required");
            }

            var byKey = new Dictionary<string, RegionNode>(StringComparer.OrdinalIgnoreCase);
            foreach (string[] row in rows.Skip(1))
            {
                string Get(int c) => c >= 0 && c < row.Length ? row[c].Trim() : string.Empty;
                int? N(int c) => int.TryParse(Get(c), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : (int?)null;

                string facetName = Get(cFacet);
                int mapId = N(cMap) ?? RegionCatalog.FacetMapId(facetName);
                if (mapId < 0)
                {
                    mapId = 0;
                }

                FacetRegions facet = project.GetFacet(mapId, string.IsNullOrEmpty(facetName) ? RegionCatalog.FacetName(mapId, null) : facetName, true);
                string name = Get(cName);
                string key = mapId + "|" + name;
                if (!byKey.TryGetValue(key, out RegionNode region))
                {
                    region = new RegionNode
                    {
                        Name = name,
                        Type = Get(cType),
                        Priority = N(cPrio) ?? 50,
                        Music = string.IsNullOrEmpty(Get(cMusic)) ? null : Get(cMusic)
                    };
                    if (N(cGx).HasValue && N(cGy).HasValue)
                    {
                        region.Go = new RegionPoint { X = N(cGx).Value, Y = N(cGy).Value, Z = N(cGz) };
                    }

                    string parentName = Get(cParent);
                    if (!string.IsNullOrEmpty(parentName) && byKey.TryGetValue(mapId + "|" + parentName, out RegionNode parent))
                    {
                        parent.Children.Add(region);
                    }
                    else
                    {
                        facet.Regions.Add(region);
                    }

                    byKey[key] = region;
                }

                if (N(cX).HasValue && N(cY).HasValue && N(cW).HasValue && N(cH).HasValue)
                {
                    region.Rects.Add(new RegionRect { X = N(cX).Value, Y = N(cY).Value, Width = N(cW).Value, Height = N(cH).Value, ZMin = N(cZ1), ZMax = N(cZ2) });
                }
            }

            return project;
        }

        private static List<string[]> ReadRows(string text)
        {
            var rows = new List<string[]>();
            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            if (lines.Length == 0)
            {
                return rows;
            }

            char sep = lines[0].Count(c => c == ';') >= lines[0].Count(c => c == ',') ? ';' : ',';
            var cells = new List<string>();
            var cell = new StringBuilder();
            bool quoted = false;
            foreach (string line in lines)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];
                    if (quoted)
                    {
                        if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                        {
                            cell.Append('"');
                            i++;
                        }
                        else if (c == '"')
                        {
                            quoted = false;
                        }
                        else
                        {
                            cell.Append(c);
                        }
                    }
                    else if (c == '"')
                    {
                        quoted = true;
                    }
                    else if (c == sep)
                    {
                        cells.Add(cell.ToString());
                        cell.Clear();
                    }
                    else
                    {
                        cell.Append(c);
                    }
                }

                if (quoted)
                {
                    cell.Append('\n');
                    continue;
                }

                cells.Add(cell.ToString());
                cell.Clear();
                if (cells.Count > 1 || cells[0].Length > 0)
                {
                    rows.Add(cells.ToArray());
                }

                cells.Clear();
            }

            return rows;
        }
    }

    public static class TextFormat
    {
        public static string ToText(RegionProject project)
        {
            var sb = new StringBuilder();
            foreach (FacetRegions facet in project.Facets.OrderBy(f => f.MapId))
            {
                sb.AppendLine($"=== {facet.Name} (map {facet.MapId}) – {facet.AllRegions().Count()} regions ===");
                foreach (RegionNode top in facet.Regions)
                {
                    Write(sb, top, 0);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static void Write(StringBuilder sb, RegionNode region, int depth)
        {
            string indent = new string(' ', depth * 2);
            sb.AppendLine($"{indent}{region.DisplayName}  [{region.DisplayType}, prio {region.Priority}{(string.IsNullOrEmpty(region.Music) ? string.Empty : ", " + region.Music)}]");
            foreach (RegionRect r in region.Rects)
            {
                sb.AppendLine($"{indent}    rect {r}");
            }

            foreach (RegionPolygon p in region.Polygons)
            {
                sb.AppendLine($"{indent}    polygon {string.Join(" ", p.Points.Select(pt => $"{pt.X},{pt.Y}"))}");
            }

            if (region.Go != null)
            {
                sb.AppendLine($"{indent}    go {region.Go}");
            }

            if (region.Entrance != null)
            {
                sb.AppendLine($"{indent}    entrance {region.Entrance}");
            }

            foreach (RegionNode child in region.Children)
            {
                Write(sb, child, depth + 1);
            }
        }
    }

    public static class HtmlFormat
    {
        /// <param name="mapImages">optional PNG per map id (shown under the sketch)</param>
        public static string ToHtml(RegionProject project, string title, IDictionary<int, byte[]> mapImages, Func<int, System.Drawing.Size> mapSize)
        {
            var sb = new StringBuilder();
            string E(string s) => WebUtility.HtmlEncode(s ?? string.Empty);
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>" + E(title) + "</title>");
            sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:24px;background:#f6f7f9;color:#222}" +
                          "h1{font-size:22px}h2{margin-top:32px;border-bottom:2px solid #ccd}table{border-collapse:collapse;width:100%;background:#fff}" +
                          "th,td{border:1px solid #dde;padding:4px 8px;font-size:13px;text-align:left;vertical-align:top}th{background:#eef}" +
                          ".sw{display:inline-block;width:12px;height:12px;border-radius:2px;margin-right:6px;vertical-align:middle}" +
                          "svg{background:#223;border:1px solid #889;max-width:100%;height:auto}.child td:first-child{padding-left:28px}" +
                          "img{max-width:100%;image-rendering:pixelated;border:1px solid #889}" +
                          "@media(prefers-color-scheme:dark){body{background:#1b1d22;color:#ddd}table{background:#25282f}th{background:#2f3340}th,td{border-color:#3a3e4a}}</style>");
            sb.AppendLine("</head><body>");
            sb.AppendLine($"<h1>{E(title)}</h1><p>{project.AllRegions().Count()} regions · {DateTime.Now:yyyy-MM-dd HH:mm}</p>");

            foreach (FacetRegions facet in project.Facets.OrderBy(f => f.MapId))
            {
                System.Drawing.Size size = mapSize?.Invoke(facet.MapId) ?? System.Drawing.Size.Empty;
                if (size.IsEmpty)
                {
                    System.Drawing.Rectangle all = System.Drawing.Rectangle.Empty;
                    foreach (RegionNode r in facet.AllRegions())
                    {
                        all = all.IsEmpty ? r.Bounds : System.Drawing.Rectangle.Union(all, r.Bounds);
                    }

                    size = new System.Drawing.Size(Math.Max(1, all.Right), Math.Max(1, all.Bottom));
                }

                sb.AppendLine($"<h2>{E(facet.Name)} <small>(map {facet.MapId}, {size.Width}×{size.Height})</small></h2>");
                if (mapImages != null && mapImages.TryGetValue(facet.MapId, out byte[] png) && png != null)
                {
                    sb.AppendLine($"<p><img alt=\"{E(facet.Name)}\" src=\"data:image/png;base64,{Convert.ToBase64String(png)}\"></p>");
                }
                else
                {
                    sb.AppendLine($"<svg viewBox=\"0 0 {size.Width} {size.Height}\" width=\"{Math.Min(1200, size.Width / 4)}\">");
                    foreach (RegionNode r in facet.AllRegions())
                    {
                        string col = Hex(r);
                        foreach (RegionRect rect in Geometry.AllRects(r))
                        {
                            sb.AppendLine($"<rect x=\"{rect.X}\" y=\"{rect.Y}\" width=\"{rect.Width}\" height=\"{rect.Height}\" fill=\"{col}\" fill-opacity=\"0.35\" stroke=\"{col}\" stroke-width=\"4\"><title>{E(r.DisplayName)}</title></rect>");
                        }
                    }

                    sb.AppendLine("</svg>");
                }

                sb.AppendLine("<table><tr><th>Region</th><th>Type</th><th>Priority</th><th>Music</th><th>Rects</th><th>Area</th><th>Go</th></tr>");
                foreach (RegionNode top in facet.Regions)
                {
                    Row(sb, top, 0, E);
                }

                sb.AppendLine("</table>");
            }

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static void Row(StringBuilder sb, RegionNode r, int depth, Func<string, string> e)
        {
            string pad = depth == 0 ? string.Empty : $" style=\"padding-left:{8 + (depth * 20)}px\"";
            sb.AppendLine($"<tr><td{pad}><span class=\"sw\" style=\"background:{Hex(r)}\"></span>{e(r.DisplayName)}</td><td>{e(r.DisplayType)}</td>" +
                          $"<td>{r.Priority}</td><td>{e(r.Music)}</td><td>{r.Rects.Count + r.Polygons.Count}</td><td>{r.Area:N0}</td><td>{e(r.Go?.ToString())}</td></tr>");
            foreach (RegionNode child in r.Children)
            {
                Row(sb, child, depth + 1, e);
            }
        }

        private static string Hex(RegionNode r)
        {
            System.Drawing.Color c = r.Color.HasValue ? System.Drawing.Color.FromArgb(r.Color.Value) : RegionCatalog.TypeColor(r.Type);
            return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        }
    }

    public static class CSharpFormat
    {
        private static readonly HashSet<string> _builtIn = new HashSet<string>(RegionCatalog.Types, StringComparer.OrdinalIgnoreCase);

        /// <summary>region types of the project that ServUO does not have</summary>
        public static List<string> CustomTypes(RegionProject project) =>
            project.AllRegions().Select(r => r.Type).Where(t => !string.IsNullOrWhiteSpace(t) && !_builtIn.Contains(t))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(t => t, StringComparer.OrdinalIgnoreCase).ToList();

        /// <summary>
        /// C# skeleton of a region class for ServUO. Regions.xml creates it with the
        /// constructor (XmlElement xml, Map map, Region parent).
        /// </summary>
        public static string ClassFor(string type, string nameSpace = "Server.Regions")
        {
            string cls = new string((type ?? "CustomRegion").Where(char.IsLetterOrDigit).ToArray());
            if (cls.Length == 0 || char.IsDigit(cls[0]))
            {
                cls = "Custom" + cls;
            }

            return
$@"// Region class created by the UoFiddler region editor.
// Copy this file into the Scripts folder of your server (e.g. Scripts\Regions\{cls}.cs).
using System;
using System.Xml;
using Server.Mobiles;

namespace {nameSpace}
{{
    public class {cls} : BaseRegion
    {{
        // Regions.xml: <region type=""{cls}"" priority=""50"" name=""..."">
        public {cls}(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {{
        }}

        public override void OnEnter(Mobile m)
        {{
            base.OnEnter(m);
            // m.SendMessage(""You enter {cls}."");
        }}

        public override void OnExit(Mobile m)
        {{
            base.OnExit(m);
        }}

        public override bool AllowHousing(Mobile from, Point3D p)
        {{
            return base.AllowHousing(from, p);
        }}

        public override bool OnBeginSpellCast(Mobile m, ISpell s)
        {{
            return base.OnBeginSpellCast(m, s);
        }}
    }}
}}
";
        }

        /// <summary>writes one .cs file per custom type into the folder, returns the written files</summary>
        public static List<string> WriteClasses(RegionProject project, string folder)
        {
            var files = new List<string>();
            foreach (string type in CustomTypes(project))
            {
                string cls = new string(type.Where(char.IsLetterOrDigit).ToArray());
                string path = Path.Combine(folder, (cls.Length == 0 ? "CustomRegion" : cls) + ".cs");
                FileHelper.WriteAllText(path, ClassFor(type));
                files.Add(path);
            }

            return files;
        }
    }
}

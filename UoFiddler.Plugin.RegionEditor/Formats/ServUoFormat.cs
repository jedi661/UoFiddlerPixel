/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * ServUO / RunUO: Data\Regions.xml
 *
 *  <ServerRegions>
 *    <Facet name="Felucca">
 *      <region type="TownRegion" priority="50" name="Britain">
 *        <rect x="1416" y="1498" width="324" height="279" />      (or x1/y1/x2/y2, optional zmin/zmax)
 *        <go x="1495" y="1629" z="10" />
 *        <music name="Britain1" />
 *        <region name="Britain Graveyard" ...> … </region>        (children inherit the priority)
 *      </region>
 *
 * Unknown elements and attributes (e.g. <spawning>) and comments in front of a
 * region are kept and written back unchanged.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UoFiddler.Plugin.RegionEditor.Classes;

namespace UoFiddler.Plugin.RegionEditor.Formats
{
    public enum MergeMode
    {
        /// <summary>the file is replaced by the project</summary>
        ReplaceFile,

        /// <summary>facets of the project replace the same facets of the file, other facets stay</summary>
        ReplaceFacets,

        /// <summary>top level regions with the same name are replaced, new ones are added, all others stay</summary>
        MergeByName
    }

    public static class ServUoFormat
    {
        private static readonly HashSet<string> _knownRegionAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "type", "priority", "name" };

        #region Import

        public static RegionProject Load(string fileName)
        {
            XDocument doc = XDocument.Load(fileName, LoadOptions.None);
            RegionProject project = Read(doc);
            project.SourceFile = fileName;
            project.SourceFormat = "servuo";
            return project;
        }

        public static RegionProject Parse(string xml) => Read(XDocument.Parse(xml));

        private static RegionProject Read(XDocument doc)
        {
            var project = new RegionProject();
            XElement root = doc.Root ?? throw new InvalidDataException("empty file");
            int unknown = 100;
            foreach (XElement facetElement in root.Elements().Where(e => e.Name.LocalName.Equals("Facet", StringComparison.OrdinalIgnoreCase)))
            {
                string name = (string)facetElement.Attribute("name") ?? "Felucca";
                int mapId = RegionCatalog.FacetMapId(name);
                if (mapId < 0)
                {
                    mapId = unknown++;
                }

                FacetRegions facet = project.GetFacet(mapId, name, true);
                ReadRegions(facetElement, facet.Regions, null);
            }

            return project;
        }

        private static void ReadRegions(XElement container, List<RegionNode> target, RegionNode parent)
        {
            string comment = null;
            foreach (XNode node in container.Nodes())
            {
                if (node is XComment c)
                {
                    comment = comment == null ? c.Value : comment + "\n" + c.Value;
                    continue;
                }

                if (!(node is XElement e))
                {
                    continue;
                }

                if (e.Name.LocalName.Equals("region", StringComparison.OrdinalIgnoreCase))
                {
                    RegionNode region = ReadRegion(e, parent);
                    region.LeadingComment = comment?.Trim();
                    target.Add(region);
                }

                comment = null;
            }
        }

        private static RegionNode ReadRegion(XElement e, RegionNode parent)
        {
            var region = new RegionNode
            {
                Name = (string)e.Attribute("name") ?? string.Empty,
                Type = (string)e.Attribute("type") ?? string.Empty,
                Priority = Int(e.Attribute("priority"), parent?.Priority ?? 50)
            };

            foreach (XAttribute a in e.Attributes().Where(a => !_knownRegionAttributes.Contains(a.Name.LocalName)))
            {
                region.ExtraAttributes[a.Name.LocalName] = a.Value;
            }

            foreach (XElement child in e.Elements())
            {
                switch (child.Name.LocalName.ToLowerInvariant())
                {
                    case "rect":
                        region.Rects.Add(ReadRect(child));
                        break;
                    case "zrange":
                        region.ZMin = NInt(child.Attribute("min"));
                        region.ZMax = NInt(child.Attribute("max"));
                        break;
                    case "go":
                        region.Go = ReadPoint(child);
                        break;
                    case "entrance":
                        region.Entrance = ReadPoint(child);
                        region.Entrance.Map = (string)child.Attribute("map");
                        break;
                    case "music":
                        region.Music = (string)child.Attribute("name");
                        break;
                    case "rune":
                        region.RuneName = (string)child.Attribute("name");
                        break;
                    case "guards":
                        region.GuardsType = (string)child.Attribute("type");
                        region.GuardsDisabled = Bool(child.Attribute("disabled"));
                        break;
                    case "smartnohousing":
                        region.SmartNoHousing = Bool(child.Attribute("active"));
                        break;
                    case "logoutdelay":
                        region.LogoutDelayActive = Bool(child.Attribute("active"));
                        break;
                    case "region":
                        break;
                    default:
                        region.ExtraXml.Add(child.ToString(SaveOptions.DisableFormatting));
                        break;
                }
            }

            ReadRegions(e, region.Children, region);
            return region;
        }

        private static RegionRect ReadRect(XElement e)
        {
            var rect = new RegionRect();
            if (e.Attribute("x") != null || e.Attribute("width") != null)
            {
                rect.X = Int(e.Attribute("x"), 0);
                rect.Y = Int(e.Attribute("y"), 0);
                rect.Width = Int(e.Attribute("width"), 0);
                rect.Height = Int(e.Attribute("height"), 0);
            }
            else
            {
                rect.X = Int(e.Attribute("x1"), 0);
                rect.Y = Int(e.Attribute("y1"), 0);
                rect.Width = Int(e.Attribute("x2"), 0) - rect.X;
                rect.Height = Int(e.Attribute("y2"), 0) - rect.Y;
            }

            rect.ZMin = NInt(e.Attribute("zmin"));
            rect.ZMax = NInt(e.Attribute("zmax"));
            rect.Expansion = (string)e.Attribute("expansion");
            return rect;
        }

        private static RegionPoint ReadPoint(XElement e)
        {
            return new RegionPoint
            {
                X = Int(e.Attribute("x"), 0),
                Y = Int(e.Attribute("y"), 0),
                Z = NInt(e.Attribute("z"))
            };
        }

        private static int Int(XAttribute a, int fallback) =>
            a != null && int.TryParse(a.Value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : fallback;

        private static int? NInt(XAttribute a) =>
            a != null && int.TryParse(a.Value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : (int?)null;

        private static bool Bool(XAttribute a) => a != null && (a.Value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase) || a.Value.Trim() == "1");

        #endregion

        #region Export

        public sealed class Options
        {
            /// <summary>write x/y/width/height (true, ServUO style) or x1/y1/x2/y2</summary>
            public bool WidthHeight { get; set; } = true;

            /// <summary>regions marked as hidden in the editor are left out</summary>
            public bool SkipHidden { get; set; }

            public MergeMode Merge { get; set; } = MergeMode.ReplaceFile;
        }

        public static XDocument ToDocument(RegionProject project, Options options)
        {
            var root = new XElement("ServerRegions");
            foreach (FacetRegions facet in project.Facets.OrderBy(f => f.MapId))
            {
                root.Add(FacetElement(facet, options));
            }

            return new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        }

        private static XElement FacetElement(FacetRegions facet, Options options)
        {
            var element = new XElement("Facet", new XAttribute("name", facet.Name));
            AddRegions(element, facet.Regions, null, options);
            return element;
        }

        private static void AddRegions(XElement container, IEnumerable<RegionNode> regions, RegionNode parent, Options options)
        {
            foreach (RegionNode region in regions)
            {
                if (options.SkipHidden && region.Hidden)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(region.LeadingComment))
                {
                    container.Add(new XComment(" " + region.LeadingComment.Trim() + " "));
                }

                container.Add(RegionElement(region, parent, options));
            }
        }

        public static XElement RegionElement(RegionNode region, RegionNode parent, Options options)
        {
            var e = new XElement("region");
            if (!string.IsNullOrWhiteSpace(region.Type))
            {
                e.Add(new XAttribute("type", region.Type));
            }

            if (parent == null || region.Priority != parent.Priority)
            {
                e.Add(new XAttribute("priority", region.Priority.ToString(CultureInfo.InvariantCulture)));
            }

            if (!string.IsNullOrWhiteSpace(region.Name))
            {
                e.Add(new XAttribute("name", region.Name));
            }

            foreach (KeyValuePair<string, string> a in region.ExtraAttributes)
            {
                e.Add(new XAttribute(a.Key, a.Value));
            }

            if (!string.IsNullOrEmpty(region.RuneName))
            {
                e.Add(new XElement("rune", new XAttribute("name", region.RuneName)));
            }

            if (region.GuardsDisabled || !string.IsNullOrEmpty(region.GuardsType))
            {
                var guards = new XElement("guards");
                if (!string.IsNullOrEmpty(region.GuardsType))
                {
                    guards.Add(new XAttribute("type", region.GuardsType));
                }

                if (region.GuardsDisabled)
                {
                    guards.Add(new XAttribute("disabled", "true"));
                }

                e.Add(guards);
            }

            if (region.ZMin.HasValue || region.ZMax.HasValue)
            {
                var z = new XElement("zrange");
                if (region.ZMin.HasValue)
                {
                    z.Add(new XAttribute("min", region.ZMin.Value));
                }

                if (region.ZMax.HasValue)
                {
                    z.Add(new XAttribute("max", region.ZMax.Value));
                }

                e.Add(z);
            }

            foreach (RegionRect r in Geometry.AllRects(region))
            {
                var rect = new XElement("rect");
                if (options.WidthHeight)
                {
                    rect.Add(new XAttribute("x", r.X), new XAttribute("y", r.Y), new XAttribute("width", r.Width), new XAttribute("height", r.Height));
                }
                else
                {
                    rect.Add(new XAttribute("x1", r.X), new XAttribute("y1", r.Y), new XAttribute("x2", r.X + r.Width), new XAttribute("y2", r.Y + r.Height));
                }

                if (r.ZMin.HasValue)
                {
                    rect.Add(new XAttribute("zmin", r.ZMin.Value));
                }

                if (r.ZMax.HasValue)
                {
                    rect.Add(new XAttribute("zmax", r.ZMax.Value));
                }

                if (!string.IsNullOrEmpty(r.Expansion))
                {
                    rect.Add(new XAttribute("expansion", r.Expansion));
                }

                e.Add(rect);
            }

            if (region.Go != null)
            {
                e.Add(PointElement("go", region.Go));
            }

            if (region.Entrance != null)
            {
                XElement entrance = PointElement("entrance", region.Entrance);
                if (!string.IsNullOrEmpty(region.Entrance.Map))
                {
                    entrance.Add(new XAttribute("map", region.Entrance.Map));
                }

                e.Add(entrance);
            }

            if (!string.IsNullOrEmpty(region.Music) && (parent == null || !string.Equals(region.Music, parent.Music, StringComparison.Ordinal)))
            {
                e.Add(new XElement("music", new XAttribute("name", region.Music)));
            }

            if (region.SmartNoHousing.HasValue)
            {
                e.Add(new XElement("smartNoHousing", new XAttribute("active", region.SmartNoHousing.Value ? "true" : "false")));
            }

            if (region.LogoutDelayActive.HasValue)
            {
                e.Add(new XElement("logoutDelay", new XAttribute("active", region.LogoutDelayActive.Value ? "true" : "false")));
            }

            foreach (string xml in region.ExtraXml)
            {
                try
                {
                    e.Add(XElement.Parse(xml, LoadOptions.PreserveWhitespace));
                }
                catch (XmlException)
                {
                    e.Add(new XComment(" invalid xml kept by the region editor: " + xml.Replace("--", "- -") + " "));
                }
            }

            AddRegions(e, region.Children, region, options);
            return e;
        }

        private static XElement PointElement(string name, RegionPoint p)
        {
            var e = new XElement(name, new XAttribute("x", p.X), new XAttribute("y", p.Y));
            if (p.Z.HasValue)
            {
                e.Add(new XAttribute("z", p.Z.Value));
            }

            return e;
        }

        public static string ToXml(RegionProject project, Options options)
        {
            return Write(ToDocument(project, options));
        }

        private static string Write(XDocument doc)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t",
                Encoding = new UTF8Encoding(false),
                OmitXmlDeclaration = false
            };

            using (var stream = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    doc.Save(writer);
                }

                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// Writes the project into a Regions.xml. Existing file: a backup is made first (returned),
        /// and depending on <see cref="Options.Merge"/> parts of the file are kept.
        /// </summary>
        public static string Save(RegionProject project, string fileName, Options options)
        {
            string backup = FileHelper.Backup(fileName);
            XDocument doc;
            if (options.Merge == MergeMode.ReplaceFile || !File.Exists(fileName))
            {
                doc = ToDocument(project, options);
            }
            else
            {
                doc = XDocument.Load(fileName, LoadOptions.None);
                Merge(doc, project, options);
            }

            FileHelper.WriteAllText(fileName, Write(doc));
            return backup;
        }

        /// <summary>merges the project into an existing document</summary>
        public static void Merge(XDocument doc, RegionProject project, Options options)
        {
            XElement root = doc.Root;
            if (root == null)
            {
                root = new XElement("ServerRegions");
                doc.Add(root);
            }

            foreach (FacetRegions facet in project.Facets.OrderBy(f => f.MapId))
            {
                XElement existing = root.Elements().FirstOrDefault(e => e.Name.LocalName.Equals("Facet", StringComparison.OrdinalIgnoreCase) &&
                                                                       string.Equals((string)e.Attribute("name"), facet.Name, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    root.Add(FacetElement(facet, options));
                    continue;
                }

                if (options.Merge == MergeMode.ReplaceFacets)
                {
                    existing.ReplaceWith(FacetElement(facet, options));
                    continue;
                }

                // merge by name: replace regions with the same name (also nested ones), add the others at the end
                foreach (RegionNode region in facet.Regions.Where(r => !(options.SkipHidden && r.Hidden)))
                {
                    XElement element = RegionElement(region, null, options);
                    XElement same = existing.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals("region", StringComparison.OrdinalIgnoreCase) &&
                                                                             !string.IsNullOrEmpty(region.Name) &&
                                                                             string.Equals((string)e.Attribute("name"), region.Name, StringComparison.OrdinalIgnoreCase));
                    if (same != null)
                    {
                        same.ReplaceWith(element);
                    }
                    else
                    {
                        existing.Add(element);
                    }
                }
            }
        }

        #endregion
    }

    public static class FileHelper
    {
        /// <summary>copies an existing file to "name.yyyyMMdd-HHmmss.bak", returns the backup path or null</summary>
        public static string Backup(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return null;
            }

            string backup = $"{fileName}.{DateTime.Now:yyyyMMdd-HHmmss}.bak";
            File.Copy(fileName, backup, true);
            return backup;
        }

        public static void WriteAllText(string fileName, string text)
        {
            string dir = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // write into a temporary file first so a crash does not leave a half written file
            string temp = fileName + ".tmp";
            File.WriteAllText(temp, text, new UTF8Encoding(false));
            if (File.Exists(fileName))
            {
                File.Replace(temp, fileName, null);
            }
            else
            {
                File.Move(temp, fileName);
            }
        }
    }
}

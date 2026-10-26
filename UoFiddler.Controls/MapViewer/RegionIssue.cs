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
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace UoFiddler.Controls.MapViewer
{
    public enum RegionIssueType
    {
        InvalidLandId,
        InvalidStaticId,
        DuplicateStatic,
        StaticBelowLand,
        FloatingStatic,
        SteepLand,
        Manual
    }

    public enum RegionIssueSeverity
    {
        Info,
        Warning,
        Error
    }

    public enum RegionIssueStatus
    {
        Open,
        Fixed,
        Ignored
    }

    /// <summary>
    /// One entry of the region protocol (found by <see cref="RegionAnalyzer"/> or marked by hand)
    /// </summary>
    public sealed class RegionIssue
    {
        public RegionIssueType Type { get; set; }
        public RegionIssueSeverity Severity { get; set; }
        public RegionIssueStatus Status { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        /// <summary>land or static id, -1 if not related to a tile</summary>
        public int TileId { get; set; } = -1;

        public int Hue { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;

        /// <summary>true if <see cref="RegionEditSession.Fix"/> can repair it</summary>
        public bool CanAutoFix { get; set; }

        /// <summary>fixed by "Fix all" (only safe repairs)</summary>
        public bool FixByDefault { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;

        public string TypeText => Type switch
        {
            RegionIssueType.InvalidLandId => "Invalid land id",
            RegionIssueType.InvalidStaticId => "Invalid static id",
            RegionIssueType.DuplicateStatic => "Duplicate static",
            RegionIssueType.StaticBelowLand => "Static below land",
            RegionIssueType.FloatingStatic => "Floating static",
            RegionIssueType.SteepLand => "Steep land",
            RegionIssueType.Manual => "Marked by hand",
            _ => Type.ToString()
        };

        /// <summary>
        /// Same problem at the same place (used to keep status/notes after a new check)
        /// </summary>
        public bool IsSameAs(RegionIssue other)
        {
            return other != null && other.Type == Type && other.X == X && other.Y == Y && other.Z == Z && other.TileId == TileId;
        }

        #region Protocol (csv / xml)

        public static void ExportCsv(string fileName, string mapName, IEnumerable<RegionIssue> issues)
        {
            using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                writer.WriteLine("Map;X;Y;Z;Type;Severity;Status;TileId;Hue;Description;Note;Created");
                foreach (RegionIssue issue in issues)
                {
                    writer.WriteLine(string.Join(";",
                        Csv(mapName),
                        issue.X.ToString(CultureInfo.InvariantCulture),
                        issue.Y.ToString(CultureInfo.InvariantCulture),
                        issue.Z.ToString(CultureInfo.InvariantCulture),
                        Csv(issue.TypeText),
                        issue.Severity,
                        issue.Status,
                        issue.TileId >= 0 ? $"0x{issue.TileId:X4}" : string.Empty,
                        issue.Hue.ToString(CultureInfo.InvariantCulture),
                        Csv(issue.Description),
                        Csv(issue.Note),
                        issue.Created.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)));
                }
            }
        }

        private static string Csv(string text)
        {
            text ??= string.Empty;
            return text.Contains(';') || text.Contains('"') || text.Contains('\n')
                ? "\"" + text.Replace("\"", "\"\"") + "\""
                : text;
        }

        public static void SaveXml(string fileName, int mapId, string mapName, System.Drawing.Rectangle area, IEnumerable<RegionIssue> issues)
        {
            var dom = new XmlDocument();
            dom.AppendChild(dom.CreateXmlDeclaration("1.0", "utf-8", null));
            XmlElement root = dom.CreateElement("RegionProtocol");
            root.SetAttribute("map", mapId.ToString(CultureInfo.InvariantCulture));
            root.SetAttribute("mapName", mapName ?? string.Empty);
            root.SetAttribute("x", area.X.ToString(CultureInfo.InvariantCulture));
            root.SetAttribute("y", area.Y.ToString(CultureInfo.InvariantCulture));
            root.SetAttribute("width", area.Width.ToString(CultureInfo.InvariantCulture));
            root.SetAttribute("height", area.Height.ToString(CultureInfo.InvariantCulture));
            root.SetAttribute("saved", DateTime.Now.ToString("s", CultureInfo.InvariantCulture));
            dom.AppendChild(root);

            foreach (RegionIssue issue in issues)
            {
                XmlElement e = dom.CreateElement("Issue");
                e.SetAttribute("type", issue.Type.ToString());
                e.SetAttribute("severity", issue.Severity.ToString());
                e.SetAttribute("status", issue.Status.ToString());
                e.SetAttribute("x", issue.X.ToString(CultureInfo.InvariantCulture));
                e.SetAttribute("y", issue.Y.ToString(CultureInfo.InvariantCulture));
                e.SetAttribute("z", issue.Z.ToString(CultureInfo.InvariantCulture));
                e.SetAttribute("tile", issue.TileId.ToString(CultureInfo.InvariantCulture));
                e.SetAttribute("hue", issue.Hue.ToString(CultureInfo.InvariantCulture));
                e.SetAttribute("autofix", issue.CanAutoFix.ToString());
                e.SetAttribute("default", issue.FixByDefault.ToString());
                e.SetAttribute("created", issue.Created.ToString("s", CultureInfo.InvariantCulture));
                e.SetAttribute("description", issue.Description ?? string.Empty);
                e.SetAttribute("note", issue.Note ?? string.Empty);
                root.AppendChild(e);
            }

            dom.Save(fileName);
        }

        public static List<RegionIssue> LoadXml(string fileName, out int mapId)
        {
            var result = new List<RegionIssue>();
            mapId = -1;

            var dom = new XmlDocument();
            dom.Load(fileName);
            XmlElement root = dom["RegionProtocol"];
            if (root == null)
            {
                return result;
            }

            int.TryParse(root.GetAttribute("map"), NumberStyles.Integer, CultureInfo.InvariantCulture, out mapId);

            foreach (XmlElement e in root.SelectNodes("Issue"))
            {
                var issue = new RegionIssue
                {
                    Type = Enum.TryParse(e.GetAttribute("type"), out RegionIssueType type) ? type : RegionIssueType.Manual,
                    Severity = Enum.TryParse(e.GetAttribute("severity"), out RegionIssueSeverity severity) ? severity : RegionIssueSeverity.Warning,
                    Status = Enum.TryParse(e.GetAttribute("status"), out RegionIssueStatus status) ? status : RegionIssueStatus.Open,
                    X = ParseInt(e, "x"),
                    Y = ParseInt(e, "y"),
                    Z = ParseInt(e, "z"),
                    TileId = ParseInt(e, "tile", -1),
                    Hue = ParseInt(e, "hue"),
                    CanAutoFix = bool.TryParse(e.GetAttribute("autofix"), out bool af) && af,
                    FixByDefault = bool.TryParse(e.GetAttribute("default"), out bool fd) && fd,
                    Description = e.GetAttribute("description"),
                    Note = e.GetAttribute("note")
                };

                if (DateTime.TryParse(e.GetAttribute("created"), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime created))
                {
                    issue.Created = created;
                }

                result.Add(issue);
            }

            return result;
        }

        private static int ParseInt(XmlElement e, string name, int fallback = 0)
        {
            return int.TryParse(e.GetAttribute(name), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : fallback;
        }

        #endregion
    }
}

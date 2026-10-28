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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.MapViewer.Editor.Transitions
{
    /// <summary>
    /// Terrain groups and transitions, saved as XML
    /// (default: <c>&lt;Output&gt;\UoFiddler_Transitions\Transitions.xml</c>)
    /// </summary>
    public sealed class TransitionLibrary
    {
        public const string FolderName = "UoFiddler_Transitions";
        public const string DefaultFileName = "Transitions.xml";

        public string FileName { get; set; }

        public List<TerrainGroup> Groups { get; } = new List<TerrainGroup>();

        public List<TransitionSet> Sets { get; } = new List<TransitionSet>();

        public event EventHandler Changed;

        public void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);

        public TerrainGroup GetGroup(string name)
        {
            return Groups.FirstOrDefault(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Ids of a group (empty if unknown)</summary>
        public IReadOnlyList<ushort> GroupIds(string name)
        {
            return (IReadOnlyList<ushort>)GetGroup(name)?.Ids ?? Array.Empty<ushort>();
        }

        /// <summary>Group that contains the land id (first match)</summary>
        public TerrainGroup GroupOf(ushort id)
        {
            return Groups.FirstOrDefault(g => g.Ids.Contains(id));
        }

        #region Location

        private static TransitionLibrary _default;

        /// <summary>The library of the default location (loaded once)</summary>
        public static TransitionLibrary Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new TransitionLibrary { FileName = DefaultPath() };
                    try
                    {
                        if (File.Exists(_default.FileName))
                        {
                            _default.Load(_default.FileName);
                        }
                    }
                    catch (Exception)
                    {
                        // a broken file must not block the editor, it is kept and not overwritten until saved
                    }
                }

                return _default;
            }
        }

        public static string DefaultPath()
        {
            string baseDir = !string.IsNullOrEmpty(Options.OutputPath) && Directory.Exists(Options.OutputPath)
                ? Options.OutputPath
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UoFiddler");
            return Path.Combine(baseDir, FolderName, DefaultFileName);
        }

        #endregion

        #region Load / save

        public void Load(string fileName)
        {
            var doc = new XmlDocument();
            doc.Load(fileName);
            Groups.Clear();
            Sets.Clear();

            XmlElement root = doc.DocumentElement;
            if (root == null)
            {
                return;
            }

            foreach (XmlElement g in root.SelectNodes("Group").OfType<XmlElement>())
            {
                var group = new TerrainGroup { Name = g.GetAttribute("name") };
                group.Ids.AddRange(TransitionSet.ParseIds(g.GetAttribute("ids")));
                Groups.Add(group);
            }

            foreach (XmlElement s in root.SelectNodes("Set").OfType<XmlElement>())
            {
                var set = new TransitionSet
                {
                    Name = s.GetAttribute("name"),
                    GroupA = s.GetAttribute("from"),
                    GroupB = s.GetAttribute("to"),
                    Notes = s.SelectSingleNode("Notes")?.InnerText ?? string.Empty
                };

                foreach (XmlElement slot in s.SelectNodes("Slot").OfType<XmlElement>())
                {
                    if (Enum.TryParse(slot.GetAttribute("name"), out TransitionSlot key))
                    {
                        set.Slots[key].AddRange(TransitionSet.ParseIds(slot.GetAttribute("ids")));
                    }
                }

                Sets.Add(set);
            }

            FileName = fileName;
        }

        public void Save(string fileName = null)
        {
            fileName ??= FileName;
            if (string.IsNullOrEmpty(fileName))
            {
                throw new InvalidOperationException("No file name.");
            }

            string dir = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
                WriteReadme(dir);
            }

            var settings = new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) };
            string temp = fileName + ".tmp";
            using (XmlWriter w = XmlWriter.Create(temp, settings))
            {
                w.WriteStartElement("Transitions");
                w.WriteAttributeString("version", "1");
                foreach (TerrainGroup g in Groups)
                {
                    w.WriteStartElement("Group");
                    w.WriteAttributeString("name", g.Name);
                    w.WriteAttributeString("ids", TransitionSet.FormatIds(g.Ids));
                    w.WriteEndElement();
                }

                foreach (TransitionSet s in Sets)
                {
                    w.WriteStartElement("Set");
                    w.WriteAttributeString("name", s.Name);
                    w.WriteAttributeString("from", s.GroupA);
                    w.WriteAttributeString("to", s.GroupB);
                    foreach (TransitionSlot slot in TransitionSet.AllSlots)
                    {
                        if (s.Slots[slot].Count == 0)
                        {
                            continue;
                        }

                        w.WriteStartElement("Slot");
                        w.WriteAttributeString("name", slot.ToString());
                        w.WriteAttributeString("ids", TransitionSet.FormatIds(s.Slots[slot]));
                        w.WriteEndElement();
                    }

                    if (!string.IsNullOrEmpty(s.Notes))
                    {
                        w.WriteElementString("Notes", s.Notes);
                    }

                    w.WriteEndElement();
                }

                w.WriteEndElement();
            }

            File.Move(temp, fileName, true);
            FileName = fileName;
        }

        private static void WriteReadme(string dir)
        {
            string readme = Path.Combine(dir, "README.txt");
            if (File.Exists(readme))
            {
                return;
            }

            File.WriteAllText(readme,
                "UoFiddler - terrain groups and transitions of the map editor\r\n" +
                "\r\n" +
                "Transitions.xml\r\n" +
                "  <Group name=\"Sand\" ids=\"0x0016,0x0017,...\"/>   land tiles of one terrain\r\n" +
                "  <Set name=\"Sand -> Grass\" from=\"Sand\" to=\"Grass\">  transition A -> B\r\n" +
                "    <Slot name=\"EdgeN\" ids=\"...\"/>   several ids = variants\r\n" +
                "\r\n" +
                "Slots (position of the cell relative to A, labels like UO-Caeri):\r\n" +
                "  EdgeN/E/S/W        1-1 .. 1-4  edges around A\r\n" +
                "  CornerNW/NE/SE/SW  2-1 .. 2-4  outer corners around A\r\n" +
                "  InnerSE/SW/NW/NE   3-1 .. 3-4  inner corners (A on two sides of the cell)\r\n" +
                "  BEdgeN/E/S/W       optional edges around B\r\n" +
                "\r\n" +
                "The file can be copied/shared; make a backup before editing it by hand.\r\n",
                Encoding.UTF8);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Suggests terrain groups from the names in tiledata (all land tiles with the same name and a texture).
        /// Existing groups are kept. Returns the number of new groups.
        /// </summary>
        public int AddGroupsFromTileNames(int minCount = 2)
        {
            int added = 0;
            LandData[] table = TileData.LandTable;
            if (table == null)
            {
                return 0;
            }

            var byName = new Dictionary<string, List<ushort>>(StringComparer.OrdinalIgnoreCase);
            for (int id = 1; id < table.Length && id <= ushort.MaxValue; id++)
            {
                string name = table[id].Name?.Trim();
                if (string.IsNullOrEmpty(name) || name.Equals("NoName", StringComparison.OrdinalIgnoreCase) || table[id].TextureId == 0)
                {
                    continue;
                }

                if (!byName.TryGetValue(name, out List<ushort> list))
                {
                    byName[name] = list = new List<ushort>();
                }

                list.Add((ushort)id);
            }

            foreach (KeyValuePair<string, List<ushort>> pair in byName.OrderBy(p => p.Key))
            {
                if (pair.Value.Count < minCount || GetGroup(pair.Key) != null)
                {
                    continue;
                }

                var group = new TerrainGroup { Name = pair.Key };
                group.Ids.AddRange(pair.Value);
                Groups.Add(group);
                added++;
            }

            return added;
        }

        #endregion
    }
}

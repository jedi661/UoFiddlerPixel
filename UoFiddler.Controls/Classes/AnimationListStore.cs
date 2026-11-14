/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Central access to Animationlist.xml (the list shown in the AnimationList tab).
 * Used by the AnimationListControl, the "add animation" dialog and the
 * AnimationEditForm, so new animations end up in the list immediately.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Ultima;

namespace UoFiddler.Controls.Classes
{
    /// <summary>One entry of Animationlist.xml</summary>
    public sealed class AnimationListEntry
    {
        public int Body { get; set; }

        /// <summary>0 = Monster, 1 = Sea, 2 = Animal, 3 = Human/Equipment</summary>
        public int Type { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>true = &lt;Equip&gt; (tab "Equipment"), false = &lt;Mob&gt;</summary>
        public bool IsEquipment { get; set; }

        public override string ToString() => $"{Name} ({Body})";
    }

    public sealed class AnimationListChangedEventArgs : EventArgs
    {
        public AnimationListChangedEventArgs(AnimationListEntry entry, bool removed)
        {
            Entry = entry;
            Removed = removed;
        }

        public AnimationListEntry Entry { get; }
        public bool Removed { get; }
    }

    public static class AnimationListStore
    {
        public const string FileName = "Animationlist.xml";

        /// <summary>Display names of the types (index = type value, 4 = equipment)</summary>
        public static readonly string[] TypeNames = { "Monster", "Sea", "Animal", "Human", "Equipment" };

        private static readonly Regex _hexSuffix = new Regex(@"\s\(0x[0-9A-Fa-f]+\)$", RegexOptions.Compiled);

        /// <summary>Raised after an entry was added, changed or removed (from any window)</summary>
        public static event EventHandler<AnimationListChangedEventArgs> Changed;

        /// <summary>The list that is loaded by the AnimationList tab</summary>
        public static string FilePath => Path.Combine(Options.AppDataPath, FileName);

        /// <summary>Removes the " (0x1A)" suffix that is only added for the display in the tree</summary>
        public static string CleanName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            string result = name.Trim();
            while (_hexSuffix.IsMatch(result))
            {
                result = _hexSuffix.Replace(result, string.Empty).TrimEnd();
            }

            return result;
        }

        /// <summary>Type 0..3 (4 = equipment is stored as type 3 with the Equip tag)</summary>
        public static int NormalizeType(int type) => type < 0 || type > 3 ? 3 : type;

        public static List<AnimationListEntry> Load()
        {
            var list = new List<AnimationListEntry>();
            XmlDocument dom = TryLoad(FilePath);
            XmlElement root = dom?["Graphics"];
            if (root == null)
            {
                return list;
            }

            foreach (XmlElement el in root.ChildNodes.OfType<XmlElement>())
            {
                bool equip = el.Name == "Equip";
                if (!equip && el.Name != "Mob")
                {
                    continue;
                }

                if (!int.TryParse(el.GetAttribute("body"), out int body))
                {
                    continue;
                }

                int.TryParse(el.GetAttribute("type"), out int type);
                list.Add(new AnimationListEntry
                {
                    Body = body,
                    Type = NormalizeType(type),
                    Name = CleanName(el.GetAttribute("name")),
                    IsEquipment = equip
                });
            }

            return list;
        }

        public static bool Contains(int body)
        {
            return Load().Any(e => e.Body == body);
        }

        public static AnimationListEntry Find(int body)
        {
            return Load().FirstOrDefault(e => e.Body == body);
        }

        /// <summary>
        /// Adds the entry or updates name/type of an existing one (same body and same tab).
        /// The file is written immediately (a backup Animationlist.xml.bak is kept).
        /// </summary>
        public static void AddOrUpdate(AnimationListEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            entry.Type = NormalizeType(entry.Type);
            entry.Name = CleanName(entry.Name);
            if (entry.Name.Length == 0)
            {
                entry.Name = SuggestName(entry.Body);
            }

            XmlDocument dom = TryLoad(FilePath) ?? CreateEmpty();
            XmlElement root = dom["Graphics"];
            if (root == null)
            {
                dom = CreateEmpty();
                root = dom["Graphics"];
            }

            string tag = entry.IsEquipment ? "Equip" : "Mob";
            XmlElement existing = root.SelectNodes(tag).OfType<XmlElement>()
                .FirstOrDefault(el => int.TryParse(el.GetAttribute("body"), out int b) && b == entry.Body);

            XmlElement elem = existing ?? dom.CreateElement(tag);
            elem.SetAttribute("name", entry.Name);
            elem.SetAttribute("body", entry.Body.ToString());
            elem.SetAttribute("type", entry.Type.ToString());

            if (existing == null)
            {
                // keep the list sorted by body inside its tag
                XmlElement after = root.SelectNodes(tag).OfType<XmlElement>()
                    .LastOrDefault(el => int.TryParse(el.GetAttribute("body"), out int b) && b < entry.Body);
                XmlElement first = root.SelectNodes(tag).OfType<XmlElement>().FirstOrDefault();
                if (after != null)
                {
                    root.InsertAfter(elem, after);
                }
                else if (first != null)
                {
                    root.InsertBefore(elem, first);
                }
                else
                {
                    root.AppendChild(elem);
                }
            }

            Save(dom);
            Changed?.Invoke(null, new AnimationListChangedEventArgs(entry, false));
        }

        /// <summary>Removes the entry of a body from the Mob or Equip tab. Returns false if it did not exist.</summary>
        public static bool Remove(int body, bool equipment)
        {
            XmlDocument dom = TryLoad(FilePath);
            XmlElement root = dom?["Graphics"];
            if (root == null)
            {
                return false;
            }

            string tag = equipment ? "Equip" : "Mob";
            List<XmlElement> found = root.SelectNodes(tag).OfType<XmlElement>()
                .Where(el => int.TryParse(el.GetAttribute("body"), out int b) && b == body).ToList();
            if (found.Count == 0)
            {
                return false;
            }

            var entry = new AnimationListEntry
            {
                Body = body,
                IsEquipment = equipment,
                Name = CleanName(found[0].GetAttribute("name"))
            };

            foreach (XmlElement el in found)
            {
                root.RemoveChild(el);
            }

            Save(dom);
            Changed?.Invoke(null, new AnimationListChangedEventArgs(entry, true));
            return true;
        }

        /// <summary>Writes a complete list (used by "save list" of the AnimationList tab)</summary>
        public static void SaveAll(IEnumerable<AnimationListEntry> entries, string fileName = null)
        {
            XmlDocument dom = CreateEmpty();
            XmlElement root = dom["Graphics"];
            List<AnimationListEntry> list = entries.ToList();
            foreach (AnimationListEntry e in list.Where(e => !e.IsEquipment).Concat(list.Where(e => e.IsEquipment)))
            {
                XmlElement elem = dom.CreateElement(e.IsEquipment ? "Equip" : "Mob");
                elem.SetAttribute("name", CleanName(e.Name));
                elem.SetAttribute("body", e.Body.ToString());
                elem.SetAttribute("type", NormalizeType(e.Type).ToString());
                root.AppendChild(elem);
            }

            Save(dom, fileName ?? FilePath);
        }

        #region Suggestions

        /// <summary>Name from the animation file (anim.mul / uop) or "anim_1234"</summary>
        public static string SuggestName(int body)
        {
            string name = null;
            try
            {
                name = Animations.GetFileName(body);
            }
            catch (Exception)
            {
                // ignored
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return $"anim_{body}";
            }

            // "anim3.mul" etc. is no useful name
            return name.EndsWith(".mul", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".uop", StringComparison.OrdinalIgnoreCase)
                ? $"anim_{body}"
                : $"{name} ({body})";
        }

        /// <summary>Type with the most defined actions (0..3), like "Save missing XML"</summary>
        public static int SuggestType(int body, string[][] actionNames)
        {
            int bestType = 0, bestCount = 0;
            for (int type = 0; type <= 3 && type < actionNames.Length; type++)
            {
                int defined = 0;
                for (int action = 0; action < actionNames[type].Length; action++)
                {
                    try
                    {
                        if (Animations.IsActionDefined(body, action, 0))
                        {
                            defined++;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                if (defined > bestCount)
                {
                    bestCount = defined;
                    bestType = type;
                }
            }

            return bestType;
        }

        /// <summary>true if action 0 of the body can be read from the client files (shown in the list)</summary>
        public static bool IsInClientFiles(int body)
        {
            try
            {
                return Animations.IsActionDefined(body, 0, 0);
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region File helpers

        private static XmlDocument TryLoad(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var dom = new XmlDocument();
            try
            {
                dom.Load(path);
                return dom;
            }
            catch (XmlException)
            {
                return null;
            }
        }

        private static XmlDocument CreateEmpty()
        {
            var dom = new XmlDocument();
            dom.AppendChild(dom.CreateXmlDeclaration("1.0", "utf-8", null));
            XmlElement root = dom.CreateElement("Graphics");
            root.AppendChild(dom.CreateComment("Entries in Mob tab"));
            root.AppendChild(dom.CreateComment("Name=Displayed name"));
            root.AppendChild(dom.CreateComment("body=Graphic"));
            root.AppendChild(dom.CreateComment("type=0:Monster, 1:Sea, 2:Animal, 3:Human/Equipment"));
            dom.AppendChild(root);
            return dom;
        }

        private static void Save(XmlDocument dom, string path = null)
        {
            path ??= FilePath;
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (File.Exists(path))
            {
                File.Copy(path, path + ".bak", true);
            }

            string temp = path + ".tmp";
            dom.Save(temp);
            File.Move(temp, path, true);
        }

        #endregion
    }
}

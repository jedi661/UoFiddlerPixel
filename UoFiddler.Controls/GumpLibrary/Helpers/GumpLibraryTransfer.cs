// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using Ultima;
using UoFiddler.Controls.ArtLibrary.Core;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.GumpLibrary.Helpers
{
    /// <summary>
    /// The only place where the gump library talks to the original gumps (Ultima.Gumps) and to the
    /// gump names of the Gumps tab (IDGumpNames.xml in the program folder). Changes go into memory like in
    /// the Gumps tab and are saved there ("Save").
    /// </summary>
    public static class GumpLibraryTransfer
    {
        public const string NamesFileName = "IDGumpNames.xml";

        /// <summary>First id of the male paperdoll gumps; female = male + 10000.</summary>
        public const int MaleGumpStart = 50000;
        public const int FemaleGumpStart = 60000;
        public const int GenderOffset = 10000;

        private static Dictionary<int, string> _names;
        private static DateTime _namesTime;

        public static bool IsOriginalAvailable
        {
            get
            {
                try
                {
                    return Gumps.GetCount() > 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static int OriginalMaxId => Math.Max(0, Gumps.GetCount() - 1);

        public static bool IsValidOriginalId(int id) => id >= 0 && id < Gumps.GetCount();

        public static bool IsOriginalOccupied(int id) => IsValidOriginalId(id) && Gumps.IsValidIndex(id);

        /// <summary>Original gump (including unsaved changes of the Gumps tab) or null.</summary>
        public static Bitmap GetOriginal(int id)
        {
            return IsOriginalOccupied(id) ? Gumps.GetGump(id) : null;
        }

        public static Bitmap GetOriginal(int id, out bool patched)
        {
            patched = false;
            return IsOriginalOccupied(id) ? Gumps.GetGump(id, out patched) : null;
        }

        /// <summary>Writes a gump into the original gumps (memory) and refreshes the Gumps tab.</summary>
        public static void WriteToOriginal(int id, Bitmap bitmap, object sender)
        {
            if (!IsValidOriginalId(id))
            {
                throw new ArgumentOutOfRangeException(nameof(id), id, $"Die ID liegt außerhalb der Gump-Datei (0 bis 0x{OriginalMaxId:X}).");
            }

            // independent ARGB1555 copy - the library bitmap is cached and shared
            Bitmap copy = ArtLibraryCodec.ToBitmap(ArtLibraryCodec.ToPixels(bitmap), bitmap.Width, bitmap.Height);
            Gumps.ReplaceGump(id, copy);
            ControlEvents.FireGumpChangeEvent(sender, id);
            Options.ChangedUltimaClass["Gumps"] = true;
        }

        public static void RemoveFromOriginal(int id, object sender)
        {
            if (!IsOriginalOccupied(id))
            {
                return;
            }

            Gumps.RemoveGump(id);
            ControlEvents.FireGumpChangeEvent(sender, id);
            Options.ChangedUltimaClass["Gumps"] = true;
        }

        /// <summary>The other gender gump of a paperdoll gump (50000+ male, 60000+ female) or -1.</summary>
        public static int GetGenderPartner(int id)
        {
            if (id >= FemaleGumpStart)
            {
                return id - GenderOffset;
            }

            return id >= MaleGumpStart ? id + GenderOffset : -1;
        }

        public static string GetGenderText(int id)
        {
            if (id >= FemaleGumpStart)
            {
                return "weiblich";
            }

            return id >= MaleGumpStart ? "männlich" : string.Empty;
        }

        #region [ IDGumpNames.xml ]

        public static string NamesPath => Path.Combine(Application.StartupPath, NamesFileName);

        /// <summary>Names of the Gumps tab (reloaded when the file changed).</summary>
        public static string GetOriginalName(int id)
        {
            LoadNames();
            return _names.TryGetValue(id, out string name) ? name : string.Empty;
        }

        public static IReadOnlyDictionary<int, string> GetOriginalNames()
        {
            LoadNames();
            return _names;
        }

        private static void LoadNames()
        {
            try
            {
                DateTime time = File.Exists(NamesPath) ? File.GetLastWriteTimeUtc(NamesPath) : DateTime.MinValue;
                if (_names != null && time == _namesTime)
                {
                    return;
                }

                _namesTime = time;
                _names = new Dictionary<int, string>();
                if (time == DateTime.MinValue)
                {
                    return;
                }

                XDocument doc = XDocument.Load(NamesPath);
                foreach (XElement element in doc.Root?.Elements("ID") ?? new List<XElement>())
                {
                    if (int.TryParse(element.Attribute("value")?.Value, out int id))
                    {
                        _names[id] = element.Attribute("name")?.Value ?? string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Options.Logger?.Warning(ex, "GumpLibrary: could not read {Path}", NamesPath);
                _names ??= new Dictionary<int, string>();
            }
        }

        /// <summary>
        /// Writes names into IDGumpNames.xml (same format as "Add ID Names" of the Gumps tab).
        /// The Gumps tab shows them after its next reload.
        /// </summary>
        public static int WriteOriginalNames(IReadOnlyDictionary<int, string> names)
        {
            XDocument doc = File.Exists(NamesPath) ? XDocument.Load(NamesPath) : new XDocument(new XElement("IDNames"));
            if (doc.Root == null)
            {
                doc.Add(new XElement("IDNames"));
            }

            int count = 0;
            foreach (KeyValuePair<int, string> pair in names)
            {
                if (string.IsNullOrWhiteSpace(pair.Value))
                {
                    continue;
                }

                string value = pair.Key.ToString();
                XElement existing = null;
                foreach (XElement element in doc.Root.Elements("ID"))
                {
                    if (element.Attribute("value")?.Value == value)
                    {
                        existing = element;
                        break;
                    }
                }

                if (existing == null)
                {
                    doc.Root.Add(new XElement("ID", new XAttribute("value", value), new XAttribute("name", pair.Value)));
                }
                else
                {
                    existing.SetAttributeValue("name", pair.Value);
                }

                ++count;
            }

            doc.Save(NamesPath);
            _names = null;
            return count;
        }

        #endregion

        /// <summary>Export file name like the Gumps tab: "0x0003.png".</summary>
        public static string GetExportFileName(int id, string extension) => $"0x{id:X4}.{extension.TrimStart('.')}";
    }
}

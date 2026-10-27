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
using System.Linq;
using System.Text;
using Ultima;

namespace UoFiddler.Controls.MapViewer.Build
{
    public enum TxtIdFormat
    {
        /// <summary>"0x" or letters A-F = hex, otherwise decimal</summary>
        Auto,
        Hex,
        Decimal
    }

    public enum MultiExportFormat
    {
        TxtHex,
        TxtDecimal,
        UoaBinary,
        UoaText,
        Wsc,
        Csv,
        Uox3,
        Xml
    }

    /// <summary>
    /// Import and export of building templates
    /// </summary>
    public static class MultiFormats
    {
        public const string ImportFilter =
            "All building files (*.txt;*.multi;*.uoa;*.wsc;*.csv)|*.txt;*.multi;*.uoa;*.wsc;*.csv|" +
            "Text (ID X Y Z [Flags] [Hue]) (*.txt;*.multi)|*.txt;*.multi|" +
            "UO Architect (*.uoa)|*.uoa|WSC (*.wsc)|*.wsc|CSV (*.csv)|*.csv|All files (*.*)|*.*";

        public static readonly string[] ImportExtensions = { ".txt", ".multi", ".uoa", ".wsc", ".csv" };

        public static string ExportFilter(MultiExportFormat format)
        {
            return format switch
            {
                MultiExportFormat.TxtHex => "Text, hex ids (*.txt)|*.txt",
                MultiExportFormat.TxtDecimal => "Text, decimal ids (*.txt)|*.txt",
                MultiExportFormat.UoaBinary => "UO Architect binary (*.uoa)|*.uoa",
                MultiExportFormat.UoaText => "UO Architect text (*.uoa.txt)|*.txt",
                MultiExportFormat.Wsc => "WSC (*.wsc)|*.wsc",
                MultiExportFormat.Csv => "CSV (*.csv)|*.csv",
                MultiExportFormat.Uox3 => "UOX3 (*.uox3)|*.uox3",
                MultiExportFormat.Xml => "XML (*.xml)|*.xml",
                _ => "All files (*.*)|*.*"
            };
        }

        public static string FormatName(MultiExportFormat format)
        {
            return format switch
            {
                MultiExportFormat.TxtHex => "Text (hex ids, like UOFiddler)",
                MultiExportFormat.TxtDecimal => "Text (decimal ids)",
                MultiExportFormat.UoaBinary => "UO Architect (binary, with hue)",
                MultiExportFormat.UoaText => "UO Architect (text)",
                MultiExportFormat.Wsc => "WSC",
                MultiExportFormat.Csv => "CSV (Punt's multi tool)",
                MultiExportFormat.Uox3 => "UOX3",
                MultiExportFormat.Xml => "XML",
                _ => format.ToString()
            };
        }

        #region Import

        /// <summary>
        /// Loads a template, the format is detected by extension and content
        /// </summary>
        public static MultiTemplate Load(string path, TxtIdFormat idFormat = TxtIdFormat.Auto)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            MultiTemplate t = ext switch
            {
                ".uoa" => IsUoaBinary(path) ? ReadUoaBinary(path) : FromSdk(path, Multis.ImportType.UOA, "UOA text"),
                ".wsc" => FromSdk(path, Multis.ImportType.WSC, "WSC"),
                ".csv" => FromSdk(path, Multis.ImportType.CSV, "CSV"),
                _ => ReadTxt(path, idFormat)
            };

            if (string.IsNullOrWhiteSpace(t.Name))
            {
                t.Name = Path.GetFileNameWithoutExtension(path);
            }

            return t;
        }

        /// <summary>
        /// Text format: one part per line "ItemID X Y Z [Flags] [Hue]". ItemID decimal (1904) or hex (0x31F4 / 31F4).
        /// Empty lines and comments (#, //, ;) are skipped.
        /// </summary>
        public static MultiTemplate ReadTxt(string path, TxtIdFormat idFormat = TxtIdFormat.Auto)
        {
            var lines = new List<(int no, string[] parts)>();
            int lineNo = 0;
            foreach (string raw in File.ReadLines(path))
            {
                lineNo++;
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("//") || line.StartsWith(";"))
                {
                    continue;
                }

                string[] parts = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4)
                {
                    throw new FormatException($"{Path.GetFileName(path)} line {lineNo}: expected 'ID X Y Z [Flags] [Hue]'");
                }

                lines.Add((lineNo, parts));
            }

            bool hex = idFormat == TxtIdFormat.Hex ||
                       (idFormat == TxtIdFormat.Auto && lines.Any(l => LooksHex(l.parts[0])));

            var t = new MultiTemplate { Name = Path.GetFileNameWithoutExtension(path), Source = hex ? "TXT (hex)" : "TXT (decimal)" };
            foreach ((int no, string[] p) in lines)
            {
                try
                {
                    t.Components.Add(new MultiComponent(
                        (ushort)ParseId(p[0], hex),
                        ParseInt(p[1]),
                        ParseInt(p[2]),
                        ParseInt(p[3]),
                        p.Length > 5 ? (ushort)ParseInt(p[5]) : (ushort)0,
                        p.Length > 4 ? ParseInt(p[4]) : 1));
                }
                catch (Exception ex) when (ex is FormatException || ex is OverflowException)
                {
                    throw new FormatException($"{Path.GetFileName(path)} line {no}: {ex.Message}", ex);
                }
            }

            return t;
        }

        private static bool LooksHex(string token)
        {
            if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return token.Any(c => (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
        }

        private static int ParseId(string s, bool hex)
        {
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.Parse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return hex
                ? int.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                : int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        private static int ParseInt(string s)
        {
            return s.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? int.Parse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                : int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        private static bool IsUoaBinary(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (fs.Length < 4)
                {
                    return false;
                }

                // text files start with "6 version"; binary files with a small ushort version
                int b0 = fs.ReadByte();
                int b1 = fs.ReadByte();
                return !(b0 >= '0' && b0 <= '9' && (b1 == ' ' || (b1 >= '0' && b1 <= '9')));
            }
        }

        /// <summary>
        /// UO Architect binary (.uoa), little endian:
        /// ushort version, 3 x string (byte marker=1, byte length, ascii): name, author, category,
        /// 4 x int (size), int count, count x 12 bytes: ushort id, short x, short y, short z, ushort level, ushort hue
        /// </summary>
        public static MultiTemplate ReadUoaBinary(string path)
        {
            using (var br = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.Latin1))
            {
                ushort version = br.ReadUInt16();
                var t = new MultiTemplate
                {
                    Source = $"UOA v{version}",
                    Name = ReadUoaString(br),
                    Author = ReadUoaString(br),
                    Category = ReadUoaString(br)
                };

                br.ReadInt32();
                br.ReadInt32();
                br.ReadInt32();
                br.ReadInt32();
                int count = br.ReadInt32();
                if (count < 0 || br.BaseStream.Position + (count * 12L) > br.BaseStream.Length)
                {
                    throw new FormatException("UOA: invalid number of parts - maybe another file version.");
                }

                for (int i = 0; i < count; i++)
                {
                    ushort id = br.ReadUInt16();
                    short x = br.ReadInt16();
                    short y = br.ReadInt16();
                    short z = br.ReadInt16();
                    ushort level = br.ReadUInt16();
                    ushort hue = br.ReadUInt16();
                    t.Components.Add(new MultiComponent(id, x, y, z, hue, 1, level));
                }

                return t;
            }
        }

        private static string ReadUoaString(BinaryReader br)
        {
            byte marker = br.ReadByte();
            if (marker == 0)
            {
                return string.Empty;
            }

            int len = br.ReadByte();
            return Encoding.Latin1.GetString(br.ReadBytes(len));
        }

        private static MultiTemplate FromSdk(string path, Multis.ImportType type, string source)
        {
            MultiComponentList list = Multis.LoadFromFile(path, type);
            if (list == null || list == MultiComponentList.Empty || list.SortedTiles == null)
            {
                throw new FormatException($"{Path.GetFileName(path)} could not be read as {source}.");
            }

            MultiTemplate t = FromComponentList(list, Path.GetFileNameWithoutExtension(path));
            t.Source = source;
            return t;
        }

        #endregion

        #region multi.mul

        public static MultiTemplate FromComponentList(MultiComponentList list, string name)
        {
            var t = new MultiTemplate { Name = name, Source = "multi.mul" };
            if (list?.SortedTiles == null)
            {
                return t;
            }

            foreach (MultiComponentList.MultiTileEntry e in list.SortedTiles)
            {
                t.Components.Add(new MultiComponent(e.ItemId, e.OffsetX, e.OffsetY, e.OffsetZ, 0, e.Flags));
            }

            return t;
        }

        public static MultiComponentList ToComponentList(MultiTemplate t)
        {
            var entries = t.Components.Select(c => new MultiComponentList.MultiTileEntry
            {
                ItemId = c.ItemId,
                OffsetX = (short)c.X,
                OffsetY = (short)c.Y,
                OffsetZ = (short)c.Z,
                Flags = c.Flags,
                Unk1 = 0
            }).ToList();

            return new MultiComponentList(entries);
        }

        #endregion

        #region Export

        public static void Export(MultiTemplate t, string path, MultiExportFormat format)
        {
            switch (format)
            {
                case MultiExportFormat.TxtHex:
                    WriteTxt(t, path, true);
                    break;
                case MultiExportFormat.TxtDecimal:
                    WriteTxt(t, path, false);
                    break;
                case MultiExportFormat.UoaBinary:
                    WriteUoaBinary(t, path);
                    break;
                case MultiExportFormat.UoaText:
                    ToComponentList(t).ExportToUOAFile(path);
                    break;
                case MultiExportFormat.Wsc:
                    ToComponentList(t).ExportToWscFile(path);
                    break;
                case MultiExportFormat.Csv:
                    ToComponentList(t).ExportToCsvFile(path);
                    break;
                case MultiExportFormat.Uox3:
                    ToComponentList(t).ExportToUox3File(path);
                    break;
                case MultiExportFormat.Xml:
                    ToComponentList(t).ExportToXmlFile(path, t.Name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format));
            }
        }

        /// <summary>
        /// "ID X Y Z Flags Hue", the hue column is only written if a part has a hue
        /// </summary>
        public static void WriteTxt(MultiTemplate t, string path, bool hex)
        {
            bool withHue = t.HasHues;
            using (var w = new StreamWriter(path, false, new UTF8Encoding(false)))
            {
                foreach (MultiComponent c in t.Components)
                {
                    string id = hex ? $"0x{c.ItemId:X4}" : c.ItemId.ToString(CultureInfo.InvariantCulture);
                    w.WriteLine(withHue
                        ? $"{id} {c.X} {c.Y} {c.Z} {c.Flags} {c.Hue}"
                        : $"{id} {c.X} {c.Y} {c.Z} {c.Flags}");
                }
            }
        }

        public static void WriteUoaBinary(MultiTemplate t, string path)
        {
            using (var bw = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write), Encoding.Latin1))
            {
                bw.Write((ushort)1);
                WriteUoaString(bw, t.Name);
                WriteUoaString(bw, t.Author);
                WriteUoaString(bw, t.Category);
                bw.Write(t.Width);
                bw.Write(t.Height);
                bw.Write(t.Width);
                bw.Write(t.Height);
                bw.Write(t.Components.Count);
                foreach (MultiComponent c in t.Components)
                {
                    bw.Write(c.ItemId);
                    bw.Write((short)c.X);
                    bw.Write((short)c.Y);
                    bw.Write((short)c.Z);
                    bw.Write((ushort)c.Level);
                    bw.Write(c.Hue);
                }
            }
        }

        private static void WriteUoaString(BinaryWriter bw, string text)
        {
            text ??= string.Empty;
            if (text.Length == 0)
            {
                bw.Write((byte)0);
                return;
            }

            byte[] bytes = Encoding.Latin1.GetBytes(text.Length > 255 ? text.Substring(0, 255) : text);
            bw.Write((byte)1);
            bw.Write((byte)bytes.Length);
            bw.Write(bytes);
        }

        #endregion
    }
}

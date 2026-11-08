/***************************************************************************
 *
 * MultiEditor – import / export of multi designs
 *
 * Supported import:
 *  - Text lists "id x y z [flag]" (hex 0x.., decimal or auto detect), UOA text, CSV
 *  - UOA binary (.uoa, keeps hues), WSC / Freeze (SECTION WORLDITEM), UOX3, XML
 *  - ServUO / RunUO addon scripts (.cs): arrays, AddComponent(...), AddComplexComponent(...)
 *  - MultiEditor project (.mep, json)
 *
 * Supported export:
 *  - ServUO addon script (.cs, array or line style, hues), text hex / decimal,
 *    WSC with hues, XML with hues, MultiEditor project (.mep)
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using Ultima;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// A tile with offsets relative to the multi center
    /// </summary>
    public sealed class ImportTile
    {
        public ushort Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int Flag { get; set; } = 1;
        public int Hue { get; set; }

        /// <summary>
        /// Furniture / decoration (addon only, not written into multi.mul)
        /// </summary>
        public bool Deco { get; set; }

        public ImportTile Clone()
        {
            return new ImportTile { Id = Id, X = X, Y = Y, Z = Z, Flag = Flag, Hue = Hue, Deco = Deco };
        }
    }

    public sealed class ImportResult
    {
        public string Name { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public List<ImportTile> Tiles { get; } = new List<ImportTile>();
        public List<string> Warnings { get; } = new List<string>();

        /// <summary>
        /// Only set when a project file was loaded
        /// </summary>
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? OriginX { get; set; }
        public int? OriginY { get; set; }

        /// <summary>
        /// House markers from a project: [kind, x, y, z] relative to the origin
        /// </summary>
        public List<int[]> Markers { get; set; }
    }

    public enum NumberMode
    {
        Auto,
        Decimal,
        Hex
    }

    public enum AddonScriptStyle
    {
        Array,
        Lines
    }

    public sealed class AddonExportOptions
    {
        public string Name { get; set; } = "MyMulti";
        public string Namespace { get; set; } = "Server.Items";
        public AddonScriptStyle Style { get; set; } = AddonScriptStyle.Array;
        public bool IncludeInvisibleFlag { get; set; } = true;
        public bool HexIds { get; set; } = true;
        public string DeedName { get; set; } = string.Empty;
        public bool Redeedable { get; set; } = true;
    }

    public static class MultiFormats
    {
        public const string ImportFilter =
            "All supported|*.txt;*.uoa;*.wsc;*.csv;*.uox3;*.xml;*.cs;*.mep|" +
            "Text list (*.txt)|*.txt|UO Architect (*.uoa)|*.uoa|WSC / Freeze (*.wsc)|*.wsc|CSV (*.csv)|*.csv|" +
            "UOX3 (*.uox3)|*.uox3|XML (*.xml)|*.xml|ServUO/RunUO addon script (*.cs)|*.cs|MultiEditor project (*.mep)|*.mep|All files|*.*";

        private static readonly Encoding _ansi = CreateAnsi();

        private static Encoding CreateAnsi()
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                return Encoding.GetEncoding(1252);
            }
            catch
            {
                return Encoding.Latin1;
            }
        }

        private static readonly Regex _number = new Regex(@"^[+-]?(0x[0-9a-fA-F]+|[0-9a-fA-F]+)$", RegexOptions.Compiled);

        #region Import

        /// <summary>
        /// Loads a file, the format is detected by extension and content
        /// </summary>
        public static ImportResult Load(string fileName, NumberMode mode = NumberMode.Auto)
        {
            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            ImportResult result;

            switch (ext)
            {
                case ".cs":
                    result = LoadServUOScript(fileName);
                    break;
                case ".uoa":
                    result = IsUoaBinary(fileName) ? LoadUoaBinary(fileName) : LoadTextList(fileName, mode);
                    break;
                case ".wsc":
                    result = LoadWsc(fileName);
                    break;
                case ".uox3":
                    result = LoadUox3(fileName);
                    break;
                case ".xml":
                    result = LoadXml(fileName);
                    break;
                case ".mep":
                    result = LoadProject(fileName);
                    break;
                default:
                    {
                        string head = ReadHead(fileName);
                        if (head.Contains("SECTION WORLDITEM"))
                        {
                            result = LoadWsc(fileName);
                        }
                        else if (head.Contains("[HOUSE ITEM"))
                        {
                            result = LoadUox3(fileName);
                        }
                        else if (head.Contains("BaseAddon") || head.Contains("AddComponent") || head.Contains("using Server"))
                        {
                            result = LoadServUOScript(fileName);
                        }
                        else if (IsUoaBinary(fileName))
                        {
                            result = LoadUoaBinary(fileName);
                        }
                        else
                        {
                            result = LoadTextList(fileName, mode);
                        }

                        break;
                    }
            }

            if (string.IsNullOrEmpty(result.Name))
            {
                result.Name = Path.GetFileNameWithoutExtension(fileName);
            }

            return result;
        }

        private static string ReadHead(string fileName)
        {
            using var sr = new StreamReader(fileName, _ansi);
            char[] buffer = new char[8192];
            int n = sr.Read(buffer, 0, buffer.Length);
            return new string(buffer, 0, n);
        }

        private static bool IsUoaBinary(string fileName)
        {
            try
            {
                using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (fs.Length < 20)
                {
                    return false;
                }

                int b0 = fs.ReadByte();
                int b1 = fs.ReadByte();
                return (b0 == 1 || b0 == 2) && b1 == 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryParseNumber(string token, bool hex, out int value)
        {
            value = 0;
            token = token.Trim();
            if (token.Length == 0)
            {
                return false;
            }

            bool negative = false;
            if (token[0] == '-' || token[0] == '+')
            {
                negative = token[0] == '-';
                token = token.Substring(1);
            }

            bool ok;
            if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                ok = int.TryParse(token.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
            }
            else if (hex)
            {
                ok = int.TryParse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
            }
            else
            {
                ok = int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
            }

            if (negative)
            {
                value = -value;
            }

            return ok;
        }

        /// <summary>
        /// Generic list "id x y z [flag] [hue]" separated by spaces, tabs, comma or semicolon.
        /// Lines with less than 4 numbers (headers, comments) are ignored.
        /// </summary>
        public static ImportResult LoadTextList(string fileName, NumberMode mode)
        {
            var result = new ImportResult { Format = "Text" };
            var rows = new List<string[]>();

            foreach (string raw in File.ReadAllLines(fileName, _ansi))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("//") || line.StartsWith(";"))
                {
                    continue;
                }

                string[] parts = line.Split(new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4)
                {
                    continue;
                }

                // the first 4 tokens must be numbers (x,y,z always decimal)
                if (!_number.IsMatch(parts[0]))
                {
                    continue;
                }

                bool coordsOk = true;
                for (int i = 1; i < 4; ++i)
                {
                    if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                    {
                        coordsOk = false;
                        break;
                    }
                }

                if (!coordsOk)
                {
                    continue;
                }

                rows.Add(parts);
            }

            bool hex;
            switch (mode)
            {
                case NumberMode.Hex:
                    hex = true;
                    break;
                case NumberMode.Decimal:
                    hex = false;
                    break;
                default:
                    hex = DetectHex(rows.Select(r => r[0]).ToList());
                    break;
            }

            result.Format = hex ? "Text (hex)" : "Text (decimal)";

            foreach (string[] parts in rows)
            {
                if (!TryParseNumber(parts[0], hex, out int id))
                {
                    continue;
                }

                var tile = new ImportTile
                {
                    Id = (ushort)id,
                    X = int.Parse(parts[1], CultureInfo.InvariantCulture),
                    Y = int.Parse(parts[2], CultureInfo.InvariantCulture),
                    Z = int.Parse(parts[3], CultureInfo.InvariantCulture),
                    Flag = 1
                };

                if (parts.Length > 4 && TryParseNumber(parts[4], false, out int flag))
                {
                    tile.Flag = flag;
                }

                if (parts.Length > 5 && TryParseNumber(parts[5], false, out int hue))
                {
                    tile.Hue = hue;
                }

                result.Tiles.Add(tile);
            }

            if (result.Tiles.Count == 0)
            {
                result.Warnings.Add("No tiles found.");
            }

            return result;
        }

        /// <summary>
        /// Decides if ids without 0x prefix are hex. Uses letters a-f and the number of valid statics as hint.
        /// </summary>
        private static bool DetectHex(List<string> ids)
        {
            if (ids.Count == 0)
            {
                return false;
            }

            List<string> plain = ids.Where(t => !t.StartsWith("0x", StringComparison.OrdinalIgnoreCase)).ToList();
            if (plain.Count == 0)
            {
                return true; // everything has 0x, the mode does not matter
            }

            if (plain.Any(t => t.IndexOfAny("abcdefABCDEF".ToCharArray()) >= 0))
            {
                return true;
            }

            int validDec = 0;
            int validHex = 0;
            foreach (string t in plain.Take(500))
            {
                if (TryParseNumber(t, false, out int d) && IsValidStaticSafe(d))
                {
                    ++validDec;
                }

                if (TryParseNumber(t, true, out int h) && IsValidStaticSafe(h))
                {
                    ++validHex;
                }
            }

            return validHex > validDec * 1.5;
        }

        private static bool IsValidStaticSafe(int id)
        {
            try
            {
                return id >= 0 && id < 0x10000 && Art.IsValidStatic(id);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// UO Architect binary file (version 1). Hue is kept.
        /// </summary>
        public static ImportResult LoadUoaBinary(string fileName)
        {
            var result = new ImportResult { Format = "UOA binary" };
            using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(fs);
            short version = reader.ReadInt16();
            if (version < 1 || version > 2)
            {
                result.Warnings.Add($"Unknown UOA version {version}.");
                return result;
            }

            if (version >= 2)
            {
                string extra = ReadUoaString(reader); // version 2: one more text in front of the name
                result.Name = extra;
            }

            string name = ReadUoaString(reader);
            if (!string.IsNullOrEmpty(name))
            {
                result.Name = name;
            }

            _ = ReadUoaString(reader); // category
            _ = ReadUoaString(reader); // subsection
            _ = reader.ReadInt32();
            _ = reader.ReadInt32();
            _ = reader.ReadInt32();
            _ = reader.ReadInt32();
            int count = reader.ReadInt32();
            if (count < 0 || (long)count * 12 > fs.Length - fs.Position)
            {
                result.Warnings.Add($"UOA: invalid component count {count}.");
                return result;
            }

            result.Format = $"UOA binary v{version}";

            for (int i = 0; i < count && fs.Position + 12 <= fs.Length; ++i)
            {
                var tile = new ImportTile
                {
                    Id = (ushort)reader.ReadInt16(),
                    X = reader.ReadInt16(),
                    Y = reader.ReadInt16(),
                    Z = reader.ReadInt16(),
                    Flag = 1
                };
                _ = reader.ReadInt16(); // level
                tile.Hue = reader.ReadInt16();
                result.Tiles.Add(tile);
            }

            return result;
        }

        private static string ReadUoaString(BinaryReader reader)
        {
            byte flag = reader.ReadByte();
            return flag == 0 ? string.Empty : reader.ReadString();
        }

        /// <summary>
        /// SECTION WORLDITEM blocks (WSC / UO Fiddler freeze export). ID/X/Y/Z/COLOR.
        /// </summary>
        public static ImportResult LoadWsc(string fileName)
        {
            var result = new ImportResult { Format = "WSC" };
            ImportTile current = null;

            foreach (string raw in File.ReadAllLines(fileName, _ansi))
            {
                string line = raw.Trim();
                if (line.StartsWith("SECTION WORLDITEM", StringComparison.OrdinalIgnoreCase))
                {
                    if (current != null)
                    {
                        result.Tiles.Add(current);
                    }

                    current = new ImportTile();
                    continue;
                }

                if (current == null)
                {
                    continue;
                }

                string[] parts = line.Split(new[] { ' ', '\t', '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    continue;
                }

                string key = parts[0].ToUpperInvariant();
                if (!TryParseNumber(parts[1], false, out int value))
                {
                    continue;
                }

                switch (key)
                {
                    case "ID":
                        current.Id = (ushort)value;
                        break;
                    case "X":
                        current.X = value;
                        break;
                    case "Y":
                        current.Y = value;
                        break;
                    case "Z":
                        current.Z = value;
                        break;
                    case "COLOR":
                    case "HUE":
                        current.Hue = value;
                        break;
                }
            }

            if (current != null)
            {
                result.Tiles.Add(current);
            }

            NormalizeWorldCoordinates(result);
            return result;
        }

        /// <summary>
        /// Freeze exports contain world coordinates, move them near 0,0 so the multi stays small
        /// </summary>
        private static void NormalizeWorldCoordinates(ImportResult result)
        {
            if (result.Tiles.Count == 0)
            {
                return;
            }

            int minX = result.Tiles.Min(t => t.X);
            int minY = result.Tiles.Min(t => t.Y);
            int maxX = result.Tiles.Max(t => t.X);
            int maxY = result.Tiles.Max(t => t.Y);
            if (Math.Abs(minX) < 200 && Math.Abs(minY) < 200)
            {
                return;
            }

            int cx = (minX + maxX) / 2;
            int cy = (minY + maxY) / 2;
            foreach (ImportTile t in result.Tiles)
            {
                t.X -= cx;
                t.Y -= cy;
            }

            result.Warnings.Add($"World coordinates detected, moved by {-cx},{-cy} (original center {cx},{cy}).");
        }

        public static ImportResult LoadUox3(string fileName)
        {
            var result = new ImportResult { Format = "UOX3" };
            ImportTile current = null;
            foreach (string raw in File.ReadAllLines(fileName, _ansi))
            {
                string line = raw.Trim();
                if (line.StartsWith("[HOUSE ITEM", StringComparison.OrdinalIgnoreCase))
                {
                    if (current != null)
                    {
                        result.Tiles.Add(current);
                    }

                    current = new ImportTile();
                    continue;
                }

                if (current == null)
                {
                    continue;
                }

                int eq = line.IndexOf('=');
                if (eq <= 0)
                {
                    continue;
                }

                string key = line.Substring(0, eq).Trim().ToUpperInvariant();
                if (!TryParseNumber(line.Substring(eq + 1), false, out int value))
                {
                    continue;
                }

                switch (key)
                {
                    case "ITEM":
                        current.Id = (ushort)value;
                        break;
                    case "X":
                        current.X = value;
                        break;
                    case "Y":
                        current.Y = value;
                        break;
                    case "Z":
                        current.Z = value;
                        break;
                    case "COLOR":
                    case "HUE":
                        current.Hue = value;
                        break;
                }
            }

            if (current != null)
            {
                result.Tiles.Add(current);
            }

            return result;
        }

        public static ImportResult LoadXml(string fileName)
        {
            var result = new ImportResult { Format = "XML" };
            var doc = new XmlDocument();
            doc.Load(fileName);
            XmlNodeList items = doc.SelectNodes("//Item");
            if (items == null)
            {
                return result;
            }

            XmlElement entry = doc.DocumentElement;
            if (entry != null && entry.HasAttribute("ID"))
            {
                result.Name = entry.GetAttribute("ID");
            }

            foreach (XmlElement item in items)
            {
                var tile = new ImportTile();
                if (TryParseNumber(item.GetAttribute("ID"), false, out int id))
                {
                    tile.Id = (ushort)id;
                }

                TryParseNumber(item.GetAttribute("X"), false, out int x);
                TryParseNumber(item.GetAttribute("Y"), false, out int y);
                TryParseNumber(item.GetAttribute("Z"), false, out int z);
                tile.X = x;
                tile.Y = y;
                tile.Z = z;
                if (item.HasAttribute("Hue") && TryParseNumber(item.GetAttribute("Hue"), false, out int hue))
                {
                    tile.Hue = hue;
                }

                if (item.HasAttribute("Flag") && TryParseNumber(item.GetAttribute("Flag"), false, out int flag))
                {
                    tile.Flag = flag;
                }

                result.Tiles.Add(tile);
            }

            return result;
        }

        private const string _num = @"[+-]?\s*(?:0x[0-9a-fA-F]+|\d+)";

        private static readonly Regex _arrayBlock = new Regex(@"new\s+int\s*\[\s*,\s*\]\s*\{(?<body>.*?)\}\s*;", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex _arrayRow = new Regex(@"\{(?<row>[^{}]*)\}", RegexOptions.Compiled);

        private static readonly Regex _complex = new Regex(
            @"AddComplexComponent\s*\(\s*(?:\(\s*BaseAddon\s*\)\s*)?this\s*,\s*(?<id>" + _num + @")\s*,\s*(?<x>" + _num + @")\s*,\s*(?<y>" + _num + @")\s*,\s*(?<z>" + _num + @")(?:\s*,\s*(?<hue>" + _num + @"))?",
            RegexOptions.Compiled);

        private static readonly Regex _directAdd = new Regex(
            @"AddComponent\s*\(\s*new\s+[\w.]*Component\s*\(\s*(?<id>" + _num + @")[^()]*\)\s*(?:\{[^{}]*?Hue\s*=\s*(?<hue>" + _num + @")[^{}]*\})?\s*,\s*(?<x>" + _num + @")\s*,\s*(?<y>" + _num + @")\s*,\s*(?<z>" + _num + @")\s*\)",
            RegexOptions.Compiled);

        private static readonly Regex _varNew = new Regex(
            @"(?<var>\b\w+)\s*=\s*new\s+[\w.]*Component\s*\(\s*(?<id>" + _num + @")[^()]*\)",
            RegexOptions.Compiled);

        private static readonly Regex _varHue = new Regex(@"(?<var>\b\w+)\s*\.\s*Hue\s*=\s*(?<hue>" + _num + @")", RegexOptions.Compiled);

        private static readonly Regex _varAdd = new Regex(
            @"AddComponent\s*\(\s*(?<var>[A-Za-z_]\w*)\s*,\s*(?<x>" + _num + @")\s*,\s*(?<y>" + _num + @")\s*,\s*(?<z>" + _num + @")\s*\)",
            RegexOptions.Compiled);

        private static readonly Regex _className = new Regex(@"class\s+(?<name>\w+?)(?:Addon)?\s*:\s*BaseAddon\b", RegexOptions.Compiled);

        private static readonly Regex _comments = new Regex(@"//[^\n]*|/\*.*?\*/", RegexOptions.Singleline | RegexOptions.Compiled);

        private static int Num(string s)
        {
            TryParseNumber(s.Replace(" ", string.Empty), false, out int v);
            return v;
        }

        /// <summary>
        /// Reads ServUO / RunUO addon scripts (UOArchitect / AddonGenerator style and hand written ones)
        /// </summary>
        public static ImportResult LoadServUOScript(string fileName)
        {
            string code = File.ReadAllText(fileName);
            return ParseServUOScript(code);
        }

        public static ImportResult ParseServUOScript(string code)
        {
            var result = new ImportResult { Format = "ServUO addon" };
            code = _comments.Replace(code, string.Empty);

            Match nameMatch = _className.Match(code);
            if (nameMatch.Success)
            {
                result.Name = nameMatch.Groups["name"].Value;
            }

            // 1. int[,] arrays {id, x, y, z} (simple components), sometimes with more columns
            foreach (Match block in _arrayBlock.Matches(code))
            {
                foreach (Match row in _arrayRow.Matches(block.Groups["body"].Value))
                {
                    string[] values = row.Groups["row"].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (values.Length < 4)
                    {
                        continue;
                    }

                    int[] n = values.Select(v => Num(v.Trim())).ToArray();
                    int id = n[0], x = n[1], y = n[2], z = n[3];

                    // some generators write {x, y, z, id}
                    if (Math.Abs(n[0]) <= 64 && n[3] > 127 && Math.Abs(n[1]) <= 64 && Math.Abs(n[2]) <= 64)
                    {
                        x = n[0];
                        y = n[1];
                        z = n[2];
                        id = n[3];
                    }

                    if (id <= 0 || id > 0xFFFF)
                    {
                        continue;
                    }

                    var tile = new ImportTile { Id = (ushort)id, X = x, Y = y, Z = z };
                    if (n.Length >= 5 && n[4] > 0 && n[4] < 0x4000)
                    {
                        tile.Hue = n[4];
                    }

                    result.Tiles.Add(tile);
                }
            }

            // 2. AddComplexComponent((BaseAddon)this, id, x, y, z, hue, ...)
            foreach (Match m in _complex.Matches(code))
            {
                var tile = new ImportTile
                {
                    Id = (ushort)Num(m.Groups["id"].Value),
                    X = Num(m.Groups["x"].Value),
                    Y = Num(m.Groups["y"].Value),
                    Z = Num(m.Groups["z"].Value)
                };
                if (m.Groups["hue"].Success)
                {
                    tile.Hue = Math.Max(0, Num(m.Groups["hue"].Value));
                }

                result.Tiles.Add(tile);
            }

            // 3. AddComponent(new AddonComponent(id) { Hue = h }, x, y, z)
            foreach (Match m in _directAdd.Matches(code))
            {
                var tile = new ImportTile
                {
                    Id = (ushort)Num(m.Groups["id"].Value),
                    X = Num(m.Groups["x"].Value),
                    Y = Num(m.Groups["y"].Value),
                    Z = Num(m.Groups["z"].Value)
                };
                if (m.Groups["hue"].Success)
                {
                    tile.Hue = Num(m.Groups["hue"].Value);
                }

                result.Tiles.Add(tile);
            }

            // 4. variable style: ac = new AddonComponent(id); ac.Hue = 5; AddComponent(ac, x, y, z);
            var events = new List<(int pos, int kind, Match m)>();
            events.AddRange(_varNew.Matches(code).Cast<Match>().Select(m => (m.Index, 0, m)));
            events.AddRange(_varHue.Matches(code).Cast<Match>().Select(m => (m.Index, 1, m)));
            events.AddRange(_varAdd.Matches(code).Cast<Match>().Select(m => (m.Index, 2, m)));
            var vars = new Dictionary<string, ImportTile>();
            foreach (var (_, kind, m) in events.OrderBy(e => e.pos))
            {
                string var = m.Groups["var"].Value;
                switch (kind)
                {
                    case 0:
                        vars[var] = new ImportTile { Id = (ushort)Num(m.Groups["id"].Value) };
                        break;
                    case 1:
                        if (vars.TryGetValue(var, out ImportTile hued))
                        {
                            hued.Hue = Num(m.Groups["hue"].Value);
                        }

                        break;
                    case 2:
                        if (vars.TryGetValue(var, out ImportTile t))
                        {
                            ImportTile copy = t.Clone();
                            copy.X = Num(m.Groups["x"].Value);
                            copy.Y = Num(m.Groups["y"].Value);
                            copy.Z = Num(m.Groups["z"].Value);
                            result.Tiles.Add(copy);
                        }

                        break;
                }
            }

            if (result.Tiles.Count == 0)
            {
                result.Warnings.Add("No addon components found in script.");
            }

            return result;
        }

        private sealed class ProjectFile
        {
            public string Name { get; set; }
            public int Version { get; set; } = 1;
            public int Width { get; set; }
            public int Height { get; set; }
            public int OriginX { get; set; }
            public int OriginY { get; set; }
            public List<int[]> Tiles { get; set; } = new List<int[]>();
            public string Background { get; set; }
            public List<int[]> Markers { get; set; }
        }

        public static ImportResult LoadProject(string fileName)
        {
            var result = new ImportResult { Format = "MultiEditor project" };
            ProjectFile p = JsonSerializer.Deserialize<ProjectFile>(File.ReadAllText(fileName));
            if (p == null)
            {
                return result;
            }

            result.Name = p.Name;
            result.Width = p.Width;
            result.Height = p.Height;
            result.OriginX = p.OriginX;
            result.OriginY = p.OriginY;
            result.Markers = p.Markers;
            foreach (int[] t in p.Tiles)
            {
                if (t.Length < 4)
                {
                    continue;
                }

                result.Tiles.Add(new ImportTile
                {
                    Id = (ushort)t[0],
                    X = t[1],
                    Y = t[2],
                    Z = t[3],
                    Flag = t.Length > 4 ? t[4] : 1,
                    Hue = t.Length > 5 ? t[5] : 0,
                    Deco = t.Length > 6 && t[6] != 0
                });
            }

            return result;
        }

        /// <summary>
        /// Converts a multi of the Ultima SDK to import tiles
        /// </summary>
        public static List<ImportTile> FromSdk(MultiComponentList list)
        {
            var tiles = new List<ImportTile>();
            if (list == null || list == MultiComponentList.Empty)
            {
                return tiles;
            }

            foreach (MultiComponentList.MultiTileEntry e in list.SortedTiles)
            {
                tiles.Add(new ImportTile { Id = e.ItemId, X = e.OffsetX, Y = e.OffsetY, Z = e.OffsetZ, Flag = e.Flags });
            }

            return tiles;
        }

        #endregion

        #region Export

        private static IEnumerable<ImportTile> Exportable(IEnumerable<ImportTile> tiles, bool includeInvisible)
        {
            return tiles.Where(t => t.Id != 0x1 && (includeInvisible || t.Flag != 0))
                        .OrderBy(t => t.Z).ThenBy(t => t.X + t.Y).ThenBy(t => t.X);
        }

        public static void SaveTextList(string fileName, IEnumerable<ImportTile> tiles, bool hex, bool withHue)
        {
            var sb = new StringBuilder();
            foreach (ImportTile t in tiles)
            {
                string id = hex ? $"0x{t.Id:X4}" : t.Id.ToString(CultureInfo.InvariantCulture);
                sb.Append(id).Append(' ').Append(t.X).Append(' ').Append(t.Y).Append(' ').Append(t.Z).Append(' ').Append(t.Flag);
                if (withHue && t.Hue > 0)
                {
                    sb.Append(' ').Append(t.Hue);
                }

                sb.AppendLine();
            }

            File.WriteAllText(fileName, sb.ToString(), _ansi);
        }

        public static void SaveWsc(string fileName, IList<ImportTile> tiles)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < tiles.Count; ++i)
            {
                ImportTile t = tiles[i];
                sb.AppendLine($"SECTION WORLDITEM {i}");
                sb.AppendLine("{");
                sb.AppendLine($"\tID\t{t.Id}");
                sb.AppendLine($"\tX\t{t.X}");
                sb.AppendLine($"\tY\t{t.Y}");
                sb.AppendLine($"\tZ\t{t.Z}");
                sb.AppendLine($"\tColor\t{t.Hue}");
                sb.AppendLine("}");
            }

            File.WriteAllText(fileName, sb.ToString(), _ansi);
        }

        public static void SaveXml(string fileName, IEnumerable<ImportTile> tiles, string name)
        {
            using var w = XmlWriter.Create(fileName, new XmlWriterSettings { Indent = true });
            w.WriteStartElement("Entry");
            w.WriteAttributeString("ID", name);
            foreach (ImportTile t in tiles)
            {
                w.WriteStartElement("Item");
                w.WriteAttributeString("X", t.X.ToString(CultureInfo.InvariantCulture));
                w.WriteAttributeString("Y", t.Y.ToString(CultureInfo.InvariantCulture));
                w.WriteAttributeString("Z", t.Z.ToString(CultureInfo.InvariantCulture));
                w.WriteAttributeString("ID", $"0x{t.Id:X4}");
                if (t.Hue > 0)
                {
                    w.WriteAttributeString("Hue", t.Hue.ToString(CultureInfo.InvariantCulture));
                }

                if (t.Flag != 1)
                {
                    w.WriteAttributeString("Flag", t.Flag.ToString(CultureInfo.InvariantCulture));
                }

                w.WriteEndElement();
            }

            w.WriteEndElement();
        }

        public static void SaveProject(string fileName, string name, int width, int height, int originX, int originY, IEnumerable<ImportTile> tiles, List<int[]> markers = null)
        {
            var p = new ProjectFile
            {
                Name = name,
                Width = width,
                Height = height,
                OriginX = originX,
                OriginY = originY,
                Tiles = tiles.Select(t => new[] { (int)t.Id, t.X, t.Y, t.Z, t.Flag, t.Hue, t.Deco ? 1 : 0 }).ToList(),
                Markers = markers
            };
            File.WriteAllText(fileName, JsonSerializer.Serialize(p, new JsonSerializerOptions { WriteIndented = false }));
        }

        public static string SanitizeClassName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "MyMulti";
            }

            name = name.Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("Ä", "Ae").Replace("Ö", "Oe").Replace("Ü", "Ue").Replace("ß", "ss");
            var sb = new StringBuilder();
            bool upper = true;
            foreach (char c in name)
            {
                if (c > 127)
                {
                    upper = true;
                    continue;
                }

                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(upper ? char.ToUpperInvariant(c) : c);
                    upper = false;
                }
                else
                {
                    upper = true;
                }
            }

            string s = sb.ToString();
            if (s.Length == 0)
            {
                s = "MyMulti";
            }

            if (char.IsDigit(s[0]))
            {
                s = "M" + s;
            }

            if (s.EndsWith("Addon", StringComparison.Ordinal))
            {
                s = s.Substring(0, s.Length - 5);
            }

            return s;
        }

        /// <summary>
        /// Creates a ServUO addon script (Addon + AddonDeed)
        /// </summary>
        public static string CreateServUOAddon(IEnumerable<ImportTile> tiles, AddonExportOptions o)
        {
            string cls = SanitizeClassName(o.Name);
            string deedName = string.IsNullOrWhiteSpace(o.DeedName) ? o.Name : o.DeedName;
            List<ImportTile> list = Exportable(tiles, o.IncludeInvisibleFlag).ToList();
            List<ImportTile> simple = list.Where(t => t.Hue <= 0).ToList();
            List<ImportTile> complex = list.Where(t => t.Hue > 0).ToList();

            string Id(int id) => o.HexIds ? $"0x{id:X4}" : id.ToString(CultureInfo.InvariantCulture);

            var sb = new StringBuilder();
            sb.AppendLine("// ------------------------------------------------------------------");
            sb.AppendLine($"// {cls}Addon  -  generated by UO Fiddler MultiEditor  {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"// Components: {list.Count}");
            sb.AppendLine("// ------------------------------------------------------------------");
            sb.AppendLine("using System;");
            sb.AppendLine("using Server;");
            sb.AppendLine("using Server.Items;");
            sb.AppendLine();
            sb.AppendLine($"namespace {o.Namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {cls}Addon : BaseAddon");
            sb.AppendLine("    {");

            if (o.Style == AddonScriptStyle.Array && simple.Count > 0)
            {
                sb.AppendLine("        private static readonly int[,] m_AddOnSimpleComponents = new int[,]");
                sb.AppendLine("        {");
                for (int i = 0; i < simple.Count; ++i)
                {
                    ImportTile t = simple[i];
                    sb.Append($"            {{{Id(t.Id)}, {t.X}, {t.Y}, {t.Z}}}");
                    sb.AppendLine(i < simple.Count - 1 ? "," : string.Empty);
                }

                sb.AppendLine("        };");
                sb.AppendLine();
            }

            sb.AppendLine($"        public override BaseAddonDeed Deed => new {cls}AddonDeed();");
            sb.AppendLine();
            sb.AppendLine("        [Constructable]");
            sb.AppendLine($"        public {cls}Addon()");
            sb.AppendLine("        {");

            if (o.Style == AddonScriptStyle.Array && simple.Count > 0)
            {
                sb.AppendLine("            for (int i = 0; i < m_AddOnSimpleComponents.Length / 4; i++)");
                sb.AppendLine("            {");
                sb.AppendLine("                AddComponent(new AddonComponent(m_AddOnSimpleComponents[i, 0]), m_AddOnSimpleComponents[i, 1], m_AddOnSimpleComponents[i, 2], m_AddOnSimpleComponents[i, 3]);");
                sb.AppendLine("            }");
            }
            else
            {
                foreach (ImportTile t in simple)
                {
                    sb.AppendLine($"            AddComponent(new AddonComponent({Id(t.Id)}), {t.X}, {t.Y}, {t.Z});");
                }
            }

            if (complex.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("            // hued components");
                foreach (ImportTile t in complex)
                {
                    sb.AppendLine($"            AddComplexComponent(this, {Id(t.Id)}, {t.X}, {t.Y}, {t.Z}, {t.Hue}, -1, \"\", 1);");
                }
            }

            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public {cls}Addon(Serial serial) : base(serial)");
            sb.AppendLine("        {");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        private static void AddComplexComponent(BaseAddon addon, int item, int xoffset, int yoffset, int zoffset, int hue, int lightsource, string name, int amount)");
            sb.AppendLine("        {");
            sb.AppendLine("            AddonComponent ac = new AddonComponent(item);");
            sb.AppendLine("            if (!string.IsNullOrEmpty(name))");
            sb.AppendLine("                ac.Name = name;");
            sb.AppendLine("            if (hue != 0)");
            sb.AppendLine("                ac.Hue = hue;");
            sb.AppendLine("            if (amount > 1)");
            sb.AppendLine("            {");
            sb.AppendLine("                ac.Stackable = true;");
            sb.AppendLine("                ac.Amount = amount;");
            sb.AppendLine("            }");
            sb.AppendLine("            if (lightsource != -1)");
            sb.AppendLine("                ac.Light = (LightType)lightsource;");
            sb.AppendLine("            addon.AddComponent(ac, xoffset, yoffset, zoffset);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void Serialize(GenericWriter writer)");
            sb.AppendLine("        {");
            sb.AppendLine("            base.Serialize(writer);");
            sb.AppendLine("            writer.Write(0); // version");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void Deserialize(GenericReader reader)");
            sb.AppendLine("        {");
            sb.AppendLine("            base.Deserialize(reader);");
            sb.AppendLine("            reader.ReadInt();");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public class {cls}AddonDeed : BaseAddonDeed");
            sb.AppendLine("    {");
            sb.AppendLine($"        public override BaseAddon Addon => new {cls}Addon();");
            sb.AppendLine();
            sb.AppendLine("        [Constructable]");
            sb.AppendLine($"        public {cls}AddonDeed()");
            sb.AppendLine("        {");
            sb.AppendLine($"            Name = \"{deedName.Replace("\"", "'")}\";");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public {cls}AddonDeed(Serial serial) : base(serial)");
            sb.AppendLine("        {");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void Serialize(GenericWriter writer)");
            sb.AppendLine("        {");
            sb.AppendLine("            base.Serialize(writer);");
            sb.AppendLine("            writer.Write(0); // version");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void Deserialize(GenericReader reader)");
            sb.AppendLine("        {");
            sb.AppendLine("            base.Deserialize(reader);");
            sb.AppendLine("            reader.ReadInt();");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// Freeze file with world coordinates (UO Fiddler map: "Insert statics from file" / WSC)
        /// </summary>
        public static void SaveFreezeFile(string fileName, IEnumerable<ImportTile> tiles, int worldX, int worldY, int worldZ)
        {
            var sb = new StringBuilder();
            int i = 0;
            foreach (ImportTile t in tiles)
            {
                if (t.Id == 0x1)
                {
                    continue;
                }

                sb.AppendLine($"SECTION WORLDITEM {i++}");
                sb.AppendLine("{");
                sb.AppendLine($"\tID\t{t.Id}");
                sb.AppendLine($"\tX\t{worldX + t.X}");
                sb.AppendLine($"\tY\t{worldY + t.Y}");
                sb.AppendLine($"\tZ\t{worldZ + t.Z}");
                sb.AppendLine($"\tColor\t{t.Hue}");
                sb.AppendLine("}");
            }

            File.WriteAllText(fileName, sb.ToString(), _ansi);
        }

        #endregion
    }
}

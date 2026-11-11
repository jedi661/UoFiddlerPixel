using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextConverter
{
    /// <summary>One part of a multi: item ID and offset (or world position before re-centering).</summary>
    internal struct MultiPart
    {
        public int Id;
        public int X;
        public int Y;
        public int Z;
        public int Flags;
        public int Hue;
    }

    internal enum MultiInput { Auto, Lines, UoFiddlerTxt, Csv, Wsc, Uox3, Xml, ServUOAddon, SphereMultiDef, WorldCoordinates }

    internal enum MultiOutput { UoFiddlerTxt, Uoa, PlainDecimal, Csv, Wsc, Uox3, Xml, ServUOAddon, SphereMultiDef, Decoration, CSharpArray }

    internal enum IdStyle { Auto, Decimal, Hex }

    internal enum OriginMode { Keep, Center, MinCorner, Manual }

    /// <summary>Settings of a multi conversion.</summary>
    internal sealed class MultiOptions
    {
        public MultiInput Input = MultiInput.Auto;
        public IdStyle Ids = IdStyle.Auto;
        public OriginMode Origin = OriginMode.Keep;
        public int OriginX, OriginY, OriginZ;
        public int ShiftX, ShiftY, ShiftZ;
        public int FlagsMode;            // 0 keep, 1 all 0, 2 all 1
        public bool RemoveDuplicates;
        public bool Sort;
        public string Replace = string.Empty;   // "0x1234=0x1235; 99=100"
        public string ClassName = "MyAddon";
        public string Name = "my addon";
        public int BaseX, BaseY, BaseZ;  // position for Decoration output
    }

    /// <summary>Reads and writes the multi text formats (UoFiddler exports, UOA / Sphere, ServUO addons …).</summary>
    internal static class MultiFormats
    {
        public static readonly string[] InputNames =
        {
            "Auto (recognize)", "Lines: ID X Y Z [Flags] (UOA, Sphere, UoFiddler TXT)", "UoFiddler TXT (hex IDs)", "CSV (Punt's multi tool)", "WSC",
            "UOX3", "XML (UoFiddler)", "ServUO / RunUO addon script", "Sphere MULTIDEF (COMPONENT=)", "World coordinates (X: … Y: … ID: … Z: …)"
        };

        public static readonly string[] OutputNames =
        {
            "UoFiddler TXT – for multi import (0xID X Y Z F)", "UOA (with header, decimal)", "Plain lines, decimal (Sphere / UOA without header)",
            "CSV (Punt's multi tool)", "WSC", "UOX3", "XML (UoFiddler)", "ServUO addon script + deed", "Sphere MULTIDEF", "ServUO decoration .cfg (at base X / Y / Z)",
            "C# array"
        };

        // ------------------------------------------------------------------ numbers

        /// <summary>Hex with 0x, "h" at the end, Sphere hex (leading 0 when allowed) or decimal.</summary>
        public static bool TryNumber(string s, IdStyle style, bool sphereHex, out int value)
        {
            value = 0;
            s = (s ?? string.Empty).Trim().TrimEnd(',', ';');
            if (s.Length == 0)
            {
                return false;
            }
            bool neg = s.StartsWith("-", StringComparison.Ordinal);
            if (neg)
            {
                s = s.Substring(1);
            }
            bool ok;
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                ok = int.TryParse(s.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value);
            }
            else if (style == IdStyle.Hex || (style == IdStyle.Auto && Regex.IsMatch(s, "[a-fA-F]")) || (sphereHex && s.Length > 1 && s[0] == '0'))
            {
                ok = int.TryParse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value);
            }
            else
            {
                ok = int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out value);
            }
            if (ok && neg)
            {
                value = -value;
            }
            return ok;
        }

        private static int Coord(string s) => int.TryParse(s.Trim().TrimEnd(',', ';'), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int v) ? v : throw new FormatException($"\"{s}\" is not a number");

        // ------------------------------------------------------------------ detect

        public static MultiInput Detect(string text)
        {
            string t = text ?? string.Empty;
            if (Regex.IsMatch(t, @"<Item\b[^>]*\bID\s*=", RegexOptions.IgnoreCase))
            {
                return MultiInput.Xml;
            }
            if (t.IndexOf("SECTION WORLDITEM", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return MultiInput.Wsc;
            }
            if (t.IndexOf("[HOUSE ITEM", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return MultiInput.Uox3;
            }
            if (Regex.IsMatch(t, @"AddComponent\s*\(", RegexOptions.IgnoreCase))
            {
                return MultiInput.ServUOAddon;
            }
            if (Regex.IsMatch(t, @"^\s*COMPONENT\s*=", RegexOptions.IgnoreCase | RegexOptions.Multiline))
            {
                return MultiInput.SphereMultiDef;
            }
            if (Regex.IsMatch(t, @"\bX\s*[:=]\s*-?\d+.*\bY\s*[:=]\s*-?\d+", RegexOptions.IgnoreCase))
            {
                return MultiInput.WorldCoordinates;
            }
            if (Regex.IsMatch(t, @"TileID\s*,|^\s*(0x)?[0-9A-Fa-f]+\s*,\s*-?\d+\s*,\s*-?\d+\s*,", RegexOptions.IgnoreCase | RegexOptions.Multiline))
            {
                return MultiInput.Csv;
            }
            return MultiInput.Lines;
        }

        // ------------------------------------------------------------------ read

        /// <summary>Reads the parts. Lines that cannot be read are counted in <paramref name="skipped"/> with their line number.</summary>
        public static List<MultiPart> Parse(string text, MultiOptions o, out MultiInput used, List<string> skipped)
        {
            used = o.Input == MultiInput.Auto ? Detect(text) : o.Input;
            var parts = new List<MultiPart>();
            string[] lines = (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            switch (used)
            {
                case MultiInput.Xml:
                    foreach (Match m in Regex.Matches(text, @"<Item\b([^>]*)>", RegexOptions.IgnoreCase))
                    {
                        string a = m.Groups[1].Value;
                        string Attr(string n) => Regex.Match(a, n + @"\s*=\s*""([^""]*)""", RegexOptions.IgnoreCase).Groups[1].Value;
                        if (TryNumber(Attr("ID"), IdStyle.Auto, false, out int id))
                        {
                            TryNumber(Attr("Flags"), IdStyle.Decimal, false, out int flags);
                            parts.Add(new MultiPart { Id = id, X = Coord(Attr("X")), Y = Coord(Attr("Y")), Z = Coord(Attr("Z")), Flags = flags });
                        }
                    }
                    break;
                case MultiInput.Wsc:
                case MultiInput.Uox3:
                    ParseBlocks(lines, used, parts, skipped);
                    break;
                case MultiInput.ServUOAddon:
                    foreach (Match m in Regex.Matches(text, @"AddComponent\s*\(\s*new\s+\w*Component\s*\(\s*([^,)]+)[^)]*\)\s*(?:\{[^}]*\})?\s*,\s*(-?\d+)\s*,\s*(-?\d+)\s*,\s*(-?\d+)\s*\)", RegexOptions.IgnoreCase))
                    {
                        if (TryNumber(m.Groups[1].Value, IdStyle.Auto, false, out int id))
                        {
                            parts.Add(new MultiPart { Id = id, X = Coord(m.Groups[2].Value), Y = Coord(m.Groups[3].Value), Z = Coord(m.Groups[4].Value), Flags = 1 });
                        }
                        else
                        {
                            skipped?.Add($"AddComponent with \"{m.Groups[1].Value}\" – not a number");
                        }
                    }
                    break;
                case MultiInput.SphereMultiDef:
                    for (int i = 0; i < lines.Length; i++)
                    {
                        Match m = Regex.Match(lines[i], @"^\s*COMPONENT\s*=\s*([^,\s]+)\s*,\s*(-?\d+)\s*,\s*(-?\d+)\s*,\s*(-?\d+)", RegexOptions.IgnoreCase);
                        if (!m.Success)
                        {
                            continue;
                        }
                        if (TryNumber(m.Groups[1].Value, IdStyle.Auto, true, out int id))
                        {
                            parts.Add(new MultiPart { Id = id, X = Coord(m.Groups[2].Value), Y = Coord(m.Groups[3].Value), Z = Coord(m.Groups[4].Value), Flags = 1 });
                        }
                        else
                        {
                            skipped?.Add($"line {i + 1}: \"{m.Groups[1].Value}\" is a defname – use the item number");
                        }
                    }
                    break;
                case MultiInput.WorldCoordinates:
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string l = lines[i];
                        if (l.Trim().Length == 0)
                        {
                            continue;
                        }
                        string Val(string key) => Regex.Match(l, @"\b" + key + @"\s*[:=]\s*((?:0x)?-?[0-9A-Fa-f]+)", RegexOptions.IgnoreCase).Groups[1].Value;
                        string idText = Val("ID");
                        if (idText.Length == 0)
                        {
                            idText = Val("ItemID");
                        }
                        if (idText.Length == 0)
                        {
                            idText = Val("Graphic");
                        }
                        if (!TryNumber(idText, o.Ids, false, out int id) || Val("X").Length == 0 || Val("Y").Length == 0)
                        {
                            skipped?.Add($"line {i + 1}: {Short(l)}");
                            continue;
                        }
                        string z = Val("Z");
                        TryNumber(Val("Hue"), IdStyle.Auto, false, out int hue);
                        parts.Add(new MultiPart { Id = id, X = Coord(Val("X")), Y = Coord(Val("Y")), Z = z.Length > 0 ? Coord(z) : 0, Flags = 1, Hue = hue });
                    }
                    break;
                case MultiInput.Csv:
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string[] p = lines[i].Split(',');
                        if (p.Length < 4 || p[0].Trim().Equals("TileID", StringComparison.OrdinalIgnoreCase) || lines[i].Trim().Length == 0)
                        {
                            continue;
                        }
                        try
                        {
                            if (!TryNumber(p[0], IdStyle.Auto, false, out int id))
                            {
                                throw new FormatException();
                            }
                            int flags = p.Length > 4 && TryNumber(p[4], IdStyle.Auto, false, out int f) ? f : 0;
                            parts.Add(new MultiPart { Id = id, X = Coord(p[1]), Y = Coord(p[2]), Z = Coord(p[3]), Flags = flags });
                        }
                        catch (FormatException)
                        {
                            skipped?.Add($"line {i + 1}: {Short(lines[i])}");
                        }
                    }
                    break;
                default:
                {
                    // ID X Y Z [Flags]; header lines (… version, … template id, … num components) and text lines are skipped
                    IdStyle ids = used == MultiInput.UoFiddlerTxt ? IdStyle.Hex : o.Ids;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string l = lines[i].Trim();
                        if (l.Length == 0 || l.StartsWith("//", StringComparison.Ordinal) || l.StartsWith("#", StringComparison.Ordinal) || l.StartsWith(";", StringComparison.Ordinal))
                        {
                            continue;
                        }
                        if (Regex.IsMatch(l, @"^-?\d+\s+(version|template id|item version|num components)\b", RegexOptions.IgnoreCase))
                        {
                            continue;
                        }
                        string[] p = Regex.Split(l, @"[\s,;]+").Where(x => x.Length > 0).ToArray();
                        if (p.Length < 4 || !TryNumber(p[0], ids, false, out int id))
                        {
                            skipped?.Add($"line {i + 1}: {Short(l)}");
                            continue;
                        }
                        try
                        {
                            int flags = p.Length > 4 && TryNumber(p[4], IdStyle.Decimal, false, out int f) ? f : 0;
                            parts.Add(new MultiPart { Id = id, X = Coord(p[1]), Y = Coord(p[2]), Z = Coord(p[3]), Flags = flags });
                        }
                        catch (FormatException)
                        {
                            skipped?.Add($"line {i + 1}: {Short(l)}");
                        }
                    }
                    break;
                }
            }
            return parts;
        }

        private static void ParseBlocks(string[] lines, MultiInput type, List<MultiPart> parts, List<string> skipped)
        {
            MultiPart? current = null;
            void Flush()
            {
                if (current.HasValue)
                {
                    parts.Add(current.Value);
                }
                current = null;
            }
            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i].Trim();
                if (l.StartsWith("SECTION WORLDITEM", StringComparison.OrdinalIgnoreCase) || l.StartsWith("[HOUSE ITEM", StringComparison.OrdinalIgnoreCase))
                {
                    Flush();
                    current = new MultiPart { Flags = 1 };
                    continue;
                }
                if (!current.HasValue)
                {
                    continue;
                }
                Match m = Regex.Match(l, @"^(\w+)\s*(?:=|\s)\s*(\S+)");
                if (!m.Success)
                {
                    continue;
                }
                MultiPart p = current.Value;
                string key = m.Groups[1].Value.ToUpperInvariant();
                string v = m.Groups[2].Value;
                try
                {
                    switch (key)
                    {
                        case "ID":
                        case "ITEM":
                            if (TryNumber(v, type == MultiInput.Wsc ? IdStyle.Decimal : IdStyle.Auto, false, out int id))
                            {
                                p.Id = id;
                            }
                            break;
                        case "X": p.X = Coord(v); break;
                        case "Y": p.Y = Coord(v); break;
                        case "Z": p.Z = Coord(v); break;
                        case "COLOR":
                        case "HUE":
                            TryNumber(v, IdStyle.Auto, false, out p.Hue);
                            break;
                    }
                }
                catch (FormatException)
                {
                    skipped?.Add($"line {i + 1}: {Short(l)}");
                }
                current = p;
            }
            Flush();
        }

        private static string Short(string s) => s.Length > 60 ? s.Substring(0, 60) + "…" : s;

        // ------------------------------------------------------------------ tools

        /// <summary>Re-center, shift, flags, replace IDs, remove duplicates, sort.</summary>
        public static List<MultiPart> Transform(List<MultiPart> parts, MultiOptions o, List<string> notes)
        {
            var list = parts.ToList();
            if (list.Count == 0)
            {
                return list;
            }
            int ox = 0, oy = 0, oz = 0;
            switch (o.Origin)
            {
                case OriginMode.Center:
                    ox = (list.Min(p => p.X) + list.Max(p => p.X)) / 2;
                    oy = (list.Min(p => p.Y) + list.Max(p => p.Y)) / 2;
                    oz = list.Min(p => p.Z);
                    break;
                case OriginMode.MinCorner:
                    ox = list.Min(p => p.X);
                    oy = list.Min(p => p.Y);
                    oz = list.Min(p => p.Z);
                    break;
                case OriginMode.Manual:
                    ox = o.OriginX;
                    oy = o.OriginY;
                    oz = o.OriginZ;
                    break;
            }
            if (o.Origin != OriginMode.Keep)
            {
                notes?.Add($"origin {ox}, {oy}, {oz} subtracted");
            }
            Dictionary<int, int> replace = ParseReplace(o.Replace, notes);
            for (int i = 0; i < list.Count; i++)
            {
                MultiPart p = list[i];
                p.X = p.X - ox + o.ShiftX;
                p.Y = p.Y - oy + o.ShiftY;
                p.Z = p.Z - oz + o.ShiftZ;
                if (o.FlagsMode == 1)
                {
                    p.Flags = 0;
                }
                else if (o.FlagsMode == 2)
                {
                    p.Flags = 1;
                }
                if (replace.TryGetValue(p.Id, out int to))
                {
                    p.Id = to;
                }
                list[i] = p;
            }
            if (o.RemoveDuplicates)
            {
                int before = list.Count;
                list = list.GroupBy(p => (p.Id, p.X, p.Y, p.Z)).Select(g => g.First()).ToList();
                if (before != list.Count)
                {
                    notes?.Add($"{before - list.Count} duplicates removed");
                }
            }
            if (o.Sort)
            {
                // draw order of the client: back to front, bottom to top
                list = list.OrderBy(p => p.X + p.Y).ThenBy(p => p.Z).ThenBy(p => p.Y).ThenBy(p => p.X).ToList();
            }
            int bad = list.Count(p => p.X < short.MinValue || p.X > short.MaxValue || p.Y < short.MinValue || p.Y > short.MaxValue || p.Z < -128 || p.Z > 127);
            if (bad > 0)
            {
                notes?.Add($"⚠ {bad} parts are outside the multi range (X / Y ±32767, Z −128 … 127) – re-center?");
            }
            int badId = list.Count(p => p.Id < 0 || p.Id > 0xFFFF);
            if (badId > 0)
            {
                notes?.Add($"⚠ {badId} IDs are above 0xFFFF");
            }
            return list;
        }

        private static Dictionary<int, int> ParseReplace(string text, List<string> notes)
        {
            var d = new Dictionary<int, int>();
            foreach (string pair in (text ?? string.Empty).Split(';', '\n'))
            {
                string[] ab = pair.Split('=', '>');
                if (ab.Length < 2)
                {
                    continue;
                }
                if (TryNumber(ab[0], IdStyle.Auto, false, out int a) && TryNumber(ab[ab.Length - 1], IdStyle.Auto, false, out int b))
                {
                    d[a] = b;
                }
                else if (pair.Trim().Length > 0)
                {
                    notes?.Add($"⚠ replace \"{pair.Trim()}\" not understood");
                }
            }
            return d;
        }

        // ------------------------------------------------------------------ write

        public static string Write(List<MultiPart> parts, MultiOutput format, MultiOptions o)
        {
            var sb = new StringBuilder();
            string nl = "\r\n";
            switch (format)
            {
                case MultiOutput.UoFiddlerTxt:
                    // exactly what UoFiddler's TXT import reads: hex ID, single spaces, no empty lines
                    sb.Append(string.Join(nl, parts.Select(p => $"0x{p.Id:X4} {p.X} {p.Y} {p.Z} {p.Flags}")));
                    break;
                case MultiOutput.Uoa:
                    sb.Append("6 version").Append(nl).Append("1 template id").Append(nl).Append("-1 item version").Append(nl).Append($"{parts.Count} num components").Append(nl);
                    sb.Append(string.Join(nl, parts.Select(p => $"{p.Id} {p.X} {p.Y} {p.Z} {p.Flags}")));
                    break;
                case MultiOutput.PlainDecimal:
                    sb.Append(string.Join(nl, parts.Select(p => $"{p.Id} {p.X} {p.Y} {p.Z} {p.Flags}")));
                    break;
                case MultiOutput.Csv:
                    sb.Append("TileID,OffsetX,OffsetY,OffsetZ,Flag,Cliloc").Append(nl);
                    sb.Append(string.Join(nl, parts.Select(p => $"0x{p.Id:x4},{p.X},{p.Y},{p.Z},0x{p.Flags:x},")));
                    break;
                case MultiOutput.Wsc:
                    for (int i = 0; i < parts.Count; i++)
                    {
                        MultiPart p = parts[i];
                        sb.Append($"SECTION WORLDITEM {i}{nl}{{{nl}\tID\t{p.Id}{nl}\tX\t{p.X}{nl}\tY\t{p.Y}{nl}\tZ\t{p.Z}{nl}\tColor\t{p.Hue}{nl}}}{nl}");
                    }
                    break;
                case MultiOutput.Uox3:
                    for (int i = 0; i < parts.Count; i++)
                    {
                        MultiPart p = parts[i];
                        sb.Append($"[HOUSE ITEM {i}]{nl}{{{nl}ITEM=0x{p.Id:X4}{nl}X={p.X}{nl}Y={p.Y}{nl}Z={p.Z}{nl}}}{nl}{nl}");
                    }
                    break;
                case MultiOutput.Xml:
                    sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>").Append(nl).Append("<Entry ID=\"0\">").Append(nl);
                    foreach (MultiPart p in parts)
                    {
                        sb.Append($"  <Item X=\"{p.X}\" Y=\"{p.Y}\" Z=\"{p.Z}\" ID=\"0x{p.Id:X4}\" />").Append(nl);
                    }
                    sb.Append("</Entry>");
                    break;
                case MultiOutput.ServUOAddon:
                    sb.Append(AddonScript(parts, o));
                    break;
                case MultiOutput.SphereMultiDef:
                    sb.Append($"[MULTIDEF m_{Regex.Replace(o.ClassName ?? "multi", @"[^A-Za-z0-9_]", "_").ToLowerInvariant()}]").Append(nl);
                    sb.Append($"NAME={o.Name}").Append(nl).Append("TYPE=t_multi").Append(nl);
                    foreach (MultiPart p in parts)
                    {
                        sb.Append($"COMPONENT=0{p.Id:x},{p.X},{p.Y},{p.Z}").Append(nl);
                    }
                    if (parts.Count > 0)
                    {
                        sb.Append($"MULTIREGION={parts.Min(p => p.X)},{parts.Min(p => p.Y)},{parts.Max(p => p.X) + 1},{parts.Max(p => p.Y) + 1}").Append(nl);
                    }
                    break;
                case MultiOutput.Decoration:
                    // ServUO Data/Decoration format: a type line, then one position per line, empty line between groups
                    foreach (IGrouping<(int Id, int Hue), MultiPart> g in parts.GroupBy(p => (p.Id, p.Hue)).OrderBy(g => g.Key.Id))
                    {
                        sb.Append($"Static 0x{g.Key.Id:X4}" + (g.Key.Hue != 0 ? $" (Hue=0x{g.Key.Hue:X})" : string.Empty)).Append(nl);
                        foreach (MultiPart p in g)
                        {
                            sb.Append($"{p.X + o.BaseX} {p.Y + o.BaseY} {p.Z + o.BaseZ}").Append(nl);
                        }
                        sb.Append(nl);
                    }
                    break;
                case MultiOutput.CSharpArray:
                    sb.Append("// ItemID, X, Y, Z").Append(nl).Append("private static readonly int[,] m_Parts = new int[,]").Append(nl).Append("{").Append(nl);
                    sb.Append(string.Join("," + nl, parts.Select(p => $"    {{ 0x{p.Id:X4}, {p.X}, {p.Y}, {p.Z} }}"))).Append(nl).Append("};");
                    break;
            }
            return sb.ToString();
        }

        private static string AddonScript(List<MultiPart> parts, MultiOptions o)
        {
            string cls = Regex.Replace(o.ClassName ?? "MyAddon", @"[^A-Za-z0-9_]", "_");
            if (cls.Length == 0 || char.IsDigit(cls[0]))
            {
                cls = "Addon" + cls;
            }
            string name = (o.Name ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using Server;");
            sb.AppendLine();
            sb.AppendLine("namespace Server.Items");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {cls} : BaseAddon");
            sb.AppendLine("    {");
            sb.AppendLine($"        public override BaseAddonDeed Deed {{ get {{ return new {cls}Deed(); }} }}");
            sb.AppendLine();
            sb.AppendLine("        [Constructable]");
            sb.AppendLine($"        public {cls}()");
            sb.AppendLine("        {");
            foreach (MultiPart p in parts)
            {
                sb.AppendLine(p.Hue != 0
                    ? $"            AddComponent(new AddonComponent(0x{p.Id:X4}) {{ Hue = 0x{p.Hue:X} }}, {p.X}, {p.Y}, {p.Z});"
                    : $"            AddComponent(new AddonComponent(0x{p.Id:X4}), {p.X}, {p.Y}, {p.Z});");
            }
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public {cls}(Serial serial) : base(serial) {{ }}");
            sb.AppendLine();
            sb.AppendLine("        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }");
            sb.AppendLine("        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public class {cls}Deed : BaseAddonDeed");
            sb.AppendLine("    {");
            sb.AppendLine($"        public override BaseAddon Addon {{ get {{ return new {cls}(); }} }}");
            sb.AppendLine();
            sb.AppendLine("        [Constructable]");
            sb.AppendLine($"        public {cls}Deed()");
            sb.AppendLine("        {");
            sb.AppendLine($"            Name = \"{name} deed\";");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public {cls}Deed(Serial serial) : base(serial) {{ }}");
            sb.AppendLine();
            sb.AppendLine("        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }");
            sb.AppendLine("        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }");
            sb.AppendLine("    }");
            sb.Append('}');
            return sb.ToString();
        }

        /// <summary>Size and ID statistics.</summary>
        public static string Describe(List<MultiPart> parts)
        {
            if (parts.Count == 0)
            {
                return "no parts";
            }
            return $"{parts.Count} parts · {parts.Select(p => p.Id).Distinct().Count()} different IDs · X {parts.Min(p => p.X)} … {parts.Max(p => p.X)} · " +
                   $"Y {parts.Min(p => p.Y)} … {parts.Max(p => p.Y)} · Z {parts.Min(p => p.Z)} … {parts.Max(p => p.Z)}";
        }
    }
}

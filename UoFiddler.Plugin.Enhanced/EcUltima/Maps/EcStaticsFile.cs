/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Maps
{
    /// <summary>One static of an export / import file.</summary>
    public sealed class EcStaticsFileEntry
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public uint ItemId { get; set; }
        public uint Hue { get; set; }
    }

    public enum EcStaticsFormat
    {
        /// <summary>"0xITEM x y z hue", x / y relative to the origin of the area (can be placed anywhere).</summary>
        Text,

        /// <summary>"X,Y,Z,ItemId,Hue" with map coordinates (placed at the same position again).</summary>
        Csv,

        /// <summary>"0xITEM x y z 0" relative to the center of the area – the format of the Multis tabs (multi parts).</summary>
        MultiText
    }

    /// <summary>
    /// Export / import of statics of a map area. Reads the own formats and the formats of the Multis tab
    /// (text "item x y z flags", UOA, CSV of punt's multi tool, WSC).
    /// </summary>
    public static class EcStaticsFile
    {
        public const string Header = "# UoFiddler Enhanced statics";

        public static string Write(IEnumerable<(int X, int Y, EcStatic Static)> statics, EcStaticsFormat format, System.Drawing.Rectangle area, int facetId)
        {
            var sb = new StringBuilder();
            var list = statics.ToList();
            switch (format)
            {
                case EcStaticsFormat.Csv:
                    sb.AppendLine("X,Y,Z,ItemId,Hue");
                    foreach (var (x, y, st) in list)
                    {
                        sb.AppendLine(FormattableString.Invariant($"{x},{y},{st.Z},0x{st.ItemId:X4},{st.Hue}"));
                    }
                    break;
                case EcStaticsFormat.MultiText:
                    int cx = area.X + (area.Width / 2), cy = area.Y + (area.Height / 2);
                    foreach (var (x, y, st) in list)
                    {
                        sb.AppendLine(FormattableString.Invariant($"0x{st.ItemId:X4} {x - cx} {y - cy} {st.Z} 0"));
                    }
                    break;
                default:
                    sb.AppendLine(Header);
                    sb.AppendLine(FormattableString.Invariant($"# facet {facetId} origin {area.X} {area.Y} size {area.Width} {area.Height}"));
                    sb.AppendLine("# item x y z hue (x, y relative to the origin)");
                    foreach (var (x, y, st) in list)
                    {
                        sb.AppendLine(FormattableString.Invariant($"0x{st.ItemId:X4} {x - area.X} {y - area.Y} {st.Z} {st.Hue}"));
                    }
                    break;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Reads a file. <paramref name="absolute"/> is true for the CSV export (map coordinates);
        /// <paramref name="centered"/> is true for multi formats (coordinates relative to a center, can be negative).
        /// </summary>
        public static List<EcStaticsFileEntry> Read(string path, out bool absolute, out bool centered)
        {
            string[] lines = File.ReadAllLines(path);
            var result = new List<EcStaticsFileEntry>();
            absolute = false;
            centered = false;
            string first = lines.FirstOrDefault(l => l.Trim().Length > 0)?.Trim() ?? string.Empty;

            if (lines.Any(l => l.TrimStart().StartsWith("SECTION WORLDITEM", StringComparison.OrdinalIgnoreCase)))
            {
                // WSC (sections with ID / X / Y / Z / Color)
                centered = true;
                EcStaticsFileEntry entry = null;
                foreach (string raw in lines)
                {
                    string line = raw.Trim();
                    if (line.StartsWith("SECTION", StringComparison.OrdinalIgnoreCase))
                    {
                        entry = new EcStaticsFileEntry();
                        result.Add(entry);
                        continue;
                    }
                    string[] v = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (entry == null || v.Length < 2)
                    {
                        continue;
                    }
                    switch (v[0].ToUpperInvariant())
                    {
                        case "ID": entry.ItemId = (uint)Num(v[1]); break;
                        case "X": entry.X = (int)Num(v[1]); break;
                        case "Y": entry.Y = (int)Num(v[1]); break;
                        case "Z": entry.Z = (int)Num(v[1]); break;
                        case "COLOR": entry.Hue = (uint)Num(v[1]); break;
                    }
                }
                return result;
            }

            bool own = first.StartsWith(Header, StringComparison.OrdinalIgnoreCase);
            bool ownCsv = first.StartsWith("X,Y,Z,ItemId", StringComparison.OrdinalIgnoreCase);
            bool multiCsv = first.StartsWith("TileID", StringComparison.OrdinalIgnoreCase);
            bool uoa = lines.Length > 3 && first.EndsWith("version", StringComparison.OrdinalIgnoreCase) && lines[3].Contains("num components");
            absolute = ownCsv;
            centered = !own && !ownCsv;

            foreach (string raw in lines.Skip(uoa ? 4 : (ownCsv || multiCsv) ? 1 : 0))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal) || line.StartsWith("//", StringComparison.Ordinal))
                {
                    continue;
                }
                string[] v = line.Split(new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (v.Length < 4)
                {
                    continue;
                }
                if (ownCsv)
                {
                    result.Add(new EcStaticsFileEntry
                    {
                        X = (int)Num(v[0]),
                        Y = (int)Num(v[1]),
                        Z = (int)Num(v[2]),
                        ItemId = (uint)Num(v[3]),
                        Hue = v.Length > 4 ? (uint)Num(v[4]) : 0
                    });
                    continue;
                }
                result.Add(new EcStaticsFileEntry
                {
                    ItemId = (uint)Num(v[0]),
                    X = (int)Num(v[1]),
                    Y = (int)Num(v[2]),
                    Z = (int)Num(v[3]),
                    // own text: 5th value = hue; multi formats: 5th value = flags (no hue)
                    Hue = own && v.Length > 4 ? (uint)Num(v[4]) : 0
                });
            }
            return result;
        }

        private static long Num(string text)
        {
            text = text.Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return long.Parse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return long.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
    }
}

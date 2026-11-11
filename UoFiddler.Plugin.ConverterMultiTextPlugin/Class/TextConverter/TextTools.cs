using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextConverter
{
    /// <summary>Text conversions besides multis: number columns, Sphere hex, coordinates, binary, Morse, Base64, random numbers.</summary>
    internal static class TextTools
    {
        private static readonly Regex Separator = new Regex(@"(\s+|,|;)", RegexOptions.Compiled);

        /// <summary>
        /// Converts the numbers in the chosen columns (1-based, "1" or "1,3" or "all"). toHex: decimal → 0xABC, otherwise hex → decimal.
        /// Separators and everything else stay as they are. Lines without numbers are copied.
        /// </summary>
        public static string ConvertColumns(string text, string columns, bool toHex, int digits, out int changed)
        {
            changed = 0;
            bool all = string.IsNullOrWhiteSpace(columns) || columns.Trim().Equals("all", StringComparison.OrdinalIgnoreCase);
            var wanted = new HashSet<int>();
            if (!all)
            {
                foreach (string c in columns.Split(',', ' ', ';'))
                {
                    if (int.TryParse(c.Trim(), out int n) && n > 0)
                    {
                        wanted.Add(n);
                    }
                }
            }
            var sb = new StringBuilder();
            string[] lines = (text ?? string.Empty).Replace("\r\n", "\n").Split('\n');
            for (int li = 0; li < lines.Length; li++)
            {
                string[] tokens = Separator.Split(lines[li]); // keeps the separators (odd indices)
                int column = 0;
                for (int i = 0; i < tokens.Length; i++)
                {
                    string t = tokens[i];
                    if (t.Length == 0 || i % 2 == 1) // Regex.Split with a group puts the separators at odd places
                    {
                        sb.Append(t);
                        continue;
                    }
                    column++;
                    if (all || wanted.Contains(column))
                    {
                        if (toHex && int.TryParse(t, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int d) && d >= 0)
                        {
                            t = "0x" + d.ToString("X" + Math.Max(1, digits), CultureInfo.InvariantCulture);
                            changed++;
                        }
                        else if (!toHex && t.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
                                 int.TryParse(t.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int h))
                        {
                            t = h.ToString(CultureInfo.InvariantCulture);
                            changed++;
                        }
                    }
                    sb.Append(t);
                }
                if (li < lines.Length - 1)
                {
                    sb.Append("\r\n");
                }
            }
            return sb.ToString();
        }

        /// <summary>Sphere writes hex with a leading 0 (0a3f), C# / ServUO with 0x (0xA3F). Only "words" that are numbers are changed.</summary>
        public static string SphereHex(string text, bool toCSharp, out int changed)
        {
            int n = 0;
            string result = toCSharp
                ? Regex.Replace(text ?? string.Empty, @"(?<![\w.])0([0-9a-fA-F]+)\b", m =>
                {
                    // a plain decimal like 0 or 05 stays decimal-looking but Sphere reads it as hex anyway
                    n++;
                    return "0x" + m.Groups[1].Value.ToUpperInvariant();
                })
                : Regex.Replace(text ?? string.Empty, @"\b0x([0-9a-fA-F]+)\b", m =>
                {
                    n++;
                    return "0" + m.Groups[1].Value.ToLowerInvariant();
                }, RegexOptions.IgnoreCase);
            changed = n;
            return result;
        }

        public static readonly string[] CoordinateOutputs =
        {
            "ServUO: new Point3D(x, y, z)", "ServUO: new Point3D(x, y, z), Map.Felucca", "Sphere: P=x,y,z,map", "x y z", "ServUO [go x y z", "XML spawner: x y z"
        };

        /// <summary>Lines with 2 – 4 numbers (x y [z] [map]) in any form → the chosen format.</summary>
        public static string Coordinates(string text, int format, string mapName, int sphereMap, List<string> skipped)
        {
            var sb = new StringBuilder();
            string[] lines = (text ?? string.Empty).Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                MatchCollection nums = Regex.Matches(lines[i], @"-?\d+");
                if (lines[i].Trim().Length == 0)
                {
                    continue;
                }
                if (nums.Count < 2)
                {
                    skipped?.Add($"line {i + 1}: {lines[i].Trim()}");
                    continue;
                }
                int x = int.Parse(nums[0].Value, CultureInfo.InvariantCulture);
                int y = int.Parse(nums[1].Value, CultureInfo.InvariantCulture);
                int z = nums.Count > 2 ? int.Parse(nums[2].Value, CultureInfo.InvariantCulture) : 0;
                int map = nums.Count > 3 ? int.Parse(nums[3].Value, CultureInfo.InvariantCulture) : sphereMap;
                switch (format)
                {
                    case 0: sb.Append($"new Point3D({x}, {y}, {z}),"); break;
                    case 1: sb.Append($"new Point3D({x}, {y}, {z}), Map.{mapName},"); break;
                    case 2: sb.Append($"P={x},{y},{z},{map}"); break;
                    case 3: sb.Append($"{x} {y} {z}"); break;
                    case 4: sb.Append($"[go {x} {y} {z}"); break;
                    default: sb.Append($"{x} {y} {z}"); break;
                }
                sb.Append("\r\n");
            }
            return sb.ToString().TrimEnd();
        }

        // ------------------------------------------------------------------ text tools (repaired from the old tab)

        public static string ToBinary(string text) => string.Join(" ", Encoding.UTF8.GetBytes(text ?? string.Empty).Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

        public static string FromBinary(string bits)
        {
            var bytes = new List<byte>();
            foreach (string b in Regex.Split((bits ?? string.Empty).Trim(), @"\s+").Where(b => b.Length > 0))
            {
                if (!Regex.IsMatch(b, "^[01]{1,8}$"))
                {
                    throw new FormatException($"\"{b}\" is not a binary byte (only 0 and 1, max. 8 digits)");
                }
                bytes.Add(Convert.ToByte(b, 2));
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        private static readonly Dictionary<char, string> Morse = new Dictionary<char, string>
        {
            {'A', ".-"}, {'B', "-..."}, {'C', "-.-."}, {'D', "-.."}, {'E', "."}, {'F', "..-."}, {'G', "--."}, {'H', "...."}, {'I', ".."},
            {'J', ".---"}, {'K', "-.-"}, {'L', ".-.."}, {'M', "--"}, {'N', "-."}, {'O', "---"}, {'P', ".--."}, {'Q', "--.-"}, {'R', ".-."},
            {'S', "..."}, {'T', "-"}, {'U', "..-"}, {'V', "...-"}, {'W', ".--"}, {'X', "-..-"}, {'Y', "-.--"}, {'Z', "--.."},
            {'0', "-----"}, {'1', ".----"}, {'2', "..---"}, {'3', "...--"}, {'4', "....-"}, {'5', "....."}, {'6', "-...."}, {'7', "--..."},
            {'8', "---.."}, {'9', "----."}, {'Ä', ".-.-"}, {'Ö', "---."}, {'Ü', "..--"}, {'.', ".-.-.-"}, {',', "--..--"}, {'?', "..--.."},
            {'!', "-.-.--"}, {'-', "-....-"}, {'/', "-..-."}, {'(', "-.--."}, {')', "-.--.-"}, {':', "---..."}, {'=', "-...-"}, {'+', ".-.-."}, {'@', ".--.-."}
        };

        public static string ToMorse(string text, out int unknown)
        {
            int u = 0;
            string result = string.Join(" / ", (text ?? string.Empty).ToUpperInvariant().Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => string.Join(" ", word.Select(c =>
                {
                    if (Morse.TryGetValue(c, out string m))
                    {
                        return m;
                    }
                    u++;
                    return "?";
                }))));
            unknown = u;
            return result;
        }

        public static string FromMorse(string code, out int unknown)
        {
            var reverse = Morse.ToDictionary(kv => kv.Value, kv => kv.Key);
            int u = 0;
            string result = string.Join(" ", Regex.Split((code ?? string.Empty).Trim(), @"\s*/\s*|\s{3,}")
                .Select(word => new string(Regex.Split(word.Trim(), @"\s+").Where(l => l.Length > 0).Select(l =>
                {
                    if (reverse.TryGetValue(l, out char c))
                    {
                        return c;
                    }
                    u++;
                    return '?';
                }).ToArray())));
            unknown = u;
            return result;
        }

        public static string ToBase64(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text ?? string.Empty), Base64FormattingOptions.InsertLineBreaks);

        public static string FromBase64(string text) => Encoding.UTF8.GetString(Convert.FromBase64String(Regex.Replace(text ?? string.Empty, @"\s", string.Empty)));

        public static string ToHexBytes(string text) => BitConverter.ToString(Encoding.UTF8.GetBytes(text ?? string.Empty)).Replace("-", " ");

        public static string FromHexBytes(string text)
        {
            string[] parts = Regex.Split((text ?? string.Empty).Trim(), @"[\s,\-]+").Where(p => p.Length > 0).ToArray();
            var bytes = new byte[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                string p = parts[i].StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? parts[i].Substring(2) : parts[i];
                if (!byte.TryParse(p, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out bytes[i]))
                {
                    throw new FormatException($"\"{parts[i]}\" is not a hex byte");
                }
            }
            return Encoding.UTF8.GetString(bytes);
        }

        public static string RandomNumbers(int count, int min, int max, bool unique, Random random)
        {
            if (min > max)
            {
                (min, max) = (max, min);
            }
            long range = (long)max - min + 1;
            if (unique && count > range)
            {
                throw new ArgumentException($"Only {range} different numbers exist between {min} and {max}.");
            }
            var result = new List<int>();
            var used = new HashSet<int>();
            while (result.Count < count)
            {
                int v = (int)(min + (long)(random.NextDouble() * range));
                if (unique && !used.Add(v))
                {
                    continue;
                }
                result.Add(v);
            }
            return string.Join("\r\n", result);
        }
    }
}

// /***************************************************************************
//  *
//  * Dragon Map Studio – reads and writes the original Dragon script folder
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>
    /// Reads / writes the Dragon 1.03.62 (IMod) script format:
    /// groups.txt, maptrans.txt, betweentrans(2).txt, statbetweentrans(2,3).txt, items.txt, unevenly.txt,
    /// the sub folders map, map2, statics, statics2, statics3, items and a fix table.
    /// Commented out rules ("//00 01 file.txt") are kept as disabled rules.
    /// </summary>
    public static class DragonScriptIO
    {
        private static readonly Encoding FileEncoding = Encoding.Latin1;
        private static readonly char[] Separators = { ' ', '\t' };
        private static readonly Regex GroupNameRegex = new Regex(@"^/+\s*([0-9a-fA-F]{2}|[0-9a-fA-F]{4})\s*=\s*(.+?)\s*$", RegexOptions.Compiled);

        // ================================================================== LOAD

        /// <summary>
        /// Loads a complete script folder. <paramref name="paletteFile"/> may be a BMP (8 bit), ACT, PAL, ACO or GPL file
        /// – without it the colours stay black (Dragon itself only uses the palette index).
        /// </summary>
        public static DragonRuleSet LoadFolder(string scriptFolder, string paletteFile, string mapTransName = "maptrans.txt")
        {
            var set = new DragonRuleSet { Name = Path.GetFileName(Path.GetDirectoryName(scriptFolder.TrimEnd('\\', '/'))) ?? "Dragon" };
            if (string.IsNullOrWhiteSpace(set.Name))
            {
                set.Name = "Dragon";
            }

            if (!Directory.Exists(scriptFolder))
            {
                set.Warnings.Add($"Skript-Ordner nicht gefunden: {scriptFolder}");
                return set;
            }

            if (!string.IsNullOrWhiteSpace(paletteFile) && File.Exists(paletteFile))
            {
                try
                {
                    Color[] colors = PaletteIO.Load(paletteFile);
                    for (int i = 0; i < 256 && i < colors.Length; i++)
                    {
                        set.Palette[i].SetColor(colors[i]);
                    }
                }
                catch (Exception ex)
                {
                    set.Warnings.Add($"Palette {paletteFile}: {ex.Message}");
                }
            }
            else
            {
                set.Warnings.Add("Keine Palette / Farbtabelle angegeben – Farben sind schwarz.");
            }

            ReadGroups(set, Find(scriptFolder, "groups.txt"));
            ReadMapTrans(set, Find(scriptFolder, string.IsNullOrWhiteSpace(mapTransName) ? "maptrans.txt" : mapTransName));

            foreach (TransitionLayer layer in new[] { TransitionLayer.Map, TransitionLayer.Static })
            {
                for (int scriptSet = 1; scriptSet <= 3; scriptSet++)
                {
                    string list = Find(scriptFolder, TransitionRule.GetListFileName(layer, scriptSet));
                    if (list != null)
                    {
                        ReadTransitionList(set, scriptFolder, list, layer, scriptSet);
                    }
                }
            }

            // transition files that no list uses (e.g. map/cave-enti2mountain.txt) are loaded too – nothing gets lost
            foreach (string folder in new[] { "map", "map2", "map3", "statics", "statics2", "statics3" })
            {
                string dir = Path.Combine(scriptFolder, folder);
                if (!Directory.Exists(dir))
                {
                    continue;
                }
                foreach (string f in Directory.GetFiles(dir, "*.txt").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                {
                    string rel = DragonRuleSet.NormalizePath(folder + "/" + Path.GetFileName(f));
                    if (set.FindTransitionFile(rel) == null)
                    {
                        set.TransitionFiles.Add(ReadTransitionFile(set, f, rel));
                    }
                }
            }

            ReadItems(set, scriptFolder, Find(scriptFolder, "items.txt"));
            ReadUnevenly(set, Find(scriptFolder, "unevenly.txt"));

            // fix tables: in the script folder or ..\Addon\FixTable
            foreach (string fix in FindFixFiles(scriptFolder))
            {
                ReadFixTable(set, fix);
            }

            NameUnnamedEntries(set);
            set.Normalize();
            return set;
        }

        private static IEnumerable<string> FindFixFiles(string scriptFolder)
        {
            var result = new List<string>();
            string own = Path.Combine(scriptFolder, "fixtable.fix");
            if (File.Exists(own))
            {
                result.Add(own);
            }
            return result;
        }

        /// <summary>Case insensitive file lookup (Dragon scripts mix "Water2x.txt" and "water2x.txt").</summary>
        public static string Find(string folder, string relative)
        {
            if (string.IsNullOrWhiteSpace(relative))
            {
                return null;
            }
            string direct = Path.Combine(folder, relative.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));
            if (File.Exists(direct))
            {
                return direct;
            }
            string dir = Path.GetDirectoryName(direct);
            if (dir == null || !Directory.Exists(dir))
            {
                // try case insensitive sub folder
                string[] parts = DragonRuleSet.NormalizePath(relative).Split('/');
                string current = folder;
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    string match = Directory.Exists(current)
                        ? Directory.GetDirectories(current).FirstOrDefault(d => string.Equals(Path.GetFileName(d), parts[i], StringComparison.OrdinalIgnoreCase))
                        : null;
                    if (match == null)
                    {
                        return null;
                    }
                    current = match;
                }
                dir = current;
            }
            string name = Path.GetFileName(direct);
            return Directory.GetFiles(dir).FirstOrDefault(f => string.Equals(Path.GetFileName(f), name, StringComparison.OrdinalIgnoreCase));
        }

        private static List<string> ReadLines(string path) => File.ReadAllLines(path, FileEncoding).ToList();

        private static bool IsComment(string line) => line.StartsWith("//", StringComparison.Ordinal);

        private static string CommentText(string line) => line.TrimStart('/').Trim();

        /// <summary>True for comment lines that are only decoration ("// -----", "// *****").</summary>
        private static bool IsDecoration(string text) => !text.Any(char.IsLetterOrDigit);

        /// <summary>Removes "// …" after data.</summary>
        private static string StripInlineComment(string line)
        {
            int idx = line.IndexOf("//", StringComparison.Ordinal);
            return idx > 0 ? line.Substring(0, idx).Trim() : line;
        }

        private static string[] Tokens(string line) => line.Split(Separators, StringSplitOptions.RemoveEmptyEntries);

        private static string HeaderOf(List<string> lines, Func<string, bool> isData)
        {
            var sb = new StringBuilder();
            foreach (string raw in lines)
            {
                string line = raw.Trim();
                if (line.Length == 0)
                {
                    sb.AppendLine();
                    continue;
                }
                if (!IsComment(line) || isData(line))
                {
                    break;
                }
                sb.AppendLine(line);
            }
            return sb.ToString().TrimEnd();
        }

        private static void ReadGroups(DragonRuleSet set, string path)
        {
            if (path == null)
            {
                set.Warnings.Add("groups.txt fehlt.");
                return;
            }
            List<string> lines = ReadLines(path);
            set.FileHeaders["groups.txt"] = HeaderOf(lines, l => GroupNameRegex.IsMatch(l));
            var names = new Dictionary<int, string>();
            int lineNo = 0;
            foreach (string raw in lines)
            {
                lineNo++;
                string line = raw.Trim();
                if (line.Length == 0)
                {
                    continue;
                }
                if (IsComment(line))
                {
                    Match m = GroupNameRegex.Match(line);
                    if (m.Success)
                    {
                        names[HexList.ParseHexInt(m.Groups[1].Value)] = m.Groups[2].Value.Trim();
                    }
                    continue;
                }
                string[] t = Tokens(StripInlineComment(line));
                if (t.Length >= 2 && HexList.TryParseHexAddress(t[0], out int index) && HexList.TryParseHexAddress(t[1], out int group))
                {
                    Entry(set, index, t[0]).Group = group;
                    if (!names.ContainsKey(group))
                    {
                        names[group] = $"Gruppe {set.HexGroup(group)}";
                    }
                }
                else
                {
                    set.Warnings.Add($"groups.txt Zeile {lineNo}: nicht lesbar: {line}");
                }
            }
            foreach (KeyValuePair<int, string> kv in names.OrderBy(k => k.Key))
            {
                set.Groups.Add(new DragonGroup { Id = kv.Key, Name = kv.Value });
            }
        }

        /// <summary>Palette entry of an address; a 4-digit address (or one above ff) switches the rule set to 16 bit.</summary>
        private static DragonPaletteEntry Entry(DragonRuleSet set, int index, string token)
        {
            if (index > 0xFF || token.Length > 2)
            {
                set.ColorBits = 16;
            }
            if (index >= set.Palette.Count)
            {
                set.EnsurePaletteSize(index + 1);
            }
            return set.Palette[index];
        }

        private static void ReadMapTrans(DragonRuleSet set, string path)
        {
            if (path == null)
            {
                set.Warnings.Add("maptrans.txt fehlt.");
                return;
            }
            List<string> lines = ReadLines(path);
            set.FileHeaders["maptrans.txt"] = HeaderOf(lines, _ => false);
            string section = string.Empty;
            bool headerDone = false;
            int lineNo = 0;
            foreach (string raw in lines)
            {
                lineNo++;
                string line = raw.Trim();
                if (line.Length == 0)
                {
                    continue;
                }
                if (IsComment(line))
                {
                    if (headerDone && !IsDecoration(CommentText(line)))
                    {
                        section = CommentText(line);
                    }
                    continue;
                }
                headerDone = true;
                string[] t = Tokens(StripInlineComment(line));
                if (t.Length >= 3 && HexList.TryParseHexAddress(t[0], out int index) && HexList.IsInt(t[1]))
                {
                    DragonPaletteEntry e = Entry(set, index, t[0]);
                    e.Defined = true;
                    e.Altitude = HexList.ParseInt(t[1]);
                    e.Tiles = string.Join(" ", t.Skip(2).Select(s => HexList.ParseHex(s).ToString("x4")));
                    e.Comment = section;
                }
                else
                {
                    set.Warnings.Add($"maptrans Zeile {lineNo}: nicht lesbar: {line}");
                }
            }
        }

        private static void ReadTransitionList(DragonRuleSet set, string scriptFolder, string listPath, TransitionLayer layer, int scriptSet)
        {
            string listName = TransitionRule.GetListFileName(layer, scriptSet);
            List<string> lines = ReadLines(listPath);
            set.FileHeaders[listName] = HeaderOf(lines, l => TryParseTransitionLine(CommentText(l), scriptSet, out _, out _, out _));
            string comment = string.Empty;
            bool seenRule = false;
            foreach (string raw in lines)
            {
                string line = raw.Trim();
                if (line.Length == 0)
                {
                    continue;
                }
                bool disabled = IsComment(line);
                string data = disabled ? CommentText(line) : StripInlineComment(line);
                if (!TryParseTransitionLine(data, scriptSet, out int a, out int b, out string file))
                {
                    if (disabled && seenRule && !IsDecoration(CommentText(line)))
                    {
                        comment = CommentText(line);
                    }
                    continue;
                }
                seenRule = true;

                string folder = TransitionRule.GetFolder(layer, scriptSet);
                string relative = DragonRuleSet.NormalizePath(folder + "/" + file);
                var rule = new TransitionRule
                {
                    Layer = layer,
                    ScriptSet = scriptSet,
                    GroupA = a,
                    GroupB = b,
                    File = relative,
                    Enabled = !disabled,
                    Comment = comment
                };
                set.Transitions.Add(rule);

                if (set.FindTransitionFile(relative) == null)
                {
                    string full = Find(scriptFolder, relative);
                    if (full == null)
                    {
                        if (!disabled)
                        {
                            set.Warnings.Add($"{listName}: Datei fehlt: {relative}");
                        }
                        continue;
                    }
                    // use the real spelling of the file name
                    rule.File = DragonRuleSet.NormalizePath(folder + "/" + Path.GetFileName(full));
                    if (set.FindTransitionFile(rule.File) == null)
                    {
                        set.TransitionFiles.Add(ReadTransitionFile(set, full, rule.File));
                    }
                }
                else
                {
                    rule.File = set.FindTransitionFile(relative).Path;
                }
            }
        }

        private static bool TryParseTransitionLine(string data, int scriptSet, out int a, out int b, out string file)
        {
            a = b = -1;
            file = null;
            string[] t = Tokens(data);
            if (scriptSet <= 1)
            {
                if (t.Length >= 3 && HexList.TryParseHexAddress(t[0], out a) && HexList.TryParseHexAddress(t[1], out b) &&
                    t[2].EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    file = t[2];
                    return true;
                }
                return false;
            }
            if (t.Length >= 2 && HexList.TryParseHexAddress(t[0], out int g) && t[1].EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                file = t[1];
                if (scriptSet == 2)
                {
                    a = g;
                    b = -1;
                }
                else
                {
                    a = -1;
                    b = g;
                }
                return true;
            }
            return false;
        }

        public static TransitionFile ReadTransitionFile(DragonRuleSet set, string fullPath, string relative)
        {
            var file = new TransitionFile { Path = relative };
            List<string> lines = ReadLines(fullPath);
            file.HeaderComment = HeaderOf(lines, l => false);
            string section = string.Empty;
            bool headerDone = false;
            int lineNo = 0;
            foreach (string raw in lines)
            {
                lineNo++;
                string line = raw.Trim();
                if (line.Length == 0)
                {
                    continue;
                }
                if (IsComment(line))
                {
                    string text = CommentText(line);
                    if (text.Length > 0 && !IsDecoration(text))
                    {
                        section = text;
                    }
                    continue;
                }
                headerDone = true;
                string[] t = Tokens(StripInlineComment(line));
                if (t.Length < 2 || t[0].Length != 8 || t[0].Any(c => char.ToUpperInvariant(c) != 'A' && char.ToUpperInvariant(c) != 'B'))
                {
                    set.Warnings.Add($"{relative} Zeile {lineNo}: nicht lesbar: {line}");
                    continue;
                }
                // pattern tile tile … z1 z2 : the last two numbers are decimal values
                var numbers = t.Skip(1).ToList();
                int z1 = 0, z2 = 0;
                if (numbers.Count >= 3)
                {
                    z2 = HexList.ParseInt(numbers[numbers.Count - 1]);
                    z1 = HexList.ParseInt(numbers[numbers.Count - 2]);
                    numbers.RemoveRange(numbers.Count - 2, 2);
                }
                else if (numbers.Count == 2)
                {
                    z1 = HexList.ParseInt(numbers[1]);
                    numbers.RemoveAt(1);
                }
                file.Patterns.Add(new TransitionPattern
                {
                    Pattern = t[0],
                    Tiles = string.Join(" ", numbers.Select(n => HexList.ParseHex(n).ToString("x4"))),
                    Z1 = z1,
                    Z2 = z2,
                    Comment = section
                });
            }
            if (!headerDone)
            {
                file.HeaderComment = string.Empty;
            }
            return file;
        }

        private static void ReadItems(DragonRuleSet set, string scriptFolder, string path)
        {
            if (path == null)
            {
                set.Warnings.Add("items.txt fehlt.");
                return;
            }
            List<string> lines = ReadLines(path);
            set.FileHeaders["items.txt"] = HeaderOf(lines, l => TryParseRectLine(CommentText(l), out _));
            string comment = string.Empty;
            bool seenRule = false;
            foreach (string raw in lines)
            {
                string line = raw.Trim();
                if (line.Length == 0)
                {
                    continue;
                }
                bool disabled = IsComment(line);
                string data = disabled ? CommentText(line) : StripInlineComment(line);
                if (!TryParseRectLine(data, out string[] t))
                {
                    if (disabled && seenRule && !IsDecoration(CommentText(line)))
                    {
                        comment = CommentText(line);
                    }
                    continue;
                }
                seenRule = true;
                string relative = DragonRuleSet.NormalizePath("items/" + t[6]);
                var rule = new ItemRule
                {
                    Group = HexList.ParseHexInt(t[0]),
                    Chance = HexList.ParseInt(t[1]),
                    Left = HexList.ParseInt(t[2]),
                    Top = HexList.ParseInt(t[3]),
                    Right = HexList.ParseInt(t[4]),
                    Bottom = HexList.ParseInt(t[5]),
                    File = relative,
                    Enabled = !disabled,
                    Comment = comment
                };
                set.ItemRules.Add(rule);
                if (set.FindItemFile(relative) == null)
                {
                    string full = Find(scriptFolder, relative);
                    if (full == null)
                    {
                        set.Warnings.Add($"items.txt: Datei fehlt: {relative}");
                        continue;
                    }
                    rule.File = DragonRuleSet.NormalizePath("items/" + Path.GetFileName(full));
                    if (set.FindItemFile(rule.File) == null)
                    {
                        set.ItemFiles.Add(ReadItemFile(set, full, rule.File));
                    }
                }
                else
                {
                    rule.File = set.FindItemFile(relative).Path;
                }
            }

            // item files that are not used by items.txt (e.g. grass2.txt) are loaded too – nothing gets lost
            string itemDir = Path.Combine(scriptFolder, "items");
            if (Directory.Exists(itemDir))
            {
                foreach (string f in Directory.GetFiles(itemDir, "*.txt").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                {
                    string rel = DragonRuleSet.NormalizePath("items/" + Path.GetFileName(f));
                    if (set.FindItemFile(rel) == null)
                    {
                        set.ItemFiles.Add(ReadItemFile(set, f, rel));
                    }
                }
            }
        }

        /// <summary>"GG NNNN L T R B something" – used by items.txt and unevenly.txt.</summary>
        private static bool TryParseRectLine(string data, out string[] tokens)
        {
            tokens = Tokens(data);
            if (tokens.Length < 7 || !HexList.TryParseHexAddress(tokens[0], out _))
            {
                return false;
            }
            for (int i = 1; i <= 5; i++)
            {
                if (!HexList.IsInt(tokens[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static ItemFile ReadItemFile(DragonRuleSet set, string fullPath, string relative)
        {
            var file = new ItemFile { Path = relative };
            List<string> lines = ReadLines(fullPath);
            file.HeaderComment = HeaderOf(lines, l => false);
            string comment = string.Empty;
            bool headerDone = false;
            int lineNo = 0;
            foreach (string raw in lines)
            {
                lineNo++;
                string line = raw.Trim();
                if (line.Length == 0)
                {
                    comment = headerDone ? string.Empty : comment;
                    continue;
                }
                if (IsComment(line))
                {
                    if (headerDone && !IsDecoration(CommentText(line)))
                    {
                        comment = CommentText(line);
                    }
                    continue;
                }
                headerDone = true;
                string[] t = Tokens(StripInlineComment(line));
                if (t.Length < 6 || !HexList.IsInt(t[0]))
                {
                    set.Warnings.Add($"{relative} Zeile {lineNo}: nicht lesbar: {line}");
                    continue;
                }
                var group = new ItemGroupDef { Frequency = HexList.ParseInt(t[0]), Comment = comment };
                int count = (t.Length - 1) / 5;
                if ((t.Length - 1) % 5 != 0)
                {
                    set.Warnings.Add($"{relative} Zeile {lineNo}: Anzahl der Werte passt nicht (5 pro Static) – Rest ignoriert.");
                }
                for (int i = 0; i < count; i++)
                {
                    int o = 1 + i * 5;
                    group.Parts.Add(new ItemPart
                    {
                        Id = HexList.ParseHex(t[o]),
                        Z = HexList.ParseInt(t[o + 1]),
                        Hue = HexList.ParseHex(t[o + 2]),
                        Dx = HexList.ParseInt(t[o + 3]),
                        Dy = HexList.ParseInt(t[o + 4])
                    });
                }
                if (group.Parts.Count > 0)
                {
                    file.Groups.Add(group);
                }
                comment = string.Empty;
            }
            return file;
        }

        private static void ReadUnevenly(DragonRuleSet set, string path)
        {
            if (path == null)
            {
                return;
            }
            List<string> lines = ReadLines(path);
            set.FileHeaders["unevenly.txt"] = HeaderOf(lines, l => TryParseRectLine(CommentText(l), out _));
            string comment = string.Empty;
            bool seenRule = false;
            foreach (string raw in lines)
            {
                string line = raw.Trim();
                if (line.Length == 0)
                {
                    continue;
                }
                bool disabled = IsComment(line);
                string data = disabled ? CommentText(line) : StripInlineComment(line);
                if (!TryParseRectLine(data, out string[] t) || !HexList.IsInt(t[6]))
                {
                    if (disabled && seenRule && !IsDecoration(CommentText(line)))
                    {
                        comment = CommentText(line);
                    }
                    continue;
                }
                seenRule = true;
                set.UnevenRules.Add(new UnevenRule
                {
                    Group = HexList.ParseHexInt(t[0]),
                    Frequency = HexList.ParseInt(t[1]),
                    Left = HexList.ParseInt(t[2]),
                    Top = HexList.ParseInt(t[3]),
                    Right = HexList.ParseInt(t[4]),
                    Bottom = HexList.ParseInt(t[5]),
                    Amount = HexList.ParseInt(t[6]),
                    Enabled = !disabled,
                    Comment = comment
                });
            }
        }

        /// <summary>Fix table: "X Y ID Z" (all decimal, as written in the header of example.fix).</summary>
        public static void ReadFixTable(DragonRuleSet set, string path)
        {
            foreach (string raw in ReadLines(path))
            {
                string line = raw.Trim();
                if (line.Length == 0 || IsComment(line))
                {
                    continue;
                }
                string[] t = Tokens(StripInlineComment(line));
                if (t.Length >= 4 && t.Take(4).All(HexList.IsInt))
                {
                    set.Fixes.Add(new FixEntry
                    {
                        X = HexList.ParseInt(t[0]),
                        Y = HexList.ParseInt(t[1]),
                        TileId = (ushort)HexList.ParseInt(t[2]),
                        Z = HexList.ParseInt(t[3]),
                        Comment = Path.GetFileName(path)
                    });
                }
            }
        }

        private static void NameUnnamedEntries(DragonRuleSet set)
        {
            foreach (DragonPaletteEntry e in set.Palette)
            {
                if (e.Group >= 0)
                {
                    string g = set.GroupName(e.Group);
                    e.Name = e.Defined ? $"{g} z{e.Altitude}" : g;
                }
                else if (e.Defined)
                {
                    e.Name = $"Farbe {e.HexIndex} z{e.Altitude}";
                }
                else
                {
                    e.Name = "frei";
                }
            }
        }

        // ================================================================== SAVE

        /// <summary>Writes the rule set as a Dragon script folder (creates all sub folders).</summary>
        public static void SaveFolder(DragonRuleSet set, string scriptFolder)
        {
            Directory.CreateDirectory(scriptFolder);
            WriteGroups(set, Path.Combine(scriptFolder, "groups.txt"));
            WriteMapTrans(set, Path.Combine(scriptFolder, "maptrans.txt"));

            foreach (TransitionLayer layer in new[] { TransitionLayer.Map, TransitionLayer.Static })
            {
                for (int scriptSet = 1; scriptSet <= 3; scriptSet++)
                {
                    List<TransitionRule> rules = set.Transitions.Where(r => r.Layer == layer && Math.Max(1, Math.Min(3, r.ScriptSet)) == scriptSet).ToList();
                    string listName = TransitionRule.GetListFileName(layer, scriptSet);
                    if (rules.Count == 0 && !set.FileHeaders.ContainsKey(listName))
                    {
                        continue;
                    }
                    var sb = new StringBuilder();
                    AppendHeader(sb, set, listName, null);
                    string lastComment = null;
                    foreach (TransitionRule r in rules)
                    {
                        if (!string.IsNullOrEmpty(r.Comment) && r.Comment != lastComment)
                        {
                            sb.Append("// ").AppendLine(r.Comment);
                        }
                        lastComment = r.Comment;
                        string prefix = r.Enabled ? string.Empty : "//";
                        string file = Path.GetFileName(r.File);
                        if (scriptSet == 1)
                        {
                            sb.AppendLine($"{prefix}{HexG(set, r.GroupA)} {HexG(set, r.GroupB)} {file}");
                        }
                        else
                        {
                            sb.AppendLine($"{prefix}{HexG(set, scriptSet == 2 ? r.GroupA : r.GroupB)} {file}");
                        }
                    }
                    File.WriteAllText(Path.Combine(scriptFolder, listName), sb.ToString(), FileEncoding);
                }
            }

            foreach (TransitionFile f in set.TransitionFiles)
            {
                WriteTransitionFile(f, Path.Combine(scriptFolder, f.Path.Replace('/', Path.DirectorySeparatorChar)));
            }

            WriteItems(set, scriptFolder);
            WriteUnevenly(set, Path.Combine(scriptFolder, "unevenly.txt"));
            if (set.Fixes.Count > 0)
            {
                WriteFixTable(set, Path.Combine(scriptFolder, "fixtable.fix"));
            }
        }

        /// <summary>Group in the scripts: 2 hex digits (8 bit) or 4 (16 bit); -1 = ff / ffff.</summary>
        private static string HexG(DragonRuleSet set, int v) => set.ColorBits == 16 ? (v < 0 ? "ffff" : v.ToString("x4")) : (v < 0 ? "ff" : (v & 0xFF).ToString("x2"));

        private static string Dec3(int v) => v >= 0 ? v.ToString("000", CultureInfo.InvariantCulture) : v.ToString(CultureInfo.InvariantCulture).PadLeft(3);

        private static string Dec4(int v) => v >= 0 ? v.ToString("0000", CultureInfo.InvariantCulture) : v.ToString(CultureInfo.InvariantCulture);

        private static void AppendHeader(StringBuilder sb, DragonRuleSet set, string key, string fallback)
        {
            if (set.FileHeaders.TryGetValue(key, out string header) && !string.IsNullOrWhiteSpace(header))
            {
                sb.AppendLine(header);
                sb.AppendLine();
            }
            else if (!string.IsNullOrEmpty(fallback))
            {
                sb.AppendLine(fallback);
                sb.AppendLine();
            }
        }

        private static void WriteGroups(DragonRuleSet set, string path)
        {
            var sb = new StringBuilder();
            AppendHeader(sb, set, "groups.txt", "// Written by Dragon Map Studio\r\n//  You can create up to 64 groups\r\n//  Blacktiles must be Group 04 !!!");
            var ids = set.Groups.Select(g => g.Id).Union(set.Palette.Where(p => p.Group >= 0).Select(p => p.Group)).Distinct().OrderBy(i => i);
            foreach (int id in ids)
            {
                sb.AppendLine();
                sb.AppendLine("// ****************************");
                sb.AppendLine($"// {set.HexGroup(id)} = {set.GroupName(id)}");
                sb.AppendLine("// ****************************");
                foreach (DragonPaletteEntry e in set.Palette.Where(p => p.Group == id))
                {
                    sb.AppendLine($"{set.HexIndex(e.Index)} {set.HexGroup(id)}");
                }
            }
            File.WriteAllText(path, sb.ToString(), FileEncoding);
        }

        private static void WriteMapTrans(DragonRuleSet set, string path)
        {
            var sb = new StringBuilder();
            AppendHeader(sb, set, "maptrans.txt", "// Written by Dragon Map Studio\r\n// index(hex) altitude(dec) tile1(hex) tile2(hex) ...");
            string last = null;
            foreach (DragonPaletteEntry e in set.Palette.Where(p => p.Defined))
            {
                if (!string.IsNullOrEmpty(e.Comment) && e.Comment != last)
                {
                    sb.AppendLine();
                    sb.Append("// ").AppendLine(e.Comment);
                }
                last = e.Comment;
                string tiles = string.Join(" ", e.GetTileIds().Select(id => id.ToString("x4")));
                sb.AppendLine($"{set.HexIndex(e.Index)} {Dec3(e.Altitude)} {tiles}");
            }
            File.WriteAllText(path, sb.ToString(), FileEncoding);
        }

        public static void WriteTransitionFile(TransitionFile f, string fullPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(f.HeaderComment))
            {
                sb.AppendLine(f.HeaderComment);
                sb.AppendLine();
            }
            string last = null;
            foreach (TransitionPattern p in f.Patterns)
            {
                if (!string.IsNullOrEmpty(p.Comment) && p.Comment != last)
                {
                    if (sb.Length > 0)
                    {
                        sb.AppendLine();
                    }
                    sb.Append("// ").AppendLine(p.Comment);
                }
                last = p.Comment;
                string tiles = string.Join(" ", p.GetTileIds().Select(id => id.ToString("x4")));
                sb.AppendLine($"{p.Pattern} {tiles} {Dec3(p.Z1)} {Dec3(p.Z2)}");
            }
            File.WriteAllText(fullPath, sb.ToString(), FileEncoding);
        }

        private static void WriteItems(DragonRuleSet set, string scriptFolder)
        {
            var sb = new StringBuilder();
            AppendHeader(sb, set, "items.txt", "// [SourceGroup] [ChanceToPlace] [Left] [Top] [Right] [Bottom] [Filename]");
            foreach (ItemRule r in set.ItemRules)
            {
                string prefix = r.Enabled ? string.Empty : "//";
                sb.AppendLine($"{prefix}{HexG(set, r.Group)} {Dec4(r.Chance)} {Dec4(r.Left)} {Dec4(r.Top)} {Dec4(r.Right)} {Dec4(r.Bottom)} {Path.GetFileName(r.File)}");
            }
            File.WriteAllText(Path.Combine(scriptFolder, "items.txt"), sb.ToString(), FileEncoding);

            foreach (ItemFile f in set.ItemFiles)
            {
                WriteItemFile(f, Path.Combine(scriptFolder, f.Path.Replace('/', Path.DirectorySeparatorChar)));
            }
        }

        public static void WriteItemFile(ItemFile f, string fullPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(f.HeaderComment))
            {
                sb.AppendLine(f.HeaderComment);
                sb.AppendLine();
            }
            foreach (ItemGroupDef g in f.Groups)
            {
                if (!string.IsNullOrEmpty(g.Comment))
                {
                    sb.Append("// ").AppendLine(g.Comment);
                }
                sb.Append(Dec4(g.Frequency));
                foreach (ItemPart p in g.Parts)
                {
                    sb.Append($" {p.Id:x4} {Dec4(p.Z)} {p.Hue:x4} {Dec4(p.Dx)} {Dec4(p.Dy)}");
                }
                sb.AppendLine();
            }
            File.WriteAllText(fullPath, sb.ToString(), FileEncoding);
        }

        private static void WriteUnevenly(DragonRuleSet set, string path)
        {
            var sb = new StringBuilder();
            AppendHeader(sb, set, "unevenly.txt", "// [Source Group] [Frequency 1..1000] [Left] [Top] [Right] [Bottom] [Altitude random addition]");
            foreach (UnevenRule r in set.UnevenRules)
            {
                string prefix = r.Enabled ? string.Empty : "//";
                sb.AppendLine($"{prefix}{HexG(set, r.Group)} {Dec4(r.Frequency)} {Dec4(r.Left)} {Dec4(r.Top)} {Dec4(r.Right)} {Dec4(r.Bottom)} {Dec3(r.Amount)}");
            }
            File.WriteAllText(path, sb.ToString(), FileEncoding);
        }

        public static void WriteFixTable(DragonRuleSet set, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// Fix table written by Dragon Map Studio");
            sb.AppendLine("// X  Y   ID(dec) Z(dec)");
            foreach (FixEntry f in set.Fixes)
            {
                sb.AppendLine($"{Dec4(f.X)} {Dec4(f.Y)} {Dec4(f.TileId)} {Dec4(f.Z)}");
            }
            File.WriteAllText(path, sb.ToString(), FileEncoding);
        }
    }
}

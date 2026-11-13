using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>One change in the script (shown before writing).</summary>
    public sealed class ScriptChange
    {
        public int Line { get; set; }

        public string What { get; set; }

        public string Old { get; set; }

        public string New { get; set; }
    }

    /// <summary>Result of planning the write back: the new code, the changes and what could not be written.</summary>
    public sealed class ScriptWritePlan
    {
        public string NewCode { get; set; }

        public List<ScriptChange> Changes { get; } = new List<ScriptChange>();

        public List<string> Problems { get; } = new List<string>();
    }

    /// <summary>
    /// Writes changes of an imported gump back into its original script: only the numbers / texts of the Add… calls that changed
    /// are replaced, everything else (logic, comments, formatting) stays. Values that come from expressions are adjusted
    /// (x + 20 → x + 25), loops are moved as a whole, removed elements are commented out, new elements are added as lines.
    /// </summary>
    public static class GumpScriptWriter
    {
        public static string Hash(string code)
        {
            using (SHA1 sha = SHA1.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(code ?? string.Empty))).Replace("-", string.Empty);
            }
        }

        /// <summary>Argument index of a property per Add… method (by number of arguments); -1 = not written.</summary>
        private static int ArgIndex(string method, int count, string prop)
        {
            switch (method)
            {
                case "AddBackground":
                case "AddImageTiled":
                    return Index(prop, "X", "Y", "Width", "Height", "GumpId");
                case "AddImage":
                    return Index(prop, "X", "Y", "GumpId", "Hue");
                case "AddItem":
                    return Index(prop, "X", "Y", "ItemId", "Hue");
                case "AddSpriteImage":
                    return Index(prop, "X", "Y", "GumpId", "Width", "Height", "SourceX", "SourceY");
                case "AddAlphaRegion":
                    return Index(prop, "X", "Y", "Width", "Height");
                case "AddLabel":
                    return Index(prop, "X", "Y", "Hue", "Text");
                case "AddLabelCropped":
                    return Index(prop, "X", "Y", "Width", "Height", "Hue", "Text");
                case "AddHtml":
                    return Index(prop, "X", "Y", "Width", "Height", "Text", "HtmlBackground", "Scrollbar");
                case "AddHtmlLocalized":
                    switch (count)
                    {
                        case 8: return Index(prop, "X", "Y", "Width", "Height", "Cliloc", "Hue", "HtmlBackground", "Scrollbar");
                        case 9: return Index(prop, "X", "Y", "Width", "Height", "Cliloc", null, "Hue", "HtmlBackground", "Scrollbar");
                        case 6: return Index(prop, "X", "Y", "Width", "Height", "Cliloc", "Hue");
                        default: return Index(prop, "X", "Y", "Width", "Height", "Cliloc", "HtmlBackground", "Scrollbar");
                    }
                case "AddButton":
                    return Index(prop, "X", "Y", "GumpId", "GumpId2", "ButtonId", null, "TargetPage");
                case "AddImageTiledButton":
                    return Index(prop, "X", "Y", "GumpId", "GumpId2", "ButtonId", null, "TargetPage", "ItemId", "Hue", "Width", "Height", "Cliloc");
                case "AddCheck":
                case "AddCheckbox":
                case "AddRadio":
                    return Index(prop, "X", "Y", "GumpId", "GumpId2", "Checked", "SwitchId");
                case "AddTextEntry":
                case "AddTextEntryLimited":
                    return Index(prop, "X", "Y", "Width", "Height", "Hue", "EntryId", "Text", "MaxLength");
                default:
                    return -1;
            }
        }

        private static int Index(string prop, params string[] names) => Array.IndexOf(names, prop);

        private static readonly string[] Props =
        {
            "X", "Y", "Width", "Height", "GumpId", "GumpId2", "ItemId", "Hue", "SourceX", "SourceY", "Text", "Cliloc",
            "HtmlBackground", "Scrollbar", "MaxLength", "ButtonId", "TargetPage", "SwitchId", "Checked", "EntryId"
        };

        /// <summary>Properties that the element type really has (others keep their defaults and are ignored).</summary>
        private static bool Uses(GumpElement e, string prop)
        {
            PropertyInfo pi = typeof(GumpElement).GetProperty(prop);
            var applies = pi?.GetCustomAttribute<AppliesToAttribute>();
            return applies == null || applies.Types.Contains(e.Type);
        }

        private static object Value(GumpElement e, string prop) => typeof(GumpElement).GetProperty(prop)?.GetValue(e);

        /// <summary>The Add… call at a position: method name and the arguments with their positions in the code.</summary>
        private static (string Method, int Open, int Close, List<(int Start, int Length, string Text)> Args) Call(string code, int at)
        {
            Match m = Regex.Match(code.Substring(at, Math.Min(200, code.Length - at)), @"^(?:\w+\.)?(\w+)\s*(?:<[^>]*>)?\s*\(");
            if (!m.Success)
            {
                return (null, -1, -1, null);
            }
            int open = at + m.Length - 1;
            int depth = 0, close = -1;
            var args = new List<(int, int, string)>();
            int argStart = open + 1;
            for (int i = open; i < code.Length; i++)
            {
                char c = code[i];
                if (c == '"' || c == '\'')
                {
                    i = SkipLiteral(code, i) - 1;
                    continue;
                }
                if (c == '(' || c == '[' || c == '{')
                {
                    depth++;
                }
                else if (c == ')' || c == ']' || c == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        close = i;
                        Add(code, args, argStart, i);
                        break;
                    }
                }
                else if (c == ',' && depth == 1)
                {
                    Add(code, args, argStart, i);
                    argStart = i + 1;
                }
            }
            return (m.Groups[1].Value, open, close, args);
        }

        private static void Add(string code, List<(int, int, string)> args, int from, int to)
        {
            string raw = code.Substring(from, to - from);
            int lead = raw.Length - raw.TrimStart().Length;
            string t = raw.Trim();
            if (t.Length > 0)
            {
                args.Add((from + lead, t.Length, t));
            }
        }

        private static int SkipLiteral(string s, int i)
        {
            bool verbatim = i > 0 && s[i - 1] == '@' || i > 1 && s[i - 2] == '@' && s[i - 1] == '$';
            char q = s[i];
            for (i++; i < s.Length; i++)
            {
                if (verbatim)
                {
                    if (s[i] == '"')
                    {
                        if (i + 1 < s.Length && s[i + 1] == '"')
                        {
                            i++;
                            continue;
                        }
                        return i + 1;
                    }
                    continue;
                }
                if (s[i] == '\\')
                {
                    i++;
                    continue;
                }
                if (s[i] == q || s[i] == '\n')
                {
                    return i + 1;
                }
            }
            return s.Length;
        }

        private static string Literal(object v, string old)
        {
            switch (v)
            {
                case bool b:
                    return b ? "true" : "false";
                case int i:
                    return Regex.IsMatch(old ?? string.Empty, @"^0[xX]") && i >= 0 ? "0x" + i.ToString(old.Length > 2 && old.Substring(2).Any(char.IsLower) ? "x" : "X", CultureInfo.InvariantCulture) : i.ToString(CultureInfo.InvariantCulture);
                case string s:
                    return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", string.Empty).Replace("\n", "\\n") + "\"";
            }
            return null;
        }

        private static bool IsLiteral(string arg) =>
            Regex.IsMatch(arg, @"^-?(0[xX][0-9a-fA-F]+|\d+)$") || arg == "true" || arg == "false" || Regex.IsMatch(arg, @"^@?""([^""\\]|\\.|"""")*""$");

        /// <summary>x + 20 changed by 5 → x + 25; y → y + 5.</summary>
        private static string Adjust(string expr, int delta)
        {
            Match m = Regex.Match(expr, @"^(.*\S)\s*([+-])\s*(\d+)$", RegexOptions.Singleline);
            if (m.Success && Balanced(m.Groups[1].Value) && !m.Groups[1].Value.Contains("?"))
            {
                int n = (m.Groups[2].Value == "-" ? -1 : 1) * int.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture) + delta;
                return n == 0 ? m.Groups[1].Value : n > 0 ? $"{m.Groups[1].Value} + {n}" : $"{m.Groups[1].Value} - {-n}";
            }
            string baseExpr = expr.Contains("?") || expr.Contains("??") ? $"({expr})" : expr;
            return delta > 0 ? $"{baseExpr} + {delta}" : $"{baseExpr} - {-delta}";
        }

        private static bool Balanced(string s)
        {
            int d = 0;
            foreach (char c in s)
            {
                if (c == '(') d++;
                if (c == ')' && --d < 0) return false;
            }
            return d == 0;
        }

        private static int LineOf(string code, int pos)
        {
            int n = 1;
            for (int i = 0; i < pos && i < code.Length; i++)
            {
                if (code[i] == '\n')
                {
                    n++;
                }
            }
            return n;
        }

        private static (int At, int Occurrence) ParseSource(string s)
        {
            string[] p = (s ?? string.Empty).Split('#');
            return p.Length >= 2 && int.TryParse(p[0], out int at) && int.TryParse(p[1], out int occ) ? (at, occ) : (-1, -1);
        }

        /// <summary>Compares the gump with the script and plans the changes (the file is not written here).</summary>
        public static ScriptWritePlan Plan(GumpDesign current, string code, Func<string, string> resolve)
        {
            var plan = new ScriptWritePlan();
            if (!string.IsNullOrEmpty(current.SourceHash) && current.SourceHash != Hash(code))
            {
                plan.Problems.Add("The script was changed since it was imported – import it again, then make your changes. / Skript wurde seit dem Import geändert.");
                plan.NewCode = code;
                return plan;
            }
            GumpDesign fresh = GumpImporter.Import(code, null, current.ClassName, current.SourceResolve ? resolve : null);
            var edits = new List<(int Start, int Length, string Text, string What)>();
            var freshBySite = fresh.Elements.Where(e => ParseSource(e.Source).At >= 0).GroupBy(e => ParseSource(e.Source).At).ToDictionary(g => g.Key, g => g.ToList());
            var currentBySource = current.Elements.Where(e => !string.IsNullOrEmpty(e.Source)).GroupBy(e => e.Source).ToDictionary(g => g.Key, g => g.First());
            foreach (KeyValuePair<int, List<GumpElement>> site in freshBySite)
            {
                (string method, int open, int close, List<(int Start, int Length, string Text)> args) = Call(code, site.Key);
                if (method == null || close < 0)
                {
                    continue;
                }
                string line = $"line {LineOf(code, site.Key)}";
                var pairs = site.Value.Select(f => (Fresh: f, Now: currentBySource.TryGetValue(f.Source, out GumpElement n) ? n : null)).ToList();
                int removed = pairs.Count(p => p.Now == null);
                if (removed > 0)
                {
                    if (pairs.Count == 1)
                    {
                        string callText = code.Substring(site.Key, close + 1 - site.Key);
                        edits.Add((site.Key, close + 1 - site.Key, "/* removed by Gump Studio: " + callText.Replace("*/", "* /") + " */", "removed " + method));
                    }
                    else
                    {
                        plan.Problems.Add($"{line}: {removed} of {pairs.Count} elements of the loop were deleted – a loop can only be changed as a whole (not written).");
                    }
                    continue;
                }
                foreach ((GumpElement f, GumpElement now) in pairs.Where(p => p.Now.Page != p.Fresh.Page))
                {
                    plan.Problems.Add($"{line}: {now.Type} was moved from page {f.Page} to page {now.Page} – pages are not changed in the script.");
                    break;
                }
                foreach (string prop in Props)
                {
                    if (!Uses(pairs[0].Fresh, prop))
                    {
                        continue;
                    }
                    var changed = pairs.Where(p => !Equals(Value(p.Fresh, prop), Value(p.Now, prop))).ToList();
                    if (changed.Count == 0)
                    {
                        continue;
                    }
                    int index = ArgIndex(method, args.Count, prop);
                    if (index < 0)
                    {
                        plan.Problems.Add($"{line}: {prop} of {method} cannot be written back.");
                        continue;
                    }
                    if (index >= args.Count)
                    {
                        // optional argument that is missing (hue of AddImage / AddItem, size of AddTextEntry)
                        if (pairs.Count == 1 && index == args.Count)
                        {
                            object nv = Value(pairs[0].Now, prop);
                            edits.Add((close, 0, ", " + Literal(nv, null), $"{prop} {Value(pairs[0].Fresh, prop)} → {nv}"));
                        }
                        else
                        {
                            plan.Problems.Add($"{line}: {prop} of {method} is not in the call – add it by hand.");
                        }
                        continue;
                    }
                    (int start, int length, string text) = args[index];
                    if (pairs.Count == 1)
                    {
                        object nv = Value(pairs[0].Now, prop), ov = Value(pairs[0].Fresh, prop);
                        if (IsLiteral(text))
                        {
                            edits.Add((start, length, Literal(nv, text), $"{prop} {ov} → {nv}"));
                        }
                        else if (nv is int ni && ov is int oi)
                        {
                            edits.Add((start, length, Adjust(text, ni - oi), $"{prop} {oi} → {ni}"));
                        }
                        else
                        {
                            plan.Problems.Add($"{line}: {prop} comes from \"{text}\" – a text from code cannot be replaced (new value: \"{nv}\").");
                        }
                        continue;
                    }
                    // loop: numbers that changed by the same amount everywhere
                    List<int> deltas = pairs.Select(p => Value(p.Now, prop) is int a && Value(p.Fresh, prop) is int b ? a - b : int.MinValue).Distinct().ToList();
                    if (deltas.Count == 1 && deltas[0] != int.MinValue && deltas[0] != 0)
                    {
                        edits.Add((start, length, IsLiteral(text) && int.TryParse(text, out int lit) ? (lit + deltas[0]).ToString(CultureInfo.InvariantCulture) : Adjust(text, deltas[0]),
                            $"{prop} of the loop {(deltas[0] > 0 ? "+" : string.Empty)}{deltas[0]}"));
                    }
                    else
                    {
                        plan.Problems.Add($"{line}: {prop} of the {pairs.Count} elements of the loop changed differently – only equal changes of the whole loop can be written.");
                    }
                }
            }
            // the gump position in : base(x, y)
            if (current.X != fresh.X || current.Y != fresh.Y)
            {
                Match b = Regex.Match(code, @":\s*base\s*\(\s*(-?\d+)\s*,\s*(-?\d+)\s*\)");
                if (b.Success)
                {
                    if (current.X != fresh.X)
                    {
                        edits.Add((b.Groups[1].Index, b.Groups[1].Length, current.X.ToString(CultureInfo.InvariantCulture), $"gump X {fresh.X} → {current.X}"));
                    }
                    if (current.Y != fresh.Y)
                    {
                        edits.Add((b.Groups[2].Index, b.Groups[2].Length, current.Y.ToString(CultureInfo.InvariantCulture), $"gump Y {fresh.Y} → {current.Y}"));
                    }
                }
                else
                {
                    plan.Problems.Add("The gump position comes from code – not written.");
                }
            }
            AddNewElements(current, code, freshBySite, edits, plan);
            // apply from the end so positions stay valid
            var sorted = edits.OrderByDescending(e => e.Start).ThenByDescending(e => e.Length).ToList();
            var applied = new List<(int Start, int Length, string Text, string What)>();
            var sb = new StringBuilder(code);
            int lastStart = int.MaxValue;
            foreach ((int start, int length, string text, string what) e in sorted)
            {
                if (e.start + e.length > lastStart)
                {
                    plan.Problems.Add($"line {LineOf(code, e.start)}: overlapping change skipped ({e.what}).");
                    continue;
                }
                sb.Remove(e.start, e.length).Insert(e.start, e.text);
                applied.Add(e);
                lastStart = e.start;
            }
            // one entry per changed line: the old line and the line with all its changes
            foreach (IGrouping<int, (int Start, int Length, string Text, string What)> lineEdits in applied.GroupBy(e => code.LastIndexOf('\n', Math.Max(0, e.Start - 1)) + 1).OrderBy(g => g.Key))
            {
                int ls = lineEdits.Key;
                int le = code.IndexOf('\n', Math.Max(ls, lineEdits.Max(e => e.Start + e.Length) - 1));
                le = le < 0 ? code.Length : le;
                string newLine = code.Substring(ls, le - ls);
                foreach ((int start, int length, string text, string what) e in lineEdits.OrderByDescending(e => e.Start))
                {
                    newLine = newLine.Remove(e.start - ls, e.length).Insert(e.start - ls, e.text);
                }
                plan.Changes.Add(new ScriptChange
                {
                    Line = LineOf(code, ls), What = string.Join(", ", lineEdits.OrderBy(e => e.Start).Select(e => e.What)),
                    Old = code.Substring(ls, le - ls).Trim(), New = newLine.Trim()
                });
            }
            plan.NewCode = sb.ToString();
            return plan;
        }

        /// <summary>Elements made in the studio are added after the last Add… call of their page.</summary>
        private static void AddNewElements(GumpDesign current, string code, Dictionary<int, List<GumpElement>> freshBySite, List<(int, int, string, string)> edits, ScriptWritePlan plan)
        {
            List<GumpElement> added = current.Elements.Where(e => string.IsNullOrEmpty(e.Source) && (string.IsNullOrEmpty(e.ListId) || e.ListRow == 0)).ToList();
            if (added.Count == 0)
            {
                return;
            }
            if (current.Lists != null && current.Lists.Count > 0)
            {
                plan.Problems.Add("Lists made with the list generator are not written back – copy their loop from the generated script.");
            }
            foreach (IGrouping<int, GumpElement> group in added.Where(e => string.IsNullOrEmpty(e.ListId)).GroupBy(e => e.Page))
            {
                // after the last call of that page that runs once in the constructor / BuildLayout (not in a helper method or loop)
                int site = freshBySite.Where(s => s.Value.Count == 1 && s.Value[0].Page == group.Key && !s.Value[0].Source.EndsWith("#h", StringComparison.Ordinal))
                    .Select(s => s.Key).DefaultIfEmpty(-1).Max();
                if (site < 0)
                {
                    plan.Problems.Add($"{group.Count()} new element(s) on page {group.Key}: the script has no Add… call on that page – add them by hand (copy from the generated script).");
                    continue;
                }
                int semi = code.IndexOf(';', site);
                if (semi < 0)
                {
                    continue;
                }
                int lineStart = code.LastIndexOf('\n', site) + 1;
                string indent = Regex.Match(code.Substring(lineStart, site - lineStart), @"^\s*").Value;
                bool modern = Regex.IsMatch(code.Substring(site, Math.Min(20, code.Length - site)), @"^\w+\.Add");
                string nl = code.Contains("\r\n") ? "\r\n" : "\n";
                var sb = new StringBuilder();
                sb.Append(nl).Append(indent).Append("// added by Gump Studio");
                foreach (GumpElement e in group)
                {
                    sb.Append(nl).Append(indent).Append(GumpCodeGenerator.AddStatement(e, current, modern));
                    if (!string.IsNullOrEmpty(e.Name))
                    {
                        sb.Append(" // ").Append(e.Name);
                    }
                }
                int lineEnd = code.IndexOf('\n', semi);
                int insertAt = lineEnd < 0 ? code.Length : (lineEnd > 0 && code[lineEnd - 1] == '\r' ? lineEnd - 1 : lineEnd);
                edits.Add((insertAt, 0, sb.ToString(), $"{group.Count()} new element(s) on page {group.Key}"));
            }
        }
    }
}

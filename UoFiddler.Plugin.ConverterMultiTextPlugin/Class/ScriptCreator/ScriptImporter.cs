using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.ScriptCreator
{
    /// <summary>A value found in an existing script that can become a template field.</summary>
    internal sealed class ImportCandidate
    {
        public bool Use = true;
        public string Key;
        public string Type;
        public string Label;
        public string Default;
        public int Start;        // span in the source that is replaced
        public int Length;
        public string Prefix = string.Empty;   // kept text before the placeholder (e.g. quote, "LootType.")
        public string Suffix = string.Empty;
        public string Filter = string.Empty;   // e.g. "|cs" for strings
        public string Context;   // the source line, for the user
    }

    /// <summary>
    /// Turns an existing script into a template: finds the class name, the base class and literal values
    /// (assignments, method calls like SetStr(…), constructor base(…) arguments, overridden properties, attribute arguments)
    /// and replaces the chosen ones with {{placeholders}}.
    /// </summary>
    internal static class ScriptImporter
    {
        private static readonly Regex ClassRx = new Regex(@"\bclass\s+([A-Za-z_]\w*)\s*(?::\s*([A-Za-z_][\w\.]*))?", RegexOptions.Compiled);

        // literal: hex, number, string, bool, Enum.Member
        private const string Literal = @"(0x[0-9A-Fa-f]+|-?\d+(?:\.\d+)?|""(?:[^""\\]|\\.)*""|true|false|[A-Z]\w*\.[A-Z_]\w*)";

        private static readonly Regex AssignRx = new Regex(@"(?m)^[ \t]*(?:this\.)?([A-Za-z_][\w\.]*)\s*=\s*" + Literal + @"\s*;", RegexOptions.Compiled);
        private static readonly Regex CallRx = new Regex(@"(?m)^[ \t]*(?:this\.)?([A-Z]\w*)\s*\(([^;\r\n]*)\)\s*;", RegexOptions.Compiled);
        private static readonly Regex BaseRx = new Regex(@":\s*(base|this)\s*\(([^)]*)\)", RegexOptions.Compiled);
        private static readonly Regex OverrideRx = new Regex(@"override\s+(int|bool|double|string|\w+)\s+(\w+)\s*(?:\{\s*get\s*\{\s*return\s+" + Literal + @"\s*;\s*\}\s*\}|=>\s*" + Literal + @"\s*;)", RegexOptions.Compiled);
        private static readonly Regex AttrRx = new Regex(@"\[(Flipable|CorpseName|TypeAlias)\s*\(([^)]*)\)\]", RegexOptions.Compiled);
        private static readonly Regex ArgRx = new Regex(Literal, RegexOptions.Compiled);

        private static readonly HashSet<string> NotEnums = new HashSet<string>(StringComparer.Ordinal)
        {
            "Utility", "Math", "String", "Console", "DateTime", "TimeSpan", "Timer", "Convert", "Server", "System"
        };

        private static readonly HashSet<string> SkipCalls = new HashSet<string>(StringComparer.Ordinal)
        {
            "Serialize", "Deserialize", "InvalidateProperties", "Delete", "Console", "WriteLine"
        };

        public sealed class Result
        {
            public string Source;
            public string ClassName;
            public string BaseClass;
            public bool IsSphere;
            public List<ImportCandidate> Candidates = new List<ImportCandidate>();
        }

        public static Result Analyze(string source)
        {
            var r = new Result { Source = source ?? string.Empty };
            string src = r.Source;
            r.IsSphere = Regex.IsMatch(src, @"^\s*\[(ITEMDEF|CHARDEF)\s", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var taken = new List<(int Start, int End)>();

            bool Free(int start, int length) => !taken.Any(t => start < t.End && start + length > t.Start);

            void Add(ImportCandidate c)
            {
                if (c.Length <= 0 || !Free(c.Start, c.Length))
                {
                    return;
                }
                string key = Regex.Replace(c.Key, @"[^A-Za-z0-9_]", "_").Trim('_');
                if (key.Length == 0 || char.IsDigit(key[0]))
                {
                    key = "Value_" + key;
                }
                string unique = key;
                for (int n = 2; keys.Contains(unique); n++)
                {
                    unique = key + n;
                }
                keys.Add(unique);
                c.Key = unique;
                c.Label = c.Label ?? unique;
                c.Context = LineAt(src, c.Start);
                taken.Add((c.Start, c.Start + c.Length));
                r.Candidates.Add(c);
            }

            if (r.IsSphere)
            {
                AnalyzeSphere(r, Add);
                return r;
            }

            Match cls = ClassRx.Match(src);
            if (cls.Success)
            {
                r.ClassName = cls.Groups[1].Value;
                r.BaseClass = cls.Groups[2].Success ? cls.Groups[2].Value : null;
                if (r.BaseClass != null)
                {
                    Group g = cls.Groups[2];
                    Add(new ImportCandidate { Key = "BaseClass", Type = "string", Label = "Base class", Default = g.Value, Start = g.Index, Length = g.Length });
                }
            }

            foreach (Match m in AttrRx.Matches(src))
            {
                string attr = m.Groups[1].Value;
                Group args = m.Groups[2];
                if (attr == "Flipable")
                {
                    Add(new ImportCandidate { Key = "FlipIDs", Type = "string", Label = "Flip IDs", Default = args.Value.Trim(), Start = args.Index, Length = args.Length });
                }
                else
                {
                    AddLiterals(args, attr, Add);
                }
            }

            foreach (Match m in BaseRx.Matches(src))
            {
                AddLiterals(m.Groups[2], m.Groups[1].Value == "base" ? "Base" : "This", Add);
            }

            foreach (Match m in OverrideRx.Matches(src))
            {
                Group lit = m.Groups[3].Success ? m.Groups[3] : m.Groups[4];
                AddLiteral(lit, m.Groups[2].Value, Add);
            }

            foreach (Match m in AssignRx.Matches(src))
            {
                string path = m.Groups[1].Value;
                if (path.StartsWith("version", StringComparison.Ordinal) || path.StartsWith("m_", StringComparison.Ordinal))
                {
                    continue;
                }
                AddLiteral(m.Groups[2], path, Add);
            }

            foreach (Match m in CallRx.Matches(src))
            {
                string method = m.Groups[1].Value;
                if (SkipCalls.Contains(method))
                {
                    continue;
                }
                AddLiterals(m.Groups[2], method, Add);
            }

            r.Candidates.Sort((a, b) => a.Start.CompareTo(b.Start));
            return r;
        }

        /// <summary>Literals inside an argument list, named Method_Enum_1, _2 … (enum arguments name the group: SetSkill(SkillName.Magery, 90, 100)).</summary>
        private static void AddLiterals(Group args, string prefix, Action<ImportCandidate> add)
        {
            const string enumRx = @"^[A-Z]\w*\.[A-Z_]\w*$";
            Match[] lits = ArgRx.Matches(args.Value).Cast<Match>().ToArray();
            Match[] enums = lits.Where(l => Regex.IsMatch(l.Value, enumRx)).ToArray();
            int numeric = lits.Length - enums.Length;
            // one enum + numbers (SetSkill(SkillName.Magery, 90, 100)): the enum names the group and stays fixed
            bool groupByEnum = enums.Length == 1 && numeric > 0;
            string name = groupByEnum ? prefix + "_" + enums[0].Value.Substring(enums[0].Value.IndexOf('.') + 1) : prefix;
            int n = 0;
            foreach (Match l in lits)
            {
                bool isEnum = Regex.IsMatch(l.Value, enumRx);
                string key;
                if (isEnum)
                {
                    if (groupByEnum)
                    {
                        continue;
                    }
                    key = enums.Length == 1 ? prefix + "_" + l.Value.Substring(0, l.Value.IndexOf('.')) : l.Value.Substring(0, l.Value.IndexOf('.'));
                }
                else
                {
                    n++;
                    key = numeric > 1 ? $"{name}_{n}" : name;
                }
                AddLiteral(new FakeGroup(args.Index + l.Index, l.Value), key, add);
            }
        }

        private readonly struct FakeGroup
        {
            public FakeGroup(int index, string value)
            {
                Index = index;
                Value = value;
            }

            public int Index { get; }
            public string Value { get; }
        }

        private static void AddLiteral(Group g, string name, Action<ImportCandidate> add) => AddLiteral(new FakeGroup(g.Index, g.Value), name, add);

        private static void AddLiteral(FakeGroup g, string name, Action<ImportCandidate> add)
        {
            string v = g.Value;
            string last = name.Contains('.') ? name.Substring(name.LastIndexOf('.') + 1) : name;
            var c = new ImportCandidate { Key = name.Replace('.', '_'), Label = name, Start = g.Index, Length = v.Length };
            if (v.StartsWith("\"", StringComparison.Ordinal))
            {
                c.Type = "string";
                c.Default = Regex.Unescape(v.Substring(1, v.Length - 2));
                c.Start = g.Index + 1;
                c.Length = v.Length - 2;
                c.Filter = "|cs";
                if (c.Length == 0)
                {
                    return;
                }
            }
            else if (v == "true" || v == "false")
            {
                c.Type = "bool";
                c.Default = v;
            }
            else if (Regex.IsMatch(v, @"^[A-Z]\w*\.[A-Z_]\w*$"))
            {
                int dot = v.IndexOf('.');
                if (NotEnums.Contains(v.Substring(0, dot)))
                {
                    return; // a static call like Utility.RandomDyedHue(), not a value
                }
                string enumName = v.Substring(0, dot);
                c.Default = v.Substring(dot + 1);
                c.Start = g.Index + dot + 1;
                c.Length = v.Length - dot - 1;
                c.Type = ScriptLists.Resolve("@" + enumName).Length > 0 ? "choice:@" + enumName : "string";
            }
            else if (v.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                c.Default = v;
                c.Type = last.Equals("Hue", StringComparison.OrdinalIgnoreCase) ? "hue"
                    : Regex.IsMatch(last, "Gump|Label|Cliloc", RegexOptions.IgnoreCase) ? "int"
                    : Regex.IsMatch(last, "Sound", RegexOptions.IgnoreCase) ? "sound"
                    : Regex.IsMatch(last, "^Body", RegexOptions.IgnoreCase) ? "body"
                    : "itemid";
            }
            else if (v.Contains('.'))
            {
                c.Type = "double";
                c.Default = v;
            }
            else
            {
                c.Default = v;
                c.Type = last.Equals("Hue", StringComparison.OrdinalIgnoreCase) ? "hue"
                    : last.Equals("ItemID", StringComparison.OrdinalIgnoreCase) ? "itemid"
                    : Regex.IsMatch(last, "Sound", RegexOptions.IgnoreCase) ? "sound"
                    : Regex.IsMatch(last, "^Body", RegexOptions.IgnoreCase) ? "body"
                    : "int";
            }
            add(c);
        }

        private static void AnalyzeSphere(Result r, Action<ImportCandidate> add)
        {
            string src = r.Source;
            Match head = Regex.Match(src, @"^\s*\[(ITEMDEF|CHARDEF)\s+([^\]\s]+)\]", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            if (head.Success)
            {
                r.ClassName = head.Groups[2].Value;
                Group g = head.Groups[2];
                add(new ImportCandidate { Key = "DefName", Type = "class", Label = "Defname", Default = g.Value, Start = g.Index, Length = g.Length });
            }
            foreach (Match m in Regex.Matches(src, @"(?m)^[ \t]*([A-Za-z_][\w\.]*)[ \t]*=[ \t]*([^\r\n/]*?)[ \t]*(?://.*)?$"))
            {
                string key = m.Groups[1].Value;
                Group val = m.Groups[2];
                if (val.Length == 0 || key.Equals("ON", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                add(new ImportCandidate { Key = key, Type = "string", Label = key, Default = val.Value, Start = val.Index, Length = val.Length });
            }
        }

        private static string LineAt(string s, int pos)
        {
            int a = s.LastIndexOf('\n', Math.Max(0, Math.Min(s.Length - 1, pos)));
            int b = s.IndexOf('\n', Math.Min(s.Length, pos));
            if (b < 0)
            {
                b = s.Length;
            }
            return s.Substring(a + 1, b - a - 1).Trim();
        }

        /// <summary>Builds the template from the chosen candidates. The class name becomes {{ClassName}} everywhere.</summary>
        public static ScriptTemplate Build(Result r, string name, string group, string description)
        {
            string src = r.Source.Replace("{{", "\u0001\u0002"); // same length – the candidate positions stay valid
            var t = new ScriptTemplate
            {
                Name = name,
                Group = group,
                Description = description ?? string.Empty,
                Output = r.IsSphere ? "scp" : "cs",
            };
            bool classField = !string.IsNullOrEmpty(r.ClassName) && !r.IsSphere;
            if (classField)
            {
                t.Fields.Add(new TemplateField { Key = "ClassName", Type = "class", Label = "Class name", Default = r.ClassName });
                t.FileName = "{{ClassName}}.cs";
            }
            else if (r.IsSphere)
            {
                t.FileName = "{{DefName}}.scp";
            }
            else
            {
                t.FileName = "script.cs";
            }

            var used = r.Candidates.Where(c => c.Use).OrderBy(c => c.Start).ToList();
            foreach (ImportCandidate c in used)
            {
                if (t.Fields.Any(f => string.Equals(f.Key, c.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
                t.Fields.Add(new TemplateField { Key = c.Key, Type = c.Type, Label = c.Label, Default = c.Default ?? string.Empty });
            }

            var sb = new StringBuilder();
            int pos = 0;
            foreach (ImportCandidate c in used)
            {
                if (c.Start < pos)
                {
                    continue;
                }
                sb.Append(ReplaceClass(src.Substring(pos, c.Start - pos), r.ClassName, classField));
                sb.Append("{{").Append(c.Key).Append(c.Filter).Append("}}");
                pos = c.Start + c.Length;
            }
            sb.Append(ReplaceClass(src.Substring(pos), r.ClassName, classField));
            t.Body = sb.ToString().Replace("\u0001\u0002", "{{lb}}").Replace("\r\n", "\n");
            return t;
        }

        private static string ReplaceClass(string text, string className, bool active) =>
            active ? Regex.Replace(text, @"\b" + Regex.Escape(className) + @"\b", "{{ClassName}}") : text;

        public static string DescribeType(string type) => type ?? "string";

        public static string[] Types => new[] { "string", "class", "int", "double", "range", "bool", "itemid", "hue", "body", "sound", "text" };

        public static string Invariant(double d) => d.ToString(CultureInfo.InvariantCulture);
    }
}

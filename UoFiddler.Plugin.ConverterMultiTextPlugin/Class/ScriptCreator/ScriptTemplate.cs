using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.ScriptCreator
{
    /// <summary>One input of a template (a line "@field Key | type | Label | default | tip").</summary>
    internal sealed class TemplateField
    {
        public string Key;
        public string Type = "string";   // class, string, text, int[:min:max], double, range, bool, itemid, hue, body, sound, choice:..., combo:..., list:..., pages[:lines:chars]
        public string Label;
        public string Default = string.Empty;
        public string Tip;

        public string BaseType => Type.Split(':')[0].Trim().ToLowerInvariant();

        /// <summary>The part after the first ':' (choice items, list name, ranges).</summary>
        public string Options
        {
            get
            {
                int i = Type.IndexOf(':');
                return i < 0 ? string.Empty : Type.Substring(i + 1).Trim();
            }
        }

        public bool IsList => BaseType == "list";
        public bool IsMultiline => BaseType == "text" || BaseType == "pages";
    }

    /// <summary>
    /// A script template. File format (*.uotpl, UTF-8):
    /// <code>
    /// @name Ranged weapon
    /// @group Weapons
    /// @output cs                         (file extension of the result: cs, scp, txt …)
    /// @file {{ClassName}}.cs             (file name of the result)
    /// @description What it is for
    /// @field ClassName | class | Class name | MagicBow | optional tooltip
    /// @check sum:Phys,Fire,Cold,Pois,Nrgy=100
    /// @template
    /// …script text with {{Key}}, {{Key|cs}}, {{#if Key}}…{{else}}…{{/if}}, {{#each List}}{{key}} {{value}}{{/each}}…
    /// </code>
    /// </summary>
    internal sealed class ScriptTemplate
    {
        public string Name = "New template";
        public string Group = "My templates";
        public string Output = "cs";
        public string FileName = "{{ClassName}}.cs";
        public string Description = string.Empty;
        public List<TemplateField> Fields = new List<TemplateField>();
        public List<string> Checks = new List<string>();
        public string Body = string.Empty;

        /// <summary>Where it came from: null = built in, otherwise the file.</summary>
        public string SourceFile;

        public bool IsBuiltIn => SourceFile == null;

        public override string ToString() => Name;

        // ------------------------------------------------------------------ parse / write

        public static ScriptTemplate Parse(string text)
        {
            var t = new ScriptTemplate();
            string[] lines = (text ?? string.Empty).Replace("\r\n", "\n").Split('\n');
            int i = 0;
            for (; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd();
                if (line.Trim() == "@template")
                {
                    i++;
                    break;
                }
                if (line.Trim().Length == 0 || line.TrimStart().StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }
                if (!line.StartsWith("@", StringComparison.Ordinal))
                {
                    throw new FormatException($"Line {i + 1}: header lines start with @ (missing @template?)");
                }
                int sp = line.IndexOf(' ');
                string key = (sp < 0 ? line : line.Substring(0, sp)).Substring(1).ToLowerInvariant();
                string value = sp < 0 ? string.Empty : line.Substring(sp + 1).Trim();
                switch (key)
                {
                    case "name": t.Name = value; break;
                    case "group": t.Group = value; break;
                    case "output": t.Output = value.TrimStart('.'); break;
                    case "file": t.FileName = value; break;
                    case "description": t.Description += (t.Description.Length > 0 ? "\n" : string.Empty) + value; break;
                    case "check": t.Checks.Add(value); break;
                    case "field":
                    {
                        string[] p = value.Split('|').Select(x => x.Trim()).ToArray();
                        if (p.Length < 1 || p[0].Length == 0)
                        {
                            throw new FormatException($"Line {i + 1}: @field needs a key");
                        }
                        t.Fields.Add(new TemplateField
                        {
                            Key = p[0],
                            Type = p.Length > 1 && p[1].Length > 0 ? p[1] : "string",
                            Label = p.Length > 2 && p[2].Length > 0 ? p[2] : p[0],
                            Default = p.Length > 3 ? p[3].Replace("\\n", "\n") : string.Empty,
                            Tip = p.Length > 4 ? string.Join(" | ", p.Skip(4)) : null
                        });
                        break;
                    }
                    default:
                        throw new FormatException($"Line {i + 1}: unknown header \"@{key}\"");
                }
            }
            t.Body = string.Join("\n", lines.Skip(i));
            return t;
        }

        public string ToText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("@name " + Name);
            sb.AppendLine("@group " + Group);
            sb.AppendLine("@output " + Output);
            sb.AppendLine("@file " + FileName);
            foreach (string d in (Description ?? string.Empty).Split('\n').Where(d => d.Trim().Length > 0))
            {
                sb.AppendLine("@description " + d.Trim());
            }
            foreach (TemplateField f in Fields)
            {
                sb.Append("@field ").Append(f.Key).Append(" | ").Append(f.Type).Append(" | ").Append(f.Label).Append(" | ").Append((f.Default ?? string.Empty).Replace("\r\n", "\n").Replace("\n", "\\n"));
                if (!string.IsNullOrEmpty(f.Tip))
                {
                    sb.Append(" | ").Append(f.Tip);
                }
                sb.AppendLine();
            }
            foreach (string c in Checks)
            {
                sb.AppendLine("@check " + c);
            }
            sb.AppendLine("@template");
            sb.Append(Body);
            return sb.ToString();
        }

        // ------------------------------------------------------------------ values

        /// <summary>Default values of all fields.</summary>
        public Dictionary<string, string> DefaultValues() => Fields.ToDictionary(f => f.Key, f => f.Default ?? string.Empty, StringComparer.OrdinalIgnoreCase);

        /// <summary>A list value "Key=Value;Key2=Value2" (also one per line) → pairs.</summary>
        public static List<KeyValuePair<string, string>> ParseList(string value)
        {
            var list = new List<KeyValuePair<string, string>>();
            foreach (string part in (value ?? string.Empty).Replace("\r\n", "\n").Split(';', '\n'))
            {
                string p = part.Trim();
                if (p.Length == 0)
                {
                    continue;
                }
                int eq = p.IndexOf('=');
                list.Add(eq < 0 ? new KeyValuePair<string, string>(p, string.Empty) : new KeyValuePair<string, string>(p.Substring(0, eq).Trim(), p.Substring(eq + 1).Trim()));
            }
            return list;
        }

        public static string JoinList(IEnumerable<KeyValuePair<string, string>> items) =>
            string.Join(";", items.Where(kv => !string.IsNullOrWhiteSpace(kv.Key)).Select(kv => kv.Value.Length > 0 ? $"{kv.Key}={kv.Value}" : kv.Key));

        // ------------------------------------------------------------------ validation

        private static readonly HashSet<string> CsKeywords = new HashSet<string>(
            "abstract as base bool break byte case catch char checked class const continue decimal default delegate do double else enum event explicit extern false finally fixed float for foreach goto if implicit in int interface internal is lock long namespace new null object operator out override params private protected public readonly ref return sbyte sealed short sizeof stackalloc static string struct switch this throw true try typeof uint ulong unchecked unsafe ushort using virtual void volatile while"
                .Split(' '));

        /// <summary>Problems with the values (empty = all fine).</summary>
        public List<string> Validate(Dictionary<string, string> values)
        {
            var problems = new List<string>();
            foreach (TemplateField f in Fields)
            {
                values.TryGetValue(f.Key, out string v);
                v = v ?? string.Empty;
                string label = f.Label ?? f.Key;
                string[] o = f.Options.Split(':');
                switch (f.BaseType)
                {
                    case "class":
                        if (!Regex.IsMatch(v, @"^[A-Za-z_][A-Za-z0-9_]*$"))
                        {
                            problems.Add($"{label}: \"{v}\" is not a valid class name (letters, digits, _; no space, not starting with a digit).");
                        }
                        else if (CsKeywords.Contains(v))
                        {
                            problems.Add($"{label}: \"{v}\" is a C# keyword.");
                        }
                        break;
                    case "int":
                    case "itemid":
                    case "hue":
                    case "body":
                    case "sound":
                        if (!TryParseInt(v, out long n))
                        {
                            problems.Add($"{label}: \"{v}\" is not a number.");
                            break;
                        }
                        long min = f.BaseType == "int" && o.Length > 0 && long.TryParse(o[0], out long a) ? a : long.MinValue;
                        long max = f.BaseType == "int" && o.Length > 1 && long.TryParse(o[1], out long b) ? b : long.MaxValue;
                        if (f.BaseType == "itemid") { min = 0; max = 0xFFFF; }
                        if (f.BaseType == "hue") { min = 0; max = 0xFFFF; }
                        if (f.BaseType == "body" || f.BaseType == "sound") { min = 0; max = 0xFFFF; }
                        if (n < min || n > max)
                        {
                            problems.Add($"{label}: {n} is outside {min} … {max}.");
                        }
                        break;
                    case "double":
                        if (!double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                        {
                            problems.Add($"{label}: \"{v}\" is not a number (use a dot: 2.5).");
                        }
                        break;
                    case "bool":
                        if (!bool.TryParse(v, out _))
                        {
                            problems.Add($"{label}: use true or false.");
                        }
                        break;
                    case "range":
                    {
                        string[] r = v.Split(',');
                        if (r.Length != 2 || r.Any(x => !double.TryParse(x.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out _)))
                        {
                            problems.Add($"{label}: write min,max (e.g. 100,150).");
                        }
                        else if (double.Parse(r[0].Trim(), CultureInfo.InvariantCulture) > double.Parse(r[1].Trim(), CultureInfo.InvariantCulture))
                        {
                            problems.Add($"{label}: min is larger than max.");
                        }
                        break;
                    }
                    case "list":
                        foreach (KeyValuePair<string, string> kv in ParseList(v))
                        {
                            if (!Regex.IsMatch(kv.Key, @"^[A-Za-z_][A-Za-z0-9_\.]*$"))
                            {
                                problems.Add($"{label}: \"{kv.Key}\" is not a valid name.");
                            }
                        }
                        break;
                }
            }
            foreach (string check in Checks)
            {
                // sum:A,B,C=100
                Match m = Regex.Match(check, @"^sum:([\w,\s]+)=(-?\d+)$", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    long sum = 0;
                    string[] keys = m.Groups[1].Value.Split(',').Select(k => k.Trim()).ToArray();
                    foreach (string k in keys)
                    {
                        if (!values.TryGetValue(k, out string v))
                        {
                            continue;
                        }
                        TemplateField lf = Fields.FirstOrDefault(f => string.Equals(f.Key, k, StringComparison.OrdinalIgnoreCase));
                        if (lf != null && lf.IsList)
                        {
                            foreach (KeyValuePair<string, string> kv in ParseList(v))
                            {
                                if (TryParseInt(kv.Value, out long ln))
                                {
                                    sum += ln;
                                }
                            }
                        }
                        else if (TryParseInt(v, out long n))
                        {
                            sum += n;
                        }
                    }
                    if (sum.ToString(CultureInfo.InvariantCulture) != m.Groups[2].Value)
                    {
                        problems.Add($"{string.Join(" + ", keys)} = {sum}, should be {m.Groups[2].Value}.");
                    }
                }
            }
            return problems;
        }

        public static bool TryParseInt(string text, out long value)
        {
            value = 0;
            string t = (text ?? string.Empty).Trim();
            bool neg = t.StartsWith("-", StringComparison.Ordinal);
            if (neg)
            {
                t = t.Substring(1);
            }
            bool ok = t.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? long.TryParse(t.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value)
                : long.TryParse(t, NumberStyles.None, CultureInfo.InvariantCulture, out value);
            if (ok && neg)
            {
                value = -value;
            }
            return ok;
        }

        // ------------------------------------------------------------------ rendering

        /// <summary>Builds the script from the values.</summary>
        public string Render(Dictionary<string, string> values) => TemplateEngine.Render(Body, this, values);

        public string RenderFileName(Dictionary<string, string> values)
        {
            string name = TemplateEngine.Render(FileName ?? "script.{{output}}", this, values).Trim();
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            if (!name.Contains("."))
            {
                name += "." + Output;
            }
            return name;
        }
    }

    /// <summary>
    /// Small template language:
    /// {{Key}} value · {{Key|filter}} filters: cs (C# string escape), lower, upper, hex, dec, scp (Sphere hex 0abc), trim, ident (class name safe) ·
    /// {{#if Key}}…{{else}}…{{/if}} (true = not empty, not 0, not false, not None) · {{#if !Key}} ·
    /// {{#each List}}…{{/each}} with {{key}} {{value}} {{value1}} {{value2}} {{index}} {{sep}} (", " except at the last) ·
    /// {{lb}} = "{{" · lines that hold only a block tag disappear.
    /// </summary>
    internal static class TemplateEngine
    {
        private abstract class Node
        {
        }

        private sealed class TextNode : Node
        {
            public string Text;
        }

        private sealed class VarNode : Node
        {
            public string Name;
            public string[] Filters;
        }

        private sealed class IfNode : Node
        {
            public string Name;
            public bool Negate;
            public List<Node> Then = new List<Node>();
            public List<Node> Else = new List<Node>();
        }

        private sealed class EachNode : Node
        {
            public string Name;
            public List<Node> Body = new List<Node>();
        }

        private static readonly Regex Standalone = new Regex(@"(?m)^[ \t]*(\{\{(?:#|/|else)[^}]*\}\})[ \t]*\r?\n", RegexOptions.Compiled);
        private static readonly Regex Tag = new Regex(@"\{\{(.*?)\}\}", RegexOptions.Compiled | RegexOptions.Singleline);

        public static string Render(string body, ScriptTemplate template, Dictionary<string, string> values)
        {
            string prepared = Standalone.Replace(body ?? string.Empty, "$1");
            int pos = 0;
            List<Node> nodes = ParseNodes(prepared, ref pos, null);
            var sb = new StringBuilder();
            var fields = template.Fields.ToDictionary(f => f.Key, f => f, StringComparer.OrdinalIgnoreCase);
            Emit(nodes, sb, new Scope(values, fields, null));
            return sb.ToString();
        }

        private static List<Node> ParseNodes(string s, ref int pos, string until)
        {
            var nodes = new List<Node>();
            while (pos < s.Length)
            {
                Match m = Tag.Match(s, pos);
                if (!m.Success)
                {
                    nodes.Add(new TextNode { Text = s.Substring(pos) });
                    pos = s.Length;
                    break;
                }
                if (m.Index > pos)
                {
                    nodes.Add(new TextNode { Text = s.Substring(pos, m.Index - pos) });
                }
                pos = m.Index + m.Length;
                string tag = m.Groups[1].Value.Trim();
                if (tag.StartsWith("/", StringComparison.Ordinal) || tag == "else")
                {
                    if (until == null)
                    {
                        throw new FormatException($"\"{{{{{tag}}}}}\" without an opening block.");
                    }
                    // hand the closing tag back to the caller
                    pos = m.Index;
                    return nodes;
                }
                if (tag.StartsWith("#if ", StringComparison.Ordinal))
                {
                    string name = tag.Substring(4).Trim();
                    var node = new IfNode { Negate = name.StartsWith("!", StringComparison.Ordinal) };
                    node.Name = name.TrimStart('!').Trim();
                    node.Then = ParseNodes(s, ref pos, "if");
                    string closing = ReadTag(s, ref pos);
                    if (closing == "else")
                    {
                        node.Else = ParseNodes(s, ref pos, "if");
                        closing = ReadTag(s, ref pos);
                    }
                    if (closing != "/if")
                    {
                        throw new FormatException($"{{{{#if {name}}}}} needs {{{{/if}}}}.");
                    }
                    nodes.Add(node);
                }
                else if (tag.StartsWith("#each ", StringComparison.Ordinal))
                {
                    var node = new EachNode { Name = tag.Substring(6).Trim() };
                    node.Body = ParseNodes(s, ref pos, "each");
                    if (ReadTag(s, ref pos) != "/each")
                    {
                        throw new FormatException($"{{{{#each {node.Name}}}}} needs {{{{/each}}}}.");
                    }
                    nodes.Add(node);
                }
                else
                {
                    string[] parts = tag.Split('|').Select(p => p.Trim()).ToArray();
                    nodes.Add(new VarNode { Name = parts[0], Filters = parts.Skip(1).ToArray() });
                }
            }
            if (until != null)
            {
                throw new FormatException($"A {{{{#{until}}}}} block is not closed.");
            }
            return nodes;
        }

        private static string ReadTag(string s, ref int pos)
        {
            Match m = Tag.Match(s, pos);
            if (!m.Success || m.Index != pos)
            {
                return null;
            }
            pos = m.Index + m.Length;
            return m.Groups[1].Value.Trim();
        }

        private sealed class Scope
        {
            private readonly Dictionary<string, string> _values;
            private readonly Dictionary<string, TemplateField> _fields;
            private readonly Scope _parent;
            public Dictionary<string, string> Locals;

            public Scope(Dictionary<string, string> values, Dictionary<string, TemplateField> fields, Scope parent)
            {
                _values = values;
                _fields = fields;
                _parent = parent;
            }

            public string Get(string name)
            {
                if (Locals != null && Locals.TryGetValue(name, out string l))
                {
                    return l;
                }
                if (_parent != null)
                {
                    return _parent.Get(name);
                }
                if (name == "lb")
                {
                    return "{{";
                }
                if (name == "rb")
                {
                    return "}}";
                }
                return _values != null && _values.TryGetValue(name, out string v) ? v ?? string.Empty : null;
            }

            public TemplateField Field(string name) => _parent != null ? _parent.Field(name) : _fields.TryGetValue(name, out TemplateField f) ? f : null;

            public Scope Child() => new Scope(_values, _fields, this) { Locals = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) };
        }

        private static void Emit(List<Node> nodes, StringBuilder sb, Scope scope)
        {
            foreach (Node n in nodes)
            {
                switch (n)
                {
                    case TextNode t:
                        sb.Append(t.Text);
                        break;
                    case VarNode v:
                    {
                        string value = scope.Get(v.Name);
                        if (value == null)
                        {
                            sb.Append("{{?").Append(v.Name).Append("}}");
                            break;
                        }
                        foreach (string f in v.Filters)
                        {
                            value = ApplyFilter(value, f);
                        }
                        sb.Append(value);
                        break;
                    }
                    case IfNode i:
                    {
                        bool truth = IsTrue(scope.Get(i.Name), scope.Field(i.Name));
                        Emit(truth != i.Negate ? i.Then : i.Else, sb, scope);
                        break;
                    }
                    case EachNode e:
                    {
                        TemplateField field = scope.Field(e.Name);
                        List<KeyValuePair<string, string>> items = field != null && field.BaseType == "pages"
                            ? BookPages(scope.Get(e.Name), field)
                            : ScriptTemplate.ParseList(scope.Get(e.Name));
                        for (int k = 0; k < items.Count; k++)
                        {
                            Scope child = scope.Child();
                            child.Locals["key"] = items[k].Key;
                            child.Locals["value"] = items[k].Value;
                            string[] parts = items[k].Value.Split(',');
                            for (int p = 0; p < parts.Length; p++)
                            {
                                child.Locals["value" + (p + 1)] = parts[p].Trim();
                            }
                            child.Locals["index"] = k.ToString(CultureInfo.InvariantCulture);
                            child.Locals["number"] = (k + 1).ToString(CultureInfo.InvariantCulture);
                            child.Locals["sep"] = k < items.Count - 1 ? ", " : string.Empty;
                            child.Locals["last"] = k == items.Count - 1 ? "true" : string.Empty;
                            child.Locals["first"] = k == 0 ? "true" : string.Empty;
                            Emit(e.Body, sb, child);
                        }
                        break;
                    }
                }
            }
        }

        private static bool IsTrue(string v, TemplateField field)
        {
            if (field != null && field.IsList)
            {
                return ScriptTemplate.ParseList(v).Count > 0;
            }
            v = (v ?? string.Empty).Trim();
            return v.Length > 0 && v != "0" && v != "0x0" && !v.Equals("false", StringComparison.OrdinalIgnoreCase) && !v.Equals("none", StringComparison.OrdinalIgnoreCase);
        }

        public static string ApplyFilter(string value, string filter)
        {
            switch (filter.ToLowerInvariant())
            {
                case "cs":
                    return value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", string.Empty).Replace("\n", "\\n");
                case "lower":
                    return value.ToLowerInvariant();
                case "upper":
                    return value.ToUpperInvariant();
                case "trim":
                    return value.Trim();
                case "hex":
                    return ScriptTemplate.TryParseInt(value, out long h) ? "0x" + h.ToString("X4", CultureInfo.InvariantCulture) : value;
                case "dec":
                    return ScriptTemplate.TryParseInt(value, out long d) ? d.ToString(CultureInfo.InvariantCulture) : value;
                case "scp":
                    return ScriptTemplate.TryParseInt(value, out long s) ? "0" + s.ToString("x", CultureInfo.InvariantCulture) : value;
                case "ident":
                    return Regex.Replace(value, @"[^A-Za-z0-9_]", "_");
                default:
                    return value;
            }
        }

        /// <summary>
        /// Book text → pages. Type "pages:lines:chars" (default 8 lines of 20 characters). Item value = the quoted, escaped lines
        /// separated by ", " – ready for new BookPageInfo(…). Words are not cut; an empty line in the text starts a new page.
        /// </summary>
        public static List<KeyValuePair<string, string>> BookPages(string text, TemplateField field)
        {
            string[] o = field.Options.Split(':');
            int linesPerPage = o.Length > 0 && int.TryParse(o[0], out int lp) && lp > 0 ? lp : 8;
            int chars = o.Length > 1 && int.TryParse(o[1], out int cp) && cp > 4 ? cp : 20;
            var pages = new List<List<string>>();
            var page = new List<string>();
            foreach (string paragraph in (text ?? string.Empty).Replace("\r\n", "\n").Split('\n'))
            {
                if (paragraph.Trim().Length == 0)
                {
                    if (page.Count > 0)
                    {
                        pages.Add(page);
                        page = new List<string>();
                    }
                    continue;
                }
                foreach (string line in Wrap(paragraph.Trim(), chars))
                {
                    if (page.Count == linesPerPage)
                    {
                        pages.Add(page);
                        page = new List<string>();
                    }
                    page.Add(line);
                }
            }
            if (page.Count > 0)
            {
                pages.Add(page);
            }
            return pages.Select((p, i) => new KeyValuePair<string, string>((i + 1).ToString(CultureInfo.InvariantCulture),
                string.Join(", ", p.Select(l => "\"" + ApplyFilter(l, "cs") + "\"")))).ToList();
        }

        private static IEnumerable<string> Wrap(string text, int width)
        {
            var line = new StringBuilder();
            foreach (string word in text.Split(' ').Where(w => w.Length > 0))
            {
                string w = word;
                while (w.Length > width)
                {
                    if (line.Length > 0)
                    {
                        yield return line.ToString();
                        line.Clear();
                    }
                    yield return w.Substring(0, width);
                    w = w.Substring(width);
                }
                if (line.Length > 0 && line.Length + 1 + w.Length > width)
                {
                    yield return line.ToString();
                    line.Clear();
                }
                if (line.Length > 0)
                {
                    line.Append(' ');
                }
                line.Append(w);
            }
            if (line.Length > 0)
            {
                yield return line.ToString();
            }
        }
    }
}

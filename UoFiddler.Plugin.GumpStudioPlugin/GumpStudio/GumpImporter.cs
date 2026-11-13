using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>
    /// Reads an existing ServUO / RunUO / ModernUO gump script and rebuilds the gump.
    /// The constructor is run by a small interpreter: variables (int x = 20; y += 25;), arrays, for / foreach / while loops,
    /// if / else, switch, helper methods of the gump class (also with return value), String.Format and $"…" strings.
    /// Values that only exist in the game (from.Name, Utility.Random …) become placeholders like {from.Name} (texts) or 0 (numbers).
    /// </summary>
    public static class GumpImporter
    {
        /// <summary>Gump classes in the file (classes deriving from …Gump; if there are none, all classes).</summary>
        public static List<string> FindGumpClasses(string code)
        {
            code = StripComments(code ?? string.Empty);
            var gumps = Regex.Matches(code, @"\bclass\s+(\w+)\s*(?:<[^>]*>)?\s*:\s*([\w.<>]+)")
                .Cast<Match>().Where(m => m.Groups[2].Value.EndsWith("Gump", StringComparison.Ordinal) || m.Groups[2].Value.EndsWith("Gump>", StringComparison.Ordinal))
                .Select(m => m.Groups[1].Value).Distinct().ToList();
            if (gumps.Count == 0)
            {
                gumps = Regex.Matches(code, @"\bclass\s+(\w+)").Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToList();
            }
            return gumps;
        }

        public static GumpDesign Import(string code, List<string> warnings) => Import(code, warnings, null, null);

        /// <param name="className">Gump class to read; null = the first gump class (or loose Add… lines without a class).</param>
        /// <param name="resolveClass">Returns the code of another class (e.g. PropsConfig) whose constants the gump uses; may be null.</param>
        public static GumpDesign Import(string code, List<string> warnings, string className, Func<string, string> resolveClass)
        {
            code = code ?? string.Empty;
            if (className == null)
            {
                className = FindGumpClasses(code).FirstOrDefault();
            }
            var run = new Interpreter(code, className, warnings ?? new List<string>(), resolveClass, null, 0);
            run.Run();
            GumpDesign d = run.Design;
            if (d.Elements.Count > 0)
            {
                d.CanvasWidth = Math.Max(640, Math.Min(4000, d.Elements.Max(e => e.X + Math.Max(e.Width, 50)) + 40));
                d.CanvasHeight = Math.Max(480, Math.Min(4000, d.Elements.Max(e => e.Y + Math.Max(e.Height, 50)) + 40));
            }
            return d;
        }

        /// <summary>
        /// Finds classes in the scripts around a file: from its folder up to the "Scripts" folder (at most 6 levels).
        /// The index is built on the first request.
        /// </summary>
        public static Func<string, string> FolderResolver(string file)
        {
            Dictionary<string, string> index = null;
            return name =>
            {
                if (index == null)
                {
                    index = new Dictionary<string, string>(StringComparer.Ordinal);
                    string dir = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(file));
                    string root = dir;
                    for (string d = dir; d != null && root.Length > 3; d = System.IO.Path.GetDirectoryName(d))
                    {
                        if (string.Equals(System.IO.Path.GetFileName(d), "Scripts", StringComparison.OrdinalIgnoreCase))
                        {
                            root = d;
                            break;
                        }
                        if (dir.Count(c => c == System.IO.Path.DirectorySeparatorChar) - d.Count(c => c == System.IO.Path.DirectorySeparatorChar) >= 6)
                        {
                            break;
                        }
                    }
                    int count = 0;
                    foreach (string f in System.IO.Directory.EnumerateFiles(root, "*.cs", System.IO.SearchOption.AllDirectories))
                    {
                        if (++count > 30000)
                        {
                            break;
                        }
                        try
                        {
                            foreach (Match m in Regex.Matches(System.IO.File.ReadAllText(f), @"\b(?:class|struct|enum)\s+(\w+)"))
                            {
                                if (!index.ContainsKey(m.Groups[1].Value))
                                {
                                    index[m.Groups[1].Value] = f;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // unreadable file
                        }
                    }
                }
                return index.TryGetValue(name, out string path) ? System.IO.File.ReadAllText(path) : null;
            };
        }

        /// <summary>Finds classes in all scripts below a folder (one index for many imports, e.g. the gump gallery).</summary>
        public static Func<string, string> RootResolver(string folder)
        {
            Dictionary<string, string> index = null;
            object sync = new object();
            return name =>
            {
                lock (sync)
                {
                    if (index == null)
                    {
                        index = new Dictionary<string, string>(StringComparer.Ordinal);
                        int count = 0;
                        foreach (string f in System.IO.Directory.EnumerateFiles(folder, "*.cs", System.IO.SearchOption.AllDirectories))
                        {
                            if (++count > 30000)
                            {
                                break;
                            }
                            try
                            {
                                foreach (Match m in Regex.Matches(System.IO.File.ReadAllText(f), @"\b(?:class|struct|enum)\s+(\w+)"))
                                {
                                    if (!index.ContainsKey(m.Groups[1].Value))
                                    {
                                        index[m.Groups[1].Value] = f;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // unreadable file
                            }
                        }
                    }
                    return index.TryGetValue(name, out string path) ? System.IO.File.ReadAllText(path) : null;
                }
            };
        }

        /// <summary>Removes /* */ comments and #region / #if lines (line comments stay, they name elements).</summary>
        private static string StripComments(string code)
        {
            var sb = new StringBuilder(code.Length);
            for (int i = 0; i < code.Length; i++)
            {
                char c = code[i];
                if (c == '"' || c == '\'')
                {
                    int end = SkipString(code, i);
                    sb.Append(code, i, end - i);
                    i = end - 1;
                    continue;
                }
                if (c == '/' && i + 1 < code.Length && code[i + 1] == '/')
                {
                    int end = code.IndexOf('\n', i);
                    end = end < 0 ? code.Length : end;
                    sb.Append(code, i, end - i);
                    i = end - 1;
                    continue;
                }
                if (c == '/' && i + 1 < code.Length && code[i + 1] == '*')
                {
                    int end = code.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    end = end < 0 ? code.Length : end + 2;
                    // same length: positions stay the same as in the file (needed to write changes back)
                    foreach (char x in code.Substring(i, end - i))
                    {
                        sb.Append(x == '\n' || x == '\r' ? x : ' ');
                    }
                    i = end - 1;
                    continue;
                }
                sb.Append(c);
            }
            return Regex.Replace(sb.ToString(), @"^[ \t]*#[^\r\n]*", m => new string(' ', m.Length), RegexOptions.Multiline);
        }

        /// <summary>Index after the string / char literal that starts at i (handles @"", $"", $@"" and escapes).</summary>
        private static int SkipString(string s, int i)
        {
            bool verbatim = false;
            int start = i;
            // prefixes $ and @ are before the quote
            if (start > 0 && s[start - 1] == '@')
            {
                verbatim = true;
            }
            if (start > 1 && s[start - 2] == '@' && s[start - 1] == '$')
            {
                verbatim = true;
            }
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

        #region Statement tree

        private sealed class Node
        {
            public string Text = string.Empty;   // statement without ';' or the header before '{'
            public int Start = -1;                // position of Text in the file (-1 = made up)
            public List<Node> Block;             // body in { } (null = simple statement)
            public int Line;
            public string Comment;               // // comment at the end of the line
            public override string ToString() => Text;
        }

        private sealed class Parser
        {
            private readonly string _s;
            private readonly List<int> _lineStarts = new List<int> { 0 };
            private int _p;

            public Parser(string s)
            {
                _s = s;
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == '\n')
                    {
                        _lineStarts.Add(i + 1);
                    }
                }
            }

            private int LineAt(int pos)
            {
                int idx = _lineStarts.BinarySearch(pos);
                return (idx >= 0 ? idx : ~idx - 1) + 1;
            }

            public List<Node> ParseAll()
            {
                List<Node> list = ParseBlock();
                return list;
            }

            private List<Node> ParseBlock()
            {
                var list = new List<Node>();
                while (_p < _s.Length)
                {
                    SkipSpace(list);
                    if (_p >= _s.Length)
                    {
                        break;
                    }
                    if (_s[_p] == '}')
                    {
                        _p++;
                        return list;
                    }
                    if (_s[_p] == ';')
                    {
                        _p++;
                        continue;
                    }
                    int start = _p;
                    var text = new StringBuilder();
                    int depth = 0;
                    bool done = false;
                    while (_p < _s.Length && !done)
                    {
                        char c = _s[_p];
                        if (c == '"' || c == '\'')
                        {
                            int end = SkipString(_s, _p);
                            text.Append(_s, _p, end - _p);
                            _p = end;
                            continue;
                        }
                        if (c == '/' && _p + 1 < _s.Length && _s[_p + 1] == '/')
                        {
                            int end = _s.IndexOf('\n', _p);
                            end = end < 0 ? _s.Length : end;
                            text.Append(' ', end - _p);   // same length: positions stay right
                            _p = end;
                            continue;
                        }
                        if (c == '(' || c == '[')
                        {
                            depth++;
                        }
                        else if (c == ')' || c == ']')
                        {
                            depth--;
                        }
                        else if (depth <= 0 && c == ';')
                        {
                            _p++;
                            var node = new Node { Text = text.ToString().Trim(), Line = LineAt(start), Start = start };
                            node.Comment = TrailingComment();
                            list.Add(node);
                            done = true;
                            continue;
                        }
                        else if (depth <= 0 && c == '}')
                        {
                            // statement without ';' before the closing brace
                            if (text.ToString().Trim().Length > 0)
                            {
                                list.Add(new Node { Text = text.ToString().Trim(), Line = LineAt(start), Start = start });
                            }
                            done = true;
                            continue;
                        }
                        else if (c == '{')
                        {
                            string head = text.ToString().Trim();
                            if (depth > 0 || IsExpressionBrace(head))
                            {
                                // array / object initializer or lambda: part of the expression
                                int end = MatchBrace(_p);
                                text.Append(_s, _p, end - _p);
                                _p = end;
                                continue;
                            }
                            _p++;
                            var node = new Node { Text = head, Line = LineAt(start), Start = start };
                            node.Block = ParseBlock();
                            list.Add(node);
                            done = true;
                            continue;
                        }
                        text.Append(c);
                        _p++;
                    }
                    if (!done && text.ToString().Trim().Length > 0)
                    {
                        list.Add(new Node { Text = text.ToString().Trim(), Line = LineAt(start), Start = start });
                    }
                }
                return list;
            }

            private static bool IsExpressionBrace(string head)
            {
                if (head.EndsWith("=", StringComparison.Ordinal) || head.EndsWith("=>", StringComparison.Ordinal) || Regex.IsMatch(head, @"\breturn$"))
                {
                    return true;
                }
                // new[] { … }, new string[] { … }, new List<int> { … }, new Foo(1) { … } – but not "public new void Foo()"
                return Regex.IsMatch(head, @"\bnew\b[^;(){}=]*(\([^()]*\))?\s*$") && !Regex.IsMatch(head, @"\b(public|private|protected|internal|static)\s+new\b");
            }

            private int MatchBrace(int p)
            {
                int depth = 0;
                for (int i = p; i < _s.Length; i++)
                {
                    char c = _s[i];
                    if (c == '"' || c == '\'')
                    {
                        i = SkipString(_s, i) - 1;
                        continue;
                    }
                    if (c == '{')
                    {
                        depth++;
                    }
                    else if (c == '}' && --depth == 0)
                    {
                        return i + 1;
                    }
                }
                return _s.Length;
            }

            private string TrailingComment()
            {
                Match m = Regex.Match(_s.Substring(_p, Math.Min(400, _s.Length - _p)), @"^[ \t]*//[ \t]*([^\r\n]*)");
                if (!m.Success)
                {
                    return null;
                }
                _p += m.Length;
                string c = m.Groups[1].Value.Trim();
                return c.Length == 0 ? null : c;
            }

            private void SkipSpace(List<Node> list)
            {
                while (_p < _s.Length)
                {
                    if (char.IsWhiteSpace(_s[_p]))
                    {
                        _p++;
                    }
                    else if (_s[_p] == '/' && _p + 1 < _s.Length && _s[_p + 1] == '/')
                    {
                        int end = _s.IndexOf('\n', _p);
                        _p = end < 0 ? _s.Length : end;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        #endregion

        #region Values

        /// <summary>A value that only exists in the running game (e.g. from.Name).</summary>
        private sealed class Unknown
        {
            public readonly string Text;
            public Unknown(string text) => Text = text.Trim();
            public override string ToString() => "{" + Text + "}";
        }

        private sealed class Method
        {
            public string Name;
            public List<string> Params;
            public List<object> Defaults;
            public List<Node> Body;
            public string ExpressionBody;
            public int Adds = -1;
        }

        /// <summary>An object of a class from the same file (new DoorInfo(typeof(MetalDoor), 0x675)) with its fields.</summary>
        private sealed class ObjectValue
        {
            public string Type;
            public Dictionary<string, object> Fields = new Dictionary<string, object>(StringComparer.Ordinal);
            public override string ToString() => "{" + Type + "}";
        }

        private sealed class FlowException : Exception
        {
        }

        #endregion

        private sealed class Interpreter
        {
            private const int MaxSteps = 400000;
            private const int MaxLoop = 2000;
            private const int MaxElements = 5000;
            private const int SampleCount = 3;
            private readonly Dictionary<int, int> _sourceCount = new Dictionary<int, int>();
            private int _helperDepth;
            private int _quiet;

            private readonly string _code;
            private readonly string _className;
            private readonly List<string> _warnings;
            private readonly HashSet<string> _warned = new HashSet<string>();
            private readonly Dictionary<string, object> _globals = new Dictionary<string, object>(StringComparer.Ordinal);
            private readonly HashSet<string> _constNames = new HashSet<string>(StringComparer.Ordinal);
            private readonly Dictionary<string, List<Method>> _methods = new Dictionary<string, List<Method>>(StringComparer.Ordinal);
            private readonly Stack<Dictionary<string, object>> _scopes = new Stack<Dictionary<string, object>>();
            private int _steps;
            private int _line;
            private int _page;
            private int _group;
            private GumpElement _last;
            private string _flow;          // "break", "continue", "return"
            private object _return;

            public GumpDesign Design { get; } = new GumpDesign { Elements = new List<GumpElement>() };

            private readonly Func<string, string> _resolve;
            private readonly Dictionary<string, Dictionary<string, object>> _classCache;
            private readonly int _depth;

            public Interpreter(string code, string className, List<string> warnings, Func<string, string> resolve, Dictionary<string, Dictionary<string, object>> classCache, int depth)
            {
                _code = StripComments(code);
                _className = className;
                _warnings = warnings;
                _resolve = resolve;
                _classCache = classCache ?? new Dictionary<string, Dictionary<string, object>>(StringComparer.Ordinal);
                _depth = depth;
            }

            private void Warn(string text, string key = null)
            {
                if (_quiet > 0)
                {
                    return;
                }
                key = key ?? (_line + ":" + text);
                if (_warnings.Count < 500 && _warned.Add(key))
                {
                    _warnings.Add(text);
                }
            }

            private Dictionary<string, object> Scope => _scopes.Count > 0 ? _scopes.Peek() : _globals;

            public void Run()
            {
                List<Node> nodes = new Parser(_code).ParseAll();
                _allNodes = nodes;
                Match ns = Regex.Match(_code, @"\bnamespace\s+([\w.]+)");
                if (ns.Success)
                {
                    Design.Namespace = ns.Groups[1].Value;
                }
                if (_className != null)
                {
                    Design.ClassName = _className;
                }
                Match cmd = Regex.Match(_code, @"CommandSystem\.Register\s*\(\s*""(\w+)""\s*,\s*AccessLevel\.(\w+)");
                Design.CloseExisting = Regex.IsMatch(_code, @"\bSingleton\s*=>\s*true") || Regex.IsMatch(_code, @"CloseGump\s*\(\s*typeof\s*\(\s*" + Regex.Escape(_className ?? "\u0001") + @"\s*\)");
                if (cmd.Success)
                {
                    Design.Command = cmd.Groups[1].Value;
                    Design.CommandAccess = cmd.Groups[2].Value;
                }
                Node cls = _className == null ? null : FindClass(nodes, _className);
                try
                {
                    if (cls == null)
                    {
                        // loose lines (e.g. from the clipboard) or a file without the class
                        if (!nodes.Any(n => n.Block != null && Regex.IsMatch(n.Text, @"\b(class|namespace)\b")))
                        {
                            _scopes.Push(new Dictionary<string, object>(StringComparer.Ordinal));
                            ExecList(nodes);
                        }
                        return;
                    }
                    RunClass(cls);
                }
                catch (FlowException)
                {
                    Warn("The script is too long or has an endless loop – stopped (the part read so far is shown).", "steps");
                }
            }

            private static Node FindClass(List<Node> nodes, string name)
            {
                foreach (Node n in nodes)
                {
                    if (n.Block == null)
                    {
                        continue;
                    }
                    if (Regex.IsMatch(n.Text, @"\bclass\s+" + Regex.Escape(name) + @"\b"))
                    {
                        return n;
                    }
                    Node inner = FindClass(n.Block, name);
                    if (inner != null)
                    {
                        return inner;
                    }
                }
                return null;
            }

            private static readonly Regex MethodHead = new Regex(
                @"^(?:(?:public|private|protected|internal|static|override|virtual|sealed|new|async|unsafe|extern|partial)\s+)*([\w.<>\[\],?]+(?:\s*\[\])?)\s+(\w+)\s*\((.*?)\)\s*(?:=>\s*(.*))?$",
                RegexOptions.Singleline);

            private readonly List<Node> _ctors = new List<Node>();
            private List<Node> _allNodes;

            /// <summary>
            /// Creates an object of a class written in the same file: fields with their start values, then the constructor.
            /// Returns null when the class is not in the file.
            /// </summary>
            private ObjectValue CreateObject(string type, List<object> args)
            {
                if (_allNodes == null || type == null || _scopes.Count > 40)
                {
                    return null;
                }
                string name = type.Contains(".") ? type.Substring(type.LastIndexOf('.') + 1) : type;
                Node cls = FindClass(_allNodes, name);
                if (cls?.Block == null || name == _className)
                {
                    return null;
                }
                var obj = new ObjectValue { Type = name };
                _quiet++;
                _scopes.Push(obj.Fields);
                try
                {
                    foreach (Node n in cls.Block.Where(b => b.Block == null))
                    {
                        string t = StripAttributes(n.Text);
                        if (!Regex.IsMatch(t, @"\bstatic\b") && !Regex.IsMatch(t, @"\(.*\)\s*$|=>"))
                        {
                            ExecSimple(n, t, false);
                        }
                    }
                    Node ctor = cls.Block.FirstOrDefault(b => b.Block != null
                        && Regex.IsMatch(StripAttributes(b.Text), @"^(?:(?:public|private|protected|internal)\s+)*" + Regex.Escape(name) + @"\s*\(")
                        && Params(CtorHead(b).Groups[1].Value).Count() == args.Count);
                    if (ctor != null)
                    {
                        var scope = new Dictionary<string, object>(StringComparer.Ordinal);
                        List<(string Type, string Name, string Default)> ps = Params(CtorHead(ctor).Groups[1].Value).ToList();
                        for (int k = 0; k < ps.Count; k++)
                        {
                            scope[ps[k].Name] = args[k];
                        }
                        _scopes.Push(scope);
                        int line = _line;
                        try
                        {
                            ExecList(ctor.Block);
                        }
                        finally
                        {
                            _flow = null;
                            _return = null;
                            _scopes.Pop();
                            _line = line;
                        }
                    }
                }
                finally
                {
                    _scopes.Pop();
                    _quiet--;
                }
                return obj;
            }

            /// <summary>Fields, constants and methods of the class (twice, so fields may use constants declared further down).</summary>
            private void LoadMembers(Node cls)
            {
                for (int pass = 0; pass < 2; pass++)
                {
                    foreach (Node n in cls.Block)
                    {
                        if (pass == 1 && (n.Block != null || n.Text.Contains("=>")))
                        {
                            continue;
                        }
                        _line = n.Line;
                        string t = StripAttributes(n.Text);
                        Match ctor = Regex.Match(t, @"^(?:(?:public|private|protected|internal|static)\s+)*" + Regex.Escape(_className) + @"\s*\(", RegexOptions.Singleline);
                        if (ctor.Success && n.Block != null)
                        {
                            if (pass == 0 && !t.Contains("static"))
                            {
                                _ctors.Add(n);
                            }
                            continue;
                        }
                        if (ctor.Success && n.Block == null && t.Contains("=>"))
                        {
                            // expression bodied constructor: MyGump(Account a) : base(30, 30) => _acct = a;
                            if (pass == 0)
                            {
                                int close = MatchParen(t, t.IndexOf('('));
                                int arrow = close < 0 ? -1 : t.IndexOf("=>", close, StringComparison.Ordinal);
                                if (arrow > 0)
                                {
                                    _ctors.Add(new Node
                                    {
                                        Text = t.Substring(0, arrow).Trim(), Line = n.Line,
                                        Block = new List<Node> { new Node { Text = t.Substring(arrow + 2).Trim(), Line = n.Line } }
                                    });
                                }
                            }
                            continue;
                        }
                        if (Regex.IsMatch(t, @"\b(class|struct|enum|interface|delegate)\b"))
                        {
                            continue;
                        }
                        Match mh = MethodHead.Match(t);
                        if (mh.Success && (n.Block != null || mh.Groups[4].Success) && mh.Groups[1].Value != "new" && mh.Groups[1].Value != "return")
                        {
                            if (pass == 0)
                            {
                                AddMethod(mh.Groups[2].Value, mh.Groups[3].Value, n.Block, mh.Groups[4].Success ? mh.Groups[4].Value : null);
                            }
                            continue;
                        }
                        if (n.Block != null)
                        {
                            continue;   // property, event …
                        }
                        if (Regex.IsMatch(t, @"\(.*\)\s*$") && !t.Contains("="))
                        {
                            continue;   // abstract / extern method
                        }
                        try
                        {
                            ExecSimple(n, t, true);
                        }
                        catch (FlowException)
                        {
                            throw;
                        }
                        catch (Exception)
                        {
                            // field we do not understand
                        }
                    }
                }
            }

            private static string StripAttributes(string text) => Regex.Replace(text, @"^(\s*\[[^\]]*\])+\s*", string.Empty);

            private static Match CtorHead(Node ctor) => Regex.Match(StripAttributes(ctor.Text), @"\((.*?)\)\s*(?::\s*(base|this)\s*\((.*)\))?\s*$", RegexOptions.Singleline);

            private Node FindCtor(int argCount, Node exclude) =>
                _ctors.FirstOrDefault(c => c != exclude && Params(CtorHead(c).Groups[1].Value).Count() == argCount);

            /// <summary>Number of Add… calls the constructor makes (with called methods and : this(…) constructors).</summary>
            private int Score(Node ctor, int depth)
            {
                int score = AddsWithCalls(ctor.Block, new HashSet<string>());
                Match head = CtorHead(ctor);
                if (depth < 4 && head.Groups[2].Value == "this")
                {
                    Node target = FindCtor(SplitArgs(head.Groups[3].Value).Count, ctor);
                    if (target != null)
                    {
                        score += Score(target, depth + 1);
                    }
                }
                return score;
            }

            private int AddsWithCalls(List<Node> nodes, HashSet<string> seen)
            {
                int score = 0;
                foreach (Node n in nodes ?? new List<Node>())
                {
                    score += Regex.Matches(n.Text, @"\bAdd\w+\s*\(").Count;
                    foreach (Match m in Regex.Matches(n.Text, @"\b(\w+)\s*\("))
                    {
                        if (_methods.TryGetValue(m.Groups[1].Value, out List<Method> list) && seen.Add(m.Groups[1].Value))
                        {
                            foreach (Method me in list)
                            {
                                score += AddsWithCalls(me.Body, seen) + (me.ExpressionBody != null ? Regex.Matches(me.ExpressionBody, @"\bAdd\w+\s*\(").Count : 0);
                            }
                        }
                    }
                    score += AddsWithCalls(n.Block, seen);
                }
                return score;
            }

            private void RunClass(Node cls)
            {
                LoadMembers(cls);
                bool primary = false;
                if (_ctors.Count == 0)
                {
                    // C# 12 primary constructor: class MyGump(Mobile from) : Gump(50, 50)
                    Match pc = Regex.Match(StripAttributes(cls.Text), @"\bclass\s+" + Regex.Escape(_className) + @"\s*\(([^)]*)\)\s*:\s*[\w.<>]+\s*(?:\((.*)\))?", RegexOptions.Singleline);
                    if (pc.Success)
                    {
                        _ctors.Add(new Node
                        {
                            Text = $"{_className}({pc.Groups[1].Value})" + (pc.Groups[2].Success ? $" : base({pc.Groups[2].Value})" : string.Empty),
                            Line = cls.Line, Block = new List<Node>()
                        });
                        primary = true;
                    }
                }
                if (_ctors.Count == 0)
                {
                    if (_methods.ContainsKey("BuildLayout"))
                    {
                        Call("BuildLayout", new List<object> { new Unknown("builder") });
                        return;
                    }
                    Warn($"No constructor of {_className} found.", "noctor");
                    return;
                }
                Node best = _ctors.Select((c, i) => (c, i)).OrderByDescending(x => Score(x.c, 0)).ThenBy(x => x.i).First().c;
                if (_ctors.Count > 1)
                {
                    Warn($"{_className} has {_ctors.Count} constructors – the one in line {best.Line} is used.", "ctors");
                }
                // values from a call in this file: new MyGump(from, 300, 200)
                List<object> callArgs = null;
                int paramCount = Params(CtorHead(best).Groups[1].Value).Count();
                foreach (Match call in Regex.Matches(_code, @"\bnew\s+" + Regex.Escape(_className) + @"\s*\("))
                {
                    int open = _code.IndexOf('(', call.Index);
                    int close = MatchParen(_code, open);
                    List<string> a = close > open ? SplitArgs(_code.Substring(open + 1, close - open - 1)) : null;
                    if (a != null && a.Count == paramCount && paramCount > 0)
                    {
                        _scopes.Push(new Dictionary<string, object>(StringComparer.Ordinal));
                        callArgs = a.Select(x => Eval(x)).ToList();
                        _scopes.Pop();
                        break;
                    }
                }
                RunCtor(best, callArgs, 0, primary);
                // ModernUO: the layout is built in BuildLayout(ref DynamicGumpBuilder builder)
                if (_methods.ContainsKey("BuildLayout"))
                {
                    _line = best.Line;
                    Call("BuildLayout", new List<object> { new Unknown("builder") });
                }
            }

            private void RunCtor(Node ctor, List<object> args, int depth, bool keepParameters = false)
            {
                var scope = new Dictionary<string, object>(StringComparer.Ordinal);
                Match head = CtorHead(ctor);
                _line = ctor.Line;
                List<(string Type, string Name, string Default)> ps = Params(head.Groups[1].Value).ToList();
                for (int k = 0; k < ps.Count; k++)
                {
                    object v = args != null && k < args.Count ? args[k] : null;
                    if (v == null || v is Unknown)
                    {
                        v = ps[k].Default != null ? Eval(ps[k].Default) : Guess(ps[k].Type, ps[k].Name);
                    }
                    scope[ps[k].Name] = v;
                }
                _scopes.Push(scope);
                if (head.Groups[2].Value == "base")
                {
                    List<string> a = SplitArgs(head.Groups[3].Value);
                    if (a.Count >= 2)
                    {
                        Design.X = Int(a[0], 50);
                        Design.Y = Int(a[1], 50);
                    }
                }
                else if (head.Groups[2].Value == "this" && depth < 5)
                {
                    List<string> a = SplitArgs(head.Groups[3].Value);
                    Node target = FindCtor(a.Count, ctor);
                    if (target != null)
                    {
                        RunCtor(target, a.Select(x => Eval(x)).ToList(), depth + 1);
                    }
                }
                ExecList(ctor.Block);
                _flow = null;
                _scopes.Pop();
                if (keepParameters)
                {
                    // parameters of a primary constructor are visible in the whole class
                    foreach (KeyValuePair<string, object> kv in scope)
                    {
                        _globals[kv.Key] = kv.Value;
                    }
                }
            }

            /// <summary>Value for a constructor parameter that is only known in the game (sizes get a usable default).</summary>
            private object Guess(string type, string name)
            {
                string l = name.ToLowerInvariant();
                if (Regex.IsMatch(type, @"^(int|short|long|uint|ushort)$"))
                {
                    int v = l.Contains("width") ? 400 : l.Contains("height") ? 300 : l.EndsWith("page", StringComparison.Ordinal) || l == "index" || l == "idx" || l == "start" ? 0 : -1;
                    if (v >= 0)
                    {
                        Warn($"parameter {name} is only known in the game – {v} is used.", "param:" + name);
                        return v;
                    }
                }
                return new Unknown(name);
            }

            /// <summary>Static fields / enum values of another class (same file, or a script next to the imported one).</summary>
            private Dictionary<string, object> ClassFields(string name)
            {
                if (name == _className)
                {
                    return _globals;
                }
                if (_classCache.TryGetValue(name, out Dictionary<string, object> cached))
                {
                    return cached;
                }
                _classCache[name] = null;
                string code = Regex.IsMatch(_code, @"\b(class|struct|enum)\s+" + Regex.Escape(name) + @"\b") ? _code : null;
                if (code == null && _resolve != null && _depth < 3)
                {
                    try
                    {
                        code = _resolve(name);
                    }
                    catch (Exception)
                    {
                        code = null;
                    }
                }
                if (code == null)
                {
                    return null;
                }
                var fields = new Dictionary<string, object>(StringComparer.Ordinal);
                Match en = Regex.Match(StripComments(code), @"\benum\s+" + Regex.Escape(name) + @"\s*(?::\s*\w+)?\s*\{([^}]*)\}");
                if (en.Success)
                {
                    int next = 0;
                    foreach (string item in SplitArgs(Regex.Replace(en.Groups[1].Value, @"//[^\n]*", string.Empty)))
                    {
                        Match m = Regex.Match(item, @"^(?:\[[^\]]*\]\s*)*(\w+)\s*(?:=\s*(.+))?$", RegexOptions.Singleline);
                        if (!m.Success)
                        {
                            continue;
                        }
                        if (m.Groups[2].Success)
                        {
                            _scopes.Push(new Dictionary<string, object>(fields, StringComparer.Ordinal));
                            object v = Eval(m.Groups[2].Value);
                            _scopes.Pop();
                            next = v is int iv ? iv : next;
                        }
                        fields[m.Groups[1].Value] = next++;
                    }
                }
                else
                {
                    var sub = new Interpreter(code, name, new List<string>(), _resolve, _classCache, _depth + 1);
                    fields = sub.FieldsOnly();
                }
                _classCache[name] = fields;
                return fields;
            }

            private Dictionary<string, object> FieldsOnly()
            {
                try
                {
                    Node cls = FindClass(new Parser(_code).ParseAll(), _className);
                    if (cls != null)
                    {
                        LoadMembers(cls);
                    }
                }
                catch (Exception)
                {
                    // partial result
                }
                return _globals;
            }

            private static IEnumerable<(string Type, string Name, string Default)> Params(string list)
            {
                foreach (string p in SplitArgs(list))
                {
                    Match m = Regex.Match(p, @"^(?:(?:ref|out|in|params|this)\s+)*([\w.<>\[\],?]+)\s+(\w+)\s*(?:=\s*(.+))?$", RegexOptions.Singleline);
                    if (m.Success)
                    {
                        yield return (m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Success ? m.Groups[3].Value : null);
                    }
                }
            }

            private void AddMethod(string name, string paramList, List<Node> body, string exprBody)
            {
                var m = new Method { Name = name, Params = new List<string>(), Defaults = new List<object>(), Body = body, ExpressionBody = exprBody };
                foreach ((string type, string pname, string def) in Params(paramList))
                {
                    m.Params.Add(pname);
                    m.Defaults.Add(def);
                }
                if (!_methods.TryGetValue(name, out List<Method> list))
                {
                    _methods[name] = list = new List<Method>();
                }
                list.Add(m);
            }

            private void Step()
            {
                if (++_steps > MaxSteps || Design.Elements.Count > MaxElements)
                {
                    throw new FlowException();
                }
            }

            #region Statements

            private void ExecList(List<Node> nodes)
            {
                for (int i = 0; i < nodes.Count && _flow == null; i++)
                {
                    try
                    {
                        Exec(nodes, ref i, nodes[i].Text, nodes[i].Block);
                    }
                    catch (FlowException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Warn($"line {nodes[Math.Min(i, nodes.Count - 1)].Line}: {Short(nodes[Math.Min(i, nodes.Count - 1)].Text)} – {(ex is ArgumentOutOfRangeException ? "too few arguments" : ex.Message)}, skipped.");
                    }
                }
            }

            private static bool Keyword(string text, string kw, out string inParens, out string rest)
            {
                inParens = rest = null;
                if (!Regex.IsMatch(text, @"^" + kw + @"\s*\("))
                {
                    return false;
                }
                int open = text.IndexOf('(');
                int close = MatchParen(text, open);
                if (close < 0)
                {
                    return false;
                }
                inParens = text.Substring(open + 1, close - open - 1);
                rest = text.Substring(close + 1).Trim();
                return true;
            }

            private static int MatchParen(string s, int open)
            {
                int depth = 0;
                for (int i = open; i < s.Length; i++)
                {
                    char c = s[i];
                    if (c == '"' || c == '\'')
                    {
                        i = SkipString(s, i) - 1;
                        continue;
                    }
                    if (c == '(')
                    {
                        depth++;
                    }
                    else if (c == ')' && --depth == 0)
                    {
                        return i;
                    }
                }
                return -1;
            }

            private static bool StartsWithWord(string text, string word) => Regex.IsMatch(text ?? string.Empty, @"^" + word + @"\b");

            private void Body(string rest, List<Node> block, Node origin)
            {
                if (!string.IsNullOrWhiteSpace(rest))
                {
                    var single = new List<Node>
                    {
                        new Node
                        {
                            Text = rest, Block = block, Line = origin.Line, Comment = origin.Comment,
                            Start = origin.Start >= 0 && origin.Text.EndsWith(rest, StringComparison.Ordinal) ? origin.Start + origin.Text.Length - rest.Length : -1
                        }
                    };
                    int j = 0;
                    Exec(single, ref j, rest, block);
                }
                else if (block != null)
                {
                    ExecList(block);
                }
            }

            private void Exec(List<Node> nodes, ref int i, string text, List<Node> block)
            {
                Step();
                Node n = nodes[i];
                _line = n.Line;
                text = text.Trim();
                if (text.Length == 0)
                {
                    if (block != null)
                    {
                        ExecList(block);
                    }
                    return;
                }
                if (Keyword(text, "if", out string cond, out string rest))
                {
                    object c = Eval(cond);
                    bool known = c is bool;
                    if (!known && IsExit(rest, block))
                    {
                        // guard clause (if (state == null) return;): the gump is built as if the check passed
                        c = false;
                        known = true;
                    }
                    if (!known)
                    {
                        Warn($"line {n.Line}: if ({Short(cond)}) is only known in the game – the \"if\" part is shown.");
                    }
                    if (!known || (bool)c)
                    {
                        Body(rest, block, n);
                        // skip the else chain
                        while (i + 1 < nodes.Count && StartsWithWord(nodes[i + 1].Text, "else"))
                        {
                            i++;
                            if (!Regex.IsMatch(nodes[i].Text, @"^else\s+if\b"))
                            {
                                break;
                            }
                        }
                    }
                    else if (i + 1 < nodes.Count && StartsWithWord(nodes[i + 1].Text, "else"))
                    {
                        i++;
                        Exec(nodes, ref i, nodes[i].Text.Trim().Substring(4), nodes[i].Block);
                    }
                    return;
                }
                if (StartsWithWord(text, "else"))
                {
                    return;   // else without if (already handled)
                }
                if (Keyword(text, "for", out string head, out rest))
                {
                    List<string> parts = SplitTop(head, ';');
                    if (parts.Count != 3)
                    {
                        return;
                    }
                    Match initType = Regex.Match(parts[0], @"^\s*(var|int|long|short|byte|uint|double)\s+");
                    List<string> inits = SplitArgs(parts[0]);
                    for (int k = 0; k < inits.Count; k++)
                    {
                        ExecSimple(n, k > 0 && initType.Success ? initType.Groups[1].Value + " " + inits[k] : inits[k], false);
                    }
                    int count = 0;
                    while (true)
                    {
                        object c = parts[1].Trim().Length == 0 ? true : Eval(parts[1]);
                        if (!(c is bool))
                        {
                            Warn($"line {n.Line}: for ({Short(head)}) – the end is only known in the game, one pass is shown.");
                            Body(rest, block, n);
                            break;
                        }
                        if (!(bool)c)
                        {
                            break;
                        }
                        if (++count > MaxLoop)
                        {
                            Warn($"line {n.Line}: loop stopped after {MaxLoop} passes.");
                            break;
                        }
                        Body(rest, block, n);
                        if (EndLoop())
                        {
                            break;
                        }
                        foreach (string it in SplitArgs(parts[2]))
                        {
                            ExecSimple(n, it, false);
                        }
                    }
                    return;
                }
                if (Keyword(text, "foreach", out head, out rest))
                {
                    Match m = Regex.Match(head, @"^\s*(?:[\w.<>\[\],?()\s]+\s+)?(\w+)\s+in\s+(.+)$", RegexOptions.Singleline);
                    if (!m.Success)
                    {
                        return;
                    }
                    object list = Eval(m.Groups[2].Value);
                    if (list is object[] arr)
                    {
                        foreach (object v in arr.Take(MaxLoop))
                        {
                            Scope[m.Groups[1].Value] = v;
                            Body(rest, block, n);
                            if (EndLoop())
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        Warn($"line {n.Line}: foreach over {Short(m.Groups[2].Value)} – only known in the game, one pass is shown.");
                        Scope[m.Groups[1].Value] = new Unknown(m.Groups[1].Value);
                        Body(rest, block, n);
                        EndLoop();
                    }
                    return;
                }
                if (Keyword(text, "while", out cond, out rest))
                {
                    int count = 0;
                    while (true)
                    {
                        object c = Eval(cond);
                        if (!(c is bool))
                        {
                            Warn($"line {n.Line}: while ({Short(cond)}) – only known in the game, one pass is shown.");
                            Body(rest, block, n);
                            EndLoop();
                            break;
                        }
                        if (!(bool)c || ++count > MaxLoop)
                        {
                            break;
                        }
                        Body(rest, block, n);
                        if (EndLoop())
                        {
                            break;
                        }
                    }
                    return;
                }
                if (StartsWithWord(text, "do"))
                {
                    Body(text.Substring(2), block, n);
                    EndLoop();
                    if (i + 1 < nodes.Count && StartsWithWord(nodes[i + 1].Text, "while"))
                    {
                        i++;
                    }
                    return;
                }
                if (Keyword(text, "switch", out cond, out rest))
                {
                    ExecSwitch(n, cond, block);
                    return;
                }
                if (Keyword(text, "using", out _, out rest) || Keyword(text, "lock", out _, out rest) || Keyword(text, "fixed", out _, out rest)
                    || Regex.IsMatch(text, @"^(try|finally|checked|unchecked|unsafe)$"))
                {
                    Body(rest, block, n);
                    return;
                }
                if (StartsWithWord(text, "catch"))
                {
                    return;
                }
                if (text == "break" || text == "continue")
                {
                    _flow = text;
                    return;
                }
                if (StartsWithWord(text, "return"))
                {
                    string expr = text.Substring(6).Trim();
                    _return = expr.Length > 0 ? Eval(expr) : null;
                    _flow = "return";
                    return;
                }
                if (StartsWithWord(text, "throw") || StartsWithWord(text, "goto") || StartsWithWord(text, "yield"))
                {
                    return;
                }
                if (block != null)
                {
                    // local function or unknown block
                    Match mh = MethodHead.Match(text);
                    if (mh.Success)
                    {
                        AddMethod(mh.Groups[2].Value, mh.Groups[3].Value, block, null);
                        return;
                    }
                    ExecList(block);
                    return;
                }
                ExecSimple(n, text, false);
            }

            /// <summary>Body that only leaves (return / throw / break / continue) – maybe with a message before it.</summary>
            private static bool IsExit(string rest, List<Node> block)
            {
                var texts = new List<string>();
                if (!string.IsNullOrWhiteSpace(rest))
                {
                    texts.Add(rest.Trim());
                }
                else if (block != null && block.All(b => b.Block == null))
                {
                    texts.AddRange(block.Select(b => b.Text.Trim()));
                }
                if (texts.Count == 0 || !Regex.IsMatch(texts.Last(), @"^(return|throw|break|continue)\b"))
                {
                    return false;
                }
                return !texts.Any(t => Regex.IsMatch(t, @"\bAdd\w+\s*\("));
            }

            /// <summary>True when break / return ends the loop (continue is consumed).</summary>
            private bool EndLoop()
            {
                if (_flow == "continue")
                {
                    _flow = null;
                    return false;
                }
                if (_flow == "break")
                {
                    _flow = null;
                    return true;
                }
                return _flow != null;
            }

            private void ExecSwitch(Node n, string expr, List<Node> block)
            {
                if (block == null)
                {
                    return;
                }
                object v = Eval(expr);
                if (v is Unknown)
                {
                    Warn($"line {n.Line}: switch ({Short(expr)}) – only known in the game, the first case is shown.");
                }
                int start = -1, def = -1;
                for (int i = 0; i < block.Count && start < 0; i++)
                {
                    foreach (string label in Labels(block[i].Text, out _))
                    {
                        if (label == null)
                        {
                            def = def < 0 ? i : def;
                        }
                        else if (v is Unknown || Equal(Eval(label), v))
                        {
                            start = i;
                            break;
                        }
                    }
                }
                start = start < 0 ? def : start;
                if (start < 0)
                {
                    return;
                }
                for (int i = start; i < block.Count && _flow == null; i++)
                {
                    Labels(block[i].Text, out string body);
                    Exec(block, ref i, body, block[i].Block);
                }
                if (_flow == "break")
                {
                    _flow = null;
                }
            }

            /// <summary>case labels at the start of a statement (null = default); body = the rest.</summary>
            private static List<string> Labels(string text, out string body)
            {
                var labels = new List<string>();
                body = text;
                while (true)
                {
                    Match m = Regex.Match(body, @"^\s*(?:case\s+((?:[^:""']|""(?:[^""\\]|\\.)*""|'(?:[^'\\]|\\.)*')+?)|default)\s*:(?!:)", RegexOptions.Singleline);
                    if (!m.Success)
                    {
                        return labels;
                    }
                    labels.Add(m.Groups[1].Success ? m.Groups[1].Value : null);
                    body = body.Substring(m.Length);
                }
            }

            private static readonly HashSet<string> Flags = new HashSet<string> { "Closable", "Dragable", "Disposable", "Resizable", "X", "Y" };

            private void ExecSimple(Node n, string text, bool classLevel)
            {
                Step();
                text = text.Trim();
                if (text.Length == 0 || StartsWithWord(text, "using") || StartsWithWord(text, "base") && !text.StartsWith("base.", StringComparison.Ordinal))
                {
                    return;
                }
                // declaration: [modifiers] type name [= expr][, name2 = expr2]
                // declaration with a tuple type: (Type Type, int BaseID)[][] _types = …;
                Match tdecl = Regex.Match(text, @"^((?:(?:static|readonly|private|public|protected|internal)\s+)*)\(((?:[^()]|\([^()]*\))*)\)((?:\s*\[[\s,]*\])*)\??\s+(\w+)\s*(?:=(?!=)\s*(.+))?$", RegexOptions.Singleline);
                if (tdecl.Success)
                {
                    List<string> names = SplitArgs(tdecl.Groups[2].Value).Select(x => Regex.Match(x.Trim(), @"\s(\w+)$") is Match nm && nm.Success ? nm.Groups[1].Value : null).ToList();
                    object value = tdecl.Groups[5].Success ? Eval(tdecl.Groups[5].Value) : null;
                    NameTuples(value, names);
                    (classLevel ? _globals : Scope)[tdecl.Groups[4].Value] = value;
                    return;
                }
                Match decl = Regex.Match(text, @"^((?:(?:const|static|readonly|private|public|protected|internal|volatile|new)\s+)*)((?:[\w.]+)(?:<[^=]*?>)?\??(?:\s*\[[\s,]*\])*)\s+(\w+)\s*(=(?!=)|,|$)", RegexOptions.Singleline);
                if (decl.Success && decl.Groups[2].Value != "return" && decl.Groups[2].Value != "new" && decl.Groups[2].Value != "await")
                {
                    bool isConst = decl.Groups[1].Value.Contains("const") || decl.Groups[1].Value.Contains("readonly");
                    string declarators = text.Substring(decl.Groups[1].Length + decl.Groups[2].Length).Trim();
                    foreach (string d in SplitArgs(declarators))
                    {
                        Match dm = Regex.Match(d, @"^(\w+)\s*(?:=\s*(.+))?$", RegexOptions.Singleline);
                        if (!dm.Success)
                        {
                            continue;
                        }
                        string name = dm.Groups[1].Value;
                        object value = dm.Groups[2].Success ? Eval(dm.Groups[2].Value) : Default(decl.Groups[2].Value);
                        (classLevel ? _globals : Scope)[name] = value;
                        if (isConst)
                        {
                            _constNames.Add(name);
                        }
                    }
                    return;
                }
                // x++ / ++x
                Match inc = Regex.Match(text, @"^(?:(\+\+|--)\s*([\w.]+)|([\w.]+)\s*(\+\+|--))$");
                if (inc.Success)
                {
                    string name = inc.Groups[2].Success ? inc.Groups[2].Value : inc.Groups[3].Value;
                    string op = inc.Groups[1].Success ? inc.Groups[1].Value : inc.Groups[4].Value;
                    Assign(name, Binary(op == "++" ? "+" : "-", Get(name), 1, name));
                    return;
                }
                // assignment
                Match asg = Regex.Match(text, @"^([\w.]+(?:\s*\[[^\]]*\])?)\s*(=|\+=|-=|\*=|/=|%=|\|=|&=|\?\?=)(?!=)\s*(.+)$", RegexOptions.Singleline);
                if (asg.Success)
                {
                    string target = asg.Groups[1].Value.Replace(" ", string.Empty);
                    object value = Eval(asg.Groups[3].Value);
                    string op = asg.Groups[2].Value;
                    if (op != "=")
                    {
                        object old = Get(target);
                        value = op == "??=" ? (old ?? value) : Binary(op.Substring(0, op.Length - 1), old, value, text);
                    }
                    Assign(target, value);
                    return;
                }
                // call
                Match call = Regex.Match(text, @"^(?:(this|base|\w+)\.)?(\w+)\s*(?:<[^>]*>)?\s*\(", RegexOptions.Singleline);
                if (call.Success)
                {
                    int open = text.IndexOf('(', call.Groups[2].Index + call.Groups[2].Length);
                    int close = MatchParen(text, open);
                    string owner = call.Groups[1].Success ? call.Groups[1].Value : null;
                    string name = call.Groups[2].Value;
                    if (owner != null && owner != "this" && owner != "base" && !GumpMethods.Contains(name))
                    {
                        // from.SendGump(…), list.Add(…): not a gump part (ModernUO: builder.AddButton(…) is one)
                        return;
                    }
                    if (close == text.Length - 1)
                    {
                        List<string> args = SplitArgs(text.Substring(open + 1, close - open - 1));
                        if (name.StartsWith("SetNo", StringComparison.Ordinal))
                        {
                            // ModernUO builder.SetNoClose() …
                            switch (name)
                            {
                                case "SetNoClose": Design.Closable = false; break;
                                case "SetNoMove": Design.Dragable = false; break;
                                case "SetNoDispose": Design.Disposable = false; break;
                            }
                            return;
                        }
                        if (name.StartsWith("Add", StringComparison.Ordinal) && !_methods.ContainsKey(name))
                        {
                            AddElement(n, name, args, text);
                            return;
                        }
                        if (_methods.ContainsKey(name))
                        {
                            Call(name, args.Select(a => Eval(a)).ToList());
                            return;
                        }
                        if (!classLevel && char.IsUpper(name[0]) && name != "Server")
                        {
                            Warn($"line {n.Line}: {Short(text)} – method not in this file, skipped.", "call:" + name);
                        }
                        return;
                    }
                }
                // anything else: evaluate (calls of helpers in expressions)
                if (Regex.IsMatch(text, @"\bAdd\w+\s*\("))
                {
                    Warn($"line {n.Line}: {Short(text)} – not understood, skipped.");
                }
            }

            private static object Default(string type)
            {
                switch (type.TrimEnd('?'))
                {
                    case "int": case "long": case "short": case "byte": case "uint": case "ushort": case "sbyte": case "ulong": return 0;
                    case "bool": return false;
                    case "double": case "float": case "decimal": return 0.0;
                    case "string": case "String": return null;
                    default: return null;
                }
            }

            private object Get(string name)
            {
                name = name.StartsWith("this.", StringComparison.Ordinal) ? name.Substring(5) : name;
                Match idx = Regex.Match(name, @"^(\w+)\[(.+)\]$");
                if (idx.Success)
                {
                    return Index(Get(idx.Groups[1].Value), Eval(idx.Groups[2].Value), name);
                }
                foreach (Dictionary<string, object> s in _scopes)
                {
                    if (s.TryGetValue(name, out object v))
                    {
                        return v;
                    }
                }
                if (_globals.TryGetValue(name, out object g))
                {
                    return g;
                }
                return new Unknown(name);
            }

            private void Assign(string name, object value)
            {
                name = name.StartsWith("this.", StringComparison.Ordinal) ? name.Substring(5) : name;
                Match idx = Regex.Match(name, @"^(\w+)\[(.+)\]$");
                if (idx.Success)
                {
                    if (Get(idx.Groups[1].Value) is object[] arr && Eval(idx.Groups[2].Value) is int i && i >= 0 && i < arr.Length)
                    {
                        arr[i] = value;
                    }
                    return;
                }
                if (Flags.Contains(name) && !ScopesHave(name))
                {
                    switch (name)
                    {
                        case "Closable": Design.Closable = !(value is bool b0) || b0; break;
                        case "Dragable": Design.Dragable = !(value is bool b1) || b1; break;
                        case "Disposable": Design.Disposable = !(value is bool b2) || b2; break;
                        case "Resizable": Design.Resizable = value is bool b3 && b3; break;
                        case "X": if (value is int x) { Design.X = x; } break;
                        default: if (value is int y) { Design.Y = y; } break;
                    }
                    return;
                }
                foreach (Dictionary<string, object> s in _scopes)
                {
                    if (s.ContainsKey(name))
                    {
                        s[name] = value;
                        return;
                    }
                }
                if (_globals.ContainsKey(name) || _scopes.Count == 0)
                {
                    _globals[name] = value;
                    return;
                }
                Scope[name] = value;
            }

            private bool ScopesHave(string name) => _scopes.Any(s => s.ContainsKey(name)) || _globals.ContainsKey(name);

            private object Call(string name, List<object> args)
            {
                List<Method> overloads = _methods[name];
                Method m = overloads.FirstOrDefault(o => o.Params.Count == args.Count)
                           ?? overloads.FirstOrDefault(o => o.Params.Count > args.Count && o.Defaults.Skip(args.Count).All(x => x != null))
                           ?? overloads[0];
                if (_scopes.Count > 40)
                {
                    return new Unknown(name + "(…)");
                }
                var scope = new Dictionary<string, object>(StringComparer.Ordinal);
                for (int i = 0; i < m.Params.Count; i++)
                {
                    scope[m.Params[i]] = i < args.Count ? args[i] : m.Defaults[i] is string def ? Eval(def) : new Unknown(m.Params[i]);
                }
                if (m.Adds < 0)
                {
                    m.Adds = AddsWithCalls(m.Body, new HashSet<string>()) + (m.ExpressionBody != null ? Regex.Matches(m.ExpressionBody, @"\bAdd\w+\s*\(").Count : 0);
                }
                bool quiet = m.Adds == 0;   // helpers without gump parts (BuildList …) give no hints
                bool helper = name != "BuildLayout";
                if (helper)
                {
                    _helperDepth++;
                }
                _scopes.Push(scope);
                int line = _line;
                object result = null;
                if (quiet)
                {
                    _quiet++;
                }
                try
                {
                    if (m.ExpressionBody != null)
                    {
                        if (Regex.IsMatch(m.ExpressionBody, @"^\s*(?:this\.)?Add\w+\s*\("))
                        {
                            ExecSimple(new Node { Text = m.ExpressionBody, Line = line }, m.ExpressionBody, false);
                        }
                        else
                        {
                            result = Eval(m.ExpressionBody);
                        }
                    }
                    else if (m.Body != null)
                    {
                        ExecList(m.Body);
                        result = _return;
                    }
                }
                finally
                {
                    if (quiet)
                    {
                        _quiet--;
                    }
                    if (helper)
                    {
                        _helperDepth--;
                    }
                    _scopes.Pop();
                    _flow = null;
                    _return = null;
                    _line = line;
                }
                return result;
            }

            #endregion

            #region Elements

            private int Int(string arg, int fallback = 0)
            {
                object v = Eval(arg);
                switch (v)
                {
                    case int i: return i;
                    case double dd: return (int)dd;
                    case bool b: return b ? 1 : 0;
                    case string s when int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int p): return p;
                }
                Warn($"line {_line}: {Short(arg)} is only known in the game – {fallback} is used.", "int:" + arg.Trim());
                return fallback;
            }

            private string Str(string arg)
            {
                object v = Eval(arg);
                return v == null ? string.Empty : Text(v);
            }

            private bool Bool(string arg)
            {
                object v = Eval(arg);
                return v is bool b ? b : v is int i && i != 0;
            }

            private static string A(List<string> a, int i, string def) => i < a.Count ? a[i] : def;

            private static readonly HashSet<string> GumpMethods = new HashSet<string>
            {
                "AddPage", "AddGroup", "AddTooltip", "AddBackground", "AddImage", "AddImageTiled", "AddSpriteImage", "AddAlphaRegion", "AddLabel",
                "AddLabelCropped", "AddHtml", "AddHtmlLocalized", "AddButton", "AddImageTiledButton", "AddCheck", "AddCheckbox", "AddRadio",
                "AddTextEntry", "AddTextEntryLimited", "AddItem", "AddItemProperty", "AddGumpIdOverride", "AddMasterGump",
                "SetNoClose", "SetNoMove", "SetNoDispose", "SetNoResize"
            };

            /// <summary>Named arguments (background: true) to their place; unknown names are dropped.</summary>
            private static List<string> Positional(string name, List<string> a)
            {
                if (!a.Any(x => Regex.IsMatch(x, @"^\w+\s*:(?!:)")))
                {
                    return a;
                }
                var pos = a.Where(x => !Regex.IsMatch(x, @"^\w+\s*:(?!:)")).ToList();
                foreach (string x in a.Where(x => Regex.IsMatch(x, @"^\w+\s*:(?!:)")))
                {
                    Match m = Regex.Match(x, @"^(\w+)\s*:\s*(.*)$", RegexOptions.Singleline);
                    int index = -1;
                    switch (m.Groups[1].Value)
                    {
                        case "background": index = name == "AddHtml" ? 5 : pos.Count >= 6 && name == "AddHtmlLocalized" ? pos.Count : 5; break;
                        case "scrollbar": index = name == "AddHtml" ? 6 : pos.Count; break;
                        case "hue": index = 3; break;
                        case "type": index = 5; break;
                        case "param": index = 6; break;
                    }
                    if (index < 0)
                    {
                        continue;
                    }
                    while (pos.Count < index)
                    {
                        pos.Add(index - pos.Count == 1 && (name == "AddButton" && index == 6) ? "0" : name == "AddButton" && pos.Count == 5 ? "GumpButtonType.Reply" : "false");
                    }
                    if (pos.Count == index)
                    {
                        pos.Add(m.Groups[2].Value);
                    }
                    else
                    {
                        pos[index] = m.Groups[2].Value;
                    }
                }
                return pos;
            }

            private void AddElement(Node n, string name, List<string> a, string text)
            {
                a = Positional(name, a);
                if (name == "AddCheckbox")
                {
                    name = "AddCheck";
                }
                if (name == "AddItemProperty" || name == "AddGumpIdOverride" || name == "AddMasterGump")
                {
                    return;
                }
                GumpElement e;
                switch (name)
                {
                    case "AddPage": _page = Int(A(a, 0, "0")); return;
                    case "AddGroup": _group = Int(A(a, 0, "0")); return;
                    case "AddTooltip":
                        if (_last != null && a.Count > 0)
                        {
                            object v = Eval(a[0]);
                            if (v is int cl)
                            {
                                _last.TooltipCliloc = cl;
                            }
                            else
                            {
                                _last.TooltipText = Text(v);
                            }
                        }
                        return;
                    case "AddBackground": e = new GumpElement { Type = ElementType.Background, X = Int(a[0]), Y = Int(a[1]), Width = Int(a[2]), Height = Int(a[3]), GumpId = Int(a[4]) }; break;
                    case "AddImage": e = new GumpElement { Type = ElementType.Image, X = Int(a[0]), Y = Int(a[1]), GumpId = Int(a[2]), Hue = a.Count > 3 ? Int(a[3]) : 0 }; break;
                    case "AddImageTiled": e = new GumpElement { Type = ElementType.ImageTiled, X = Int(a[0]), Y = Int(a[1]), Width = Int(a[2]), Height = Int(a[3]), GumpId = Int(a[4]) }; break;
                    case "AddSpriteImage": e = new GumpElement { Type = ElementType.SpriteImage, X = Int(a[0]), Y = Int(a[1]), GumpId = Int(a[2]), Width = Int(a[3]), Height = Int(a[4]), SourceX = Int(a[5]), SourceY = Int(a[6]) }; break;
                    case "AddAlphaRegion": e = new GumpElement { Type = ElementType.AlphaRegion, X = Int(a[0]), Y = Int(a[1]), Width = Int(a[2]), Height = Int(a[3]) }; break;
                    case "AddLabel": e = new GumpElement { Type = ElementType.Label, X = Int(a[0]), Y = Int(a[1]), Hue = Int(a[2]), Text = Str(a[3]) }; break;
                    case "AddLabelCropped": e = new GumpElement { Type = ElementType.LabelCropped, X = Int(a[0]), Y = Int(a[1]), Width = Int(a[2]), Height = Int(a[3]), Hue = Int(a[4]), Text = Str(a[5]) }; break;
                    case "AddHtml": e = new GumpElement { Type = ElementType.Html, X = Int(a[0]), Y = Int(a[1]), Width = Int(a[2]), Height = Int(a[3]), Text = Str(a[4]), HtmlBackground = Bool(A(a, 5, "false")), Scrollbar = Bool(A(a, 6, "false")) }; break;
                    case "AddHtmlLocalized":
                        e = new GumpElement { Type = ElementType.HtmlLocalized, X = Int(a[0]), Y = Int(a[1]), Width = Int(a[2]), Height = Int(a[3]), Cliloc = Int(a[4]) };
                        if (a.Count == 7) { e.HtmlBackground = Bool(a[5]); e.Scrollbar = Bool(a[6]); }
                        else if (a.Count == 8) { e.Hue = Int(a[5]); e.HtmlBackground = Bool(a[6]); e.Scrollbar = Bool(a[7]); }
                        else if (a.Count >= 9) { e.Hue = Int(a[6]); e.HtmlBackground = Bool(a[7]); e.Scrollbar = Bool(a[8]); }
                        else if (a.Count == 6) { e.Hue = Int(a[5]); }
                        break;
                    case "AddButton":
                    case "AddImageTiledButton":
                    {
                        string typeArg = A(a, 5, "GumpButtonType.Reply");
                        object tv = Eval(typeArg);
                        bool pageType = tv is int ti ? ti == 0 : typeArg.Contains("Page");
                        e = new GumpElement
                        {
                            Type = name == "AddButton" ? ElementType.Button : ElementType.ItemButton,
                            X = Int(a[0]), Y = Int(a[1]), GumpId = Int(a[2]), GumpId2 = Int(a[3]), ButtonId = Int(a[4]),
                            ButtonType = pageType ? ButtonKind.Page : ButtonKind.Reply, TargetPage = Int(A(a, 6, "0"))
                        };
                        if (name == "AddImageTiledButton")
                        {
                            e.ItemId = Int(a[7]);
                            e.Hue = Int(a[8]);
                            e.Width = Int(a[9]);
                            e.Height = Int(a[10]);
                            e.Cliloc = a.Count > 11 ? Int(a[11]) : 0;
                        }
                        break;
                    }
                    case "AddCheck": e = new GumpElement { Type = ElementType.Check, X = Int(a[0]), Y = Int(a[1]), GumpId = Int(a[2]), GumpId2 = Int(a[3]), Checked = Bool(a[4]), SwitchId = Int(a[5]) }; break;
                    case "AddRadio": e = new GumpElement { Type = ElementType.Radio, X = Int(a[0]), Y = Int(a[1]), GumpId = Int(a[2]), GumpId2 = Int(a[3]), Checked = Bool(a[4]), SwitchId = Int(a[5]), Group = _group }; break;
                    case "AddTextEntry":
                    case "AddTextEntryLimited":
                        e = new GumpElement { Type = ElementType.TextEntry, X = Int(a[0]), Y = Int(a[1]), Width = Int(a[2]), Height = Int(a[3]), Hue = Int(a[4]), EntryId = Int(a[5]), Text = Str(A(a, 6, "\"\"")), MaxLength = a.Count > 7 ? Int(a[7]) : 0 };
                        break;
                    case "AddItem": e = new GumpElement { Type = ElementType.Item, X = Int(a[0]), Y = Int(a[1]), ItemId = Int(a[2]), Hue = a.Count > 3 ? Int(a[3]) : 0 }; break;
                    default:
                        Warn($"line {n.Line}: {name} is not supported – skipped.", "unsupported:" + name);
                        return;
                }
                e.Page = _page;
                if (n.Start >= 0 && n.Text.EndsWith(text, StringComparison.Ordinal))
                {
                    int at = n.Start + n.Text.Length - text.Length;
                    _sourceCount.TryGetValue(at, out int occurrence);
                    _sourceCount[at] = occurrence + 1;
                    // #h = made inside a helper method (new elements are not written there)
                    e.Source = at.ToString(CultureInfo.InvariantCulture) + "#" + occurrence.ToString(CultureInfo.InvariantCulture) + (_helperDepth > 0 ? "#h" : string.Empty);
                }
                // a named ID (const int ButtonOkay = 1) names the element
                int idArg = name == "AddButton" || name == "AddImageTiledButton" ? 4 : name == "AddCheck" || name == "AddRadio" || name.StartsWith("AddTextEntry", StringComparison.Ordinal) ? 5 : -1;
                string constant = idArg >= 0 && idArg < a.Count && _constNames.Contains(a[idArg].Trim()) ? a[idArg].Trim() : null;
                if (!string.IsNullOrEmpty(n.Comment) && n.Comment.Length <= 40)
                {
                    e.Name = n.Comment;
                }
                else if (constant != null)
                {
                    e.Name = Regex.Replace(constant, "^(Button|Switch|Entry)", string.Empty);
                }
                Design.Elements.Add(e);
                _last = e;
            }

            #endregion

            #region Expressions

            private sealed class Tok
            {
                public char Kind;     // n number, s string, i identifier, o operator, $ interpolated
                public string Text;
                public object Value;
                public int Start;
                public int End;
            }

            private List<Tok> _t;
            private int _i;
            private string _src;

            private object Eval(string expr)
            {
                Step();
                string saveSrc = _src;
                List<Tok> saveT = _t;
                int saveI = _i;
                try
                {
                    _src = expr ?? string.Empty;
                    _t = Lex(_src);
                    _i = 0;
                    if (_t.Count == 0)
                    {
                        return null;
                    }
                    object v = Ternary();
                    return _i < _t.Count ? new Unknown(expr) : v;
                }
                catch (FlowException)
                {
                    throw;
                }
                catch (Exception)
                {
                    return new Unknown(expr);
                }
                finally
                {
                    _src = saveSrc;
                    _t = saveT;
                    _i = saveI;
                }
            }

            private static List<Tok> Lex(string s)
            {
                var list = new List<Tok>();
                int i = 0;
                string[] ops = { "??=", "<<", ">>", "<=", ">=", "==", "!=", "&&", "||", "??", "?.", "=>", "++", "--" };
                while (i < s.Length)
                {
                    char c = s[i];
                    if (char.IsWhiteSpace(c))
                    {
                        i++;
                        continue;
                    }
                    int start = i;
                    int q = i;
                    bool interp = false, verb = false;
                    while (q < s.Length && (s[q] == '$' || s[q] == '@') && q - i < 2)
                    {
                        interp |= s[q] == '$';
                        verb |= s[q] == '@';
                        q++;
                    }
                    if (q < s.Length && s[q] == '"')
                    {
                        int end = SkipString(s, q);
                        string raw = s.Substring(q + 1, Math.Max(0, end - q - 2));
                        if (verb)
                        {
                            raw = raw.Replace("\"\"", "\"");
                        }
                        list.Add(new Tok
                        {
                            Kind = interp ? '$' : 's', Text = s.Substring(start, end - start), Start = start, End = end,
                            Value = interp ? (object)(raw, verb) : verb ? raw : Unescape(raw)
                        });
                        i = end;
                        continue;
                    }
                    if (c == '\'')
                    {
                        int end = SkipString(s, i);
                        list.Add(new Tok { Kind = 's', Text = s.Substring(start, end - start), Value = Unescape(s.Substring(i + 1, Math.Max(0, end - i - 2))), Start = start, End = end });
                        i = end;
                        continue;
                    }
                    if (char.IsDigit(c) || c == '.' && i + 1 < s.Length && char.IsDigit(s[i + 1]))
                    {
                        Match m = Regex.Match(s.Substring(i), @"^(0[xX][0-9a-fA-F_]+|0[bB][01_]+|[\d_]*\.?[\d_]+(?:[eE][+-]?\d+)?)([uUlLfFdDmM]{0,2})");
                        string num = m.Groups[1].Value.Replace("_", string.Empty);
                        object v;
                        if (num.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            v = unchecked((int)Convert.ToInt64(num.Substring(2), 16));
                        }
                        else if (num.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
                        {
                            v = Convert.ToInt32(num.Substring(2), 2);
                        }
                        else if (num.Contains(".") || num.Contains("e") || num.Contains("E") || Regex.IsMatch(m.Groups[2].Value, "[fFdDmM]"))
                        {
                            v = double.Parse(num, CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            v = long.TryParse(num, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l) ? (object)unchecked((int)l) : 0;
                        }
                        i += m.Length;
                        list.Add(new Tok { Kind = 'n', Text = m.Value, Value = v, Start = start, End = i });
                        continue;
                    }
                    if (char.IsLetter(c) || c == '_')
                    {
                        while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_'))
                        {
                            i++;
                        }
                        list.Add(new Tok { Kind = 'i', Text = s.Substring(start, i - start), Start = start, End = i });
                        continue;
                    }
                    string op = ops.FirstOrDefault(o => string.CompareOrdinal(s, i, o, 0, o.Length) == 0) ?? c.ToString();
                    i += op.Length;
                    list.Add(new Tok { Kind = 'o', Text = op, Start = start, End = i });
                }
                return list;
            }

            private static string Unescape(string s)
            {
                try
                {
                    return Regex.Unescape(s.Replace("\\'", "'"));
                }
                catch (ArgumentException)
                {
                    return s;
                }
            }

            private Tok Peek(int ahead = 0) => _i + ahead < _t.Count ? _t[_i + ahead] : null;

            private bool IsOp(string op, int ahead = 0) => Peek(ahead) is Tok t && t.Kind == 'o' && t.Text == op;

            private bool Accept(string op)
            {
                if (IsOp(op))
                {
                    _i++;
                    return true;
                }
                return false;
            }

            private void Expect(string op)
            {
                if (!Accept(op))
                {
                    throw new FormatException("expected " + op);
                }
            }

            private string Span(int fromTok)
            {
                int a = _t[Math.Min(fromTok, _t.Count - 1)].Start;
                int b = _t[Math.Max(Math.Min(_i - 1, _t.Count - 1), 0)].End;
                return b > a ? _src.Substring(a, b - a) : _src;
            }

            private object Ternary()
            {
                int from = _i;
                object c = Coalesce();
                if (!Accept("?"))
                {
                    return c;
                }
                object a = Ternary();
                Expect(":");
                object b = Ternary();
                if (c is bool cb)
                {
                    return cb ? a : b;
                }
                // unknown condition: texts show the game value as placeholder ({m.Name}), numbers use the known choice
                if (a is string || b is string)
                {
                    return a is Unknown ? a : b is Unknown ? b : a;
                }
                return !(a is Unknown) && a != null ? a : !(b is Unknown) && b != null ? b : new Unknown(Span(from));
            }

            private object Coalesce()
            {
                object a = Or();
                while (Accept("??"))
                {
                    object b = Or();
                    a = a ?? b;
                }
                return a;
            }

            private object Or()
            {
                int from = _i;
                object a = And();
                while (IsOp("||"))
                {
                    _i++;
                    object b = And();
                    a = a is bool x && b is bool y ? x || y : a is bool x2 && x2 || b is bool y2 && y2 ? true : (object)new Unknown(Span(from));
                }
                return a;
            }

            private object And()
            {
                int from = _i;
                object a = BitOr();
                while (IsOp("&&"))
                {
                    _i++;
                    object b = BitOr();
                    a = a is bool x && b is bool y ? x && y : a is bool x2 && !x2 || b is bool y2 && !y2 ? false : (object)new Unknown(Span(from));
                }
                return a;
            }

            private object BitOr()
            {
                int from = _i;
                object a = Equality();
                while (IsOp("|") || IsOp("&") || IsOp("^"))
                {
                    string op = _t[_i++].Text;
                    object b = Equality();
                    a = Binary(op, a, b, null) is Unknown ? new Unknown(Span(from)) : Binary(op, a, b, null);
                }
                return a;
            }

            private object Equality()
            {
                int from = _i;
                object a = Relational();
                while (IsOp("==") || IsOp("!="))
                {
                    string op = _t[_i++].Text;
                    object b = Relational();
                    if (a is Unknown || b is Unknown)
                    {
                        a = new Unknown(Span(from));
                        continue;
                    }
                    bool eq = Equal(a, b);
                    a = op == "==" ? eq : !eq;
                }
                return a;
            }

            private object Relational()
            {
                int from = _i;
                object a = Shift();
                while (IsOp("<") || IsOp(">") || IsOp("<=") || IsOp(">=") || Peek() is Tok t && t.Kind == 'i' && (t.Text == "is" || t.Text == "as"))
                {
                    string op = _t[_i++].Text;
                    if (op == "is" || op == "as")
                    {
                        Type();
                        a = new Unknown(Span(from));
                        continue;
                    }
                    object b = Shift();
                    if (!(Num(a) is double x) || !(Num(b) is double y))
                    {
                        a = new Unknown(Span(from));
                        continue;
                    }
                    a = op == "<" ? x < y : op == ">" ? x > y : op == "<=" ? x <= y : x >= y;
                }
                return a;
            }

            private object Shift()
            {
                int from = _i;
                object a = Additive();
                while (IsOp("<<") || IsOp(">>"))
                {
                    string op = _t[_i++].Text;
                    object b = Additive();
                    object r = Binary(op, a, b, null);
                    a = r is Unknown ? new Unknown(Span(from)) : r;
                }
                return a;
            }

            private object Additive()
            {
                int from = _i;
                object a = Multiplicative();
                while (IsOp("+") || IsOp("-"))
                {
                    string op = _t[_i++].Text;
                    object b = Multiplicative();
                    object r = Binary(op, a, b, null);
                    a = r is Unknown && !(a is string) && !(b is string) ? new Unknown(Span(from)) : r;
                }
                return a;
            }

            private object Multiplicative()
            {
                int from = _i;
                object a = Unary();
                while (IsOp("*") || IsOp("/") || IsOp("%"))
                {
                    string op = _t[_i++].Text;
                    object b = Unary();
                    object r = Binary(op, a, b, null);
                    a = r is Unknown ? new Unknown(Span(from)) : r;
                }
                return a;
            }

            private static readonly HashSet<string> CastTypes = new HashSet<string>
            {
                "int", "uint", "short", "ushort", "byte", "sbyte", "long", "ulong", "double", "float", "decimal", "string", "object", "char", "bool"
            };

            private object Unary()
            {
                int from = _i;
                if (IsOp("-") || IsOp("+") || IsOp("!") || IsOp("~"))
                {
                    string op = _t[_i++].Text;
                    object v = Unary();
                    switch (op)
                    {
                        case "-": return v is int i ? -i : v is double d ? -d : (object)new Unknown(Span(from));
                        case "+": return v;
                        case "!": return v is bool b ? !b : (object)new Unknown(Span(from));
                        default: return v is int i2 ? ~i2 : (object)new Unknown(Span(from));
                    }
                }
                if ((IsOp("++") || IsOp("--")) && Peek(1) is Tok pre && pre.Kind == 'i')
                {
                    // ++x: change the variable, the new value is used
                    bool up = _t[_i++].Text == "++";
                    string name = _t[_i++].Text;
                    object nv = Binary(up ? "+" : "-", Get(name), 1, name);
                    Assign(name, nv);
                    return nv;
                }
                // cast: (int)x, (GumpButtonType)1
                if (IsOp("(") && Peek(1) is Tok t1 && t1.Kind == 'i' && !ScopesHave(t1.Text))
                {
                    int save = _i;
                    _i++;
                    string type = Type();
                    if (type != null && Accept(")") && Peek() is Tok nt && (nt.Kind != 'o' || nt.Text == "(" || nt.Text == "!")
                        && (CastTypes.Contains(type) || char.IsUpper(type[0])))
                    {
                        object v = Unary();
                        return Cast(type, v);
                    }
                    _i = save;
                }
                return Postfix();
            }

            private static object Cast(string type, object v)
            {
                switch (type)
                {
                    case "int": case "short": case "ushort": case "byte": case "sbyte": case "uint": case "long": case "ulong":
                        return v is double d ? (int)d : v;
                    case "double": case "float": case "decimal":
                        return v is int i ? (double)i : v;
                    case "string":
                        return v;
                    default:
                        return v;
                }
            }

            /// <summary>Reads a type name (a.b.c&lt;x&gt;[]) and returns it, or null.</summary>
            private string Type()
            {
                if (!(Peek() is Tok t) || t.Kind != 'i')
                {
                    return null;
                }
                var sb = new StringBuilder(t.Text);
                _i++;
                while (IsOp(".") && Peek(1) is Tok n && n.Kind == 'i')
                {
                    sb.Append('.').Append(n.Text);
                    _i += 2;
                }
                if (IsOp("<"))
                {
                    int depth = 0;
                    do
                    {
                        if (IsOp("<")) depth++;
                        else if (IsOp(">")) depth--;
                        _i++;
                    }
                    while (depth > 0 && _i < _t.Count);
                }
                while (IsOp("[") && IsOp("]", 1))
                {
                    _i += 2;
                }
                Accept("?");
                return sb.ToString();
            }

            private object Postfix()
            {
                int from = _i;
                object v = Primary(out string path);
                while (true)
                {
                    if (IsOp(".") || IsOp("?."))
                    {
                        _i++;
                        Tok m = _t[_i++];
                        string member = m.Text;
                        if (IsOp("("))
                        {
                            List<object> args = Args();
                            v = MemberCall(v, path, member, args, from);
                            path = null;
                        }
                        else
                        {
                            v = Member(v, path, member, from);
                            path = path != null ? path + "." + member : null;
                        }
                        continue;
                    }
                    if (IsOp("("))
                    {
                        List<object> args = Args();
                        v = path != null && _methods.ContainsKey(path) ? Call(path, args) ?? new Unknown(Span(from)) : new Unknown(Span(from));
                        path = null;
                        continue;
                    }
                    if (IsOp("["))
                    {
                        _i++;
                        object idx = Ternary();
                        Expect("]");
                        v = Index(v, idx, Span(from));
                        path = null;
                        continue;
                    }
                    if (IsOp("++") || IsOp("--"))
                    {
                        // x++: the old value is used, the variable changes
                        bool up = _t[_i++].Text == "++";
                        if (path != null && path != "this")
                        {
                            Assign(path, Binary(up ? "+" : "-", v, 1, path));
                        }
                        continue;
                    }
                    if (IsOp("!"))
                    {
                        // null-forgiving operator
                        if (Peek(1) == null || Peek(1).Kind == 'o' && Peek(1).Text != "(")
                        {
                            _i++;
                            continue;
                        }
                    }
                    return v;
                }
            }

            private List<object> Args()
            {
                Expect("(");
                var list = new List<object>();
                if (Accept(")"))
                {
                    return list;
                }
                do
                {
                    // named arguments (name: value) and ref / out
                    if (Peek() is Tok t && t.Kind == 'i' && IsOp(":", 1))
                    {
                        _i += 2;
                    }
                    if (Peek() is Tok r && r.Kind == 'i' && (r.Text == "ref" || r.Text == "out" || r.Text == "in"))
                    {
                        _i++;
                    }
                    list.Add(Ternary());
                }
                while (Accept(","));
                Expect(")");
                return list;
            }

            private object Primary(out string path)
            {
                path = null;
                Tok t = Peek() ?? throw new FormatException("end");
                int from = _i;
                switch (t.Kind)
                {
                    case 'n':
                    case 's':
                        _i++;
                        return t.Value;
                    case '$':
                        _i++;
                        (string raw, bool verbatim) = ((string, bool))t.Value;
                        return Interpolate(raw, verbatim);
                    case 'o':
                        if (t.Text == "(")
                        {
                            _i++;
                            string firstName = TupleName();
                            object v = Ternary();
                            if (Accept(","))
                            {
                                // tuple (a, b) or (Name: a, Id: b): fields Item1 … and the names
                                var tuple = new ObjectValue { Type = "tuple" };
                                tuple.Fields["Item1"] = v;
                                if (firstName != null)
                                {
                                    tuple.Fields[firstName] = v;
                                }
                                int k = 2;
                                do
                                {
                                    string nm = TupleName();
                                    object x = Ternary();
                                    tuple.Fields["Item" + k++] = x;
                                    if (nm != null)
                                    {
                                        tuple.Fields[nm] = x;
                                    }
                                }
                                while (Accept(","));
                                Expect(")");
                                return tuple;
                            }
                            Expect(")");
                            return v;
                        }
                        if (t.Text == "{")
                        {
                            return Initializer();
                        }
                        if (t.Text == "[")
                        {
                            // collection expression [a, b]
                            _i++;
                            var items = new List<object>();
                            if (!Accept("]"))
                            {
                                do
                                {
                                    if (IsOp("]"))
                                    {
                                        break;   // trailing comma
                                    }
                                    items.Add(Ternary());
                                }
                                while (Accept(","));
                                Expect("]");
                            }
                            return items.ToArray();
                        }
                        throw new FormatException("unexpected " + t.Text);
                }
                // identifier
                _i++;
                switch (t.Text)
                {
                    case "true": return true;
                    case "false": return false;
                    case "null": return null;
                    case "this": path = "this"; return new Unknown("this");
                    case "typeof":
                    case "nameof":
                    case "sizeof":
                    {
                        Expect("(");
                        int depth = 1;
                        int start = _i;
                        while (_i < _t.Count && depth > 0)
                        {
                            if (IsOp("(")) depth++;
                            if (IsOp(")")) depth--;
                            _i++;
                        }
                        if (t.Text == "nameof" && _i - 2 >= start)
                        {
                            return _t[_i - 2].Text;
                        }
                        return new Unknown(Span(from));
                    }
                    case "new":
                        return New(from);
                    case "default":
                        if (IsOp("("))
                        {
                            _i++;
                            string type = Type();
                            Expect(")");
                            return Default(type ?? string.Empty);
                        }
                        return null;
                }
                path = t.Text;
                foreach (Dictionary<string, object> s in _scopes)
                {
                    if (s.TryGetValue(t.Text, out object v))
                    {
                        return v;
                    }
                }
                if (_globals.TryGetValue(t.Text, out object g))
                {
                    return g;
                }
                return new Unknown(t.Text);
            }

            private string TupleName()
            {
                if (Peek() is Tok t && t.Kind == 'i' && IsOp(":", 1))
                {
                    _i += 2;
                    return t.Text;
                }
                return null;
            }

            /// <summary>Gives tuples the names of their declared type: (Type Type, int BaseID)[] → .BaseID works.</summary>
            private static void NameTuples(object value, List<string> names)
            {
                switch (value)
                {
                    case object[] arr:
                        foreach (object o in arr)
                        {
                            NameTuples(o, names);
                        }
                        break;
                    case ObjectValue ov when ov.Type == "tuple":
                        for (int i = 0; i < names.Count; i++)
                        {
                            if (names[i] != null && ov.Fields.TryGetValue("Item" + (i + 1), out object item))
                            {
                                ov.Fields[names[i]] = item;
                            }
                        }
                        break;
                }
            }

            private object New(int from)
            {
                // new[] { … }, new string[] { … }, new int[5], new List<string> { … }, new Something(…)
                string type = IsOp("[") ? null : Type();
                if (IsOp("["))
                {
                    _i++;
                    object size = IsOp("]") ? null : Ternary();
                    Expect("]");
                    while (IsOp("[") && IsOp("]", 1))
                    {
                        _i += 2;
                    }
                    if (IsOp("{"))
                    {
                        return Initializer();
                    }
                    if (size is int n && n >= 0 && n <= 10000)
                    {
                        object def = Default(type ?? string.Empty);
                        return Enumerable.Repeat(def, n).ToArray();
                    }
                    return new Unknown(Span(from));
                }
                if (IsOp("("))
                {
                    List<object> args = Args();
                    if (IsOp("{"))
                    {
                        return Initializer();
                    }
                    ObjectValue created = CreateObject(type, args);
                    if (created != null)
                    {
                        return created;
                    }
                    // new List<Mobile>() is filled in the game: unknown (its Count becomes a sample size)
                    return new Unknown(Span(from));
                }
                if (IsOp("{"))
                {
                    return Initializer();
                }
                return new Unknown(Span(from));
            }

            private object Initializer()
            {
                Expect("{");
                var items = new List<object>();
                if (Accept("}"))
                {
                    return items.ToArray();
                }
                do
                {
                    if (IsOp("}"))
                    {
                        break;   // trailing comma
                    }
                    items.Add(IsOp("{") ? Initializer() : Ternary());
                }
                while (Accept(","));
                Expect("}");
                return items.ToArray();
            }

            private object Member(object v, string path, string member, int from)
            {
                if (v is ObjectValue ov)
                {
                    return ov.Fields.TryGetValue(member, out object fv0) ? fv0 : new Unknown(Span(from));
                }
                switch (v)
                {
                    case object[] arr when member == "Length" || member == "Count":
                        return arr.Length;
                    case string s when member == "Length":
                        return s.Length;
                    case Unknown u when (member == "Count" || member == "Length") && !(path != null && char.IsUpper(path[0]) && !ScopesHave(path)):
                        // a list of the game: show a few sample rows
                        Warn($"{u.Text}.{member} is only known in the game – {SampleCount} is used, so the list shows sample rows.", "count:" + u.Text);
                        return SampleCount;
                }
                if (path != null)
                {
                    string full = path + "." + member;
                    switch (full)
                    {
                        case "string.Empty":
                        case "String.Empty":
                            return string.Empty;
                        case "int.MaxValue": return int.MaxValue;
                        case "int.MinValue": return int.MinValue;
                        case "GumpButtonType.Page": return 0;
                        case "GumpButtonType.Reply": return 1;
                        case "Environment.NewLine": return "\n";
                    }
                    if (full.EndsWith("GumpButtonType.Page", StringComparison.Ordinal))
                    {
                        return 0;
                    }
                    if (full.EndsWith("GumpButtonType.Reply", StringComparison.Ordinal))
                    {
                        return 1;
                    }
                    string owner = path.Contains(".") ? path.Substring(path.LastIndexOf('.') + 1) : path;
                    if (v is Unknown && owner.Length > 0 && char.IsUpper(owner[0]) && !ScopesHave(path))
                    {
                        Dictionary<string, object> fields = ClassFields(owner);
                        if (fields != null && fields.TryGetValue(member, out object fv))
                        {
                            return fv;
                        }
                    }
                    if (full.StartsWith("this.", StringComparison.Ordinal))
                    {
                        object o = Get(member);
                        if (!(o is Unknown))
                        {
                            return o;
                        }
                    }
                }
                return new Unknown(Span(from));
            }

            private object MemberCall(object v, string path, string member, List<object> args, int from)
            {
                string full = path != null ? path + "." + member : null;
                switch (full)
                {
                    case "String.Format":
                    case "string.Format":
                        if (args.Count > 0 && args[0] is string fmt)
                        {
                            return Format(fmt, args.Skip(1).ToList());
                        }
                        return new Unknown(Span(from));
                    case "String.Concat":
                    case "string.Concat":
                        return string.Concat(args.Select(Text));
                    case "String.Join":
                    case "string.Join":
                        if (args.Count == 2 && args[1] is object[] parts)
                        {
                            return string.Join(Text(args[0]), parts.Select(Text));
                        }
                        return new Unknown(Span(from));
                    case "Math.Max":
                        return args.Count == 2 && args[0] is int a && args[1] is int b ? Math.Max(a, b) : Num(args.ElementAtOrDefault(0)) is double x && Num(args.ElementAtOrDefault(1)) is double y ? (object)Math.Max(x, y) : new Unknown(Span(from));
                    case "Math.Min":
                        return args.Count == 2 && args[0] is int a2 && args[1] is int b2 ? Math.Min(a2, b2) : Num(args.ElementAtOrDefault(0)) is double x2 && Num(args.ElementAtOrDefault(1)) is double y2 ? (object)Math.Min(x2, y2) : new Unknown(Span(from));
                    case "Math.Abs":
                        return args.Count == 1 && args[0] is int a3 ? Math.Abs(a3) : args.Count == 1 && args[0] is double d3 ? (object)Math.Abs(d3) : new Unknown(Span(from));
                    case "Math.Clamp":
                        return args.Count == 3 && args[0] is int cv && args[1] is int cmin && args[2] is int cmax ? Math.Max(cmin, Math.Min(cmax, cv)) : (object)new Unknown(Span(from));
                    case "Math.Ceiling":
                        return Num(args.ElementAtOrDefault(0)) is double c ? (object)Math.Ceiling(c) : new Unknown(Span(from));
                    case "Math.Floor":
                        return Num(args.ElementAtOrDefault(0)) is double f ? (object)Math.Floor(f) : new Unknown(Span(from));
                    case "Convert.ToInt32":
                    case "int.Parse":
                        return args.Count > 0 && args[0] is int ci ? ci : args.Count > 0 && args[0] is double cd ? (int)cd : args.Count > 0 && args[0] is string cs && int.TryParse(cs, out int pi) ? pi : (object)new Unknown(Span(from));
                    case "Color.FromArgb":
                        return new Unknown(Span(from));
                }
                if (full != null && _methods.ContainsKey(member) && (path == "this" || path == "base"))
                {
                    return Call(member, args) ?? new Unknown(Span(from));
                }
                if (v is string s)
                {
                    switch (member)
                    {
                        case "ToUpper": case "ToUpperInvariant": return s.ToUpperInvariant();
                        case "ToLower": case "ToLowerInvariant": return s.ToLowerInvariant();
                        case "Trim": return s.Trim();
                        case "ToString": return s;
                        case "Replace" when args.Count == 2 && args[0] is string o && args[1] is string r: return o.Length == 0 ? s : s.Replace(o, r);
                        case "Substring" when args.Count >= 1 && args[0] is int st && st <= s.Length:
                            return args.Count == 2 && args[1] is int ln && st + ln <= s.Length ? s.Substring(st, ln) : s.Substring(st);
                        case "PadLeft" when args.Count >= 1 && args[0] is int pl: return s.PadLeft(pl);
                        case "PadRight" when args.Count >= 1 && args[0] is int pr: return s.PadRight(pr);
                    }
                }
                if (member == "ToString" && !(v is Unknown) && v != null)
                {
                    if (args.Count == 1 && args[0] is string spec)
                    {
                        return FormatValue(v, spec);
                    }
                    return Text(v);
                }
                return new Unknown(Span(from));
            }

            private object Index(object v, object idx, string text)
            {
                if (v is object[] arr && idx is int i && i >= 0 && i < arr.Length)
                {
                    return arr[i];
                }
                if (v is string s && idx is int j && j >= 0 && j < s.Length)
                {
                    return s[j].ToString();
                }
                return new Unknown(text);
            }

            private string Interpolate(string raw, bool verbatim)
            {
                var sb = new StringBuilder();
                var lit = new StringBuilder();
                void Flush()
                {
                    sb.Append(verbatim ? lit.ToString() : Unescape(lit.ToString()));
                    lit.Clear();
                }
                for (int i = 0; i < raw.Length; i++)
                {
                    char c = raw[i];
                    if (c == '{')
                    {
                        if (i + 1 < raw.Length && raw[i + 1] == '{')
                        {
                            lit.Append('{');
                            i++;
                            continue;
                        }
                        Flush();
                        int depth = 1, j = i + 1;
                        for (; j < raw.Length && depth > 0; j++)
                        {
                            if (raw[j] == '"')
                            {
                                j = SkipString(raw, j) - 1;
                                continue;
                            }
                            if (raw[j] == '{') depth++;
                            if (raw[j] == '}') depth--;
                        }
                        string hole = raw.Substring(i + 1, Math.Max(0, j - i - 2));
                        string spec = null;
                        Match f = Regex.Match(hole, @"^(.*?)(?:,\s*-?\d+)?:([A-Za-z]\d*|[#0.,]+)$", RegexOptions.Singleline);
                        if (f.Success && !hole.Contains("?"))
                        {
                            hole = f.Groups[1].Value;
                            spec = f.Groups[2].Value;
                        }
                        object v = Eval(hole);
                        sb.Append(spec != null && !(v is Unknown) ? FormatValue(v, spec) : Text(v));
                        i = j - 1;
                        continue;
                    }
                    if (c == '}' && i + 1 < raw.Length && raw[i + 1] == '}')
                    {
                        i++;
                    }
                    lit.Append(c);
                }
                Flush();
                return sb.ToString();
            }

            private static string Format(string fmt, List<object> args)
            {
                return Regex.Replace(fmt, @"\{\{|\}\}|\{(\d+)(?:,\s*-?\d+)?(?::([^}]*))?\}", m =>
                {
                    if (m.Value == "{{") return "{";
                    if (m.Value == "}}") return "}";
                    int n = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                    if (n >= args.Count)
                    {
                        return m.Value;
                    }
                    return m.Groups[2].Success && !(args[n] is Unknown) ? FormatValue(args[n], m.Groups[2].Value) : Text(args[n]);
                });
            }

            private static string FormatValue(object v, string spec)
            {
                try
                {
                    switch (v)
                    {
                        case int i: return i.ToString(spec, CultureInfo.InvariantCulture);
                        case double d: return d.ToString(spec, CultureInfo.InvariantCulture);
                    }
                }
                catch (FormatException)
                {
                }
                return Text(v);
            }

            private static string Text(object v)
            {
                switch (v)
                {
                    case null: return string.Empty;
                    case string s: return s;
                    case bool b: return b ? "True" : "False";
                    case double d: return d.ToString(CultureInfo.InvariantCulture);
                    case int i: return i.ToString(CultureInfo.InvariantCulture);
                    case object[] arr: return "{" + string.Join(", ", arr.Select(Text)) + "}";
                    default: return v.ToString();
                }
            }

            private static double? Num(object v) => v is int i ? i : v is double d ? d : (double?)null;

            private static bool Equal(object a, object b)
            {
                if (Num(a) is double x && Num(b) is double y)
                {
                    return Math.Abs(x - y) < 1e-9;
                }
                return Equals(a, b);
            }

            private static object Binary(string op, object a, object b, string text)
            {
                if (op == "+" && (a is string || b is string))
                {
                    return Text(a) + Text(b);
                }
                if (a is Unknown || b is Unknown || a == null || b == null)
                {
                    return new Unknown(text ?? (Text(a) + " " + op + " " + Text(b)));
                }
                if (a is bool ba && b is bool bb)
                {
                    switch (op)
                    {
                        case "&": return ba & bb;
                        case "|": return ba | bb;
                        case "^": return ba ^ bb;
                    }
                }
                if (a is int x && b is int y)
                {
                    unchecked
                    {
                        switch (op)
                        {
                            case "+": return x + y;
                            case "-": return x - y;
                            case "*": return x * y;
                            case "/": return y == 0 ? (object)new Unknown(text ?? "÷0") : x / y;
                            case "%": return y == 0 ? (object)new Unknown(text ?? "÷0") : x % y;
                            case "&": return x & y;
                            case "|": return x | y;
                            case "^": return x ^ y;
                            case "<<": return x << y;
                            case ">>": return x >> y;
                        }
                    }
                }
                if (Num(a) is double dx && Num(b) is double dy)
                {
                    switch (op)
                    {
                        case "+": return dx + dy;
                        case "-": return dx - dy;
                        case "*": return dx * dy;
                        case "/": return dy == 0 ? (object)new Unknown(text ?? "÷0") : dx / dy;
                        case "%": return dy == 0 ? (object)new Unknown(text ?? "÷0") : dx % dy;
                    }
                }
                return new Unknown(text ?? op);
            }

            #endregion
        }

        #region Helpers

        private static string Short(string s)
        {
            s = Regex.Replace(s ?? string.Empty, @"\s+", " ").Trim();
            return s.Length > 70 ? s.Substring(0, 70) + "…" : s;
        }

        /// <summary>Splits at commas outside (), [], {}, &lt;&gt; generics and strings.</summary>
        private static List<string> SplitArgs(string s) => SplitTop(s, ',');

        private static List<string> SplitTop(string s, char sep)
        {
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(s))
            {
                return list;
            }
            var cur = new StringBuilder();
            int depth = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '"' || c == '\'')
                {
                    int end = SkipString(s, i);
                    cur.Append(s, i, end - i);
                    i = end - 1;
                    continue;
                }
                if (c == '(' || c == '[' || c == '{')
                {
                    depth++;
                }
                else if (c == ')' || c == ']' || c == '}')
                {
                    depth--;
                }
                if (c == sep && depth == 0)
                {
                    list.Add(cur.ToString().Trim());
                    cur.Clear();
                    continue;
                }
                cur.Append(c);
            }
            list.Add(cur.ToString().Trim());
            return list;
        }

        #endregion
    }
}

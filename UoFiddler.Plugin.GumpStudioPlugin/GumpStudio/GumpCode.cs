using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    public enum CodeTarget { ServUO, ServUOAddCallsOnly, Sphere, ModernUO }

    /// <summary>
    /// Writes the gump as ServUO / RunUO script (class with constructor, OnResponse with the button actions, command),
    /// as ModernUO DynamicGump (BuildLayout with builder.Add…) or as Sphere dialog.
    /// </summary>
    public static class GumpCodeGenerator
    {
        public static readonly string[] TargetNames =
        {
            "ServUO / RunUO – complete gump class", "ServUO – only the Add… lines", "Sphere – [DIALOG] script", "ModernUO – DynamicGump class"
        };

        private static readonly string[] MapNames = { "Felucca", "Trammel", "Ilshenar", "Malas", "Tokuno", "TerMur" };

        private static string Str(string s) => "\"" + (s ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", string.Empty).Replace("\n", "\\n") + "\"";

        private static string B(bool b) => b ? "true" : "false";

        private static string Hex(int v) => v < 10 ? v.ToString(CultureInfo.InvariantCulture) : "0x" + v.ToString("X", CultureInfo.InvariantCulture);

        private static string N(int v) => v.ToString(CultureInfo.InvariantCulture);

        public static string Identifier(string name, string fallback)
        {
            string s = Regex.Replace(name ?? string.Empty, @"[^A-Za-z0-9_]", string.Empty);
            if (s.Length == 0)
            {
                s = fallback;
            }
            if (char.IsDigit(s[0]))
            {
                s = "_" + s;
            }
            return char.ToUpperInvariant(s[0]) + s.Substring(1);
        }

        public static string Generate(GumpDesign d, CodeTarget target)
        {
            return target switch
            {
                CodeTarget.Sphere => Sphere(d),
                CodeTarget.ServUOAddCallsOnly => string.Join("\r\n", AddLines(d, "", new Ctx(d, false) { LanguageVariable = Bilingual(d) })),
                CodeTarget.ModernUO => ModernUO(d),
                _ => ServUO(d)
            };
        }

        /// <summary>One Add… statement (ServUO, or ModernUO with "builder.").</summary>
        public static string AddStatement(GumpElement e, GumpDesign d, bool modern) => AddCall(e, new Ctx(d, modern), null, null, null, null);

        /// <summary>The Add… lines of some elements (copy to the clipboard).</summary>
        public static List<string> AddLines(GumpDesign d, string indent) => AddLines(d, indent, new Ctx(d, false));

        /// <summary>Settings of one script run.</summary>
        private sealed class Ctx
        {
            public Ctx(GumpDesign d, bool modern)
            {
                D = d;
                Modern = modern;
                Names = ConstantNames(d);
            }

            public GumpDesign D { get; }

            public bool Modern { get; }

            public Dictionary<GumpElement, string> Names { get; }

            /// <summary>Name of the bool that is true for the second language (null = one language).</summary>
            public bool LanguageVariable { get; set; }

            public string P => Modern ? "builder." : string.Empty;
        }

        private static bool Bilingual(GumpDesign d) =>
            !string.IsNullOrWhiteSpace(d.SecondLanguage) && d.Elements.Any(e => !string.IsNullOrEmpty(e.TextDe) && e.TextDe != e.Text);

        private static string TextExpr(GumpElement e, Ctx c)
        {
            if (c.LanguageVariable && !string.IsNullOrEmpty(e.TextDe) && e.TextDe != e.Text)
            {
                return $"(de ? {Str(e.TextDe)} : {Str(e.Text)})";
            }
            return Str(e.Text);
        }

        // ------------------------------------------------------------------ Add… lines

        /// <summary>The Add… lines in client order: page 0 first, then AddPage(n) with its entries; lists become for loops.</summary>
        private static List<string> AddLines(GumpDesign d, string indent, Ctx c)
        {
            var lines = new List<string>();
            int maxPage = d.Pages;
            bool anyPages = d.Elements.Any(e => e.Page > 0);
            List<GumpListSpec> lists = d.Lists ?? new List<GumpListSpec>();
            for (int page = 0; page <= maxPage; page++)
            {
                List<GumpElement> els = d.Elements.Where(e => e.Page == page && (string.IsNullOrEmpty(e.ListId) || !lists.Any(l => l.Id == e.ListId))).ToList();
                List<GumpListSpec> here = lists.Where(l => !l.Paged && l.FirstPage == page).ToList();
                if (page > 0)
                {
                    if (!anyPages)
                    {
                        break;
                    }
                    if (els.Count == 0 && here.Count == 0)
                    {
                        continue;   // page that only holds rows of a list (the loop adds it)
                    }
                    lines.Add(string.Empty);
                    lines.Add($"{indent}{c.P}AddPage({page});");
                }
                else if (anyPages)
                {
                    lines.Add(c.Modern ? $"{indent}builder.AddPage();" : $"{indent}AddPage(0);");
                }
                int group = -1;
                foreach (GumpElement e in els)
                {
                    if (e.Type == ElementType.Radio && e.Group != group)
                    {
                        group = e.Group;
                        lines.Add($"{indent}{c.P}AddGroup({group});");
                    }
                    string comment = string.IsNullOrEmpty(e.Name) ? string.Empty : " // " + e.Name;
                    lines.Add(indent + AddCall(e, c, null, null, null, null) + comment);
                    lines.AddRange(Tooltip(e, c, indent));
                }
                foreach (GumpListSpec spec in here)
                {
                    lines.AddRange(ListLoop(d, spec, c, indent));
                }
            }
            foreach (GumpListSpec spec in lists.Where(l => l.Paged))
            {
                lines.AddRange(ListLoop(d, spec, c, indent));
            }
            return lines;
        }

        private static IEnumerable<string> Tooltip(GumpElement e, Ctx c, string indent)
        {
            if (e.TooltipCliloc > 0)
            {
                yield return $"{indent}{c.P}AddTooltip({e.TooltipCliloc});";
            }
            else if (!string.IsNullOrEmpty(e.TooltipText))
            {
                yield return c.Modern
                    ? $"{indent}// tooltip text \"{e.TooltipText}\": ModernUO only has cliloc tooltips (builder.AddTooltip(number))"
                    : $"{indent}AddTooltip({Str(e.TooltipText)});";
            }
        }

        /// <summary>A list as for loop: arrays for the column texts, page buttons inside the loop when it has more pages.</summary>
        private static List<string> ListLoop(GumpDesign d, GumpListSpec spec, Ctx c, string indent)
        {
            var lines = new List<string>();
            List<GumpElement> template = GumpLists.Template(d, spec);
            if (template.Count == 0)
            {
                return lines;
            }
            string L = spec.Identifier;
            string i2 = indent + "    ", i3 = i2 + "    ";
            int per = spec.PerPage;
            lines.Add(string.Empty);
            lines.Add($"{indent}// {spec.Name}: {spec.Rows} rows" + (spec.Paged ? $", {spec.RowsPerPage} per page (pages {spec.FirstPage} … {spec.FirstPage + spec.PageCount - 1})" : string.Empty));
            foreach (int col in template.Where(t => t.ListColumn >= 0).Select(t => t.ListColumn).Distinct().OrderBy(x => x))
            {
                IEnumerable<string> texts = Enumerable.Range(0, spec.Rows).Select(r => Str(spec.Text(col, r)));
                lines.Add($"{indent}string[] {L}Column{col + 1} = {{ {string.Join(", ", texts)} }};");
            }
            lines.Add($"{indent}for (int i = 0; i < {spec.Rows}; i++)");
            lines.Add($"{indent}{{");
            if (spec.Paged)
            {
                GumpElement back = d.Elements.FirstOrDefault(e => e.ListId == spec.Id && e.ListRow < 0 && e.TargetPage < e.Page);
                GumpElement next = d.Elements.FirstOrDefault(e => e.ListId == spec.Id && e.ListRow < 0 && e.TargetPage > e.Page);
                lines.Add($"{i2}if (i % {per} == 0)");
                lines.Add($"{i2}{{");
                lines.Add($"{i3}int page = {spec.FirstPage} + i / {per};");
                lines.Add($"{i3}{c.P}AddPage(page);");
                lines.Add($"{i3}if (page > {spec.FirstPage})");
                lines.Add($"{i3}{{");
                lines.Add($"{i3}    {c.P}AddButton({back?.X ?? spec.NavBackX}, {back?.Y ?? spec.NavY}, {back?.GumpId ?? 4014}, {back?.GumpId2 ?? 4016}, 0, GumpButtonType.Page, page - 1); // back");
                lines.Add($"{i3}}}");
                lines.Add($"{i3}if (i + {per} < {spec.Rows})");
                lines.Add($"{i3}{{");
                lines.Add($"{i3}    {c.P}AddButton({next?.X ?? spec.NavNextX}, {next?.Y ?? spec.NavY}, {next?.GumpId ?? 4005}, {next?.GumpId2 ?? 4007}, 0, GumpButtonType.Page, page + 1); // next");
                lines.Add($"{i3}}}");
                lines.Add($"{i2}}}");
                lines.Add($"{i2}int y = {spec.Y} + i % {per} * {spec.RowHeight};");
            }
            else
            {
                lines.Add($"{i2}int y = {spec.Y} + i * {spec.RowHeight};");
            }
            foreach (GumpElement t in template)
            {
                int dy = t.Y - spec.Y;
                string y = dy == 0 ? "y" : dy > 0 ? $"y + {dy}" : $"y - {-dy}";
                string id = t.Type switch
                {
                    ElementType.Button or ElementType.ItemButton when t.ButtonType == ButtonKind.Reply => $"{spec.ButtonBase} + i",
                    ElementType.Check or ElementType.Radio => $"{spec.SwitchBase} + i",
                    ElementType.TextEntry => $"{spec.EntryBase} + i",
                    _ => null
                };
                string text = t.ListColumn >= 0 ? $"{L}Column{t.ListColumn + 1}[i]" : null;
                lines.Add(i2 + AddCall(t, c, null, y, id, text));
            }
            lines.Add($"{indent}}}");
            return lines;
        }

        /// <summary>One Add… call; x / y / id / text can be replaced by expressions (lists).</summary>
        private static string AddCall(GumpElement e, Ctx c, string xExpr, string yExpr, string idExpr, string textExpr)
        {
            string x = xExpr ?? N(e.X), y = yExpr ?? N(e.Y), p = c.P;
            string id(int value) => idExpr ?? (c.Names.TryGetValue(e, out string n) ? n : N(value));
            string text = textExpr ?? TextExpr(e, c);
            switch (e.Type)
            {
                case ElementType.Background: return $"{p}AddBackground({x}, {y}, {e.Width}, {e.Height}, {e.GumpId});";
                case ElementType.Image: return e.Hue > 0 ? $"{p}AddImage({x}, {y}, {e.GumpId}, {e.Hue});" : $"{p}AddImage({x}, {y}, {e.GumpId});";
                case ElementType.ImageTiled: return $"{p}AddImageTiled({x}, {y}, {e.Width}, {e.Height}, {e.GumpId});";
                case ElementType.SpriteImage: return $"{p}AddSpriteImage({x}, {y}, {e.GumpId}, {e.Width}, {e.Height}, {e.SourceX}, {e.SourceY});";
                case ElementType.AlphaRegion: return $"{p}AddAlphaRegion({x}, {y}, {e.Width}, {e.Height});";
                case ElementType.Label: return $"{p}AddLabel({x}, {y}, {e.Hue}, {text});";
                case ElementType.LabelCropped: return $"{p}AddLabelCropped({x}, {y}, {e.Width}, {e.Height}, {e.Hue}, {text});";
                case ElementType.Html:
                    return c.Modern
                        ? $"builder.AddHtml({x}, {y}, {e.Width}, {e.Height}, {text}" + (e.HtmlBackground ? ", background: true" : string.Empty) + (e.Scrollbar ? ", scrollbar: true" : string.Empty) + ");"
                        : $"AddHtml({x}, {y}, {e.Width}, {e.Height}, {text}, {B(e.HtmlBackground)}, {B(e.Scrollbar)});";
                case ElementType.HtmlLocalized:
                    return e.Hue > 0
                        ? $"{p}AddHtmlLocalized({x}, {y}, {e.Width}, {e.Height}, {e.Cliloc}, 0x{e.Hue:X4}, {B(e.HtmlBackground)}, {B(e.Scrollbar)});"
                        : $"{p}AddHtmlLocalized({x}, {y}, {e.Width}, {e.Height}, {e.Cliloc}, {B(e.HtmlBackground)}, {B(e.Scrollbar)});";
                case ElementType.Button:
                    if (e.ButtonType == ButtonKind.Page)
                    {
                        return $"{p}AddButton({x}, {y}, {e.GumpId}, {e.GumpId2}, 0, GumpButtonType.Page, {e.TargetPage});";
                    }
                    return c.Modern && e.TargetPage == 0
                        ? $"builder.AddButton({x}, {y}, {e.GumpId}, {e.GumpId2}, {id(e.ButtonId)});"
                        : $"{p}AddButton({x}, {y}, {e.GumpId}, {e.GumpId2}, {id(e.ButtonId)}, GumpButtonType.Reply, {e.TargetPage});";
                case ElementType.ItemButton:
                    return $"{p}AddImageTiledButton({x}, {y}, {e.GumpId}, {e.GumpId2}, {(e.ButtonType == ButtonKind.Page ? "0" : id(e.ButtonId))}, GumpButtonType.{e.ButtonType}, {e.TargetPage}, {Hex(e.ItemId)}, {e.Hue}, {e.Width}, {e.Height}" +
                           (e.Cliloc > 0 ? $", {e.Cliloc});" : ");");
                case ElementType.Check: return $"{p}{(c.Modern ? "AddCheckbox" : "AddCheck")}({x}, {y}, {e.GumpId}, {e.GumpId2}, {B(e.Checked)}, {id(e.SwitchId)});";
                case ElementType.Radio: return $"{p}AddRadio({x}, {y}, {e.GumpId}, {e.GumpId2}, {B(e.Checked)}, {id(e.SwitchId)});";
                case ElementType.TextEntry:
                    if (e.MaxLength > 0)
                    {
                        return c.Modern
                            ? $"builder.AddTextEntryLimited({x}, {y}, {e.Width}, {e.Height}, {e.Hue}, {id(e.EntryId)}, {text}, {e.MaxLength});"
                            : $"AddTextEntry({x}, {y}, {e.Width}, {e.Height}, {e.Hue}, {id(e.EntryId)}, {text}, {e.MaxLength});";
                    }
                    return $"{p}AddTextEntry({x}, {y}, {e.Width}, {e.Height}, {e.Hue}, {id(e.EntryId)}, {text});";
                case ElementType.Item: return e.Hue > 0 ? $"{p}AddItem({x}, {y}, {Hex(e.ItemId)}, {e.Hue});" : $"{p}AddItem({x}, {y}, {Hex(e.ItemId)});";
                default: return "// ?";
            }
        }

        /// <summary>Named constants for reply buttons, switches and text entries (only for elements with a name, not list rows).</summary>
        private static Dictionary<GumpElement, string> ConstantNames(GumpDesign d)
        {
            var map = new Dictionary<GumpElement, string>();
            var used = new HashSet<string>();
            foreach (GumpElement e in d.Elements)
            {
                if (string.IsNullOrWhiteSpace(e.Name) || !string.IsNullOrEmpty(e.ListId))
                {
                    continue;
                }
                string prefix = e.Type switch
                {
                    ElementType.Button or ElementType.ItemButton when e.ButtonType == ButtonKind.Reply => "Button",
                    ElementType.Check or ElementType.Radio => "Switch",
                    ElementType.TextEntry => "Entry",
                    _ => null
                };
                if (prefix == null)
                {
                    continue;
                }
                string n = prefix + Identifier(e.Name, prefix);
                string unique = n;
                for (int i = 2; used.Contains(unique); i++)
                {
                    unique = n + i;
                }
                used.Add(unique);
                map[e] = unique;
            }
            return map;
        }

        // ------------------------------------------------------------------ OnResponse

        private static string CtorArgs(GumpDesign d) => d.PassMobile ? "from" : string.Empty;

        /// <summary>The code of a button action (without break).</summary>
        private static List<string> ActionLines(GumpElement e, GumpDesign d, string cls, string what)
        {
            var l = new List<string>();
            string target = (e.ActionTarget ?? string.Empty).Trim();
            switch (e.Action)
            {
                case ButtonAction.None:
                    l.Add($"from.SendMessage(\"{Regex.Replace(what, "[\"\\\\]", string.Empty)} pressed.\");");
                    l.Add($"// from.SendGump(new {cls}({CtorArgs(d)})); // open again");
                    break;
                case ButtonAction.Close:
                    l.Add("// the gump is closed");
                    break;
                case ButtonAction.Reopen:
                    l.Add($"from.SendGump(new {cls}({CtorArgs(d)}));");
                    break;
                case ButtonAction.OpenGump:
                    if (target.Length == 0)
                    {
                        l.Add("// Action target: class name of the gump to open");
                    }
                    else
                    {
                        string gump = target.Contains("(") ? target : $"{Identifier(target, "MyGump")}(from)";
                        l.Add($"from.SendGump(new {gump});");
                    }
                    break;
                case ButtonAction.Teleport:
                {
                    string[] parts = (e.ActionLocation ?? string.Empty).Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && parts.Take(Math.Min(3, parts.Length)).All(x => int.TryParse(x, out _)))
                    {
                        string z = parts.Length > 2 && int.TryParse(parts[2], out _) ? parts[2] : "0";
                        string mapName = parts.Skip(2).FirstOrDefault(x => !int.TryParse(x, out _));
                        string map = MapNames.FirstOrDefault(m => string.Equals(m, mapName, StringComparison.OrdinalIgnoreCase));
                        l.Add($"from.MoveToWorld(new Point3D({parts[0]}, {parts[1]}, {z}), {(map != null ? "Map." + map : "from.Map")});");
                    }
                    else
                    {
                        l.Add("// Action location: \"x y z map\", e.g. \"1434 1699 2 Felucca\"");
                    }
                    break;
                }
                case ButtonAction.GiveItem:
                    if (target.Length == 0)
                    {
                        l.Add("// Action target: item type, e.g. Gold(100)");
                    }
                    else
                    {
                        string item = target.Contains("(") ? target : Identifier(target, "Gold") + "()";
                        l.Add($"from.AddToBackpack(new {item});");
                    }
                    break;
                case ButtonAction.RunCommand:
                    l.Add($"CommandSystem.Handle(from, CommandSystem.Prefix + {Str(target.TrimStart('['))});");
                    break;
                case ButtonAction.Message:
                    l.Add($"from.SendMessage({Str(target)});");
                    break;
                case ButtonAction.CustomCode:
                    l.AddRange(target.Replace("\\n", "\n").Replace("\r", string.Empty).Split('\n').Select(x => x.TrimEnd()).Where(x => x.Length > 0));
                    if (l.Count == 0)
                    {
                        l.Add("// your code (Action target)");
                    }
                    break;
            }
            if (e.ActionReopen && e.Action != ButtonAction.Reopen)
            {
                l.Add($"from.SendGump(new {cls}({CtorArgs(d)}));");
            }
            return l;
        }

        /// <summary>Body of OnResponse (after "from" is known): text entries, switches, list rows, switch over the buttons.</summary>
        private static List<string> ResponseBody(GumpDesign d, Ctx c, string cls, string indent)
        {
            var lines = new List<string>();
            void L(string s = "") => lines.Add(s.Length == 0 ? string.Empty : indent + s);
            List<GumpElement> replies = d.Elements.Where(e => (e.Type == ElementType.Button || e.Type == ElementType.ItemButton) && e.ButtonType == ButtonKind.Reply && e.ButtonId > 0 && string.IsNullOrEmpty(e.ListId)).ToList();
            List<GumpElement> switches = d.Elements.Where(e => (e.Type == ElementType.Check || e.Type == ElementType.Radio) && string.IsNullOrEmpty(e.ListId)).ToList();
            List<GumpElement> entries = d.Elements.Where(e => e.Type == ElementType.TextEntry && string.IsNullOrEmpty(e.ListId)).ToList();
            foreach (GumpElement e in entries)
            {
                string var = "text" + Identifier(e.Name, "Entry" + e.EntryId);
                string idText = c.Names.TryGetValue(e, out string n) ? n : N(e.EntryId);
                if (c.Modern)
                {
                    L($"string {var} = info.GetTextEntry({idText})?.Trim() ?? string.Empty;");
                }
                else
                {
                    L($"TextRelay {var}Relay = info.GetTextEntry({idText});");
                    L($"string {var} = {var}Relay != null ? {var}Relay.Text.Trim() : string.Empty;");
                }
            }
            foreach (GumpElement e in switches)
            {
                string var = "switch" + Identifier(e.Name, (e.Type == ElementType.Radio ? "Radio" : "Check") + e.SwitchId);
                string idText = c.Names.TryGetValue(e, out string n) ? n : N(e.SwitchId);
                L($"bool {var} = info.IsSwitched({idText});");
            }
            if (entries.Count + switches.Count > 0)
            {
                L();
            }
            foreach (GumpListSpec spec in d.Lists ?? new List<GumpListSpec>())
            {
                List<GumpElement> template = GumpLists.Template(d, spec);
                if (template.Any(t => (t.Type == ElementType.Button || t.Type == ElementType.ItemButton) && t.ButtonType == ButtonKind.Reply))
                {
                    L($"if (info.ButtonID >= {spec.ButtonBase} && info.ButtonID < {spec.ButtonBase + spec.Rows}) // {spec.Name}");
                    L("{");
                    L($"    int row = info.ButtonID - {spec.ButtonBase};");
                    L($"    from.SendMessage(\"{Regex.Replace(spec.Name, "[\"\\\\]", string.Empty)}: row \" + (row + 1) + \" chosen.\");");
                    L("    return;");
                    L("}");
                    L();
                }
                if (template.Any(t => t.Type == ElementType.Check || t.Type == ElementType.Radio))
                {
                    L($"// {spec.Name}: row r is ticked when info.IsSwitched({spec.SwitchBase} + r)");
                }
                if (template.Any(t => t.Type == ElementType.TextEntry))
                {
                    L($"// {spec.Name}: text of row r = info.GetTextEntry({spec.EntryBase} + r)");
                }
            }
            L("switch (info.ButtonID)");
            L("{");
            L("    case 0: // closed (right click or a button with ID 0)");
            L("    {");
            L("        break;");
            L("    }");
            foreach (IGrouping<int, GumpElement> grp in replies.GroupBy(e => e.ButtonId).OrderBy(g => g.Key))
            {
                GumpElement e = grp.First();
                string label = c.Names.TryGetValue(e, out string n) ? n : N(grp.Key);
                string what = string.Join(", ", grp.Select(x => string.IsNullOrEmpty(x.Name) ? $"button at {x.X}, {x.Y}" : x.Name));
                L($"    case {label}: // {what}" + (e.Action != ButtonAction.None ? $" → {e.Action}" : string.Empty));
                L("    {");
                foreach (string a in ActionLines(e, d, cls, what))
                {
                    L("        " + a);
                }
                L("        break;");
                L("    }");
            }
            L("}");
            return lines;
        }

        private static IEnumerable<string> TestLogComment(GumpDesign d, string indent)
        {
            if (!d.IncludeTestLog || d.TestLog == null || d.TestLog.Count == 0)
            {
                yield break;
            }
            yield return indent + "// Test (Gump Studio, F5) – what OnResponse gets:";
            foreach (string t in d.TestLog.Take(50))
            {
                yield return indent + "//   " + t;
            }
        }

        // ------------------------------------------------------------------ ServUO

        private static string ServUO(GumpDesign d)
        {
            string cls = Identifier(d.ClassName, "MyGump");
            var c = new Ctx(d, false) { LanguageVariable = Bilingual(d) };
            var sb = new StringBuilder();
            void L(string s = "") => sb.Append(s).Append("\r\n");

            L("using System;");
            L("using Server;");
            L("using Server.Commands;");
            L("using Server.Gumps;");
            L("using Server.Items;");
            L("using Server.Mobiles;");
            L("using Server.Network;");
            L();
            L($"namespace {(string.IsNullOrWhiteSpace(d.Namespace) ? "Server.Gumps" : d.Namespace.Trim())}");
            L("{");
            L($"    public class {cls} : Gump");
            L("    {");
            if (c.Names.Count > 0)
            {
                foreach (KeyValuePair<GumpElement, string> kv in c.Names)
                {
                    L($"        private const int {kv.Value} = {ConstValue(kv.Key)};");
                }
                L();
            }
            if (!string.IsNullOrWhiteSpace(d.Command))
            {
                string cmd = Regex.Replace(d.Command.Trim().TrimStart('['), @"\s", string.Empty);
                L("        public static void Initialize()");
                L("        {");
                L($"            CommandSystem.Register(\"{cmd}\", AccessLevel.{(string.IsNullOrWhiteSpace(d.CommandAccess) ? "GameMaster" : d.CommandAccess)}, e =>");
                L("            {");
                if (d.CloseExisting)
                {
                    L($"                e.Mobile.CloseGump(typeof({cls}));");
                }
                L($"                e.Mobile.SendGump(new {cls}({(d.PassMobile ? "e.Mobile" : string.Empty)}));");
                L("            });");
                L("        }");
                L();
            }
            if (d.PassMobile)
            {
                L("        private readonly Mobile m_From;");
                L();
            }
            L($"        public {cls}({(d.PassMobile ? "Mobile from" : string.Empty)})");
            L($"            : base({d.X}, {d.Y})");
            L("        {");
            if (d.PassMobile)
            {
                L("            m_From = from;");
                L();
            }
            if (c.LanguageVariable)
            {
                L(d.PassMobile
                    ? $"            bool de = from != null && from.Language == \"{d.SecondLanguage.Trim().ToUpperInvariant()}\"; // second language"
                    : "            bool de = false; // switch on \"PassMobile\" to show the second language");
                L();
            }
            L($"            Closable = {B(d.Closable)};");
            L($"            Disposable = {B(d.Disposable)};");
            L($"            Dragable = {B(d.Dragable)};");
            L($"            Resizable = {B(d.Resizable)};");
            L();
            foreach (string line in AddLines(d, "            ", c))
            {
                L(line);
            }
            L("        }");
            L();
            foreach (string t in TestLogComment(d, "        "))
            {
                L(t);
            }
            L("        public override void OnResponse(NetState sender, RelayInfo info)");
            L("        {");
            L("            Mobile from = sender.Mobile;");
            L();
            L("            if (from == null)");
            L("            {");
            L("                return;");
            L("            }");
            L();
            foreach (string line in ResponseBody(d, c, cls, "            "))
            {
                L(line);
            }
            L("        }");
            L("    }");
            L("}");
            return sb.ToString();
        }

        private static int ConstValue(GumpElement e) =>
            e.Type == ElementType.TextEntry ? e.EntryId : e.Type == ElementType.Check || e.Type == ElementType.Radio ? e.SwitchId : e.ButtonId;

        // ------------------------------------------------------------------ ModernUO

        /// <summary>ModernUO: DynamicGump with BuildLayout(ref DynamicGumpBuilder builder), command in Configure().</summary>
        private static string ModernUO(GumpDesign d)
        {
            string cls = Identifier(d.ClassName, "MyGump");
            var c = new Ctx(d, true) { LanguageVariable = Bilingual(d) };
            var sb = new StringBuilder();
            void L(string s = "") => sb.Append(s).Append("\r\n");

            L("using System;");
            L("using Server;");
            L("using Server.Gumps;");
            L("using Server.Items;");
            L("using Server.Mobiles;");
            L("using Server.Network;");
            L();
            L($"namespace {(string.IsNullOrWhiteSpace(d.Namespace) ? "Server.Gumps" : d.Namespace.Trim())};");
            L();
            L($"public class {cls} : DynamicGump");
            L("{");
            if (c.Names.Count > 0)
            {
                foreach (KeyValuePair<GumpElement, string> kv in c.Names)
                {
                    L($"    private const int {kv.Value} = {ConstValue(kv.Key)};");
                }
                L();
            }
            if (d.PassMobile)
            {
                L("    private readonly Mobile _from;");
                L();
            }
            if (!string.IsNullOrWhiteSpace(d.Command))
            {
                string cmd = Regex.Replace(d.Command.Trim().TrimStart('['), @"\s", string.Empty);
                L("    public static void Configure()");
                L("    {");
                L($"        CommandSystem.Register(\"{cmd}\", AccessLevel.{(string.IsNullOrWhiteSpace(d.CommandAccess) ? "GameMaster" : d.CommandAccess)}, OnCommand);");
                L("    }");
                L();
                L($"    [Usage(\"{cmd}\")]");
                L($"    [Description(\"Opens the {cls} gump.\")]");
                L("    private static void OnCommand(CommandEventArgs e)");
                L("    {");
                L($"        e.Mobile.SendGump(new {cls}({(d.PassMobile ? "e.Mobile" : string.Empty)}));");
                L("    }");
                L();
            }
            if (d.CloseExisting)
            {
                L("    public override bool Singleton => true; // only one of these gumps is open at a time");
                L();
            }
            L($"    public {cls}({(d.PassMobile ? "Mobile from" : string.Empty)}) : base({d.X}, {d.Y})");
            L("    {");
            if (d.PassMobile)
            {
                L("        _from = from;");
            }
            L("    }");
            L();
            L("    protected override void BuildLayout(ref DynamicGumpBuilder builder)");
            L("    {");
            if (!d.Closable) L("        builder.SetNoClose();");
            if (!d.Dragable) L("        builder.SetNoMove();");
            if (!d.Disposable) L("        builder.SetNoDispose();");
            if (!d.Resizable) L("        builder.SetNoResize();");
            if (c.LanguageVariable)
            {
                L(d.PassMobile
                    ? $"        var de = _from?.Language == \"{d.SecondLanguage.Trim().ToUpperInvariant()}\"; // second language"
                    : "        var de = false; // switch on \"PassMobile\" to show the second language");
            }
            L();
            foreach (string line in AddLines(d, "        ", c))
            {
                L(line);
            }
            L("    }");
            L();
            foreach (string t in TestLogComment(d, "    "))
            {
                L(t);
            }
            L("    public override void OnResponse(NetState sender, in RelayInfo info)");
            L("    {");
            L("        var from = sender.Mobile;");
            L();
            L("        if (from == null)");
            L("        {");
            L("            return;");
            L("        }");
            L();
            foreach (string line in ResponseBody(d, c, cls, "        "))
            {
                L(line);
            }
            L("    }");
            L("}");
            return sb.ToString();
        }

        // ------------------------------------------------------------------ Sphere

        private static string Sphere(GumpDesign d)
        {
            string name = "d_" + Regex.Replace((d.ClassName ?? "gump").ToLowerInvariant(), @"[^a-z0-9_]", "_");
            var texts = new List<string>();
            int T(string s)
            {
                texts.Add((s ?? string.Empty).Replace("\r", string.Empty).Replace("\n", " "));
                return texts.Count - 1;
            }
            var sb = new StringBuilder();
            void L(string s = "") => sb.Append(s).Append("\r\n");
            L($"[DIALOG {name}]");
            L($"{d.X},{d.Y}");
            if (!d.Closable) L("noclose");
            if (!d.Dragable) L("nomove");
            if (!d.Disposable) L("nodispose");
            int maxPage = d.Pages;
            bool anyPages = d.Elements.Any(e => e.Page > 0);
            for (int page = 0; page <= maxPage; page++)
            {
                if (page > 0 && !anyPages)
                {
                    break;
                }
                if (anyPages)
                {
                    L($"page {page}");
                }
                int group = -1;
                foreach (GumpElement e in d.Elements.Where(x => x.Page == page))
                {
                    switch (e.Type)
                    {
                        case ElementType.Background: L($"resizepic {e.X} {e.Y} {e.GumpId} {e.Width} {e.Height}"); break;
                        case ElementType.Image: L(e.Hue > 0 ? $"gumppic {e.X} {e.Y} {e.GumpId} hue={e.Hue}" : $"gumppic {e.X} {e.Y} {e.GumpId}"); break;
                        case ElementType.ImageTiled: L($"gumppictiled {e.X} {e.Y} {e.Width} {e.Height} {e.GumpId}"); break;
                        case ElementType.AlphaRegion: L($"checkertrans {e.X} {e.Y} {e.Width} {e.Height}"); break;
                        case ElementType.Label: L($"text {e.X} {e.Y} {e.Hue} {T(e.Text)}"); break;
                        case ElementType.LabelCropped: L($"croppedtext {e.X} {e.Y} {e.Width} {e.Height} {e.Hue} {T(e.Text)}"); break;
                        case ElementType.Html: L($"htmlgump {e.X} {e.Y} {e.Width} {e.Height} {T(e.Text)} {(e.HtmlBackground ? 1 : 0)} {(e.Scrollbar ? 1 : 0)}"); break;
                        case ElementType.HtmlLocalized: L($"xmfhtmlgump {e.X} {e.Y} {e.Width} {e.Height} {e.Cliloc} {(e.HtmlBackground ? 1 : 0)} {(e.Scrollbar ? 1 : 0)}"); break;
                        case ElementType.Button:
                        case ElementType.ItemButton:
                            L(e.ButtonType == ButtonKind.Page
                                ? $"button {e.X} {e.Y} {e.GumpId} {e.GumpId2} 0 {e.TargetPage} 0"
                                : $"button {e.X} {e.Y} {e.GumpId} {e.GumpId2} 1 0 {e.ButtonId}");
                            break;
                        case ElementType.Check: L($"checkbox {e.X} {e.Y} {e.GumpId} {e.GumpId2} {(e.Checked ? 1 : 0)} {e.SwitchId}"); break;
                        case ElementType.Radio:
                            if (e.Group != group)
                            {
                                group = e.Group;
                                L($"group {group}");
                            }
                            L($"radio {e.X} {e.Y} {e.GumpId} {e.GumpId2} {(e.Checked ? 1 : 0)} {e.SwitchId}");
                            break;
                        case ElementType.TextEntry: L($"textentry {e.X} {e.Y} {e.Width} {e.Height} {e.Hue} {e.EntryId} {T(e.Text)}"); break;
                        case ElementType.Item: L(e.Hue > 0 ? $"tilepichue {e.X} {e.Y} {e.ItemId} {e.Hue}" : $"tilepic {e.X} {e.Y} {e.ItemId}"); break;
                        case ElementType.SpriteImage: L($"gumppic {e.X} {e.Y} {e.GumpId} // sprite part {e.SourceX},{e.SourceY} {e.Width}x{e.Height}"); break;
                    }
                }
            }
            L();
            L($"[DIALOG {name} TEXT]");
            foreach (string t in texts)
            {
                L(t);
            }
            L();
            L($"[DIALOG {name} BUTTON]");
            foreach (IGrouping<int, GumpElement> g in d.Elements.Where(e => (e.Type == ElementType.Button || e.Type == ElementType.ItemButton) && e.ButtonType == ButtonKind.Reply)
                         .GroupBy(e => e.ButtonId).OrderBy(g => g.Key))
            {
                L($"ON={g.Key} // {string.Join(", ", g.Select(x => string.IsNullOrEmpty(x.Name) ? $"{x.X},{x.Y}" : x.Name))}");
                foreach (GumpElement e in d.Elements.Where(e => e.Type == ElementType.TextEntry))
                {
                    L($"// <ARGTXT[{e.EntryId}]> = text of {(string.IsNullOrEmpty(e.Name) ? "entry " + e.EntryId : e.Name)}");
                }
                L("SYSMESSAGE Button " + g.Key);
                L();
            }
            return sb.ToString();
        }

        // ------------------------------------------------------------------ checks

        private static string Plain(string html) => Regex.Replace(Regex.Replace(html ?? string.Empty, @"<BR\s*/?>", "\n", RegexOptions.IgnoreCase), "<[^>]*>", string.Empty);

        /// <summary>Things the client does differently than the designer shows: cut texts, sizes, newer client features.</summary>
        private static void ClientLimits(GumpDesign d, List<string> p)
        {
            List<GumpElement> all = d.Elements.Where(e => e.ListRow <= 0).ToList();
            if (d.Elements.Count == 0)
            {
                return;
            }
            int right = d.Elements.Max(e => e.X + Math.Max(e.Width, 20)) + d.X;
            int bottom = d.Elements.Max(e => e.Y + Math.Max(e.Height, 20)) + d.Y;
            if (right > 800 || bottom > 600)
            {
                p.Add($"The gump reaches to {right} × {bottom} on the screen (with its position {d.X}, {d.Y}) – too big for a small 800 × 600 client window.");
            }
            if (d.Elements.Count > 800)
            {
                p.Add($"{d.Elements.Count} entries – very big gumps are slow and can reach the packet size limit; use pages or lists.");
            }
            List<GumpElement> backgrounds = d.Elements.Where(e => e.Type == ElementType.Background).ToList();
            foreach (GumpElement e in all)
            {
                string where = $"{e.Type} at {e.X}, {e.Y}";
                switch (e.Type)
                {
                    case ElementType.Label:
                    {
                        int width = (e.Text ?? string.Empty).Length * 7;
                        GumpElement bg = backgrounds.LastOrDefault(b => (b.Page == 0 || b.Page == e.Page || e.Page == 0) && new System.Drawing.Rectangle(b.X, b.Y, b.Width, b.Height).Contains(e.X, e.Y));
                        if (bg != null && e.X + width > bg.X + bg.Width)
                        {
                            p.Add($"Label \"{Short(e.Text)}\" probably runs over the right edge of its background – use LabelCropped or HTML.");
                        }
                        break;
                    }
                    case ElementType.LabelCropped:
                        if ((e.Text ?? string.Empty).Length * 7 > e.Width)
                        {
                            p.Add($"Cropped label \"{Short(e.Text)}\" is cut at {e.Width} px (the client shows only the start).");
                        }
                        break;
                    case ElementType.Html when !e.Scrollbar:
                    {
                        string text = Plain(e.Text);
                        int charsPerLine = Math.Max(1, (e.Width - 4) / 7);
                        int lines = text.Split('\n').Sum(l => Math.Max(1, (int)Math.Ceiling(l.Length / (double)charsPerLine)));
                        if (lines * 18 > e.Height + 4)
                        {
                            p.Add($"HTML at {e.X}, {e.Y}: about {lines} lines do not fit into {e.Height} px – switch on Scrollbar or make it higher.");
                        }
                        break;
                    }
                    case ElementType.TextEntry when e.MaxLength == 0:
                        p.Add($"Text entry {(string.IsNullOrEmpty(e.Name) ? "at " + e.X + ", " + e.Y : e.Name)} has no length limit – set MaxLength so players cannot send very long text.");
                        break;
                    case ElementType.Item when e.ItemId > 0x3FFF:
                    case ElementType.ItemButton when e.ItemId > 0x3FFF:
                        p.Add($"{where}: item 0x{e.ItemId:X} is newer art (above 0x3FFF) – old clients do not have it.");
                        break;
                }
                if (e.Type == ElementType.ItemButton || e.Type == ElementType.SpriteImage)
                {
                    p.Add($"{where}: {(e.Type == ElementType.ItemButton ? "AddImageTiledButton" : "AddSpriteImage")} is only shown by newer clients.");
                }
                if (!string.IsNullOrEmpty(e.TooltipText))
                {
                    p.Add($"{where}: text tooltips are only shown by newer clients – a cliloc tooltip works everywhere.");
                }
                if ((e.Type == ElementType.Button || e.Type == ElementType.ItemButton) && e.ButtonType == ButtonKind.Reply)
                {
                    if (e.Action == ButtonAction.Teleport && !Regex.IsMatch(e.ActionLocation ?? string.Empty, @"^\s*-?\d+[\s,;]+-?\d+"))
                    {
                        p.Add($"Button {(string.IsNullOrEmpty(e.Name) ? e.ButtonId.ToString(CultureInfo.InvariantCulture) : e.Name)}: Teleport needs \"x y z map\" in Action location.");
                    }
                    if ((e.Action == ButtonAction.OpenGump || e.Action == ButtonAction.GiveItem || e.Action == ButtonAction.RunCommand || e.Action == ButtonAction.Message) && string.IsNullOrWhiteSpace(e.ActionTarget))
                    {
                        p.Add($"Button {(string.IsNullOrEmpty(e.Name) ? e.ButtonId.ToString(CultureInfo.InvariantCulture) : e.Name)}: {e.Action} needs an Action target.");
                    }
                }
            }
        }

        private static string Short(string s) => (s ?? string.Empty).Length > 30 ? s.Substring(0, 30) + "…" : s ?? string.Empty;

        /// <summary>Problems that would make the gump fail or behave oddly.</summary>
        public static List<string> Validate(GumpDesign d)
        {
            var p = new List<string>();
            if (!Regex.IsMatch(d.ClassName ?? string.Empty, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                p.Add("Class name is not a valid C# name.");
            }
            foreach (IGrouping<int, GumpElement> g in d.Elements.Where(e => (e.Type == ElementType.Button || e.Type == ElementType.ItemButton) && e.ButtonType == ButtonKind.Reply && e.ButtonId > 0)
                         .GroupBy(e => e.ButtonId).Where(g => g.Count() > 1))
            {
                p.Add($"Button ID {g.Key} is used {g.Count()} times (fine only if they do the same).");
            }
            foreach (IGrouping<int, GumpElement> g in d.Elements.Where(e => e.Type == ElementType.Check || e.Type == ElementType.Radio).GroupBy(e => e.SwitchId).Where(g => g.Count() > 1))
            {
                p.Add($"Switch ID {g.Key} is used {g.Count()} times – info.IsSwitched cannot tell them apart.");
            }
            foreach (IGrouping<int, GumpElement> g in d.Elements.Where(e => e.Type == ElementType.TextEntry).GroupBy(e => e.EntryId).Where(g => g.Count() > 1))
            {
                p.Add($"Text entry ID {g.Key} is used {g.Count()} times.");
            }
            int pages = d.Pages;
            foreach (GumpElement e in d.Elements.Where(e => (e.Type == ElementType.Button || e.Type == ElementType.ItemButton) && e.ButtonType == ButtonKind.Page))
            {
                if (e.TargetPage < 1 || e.TargetPage > pages)
                {
                    p.Add($"Page button at {e.X}, {e.Y} goes to page {e.TargetPage}, which has no entries.");
                }
            }
            foreach (GumpElement e in d.Elements)
            {
                if (e.X < 0 || e.Y < 0)
                {
                    p.Add($"{e.Type} at {e.X}, {e.Y} is outside the gump (negative position).");
                }
                if ((e.Type == ElementType.Image || e.Type == ElementType.Button || e.Type == ElementType.Check || e.Type == ElementType.Radio || e.Type == ElementType.ImageTiled)
                    && GumpRenderer.Gump(e.GumpId) == null)
                {
                    p.Add($"{e.Type} at {e.X}, {e.Y}: gump {e.GumpId} does not exist in your gumpart.");
                }
                if (e.Type == ElementType.Background && Enumerable.Range(0, 9).All(i => GumpRenderer.Gump(e.GumpId + i) == null))
                {
                    p.Add($"Background at {e.X}, {e.Y}: gumps {e.GumpId} … {e.GumpId + 8} do not exist.");
                }
                if (e.Type == ElementType.Item && GumpRenderer.Item(e.ItemId) == null)
                {
                    p.Add($"Item at {e.X}, {e.Y}: item 0x{e.ItemId:X4} does not exist.");
                }
                if ((e.Type == ElementType.Label || e.Type == ElementType.LabelCropped) && ((e.Text ?? string.Empty) + (e.TextDe ?? string.Empty)).Any(c => c > 127))
                {
                    p.Add($"Label \"{e.Text}\" has special characters – ServUO sends labels as ASCII (use AddHtml for umlauts).");
                }
            }
            ClientLimits(d, p);
            return p;
        }
    }
}

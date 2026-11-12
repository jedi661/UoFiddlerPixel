using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    public enum CodeTarget { ServUO, ServUOAddCallsOnly, Sphere }

    /// <summary>Writes the gump as ServUO script (class with constructor, OnResponse, command) or as Sphere dialog.</summary>
    public static class GumpCodeGenerator
    {
        public static readonly string[] TargetNames = { "ServUO / RunUO – complete gump class", "ServUO – only the Add… lines", "Sphere – [DIALOG] script" };

        private static string Str(string s) => "\"" + (s ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", string.Empty).Replace("\n", "\\n") + "\"";

        private static string B(bool b) => b ? "true" : "false";

        private static string Hex(int v) => v < 10 ? v.ToString(CultureInfo.InvariantCulture) : "0x" + v.ToString("X", CultureInfo.InvariantCulture);

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

        public static string Generate(GumpDesign d, CodeTarget target) => target switch
        {
            CodeTarget.Sphere => Sphere(d),
            CodeTarget.ServUOAddCallsOnly => string.Join("\r\n", AddLines(d, "")),
            _ => ServUO(d)
        };

        // ------------------------------------------------------------------ ServUO

        /// <summary>The Add… lines in client order: page 0 first, then AddPage(n) with its entries.</summary>
        public static List<string> AddLines(GumpDesign d, string indent)
        {
            var lines = new List<string>();
            var names = ConstantNames(d);
            int maxPage = d.Pages;
            bool anyPages = d.Elements.Any(e => e.Page > 0);
            for (int page = 0; page <= maxPage; page++)
            {
                List<GumpElement> els = d.Elements.Where(e => e.Page == page).ToList();
                if (page > 0)
                {
                    if (!anyPages)
                    {
                        break;
                    }
                    lines.Add(string.Empty);
                    lines.Add($"{indent}AddPage({page});");
                }
                else if (anyPages)
                {
                    lines.Add($"{indent}AddPage(0);");
                }
                int group = -1;
                foreach (GumpElement e in els)
                {
                    if (e.Type == ElementType.Radio && e.Group != group)
                    {
                        group = e.Group;
                        lines.Add($"{indent}AddGroup({group});");
                    }
                    string comment = string.IsNullOrEmpty(e.Name) ? string.Empty : " // " + e.Name;
                    lines.Add(indent + AddCall(e, names) + comment);
                    if (e.TooltipCliloc > 0)
                    {
                        lines.Add($"{indent}AddTooltip({e.TooltipCliloc});");
                    }
                    else if (!string.IsNullOrEmpty(e.TooltipText))
                    {
                        lines.Add($"{indent}AddTooltip({Str(e.TooltipText)});");
                    }
                }
            }
            return lines;
        }

        private static string AddCall(GumpElement e, Dictionary<GumpElement, string> names)
        {
            string id(int fallback) => names.TryGetValue(e, out string n) ? n : fallback.ToString(CultureInfo.InvariantCulture);
            switch (e.Type)
            {
                case ElementType.Background: return $"AddBackground({e.X}, {e.Y}, {e.Width}, {e.Height}, {e.GumpId});";
                case ElementType.Image: return e.Hue > 0 ? $"AddImage({e.X}, {e.Y}, {e.GumpId}, {e.Hue});" : $"AddImage({e.X}, {e.Y}, {e.GumpId});";
                case ElementType.ImageTiled: return $"AddImageTiled({e.X}, {e.Y}, {e.Width}, {e.Height}, {e.GumpId});";
                case ElementType.SpriteImage: return $"AddSpriteImage({e.X}, {e.Y}, {e.GumpId}, {e.Width}, {e.Height}, {e.SourceX}, {e.SourceY});";
                case ElementType.AlphaRegion: return $"AddAlphaRegion({e.X}, {e.Y}, {e.Width}, {e.Height});";
                case ElementType.Label: return $"AddLabel({e.X}, {e.Y}, {e.Hue}, {Str(e.Text)});";
                case ElementType.LabelCropped: return $"AddLabelCropped({e.X}, {e.Y}, {e.Width}, {e.Height}, {e.Hue}, {Str(e.Text)});";
                case ElementType.Html: return $"AddHtml({e.X}, {e.Y}, {e.Width}, {e.Height}, {Str(e.Text)}, {B(e.HtmlBackground)}, {B(e.Scrollbar)});";
                case ElementType.HtmlLocalized:
                    return e.Hue > 0
                        ? $"AddHtmlLocalized({e.X}, {e.Y}, {e.Width}, {e.Height}, {e.Cliloc}, 0x{e.Hue:X4}, {B(e.HtmlBackground)}, {B(e.Scrollbar)});"
                        : $"AddHtmlLocalized({e.X}, {e.Y}, {e.Width}, {e.Height}, {e.Cliloc}, {B(e.HtmlBackground)}, {B(e.Scrollbar)});";
                case ElementType.Button:
                    return e.ButtonType == ButtonKind.Page
                        ? $"AddButton({e.X}, {e.Y}, {e.GumpId}, {e.GumpId2}, 0, GumpButtonType.Page, {e.TargetPage});"
                        : $"AddButton({e.X}, {e.Y}, {e.GumpId}, {e.GumpId2}, {id(e.ButtonId)}, GumpButtonType.Reply, {e.TargetPage});";
                case ElementType.ItemButton:
                    return $"AddImageTiledButton({e.X}, {e.Y}, {e.GumpId}, {e.GumpId2}, {(e.ButtonType == ButtonKind.Page ? "0" : id(e.ButtonId))}, GumpButtonType.{e.ButtonType}, {e.TargetPage}, {Hex(e.ItemId)}, {e.Hue}, {e.Width}, {e.Height}" +
                           (e.Cliloc > 0 ? $", {e.Cliloc});" : ");");
                case ElementType.Check: return $"AddCheck({e.X}, {e.Y}, {e.GumpId}, {e.GumpId2}, {B(e.Checked)}, {id(e.SwitchId)});";
                case ElementType.Radio: return $"AddRadio({e.X}, {e.Y}, {e.GumpId}, {e.GumpId2}, {B(e.Checked)}, {id(e.SwitchId)});";
                case ElementType.TextEntry:
                    return e.MaxLength > 0
                        ? $"AddTextEntry({e.X}, {e.Y}, {e.Width}, {e.Height}, {e.Hue}, {id(e.EntryId)}, {Str(e.Text)}, {e.MaxLength});"
                        : $"AddTextEntry({e.X}, {e.Y}, {e.Width}, {e.Height}, {e.Hue}, {id(e.EntryId)}, {Str(e.Text)});";
                case ElementType.Item: return e.Hue > 0 ? $"AddItem({e.X}, {e.Y}, {Hex(e.ItemId)}, {e.Hue});" : $"AddItem({e.X}, {e.Y}, {Hex(e.ItemId)});";
                default: return "// ?";
            }
        }

        /// <summary>Named constants for reply buttons, switches and text entries (only for elements with a name).</summary>
        private static Dictionary<GumpElement, string> ConstantNames(GumpDesign d)
        {
            var map = new Dictionary<GumpElement, string>();
            var used = new HashSet<string>();
            foreach (GumpElement e in d.Elements)
            {
                if (string.IsNullOrWhiteSpace(e.Name))
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

        private static string ServUO(GumpDesign d)
        {
            string cls = Identifier(d.ClassName, "MyGump");
            var names = ConstantNames(d);
            var sb = new StringBuilder();
            void L(string s = "") => sb.Append(s).Append("\r\n");

            L("using System;");
            L("using Server;");
            L("using Server.Commands;");
            L("using Server.Gumps;");
            L("using Server.Mobiles;");
            L("using Server.Network;");
            L();
            L($"namespace {(string.IsNullOrWhiteSpace(d.Namespace) ? "Server.Gumps" : d.Namespace.Trim())}");
            L("{");
            L($"    public class {cls} : Gump");
            L("    {");
            List<GumpElement> replies = d.Elements.Where(e => (e.Type == ElementType.Button || e.Type == ElementType.ItemButton) && e.ButtonType == ButtonKind.Reply && e.ButtonId > 0).ToList();
            List<GumpElement> switches = d.Elements.Where(e => e.Type == ElementType.Check || e.Type == ElementType.Radio).ToList();
            List<GumpElement> entries = d.Elements.Where(e => e.Type == ElementType.TextEntry).ToList();
            if (names.Count > 0)
            {
                foreach (KeyValuePair<GumpElement, string> kv in names)
                {
                    int value = kv.Key.Type == ElementType.TextEntry ? kv.Key.EntryId : kv.Key.Type == ElementType.Check || kv.Key.Type == ElementType.Radio ? kv.Key.SwitchId : kv.Key.ButtonId;
                    L($"        private const int {kv.Value} = {value};");
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
            L($"            Closable = {B(d.Closable)};");
            L($"            Disposable = {B(d.Disposable)};");
            L($"            Dragable = {B(d.Dragable)};");
            L($"            Resizable = {B(d.Resizable)};");
            L();
            foreach (string line in AddLines(d, "            "))
            {
                L(line);
            }
            L("        }");
            L();
            L("        public override void OnResponse(NetState sender, RelayInfo info)");
            L("        {");
            L("            Mobile from = sender.Mobile;");
            L();
            L("            if (from == null)");
            L("            {");
            L("                return;");
            L("            }");
            L();
            foreach (GumpElement e in entries)
            {
                string var = "text" + Identifier(e.Name, "Entry" + e.EntryId);
                string idText = names.TryGetValue(e, out string n) ? n : e.EntryId.ToString(CultureInfo.InvariantCulture);
                L($"            TextRelay {var}Relay = info.GetTextEntry({idText});");
                L($"            string {var} = {var}Relay != null ? {var}Relay.Text.Trim() : string.Empty;");
            }
            foreach (GumpElement e in switches)
            {
                string var = "switch" + Identifier(e.Name, (e.Type == ElementType.Radio ? "Radio" : "Check") + e.SwitchId);
                string idText = names.TryGetValue(e, out string n) ? n : e.SwitchId.ToString(CultureInfo.InvariantCulture);
                L($"            bool {var} = info.IsSwitched({idText});");
            }
            if (entries.Count + switches.Count > 0)
            {
                L();
            }
            L("            switch (info.ButtonID)");
            L("            {");
            L("                case 0: // closed (right click or a button with ID 0)");
            L("                {");
            L("                    break;");
            L("                }");
            foreach (IGrouping<int, GumpElement> grp in replies.GroupBy(e => e.ButtonId).OrderBy(g => g.Key))
            {
                GumpElement e = grp.First();
                string label = names.TryGetValue(e, out string n) ? n : grp.Key.ToString(CultureInfo.InvariantCulture);
                string what = string.Join(", ", grp.Select(x => string.IsNullOrEmpty(x.Name) ? $"button at {x.X}, {x.Y}" : x.Name));
                L($"                case {label}: // {what}");
                L("                {");
                L($"                    from.SendMessage(\"{Regex.Replace(what, "[\"\\\\]", string.Empty)} pressed.\");");
                L($"                    // from.SendGump(new {cls}({(d.PassMobile ? "from" : string.Empty)})); // open again");
                L("                    break;");
                L("                }");
            }
            L("            }");
            L("        }");
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
                if (e.Type == ElementType.Label && (e.Text ?? string.Empty).Any(c => c > 127))
                {
                    p.Add($"Label \"{e.Text}\" has special characters – ServUO sends labels as ASCII (use AddHtml for umlauts).");
                }
            }
            return p;
        }
    }
}

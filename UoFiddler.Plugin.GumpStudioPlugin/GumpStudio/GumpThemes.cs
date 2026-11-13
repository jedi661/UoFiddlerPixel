using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>A look for gumps: frames, button set, check boxes, text colors. Applied with one click.</summary>
    public sealed class GumpTheme
    {
        [Category("0 Theme"), Description("Name of the theme.")]
        public string Name { get; set; } = "My theme";

        [Category("1 Frames"), Description("Outer frame (the biggest background).")]
        [Editor(typeof(GumpIdEditor), typeof(UITypeEditor)), TypeConverter(typeof(HexIntConverter))]
        public int Background { get; set; } = 9200;

        [Category("1 Frames"), Description("Inner frames (smaller backgrounds inside the gump).")]
        [Editor(typeof(GumpIdEditor), typeof(UITypeEditor)), TypeConverter(typeof(HexIntConverter))]
        public int InnerBackground { get; set; } = 3000;

        [Category("1 Frames"), Description("Frame behind text entries (backgrounds directly under a text entry).")]
        [Editor(typeof(GumpIdEditor), typeof(UITypeEditor)), TypeConverter(typeof(HexIntConverter))]
        public int EntryBackground { get; set; } = 3000;

        [Category("2 Buttons"), Description("OK / yes / apply button (up).")]
        [Editor(typeof(GumpIdEditor), typeof(UITypeEditor)), TypeConverter(typeof(HexIntConverter))]
        public int OkUp { get; set; } = 247;

        [Category("2 Buttons"), Description("OK button (pressed).")]
        [Editor(typeof(GumpIdEditor), typeof(UITypeEditor)), TypeConverter(typeof(HexIntConverter))]
        public int OkDown { get; set; } = 248;

        [Category("2 Buttons"), Description("Cancel / no button (up).")]
        [Editor(typeof(GumpIdEditor), typeof(UITypeEditor)), TypeConverter(typeof(HexIntConverter))]
        public int CancelUp { get; set; } = 241;

        [Category("2 Buttons"), Description("Cancel button (pressed).")]
        [Editor(typeof(GumpIdEditor), typeof(UITypeEditor)), TypeConverter(typeof(HexIntConverter))]
        public int CancelDown { get; set; } = 242;

        [Category("2 Buttons"), Description("Next / choose arrow (up).")]
        [Editor(typeof(GumpIdEditor), typeof(UITypeEditor)), TypeConverter(typeof(HexIntConverter))]
        public int NextUp { get; set; } = 4005;

        [Category("2 Buttons"), Description("Next arrow (pressed).")]
        [Editor(typeof(GumpIdEditor), typeof(UITypeEditor)), TypeConverter(typeof(HexIntConverter))]
        public int NextDown { get; set; } = 4007;

        [Category("2 Buttons"), Description("Back arrow (up).")]
        [Editor(typeof(GumpIdEditor), typeof(UITypeEditor)), TypeConverter(typeof(HexIntConverter))]
        public int BackUp { get; set; } = 4014;

        [Category("2 Buttons"), Description("Back arrow (pressed).")]
        [Editor(typeof(GumpIdEditor), typeof(UITypeEditor)), TypeConverter(typeof(HexIntConverter))]
        public int BackDown { get; set; } = 4016;

        [Category("3 Switches"), Description("Check box off.")]
        [Editor(typeof(GumpIdEditor), typeof(UITypeEditor)), TypeConverter(typeof(HexIntConverter))]
        public int CheckOff { get; set; } = 210;

        [Category("3 Switches"), Description("Check box on.")]
        [Editor(typeof(GumpIdEditor), typeof(UITypeEditor)), TypeConverter(typeof(HexIntConverter))]
        public int CheckOn { get; set; } = 211;

        [Category("3 Switches"), Description("Radio off.")]
        [Editor(typeof(GumpIdEditor), typeof(UITypeEditor)), TypeConverter(typeof(HexIntConverter))]
        public int RadioOff { get; set; } = 208;

        [Category("3 Switches"), Description("Radio on.")]
        [Editor(typeof(GumpIdEditor), typeof(UITypeEditor)), TypeConverter(typeof(HexIntConverter))]
        public int RadioOn { get; set; } = 209;

        [Category("4 Text"), Description("Hue of normal labels and text entries.")]
        [Editor(typeof(HueEditor), typeof(UITypeEditor))]
        public int TextHue { get; set; }

        [Category("4 Text"), Description("Hue of titles (labels named Title… or at the top of the gump).")]
        [Editor(typeof(HueEditor), typeof(UITypeEditor))]
        public int TitleHue { get; set; }

        [Category("4 Text"), Description("HTML text color, e.g. #FFFFFF (empty = keep). Replaces BASEFONT COLOR of HTML texts.")]
        public string HtmlColor { get; set; } = string.Empty;

        [Category("1 Frames"), Description("Put a see-through alpha region over the outer frame (dark glass look).")]
        public bool AlphaOverFrame { get; set; }

        [Browsable(false)]
        public bool BuiltIn { get; set; }

        public GumpTheme Clone() => (GumpTheme)MemberwiseClone();

        public override string ToString() => BuiltIn ? Name + " (built-in)" : Name;
    }

    /// <summary>Built-in themes, own themes (GumpThemes\*.json next to UoFiddler) and applying a theme to a gump.</summary>
    public static class GumpThemes
    {
        public static string Folder { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GumpThemes");

        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { WriteIndented = true };

        // button roles by their usual graphics
        private static readonly HashSet<int> OkIds = new HashSet<int> { 247, 249, 4023, 2130, 2128, 4011 };
        private static readonly HashSet<int> CancelIds = new HashSet<int> { 241, 242, 4017, 4020, 2119, 2143, 4026 };
        private static readonly HashSet<int> NextIds = new HashSet<int> { 4005, 5601, 9762, 2224, 4029, 2151, 2117 };
        private static readonly HashSet<int> BackIds = new HashSet<int> { 4014, 5603, 9770, 4008 };

        public static List<GumpTheme> BuiltIn() => new List<GumpTheme>
        {
            new GumpTheme { Name = "Classic gray", BuiltIn = true, Background = 9200, InnerBackground = 3000, EntryBackground = 3000, TextHue = 0, TitleHue = 0 },
            new GumpTheme
            {
                Name = "Dark stone", BuiltIn = true, Background = 5054, InnerBackground = 3000, EntryBackground = 3000, TextHue = 1152, TitleHue = 53, HtmlColor = "#FFFFFF",
                OkUp = 4023, OkDown = 4025, CancelUp = 4017, CancelDown = 4019, AlphaOverFrame = true
            },
            new GumpTheme
            {
                Name = "Marble", BuiltIn = true, Background = 9270, InnerBackground = 9350, EntryBackground = 9350, TextHue = 1152, TitleHue = 53, HtmlColor = "#FFFFFF",
                OkUp = 4005, OkDown = 4007, CancelUp = 4017, CancelDown = 4019
            },
            new GumpTheme
            {
                Name = "Parchment scroll", BuiltIn = true, Background = 9380, InnerBackground = 3500, EntryBackground = 3000, TextHue = 0, TitleHue = 0, HtmlColor = "#000000"
            },
            new GumpTheme
            {
                Name = "Golden", BuiltIn = true, Background = 2600, InnerBackground = 3000, EntryBackground = 3000, TextHue = 0, TitleHue = 0
            },
            new GumpTheme
            {
                Name = "Black glass", BuiltIn = true, Background = 9250, InnerBackground = 9300, EntryBackground = 9300, TextHue = 1152, TitleHue = 1153, HtmlColor = "#FFFFFF",
                OkUp = 4023, OkDown = 4025, CancelUp = 4017, CancelDown = 4019, CheckOff = 9026, CheckOn = 9027, AlphaOverFrame = true
            }
        };

        public static List<GumpTheme> LoadUser(List<string> errors)
        {
            var list = new List<GumpTheme>();
            if (!Directory.Exists(Folder))
            {
                return list;
            }
            foreach (string f in Directory.EnumerateFiles(Folder, "*.json"))
            {
                try
                {
                    GumpTheme t = JsonSerializer.Deserialize<GumpTheme>(File.ReadAllText(f), Json);
                    if (t != null)
                    {
                        t.BuiltIn = false;
                        list.Add(t);
                    }
                }
                catch (Exception ex)
                {
                    errors?.Add(Path.GetFileName(f) + ": " + ex.Message);
                }
            }
            return list.OrderBy(t => t.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        public static string FileOf(GumpTheme t) => Path.Combine(Folder, GumpTemplateLibrary.Safe(t.Name) + ".json");

        public static void Save(GumpTheme t)
        {
            Directory.CreateDirectory(Folder);
            GumpTheme copy = t.Clone();
            copy.BuiltIn = false;
            File.WriteAllText(FileOf(copy), JsonSerializer.Serialize(copy, Json), new UTF8Encoding(false));
        }

        public static void Delete(GumpTheme t)
        {
            if (!t.BuiltIn && File.Exists(FileOf(t)))
            {
                File.Delete(FileOf(t));
            }
        }

        private static GumpElement Outer(GumpDesign d) =>
            d.Elements.Where(e => e.Type == ElementType.Background).OrderByDescending(e => e.Width * e.Height).FirstOrDefault();

        private static bool IsEntryFrame(GumpDesign d, GumpElement bg) =>
            d.Elements.Any(e => e.Type == ElementType.TextEntry && e.X >= bg.X && e.Y >= bg.Y && e.X <= bg.X + 20 && e.Y <= bg.Y + 20 && e.Width <= bg.Width + 4);

        private static bool IsTitle(GumpDesign d, GumpElement e, GumpElement outer) =>
            (e.Name ?? string.Empty).IndexOf("title", StringComparison.OrdinalIgnoreCase) >= 0 || outer != null && e.Y - outer.Y < 40;

        /// <summary>A theme made from the gump: its outer frame, its buttons, switches and text colors.</summary>
        public static GumpTheme FromDesign(GumpDesign d, string name)
        {
            var t = new GumpTheme { Name = name };
            GumpElement outer = Outer(d);
            if (outer != null)
            {
                t.Background = outer.GumpId;
                GumpElement inner = d.Elements.FirstOrDefault(e => e.Type == ElementType.Background && e != outer && !IsEntryFrame(d, e));
                if (inner != null) t.InnerBackground = inner.GumpId;
                GumpElement entry = d.Elements.FirstOrDefault(e => e.Type == ElementType.Background && e != outer && IsEntryFrame(d, e));
                if (entry != null) t.EntryBackground = entry.GumpId;
            }
            foreach (GumpElement b in d.Elements.Where(e => e.Type == ElementType.Button))
            {
                if (OkIds.Contains(b.GumpId) || b.Name.IndexOf("ok", StringComparison.OrdinalIgnoreCase) >= 0) { t.OkUp = b.GumpId; t.OkDown = b.GumpId2; }
                else if (CancelIds.Contains(b.GumpId) || b.Name.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0) { t.CancelUp = b.GumpId; t.CancelDown = b.GumpId2; }
                else if (BackIds.Contains(b.GumpId) || b.ButtonType == ButtonKind.Page && b.TargetPage < b.Page) { t.BackUp = b.GumpId; t.BackDown = b.GumpId2; }
                else if (NextIds.Contains(b.GumpId) || b.ButtonType == ButtonKind.Page) { t.NextUp = b.GumpId; t.NextDown = b.GumpId2; }
            }
            GumpElement check = d.Elements.FirstOrDefault(e => e.Type == ElementType.Check);
            if (check != null) { t.CheckOff = check.GumpId; t.CheckOn = check.GumpId2; }
            GumpElement radio = d.Elements.FirstOrDefault(e => e.Type == ElementType.Radio);
            if (radio != null) { t.RadioOff = radio.GumpId; t.RadioOn = radio.GumpId2; }
            List<GumpElement> labels = d.Elements.Where(e => e.Type == ElementType.Label || e.Type == ElementType.LabelCropped).ToList();
            GumpElement title = labels.FirstOrDefault(e => IsTitle(d, e, outer));
            GumpElement text = labels.FirstOrDefault(e => !IsTitle(d, e, outer));
            if (title != null) t.TitleHue = title.Hue;
            if (text != null) t.TextHue = text.Hue;
            Match color = d.Elements.Where(e => e.Type == ElementType.Html).Select(e => Regex.Match(e.Text ?? string.Empty, @"BASEFONT\s+COLOR\s*=\s*""?(#[0-9A-Fa-f]{6})", RegexOptions.IgnoreCase)).FirstOrDefault(m => m.Success);
            if (color != null) t.HtmlColor = color.Groups[1].Value;
            return t;
        }

        /// <summary>Applies the theme to the gump. Returns how many elements changed.</summary>
        public static int Apply(GumpDesign d, GumpTheme t, bool frames, bool buttons, bool switches, bool texts)
        {
            int n = 0;
            void Set(Action change, GumpElement e)
            {
                string before = GumpDesign.ElementsToJson(new[] { e });
                change();
                if (before != GumpDesign.ElementsToJson(new[] { e }))
                {
                    n++;
                }
            }
            GumpElement outer = Outer(d);
            foreach (GumpElement e in d.Elements.ToList())
            {
                switch (e.Type)
                {
                    case ElementType.Background when frames:
                        Set(() => e.GumpId = e == outer ? t.Background : IsEntryFrame(d, e) ? t.EntryBackground : t.InnerBackground, e);
                        break;
                    case ElementType.Button when buttons:
                        Set(() =>
                        {
                            if (OkIds.Contains(e.GumpId)) { e.GumpId = t.OkUp; e.GumpId2 = t.OkDown; }
                            else if (CancelIds.Contains(e.GumpId)) { e.GumpId = t.CancelUp; e.GumpId2 = t.CancelDown; }
                            else if (BackIds.Contains(e.GumpId) || e.ButtonType == ButtonKind.Page && e.TargetPage < e.Page) { e.GumpId = t.BackUp; e.GumpId2 = t.BackDown; }
                            else if (NextIds.Contains(e.GumpId) || e.ButtonType == ButtonKind.Page) { e.GumpId = t.NextUp; e.GumpId2 = t.NextDown; }
                        }, e);
                        break;
                    case ElementType.Check when switches:
                        Set(() => { e.GumpId = t.CheckOff; e.GumpId2 = t.CheckOn; }, e);
                        break;
                    case ElementType.Radio when switches:
                        Set(() => { e.GumpId = t.RadioOff; e.GumpId2 = t.RadioOn; }, e);
                        break;
                    case ElementType.Label when texts:
                    case ElementType.LabelCropped when texts:
                        Set(() => e.Hue = IsTitle(d, e, outer) ? t.TitleHue : t.TextHue, e);
                        break;
                    case ElementType.TextEntry when texts:
                        Set(() => e.Hue = t.TextHue, e);
                        break;
                    case ElementType.Html when texts && !string.IsNullOrWhiteSpace(t.HtmlColor):
                        Set(() =>
                        {
                            string html = e.Text ?? string.Empty;
                            e.Text = Regex.IsMatch(html, @"BASEFONT\s+COLOR", RegexOptions.IgnoreCase)
                                ? Regex.Replace(html, @"(BASEFONT\s+COLOR\s*=\s*""?)#[0-9A-Fa-f]{6}", "${1}" + t.HtmlColor.Trim(), RegexOptions.IgnoreCase)
                                : $"<BASEFONT COLOR={t.HtmlColor.Trim()}>{html}</BASEFONT>";
                        }, e);
                        break;
                }
            }
            if (frames && outer != null)
            {
                bool has = d.Elements.Any(e => e.Type == ElementType.AlphaRegion && e.X <= outer.X + 12 && e.Y <= outer.Y + 12 && e.Width >= outer.Width - 24 && e.Name == "ThemeGlass");
                if (t.AlphaOverFrame && !has)
                {
                    d.Elements.Insert(d.Elements.IndexOf(outer) + 1, new GumpElement
                    {
                        Type = ElementType.AlphaRegion, X = outer.X + 10, Y = outer.Y + 10, Width = Math.Max(4, outer.Width - 20), Height = Math.Max(4, outer.Height - 20),
                        Page = outer.Page, Name = "ThemeGlass"
                    });
                    n++;
                }
                else if (!t.AlphaOverFrame && has)
                {
                    n += d.Elements.RemoveAll(e => e.Type == ElementType.AlphaRegion && e.Name == "ThemeGlass");
                }
            }
            return n;
        }
    }
}

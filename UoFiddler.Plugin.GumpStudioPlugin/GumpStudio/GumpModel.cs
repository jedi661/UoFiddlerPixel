using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>The kinds of gump entries (ServUO Gump.Add… methods).</summary>
    public enum ElementType
    {
        Background,     // AddBackground – 9 part frame
        Image,          // AddImage
        ImageTiled,     // AddImageTiled
        AlphaRegion,    // AddAlphaRegion
        Label,          // AddLabel
        LabelCropped,   // AddLabelCropped
        Html,           // AddHtml
        HtmlLocalized,  // AddHtmlLocalized
        Button,         // AddButton
        Check,          // AddCheck
        Radio,          // AddRadio
        TextEntry,      // AddTextEntry
        Item,           // AddItem
        SpriteImage,    // AddSpriteImage
        ItemButton      // AddImageTiledButton (button with an item picture)
    }

    public enum ButtonKind
    {
        Reply = 1,
        Page = 0
    }

    /// <summary>What a reply button does in OnResponse (the script is written for it).</summary>
    public enum ButtonAction
    {
        None,           // only a message, you write the code
        Close,          // nothing – the gump is closed
        Reopen,         // open the same gump again
        OpenGump,       // open another gump (Action target = class name)
        Teleport,       // move the player (Action location = "x y z map")
        GiveItem,       // put an item in the backpack (Action target = item type, e.g. Gold or "Gold(100)")
        RunCommand,     // run a command as the player (Action target = e.g. "Where")
        Message,        // send a message (Action target = text)
        CustomCode      // Action target = your own C# lines
    }

    /// <summary>Marks a property as used only by some element types (the property grid hides it for the others).</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AppliesToAttribute : Attribute
    {
        public AppliesToAttribute(params ElementType[] types) => Types = types;

        public ElementType[] Types { get; }
    }

    /// <summary>One entry of the gump. All properties of all types live here; the grid shows only the ones that apply.</summary>
    public sealed class GumpElement : ICustomTypeDescriptor
    {
        private static readonly ElementType[] Sized =
        {
            ElementType.Background, ElementType.ImageTiled, ElementType.AlphaRegion, ElementType.LabelCropped, ElementType.Html, ElementType.HtmlLocalized,
            ElementType.TextEntry, ElementType.SpriteImage, ElementType.ItemButton
        };

        [Category("1 Element"), Description("What this entry is. / Art des Eintrags.")]
        [ReadOnly(true)]
        public ElementType Type { get; set; }

        [Category("1 Element"), Description("Name used in the script (constants, comments). / Name im Skript (Konstanten, Kommentare).")]
        public string Name { get; set; } = string.Empty;

        [Category("1 Element"), Description("Page of the gump: 0 = shown on every page. / Seite: 0 = auf jeder Seite sichtbar.")]
        public int Page { get; set; }

        [Category("2 Position"), Description("X from the left edge of the gump.")]
        public int X { get; set; }

        [Category("2 Position"), Description("Y from the top edge of the gump.")]
        public int Y { get; set; }

        [Category("2 Position"), Description("Width in pixels.")]
        [AppliesTo(ElementType.Background, ElementType.ImageTiled, ElementType.AlphaRegion, ElementType.LabelCropped, ElementType.Html, ElementType.HtmlLocalized,
            ElementType.TextEntry, ElementType.SpriteImage, ElementType.ItemButton)]
        public int Width { get; set; } = 100;

        [Category("2 Position"), Description("Height in pixels.")]
        [AppliesTo(ElementType.Background, ElementType.ImageTiled, ElementType.AlphaRegion, ElementType.LabelCropped, ElementType.Html, ElementType.HtmlLocalized,
            ElementType.TextEntry, ElementType.SpriteImage, ElementType.ItemButton)]
        public int Height { get; set; } = 20;

        [Category("3 Graphic"), Description("Gump ID (picture, first of the 9 background parts, button up / checkbox off). Click … to choose.")]
        [Editor(typeof(GumpIdEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(HexIntConverter))]
        [AppliesTo(ElementType.Background, ElementType.Image, ElementType.ImageTiled, ElementType.Button, ElementType.Check, ElementType.Radio, ElementType.SpriteImage, ElementType.ItemButton)]
        public int GumpId { get; set; }

        [Category("3 Graphic"), Description("Gump ID when pressed (button) or active (checkbox / radio). Click … to choose.")]
        [Editor(typeof(GumpIdEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(HexIntConverter))]
        [AppliesTo(ElementType.Button, ElementType.Check, ElementType.Radio, ElementType.ItemButton)]
        public int GumpId2 { get; set; }

        [Category("3 Graphic"), Description("Item ID (art). Click … to choose.")]
        [Editor(typeof(ItemIdEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(HexIntConverter))]
        [AppliesTo(ElementType.Item, ElementType.ItemButton)]
        public int ItemId { get; set; }

        [Category("3 Graphic"), Description("Hue (0 = none). Labels: the text color, e.g. 1152 = white. Click … to choose.")]
        [Editor(typeof(HueEditor), typeof(UITypeEditor))]
        [AppliesTo(ElementType.Image, ElementType.Label, ElementType.LabelCropped, ElementType.TextEntry, ElementType.Item, ElementType.ItemButton, ElementType.HtmlLocalized)]
        public int Hue { get; set; }

        [Category("3 Graphic"), Description("Sprite: x of the part of the gump picture that is shown.")]
        [AppliesTo(ElementType.SpriteImage)]
        public int SourceX { get; set; }

        [Category("3 Graphic"), Description("Sprite: y of the part of the gump picture that is shown.")]
        [AppliesTo(ElementType.SpriteImage)]
        public int SourceY { get; set; }

        [Category("4 Text"), Description("Text. HTML may use <BASEFONT COLOR=#FFFFFF>, <CENTER>, <BR>, <B>, <I>, <U>, <A HREF=…>.")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", typeof(UITypeEditor))]
        [AppliesTo(ElementType.Label, ElementType.LabelCropped, ElementType.Html, ElementType.TextEntry)]
        public string Text { get; set; } = string.Empty;

        [Category("4 Text"), Description("Cliloc number of the text (AddHtmlLocalized) or of the tooltip.")]
        [AppliesTo(ElementType.HtmlLocalized, ElementType.ItemButton)]
        public int Cliloc { get; set; }

        [Category("4 Text"), Description("HTML with the light background (gump 3000).")]
        [AppliesTo(ElementType.Html, ElementType.HtmlLocalized)]
        public bool HtmlBackground { get; set; }

        [Category("4 Text"), Description("HTML with a scrollbar.")]
        [AppliesTo(ElementType.Html, ElementType.HtmlLocalized)]
        public bool Scrollbar { get; set; }

        [Category("4 Text"), Description("Maximum number of characters (0 = no limit). / Höchstzahl Zeichen (0 = ohne Grenze).")]
        [AppliesTo(ElementType.TextEntry)]
        public int MaxLength { get; set; }

        [Category("5 Action"), Description("Reply = the server gets the button ID (OnResponse). Page = the client switches to the page in \"Target page\".")]
        [AppliesTo(ElementType.Button, ElementType.ItemButton)]
        public ButtonKind ButtonType { get; set; } = ButtonKind.Reply;

        [Category("5 Action"), Description("Button ID sent to OnResponse (Reply). 0 = close without action.")]
        [AppliesTo(ElementType.Button, ElementType.ItemButton)]
        public int ButtonId { get; set; } = 1;

        [Category("5 Action"), Description("Page buttons: the page to show. Reply buttons: the extra param (usually 0).")]
        [AppliesTo(ElementType.Button, ElementType.ItemButton)]
        public int TargetPage { get; set; }

        [Category("5 Action"), Description("Switch ID (checkbox / radio) – read with info.IsSwitched(id).")]
        [AppliesTo(ElementType.Check, ElementType.Radio)]
        public int SwitchId { get; set; } = 1;

        [Category("5 Action"), Description("Ticked when the gump opens.")]
        [AppliesTo(ElementType.Check, ElementType.Radio)]
        public bool Checked { get; set; }

        [Category("5 Action"), Description("Radio group: only one radio of a group can be on (AddGroup).")]
        [AppliesTo(ElementType.Radio)]
        public int Group { get; set; }

        [Category("5 Action"), Description("Text entry ID – read with info.GetTextEntry(id).")]
        [AppliesTo(ElementType.TextEntry)]
        public int EntryId { get; set; } = 1;

        [Category("5 Action"), Description("What the button does in OnResponse – the script is written for it. / Was der Button in OnResponse tut.")]
        [AppliesTo(ElementType.Button, ElementType.ItemButton)]
        public ButtonAction Action { get; set; }

        [Category("5 Action"), Description("Open gump: class name · Give item: item type, e.g. Gold or Gold(100) · Command: e.g. Where · Message: the text · Custom code: C# lines (\\n = new line).")]
        [AppliesTo(ElementType.Button, ElementType.ItemButton)]
        public string ActionTarget { get; set; } = string.Empty;

        [Category("5 Action"), Description("Teleport: \"x y z map\", e.g. \"1434 1699 2 Felucca\".")]
        [AppliesTo(ElementType.Button, ElementType.ItemButton)]
        public string ActionLocation { get; set; } = string.Empty;

        [Category("5 Action"), Description("Open this gump again after the action.")]
        [AppliesTo(ElementType.Button, ElementType.ItemButton)]
        public bool ActionReopen { get; set; }

        [Category("4 Text"), Description("Text in the second language (German, for players with client language DEU). Empty = same as Text. / Text in der zweiten Sprache.")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", typeof(UITypeEditor))]
        [AppliesTo(ElementType.Label, ElementType.LabelCropped, ElementType.Html, ElementType.TextEntry)]
        public string TextDe { get; set; } = string.Empty;

        [Category("6 Extra"), Description("Designer group: elements of a group are selected and moved together (Ctrl+G / Ctrl+Shift+G).")]
        public string DesignGroup { get; set; } = string.Empty;

        [Category("6 Extra"), Description("List this element belongs to (List / table generator). Rows are made from row 1.")]
        [ReadOnly(true)]
        public string ListId { get; set; } = string.Empty;

        [Browsable(false)]
        public int ListRow { get; set; }

        [Browsable(false)]
        public int ListSlot { get; set; }

        [Browsable(false)]
        public int ListColumn { get; set; } = -1;

        /// <summary>Where the element came from in the imported script (for writing back): "offset#occurrence".</summary>
        [Browsable(false)]
        public string Source { get; set; } = string.Empty;

        [Category("6 Extra"), Description("Tooltip as cliloc number (0 = none) – AddTooltip after this entry.")]
        public int TooltipCliloc { get; set; }

        [Category("6 Extra"), Description("Tooltip as text (newer clients) – AddTooltip(\"…\").")]
        public string TooltipText { get; set; } = string.Empty;

        [Category("6 Extra"), Description("Locked: cannot be moved with the mouse. / Gesperrt: nicht mit der Maus verschiebbar.")]
        public bool Locked { get; set; }

        [Category("6 Extra"), Description("Hidden in the designer only (still written to the script).")]
        public bool Hidden { get; set; }

        [Browsable(false)]
        [JsonIgnore]
        public bool IsSized => Sized.Contains(Type);

        [Browsable(false)]
        [JsonIgnore]
        public Rectangle Bounds { get; set; }

        public GumpElement Clone() => (GumpElement)MemberwiseClone();

        public override string ToString() => (string.IsNullOrEmpty(Name) ? Type.ToString() : $"{Name} ({Type})") + $"  {X}, {Y}";

        // ------------------------------------------------------------------ ICustomTypeDescriptor: show only the properties of this type

        AttributeCollection ICustomTypeDescriptor.GetAttributes() => TypeDescriptor.GetAttributes(this, true);

        string ICustomTypeDescriptor.GetClassName() => TypeDescriptor.GetClassName(this, true);

        string ICustomTypeDescriptor.GetComponentName() => TypeDescriptor.GetComponentName(this, true);

        TypeConverter ICustomTypeDescriptor.GetConverter() => TypeDescriptor.GetConverter(this, true);

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(this, true);

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(this, true);

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(this, editorBaseType, true);

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() => TypeDescriptor.GetEvents(this, true);

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) => TypeDescriptor.GetEvents(this, attributes, true);

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() => ((ICustomTypeDescriptor)this).GetProperties(null);

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection all = TypeDescriptor.GetProperties(this, attributes, true);
            var list = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor p in all)
            {
                var applies = (AppliesToAttribute)p.Attributes[typeof(AppliesToAttribute)];
                if (applies == null || applies.Types.Contains(Type))
                {
                    list.Add(p);
                }
            }
            return new PropertyDescriptorCollection(list.ToArray());
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) => this;
    }

    /// <summary>Shows and reads integers as hex (0x0BB8) in the property grid; decimal input works too.</summary>
    public sealed class HexIntConverter : Int32Converter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is int i)
            {
                return $"0x{i:X4} ({i})";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string s)
            {
                s = s.Trim();
                int paren = s.IndexOf('(');
                if (paren > 0)
                {
                    s = s.Substring(0, paren).Trim();
                }
                if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    return Convert.ToInt32(s.Substring(2), 16);
                }
                return int.Parse(s, culture);
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    /// <summary>The whole gump: settings and entries (z-order = list order).</summary>
    public sealed class GumpDesign
    {
        [Category("1 Script"), Description("Class name of the gump. / Klassenname des Gumps.")]
        public string ClassName { get; set; } = "MyGump";

        [Category("1 Script"), Description("Namespace of the script.")]
        public string Namespace { get; set; } = "Server.Gumps";

        [Category("1 Script"), Description("Command that opens the gump in game, e.g. MyGump → [MyGump (empty = no command).")]
        public string Command { get; set; } = string.Empty;

        [Category("1 Script"), Description("Access level of the command.")]
        public string CommandAccess { get; set; } = "GameMaster";

        [Category("1 Script"), Description("Constructor gets the Mobile that sees the gump (Mobile from).")]
        public bool PassMobile { get; set; } = true;

        [Category("1 Script"), Description("Close the gump of the same type before opening it again (CloseGump).")]
        public bool CloseExisting { get; set; } = true;

        [Category("2 Gump"), Description("Screen position of the gump.")]
        public int X { get; set; } = 50;

        [Category("2 Gump"), Description("Screen position of the gump.")]
        public int Y { get; set; } = 50;

        [Category("2 Gump"), Description("Right click closes the gump.")]
        public bool Closable { get; set; } = true;

        [Category("2 Gump"), Description("The gump can be moved.")]
        public bool Dragable { get; set; } = true;

        [Category("2 Gump"), Description("The gump closes when the player moves away / logs out.")]
        public bool Disposable { get; set; } = true;

        [Category("2 Gump"), Description("Resizable flag of the gump.")]
        public bool Resizable { get; set; }

        [Category("3 Designer"), Description("Size of the work area. / Größe der Arbeitsfläche.")]
        public int CanvasWidth { get; set; } = 640;

        [Category("3 Designer"), Description("Size of the work area.")]
        public int CanvasHeight { get; set; } = 480;

        [Category("1 Script"), Description("Second language for players whose client language is this code (DEU = German). Texts come from \"TextDe\" of the elements.")]
        public string SecondLanguage { get; set; } = "DEU";

        [Category("1 Script"), Description("Write the actions of the test mode (F5) as comment into the script.")]
        public bool IncludeTestLog { get; set; }

        [Category("3 Designer"), Description("Script the gump was imported from (Write back changes the numbers there).")]
        [ReadOnly(true)]
        public string SourceFile { get; set; } = string.Empty;

        /// <summary>The import read values of other classes from the scripts around the file (needed to compare when writing back).</summary>
        [Browsable(false)]
        public bool SourceResolve { get; set; }

        /// <summary>Check sum of the script when it was imported – writing back needs the same file.</summary>
        [Browsable(false)]
        public string SourceHash { get; set; } = string.Empty;

        [Browsable(false)]
        public List<GumpListSpec> Lists { get; set; } = new List<GumpListSpec>();

        [Browsable(false)]
        public List<string> TestLog { get; set; } = new List<string>();

        [Category("3 Designer"), Description("Note for the template library (what the gump is for). / Notiz für die Vorlagen-Bibliothek.")]
        public string Description { get; set; } = string.Empty;

        [Browsable(false)]
        public List<GumpElement> Elements { get; set; } = new List<GumpElement>();

        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public string ToJson() => JsonSerializer.Serialize(this, Json);

        public static GumpDesign FromJson(string json) => JsonSerializer.Deserialize<GumpDesign>(json, Json) ?? new GumpDesign();

        public GumpDesign Clone() => FromJson(ToJson());

        public int Pages => Elements.Count == 0 ? 1 : Math.Max(1, Elements.Max(e => e.Page));

        public static string ElementsToJson(IEnumerable<GumpElement> elements) => JsonSerializer.Serialize(elements.ToList(), Json);

        public static List<GumpElement> ElementsFromJson(string json) => JsonSerializer.Deserialize<List<GumpElement>>(json, Json) ?? new List<GumpElement>();

        /// <summary>A new element with useful defaults (UO standard graphics).</summary>
        public static GumpElement Create(ElementType type, int x, int y)
        {
            var e = new GumpElement { Type = type, X = x, Y = y };
            switch (type)
            {
                case ElementType.Background: e.GumpId = 9200; e.Width = 300; e.Height = 200; break;
                case ElementType.Image: e.GumpId = 0x15A9; break;
                case ElementType.ImageTiled: e.GumpId = 2624; e.Width = 200; e.Height = 100; break;
                case ElementType.AlphaRegion: e.Width = 200; e.Height = 100; break;
                case ElementType.Label: e.Text = "Label"; e.Hue = 0; break;
                case ElementType.LabelCropped: e.Text = "Cropped label"; e.Width = 120; e.Height = 20; break;
                case ElementType.Html: e.Text = "<BASEFONT COLOR=#FFFFFF>HTML text"; e.Width = 200; e.Height = 60; break;
                case ElementType.HtmlLocalized: e.Cliloc = 1011036; e.Width = 200; e.Height = 20; e.Hue = 0x7FFF; break;
                case ElementType.Button: e.GumpId = 247; e.GumpId2 = 248; e.ButtonId = 1; break;
                case ElementType.Check: e.GumpId = 210; e.GumpId2 = 211; break;
                case ElementType.Radio: e.GumpId = 208; e.GumpId2 = 209; break;
                case ElementType.TextEntry: e.Text = string.Empty; e.Width = 150; e.Height = 20; e.Hue = 0; break;
                case ElementType.Item: e.ItemId = 0x0EED; break;
                case ElementType.SpriteImage: e.GumpId = 9200; e.Width = 30; e.Height = 30; break;
                case ElementType.ItemButton: e.GumpId = 0x918; e.GumpId2 = 0x919; e.ItemId = 0x0EED; e.Width = 60; e.Height = 60; break;
            }
            return e;
        }
    }
}

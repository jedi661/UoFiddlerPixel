using System;
using System.Collections.Generic;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>Ready-made gumps to start from (standard UO graphics).</summary>
    public static class GumpTemplates
    {
        public static readonly (string Name, Func<GumpDesign> Create)[] All =
        {
            ("Empty window", Window),
            ("Yes / No question", Confirm),
            ("Text input form", InputForm),
            ("Teleporter menu (pages)", Teleporter),
            ("Choice list (check boxes + radios)", ChoiceList),
            ("Info text with scrollbar", InfoText),
            ("Empty work area", () => new GumpDesign { ClassName = "MyGump" })
        };

        private static GumpElement E(ElementType t, int x, int y, Action<GumpElement> set = null)
        {
            GumpElement e = GumpDesign.Create(t, x, y);
            set?.Invoke(e);
            return e;
        }

        private static GumpDesign Window() => new GumpDesign
        {
            ClassName = "MyWindowGump",
            Command = "MyWindow",
            Elements = new List<GumpElement>
            {
                E(ElementType.Background, 0, 0, e => { e.Width = 320; e.Height = 220; e.GumpId = 9200; e.Name = "Frame"; }),
                E(ElementType.Label, 20, 15, e => { e.Text = "My window"; e.Hue = 0; e.Name = "Title"; }),
                E(ElementType.Html, 20, 45, e => { e.Width = 280; e.Height = 120; e.Text = "Text of the window."; }),
                E(ElementType.Button, 230, 180, e => { e.GumpId = 247; e.GumpId2 = 248; e.ButtonId = 1; e.Name = "Okay"; })
            }
        };

        private static GumpDesign Confirm() => new GumpDesign
        {
            ClassName = "ConfirmGump",
            Command = "Confirm",
            Elements = new List<GumpElement>
            {
                E(ElementType.Background, 0, 0, e => { e.Width = 300; e.Height = 150; e.GumpId = 9270; e.Name = "Frame"; }),
                E(ElementType.AlphaRegion, 10, 10, e => { e.Width = 280; e.Height = 130; }),
                E(ElementType.Html, 20, 20, e => { e.Width = 260; e.Height = 70; e.Text = "<CENTER><BASEFONT COLOR=#FFFFFF>Are you sure?</BASEFONT></CENTER>"; e.Name = "Question"; }),
                E(ElementType.Button, 50, 105, e => { e.GumpId = 247; e.GumpId2 = 248; e.ButtonId = 1; e.Name = "Yes"; }),
                E(ElementType.Button, 180, 105, e => { e.GumpId = 241; e.GumpId2 = 242; e.ButtonId = 0; e.Name = "No"; })
            }
        };

        private static GumpDesign InputForm() => new GumpDesign
        {
            ClassName = "InputGump",
            Command = "Input",
            Elements = new List<GumpElement>
            {
                E(ElementType.Background, 0, 0, e => { e.Width = 360; e.Height = 190; e.GumpId = 9200; }),
                E(ElementType.Label, 20, 15, e => { e.Text = "Please enter:"; e.Name = "Title"; }),
                E(ElementType.Label, 20, 50, e => e.Text = "Name:"),
                E(ElementType.Background, 90, 45, e => { e.Width = 240; e.Height = 28; e.GumpId = 3000; }),
                E(ElementType.TextEntry, 97, 50, e => { e.Width = 225; e.Height = 20; e.EntryId = 1; e.Name = "Name"; e.MaxLength = 20; }),
                E(ElementType.Label, 20, 90, e => e.Text = "Amount:"),
                E(ElementType.Background, 90, 85, e => { e.Width = 100; e.Height = 28; e.GumpId = 3000; }),
                E(ElementType.TextEntry, 97, 90, e => { e.Width = 85; e.Height = 20; e.EntryId = 2; e.Name = "Amount"; e.Text = "1"; e.MaxLength = 5; }),
                E(ElementType.Button, 160, 145, e => { e.GumpId = 247; e.GumpId2 = 248; e.ButtonId = 1; e.Name = "Okay"; }),
                E(ElementType.Button, 255, 145, e => { e.GumpId = 241; e.GumpId2 = 242; e.ButtonId = 0; e.Name = "Cancel"; })
            }
        };

        private static GumpDesign Teleporter()
        {
            var d = new GumpDesign { ClassName = "TeleporterGump", Command = "Travel", CanvasWidth = 640 };
            d.Elements.Add(E(ElementType.Background, 0, 0, e => { e.Width = 330; e.Height = 290; e.GumpId = 9270; }));
            d.Elements.Add(E(ElementType.Html, 20, 18, e => { e.Width = 290; e.Height = 20; e.Text = "<CENTER><BASEFONT COLOR=#FFCC66>Travel</BASEFONT></CENTER>"; }));
            string[][] pages =
            {
                new[] { "Britain", "Trinsic", "Minoc", "Yew", "Moonglow" },
                new[] { "Vesper", "Skara Brae", "Jhelom", "Magincia", "Ocllo" }
            };
            int id = 1;
            for (int p = 0; p < pages.Length; p++)
            {
                int page = p + 1;
                for (int i = 0; i < pages[p].Length; i++)
                {
                    string city = pages[p][i];
                    d.Elements.Add(E(ElementType.Button, 30, 55 + i * 35, e => { e.GumpId = 4005; e.GumpId2 = 4007; e.ButtonId = id; e.Name = city.Replace(" ", string.Empty); e.Page = page; }));
                    d.Elements.Add(E(ElementType.Label, 70, 57 + i * 35, e => { e.Text = city; e.Hue = 1152; e.Page = page; }));
                    id++;
                }
                if (page < pages.Length)
                {
                    d.Elements.Add(E(ElementType.Button, 270, 245, e => { e.GumpId = 4005; e.GumpId2 = 4007; e.ButtonType = ButtonKind.Page; e.TargetPage = page + 1; e.Page = page; e.Name = "Next"; }));
                }
                if (page > 1)
                {
                    d.Elements.Add(E(ElementType.Button, 30, 245, e => { e.GumpId = 4014; e.GumpId2 = 4016; e.ButtonType = ButtonKind.Page; e.TargetPage = page - 1; e.Page = page; e.Name = "Back"; }));
                }
            }
            return d;
        }

        private static GumpDesign ChoiceList()
        {
            var d = new GumpDesign { ClassName = "ChoiceGump", Command = "Choose" };
            d.Elements.Add(E(ElementType.Background, 0, 0, e => { e.Width = 340; e.Height = 300; e.GumpId = 9200; }));
            d.Elements.Add(E(ElementType.Label, 20, 15, e => e.Text = "Options:"));
            string[] checks = { "Show hidden items", "Auto loot", "Play sounds" };
            for (int i = 0; i < checks.Length; i++)
            {
                int n = i;
                d.Elements.Add(E(ElementType.Check, 25, 45 + i * 30, e => { e.SwitchId = n + 1; e.Name = checks[n].Replace(" ", string.Empty); e.Checked = n == 0; }));
                d.Elements.Add(E(ElementType.Label, 55, 45 + i * 30, e => e.Text = checks[n]));
            }
            d.Elements.Add(E(ElementType.Label, 20, 145, e => e.Text = "Difficulty:"));
            string[] radios = { "Easy", "Normal", "Hard" };
            for (int i = 0; i < radios.Length; i++)
            {
                int n = i;
                d.Elements.Add(E(ElementType.Radio, 25, 175 + i * 28, e => { e.SwitchId = 10 + n; e.Group = 1; e.Name = radios[n]; e.Checked = n == 1; }));
                d.Elements.Add(E(ElementType.Label, 55, 175 + i * 28, e => e.Text = radios[n]));
            }
            d.Elements.Add(E(ElementType.Button, 240, 260, e => { e.GumpId = 247; e.GumpId2 = 248; e.ButtonId = 1; e.Name = "Apply"; }));
            return d;
        }

        private static GumpDesign InfoText() => new GumpDesign
        {
            ClassName = "InfoGump",
            Command = "Info",
            Elements = new List<GumpElement>
            {
                E(ElementType.Background, 0, 0, e => { e.Width = 420; e.Height = 360; e.GumpId = 9380; }),
                E(ElementType.Html, 40, 45, e => { e.Width = 340; e.Height = 20; e.Text = "<CENTER><B>Rules of the shard</B></CENTER>"; }),
                E(ElementType.Html, 40, 75, e =>
                {
                    e.Width = 340; e.Height = 220; e.HtmlBackground = true; e.Scrollbar = true;
                    e.Text = "1. Be nice.<BR>2. No cheating.<BR>3. Have fun!<BR><BR>Long text scrolls with the scrollbar.";
                }),
                E(ElementType.Button, 330, 310, e => { e.GumpId = 247; e.GumpId2 = 248; e.ButtonId = 0; e.Name = "Close"; })
            }
        };
    }
}

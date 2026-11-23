/***************************************************************************
 *
 *  * Dragon Map Studio – CGI view: side panel layout
 *  *
 *  *  - overview map in a folding section: zoom (wheel, + / −), size, undock into its own window
 *  *  - "Gehe zu" folding
 *  *  - coloured category buttons with pictures instead of two rows of tabs:
 *  *      Bauen (Items, Multis, Vorlagen) · Gelände (Land, Küste / Wasser, Berg)
 *  *      Höhlen (Höhle / Weg, Höhlen-Stempel) · Prüfen (Auswahl, Probleme)
 *  *  - the long tabs (Höhle / Weg, Höhlen-Stempel, Berg) as folding sections,
 *  *    help texts folded away, open / closed is remembered
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        private static readonly Color ColBuild = Color.FromArgb(37, 99, 235);
        private static readonly Color ColTerrain = Color.FromArgb(22, 128, 61);
        private static readonly Color ColCave = Color.FromArgb(180, 83, 9);
        private static readonly Color ColCheck = Color.FromArgb(185, 28, 28);
        private static readonly Color ColView = Color.FromArgb(71, 85, 105);

        private sealed class NavEntry
        {
            public TabPage Tab;
            public Color Color;
            public Button Button;
            public Func<Image> Icon;
        }

        private readonly List<NavEntry> _cgiNav = new List<NavEntry>();
        private readonly Dictionary<CollapsibleSection, string> _sectionKeys = new Dictionary<CollapsibleSection, string>();
        private readonly List<(CollapsibleSection section, Func<Image> icon)> _sectionIcons = new List<(CollapsibleSection, Func<Image>)>();
        private static readonly int[] OverviewHeights = { 170, 250, 380 };

        private ToolStripDropDownButton _cgiPatternButton;
        private ColorStamp _cgiPatternStamp;
        private bool _cgiPatternFreehand = true;
        private StampLayers _cgiPatternLayers = StampLayers.Both;
        private bool _cgiPatternTransitions = true;
        private bool _cgiPatternSteps;

        /// <summary>Menu of the CGI pattern brush: stamps (with built land tiles or statics) + options.</summary>
        private void FillCgiPatternMenu()
        {
            if (_cgiPatternButton == null)
            {
                return;
            }
            ToolStripItemCollection items = _cgiPatternButton.DropDownItems;
            items.Clear();
            var usable = _stamps.Where(s => s.CanLearn || (s.Statics?.Count ?? 0) > 0).ToList();
            if (usable.Count == 0)
            {
                items.Add(new ToolStripMenuItem(L.T("Noch keine Stempel mit Land-Tiles oder Statics – in der CGI-Ansicht einen Bereich markieren und '▣ Bereich → Farb-Stempel'")) { Enabled = false });
            }
            foreach (IGrouping<string, ColorStamp> cat in usable.GroupBy(s => string.IsNullOrWhiteSpace(s.Category) ? L.T("ohne Kategorie") : s.Category).OrderBy(g => g.Key))
            {
                var sub = new ToolStripMenuItem(cat.Key);
                foreach (ColorStamp st in cat.OrderBy(s => s.Name))
                {
                    ColorStamp captured = st;
                    var it = new ToolStripMenuItem($"{st.Name}  ({st.Width}×{st.Height}{(st.CanLearn ? string.Empty : ", " + L.T("nur Statics"))})")
                    {
                        Checked = ReferenceEquals(_cgiPatternStamp, st) || (_cgiPatternStamp != null && _cgiPatternStamp.File == st.File)
                    };
                    it.Click += (s, e) =>
                    {
                        _cgiPatternStamp = captured;
                        _cgiPatternFreehand = captured.DrawMode == StampDrawMode.Freehand;
                        _cgiPatternLayers = captured.Layers;
                        ActivateCgiPattern();
                    };
                    sub.DropDownItems.Add(it);
                }
                items.Add(sub);
            }
            items.Add(new ToolStripSeparator());
            ToolStripMenuItem Opt(string text, bool on, Action click)
            {
                var o = new ToolStripMenuItem(L.T(text)) { Checked = on };
                o.Click += (s, e) => { click(); ActivateCgiPattern(); };
                return o;
            }
            items.Add(Opt("Freihand (durchgehend ziehen)", _cgiPatternFreehand, () => _cgiPatternFreehand = true));
            items.Add(Opt("Einmal (Klick setzt das ganze Muster)", !_cgiPatternFreehand, () => _cgiPatternFreehand = false));
            items.Add(new ToolStripSeparator());
            items.Add(Opt("Land + Statics", _cgiPatternLayers == StampLayers.Both, () => _cgiPatternLayers = StampLayers.Both));
            items.Add(Opt("Nur Land", _cgiPatternLayers == StampLayers.LandOnly, () => _cgiPatternLayers = StampLayers.LandOnly));
            items.Add(Opt("Nur Statics", _cgiPatternLayers == StampLayers.StaticsOnly, () => _cgiPatternLayers = StampLayers.StaticsOnly));
            items.Add(new ToolStripSeparator());
            var height = new ToolStripMenuItem(L.T("Höhe") + ": " + HeightModeText(_settings.PatternHeight) +
                                               (_settings.PatternHeightOffset != 0 ? $" {_settings.PatternHeightOffset:+0;-0}" : string.Empty));
            height.DropDownItems.Add(Opt("Gelände der Karte behalten (Straße folgt dem Gelände)", _settings.PatternHeight == PatternHeightMode.Terrain,
                () => _settings.PatternHeight = PatternHeightMode.Terrain));
            height.DropDownItems.Add(Opt("Original: Höhen aus dem Stempel (relativ)", _settings.PatternHeight == PatternHeightMode.Original,
                () => _settings.PatternHeight = PatternHeightMode.Original));
            height.DropDownItems.Add(Opt($"Feste Höhe… (jetzt {_settings.PatternHeightValue})", _settings.PatternHeight == PatternHeightMode.Fixed, () =>
            {
                if (AskInt(L.T("Feste Höhe (Z) für den Muster-Pinsel:"), _settings.PatternHeightValue, out int v))
                {
                    _settings.PatternHeightValue = v;
                    _settings.PatternHeight = PatternHeightMode.Fixed;
                }
            }));
            height.DropDownItems.Add(new ToolStripSeparator());
            height.DropDownItems.Add(Opt($"Höhe ± … (jetzt {_settings.PatternHeightOffset})", _settings.PatternHeightOffset != 0, () =>
            {
                if (AskInt(L.T("Zur Höhe addieren (z. B. -3 für ein Flussbett, +1 für eine Straße):"), _settings.PatternHeightOffset, out int v))
                {
                    _settings.PatternHeightOffset = v;
                }
            }));
            items.Add(height);
            items.Add(Opt("Automatisch nachbessern (Höhen-Spitzen am Strich glätten)", _settings.PatternAutoFix, () => _settings.PatternAutoFix = !_settings.PatternAutoFix));
            items.Add(Opt("Übergänge zum Rand danach berechnen", _cgiPatternTransitions, () => _cgiPatternTransitions = !_cgiPatternTransitions));
            items.Add(Opt("Rückgängig schrittweise (je 32 Felder)", _cgiPatternSteps, () => _cgiPatternSteps = !_cgiPatternSteps));
            var undoAll = new ToolStripMenuItem(L.T("Ganzen Strich rückgängig (Strg+Umschalt+Z)"));
            undoAll.Click += (s, e) => isoView.UndoWholeStroke();
            items.Add(undoAll);
        }

        private static string HeightModeText(PatternHeightMode m) => m switch
        {
            PatternHeightMode.Original => L.T("Original"),
            PatternHeightMode.Fixed => L.T("fest"),
            _ => L.T("Gelände")
        };

        /// <summary>Small input for a whole number.</summary>
        private bool AskInt(string prompt, int current, out int value)
        {
            string text = Microsoft.VisualBasic.Interaction.InputBox(prompt, Text, current.ToString());
            return int.TryParse(text, out value);
        }

        private void ActivateCgiPattern()
        {
            if (_cgiPatternStamp == null)
            {
                SetStatus(L.T("Muster-Pinsel: erst einen Stempel im Menü wählen."));
                return;
            }
            ApplyCaveEntranceRules();
            TilePattern p = TilePattern.FromStamp(_cgiPatternStamp);
            p.Freehand = _cgiPatternFreehand;
            p.Land = _cgiPatternLayers != StampLayers.StaticsOnly && p.HasLand;
            p.StaticsOn = _cgiPatternLayers != StampLayers.LandOnly;
            p.Transitions = _cgiPatternTransitions;
            p.StepCells = _cgiPatternSteps ? 32 : 0;
            p.HeightMode = _settings.PatternHeight;
            p.HeightValue = _settings.PatternHeightValue;
            p.HeightOffset = _settings.PatternHeightOffset;
            p.AutoFix = _settings.PatternAutoFix;
            isoView.Pattern = p;
            SelectCgiTool(IsoTool.PatternBrush);
            isoView.Focus();
            _cgiPatternButton.Text = "✎ " + _cgiPatternStamp.Name;
            SetStatus($"Muster-Pinsel '{p.Name}': {(p.Freehand ? L.T("ziehen = malen") : L.T("Klick = einmal setzen"))}, " +
                      $"{(p.Land ? L.T("Land") : string.Empty)}{(p.Land && p.StaticsOn ? " + " : string.Empty)}{(p.StaticsOn ? L.T("Statics") : string.Empty)}" +
                      (p.HasLand ? string.Empty : L.T(" (der Stempel hat keine gebauten Land-Tiles – nur Statics)")));
        }

        /// <summary>Shown in the window title: which state of the studio is running (to check that a new build is really used).</summary>
        public const string BuildMark = "Stand 23.11.2026 – Werkstatt 32";
        private PictureBox _picStampPreview;
        private ToolStripButton _tsbStaticIds;
        private bool _cgiLayoutBuilt;

        /// <summary>Builds the new side panel once (after the CGI controls are initialised).</summary>
        private void BuildCgiLayout()
        {
            if (_cgiLayoutBuilt)
            {
                return;
            }
            _cgiLayoutBuilt = true;
            _settings.CgiCollapsed ??= new List<string>();
            Control side = splitCgi.Panel2;
            side.SuspendLayout();
            side.Controls.Clear();
            side.BackColor = Color.FromArgb(236, 239, 244);

            // ------------------------------------------------ overview
            CollapsibleSection secOverview = OverviewSection(cgiNavigator, "overview", splitCgi.Panel2);

            // ------------------------------------------------ go to
            CollapsibleSection secGoto = NewSection("goto", "Gehe zu", ColView, 74);
            grpCgiGoto.Text = string.Empty;
            grpCgiGoto.Dock = DockStyle.Fill;
            grpCgiGoto.FlatStyle = FlatStyle.Flat;
            secGoto.Body.Padding = new Padding(2, 0, 2, 0);
            secGoto.Body.Controls.Add(grpCgiGoto);
            foreach (Control c in grpCgiGoto.Controls)
            {
                c.Top = Math.Max(2, c.Top - 14);
            }

            // ------------------------------------------------ categories
            var nav = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                Padding = new Padding(4, 2, 4, 4),
                BackColor = Color.White
            };
            AddCategory(nav, "BAUEN", ColBuild, (tabCgiItems, () => UoArtCache.Static(0x0E75, 0)), (tabCgiMulti, () => UoArtCache.Static(0x0006, 0)), (tabCgiTemplates, () => UoArtCache.Static(0x0FAF, 0)));
            AddCategory(nav, "GELÄNDE", ColTerrain, (tabCgiLand, () => UoArtCache.Land(0x0003)), (tabCgiCoast, () => UoArtCache.Land(0x00A8)), (tabCgiMountain, () => UoArtCache.Land(0x022C)));
            AddCategory(nav, "HÖHLEN", ColCave, (tabCgiCave, () => UoArtCache.Land(0x0245)), (tabCgiStamp, () => UoArtCache.Land(0x01D4)));
            AddCategory(nav, "PRÜFEN", ColCheck, (tabCgiSelection, null), (tabCgiProblems, null));
            AddCategory(nav, "PINSEL", Color.FromArgb(126, 34, 206), (EnsureBrushTab(), () => ToolIcons.Get("scatter")));
            nav.Resize += (s, e) => LayoutNav(nav);

            // ------------------------------------------------ tabs without the tab row
            tabCgiSide.Multiline = false;
            tabCgiSide.Appearance = TabAppearance.FlatButtons;
            tabCgiSide.ItemSize = new Size(0, 1);
            tabCgiSide.SizeMode = TabSizeMode.Fixed;
            tabCgiSide.Dock = DockStyle.Fill;

            // last added = top
            side.Controls.Add(tabCgiSide);
            side.Controls.Add(nav);
            side.Controls.Add(secGoto);
            side.Controls.Add(secOverview);
            side.ResumeLayout(true);
            splitCgi.Panel2MinSize = 300;
            if (splitCgi.Width > 900)
            {
                // a bit wider than before: three category buttons and the property grids fit
                splitCgi.SplitterDistance = splitCgi.Width - 370;
            }
            LayoutNav(nav);

            BuildStampTab();
            BuildCaveTab();
            BuildMountainTab();
            UpdateCgiNav();
            RefreshCgiIcons();

            // conversion to colours: always visible in the top tool bar (it has nothing to do with caves)
            var toStamp = new ToolStripButton(L.T("▣ Bereich → Farb-Stempel"))
            {
                ToolTipText = L.T("Erst mit 'Bereich' (Leiste Land) markieren: Land-Tiles + Höhen + Statics als Farb-Stempel für den Karten-Editor (Name, Kategorie, Statics-Art, fehlende Farben) – Taste P"),
                BackColor = ColTerrain,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            toStamp.Click += (s, e) => CgiAreaToColorStamp();
            var toMap = new ToolStripButton(L.T("▤ Karte → Farbkarte"))
            {
                ToolTipText = L.T("Die ganze CGI-Karte als Farbkarte + Statics-Ebene in den Karten-Editor (Nachbauen, Fehlersuche)"),
                BackColor = ColBuild,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            toMap.Click += (s, e) => CgiMapToColorMap();
            var toEditor = new ToolStripButton(L.T("▧ Bereich → in Farbkarte"))
            {
                ToolTipText = L.T("Den markierten Bereich an derselben Stelle in die geöffnete Farbkarte schreiben (Rest bleibt, Strg+Z im Karten-Editor)"),
                BackColor = Color.FromArgb(15, 118, 110),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            toEditor.Click += (s, e) => CgiAreaToEditorMap();
            var learnRules = new ToolStripButton(L.T("✦ Regeln lernen"))
            {
                ToolTipText = L.T("Aus dem markierten Bereich Map-Übergänge, Items und Static-Übergänge lernen (Land, Statics oder beides) → neue Dragon-Skripte + Regeln"),
                BackColor = Color.FromArgb(126, 34, 206),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            learnRules.Click += (s, e) => LearnRulesFromCgiArea();
            toolStripCgi.Items.Insert(0, learnRules);
            toolStripCgi.Items.Insert(0, new ToolStripSeparator());
            toolStripCgi.Items.Insert(0, toMap);
            toolStripCgi.Items.Insert(0, toEditor);
            toolStripCgi.Items.Insert(0, toStamp);
            isoView.AreaToStampRequested += (s, e) => CgiAreaToColorStamp();

            // pattern brush in the CGI view: stamps with built land tiles / statics
            _cgiPatternButton = new ToolStripDropDownButton(L.T("✎ Muster-Pinsel"))
            {
                ToolTipText = L.T("Stempel als Pinsel in der CGI-Ansicht: Land-Tiles + Statics, Freihand (Straßen, Flüsse) oder einmal – man sieht sofort, wie es entsteht"),
                BackColor = Color.FromArgb(126, 34, 206),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            _cgiPatternButton.DropDownOpening += (s, e) => FillCgiPatternMenu();
            toolStripCgiLand.Items.Add(new ToolStripSeparator());
            toolStripCgiLand.Items.Add(_cgiPatternButton);
            var staticIds = new ToolStripButton(L.T("Static-IDs")) { CheckOnClick = true, ToolTipText = L.T("ID und Höhe jedes Statics in der CGI-Ansicht einblenden") };
            _tsbStaticIds = staticIds;
            staticIds.Click += (s, e) => { isoView.ShowStaticIds = staticIds.Checked; isoView.Refresh(true); };
            int at = toolStripCgi.Items.IndexOf(tscCgiLabels);
            toolStripCgi.Items.Insert(at >= 0 ? at + 1 : toolStripCgi.Items.Count, staticIds);
            FillCgiPatternMenu();
        }

        /// <summary>Folding section; open / closed is remembered (sections that start folded remember "open").</summary>
        private CollapsibleSection NewSection(string key, string title, Color color, int bodyHeight, bool startCollapsed = false)
        {
            var s = new CollapsibleSection(L.T(title), color, bodyHeight) { Dock = DockStyle.Top };
            _sectionKeys[s] = key;
            string mark = startCollapsed ? key + ":open" : key;
            bool stored = _settings.CgiCollapsed.Contains(mark);
            s.Collapsed = startCollapsed ? !stored : stored;
            s.CollapsedChanged += (o, e) =>
            {
                _settings.CgiCollapsed.Remove(mark);
                if (startCollapsed ? !s.Collapsed : s.Collapsed)
                {
                    _settings.CgiCollapsed.Add(mark);
                }
            };
            return s;
        }

        // ================================================================== category buttons

        private void AddCategory(FlowLayoutPanel nav, string title, Color color, params (TabPage tab, Func<Image> icon)[] tabs)
        {
            var head = new Label
            {
                Text = L.T(title),
                AutoSize = false,
                Height = 16,
                ForeColor = color,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.BottomLeft,
                Margin = new Padding(2, 4, 2, 0)
            };
            nav.Controls.Add(head);
            nav.SetFlowBreak(head, true);
            Control last = null;
            foreach ((TabPage tab, Func<Image> icon) in tabs)
            {
                var b = new Button
                {
                    Text = tab.Text,
                    Height = 30,
                    FlatStyle = FlatStyle.Flat,
                    TextImageRelation = TextImageRelation.ImageBeforeText,
                    ImageAlign = ContentAlignment.MiddleLeft,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(2),
                    Font = new Font("Segoe UI", 8.25f),
                    Cursor = Cursors.Hand,
                    Tag = color
                };
                b.FlatAppearance.BorderColor = color;
                b.Click += (s, e) =>
                {
                    tabCgiSide.SelectedTab = tab;
                    UpdateCgiNav();
                };
                var entry = new NavEntry { Tab = tab, Color = color, Button = b, Icon = icon ?? (() => LetterIcon(tab.Text, color)) };
                _cgiNav.Add(entry);
                nav.Controls.Add(b);
                last = b;
            }
            if (last != null)
            {
                nav.SetFlowBreak(last, true);
            }
        }

        private static void LayoutNav(FlowLayoutPanel nav)
        {
            int w = nav.ClientSize.Width - nav.Padding.Horizontal;
            foreach (Control c in nav.Controls)
            {
                if (c is Label)
                {
                    c.Width = w - 4;
                }
                else if (c is Button)
                {
                    c.Width = Math.Max(60, w / 3 - 4);
                }
            }
        }

        /// <summary>Highlights the button of the visible tab.</summary>
        private void UpdateCgiNav()
        {
            foreach (NavEntry n in _cgiNav)
            {
                bool on = tabCgiSide.SelectedTab == n.Tab;
                n.Button.Text = n.Tab.Text;
                n.Button.BackColor = on ? n.Color : Color.FromArgb(245, 247, 250);
                n.Button.ForeColor = on ? Color.White : Color.FromArgb(30, 41, 59);
                n.Button.Font = new Font("Segoe UI", 8.25f, on ? FontStyle.Bold : FontStyle.Regular);
                n.Button.FlatAppearance.BorderSize = on ? 0 : 1;
            }
        }

        /// <summary>Pictures of the buttons / sections (land / item art once the client files are loaded).</summary>
        private void RefreshCgiIcons()
        {
            foreach (NavEntry n in _cgiNav)
            {
                Image img = null;
                try
                {
                    img = n.Icon();
                }
                catch (Exception)
                {
                    img = null;
                }
                n.Button.Image = Thumb(img ?? LetterIcon(n.Tab.Text, n.Color), 22);
            }
            foreach ((CollapsibleSection s, Func<Image> icon) in _sectionIcons)
            {
                try
                {
                    s.Icon = icon();
                }
                catch (Exception)
                {
                    s.Icon = null;
                }
                s.Invalidate(true);
            }
        }

        private static Image Thumb(Image img, int size)
        {
            var b = new Bitmap(size, size);
            using Graphics g = Graphics.FromImage(b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            float k = Math.Min((float)size / img.Width, (float)size / img.Height);
            float w = img.Width * k, h = img.Height * k;
            g.DrawImage(img, (size - w) / 2, (size - h) / 2, w, h);
            return b;
        }

        private static Image LetterIcon(string text, Color color)
        {
            var b = new Bitmap(22, 22);
            using Graphics g = Graphics.FromImage(b);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, 1, 1, 20, 20);
            using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
            TextRenderer.DrawText(g, text.Substring(0, 1).ToUpperInvariant(), font, new Rectangle(0, 0, 22, 22), Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            return b;
        }

        // ================================================================== overview window

        /// <summary>
        /// Folding section with an overview map: − / + zoom, ↕ size, ⧉ undock into its own window (closing it docks back).
        /// Used by the CGI view and the map editor.
        /// </summary>
        private CollapsibleSection OverviewSection(MapNavigator nav, string key, Control anchor)
        {
            int size = 1;
            Form window = null;
            CollapsibleSection sec = NewSection(key, "Kartenübersicht", ColView, OverviewHeights[size]);
            sec.Body.Padding = new Padding(0);
            nav.Dock = DockStyle.Fill;
            sec.Body.Controls.Add(nav);
            sec.AddHeaderButton("−", L.T("Herauszoomen (auch Mausrad)"), (s, e) => nav.ZoomStep(-1));
            sec.AddHeaderButton("+", L.T("Hineinzoomen (auch Mausrad) – rechte Maustaste schiebt, Doppelklick rechts = ganze Karte"), (s, e) => nav.ZoomStep(1));
            sec.AddHeaderButton("↕", L.T("Größe: klein / mittel / groß"), (s, e) =>
            {
                size = (size + 1) % OverviewHeights.Length;
                if (window == null)
                {
                    sec.BodyHeight = OverviewHeights[size];
                }
            });
            sec.AddHeaderButton("⧉", L.T("Übersicht in einem eigenen Fenster (abdocken / wieder andocken)"), (s, e) =>
            {
                if (window != null)
                {
                    window.Close();
                    return;
                }
                var f = new Form
                {
                    Text = L.T("Kartenübersicht – Mausrad = Zoom, rechte Taste = schieben, Klick = hinspringen"),
                    FormBorderStyle = FormBorderStyle.SizableToolWindow,
                    ShowInTaskbar = false,
                    StartPosition = FormStartPosition.Manual,
                    Size = new Size(720, 720),
                    Owner = this
                };
                Point screen = anchor.PointToScreen(Point.Empty);
                f.Location = new Point(Math.Max(0, screen.X - f.Width - 8), Math.Max(0, screen.Y));
                nav.Parent = f;
                nav.Dock = DockStyle.Fill;
                sec.Title = L.T("Kartenübersicht (abgedockt)");
                sec.BodyHeight = 0;
                sec.Invalidate(true);
                f.FormClosed += (o, ev) =>
                {
                    sec.Body.Controls.Add(nav);
                    nav.Dock = DockStyle.Fill;
                    sec.Title = L.T("Kartenübersicht");
                    sec.BodyHeight = OverviewHeights[size];
                    sec.Invalidate(true);
                    window = null;
                };
                window = f;
                f.Show(this);
            });
            return sec;
        }

        // ================================================================== map editor side panel

        private bool _editorLayoutBuilt;

        /// <summary>Map editor: overview (zoom, undock), map list and cave tool options in folding sections, colours below.</summary>
        private void BuildEditorLayout()
        {
            if (_editorLayoutBuilt)
            {
                return;
            }
            _editorLayoutBuilt = true;
            _settings.CgiCollapsed ??= new List<string>();
            Control side = splitEditor.Panel1;
            side.SuspendLayout();
            side.Controls.Clear();
            side.BackColor = Color.FromArgb(236, 239, 244);

            CollapsibleSection secOverview = OverviewSection(mapNavigator, "editor.overview", splitEditor.Panel1);
            (CollapsibleSection secInfo, CollapsibleSection secCatalog, CollapsibleSection secStamps) = BuildEditorToolSections();

            CollapsibleSection secMaps = NewSection("editor.maps", "Karten (Projekt-Ordner)", ColBuild, 220, true);
            grpLibrary.Text = string.Empty;
            grpLibrary.Dock = DockStyle.Fill;
            secMaps.Body.Padding = new Padding(2);
            secMaps.Body.Controls.Add(grpLibrary);

            _settings.CaveColor ??= new CaveColorOptions();
            var propCave = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                ToolbarVisible = false,
                HelpVisible = true,
                SelectedObject = _settings.CaveColor
            };
            CollapsibleSection secCave = NewSection("editor.cave", "Werkzeug „Höhle“ (Höhleneingang)", ColCave, 230, true);
            secCave.Body.Padding = new Padding(2);
            secCave.Body.Controls.Add(propCave);
            var caveHint = new Label
            {
                Dock = DockStyle.Top,
                Height = 34,
                Text = L.T("Klick neben den Berg = Eingang · Umschalt = unsichtbar (1d) · Rechtsklick = wieder Berg"),
                ForeColor = Color.FromArgb(71, 85, 105),
                Font = new Font("Segoe UI", 8f)
            };
            secCave.Body.Controls.Add(caveHint);
            _sectionIcons.Add((secCave, () => UoArtCache.Land(0x01D4)));

            var colorsHead = new CollapsibleSection(L.T("Farben – linke Taste = Farbe 1, rechte = Farbe 2"), ColTerrain, 0) { Dock = DockStyle.Top, Height = CollapsibleSection.HeaderHeight };
            colorsHead.CollapsedChanged += (o, e) => grpPalette.Visible = !colorsHead.Collapsed;
            grpPalette.Text = string.Empty;
            grpPalette.Dock = DockStyle.Fill;

            // last added = top
            side.Controls.Add(grpPalette);
            side.Controls.Add(colorsHead);
            side.Controls.Add(secCave);
            side.Controls.Add(secMaps);
            side.Controls.Add(secStamps);
            side.Controls.Add(secCatalog);
            side.Controls.Add(secInfo);
            side.Controls.Add(secOverview);
            // the colour grid is long: folded by default when the catalogue is open
            if (!_settings.CgiCollapsed.Contains("editor.colors:open"))
            {
                colorsHead.Collapsed = true;
            }
            colorsHead.CollapsedChanged += (o, e) =>
            {
                _settings.CgiCollapsed.Remove("editor.colors:open");
                if (!colorsHead.Collapsed)
                {
                    _settings.CgiCollapsed.Add("editor.colors:open");
                }
            };
            side.ResumeLayout(true);
            if (splitEditor.SplitterDistance < 340)
            {
                splitEditor.SplitterDistance = 350;
            }
            // quick preview of the marked area as CGI (nice coloured button in the editor tool bar)
            var preview = new ToolStripButton(L.T("▶ CGI-Vorschau"))
            {
                ToolTipText = L.T("Den markierten Bereich (Auswahl, S) nur im Speicher bauen und als CGI-Ansicht zeigen – Taste V"),
                BackColor = ColCave,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            preview.Click += (s, e) => ShowSelectionInCgi();
            toolStripEditor.Items.Add(new ToolStripSeparator());
            toolStripEditor.Items.Add(preview);
            var stampMenu = new ToolStripButton(L.T("▦ Stempel-Menü"))
            {
                ToolTipText = L.T("Farb-Stempel mit Bildern: bei mehreren Monitoren als eigenes Fenster, sonst rechts neben der Karte"),
                BackColor = ColBuild,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            stampMenu.Click += (s, e) => ToggleStampMenu();
            toolStripEditor.Items.Add(stampMenu);
            AddLiveButtons();
            var refresh = new ToolStripButton(L.T("⟳ CGI aktualisieren"))
            {
                ToolTipText = L.T("Den sichtbaren Teil der Farbkarte (oder die Auswahl) neu bauen und in die CGI-Ansicht übernehmen – der Rest der CGI-Karte bleibt"),
                BackColor = ColTerrain,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            refresh.Click += (s, e) => RefreshCgiFromEditor();
            toolStripEditor.Items.Add(refresh);
        }

        // ================================================================== sections in the tabs

        /// <summary>Scrolling column of sections for a tab page; the old content is removed.</summary>
        private FlowLayoutPanel SectionColumn(TabPage tab)
        {
            tab.Controls.Clear();
            tab.Padding = new Padding(0);
            var col = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(2, 2, 2, 2),
                BackColor = Color.FromArgb(236, 239, 244)
            };
            col.Resize += (s, e) =>
            {
                foreach (Control c in col.Controls)
                {
                    c.Width = col.ClientSize.Width - col.Padding.Horizontal - 2;
                }
            };
            tab.Controls.Add(col);
            return col;
        }

        /// <summary>
        /// Section with controls in rows: one control = full width; several = labels / numbers / check boxes keep their
        /// width, the rest share the line. Long labels (AutoSize off) get the height of their text.
        /// </summary>
        private CollapsibleSection AddSection(FlowLayoutPanel col, string key, string title, Color color, Func<Image> icon, params Control[][] rows)
            => AddSection(col, key, title, color, icon, false, rows);

        private CollapsibleSection AddSection(FlowLayoutPanel col, string key, string title, Color color, Func<Image> icon, bool startCollapsed, params Control[][] rows)
        {
            CollapsibleSection s = NewSection(key, title, color, 40, startCollapsed);
            s.Dock = DockStyle.None;
            s.Width = col.ClientSize.Width - col.Padding.Horizontal - 2;
            foreach (Control[] row in rows)
            {
                foreach (Control c in row)
                {
                    c.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                    s.Body.Controls.Add(c);
                }
            }
            void Relayout()
            {
                int y = 4, w = s.Body.ClientSize.Width - s.Body.Padding.Horizontal;
                if (w <= 20)
                {
                    return;
                }
                foreach (Control[] row in rows)
                {
                    int x = s.Body.Padding.Left, h = 0;
                    var stretch = row.Where(c => !IsFixedWidth(c)).ToList();
                    int fixedW = row.Where(IsFixedWidth).Sum(c => c.Width + 4);
                    int each = stretch.Count > 0 ? Math.Max(30, (w - fixedW - 4 * (stretch.Count - 1)) / stretch.Count) : 0;
                    foreach (Control c in row)
                    {
                        if (!IsFixedWidth(c))
                        {
                            c.Width = each;
                        }
                        if (c is Label l && !l.AutoSize)
                        {
                            l.Height = TextRenderer.MeasureText(l.Text, l.Font, new Size(c.Width, int.MaxValue), TextFormatFlags.WordBreak).Height + 6;
                        }
                        c.Location = new Point(x, y + (c is Label lbl && lbl.AutoSize ? 4 : 0));
                        x += c.Width + 4;
                        h = Math.Max(h, c.Height);
                    }
                    y += h + 5;
                }
                s.BodyHeight = y + 2;
            }
            s.Body.Resize += (o, e) => Relayout();
            col.Controls.Add(s);
            Relayout();
            if (icon != null)
            {
                _sectionIcons.Add((s, icon));
            }
            return s;
        }

        private static bool IsFixedWidth(Control c) =>
            (c is Label l && l.AutoSize) || c is NumericUpDown || c is CheckBox || (c.Tag as string) == "fixed";

        private static Control[] Row(params Control[] c) => c;

        private static Label Hint(string text) => new Label
        {
            Text = text,
            AutoSize = false,
            ForeColor = Color.FromArgb(71, 85, 105),
            Font = new Font("Segoe UI", 8f)
        };

        private void StyleButton(Button b, Color color, bool primary = false)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = color;
            b.BackColor = primary ? color : Color.White;
            b.ForeColor = primary ? Color.White : Color.FromArgb(30, 41, 59);
            b.Height = primary ? 30 : 26;
            b.Font = new Font("Segoe UI", 8.5f, primary ? FontStyle.Bold : FontStyle.Regular);
            b.Cursor = Cursors.Hand;
        }

        private void BuildStampTab()
        {
            FlowLayoutPanel col = SectionColumn(tabCgiStamp);
            _picStampPreview = new PictureBox { Height = 200, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(24, 24, 27) };
            AddSection(col, "stamp.preview", "Vorschau – so baut der Stempel", ColCave, () => UoArtCache.Land(0x01DB), Row(_picStampPreview));
            StyleButton(btnCgiStampTool, ColCave, true);
            AddSection(col, "stamp.tool", "Werkzeug", ColCave, null,
                Row(btnCgiStampTool),
                Row(Hint(L.T("Klick vor die Bergwand = setzen · Ziehen = Breite + Höhe · Strg+Klick = entfernen · Strg+Z = zurück"))));
            propCgiStamp.Dock = DockStyle.None;
            propCgiStamp.Height = 320;
            propCgiStamp.HelpVisible = true;
            AddSection(col, "stamp.settings", "Einstellungen (Variante, Größe, Höhen, Weg)", ColCave, null, Row(propCgiStamp));
            AddSection(col, "stamp.view", "Ansicht", ColView, null, Row(chkCgiAlwaysTextures), Row(chkCgiShowNoDraw));
            StyleButton(btnCgiAreaLog, ColCheck);
            StyleButton(btnCgiTransArea, ColTerrain);
            AddSection(col, "stamp.tools", "Auswerten & Übergänge (Bereich markieren)", ColCheck, null, Row(btnCgiAreaLog), Row(btnCgiTransArea));
            StyleButton(btnCgiStampSave, ColBuild);
            StyleButton(btnCgiStampPlaceSaved, ColBuild);
            lstCgiStampSaved.Height = 110;
            AddSection(col, "stamp.saved", "Gespeicherte Stempel", ColBuild, null, Row(lstCgiStampSaved), Row(btnCgiStampSave, btnCgiStampPlaceSaved));
            var toStamp = new Button { Text = L.T("Markierten Bereich → Farb-Stempel…") };
            StyleButton(toStamp, ColTerrain, true);
            toStamp.Click += (s, e) => CgiAreaToColorStamp();
            var toMap = new Button { Text = L.T("Ganze Karte → Farbkarte (Karten-Editor)…") };
            StyleButton(toMap, ColTerrain);
            toMap.Click += (s, e) => CgiMapToColorMap();
            AddSection(col, "stamp.tocolor", "In Farben umwandeln (für den Karten-Editor)", ColTerrain, () => UoArtCache.Land(0x0003),
                Row(toStamp), Row(toMap),
                Row(Hint(L.T("Land-Tiles + Höhe → Farben der Farbtabelle (fehlende: Vorschlag / automatisch / ähnlichste), Statics → Statics-Ebene, Item-Regel oder weglassen. " +
                             "Mit Items, Multis, Land und Vorlagen hier bauen oder aus einer anderen Karte kopieren – dann als Stempel speichern."))));
            lblCgiStampInfo.AutoSize = false;
            AddSection(col, "stamp.help", "Hilfe", ColView, null, true, Row(lblCgiStampInfo));
            propCgiStamp.PropertyValueChanged += (s, e) => UpdateStampPreview();
            UpdateStampPreview();
        }

        private void BuildCaveTab()
        {
            FlowLayoutPanel col = SectionColumn(tabCgiCave);
            StyleButton(btnCgiCaveTool, ColCave, true);
            StyleButton(btnCgiCaveBlack, ColView);
            AddSection(col, "cave.brush", "Höhlen-Pinsel (Boden graben, Rand wird Wand)", ColCave, () => UoArtCache.Land(0x0245),
                Row(lblCgiCaveFloor, txtCgiCaveFloor), Row(lblCgiCaveWall, numCgiCaveWall), Row(chkCgiCaveWalls), Row(chkCgiCaveTarget),
                Row(btnCgiCaveTool), Row(btnCgiCaveBlack));
            StyleButton(btnCgiCaveEntrance, ColCave, true);
            AddSection(col, "cave.entrance", "Eingang-Pinsel (frei malen)", ColCave, () => UoArtCache.Land(0x01D4),
                Row(btnCgiCaveEntrance), Row(chkCgiCaveInvisible), Row(chkCgiCaveFront), Row(lblCgiCaveMouth, numCgiCaveMouth));
            lblCgiCaveTiles.AutoSize = false;
            gridCgiCaveTiles.Height = 230;
            btnCgiCaveAddOwn.Tag = "fixed";
            StyleButton(btnCgiCaveAsFloor, ColCave);
            StyleButton(btnCgiCaveAsMouth, ColCave);
            AddSection(col, "cave.tiles", "Höhlen-Tiles (TileData 'cave' + eigene)", ColTerrain, () => UoArtCache.Land(0x01DB),
                Row(lblCgiCaveTiles), Row(gridCgiCaveTiles), Row(chkCgiCaveTextures), Row(lblCgiCaveOwn, txtCgiCaveOwn, btnCgiCaveAddOwn),
                Row(btnCgiCaveAsFloor, btnCgiCaveAsMouth));
            lstCgiCavePatterns.Height = 110;
            StyleButton(btnCgiCaveSavePattern, ColBuild);
            StyleButton(btnCgiCavePlacePattern, ColBuild);
            AddSection(col, "cave.patterns", "Eingangs-Muster (Vorlagen)", ColBuild, null,
                Row(lstCgiCavePatterns), Row(btnCgiCaveSavePattern, btnCgiCavePlacePattern));
            lblCgiCaveInfo.AutoSize = false;
            AddSection(col, "cave.help", "Hilfe", ColView, null, true, Row(lblCgiCaveInfo));
        }

        private void BuildMountainTab()
        {
            FlowLayoutPanel col = SectionColumn(tabCgiMountain);
            StyleButton(btnCgiMountainTool, ColTerrain, true);
            StyleButton(btnCgiMountainArea, ColTerrain);
            StyleButton(btnCgiMountainSeed, ColView);
            AddSection(col, "mountain.tool", "Werkzeug", ColTerrain, () => UoArtCache.Land(0x022C),
                Row(btnCgiMountainTool), Row(btnCgiMountainArea), Row(btnCgiMountainSeed));
            propCgiMountain.Dock = DockStyle.None;
            propCgiMountain.Height = 380;
            AddSection(col, "mountain.settings", "Einstellungen (Höhe, Form, Tiles)", ColTerrain, null, Row(propCgiMountain));
            lblCgiMountainInfo.AutoSize = false;
            AddSection(col, "mountain.help", "Hilfe", ColView, null, true, Row(lblCgiMountainInfo));
        }

        // ================================================================== stamp preview picture

        /// <summary>The raster of the chosen variant with the land art of every tile, its id and height.</summary>
        private void UpdateStampPreview()
        {
            if (_picStampPreview == null)
            {
                return;
            }
            CaveStampSettings s = _settings.CaveStamp ?? new CaveStampSettings();
            bool invisible = s.Variant == CaveVariant.Invisible, onGround = s.Variant == CaveVariant.OnGround;
            var rows = new List<(string name, ushort[] tiles, string z)>
            {
                ("oben", new ushort[] { 0x01D3, 0x01D4, 0x01D4, 0x01D4, 0x01D5 }, $"+{s.ArchHeight}"),
                ("Mund", new ushort[] { 0x01DA, (ushort)(invisible ? 0x0002 : 0x01DB), (ushort)(invisible ? 0x0002 : 0x01DB), (ushort)(invisible ? 0x0002 : 0x01DB), 0x01D6 }, $"+{s.FrameHeight} / +{s.MouthHeight}")
            };
            if (!onGround)
            {
                rows.Add(("vorn", invisible ? new ushort[] { 0x0245, 0x0246, 0x0247, 0x0248, 0x0249 } : new ushort[] { 0x01E6, 0x01DF, 0x01DF, 0x01DF, 0x01E4 }, "0"));
                if (s.PathLength > 0)
                {
                    rows.Add(("Weg", invisible ? new ushort[] { 0x0245, 0x0246, 0x0247, 0x0248, 0x0249 } : new ushort[] { 0x0071, 0x0073, 0x0075, 0x0077, 0x0072 }, "0"));
                }
            }
            const int cell = 52, labelW = 74, top = 4;
            var bmp = new Bitmap(labelW + cell * 5 + 8, top + rows.Count * (cell + 4) + 22);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(24, 24, 27));
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                using var font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
                using var small = new Font("Segoe UI", 7f);
                for (int r = 0; r < rows.Count; r++)
                {
                    int y = top + r * (cell + 4);
                    TextRenderer.DrawText(g, rows[r].name, font, new Rectangle(2, y + 8, labelW - 4, 16), Color.White, TextFormatFlags.Left);
                    TextRenderer.DrawText(g, "z " + rows[r].z, small, new Rectangle(2, y + 26, labelW - 4, 16), Color.FromArgb(250, 204, 21), TextFormatFlags.Left);
                    for (int c = 0; c < 5; c++)
                    {
                        ushort id = rows[r].tiles[c];
                        var rect = new Rectangle(labelW + c * cell, y, cell - 2, cell - 2);
                        Image art = null;
                        try
                        {
                            art = CaveEntranceBuilder.IsNoDraw(id) ? null : (Image)UoArtCache.Texture(id) ?? UoArtCache.Land(id);
                        }
                        catch (Exception)
                        {
                            art = null;
                        }
                        if (art != null)
                        {
                            g.DrawImage(art, rect);
                        }
                        else
                        {
                            using var b = new SolidBrush(CaveEntranceBuilder.IsNoDraw(id) ? Color.FromArgb(40, 40, 46) : Color.FromArgb(70, 70, 80));
                            g.FillRectangle(b, rect);
                        }
                        using var pen = new Pen(CaveEntranceBuilder.IsNoDraw(id) ? Color.Gold : Color.FromArgb(120, 255, 255, 255)) { DashStyle = CaveEntranceBuilder.IsNoDraw(id) ? DashStyle.Dash : DashStyle.Solid };
                        g.DrawRectangle(pen, rect);
                        using var back = new SolidBrush(Color.FromArgb(170, 0, 0, 0));
                        g.FillRectangle(back, rect.X, rect.Bottom - 13, rect.Width, 13);
                        TextRenderer.DrawText(g, id.ToString("x4"), small, new Rectangle(rect.X, rect.Bottom - 14, rect.Width, 14), Color.White, TextFormatFlags.HorizontalCenter);
                    }
                }
                string note = (s.NoDrawStatics ? "NODRAW-Statics auf der Mund-Reihe · " : string.Empty) + (s.Transitions ? "Übergänge zum Gras werden berechnet" : "ohne Übergänge");
                TextRenderer.DrawText(g, note, small, new Rectangle(4, bmp.Height - 18, bmp.Width - 8, 16), Color.FromArgb(203, 213, 225), TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            }
            Image old = _picStampPreview.Image;
            _picStampPreview.Image = bmp;
            old?.Dispose();
        }

        // ================================================================== sharper overview picture

        /// <summary>Radar colour overview up to 1024 pixels (fast, for zooming in).</summary>
        private Bitmap RadarThumbnail(MapData map, int max)
        {
            int step = Math.Max(1, (Math.Max(map.Width, map.Height) + max - 1) / max);
            int w = Math.Max(1, map.Width / step), h = Math.Max(1, map.Height / step);
            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            var colors = new Dictionary<ushort, int>();
            var buffer = new int[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    ushort id = map.LandId(x * step, y * step);
                    if (!colors.TryGetValue(id, out int c))
                    {
                        colors[id] = c = UoArtCache.LandColor(id).ToArgb();
                    }
                    buffer[y * w + x] = c;
                }
            }
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < h; y++)
                {
                    Marshal.Copy(buffer, y * w, data.Scan0 + y * data.Stride, w);
                }
            }
            finally
            {
                bmp.UnlockBits(data);
            }
            return bmp;
        }
    }
}

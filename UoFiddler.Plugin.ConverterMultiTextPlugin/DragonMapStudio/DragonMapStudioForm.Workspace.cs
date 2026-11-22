/***************************************************************************
 *
 *  * Dragon Map Studio – work space like Photoshop
 *  *
 *  *  - tool palettes at the left of the map editor and the CGI view (pictures, groups, key, help on right click),
 *  *    undock into their own window (⧉), closing the window docks them back at the left, the place is remembered
 *  *  - options bar at the top: only what the chosen tool offers; view switches in a "Ansicht" menu
 *  *  - context menu of the CGI view with most commands (tools, view incl. statics on / off, area, coast, map)
 *  *  - coast: "Küste glätten" brush, area, whole map, only errors, show errors
 *  *  - F1 / help window (German / English)
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        private bool _workspaceBuilt;
        private ToolPalette _editorPalette;
        private ToolPalette _cgiPalette;
        private ToolPaletteDock _editorDock;
        private ToolPaletteDock _cgiDock;

        // editor options bar
        private ToolStripLabel _edToolLabel;
        private ToolStripButton _edKeepHeight;
        private ToolStripDropDownButton _edView;
        private ToolStripLabel _edHint;
        private readonly List<ToolStripItem> _edSelectItems = new List<ToolStripItem>();
        private readonly List<ToolStripItem> _edBrushItems = new List<ToolStripItem>();

        // CGI options bar
        private ToolStripDropDownButton _cgiView;
        private readonly List<ToolStripItem> _cgiAreaItems = new List<ToolStripItem>();
        private readonly List<ToolStripItem> _cgiSelectItems = new List<ToolStripItem>();
        private readonly List<ToolStripItem> _cgiCoastItems = new List<ToolStripItem>();
        private ContextMenuStrip _cgiMenu;

        private HelpWindow _help;

        private void InitWorkspace()
        {
            if (_workspaceBuilt)
            {
                return;
            }
            _workspaceBuilt = true;
            _settings.CoastSmooth ??= new CoastSmoothSettings();
            isoView.CoastSmooth = _settings.CoastSmooth;
            ToolPalette.Translate = L.T;
            InitCgiBrushes();
            InitBuilders();
            BuildEditorPalette();
            BuildEditorOptionsBar();
            BuildCgiPalette();
            BuildCgiOptionsBar();
            BuildBrushOptionItems();
            BuildBuilderOptionItems();
            BuildCoastSmoothUi();
            isoView.ContextMenuRequested += IsoView_ContextMenuRequested;
            UpdateEditorOptions();
            UpdateCgiOptions();
            // the palettes come back where they were (own window or docked)
            RestorePaletteWindow(_editorDock, _settings.EditorPaletteWindow);
            RestorePaletteWindow(_cgiDock, _settings.CgiPaletteWindow);
        }

        // ================================================================== palettes

        private static readonly Color AccDraw = Color.FromArgb(124, 58, 237);
        private static readonly Color AccShape = Color.FromArgb(37, 99, 235);
        private static readonly Color AccHeight = Color.FromArgb(22, 128, 61);
        private static readonly Color AccSelect = Color.FromArgb(8, 145, 178);
        private static readonly Color AccCave = Color.FromArgb(180, 83, 9);

        private ToolPaletteEntry EdTool(string key, string name, string shortcut, string help, MapTool tool, Color accent) => new ToolPaletteEntry
        {
            Key = key,
            Name = name,
            Shortcut = shortcut,
            Help = help,
            Accent = accent,
            IsChecked = () => mapCanvas.Tool == tool,
            Click = () =>
            {
                tabMain.SelectedTab = tabEditor;
                SelectTool(tool);
                mapCanvas.Focus();
            }
        };

        private void BuildEditorPalette()
        {
            var p = new ToolPalette("Werkzeuge");
            p.Add(EdTool("brush", "Pinsel", "B", "Links = Farbe 1, rechts = Farbe 2 · [ ] = Größe · rund / eckig und 'Höhe behalten' oben", MapTool.Brush, AccDraw));
            p.Add(EdTool("eraser", "Radierer", "E", "Malt Farbe 2", MapTool.Eraser, AccDraw));
            p.Add(EdTool("spray", "Sprühen", string.Empty, "Einzelne Punkte im Pinsel", MapTool.Spray, AccDraw));
            p.Add(new ToolPaletteEntry
            {
                Key = "pattern",
                Name = "Muster-Pinsel",
                Help = ("Ein Stempel als Pinsel (Straßen, Flüsse) – ohne gewählten Stempel öffnet sich das Stempel-Menü"),
                Accent = AccDraw,
                IsChecked = () => mapCanvas.Tool == MapTool.PatternBrush,
                Click = () =>
                {
                    if (_patternStamp != null)
                    {
                        SelectTool(MapTool.PatternBrush);
                        mapCanvas.Focus();
                    }
                    else
                    {
                        ToggleStampMenu();
                    }
                }
            });
            p.AddSeparator();
            p.Add(EdTool("line", "Linie", "L", "Ziehen = Linie, Dicke = Pinselgröße", MapTool.Line, AccShape));
            p.Add(EdTool("fill", "Füllen", "F", "Füllt die zusammenhängende gleiche Farbe", MapTool.Fill, AccShape));
            p.Add(EdTool("rect", "Rechteck", string.Empty, "Rahmen, Dicke = Pinselgröße", MapTool.Rectangle, AccShape));
            p.Add(EdTool("rectfill", "Rechteck gefüllt", "R", "Gefülltes Rechteck", MapTool.FilledRectangle, AccShape));
            p.Add(EdTool("ellipse", "Ellipse", string.Empty, "Rahmen, Dicke = Pinselgröße", MapTool.Ellipse, AccShape));
            p.Add(EdTool("ellipsefill", "Ellipse gefüllt", string.Empty, "Gefüllte Ellipse", MapTool.FilledEllipse, AccShape));
            AddEditorShapeEntries(p);
            p.AddSeparator();
            p.Add(EdTool("raise", "Höhe +", string.Empty, "Nächste Farbe der Gruppe höher (rechte Maus: tiefer)", MapTool.Raise, AccHeight));
            p.Add(EdTool("lower", "Höhe −", string.Empty, "Nächste Farbe der Gruppe tiefer (rechte Maus: höher)", MapTool.Lower, AccHeight));
            p.Add(new ToolPaletteEntry
            {
                Key = "heightkeep",
                Name = "Höhe behalten (an / aus)",
                Help = ("Kein Werkzeug, ein Schalter: Pinsel, Linie, Formen und Füllen behalten die Höhe des Bodens – fehlt die Farbe auf dieser Höhe, merkt sie sich die Höhen-Ebene"),
                Accent = AccHeight,
                IsChecked = () => _settings.BrushKeepHeight,
                Click = () => ToggleKeepHeight(!_settings.BrushKeepHeight)
            });
            p.AddSeparator();
            p.Add(EdTool("picker", "Pipette", "I", "Links = Farbe 1, rechts = Farbe 2 aufnehmen · Rechtsklick-Menü", MapTool.Picker, AccSelect));
            p.Add(EdTool("select", "Auswahl", "S", "Bereich aufziehen · Strg+C / X / V · Entf · Strg+F · V = CGI-Vorschau · Rechtsklick = Menü", MapTool.Select, AccSelect));
            p.Add(EdTool("pan", "Hand", "H", "Ansicht verschieben (auch Leertaste oder mittlere Maus)", MapTool.Pan, AccSelect));
            p.Add(EdTool("caveentrance", "Höhleneingang", string.Empty, "Klick neben den Berg = Eingang · Umschalt = unsichtbar (1d) · Rechtsklick = wieder Berg", MapTool.CaveStamp, AccCave));
            p.ToolHelpRequested += (s, e) => ShowHelp(e.Key == "heightkeep" ? "heights" : e.Key == "pattern" ? "stamps" : "editor");
            _editorPalette = p;
            _editorDock = new ToolPaletteDock(p, splitEditor.Panel2, this);
            _editorDock.Changed += (s, e) => _settings.EditorPaletteWindow = PaletteWindowText(_editorDock);
        }

        private ToolPaletteEntry CgiTool(string key, string name, string shortcut, string help, IsoTool tool, Color accent, string topic = "cgi") => new ToolPaletteEntry
        {
            Key = key,
            Name = name,
            Shortcut = shortcut,
            Help = help,
            Accent = accent,
            IsChecked = () => isoView.Tool == tool,
            Click = () =>
            {
                tabMain.SelectedTab = tabCgi;
                SelectCgiTool(tool);
                isoView.Focus();
            }
        };

        private readonly Dictionary<string, string> _cgiHelpTopic = new Dictionary<string, string>();

        private void BuildCgiPalette()
        {
            var p = new ToolPalette("Werkzeuge");
            void Add(ToolPaletteEntry e, string topic)
            {
                _cgiHelpTopic[e.Key] = topic;
                p.Add(e);
            }
            Add(CgiTool("select", "Auswahl", string.Empty, "Klick = wählen · Strg = dazu · Pfeile / Bild↑↓ verschieben · Entf · Rechtsklick = Menü", IsoTool.Select, AccSelect), "cgi");
            Add(CgiTool("place", "Static setzen", string.Empty, "Item aus dem Katalog (Reiter Items) setzen · Rechtsklick auf ein Static = aufnehmen", IsoTool.Place, ColBuild), "cgi");
            Add(CgiTool("multi", "Multi setzen", string.Empty, "Multi aus dem Reiter Multis als Statics setzen", IsoTool.Multi, ColBuild), "cgi");
            Add(CgiTool("template", "Vorlage setzen", string.Empty, "Gebäude / Böden aus dem Reiter Vorlagen", IsoTool.Template, ColBuild), "cgi");
            Add(CgiTool("delete", "Löschen", string.Empty, "Klick löscht ein Static", IsoTool.Delete, ColCheck), "cgi");
            Add(CgiTool("hue", "Färben", string.Empty, "Klick färbt das Static mit der Hue (Reiter Items)", IsoTool.Hue, AccDraw), "cgi");
            Add(CgiTool("scatter", "Streu-Pinsel", string.Empty, "Bäume, Büsche, Steine aus einem Set streuen (Reiter Pinsel) · Strg = entfernen", IsoTool.Scatter, ColTerrain), "brushes");
            Add(CgiTool("huebrush", "Hue-Pinsel", string.Empty, "Alle Statics unter dem Pinsel färben · Strg = Hue 0", IsoTool.HueBrush, AccDraw), "brushes");
            Add(CgiTool("wall", "Wand / Zaun", string.Empty, "Von Ende zu Ende ziehen: Wandstücke + Pfosten (Wand-Set im Reiter Pinsel)", IsoTool.Wall, AccCave), "brushes");
            p.AddSeparator();
            Add(CgiTool("landset", "Land setzen", string.Empty, "Land-Tile malen (Reiter Land) · Rechtsklick = ID + Höhe aufnehmen", IsoTool.LandSet, AccHeight), "cgi");
            Add(CgiTool("raise", "Höhe +", string.Empty, "Land anheben (Schritt im Reiter Land)", IsoTool.Raise, AccHeight), "cgi");
            Add(CgiTool("lower", "Höhe −", string.Empty, "Land absenken", IsoTool.Lower, AccHeight), "cgi");
            Add(CgiTool("setheight", "Höhe =", string.Empty, "Land auf eine feste Höhe setzen", IsoTool.SetHeight, AccHeight), "cgi");
            Add(CgiTool("smooth", "Glätten", string.Empty, "Höhen an die Nachbarn angleichen", IsoTool.Smooth, AccHeight), "cgi");
            Add(CgiTool("area", "Bereich", string.Empty, "Rechteck markieren: F K J W G M T L P S · Rechtsklick = alle Befehle", IsoTool.Area, AccSelect), "area");
            Add(new ToolPaletteEntry
            {
                Key = "pattern",
                Name = "Muster-Pinsel",
                Help = ("Stempel mit Land-Tiles / Statics als Pinsel – Klick öffnet die Stempel-Auswahl"),
                Accent = AccDraw,
                IsChecked = () => isoView.Tool == IsoTool.PatternBrush,
                Click = () =>
                {
                    tabMain.SelectedTab = tabCgi;
                    FillCgiPatternMenu();
                    ToolPaletteEntry me = _cgiPalette.Entries.FirstOrDefault(x => x.Key == "pattern");
                    Rectangle r = me?.Bounds ?? Rectangle.Empty;
                    _cgiPatternButton.DropDown.Show(_cgiPalette, new Point(r.Right, r.Top));
                }
            }, "stamps");
            p.AddSeparator();
            Add(CgiTool("coast", "Küste", "", "Runder Pinsel: Küste wie das Original (Meeresgrund -15, Wasser-Statics, Strand)", IsoTool.Coast, ColTerrain), "coast");
            Add(CgiTool("coastsmooth", "Küste glätten", "", "Runder Pinsel: Ufer ohne Sand-Zacken, Spitzen / Buchten weg, Statics gehen mit · Bereich + J · ganze Karte / nur Fehler oben", IsoTool.CoastSmooth, ColTerrain), "coast");
            Add(CgiTool("water", "Wasser", "", "Wasser-Statics malen · Strg = entfernen", IsoTool.Water, ColTerrain), "coast");
            Add(CgiTool("mountain", "Berg", "", "Berg ab der Bodenhöhe aufbauen (Reiter Berg)", IsoTool.Mountain, ColTerrain), "caves");
            Add(CgiTool("river", "Fluss", string.Empty, "Flussbett mit Wasser-Tiles und weichem Ufer ziehen", IsoTool.River, ColBuild), "brushes");
            Add(CgiTool("road", "Weg / Straße", string.Empty, "Punkte klicken = weiche Kurve, Doppelklick / Enter = bauen", IsoTool.Road, AccCave), "brushes");
            Add(CgiTool("ramp", "Rampe", string.Empty, "Von unten nach oben ziehen = gleichmäßige Steigung", IsoTool.Ramp, AccHeight), "brushes");
            Add(CgiTool("plateau", "Plateau / Terrassen", string.Empty, "Abflachen auf eine Höhe oder Stufen", IsoTool.Plateau, AccHeight), "brushes");
            Add(CgiTool("noise", "Unebenheit", string.Empty, "Leichte Wellen ins Gelände", IsoTool.Noise, AccHeight), "brushes");
            Add(CgiTool("variants", "Varianten", string.Empty, "Tiles derselben Farbe neu mischen (gegen Kachel-Muster)", IsoTool.Variants, AccHeight), "brushes");
            Add(CgiTool("transitionbrush", "Übergänge-Pinsel", string.Empty, "Dragon-Übergänge nur unter dem Pinsel neu berechnen", IsoTool.TransitionBrush, AccHeight), "brushes");
            p.AddSeparator();
            Add(CgiTool("cave", "Höhle", string.Empty, "In den Berg ziehen: Boden auf einer Höhe, Rand wird Wand", IsoTool.Cave, AccCave), "caves");
            Add(CgiTool("caveentrance", "Eingang", string.Empty, "Höhleneingang am Bergfuß malen", IsoTool.CaveEntrance, AccCave), "caves");
            Add(CgiTool("cavestamp", "Höhlen-Stempel", string.Empty, "Klick vor eine Bergwand = ganzer Eingang · Strg+Klick = entfernen", IsoTool.CaveStamp, AccCave), "caves");
            AddCgiBuilderEntries(p, Add);
            p.AddSeparator();
            Add(CgiTool("lasso", "Lasso", string.Empty, "Punkte um eine Fläche klicken – danach wirken alle Werkzeuge nur darin · Esc = aufheben", IsoTool.Lasso, AccSelect), "brushes");
            Add(CgiTool("magicwand", "Zauberstab", string.Empty, "Klick wählt gleiches zusammenhängendes Land · Umschalt = dazu, Strg = weg", IsoTool.MagicWand, AccSelect), "brushes");
            Add(CgiTool("measure", "Messen", string.Empty, "Ziehen: Abstand, Höhenunterschied, Steigung", IsoTool.Measure, AccSelect), "brushes");
            p.ToolHelpRequested += (s, e) => ShowHelp(_cgiHelpTopic.TryGetValue(e.Key, out string t) ? t : "cgi");
            _cgiPalette = p;
            _cgiDock = new ToolPaletteDock(p, splitCgi.Panel1, this);
            _cgiDock.Changed += (s, e) => _settings.CgiPaletteWindow = PaletteWindowText(_cgiDock);
        }

        private static string PaletteWindowText(ToolPaletteDock d)
        {
            if (!d.Floating)
            {
                return string.Empty;
            }
            Rectangle r = d.WindowBounds;
            return string.Join(",", new[] { r.X, r.Y, r.Width, r.Height }.Select(v => v.ToString(CultureInfo.InvariantCulture)));
        }

        private void RestorePaletteWindow(ToolPaletteDock dock, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            int[] v = text.Split(',').Select(t => int.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : int.MinValue).ToArray();
            if (v.Length != 4 || v.Any(n => n == int.MinValue) || v[2] < 40 || v[3] < 60)
            {
                return;
            }
            // after the form is shown: the window needs a visible owner
            void Undock(object s, EventArgs e)
            {
                Shown -= Undock;
                dock.Undock(new Rectangle(v[0], v[1], v[2], v[3]));
            }
            if (Visible)
            {
                dock.Undock(new Rectangle(v[0], v[1], v[2], v[3]));
            }
            else
            {
                Shown += Undock;
            }
        }

        // ================================================================== editor options bar

        private static readonly MapTool[] BrushTools =
        {
            MapTool.Brush, MapTool.Eraser, MapTool.Spray, MapTool.Line, MapTool.Rectangle, MapTool.Ellipse, MapTool.Raise, MapTool.Lower, MapTool.PatternBrush,
            MapTool.HalfCircle, MapTool.Triangle, MapTool.SlopeLine, MapTool.Course
        };

        private static readonly MapTool[] ColourTools =
        {
            MapTool.Brush, MapTool.Eraser, MapTool.Spray, MapTool.Line, MapTool.Rectangle, MapTool.FilledRectangle, MapTool.Ellipse, MapTool.FilledEllipse, MapTool.Fill, MapTool.Select,
            MapTool.HalfCircle, MapTool.Triangle, MapTool.Course
        };

        private void BuildEditorOptionsBar()
        {
            // the tools are in the palette now: their buttons stay (shortcuts, code), only hidden
            foreach (ToolStripButton b in _toolButtons.Keys)
            {
                b.Visible = false;
            }
            tssEd1.Visible = false;
            _edToolLabel = new ToolStripLabel { Font = new Font("Segoe UI", 9f, FontStyle.Bold), ImageScaling = ToolStripItemImageScaling.None, Margin = new Padding(4, 1, 8, 2) };
            toolStripEditor.Items.Insert(0, _edToolLabel);

            _edBrushItems.AddRange(new ToolStripItem[] { tslBrushSize, tscBrushSize, tsbSquare });
            _edKeepHeight = new ToolStripButton(L.T("⛰ Höhe behalten"))
            {
                ToolTipText = L.T("Pinsel, Radierer, Sprühen, Linie, Formen und Füllen behalten die Höhe des Bodens: die Farbe der Gruppe auf dieser Höhe – fehlt sie (z. B. Holzboden nur Z 0), merkt sich die Höhen-Ebene die Höhe"),
                CheckOnClick = false
            };
            _edKeepHeight.Click += (s, e) => ToggleKeepHeight(!_settings.BrushKeepHeight);
            int at = toolStripEditor.Items.IndexOf(tsbSquare) + 1;
            toolStripEditor.Items.Insert(at++, _edKeepHeight);

            // commands of the selection
            ToolStripButton Cmd(string text, string tip, Action click)
            {
                var b = new ToolStripButton(L.T(text)) { ToolTipText = L.T(tip) };
                b.Click += (s, e) => click();
                _edSelectItems.Add(b);
                return b;
            }
            foreach (ToolStripItem item in new ToolStripItem[]
                     {
                         Cmd("Kopieren", "Strg+C", () => MnuCopy_Click(this, EventArgs.Empty)),
                         Cmd("Ausschneiden", "Strg+X", () => MnuCut_Click(this, EventArgs.Empty)),
                         Cmd("Einfügen", "Strg+V", () => MnuPaste_Click(this, EventArgs.Empty)),
                         Cmd("Füllen", "Auswahl mit Farbe 1 füllen (Strg+F)", () => MnuFillSelection_Click(this, EventArgs.Empty)),
                         Cmd("Löschen", "Auswahl mit Farbe 2 füllen (Entf)", () => MnuDeleteSelection_Click(this, EventArgs.Empty)),
                         Cmd("→ Stempel", "Auswahl als Farb-Stempel speichern", SaveSelectionAsStamp)
                     })
            {
                toolStripEditor.Items.Insert(at++, item);
            }
            _edHint = new ToolStripLabel { ForeColor = Color.FromArgb(100, 116, 139), Margin = new Padding(8, 1, 4, 2) };
            toolStripEditor.Items.Insert(at, _edHint);

            // view switches in one menu
            foreach (ToolStripItem item in new ToolStripItem[] { tsbZoomIn, tsbZoomOut, tsbZoomFit, tsbZoom100, tsbGrid, tsbHeights, tssEd2 })
            {
                item.Visible = false;
            }
            _edView = new ToolStripDropDownButton(L.T("👁 Ansicht")) { ToolTipText = L.T("Zoom, Raster, Höhen, Höhen-Ebene") };
            _edView.DropDownOpening += (s, e) => FillEditorViewMenu(_edView.DropDownItems);
            FillEditorViewMenu(_edView.DropDownItems);
            toolStripEditor.Items.Insert(toolStripEditor.Items.IndexOf(tsbUndo), _edView);
            toolStripEditor.Items.Insert(toolStripEditor.Items.IndexOf(tsbUndo), new ToolStripSeparator());

            var help = new ToolStripButton("?") { ToolTipText = L.T("Hilfe (F1)"), Alignment = ToolStripItemAlignment.Right, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            help.Click += (s, e) => ShowHelp(HelpTopicNow());
            toolStripEditor.Items.Add(help);
        }

        private void FillEditorViewMenu(ToolStripItemCollection items)
        {
            items.Clear();
            items.Add(L.T("Hineinzoomen (+)"), null, (s, e) => mapCanvas.ZoomStep(1));
            items.Add(L.T("Herauszoomen (−)"), null, (s, e) => mapCanvas.ZoomStep(-1));
            items.Add(L.T("Ganze Karte"), null, (s, e) => mapCanvas.ZoomToFit());
            items.Add(L.T("Zoom 1:1"), null, (s, e) => mapCanvas.SetZoom(1));
            items.Add(new ToolStripSeparator());
            items.Add(new ToolStripMenuItem(L.T("Raster"), null, (s, e) =>
            {
                tsbGrid.Checked = !tsbGrid.Checked;
                TsbGrid_Click(tsbGrid, EventArgs.Empty);
            }) { Checked = tsbGrid.Checked });
            items.Add(new ToolStripMenuItem(L.T("Höhen (ab Zoom 12)"), null, (s, e) =>
            {
                tsbHeights.Checked = !tsbHeights.Checked;
                TsbHeights_Click(tsbHeights, EventArgs.Empty);
            }) { Checked = tsbHeights.Checked });
            items.Add(new ToolStripMenuItem(L.T("Höhen-Ebene zeigen"), null, (s, e) => ToggleHeightLayerView(!_settings.ShowHeightLayer)) { Checked = _settings.ShowHeightLayer });
            items.Add(new ToolStripSeparator());
            items.Add(new ToolStripMenuItem(L.T("Werkzeug-Palette abdocken / andocken"), null, (s, e) =>
            {
                if (_editorDock.Floating)
                {
                    _editorPalette.FindForm()?.Close();
                }
                else
                {
                    _editorDock.Undock(null);
                }
            }) { Checked = _editorDock?.Floating ?? false });
        }

        private static string ToolTitle(MapTool t) => t switch
        {
            MapTool.Brush => "Pinsel",
            MapTool.Eraser => "Radierer",
            MapTool.Spray => "Sprühen",
            MapTool.Line => "Linie",
            MapTool.Rectangle => "Rechteck",
            MapTool.FilledRectangle => "Rechteck gefüllt",
            MapTool.Ellipse => "Ellipse",
            MapTool.FilledEllipse => "Ellipse gefüllt",
            MapTool.Fill => "Füllen",
            MapTool.Picker => "Pipette",
            MapTool.Raise => "Höhe +",
            MapTool.Lower => "Höhe −",
            MapTool.Select => "Auswahl",
            MapTool.Pan => "Hand",
            MapTool.CaveStamp => "Höhleneingang",
            MapTool.PatternBrush => "Muster-Pinsel",
            MapTool.HalfCircle => "Halbkreis",
            MapTool.Triangle => "Dreieck",
            MapTool.SlopeLine => "Neigungs-Linie",
            MapTool.Course => "Natürlicher Lauf",
            MapTool.Dungeon => "Dungeon",
            _ => t.ToString()
        };

        private static string ToolIconKey(MapTool t) => t switch
        {
            MapTool.Rectangle => "rect",
            MapTool.FilledRectangle => "rectfill",
            MapTool.FilledEllipse => "ellipsefill",
            MapTool.CaveStamp => "caveentrance",
            MapTool.PatternBrush => "pattern",
            _ => t.ToString().ToLowerInvariant()
        };

        private static string ToolHint(MapTool t) => t switch
        {
            MapTool.Brush or MapTool.Spray => "Links = Farbe 1 · Rechts = Farbe 2 · [ ] = Größe",
            MapTool.Eraser => "Malt Farbe 2 · [ ] = Größe",
            MapTool.Line or MapTool.Rectangle or MapTool.Ellipse => "Ziehen · Dicke = Pinselgröße",
            MapTool.FilledRectangle or MapTool.FilledEllipse => "Ziehen · links = Farbe 1, rechts = Farbe 2",
            MapTool.Fill => "Klick füllt die gleiche Farbe",
            MapTool.Picker => "Links = Farbe 1 · Rechts = Farbe 2 · Umschalt+Rechts = Menü",
            MapTool.Raise or MapTool.Lower => "Rechte Maus = umgekehrt",
            MapTool.Select => "Rechtsklick = Menü · V = CGI-Vorschau",
            MapTool.Pan => "Ziehen = verschieben",
            MapTool.CaveStamp => "Klick neben den Berg · Umschalt = unsichtbar · Rechts = Berg",
            MapTool.PatternBrush => "Ziehen = Muster malen · Stempel-Menü = Optionen",
            MapTool.HalfCircle => "Von der Mitte der geraden Seite zur Rundung ziehen",
            MapTool.Triangle => "Von der Mitte der Grundseite zur Spitze ziehen",
            MapTool.SlopeLine => "Ziehen: Höhe läuft vom Start zum Ende (Farbe 1, sonst Höhen-Ebene)",
            MapTool.Course => "Von der Quelle zur Mündung ziehen · Breite = Pinsel",
            MapTool.Dungeon => "Rechteck aufziehen · Farbe 1 = Boden, Farbe 2 = Wand",
            _ => string.Empty
        };

        /// <summary>Shows in the options bar what the chosen tool offers.</summary>
        private void UpdateEditorOptions()
        {
            if (_edToolLabel == null)
            {
                return;
            }
            MapTool t = mapCanvas.Tool;
            _edToolLabel.Text = L.T(ToolTitle(t));
            _edToolLabel.Image = ToolIcons.Get(ToolIconKey(t));
            bool brush = BrushTools.Contains(t);
            foreach (ToolStripItem i in _edBrushItems)
            {
                i.Visible = brush;
            }
            tssEd1.Visible = false;
            _edKeepHeight.Visible = ColourTools.Contains(t) && t != MapTool.Select;
            _edKeepHeight.Checked = _settings.BrushKeepHeight;
            _edKeepHeight.BackColor = _settings.BrushKeepHeight ? Color.FromArgb(187, 247, 208) : Color.Transparent;
            bool sel = t == MapTool.Select;
            foreach (ToolStripItem i in _edSelectItems)
            {
                i.Visible = sel;
            }
            _edHint.Text = L.T(ToolHint(t));
            UpdateBuilderEditorItems(t);
            _editorPalette?.RefreshState();
        }

        // ================================================================== CGI options bar

        private void BuildCgiOptionsBar()
        {
            foreach (ToolStripButton b in _cgiTools.Keys)
            {
                b.Visible = false;
            }
            tssCgi1.Visible = true;
            tslCgiLandTools.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            tslCgiLandTools.ImageScaling = ToolStripItemImageScaling.None;

            ToolStripButton Cmd(List<ToolStripItem> list, string text, string tip, Action click)
            {
                var b = new ToolStripButton(L.T(text)) { ToolTipText = L.T(tip) };
                b.Click += (s, e) =>
                {
                    click();
                    isoView.Focus();
                };
                list.Add(b);
                toolStripCgiLand.Items.Add(b);
                return b;
            }
            Cmd(_cgiAreaItems, "Füllen (F)", "Bereich mit dem Land-Tile füllen", () => isoView.FillArea());
            Cmd(_cgiAreaItems, "Küste (K)", "Küste wie das Original", () => isoView.ShapeCoastArea());
            Cmd(_cgiAreaItems, "Küste glätten (J)", "Ufer ohne Zacken, Statics gehen mit", () => isoView.SmoothCoastArea());
            Cmd(_cgiAreaItems, "Wasser (W)", "Wasser-Statics setzen", () => isoView.WaterArea(false));
            Cmd(_cgiAreaItems, "Glätten (G)", "Höhen glätten", () => isoView.SmoothArea());
            Cmd(_cgiAreaItems, "Berg (M)", "Berg formen (Reiter Berg)", () => isoView.MountainArea());
            Cmd(_cgiAreaItems, "Übergänge (T)", "Übergänge neu berechnen", () => isoView.TransitionArea());
            Cmd(_cgiAreaItems, "→ Stempel (P)", "Bereich als Farb-Stempel", CgiAreaToColorStamp);
            Cmd(_cgiSelectItems, "Kopieren", "Strg+C", () => isoView.CopySelection());
            Cmd(_cgiSelectItems, "Löschen", "Entf", () => isoView.DeleteSelection());
            Cmd(_cgiCoastItems, "Ganze Karte glätten", "Alle Küsten der Karte ohne Zacken (ein Rückgängig-Schritt)", () => SmoothCoastWholeMap(false));
            Cmd(_cgiCoastItems, "Nur Fehlerstellen", "Zacken, Spitzen und 1-Tile-Buchten suchen und nur dort glätten", () => SmoothCoastWholeMap(true));
            Cmd(_cgiCoastItems, "Fehler zeigen", "Küsten-Fehler rot markieren und unter Prüfen → Probleme auflisten", ShowCoastErrors);

            // view switches in one menu ("Statics" stays visible)
            foreach (ToolStripItem item in new ToolStripItem[] { tsbCgiGrid, tsbCgiHeights, tscCgiLabels, tsbCgi2D, tsbCgiSeeThrough, tsbCgiProblems, _tsbStaticIds, tsbCgiZoomIn, tsbCgiZoomOut })
            {
                if (item != null)
                {
                    item.Visible = false;
                }
            }
            _cgiView = new ToolStripDropDownButton(L.T("👁 Ansicht")) { ToolTipText = L.T("Raster, Höhen, Beschriftung, 2D, Wasser, Probleme, Static-IDs, Zoom") };
            _cgiView.DropDownOpening += (s, e) => FillCgiViewMenu(_cgiView.DropDownItems);
            FillCgiViewMenu(_cgiView.DropDownItems);
            toolStripCgi.Items.Insert(toolStripCgi.Items.IndexOf(tsbCgiStatics), _cgiView);
            var help = new ToolStripButton("?") { ToolTipText = L.T("Hilfe (F1)"), Alignment = ToolStripItemAlignment.Right, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            help.Click += (s, e) => ShowHelp(HelpTopicNow());
            toolStripCgi.Items.Add(help);
        }

        private void FillCgiViewMenu(ToolStripItemCollection items)
        {
            items.Clear();
            ToolStripMenuItem Toggle(string text, ToolStripButton b)
            {
                return new ToolStripMenuItem(L.T(text), null, (s, e) =>
                {
                    b.Checked = !b.Checked;
                    TsbCgiView_Click(b, EventArgs.Empty);
                }) { Checked = b.Checked };
            }
            items.Add(Toggle("Statics zeigen", tsbCgiStatics));
            items.Add(Toggle("Raster", tsbCgiGrid));
            items.Add(Toggle("Höhen / IDs einblenden", tsbCgiHeights));
            var labels = new ToolStripMenuItem(L.T("Beschriftung"));
            for (int i = 0; i < tscCgiLabels.Items.Count; i++)
            {
                int index = i;
                labels.DropDownItems.Add(new ToolStripMenuItem(tscCgiLabels.Items[i].ToString(), null, (s, e) =>
                {
                    tscCgiLabels.SelectedIndex = index;
                    if (!tsbCgiHeights.Checked)
                    {
                        tsbCgiHeights.Checked = true;
                        TsbCgiView_Click(tsbCgiHeights, EventArgs.Empty);
                    }
                }) { Checked = tscCgiLabels.SelectedIndex == i });
            }
            items.Add(labels);
            if (_tsbStaticIds != null)
            {
                items.Add(new ToolStripMenuItem(L.T("Static-IDs"), null, (s, e) =>
                {
                    _tsbStaticIds.Checked = !_tsbStaticIds.Checked;
                    isoView.ShowStaticIds = _tsbStaticIds.Checked;
                    isoView.Refresh(true);
                }) { Checked = _tsbStaticIds.Checked });
            }
            items.Add(Toggle("2D-Ansicht (T)", tsbCgi2D));
            items.Add(Toggle("Wasser durchsichtig", tsbCgiSeeThrough));
            items.Add(Toggle("Probleme markieren", tsbCgiProblems));
            items.Add(new ToolStripMenuItem(L.T("Unter der Karte bauen (Bau-Ebene)"), ToolIcons.Get("undermap"), (s, e) => SetUnderView(!isoView.UnderView)) { Checked = isoView.UnderView });
            items.Add(new ToolStripSeparator());
            items.Add(L.T("Hineinzoomen"), null, (s, e) => isoView.ZoomStep(1));
            items.Add(L.T("Herauszoomen"), null, (s, e) => isoView.ZoomStep(-1));
            items.Add(new ToolStripSeparator());
            items.Add(new ToolStripMenuItem(L.T("Werkzeug-Palette abdocken / andocken"), null, (s, e) =>
            {
                if (_cgiDock.Floating)
                {
                    _cgiPalette.FindForm()?.Close();
                }
                else
                {
                    _cgiDock.Undock(null);
                }
            }) { Checked = _cgiDock?.Floating ?? false });
        }

        private static string CgiToolTitle(IsoTool t) => t switch
        {
            IsoTool.Select => "Auswahl",
            IsoTool.Place => "Static setzen",
            IsoTool.Multi => "Multi setzen",
            IsoTool.Template => "Vorlage setzen",
            IsoTool.Delete => "Löschen",
            IsoTool.Hue => "Färben",
            IsoTool.LandSet => "Land setzen",
            IsoTool.Raise => "Höhe +",
            IsoTool.Lower => "Höhe −",
            IsoTool.SetHeight => "Höhe =",
            IsoTool.Smooth => "Glätten",
            IsoTool.Area => "Bereich",
            IsoTool.Coast => "Küste",
            IsoTool.CoastSmooth => "Küste glätten",
            IsoTool.Water => "Wasser",
            IsoTool.Mountain => "Berg",
            IsoTool.Cave => "Höhle",
            IsoTool.CaveEntrance => "Eingang",
            IsoTool.CaveStamp => "Höhlen-Stempel",
            IsoTool.PatternBrush => "Muster-Pinsel",
            IsoTool.Scatter => "Streu-Pinsel",
            IsoTool.HueBrush => "Hue-Pinsel",
            IsoTool.Wall => "Wand / Zaun",
            IsoTool.River => "Fluss",
            IsoTool.Road => "Weg / Straße",
            IsoTool.Ramp => "Rampe",
            IsoTool.Plateau => "Plateau / Terrassen",
            IsoTool.Noise => "Unebenheit",
            IsoTool.Variants => "Varianten",
            IsoTool.TransitionBrush => "Übergänge-Pinsel",
            IsoTool.Lasso => "Lasso",
            IsoTool.MagicWand => "Zauberstab",
            IsoTool.Measure => "Messen",
            IsoTool.Bridge => "Brücke",
            IsoTool.Cellar => "Keller",
            IsoTool.Dungeon => "Dungeon-Generator",
            IsoTool.Course => "Fluss-Lauf",
            IsoTool.MountainWall => "Bergwall",
            _ => t.ToString()
        };

        private static string CgiIconKey(IsoTool t) => t switch
        {
            IsoTool.SetHeight => "setheight",
            IsoTool.LandSet => "landset",
            IsoTool.CoastSmooth => "coastsmooth",
            IsoTool.CaveEntrance => "caveentrance",
            IsoTool.CaveStamp => "cavestamp",
            IsoTool.PatternBrush => "pattern",
            _ => t.ToString().ToLowerInvariant()
        };

        private static bool CgiBrushTool(IsoTool t) => t == IsoTool.LandSet || t == IsoTool.Raise || t == IsoTool.Lower || t == IsoTool.SetHeight ||
                                                       t == IsoTool.Smooth || t == IsoTool.Coast || t == IsoTool.CoastSmooth || t == IsoTool.Water ||
                                                       t == IsoTool.Cave || t == IsoTool.CaveEntrance || t == IsoTool.Mountain || t == IsoTool.PatternBrush ||
                                                       t == IsoTool.Scatter || t == IsoTool.HueBrush || t == IsoTool.River || t == IsoTool.Plateau || t == IsoTool.Noise ||
                                                       t == IsoTool.Variants || t == IsoTool.TransitionBrush || t == IsoTool.Ramp;

        private void UpdateCgiOptions()
        {
            if (_cgiView == null)
            {
                return;
            }
            IsoTool t = isoView.Tool;
            tslCgiLandTools.Text = L.T(CgiToolTitle(t));
            tslCgiLandTools.Image = ToolIcons.Get(CgiIconKey(t));
            bool brush = CgiBrushTool(t);
            tslCgiBrush.Visible = tscCgiBrush.Visible = tssCgiLand1.Visible = brush;
            if (_cgiPatternButton != null)
            {
                _cgiPatternButton.Visible = t == IsoTool.PatternBrush;
            }
            foreach (ToolStripItem i in _cgiAreaItems)
            {
                i.Visible = t == IsoTool.Area || t == IsoTool.Lasso || t == IsoTool.MagicWand;
            }
            UpdateBrushOptionItems(t);
            foreach (ToolStripItem i in _cgiSelectItems)
            {
                i.Visible = t == IsoTool.Select;
            }
            foreach (ToolStripItem i in _cgiCoastItems)
            {
                i.Visible = t == IsoTool.CoastSmooth || t == IsoTool.Coast;
            }
            _cgiPalette?.RefreshState();
        }

        // ================================================================== CGI context menu

        private void IsoView_ContextMenuRequested(object sender, MapPointEventArgs e)
        {
            _cgiMenu?.Dispose();
            _cgiMenu = new ContextMenuStrip();
            ToolStripItemCollection items = _cgiMenu.Items;
            bool has = _cgiMap != null;
            Point client = isoView.PointToClient(Control.MousePosition);
            StaticItem hit = has ? isoView.StaticAt(client) : null;
            bool onMap = has && _cgiMap.InBounds(e.X, e.Y);

            // ---- what is under the mouse
            if (hit != null)
            {
                var head = new ToolStripMenuItem($"{hit.Id:x4} {UoArtCache.ItemName(hit.Id)}  (z {hit.Z}{(hit.Hue != 0 ? ", hue " + hit.Hue : string.Empty)})") { Enabled = false };
                try
                {
                    Image art = UoArtCache.Static(hit.Id, hit.Hue);
                    head.Image = art == null ? null : new Bitmap(art, new Size(16, 16));
                }
                catch (Exception)
                {
                    head.Image = null;
                }
                items.Add(head);
                items.Add(L.T("Item übernehmen (zum Setzen)"), null, (s, a) => IsoView_ItemPicked(isoView, new StaticPickEventArgs(hit)));
                items.Add(L.T("Static auswählen"), null, (s, a) =>
                {
                    SelectCgiTool(IsoTool.Select);
                    isoView.SelectStatic(hit);
                });
                items.Add(L.T("Static löschen"), null, (s, a) =>
                {
                    isoView.SelectStatic(hit);
                    isoView.DeleteSelection();
                });
                items.Add(new ToolStripSeparator());
            }
            if (onMap)
            {
                ushort land = _cgiMap.LandId(e.X, e.Y);
                items.Add(new ToolStripMenuItem($"{L.T("Land")} {land:x4} {UoArtCache.LandName(land)}  (z {_cgiMap.LandZ(e.X, e.Y)}) · {e.X}, {e.Y}") { Enabled = false });
                items.Add(L.T("Land-Tile + Höhe übernehmen"), null, (s, a) => IsoView_LandPicked(isoView, new MapPointEventArgs(e.X, e.Y, land)));
                items.Add(new ToolStripSeparator());
            }

            // ---- tools
            var tools = new ToolStripMenuItem(L.T("Werkzeug"));
            foreach (ToolPaletteEntry pe in _cgiPalette.Entries.Where(x => x.Name != null))
            {
                ToolPaletteEntry entry = pe;
                bool on = SafeCheck(entry);
                tools.DropDownItems.Add(new ToolStripMenuItem(L.T(entry.Name) + (string.IsNullOrEmpty(entry.Shortcut) ? string.Empty : $"  ({entry.Shortcut})"),
                    ToolIcons.Get(entry.Key), (s, a) => entry.Click()) { Checked = on });
            }
            items.Add(tools);

            // ---- view
            var view = new ToolStripMenuItem(L.T("Ansicht"));
            FillCgiViewMenu(view.DropDownItems);
            items.Add(view);
            items.Add(new ToolStripMenuItem(L.T("Statics zeigen"), null, (s, a) =>
            {
                tsbCgiStatics.Checked = !tsbCgiStatics.Checked;
                TsbCgiView_Click(tsbCgiStatics, EventArgs.Empty);
            }) { Checked = tsbCgiStatics.Checked });

            // ---- area
            bool area = has && !isoView.Area.IsEmpty;
            var areaMenu = new ToolStripMenuItem(L.T("Bereich") + (area ? $"  ({isoView.Area.Width}×{isoView.Area.Height})" : string.Empty)) { Enabled = has };
            void A(string text, Action act, bool needArea = true)
            {
                areaMenu.DropDownItems.Add(L.T(text), null, (s, a) => act()).Enabled = !needArea || area;
            }
            A("Bereich markieren (Werkzeug)", () => SelectCgiTool(IsoTool.Area), false);
            areaMenu.DropDownItems.Add(new ToolStripSeparator());
            A("Mit Land-Tile füllen (F)", () => isoView.FillArea());
            A("Küste anpassen (K)", () => isoView.ShapeCoastArea());
            A("Küste glätten – ohne Zacken (J)", () => isoView.SmoothCoastArea());
            A("Wasser-Statics setzen (W)", () => isoView.WaterArea(false));
            A("Wasser-Statics entfernen (Umschalt+W)", () => isoView.WaterArea(true));
            A("Höhen glätten (G)", () => isoView.SmoothArea());
            A("Heben (Bild↑)", () => isoView.RaiseArea(1));
            A("Senken (Bild↓)", () => isoView.RaiseArea(-1));
            A("Berg formen (M)", () => isoView.MountainArea());
            A("Übergänge neu berechnen (T)", () => isoView.TransitionArea());
            areaMenu.DropDownItems.Add(new ToolStripSeparator());
            A("Kopieren (Strg+C)", () => isoView.CopyArea());
            A("Einfügen (Strg+V)", () =>
            {
                SelectCgiTool(IsoTool.Area);
                isoView.BeginAreaPaste();
            }, false);
            A("Statics darin auswählen (S)", () => isoView.SelectAreaStatics());
            A("Statics darin löschen (Entf)", () => isoView.DeleteAreaStatics());
            areaMenu.DropDownItems.Add(new ToolStripSeparator());
            A("→ Farb-Stempel (P)", CgiAreaToColorStamp);
            A("→ in die Farbkarte (gleiche Stelle)", CgiAreaToEditorMap);
            A("Regeln lernen", LearnRulesFromCgiArea);
            A("Bereich auswerten (Log)", () => BtnCgiAreaLog_Click(this, EventArgs.Empty));
            A("Dungeon-Werkstatt für diesen Platz …", () => OpenDungeonWorkshop(WorkshopKind.Classic));
            A("Keller-Werkstatt für diesen Platz …", () => OpenDungeonWorkshop(WorkshopKind.Cellar));
            A("Dungeon analysieren (Log) …", () => AnalyzeDungeonArea(false));
            A("Dungeon-Stil aus dem Bereich lernen …", () => AnalyzeDungeonArea(true));
            A("Markierung entfernen (Esc)", () => isoView.ClearArea());
            items.Add(areaMenu);

            // ---- coast
            var coast = new ToolStripMenuItem(L.T("Küste")) { Enabled = has, Image = ToolIcons.Get("coastsmooth") };
            coast.DropDownItems.Add(L.T("Pinsel 'Küste glätten'"), ToolIcons.Get("coastsmooth"), (s, a) => SelectCgiTool(IsoTool.CoastSmooth));
            coast.DropDownItems.Add(L.T("Im Bereich glätten (J)"), null, (s, a) => isoView.SmoothCoastArea()).Enabled = area;
            coast.DropDownItems.Add(L.T("Ganze Karte glätten"), null, (s, a) => SmoothCoastWholeMap(false));
            coast.DropDownItems.Add(L.T("Nur Fehlerstellen glätten"), null, (s, a) => SmoothCoastWholeMap(true));
            coast.DropDownItems.Add(L.T("Küsten-Fehler zeigen"), null, (s, a) => ShowCoastErrors());
            coast.DropDownItems.Add(new ToolStripSeparator());
            coast.DropDownItems.Add(L.T("Einstellungen (Reiter Küste / Wasser)"), null, (s, a) => tabCgiSide.SelectedTab = tabCgiCoast);
            items.Add(coast);

            AddBrushMenu(items, e);

            // ---- map
            var map = new ToolStripMenuItem(L.T("Karte"));
            map.DropDownItems.Add(L.T("Aus Editor-Karte bauen"), null, (s, a) => TsbCgiBuild_Click(this, EventArgs.Empty));
            map.DropDownItems.Add(L.T("MUL / UOP laden …"), null, (s, a) => TsbCgiLoad_Click(this, EventArgs.Empty));
            map.DropDownItems.Add(L.T("Speichern"), null, (s, a) => TsbCgiSave_Click(this, EventArgs.Empty)).Enabled = has;
            map.DropDownItems.Add(L.T("Karte → Farbkarte"), null, (s, a) => CgiMapToColorMap()).Enabled = has;
            map.DropDownItems.Add(L.T("Ganze Karte prüfen (Probleme)"), null, (s, a) => BtnCgiScan_Click(this, EventArgs.Empty)).Enabled = has;
            map.DropDownItems.Add(L.T("Diese Stelle im Karten-Editor zeigen"), null, (s, a) =>
            {
                if (_map != null && _map.InBounds(e.X, e.Y))
                {
                    tabMain.SelectedTab = tabEditor;
                    mapCanvas.CenterOn(e.X + 0.5, e.Y + 0.5);
                }
            }).Enabled = onMap && _map != null && _map.InBounds(e.X, e.Y);
            items.Add(map);

            items.Add(new ToolStripSeparator());
            items.Add(L.T("Rückgängig (Strg+Z)"), null, (s, a) => isoView.Undo()).Enabled = has && _cgiMap.CanUndo;
            items.Add(L.T("Wiederholen (Strg+Y)"), null, (s, a) => isoView.Redo()).Enabled = has && _cgiMap.CanRedo;
            items.Add(L.T("Ganzen Strich rückgängig (Strg+Umschalt+Z)"), null, (s, a) => isoView.UndoWholeStroke()).Enabled = has && _cgiMap.CanUndo;
            items.Add(new ToolStripSeparator());
            items.Add(L.T("Hilfe (F1)"), null, (s, a) => ShowHelp("cgimenu"));
            _cgiMenu.Show(isoView, client);
        }

        private static bool SafeCheck(ToolPaletteEntry e)
        {
            try
            {
                return e.IsChecked?.Invoke() ?? false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ================================================================== coast smoothing (tab Küste / Wasser)

        private void BuildCoastSmoothUi()
        {
            const int height = 360;
            foreach (Control c in panelCgiCoast.Controls)
            {
                c.Top += height;
            }
            var box = new GroupBox
            {
                Text = L.T("Küste glätten (ohne Zacken)"),
                Location = new Point(4, 4),
                Size = new Size(280, height - 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            var hint = new Label
            {
                Text = L.T("Ufer-Höhen folgen dem echten Abstand zur Küste: keine Sand-Zacken, die Tiefe bleibt und verschiebt sich mit der Küste. Pinsel 'Küste glätten' (rund), Bereich + J, oder:"),
                Location = new Point(6, 18),
                Size = new Size(266, 46),
                ForeColor = Color.FromArgb(71, 85, 105),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            var grid = new PropertyGrid
            {
                Location = new Point(6, 66),
                Size = new Size(266, 180),
                ToolbarVisible = false,
                HelpVisible = true,
                PropertySort = PropertySort.Categorized,
                SelectedObject = _settings.CoastSmooth,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            grid.PropertyValueChanged += (s, e) => isoView.CoastSmooth = _settings.CoastSmooth;
            Button B(string text, int y, Color c, bool primary, Action click)
            {
                var b = new Button { Text = L.T(text), Location = new Point(6, y), Size = new Size(266, 24), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                StyleButton(b, c, primary);
                b.Height = 24;
                b.Click += (s, e) => click();
                return b;
            }
            box.Controls.Add(hint);
            box.Controls.Add(grid);
            box.Controls.Add(B("Pinsel 'Küste glätten' wählen", 250, ColTerrain, true, () => SelectCgiTool(IsoTool.CoastSmooth)));
            box.Controls.Add(B("Markierten Bereich glätten (J)", 276, ColTerrain, false, () => isoView.SmoothCoastArea()));
            box.Controls.Add(B("Ganze Karte glätten", 302, ColBuild, false, () => SmoothCoastWholeMap(false)));
            var errors = B("Nur Fehlerstellen", 328, ColCheck, false, () => SmoothCoastWholeMap(true));
            errors.Width = 130;
            errors.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            var show = B("Fehler zeigen", 328, ColCheck, false, ShowCoastErrors);
            show.Left = 142;
            show.Width = 130;
            box.Controls.Add(errors);
            box.Controls.Add(show);
            panelCgiCoast.Controls.Add(box);
        }

        private bool _coastBusy;

        /// <summary>Whole map or only the error places – in the background, then one undo step.</summary>
        private void SmoothCoastWholeMap(bool onlyErrors)
        {
            if (_cgiMap == null || _coastBusy)
            {
                SetStatus(L.T("Keine CGI-Karte geladen."));
                return;
            }
            MapData map = _cgiMap;
            CoastSettings coast = isoView.Coast;
            CoastSmoothSettings o = _settings.CoastSmooth.Copy();
            Rectangle all = isoView.Area.IsEmpty ? new Rectangle(0, 0, map.Width, map.Height) : isoView.Area;
            bool inArea = !isoView.Area.IsEmpty;
            _coastBusy = true;
            SetStatus(onlyErrors ? L.T("Küste: suche Fehlerstellen …") : L.T("Küste: glätte die ganze Karte …"));
            Cursor = Cursors.WaitCursor;
            var progress = new Progress<int>(p => SetStatus(L.T("Küste glätten") + $" … {p} %"));
            Task.Run(() =>
            {
                int errors = 0;
                CoastEdit edit = onlyErrors
                    ? CoastSmoother.SmoothErrors(map, all, coast, o, new Random(), out errors, progress)
                    : CoastSmoother.Smooth(map, all, coast, o, new Random(), progress: progress);
                return (edit, errors);
            }).ContinueWith(t =>
            {
                if (IsDisposed)
                {
                    return;
                }
                BeginInvoke(new Action(() =>
                {
                    _coastBusy = false;
                    Cursor = Cursors.Default;
                    if (t.IsFaulted)
                    {
                        SetStatus(L.T("Küste glätten fehlgeschlagen") + ": " + t.Exception?.GetBaseException().Message);
                        return;
                    }
                    if (!ReferenceEquals(map, _cgiMap))
                    {
                        SetStatus(L.T("Die CGI-Karte wurde inzwischen gewechselt – nichts geändert."));
                        return;
                    }
                    (CoastEdit edit, int errors) = t.Result;
                    string where = inArea ? L.T("im markierten Bereich") : L.T("auf der ganzen Karte");
                    if (edit.Count == 0)
                    {
                        SetStatus(onlyErrors && errors == 0 ? L.T("Keine Küsten-Fehler gefunden") + " " + where + "." : L.T("Küste: nichts zu ändern") + " " + where + ".");
                        return;
                    }
                    isoView.ApplyCoastEdit(edit, (onlyErrors ? $"{errors:N0} " + L.T("Fehlerstellen geglättet") : L.T("Küste geglättet")) + " " + where +
                                                  $": {edit.Land.Count:N0} " + L.T("Land-Tiles") + $", {edit.Statics.Count:N0} Statics – Strg+Z " + L.T("nimmt es zurück") + ".");
                    UpdateCgiState();
                }));
            });
        }

        /// <summary>Marks the coast errors in red and lists them under Prüfen → Probleme.</summary>
        private void ShowCoastErrors()
        {
            if (_cgiMap == null)
            {
                SetStatus(L.T("Keine CGI-Karte geladen."));
                return;
            }
            Rectangle all = isoView.Area.IsEmpty ? new Rectangle(0, 0, _cgiMap.Width, _cgiMap.Height) : isoView.Area;
            Cursor = Cursors.WaitCursor;
            List<Point> found;
            try
            {
                found = CoastSmoother.FindErrors(_cgiMap, all, isoView.Coast, _settings.CoastSmooth);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
            _problems = found.Take(5000).Select(p => new MapProblem { Kind = MapProblemKind.Coast, X = p.X, Y = p.Y, Text = L.T("Küste: Zacke / Spitze / Bucht") }).ToList();
            _fillingProblems = true;
            lstCgiProblems.DataSource = null;
            lstCgiProblems.DataSource = _problems;
            _fillingProblems = false;
            isoView.SetProblems(_problems);
            if (!tsbCgiProblems.Checked)
            {
                tsbCgiProblems.Checked = true;
                TsbCgiView_Click(tsbCgiProblems, EventArgs.Empty);
            }
            SetStatus(found.Count == 0 ? L.T("Keine Küsten-Fehler gefunden.") : $"{found.Count:N0} " + L.T("Küsten-Fehlerstellen (rot, Liste unter Prüfen → Probleme, Doppelklick = hinspringen)."));
            if (found.Count > 0)
            {
                tabCgiSide.SelectedTab = tabCgiProblems;
                UpdateCgiNav();
            }
        }

        // ================================================================== help

        /// <summary>Topic of what is on screen now (tab, tool).</summary>
        private string HelpTopicNow()
        {
            if (tabMain.SelectedTab == tabCgi)
            {
                return isoView.Tool switch
                {
                    IsoTool.Area => "area",
                    IsoTool.Coast or IsoTool.CoastSmooth or IsoTool.Water => "coast",
                    IsoTool.Cave or IsoTool.CaveEntrance or IsoTool.CaveStamp or IsoTool.Mountain => "caves",
                    IsoTool.PatternBrush => "stamps",
                    IsoTool.Bridge or IsoTool.Cellar or IsoTool.Dungeon or IsoTool.Course or IsoTool.MountainWall => "build",
                    >= IsoTool.Scatter => "brushes",
                    _ => "cgi"
                };
            }
            if (tabMain.SelectedTab == tabEditor)
            {
                return mapCanvas.Tool switch
                {
                    MapTool.PatternBrush => "stamps",
                    MapTool.HalfCircle or MapTool.Triangle or MapTool.SlopeLine or MapTool.Course or MapTool.Dungeon => "shapes",
                    _ => "editor"
                };
            }
            return "start";
        }

        private void ShowHelp(string topic)
        {
            if (_help == null || _help.IsDisposed)
            {
                bool english = _settings.HelpLanguage == "en" || (string.IsNullOrEmpty(_settings.HelpLanguage) && L.Current == UiLanguage.English);
                _help = new HelpWindow(english);
                _help.LanguageChanged += (s, en) => _settings.HelpLanguage = en ? "en" : "de";
                _help.Show(this);
            }
            _help.ShowTopic(topic);
            _help.Activate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                ShowHelp(HelpTopicNow());
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}

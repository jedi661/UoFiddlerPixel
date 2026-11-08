/***************************************************************************
 *
 * MultiEditor – extended functions
 *  menu / toolbar / context menus, zoom, multi selection, copy & paste,
 *  rectangle / line / box select / erase area tools, keyboard shortcuts,
 *  tile group editor, import / export, multi.mul and statics writing,
 *  background (pattern / image / map), house generator, help, DE/EN.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.MultiEditor.Classes;
using UoFiddler.Plugin.MultiEditor.Forms;

namespace UoFiddler.Plugin.MultiEditor.UserControls
{
    public partial class MultiEditorControl
    {
        private enum ExtTool
        {
            None,
            Rect,
            Line,
            BoxSelect,
            EraseArea,
            Structure,
            MapSelect
        }

        private const string _specialPrefix = "__";

        private ExtTool _extTool = ExtTool.None;
        private readonly HashSet<MultiTile> _selection = new HashSet<MultiTile>();
        private List<ImportTile> _clipboard;
        private bool _pasteMode;
        private float _zoom = 1f;

        private bool _dragging;
        private Point _dragStartCell;
        private Point _dragEndCell;
        private int _dragZ;

        private Point _rightDownPos;
        private bool _rightMoved;

        private readonly BackgroundLayer _background = new BackgroundLayer();
        private bool _snapToGround = true;
        private bool _swapWallsOnRotate = true;
        private int _drawHue;
        private int _minVisibleZ = -128;

        private MenuStrip _menu;
        private ToolStrip _toolBar;
        private ContextMenuStrip _multiContext;
        private ContextMenuStrip _paletteContext;
        private ContextMenuStrip _treeContext;
        private ToolStripButton _tbRect;
        private ToolStripButton _tbLine;
        private ToolStripButton _tbBox;
        private ToolStripButton _tbErase;
        private ToolStripButton _tbGrid;
        private ToolStripButton _tbHeights;
        private ToolStripLabel _zoomLabel;
        private ToolStripLabel _infoLabel;
        private NumericUpDown _hueBox;
        private NumericUpDown _minZBox;
        private Button _focusSink;
        private TabPage _extraTab;
        private Font _heightFont;

        private MultiTile _contextTile;
        private Point _contextCell;
        private int _contextZ;
        private int _paletteContextIndex = -1;

        private static EditorSettings Settings => EditorSettings.Instance;

        public int MinVisibleZ => _minVisibleZ;

        public bool ShowInvisibleMarked => Settings.ShowInvisible;

        public bool IsInSelection(MultiTile tile)
        {
            return _selection.Count > 0 && _selection.Contains(tile);
        }

        #region Init

        private void InitializeExtensions()
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }

            _heightFont = new Font("Arial", 7f, FontStyle.Bold);

            _focusSink = new Button { Size = new Size(1, 1), Location = new Point(-20, -20), TabStop = false };
            splitContainer2.Panel2.Controls.Add(_focusSink);

            _multiContext = new ContextMenuStrip();
            _multiContext.Opening += MultiContext_Opening;

            _paletteContext = new ContextMenuStrip();
            _paletteContext.Opening += PaletteContext_Opening;
            pictureBoxDrawTiles.ContextMenuStrip = _paletteContext;
            pictureBoxDrawTiles.MouseDown += (_, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    _paletteContextIndex = GetIndex(e.X / (_drawTileSizeWidth - 1), e.Y / (_drawTileSizeHeight - 1));
                }
            };

            _treeContext = new ContextMenuStrip();
            _treeContext.Opening += TreeContext_Opening;
            treeViewTilesXML.ContextMenuStrip = _treeContext;
            treeViewTilesXML.NodeMouseClick += (_, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    treeViewTilesXML.SelectedNode = e.Node;
                }
            };
            treeViewTilesXML.BeforeSelect += TreeViewTilesXML_BeforeSelect;

            pictureBoxMulti.MouseWheel += PictureBoxMulti_MouseWheel;
            pictureBoxMulti.MouseLeave += (_, _) => pictureBoxMulti.Invalidate();

            _infoLabel = new ToolStripLabel { Alignment = ToolStripItemAlignment.Right };
            toolStrip1.Items.Add(_infoLabel);

            var redo = new ToolStripMenuItem("Redo");
            redo.Click += (_, _) => DoRedo();
            toolStripDropDownButton1.DropDownItems.Insert(1, redo);
            toolStripDropDownButton1.DropDownOpening += (_, _) =>
            {
                redo.Text = L.T("Wiederholen (Strg+Y)", "Redo (Ctrl+Y)");
                redo.Enabled = _compList?.CanRedo == true;
                UndoItems.Text = L.T("Rückgängig", "Undo");
            };

            InitializeStructures();
            BuildUi();
        }

        /// <summary>
        /// (Re)creates all texts depending on the language
        /// </summary>
        private void BuildUi()
        {
            SuspendLayout();

            if (_menu != null)
            {
                splitContainer1.Panel2.Controls.Remove(_menu);
                _menu.Dispose();
            }

            if (_toolBar != null)
            {
                splitContainer1.Panel2.Controls.Remove(_toolBar);
                _toolBar.Dispose();
            }

            BuildToolBar();
            BuildMenu();
            BuildStructureToolBar();
            AddHouseToolButtons();
            AddHouseToolMenus();
            splitContainer1.Panel2.Controls.Add(_toolBar);
            splitContainer1.Panel2.Controls.Add(_menu);

            BuildExtraTab();
            BuildDesignTabButtons();
            BuildLayersTab();
            BuildPrefabTab();
            SetToolTips();
            UpdateInfo();

            ResumeLayout(true);
        }

        private void SetToolTips()
        {
            toolTip1.SetToolTip(BTN_Select, L.T("Auswählen (1) – Strg+Klick = Mehrfachauswahl", "Select (1) – Ctrl+click = multi selection"));
            toolTip1.SetToolTip(BTN_Draw, L.T("Zeichnen (2)", "Draw (2)"));
            toolTip1.SetToolTip(BTN_Remove, L.T("Entfernen (3)", "Remove (3)"));
            toolTip1.SetToolTip(BTN_Z, L.T("Höhe ändern um Wert (4)", "Change altitude by value (4)"));
            toolTip1.SetToolTip(BTN_Pipette, L.T("Pipette (5)", "Pipette (5)"));
            toolTip1.SetToolTip(BTN_Trans, L.T("Transparent umschalten (6)", "Toggle transparent (6)"));
            toolTip1.SetToolTip(BTN_Floor, L.T("Virtueller Boden (Bau-Ebene)", "Virtual floor (building level)"));
            toolTip1.SetToolTip(numericUpDown_Floor, L.T("Höhe des virtuellen Bodens", "Height of the virtual floor"));
            toolTip1.SetToolTip(numericUpDown_Z, L.T("Wert für das Höhen-Werkzeug", "Value for the altitude tool"));
        }

        private static ToolStripMenuItem Item(string text, EventHandler click, Keys keys = Keys.None)
        {
            var i = new ToolStripMenuItem(text);
            if (click != null)
            {
                i.Click += click;
            }

            if (keys != Keys.None)
            {
                i.ShortcutKeyDisplayString = new KeysConverter().ConvertToString(keys);
            }

            return i;
        }

        private void BuildMenu()
        {
            _menu = new MenuStrip { Dock = DockStyle.Top };

            var file = new ToolStripMenuItem(L.T("&Datei", "&File"));
            file.DropDownItems.Add(Item(L.T("Neues leeres Multi…", "New blank multi…"), (_, _) => NewBlankAsk()));
            file.DropDownItems.Add(Item(L.T("Öffnen (alle Formate)…", "Open (all formats)…"), (_, _) => OpenAnyFile(NumberMode.Auto), Keys.Control | Keys.O));
            var openAs = new ToolStripMenuItem(L.T("Öffnen als", "Open as"));
            openAs.DropDownItems.Add(Item(L.T("Text dezimal…", "Text decimal…"), (_, _) => OpenAnyFile(NumberMode.Decimal)));
            openAs.DropDownItems.Add(Item(L.T("Text hex…", "Text hex…"), (_, _) => OpenAnyFile(NumberMode.Hex)));
            file.DropDownItems.Add(openAs);
            var recent = new ToolStripMenuItem(L.T("Zuletzt geöffnet", "Recent files"));
            recent.DropDownOpening += (_, _) =>
            {
                recent.DropDownItems.Clear();
                foreach (string f in Settings.RecentFiles)
                {
                    string path = f;
                    recent.DropDownItems.Add(Item(path, (_, _) => OpenFile(path, NumberMode.Auto)));
                }

                if (recent.DropDownItems.Count == 0)
                {
                    recent.DropDownItems.Add(new ToolStripMenuItem("---") { Enabled = false });
                }
            };
            recent.DropDownItems.Add("---");
            file.DropDownItems.Add(recent);
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(Item(L.T("Projekt speichern (.mep)…", "Save project (.mep)…"), (_, _) => SaveProject(), Keys.Control | Keys.S));
            file.DropDownItems.Add(Item(L.T("ServUO Addon exportieren (.cs)…", "Export ServUO addon (.cs)…"), (_, _) => ExportServUOAddon()));
            var export = new ToolStripMenuItem(L.T("Exportieren als", "Export as"));
            export.DropDownItems.Add(Item(L.T("Text hex (0x…)", "Text hex (0x…)"), (_, _) => ExportGeneric("txthex")));
            export.DropDownItems.Add(Item(L.T("Text dezimal", "Text decimal"), (_, _) => ExportGeneric("txtdec")));
            export.DropDownItems.Add(Item(L.T("WSC mit Farben", "WSC with hues"), (_, _) => ExportGeneric("wsc")));
            export.DropDownItems.Add(Item(L.T("XML mit Farben", "XML with hues"), (_, _) => ExportGeneric("xml")));
            export.DropDownItems.Add(Item(L.T("Vorschau-Bild (PNG)", "Preview image (PNG)"), (_, _) => ExportGeneric("png")));
            file.DropDownItems.Add(export);
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(Item(L.T("In Multi.mul schreiben…", "Write to Multi.mul…"), (_, _) => SaveToMultiMul()));
            file.DropDownItems.Add(Item(L.T("Nächste freie Multi-ID", "Next free multi id"), (_, _) => FindNextFreeId()));
            file.DropDownItems.Add(Item(L.T("Multi-ID löschen…", "Delete multi id…"), (_, _) => DeleteMultiId()));

            var edit = new ToolStripMenuItem(L.T("&Bearbeiten", "&Edit"));
            edit.DropDownItems.Add(Item(L.T("Rückgängig", "Undo"), (_, _) => DoUndo(), Keys.Control | Keys.Z));
            edit.DropDownItems.Add(Item(L.T("Wiederholen", "Redo"), (_, _) => DoRedo(), Keys.Control | Keys.Y));
            edit.DropDownItems.Add(new ToolStripSeparator());
            edit.DropDownItems.Add(Item(L.T("Kopieren", "Copy"), (_, _) => CopySelection(false), Keys.Control | Keys.C));
            edit.DropDownItems.Add(Item(L.T("Ausschneiden", "Cut"), (_, _) => CopySelection(true), Keys.Control | Keys.X));
            edit.DropDownItems.Add(Item(L.T("Einfügen", "Paste"), (_, _) => StartPaste(), Keys.Control | Keys.V));
            edit.DropDownItems.Add(Item(L.T("Auswahl löschen", "Delete selection"), (_, _) => DeleteSelection(), Keys.Delete));
            edit.DropDownItems.Add(Item(L.T("Alles auswählen", "Select all"), (_, _) => SelectAll(), Keys.Control | Keys.A));
            edit.DropDownItems.Add(Item(L.T("Auswahl aufheben", "Clear selection"), (_, _) => ClearSelection(), Keys.Escape));
            edit.DropDownItems.Add(new ToolStripSeparator());
            edit.DropDownItems.Add(Item(L.T("Auswahl als Einrichtung markieren", "Mark selection as furniture"), (_, _) => SetDecoOfWorkSet(true)));
            edit.DropDownItems.Add(Item(L.T("Einrichtungs-Markierung entfernen", "Remove furniture mark"), (_, _) => SetDecoOfWorkSet(false)));
            edit.DropDownItems.Add(Item(L.T("Auswahl als Bauteil speichern…", "Save selection as building part…"), (_, _) =>
                SavePrefab(GetWorkSet().Select(t => new ImportTile { Id = t.Id, X = t.X, Y = t.Y, Z = t.Z, Flag = t.Flag, Hue = t.Hue, Deco = t.IsDeco }).ToList())));
            edit.DropDownItems.Add(Item(L.T("IDs ersetzen…", "Replace ids…"), (_, _) => ReplaceIdsDialog()));
            edit.DropDownItems.Add(Item(L.T("Hue der Auswahl setzen (Werkzeugleiste)", "Set hue of selection (toolbar)"), (_, _) => SetHueOfSelection(_drawHue)));
            edit.DropDownItems.Add(Item(L.T("Auswahl Z verschieben…", "Move selection Z…"), (_, _) => MoveSelectionZDialog()));
            edit.DropDownItems.Add(new ToolStripSeparator());
            edit.DropDownItems.Add(Item(L.T("Drehen 90° im Uhrzeigersinn", "Rotate 90° clockwise"), (_, _) => RotateMulti()));
            edit.DropDownItems.Add(Item(L.T("Spiegeln X", "Mirror X"), (_, _) => MirrorMulti(true)));
            edit.DropDownItems.Add(Item(L.T("Spiegeln Y", "Mirror Y"), (_, _) => MirrorMulti(false)));
            var swap = Item(L.T("Wand-Richtung beim Drehen tauschen", "Swap wall direction when rotating"), null);
            swap.Checked = _swapWallsOnRotate;
            swap.Click += (_, _) =>
            {
                _swapWallsOnRotate = !_swapWallsOnRotate;
                swap.Checked = _swapWallsOnRotate;
            };
            edit.DropDownItems.Add(swap);
            edit.DropDownItems.Add(Item(L.T("Zuschneiden auf benutzte Fläche", "Crop to used area"), (_, _) => CropMulti()));
            edit.DropDownItems.Add(Item(L.T("Ursprung auf Mitte setzen", "Set origin to center"), (_, _) => SetOriginCenter()));
            edit.DropDownItems.Add(Item(L.T("Alles leeren", "Clear all"), (_, _) => ClearAllTiles()));

            var view = new ToolStripMenuItem(L.T("&Ansicht", "&View"));
            view.DropDownItems.Add(Item(L.T("Hintergrund…", "Background…"), (_, _) => BackgroundDialog()));
            var bgQuick = new ToolStripMenuItem(L.T("Hintergrund-Muster", "Background pattern"));
            bgQuick.DropDownItems.Add(Item(L.T("Kein", "None"), (_, _) => SetBackgroundPattern(null)));
            foreach (BackgroundPattern p in BackgroundLayer.Patterns.Take(BackgroundLayer.Patterns.Length - 1))
            {
                BackgroundPattern pattern = p;
                bgQuick.DropDownItems.Add(Item(pattern.Name, (_, _) => SetBackgroundPattern(pattern)));
            }

            view.DropDownItems.Add(bgQuick);
            view.DropDownItems.Add(new ToolStripSeparator());
            var grid = Item(L.T("Raster", "Grid"), (_, _) => ToggleGrid(), Keys.G);
            var heights = Item(L.T("Höhen (Map)", "Heights (map)"), (_, _) => ToggleHeights(), Keys.H);
            var invisible = Item(L.T("Unsichtbare Kacheln markieren", "Mark invisible tiles"), (_, _) =>
            {
                Settings.ShowInvisible = !Settings.ShowInvisible;
                Settings.Save();
                pictureBoxMulti.Invalidate();
            });
            view.DropDownOpening += (_, _) =>
            {
                grid.Checked = Settings.ShowGrid;
                heights.Checked = Settings.ShowHeights;
                invisible.Checked = Settings.ShowInvisible;
            };
            view.DropDownItems.Add(grid);
            view.DropDownItems.Add(heights);
            view.DropDownItems.Add(invisible);
            var deco = Item(L.T("Einrichtung einfärben", "Tint furniture"), (_, _) =>
            {
                _showDecoTint = !_showDecoTint;
                pictureBoxMulti.Invalidate();
            });
            var ruler = Item(L.T("Höhenlineal", "Height ruler"), (_, _) =>
            {
                _showRuler = !_showRuler;
                pictureBoxMulti.Invalidate();
            }, Keys.L);
            view.DropDownOpening += (_, _) =>
            {
                deco.Checked = _showDecoTint;
                ruler.Checked = _showRuler;
            };
            view.DropDownItems.Add(deco);
            view.DropDownItems.Add(ruler);
            view.DropDownItems.Add(Item(L.T("Ansicht um 90° drehen", "Rotate view by 90°"), (_, _) => RotateView()));
            view.DropDownItems.Add(new ToolStripSeparator());
            view.DropDownItems.Add(Item(L.T("Vergrößern", "Zoom in"), (_, _) => SetZoom(_zoom * 1.25f), Keys.Control | Keys.Oemplus));
            view.DropDownItems.Add(Item(L.T("Verkleinern", "Zoom out"), (_, _) => SetZoom(_zoom / 1.25f), Keys.Control | Keys.OemMinus));
            view.DropDownItems.Add(Item("Zoom 100 %", (_, _) => SetZoom(1f), Keys.Control | Keys.D0));

            var map = new ToolStripMenuItem("&Map");
            map.DropDownItems.Add(Item(L.T("Map-Bereich laden…", "Load map area…"), (_, _) => LoadMapArea()));
            var snap = Item(L.T("Beim Zeichnen auf Boden setzen", "Snap to ground when drawing"), null);
            var statics = Item(L.T("Map-Statics zeigen", "Show map statics"), null);
            var land = Item(L.T("Map-Boden zeigen", "Show map land"), null);
            snap.Click += (_, _) => _snapToGround = !_snapToGround;
            statics.Click += (_, _) =>
            {
                _background.ShowStatics = !_background.ShowStatics;
                pictureBoxMulti.Invalidate();
            };
            land.Click += (_, _) =>
            {
                _background.ShowLand = !_background.ShowLand;
                pictureBoxMulti.Invalidate();
            };
            map.DropDownOpening += (_, _) =>
            {
                snap.Checked = _snapToGround;
                statics.Checked = _background.ShowStatics;
                land.Checked = _background.ShowLand;
            };
            map.DropDownItems.Add(snap);
            map.DropDownItems.Add(statics);
            map.DropDownItems.Add(land);
            map.DropDownItems.Add(Item(L.T("Statics-Transparenz…", "Statics transparency…"), (_, _) => StaticsAlphaDialog()));
            map.DropDownItems.Add(Item(L.T("Static im Bereich finden…", "Find static in area…"), (_, _) => FindStaticDialog()));
            map.DropDownItems.Add(Item(L.T("Statics des Bereichs als Kacheln übernehmen", "Import statics of area as tiles"), (_, _) => ImportMapStatics()));
            map.DropDownItems.Add(new ToolStripSeparator());
            map.DropDownItems.Add(Item(L.T("Map-Statics auswählen (M)", "Select map statics (M)"), (_, _) => SetExtTool(ExtTool.MapSelect)));
            map.DropDownItems.Add(Item(L.T("Map-Auswahl kopieren", "Copy map selection"), (_, _) => CopyMapSelection()));
            map.DropDownItems.Add(Item(L.T("Map-Auswahl ins Multi verschieben", "Move map selection into multi"), (_, _) => TakeMapSelection(true)));
            map.DropDownItems.Add(Item(L.T("Map-Auswahl löschen", "Delete map selection"), (_, _) => DeleteMapSelection()));
            map.DropDownItems.Add(new ToolStripSeparator());
            map.DropDownItems.Add(Item(L.T("Multi in Statics schreiben…", "Write multi to statics…"), (_, _) => WriteToStatics()));
            map.DropDownItems.Add(Item(L.T("Map-Änderung rückgängig", "Undo map change"), (_, _) => UndoMapChange()));
            map.DropDownItems.Add(Item(L.T("Map-Statics speichern…", "Save map statics…"), (_, _) => SaveMapStatics()));
            map.DropDownItems.Add(Item(L.T("Map-Hintergrund entfernen", "Remove map background"), (_, _) => SetBackgroundPattern(null)));

            var gen = new ToolStripMenuItem("&Generator");
            gen.DropDownItems.Add(Item(L.T("Hausbau-Generator…", "House generator…"), (_, _) => OpenGenerator()));
            gen.DropDownItems.Add(Item(L.T("Aus aktuellem Multi lernen", "Learn from current multi"), (_, _) => LearnFromCurrent()));

            var lang = new ToolStripMenuItem(L.T("&Sprache", "&Language"));
            lang.DropDownItems.Add(Item("Deutsch", (_, _) => SetLanguage(true)));
            lang.DropDownItems.Add(Item("English", (_, _) => SetLanguage(false)));

            var help = new ToolStripMenuItem(L.T("&Hilfe", "&Help"));
            help.DropDownItems.Add(Item(L.T("Hilfe", "Help"), (_, _) => HelpForm.ShowHelp(this), Keys.F1));
            help.DropDownItems.Add(Item(L.T("Tasten & Kontextmenü", "Keys & context menu"), (_, _) => HelpForm.ShowHelp(this, 2)));
            help.DropDownItems.Add(Item(L.T("Import-Formate", "Import formats"), (_, _) => HelpForm.ShowHelp(this, 4)));
            help.DropDownItems.Add(Item(L.T("Map / Statics", "Map / statics"), (_, _) => HelpForm.ShowHelp(this, 7)));
            help.DropDownItems.Add(Item(L.T("Generator", "Generator"), (_, _) => HelpForm.ShowHelp(this, 9)));
            help.DropDownItems.Add(Item(L.T("Struktur-Werkzeug", "Structure tool"), (_, _) => HelpForm.ShowHelp(this, 10)));
            help.DropDownItems.Add(Item(L.T("Bauteile", "Building parts"), (_, _) => HelpForm.ShowHelp(this, 11)));
            help.DropDownItems.Add(Item(L.T("Ebenen & Lineal", "Layers & ruler"), (_, _) => HelpForm.ShowHelp(this, 12)));
            help.DropDownItems.Add(Item(L.T("Einrichtung & Ansicht drehen", "Furniture & rotate view"), (_, _) => HelpForm.ShowHelp(this, 13)));
            help.DropDownItems.Add(Item(L.T("Map-Statics bearbeiten", "Edit map statics"), (_, _) => HelpForm.ShowHelp(this, 14)));
            help.DropDownItems.Add(Item(L.T("ServUO-Haus & Prüfung", "ServUO house & check"), (_, _) => HelpForm.ShowHelp(this, 15)));
            help.DropDownItems.Add(Item(L.T("Symmetrie, Farben, Listen", "Symmetry, hues, lists"), (_, _) => HelpForm.ShowHelp(this, 16)));

            _menu.Items.AddRange(new ToolStripItem[] { file, edit, view, map, gen, lang, help });
        }

        private void BuildToolBar()
        {
            _toolBar = new ToolStrip { Dock = DockStyle.Top, GripStyle = ToolStripGripStyle.Hidden };

            _tbRect = new ToolStripButton(L.T("Rechteck", "Rect")) { ToolTipText = L.T("Rechteck füllen (7) – Umschalt = nur Rand", "Fill rectangle (7) – Shift = border only") };
            _tbLine = new ToolStripButton(L.T("Linie", "Line")) { ToolTipText = L.T("Linie entlang X/Y (8)", "Line along X/Y (8)") };
            _tbBox = new ToolStripButton(L.T("Rahmen", "Box")) { ToolTipText = L.T("Rahmen-Auswahl (9) – Strg = hinzufügen", "Box select (9) – Ctrl = add") };
            _tbErase = new ToolStripButton(L.T("Bereich löschen", "Erase area")) { ToolTipText = L.T("Bereich löschen (0)", "Erase area (0)") };
            _tbRect.Click += (_, _) => SetTool(7);
            _tbLine.Click += (_, _) => SetTool(8);
            _tbBox.Click += (_, _) => SetTool(9);
            _tbErase.Click += (_, _) => SetTool(0);

            var undo = new ToolStripButton(L.T("↶ Rückgängig", "↶ Undo")) { ToolTipText = L.T("Rückgängig (Strg+Z)", "Undo (Ctrl+Z)") };
            undo.Click += (_, _) => DoUndo();
            var redo = new ToolStripButton(L.T("↷ Wiederholen", "↷ Redo")) { ToolTipText = L.T("Wiederholen (Strg+Y)", "Redo (Ctrl+Y)") };
            redo.Click += (_, _) => DoRedo();

            var rectMode = new ToolStripDropDownButton(_rectOutline ? "□" : "■") { ToolTipText = L.T("Rechteck: gefüllt oder nur Rand (Umschalt kehrt um). Rand mit Wandkacheln eines gelernten Stils setzt die richtigen Ecken.", "Rectangle: filled or border only (Shift inverts). Border with wall tiles of a learned style sets the right corners.") };
            var fill = new ToolStripMenuItem(L.T("■ Gefüllt", "■ Filled"));
            var outline = new ToolStripMenuItem(L.T("□ Nur Rand", "□ Border only"));
            var smartWalls = new ToolStripMenuItem(L.T("Rand mit richtigen Ecken (gelernter Stil)", "Border with proper corners (learned style)")) { CheckOnClick = true, Checked = _rectSmartWalls };
            fill.Click += (_, _) =>
            {
                _rectOutline = false;
                rectMode.Text = "■";
                SetTool(7);
            };
            outline.Click += (_, _) =>
            {
                _rectOutline = true;
                rectMode.Text = "□";
                SetTool(7);
            };
            smartWalls.CheckedChanged += (_, _) => _rectSmartWalls = smartWalls.Checked;
            rectMode.DropDownItems.AddRange(new ToolStripItem[] { fill, outline, new ToolStripSeparator(), smartWalls });

            var zoomOut = new ToolStripButton("−") { ToolTipText = L.T("Verkleinern (Strg+Rad)", "Zoom out (Ctrl+wheel)") };
            var zoomIn = new ToolStripButton("+") { ToolTipText = L.T("Vergrößern (Strg+Rad)", "Zoom in (Ctrl+wheel)") };
            zoomOut.Click += (_, _) => SetZoom(_zoom / 1.25f);
            zoomIn.Click += (_, _) => SetZoom(_zoom * 1.25f);
            _zoomLabel = new ToolStripLabel($"{_zoom * 100:0}%");
            _zoomLabel.Click += (_, _) => SetZoom(1f);

            // the hosts are disposed with the old toolbar, so the boxes are created new
            _hueBox = new NumericUpDown { Minimum = 0, Maximum = 3000, Width = 60, Value = Math.Max(0, Math.Min(3000, _drawHue)) };
            _hueBox.ValueChanged += HueBox_ValueChanged;
            _minZBox = new NumericUpDown { Minimum = -128, Maximum = 127, Width = 55, Value = Math.Max(-128, Math.Min(127, _minVisibleZ)) };
            _minZBox.ValueChanged += MinZBox_ValueChanged;

            _tbGrid = new ToolStripButton(L.T("Raster", "Grid")) { CheckOnClick = true, Checked = Settings.ShowGrid, ToolTipText = L.T("Raster ein/aus (G)", "Grid on/off (G)") };
            _tbGrid.Click += (_, _) =>
            {
                Settings.ShowGrid = _tbGrid.Checked;
                Settings.Save();
                pictureBoxMulti.Invalidate();
            };
            _tbHeights = new ToolStripButton(L.T("Höhen", "Heights")) { CheckOnClick = true, Checked = Settings.ShowHeights, ToolTipText = L.T("Boden-Höhen der Map (H)", "Ground heights of the map (H)") };
            _tbHeights.Click += (_, _) =>
            {
                Settings.ShowHeights = _tbHeights.Checked;
                Settings.Save();
                pictureBoxMulti.Invalidate();
            };

            var bg = new ToolStripDropDownButton(L.T("Hintergrund", "Background"));
            bg.DropDownItems.Add(Item(L.T("Kein", "None"), (_, _) => SetBackgroundPattern(null)));
            foreach (BackgroundPattern p in BackgroundLayer.Patterns.Take(BackgroundLayer.Patterns.Length - 1))
            {
                BackgroundPattern pattern = p;
                bg.DropDownItems.Add(Item(pattern.Name, (_, _) => SetBackgroundPattern(pattern)));
            }

            bg.DropDownItems.Add(new ToolStripSeparator());
            bg.DropDownItems.Add(Item(L.T("Bild…", "Image…"), (_, _) => LoadBackgroundImage()));
            bg.DropDownItems.Add(Item(L.T("Map-Bereich…", "Map area…"), (_, _) => LoadMapArea()));
            bg.DropDownItems.Add(Item(L.T("Einstellungen…", "Settings…"), (_, _) => BackgroundDialog()));

            var gen = new ToolStripButton(L.T("Generator", "Generator")) { ToolTipText = L.T("Hausbau-Generator", "House generator") };
            gen.Click += (_, _) => OpenGenerator();
            var help = new ToolStripButton("?") { ToolTipText = L.T("Hilfe (F1)", "Help (F1)") };
            help.Click += (_, _) => HelpForm.ShowHelp(this);

            _toolBar.Items.AddRange(new ToolStripItem[]
            {
                undo, redo, new ToolStripSeparator(),
                _tbRect, rectMode, _tbLine, _tbBox, _tbErase, new ToolStripSeparator(),
                zoomOut, _zoomLabel, zoomIn, new ToolStripSeparator(),
                new ToolStripLabel("Hue:"), new ToolStripControlHost(_hueBox) { ToolTipText = L.T("Hue beim Zeichnen (0 = keine)", "Hue for drawing (0 = none)") },
                new ToolStripLabel("Min Z:"), new ToolStripControlHost(_minZBox) { ToolTipText = L.T("Kacheln darunter ausblenden", "Hide tiles below") },
                new ToolStripSeparator(), _tbGrid, _tbHeights, bg, new ToolStripSeparator(), gen, help
            });
            UpdateToolButtons();
        }

        private bool _rectOutline;
        private bool _rectSmartWalls = true;

        private bool RectOutline => _rectOutline ^ ((ModifierKeys & Keys.Shift) != 0);

        /// <summary>
        /// Border rectangle with wall tiles of a learned style: walls with the learned corners and sides, null if not possible
        /// </summary>
        private List<ImportTile> SmartRectTiles()
        {
            if (_extTool != ExtTool.Rect || !RectOutline || !_rectSmartWalls || _drawTile.Id == 0xFFFF)
            {
                return null;
            }

            HouseStyle style = StyleKnowledge.Instance.Styles.FirstOrDefault(s => s.Template != null && s.Template.WallIds().Contains(_drawTile.Id));
            if (style == null || Math.Abs(_dragEndCell.X - _dragStartCell.X) < 1 || Math.Abs(_dragEndCell.Y - _dragStartCell.Y) < 1)
            {
                return null;
            }

            List<ImportTile> tiles = style.Template.BuildWallRectangle(_dragStartCell.X, _dragStartCell.Y, _dragEndCell.X, _dragEndCell.Y, _dragZ, _structWindows);
            foreach (ImportTile t in tiles)
            {
                t.Hue = _drawHue;
                t.Deco = _drawDeco;
            }

            return tiles.Where(t => InMulti(t.X, t.Y)).ToList();
        }

        private void HueBox_ValueChanged(object sender, EventArgs e)
        {
            _drawHue = (int)_hueBox.Value;
            pictureBoxMulti.Invalidate();
        }

        private void MinZBox_ValueChanged(object sender, EventArgs e)
        {
            _minVisibleZ = (int)_minZBox.Value;
            pictureBoxMulti.Invalidate();
        }

        private void BuildExtraTab()
        {
            if (_extraTab == null)
            {
                _extraTab = new TabPage { UseVisualStyleBackColor = true, AutoScroll = true };
                TC_MultiEditorToolbox.TabPages.Add(_extraTab);
            }

            _extraTab.Text = "Extra";
            _extraTab.Controls.Clear();

            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };

            void Header(string text) => flow.Controls.Add(new Label { Text = text, AutoSize = true, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(3, 8, 3, 2) });

            void Btn(string text, Action action)
            {
                var b = new Button { Text = text, Width = 200, Height = 26, TextAlign = ContentAlignment.MiddleLeft };
                b.Click += (_, _) => action();
                flow.Controls.Add(b);
            }

            Header(L.T("Datei", "File"));
            Btn(L.T("Öffnen (alle Formate)…", "Open (all formats)…"), () => OpenAnyFile(NumberMode.Auto));
            Btn(L.T("Projekt speichern (.mep)…", "Save project (.mep)…"), SaveProject);
            Btn(L.T("ServUO Addon (.cs)…", "ServUO addon (.cs)…"), ExportServUOAddon);
            Btn(L.T("Text hex / dezimal…", "Text hex / decimal…"), () => ExportGeneric(MessageBox.Show(L.T("Hex (Ja) oder dezimal (Nein)?", "Hex (yes) or decimal (no)?"), "Export", MessageBoxButtons.YesNo) == DialogResult.Yes ? "txthex" : "txtdec"));

            Header("Multi.mul");
            Btn(L.T("In Multi.mul schreiben…", "Write to Multi.mul…"), SaveToMultiMul);
            Btn(L.T("Nächste freie ID", "Next free id"), FindNextFreeId);
            Btn(L.T("Multi-ID löschen…", "Delete multi id…"), DeleteMultiId);

            Header(L.T("Map / Hintergrund", "Map / background"));
            Btn(L.T("Map-Bereich laden…", "Load map area…"), LoadMapArea);
            Btn(L.T("Hintergrund…", "Background…"), BackgroundDialog);
            Btn(L.T("In Statics schreiben…", "Write to statics…"), WriteToStatics);

            Header(L.T("Bauen", "Building"));
            Btn(L.T("Hausbau-Generator…", "House generator…"), OpenGenerator);
            Btn(L.T("Drehen 90°", "Rotate 90°"), RotateMulti);
            Btn(L.T("Zuschneiden", "Crop"), CropMulti);

            Header(L.T("Sprache / Hilfe", "Language / help"));
            Btn("Deutsch / English", () => SetLanguage(!L.German));
            Btn(L.T("Hilfe (F1)", "Help (F1)"), () => HelpForm.ShowHelp(this));

            _extraTab.Controls.Add(flow);
        }

        private Button _designUndo;
        private Button _designRedo;

        /// <summary>
        /// Undo / redo buttons on the design tab
        /// </summary>
        private void BuildDesignTabButtons()
        {
            if (_designUndo == null)
            {
                var group = new GroupBox { Location = new Point(7, 103), Size = new Size(210, 60), Text = L.T("Bearbeiten", "Edit") };
                _designUndo = new Button { Location = new Point(10, 22), Size = new Size(92, 27) };
                _designRedo = new Button { Location = new Point(108, 22), Size = new Size(92, 27) };
                _designUndo.Click += (_, _) => DoUndo();
                _designRedo.Click += (_, _) => DoRedo();
                group.Controls.Add(_designUndo);
                group.Controls.Add(_designRedo);
                designTab.Controls.Add(group);
            }

            _designUndo.Text = L.T("↶ Rückgängig", "↶ Undo");
            _designRedo.Text = L.T("↷ Wiederholen", "↷ Redo");
            ((GroupBox)_designUndo.Parent).Text = L.T("Bearbeiten", "Edit");
            toolTip1.SetToolTip(_designUndo, L.T("Rückgängig (Strg+Z)", "Undo (Ctrl+Z)"));
            toolTip1.SetToolTip(_designRedo, L.T("Wiederholen (Strg+Y)", "Redo (Ctrl+Y)"));
        }

        private void SetLanguage(bool german)
        {
            Settings.German = german;
            Settings.Save();
            BuildUi();
            RefreshExtendedImportNodes();
            RefreshSpecialTileNodes();
        }

        #endregion

        #region Tools / zoom / info

        private void SetTool(int n)
        {
            _pasteMode = false;
            _dragging = false;
            switch (n)
            {
                case 1:
                    _extTool = ExtTool.None;
                    BTN_Select_Click(null, EventArgs.Empty);
                    break;
                case 2:
                    _extTool = ExtTool.None;
                    BTN_Draw_Click(null, EventArgs.Empty);
                    break;
                case 3:
                    _extTool = ExtTool.None;
                    BTN_Remove_Click(null, EventArgs.Empty);
                    break;
                case 4:
                    _extTool = ExtTool.None;
                    BTN_Z_Click(null, EventArgs.Empty);
                    break;
                case 5:
                    _extTool = ExtTool.None;
                    BTN_Pipette_Click(null, EventArgs.Empty);
                    break;
                case 6:
                    _extTool = ExtTool.None;
                    BTN_Trans_Clicked(null, EventArgs.Empty);
                    break;
                default:
                    SetExtTool(n switch
                    {
                        7 => ExtTool.Rect,
                        8 => ExtTool.Line,
                        9 => ExtTool.BoxSelect,
                        _ => ExtTool.EraseArea
                    });
                    return;
            }

            UpdateToolButtons();
            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Called from the classic tool buttons
        /// </summary>
        private void ResetExtTool()
        {
            _extTool = ExtTool.None;
            _pasteMode = false;
            _dragging = false;
            UpdateToolButtons();
        }

        private void UpdateToolButtons()
        {
            if (_tbRect == null)
            {
                return;
            }

            _tbRect.Checked = _extTool == ExtTool.Rect;
            _tbLine.Checked = _extTool == ExtTool.Line;
            _tbBox.Checked = _extTool == ExtTool.BoxSelect;
            _tbErase.Checked = _extTool == ExtTool.EraseArea;
            UpdateStructureButtons();
        }

        private void SetZoom(float zoom)
        {
            zoom = Math.Max(0.2f, Math.Min(4f, zoom));
            if (Math.Abs(zoom - 1f) < 0.06f)
            {
                zoom = 1f;
            }

            _zoom = zoom;
            if (_zoomLabel != null)
            {
                _zoomLabel.Text = $"{_zoom * 100:0}%";
            }

            ScrollbarsSetValue();
            pictureBoxMulti.Invalidate();
        }

        private void ToggleGrid()
        {
            Settings.ShowGrid = !Settings.ShowGrid;
            Settings.Save();
            if (_tbGrid != null)
            {
                _tbGrid.Checked = Settings.ShowGrid;
            }

            pictureBoxMulti.Invalidate();
        }

        private void ToggleHeights()
        {
            Settings.ShowHeights = !Settings.ShowHeights;
            Settings.Save();
            if (_tbHeights != null)
            {
                _tbHeights.Checked = Settings.ShowHeights;
            }

            pictureBoxMulti.Invalidate();
        }

        private void UpdateInfo()
        {
            if (_infoLabel == null)
            {
                return;
            }

            if (_compList == null)
            {
                _infoLabel.Text = string.Empty;
                return;
            }

            string sel = _selection.Count > 0 ? L.T($" | Auswahl {_selection.Count}", $" | selected {_selection.Count}") : string.Empty;
            string mode = _pasteMode ? L.T(" | EINFÜGEN (Esc, R = drehen)", " | PASTE (Esc, R = rotate)") : string.Empty;
            if (_compList.ViewRotation > 0)
            {
                mode += $" | ↻ {_compList.ViewRotation * 90}°";
            }

            if (_compList.DecoCount > 0)
            {
                mode += L.T($" | Einrichtung {_compList.DecoCount}", $" | furniture {_compList.DecoCount}");
            }

            if (_mapEditor.ChangedMaps.Count > 0)
            {
                mode += L.T(" | Map geändert*", " | map changed*");
            }
            _infoLabel.Text = L.T(
                $"{_compList.RealTileCount} Kacheln | {_compList.Width}x{_compList.Height} | Ursprung {_compList.OriginX},{_compList.OriginY}{sel}{mode}",
                $"{_compList.RealTileCount} tiles | {_compList.Width}x{_compList.Height} | origin {_compList.OriginX},{_compList.OriginY}{sel}{mode}");
        }

        private ToolStripLabel _statusLabel;

        private void ShowStatus(string text)
        {
            if (_statusLabel == null)
            {
                _statusLabel = new ToolStripLabel { ForeColor = Color.DarkGreen };
                toolStrip1.Items.Add(new ToolStripSeparator());
                toolStrip1.Items.Add(_statusLabel);
            }

            _statusLabel.Text = text;
            toolStrip1.Update();
        }

        private void UpdateViewMargin()
        {
            if (_compList != null)
            {
                _compList.ViewMargin = _background.Mode is BackgroundMode.Pattern or BackgroundMode.Map ? _background.Margin * 44 : 0;
            }
        }

        #endregion

        #region Model refresh / selection

        /// <summary>
        /// Refresh controls after the model changed (undo, bulk operations, …)
        /// </summary>
        private void AfterModelChanged(bool keepSelection = false)
        {
            if (_compList == null)
            {
                return;
            }

            if (!keepSelection)
            {
                ClearSelectionInternal();
            }
            else
            {
                _selection.RemoveWhere(t => !_compList.Tiles.Contains(t));
            }

            int oldValue = MaxHeightTrackBar.Value;
            bool wasAtMax = oldValue >= MaxHeightTrackBar.Maximum;
            MaxHeightTrackBar.SetRange(Math.Min(_compList.ZMin, _compList.ZMax), Math.Max(_compList.ZMin, _compList.ZMax));
            MaxHeightTrackBar.Value = !keepSelection || wasAtMax
                ? MaxHeightTrackBar.Maximum
                : Math.Max(MaxHeightTrackBar.Minimum, Math.Min(MaxHeightTrackBar.Maximum, oldValue));

            numericUpDown_Size_Width.Value = Math.Max(numericUpDown_Size_Width.Minimum, Math.Min(numericUpDown_Size_Width.Maximum, _compList.Width));
            numericUpDown_Size_Height.Value = Math.Max(numericUpDown_Size_Height.Minimum, Math.Min(numericUpDown_Size_Height.Maximum, _compList.Height));
            numericUpDown_Selected_X.Maximum = Math.Max(0, _compList.Width - 1);
            numericUpDown_Selected_Y.Maximum = Math.Max(0, _compList.Height - 1);

            ScrollbarsSetValue();
            UpdateInfo();
            pictureBoxMulti.Invalidate();
        }

        private void ClearSelectionInternal()
        {
            _selection.Clear();
            SelectedTile = null;
            _overlayList.Clear();
        }

        private void ClearSelection()
        {
            if (_pasteMode || _dragging || _extTool != ExtTool.None)
            {
                _pasteMode = false;
                _dragging = false;
            }

            ClearSelectionInternal();
            UpdateInfo();
            pictureBoxMulti.Invalidate();
        }

        private void SelectAll()
        {
            if (_compList == null)
            {
                return;
            }

            _selection.Clear();
            foreach (MultiTile t in _compList.RealTiles)
            {
                _selection.Add(t);
            }

            UpdateInfo();
            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Selection, or the selected tile if nothing else is selected
        /// </summary>
        private List<MultiTile> GetWorkSet()
        {
            if (_selection.Count > 0)
            {
                return _selection.ToList();
            }

            return SelectedTile != null ? new List<MultiTile> { SelectedTile } : new List<MultiTile>();
        }

        private void SetDrawTile(int id)
        {
            if (id < 0 || id >= 0x10000)
            {
                return;
            }

            _drawTile.Set((ushort)id, 0);
            DrawTileLabel.Text = $"Draw ID: 0x{id:X}";
            PictureBoxDrawTiles_Select();
            pictureBoxDrawTiles.Invalidate();
        }

        private void RememberDrawnTile(int id)
        {
            if (Settings.RecentTiles.Count > 0 && Settings.RecentTiles[0] == id)
            {
                return;
            }

            Settings.AddRecentTile(id);
            Settings.Save();
            TreeNode recent = treeViewTilesXML.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Name == _specialPrefix + "recent");
            if (recent != null)
            {
                recent.Tag = Settings.RecentTiles.Where(Art.IsValidStatic).ToList();
                if (treeViewTilesXML.SelectedNode == recent)
                {
                    _drawTilesList = (List<int>)recent.Tag;
                    pictureBoxDrawTiles.Invalidate();
                }
            }
        }

        #endregion

        #region Mouse

        private Point ScreenToWorld(Point p)
        {
            return new Point((int)(p.X / _zoom) + hScrollBar.Value, (int)(p.Y / _zoom) + vScrollBar.Value);
        }

        /// <summary>
        /// Cell below a world pixel at a given Z
        /// </summary>
        private void CellAt(Point point, int z, out int x, out int y)
        {
            int cx = 0;
            int cy = 0;
            cy -= z * 4;
            cy -= 44;
            cx -= _compList.XMin;
            cy -= _compList.YMin;
            cy += MultiEditorComponentList.GapYMod;
            cx += MultiEditorComponentList.GapXMod;

            double mx = point.X - cx;
            double my = point.Y - cy;
            double xx = mx;
            double yy = my;
            my = (xx * _coordinateTransform) - (yy * _coordinateTransform);
            mx = (yy * _coordinateTransform) + (xx * _coordinateTransform);
            mx /= _coordinateTransform * 44;
            my /= _coordinateTransform * 44;
            my *= -1;
            x = (int)Math.Floor(mx);
            y = (int)Math.Floor(my);
        }

        private bool InMulti(int x, int y)
        {
            return _compList != null && x >= 0 && y >= 0 && x < _compList.Width && y < _compList.Height;
        }

        /// <returns>true if handled</returns>
        private bool HandleExtMouseDown(MouseEventArgs e)
        {
            _focusSink?.Focus();
            _mouseLoc = ScreenToWorld(e.Location);

            if (e.Button == MouseButtons.Right)
            {
                _rightDownPos = e.Location;
                _rightMoved = false;
                return false;
            }

            if (e.Button != MouseButtons.Left || _compList == null || _pasteMode)
            {
                return false;
            }

            if (_extTool == ExtTool.None)
            {
                return false;
            }

            ConvertCoords(_mouseLoc, out int x, out int y, out int z);
            if (_extTool is ExtTool.BoxSelect or ExtTool.EraseArea or ExtTool.Structure or ExtTool.MapSelect)
            {
                // use the floor level so the box is not bound to a tile
                z = BTN_Floor.Checked ? DrawFloorZ : 0;
                CellAt(_mouseLoc, z, out x, out y);
            }

            _dragging = true;
            _dragStartCell = new Point(x, y);
            _dragEndCell = _dragStartCell;
            _dragZ = z;
            return true;
        }

        private void HandleExtMouseMove(MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Right) != 0 && !_rightMoved)
            {
                if (Math.Abs(e.X - _rightDownPos.X) > 4 || Math.Abs(e.Y - _rightDownPos.Y) > 4)
                {
                    _rightMoved = true;
                }
            }

            if (_dragging && _compList != null)
            {
                CellAt(_mouseLoc, _dragZ, out int x, out int y);
                _dragEndCell = new Point(x, y);
            }
        }

        /// <returns>true if handled</returns>
        private bool HandleExtMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (!_rightMoved)
                {
                    if (_pasteMode)
                    {
                        _pasteMode = false;
                        UpdateInfo();
                        pictureBoxMulti.Invalidate();
                    }
                    else
                    {
                        ShowMultiContext(e.Location);
                    }
                }

                return false;
            }

            if (e.Button != MouseButtons.Left || _compList == null)
            {
                return false;
            }

            if (_pasteMode)
            {
                PasteAtMouse();
                return true;
            }

            if (_dragging)
            {
                _dragging = false;
                FinishDrag();
                pictureBoxMulti.Invalidate();
                return true;
            }

            // Ctrl + click with select tool = toggle multi selection
            if (BTN_Select.Checked && (ModifierKeys & Keys.Control) != 0)
            {
                if (_hoverTile != null)
                {
                    if (!_selection.Remove(_hoverTile))
                    {
                        _selection.Add(_hoverTile);
                    }

                    if (SelectedTile != null && !_selection.Contains(SelectedTile) && _selection.Count > 0)
                    {
                        _selection.Add(SelectedTile);
                    }
                }

                UpdateInfo();
                pictureBoxMulti.Invalidate();
                return true;
            }

            if (BTN_Select.Checked)
            {
                _selection.Clear();
            }

            return false;
        }

        private IEnumerable<Point> DragCells()
        {
            int x1 = Math.Min(_dragStartCell.X, _dragEndCell.X);
            int x2 = Math.Max(_dragStartCell.X, _dragEndCell.X);
            int y1 = Math.Min(_dragStartCell.Y, _dragEndCell.Y);
            int y2 = Math.Max(_dragStartCell.Y, _dragEndCell.Y);

            switch (_extTool)
            {
                case ExtTool.Line:
                    {
                        int dx = Math.Abs(_dragEndCell.X - _dragStartCell.X);
                        int dy = Math.Abs(_dragEndCell.Y - _dragStartCell.Y);
                        if (dx >= dy)
                        {
                            for (int x = x1; x <= x2; ++x)
                            {
                                yield return new Point(x, _dragStartCell.Y);
                            }
                        }
                        else
                        {
                            for (int y = y1; y <= y2; ++y)
                            {
                                yield return new Point(_dragStartCell.X, y);
                            }
                        }

                        break;
                    }
                case ExtTool.Rect when RectOutline:
                    for (int x = x1; x <= x2; ++x)
                    {
                        for (int y = y1; y <= y2; ++y)
                        {
                            if (x == x1 || x == x2 || y == y1 || y == y2)
                            {
                                yield return new Point(x, y);
                            }
                        }
                    }

                    break;
                default:
                    for (int x = x1; x <= x2; ++x)
                    {
                        for (int y = y1; y <= y2; ++y)
                        {
                            yield return new Point(x, y);
                        }
                    }

                    break;
            }
        }

        private void FinishDrag()
        {
            switch (_extTool)
            {
                case ExtTool.Rect:
                case ExtTool.Line:
                    {
                        if (_drawTile.Id == 0xFFFF)
                        {
                            return;
                        }

                        List<ImportTile> tiles = SmartRectTiles() ?? DragCells()
                            .Where(p => InMulti(p.X, p.Y))
                            .Select(p => new ImportTile { Id = _drawTile.Id, X = p.X, Y = p.Y, Z = _dragZ, Hue = _drawHue, Deco = _drawDeco })
                            .ToList();
                        if (_compList.TilesAdd(WithSymmetry(tiles), _extTool == ExtTool.Rect ? "Rectangle" : "Line") > 0)
                        {
                            RememberDrawnTile(_drawTile.Id);
                        }

                        AfterModelChanged(true);
                        break;
                    }
                case ExtTool.BoxSelect:
                    {
                        if ((ModifierKeys & Keys.Control) == 0)
                        {
                            _selection.Clear();
                        }

                        foreach (MultiTile t in TilesInDragRect())
                        {
                            _selection.Add(t);
                        }

                        UpdateInfo();
                        break;
                    }
                case ExtTool.EraseArea:
                    {
                        List<MultiTile> tiles = TilesInDragRect();
                        _compList.TilesRemove(tiles, "Erase area");
                        AfterModelChanged(true);
                        break;
                    }
                case ExtTool.Structure:
                    FinishStructure();
                    break;
                case ExtTool.MapSelect:
                    FinishMapSelect();
                    break;
            }
        }

        private List<MultiTile> TilesInDragRect()
        {
            int maxZ = MaxHeightTrackBar.Value;
            return _compList.GetTilesInRect(_dragStartCell.X, _dragStartCell.Y, _dragEndCell.X, _dragEndCell.Y)
                .Where(t => t.Z <= maxZ && t.Z >= _minVisibleZ && (!BTN_Floor.Checked || t.Z >= DrawFloorZ))
                .ToList();
        }

        private void PictureBoxMulti_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) != 0)
            {
                // zoom to the mouse position
                Point before = ScreenToWorld(e.Location);
                SetZoom(e.Delta > 0 ? _zoom * 1.25f : _zoom / 1.25f);
                Point after = ScreenToWorld(e.Location);
                ScrollBy(before.X - after.X, before.Y - after.Y);
                return;
            }

            if ((ModifierKeys & Keys.Shift) != 0)
            {
                ScrollBy(-Math.Sign(e.Delta) * 60, 0);
            }
            else
            {
                ScrollBy(0, -Math.Sign(e.Delta) * 60);
            }
        }

        private void ScrollBy(int dx, int dy)
        {
            if (hScrollBar.Enabled)
            {
                hScrollBar.Value = Math.Max(hScrollBar.Minimum, Math.Min(hScrollBar.Maximum, hScrollBar.Value + dx));
            }

            if (vScrollBar.Enabled)
            {
                vScrollBar.Value = Math.Max(vScrollBar.Minimum, Math.Min(vScrollBar.Maximum, vScrollBar.Value + dy));
            }

            pictureBoxMulti.Invalidate();
        }

        #endregion

        #region Painting (overlays)

        private void PaintBackground(Graphics g)
        {
            try
            {
                _background.Draw(g, _compList, hScrollBar.Value, vScrollBar.Value);
            }
            catch (Exception ex)
            {
                // a broken background must never stop the editor
                ShowStatus(L.T("Hintergrund-Fehler: ", "Background error: ") + ex.Message);
            }
        }

        private void PaintOverlays(Graphics g)
        {
            if (_compList == null)
            {
                return;
            }

            if (Settings.ShowGrid)
            {
                _background.DrawGrid(g, _compList, hScrollBar.Value, vScrollBar.Value, BTN_Floor.Checked ? DrawFloorZ : 0, !BTN_Floor.Checked && _snapToGround);
            }

            if (Settings.ShowHeights && _background.HasMap && _zoom >= 0.6f)
            {
                _background.DrawHeights(g, _compList, hScrollBar.Value, vScrollBar.Value, _heightFont);
            }

            PaintMapSelection(g);

            if (_dragging)
            {
                PaintDragPreview(g);
            }

            if (_pasteMode && _clipboard != null && pictureBoxMulti.ClientRectangle.Contains(pictureBoxMulti.PointToClient(MousePosition)))
            {
                PaintPastePreview(g);
            }

            PaintSymmetryAxes(g);
            PaintHouseMarkers(g);
            PaintWalkIssues(g);
            PaintRuler(g);
        }

        private void DrawCellOutline(Graphics g, Pen pen, int x, int y, int z)
        {
            Point p = BackgroundLayer.CellToScreen(_compList, x, y, z, hScrollBar.Value, vScrollBar.Value);
            g.DrawPolygon(pen, new[]
            {
                new Point(p.X + 22, p.Y),
                new Point(p.X + 44, p.Y + 22),
                new Point(p.X + 22, p.Y + 44),
                new Point(p.X, p.Y + 22)
            });
        }

        private void DrawGhost(Graphics g, ushort id, int hue, int x, int y, int z)
        {
            Bitmap bmp = Art.GetStatic(id);
            if (bmp == null)
            {
                return;
            }

            if (hue > 0)
            {
                bmp = MultiTile.GetHuedBitmap(id, hue, bmp);
            }

            int px = ((x - y) * 22) - (bmp.Width / 2) + MultiEditorComponentList.GapXMod - _compList.XMin - hScrollBar.Value;
            int py = ((x + y) * 22) - (z * 4) - bmp.Height + MultiEditorComponentList.GapYMod - _compList.YMin - vScrollBar.Value;
            g.DrawImage(bmp, new Rectangle(px, py, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, MultiTile.DrawColor);
        }

        private void PaintDragPreview(Graphics g)
        {
            if (_extTool == ExtTool.Structure)
            {
                PaintStructurePreview(g);
                return;
            }

            List<Point> cells = DragCells().ToList();
            Color color = _extTool switch
            {
                ExtTool.EraseArea => Color.Red,
                ExtTool.BoxSelect => Color.DeepSkyBlue,
                ExtTool.MapSelect => Color.Cyan,
                _ => Color.Yellow
            };

            using var pen = new Pen(color, 2);
            List<ImportTile> smart = SmartRectTiles();
            if (smart != null)
            {
                foreach (ImportTile t in smart.OrderBy(t => t.X + t.Y).ThenBy(t => t.Z).Take(1500))
                {
                    DrawGhost(g, t.Id, t.Hue, t.X, t.Y, t.Z);
                }

                cells.Clear();
            }

            bool ghosts = (_extTool is ExtTool.Rect or ExtTool.Line) && cells.Count <= 600 && _drawTile.Id != 0xFFFF;
            foreach (Point c in cells.OrderBy(c => c.X + c.Y))
            {
                if (ghosts && InMulti(c.X, c.Y))
                {
                    DrawGhost(g, _drawTile.Id, _drawHue, c.X, c.Y, _dragZ);
                }
                else
                {
                    DrawCellOutline(g, pen, c.X, c.Y, _dragZ);
                }
            }

            int w = Math.Abs(_dragEndCell.X - _dragStartCell.X) + 1;
            int h = Math.Abs(_dragEndCell.Y - _dragStartCell.Y) + 1;
            SetToolStripText($"{_dragStartCell.X},{_dragStartCell.Y} → {_dragEndCell.X},{_dragEndCell.Y}  ({w} x {h})");
        }

        private Point PasteAnchor(out int z)
        {
            z = BTN_Floor.Checked ? DrawFloorZ : 0;
            CellAt(_mouseLoc, z, out int x, out int y);
            if (!BTN_Floor.Checked && _snapToGround && _background.HasMap)
            {
                z = _background.GetRelativeGroundZ(x, y);
            }

            return new Point(x, y);
        }

        private void PaintPastePreview(Graphics g)
        {
            Point a = PasteAnchor(out _);
            int count = 0;
            foreach (ImportTile t in _clipboard.OrderBy(t => t.X + t.Y).ThenBy(t => t.Z))
            {
                if (++count > 800)
                {
                    break;
                }

                DrawGhost(g, t.Id, t.Hue, a.X + t.X, a.Y + t.Y, t.Z);
            }
        }

        #endregion

        #region Keyboard

        private static Control GetDeepFocus(Control c)
        {
            while (c is ContainerControl cc && cc.ActiveControl != null)
            {
                c = cc.ActiveControl;
            }

            return c;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Control focus = GetDeepFocus(this);
            bool textFocus = focus is TextBoxBase || focus is UpDownBase || focus is ComboBox || focus?.Parent is UpDownBase;
            bool listFocus = focus is TreeView || focus is ListBox || focus is DataGridView;
            Keys key = keyData & Keys.KeyCode;
            bool ctrl = (keyData & Keys.Control) != 0;
            bool shift = (keyData & Keys.Shift) != 0;

            if (key == Keys.F1)
            {
                HelpForm.ShowHelp(this);
                return true;
            }

            if (textFocus)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            if (ctrl)
            {
                switch (key)
                {
                    case Keys.Z when shift:
                    case Keys.Y:
                        DoRedo();
                        return true;
                    case Keys.Z:
                        DoUndo();
                        return true;
                    case Keys.C:
                        CopySelection(false);
                        return true;
                    case Keys.X:
                        CopySelection(true);
                        return true;
                    case Keys.V:
                        StartPaste();
                        return true;
                    case Keys.A:
                        SelectAll();
                        return true;
                    case Keys.S:
                        SaveProject();
                        return true;
                    case Keys.O:
                        OpenAnyFile(NumberMode.Auto);
                        return true;
                    case Keys.D0:
                    case Keys.NumPad0:
                        SetZoom(1f);
                        return true;
                    case Keys.Oemplus:
                    case Keys.Add:
                        SetZoom(_zoom * 1.25f);
                        return true;
                    case Keys.OemMinus:
                    case Keys.Subtract:
                        SetZoom(_zoom / 1.25f);
                        return true;
                }
            }

            if (listFocus || ctrl || (keyData & Keys.Alt) != 0)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            switch (key)
            {
                case Keys.Escape:
                    ClearSelection();
                    return true;
                case Keys.Delete:
                    DeleteSelection();
                    return true;
                case Keys.G:
                    ToggleGrid();
                    return true;
                case Keys.H:
                    ToggleHeights();
                    return true;
                case Keys.B:
                    SetExtTool(ExtTool.Structure);
                    return true;
                case Keys.M:
                    SetExtTool(ExtTool.MapSelect);
                    return true;
                case Keys.E:
                    _drawDeco = !_drawDeco;
                    UpdateStructureButtons();
                    ShowStatus(_drawDeco ? L.T("Einrichtung zeichnen: AN", "Draw furniture: ON") : L.T("Einrichtung zeichnen: AUS", "Draw furniture: OFF"));
                    return true;
                case Keys.R when _pasteMode:
                    RotateClipboard();
                    return true;
                case Keys.L:
                    _showRuler = !_showRuler;
                    pictureBoxMulti.Invalidate();
                    return true;
                case >= Keys.D0 and <= Keys.D9:
                    SetTool(key - Keys.D0);
                    return true;
                case >= Keys.NumPad0 and <= Keys.NumPad9:
                    SetTool(key - Keys.NumPad0);
                    return true;
                case Keys.Left:
                    return MoveSelection(-1, 0, 0);
                case Keys.Right:
                    return MoveSelection(1, 0, 0);
                case Keys.Up:
                    return MoveSelection(0, -1, 0);
                case Keys.Down:
                    return MoveSelection(0, 1, 0);
                case Keys.PageUp:
                    return MoveSelection(0, 0, shift ? 5 : 1);
                case Keys.PageDown:
                    return MoveSelection(0, 0, shift ? -5 : -1);
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool MoveSelection(int dx, int dy, int dz)
        {
            List<MultiTile> work = GetWorkSet();
            if (_compList == null || work.Count == 0)
            {
                return false;
            }

            if ((ModifierKeys & Keys.Control) != 0 && dz != 0)
            {
                dz *= 20;
            }

            _compList.TilesMove(work, dx, dy, dz);
            foreach (MultiTile t in work)
            {
                _selection.Add(t);
            }

            if (SelectedTile != null)
            {
                SelectedTile = SelectedTile; // refresh panel
            }

            AfterModelChanged(true);
            return true;
        }

        #endregion

        #region Undo / clipboard / edit

        private void DoUndo()
        {
            if (_compList == null || !_compList.Undo())
            {
                return;
            }

            AfterModelChanged();
        }

        private void DoRedo()
        {
            if (_compList == null || !_compList.Redo())
            {
                return;
            }

            AfterModelChanged();
        }

        private void CopySelection(bool cut)
        {
            List<MultiTile> work = GetWorkSet();
            if (_compList == null || work.Count == 0)
            {
                return;
            }

            int ax = work.Min(t => t.X);
            int ay = work.Min(t => t.Y);
            _clipboard = work.Select(t => new ImportTile { Id = t.Id, X = t.X - ax, Y = t.Y - ay, Z = t.Z, Flag = t.Flag, Hue = t.Hue }).ToList();

            // text to the windows clipboard too (hex list)
            try
            {
                var sb = new StringBuilder();
                foreach (ImportTile t in _clipboard)
                {
                    sb.AppendLine($"0x{t.Id:X4} {t.X} {t.Y} {t.Z} {t.Flag}" + (t.Hue > 0 ? $" {t.Hue}" : string.Empty));
                }

                Clipboard.SetText(sb.ToString());
            }
            catch
            {
                // clipboard busy
            }

            if (cut)
            {
                _compList.TilesRemove(work, "Cut");
                AfterModelChanged();
            }

            UpdateInfo();
        }

        private void StartPaste()
        {
            if (_compList == null)
            {
                return;
            }

            if (_clipboard == null || _clipboard.Count == 0)
            {
                // try text from the windows clipboard (id x y z list)
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        string tmp = Path.Combine(Path.GetTempPath(), "multieditor_clip.txt");
                        File.WriteAllText(tmp, Clipboard.GetText());
                        ImportResult r = MultiFormats.LoadTextList(tmp, NumberMode.Auto);
                        File.Delete(tmp);
                        if (r.Tiles.Count > 0)
                        {
                            int ax = r.Tiles.Min(t => t.X);
                            int ay = r.Tiles.Min(t => t.Y);
                            _clipboard = r.Tiles.Select(t =>
                            {
                                ImportTile c = t.Clone();
                                c.X -= ax;
                                c.Y -= ay;
                                return c;
                            }).ToList();
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            }

            if (_clipboard == null || _clipboard.Count == 0)
            {
                return;
            }

            _pasteMode = true;
            UpdateInfo();
            pictureBoxMulti.Invalidate();
        }

        private void PasteAtMouse()
        {
            if (_clipboard == null || _compList == null)
            {
                return;
            }

            Point a = PasteAnchor(out int groundZ);
            int zOffset = !BTN_Floor.Checked && _snapToGround && _background.HasMap ? groundZ - _clipboard.Min(t => t.Z) : 0;
            List<ImportTile> tiles = _clipboard.Select(t =>
            {
                ImportTile c = t.Clone();
                c.X += a.X;
                c.Y += a.Y;
                c.Z += zOffset;
                return c;
            }).ToList();

            tiles = WithSymmetry(tiles);
            int added = _compList.TilesAdd(tiles, "Paste");
            if (added < tiles.Count)
            {
                ShowStatus(L.T($"{tiles.Count - added} Kacheln außerhalb ausgelassen", $"{tiles.Count - added} tiles outside skipped"));
            }

            AfterModelChanged();
        }

        private void DeleteSelection()
        {
            List<MultiTile> work = GetWorkSet();
            if (_compList == null || work.Count == 0)
            {
                return;
            }

            _compList.TilesRemove(work, "Delete selection");
            AfterModelChanged();
        }

        private void SetHueOfSelection(int hue)
        {
            List<MultiTile> work = GetWorkSet();
            if (_compList == null || work.Count == 0)
            {
                return;
            }

            _compList.TilesSetHue(work, hue);
            AfterModelChanged(true);
        }

        private void MoveSelectionZDialog()
        {
            List<MultiTile> work = GetWorkSet();
            if (_compList == null || work.Count == 0)
            {
                return;
            }

            int? dz = SimpleDialog.AskNumber(this, L.T("Z verschieben", "Move Z"), L.T("Z +/-:", "Z +/-:"), 0, -255, 255);
            if (dz is null or 0)
            {
                return;
            }

            _compList.TilesMove(work, 0, 0, dz.Value);
            AfterModelChanged(true);
        }

        private void ReplaceIdsDialog()
        {
            if (_compList == null)
            {
                return;
            }

            var d = new SimpleDialog(L.T("IDs ersetzen", "Replace ids"))
                .AddText("from", L.T("Alte ID:", "Old id:"), SelectedTile != null ? $"0x{SelectedTile.Id:X4}" : string.Empty)
                .AddText("to", L.T("Neue ID:", "New id:"), _drawTile.Id != 0xFFFF ? $"0x{_drawTile.Id:X4}" : string.Empty)
                .AddCheck("sel", L.T("Nur in der Auswahl", "Only in selection"), _selection.Count > 0);
            if (!d.ShowOk(this) || !SimpleDialog.TryParseId(d.GetText("from"), out int from) || !SimpleDialog.TryParseId(d.GetText("to"), out int to))
            {
                return;
            }

            List<MultiTile> scope = d.GetCheck("sel") ? GetWorkSet().Where(t => t.Id == from).ToList() : null;
            int n = _compList.ReplaceId(scope, from, (ushort)to);
            AfterModelChanged(true);
            ShowStatus(L.T($"{n} Kacheln ersetzt", $"{n} tiles replaced"));
        }

        private Dictionary<ushort, ushort> SwapMap()
        {
            return _swapWallsOnRotate ? StyleKnowledge.Instance.GetRotationSwapMap() : null;
        }

        private void RotateMulti()
        {
            if (_compList == null)
            {
                return;
            }

            _compList.Rotate90(SwapMap());
            AfterModelChanged();
        }

        private void MirrorMulti(bool alongX)
        {
            if (_compList == null)
            {
                return;
            }

            _compList.Mirror(alongX, SwapMap());
            AfterModelChanged();
        }

        private void CropMulti()
        {
            if (_compList == null)
            {
                return;
            }

            _compList.Crop(0);
            AfterModelChanged();
        }

        private void SetOriginCenter()
        {
            if (_compList == null)
            {
                return;
            }

            _compList.AddToUndoList("Set origin");
            _compList.SetDefaultOrigin();
            AfterModelChanged(true);
        }

        private void ClearAllTiles()
        {
            if (_compList == null)
            {
                return;
            }

            if (MessageBox.Show(L.T("Alle Kacheln löschen?", "Delete all tiles?"), "Multi Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _compList.ClearAll();
            AfterModelChanged();
        }

        #endregion

        #region Context menu (multi view)

        private void ShowMultiContext(Point location)
        {
            if (_compList == null)
            {
                return;
            }

            _contextTile = _hoverTile;
            int z = BTN_Floor.Checked ? DrawFloorZ : 0;
            CellAt(_mouseLoc, z, out int x, out int y);
            if (_contextTile != null)
            {
                x = _contextTile.X;
                y = _contextTile.Y;
                z = _contextTile.Z;
            }
            else if (!BTN_Floor.Checked && _background.HasMap)
            {
                z = _background.GetRelativeGroundZ(x, y);
                CellAt(_mouseLoc, z, out x, out y);
            }

            _contextCell = new Point(x, y);
            _contextZ = z;
            _multiContext.Show(pictureBoxMulti, location);
        }

        private static string TileName(int id)
        {
            try
            {
                return TileData.ItemTable[id].Name;
            }
            catch
            {
                return string.Empty;
            }
        }

        private void MultiContext_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _multiContext.Items.Clear();
            MultiTile t = _contextTile;

            string header = t != null
                ? $"0x{t.Id:X4} {TileName(t.Id)}  ({t.X},{t.Y},{t.Z})" + (t.Hue > 0 ? $" Hue {t.Hue}" : string.Empty)
                : L.T($"Zelle {_contextCell.X},{_contextCell.Y}  Z {_contextZ}", $"Cell {_contextCell.X},{_contextCell.Y}  Z {_contextZ}");
            _multiContext.Items.Add(new ToolStripLabel(header) { Font = new Font(_multiContext.Font, FontStyle.Bold) });
            _multiContext.Items.Add(new ToolStripSeparator());

            var tools = new ToolStripMenuItem(L.T("Werkzeug", "Tool"));
            string[] names =
            {
                L.T("Auswählen", "Select"), L.T("Zeichnen", "Draw"), L.T("Entfernen", "Remove"), L.T("Höhe", "Altitude"),
                L.T("Pipette", "Pipette"), L.T("Transparent", "Transparent"), L.T("Rechteck", "Rectangle"), L.T("Linie", "Line"),
                L.T("Rahmen-Auswahl", "Box select"), L.T("Bereich löschen", "Erase area")
            };
            for (int i = 0; i < names.Length; ++i)
            {
                int tool = i == 9 ? 0 : i + 1;
                tools.DropDownItems.Add(Item($"{names[i]} ({tool})", (_, _) => SetTool(tool)));
            }

            _multiContext.Items.Add(tools);

            if (t != null)
            {
                _multiContext.Items.Add(Item(L.T("Als Zeichen-Kachel übernehmen", "Use as draw tile"), (_, _) => SetDrawTile(t.Id)));
                _multiContext.Items.Add(Item(L.T("Auswählen", "Select"), (_, _) =>
                {
                    SelectedTile = t;
                    pictureBoxMulti.Invalidate();
                }));
                _multiContext.Items.Add(Item(L.T("Kachel entfernen", "Remove tile"), (_, _) =>
                {
                    _compList.TileRemove(t);
                    AfterModelChanged(true);
                }));
                _multiContext.Items.Add(new ToolStripSeparator());

                var zMenu = new ToolStripMenuItem(L.T("Höhe (Z)", "Height (Z)"));
                foreach (int dz in new[] { 1, 5, 20, -1, -5, -20 })
                {
                    int d = dz;
                    zMenu.DropDownItems.Add(Item((d > 0 ? "+" : string.Empty) + d, (_, _) =>
                    {
                        List<MultiTile> set = IsInSelection(t) ? _selection.ToList() : new List<MultiTile> { t };
                        _compList.TilesMove(set, 0, 0, d);
                        AfterModelChanged(true);
                    }));
                }

                zMenu.DropDownItems.Add(Item(L.T("Wert setzen…", "Set value…"), (_, _) =>
                {
                    int? z = SimpleDialog.AskNumber(this, "Z", "Z:", t.Z, -128, 127);
                    if (z != null)
                    {
                        _compList.TileZSet(t, z.Value);
                        AfterModelChanged(true);
                    }
                }));
                _multiContext.Items.Add(zMenu);

                _multiContext.Items.Add(Item(L.T("Hue setzen…", "Set hue…"), (_, _) =>
                {
                    int? hue = PickHue(t.Hue > 0 ? t.Hue : _drawHue, t.Id);
                    if (hue != null)
                    {
                        List<MultiTile> set = IsInSelection(t) ? _selection.ToList() : new List<MultiTile> { t };
                        _compList.TilesSetHue(set, hue.Value);
                        AfterModelChanged(true);
                    }
                }));
                var inv = Item(L.T("Unsichtbar (Flag 0)", "Invisible (flag 0)"), (_, _) =>
                {
                    List<MultiTile> set = IsInSelection(t) ? _selection.ToList() : new List<MultiTile> { t };
                    _compList.TilesSetInvisible(set, !t.Invisible);
                    if (SelectedTile == t)
                    {
                        SelectedTile = t;
                    }

                    pictureBoxMulti.Invalidate();
                });
                inv.Checked = t.Invisible;
                _multiContext.Items.Add(inv);
                _multiContext.Items.Add(new ToolStripSeparator());

                _multiContext.Items.Add(Item(L.T("Alle mit dieser ID auswählen", "Select all with this id"), (_, _) =>
                {
                    _selection.Clear();
                    foreach (MultiTile o in _compList.RealTiles.Where(o => o.Id == t.Id))
                    {
                        _selection.Add(o);
                    }

                    UpdateInfo();
                    pictureBoxMulti.Invalidate();
                }));
                if (_drawTile.Id != 0xFFFF)
                {
                    _multiContext.Items.Add(Item(L.T($"Alle 0x{t.Id:X} durch Zeichen-Kachel 0x{_drawTile.Id:X} ersetzen", $"Replace all 0x{t.Id:X} with draw tile 0x{_drawTile.Id:X}"), (_, _) =>
                    {
                        _compList.ReplaceId(null, t.Id, _drawTile.Id);
                        AfterModelChanged(true);
                    }));
                }

                _multiContext.Items.Add(Item(L.T("TileData-Info…", "Tiledata info…"), (_, _) => ShowTileInfo(t.Id)));
            }

            _multiContext.Items.Add(Item(L.T("Alle Kacheln dieser Zelle entfernen", "Remove all tiles of this cell"), (_, _) =>
            {
                _compList.TilesRemove(_compList.GetMultiTileLitAtCoordinate(_contextCell.X, _contextCell.Y), "Clear cell");
                AfterModelChanged(true);
            }));

            _multiContext.Items.Add(new ToolStripSeparator());
            List<MultiTile> work = GetWorkSet();
            AddEnabled(_multiContext, Item(L.T("Kopieren", "Copy") + "\tCtrl+C", (_, _) => CopySelection(false)), work.Count > 0);
            AddEnabled(_multiContext, Item(L.T("Ausschneiden", "Cut") + "\tCtrl+X", (_, _) => CopySelection(true)), work.Count > 0);
            AddEnabled(_multiContext, Item(L.T("Einfügen", "Paste") + "\tCtrl+V", (_, _) => StartPaste()), _clipboard?.Count > 0 || Clipboard.ContainsText());
            AddEnabled(_multiContext, Item(L.T("Auswahl löschen", "Delete selection") + "\tDel", (_, _) => DeleteSelection()), work.Count > 0);
            _multiContext.Items.Add(Item(L.T("Alles auswählen", "Select all") + "\tCtrl+A", (_, _) => SelectAll()));
            if (_selection.Count > 0)
            {
                _multiContext.Items.Add(Item(L.T($"Hue {_drawHue} auf Auswahl", $"Hue {_drawHue} to selection"), (_, _) => SetHueOfSelection(_drawHue)));
                _multiContext.Items.Add(Item(L.T("Auswahl aufheben", "Clear selection") + "\tEsc", (_, _) => ClearSelection()));
            }

            _multiContext.Items.Add(new ToolStripSeparator());
            if (InMulti(_contextCell.X, _contextCell.Y))
            {
                _multiContext.Items.Add(Item(L.T("Ursprung (Mitte) hier setzen", "Set origin (center) here"), (_, _) =>
                {
                    _compList.AddToUndoList("Set origin");
                    _compList.OriginX = _contextCell.X;
                    _compList.OriginY = _contextCell.Y;
                    AfterModelChanged(true);
                }));
                if (_drawTile.Id != 0xFFFF)
                {
                    _multiContext.Items.Add(Item(L.T($"Zeichen-Kachel hier setzen (Z {_contextZ})", $"Place draw tile here (Z {_contextZ})"), (_, _) =>
                    {
                        _compList.TileAdd(_contextCell.X, _contextCell.Y, _contextZ, _drawTile.Id, _drawHue);
                        RememberDrawnTile(_drawTile.Id);
                        AfterModelChanged(true);
                    }));
                }
            }

            if (_background.HasMap)
            {
                HuedTile? st = _background.GetTopStatic(_contextCell.X, _contextCell.Y);
                if (st != null)
                {
                    HuedTile s = st.Value;
                    _multiContext.Items.Add(Item(L.T($"Hintergrund-Static 0x{s.Id:X4} als Zeichen-Kachel", $"Background static 0x{s.Id:X4} as draw tile"), (_, _) =>
                    {
                        SetDrawTile(s.Id);
                        if (s.Hue > 0)
                        {
                            _hueBox.Value = Math.Min(_hueBox.Maximum, s.Hue);
                        }
                    }));
                }

                if (_background.TryGetLand(_contextCell.X, _contextCell.Y, out int landId, out int landZ))
                {
                    _multiContext.Items.Add(new ToolStripLabel($"Land 0x{landId:X4}  Z {landZ}  ({_background.MapX + _contextCell.X},{_background.MapY + _contextCell.Y})") { ForeColor = SystemColors.GrayText });
                }
            }

            _multiContext.Items.Add(new ToolStripSeparator());
            AddStructureContextItems(t);
            AddMarkerContextItems();

            _multiContext.Items.Add(new ToolStripSeparator());
            AddEnabled(_multiContext, Item(L.T("Rückgängig", "Undo") + "\tCtrl+Z", (_, _) => DoUndo()), _compList.CanUndo);
            AddEnabled(_multiContext, Item(L.T("Wiederholen", "Redo") + "\tCtrl+Y", (_, _) => DoRedo()), _compList.CanRedo);
            var view = new ToolStripMenuItem(L.T("Ansicht", "View"));
            view.DropDownItems.Add(Item(L.T("Raster", "Grid") + " (G)", (_, _) => ToggleGrid()));
            view.DropDownItems.Add(Item(L.T("Höhen", "Heights") + " (H)", (_, _) => ToggleHeights()));
            view.DropDownItems.Add(Item("Zoom 100 %", (_, _) => SetZoom(1f)));
            view.DropDownItems.Add(Item(L.T("Hintergrund…", "Background…"), (_, _) => BackgroundDialog()));
            _multiContext.Items.Add(view);

            e.Cancel = false;
        }

        private static void AddEnabled(ContextMenuStrip menu, ToolStripItem item, bool enabled)
        {
            item.Enabled = enabled;
            menu.Items.Add(item);
        }

        private void ShowTileInfo(int id)
        {
            try
            {
                ItemData d = TileData.ItemTable[id];
                string text = $"ID: 0x{id:X4} ({id})\nName: {d.Name}\nHeight: {d.Height}\nWeight: {d.Weight}\nAnimation: {d.Animation}\nHue: {d.Hue}\n" +
                              $"Flags: {d.Flags}\n\n" + L.T("Rolle (Generator): ", "Role (generator): ") + StyleKnowledge.Instance.GetRole(id);
                MessageBox.Show(this, text, "TileData", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        #endregion

        #region Tile palette / groups

        private void PaletteContext_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
            _paletteContext.Items.Clear();
            int idx = _paletteContextIndex;
            if (idx < 0)
            {
                _paletteContext.Items.Add(Item(L.T("Kacheln suchen…", "Search tiles…"), (_, _) => SearchTiles()));
                return;
            }

            _paletteContext.Items.Add(new ToolStripLabel($"0x{idx:X4} {TileName(idx)}") { Font = new Font(_paletteContext.Font, FontStyle.Bold) });
            _paletteContext.Items.Add(Item(L.T("Als Zeichen-Kachel", "Use as draw tile"), (_, _) => SetDrawTile(idx)));

            var add = new ToolStripMenuItem(L.T("Zu Untergruppe hinzufügen", "Add to subgroup"));
            foreach (TreeNode g in treeViewTilesXML.Nodes)
            {
                if (g.Name.StartsWith(_specialPrefix))
                {
                    continue;
                }

                var gi = new ToolStripMenuItem(g.Text);
                foreach (TreeNode sg in g.Nodes)
                {
                    TreeNode target = sg;
                    gi.DropDownItems.Add(Item(sg.Text, (_, _) => AddTilesToSubgroup(target, new[] { idx })));
                }

                if (gi.DropDownItems.Count > 0)
                {
                    add.DropDownItems.Add(gi);
                }
            }

            _paletteContext.Items.Add(add);

            TreeNode sel = treeViewTilesXML.SelectedNode;
            if (sel != null && sel.Level == 1 && sel.Tag is List<int>)
            {
                _paletteContext.Items.Add(Item(L.T($"Aus „{sel.Text}“ entfernen", $"Remove from \"{sel.Text}\""), (_, _) =>
                {
                    ((List<int>)sel.Tag).Remove(idx);
                    _drawTilesList = (List<int>)sel.Tag;
                    pictureBoxDrawTiles.Invalidate();
                }));
            }

            _paletteContext.Items.Add(Item(L.T("TileData-Info…", "Tiledata info…"), (_, _) => ShowTileInfo(idx)));
            _paletteContext.Items.Add(Item(L.T("Kacheln suchen…", "Search tiles…"), (_, _) => SearchTiles()));
        }

        private void TreeContext_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
            _treeContext.Items.Clear();
            TreeNode node = treeViewTilesXML.SelectedNode;
            bool special = node != null && (node.Name.StartsWith(_specialPrefix) || node.Parent?.Name.StartsWith(_specialPrefix) == true);
            TreeNode group = node == null ? null : node.Level == 0 ? node : node.Parent;
            TreeNode sub = node != null && node.Level == 1 ? node : null;

            _treeContext.Items.Add(Item(L.T("Neue Gruppe…", "New group…"), (_, _) => NewTileGroup()));
            if (group != null && !special)
            {
                _treeContext.Items.Add(Item(L.T("Neue Untergruppe…", "New subgroup…"), (_, _) => NewTileSubgroup(group)));
            }

            if (node != null && !special)
            {
                _treeContext.Items.Add(Item(L.T("Umbenennen…", "Rename…"), (_, _) =>
                {
                    string name = SimpleDialog.AskText(this, L.T("Umbenennen", "Rename"), "Name:", node.Text);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        node.Text = name;
                    }
                }));
                _treeContext.Items.Add(Item(L.T("Löschen", "Delete"), (_, _) =>
                {
                    if (MessageBox.Show(this, L.T($"„{node.Text}“ löschen?", $"Delete \"{node.Text}\"?"), "Tiles", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        node.Remove();
                    }
                }));
            }

            if (sub != null && !special)
            {
                _treeContext.Items.Add(new ToolStripSeparator());
                if (_drawTile.Id != 0xFFFF)
                {
                    _treeContext.Items.Add(Item(L.T($"Zeichen-Kachel 0x{_drawTile.Id:X} hinzufügen", $"Add draw tile 0x{_drawTile.Id:X}"), (_, _) => AddTilesToSubgroup(sub, new[] { (int)_drawTile.Id })));
                }

                _treeContext.Items.Add(Item(L.T("ID-Bereich hinzufügen…", "Add id range…"), (_, _) => AddIdRange(sub)));
                _treeContext.Items.Add(Item(L.T("Kacheln des aktuellen Multis hinzufügen", "Add tiles of the current multi"), (_, _) =>
                {
                    if (_compList != null)
                    {
                        AddTilesToSubgroup(sub, _compList.RealTiles.Select(t => (int)t.Id).Distinct().OrderBy(i => i));
                    }
                }));
                _treeContext.Items.Add(Item(L.T("Sortieren", "Sort"), (_, _) =>
                {
                    ((List<int>)sub.Tag).Sort();
                    _drawTilesList = (List<int>)sub.Tag;
                    pictureBoxDrawTiles.Invalidate();
                }));
            }

            if (group != null && !special && _compList != null)
            {
                _treeContext.Items.Add(Item(L.T("Neue Untergruppe aus aktuellem Multi", "New subgroup from current multi"), (_, _) =>
                {
                    TreeNode n = NewTileSubgroup(group, textBox_Export.Text);
                    if (n != null)
                    {
                        AddTilesToSubgroup(n, _compList.RealTiles.Select(t => (int)t.Id).Distinct().OrderBy(i => i));
                    }
                }));
            }

            _treeContext.Items.Add(new ToolStripSeparator());
            _treeContext.Items.Add(Item(L.T("Kacheln suchen…", "Search tiles…"), (_, _) => SearchTiles()));
            _treeContext.Items.Add(Item(L.T("Gruppen speichern (multieditor.xml)", "Save groups (multieditor.xml)"), (_, _) => SaveTileGroups()));
            _treeContext.Items.Add(Item(L.T("Neu laden", "Reload"), (_, _) =>
            {
                treeViewTilesXML.Nodes.Clear();
                XML_InitializeToolBox();
                RefreshSpecialTileNodes();
            }));
        }

        private void NewTileGroup()
        {
            string name = SimpleDialog.AskText(this, L.T("Neue Gruppe", "New group"), "Name:", L.T("Meine Kacheln", "My tiles"));
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            var node = new TreeNode(name) { ImageIndex = 0 };
            int insertAt = treeViewTilesXML.Nodes.Cast<TreeNode>().Count(n => n.Name.StartsWith(_specialPrefix));
            treeViewTilesXML.Nodes.Insert(Math.Min(insertAt, treeViewTilesXML.Nodes.Count), node);
            NewTileSubgroup(node, L.T("Allgemein", "General"));
        }

        private TreeNode NewTileSubgroup(TreeNode group, string defaultName = null)
        {
            string name = defaultName ?? SimpleDialog.AskText(this, L.T("Neue Untergruppe", "New subgroup"), "Name:", L.T("Neu", "New"));
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var node = new TreeNode(name) { ImageIndex = 0, Tag = new List<int>() };
            group.Nodes.Add(node);
            group.Expand();
            treeViewTilesXML.SelectedNode = node;
            return node;
        }

        private void AddTilesToSubgroup(TreeNode sub, IEnumerable<int> ids)
        {
            if (sub.Tag is not List<int> list)
            {
                list = new List<int>();
                sub.Tag = list;
            }

            int added = 0;
            foreach (int id in ids)
            {
                if (!list.Contains(id) && Art.IsValidStatic(id))
                {
                    list.Add(id);
                    ++added;
                }
            }

            if (treeViewTilesXML.SelectedNode == sub)
            {
                _drawTilesList = list;
                PictureBoxDrawTiles_OnResize(this, EventArgs.Empty);
            }

            ShowStatus(L.T($"{added} Kacheln zu „{sub.Text}“ hinzugefügt (Speichern nicht vergessen)", $"{added} tiles added to \"{sub.Text}\" (don't forget to save)"));
        }

        private void AddIdRange(TreeNode sub)
        {
            var d = new SimpleDialog(L.T("ID-Bereich", "Id range"))
                .AddText("from", L.T("Von:", "From:"), _drawTile.Id != 0xFFFF ? $"0x{_drawTile.Id:X4}" : "0x0001")
                .AddText("to", L.T("Bis:", "To:"), _drawTile.Id != 0xFFFF ? $"0x{_drawTile.Id + 8:X4}" : "0x0010");
            if (!d.ShowOk(this) || !SimpleDialog.TryParseId(d.GetText("from"), out int from) || !SimpleDialog.TryParseId(d.GetText("to"), out int to))
            {
                return;
            }

            if (to < from)
            {
                (from, to) = (to, from);
            }

            to = Math.Min(to, from + 4096);
            AddTilesToSubgroup(sub, Enumerable.Range(from, to - from + 1));
        }

        private void SearchTiles()
        {
            string term = SimpleDialog.AskText(this, L.T("Kacheln suchen", "Search tiles"), L.T("Name, ID oder Bereich (0x100-0x120):", "Name, id or range (0x100-0x120):"), string.Empty);
            if (string.IsNullOrWhiteSpace(term))
            {
                return;
            }

            var result = new List<int>();
            string[] range = term.Split('-');
            if (range.Length == 2 && SimpleDialog.TryParseId(range[0], out int a) && SimpleDialog.TryParseId(range[1], out int b))
            {
                for (int i = Math.Min(a, b); i <= Math.Max(a, b) && result.Count < 5000; ++i)
                {
                    if (Art.IsValidStatic(i))
                    {
                        result.Add(i);
                    }
                }
            }
            else if ((term.StartsWith("0x", StringComparison.OrdinalIgnoreCase) || term.All(char.IsDigit)) && SimpleDialog.TryParseId(term, out int id))
            {
                if (Art.IsValidStatic(id))
                {
                    result.Add(id);
                }
            }
            else
            {
                int max = Math.Min(TileData.ItemTable.Length, Art.GetMaxItemId() + 1);
                for (int i = 0; i < max && result.Count < 5000; ++i)
                {
                    if (TileData.ItemTable[i].Name?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 && Art.IsValidStatic(i))
                    {
                        result.Add(i);
                    }
                }
            }

            TreeNode node = treeViewTilesXML.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Name == _specialPrefix + "search");
            if (node == null)
            {
                node = new TreeNode { Name = _specialPrefix + "search", ImageIndex = 0 };
                treeViewTilesXML.Nodes.Insert(0, node);
            }

            node.Text = L.T($"Suche: {term} ({result.Count})", $"Search: {term} ({result.Count})");
            node.Tag = result;
            treeViewTilesXML.SelectedNode = null;
            treeViewTilesXML.SelectedNode = node;
        }

        private void TreeViewTilesXML_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node?.Name == _specialPrefix + "used")
            {
                e.Node.Tag = _compList == null
                    ? new List<int>()
                    : _compList.RealTiles.Select(t => (int)t.Id).Distinct().OrderBy(i => i).ToList();
            }
        }

        /// <summary>
        /// "Recently used" and "Used in multi" nodes at the top of the tile tree
        /// </summary>
        private void RefreshSpecialTileNodes()
        {
            TreeNode recent = treeViewTilesXML.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Name == _specialPrefix + "recent");
            if (recent == null)
            {
                recent = new TreeNode { Name = _specialPrefix + "recent", ImageIndex = 0 };
                treeViewTilesXML.Nodes.Insert(0, recent);
            }

            recent.Text = L.T("★ Zuletzt benutzt", "★ Recently used");
            recent.Tag = Settings.RecentTiles.Where(Art.IsValidStatic).ToList();

            TreeNode used = treeViewTilesXML.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Name == _specialPrefix + "used");
            if (used == null)
            {
                used = new TreeNode { Name = _specialPrefix + "used", ImageIndex = 0, Tag = new List<int>() };
                treeViewTilesXML.Nodes.Insert(1, used);
            }

            used.Text = L.T("◆ Im Multi benutzt", "◆ Used in multi");
        }

        private void SaveTileGroups()
        {
            string fileName = Path.Combine(Options.AppDataPath, "plugins/multieditor.xml");
            try
            {
                if (File.Exists(fileName))
                {
                    File.Copy(fileName, fileName + ".bak", true);
                }

                var settings = new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(true) };
                using (XmlWriter w = XmlWriter.Create(fileName, settings))
                {
                    w.WriteStartDocument();
                    w.WriteStartElement("TileGroups");
                    int gid = 0;
                    foreach (TreeNode g in treeViewTilesXML.Nodes)
                    {
                        if (g.Name.StartsWith(_specialPrefix))
                        {
                            continue;
                        }

                        w.WriteStartElement("group");
                        w.WriteAttributeString("id", (gid++).ToString());
                        w.WriteAttributeString("name", g.Text);
                        foreach (TreeNode s in g.Nodes)
                        {
                            w.WriteStartElement("subgroup");
                            w.WriteAttributeString("name", s.Text);
                            if (s.Tag is List<int> list)
                            {
                                foreach (int id in list)
                                {
                                    w.WriteStartElement("entry");
                                    w.WriteAttributeString("index", id.ToString());
                                    w.WriteEndElement();
                                }
                            }

                            w.WriteEndElement();
                        }

                        w.WriteEndElement();
                    }

                    w.WriteEndElement();
                    w.WriteEndDocument();
                }

                MessageBox.Show(this, L.T($"Gespeichert: {fileName}", $"Saved: {fileName}"), "Tiles", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Tiles", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Import

        /// <summary>
        /// Adds the new entries to the import tree (called from OnLoad)
        /// </summary>
        private void RefreshExtendedImportNodes()
        {
            if (treeViewMultiList.Nodes.Count < 2)
            {
                return;
            }

            TreeNode fileNode = treeViewMultiList.Nodes[1];
            for (int i = fileNode.Nodes.Count - 1; i >= 0; --i)
            {
                if (fileNode.Nodes[i].Tag is string s && s.StartsWith("ext:"))
                {
                    fileNode.Nodes.RemoveAt(i);
                }
            }

            fileNode.Nodes.Add(new TreeNode(L.T("Alle Formate (automatisch)", "All formats (auto detect)")) { Tag = "ext:auto" });
            fileNode.Nodes.Add(new TreeNode(L.T("Text dezimal", "Text decimal")) { Tag = "ext:dec" });
            fileNode.Nodes.Add(new TreeNode(L.T("Text hex (0x…)", "Text hex (0x…)")) { Tag = "ext:hex" });
            fileNode.Nodes.Add(new TreeNode(L.T("ServUO / RunUO Addon (.cs)", "ServUO / RunUO addon (.cs)")) { Tag = "ext:servuo" });
            fileNode.Nodes.Add(new TreeNode(L.T("MultiEditor-Projekt (.mep)", "MultiEditor project (.mep)")) { Tag = "ext:mep" });
            fileNode.Text = L.T("Aus Datei", "From File");
            fileNode.Expand();

            RefreshRecentNode();
        }

        private void RefreshRecentNode()
        {
            TreeNode recent = treeViewMultiList.Nodes.Cast<TreeNode>().FirstOrDefault(n => (n.Tag as string) == "ext:recentroot");
            if (recent == null)
            {
                recent = new TreeNode { Tag = "ext:recentroot" };
                treeViewMultiList.Nodes.Add(recent);
            }

            recent.Text = L.T("Zuletzt geöffnet", "Recent files");
            recent.Nodes.Clear();
            foreach (string f in Settings.RecentFiles)
            {
                recent.Nodes.Add(new TreeNode(Path.GetFileName(f)) { Tag = "file:" + f, ToolTipText = f });
            }
        }

        /// <returns>true if the node was handled here</returns>
        private bool HandleExtendedImportNode(TreeNode node)
        {
            if (node.Tag is not string tag)
            {
                return false;
            }

            switch (tag)
            {
                case "txt":
                case "ext:auto":
                    OpenAnyFile(NumberMode.Auto, tag == "txt" ? "Text (*.txt)|*.txt|All files|*.*" : null);
                    return true;
                case "ext:dec":
                    OpenAnyFile(NumberMode.Decimal, "Text (*.txt)|*.txt|All files|*.*");
                    return true;
                case "ext:hex":
                    OpenAnyFile(NumberMode.Hex, "Text (*.txt)|*.txt|All files|*.*");
                    return true;
                case "uoab":
                    OpenAnyFile(NumberMode.Auto, "UO Architect (*.uoa)|*.uoa");
                    return true;
                case "ext:servuo":
                    OpenAnyFile(NumberMode.Auto, "ServUO addon (*.cs)|*.cs|All files|*.*");
                    return true;
                case "ext:mep":
                    OpenAnyFile(NumberMode.Auto, "MultiEditor project (*.mep)|*.mep");
                    return true;
                case "ext:recentroot":
                    return true;
            }

            if (tag.StartsWith("file:"))
            {
                OpenFile(tag.Substring(5), NumberMode.Auto);
                return true;
            }

            return false;
        }

        private void OpenAnyFile(NumberMode mode, string filter = null)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = filter ?? MultiFormats.ImportFilter,
                Title = L.T("Multi-Datei öffnen", "Open multi file"),
                InitialDirectory = Directory.Exists(Settings.LastImportDir) ? Settings.LastImportDir : string.Empty
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            Settings.LastImportDir = Path.GetDirectoryName(dlg.FileName);
            OpenFile(dlg.FileName, mode);
        }

        private void OpenFile(string fileName, NumberMode mode)
        {
            if (!File.Exists(fileName))
            {
                MessageBox.Show(this, L.T("Datei nicht gefunden: ", "File not found: ") + fileName);
                Settings.RecentFiles.Remove(fileName);
                Settings.Save();
                RefreshRecentNode();
                return;
            }

            ImportResult r;
            try
            {
                Cursor = Cursors.WaitCursor;
                r = MultiFormats.Load(fileName, mode);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, L.T("Import-Fehler", "Import error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            if (r.Tiles.Count == 0)
            {
                MessageBox.Show(this, string.Join(Environment.NewLine, r.Warnings.DefaultIfEmpty(L.T("Keine Kacheln gefunden.", "No tiles found."))),
                    L.T("Import", "Import"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (r.Width.HasValue && r.Height.HasValue)
            {
                int ox = r.OriginX ?? 0;
                int oy = r.OriginY ?? 0;
                List<ImportTile> cells = r.Tiles.Select(t =>
                {
                    ImportTile c = t.Clone();
                    c.X += ox;
                    c.Y += oy;
                    return c;
                }).ToList();
                _compList = new MultiEditorComponentList(cells, r.Width.Value, r.Height.Value, ox, oy, this);
            }
            else
            {
                _compList = new MultiEditorComponentList(r.Tiles, this);
            }

            AfterMultiLoaded(r.Name, null);
            if (r.Markers != null && r.OriginX.HasValue)
            {
                foreach (int[] m in r.Markers.Where(m => m.Length >= 4))
                {
                    _houseMarkers.Add(new HouseMarker { Kind = (HouseMarkerKind)m[0], X = m[1] + r.OriginX.Value, Y = m[2] + r.OriginY.Value, Z = m[3] });
                }
            }

            Settings.AddRecentFile(fileName);
            RefreshRecentNode();

            int invalid = r.Tiles.Count(t => !Art.IsValidStatic(t.Id));
            string msg = $"{r.Format}: {r.Tiles.Count} " + L.T("Kacheln", "tiles");
            if (invalid > 0)
            {
                msg += L.T($", {invalid} ohne Grafik", $", {invalid} without art");
            }

            ShowStatus(msg);
            if (r.Warnings.Count > 0 || invalid > r.Tiles.Count / 3)
            {
                string w = string.Join(Environment.NewLine, r.Warnings);
                if (invalid > r.Tiles.Count / 3)
                {
                    w += Environment.NewLine + L.T(
                        "Viele IDs ohne Grafik – evtl. falsches Zahlenformat? Versuche Datei → Öffnen als → Text dezimal / hex.",
                        "Many ids without art – wrong number format? Try File → Open as → Text decimal / hex.");
                }

                MessageBox.Show(this, msg + Environment.NewLine + w.Trim(), "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Common refresh after a new multi was created / loaded
        /// </summary>
        private void AfterMultiLoaded(string name, int? id)
        {
            UndoList_Clear();
            if (!string.IsNullOrEmpty(name))
            {
                textBox_Export.Text = MultiFormats.SanitizeClassName(name);
            }

            textBox_SaveToID.Text = id?.ToString() ?? "0";
            vScrollBar.Value = 0;
            hScrollBar.Value = 0;
            AfterModelChanged();
        }

        #endregion

        #region Export / save

        private string AskSaveFile(string filter, string defaultName)
        {
            using var dlg = new SaveFileDialog
            {
                Filter = filter,
                FileName = defaultName,
                InitialDirectory = Directory.Exists(Settings.LastExportDir) ? Settings.LastExportDir : Options.OutputPath
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return null;
            }

            Settings.LastExportDir = Path.GetDirectoryName(dlg.FileName);
            Settings.Save();
            return dlg.FileName;
        }

        private string DesignName => string.IsNullOrWhiteSpace(textBox_Export.Text) ? "MyMulti" : textBox_Export.Text.Trim();

        private void SaveProject()
        {
            if (_compList == null)
            {
                return;
            }

            string file = AskSaveFile("MultiEditor project (*.mep)|*.mep", DesignName + ".mep");
            if (file == null)
            {
                return;
            }

            _compList.GetCanonicalCells(out int pw, out int ph, out int pox, out int poy);
            MultiFormats.SaveProject(file, DesignName, pw, ph, pox, poy, _compList.ToImportTiles(),
                GetCanonicalMarkers().Select(m => new[] { (int)m.Kind, m.X, m.Y, m.Z }).ToList());
            Settings.AddRecentFile(file);
            RefreshRecentNode();
            ShowStatus(L.T("Projekt gespeichert: ", "Project saved: ") + file);
        }

        private void ExportServUOAddon()
        {
            if (_compList == null)
            {
                return;
            }

            var d = new SimpleDialog(L.T("ServUO Addon exportieren", "Export ServUO addon"))
                .AddText("name", L.T("Klassenname:", "Class name:"), MultiFormats.SanitizeClassName(DesignName))
                .AddText("deed", L.T("Deed-Name:", "Deed name:"), DesignName)
                .AddText("ns", "Namespace:", Settings.ServUONamespace)
                .AddCombo("style", L.T("Stil:", "Style:"), new object[] { L.T("Array (kompakt)", "Array (compact)"), L.T("Zeilen (AddComponent)", "Lines (AddComponent)") }, Settings.AddonStyle)
                .AddCheck("hex", L.T("IDs als Hex (0x…)", "Ids as hex (0x…)"), Settings.AddonHexIds)
                .AddCheck("inv", L.T("Unsichtbare Kacheln (Flag 0) mit exportieren", "Export invisible tiles (flag 0)"), true)
                .AddCombo("scope", L.T("Inhalt:", "Content:"), new object[] { L.T("Alles", "Everything"), L.T("Nur Einrichtung", "Furniture only"), L.T("Ohne Einrichtung", "Without furniture") }, _compList.DecoCount > 0 ? 1 : 0)
                .AddInfo(L.T("Kacheln mit Hue werden mit AddComplexComponent geschrieben. Die Kachel 0x1 (Multi-Mitte) wird ausgelassen.",
                    "Tiles with hue are written with AddComplexComponent. Tile 0x1 (multi center) is skipped."));
            if (!d.ShowOk(this))
            {
                return;
            }

            var o = new AddonExportOptions
            {
                Name = d.GetText("name"),
                DeedName = d.GetText("deed"),
                Namespace = string.IsNullOrWhiteSpace(d.GetText("ns")) ? "Server.Items" : d.GetText("ns").Trim(),
                Style = (AddonScriptStyle)d.GetIndex("style"),
                HexIds = d.GetCheck("hex"),
                IncludeInvisibleFlag = d.GetCheck("inv")
            };
            Settings.ServUONamespace = o.Namespace;
            Settings.AddonStyle = (int)o.Style;
            Settings.AddonHexIds = o.HexIds;

            string cls = MultiFormats.SanitizeClassName(o.Name);
            string file = AskSaveFile("C# script (*.cs)|*.cs", cls + "Addon.cs");
            if (file == null)
            {
                return;
            }

            int scope = d.GetIndex("scope");
            List<ImportTile> exportTiles = _compList.ToImportTiles().Where(t => scope == 0 || (scope == 1 ? t.Deco : !t.Deco)).ToList();
            File.WriteAllText(file, MultiFormats.CreateServUOAddon(exportTiles, o), new UTF8Encoding(false));
            MessageBox.Show(this, L.T($"Addon gespeichert:\n{file}\n\nIm Spiel: [add {cls}AddonDeed", $"Addon saved:\n{file}\n\nIn game: [add {cls}AddonDeed"),
                "ServUO", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportGeneric(string kind)
        {
            if (_compList == null)
            {
                return;
            }

            List<ImportTile> tiles = _compList.ToImportTiles();
            string file;
            switch (kind)
            {
                case "txthex":
                case "txtdec":
                    file = AskSaveFile("Text (*.txt)|*.txt", DesignName + ".txt");
                    if (file != null)
                    {
                        MultiFormats.SaveTextList(file, tiles, kind == "txthex", true);
                    }

                    break;
                case "wsc":
                    file = AskSaveFile("WSC (*.wsc)|*.wsc", DesignName + ".wsc");
                    if (file != null)
                    {
                        MultiFormats.SaveWsc(file, tiles);
                    }

                    break;
                case "xml":
                    file = AskSaveFile("XML (*.xml)|*.xml", DesignName + ".xml");
                    if (file != null)
                    {
                        MultiFormats.SaveXml(file, tiles, DesignName);
                    }

                    break;
                case "png":
                    file = AskSaveFile("PNG (*.png)|*.png", DesignName + ".png");
                    if (file != null)
                    {
                        using Bitmap bmp = MultiPreview.Render(tiles, 8000, 8000);
                        bmp?.Save(file, System.Drawing.Imaging.ImageFormat.Png);
                    }

                    break;
                default:
                    return;
            }

            if (file != null)
            {
                ShowStatus(L.T("Gespeichert: ", "Saved: ") + file);
            }
        }

        private bool TryGetSaveId(out int id)
        {
            if (Utils.ConvertStringToInt(textBox_SaveToID.Text, out id, 0, Multis.MaximumMultiIndex - 1))
            {
                return true;
            }

            MessageBox.Show(this, L.T("Ungültige Multi-ID im Reiter Save.", "Invalid multi id in the Save tab."), "Multi.mul", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private void SaveToMultiMul()
        {
            if (_compList == null || !TryGetSaveId(out int id))
            {
                return;
            }

            string current = null;
            try
            {
                current = Files.GetFilePath("multi.mul");
            }
            catch
            {
                // ignore
            }

            string clientDir = current != null ? Path.GetDirectoryName(current) : Files.RootDir;
            bool usesUop = false;
            try
            {
                usesUop = Files.GetFilePath("MultiCollection.uop") != null;
            }
            catch
            {
                // ignore
            }

            var d = new SimpleDialog(L.T("In Multi.mul schreiben", "Write to Multi.mul"))
                .AddInfo(L.T($"Multi wird unter ID {id} (0x{id:X}) gespeichert und multi.mul + multi.idx werden geschrieben.",
                    $"The multi is stored as id {id} (0x{id:X}) and multi.mul + multi.idx are written."))
                .AddCheck("direct", L.T($"Direkt in den Client-Ordner schreiben ({clientDir})", $"Write directly into the client folder ({clientDir})"), false)
                .AddCheck("backup", L.T("Vorher Sicherung anlegen (*.bak_Datum)", "Create backup first (*.bak_date)"), Settings.AutoBackup)
                .AddInfo(L.T($"Sonst: Ausgabeordner {Options.OutputPath}", $"Otherwise: output folder {Options.OutputPath}") +
                         (usesUop ? Environment.NewLine + L.T("Hinweis: der Client nutzt MultiCollection.uop – danach mit dem UOP-Packer packen.", "Note: the client uses MultiCollection.uop – pack it with the UOP packer afterwards.") : string.Empty));
            if (!d.ShowOk(this))
            {
                return;
            }

            bool direct = d.GetCheck("direct");
            Settings.AutoBackup = d.GetCheck("backup");
            Settings.Save();

            if (Multis.GetComponents(id) != MultiComponentList.Empty &&
                MessageBox.Show(this, L.T($"ID {id} ist belegt. Überschreiben?", $"Id {id} is used. Overwrite?"), "Multi.mul", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            string temp = Path.Combine(Path.GetTempPath(), "MultiEditor_" + Guid.NewGuid().ToString("N"));
            try
            {
                Cursor = Cursors.WaitCursor;
                _compList.AddToSdkComponentList(id);

                // load every multi into memory first, the source file may be the target
                for (int i = 0; i < Multis.MaximumMultiIndex; ++i)
                {
                    Multis.GetComponents(i);
                }

                Directory.CreateDirectory(temp);
                Multis.Save(temp);

                string targetDir = direct && !string.IsNullOrEmpty(clientDir) ? clientDir : Options.OutputPath;
                Directory.CreateDirectory(targetDir);
                string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                foreach (string name in new[] { "multi.mul", "multi.idx" })
                {
                    string target = Path.Combine(targetDir, name);
                    if (Settings.AutoBackup && File.Exists(target))
                    {
                        File.Copy(target, target + ".bak_" + stamp, true);
                    }

                    File.Copy(Path.Combine(temp, name), target, true);
                }

                MessageBox.Show(this, L.T($"Gespeichert unter ID {id} in\n{targetDir}", $"Saved as id {id} in\n{targetDir}"), "Multi.mul", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (IOException ex)
            {
                MessageBox.Show(this, ex.Message + Environment.NewLine + Environment.NewLine +
                                      L.T("Die Datei ist vermutlich vom Client oder UO Fiddler geöffnet. Speichere in den Ausgabeordner und kopiere sie danach.",
                                          "The file is probably opened by the client or UO Fiddler. Save into the output folder and copy it afterwards."),
                    "Multi.mul", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Multi.mul", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                try
                {
                    if (Directory.Exists(temp))
                    {
                        Directory.Delete(temp, true);
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        private void FindNextFreeId()
        {
            Utils.ConvertStringToInt(textBox_SaveToID.Text, out int start, 0, Multis.MaximumMultiIndex - 1);
            for (int i = Math.Max(0, start); i < Multis.MaximumMultiIndex; ++i)
            {
                if (Multis.GetComponents(i) == MultiComponentList.Empty)
                {
                    textBox_SaveToID.Text = i.ToString();
                    ShowStatus(L.T($"Nächste freie ID: {i} (0x{i:X})", $"Next free id: {i} (0x{i:X})"));
                    TC_MultiEditorToolbox.SelectedTab = Save;
                    return;
                }
            }

            ShowStatus(L.T("Keine freie ID gefunden", "No free id found"));
        }

        private void DeleteMultiId()
        {
            if (!TryGetSaveId(out int id))
            {
                return;
            }

            if (MessageBox.Show(this, L.T($"Multi {id} (0x{id:X}) aus dem Speicher löschen? (Danach Multi.mul speichern)", $"Delete multi {id} (0x{id:X}) from memory? (Save multi.mul afterwards)"),
                    "Multi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            Multis.Remove(id);
            Options.ChangedUltimaClass["Multis"] = true;
            ControlEvents.FireMultiChangeEvent(_background, id);
        }

        #endregion

        #region Background / map

        private void SetBackgroundPattern(BackgroundPattern pattern)
        {
            if (pattern == null)
            {
                _background.Mode = BackgroundMode.None;
            }
            else
            {
                _background.Mode = BackgroundMode.Pattern;
                _background.Pattern = pattern;
            }

            ScrollbarsSetValue();
            pictureBoxMulti.Invalidate();
        }

        private void LoadBackgroundImage()
        {
            using var dlg = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*" };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                using var tmp = new Bitmap(dlg.FileName);
                _background.Image?.Dispose();
                _background.Image = new Bitmap(tmp);
                _background.ImagePath = dlg.FileName;
                _background.Mode = BackgroundMode.Image;
                ScrollbarsSetValue();
                pictureBoxMulti.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void BackgroundDialog()
        {
            int patternIndex = Array.IndexOf(BackgroundLayer.Patterns, _background.Pattern);
            BackgroundPattern custom = BackgroundLayer.Patterns[BackgroundLayer.Patterns.Length - 1];
            var d = new SimpleDialog(L.T("Hintergrund", "Background"))
                .AddCombo("mode", L.T("Art:", "Type:"), new object[] { L.T("Kein", "None"), L.T("Muster", "Pattern"), L.T("Bild", "Image"), L.T("Map-Bereich", "Map area") }, (int)_background.Mode)
                .AddCombo("pattern", L.T("Muster:", "Pattern:"), BackgroundLayer.Patterns.Select(p => (object)p.Name), Math.Max(0, patternIndex))
                .AddText("custom", L.T("Eigene Land-IDs:", "Custom land ids:"), string.Join(",", custom.LandIds.Select(i => $"0x{i:X}")))
                .AddNumber("margin", L.T("Rand (Felder):", "Margin (cells):"), _background.Margin, 0, 40)
                .AddNumber("ix", L.T("Bild Versatz X:", "Image offset X:"), _background.ImageOffsetX, -5000, 5000)
                .AddNumber("iy", L.T("Bild Versatz Y:", "Image offset Y:"), _background.ImageOffsetY, -5000, 5000)
                .AddNumber("scale", L.T("Bild Skalierung:", "Image scale:"), (decimal)_background.ImageScale, 0.1m, 10m, 2)
                .AddNumber("alpha", L.T("Bild Deckkraft:", "Image opacity:"), (decimal)_background.ImageAlpha, 0.05m, 1m, 2)
                .AddNumber("salpha", L.T("Statics Deckkraft:", "Statics opacity:"), (decimal)_background.StaticsAlpha, 0.05m, 1m, 2)
                .AddCheck("newimg", L.T("Neues Bild laden…", "Load new image…"), false);
            if (!d.ShowOk(this))
            {
                return;
            }

            var ids = new List<int>();
            foreach (string part in d.GetText("custom").Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (SimpleDialog.TryParseId(part, out int id))
                {
                    ids.Add(id);
                }
            }

            if (ids.Count > 0)
            {
                custom.LandIds = ids.ToArray();
            }

            _background.Margin = d.GetInt("margin");
            _background.ImageOffsetX = d.GetInt("ix");
            _background.ImageOffsetY = d.GetInt("iy");
            _background.ImageScale = d.GetFloat("scale");
            _background.ImageAlpha = d.GetFloat("alpha");
            _background.StaticsAlpha = d.GetFloat("salpha");
            _background.Pattern = BackgroundLayer.Patterns[Math.Max(0, d.GetIndex("pattern"))];

            var mode = (BackgroundMode)d.GetIndex("mode");
            if (mode == BackgroundMode.Image && (d.GetCheck("newimg") || _background.Image == null))
            {
                LoadBackgroundImage();
                return;
            }

            if (mode == BackgroundMode.Map && _background.Map == null)
            {
                LoadMapArea();
                return;
            }

            _background.Mode = mode;
            ScrollbarsSetValue();
            pictureBoxMulti.Invalidate();
        }

        private void StaticsAlphaDialog()
        {
            var d = new SimpleDialog(L.T("Statics-Transparenz", "Statics transparency"))
                .AddNumber("a", L.T("Deckkraft (0.05 - 1):", "Opacity (0.05 - 1):"), (decimal)_background.StaticsAlpha, 0.05m, 1m, 2);
            if (!d.ShowOk(this))
            {
                return;
            }

            _background.StaticsAlpha = d.GetFloat("a");
            pictureBoxMulti.Invalidate();
        }

        private void LoadMapArea()
        {
            int w = _compList?.Width ?? (int)numericUpDown_Size_Width.Value;
            int h = _compList?.Height ?? (int)numericUpDown_Size_Height.Value;
            var area = new Rectangle(_background.MapX, _background.MapY, Math.Max(1, w), Math.Max(1, h));
            if (area.X == 0 && area.Y == 0)
            {
                area.X = 1400;
                area.Y = 1600;
            }

            using var form = new MapAreaForm(_background.MapIndex, area, _compList == null || _compList.RealTileCount == 0);
            if (form.ShowDialog(this) != DialogResult.OK || form.SelectedMap == null)
            {
                return;
            }

            Rectangle a = form.Area;
            _background.Map = form.SelectedMap;
            _background.MapIndex = form.MapIndex;
            _background.MapX = a.X;
            _background.MapY = a.Y;
            _background.BaseZ = form.BaseZ;
            _background.Mode = BackgroundMode.Map;

            if (form.CreateNewMulti || _compList == null)
            {
                numericUpDown_Size_Width.Value = Math.Min(numericUpDown_Size_Width.Maximum, a.Width);
                numericUpDown_Size_Height.Value = Math.Min(numericUpDown_Size_Height.Maximum, a.Height);
                _compList = new MultiEditorComponentList(a.Width, a.Height, this);
                AfterMultiLoaded(textBox_Export.Text, null);
            }

            if (form.ImportStatics)
            {
                ImportMapStatics();
            }

            Settings.ShowGrid = true;
            if (_tbGrid != null)
            {
                _tbGrid.Checked = true;
            }

            ScrollbarsSetValue();
            pictureBoxMulti.Invalidate();
            ShowStatus(_background.Describe());
        }

        private void ImportMapStatics()
        {
            if (_compList == null || !_background.HasMap)
            {
                return;
            }

            List<ImportTile> tiles = _background.GetStaticsAsTiles(_compList.Width, _compList.Height);
            int n = _compList.TilesAdd(tiles, "Import map statics");
            AfterModelChanged();
            ShowStatus(L.T($"{n} Statics übernommen (Map-Statics ggf. ausblenden: Menü Map)", $"{n} statics imported (hide map statics: menu Map)"));
        }

        private void FindStaticDialog()
        {
            if (_compList == null || !_background.HasMap)
            {
                MessageBox.Show(this, L.T("Zuerst einen Map-Bereich laden.", "Load a map area first."));
                return;
            }

            string text = SimpleDialog.AskText(this, L.T("Static finden", "Find static"), L.T("ID (leer = Markierung aus):", "Id (empty = clear marks):"),
                _drawTile.Id != 0xFFFF ? $"0x{_drawTile.Id:X4}" : string.Empty);
            if (text == null)
            {
                return;
            }

            if (!SimpleDialog.TryParseId(text, out int id))
            {
                _background.HighlightStaticId = -1;
                pictureBoxMulti.Invalidate();
                return;
            }

            _background.HighlightStaticId = id;
            int count = _background.FindStatic(id, _compList.Width, _compList.Height).Count;
            pictureBoxMulti.Invalidate();
            ShowStatus(L.T($"0x{id:X4}: {count} Fundstellen (magenta markiert)", $"0x{id:X4}: {count} found (marked magenta)"));
        }

        private void WriteToStatics()
        {
            if (_compList == null)
            {
                return;
            }

            bool hasMap = _background.HasMap;
            var mapNames = Enumerable.Range(0, Map.BuiltInCount).Select(i => (object)$"{i}: {(i < Options.MapNames.Length ? Options.MapNames[i] : "Map " + i)}");
            var d = new SimpleDialog(L.T("Multi in Statics schreiben", "Write multi to statics"))
                .AddCombo("map", "Map:", mapNames, _background.MapIndex)
                .AddNumber("x", L.T("Welt X:", "World X:"), hasMap ? _background.MapX : 0, 0, 10000)
                .AddNumber("y", L.T("Welt Y:", "World Y:"), hasMap ? _background.MapY : 0, 0, 10000)
                .AddNumber("z", L.T("Welt Z (für Multi Z 0):", "World Z (for multi Z 0):"), hasMap ? _background.BaseZ : 0, -128, 127)
                .AddCheck("center", L.T("Position ist der Ursprung (Mitte) statt Zelle 0,0", "Position is the origin (center) instead of cell 0,0"), !hasMap)
                .AddCheck("skipinv", L.T("Unsichtbare Kacheln (Flag 0) auslassen", "Skip invisible tiles (flag 0)"), true)
                .AddCheck("save", L.T("Sofort speichern (statics/staidx in den Ausgabeordner)", "Save now (statics/staidx into the output folder)"), false)
                .AddCheck("freeze", L.T("Zusätzlich Freeze-Datei (WSC) speichern", "Also save a freeze file (WSC)"), false)
                .AddInfo(L.T("Vor dem Speichern rückgängig machbar (Menü Map). Nach dem Speichern nicht mehr – vorher Sicherung machen!", "Can be undone before saving (menu Map). Not after saving – make a backup first!"));
            if (!d.ShowOk(this))
            {
                return;
            }

            Map map = Map.GetBuiltIn(d.GetIndex("map"));
            if (map == null)
            {
                return;
            }

            int baseX = d.GetInt("x");
            int baseY = d.GetInt("y");
            int baseZ = d.GetInt("z");
            bool center = d.GetCheck("center");
            bool skipInvisible = d.GetCheck("skipinv");

            // always in the original orientation (view rotation is undone here)
            List<ImportTile> cells = _compList.GetCanonicalCells(out _, out _, out int ox, out int oy);
            var statics = new List<MapStatic>();
            foreach (ImportTile t in cells)
            {
                if (t.Id == 0x1 || (skipInvisible && t.Flag == 0))
                {
                    continue;
                }

                statics.Add(new MapStatic(
                    baseX + (center ? t.X - ox : t.X),
                    baseY + (center ? t.Y - oy : t.Y),
                    t.Id,
                    Math.Max(-128, Math.Min(127, baseZ + t.Z)),
                    t.Hue));
            }

            try
            {
                int written = _mapEditor.Add(map, statics, "Write multi to statics");

                if (d.GetCheck("freeze"))
                {
                    string file = AskSaveFile("WSC (*.wsc)|*.wsc", DesignName + "_freeze.wsc");
                    if (file != null)
                    {
                        MultiFormats.SaveFreezeFile(file, statics.Select(w => new ImportTile { Id = w.Id, X = w.X, Y = w.Y, Z = w.Z, Hue = w.Hue }), 0, 0, 0);
                    }
                }

                if (d.GetCheck("save"))
                {
                    Cursor = Cursors.WaitCursor;
                    _mapEditor.SaveAll(Options.OutputPath);
                    Cursor = Cursors.Default;
                    MessageBox.Show(this, L.T($"{written} Statics geschrieben und gespeichert in\n{Options.OutputPath}", $"{written} statics written and saved to\n{Options.OutputPath}"),
                        "Statics", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(this, L.T($"{written} Statics hinzugefügt (noch nicht gespeichert – Menü Map → „Map-Statics speichern…“, rückgängig mit „Map-Änderung rückgängig“).",
                            $"{written} statics added (not saved yet – menu Map → \"Save map statics…\", undo with \"Undo map change\")."),
                        "Statics", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show(this, ex.Message, "Statics", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            UpdateInfo();
            pictureBoxMulti.Invalidate();
        }

        #endregion

        #region Generator

        private HouseGeneratorForm _generatorForm;

        private void OpenGenerator()
        {
            if (_generatorForm == null || _generatorForm.IsDisposed)
            {
                _generatorForm = new HouseGeneratorForm(
                    () => _compList?.ToImportTiles(),
                    () => _drawTile.Id,
                    GeneratorCreateNew,
                    GeneratorInsert,
                    GeneratorRestyle);
            }

            _generatorForm.Show(this);
            _generatorForm.BringToFront();
        }

        private void GeneratorCreateNew(List<ImportTile> cellTiles, string name)
        {
            if (cellTiles.Count == 0)
            {
                return;
            }

            if (_compList != null && _compList.RealTileCount > 0 &&
                MessageBox.Show(this, L.T("Aktuelles Multi ersetzen?", "Replace the current multi?"), "Generator", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            int w = cellTiles.Max(t => t.X) + 1;
            int h = cellTiles.Max(t => t.Y) + 1;
            var tmp = new MultiEditorComponentList(cellTiles, w, h, 0, 0, this);
            tmp.SetDefaultOrigin();
            _compList = tmp;
            AfterMultiLoaded(name, null);
        }

        private void GeneratorInsert(List<ImportTile> cellTiles)
        {
            if (_compList == null)
            {
                GeneratorCreateNew(cellTiles, "House");
                return;
            }

            // put it at the selected tile or at 0,0
            int ox = SelectedTile?.X ?? 0;
            int oy = SelectedTile?.Y ?? 0;
            int needW = cellTiles.Max(t => t.X) + ox + 1;
            int needH = cellTiles.Max(t => t.Y) + oy + 1;
            if (needW > _compList.Width || needH > _compList.Height)
            {
                _compList.Resize(Math.Max(needW, _compList.Width), Math.Max(needH, _compList.Height));
            }

            _compList.TilesAdd(cellTiles.Select(t =>
            {
                ImportTile c = t.Clone();
                c.X += ox;
                c.Y += oy;
                return c;
            }), "Generator");
            AfterModelChanged();
        }

        private void GeneratorRestyle(HouseStyle style)
        {
            if (_compList == null)
            {
                return;
            }

            Dictionary<ushort, ushort> map = StyleKnowledge.Instance.GetRestyleMap(_compList.RealTiles.Select(t => (int)t.Id), style);
            int n = _compList.ReplaceIds(map, "Restyle");
            AfterModelChanged();
            ShowStatus(L.T($"{n} Kacheln umgestylt", $"{n} tiles restyled"));
        }

        private void LearnFromCurrent()
        {
            if (_compList == null)
            {
                return;
            }

            HouseStyle s = StyleKnowledge.Instance.Learn(_compList.ToImportTiles(), DesignName);
            StyleKnowledge.Instance.Save();
            MessageBox.Show(this, s.IsUsable
                    ? L.T($"Stil „{s.Name}“ gelernt: Wand X 0x{s.WallX:X}, Wand Y 0x{s.WallY:X}, Boden 0x{s.Floor:X}", $"Style \"{s.Name}\" learned: wall X 0x{s.WallX:X}, wall Y 0x{s.WallY:X}, floor 0x{s.Floor:X}")
                    : L.T("Rollen gelernt, aber kein vollständiger Stil (Wände/Boden) gefunden.", "Roles learned, but no complete style (walls/floor) found."),
                "Generator", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        private void NewBlankAsk()
        {
            var d = new SimpleDialog(L.T("Neues Multi", "New multi"))
                .AddNumber("w", L.T("Breite:", "Width:"), numericUpDown_Size_Width.Value, 1, 1000)
                .AddNumber("h", L.T("Tiefe:", "Depth:"), numericUpDown_Size_Height.Value, 1, 1000)
                .AddText("name", "Name:", DesignName);
            if (!d.ShowOk(this))
            {
                return;
            }

            numericUpDown_Size_Width.Value = d.GetInt("w");
            numericUpDown_Size_Height.Value = d.GetInt("h");
            _compList = new MultiEditorComponentList(d.GetInt("w"), d.GetInt("h"), this);
            AfterMultiLoaded(d.GetText("name"), null);
        }
    }
}

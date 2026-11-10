using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextureCutter;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Texture Cutter: load images or textures, cut them into tiles, resize them, turn them into UO land tiles,
    /// change their colors, paint on them, put other graphics on top and save them as files or into texmaps.mul.
    /// </summary>
    public partial class TextureCutter : Form
    {
        private const int MaxUndo = 30;
        private static readonly string OutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempGrafic");

        private enum PreviewKind { None, Adjust, Mode, Particle, Transition }

        private enum PickTarget { Paint, From, To }

        private Bitmap _image;                 // the edited image (32bpp, owned)
        private Bitmap _previewImage;          // live preview of a color change, not applied yet
        private PreviewKind _previewKind = PreviewKind.None;
        private bool _resettingPreviewControls;
        private readonly List<Bitmap> _undo = new List<Bitmap>();
        private readonly List<Bitmap> _redo = new List<Bitmap>();
        private Bitmap _pattern;               // pattern brush (owned)
        private Bitmap _blendB;                // texture B of a transition (owned)
        private readonly CutterPalette _palette = new CutterPalette();
        private TerrainPreviewForm _terrain;
        private PickTarget _pickTarget = PickTarget.Paint;
        private CanvasTool _toolBeforePick = CanvasTool.Select;
        private int[] _customColors;
        private bool _onlySelection;
        private bool _protectBlackWhite = true;
        private bool[] _usedTextures;
        private bool _texLoaded;
        private bool _updating;

        public TextureCutter()
        {
            InitializeComponent();

            BuildMenus();
            BuildToolbar();
            BuildTabs();

            canvas.BeginEdit += Canvas_BeginEdit;
            canvas.ImageEdited += (s, e) => UpdateStatus();
            canvas.HoverPixel += Canvas_HoverPixel;
            canvas.ColorPicked += Canvas_ColorPicked;
            canvas.SelectionChanged += (s, e) => UpdateStatus();
            canvas.OverlayMoved += Canvas_LayerMoved;
            canvas.ActiveLayerChanged += (s, e) => RefreshLayerList();
            canvas.ImageEdited += (s, e) => NotifyTerrain();
            canvas.GridSize = new Size(64, 64);
            panelCanvas.MouseWheel += Canvas_MouseWheel;
            canvas.MouseWheel += Canvas_MouseWheel;
            tabControl1.SelectedIndexChanged += (s, e) =>
            {
                if (tabControl1.SelectedTab == tabTexmaps && !_texLoaded)
                {
                    ReloadTextures();
                }
            };

            try
            {
                if (File.Exists(CutterPalette.DefaultFile))
                {
                    _palette.Colors.AddRange(CutterPalette.Load(CutterPalette.DefaultFile).Colors);
                }
            }
            catch (Exception)
            {
                // a broken palette file is ignored
            }
            RefreshPalette();

            SetTool(CanvasTool.Select);
            SetPrimary(Color.Black);
            SetSecondary(Color.White);
            SyncUi();
            UpdateStatus();
            SetMessage("Ready – Ctrl+O open, Ctrl+V paste, F1 help.");
        }

        #region Small helpers

        private static string Tip(string en, string de) => en + "\n\nDE: " + de;

        private void SetMessage(string text) => statusMessage.Text = text;

        private static string Hex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        private static bool TryParseHex(string text, out Color color)
        {
            color = Color.Empty;
            string t = (text ?? string.Empty).Trim().TrimStart('#');
            if (t.Length != 6 || !int.TryParse(t, NumberStyles.HexNumber, null, out int rgb))
            {
                return false;
            }
            color = Color.FromArgb(255, Color.FromArgb(rgb));
            return true;
        }

        private static Bitmap Swatch(Color c)
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                CutterImaging.DrawChecker(g, new Rectangle(0, 0, 16, 16));
                using (var b = new SolidBrush(c))
                {
                    g.FillRectangle(b, 0, 0, 16, 16);
                }
                g.DrawRectangle(Pens.Gray, 0, 0, 15, 15);
            }
            return bmp;
        }

        private static void PaintSwatchButton(Button b, Color c)
        {
            b.BackColor = c;
            b.ForeColor = c.R * 0.299 + c.G * 0.587 + c.B * 0.114 > 150 ? Color.Black : Color.White;
            b.Text = Hex(c);
        }

        private bool CheckImage()
        {
            if (_image != null)
            {
                return true;
            }
            MessageBox.Show(this, "No image is loaded. Open one (Ctrl+O), paste one (Ctrl+V) or load a texture from the texmaps tab.",
                "Texture Cutter", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private Color? ChooseColor(Color current)
        {
            using (var dialog = new ColorDialog { Color = current, FullOpen = true })
            {
                if (_customColors != null)
                {
                    dialog.CustomColors = _customColors;
                }
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return null;
                }
                _customColors = dialog.CustomColors;
                return dialog.Color;
            }
        }

        private bool IsTextInputFocused()
        {
            foreach (ToolStripItem item in toolStrip1.Items)
            {
                if ((item is ToolStripComboBox cb && cb.Focused) || (item is ToolStripTextBox tb && tb.Focused))
                {
                    return true;
                }
            }
            Control c = ActiveControl;
            while (c is ContainerControl cc && cc.ActiveControl != null)
            {
                c = cc.ActiveControl;
            }
            return c is TextBoxBase || c is UpDownBase || c?.Parent is UpDownBase || c is ComboBox || c is ListView || c is ListBox || c is ArtTileGrid || c is TrackBar;
        }

        #endregion

        #region Menus and toolbar

        private ToolStripMenuItem _undoMenu, _redoMenu, _onlySelMenu, _protectMenu, _gridMenu, _tiledMenu, _topMostMenu;
        private ToolStripButton _tsUndo, _tsRedo, _tsMirror, _tsGrid, _tsTiled, _tsOnlySel, _tsProtect, _tsColor1, _tsColor2;
        private ToolStripComboBox _tsFormat, _tsShape, _tsBrush;
        private ToolStripLabel _tsZoom;
        private ToolStripSplitButton _tsPattern;
        private readonly Dictionary<CanvasTool, ToolStripButton> _toolButtons = new Dictionary<CanvasTool, ToolStripButton>();

        private static ToolStripMenuItem Item(string text, EventHandler click, string keys = null, Image image = null)
        {
            var item = new ToolStripMenuItem(text, image, click);
            if (keys != null)
            {
                item.ShortcutKeyDisplayString = keys;
            }
            return item;
        }

        private void BuildMenus()
        {
            var file = new ToolStripMenuItem("&File");
            file.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("Open image...", (s, e) => OpenImage(), "Ctrl+O", Properties.Resources.Load),
                Item("Paste image from clipboard", (s, e) => PasteImage(), "Ctrl+V", Properties.Resources.import),
                Item("Paste clipboard as layer", (s, e) => LayerFromClipboard()),
                new ToolStripSeparator(),
                Item("Save (tempGrafic)", (s, e) => QuickSave(), "Ctrl+S", Properties.Resources.Save),
                Item("Save as...", (s, e) => SaveAs(), "Ctrl+Shift+S"),
                Item("Open tempGrafic folder", (s, e) => OpenFolder(OutputDirectory)),
                new ToolStripSeparator(),
                Item("Save texmaps.mul (output folder)", (s, e) => SaveTexmaps()),
                new ToolStripSeparator(),
                Item("Close", (s, e) => Close())
            });

            _undoMenu = Item("Undo", (s, e) => Undo(), "Ctrl+Z");
            _redoMenu = Item("Redo", (s, e) => Redo(), "Ctrl+Y");
            _onlySelMenu = Item("Changes only inside the selection", (s, e) => ToggleOnlySelection());
            _protectMenu = Item("Protect black / white", (s, e) => ToggleProtect());
            var edit = new ToolStripMenuItem("&Edit");
            edit.DropDownItems.AddRange(new ToolStripItem[]
            {
                _undoMenu, _redoMenu, new ToolStripSeparator(),
                Item("Copy image", (s, e) => CopyImage(), "Ctrl+C", Properties.Resources.Copy),
                Item("Copy selection", (s, e) => CopySelection(), "Ctrl+Shift+C"),
                new ToolStripSeparator(),
                Item("Select all", (s, e) => canvas.SelectAll(), "Ctrl+A"),
                Item("Remove selection", (s, e) => canvas.ClearSelection(), "Esc"),
                Item("Crop to selection", (s, e) => CropToSelection()),
                Item("Delete selection (transparent)", (s, e) => DeleteSelection(), "Del"),
                new ToolStripSeparator(),
                _onlySelMenu, _protectMenu
            });

            var image = new ToolStripMenuItem("&Image");
            image.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("Rotate 90° right", (s, e) => ApplyFilter("Rotate", b => Rotated(b, RotateFlipType.Rotate90FlipNone), true), null, Properties.Resources.Rotate),
                Item("Rotate 90° left", (s, e) => ApplyFilter("Rotate", b => Rotated(b, RotateFlipType.Rotate270FlipNone), true)),
                Item("Mirror horizontally", (s, e) => ApplyFilter("Mirror", b => Rotated(b, RotateFlipType.RotateNoneFlipX), true), null, Properties.Resources.Mirror1),
                Item("Mirror vertically", (s, e) => ApplyFilter("Mirror", b => Rotated(b, RotateFlipType.RotateNoneFlipY), true)),
                new ToolStripSeparator(),
                Item("Texture → land tile 44 × 44", (s, e) => MakeLandTile()),
                Item("Make seamless (repeatable)", (s, e) => MakeSeamless()),
                Item("Reduce to UO 16-bit colors", (s, e) => ApplyFilter("UO 16-bit colors", TextureImaging.To16Bit)),
                new ToolStripSeparator(),
                Item("New empty image...", (s, e) => NewImage())
            });

            _gridMenu = Item("Tile grid", (s, e) => ToggleGrid(), "Ctrl+G", Properties.Resources.Schablone);
            _tiledMenu = Item("Tiled view 3 × 3 (seam check)", (s, e) => ToggleTiled(), "Ctrl+T");
            _topMostMenu = Item("Always on top", (s, e) => { TopMost = !TopMost; SyncUi(); });
            var view = new ToolStripMenuItem("&View");
            view.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("Zoom in", (s, e) => SetZoom(canvas.Zoom + 1), "Ctrl++"),
                Item("Zoom out", (s, e) => SetZoom(canvas.Zoom - 1), "Ctrl+-"),
                Item("Zoom 1:1", (s, e) => SetZoom(1), "Ctrl+0"),
                Item("Fit into window", (s, e) => FitZoom(), "Ctrl+F"),
                new ToolStripSeparator(),
                _gridMenu,
                Item("Grid color...", (s, e) => { Color? c = ChooseColor(canvas.GridColor); if (c.HasValue) { canvas.GridColor = c.Value; canvas.Invalidate(); } }),
                _tiledMenu,
                new ToolStripSeparator(),
                _topMostMenu
            });

            var help = new ToolStripMenuItem("&Help");
            help.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("Help / Hilfe", (s, e) => ShowHelp(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de"), "F1"),
                Item("Help (English)", (s, e) => ShowHelp(false)),
                Item("Hilfe (Deutsch)", (s, e) => ShowHelp(true))
            });

            var tools = new ToolStripMenuItem("&Tools");
            tools.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("Terrain preview (2D / 3D, map)...", (s, e) => OpenTerrainPreview(), "Ctrl+P"),
                Item("Batch – many images at once...", (s, e) => OpenBatch(), "Ctrl+B"),
                new ToolStripSeparator(),
                Item("Pattern brush from selected layer", (s, e) => PatternFrom(() => ActiveLayer == null ? null : CutterImaging.To32Bpp(ActiveLayer.Image), "layer")),
                Item("Pattern brush from selection", (s, e) => PatternFrom(() => _image == null ? null : ExtractSelection(_image), "selection")),
                Item("Pattern brush from texture (texmaps)", (s, e) => PatternFrom(() => _gridTex.SelectedItem >= 0 ? TextureMul.GetCopy(_gridTex.SelectedItem) : null, "texture")),
                Item("Pattern brush from file...", (s, e) => PatternFrom(PatternFromFile, "file")),
                Item("Pattern brush from clipboard", (s, e) => PatternFrom(ReadClipboard, "clipboard image")),
                Item("Pattern brush off", (s, e) => SetPattern(null))
            });

            menuStrip1.Items.AddRange(new ToolStripItem[] { file, edit, image, view, tools, help });
        }

        private ToolStripButton TextButton(string text, string tip, EventHandler click, bool bold = false)
        {
            var b = new ToolStripButton(text) { DisplayStyle = ToolStripItemDisplayStyle.Text, ToolTipText = tip };
            if (bold)
            {
                b.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            }
            b.Click += click;
            return b;
        }

        private ToolStripButton ImageButton(Image image, string tip, EventHandler click)
        {
            var b = new ToolStripButton(image) { DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = tip };
            b.Click += click;
            return b;
        }

        private void BuildToolbar()
        {
            _tsFormat = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(60, 23), ToolTipText = Tip("File format for Save and tiles", "Dateiformat für Speichern und Kacheln") };
            _tsFormat.Items.AddRange(new object[] { "BMP", "PNG", "TIFF", "JPEG" });
            _tsFormat.SelectedIndex = 0;

            _tsUndo = TextButton("↶", Tip("Undo (Ctrl+Z)", "Rückgängig (Strg+Z)"), (s, e) => Undo(), true);
            _tsRedo = TextButton("↷", Tip("Redo (Ctrl+Y)", "Wiederholen (Strg+Y)"), (s, e) => Redo(), true);

            void AddTool(CanvasTool tool, string text, string en, string de)
            {
                ToolStripButton b = TextButton(text, Tip(en, de), (s, e) => SetTool(tool));
                _toolButtons[tool] = b;
            }
            AddTool(CanvasTool.Select, "Select", "Select an area (M). Shape: list on the right.", "Bereich auswählen (M). Form: Liste rechts daneben.");
            AddTool(CanvasTool.Pencil, "Pencil", "Draw (B). Left = color 1, right = color 2.", "Zeichnen (B). Links = Farbe 1, rechts = Farbe 2.");
            AddTool(CanvasTool.Eraser, "Eraser", "Erase to transparent (E). Option on the Replace tab: erase to white.", "Radieren zu transparent (E). Option im Reiter Replace: zu Weiß radieren.");
            AddTool(CanvasTool.Line, "Line", "Straight line (L).", "Gerade Linie (L).");
            AddTool(CanvasTool.Rectangle, "Rect", "Rectangle outline (R).", "Rechteck-Rahmen (R).");
            AddTool(CanvasTool.Fill, "Fill", "Fill an area of similar color (G). Tolerance on the Replace tab.", "Fläche ähnlicher Farbe füllen (G). Toleranz im Reiter Replace.");
            AddTool(CanvasTool.Pipette, "Pipette", "Pick a color (I). Left = color 1, right = color 2.", "Farbe aufnehmen (I). Links = Farbe 1, rechts = Farbe 2.");
            AddTool(CanvasTool.Overlay, "Move layer", "Click a layer to select it, drag it (O). Arrow keys = 1 px, Shift = 10 px.", "Ebene anklicken und ziehen (O). Pfeiltasten = 1 px, Umschalt = 10 px.");

            _tsShape = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(85, 23), ToolTipText = Tip("Shape of the selection. Polygon: click points, double / right click closes.", "Form der Auswahl. Polygon: Punkte klicken, Doppel- / Rechtsklick schließt.") };
            _tsShape.Items.AddRange(new object[] { "Rectangle", "Ellipse", "Freehand", "Polygon" });
            _tsShape.SelectedIndex = 0;
            _tsShape.SelectedIndexChanged += (s, e) =>
            {
                canvas.SelectionShape = (CanvasSelection)_tsShape.SelectedIndex;
                canvas.ClearSelection();
                SetTool(CanvasTool.Select);
            };

            _tsColor1 = new ToolStripButton { DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = Tip("Color 1 (left mouse button). Click = choose.", "Farbe 1 (linke Maustaste). Klick = wählen.") };
            _tsColor1.Click += (s, e) => { Color? c = ChooseColor(canvas.PrimaryColor); if (c.HasValue) SetPrimary(c.Value); };
            _tsColor2 = new ToolStripButton { DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = Tip("Color 2 (right mouse button). Click = choose.", "Farbe 2 (rechte Maustaste). Klick = wählen.") };
            _tsColor2.Click += (s, e) => { Color? c = ChooseColor(canvas.SecondaryColor); if (c.HasValue) SetSecondary(c.Value); };
            ToolStripButton swap = TextButton("⇄", Tip("Swap color 1 and 2 (X)", "Farbe 1 und 2 tauschen (X)"), (s, e) => SwapColors());

            _tsBrush = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(45, 23), ToolTipText = Tip("Brush size in pixels", "Pinselgröße in Pixeln") };
            foreach (int i in new[] { 1, 2, 3, 4, 5, 6, 8, 10, 12, 16, 24, 32 })
            {
                _tsBrush.Items.Add(i);
            }
            _tsBrush.SelectedIndex = 0;
            _tsBrush.SelectedIndexChanged += (s, e) => canvas.BrushSize = (int)_tsBrush.SelectedItem;

            _tsPattern = new ToolStripSplitButton("Pattern") { DisplayStyle = ToolStripItemDisplayStyle.Text,
                ToolTipText = Tip("Pattern brush: pencil, line, rectangle and fill paint with an image. Arrow = choose the pattern.",
                    "Muster-Pinsel: Stift, Linie, Rechteck und Füllen malen mit einem Bild. Pfeil = Muster wählen.") };
            _tsPattern.ButtonClick += (s, e) =>
            {
                if (_pattern == null)
                {
                    _tsPattern.ShowDropDown();
                    return;
                }
                canvas.UsePattern = !canvas.UsePattern;
                SyncUi();
            };
            _tsPattern.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("From selected layer", (s, e) => PatternFrom(() => ActiveLayer == null ? null : CutterImaging.To32Bpp(ActiveLayer.Image), "layer")),
                Item("From selection", (s, e) => PatternFrom(() => _image == null ? null : ExtractSelection(_image), "selection")),
                Item("From texture (texmaps)", (s, e) => PatternFrom(() => _gridTex.SelectedItem >= 0 ? TextureMul.GetCopy(_gridTex.SelectedItem) : null, "texture")),
                Item("From file...", (s, e) => PatternFrom(PatternFromFile, "file")),
                Item("From clipboard", (s, e) => PatternFrom(ReadClipboard, "clipboard image")),
                Item("Off", (s, e) => SetPattern(null))
            });

            _tsMirror = TextButton("Mirror draw", Tip("Paint a mirrored copy of every stroke (left / right)", "Jeden Strich gespiegelt mitzeichnen (links / rechts)"),
                (s, e) => { canvas.MirrorDraw = !canvas.MirrorDraw; SyncUi(); });
            _tsOnlySel = TextButton("Only selection", Tip("Color changes and filters only inside the selection", "Farbänderungen und Filter nur innerhalb der Auswahl"),
                (s, e) => ToggleOnlySelection());
            _tsProtect = TextButton("Protect B/W", Tip("Pure black and white (UO transparency) stay unchanged by color changes", "Reines Schwarz und Weiß (UO-Transparenz) bleiben bei Farbänderungen unverändert"),
                (s, e) => ToggleProtect());

            _tsZoom = new ToolStripLabel("1x") { AutoSize = false, Width = 36 };
            _tsGrid = ImageButton(Properties.Resources.Schablone, Tip("Tile grid (Ctrl+G)", "Kachelraster (Strg+G)"), (s, e) => ToggleGrid());
            _tsTiled = TextButton("3×3", Tip("Tiled view: shows the image 3 × 3 times to check the seams (Ctrl+T)", "Kachelansicht: Bild 3 × 3 mal, um Nahtstellen zu prüfen (Strg+T)"), (s, e) => ToggleTiled());

            toolStrip1.Items.AddRange(new ToolStripItem[]
            {
                ImageButton(Properties.Resources.Load, Tip("Open image (Ctrl+O)", "Bild öffnen (Strg+O)"), (s, e) => OpenImage()),
                ImageButton(Properties.Resources.import, Tip("Paste image (Ctrl+V)", "Bild einfügen (Strg+V)"), (s, e) => PasteImage()),
                ImageButton(Properties.Resources.Save, Tip("Save into tempGrafic (Ctrl+S)", "In tempGrafic speichern (Strg+S)"), (s, e) => QuickSave()),
                _tsFormat,
                ImageButton(Properties.Resources.Copy, Tip("Copy image (Ctrl+C)", "Bild kopieren (Strg+C)"), (s, e) => CopyImage()),
                new ToolStripSeparator(), _tsUndo, _tsRedo, new ToolStripSeparator(),
                _toolButtons[CanvasTool.Select], _tsShape, _toolButtons[CanvasTool.Pencil], _toolButtons[CanvasTool.Eraser], _toolButtons[CanvasTool.Line],
                _toolButtons[CanvasTool.Rectangle], _toolButtons[CanvasTool.Fill], _toolButtons[CanvasTool.Pipette], _toolButtons[CanvasTool.Overlay],
                new ToolStripSeparator(), _tsColor1, _tsColor2, swap, _tsBrush, _tsPattern, _tsMirror,
                new ToolStripSeparator(), _tsOnlySel, _tsProtect,
                new ToolStripSeparator(),
                TextButton("–", Tip("Zoom out (Ctrl+-)", "Verkleinern (Strg+-)"), (s, e) => SetZoom(canvas.Zoom - 1), true), _tsZoom,
                TextButton("+", Tip("Zoom in (Ctrl++ or Ctrl+wheel)", "Vergrößern (Strg++ oder Strg+Mausrad)"), (s, e) => SetZoom(canvas.Zoom + 1), true),
                _tsGrid, _tsTiled, new ToolStripSeparator(),
                TextButton("Terrain", Tip("Terrain preview: the texture on the ground, 2D / 3D with heights, next to other textures or in a map area (Ctrl+P)",
                    "Gelände-Vorschau: die Textur auf dem Boden, 2D / 3D mit Höhen, neben anderen Texturen oder in einem Kartenbereich (Strg+P)"), (s, e) => OpenTerrainPreview()),
                TextButton("Batch", Tip("Process many images at once (Ctrl+B)", "Viele Bilder auf einmal bearbeiten (Strg+B)"), (s, e) => OpenBatch()),
                TextButton("?", Tip("Help (F1)", "Hilfe (F1)"), (s, e) => ShowHelp(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de"), true)
            });
        }

        private void SyncUi()
        {
            _undoMenu.Enabled = _tsUndo.Enabled = _undo.Count > 0;
            _redoMenu.Enabled = _tsRedo.Enabled = _redo.Count > 0;
            _onlySelMenu.Checked = _tsOnlySel.Checked = _onlySelection;
            _protectMenu.Checked = _tsProtect.Checked = _protectBlackWhite;
            _gridMenu.Checked = _tsGrid.Checked = canvas.ShowGrid;
            _tiledMenu.Checked = _tsTiled.Checked = canvas.TiledView;
            _topMostMenu.Checked = TopMost;
            _tsMirror.Checked = canvas.MirrorDraw;
            _tsPattern.Text = canvas.UsePattern && _pattern != null ? "Pattern ✔" : "Pattern";
            _tsZoom.Text = canvas.Zoom + "x";
            foreach (KeyValuePair<CanvasTool, ToolStripButton> kv in _toolButtons)
            {
                kv.Value.Checked = canvas.Tool == kv.Key;
            }
            if (_chkShowGrid != null && _chkShowGrid.Checked != canvas.ShowGrid)
            {
                _updating = true;
                _chkShowGrid.Checked = canvas.ShowGrid;
                _updating = false;
            }
        }

        private void UpdateStatus()
        {
            string img = _image == null ? "no image" : $"image {_image.Width}×{_image.Height}";
            Rectangle sel = canvas.GetSelectionBounds();
            string selText = sel.Width > 0 ? $"   |   selection {sel.Width}×{sel.Height} at {sel.X},{sel.Y}" : string.Empty;
            string preview = _previewImage != null ? "   |   PREVIEW (not applied)" : string.Empty;
            statusInfo.Text = $"{img}   |   zoom {canvas.Zoom}x{selText}{preview}";
        }

        private void SetTool(CanvasTool tool)
        {
            canvas.Tool = tool;
            if (tool != CanvasTool.Pipette)
            {
                _pickTarget = PickTarget.Paint;
            }
            SyncUi();
        }

        private void SetPrimary(Color c)
        {
            canvas.PrimaryColor = c;
            _tsColor1.Image?.Dispose();
            _tsColor1.Image = Swatch(c);
        }

        private void SetSecondary(Color c)
        {
            canvas.SecondaryColor = c;
            _tsColor2.Image?.Dispose();
            _tsColor2.Image = Swatch(c);
        }

        private void SwapColors()
        {
            Color a = canvas.PrimaryColor;
            SetPrimary(canvas.SecondaryColor);
            SetSecondary(a);
        }

        private void ToggleOnlySelection()
        {
            _onlySelection = !_onlySelection;
            SyncUi();
            SchedulePreviewRefresh();
        }

        private void ToggleProtect()
        {
            _protectBlackWhite = !_protectBlackWhite;
            SyncUi();
            SchedulePreviewRefresh();
        }

        private void ToggleGrid()
        {
            canvas.ShowGrid = !canvas.ShowGrid;
            canvas.Invalidate();
            SyncUi();
        }

        private void ToggleTiled()
        {
            canvas.TiledView = !canvas.TiledView;
            canvas.RefreshView();
            CenterCanvas();
            SyncUi();
            SetMessage(canvas.TiledView ? "Tiled view: painting is off. Ctrl+T switches back." : "Normal view.");
        }

        #endregion

        #region Tabs – building

        // Tiles tab
        private ComboBox _comboTileSize, _comboQuality, _comboLandToTex;
        private NumericUpDown _numTileW, _numTileH, _numOffX, _numOffY, _numResizeW, _numResizeH, _numSeamless;
        private CheckBox _chkAllowTiny, _chkShowGrid, _chkPad, _chkSkipEmpty, _chkTexSkipUsed, _chkKeepAspect, _chkBlackOutside;
        private TextBox _txtTexStart, _txtArtLandId;

        // Colors tab
        private Label _lblPreview;
        private Button _btnApplyPreview, _btnCancelPreview;
        private TrackBar _tbBright, _tbContrast, _tbGamma, _tbSat, _tbHue;
        private Label _lblBright, _lblContrast, _lblGamma, _lblSat, _lblHue;
        private CheckBox _chkGray, _chkDarken;
        private NumericUpDown _numSharpen, _numWhite, _numPosterize;
        private ComboBox _comboModes;
        private Button _btnParticleColor;
        private RadioButton _radioParticleShaded;
        private Color _particleColor = Color.FromArgb(0xF4, 0x56, 0xEA);

        // Replace tab
        private Button _btnFrom, _btnTo;
        private TextBox _txtFrom, _txtTo;
        private NumericUpDown _numTolerance;
        private CheckBox _chkKeepShading, _chkListSkipBW, _chkEraseWhite;
        private ListView _listColors;
        private Label _lblColorCount;
        private Color _fromColor = Color.White, _toColor = Color.Black;

        // Layers tab
        private CheckedListBox _listLayers;
        private TrackBar _tbOpacity;
        private Label _lblOpacity;
        private NumericUpDown _numOvScale, _numOvX, _numOvY;
        private CheckBox _chkOvTransparent;
        private RadioButton _radioOvArtLand, _radioFillTile;
        private TextBox _txtOvArtId;

        // Blend tab
        private PictureBox _picBlendB;
        private Label _lblBlendB;
        private ComboBox _comboShape;
        private NumericUpDown _numSoftness, _numRoughness, _numSeed;
        private TextBox _txtBlendTexId;

        // Palette
        private FlowLayoutPanel _paletteFlow;
        private Label _lblPalette;
        private NumericUpDown _numHue;

        // texmaps tab
        private ComboBox _comboTexFilter;
        private TextBox _txtTexId;
        private ArtTileGrid _gridTex;
        private Panel _panelTexPreview;
        private Label _lblTexInfo;

        private static FlowLayoutPanel Row(params Control[] controls)
        {
            var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 1, 0, 1) };
            row.Controls.AddRange(controls);
            return row;
        }

        private static Label Lbl(string text, int width = 0) =>
            width > 0
                ? new Label { Text = text, Width = width, Margin = new Padding(3, 7, 3, 0) }
                : new Label { Text = text, AutoSize = true, Margin = new Padding(3, 7, 3, 0) };

        private Button Btn(string text, EventHandler click, string tip = null, int width = 0)
        {
            var b = new Button { Text = text, AutoSize = width == 0, Height = 26, UseVisualStyleBackColor = true, Margin = new Padding(2) };
            if (width > 0)
            {
                b.Width = width;
            }
            b.Click += click;
            if (tip != null)
            {
                toolTip1.SetToolTip(b, tip);
            }
            return b;
        }

        private static NumericUpDown Num(int min, int max, int value, int width = 60) =>
            new NumericUpDown { Minimum = min, Maximum = max, Value = value, Width = width, Margin = new Padding(3, 3, 3, 0) };

        private static CheckBox Chk(string text, bool value = false) =>
            new CheckBox { Text = text, AutoSize = true, Checked = value, Margin = new Padding(3, 5, 3, 0) };

        private static GroupBox Group(string title, params Control[] rows)
        {
            var box = new GroupBox { Text = title, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(6, 4, 6, 6), Margin = new Padding(3, 3, 3, 6) };
            var flow = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Dock = DockStyle.Fill };
            flow.Controls.AddRange(rows);
            box.Controls.Add(flow);
            return box;
        }

        private static void FillTab(TabPage page, params Control[] groups)
        {
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
            flow.Controls.AddRange(groups);
            page.Controls.Add(flow);
        }

        private void BuildTabs()
        {
            BuildTilesTab();
            BuildColorsTab();
            BuildReplaceTab();
            BuildOverlayTab();
            BuildBlendTab();
            BuildTexmapsTab();
        }

        private void BuildTilesTab()
        {
            _comboTileSize = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };
            _comboTileSize.Items.AddRange(new object[] { "33 × 33", "44 × 44", "64 × 64", "128 × 128", "256 × 256", "Custom" });
            _numTileW = Num(6, 4096, 64);
            _numTileH = Num(6, 4096, 64);
            _numOffX = Num(0, 4095, 0);
            _numOffY = Num(0, 4095, 0);
            _chkAllowTiny = Chk("Allow very small tiles (1–5 px)");
            _chkShowGrid = Chk("Show tile grid");
            _chkPad = Chk("Also cut the incomplete tiles at the edge");
            _chkSkipEmpty = Chk("Skip empty (transparent) tiles", true);
            _txtTexStart = new TextBox { Width = 80, Margin = new Padding(3, 4, 3, 0) };
            _chkTexSkipUsed = Chk("Skip used IDs", true);

            _comboTileSize.SelectedIndexChanged += (s, e) =>
            {
                int[] sizes = { 33, 44, 64, 128, 256 };
                if (_comboTileSize.SelectedIndex >= 0 && _comboTileSize.SelectedIndex < sizes.Length)
                {
                    _updating = true;
                    int v = sizes[_comboTileSize.SelectedIndex];
                    _numTileW.Value = v;
                    _numTileH.Value = v;
                    _updating = false;
                }
                UpdateGrid();
            };
            EventHandler customSize = (s, e) =>
            {
                if (_updating)
                {
                    return;
                }
                _comboTileSize.SelectedIndex = 5;
                UpdateGrid();
            };
            _numTileW.ValueChanged += customSize;
            _numTileH.ValueChanged += customSize;
            _numOffX.ValueChanged += (s, e) => UpdateGrid();
            _numOffY.ValueChanged += (s, e) => UpdateGrid();
            _chkAllowTiny.CheckedChanged += (s, e) =>
            {
                _numTileW.Minimum = _numTileH.Minimum = _chkAllowTiny.Checked ? 1 : 6;
            };
            _chkShowGrid.CheckedChanged += (s, e) =>
            {
                if (!_updating && _chkShowGrid.Checked != canvas.ShowGrid)
                {
                    ToggleGrid();
                }
            };
            _comboTileSize.SelectedIndex = 2;

            toolTip1.SetToolTip(_chkAllowTiny, Tip("Tiles of 1–5 px create very many files. Only on purpose.", "Kacheln von 1–5 px erzeugen sehr viele Dateien. Nur mit Absicht."));
            toolTip1.SetToolTip(_chkPad, Tip("Off: only complete tiles. On: tiles at the right / bottom edge are filled up with transparency.",
                "Aus: nur vollständige Kacheln. An: Kacheln am rechten / unteren Rand werden mit Transparenz aufgefüllt."));
            toolTip1.SetToolTip(_txtTexStart, Tip("First texture ID (hex 0x1A2B or decimal).", "Erste Textur-ID (hex 0x1A2B oder dezimal)."));

            GroupBox tiles = Group("Tile size / cut",
                Row(Lbl("Size:", 45), _comboTileSize),
                Row(Lbl("W:", 45), _numTileW, Lbl("H:"), _numTileH),
                Row(Lbl("Offset:", 45), _numOffX, Lbl("Y:"), _numOffY),
                _chkAllowTiny, _chkShowGrid, _chkPad, _chkSkipEmpty,
                Row(Btn("Cut tiles → files", (s, e) => CutTilesToFiles(), Tip("Saves every tile into a new folder in tempGrafic.", "Speichert jede Kachel in einen neuen Ordner in tempGrafic.")),
                    Btn("Crop to selection", (s, e) => CropToSelection())),
                Row(Lbl("texmaps from ID:"), _txtTexStart, Btn("Free block", (s, e) => FindFreeTextureBlock())),
                _chkTexSkipUsed,
                Row(Btn("Cut tiles → texmaps", (s, e) => CutTilesToTexmaps(), Tip("Writes all tiles (64 or 128) into texmaps.mul from the start ID on.", "Schreibt alle Kacheln (64 oder 128) ab der Start-ID in texmaps.mul."))));

            _numResizeW = Num(1, 4096, 64);
            _numResizeH = Num(1, 4096, 64);
            _chkKeepAspect = Chk("Keep aspect ratio");
            _comboQuality = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };
            _comboQuality.Items.AddRange(new object[] { "Pixel (hard)", "Smooth", "Smooth + sharpen" });
            _comboQuality.SelectedIndex = 1;
            _numResizeW.ValueChanged += (s, e) => KeepAspect(true);
            _numResizeH.ValueChanged += (s, e) => KeepAspect(false);
            GroupBox size = Group("Size (old \"Resolution\")",
                Row(Lbl("W:", 25), _numResizeW, Lbl("H:"), _numResizeH),
                Row(_chkKeepAspect, _comboQuality),
                Row(Btn("Resize", (s, e) => ResizeImage(-1)), Btn("44", (s, e) => ResizeImage(44)), Btn("64", (s, e) => ResizeImage(64)),
                    Btn("128", (s, e) => ResizeImage(128)), Btn("256", (s, e) => ResizeImage(256))));

            _chkBlackOutside = Chk("Black outside the diamond", true);
            _comboLandToTex = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 70 };
            _comboLandToTex.Items.AddRange(new object[] { "64", "128" });
            _comboLandToTex.SelectedIndex = 0;
            _txtArtLandId = new TextBox { Width = 80, Margin = new Padding(3, 4, 3, 0) };
            toolTip1.SetToolTip(_txtArtLandId, Tip("Land tile ID in art.mul (hex 0x1A2B or decimal).", "Landkachel-ID in art.mul (hex 0x1A2B oder dezimal)."));
            GroupBox land = Group("UO land tile 44 × 44",
                Row(Btn("Texture → land tile", (s, e) => MakeLandTile(), Tip("Turns the square texture into the 44 × 44 diamond of a land tile (exact, smooth).",
                        "Macht aus der quadratischen Textur die 44 × 44 Raute einer Landkachel (genau, weich).")),
                    Btn("Old way (33 px, 45°)", (s, e) => ApplyFilter("Old land tile", TextureImaging.OldRotatedTile, true))),
                _chkBlackOutside,
                Row(Btn("Land tile → texture", (s, e) => LandTileToTexture(), Tip("The way back: 44 × 44 land tile → square texture.", "Der Rückweg: 44 × 44 Landkachel → quadratische Textur.")),
                    _comboLandToTex),
                Row(Lbl("art.mul land ID:"), _txtArtLandId),
                Row(Btn("Load land tile", (s, e) => LoadArtLand()), Btn("Write land tile", (s, e) => WriteArtLand())));

            _numSeamless = Num(10, 100, 50);
            toolTip1.SetToolTip(_numSeamless, Tip("How wide the blended border is (% of the image).", "Wie breit der überblendete Rand ist (% des Bildes)."));
            GroupBox repeat = Group("Repeat / seams",
                Row(Btn("Make seamless", (s, e) => MakeSeamless()), Lbl("Blend %:"), _numSeamless),
                Row(Btn("Tiled view 3 × 3", (s, e) => ToggleTiled())));

            FillTab(tabTiles, tiles, size, land, repeat);
        }

        private (TrackBar Bar, Label Label) Slider(int min, int max, string name)
        {
            var bar = new TrackBar { Minimum = min, Maximum = max, Value = 0, TickStyle = TickStyle.None, Width = 200, AutoSize = false, Height = 26 };
            var label = new Label { Width = 110, Margin = new Padding(3, 6, 3, 0), Text = $"{name}: 0" };
            bar.ValueChanged += (s, e) =>
            {
                label.Text = $"{name}: {bar.Value}";
                if (!_resettingPreviewControls)
                {
                    SchedulePreview(PreviewKind.Adjust);
                }
            };
            return (bar, label);
        }

        private void BuildColorsTab()
        {
            _lblPreview = new Label { Text = "No preview.", AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 7, 3, 0) };
            _btnApplyPreview = Btn("Apply", (s, e) => CommitPreview(true), Tip("Applies the preview to the image (Enter).", "Übernimmt die Vorschau ins Bild (Enter)."));
            _btnCancelPreview = Btn("Cancel", (s, e) => CancelPreview(), Tip("Throws the preview away.", "Verwirft die Vorschau."));
            GroupBox preview = Group("Preview", Row(_btnApplyPreview, _btnCancelPreview, _lblPreview));

            (_tbBright, _lblBright) = Slider(-100, 100, "Brightness");
            (_tbContrast, _lblContrast) = Slider(-100, 100, "Contrast");
            (_tbGamma, _lblGamma) = Slider(-90, 300, "Gamma");
            (_tbSat, _lblSat) = Slider(-100, 100, "Saturation");
            (_tbHue, _lblHue) = Slider(-180, 180, "Hue");
            _chkGray = Chk("Grayscale");
            _chkGray.CheckedChanged += (s, e) =>
            {
                if (!_resettingPreviewControls)
                {
                    SchedulePreview(PreviewKind.Adjust);
                }
            };
            GroupBox adjust = Group("Adjust (old \"Modus\" / \"Optimize\")",
                Row(_lblBright, _tbBright), Row(_lblContrast, _tbContrast), Row(_lblGamma, _tbGamma), Row(_lblSat, _tbSat), Row(_lblHue, _tbHue),
                Row(_chkGray, Btn("Reset sliders", (s, e) => CancelPreview())));

            _numSharpen = Num(1, 100, 30);
            _numWhite = Num(1, 100, 10);
            _chkDarken = Chk("Darken");
            _numPosterize = Num(2, 64, 8);
            GroupBox filters = Group("Filters",
                Row(Btn("Sharpen", (s, e) => ApplyFilter("Sharpen", b => TextureImaging.Sharpen(b, (int)_numSharpen.Value, _protectBlackWhite)), null, 110), Lbl("Amount %:"), _numSharpen),
                Row(Btn("White balance", (s, e) => ApplyFilter("White balance", b => TextureImaging.WhiteBalance(b, (int)_numWhite.Value, _chkDarken.Checked)), null, 110), Lbl("%:"), _numWhite, _chkDarken),
                Row(Btn("Color enhance", (s, e) => ApplyFilter("Color enhancement", b => TextureImaging.AutoEnhance(b, _protectBlackWhite)), Tip("More saturation and a little contrast.", "Mehr Sättigung und etwas Kontrast."), 110),
                    Btn("Normalize", (s, e) => ApplyFilter("Normalize", b => TextureImaging.Normalize(b, _protectBlackWhite)), Tip("Stretches the colors to the full range.", "Streckt die Farben auf den vollen Bereich."), 110)),
                Row(Btn("Invert", (s, e) => ApplyFilter("Invert", b => TextureImaging.Invert(b, _protectBlackWhite)), null, 110),
                    Btn("UO 16-bit", (s, e) => ApplyFilter("UO 16-bit colors", TextureImaging.To16Bit), Tip("Shows the colors as UO stores them (old \"indexed\").", "Zeigt die Farben so, wie UO sie speichert (früher \"indexed\")."), 110)),
                Row(Btn("Posterize", (s, e) => ApplyFilter("Posterize", b => TextureImaging.Posterize(b, (int)_numPosterize.Value, _protectBlackWhite)), null, 110), Lbl("Levels:"), _numPosterize));

            _comboModes = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 210 };
            _comboModes.Items.AddRange(TextureImaging.ColorModes);
            _comboModes.SelectedIndexChanged += (s, e) =>
            {
                if (!_resettingPreviewControls && _comboModes.SelectedIndex >= 0)
                {
                    SchedulePreview(PreviewKind.Mode);
                }
            };
            GroupBox modes = Group("Color modes (old R1 / R2)",
                Row(_comboModes),
                Row(Btn("◀ Previous", (s, e) => StepMode(-1)), Btn("Next ▶", (s, e) => StepMode(1)), Lbl("then Apply")));

            _btnParticleColor = new Button { Width = 90, Height = 26, FlatStyle = FlatStyle.Flat, Margin = new Padding(2) };
            PaintSwatchButton(_btnParticleColor, _particleColor);
            _btnParticleColor.Click += (s, e) =>
            {
                Color? c = ChooseColor(_particleColor);
                if (c.HasValue)
                {
                    _particleColor = c.Value;
                    PaintSwatchButton(_btnParticleColor, _particleColor);
                    if (_previewKind == PreviewKind.Particle)
                    {
                        SchedulePreview(PreviewKind.Particle);
                    }
                }
            };
            var flat = new RadioButton { Text = "Flat", AutoSize = true, Margin = new Padding(3, 6, 3, 0) };
            _radioParticleShaded = new RadioButton { Text = "With shadow", AutoSize = true, Checked = true, Margin = new Padding(3, 6, 3, 0) };
            _radioParticleShaded.CheckedChanged += (s, e) =>
            {
                if (_previewKind == PreviewKind.Particle)
                {
                    SchedulePreview(PreviewKind.Particle);
                }
            };
            GroupBox particle = Group("Particle grey",
                Row(Lbl("Color:"), _btnParticleColor, flat, _radioParticleShaded),
                Row(Btn("Show preview", (s, e) => SchedulePreview(PreviewKind.Particle), Tip("All gray pixels (#030303 – #FBFBFB) get the color. With shadow keeps their brightness.",
                    "Alle grauen Pixel (#030303 – #FBFBFB) bekommen die Farbe. Mit Schatten behält ihre Helligkeit."))));

            FillTab(tabColors, preview, adjust, filters, modes, particle);
        }

        private Button SwatchButton(Color c, Action<Color> set)
        {
            var b = new Button { Width = 80, Height = 26, FlatStyle = FlatStyle.Flat, Margin = new Padding(2) };
            PaintSwatchButton(b, c);
            b.Click += (s, e) =>
            {
                Color? chosen = ChooseColor(b.BackColor);
                if (chosen.HasValue)
                {
                    set(chosen.Value);
                }
            };
            return b;
        }

        private void BuildReplaceTab()
        {
            _btnFrom = SwatchButton(_fromColor, SetFromColor);
            _btnTo = SwatchButton(_toColor, SetToColor);
            _txtFrom = new TextBox { Width = 70, Text = Hex(_fromColor), Margin = new Padding(3, 4, 3, 0) };
            _txtTo = new TextBox { Width = 70, Text = Hex(_toColor), Margin = new Padding(3, 4, 3, 0) };
            _txtFrom.TextChanged += (s, e) => HexChanged(_txtFrom, SetFromColor);
            _txtTo.TextChanged += (s, e) => HexChanged(_txtTo, SetToColor);
            var presets = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90 };
            presets.Items.AddRange(new object[] { "#FFFFFF", "#000000", "#F456EA", "#FDE2CD" });
            presets.SelectedIndexChanged += (s, e) =>
            {
                if (presets.SelectedItem is string hex && TryParseHex(hex, out Color c))
                {
                    SetFromColor(c);
                }
            };
            _numTolerance = Num(0, 100, 0);
            _chkKeepShading = Chk("Keep shading");
            _chkEraseWhite = Chk("Eraser tool erases to white");
            _chkEraseWhite.CheckedChanged += (s, e) => canvas.EraseToWhite = _chkEraseWhite.Checked;
            _numTolerance.ValueChanged += (s, e) => canvas.FillTolerance = (int)Math.Round((int)_numTolerance.Value * 2.55);
            toolTip1.SetToolTip(_numTolerance, Tip("How different a color may be and still count (0 = exact, 100 = all). Also for the fill tool.",
                "Wie stark eine Farbe abweichen darf (0 = genau, 100 = alle). Gilt auch fürs Füllwerkzeug."));
            toolTip1.SetToolTip(_chkKeepShading, Tip("The new color keeps the light and dark of the old pixels.", "Die neue Farbe behält Hell und Dunkel der alten Pixel."));

            GroupBox replace = Group("Replace color",
                Row(Lbl("From:", 40), _btnFrom, _txtFrom, Btn("Pick", (s, e) => StartPick(PickTarget.From), Tip("Click a pixel in the image.", "Pixel im Bild anklicken."))),
                Row(Lbl("To:", 40), _btnTo, _txtTo, Btn("Pick", (s, e) => StartPick(PickTarget.To))),
                Row(Lbl("Preset:", 40), presets, Btn("⇄ Swap", (s, e) => { Color a = _fromColor; SetFromColor(_toColor); SetToColor(a); })),
                Row(Lbl("Tolerance %:"), _numTolerance, _chkKeepShading),
                Row(Btn("Replace", (s, e) => ReplaceColor(), null, 100), Btn("Erase → transparent", (s, e) => EraseColor(false)), Btn("→ white", (s, e) => EraseColor(true))),
                _chkEraseWhite);

            _listColors = new ListView { View = View.Details, FullRowSelect = true, Width = 330, Height = 300, OwnerDraw = true, HideSelection = false };
            _listColors.Columns.Add("", 30);
            _listColors.Columns.Add("Color", 90);
            _listColors.Columns.Add("Pixels", 80);
            _listColors.Columns.Add("%", 70);
            _listColors.DrawColumnHeader += (s, e) => e.DrawDefault = true;
            _listColors.DrawSubItem += ListColors_DrawSubItem;
            _listColors.ItemActivate += (s, e) =>
            {
                if (_listColors.SelectedItems.Count > 0 && _listColors.SelectedItems[0].Tag is Color c)
                {
                    Clipboard.SetText(Hex(c));
                    SetMessage(Hex(c) + " copied to the clipboard.");
                }
            };
            _listColors.SelectedIndexChanged += (s, e) =>
            {
                if (_listColors.SelectedItems.Count > 0 && _listColors.SelectedItems[0].Tag is Color c)
                {
                    SetFromColor(c);
                }
            };
            _chkListSkipBW = Chk("Without black / white", true);
            _lblColorCount = new Label { AutoSize = true, Margin = new Padding(3, 7, 3, 0) };
            toolTip1.SetToolTip(_listColors, Tip("Click = use as \"From\" color. Double click = copy the hex code.", "Klick = als \"From\"-Farbe nehmen. Doppelklick = Hex-Code kopieren."));
            GroupBox list = Group("Colors in the image (old \"Evaluate\" / \"Color List\")",
                Row(Btn("Refresh", (s, e) => RefreshColorList()), _chkListSkipBW, _lblColorCount),
                _listColors);

            FillTab(tabReplace, BuildPaletteGroup(), replace, list);
        }

        private void BuildOverlayTab()
        {
            _listLayers = new CheckedListBox { Width = 330, Height = 120, CheckOnClick = false, IntegralHeight = false };
            _listLayers.SelectedIndexChanged += ListLayers_SelectedIndexChanged;
            _listLayers.ItemCheck += ListLayers_ItemCheck;
            toolTip1.SetToolTip(_listLayers, Tip("Top layer first. Checkbox = visible. Selected = the layer you move and change.",
                "Oberste Ebene zuerst. Häkchen = sichtbar. Ausgewählt = die Ebene, die du verschiebst und änderst."));

            _radioOvArtLand = new RadioButton { Text = "Land", AutoSize = true, Margin = new Padding(3, 6, 3, 0) };
            var items = new RadioButton { Text = "Item", AutoSize = true, Checked = true, Margin = new Padding(3, 6, 3, 0) };
            _txtOvArtId = new TextBox { Width = 70, Margin = new Padding(3, 4, 3, 0) };
            GroupBox add = Group("Layers (graphics on top, stay movable)",
                _listLayers,
                Row(Btn("+ File...", (s, e) => LayerFromFile()), Btn("+ Clipboard", (s, e) => LayerFromClipboard()), Btn("+ Texture (texmaps)", (s, e) => LayerFromTexture())),
                Row(Lbl("art.mul:"), items, _radioOvArtLand, _txtOvArtId, Btn("+ Add", (s, e) => LayerFromArt())),
                Row(Btn("▲", (s, e) => MoveLayer(1), Tip("Layer up", "Ebene nach oben")), Btn("▼", (s, e) => MoveLayer(-1), Tip("Layer down", "Ebene nach unten")),
                    Btn("Duplicate", (s, e) => DuplicateLayer()), Btn("Remove", (s, e) => RemoveLayer(ActiveLayer))),
                Row(Btn("Burn in selected", (s, e) => BurnLayers(false), Tip("Draw the selected layer into the image and remove the layer.", "Ausgewählte Ebene ins Bild zeichnen und die Ebene entfernen.")),
                    Btn("Burn in all", (s, e) => BurnLayers(true))));

            _chkOvTransparent = Chk("Black / white transparent", true);
            _chkOvTransparent.CheckedChanged += (s, e) =>
            {
                if (!_updating && ActiveLayer != null)
                {
                    ActiveLayer.BlackWhiteTransparent = _chkOvTransparent.Checked;
                    canvas.Invalidate();
                    NotifyTerrain();
                }
            };
            _tbOpacity = new TrackBar { Minimum = 0, Maximum = 100, Value = 100, TickStyle = TickStyle.None, Width = 180, AutoSize = false, Height = 26 };
            _lblOpacity = new Label { Text = "Opacity: 100 %", Width = 100, Margin = new Padding(3, 6, 3, 0) };
            _tbOpacity.ValueChanged += (s, e) =>
            {
                _lblOpacity.Text = $"Opacity: {_tbOpacity.Value} %";
                if (!_updating && ActiveLayer != null)
                {
                    ActiveLayer.Opacity = _tbOpacity.Value;
                    canvas.Invalidate();
                    NotifyTerrain();
                }
            };
            _numOvScale = Num(5, 1600, 100);
            _numOvScale.ValueChanged += (s, e) =>
            {
                if (!_updating && ActiveLayer != null)
                {
                    ActiveLayer.Scale = (int)_numOvScale.Value;
                    canvas.Invalidate();
                    NotifyTerrain();
                }
            };
            _numOvX = Num(-4096, 4096, 0);
            _numOvY = Num(-4096, 4096, 0);
            EventHandler pos = (s, e) =>
            {
                if (_updating || ActiveLayer == null)
                {
                    return;
                }
                ActiveLayer.Position = new Point((int)_numOvX.Value, (int)_numOvY.Value);
                canvas.Invalidate();
                NotifyTerrain();
            };
            _numOvX.ValueChanged += pos;
            _numOvY.ValueChanged += pos;
            GroupBox place = Group("Selected layer",
                Row(_lblOpacity, _tbOpacity),
                Row(Lbl("Scale %:", 60), _numOvScale, Btn("Fit", (s, e) => FitLayer(), Tip("Scale the layer to the image size.", "Ebene auf Bildgröße skalieren."))),
                Row(Lbl("X:", 20), _numOvX, Lbl("Y:"), _numOvY, Btn("Center", (s, e) => CenterLayer())),
                Row(_chkOvTransparent, Btn("Move tool", (s, e) => SetTool(CanvasTool.Overlay))));

            _radioFillTile = new RadioButton { Text = "Repeat (tile)", AutoSize = true, Checked = true, Margin = new Padding(3, 6, 3, 0) };
            var stretch = new RadioButton { Text = "Stretch", AutoSize = true, Margin = new Padding(3, 6, 3, 0) };
            GroupBox fill = Group("Fill the selection (old \"Fill Texture\")",
                Row(_radioFillTile, stretch),
                Row(Btn("With image file...", (s, e) => FillSelectionFromFile()), Btn("With selected layer", (s, e) => FillSelection(ActiveLayer?.Image))));

            FillTab(tabOverlay, add, place, fill);
            LoadLayerProperties();
        }

        private void BuildBlendTab()
        {
            _picBlendB = new PictureBox { Width = 64, Height = 64, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, Margin = new Padding(3) };
            _lblBlendB = new Label { Text = "B: none", AutoSize = true, Margin = new Padding(3, 7, 3, 0) };
            GroupBox source = Group("Texture B (A = the edited image)",
                Row(_picBlendB, _lblBlendB),
                Row(Btn("From texmaps (selected)", (s, e) => SetBlendB(_gridTex.SelectedItem >= 0 ? TextureMul.GetCopy(_gridTex.SelectedItem) : null, $"texture 0x{_gridTex.SelectedItem:X4}")),
                    Btn("From layer", (s, e) => SetBlendB(ActiveLayer == null ? null : CutterImaging.To32Bpp(ActiveLayer.Source), ActiveLayer?.Name ?? "layer"))),
                Row(Btn("From file...", (s, e) =>
                    {
                        using (var d = new OpenFileDialog { Filter = CutterImaging.ImageFilter })
                        {
                            if (d.ShowDialog(this) == DialogResult.OK)
                            {
                                SetBlendB(CutterImaging.LoadBitmap(d.FileName), Path.GetFileNameWithoutExtension(d.FileName));
                            }
                        }
                    }),
                    Btn("From clipboard", (s, e) => SetBlendB(ReadClipboard(), "clipboard"))));

            _comboShape = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 170 };
            _comboShape.Items.AddRange(TextureBlend.ShapeNames);
            _comboShape.SelectedIndex = 0;
            _numSoftness = Num(0, 100, 35);
            _numRoughness = Num(0, 100, 40);
            _numSeed = Num(0, 99999, 1, 70);
            EventHandler changed = (s, e) =>
            {
                if (_previewKind == PreviewKind.Transition)
                {
                    SchedulePreview(PreviewKind.Transition);
                }
            };
            _comboShape.SelectedIndexChanged += changed;
            _numSoftness.ValueChanged += changed;
            _numRoughness.ValueChanged += changed;
            _numSeed.ValueChanged += changed;
            toolTip1.SetToolTip(_numSoftness, Tip("Width of the soft border (0 = hard edge).", "Breite des weichen Übergangs (0 = harte Kante)."));
            toolTip1.SetToolTip(_numRoughness, Tip("How much the border wanders (natural, irregular edge).", "Wie stark die Kante wandert (natürlich, unregelmäßig)."));
            toolTip1.SetToolTip(_numSeed, Tip("Other number = other pattern of the irregular edge.", "Andere Zahl = anderes Muster der unregelmäßigen Kante."));
            _txtBlendTexId = new TextBox { Width = 80, Margin = new Padding(3, 4, 3, 0) };
            GroupBox shape = Group("Transition A → B",
                Row(Lbl("Shape:", 70), _comboShape),
                Row(Lbl("Softness:", 70), _numSoftness, Lbl("Rough:"), _numRoughness),
                Row(Lbl("Pattern:", 70), _numSeed, Btn("New", (s, e) => _numSeed.Value = new Random().Next(0, 99999))),
                Row(Btn("Show preview", (s, e) => ShowTransitionPreview()), Btn("Apply", (s, e) => CommitPreview(true)), Btn("Cancel", (s, e) => CancelPreview())),
                Row(Btn("All 9 → files", (s, e) => AllTransitions(false), Tip("Saves all nine shapes (edges, diagonals, corners, island).", "Speichert alle neun Formen (Kanten, Diagonalen, Ecken, Insel)."))),
                Row(Btn("All 9 → texmaps from", (s, e) => AllTransitions(true)), _txtBlendTexId),
                Row(Btn("Check in terrain preview", (s, e) => OpenTerrainPreview(), Tip("Shows the result on the ground next to other textures.", "Zeigt das Ergebnis auf dem Boden neben anderen Texturen."))));

            FillTab(tabBlend, source, shape);
        }

        private GroupBox BuildPaletteGroup()
        {
            _paletteFlow = new FlowLayoutPanel { Width = 330, Height = 66, AutoScroll = true, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(3) };
            _lblPalette = new Label { AutoSize = true, Margin = new Padding(3, 7, 3, 0) };
            _numHue = Num(1, 3000, 1, 60);
            toolTip1.SetToolTip(_numHue, Tip("UO hue number from hues.mul (32 colors each).", "UO-Hue-Nummer aus hues.mul (je 32 Farben)."));
            return Group("Palette",
                _paletteFlow,
                Row(Btn("+ Color 1", (s, e) => { _palette.Add(canvas.PrimaryColor); RefreshPalette(); }),
                    Btn("+ From image", (s, e) => PaletteFromImage(), Tip("The 16 most common colors of the image.", "Die 16 häufigsten Farben des Bildes.")),
                    Btn("Clear", (s, e) => { _palette.Colors.Clear(); RefreshPalette(); }), _lblPalette),
                Row(Lbl("UO hue:"), _numHue, Btn("+ Hue", (s, e) => PaletteFromHue()), Btn("Load...", (s, e) => LoadPalette()), Btn("Save...", (s, e) => SavePalette())));
        }

        private void BuildTexmapsTab()
        {
            _comboTexFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 70 };
            _comboTexFilter.Items.AddRange(new object[] { "All", "Used", "Free" });
            _comboTexFilter.SelectedIndex = 1;
            _comboTexFilter.SelectedIndexChanged += (s, e) => RebuildTextureList();
            _txtTexId = new TextBox { Width = 70, Margin = new Padding(3, 4, 3, 0) };
            _txtTexId.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    GoToTexture();
                }
            };
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 34 };
            top.Controls.AddRange(new Control[] { _comboTexFilter, Lbl("ID:"), _txtTexId, Btn("Go", (s, e) => GoToTexture()), Btn("Next free", (s, e) => NextFreeTexture()) });

            _gridTex = new ArtTileGrid { Dock = DockStyle.Fill, Text = "texmaps.mul is not loaded.\nLoad the client files in UoFiddler." };
            _gridTex.ImageProvider = id => TextureMul.GetShared(id);
            _gridTex.LabelProvider = id => $"0x{id:X4}";
            _gridTex.IsMarked = id => _usedTextures != null && !_usedTextures[id];
            _gridTex.SelectionChanged += (s, e) => UpdateTexturePreview();
            _gridTex.ItemActivated += (s, e) => TextureToEditor();

            _panelTexPreview = new Panel { Dock = DockStyle.Top, Height = 140, BackColor = Color.FromArgb(45, 45, 48) };
            _panelTexPreview.Paint += PanelTexPreview_Paint;
            _lblTexInfo = new Label { Dock = DockStyle.Top, Height = 34, TextAlign = ContentAlignment.MiddleLeft };

            var buttons = new TableLayoutPanel { Dock = DockStyle.Bottom, ColumnCount = 2, RowCount = 5, Height = 5 * 32 + 4 };
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int i = 0; i < 5; i++)
            {
                buttons.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            }
            Button B(string text, EventHandler click, string tip)
            {
                Button b = Btn(text, click, tip);
                b.AutoSize = false;
                b.Dock = DockStyle.Fill;
                return b;
            }
            buttons.Controls.Add(B("→ Editor", (s, e) => TextureToEditor(), Tip("Load the texture into the editor (double click).", "Textur in den Editor laden (Doppelklick).")), 0, 0);
            buttons.Controls.Add(B("→ Layer", (s, e) => LayerFromTexture(), Tip("Show the texture as a layer over the image (e.g. to compare).", "Textur als Ebene über dem Bild einblenden (z. B. zum Vergleichen).")), 1, 0);
            buttons.Controls.Add(B("Image → ID", (s, e) => ImageToTexture(), Tip("Write the edited image into the selected ID (64 or 128).", "Bearbeitetes Bild in die gewählte ID schreiben (64 oder 128).")), 0, 1);
            buttons.Controls.Add(B("Remove ID", (s, e) => RemoveTexture(), Tip("Remove the texture of the selected ID.", "Textur der gewählten ID entfernen.")), 1, 1);
            buttons.Controls.Add(B("Copy texture", (s, e) => CopyTexture(), Tip("Copy the selected texture to the clipboard.", "Gewählte Textur in die Zwischenablage kopieren.")), 0, 2);
            buttons.Controls.Add(B("Reload list", (s, e) => ReloadTextures(), Tip("Read the used / free IDs again.", "Belegte / freie IDs neu einlesen.")), 1, 2);
            buttons.Controls.Add(B("Image → its land tiles", (s, e) => ImageToLandTilesOfTexture(), Tip("Turns the image into a 44 × 44 land tile and writes it into every land tile that uses the selected texture (tiledata).",
                "Macht aus dem Bild eine 44 × 44 Landkachel und schreibt sie in jede Landkachel, die laut tiledata die gewählte Textur benutzt.")), 0, 3);
            buttons.Controls.Add(B("Terrain preview", (s, e) => OpenTerrainPreview(), Tip("See the image on the ground (2D / 3D).", "Das Bild auf dem Boden ansehen (2D / 3D).")), 1, 3);
            Button save = B("Save texmaps.mul", (s, e) => SaveTexmaps(), Tip("Save texidx.mul and texmaps.mul into the UoFiddler output folder.", "texidx.mul und texmaps.mul in den UoFiddler-Ausgabeordner speichern."));
            buttons.Controls.Add(save, 0, 4);
            buttons.SetColumnSpan(save, 2);

            tabTexmaps.Controls.Add(_gridTex);
            tabTexmaps.Controls.Add(_lblTexInfo);
            tabTexmaps.Controls.Add(_panelTexPreview);
            tabTexmaps.Controls.Add(top);
            tabTexmaps.Controls.Add(buttons);
        }

        #endregion

        #region Image, undo, preview

        private void Canvas_BeginEdit(object sender, EventArgs e)
        {
            CommitPreview(false);
            PushUndo();
        }

        private void PushUndo()
        {
            if (_image == null)
            {
                return;
            }
            _undo.Add(CutterImaging.To32Bpp(_image));
            if (_undo.Count > MaxUndo)
            {
                _undo[0].Dispose();
                _undo.RemoveAt(0);
            }
            foreach (Bitmap b in _redo)
            {
                b.Dispose();
            }
            _redo.Clear();
            SyncUi();
        }

        private void Undo()
        {
            CancelPreview();
            if (_undo.Count == 0)
            {
                return;
            }
            if (_image != null)
            {
                _redo.Add(_image);
            }
            _image = _undo[_undo.Count - 1];
            _undo.RemoveAt(_undo.Count - 1);
            ShowImage();
            SetMessage($"Undo ({_undo.Count} more).");
        }

        private void Redo()
        {
            CancelPreview();
            if (_redo.Count == 0)
            {
                return;
            }
            if (_image != null)
            {
                _undo.Add(_image);
            }
            _image = _redo[_redo.Count - 1];
            _redo.RemoveAt(_redo.Count - 1);
            ShowImage();
            SetMessage($"Redo ({_redo.Count} more).");
        }

        /// <summary>Shows _image on the canvas after it was replaced.</summary>
        private void ShowImage()
        {
            Size old = canvas.Image?.Size ?? Size.Empty;
            canvas.Image = _image;
            if (_image != null && old != _image.Size)
            {
                canvas.ClearSelection();
                UpdateResizeFields();
            }
            CenterCanvas();
            SyncUi();
            UpdateStatus();
            panelCanvas.Invalidate();
            NotifyTerrain();
        }

        /// <summary>Replaces the image (the old one is kept in the undo list by the caller).</summary>
        private void ReplaceImage(Bitmap bmp)
        {
            Bitmap old = _image;
            _image = bmp;
            ShowImage();
            if (old != null && !ReferenceEquals(old, bmp) && !_undo.Contains(old) && !_redo.Contains(old))
            {
                old.Dispose();
            }
        }

        private void LoadImage(Bitmap bmp, string name)
        {
            CancelPreview();
            if (_image != null)
            {
                PushUndo();
            }
            ReplaceImage(bmp);
            canvas.ClearSelection();
            FitZoom();
            UpdateResizeFields();
            SetMessage($"Loaded {name} ({bmp.Width}×{bmp.Height}).");
        }

        /// <summary>Only the selected area takes the change when "only selection" is on.</summary>
        private Bitmap MaybeMergeSelection(Bitmap result)
        {
            if (!_onlySelection || !canvas.HasSelection || result.Size != _image.Size)
            {
                return result;
            }
            Bitmap merged = CutterImaging.To32Bpp(_image);
            using (GraphicsPath path = canvas.GetSelectionPath())
            using (var g = Graphics.FromImage(merged))
            {
                g.SetClip(path);
                g.CompositingMode = CompositingMode.SourceCopy;
                g.DrawImage(result, new Rectangle(0, 0, merged.Width, merged.Height));
            }
            result.Dispose();
            return merged;
        }

        /// <summary>Runs a filter on the image with undo. sizeChanging: no selection merge (rotate, resize, ...).</summary>
        private void ApplyFilter(string name, Func<Bitmap, Bitmap> filter, bool sizeChanging = false)
        {
            if (!CheckImage())
            {
                return;
            }
            CommitPreview(false);
            Bitmap result;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                result = filter(_image);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"{name} failed:\n{ex.Message}", "Texture Cutter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
            if (result == null)
            {
                return;
            }
            if (!sizeChanging)
            {
                result = MaybeMergeSelection(result);
            }
            PushUndo();
            ReplaceImage(result);
            SetMessage($"{name} applied" + (_onlySelection && canvas.HasSelection && !sizeChanging ? " (inside the selection)." : "."));
        }

        private static Bitmap Rotated(Bitmap b, RotateFlipType type)
        {
            Bitmap copy = CutterImaging.To32Bpp(b);
            copy.RotateFlip(type);
            return copy;
        }

        private void SchedulePreview(PreviewKind kind)
        {
            if (_image == null)
            {
                return;
            }
            if (_previewKind != PreviewKind.None && _previewKind != kind)
            {
                ResetPreviewControls(kind);
            }
            _previewKind = kind;
            previewTimer.Stop();
            previewTimer.Start();
        }

        private void SchedulePreviewRefresh()
        {
            if (_previewKind != PreviewKind.None)
            {
                SchedulePreview(_previewKind);
            }
        }

        private ColorAdjust CurrentAdjust() => new ColorAdjust
        {
            Brightness = _tbBright.Value,
            Contrast = _tbContrast.Value,
            Gamma = _tbGamma.Value,
            Saturation = _tbSat.Value,
            Hue = _tbHue.Value,
            Grayscale = _chkGray.Checked
        };

        private void PreviewTimer_Tick(object sender, EventArgs e)
        {
            previewTimer.Stop();
            if (_image == null || _previewKind == PreviewKind.None)
            {
                return;
            }
            Bitmap preview = null;
            switch (_previewKind)
            {
                case PreviewKind.Adjust:
                    ColorAdjust a = CurrentAdjust();
                    if (a.IsNeutral)
                    {
                        CancelPreview();
                        return;
                    }
                    preview = TextureImaging.Adjust(_image, a, _protectBlackWhite);
                    break;
                case PreviewKind.Mode:
                    if (_comboModes.SelectedItem is Class.TextureCutter.ColorMode mode)
                    {
                        preview = TextureImaging.ApplyColorMode(_image, mode, _protectBlackWhite);
                    }
                    break;
                case PreviewKind.Particle:
                    preview = TextureImaging.ParticleGrey(_image, _particleColor, _radioParticleShaded.Checked);
                    break;
                case PreviewKind.Transition:
                    if (_blendB != null)
                    {
                        preview = MakeTransition(_image, (TransitionShape)Math.Max(0, _comboShape.SelectedIndex));
                    }
                    break;
            }
            if (preview == null)
            {
                return;
            }
            preview = MaybeMergeSelection(preview);
            _previewImage?.Dispose();
            _previewImage = preview;
            canvas.Image = _previewImage;
            NotifyTerrain();
            _lblPreview.Text = "Preview – not applied yet.";
            _lblPreview.ForeColor = Color.DarkRed;
            UpdateStatus();
        }

        /// <summary>Puts the preview into the image (with undo).</summary>
        private void CommitPreview(bool showMessage)
        {
            previewTimer.Stop();
            if (_previewImage == null)
            {
                if (_previewKind != PreviewKind.None)
                {
                    ResetPreviewControls(PreviewKind.None);
                }
                return;
            }
            Bitmap result = _previewImage;
            _previewImage = null;
            PushUndo();
            ReplaceImage(result);
            ResetPreviewControls(PreviewKind.None);
            if (showMessage)
            {
                SetMessage("Color change applied.");
            }
        }

        private void CancelPreview()
        {
            previewTimer.Stop();
            if (_previewImage != null)
            {
                _previewImage.Dispose();
                _previewImage = null;
                canvas.Image = _image;
            }
            ResetPreviewControls(PreviewKind.None);
            UpdateStatus();
        }

        /// <summary>Sets the controls of all preview kinds except "keep" back to neutral.</summary>
        private void ResetPreviewControls(PreviewKind keep)
        {
            _resettingPreviewControls = true;
            if (keep != PreviewKind.Adjust)
            {
                _tbBright.Value = _tbContrast.Value = _tbGamma.Value = _tbSat.Value = _tbHue.Value = 0;
                _chkGray.Checked = false;
            }
            if (keep != PreviewKind.Mode)
            {
                _comboModes.SelectedIndex = -1;
            }
            _resettingPreviewControls = false;
            _previewKind = keep;
            if (keep == PreviewKind.None)
            {
                _lblPreview.Text = "No preview.";
                _lblPreview.ForeColor = Color.DimGray;
            }
        }

        private void StepMode(int delta)
        {
            if (!CheckImage())
            {
                return;
            }
            int n = TextureImaging.ColorModes.Length;
            int i = _comboModes.SelectedIndex < 0 ? (delta > 0 ? 0 : n - 1) : (_comboModes.SelectedIndex + delta + n) % n;
            _comboModes.SelectedIndex = i;
        }

        #endregion

        #region Canvas events, zoom

        private void Canvas_HoverPixel(object sender, Point p)
        {
            Bitmap shown = canvas.Image;
            if (p.X < 0 || shown == null || p.X >= shown.Width || p.Y >= shown.Height)
            {
                statusPos.Text = string.Empty;
                statusColor.Text = string.Empty;
                statusColor.BackColor = SystemColors.Control;
                return;
            }
            Color c = shown.GetPixel(p.X, p.Y);
            statusPos.Text = $"X {p.X}  Y {p.Y}";
            statusColor.Text = c.A == 0 ? "transparent" : $"{Hex(c)}  ({c.R}, {c.G}, {c.B})";
            statusColor.BackColor = c.A == 0 ? SystemColors.Control : c;
            statusColor.ForeColor = c.R * 0.299 + c.G * 0.587 + c.B * 0.114 > 150 || c.A == 0 ? Color.Black : Color.White;
        }

        private void StartPick(PickTarget target)
        {
            if (!CheckImage())
            {
                return;
            }
            _toolBeforePick = canvas.Tool == CanvasTool.Pipette ? CanvasTool.Select : canvas.Tool;
            canvas.Tool = CanvasTool.Pipette;
            _pickTarget = target;
            SyncUi();
            SetMessage("Click a pixel in the image (Esc = cancel).");
        }

        private void Canvas_ColorPicked(object sender, (Color Color, bool Secondary) pick)
        {
            Color c = Color.FromArgb(255, pick.Color);
            switch (_pickTarget)
            {
                case PickTarget.From:
                    SetFromColor(c);
                    SetTool(_toolBeforePick);
                    break;
                case PickTarget.To:
                    SetToColor(c);
                    SetTool(_toolBeforePick);
                    break;
                default:
                    if (pick.Secondary)
                    {
                        SetSecondary(c);
                    }
                    else
                    {
                        SetPrimary(c);
                    }
                    break;
            }
            SetMessage($"Picked {Hex(c)}" + (pick.Color.A == 0 ? " (pixel was transparent)." : "."));
        }

        private void Canvas_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) != Keys.Control)
            {
                return;
            }
            if (e is HandledMouseEventArgs h)
            {
                h.Handled = true;
            }
            SetZoom(canvas.Zoom + (e.Delta > 0 ? 1 : -1));
        }

        private void SetZoom(int zoom)
        {
            canvas.Zoom = zoom;
            CenterCanvas();
            SyncUi();
            UpdateStatus();
        }

        private void FitZoom()
        {
            if (_image == null)
            {
                return;
            }
            Size client = panelCanvas.ClientSize;
            int repeat = canvas.TiledView ? 3 : 1;
            int z = Math.Min((client.Width - 10) / (_image.Width * repeat), (client.Height - 10) / (_image.Height * repeat));
            SetZoom(Math.Max(1, Math.Min(16, z)));
        }

        private void CenterCanvas()
        {
            Size client = panelCanvas.ClientSize;
            Point scroll = panelCanvas.AutoScrollPosition;
            int x = Math.Max(0, (client.Width - canvas.Width) / 2) + scroll.X;
            int y = Math.Max(0, (client.Height - canvas.Height) / 2) + scroll.Y;
            canvas.Location = new Point(x, y);
        }

        private void PanelCanvas_Resize(object sender, EventArgs e) => CenterCanvas();

        private void PanelCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (_image != null)
            {
                return;
            }
            const string hint = "No image.\nOpen one (Ctrl+O), paste one (Ctrl+V) or load a texture in the texmaps tab.\n\n"
                                + "Kein Bild.\nÖffnen (Strg+O), einfügen (Strg+V) oder eine Textur im Reiter texmaps laden.";
            TextRenderer.DrawText(e.Graphics, hint, Font, panelCanvas.ClientRectangle, Color.Gainsboro,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }

        #endregion

        #region File, clipboard

        private void OpenImage()
        {
            using (var dialog = new OpenFileDialog { Filter = CutterImaging.ImageFilter, Title = "Open image" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    LoadImage(CutterImaging.LoadBitmap(dialog.FileName), Path.GetFileName(dialog.FileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "The image could not be loaded:\n" + ex.Message, "Open image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private Bitmap ReadClipboard()
        {
            try
            {
                Bitmap bmp = CutterImaging.GetClipboardImage();
                if (bmp == null)
                {
                    SetMessage("The clipboard contains no image.");
                }
                return bmp;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The clipboard could not be read:\n" + ex.Message, "Paste", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void PasteImage()
        {
            Bitmap bmp = ReadClipboard();
            if (bmp != null)
            {
                LoadImage(bmp, "clipboard image");
            }
        }

        private void NewImage()
        {
            int size = (int)_numTileW.Value;
            CancelPreview();
            var bmp = new Bitmap(size, (int)_numTileH.Value, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
            }
            LoadImage(bmp, "new empty image (tile size)");
        }

        private string Extension
        {
            get
            {
                switch (_tsFormat.SelectedIndex)
                {
                    case 1: return ".png";
                    case 2: return ".tiff";
                    case 3: return ".jpeg";
                    default: return ".bmp";
                }
            }
        }

        private void QuickSave()
        {
            if (!CheckImage())
            {
                return;
            }
            CommitPreview(false);
            try
            {
                Directory.CreateDirectory(OutputDirectory);
                string file = Path.Combine(OutputDirectory, "TextureSingleImage_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + Extension);
                using (Bitmap output = Output())
                {
                    CutterImaging.SaveImage(output, file, TransparentFill.Keep);
                }
                SetMessage("Saved: " + file);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The image could not be saved:\n" + ex.Message, "Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveAs()
        {
            if (!CheckImage())
            {
                return;
            }
            CommitPreview(false);
            using (var dialog = new SaveFileDialog { Filter = "PNG|*.png|BMP|*.bmp|TIFF|*.tiff|JPEG|*.jpg", FileName = "Texture_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") })
            {
                if (Directory.Exists(OutputDirectory))
                {
                    dialog.InitialDirectory = OutputDirectory;
                }
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    using (Bitmap output = Output())
                    {
                        CutterImaging.SaveImage(output, dialog.FileName, TransparentFill.Keep);
                    }
                    SetMessage("Saved: " + dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "The image could not be saved:\n" + ex.Message, "Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static void OpenFolder(string folder)
        {
            try
            {
                Directory.CreateDirectory(folder);
                Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
            }
            catch (Exception)
            {
                // Explorer not available.
            }
        }

        private void CopyImage()
        {
            if (!CheckImage())
            {
                return;
            }
            try
            {
                using (Bitmap output = Output())
                {
                    CutterImaging.CopyToClipboard(output);
                }
                SetMessage("Image copied to the clipboard" + (_previewImage != null ? " (with the preview)" : string.Empty) + (HasVisibleLayers ? " (with the layers)." : "."));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Copy failed:\n" + ex.Message, "Copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Bitmap ExtractSelection(Bitmap source)
        {
            Rectangle bounds = canvas.GetSelectionBounds();
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return null;
            }
            var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            using (GraphicsPath path = canvas.GetSelectionPath())
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.TranslateTransform(-bounds.X, -bounds.Y);
                if (canvas.SelectionShape != CanvasSelection.Rectangle)
                {
                    g.SetClip(path);
                }
                g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height));
            }
            return bmp;
        }

        private void CopySelection()
        {
            if (!CheckImage())
            {
                return;
            }
            using (Bitmap part = ExtractSelection(canvas.Image))
            {
                if (part == null)
                {
                    SetMessage("Select an area first.");
                    return;
                }
                try
                {
                    CutterImaging.CopyToClipboard(part);
                    SetMessage($"Selection {part.Width}×{part.Height} copied.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Copy failed:\n" + ex.Message, "Copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CropToSelection()
        {
            if (!CheckImage())
            {
                return;
            }
            if (!canvas.HasSelection)
            {
                SetMessage("Select an area first.");
                return;
            }
            ApplyFilter("Crop", ExtractSelection, true);
        }

        private void DeleteSelection()
        {
            if (!CheckImage() || !canvas.HasSelection)
            {
                return;
            }
            ApplyFilter("Delete selection", b =>
            {
                Bitmap copy = CutterImaging.To32Bpp(b);
                using (GraphicsPath path = canvas.GetSelectionPath())
                using (var g = Graphics.FromImage(copy))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    using (var brush = new SolidBrush(Color.Transparent))
                    {
                        g.FillPath(brush, path);
                    }
                }
                return copy;
            }, true);
        }

        #endregion

        #region Tiles, size, land tiles

        private void UpdateGrid()
        {
            canvas.GridSize = new Size((int)_numTileW.Value, (int)_numTileH.Value);
            canvas.GridOffset = new Point((int)_numOffX.Value, (int)_numOffY.Value);
            canvas.Invalidate();
        }

        private Size TileSize => new Size((int)_numTileW.Value, (int)_numTileH.Value);

        private List<(Bitmap Tile, Point Position)> CutTiles()
        {
            CommitPreview(false);
            using (Bitmap output = Output())
            {
                var tiles = TextureImaging.CutTiles(output, TileSize, new Point((int)_numOffX.Value, (int)_numOffY.Value), _chkPad.Checked, _chkSkipEmpty.Checked);
                if (tiles.Count == 0)
                {
                    MessageBox.Show(this, "No tile fits into the image. Turn on \"Also cut the incomplete tiles\" or choose a smaller size.",
                        "Cut tiles", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return tiles;
            }
        }

        private void CutTilesToFiles()
        {
            if (!CheckImage())
            {
                return;
            }
            Size size = TileSize;
            int estimate = ((_image.Width + size.Width - 1) / size.Width) * ((_image.Height + size.Height - 1) / size.Height);
            if (estimate > 2000 && MessageBox.Show(this, $"This creates about {estimate} files. Continue?", "Cut tiles", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }
            List<(Bitmap Tile, Point Position)> tiles = CutTiles();
            if (tiles.Count == 0)
            {
                return;
            }
            string folder = Path.Combine(OutputDirectory, $"Tiles_{size.Width}x{size.Height}_{DateTime.Now:yyyyMMdd_HHmmss}");
            try
            {
                Directory.CreateDirectory(folder);
                int n = 1;
                foreach ((Bitmap tile, Point pos) in tiles)
                {
                    CutterImaging.SaveImage(tile, Path.Combine(folder, $"TextureImage{n:D3}_x{pos.X}_y{pos.Y}{Extension}"), TransparentFill.Keep);
                    n++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Saving the tiles failed:\n" + ex.Message, "Cut tiles", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                tiles.ForEach(t => t.Tile.Dispose());
            }
            SetMessage($"{tiles.Count} tiles saved in {folder}");
            if (MessageBox.Show(this, $"{tiles.Count} tiles were saved in:\n{folder}\n\nOpen the folder?", "Cut tiles", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                OpenFolder(folder);
            }
        }

        private void FindFreeTextureBlock()
        {
            if (!TextureMul.IsAvailable || _image == null)
            {
                SetMessage("texmaps.mul is not loaded or no image is open.");
                return;
            }
            Size size = TileSize;
            int count = Math.Max(1, (_image.Width / size.Width) * (_image.Height / size.Height));
            CutterImaging.TryParseId(_txtTexStart.Text, out int start);
            int id = TextureMul.FindFreeBlock(start, count);
            if (id < 0)
            {
                SetMessage($"No {count} free texture IDs in a row found.");
                return;
            }
            _txtTexStart.Text = $"0x{id:X4}";
            SetMessage($"Free block for {count} tiles: 0x{id:X4} – 0x{id + count - 1:X4}");
        }

        private void CutTilesToTexmaps()
        {
            if (!CheckImage())
            {
                return;
            }
            if (!TextureMul.IsAvailable)
            {
                MessageBox.Show(this, "texmaps.mul is not loaded. Load the client files in UoFiddler first.", "texmaps", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!TextureMul.IsValidSize(TileSize))
            {
                MessageBox.Show(this, "Textures must be 64 × 64 or 128 × 128. Choose one of these tile sizes.", "texmaps", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!CutterImaging.TryParseId(_txtTexStart.Text, out int start))
            {
                MessageBox.Show(this, "Enter the first texture ID (hex 0x1A2B or decimal) or use \"Free block\".", "texmaps", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            List<(Bitmap Tile, Point Position)> tiles = CutTiles();
            if (tiles.Count == 0)
            {
                return;
            }
            try
            {
                string question = $"Write {tiles.Count} tiles into texmaps.mul from 0x{start:X4} on?" + (_chkTexSkipUsed.Checked ? "\nUsed IDs are skipped." : "\nUsed IDs are REPLACED.");
                if (MessageBox.Show(this, question, "texmaps", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                {
                    return;
                }
                List<int> ids = TextureMul.WriteMany(start, tiles.Select(t => t.Tile).ToList(), _chkTexSkipUsed.Checked, this);
                MarkTexturesUsed(ids);
                string range = ids.Count > 0 ? $"0x{ids[0]:X4} – 0x{ids[ids.Count - 1]:X4}" : "-";
                SetMessage($"{ids.Count} tiles written to texmaps ({range}). Save texmaps.mul to keep them.");
                if (ids.Count > 0)
                {
                    SelectTexture(ids[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Writing failed:\n" + ex.Message, "texmaps", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                tiles.ForEach(t => t.Tile.Dispose());
            }
        }

        private void UpdateResizeFields()
        {
            if (_image == null || _numResizeW == null)
            {
                return;
            }
            _updating = true;
            _numResizeW.Value = Math.Min(_numResizeW.Maximum, _image.Width);
            _numResizeH.Value = Math.Min(_numResizeH.Maximum, _image.Height);
            _updating = false;
        }

        private void KeepAspect(bool widthChanged)
        {
            if (_updating || !_chkKeepAspect.Checked || _image == null)
            {
                return;
            }
            _updating = true;
            if (widthChanged)
            {
                _numResizeH.Value = Math.Max(1, Math.Min(_numResizeH.Maximum, (int)Math.Round((double)_numResizeW.Value * _image.Height / _image.Width)));
            }
            else
            {
                _numResizeW.Value = Math.Max(1, Math.Min(_numResizeW.Maximum, (int)Math.Round((double)_numResizeH.Value * _image.Width / _image.Height)));
            }
            _updating = false;
        }

        private void ResizeImage(int square)
        {
            if (!CheckImage())
            {
                return;
            }
            int w = square > 0 ? square : (int)_numResizeW.Value;
            int h = square > 0 ? square : (int)_numResizeH.Value;
            var quality = (TextureImaging.ResizeQuality)_comboQuality.SelectedIndex;
            ApplyFilter($"Resize to {w}×{h}", b => TextureImaging.Resize(b, w, h, quality), true);
        }

        private void MakeLandTile()
        {
            if (!CheckImage())
            {
                return;
            }
            if (_image.Width != _image.Height &&
                MessageBox.Show(this, "The image is not square, the land tile will be stretched. Continue?", "Land tile", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            ApplyFilter("Texture → land tile", b => TextureImaging.TextureToLandTile(b, _chkBlackOutside.Checked), true);
        }

        private void LandTileToTexture()
        {
            if (!CheckImage())
            {
                return;
            }
            if (_image.Width != TextureImaging.LandTileSize || _image.Height != TextureImaging.LandTileSize)
            {
                MessageBox.Show(this, "The image must be a 44 × 44 land tile.", "Land tile → texture", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int size = _comboLandToTex.SelectedIndex == 1 ? 128 : 64;
            ApplyFilter("Land tile → texture", b => TextureImaging.LandTileToTexture(b, size), true);
        }

        private void MakeSeamless()
        {
            int blend = (int)_numSeamless.Value;
            ApplyFilter("Make seamless", b => TextureImaging.MakeSeamless(b, blend), true);
        }

        private bool ReadArtLandId(out int id)
        {
            if (CutterImaging.TryParseId(_txtArtLandId.Text, out id) && id >= 0 && id <= CutterArt.MaxId(ArtKind.Land))
            {
                return true;
            }
            MessageBox.Show(this, "Enter a land tile ID (hex 0x1A2B or decimal, 0 – 0x3FFF).", "art.mul", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private void LoadArtLand()
        {
            if (!ReadArtLandId(out int id))
            {
                return;
            }
            Bitmap bmp = CutterArt.GetCopy(ArtKind.Land, id);
            if (bmp == null)
            {
                SetMessage($"Land tile 0x{id:X4} is empty or art.mul is not loaded.");
                return;
            }
            LoadImage(bmp, $"land tile 0x{id:X4}");
        }

        private void WriteArtLand()
        {
            if (!CheckImage() || !ReadArtLandId(out int id))
            {
                return;
            }
            CommitPreview(false);
            if (HasVisibleLayers)
            {
                SetMessage("Note: the visible layers are written too.");
            }
            if (_image.Width != CutterArt.LandSize || _image.Height != CutterArt.LandSize)
            {
                MessageBox.Show(this, "Land tiles must be 44 × 44. Use \"Texture → land tile\" first.", "art.mul", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!CutterArt.IsAvailable)
            {
                MessageBox.Show(this, "art.mul is not loaded.", "art.mul", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (CutterArt.IsValid(ArtKind.Land, id) &&
                MessageBox.Show(this, $"Land tile 0x{id:X4} already contains a graphic. Replace it?", "art.mul", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                using (Bitmap output = Output())
                {
                    CutterArt.Write(ArtKind.Land, id, output, this);
                }
                SetMessage($"Written to land tile 0x{id:X4}. Save art.mul in UoFiddler (Land tiles tab) to keep it.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Writing failed:\n" + ex.Message, "art.mul", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Replace colors and color list

        private void SetFromColor(Color c)
        {
            _fromColor = c;
            PaintSwatchButton(_btnFrom, c);
            if (!string.Equals(_txtFrom.Text, Hex(c), StringComparison.OrdinalIgnoreCase))
            {
                _txtFrom.Text = Hex(c);
            }
        }

        private void SetToColor(Color c)
        {
            _toColor = c;
            PaintSwatchButton(_btnTo, c);
            if (!string.Equals(_txtTo.Text, Hex(c), StringComparison.OrdinalIgnoreCase))
            {
                _txtTo.Text = Hex(c);
            }
        }

        /// <summary>No message boxes while typing: invalid codes only turn the field red.</summary>
        private static void HexChanged(TextBox box, Action<Color> set)
        {
            if (TryParseHex(box.Text, out Color c))
            {
                box.BackColor = SystemColors.Window;
                set(c);
            }
            else
            {
                box.BackColor = Color.MistyRose;
            }
        }

        private int ToleranceValue => (int)Math.Round((int)_numTolerance.Value * 2.55);

        private void ReplaceColor()
        {
            int count = 0;
            Color from = _fromColor, to = _toColor;
            ApplyFilter($"Replace {Hex(from)} → {Hex(to)}", b => TextureImaging.ReplaceColor(b, from, to, ToleranceValue, _chkKeepShading.Checked, out count));
            if (_image != null)
            {
                SetMessage($"{count} pixels replaced ({Hex(from)} → {Hex(to)}).");
            }
        }

        private void EraseColor(bool toWhite)
        {
            int count = 0;
            Color c = _fromColor;
            ApplyFilter($"Erase {Hex(c)}", b => TextureImaging.EraseColor(b, c, ToleranceValue, toWhite, out count));
            if (_image != null)
            {
                SetMessage($"{count} pixels of {Hex(c)} " + (toWhite ? "set to white." : "made transparent."));
            }
        }

        private void RefreshColorList()
        {
            if (!CheckImage())
            {
                return;
            }
            Cursor.Current = Cursors.WaitCursor;
            List<KeyValuePair<int, int>> colors = TextureImaging.CountColors(canvas.Image, _chkListSkipBW.Checked);
            int total = colors.Sum(c => c.Value);
            _listColors.BeginUpdate();
            _listColors.Items.Clear();
            foreach (KeyValuePair<int, int> kv in colors.Take(1000))
            {
                Color c = Color.FromArgb(255, Color.FromArgb(kv.Key));
                var item = new ListViewItem(new[] { string.Empty, Hex(c), kv.Value.ToString(), (100.0 * kv.Value / Math.Max(1, total)).ToString("F2") }) { Tag = c };
                _listColors.Items.Add(item);
            }
            _listColors.EndUpdate();
            _lblColorCount.Text = colors.Count > 1000 ? $"{colors.Count} colors (first 1000 shown)" : $"{colors.Count} colors";
            Cursor.Current = Cursors.Default;
        }

        private void ListColors_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.Item.Tag is Color c)
            {
                if (e.Item.Selected)
                {
                    e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
                }
                using (var brush = new SolidBrush(c))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds.X + 4, e.Bounds.Y + 2, e.Bounds.Width - 8, e.Bounds.Height - 4);
                }
                e.Graphics.DrawRectangle(Pens.Gray, e.Bounds.X + 4, e.Bounds.Y + 2, e.Bounds.Width - 8, e.Bounds.Height - 4);
                return;
            }
            e.DrawDefault = true;
        }

        #endregion

        #region Layers

        private CanvasLayer ActiveLayer => canvas.ActiveLayer;

        private void AddLayer(Bitmap bmp, string name)
        {
            if (bmp == null)
            {
                return;
            }
            var layer = new CanvasLayer(bmp, name) { BlackWhiteTransparent = _chkOvTransparent.Checked };
            canvas.Layers.Add(layer);
            canvas.ActiveLayer = layer;
            CenterLayer();
            RefreshLayerList();
            SetTool(CanvasTool.Overlay);
            tabControl1.SelectedTab = tabOverlay;
            NotifyTerrain();
            SetMessage($"Layer \"{name}\" ({bmp.Width}×{bmp.Height}) added. Drag it with \"Move layer\"; it stays a layer until you burn it in.");
        }

        private void RefreshLayerList()
        {
            _updating = true;
            _listLayers.Items.Clear();
            for (int i = canvas.Layers.Count - 1; i >= 0; i--)
            {
                CanvasLayer layer = canvas.Layers[i];
                _listLayers.Items.Add(layer, layer.Visible);
                if (ReferenceEquals(layer, canvas.ActiveLayer))
                {
                    _listLayers.SelectedIndex = _listLayers.Items.Count - 1;
                }
            }
            _updating = false;
            LoadLayerProperties();
            canvas.Invalidate();
        }

        private void LoadLayerProperties()
        {
            CanvasLayer layer = ActiveLayer;
            _updating = true;
            bool has = layer != null;
            _tbOpacity.Enabled = _numOvScale.Enabled = _numOvX.Enabled = _numOvY.Enabled = has;
            if (has)
            {
                _tbOpacity.Value = Math.Max(0, Math.Min(100, layer.Opacity));
                _lblOpacity.Text = $"Opacity: {layer.Opacity} %";
                _numOvScale.Value = Math.Max(_numOvScale.Minimum, Math.Min(_numOvScale.Maximum, layer.Scale));
                _numOvX.Value = Math.Max(_numOvX.Minimum, Math.Min(_numOvX.Maximum, layer.Position.X));
                _numOvY.Value = Math.Max(_numOvY.Minimum, Math.Min(_numOvY.Maximum, layer.Position.Y));
                _chkOvTransparent.Checked = layer.BlackWhiteTransparent;
            }
            _updating = false;
        }

        private void ListLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }
            canvas.ActiveLayer = _listLayers.SelectedItem as CanvasLayer;
            LoadLayerProperties();
            canvas.Invalidate();
        }

        private void ListLayers_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_updating || !(_listLayers.Items[e.Index] is CanvasLayer layer))
            {
                return;
            }
            layer.Visible = e.NewValue == CheckState.Checked;
            canvas.Invalidate();
            NotifyTerrain();
        }

        private void Canvas_LayerMoved(object sender, EventArgs e)
        {
            if (ActiveLayer == null)
            {
                return;
            }
            _updating = true;
            _numOvX.Value = Math.Max(_numOvX.Minimum, Math.Min(_numOvX.Maximum, ActiveLayer.Position.X));
            _numOvY.Value = Math.Max(_numOvY.Minimum, Math.Min(_numOvY.Maximum, ActiveLayer.Position.Y));
            _updating = false;
            NotifyTerrain();
        }

        private void CenterLayer()
        {
            if (ActiveLayer == null || _image == null)
            {
                return;
            }
            Size s = ActiveLayer.Size;
            ActiveLayer.Position = new Point((_image.Width - s.Width) / 2, (_image.Height - s.Height) / 2);
            Canvas_LayerMoved(this, EventArgs.Empty);
            canvas.Invalidate();
        }

        private void FitLayer()
        {
            if (ActiveLayer == null || _image == null)
            {
                return;
            }
            Bitmap img = ActiveLayer.Image;
            int scale = (int)Math.Floor(100.0 * Math.Min((double)_image.Width / img.Width, (double)_image.Height / img.Height));
            ActiveLayer.Scale = (int)Math.Max(_numOvScale.Minimum, Math.Min(_numOvScale.Maximum, scale));
            LoadLayerProperties();
            CenterLayer();
        }

        private void MoveLayer(int delta)
        {
            CanvasLayer layer = ActiveLayer;
            if (layer == null)
            {
                return;
            }
            int i = canvas.Layers.IndexOf(layer);
            int j = i + delta;
            if (j < 0 || j >= canvas.Layers.Count)
            {
                return;
            }
            canvas.Layers.RemoveAt(i);
            canvas.Layers.Insert(j, layer);
            RefreshLayerList();
            NotifyTerrain();
        }

        private void DuplicateLayer()
        {
            if (ActiveLayer == null)
            {
                return;
            }
            CanvasLayer copy = ActiveLayer.Clone();
            copy.Position = new Point(copy.Position.X + 4, copy.Position.Y + 4);
            canvas.Layers.Insert(canvas.Layers.IndexOf(ActiveLayer) + 1, copy);
            canvas.ActiveLayer = copy;
            RefreshLayerList();
        }

        private void RemoveLayer(CanvasLayer layer)
        {
            if (layer == null)
            {
                return;
            }
            int i = canvas.Layers.IndexOf(layer);
            canvas.Layers.Remove(layer);
            if (ReferenceEquals(canvas.ActiveLayer, layer))
            {
                canvas.ActiveLayer = canvas.Layers.Count == 0 ? null : canvas.Layers[Math.Max(0, Math.Min(canvas.Layers.Count - 1, i - 1))];
            }
            if (ReferenceEquals(canvas.Pattern, layer.Image))
            {
                SetPattern(null);
            }
            layer.Dispose();
            if (canvas.Layers.Count == 0 && canvas.Tool == CanvasTool.Overlay)
            {
                SetTool(CanvasTool.Select);
            }
            RefreshLayerList();
            NotifyTerrain();
        }

        private static void DrawLayers(Bitmap target, IEnumerable<CanvasLayer> layers)
        {
            using (var g = Graphics.FromImage(target))
            {
                foreach (CanvasLayer layer in layers)
                {
                    layer.DrawTo(g, 1f);
                }
            }
        }

        /// <summary>Burns layers into the image (with undo) and removes them.</summary>
        private void BurnLayers(bool all)
        {
            List<CanvasLayer> layers = all ? canvas.Layers.Where(l => l.Visible).ToList() : (ActiveLayer != null ? new List<CanvasLayer> { ActiveLayer } : new List<CanvasLayer>());
            if (layers.Count == 0)
            {
                SetMessage("There is no (visible) layer to burn in.");
                return;
            }
            if (!CheckImage())
            {
                return;
            }
            ApplyFilter(layers.Count == 1 ? $"Layer \"{layers[0].Name}\" burned in" : $"{layers.Count} layers burned in", b =>
            {
                Bitmap copy = CutterImaging.To32Bpp(b);
                DrawLayers(copy, layers);
                return copy;
            });
            foreach (CanvasLayer layer in layers)
            {
                RemoveLayer(layer);
            }
        }

        /// <summary>
        /// The image as it is seen: base image (or color preview) plus all visible layers.
        /// Used for saving, copying, tiles, texmaps / art.mul and the terrain preview. The caller disposes it.
        /// </summary>
        private Bitmap Output()
        {
            Bitmap shown = canvas.Image ?? _image;
            if (shown == null)
            {
                return null;
            }
            Bitmap copy = CutterImaging.To32Bpp(shown);
            if (canvas.Layers.Any(l => l.Visible))
            {
                DrawLayers(copy, canvas.Layers.Where(l => l.Visible));
            }
            return copy;
        }

        private bool HasVisibleLayers => canvas.Layers.Any(l => l.Visible);

        private void LayerFromFile()
        {
            using (var dialog = new OpenFileDialog { Filter = CutterImaging.ImageFilter, Title = "Image for a new layer" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    AddLayer(CutterImaging.LoadBitmap(dialog.FileName), Path.GetFileNameWithoutExtension(dialog.FileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "The image could not be loaded:\n" + ex.Message, "Layer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LayerFromClipboard()
        {
            Bitmap bmp = ReadClipboard();
            if (bmp != null)
            {
                AddLayer(bmp, "clipboard");
            }
        }

        private void LayerFromTexture()
        {
            int id = _gridTex.SelectedItem;
            Bitmap bmp = id >= 0 ? TextureMul.GetCopy(id) : null;
            if (bmp == null)
            {
                SetMessage("Select a used texture in the texmaps tab first.");
                return;
            }
            AddLayer(bmp, $"texture 0x{id:X4}");
        }

        private void LayerFromArt()
        {
            ArtKind kind = _radioOvArtLand.Checked ? ArtKind.Land : ArtKind.Item;
            if (!CutterImaging.TryParseId(_txtOvArtId.Text, out int id))
            {
                SetMessage("Enter an art.mul ID (hex 0x1A2B or decimal).");
                return;
            }
            Bitmap bmp = CutterArt.GetCopy(kind, id);
            if (bmp == null)
            {
                SetMessage($"{kind} 0x{id:X4} is empty or art.mul is not loaded.");
                return;
            }
            AddLayer(bmp, $"{kind.ToString().ToLowerInvariant()} 0x{id:X4}");
        }

        private void FillSelectionFromFile()
        {
            if (!CheckImage())
            {
                return;
            }
            if (!canvas.HasSelection)
            {
                SetMessage("Select an area first.");
                return;
            }
            using (var dialog = new OpenFileDialog { Filter = CutterImaging.ImageFilter, Title = "Image for the selection" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    using (Bitmap texture = CutterImaging.LoadBitmap(dialog.FileName))
                    {
                        FillSelection(texture);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "The image could not be loaded:\n" + ex.Message, "Fill", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void FillSelection(Bitmap texture)
        {
            if (texture == null)
            {
                SetMessage("Select a layer first.");
                return;
            }
            if (!CheckImage())
            {
                return;
            }
            if (!canvas.HasSelection)
            {
                SetMessage("Select an area first.");
                return;
            }
            bool tile = _radioFillTile.Checked;
            ApplyFilter("Fill selection", b =>
            {
                Bitmap copy = CutterImaging.To32Bpp(b);
                using (GraphicsPath path = canvas.GetSelectionPath())
                using (var g = Graphics.FromImage(copy))
                {
                    RectangleF bounds = path.GetBounds();
                    if (tile)
                    {
                        using (var brush = new TextureBrush(texture, WrapMode.Tile))
                        {
                            brush.TranslateTransform(bounds.X, bounds.Y);
                            g.FillPath(brush, path);
                        }
                    }
                    else
                    {
                        g.SetClip(path);
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(texture, bounds);
                    }
                }
                return copy;
            }, true);
        }

        #endregion

        #region Pattern brush

        private void SetPattern(Bitmap pattern)
        {
            Bitmap old = _pattern;
            _pattern = pattern;
            canvas.Pattern = pattern;
            canvas.UsePattern = pattern != null;
            _tsPattern.Image?.Dispose();
            _tsPattern.Image = pattern == null ? null : TextureImaging.Resize(pattern, 16, 16, TextureImaging.ResizeQuality.Smooth);
            _tsPattern.DisplayStyle = pattern == null ? ToolStripItemDisplayStyle.Text : ToolStripItemDisplayStyle.ImageAndText;
            if (old != null && !ReferenceEquals(old, pattern))
            {
                old.Dispose();
            }
            SyncUi();
            SetMessage(pattern == null ? "Pattern brush off." : $"Pattern brush on ({pattern.Width}×{pattern.Height}): pencil, line, rectangle and fill paint with it.");
        }

        private void PatternFrom(Func<Bitmap> source, string what)
        {
            Bitmap bmp = source();
            if (bmp == null)
            {
                SetMessage($"No {what} for the pattern.");
                return;
            }
            SetPattern(bmp);
            if (canvas.Tool != CanvasTool.Pencil && canvas.Tool != CanvasTool.Fill && canvas.Tool != CanvasTool.Line && canvas.Tool != CanvasTool.Rectangle)
            {
                SetTool(CanvasTool.Pencil);
            }
        }

        private Bitmap PatternFromFile()
        {
            using (var dialog = new OpenFileDialog { Filter = CutterImaging.ImageFilter, Title = "Pattern image" })
            {
                return dialog.ShowDialog(this) == DialogResult.OK ? CutterImaging.LoadBitmap(dialog.FileName) : null;
            }
        }

        #endregion

        #region Palette

        private void RefreshPalette()
        {
            _paletteFlow.SuspendLayout();
            foreach (Control c in _paletteFlow.Controls.Cast<Control>().ToList())
            {
                c.Dispose();
            }
            _paletteFlow.Controls.Clear();
            foreach (Color color in _palette.Colors)
            {
                Color c = color;
                var b = new Button { Width = 20, Height = 20, Margin = new Padding(1), FlatStyle = FlatStyle.Flat, BackColor = c, TabStop = false };
                b.FlatAppearance.BorderColor = Color.Gray;
                b.MouseUp += (s, e) =>
                {
                    if ((ModifierKeys & Keys.Control) == Keys.Control)
                    {
                        _palette.Colors.Remove(c);
                        RefreshPalette();
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        SetSecondary(c);
                    }
                    else if (e.Button == MouseButtons.Left)
                    {
                        SetPrimary(c);
                    }
                };
                toolTip1.SetToolTip(b, $"{Hex(c)}\nLeft = color 1, right = color 2, Ctrl + click = remove\nDE: Links = Farbe 1, rechts = Farbe 2, Strg + Klick = entfernen");
                _paletteFlow.Controls.Add(b);
            }
            _paletteFlow.ResumeLayout();
            _lblPalette.Text = $"{_palette.Colors.Count} colors";
        }

        private void PaletteFromImage()
        {
            if (!CheckImage())
            {
                return;
            }
            foreach (KeyValuePair<int, int> kv in TextureImaging.CountColors(canvas.Image, true).Take(16))
            {
                _palette.Add(Color.FromArgb(kv.Key));
            }
            RefreshPalette();
        }

        private void PaletteFromHue()
        {
            List<Color> colors = CutterPalette.FromHue((int)_numHue.Value, out string name);
            if (colors == null)
            {
                SetMessage("Hue not found (is hues.mul loaded?).");
                return;
            }
            foreach (Color c in colors)
            {
                _palette.Add(c);
            }
            RefreshPalette();
            SetMessage($"Hue {(int)_numHue.Value} \"{name}\": 32 colors added.");
        }

        private void LoadPalette()
        {
            using (var dialog = new OpenFileDialog { Filter = "Palettes|*.pal;*.gpl;*.txt|All files|*.*" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    CutterPalette loaded = CutterPalette.Load(dialog.FileName);
                    _palette.Colors.Clear();
                    _palette.Colors.AddRange(loaded.Colors);
                    RefreshPalette();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "The palette could not be loaded:\n" + ex.Message, "Palette", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SavePalette()
        {
            using (var dialog = new SaveFileDialog { Filter = "JASC palette|*.pal", FileName = "palette.pal" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    _palette.Save(dialog.FileName);
                    SetMessage("Palette saved: " + dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "The palette could not be saved:\n" + ex.Message, "Palette", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Transitions (blend)

        private void SetBlendB(Bitmap bmp, string name)
        {
            if (bmp == null)
            {
                SetMessage($"No {name} for texture B.");
                return;
            }
            _blendB?.Dispose();
            _blendB = bmp;
            _picBlendB.Image?.Dispose();
            _picBlendB.Image = TextureImaging.Resize(bmp, 64, 64, TextureImaging.ResizeQuality.Smooth);
            _lblBlendB.Text = $"B: {name} ({bmp.Width}×{bmp.Height})";
            SchedulePreview(PreviewKind.Transition);
        }

        private Bitmap MakeTransition(Bitmap a, TransitionShape shape) =>
            TextureBlend.MakeTransition(a, _blendB, shape, (int)_numSoftness.Value, (int)_numRoughness.Value, (int)_numSeed.Value);

        private void ShowTransitionPreview()
        {
            if (!CheckImage())
            {
                return;
            }
            if (_blendB == null)
            {
                SetMessage("Choose texture B first (the edited image is texture A).");
                return;
            }
            SchedulePreview(PreviewKind.Transition);
        }

        /// <summary>All nine transitions at once: files and / or texmaps.mul.</summary>
        private void AllTransitions(bool toTexmaps)
        {
            if (!CheckImage())
            {
                return;
            }
            if (_blendB == null)
            {
                SetMessage("Choose texture B first.");
                return;
            }
            CommitPreview(false);
            var pieces = new List<Bitmap>();
            try
            {
                foreach (TransitionShape shape in Enum.GetValues(typeof(TransitionShape)))
                {
                    pieces.Add(MakeTransition(_image, shape));
                }
                if (!toTexmaps)
                {
                    string folder = Path.Combine(OutputDirectory, "Transitions_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                    Directory.CreateDirectory(folder);
                    for (int i = 0; i < pieces.Count; i++)
                    {
                        string name = ((TransitionShape)i).ToString();
                        CutterImaging.SaveImage(pieces[i], Path.Combine(folder, $"{i + 1:D2}_{name}{Extension}"), TransparentFill.Keep);
                    }
                    SetMessage($"9 transitions saved in {folder}");
                    if (MessageBox.Show(this, $"9 transitions saved in:\n{folder}\n\nOpen the folder?", "Transitions", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        OpenFolder(folder);
                    }
                    return;
                }
                if (!TextureMul.IsAvailable || !TextureMul.IsValidSize(_image.Size))
                {
                    MessageBox.Show(this, "texmaps.mul must be loaded and texture A must be 64 × 64 or 128 × 128.", "Transitions", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (!CutterImaging.TryParseId(_txtBlendTexId.Text, out int start))
                {
                    MessageBox.Show(this, "Enter the first texture ID.", "Transitions", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (MessageBox.Show(this, $"Write 9 transitions into texmaps.mul from 0x{start:X4} on (used IDs are skipped)?", "Transitions", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                {
                    return;
                }
                List<int> ids = TextureMul.WriteMany(start, pieces, true, this);
                MarkTexturesUsed(ids);
                SetMessage($"{ids.Count} transitions written to texmaps ({string.Join(", ", ids.Select(i => $"0x{i:X4}"))}). Save texmaps.mul to keep them.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Creating the transitions failed:\n" + ex.Message, "Transitions", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                pieces.ForEach(p => p.Dispose());
            }
        }

        #endregion

        #region Batch and terrain preview

        private void OpenBatch()
        {
            CommitPreview(false);
            using (var dialog = new BatchDialog(OutputDirectory) { TopMost = TopMost })
            {
                dialog.ShowDialog(this);
                if (dialog.WrittenTextures.Count > 0)
                {
                    MarkTexturesUsed(dialog.WrittenTextures);
                }
            }
        }

        private void OpenTerrainPreview()
        {
            if (_terrain == null || _terrain.IsDisposed)
            {
                _terrain = new TerrainPreviewForm(Output) { TopMost = TopMost };
                _terrain.FormClosed += (s, e) => _terrain = null;
                _terrain.Show(this);
            }
            else
            {
                _terrain.Activate();
            }
        }

        /// <summary>Tells the terrain preview that the image changed.</summary>
        private void NotifyTerrain()
        {
            if (_terrain != null && !_terrain.IsDisposed)
            {
                _terrain.EditorImageChanged();
            }
        }

        #endregion

        #region texmaps

        private void ReloadTextures()
        {
            _texLoaded = true;
            if (!TextureMul.IsAvailable)
            {
                _usedTextures = null;
                _gridTex.Items = Array.Empty<int>();
                _lblTexInfo.Text = "texmaps.mul is not loaded.";
                return;
            }
            Cursor.Current = Cursors.WaitCursor;
            _usedTextures = TextureMul.ScanUsed();
            Cursor.Current = Cursors.Default;
            RebuildTextureList();
        }

        private void RebuildTextureList()
        {
            if (_usedTextures == null)
            {
                return;
            }
            int keep = _gridTex.SelectedItem;
            var list = new List<int>();
            for (int i = 0; i < _usedTextures.Length; i++)
            {
                if (_comboTexFilter.SelectedIndex == 0 || (_comboTexFilter.SelectedIndex == 1 && _usedTextures[i]) || (_comboTexFilter.SelectedIndex == 2 && !_usedTextures[i]))
                {
                    list.Add(i);
                }
            }
            _gridTex.Items = list;
            if (keep >= 0)
            {
                _gridTex.SelectItem(keep);
            }
            UpdateTexturePreview();
        }

        private void SelectTexture(int id)
        {
            if (!_texLoaded)
            {
                ReloadTextures();
            }
            if (!_gridTex.Items.Contains(id))
            {
                _comboTexFilter.SelectedIndex = 0;
            }
            _gridTex.SelectItem(id);
        }

        private void MarkTexturesUsed(IEnumerable<int> ids, bool used = true)
        {
            if (_usedTextures != null)
            {
                foreach (int id in ids)
                {
                    if (id >= 0 && id < _usedTextures.Length)
                    {
                        _usedTextures[id] = used;
                    }
                }
            }
            _gridTex.Invalidate();
            UpdateTexturePreview();
        }

        private void GoToTexture()
        {
            if (!CutterImaging.TryParseId(_txtTexId.Text, out int id) || id < 0 || id > TextureMul.MaxId)
            {
                SetMessage("Invalid texture ID (0 – 0x3FFF).");
                return;
            }
            SelectTexture(id);
            _gridTex.Focus();
        }

        private void NextFreeTexture()
        {
            int id = TextureMul.FindFree(Math.Max(0, _gridTex.SelectedItem + 1));
            if (id < 0)
            {
                SetMessage("No free texture ID found.");
                return;
            }
            SelectTexture(id);
            _gridTex.Focus();
        }

        private void UpdateTexturePreview()
        {
            int id = _gridTex.SelectedItem;
            if (id < 0)
            {
                _lblTexInfo.Text = _usedTextures != null ? $"{_gridTex.Items.Count} entries" : _lblTexInfo.Text;
            }
            else
            {
                Bitmap bmp = TextureMul.GetShared(id);
                string state = bmp == null ? "free" : $"{bmp.Width}×{bmp.Height}";
                string land = string.Empty;
                if (bmp != null)
                {
                    List<int> lands = TextureMul.LandTilesUsing(id);
                    land = lands.Count == 0 ? "\nno land tile uses it (tiledata)"
                        : $"\nused by {lands.Count} land tiles: " + string.Join(", ", lands.Take(4).Select(l => $"0x{l:X4}")) + (lands.Count > 4 ? " …" : string.Empty);
                }
                _lblTexInfo.Text = $"Texture {CutterImaging.IdText(id)} – {state}{land}";
                _txtTexId.Text = $"0x{id:X4}";
            }
            _panelTexPreview.Invalidate();
        }

        private void PanelTexPreview_Paint(object sender, PaintEventArgs e)
        {
            int id = _gridTex.SelectedItem;
            Bitmap bmp = id >= 0 ? TextureMul.GetShared(id) : null;
            if (bmp == null)
            {
                if (id >= 0)
                {
                    TextRenderer.DrawText(e.Graphics, "free / frei", Font, _panelTexPreview.ClientRectangle, Color.IndianRed,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                return;
            }
            Rectangle box = Rectangle.Inflate(_panelTexPreview.ClientRectangle, -4, -4);
            try
            {
                // texture and the land tile made from it, side by side
                int s = Math.Min(box.Height, (box.Width - 10) / 2);
                var left = new Rectangle(box.X, box.Y + (box.Height - s) / 2, s, s);
                CutterImaging.DrawChecker(e.Graphics, left);
                CutterImaging.DrawFitted(e.Graphics, bmp, left, true);
                Bitmap copy;
                lock (Class.AnimEdit.SdkSync.Lock)
                {
                    copy = CutterImaging.To32Bpp(bmp);
                }
                using (copy)
                using (Bitmap tile = TextureImaging.TextureToLandTile(copy, false))
                {
                    var right = new Rectangle(left.Right + 10, left.Y, s, s);
                    CutterImaging.DrawFitted(e.Graphics, tile, right, true);
                }
            }
            catch (Exception)
            {
                // bitmap in use – skip this paint
            }
        }

        private bool CheckTexSelection(bool needUsed)
        {
            if (!TextureMul.IsAvailable)
            {
                MessageBox.Show(this, "texmaps.mul is not loaded. Load the client files in UoFiddler first.", "texmaps", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            int id = _gridTex.SelectedItem;
            if (id < 0)
            {
                MessageBox.Show(this, "Select a texture ID first.", "texmaps", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (needUsed && !TextureMul.IsValid(id))
            {
                MessageBox.Show(this, $"Texture 0x{id:X4} is empty.", "texmaps", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        private void TextureToEditor()
        {
            if (!CheckTexSelection(true))
            {
                return;
            }
            int id = _gridTex.SelectedItem;
            Bitmap bmp = TextureMul.GetCopy(id);
            if (bmp != null)
            {
                LoadImage(bmp, $"texture 0x{id:X4}");
                _txtTexStart.Text = $"0x{id:X4}";
            }
        }

        private void CopyTexture()
        {
            if (!CheckTexSelection(true))
            {
                return;
            }
            using (Bitmap bmp = TextureMul.GetCopy(_gridTex.SelectedItem))
            {
                if (bmp != null)
                {
                    CutterImaging.CopyToClipboard(bmp);
                    SetMessage($"Texture 0x{_gridTex.SelectedItem:X4} copied.");
                }
            }
        }

        private void ImageToTexture()
        {
            if (!CheckImage() || !CheckTexSelection(false))
            {
                return;
            }
            CommitPreview(false);
            int id = _gridTex.SelectedItem;
            Bitmap output = Output();
            Bitmap toWrite = output;
            Bitmap scaled = null;
            if (!TextureMul.IsValidSize(output.Size))
            {
                DialogResult answer = MessageBox.Show(this,
                    $"Textures must be 64 × 64 or 128 × 128. The image is {output.Width} × {output.Height}.\n\nYes = scale to 64 × 64\nNo = scale to 128 × 128\nCancel = do nothing",
                    "Image → texture", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (answer == DialogResult.Cancel)
                {
                    output.Dispose();
                    return;
                }
                int size = answer == DialogResult.Yes ? 64 : 128;
                scaled = TextureImaging.Resize(output, size, size, TextureImaging.ResizeQuality.Smooth);
                toWrite = scaled;
            }
            try
            {
                if (TextureMul.IsValid(id) &&
                    MessageBox.Show(this, $"Texture 0x{id:X4} already exists. Replace it?", "Image → texture", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return;
                }
                TextureMul.Write(id, toWrite, this);
                MarkTexturesUsed(new[] { id });
                SetMessage($"Written to texture 0x{id:X4} ({toWrite.Width}×{toWrite.Height}). Save texmaps.mul to keep it.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Writing failed:\n" + ex.Message, "texmaps", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                scaled?.Dispose();
                output.Dispose();
            }
        }

        /// <summary>Image → 44 × 44 land tile → every land tile that uses the selected texture (tiledata).</summary>
        private void ImageToLandTilesOfTexture()
        {
            if (!CheckImage() || !CheckTexSelection(false))
            {
                return;
            }
            if (!CutterArt.IsAvailable)
            {
                MessageBox.Show(this, "art.mul is not loaded.", "Land tiles", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int texId = _gridTex.SelectedItem;
            List<int> lands = TextureMul.LandTilesUsing(texId);
            if (lands.Count == 0)
            {
                MessageBox.Show(this, $"No land tile uses texture 0x{texId:X4} (tiledata.mul).", "Land tiles", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            CommitPreview(false);
            using (Bitmap output = Output())
            {
                bool isTile = output.Width == CutterArt.LandSize && output.Height == CutterArt.LandSize;
                string list = string.Join(", ", lands.Take(12).Select(l => $"0x{l:X4}")) + (lands.Count > 12 ? " …" : string.Empty);
                if (MessageBox.Show(this, $"Write the image as land tile into {lands.Count} land tiles that use texture 0x{texId:X4}?\n{list}\n\n"
                        + (isTile ? "The image is already a 44 × 44 land tile." : "The image is turned into a 44 × 44 land tile first (black outside)."),
                        "Land tiles", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                {
                    return;
                }
                using (Bitmap tile = isTile ? CutterImaging.To32Bpp(output) : TextureImaging.TextureToLandTile(output, true))
                {
                    try
                    {
                        foreach (int land in lands)
                        {
                            CutterArt.Write(ArtKind.Land, land, tile, this);
                        }
                        SetMessage($"{lands.Count} land tiles written ({list}). Save art.mul in UoFiddler (Land tiles tab) to keep them.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "Writing failed:\n" + ex.Message, "Land tiles", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void RemoveTexture()
        {
            if (!CheckTexSelection(true))
            {
                return;
            }
            int id = _gridTex.SelectedItem;
            if (MessageBox.Show(this, $"Remove texture 0x{id:X4}?", "texmaps", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                TextureMul.Remove(id, this);
                MarkTexturesUsed(new[] { id }, false);
                SetMessage($"Texture 0x{id:X4} removed. Save texmaps.mul to keep it.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Removing failed:\n" + ex.Message, "texmaps", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveTexmaps()
        {
            if (!TextureMul.IsAvailable)
            {
                MessageBox.Show(this, "texmaps.mul is not loaded.", "Save texmaps.mul", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show(this, $"Save texidx.mul and texmaps.mul into:\n{UoFiddler.Controls.Classes.Options.OutputPath}", "Save texmaps.mul",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                SetMessage("texmaps.mul saved in " + TextureMul.Save());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Saving failed:\n" + ex.Message, "Save texmaps.mul", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        #endregion

        #region Help, window, keyboard

        private void ShowHelp(bool german) => TextureCutterHelp.Show(this, german);

        // Saves the position.
        private void TextureCutter_FormClosed(object sender, FormClosedEventArgs e)
        {
            previewTimer.Stop();
            Properties.Settings.Default.FormTextureCutter = Location;
            Properties.Settings.Default.Save();

            try
            {
                _palette.Save(CutterPalette.DefaultFile);
            }
            catch (Exception)
            {
                // palette could not be saved – not important
            }
            if (_terrain != null && !_terrain.IsDisposed)
            {
                _terrain.Close();
            }
            _image?.Dispose();
            _previewImage?.Dispose();
            _pattern?.Dispose();
            _blendB?.Dispose();
            canvas.Layers.ForEach(l => l.Dispose());
            canvas.Layers.Clear();
            _undo.ForEach(b => b.Dispose());
            _redo.ForEach(b => b.Dispose());
        }

        // Loads the position – only if it is still on a screen.
        private void TextureCutterForm_Load(object sender, EventArgs e)
        {
            Point saved = Properties.Settings.Default.FormTextureCutter;
            if (saved != Point.Empty && Screen.AllScreens.Any(s => s.WorkingArea.Contains(new Point(saved.X + 50, saved.Y + 20))))
            {
                StartPosition = FormStartPosition.Manual;
                Location = saved;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            bool ctrl = (keyData & Keys.Control) == Keys.Control;
            bool shift = (keyData & Keys.Shift) == Keys.Shift;
            bool alt = (keyData & Keys.Alt) == Keys.Alt;
            bool textFocus = IsTextInputFocused();

            if (alt)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
            if (key == Keys.F1)
            {
                ShowHelp(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de");
                return true;
            }
            if (ctrl)
            {
                switch (key)
                {
                    case Keys.O: OpenImage(); return true;
                    case Keys.S: if (shift) SaveAs(); else QuickSave(); return true;
                    case Keys.G: ToggleGrid(); return true;
                    case Keys.T: ToggleTiled(); return true;
                    case Keys.F: FitZoom(); return true;
                    case Keys.P: OpenTerrainPreview(); return true;
                    case Keys.B: OpenBatch(); return true;
                    case Keys.D0: case Keys.NumPad0: SetZoom(1); return true;
                    case Keys.Oemplus: case Keys.Add: SetZoom(canvas.Zoom + 1); return true;
                    case Keys.OemMinus: case Keys.Subtract: SetZoom(canvas.Zoom - 1); return true;
                }
                if (!textFocus)
                {
                    switch (key)
                    {
                        case Keys.V: PasteImage(); return true;
                        case Keys.C:
                        case Keys.X: // Ctrl+X was the old copy shortcut.
                            if (shift) CopySelection(); else CopyImage();
                            return true;
                        case Keys.Z: if (shift) Redo(); else Undo(); return true;
                        case Keys.Y: Redo(); return true;
                        case Keys.A: canvas.SelectAll(); return true;
                    }
                }
                return base.ProcessCmdKey(ref msg, keyData);
            }
            if (textFocus)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            switch (key)
            {
                case Keys.Escape:
                    if (canvas.Tool == CanvasTool.Pipette && _pickTarget != PickTarget.Paint)
                    {
                        SetTool(_toolBeforePick);
                    }
                    else if (_previewImage != null)
                    {
                        CancelPreview();
                    }
                    else
                    {
                        canvas.ClearSelection();
                    }
                    return true;
                case Keys.Enter:
                    if (_previewImage != null)
                    {
                        CommitPreview(true);
                        return true;
                    }
                    break;
                case Keys.Delete:
                    DeleteSelection();
                    return true;
                case Keys.M: SetTool(CanvasTool.Select); return true;
                case Keys.B: SetTool(CanvasTool.Pencil); return true;
                case Keys.E: SetTool(CanvasTool.Eraser); return true;
                case Keys.L: SetTool(CanvasTool.Line); return true;
                case Keys.R: SetTool(CanvasTool.Rectangle); return true;
                case Keys.G: SetTool(CanvasTool.Fill); return true;
                case Keys.I: SetTool(CanvasTool.Pipette); return true;
                case Keys.O: SetTool(CanvasTool.Overlay); return true;
                case Keys.X: SwapColors(); return true;
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    if (canvas.ActiveLayer != null)
                    {
                        int step = shift ? 10 : 1;
                        Point p = canvas.ActiveLayer.Position;
                        p.Offset(key == Keys.Left ? -step : key == Keys.Right ? step : 0, key == Keys.Up ? -step : key == Keys.Down ? step : 0);
                        canvas.ActiveLayer.Position = p;
                        canvas.Invalidate();
                        Canvas_LayerMoved(this, EventArgs.Empty);
                        return true;
                    }
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion
    }
}

// /***************************************************************************
//  *
//  * Altitude tool – map heights ↔ image.
//  * Built-in replacement of AltitudeTool.exe ((c) 2007 Andreas Schneider): extract, apply and modify
//  * work directly in C#, the external program is no longer needed.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AltitudeTool;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin
{
    /// <summary>
    /// Heights of a UO map as an image and back: extract (map → image), apply (image → map), modify (raise / lower,
    /// optionally with the statics) – plus a zoomable view, selections, brushes, smoothing and more, all with undo.
    /// </summary>
    public partial class AltitudeToolForm : Form
    {
        private static readonly string[] MapNames = { "Felucca (map0)", "Trammel (map1)", "Ilshenar (map2)", "Malas (map3)", "Tokuno (map4)", "Ter Mur (map5)" };
        private const long UndoBudget = 256L * 1024 * 1024;

        private HeightView _view;
        private HeightMap _map;
        private HeightMap _original;
        private string _sourceFile;       // map*.mul the map was read from (null = loaded from UoFiddler, UOP or mul)
        private int _sourceMapIndex = -1; // UoFiddler map index when loaded from UoFiddler
        private string _patchFile;        // mul to copy / patch when saving
        private readonly List<(Rectangle Area, sbyte[] Data)> _undo = new List<(Rectangle, sbyte[])>();
        private readonly List<(Rectangle Area, sbyte[] Data)> _redo = new List<(Rectangle, sbyte[])>();
        private sbyte[] _strokeBackup;
        private CancellationTokenSource _cts;
        private bool _busy;

        public AltitudeToolForm()
        {
            InitializeComponent();
            _view = new HeightView { Dock = DockStyle.Fill };
            splitContainer1.Panel1.Controls.Add(_view);
            _view.HoverTile += View_HoverTile;
            _view.SelectionChanged += (s, e) => SelectionChanged();
            _view.ViewChanged += (s, e) => UpdateZoomLabel();
            _view.BrushStarted += (s, e) => _strokeBackup = (sbyte[])_map.Z.Clone();
            _view.BrushEnded += View_BrushEnded;
            _view.ZPicked += (s, z) =>
            {
                _numFlatten.Value = z;
                _numSetZ.Value = z;
                SetMessage($"Picked z {z} – used by Flatten and \"Set to\".");
            };
            statusCancel.Click += (s, e) => _cts?.Cancel();

            BuildToolbar();
            BuildSourceTab();
            BuildExportTab();
            BuildImportTab();
            BuildEditTab();
            BuildInfoTab();
            SelectionChanged();
            UpdateInfo();
            SyncUi();
            SetMessage("Load a map file (map*.mul) or the map UoFiddler has loaded. F1 = help.");
        }

        #region Helpers

        private static string Tip(string en, string de) => en + "\n\nDE: " + de;

        private void SetMessage(string text) => statusMessage.Text = text;

        private static FlowLayoutPanel Row(params Control[] controls)
        {
            var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 1, 0, 1) };
            row.Controls.AddRange(controls);
            return row;
        }

        private static Label Lbl(string text, int width = 0) => width > 0
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

        private static NumericUpDown Num(decimal min, decimal max, decimal value, int width = 60, int decimals = 0) =>
            new NumericUpDown
            {
                Minimum = min, Maximum = max, DecimalPlaces = decimals, Increment = decimals > 0 ? 0.1m : 1,
                Value = Math.Max(min, Math.Min(max, value)), Width = width, Margin = new Padding(3, 3, 3, 0)
            };

        private static CheckBox Chk(string text, bool value = false) => new CheckBox { Text = text, AutoSize = true, Checked = value, Margin = new Padding(3, 5, 3, 0) };

        private static TextBox In(int width) => new TextBox { Width = width, Margin = new Padding(3, 4, 3, 0) };

        private static ComboBox Combo(int width, params object[] items)
        {
            var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = width, Margin = new Padding(3, 4, 3, 0) };
            c.Items.AddRange(items);
            if (items.Length > 0)
            {
                c.SelectedIndex = 0;
            }
            return c;
        }

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

        private bool CheckMap()
        {
            if (_map != null)
            {
                return true;
            }
            SetMessage("Load a map first (tab Map).");
            return false;
        }

        private bool IsDirty => _map != null && _original != null && _map.CountDifferences(_original) > 0;

        /// <summary>Target of the edit operations: the selection, or the whole map.</summary>
        private Rectangle Target => _view.Selection.IsEmpty ? _map.Bounds : _view.Selection;

        private string TargetText => _view.Selection.IsEmpty ? "whole map" : $"selection {_view.Selection.Width}×{_view.Selection.Height}";

        private static string BrowseOpen(IWin32Window owner, string filter, string current)
        {
            using (var d = new OpenFileDialog { Filter = filter })
            {
                try
                {
                    if (!string.IsNullOrEmpty(current) && File.Exists(current))
                    {
                        d.InitialDirectory = Path.GetDirectoryName(current);
                        d.FileName = Path.GetFileName(current);
                    }
                }
                catch (Exception)
                {
                    // invalid path
                }
                return d.ShowDialog(owner) == DialogResult.OK ? d.FileName : null;
            }
        }

        #endregion

        #region Background work

        /// <summary>Runs work in the background with progress bar and cancel; the UI is locked meanwhile.</summary>
        private async Task<T> RunBusy<T>(string what, Func<IProgress<int>, CancellationToken, T> work)
        {
            _busy = true;
            _cts = new CancellationTokenSource();
            splitContainer1.Enabled = toolStrip1.Enabled = false;
            statusProgress.Value = 0;
            statusProgress.Visible = statusCancel.Visible = true;
            SetMessage(what + " …");
            Cursor = Cursors.WaitCursor;
            var progress = new Progress<int>(p => statusProgress.Value = Math.Max(0, Math.Min(100, p)));
            try
            {
                CancellationToken token = _cts.Token;
                return await Task.Run(() => work(progress, token), token);
            }
            finally
            {
                Cursor = Cursors.Default;
                statusProgress.Visible = statusCancel.Visible = false;
                splitContainer1.Enabled = toolStrip1.Enabled = true;
                _cts.Dispose();
                _cts = null;
                _busy = false;
            }
        }

        private void ShowError(string what, Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                SetMessage(what + " cancelled.");
                return;
            }
            SetMessage(what + " failed.");
            MessageBox.Show(this, what + " failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion

        #region Toolbar

        private readonly Dictionary<HeightTool, ToolStripButton> _toolButtons = new Dictionary<HeightTool, ToolStripButton>();
        private ToolStripButton _tsUndo, _tsRedo, _tsChanges, _tsGrid;
        private ToolStripComboBox _tsColor, _tsContour, _tsRadius, _tsStrength;
        private ToolStripLabel _tsZoom;

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

        private void BuildToolbar()
        {
            _tsUndo = TextButton("↶", Tip("Undo (Ctrl+Z)", "Rückgängig (Strg+Z)"), (s, e) => Undo(), true);
            _tsRedo = TextButton("↷", Tip("Redo (Ctrl+Y)", "Wiederholen (Strg+Y)"), (s, e) => Redo(), true);
            void Tool(HeightTool t, string text, string en, string de) => _toolButtons[t] = TextButton(text, Tip(en, de), (s, e) => SetTool(t));
            Tool(HeightTool.Select, "Select", "Select an area (S). The edit operations and the export use it. Right / middle mouse = move, wheel = zoom.",
                "Bereich auswählen (S). Bearbeitung und Export benutzen ihn. Rechte / mittlere Maus = verschieben, Mausrad = Zoom.");
            Tool(HeightTool.Raise, "Raise", "Brush: raise the land (R).", "Pinsel: Land anheben (R).");
            Tool(HeightTool.Lower, "Lower", "Brush: lower the land (L).", "Pinsel: Land absenken (L).");
            Tool(HeightTool.Smooth, "Smooth", "Brush: smooth (M).", "Pinsel: glätten (M).");
            Tool(HeightTool.Flatten, "Flatten", "Brush: pull towards the flatten height (F, set in Modify or with the pipette).",
                "Pinsel: zur Flatten-Höhe ziehen (F, im Reiter Modify oder per Pipette).");
            Tool(HeightTool.Pick, "Pipette", "Pick an altitude (I) for Flatten and \"Set to\".", "Höhe aufnehmen (I) für Flatten und \"Set to\".");

            _tsRadius = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(45, 23), ToolTipText = Tip("Brush radius in tiles", "Pinselradius in Kacheln") };
            foreach (int r in new[] { 0, 1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64 })
            {
                _tsRadius.Items.Add(r);
            }
            _tsRadius.SelectedItem = 6;
            _tsRadius.SelectedIndexChanged += (s, e) => _view.BrushRadius = (int)_tsRadius.SelectedItem;
            _tsStrength = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(40, 23), ToolTipText = Tip("Brush strength (z per step)", "Pinselstärke (z pro Schritt)") };
            for (int i = 1; i <= 10; i++)
            {
                _tsStrength.Items.Add(i);
            }
            _tsStrength.SelectedIndex = 1;
            _tsStrength.SelectedIndexChanged += (s, e) => _view.BrushStrength = (int)_tsStrength.SelectedItem;

            _tsColor = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(130, 23), ToolTipText = Tip("How the heights are shown", "Wie die Höhen angezeigt werden") };
            _tsColor.Items.AddRange(new object[] { "Gray (z + 128, like UO)", "Gray stretched", "Terrain colors", "Hillshade", "Terrain + shade" });
            _tsColor.SelectedIndex = 4;
            _tsColor.SelectedIndexChanged += (s, e) =>
            {
                _view.ColorMode = (HeightColorMode)_tsColor.SelectedIndex;
                _view.UpdatePalette();
            };
            _tsContour = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(75, 23), ToolTipText = Tip("Contour lines every n z", "Höhenlinien alle n z") };
            _tsContour.Items.AddRange(new object[] { "no lines", "lines 5", "lines 10", "lines 20" });
            _tsContour.SelectedIndex = 0;
            _tsContour.SelectedIndexChanged += (s, e) =>
            {
                _view.Contour = new[] { 0, 5, 10, 20 }[_tsContour.SelectedIndex];
                _view.InvalidateMap();
            };
            _tsChanges = TextButton("Changes", Tip("Tint changed tiles (not yet saved) red", "Geänderte (noch nicht gespeicherte) Kacheln rot färben"), (s, e) =>
            {
                _view.ShowChanges = !_view.ShowChanges;
                _view.InvalidateMap();
                SyncUi();
            });
            _tsGrid = TextButton("Blocks", Tip("8 × 8 block grid (from zoom 4×)", "8 × 8 Blockraster (ab Zoom 4×)"), (s, e) =>
            {
                _view.ShowGrid = !_view.ShowGrid;
                _view.Invalidate();
                SyncUi();
            });
            _tsZoom = new ToolStripLabel("–") { AutoSize = false, Width = 50 };

            toolStrip1.Items.AddRange(new ToolStripItem[]
            {
                ImageButton(Properties.Resources.Load, Tip("Open a map*.mul (Ctrl+O)", "map*.mul öffnen (Strg+O)"), (s, e) => BrowseAndLoadFile()),
                ImageButton(Properties.Resources.Save, Tip("Save the map (Ctrl+S)", "Karte speichern (Strg+S)"), (s, e) => SaveMap()),
                new ToolStripSeparator(), _tsUndo, _tsRedo, new ToolStripSeparator(),
                _toolButtons[HeightTool.Select], _toolButtons[HeightTool.Raise], _toolButtons[HeightTool.Lower], _toolButtons[HeightTool.Smooth],
                _toolButtons[HeightTool.Flatten], _toolButtons[HeightTool.Pick],
                new ToolStripLabel("r"), _tsRadius, new ToolStripLabel("str"), _tsStrength, new ToolStripSeparator(),
                _tsColor, _tsContour, _tsChanges, _tsGrid, new ToolStripSeparator(),
                TextButton("–", Tip("Zoom out (-)", "Verkleinern (-)"), (s, e) => _view.ZoomStep(-1), true), _tsZoom,
                TextButton("+", Tip("Zoom in (+ or mouse wheel)", "Vergrößern (+ oder Mausrad)"), (s, e) => _view.ZoomStep(1), true),
                TextButton("Fit", Tip("Whole map (0)", "Ganze Karte (0)"), (s, e) => _view.FitAll()),
                TextButton("Sel", Tip("Zoom to the selection", "Auf die Auswahl zoomen"), (s, e) => { if (!_view.Selection.IsEmpty) _view.ZoomTo(_view.Selection); }),
                new ToolStripSeparator(),
                TextButton("?", Tip("Help (F1)", "Hilfe (F1)"), (s, e) => ShowHelp(), true)
            });
        }

        private ToolStripButton ImageButton(Image image, string tip, EventHandler click)
        {
            var b = new ToolStripButton(image) { DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = tip };
            b.Click += click;
            return b;
        }

        private void SetTool(HeightTool tool)
        {
            _view.Tool = tool;
            _view.Invalidate();
            SyncUi();
        }

        private void SyncUi()
        {
            _tsUndo.Enabled = _undo.Count > 0;
            _tsRedo.Enabled = _redo.Count > 0;
            _tsChanges.Checked = _view.ShowChanges;
            _tsGrid.Checked = _view.ShowGrid;
            foreach (KeyValuePair<HeightTool, ToolStripButton> kv in _toolButtons)
            {
                kv.Value.Checked = _view.Tool == kv.Key;
            }
            UpdateZoomLabel();
        }

        private void UpdateZoomLabel()
        {
            float s = _view.ScaleFactor;
            _tsZoom.Text = s >= 1 ? $"{s:0}×" : $"1:{1 / s:0}";
        }

        private void View_HoverTile(object sender, Point p)
        {
            if (p.X < 0 || _map == null)
            {
                statusPos.Text = string.Empty;
                return;
            }
            int z = _map[p.X, p.Y];
            string orig = _original != null && _original[p.X, p.Y] != z ? $" (was {_original[p.X, p.Y]})" : string.Empty;
            statusPos.Text = $"X {p.X}  Y {p.Y}  Z {z}{orig}";
        }

        #endregion

        #region Undo

        private void PushUndo(Rectangle area, sbyte[] before)
        {
            if (area.IsEmpty)
            {
                return;
            }
            _undo.Add((area, before));
            _redo.Clear();
            long total = _undo.Sum(u => (long)u.Data.Length);
            while (_undo.Count > 1 && (total > UndoBudget || _undo.Count > 50))
            {
                total -= _undo[0].Data.Length;
                _undo.RemoveAt(0);
            }
            SyncUi();
        }

        /// <summary>Runs an edit on a region with undo.</summary>
        private void Edit(Rectangle area, string what, Func<int> op)
        {
            if (!CheckMap())
            {
                return;
            }
            area = Rectangle.Intersect(area, _map.Bounds);
            if (area.IsEmpty)
            {
                SetMessage("The area is outside the map.");
                return;
            }
            sbyte[] before = _map.CopyRegion(area);
            Cursor = Cursors.WaitCursor;
            int changed;
            try
            {
                changed = op();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
            if (changed > 0)
            {
                PushUndo(area, before);
            }
            AfterEdit();
            SetMessage($"{what}: {changed:N0} tiles changed ({TargetText}).");
        }

        private void View_BrushEnded(object sender, Rectangle area)
        {
            if (_strokeBackup == null || area.IsEmpty)
            {
                return;
            }
            var before = new sbyte[area.Width * area.Height];
            for (int y = 0; y < area.Height; y++)
            {
                Buffer.BlockCopy(_strokeBackup, (area.Y + y) * _map.Width + area.X, before, y * area.Width, area.Width);
            }
            _strokeBackup = null;
            PushUndo(area, before);
            AfterEdit();
        }

        private void Undo() => Swap(_undo, _redo, "Undo");

        private void Redo() => Swap(_redo, _undo, "Redo");

        private void Swap(List<(Rectangle Area, sbyte[] Data)> from, List<(Rectangle Area, sbyte[] Data)> to, string what)
        {
            if (from.Count == 0 || _map == null)
            {
                return;
            }
            (Rectangle area, sbyte[] data) = from[from.Count - 1];
            from.RemoveAt(from.Count - 1);
            to.Add((area, _map.CopyRegion(area)));
            _map.PasteRegion(area, data);
            AfterEdit();
            SyncUi();
            SetMessage($"{what} ({area.Width}×{area.Height} at {area.X}, {area.Y}).");
        }

        private void AfterEdit()
        {
            _view.InvalidateMap();
            UpdateInfo();
            SyncUi();
        }

        #endregion

        #region Map tab (load / save)

        private TextBox _txtMapFile, _txtStaIdx, _txtStatics, _txtOutput;
        private NumericUpDown _numBlockHeight;
        private Label _lblDetected, _lblSource;
        private ComboBox _comboLoaded;
        private CheckBox _chkStatics;
        private RadioButton _radioOutput, _radioOverwrite;

        private void BuildSourceTab()
        {
            _txtMapFile = In(240);
            _numBlockHeight = Num(1, 8192, 512, 70);
            toolTip1.SetToolTip(_numBlockHeight, Tip("Map height in blocks (tiles / 8): 512 Felucca / Trammel / Ter Mur, 200 Ilshenar, 256 Malas, 181 Tokuno.",
                "Kartenhöhe in Blöcken (Kacheln / 8): 512 Felucca / Trammel / Ter Mur, 200 Ilshenar, 256 Malas, 181 Tokuno."));
            _lblDetected = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 3, 3, 0) };
            _txtMapFile.TextChanged += (s, e) => DetectSize();
            GroupBox file = Group("Map file (map*.mul) – like the original tool",
                Row(_txtMapFile, Btn("...", (s, e) => BrowseMapFile(), width: 30)),
                Row(Lbl("Height in blocks:"), _numBlockHeight, Btn("Load", (s, e) => LoadFile(), Tip("Reads all land altitudes.", "Liest alle Landhöhen."))),
                _lblDetected);

            _comboLoaded = Combo(170, MapNames.Cast<object>().ToArray());
            GroupBox loaded = Group("Map loaded in UoFiddler (also UOP)",
                Row(_comboLoaded, Btn("Load", (s, e) => LoadFromUoFiddler(), Tip(
                    "Reads the map of the client files that UoFiddler uses (map*.mul or map*LegacyMUL.uop). Saving writes a new map*.mul.",
                    "Liest die Karte der Client-Dateien, die UoFiddler benutzt (map*.mul oder map*LegacyMUL.uop). Speichern schreibt eine neue map*.mul."))));

            _txtStaIdx = In(240);
            _txtStatics = In(240);
            _chkStatics = Chk("Move statics with the land when saving", true);
            toolTip1.SetToolTip(_chkStatics, Tip("Every static gets the same height change as the land under it (original: optional staticsX.mul / staidxX.mul).",
                "Jedes Static bekommt dieselbe Höhenänderung wie das Land darunter (Original: optional staticsX.mul / staidxX.mul)."));
            GroupBox statics = Group("Statics (optional)",
                _chkStatics,
                Row(Lbl("staidx:", 50), _txtStaIdx, Btn("...", (s, e) => { string f = BrowseOpen(this, "staidx*.mul|staidx*.mul|All|*.*", _txtStaIdx.Text); if (f != null) _txtStaIdx.Text = f; }, width: 30)),
                Row(Lbl("statics:", 50), _txtStatics, Btn("...", (s, e) => { string f = BrowseOpen(this, "statics*.mul|statics*.mul|All|*.*", _txtStatics.Text); if (f != null) _txtStatics.Text = f; }, width: 30)));

            _txtOutput = In(240);
            _txtOutput.Text = UoFiddler.Controls.Classes.Options.OutputPath;
            _radioOutput = new RadioButton { Text = "Write into the output folder", AutoSize = true, Checked = true, Margin = new Padding(3, 4, 3, 0) };
            _radioOverwrite = new RadioButton { Text = "Overwrite the source file (with .bak backup)", AutoSize = true, Margin = new Padding(3, 2, 3, 0) };
            _lblSource = new Label { AutoSize = true, MaximumSize = new Size(320, 0), ForeColor = Color.DimGray, Margin = new Padding(3, 3, 3, 0) };
            GroupBox save = Group("Save",
                _radioOutput,
                Row(_txtOutput, Btn("...", (s, e) => BrowseOutput(), width: 30)),
                _radioOverwrite,
                Row(Btn("Save map (Ctrl+S)", (s, e) => SaveMap()), Btn("Revert all", (s, e) => RevertAll(), Tip("Back to the loaded state.", "Zurück zum geladenen Stand."))),
                _lblSource);
            FillTab(tabSource, file, loaded, statics, save);
        }

        private void BrowseMapFile()
        {
            string f = BrowseOpen(this, "Map files (map*.mul)|map*.mul|All files|*.*", _txtMapFile.Text);
            if (f != null)
            {
                _txtMapFile.Text = f;
            }
        }

        private void BrowseAndLoadFile()
        {
            string f = BrowseOpen(this, "Map files (map*.mul)|map*.mul|All files|*.*", _txtMapFile.Text);
            if (f == null)
            {
                return;
            }
            _txtMapFile.Text = f;
            LoadFile();
        }

        private void BrowseOutput()
        {
            using (var d = new FolderBrowserDialog { SelectedPath = _txtOutput.Text })
            {
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    _txtOutput.Text = d.SelectedPath;
                }
            }
        }

        private void DetectSize()
        {
            try
            {
                string f = _txtMapFile.Text.Trim();
                if (!File.Exists(f))
                {
                    _lblDetected.Text = string.Empty;
                    return;
                }
                long length = new FileInfo(f).Length;
                int h = HeightMap.GuessBlockHeight(length, out string name);
                if (h > 0)
                {
                    _numBlockHeight.Value = h;
                    long blocks = length / HeightMap.BlockBytes;
                    _lblDetected.Text = $"Detected: {name}, {blocks / h * 8} × {h * 8} tiles";
                }
                else
                {
                    _lblDetected.Text = length % HeightMap.BlockBytes != 0
                        ? "⚠ size is not a multiple of 196 bytes – not a map*.mul (UOP? use \"Map loaded in UoFiddler\")"
                        : "Size unknown – enter the height in blocks.";
                }
                // statics next to the map
                string dir = Path.GetDirectoryName(f);
                string num = new string(Path.GetFileNameWithoutExtension(f).Where(char.IsDigit).ToArray());
                string idx = Path.Combine(dir, $"staidx{num}.mul"), sta = Path.Combine(dir, $"statics{num}.mul");
                if (File.Exists(idx) && File.Exists(sta))
                {
                    _txtStaIdx.Text = idx;
                    _txtStatics.Text = sta;
                }
            }
            catch (Exception)
            {
                _lblDetected.Text = string.Empty;
            }
        }

        private bool ConfirmDiscard()
        {
            return !IsDirty || MessageBox.Show(this, "The current map has unsaved changes. Discard them?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
        }

        private async void LoadFile()
        {
            string f = _txtMapFile.Text.Trim();
            if (!File.Exists(f))
            {
                SetMessage("Choose a map*.mul first.");
                return;
            }
            if (_busy || !ConfirmDiscard())
            {
                return;
            }
            int h = (int)_numBlockHeight.Value;
            try
            {
                HeightMap map = await RunBusy("Reading " + Path.GetFileName(f), (p, t) => HeightMap.ReadMul(f, h, p, t));
                SetMap(map, f, -1, f);
            }
            catch (Exception ex)
            {
                ShowError("Reading", ex);
            }
        }

        private async void LoadFromUoFiddler()
        {
            if (_busy || !ConfirmDiscard())
            {
                return;
            }
            int index = _comboLoaded.SelectedIndex;
            try
            {
                Ultima.Map source = Ultima.Map.GetBuiltIn(index);
                if (source == null)
                {
                    SetMessage("This map is not available.");
                    return;
                }
                HeightMap map = await RunBusy("Reading " + MapNames[index], (p, t) =>
                {
                    lock (SdkSync.Lock)
                    {
                        return HeightMap.ReadLoaded(source, p, t);
                    }
                });
                string mapPath = source.Tiles.MapPath;
                string patch = !string.IsNullOrEmpty(mapPath) && mapPath.EndsWith(".mul", StringComparison.OrdinalIgnoreCase) && File.Exists(mapPath) ? mapPath : null;
                SetMap(map, null, index, patch);
                _txtMapFile.Text = patch ?? string.Empty;
                string idx = Ultima.Files.GetFilePath($"staidx{source.FileIndex}.mul"), sta = Ultima.Files.GetFilePath($"statics{source.FileIndex}.mul");
                _txtStaIdx.Text = idx ?? string.Empty;
                _txtStatics.Text = sta ?? string.Empty;
            }
            catch (Exception ex)
            {
                ShowError("Reading", ex);
            }
        }

        private void SetMap(HeightMap map, string sourceFile, int mapIndex, string patchFile)
        {
            _map = map;
            _original = map.Clone();
            _sourceFile = sourceFile;
            _sourceMapIndex = mapIndex;
            _patchFile = patchFile;
            _undo.Clear();
            _redo.Clear();
            _view.Original = _original;
            _view.Map = map;
            _radioOverwrite.Enabled = sourceFile != null;
            if (sourceFile == null)
            {
                _radioOutput.Checked = true;
            }
            string from = sourceFile != null ? Path.GetFileName(sourceFile) : $"UoFiddler {MapNames[mapIndex]}" + (patchFile == null ? " (UOP – saving writes a new map*.mul)" : string.Empty);
            _lblSource.Text = "Loaded: " + from;
            Text = $"Altitude Tool – {from}";
            _numImportX.Maximum = _numImportY.Maximum = Math.Max(map.Width, map.Height);
            AfterEdit();
            SetMessage($"Loaded {map.Width} × {map.Height} tiles ({map.BlockWidth} × {map.BlockHeight} blocks).");
        }

        private string MapFileName => _sourceFile != null ? Path.GetFileName(_sourceFile) : $"map{Math.Max(0, Ultima.Map.GetBuiltIn(Math.Max(0, _sourceMapIndex))?.FileIndex ?? 0)}.mul";

        private async void SaveMap()
        {
            if (!CheckMap() || _busy)
            {
                return;
            }
            bool overwrite = _radioOverwrite.Checked && _sourceFile != null;
            string folder = overwrite ? Path.GetDirectoryName(_sourceFile) : _txtOutput.Text.Trim();
            if (string.IsNullOrEmpty(folder))
            {
                SetMessage("Choose an output folder.");
                return;
            }
            string target = Path.Combine(folder, MapFileName);
            bool statics = _chkStatics.Checked && File.Exists(_txtStaIdx.Text) && File.Exists(_txtStatics.Text);
            int changedTiles = _map.CountDifferences(_original);
            string question = $"Write {changedTiles:N0} changed tiles into\n{target}" + (overwrite ? "\n(a .bak copy of the old file is made)" : string.Empty)
                + (statics ? $"\nand move the statics in\n{Path.Combine(folder, Path.GetFileName(_txtStatics.Text))}" : _chkStatics.Checked ? "\n(statics files not found – statics stay)" : string.Empty) + "?";
            if (MessageBox.Show(this, question, Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }
            string staidx = _txtStaIdx.Text, staticsFile = _txtStatics.Text;
            HeightMap map = _map, original = _original;
            string patch = _patchFile;
            try
            {
                (int tiles, int moved) = await RunBusy("Saving", (p, t) =>
                {
                    Directory.CreateDirectory(folder);
                    if (overwrite)
                    {
                        File.Copy(target, target + ".bak", true);
                    }
                    int changed = map.WriteMap(target, patch, p, t);
                    int m = 0;
                    if (statics)
                    {
                        string targetStatics = Path.Combine(folder, Path.GetFileName(staticsFile));
                        string targetIdx = Path.Combine(folder, Path.GetFileName(staidx));
                        if (overwrite && File.Exists(targetStatics))
                        {
                            File.Copy(targetStatics, targetStatics + ".bak", true);
                        }
                        if (!string.Equals(Path.GetFullPath(staidx), Path.GetFullPath(targetIdx), StringComparison.OrdinalIgnoreCase))
                        {
                            File.Copy(staidx, targetIdx, true);
                        }
                        m = map.ShiftStatics(original, staidx, staticsFile, targetStatics, p, t);
                    }
                    return (changed, m);
                });
                _original = _map.Clone();
                _view.Original = _original;
                _undo.Clear();
                _redo.Clear();
                if (_sourceFile != null && overwrite)
                {
                    _patchFile = target;
                }
                AfterEdit();
                SetMessage($"Saved {target}: {tiles:N0} tiles changed" + (statics ? $", {moved:N0} statics moved" : string.Empty) + ". Reload the map in UoFiddler to see it there.");
            }
            catch (Exception ex)
            {
                ShowError("Saving", ex);
            }
        }

        private void RevertAll()
        {
            if (!CheckMap() || !IsDirty)
            {
                return;
            }
            if (MessageBox.Show(this, "Throw away all changes since loading / saving?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            PushUndo(_map.Bounds, (sbyte[])_map.Z.Clone());
            Buffer.BlockCopy(_original.Z, 0, _map.Z, 0, _map.Z.Length);
            AfterEdit();
            SetMessage("All changes reverted (undo is possible).");
        }

        #endregion

        #region Extract tab

        private ComboBox _comboExportArea, _comboExportFormat;

        private void BuildExportTab()
        {
            _comboExportArea = Combo(200, "Whole map", "Selection", "Visible view");
            _comboExportFormat = Combo(260,
                "BMP 8 bit gray (z + 128) – AltitudeTool",
                "PNG 8 bit gray (z + 128)",
                "PNG as shown (colors, shade, lines)",
                "RAW 8 bit (z + 128, no header)");
            toolTip1.SetToolTip(_comboExportFormat, Tip(
                "The gray formats are exactly what the original \"extract\" wrote: 1 pixel per tile, gray = z + 128 (z 0 = 128). Edit them in any image program and load them back with Apply.",
                "Die Grauformate sind genau das, was das Original-\"extract\" schrieb: 1 Pixel pro Kachel, Grau = z + 128 (z 0 = 128). In jedem Malprogramm bearbeiten und mit Apply zurückladen."));
            GroupBox export = Group("Extract heights → image (original \"extract\")",
                Row(Lbl("Area:", 55), _comboExportArea),
                Row(Lbl("Format:", 55), _comboExportFormat),
                Row(Btn("Save image...", (s, e) => ExportImage()), Btn("Copy as shown", (s, e) => CopyView())));
            var info = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(330, 0),
                ForeColor = Color.DimGray,
                Text = "Gray value = z + 128:\n  black 0 = z −128 · 123 = z −5 (sea) · 128 = z 0 · 255 = z 127\n\n" +
                       "Grauwert = z + 128:\n  schwarz 0 = z −128 · 123 = z −5 (Meer) · 128 = z 0 · 255 = z 127"
            };
            FillTab(tabExport, export, info);
        }

        private Rectangle ExportArea()
        {
            switch (_comboExportArea.SelectedIndex)
            {
                case 1:
                    return _view.Selection;
                case 2:
                {
                    Point a = _view.ScreenToMap(Point.Empty), b = _view.ScreenToMap(new Point(_view.Width, _view.Height));
                    return Rectangle.Intersect(Rectangle.FromLTRB(a.X, a.Y, b.X + 1, b.Y + 1), _map.Bounds);
                }
                default:
                    return _map.Bounds;
            }
        }

        private void ExportImage()
        {
            if (!CheckMap())
            {
                return;
            }
            Rectangle r = ExportArea();
            if (r.IsEmpty)
            {
                SetMessage("Nothing selected – choose \"Whole map\" or select an area.");
                return;
            }
            int format = _comboExportFormat.SelectedIndex;
            string filter = format == 0 ? "BMP|*.bmp" : format == 3 ? "RAW|*.raw" : "PNG|*.png";
            string name = Path.GetFileNameWithoutExtension(MapFileName) + (r == _map.Bounds ? string.Empty : $"_{r.X}_{r.Y}_{r.Width}x{r.Height}") + "_height";
            using (var d = new SaveFileDialog { Filter = filter, FileName = name })
            {
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    Cursor = Cursors.WaitCursor;
                    if (format == 3)
                    {
                        _map.SaveRaw(d.FileName, r);
                    }
                    else if (format == 2)
                    {
                        using (Bitmap bmp = RenderArea(r))
                        {
                            bmp.Save(d.FileName, ImageFormat.Png);
                        }
                    }
                    else
                    {
                        using (Bitmap bmp = _map.ToGrayBitmap(r))
                        {
                            bmp.Save(d.FileName, format == 0 ? ImageFormat.Bmp : ImageFormat.Png);
                        }
                    }
                    SetMessage($"Saved {r.Width} × {r.Height} (from {r.X}, {r.Y}): {d.FileName}" + (r == _map.Bounds ? string.Empty : $" – apply it back at X {r.X}, Y {r.Y}."));
                }
                catch (Exception ex)
                {
                    ShowError("Saving the image", ex);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private Bitmap RenderArea(Rectangle r)
        {
            var bmp = new Bitmap(r.Width, r.Height, PixelFormat.Format32bppArgb);
            int min = 0, max = 0;
            if (_view.ColorMode == HeightColorMode.GrayStretch)
            {
                _map.Histogram(_map.Bounds, out min, out max, out _);
            }
            HeightOps.Render(_map, r, bmp, _view.ColorMode, HeightOps.MakePalette(_view.ColorMode, min, max), _view.Contour, null);
            return bmp;
        }

        private void CopyView()
        {
            if (!CheckMap())
            {
                return;
            }
            Rectangle r = ExportArea();
            if (r.IsEmpty)
            {
                return;
            }
            if ((long)r.Width * r.Height > 4096L * 4096)
            {
                SetMessage("Too large for the clipboard – save it as a file.");
                return;
            }
            using (Bitmap bmp = RenderArea(r))
            {
                Class.GraphicCutter.CutterImaging.CopyToClipboard(bmp);
            }
            SetMessage("Copied to the clipboard.");
        }

        #endregion

        #region Apply tab

        private TextBox _txtImport;
        private NumericUpDown _numImportX, _numImportY;
        private CheckBox _chkRelative;
        private Label _lblImport;

        private void BuildImportTab()
        {
            _txtImport = In(240);
            _numImportX = Num(0, 65535, 0, 70);
            _numImportY = Num(0, 65535, 0, 70);
            _chkRelative = Chk("Relative: add (gray − 128) to the current height");
            toolTip1.SetToolTip(_chkRelative, Tip("Off = the image sets the height (original \"apply\"). On = the image raises / lowers (gray 128 = no change).",
                "Aus = das Bild setzt die Höhe (Original-\"apply\"). An = das Bild hebt / senkt (Grau 128 = keine Änderung)."));
            _lblImport = new Label { AutoSize = true, MaximumSize = new Size(330, 0), ForeColor = Color.DimGray, Margin = new Padding(3, 3, 3, 0) };
            GroupBox apply = Group("Apply image → heights (original \"apply\")",
                Row(_txtImport, Btn("...", (s, e) => { string f = BrowseOpen(this, "Images|*.bmp;*.png;*.tif;*.tiff;*.gif;*.jpg|All|*.*", _txtImport.Text); if (f != null) _txtImport.Text = f; }, width: 30)),
                Row(Lbl("At X:"), _numImportX, Lbl("Y:"), _numImportY, Btn("= selection", (s, e) => { if (!_view.Selection.IsEmpty) { _numImportX.Value = _view.Selection.X; _numImportY.Value = _view.Selection.Y; } })),
                _chkRelative,
                Row(Btn("Apply", (s, e) => ApplyImage(), Tip("Into memory with undo – \"Save map\" writes it into the file.", "In den Speicher mit Undo – \"Save map\" schreibt es in die Datei."))),
                _lblImport);
            var info = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(330, 0),
                ForeColor = Color.DimGray,
                Text = "• z = gray − 128. Color images use their brightness.\n• The image may be smaller than the map: it is put at X / Y (the original needed the full size).\n• Transparent pixels (PNG) keep the map height – edit only parts.\n• The statics are moved when you save (tab Map).\n\n" +
                       "• z = Grau − 128. Farbbilder benutzen ihre Helligkeit.\n• Das Bild darf kleiner als die Karte sein: es kommt an X / Y (das Original brauchte die volle Größe).\n• Transparente Pixel (PNG) behalten die Kartenhöhe – nur Teile bearbeiten.\n• Die Statics werden beim Speichern mitbewegt (Reiter Map)."
            };
            FillTab(tabImport, apply, info);
        }

        private void ApplyImage()
        {
            if (!CheckMap())
            {
                return;
            }
            string f = _txtImport.Text.Trim();
            if (!File.Exists(f))
            {
                SetMessage("Choose an image first.");
                return;
            }
            sbyte[] z;
            int w, h;
            bool[] skip;
            try
            {
                Cursor = Cursors.WaitCursor;
                z = HeightMap.ReadHeightImage(f, out w, out h, out skip);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                ShowError("Reading the image", ex);
                return;
            }
            Cursor = Cursors.Default;
            int x = (int)_numImportX.Value, y = (int)_numImportY.Value;
            if (x + w > _map.Width || y + h > _map.Height)
            {
                if (MessageBox.Show(this, $"The image ({w} × {h}) at {x}, {y} goes beyond the map ({_map.Width} × {_map.Height}). Apply the part that fits?", Text,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }
            }
            bool relative = _chkRelative.Checked;
            _lblImport.Text = $"{Path.GetFileName(f)}: {w} × {h}" + (skip != null ? ", with transparent parts" : string.Empty);
            Edit(new Rectangle(x, y, w, h), "Apply image", () => _map.Import(z, w, h, skip, x, y, relative));
            _view.Selection = Rectangle.Intersect(new Rectangle(x, y, w, h), _map.Bounds);
        }

        #endregion

        #region Modify tab

        private NumericUpDown _numOffset, _numSetZ, _numSmoothRadius, _numSmoothPasses, _numScale, _numScaleBase, _numMin, _numMax, _numTerrace,
            _numNoiseAmp, _numNoiseSize, _numNoiseSeed, _numFlatten;
        private Label _lblTarget;

        private void BuildEditTab()
        {
            _lblTarget = new Label { AutoSize = true, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(3, 3, 3, 3) };
            _numOffset = Num(-255, 255, 5);
            _numSetZ = Num(-128, 127, 0);
            _numSmoothRadius = Num(1, 32, 1);
            _numSmoothPasses = Num(1, 20, 1);
            _numScale = Num(0, 5, 1.5m, 60, 2);
            _numScaleBase = Num(-128, 127, 0);
            _numMin = Num(-128, 127, -5);
            _numMax = Num(-128, 127, 100);
            _numTerrace = Num(2, 40, 5);
            _numNoiseAmp = Num(1, 60, 4);
            _numNoiseSize = Num(2, 256, 16);
            _numNoiseSeed = Num(0, 99999, 1, 70);
            _numFlatten = Num(-128, 127, 0);
            _numFlatten.ValueChanged += (s, e) => _view.FlattenZ = (int)_numFlatten.Value;

            GroupBox target = Group("Target", _lblTarget,
                Row(Btn("Select all (Ctrl+A)", (s, e) => SelectAll()), Btn("No selection (Esc)", (s, e) => _view.Selection = Rectangle.Empty)));
            GroupBox modify = Group("Raise / lower (original \"modify\")",
                Row(Lbl("Offset z:", 70), _numOffset, Btn("Apply", (s, e) => Edit(Target, $"Offset {_numOffset.Value:+0;-0}", () => HeightOps.Offset(_map, Target, (int)_numOffset.Value)))),
                Lbl("Values are kept within −128 … 127 (like the original)."));
            GroupBox tools = Group("More",
                Row(Lbl("Set to z:", 70), _numSetZ, Btn("Apply", (s, e) => Edit(Target, $"Set to {_numSetZ.Value}", () => HeightOps.SetTo(_map, Target, (int)_numSetZ.Value)))),
                Row(Lbl("Smooth r:", 70), _numSmoothRadius, Lbl("×"), _numSmoothPasses,
                    Btn("Apply", (s, e) => Edit(Target, "Smooth", () => HeightOps.Smooth(_map, Target, (int)_numSmoothRadius.Value, (int)_numSmoothPasses.Value)))),
                Row(Lbl("Scale:", 70), _numScale, Lbl("around z"), _numScaleBase,
                    Btn("Apply", (s, e) => Edit(Target, $"Scale {_numScale.Value}", () => HeightOps.Scale(_map, Target, (double)_numScale.Value, (int)_numScaleBase.Value)))),
                Row(Lbl("Limit:", 70), _numMin, Lbl("…"), _numMax,
                    Btn("Apply", (s, e) => Edit(Target, "Limit", () => HeightOps.ClampRange(_map, Target, (int)_numMin.Value, (int)_numMax.Value)))),
                Row(Lbl("Terraces:", 70), _numTerrace, Btn("Apply", (s, e) => Edit(Target, "Terraces", () => HeightOps.Terrace(_map, Target, (int)_numTerrace.Value)), Tip(
                    "Rounds to steps (e.g. 5) – plateaus and cliffs.", "Rundet auf Stufen (z. B. 5) – Plateaus und Klippen."))),
                Row(Lbl("Hills ±:", 70), _numNoiseAmp, Lbl("size"), _numNoiseSize),
                Row(Lbl("seed", 70), _numNoiseSeed, Btn("Apply", (s, e) => Edit(Target, "Hills", () => HeightOps.Noise(_map, Target, (int)_numNoiseAmp.Value, (int)_numNoiseSize.Value, (int)_numNoiseSeed.Value)), Tip(
                    "Adds natural random hills.", "Fügt natürliche Zufallshügel hinzu."))));
            GroupBox brush = Group("Brushes (toolbar: Raise, Lower, Smooth, Flatten)",
                Row(Lbl("Flatten to z:", 80), _numFlatten, Lbl("(pipette I picks it)")),
                Lbl("Radius and strength: toolbar (r / str).\nRight / middle mouse = move, wheel = zoom."));
            FillTab(tabEdit, target, modify, tools, brush);
        }

        private void SelectAll()
        {
            if (CheckMap())
            {
                _view.Selection = _map.Bounds;
            }
        }

        #endregion

        #region Info tab

        private Label _lblMapInfo, _lblSelInfo;
        private Panel _panelHistogram;
        private NumericUpDown _numGoX, _numGoY, _numSelX, _numSelY, _numSelW, _numSelH;
        private int[] _histogram;
        private bool _selUpdating;

        private void BuildInfoTab()
        {
            _lblMapInfo = new Label { AutoSize = true, MaximumSize = new Size(330, 0), Margin = new Padding(3) };
            _lblSelInfo = new Label { AutoSize = true, MaximumSize = new Size(330, 0), Margin = new Padding(3) };
            _panelHistogram = new Panel { Width = 330, Height = 140, BackColor = Color.FromArgb(45, 45, 48), Margin = new Padding(3) };
            _panelHistogram.Paint += PanelHistogram_Paint;
            toolTip1.SetToolTip(_panelHistogram, Tip("How many tiles have which altitude (−128 left … 127 right) in the selection or map. Green line = z 0.",
                "Wie viele Kacheln welche Höhe haben (−128 links … 127 rechts) in Auswahl oder Karte. Grüne Linie = z 0."));
            _numGoX = Num(0, 65535, 1323, 70);
            _numGoY = Num(0, 65535, 1624, 70);
            _numSelX = Num(0, 65535, 0, 65); _numSelY = Num(0, 65535, 0, 65); _numSelW = Num(0, 65535, 0, 65); _numSelH = Num(0, 65535, 0, 65);
            foreach (NumericUpDown n in new[] { _numSelX, _numSelY, _numSelW, _numSelH })
            {
                n.ValueChanged += (s, e) =>
                {
                    if (!_selUpdating && _map != null)
                    {
                        _view.Selection = new Rectangle((int)_numSelX.Value, (int)_numSelY.Value, (int)_numSelW.Value, (int)_numSelH.Value);
                    }
                };
            }
            GroupBox map = Group("Map", _lblMapInfo);
            GroupBox sel = Group("Selection / histogram", _lblSelInfo, _panelHistogram,
                Row(Lbl("X"), _numSelX, Lbl("Y"), _numSelY),
                Row(Lbl("W"), _numSelW, Lbl("H"), _numSelH, Btn("Zoom", (s, e) => { if (!_view.Selection.IsEmpty) _view.ZoomTo(_view.Selection); })));
            GroupBox go = Group("Go to",
                Row(Lbl("X"), _numGoX, Lbl("Y"), _numGoY, Btn("Go", (s, e) => GoTo())),
                Row(Btn("Next change", (s, e) => NextChange(), Tip("Jumps to the next changed 8 × 8 block.", "Springt zum nächsten geänderten 8 × 8 Block."))));
            FillTab(tabInfo, map, sel, go);
        }

        private void SelectionChanged()
        {
            Rectangle s = _view.Selection;
            _lblTarget.Text = _map == null ? "no map" : "Target: " + TargetText;
            _selUpdating = true;
            _numSelX.Value = s.X;
            _numSelY.Value = s.Y;
            _numSelW.Value = s.Width;
            _numSelH.Value = s.Height;
            _selUpdating = false;
            statusInfo.Text = s.IsEmpty ? string.Empty : $"selection {s.X}, {s.Y}  {s.Width} × {s.Height}";
            UpdateSelectionInfo();
        }

        private void UpdateInfo()
        {
            if (_map == null)
            {
                _lblMapInfo.Text = "no map";
                return;
            }
            _map.Histogram(_map.Bounds, out int min, out int max, out double avg);
            int changed = _original != null ? _map.CountDifferences(_original) : 0;
            _lblMapInfo.Text = $"{_map.Width} × {_map.Height} tiles, {_map.BlockWidth} × {_map.BlockHeight} blocks\n" +
                               $"z {min} … {max}, average {avg:0.0}\n" +
                               $"changed (not saved): {changed:N0} tiles";
            UpdateSelectionInfo();
        }

        private void UpdateSelectionInfo()
        {
            if (_map == null)
            {
                _histogram = null;
                _lblSelInfo.Text = string.Empty;
                _panelHistogram.Invalidate();
                return;
            }
            Rectangle r = Target;
            _histogram = _map.Histogram(r, out int min, out int max, out double avg);
            _lblSelInfo.Text = $"{TargetText}: z {min} … {max}, average {avg:0.0}";
            _panelHistogram.Invalidate();
        }

        private void PanelHistogram_Paint(object sender, PaintEventArgs e)
        {
            if (_histogram == null)
            {
                return;
            }
            Rectangle r = Rectangle.Inflate(_panelHistogram.ClientRectangle, -4, -4);
            double max = Math.Log(1 + _histogram.Max());
            float w = r.Width / 256f;
            using (var b = new SolidBrush(Color.FromArgb(120, 170, 230)))
            {
                for (int i = 0; i < 256; i++)
                {
                    if (_histogram[i] == 0)
                    {
                        continue;
                    }
                    int h = (int)(r.Height * Math.Log(1 + _histogram[i]) / Math.Max(1e-9, max));
                    e.Graphics.FillRectangle(b, r.X + i * w, r.Bottom - h, Math.Max(1, w), h);
                }
            }
            using (var pen = new Pen(Color.LightGreen))
            {
                float x0 = r.X + 128 * w;
                e.Graphics.DrawLine(pen, x0, r.Top, x0, r.Bottom);
            }
        }

        private void GoTo()
        {
            if (!CheckMap())
            {
                return;
            }
            int x = (int)_numGoX.Value, y = (int)_numGoY.Value;
            if (_view.ScaleFactor < 4)
            {
                _view.ZoomTo(new Rectangle(x - 32, y - 32, 64, 64));
            }
            else
            {
                _view.CenterOn(x, y);
            }
        }

        private void NextChange()
        {
            if (!CheckMap())
            {
                return;
            }
            List<Rectangle> blocks = _map.ChangedBlocks(_original);
            if (blocks.Count == 0)
            {
                SetMessage("No changes.");
                return;
            }
            Point c = _view.ScreenToMap(new Point(_view.Width / 2, _view.Height / 2));
            int current = (c.X >> 3) * _map.BlockHeight + (c.Y >> 3);
            Rectangle next = blocks.FirstOrDefault(b => (b.X >> 3) * _map.BlockHeight + (b.Y >> 3) > current);
            if (next.IsEmpty)
            {
                next = blocks[0];
            }
            _view.ZoomTo(Rectangle.Inflate(next, 24, 24));
            SetMessage($"{blocks.Count} changed blocks – showing {next.X}, {next.Y}.");
        }

        #endregion

        #region Keyboard, help, closing

        private void ShowHelp() => AltitudeHelp.Show(this, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de");

        private bool IsTextInputFocused()
        {
            Control c = ActiveControl;
            while (c is ContainerControl cc && cc.ActiveControl != null)
            {
                c = cc.ActiveControl;
            }
            return c is TextBoxBase || c is UpDownBase || c?.Parent is UpDownBase || c is ComboBox;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_busy)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
            Keys key = keyData & Keys.KeyCode;
            bool ctrl = (keyData & Keys.Control) == Keys.Control;
            if (keyData == Keys.F1)
            {
                ShowHelp();
                return true;
            }
            if (ctrl)
            {
                switch (key)
                {
                    case Keys.O: BrowseAndLoadFile(); return true;
                    case Keys.S: SaveMap(); return true;
                }
                if (!IsTextInputFocused())
                {
                    switch (key)
                    {
                        case Keys.Z: Undo(); return true;
                        case Keys.Y: Redo(); return true;
                        case Keys.A: SelectAll(); return true;
                    }
                }
                return base.ProcessCmdKey(ref msg, keyData);
            }
            if (IsTextInputFocused() || (keyData & Keys.Alt) == Keys.Alt)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
            switch (key)
            {
                case Keys.S: SetTool(HeightTool.Select); return true;
                case Keys.R: SetTool(HeightTool.Raise); return true;
                case Keys.L: SetTool(HeightTool.Lower); return true;
                case Keys.M: SetTool(HeightTool.Smooth); return true;
                case Keys.F: SetTool(HeightTool.Flatten); return true;
                case Keys.I: SetTool(HeightTool.Pick); return true;
                case Keys.Oemplus: case Keys.Add: _view.ZoomStep(1); return true;
                case Keys.OemMinus: case Keys.Subtract: _view.ZoomStep(-1); return true;
                case Keys.D0: case Keys.NumPad0: _view.FitAll(); return true;
                case Keys.Escape: _view.Selection = Rectangle.Empty; return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_busy)
            {
                e.Cancel = MessageBox.Show(this, "Work is still running. Cancel it and close?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes;
                if (!e.Cancel)
                {
                    _cts?.Cancel();
                }
            }
            else if (IsDirty)
            {
                e.Cancel = MessageBox.Show(this, "The map has unsaved changes. Close anyway?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes;
            }
            base.OnFormClosing(e);
        }

        #endregion
    }
}

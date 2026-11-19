/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// Photoshop like "free transform" for UO art without blur: flip, rotate, skew, scale, move, distort
    /// (4 corners) and warp (3x3 points). Every pixel of the result is exactly one pixel of the source
    /// (nearest neighbour), colours never change. All steps are kept in a list and computed from the original,
    /// so a flip does not reset a skew. Undo / redo, grid with points, and the transformation of an image is
    /// remembered: loading the same image again applies it automatically.
    /// </summary>
    public sealed class ArtTransformForm : Form
    {
        private enum PointMode
        {
            Off,
            Distort,
            Warp
        }

        private readonly Action<int, Bitmap> _insertCallback;
        private readonly TransformMemory _memory =
            new TransformMemory(Path.Combine(Options.AppDataPath ?? Path.GetTempPath(), "ArtTransformMemory.json"));

        private ArtPixelImage _source;
        private int _sourceId = -1;
        private string _sourceName = string.Empty;
        private TransformSettings _settings = new TransformSettings();
        private ArtPixelImage _result;

        private readonly List<TransformSettings> _undo = new List<TransformSettings>();
        private readonly List<TransformSettings> _redo = new List<TransformSettings>();
        private string _lastEditKey;
        private DateTime _lastEditTime;

        private int _editIndex = -1;  // step that the numeric fields / points edit
        private PointMode _pointMode = PointMode.Off;
        private bool _programmatic;

        // controls
        private PixelCanvas _canvas;
        private NumericUpDown _skewX, _skewY, _rotate, _scaleX, _scaleY, _moveX, _moveY;
        private NumericUpDown _canvasW, _canvasH;
        private CheckBox _keepBottom;
        private RadioButton _pointsOff, _pointsDistort, _pointsWarp;
        private ListBox _stepList;
        private NumericUpDown _zoom, _gridSize;
        private ComboBox _gridMode;
        private CheckBox _ghost;
        private TextBox _idBox;
        private CheckBox _rememberCheck;
        private Label _status, _memoryLabel, _hoverLabel;
        private Button _undoButton, _redoButton;

        public ArtTransformForm(Action<int, Bitmap> insertCallback)
        {
            _insertCallback = insertCallback;

            Text = "Art Transform - skew / distort / warp (pixel exact)";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1250, 820);
            MinimumSize = new Size(900, 600);
            KeyPreview = true;
            KeyDown += OnKeyDown;

            BuildUi();
        }

        #region UI
        private void BuildUi()
        {
            _programmatic = true;

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3 };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(root);

            // --- top bar
            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            top.Controls.Add(Btn("Load item ID...", (s, e) => LoadFromItemId()));
            top.Controls.Add(Btn("Load image...", (s, e) => LoadFromFile()));
            top.Controls.Add(Btn("Paste (Ctrl+V)", (s, e) => LoadFromClipboard()));
            top.Controls.Add(Lbl("  "));
            _undoButton = Btn("↶ Undo (Ctrl+Z)", (s, e) => Undo());
            _redoButton = Btn("↷ Redo (Ctrl+Y)", (s, e) => Redo());
            top.Controls.Add(_undoButton);
            top.Controls.Add(_redoButton);
            top.Controls.Add(Btn("Reset all", (s, e) => Commit("reset", () => { _settings.Steps.Clear(); _editIndex = -1; }, false)));
            top.Controls.Add(Lbl("   ID:"));
            _idBox = new TextBox { Width = 80 };
            top.Controls.Add(_idBox);
            top.Controls.Add(Btn("Insert into Art", (s, e) => InsertIntoArt()));
            top.Controls.Add(Btn("Copy", (s, e) => CopyResult(Color.Black)));
            top.Controls.Add(Btn("Copy transparent", (s, e) => CopyResult(Color.Transparent)));
            top.Controls.Add(Btn("PNG...", (s, e) => SavePng()));
            _rememberCheck = new CheckBox { Text = "Remember && auto-apply", AutoSize = true, Checked = true, Margin = new Padding(10, 7, 3, 3) };
            new ToolTip().SetToolTip(_rememberCheck,
                "Insert remembers the transformation of this image. Loading the same image (same item id, or the same" + Environment.NewLine +
                "picture from a file / the clipboard) applies it again automatically.");
            top.Controls.Add(_rememberCheck);
            top.Controls.Add(Btn("Remember now", (s, e) => Remember(true)));
            top.Controls.Add(Btn("Forget", (s, e) => Forget()));
            root.Controls.Add(top, 0, 0);
            root.SetColumnSpan(top, 2);

            // --- tool panel
            var tools = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };

            tools.Controls.Add(Group("Orientation (exact)", g =>
            {
                g.Controls.Add(Btn("Flip ↔", (s, e) => AddStep(new TransformStep { Type = TransformType.FlipH })));
                g.Controls.Add(Btn("Flip ↕", (s, e) => AddStep(new TransformStep { Type = TransformType.FlipV })));
                g.Controls.Add(Btn("90° ↺", (s, e) => AddStep(new TransformStep { Type = TransformType.Rot90CCW })));
                g.Controls.Add(Btn("90° ↻", (s, e) => AddStep(new TransformStep { Type = TransformType.Rot90CW })));
                g.Controls.Add(Btn("180°", (s, e) => AddStep(new TransformStep { Type = TransformType.Rot180 })));
            }));

            tools.Controls.Add(Group("Skew (Neigen)", g =>
            {
                g.Controls.Add(Lbl("X"));
                _skewX = Num(-3, 3, 0, 2, 0.05m, v => Edit(TransformType.SkewX, (double)v, 0));
                g.Controls.Add(_skewX);
                g.Controls.Add(Lbl("Y"));
                _skewY = Num(-3, 3, 0, 2, 0.05m, v => Edit(TransformType.SkewY, (double)v, 0));
                g.Controls.Add(_skewY);
                g.Controls.Add(Lbl("(0.5 = iso 2:1)"));
            }));

            tools.Controls.Add(Group("Rotate / scale / move", g =>
            {
                g.Controls.Add(Lbl("Rotate°"));
                _rotate = Num(-180, 180, 0, 1, 0.5m, v => Edit(TransformType.Rotate, (double)v, 0));
                g.Controls.Add(_rotate);
                g.SetFlowBreak(_rotate, true);
                g.Controls.Add(Lbl("Scale X%"));
                _scaleX = Num(10, 800, 100, 0, 5, v => Edit(TransformType.Scale, (double)v / 100, (double)_scaleY.Value / 100));
                g.Controls.Add(_scaleX);
                g.Controls.Add(Lbl("Y%"));
                _scaleY = Num(10, 800, 100, 0, 5, v => Edit(TransformType.Scale, (double)_scaleX.Value / 100, (double)v / 100));
                g.Controls.Add(_scaleY);
                g.SetFlowBreak(_scaleY, true);
                g.Controls.Add(Lbl("Move X"));
                _moveX = Num(-300, 300, 0, 0, 1, v => Edit(TransformType.Move, (double)v, (double)_moveY.Value));
                g.Controls.Add(_moveX);
                g.Controls.Add(Lbl("Y"));
                _moveY = Num(-300, 300, 0, 0, 1, v => Edit(TransformType.Move, (double)_moveX.Value, (double)v));
                g.Controls.Add(_moveY);
            }));

            tools.Controls.Add(Group("Distort / warp (drag the points)", g =>
            {
                _pointsOff = new RadioButton { Text = "Off", AutoSize = true, Checked = true };
                _pointsDistort = new RadioButton { Text = "Distort (4 corners)", AutoSize = true };
                _pointsWarp = new RadioButton { Text = "Warp (3x3 points)", AutoSize = true };
                _pointsOff.CheckedChanged += (s, e) => { if (_pointsOff.Checked) { SetPointMode(PointMode.Off); } };
                _pointsDistort.CheckedChanged += (s, e) => { if (_pointsDistort.Checked) { SetPointMode(PointMode.Distort); } };
                _pointsWarp.CheckedChanged += (s, e) => { if (_pointsWarp.Checked) { SetPointMode(PointMode.Warp); } };
                g.Controls.Add(_pointsOff);
                g.Controls.Add(_pointsDistort);
                g.Controls.Add(_pointsWarp);
                g.Controls.Add(Btn("Reset points", (s, e) => ResetPoints()));
            }));

            tools.Controls.Add(Group("Canvas", g =>
            {
                g.Controls.Add(Lbl("W"));
                _canvasW = Num(1, 1024, 44, 0, 1, v => Commit("canvas", () => _settings.CanvasWidth = (int)v, true));
                g.Controls.Add(_canvasW);
                g.Controls.Add(Lbl("H"));
                _canvasH = Num(1, 1024, 44, 0, 1, v => Commit("canvas", () => _settings.CanvasHeight = (int)v, true));
                g.Controls.Add(_canvasH);
                g.Controls.Add(Btn("Fit", (s, e) => FitCanvas()));
                g.Controls.Add(Btn("Source size", (s, e) => SourceCanvas()));
                _keepBottom = new CheckBox { Text = "Keep bottom row (UO anchor)", AutoSize = true, Checked = true };
                _keepBottom.CheckedChanged += (s, e) =>
                {
                    if (!_programmatic)
                    {
                        Commit("keep", () => _settings.KeepBottom = _keepBottom.Checked, false);
                    }
                };
                g.Controls.Add(_keepBottom);
            }));

            tools.Controls.Add(Group("Steps (click = edit)", g =>
            {
                _stepList = new ListBox { Width = 260, Height = 120, IntegralHeight = false };
                _stepList.SelectedIndexChanged += (s, e) => SelectStep(_stepList.SelectedIndex);
                g.Controls.Add(_stepList);
                g.Controls.Add(Btn("Delete step", (s, e) => DeleteStep()));
                g.Controls.Add(Btn("New step", (s, e) => { _editIndex = -1; _stepList.ClearSelected(); ResetToolValues(); }));
            }));

            tools.Controls.Add(Group("View", g =>
            {
                g.Controls.Add(Lbl("Zoom"));
                _zoom = Num(1, 32, 4, 0, 1, v => _canvas.Zoom = (int)v);
                g.Controls.Add(_zoom);
                _ghost = new CheckBox { Text = "Original as ghost", AutoSize = true, Margin = new Padding(6, 6, 3, 3) };
                _ghost.CheckedChanged += (s, e) => { _canvas.ShowGhost = _ghost.Checked; _canvas.Invalidate(); };
                g.Controls.Add(_ghost);
                g.SetFlowBreak(_ghost, true);
                g.Controls.Add(Lbl("Grid"));
                _gridMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };
                _gridMode.Items.AddRange(new object[] { "None", "Lines", "Dots", "Lines + dots", "UO iso diamonds" });
                _gridMode.SelectedIndex = 0;
                _gridMode.SelectedIndexChanged += (s, e) =>
                {
                    _canvas.GridMode = (PixelGridMode)_gridMode.SelectedIndex;
                    if (_canvas.GridMode == PixelGridMode.Iso && _gridSize.Value < 8)
                    {
                        _gridSize.Value = 44;
                    }

                    _canvas.Invalidate();
                };
                g.Controls.Add(_gridMode);
                g.Controls.Add(Lbl("size"));
                _gridSize = Num(1, 200, 8, 0, 1, v => { _canvas.GridSize = (int)v; _canvas.Invalidate(); });
                g.Controls.Add(_gridSize);
                g.SetFlowBreak(_gridSize, true);
                g.Controls.Add(Btn("Grid colour...", (s, e) =>
                {
                    using var dlg = new ColorDialog { Color = _canvas.GridColor };
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        _canvas.GridColor = Color.FromArgb(160, dlg.Color);
                        _canvas.Invalidate();
                    }
                }));
                g.Controls.Add(Btn("Black", (s, e) => { _canvas.BackColor = Color.Black; _canvas.Parent.BackColor = Color.Black; }));
                g.Controls.Add(Btn("White", (s, e) => { _canvas.BackColor = Color.White; _canvas.Parent.BackColor = Color.White; }));
            }));

            root.Controls.Add(tools, 0, 1);

            // --- canvas
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Black };
            _canvas = new PixelCanvas { Location = new Point(8, 8) };
            _canvas.HandleMoved += OnHandleMoved;
            _canvas.HandleDragEnded += () => _lastEditKey = null;
            _canvas.PixelHover += (x, y, c) => _hoverLabel.Text = x < 0 ? string.Empty
                : $"x {x}  y {y}   " + (c == 0 ? "transparent" : $"#{ArtDirectionGenerator.R5(c) * 255 / 31:X2}{ArtDirectionGenerator.G5(c) * 255 / 31:X2}{ArtDirectionGenerator.B5(c) * 255 / 31:X2}");
            scroll.Controls.Add(_canvas);
            root.Controls.Add(scroll, 1, 1);

            // --- status
            var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            _status = Lbl("Load an image (or open this window from the Items tab).");
            _memoryLabel = new Label { AutoSize = true, ForeColor = Color.DarkSlateBlue, Margin = new Padding(20, 6, 3, 3) };
            _hoverLabel = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(20, 6, 3, 3) };
            bottom.Controls.Add(_status);
            bottom.Controls.Add(_memoryLabel);
            bottom.Controls.Add(_hoverLabel);
            root.Controls.Add(bottom, 0, 2);
            root.SetColumnSpan(bottom, 2);

            _programmatic = false;
            UpdateUndoButtons();
        }

        private static Button Btn(string text, EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true };
            b.Click += click;
            return b;
        }

        private static Label Lbl(string text) => new Label { Text = text, AutoSize = true, Margin = new Padding(3, 7, 3, 3) };

        private NumericUpDown Num(decimal min, decimal max, decimal value, int decimals, decimal inc, Action<decimal> changed)
        {
            var n = new NumericUpDown { Minimum = min, Maximum = max, Value = value, DecimalPlaces = decimals, Increment = inc, Width = 64 };
            n.ValueChanged += (s, e) =>
            {
                if (!_programmatic && _source != null)
                {
                    changed(n.Value);
                }
            };
            return n;
        }

        private static GroupBox Group(string title, Action<FlowLayoutPanel> fill)
        {
            var g = new GroupBox { Text = title, Width = 275, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Width = 265, MaximumSize = new Size(265, 0), WrapContents = true };
            fill(flow);
            g.Controls.Add(flow);
            return g;
        }
        #endregion

        #region Loading
        /// <summary>Sets the source; a remembered transformation of this image is applied automatically.</summary>
        public void LoadSource(Bitmap bitmap, int itemId, string name = null)
        {
            if (bitmap == null)
            {
                return;
            }

            LoadSource(ArtDirectionForm.FromBitmap(bitmap, false), itemId, name ?? (itemId >= 0 ? $"Item 0x{itemId:X4}" : "image"));
        }

        public void LoadSource(ArtPixelImage image, int itemId, string name)
        {
            _source = image;
            _sourceId = itemId;
            _sourceName = name;
            _settings = new TransformSettings { CanvasWidth = image.Width, CanvasHeight = image.Height };
            _undo.Clear();
            _redo.Clear();
            _editIndex = -1;
            _idBox.Text = itemId >= 0 ? $"0x{itemId:X4}" : NextFreeId();

            TransformMemoryEntry entry = _memory.Find(ArtFeatures.Compute(image, itemId), itemId, out double distance);
            if (entry != null && _rememberCheck.Checked)
            {
                _settings = entry.Settings.Clone();
                _memoryLabel.Text = $"Remembered transformation applied ({entry.Settings.Steps.Count} steps, saved {entry.Saved:g}) - Undo/Reset to remove it.";
                _undo.Add(new TransformSettings { CanvasWidth = image.Width, CanvasHeight = image.Height });
            }
            else
            {
                _memoryLabel.Text = string.Empty;
            }

            _pointsOff.Checked = true;
            _pointMode = PointMode.Off;
            SyncToolsFromSettings();
            UpdateUndoButtons();
            RefreshAll();
            _status.Text = $"{name}: {image.Width}x{image.Height}";
        }

        private void LoadFromItemId()
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Item ID (hex 0x... or decimal):", "Load item", _sourceId >= 0 ? $"0x{_sourceId:X}" : "0x");
            if (Utils.ConvertStringToInt(input, out int id, 0, Art.GetMaxExtendedItemId()) && Art.IsValidStatic(id))
            {
                LoadSource(Art.GetStatic(id), id);
            }
        }

        private void LoadFromFile()
        {
            using var dlg = new OpenFileDialog { Filter = "Image files (*.png;*.bmp;*.tif;*.tiff;*.gif)|*.png;*.bmp;*.tif;*.tiff;*.gif" };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            using var tmp = new Bitmap(dlg.FileName);
            LoadSource(ArtDirectionForm.FromBitmap(tmp, true), -1, Path.GetFileName(dlg.FileName));
        }

        private void LoadFromClipboard()
        {
            if (!Clipboard.ContainsImage())
            {
                _status.Text = "No image in the clipboard.";
                return;
            }

            using Image img = Clipboard.GetImage();
            using var tmp = new Bitmap(img);
            LoadSource(ArtDirectionForm.FromBitmap(tmp, true), -1, "clipboard");
        }
        #endregion

        #region Editing
        /// <summary>Changes the settings with undo support. Edits of the same kind in a row are one undo step.</summary>
        private void Commit(string key, Action change, bool coalesce)
        {
            if (_source == null)
            {
                return;
            }

            bool merge = coalesce && key == _lastEditKey && (DateTime.Now - _lastEditTime).TotalSeconds < 1.5;
            if (!merge)
            {
                _undo.Add(_settings.Clone());
                if (_undo.Count > 200)
                {
                    _undo.RemoveAt(0);
                }
            }

            _redo.Clear();
            change();
            _lastEditKey = coalesce ? key : null;
            _lastEditTime = DateTime.Now;
            UpdateUndoButtons();
            RefreshAll();
        }

        private void AddStep(TransformStep step)
        {
            Commit("add", () =>
            {
                _settings.Steps.Add(step);
                _editIndex = -1; // the next numeric edit starts a new step after this one
            }, false);
            ResetToolValues();
        }

        /// <summary>
        /// A numeric tool changed: edits the selected step of that type, or the last step if it has that type,
        /// otherwise a new step is added at the end (so earlier steps - e.g. a flip - are kept).
        /// </summary>
        private void Edit(TransformType type, double a, double b)
        {
            int index = _editIndex >= 0 && _editIndex < _settings.Steps.Count && _settings.Steps[_editIndex].Type == type
                ? _editIndex
                : _settings.Steps.Count > 0 && _settings.Steps[^1].Type == type ? _settings.Steps.Count - 1 : -1;

            Commit("edit" + type + index, () =>
            {
                if (index < 0)
                {
                    _settings.Steps.Add(new TransformStep { Type = type, A = a, B = b });
                    _editIndex = _settings.Steps.Count - 1;
                }
                else
                {
                    _settings.Steps[index].A = a;
                    _settings.Steps[index].B = b;
                    _editIndex = index;
                }
            }, true);
        }

        private void SetPointMode(PointMode mode)
        {
            _pointMode = mode;
            if (_source == null || mode == PointMode.Off)
            {
                RefreshAll();
                return;
            }

            TransformType type = mode == PointMode.Distort ? TransformType.Distort : TransformType.Warp;
            int index = _editIndex >= 0 && _editIndex < _settings.Steps.Count && _settings.Steps[_editIndex].Type == type
                ? _editIndex
                : _settings.Steps.Count > 0 && _settings.Steps[^1].Type == type ? _settings.Steps.Count - 1 : -1;

            if (index < 0)
            {
                Commit("add", () =>
                {
                    _settings.Steps.Add(mode == PointMode.Distort ? TransformStep.NewDistort() : TransformStep.NewWarp());
                    _editIndex = _settings.Steps.Count - 1;
                }, false);
            }
            else
            {
                _editIndex = index;
                RefreshAll();
            }
        }

        private void OnHandleMoved(int handle, int x, int y)
        {
            if (_editIndex < 0 || _editIndex >= _settings.Steps.Count)
            {
                return;
            }

            TransformStep step = _settings.Steps[_editIndex];
            if (step.Type != TransformType.Distort && step.Type != TransformType.Warp)
            {
                return;
            }

            ArtPixelImage before = ArtTransform.ApplyBefore(_source, _settings, _editIndex);
            double[] zero = new double[step.P.Length];
            (double X, double Y)[] baseHandles = step.Type == TransformType.Distort
                ? ArtTransform.DistortHandles(before, zero)
                : ArtTransform.WarpHandles(before, zero);
            if (handle >= baseHandles.Length)
            {
                return;
            }

            double nx = x - baseHandles[handle].X, ny = y - baseHandles[handle].Y;
            if (Math.Abs(step.P[handle * 2] - nx) < 0.01 && Math.Abs(step.P[(handle * 2) + 1] - ny) < 0.01)
            {
                return;
            }

            Commit("handle" + _editIndex, () =>
            {
                step.P[handle * 2] = nx;
                step.P[(handle * 2) + 1] = ny;
            }, true);
        }

        private void ResetPoints()
        {
            if (_editIndex >= 0 && _editIndex < _settings.Steps.Count && _settings.Steps[_editIndex].P != null)
            {
                Commit("resetpoints", () => Array.Clear(_settings.Steps[_editIndex].P), false);
            }
        }

        private void SelectStep(int index)
        {
            if (_programmatic || index < 0 || index >= _settings.Steps.Count)
            {
                return;
            }

            _editIndex = index;
            TransformStep step = _settings.Steps[index];
            _programmatic = true;
            ResetToolValues();
            switch (step.Type)
            {
                case TransformType.SkewX: _skewX.Value = Clamp(_skewX, step.A); break;
                case TransformType.SkewY: _skewY.Value = Clamp(_skewY, step.A); break;
                case TransformType.Rotate: _rotate.Value = Clamp(_rotate, step.A); break;
                case TransformType.Scale: _scaleX.Value = Clamp(_scaleX, step.A * 100); _scaleY.Value = Clamp(_scaleY, step.B * 100); break;
                case TransformType.Move: _moveX.Value = Clamp(_moveX, step.A); _moveY.Value = Clamp(_moveY, step.B); break;
            }

            _pointMode = step.Type == TransformType.Distort ? PointMode.Distort : step.Type == TransformType.Warp ? PointMode.Warp : PointMode.Off;
            _pointsDistort.Checked = _pointMode == PointMode.Distort;
            _pointsWarp.Checked = _pointMode == PointMode.Warp;
            _pointsOff.Checked = _pointMode == PointMode.Off;
            _programmatic = false;
            RefreshAll();
        }

        private static decimal Clamp(NumericUpDown n, double v) => Math.Clamp((decimal)v, n.Minimum, n.Maximum);

        private void DeleteStep()
        {
            int index = _stepList.SelectedIndex >= 0 ? _stepList.SelectedIndex : _settings.Steps.Count - 1;
            if (index < 0 || index >= _settings.Steps.Count)
            {
                return;
            }

            Commit("delete", () =>
            {
                _settings.Steps.RemoveAt(index);
                _editIndex = -1;
            }, false);
            ResetToolValues();
        }

        private void ResetToolValues()
        {
            bool old = _programmatic;
            _programmatic = true;
            _skewX.Value = 0;
            _skewY.Value = 0;
            _rotate.Value = 0;
            _scaleX.Value = 100;
            _scaleY.Value = 100;
            _moveX.Value = 0;
            _moveY.Value = 0;
            _programmatic = old;
        }

        private void SyncToolsFromSettings()
        {
            _programmatic = true;
            _canvasW.Value = Math.Clamp(_settings.CanvasWidth > 0 ? _settings.CanvasWidth : _source.Width, 1, 1024);
            _canvasH.Value = Math.Clamp(_settings.CanvasHeight > 0 ? _settings.CanvasHeight : _source.Height, 1, 1024);
            _keepBottom.Checked = _settings.KeepBottom;
            ResetToolValues();
            _programmatic = false;
        }

        private void FitCanvas()
        {
            if (_source == null)
            {
                return;
            }

            // room for turns and skews: square of the longer side plus a margin
            int size = Math.Min(1024, (int)(Math.Max(_source.Width, _source.Height) * 1.3) + 4);
            Commit("canvas", () =>
            {
                _settings.CanvasWidth = size;
                _settings.CanvasHeight = size;
            }, false);
            SyncToolsFromSettings();
        }

        private void SourceCanvas()
        {
            if (_source == null)
            {
                return;
            }

            Commit("canvas", () =>
            {
                _settings.CanvasWidth = _source.Width;
                _settings.CanvasHeight = _source.Height;
            }, false);
            SyncToolsFromSettings();
        }
        #endregion

        #region Undo / redo
        private void Undo()
        {
            if (_undo.Count == 0)
            {
                return;
            }

            _redo.Add(_settings.Clone());
            _settings = _undo[^1];
            _undo.RemoveAt(_undo.Count - 1);
            AfterHistoryChange();
        }

        private void Redo()
        {
            if (_redo.Count == 0)
            {
                return;
            }

            _undo.Add(_settings.Clone());
            _settings = _redo[^1];
            _redo.RemoveAt(_redo.Count - 1);
            AfterHistoryChange();
        }

        private void AfterHistoryChange()
        {
            _lastEditKey = null;
            _editIndex = Math.Min(_editIndex, _settings.Steps.Count - 1);
            if (_source != null)
            {
                SyncToolsFromSettings();
            }

            UpdateUndoButtons();
            RefreshAll();
        }

        private void UpdateUndoButtons()
        {
            _undoButton.Enabled = _undo.Count > 0;
            _redoButton.Enabled = _redo.Count > 0;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Control || ActiveControl is TextBox)
            {
                return;
            }

            if (e.KeyCode == Keys.Z)
            {
                Undo();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Y)
            {
                Redo();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.V)
            {
                LoadFromClipboard();
                e.Handled = true;
            }
        }
        #endregion

        #region Refresh
        private void RefreshAll()
        {
            if (_source == null)
            {
                return;
            }

            _result = ArtTransform.Apply(_source, _settings);
            ArtPixelImage ghost = ArtTransform.PlaceOnCanvas(_source, _settings);
            _canvas.SetImage(_result, ghost);

            // edit points
            _canvas.Handles.Clear();
            _canvas.HandleLinks.Clear();
            if (_pointMode != PointMode.Off && _editIndex >= 0 && _editIndex < _settings.Steps.Count)
            {
                TransformStep step = _settings.Steps[_editIndex];
                ArtPixelImage before = ArtTransform.ApplyBefore(_source, _settings, _editIndex);
                (double X, double Y)[] handles = step.Type == TransformType.Distort ? ArtTransform.DistortHandles(before, step.P)
                    : step.Type == TransformType.Warp ? ArtTransform.WarpHandles(before, step.P)
                    : Array.Empty<(double, double)>();
                foreach ((double x, double y) in handles)
                {
                    _canvas.Handles.Add(new PointF((float)x, (float)y));
                }

                if (step.Type == TransformType.Distort && handles.Length == 4)
                {
                    _canvas.HandleLinks.AddRange(new[] { (0, 1), (1, 3), (3, 2), (2, 0) });
                }
                else if (handles.Length == 9)
                {
                    for (int j = 0; j < 3; ++j)
                    {
                        for (int i = 0; i < 3; ++i)
                        {
                            int k = (j * 3) + i;
                            if (i < 2) { _canvas.HandleLinks.Add((k, k + 1)); }
                            if (j < 2) { _canvas.HandleLinks.Add((k, k + 3)); }
                        }
                    }
                }
            }

            _canvas.Invalidate();

            // step list
            _programmatic = true;
            _stepList.BeginUpdate();
            _stepList.Items.Clear();
            for (int i = 0; i < _settings.Steps.Count; ++i)
            {
                _stepList.Items.Add($"{i + 1}. {_settings.Steps[i]}");
            }

            if (_editIndex >= 0 && _editIndex < _stepList.Items.Count)
            {
                _stepList.SelectedIndex = _editIndex;
            }

            _stepList.EndUpdate();
            _programmatic = false;

            // touches the canvas edge -> probably clipped
            ArtTransform.GetBounds(_result, out int minX, out int minY, out int maxX, out int maxY);
            bool edge = maxX >= 0 && (minX == 0 || minY == 0 || maxX == _result.Width - 1);
            _status.Text = $"{_sourceName}: result {_result.Width}x{_result.Height}, {_settings.Steps.Count} step(s)" +
                           (edge ? "  -  touches the canvas edge, maybe cut off: press 'Fit' (Canvas)" : string.Empty);
            _status.ForeColor = edge ? Color.DarkOrange : SystemColors.ControlText;
        }
        #endregion

        #region Output / memory
        private string NextFreeId()
        {
            int max = Art.GetMaxExtendedItemId();
            for (int id = Math.Max(1, _sourceId + 1); id <= max; ++id)
            {
                if (Art.IsLegalStaticId(id) && !Art.IsValidStatic(id))
                {
                    return $"0x{id:X4}";
                }
            }

            return string.Empty;
        }

        private void InsertIntoArt()
        {
            if (_result == null)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(_idBox.Text, out int id, 0, Art.GetMaxExtendedItemId()) || !Art.IsLegalStaticId(id))
            {
                _status.Text = $"Invalid ID '{_idBox.Text}'.";
                return;
            }

            if (Art.IsValidStatic(id) && MessageBox.Show(this, $"0x{id:X4} is already used. Replace it?", "Insert",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            if (_rememberCheck.Checked)
            {
                Remember(false);
            }

            _insertCallback?.Invoke(id, ArtDirectionForm.ToBitmap(_result));
            _status.Text = $"Inserted as 0x{id:X4}. Save the Art to write the files.";
        }

        private void Remember(bool showMessage)
        {
            if (_source == null)
            {
                return;
            }

            try
            {
                _memory.Remember(_sourceName, _sourceId, ArtFeatures.Compute(_source, _sourceId), _settings);
                _memoryLabel.Text = $"Transformation remembered ({_settings.Steps.Count} steps) - loading this image again applies it.";
                if (showMessage)
                {
                    _status.Text = "Remembered.";
                }
            }
            catch (Exception ex)
            {
                _status.Text = $"Could not save: {ex.Message}";
            }
        }

        private void Forget()
        {
            if (_source == null)
            {
                return;
            }

            _memory.Forget(_sourceId, ArtFeatures.Compute(_source, _sourceId));
            _memoryLabel.Text = "Forgotten - this image is no longer transformed automatically.";
        }

        private Bitmap ExportBitmap(Color background)
        {
            using Bitmap art = ArtDirectionForm.ToBitmap(_result);
            bool transparent = background.A == 0;
            var bmp = new Bitmap(art.Width, art.Height, transparent ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(background);
                g.DrawImageUnscaled(art, 0, 0);
            }

            return bmp;
        }

        private void CopyResult(Color background)
        {
            if (_result == null)
            {
                return;
            }

            using Bitmap bmp = ExportBitmap(background);
            var data = new DataObject();
            data.SetData(DataFormats.Bitmap, true, bmp);
            var png = new MemoryStream();
            bmp.Save(png, ImageFormat.Png);
            data.SetData("PNG", false, png);
            Clipboard.SetDataObject(data, true);
            _status.Text = "Copied to the clipboard.";
        }

        private void SavePng()
        {
            if (_result == null)
            {
                return;
            }

            using var dlg = new SaveFileDialog { Filter = "PNG (*.png)|*.png|Bitmap (*.bmp)|*.bmp", FileName = "transformed.png" };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            using Bitmap bmp = ExportBitmap(Color.Black);
            bmp.Save(dlg.FileName, dlg.FilterIndex == 2 ? ImageFormat.Bmp : ImageFormat.Png);
            _status.Text = $"Saved {dlg.FileName}";
        }
        #endregion
    }
}

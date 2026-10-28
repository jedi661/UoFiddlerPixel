/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Controls.MapViewer.Build;
using UoFiddler.Controls.MapViewer.Editor.Tools;

namespace UoFiddler.Controls.MapViewer.Editor
{
    /// <summary>
    /// The brush of the "Paint brush" tool: textures or a copied pattern (with heights and statics),
    /// saved brushes, preview under the mouse.
    /// </summary>
    public partial class MapEditorForm
    {
        private BrushPreset _brush = new BrushPreset();
        private BrushLibrary _brushes;
        private GroupBox _brushPanel;
        private ComboBox _comboBrush;
        private RadioButton _radioBrushTextures;
        private RadioButton _radioBrushPattern;
        private CheckBox _checkBrushPreview;
        private BrushGridControl _brushGrid;
        private CheckBox _checkBrushNumbers;
        private CheckBox _checkBrushMove;
        private Button _buttonFixPositions;
        private Label _labelBrush;
        private CheckBox _checkPatternLand;
        private CheckBox _checkPatternStatics;
        private CheckBox _checkPatternReplace;
        private ComboBox _comboPatternHeight;
        private ComboBox _comboPatternAnchor;
        private bool _brushUpdating;

        private MultiImage _brushGhost;
        private (Point Cell, int Version, int Size, int Shape, int Origin) _brushGhostKey;

        private GroupBox CreateBrushPanel()
        {
            _brushes = BrushLibrary.Default;

            _brushPanel = new GroupBox { Text = "Brush (for \"Paint brush\")", Dock = DockStyle.Top, Height = 360, Padding = new Padding(4) };

            _comboBrush = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Left = 8, Top = 18, Width = 170 };
            _comboBrush.SelectedIndexChanged += OnBrushPresetChanged;
            var buttonNew = new Button { Text = "New", Left = 182, Top = 17, Width = 50, Height = 25 };
            buttonNew.Click += (s, e) => SetBrush(new BrushPreset { Size = (int)_numericBrush.Value, Shape = (BrushShape)_comboShape.SelectedIndex }, true);
            var buttonSave = new Button { Text = "Save...", Left = 234, Top = 17, Width = 64, Height = 25 };
            buttonSave.Click += OnClickSaveBrush;
            var buttonDelete = new Button { Text = "Edit...", Left = 300, Top = 17, Width = 64, Height = 25 };
            buttonDelete.Click += OnClickEditBrushes;

            _radioBrushTextures = new RadioButton { Text = "Textures", Left = 8, Top = 46, AutoSize = true, Checked = true };
            _radioBrushPattern = new RadioButton { Text = "Pattern", Left = 84, Top = 46, AutoSize = true };
            _radioBrushTextures.CheckedChanged += OnBrushModeChanged;
            _checkBrushPreview = new CheckBox { Text = "Preview", Left = 156, Top = 46, AutoSize = true, Checked = true };
            _checkBrushPreview.CheckedChanged += (s, e) => UpdateBrushOutline();
            _checkBrushNumbers = new CheckBox { Text = "Numbers", Left = 228, Top = 46, AutoSize = true, Checked = true };
            _checkBrushNumbers.CheckedChanged += (s, e) =>
            {
                _brushGrid.ShowNumbers = _checkBrushNumbers.Checked;
                _brushGrid.Invalidate();
                _brushGhostKey = default;
                UpdateBrushOutline();
            };

            _labelBrush = new Label { Left = 8, Top = 68, Width = 356, Height = 16, ForeColor = SystemColors.GrayText };

            // the brush from above: which tile lies where
            _brushGrid = new BrushGridControl { Left = 8, Top = 86, Width = 212, Height = 212, MaxCellSize = 48 };
            _brushGrid.Selection = () => _palette.SelectedIds;
            _brushGrid.Image = id => _palette.GetImage(id);
            _brushGrid.PatternChanged += (s, e) =>
            {
                _brush.Touch();
                _labelBrush.Text = _brush.Describe();
                UpdateBrushOutline();
            };
            _brushGrid.CellPicked += (s, e) =>
            {
                if (e.Tiles.Count > 0)
                {
                    _palette.Select(e.Tiles);
                }
            };

            // right of the grid
            _checkBrushMove = new CheckBox { Text = "Move cells", Left = 226, Top = 86, AutoSize = true };
            _checkBrushMove.CheckedChanged += (s, e) => _brushGrid.MoveMode = _checkBrushMove.Checked;
            Button Arrow(string text, int left, int dx, int dy)
            {
                var b = new Button { Text = text, Left = left, Top = 108, Width = 32, Height = 25 };
                b.Click += (s, e) => ShiftBrush(dx, dy);
                return b;
            }

            Button[] arrows = { Arrow("◄", 226, -1, 0), Arrow("►", 260, 1, 0), Arrow("▲", 294, 0, -1), Arrow("▼", 328, 0, 1) };
            _buttonFixPositions = new Button { Text = "Fix positions", Left = 226, Top = 138, Width = 134, Height = 25 };
            _buttonFixPositions.Click += (s, e) =>
            {
                if (_brush.Tiles.Count == 0)
                {
                    return;
                }

                _brush.Pattern = BrushPattern.FromTextures(_brush.Tiles, (int)_numericBrush.Value, (BrushShape)_comboShape.SelectedIndex, true);
                _brush.Mode = BrushMode.Pattern;
                _brush.Anchor = PatternAnchor.StrokeStart;
                SetBrush(_brush, false);
            };
            var buttonCopy = new Button { Text = "Copy from map", Left = 226, Top = 166, Width = 134, Height = 25 };
            buttonCopy.Click += (s, e) => SelectTool<CopyToBrushTool>();
            var buttonFromClip = new Button { Text = "Clip -> brush", Left = 226, Top = 194, Width = 134, Height = 25 };
            buttonFromClip.Click += OnClickClipToBrush;
            var buttonFit = new Button { Text = "Brush = pattern", Left = 226, Top = 222, Width = 134, Height = 25 };
            buttonFit.Click += OnClickBrushFitPattern;
            _checkPatternLand = new CheckBox { Text = "Land", Left = 226, Top = 252, AutoSize = true, Checked = true };
            _checkPatternStatics = new CheckBox { Text = "Statics", Left = 290, Top = 252, AutoSize = true, Checked = true };
            _checkPatternReplace = new CheckBox { Text = "Replace statics", Left = 226, Top = 274, AutoSize = true, Checked = true };

            _comboPatternHeight = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Left = 8, Top = 304, Width = 352 };
            _comboPatternHeight.Items.AddRange(new object[] { "Height: keep the ground", "Height: as copied (absolute z)", "Height: as copied, moved to the ground" });
            _comboPatternAnchor = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Left = 8, Top = 330, Width = 352 };
            _comboPatternAnchor.Items.AddRange(new object[] { "Pattern starts where the stroke starts (click = stamp)", "Pattern repeats on the map grid (seamless)" });
            foreach (CheckBox c in new[] { _checkPatternLand, _checkPatternStatics, _checkPatternReplace })
            {
                c.CheckedChanged += OnPatternOptionChanged;
            }

            _comboPatternHeight.SelectedIndexChanged += OnPatternOptionChanged;
            _comboPatternAnchor.SelectedIndexChanged += OnPatternOptionChanged;

            _brushPanel.Controls.AddRange(new Control[] { _comboBrush, buttonNew, buttonSave, buttonDelete, _radioBrushTextures, _radioBrushPattern, _checkBrushPreview, _checkBrushNumbers,
                _labelBrush, _brushGrid, _checkBrushMove, _buttonFixPositions, buttonCopy, buttonFromClip, buttonFit, _checkPatternLand, _checkPatternStatics, _checkPatternReplace,
                _comboPatternHeight, _comboPatternAnchor });
            _brushPanel.Controls.AddRange(arrows);

            _numericBrush.ValueChanged += OnMainBrushSizeChanged;
            _comboShape.SelectedIndexChanged += OnMainBrushSizeChanged;

            toolTip.SetToolTip(_brushGrid, "The brush from above, like it is painted. Left = palette tile(s) into a cell, right = empty, Shift+drag = move, Ctrl+click = into the palette");
            toolTip.SetToolTip(_checkBrushMove, "Drag a cell onto another cell: the two cells change places");
            toolTip.SetToolTip(_checkBrushNumbers, "Numbers of the cells in the grid and on the map preview");
            toolTip.SetToolTip(_buttonFixPositions, "Random textures -> pattern: every texture gets a fixed place that can be moved");
            toolTip.SetToolTip(arrows[0], "Move the whole pattern one cell (what leaves on one side comes back on the other)");
            toolTip.SetToolTip(_comboBrush, "Saved brushes (UoFiddler_Brushes\\Brushes.xml in the output directory)");
            toolTip.SetToolTip(buttonDelete, "Brush editor: which tile at which position, categories, search, copy, rename, delete, picture import");
            toolTip.SetToolTip(_radioBrushTextures, "The brush paints the textures shown below (click tiles in the palette, several = random variants)");
            toolTip.SetToolTip(_radioBrushPattern, "The brush paints a piece copied from a map: land, heights and statics (e.g. a coast)");
            toolTip.SetToolTip(_checkBrushPreview, "Shows what will be painted under the mouse (half transparent)");
            toolTip.SetToolTip(buttonCopy, "Tool \"Copy to brush\": click on the map copies the area under the brush outline into the brush");
            toolTip.SetToolTip(buttonFromClip, "Uses the copied clip (tab 2) as pattern (max. 64 x 64)");
            toolTip.SetToolTip(buttonFit, "Sets the brush size to the pattern size and square: one click stamps the whole pattern");
            toolTip.SetToolTip(_checkPatternLand, "Paint the land tiles of the pattern");
            toolTip.SetToolTip(_checkPatternStatics, "Paint the statics of the pattern (each cell once per stroke)");
            toolTip.SetToolTip(_checkPatternReplace, "Remove the statics of the painted cells first");

            _palette.SelectionChanged += OnPaletteSelectionChanged;
            FillBrushPresets(null);
            SetBrush(_brush, false);
            return _brushPanel;
        }

        #region Presets

        private void FillBrushPresets(BrushPreset select)
        {
            _brushUpdating = true;
            _comboBrush.BeginUpdate();
            _comboBrush.Items.Clear();
            _comboBrush.Items.Add("(unsaved brush)");
            _comboBrush.Items.AddRange(_brushes.Brushes.Cast<object>().ToArray());
            _comboBrush.EndUpdate();
            _comboBrush.SelectedItem = select;
            if (_comboBrush.SelectedIndex < 0)
            {
                _comboBrush.SelectedIndex = 0;
            }

            _brushUpdating = false;
        }

        private void OnBrushPresetChanged(object sender, EventArgs e)
        {
            if (_brushUpdating || !(_comboBrush.SelectedItem is BrushPreset preset))
            {
                return;
            }

            SetBrush(preset.Clone(), false);
            _numericBrush.Value = Math.Max(_numericBrush.Minimum, Math.Min(_numericBrush.Maximum, preset.Size));
            _comboShape.SelectedIndex = (int)preset.Shape;
            SelectTool<PaintLandTool>();
            statusLabelInfo.Text = $"Brush \"{preset.Name}\": {preset.Describe()}";
        }

        private void OnClickSaveBrush(object sender, EventArgs e)
        {
            string name = InputDialog.Show(this, "Save brush", "Name of the brush (same name = replace):", _brush.Name, false);
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            _brush.Name = name.Trim();
            _brush.Size = (int)_numericBrush.Value;
            _brush.Shape = (BrushShape)_comboShape.SelectedIndex;
            BrushPreset saved = _brush.Clone();
            int index = _brushes.Brushes.FindIndex(b => string.Equals(b.Name, saved.Name, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _brushes.Brushes[index] = saved;
            }
            else
            {
                _brushes.Brushes.Add(saved);
            }

            try
            {
                _brushes.Save();
                statusLabelInfo.Text = $"Brush saved: {_brushes.FileName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Saving failed:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            FillBrushPresets(saved);
        }

        private void OnClickEditBrushes(object sender, EventArgs e)
        {
            _brush.Size = (int)_numericBrush.Value;
            _brush.Shape = (BrushShape)_comboShape.SelectedIndex;
            using (var form = new BrushEditorForm(_brushes, _art, _brush))
            {
                DialogResult result = form.ShowDialog(this);
                FillBrushPresets(null);
                if (result != DialogResult.OK || form.Result == null)
                {
                    return;
                }

                SetBrush(form.Result, true);
                    _numericBrush.Value = Math.Max(_numericBrush.Minimum, Math.Min(_numericBrush.Maximum, form.Result.Size));
                _comboShape.SelectedIndex = (int)form.Result.Shape;
                UpdateBrushView();

                SelectTool<PaintLandTool>();
            }
        }

        private void OnClickDeleteBrush(object sender, EventArgs e)
        {
            if (!(_comboBrush.SelectedItem is BrushPreset preset) ||
                MessageBox.Show($"Delete the brush \"{preset.Name}\"?", Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            _brushes.Brushes.Remove(preset);
            try
            {
                _brushes.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Saving failed:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            FillBrushPresets(null);
        }

        #endregion

        #region Brush state

        /// <summary>Uses a brush and shows its settings</summary>
        private void SetBrush(BrushPreset brush, bool resetCombo)
        {
            _brush = brush;
            _brushUpdating = true;
            _radioBrushTextures.Checked = brush.Mode == BrushMode.Textures;
            _radioBrushPattern.Checked = brush.Mode == BrushMode.Pattern;
            _checkPatternLand.Checked = brush.PaintLand;
            _checkPatternStatics.Checked = brush.PaintStatics;
            _checkPatternReplace.Checked = brush.ReplaceStatics;
            _comboPatternHeight.SelectedIndex = (int)brush.Height;
            _comboPatternAnchor.SelectedIndex = (int)brush.Anchor;
            if (resetCombo)
            {
                _comboBrush.SelectedIndex = 0;
            }

            _brushUpdating = false;
            UpdateBrushView();
        }

        private void OnBrushModeChanged(object sender, EventArgs e)
        {
            if (_brushUpdating)
            {
                return;
            }

            _brush.Mode = _radioBrushPattern.Checked ? BrushMode.Pattern : BrushMode.Textures;
            _brush.Touch();
            UpdateBrushView();
        }

        private void OnPatternOptionChanged(object sender, EventArgs e)
        {
            if (_brushUpdating)
            {
                return;
            }

            _brush.PaintLand = _checkPatternLand.Checked;
            _brush.PaintStatics = _checkPatternStatics.Checked;
            _brush.ReplaceStatics = _checkPatternReplace.Checked;
            _brush.Height = (PatternHeight)Math.Max(0, _comboPatternHeight.SelectedIndex);
            _brush.Anchor = (PatternAnchor)Math.Max(0, _comboPatternAnchor.SelectedIndex);
            _brush.Touch();
            UpdateBrushView();
        }

        /// <summary>Palette click: sets the textures of the brush (Ctrl+click in the palette = several)</summary>
        private void OnPaletteSelectionChanged(object sender, EventArgs e)
        {
            if (_brush.Mode != BrushMode.Textures)
            {
                return;
            }

            _brush.Tiles.Clear();
            _brush.Tiles.AddRange(_palette.SelectedIds);
            _brush.Touch();
            UpdateBrushView();
        }

        private void UpdateBrushView()
        {
            bool pattern = _brush.Mode == BrushMode.Pattern;
            _labelBrush.Text = _brush.Describe();
            _buttonFixPositions.Enabled = !pattern && _brush.Tiles.Count > 0;
            _checkBrushMove.Enabled = pattern;
            UpdateBrushGrid();
            _brushGhostKey = default;
            UpdateBrushOutline();
        }

        private void UpdateBrushGrid()
        {
            bool pattern = _brush.Mode == BrushMode.Pattern;
            _brushGrid.Pattern = pattern ? _brush.Pattern : null;
            _brushGrid.RandomTiles = pattern ? null : _brush.Tiles;
            _brushGrid.BrushSize = (int)_numericBrush.Value;
            _brushGrid.Shape = (BrushShape)Math.Max(0, _comboShape.SelectedIndex);
            _brushGrid.Invalidate();
        }

        /// <summary>Brush size/shape in the tool settings: a square pattern of the old size gets the new size</summary>
        private void OnMainBrushSizeChanged(object sender, EventArgs e)
        {
            int size = (int)_numericBrush.Value;
            BrushPattern p = _brush.Pattern;
            if (_brush.Mode == BrushMode.Pattern && p != null && p.Width == p.Height && p.Width == _brush.Size && size != _brush.Size)
            {
                _brush.Pattern = p.ResizeCentered(size, size);
            }

            _brush.Size = size;
            _brush.Shape = (BrushShape)Math.Max(0, _comboShape.SelectedIndex);
            _brush.Touch();
            _labelBrush.Text = _brush.Describe();
            UpdateBrushGrid();
        }

        private void ShiftBrush(int dx, int dy)
        {
            if (_brush.Mode != BrushMode.Pattern || _brush.Pattern == null)
            {
                return;
            }

            _brush.Pattern = _brush.Pattern.Shift(dx, dy);
            UpdateBrushView();
        }

        private void SelectTool<T>() where T : MapTool
        {
            MapTool tool = MapTool.All.OfType<T>().FirstOrDefault();
            if (tool != null)
            {
                _comboTool.SelectedItem = tool;
            }

            if (!ToolActive && _data != null)
            {
                _checkToolActive.Checked = true;
            }
        }

        #endregion

        #region Copy into the brush

        /// <summary>Copies the brush area around a cell into the brush</summary>
        private void CopyToBrush(RegionCell cell)
        {
            int size = (int)_numericBrush.Value;
            Rectangle area = MapBrush.Bounds(MapBrush.Cells(cell.X, cell.Y, size, BrushShape.Square, _data.Map.Width, _data.Map.Height));
            MapClip clip = MapClip.FromMap(_data.Map, area, TargetSession, _targetDef?.Name ?? "map");
            _brush.Pattern = BrushPattern.FromClip(clip);
            _brush.Mode = BrushMode.Pattern;
            _brush.Name = $"Copy {area.X},{area.Y}";
            SetBrush(_brush, true);
            SelectTool<PaintLandTool>();
            statusLabelInfo.Text = $"Copied into the brush: {area.Width} x {area.Height} at {area.X},{area.Y}, {clip.Statics.Count} statics. Paint with the left mouse.";
        }

        private void OnClickClipToBrush(object sender, EventArgs e)
        {
            if (_clip == null)
            {
                MessageBox.Show("There is no clip. Copy an area in tab \"1. Source\" first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_clip.Width > 64 || _clip.Height > 64)
            {
                MessageBox.Show($"The clip is {_clip.Width} x {_clip.Height}. A brush pattern can be at most 64 x 64.\nUse \"3. Target\" to paste large areas.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var copy = new MapClip(_clip.Width, _clip.Height) { SourceName = _clip.SourceName, SourceArea = _clip.SourceArea };
            Array.Copy(_clip.LandIds, copy.LandIds, copy.LandIds.Length);
            Array.Copy(_clip.LandZ, copy.LandZ, copy.LandZ.Length);
            copy.Statics.AddRange(_clip.Statics.Where(IsIncluded));
            _brush.Pattern = BrushPattern.FromClip(copy);
            _brush.Mode = BrushMode.Pattern;
            SetBrush(_brush, true);
            SelectTool<PaintLandTool>();
        }

        private void OnClickBrushFitPattern(object sender, EventArgs e)
        {
            if (!_brush.HasPattern)
            {
                return;
            }

            _numericBrush.Value = Math.Max(_numericBrush.Minimum, Math.Min(_numericBrush.Maximum, Math.Max(_brush.Pattern.Width, _brush.Pattern.Height)));
            _comboShape.SelectedIndex = 0;
            _comboPatternAnchor.SelectedIndex = (int)PatternAnchor.StrokeStart;
        }

        #endregion

        #region Preview

        /// <summary>Half transparent picture of what "Paint brush" would paint under the mouse</summary>
        private void UpdateBrushPreview(RegionCell cell, IReadOnlyList<Point> cells)
        {
            bool show = ToolActive && _checkBrushPreview?.Checked == true && CurrentTool is PaintLandTool && cell != null && cells != null && cells.Count > 0 && _data != null;
            if (!show)
            {
                if (isoView.GhostImage != null && !_pasting)
                {
                    isoView.GhostImage = null;
                }

                isoView.CellLabels = null;
                return;
            }

            // inside of a stroke the pattern keeps its start position
            Point? origin = _stroke?.PatternOrigin;
            var key = (new Point(cell.X, cell.Y), _brush.Version + (int)_numericBrush.Value * 7919, (int)_numericBrush.Value, _comboShape.SelectedIndex,
                origin.HasValue ? origin.Value.X * 31 + origin.Value.Y : -1);
            if (_brushGhost != null && key == _brushGhostKey)
            {
                return;
            }

            _brushGhostKey = key;
            _brushGhost?.Dispose();
            _brushGhost = null;

            var context = new ToolContext(_data.Map, TargetSession) { Brush = _brush, Tiles = _palette.SelectedIds.ToList(), PatternOrigin = origin, StrokeZ = _stroke?.StrokeZ, BrushCenter = new Point(cell.X, cell.Y) };
            PaintPlan plan;
            try
            {
                plan = PaintLandTool.Plan(context, cells, false);
            }
            catch (Exception)
            {
                return;
            }

            Rectangle b = MapBrush.Bounds(cells);
            var clip = new MapClip(b.Width, b.Height);
            var painted = plan.Land.ToDictionary(l => new Point(l.X, l.Y), l => (l.Id, l.Z));
            Ultima.TileMatrix tiles = _data.Map.Tiles;
            var inBrush = new HashSet<Point>(cells);
            for (int y = 0; y < b.Height; y++)
            {
                for (int x = 0; x < b.Width; x++)
                {
                    var p = new Point(b.X + x, b.Y + y);
                    Ultima.Tile t = tiles.GetLandTile(p.X, p.Y);
                    if (!inBrush.Contains(p))
                    {
                        // outside of a round brush: nothing
                        clip.SetLand(x, y, 2, t.Z);
                        continue;
                    }

                    if (painted.TryGetValue(p, out (ushort Id, sbyte Z) n))
                    {
                        clip.SetLand(x, y, n.Id, n.Z);
                    }
                    else
                    {
                        clip.SetLand(x, y, t.Id, t.Z);
                    }
                }
            }

            foreach (MapStatic s in plan.AddStatics)
            {
                clip.Statics.Add(new ClipStatic(s.X - b.X, s.Y - b.Y, s.Id, s.Z, s.Hue));
            }

            // numbers of the pattern cells on the map (same numbers as in the grid)
            if (_checkBrushNumbers.Checked && _brush.Mode == BrushMode.Pattern && _brush.HasPattern && context.PatternOrigin.HasValue)
            {
                BrushPattern pat = _brush.Pattern;
                Point o = context.PatternOrigin.Value;
                int Mod(int a, int m) => ((a % m) + m) % m;
                isoView.CellLabels = cells.ToDictionary(p => p, p => ((Mod(p.Y - o.Y, pat.Height) * pat.Width) + Mod(p.X - o.X, pat.Width) + 1).ToString());
            }
            else
            {
                isoView.CellLabels = null;
            }

            _brushGhost = ClipPreview.Render(clip, _art, null);
            isoView.GhostImage = _brushGhost?.Bitmap;
            if (_brushGhost != null)
            {
                isoView.GhostLocation = new PointF(_data.CanvasX(b.X, b.Y) - _brushGhost.OriginX, _data.CanvasY(b.X, b.Y, 0) - _brushGhost.OriginY);
            }
        }

        private void DisposeBrush()
        {
            if (_palette != null)
            {
                _palette.SelectionChanged -= OnPaletteSelectionChanged;
            }

            _brushGhost?.Dispose();
            _brushGhost = null;
        }

        #endregion
    }
}

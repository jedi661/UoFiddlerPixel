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
using Ultima;
using UoFiddler.Controls.MapViewer.Build;
using UoFiddler.Controls.MapViewer.Editor.Tools;
using UoFiddler.Controls.MapViewer.Editor.Transitions;

namespace UoFiddler.Controls.MapViewer.Editor
{
    /// <summary>
    /// Tab "4. Tools": brushes (texture, terrain with transitions, replace, height, smooth, erase statics, eyedropper)
    /// and the land tile palette. One mouse stroke = one undo step.
    /// </summary>
    public partial class MapEditorForm
    {
        private TabPage _tabTools;
        private CheckBox _checkToolActive;
        private ComboBox _comboTool;
        private Label _labelToolHelp;
        private NumericUpDown _numericBrush;
        private ComboBox _comboShape;
        private NumericUpDown _numericStep;
        private NumericUpDown _numericZ;
        private ComboBox _comboStaticFilter;
        private CheckBox _checkStaticsFollow;
        private TextBox _textReplace;
        private ComboBox _comboTransition;
        private RadioButton _radioPaintA;
        private RadioButton _radioPaintB;
        private LandPaletteControl _palette;
        private Timer _strokeTimer;

        private ToolContext _stroke;
        private IDisposable _strokeBatch;
        private MapTool _strokeTool;
        private TransitionLibrary _transitions;

        private MapTool CurrentTool => _comboTool?.SelectedItem as MapTool;

        private bool ToolActive => _checkToolActive?.Checked == true;

        private void InitTools()
        {
            _transitions = TransitionLibrary.Default;
            _transitions.Changed += OnTransitionsChanged;

            _tabTools = new TabPage("4. Tools") { UseVisualStyleBackColor = true };
            var top = new Panel { Dock = DockStyle.Top, Height = 318 };

            _checkToolActive = new CheckBox
            {
                Text = "Tool active (T)",
                Appearance = Appearance.Button,
                TextAlign = ContentAlignment.MiddleCenter,
                Left = 6,
                Top = 4,
                Width = 120,
                Height = 28
            };
            _checkToolActive.CheckedChanged += OnToolActiveChanged;

            _comboTool = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Left = 132, Top = 7, Width = 238 };
            _comboTool.Items.AddRange(MapTool.All.Cast<object>().ToArray());
            _comboTool.SelectedIndexChanged += OnToolChanged;

            _labelToolHelp = new Label { Left = 6, Top = 36, Width = 366, Height = 32, ForeColor = SystemColors.GrayText };

            var labelBrush = new Label { Text = "Brush:", Left = 6, Top = 74, AutoSize = true };
            _numericBrush = new NumericUpDown { Left = 60, Top = 71, Width = 54, Minimum = 1, Maximum = 64, Value = 3 };
            _numericBrush.ValueChanged += (s, e) => UpdateBrushOutline();
            _comboShape = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Left = 120, Top = 71, Width = 80 };
            _comboShape.Items.AddRange(new object[] { "Square", "Round" });
            _comboShape.SelectedIndex = 0;
            _comboShape.SelectedIndexChanged += (s, e) => UpdateBrushOutline();
            var labelKeys = new Label { Text = "[ ] = size", Left = 206, Top = 74, AutoSize = true, ForeColor = SystemColors.GrayText };

            var labelStep = new Label { Text = "Step:", Left = 6, Top = 104, AutoSize = true };
            _numericStep = new NumericUpDown { Left = 60, Top = 101, Width = 54, Minimum = 1, Maximum = 40, Value = 2 };
            var labelZ = new Label { Text = "Set height z:", Left = 122, Top = 104, AutoSize = true };
            _numericZ = new NumericUpDown { Left = 204, Top = 101, Width = 60, Minimum = -128, Maximum = 127 };
            _checkStaticsFollow = new CheckBox { Text = "Statics follow", Left = 272, Top = 102, AutoSize = true, Checked = true };

            var labelFilter = new Label { Text = "Erase:", Left = 6, Top = 134, AutoSize = true };
            _comboStaticFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Left = 60, Top = 131, Width = 200 };
            _comboStaticFilter.Items.AddRange(new object[] { "All statics", "Only trees/plants/rocks" });
            _comboStaticFilter.SelectedIndex = 0;

            var labelReplace = new Label { Text = "Replace:", Left = 6, Top = 164, AutoSize = true };
            _textReplace = new TextBox { Left = 60, Top = 161, Width = 210, PlaceholderText = "ids, e.g. 0x3-0x6 (Alt+click = eyedropper)" };
            var buttonReplaceFromPalette = new Button { Text = "< palette", Left = 274, Top = 160, Width = 96, Height = 25 };
            buttonReplaceFromPalette.Click += (s, e) => _textReplace.Text = TransitionSet.FormatIds(_palette.SelectedIds);

            var groupTransition = new GroupBox { Text = "Terrain + transitions", Left = 4, Top = 192, Width = 368, Height = 120 };
            _comboTransition = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Left = 8, Top = 20, Width = 230 };
            var buttonTransitions = new Button { Text = "Transitions...", Left = 244, Top = 19, Width = 116, Height = 25 };
            buttonTransitions.Click += OnClickTransitions;
            _radioPaintA = new RadioButton { Text = "paint A (inside)", Left = 8, Top = 50, AutoSize = true, Checked = true };
            _radioPaintB = new RadioButton { Text = "paint B (outside)", Left = 150, Top = 50, AutoSize = true };
            var buttonFix = new Button { Text = "Set transitions in the working area", Left = 8, Top = 78, Width = 250, Height = 28 };
            buttonFix.Click += OnClickFixTransitions;
            groupTransition.Controls.AddRange(new Control[] { _comboTransition, buttonTransitions, _radioPaintA, _radioPaintB, buttonFix });

            top.Controls.AddRange(new Control[]
            {
                _checkToolActive, _comboTool, _labelToolHelp, labelBrush, _numericBrush, _comboShape, labelKeys,
                labelStep, _numericStep, labelZ, _numericZ, _checkStaticsFollow, labelFilter, _comboStaticFilter,
                labelReplace, _textReplace, buttonReplaceFromPalette, groupTransition
            });

            _palette = new LandPaletteControl(_art) { Dock = DockStyle.Fill };

            // settings scroll, the splitter changes the height against the palette
            var upper = new Panel { Dock = DockStyle.Top, Height = 430, AutoScroll = true };
            top.Dock = DockStyle.Top;
            upper.Controls.Add(CreateShapePanel());
            upper.Controls.Add(CreateBrushPanel());
            upper.Controls.Add(top);
            _tabTools.Controls.Add(_palette);
            _tabTools.Controls.Add(new Splitter { Dock = DockStyle.Top, Height = 5, BackColor = SystemColors.ControlDark });
            _tabTools.Controls.Add(upper);
            tabControlSide.TabPages.Add(_tabTools);

            _strokeTimer = new Timer { Interval = 150 };
            _strokeTimer.Tick += (s, e) =>
            {
                _strokeTimer.Stop();
                ReloadView();
            };

            isoView.StrokeStarted += OnStrokeStarted;
            isoView.StrokeCell += OnStrokeCell;
            isoView.StrokeEnded += OnStrokeEnded;
            tabControlSide.SelectedIndexChanged += (s, e) =>
            {
                if (tabControlSide.SelectedTab == _tabTools)
                {
                    EnsurePalette();
                }
            };

            toolTip.SetToolTip(_checkToolActive, "On: the left mouse uses the tool in the view (key T). Right/middle mouse moves the view.");
            toolTip.SetToolTip(_comboTool, "Tool for the left mouse button (F1 = help for all tools)");
            toolTip.SetToolTip(_numericBrush, "Brush size in tiles (1-64), keys [ and ]");
            toolTip.SetToolTip(_comboShape, "Square or round brush");
            toolTip.SetToolTip(_numericStep, "Raise / Lower: height change per stroke");
            toolTip.SetToolTip(_numericZ, "Set height: target z (the eyedropper takes it from the map)");
            toolTip.SetToolTip(_checkStaticsFollow, "Statics move up/down with the ground");
            toolTip.SetToolTip(_comboStaticFilter, "Erase statics: all or only trees, plants and rocks");
            toolTip.SetToolTip(_textReplace, "Replace texture: these land tiles are replaced (hex 0x.., ranges 0x3-0x6)");
            toolTip.SetToolTip(buttonReplaceFromPalette, "Takes the tiles selected in the palette into the replace list");
            toolTip.SetToolTip(_comboTransition, "Transition for \"Terrain + transitions\" and the button below");
            toolTip.SetToolTip(buttonTransitions, "Create and edit transitions and terrain groups");
            toolTip.SetToolTip(_radioPaintA, "Terrain tool paints the inside terrain A (e.g. sand)");
            toolTip.SetToolTip(_radioPaintB, "Terrain tool paints the outside terrain B (e.g. grass)");
            toolTip.SetToolTip(buttonFix, "Sets all edges and corners of the chosen transition in the loaded working area");

            _comboTool.SelectedIndex = 0;
            FillTransitions(null);
        }

        private bool _paletteLoaded;

        private void EnsurePalette()
        {
            if (!_paletteLoaded)
            {
                _paletteLoaded = true;
                Cursor.Current = Cursors.WaitCursor;
                _palette.Reload();
                Cursor.Current = Cursors.Default;
            }
        }

        private void DisposeTools()
        {
            if (_transitions != null)
            {
                _transitions.Changed -= OnTransitionsChanged;
            }

            DisposeBrush();
            _strokeTimer?.Dispose();
            _strokeBatch?.Dispose();
            _strokeBatch = null;
        }

        #region Tool selection

        private void OnToolChanged(object sender, EventArgs e)
        {
            MapTool tool = CurrentTool;
            _labelToolHelp.Text = tool?.Help ?? string.Empty;
            if (!(tool is LineTool) && isoView != null)
            {
                isoView.ObstacleCells = null;
            }

            if (tool is SelectAreaTool && _generatorArea.HasValue)
            {
                isoView.SelectedArea = _generatorArea;
            }

            UpdateBrushOutline();
        }

        private void OnToolActiveChanged(object sender, EventArgs e)
        {
            if (ToolActive)
            {
                if (_pasting)
                {
                    StopPasting();
                }

                if (_data == null)
                {
                    MessageBox.Show("Load a working area first (tab \"3. Target\": click on the map).", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _checkToolActive.Checked = false;
                    return;
                }

                EnsurePalette();
            }

            isoView.ToolMode = ToolActive;
            _checkToolActive.BackColor = ToolActive ? Color.LightGreen : SystemColors.Control;
            UpdateBrushOutline();
            statusLabelInfo.Text = ToolActive
                ? "Tool active: left mouse = use the tool, right/middle mouse = move the view, Alt+click = eyedropper"
                : string.Empty;
        }

        private void UpdateBrushOutline()
        {
            if (isoView == null)
            {
                return;
            }

            RegionCell cell = isoView.HoverCell;
            MapTool tool = CurrentTool;
            List<Point> cells = null;
            if (!ToolActive || cell == null || tool == null || _data == null)
            {
                isoView.BrushCells = null;
            }
            else
            {
                int size = tool.UsesBrush ? (int)_numericBrush.Value : 1;
                cells = MapBrush.Cells(cell.X, cell.Y, size, (BrushShape)_comboShape.SelectedIndex, _data.Map.Width, _data.Map.Height);
                isoView.BrushCells = cells;
            }

            if (_brushPanel != null)
            {
                UpdateBrushPreview(cell, cells);
            }

            if (_shapePanel != null && _shapeStart == null && ToolActive)
            {
                UpdateShapeOverlay(cell);
            }

            isoView.Invalidate();
        }

        #endregion

        #region Strokes

        private ToolContext CreateContext()
        {
            HashSet<ushort> replace;
            try
            {
                replace = new HashSet<ushort>(TransitionSet.ParseIds(_textReplace.Text));
            }
            catch (FormatException)
            {
                replace = new HashSet<ushort>();
            }

            return new ToolContext(_data.Map, TargetSession)
            {
                Brush = _brush,
                Tiles = _brush.Mode == BrushMode.Textures && _brush.Tiles.Count > 0 ? _brush.Tiles.ToList() : _palette.SelectedIds.ToList(),
                ReplaceFrom = replace,
                Amount = (int)_numericStep.Value,
                TargetZ = (int)_numericZ.Value,
                StaticFilter = _comboStaticFilter.SelectedIndex == 1 ? ObstacleFilter.Plants : ObstacleFilter.All,
                StaticsFollowGround = _checkStaticsFollow.Checked,
                Transition = _comboTransition.SelectedItem as TransitionSet,
                Library = _transitions,
                PaintTargetTerrain = _radioPaintB.Checked
            };
        }

        private void OnStrokeStarted(object sender, IsoCellEventArgs e)
        {
            if (!ToolActive || _data == null || e.Cell == null)
            {
                return;
            }

            MapTool tool = CurrentTool;
            if (tool is EyedropperTool || (ModifierKeys & Keys.Alt) != 0)
            {
                PickTile(e.Cell);
                return;
            }

            if (tool is CopyToBrushTool)
            {
                CopyToBrush(e.Cell);
                return;
            }

            if (HandleShapeStrokeStart(tool, e.Cell))
            {
                return;
            }

            if (tool == null)
            {
                return;
            }

            _strokeBatch?.Dispose();
            _strokeTool = tool;
            _stroke = CreateContext();
            _stroke.BeginStroke();
            _strokeBatch = TargetSession.Batch(tool.Name);
        }

        private void OnStrokeCell(object sender, IsoCellEventArgs e)
        {
            if (HandleShapeStrokeCell(e.Cell))
            {
                return;
            }

            if (_strokeBatch == null || _stroke == null || e.Cell == null)
            {
                return;
            }

            int size = _strokeTool.UsesBrush ? (int)_numericBrush.Value : 1;
            List<Point> cells = MapBrush.Cells(e.Cell.X, e.Cell.Y, size, (BrushShape)_comboShape.SelectedIndex, _data.Map.Width, _data.Map.Height);
            try
            {
                _stroke.BrushCenter = new Point(e.Cell.X, e.Cell.Y);
                _strokeTool.Apply(_stroke, cells);
            }
            catch (Exception ex)
            {
                _stroke.Message = $"Error: {ex.Message}";
            }

            if (_stroke.Message != null)
            {
                statusLabelInfo.Text = _stroke.Message;
            }

            _strokeTimer.Stop();
            _strokeTimer.Start();
        }

        private void OnStrokeEnded(object sender, EventArgs e)
        {
            if (HandleShapeStrokeEnd())
            {
                return;
            }

            if (_strokeBatch == null)
            {
                return;
            }

            _strokeBatch.Dispose();
            _strokeBatch = null;
            _strokeTimer.Stop();
            ReloadView();
            if (_stroke?.Message == null && TargetSession.LastOperationName == _strokeTool?.Name)
            {
                statusLabelInfo.Text = $"{_strokeTool.Name}: done (Ctrl+Z = undo)";
            }

            _stroke = null;
            UpdateState();
        }

        private void PickTile(RegionCell cell)
        {
            _palette.Select(new[] { cell.LandId });
            _textReplace.Text = $"0x{cell.LandId:X4}";
            _numericZ.Value = Math.Max(_numericZ.Minimum, Math.Min(_numericZ.Maximum, cell.LandZ));
            TerrainGroup group = _transitions.GroupOf(cell.LandId);
            statusLabelInfo.Text = $"Picked: land 0x{cell.LandId:X4} {cell.LandName}, z {cell.LandZ}" + (group != null ? $", group \"{group.Name}\"" : string.Empty);
        }

        private bool HandleToolKey(Keys keyData)
        {
            if (!ToolActive)
            {
                return false;
            }

            switch (keyData)
            {
                case Keys.OemOpenBrackets:
                    _numericBrush.Value = Math.Max(_numericBrush.Minimum, _numericBrush.Value - 1);
                    return true;
                case Keys.OemCloseBrackets:
                    _numericBrush.Value = Math.Min(_numericBrush.Maximum, _numericBrush.Value + 1);
                    return true;
                case Keys.Escape:
                    _checkToolActive.Checked = false;
                    return true;
            }

            return false;
        }

        #endregion

        #region Transitions

        private void FillTransitions(TransitionSet select)
        {
            select ??= _comboTransition.SelectedItem as TransitionSet;
            _comboTransition.BeginUpdate();
            _comboTransition.Items.Clear();
            _comboTransition.Items.AddRange(_transitions.Sets.Cast<object>().ToArray());
            _comboTransition.EndUpdate();
            if (select != null && _comboTransition.Items.Contains(select))
            {
                _comboTransition.SelectedItem = select;
            }
            else if (_comboTransition.Items.Count > 0)
            {
                _comboTransition.SelectedIndex = 0;
            }
        }

        private void OnTransitionsChanged(object sender, EventArgs e)
        {
            if (!IsDisposed)
            {
                FillTransitions(null);
            }
        }

        private void OnClickTransitions(object sender, EventArgs e)
        {
            using (var form = new TransitionEditorForm(_transitions, _art))
            {
                form.ShowDialog(this);
                FillTransitions(form.CurrentSet);
            }
        }

        private void OnClickFixTransitions(object sender, EventArgs e)
        {
            if (_data == null || !(_comboTransition.SelectedItem is TransitionSet set))
            {
                MessageBox.Show("Load a working area and choose a transition.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            StopPasting();
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                TransitionResult result = TransitionEngine.Compute(_data.Map, _data.Area, set, _transitions.GroupIds(set.GroupA), _transitions.GroupIds(set.GroupB));
                if (result.Land.Count > 0)
                {
                    TargetSession.SetLandTiles(result.Land, $"Transitions {set}");
                    ReloadView();
                }

                statusLabelInfo.Text = $"Transitions \"{set}\": {result.Land.Count} tiles changed, {result.Filled} filled with A" +
                                       (result.Missing.Count > 0 ? ", missing graphics: " + string.Join(", ", result.Missing.Select(m => $"{TransitionSet.Label(m.Key)} ({m.Value}x)")) : string.Empty);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            UpdateState();
        }

        #endregion
    }
}

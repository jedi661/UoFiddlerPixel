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
using UoFiddler.Controls.MapViewer.Editor.Tools;

namespace UoFiddler.Controls.MapViewer.Editor
{
    /// <summary>
    /// Lines/curves through points, shapes (rectangle, ellipse, triangle, line) and the area of the generator.
    /// The cells are changed with the tool chosen in "Draw with" (paint brush, terrain, height ...).
    /// </summary>
    public partial class MapEditorForm
    {
        private readonly List<Point> _linePoints = new List<Point>();
        private Point? _shapeStart;
        private List<Point> _shapeCells;
        private Rectangle? _generatorArea;
        private GroupBox _shapePanel;
        private ComboBox _comboShapeKind;
        private CheckBox _checkFilled;
        private CheckBox _checkCurve;
        private ComboBox _comboDrawWith;
        private GeneratorForm _generator;

        private GroupBox CreateShapePanel()
        {
            _shapePanel = new GroupBox { Text = "Lines, shapes, generator", Dock = DockStyle.Top, Height = 116 };

            var labelShape = new Label { Text = "Shape:", Left = 8, Top = 22, AutoSize = true };
            _comboShapeKind = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Left = 60, Top = 18, Width = 100 };
            _comboShapeKind.Items.AddRange(new object[] { "Rectangle", "Ellipse / circle", "Triangle", "Line" });
            _comboShapeKind.SelectedIndex = 0;
            _checkFilled = new CheckBox { Text = "Filled", Left = 168, Top = 20, AutoSize = true, Checked = true };
            _checkCurve = new CheckBox { Text = "Line as curve", Left = 236, Top = 20, AutoSize = true, Checked = true };

            var labelWith = new Label { Text = "Draw with:", Left = 8, Top = 52, AutoSize = true };
            _comboDrawWith = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Left = 80, Top = 48, Width = 200 };
            _comboDrawWith.Items.AddRange(MapTool.All.Where(t => t is PaintLandTool || t is TerrainTool || t is ReplaceLandTool || t is RaiseLandTool ||
                                                                 t is FlattenTool || t is SetHeightTool || t is SmoothTool || t is EraseStaticsTool).Cast<object>().ToArray());
            _comboDrawWith.SelectedIndex = 0;

            var buttonDraw = new Button { Text = "Draw line (Enter)", Left = 8, Top = 80, Width = 120, Height = 26 };
            buttonDraw.Click += (s, e) => DrawLine();
            var buttonClear = new Button { Text = "Clear points", Left = 132, Top = 80, Width = 90, Height = 26 };
            buttonClear.Click += (s, e) => ClearLine();
            var buttonGenerator = new Button { Text = "Generator...", Left = 226, Top = 80, Width = 110, Height = 26 };
            buttonGenerator.Click += OnClickGenerator;

            _shapePanel.Controls.AddRange(new Control[] { labelShape, _comboShapeKind, _checkFilled, _checkCurve, labelWith, _comboDrawWith, buttonDraw, buttonClear, buttonGenerator });

            toolTip.SetToolTip(_comboShapeKind, "Tool \"Shape\": drag the shape on the map");
            toolTip.SetToolTip(_checkFilled, "Filled shape, otherwise only the outline (width = brush size)");
            toolTip.SetToolTip(_checkCurve, "Tool \"Line / curve\": smooth curve through the points instead of straight lines");
            toolTip.SetToolTip(_comboDrawWith, "Tool that is used on the cells of lines and shapes (e.g. paint brush for floors, terrain for roads)");
            toolTip.SetToolTip(buttonDraw, "Draws the line through the clicked points (Enter). Backspace = remove the last point, Esc = clear");
            toolTip.SetToolTip(buttonGenerator, "Settlement (plots, road), lake, rough ground, trees and plants in the selected area");
            return _shapePanel;
        }

        private MapTool DrawWithTool => _comboDrawWith.SelectedItem as MapTool ?? MapTool.All[0];

        #region Strokes

        /// <summary>Mouse down with a shape tool; true = handled</summary>
        private bool HandleShapeStrokeStart(MapTool tool, RegionCell cell)
        {
            switch (tool)
            {
                case LineTool _:
                    _linePoints.Add(new Point(cell.X, cell.Y));
                    UpdateShapeOverlay(cell);
                    statusLabelInfo.Text = $"{_linePoints.Count} point(s). Enter = draw, Backspace = remove the last point, Esc = clear.";
                    return true;
                case ShapeTool _:
                case SelectAreaTool _:
                    _shapeStart = new Point(cell.X, cell.Y);
                    UpdateShapeDrag(cell);
                    return true;
                default:
                    return false;
            }
        }

        private bool HandleShapeStrokeCell(RegionCell cell)
        {
            if (_shapeStart == null || cell == null)
            {
                return false;
            }

            UpdateShapeDrag(cell);
            return true;
        }

        private bool HandleShapeStrokeEnd()
        {
            if (_shapeStart == null)
            {
                return false;
            }

            _shapeStart = null;
            if (CurrentTool is SelectAreaTool)
            {
                _generatorArea = isoView.SelectedArea;
                if (_generatorArea.HasValue)
                {
                    statusLabelInfo.Text = $"Generator area: {_generatorArea.Value.X},{_generatorArea.Value.Y} ({_generatorArea.Value.Width} x {_generatorArea.Value.Height})";
                    if (_generator != null && !_generator.IsDisposed)
                    {
                        _generator.SetArea(_generatorArea.Value);
                    }
                }

                return true;
            }

            List<Point> cells = _shapeCells;
            _shapeCells = null;
            isoView.BrushCells = null;
            if (cells != null && cells.Count > 0)
            {
                Rectangle bounds = MapBrush.Bounds(cells);
                ApplyCells(cells, bounds.Location, null, $"Shape: {_comboShapeKind.Text}");
            }

            return true;
        }

        private void UpdateShapeDrag(RegionCell cell)
        {
            if (_shapeStart == null)
            {
                return;
            }

            Point a = _shapeStart.Value, b = new Point(cell.X, cell.Y);
            if (CurrentTool is SelectAreaTool)
            {
                isoView.SelectedArea = Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X) + 1, Math.Max(a.Y, b.Y) + 1);
                isoView.Invalidate();
                return;
            }

            var kind = (ShapeKind)Math.Max(0, _comboShapeKind.SelectedIndex);
            bool filled = _checkFilled.Checked && kind != ShapeKind.Line;
            List<Point> cells = Shapes.Shape(kind, a, b, filled);
            if (!filled)
            {
                cells = Shapes.Thicken(cells, (int)_numericBrush.Value, (BrushShape)_comboShape.SelectedIndex, _data.Map.Width, _data.Map.Height);
            }

            _shapeCells = cells.Where(p => p.X >= 0 && p.Y >= 0 && p.X < _data.Map.Width && p.Y < _data.Map.Height).ToList();
            isoView.BrushCells = _shapeCells;
            isoView.GhostImage = null;
            Rectangle r = MapBrush.Bounds(_shapeCells);
            statusLabelInfo.Text = $"{_comboShapeKind.Text} {r.Width} x {r.Height}, {_shapeCells.Count} cells - release the mouse to draw with \"{DrawWithTool.Name}\"";
            isoView.Invalidate();
        }

        /// <summary>Line tool: path through the points (and to the mouse) as outline</summary>
        private void UpdateShapeOverlay(RegionCell hover)
        {
            if (!(CurrentTool is LineTool) || _data == null)
            {
                if (!(CurrentTool is SelectAreaTool) && _generator == null)
                {
                    isoView.SelectedArea = null;
                }

                return;
            }

            var points = new List<Point>(_linePoints);
            if (hover != null && (points.Count == 0 || points[points.Count - 1] != new Point(hover.X, hover.Y)))
            {
                points.Add(new Point(hover.X, hover.Y));
            }

            List<Point> path = _checkCurve.Checked ? Shapes.Curve(points) : Shapes.Polyline(points);
            isoView.BrushCells = Shapes.Thicken(path, (int)_numericBrush.Value, (BrushShape)_comboShape.SelectedIndex, _data.Map.Width, _data.Map.Height);
            isoView.ObstacleCells = _linePoints.ToList();
            isoView.GhostImage = null;
        }

        private bool HandleShapeKey(Keys keyData)
        {
            if (!ToolActive || !(CurrentTool is LineTool))
            {
                return false;
            }

            switch (keyData)
            {
                case Keys.Enter:
                    DrawLine();
                    return true;
                case Keys.Back:
                    if (_linePoints.Count > 0)
                    {
                        _linePoints.RemoveAt(_linePoints.Count - 1);
                        UpdateShapeOverlay(isoView.HoverCell);
                        isoView.Invalidate();
                    }

                    return true;
                case Keys.Escape when _linePoints.Count > 0:
                    ClearLine();
                    return true;
            }

            return false;
        }

        #endregion

        #region Draw

        private void ClearLine()
        {
            _linePoints.Clear();
            isoView.ObstacleCells = null;
            UpdateBrushOutline();
        }

        private void DrawLine()
        {
            if (_data == null)
            {
                return;
            }

            if (_linePoints.Count < 2)
            {
                MessageBox.Show("Click at least two points on the map (tool \"Line / curve\").", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<Point> path = _checkCurve.Checked ? Shapes.Curve(_linePoints) : Shapes.Polyline(_linePoints);
            List<Point> cells = Shapes.Thicken(path, (int)_numericBrush.Value, (BrushShape)_comboShape.SelectedIndex, _data.Map.Width, _data.Map.Height);
            Point first = _linePoints[0], last = _linePoints[_linePoints.Count - 1];
            ApplyCells(cells, null, first, _checkCurve.Checked ? "Curve" : "Line");

            if (MessageBox.Show("Continue the line from its end point?\n\nYes = more points from the end point\nNo = finished", Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _linePoints.Clear();
                _linePoints.Add(last);
            }
            else
            {
                _linePoints.Clear();
            }

            isoView.ObstacleCells = _linePoints.Count > 0 ? _linePoints.ToList() : null;
            UpdateBrushOutline();
        }

        /// <summary>Uses the "Draw with" tool on the cells as one undo step</summary>
        /// <param name="patternOrigin">pattern cell 0,0 (shapes: upper left corner, so floors start at the wall)</param>
        /// <param name="center">start of a line (pattern brushes are centered there)</param>
        private void ApplyCells(List<Point> cells, Point? patternOrigin, Point? center, string name)
        {
            MapTool tool = DrawWithTool;
            if (_pasting)
            {
                StopPasting();
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                ToolContext context = CreateContext();
                context.BeginStroke();
                context.PatternOrigin = patternOrigin;
                context.BrushCenter = center ?? patternOrigin;
                using (TargetSession.Batch($"{name} ({tool.Name})"))
                {
                    tool.Apply(context, cells);
                }

                ReloadView();
                statusLabelInfo.Text = context.Message ?? $"{name}: {cells.Count} cells with \"{tool.Name}\" (Ctrl+Z = undo, Ctrl+Y = redo)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Drawing failed:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            UpdateState();
        }

        #endregion

        #region Generator

        private void OnClickGenerator(object sender, EventArgs e)
        {
            if (_data == null)
            {
                MessageBox.Show("Load a working area first (tab \"3. Target\").", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_pasting)
            {
                StopPasting();
            }

            Rectangle area = _generatorArea ?? Rectangle.Inflate(_data.Area, -4, -4);
            if (_generator != null && !_generator.IsDisposed)
            {
                _generator.SetArea(area);
                _generator.Activate();
                return;
            }

            _generator = new GeneratorForm(_data.Map, TargetSession, area, _transitions, _brushes, () => _brush, () => _palette.SelectedIds,
                () => _clip?.Statics.Select(s => s.Id)) { Owner = this };
            _generator.Generated += (s, a) =>
            {
                ReloadView();
                UpdateState();
            };
            _generator.FormClosed += (s, a) =>
            {
                _generator = null;
                if (!(CurrentTool is SelectAreaTool))
                {
                    isoView.SelectedArea = null;
                    isoView.Invalidate();
                }
            };
            isoView.SelectedArea = area;
            isoView.Invalidate();
            _generator.Show(this);
        }

        #endregion
    }
}

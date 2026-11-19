/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Extended: list or grid view, filter, multi selection, keyboard, undo/redo.
 * The numbers shown are the hue numbers of the game/server (file index + 1).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class HuesControl : UserControl
    {
        public HuesControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            pictureBox.MouseWheel += OnMouseWheel;
            pictureBox.MouseDown += OnMouseDownList;
            pictureBox.MouseMove += OnMouseMoveList;
            InitToolbarExtensions();
        }

        private const int _itemHeight = 20;
        private const int _tileWidth = 72;
        private const int _tileHeight = 24;

        private int _selected;
        private bool _loaded;

        // indices of the hues that are shown (filter), in display order
        private List<int> _view = new List<int>();

        // additionally selected hues (Ctrl/Shift + click); always contains _selected when used
        private readonly HashSet<int> _multi = new HashSet<int>();
        private bool _grid;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HuesEditable { get; set; } = true;

        /// <summary>Raised when the selected hue changed</summary>
        public event EventHandler SelectedChanged;

        /// <summary>
        /// Selected hue (file index, the game hue number is Selected + 1)
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
                {
                    return;
                }

                if (!_loaded)
                {
                    return;
                }

                if (Hues.List.Length > 0)
                {
                    EnsureVisible(value);
                    pictureBox.Invalidate();
                }

                SelectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>All selected hues (sorted file indices)</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<int> SelectedIndices
        {
            get
            {
                var set = new HashSet<int>(_multi) { _selected };
                return set.Where(i => i >= 0 && i < Hues.List.Length).OrderBy(i => i).ToList();
            }
        }

        /// <summary>
        /// Reload when loaded (file changed)
        /// </summary>
        private void Reload()
        {
            if (!_loaded)
            {
                return;
            }

            _selected = 0;
            _multi.Clear();
            HueHistory.Main.Clear();
            OnLoad(this, EventArgs.Empty);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Options.LoadedUltimaClass["Hues"] = true;

            if (!HuesEditable)
            {
                pictureBox.MouseDoubleClick -= OnMouseDoubleClick;
                pictureBox.ContextMenuStrip = new ContextMenuStrip();
                HideEditExtensions();
            }
            else if (!_loaded)
            {
                InitEditExtensions();
            }

            CaptureBaseline();
            ApplyFilter(false);
            EnsureVisible(_selected);

            pictureBox.Invalidate();
            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.HueChangeEvent += OnHueChangeEvent;
                UoExtensions.Changed += OnExtensionChanged;
                Disposed += (s, ev) =>
                {
                    ControlEvents.FilePathChangeEvent -= OnFilePathChangeEvent;
                    ControlEvents.HueChangeEvent -= OnHueChangeEvent;
                    UoExtensions.Changed -= OnExtensionChanged;
                };
            }

            _loaded = true;
            SelectedChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        /// <summary>Extended hues created / resized / loaded: the list has a new length.</summary>
        private void OnExtensionChanged(UoExtensionKind? kind)
        {
            if (kind == null || kind == UoExtensionKind.Hues)
            {
                Reload();
            }
        }

        private void OnHueChangeEvent()
        {
            if (FilterDependsOnContent)
            {
                ApplyFilter(true);
            }

            pictureBox.Invalidate();
            SelectedChanged?.Invoke(this, EventArgs.Empty);
        }

        #region Layout (list / grid)

        private int Columns => _grid ? Math.Max(1, pictureBox.Width / _tileWidth) : 1;

        private int RowHeight => _grid ? _tileHeight : _itemHeight;

        private int VisibleRows => Math.Max(1, pictureBox.Height / RowHeight);

        private int TotalRows => (_view.Count + Columns - 1) / Columns;

        private void UpdateScrollBar()
        {
            int max = Math.Max(0, TotalRows - 1);
            vScrollBar.Minimum = 0;
            vScrollBar.Maximum = max;
            vScrollBar.LargeChange = Math.Max(1, VisibleRows);
            vScrollBar.SmallChange = 1;
            int maxValue = Math.Max(0, TotalRows - VisibleRows);
            if (vScrollBar.Value > maxValue)
            {
                vScrollBar.Value = maxValue;
            }
        }

        private void ScrollTo(int value)
        {
            int maxValue = Math.Max(0, TotalRows - VisibleRows);
            vScrollBar.Value = Math.Max(0, Math.Min(maxValue, Math.Min(vScrollBar.Maximum, value)));
            pictureBox.Invalidate();
        }

        /// <summary>Scrolls so that a hue is visible (if it is part of the filtered list)</summary>
        private void EnsureVisible(int index)
        {
            int pos = _view.IndexOf(index);
            if (pos < 0)
            {
                return;
            }

            int row = pos / Columns;
            if (row < vScrollBar.Value)
            {
                ScrollTo(row);
            }
            else if (row >= vScrollBar.Value + VisibleRows)
            {
                ScrollTo(row - VisibleRows + 1);
            }
        }

        /// <summary>File index of the hue at a position of the picture box or -1</summary>
        private int GetIndexAt(Point p)
        {
            if (p.Y < 0 || p.X < 0)
            {
                return -1;
            }

            int row = vScrollBar.Value + (p.Y / RowHeight);
            int col = _grid ? p.X / _tileWidth : 0;
            if (col >= Columns)
            {
                return -1;
            }

            int pos = (row * Columns) + col;
            return pos >= 0 && pos < _view.Count ? _view[pos] : -1;
        }

        #endregion

        #region Paint

        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            e.Graphics.Clear(Color.White);
            if (_view.Count == 0)
            {
                TextRenderer.DrawText(e.Graphics, _loaded ? "No hue matches the filter." : string.Empty, Font, pictureBox.ClientRectangle,
                    Color.Gray, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            if (_grid)
            {
                PaintGrid(e.Graphics);
            }
            else
            {
                PaintList(e.Graphics);
            }
        }

        private void PaintList(Graphics g)
        {
            int rows = VisibleRows + 1;
            float size = (float)(pictureBox.Width - 200) / 32;
            for (int y = 0; y < rows; ++y)
            {
                int pos = vScrollBar.Value + y;
                if (pos >= _view.Count)
                {
                    break;
                }

                int index = _view[pos];
                Hue hue = Hues.List[index];
                Rectangle rect = new Rectangle(0, y * _itemHeight, 200, _itemHeight);
                Brush back = index == _selected ? SystemBrushes.Highlight
                    : _multi.Contains(index) ? Brushes.LightSteelBlue
                    : SystemBrushes.Window;
                g.FillRectangle(back, rect);

                if (IsModified(index))
                {
                    g.FillRectangle(Brushes.Orange, 0, y * _itemHeight, 4, _itemHeight);
                }

                Rectangle stringRect = new Rectangle(6, y * _itemHeight, 194, _itemHeight);
                Brush textBrush = index == _selected ? SystemBrushes.HighlightText : Brushes.Black;
                int number = hue.Index + 1;
                g.DrawString($"{number,-5} {$"(0x{number:X})",-7} {hue.Name}", Font, textBrush, stringRect);

                for (int i = 0; i < hue.Colors.Length; ++i)
                {
                    Rectangle rectangle = new Rectangle(200 + (int)Math.Round(i * size), y * _itemHeight, (int)Math.Round(size + 1f), _itemHeight);
                    using (var brush = new SolidBrush(hue.GetColor(i)))
                    {
                        g.FillRectangle(brush, rectangle);
                    }
                }
            }
        }

        private void PaintGrid(Graphics g)
        {
            int cols = Columns;
            int rows = VisibleRows + 1;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    int pos = ((vScrollBar.Value + y) * cols) + x;
                    if (pos >= _view.Count)
                    {
                        return;
                    }

                    int index = _view[pos];
                    var cell = new Rectangle(x * _tileWidth, y * _tileHeight, _tileWidth, _tileHeight);
                    HueAnalysis.DrawStrip(g, new Rectangle(cell.X + 2, cell.Y + 2, cell.Width - 4, cell.Height - 4), Hues.List[index].Colors);

                    if (IsModified(index))
                    {
                        g.FillRectangle(Brushes.Orange, cell.X + 2, cell.Y + 2, 5, 5);
                    }

                    if (index == _selected)
                    {
                        using (var pen = new Pen(SystemColors.Highlight, 3))
                        {
                            g.DrawRectangle(pen, cell.X + 1, cell.Y + 1, cell.Width - 3, cell.Height - 3);
                        }
                    }
                    else if (_multi.Contains(index))
                    {
                        using (var pen = new Pen(Color.SteelBlue, 2))
                        {
                            g.DrawRectangle(pen, cell.X + 1, cell.Y + 1, cell.Width - 3, cell.Height - 3);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Print a nice border
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            const int borderWidth = 1;

            Color borderColor = VisualStyleInformation.TextControlBorder;

            ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, borderColor,
                borderWidth, ButtonBorderStyle.Solid, borderColor, borderWidth,
                ButtonBorderStyle.Solid, borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        #endregion

        #region Mouse / keyboard

        private void OnScroll(object sender, ScrollEventArgs e)
        {
            pictureBox.Invalidate();
        }

        private const int _scrollStep = 4;

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            int step = _grid ? 2 : _scrollStep;
            // before: the last rows could not be reached with the wheel
            ScrollTo(vScrollBar.Value + (e.Delta < 0 ? step : -step));
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (!_loaded)
            {
                return;
            }

            UpdateScrollBar();
            pictureBox.Invalidate();
        }

        private string _lastTip;

        private void OnMouseMoveList(object sender, MouseEventArgs e)
        {
            if (!_grid)
            {
                return;
            }

            int index = GetIndexAt(e.Location);
            string tip = index < 0 ? null : $"{index + 1} (0x{index + 1:X})  {Hues.List[index].Name}";
            if (tip != _lastTip)
            {
                _lastTip = tip;
                _toolTip.SetToolTip(pictureBox, tip);
            }
        }

        private void OnMouseDownList(object sender, MouseEventArgs e)
        {
            int index = GetIndexAt(e.Location);
            FocusList();
            if (index < 0)
            {
                return;
            }

            bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
            bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;

            if (e.Button == MouseButtons.Right)
            {
                // right click on an unselected hue selects it (for the context menu)
                if (index != _selected && !_multi.Contains(index))
                {
                    _multi.Clear();
                    Selected = index;
                }

                return;
            }

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (HuesEditable && shift)
            {
                int a = _view.IndexOf(_selected), b = _view.IndexOf(index);
                if (a >= 0 && b >= 0)
                {
                    if (!ctrl)
                    {
                        _multi.Clear();
                    }

                    for (int p = Math.Min(a, b); p <= Math.Max(a, b); p++)
                    {
                        _multi.Add(_view[p]);
                    }
                }

                Selected = index;
            }
            else if (HuesEditable && ctrl)
            {
                if (_multi.Count == 0)
                {
                    _multi.Add(_selected);
                }

                if (!_multi.Remove(index))
                {
                    _multi.Add(index);
                }

                Selected = index;
            }
            else
            {
                _multi.Clear();
                Selected = index;
            }
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            // selection happens in OnMouseDownList
        }

        private void UpdateSelection()
        {
            int index = GetIndexAt(pictureBox.PointToClient(MousePosition));
            if (index >= 0)
            {
                Selected = index;
            }
        }

        private void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            UpdateSelection();
            if (Selected >= 0)
            {
                OpenEditor(Selected);
            }
        }

        /// <summary>The picture box can not get the focus, the scroll bar takes it (keys are handled in ProcessCmdKey)</summary>
        private void FocusList()
        {
            vScrollBar.Focus();
        }

        private Control FocusedControl()
        {
            Control c = this;
            while (c is ContainerControl container && container.ActiveControl != null)
            {
                c = container.ActiveControl;
            }

            return c;
        }

        /// <summary>true if the hue list has the focus (the scroll bar takes it for the picture box)</summary>
        private bool ListHasFocus() => vScrollBar.Focused || FocusedControl() == vScrollBar;

        private bool TextHasFocus()
        {
            Control c = FocusedControl();

            if (c is TextBoxBase || c is ComboBox || c is NumericUpDown)
            {
                return true;
            }

            if (c is ToolStrip strip)
            {
                return strip.Items.OfType<ToolStripControlHost>().Any(h => h.Focused);
            }

            return false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!_loaded || TextHasFocus())
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            if (!ListHasFocus())
            {
                // e.g. the result list of the side panel: only the edit shortcuts (Ctrl+Z …) are handled
                return (HuesEditable && (keyData & Keys.Control) == Keys.Control && HandleEditKey(keyData)) || base.ProcessCmdKey(ref msg, keyData);
            }

            int pos = _view.IndexOf(_selected);
            switch (keyData)
            {
                case Keys.Up:
                    MoveSelection(pos - Columns);
                    return true;
                case Keys.Down:
                    MoveSelection(pos + Columns);
                    return true;
                case Keys.Left when _grid:
                    MoveSelection(pos - 1);
                    return true;
                case Keys.Right when _grid:
                    MoveSelection(pos + 1);
                    return true;
                case Keys.PageUp:
                    MoveSelection(pos - (Columns * VisibleRows));
                    return true;
                case Keys.PageDown:
                    MoveSelection(pos + (Columns * VisibleRows));
                    return true;
                case Keys.Home:
                    MoveSelection(0);
                    return true;
                case Keys.End:
                    MoveSelection(_view.Count - 1);
                    return true;
            }

            if (HuesEditable && HandleEditKey(keyData))
            {
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MoveSelection(int pos)
        {
            if (_view.Count == 0)
            {
                return;
            }

            pos = Math.Max(0, Math.Min(_view.Count - 1, pos));
            _multi.Clear();
            Selected = _view[pos];
        }

        #endregion

        #region Save / replace / export / import (original menu)

        private void OnClickSave(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                MessageBox.Show("The output folder is not set or does not exist (Options).", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Hues.Save(path);
            MessageBox.Show($"Hue saved to {path}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Hues"] = false;
            CaptureBaseline();
            pictureBox.Invalidate();
        }

        private void OnTextChangedReplace(object sender, EventArgs e)
        {
            ReplaceText.ForeColor = Utils.ConvertStringToInt(ReplaceText.Text, out _, 1, Hues.List.Length) ? Color.Black : Color.Red;
        }

        private void OnKeyDownReplace(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            // the entered number is the hue number (as shown in the list)
            if (!Utils.ConvertStringToInt(ReplaceText.Text, out int number, 1, Hues.List.Length))
            {
                return;
            }

            contextMenuStrip1.Close();
            int source = number - 1;

            // before: the Hue object itself was put into the list twice (both entries changed together)
            HueState state = HueState.Capture(Hues.List[source]);
            List<int> targets = SelectedIndices;
            HueHistory.Main.Record($"Replace with hue {number}", targets, () =>
            {
                foreach (int t in targets)
                {
                    state.ApplyTo(Hues.List[t]);
                }
            });
        }

        private void OnExport(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                MessageBox.Show("The output folder is not set or does not exist (Options).", "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string fileName = Path.Combine(path, $"Hue {_selected + 1}.txt");
            Hues.List[_selected].Export(fileName);
            MessageBox.Show($"Hue saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnImport(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog { Multiselect = false, Title = "Choose txt file to import", CheckFileExists = true, Filter = "txt files (*.txt)|*.txt" })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                int index = _selected;
                HueHistory.Main.Record($"Import into hue {index + 1}", new[] { index }, () => Hues.List[index].Import(dialog.FileName));
            }
        }

        #endregion

        #region Search (toolbar)

        private void HueIndexToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            // the number is the hue number as shown in the list (file index + 1)
            if (!Utils.ConvertStringToInt(HueIndexToolStripTextBox.Text, out int number))
            {
                return;
            }

            int index = Math.Max(0, Math.Min(Hues.List.Length - 1, number - 1));
            ShowHue(index);
        }

        /// <summary>Selects a hue and makes it visible (resets the filter if it hides the hue)</summary>
        public void ShowHue(int index)
        {
            if (index < 0 || index >= Hues.List.Length)
            {
                return;
            }

            if (!_view.Contains(index))
            {
                ResetFilter();
            }

            _multi.Clear();
            Selected = index;
            EnsureVisible(index);
        }

        private void SearchNameToolStripButton_Click(object sender, EventArgs e)
        {
            FindByName(Selected);
        }

        private void HueNameToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                FindByName(Selected);
                return;
            }

            FindByName(-1);
        }

        private void FindByName(int startIndex)
        {
            if (HueNameToolStripTextBox.TextBox == null)
            {
                return;
            }

            var search = HueNameToolStripTextBox.TextBox.Text.Trim();

            if (string.IsNullOrEmpty(search))
            {
                return;
            }

            var searchResult = FindByNameInternal(search, startIndex) ?? FindByNameInternal(search, -1);
            if (searchResult == null)
            {
                return;
            }

            ShowHue(searchResult.Index);
        }

        private static Hue FindByNameInternal(string search, int currentIndex)
        {
            return Hues.List.FirstOrDefault(hue =>
                hue.Name != null && hue.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 && hue.Index > currentIndex);
        }

        #region ExportAllHueNamesListTool
        private void ExportAllHueNamesListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = Path.Combine(Options.OutputPath, "Hue names list.txt");
            Hues.ExportHueList(fileName);
            MessageBox.Show($"Hue names list saved to {fileName}\n(the numbers in this list are file indices = hue number - 1)",
                "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
        #endregion

        #endregion
    }
}

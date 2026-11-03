/***************************************************************************
 *
 * $Author: Turley
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
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Ultima;

namespace UoFiddler.Plugin.SendItem.Forms
{
    /// <summary>
    /// Virtual thumbnail grid for statics. Only visible tiles are drawn, so it stays fast with 65k+ items.
    /// Supports multi selection (Ctrl / Shift), keyboard navigation and dragging statics onto a tab.
    /// </summary>
    public class StaticGrid : Control
    {
        public const string DragFormat = "SendItem.StaticIds";

        private readonly VScrollBar _scroll = new() { Dock = DockStyle.Right };
        private readonly ToolTip _toolTip = new() { InitialDelay = 400, ReshowDelay = 100 };
        private List<int> _ids = new();
        private readonly HashSet<int> _selected = new();
        private int _focus = -1;
        private int _anchor = -1;
        private int _hover = -1;
        private int _tileSize = 64;
        private Point _mouseDownAt;
        private bool _mouseDown;

        public event EventHandler SelectionChanged;
        public event EventHandler<int> ItemActivated;

        /// <summary>
        /// Returns the display name of a static (for tooltips).
        /// </summary>
        public Func<int, string> NameProvider { get; set; }

        public StaticGrid()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.Selectable, true);
            BackColor = Color.FromArgb(40, 42, 48);
            ForeColor = Color.FromArgb(170, 175, 185);
            Font = new Font("Segoe UI", 7.5F);
            _scroll.SmallChange = 1;
            _scroll.ValueChanged += (_, _) => Invalidate();
            Controls.Add(_scroll);
        }

        public int TileSize
        {
            get => _tileSize;
            set
            {
                _tileSize = Math.Clamp(value, 32, 160);
                UpdateScroll();
                EnsureVisible(_focus);
                Invalidate();
            }
        }

        public bool ShowIds { get; set; } = true;

        public IReadOnlyList<int> Items => _ids;

        public int FocusedId => _focus >= 0 && _focus < _ids.Count ? _ids[_focus] : -1;

        /// <summary>
        /// Selected ids in grid order.
        /// </summary>
        public List<int> SelectedIds => _ids.Where(_selected.Contains).ToList();

        public void SetItems(List<int> ids)
        {
            int keep = FocusedId;
            _ids = ids ?? new List<int>();
            _selected.IntersectWith(_ids);
            _focus = keep >= 0 ? _ids.IndexOf(keep) : -1;
            if (_focus < 0 && _ids.Count > 0 && _selected.Count == 0)
            {
                _focus = -1;
            }

            _anchor = _focus;
            UpdateScroll();
            _scroll.Value = 0;
            if (_focus >= 0)
            {
                EnsureVisible(_focus);
            }

            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SelectId(int id)
        {
            int index = _ids.IndexOf(id);
            if (index < 0)
            {
                return;
            }

            _selected.Clear();
            _selected.Add(id);
            _focus = _anchor = index;
            EnsureVisible(index);
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        #region Layout

        private int Columns => Math.Max(1, (ClientSize.Width - _scroll.Width) / _tileSize);

        private int RowCount => (_ids.Count + Columns - 1) / Columns;

        private int VisibleRows => Math.Max(1, ClientSize.Height / _tileSize);

        private void UpdateScroll()
        {
            _scroll.Maximum = Math.Max(0, RowCount - 1);
            _scroll.LargeChange = Math.Max(1, VisibleRows);
            _scroll.Enabled = RowCount > VisibleRows;
            if (_scroll.Value > MaxScroll)
            {
                _scroll.Value = MaxScroll;
            }
        }

        private int MaxScroll => Math.Max(0, RowCount - VisibleRows);

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScroll();
        }

        private void EnsureVisible(int index)
        {
            if (index < 0)
            {
                return;
            }

            int row = index / Columns;
            if (row < _scroll.Value)
            {
                _scroll.Value = row;
            }
            else if (row >= _scroll.Value + VisibleRows)
            {
                _scroll.Value = Math.Min(MaxScroll, row - VisibleRows + 1);
            }
        }

        private int HitTest(Point p)
        {
            if (p.X >= ClientSize.Width - _scroll.Width)
            {
                return -1;
            }

            int col = p.X / _tileSize;
            int row = p.Y / _tileSize + _scroll.Value;
            if (col >= Columns)
            {
                return -1;
            }

            int index = row * Columns + col;
            return index >= 0 && index < _ids.Count ? index : -1;
        }

        #endregion

        #region Painting

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            if (_ids.Count == 0)
            {
                TextRenderer.DrawText(g, "No statics found", Font, ClientRectangle, ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            int cols = Columns;
            int first = _scroll.Value * cols;
            int last = Math.Min(_ids.Count - 1, first + (VisibleRows + 1) * cols - 1);

            using var gridPen = new Pen(Color.FromArgb(55, 58, 66));
            using var selBrush = new SolidBrush(Color.FromArgb(70, 30, 144, 255));
            using var hoverBrush = new SolidBrush(Color.FromArgb(25, 255, 255, 255));
            using var selPen = new Pen(Color.DodgerBlue, 2f);

            for (int i = first; i <= last; i++)
            {
                int col = (i - first) % cols;
                int row = (i - first) / cols;
                var cell = new Rectangle(col * _tileSize, row * _tileSize, _tileSize, _tileSize);
                int id = _ids[i];
                bool selected = _selected.Contains(id);

                if (selected)
                {
                    g.FillRectangle(selBrush, cell);
                }
                else if (i == _hover)
                {
                    g.FillRectangle(hoverBrush, cell);
                }

                g.DrawRectangle(gridPen, cell);

                int textHeight = ShowIds ? Font.Height : 0;
                Rectangle imageArea = Rectangle.Inflate(new Rectangle(cell.X, cell.Y, cell.Width, cell.Height - textHeight), -3, -3);
                DrawStatic(g, id, imageArea);

                if (ShowIds)
                {
                    TextRenderer.DrawText(g, "0x" + id.ToString("X4"), Font,
                        new Rectangle(cell.X, cell.Bottom - textHeight - 1, cell.Width, textHeight),
                        selected ? Color.White : ForeColor, TextFormatFlags.HorizontalCenter);
                }

                if (selected)
                {
                    g.DrawRectangle(selPen, Rectangle.Inflate(cell, -1, -1));
                }

                if (i == _focus && Focused)
                {
                    ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(cell, -3, -3));
                }
            }
        }

        public static void DrawStatic(Graphics g, int id, Rectangle area)
        {
            Bitmap bmp;
            try
            {
                bmp = Art.GetStatic(id);
            }
            catch
            {
                return;
            }

            if (bmp == null || area.Width <= 0 || area.Height <= 0)
            {
                return;
            }

            // scale down to fit, small items are shown up to 2x
            float scale = Math.Min(2f, Math.Min(area.Width / (float)bmp.Width, area.Height / (float)bmp.Height));
            int w = Math.Max(1, (int)(bmp.Width * scale));
            int h = Math.Max(1, (int)(bmp.Height * scale));
            g.DrawImage(bmp, area.X + (area.Width - w) / 2, area.Y + (area.Height - h) / 2, w, h);
        }

        #endregion

        #region Mouse / keyboard

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int index = HitTest(e.Location);
            if (index < 0)
            {
                return;
            }

            if (e.Button == MouseButtons.Right)
            {
                if (!_selected.Contains(_ids[index]))
                {
                    SelectIndex(index, false, false);
                }

                return;
            }

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            bool ctrl = (ModifierKeys & Keys.Control) != 0;
            bool shift = (ModifierKeys & Keys.Shift) != 0;

            // keep a multi selection when the user starts dragging it
            if (!ctrl && !shift && _selected.Contains(_ids[index]) && _selected.Count > 1)
            {
                _focus = index;
            }
            else
            {
                SelectIndex(index, ctrl, shift);
            }

            _mouseDown = true;
            _mouseDownAt = e.Location;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_mouseDown && e.Button == MouseButtons.Left && (ModifierKeys & (Keys.Control | Keys.Shift)) == 0)
            {
                int index = HitTest(e.Location);
                if (index >= 0 && _selected.Count > 1 && index == _focus)
                {
                    SelectIndex(index, false, false);
                }
            }

            _mouseDown = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_mouseDown && e.Button == MouseButtons.Left)
            {
                Size drag = SystemInformation.DragSize;
                if (Math.Abs(e.X - _mouseDownAt.X) > drag.Width || Math.Abs(e.Y - _mouseDownAt.Y) > drag.Height)
                {
                    _mouseDown = false;
                    List<int> ids = SelectedIds;
                    if (ids.Count > 0)
                    {
                        var data = new DataObject();
                        data.SetData(DragFormat, ids.ToArray());
                        DoDragDrop(data, DragDropEffects.Copy);
                    }

                    return;
                }
            }

            int index = HitTest(e.Location);
            if (index != _hover)
            {
                _hover = index;
                Invalidate();
                if (index >= 0)
                {
                    int id = _ids[index];
                    string name = NameProvider?.Invoke(id) ?? "";
                    _toolTip.SetToolTip(this, $"0x{id:X4} ({id})" + (name.Length > 0 ? "  " + name : ""));
                }
                else
                {
                    _toolTip.SetToolTip(this, null);
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = -1;
            Invalidate();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            int index = HitTest(e.Location);
            if (index >= 0 && e.Button == MouseButtons.Left)
            {
                ItemActivated?.Invoke(this, _ids[index]);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int rows = Math.Max(1, SystemInformation.MouseWheelScrollLines / 2);
            int value = _scroll.Value - Math.Sign(e.Delta) * rows;
            _scroll.Value = Math.Clamp(value, 0, MaxScroll);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.PageUp:
                case Keys.PageDown:
                case Keys.Home:
                case Keys.End:
                    return true;
            }

            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (_ids.Count == 0)
            {
                return;
            }

            int cols = Columns;
            int target = _focus < 0 ? 0 : _focus;
            switch (e.KeyCode)
            {
                case Keys.Left: target--; break;
                case Keys.Right: target++; break;
                case Keys.Up: target -= cols; break;
                case Keys.Down: target += cols; break;
                case Keys.PageUp: target -= cols * VisibleRows; break;
                case Keys.PageDown: target += cols * VisibleRows; break;
                case Keys.Home: target = 0; break;
                case Keys.End: target = _ids.Count - 1; break;
                case Keys.Enter:
                    if (FocusedId >= 0)
                    {
                        ItemActivated?.Invoke(this, FocusedId);
                    }

                    e.Handled = true;
                    return;
                case Keys.A when e.Control:
                    _selected.Clear();
                    _selected.UnionWith(_ids);
                    Invalidate();
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    e.Handled = true;
                    return;
                default:
                    return;
            }

            SelectIndex(Math.Clamp(target, 0, _ids.Count - 1), false, e.Shift);
            e.Handled = true;
        }

        private void SelectIndex(int index, bool toggle, bool range)
        {
            if (range && _anchor >= 0)
            {
                _selected.Clear();
                int a = Math.Min(_anchor, index), b = Math.Max(_anchor, index);
                for (int i = a; i <= b; i++)
                {
                    _selected.Add(_ids[i]);
                }
            }
            else if (toggle)
            {
                if (!_selected.Remove(_ids[index]))
                {
                    _selected.Add(_ids[index]);
                }

                _anchor = index;
            }
            else
            {
                _selected.Clear();
                _selected.Add(_ids[index]);
                _anchor = index;
            }

            _focus = index;
            EnsureVisible(index);
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toolTip.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

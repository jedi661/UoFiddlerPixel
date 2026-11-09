using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter
{
    /// <summary>
    /// Scrollable grid of thumbnails. Only the visible cells are drawn, so it works with 65,000 entries.
    /// The owner supplies the images and labels through delegates.
    /// </summary>
    internal sealed class ArtTileGrid : Control
    {
        private readonly VScrollBar _scroll = new VScrollBar { Dock = DockStyle.Right };
        private IReadOnlyList<int> _items = Array.Empty<int>();
        private int _selectedIndex = -1;

        public ArtTileGrid()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint
                     | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.FromArgb(45, 45, 48);
            ForeColor = Color.Gainsboro;
            TabStop = true;
            Controls.Add(_scroll);
            _scroll.Scroll += (s, e) => Invalidate();
            _scroll.ValueChanged += (s, e) => Invalidate();
        }

        public Size CellSize { get; set; } = new Size(66, 80);

        /// <summary>Image of an item. The grid never disposes it.</summary>
        public Func<int, Image> ImageProvider { get; set; }

        public Func<int, string> LabelProvider { get; set; }

        /// <summary>Items drawn with a red label (e.g. free art ids).</summary>
        public Func<int, bool> IsMarked { get; set; }

        public event EventHandler SelectionChanged;

        public event EventHandler ItemActivated;

        public IReadOnlyList<int> Items
        {
            get => _items;
            set
            {
                _items = value ?? Array.Empty<int>();
                _selectedIndex = _items.Count > 0 ? Math.Min(Math.Max(_selectedIndex, 0), _items.Count - 1) : -1;
                UpdateScroll();
                Invalidate();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                int v = _items.Count == 0 ? -1 : Math.Max(0, Math.Min(_items.Count - 1, value));
                if (v == _selectedIndex)
                {
                    return;
                }
                _selectedIndex = v;
                EnsureVisible(v);
                Invalidate();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int SelectedItem => _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : -1;

        /// <summary>Selects the item with the given key (or the nearest following one).</summary>
        public void SelectItem(int key)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] >= key)
                {
                    SelectedIndex = i;
                    return;
                }
            }
            SelectedIndex = _items.Count - 1;
        }

        private int Columns => Math.Max(1, (ClientSize.Width - _scroll.Width) / CellSize.Width);

        private int VisibleRows => Math.Max(1, ClientSize.Height / CellSize.Height);

        private void UpdateScroll()
        {
            int rows = (_items.Count + Columns - 1) / Columns;
            _scroll.Minimum = 0;
            _scroll.LargeChange = VisibleRows;
            _scroll.SmallChange = 1;
            _scroll.Maximum = Math.Max(0, rows - 1);
            _scroll.Enabled = rows > VisibleRows;
            if (_scroll.Value > Math.Max(0, rows - VisibleRows))
            {
                _scroll.Value = Math.Max(0, rows - VisibleRows);
            }
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
                _scroll.Value = Math.Min(_scroll.Maximum, row - VisibleRows + 1);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScroll();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (_items.Count == 0)
            {
                TextRenderer.DrawText(g, Text, Font, ClientRectangle, ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
                return;
            }

            int cols = Columns;
            int first = _scroll.Value * cols;
            int last = Math.Min(_items.Count - 1, first + (VisibleRows + 1) * cols - 1);
            using (var selBrush = new SolidBrush(Color.FromArgb(0, 122, 204)))
            using (var cellBrush = new SolidBrush(Color.FromArgb(30, 30, 30)))
            using (var labelFont = new Font(Font.FontFamily, 7f))
            {
                for (int i = first; i <= last; i++)
                {
                    int col = (i - first) % cols;
                    int row = (i - first) / cols;
                    var cell = new Rectangle(col * CellSize.Width, row * CellSize.Height, CellSize.Width, CellSize.Height);
                    var inner = Rectangle.Inflate(cell, -2, -2);
                    g.FillRectangle(i == _selectedIndex ? selBrush : cellBrush, inner);

                    int key = _items[i];
                    var imageBox = new Rectangle(inner.X + 3, inner.Y + 3, inner.Width - 6, inner.Height - 18);
                    try
                    {
                        Image image = ImageProvider?.Invoke(key);
                        if (image != null)
                        {
                            CutterImaging.DrawFitted(g, image, imageBox, false);
                        }
                    }
                    catch (Exception)
                    {
                        // A broken entry must not stop the drawing.
                    }

                    string label = LabelProvider?.Invoke(key) ?? key.ToString();
                    Color color = IsMarked != null && IsMarked(key) ? Color.IndianRed : ForeColor;
                    TextRenderer.DrawText(g, label, labelFont, new Rectangle(inner.X, inner.Bottom - 15, inner.Width, 14), color,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
                }
            }
        }

        private int HitTest(Point p)
        {
            if (p.X >= ClientSize.Width - _scroll.Width)
            {
                return -1;
            }
            int col = p.X / CellSize.Width;
            int row = p.Y / CellSize.Height;
            if (col >= Columns)
            {
                return -1;
            }
            int index = (_scroll.Value + row) * Columns + col;
            return index < _items.Count ? index : -1;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int i = HitTest(e.Location);
            if (i >= 0)
            {
                SelectedIndex = i;
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.Button == MouseButtons.Left && HitTest(e.Location) >= 0)
            {
                ItemActivated?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int v = _scroll.Value - Math.Sign(e.Delta) * 2;
            _scroll.Value = Math.Max(0, Math.Min(Math.Max(0, _scroll.Maximum - _scroll.LargeChange + 1), v));
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
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
            int cols = Columns;
            switch (e.KeyCode)
            {
                case Keys.Left: SelectedIndex = _selectedIndex - 1; break;
                case Keys.Right: SelectedIndex = _selectedIndex + 1; break;
                case Keys.Up: SelectedIndex = _selectedIndex - cols; break;
                case Keys.Down: SelectedIndex = _selectedIndex + cols; break;
                case Keys.PageUp: SelectedIndex = _selectedIndex - cols * VisibleRows; break;
                case Keys.PageDown: SelectedIndex = _selectedIndex + cols * VisibleRows; break;
                case Keys.Home: SelectedIndex = 0; break;
                case Keys.End: SelectedIndex = _items.Count - 1; break;
                case Keys.Enter: ItemActivated?.Invoke(this, EventArgs.Empty); break;
                default: return;
            }
            e.Handled = true;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }
    }
}

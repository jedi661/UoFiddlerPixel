/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Virtual grid of thumbnails. Images are requested through <see cref="ImageRequested"/> only for visible cells
    /// and kept in a small cache, so lists with tens of thousands of entries stay fast.
    /// </summary>
    public sealed class EcThumbnailGrid : Control
    {
        private readonly VScrollBar _scrollBar;
        private readonly Dictionary<int, Bitmap> _cache = new Dictionary<int, Bitmap>();
        private readonly LinkedList<int> _cacheOrder = new LinkedList<int>();
        private const int CacheLimit = 800;

        private int _itemCount;
        private int _selectedIndex = -1;
        private int _cellSize = 96;

        public event Func<int, Bitmap> ImageRequested;
        public event Func<int, string> LabelRequested;
        public event EventHandler SelectionChanged;
        public event EventHandler ItemActivated;

        public EcThumbnailGrid()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.White;
            _scrollBar = new VScrollBar { Dock = DockStyle.Right };
            _scrollBar.ValueChanged += (s, e) => Invalidate();
            Controls.Add(_scrollBar);
        }

        public int ItemCount
        {
            get => _itemCount;
            set
            {
                _itemCount = Math.Max(0, value);
                ClearCache();
                _selectedIndex = _itemCount == 0 ? -1 : Math.Min(_selectedIndex, _itemCount - 1);
                UpdateScrollBar();
                Invalidate();
            }
        }

        public int CellSize
        {
            get => _cellSize;
            set
            {
                _cellSize = Math.Max(32, value);
                ClearCache();
                UpdateScrollBar();
                Invalidate();
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                int v = _itemCount == 0 ? -1 : Math.Max(-1, Math.Min(_itemCount - 1, value));
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

        private int LabelHeight => Font.Height + 2;
        private int CellWidth => _cellSize + 8;
        private int CellHeight => _cellSize + 8 + LabelHeight;
        private int Columns => Math.Max(1, (ClientSize.Width - _scrollBar.Width) / CellWidth);
        private int Rows => (_itemCount + Columns - 1) / Columns;
        private int VisibleRows => Math.Max(1, ClientSize.Height / CellHeight);

        /// <summary>
        /// Drops the cached thumbnail of one item (after it was changed) or of all items.
        /// </summary>
        public void InvalidateItem(int index)
        {
            if (_cache.TryGetValue(index, out Bitmap bmp))
            {
                bmp.Dispose();
                _cache.Remove(index);
                _cacheOrder.Remove(index);
            }
            Invalidate();
        }

        public void ClearCache()
        {
            foreach (Bitmap bmp in _cache.Values)
            {
                bmp.Dispose();
            }
            _cache.Clear();
            _cacheOrder.Clear();
        }

        public void EnsureVisible(int index)
        {
            if (index < 0)
            {
                return;
            }
            int row = index / Columns;
            if (row < _scrollBar.Value)
            {
                _scrollBar.Value = row;
            }
            else if (row >= _scrollBar.Value + VisibleRows)
            {
                _scrollBar.Value = Math.Min(MaxScroll, row - VisibleRows + 1);
            }
        }

        private int MaxScroll => Math.Max(0, Rows - VisibleRows);

        private void UpdateScrollBar()
        {
            _scrollBar.Minimum = 0;
            _scrollBar.LargeChange = VisibleRows;
            _scrollBar.SmallChange = 1;
            _scrollBar.Maximum = Math.Max(0, Rows - 1);
            if (_scrollBar.Value > MaxScroll)
            {
                _scrollBar.Value = MaxScroll;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScrollBar();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int v = _scrollBar.Value - (Math.Sign(e.Delta) * 2);
            _scrollBar.Value = Math.Max(0, Math.Min(MaxScroll, v));
        }

        private int HitTest(Point p)
        {
            int col = p.X / CellWidth;
            if (col >= Columns)
            {
                return -1;
            }
            int row = (p.Y / CellHeight) + _scrollBar.Value;
            int index = (row * Columns) + col;
            return index < _itemCount ? index : -1;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int index = HitTest(e.Location);
            if (index >= 0)
            {
                SelectedIndex = index;
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (HitTest(e.Location) >= 0)
            {
                ItemActivated?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            int index = Math.Max(0, _selectedIndex);
            switch (e.KeyCode)
            {
                case Keys.Left: SelectedIndex = index - 1; break;
                case Keys.Right: SelectedIndex = index + 1; break;
                case Keys.Up: SelectedIndex = index - Columns; break;
                case Keys.Down: SelectedIndex = index + Columns; break;
                case Keys.PageUp: SelectedIndex = index - (Columns * VisibleRows); break;
                case Keys.PageDown: SelectedIndex = index + (Columns * VisibleRows); break;
                case Keys.Home: SelectedIndex = 0; break;
                case Keys.End: SelectedIndex = _itemCount - 1; break;
                case Keys.Enter: ItemActivated?.Invoke(this, EventArgs.Empty); break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);

            int columns = Columns;
            int first = _scrollBar.Value * columns;
            int last = Math.Min(_itemCount, first + ((VisibleRows + 1) * columns));

            using (var labelBrush = new SolidBrush(ForeColor))
            using (var selectionPen = new Pen(SystemColors.Highlight, 2))
            using (var gridPen = new Pen(Color.Gainsboro))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            {
                for (int index = first; index < last; index++)
                {
                    int col = (index - first) % columns;
                    int row = (index - first) / columns;
                    var cell = new Rectangle(col * CellWidth, row * CellHeight, CellWidth, CellHeight);
                    var imageArea = new Rectangle(cell.X + 4, cell.Y + 4, _cellSize, _cellSize);

                    EcImageHelper.DrawCheckerboard(g, imageArea);
                    g.DrawRectangle(gridPen, imageArea);

                    Bitmap thumb = GetThumbnail(index);
                    if (thumb != null)
                    {
                        g.DrawImage(thumb, imageArea.X + ((imageArea.Width - thumb.Width) / 2), imageArea.Y + ((imageArea.Height - thumb.Height) / 2), thumb.Width, thumb.Height);
                    }

                    string label = LabelRequested?.Invoke(index) ?? index.ToString();
                    g.DrawString(label, Font, labelBrush, new RectangleF(cell.X, imageArea.Bottom + 1, cell.Width, LabelHeight), format);

                    if (index == _selectedIndex)
                    {
                        g.DrawRectangle(selectionPen, cell.X + 1, cell.Y + 1, cell.Width - 3, cell.Height - 3);
                    }
                }
            }
        }

        private Bitmap GetThumbnail(int index)
        {
            if (_cache.TryGetValue(index, out Bitmap cached))
            {
                return cached;
            }

            Bitmap thumb = null;
            try
            {
                using (Bitmap full = ImageRequested?.Invoke(index))
                {
                    if (full != null)
                    {
                        thumb = EcImageHelper.CreateThumbnail(full, _cellSize);
                    }
                }
            }
            catch (Exception)
            {
                thumb = null;
            }

            if (thumb != null)
            {
                _cache[index] = thumb;
                _cacheOrder.AddLast(index);
                while (_cacheOrder.Count > CacheLimit)
                {
                    int old = _cacheOrder.First.Value;
                    _cacheOrder.RemoveFirst();
                    if (_cache.TryGetValue(old, out Bitmap oldBmp))
                    {
                        oldBmp.Dispose();
                        _cache.Remove(old);
                    }
                }
            }
            return thumb;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearCache();
            }
            base.Dispose(disposing);
        }
    }
}

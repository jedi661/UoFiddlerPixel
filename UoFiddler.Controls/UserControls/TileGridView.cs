/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Grid of images (land tiles, statics, lights …) with ids, scrolling,
 * selection and keyboard. Images are only loaded for the visible cells.
 * Can be placed in the Visual Studio designer.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UoFiddler.Controls.UserControls
{
    [ToolboxItem(true)]
    [DesignerCategory("Code")]
    public class TileGridView : ScrollableControl
    {
        private IList<int> _ids = new List<int>();
        private int _selectedIndex = -1;
        private int _cellSize = 96;
        private readonly Dictionary<int, Bitmap> _thumbs = new Dictionary<int, Bitmap>();
        private readonly Font _font = new Font("Segoe UI", 7.5f);

        public TileGridView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            AutoScroll = true;
            BackColor = Color.FromArgb(40, 42, 46);
            ForeColor = Color.Gainsboro;
        }

        /// <summary>raised when the selected id changes</summary>
        public event EventHandler SelectionChanged;

        /// <summary>double click / Enter on a cell</summary>
        public event EventHandler ItemActivated;

        /// <summary>image of an id (null = empty cell)</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<int, Bitmap> ImageProvider { get; set; }

        /// <summary>caption of an id (default "0x0123")</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<int, string> LabelProvider { get; set; }

        /// <summary>images are drawn on black (lights) instead of the cell colour</summary>
        [DefaultValue(false)]
        public bool BlackBackground { get; set; }

        [DefaultValue(96)]
        public int CellSize
        {
            get => _cellSize;
            set
            {
                _cellSize = Math.Max(32, Math.Min(256, value));
                ClearThumbs();
                UpdateScroll();
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList<int> Ids
        {
            get => _ids;
            set
            {
                _ids = value ?? new List<int>();
                _selectedIndex = _ids.Count > 0 ? Math.Min(Math.Max(0, _selectedIndex), _ids.Count - 1) : -1;
                ClearThumbs();
                AutoScrollPosition = Point.Empty;
                UpdateScroll();
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedId
        {
            get => _selectedIndex >= 0 && _selectedIndex < _ids.Count ? _ids[_selectedIndex] : -1;
            set
            {
                int index = _ids.IndexOf(value);
                if (index >= 0)
                {
                    Select(index);
                }
            }
        }

        public void ClearThumbs()
        {
            foreach (Bitmap b in _thumbs.Values)
            {
                b?.Dispose();
            }

            _thumbs.Clear();
        }

        private int Columns => Math.Max(1, (ClientSize.Width - 4) / _cellSize);

        private void UpdateScroll()
        {
            int rows = (_ids.Count + Columns - 1) / Columns;
            AutoScrollMinSize = new Size(0, (rows * _cellSize) + 4);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScroll();
        }

        private Rectangle CellRect(int index)
        {
            int col = index % Columns, row = index / Columns;
            return new Rectangle(2 + (col * _cellSize), 2 + (row * _cellSize) + AutoScrollPosition.Y, _cellSize, _cellSize);
        }

        private Bitmap Thumb(int id)
        {
            if (_thumbs.TryGetValue(id, out Bitmap thumb))
            {
                return thumb;
            }

            Bitmap source = null;
            try
            {
                source = ImageProvider?.Invoke(id);
            }
            catch (Exception)
            {
                // broken image
            }

            if (source == null)
            {
                _thumbs[id] = null;
                return null;
            }

            int max = _cellSize - 18;
            double scale = Math.Min(1.0, Math.Min((double)max / source.Width, (double)max / source.Height));
            int w = Math.Max(1, (int)(source.Width * scale)), h = Math.Max(1, (int)(source.Height * scale));
            thumb = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(thumb))
            {
                g.InterpolationMode = scale < 1 ? InterpolationMode.HighQualityBilinear : InterpolationMode.NearestNeighbor;
                g.DrawImage(source, new Rectangle(0, 0, w, h));
            }

            // keep the memory small: forget the oldest thumbs
            if (_thumbs.Count > 600)
            {
                ClearThumbs();
            }

            _thumbs[id] = thumb;
            return thumb;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (_ids.Count == 0)
            {
                TextRenderer.DrawText(g, DesignMode ? "TileGridView" : "–", Font, new Point(8, 8), ForeColor);
                return;
            }

            int cols = Columns;
            int firstRow = Math.Max(0, (-AutoScrollPosition.Y - 2) / _cellSize);
            int lastRow = ((-AutoScrollPosition.Y + ClientSize.Height) / _cellSize) + 1;
            using (var sel = new SolidBrush(Color.FromArgb(70, 120, 200)))
            using (var cell = new SolidBrush(Color.FromArgb(52, 55, 61)))
            using (var text = new SolidBrush(ForeColor))
            {
                for (int row = firstRow; row <= lastRow; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        int index = (row * cols) + col;
                        if (index >= _ids.Count)
                        {
                            break;
                        }

                        Rectangle r = CellRect(index);
                        Rectangle inner = Rectangle.Inflate(r, -2, -2);
                        g.FillRectangle(index == _selectedIndex ? sel : cell, inner);
                        Rectangle imageArea = new Rectangle(inner.X + 2, inner.Y + 2, inner.Width - 4, inner.Height - 16);
                        if (BlackBackground)
                        {
                            g.FillRectangle(Brushes.Black, imageArea);
                        }

                        Bitmap thumb = DesignMode ? null : Thumb(_ids[index]);
                        if (thumb != null)
                        {
                            g.DrawImage(thumb, imageArea.X + ((imageArea.Width - thumb.Width) / 2), imageArea.Y + ((imageArea.Height - thumb.Height) / 2), thumb.Width, thumb.Height);
                        }

                        string label = LabelProvider?.Invoke(_ids[index]) ?? $"0x{_ids[index]:X4}";
                        g.DrawString(label, _font, text, new RectangleF(inner.X + 2, inner.Bottom - 14, inner.Width - 4, 14),
                            new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap });
                    }
                }
            }
        }

        private int IndexAt(Point p)
        {
            int col = (p.X - 2) / _cellSize, row = (p.Y - 2 - AutoScrollPosition.Y) / _cellSize;
            if (col < 0 || col >= Columns || row < 0)
            {
                return -1;
            }

            int index = (row * Columns) + col;
            return index < _ids.Count ? index : -1;
        }

        private void Select(int index)
        {
            if (index < 0 || index >= _ids.Count)
            {
                return;
            }

            _selectedIndex = index;
            EnsureVisible(index);
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void EnsureVisible(int index)
        {
            Rectangle r = CellRect(index);
            int y = -AutoScrollPosition.Y;
            if (r.Top < 0)
            {
                AutoScrollPosition = new Point(0, y + r.Top - 2);
            }
            else if (r.Bottom > ClientSize.Height)
            {
                AutoScrollPosition = new Point(0, y + (r.Bottom - ClientSize.Height) + 2);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int index = IndexAt(e.Location);
            if (index >= 0)
            {
                Select(index);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (IndexAt(e.Location) >= 0)
            {
                ItemActivated?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Invalidate();
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
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
                case Keys.Enter:
                    return true;
            }

            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            int cols = Columns;
            int rowsVisible = Math.Max(1, ClientSize.Height / _cellSize);
            switch (e.KeyCode)
            {
                case Keys.Left: Select(Math.Max(0, _selectedIndex - 1)); break;
                case Keys.Right: Select(Math.Min(_ids.Count - 1, _selectedIndex + 1)); break;
                case Keys.Up: Select(Math.Max(0, _selectedIndex - cols)); break;
                case Keys.Down: Select(Math.Min(_ids.Count - 1, _selectedIndex + cols)); break;
                case Keys.PageUp: Select(Math.Max(0, _selectedIndex - (cols * rowsVisible))); break;
                case Keys.PageDown: Select(Math.Min(_ids.Count - 1, _selectedIndex + (cols * rowsVisible))); break;
                case Keys.Home: Select(0); break;
                case Keys.End: Select(_ids.Count - 1); break;
                case Keys.Enter: ItemActivated?.Invoke(this, EventArgs.Empty); break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearThumbs();
                _font.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

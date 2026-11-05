// /***************************************************************************
//  *
//  * Dragon Map Studio – picture catalogue of land tiles, textures and items
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls
{
    /// <summary>
    /// Virtual grid of pictures (land art, textures or items) with the hex id below each picture.
    /// Click selects, double click / Enter activates, the mouse wheel and the arrow keys scroll.
    /// </summary>
    [ToolboxItem(true)]
    public class TileGridView : Control
    {
        private readonly VScrollBar _scroll;
        private readonly ToolTip _tip = new ToolTip();
        private IReadOnlyList<int> _ids = Array.Empty<int>();
        private int _selected = -1;
        private int _hover = -1;
        private int _cellSize = 64;

        public TileGridView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.FromArgb(36, 36, 36);
            ForeColor = Color.Gainsboro;
            TabStop = true;
            _scroll = new VScrollBar { Dock = DockStyle.Right };
            _scroll.ValueChanged += (s, e) => Invalidate();
            Controls.Add(_scroll);
        }

        /// <summary>The selected id changed (click, keys or <see cref="SelectedId"/>).</summary>
        public event EventHandler SelectedIdChanged;

        /// <summary>Double click or Enter on a picture.</summary>
        public event EventHandler ItemActivated;

        /// <summary>Picture of an id (may return null: an empty cell is drawn).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<int, Image> ImageOf { get; set; }

        /// <summary>Tooltip text of an id.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<int, string> NameOf { get; set; }

        [Category("Dragon"), DefaultValue(64)]
        public int CellSize
        {
            get => _cellSize;
            set
            {
                _cellSize = Math.Clamp(value, 32, 160);
                UpdateScroll();
                Invalidate();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<int> Ids
        {
            get => _ids;
            set
            {
                _ids = value ?? Array.Empty<int>();
                _hover = -1;
                UpdateScroll();
                EnsureVisible(SelectedId);
                Invalidate();
            }
        }

        /// <summary>Selected id, -1 = none. Setting it does not raise <see cref="SelectedIdChanged"/>.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedId
        {
            get => _selected;
            set
            {
                _selected = value;
                EnsureVisible(value);
                Invalidate();
            }
        }

        /// <summary>The id is in the shown list.</summary>
        public bool Contains(int id) => IndexOf(id) >= 0;

        private int Columns => Math.Max(1, (ClientSize.Width - _scroll.Width) / _cellSize);

        private int Rows => (_ids.Count + Columns - 1) / Columns;

        private int VisibleRows => Math.Max(1, ClientSize.Height / CellHeight);

        private int CellHeight => _cellSize + 14;

        private int IndexOf(int id)
        {
            if (id < 0)
            {
                return -1;
            }
            // the lists are sorted: binary search first, linear as fall back
            if (_ids is List<int> list)
            {
                int i = list.BinarySearch(id);
                if (i >= 0)
                {
                    return i;
                }
            }
            for (int i = 0; i < _ids.Count; i++)
            {
                if (_ids[i] == id)
                {
                    return i;
                }
            }
            return -1;
        }

        public void EnsureVisible(int id)
        {
            int i = IndexOf(id);
            if (i < 0)
            {
                return;
            }
            int row = i / Columns;
            if (row < _scroll.Value)
            {
                _scroll.Value = row;
            }
            else if (row >= _scroll.Value + VisibleRows)
            {
                _scroll.Value = Math.Min(Math.Max(0, row - VisibleRows + 1), MaxScroll);
            }
        }

        private int MaxScroll => Math.Max(0, Rows - VisibleRows);

        private void UpdateScroll()
        {
            _scroll.Minimum = 0;
            _scroll.LargeChange = VisibleRows;
            _scroll.SmallChange = 1;
            _scroll.Maximum = Math.Max(0, Rows - 1);
            _scroll.Enabled = Rows > VisibleRows;
            if (_scroll.Value > MaxScroll)
            {
                _scroll.Value = MaxScroll;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScroll();
        }

        private int HitIndex(Point p)
        {
            int col = p.X / _cellSize, row = p.Y / CellHeight + _scroll.Value;
            if (col >= Columns || p.X >= ClientSize.Width - _scroll.Width)
            {
                return -1;
            }
            int i = row * Columns + col;
            return i >= 0 && i < _ids.Count ? i : -1;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (_ids.Count == 0)
            {
                TextRenderer.DrawText(g, "Keine Einträge.", Font, new Point(8, 8), ForeColor);
                return;
            }
            int cols = Columns;
            using var font = new Font("Segoe UI", 7.5f);
            using var selPen = new Pen(Color.Gold, 2);
            using var hoverPen = new Pen(Color.FromArgb(120, 255, 255, 255));
            using var cellBrush = new SolidBrush(Color.FromArgb(48, 48, 48));
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            for (int r = 0; r <= VisibleRows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int i = (r + _scroll.Value) * cols + c;
                    if (i >= _ids.Count)
                    {
                        break;
                    }
                    int id = _ids[i];
                    var cell = new Rectangle(c * _cellSize, r * CellHeight, _cellSize, CellHeight);
                    var pic = new Rectangle(cell.X + 3, cell.Y + 3, _cellSize - 6, _cellSize - 6);
                    g.FillRectangle(cellBrush, pic);
                    Image img = null;
                    try
                    {
                        img = ImageOf?.Invoke(id);
                    }
                    catch (Exception)
                    {
                        img = null;
                    }
                    if (img != null)
                    {
                        float k = Math.Min(1.5f, Math.Min(pic.Width / (float)img.Width, pic.Height / (float)img.Height));
                        int w = Math.Max(1, (int)(img.Width * k)), h = Math.Max(1, (int)(img.Height * k));
                        g.DrawImage(img, new Rectangle(pic.X + (pic.Width - w) / 2, pic.Bottom - h - (pic.Height - h) / 2, w, h));
                    }
                    TextRenderer.DrawText(g, id.ToString("x4"), font, new Rectangle(cell.X, cell.Bottom - 15, cell.Width, 14), ForeColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    if (id == _selected)
                    {
                        g.DrawRectangle(selPen, cell.X + 1, cell.Y + 1, cell.Width - 3, cell.Height - 3);
                    }
                    else if (i == _hover)
                    {
                        g.DrawRectangle(hoverPen, cell.X + 1, cell.Y + 1, cell.Width - 3, cell.Height - 3);
                    }
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int i = HitIndex(e.Location);
            if (i >= 0 && e.Button == MouseButtons.Left)
            {
                Select(_ids[i]);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (HitIndex(e.Location) >= 0)
            {
                ItemActivated?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int i = HitIndex(e.Location);
            if (i != _hover)
            {
                _hover = i;
                Invalidate();
                string name = i >= 0 ? NameOf?.Invoke(_ids[i]) : null;
                _tip.SetToolTip(this, i >= 0 ? $"{_ids[i]:x4}  {name}" : string.Empty);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = -1;
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            _scroll.Value = Math.Clamp(_scroll.Value - Math.Sign(e.Delta) * 2, 0, MaxScroll);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            Keys k = keyData & Keys.KeyCode;
            return k == Keys.Left || k == Keys.Right || k == Keys.Up || k == Keys.Down || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            int i = IndexOf(_selected);
            int step = e.KeyCode switch
            {
                Keys.Left => -1,
                Keys.Right => 1,
                Keys.Up => -Columns,
                Keys.Down => Columns,
                Keys.PageUp => -Columns * VisibleRows,
                Keys.PageDown => Columns * VisibleRows,
                _ => 0
            };
            if (e.KeyCode == Keys.Enter && i >= 0)
            {
                ItemActivated?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                return;
            }
            if (step != 0 && _ids.Count > 0)
            {
                Select(_ids[Math.Clamp((i < 0 ? 0 : i) + step, 0, _ids.Count - 1)]);
                e.Handled = true;
            }
        }

        private void Select(int id)
        {
            _selected = id;
            EnsureVisible(id);
            Invalidate();
            SelectedIdChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tip.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

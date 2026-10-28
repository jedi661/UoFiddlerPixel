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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Ultima;

namespace UoFiddler.Controls.MapViewer.Editor
{
    /// <summary>
    /// Grid of all land tiles (texture or land art) with search, multi selection (Ctrl/Shift) and drag &amp; drop.
    /// </summary>
    public class LandPaletteControl : UserControl
    {
        /// <summary>Data format for drag &amp; drop (value: ushort land id)</summary>
        public const string DragFormat = "UoFiddlerLandTile";

        private readonly TextBox _search = new TextBox();
        private readonly ComboBox _mode = new ComboBox();
        private readonly CheckBox _textures = new CheckBox();
        private readonly GridPanel _grid;
        private readonly VScrollBar _scroll = new VScrollBar();
        private readonly Label _info = new Label();
        private readonly ArtPixelCache _art;
        private readonly Dictionary<ushort, Bitmap> _thumbs = new Dictionary<ushort, Bitmap>();
        private readonly List<ushort> _selected = new List<ushort>();
        private List<ushort> _items = new List<ushort>();
        private int _anchor = -1;
        private Point _mouseDown;
        private int _downIndex = -1;
        private int _hover = -1;

        public LandPaletteControl() : this(new ArtPixelCache())
        {
        }

        public LandPaletteControl(ArtPixelCache art)
        {
            _art = art;

            _search.Dock = DockStyle.Fill;
            _search.PlaceholderText = "Search: id (0x16, 22, 16-19) or name";
            _search.TextChanged += (s, e) => Filter();

            _mode.DropDownStyle = ComboBoxStyle.DropDownList;
            _mode.Items.AddRange(new object[] { "All valid", "With texture", "Selected" });
            _mode.SelectedIndex = 0;
            _mode.Width = 104;
            _mode.Dock = DockStyle.Right;
            _mode.SelectedIndexChanged += (s, e) => Filter();

            _textures.Text = "Tex";
            _textures.Checked = true;
            _textures.AutoSize = true;
            _textures.Dock = DockStyle.Right;
            _textures.CheckedChanged += (s, e) =>
            {
                ClearThumbs();
                _grid.Invalidate();
            };
            new ToolTip().SetToolTip(_textures, "Show the texture (texmaps) instead of the land art");

            var top = new Panel { Dock = DockStyle.Top, Height = 25, Padding = new Padding(0, 1, 0, 1) };
            top.Controls.Add(_search);
            top.Controls.Add(_textures);
            top.Controls.Add(_mode);

            _info.Dock = DockStyle.Bottom;
            _info.Height = 34;
            _info.Padding = new Padding(2);

            _grid = new GridPanel(this) { Dock = DockStyle.Fill };
            _scroll.Dock = DockStyle.Right;
            _scroll.Scroll += (s, e) => _grid.Invalidate();

            Controls.Add(_grid);
            Controls.Add(_scroll);
            Controls.Add(_info);
            Controls.Add(top);

            CellSize = 52;
        }

        /// <summary>size of one cell in pixels (image + id)</summary>
        public int CellSize
        {
            get => _cellSize;
            set
            {
                _cellSize = Math.Max(24, value);
                UpdateScroll();
                _grid?.Invalidate();
            }
        }

        private int _cellSize = 52;

        /// <summary>selected tiles in click order</summary>
        public IReadOnlyList<ushort> SelectedIds => _selected;

        public event EventHandler SelectionChanged;

        /// <summary>double click on a tile</summary>
        public event EventHandler<LandTileEventArgs> TileActivated;

        /// <summary>Loads the list (call when the files are loaded)</summary>
        public void Reload()
        {
            ClearThumbs();
            Filter();
        }

        public void Select(IEnumerable<ushort> ids, bool scrollTo = true)
        {
            _selected.Clear();
            _selected.AddRange(ids.Distinct());
            if (scrollTo && _selected.Count > 0)
            {
                ScrollTo(_selected[0]);
            }

            UpdateInfo();
            _grid.Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ScrollTo(ushort id)
        {
            int index = _items.IndexOf(id);
            if (index < 0)
            {
                _search.Text = string.Empty;
                _mode.SelectedIndex = 0;
                index = _items.IndexOf(id);
            }

            if (index < 0)
            {
                return;
            }

            int row = index / Columns;
            int visibleRows = Math.Max(1, _grid.ClientSize.Height / CellSize);
            if (row < _scroll.Value || row >= _scroll.Value + visibleRows)
            {
                _scroll.Value = Math.Max(0, Math.Min(Math.Max(0, _scroll.Maximum - _scroll.LargeChange + 1), row - (visibleRows / 2)));
            }

            _grid.Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearThumbs();
            }

            base.Dispose(disposing);
        }

        private void ClearThumbs()
        {
            foreach (Bitmap b in _thumbs.Values)
            {
                b?.Dispose();
            }

            _thumbs.Clear();
        }

        #region Filter

        private void Filter()
        {
            LandData[] table = TileData.LandTable;
            int max = Math.Min(0x4000, table?.Length ?? 0x4000);
            string text = _search.Text.Trim();
            HashSet<ushort> idFilter = null;
            if (text.Length > 0)
            {
                try
                {
                    if (text.Any(char.IsDigit) && text.All(c => char.IsLetterOrDigit(c) || c == ',' || c == ' ' || c == '-' || c == ';'))
                    {
                        bool looksHex = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
                        if (looksHex || text.All(c => char.IsDigit(c) || c == ',' || c == ' ' || c == '-' || c == ';'))
                        {
                            idFilter = new HashSet<ushort>(Transitions.TransitionSet.ParseIds(text));
                        }
                    }
                }
                catch (FormatException)
                {
                    idFilter = null;
                }
            }

            var items = new List<ushort>();
            int mode = _mode.SelectedIndex;
            if (mode == 2)
            {
                items.AddRange(_selected);
            }
            else
            {
                for (int id = 0; id < max; id++)
                {
                    if (idFilter != null)
                    {
                        if (!idFilter.Contains((ushort)id))
                        {
                            continue;
                        }
                    }
                    else if (text.Length > 0)
                    {
                        string name = table != null && id < table.Length ? table[id].Name : null;
                        if (name == null || name.IndexOf(text, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            continue;
                        }
                    }

                    bool hasTexture = table != null && id < table.Length && table[id].TextureId > 0;
                    if (mode == 1 && !hasTexture)
                    {
                        continue;
                    }

                    if (!SafeIsValidLand(id) && !hasTexture)
                    {
                        continue;
                    }

                    items.Add((ushort)id);
                }
            }

            _items = items;
            _hover = -1;
            UpdateScroll();
            UpdateInfo();
            _grid.Invalidate();
        }

        private static bool SafeIsValidLand(int id)
        {
            try
            {
                return Art.IsValidLand(id);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private int Columns => Math.Max(1, _grid.ClientSize.Width / CellSize);

        private void UpdateScroll()
        {
            if (_grid == null || _scroll == null)
            {
                // called by a resize while the control is built
                return;
            }

            int rows = (_items.Count + Columns - 1) / Columns;
            int visible = Math.Max(1, _grid.ClientSize.Height / CellSize);
            _scroll.Minimum = 0;
            _scroll.LargeChange = visible;
            _scroll.SmallChange = 1;
            _scroll.Maximum = Math.Max(0, rows - 1);
            _scroll.Enabled = rows > visible;
            if (_scroll.Value > Math.Max(0, rows - visible))
            {
                _scroll.Value = Math.Max(0, rows - visible);
            }
        }

        private void UpdateInfo()
        {
            string sel = _selected.Count == 0
                ? "Nothing selected"
                : _selected.Count == 1
                    ? $"0x{_selected[0]:X4} {TileName(_selected[0])}"
                    : $"{_selected.Count} selected: " + string.Join(" ", _selected.Take(8).Select(i => $"0x{i:X}")) + (_selected.Count > 8 ? " ..." : string.Empty);
            _info.Text = $"{_items.Count} tiles. {sel}\nClick, Ctrl/Shift+click = several (random variants), drag = drag & drop";
        }

        private static string TileName(ushort id)
        {
            LandData[] table = TileData.LandTable;
            return table != null && id < table.Length ? table[id].Name : string.Empty;
        }

        #endregion

        #region Thumbnails

        private Bitmap GetThumb(ushort id)
        {
            if (_thumbs.TryGetValue(id, out Bitmap bmp))
            {
                return bmp;
            }

            ArtImage img = null;
            try
            {
                LandData[] table = TileData.LandTable;
                int tex = table != null && id < table.Length ? table[id].TextureId : 0;
                if (_textures.Checked && tex > 0)
                {
                    img = _art.GetTexture(tex);
                }

                img ??= _art.GetLand(id);
            }
            catch (Exception)
            {
                img = null;
            }

            bmp = img == null || img.Width == 0 ? null : ToBitmap(img);
            _thumbs[id] = bmp;
            return bmp;
        }

        public static Bitmap ToBitmap(ArtImage img)
        {
            var bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < img.Height; y++)
                {
                    Marshal.Copy(img.Pixels, y * img.Width, bd.Scan0 + (y * bd.Stride), img.Width);
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }

            return bmp;
        }

        /// <summary>Picture of a land tile for other controls (texture if available)</summary>
        public Bitmap GetImage(ushort id) => GetThumb(id);

        #endregion

        #region Grid

        private int IndexAt(Point p)
        {
            int col = p.X / CellSize, row = (p.Y / CellSize) + _scroll.Value;
            if (col >= Columns)
            {
                return -1;
            }

            int index = (row * Columns) + col;
            return index >= 0 && index < _items.Count ? index : -1;
        }

        private void PaintGrid(Graphics g)
        {
            g.Clear(Color.FromArgb(24, 24, 28));
            int cols = Columns;
            int first = _scroll.Value * cols;
            int visibleRows = (_grid.ClientSize.Height / CellSize) + 1;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            using (var font = new Font(Font.FontFamily, 7f))
            using (var selPen = new Pen(Color.DeepSkyBlue, 3))
            using (var hoverPen = new Pen(Color.Goldenrod, 1))
            {
                for (int i = first; i < _items.Count && i < first + (visibleRows * cols); i++)
                {
                    int col = (i - first) % cols, row = (i - first) / cols;
                    var cell = new Rectangle(col * CellSize, row * CellSize, CellSize, CellSize);
                    ushort id = _items[i];
                    var imageRect = new Rectangle(cell.X + 3, cell.Y + 2, CellSize - 6, CellSize - 16);
                    Bitmap bmp = GetThumb(id);
                    if (bmp != null)
                    {
                        g.DrawImage(bmp, imageRect);
                    }
                    else
                    {
                        g.FillRectangle(Brushes.DimGray, imageRect);
                    }

                    g.DrawString($"0x{id:X}", font, Brushes.Gainsboro, cell.X + 3, cell.Bottom - 14);
                    int selIndex = _selected.IndexOf(id);
                    if (selIndex >= 0)
                    {
                        g.DrawRectangle(selPen, cell.X + 1, cell.Y + 1, cell.Width - 3, cell.Height - 3);
                        if (_selected.Count > 1)
                        {
                            g.DrawString((selIndex + 1).ToString(CultureInfo.InvariantCulture), font, Brushes.DeepSkyBlue, cell.Right - 14, cell.Y + 2);
                        }
                    }
                    else if (i == _hover)
                    {
                        g.DrawRectangle(hoverPen, cell.X + 1, cell.Y + 1, cell.Width - 3, cell.Height - 3);
                    }
                }
            }
        }

        private void OnGridMouseDown(MouseEventArgs e)
        {
            _grid.Focus();
            _mouseDown = e.Location;
            _downIndex = IndexAt(e.Location);
            if (_downIndex < 0 || e.Button != MouseButtons.Left)
            {
                return;
            }

            ushort id = _items[_downIndex];
            Keys mods = ModifierKeys;
            if ((mods & Keys.Shift) != 0 && _anchor >= 0 && _anchor < _items.Count)
            {
                int a = Math.Min(_anchor, _downIndex), b = Math.Max(_anchor, _downIndex);
                if ((mods & Keys.Control) == 0)
                {
                    _selected.Clear();
                }

                for (int i = a; i <= b; i++)
                {
                    if (!_selected.Contains(_items[i]))
                    {
                        _selected.Add(_items[i]);
                    }
                }
            }
            else if ((mods & Keys.Control) != 0)
            {
                if (!_selected.Remove(id))
                {
                    _selected.Add(id);
                }

                _anchor = _downIndex;
            }
            else
            {
                if (_selected.Count == 1 && _selected[0] == id)
                {
                    _anchor = _downIndex;
                    return;
                }

                _selected.Clear();
                _selected.Add(id);
                _anchor = _downIndex;
            }

            UpdateInfo();
            _grid.Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnGridMouseMove(MouseEventArgs e)
        {
            int index = IndexAt(e.Location);
            if (index != _hover)
            {
                _hover = index;
                _grid.Invalidate();
                if (index >= 0)
                {
                    ushort id = _items[index];
                    LandData[] table = TileData.LandTable;
                    int tex = table != null && id < table.Length ? table[id].TextureId : 0;
                    _info.Text = $"0x{id:X4} ({id})  {TileName(id)}  texture 0x{tex:X}";
                }
                else
                {
                    UpdateInfo();
                }
            }

            if (e.Button == MouseButtons.Left && _downIndex >= 0 &&
                (Math.Abs(e.X - _mouseDown.X) > 5 || Math.Abs(e.Y - _mouseDown.Y) > 5))
            {
                ushort id = _items[_downIndex];
                _downIndex = -1;
                var data = new DataObject();
                data.SetData(DragFormat, id);
                data.SetData(DataFormats.Text, $"0x{id:X4}");
                _grid.DoDragDrop(data, DragDropEffects.Copy);
            }
        }

        private void OnGridDoubleClick()
        {
            int index = IndexAt(_grid.PointToClient(MousePosition));
            if (index >= 0)
            {
                TileActivated?.Invoke(this, new LandTileEventArgs(_items[index]));
            }
        }

        /// <summary>Reads a land id from drag &amp; drop data</summary>
        public static bool TryGetDragId(IDataObject data, out ushort id)
        {
            id = 0;
            if (data == null)
            {
                return false;
            }

            if (data.GetDataPresent(DragFormat) && data.GetData(DragFormat) is ushort u)
            {
                id = u;
                return true;
            }

            return false;
        }

        /// <summary>Panel with double buffering for the grid</summary>
        private sealed class GridPanel : Panel
        {
            private readonly LandPaletteControl _owner;

            public GridPanel(LandPaletteControl owner)
            {
                _owner = owner;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint |
                         ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
                TabStop = true;
            }

            protected override void OnPaint(PaintEventArgs e) => _owner.PaintGrid(e.Graphics);

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                _owner.UpdateScroll();
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                _owner.OnGridMouseDown(e);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                _owner.OnGridMouseMove(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                _owner._hover = -1;
                Invalidate();
            }

            protected override void OnDoubleClick(EventArgs e)
            {
                base.OnDoubleClick(e);
                _owner.OnGridDoubleClick();
            }

            protected override void OnMouseWheel(MouseEventArgs e)
            {
                base.OnMouseWheel(e);
                ScrollBar s = _owner._scroll;
                int max = Math.Max(0, s.Maximum - s.LargeChange + 1);
                s.Value = Math.Max(0, Math.Min(max, s.Value - (Math.Sign(e.Delta) * 2)));
                Invalidate();
            }
        }

        #endregion
    }

    public sealed class LandTileEventArgs : EventArgs
    {
        public LandTileEventArgs(ushort id) => Id = id;

        public ushort Id { get; }
    }
}

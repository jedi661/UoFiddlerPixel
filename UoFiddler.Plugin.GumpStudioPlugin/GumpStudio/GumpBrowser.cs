using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Ultima;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    public enum BrowserSource { Gumps, Items }

    /// <summary>Thumbnail grid of gumps or items. Drag a picture onto the work area, double click = use it.</summary>
    public class GumpBrowser : Control
    {
        public static readonly int[] KnownBackgrounds = { 83, 2600, 2620, 3000, 3500, 3600, 5054, 5100, 5120, 5150, 5170, 9200, 9250, 9260, 9270, 9300, 9350, 9380, 9390, 9400, 9450, 9500, 9550, 30536 };
        public static readonly Dictionary<int, int> KnownButtons = new Dictionary<int, int>
        {
            [247] = 248, [241] = 242, [4005] = 4007, [4014] = 4016, [4017] = 4019, [4023] = 4025, [4011] = 4013, [4008] = 4010, [4020] = 4022,
            [4026] = 4028, [4029] = 4031, [2117] = 2118, [2151] = 2153, [2103] = 2104, [1209] = 1210, [2224] = 2224, [5540] = 5541, [2445] = 2445,
            [239] = 240, [249] = 250, [242] = 241, [2130] = 2129, [2128] = 2127, [2143] = 2142, [5601] = 5605, [5603] = 5607, [9762] = 9763, [9770] = 9771
        };
        public static readonly Dictionary<int, int> KnownSwitches = new Dictionary<int, int>
        {
            [210] = 211, [208] = 209, [2510] = 2511, [9026] = 9027, [9720] = 9721, [9724] = 9725, [9727] = 9730, [2152] = 2154, [4002] = 4003
        };

        private readonly VScrollBar _scroll = new VScrollBar { Dock = DockStyle.Right };
        private readonly Dictionary<long, Bitmap> _thumbs = new Dictionary<long, Bitmap>();
        private List<int> _items = new List<int>();
        private int _selected = -1;
        private Point _mouseDown;
        private readonly ToolTip _tip = new ToolTip();
        private int _tipIndex = -1;

        public GumpBrowser()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.FromArgb(45, 45, 48);
            ForeColor = Color.Gainsboro;
            Controls.Add(_scroll);
            _scroll.ValueChanged += (s, e) => Invalidate();
        }

        /// <summary>Double click on a picture.</summary>
        public event EventHandler<int> Activated;

        public event EventHandler<int> SelectedChanged;

        [DefaultValue(BrowserSource.Gumps)]
        public BrowserSource Source { get; set; } = BrowserSource.Gumps;

        /// <summary>Element type that a dragged picture becomes (Image, Background, Button, Check, Item …).</summary>
        [DefaultValue(ElementType.Image)]
        public ElementType DropType { get; set; } = ElementType.Image;

        [DefaultValue(typeof(Size), "84, 96")]
        public Size CellSize { get; set; } = new Size(84, 96);

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<int> Items
        {
            get => _items;
            set
            {
                _items = value?.ToList() ?? new List<int>();
                _selected = -1;
                UpdateScroll();
                Invalidate();
            }
        }

        [Browsable(false)]
        public int SelectedId => _selected >= 0 && _selected < _items.Count ? _items[_selected] : -1;

        public void SelectId(int id)
        {
            int i = _items.IndexOf(id);
            if (i < 0)
            {
                return;
            }
            _selected = i;
            int cols = Columns;
            int row = i / cols;
            if (row * CellSize.Height < _scroll.Value || (row + 1) * CellSize.Height > _scroll.Value + ClientSize.Height)
            {
                _scroll.Value = Math.Max(0, Math.Min(_scroll.Maximum, row * CellSize.Height));
            }
            Invalidate();
            SelectedChanged?.Invoke(this, id);
        }

        private int Columns => Math.Max(1, (ClientSize.Width - _scroll.Width) / CellSize.Width);

        private void UpdateScroll()
        {
            int rows = (_items.Count + Columns - 1) / Columns;
            int total = rows * CellSize.Height;
            _scroll.Maximum = Math.Max(0, total);
            _scroll.LargeChange = Math.Max(1, ClientSize.Height);
            _scroll.SmallChange = CellSize.Height;
            _scroll.Enabled = total > ClientSize.Height;
            if (_scroll.Value > Math.Max(0, total - ClientSize.Height))
            {
                _scroll.Value = Math.Max(0, Math.Min(_scroll.Maximum, total - ClientSize.Height));
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScroll();
        }

        private Bitmap Thumb(int id)
        {
            long key = ((long)Source << 32) | (uint)id;
            if (_thumbs.TryGetValue(key, out Bitmap b))
            {
                return b;
            }
            Bitmap src = Source == BrowserSource.Items ? GumpRenderer.Item(id) : GumpRenderer.Gump(id);
            Bitmap thumb = null;
            if (src != null)
            {
                int max = Math.Min(CellSize.Width, CellSize.Height) - 18;
                double s = Math.Min(1.0, Math.Min(max / (double)src.Width, max / (double)src.Height));
                thumb = new Bitmap(Math.Max(1, (int)(src.Width * s)), Math.Max(1, (int)(src.Height * s)));
                using (Graphics g = Graphics.FromImage(thumb))
                {
                    g.InterpolationMode = s < 1 ? InterpolationMode.HighQualityBicubic : InterpolationMode.NearestNeighbor;
                    g.DrawImage(src, new Rectangle(0, 0, thumb.Width, thumb.Height));
                }
            }
            if (_thumbs.Count > 3000)
            {
                foreach (Bitmap t in _thumbs.Values)
                {
                    t?.Dispose();
                }
                _thumbs.Clear();
            }
            _thumbs[key] = thumb;
            return thumb;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (_items.Count == 0)
            {
                TextRenderer.DrawText(g, Text, Font, ClientRectangle, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
                return;
            }
            int cols = Columns, first = _scroll.Value / CellSize.Height * cols;
            int yOff = -(_scroll.Value % CellSize.Height);
            using (var cell = new SolidBrush(Color.FromArgb(58, 58, 62)))
            using (var sel = new SolidBrush(Color.FromArgb(0, 110, 190)))
            using (var font = new Font("Segoe UI", 7.5f))
            {
                for (int i = first; i < _items.Count; i++)
                {
                    int col = (i - first) % cols, row = (i - first) / cols;
                    var r = new Rectangle(col * CellSize.Width + 2, yOff + row * CellSize.Height + 2, CellSize.Width - 4, CellSize.Height - 4);
                    if (r.Top > ClientSize.Height)
                    {
                        break;
                    }
                    g.FillRectangle(i == _selected ? sel : cell, r);
                    Bitmap t = Thumb(_items[i]);
                    if (t != null)
                    {
                        g.DrawImageUnscaled(t, r.X + (r.Width - t.Width) / 2, r.Y + (r.Height - 14 - t.Height) / 2);
                    }
                    TextRenderer.DrawText(g, Source == BrowserSource.Items ? $"0x{_items[i]:X4}" : _items[i].ToString(), font,
                        new Rectangle(r.X, r.Bottom - 15, r.Width, 14), ForeColor, TextFormatFlags.HorizontalCenter);
                }
            }
        }

        private int HitTest(Point p)
        {
            if (p.X >= ClientSize.Width - _scroll.Width)
            {
                return -1;
            }
            int col = p.X / CellSize.Width, row = (p.Y + _scroll.Value) / CellSize.Height;
            if (col >= Columns)
            {
                return -1;
            }
            int i = row * Columns + col;
            return i >= 0 && i < _items.Count ? i : -1;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int i = HitTest(e.Location);
            if (i >= 0)
            {
                _selected = i;
                Invalidate();
                SelectedChanged?.Invoke(this, _items[i]);
            }
            _mouseDown = e.Location;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Left && SelectedId >= 0 && (Math.Abs(e.X - _mouseDown.X) + Math.Abs(e.Y - _mouseDown.Y)) > 6)
            {
                var drop = new GumpDrop { Type = Source == BrowserSource.Items ? ElementType.Item : DropType, Id = SelectedId };
                DoDragDrop(new DataObject(drop), DragDropEffects.Copy);
                return;
            }
            int hit = HitTest(e.Location);
            if (hit != _tipIndex)
            {
                _tipIndex = hit;
                if (hit >= 0)
                {
                    Bitmap src = Source == BrowserSource.Items ? GumpRenderer.Item(_items[hit]) : GumpRenderer.Gump(_items[hit]);
                    _tip.SetToolTip(this, $"{_items[hit]} (0x{_items[hit]:X4})" + (src != null ? $"  {src.Width} × {src.Height}" : string.Empty));
                }
                else
                {
                    _tip.SetToolTip(this, null);
                }
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            int i = HitTest(e.Location);
            if (i >= 0)
            {
                Activated?.Invoke(this, _items[i]);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            _scroll.Value = Math.Max(0, Math.Min(Math.Max(0, _scroll.Maximum - _scroll.LargeChange + 1), _scroll.Value - Math.Sign(e.Delta) * CellSize.Height));
        }

        protected override bool IsInputKey(Keys keyData) =>
            (keyData & Keys.KeyCode) is Keys.Left or Keys.Right or Keys.Up or Keys.Down || base.IsInputKey(keyData);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            int d = e.KeyCode switch { Keys.Left => -1, Keys.Right => 1, Keys.Up => -Columns, Keys.Down => Columns, _ => 0 };
            if (e.KeyCode == Keys.Enter && SelectedId >= 0)
            {
                Activated?.Invoke(this, SelectedId);
                return;
            }
            if (d != 0 && _items.Count > 0)
            {
                SelectId(_items[Math.Max(0, Math.Min(_items.Count - 1, Math.Max(0, _selected) + d))]);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Bitmap t in _thumbs.Values)
                {
                    t?.Dispose();
                }
                _thumbs.Clear();
                _tip.Dispose();
            }
            base.Dispose(disposing);
        }

        // ------------------------------------------------------------------ lists

        /// <summary>All valid gump IDs in a range.</summary>
        public static List<int> ValidGumps(int from, int to)
        {
            var list = new List<int>();
            try
            {
                int count = Gumps.GetCount();
                for (int i = Math.Max(0, from); i < Math.Min(count, to); i++)
                {
                    if (Gumps.IsValidIndex(i))
                    {
                        list.Add(i);
                    }
                }
            }
            catch (Exception)
            {
                // gumps not loaded
            }
            return list;
        }

        public static List<int> ValidItems(string nameFilter)
        {
            var list = new List<int>();
            try
            {
                int max = Art.GetMaxItemId();
                for (int i = 0; i <= max; i++)
                {
                    if (!Art.IsValidStatic(i))
                    {
                        continue;
                    }
                    if (!string.IsNullOrEmpty(nameFilter) && (i >= TileData.ItemTable.Length || (TileData.ItemTable[i].Name ?? string.Empty).IndexOf(nameFilter, StringComparison.OrdinalIgnoreCase) < 0))
                    {
                        continue;
                    }
                    list.Add(i);
                }
            }
            catch (Exception)
            {
                // art not loaded
            }
            return list;
        }
    }

    // ------------------------------------------------------------------ pickers for the property grid

    /// <summary>Small dialog: browser + ID box; returns the chosen ID.</summary>
    public sealed class IdPickerForm : Form
    {
        private readonly GumpBrowser _browser;
        private readonly TextBox _id;

        public int Value { get; private set; }

        public IdPickerForm(BrowserSource source, int current)
        {
            Text = source == BrowserSource.Items ? "Choose item / Item wählen" : "Choose gump / Gump wählen";
            Size = new Size(760, 600);
            StartPosition = FormStartPosition.CenterParent;
            ShowIcon = false;
            MinimizeBox = false;
            _browser = new GumpBrowser { Dock = DockStyle.Fill, Source = source, Text = "Nothing found." };
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(4) };
            var filter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 230 };
            if (source == BrowserSource.Gumps)
            {
                filter.Items.AddRange(new object[] { "Backgrounds (9 parts)", "Buttons", "Check boxes / radios", "0 – 9999", "10000 – 29999", "30000 – 49999", "Paperdoll 50000+", "All" });
            }
            else
            {
                filter.Items.Add("All items");
            }
            var search = new TextBox { Width = 160, PlaceholderText = source == BrowserSource.Items ? "name or ID" : "ID" };
            _id = new TextBox { Width = 80, Text = current.ToString() };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
            top.Controls.AddRange(new Control[] { filter, search, new Label { Text = "ID:", AutoSize = true, Margin = new Padding(8, 7, 2, 0) }, _id, ok, cancel });
            Controls.Add(_browser);
            Controls.Add(top);
            AcceptButton = ok;
            CancelButton = cancel;
            void Fill()
            {
                Cursor = Cursors.WaitCursor;
                string s = search.Text.Trim();
                bool isNum = int.TryParse(s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? s.Substring(2) : s,
                    s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? System.Globalization.NumberStyles.HexNumber : System.Globalization.NumberStyles.Integer, null, out int n);
                if (source == BrowserSource.Items)
                {
                    _browser.Items = isNum ? new List<int> { n } : GumpBrowser.ValidItems(s);
                }
                else
                {
                    List<int> list = filter.SelectedIndex switch
                    {
                        0 => GumpBrowser.KnownBackgrounds.Where(id => GumpRenderer.Gump(id) != null).ToList(),
                        1 => GumpBrowser.KnownButtons.Keys.Where(id => GumpRenderer.Gump(id) != null).ToList(),
                        2 => GumpBrowser.KnownSwitches.Keys.Where(id => GumpRenderer.Gump(id) != null).ToList(),
                        3 => GumpBrowser.ValidGumps(0, 10000),
                        4 => GumpBrowser.ValidGumps(10000, 30000),
                        5 => GumpBrowser.ValidGumps(30000, 50000),
                        6 => GumpBrowser.ValidGumps(50000, 70000),
                        _ => GumpBrowser.ValidGumps(0, 0x10000)
                    };
                    _browser.Items = list;
                    if (isNum)
                    {
                        _browser.SelectId(n);
                    }
                }
                Cursor = Cursors.Default;
            }
            filter.SelectedIndexChanged += (s, e) => Fill();
            search.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Fill(); } };
            _browser.SelectedChanged += (s, id) => _id.Text = id.ToString();
            _browser.Activated += (s, id) => { _id.Text = id.ToString(); DialogResult = DialogResult.OK; };
            FormClosing += (s, e) =>
            {
                if (DialogResult == DialogResult.OK)
                {
                    string t = _id.Text.Trim();
                    Value = t.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt32(t.Substring(2), 16) : int.TryParse(t, out int v) ? v : current;
                }
            };
            Value = current;
            filter.SelectedIndex = source == BrowserSource.Gumps ? (GumpBrowser.KnownBackgrounds.Contains(current) ? 0 : GumpBrowser.KnownButtons.ContainsKey(current) ? 1 : 3) : 0;
            if (source == BrowserSource.Gumps)
            {
                _browser.SelectId(current);
            }
        }
    }

    /// <summary>Hue list with color strips.</summary>
    public sealed class HuePickerForm : Form
    {
        public int Value { get; private set; }

        public HuePickerForm(int current)
        {
            Text = "Choose hue / Hue wählen";
            Size = new Size(460, 600);
            StartPosition = FormStartPosition.CenterParent;
            ShowIcon = false;
            var list = new ListBox { Dock = DockStyle.Fill, DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 20, IntegralHeight = false };
            var search = new TextBox { Dock = DockStyle.Top, PlaceholderText = "name or number / Name oder Nummer" };
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
            buttons.Controls.AddRange(new Control[] { cancel, ok });
            Controls.Add(list);
            Controls.Add(search);
            Controls.Add(buttons);
            AcceptButton = ok;
            CancelButton = cancel;
            var all = new List<(int Number, string Name)> { (0, "(none)") };
            try
            {
                for (int i = 0; Hues.List != null && i < Hues.List.Length; i++)
                {
                    all.Add((i + 1, Hues.List[i].Name?.Trim()));
                }
            }
            catch (Exception)
            {
                // hues missing
            }
            void Fill()
            {
                string f = search.Text.Trim();
                list.BeginUpdate();
                list.Items.Clear();
                foreach ((int n, string name) in all.Where(h => f.Length == 0 || h.Number.ToString() == f || (h.Name ?? string.Empty).IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    list.Items.Add(n);
                }
                list.EndUpdate();
            }
            list.DrawItem += (s, e) =>
            {
                if (e.Index < 0)
                {
                    return;
                }
                e.DrawBackground();
                int n = (int)list.Items[e.Index];
                if (n > 0)
                {
                    try
                    {
                        Hue h = Hues.List[n - 1];
                        float w = 160 / 32f;
                        for (int i = 0; i < 32; i++)
                        {
                            using (var b = new SolidBrush(GumpRenderer.Color555(h.Colors[i])))
                            {
                                e.Graphics.FillRectangle(b, e.Bounds.X + 2 + i * w, e.Bounds.Y + 2, w + 1, e.Bounds.Height - 4);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }
                string name = all.FirstOrDefault(x => x.Number == n).Name;
                TextRenderer.DrawText(e.Graphics, $"{n,5}  {name}", e.Font, new Point(e.Bounds.X + 168, e.Bounds.Y + 2),
                    (e.State & DrawItemState.Selected) != 0 ? SystemColors.HighlightText : SystemColors.ControlText);
            };
            list.DoubleClick += (s, e) => { if (list.SelectedItem != null) DialogResult = DialogResult.OK; };
            search.TextChanged += (s, e) => Fill();
            FormClosing += (s, e) =>
            {
                if (DialogResult == DialogResult.OK && list.SelectedItem is int v)
                {
                    Value = v;
                }
            };
            Value = current;
            Fill();
            int idx = list.Items.IndexOf(current);
            if (idx >= 0)
            {
                list.SelectedIndex = idx;
            }
        }
    }

    public sealed class GumpIdEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            using (var f = new IdPickerForm(BrowserSource.Gumps, value is int i ? i : 0))
            {
                return f.ShowDialog() == DialogResult.OK ? f.Value : value;
            }
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context) => true;

        public override void PaintValue(PaintValueEventArgs e)
        {
            if (e.Value is int id && GumpRenderer.Gump(id) is Bitmap b)
            {
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.DrawImage(b, e.Bounds);
            }
        }
    }

    public sealed class ItemIdEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            using (var f = new IdPickerForm(BrowserSource.Items, value is int i ? i : 0))
            {
                return f.ShowDialog() == DialogResult.OK ? f.Value : value;
            }
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context) => true;

        public override void PaintValue(PaintValueEventArgs e)
        {
            if (e.Value is int id && GumpRenderer.Item(id) is Bitmap b)
            {
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.DrawImage(b, e.Bounds);
            }
        }
    }

    public sealed class HueEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            using (var f = new HuePickerForm(value is int i ? i : 0))
            {
                return f.ShowDialog() == DialogResult.OK ? f.Value : value;
            }
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context) => true;

        public override void PaintValue(PaintValueEventArgs e)
        {
            if (e.Value is int h && h > 0)
            {
                using (var b = new SolidBrush(GumpRenderer.HueColor(h)))
                {
                    e.Graphics.FillRectangle(b, e.Bounds);
                }
            }
        }
    }
}

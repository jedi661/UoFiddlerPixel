/***************************************************************************
 *
 * MultiEditor – choose an area of a map as building background
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Forms
{
    public sealed class MapAreaForm : Form
    {
        private readonly ComboBox _mapCombo;
        private readonly CheckBox _statics;
        private readonly ComboBox _zoomCombo;
        private readonly Panel _view;
        private readonly HScrollBar _hScroll;
        private readonly VScrollBar _vScroll;
        private readonly NumericUpDown _x;
        private readonly NumericUpDown _y;
        private readonly NumericUpDown _w;
        private readonly NumericUpDown _h;
        private readonly NumericUpDown _baseZ;
        private readonly CheckBox _autoZ;
        private readonly CheckBox _newMulti;
        private readonly CheckBox _importStatics;
        private readonly TextBox _findId;
        private readonly Label _status;

        private Map _map;
        private Bitmap _viewBitmap;
        private Rectangle _viewBitmapRect;
        private float _zoom = 2f;
        private bool _dragging;
        private Point _dragStart;
        private readonly List<Point> _found = new List<Point>();
        private bool _updating;

        public MapAreaForm(int mapIndex, Rectangle area, bool allowNewMulti)
        {
            Text = L.T("Map-Bereich als Bau-Hintergrund wählen", "Choose map area as building background");
            Size = new Size(1100, 760);
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            ShowInTaskbar = false;
            KeyPreview = true;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(4), WrapContents = true };
            _mapCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
            for (int i = 0; i < Map.BuiltInCount; ++i)
            {
                string name = i < Options.MapNames.Length ? Options.MapNames[i] : $"Map {i}";
                _mapCombo.Items.Add($"{i}: {name}");
            }

            _statics = new CheckBox { Text = L.T("Statics zeigen", "Show statics"), Checked = true, AutoSize = true, Margin = new Padding(8, 6, 3, 3) };
            _zoomCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 70 };
            _zoomCombo.Items.AddRange(new object[] { "25%", "50%", "100%", "200%", "400%", "800%" });
            _zoomCombo.SelectedIndex = 3;

            _x = Num(0, 7000);
            _y = Num(0, 7000);
            _w = Num(1, 128);
            _h = Num(1, 128);
            _baseZ = new NumericUpDown { Minimum = -128, Maximum = 127, Width = 55 };
            _autoZ = new CheckBox { Text = L.T("Z automatisch", "Auto Z"), Checked = true, AutoSize = true, Margin = new Padding(3, 6, 3, 3) };

            top.Controls.Add(new Label { Text = "Map:", AutoSize = true, Margin = new Padding(3, 7, 0, 0) });
            top.Controls.Add(_mapCombo);
            top.Controls.Add(_statics);
            top.Controls.Add(new Label { Text = "Zoom:", AutoSize = true, Margin = new Padding(8, 7, 0, 0) });
            top.Controls.Add(_zoomCombo);
            top.Controls.Add(new Label { Text = "X:", AutoSize = true, Margin = new Padding(12, 7, 0, 0) });
            top.Controls.Add(_x);
            top.Controls.Add(new Label { Text = "Y:", AutoSize = true, Margin = new Padding(3, 7, 0, 0) });
            top.Controls.Add(_y);
            top.Controls.Add(new Label { Text = L.T("Breite:", "Width:"), AutoSize = true, Margin = new Padding(8, 7, 0, 0) });
            top.Controls.Add(_w);
            top.Controls.Add(new Label { Text = L.T("Tiefe:", "Depth:"), AutoSize = true, Margin = new Padding(3, 7, 0, 0) });
            top.Controls.Add(_h);
            top.Controls.Add(new Label { Text = L.T("Basis-Z:", "Base Z:"), AutoSize = true, Margin = new Padding(8, 7, 0, 0) });
            top.Controls.Add(_baseZ);
            top.Controls.Add(_autoZ);

            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(4) };
            _newMulti = new CheckBox { Text = L.T("Neues leeres Multi in Bereichsgröße", "New blank multi with area size"), Checked = allowNewMulti, AutoSize = true, Margin = new Padding(3, 7, 3, 3) };
            _importStatics = new CheckBox { Text = L.T("Statics im Bereich als bearbeitbare Kacheln übernehmen", "Import statics in area as editable tiles"), AutoSize = true, Margin = new Padding(3, 7, 3, 3) };
            _findId = new TextBox { Width = 70 };
            var find = new Button { Text = L.T("Static suchen", "Find static"), AutoSize = true };
            find.Click += (_, _) => FindStatic();
            var ok = new Button { Text = L.T("Als Hintergrund übernehmen", "Use as background"), DialogResult = DialogResult.OK, AutoSize = true };
            var cancel = new Button { Text = L.T("Abbrechen", "Cancel"), DialogResult = DialogResult.Cancel, AutoSize = true };
            _status = new Label { AutoSize = true, Margin = new Padding(10, 8, 3, 3) };
            bottom.Controls.Add(_newMulti);
            bottom.Controls.Add(_importStatics);
            bottom.Controls.Add(new Label { Text = "ID:", AutoSize = true, Margin = new Padding(12, 8, 0, 0) });
            bottom.Controls.Add(_findId);
            bottom.Controls.Add(find);
            bottom.Controls.Add(ok);
            bottom.Controls.Add(cancel);
            bottom.Controls.Add(_status);
            AcceptButton = ok;
            CancelButton = cancel;

            var help = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 20,
                ForeColor = SystemColors.GrayText,
                Text = L.T(
                    "Linke Maus ziehen = Bereich wählen · Rechte Maus ziehen = verschieben · Mausrad = scrollen, Strg+Rad = Zoom",
                    "Left drag = select area · Right drag = pan · Wheel = scroll, Ctrl+wheel = zoom")
            };

            _view = new DoubleBufferedPanel { Dock = DockStyle.Fill, BackColor = Color.Black };
            _hScroll = new HScrollBar { Dock = DockStyle.Bottom };
            _vScroll = new VScrollBar { Dock = DockStyle.Right };

            Controls.Add(_view);
            Controls.Add(_hScroll);
            Controls.Add(_vScroll);
            Controls.Add(help);
            Controls.Add(bottom);
            Controls.Add(top);

            _view.Paint += View_Paint;
            _view.MouseDown += View_MouseDown;
            _view.MouseMove += View_MouseMove;
            _view.MouseUp += View_MouseUp;
            _view.MouseWheel += View_MouseWheel;
            _view.Resize += (_, _) => UpdateScrollbars();
            _hScroll.ValueChanged += (_, _) => InvalidateView();
            _vScroll.ValueChanged += (_, _) => InvalidateView();
            _mapCombo.SelectedIndexChanged += (_, _) => ChangeMap();
            _statics.CheckedChanged += (_, _) => InvalidateView();
            _zoomCombo.SelectedIndexChanged += (_, _) => ChangeZoom();
            foreach (NumericUpDown n in new[] { _x, _y, _w, _h })
            {
                n.ValueChanged += (_, _) => AreaChanged();
            }

            _baseZ.ValueChanged += (_, _) =>
            {
                if (!_updating)
                {
                    _autoZ.Checked = false;
                }
            };

            _updating = true;
            _x.Value = Math.Max(0, area.X);
            _y.Value = Math.Max(0, area.Y);
            _w.Value = Math.Max(1, Math.Min(128, area.Width));
            _h.Value = Math.Max(1, Math.Min(128, area.Height));
            _updating = false;
            _mapCombo.SelectedIndex = Math.Max(0, Math.Min(Map.BuiltInCount - 1, mapIndex));
        }

        private static NumericUpDown Num(int min, int max)
        {
            return new NumericUpDown { Minimum = min, Maximum = max, Width = 60 };
        }

        public Map SelectedMap => _map;
        public int MapIndex => _mapCombo.SelectedIndex;
        public Rectangle Area => new Rectangle((int)_x.Value, (int)_y.Value, (int)_w.Value, (int)_h.Value);
        public int BaseZ => (int)_baseZ.Value;
        public bool CreateNewMulti => _newMulti.Checked;
        public bool ImportStatics => _importStatics.Checked;

        private void ChangeMap()
        {
            try
            {
                _map = Map.GetBuiltIn(_mapCombo.SelectedIndex);
                if (_map == null || !_map.Tiles.AllFilesExist())
                {
                    _status.Text = L.T("Map-Dateien nicht gefunden.", "Map files not found.");
                }
                else
                {
                    _status.Text = $"{_map.Width} x {_map.Height}";
                }
            }
            catch (Exception ex)
            {
                _map = null;
                _status.Text = ex.Message;
            }

            _x.Maximum = _map?.Width - 1 ?? 7000;
            _y.Maximum = _map?.Height - 1 ?? 7000;
            UpdateScrollbars();
            CenterOnArea();
            UpdateAutoZ();
            InvalidateView();
        }

        private void ChangeZoom()
        {
            float[] zooms = { 0.25f, 0.5f, 1f, 2f, 4f, 8f };
            Point center = new Point(
                _hScroll.Value + (int)(_view.Width / 2 / _zoom),
                _vScroll.Value + (int)(_view.Height / 2 / _zoom));
            _zoom = zooms[Math.Max(0, _zoomCombo.SelectedIndex)];
            UpdateScrollbars();
            ScrollTo(center);
            InvalidateView();
        }

        private void UpdateScrollbars()
        {
            if (_map == null)
            {
                return;
            }

            int visW = (int)(_view.Width / _zoom);
            int visH = (int)(_view.Height / _zoom);
            _hScroll.Maximum = Math.Max(0, _map.Width - 1);
            _vScroll.Maximum = Math.Max(0, _map.Height - 1);
            _hScroll.LargeChange = Math.Max(1, visW);
            _vScroll.LargeChange = Math.Max(1, visH);
            _hScroll.SmallChange = Math.Max(1, visW / 10);
            _vScroll.SmallChange = Math.Max(1, visH / 10);
        }

        private void ScrollTo(Point center)
        {
            int visW = (int)(_view.Width / _zoom);
            int visH = (int)(_view.Height / _zoom);
            _hScroll.Value = Math.Max(_hScroll.Minimum, Math.Min(Math.Max(0, _hScroll.Maximum - _hScroll.LargeChange + 1), center.X - (visW / 2)));
            _vScroll.Value = Math.Max(_vScroll.Minimum, Math.Min(Math.Max(0, _vScroll.Maximum - _vScroll.LargeChange + 1), center.Y - (visH / 2)));
        }

        private void CenterOnArea()
        {
            Rectangle a = Area;
            ScrollTo(new Point(a.X + (a.Width / 2), a.Y + (a.Height / 2)));
        }

        private void AreaChanged()
        {
            if (_updating)
            {
                return;
            }

            UpdateAutoZ();
            InvalidateView();
        }

        private void UpdateAutoZ()
        {
            if (!_autoZ.Checked || _map == null)
            {
                return;
            }

            try
            {
                Rectangle a = Area;
                var zs = new List<int>();
                for (int x = a.X; x < a.Right; ++x)
                {
                    for (int y = a.Y; y < a.Bottom; ++y)
                    {
                        if (x < _map.Width && y < _map.Height)
                        {
                            zs.Add(_map.Tiles.GetLandTile(x, y).Z);
                        }
                    }
                }

                if (zs.Count > 0)
                {
                    zs.Sort();
                    _updating = true;
                    _baseZ.Value = Math.Max(-128, Math.Min(127, zs[zs.Count / 2]));
                    _updating = false;
                    _status.Text = L.T($"Boden Z: min {zs.First()}  max {zs.Last()}  Median {zs[zs.Count / 2]}",
                        $"Ground Z: min {zs.First()}  max {zs.Last()}  median {zs[zs.Count / 2]}");
                }
            }
            catch
            {
                // ignore
            }
        }

        private void InvalidateView()
        {
            _viewBitmap?.Dispose();
            _viewBitmap = null;
            _view.Invalidate();
        }

        private void View_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Black);
            if (_map == null)
            {
                return;
            }

            int visW = (int)Math.Ceiling(_view.Width / _zoom) + 8;
            int visH = (int)Math.Ceiling(_view.Height / _zoom) + 8;
            int bx = Math.Max(0, _hScroll.Value >> 3);
            int by = Math.Max(0, _vScroll.Value >> 3);
            int bw = Math.Min((_map.Width >> 3) - bx, (visW >> 3) + 2);
            int bh = Math.Min((_map.Height >> 3) - by, (visH >> 3) + 2);
            if (bw <= 0 || bh <= 0)
            {
                return;
            }

            if (_viewBitmap == null)
            {
                try
                {
                    _viewBitmap = _map.GetImage(bx, by, bw, bh, _statics.Checked);
                    _viewBitmapRect = new Rectangle(bx << 3, by << 3, bw << 3, bh << 3);
                }
                catch (Exception ex)
                {
                    e.Graphics.DrawString(ex.Message, Font, Brushes.Red, 5, 5);
                    return;
                }
            }

            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            float ox = (_viewBitmapRect.X - _hScroll.Value) * _zoom;
            float oy = (_viewBitmapRect.Y - _vScroll.Value) * _zoom;
            e.Graphics.DrawImage(_viewBitmap, ox, oy, _viewBitmap.Width * _zoom, _viewBitmap.Height * _zoom);

            // selection
            Rectangle a = Area;
            RectangleF sel = new RectangleF((a.X - _hScroll.Value) * _zoom, (a.Y - _vScroll.Value) * _zoom, a.Width * _zoom, a.Height * _zoom);
            using (var fill = new SolidBrush(Color.FromArgb(50, 255, 255, 0)))
            {
                e.Graphics.FillRectangle(fill, sel);
            }

            using (var pen = new Pen(Color.Yellow, 2))
            {
                e.Graphics.DrawRectangle(pen, sel.X, sel.Y, sel.Width, sel.Height);
            }

            // grid at high zoom
            if (_zoom >= 4)
            {
                using var gp = new Pen(Color.FromArgb(60, 255, 255, 255));
                for (int x = 0; x < visW; ++x)
                {
                    float px = (x + (_hScroll.Value - _hScroll.Value)) * _zoom;
                    e.Graphics.DrawLine(gp, px, 0, px, _view.Height);
                }

                for (int y = 0; y < visH; ++y)
                {
                    float py = y * _zoom;
                    e.Graphics.DrawLine(gp, 0, py, _view.Width, py);
                }
            }

            foreach (Point p in _found)
            {
                float px = (p.X - _hScroll.Value) * _zoom;
                float py = (p.Y - _vScroll.Value) * _zoom;
                float r = Math.Max(3, _zoom);
                e.Graphics.FillEllipse(Brushes.Magenta, px - r, py - r, r * 2, r * 2);
            }
        }

        private Point ToMap(Point p)
        {
            return new Point(_hScroll.Value + (int)(p.X / _zoom), _vScroll.Value + (int)(p.Y / _zoom));
        }

        private Point _panStart;
        private Point _panScroll;
        private bool _panning;

        private void View_MouseDown(object sender, MouseEventArgs e)
        {
            _view.Focus();
            if (e.Button == MouseButtons.Left)
            {
                _dragging = true;
                _dragStart = ToMap(e.Location);
            }
            else if (e.Button == MouseButtons.Right)
            {
                _panning = true;
                _panStart = e.Location;
                _panScroll = new Point(_hScroll.Value, _vScroll.Value);
                _view.Cursor = Cursors.Hand;
            }
        }

        private void View_MouseMove(object sender, MouseEventArgs e)
        {
            Point m = ToMap(e.Location);
            if (_map != null && m.X >= 0 && m.Y >= 0 && m.X < _map.Width && m.Y < _map.Height)
            {
                try
                {
                    Tile land = _map.Tiles.GetLandTile(m.X, m.Y);
                    HuedTile[] statics = _map.Tiles.GetStaticTiles(m.X, m.Y);
                    string st = statics.Length > 0 ? string.Join(", ", statics.Take(4).Select(s => $"0x{s.Id:X4}@{s.Z}")) : "-";
                    _status.Text = $"{m.X},{m.Y}  Land 0x{land.Id:X4} Z {land.Z}  Statics: {st}";
                }
                catch
                {
                    // ignore
                }
            }

            if (_dragging)
            {
                Point s = _dragStart;
                _updating = true;
                _x.Value = Math.Max(_x.Minimum, Math.Min(_x.Maximum, Math.Min(s.X, m.X)));
                _y.Value = Math.Max(_y.Minimum, Math.Min(_y.Maximum, Math.Min(s.Y, m.Y)));
                _w.Value = Math.Max(1, Math.Min(_w.Maximum, Math.Abs(m.X - s.X) + 1));
                _h.Value = Math.Max(1, Math.Min(_h.Maximum, Math.Abs(m.Y - s.Y) + 1));
                _updating = false;
                _view.Invalidate();
            }
            else if (_panning)
            {
                int dx = (int)((_panStart.X - e.X) / _zoom);
                int dy = (int)((_panStart.Y - e.Y) / _zoom);
                _hScroll.Value = Math.Max(0, Math.Min(Math.Max(0, _hScroll.Maximum - _hScroll.LargeChange + 1), _panScroll.X + dx));
                _vScroll.Value = Math.Max(0, Math.Min(Math.Max(0, _vScroll.Maximum - _vScroll.LargeChange + 1), _panScroll.Y + dy));
            }
        }

        private void View_MouseUp(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                _dragging = false;
                UpdateAutoZ();
            }

            _panning = false;
            _view.Cursor = Cursors.Default;
            _view.Invalidate();
        }

        private void View_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) != 0)
            {
                int idx = _zoomCombo.SelectedIndex + (e.Delta > 0 ? 1 : -1);
                _zoomCombo.SelectedIndex = Math.Max(0, Math.Min(_zoomCombo.Items.Count - 1, idx));
                return;
            }

            ScrollBar bar = (ModifierKeys & Keys.Shift) != 0 ? _hScroll : _vScroll;
            int v = bar.Value - (Math.Sign(e.Delta) * bar.SmallChange * 3);
            bar.Value = Math.Max(0, Math.Min(Math.Max(0, bar.Maximum - bar.LargeChange + 1), v));
        }

        private void FindStatic()
        {
            _found.Clear();
            if (_map == null || !SimpleDialog.TryParseId(_findId.Text, out int id))
            {
                _status.Text = L.T("ID eingeben (z.B. 0x0B2C oder 2860)", "Enter id (e.g. 0x0B2C or 2860)");
                return;
            }

            // search visible area (max 512x512)
            int visW = Math.Min(512, (int)(_view.Width / _zoom));
            int visH = Math.Min(512, (int)(_view.Height / _zoom));
            for (int x = _hScroll.Value; x < _hScroll.Value + visW && x < _map.Width; ++x)
            {
                for (int y = _vScroll.Value; y < _vScroll.Value + visH && y < _map.Height; ++y)
                {
                    if (_map.Tiles.GetStaticTiles(x, y).Any(s => s.Id == id))
                    {
                        _found.Add(new Point(x, y));
                    }
                }
            }

            _status.Text = L.T($"{_found.Count} x 0x{id:X4} im sichtbaren Bereich gefunden", $"{_found.Count} x 0x{id:X4} found in visible area");
            _view.Invalidate();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            UpdateScrollbars();
            CenterOnArea();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _viewBitmap?.Dispose();
            }

            base.Dispose(disposing);
        }

        private sealed class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                DoubleBuffered = true;
                SetStyle(ControlStyles.Selectable, true);
                TabStop = true;
            }
        }
    }
}

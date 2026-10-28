/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Ultima;

namespace UoFiddler.Controls.MapViewer.Editor
{
    /// <summary>
    /// Top view of a whole map (radar colors like the map tab). Mouse wheel = zoom, right/middle mouse = move,
    /// left mouse = mark an area (<see cref="SelectMode"/>) or click a position.
    /// </summary>
    public class MapOverviewControl : Control
    {
        private static readonly float[] _zoomLevels = { 0.0625f, 0.125f, 0.25f, 0.5f, 1f, 2f, 4f, 8f };

        private Map _map;
        private float _zoom = 0.125f;
        private float _originX;
        private float _originY;
        private Bitmap _image;
        private Rectangle _imageBlocks;
        private bool _imageStatics;
        private string _error;

        private Point _mouseDown;
        private bool _dragging;
        private bool _panning;
        private PointF _panStartOrigin;
        private Point? _selectStart;
        private readonly Timer _thumbTimer;

        /// <summary>below this zoom the shared small picture of the whole map is used</summary>
        private const float ThumbnailZoom = 0.25f;

        public MapOverviewControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.Black;
            _thumbTimer = new Timer { Interval = 20 };
            _thumbTimer.Tick += OnThumbTimer;
        }

        private bool UsesThumbnail => _zoom <= ThumbnailZoom;

        private void OnThumbTimer(object sender, EventArgs e)
        {
            if (_map == null || !UsesThumbnail || !Visible)
            {
                _thumbTimer.Stop();
                return;
            }

            OverviewThumbnail thumb = OverviewThumbnail.Get(_map, ShowStatics);
            if (!thumb.Step(35))
            {
                _thumbTimer.Stop();
            }

            Invalidate();
        }

        /// <summary>The shown map (null = empty)</summary>
        public Map Map
        {
            get => _map;
            set
            {
                _map = value;
                _error = null;
                DisposeImage();
                Selection = null;
                Highlight = null;
                ZoomToFit();
            }
        }

        public bool ShowStatics { get; set; } = true;

        /// <summary>Left mouse drag marks an area</summary>
        public bool SelectMode { get; set; } = true;

        /// <summary>Marked area (map coordinates)</summary>
        public Rectangle? Selection { get; set; }

        /// <summary>A second area, e.g. the loaded working area or the paste position</summary>
        public Rectangle? Highlight { get; set; }

        public Color HighlightColor { get; set; } = Color.Cyan;

        public float Zoom => _zoom;

        /// <summary>The marked area changed by the mouse</summary>
        public event EventHandler SelectionChanged;

        /// <summary>A position was clicked (without moving the mouse)</summary>
        public event EventHandler<MapPositionEventArgs> PositionClicked;

        /// <summary>Map coordinates under the mouse (or null)</summary>
        public event EventHandler<MapPositionEventArgs> PositionHover;

        public void ZoomToFit()
        {
            if (_map == null || ClientSize.Width <= 0 || ClientSize.Height <= 0)
            {
                Invalidate();
                return;
            }

            float fit = Math.Min(ClientSize.Width / (float)_map.Width, ClientSize.Height / (float)_map.Height);
            _zoom = _zoomLevels[0];
            foreach (float z in _zoomLevels)
            {
                if (z <= fit)
                {
                    _zoom = z;
                }
            }

            _originX = (_map.Width - (ClientSize.Width / _zoom)) / 2f;
            _originY = (_map.Height - (ClientSize.Height / _zoom)) / 2f;
            Invalidate();
        }

        /// <summary>Centers the view on a map position</summary>
        public void CenterOn(int x, int y)
        {
            _originX = x - (ClientSize.Width / _zoom / 2f);
            _originY = y - (ClientSize.Height / _zoom / 2f);
            ClampOrigin();
            Invalidate();
        }

        /// <summary>Shows a rectangle completely (zooms in if possible)</summary>
        public void ShowArea(Rectangle area)
        {
            if (_map == null)
            {
                return;
            }

            float fit = Math.Min(ClientSize.Width / (float)Math.Max(1, area.Width * 3), ClientSize.Height / (float)Math.Max(1, area.Height * 3));
            float zoom = _zoomLevels[0];
            foreach (float z in _zoomLevels)
            {
                if (z <= fit)
                {
                    zoom = z;
                }
            }

            _zoom = zoom;
            CenterOn(area.X + (area.Width / 2), area.Y + (area.Height / 2));
        }

        /// <summary>Forgets the rendered image (after the map was changed)</summary>
        /// <param name="area">changed area (null = everything)</param>
        public void RefreshMap(Rectangle? area = null)
        {
            _error = null;
            DisposeImage();
            if (_map != null)
            {
                OverviewThumbnail.Invalidate(_map, area);
            }

            Invalidate();
        }

        public Point ScreenToMap(Point p)
        {
            return new Point((int)Math.Floor(_originX + (p.X / _zoom)), (int)Math.Floor(_originY + (p.Y / _zoom)));
        }

        private PointF MapToScreen(float x, float y)
        {
            return new PointF((x - _originX) * _zoom, (y - _originY) * _zoom);
        }

        private RectangleF MapToScreen(Rectangle r)
        {
            PointF p = MapToScreen(r.X, r.Y);
            return new RectangleF(p.X, p.Y, r.Width * _zoom, r.Height * _zoom);
        }

        private void ClampOrigin()
        {
            if (_map == null)
            {
                return;
            }

            float viewW = ClientSize.Width / _zoom, viewH = ClientSize.Height / _zoom;
            _originX = viewW >= _map.Width ? (_map.Width - viewW) / 2f : Math.Max(0, Math.Min(_map.Width - viewW, _originX));
            _originY = viewH >= _map.Height ? (_map.Height - viewH) / 2f : Math.Max(0, Math.Min(_map.Height - viewH, _originY));
        }

        private void DisposeImage()
        {
            _image?.Dispose();
            _image = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _thumbTimer.Dispose();
                DisposeImage();
            }

            base.Dispose(disposing);
        }

        #region Paint

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);

            if (_map == null)
            {
                g.DrawString("No map", Font, Brushes.Gray, 8, 8);
                return;
            }

            if (_error != null)
            {
                g.DrawString(_error, Font, Brushes.OrangeRed, new RectangleF(8, 8, ClientSize.Width - 16, ClientSize.Height - 16));
                return;
            }

            if (UsesThumbnail)
            {
                PaintThumbnail(g);
                PaintMarks(g);
                return;
            }

            // visible 8x8 blocks
            int blocksW = _map.Width >> 3, blocksH = _map.Height >> 3;
            int bx0 = Math.Max(0, (int)Math.Floor(_originX / 8f));
            int by0 = Math.Max(0, (int)Math.Floor(_originY / 8f));
            int bx1 = Math.Min(blocksW, (int)Math.Ceiling((_originX + (ClientSize.Width / _zoom)) / 8f) + 1);
            int by1 = Math.Min(blocksH, (int)Math.Ceiling((_originY + (ClientSize.Height / _zoom)) / 8f) + 1);
            if (bx1 <= bx0 || by1 <= by0)
            {
                return;
            }

            var blocks = new Rectangle(bx0, by0, bx1 - bx0, by1 - by0);
            if (_image == null || _imageBlocks != blocks || _imageStatics != ShowStatics)
            {
                DisposeImage();
                try
                {
                    _image = _map.GetImage(blocks.X, blocks.Y, blocks.Width, blocks.Height, ShowStatics);
                    _imageBlocks = blocks;
                    _imageStatics = ShowStatics;
                }
                catch (Exception ex)
                {
                    DisposeImage();
                    _map.ResetCache();
                    _error = $"The map can not be read:\n{ex.GetType().Name}: {ex.Message}";
                    g.DrawString(_error, Font, Brushes.OrangeRed, new RectangleF(8, 8, ClientSize.Width - 16, ClientSize.Height - 16));
                    return;
                }
            }

            g.InterpolationMode = _zoom >= 1 ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBilinear;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            RectangleF dest = MapToScreen(new Rectangle(blocks.X << 3, blocks.Y << 3, blocks.Width << 3, blocks.Height << 3));
            g.DrawImage(_image, dest);
            g.PixelOffsetMode = PixelOffsetMode.Default;
            PaintMarks(g);
        }

        private void PaintThumbnail(Graphics g)
        {
            OverviewThumbnail thumb = OverviewThumbnail.Get(_map, ShowStatics);
            if (thumb.Error != null)
            {
                g.DrawString(thumb.Error, Font, Brushes.OrangeRed, new RectangleF(8, 8, ClientSize.Width - 16, ClientSize.Height - 16));
                return;
            }

            g.InterpolationMode = _zoom >= ThumbnailZoom ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBilinear;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(thumb.GetBitmap(), MapToScreen(new Rectangle(0, 0, thumb.Width * OverviewThumbnail.Scale, thumb.Height * OverviewThumbnail.Scale)));
            g.PixelOffsetMode = PixelOffsetMode.Default;

            if (!thumb.IsComplete)
            {
                string text = $"Loading map overview... {thumb.Progress * 100:0}%";
                SizeF size = g.MeasureString(text, Font);
                g.FillRectangle(Brushes.Black, 4, ClientSize.Height - size.Height - 8, size.Width + 8, size.Height + 4);
                g.DrawString(text, Font, Brushes.Gold, 8, ClientSize.Height - size.Height - 6);
                if (!_thumbTimer.Enabled)
                {
                    _thumbTimer.Start();
                }
            }
        }

        private void PaintMarks(Graphics g)
        {
            if (Highlight.HasValue)
            {
                DrawRect(g, Highlight.Value, HighlightColor);
            }

            if (Selection.HasValue)
            {
                DrawRect(g, Selection.Value, Color.Red);
            }
        }

        private void DrawRect(Graphics g, Rectangle area, Color color)
        {
            RectangleF r = MapToScreen(area);
            r.Width = Math.Max(2, r.Width);
            r.Height = Math.Max(2, r.Height);
            using (var fill = new SolidBrush(Color.FromArgb(40, color)))
            using (var pen = new Pen(color, 2))
            {
                g.FillRectangle(fill, r);
                g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
            }
        }

        #endregion

        #region Mouse

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ClampOrigin();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_map == null)
            {
                return;
            }

            int index = Array.IndexOf(_zoomLevels, _zoom);
            int next = Math.Max(0, Math.Min(_zoomLevels.Length - 1, index + (e.Delta > 0 ? 1 : -1)));
            if (next == index)
            {
                return;
            }

            // keep the position under the mouse
            float mx = _originX + (e.X / _zoom), my = _originY + (e.Y / _zoom);
            _zoom = _zoomLevels[next];
            _originX = mx - (e.X / _zoom);
            _originY = my - (e.Y / _zoom);
            ClampOrigin();
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (_map == null)
            {
                return;
            }

            _mouseDown = e.Location;
            _dragging = false;
            _panStartOrigin = new PointF(_originX, _originY);
            _panning = e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && !SelectMode);
            _selectStart = e.Button == MouseButtons.Left && SelectMode ? ClampToMap(ScreenToMap(e.Location)) : (Point?)null;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_map == null)
            {
                return;
            }

            Point pos = ScreenToMap(e.Location);
            if (pos.X >= 0 && pos.Y >= 0 && pos.X < _map.Width && pos.Y < _map.Height)
            {
                PositionHover?.Invoke(this, new MapPositionEventArgs(pos.X, pos.Y));
            }

            if (e.Button == MouseButtons.None)
            {
                return;
            }

            if (!_dragging && (Math.Abs(e.X - _mouseDown.X) > 3 || Math.Abs(e.Y - _mouseDown.Y) > 3))
            {
                _dragging = true;
            }

            if (!_dragging)
            {
                return;
            }

            if (_panning)
            {
                _originX = _panStartOrigin.X - ((e.X - _mouseDown.X) / _zoom);
                _originY = _panStartOrigin.Y - ((e.Y - _mouseDown.Y) / _zoom);
                ClampOrigin();
                Invalidate();
            }
            else if (_selectStart.HasValue)
            {
                Point end = ClampToMap(pos);
                Point s = _selectStart.Value;
                Selection = Rectangle.FromLTRB(Math.Min(s.X, end.X), Math.Min(s.Y, end.Y), Math.Max(s.X, end.X) + 1, Math.Max(s.Y, end.Y) + 1);
                Invalidate();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_map == null)
            {
                return;
            }

            if (!_dragging && e.Button == MouseButtons.Left)
            {
                Point pos = ScreenToMap(e.Location);
                if (pos.X >= 0 && pos.Y >= 0 && pos.X < _map.Width && pos.Y < _map.Height)
                {
                    PositionClicked?.Invoke(this, new MapPositionEventArgs(pos.X, pos.Y));
                }
            }

            _dragging = false;
            _panning = false;
            _selectStart = null;
        }

        private Point ClampToMap(Point p)
        {
            return new Point(Math.Max(0, Math.Min(_map.Width - 1, p.X)), Math.Max(0, Math.Min(_map.Height - 1, p.Y)));
        }

        #endregion
    }

    public sealed class MapPositionEventArgs : EventArgs
    {
        public MapPositionEventArgs(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }
    }
}

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AltitudeTool
{
    internal enum HeightTool { Select, Raise, Lower, Smooth, Flatten, Pick }

    /// <summary>Zoomable view of a height map: wheel = zoom, right / middle drag = move, left = selection or brush.</summary>
    internal sealed class HeightView : Control
    {
        private HeightMap _map;
        private Bitmap _cache;
        private bool _dirty = true;
        private PointF _origin;       // map tile at the top left corner
        private float _scale = 1f;    // screen pixels per tile
        private Point _panStart;
        private PointF _panOrigin;
        private bool _panning, _selecting, _painting;
        private Point _selStart;
        private Rectangle _selection;
        private Point _hover = new Point(-1, -1);
        private Rectangle _strokeBounds;
        private int[] _palette;

        public HeightView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.FromArgb(30, 30, 30);
        }

        public event EventHandler<Point> HoverTile;
        public event EventHandler SelectionChanged;
        public event EventHandler ViewChanged;
        public event EventHandler BrushStarted;
        public event EventHandler<Rectangle> BrushEnded;
        public event EventHandler<int> ZPicked;

        public HeightMap Map
        {
            get => _map;
            set
            {
                _map = value;
                _selection = Rectangle.Empty;
                UpdatePalette();
                FitAll();
            }
        }

        /// <summary>Unchanged map – changed tiles are tinted when <see cref="ShowChanges"/> is on.</summary>
        public HeightMap Original { get; set; }

        public bool ShowChanges { get; set; } = true;
        public HeightColorMode ColorMode { get; set; } = HeightColorMode.TerrainShade;
        public int Contour { get; set; }
        public bool ShowGrid { get; set; } = true;
        public HeightTool Tool { get; set; } = HeightTool.Select;
        public int BrushRadius { get; set; } = 6;
        public int BrushStrength { get; set; } = 2;
        public int FlattenZ { get; set; }

        public float ScaleFactor => _scale;

        public Point HoverPoint => _hover;

        /// <summary>Selected tiles (empty = none).</summary>
        public Rectangle Selection
        {
            get => _selection;
            set
            {
                _selection = _map == null ? Rectangle.Empty : Rectangle.Intersect(value, _map.Bounds);
                Invalidate();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void UpdatePalette()
        {
            if (_map == null)
            {
                return;
            }
            int min = 0, max = 0;
            if (ColorMode == HeightColorMode.GrayStretch)
            {
                _map.Histogram(_map.Bounds, out min, out max, out _);
            }
            _palette = HeightOps.MakePalette(ColorMode, min, max);
            InvalidateMap();
        }

        public void InvalidateMap()
        {
            _dirty = true;
            Invalidate();
        }

        public void FitAll()
        {
            if (_map == null || Width <= 0 || Height <= 0)
            {
                InvalidateMap();
                return;
            }
            _scale = Math.Min(Width / (float)_map.Width, Height / (float)_map.Height);
            _scale = SnapScale(_scale, false);
            _origin = new PointF((_map.Width - Width / _scale) / 2, (_map.Height - Height / _scale) / 2);
            InvalidateMap();
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ZoomTo(Rectangle tiles)
        {
            if (_map == null || tiles.Width <= 0 || tiles.Height <= 0)
            {
                return;
            }
            _scale = SnapScale(Math.Min(Width / (float)tiles.Width, Height / (float)tiles.Height), false);
            _origin = new PointF(tiles.X + tiles.Width / 2f - Width / _scale / 2, tiles.Y + tiles.Height / 2f - Height / _scale / 2);
            InvalidateMap();
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        public void CenterOn(int x, int y)
        {
            _origin = new PointF(x - Width / _scale / 2, y - Height / _scale / 2);
            InvalidateMap();
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ZoomStep(int dir, Point? around = null)
        {
            Point p = around ?? new Point(Width / 2, Height / 2);
            PointF tile = ScreenToMapF(p);
            float s = SnapScale(dir > 0 ? _scale * 2 : _scale / 2, true);
            _scale = Math.Max(1 / 32f, Math.Min(32f, s));
            _origin = new PointF(tile.X - p.X / _scale, tile.Y - p.Y / _scale);
            InvalidateMap();
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        private static float SnapScale(float s, bool exact)
        {
            // powers of two keep the pixels sharp
            double l = Math.Log(Math.Max(1 / 32.0, Math.Min(32, s)), 2);
            return (float)Math.Pow(2, exact ? Math.Round(l) : Math.Floor(l));
        }

        private PointF ScreenToMapF(Point p) => new PointF(_origin.X + p.X / _scale, _origin.Y + p.Y / _scale);

        public Point ScreenToMap(Point p)
        {
            PointF f = ScreenToMapF(p);
            return new Point((int)Math.Floor(f.X), (int)Math.Floor(f.Y));
        }

        private RectangleF MapToScreen(Rectangle r) => new RectangleF((r.X - _origin.X) * _scale, (r.Y - _origin.Y) * _scale, r.Width * _scale, r.Height * _scale);

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            InvalidateMap();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (_map == null)
            {
                g.Clear(BackColor);
                TextRenderer.DrawText(g, "No map loaded.\nKeine Karte geladen.", Font, ClientRectangle, Color.Gainsboro,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }
            if (_dirty || _cache == null || _cache.Width != Math.Max(1, Width) || _cache.Height != Math.Max(1, Height))
            {
                if (_cache == null || _cache.Width != Math.Max(1, Width) || _cache.Height != Math.Max(1, Height))
                {
                    _cache?.Dispose();
                    _cache = new Bitmap(Math.Max(1, Width), Math.Max(1, Height), PixelFormat.Format32bppArgb);
                }
                if (_palette == null)
                {
                    _palette = HeightOps.MakePalette(ColorMode, 0, 0);
                }
                HeightOps.Render(_map, new RectangleF(_origin.X, _origin.Y, Width / _scale, Height / _scale), _cache, ColorMode, _palette, Contour,
                    ShowChanges ? Original : null);
                _dirty = false;
            }
            g.DrawImageUnscaled(_cache, 0, 0);

            if (ShowGrid && _scale >= 4)
            {
                // block grid (8 × 8 tiles)
                using (var pen = new Pen(Color.FromArgb(70, 255, 255, 255)))
                {
                    int x0 = (int)Math.Floor(_origin.X / 8) * 8, y0 = (int)Math.Floor(_origin.Y / 8) * 8;
                    for (int x = x0; x < _origin.X + Width / _scale; x += 8)
                    {
                        float sx = (x - _origin.X) * _scale;
                        g.DrawLine(pen, sx, 0, sx, Height);
                    }
                    for (int y = y0; y < _origin.Y + Height / _scale; y += 8)
                    {
                        float sy = (y - _origin.Y) * _scale;
                        g.DrawLine(pen, 0, sy, Width, sy);
                    }
                }
            }
            if (_scale >= 24)
            {
                // altitude numbers when zoomed in very far
                using (var f = new Font("Segoe UI", 7f))
                {
                    int x1 = (int)Math.Ceiling(_origin.X + Width / _scale), y1 = (int)Math.Ceiling(_origin.Y + Height / _scale);
                    for (int y = Math.Max(0, (int)_origin.Y); y < Math.Min(_map.Height, y1); y++)
                    {
                        for (int x = Math.Max(0, (int)_origin.X); x < Math.Min(_map.Width, x1); x++)
                        {
                            var r = Rectangle.Round(MapToScreen(new Rectangle(x, y, 1, 1)));
                            TextRenderer.DrawText(g, _map[x, y].ToString(), f, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }
                }
            }
            if (!_selection.IsEmpty)
            {
                RectangleF s = MapToScreen(_selection);
                using (var fill = new SolidBrush(Color.FromArgb(40, 255, 255, 0)))
                using (var pen = new Pen(Color.Yellow) { DashStyle = DashStyle.Dash })
                {
                    g.FillRectangle(fill, s);
                    g.DrawRectangle(pen, s.X, s.Y, s.Width, s.Height);
                }
            }
            if (Tool != HeightTool.Select && Tool != HeightTool.Pick && _hover.X >= 0)
            {
                float r = (BrushRadius + 0.5f) * _scale;
                float cx = (_hover.X + 0.5f - _origin.X) * _scale, cy = (_hover.Y + 0.5f - _origin.Y) * _scale;
                using (var pen = new Pen(Color.White, 1.5f))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawEllipse(pen, cx - r, cy - r, r * 2, r * 2);
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_map != null)
            {
                ZoomStep(e.Delta > 0 ? 1 : -1, e.Location);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (_map == null)
            {
                return;
            }
            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                _panning = true;
                _panStart = e.Location;
                _panOrigin = _origin;
                Cursor = Cursors.SizeAll;
                return;
            }
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            Point t = ScreenToMap(e.Location);
            switch (Tool)
            {
                case HeightTool.Select:
                    _selecting = true;
                    _selStart = t;
                    Selection = Rectangle.Empty;
                    break;
                case HeightTool.Pick:
                    if (_map.Bounds.Contains(t))
                    {
                        ZPicked?.Invoke(this, _map[t.X, t.Y]);
                    }
                    break;
                default:
                    _painting = true;
                    _strokeBounds = Rectangle.Empty;
                    BrushStarted?.Invoke(this, EventArgs.Empty);
                    PaintBrush(t);
                    break;
            }
        }

        private void PaintBrush(Point t)
        {
            int mode = Tool == HeightTool.Raise ? 0 : Tool == HeightTool.Lower ? 1 : Tool == HeightTool.Smooth ? 2 : 3;
            Rectangle r = HeightOps.Brush(_map, t.X, t.Y, BrushRadius, BrushStrength, mode, FlattenZ);
            if (!r.IsEmpty)
            {
                _strokeBounds = _strokeBounds.IsEmpty ? r : Rectangle.Union(_strokeBounds, r);
                InvalidateMap();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_map == null)
            {
                return;
            }
            if (_panning)
            {
                _origin = new PointF(_panOrigin.X - (e.X - _panStart.X) / _scale, _panOrigin.Y - (e.Y - _panStart.Y) / _scale);
                InvalidateMap();
                ViewChanged?.Invoke(this, EventArgs.Empty);
                return;
            }
            Point t = ScreenToMap(e.Location);
            bool moved = t != _hover;
            _hover = _map.Bounds.Contains(t) ? t : new Point(-1, -1);
            if (moved)
            {
                HoverTile?.Invoke(this, _hover);
            }
            if (_selecting)
            {
                var r = Rectangle.FromLTRB(Math.Min(_selStart.X, t.X), Math.Min(_selStart.Y, t.Y), Math.Max(_selStart.X, t.X) + 1, Math.Max(_selStart.Y, t.Y) + 1);
                Selection = r;
            }
            else if (_painting && moved && _hover.X >= 0)
            {
                PaintBrush(t);
            }
            else if (moved && Tool != HeightTool.Select)
            {
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_panning)
            {
                _panning = false;
                Cursor = Cursors.Default;
            }
            _selecting = false;
            if (_painting)
            {
                _painting = false;
                BrushEnded?.Invoke(this, _strokeBounds);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = new Point(-1, -1);
            HoverTile?.Invoke(this, _hover);
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cache?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

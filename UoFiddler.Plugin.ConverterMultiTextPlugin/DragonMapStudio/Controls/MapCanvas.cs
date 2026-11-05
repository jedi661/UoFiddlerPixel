// /***************************************************************************
//  *
//  * Dragon Map Studio – map drawing surface
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
using System.Drawing.Imaging;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls
{
    public enum MapTool
    {
        Brush,
        Eraser,
        Spray,
        Line,
        Rectangle,
        FilledRectangle,
        Ellipse,
        FilledEllipse,
        Fill,
        Picker,
        Raise,
        Lower,
        Select,
        Pan
    }

    public sealed class MapPointEventArgs : EventArgs
    {
        public MapPointEventArgs(int x, int y, int index)
        {
            X = x;
            Y = y;
            Index = index;
        }

        public int X { get; }
        public int Y { get; }

        /// <summary>Palette index under the cursor (-1 outside the map).</summary>
        public int Index { get; }
    }

    /// <summary>
    /// Draws the 8-bit map with zoom and panning (renders only the visible part, so 7168×4096 maps stay fast)
    /// and implements all painting tools. Every change goes through <see cref="MapHistory"/> (undo / redo).
    /// Left mouse = colour 1, right mouse = colour 2, middle mouse or space = pan, wheel = zoom.
    /// </summary>
    [ToolboxItem(true)]
    public class MapCanvas : Control
    {
        private IndexedMap _map;
        private MapHistory _history;
        private readonly int[] _argb = new int[65536];
        private Bitmap _view;
        private bool _viewDirty = true;

        private double _zoom = 1;
        private double _originX;
        private double _originY;

        private MapTool _tool = MapTool.Brush;
        private bool _drawing;
        private MouseButtons _drawButton;
        private Point _start;
        private Point _last;
        private Point _cursor = new Point(-1, -1);
        private bool _panning;
        private Point _panStart;
        private double _panOriginX;
        private double _panOriginY;
        private bool _spaceDown;
        private readonly Random _rnd = new Random();

        private Rectangle _selection = Rectangle.Empty;
        private IndexedMap _pasteBuffer;
        private bool _pasteMode;

        private ushort[] _raise = Identity();
        private ushort[] _lower = Identity();

        public MapCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.FromArgb(30, 30, 34);
            TabStop = true;
        }

        public event EventHandler<MapPointEventArgs> CursorMoved;
        public event EventHandler<PaletteIndexEventArgs> ColorPicked;
        public event EventHandler ViewChanged;
        public event EventHandler MapEdited;
        public event EventHandler SelectionChanged;

        // ------------------------------------------------------------------ properties

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IndexedMap Map => _map;

        [Category("Dragon"), DefaultValue(MapTool.Brush)]
        public MapTool Tool
        {
            get => _tool;
            set
            {
                if (_tool == MapTool.Select && value != MapTool.Select)
                {
                    _pasteMode = false;
                }
                _tool = value;
                Cursor = value == MapTool.Pan ? Cursors.Hand : value == MapTool.Picker ? Cursors.Cross : Cursors.Default;
                Invalidate();
            }
        }

        [Category("Dragon"), DefaultValue(8)]
        public int BrushSize { get; set; } = 8;

        [Category("Dragon"), DefaultValue(false)]
        public bool SquareBrush { get; set; }

        [Category("Dragon"), DefaultValue(0)]
        public int PrimaryIndex { get; set; }

        [Category("Dragon"), DefaultValue(32)]
        public int SecondaryIndex { get; set; } = 32;

        [Category("Dragon"), DefaultValue(true)]
        public bool ShowGrid { get; set; } = true;

        [Category("Dragon"), DefaultValue(6)]
        public int GridFromZoom { get; set; } = 6;

        /// <summary>Show the altitude of every pixel (from zoom 12).</summary>
        [Category("Dragon"), DefaultValue(false)]
        public bool ShowHeights { get; set; }

        private int[] _altitudes = new int[65536];

        /// <summary>Altitude (Z) of every palette index (for the height numbers).</summary>
        public void SetAltitudes(int[] altitudes)
        {
            _altitudes = new int[65536];
            altitudes?.AsSpan(0, Math.Min(altitudes.Length, 65536)).CopyTo(_altitudes);
            Invalidate();
        }

        /// <summary>Spray: dots per step in percent of the brush area.</summary>
        [Category("Dragon"), DefaultValue(8)]
        public int SprayDensity { get; set; } = 8;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double Zoom => _zoom;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle Selection => _selection;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool PasteMode => _pasteMode;

        /// <summary>The visible part of the map in map coordinates.</summary>
        [Browsable(false)]
        public RectangleF ViewRect => new RectangleF((float)_originX, (float)_originY, (float)(Width / _zoom), (float)(Height / _zoom));

        // ------------------------------------------------------------------ setup

        public void SetMap(IndexedMap map, MapHistory history)
        {
            _map = map;
            _history = history;
            _selection = Rectangle.Empty;
            _pasteMode = false;
            ZoomToFit();
            InvalidateMap();
        }

        public void SetPalette(Color[] colors)
        {
            Array.Clear(_argb);
            for (int i = 0; i < colors.Length && i < _argb.Length; i++)
            {
                _argb[i] = colors[i].ToArgb();
            }
            InvalidateMap();
        }

        public void SetHeightLookups(ushort[] raise, ushort[] lower)
        {
            _raise = Identity();
            _lower = Identity();
            raise?.AsSpan(0, Math.Min(raise.Length, 65536)).CopyTo(_raise);
            lower?.AsSpan(0, Math.Min(lower.Length, 65536)).CopyTo(_lower);
        }

        /// <summary>Table "index -> same index" for all 65 536 colour addresses.</summary>
        private static ushort[] Identity()
        {
            var a = new ushort[65536];
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = (ushort)i;
            }
            return a;
        }

        /// <summary>Call after the map was changed from outside (generator, undo …).</summary>
        public void InvalidateMap()
        {
            _viewDirty = true;
            Invalidate();
        }

        // ------------------------------------------------------------------ view

        public void ZoomToFit()
        {
            if (_map == null || Width <= 0 || Height <= 0)
            {
                return;
            }
            _zoom = Math.Min((double)Width / _map.Width, (double)Height / _map.Height);
            _zoom = Math.Max(1.0 / 64, _zoom);
            _originX = (_map.Width - Width / _zoom) / 2;
            _originY = (_map.Height - Height / _zoom) / 2;
            OnViewChanged();
        }

        public void SetZoom(double zoom, Point? anchor = null)
        {
            if (_map == null)
            {
                return;
            }
            Point a = anchor ?? new Point(Width / 2, Height / 2);
            double mx = _originX + a.X / _zoom, my = _originY + a.Y / _zoom;
            _zoom = Math.Clamp(zoom, 1.0 / 64, 64);
            _originX = mx - a.X / _zoom;
            _originY = my - a.Y / _zoom;
            OnViewChanged();
        }

        public void ZoomStep(int direction, Point? anchor = null)
        {
            double[] steps = { 1.0 / 64, 1.0 / 32, 1.0 / 16, 1.0 / 8, 1.0 / 6, 1.0 / 4, 1.0 / 3, 1.0 / 2, 2.0 / 3, 1, 1.5, 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64 };
            int i = 0;
            while (i < steps.Length - 1 && steps[i] < _zoom - 1e-9)
            {
                i++;
            }
            if (direction > 0)
            {
                i = steps[i] > _zoom + 1e-9 ? i : Math.Min(steps.Length - 1, i + 1);
            }
            else
            {
                i = Math.Max(0, i - 1);
            }
            SetZoom(steps[i], anchor);
        }

        /// <summary>Centres the view on a map position.</summary>
        public void CenterOn(double mapX, double mapY)
        {
            _originX = mapX - Width / _zoom / 2;
            _originY = mapY - Height / _zoom / 2;
            OnViewChanged();
        }

        private void OnViewChanged()
        {
            _viewDirty = true;
            Invalidate();
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        public Point ScreenToMap(Point p) => new Point((int)Math.Floor(_originX + p.X / _zoom), (int)Math.Floor(_originY + p.Y / _zoom));

        private PointF MapToScreen(double x, double y) => new PointF((float)((x - _originX) * _zoom), (float)((y - _originY) * _zoom));

        // ------------------------------------------------------------------ painting

        private unsafe void RenderView()
        {
            if (_view == null || _view.Width != Width || _view.Height != Height)
            {
                _view?.Dispose();
                _view = new Bitmap(Math.Max(1, Width), Math.Max(1, Height), PixelFormat.Format32bppRgb);
            }
            int w = _view.Width, h = _view.Height;
            BitmapData bd = _view.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            try
            {
                int bg = BackColor.ToArgb();
                var colMap = new int[w];
                for (int sx = 0; sx < w; sx++)
                {
                    double mx = Math.Floor(_originX + sx / _zoom);
                    colMap[sx] = mx < 0 || mx >= _map.Width ? -1 : (int)mx;
                }
                ushort[] pixels = _map.Pixels;
                int mapW = _map.Width;
                fixed (int* pal = _argb)
                fixed (ushort* src = pixels)
                {
                    for (int sy = 0; sy < h; sy++)
                    {
                        int* row = (int*)((byte*)bd.Scan0 + sy * bd.Stride);
                        double my = Math.Floor(_originY + sy / _zoom);
                        if (my < 0 || my >= _map.Height)
                        {
                            for (int sx = 0; sx < w; sx++)
                            {
                                row[sx] = bg;
                            }
                            continue;
                        }
                        ushort* line = src + (long)my * mapW;
                        for (int sx = 0; sx < w; sx++)
                        {
                            int mx = colMap[sx];
                            row[sx] = mx < 0 ? bg : pal[line[mx]];
                        }
                    }
                }
            }
            finally
            {
                _view.UnlockBits(bd);
            }
            _viewDirty = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (_map == null)
            {
                g.Clear(BackColor);
                TextRenderer.DrawText(g, "Keine Karte geöffnet – Datei → Neue Karte oder Karte öffnen.", Font, new Point(12, 12), Color.Gainsboro);
                return;
            }
            if (_viewDirty)
            {
                RenderView();
            }
            g.DrawImageUnscaled(_view, 0, 0);

            DrawGrid(g);
            DrawHeights(g);
            DrawOverlay(g);
        }

        private void DrawGrid(Graphics g)
        {
            if (!ShowGrid || _zoom < GridFromZoom)
            {
                return;
            }
            RectangleF v = ViewRect;
            int x0 = Math.Max(0, (int)Math.Floor(v.Left)), x1 = Math.Min(_map.Width, (int)Math.Ceiling(v.Right));
            int y0 = Math.Max(0, (int)Math.Floor(v.Top)), y1 = Math.Min(_map.Height, (int)Math.Ceiling(v.Bottom));
            using var block = new Pen(Color.FromArgb(110, 0, 0, 0));
            using var cell = new Pen(Color.FromArgb(35, 0, 0, 0));
            bool cells = _zoom >= 16;
            for (int x = x0; x <= x1; x++)
            {
                if (x % 8 == 0 || cells)
                {
                    float sx = MapToScreen(x, 0).X;
                    g.DrawLine(x % 8 == 0 ? block : cell, sx, MapToScreen(0, y0).Y, sx, MapToScreen(0, y1).Y);
                }
            }
            for (int y = y0; y <= y1; y++)
            {
                if (y % 8 == 0 || cells)
                {
                    float sy = MapToScreen(0, y).Y;
                    g.DrawLine(y % 8 == 0 ? block : cell, MapToScreen(x0, 0).X, sy, MapToScreen(x1, 0).X, sy);
                }
            }
        }

        private void DrawHeights(Graphics g)
        {
            if (!ShowHeights || _zoom < 12)
            {
                return;
            }
            RectangleF v = ViewRect;
            int x0 = Math.Max(0, (int)Math.Floor(v.Left)), x1 = Math.Min(_map.Width - 1, (int)Math.Ceiling(v.Right));
            int y0 = Math.Max(0, (int)Math.Floor(v.Top)), y1 = Math.Min(_map.Height - 1, (int)Math.Ceiling(v.Bottom));
            using var font = new Font("Segoe UI", Math.Min(9f, (float)(_zoom * 0.32)), FontStyle.Bold);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    int z = _altitudes[_map[x, y]];
                    PointF c = MapToScreen(x + 0.5, y + 0.5);
                    string t = z.ToString();
                    g.DrawString(t, font, Brushes.Black, c.X + 1, c.Y + 1, sf);
                    g.DrawString(t, font, Brushes.White, c.X, c.Y, sf);
                }
            }
        }

        private void DrawOverlay(Graphics g)
        {
            // selection
            if (!_selection.IsEmpty)
            {
                PointF a = MapToScreen(_selection.Left, _selection.Top);
                PointF b = MapToScreen(_selection.Right, _selection.Bottom);
                using var pen = new Pen(Color.White) { DashStyle = DashStyle.Dash };
                g.DrawRectangle(Pens.Black, a.X, a.Y, b.X - a.X, b.Y - a.Y);
                g.DrawRectangle(pen, a.X, a.Y, b.X - a.X, b.Y - a.Y);
            }

            if (_cursor.X == -1 && _cursor.Y == -1)
            {
                return;
            }

            // paste preview
            if (_pasteMode && _pasteBuffer != null)
            {
                PointF a = MapToScreen(_cursor.X, _cursor.Y);
                using Bitmap prev = IndexedBitmapIO.ToBitmap(_pasteBuffer, PaletteColors(), 512);
                var attr = new ImageAttributes();
                attr.SetColorMatrix(new ColorMatrix { Matrix33 = 0.7f });
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                var dest = new Rectangle((int)a.X, (int)a.Y, (int)(_pasteBuffer.Width * _zoom), (int)(_pasteBuffer.Height * _zoom));
                g.DrawImage(prev, dest, 0, 0, prev.Width, prev.Height, GraphicsUnit.Pixel, attr);
                g.DrawRectangle(Pens.Yellow, dest);
                return;
            }

            // shape preview
            if (_drawing && IsShapeTool(_tool))
            {
                using var pen = new Pen(Color.Yellow) { DashStyle = DashStyle.Dot };
                Rectangle r = NormRect(_start, _cursor);
                PointF a = MapToScreen(r.Left, r.Top);
                PointF b = MapToScreen(r.Right + 1, r.Bottom + 1);
                switch (_tool)
                {
                    case MapTool.Line:
                        PointF s = MapToScreen(_start.X + 0.5, _start.Y + 0.5), t = MapToScreen(_cursor.X + 0.5, _cursor.Y + 0.5);
                        g.DrawLine(pen, s, t);
                        break;
                    case MapTool.Ellipse:
                    case MapTool.FilledEllipse:
                        g.DrawEllipse(pen, a.X, a.Y, b.X - a.X, b.Y - a.Y);
                        break;
                    default:
                        g.DrawRectangle(pen, a.X, a.Y, b.X - a.X, b.Y - a.Y);
                        break;
                }
            }
            if (_drawing && _tool == MapTool.Select)
            {
                Rectangle r = NormRect(_start, _cursor);
                PointF a = MapToScreen(r.Left, r.Top);
                PointF b = MapToScreen(r.Right + 1, r.Bottom + 1);
                using var pen = new Pen(Color.Cyan) { DashStyle = DashStyle.Dash };
                g.DrawRectangle(pen, a.X, a.Y, b.X - a.X, b.Y - a.Y);
            }

            // brush outline
            if (UsesBrush(_tool))
            {
                int size = Math.Max(1, BrushSize);
                int half = size / 2;
                PointF a = MapToScreen(_cursor.X - half, _cursor.Y - half);
                float d = (float)(size * _zoom);
                using var pen = new Pen(Color.FromArgb(220, 255, 255, 255));
                if (SquareBrush || size <= 2)
                {
                    g.DrawRectangle(Pens.Black, a.X - 1, a.Y - 1, d + 2, d + 2);
                    g.DrawRectangle(pen, a.X, a.Y, d, d);
                }
                else
                {
                    g.DrawEllipse(Pens.Black, a.X - 1, a.Y - 1, d + 2, d + 2);
                    g.DrawEllipse(pen, a.X, a.Y, d, d);
                }
            }
        }

        private Color[] PaletteColors()
        {
            var c = new Color[65536];
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = Color.FromArgb(_argb[i]);
            }
            return c;
        }

        private static bool UsesBrush(MapTool t) => t == MapTool.Brush || t == MapTool.Eraser || t == MapTool.Spray || t == MapTool.Raise || t == MapTool.Lower;

        private static bool IsShapeTool(MapTool t) =>
            t == MapTool.Line || t == MapTool.Rectangle || t == MapTool.FilledRectangle || t == MapTool.Ellipse || t == MapTool.FilledEllipse;

        private static Rectangle NormRect(Point a, Point b) =>
            Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));

        // ------------------------------------------------------------------ mouse

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            ZoomStep(e.Delta > 0 ? 1 : -1, e.Location);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (_map == null)
            {
                return;
            }
            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && (_spaceDown || _tool == MapTool.Pan)))
            {
                _panning = true;
                _panStart = e.Location;
                _panOriginX = _originX;
                _panOriginY = _originY;
                Cursor = Cursors.SizeAll;
                return;
            }
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right)
            {
                return;
            }

            Point p = ScreenToMap(e.Location);
            ushort color = (ushort)(e.Button == MouseButtons.Left ? PrimaryIndex : SecondaryIndex);

            if (_pasteMode && _pasteBuffer != null)
            {
                if (e.Button == MouseButtons.Right)
                {
                    _pasteMode = false;
                    Invalidate();
                    return;
                }
                PasteAt(p);
                return;
            }

            switch (_tool)
            {
                case MapTool.Picker:
                    if (_map.InBounds(p.X, p.Y))
                    {
                        ColorPicked?.Invoke(this, new PaletteIndexEventArgs(_map[p.X, p.Y], e.Button));
                    }
                    return;
                case MapTool.Fill:
                    if (_map.InBounds(p.X, p.Y))
                    {
                        _history?.Begin("Füllen");
                        FloodFill(p.X, p.Y, color);
                        EndEdit();
                    }
                    return;
            }

            _drawing = true;
            _drawButton = e.Button;
            _start = p;
            _last = p;
            _cursor = p;
            if (_tool == MapTool.Select)
            {
                return;
            }
            if (!IsShapeTool(_tool))
            {
                _history?.Begin(ToolName(_tool));
                Stamp(p.X, p.Y, color, e.Button);
            }
            Invalidate();
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
                _originX = _panOriginX - (e.X - _panStart.X) / _zoom;
                _originY = _panOriginY - (e.Y - _panStart.Y) / _zoom;
                OnViewChanged();
                return;
            }
            Point p = ScreenToMap(e.Location);
            bool moved = p != _cursor;
            _cursor = p;
            if (moved)
            {
                CursorMoved?.Invoke(this, new MapPointEventArgs(p.X, p.Y, _map.InBounds(p.X, p.Y) ? _map[p.X, p.Y] : -1));
            }

            if (_drawing && !IsShapeTool(_tool) && _tool != MapTool.Select)
            {
                ushort color = (ushort)(_drawButton == MouseButtons.Left ? PrimaryIndex : SecondaryIndex);
                StrokeLine(_last, p, color, _drawButton);
                _last = p;
            }
            if (moved)
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
                Tool = _tool;
                return;
            }
            if (!_drawing || _map == null)
            {
                return;
            }
            _drawing = false;
            Point p = ScreenToMap(e.Location);
            ushort color = (ushort)(_drawButton == MouseButtons.Left ? PrimaryIndex : SecondaryIndex);

            if (_tool == MapTool.Select)
            {
                Rectangle r = NormRect(_start, p);
                r = new Rectangle(r.X, r.Y, r.Width + 1, r.Height + 1);
                r.Intersect(_map.Bounds);
                _selection = r.Width > 1 || r.Height > 1 ? r : Rectangle.Empty;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
                return;
            }
            if (IsShapeTool(_tool))
            {
                _history?.Begin(ToolName(_tool));
                DrawShape(_start, p, color);
            }
            EndEdit();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (!_drawing)
            {
                _cursor = new Point(-1, -1);
                Invalidate();
            }
        }

        // ------------------------------------------------------------------ keyboard

        protected override bool IsInputKey(Keys keyData)
        {
            Keys k = keyData & Keys.KeyCode;
            return k == Keys.Left || k == Keys.Right || k == Keys.Up || k == Keys.Down || k == Keys.Space || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            double step = 64 / _zoom;
            switch (e.KeyCode)
            {
                case Keys.Space:
                    _spaceDown = true;
                    Cursor = Cursors.Hand;
                    break;
                case Keys.Left:
                    _originX -= step;
                    OnViewChanged();
                    break;
                case Keys.Right:
                    _originX += step;
                    OnViewChanged();
                    break;
                case Keys.Up:
                    _originY -= step;
                    OnViewChanged();
                    break;
                case Keys.Down:
                    _originY += step;
                    OnViewChanged();
                    break;
                case Keys.Escape:
                    _pasteMode = false;
                    _selection = Rectangle.Empty;
                    if (_drawing)
                    {
                        _drawing = false;
                        _history?.Cancel();
                        InvalidateMap();
                    }
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode == Keys.Space)
            {
                _spaceDown = false;
                Tool = _tool;
            }
        }

        // ------------------------------------------------------------------ editing

        private static string ToolName(MapTool t)
        {
            switch (t)
            {
                case MapTool.Brush: return "Pinsel";
                case MapTool.Eraser: return "Radierer";
                case MapTool.Spray: return "Sprühen";
                case MapTool.Line: return "Linie";
                case MapTool.Rectangle: return "Rechteck";
                case MapTool.FilledRectangle: return "Rechteck gefüllt";
                case MapTool.Ellipse: return "Ellipse";
                case MapTool.FilledEllipse: return "Ellipse gefüllt";
                case MapTool.Raise: return "Höhe +";
                case MapTool.Lower: return "Höhe −";
                default: return t.ToString();
            }
        }

        private void EndEdit()
        {
            _history?.End();
            MapEdited?.Invoke(this, EventArgs.Empty);
        }

        private void Changed(Rectangle r)
        {
            // fast path: only the changed screen area is re-rendered on the next paint
            _viewDirty = true;
            PointF a = MapToScreen(r.Left, r.Top);
            PointF b = MapToScreen(r.Right, r.Bottom);
            Invalidate(Rectangle.FromLTRB((int)a.X - 2, (int)a.Y - 2, (int)Math.Ceiling(b.X) + 2, (int)Math.Ceiling(b.Y) + 2));
        }

        private void StrokeLine(Point a, Point b, ushort color, MouseButtons button)
        {
            int dx = Math.Abs(b.X - a.X), dy = Math.Abs(b.Y - a.Y);
            int steps = Math.Max(dx, dy);
            int spacing = _tool == MapTool.Spray ? Math.Max(1, BrushSize / 3) : Math.Max(1, BrushSize / 4);
            if (steps == 0)
            {
                return;
            }
            for (int s = spacing; s <= steps; s += spacing)
            {
                int x = a.X + (b.X - a.X) * s / steps, y = a.Y + (b.Y - a.Y) * s / steps;
                Stamp(x, y, color, button);
            }
            if (steps % spacing != 0)
            {
                Stamp(b.X, b.Y, color, button);
            }
        }

        /// <summary>One brush stamp at (cx, cy).</summary>
        private void Stamp(int cx, int cy, ushort color, MouseButtons button)
        {
            int size = Math.Max(1, BrushSize);
            int half = size / 2;
            var r = new Rectangle(cx - half, cy - half, size, size);
            r.Intersect(_map.Bounds);
            if (r.Width <= 0 || r.Height <= 0)
            {
                return;
            }
            _history?.Touch(r);
            bool round = !SquareBrush && size > 2;
            double radius = size / 2.0;
            double ccx = cx - half + radius, ccy = cy - half + radius;
            ushort[] heightTable = _tool == MapTool.Raise ? (button == MouseButtons.Right ? _lower : _raise)
                : _tool == MapTool.Lower ? (button == MouseButtons.Right ? _raise : _lower) : null;
            ushort value = _tool == MapTool.Eraser ? (ushort)SecondaryIndex : color;
            int sprayChance = Math.Clamp(SprayDensity, 1, 100);

            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    if (round)
                    {
                        double ddx = x + 0.5 - ccx, ddy = y + 0.5 - ccy;
                        if (ddx * ddx + ddy * ddy > radius * radius)
                        {
                            continue;
                        }
                    }
                    if (_tool == MapTool.Spray && _rnd.Next(100) >= sprayChance)
                    {
                        continue;
                    }
                    int i = y * _map.Width + x;
                    _map.Pixels[i] = heightTable != null ? heightTable[_map.Pixels[i]] : value;
                }
            }
            Changed(r);
        }

        private void SetPixel(int x, int y, ushort color)
        {
            if (_map.InBounds(x, y))
            {
                _map.Pixels[y * _map.Width + x] = color;
            }
        }

        private void DrawShape(Point a, Point b, ushort color)
        {
            Rectangle r = NormRect(a, b);
            var touch = new Rectangle(r.X - BrushSize, r.Y - BrushSize, r.Width + 2 * BrushSize + 1, r.Height + 2 * BrushSize + 1);
            _history?.Touch(touch);
            int thick = Math.Max(1, _tool == MapTool.Line ? BrushSize : Math.Min(BrushSize, 64));

            switch (_tool)
            {
                case MapTool.Line:
                    {
                        var saveTool = _tool;
                        _tool = MapTool.Brush;
                        Stamp(a.X, a.Y, color, MouseButtons.Left);
                        StrokeLine(a, b, color, MouseButtons.Left);
                        _tool = saveTool;
                        break;
                    }
                case MapTool.FilledRectangle:
                    for (int y = r.Top; y <= r.Bottom; y++)
                    {
                        for (int x = r.Left; x <= r.Right; x++)
                        {
                            SetPixel(x, y, color);
                        }
                    }
                    break;
                case MapTool.Rectangle:
                    for (int y = r.Top; y <= r.Bottom; y++)
                    {
                        for (int x = r.Left; x <= r.Right; x++)
                        {
                            if (x - r.Left < thick || r.Right - x < thick || y - r.Top < thick || r.Bottom - y < thick)
                            {
                                SetPixel(x, y, color);
                            }
                        }
                    }
                    break;
                case MapTool.Ellipse:
                case MapTool.FilledEllipse:
                    {
                        double rx = (r.Width + 1) / 2.0, ry = (r.Height + 1) / 2.0;
                        double cx = r.Left + rx, cy = r.Top + ry;
                        double irx = Math.Max(0, rx - thick), iry = Math.Max(0, ry - thick);
                        for (int y = r.Top; y <= r.Bottom; y++)
                        {
                            for (int x = r.Left; x <= r.Right; x++)
                            {
                                double nx = (x + 0.5 - cx) / rx, ny = (y + 0.5 - cy) / ry;
                                if (nx * nx + ny * ny > 1)
                                {
                                    continue;
                                }
                                if (_tool == MapTool.Ellipse && irx > 0 && iry > 0)
                                {
                                    double ix = (x + 0.5 - cx) / irx, iy = (y + 0.5 - cy) / iry;
                                    if (ix * ix + iy * iy < 1)
                                    {
                                        continue;
                                    }
                                }
                                SetPixel(x, y, color);
                            }
                        }
                        break;
                    }
            }
            Changed(touch);
        }

        /// <summary>Scanline flood fill (4 neighbours) of the area with the same palette index.</summary>
        private void FloodFill(int sx, int sy, ushort color)
        {
            ushort target = _map[sx, sy];
            if (target == color)
            {
                return;
            }
            int w = _map.Width, h = _map.Height;
            ushort[] px = _map.Pixels;
            var spans = new List<(int y, int x0, int x1)>();
            var visited = new System.Collections.BitArray(w * h);
            var stack = new Stack<Point>();
            stack.Push(new Point(sx, sy));
            int minX = sx, maxX = sx, minY = sy, maxY = sy;
            while (stack.Count > 0)
            {
                Point p = stack.Pop();
                int y = p.Y;
                int x = p.X;
                if (visited[y * w + x] || px[y * w + x] != target)
                {
                    continue;
                }
                int x0 = x, x1 = x;
                while (x0 > 0 && px[y * w + x0 - 1] == target && !visited[y * w + x0 - 1])
                {
                    x0--;
                }
                while (x1 < w - 1 && px[y * w + x1 + 1] == target && !visited[y * w + x1 + 1])
                {
                    x1++;
                }
                for (int k = x0; k <= x1; k++)
                {
                    visited[y * w + k] = true;
                }
                spans.Add((y, x0, x1));
                minX = Math.Min(minX, x0);
                maxX = Math.Max(maxX, x1);
                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);
                for (int ny = y - 1; ny <= y + 1; ny += 2)
                {
                    if (ny < 0 || ny >= h)
                    {
                        continue;
                    }
                    bool inRun = false;
                    for (int k = x0; k <= x1; k++)
                    {
                        bool ok = px[ny * w + k] == target && !visited[ny * w + k];
                        if (ok && !inRun)
                        {
                            stack.Push(new Point(k, ny));
                        }
                        inRun = ok;
                    }
                }
            }
            Rectangle bounds = Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
            _history?.Touch(bounds);
            foreach ((int y, int x0, int x1) in spans)
            {
                for (int k = x0; k <= x1; k++)
                {
                    px[y * w + k] = color;
                }
            }
            Changed(bounds);
        }

        // ------------------------------------------------------------------ selection / clipboard

        public void SelectAll()
        {
            if (_map == null)
            {
                return;
            }
            _selection = _map.Bounds;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        public IndexedMap CopySelection() => _map == null || _selection.IsEmpty ? null : _map.Crop(_selection);

        /// <summary>Fills the selection (Delete = colour 2).</summary>
        public void FillSelection(ushort color, string name)
        {
            if (_map == null || _selection.IsEmpty)
            {
                return;
            }
            _history?.Begin(name);
            _history?.Touch(_selection);
            for (int y = _selection.Top; y < _selection.Bottom; y++)
            {
                for (int x = _selection.Left; x < _selection.Right; x++)
                {
                    _map.Pixels[y * _map.Width + x] = color;
                }
            }
            Changed(_selection);
            EndEdit();
        }

        /// <summary>Starts paste mode: the buffer follows the mouse, a left click places it (several times), Esc / right click ends.</summary>
        public void BeginPaste(IndexedMap buffer)
        {
            if (buffer == null || _map == null)
            {
                return;
            }
            _pasteBuffer = buffer;
            _pasteMode = true;
            Invalidate();
        }

        private void PasteAt(Point p)
        {
            var r = new Rectangle(p.X, p.Y, _pasteBuffer.Width, _pasteBuffer.Height);
            r.Intersect(_map.Bounds);
            if (r.Width <= 0 || r.Height <= 0)
            {
                return;
            }
            _history?.Begin("Einfügen");
            _history?.Touch(r);
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    _map.Pixels[y * _map.Width + x] = _pasteBuffer.Pixels[(y - p.Y) * _pasteBuffer.Width + (x - p.X)];
                }
            }
            Changed(r);
            EndEdit();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _viewDirty = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _view?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

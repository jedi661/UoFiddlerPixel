// /***************************************************************************
//  *
//  * $Author: Turley
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit
{
    internal sealed class TileMouseEventArgs : EventArgs
    {
        public int X { get; }
        public int Y { get; }
        public MouseButtons Button { get; }
        public Keys Modifiers { get; }

        public TileMouseEventArgs(int x, int y, MouseButtons button, Keys modifiers)
        {
            X = x;
            Y = y;
            Button = button;
            Modifiers = modifiers;
        }
    }

    /// <summary>
    /// Zoomable, pannable view of a <see cref="MapDocument"/>.
    /// Only the visible part is rendered, so it is fast on 7168x4096 maps as well.
    /// Mouse wheel = zoom, middle or right mouse button = pan (right only when <see cref="RightButtonPans"/>).
    /// </summary>
    internal sealed class MapCanvas : Control
    {
        private MapDocument _document;
        private Bitmap _buffer;
        private double _zoom = 1;
        private double _viewX;
        private double _viewY;
        private bool _panning;
        private Point _panStart;
        private double _panViewX;
        private double _panViewY;
        private bool _toolActive;
        private Point _mouseTile = new Point(-1, -1);
        private bool _dirty = true;

        public MapCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.FromArgb(32, 32, 36);
            TabStop = true;
        }

        public event EventHandler<TileMouseEventArgs> ToolDown;
        public event EventHandler<TileMouseEventArgs> ToolMove;
        public event EventHandler<TileMouseEventArgs> ToolUp;
        public event EventHandler<TileMouseEventArgs> TileHover;
        public event EventHandler ViewChanged;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TerrainPalette TerrainPalette { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AltitudePalette AltitudePalette { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MapDocument Document
        {
            get => _document;
            set
            {
                _document = value;
                ZoomToFit();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MapDocument.RenderMode Mode { get; set; } = MapDocument.RenderMode.Terrain;

        [DefaultValue(true)]
        public bool ShowGrid { get; set; } = true;

        [DefaultValue(true)]
        public bool RightButtonPans { get; set; } = true;

        /// <summary>Brush radius in tiles for the cursor outline (0 = 1 tile).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BrushRadius { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SquareBrush { get; set; }

        /// <summary>Selection rectangle in tiles (drawn as outline), empty = none.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle Selection { get; set; } = Rectangle.Empty;

        [Browsable(false)]
        public double Zoom => _zoom;

        [Browsable(false)]
        public Point MouseTile => _mouseTile;

        /// <summary>Call after the document content changed.</summary>
        public void RefreshMap()
        {
            _dirty = true;
            Invalidate();
        }

        public void ZoomToFit()
        {
            if (_document == null || ClientSize.Width <= 0 || ClientSize.Height <= 0)
            {
                _zoom = 1;
                _viewX = 0;
                _viewY = 0;
            }
            else
            {
                _zoom = Math.Min((double)ClientSize.Width / _document.Width, (double)ClientSize.Height / _document.Height);
                _zoom = ClampZoom(_zoom);
                _viewX = -((ClientSize.Width / _zoom) - _document.Width) / 2;
                _viewY = -((ClientSize.Height / _zoom) - _document.Height) / 2;
            }

            RefreshMap();
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetZoom(double zoom, Point? anchor = null)
        {
            Point a = anchor ?? new Point(ClientSize.Width / 2, ClientSize.Height / 2);
            double docX = _viewX + (a.X / _zoom);
            double docY = _viewY + (a.Y / _zoom);
            _zoom = ClampZoom(zoom);
            _viewX = docX - (a.X / _zoom);
            _viewY = docY - (a.Y / _zoom);
            RefreshMap();
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        public void CenterOn(int x, int y)
        {
            _viewX = x - (ClientSize.Width / _zoom / 2);
            _viewY = y - (ClientSize.Height / _zoom / 2);
            RefreshMap();
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        private static double ClampZoom(double zoom) => Math.Clamp(zoom, 1.0 / 32, 48);

        public Point ScreenToTile(Point p) => new Point((int)Math.Floor(_viewX + (p.X / _zoom)), (int)Math.Floor(_viewY + (p.Y / _zoom)));

        public PointF TileToScreen(double x, double y) => new PointF((float)((x - _viewX) * _zoom), (float)((y - _viewY) * _zoom));

        // ---------------------------------------------------------------- painting

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (_document == null || TerrainPalette == null || AltitudePalette == null)
            {
                g.Clear(BackColor);
                TextRenderer.DrawText(g, "No map loaded.", Font, ClientRectangle, Color.Gainsboro, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            if (_dirty || _buffer == null || _buffer.Width != ClientSize.Width || _buffer.Height != ClientSize.Height)
            {
                RenderBuffer();
                _dirty = false;
            }

            if (_buffer != null)
            {
                g.DrawImageUnscaled(_buffer, 0, 0);
            }

            g.SmoothingMode = SmoothingMode.None;
            if (ShowGrid && _zoom >= 3)
            {
                DrawGrid(g);
            }

            if (!Selection.IsEmpty)
            {
                PointF a = TileToScreen(Selection.Left, Selection.Top);
                PointF b = TileToScreen(Selection.Right, Selection.Bottom);
                using (Pen pen = new Pen(Color.Yellow, 1) { DashStyle = DashStyle.Dash })
                {
                    g.DrawRectangle(pen, a.X, a.Y, b.X - a.X, b.Y - a.Y);
                }
            }

            if (_mouseTile.X >= 0)
            {
                int r = Math.Max(0, BrushRadius);
                PointF a = TileToScreen(_mouseTile.X - r, _mouseTile.Y - r);
                float size = (float)(((2 * r) + 1) * _zoom);
                using (Pen pen = new Pen(Color.White, 1))
                {
                    if (SquareBrush || r == 0)
                    {
                        g.DrawRectangle(pen, a.X, a.Y, Math.Max(2, size), Math.Max(2, size));
                    }
                    else
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.DrawEllipse(pen, a.X, a.Y, Math.Max(2, size), Math.Max(2, size));
                    }
                }
            }
        }

        private void DrawGrid(Graphics g)
        {
            int x0 = (int)Math.Floor(_viewX / 8) * 8;
            int y0 = (int)Math.Floor(_viewY / 8) * 8;
            double x1 = _viewX + (ClientSize.Width / _zoom);
            double y1 = _viewY + (ClientSize.Height / _zoom);
            using (Pen pen = new Pen(Color.FromArgb(70, 255, 255, 255)))
            {
                for (int x = Math.Max(0, x0); x <= Math.Min(_document.Width, x1); x += 8)
                {
                    float sx = TileToScreen(x, 0).X;
                    g.DrawLine(pen, sx, 0, sx, ClientSize.Height);
                }

                for (int y = Math.Max(0, y0); y <= Math.Min(_document.Height, y1); y += 8)
                {
                    float sy = TileToScreen(0, y).Y;
                    g.DrawLine(pen, 0, sy, ClientSize.Width, sy);
                }
            }
        }

        private unsafe void RenderBuffer()
        {
            int w = ClientSize.Width;
            int h = ClientSize.Height;
            if (w <= 0 || h <= 0)
            {
                return;
            }

            if (_buffer == null || _buffer.Width != w || _buffer.Height != h)
            {
                _buffer?.Dispose();
                _buffer = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            }

            MapDocument doc = _document;
            int[] terrainColors = TerrainPalette.Argb;
            int[] altColors = AltitudePalette.Argb;
            short[] altValues = AltitudePalette.Altitude;
            int background = BackColor.ToArgb();
            int[] columns = new int[w];
            for (int sx = 0; sx < w; sx++)
            {
                int x = (int)Math.Floor(_viewX + (sx / _zoom));
                columns[sx] = x >= 0 && x < doc.Width ? x : -1;
            }

            BitmapData data = _buffer.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                byte* scan0 = (byte*)data.Scan0;
                int stride = data.Stride;
                MapDocument.RenderMode mode = Mode;
                for (int sy = 0; sy < h; sy++)
                {
                    int* row = (int*)(scan0 + (sy * stride));
                    int y = (int)Math.Floor(_viewY + (sy / _zoom));
                    if (y < 0 || y >= doc.Height)
                    {
                        for (int sx = 0; sx < w; sx++)
                        {
                            row[sx] = background;
                        }

                        continue;
                    }

                    int rowStart = y * doc.Width;
                    for (int sx = 0; sx < w; sx++)
                    {
                        int x = columns[sx];
                        if (x < 0)
                        {
                            row[sx] = background;
                            continue;
                        }

                        int i = rowStart + x;
                        switch (mode)
                        {
                            case MapDocument.RenderMode.Altitude:
                                row[sx] = altColors[doc.Altitude[i]];
                                break;
                            case MapDocument.RenderMode.Relief:
                                {
                                    int here = altValues[doc.Altitude[i]];
                                    int nx = x > 0 ? altValues[doc.Altitude[i - 1]] : here;
                                    int ny = y > 0 ? altValues[doc.Altitude[i - doc.Width]] : here;
                                    row[sx] = MapDocument.Shade(terrainColors[doc.Terrain[i]], ((here - nx) + (here - ny)) * 6);
                                    break;
                                }

                            default:
                                row[sx] = terrainColors[doc.Terrain[i]];
                                break;
                        }
                    }
                }
            }
            finally
            {
                _buffer.UnlockBits(data);
            }
        }

        // ---------------------------------------------------------------- input

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _dirty = true;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            double factor = e.Delta > 0 ? 1.25 : 0.8;
            SetZoom(_zoom * factor, e.Location);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (_document == null)
            {
                return;
            }

            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Right && RightButtonPans))
            {
                _panning = true;
                _panStart = e.Location;
                _panViewX = _viewX;
                _panViewY = _viewY;
                Cursor = Cursors.SizeAll;
                return;
            }

            Point tile = ScreenToTile(e.Location);
            _toolActive = true;
            Capture = true;
            ToolDown?.Invoke(this, new TileMouseEventArgs(tile.X, tile.Y, e.Button, ModifierKeys));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_document == null)
            {
                return;
            }

            if (_panning)
            {
                _viewX = _panViewX - ((e.X - _panStart.X) / _zoom);
                _viewY = _panViewY - ((e.Y - _panStart.Y) / _zoom);
                RefreshMap();
                ViewChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            Point tile = ScreenToTile(e.Location);
            if (tile != _mouseTile)
            {
                _mouseTile = tile;
                TileHover?.Invoke(this, new TileMouseEventArgs(tile.X, tile.Y, e.Button, ModifierKeys));
                if (_toolActive)
                {
                    ToolMove?.Invoke(this, new TileMouseEventArgs(tile.X, tile.Y, e.Button, ModifierKeys));
                }

                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_panning)
            {
                _panning = false;
                Cursor = Cursors.Cross;
                return;
            }

            if (_toolActive)
            {
                _toolActive = false;
                Capture = false;
                Point tile = ScreenToTile(e.Location);
                ToolUp?.Invoke(this, new TileMouseEventArgs(tile.X, tile.Y, e.Button, ModifierKeys));
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _mouseTile = new Point(-1, -1);
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
                    return true;
                default:
                    return base.IsInputKey(keyData);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            double stepTiles = Math.Max(1, 64 / _zoom);
            switch (e.KeyCode)
            {
                case Keys.Left:
                    _viewX -= stepTiles;
                    break;
                case Keys.Right:
                    _viewX += stepTiles;
                    break;
                case Keys.Up:
                    _viewY -= stepTiles;
                    break;
                case Keys.Down:
                    _viewY += stepTiles;
                    break;
                case Keys.Add:
                case Keys.Oemplus:
                    SetZoom(_zoom * 1.25);
                    return;
                case Keys.Subtract:
                case Keys.OemMinus:
                    SetZoom(_zoom * 0.8);
                    return;
                default:
                    return;
            }

            RefreshMap();
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Cursor = Cursors.Cross;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _buffer?.Dispose();
                _buffer = null;
            }

            base.Dispose(disposing);
        }
    }
}

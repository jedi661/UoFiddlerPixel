/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;

namespace UoFiddler.Controls.UserControls
{
    public enum PixelGridMode
    {
        None,
        Lines,
        Dots,
        LinesAndDots,

        /// <summary>UO field: main tile (44x44 diamond, bottom corner = fix point) + 5x5 neighbours, centre and ground line.</summary>
        UoField
    }

    /// <summary>
    /// Zoomed, pixel exact view of an <see cref="ArtPixelImage"/> (nearest neighbour, no smoothing) with an optional
    /// ghost of the original, a grid (lines / dots / UO field) and draggable edit points. A margin around the frame
    /// shows the neighbour tiles; Ctrl + mouse wheel zooms.
    /// </summary>
    public sealed class PixelCanvas : Control
    {
        private Bitmap _image;
        private Bitmap _ghost;
        private ArtPixelImage _pixels;
        private int _zoom = 4;
        private int _drag = -1;

        public PixelCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.Black;
        }

        public int Zoom
        {
            get => _zoom;
            set
            {
                _zoom = Math.Clamp(value, 1, 32);
                UpdateSize();
                Invalidate();
            }
        }

        public PixelGridMode GridMode { get; set; } = PixelGridMode.None;

        /// <summary>Space around the frame in image pixels (to see the neighbour tiles of the UO field).</summary>
        public int ViewMargin
        {
            get => _margin;
            set
            {
                _margin = Math.Max(0, value);
                UpdateSize();
                Invalidate();
            }
        }

        private int _margin;
        private int Off => _margin * _zoom;

        /// <summary>Ctrl + mouse wheel: +1 / -1.</summary>
        public event Action<int> ZoomRequested;

        /// <summary>Also draw the frame of the graphic (its full size defines the position in the field).</summary>
        public bool ShowFrame { get; set; } = true;
        public int GridSize { get; set; } = 8;
        public Color GridColor { get; set; } = Color.FromArgb(150, 0, 200, 255);
        public bool ShowGhost { get; set; }

        /// <summary>Edit points in image coordinates (pixel edges).</summary>
        public List<PointF> Handles { get; } = new List<PointF>();

        /// <summary>Lines between edit points (pairs of indices) for the outline of distort / warp.</summary>
        public List<(int A, int B)> HandleLinks { get; } = new List<(int A, int B)>();

        /// <summary>Raised while a point is dragged: index, new position (image pixels, rounded).</summary>
        public event Action<int, int, int> HandleMoved;

        /// <summary>Raised when the mouse button is released after dragging.</summary>
        public event Action HandleDragEnded;

        /// <summary>Mouse over the image: x, y (or -1 outside) and the pixel colour (0 = transparent).</summary>
        public event Action<int, int, ushort> PixelHover;

        public ArtPixelImage Pixels => _pixels;

        public void SetImage(ArtPixelImage image, ArtPixelImage ghost)
        {
            _pixels = image;
            ReplaceBitmap(ref _image, image == null ? null : ArtDirectionForm.ToBitmap(image));
            ReplaceBitmap(ref _ghost, ghost == null ? null : ArtDirectionForm.ToBitmap(ghost));
            UpdateSize();
            Invalidate();
        }

        private static void ReplaceBitmap(ref Bitmap field, Bitmap value)
        {
            Bitmap old = field;
            field = value;
            old?.Dispose();
        }

        private void UpdateSize()
        {
            if (_pixels != null)
            {
                Size = new Size(((_pixels.Width + (2 * _margin)) * _zoom) + 1, ((_pixels.Height + (2 * _margin)) * _zoom) + 1);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (_pixels == null)
            {
                return;
            }

            g.TranslateTransform(Off, Off);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            var dest = new Rectangle(0, 0, _pixels.Width * _zoom, _pixels.Height * _zoom);

            if (ShowGhost && _ghost != null)
            {
                using var attributes = new ImageAttributes();
                attributes.SetColorMatrix(new ColorMatrix { Matrix33 = 0.3f });
                g.DrawImage(_ghost, dest, 0, 0, _ghost.Width, _ghost.Height, GraphicsUnit.Pixel, attributes);
            }

            if (_image != null)
            {
                g.DrawImage(_image, dest);
            }

            g.PixelOffsetMode = PixelOffsetMode.Default;
            if (ShowFrame)
            {
                // the whole frame - not only the coloured pixels - defines where the graphic sits in the field
                using var framePen = new Pen(Color.FromArgb(110, 128, 128, 128)) { DashStyle = DashStyle.Dot };
                g.DrawRectangle(framePen, 0, 0, (_pixels.Width * _zoom) - 1, (_pixels.Height * _zoom) - 1);
            }

            DrawGrid(g);
            DrawHandles(g);
        }

        private void DrawGrid(Graphics g)
        {
            if (GridMode == PixelGridMode.None || GridSize < 1)
            {
                return;
            }

            int w = _pixels.Width, h = _pixels.Height;
            using var pen = new Pen(GridColor);
            using var dot = new SolidBrush(Color.FromArgb(Math.Min(255, GridColor.A + 80), GridColor));
            float r = Math.Max(1.5f, _zoom / 3f);

            if (GridMode == PixelGridMode.UoField)
            {
                DrawUoField(g, w, h);
                return;
            }

            bool lines = GridMode == PixelGridMode.Lines || GridMode == PixelGridMode.LinesAndDots;
            bool dots = GridMode == PixelGridMode.Dots || GridMode == PixelGridMode.LinesAndDots;

            // aligned to the bottom centre (the UO anchor), so the grid stays the same when the canvas changes
            int startX = ((w / 2) % GridSize) - GridSize, startY = (h % GridSize) - GridSize;
            if (lines)
            {
                for (int x = startX; x <= w; x += GridSize)
                {
                    if (x >= 0)
                    {
                        g.DrawLine(pen, x * _zoom, 0, x * _zoom, h * _zoom);
                    }
                }

                for (int y = startY; y <= h; y += GridSize)
                {
                    if (y >= 0)
                    {
                        g.DrawLine(pen, 0, y * _zoom, w * _zoom, y * _zoom);
                    }
                }
            }

            if (dots)
            {
                for (int x = startX; x <= w; x += GridSize)
                {
                    for (int y = startY; y <= h; y += GridSize)
                    {
                        if (x >= 0 && y >= 0)
                        {
                            g.FillEllipse(dot, (x * _zoom) - r, (y * _zoom) - r, r * 2, r * 2);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// UO field (see UoTile): the graphic stands with the middle pixel of its lowest row on the bottom corner of
        /// the main tile. Draws 5x5 neighbour tiles (thin), the main tile (bold), the vertical line through the
        /// tile centre, the ground line (lowest row) and the fix point. All in image pixels, scaled at the end.
        /// </summary>
        private void DrawUoField(Graphics g, int w, int h)
        {
            (int tx, int ty) = UoTile.TileOrigin(w, h);
            using var thin = new Pen(Color.FromArgb(Math.Min((int)GridColor.A, 120), GridColor));
            using var bold = new Pen(Color.FromArgb(230, 255, 200, 40), Math.Max(2, _zoom / 2f));
            using var help = new Pen(Color.FromArgb(150, 255, 200, 40)) { DashStyle = DashStyle.Dash };
            using var dot = new SolidBrush(Color.FromArgb(200, GridColor));
            float r = Math.Max(1.5f, _zoom / 3f);

            for (int j = -2; j <= 2; ++j)
            {
                for (int i = -2; i <= 2; ++i)
                {
                    (int nx, int ny) = UoTile.NeighbourOrigin(tx, ty, i, j);
                    (int X, int Y)[] d = UoTile.Diamond(nx, ny);
                    DrawDiamond(g, thin, d);
                    foreach ((int X, int Y) c in d)
                    {
                        g.FillEllipse(dot, (c.X * _zoom) - r, (c.Y * _zoom) - r, r * 2, r * 2);
                    }
                }
            }

            DrawDiamond(g, bold, UoTile.Diamond(tx, ty));

            // centre line (x = tx + 22) and ground line (y = ty + 44 = lowest row of the frame)
            int top = -_margin, bottom = h + _margin, left = -_margin, right = w + _margin;
            g.DrawLine(help, (tx + UoTile.Half) * _zoom, top * _zoom, (tx + UoTile.Half) * _zoom, bottom * _zoom);
            g.DrawLine(help, left * _zoom, (ty + UoTile.Size) * _zoom, right * _zoom, (ty + UoTile.Size) * _zoom);

            // fix point: middle pixel of the lowest row
            (int ax, int ay) = UoTile.Anchor(w, h);
            using var fix = new Pen(Color.Red, 2);
            g.DrawRectangle(fix, ax * _zoom, ay * _zoom, _zoom, _zoom);
        }

        private void DrawDiamond(Graphics g, Pen pen, (int X, int Y)[] d)
        {
            var pts = new PointF[5];
            for (int k = 0; k < 4; ++k)
            {
                pts[k] = new PointF(d[k].X * _zoom, d[k].Y * _zoom);
            }

            pts[4] = pts[0];
            g.DrawLines(pen, pts);
        }

        private void DrawHandles(Graphics g)
        {
            if (Handles.Count == 0)
            {
                return;
            }

            using var link = new Pen(Color.FromArgb(200, 255, 200, 0)) { DashStyle = DashStyle.Dash };
            foreach ((int a, int b) in HandleLinks)
            {
                if (a < Handles.Count && b < Handles.Count)
                {
                    g.DrawLine(link, Handles[a].X * _zoom, Handles[a].Y * _zoom, Handles[b].X * _zoom, Handles[b].Y * _zoom);
                }
            }

            for (int i = 0; i < Handles.Count; ++i)
            {
                float x = Handles[i].X * _zoom, y = Handles[i].Y * _zoom;
                float hs = Handles.Count > 20 ? 3.5f : 5;
                var rect = new RectangleF(x - hs, y - hs, hs * 2, hs * 2);
                g.FillRectangle(i == _drag ? Brushes.Orange : Brushes.White, rect);
                g.DrawRectangle(Pens.Black, rect.X, rect.Y, rect.Width, rect.Height);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            _drag = -1;
            double best = 10;
            for (int i = 0; i < Handles.Count; ++i)
            {
                double d = Math.Sqrt(Math.Pow((Handles[i].X * _zoom) + Off - e.X, 2) + Math.Pow((Handles[i].Y * _zoom) + Off - e.Y, 2));
                if (d < best)
                {
                    best = d;
                    _drag = i;
                }
            }

            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_pixels == null)
            {
                return;
            }

            int px = (int)Math.Floor((e.X - Off) / (double)_zoom), py = (int)Math.Floor((e.Y - Off) / (double)_zoom);
            bool inside = px >= 0 && py >= 0 && px < _pixels.Width && py < _pixels.Height;
            PixelHover?.Invoke(inside ? px : -1, inside ? py : -1, inside ? _pixels[px, py] : (ushort)0);

            if (_drag >= 0 && e.Button == MouseButtons.Left)
            {
                // points sit on pixel edges: round to whole pixels -> pixel exact results
                int ix = (int)Math.Round((e.X - Off) / (double)_zoom), iy = (int)Math.Round((e.Y - Off) / (double)_zoom);
                HandleMoved?.Invoke(_drag, ix, iy);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) != 0)
            {
                ZoomRequested?.Invoke(e.Delta > 0 ? 1 : -1);
                if (e is HandledMouseEventArgs handled)
                {
                    handled.Handled = true;
                }

                return;
            }

            base.OnMouseWheel(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_drag >= 0)
            {
                _drag = -1;
                HandleDragEnded?.Invoke();
                Invalidate();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReplaceBitmap(ref _image, null);
                ReplaceBitmap(ref _ghost, null);
            }

            base.Dispose(disposing);
        }
    }
}

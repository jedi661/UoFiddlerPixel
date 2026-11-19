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
        Iso
    }

    /// <summary>
    /// Zoomed, pixel exact view of an <see cref="ArtPixelImage"/> (nearest neighbour, no smoothing) with an optional
    /// ghost of the original, a grid (lines / dots / UO iso diamonds) and draggable edit points.
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
                Size = new Size((_pixels.Width * _zoom) + 1, (_pixels.Height * _zoom) + 1);
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

            if (GridMode == PixelGridMode.Iso)
            {
                // UO tile diamonds (GridSize = tile width, height = width / 2), anchored at the bottom centre
                float ax = w / 2f, ay = h;
                int tw = Math.Max(4, GridSize);
                for (int c = -(w + (2 * h)); c <= w + (2 * h); c += tw)
                {
                    // lines x - 2y = ax - 2ay + c   and   x + 2y = ax + 2ay + c
                    DrawIsoLine(g, pen, ax - (2 * ay) + c, 1, w, h);
                    DrawIsoLine(g, pen, ax + (2 * ay) + c, -1, w, h);
                }

                for (int c1 = -(w + (2 * h)); c1 <= w + (2 * h); c1 += tw)
                {
                    for (int c2 = -(w + (2 * h)); c2 <= w + (2 * h); c2 += tw)
                    {
                        float k1 = ax - (2 * ay) + c1, k2 = ax + (2 * ay) + c2;
                        float x = (k1 + k2) / 2, y = (k2 - k1) / 4;
                        if (x >= 0 && y >= 0 && x <= w && y <= h)
                        {
                            g.FillEllipse(dot, (x * _zoom) - r, (y * _zoom) - r, r * 2, r * 2);
                        }
                    }
                }

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

        /// <summary>Line x - 2y = k (sign 1, "\") or x + 2y = k (sign -1, "/") clipped to the image.</summary>
        private void DrawIsoLine(Graphics g, Pen pen, float k, int sign, int w, int h)
        {
            // y = (x - k) / 2 for sign 1, y = (k - x) / 2 for sign -1
            float yAt0 = sign == 1 ? -k / 2 : k / 2;
            float yAtW = sign == 1 ? (w - k) / 2 : (k - w) / 2;
            if ((yAt0 < 0 && yAtW < 0) || (yAt0 > h && yAtW > h))
            {
                return;
            }

            g.DrawLine(pen, 0, yAt0 * _zoom, w * _zoom, yAtW * _zoom);
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
                var rect = new RectangleF(x - 5, y - 5, 10, 10);
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
                double d = Math.Sqrt(Math.Pow((Handles[i].X * _zoom) - e.X, 2) + Math.Pow((Handles[i].Y * _zoom) - e.Y, 2));
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

            int px = e.X / _zoom, py = e.Y / _zoom;
            bool inside = px >= 0 && py >= 0 && px < _pixels.Width && py < _pixels.Height;
            PixelHover?.Invoke(inside ? px : -1, inside ? py : -1, inside ? _pixels[px, py] : (ushort)0);

            if (_drag >= 0 && e.Button == MouseButtons.Left)
            {
                // points sit on pixel edges: round to whole pixels -> pixel exact results
                int ix = (int)Math.Round(e.X / (double)_zoom), iy = (int)Math.Round(e.Y / (double)_zoom);
                HandleMoved?.Invoke(_drag, ix, iy);
            }
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

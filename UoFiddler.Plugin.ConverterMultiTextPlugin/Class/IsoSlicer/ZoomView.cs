using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.IsoSlicer
{
    /// <summary>
    /// Shows an image with zoom (mouse wheel), moving (right / middle mouse) and an overlay drawn by the owner.
    /// Reports the image pixel under the mouse and clicks. Usable in the designer.
    /// </summary>
    public class ZoomView : Control
    {
        private Image _image;
        private float _zoom = 1f;
        private PointF _origin;          // image point at the top left corner
        private bool _panning;
        private Point _panStart;
        private PointF _panOrigin;

        public ZoomView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.FromArgb(40, 40, 40);
        }

        /// <summary>Draws over the image; the matrix maps image pixels to the screen.</summary>
        public event EventHandler<ZoomPaintEventArgs> PaintOverlay;

        /// <summary>Image pixel under the mouse (X = -1 when outside).</summary>
        public event EventHandler<Point> HoverPixel;

        public event EventHandler<MouseEventArgs> ImageClick;

        public event EventHandler ZoomChanged;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image Image
        {
            get => _image;
            set
            {
                bool first = _image == null || value == null || _image.Size != value?.Size;
                _image = value;
                if (first)
                {
                    FitImage();
                }
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool Checkerboard { get; set; } = true;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Zoom => _zoom;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ZoomText => _zoom >= 1 ? $"{_zoom:0.#}×" : $"{_zoom * 100:0}%";

        public void FitImage()
        {
            if (_image == null || Width < 10 || Height < 10)
            {
                _zoom = 1;
                _origin = PointF.Empty;
                ZoomChanged?.Invoke(this, EventArgs.Empty);
                return;
            }
            float z = Math.Min((Width - 10f) / _image.Width, (Height - 10f) / _image.Height);
            _zoom = Snap(Math.Min(8, z));
            _origin = new PointF(_image.Width / 2f - Width / 2f / _zoom, _image.Height / 2f - Height / 2f / _zoom);
            ZoomChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        public void ZoomStep(int dir, Point? around = null)
        {
            Point p = around ?? new Point(Width / 2, Height / 2);
            PointF img = ToImage(p);
            float[] steps = { 0.125f, 0.25f, 1f / 3, 0.5f, 2f / 3, 1, 1.5f, 2, 3, 4, 6, 8, 12, 16, 24, 32 };
            int i = Array.FindIndex(steps, s => s >= _zoom - 0.001f);
            if (i < 0)
            {
                i = steps.Length - 1;
            }
            i = Math.Max(0, Math.Min(steps.Length - 1, dir > 0 ? (steps[i] > _zoom + 0.001f ? i : i + 1) : i - 1));
            _zoom = steps[i];
            _origin = new PointF(img.X - p.X / _zoom, img.Y - p.Y / _zoom);
            ZoomChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        public void CenterOn(PointF imagePoint)
        {
            _origin = new PointF(imagePoint.X - Width / 2f / _zoom, imagePoint.Y - Height / 2f / _zoom);
            Invalidate();
        }

        private static float Snap(float z)
        {
            float[] steps = { 0.125f, 0.25f, 1f / 3, 0.5f, 2f / 3, 1, 1.5f, 2, 3, 4, 6, 8 };
            float best = steps[0];
            foreach (float s in steps)
            {
                if (s <= z + 0.001f)
                {
                    best = s;
                }
            }
            return best;
        }

        public PointF ToImage(Point p) => new PointF(_origin.X + p.X / _zoom, _origin.Y + p.Y / _zoom);

        public PointF ToScreen(PointF p) => new PointF((p.X - _origin.X) * _zoom, (p.Y - _origin.Y) * _zoom);

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (_image == null)
            {
                TextRenderer.DrawText(g, Text, Font, ClientRectangle, Color.Gainsboro, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
                return;
            }
            RectangleF dest = new RectangleF(ToScreen(PointF.Empty), new SizeF(_image.Width * _zoom, _image.Height * _zoom));
            if (Checkerboard)
            {
                RectangleF vis = RectangleF.Intersect(dest, ClientRectangle);
                if (vis.Width > 0 && vis.Height > 0)
                {
                    using (var b = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.FromArgb(70, 70, 70), Color.FromArgb(55, 55, 55)))
                    {
                        g.FillRectangle(b, vis);
                    }
                }
            }
            g.InterpolationMode = _zoom >= 1 ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBilinear;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(_image, dest);
            g.PixelOffsetMode = PixelOffsetMode.Default;
            if (PaintOverlay != null)
            {
                using (var m = new Matrix())
                {
                    m.Scale(_zoom, _zoom);
                    m.Translate(-_origin.X, -_origin.Y);
                    PaintOverlay(this, new ZoomPaintEventArgs(g, m, _zoom));
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_image != null)
            {
                ZoomStep(e.Delta > 0 ? 1 : -1, e.Location);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                _panning = true;
                _panStart = e.Location;
                _panOrigin = _origin;
                Cursor = Cursors.SizeAll;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_panning)
            {
                _origin = new PointF(_panOrigin.X - (e.X - _panStart.X) / _zoom, _panOrigin.Y - (e.Y - _panStart.Y) / _zoom);
                Invalidate();
                return;
            }
            if (_image != null)
            {
                PointF p = ToImage(e.Location);
                var pixel = new Point((int)Math.Floor(p.X), (int)Math.Floor(p.Y));
                HoverPixel?.Invoke(this, pixel.X >= 0 && pixel.Y >= 0 && pixel.X < _image.Width && pixel.Y < _image.Height ? pixel : new Point(-1, -1));
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_panning)
            {
                bool moved = Math.Abs(e.X - _panStart.X) + Math.Abs(e.Y - _panStart.Y) > 3;
                _panning = false;
                Cursor = Cursors.Default;
                if (moved)
                {
                    return;
                }
            }
            if (e.Button == MouseButtons.Left && _image != null)
            {
                ImageClick?.Invoke(this, e);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            HoverPixel?.Invoke(this, new Point(-1, -1));
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }

    public sealed class ZoomPaintEventArgs : EventArgs
    {
        public ZoomPaintEventArgs(Graphics graphics, Matrix imageToScreen, float zoom)
        {
            Graphics = graphics;
            ImageToScreen = imageToScreen;
            Zoom = zoom;
        }

        public Graphics Graphics { get; }

        public Matrix ImageToScreen { get; }

        public float Zoom { get; }
    }
}

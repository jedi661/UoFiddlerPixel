/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UoFiddler.Controls.MultiMapImages
{
    /// <summary>
    /// Shows an image with zoom (mouse wheel, zooms at the mouse position) and scrolling.
    /// Moving: middle mouse button, or left mouse button when <see cref="PanWithLeftButton"/> is set.
    /// </summary>
    public class ZoomCanvas : ScrollableControl
    {
        public const float MinZoom = 0.02f;
        public const float MaxZoom = 32f;

        private Image _image;
        private float _zoom = 1f;
        private bool _panning;
        private Point _panStart;
        private Point _panScrollStart;

        public ZoomCanvas()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            AutoScroll = true;
            BackColor = Color.FromArgb(96, 96, 96);
            TabStop = true;
        }

        /// <summary>Raised after the zoom changed</summary>
        public event EventHandler ZoomChanged;

        /// <summary>Paints over the image. The graphics is transformed to image coordinates.</summary>
        public event PaintEventHandler PaintOverlay;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Image Image
        {
            get => _image;
            set
            {
                bool sizeChanged = _image == null || value == null || _image.Size != value.Size;
                _image = value;
                if (sizeChanged)
                {
                    UpdateScrollSize();
                }

                Invalidate();
            }
        }

        [DefaultValue(false)]
        public bool PanWithLeftButton { get; set; }

        /// <summary>Use smooth scaling when zoomed out (nearest neighbor is always used when zoomed in)</summary>
        [DefaultValue(true)]
        public bool SmoothZoomOut { get; set; } = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public float Zoom
        {
            get => _zoom;
            set => SetZoom(value, new Point(ClientSize.Width / 2, ClientSize.Height / 2));
        }

        /// <summary>True while the user moves the image</summary>
        [Browsable(false)]
        public bool IsPanning => _panning;

        /// <summary>Top left corner of the image in client coordinates</summary>
        public PointF ImageOrigin
        {
            get
            {
                if (_image == null)
                {
                    return PointF.Empty;
                }

                float w = _image.Width * _zoom, h = _image.Height * _zoom;
                float x = w < ClientSize.Width ? (ClientSize.Width - w) / 2f : AutoScrollPosition.X;
                float y = h < ClientSize.Height ? (ClientSize.Height - h) / 2f : AutoScrollPosition.Y;
                return new PointF(x, y);
            }
        }

        public PointF ClientToImage(Point p)
        {
            PointF o = ImageOrigin;
            return new PointF((p.X - o.X) / _zoom, (p.Y - o.Y) / _zoom);
        }

        public PointF ImageToClient(PointF p)
        {
            PointF o = ImageOrigin;
            return new PointF(o.X + p.X * _zoom, o.Y + p.Y * _zoom);
        }

        /// <summary>Repaints only the given image rectangle (plus a border)</summary>
        public void InvalidateImageRect(RectangleF r, float border = 2)
        {
            PointF a = ImageToClient(new PointF(r.Left, r.Top));
            var rect = Rectangle.Round(new RectangleF(a.X - border * _zoom - 2, a.Y - border * _zoom - 2,
                r.Width * _zoom + 2 * border * _zoom + 4, r.Height * _zoom + 2 * border * _zoom + 4));
            Invalidate(rect);
        }

        public void ZoomIn()
        {
            SetZoom(NextStep(_zoom, true), new Point(ClientSize.Width / 2, ClientSize.Height / 2));
        }

        public void ZoomOut()
        {
            SetZoom(NextStep(_zoom, false), new Point(ClientSize.Width / 2, ClientSize.Height / 2));
        }

        public void ZoomActualSize()
        {
            SetZoom(1f, new Point(ClientSize.Width / 2, ClientSize.Height / 2));
        }

        public void ZoomToFit()
        {
            if (_image == null || ClientSize.Width < 10 || ClientSize.Height < 10)
            {
                return;
            }

            float z = Math.Min((ClientSize.Width - 4) / (float)_image.Width, (ClientSize.Height - 4) / (float)_image.Height);
            SetZoom(Math.Max(MinZoom, Math.Min(MaxZoom, z)), new Point(ClientSize.Width / 2, ClientSize.Height / 2));
        }

        private static float NextStep(float zoom, bool up)
        {
            float f = up ? 1.25f : 0.8f;
            float z = zoom * f;
            // snap to 100 %
            if ((zoom < 1f && z > 1f) || (zoom > 1f && z < 1f))
            {
                z = 1f;
            }

            return z;
        }

        /// <summary>Changes the zoom, the image point under <paramref name="anchor"/> stays at its place</summary>
        public void SetZoom(float zoom, Point anchor)
        {
            zoom = Math.Max(MinZoom, Math.Min(MaxZoom, zoom));
            if (_image == null)
            {
                _zoom = zoom;
                ZoomChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (Math.Abs(zoom - _zoom) < 0.0001f)
            {
                return;
            }

            PointF imagePoint = ClientToImage(anchor);
            _zoom = zoom;
            UpdateScrollSize();

            // scroll so that imagePoint is under the anchor again
            int sx = (int)Math.Round(imagePoint.X * _zoom - anchor.X);
            int sy = (int)Math.Round(imagePoint.Y * _zoom - anchor.Y);
            AutoScrollPosition = new Point(Math.Max(0, sx), Math.Max(0, sy));
            Invalidate();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateScrollSize()
        {
            AutoScrollMinSize = _image == null
                ? Size.Empty
                : new Size((int)Math.Ceiling(_image.Width * _zoom), (int)Math.Ceiling(_image.Height * _zoom));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            if (_image == null)
            {
                base.OnPaint(e);
                return;
            }

            PointF o = ImageOrigin;
            var dest = new RectangleF(o.X, o.Y, _image.Width * _zoom, _image.Height * _zoom);
            RectangleF visible = RectangleF.Intersect(dest, e.ClipRectangle);
            if (visible.Width > 0 && visible.Height > 0)
            {
                // only draw the visible part (fast for big images)
                var src = new RectangleF((visible.X - o.X) / _zoom, (visible.Y - o.Y) / _zoom, visible.Width / _zoom, visible.Height / _zoom);
                // whole pixels avoid blurred borders when zoomed in
                float sx = (float)Math.Floor(src.X), sy = (float)Math.Floor(src.Y);
                float ex = (float)Math.Min(_image.Width, Math.Ceiling(src.Right)), ey = (float)Math.Min(_image.Height, Math.Ceiling(src.Bottom));
                src = new RectangleF(sx, sy, ex - sx, ey - sy);
                var dst = new RectangleF(o.X + sx * _zoom, o.Y + sy * _zoom, src.Width * _zoom, src.Height * _zoom);

                e.Graphics.InterpolationMode = _zoom >= 1f || !SmoothZoomOut
                    ? InterpolationMode.NearestNeighbor
                    : InterpolationMode.HighQualityBilinear;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                e.Graphics.DrawImage(_image, dst, src, GraphicsUnit.Pixel);
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
            }

            if (PaintOverlay != null)
            {
                GraphicsState state = e.Graphics.Save();
                e.Graphics.TranslateTransform(o.X, o.Y);
                e.Graphics.ScaleTransform(_zoom, _zoom);
                PaintOverlay(this, e);
                e.Graphics.Restore(state);
            }

            base.OnPaint(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (_image != null)
            {
                float factor = e.Delta > 0 ? 1.2f : 1f / 1.2f;
                int steps = Math.Max(1, Math.Abs(e.Delta) / 120);
                float z = _zoom;
                for (int i = 0; i < steps; i++)
                {
                    z *= factor;
                }

                SetZoom(z, e.Location);
            }

            if (e is HandledMouseEventArgs h)
            {
                h.Handled = true;
            }

            // no base call: the wheel zooms instead of scrolling
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            // the wheel only goes to the focused control
            Form form = FindForm();
            if (form != null && Form.ActiveForm == form && !Focused && !(form.ActiveControl is TextBoxBase))
            {
                Focus();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!Focused)
            {
                Focus();
            }

            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && PanWithLeftButton))
            {
                _panning = true;
                _panStart = e.Location;
                _panScrollStart = new Point(-AutoScrollPosition.X, -AutoScrollPosition.Y);
                Cursor = Cursors.SizeAll;
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_panning)
            {
                AutoScrollPosition = new Point(
                    Math.Max(0, _panScrollStart.X - (e.X - _panStart.X)),
                    Math.Max(0, _panScrollStart.Y - (e.Y - _panStart.Y)));
                Invalidate();
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_panning && (e.Button == MouseButtons.Middle || e.Button == MouseButtons.Left))
            {
                _panning = false;
                Cursor = DefaultCursorForTool ?? Cursors.Default;
            }

            base.OnMouseUp(e);
        }

        /// <summary>Cursor that is restored after moving the image</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Cursor DefaultCursorForTool { get; set; }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
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
            }

            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            const int step = 40;
            Point p = new Point(-AutoScrollPosition.X, -AutoScrollPosition.Y);
            switch (e.KeyCode)
            {
                case Keys.Left:
                    AutoScrollPosition = new Point(Math.Max(0, p.X - step), p.Y);
                    e.Handled = true;
                    break;
                case Keys.Right:
                    AutoScrollPosition = new Point(p.X + step, p.Y);
                    e.Handled = true;
                    break;
                case Keys.Up:
                    AutoScrollPosition = new Point(p.X, Math.Max(0, p.Y - step));
                    e.Handled = true;
                    break;
                case Keys.Down:
                    AutoScrollPosition = new Point(p.X, p.Y + step);
                    e.Handled = true;
                    break;
                case Keys.Add:
                case Keys.Oemplus:
                    if (!e.Control)
                    {
                        ZoomIn();
                        e.Handled = true;
                    }

                    break;
                case Keys.Subtract:
                case Keys.OemMinus:
                    if (!e.Control)
                    {
                        ZoomOut();
                        e.Handled = true;
                    }

                    break;
            }

            if (e.Handled)
            {
                Invalidate();
            }

            base.OnKeyDown(e);
        }
    }
}

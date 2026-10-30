/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Shows one image on a checkerboard background, with zoom (mouse wheel) and scrolling.
    /// </summary>
    public sealed class EcPreviewPanel : ScrollableControl
    {
        private Bitmap _image;
        private float _zoom = 1f;

        public EcPreviewPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            AutoScroll = true;
            BackColor = Color.White;
        }

        /// <summary>
        /// The panel takes ownership of the bitmap and disposes the previous one.
        /// </summary>
        public Bitmap Image
        {
            get => _image;
            set
            {
                if (ReferenceEquals(_image, value))
                {
                    return;
                }
                _image?.Dispose();
                _image = value;
                UpdateScrollSize();
                Invalidate();
            }
        }

        public float Zoom
        {
            get => _zoom;
            set
            {
                _zoom = Math.Max(0.125f, Math.Min(16f, value));
                UpdateScrollSize();
                Invalidate();
                ZoomChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler ZoomChanged;

        /// <summary>Tiled background behind the image (null = checkerboard).</summary>
        public Image BackgroundTile
        {
            get => _backgroundTile;
            set
            {
                _backgroundTile = value;
                Invalidate();
            }
        }

        private Image _backgroundTile;

        /// <summary>Called after the image is painted: graphics, area of the image on the control, zoom.</summary>
        public Action<Graphics, Rectangle, float> Overlay { get; set; }

        /// <summary>Area of the image on the control.</summary>
        public Rectangle ImageArea
        {
            get
            {
                Size size = ScaledSize;
                int x = Math.Max(0, (ClientSize.Width - size.Width) / 2) + AutoScrollPosition.X;
                int y = Math.Max(0, (ClientSize.Height - size.Height) / 2) + AutoScrollPosition.Y;
                return new Rectangle(x, y, size.Width, size.Height);
            }
        }

        /// <summary>Image pixel under a point of the control, or null outside the image.</summary>
        public Point? ClientToImage(Point point)
        {
            if (_image == null)
            {
                return null;
            }
            Rectangle area = ImageArea;
            if (!area.Contains(point))
            {
                return null;
            }
            return new Point((int)((point.X - area.X) / _zoom), (int)((point.Y - area.Y) / _zoom));
        }

        private Size ScaledSize => _image == null ? Size.Empty : new Size((int)(_image.Width * _zoom), (int)(_image.Height * _zoom));

        private void UpdateScrollSize()
        {
            AutoScrollMinSize = ScaledSize;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) != 0 || !VScroll)
            {
                Zoom = e.Delta > 0 ? _zoom * 2 : _zoom / 2;
                return;
            }
            base.OnMouseWheel(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (_image == null)
            {
                return;
            }

            Rectangle area = ImageArea;

            if (_backgroundTile != null)
            {
                using (var brush = new TextureBrush(_backgroundTile, System.Drawing.Drawing2D.WrapMode.Tile))
                {
                    brush.TranslateTransform(area.X, area.Y);
                    g.FillRectangle(brush, area);
                }
            }
            else
            {
                EcImageHelper.DrawCheckerboard(g, area);
            }
            g.InterpolationMode = _zoom >= 1 ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(_image, area);
            g.DrawRectangle(Pens.Silver, area.X - 1, area.Y - 1, area.Width + 1, area.Height + 1);
            Overlay?.Invoke(g, area, _zoom);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _image?.Dispose();
                _image = null;
            }
            base.Dispose(disposing);
        }
    }
}

/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
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

            Size size = ScaledSize;
            int x = Math.Max(0, (ClientSize.Width - size.Width) / 2) + AutoScrollPosition.X;
            int y = Math.Max(0, (ClientSize.Height - size.Height) / 2) + AutoScrollPosition.Y;
            var area = new Rectangle(x, y, size.Width, size.Height);

            EcImageHelper.DrawCheckerboard(g, area);
            g.InterpolationMode = _zoom >= 1 ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(_image, area);
            g.DrawRectangle(Pens.Silver, area.X - 1, area.Y - 1, area.Width + 1, area.Height + 1);
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

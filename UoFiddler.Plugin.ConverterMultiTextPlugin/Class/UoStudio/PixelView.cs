/***************************************************************************
 *
 * $Author: Nikodemus
 *
 * "THE WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a Wine in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio
{
    /// <summary>
    /// Picture view with checker board background, integer zoom (nearest neighbour, pixel exact)
    /// and optional dark background (for additive effects).
    /// </summary>
    public class PixelView : Panel
    {
        private Image _image;
        private int _zoom = 2;
        private bool _darkBackground;

        public PixelView()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            AutoScroll = true;
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = Color.White;
        }

        /// <summary>The displayed image (not disposed by the view).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image Image
        {
            get => _image;
            set
            {
                _image = value;
                UpdateScrollSize();
                Invalidate();
            }
        }

        [DefaultValue(2)]
        [Category("UO Studio")]
        public int Zoom
        {
            get => _zoom;
            set
            {
                _zoom = Math.Max(1, Math.Min(16, value));
                UpdateScrollSize();
                Invalidate();
            }
        }

        [DefaultValue(false)]
        [Category("UO Studio")]
        public bool DarkBackground
        {
            get => _darkBackground;
            set
            {
                _darkBackground = value;
                Invalidate();
            }
        }

        /// <summary>Optional text drawn into the upper left corner.</summary>
        [DefaultValue(null)]
        [Category("UO Studio")]
        public string Caption { get; set; }

        /// <summary>Optional extra painting on top of the image (in image coordinates * zoom).</summary>
        public event Action<Graphics, Rectangle> PaintOverlay;

        /// <summary>
        /// Size of the image, or empty if there is none or it was already disposed
        /// (a disposed GDI+ image throws "Parameter is not valid" on Width/Height).
        /// </summary>
        private Size ImageSize
        {
            get
            {
                if (_image == null)
                {
                    return Size.Empty;
                }
                try
                {
                    return _image.Size;
                }
                catch (ArgumentException)
                {
                    _image = null;
                    return Size.Empty;
                }
            }
        }

        private void UpdateScrollSize()
        {
            Size s = ImageSize;
            AutoScrollMinSize = s.IsEmpty ? Size.Empty : new Size(s.Width * _zoom, s.Height * _zoom);
        }

        [Browsable(false)]
        public Rectangle ImageRectangle
        {
            get
            {
                Size s = ImageSize;
                if (s.IsEmpty)
                {
                    return Rectangle.Empty;
                }
                int w = s.Width * _zoom, h = s.Height * _zoom;
                int x = Math.Max(0, (ClientSize.Width - w) / 2) + AutoScrollPosition.X;
                int y = Math.Max(0, (ClientSize.Height - h) / 2) + AutoScrollPosition.Y;
                return new Rectangle(x, y, w, h);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            if (_darkBackground)
            {
                g.Clear(Color.FromArgb(20, 20, 24));
            }
            else
            {
                StudioImaging.DrawChecker(g, ClientRectangle, 10);
            }

            if (!ImageSize.IsEmpty)
            {
                Rectangle r = ImageRectangle;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(_image, r);
                PaintOverlay?.Invoke(g, r);
            }

            if (!string.IsNullOrEmpty(Caption))
            {
                using var bg = new SolidBrush(Color.FromArgb(160, 0, 0, 0));
                SizeF s = g.MeasureString(Caption, Font);
                g.FillRectangle(bg, 2, 2, s.Width + 6, s.Height + 2);
                g.DrawString(Caption, Font, Brushes.White, 5, 3);
            }
        }
    }
}

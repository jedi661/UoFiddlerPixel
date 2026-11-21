// /***************************************************************************
//  *
//  * Dragon Map Studio – overview of the whole map (click = jump there)
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls
{
    /// <summary>Small overview picture with the visible area of the <see cref="MapCanvas"/>.</summary>
    [ToolboxItem(true)]
    public class MapNavigator : Control
    {
        private Bitmap _thumb;
        private int _mapW = 1;
        private int _mapH = 1;
        private RectangleF _view;

        public MapNavigator()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.FromArgb(25, 25, 28);
            Cursor = Cursors.Hand;
        }

        /// <summary>Raised with the map position the user clicked / dragged to.</summary>
        public event EventHandler<MapPointEventArgs> NavigateTo;

        public void SetThumbnail(Bitmap thumb, int mapWidth, int mapHeight)
        {
            _thumb?.Dispose();
            _thumb = thumb;
            _mapW = Math.Max(1, mapWidth);
            _mapH = Math.Max(1, mapHeight);
            Invalidate();
        }

        /// <summary>Extra marked area (e.g. the selection of the map editor), empty = none.</summary>
        public void SetMark(RectangleF mark)
        {
            _mark = mark;
            Invalidate();
        }

        private RectangleF _mark;

        public void SetView(RectangleF view)
        {
            _view = view;
            if (FollowView && _zoom > 1.001f && !_panning && view.Width > 0)
            {
                // keep the view inside the zoomed overview
                RectangleF r = ImageRect();
                float k = r.Width / _mapW;
                var v = new RectangleF(r.X + view.X * k, r.Y + view.Y * k, view.Width * k, view.Height * k);
                if (v.Left < 0 || v.Top < 0 || v.Right > Width || v.Bottom > Height)
                {
                    _center = new PointF(view.X + view.Width / 2, view.Y + view.Height / 2);
                }
            }
            Invalidate();
        }

        private float _zoom = 1f;
        private PointF _center = new PointF(-1, -1);
        private bool _panning;
        private Point _panStart;
        private PointF _panCenter;

        /// <summary>1 = whole map, 2–32 = zoomed in (mouse wheel, buttons). The zoomed overview follows the view.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Zoom
        {
            get => _zoom;
            set
            {
                _zoom = Math.Clamp(value, 1f, 32f);
                ZoomChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        /// <summary>Zoomed in: keep the visible area of the big view in the middle.</summary>
        [DefaultValue(true)]
        public bool FollowView { get; set; } = true;

        public event EventHandler ZoomChanged;

        public void ZoomStep(int dir) => Zoom = dir > 0 ? _zoom * 1.5f : _zoom / 1.5f;

        /// <summary>Map point in the middle of the overview (zoomed).</summary>
        private PointF Center()
        {
            if (_center.X < 0)
            {
                return new PointF(_mapW / 2f, _mapH / 2f);
            }
            return _center;
        }

        private RectangleF ImageRect()
        {
            float k = Math.Min((float)Width / _mapW, (float)Height / _mapH) * _zoom;
            float w = _mapW * k, h = _mapH * k;
            if (_zoom <= 1.001f)
            {
                return new RectangleF((Width - w) / 2, (Height - h) / 2, w, h);
            }
            PointF c = Center();
            return new RectangleF(Width / 2f - c.X * k, Height / 2f - c.Y * k, w, h);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            RectangleF r = ImageRect();
            float k = r.Width / _mapW;
            // zoom around the mouse
            var at = new PointF((e.X - r.X) / k, (e.Y - r.Y) / k);
            float old = _zoom;
            Zoom = e.Delta > 0 ? _zoom * 1.25f : _zoom / 1.25f;
            if (_zoom > 1.001f)
            {
                float k2 = k * _zoom / old;
                _center = new PointF(at.X - (e.X - Width / 2f) / k2, at.Y - (e.Y - Height / 2f) / k2);
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (_thumb == null)
            {
                return;
            }
            RectangleF r = ImageRect();
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(_thumb, r);
            float k = r.Width / _mapW;
            var v = new RectangleF(r.X + _view.X * k, r.Y + _view.Y * k, _view.Width * k, _view.Height * k);
            if (!_mark.IsEmpty)
            {
                using var markPen = new Pen(Color.OrangeRed, 2f) { DashStyle = DashStyle.Dash };
                g.DrawRectangle(markPen, r.X + _mark.X * k, r.Y + _mark.Y * k, Math.Max(2, _mark.Width * k), Math.Max(2, _mark.Height * k));
            }
            using var pen = new Pen(Color.Yellow, 1.5f);
            g.DrawRectangle(Pens.Black, v.X - 1, v.Y - 1, v.Width + 2, v.Height + 2);
            g.DrawRectangle(pen, v.X, v.Y, v.Width, v.Height);
            if (_zoom > 1.001f)
            {
                string text = $"{_zoom:0.#}×  ·  Rad = Zoom, rechte Taste = schieben";
                using var font = new Font("Segoe UI", 7.5f);
                SizeF s = g.MeasureString(text, font);
                using var back = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
                g.FillRectangle(back, 2, Height - s.Height - 4, s.Width + 6, s.Height + 2);
                g.DrawString(text, font, Brushes.White, 5, Height - s.Height - 3);
            }
        }

        private void Navigate(Point p)
        {
            RectangleF r = ImageRect();
            if (r.Width <= 0)
            {
                return;
            }
            float k = r.Width / _mapW;
            int x = (int)((p.X - r.X) / k), y = (int)((p.Y - r.Y) / k);
            NavigateTo?.Invoke(this, new MapPointEventArgs(Math.Clamp(x, 0, _mapW - 1), Math.Clamp(y, 0, _mapH - 1), -1));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (e.Button == MouseButtons.Left)
            {
                Navigate(e.Location);
            }
            else if ((e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle) && _zoom > 1.001f)
            {
                _panning = true;
                _panStart = e.Location;
                _panCenter = Center();
                Cursor = Cursors.SizeAll;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Left)
            {
                Navigate(e.Location);
            }
            else if (_panning)
            {
                RectangleF r = ImageRect();
                float k = r.Width / _mapW;
                _center = new PointF(Math.Clamp(_panCenter.X - (e.X - _panStart.X) / k, 0, _mapW), Math.Clamp(_panCenter.Y - (e.Y - _panStart.Y) / k, 0, _mapH));
                Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            // the mouse wheel zooms as soon as the mouse is over the overview
            if (CanFocus && FindForm()?.ContainsFocus == true)
            {
                Focus();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_panning)
            {
                _panning = false;
                Cursor = Cursors.Hand;
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.Button == MouseButtons.Right)
            {
                Zoom = 1f;
                _center = new PointF(-1, -1);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _thumb?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

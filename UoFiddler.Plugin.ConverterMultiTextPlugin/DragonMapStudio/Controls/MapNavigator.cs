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

        public void SetView(RectangleF view)
        {
            _view = view;
            Invalidate();
        }

        private RectangleF ImageRect()
        {
            float k = Math.Min((float)Width / _mapW, (float)Height / _mapH);
            float w = _mapW * k, h = _mapH * k;
            return new RectangleF((Width - w) / 2, (Height - h) / 2, w, h);
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
            using var pen = new Pen(Color.Yellow, 1.5f);
            g.DrawRectangle(Pens.Black, v.X - 1, v.Y - 1, v.Width + 2, v.Height + 2);
            g.DrawRectangle(pen, v.X, v.Y, v.Width, v.Height);
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
            if (e.Button == MouseButtons.Left)
            {
                Navigate(e.Location);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Left)
            {
                Navigate(e.Location);
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

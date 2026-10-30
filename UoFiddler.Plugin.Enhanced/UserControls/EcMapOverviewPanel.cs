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
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Imaging;
using UoFiddler.Plugin.Enhanced.EcUltima.Maps;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Overview of an Enhanced Client map (radar image); a click selects a tile.
    /// </summary>
    public sealed class EcMapOverviewPanel : ScrollableControl
    {
        public Bitmap Image;
        public float TileScale = 1f;    // tiles per pixel of the image
        public Rectangle Selection;     // in tiles
        public event Action<int, int> TileClicked;

        public EcMapOverviewPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            AutoScroll = true;
            BackColor = Color.Black;
        }

        public void SetImage(Bitmap image, float scale)
        {
            Image?.Dispose();
            Image = image;
            TileScale = scale;
            AutoScrollMinSize = image?.Size ?? Size.Empty;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            if (Image == null)
            {
                return;
            }
            e.Graphics.DrawImageUnscaled(Image, AutoScrollPosition.X, AutoScrollPosition.Y);
            if (Selection.Width > 0)
            {
                var r = new Rectangle(
                    (int)(Selection.X / TileScale) + AutoScrollPosition.X,
                    (int)(Selection.Y / TileScale) + AutoScrollPosition.Y,
                    Math.Max(3, (int)(Selection.Width / TileScale)),
                    Math.Max(3, (int)(Selection.Height / TileScale)));
                using (var pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, r);
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (Image == null)
            {
                return;
            }
            int x = (int)((e.X - AutoScrollPosition.X) * TileScale);
            int y = (int)((e.Y - AutoScrollPosition.Y) * TileScale);
            TileClicked?.Invoke(x, y);
        }

        public void ScrollTo(int tileX, int tileY)
        {
            if (Image == null)
            {
                return;
            }
            int px = (int)(tileX / TileScale) - (ClientSize.Width / 2);
            int py = (int)(tileY / TileScale) - (ClientSize.Height / 2);
            AutoScrollPosition = new Point(Math.Max(0, px), Math.Max(0, py));
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Image?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

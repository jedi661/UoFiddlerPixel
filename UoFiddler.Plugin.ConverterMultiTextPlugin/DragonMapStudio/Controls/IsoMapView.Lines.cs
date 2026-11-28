/***************************************************************************
 *
 *  * Dragon Map Studio – line system of the view: coordinate lines every N tiles (8 = one map block) with their
 *  *  numbers, and a small info box at the mouse (tile X / Y, land height, the statics standing there).
 *  *  So one always knows on which grid field a static is.
 *
 ***************************************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls
{
    public partial class IsoMapView
    {
        /// <summary>Coordinate lines every N tiles (0 = off, 8 = map blocks, 16, 32, 64).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CoordLines { get; set; }

        /// <summary>Info box at the mouse: tile, land height, statics on the tile.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool TileInfo { get; set; }

        /// <summary>Added to the shown coordinates (a region copy shows the coordinates of the big map).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point CoordOffset { get; set; }

        private void DrawCoordLines(Graphics g)
        {
            if (_map == null || (CoordLines <= 0 && !TileInfo))
            {
                return;
            }
            Point off = CoordOffset;
            if (CoordLines > 0)
            {
                int step = Math.Max(1, CoordLines);
                RectangleF v = ViewRect;
                int x0 = Math.Max(0, (int)Math.Floor(v.Left) - step), x1 = Math.Min(_map.Width, (int)Math.Ceiling(v.Right) + step);
                int y0 = Math.Max(0, (int)Math.Floor(v.Top) - step), y1 = Math.Min(_map.Height, (int)Math.Ceiling(v.Bottom) + step);
                // lines on the coordinates of the big map (a copy may start anywhere)
                int fx = x0 + ((step - (x0 + off.X) % step) % step), fy = y0 + ((step - (y0 + off.Y) % step) % step);
                if ((x1 - fx) / step + (y1 - fy) / step > 400)
                {
                    return; // too many lines at this zoom
                }
                using var pen = new Pen(Color.FromArgb(150, 255, 255, 255), 1f);
                using var strong = new Pen(Color.FromArgb(200, 250, 204, 21), 1.5f);
                using var font = new Font("Segoe UI", 8f, FontStyle.Bold);
                using var shadow = new SolidBrush(Color.FromArgb(170, 0, 0, 0));
                using var text = new SolidBrush(Color.FromArgb(250, 204, 21));
                for (int x = fx; x <= x1; x += step)
                {
                    bool big = (x + off.X) % (step * 8) == 0;
                    g.DrawLine(big ? strong : pen, Corner(x, y0), Corner(x, y1));
                    PointF p = Corner(x, Math.Clamp((int)Math.Round(_camY), y0, y1));
                    string s = "x " + (x + off.X);
                    SizeF sz = g.MeasureString(s, font);
                    g.FillRectangle(shadow, p.X - sz.Width / 2, p.Y - sz.Height - 2, sz.Width, sz.Height);
                    g.DrawString(s, font, text, p.X - sz.Width / 2, p.Y - sz.Height - 2);
                }
                for (int y = fy; y <= y1; y += step)
                {
                    bool big = (y + off.Y) % (step * 8) == 0;
                    g.DrawLine(big ? strong : pen, Corner(x0, y), Corner(x1, y));
                    PointF p = Corner(Math.Clamp((int)Math.Round(_camX), x0, x1), y);
                    string s = "y " + (y + off.Y);
                    SizeF sz = g.MeasureString(s, font);
                    g.FillRectangle(shadow, p.X - sz.Width / 2, p.Y + 2, sz.Width, sz.Height);
                    g.DrawString(s, font, text, p.X - sz.Width / 2, p.Y + 2);
                }
            }
            if (TileInfo && _map.InBounds(_hover.X, _hover.Y))
            {
                var items = _map.StaticsAt(_hover.X, _hover.Y).OrderByDescending(s => s.Z).Take(4).ToList();
                int count = _map.StaticsAt(_hover.X, _hover.Y).Count();
                string info = $"({_hover.X + off.X}, {_hover.Y + off.Y})  " + L.T("Boden") + $" z {_map.LandZ(_hover.X, _hover.Y)}  0x{_map.LandId(_hover.X, _hover.Y):X4}";
                if (count > 0)
                {
                    info += Environment.NewLine + $"{count} Statics: " + string.Join("  ", items.Select(s => $"{s.Id:X4}@{s.Z}")) + (count > items.Count ? " …" : string.Empty);
                }
                using var font = new Font("Segoe UI", 8.5f);
                SizeF sz = g.MeasureString(info, font);
                PointF at = Corner(_hover.X + 1, _hover.Y + 1);
                float bx = Math.Min(at.X + 14, Width - sz.Width - 6), by = Math.Min(at.Y + 6, Height - sz.Height - 6);
                using var back = new SolidBrush(Color.FromArgb(215, 15, 23, 42));
                using var fore = new SolidBrush(Color.White);
                using var edge = new Pen(Color.FromArgb(250, 204, 21));
                g.FillRectangle(back, bx - 4, by - 2, sz.Width + 8, sz.Height + 4);
                g.DrawRectangle(edge, bx - 4, by - 2, sz.Width + 8, sz.Height + 4);
                g.DrawString(info, font, fore, bx, by);
                using var hl = new Pen(Color.FromArgb(250, 204, 21), 2f);
                g.DrawPolygon(hl, LandPoly(_hover.X, _hover.Y));
            }
        }
    }
}

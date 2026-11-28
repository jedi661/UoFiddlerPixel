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
            if (_map == null || (CoordLines <= 0 && !TileInfo && !HeightScale))
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
                using var hl = new Pen(Color.FromArgb(250, 204, 21), 2f);
                g.DrawPolygon(hl, LandPoly(_hover.X, _hover.Y));
                // the box in screen pixels (same size at every zoom)
                PointF at = Corner(_hover.X + 1, _hover.Y + 1);
                System.Drawing.Drawing2D.Matrix keep = g.Transform;
                g.ResetTransform();
                using var font = new Font("Segoe UI", 8.5f);
                SizeF sz = g.MeasureString(info, font);
                float bx = Math.Min(at.X * (float)_zoom + 14, Width - sz.Width - 6), by = Math.Min(at.Y * (float)_zoom + 6, Height - sz.Height - 6);
                using var back = new SolidBrush(Color.FromArgb(215, 15, 23, 42));
                using var fore = new SolidBrush(Color.White);
                using var edge = new Pen(Color.FromArgb(250, 204, 21));
                g.FillRectangle(back, bx - 4, by - 2, sz.Width + 8, sz.Height + 4);
                g.DrawRectangle(edge, bx - 4, by - 2, sz.Width + 8, sz.Height + 4);
                g.DrawString(info, font, fore, bx, by);
                g.Transform = keep;
            }
            DrawHeightScale(g);
        }

        /// <summary>Height scale at the right border (for the tile under the mouse).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HeightScale { get; set; }

        /// <summary>
        /// "Oberfläche aus": land only as outlines and the statics on / above the ground hidden – what lies below the map
        /// (cellars, sewers, caves) can be seen and built. The map itself is not changed.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SurfaceHidden
        {
            get => _surfaceHidden;
            set
            {
                _surfaceHidden = value;
                Refresh(true);
            }
        }

        private bool _surfaceHidden;

        /// <summary>
        /// Height scale: a ruler at the right border, its Z lined up with the tile under the mouse – ground (green), build
        /// level (orange), cellar level (purple, under the map), and every static on the tile with its height (blue bars).
        /// </summary>
        private void DrawHeightScale(Graphics g)
        {
            if (!HeightScale || _map == null || _topDown)
            {
                return;
            }
            Point t = _map.InBounds(_hover.X, _hover.Y) ? _hover : CameraTile;
            if (!_map.InBounds(t.X, t.Y))
            {
                return;
            }
            float zoom = (float)_zoom;
            PointF top = TileTop(t.X, t.Y);
            float baseY = (top.Y + Half) * zoom;
            float Y(int z) => baseY - z * ZStep * zoom;
            System.Drawing.Drawing2D.Matrix keep = g.Transform;
            g.ResetTransform();
            float rx = Width - 52;
            using var back = new SolidBrush(Color.FromArgb(200, 15, 23, 42));
            g.FillRectangle(back, rx - 6, 0, 58, Height);
            using var axis = new Pen(Color.FromArgb(200, 226, 232, 240), 1.5f);
            g.DrawLine(axis, rx, 0, rx, Height);
            float perZ = ZStep * zoom;
            int label = perZ >= 2.5f ? 10 : perZ >= 1.2f ? 20 : 40;
            using var font = new Font("Segoe UI", 7.5f);
            using var fore = new SolidBrush(Color.FromArgb(226, 232, 240));
            for (int z = -128; z <= 127; z++)
            {
                float y = Y(z);
                if (y < 0 || y > Height || z % 5 != 0)
                {
                    continue;
                }
                bool big = z % label == 0;
                g.DrawLine(axis, rx, y, rx + (big ? 10 : 5), y);
                if (big)
                {
                    g.DrawString(z.ToString(), font, fore, rx + 12, y - 7);
                }
            }
            void Mark(int z, Color c, string text, float width = 2f)
            {
                float y = Y(z);
                if (y < 0 || y > Height)
                {
                    return;
                }
                using var p = new Pen(c, width);
                g.DrawLine(p, rx - 6, y, rx + 46, y);
                using var b = new SolidBrush(c);
                SizeF sz = g.MeasureString(text, font);
                g.FillRectangle(back, rx - 8 - sz.Width, y - sz.Height / 2, sz.Width + 2, sz.Height);
                g.DrawString(text, font, b, rx - 7 - sz.Width, y - sz.Height / 2);
            }
            // statics on the tile: bar from z to z + height
            using (var bar = new SolidBrush(Color.FromArgb(160, 56, 189, 248)))
            {
                int k = 0;
                foreach (StaticItem s in _map.StaticsAt(t.X, t.Y).OrderBy(s => s.Z).Take(12))
                {
                    int h = Math.Max(1, UoArtCache.ItemHeight(s.Id));
                    float y0 = Y(s.Z), y1 = Y(s.Z + h);
                    g.FillRectangle(bar, rx + 34 + (k % 3) * 5, Math.Min(y0, y1), 4, Math.Max(2, Math.Abs(y0 - y1)));
                    k++;
                }
            }
            int ground = _map.LandZ(t.X, t.Y);
            Mark(ground, Color.FromArgb(74, 222, 128), L.T("Boden") + $" {ground}");
            int build = PlaceZ(t.X, t.Y);
            if (build != ground)
            {
                Mark(build, Color.FromArgb(251, 146, 60), L.T("Bau") + $" {build}");
            }
            if (UnderView)
            {
                int cellar = Math.Clamp(ground + BrushSettings.UnderOffset, -128, 127);
                Mark(cellar, Color.FromArgb(192, 132, 252), L.T("Keller") + $" {cellar}", 1.5f);
            }
            // line from the tile to the scale
            using (var guide = new Pen(Color.FromArgb(120, 74, 222, 128), 1f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
            {
                g.DrawLine(guide, top.X * zoom, Y(ground), rx - 6, Y(ground));
            }
            g.Transform = keep;
        }
    }
}

/***************************************************************************
 *
 *  * Dragon Map Studio – cellar level ("Keller-Ebene"): a flat building level below the map at a fixed height.
 *  *  While it is on, the mouse hits the tiles of THIS level (not the surface), "Static setzen", walls, stairs,
 *  *  multis and motifs are placed on it, everything below the ground is drawn on top, and a grid of the level shows
 *  *  where one builds. The surface textures can be switched off on their own ("Texturen"). Typical: entrance on the
 *  *  map (Höhleneingang / NoDraw 0002 = walkable), then the cave itself on the level below.
 *
 ***************************************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls
{
    public partial class IsoMapView
    {
        private bool _layerMode;
        private int _layerZ = -40;

        /// <summary>Building on the flat cellar level <see cref="LayerZ"/> (mouse, placing, grid).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool LayerMode
        {
            get => _layerMode;
            set
            {
                _layerMode = value;
                Refresh(true);
            }
        }

        /// <summary>Absolute height of the cellar level.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int LayerZ
        {
            get => _layerZ;
            set
            {
                _layerZ = Math.Clamp(value, -128, 127);
                Invalidate();
            }
        }

        /// <summary>Grid of the cellar level (thin lines on the level).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool LayerGrid { get; set; } = true;

        /// <summary>Ground for multis / templates: the cellar level while it is on, else the land.</summary>
        private int GroundAt(int x, int y) => _layerMode ? _layerZ : _map.LandZ(x, y);

        /// <summary>Statics below the ground are drawn on top (under-map view or cellar level).</summary>
        private bool ShowBelow => UnderView || _layerMode;

        /// <summary>Height for placing on the cellar level: the level, or on top of the cave statics there (stacking).</summary>
        private int LayerPlaceZ(int x, int y)
        {
            int z = _layerZ;
            if (StackOnStatics && _map.InBounds(x, y))
            {
                int ground = _map.LandZ(x, y);
                foreach (StaticItem s in _map.StaticsAt(x, y).Where(s => s.Z >= _layerZ - 2 && s.Z < ground))
                {
                    z = Math.Max(z, s.Z + Math.Max(0, UoArtCache.StackHeight(s.Id)));
                }
            }
            return Math.Clamp(z + ZOffset, -128, 127);
        }

        /// <summary>Scene point of the corner (x, y) on the cellar level.</summary>
        private PointF LayerCorner(int x, int y)
        {
            PointF t = TileTop(x, y);
            return new PointF(t.X, t.Y - _layerZ * ZStep);
        }

        /// <summary>The grid of the level and the tile under the mouse on it.</summary>
        private void DrawLayerPlane(Graphics g)
        {
            if (!_layerMode || _map == null || _topDown)
            {
                return;
            }
            if (LayerGrid)
            {
                RectangleF v = ViewRect;
                int x0 = Math.Max(0, (int)v.Left - 4), x1 = Math.Min(_map.Width, (int)v.Right + 4);
                int y0 = Math.Max(0, (int)v.Top - 4), y1 = Math.Min(_map.Height, (int)v.Bottom + 4);
                if ((x1 - x0) + (y1 - y0) < 700)
                {
                    using var pen = new Pen(Color.FromArgb(80, 34, 211, 238), 1f);
                    for (int x = x0; x <= x1; x++)
                    {
                        g.DrawLine(pen, LayerCorner(x, y0), LayerCorner(x, y1));
                    }
                    for (int y = y0; y <= y1; y++)
                    {
                        g.DrawLine(pen, LayerCorner(x0, y), LayerCorner(x1, y));
                    }
                }
            }
            if (_map.InBounds(_hover.X, _hover.Y))
            {
                PointF[] poly = { LayerCorner(_hover.X, _hover.Y), LayerCorner(_hover.X + 1, _hover.Y), LayerCorner(_hover.X + 1, _hover.Y + 1), LayerCorner(_hover.X, _hover.Y + 1) };
                using var fill = new SolidBrush(Color.FromArgb(70, 34, 211, 238));
                using var edge = new Pen(Color.FromArgb(230, 34, 211, 238), 2f);
                g.FillPolygon(fill, poly);
                g.DrawPolygon(edge, poly);
                using var font = new Font("Segoe UI", 8f, FontStyle.Bold);
                using var text = new SolidBrush(Color.FromArgb(34, 211, 238));
                g.DrawString(L.T("Keller-Ebene") + $" z {_layerZ}" + (LayerPlaceZ(_hover.X, _hover.Y) != _layerZ ? $" → {LayerPlaceZ(_hover.X, _hover.Y)}" : string.Empty), font, text, poly[1].X + 4, poly[1].Y - 6);
            }
        }
    }
}

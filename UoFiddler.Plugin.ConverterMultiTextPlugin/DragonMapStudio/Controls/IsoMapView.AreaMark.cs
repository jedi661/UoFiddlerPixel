/***************************************************************************
 *
 *  * Dragon Map Studio – marking large areas with the tool "Bereich":
 *  *    Strg + Klick      = corner 1, then scroll / jump anywhere, Strg + Klick = corner 2 (any size)
 *  *    Umschalt + ziehen = extend the marked area (the new rectangle is added)
 *  *    ziehen an den Rand = the view scrolls along while the area grows
 *  *    "Bereich per Koordinaten …" (dialog) = exact numbers
 *  *  Used in the CGI view, the foreign map window and the designers.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls
{
    public partial class IsoMapView
    {
        private const int EdgeZone = 28;
        private Point? _areaCorner;
        private bool _areaExtend;
        private Rectangle _areaBase = Rectangle.Empty;
        private Timer _edgeTimer;
        private Point _edgeMouse;

        private Point _lastTile = new Point(-1, -1);

        /// <summary>The last map tile the mouse was over (stays when the mouse leaves the view, e.g. to click a button).</summary>
        public Point LastTile => _lastTile;

        /// <summary>Asks for the area by numbers (dialog) and marks it.</summary>
        public void AskArea(IWin32Window owner)
        {
            if (_map == null)
            {
                return;
            }
            Rectangle? r = Dialogs.AreaDialog.Ask(owner, _area, new Size(_map.Width, _map.Height), _lastTile.X < 0 ? new Point((int)_camX, (int)_camY) : _lastTile);
            if (r != null)
            {
                Tool = IsoTool.Area;
                SetAreaAndShow(r.Value);
            }
        }

        /// <summary>Corner 1 of a two-corner area (Strg + Klick), null = none.</summary>
        public Point? AreaCorner => _areaCorner;

        /// <summary>Marks an area, shows it (jumps to its middle) and tells the listeners.</summary>
        public void SetAreaAndShow(Rectangle r, bool jump = true)
        {
            if (_map == null)
            {
                return;
            }
            r.Intersect(new Rectangle(0, 0, _map.Width, _map.Height));
            _area = r;
            _areaCorner = null;
            if (jump && !r.IsEmpty)
            {
                CenterOn(r.X + r.Width / 2.0, r.Y + r.Height / 2.0);
            }
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            Message?.Invoke(this, AreaText());
        }

        private string AreaText() => _area.IsEmpty
            ? L.T("Kein Bereich markiert.")
            : L.T("Bereich") + $" {_area.Width}×{_area.Height} " + L.T("von") + $" ({_area.X}, {_area.Y}) " + L.T("bis") + $" ({_area.Right - 1}, {_area.Bottom - 1})";

        /// <summary>Mouse down with the tool "Bereich": Strg = corners, Umschalt = extend. True = handled here.</summary>
        private bool AreaMouseDown(Point tile)
        {
            Keys mods = ModifierKeys;
            if ((mods & Keys.Control) != 0 && (mods & Keys.Shift) == 0)
            {
                if (!_map.InBounds(tile.X, tile.Y))
                {
                    return true;
                }
                if (_areaCorner == null)
                {
                    _areaCorner = tile;
                    Message?.Invoke(this, L.T("Ecke 1") + $" ({tile.X}, {tile.Y}) " + L.T("gesetzt – jetzt zur anderen Ecke scrollen oder springen (auch weit weg) und mit Strg + Klick Ecke 2 setzen · Esc = abbrechen"));
                }
                else
                {
                    Point a = _areaCorner.Value;
                    _areaCorner = null;
                    ClearMask();
                    _area = Rectangle.FromLTRB(Math.Min(a.X, tile.X), Math.Min(a.Y, tile.Y), Math.Max(a.X, tile.X) + 1, Math.Max(a.Y, tile.Y) + 1);
                    _area.Intersect(new Rectangle(0, 0, _map.Width, _map.Height));
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    Message?.Invoke(this, AreaText() + " – " + L.T("Umschalt + ziehen erweitert ihn"));
                }
                Invalidate();
                return true;
            }
            if ((mods & Keys.Shift) != 0 && !_area.IsEmpty)
            {
                _areaBase = _area;
                _areaExtend = true;
                _areaStart = tile;
                _areaDragging = true;
                _area = Rectangle.Union(_areaBase, new Rectangle(tile.X, tile.Y, 1, 1));
                _area.Intersect(new Rectangle(0, 0, _map.Width, _map.Height));
                Invalidate();
                return true;
            }
            _areaExtend = false;
            return false;
        }

        /// <summary>The area while dragging (with Umschalt: added to the area before).</summary>
        private Rectangle DragArea(Point t)
        {
            Rectangle r = Rectangle.FromLTRB(Math.Min(_areaStart.X, t.X), Math.Min(_areaStart.Y, t.Y), Math.Max(_areaStart.X, t.X) + 1, Math.Max(_areaStart.Y, t.Y) + 1);
            if (_areaExtend && !_areaBase.IsEmpty)
            {
                r = Rectangle.Union(_areaBase, r);
            }
            r.Intersect(new Rectangle(0, 0, _map.Width, _map.Height));
            return r;
        }

        /// <summary>While the area is dragged near the border of the view, the view scrolls along.</summary>
        private void AreaEdgeScroll(Point mouse)
        {
            _edgeMouse = mouse;
            bool near = mouse.X < EdgeZone || mouse.Y < EdgeZone || mouse.X > Width - EdgeZone || mouse.Y > Height - EdgeZone;
            if (!near || !_areaDragging)
            {
                _edgeTimer?.Stop();
                return;
            }
            if (_edgeTimer == null)
            {
                _edgeTimer = new Timer { Interval = 40 };
                _edgeTimer.Tick += (s, e) => EdgeTick();
            }
            if (!_edgeTimer.Enabled)
            {
                _edgeTimer.Start();
            }
        }

        private void EdgeTick()
        {
            if (!_areaDragging || _map == null || MouseButtons == MouseButtons.None)
            {
                _edgeTimer?.Stop();
                _areaDragging = _areaDragging && MouseButtons != MouseButtons.None;
                return;
            }
            Point m = PointToClient(Cursor.Position);
            _edgeMouse = m;
            double dx = m.X < EdgeZone ? -1 : m.X > Width - EdgeZone ? 1 : 0;
            double dy = m.Y < EdgeZone ? -1 : m.Y > Height - EdgeZone ? 1 : 0;
            if (dx == 0 && dy == 0)
            {
                _edgeTimer.Stop();
                return;
            }
            double px = dx * 18, py = dy * 18; // screen pixels per tick
            if (_topDown)
            {
                _camX = Math.Clamp(_camX + px / (Cell * _zoom), 0, _map.Width - 1);
                _camY = Math.Clamp(_camY + py / (Cell * _zoom), 0, _map.Height - 1);
            }
            else
            {
                double du = px / (Half * _zoom), dv = py / (Half * _zoom);
                _camX = Math.Clamp(_camX + (du + dv) / 2, 0, _map.Width - 1);
                _camY = Math.Clamp(_camY + (dv - du) / 2, 0, _map.Height - 1);
            }
            OnViewChanged();
            Point t = TileAt(m);
            _hover = t;
            _area = DragArea(t);
            Invalidate();
            Message?.Invoke(this, AreaText());
        }

        /// <summary>Corner 1 marker and the rectangle to the mouse (two-corner marking).</summary>
        private void DrawAreaCorner(Graphics g)
        {
            if (_areaCorner == null || _map == null)
            {
                return;
            }
            Point a = _areaCorner.Value;
            using var pen = new Pen(Color.Magenta, 3);
            g.DrawPolygon(pen, AreaPoly(new Rectangle(a.X, a.Y, 1, 1)));
            if (_map.InBounds(_hover.X, _hover.Y) && (Tool == IsoTool.Area))
            {
                using var dash = new Pen(Color.Magenta, 1.5f) { DashStyle = DashStyle.Dash };
                g.DrawPolygon(dash, AreaPoly(Rectangle.FromLTRB(Math.Min(a.X, _hover.X), Math.Min(a.Y, _hover.Y), Math.Max(a.X, _hover.X) + 1, Math.Max(a.Y, _hover.Y) + 1)));
            }
        }
    }
}

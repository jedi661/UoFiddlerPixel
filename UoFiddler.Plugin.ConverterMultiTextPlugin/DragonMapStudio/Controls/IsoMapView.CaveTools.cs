/***************************************************************************
 *
 *  * Dragon Map Studio – quick cave building in the view:
 *  *   Höhle:   while the mouse button is held, the mouse wheel moves the floor of this stroke up / down (±5, Umschalt ±1)
 *  *            – everything painted in the stroke follows at once.
 *  *   Eingang: one click = a whole entrance: "Standard" (the cave stamp, height + wheel) or a saved entrance
 *  *            (land + statics, anchored on the ground in front). Mouse wheel = next entrance (preview at the mouse),
 *  *            Umschalt + wheel = height, Strg + wheel = zoom, R = direction of the standard entrance.
 *  *            "Malen" (CaveEntranceClick off) = the old painting of the mouth.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls
{
    public partial class IsoMapView
    {
        /// <summary>Eingang: one click places a whole entrance (else: paint the mouth like before).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CaveEntranceClick { get; set; } = true;

        /// <summary>Eingang: 0 = standard (cave stamp), 1… = saved entrance number.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CaveEntranceIndex { get; set; }

        /// <summary>Eingang: height added to the mountain part (mouth, frame, arch) – mouse wheel with Umschalt.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CaveEntranceHeight { get; set; }

        /// <summary>The saved entrances (from the form).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<List<MapTemplate>> CaveEntrancePresets { get; set; }

        /// <summary>Entrance / height / cave floor changed by the mouse wheel (the window shows it).</summary>
        public event EventHandler CaveToolChanged;

        private List<MapTemplate> Presets() => CaveEntrancePresets?.Invoke() ?? new List<MapTemplate>();

        private string EntranceName()
        {
            List<MapTemplate> p = Presets();
            int i = Math.Clamp(CaveEntranceIndex, 0, p.Count);
            return i == 0 ? L.T("Standard-Eingang") + " (" + L.T(CaveStamp?.Facing switch { CaveFacing.South => "Süden", CaveFacing.East => "Osten", _ => "Richtung automatisch" }) + ")" : p[i - 1].Name;
        }

        /// <summary>Mouse wheel of the cave tools. True = used (no zoom).</summary>
        private bool CaveWheel(MouseEventArgs e)
        {
            int dir = e.Delta > 0 ? 1 : -1;
            Keys mods = ModifierKeys;
            if ((mods & Keys.Control) != 0)
            {
                return false; // Strg + wheel = zoom as always
            }
            if (Tool == IsoTool.Cave && _landStroke && (MouseButtons & MouseButtons.Left) != 0)
            {
                int step = (mods & Keys.Shift) != 0 ? 1 : 5;
                _strokeZ = Math.Clamp(_strokeZ + dir * step, -128, 127);
                // everything painted in this stroke follows the new floor height
                int x0 = int.MaxValue, y0 = int.MaxValue, x1 = int.MinValue, y1 = int.MinValue;
                foreach (int i in _strokeCells)
                {
                    int x = i % _map.Width, y = i / _map.Width;
                    _map.SetLand(x, y, _map.LandId(x, y), _strokeZ);
                    x0 = Math.Min(x0, x);
                    y0 = Math.Min(y0, y);
                    x1 = Math.Max(x1, x);
                    y1 = Math.Max(y1, y);
                }
                if (CaveRaiseWalls && x0 <= x1)
                {
                    var c = new Point((x0 + x1) / 2, (y0 + y1) / 2);
                    RaiseCaveWalls(c, Math.Max(x1 - x0, y1 - y0) / 2 + 2);
                }
                Refresh(true);
                Message?.Invoke(this, L.T("Höhlen-Boden") + $" z {_strokeZ} (" + L.T("Mausrad ±5, Umschalt ±1, Taste loslassen = fertig") + ")");
                CaveToolChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            if (Tool == IsoTool.CaveEntrance && CaveEntranceClick)
            {
                if ((mods & Keys.Shift) != 0)
                {
                    CaveEntranceHeight = Math.Clamp(CaveEntranceHeight + dir * 5, -60, 60);
                }
                else
                {
                    int n = Presets().Count + 1;
                    CaveEntranceIndex = ((CaveEntranceIndex - dir) % n + n) % n;
                }
                Message?.Invoke(this, L.T("Eingang") + $": {EntranceName()} · " + L.T("Höhe") + $" {CaveEntranceHeight:+0;-0;0} · " +
                                      L.T("Mausrad = nächster, Umschalt+Mausrad = Höhe, Strg+Mausrad = Zoom, R = Richtung"));
                Invalidate();
                CaveToolChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        /// <summary>R with the entrance tool: direction of the standard entrance (automatic → south → east).</summary>
        private bool CaveKey(KeyEventArgs e)
        {
            if (Tool == IsoTool.CaveEntrance && CaveEntranceClick && e.KeyCode == Keys.R && !e.Control)
            {
                CaveStamp ??= new CaveStampSettings();
                CaveStamp.Facing = CaveStamp.Facing switch { CaveFacing.Auto => CaveFacing.South, CaveFacing.South => CaveFacing.East, _ => CaveFacing.Auto };
                Message?.Invoke(this, L.T("Eingang") + $": {EntranceName()}");
                Invalidate();
                CaveToolChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        /// <summary>One click with "Eingang": the chosen entrance at the tile.</summary>
        private void PlaceEntrance(Point tile)
        {
            List<MapTemplate> presets = Presets();
            int i = Math.Clamp(CaveEntranceIndex, 0, presets.Count);
            if (i == 0)
            {
                CaveStampSettings s = JsonSerializer.Deserialize<CaveStampSettings>(JsonSerializer.Serialize(CaveStamp ?? new CaveStampSettings()));
                s.MouthHeight += CaveEntranceHeight;
                s.FrameHeight += CaveEntranceHeight;
                s.ArchHeight += CaveEntranceHeight;
                CaveStampSettings keep = CaveStamp;
                CaveStamp = s;
                try
                {
                    PlaceCaveStamp(tile.X, tile.Y, null);
                }
                finally
                {
                    CaveStamp = keep;
                }
                return;
            }
            MapTemplate t = presets[i - 1];
            int gz = _map.LandZ(tile.X, tile.Y);
            int h = CaveEntranceHeight;
            _map.BeginLand();
            if (t.HasLand)
            {
                for (int y = 0; y < t.LandHeight; y++)
                {
                    for (int x = 0; x < t.LandWidth; x++)
                    {
                        int k = y * t.LandWidth + x;
                        int rel = t.LandZ != null && k < t.LandZ.Length ? t.LandZ[k] : 0;
                        _map.SetLand(tile.X + t.LandX + x, tile.Y + t.LandY + y, t.LandTiles[k], gz + rel + (rel > 0 ? h : 0));
                    }
                }
            }
            var statics = t.Statics.Select(s => new StaticItem { Id = s.Id, Hue = s.Hue, X = tile.X + s.X, Y = tile.Y + s.Y, Z = Math.Clamp(gz + s.Z + (s.Z > 0 ? h : 0), -128, 127) })
                           .Where(s => _map.InBounds(s.X, s.Y)).Select(MapData.Add).ToList();
            _map.ApplyStatics(statics);
            int n = _map.EndLand();
            Refresh(true);
            if (n > 0)
            {
                MapChanged?.Invoke(this, EventArgs.Empty);
            }
            Message?.Invoke(this, L.T("Eingang gesetzt") + $": '{t.Name}' " + L.T("bei") + $" ({tile.X}, {tile.Y}), " + L.T("Höhe") + $" {h:+0;-0;0} – Strg+Z " + L.T("nimmt ihn zurück") + ".");
        }

        /// <summary>
        /// The marked area as an entrance: land with heights and statics, anchored on the lowest tile at the front
        /// (the ground in front of the mouth, where one clicks later).
        /// </summary>
        public MapTemplate CaptureEntrance()
        {
            if (_map == null || _area.IsEmpty)
            {
                return null;
            }
            Point anchor = new Point(_area.X, _area.Bottom - 1);
            int best = int.MaxValue;
            for (int y = _area.Top; y < _area.Bottom; y++)
            {
                for (int x = _area.Left; x < _area.Right; x++)
                {
                    // lowest ground, at the same height the one nearest to the camera (south-east)
                    int score = _map.LandZ(x, y) * 1000 - (x + y);
                    if (score < best)
                    {
                        best = score;
                        anchor = new Point(x, y);
                    }
                }
            }
            int gz = _map.LandZ(anchor.X, anchor.Y);
            var t = new MapTemplate
            {
                LandX = _area.X - anchor.X, LandY = _area.Y - anchor.Y, LandWidth = _area.Width, LandHeight = _area.Height,
                LandTiles = new ushort[_area.Width * _area.Height], LandZ = new int[_area.Width * _area.Height],
                OriginX = anchor.X, OriginY = anchor.Y, OriginZ = gz, Category = "Höhleneingang"
            };
            for (int y = 0; y < _area.Height; y++)
            {
                for (int x = 0; x < _area.Width; x++)
                {
                    t.LandTiles[y * _area.Width + x] = _map.LandId(_area.X + x, _area.Y + y);
                    t.LandZ[y * _area.Width + x] = _map.LandZ(_area.X + x, _area.Y + y) - gz;
                    foreach (StaticItem s in _map.StaticsAt(_area.X + x, _area.Y + y))
                    {
                        t.Statics.Add(new TemplateStatic { Id = s.Id, Hue = s.Hue, X = s.X - anchor.X, Y = s.Y - anchor.Y, Z = s.Z - gz });
                    }
                }
            }
            return t;
        }

        private PointF CornerAtZ(int x, int y, int z)
        {
            PointF t = TileTop(x, y);
            return new PointF(t.X, t.Y - z * ZStep);
        }

        /// <summary>Preview of the entrance tool at the mouse (outline of the land at its new heights).</summary>
        private void DrawEntranceGhost(Graphics g)
        {
            if (Tool != IsoTool.CaveEntrance || !CaveEntranceClick || _map == null || !_map.InBounds(_hover.X, _hover.Y) || _topDown)
            {
                return;
            }
            List<MapTemplate> presets = Presets();
            int i = Math.Clamp(CaveEntranceIndex, 0, presets.Count);
            int gz = _map.LandZ(_hover.X, _hover.Y);
            using var font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            using var text = new SolidBrush(Color.FromArgb(251, 191, 36));
            PointF at = CornerAtZ(_hover.X + 1, _hover.Y, gz);
            if (i == 0)
            {
                int w = Math.Max(1, (CaveStamp?.Width ?? 3)) + 2;
                using var pen = new Pen(Color.FromArgb(230, 251, 191, 36), 2f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
                g.DrawPolygon(pen, LandPoly(_hover.X, _hover.Y));
                g.DrawString(EntranceName() + $" · {w}× · " + L.T("Höhe") + $" {CaveEntranceHeight:+0;-0;0}", font, text, at.X + 6, at.Y - 18);
                return;
            }
            MapTemplate t = presets[i - 1];
            if (t.HasLand)
            {
                using var fill = new SolidBrush(Color.FromArgb(70, 251, 191, 36));
                using var edge = new Pen(Color.FromArgb(200, 251, 191, 36), 1f);
                for (int y = 0; y < t.LandHeight; y++)
                {
                    for (int x = 0; x < t.LandWidth; x++)
                    {
                        int k = y * t.LandWidth + x;
                        int rel = t.LandZ != null && k < t.LandZ.Length ? t.LandZ[k] : 0;
                        int z = gz + rel + (rel > 0 ? CaveEntranceHeight : 0);
                        int mx = _hover.X + t.LandX + x, my = _hover.Y + t.LandY + y;
                        PointF[] poly = { CornerAtZ(mx, my, z), CornerAtZ(mx + 1, my, z), CornerAtZ(mx + 1, my + 1, z), CornerAtZ(mx, my + 1, z) };
                        if (CaveEntranceBuilder.IsEntranceTile(t.LandTiles[k], 0x01DB))
                        {
                            g.FillPolygon(fill, poly);
                        }
                        g.DrawPolygon(edge, poly);
                    }
                }
            }
            g.DrawString($"{t.Name} · " + L.T("Höhe") + $" {CaveEntranceHeight:+0;-0;0} · " + L.T("Mausrad = nächster"), font, text, at.X + 6, at.Y - 18);
        }
    }
}

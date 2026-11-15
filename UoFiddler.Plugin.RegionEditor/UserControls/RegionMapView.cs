/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Map view of the region editor: shows a map of the MapRegistry (like the
 * MapControl, rendered with Map.GetImage) and the regions on top of it.
 *
 * Tools: select / move / resize, pan, rectangle, polygon, circle, fill,
 * go location, entrance. Mouse wheel = zoom, middle mouse button = pan.
 *
 * The control can be placed in the Visual Studio designer (toolbox).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.RegionEditor.Classes;

namespace UoFiddler.Plugin.RegionEditor.UserControls
{
    public enum MapTool
    {
        Select,
        Pan,
        Rect,
        Polygon,
        Circle,
        Fill,
        Go,
        Entrance
    }

    public sealed class ShapeEventArgs : EventArgs
    {
        public MapTool Tool { get; set; }
        public Rectangle Rect { get; set; }
        public List<Point> Polygon { get; set; }
        public Point Cell { get; set; }
        public int Radius { get; set; }

        /// <summary>Shift was held: add to the selected region instead of creating a new one</summary>
        public bool AddToSelected { get; set; }

        /// <summary>Alt was held: cut the shape out of the selected region</summary>
        public bool Subtract { get; set; }
    }

    public sealed class RegionPickEventArgs : EventArgs
    {
        public RegionNode Region { get; set; }
        public int RectIndex { get; set; } = -1;
        public Point Cell { get; set; }
    }

    [ToolboxItem(true)]
    [DesignerCategory("Code")]
    public class RegionMapView : Control, IThemed
    {
        private const int HandleSize = 7;
        private static readonly double[] _zoomLevels = { 0.25, 0.5, 0.75, 1, 1.5, 2, 3, 4, 6, 8, 12, 16 };

        private MapDefinition _def;
        private Map _map;
        private double _zoom = 1;
        private double _viewX;
        private double _viewY;

        // rendered map part (re-rendered only when the visible blocks change)
        private Bitmap _mapBitmap;
        private Rectangle _mapBlocks;
        private bool _mapStatics;
        private string _renderError;

        // minimap, built row by row in the background of the UI thread
        private Bitmap _minimap;
        private int _minimapRow;
        private readonly Timer _minimapTimer;

        // mouse
        private Point _mouseDown;
        private Point _downCell;
        private Point _lastCell;
        private bool _panning;
        private double _panStartX;
        private double _panStartY;
        private bool _drawing;
        private readonly List<Point> _polygon = new List<Point>();

        // editing of the selected region
        private enum DragKind
        {
            None,
            MoveRect,
            MoveRegion,
            Handle,
            Vertex,
            GoPoint,
            EntrancePoint
        }

        private DragKind _drag;
        private int _dragRect = -1;
        private int _dragHandle = -1;
        private int _dragPoly = -1;
        private int _dragVertex = -1;
        private Rectangle _dragStartRect;
        private bool _dragChanged;
        private List<(RegionRect Rect, Rectangle Start)> _dragRegionStart;
        private List<List<Point>> _dragPolyStart;

        private ThemePalette _palette = ThemePalette.Light;
        private readonly Font _labelFont = new Font("Segoe UI", 8.25f, FontStyle.Bold);
        private readonly Font _infoFont = new Font("Segoe UI", 9f);

        public RegionMapView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.FromArgb(40, 44, 52);
            _minimapTimer = new Timer { Interval = 30 };
            _minimapTimer.Tick += OnMinimapTick;
        }

        #region Events

        /// <summary>a shape was drawn (rect, polygon, circle, fill)</summary>
        public event EventHandler<ShapeEventArgs> ShapeCreated;

        /// <summary>a point was picked with the go/entrance tool</summary>
        public event EventHandler<ShapeEventArgs> PointPicked;

        /// <summary>a region was clicked with the select tool (Region = null: empty place)</summary>
        public event EventHandler<RegionPickEventArgs> RegionPicked;

        /// <summary>raised BEFORE the selected region is changed by dragging (for undo)</summary>
        public event EventHandler EditStarting;

        /// <summary>raised after a drag changed the selected region</summary>
        public event EventHandler EditFinished;

        /// <summary>the cell under the mouse changed</summary>
        public event EventHandler<RegionPickEventArgs> CursorMoved;

        /// <summary>zoom or position changed</summary>
        public event EventHandler ViewChanged;

        #endregion

        #region Properties

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MapDefinition MapDefinition => _def;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MapId => _def?.Id ?? -1;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Size MapSize => _def == null ? Size.Empty : new Size(_def.Width, _def.Height);

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FacetRegions Facet { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RegionNode SelectedRegion { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedRectIndex { get; set; } = -1;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RegionNode HoverRegion { get; private set; }

        [Category("Region editor")]
        [DefaultValue(MapTool.Select)]
        public MapTool Tool
        {
            get => _tool;
            set
            {
                _tool = value;
                _polygon.Clear();
                _drawing = false;
                Cursor = value == MapTool.Pan ? Cursors.SizeAll : value == MapTool.Select ? Cursors.Default : Cursors.Cross;
                Invalidate();
            }
        }

        private MapTool _tool = MapTool.Select;

        [Category("Region editor")]
        [DefaultValue(1)]
        public int Snap { get; set; } = 1;

        [Category("Region editor")]
        [DefaultValue(true)]
        public bool ShowStatics
        {
            get => _showStatics;
            set
            {
                _showStatics = value;
                Invalidate();
            }
        }

        private bool _showStatics = true;

        [Category("Region editor")]
        [DefaultValue(false)]
        public bool ShowGrid { get; set; }

        [Category("Region editor")]
        [DefaultValue(true)]
        public bool ShowLabels { get; set; } = true;

        [Category("Region editor")]
        [DefaultValue(true)]
        public bool ShowMinimap { get; set; } = true;

        [Category("Region editor")]
        [DefaultValue(70)]
        public int FillAlpha { get; set; } = 70;

        /// <summary>position of the client (x, y, map id), null = not shown</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public (int X, int Y, int Map)? ClientPosition { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double Zoom => _zoom;

        /// <summary>cell under the mouse (also the cell of the last right click)</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point CursorCell => _lastCell;

        /// <summary>visible area in map cells</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle VisibleCells => new Rectangle((int)_viewX, (int)_viewY, (int)Math.Ceiling(Width / _zoom), (int)Math.Ceiling(Height / _zoom));

        #endregion

        #region Map

        public void SetMap(MapDefinition def)
        {
            bool changed = _def == null || def == null || _def.Id != def.Id;
            _def = def;
            _map = def?.GetMap();
            DropMapBitmap();
            if (changed)
            {
                _minimap?.Dispose();
                _minimap = null;
                _minimapRow = 0;
                if (_map != null && ShowMinimap && !DesignMode)
                {
                    _minimapTimer.Start();
                }
            }

            ClampView();
            Invalidate();
        }

        /// <summary>map files changed (e.g. after a reload): render again</summary>
        public void RefreshMap()
        {
            _map?.ResetCache();
            DropMapBitmap();
            _minimap?.Dispose();
            _minimap = null;
            _minimapRow = 0;
            if (_map != null && ShowMinimap)
            {
                _minimapTimer.Start();
            }

            Invalidate();
        }

        private void DropMapBitmap()
        {
            _mapBitmap?.Dispose();
            _mapBitmap = null;
            _renderError = null;
        }

        public void CenterOn(int x, int y)
        {
            _viewX = x - (Width / _zoom / 2);
            _viewY = y - (Height / _zoom / 2);
            ClampView();
            Invalidate();
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>zooms so the rectangle fills the view</summary>
        public void ZoomTo(Rectangle cells)
        {
            if (cells.Width <= 0 || cells.Height <= 0 || Width <= 0 || Height <= 0)
            {
                return;
            }

            double wanted = Math.Min(Width / (cells.Width * 1.3), Height / (cells.Height * 1.3));
            _zoom = _zoomLevels.LastOrDefault(z => z <= wanted);
            if (_zoom <= 0)
            {
                _zoom = _zoomLevels[0];
            }

            CenterOn(cells.X + (cells.Width / 2), cells.Y + (cells.Height / 2));
        }

        public void ZoomIn() => ZoomStep(1, new Point(Width / 2, Height / 2));

        public void ZoomOut() => ZoomStep(-1, new Point(Width / 2, Height / 2));

        public void SetZoom(double zoom)
        {
            double cx = _viewX + (Width / _zoom / 2), cy = _viewY + (Height / _zoom / 2);
            _zoom = Math.Max(_zoomLevels[0], Math.Min(_zoomLevels[_zoomLevels.Length - 1], zoom));
            CenterOn((int)cx, (int)cy);
        }

        private void ZoomStep(int dir, Point anchor)
        {
            int i = Array.IndexOf(_zoomLevels, _zoom);
            if (i < 0)
            {
                i = Array.FindLastIndex(_zoomLevels, z => z <= _zoom);
            }

            int n = Math.Max(0, Math.Min(_zoomLevels.Length - 1, i + dir));
            if (_zoomLevels[n] == _zoom)
            {
                return;
            }

            double mx = _viewX + (anchor.X / _zoom), my = _viewY + (anchor.Y / _zoom);
            _zoom = _zoomLevels[n];
            _viewX = mx - (anchor.X / _zoom);
            _viewY = my - (anchor.Y / _zoom);
            ClampView();
            Invalidate();
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ClampView()
        {
            if (_def == null)
            {
                return;
            }

            double w = Width / _zoom, h = Height / _zoom;
            _viewX = Math.Max(-w / 2, Math.Min(_def.Width - (w / 2), _viewX));
            _viewY = Math.Max(-h / 2, Math.Min(_def.Height - (h / 2), _viewY));
        }

        public Point CellAt(Point screen) =>
            new Point((int)Math.Floor(_viewX + (screen.X / _zoom)), (int)Math.Floor(_viewY + (screen.Y / _zoom)));

        public PointF ScreenOf(double x, double y) => new PointF((float)((x - _viewX) * _zoom), (float)((y - _viewY) * _zoom));

        public RectangleF ScreenOf(Rectangle cells)
        {
            PointF a = ScreenOf(cells.X, cells.Y);
            return new RectangleF(a.X, a.Y, (float)(cells.Width * _zoom), (float)(cells.Height * _zoom));
        }

        #endregion

        #region Painting

        public void ApplyTheme(ThemePalette palette)
        {
            _palette = palette;
            BackColor = palette.MapBack;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (DesignMode || _def == null)
            {
                using (var brush = new SolidBrush(Color.FromArgb(160, 255, 255, 255)))
                {
                    g.DrawString(DesignMode ? "RegionMapView" : Loc.T("map.none"), _infoFont, brush, 12, 12);
                }

                return;
            }

            DrawMap(g);
            if (ShowGrid)
            {
                DrawGrid(g);
            }

            g.SmoothingMode = SmoothingMode.None;
            DrawRegions(g);
            DrawPreview(g);
            DrawClient(g);
            if (ShowMinimap)
            {
                DrawMinimap(g);
            }

            if (_renderError != null)
            {
                g.DrawString(_renderError, _infoFont, Brushes.OrangeRed, 12, 12);
            }
        }

        private void DrawMap(Graphics g)
        {
            if (_map == null)
            {
                return;
            }

            int mapBlocksW = _def.Width >> 3, mapBlocksH = _def.Height >> 3;
            int bx = Math.Max(0, (int)Math.Floor(_viewX) >> 3);
            int by = Math.Max(0, (int)Math.Floor(_viewY) >> 3);
            int bw = Math.Min(mapBlocksW - bx, ((int)(Width / _zoom) >> 3) + 3);
            int bh = Math.Min(mapBlocksH - by, ((int)(Height / _zoom) >> 3) + 3);
            if (bw <= 0 || bh <= 0)
            {
                return;
            }

            var blocks = new Rectangle(bx, by, bw, bh);
            if (_mapBitmap == null || blocks != _mapBlocks || _mapStatics != ShowStatics)
            {
                if (_mapBitmap == null || _mapBitmap.Width != bw << 3 || _mapBitmap.Height != bh << 3)
                {
                    _mapBitmap?.Dispose();
                    _mapBitmap = new Bitmap(bw << 3, bh << 3, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
                }

                try
                {
                    _map.GetImage(bx, by, bw, bh, _mapBitmap, ShowStatics);
                    _renderError = null;
                }
                catch (Exception ex)
                {
                    _renderError = Loc.F("map.error", _def.Name, ex.Message);
                    _map.ResetCache();
                }

                _mapBlocks = blocks;
                _mapStatics = ShowStatics;
            }

            g.InterpolationMode = _zoom < 1 ? InterpolationMode.HighQualityBilinear : InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            RectangleF dest = ScreenOf(new Rectangle(bx << 3, by << 3, bw << 3, bh << 3));
            g.DrawImage(_mapBitmap, dest, new RectangleF(0, 0, bw << 3, bh << 3), GraphicsUnit.Pixel);
            g.PixelOffsetMode = PixelOffsetMode.Default;
            g.InterpolationMode = InterpolationMode.Default;
        }

        private void DrawGrid(Graphics g)
        {
            int step = _zoom >= 8 ? 1 : _zoom >= 1 ? 8 : 64;
            Rectangle v = VisibleCells;
            using (var pen = new Pen(Color.FromArgb(50, 255, 255, 255)))
            using (var strong = new Pen(Color.FromArgb(90, 255, 255, 255)))
            {
                for (int x = v.Left - (v.Left % step); x <= v.Right; x += step)
                {
                    float sx = ScreenOf(x, 0).X;
                    g.DrawLine(x % (step * 8) == 0 ? strong : pen, sx, 0, sx, Height);
                }

                for (int y = v.Top - (v.Top % step); y <= v.Bottom; y += step)
                {
                    float sy = ScreenOf(0, y).Y;
                    g.DrawLine(y % (step * 8) == 0 ? strong : pen, 0, sy, Width, sy);
                }
            }
        }

        public static Color RegionColor(RegionNode r) => r.Color.HasValue ? Color.FromArgb(255, Color.FromArgb(r.Color.Value)) : RegionCatalog.TypeColor(r.Type);

        private void DrawRegions(Graphics g)
        {
            if (Facet == null)
            {
                return;
            }

            Rectangle visible = VisibleCells;
            visible.Inflate(2, 2);
            var labels = new List<(RegionNode Region, RectangleF Box)>();

            foreach (RegionNode region in Facet.AllRegions())
            {
                if (region.Hidden || region == SelectedRegion)
                {
                    continue;
                }

                DrawRegion(g, region, visible, false, labels);
            }

            if (SelectedRegion != null && !SelectedRegion.Hidden)
            {
                DrawRegion(g, SelectedRegion, visible, true, labels);
            }

            if (ShowLabels)
            {
                foreach (var (region, box) in labels)
                {
                    string text = region.DisplayName;
                    SizeF size = g.MeasureString(text, _labelFont);
                    if (box.Width < 24 || box.Height < 12)
                    {
                        continue;
                    }

                    float x = Math.Max(2, box.X + 3), y = Math.Max(2, box.Y + 2);
                    using (var back = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
                    {
                        g.FillRectangle(back, x - 2, y - 1, size.Width + 2, size.Height);
                    }

                    using (var brush = new SolidBrush(Color.FromArgb(255, Lighten(RegionColor(region)))))
                    {
                        g.DrawString(text, _labelFont, brush, x, y);
                    }
                }
            }
        }

        private static Color Lighten(Color c) => Color.FromArgb(Math.Min(255, c.R + 70), Math.Min(255, c.G + 70), Math.Min(255, c.B + 70));

        private void DrawRegion(Graphics g, RegionNode region, Rectangle visible, bool selected, List<(RegionNode, RectangleF)> labels)
        {
            Color color = RegionColor(region);
            bool hover = region == HoverRegion && !selected;
            int alpha = Math.Max(0, Math.Min(255, FillAlpha + (selected ? 30 : hover ? 20 : 0)));
            RectangleF labelBox = RectangleF.Empty;

            using (var fill = new SolidBrush(Color.FromArgb(alpha, color)))
            using (var pen = new Pen(selected ? Color.White : hover ? Lighten(color) : color, selected ? 2f : 1.4f))
            using (var selPen = new Pen(Color.Yellow, 2.5f) { DashStyle = DashStyle.Dash })
            {
                for (int i = 0; i < region.Rects.Count; i++)
                {
                    RegionRect r = region.Rects[i];
                    if (!r.Bounds.IntersectsWith(visible))
                    {
                        continue;
                    }

                    RectangleF s = ScreenOf(r.Bounds);
                    g.FillRectangle(fill, s);
                    g.DrawRectangle(selected && i == SelectedRectIndex ? selPen : pen, s.X, s.Y, s.Width, s.Height);
                    if (labelBox.IsEmpty)
                    {
                        labelBox = s;
                    }

                    if (selected && (i == SelectedRectIndex || region.Rects.Count == 1))
                    {
                        DrawHandles(g, s);
                    }
                }

                for (int pi = 0; pi < region.Polygons.Count; pi++)
                {
                    RegionPolygon poly = region.Polygons[pi];
                    if (poly.Points.Count < 2 || !poly.Bounds.IntersectsWith(visible))
                    {
                        continue;
                    }

                    PointF[] pts = poly.Points.Select(p => ScreenOf(p.X, p.Y)).ToArray();
                    if (pts.Length >= 3)
                    {
                        g.FillPolygon(fill, pts);
                    }

                    g.DrawPolygon(pen, pts);
                    if (labelBox.IsEmpty)
                    {
                        labelBox = ScreenOf(poly.Bounds);
                    }

                    if (selected)
                    {
                        foreach (PointF p in pts)
                        {
                            g.FillRectangle(Brushes.White, p.X - 3, p.Y - 3, 6, 6);
                            g.DrawRectangle(Pens.Black, p.X - 3, p.Y - 3, 6, 6);
                        }
                    }
                }
            }

            if (!labelBox.IsEmpty)
            {
                labels.Add((region, labelBox));
            }

            if (selected || hover)
            {
                if (region.Go != null)
                {
                    DrawMarker(g, region.Go.X, region.Go.Y, Color.LimeGreen, "G");
                }

                if (region.Entrance != null && (string.IsNullOrEmpty(region.Entrance.Map) || RegionCatalog.FacetMapId(region.Entrance.Map) == MapId))
                {
                    DrawMarker(g, region.Entrance.X, region.Entrance.Y, Color.DeepSkyBlue, "E");
                }
            }
        }

        private void DrawMarker(Graphics g, int x, int y, Color color, string letter)
        {
            PointF p = ScreenOf(x + 0.5, y + 0.5);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new SolidBrush(color))
            {
                g.FillEllipse(brush, p.X - 8, p.Y - 8, 16, 16);
            }

            g.DrawEllipse(Pens.Black, p.X - 8, p.Y - 8, 16, 16);
            g.DrawString(letter, _labelFont, Brushes.Black, p.X - 5, p.Y - 7);
            g.SmoothingMode = SmoothingMode.None;
        }

        private static void DrawHandles(Graphics g, RectangleF s)
        {
            foreach (PointF h in HandlePoints(s))
            {
                g.FillRectangle(Brushes.White, h.X - (HandleSize / 2f), h.Y - (HandleSize / 2f), HandleSize, HandleSize);
                g.DrawRectangle(Pens.Black, h.X - (HandleSize / 2f), h.Y - (HandleSize / 2f), HandleSize, HandleSize);
            }
        }

        /// <summary>0 = top left, 1 = top, 2 = top right, 3 = right, 4 = bottom right, 5 = bottom, 6 = bottom left, 7 = left</summary>
        private static PointF[] HandlePoints(RectangleF s)
        {
            float cx = s.X + (s.Width / 2), cy = s.Y + (s.Height / 2);
            return new[]
            {
                new PointF(s.Left, s.Top), new PointF(cx, s.Top), new PointF(s.Right, s.Top), new PointF(s.Right, cy),
                new PointF(s.Right, s.Bottom), new PointF(cx, s.Bottom), new PointF(s.Left, s.Bottom), new PointF(s.Left, cy)
            };
        }

        private void DrawPreview(Graphics g)
        {
            using (var pen = new Pen(Color.Yellow, 2f) { DashStyle = DashStyle.Dash })
            using (var fill = new SolidBrush(Color.FromArgb(60, Color.Yellow)))
            {
                if (_drawing && (_tool == MapTool.Rect))
                {
                    Rectangle r = SnapRect(Geometry.Normalize(_downCell, _lastCell));
                    RectangleF s = ScreenOf(r);
                    g.FillRectangle(fill, s);
                    g.DrawRectangle(pen, s.X, s.Y, s.Width, s.Height);
                    DrawSizeText(g, s, $"{r.Width} × {r.Height}");
                }
                else if (_drawing && _tool == MapTool.Circle)
                {
                    int radius = Radius();
                    PointF c = ScreenOf(_downCell.X + 0.5, _downCell.Y + 0.5);
                    float rr = (float)((radius + 0.5) * _zoom);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillEllipse(fill, c.X - rr, c.Y - rr, rr * 2, rr * 2);
                    g.DrawEllipse(pen, c.X - rr, c.Y - rr, rr * 2, rr * 2);
                    g.SmoothingMode = SmoothingMode.None;
                    DrawSizeText(g, new RectangleF(c.X - rr, c.Y - rr, rr * 2, rr * 2), $"r = {radius}");
                }
                else if (_tool == MapTool.Polygon && _polygon.Count > 0)
                {
                    var pts = _polygon.Select(p => ScreenOf(p.X, p.Y)).ToList();
                    pts.Add(ScreenOf(Snapped(_lastCell).X, Snapped(_lastCell).Y));
                    if (pts.Count >= 3)
                    {
                        g.FillPolygon(fill, pts.ToArray());
                    }

                    g.DrawLines(pen, pts.ToArray());
                    foreach (PointF p in pts)
                    {
                        g.FillRectangle(Brushes.Yellow, p.X - 3, p.Y - 3, 6, 6);
                    }

                    PointF first = pts[0];
                    g.DrawEllipse(Pens.White, first.X - 7, first.Y - 7, 14, 14);
                }
            }
        }

        private void DrawSizeText(Graphics g, RectangleF s, string text)
        {
            SizeF size = g.MeasureString(text, _labelFont);
            float x = s.Right + 6, y = s.Bottom + 4;
            g.FillRectangle(Brushes.Black, x - 2, y - 1, size.Width + 4, size.Height + 2);
            g.DrawString(text, _labelFont, Brushes.Yellow, x, y);
        }

        private void DrawClient(Graphics g)
        {
            if (ClientPosition == null || ClientPosition.Value.Map != MapId)
            {
                return;
            }

            PointF p = ScreenOf(ClientPosition.Value.X + 0.5, ClientPosition.Value.Y + 0.5);
            using (var pen = new Pen(Color.Yellow, 2))
            {
                g.DrawLine(pen, p.X - 9, p.Y, p.X + 9, p.Y);
                g.DrawLine(pen, p.X, p.Y - 9, p.X, p.Y + 9);
                g.DrawEllipse(pen, p.X - 5, p.Y - 5, 10, 10);
            }
        }

        #endregion

        #region Minimap

        private Rectangle MinimapRect
        {
            get
            {
                if (_def == null)
                {
                    return Rectangle.Empty;
                }

                int w = Math.Min(200, Width / 4);
                int h = (int)(w * (_def.Height / (double)Math.Max(1, _def.Width)));
                return new Rectangle(Width - w - 10, Height - h - 10, w, h);
            }
        }

        private void OnMinimapTick(object sender, EventArgs e)
        {
            if (_map == null || _def == null || IsDisposed)
            {
                _minimapTimer.Stop();
                return;
            }

            // one pixel per "step" blocks: the minimap needs only a few hundred pixels, so only a part of the blocks is read
            int blocksW = Math.Max(1, _def.Width >> 3), blocksH = Math.Max(1, _def.Height >> 3);
            int step = Math.Max(1, (int)Math.Ceiling(Math.Max(blocksW, blocksH) / 256.0));
            int w = Math.Max(1, blocksW / step), h = Math.Max(1, blocksH / step);
            if (_minimap == null)
            {
                _minimap = new Bitmap(w, h);
                _minimapRow = 0;
            }

            try
            {
                DateTime end = DateTime.Now.AddMilliseconds(15);
                while (_minimapRow < h && DateTime.Now < end)
                {
                    for (int x = 0; x < w; x++)
                    {
                        Tile t = _map.Tiles.GetLandTile((x * step << 3) + 4, (_minimapRow * step << 3) + 4);
                        ushort c = RadarCol.GetLandColor(t.Id);
                        _minimap.SetPixel(x, _minimapRow, Color.FromArgb(Hues.HueToColorR(c), Hues.HueToColorG(c), Hues.HueToColorB(c)));
                    }

                    _minimapRow++;
                }
            }
            catch (Exception)
            {
                _minimapRow = h;
            }

            if (_minimapRow >= h)
            {
                _minimapTimer.Stop();
            }

            if (ShowMinimap)
            {
                Invalidate(MinimapRect);
            }
        }

        private void DrawMinimap(Graphics g)
        {
            Rectangle box = MinimapRect;
            if (box.Width < 40 || box.Height < 20)
            {
                return;
            }

            using (var back = new SolidBrush(Color.FromArgb(200, 0, 0, 0)))
            {
                g.FillRectangle(back, box);
            }

            if (_minimap != null)
            {
                g.InterpolationMode = InterpolationMode.Bilinear;
                g.DrawImage(_minimap, box);
            }

            double sx = box.Width / (double)_def.Width, sy = box.Height / (double)_def.Height;
            if (Facet != null)
            {
                foreach (RegionNode r in Facet.Regions.Where(r => !r.Hidden))
                {
                    using (var pen = new Pen(r == SelectedRegion ? Color.White : RegionColor(r)))
                    {
                        foreach (RegionRect rect in r.Rects)
                        {
                            g.DrawRectangle(pen, box.X + (float)(rect.X * sx), box.Y + (float)(rect.Y * sy),
                                Math.Max(1, (float)(rect.Width * sx)), Math.Max(1, (float)(rect.Height * sy)));
                        }
                    }
                }
            }

            Rectangle v = VisibleCells;
            g.DrawRectangle(Pens.Yellow, box.X + (float)(v.X * sx), box.Y + (float)(v.Y * sy), (float)(v.Width * sx), (float)(v.Height * sy));
            g.DrawRectangle(Pens.Gray, box);
        }

        private bool MinimapHit(Point p, out Point cell)
        {
            cell = Point.Empty;
            Rectangle box = MinimapRect;
            if (!ShowMinimap || _def == null || !box.Contains(p))
            {
                return false;
            }

            cell = new Point((int)((p.X - box.X) * _def.Width / (double)box.Width), (int)((p.Y - box.Y) * _def.Height / (double)box.Height));
            return true;
        }

        #endregion

        #region Mouse

        private Point Snapped(Point cell) => Geometry.Snap(cell, Math.Max(1, Snap));

        private Rectangle SnapRect(Rectangle r)
        {
            if (Snap <= 1)
            {
                return r;
            }

            int x1 = Geometry.Snap(r.Left, Snap), y1 = Geometry.Snap(r.Top, Snap), x2 = Geometry.Snap(r.Right, Snap), y2 = Geometry.Snap(r.Bottom, Snap);
            if (x2 <= x1)
            {
                x2 = x1 + Snap;
            }

            if (y2 <= y1)
            {
                y2 = y1 + Snap;
            }

            return Rectangle.FromLTRB(x1, y1, x2, y2);
        }

        private int Radius() => (int)Math.Round(Math.Sqrt(Math.Pow(_lastCell.X - _downCell.X, 2) + Math.Pow(_lastCell.Y - _downCell.Y, 2)));

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            ZoomStep(e.Delta > 0 ? 1 : -1, e.Location);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (_def == null)
            {
                return;
            }

            _mouseDown = e.Location;
            _downCell = CellAt(e.Location);
            _lastCell = _downCell;

            if (e.Button == MouseButtons.Left && MinimapHit(e.Location, out Point mini))
            {
                CenterOn(mini.X, mini.Y);
                return;
            }

            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && (_tool == MapTool.Pan || ModifierKeys == Keys.Control && _tool != MapTool.Select)))
            {
                _panning = true;
                _panStartX = _viewX;
                _panStartY = _viewY;
                Cursor = Cursors.SizeAll;
                return;
            }

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            switch (_tool)
            {
                case MapTool.Rect:
                case MapTool.Circle:
                    _downCell = _tool == MapTool.Rect ? Snapped(_downCell) : _downCell;
                    _drawing = true;
                    break;
                case MapTool.Polygon:
                    {
                        Point p = Snapped(_downCell);
                        if (_polygon.Count >= 3 && Distance(ScreenOf(_polygon[0].X, _polygon[0].Y), e.Location) < 9)
                        {
                            FinishPolygon();
                        }
                        else if (_polygon.Count == 0 || _polygon[_polygon.Count - 1] != p)
                        {
                            _polygon.Add(p);
                        }

                        Invalidate();
                        break;
                    }
                case MapTool.Fill:
                    ShapeCreated?.Invoke(this, new ShapeEventArgs { Tool = MapTool.Fill, Cell = _downCell, AddToSelected = ModifierKeys.HasFlag(Keys.Shift) });
                    break;
                case MapTool.Go:
                case MapTool.Entrance:
                    PointPicked?.Invoke(this, new ShapeEventArgs { Tool = _tool, Cell = _downCell });
                    break;
                case MapTool.Select:
                    StartSelectDrag(e.Location);
                    break;
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (_tool == MapTool.Polygon && _polygon.Count >= 3)
            {
                FinishPolygon();
            }
        }

        private static double Distance(PointF a, Point b) => Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));

        private void FinishPolygon()
        {
            var points = _polygon.ToList();
            _polygon.Clear();
            Invalidate();
            if (points.Count >= 3)
            {
                ShapeCreated?.Invoke(this, new ShapeEventArgs
                {
                    Tool = MapTool.Polygon,
                    Polygon = points,
                    AddToSelected = ModifierKeys.HasFlag(Keys.Shift),
                    Subtract = ModifierKeys.HasFlag(Keys.Alt)
                });
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_def == null)
            {
                return;
            }

            Point cell = CellAt(e.Location);
            if (_panning)
            {
                _viewX = _panStartX - ((e.X - _mouseDown.X) / _zoom);
                _viewY = _panStartY - ((e.Y - _mouseDown.Y) / _zoom);
                ClampView();
                Invalidate();
                ViewChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (cell != _lastCell)
            {
                _lastCell = cell;
                if (_drag != DragKind.None)
                {
                    ContinueDrag(cell);
                }

                RegionNode hover = RegionAt(cell, false).FirstOrDefault();
                if (hover != HoverRegion)
                {
                    HoverRegion = hover;
                    Invalidate();
                }

                if (_drawing || _polygon.Count > 0)
                {
                    Invalidate();
                }

                CursorMoved?.Invoke(this, new RegionPickEventArgs { Cell = cell, Region = hover });
            }

            if (_tool == MapTool.Select && _drag == DragKind.None && e.Button == MouseButtons.None)
            {
                Cursor = HitHandle(e.Location, out int _, out int _) >= 0 ? Cursors.SizeNESW : Cursors.Default;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_panning)
            {
                _panning = false;
                Cursor = _tool == MapTool.Pan ? Cursors.SizeAll : _tool == MapTool.Select ? Cursors.Default : Cursors.Cross;
                return;
            }

            if (_drawing && e.Button == MouseButtons.Left)
            {
                _drawing = false;
                if (Math.Abs(e.X - _mouseDown.X) < 4 && Math.Abs(e.Y - _mouseDown.Y) < 4)
                {
                    // a simple click is no shape
                    Invalidate();
                    return;
                }

                bool shift = ModifierKeys.HasFlag(Keys.Shift), alt = ModifierKeys.HasFlag(Keys.Alt);
                if (_tool == MapTool.Rect)
                {
                    Rectangle r = SnapRect(Geometry.Normalize(_downCell, Snapped(CellAt(e.Location))));
                    ShapeCreated?.Invoke(this, new ShapeEventArgs { Tool = MapTool.Rect, Rect = r, AddToSelected = shift, Subtract = alt });
                }
                else if (_tool == MapTool.Circle)
                {
                    ShapeCreated?.Invoke(this, new ShapeEventArgs { Tool = MapTool.Circle, Cell = _downCell, Radius = Radius(), AddToSelected = shift, Subtract = alt });
                }

                Invalidate();
                return;
            }

            if (_drag != DragKind.None)
            {
                bool changed = _dragChanged;
                _drag = DragKind.None;
                _dragChanged = false;
                if (changed)
                {
                    EditFinished?.Invoke(this, EventArgs.Empty);
                }

                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (HoverRegion != null)
            {
                HoverRegion = null;
                Invalidate();
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            return key == Keys.Left || key == Keys.Right || key == Keys.Up || key == Keys.Down || key == Keys.Escape || key == Keys.Enter || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            double step = 64 / _zoom;
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    _polygon.Clear();
                    _drawing = false;
                    Invalidate();
                    break;
                case Keys.Enter:
                    if (_tool == MapTool.Polygon)
                    {
                        FinishPolygon();
                    }

                    break;
                case Keys.Back:
                    if (_polygon.Count > 0)
                    {
                        _polygon.RemoveAt(_polygon.Count - 1);
                        Invalidate();
                    }

                    break;
                case Keys.Left:
                    Pan(-step, 0);
                    break;
                case Keys.Right:
                    Pan(step, 0);
                    break;
                case Keys.Up:
                    Pan(0, -step);
                    break;
                case Keys.Down:
                    Pan(0, step);
                    break;
                case Keys.Add:
                case Keys.Oemplus:
                    ZoomIn();
                    break;
                case Keys.Subtract:
                case Keys.OemMinus:
                    ZoomOut();
                    break;
            }
        }

        private void Pan(double dx, double dy)
        {
            _viewX += dx;
            _viewY += dy;
            ClampView();
            Invalidate();
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ClampView();
        }

        #endregion

        #region Hit tests and dragging

        /// <summary>regions at a cell, the most specific (child, highest priority, smallest) first</summary>
        public List<RegionNode> RegionAt(Point cell, bool includeHidden)
        {
            if (Facet == null)
            {
                return new List<RegionNode>();
            }

            var result = new List<(RegionNode Region, int Depth)>();
            void Visit(IEnumerable<RegionNode> list, int depth)
            {
                foreach (RegionNode r in list)
                {
                    if ((includeHidden || !r.Hidden) && r.Contains(cell.X, cell.Y))
                    {
                        result.Add((r, depth));
                    }

                    Visit(r.Children, depth + 1);
                }
            }

            Visit(Facet.Regions, 0);
            return result.OrderByDescending(r => r.Depth).ThenByDescending(r => r.Region.Priority).ThenBy(r => r.Region.Area)
                .Select(r => r.Region).ToList();
        }

        /// <summary>handle index of a rect of the selected region under the mouse, -1 = none</summary>
        private int HitHandle(Point screen, out int rectIndex, out int dummy)
        {
            rectIndex = -1;
            dummy = 0;
            RegionNode sel = SelectedRegion;
            if (sel == null || sel.Locked)
            {
                return -1;
            }

            for (int i = 0; i < sel.Rects.Count; i++)
            {
                if (i != SelectedRectIndex && sel.Rects.Count > 1)
                {
                    continue;
                }

                PointF[] handles = HandlePoints(ScreenOf(sel.Rects[i].Bounds));
                for (int h = 0; h < handles.Length; h++)
                {
                    if (Math.Abs(handles[h].X - screen.X) <= HandleSize && Math.Abs(handles[h].Y - screen.Y) <= HandleSize)
                    {
                        rectIndex = i;
                        return h;
                    }
                }
            }

            return -1;
        }

        private bool HitVertex(Point screen, out int poly, out int vertex)
        {
            poly = vertex = -1;
            RegionNode sel = SelectedRegion;
            if (sel == null || sel.Locked)
            {
                return false;
            }

            for (int p = 0; p < sel.Polygons.Count; p++)
            {
                for (int v = 0; v < sel.Polygons[p].Points.Count; v++)
                {
                    PointF s = ScreenOf(sel.Polygons[p].Points[v].X, sel.Polygons[p].Points[v].Y);
                    if (Math.Abs(s.X - screen.X) <= HandleSize && Math.Abs(s.Y - screen.Y) <= HandleSize)
                    {
                        poly = p;
                        vertex = v;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool HitMarker(RegionPoint p, Point screen) => p != null && Distance(ScreenOf(p.X + 0.5, p.Y + 0.5), screen) <= 9;

        private void StartSelectDrag(Point screen)
        {
            RegionNode sel = SelectedRegion;
            _dragChanged = false;

            if (sel != null && !sel.Locked)
            {
                if (HitMarker(sel.Go, screen))
                {
                    _drag = DragKind.GoPoint;
                    return;
                }

                if (HitMarker(sel.Entrance, screen))
                {
                    _drag = DragKind.EntrancePoint;
                    return;
                }

                int handle = HitHandle(screen, out int rectIndex, out int _);
                if (handle >= 0)
                {
                    _drag = DragKind.Handle;
                    _dragHandle = handle;
                    _dragRect = rectIndex;
                    _dragStartRect = sel.Rects[rectIndex].Bounds;
                    return;
                }

                if (HitVertex(screen, out int poly, out int vertex))
                {
                    _drag = DragKind.Vertex;
                    _dragPoly = poly;
                    _dragVertex = vertex;
                    return;
                }

                // click inside the selected region: move a rect (Shift: the whole region)
                int inside = sel.Rects.FindIndex(r => r.Contains(_downCell.X, _downCell.Y));
                bool inPoly = sel.Polygons.Any(p => Geometry.PointInPolygon(p.Points, _downCell.X + 0.5, _downCell.Y + 0.5));
                if ((inside >= 0 || inPoly) && (ModifierKeys.HasFlag(Keys.Shift) || inPoly || inside == SelectedRectIndex || sel.Rects.Count == 1))
                {
                    if (ModifierKeys.HasFlag(Keys.Shift) || inPoly)
                    {
                        _drag = DragKind.MoveRegion;
                        _dragRegionStart = sel.SelfAndDescendants().SelectMany(r => r.Rects).Select(r => (r, r.Bounds)).ToList();
                        _dragPolyStart = sel.SelfAndDescendants().SelectMany(r => r.Polygons).Select(p => p.Points.ToList()).ToList();
                    }
                    else
                    {
                        _drag = DragKind.MoveRect;
                        _dragRect = inside;
                        _dragStartRect = sel.Rects[inside].Bounds;
                    }

                    if (RegionAt(_downCell, false).FirstOrDefault() == sel || ModifierKeys.HasFlag(Keys.Shift))
                    {
                        return;
                    }

                    _drag = DragKind.None;
                }
            }

            // pick: Alt cycles through overlapping regions
            List<RegionNode> hits = RegionAt(_downCell, false);
            RegionNode pick = hits.FirstOrDefault();
            if (ModifierKeys.HasFlag(Keys.Alt) && hits.Count > 1 && sel != null && hits.Contains(sel))
            {
                pick = hits[(hits.IndexOf(sel) + 1) % hits.Count];
            }

            int pickRect = pick?.Rects.FindIndex(r => r.Contains(_downCell.X, _downCell.Y)) ?? -1;
            RegionPicked?.Invoke(this, new RegionPickEventArgs { Region = pick, RectIndex = pickRect, Cell = _downCell });

            // a newly picked single rect can be moved at once
            if (pick != null && pick == SelectedRegion && !pick.Locked && pickRect >= 0)
            {
                _drag = DragKind.MoveRect;
                _dragRect = pickRect;
                _dragStartRect = pick.Rects[pickRect].Bounds;
            }
        }

        private void BeginChange()
        {
            if (!_dragChanged)
            {
                _dragChanged = true;
                EditStarting?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ContinueDrag(Point cell)
        {
            RegionNode sel = SelectedRegion;
            if (sel == null)
            {
                _drag = DragKind.None;
                return;
            }

            int dx = cell.X - _downCell.X, dy = cell.Y - _downCell.Y;
            if (Snap > 1)
            {
                dx = Geometry.Snap(dx, Snap);
                dy = Geometry.Snap(dy, Snap);
            }

            switch (_drag)
            {
                case DragKind.MoveRect:
                    if (_dragRect >= 0 && _dragRect < sel.Rects.Count && (dx != 0 || dy != 0 || _dragChanged))
                    {
                        BeginChange();
                        sel.Rects[_dragRect].X = _dragStartRect.X + dx;
                        sel.Rects[_dragRect].Y = _dragStartRect.Y + dy;
                    }

                    break;
                case DragKind.MoveRegion:
                    if (dx != 0 || dy != 0 || _dragChanged)
                    {
                        BeginChange();
                        foreach (var (rect, start) in _dragRegionStart)
                        {
                            rect.X = start.X + dx;
                            rect.Y = start.Y + dy;
                        }

                        List<RegionPolygon> polys = sel.SelfAndDescendants().SelectMany(r => r.Polygons).ToList();
                        for (int i = 0; i < polys.Count && i < _dragPolyStart.Count; i++)
                        {
                            polys[i].Points = _dragPolyStart[i].Select(p => new Point(p.X + dx, p.Y + dy)).ToList();
                        }
                    }

                    break;
                case DragKind.Handle:
                    if (_dragRect >= 0 && _dragRect < sel.Rects.Count)
                    {
                        BeginChange();
                        Rectangle s = _dragStartRect;
                        int left = s.Left, top = s.Top, right = s.Right, bottom = s.Bottom;
                        Point c = Snapped(cell);
                        if (_dragHandle == 0 || _dragHandle == 6 || _dragHandle == 7)
                        {
                            left = Math.Min(c.X, right - 1);
                        }

                        if (_dragHandle == 2 || _dragHandle == 3 || _dragHandle == 4)
                        {
                            right = Math.Max(c.X + (Snap > 1 ? 0 : 1), left + 1);
                        }

                        if (_dragHandle == 0 || _dragHandle == 1 || _dragHandle == 2)
                        {
                            top = Math.Min(c.Y, bottom - 1);
                        }

                        if (_dragHandle == 4 || _dragHandle == 5 || _dragHandle == 6)
                        {
                            bottom = Math.Max(c.Y + (Snap > 1 ? 0 : 1), top + 1);
                        }

                        RegionRect r = sel.Rects[_dragRect];
                        r.X = left;
                        r.Y = top;
                        r.Width = right - left;
                        r.Height = bottom - top;
                    }

                    break;
                case DragKind.Vertex:
                    if (_dragPoly >= 0 && _dragPoly < sel.Polygons.Count && _dragVertex < sel.Polygons[_dragPoly].Points.Count)
                    {
                        BeginChange();
                        sel.Polygons[_dragPoly].Points[_dragVertex] = Snapped(cell);
                    }

                    break;
                case DragKind.GoPoint:
                    BeginChange();
                    sel.Go.X = cell.X;
                    sel.Go.Y = cell.Y;
                    break;
                case DragKind.EntrancePoint:
                    BeginChange();
                    sel.Entrance.X = cell.X;
                    sel.Entrance.Y = cell.Y;
                    break;
            }

            Invalidate();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _minimapTimer.Stop();
                _minimapTimer.Dispose();
                _mapBitmap?.Dispose();
                _minimap?.Dispose();
                _labelFont.Dispose();
                _infoFont.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

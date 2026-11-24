// /***************************************************************************
//  *
//  * Dragon Map Studio – map drawing surface
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls
{
    public enum MapTool
    {
        Brush,
        Eraser,
        Spray,
        Line,
        Rectangle,
        FilledRectangle,
        Ellipse,
        FilledEllipse,
        Fill,
        Picker,
        Raise,
        Lower,
        Select,
        Pan,

        /// <summary>Click: cave entrance like the original (colours), right click: remove it again.</summary>
        CaveStamp,

        /// <summary>Freehand: the chosen stamp is painted as a continuous texture along the mouse (roads, rivers …).</summary>
        PatternBrush,

        /// <summary>Drag from the middle of the flat side to the round side: half circle.</summary>
        HalfCircle,

        /// <summary>Drag from the middle of the base to the tip: triangle.</summary>
        Triangle,

        /// <summary>Line whose height runs from the height at the start to the height at the end (colours of the group + height layer).</summary>
        SlopeLine,

        /// <summary>Natural course from A to B (river, road) with the brush or the pattern stamp.</summary>
        Course,

        /// <summary>Drag a rectangle: the dungeon generator fills it (colour 1 = floor, colour 2 = walls).</summary>
        Dungeon,

        /// <summary>Click / drag: a motif of the "Muster-Baukasten" into the statics layer (repeated along the drag).</summary>
        Motif
    }

    public sealed class MapClickEventArgs : EventArgs
    {
        public MapClickEventArgs(int x, int y, MouseButtons button, Keys modifiers)
        {
            X = x;
            Y = y;
            Button = button;
            Modifiers = modifiers;
        }

        public int X { get; }
        public int Y { get; }
        public MouseButtons Button { get; }
        public Keys Modifiers { get; }
    }

    public sealed class MapPointEventArgs : EventArgs
    {
        public MapPointEventArgs(int x, int y, int index)
        {
            X = x;
            Y = y;
            Index = index;
        }

        public int X { get; }
        public int Y { get; }

        /// <summary>Palette index under the cursor (-1 outside the map).</summary>
        public int Index { get; }
    }

    /// <summary>
    /// Draws the 8-bit map with zoom and panning (renders only the visible part, so 7168×4096 maps stay fast)
    /// and implements all painting tools. Every change goes through <see cref="MapHistory"/> (undo / redo).
    /// Left mouse = colour 1, right mouse = colour 2, middle mouse or space = pan, wheel = zoom.
    /// </summary>
    [ToolboxItem(true)]
    public class MapCanvas : Control
    {
        private IndexedMap _map;
        private MapHistory _history;
        private readonly int[] _argb = new int[65536];
        private Bitmap _view;
        private bool _viewDirty = true;

        private double _zoom = 1;
        private double _originX;
        private double _originY;

        private MapTool _tool = MapTool.Brush;
        private bool _drawing;
        private MouseButtons _drawButton;
        private Point _start;
        private Point _last;
        private Point _cursor = new Point(-1, -1);
        private bool _panning;
        private Point _panStart;
        private double _panOriginX;
        private double _panOriginY;
        private bool _spaceDown;
        private readonly Random _rnd = new Random();

        private Rectangle _selection = Rectangle.Empty;
        private IndexedMap _pasteBuffer;
        private bool _pasteMode;

        private ushort[] _raise = Identity();
        private ushort[] _lower = Identity();

        public MapCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.FromArgb(30, 30, 34);
            TabStop = true;
        }

        public event EventHandler<MapPointEventArgs> CursorMoved;

        /// <summary>Click with a tool the form carries out (cave stamp).</summary>
        public event EventHandler<MapClickEventArgs> ToolClicked;

        /// <summary>
        /// Right click with Auswahl / Hand / Pipette, or Umschalt + right click with any tool: the form shows its context menu
        /// (X / Y = map position, the screen position is <see cref="Control.MousePosition"/>).
        /// </summary>
        public event EventHandler<MapClickEventArgs> ContextRequested;

        /// <summary>Mouse button (plus <see cref="JumpModifiers"/>) that jumps the live CGI to the clicked cell; None = off.</summary>
        public MouseButtons JumpButton { get; set; } = MouseButtons.None;

        public Keys JumpModifiers { get; set; } = Keys.None;

        /// <summary>The jump button was clicked on the map.</summary>
        public event EventHandler<MapClickEventArgs> JumpClicked;

        /// <summary>The next click on the map only reports its button (to assign the jump button) and does nothing else.</summary>
        public bool CaptureNextButton { get; set; }

        public event EventHandler<MapClickEventArgs> ButtonCaptured;

        /// <summary>Paste / colour stamp: turns the buffer into the pixels for this position (e.g. heights adapted to the ground there).</summary>
        [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Func<IndexedMap, Point, IndexedMap> PasteTransform { get; set; }

        /// <summary>Name of the undo step while pasting (e.g. "Stempel Haus").</summary>
        [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string PasteName { get; set; } = "Einfügen";

        /// <summary>Paste mode stays on after a click (stamp several times); Esc / right click ends it.</summary>
        [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool PasteRepeat { get; set; }

        public bool PasteActive => _pasteMode && _pasteBuffer != null;

        // ------------------------------------------------------------------ pattern brush

        /// <summary>
        /// Pattern brush: new colour for a cell (x, y, current colour) or -1 = keep the colour (statics only / transparent).
        /// Every cell is asked once per stroke (no drift of the heights).
        /// </summary>
        [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Func<int, int, ushort, int> PatternColour { get; set; }

        /// <summary>Cells the pattern brush reached for the first time in this stroke (inside the undo step) – the form adds statics there.</summary>
        public event EventHandler<List<Point>> PatternCellsPainted;

        /// <summary>Raised while a brush stroke changes pixels (before the stroke ends) – live CGI preview.</summary>
        public event EventHandler Painting;

        /// <summary>Stroke ended (mouse up) – e.g. for the live CGI preview.</summary>
        public event EventHandler StrokeEnded;

        /// <summary>Mouse up of a pattern stroke, before its undo step closes (automatic repair belongs into the same step).</summary>
        public event EventHandler StrokeFinishing;

        /// <summary>
        /// Undo per part of a stroke: after this many painted cells a new undo step begins (0 = the whole stroke is one step).
        /// The parts of one stroke form a group: "Ganzen Strich rückgängig" takes them back together.
        /// </summary>
        [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int StrokeStepCells { get; set; }

        private readonly HashSet<int> _patternDone = new HashSet<int>();
        private int _strokeGroup;
        private int _stepCells;

        /// <summary>Last map position of the mouse (for the live preview).</summary>
        public Point CursorTile => _cursor;

        /// <summary>
        /// Brush, eraser, spray, line, shapes, fill and "fill selection": the colour that really goes into a cell
        /// (x, y, colour there, chosen colour) – e.g. the same group at the height of the ground ("Höhe behalten").
        /// Null = the chosen colour.
        /// </summary>
        [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Func<int, int, ushort, ushort, ushort> CellColour { get; set; }

        /// <summary>An undo step of the canvas is about to close – the form adds what belongs to it (height layer).</summary>
        public event EventHandler StepClosing;

        /// <summary>Height layer of the map (x, y) → (z, colour): the height counts while the cell still has that colour.</summary>
        [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Func<IReadOnlyDictionary<(int x, int y), (sbyte z, ushort colour)>> HeightLayer { get; set; }

        /// <summary>Show the cells of the height layer (cyan = active, grey = old entry whose colour was painted over).</summary>
        [Category("Dragon"), DefaultValue(false)]
        public bool ShowHeightLayer { get; set; }

        /// <summary>Height of a cell: the height layer when its colour still matches, else the altitude of the colour.</summary>
        public int AltitudeAt(int x, int y)
        {
            if (_map == null || !_map.InBounds(x, y))
            {
                return 0;
            }
            ushort c = _map[x, y];
            IReadOnlyDictionary<(int x, int y), (sbyte z, ushort colour)> layer = HeightLayer?.Invoke();
            if (layer != null && layer.TryGetValue((x, y), out (sbyte z, ushort colour) h) && h.colour == c)
            {
                return h.z;
            }
            return _altitudes[c];
        }

        /// <summary>Half circle / triangle: filled (true) or only the outline with the brush size as thickness.</summary>
        [Category("Dragon"), DefaultValue(true)]
        public bool ShapeFilled { get; set; } = true;

        /// <summary>Slope line: colour of a cell at 0..1 along the line from a to b (x, y, current, chosen, t, a, b). Null = chosen colour.</summary>
        [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Func<int, int, ushort, ushort, double, Point, Point, ushort> SlopeColour { get; set; }

        /// <summary>Settings of the natural course.</summary>
        [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public CourseSettings Course { get; set; } = new CourseSettings();

        /// <summary>Natural course with the pattern stamp (statics) instead of the colour brush.</summary>
        [Category("Dragon"), DefaultValue(false)]
        public bool CourseWithPattern { get; set; }

        /// <summary>Colours of the course: core (−1 = colour 1), edge and outer edge (−1 = none) with their widths per side.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public (int core, int edge, int edgeWidth, int edge2, int edge2Width)? CourseColours { get; set; }

        /// <summary>Tool "Dungeon": the rectangle was dragged – the form builds the dungeon.</summary>
        public event EventHandler<Rectangle> DungeonRequested;

        /// <summary>Tool "Muster": click (a == b) or drag from a to b – the form puts the motif into the statics layer.</summary>
        public event Action<Point, Point> MotifRequested;

        /// <summary>Tool "Muster", point to point: the clicked points are finished (double click, right click, Enter).</summary>
        public event Action<List<Point>> MotifPathRequested;

        /// <summary>Tool "Muster" collects points (point to point with corners) instead of dragging.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<bool> MotifPointMode { get; set; }

        /// <summary>Preview of point to point: the cells of the path through the points.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<List<Point>, IReadOnlyCollection<Point>> MotifPathPreview { get; set; }

        private readonly List<Point> _motifPoints = new List<Point>();

        private bool MotifPoints => _tool == MapTool.Motif && (MotifPointMode?.Invoke() ?? false);

        private void FinishMotifPoints()
        {
            var pts = _motifPoints.ToList();
            _motifPoints.Clear();
            Invalidate();
            if (pts.Count > 0)
            {
                MotifPathRequested?.Invoke(pts);
            }
        }

        /// <summary>Preview of the tool "Muster": the cells the motif would cover for a click / drag from a to b.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<Point, Point, IReadOnlyCollection<Point>> MotifPreview { get; set; }

        /// <summary>Colour written into a cell by the colour tools (see <see cref="CellColour"/>).</summary>
        private ushort PaintCell(int x, int y, ushort current, ushort chosen) => CellColour == null ? chosen : CellColour(x, y, current, chosen);

        private void PatternStamp(int cx, int cy)
        {
            int size = Math.Max(1, BrushSize);
            int half = size / 2;
            var r = new Rectangle(cx - half, cy - half, size, size);
            r.Intersect(_map.Bounds);
            if (r.Width <= 0 || r.Height <= 0 || PatternColour == null)
            {
                return;
            }
            _history?.Touch(r);
            bool round = !SquareBrush && size > 2;
            double radius = size / 2.0;
            double ccx = cx - half + radius, ccy = cy - half + radius;
            var cells = new List<Point>();
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    if (round)
                    {
                        double ddx = x + 0.5 - ccx, ddy = y + 0.5 - ccy;
                        if (ddx * ddx + ddy * ddy > radius * radius)
                        {
                            continue;
                        }
                    }
                    int i = y * _map.Width + x;
                    if (!_patternDone.Add(i))
                    {
                        continue;
                    }
                    int c = PatternColour(x, y, _map.Pixels[i]);
                    if (c >= 0)
                    {
                        _map.Pixels[i] = (ushort)c;
                    }
                    cells.Add(new Point(x, y));
                }
            }
            if (cells.Count > 0)
            {
                PatternCellsPainted?.Invoke(this, cells);
            }
            Changed(r);
            SplitStrokeStep(cells.Count);
        }

        /// <summary>Stepwise undo: closes the current step after StrokeStepCells cells and opens the next one of the same stroke.</summary>
        private void SplitStrokeStep(int painted)
        {
            if (StrokeStepCells <= 0 || _history == null)
            {
                return;
            }
            _stepCells += painted;
            if (_stepCells >= StrokeStepCells)
            {
                _stepCells = 0;
                StepClosing?.Invoke(this, EventArgs.Empty);
                _history.End();
                MapEdited?.Invoke(this, EventArgs.Empty);
                _history.Begin(ToolName(_tool) + " (Teil)", _strokeGroup);
            }
        }

        /// <summary>After a paste / stamp was written, still inside its undo step (the form adds the statics of the stamp).</summary>
        public event EventHandler<MapPointEventArgs> PasteApplied;

        private IReadOnlyList<LayerStatic> _markers;

        /// <summary>Statics layer of the map: shown as small orange squares from zoom 2 on.</summary>
        public void SetStaticMarkers(IReadOnlyList<LayerStatic> items)
        {
            _markers = items;
            Invalidate();
        }

        private void DrawStaticMarkers(Graphics g)
        {
            if (_markers == null || _markers.Count == 0 || _zoom < 2)
            {
                return;
            }
            RectangleF v = ViewRect;
            float size = (float)Math.Max(2, _zoom * 0.45);
            using var brush = new SolidBrush(Color.FromArgb(220, 255, 140, 0));
            int drawn = 0;
            foreach (LayerStatic s in _markers)
            {
                if (s.X < v.Left - 1 || s.Y < v.Top - 1 || s.X > v.Right || s.Y > v.Bottom)
                {
                    continue;
                }
                PointF p = MapToScreen(s.X + 0.5, s.Y + 0.5);
                g.FillRectangle(brush, p.X - size / 2, p.Y - size / 2, size, size);
                if (++drawn > 20000)
                {
                    break;
                }
            }
        }

        public void EndPaste()
        {
            _pasteMode = false;
            PasteTransform = null;
            PasteRepeat = false;
            PasteName = "Einfügen";
            Invalidate();
        }

        /// <summary>
        /// One undoable edit by the form: everything within <paramref name="radius"/> around (x, y) is recorded first,
        /// <paramref name="edit"/> changes the map and returns the changed area (empty = nothing changed, no undo step).
        /// </summary>
        /// <summary>
        /// One undoable edit of a rectangle (recorded first). <paramref name="extra"/> is called inside the step
        /// (e.g. to add the statics of the step to the history).
        /// </summary>
        public void EditRect(Rectangle area, string name, Action<IndexedMap> edit, Action extra = null)
        {
            if (_map == null)
            {
                return;
            }
            area.Intersect(_map.Bounds);
            if (area.IsEmpty)
            {
                return;
            }
            _history?.Begin(name);
            _history?.Touch(area);
            edit(_map);
            extra?.Invoke();
            EndEdit();
            Changed(area);
        }

        public Rectangle EditAround(int x, int y, int radius, string name, Func<IndexedMap, Rectangle> edit)
        {
            if (_map == null)
            {
                return Rectangle.Empty;
            }
            var area = Rectangle.FromLTRB(x - radius, y - radius, x + radius + 1, y + radius + 1);
            area.Intersect(_map.Bounds);
            _history?.Begin(name);
            _history?.Touch(area);
            Rectangle r = edit(_map);
            if (r.IsEmpty)
            {
                _history?.Cancel();
                return r;
            }
            EndEdit();
            Changed(r);
            return r;
        }
        public event EventHandler<PaletteIndexEventArgs> ColorPicked;
        public event EventHandler ViewChanged;
        public event EventHandler MapEdited;
        public event EventHandler SelectionChanged;

        // ------------------------------------------------------------------ properties

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IndexedMap Map => _map;

        [Category("Dragon"), DefaultValue(MapTool.Brush)]
        public MapTool Tool
        {
            get => _tool;
            set
            {
                if (_tool == MapTool.Select && value != MapTool.Select)
                {
                    _pasteMode = false;
                }
                _tool = value;
                Cursor = value == MapTool.Pan ? Cursors.Hand : value == MapTool.Picker ? Cursors.Cross : Cursors.Default;
                Invalidate();
            }
        }

        [Category("Dragon"), DefaultValue(8)]
        public int BrushSize { get; set; } = 8;

        [Category("Dragon"), DefaultValue(false)]
        public bool SquareBrush { get; set; }

        [Category("Dragon"), DefaultValue(0)]
        public int PrimaryIndex { get; set; }

        [Category("Dragon"), DefaultValue(32)]
        public int SecondaryIndex { get; set; } = 32;

        [Category("Dragon"), DefaultValue(true)]
        public bool ShowGrid { get; set; } = true;

        [Category("Dragon"), DefaultValue(6)]
        public int GridFromZoom { get; set; } = 6;

        /// <summary>Show the altitude of every pixel (from zoom 12).</summary>
        [Category("Dragon"), DefaultValue(false)]
        public bool ShowHeights { get; set; }

        private int[] _altitudes = new int[65536];

        /// <summary>Altitude (Z) of every palette index (for the height numbers).</summary>
        public void SetAltitudes(int[] altitudes)
        {
            _altitudes = new int[65536];
            altitudes?.AsSpan(0, Math.Min(altitudes.Length, 65536)).CopyTo(_altitudes);
            Invalidate();
        }

        /// <summary>Spray: dots per step in percent of the brush area.</summary>
        [Category("Dragon"), DefaultValue(8)]
        public int SprayDensity { get; set; } = 8;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double Zoom => _zoom;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle Selection => _selection;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool PasteMode => _pasteMode;

        /// <summary>The visible part of the map in map coordinates.</summary>
        [Browsable(false)]
        public RectangleF ViewRect => new RectangleF((float)_originX, (float)_originY, (float)(Width / _zoom), (float)(Height / _zoom));

        // ------------------------------------------------------------------ setup

        public void SetMap(IndexedMap map, MapHistory history)
        {
            _map = map;
            _history = history;
            _selection = Rectangle.Empty;
            _pasteMode = false;
            ZoomToFit();
            InvalidateMap();
        }

        public void SetPalette(Color[] colors)
        {
            Array.Clear(_argb);
            for (int i = 0; i < colors.Length && i < _argb.Length; i++)
            {
                _argb[i] = colors[i].ToArgb();
            }
            InvalidateMap();
        }

        public void SetHeightLookups(ushort[] raise, ushort[] lower)
        {
            _raise = Identity();
            _lower = Identity();
            raise?.AsSpan(0, Math.Min(raise.Length, 65536)).CopyTo(_raise);
            lower?.AsSpan(0, Math.Min(lower.Length, 65536)).CopyTo(_lower);
        }

        /// <summary>Table "index -> same index" for all 65 536 colour addresses.</summary>
        private static ushort[] Identity()
        {
            var a = new ushort[65536];
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = (ushort)i;
            }
            return a;
        }

        /// <summary>Call after the map was changed from outside (generator, undo …).</summary>
        public void InvalidateMap()
        {
            _viewDirty = true;
            Invalidate();
        }

        // ------------------------------------------------------------------ view

        public void ZoomToFit()
        {
            if (_map == null || Width <= 0 || Height <= 0)
            {
                return;
            }
            _zoom = Math.Min((double)Width / _map.Width, (double)Height / _map.Height);
            _zoom = Math.Max(1.0 / 64, _zoom);
            _originX = (_map.Width - Width / _zoom) / 2;
            _originY = (_map.Height - Height / _zoom) / 2;
            OnViewChanged();
        }

        public void SetZoom(double zoom, Point? anchor = null)
        {
            if (_map == null)
            {
                return;
            }
            Point a = anchor ?? new Point(Width / 2, Height / 2);
            double mx = _originX + a.X / _zoom, my = _originY + a.Y / _zoom;
            _zoom = Math.Clamp(zoom, 1.0 / 64, 64);
            _originX = mx - a.X / _zoom;
            _originY = my - a.Y / _zoom;
            OnViewChanged();
        }

        public void ZoomStep(int direction, Point? anchor = null)
        {
            double[] steps = { 1.0 / 64, 1.0 / 32, 1.0 / 16, 1.0 / 8, 1.0 / 6, 1.0 / 4, 1.0 / 3, 1.0 / 2, 2.0 / 3, 1, 1.5, 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64 };
            int i = 0;
            while (i < steps.Length - 1 && steps[i] < _zoom - 1e-9)
            {
                i++;
            }
            if (direction > 0)
            {
                i = steps[i] > _zoom + 1e-9 ? i : Math.Min(steps.Length - 1, i + 1);
            }
            else
            {
                i = Math.Max(0, i - 1);
            }
            SetZoom(steps[i], anchor);
        }

        /// <summary>Centres the view on a map position.</summary>
        public void CenterOn(double mapX, double mapY)
        {
            _originX = mapX - Width / _zoom / 2;
            _originY = mapY - Height / _zoom / 2;
            OnViewChanged();
        }

        private void OnViewChanged()
        {
            _viewDirty = true;
            Invalidate();
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        public Point ScreenToMap(Point p) => new Point((int)Math.Floor(_originX + p.X / _zoom), (int)Math.Floor(_originY + p.Y / _zoom));

        private PointF MapToScreen(double x, double y) => new PointF((float)((x - _originX) * _zoom), (float)((y - _originY) * _zoom));

        // ------------------------------------------------------------------ painting

        private unsafe void RenderView()
        {
            if (_view == null || _view.Width != Width || _view.Height != Height)
            {
                _view?.Dispose();
                _view = new Bitmap(Math.Max(1, Width), Math.Max(1, Height), PixelFormat.Format32bppRgb);
            }
            int w = _view.Width, h = _view.Height;
            BitmapData bd = _view.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            try
            {
                int bg = BackColor.ToArgb();
                var colMap = new int[w];
                for (int sx = 0; sx < w; sx++)
                {
                    double mx = Math.Floor(_originX + sx / _zoom);
                    colMap[sx] = mx < 0 || mx >= _map.Width ? -1 : (int)mx;
                }
                ushort[] pixels = _map.Pixels;
                int mapW = _map.Width;
                fixed (int* pal = _argb)
                fixed (ushort* src = pixels)
                {
                    for (int sy = 0; sy < h; sy++)
                    {
                        int* row = (int*)((byte*)bd.Scan0 + sy * bd.Stride);
                        double my = Math.Floor(_originY + sy / _zoom);
                        if (my < 0 || my >= _map.Height)
                        {
                            for (int sx = 0; sx < w; sx++)
                            {
                                row[sx] = bg;
                            }
                            continue;
                        }
                        ushort* line = src + (long)my * mapW;
                        for (int sx = 0; sx < w; sx++)
                        {
                            int mx = colMap[sx];
                            row[sx] = mx < 0 ? bg : pal[line[mx]];
                        }
                    }
                }
            }
            finally
            {
                _view.UnlockBits(bd);
            }
            _viewDirty = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (_map == null)
            {
                g.Clear(BackColor);
                TextRenderer.DrawText(g, "Keine Karte geöffnet – Datei → Neue Karte oder Karte öffnen.", Font, new Point(12, 12), Color.Gainsboro);
                return;
            }
            if (_viewDirty)
            {
                RenderView();
            }
            g.DrawImageUnscaled(_view, 0, 0);

            DrawGrid(g);
            DrawHeightLayer(g);
            DrawHeights(g);
            DrawOverlay(g);
        }

        private void DrawGrid(Graphics g)
        {
            if (!ShowGrid || _zoom < GridFromZoom)
            {
                return;
            }
            RectangleF v = ViewRect;
            int x0 = Math.Max(0, (int)Math.Floor(v.Left)), x1 = Math.Min(_map.Width, (int)Math.Ceiling(v.Right));
            int y0 = Math.Max(0, (int)Math.Floor(v.Top)), y1 = Math.Min(_map.Height, (int)Math.Ceiling(v.Bottom));
            using var block = new Pen(Color.FromArgb(110, 0, 0, 0));
            using var cell = new Pen(Color.FromArgb(35, 0, 0, 0));
            bool cells = _zoom >= 16;
            for (int x = x0; x <= x1; x++)
            {
                if (x % 8 == 0 || cells)
                {
                    float sx = MapToScreen(x, 0).X;
                    g.DrawLine(x % 8 == 0 ? block : cell, sx, MapToScreen(0, y0).Y, sx, MapToScreen(0, y1).Y);
                }
            }
            for (int y = y0; y <= y1; y++)
            {
                if (y % 8 == 0 || cells)
                {
                    float sy = MapToScreen(0, y).Y;
                    g.DrawLine(y % 8 == 0 ? block : cell, MapToScreen(x0, 0).X, sy, MapToScreen(x1, 0).X, sy);
                }
            }
        }

        /// <summary>
        /// Cells of the height layer: cyan frame = the height counts (the cell still has the colour), grey = old entry
        /// (painted over with another colour, ignored by the build). From zoom 12 with the height inside.
        /// </summary>
        private void DrawHeightLayer(Graphics g)
        {
            IReadOnlyDictionary<(int x, int y), (sbyte z, ushort colour)> layer = HeightLayer?.Invoke();
            if (!ShowHeightLayer || layer == null || layer.Count == 0 || _zoom < 1)
            {
                return;
            }
            RectangleF v = ViewRect;
            float cell = (float)_zoom;
            using var active = new SolidBrush(Color.FromArgb(90, 0, 220, 255));
            using var stale = new SolidBrush(Color.FromArgb(70, 160, 160, 160));
            using var frame = new Pen(Color.FromArgb(200, 0, 200, 255));
            using var font = _zoom >= 12 ? new Font("Segoe UI", Math.Min(8f, (float)(_zoom * 0.26)), FontStyle.Bold) : null;
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
            int drawn = 0;
            foreach (KeyValuePair<(int x, int y), (sbyte z, ushort colour)> kv in layer)
            {
                (int x, int y) = kv.Key;
                if (x < v.Left - 1 || y < v.Top - 1 || x > v.Right || y > v.Bottom || !_map.InBounds(x, y))
                {
                    continue;
                }
                bool on = _map[x, y] == kv.Value.colour;
                PointF a = MapToScreen(x, y);
                g.FillRectangle(on ? active : stale, a.X, a.Y, cell, cell);
                if (on && cell >= 4)
                {
                    g.DrawRectangle(frame, a.X + 0.5f, a.Y + 0.5f, cell - 1, cell - 1);
                }
                if (font != null && !ShowHeights)
                {
                    string t = "z" + kv.Value.z;
                    g.DrawString(t, font, Brushes.Black, new RectangleF(a.X + 1, a.Y + 1, cell, cell - 1), sf);
                    g.DrawString(t, font, on ? Brushes.Gold : Brushes.Silver, new RectangleF(a.X, a.Y, cell, cell - 1), sf);
                }
                if (++drawn > 60000)
                {
                    break;
                }
            }
        }

        private void DrawHeights(Graphics g)
        {
            if (!ShowHeights || _zoom < 12)
            {
                return;
            }
            RectangleF v = ViewRect;
            int x0 = Math.Max(0, (int)Math.Floor(v.Left)), x1 = Math.Min(_map.Width - 1, (int)Math.Ceiling(v.Right));
            int y0 = Math.Max(0, (int)Math.Floor(v.Top)), y1 = Math.Min(_map.Height - 1, (int)Math.Ceiling(v.Bottom));
            using var font = new Font("Segoe UI", Math.Min(9f, (float)(_zoom * 0.32)), FontStyle.Bold);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            IReadOnlyDictionary<(int x, int y), (sbyte z, ushort colour)> layer = HeightLayer?.Invoke();
            if (layer != null && layer.Count == 0)
            {
                layer = null;
            }
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    ushort colour = _map[x, y];
                    int z = _altitudes[colour];
                    bool fromLayer = false;
                    if (layer != null && layer.TryGetValue((x, y), out (sbyte z, ushort colour) h) && h.colour == colour)
                    {
                        z = h.z;
                        fromLayer = true;
                    }
                    PointF c = MapToScreen(x + 0.5, y + 0.5);
                    string t = z.ToString();
                    g.DrawString(t, font, Brushes.Black, c.X + 1, c.Y + 1, sf);
                    g.DrawString(t, font, fromLayer ? Brushes.Gold : Brushes.White, c.X, c.Y, sf);
                }
            }
        }

        private void DrawOverlay(Graphics g)
        {
            // selection
            if (!_selection.IsEmpty)
            {
                PointF a = MapToScreen(_selection.Left, _selection.Top);
                PointF b = MapToScreen(_selection.Right, _selection.Bottom);
                using var pen = new Pen(Color.White) { DashStyle = DashStyle.Dash };
                g.DrawRectangle(Pens.Black, a.X, a.Y, b.X - a.X, b.Y - a.Y);
                g.DrawRectangle(pen, a.X, a.Y, b.X - a.X, b.Y - a.Y);
            }

            DrawStaticMarkers(g);

            if (_cursor.X == -1 && _cursor.Y == -1)
            {
                return;
            }

            // paste preview
            if (_pasteMode && _pasteBuffer != null)
            {
                PointF a = MapToScreen(_cursor.X, _cursor.Y);
                using Bitmap prev = IndexedBitmapIO.ToBitmap(_pasteBuffer, PaletteColors(), 512);
                var attr = new ImageAttributes();
                attr.SetColorMatrix(new ColorMatrix { Matrix33 = 0.7f });
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                var dest = new Rectangle((int)a.X, (int)a.Y, (int)(_pasteBuffer.Width * _zoom), (int)(_pasteBuffer.Height * _zoom));
                g.DrawImage(prev, dest, 0, 0, prev.Width, prev.Height, GraphicsUnit.Pixel, attr);
                g.DrawRectangle(Pens.Yellow, dest);
                return;
            }

            // point to point of the tool "Muster": the path so far + the mouse
            if (MotifPoints && _motifPoints.Count > 0)
            {
                var pts = _motifPoints.ToList();
                if (_map.InBounds(_cursor.X, _cursor.Y))
                {
                    pts.Add(_cursor);
                }
                IReadOnlyCollection<Point> cells = MotifPathPreview?.Invoke(pts);
                if (cells != null)
                {
                    using var fill = new SolidBrush(Color.FromArgb(140, 251, 191, 36));
                    foreach (Point c in cells)
                    {
                        PointF c0 = MapToScreen(c.X, c.Y), c1 = MapToScreen(c.X + 1, c.Y + 1);
                        g.FillRectangle(fill, c0.X, c0.Y, Math.Max(1, c1.X - c0.X), Math.Max(1, c1.Y - c0.Y));
                    }
                }
                using var line = new Pen(Color.Orange, 2);
                PointF[] sp = Motif.SnapPath(pts).Select(q => MapToScreen(q.X + 0.5, q.Y + 0.5)).ToArray();
                if (sp.Length > 1)
                {
                    g.DrawLines(line, sp);
                }
                foreach (PointF q in sp)
                {
                    g.FillEllipse(Brushes.Orange, q.X - 3, q.Y - 3, 6, 6);
                }
            }

            // shape preview
            if (_drawing && IsShapeTool(_tool))
            {
                using var pen = new Pen(Color.Yellow) { DashStyle = DashStyle.Dot };
                Rectangle r = NormRect(_start, _cursor);
                PointF a = MapToScreen(r.Left, r.Top);
                PointF b = MapToScreen(r.Right + 1, r.Bottom + 1);
                switch (_tool)
                {
                    case MapTool.Line:
                    case MapTool.SlopeLine:
                        PointF s = MapToScreen(_start.X + 0.5, _start.Y + 0.5), t = MapToScreen(_cursor.X + 0.5, _cursor.Y + 0.5);
                        g.DrawLine(pen, s, t);
                        if (_tool == MapTool.SlopeLine)
                        {
                            using var arrow = new Pen(Color.Orange, 2) { EndCap = LineCap.ArrowAnchor };
                            g.DrawLine(arrow, s, t);
                        }
                        break;
                    case MapTool.Motif:
                        {
                            IReadOnlyCollection<Point> cells = MotifPreview?.Invoke(_start, _cursor);
                            if (cells != null)
                            {
                                using var fill = new SolidBrush(Color.FromArgb(140, 251, 191, 36));
                                foreach (Point c in cells)
                                {
                                    PointF c0 = MapToScreen(c.X, c.Y), c1 = MapToScreen(c.X + 1, c.Y + 1);
                                    g.FillRectangle(fill, c0.X, c0.Y, Math.Max(1, c1.X - c0.X), Math.Max(1, c1.Y - c0.Y));
                                }
                            }
                            using var arrow = new Pen(Color.Orange, 2) { EndCap = LineCap.ArrowAnchor };
                            g.DrawLine(arrow, MapToScreen(_start.X + 0.5, _start.Y + 0.5), MapToScreen(_cursor.X + 0.5, _cursor.Y + 0.5));
                            break;
                        }
                    case MapTool.HalfCircle:
                    case MapTool.Triangle:
                        {
                            PointF[] poly = ShapeOutline(_start, _cursor).Select(p => MapToScreen(p.X, p.Y)).ToArray();
                            if (poly.Length > 2)
                            {
                                g.DrawPolygon(pen, poly);
                            }
                            break;
                        }
                    case MapTool.Course:
                        {
                            var pts = Builders.Course(new PointF(_start.X + 0.5f, _start.Y + 0.5f), new PointF(_cursor.X + 0.5f, _cursor.Y + 0.5f), Course, new Random(_previewSeed))
                                              .Select(p => MapToScreen(p.x, p.y)).ToArray();
                            if (pts.Length > 1)
                            {
                                for (int i = 1; i < pts.Length; i++)
                                {
                                    using var wide = new Pen(Color.FromArgb(120, 80, 160, 255), (float)Math.Max(2, Course.WidthAt(i / (double)(pts.Length - 1), BrushSize) * _zoom)) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                                    g.DrawLine(wide, pts[i - 1], pts[i]);
                                }
                            }
                            break;
                        }
                    case MapTool.Ellipse:
                    case MapTool.FilledEllipse:
                        g.DrawEllipse(pen, a.X, a.Y, b.X - a.X, b.Y - a.Y);
                        break;
                    default:
                        g.DrawRectangle(pen, a.X, a.Y, b.X - a.X, b.Y - a.Y);
                        break;
                }
            }
            if (_drawing && _tool == MapTool.Select)
            {
                Rectangle r = NormRect(_start, _cursor);
                PointF a = MapToScreen(r.Left, r.Top);
                PointF b = MapToScreen(r.Right + 1, r.Bottom + 1);
                using var pen = new Pen(Color.Cyan) { DashStyle = DashStyle.Dash };
                g.DrawRectangle(pen, a.X, a.Y, b.X - a.X, b.Y - a.Y);
            }

            // brush outline
            if (UsesBrush(_tool))
            {
                int size = Math.Max(1, BrushSize);
                int half = size / 2;
                PointF a = MapToScreen(_cursor.X - half, _cursor.Y - half);
                float d = (float)(size * _zoom);
                using var pen = new Pen(Color.FromArgb(220, 255, 255, 255));
                if (SquareBrush || size <= 2)
                {
                    g.DrawRectangle(Pens.Black, a.X - 1, a.Y - 1, d + 2, d + 2);
                    g.DrawRectangle(pen, a.X, a.Y, d, d);
                }
                else
                {
                    g.DrawEllipse(Pens.Black, a.X - 1, a.Y - 1, d + 2, d + 2);
                    g.DrawEllipse(pen, a.X, a.Y, d, d);
                }
            }
        }

        private Color[] PaletteColors()
        {
            var c = new Color[65536];
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = Color.FromArgb(_argb[i]);
            }
            return c;
        }

        private static bool UsesBrush(MapTool t) => t == MapTool.Brush || t == MapTool.Eraser || t == MapTool.Spray || t == MapTool.Raise || t == MapTool.Lower || t == MapTool.PatternBrush;

        private static bool IsShapeTool(MapTool t) =>
            t == MapTool.Line || t == MapTool.Rectangle || t == MapTool.FilledRectangle || t == MapTool.Ellipse || t == MapTool.FilledEllipse ||
            t == MapTool.HalfCircle || t == MapTool.Triangle || t == MapTool.SlopeLine || t == MapTool.Course || t == MapTool.Dungeon || t == MapTool.Motif;

        private static Rectangle NormRect(Point a, Point b) =>
            Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));

        // ------------------------------------------------------------------ mouse

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            ZoomStep(e.Delta > 0 ? 1 : -1, e.Location);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            Keys mods = ModifierKeys & (Keys.Control | Keys.Alt | Keys.Shift);
            if (CaptureNextButton)
            {
                CaptureNextButton = false;
                Point cp = ScreenToMap(e.Location);
                ButtonCaptured?.Invoke(this, new MapClickEventArgs(cp.X, cp.Y, e.Button, mods));
                return;
            }
            if (_map == null)
            {
                return;
            }
            if (JumpButton != MouseButtons.None && e.Button == JumpButton && mods == JumpModifiers)
            {
                Point jp = ScreenToMap(e.Location);
                if (_map.InBounds(jp.X, jp.Y))
                {
                    JumpClicked?.Invoke(this, new MapClickEventArgs(jp.X, jp.Y, e.Button, mods));
                }
                return;
            }
            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && (_spaceDown || _tool == MapTool.Pan)))
            {
                _panning = true;
                _panStart = e.Location;
                _panOriginX = _originX;
                _panOriginY = _originY;
                Cursor = Cursors.SizeAll;
                return;
            }
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right)
            {
                return;
            }

            Point p = ScreenToMap(e.Location);
            ushort color = (ushort)(e.Button == MouseButtons.Left ? PrimaryIndex : SecondaryIndex);

            if (MotifPoints && (ModifierKeys & Keys.Shift) == 0)
            {
                if (e.Button == MouseButtons.Right || e.Clicks >= 2)
                {
                    FinishMotifPoints();
                }
                else if (_map.InBounds(p.X, p.Y))
                {
                    _motifPoints.Add(p);
                    Invalidate();
                }
                return;
            }

            if (e.Button == MouseButtons.Right && !(_pasteMode && _pasteBuffer != null) &&
                ((ModifierKeys & Keys.Shift) != 0 || _tool == MapTool.Select || _tool == MapTool.Pan || _tool == MapTool.Picker))
            {
                ContextRequested?.Invoke(this, new MapClickEventArgs(p.X, p.Y, e.Button, ModifierKeys));
                return;
            }

            if (_pasteMode && _pasteBuffer != null)
            {
                if (e.Button == MouseButtons.Right)
                {
                    PasteTransform = null;
                    PasteRepeat = false;
                    PasteName = "Einfügen";
                    PasteEnded?.Invoke(this, EventArgs.Empty);
                    _pasteMode = false;
                    Invalidate();
                    return;
                }
                PasteAt(p);
                return;
            }

            switch (_tool)
            {
                case MapTool.CaveStamp:
                    if (_map.InBounds(p.X, p.Y))
                    {
                        ToolClicked?.Invoke(this, new MapClickEventArgs(p.X, p.Y, e.Button, ModifierKeys));
                    }
                    return;
                case MapTool.Picker:
                    if (_map.InBounds(p.X, p.Y))
                    {
                        ColorPicked?.Invoke(this, new PaletteIndexEventArgs(_map[p.X, p.Y], e.Button));
                    }
                    return;
                case MapTool.Fill:
                    if (_map.InBounds(p.X, p.Y))
                    {
                        _history?.Begin("Füllen");
                        FloodFill(p.X, p.Y, color);
                        EndEdit();
                    }
                    return;
            }

            _drawing = true;
            _drawButton = e.Button;
            _start = p;
            _last = p;
            _cursor = p;
            if (_tool == MapTool.Select)
            {
                return;
            }
            if (!IsShapeTool(_tool))
            {
                _strokeGroup = _history?.NewGroup() ?? 0;
                _stepCells = 0;
                _patternDone.Clear();
                _history?.Begin(ToolName(_tool), StrokeStepCells > 0 ? _strokeGroup : 0);
                if (_tool == MapTool.PatternBrush)
                {
                    PatternStamp(p.X, p.Y);
                }
                else
                {
                    Stamp(p.X, p.Y, color, e.Button);
                }
            }
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_map == null)
            {
                return;
            }
            if (_panning)
            {
                _originX = _panOriginX - (e.X - _panStart.X) / _zoom;
                _originY = _panOriginY - (e.Y - _panStart.Y) / _zoom;
                OnViewChanged();
                return;
            }
            Point p = ScreenToMap(e.Location);
            bool moved = p != _cursor;
            _cursor = p;
            if (moved)
            {
                CursorMoved?.Invoke(this, new MapPointEventArgs(p.X, p.Y, _map.InBounds(p.X, p.Y) ? _map[p.X, p.Y] : -1));
            }

            if (_drawing && !IsShapeTool(_tool) && _tool != MapTool.Select)
            {
                ushort color = (ushort)(_drawButton == MouseButtons.Left ? PrimaryIndex : SecondaryIndex);
                StrokeLine(_last, p, color, _drawButton);
                _last = p;
            }
            if (moved)
            {
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_panning)
            {
                _panning = false;
                Tool = _tool;
                return;
            }
            if (!_drawing || _map == null)
            {
                return;
            }
            _drawing = false;
            Point p = ScreenToMap(e.Location);
            ushort color = (ushort)(_drawButton == MouseButtons.Left ? PrimaryIndex : SecondaryIndex);

            if (_tool == MapTool.Select)
            {
                Rectangle r = NormRect(_start, p);
                r = new Rectangle(r.X, r.Y, r.Width + 1, r.Height + 1);
                r.Intersect(_map.Bounds);
                _selection = r.Width > 1 || r.Height > 1 ? r : Rectangle.Empty;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
                return;
            }
            if (IsShapeTool(_tool))
            {
                _history?.Begin(ToolName(_tool));
                DrawShape(_start, p, color);
            }
            if (_tool == MapTool.PatternBrush)
            {
                StrokeFinishing?.Invoke(this, EventArgs.Empty);
            }
            EndEdit();
            _patternDone.Clear();
            StrokeEnded?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (!_drawing)
            {
                _cursor = new Point(-1, -1);
                Invalidate();
            }
        }

        // ------------------------------------------------------------------ keyboard

        protected override bool IsInputKey(Keys keyData)
        {
            Keys k = keyData & Keys.KeyCode;
            return k == Keys.Left || k == Keys.Right || k == Keys.Up || k == Keys.Down || k == Keys.Space || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (_motifPoints.Count > 0 && MotifPoints)
            {
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        FinishMotifPoints();
                        e.Handled = true;
                        return;
                    case Keys.Back:
                        _motifPoints.RemoveAt(_motifPoints.Count - 1);
                        Invalidate();
                        e.Handled = true;
                        return;
                    case Keys.Escape:
                        _motifPoints.Clear();
                        Invalidate();
                        e.Handled = true;
                        return;
                }
            }
            double step = 64 / _zoom;
            switch (e.KeyCode)
            {
                case Keys.Space:
                    _spaceDown = true;
                    Cursor = Cursors.Hand;
                    break;
                case Keys.Left:
                    _originX -= step;
                    OnViewChanged();
                    break;
                case Keys.Right:
                    _originX += step;
                    OnViewChanged();
                    break;
                case Keys.Up:
                    _originY -= step;
                    OnViewChanged();
                    break;
                case Keys.Down:
                    _originY += step;
                    OnViewChanged();
                    break;
                case Keys.Escape:
                    if (_pasteMode)
                    {
                        EndPaste();
                        PasteEnded?.Invoke(this, EventArgs.Empty);
                    }
                    _pasteMode = false;
                    _selection = Rectangle.Empty;
                    if (_drawing)
                    {
                        _drawing = false;
                        _history?.Cancel();
                        InvalidateMap();
                    }
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode == Keys.Space)
            {
                _spaceDown = false;
                Tool = _tool;
            }
        }

        // ------------------------------------------------------------------ editing

        private static string ToolName(MapTool t)
        {
            switch (t)
            {
                case MapTool.Brush: return "Pinsel";
                case MapTool.Eraser: return "Radierer";
                case MapTool.Spray: return "Sprühen";
                case MapTool.Line: return "Linie";
                case MapTool.Rectangle: return "Rechteck";
                case MapTool.FilledRectangle: return "Rechteck gefüllt";
                case MapTool.Ellipse: return "Ellipse";
                case MapTool.FilledEllipse: return "Ellipse gefüllt";
                case MapTool.Raise: return "Höhe +";
                case MapTool.Lower: return "Höhe −";
                case MapTool.CaveStamp: return "Höhleneingang";
                case MapTool.PatternBrush: return "Muster-Pinsel";
                case MapTool.HalfCircle: return "Halbkreis";
                case MapTool.Triangle: return "Dreieck";
                case MapTool.SlopeLine: return "Neigungs-Linie";
                case MapTool.Course: return "Natürlicher Lauf";
                case MapTool.Dungeon: return "Dungeon";
                case MapTool.Motif: return "Muster";
                default: return t.ToString();
            }
        }

        private void EndEdit()
        {
            StepClosing?.Invoke(this, EventArgs.Empty);
            _history?.End();
            MapEdited?.Invoke(this, EventArgs.Empty);
        }

        private void Changed(Rectangle r)
        {
            // fast path: only the changed screen area is re-rendered on the next paint
            _viewDirty = true;
            PointF a = MapToScreen(r.Left, r.Top);
            PointF b = MapToScreen(r.Right, r.Bottom);
            Invalidate(Rectangle.FromLTRB((int)a.X - 2, (int)a.Y - 2, (int)Math.Ceiling(b.X) + 2, (int)Math.Ceiling(b.Y) + 2));
        }

        private void StrokeLine(Point a, Point b, ushort color, MouseButtons button)
        {
            int dx = Math.Abs(b.X - a.X), dy = Math.Abs(b.Y - a.Y);
            int steps = Math.Max(dx, dy);
            int spacing = _tool == MapTool.Spray ? Math.Max(1, BrushSize / 3) : Math.Max(1, BrushSize / 4);
            if (steps == 0)
            {
                return;
            }
            Action<int, int> dab = _tool == MapTool.PatternBrush ? (x, y) => PatternStamp(x, y) : (x, y) => Stamp(x, y, color, button);
            for (int s = spacing; s <= steps; s += spacing)
            {
                int x = a.X + (b.X - a.X) * s / steps, y = a.Y + (b.Y - a.Y) * s / steps;
                dab(x, y);
            }
            if (steps % spacing != 0)
            {
                dab(b.X, b.Y);
            }
        }

        /// <summary>One brush stamp at (cx, cy).</summary>
        private void Stamp(int cx, int cy, ushort color, MouseButtons button)
        {
            int size = Math.Max(1, BrushSize);
            int half = size / 2;
            var r = new Rectangle(cx - half, cy - half, size, size);
            r.Intersect(_map.Bounds);
            if (r.Width <= 0 || r.Height <= 0)
            {
                return;
            }
            _history?.Touch(r);
            bool round = !SquareBrush && size > 2;
            double radius = size / 2.0;
            double ccx = cx - half + radius, ccy = cy - half + radius;
            ushort[] heightTable = _tool == MapTool.Raise ? (button == MouseButtons.Right ? _lower : _raise)
                : _tool == MapTool.Lower ? (button == MouseButtons.Right ? _raise : _lower) : null;
            ushort value = _tool == MapTool.Eraser ? (ushort)SecondaryIndex : color;
            int sprayChance = Math.Clamp(SprayDensity, 1, 100);

            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    if (round)
                    {
                        double ddx = x + 0.5 - ccx, ddy = y + 0.5 - ccy;
                        if (ddx * ddx + ddy * ddy > radius * radius)
                        {
                            continue;
                        }
                    }
                    if (_tool == MapTool.Spray && _rnd.Next(100) >= sprayChance)
                    {
                        continue;
                    }
                    int i = y * _map.Width + x;
                    _map.Pixels[i] = heightTable != null ? heightTable[_map.Pixels[i]] : PaintCell(x, y, _map.Pixels[i], value);
                }
            }
            Changed(r);
            if (_drawing)
            {
                SplitStrokeStep(r.Width * r.Height);
                Painting?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetPixel(int x, int y, ushort color)
        {
            if (_map.InBounds(x, y))
            {
                int i = y * _map.Width + x;
                _map.Pixels[i] = PaintCell(x, y, _map.Pixels[i], color);
            }
        }

        private void DrawShape(Point a, Point b, ushort color)
        {
            if (_tool == MapTool.Motif)
            {
                // statics only: the form makes its own undo step (no pixels touched here)
                MotifRequested?.Invoke(a, b);
                return;
            }
            Rectangle r = NormRect(a, b);
            var touch = new Rectangle(r.X - BrushSize, r.Y - BrushSize, r.Width + 2 * BrushSize + 1, r.Height + 2 * BrushSize + 1);
            _history?.Touch(touch);
            int thick = Math.Max(1, _tool == MapTool.Line ? BrushSize : Math.Min(BrushSize, 64));

            if (_tool == MapTool.Dungeon)
            {
                Rectangle d = Rectangle.FromLTRB(r.Left, r.Top, r.Right + 1, r.Bottom + 1);
                d.Intersect(_map.Bounds);
                if (d.Width >= 8 && d.Height >= 8)
                {
                    DungeonRequested?.Invoke(this, d);
                }
                return;
            }
            if (_tool == MapTool.HalfCircle || _tool == MapTool.Triangle)
            {
                Rectangle box = DrawPolygonShape(a, b, color, thick);
                _history?.Touch(box);
                Changed(box);
                return;
            }
            if (_tool == MapTool.SlopeLine)
            {
                DrawSlopeLine(a, b, color);
                Changed(touch);
                return;
            }
            if (_tool == MapTool.Course)
            {
                Rectangle box = DrawCourse(a, b, color);
                Changed(box);
                return;
            }
            switch (_tool)
            {
                case MapTool.Line:
                    {
                        var saveTool = _tool;
                        _tool = MapTool.Brush;
                        Stamp(a.X, a.Y, color, MouseButtons.Left);
                        StrokeLine(a, b, color, MouseButtons.Left);
                        _tool = saveTool;
                        break;
                    }
                case MapTool.FilledRectangle:
                    for (int y = r.Top; y <= r.Bottom; y++)
                    {
                        for (int x = r.Left; x <= r.Right; x++)
                        {
                            SetPixel(x, y, color);
                        }
                    }
                    break;
                case MapTool.Rectangle:
                    for (int y = r.Top; y <= r.Bottom; y++)
                    {
                        for (int x = r.Left; x <= r.Right; x++)
                        {
                            if (x - r.Left < thick || r.Right - x < thick || y - r.Top < thick || r.Bottom - y < thick)
                            {
                                SetPixel(x, y, color);
                            }
                        }
                    }
                    break;
                case MapTool.Ellipse:
                case MapTool.FilledEllipse:
                    {
                        double rx = (r.Width + 1) / 2.0, ry = (r.Height + 1) / 2.0;
                        double cx = r.Left + rx, cy = r.Top + ry;
                        double irx = Math.Max(0, rx - thick), iry = Math.Max(0, ry - thick);
                        for (int y = r.Top; y <= r.Bottom; y++)
                        {
                            for (int x = r.Left; x <= r.Right; x++)
                            {
                                double nx = (x + 0.5 - cx) / rx, ny = (y + 0.5 - cy) / ry;
                                if (nx * nx + ny * ny > 1)
                                {
                                    continue;
                                }
                                if (_tool == MapTool.Ellipse && irx > 0 && iry > 0)
                                {
                                    double ix = (x + 0.5 - cx) / irx, iy = (y + 0.5 - cy) / iry;
                                    if (ix * ix + iy * iy < 1)
                                    {
                                        continue;
                                    }
                                }
                                SetPixel(x, y, color);
                            }
                        }
                        break;
                    }
            }
            Changed(touch);
        }

        private int _previewSeed = Environment.TickCount;

        /// <summary>Corners of the half circle (A = middle of the flat side, B = top of the round side) or of the triangle (A = middle of the base, B = tip).</summary>
        private List<PointF> ShapeOutline(Point a, Point b)
        {
            var pts = new List<PointF>();
            double ax = a.X + 0.5, ay = a.Y + 0.5, dx = b.X - a.X, dy = b.Y - a.Y, len = Math.Sqrt(dx * dx + dy * dy);
            if (len < 1)
            {
                return pts;
            }
            double ux = dx / len, uy = dy / len, nx = -uy, ny = ux;
            if (_tool == MapTool.HalfCircle)
            {
                for (int k = 0; k <= 32; k++)
                {
                    double ang = Math.PI * k / 32 - Math.PI / 2;
                    double c = Math.Cos(ang), si = Math.Sin(ang);
                    pts.Add(new PointF((float)(ax + (ux * c + nx * si) * len), (float)(ay + (uy * c + ny * si) * len)));
                }
            }
            else
            {
                double half = len * 0.58;
                pts.Add(new PointF((float)(ax + nx * half), (float)(ay + ny * half)));
                pts.Add(new PointF((float)(b.X + 0.5), (float)(b.Y + 0.5)));
                pts.Add(new PointF((float)(ax - nx * half), (float)(ay - ny * half)));
            }
            return pts;
        }

        /// <summary>Fills the half circle / triangle (or its outline with the given thickness). Returns the touched box.</summary>
        private Rectangle DrawPolygonShape(Point a, Point b, ushort color, int thick)
        {
            List<PointF> poly = ShapeOutline(a, b);
            if (poly.Count < 3)
            {
                return Rectangle.Empty;
            }
            var box = Rectangle.FromLTRB((int)Math.Floor(poly.Min(p => p.X)) - 1, (int)Math.Floor(poly.Min(p => p.Y)) - 1,
                (int)Math.Ceiling(poly.Max(p => p.X)) + 2, (int)Math.Ceiling(poly.Max(p => p.Y)) + 2);
            box.Intersect(_map.Bounds);
            _history?.Touch(box);
            for (int y = box.Top; y < box.Bottom; y++)
            {
                for (int x = box.Left; x < box.Right; x++)
                {
                    double px = x + 0.5, py = y + 0.5;
                    if (!Inside(poly, px, py))
                    {
                        continue;
                    }
                    if (!ShapeFilled && EdgeDistance(poly, px, py) >= Math.Max(1, thick))
                    {
                        continue;
                    }
                    SetPixel(x, y, color);
                }
            }
            return box;
        }

        private static bool Inside(List<PointF> poly, double px, double py)
        {
            bool inside = false;
            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                if ((poly[i].Y > py) != (poly[j].Y > py) && px < (poly[j].X - poly[i].X) * (py - poly[i].Y) / (poly[j].Y - poly[i].Y) + poly[i].X)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private static double EdgeDistance(List<PointF> poly, double px, double py)
        {
            double best = double.MaxValue;
            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                double ex = poly[i].X - poly[j].X, ey = poly[i].Y - poly[j].Y, l2 = ex * ex + ey * ey;
                double t = l2 == 0 ? 0 : Math.Clamp(((px - poly[j].X) * ex + (py - poly[j].Y) * ey) / l2, 0, 1);
                double qx = poly[j].X + ex * t - px, qy = poly[j].Y + ey * t - py;
                best = Math.Min(best, Math.Sqrt(qx * qx + qy * qy));
            }
            return best;
        }

        /// <summary>Line with brush width whose colour follows the height from a to b (see <see cref="SlopeColour"/>).</summary>
        private void DrawSlopeLine(Point a, Point b, ushort color)
        {
            int size = Math.Max(1, BrushSize);
            double half = size / 2.0;
            double dx = b.X - a.X, dy = b.Y - a.Y, l2 = Math.Max(1e-9, dx * dx + dy * dy);
            var box = Rectangle.FromLTRB(Math.Min(a.X, b.X) - size, Math.Min(a.Y, b.Y) - size, Math.Max(a.X, b.X) + size + 1, Math.Max(a.Y, b.Y) + size + 1);
            box.Intersect(_map.Bounds);
            _history?.Touch(box);
            for (int y = box.Top; y < box.Bottom; y++)
            {
                for (int x = box.Left; x < box.Right; x++)
                {
                    double t = Math.Clamp(((x - a.X) * dx + (y - a.Y) * dy) / l2, 0, 1);
                    double qx = a.X + dx * t - x, qy = a.Y + dy * t - y;
                    if (qx * qx + qy * qy > half * half + 0.25)
                    {
                        continue;
                    }
                    int i = y * _map.Width + x;
                    ushort cur = _map.Pixels[i];
                    _map.Pixels[i] = SlopeColour?.Invoke(x, y, cur, color, t, a, b) ?? PaintCell(x, y, cur, color);
                }
            }
        }

        /// <summary>Natural course from a to b: brush dabs (or pattern stamp dabs) along a winding line, wider towards the end.</summary>
        private Rectangle DrawCourse(Point a, Point b, ushort color)
        {
            var pts = Builders.Course(new PointF(a.X + 0.5f, a.Y + 0.5f), new PointF(b.X + 0.5f, b.Y + 0.5f), Course, new Random(Course.Seed != 0 ? Course.Seed : _previewSeed));
            int baseSize = BrushSize;
            bool pattern = CourseWithPattern && PatternColour != null;
            MapTool saveTool = _tool;
            var box = Rectangle.Empty;
            // passes from outside to inside: outer edge, edge, core
            var passes = new List<(ushort colour, int extra, bool pattern)>();
            if (CourseColours.HasValue && !pattern)
            {
                (int core, int edge, int ew, int edge2, int ew2) = CourseColours.Value;
                ushort coreColour = core >= 0 ? (ushort)core : color;
                if (edge2 >= 0 && ew2 > 0)
                {
                    passes.Add(((ushort)edge2, 2 * (Math.Max(0, ew) + ew2), false));
                }
                if (edge >= 0 && ew > 0)
                {
                    passes.Add(((ushort)edge, 2 * ew, false));
                }
                passes.Add((coreColour, 0, false));
            }
            else
            {
                passes.Add((color, 0, pattern));
            }
            try
            {
                foreach ((ushort colour, int extra, bool withPattern) in passes)
                {
                    _tool = withPattern ? MapTool.PatternBrush : MapTool.Brush;
                    _patternDone.Clear();
                    Point last = new Point(int.MinValue, int.MinValue);
                    foreach ((float x, float y, float t) in pts)
                    {
                        int cx = (int)Math.Floor(x), cy = (int)Math.Floor(y);
                        BrushSize = Math.Max(1, (int)Math.Round(Course.WidthAt(t, baseSize)) + extra);
                        int step = Math.Max(1, BrushSize / 4);
                        if (Math.Abs(cx - last.X) < step && Math.Abs(cy - last.Y) < step)
                        {
                            continue;
                        }
                        last = new Point(cx, cy);
                        if (withPattern)
                        {
                            PatternStamp(cx, cy);
                        }
                        else
                        {
                            Stamp(cx, cy, colour, MouseButtons.Left);
                        }
                        var dab = new Rectangle(cx - BrushSize, cy - BrushSize, BrushSize * 2 + 1, BrushSize * 2 + 1);
                        box = box.IsEmpty ? dab : Rectangle.Union(box, dab);
                    }
                }
            }
            finally
            {
                BrushSize = baseSize;
                _tool = saveTool;
                _patternDone.Clear();
            }
            _previewSeed = Environment.TickCount; // next preview / course: a new random course
            box.Intersect(_map.Bounds);
            return box;
        }

        /// <summary>Scanline flood fill (4 neighbours) of the area with the same palette index.</summary>
        private void FloodFill(int sx, int sy, ushort color)
        {
            ushort target = _map[sx, sy];
            if (target == color)
            {
                return;
            }
            int w = _map.Width, h = _map.Height;
            ushort[] px = _map.Pixels;
            var spans = new List<(int y, int x0, int x1)>();
            var visited = new System.Collections.BitArray(w * h);
            var stack = new Stack<Point>();
            stack.Push(new Point(sx, sy));
            int minX = sx, maxX = sx, minY = sy, maxY = sy;
            while (stack.Count > 0)
            {
                Point p = stack.Pop();
                int y = p.Y;
                int x = p.X;
                if (visited[y * w + x] || px[y * w + x] != target)
                {
                    continue;
                }
                int x0 = x, x1 = x;
                while (x0 > 0 && px[y * w + x0 - 1] == target && !visited[y * w + x0 - 1])
                {
                    x0--;
                }
                while (x1 < w - 1 && px[y * w + x1 + 1] == target && !visited[y * w + x1 + 1])
                {
                    x1++;
                }
                for (int k = x0; k <= x1; k++)
                {
                    visited[y * w + k] = true;
                }
                spans.Add((y, x0, x1));
                minX = Math.Min(minX, x0);
                maxX = Math.Max(maxX, x1);
                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);
                for (int ny = y - 1; ny <= y + 1; ny += 2)
                {
                    if (ny < 0 || ny >= h)
                    {
                        continue;
                    }
                    bool inRun = false;
                    for (int k = x0; k <= x1; k++)
                    {
                        bool ok = px[ny * w + k] == target && !visited[ny * w + k];
                        if (ok && !inRun)
                        {
                            stack.Push(new Point(k, ny));
                        }
                        inRun = ok;
                    }
                }
            }
            Rectangle bounds = Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
            _history?.Touch(bounds);
            foreach ((int y, int x0, int x1) in spans)
            {
                for (int k = x0; k <= x1; k++)
                {
                    px[y * w + k] = PaintCell(k, y, px[y * w + k], color);
                }
            }
            Changed(bounds);
        }

        // ------------------------------------------------------------------ selection / clipboard

        public void SelectAll()
        {
            if (_map == null)
            {
                return;
            }
            _selection = _map.Bounds;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        public IndexedMap CopySelection() => _map == null || _selection.IsEmpty ? null : _map.Crop(_selection);

        /// <summary>Fills the selection (Delete = colour 2).</summary>
        public void FillSelection(ushort color, string name)
        {
            if (_map == null || _selection.IsEmpty)
            {
                return;
            }
            _history?.Begin(name);
            _history?.Touch(_selection);
            for (int y = _selection.Top; y < _selection.Bottom; y++)
            {
                for (int x = _selection.Left; x < _selection.Right; x++)
                {
                    int i = y * _map.Width + x;
                    _map.Pixels[i] = PaintCell(x, y, _map.Pixels[i], color);
                }
            }
            Changed(_selection);
            EndEdit();
        }

        /// <summary>Starts paste mode: the buffer follows the mouse, a left click places it (several times), Esc / right click ends.</summary>
        public void BeginPaste(IndexedMap buffer)
        {
            if (buffer == null || _map == null)
            {
                return;
            }
            _pasteBuffer = buffer;
            _pasteMode = true;
            Invalidate();
        }

        private void PasteAt(Point p)
        {
            var r = new Rectangle(p.X, p.Y, _pasteBuffer.Width, _pasteBuffer.Height);
            r.Intersect(_map.Bounds);
            if (r.Width <= 0 || r.Height <= 0)
            {
                return;
            }
            IndexedMap buffer = PasteTransform?.Invoke(_pasteBuffer, p) ?? _pasteBuffer;
            _history?.Begin(PasteName ?? "Einfügen");
            _history?.Touch(r);
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    ushort c = buffer.Pixels[(y - p.Y) * buffer.Width + (x - p.X)];
                    if (c != ushort.MaxValue)
                    {
                        // 0xFFFF = transparent stamp pixel: the map keeps its colour
                        _map.Pixels[y * _map.Width + x] = c;
                    }
                }
            }
            PasteApplied?.Invoke(this, new MapPointEventArgs(p.X, p.Y, -1));
            Changed(r);
            EndEdit();
            if (!PasteRepeat)
            {
                // set once: the stamp leaves the mouse
                EndPaste();
                PasteEnded?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>Paste / stamp mode ended (set once, Esc, right click, Abbrechen).</summary>
        public event EventHandler PasteEnded;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _viewDirty = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _view?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

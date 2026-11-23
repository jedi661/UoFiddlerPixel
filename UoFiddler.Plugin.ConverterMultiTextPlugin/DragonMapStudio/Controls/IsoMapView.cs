// /***************************************************************************
//  *
//  * Dragon Map Studio – isometric "client" view of a built map with static editing
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
    /// <summary>What the "Höhen / Infos" overlay writes on every land tile.</summary>
    public enum IsoLabel
    {
        Height,
        Id,
        IdHeight,
        Full
    }

    public enum IsoTool
    {
        Select,
        Place,
        Multi,
        Delete,
        Hue,
        LandSet,
        Raise,
        Lower,
        SetHeight,
        Smooth,
        Area,

        /// <summary>Brush: coast like the original map (sea floor, water statics, beach heights).</summary>
        Coast,

        /// <summary>Brush: water statics (Ctrl = remove).</summary>
        Water,

        /// <summary>Brush: digs a cave floor at one height into the mountain, the edge becomes a wall.</summary>
        Cave,

        /// <summary>Brush: cave entrance in the mountain side – black (01DB) or invisible (0002) mouth, the edge gets the entrance tiles 01D3–01DA.</summary>
        CaveEntrance,

        /// <summary>Brush: raises a mountain from the foot height (settings in the tab "Berg").</summary>
        Mountain,

        /// <summary>Click: cuts a complete cave entrance into the mountain face (ground and mountain height are taken from the map).</summary>
        CaveStamp,

        /// <summary>Pattern brush from a stamp: land tiles + statics, freehand like a texture (roads, rivers) or once.</summary>
        PatternBrush,

        /// <summary>Places a template (building / floor pattern) from the library.</summary>
        Template,

        /// <summary>Round brush: coast without saw teeth – soft shore heights, straight shore line, statics along.</summary>
        CoastSmooth,

        /// <summary>Brush: scatters a set of statics (trees, bushes, rocks …).</summary>
        Scatter,

        /// <summary>Brush: colours every static under it.</summary>
        HueBrush,

        /// <summary>Brush: river bed with water tiles and soft banks.</summary>
        River,

        /// <summary>Brush: flatten to one height or terraces.</summary>
        Plateau,

        /// <summary>Brush: slight waves in the ground.</summary>
        Noise,

        /// <summary>Brush: random tiles of the same family (no tile pattern).</summary>
        Variants,

        /// <summary>Brush: Dragon transitions only under the brush.</summary>
        TransitionBrush,

        /// <summary>Drag: wall / fence along X or Y.</summary>
        Wall,

        /// <summary>Points: road along a smooth curve.</summary>
        Road,

        /// <summary>Drag: even slope from start to end.</summary>
        Ramp,

        /// <summary>Drag: distance and height difference.</summary>
        Measure,

        /// <summary>Points: polygon selection (mask).</summary>
        Lasso,

        /// <summary>Click: connected cells of the same land (mask).</summary>
        MagicWand,

        /// <summary>Drag: bridge of statics over water (deck, railings, posts, arch).</summary>
        Bridge,

        /// <summary>Drag a rectangle: cellar under the map (NoDraw land, floor, walls, stairs).</summary>
        Cellar,

        /// <summary>Drag a rectangle: dungeon generator (cave, built or under the map).</summary>
        Dungeon,

        /// <summary>Drag A → B: natural river course.</summary>
        Course,

        /// <summary>Drag a rectangle: labyrinth of raised rock walls (the former "cave" dungeon).</summary>
        MountainWall,

        /// <summary>Round brush: deletes statics (and / or land to void) under it – size like every brush.</summary>
        Eraser,

        /// <summary>Click: stairs down in the chosen direction (stair set), filled down to the ground.</summary>
        Stairs
    }

    public sealed class StaticPickEventArgs : EventArgs
    {
        public StaticPickEventArgs(StaticItem item)
        {
            Item = item;
        }

        public StaticItem Item { get; }
    }

    /// <summary>
    /// Shows the map like the classic client (44×44 tiles, 4 pixels per z, land art, textures on slopes,
    /// statics with hue in client order) and edits the statics: select, place, place multis, delete, colour,
    /// move / copy with the arrow keys, copy and paste, undo / redo.
    /// </summary>
    [ToolboxItem(true)]
    public partial class IsoMapView : Control
    {
        private const int Tile = 44;
        private const int Half = 22;
        private const int ZStep = 4;

        /// <summary>Size of a tile in the 2D view (at zoom 1).</summary>
        private const int Cell = 32;

        private sealed class Placed
        {
            public StaticItem Item;
            public Rectangle Rect;
            public Bitmap Image;

            /// <summary>Drawn size / image size (2D view draws smaller).</summary>
            public float Scale = 1;
        }

        private MapData _map;
        private Bitmap _scene;
        private bool _sceneDirty = true;
        private double _zoom = 1;
        private double _camX;
        private double _camY;
        private readonly List<Placed> _placed = new List<Placed>();
        private readonly HashSet<StaticItem> _selection = new HashSet<StaticItem>();
        private List<StaticItem> _clipboard = new List<StaticItem>();
        private int _clipLandZ;
        private Point _hover = new Point(-1, -1);
        private bool _panning;
        private Point _panStart;
        private double _panCamX;
        private double _panCamY;
        private bool _spaceDown;
        private HashSet<long> _problemTiles = new HashSet<long>();
        private List<MultiPart> _multiParts = new List<MultiPart>();
        private Rectangle _area = Rectangle.Empty;
        private Point _areaStart;
        private bool _areaDragging;
        private bool _landStroke;
        private Point _lastLandTile = new Point(-1, -1);
        private AreaClip _areaClip;
        private bool _areaPaste;

        /// <summary>Copied land + statics of an area.</summary>
        private sealed class AreaClip
        {
            public int W;
            public int H;
            public ushort[] Tiles;
            public sbyte[] Z;
            public int BaseZ;
            public List<StaticItem> Statics = new List<StaticItem>();
        }
        private int _multiId = -1;
        private readonly Random _rnd = new Random();
        private static readonly ImageAttributes TranslucentAttr = CreateAlpha(0.55f);

        private static ImageAttributes CreateAlpha(float alpha)
        {
            var a = new ImageAttributes();
            a.SetColorMatrix(new ColorMatrix { Matrix33 = alpha });
            return a;
        }

        public IsoMapView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.Black;
            TabStop = true;
        }

        public event EventHandler<MapPointEventArgs> HoverChanged;
        public event EventHandler SelectionChanged;
        public event EventHandler MapChanged;
        public event EventHandler ViewChanged;
        public event EventHandler<StaticPickEventArgs> ItemPicked;
        public event EventHandler<string> Message;

        /// <summary>Right click with a land tool: land tile under the mouse (X, Y).</summary>
        public event EventHandler<MapPointEventArgs> LandPicked;

        // ------------------------------------------------------------------ properties

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MapData Map => _map;

        [Category("Dragon"), DefaultValue(IsoTool.Select)]
        public IsoTool Tool { get; set; } = IsoTool.Select;

        [Category("Dragon"), DefaultValue(false)]
        public bool ShowGrid { get; set; }

        [Category("Dragon"), DefaultValue(true)]
        public bool ShowStatics { get; set; } = true;

        [Category("Dragon"), DefaultValue(true)]
        public bool ShowProblems { get; set; } = true;

        /// <summary>Statics above this z are hidden (look under roofs).</summary>
        [Category("Dragon"), DefaultValue(127)]
        public int MaxZ { get; set; } = 127;

        /// <summary>Show the land height of every tile (numbers on the grid).</summary>
        [Category("Dragon"), DefaultValue(false)]
        public bool ShowHeights { get; set; }

        /// <summary>Writes id and height at the foot of every static.</summary>
        [Category("Dragon"), DefaultValue(false)]
        public bool ShowStaticIds { get; set; }

        /// <summary>Content of the overlay: height, land id (hex), id + height, id + height + texture id.</summary>
        [Category("Dragon"), DefaultValue(IsoLabel.Height)]
        public IsoLabel LabelMode { get; set; } = IsoLabel.Height;

        private string LabelText(int x, int y)
        {
            ushort id = _map.LandId(x, y);
            int z = _map.LandZ(x, y);
            switch (LabelMode)
            {
                case IsoLabel.Id:
                    return id.ToString("x4");
                case IsoLabel.IdHeight:
                    return id.ToString("x4") + "\n" + z;
                case IsoLabel.Full:
                    {
                        int t = UoArtCache.LandTextureId(id);
                        return id.ToString("x4") + " z" + z + "\nT " + (t < 0 ? "?" : t == 0 ? "-" : t.ToString("x4"));
                    }
                default:
                    return z.ToString();
            }
        }

        /// <summary>2D view from above (land tiles as squares, no heights) instead of the client view.</summary>
        [Category("Dragon"), DefaultValue(false)]
        public bool TopDown
        {
            get => _topDown;
            set
            {
                if (_topDown != value)
                {
                    _topDown = value;
                    OnViewChanged();
                }
            }
        }

        private bool _topDown;

        /// <summary>Draws all water statics see-through (in any case: items with the tiledata flag Translucent).</summary>
        [Category("Dragon"), DefaultValue(false)]
        public bool TranslucentWater { get; set; }

        /// <summary>Land tiles of the brush "Land setzen"; one of them is taken at random per tile (empty = <see cref="PlaceLandId"/>).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ushort[] PlaceLandIds { get; set; } = Array.Empty<ushort>();

        /// <summary>Land brush like a road: every painted tile gets the height of the first tile of the stroke.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool LandFlatten { get; set; }

        /// <summary>Cave brush: floor tiles (random), wall height above the floor, raise the walls.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ushort[] CaveFloorIds { get; set; } = { 0x245, 0x246, 0x247, 0x248, 0x249 };

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CaveWallHeight { get; set; } = 20;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CaveRaiseWalls { get; set; } = true;

        /// <summary>Cave floor at <see cref="TargetHeight"/> instead of the height where the stroke starts.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CaveUseTargetHeight { get; set; }

        /// <summary>Cave entrance brush: land tile of the mouth (01DB = black entrance, 0002 = invisible entrance like Dragon group 1d).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ushort CaveEntranceFillId { get; set; } = 0x01DB;

        /// <summary>
        /// Cave entrance brush: patterns of map/cave-enti2mountain.txt by mask (A = entrance, B = mountain, order NW N NE E SE S SW W).
        /// Empty = built-in table (same tiles as the Dragon file).
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyDictionary<byte, TransitionPattern> CaveEntrancePatterns { get; set; }

        /// <summary>Cave entrance brush: land tiles that count as mountain (B) – e.g. the tiles of the mountain group of the colour table.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HashSet<ushort> CaveEntranceMountainIds { get; set; } = new HashSet<ushort>();

        /// <summary>Cave entrance brush: a neighbour at least this much higher than the mouth also counts as mountain.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CaveEntranceRise { get; set; } = 5;

        /// <summary>
        /// Cave entrance brush: the mouth lies this much above the ground of the first click. Like the original the black
        /// tile is then stretched up the mountain side (a steep face toward south / east) instead of lying flat on the ground.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CaveEntranceMouthHeight { get; set; } = 20;

        /// <summary>
        /// Cave entrance brush: the front row of the mouth (toward the open ground) becomes the invisible tile 0002 on the ground
        /// height, like the original – the black face behind it then reaches down to the ground.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CaveEntranceFrontNoDraw { get; set; } = true;

        /// <summary>Mountain brush / area: shape settings (tab "Berg").</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MountainSettings Mountain { get; set; } = new MountainSettings();

        /// <summary>Mountain tiles when <see cref="MountainSettings.Tiles"/> is empty (mountain group of the colour table).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ushort[] MountainFallbackTiles { get; set; } = { 0x022c, 0x022d, 0x022e, 0x022f };

        private ushort[] MountainTiles()
        {
            ushort[] t = HexList.Parse(Mountain?.Tiles ?? string.Empty);
            return t.Length > 0 ? t : MountainFallbackTiles ?? Array.Empty<ushort>();
        }

        /// <summary>Template of the tool "Template" (building / floor pattern).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MapTemplate Template
        {
            get => _template;
            set
            {
                _template = value;
                _templateGhost = value?.Statics.OrderBy(t => t.X + t.Y).ThenBy(t => t.Z).Take(4000).ToList() ?? new List<TemplateStatic>();
                Invalidate();
            }
        }

        private MapTemplate _template;
        private List<TemplateStatic> _templateGhost = new List<TemplateStatic>();

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool TemplateStatics { get; set; } = true;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool TemplateLand { get; set; } = true;

        /// <summary>Land of the template with its heights (relative to the target) – otherwise only the tiles.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool TemplateLandZ { get; set; } = true;

        /// <summary>Remove the statics that lie in the area of the template before placing.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool TemplateClearStatics { get; set; }

        /// <summary>Values of the coast and water brushes.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CoastSettings Coast { get; set; } = new CoastSettings();

        /// <summary>Settings of "Küste glätten" (brush, area J, whole map).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CoastSmoothSettings CoastSmooth { get; set; } = new CoastSmoothSettings();

        /// <summary>Right click (with Auswahl / Bereich, or Umschalt + right click with any tool) or the menu key: the form shows its menu.</summary>
        public event EventHandler<MapPointEventArgs> ContextMenuRequested;

        /// <summary>Static under a screen point (for the context menu).</summary>
        public StaticItem StaticAt(Point screen) => _map == null ? null : HitStatic(screen);

        /// <summary>Selects exactly this static (context menu).</summary>
        public void SelectStatic(StaticItem s)
        {
            _selection.Clear();
            if (s != null)
            {
                _selection.Add(s);
            }
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        /// <summary>Tile under a screen point.</summary>
        public Point TileAtScreen(Point screen) => TileAt(screen);

        /// <summary>Tile under the mouse.</summary>
        public Point HoverTile => _hover;

        /// <summary>A coast edit found by the form (whole map, errors) as one undo step.</summary>
        public void ApplyCoastEdit(CoastEdit edit, string message)
        {
            if (_map == null || edit == null)
            {
                return;
            }
            edit.ApplyTo(_map);
            Refresh(true);
            MapChanged?.Invoke(this, EventArgs.Empty);
            Message?.Invoke(this, message);
        }

        /// <summary>Removes the marked area.</summary>
        public void ClearArea()
        {
            ClearMask();
            _area = Rectangle.Empty;
            _areaPaste = false;
            Invalidate();
        }

        /// <summary>"Küste glätten" for the marked area (J): soft heights, straight shore line, statics along.</summary>
        public void SmoothCoastArea()
        {
            if (!NeedArea())
            {
                return;
            }
            CoastEdit edit = CoastSmoother.Smooth(_map, _area, Coast, CoastSmooth, _rnd);
            edit.ApplyTo(_map);
            Refresh(true);
            MapChanged?.Invoke(this, EventArgs.Empty);
            Message?.Invoke(this, edit.Count == 0 ? L.T("Küste glätten: nichts zu ändern (keine Küste im Bereich oder schon glatt).")
                : L.T("Küste geglättet") + $": {edit.Land.Count} " + L.T("Land-Tiles") + $", {edit.Statics.Count} Statics – Strg+Z " + L.T("nimmt es zurück") + ".");
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ushort PlaceLandId { get; set; } = 0x0003;

        /// <summary>Brush radius of the land tools (0 = one tile).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int LandRadius { get; set; }

        /// <summary>Raise / lower per tile and step.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int HeightStep { get; set; } = 1;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TargetHeight { get; set; }

        /// <summary>Marked map area (tool "Area"), empty = none.</summary>
        [Browsable(false)]
        public Rectangle Area => _area;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ushort PlaceId { get; set; } = 0x0E75;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ushort PlaceHue { get; set; }

        /// <summary>Z added to the ground (or to the top of the statics when <see cref="StackOnStatics"/>).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ZOffset { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool StackOnStatics { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool MultiInvisibleParts { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MultiId
        {
            get => _multiId;
            set
            {
                if (value != _multiId)
                {
                    _multiId = value;
                    _multiParts = value >= 0 ? UoArtCache.Multi(value) : new List<MultiPart>();
                }
            }
        }

        public int MultiPartCount => _multiParts.Count;

        [Browsable(false)]
        public IReadOnlyCollection<StaticItem> Selection => _selection;

        [Browsable(false)]
        public double Zoom => _zoom;

        [Browsable(false)]
        public Point CameraTile => new Point((int)Math.Round(_camX), (int)Math.Round(_camY));

        /// <summary>Visible area (approx.) in map tiles, for the overview.</summary>
        [Browsable(false)]
        public RectangleF ViewRect
        {
            get
            {
                if (_topDown)
                {
                    double w = Width / (Cell * _zoom), h = Height / (Cell * _zoom);
                    return new RectangleF((float)(_camX - w / 2), (float)(_camY - h / 2), (float)w, (float)h);
                }
                double r = (Width + Height) / (Tile * _zoom) / 1.2;
                return new RectangleF((float)(_camX - r / 2), (float)(_camY - r / 2), (float)r, (float)r);
            }
        }

        // ------------------------------------------------------------------ setup

        public void SetMap(MapData map)
        {
            _mask = null;
            _points.Clear();
            _dragStart = null;
            _measureA = null;
            _map = map;
            _selection.Clear();
            _problemTiles.Clear();
            if (map != null)
            {
                _camX = Math.Clamp(_camX, 0, map.Width - 1);
                _camY = Math.Clamp(_camY, 0, map.Height - 1);
                if (_camX == 0 && _camY == 0)
                {
                    _camX = map.Width / 2;
                    _camY = map.Height / 2;
                }
            }
            Refresh(true);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetProblems(IEnumerable<MapProblem> problems)
        {
            _problemTiles = new HashSet<long>(problems?.Select(p => ((long)p.X << 32) | (uint)p.Y) ?? Enumerable.Empty<long>());
            Invalidate();
        }

        /// <summary>Re-render (after data changed from outside).</summary>
        public void Refresh(bool scene)
        {
            if (scene)
            {
                _sceneDirty = true;
            }
            Invalidate();
        }

        public void CenterOn(double x, double y)
        {
            if (_map == null)
            {
                return;
            }
            _camX = Math.Clamp(x, 0, _map.Width - 1);
            _camY = Math.Clamp(y, 0, _map.Height - 1);
            OnViewChanged();
        }

        public void ZoomStep(int dir)
        {
            // below 1:4 the view is drawn as a fast overview (colours of land + statics), not with the art
            double[] steps = { 0.03125, 0.0625, 0.09375, 0.125, 0.1875, 0.25, 0.375, 0.5, 0.75, 1, 1.5, 2, 3, 4 };
            int i = Array.FindIndex(steps, s => s >= _zoom - 1e-6);
            i = Math.Clamp((i < 0 ? steps.Length - 1 : i) + dir, 0, steps.Length - 1);
            _zoom = steps[i];
            OnViewChanged();
        }

        private void OnViewChanged()
        {
            _sceneDirty = true;
            Invalidate();
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        // ------------------------------------------------------------------ coordinates

        private int SceneW => Math.Max(1, (int)Math.Ceiling(Width / _zoom));
        private int SceneH => Math.Max(1, (int)Math.Ceiling(Height / _zoom));

        /// <summary>Scene position of the top corner of tile (x, y) at z 0.</summary>
        private PointF TileTop(double x, double y)
        {
            return new PointF((float)(SceneW / 2.0 + (x - y - (_camX - _camY)) * Half),
                              (float)(SceneH / 2.0 + (x + y - (_camX + _camY)) * Half - Half));
        }

        /// <summary>Scene position of the map corner (x, y): north corner of the tile, at its land height (iso) or flat (2D).</summary>
        private PointF Corner(int x, int y)
        {
            if (_topDown)
            {
                return new PointF((float)(SceneW / 2.0 + (x - _camX - 0.5) * Cell), (float)(SceneH / 2.0 + (y - _camY - 0.5) * Cell));
            }
            PointF t = TileTop(x, y);
            return new PointF(t.X, t.Y - _map.LandZ(Math.Clamp(x, 0, _map.Width - 1), Math.Clamp(y, 0, _map.Height - 1)) * ZStep);
        }

        /// <summary>Where an item image is drawn (2D: smaller, standing in its cell).</summary>
        private RectangleF ItemRect(int x, int y, int z, Bitmap img)
        {
            float w = img?.Width ?? 16, h = img?.Height ?? 16;
            if (_topDown)
            {
                PointF c = Corner(x, y);
                float k = Cell / (float)Tile;
                return new RectangleF(c.X + Cell / 2f - w * k / 2, c.Y + Cell - h * k, w * k, h * k);
            }
            PointF top = TileTop(x, y);
            float bottom = top.Y + Tile - z * ZStep;
            return img != null ? new RectangleF(top.X - w / 2f, bottom - h, w, h) : new RectangleF(top.X - 8, bottom - 20, 16, 16);
        }

        private Point ToScene(Point p) => new Point((int)(p.X / _zoom), (int)(p.Y / _zoom));

        /// <summary>Tile under a screen point (the land height is taken into account).</summary>
        public Point TileAt(Point screen)
        {
            if (_map == null)
            {
                return new Point(-1, -1);
            }
            Point s = ToScene(screen);
            if (_topDown)
            {
                return new Point((int)Math.Floor((s.X - SceneW / 2.0) / Cell + _camX + 0.5), (int)Math.Floor((s.Y - SceneH / 2.0) / Cell + _camY + 0.5));
            }
            double u = (s.X - SceneW / 2.0) / Half + (_camX - _camY);
            double v0 = (s.Y - SceneH / 2.0 + Half) / Half + (_camX + _camY);
            double v = v0;
            int tx = 0, ty = 0;
            for (int pass = 0; pass < 3; pass++)
            {
                tx = (int)Math.Floor((u + v) / 2);
                ty = (int)Math.Floor((v - u) / 2);
                v = v0 + _map.LandZ(tx, ty) * ZStep / (double)Half;
            }
            return new Point(tx, ty);
        }

        // ------------------------------------------------------------------ rendering

        private bool _sceneIsScreen;

        /// <summary>Zoom below 1:4: one colour per tile (land, or the highest static), drawn directly in screen pixels.</summary>
        private void RenderOverview()
        {
            int w = Math.Max(1, Width), h = Math.Max(1, Height);
            if (_scene == null || _scene.Width != w || _scene.Height != h || !_sceneIsScreen)
            {
                _scene?.Dispose();
                _scene = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            }
            _sceneIsScreen = true;
            _sceneDirty = false;
            _placed.Clear();
            int bg = BackColor.ToArgb();
            var buffer = new int[w * h];
            var cache = new Dictionary<long, int>();
            int sw = SceneW, sh = SceneH;
            for (int py = 0; py < h; py++)
            {
                for (int px = 0; px < w; px++)
                {
                    double sx = px / _zoom, sy = py / _zoom;
                    int tx, ty;
                    if (_topDown)
                    {
                        tx = (int)Math.Floor((sx - sw / 2.0) / Cell + _camX + 0.5);
                        ty = (int)Math.Floor((sy - sh / 2.0) / Cell + _camY + 0.5);
                    }
                    else
                    {
                        double u = (sx - sw / 2.0) / Half + (_camX - _camY);
                        double v = (sy - sh / 2.0 + Half) / Half + (_camX + _camY);
                        tx = (int)Math.Floor((u + v) / 2);
                        ty = (int)Math.Floor((v - u) / 2);
                    }
                    if (!_map.InBounds(tx, ty))
                    {
                        buffer[py * w + px] = bg;
                        continue;
                    }
                    long key = ((long)tx << 32) | (uint)ty;
                    if (!cache.TryGetValue(key, out int c))
                    {
                        Color col = UoArtCache.LandColor(_map.LandId(tx, ty));
                        if (ShowStatics)
                        {
                            StaticItem top = null;
                            foreach (StaticItem st in _map.StaticsAt(tx, ty))
                            {
                                if (st.Z <= MaxZ && !Hidden(st.Id) && (top == null || st.Z >= top.Z))
                                {
                                    top = st;
                                }
                            }
                            if (top != null)
                            {
                                col = UoArtCache.ItemColor(top.Id);
                            }
                        }
                        c = cache[key] = col.ToArgb() | unchecked((int)0xFF000000);
                    }
                    buffer[py * w + px] = c;
                }
            }
            BitmapData data = _scene.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < h; y++)
                {
                    System.Runtime.InteropServices.Marshal.Copy(buffer, y * w, data.Scan0 + y * data.Stride, w);
                }
            }
            finally
            {
                _scene.UnlockBits(data);
            }
        }

        private void RenderScene()
        {
            if (_zoom < 0.24 && _map != null)
            {
                RenderOverview();
                return;
            }
            _sceneIsScreen = false;
            int sw = SceneW, sh = SceneH;
            if (_scene == null || _scene.Width != sw || _scene.Height != sh)
            {
                _scene?.Dispose();
                _scene = new Bitmap(sw, sh, PixelFormat.Format32bppPArgb);
            }
            _placed.Clear();
            using Graphics g = Graphics.FromImage(_scene);
            g.Clear(BackColor);
            _sceneDirty = false;
            if (_map == null)
            {
                return;
            }

            if (_topDown)
            {
                RenderTopDown(g, sw, sh);
                return;
            }

            var drawables = new List<(int depth, int prio, int order, Action draw)>(4096);
            var below = new List<(int depth, int prio, int order, Action draw)>();
            int order = 0;
            double cs = _camX + _camY, cd = _camX - _camY;
            int sMin = (int)Math.Floor(cs - sh / (double)Tile) - 8;
            int sMax = (int)Math.Ceiling(cs + sh / (double)Tile) + 34;
            int dHalf = (int)Math.Ceiling(sw / (double)Tile) + 3;

            for (int s = sMin; s <= sMax; s++)
            {
                for (int d = (int)Math.Floor(cd) - dHalf; d <= (int)Math.Ceiling(cd) + dHalf; d++)
                {
                    if (((s + d) & 1) != 0)
                    {
                        continue;
                    }
                    int x = (s + d) / 2, y = (s - d) / 2;
                    if (!_map.InBounds(x, y))
                    {
                        continue;
                    }
                    PointF top = TileTop(x, y);
                    int zt = _map.LandZ(x, y), zr = _map.LandZ(Math.Min(x + 1, _map.Width - 1), y);
                    int zb = _map.LandZ(Math.Min(x + 1, _map.Width - 1), Math.Min(y + 1, _map.Height - 1)), zl = _map.LandZ(x, Math.Min(y + 1, _map.Height - 1));
                    PointF[] poly =
                    {
                        new PointF(top.X, top.Y - zt * ZStep),
                        new PointF(top.X + Half, top.Y + Half - zr * ZStep),
                        new PointF(top.X, top.Y + Tile - zb * ZStep),
                        new PointF(top.X - Half, top.Y + Half - zl * ZStep)
                    };
                    float minY = Math.Min(Math.Min(poly[0].Y, poly[1].Y), Math.Min(poly[2].Y, poly[3].Y));
                    float maxY = Math.Max(Math.Max(poly[0].Y, poly[1].Y), Math.Max(poly[2].Y, poly[3].Y));
                    bool landVisible = maxY >= 0 && minY <= sh && top.X + Half >= 0 && top.X - Half <= sw;
                    ushort landId = _map.LandId(x, y);
                    int zMin = Math.Min(Math.Min(zt, zr), Math.Min(zb, zl));
                    if (landVisible)
                    {
                        drawables.Add((x + y, zMin * 4 - 2, order++, () => DrawLand(g, landId, poly, zt, zr, zb, zl, x, y)));
                    }

                    if (!ShowStatics)
                    {
                        continue;
                    }
                    foreach (StaticItem st in _map.StaticsAt(x, y))
                    {
                        if (st.Z > MaxZ || Hidden(st.Id))
                        {
                            continue;
                        }
                        Bitmap img = UoArtCache.Static(st.Id, st.Hue);
                        float bottom = top.Y + Tile - st.Z * ZStep;
                        Rectangle rect = img != null
                            ? new Rectangle((int)(top.X - img.Width / 2f), (int)(bottom - img.Height), img.Width, img.Height)
                            : new Rectangle((int)top.X - 8, (int)bottom - 20, 16, 16);
                        if (rect.Right < 0 || rect.Left > sw || rect.Bottom < 0 || rect.Top > sh)
                        {
                            continue;
                        }
                        int height = UoArtCache.ItemHeight(st.Id);
                        int threshold = (height > 0 ? 1 : 0) + (UoArtCache.IsBackground(st.Id) ? 0 : 1);
                        var placed = new Placed { Item = st, Rect = rect, Image = img };
                        StaticItem item = st;
                        bool seeThrough = IsSeeThrough(st.Id);
                        bool noDraw = CaveEntranceBuilder.IsNoDrawStatic(st.Id, UoArtCache.ItemName(st.Id));
                        PointF floor = new PointF(top.X, top.Y + Half - st.Z * ZStep);
                        // "unter der Karte": statics below the land are drawn after everything (else the land hides them)
                        List<(int depth, int prio, int order, Action draw)> target = UnderView && st.Z < zMin && !CaveEntranceBuilder.IsNoDraw(landId) ? below : drawables;
                        target.Add((x + y, (st.Z + threshold) * 4 + threshold, order++, () =>
                        {
                            if (noDraw)
                            {
                                // the client never draws it – only a marker so it can be seen and selected
                                if (ShowNoDraw)
                                {
                                    using var nd = new Pen(Color.FromArgb(170, 255, 140, 0)) { DashStyle = DashStyle.Dot };
                                    var r = new RectangleF(floor.X - 9, floor.Y - 5, 18, 10);
                                    g.DrawEllipse(nd, r);
                                    _placed.Add(new Placed { Item = item, Rect = Rectangle.Round(r), Image = null });
                                }
                                return;
                            }
                            if (img != null && seeThrough)
                            {
                                g.DrawImage(img, rect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, TranslucentAttr);
                            }
                            else if (img != null)
                            {
                                g.DrawImageUnscaled(img, rect.X, rect.Y);
                            }
                            else
                            {
                                using var b = new SolidBrush(UoArtCache.ItemColor(item.Id));
                                g.FillRectangle(b, rect);
                                g.DrawRectangle(Pens.Black, rect);
                            }
                            if (ShowStaticIds)
                            {
                                // id and height of the static at its foot
                                string text = $"{item.Id:x4} z{item.Z}";
                                using var font = new Font("Segoe UI", 6.5f, FontStyle.Bold);
                                SizeF sz = g.MeasureString(text, font);
                                float tx = floor.X - sz.Width / 2, ty = floor.Y - sz.Height - 2;
                                using var back = new SolidBrush(Color.FromArgb(170, 60, 20, 0));
                                g.FillRectangle(back, tx - 1, ty, sz.Width + 2, sz.Height);
                                g.DrawString(text, font, Brushes.Orange, tx, ty);
                            }
                            _placed.Add(placed);
                        }));
                    }
                }
            }

            g.SmoothingMode = SmoothingMode.None;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            foreach (var d in drawables.OrderBy(d => d.depth).ThenBy(d => d.prio).ThenBy(d => d.order).Concat(below.OrderBy(d => d.depth).ThenBy(d => d.prio).ThenBy(d => d.order)))
            {
                try
                {
                    d.draw();
                }
                catch (Exception)
                {
                    // one broken tile / image must not stop the whole view
                }
            }

            if (ShowHeights)
            {
                DrawHeights(g, sMin, sMax, cd, dHalf);
            }
        }

        /// <summary>Tiledata flag Translucent, or a water static while 'water see-through' is on.</summary>
        private bool IsSeeThrough(ushort id) => UoArtCache.IsTranslucent(id) || (TranslucentWater && UoArtCache.ItemIsWet(id));

        /// <summary>2D view: land tiles from above as squares with relief shading, statics small in their cell.</summary>
        private void RenderTopDown(Graphics g, int sw, int sh)
        {
            int x0 = (int)Math.Floor(_camX - sw / 2.0 / Cell) - 1, x1 = (int)Math.Ceiling(_camX + sw / 2.0 / Cell) + 1;
            int y0 = (int)Math.Floor(_camY - sh / 2.0 / Cell) - 1, y1 = (int)Math.Ceiling(_camY + sh / 2.0 / Cell) + 4;
            x0 = Math.Max(0, x0);
            y0 = Math.Max(0, y0);
            x1 = Math.Min(_map.Width - 1, x1);
            y1 = Math.Min(_map.Height - 1, y1);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            using var gridPen = new Pen(Color.FromArgb(60, Color.White));
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    PointF c = Corner(x, y);
                    var cell = new Rectangle((int)Math.Floor(c.X), (int)Math.Floor(c.Y), Cell, Cell);
                    try
                    {
                        g.DrawImageUnscaled(UoArtCache.LandTopDown(_map.LandId(x, y), Cell), cell.X, cell.Y);
                    }
                    catch (Exception)
                    {
                        // a broken tile must not stop the view
                    }
                    // light from the north-west: slopes become brighter / darker
                    int z = _map.LandZ(x, y);
                    int slope = (_map.LandZ(Math.Max(0, x - 1), y) - z) + (_map.LandZ(x, Math.Max(0, y - 1)) - z);
                    if (slope != 0)
                    {
                        int a = Math.Min(120, Math.Abs(slope) * 6);
                        using var shade = new SolidBrush(slope > 0 ? Color.FromArgb(a, 0, 0, 0) : Color.FromArgb(a / 2, 255, 255, 255));
                        g.FillRectangle(shade, cell);
                    }
                    if (ShowGrid)
                    {
                        g.DrawRectangle(gridPen, cell);
                    }
                }
            }
            if (ShowStatics)
            {
                g.InterpolationMode = InterpolationMode.Bilinear;
                for (int y = y0; y <= y1; y++)
                {
                    for (int x = x0; x <= x1; x++)
                    {
                        foreach (StaticItem st in _map.StaticsAt(x, y).OrderBy(s => s.Z))
                        {
                            if (st.Z > MaxZ || Hidden(st.Id))
                            {
                                continue;
                            }
                            bool seeThrough = IsSeeThrough(st.Id);
                            Bitmap flat = UoArtCache.ItemIsWet(st.Id) ? UoArtCache.StaticTopDown(st.Id, st.Hue, Cell) : null;
                            if (flat != null)
                            {
                                PointF c = Corner(x, y);
                                var r = new Rectangle((int)Math.Floor(c.X), (int)Math.Floor(c.Y), Cell, Cell);
                                if (seeThrough)
                                {
                                    g.DrawImage(flat, r, 0, 0, Cell, Cell, GraphicsUnit.Pixel, TranslucentAttr);
                                }
                                else
                                {
                                    g.DrawImageUnscaled(flat, r.X, r.Y);
                                }
                                _placed.Add(new Placed { Item = st, Rect = r, Image = flat });
                                continue;
                            }
                            Bitmap img = UoArtCache.Static(st.Id, st.Hue);
                            RectangleF rf = ItemRect(x, y, st.Z, img);
                            var rect = Rectangle.Round(rf);
                            if (rect.Right < 0 || rect.Left > sw || rect.Bottom < 0 || rect.Top > sh || rect.Width < 1 || rect.Height < 1)
                            {
                                continue;
                            }
                            try
                            {
                                if (img == null)
                                {
                                    using var b = new SolidBrush(UoArtCache.ItemColor(st.Id));
                                    g.FillRectangle(b, rect);
                                }
                                else if (seeThrough)
                                {
                                    g.DrawImage(img, rect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, TranslucentAttr);
                                }
                                else
                                {
                                    g.DrawImage(img, rect);
                                }
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                            _placed.Add(new Placed { Item = st, Rect = rect, Image = img, Scale = img == null ? 1 : rect.Width / (float)img.Width });
                        }
                    }
                }
            }
            if (ShowHeights && _zoom >= 0.75)
            {
                using var font = new Font("Segoe UI", 7f, FontStyle.Bold);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                for (int y = y0; y <= y1; y++)
                {
                    for (int x = x0; x <= x1; x++)
                    {
                        PointF c = Corner(x, y);
                        int z = _map.LandZ(x, y);
                        string text = LabelText(x, y);
                        g.DrawString(text, font, Brushes.Black, c.X + Cell / 2f + 1, c.Y + Cell / 2f + 1, sf);
                        g.DrawString(text, font, z < 0 ? Brushes.LightSkyBlue : Brushes.White, c.X + Cell / 2f, c.Y + Cell / 2f, sf);
                    }
                }
            }
        }

        /// <summary>Land z as number in the middle of every visible tile.</summary>
        private void DrawHeights(Graphics g, int sMin, int sMax, double cd, int dHalf)
        {
            using var font = new Font("Segoe UI", 7f, FontStyle.Bold);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            int sw = SceneW, sh = SceneH;
            for (int s = sMin; s <= sMax; s++)
            {
                for (int d = (int)Math.Floor(cd) - dHalf; d <= (int)Math.Ceiling(cd) + dHalf; d++)
                {
                    if (((s + d) & 1) != 0)
                    {
                        continue;
                    }
                    int x = (s + d) / 2, y = (s - d) / 2;
                    if (!_map.InBounds(x, y))
                    {
                        continue;
                    }
                    PointF top = TileTop(x, y);
                    int z = _map.LandZ(x, y);
                    var c = new PointF(top.X, top.Y + Half - z * ZStep);
                    if (c.X < -20 || c.Y < -20 || c.X > sw + 20 || c.Y > sh + 20)
                    {
                        continue;
                    }
                    string text = LabelText(x, y);
                    g.DrawString(text, font, Brushes.Black, c.X + 1, c.Y + 1, sf);
                    g.DrawString(text, font, z < 0 ? Brushes.LightSkyBlue : Brushes.White, c.X, c.Y, sf);
                }
            }
        }

        /// <summary>
        /// ClassicUO (Land.ApplyStretch): a land tile is drawn with its texture as soon as one of its 4 corner normals is not
        /// flat – i.e. when any of the 12 heights around it differs – and the texture exists. Otherwise the land art is drawn
        /// flat on the tile height (also on a slope!).
        /// </summary>
        private bool NeighbourhoodFlat(int x, int y) => CgiAreaAnalyzer.NeighbourhoodFlat(_map, x, y);

        /// <summary>Show land tiles 0–2 (the client never draws them) as a yellow outline.</summary>
        [Category("Dragon"), DefaultValue(true)]
        public bool ShowNoDraw { get; set; } = true;

        /// <summary>Preview: textured land tiles always with their texture, also on flat land (the client uses the art there).</summary>
        [Category("Dragon"), DefaultValue(false)]
        public bool AlwaysTextures { get; set; }

        private void DrawLand(Graphics g, ushort landId, PointF[] poly, int zt, int zr, int zb, int zl, int tx, int ty)
        {
            if (CaveEntranceBuilder.IsNoDraw(landId))
            {
                // the client does not draw 0000–0002 (e.g. the front row of a cave entrance): the land behind shows through
                if (ShowNoDraw)
                {
                    using var nd = new Pen(Color.FromArgb(150, 255, 220, 0)) { DashStyle = DashStyle.Dash };
                    g.DrawPolygon(nd, poly);
                }
                return;
            }
            Bitmap texture = UoArtCache.Texture(landId);
            bool stretched = texture != null && (AlwaysTextures || !NeighbourhoodFlat(tx, ty));
            bool flat = zt == zr && zr == zb && zb == zl;
            bool wetFlat = !flat && UoArtCache.LandIsWetFlat(landId);
            // like the client: without a texture the art lies flat on the tile height, also on a slope
            Bitmap art = !stretched ? UoArtCache.Land(landId) : null;
            if (art != null)
            {
                if (wetFlat)
                {
                    // like the client: wet tiles without texture (water) are never stretched
                    using var under = new SolidBrush(UoArtCache.LandColor(landId));
                    g.FillPolygon(under, poly);
                }
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(art, new RectangleF(poly[0].X - Half, poly[0].Y, Tile, Tile));
            }
            else
            {
                Bitmap tex = UoArtCache.Texture(landId);
                double light = Math.Clamp(1.0 + (zl - zr + zt - zb) * 0.035, 0.45, 1.6);
                if (tex != null)
                {
                    DrawTextured(g, tex, poly, (float)light);
                }
                else
                {
                    Bitmap flatArt = UoArtCache.Land(landId);
                    if (flatArt != null && !flat)
                    {
                        // no texture: stretch the land art over the corners
                        DrawTextured(g, flatArt, poly, (float)light, true);
                    }
                    else
                    {
                        Color c = UoArtCache.LandColor(landId);
                        c = Color.FromArgb(Math.Clamp((int)(c.R * light), 0, 255), Math.Clamp((int)(c.G * light), 0, 255), Math.Clamp((int)(c.B * light), 0, 255));
                        using var b = new SolidBrush(c);
                        g.FillPolygon(b, poly);
                    }
                }
            }
            if (ShowGrid)
            {
                using var pen = new Pen(Color.FromArgb(70, Color.White));
                g.DrawPolygon(pen, poly);
            }
        }

        /// <summary>
        /// Maps an image onto the 4 land corners with two affine triangles.
        /// Texture: the square corners go to top / right / bottom / left. Land art (diamond): its 4 points go there.
        /// </summary>
        private static void DrawTextured(Graphics g, Bitmap img, PointF[] p, float light, bool diamondArt = false)
        {
            PointF t = p[0], r = p[1], b = p[2], l = p[3];
            float w = img.Width, h = img.Height;
            // source points of top, right, bottom, left
            PointF st, sr, sb, sl;
            if (diamondArt)
            {
                st = new PointF(w / 2, 0);
                sr = new PointF(w, h / 2);
                sb = new PointF(w / 2, h);
                sl = new PointF(0, h / 2);
            }
            else
            {
                st = new PointF(0, 0);
                sr = new PointF(w, 0);
                sb = new PointF(w, h);
                sl = new PointF(0, h);
            }
            using var attr = new ImageAttributes();
            attr.SetColorMatrix(new ColorMatrix(new[]
            {
                new[] { light, 0f, 0f, 0f, 0f },
                new[] { 0f, light, 0f, 0f, 0f },
                new[] { 0f, 0f, light, 0f, 0f },
                new[] { 0f, 0f, 0f, 1f, 0f },
                new[] { 0f, 0f, 0f, 0f, 1f }
            }));
            attr.SetWrapMode(WrapMode.TileFlipXY);
            g.InterpolationMode = InterpolationMode.Bilinear;
            DrawTriangle(g, img, attr, st, sr, sl, t, r, l);
            DrawTriangle(g, img, attr, sr, sb, sl, r, b, l);
            g.ResetClip();
        }

        /// <summary>Draws the source triangle (s0, s1, s2) of the image into the destination triangle (d0, d1, d2).</summary>
        private static void DrawTriangle(Graphics g, Bitmap img, ImageAttributes attr, PointF s0, PointF s1, PointF s2, PointF d0, PointF d1, PointF d2)
        {
            // affine map M with M(s_i) = d_i; the image corners (0,0), (w,0), (0,h) go to the three DrawImage points
            double det = (s1.X - s0.X) * (s2.Y - s0.Y) - (s2.X - s0.X) * (s1.Y - s0.Y);
            double area = (d1.X - d0.X) * (d2.Y - d0.Y) - (d2.X - d0.X) * (d1.Y - d0.Y);
            if (Math.Abs(det) < 1e-6 || Math.Abs(area) < 1.0)
            {
                return; // very steep tile: triangle is only a line
            }
            double ax = ((d1.X - d0.X) * (s2.Y - s0.Y) - (d2.X - d0.X) * (s1.Y - s0.Y)) / det;
            double bx = ((d2.X - d0.X) * (s1.X - s0.X) - (d1.X - d0.X) * (s2.X - s0.X)) / det;
            double ay = ((d1.Y - d0.Y) * (s2.Y - s0.Y) - (d2.Y - d0.Y) * (s1.Y - s0.Y)) / det;
            double by = ((d2.Y - d0.Y) * (s1.X - s0.X) - (d1.Y - d0.Y) * (s2.X - s0.X)) / det;
            PointF Map(double u, double v) => new PointF((float)(d0.X + ax * (u - s0.X) + bx * (v - s0.Y)), (float)(d0.Y + ay * (u - s0.X) + by * (v - s0.Y)));
            PointF[] dest = { Map(0, 0), Map(img.Width, 0), Map(0, img.Height) };
            using var clip = new GraphicsPath();
            // grow the triangle a little so neighbouring triangles do not leave hairline gaps
            PointF c = new PointF((d0.X + d1.X + d2.X) / 3, (d0.Y + d1.Y + d2.Y) / 3);
            PointF Grow(PointF q) => new PointF(q.X + Math.Sign(q.X - c.X) * 0.6f, q.Y + Math.Sign(q.Y - c.Y) * 0.6f);
            clip.AddPolygon(new[] { Grow(d0), Grow(d1), Grow(d2) });
            g.SetClip(clip);
            try
            {
                g.DrawImage(img, dest, new RectangleF(0, 0, img.Width, img.Height), GraphicsUnit.Pixel, attr);
            }
            catch (Exception)
            {
                // GDI+ refuses extreme parallelograms – fill the triangle instead
                using var brush = new SolidBrush(Color.FromArgb(90, 90, 90));
                g.FillPolygon(brush, new[] { d0, d1, d2 });
            }
            g.ResetClip();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (_map == null)
            {
                g.Clear(BackColor);
                TextRenderer.DrawText(g, "Keine Karte geladen – 'Aus Editor-Karte bauen' oder 'MUL / UOP laden'.", Font, new Point(12, 12), Color.Gainsboro);
                return;
            }
            if (_sceneDirty || _scene == null)
            {
                RenderScene();
            }
            g.InterpolationMode = _zoom >= 1 ? InterpolationMode.NearestNeighbor : InterpolationMode.Bilinear;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            if (_sceneIsScreen)
            {
                g.DrawImageUnscaled(_scene, 0, 0);
            }
            else
            {
                g.DrawImage(_scene, new Rectangle(0, 0, (int)(_scene.Width * _zoom), (int)(_scene.Height * _zoom)));
            }

            g.ScaleTransform((float)_zoom, (float)_zoom);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (ShowProblems && _problemTiles.Count > 0)
            {
                using var pen = new Pen(Color.Red, 2);
                foreach (long key in _problemTiles)
                {
                    int x = (int)(key >> 32), y = (int)(key & 0xFFFFFFFF);
                    if (Math.Abs(x - _camX) + Math.Abs(y - _camY) < (Width + Height) / (Half * _zoom))
                    {
                        g.DrawPolygon(pen, LandPoly(x, y));
                    }
                }
            }

            foreach (Placed p in _placed)
            {
                if (_selection.Contains(p.Item))
                {
                    using var pen = new Pen(Color.Yellow, 2) { DashStyle = DashStyle.Dash };
                    g.DrawRectangle(pen, p.Rect);
                }
            }

            if (!_area.IsEmpty)
            {
                using var pen = new Pen(Color.Cyan, 2) { DashStyle = DashStyle.Dash };
                g.DrawPolygon(pen, AreaPoly(_area));
            }
            DrawToolOverlay(g);
            if (_areaPaste && _areaClip != null && _map.InBounds(_hover.X, _hover.Y))
            {
                using var pen = new Pen(Color.Orange, 2) { DashStyle = DashStyle.Dash };
                g.DrawPolygon(pen, AreaPoly(new Rectangle(_hover.X, _hover.Y, _areaClip.W, _areaClip.H)));
            }
            if (IsLandTool(Tool) && LandRadius > 0 && _map.InBounds(_hover.X, _hover.Y))
            {
                using var pen = new Pen(Color.FromArgb(160, 255, 255, 0));
                if (IsRoundBrush(Tool))
                {
                    g.DrawPolygon(pen, CirclePoly(_hover.X, _hover.Y, LandRadius));
                }
                else
                {
                    g.DrawPolygon(pen, AreaPoly(new Rectangle(_hover.X - LandRadius, _hover.Y - LandRadius, LandRadius * 2 + 1, LandRadius * 2 + 1)));
                }
            }
            if (_map.InBounds(_hover.X, _hover.Y))
            {
                using var pen = new Pen(Color.FromArgb(220, 255, 255, 255), 1.5f);
                g.DrawPolygon(pen, LandPoly(_hover.X, _hover.Y));
                DrawGhost(g);
            }
            g.ResetTransform();
            DrawBand(g);
            DrawUnderBanner(g);
        }

        private static bool IsLandTool(IsoTool t) => t == IsoTool.LandSet || t == IsoTool.Raise || t == IsoTool.Lower || t == IsoTool.SetHeight || t == IsoTool.Smooth ||
                                                     t == IsoTool.Coast || t == IsoTool.Water || t == IsoTool.Cave || t == IsoTool.CaveEntrance || t == IsoTool.Mountain || t == IsoTool.PatternBrush ||
                                                     t == IsoTool.CoastSmooth || IsNewBrush(t);

        /// <summary>Round brushes: the outline is a circle.</summary>
        private static bool IsRoundBrush(IsoTool t) => t == IsoTool.Coast || t == IsoTool.CoastSmooth || IsNewBrush(t) ||
                                                       t == IsoTool.Raise || t == IsoTool.Lower || t == IsoTool.Smooth;

        /// <summary>Outline of a round brush (circle of tiles) at the land heights.</summary>
        private PointF[] CirclePoly(int cx, int cy, int r)
        {
            var pts = new List<PointF>();
            double rr = r + 0.5;
            for (int k = 0; k < 48; k++)
            {
                double a = k * Math.PI * 2 / 48;
                double mx = cx + 0.5 + Math.Cos(a) * rr, my = cy + 0.5 + Math.Sin(a) * rr;
                int ix = (int)Math.Floor(mx), iy = (int)Math.Floor(my);
                // corner of the cell plus the rest along its two edges (screen units, at the land heights)
                PointF a0 = Corner(Math.Clamp(ix, 0, _map.Width - 1), Math.Clamp(iy, 0, _map.Height - 1));
                PointF ax = Corner(Math.Clamp(ix + 1, 0, _map.Width), Math.Clamp(iy, 0, _map.Height - 1));
                PointF ay = Corner(Math.Clamp(ix, 0, _map.Width - 1), Math.Clamp(iy + 1, 0, _map.Height));
                double fx = mx - ix, fy = my - iy;
                pts.Add(new PointF((float)(a0.X + (ax.X - a0.X) * fx + (ay.X - a0.X) * fy), (float)(a0.Y + (ax.Y - a0.Y) * fx + (ay.Y - a0.Y) * fy)));
            }
            return pts.ToArray();
        }

        /// <summary>Outline of a map rectangle (at the land heights of its corners).</summary>
        private PointF[] AreaPoly(Rectangle r)
        {
            return new[] { Corner(r.Left, r.Top), Corner(r.Right, r.Top), Corner(r.Right, r.Bottom), Corner(r.Left, r.Bottom) };
        }

        private PointF[] LandPoly(int x, int y)
        {
            return new[] { Corner(x, y), Corner(x + 1, y), Corner(x + 1, y + 1), Corner(x, y + 1) };
        }

        /// <summary>Semi transparent preview of the item / multi that would be placed.</summary>
        private void DrawGhost(Graphics g)
        {
            DrawStampDrag(g);
            var parts = new List<(ushort id, int x, int y, int z)>();
            if (Tool == IsoTool.Place)
            {
                parts.Add((PlaceId, _hover.X, _hover.Y, PlaceZ(_hover.X, _hover.Y)));
            }
            else if (Tool == IsoTool.Multi && _multiParts.Count > 0)
            {
                int baseZ = _map.LandZ(_hover.X, _hover.Y) + ZOffset;
                foreach (MultiPart m in _multiParts.Where(m => m.Visible || MultiInvisibleParts).OrderBy(m => m.Dx + m.Dy).ThenBy(m => m.Dz))
                {
                    parts.Add((m.Id, _hover.X + m.Dx, _hover.Y + m.Dy, baseZ + m.Dz));
                }
            }
            else if (Tool == IsoTool.Template && _template != null)
            {
                int baseZ = _map.LandZ(_hover.X, _hover.Y) + ZOffset;
                if (TemplateStatics)
                {
                    foreach (TemplateStatic t in _templateGhost)
                    {
                        parts.Add((t.Id, _hover.X + t.X, _hover.Y + t.Y, baseZ + t.Z));
                    }
                }
                Rectangle b = _template.Bounds;
                using var pen = new Pen(Color.FromArgb(200, 255, 170, 0), 2) { DashStyle = DashStyle.Dash };
                g.DrawPolygon(pen, AreaPoly(new Rectangle(_hover.X + b.X, _hover.Y + b.Y, b.Width, b.Height)));
            }
            else if (Tool == IsoTool.Stairs)
            {
                foreach (StaticChange c in StairPieces(_hover)
                             .OrderBy(c => c.After.X + c.After.Y).ThenBy(c => c.After.Z))
                {
                    parts.Add((c.After.Id, c.After.X, c.After.Y, c.After.Z));
                }
            }
            else if (Tool == IsoTool.Bridge && _dragStart.HasValue && _dragStart.Value != _dragEnd)
            {
                // the real pieces of the bridge while dragging
                BridgeTemplate plan = CurrentBridgePlan(out _);
                foreach (StaticChange c in BridgeKit.Build(_map, _dragStart.Value, _dragEnd, plan, BrushSettings.Bridge, new Random(1))
                             .OrderBy(c => c.After.X + c.After.Y).ThenBy(c => c.After.Z).Take(3000))
                {
                    parts.Add((c.After.Id, c.After.X, c.After.Y, c.After.Z));
                }
            }
            else if (_pasteGhost && _clipboard.Count > 0)
            {
                StaticItem a = _clipboard[0];
                int dz = _map.LandZ(_hover.X, _hover.Y) - _clipLandZ;
                foreach (StaticItem s in _clipboard)
                {
                    parts.Add((s.Id, _hover.X + s.X - a.X, _hover.Y + s.Y - a.Y, s.Z + dz));
                }
            }
            if (parts.Count == 0)
            {
                return;
            }
            using var attr = new ImageAttributes();
            attr.SetColorMatrix(new ColorMatrix { Matrix33 = 0.6f });
            foreach ((ushort id, int x, int y, int z) in parts)
            {
                Bitmap img = UoArtCache.Static(id, Tool == IsoTool.Place ? PlaceHue : 0);
                Rectangle r = Rectangle.Round(ItemRect(x, y, z, img));
                if (img != null)
                {
                    g.DrawImage(img, r, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attr);
                }
                else
                {
                    g.DrawRectangle(Pens.Orange, r);
                }
            }
        }

        private bool _pasteGhost;
        private int _strokeZ;

        /// <summary>Cells painted by the current cave entrance stroke (their height is the stroke height + pattern offset).</summary>
        private readonly HashSet<int> _strokeCells = new HashSet<int>();

        private int PlaceZ(int x, int y)
        {
            int baseZ = StackOnStatics ? _map.TopZ(x, y, id => UoArtCache.ItemHeight(id)) : _map.LandZ(x, y);
            return Math.Clamp(baseZ + ZOffset + (UnderView && !StackOnStatics ? BrushSettings.UnderOffset : 0), -128, 127);
        }

        // ------------------------------------------------------------------ hit test

        private StaticItem HitStatic(Point screen)
        {
            Point p = ToScene(screen);
            for (int i = _placed.Count - 1; i >= 0; i--)
            {
                Placed s = _placed[i];
                if (!s.Rect.Contains(p))
                {
                    continue;
                }
                if (s.Image == null)
                {
                    return s.Item;
                }
                try
                {
                    int px = Math.Clamp((int)((p.X - s.Rect.X) / s.Scale), 0, s.Image.Width - 1);
                    int py = Math.Clamp((int)((p.Y - s.Rect.Y) / s.Scale), 0, s.Image.Height - 1);
                    if (s.Image.GetPixel(px, py).A > 16)
                    {
                        return s.Item;
                    }
                }
                catch (Exception)
                {
                    return s.Item;
                }
            }
            return null;
        }

        // ------------------------------------------------------------------ mouse

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            ZoomStep(e.Delta > 0 ? 1 : -1);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (_map == null)
            {
                return;
            }
            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && _spaceDown))
            {
                _panning = true;
                _panStart = e.Location;
                _panCamX = _camX;
                _panCamY = _camY;
                Cursor = Cursors.SizeAll;
                return;
            }
            Point tile = TileAt(e.Location);
            StaticItem hit = HitStatic(e.Location);

            if (e.Button == MouseButtons.Right)
            {
                if (_pasteGhost)
                {
                    _pasteGhost = false;
                    Invalidate();
                    return;
                }
                if (ToolRightClick())
                {
                    return;
                }
                // context menu: Umschalt + right click with every tool; with tools that have no own right click a plain right click
                bool menuTool = Tool == IsoTool.Select || Tool == IsoTool.Area || Tool == IsoTool.Delete || Tool == IsoTool.Hue ||
                                Tool == IsoTool.Multi || Tool == IsoTool.Template || Tool == IsoTool.CaveStamp;
                if (ContextMenuRequested != null && !_areaPaste && ((ModifierKeys & Keys.Shift) != 0 || menuTool))
                {
                    ContextMenuRequested(this, new MapPointEventArgs(tile.X, tile.Y, -1));
                    return;
                }
                if (IsLandTool(Tool) && _map.InBounds(tile.X, tile.Y))
                {
                    LandPicked?.Invoke(this, new MapPointEventArgs(tile.X, tile.Y, _map.LandId(tile.X, tile.Y)));
                    return;
                }
                if (hit != null)
                {
                    ItemPicked?.Invoke(this, new StaticPickEventArgs(hit));
                }
                return;
            }
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (_pasteGhost)
            {
                PasteAt(tile.X, tile.Y);
                return;
            }
            if (_areaPaste)
            {
                PasteArea(tile.X, tile.Y);
                return;
            }
            if (ToolMouseDown(e, tile, hit))
            {
                return;
            }
            if (IsLandTool(Tool))
            {
                BeginBrushStroke(tile);
                _map.BeginLand();
                _landStroke = true;
                _lastLandTile = new Point(-1, -1);
                _strokeZ = Tool == IsoTool.Cave && CaveUseTargetHeight ? TargetHeight : _map.LandZ(tile.X, tile.Y);
                _strokeCells.Clear();
                _patternOnce = false;
                _patternStepCells = 0;
                _map.StrokeGroup = Tool == IsoTool.PatternBrush && Pattern != null && Pattern.StepCells > 0 ? _map.NewGroup() : 0;
                ApplyLandTool(tile);
                return;
            }
            if (Tool == IsoTool.Area)
            {
                ClearMask();
                _areaStart = tile;
                _areaDragging = true;
                _area = new Rectangle(tile.X, tile.Y, 1, 1);
                Invalidate();
                return;
            }

            switch (Tool)
            {
                case IsoTool.Select:
                    if ((ModifierKeys & Keys.Control) == 0)
                    {
                        _selection.Clear();
                    }
                    if ((ModifierKeys & Keys.Shift) != 0 && _map.InBounds(tile.X, tile.Y))
                    {
                        foreach (StaticItem s in _map.StaticsAt(tile.X, tile.Y))
                        {
                            _selection.Add(s);
                        }
                    }
                    else if (hit != null && !_selection.Remove(hit))
                    {
                        _selection.Add(hit);
                    }
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                    SelectMouseDown(e, tile, hit);
                    break;
                case IsoTool.Place:
                    if (_map.InBounds(tile.X, tile.Y))
                    {
                        var item = new StaticItem { Id = PlaceId, Hue = PlaceHue, X = tile.X, Y = tile.Y, Z = PlaceZ(tile.X, tile.Y) };
                        Edit(new List<StaticChange> { MapData.Add(item) }, $"{PlaceId:x4} gesetzt auf ({tile.X}, {tile.Y}, {item.Z})");
                        _selection.Clear();
                        _selection.Add(item);
                        SelectionChanged?.Invoke(this, EventArgs.Empty);
                    }
                    break;
                case IsoTool.Multi:
                    PlaceMulti(tile.X, tile.Y);
                    break;
                case IsoTool.Template:
                    PlaceTemplate(tile.X, tile.Y);
                    break;
                case IsoTool.CaveStamp:
                    if ((ModifierKeys & Keys.Control) != 0)
                    {
                        RemoveCaveEntrance(tile.X, tile.Y);
                        break;
                    }
                    // click = stamp with the settings; drag along the face = width, up the face = mouth height
                    _stampStart = tile;
                    _stampDragging = true;
                    Invalidate();
                    break;
                case IsoTool.Delete:
                    if (hit != null)
                    {
                        _selection.Remove(hit);
                        Edit(new List<StaticChange> { MapData.Remove(hit) }, $"{hit.Id:x4} gelöscht");
                    }
                    break;
                case IsoTool.Hue:
                    if (hit != null)
                    {
                        ushort hue = PlaceHue;
                        Edit(new List<StaticChange> { MapData.Modify(hit, s => s.Hue = hue) }, $"{hit.Id:x4} gefärbt mit Hue {hue}");
                    }
                    break;
            }
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
                if (_topDown)
                {
                    _camX = Math.Clamp(_panCamX - (e.X - _panStart.X) / (Cell * _zoom), 0, _map.Width - 1);
                    _camY = Math.Clamp(_panCamY - (e.Y - _panStart.Y) / (Cell * _zoom), 0, _map.Height - 1);
                    OnViewChanged();
                    return;
                }
                double du = -(e.X - _panStart.X) / (Half * _zoom), dv = -(e.Y - _panStart.Y) / (Half * _zoom);
                _camX = Math.Clamp(_panCamX + (du + dv) / 2, 0, _map.Width - 1);
                _camY = Math.Clamp(_panCamY + (dv - du) / 2, 0, _map.Height - 1);
                OnViewChanged();
                return;
            }
            Point t = TileAt(e.Location);
            ToolMouseMove(e, t);
            if (t != _hover)
            {
                _hover = t;
                HoverChanged?.Invoke(this, new MapPointEventArgs(t.X, t.Y, -1));
                if (_landStroke && e.Button == MouseButtons.Left)
                {
                    ApplyLandTool(t);
                }
                if (_areaDragging)
                {
                    _area = Rectangle.FromLTRB(Math.Min(_areaStart.X, t.X), Math.Min(_areaStart.Y, t.Y), Math.Max(_areaStart.X, t.X) + 1, Math.Max(_areaStart.Y, t.Y) + 1);
                    _area.Intersect(new Rectangle(0, 0, _map.Width, _map.Height));
                }
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_panning)
            {
                _panning = false;
                Cursor = Cursors.Default;
            }
            if (ToolMouseUp(e))
            {
                return;
            }
            if (_stampDragging)
            {
                _stampDragging = false;
                Point end = TileAt(e.Location);
                PlaceCaveStamp(_stampStart.X, _stampStart.Y, end == _stampStart || !_map.InBounds(end.X, end.Y) ? (Point?)null : end);
            }
            if (_landStroke)
            {
                _landStroke = false;
                if (Tool == IsoTool.PatternBrush && Pattern != null && Pattern.Land && _strokeCells.Count > 0)
                {
                    int minX = _strokeCells.Min(i => i % _map.Width), maxX = _strokeCells.Max(i => i % _map.Width);
                    int minY = _strokeCells.Min(i => i / _map.Width), maxY = _strokeCells.Max(i => i / _map.Width);
                    var box = Rectangle.FromLTRB(minX - 1, minY - 1, maxX + 2, maxY + 2);
                    if (Pattern.AutoFix)
                    {
                        int fixedCells = FixPatternHeights(box);
                        if (fixedCells > 0)
                        {
                            Message?.Invoke(this, $"Nachbesserung: {fixedCells} Höhen-Spitzen am Strich geglättet.");
                        }
                    }
                    if (Pattern.Transitions && LandTransitions != null)
                    {
                        // soft edges of the drawn road / river (Dragon transitions around the stroke)
                        LandTransitions.Apply(_map, box, _rnd);
                    }
                    Refresh(true);
                }
                EndBrushStroke();
                int n = _map.EndLand();
                _map.StrokeGroup = 0;
                if (n > 0)
                {
                    MapChanged?.Invoke(this, EventArgs.Empty);
                    Message?.Invoke(this, $"{n} Änderung(en) – ein Rückgängig-Schritt.");
                }
            }
            if (_areaDragging)
            {
                _areaDragging = false;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                Message?.Invoke(this, $"Bereich {_area.Width}×{_area.Height} ab ({_area.X}, {_area.Y}) markiert.");
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = new Point(-1, -1);
            Invalidate();
        }

        // ------------------------------------------------------------------ keyboard

        protected override bool IsInputKey(Keys keyData)
        {
            Keys k = keyData & Keys.KeyCode;
            return k == Keys.Left || k == Keys.Right || k == Keys.Up || k == Keys.Down || k == Keys.PageUp || k == Keys.PageDown ||
                   k == Keys.Space || k == Keys.Enter || k == Keys.Back || k == Keys.Escape || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (_map == null)
            {
                return;
            }
            if (ToolKeyDown(e))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (Tool == IsoTool.Area && !_area.IsEmpty && _selection.Count == 0)
            {
                bool done = true;
                switch (e.KeyCode)
                {
                    case Keys.C when e.Control:
                        CopyArea();
                        break;
                    case Keys.V when e.Control:
                        BeginAreaPaste();
                        break;
                    case Keys.Delete:
                        DeleteAreaStatics();
                        break;
                    case Keys.PageUp:
                        RaiseArea(e.Shift ? 5 : 1);
                        break;
                    case Keys.PageDown:
                        RaiseArea(e.Shift ? -5 : -1);
                        break;
                    case Keys.F:
                        FillArea();
                        break;
                    case Keys.K:
                        ShapeCoastArea();
                        break;
                    case Keys.J:
                        SmoothCoastArea();
                        break;
                    case Keys.W:
                        WaterArea(e.Shift);
                        break;
                    case Keys.G:
                        SmoothArea();
                        break;
                    case Keys.M:
                        MountainArea();
                        break;
                    case Keys.L:
                        AreaLogRequested?.Invoke(this, EventArgs.Empty);
                        break;
                    case Keys.T:
                        TransitionArea();
                        break;
                    case Keys.P:
                        AreaToStampRequested?.Invoke(this, EventArgs.Empty);
                        break;
                    case Keys.S:
                        SelectAreaStatics();
                        break;
                    case Keys.Escape:
                        _area = Rectangle.Empty;
                        _areaPaste = false;
                        Invalidate();
                        break;
                    default:
                        done = false;
                        break;
                }
                if (done)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
            }
            if (_areaPaste && e.KeyCode == Keys.Escape)
            {
                _areaPaste = false;
                Invalidate();
                return;
            }
            int dx = 0, dy = 0;
            switch (e.KeyCode)
            {
                case Keys.Left: dx = -1; break;
                case Keys.Right: dx = 1; break;
                case Keys.Up: dy = -1; break;
                case Keys.Down: dy = 1; break;
            }
            if (dx != 0 || dy != 0)
            {
                if (_selection.Count > 0)
                {
                    MoveSelection(dx, dy, 0, e.Control);
                }
                else
                {
                    int step = e.Shift ? 8 : 1;
                    CenterOn(_camX + dx * step, _camY + dy * step);
                }
                e.Handled = true;
                return;
            }
            bool handled = true;
            switch (e.KeyCode)
            {
                case Keys.Space:
                    _spaceDown = true;
                    break;
                case Keys.PageUp:
                    MoveSelection(0, 0, e.Shift ? 5 : 1, false);
                    break;
                case Keys.PageDown:
                    MoveSelection(0, 0, e.Shift ? -5 : -1, false);
                    break;
                case Keys.Delete:
                    DeleteSelection();
                    break;
                case Keys.Escape:
                    _pasteGhost = false;
                    _selection.Clear();
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                    break;
                case Keys.C when e.Control:
                    CopySelection();
                    break;
                case Keys.V when e.Control:
                    BeginPaste();
                    break;
                case Keys.Z when e.Control && e.Shift:
                    UndoWholeStroke();
                    break;
                case Keys.Z when e.Control:
                    Undo();
                    break;
                case Keys.Y when e.Control:
                    Redo();
                    break;
                case Keys.A when e.Control && _map.InBounds(_hover.X, _hover.Y):
                    foreach (StaticItem s in _map.StaticsAt(_hover.X, _hover.Y))
                    {
                        _selection.Add(s);
                    }
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                    break;
                case Keys.H:
                    HueSelection(PlaceHue);
                    break;
                case Keys.I:
                    if (_selection.Count > 0)
                    {
                        ItemPicked?.Invoke(this, new StaticPickEventArgs(_selection.First()));
                    }
                    break;
                case Keys.Apps:
                    ContextMenuRequested?.Invoke(this, new MapPointEventArgs(_hover.X, _hover.Y, -1));
                    break;
                case Keys.T:
                    TopDown = !TopDown;
                    Message?.Invoke(this, TopDown ? "2D-Ansicht (von oben)." : "CGI-Ansicht (wie der Client).");
                    break;
                default:
                    handled = false;
                    break;
            }
            if (handled)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode == Keys.Space)
            {
                _spaceDown = false;
            }
        }

        // ------------------------------------------------------------------ edits

        private void Edit(List<StaticChange> changes, string message)
        {
            if (changes.Count == 0)
            {
                return;
            }
            _map.Apply(changes);
            Refresh(true);
            MapChanged?.Invoke(this, EventArgs.Empty);
            Message?.Invoke(this, message);
        }

        /// <summary>Moves the selection; with <paramref name="copy"/> a moved copy is created and selected instead.</summary>
        public void MoveSelection(int dx, int dy, int dz, bool copy)
        {
            if (_map == null || _selection.Count == 0)
            {
                return;
            }
            var changes = new List<StaticChange>();
            var newSelection = new List<StaticItem>();
            foreach (StaticItem s in _selection)
            {
                int nx = s.X + dx, ny = s.Y + dy;
                if (!_map.InBounds(nx, ny))
                {
                    continue;
                }
                if (copy)
                {
                    var c = new StaticItem { Id = s.Id, Hue = s.Hue, X = nx, Y = ny, Z = Math.Clamp(s.Z + dz, -128, 127) };
                    changes.Add(MapData.Add(c));
                    newSelection.Add(c);
                }
                else
                {
                    changes.Add(MapData.Modify(s, t =>
                    {
                        t.X = nx;
                        t.Y = ny;
                        t.Z = Math.Clamp(t.Z + dz, -128, 127);
                    }));
                    newSelection.Add(s);
                }
            }
            Edit(changes, copy ? $"{changes.Count} Static(s) kopiert nach ({dx:+0;-0;0}, {dy:+0;-0;0})" : $"{changes.Count} Static(s) verschoben ({dx:+0;-0;0}, {dy:+0;-0;0}, z {dz:+0;-0;0})");
            _selection.Clear();
            newSelection.ForEach(s => _selection.Add(s));
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            if (newSelection.Count > 0)
            {
                // keep the selection in view
                StaticItem f = newSelection[0];
                if (Math.Abs(f.X - _camX) + Math.Abs(f.Y - _camY) > Math.Min(Width, Height) / (Tile * _zoom))
                {
                    CenterOn(f.X, f.Y);
                }
            }
        }

        public void DeleteSelection()
        {
            if (_map == null || _selection.Count == 0)
            {
                return;
            }
            var changes = _selection.Select(MapData.Remove).ToList();
            _selection.Clear();
            Edit(changes, $"{changes.Count} Static(s) gelöscht");
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void HueSelection(ushort hue)
        {
            if (_map == null || _selection.Count == 0)
            {
                return;
            }
            Edit(_selection.Select(s => MapData.Modify(s, t => t.Hue = hue)).ToList(), $"{_selection.Count} Static(s) gefärbt mit Hue {hue}");
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Changes id / hue / z of the whole selection (from the property panel).</summary>
        public void ModifySelection(Action<StaticItem> change, string message)
        {
            if (_map == null || _selection.Count == 0)
            {
                return;
            }
            Edit(_selection.Select(s => MapData.Modify(s, change)).ToList(), message);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void CopySelection()
        {
            if (_selection.Count == 0)
            {
                return;
            }
            _clipboard = _selection.OrderBy(s => s.X + s.Y).ThenBy(s => s.Z).Select(s => s.Copy()).ToList();
            _clipLandZ = _map.LandZ(_clipboard[0].X, _clipboard[0].Y);
            Message?.Invoke(this, $"{_clipboard.Count} Static(s) kopiert – Strg+V und klicken zum Einfügen.");
        }

        public void BeginPaste()
        {
            if (_clipboard.Count == 0)
            {
                Message?.Invoke(this, "Nichts kopiert.");
                return;
            }
            _pasteGhost = true;
            Message?.Invoke(this, "Einfügen: Linksklick setzt (mehrfach möglich), Rechtsklick / Esc beendet.");
            Invalidate();
        }

        private void PasteAt(int x, int y)
        {
            if (!_map.InBounds(x, y))
            {
                return;
            }
            StaticItem a = _clipboard[0];
            int dz = _map.LandZ(x, y) - _clipLandZ;
            var changes = new List<StaticChange>();
            _selection.Clear();
            foreach (StaticItem s in _clipboard)
            {
                var c = new StaticItem { Id = s.Id, Hue = s.Hue, X = x + s.X - a.X, Y = y + s.Y - a.Y, Z = Math.Clamp(s.Z + dz, -128, 127) };
                if (_map.InBounds(c.X, c.Y))
                {
                    changes.Add(MapData.Add(c));
                    _selection.Add(c);
                }
            }
            Edit(changes, $"{changes.Count} Static(s) eingefügt bei ({x}, {y})");
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void PlaceMulti(int x, int y)
        {
            if (!_map.InBounds(x, y))
            {
                return;
            }
            if (_multiParts.Count == 0)
            {
                Message?.Invoke(this, $"Multi {_multiId} hat keine Teile (Client-Dateien geladen?).");
                return;
            }
            int baseZ = _map.LandZ(x, y) + ZOffset;
            var changes = new List<StaticChange>();
            _selection.Clear();
            foreach (MultiPart m in _multiParts.Where(m => m.Visible || MultiInvisibleParts))
            {
                var c = new StaticItem { Id = m.Id, X = x + m.Dx, Y = y + m.Dy, Z = Math.Clamp(baseZ + m.Dz, -128, 127) };
                if (_map.InBounds(c.X, c.Y))
                {
                    changes.Add(MapData.Add(c));
                    _selection.Add(c);
                }
            }
            Edit(changes, $"Multi {_multiId} gesetzt ({changes.Count} Teile) – als Statics ausgewählt, mit den Pfeiltasten verschiebbar.");
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        // ------------------------------------------------------------------ land tools

        private void ApplyLandTool(Point center)
        {
            if (!_map.InBounds(center.X, center.Y) || center == _lastLandTile)
            {
                return;
            }
            _lastLandTile = center;
            int r = Math.Max(0, LandRadius);
            if (ApplyNewBrush(center, r))
            {
                return;
            }
            if (Tool == IsoTool.PatternBrush)
            {
                ApplyPattern(center, r);
                Refresh(true);
                return;
            }
            if (Tool == IsoTool.Coast || Tool == IsoTool.Water)
            {
                var brush = new Rectangle(center.X - r, center.Y - r, r * 2 + 1, r * 2 + 1);
                CoastEdit edit = Tool == IsoTool.Coast
                    ? CoastShaper.Shape(_map, brush, Coast, _rnd)
                    : CoastShaper.Water(_map, brush, Coast, _rnd, (ModifierKeys & Keys.Control) != 0);
                if (Tool == IsoTool.Coast && r > 1)
                {
                    // round brush: only the cells inside the circle
                    bool Inside(int x, int y) => (x - center.X) * (x - center.X) + (y - center.Y) * (y - center.Y) <= r * r + r;
                    edit.Land.RemoveAll(l => !Inside(l.x, l.y));
                    edit.Statics.RemoveAll(c => !Inside(c.Item.X, c.Item.Y));
                }
                edit.ApplyTo(_map);
                Refresh(true);
                return;
            }
            if (Tool == IsoTool.CoastSmooth)
            {
                int rr = Math.Max(1, r);
                var brush = new Rectangle(center.X - rr, center.Y - rr, rr * 2 + 1, rr * 2 + 1);
                bool Inside(int x, int y) => (x - center.X) * (x - center.X) + (y - center.Y) * (y - center.Y) <= rr * rr + rr;
                // the shore line of a cell is straightened once per stroke (no creeping when the brush passes again)
                CoastEdit edit = CoastSmoother.Smooth(_map, brush, Coast, CoastSmooth, _rnd, Inside, (x, y) => !_strokeCells.Contains(y * _map.Width + x));
                for (int y = brush.Top; y < brush.Bottom; y++)
                {
                    for (int x = brush.Left; x < brush.Right; x++)
                    {
                        if (_map.InBounds(x, y) && Inside(x, y))
                        {
                            _strokeCells.Add(y * _map.Width + x);
                        }
                    }
                }
                edit.ApplyTo(_map);
                Refresh(true);
                return;
            }
            var smoothSource = Tool == IsoTool.Smooth ? (sbyte[])_map.Z.Clone() : null;
            for (int y = center.Y - r; y <= center.Y + r; y++)
            {
                for (int x = center.X - r; x <= center.X + r; x++)
                {
                    if (!_map.InBounds(x, y) || (r > 1 && (x - center.X) * (x - center.X) + (y - center.Y) * (y - center.Y) > r * r + r))
                    {
                        continue;
                    }
                    ushort id = _map.LandId(x, y);
                    int z = _map.LandZ(x, y);
                    switch (Tool)
                    {
                        case IsoTool.LandSet:
                            {
                                ushort nid = PlaceLandIds != null && PlaceLandIds.Length > 0 ? PlaceLandIds[_rnd.Next(PlaceLandIds.Length)] : PlaceLandId;
                                // a stroke over the same tile does not reshuffle the random tiles
                                if (PlaceLandIds != null && PlaceLandIds.Length > 1 && Array.IndexOf(PlaceLandIds, id) >= 0)
                                {
                                    nid = id;
                                }
                                _map.SetLand(x, y, nid, LandFlatten ? _strokeZ : z);
                                break;
                            }
                        case IsoTool.Cave:
                            {
                                ushort[] floor = CaveFloorIds != null && CaveFloorIds.Length > 0 ? CaveFloorIds : new[] { PlaceLandId };
                                ushort nid = Array.IndexOf(floor, id) >= 0 ? id : floor[_rnd.Next(floor.Length)];
                                _map.SetLand(x, y, nid, _strokeZ);
                                break;
                            }
                        case IsoTool.CaveEntrance:
                            _map.SetLand(x, y, CaveEntranceFillId, Math.Clamp(_strokeZ + CaveEntranceMouthHeight, -128, 127));
                            _strokeCells.Add(y * _map.Width + x);
                            break;
                        case IsoTool.Mountain:
                            {
                                MountainSettings ms = Mountain ?? new MountainSettings();
                                ushort[] tiles = MountainTiles();
                                if (tiles.Length == 0)
                                {
                                    break;
                                }
                                // distance to the brush edge (1 = outermost ring), the edge frayed by noise
                                double d = r + 1 - Math.Sqrt((x - center.X) * (x - center.X) + (y - center.Y) * (y - center.Y));
                                d -= (r + 1) * Math.Clamp(ms.EdgeNoise, 0, 90) / 100.0 * MountainShaper.Smooth(x, y, Math.Max(2, ms.HillSize), ms.Seed + 51);
                                if (d < 0.5)
                                {
                                    break;
                                }
                                int footZ = ms.AutoFoot ? _strokeZ : ms.FootZ;
                                int nz = MountainShaper.Height(ms, x, y, Math.Max(1, d), footZ);
                                if (ms.OnlyRaise && nz < z)
                                {
                                    nz = z;
                                }
                                ushort nid = Array.IndexOf(tiles, id) >= 0 ? id : MountainShaper.Tile(tiles, x, y, ms.Seed);
                                _map.SetLand(x, y, nid, nz);
                                break;
                            }
                        case IsoTool.Raise:
                            _map.SetLand(x, y, id, z + Math.Max(BrushSettings.Hardness >= 100 ? 1 : 0, CgiBrushes.Dither(Math.Max(1, HeightStep) * HardnessWeight(x, y, center, r), _rnd)));
                            break;
                        case IsoTool.Lower:
                            _map.SetLand(x, y, id, z - Math.Max(BrushSettings.Hardness >= 100 ? 1 : 0, CgiBrushes.Dither(Math.Max(1, HeightStep) * HardnessWeight(x, y, center, r), _rnd)));
                            break;
                        case IsoTool.SetHeight:
                            _map.SetLand(x, y, id, TargetHeight);
                            break;
                        case IsoTool.Smooth:
                            {
                                int sum = 0, n = 0;
                                for (int dy = -1; dy <= 1; dy++)
                                {
                                    for (int dx = -1; dx <= 1; dx++)
                                    {
                                        if (_map.InBounds(x + dx, y + dy))
                                        {
                                            sum += smoothSource[(y + dy) * _map.Width + x + dx];
                                            n++;
                                        }
                                    }
                                }
                                double avg = sum / (double)n;
                                _map.SetLand(x, y, id, z + (int)Math.Round((avg - z) * HardnessWeight(x, y, center, r)));
                                break;
                            }
                    }
                }
            }
            if (Tool == IsoTool.Cave && CaveRaiseWalls)
            {
                RaiseCaveWalls(center, r + 1);
            }
            if (Tool == IsoTool.CaveEntrance)
            {
                RetileCaveEntrance(center, r + 1);
            }
            Refresh(true);
        }

        // ------------------------------------------------------------------ cave entrance

        /// <summary>
        /// Tile is mountain / rock (set by the form: mountain group of the colour table + tiledata names). Null = the tiles of
        /// <see cref="CaveEntranceMountainIds"/>, without those the height rule.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<ushort, bool> CaveIsMountain { get; set; }

        /// <summary>Cave stamp: one click cuts a complete entrance into the mountain face (tab "Höhle / Weg").</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CaveStampSettings CaveStamp { get; set; } = new CaveStampSettings();

        private CaveEntranceOptions EntranceOptions(ushort fill)
        {
            Func<ushort, bool> isMountain = CaveIsMountain;
            if (isMountain == null && CaveEntranceMountainIds != null && CaveEntranceMountainIds.Count > 0)
            {
                HashSet<ushort> ids = CaveEntranceMountainIds;
                isMountain = ids.Contains;
            }
            return new CaveEntranceOptions { FillId = fill, Patterns = CaveEntrancePatterns, IsMountain = isMountain, Rise = CaveEntranceRise };
        }

        /// <summary>Land tiles that belong to a cave entrance: 01D3–01DB, 0002 and the mouth tile of the brush.</summary>
        public bool IsCaveEntranceTile(ushort id) => CaveEntranceBuilder.IsEntranceTile(id, CaveEntranceFillId);

        /// <summary>Entrance cell with a direct (N/E/S/W) neighbour that is open ground (by tile – higher grass is still ground).</summary>
        private bool IsFrontCell(int x, int y, int groundZ, CaveEntranceOptions o)
        {
            int[] dx = { 0, 1, 0, -1 }, dy = { -1, 0, 1, 0 };
            for (int k = 0; k < 4; k++)
            {
                int nx = x + dx[k], ny = y + dy[k];
                if (!_map.InBounds(nx, ny) || IsCaveEntranceTile(_map.LandId(nx, ny)))
                {
                    continue;
                }
                if (!CaveEntranceBuilder.IsMountainCell(_map, nx, ny, groundZ, o))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gives every entrance cell around the brush its tile like the Dragon rule "1d → mountain": an edge to the mountain
        /// gets 01D3–01DA lifted to the mouth height, the front row stays on the ground (0002 = invisible like the original).
        /// </summary>
        private void RetileCaveEntrance(Point center, int r)
        {
            CaveEntranceOptions o = EntranceOptions(CaveEntranceFillId);
            var area = new Rectangle(center.X - r, center.Y - r, r * 2 + 1, r * 2 + 1);
            int Ground(int x, int y) => _strokeCells.Contains(y * _map.Width + x) ? _strokeZ : _map.LandZ(x, y) - CaveEntranceMouthHeight;
            CaveEntranceBuilder.Retile(_map, area, o, Ground, (x, y, dz) =>
            {
                if (!_strokeCells.Contains(y * _map.Width + x))
                {
                    return null;
                }
                if (IsFrontCell(x, y, _strokeZ, o))
                {
                    return _strokeZ;
                }
                return _strokeZ + CaveEntranceMouthHeight + (dz == int.MinValue ? 0 : dz);
            }, _rnd);
            if (CaveEntranceFrontNoDraw)
            {
                for (int y = area.Top; y < area.Bottom; y++)
                {
                    for (int x = area.Left; x < area.Right; x++)
                    {
                        if (_map.InBounds(x, y) && _strokeCells.Contains(y * _map.Width + x) && _map.LandId(x, y) == CaveEntranceFillId &&
                            IsFrontCell(x, y, _strokeZ, o))
                        {
                            _map.SetLand(x, y, 0x0002, _strokeZ);
                        }
                    }
                }
            }
        }

        /// <summary>Cave stamp at (x, y): complete entrance, one undo step.</summary>
        private bool _stampDragging;
        private Point _stampStart;

        /// <summary>While dragging the cave stamp: start and end tile, width / height text.</summary>
        private void DrawStampDrag(Graphics g)
        {
            if (!_stampDragging || !_map.InBounds(_stampStart.X, _stampStart.Y))
            {
                return;
            }
            using var pen = new Pen(Color.FromArgb(230, 255, 200, 0), 2f);
            g.DrawPolygon(pen, LandPoly(_stampStart.X, _stampStart.Y));
            if (_map.InBounds(_hover.X, _hover.Y) && _hover != _stampStart)
            {
                PointF[] a = LandPoly(_stampStart.X, _stampStart.Y), b = LandPoly(_hover.X, _hover.Y);
                g.DrawPolygon(pen, b);
                PointF ca = new PointF((a[0].X + a[2].X) / 2, (a[0].Y + a[2].Y) / 2), cb = new PointF((b[0].X + b[2].X) / 2, (b[0].Y + b[2].Y) / 2);
                g.DrawLine(pen, ca, cb);
                int w = Math.Max(Math.Abs(_hover.X - _stampStart.X), Math.Abs(_hover.Y - _stampStart.Y)) + 1;
                int h = _map.LandZ(_hover.X, _hover.Y) - _map.LandZ(_stampStart.X, _stampStart.Y);
                string text = $"Breite {w}, Höhe {h}";
                using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
                g.DrawString(text, font, Brushes.Black, cb.X + 9, cb.Y - 9);
                g.DrawString(text, font, Brushes.Yellow, cb.X + 8, cb.Y - 10);
            }
        }

        public void PlaceCaveStamp(int x, int y) => PlaceCaveStamp(x, y, null);

        /// <summary>Dragon map transitions (grass → dirt …) for the CGI view; null = rules not loaded.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LandTransitionEngine LandTransitions { get; set; }

        /// <summary>Ctrl+click with the cave stamp: entrance back to mountain / ground, one undo step.</summary>
        public void RemoveCaveEntrance(int x, int y)
        {
            _map.BeginLand();
            Rectangle r = CaveEntranceBuilder.Remove(_map, x, y, MountainTiles(), EntranceOptions(0x0002).IsMountain, _rnd, out string message);
            if (!r.IsEmpty && LandTransitions != null && (CaveStamp?.Transitions ?? true))
            {
                LandTransitions.Apply(_map, r, _rnd);
            }
            int n = _map.EndLand();
            Refresh(true);
            if (n > 0)
            {
                MapChanged?.Invoke(this, EventArgs.Empty);
            }
            Message?.Invoke(this, message + (n > 0 ? " Strg+Z nimmt es zurück." : string.Empty));
        }

        /// <summary>Area + T: Dragon map transitions in the marked area again (after painting roads, floors …).</summary>
        public void TransitionArea()
        {
            if (!NeedArea())
            {
                return;
            }
            if (LandTransitions == null)
            {
                Message?.Invoke(this, "Übergänge: kein Regelsatz geladen.");
                return;
            }
            _map.BeginLand();
            int c = LandTransitions.Apply(_map, _area, _rnd);
            _map.EndLand();
            Refresh(true);
            MapChanged?.Invoke(this, EventArgs.Empty);
            Message?.Invoke(this, $"Übergänge neu berechnet: {c} Tile(s) – Strg+Z nimmt es zurück.");
        }

        /// <summary>Key L in the area tool: write the analysis of the marked area (log file + clipboard).</summary>
        public event EventHandler AreaLogRequested;

        /// <summary>Key P in the area tool: marked area as colour stamp.</summary>
        public event EventHandler AreaToStampRequested;

        /// <summary>Marked area as analysis text (land ids, heights, textures, classes, entrance patterns, statics) or null.</summary>
        public string AnalyzeArea()
        {
            if (!NeedArea())
            {
                return null;
            }
            return CgiAreaAnalyzer.Analyze(_map, _area, EntranceOptions(0x0002).IsMountain, _map.Source);
        }

        public void PlaceCaveStamp(int x, int y, Point? dragEnd)
        {
            if (!_map.InBounds(x, y))
            {
                return;
            }
            _map.BeginLand();
            CaveStampSettings settings = CaveStamp ?? new CaveStampSettings();
            CaveStampResult res = CaveEntranceBuilder.Stamp(_map, x, y, settings, EntranceOptions(0x0002), _rnd, dragEnd);
            if (res.Ok && settings.Transitions && LandTransitions != null)
            {
                LandTransitions.Apply(_map, res.Region, _rnd);
            }
            int n = _map.EndLand();
            Refresh(true);
            if (n > 0)
            {
                MapChanged?.Invoke(this, EventArgs.Empty);
            }
            Message?.Invoke(this, res.Ok ? res.Message + " – Strg+Z nimmt ihn zurück." : res.Message);
        }

        /// <summary>Cells around the dug floor that are not floor become walls: at least the wall height above the floor.</summary>
        private void RaiseCaveWalls(Point center, int r)
        {
            ushort[] floor = CaveFloorIds ?? Array.Empty<ushort>();
            int wallZ = Math.Clamp(_strokeZ + Math.Max(1, CaveWallHeight), -128, 127);
            Func<ushort, bool> mountain = EntranceOptions(CaveEntranceFillId).IsMountain;
            for (int y = center.Y - r; y <= center.Y + r; y++)
            {
                for (int x = center.X - r; x <= center.X + r; x++)
                {
                    if (!_map.InBounds(x, y))
                    {
                        continue;
                    }
                    ushort id = _map.LandId(x, y);
                    int z = _map.LandZ(x, y);
                    // floor of an earlier stroke (other height) stays floor; entrance tiles stay as they are;
                    // only ground that is already above the floor (the mountain) becomes wall – the land in front
                    // of the entrance (same height as the floor) is not walled up
                    // with known mountain tiles only the mountain becomes wall – grass beside the entrance stays ground,
                    // even when it lies higher than the cave floor
                    bool isFloor = Array.IndexOf(floor, id) >= 0;
                    bool wallCandidate = mountain != null ? mountain(id) : z > _strokeZ + 2;
                    if (!isFloor && !IsCaveEntranceTile(id) && wallCandidate && z < wallZ)
                    {
                        _map.SetLand(x, y, id, wallZ);
                    }
                }
            }
        }

        // ------------------------------------------------------------------ templates

        /// <summary>Places the template with its anchor on (x, y): statics on the ground height + Z offset, land optional.</summary>
        public void PlaceTemplate(int x, int y)
        {
            MapTemplate t = _template;
            if (t == null || !_map.InBounds(x, y))
            {
                Message?.Invoke(this, "Keine Vorlage gewählt (Reiter Vorlagen).");
                return;
            }
            int groundZ = _map.LandZ(x, y);
            int baseZ = groundZ + ZOffset;
            var statics = new List<StaticChange>();
            var land = new List<LandChange>();
            Rectangle b = t.Bounds;
            if (TemplateClearStatics)
            {
                for (int my = y + b.Top; my < y + b.Bottom; my++)
                {
                    for (int mx = x + b.Left; mx < x + b.Right; mx++)
                    {
                        statics.AddRange(_map.StaticsAt(mx, my).ToList().Select(MapData.Remove));
                    }
                }
            }
            if (TemplateLand && t.HasLand)
            {
                for (int ly = 0; ly < t.LandHeight; ly++)
                {
                    for (int lx = 0; lx < t.LandWidth; lx++)
                    {
                        int mx = x + t.LandX + lx, my = y + t.LandY + ly;
                        if (!_map.InBounds(mx, my))
                        {
                            continue;
                        }
                        int k = ly * t.LandWidth + lx, i = my * _map.Width + mx;
                        int nz = TemplateLandZ && t.LandZ != null && k < t.LandZ.Length ? baseZ + t.LandZ[k] : _map.Z[i];
                        var c = new LandChange { Index = i, OldId = _map.Tiles[i], OldZ = _map.Z[i], NewId = t.LandTiles[k], NewZ = (sbyte)Math.Clamp(nz, -128, 127) };
                        if (c.NewId != c.OldId || c.NewZ != c.OldZ)
                        {
                            land.Add(c);
                        }
                    }
                }
            }
            _selection.Clear();
            if (TemplateStatics)
            {
                foreach (TemplateStatic s in t.Statics)
                {
                    var item = new StaticItem { Id = s.Id, Hue = s.Hue, X = x + s.X, Y = y + s.Y, Z = Math.Clamp(baseZ + s.Z, -128, 127) };
                    if (_map.InBounds(item.X, item.Y))
                    {
                        statics.Add(MapData.Add(item));
                        _selection.Add(item);
                    }
                }
            }
            _map.Apply(statics, land);
            Refresh(true);
            MapChanged?.Invoke(this, EventArgs.Empty);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            Message?.Invoke(this, $"'{t.Name}' gesetzt auf ({x}, {y}, z {baseZ}): {_selection.Count} Statics, {land.Count} Land-Tiles – ausgewählt: Pfeile / Bild↑↓ verschieben, Strg+Z nimmt alles zurück.");
        }

        /// <summary>
        /// Takes the marked area as a template: land tiles with heights relative to the centre and / or the statics.
        /// Anchor = centre of the area, Z 0 = land height there.
        /// </summary>
        public MapTemplate CaptureArea(bool land, bool statics)
        {
            if (_map == null || _area.IsEmpty)
            {
                return null;
            }
            int ax = _area.X + _area.Width / 2, ay = _area.Y + _area.Height / 2;
            int baseZ = _map.LandZ(ax, ay);
            var t = new MapTemplate();
            if (land)
            {
                t.LandX = _area.X - ax;
                t.LandY = _area.Y - ay;
                t.LandWidth = _area.Width;
                t.LandHeight = _area.Height;
                t.LandTiles = new ushort[_area.Width * _area.Height];
                t.LandZ = new int[_area.Width * _area.Height];
            }
            for (int y = _area.Top; y < _area.Bottom; y++)
            {
                for (int x = _area.Left; x < _area.Right; x++)
                {
                    if (land)
                    {
                        int k = (y - _area.Y) * _area.Width + x - _area.X;
                        t.LandTiles[k] = _map.LandId(x, y);
                        t.LandZ[k] = _map.LandZ(x, y) - baseZ;
                    }
                    if (statics)
                    {
                        foreach (StaticItem s in _map.StaticsAt(x, y))
                        {
                            t.Statics.Add(new TemplateStatic { Id = s.Id, Hue = s.Hue, X = x - ax, Y = y - ay, Z = s.Z - baseZ });
                        }
                    }
                }
            }
            return t;
        }

        /// <summary>The selected statics as a template (anchor = centre, Z 0 = land height there).</summary>
        public MapTemplate CaptureSelection()
        {
            if (_map == null || _selection.Count == 0)
            {
                return null;
            }
            int ax = (_selection.Min(s => s.X) + _selection.Max(s => s.X)) / 2, ay = (_selection.Min(s => s.Y) + _selection.Max(s => s.Y)) / 2;
            int baseZ = _map.LandZ(ax, ay);
            var t = new MapTemplate();
            t.Statics.AddRange(_selection.Select(s => new TemplateStatic { Id = s.Id, Hue = s.Hue, X = s.X - ax, Y = s.Y - ay, Z = s.Z - baseZ }));
            return t;
        }

        /// <summary>Selects all statics of the marked area (then move / copy / delete them like a selection).</summary>
        public void SelectAreaStatics()
        {
            if (!NeedArea())
            {
                return;
            }
            _selection.Clear();
            for (int y = _area.Top; y < _area.Bottom; y++)
            {
                for (int x = _area.Left; x < _area.Right; x++)
                {
                    foreach (StaticItem s in _map.StaticsAt(x, y))
                    {
                        _selection.Add(s);
                    }
                }
            }
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
            Message?.Invoke(this, $"{_selection.Count} Statics im Bereich ausgewählt – Pfeile verschieben, Strg+Pfeil kopiert, Entf löscht, Strg+C kopiert.");
        }

        public void CopyArea()
        {
            if (_area.IsEmpty)
            {
                return;
            }
            var clip = new AreaClip { W = _area.Width, H = _area.Height, Tiles = new ushort[_area.Width * _area.Height], Z = new sbyte[_area.Width * _area.Height] };
            for (int y = 0; y < clip.H; y++)
            {
                for (int x = 0; x < clip.W; x++)
                {
                    int src = (_area.Y + y) * _map.Width + _area.X + x;
                    clip.Tiles[y * clip.W + x] = _map.Tiles[src];
                    clip.Z[y * clip.W + x] = _map.Z[src];
                    foreach (StaticItem s in _map.StaticsAt(_area.X + x, _area.Y + y))
                    {
                        StaticItem c = s.Copy();
                        c.X = x;
                        c.Y = y;
                        clip.Statics.Add(c);
                    }
                }
            }
            clip.BaseZ = clip.Z[0];
            _areaClip = clip;
            Message?.Invoke(this, $"Bereich {clip.W}×{clip.H} kopiert ({clip.Statics.Count} Statics) – Strg+V und klicken zum Einfügen (Höhe wird an die Zielstelle angepasst).");
        }

        public void BeginAreaPaste()
        {
            if (_areaClip == null)
            {
                Message?.Invoke(this, "Kein Bereich kopiert.");
                return;
            }
            _areaPaste = true;
            Invalidate();
        }

        private void PasteArea(int tx, int ty)
        {
            AreaClip clip = _areaClip;
            int dz = _map.LandZ(tx, ty) - clip.BaseZ;
            var land = new List<LandChange>();
            var statics = new List<StaticChange>();
            for (int y = 0; y < clip.H; y++)
            {
                for (int x = 0; x < clip.W; x++)
                {
                    int mx = tx + x, my = ty + y;
                    if (!_map.InBounds(mx, my))
                    {
                        continue;
                    }
                    int i = my * _map.Width + mx;
                    land.Add(new LandChange { Index = i, OldId = _map.Tiles[i], OldZ = _map.Z[i], NewId = clip.Tiles[y * clip.W + x], NewZ = (sbyte)Math.Clamp(clip.Z[y * clip.W + x] + dz, -128, 127) });
                    foreach (StaticItem old in _map.StaticsAt(mx, my).ToList())
                    {
                        statics.Add(MapData.Remove(old));
                    }
                }
            }
            foreach (StaticItem s in clip.Statics)
            {
                var c = new StaticItem { Id = s.Id, Hue = s.Hue, X = tx + s.X, Y = ty + s.Y, Z = Math.Clamp(s.Z + dz, -128, 127) };
                if (_map.InBounds(c.X, c.Y))
                {
                    statics.Add(MapData.Add(c));
                }
            }
            _map.Apply(statics, land);
            _area = new Rectangle(tx, ty, clip.W, clip.H);
            _area.Intersect(new Rectangle(0, 0, _map.Width, _map.Height));
            Refresh(true);
            MapChanged?.Invoke(this, EventArgs.Empty);
            Message?.Invoke(this, $"Bereich eingefügt bei ({tx}, {ty}) – weiter klicken für mehr, Esc beendet.");
        }

        public void DeleteAreaStatics()
        {
            if (_area.IsEmpty)
            {
                return;
            }
            var changes = new List<StaticChange>();
            for (int y = _area.Top; y < _area.Bottom; y++)
            {
                for (int x = _area.Left; x < _area.Right; x++)
                {
                    changes.AddRange(_map.StaticsAt(x, y).ToList().Select(MapData.Remove));
                }
            }
            _selection.Clear();
            Edit(changes, $"{changes.Count} Statics im Bereich gelöscht");
        }

        /// <summary>Raises / lowers the land (and its statics) of the marked area.</summary>
        public void RaiseArea(int dz)
        {
            if (_area.IsEmpty)
            {
                return;
            }
            var land = new List<LandChange>();
            var statics = new List<StaticChange>();
            for (int y = _area.Top; y < _area.Bottom; y++)
            {
                for (int x = _area.Left; x < _area.Right; x++)
                {
                    int i = y * _map.Width + x;
                    land.Add(new LandChange { Index = i, OldId = _map.Tiles[i], OldZ = _map.Z[i], NewId = _map.Tiles[i], NewZ = (sbyte)Math.Clamp(_map.Z[i] + dz, -128, 127) });
                    statics.AddRange(_map.StaticsAt(x, y).ToList().Select(s => MapData.Modify(s, t => t.Z = Math.Clamp(t.Z + dz, -128, 127))));
                }
            }
            _map.Apply(statics, land);
            Refresh(true);
            MapChanged?.Invoke(this, EventArgs.Empty);
            Message?.Invoke(this, $"Bereich um {dz:+0;-0} angehoben/abgesenkt (Land + Statics).");
        }

        private bool NeedArea()
        {
            if (_map != null && !_area.IsEmpty)
            {
                return true;
            }
            Message?.Invoke(this, "Erst einen Bereich markieren (Werkzeug 'Bereich', mit der Maus aufziehen).");
            return false;
        }

        private void LandArea(Func<int, int, (ushort id, int z)> change, string message)
        {
            if (!NeedArea())
            {
                return;
            }
            var land = new List<LandChange>();
            for (int y = _area.Top; y < _area.Bottom; y++)
            {
                for (int x = _area.Left; x < _area.Right; x++)
                {
                    int i = y * _map.Width + x;
                    (ushort id, int z) = change(x, y);
                    var c = new LandChange { Index = i, OldId = _map.Tiles[i], OldZ = _map.Z[i], NewId = id, NewZ = (sbyte)Math.Clamp(z, -128, 127) };
                    if (c.NewId != c.OldId || c.NewZ != c.OldZ)
                    {
                        land.Add(c);
                    }
                }
            }
            _map.Apply(null, land);
            Refresh(true);
            MapChanged?.Invoke(this, EventArgs.Empty);
            Message?.Invoke(this, $"{message}: {land.Count} Tile(s).");
        }

        /// <summary>Fills the marked area with one of the given tiles (random), heights stay.</summary>
        public void FillArea(ushort[] ids, string name)
        {
            if (ids == null || ids.Length == 0)
            {
                return;
            }
            LandArea((x, y) =>
            {
                ushort id = _map.LandId(x, y);
                return (Array.IndexOf(ids, id) >= 0 ? id : ids[_rnd.Next(ids.Length)], _map.LandZ(x, y));
            }, $"Bereich mit {name} gefüllt");
        }

        /// <summary>Fills the marked area with the land tile of the land tools (heights stay).</summary>
        public void FillArea()
        {
            ushort[] ids = PlaceLandIds != null && PlaceLandIds.Length > 0 ? PlaceLandIds : new[] { PlaceLandId };
            FillArea(ids, string.Join(" ", ids.Take(4).Select(i => i.ToString("x4"))));
        }

        /// <summary>Sets the land of the marked area to <see cref="TargetHeight"/>.</summary>
        public void SetAreaHeight() => LandArea((x, y) => (_map.LandId(x, y), TargetHeight), $"Bereich auf Höhe {TargetHeight}");

        /// <summary>Smooths the land heights of the marked area.</summary>
        public void SmoothArea()
        {
            if (!NeedArea())
            {
                return;
            }
            sbyte[] z = (sbyte[])_map.Z.Clone();
            LandArea((x, y) =>
            {
                int sum = 0, n = 0;
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (_map.InBounds(x + dx, y + dy))
                        {
                            sum += z[(y + dy) * _map.Width + x + dx];
                            n++;
                        }
                    }
                }
                return (_map.LandId(x, y), (int)Math.Round(sum / (double)n));
            }, "Bereich geglättet");
        }

        /// <summary>Mountain over the marked area (settings <see cref="Mountain"/>), one undo step.</summary>
        public void MountainArea()
        {
            if (!NeedArea())
            {
                return;
            }
            ushort[] tiles = MountainTiles();
            if (tiles.Length == 0)
            {
                Message?.Invoke(this, "Berg: keine Berg-Tiles (Reiter Berg).");
                return;
            }
            var land = new List<LandChange>();
            foreach ((int x, int y, ushort id, int z) in MountainShaper.ShapeArea(_map, _area, Mountain ?? new MountainSettings(), tiles))
            {
                int i = y * _map.Width + x;
                var c = new LandChange { Index = i, OldId = _map.Tiles[i], OldZ = _map.Z[i], NewId = id, NewZ = (sbyte)Math.Clamp(z, -128, 127) };
                if (c.NewId != c.OldId || c.NewZ != c.OldZ)
                {
                    land.Add(c);
                }
            }
            _map.Apply(null, land);
            Refresh(true);
            MapChanged?.Invoke(this, EventArgs.Empty);
            Message?.Invoke(this, $"Berg geformt: {land.Count} Tile(s) – Strg+Z nimmt ihn zurück, andere 'Zufalls-Zahl' = anderer Berg.");
        }

        /// <summary>Coast of the marked area like the original map.</summary>
        public void ShapeCoastArea()
        {
            if (!NeedArea())
            {
                return;
            }
            CoastEdit edit = CoastShaper.Shape(_map, _area, Coast, _rnd);
            edit.ApplyTo(_map);
            Refresh(true);
            MapChanged?.Invoke(this, EventArgs.Empty);
            Message?.Invoke(this, edit.Count == 0 ? "Küste: nichts zu ändern (kein Wasser / Land im Bereich?)." : $"Küste angepasst: {edit.Land.Count} Land-Tiles, {edit.Statics.Count} Statics.");
        }

        /// <summary>Water statics on the marked area (or removes them).</summary>
        public void WaterArea(bool erase)
        {
            if (!NeedArea())
            {
                return;
            }
            CoastEdit edit = CoastShaper.Water(_map, _area, Coast, _rnd, erase);
            edit.ApplyTo(_map);
            Refresh(true);
            MapChanged?.Invoke(this, EventArgs.Empty);
            Message?.Invoke(this, erase ? $"{edit.Statics.Count} Wasser-Statics entfernt." : $"Wasser gesetzt: {edit.Statics.Count} Statics, {edit.Land.Count} Land-Tiles.");
        }

        /// <summary>Key and mouse help for the current tool (shown below the view).</summary>
        public string KeyHelp()
        {
            string view = L.T(_topDown ? "2D-Ansicht" : "CGI-Ansicht") + L.T(
                          ": Mausrad = Zoom · mittlere Maus oder Leertaste + Ziehen = verschieben · Pfeile ohne Auswahl = Ansicht (Umschalt = 8 Tiles) · T = 2D / CGI · Strg+Z / Strg+Y = Rückgängig / Wiederholen") +
                          L.T(" · Umschalt+Rechtsklick / Menütaste = Menü · F1 = Hilfe");
            string tool;
            if (_pasteGhost)
            {
                tool = L.T("Einfügen: Linksklick setzt die kopierten Statics (mehrfach möglich) · Rechtsklick oder Esc = fertig");
            }
            else if (_areaPaste)
            {
                tool = L.T("Bereich einfügen: Linksklick setzt Land + Statics (Höhe passt sich an) · Esc = fertig");
            }
            else
            {
                switch (Tool)
                {
                    case IsoTool.Select:
                        tool = L.T("Auswahl: Klick = wählen · Strg+Klick = dazu · Umschalt+Klick / Strg+A = alles auf dem Tile · Pfeile = verschieben · Strg+Pfeil = Kopie daneben · " +
                               "Bild↑/↓ = Z ±1 (Umschalt ±5) · Entf = löschen · H = färben · I = im Item-Katalog zeigen · Strg+C / Strg+V = kopieren / einfügen · Rechtsklick = Menü (Item aufnehmen, Ansicht, Werkzeuge …) · Esc = Auswahl weg");
                        break;
                    case IsoTool.Place:
                        tool = L.T("Static setzen: Klick setzt das Item aus dem Katalog (Reiter Items) · Z + und 'auf Statics stapeln' rechts · Rechtsklick auf ein Static = ID + Hue aufnehmen");
                        break;
                    case IsoTool.Multi:
                        tool = L.T("Multi setzen: Klick setzt das Multi (Reiter Multis) als Statics, danach ist es ausgewählt und mit den Pfeilen verschiebbar");
                        break;
                    case IsoTool.Delete:
                        tool = L.T("Löschen: Klick auf ein Static löscht es");
                        break;
                    case IsoTool.Hue:
                        tool = L.T("Färben: Klick färbt das Static mit der Hue (Reiter Items) · mit Auswahl: H");
                        break;
                    case IsoTool.LandSet:
                        tool = L.T("Land setzen: Klicken / Ziehen malt das Land-Tile aus dem Katalog (Reiter Land) · Pinsel oben · Rechtsklick = Land-ID + Höhe aufnehmen · ein Zug = ein Rückgängig-Schritt");
                        break;
                    case IsoTool.Raise:
                    case IsoTool.Lower:
                        tool = L.T("Höhe + / −: Klicken / Ziehen hebt oder senkt das Land um den Schritt (Reiter Land) · Pinsel oben · Rechtsklick = Land aufnehmen");
                        break;
                    case IsoTool.SetHeight:
                        tool = L.T("Höhe =: Klicken / Ziehen setzt das Land auf 'Höhe =' (Reiter Land) · Rechtsklick nimmt die Höhe auf");
                        break;
                    case IsoTool.Smooth:
                        tool = L.T("Glätten: Ziehen gleicht die Höhen an die Nachbarn an · Pinsel oben");
                        break;
                    case IsoTool.Coast:
                        tool = L.T("Küste: Ziehen über die Küste formt sie wie das Original – Meeresgrund, Wasser-Statics, Strand-Höhen (Reiter Küste / Wasser) · Pinsel oben · ein Zug = ein Rückgängig-Schritt");
                        break;
                    case IsoTool.CoastSmooth:
                        tool = L.T("Küste glätten: runder Pinsel – Ziehen über die Küste macht die Ufer-Höhen weich (keine Sand-Zacken), begradigt Spitzen und 1-Tile-Buchten, Statics gehen mit (Reiter Küste / Wasser) · Pinsel oben · Bereich + J = ganzer Bereich · ein Zug = ein Rückgängig-Schritt");
                        break;
                    case IsoTool.Water:
                        tool = L.T("Wasser: Ziehen malt Wasser-Statics (Reiter Küste / Wasser) · Strg+Ziehen entfernt sie · durchsichtig, wenn die TileData 'Translucent' hat");
                        break;
                    case IsoTool.Area:
                        tool = L.T("Bereich: Ziehen = markieren · P = als Farb-Stempel speichern · F = füllen · K = Küste · W = Wasser (Umschalt+W weg) · G = glätten · J = Küste glätten (ohne Zacken) · M = Berg formen (Reiter Berg) · L = Bereich auswerten (Log + Zwischenablage) · T = Übergänge neu berechnen · S = Statics darin auswählen · Bild↑/↓ = heben / senken · " +
                               "Strg+C / Strg+V = Land + Statics kopieren / einfügen · Entf = Statics löschen · als Vorlage speichern: Reiter Vorlagen · Rechtsklick = Menü mit allen Befehlen · Esc = Markierung weg");
                        break;
                    case IsoTool.Cave:
                        tool = L.T("Höhle: am Eingang anfangen und in den Berg ziehen – Boden auf der Höhe des ersten Klicks (oder 'Höhe ='), der Rand wird Wand (Reiter Höhle / Weg) · Pinsel oben · ein Zug = ein Rückgängig-Schritt");
                        break;
                    case IsoTool.CaveEntrance:
                        tool = L.T("Höhleneingang: am Fuß des Berges klicken / ein Stück in den Hang ziehen (mind. 2×2) – Mund 01DB (schwarz) oder 0002 (unsichtbar) liegt 'Mund-Höhe' über dem Boden und wird als steile Wand in den Hang gezogen – sichtbar nur, wenn der Eingang nach Süden / Osten zeigt; der Rand zum Berg bekommt automatisch 01D3–01DA (Reiter Höhle / Weg) · ein Zug = ein Rückgängig-Schritt");
                        break;
                    case IsoTool.PatternBrush:
                        tool = L.T("Muster-Pinsel: Freihand = ziehen, das Muster läuft wie eine Textur weiter (Straßen, Flüsse) · Einmal = Klick setzt das ganze Muster · Pinselgröße oben · Übergänge zum Rand danach · Strg+Z = Schritt / Strich, Strg+Umschalt+Z = ganzer Strich");
                        break;
                    case IsoTool.CaveStamp:
                        tool = L.T("Höhlen-Stempel: auf den Boden direkt vor einer Bergwand klicken (Süd- oder Ostseite) – Boden-Höhe vorn und Berg-Höhe dahinter werden übernommen, Vorderkante 0002, Bogen 01D3–01DA, Weg davor (Reiter Höhlen-Stempel) · Ziehen = Breite + Höhe · Strg+Klick = Eingang entfernen · Strg+Z = zurück");
                        break;
                    case IsoTool.Mountain:
                        tool = L.T("Berg: Ziehen baut einen Berg ab der Bodenhöhe des ersten Klicks (oder 'Fuß-Z') auf – steil, mit Kuppen und Felskanten (Reiter Berg) · Pinsel oben = Breite · mehrmals ziehen = Grat · Bereich + M = ganzer Berg");
                        break;
                    case IsoTool.Template:
                        tool = L.T("Vorlage setzen: die Vorschau folgt der Maus, Klick setzt auf die Boden-Höhe + 'Z +' · danach ausgewählt: Pfeile / Bild↑↓ verschieben, Entf löscht · Strg+Z nimmt alles zurück");
                        break;
                    default:
                        tool = NewToolHelp(Tool) ?? string.Empty;
                        break;
                }
            }
            return tool + Environment.NewLine + view;
        }

        /// <summary>All parts of the last stroke (pattern brush drawn stepwise).</summary>
        public void UndoWholeStroke()
        {
            if (_map != null && _map.UndoWholeGroup())
            {
                _selection.Clear();
                Refresh(true);
                MapChanged?.Invoke(this, EventArgs.Empty);
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                Message?.Invoke(this, "Ganzer Strich rückgängig.");
            }
        }

        // ------------------------------------------------------------------ pattern brush

        /// <summary>Pattern of the pattern brush (from a stamp).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TilePattern Pattern { get; set; }

        private bool _patternOnce;
        private int _patternStepCells;

        private static int Mod(int a, int m) => ((a % m) + m) % m;

        /// <summary>
        /// Automatic repair after a pattern stroke: a cell of the stroke or its border that lies more than 6 Z above / below
        /// the average of its 8 neighbours gets that average (no single spikes, steps or holes at the edges).
        /// Statics on it move with the land.
        /// </summary>
        private int FixPatternHeights(Rectangle box)
        {
            box.Intersect(new Rectangle(0, 0, _map.Width, _map.Height));
            var before = (sbyte[])_map.Z.Clone();
            int n = 0;
            var moves = new List<StaticChange>();
            for (int y = box.Top; y < box.Bottom; y++)
            {
                for (int x = box.Left; x < box.Right; x++)
                {
                    int sum = 0, c = 0;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if ((dx != 0 || dy != 0) && _map.InBounds(x + dx, y + dy))
                            {
                                sum += before[(y + dy) * _map.Width + x + dx];
                                c++;
                            }
                        }
                    }
                    if (c == 0)
                    {
                        continue;
                    }
                    int avg = (int)Math.Round(sum / (double)c);
                    int z = before[y * _map.Width + x];
                    if (Math.Abs(z - avg) > 6)
                    {
                        _map.SetLand(x, y, _map.LandId(x, y), avg);
                        foreach (StaticItem s in _map.StaticsAt(x, y).ToList())
                        {
                            StaticItem moved = s.Copy();
                            moved.Z = Math.Clamp(s.Z + avg - z, -128, 127);
                            moves.Add(new StaticChange { Item = s, Before = s.Copy(), After = moved });
                        }
                        n++;
                    }
                }
            }
            _map.ApplyStatics(moves);
            return n;
        }

        /// <summary>Freehand: the cells under the brush get the pattern cell of their position; once: the whole pattern at the click.</summary>
        private void ApplyPattern(Point center, int r)
        {
            TilePattern p = Pattern;
            if (p == null || p.Width <= 0 || p.Height <= 0)
            {
                Message?.Invoke(this, "Muster-Pinsel: erst einen Stempel wählen (Knopf 'Muster-Pinsel').");
                return;
            }
            var cells = new List<(int x, int y, int k)>();
            if (!p.Freehand)
            {
                if (_patternOnce)
                {
                    return;
                }
                _patternOnce = true;
                int ox = center.X - p.Width / 2, oy = center.Y - p.Height / 2;
                for (int py = 0; py < p.Height; py++)
                {
                    for (int px = 0; px < p.Width; px++)
                    {
                        cells.Add((ox + px, oy + py, py * p.Width + px));
                    }
                }
            }
            else
            {
                for (int y = center.Y - r; y <= center.Y + r; y++)
                {
                    for (int x = center.X - r; x <= center.X + r; x++)
                    {
                        if (r > 1 && (x - center.X) * (x - center.X) + (y - center.Y) * (y - center.Y) > r * r + r)
                        {
                            continue;
                        }
                        cells.Add((x, y, Mod(y, p.Height) * p.Width + Mod(x, p.Width)));
                    }
                }
            }
            var statics = new List<StaticChange>();
            int painted = 0;
            foreach ((int x, int y, int k) in cells)
            {
                if (!_map.InBounds(x, y) || !_strokeCells.Add(y * _map.Width + x))
                {
                    continue;
                }
                painted++;
                int landZ = _map.LandZ(x, y);
                if (p.Land && p.HasLand)
                {
                    landZ = p.HeightFor(landZ, k);
                    _map.SetLand(x, y, p.Tiles[k], landZ);
                }
                if (p.StaticsOn && p.Statics.TryGetValue(k, out List<LayerStatic> l))
                {
                    if (p.Land && p.HasLand)
                    {
                        // a new road / floor replaces what stood there
                        statics.AddRange(_map.StaticsAt(x, y).ToList().Select(MapData.Remove));
                    }
                    foreach (LayerStatic s in l)
                    {
                        statics.Add(MapData.Add(new StaticItem { Id = s.Id, Hue = s.Hue, X = x, Y = y, Z = Math.Clamp(landZ + s.Dz, -128, 127) }));
                    }
                }
            }
            _map.ApplyStatics(statics);
            if (p.StepCells > 0 && p.Freehand)
            {
                _patternStepCells += painted;
                if (_patternStepCells >= p.StepCells)
                {
                    // undo per part: close this step, the stroke goes on in the next one (same group)
                    _patternStepCells = 0;
                    _map.EndLand();
                    _map.BeginLand();
                    MapChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void Undo()
        {
            if (_map != null && _map.Undo())
            {
                _selection.Clear();
                Refresh(true);
                MapChanged?.Invoke(this, EventArgs.Empty);
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                Message?.Invoke(this, "Rückgängig.");
            }
        }

        public void Redo()
        {
            if (_map != null && _map.Redo())
            {
                _selection.Clear();
                Refresh(true);
                MapChanged?.Invoke(this, EventArgs.Empty);
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                Message?.Invoke(this, "Wiederholt.");
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _sceneDirty = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _scene?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

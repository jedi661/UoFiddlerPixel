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
    public enum IsoTool
    {
        Select,
        Place,
        Multi,
        Delete,
        Hue
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
    public class IsoMapView : Control
    {
        private const int Tile = 44;
        private const int Half = 22;
        private const int ZStep = 4;

        private sealed class Placed
        {
            public StaticItem Item;
            public Rectangle Rect;
            public Bitmap Image;
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
        private int _multiId = -1;

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
                double r = (Width + Height) / (Tile * _zoom) / 1.2;
                return new RectangleF((float)(_camX - r / 2), (float)(_camY - r / 2), (float)r, (float)r);
            }
        }

        // ------------------------------------------------------------------ setup

        public void SetMap(MapData map)
        {
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
            double[] steps = { 0.25, 0.375, 0.5, 0.75, 1, 1.5, 2, 3, 4 };
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

        private Point ToScene(Point p) => new Point((int)(p.X / _zoom), (int)(p.Y / _zoom));

        /// <summary>Tile under a screen point (the land height is taken into account).</summary>
        public Point TileAt(Point screen)
        {
            if (_map == null)
            {
                return new Point(-1, -1);
            }
            Point s = ToScene(screen);
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

        private void RenderScene()
        {
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

            var drawables = new List<(int depth, int prio, int order, Action draw)>(4096);
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
                        drawables.Add((x + y, zMin * 4 - 2, order++, () => DrawLand(g, landId, poly, zt, zr, zb, zl)));
                    }

                    if (!ShowStatics)
                    {
                        continue;
                    }
                    foreach (StaticItem st in _map.StaticsAt(x, y))
                    {
                        if (st.Z > MaxZ)
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
                        drawables.Add((x + y, (st.Z + threshold) * 4 + threshold, order++, () =>
                        {
                            if (img != null)
                            {
                                g.DrawImageUnscaled(img, rect.X, rect.Y);
                            }
                            else
                            {
                                using var b = new SolidBrush(UoArtCache.ItemColor(item.Id));
                                g.FillRectangle(b, rect);
                                g.DrawRectangle(Pens.Black, rect);
                            }
                            _placed.Add(placed);
                        }));
                    }
                }
            }

            g.SmoothingMode = SmoothingMode.None;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            foreach (var d in drawables.OrderBy(d => d.depth).ThenBy(d => d.prio).ThenBy(d => d.order))
            {
                d.draw();
            }
        }

        private void DrawLand(Graphics g, ushort landId, PointF[] poly, int zt, int zr, int zb, int zl)
        {
            bool flat = zt == zr && zr == zb && zb == zl;
            Bitmap art = flat ? UoArtCache.Land(landId) : null;
            if (art != null)
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(art, new RectangleF(poly[3].X, poly[0].Y, Tile, Tile));
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

        /// <summary>Maps a square texture onto the 4 land corners (two triangles, light by slope).</summary>
        private static void DrawTextured(Graphics g, Bitmap tex, PointF[] p, float light, bool diamondArt = false)
        {
            PointF t = p[0], r = p[1], b = p[2], l = p[3];
            using var attr = new ImageAttributes();
            attr.SetColorMatrix(new ColorMatrix(new[]
            {
                new[] { light, 0f, 0f, 0f, 0f },
                new[] { 0f, light, 0f, 0f, 0f },
                new[] { 0f, 0f, light, 0f, 0f },
                new[] { 0f, 0f, 0f, 1f, 0f },
                new[] { 0f, 0f, 0f, 0f, 1f }
            }));
            g.InterpolationMode = InterpolationMode.Bilinear;
            RectangleF src = diamondArt
                ? new RectangleF(0, 0, tex.Width, tex.Height)
                : new RectangleF(0, 0, tex.Width, tex.Height);
            if (diamondArt)
            {
                // the land art is already a diamond: map its bounding box onto the corner quad
                PointF ul = new PointF(l.X, t.Y), ur = new PointF(r.X, t.Y), ll = new PointF(l.X, b.Y);
                g.DrawImage(tex, new[] { ul, ur, ll }, src, GraphicsUnit.Pixel, attr);
                return;
            }
            using (var clip = new GraphicsPath())
            {
                clip.AddPolygon(new[] { t, r, l });
                g.SetClip(clip);
                g.DrawImage(tex, new[] { t, r, l }, src, GraphicsUnit.Pixel, attr);
            }
            using (var clip = new GraphicsPath())
            {
                clip.AddPolygon(new[] { r, b, l });
                g.SetClip(clip);
                var ul = new PointF(r.X + l.X - b.X, r.Y + l.Y - b.Y);
                g.DrawImage(tex, new[] { ul, r, l }, src, GraphicsUnit.Pixel, attr);
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
            g.DrawImage(_scene, new Rectangle(0, 0, (int)(_scene.Width * _zoom), (int)(_scene.Height * _zoom)));

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

            if (_map.InBounds(_hover.X, _hover.Y))
            {
                using var pen = new Pen(Color.FromArgb(220, 255, 255, 255), 1.5f);
                g.DrawPolygon(pen, LandPoly(_hover.X, _hover.Y));
                DrawGhost(g);
            }
        }

        private PointF[] LandPoly(int x, int y)
        {
            PointF top = TileTop(x, y);
            int zt = _map.LandZ(x, y), zr = _map.LandZ(x + 1, y), zb = _map.LandZ(x + 1, y + 1), zl = _map.LandZ(x, y + 1);
            return new[]
            {
                new PointF(top.X, top.Y - zt * ZStep), new PointF(top.X + Half, top.Y + Half - zr * ZStep),
                new PointF(top.X, top.Y + Tile - zb * ZStep), new PointF(top.X - Half, top.Y + Half - zl * ZStep)
            };
        }

        /// <summary>Semi transparent preview of the item / multi that would be placed.</summary>
        private void DrawGhost(Graphics g)
        {
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
                PointF top = TileTop(x, y);
                float bottom = top.Y + Tile - z * ZStep;
                if (img != null)
                {
                    g.DrawImage(img, new Rectangle((int)(top.X - img.Width / 2f), (int)(bottom - img.Height), img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attr);
                }
                else
                {
                    g.DrawRectangle(Pens.Orange, top.X - 8, bottom - 20, 16, 16);
                }
            }
        }

        private bool _pasteGhost;

        private int PlaceZ(int x, int y)
        {
            int baseZ = StackOnStatics ? _map.TopZ(x, y, id => UoArtCache.ItemHeight(id)) : _map.LandZ(x, y);
            return Math.Clamp(baseZ + ZOffset, -128, 127);
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
                    if (s.Image.GetPixel(p.X - s.Rect.X, p.Y - s.Rect.Y).A > 16)
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
                double du = -(e.X - _panStart.X) / (Half * _zoom), dv = -(e.Y - _panStart.Y) / (Half * _zoom);
                _camX = Math.Clamp(_panCamX + (du + dv) / 2, 0, _map.Width - 1);
                _camY = Math.Clamp(_panCamY + (dv - du) / 2, 0, _map.Height - 1);
                OnViewChanged();
                return;
            }
            Point t = TileAt(e.Location);
            if (t != _hover)
            {
                _hover = t;
                HoverChanged?.Invoke(this, new MapPointEventArgs(t.X, t.Y, -1));
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
                   k == Keys.Space || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (_map == null)
            {
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

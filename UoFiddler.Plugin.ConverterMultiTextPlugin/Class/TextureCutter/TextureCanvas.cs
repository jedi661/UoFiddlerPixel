using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextureCutter
{
    internal enum CanvasTool { Select, Pencil, Eraser, Line, Rectangle, Fill, Pipette, Overlay, MaskPaint, Wand, Stamp }

    internal enum CanvasSelection { Rectangle, Ellipse, Freehand, Polygon }

    /// <summary>An image layer above the base image: can be moved, scaled, faded and burned in later.</summary>
    internal sealed class CanvasLayer : IDisposable
    {
        private bool _blackWhiteTransparent = true;

        public CanvasLayer(Bitmap source, string name)
        {
            Source = source;
            Name = name;
            Rebuild();
        }

        public string Name { get; set; }

        /// <summary>The image as loaded (owned).</summary>
        public Bitmap Source { get; private set; }

        /// <summary>The image as shown (black / white removed if wanted).</summary>
        public Bitmap Image { get; private set; }

        public Point Position { get; set; }

        public int Scale { get; set; } = 100;

        public int Opacity { get; set; } = 100;

        public bool Visible { get; set; } = true;

        public bool BlackWhiteTransparent
        {
            get => _blackWhiteTransparent;
            set
            {
                _blackWhiteTransparent = value;
                Rebuild();
            }
        }

        public Size Size => new Size(Math.Max(1, Image.Width * Scale / 100), Math.Max(1, Image.Height * Scale / 100));

        public Rectangle Bounds => new Rectangle(Position, Size);

        private void Rebuild()
        {
            Bitmap img = CutterImaging.To32Bpp(Source);
            if (_blackWhiteTransparent)
            {
                CutterImaging.MakeTransparent(img, 0, new[] { Color.Black, Color.White });
            }
            Image?.Dispose();
            Image = img;
        }

        /// <summary>Draws the layer (position, scale, opacity) onto a graphics in image pixels.</summary>
        public void DrawTo(Graphics g, float zoom)
        {
            var dest = new Rectangle((int)(Position.X * zoom), (int)(Position.Y * zoom), (int)(Size.Width * zoom), (int)(Size.Height * zoom));
            using (var attributes = new ImageAttributes())
            {
                attributes.SetColorMatrix(new ColorMatrix { Matrix33 = Math.Max(0, Math.Min(100, Opacity)) / 100f });
                InterpolationMode oldMode = g.InterpolationMode;
                PixelOffsetMode oldOffset = g.PixelOffsetMode;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(Image, dest, 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel, attributes);
                g.InterpolationMode = oldMode;
                g.PixelOffsetMode = oldOffset;
            }
        }

        public CanvasLayer Clone() => new CanvasLayer(CutterImaging.To32Bpp(Source), Name + " copy")
        {
            Position = Position,
            Scale = Scale,
            Opacity = Opacity,
            Visible = Visible,
            BlackWhiteTransparent = BlackWhiteTransparent
        };

        public override string ToString() => Name;

        public void Dispose()
        {
            Source?.Dispose();
            Image?.Dispose();
        }
    }

    /// <summary>
    /// Drawing area of the Texture Cutter: zoom (pixel exact), tile grid, 3 × 3 tiled view, selections,
    /// paint tools and a movable overlay image. The image belongs to the owner; the canvas paints on it
    /// after raising <see cref="BeginEdit"/> (for undo).
    /// </summary>
    internal sealed class TextureCanvas : Control
    {
        private Bitmap _image;
        private int _zoom = 1;
        private bool _mouseDown;
        private MouseButtons _button;
        private Point _start;
        private Point _last;
        private Point _current;
        private Point _overlayDragStart;
        private Point _overlayStartPos;
        private bool _editing;

        public TextureCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.Selectable, true);
            Cursor = Cursors.Cross;
            Size = new Size(1, 1);
        }

        #region Properties

        public Bitmap Image
        {
            get => _image;
            set
            {
                _image = value;
                UpdateSize();
                Invalidate();
            }
        }

        public int Zoom
        {
            get => _zoom;
            set
            {
                _zoom = Math.Max(1, Math.Min(32, value));
                UpdateSize();
                Invalidate();
            }
        }

        public bool ShowGrid { get; set; }

        public Size GridSize { get; set; } = new Size(64, 64);

        public Point GridOffset { get; set; }

        public Color GridColor { get; set; } = Color.Red;

        /// <summary>Shows the image 3 × 3 times to check whether it repeats without seams. Painting is off then.</summary>
        public bool TiledView { get; set; }

        public CanvasTool Tool { get; set; } = CanvasTool.Select;

        public CanvasSelection SelectionShape { get; set; } = CanvasSelection.Rectangle;

        public Color PrimaryColor { get; set; } = Color.Black;

        public Color SecondaryColor { get; set; } = Color.White;

        public int BrushSize { get; set; } = 1;

        public bool MirrorDraw { get; set; }

        public bool EraseToWhite { get; set; }

        public int FillTolerance { get; set; }

        /// <summary>Selection in image pixels (rectangle / ellipse).</summary>
        public Rectangle Selection { get; private set; }

        /// <summary>Points of a freehand or polygon selection, in image pixels.</summary>
        public List<Point> SelectionPoints { get; } = new List<Point>();

        /// <summary>Layers above the image, bottom first. The owner keeps and disposes them.</summary>
        public List<CanvasLayer> Layers { get; } = new List<CanvasLayer>();

        /// <summary>The layer the move tool and the arrow keys work on.</summary>
        public CanvasLayer ActiveLayer { get; set; }

        /// <summary>Pattern for the pattern brush (owned by the owner).</summary>
        public Bitmap Pattern { get; set; }

        /// <summary>Pencil, line, rectangle and fill paint with the pattern instead of a color.</summary>
        public bool UsePattern { get; set; }

        public bool HasSelection =>
            SelectionShape == CanvasSelection.Freehand || SelectionShape == CanvasSelection.Polygon
                ? SelectionPoints.Count > 2
                : Selection.Width > 0 && Selection.Height > 0;

        #endregion

        #region Events

        /// <summary>Raised before the canvas changes the image (push undo here).</summary>
        public event EventHandler BeginEdit;

        /// <summary>Raised after a paint stroke / fill changed the image.</summary>
        public event EventHandler ImageEdited;

        /// <summary>Mouse position in image pixels, or (-1, -1) outside.</summary>
        public event EventHandler<Point> HoverPixel;

        /// <summary>Pipette: picked color; the bool is true for the right mouse button (secondary color).</summary>
        public event EventHandler<(Color Color, bool Secondary)> ColorPicked;

        public event EventHandler SelectionChanged;

        public event EventHandler OverlayMoved;

        /// <summary>The move tool picked another layer by clicking on it.</summary>
        public event EventHandler ActiveLayerChanged;

        /// <summary>Raised before the mask changes (push undo here).</summary>
        public event EventHandler BeginMaskEdit;

        /// <summary>Raised after the mask was changed by a mask tool.</summary>
        public event EventHandler MaskChanged;

        #region Mask (marked pixels)

        private Bitmap _maskOverlay;
        private bool _maskEditing;

        /// <summary>Marked pixels (width × height), owned by the owner. Null = no mask.</summary>
        public bool[] Mask { get; set; }

        public bool ShowMask { get; set; } = true;

        public Color MaskColor { get; set; } = Color.FromArgb(120, 255, 0, 0);

        /// <summary>Mask brush round (true) or square.</summary>
        public bool RoundBrush { get; set; } = true;

        /// <summary>Shape of the stamp tool (true = marked), centered on the mouse.</summary>
        public bool[,] StampShape { get; set; }

        /// <summary>Call after the owner changed the mask.</summary>
        public void InvalidateMask()
        {
            _maskOverlay?.Dispose();
            _maskOverlay = null;
            Invalidate();
        }

        private bool MaskReady => _image != null && Mask != null && Mask.Length == _image.Width * _image.Height;

        private void StartMaskEdit()
        {
            if (_maskEditing)
            {
                return;
            }
            _maskEditing = true;
            BeginMaskEdit?.Invoke(this, EventArgs.Empty);
        }

        private void EndMaskEdit()
        {
            if (!_maskEditing)
            {
                return;
            }
            _maskEditing = false;
            InvalidateMask();
            MaskChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SetMaskPixel(int x, int y, bool value)
        {
            if (x >= 0 && y >= 0 && x < _image.Width && y < _image.Height)
            {
                Mask[y * _image.Width + x] = value;
            }
        }

        private void MaskDot(Point c, bool value)
        {
            int size = Math.Max(1, BrushSize);
            int off = (size - 1) / 2;
            double r = size / 2.0;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (RoundBrush && size > 2)
                    {
                        double dx = x + 0.5 - r, dy = y + 0.5 - r;
                        if (dx * dx + dy * dy > r * r)
                        {
                            continue;
                        }
                    }
                    SetMaskPixel(c.X - off + x, c.Y - off + y, value);
                }
            }
        }

        private void MaskStroke(Point a, Point b, bool value)
        {
            int dx = Math.Abs(b.X - a.X), sx = a.X < b.X ? 1 : -1;
            int dy = -Math.Abs(b.Y - a.Y), sy = a.Y < b.Y ? 1 : -1;
            int err = dx + dy, x = a.X, y = a.Y;
            while (true)
            {
                MaskDot(new Point(x, y), value);
                if (MirrorDraw)
                {
                    MaskDot(new Point(_image.Width - 1 - x, y), value);
                }
                if (x == b.X && y == b.Y)
                {
                    break;
                }
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x += sx; }
                if (e2 <= dx) { err += dx; y += sy; }
            }
            _maskOverlay?.Dispose();
            _maskOverlay = null;
            Invalidate();
        }

        /// <summary>Marks (or unmarks) pixels of similar color: connected, or in the whole image.</summary>
        private void Wand(Point start, bool value, bool wholeImage)
        {
            int w = _image.Width, h = _image.Height;
            int[] px = CutterImaging.GetPixels(_image);
            int target = px[start.Y * w + start.X];
            int tol = FillTolerance;
            bool Match(int p) =>
                ((p >> 24) == 0) == ((target >> 24) == 0) &&
                Math.Abs(((p >> 16) & 0xFF) - ((target >> 16) & 0xFF)) <= tol &&
                Math.Abs(((p >> 8) & 0xFF) - ((target >> 8) & 0xFF)) <= tol &&
                Math.Abs((p & 0xFF) - (target & 0xFF)) <= tol;
            if (wholeImage)
            {
                for (int i = 0; i < px.Length; i++)
                {
                    if (Match(px[i]))
                    {
                        Mask[i] = value;
                    }
                }
                return;
            }
            var done = new bool[px.Length];
            var stack = new Stack<int>();
            stack.Push(start.Y * w + start.X);
            while (stack.Count > 0)
            {
                int i = stack.Pop();
                if (done[i] || !Match(px[i]))
                {
                    continue;
                }
                done[i] = true;
                Mask[i] = value;
                int x = i % w, y = i / w;
                if (x > 0) stack.Push(i - 1);
                if (x < w - 1) stack.Push(i + 1);
                if (y > 0) stack.Push(i - w);
                if (y < h - 1) stack.Push(i + w);
            }
        }

        private void Stamp(Point c, bool value)
        {
            bool[,] shape = StampShape;
            if (shape == null)
            {
                return;
            }
            int sw = shape.GetLength(0), sh = shape.GetLength(1);
            for (int y = 0; y < sh; y++)
            {
                for (int x = 0; x < sw; x++)
                {
                    if (shape[x, y])
                    {
                        SetMaskPixel(c.X - sw / 2 + x, c.Y - sh / 2 + y, value);
                    }
                }
            }
        }

        private void DrawMask(Graphics g)
        {
            if (!ShowMask || !MaskReady)
            {
                return;
            }
            if (_maskOverlay == null)
            {
                int w = _image.Width, h = _image.Height;
                var px = new int[w * h];
                int c = MaskColor.ToArgb();
                bool any = false;
                for (int i = 0; i < px.Length; i++)
                {
                    if (Mask[i])
                    {
                        px[i] = c;
                        any = true;
                    }
                }
                if (!any)
                {
                    return;
                }
                _maskOverlay = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                CutterImaging.SetPixels(_maskOverlay, px);
            }
            InterpolationMode oldMode = g.InterpolationMode;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(_maskOverlay, new Rectangle(0, 0, _image.Width * _zoom, _image.Height * _zoom));
            g.PixelOffsetMode = PixelOffsetMode.Default;
            g.InterpolationMode = oldMode;
        }

        private void DrawMaskToolPreview(Graphics g)
        {
            if (_image == null || _current.X < 0)
            {
                return;
            }
            using (var pen = new Pen(Color.Yellow) { DashStyle = DashStyle.Dot })
            {
                if (Tool == CanvasTool.Stamp && StampShape != null)
                {
                    int sw = StampShape.GetLength(0), sh = StampShape.GetLength(1);
                    g.DrawRectangle(pen, (_current.X - sw / 2) * _zoom, (_current.Y - sh / 2) * _zoom, sw * _zoom - 1, sh * _zoom - 1);
                }
                else if (Tool == CanvasTool.MaskPaint)
                {
                    int size = Math.Max(1, BrushSize), off = (size - 1) / 2;
                    var r = new Rectangle((_current.X - off) * _zoom, (_current.Y - off) * _zoom, size * _zoom - 1, size * _zoom - 1);
                    if (RoundBrush && size > 2)
                    {
                        g.DrawEllipse(pen, r);
                    }
                    else
                    {
                        g.DrawRectangle(pen, r);
                    }
                }
            }
        }

        #endregion

        #endregion

        private int Repeat => TiledView ? 3 : 1;

        private void UpdateSize()
        {
            Size size = _image == null ? new Size(1, 1) : new Size(_image.Width * _zoom * Repeat, _image.Height * _zoom * Repeat);
            if (Size != size)
            {
                Size = size;
            }
        }

        public void RefreshView()
        {
            UpdateSize();
            Invalidate();
        }

        public void ClearSelection()
        {
            Selection = Rectangle.Empty;
            SelectionPoints.Clear();
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SelectAll()
        {
            if (_image == null)
            {
                return;
            }
            SelectionShape = SelectionShape == CanvasSelection.Ellipse ? CanvasSelection.Ellipse : CanvasSelection.Rectangle;
            SelectionPoints.Clear();
            Selection = new Rectangle(Point.Empty, _image.Size);
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>The selection as a path in image pixels, or null.</summary>
        public GraphicsPath GetSelectionPath()
        {
            if (!HasSelection)
            {
                return null;
            }
            var path = new GraphicsPath();
            switch (SelectionShape)
            {
                case CanvasSelection.Ellipse:
                    path.AddEllipse(Selection);
                    break;
                case CanvasSelection.Freehand:
                case CanvasSelection.Polygon:
                    path.AddPolygon(SelectionPoints.ToArray());
                    break;
                default:
                    path.AddRectangle(Selection);
                    break;
            }
            return path;
        }

        /// <summary>Bounding box of the selection inside the image.</summary>
        public Rectangle GetSelectionBounds()
        {
            using (GraphicsPath path = GetSelectionPath())
            {
                if (path == null || _image == null)
                {
                    return Rectangle.Empty;
                }
                return Rectangle.Intersect(Rectangle.Ceiling(path.GetBounds()), new Rectangle(Point.Empty, _image.Size));
            }
        }

        #region Painting

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (_image == null)
            {
                return;
            }
            int w = _image.Width * _zoom, h = _image.Height * _zoom;

            for (int ty = 0; ty < Repeat; ty++)
            {
                for (int tx = 0; tx < Repeat; tx++)
                {
                    var area = new Rectangle(tx * w, ty * h, w, h);
                    if (!area.IntersectsWith(e.ClipRectangle))
                    {
                        continue;
                    }
                    CutterImaging.DrawChecker(g, area);
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(_image, area);
                    g.PixelOffsetMode = PixelOffsetMode.Default;
                }
            }

            if (TiledView)
            {
                using (var pen = new Pen(Color.FromArgb(120, Color.Yellow)) { DashStyle = DashStyle.Dot })
                {
                    g.DrawRectangle(pen, w, h, w - 1, h - 1);
                }
                return;
            }

            DrawLayers(g);
            DrawGrid(g);
            DrawSelection(g);
            DrawMask(g);
            DrawToolPreview(g);
            DrawMaskToolPreview(g);
        }

        private void DrawLayers(Graphics g)
        {
            foreach (CanvasLayer layer in Layers)
            {
                if (layer.Visible)
                {
                    layer.DrawTo(g, _zoom);
                }
            }
            if (ActiveLayer != null && ActiveLayer.Visible)
            {
                Rectangle b = ActiveLayer.Bounds;
                using (var pen = new Pen(Color.Magenta) { DashStyle = DashStyle.Dash })
                {
                    g.DrawRectangle(pen, b.X * _zoom, b.Y * _zoom, b.Width * _zoom - 1, b.Height * _zoom - 1);
                }
            }
        }

        private void DrawGrid(Graphics g)
        {
            if (!ShowGrid || GridSize.Width < 1 || GridSize.Height < 1)
            {
                return;
            }
            int w = _image.Width, h = _image.Height;
            using (var pen = new Pen(Color.FromArgb(200, GridColor)))
            {
                for (int x = GridOffset.X; x <= w; x += GridSize.Width)
                {
                    if (x >= 0)
                    {
                        g.DrawLine(pen, x * _zoom, 0, x * _zoom, h * _zoom);
                    }
                }
                for (int y = GridOffset.Y; y <= h; y += GridSize.Height)
                {
                    if (y >= 0)
                    {
                        g.DrawLine(pen, 0, y * _zoom, w * _zoom, y * _zoom);
                    }
                }
            }
        }

        private Point Scaled(Point p) => new Point(p.X * _zoom, p.Y * _zoom);

        private void DrawSelection(Graphics g)
        {
            using (var back = new Pen(Color.Black))
            using (var pen = new Pen(Color.Yellow) { DashStyle = DashStyle.Dash })
            {
                if (SelectionShape == CanvasSelection.Freehand || SelectionShape == CanvasSelection.Polygon)
                {
                    if (SelectionPoints.Count > 1)
                    {
                        Point[] pts = SelectionPoints.ConvertAll(Scaled).ToArray();
                        g.DrawLines(back, pts);
                        g.DrawLines(pen, pts);
                        if (pts.Length > 2 && !(_mouseDown && SelectionShape == CanvasSelection.Freehand))
                        {
                            g.DrawLine(pen, pts[pts.Length - 1], pts[0]);
                        }
                    }
                    if (SelectionShape == CanvasSelection.Polygon && Tool == CanvasTool.Select && SelectionPoints.Count > 0)
                    {
                        g.DrawLine(pen, Scaled(SelectionPoints[SelectionPoints.Count - 1]), Scaled(_current));
                    }
                    return;
                }
                if (Selection.Width <= 0 || Selection.Height <= 0)
                {
                    return;
                }
                var r = new Rectangle(Selection.X * _zoom, Selection.Y * _zoom, Selection.Width * _zoom - 1, Selection.Height * _zoom - 1);
                if (SelectionShape == CanvasSelection.Ellipse)
                {
                    g.DrawEllipse(back, r);
                    g.DrawEllipse(pen, r);
                }
                else
                {
                    g.DrawRectangle(back, r);
                    g.DrawRectangle(pen, r);
                }
            }
        }

        private void DrawToolPreview(Graphics g)
        {
            if (!_mouseDown || (Tool != CanvasTool.Line && Tool != CanvasTool.Rectangle))
            {
                return;
            }
            Color c = _button == MouseButtons.Right ? SecondaryColor : PrimaryColor;
            using (var pen = new Pen(Color.FromArgb(200, c), Math.Max(1, BrushSize * _zoom)))
            {
                int o = _zoom / 2;
                if (Tool == CanvasTool.Line)
                {
                    g.DrawLine(pen, _start.X * _zoom + o, _start.Y * _zoom + o, _current.X * _zoom + o, _current.Y * _zoom + o);
                }
                else
                {
                    Rectangle r = Normalize(_start, _current);
                    g.DrawRectangle(pen, r.X * _zoom + o, r.Y * _zoom + o, r.Width * _zoom, r.Height * _zoom);
                }
            }
        }

        private static Rectangle Normalize(Point a, Point b) =>
            Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));

        #endregion

        #region Mouse

        private Point ToImage(Point p, bool clamp)
        {
            int x = (int)Math.Floor(p.X / (double)_zoom);
            int y = (int)Math.Floor(p.Y / (double)_zoom);
            if (clamp && _image != null)
            {
                x = Math.Max(0, Math.Min(_image.Width - 1, x));
                y = Math.Max(0, Math.Min(_image.Height - 1, y));
            }
            return new Point(x, y);
        }

        private bool Inside(Point p) => _image != null && p.X >= 0 && p.Y >= 0 && p.X < _image.Width && p.Y < _image.Height;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (_image == null || TiledView || (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right))
            {
                return;
            }
            Point p = ToImage(e.Location, Tool != CanvasTool.Overlay);
            _button = e.Button;
            _start = _last = _current = p;

            switch (Tool)
            {
                case CanvasTool.Select:
                    if (e.Button != MouseButtons.Left)
                    {
                        if (SelectionShape == CanvasSelection.Polygon && SelectionPoints.Count > 2)
                        {
                            // Right click closes the polygon.
                            Invalidate();
                            SelectionChanged?.Invoke(this, EventArgs.Empty);
                        }
                        return;
                    }
                    if (SelectionShape == CanvasSelection.Polygon)
                    {
                        if (e.Clicks > 1)
                        {
                            SelectionChanged?.Invoke(this, EventArgs.Empty);
                            return;
                        }
                        SelectionPoints.Add(p);
                        Invalidate();
                        return;
                    }
                    _mouseDown = true;
                    SelectionPoints.Clear();
                    Selection = Rectangle.Empty;
                    if (SelectionShape == CanvasSelection.Freehand)
                    {
                        SelectionPoints.Add(p);
                    }
                    break;

                case CanvasTool.MaskPaint:
                    if (!MaskReady)
                    {
                        return;
                    }
                    _mouseDown = true;
                    StartMaskEdit();
                    MaskStroke(p, p, e.Button == MouseButtons.Left);
                    return;

                case CanvasTool.Wand:
                    if (MaskReady && Inside(p))
                    {
                        StartMaskEdit();
                        Wand(p, e.Button == MouseButtons.Left, (ModifierKeys & Keys.Shift) == Keys.Shift);
                        EndMaskEdit();
                    }
                    return;

                case CanvasTool.Stamp:
                    if (MaskReady)
                    {
                        _mouseDown = true;
                        StartMaskEdit();
                        Stamp(p, e.Button == MouseButtons.Left);
                        InvalidateMask();
                    }
                    return;

                case CanvasTool.Pipette:
                    if (Inside(p))
                    {
                        ColorPicked?.Invoke(this, (_image.GetPixel(p.X, p.Y), e.Button == MouseButtons.Right));
                    }
                    return;

                case CanvasTool.Fill:
                    if (Inside(p))
                    {
                        StartEdit();
                        FloodFill(p, e.Button == MouseButtons.Right ? SecondaryColor : PrimaryColor);
                        EndEdit();
                    }
                    return;

                case CanvasTool.Overlay:
                    for (int li = Layers.Count - 1; li >= 0; li--)
                    {
                        if (Layers[li].Visible && Layers[li].Bounds.Contains(p))
                        {
                            if (!ReferenceEquals(ActiveLayer, Layers[li]))
                            {
                                ActiveLayer = Layers[li];
                                ActiveLayerChanged?.Invoke(this, EventArgs.Empty);
                                Invalidate();
                            }
                            break;
                        }
                    }
                    if (ActiveLayer != null)
                    {
                        _mouseDown = true;
                        _overlayDragStart = p;
                        _overlayStartPos = ActiveLayer.Position;
                        Cursor = Cursors.SizeAll;
                    }
                    return;

                case CanvasTool.Pencil:
                case CanvasTool.Eraser:
                    _mouseDown = true;
                    StartEdit();
                    Stroke(p, p);
                    break;

                default: // line, rectangle: preview while dragging
                    _mouseDown = true;
                    break;
            }
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Point raw = ToImage(e.Location, false);
            if (_image != null && TiledView)
            {
                raw = new Point(((raw.X % _image.Width) + _image.Width) % _image.Width, ((raw.Y % _image.Height) + _image.Height) % _image.Height);
            }
            HoverPixel?.Invoke(this, Inside(raw) ? raw : new Point(-1, -1));

            if (_image == null || TiledView)
            {
                return;
            }
            Point p = ToImage(e.Location, Tool != CanvasTool.Overlay);
            Point old = _current;
            _current = p;

            if (Tool == CanvasTool.Select && SelectionShape == CanvasSelection.Polygon && SelectionPoints.Count > 0)
            {
                Invalidate();
                return;
            }
            if ((Tool == CanvasTool.MaskPaint || Tool == CanvasTool.Stamp) && old != p)
            {
                if (_mouseDown && Tool == CanvasTool.MaskPaint)
                {
                    MaskStroke(old, p, _button == MouseButtons.Left);
                }
                else if (_mouseDown && Tool == CanvasTool.Stamp && StampShape != null &&
                         (Math.Abs(p.X - _last.X) >= StampShape.GetLength(0) || Math.Abs(p.Y - _last.Y) >= StampShape.GetLength(1)))
                {
                    Stamp(p, _button == MouseButtons.Left);
                    _last = p;
                    InvalidateMask();
                }
                Invalidate();
                return;
            }
            if (!_mouseDown)
            {
                return;
            }

            switch (Tool)
            {
                case CanvasTool.Select:
                    if (SelectionShape == CanvasSelection.Freehand)
                    {
                        if (SelectionPoints.Count == 0 || SelectionPoints[SelectionPoints.Count - 1] != p)
                        {
                            SelectionPoints.Add(p);
                        }
                    }
                    else
                    {
                        Rectangle r = Rectangle.FromLTRB(Math.Min(_start.X, p.X), Math.Min(_start.Y, p.Y), Math.Max(_start.X, p.X) + 1, Math.Max(_start.Y, p.Y) + 1);
                        if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                        {
                            int side = Math.Max(r.Width, r.Height);
                            r = new Rectangle(p.X < _start.X ? _start.X + 1 - side : _start.X, p.Y < _start.Y ? _start.Y + 1 - side : _start.Y, side, side);
                        }
                        Selection = r;
                    }
                    Invalidate();
                    break;

                case CanvasTool.Pencil:
                case CanvasTool.Eraser:
                    Stroke(_last, p);
                    _last = p;
                    break;

                case CanvasTool.Overlay:
                    if (ActiveLayer == null)
                    {
                        break;
                    }
                    ActiveLayer.Position = new Point(_overlayStartPos.X + p.X - _overlayDragStart.X, _overlayStartPos.Y + p.Y - _overlayDragStart.Y);
                    Invalidate();
                    OverlayMoved?.Invoke(this, EventArgs.Empty);
                    break;

                default:
                    if (old != p)
                    {
                        Invalidate();
                    }
                    break;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!_mouseDown)
            {
                return;
            }
            _mouseDown = false;
            Cursor = Cursors.Cross;
            Color c = _button == MouseButtons.Right ? SecondaryColor : PrimaryColor;
            if (Tool == CanvasTool.MaskPaint || Tool == CanvasTool.Stamp)
            {
                EndMaskEdit();
                return;
            }

            switch (Tool)
            {
                case CanvasTool.Select:
                    if (SelectionShape == CanvasSelection.Freehand && SelectionPoints.Count < 3)
                    {
                        SelectionPoints.Clear();
                    }
                    Invalidate();
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    break;

                case CanvasTool.Pencil:
                case CanvasTool.Eraser:
                    EndEdit();
                    break;

                case CanvasTool.Line:
                    StartEdit();
                    using (var g = Graphics.FromImage(_image))
                    {
                        StampLine(g, _start, _current, c, false);
                    }
                    EndEdit();
                    break;

                case CanvasTool.Rectangle:
                    StartEdit();
                    Rectangle r = Normalize(_start, _current);
                    using (var g = Graphics.FromImage(_image))
                    {
                        StampLine(g, new Point(r.Left, r.Top), new Point(r.Right, r.Top), c, false);
                        StampLine(g, new Point(r.Right, r.Top), new Point(r.Right, r.Bottom), c, false);
                        StampLine(g, new Point(r.Right, r.Bottom), new Point(r.Left, r.Bottom), c, false);
                        StampLine(g, new Point(r.Left, r.Bottom), new Point(r.Left, r.Top), c, false);
                    }
                    EndEdit();
                    break;

                case CanvasTool.Overlay:
                    OverlayMoved?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            HoverPixel?.Invoke(this, new Point(-1, -1));
            if (Tool == CanvasTool.MaskPaint || Tool == CanvasTool.Stamp)
            {
                _current = new Point(-1, -1);
                Invalidate();
            }
        }

        #endregion

        #region Editing

        private void StartEdit()
        {
            if (_editing)
            {
                return;
            }
            _editing = true;
            BeginEdit?.Invoke(this, EventArgs.Empty);
        }

        private void EndEdit()
        {
            if (!_editing)
            {
                return;
            }
            _editing = false;
            Invalidate();
            ImageEdited?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Pencil / eraser from a to b (with the mirrored stroke when "mirror" is on).</summary>
        private void Stroke(Point a, Point b)
        {
            bool erase = Tool == CanvasTool.Eraser;
            Color c = erase ? (EraseToWhite ? Color.White : Color.Transparent) : (_button == MouseButtons.Right ? SecondaryColor : PrimaryColor);
            using (var g = Graphics.FromImage(_image))
            {
                StampLine(g, a, b, c, erase);
                if (MirrorDraw)
                {
                    int w = _image.Width - 1;
                    StampLine(g, new Point(w - a.X, a.Y), new Point(w - b.X, b.Y), c, erase);
                }
            }
            Invalidate();
        }

        /// <summary>Bresenham line, a square of BrushSize pixels at every point – pixel exact, no smoothing.</summary>
        private void StampLine(Graphics g, Point a, Point b, Color color, bool replace)
        {
            g.SmoothingMode = SmoothingMode.None;
            g.CompositingMode = replace || color.A == 255 ? CompositingMode.SourceCopy : CompositingMode.SourceOver;
            int size = Math.Max(1, BrushSize);
            int off = (size - 1) / 2;
            bool pattern = UsePattern && Pattern != null && !replace;
            if (pattern)
            {
                g.CompositingMode = CompositingMode.SourceOver;
            }
            using (Brush brush = pattern ? (Brush)new TextureBrush(Pattern, WrapMode.Tile) : new SolidBrush(color))
            {
                int dx = Math.Abs(b.X - a.X), sx = a.X < b.X ? 1 : -1;
                int dy = -Math.Abs(b.Y - a.Y), sy = a.Y < b.Y ? 1 : -1;
                int err = dx + dy;
                int x = a.X, y = a.Y;
                while (true)
                {
                    g.FillRectangle(brush, x - off, y - off, size, size);
                    if (x == b.X && y == b.Y)
                    {
                        break;
                    }
                    int e2 = 2 * err;
                    if (e2 >= dy)
                    {
                        err += dy;
                        x += sx;
                    }
                    if (e2 <= dx)
                    {
                        err += dx;
                        y += sy;
                    }
                }
            }
        }

        /// <summary>Fills the connected area of similar color (tolerance per channel).</summary>
        private void FloodFill(Point start, Color color)
        {
            int w = _image.Width, h = _image.Height;
            int[] px = CutterImaging.GetPixels(_image);
            int target = px[start.Y * w + start.X];
            int fill = color.ToArgb();
            int[] pat = null;
            int pw = 0, ph = 0;
            if (UsePattern && Pattern != null)
            {
                pat = CutterImaging.GetPixels(Pattern);
                pw = Pattern.Width;
                ph = Pattern.Height;
            }
            else if (target == fill)
            {
                return;
            }
            int tol = FillTolerance;
            bool Match(int p) =>
                Math.Abs(((p >> 24) & 0xFF) - ((target >> 24) & 0xFF)) <= tol &&
                Math.Abs(((p >> 16) & 0xFF) - ((target >> 16) & 0xFF)) <= tol &&
                Math.Abs(((p >> 8) & 0xFF) - ((target >> 8) & 0xFF)) <= tol &&
                Math.Abs((p & 0xFF) - (target & 0xFF)) <= tol;

            var done = new bool[px.Length];
            var stack = new Stack<int>();
            stack.Push(start.Y * w + start.X);
            while (stack.Count > 0)
            {
                int i = stack.Pop();
                if (done[i] || !Match(px[i]))
                {
                    continue;
                }
                done[i] = true;
                int x = i % w, y = i / w;
                px[i] = pat == null ? fill : pat[(y % ph) * pw + x % pw];
                if (x > 0) stack.Push(i - 1);
                if (x < w - 1) stack.Push(i + 1);
                if (y > 0) stack.Push(i - w);
                if (y < h - 1) stack.Push(i + w);
            }
            CutterImaging.SetPixels(_image, px);
        }

        #endregion

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    return true;
            }
            return base.IsInputKey(keyData);
        }
    }
}

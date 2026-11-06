using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit
{
    internal enum DrawTool
    {
        Pencil,
        Eraser,
        Line,
        Rectangle,
        FilledRectangle,
        Ellipse,
        FilledEllipse,
        Fill,
        Pipette,
        Move,
        Select,
        Overlay
    }

    internal enum CanvasBackground
    {
        Checker,
        SolidColor,
        Image
    }

    /// <summary>A reference animation that is shown transparent behind (or above) the edited frames.</summary>
    internal sealed class ReferenceSet : IDisposable
    {
        public string Name = "";
        public readonly List<Bitmap> Frames = new List<Bitmap>();
        public readonly List<Point?> Centers = new List<Point?>();

        public void Add(Bitmap bmp, Point? center)
        {
            Frames.Add(bmp);
            Centers.Add(center);
        }

        public void Dispose()
        {
            foreach (Bitmap b in Frames) b?.Dispose();
            Frames.Clear();
            Centers.Clear();
        }
    }

    /// <summary>A recorded brush stroke. Used to replay the same stroke on other frames.</summary>
    internal sealed class Stroke
    {
        internal DrawTool Tool;
        public readonly List<Point> Points = new List<Point>();
        public int Argb;
        public bool Erase;
        public int Size = 1;
        public bool Round = true;
        public int Tolerance;
        public bool Constrain;
    }

    /// <summary>
    /// Large editing surface for one frame. Shows background, reference animation, onion skin,
    /// the frame layers, grid, guide lines and the anchor, and implements the drawing tools.
    /// </summary>
    public sealed class FrameCanvas : Control
    {
        private AnimDocument document;
        private int frameIndex;
        private int zoom = 3;

        // --- Tool settings ---
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal DrawTool Tool { get; set; } = DrawTool.Pencil;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color PrimaryColor { get; set; } = Color.Black;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color SecondaryColor { get; set; } = Color.White;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BrushSize { get; set; } = 1;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RoundBrush { get; set; } = true;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int FillTolerance { get; set; }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal EditLayer Layer { get; set; } = EditLayer.Paint;

        /// <summary>Returns the frames an edit applies to (the current frame, plus checked frames in multi mode).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<IEnumerable<int>> AffectedFrames { get; set; }

        // --- View settings ---
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowGrid { get; set; } = true;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowAnchor { get; set; } = true;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowLayerBounds { get; set; } = true;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowGuides { get; set; } = true;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int GuideY1 { get; set; } = -1;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int GuideY2 { get; set; } = -1;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int GuideX { get; set; } = -1;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color GuideColor { get; set; } = Color.Red;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool OnionEnabled { get; set; }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int OnionPrev { get; set; } = 1;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int OnionNext { get; set; } = 1;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float OnionOpacity { get; set; } = 0.35f;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool OnionTint { get; set; } = true;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool OnionWrap { get; set; } = true;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal ReferenceSet Reference { get; set; }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ReferenceVisible { get; set; } = true;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float ReferenceOpacity { get; set; } = 0.4f;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point ReferenceOffset { get; set; }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ReferenceOnTop { get; set; }

        /// <summary>-1 = reference follows the edited frame number, otherwise a fixed reference frame.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ReferenceFixedFrame { get; set; } = -1;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal CanvasBackground BackgroundMode { get; set; } = CanvasBackground.Checker;
        public Color BackgroundSolid { get; set; } = Color.FromArgb(40, 40, 40);
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image BackgroundPicture { get; set; }

        // --- Events ---
        /// <summary>Frames whose content changed (thumbnails / preview need a refresh).</summary>
        public event Action<int[]> ContentChanged;

        /// <summary>Mouse position in canvas pixels (null = outside) and the visible color there.</summary>
        public event Action<Point?, Color> HoverChanged;

        public event Action<Color, MouseButtons> ColorPicked;
        public event Action<int> ZoomRequested;

        // --- Stroke state ---
        private Stroke stroke;
        private PixelBuffer work;
        private int[] workBase;
        private Bitmap workTarget;
        private Point workShift;
        private List<int> strokeFrames;

        // --- Move state ---
        private Point moveStart;
        private Dictionary<int, Point> moveStartOffsets;
        private Dictionary<int, Bitmap> moveStartPaint;
        private bool moving;
        private bool panning;

        // --- Selection ---
        private bool selecting;
        private Point selStart;
        private Rectangle selection;

        /// <summary>Marked area in canvas pixels (Rectangle.Empty = nothing marked).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle Selection
        {
            get => selection;
            set { selection = value; Invalidate(); SelectionChanged?.Invoke(); }
        }

        public event Action SelectionChanged;

        // --- Overlays ---
        private Overlay selectedOverlay;
        private bool draggingOverlay;
        private Point overlayDragStart;
        private Point overlayStartPos;

        /// <summary>Show a frame + size label around every overlay of the current frame.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowOverlayFrames { get; set; } = true;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal Overlay SelectedOverlay
        {
            get => selectedOverlay;
            set { selectedOverlay = value; Invalidate(); OverlaySelected?.Invoke(value); }
        }

        internal event Action<Overlay> OverlaySelected;

        /// <summary>The selected overlay was moved (live while dragging).</summary>
        internal event Action<Overlay> OverlayMoved;
        private Point panStart;

        public FrameCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            TabStop = true;
            BackColor = Color.FromArgb(60, 60, 60);
            Cursor = Cursors.Cross;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal AnimDocument Document
        {
            get => document;
            set { document = value; UpdateCanvasSize(); Invalidate(); }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int FrameIndex
        {
            get => frameIndex;
            set { frameIndex = Math.Max(0, Math.Min(AnimDocument.FrameCount - 1, value)); Invalidate(); }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Zoom
        {
            get => zoom;
            set { zoom = Math.Max(1, Math.Min(16, value)); UpdateCanvasSize(); Invalidate(); }
        }

        public bool IsBusy => stroke != null || moving || selecting || draggingOverlay;

        public void UpdateCanvasSize()
        {
            if (document == null) return;
            Size = new Size(document.CanvasSize.Width * zoom, document.CanvasSize.Height * zoom);
        }

        private IEnumerable<int> GetAffected()
        {
            IEnumerable<int> list = AffectedFrames?.Invoke() ?? new[] { frameIndex };
            var result = list.ToList();
            if (!result.Contains(frameIndex)) result.Insert(0, frameIndex);
            return result;
        }

        private Point ToCanvas(Point screen) => new Point(
            (int)Math.Floor(screen.X / (double)zoom), (int)Math.Floor(screen.Y / (double)zoom));

        #region Painting

        private int ReferenceIndexFor(int frame)
        {
            if (Reference == null || Reference.Frames.Count == 0) return -1;
            if (ReferenceFixedFrame >= 0) return Math.Min(ReferenceFixedFrame, Reference.Frames.Count - 1);
            return frame % Reference.Frames.Count;
        }

        private void DrawReference(Graphics g, int frame)
        {
            int ri = ReferenceIndexFor(frame);
            if (ri < 0 || !ReferenceVisible) return;
            Bitmap b = Reference.Frames[ri];
            if (b == null) return;
            Point o = document.OffsetFor(b.Size, Reference.Centers[ri]);
            o.Offset(ReferenceOffset);
            ImageUtil.DrawImage(g, b, new Rectangle(o, b.Size), ReferenceOpacity);
        }

        /// <summary>Renders background + reference + onion skin + frame at canvas size (1:1).</summary>
        public Bitmap RenderComposite(int frame, bool withBackground, bool withOnion)
        {
            Bitmap comp = document.NewLayer();
            using (Graphics g = Graphics.FromImage(comp))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                if (withBackground) DrawBackground(g, new Rectangle(Point.Empty, document.CanvasSize));

                if (!ReferenceOnTop) DrawReference(g, frame);

                if (withOnion && OnionEnabled)
                {
                    DrawOnion(g, frame, -1, OnionPrev, Color.Red);
                    DrawOnion(g, frame, +1, OnionNext, Color.DodgerBlue);
                }

                document.DrawFrame(g, frame, new Rectangle(Point.Empty, document.CanvasSize), false, 1f, null);

                if (ReferenceOnTop) DrawReference(g, frame);
            }
            return comp;
        }

        private void DrawOnion(Graphics g, int frame, int dir, int count, Color tint)
        {
            int n = AnimDocument.FrameCount;
            for (int k = count; k >= 1; k--)
            {
                int i = frame + dir * k;
                if (OnionWrap) i = ((i % n) + n) % n;
                else if (i < 0 || i >= n) continue;
                if (i == frame) continue;

                float a = OnionOpacity / k;
                document.DrawFrame(g, i, new Rectangle(Point.Empty, document.CanvasSize), false, a, OnionTint ? tint : (Color?)null);
            }
        }

        private void DrawBackground(Graphics g, Rectangle r)
        {
            switch (BackgroundMode)
            {
                case CanvasBackground.SolidColor:
                    using (var b = new SolidBrush(BackgroundSolid)) g.FillRectangle(b, r);
                    break;
                case CanvasBackground.Image:
                    if (BackgroundPicture != null)
                    {
                        using (var tb = new TextureBrush(BackgroundPicture, WrapMode.Tile))
                            g.FillRectangle(tb, r);
                    }
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (document == null) return;

            var dest = new Rectangle(0, 0, document.CanvasSize.Width * zoom, document.CanvasSize.Height * zoom);
            if (BackgroundMode == CanvasBackground.Checker)
                ImageUtil.DrawChecker(g, dest, Math.Max(8, zoom * 4));

            using (Bitmap comp = RenderComposite(frameIndex, BackgroundMode != CanvasBackground.Checker, true))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(comp, dest, 0, 0, comp.Width, comp.Height, GraphicsUnit.Pixel);
            }
            g.PixelOffsetMode = PixelOffsetMode.Default;

            // pixel grid
            if (ShowGrid && zoom >= 6)
            {
                using (var pen = new Pen(Color.FromArgb(40, 0, 0, 0)))
                {
                    int x0 = Math.Max(0, e.ClipRectangle.Left / zoom), x1 = Math.Min(document.CanvasSize.Width, e.ClipRectangle.Right / zoom + 1);
                    int y0 = Math.Max(0, e.ClipRectangle.Top / zoom), y1 = Math.Min(document.CanvasSize.Height, e.ClipRectangle.Bottom / zoom + 1);
                    for (int x = x0; x <= x1; x++) g.DrawLine(pen, x * zoom, y0 * zoom, x * zoom, y1 * zoom);
                    for (int y = y0; y <= y1; y++) g.DrawLine(pen, x0 * zoom, y * zoom, x1 * zoom, y * zoom);
                }
            }

            // bounds of the original layer while editing it
            AnimFrame f = document.Frames[frameIndex];
            if (ShowLayerBounds && Layer == EditLayer.Original && f.Original != null)
            {
                using (var pen = new Pen(Color.FromArgb(180, Color.DeepSkyBlue)) { DashStyle = DashStyle.Dash })
                    g.DrawRectangle(pen, f.Offset.X * zoom, f.Offset.Y * zoom, f.Original.Width * zoom - 1, f.Original.Height * zoom - 1);
            }

            // guide lines
            if (ShowGuides)
            {
                using (var pen = new Pen(Color.FromArgb(200, GuideColor)))
                {
                    if (GuideY1 >= 0) g.DrawLine(pen, 0, GuideY1 * zoom + zoom / 2, dest.Width, GuideY1 * zoom + zoom / 2);
                    if (GuideY2 >= 0)
                    {
                        pen.DashStyle = DashStyle.Dash;
                        g.DrawLine(pen, 0, GuideY2 * zoom + zoom / 2, dest.Width, GuideY2 * zoom + zoom / 2);
                        pen.DashStyle = DashStyle.Solid;
                    }
                    if (GuideX >= 0) g.DrawLine(pen, GuideX * zoom + zoom / 2, 0, GuideX * zoom + zoom / 2, dest.Height);
                }
            }

            // anchor (feet position of the animation)
            if (ShowAnchor)
            {
                int ax = document.Anchor.X * zoom + zoom / 2, ay = document.Anchor.Y * zoom + zoom / 2;
                using (var pen = new Pen(Color.FromArgb(220, Color.Lime)))
                {
                    g.DrawLine(pen, ax - 8, ay, ax + 8, ay);
                    g.DrawLine(pen, ax, ay - 8, ax, ay + 8);
                }
            }

            // overlay frames + size labels
            if (ShowOverlayFrames || Tool == DrawTool.Overlay)
            {
                foreach (Overlay o in document.Overlays)
                {
                    if (!o.AppliesTo(frameIndex)) continue;
                    Rectangle b = o.BoundsFor(frameIndex);
                    var r = new Rectangle(b.X * zoom, b.Y * zoom, b.Width * zoom - 1, b.Height * zoom - 1);
                    bool sel = ReferenceEquals(o, selectedOverlay);
                    using (var pen = new Pen(sel ? Color.Orange : Color.Cyan, sel ? 2 : 1) { DashStyle = sel ? DashStyle.Solid : DashStyle.Dash })
                        g.DrawRectangle(pen, r);
                    string label = $"{o.Name}  {b.Width}x{b.Height}  @{b.X},{b.Y}";
                    Size ts = TextRenderer.MeasureText(label, Font);
                    var lr = new Rectangle(r.X, Math.Max(0, r.Y - ts.Height - 1), ts.Width, ts.Height);
                    using (var bg = new SolidBrush(Color.FromArgb(170, 0, 0, 0))) g.FillRectangle(bg, lr);
                    TextRenderer.DrawText(g, label, Font, lr.Location, sel ? Color.Orange : Color.Cyan);
                }
            }

            // selection (marching ants: black + yellow dashes)
            if (!selection.IsEmpty)
            {
                var sr = new Rectangle(selection.X * zoom, selection.Y * zoom, selection.Width * zoom - 1, selection.Height * zoom - 1);
                using (var black = new Pen(Color.Black))
                using (var yellow = new Pen(Color.Yellow) { DashStyle = DashStyle.Dash })
                {
                    g.DrawRectangle(black, sr);
                    g.DrawRectangle(yellow, sr);
                }
                TextRenderer.DrawText(g, $"{selection.Width}x{selection.Height}", Font, new Point(sr.X + 2, Math.Max(0, sr.Y - 16)), Color.Yellow);
            }

            if (Focused)
                ControlPaint.DrawFocusRectangle(g, new Rectangle(0, 0, Width, Height));
        }

        #endregion

        #region Mouse

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateCursor();
        }

        public void UpdateCursor()
        {
            Cursor = Tool == DrawTool.Move || Tool == DrawTool.Overlay ? Cursors.SizeAll : Cursors.Cross;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (document == null) return;

            if (e.Button == MouseButtons.Middle)
            {
                panning = true;
                panStart = Cursor.Position;
                Cursor = Cursors.SizeAll;
                return;
            }
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) return;

            Point p = ToCanvas(e.Location);

            // Alt + click = pipette, whatever tool is active
            if (Tool == DrawTool.Pipette || ModifierKeys.HasFlag(Keys.Alt))
            {
                ColorPicked?.Invoke(SampleVisible(p), e.Button);
                return;
            }

            if (Tool == DrawTool.Overlay && e.Button == MouseButtons.Left)
            {
                Overlay hit = document.Overlays.LastOrDefault(o => o.AppliesTo(frameIndex) && o.BoundsFor(frameIndex).Contains(p));
                if (hit == null && selectedOverlay != null && selectedOverlay.AppliesTo(frameIndex) && selectedOverlay.BoundsFor(frameIndex).Contains(p))
                    hit = selectedOverlay;
                SelectedOverlay = hit;
                if (hit != null)
                {
                    document.BeginChange("Move overlay", Array.Empty<int>());
                    draggingOverlay = true;
                    overlayDragStart = p;
                    overlayStartPos = hit.Position;
                }
                return;
            }

            // Select tool, or Ctrl + drag with any tool (like the old editor)
            if (e.Button == MouseButtons.Left && (Tool == DrawTool.Select || ModifierKeys.HasFlag(Keys.Control)))
            {
                selecting = true;
                selStart = p;
                Selection = Rectangle.Empty;
                return;
            }

            if (Tool == DrawTool.Move)
            {
                BeginMove(p);
                return;
            }

            BeginStroke(p, e.Button);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (document == null) return;

            if (panning)
            {
                Point now = Cursor.Position;
                if (Parent is ScrollableControl sc)
                {
                    Point cur = sc.AutoScrollPosition;
                    sc.AutoScrollPosition = new Point(-cur.X - (now.X - panStart.X), -cur.Y - (now.Y - panStart.Y));
                }
                panStart = now;
                return;
            }

            Point p = ToCanvas(e.Location);
            bool inside = p.X >= 0 && p.Y >= 0 && p.X < document.CanvasSize.Width && p.Y < document.CanvasSize.Height;
            HoverChanged?.Invoke(inside ? p : (Point?)null, inside ? SampleVisible(p) : Color.Empty);

            if (draggingOverlay && selectedOverlay != null)
            {
                selectedOverlay.Position = new Point(overlayStartPos.X + p.X - overlayDragStart.X, overlayStartPos.Y + p.Y - overlayDragStart.Y);
                Invalidate();
                OverlayMoved?.Invoke(selectedOverlay);
                return;
            }
            if (selecting)
            {
                Point q = p;
                if (ModifierKeys.HasFlag(Keys.Shift))
                {
                    int m = Math.Max(Math.Abs(q.X - selStart.X), Math.Abs(q.Y - selStart.Y));
                    q = new Point(selStart.X + (q.X < selStart.X ? -m : m), selStart.Y + (q.Y < selStart.Y ? -m : m));
                }
                var r = Rectangle.FromLTRB(Math.Min(selStart.X, q.X), Math.Min(selStart.Y, q.Y), Math.Max(selStart.X, q.X) + 1, Math.Max(selStart.Y, q.Y) + 1);
                Selection = Rectangle.Intersect(r, new Rectangle(Point.Empty, document.CanvasSize));
                return;
            }
            if (moving) { UpdateMove(p); return; }
            if (stroke != null) ContinueStroke(p);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (panning)
            {
                panning = false;
                UpdateCursor();
                return;
            }
            if (draggingOverlay)
            {
                draggingOverlay = false;
                ContentChanged?.Invoke(Enumerable.Range(0, AnimDocument.FrameCount).ToArray());
                return;
            }
            if (selecting)
            {
                selecting = false;
                if (selection.Width < 2 || selection.Height < 2) Selection = Rectangle.Empty;
                return;
            }
            if (moving) { EndMove(); return; }
            if (stroke != null) EndStroke();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            HoverChanged?.Invoke(null, Color.Empty);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                ZoomRequested?.Invoke(e.Delta > 0 ? 1 : -1);
                return;
            }

            // Let the surrounding panel scroll (Shift = horizontal)
            if (Parent is ScrollableControl sc)
            {
                Point cur = sc.AutoScrollPosition;
                int step = e.Delta > 0 ? -60 : 60;
                sc.AutoScrollPosition = ModifierKeys.HasFlag(Keys.Shift)
                    ? new Point(-cur.X + step, -cur.Y)
                    : new Point(-cur.X, -cur.Y + step);
            }
            base.OnMouseWheel(e);
        }

        /// <summary>The visible color of the frame (paint over original) at a canvas position.</summary>
        public Color SampleVisible(Point p)
        {
            AnimFrame f = document.Frames[frameIndex];
            if (f.Paint != null && f.PaintVisible && p.X >= 0 && p.Y >= 0 && p.X < f.Paint.Width && p.Y < f.Paint.Height)
            {
                Color c = f.Paint.GetPixel(p.X, p.Y);
                if (c.A > 0) return c;
            }
            if (f.Original != null && f.OriginalVisible)
            {
                int lx = p.X - f.Offset.X, ly = p.Y - f.Offset.Y;
                if (lx >= 0 && ly >= 0 && lx < f.Original.Width && ly < f.Original.Height)
                    return f.Original.GetPixel(lx, ly);
            }
            return Color.Transparent;
        }

        #endregion

        #region Strokes

        private static string ToolName(DrawTool t) => t.ToString();

        /// <summary>Returns (and if needed creates) the bitmap of the target layer and the canvas->layer shift.</summary>
        private Bitmap TargetLayer(int frame, out Point shift)
        {
            AnimFrame f = document.Frames[frame];
            if (Layer == EditLayer.Original)
            {
                if (f.Original == null)
                {
                    f.Original = document.NewLayer();
                    f.Offset = Point.Empty;
                }
                shift = new Point(-f.Offset.X, -f.Offset.Y);
                return f.Original;
            }
            shift = Point.Empty;
            return f.Paint;
        }

        private void BeginStroke(Point p, MouseButtons button)
        {
            strokeFrames = GetAffected().ToList();
            document.BeginChange(ToolName(Tool), strokeFrames);

            Color c = button == MouseButtons.Right ? SecondaryColor : PrimaryColor;
            stroke = new Stroke
            {
                Tool = Tool,
                Argb = c.ToArgb(),
                Erase = Tool == DrawTool.Eraser,
                Size = Math.Max(1, BrushSize),
                Round = RoundBrush,
                Tolerance = FillTolerance,
                Constrain = ModifierKeys.HasFlag(Keys.Shift)
            };
            stroke.Points.Add(p);

            workTarget = TargetLayer(frameIndex, out workShift);
            work = new PixelBuffer(workTarget);
            workBase = (int[])work.Pixels.Clone();
            if (c.A < 255 && !stroke.Erase) work.StrokeMask = new bool[work.Pixels.Length];

            if (Tool == DrawTool.Fill)
            {
                RenderStroke(work, stroke, workShift);
                work.WriteTo(workTarget);
                EndStroke();
                return;
            }

            RenderStroke(work, stroke, workShift);
            work.WriteTo(workTarget);
            Invalidate();
        }

        private void ContinueStroke(Point p)
        {
            Point last = stroke.Points[stroke.Points.Count - 1];
            if (last == p) return;
            stroke.Constrain = ModifierKeys.HasFlag(Keys.Shift);

            switch (stroke.Tool)
            {
                case DrawTool.Pencil:
                case DrawTool.Eraser:
                    stroke.Points.Add(p);
                    work.Line(last.X + workShift.X, last.Y + workShift.Y, p.X + workShift.X, p.Y + workShift.Y,
                        stroke.Size, stroke.Argb, stroke.Erase, stroke.Round);
                    break;
                default:
                    // shapes: always redraw from the state at stroke start
                    if (stroke.Points.Count > 1) stroke.Points[1] = p;
                    else stroke.Points.Add(p);
                    Array.Copy(workBase, work.Pixels, workBase.Length);
                    if (work.StrokeMask != null) Array.Clear(work.StrokeMask, 0, work.StrokeMask.Length);
                    RenderStroke(work, stroke, workShift);
                    break;
            }
            work.WriteTo(workTarget);
            Invalidate();
        }

        private void EndStroke()
        {
            Stroke s = stroke;
            stroke = null;
            work = null;
            workBase = null;
            workTarget = null;

            // replay the same stroke on the other selected frames
            foreach (int i in strokeFrames)
            {
                if (i == frameIndex) continue;
                Bitmap target = TargetLayer(i, out Point shift);
                var pb = new PixelBuffer(target);
                if (((uint)s.Argb >> 24) < 255 && !s.Erase) pb.StrokeMask = new bool[pb.Pixels.Length];
                RenderStroke(pb, s, shift);
                pb.WriteTo(target);
            }

            int[] changed = strokeFrames.ToArray();
            strokeFrames = null;
            Invalidate();
            ContentChanged?.Invoke(changed);
        }

        private static Point Constrained(Point a, Point b, DrawTool tool)
        {
            int dx = b.X - a.X, dy = b.Y - a.Y;
            if (tool == DrawTool.Line)
            {
                // snap to 0°, 45°, 90°
                if (Math.Abs(dx) > 2 * Math.Abs(dy)) return new Point(b.X, a.Y);
                if (Math.Abs(dy) > 2 * Math.Abs(dx)) return new Point(a.X, b.Y);
                int d = Math.Max(Math.Abs(dx), Math.Abs(dy));
                return new Point(a.X + Math.Sign(dx) * d, a.Y + Math.Sign(dy) * d);
            }
            int m = Math.Max(Math.Abs(dx), Math.Abs(dy));
            return new Point(a.X + (dx < 0 ? -m : m), a.Y + (dy < 0 ? -m : m));
        }

        /// <summary>Draws a complete stroke into a pixel buffer. shift converts canvas to layer coordinates.</summary>
        internal static void RenderStroke(PixelBuffer pb, Stroke s, Point shift)
        {
            Point P(Point c) => new Point(c.X + shift.X, c.Y + shift.Y);

            switch (s.Tool)
            {
                case DrawTool.Pencil:
                case DrawTool.Eraser:
                {
                    Point a = P(s.Points[0]);
                    pb.Stamp(a.X, a.Y, s.Size, s.Argb, s.Erase, s.Round);
                    for (int i = 1; i < s.Points.Count; i++)
                    {
                        Point b = P(s.Points[i]);
                        pb.Line(a.X, a.Y, b.X, b.Y, s.Size, s.Argb, s.Erase, s.Round);
                        a = b;
                    }
                    break;
                }
                case DrawTool.Fill:
                {
                    Point a = P(s.Points[0]);
                    pb.FloodFill(a.X, a.Y, s.Argb, false, s.Tolerance);
                    break;
                }
                default:
                {
                    Point a = s.Points[0];
                    Point b = s.Points.Count > 1 ? s.Points[1] : a;
                    if (s.Constrain) b = Constrained(a, b, s.Tool);
                    a = P(a);
                    b = P(b);
                    Rectangle r = Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X) + 1, Math.Max(a.Y, b.Y) + 1);
                    switch (s.Tool)
                    {
                        case DrawTool.Line: pb.Line(a.X, a.Y, b.X, b.Y, s.Size, s.Argb, false, s.Round); break;
                        case DrawTool.Rectangle: pb.Rect(r, s.Size, s.Argb, false, false); break;
                        case DrawTool.FilledRectangle: pb.Rect(r, s.Size, s.Argb, false, true); break;
                        case DrawTool.Ellipse: pb.Ellipse(r, s.Size, s.Argb, false, false); break;
                        case DrawTool.FilledEllipse: pb.Ellipse(r, s.Size, s.Argb, false, true); break;
                    }
                    break;
                }
            }
        }

        #endregion

        #region Move tool

        private void BeginMove(Point p)
        {
            strokeFrames = GetAffected().ToList();
            document.BeginChange("Move " + Layer, strokeFrames);
            moving = true;
            moveStart = p;
            moveStartOffsets = new Dictionary<int, Point>();
            moveStartPaint = new Dictionary<int, Bitmap>();
            foreach (int i in strokeFrames)
            {
                AnimFrame f = document.Frames[i];
                moveStartOffsets[i] = f.Offset;
                if (Layer == EditLayer.Paint) moveStartPaint[i] = new Bitmap(f.Paint);
            }
        }

        private void UpdateMove(Point p)
        {
            int dx = p.X - moveStart.X, dy = p.Y - moveStart.Y;
            foreach (int i in strokeFrames)
            {
                AnimFrame f = document.Frames[i];
                if (Layer == EditLayer.Original)
                {
                    if (f.Original != null)
                        f.Offset = new Point(moveStartOffsets[i].X + dx, moveStartOffsets[i].Y + dy);
                }
                else
                {
                    using (Graphics g = Graphics.FromImage(f.Paint))
                    {
                        g.Clear(Color.Transparent);
                        g.DrawImage(moveStartPaint[i], dx, dy, moveStartPaint[i].Width, moveStartPaint[i].Height);
                    }
                }
            }
            Invalidate();
            ContentChanged?.Invoke(new[] { frameIndex });
        }

        private void EndMove()
        {
            moving = false;
            foreach (Bitmap b in moveStartPaint.Values) b.Dispose();
            moveStartPaint = null;
            int[] changed = strokeFrames.ToArray();
            strokeFrames = null;
            ContentChanged?.Invoke(changed);
        }

        #endregion

        protected override bool IsInputKey(Keys keyData)
        {
            Keys k = keyData & Keys.KeyCode;
            if (k == Keys.Left || k == Keys.Right || k == Keys.Up || k == Keys.Down) return true;
            return base.IsInputKey(keyData);
        }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
    }
}

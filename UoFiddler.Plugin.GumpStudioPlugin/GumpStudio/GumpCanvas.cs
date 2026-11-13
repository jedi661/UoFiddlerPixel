using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>
    /// Work area of the gump studio: shows the gump like the client and edits it (select, move, resize, rubber band, snap,
    /// arrow keys, drop from the gump browser). In test mode buttons, check boxes, radios, pages and text entries work.
    /// </summary>
    public class GumpCanvas : Control
    {
        public const int Margin0 = 16;
        private GumpDesign _design = new GumpDesign();
        private int _page = 1;
        private int _zoom = 1;
        private readonly List<GumpElement> _selection = new List<GumpElement>();
        private Point _dragStart;
        private Dictionary<GumpElement, Point> _dragOrigins;
        private Rectangle _resizeOrigin;
        private int _handle = -1;
        private bool _dragging, _rubber, _changed;
        private Rectangle _rubberRect;
        private TextBox _editor;

        public GumpCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.FromArgb(38, 38, 42);
            AllowDrop = true;
            UpdateSize();
        }

        public event EventHandler SelectionChanged;
        public event EventHandler BeforeChange;
        public event EventHandler DesignChanged;
        public event EventHandler<string> StatusText;
        public event EventHandler<string> PreviewReply;
        public event EventHandler PageChanged;

        /// <summary>Files dropped from Explorer (.cs scripts, .gump.json projects, pictures).</summary>
        public event EventHandler<string[]> FilesDropped;

        /// <summary>One line for the test log: what OnResponse would get.</summary>
        public event EventHandler<string> TestLogged;

        private readonly List<(bool Vertical, int Pos, int From, int To)> _guides = new List<(bool, int, int, int)>();
        private readonly List<(Point A, Point B, int Gap)> _gaps = new List<(Point, Point, int)>();
        private GumpDesign _reference;
        private Bitmap _referenceImage;
        private readonly Dictionary<int, Bitmap> _referenceCache = new Dictionary<int, Bitmap>();

        /// <summary>Smart guides: edges and centers snap to other elements while dragging.</summary>
        [DefaultValue(true)]
        public bool Guides { get; set; } = true;

        /// <summary>Gump shown half transparent over the work area to compare (e.g. the state before your changes).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GumpDesign ReferenceDesign
        {
            get => _reference;
            set
            {
                _reference = value;
                ClearReferenceCache();
                Invalidate();
            }
        }

        /// <summary>Picture shown half transparent over the work area (e.g. a screenshot of a gump to rebuild).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Bitmap ReferenceImage
        {
            get => _referenceImage;
            set
            {
                _referenceImage?.Dispose();
                _referenceImage = value;
                Invalidate();
            }
        }

        /// <summary>Opacity of the reference in percent.</summary>
        [DefaultValue(50)]
        public int ReferenceOpacity { get; set; } = 50;

        /// <summary>Reference drawn in red so differences are easy to see.</summary>
        [DefaultValue(true)]
        public bool ReferenceTint { get; set; } = true;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point ReferenceOffset { get; set; }

        public bool HasReference => _reference != null || _referenceImage != null;

        private void ClearReferenceCache()
        {
            foreach (Bitmap b in _referenceCache.Values)
            {
                b?.Dispose();
            }
            _referenceCache.Clear();
        }

        private Bitmap ReferencePicture(int page)
        {
            if (_reference == null)
            {
                return null;
            }
            if (!_referenceCache.TryGetValue(page, out Bitmap bmp))
            {
                bmp = new Bitmap(Math.Max(1, _reference.CanvasWidth), Math.Max(1, _reference.CanvasHeight), PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    GumpRenderer.DrawDesign(g, _reference, page, false, null);
                }
                _referenceCache[page] = bmp;
            }
            return bmp;
        }

        private void DrawReference(Graphics g)
        {
            Bitmap pic = ReferencePicture(_page) ?? _referenceImage;
            if (pic == null)
            {
                return;
            }
            float a = Math.Max(0, Math.Min(100, ReferenceOpacity)) / 100f;
            var m = ReferenceTint
                ? new ColorMatrix(new[] { new[] { 1f, 0, 0, 0, 0 }, new[] { 0, 0.35f, 0, 0, 0 }, new[] { 0, 0, 0.35f, 0, 0 }, new[] { 0, 0, 0, a, 0 }, new[] { 0.25f, 0, 0, 0, 1 } })
                : new ColorMatrix { Matrix33 = a };
            using (var attr = new ImageAttributes())
            {
                attr.SetColorMatrix(m);
                Point at = _reference != null ? Point.Empty : ReferenceOffset;
                g.DrawImage(pic, new Rectangle(at, pic.Size), 0, 0, pic.Width, pic.Height, GraphicsUnit.Pixel, attr);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GumpDesign Design
        {
            get => _design;
            set
            {
                _design = value ?? new GumpDesign();
                ClearReferenceCache();
                _selection.Clear();
                Preview = new PreviewState();
                UpdateSize();
                Invalidate();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Page
        {
            get => _page;
            set
            {
                _page = Math.Max(1, value);
                Invalidate();
                PageChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [DefaultValue(1)]
        public int Zoom
        {
            get => _zoom;
            set
            {
                _zoom = Math.Max(1, Math.Min(4, value));
                UpdateSize();
                Invalidate();
            }
        }

        [DefaultValue(true)]
        public bool ShowGrid { get; set; } = true;

        [DefaultValue(10)]
        public int GridSize { get; set; } = 10;

        [DefaultValue(true)]
        public bool Snap { get; set; } = true;

        /// <summary>Test mode: the gump works like in the client instead of being edited.</summary>
        [DefaultValue(false)]
        public bool TestMode
        {
            get => _testMode;
            set
            {
                _testMode = value;
                EndTextEdit();
                Preview = new PreviewState();
                Invalidate();
            }
        }

        private bool _testMode;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PreviewState Preview { get; private set; } = new PreviewState();

        [Browsable(false)]
        public IReadOnlyList<GumpElement> Selection => _selection;

        public void UpdateSize()
        {
            Size = new Size((_design.CanvasWidth + Margin0 * 2) * _zoom, (_design.CanvasHeight + Margin0 * 2) * _zoom);
        }

        public void Select(IEnumerable<GumpElement> elements)
        {
            _selection.Clear();
            _selection.AddRange(elements.Where(e => e != null).Distinct());
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private Point ToGump(Point screen) => new Point(screen.X / _zoom - Margin0, screen.Y / _zoom - Margin0);

        /// <summary>The element and the other members of its designer group (Alt + click = only the element).</summary>
        private List<GumpElement> GroupOf(GumpElement e)
        {
            if (string.IsNullOrEmpty(e.DesignGroup) || (ModifierKeys & Keys.Alt) != 0)
            {
                return new List<GumpElement> { e };
            }
            return _design.Elements.Where(x => x.DesignGroup == e.DesignGroup && (x.Page == 0 || x.Page == _page || x == e)).ToList();
        }

        private int SnapValue(int v) => Snap && GridSize > 1 && (ModifierKeys & Keys.Alt) == 0 ? (int)Math.Round(v / (double)GridSize) * GridSize : v;

        private IEnumerable<GumpElement> PageElements() => GumpRenderer.Visible(_design, _page, true);

        private GumpElement HitTest(Point g) => PageElements().Reverse().FirstOrDefault(e => GumpRenderer.Measure(e).Contains(g));

        // ------------------------------------------------------------------ painting

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            g.ScaleTransform(_zoom, _zoom);
            g.TranslateTransform(Margin0, Margin0);
            var area = new Rectangle(0, 0, _design.CanvasWidth, _design.CanvasHeight);
            using (var b = new SolidBrush(Color.FromArgb(28, 28, 30)))
            {
                g.FillRectangle(b, area);
            }
            if (ShowGrid && GridSize > 1 && !_testMode)
            {
                using (var pen = new Pen(Color.FromArgb(40, 255, 255, 255)))
                {
                    for (int x = 0; x <= area.Width; x += GridSize)
                    {
                        g.DrawLine(pen, x, 0, x, area.Height);
                    }
                    for (int y = 0; y <= area.Height; y += GridSize)
                    {
                        g.DrawLine(pen, 0, y, area.Width, y);
                    }
                }
            }
            g.DrawRectangle(Pens.DimGray, area.X - 1, area.Y - 1, area.Width + 1, area.Height + 1);
            GumpRenderer.DrawDesign(g, _design, _page, !_testMode, _testMode ? Preview : null);
            if (_testMode)
            {
                return;
            }
            DrawReference(g);
            DrawGuides(g);
            g.SmoothingMode = SmoothingMode.None;
            using (var sel = new Pen(Color.FromArgb(255, 255, 210, 0), 1f / _zoom) { DashStyle = DashStyle.Dash })
            using (var lockPen = new Pen(Color.FromArgb(255, 255, 90, 90), 1f / _zoom) { DashStyle = DashStyle.Dot })
            {
                foreach (GumpElement el in _selection)
                {
                    Rectangle r = GumpRenderer.Measure(el);
                    g.DrawRectangle(el.Locked ? lockPen : sel, r.X - 1, r.Y - 1, r.Width + 1, r.Height + 1);
                }
            }
            if (_selection.Count == 1 && _selection[0].IsSized && !_selection[0].Locked)
            {
                foreach (Rectangle h in Handles(GumpRenderer.Measure(_selection[0])))
                {
                    g.FillRectangle(Brushes.White, h);
                    g.DrawRectangle(Pens.Black, h);
                }
            }
            if (_rubber)
            {
                using (var b = new SolidBrush(Color.FromArgb(40, 0, 160, 255)))
                using (var pen = new Pen(Color.FromArgb(200, 0, 160, 255), 1f / _zoom))
                {
                    g.FillRectangle(b, _rubberRect);
                    g.DrawRectangle(pen, _rubberRect);
                }
            }
        }

        private void DrawGuides(Graphics g)
        {
            if (_guides.Count == 0 && _gaps.Count == 0)
            {
                return;
            }
            using (var pen = new Pen(Color.Magenta, 1f / _zoom))
            {
                foreach ((bool vertical, int pos, int from, int to) in _guides)
                {
                    if (vertical)
                    {
                        g.DrawLine(pen, pos, from - 6, pos, to + 6);
                    }
                    else
                    {
                        g.DrawLine(pen, from - 6, pos, to + 6, pos);
                    }
                }
            }
            using (var pen = new Pen(Color.FromArgb(255, 0, 200, 255), 1f / _zoom))
            using (var font = new Font("Segoe UI", 7.5f / _zoom * Math.Max(1, _zoom * 0.75f)))
            {
                foreach ((Point a, Point b, int gap) in _gaps)
                {
                    g.DrawLine(pen, a, b);
                    string t = gap.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    SizeF sz = g.MeasureString(t, font);
                    var at = new PointF((a.X + b.X) / 2f - sz.Width / 2, (a.Y + b.Y) / 2f - sz.Height / 2);
                    g.FillRectangle(Brushes.Black, at.X, at.Y, sz.Width, sz.Height);
                    g.DrawString(t, font, Brushes.Cyan, at);
                }
            }
        }

        /// <summary>
        /// Smart guides for a moved rectangle: returns the offset that makes an edge / center line up with another element
        /// (within 5 pixels) and remembers the lines to draw. int.MinValue = nothing to snap on that axis.
        /// </summary>
        private (int Dx, int Dy) GuideSnap(Rectangle moving, bool snapX, bool snapY)
        {
            _guides.Clear();
            _gaps.Clear();
            if (!Guides || (ModifierKeys & Keys.Alt) != 0)
            {
                return (int.MinValue, int.MinValue);
            }
            int threshold = Math.Max(2, 5 / Math.Max(1, _zoom) + 1);
            List<Rectangle> others = PageElements().Where(e => !_selection.Contains(e)).Select(GumpRenderer.Measure).ToList();
            others.Add(new Rectangle(0, 0, _design.CanvasWidth, _design.CanvasHeight));
            int bestDx = int.MinValue, bestDy = int.MinValue, distX = threshold + 1, distY = threshold + 1;
            int[] mx = { moving.Left, moving.Left + moving.Width / 2, moving.Right };
            int[] my = { moving.Top, moving.Top + moving.Height / 2, moving.Bottom };
            foreach (Rectangle o in others)
            {
                int[] ox = { o.Left, o.Left + o.Width / 2, o.Right };
                int[] oy = { o.Top, o.Top + o.Height / 2, o.Bottom };
                foreach (int a in mx)
                {
                    foreach (int b in ox)
                    {
                        if (snapX && Math.Abs(b - a) < distX)
                        {
                            distX = Math.Abs(b - a);
                            bestDx = b - a;
                        }
                    }
                }
                foreach (int a in my)
                {
                    foreach (int b in oy)
                    {
                        if (snapY && Math.Abs(b - a) < distY)
                        {
                            distY = Math.Abs(b - a);
                            bestDy = b - a;
                        }
                    }
                }
            }
            Rectangle snapped = moving;
            snapped.Offset(bestDx == int.MinValue ? 0 : bestDx, bestDy == int.MinValue ? 0 : bestDy);
            // lines to draw: every element that shares an edge / center with the snapped rectangle
            foreach (Rectangle o in others)
            {
                foreach (int x in new[] { snapped.Left, snapped.Left + snapped.Width / 2, snapped.Right })
                {
                    if (bestDx != int.MinValue && (x == o.Left || x == o.Left + o.Width / 2 || x == o.Right))
                    {
                        _guides.Add((true, x, Math.Min(o.Top, snapped.Top), Math.Max(o.Bottom, snapped.Bottom)));
                    }
                }
                foreach (int y in new[] { snapped.Top, snapped.Top + snapped.Height / 2, snapped.Bottom })
                {
                    if (bestDy != int.MinValue && (y == o.Top || y == o.Top + o.Height / 2 || y == o.Bottom))
                    {
                        _guides.Add((false, y, Math.Min(o.Left, snapped.Left), Math.Max(o.Right, snapped.Right)));
                    }
                }
            }
            Gaps(snapped, others.Take(others.Count - 1).ToList());
            return (bestDx, bestDy);
        }

        /// <summary>Distances to the nearest element left / right / above / below (shown while dragging).</summary>
        private void Gaps(Rectangle r, List<Rectangle> others)
        {
            Rectangle? left = others.Where(o => o.Right <= r.Left && o.Bottom > r.Top && o.Top < r.Bottom).OrderByDescending(o => o.Right).Cast<Rectangle?>().FirstOrDefault();
            Rectangle? right = others.Where(o => o.Left >= r.Right && o.Bottom > r.Top && o.Top < r.Bottom).OrderBy(o => o.Left).Cast<Rectangle?>().FirstOrDefault();
            Rectangle? up = others.Where(o => o.Bottom <= r.Top && o.Right > r.Left && o.Left < r.Right).OrderByDescending(o => o.Bottom).Cast<Rectangle?>().FirstOrDefault();
            Rectangle? down = others.Where(o => o.Top >= r.Bottom && o.Right > r.Left && o.Left < r.Right).OrderBy(o => o.Top).Cast<Rectangle?>().FirstOrDefault();
            int cy = r.Top + r.Height / 2, cx = r.Left + r.Width / 2;
            if (left is Rectangle l && r.Left - l.Right < 200) _gaps.Add((new Point(l.Right, cy), new Point(r.Left, cy), r.Left - l.Right));
            if (right is Rectangle rr && rr.Left - r.Right < 200) _gaps.Add((new Point(r.Right, cy), new Point(rr.Left, cy), rr.Left - r.Right));
            if (up is Rectangle u && r.Top - u.Bottom < 200) _gaps.Add((new Point(cx, u.Bottom), new Point(cx, r.Top), r.Top - u.Bottom));
            if (down is Rectangle dd && dd.Top - r.Bottom < 200) _gaps.Add((new Point(cx, r.Bottom), new Point(cx, dd.Top), dd.Top - r.Bottom));
        }

        /// <summary>A resized edge snaps to edges of other elements (guides), else to the grid.</summary>
        private int EdgeSnap(int value, bool vertical)
        {
            if (Guides && (ModifierKeys & Keys.Alt) == 0)
            {
                int threshold = Math.Max(2, 5 / Math.Max(1, _zoom) + 1);
                foreach (Rectangle o in PageElements().Where(e => !_selection.Contains(e)).Select(GumpRenderer.Measure))
                {
                    foreach (int edge in vertical ? new[] { o.Left, o.Right } : new[] { o.Top, o.Bottom })
                    {
                        if (Math.Abs(edge - value) < threshold)
                        {
                            _guides.Add(vertical ? (true, edge, o.Top, o.Bottom) : (false, edge, o.Left, o.Right));
                            return edge;
                        }
                    }
                }
            }
            return SnapValue(value);
        }

        /// <summary>8 resize handles: corners and edge middles (top-left first, clockwise).</summary>
        private Rectangle[] Handles(Rectangle r)
        {
            int s = Math.Max(3, 6 / _zoom);
            int cx = r.X + r.Width / 2, cy = r.Y + r.Height / 2;
            Point[] pts = { new Point(r.Left, r.Top), new Point(cx, r.Top), new Point(r.Right, r.Top), new Point(r.Right, cy), new Point(r.Right, r.Bottom), new Point(cx, r.Bottom), new Point(r.Left, r.Bottom), new Point(r.Left, cy) };
            return pts.Select(p => new Rectangle(p.X - s / 2, p.Y - s / 2, s, s)).ToArray();
        }

        // ------------------------------------------------------------------ mouse (editing)

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            EndTextEdit();
            Point g = ToGump(e.Location);
            if (_testMode)
            {
                TestMouseDown(g, e);
                return;
            }
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            _dragStart = g;
            _changed = false;
            if (_selection.Count == 1 && _selection[0].IsSized && !_selection[0].Locked)
            {
                Rectangle[] hs = Handles(GumpRenderer.Measure(_selection[0]));
                for (int i = 0; i < hs.Length; i++)
                {
                    if (Rectangle.Inflate(hs[i], 2, 2).Contains(g))
                    {
                        _handle = i;
                        _resizeOrigin = new Rectangle(_selection[0].X, _selection[0].Y, _selection[0].Width, _selection[0].Height);
                        return;
                    }
                }
            }
            GumpElement hit = HitTest(g);
            if (hit != null)
            {
                // a row of a list stands for its template row
                hit = GumpLists.TemplateOf(_design, hit);
            }
            bool add = (ModifierKeys & (Keys.Shift | Keys.Control)) != 0;
            List<GumpElement> members = hit == null ? null : GroupOf(hit);
            if (hit == null)
            {
                if (!add)
                {
                    Select(Array.Empty<GumpElement>());
                }
                _rubber = true;
                _rubberRect = new Rectangle(g, Size.Empty);
                return;
            }
            if (add)
            {
                if (_selection.Contains(hit))
                {
                    _selection.RemoveAll(members.Contains);
                }
                else
                {
                    _selection.AddRange(members.Where(m => !_selection.Contains(m)));
                }
                Invalidate();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (!_selection.Contains(hit))
            {
                Select(members);
            }
            _dragging = true;
            _dragOrigins = _selection.ToDictionary(x => x, x => new Point(x.X, x.Y));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Point g = ToGump(e.Location);
            if (_testMode)
            {
                GumpElement over = HitTest(g);
                if (Preview.Hover != over)
                {
                    Preview.Hover = over;
                    Cursor = over != null && (over.Type == ElementType.Button || over.Type == ElementType.ItemButton || over.Type == ElementType.Check || over.Type == ElementType.Radio)
                        ? Cursors.Hand : over?.Type == ElementType.TextEntry ? Cursors.IBeam : Cursors.Default;
                }
                StatusText?.Invoke(this, $"{g.X}, {g.Y}");
                return;
            }
            int dx = g.X - _dragStart.X, dy = g.Y - _dragStart.Y;
            if (_handle >= 0 && e.Button == MouseButtons.Left)
            {
                GumpElement el = _selection[0];
                if (!_changed)
                {
                    BeforeChange?.Invoke(this, EventArgs.Empty);
                    _changed = true;
                }
                _guides.Clear();
                _gaps.Clear();
                Rectangle o = _resizeOrigin;
                int left = o.Left, top = o.Top, right = o.Right, bottom = o.Bottom;
                if (_handle == 0 || _handle == 6 || _handle == 7) left = EdgeSnap(o.Left + dx, true);
                if (_handle == 2 || _handle == 3 || _handle == 4) right = EdgeSnap(o.Right + dx, true);
                if (_handle == 0 || _handle == 1 || _handle == 2) top = EdgeSnap(o.Top + dy, false);
                if (_handle == 4 || _handle == 5 || _handle == 6) bottom = EdgeSnap(o.Bottom + dy, false);
                el.X = Math.Min(left, right - 4);
                el.Y = Math.Min(top, bottom - 4);
                el.Width = Math.Max(4, right - el.X);
                el.Height = Math.Max(4, bottom - el.Y);
                Invalidate();
                StatusText?.Invoke(this, $"{el.Type}: {el.X}, {el.Y}  {el.Width} × {el.Height}");
                return;
            }
            if (_dragging && e.Button == MouseButtons.Left && (Math.Abs(dx) > 1 || Math.Abs(dy) > 1 || _changed))
            {
                if (!_changed)
                {
                    BeforeChange?.Invoke(this, EventArgs.Empty);
                    _changed = true;
                }
                // where the selection would be without snapping
                List<GumpElement> moving = _dragOrigins.Keys.Where(k => !k.Locked).ToList();
                Rectangle raw = Rectangle.Empty;
                foreach (GumpElement k in moving)
                {
                    Rectangle r = GumpRenderer.Measure(k);
                    r.Location = new Point(_dragOrigins[k].X + dx, _dragOrigins[k].Y + dy);
                    raw = raw.IsEmpty ? r : Rectangle.Union(raw, r);
                }
                (int gx, int gy) = moving.Count > 0 ? GuideSnap(raw, true, true) : (int.MinValue, int.MinValue);
                foreach (KeyValuePair<GumpElement, Point> kv in _dragOrigins)
                {
                    if (kv.Key.Locked)
                    {
                        continue;
                    }
                    kv.Key.X = gx != int.MinValue ? kv.Value.X + dx + gx : SnapValue(kv.Value.X + dx);
                    kv.Key.Y = gy != int.MinValue ? kv.Value.Y + dy + gy : SnapValue(kv.Value.Y + dy);
                }
                Invalidate();
                GumpElement first = _selection.FirstOrDefault();
                if (first != null)
                {
                    StatusText?.Invoke(this, $"{first.Type}: {first.X}, {first.Y}" + (_selection.Count > 1 ? $"  (+{_selection.Count - 1})" : string.Empty));
                }
                return;
            }
            if (_rubber)
            {
                _rubberRect = Rectangle.FromLTRB(Math.Min(_dragStart.X, g.X), Math.Min(_dragStart.Y, g.Y), Math.Max(_dragStart.X, g.X), Math.Max(_dragStart.Y, g.Y));
                Invalidate();
                return;
            }
            GumpElement h = HitTest(g);
            Cursor = h != null && !h.Locked ? Cursors.SizeAll : Cursors.Default;
            if (_selection.Count == 1 && _selection[0].IsSized && Handles(GumpRenderer.Measure(_selection[0])).Any(r => Rectangle.Inflate(r, 2, 2).Contains(g)))
            {
                Cursor = Cursors.Cross;
            }
            StatusText?.Invoke(this, $"{g.X}, {g.Y}" + (h != null ? $"  ·  {h}" : string.Empty));
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_testMode)
            {
                TestMouseUp(ToGump(e.Location));
                return;
            }
            if (_rubber)
            {
                _rubber = false;
                bool add = (ModifierKeys & (Keys.Shift | Keys.Control)) != 0;
                IEnumerable<GumpElement> inside = PageElements().Where(x => _rubberRect.IntersectsWith(GumpRenderer.Measure(x)));
                Select(add ? _selection.Concat(inside).ToList() : inside.ToList());
            }
            _guides.Clear();
            _gaps.Clear();
            if (_changed)
            {
                DesignChanged?.Invoke(this, EventArgs.Empty);
            }
            _dragging = false;
            _handle = -1;
            _changed = false;
            Invalidate();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (_testMode)
            {
                return;
            }
            GumpElement hit = HitTest(ToGump(e.Location));
            if (hit != null && (hit.Type == ElementType.Label || hit.Type == ElementType.LabelCropped || hit.Type == ElementType.TextEntry || hit.Type == ElementType.Html))
            {
                BeginTextEdit(hit, true);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                Zoom += e.Delta > 0 ? 1 : -1;
                if (e is HandledMouseEventArgs h)
                {
                    h.Handled = true;
                }
                return;
            }
            base.OnMouseWheel(e);
        }

        // ------------------------------------------------------------------ test mode

        private void TestMouseDown(Point g, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && _design.Closable)
            {
                PreviewReply?.Invoke(this, "Right click: the gump closes → OnResponse with ButtonID 0.");
                TestLogged?.Invoke(this, "right click → ButtonID 0 (closed)");
                return;
            }
            GumpElement hit = HitTest(g);
            if (hit == null)
            {
                return;
            }
            switch (hit.Type)
            {
                case ElementType.Button:
                case ElementType.ItemButton:
                    Preview.Pressed = hit;
                    Invalidate();
                    break;
                case ElementType.Check:
                    Preview.Switches[hit] = !Preview.IsOn(hit);
                    Invalidate();
                    break;
                case ElementType.Radio:
                    foreach (GumpElement other in PageElements().Where(x => x.Type == ElementType.Radio && x.Group == hit.Group))
                    {
                        Preview.Switches[other] = false;
                    }
                    Preview.Switches[hit] = true;
                    Invalidate();
                    break;
                case ElementType.TextEntry:
                    BeginTextEdit(hit, false);
                    break;
            }
        }

        private void TestMouseUp(Point g)
        {
            GumpElement pressed = Preview.Pressed;
            Preview.Pressed = null;
            Invalidate();
            if (pressed == null || HitTest(g) != pressed)
            {
                return;
            }
            if (pressed.ButtonType == ButtonKind.Page)
            {
                Page = Math.Max(1, pressed.TargetPage);
                PreviewReply?.Invoke(this, $"Page button → page {Page}.");
                TestLogged?.Invoke(this, $"page button → page {Page} (client only, no OnResponse)");
                return;
            }
            var switches = _design.Elements.Where(x => (x.Type == ElementType.Check || x.Type == ElementType.Radio) && Preview.IsOn(x)).Select(x => x.SwitchId);
            var texts = _design.Elements.Where(x => x.Type == ElementType.TextEntry).Select(x => $"{x.EntryId}=\"{Preview.TextOf(x)}\"");
            PreviewReply?.Invoke(this, $"OnResponse: ButtonID {pressed.ButtonId}" + (string.IsNullOrEmpty(pressed.Name) ? string.Empty : $" ({pressed.Name})") +
                                       $" · switches [{string.Join(", ", switches)}] · text [{string.Join(", ", texts)}] – the gump closes.");
            TestLogged?.Invoke(this, $"ButtonID {pressed.ButtonId}" + (string.IsNullOrEmpty(pressed.Name) ? string.Empty : $" ({pressed.Name})") +
                                     (pressed.Action != ButtonAction.None ? $" → {pressed.Action}" : string.Empty) +
                                     $" · switches [{string.Join(", ", switches)}] · text [{string.Join(", ", texts)}]");
        }

        // ------------------------------------------------------------------ text editing in place

        private void BeginTextEdit(GumpElement el, bool designer)
        {
            EndTextEdit();
            Rectangle r = GumpRenderer.Measure(el);
            _editor = new TextBox
            {
                Bounds = new Rectangle((r.X + Margin0) * _zoom, (r.Y + Margin0) * _zoom, Math.Max(80, r.Width * _zoom), Math.Max(20, r.Height * _zoom)),
                Text = designer ? (GumpRenderer.SecondLanguage && !string.IsNullOrEmpty(el.TextDe) ? el.TextDe : el.Text) : Preview.TextOf(el),
                Multiline = el.Type == ElementType.Html,
                Tag = (el, designer)
            };
            _editor.KeyDown += (s, a) =>
            {
                if (a.KeyCode == Keys.Enter && !_editor.Multiline)
                {
                    a.SuppressKeyPress = true;
                    EndTextEdit();
                }
                else if (a.KeyCode == Keys.Escape)
                {
                    a.SuppressKeyPress = true;
                    _editor.Tag = null;
                    EndTextEdit();
                }
            };
            _editor.LostFocus += (s, a) => EndTextEdit();
            Controls.Add(_editor);
            _editor.Focus();
            _editor.SelectAll();
        }

        private void EndTextEdit()
        {
            if (_editor == null)
            {
                return;
            }
            TextBox ed = _editor;
            _editor = null;
            if (ed.Tag is ValueTuple<GumpElement, bool> t)
            {
                if (t.Item2)
                {
                    bool second = GumpRenderer.SecondLanguage;
                    if ((second ? t.Item1.TextDe : t.Item1.Text) != ed.Text)
                    {
                        BeforeChange?.Invoke(this, EventArgs.Empty);
                        if (second)
                        {
                            t.Item1.TextDe = ed.Text;
                        }
                        else
                        {
                            t.Item1.Text = ed.Text;
                        }
                        DesignChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
                else
                {
                    Preview.Texts[t.Item1] = ed.Text;
                }
            }
            Controls.Remove(ed);
            ed.Dispose();
            Invalidate();
            Focus();
        }

        // ------------------------------------------------------------------ keyboard

        protected override bool IsInputKey(Keys keyData)
        {
            Keys k = keyData & Keys.KeyCode;
            return k == Keys.Left || k == Keys.Right || k == Keys.Up || k == Keys.Down || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (_testMode || _selection.Count == 0)
            {
                return;
            }
            int step = e.Shift ? (GridSize > 1 ? GridSize : 10) : 1;
            int dx = e.KeyCode == Keys.Left ? -step : e.KeyCode == Keys.Right ? step : 0;
            int dy = e.KeyCode == Keys.Up ? -step : e.KeyCode == Keys.Down ? step : 0;
            if (dx == 0 && dy == 0)
            {
                return;
            }
            BeforeChange?.Invoke(this, EventArgs.Empty);
            foreach (GumpElement el in _selection.Where(x => !x.Locked))
            {
                if (e.Control && el.IsSized)
                {
                    el.Width = Math.Max(4, el.Width + dx);
                    el.Height = Math.Max(4, el.Height + dy);
                }
                else
                {
                    el.X += dx;
                    el.Y += dy;
                }
            }
            e.Handled = true;
            Invalidate();
            DesignChanged?.Invoke(this, EventArgs.Empty);
        }

        // ------------------------------------------------------------------ drag & drop from the browser

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            e.Effect = (e.Data.GetDataPresent(typeof(GumpDrop)) || e.Data.GetDataPresent(DataFormats.FileDrop)) && !_testMode ? DragDropEffects.Copy : DragDropEffects.None;
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
            {
                FilesDropped?.Invoke(this, files);
                return;
            }
            if (!(e.Data.GetData(typeof(GumpDrop)) is GumpDrop drop))
            {
                return;
            }
            Point g = ToGump(PointToClient(new Point(e.X, e.Y)));
            GumpElement el = GumpDesign.Create(drop.Type, SnapValue(g.X), SnapValue(g.Y));
            if (drop.Type == ElementType.Item)
            {
                el.ItemId = drop.Id;
            }
            else
            {
                el.GumpId = drop.Id;
                if (drop.Type == ElementType.Button)
                {
                    el.GumpId2 = drop.Id + 1;
                    el.ButtonId = _design.Elements.Where(x => x.Type == ElementType.Button).Select(x => x.ButtonId).DefaultIfEmpty(0).Max() + 1;
                }
            }
            el.Page = 0;
            BeforeChange?.Invoke(this, EventArgs.Empty);
            _design.Elements.Add(el);
            Select(new[] { el });
            DesignChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Drag data of the gump / item browser.</summary>
    public sealed class GumpDrop
    {
        public ElementType Type;
        public int Id;
    }
}

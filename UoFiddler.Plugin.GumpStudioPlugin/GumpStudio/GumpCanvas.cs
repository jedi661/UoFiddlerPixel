using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GumpDesign Design
        {
            get => _design;
            set
            {
                _design = value ?? new GumpDesign();
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
            bool add = (ModifierKeys & (Keys.Shift | Keys.Control)) != 0;
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
                    _selection.Remove(hit);
                }
                else
                {
                    _selection.Add(hit);
                }
                Invalidate();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (!_selection.Contains(hit))
            {
                Select(new[] { hit });
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
                Rectangle o = _resizeOrigin;
                int left = o.Left, top = o.Top, right = o.Right, bottom = o.Bottom;
                if (_handle == 0 || _handle == 6 || _handle == 7) left = SnapValue(o.Left + dx);
                if (_handle == 2 || _handle == 3 || _handle == 4) right = SnapValue(o.Right + dx);
                if (_handle == 0 || _handle == 1 || _handle == 2) top = SnapValue(o.Top + dy);
                if (_handle == 4 || _handle == 5 || _handle == 6) bottom = SnapValue(o.Bottom + dy);
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
                foreach (KeyValuePair<GumpElement, Point> kv in _dragOrigins)
                {
                    if (kv.Key.Locked)
                    {
                        continue;
                    }
                    kv.Key.X = SnapValue(kv.Value.X + dx);
                    kv.Key.Y = SnapValue(kv.Value.Y + dy);
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
                return;
            }
            var switches = _design.Elements.Where(x => (x.Type == ElementType.Check || x.Type == ElementType.Radio) && Preview.IsOn(x)).Select(x => x.SwitchId);
            var texts = _design.Elements.Where(x => x.Type == ElementType.TextEntry).Select(x => $"{x.EntryId}=\"{Preview.TextOf(x)}\"");
            PreviewReply?.Invoke(this, $"OnResponse: ButtonID {pressed.ButtonId}" + (string.IsNullOrEmpty(pressed.Name) ? string.Empty : $" ({pressed.Name})") +
                                       $" · switches [{string.Join(", ", switches)}] · text [{string.Join(", ", texts)}] – the gump closes.");
        }

        // ------------------------------------------------------------------ text editing in place

        private void BeginTextEdit(GumpElement el, bool designer)
        {
            EndTextEdit();
            Rectangle r = GumpRenderer.Measure(el);
            _editor = new TextBox
            {
                Bounds = new Rectangle((r.X + Margin0) * _zoom, (r.Y + Margin0) * _zoom, Math.Max(80, r.Width * _zoom), Math.Max(20, r.Height * _zoom)),
                Text = designer ? el.Text : Preview.TextOf(el),
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
                    if (t.Item1.Text != ed.Text)
                    {
                        BeforeChange?.Invoke(this, EventArgs.Empty);
                        t.Item1.Text = ed.Text;
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
            e.Effect = e.Data.GetDataPresent(typeof(GumpDrop)) && !_testMode ? DragDropEffects.Copy : DragDropEffects.None;
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
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

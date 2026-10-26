/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace UoFiddler.Controls.MapViewer
{
    /// <summary>
    /// Shows a region rendered by <see cref="IsoRenderer"/>: pan with the left mouse button,
    /// zoom with the mouse wheel, click selects a cell.
    /// </summary>
    public class IsoViewControl : Control
    {
        public static readonly double[] ZoomLevels = { 0.125, 0.1875, 0.25, 0.375, 0.5, 0.75, 1, 1.5, 2, 3, 4 };

        private IsoRenderer _renderer;
        private double _zoom = 1;
        private PointF _offset; // canvas position of the upper left corner of the control
        private bool _dragging;
        private bool _dragMoved;
        private Point _dragStart;
        private Point _lastMouse;
        private readonly Timer _renderTimer;
        private readonly List<(int lod, int cx, int cy)> _missing = new List<(int, int, int)>();
        private RegionCell _hoverCell;

        public IsoViewControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.FromArgb(18, 18, 22);
            TabStop = true;

            _renderTimer = new Timer { Interval = 15 };
            _renderTimer.Tick += OnRenderTick;
        }

        #region Properties / events

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IsoRenderer Renderer
        {
            get => _renderer;
            set
            {
                _renderer = value;
                _missing.Clear();
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double Zoom => _zoom;

        [DefaultValue(false)]
        public bool ShowGrid { get; set; }

        [DefaultValue(true)]
        public bool ShowIssues { get; set; } = true;

        /// <summary>Selected map position (or null)</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point? SelectedCell { get; set; }

        /// <summary>Selected drawable (static or land) or -1</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedDrawable { get; set; } = -1;

        /// <summary>Issues shown as markers</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList<RegionIssue> Issues { get; set; }

        /// <summary>Highlighted issue (selected in the list)</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RegionIssue HighlightedIssue { get; set; }

        /// <summary>Cell under the mouse (or null)</summary>
        [Browsable(false)]
        public RegionCell HoverCell => _hoverCell;

        /// <summary>Number of chunks that still have to be rendered</summary>
        [Browsable(false)]
        public int PendingChunks => _missing.Count;

        public event EventHandler<IsoCellEventArgs> CellClicked;
        public event EventHandler HoverChanged;
        public event EventHandler ZoomChanged;
        public event EventHandler RenderProgress;

        #endregion

        #region View

        public void SetZoom(double zoom, Point? anchor = null)
        {
            zoom = Math.Max(ZoomLevels[0], Math.Min(ZoomLevels[ZoomLevels.Length - 1], zoom));
            Point a = anchor ?? new Point(ClientSize.Width / 2, ClientSize.Height / 2);
            PointF canvas = ScreenToCanvasF(a);
            _zoom = zoom;
            _offset = new PointF(canvas.X - (float)(a.X / _zoom), canvas.Y - (float)(a.Y / _zoom));
            ClampOffset();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        public void ZoomIn(Point? anchor = null)
        {
            double next = ZoomLevels.FirstOrDefault(z => z > _zoom + 0.0001);
            SetZoom(next == 0 ? _zoom : next, anchor);
        }

        public void ZoomOut(Point? anchor = null)
        {
            double next = ZoomLevels.LastOrDefault(z => z < _zoom - 0.0001);
            SetZoom(next == 0 ? _zoom : next, anchor);
        }

        /// <summary>Whole region visible</summary>
        public void ZoomToFit()
        {
            if (_renderer == null || ClientSize.Width <= 0 || ClientSize.Height <= 0)
            {
                return;
            }

            Size canvas = _renderer.Data.CanvasSize;
            double fit = Math.Min(ClientSize.Width / (double)canvas.Width, ClientSize.Height / (double)canvas.Height);
            double zoom = ZoomLevels.LastOrDefault(z => z <= fit);
            _zoom = zoom == 0 ? ZoomLevels[0] : zoom;
            CenterOnCanvas(new PointF(canvas.Width / 2f, canvas.Height / 2f));
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Centers the view on a map position</summary>
        public void CenterOnCell(int x, int y)
        {
            if (_renderer == null)
            {
                return;
            }

            RegionCell cell = _renderer.Data.GetCell(x, y);
            int z = cell?.LandZ ?? 0;
            CenterOnCanvas(new PointF(_renderer.Data.CanvasX(x, y), _renderer.Data.CanvasY(x, y, z) + RegionData.TileHalf));
        }

        private void CenterOnCanvas(PointF canvas)
        {
            _offset = new PointF(canvas.X - (float)(ClientSize.Width / 2.0 / _zoom), canvas.Y - (float)(ClientSize.Height / 2.0 / _zoom));
            ClampOffset();
            Invalidate();
        }

        private void ClampOffset()
        {
            if (_renderer == null)
            {
                return;
            }

            Size canvas = _renderer.Data.CanvasSize;
            float viewW = (float)(ClientSize.Width / _zoom);
            float viewH = (float)(ClientSize.Height / _zoom);

            // allow half a view over the border
            float minX = -viewW / 2, maxX = canvas.Width - (viewW / 2);
            float minY = -viewH / 2, maxY = canvas.Height - (viewH / 2);
            _offset = new PointF(Math.Max(minX, Math.Min(maxX, _offset.X)), Math.Max(minY, Math.Min(maxY, _offset.Y)));
        }

        public PointF ScreenToCanvasF(Point p)
        {
            return new PointF((float)(p.X / _zoom) + _offset.X, (float)(p.Y / _zoom) + _offset.Y);
        }

        public Point ScreenToCanvas(Point p)
        {
            PointF f = ScreenToCanvasF(p);
            return new Point((int)Math.Floor(f.X), (int)Math.Floor(f.Y));
        }

        public PointF CanvasToScreen(PointF p)
        {
            return new PointF((float)((p.X - _offset.X) * _zoom), (float)((p.Y - _offset.Y) * _zoom));
        }

        /// <summary>Drops the rendered chunks and redraws</summary>
        public void RefreshRender()
        {
            _renderer?.Invalidate();
            _missing.Clear();
            Invalidate();
        }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);

            if (_renderer == null)
            {
                return;
            }

            int lod = IsoRenderer.LodForZoom(_zoom);
            int span = RegionData.ChunkSize << lod;
            PointF tl = ScreenToCanvasF(Point.Empty);
            PointF br = ScreenToCanvasF(new Point(ClientSize.Width, ClientSize.Height));

            int c0 = Math.Max(0, (int)Math.Floor(tl.X / span));
            int r0 = Math.Max(0, (int)Math.Floor(tl.Y / span));
            int c1 = Math.Min(_renderer.Columns(lod) - 1, (int)Math.Floor(br.X / span));
            int r1 = Math.Min(_renderer.Rows(lod) - 1, (int)Math.Floor(br.Y / span));

            g.InterpolationMode = _zoom >= 1 ? InterpolationMode.NearestNeighbor : InterpolationMode.Bilinear;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            _missing.Clear();
            using (var placeholder = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.FromArgb(30, 30, 36), Color.FromArgb(24, 24, 30)))
            {
                for (int r = r0; r <= r1; r++)
                {
                    for (int c = c0; c <= c1; c++)
                    {
                        PointF p = CanvasToScreen(new PointF(c * span, r * span));
                        float size = (float)(span * _zoom);
                        var dest = new RectangleF(p.X, p.Y, size, size);

                        Bitmap bmp = _renderer.TryGetChunk(lod, c, r);
                        if (bmp != null)
                        {
                            g.DrawImage(bmp, dest, new RectangleF(0, 0, RegionData.ChunkSize, RegionData.ChunkSize), GraphicsUnit.Pixel);
                        }
                        else
                        {
                            g.FillRectangle(placeholder, dest);
                            _missing.Add((lod, c, r));
                        }
                    }
                }
            }

            g.PixelOffsetMode = PixelOffsetMode.Default;

            // render the chunks near the center first
            if (_missing.Count > 0)
            {
                PointF center = ScreenToCanvasF(new Point(ClientSize.Width / 2, ClientSize.Height / 2));
                _missing.Sort((a, b) => Distance(a, center, span).CompareTo(Distance(b, center, span)));
                _renderTimer.Start();
            }

            DrawOverlays(g);
        }

        private static float Distance((int lod, int cx, int cy) chunk, PointF center, int span)
        {
            float dx = ((chunk.cx + 0.5f) * span) - center.X;
            float dy = ((chunk.cy + 0.5f) * span) - center.Y;
            return (dx * dx) + (dy * dy);
        }

        private void OnRenderTick(object sender, EventArgs e)
        {
            if (_renderer == null || _missing.Count == 0)
            {
                _renderTimer.Stop();
                return;
            }

            Stopwatch watch = Stopwatch.StartNew();
            const long budget = 40;
            while (_missing.Count > 0 && watch.ElapsedMilliseconds < budget)
            {
                (int lod, int cx, int cy) chunk = _missing[0];
                if (_renderer.EnsureChunk(chunk.lod, chunk.cx, chunk.cy, watch, budget))
                {
                    _missing.RemoveAt(0);
                }
            }

            if (_missing.Count == 0)
            {
                _renderTimer.Stop();
            }

            RenderProgress?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        private void DrawOverlays(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RegionData data = _renderer.Data;

            if (ShowGrid && _zoom >= 0.375)
            {
                var view = new RectangleF(ScreenToCanvasF(Point.Empty), new SizeF((float)(ClientSize.Width / _zoom), (float)(ClientSize.Height / _zoom)));
                using (var pen = new Pen(Color.FromArgb(70, Color.White)))
                {
                    foreach (RegionCell cell in data.Cells)
                    {
                        int cx = data.CanvasX(cell.X, cell.Y);
                        int cy = data.CanvasY(cell.X, cell.Y, cell.LandZ);
                        if (cx + 30 < view.Left || cx - 30 > view.Right || cy + 600 < view.Top || cy - 600 > view.Bottom)
                        {
                            continue;
                        }

                        g.DrawPolygon(pen, ToScreen(data.GetLandCorners(cell, _renderer.IsStretched(cell))));
                    }
                }
            }

            if (ShowIssues && Issues != null)
            {
                foreach (RegionIssue issue in Issues)
                {
                    if (issue.Status != RegionIssueStatus.Open && issue != HighlightedIssue)
                    {
                        continue;
                    }

                    RegionCell cell = data.GetCell(issue.X, issue.Y);
                    if (cell == null)
                    {
                        continue;
                    }

                    Color color = issue.Status != RegionIssueStatus.Open ? Color.Gray : issue.Severity switch
                    {
                        RegionIssueSeverity.Error => Color.Red,
                        RegionIssueSeverity.Warning => Color.Orange,
                        _ => Color.DeepSkyBlue
                    };

                    PointF[] poly = ToScreen(data.GetLandCorners(cell, _renderer.IsStretched(cell)));
                    using (var fill = new SolidBrush(Color.FromArgb(issue == HighlightedIssue ? 110 : 60, color)))
                    using (var pen = new Pen(color, issue == HighlightedIssue ? 3 : 1.5f))
                    {
                        g.FillPolygon(fill, poly);
                        g.DrawPolygon(pen, poly);
                    }
                }
            }

            if (_hoverCell != null)
            {
                using (var pen = new Pen(Color.FromArgb(200, Color.White), 1))
                {
                    g.DrawPolygon(pen, ToScreen(data.GetLandCorners(_hoverCell, _renderer.IsStretched(_hoverCell))));
                }
            }

            if (SelectedDrawable >= 0 && SelectedDrawable < data.Drawables.Count)
            {
                Drawable d = data.Drawables[SelectedDrawable];
                PointF p = CanvasToScreen(d.Bounds.Location);
                using (var pen = new Pen(Color.Cyan, 1) { DashStyle = DashStyle.Dash })
                {
                    if (d.Kind == DrawableKind.Static)
                    {
                        g.DrawRectangle(pen, p.X, p.Y, (float)(d.Bounds.Width * _zoom), (float)(d.Bounds.Height * _zoom));
                    }
                }
            }

            if (SelectedCell.HasValue)
            {
                RegionCell cell = data.GetCell(SelectedCell.Value.X, SelectedCell.Value.Y);
                if (cell != null)
                {
                    using (var pen = new Pen(Color.Yellow, 2.5f))
                    {
                        g.DrawPolygon(pen, ToScreen(data.GetLandCorners(cell, _renderer.IsStretched(cell))));
                    }
                }
            }

            g.SmoothingMode = SmoothingMode.Default;
        }

        private PointF[] ToScreen(Point[] canvas)
        {
            var result = new PointF[canvas.Length];
            for (int i = 0; i < canvas.Length; i++)
            {
                result[i] = CanvasToScreen(canvas[i]);
            }

            return result;
        }

        #endregion

        #region Mouse / keyboard

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();

            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle)
            {
                _dragging = true;
                _dragMoved = false;
                _dragStart = e.Location;
                _lastMouse = e.Location;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_dragging)
            {
                if (Math.Abs(e.X - _dragStart.X) > 3 || Math.Abs(e.Y - _dragStart.Y) > 3)
                {
                    _dragMoved = true;
                    Cursor = Cursors.SizeAll;
                }

                if (_dragMoved)
                {
                    _offset = new PointF(_offset.X - (float)((e.X - _lastMouse.X) / _zoom), _offset.Y - (float)((e.Y - _lastMouse.Y) / _zoom));
                    ClampOffset();
                    _lastMouse = e.Location;
                    Invalidate();
                    return;
                }
            }

            UpdateHover(e.Location);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            bool click = _dragging && !_dragMoved;
            _dragging = false;
            Cursor = Cursors.Default;

            if (click && e.Button == MouseButtons.Left && _renderer != null)
            {
                Point canvas = ScreenToCanvas(e.Location);
                int hit = _renderer.HitTest(canvas);
                RegionCell cell = hit >= 0 ? _renderer.Data.GetCell(_renderer.Data.Drawables[hit].X, _renderer.Data.Drawables[hit].Y) : _renderer.LandAt(canvas);

                SelectedDrawable = hit;
                SelectedCell = cell == null ? (Point?)null : new Point(cell.X, cell.Y);
                Invalidate();
                CellClicked?.Invoke(this, new IsoCellEventArgs(cell, hit));
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoverCell != null)
            {
                _hoverCell = null;
                HoverChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta > 0)
            {
                ZoomIn(e.Location);
            }
            else
            {
                ZoomOut(e.Location);
            }
        }

        private void UpdateHover(Point location)
        {
            if (_renderer == null)
            {
                return;
            }

            Point canvas = ScreenToCanvas(location);
            int hit = _renderer.HitTest(canvas);
            RegionCell cell = hit >= 0
                ? _renderer.Data.GetCell(_renderer.Data.Drawables[hit].X, _renderer.Data.Drawables[hit].Y)
                : _renderer.LandAt(canvas);

            if (cell != _hoverCell)
            {
                _hoverCell = cell;
                HoverChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    return true;
            }

            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            float step = (float)(100 / _zoom);
            switch (e.KeyCode)
            {
                case Keys.Left: _offset.X -= step; break;
                case Keys.Right: _offset.X += step; break;
                case Keys.Up: _offset.Y -= step; break;
                case Keys.Down: _offset.Y += step; break;
                case Keys.Add:
                case Keys.Oemplus: ZoomIn(); return;
                case Keys.Subtract:
                case Keys.OemMinus: ZoomOut(); return;
                default: return;
            }

            ClampOffset();
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ClampOffset();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _renderTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion
    }

    public sealed class IsoCellEventArgs : EventArgs
    {
        public IsoCellEventArgs(RegionCell cell, int drawable)
        {
            Cell = cell;
            Drawable = drawable;
        }

        /// <summary>clicked cell or null</summary>
        public RegionCell Cell { get; }

        /// <summary>clicked drawable (static or land) or -1</summary>
        public int Drawable { get; }
    }
}

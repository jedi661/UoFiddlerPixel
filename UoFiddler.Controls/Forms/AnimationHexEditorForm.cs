// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UoFiddler.Controls.Forms
{
    public partial class AnimationHexEditorForm : Form
    {
        // ═══════════════════════════════════════════════════════════
        //  Layout Constants
        // ═══════════════════════════════════════════════════════════
        private const int BytesPerRow = 16;
        private const int OffsetColWidth = 90;
        private const int HexCellWidth = 26;
        private const int AsciiCellWidth = 10;
        private const int RowHeight = 18;
        private const int HeaderHeight = 24;
        private const int GutterWidth = 12;

        // ═══════════════════════════════════════════════════════════
        //  Colors
        // ═══════════════════════════════════════════════════════════
        private static readonly Color ColBg = Color.FromArgb(28, 28, 32);
        private static readonly Color ColAltRow = Color.FromArgb(33, 33, 40);
        private static readonly Color ColOffsetFg = Color.FromArgb(100, 180, 255);
        private static readonly Color ColHexNormal = Color.FromArgb(220, 220, 220);
        private static readonly Color ColHexZero = Color.FromArgb(80, 80, 90);
        private static readonly Color ColAsciiNormal = Color.FromArgb(160, 160, 160);
        private static readonly Color ColHeaderBg = Color.FromArgb(38, 38, 50);
        private static readonly Color ColHeaderFg = Color.FromArgb(140, 140, 160);
        private static readonly Color ColGridLine = Color.FromArgb(50, 50, 60);
        private static readonly Color ColSelection = Color.FromArgb(0, 100, 200);
        private static readonly Color ColSelText = Color.White;
        private static readonly Color ColHover = Color.FromArgb(55, 65, 90);
        private static readonly Color ColCursor = Color.FromArgb(255, 200, 50);
        private static readonly Color ColDiffA = Color.FromArgb(110, 200, 55, 55);
        private static readonly Color ColDiffB = Color.FromArgb(110, 55, 160, 55);
        private static readonly Color ColModified = Color.FromArgb(120, 255, 160, 30);

        // ═══════════════════════════════════════════════════════════
        //  Fonts
        // ═══════════════════════════════════════════════════════════
        private readonly Font _monoFont = new("Consolas", 9f);
        private readonly Font _headerFont = new("Consolas", 8f, FontStyle.Bold);
        private readonly Font _labelFont = new("Segoe UI", 8f);

        // ═══════════════════════════════════════════════════════════
        //  Primary Data
        // ═══════════════════════════════════════════════════════════
        private byte[] _data;
        private long _dataOffset;
        private string _sourceFile;
        private int _bodyId = -1;
        private int _actionId = -1;
        private int _directionId = -1;
        private int _frameId = -1;
        private int _sequenceId = -1;
        private bool _isUop = false;

        // ═══════════════════════════════════════════════════════════
        //  Regions
        // ═══════════════════════════════════════════════════════════
        private readonly List<HexRegion> _regions = new();
        private HexRegion _primaryRegion;
        private HexRegion _hoveredRegion;

        // ═══════════════════════════════════════════════════════════
        //  Selection & Cursor
        // ═══════════════════════════════════════════════════════════
        private long _selStart = -1;
        private long _selEnd = -1;
        private long _cursor = 0;
        private bool _selecting = false;
        private long _selAnchor = -1;
        private bool _inAscii = false;
        private bool _cursorVisible = true;

        // ═══════════════════════════════════════════════════════════
        //  Scroll
        // ═══════════════════════════════════════════════════════════
        private long _scrollOffset = 0;
        private int _visibleRows = 0;

        // ═══════════════════════════════════════════════════════════
        //  Search
        // ═══════════════════════════════════════════════════════════
        private byte[] _searchPattern;
        private long _lastSearchHit = -1;

        // ═══════════════════════════════════════════════════════════
        //  Tooltip
        // ═══════════════════════════════════════════════════════════
        private readonly System.Windows.Forms.Timer _tooltipTimer = new() { Interval = 600 };
        private Point _lastMousePos;
        private string _pendingTooltip;

        // ═══════════════════════════════════════════════════════════
        //  Bookmarks
        // ═══════════════════════════════════════════════════════════
        private readonly List<(long Offset, string Label)> _bookmarks = new();

        // ═══════════════════════════════════════════════════════════
        //  Undo / Write Mode
        // ═══════════════════════════════════════════════════════════
        private bool _writeMode = false;
        private readonly Stack<UndoEntry> _undoStack = new();
        private readonly Stack<UndoEntry> _redoStack = new();
        private readonly HashSet<long> _modifiedBytes = new();

        private sealed class UndoEntry
        {
            public long Offset;
            public byte OldValue;
            public byte NewValue;
            public string Description;
        }

        // ═══════════════════════════════════════════════════════════
        //  Diff Mode
        // ═══════════════════════════════════════════════════════════
        private bool _diffMode = false;
        private byte[] _diffData = null;
        private string _diffLabel = "";

        // ═══════════════════════════════════════════════════════════
        //  Nibble input (write mode)
        // ═══════════════════════════════════════════════════════════
        private bool _nibbleMode = false;
        private int _nibbleFirst = -1;

        // ══════════════════════════════════════════════════════════════════════
        //  Constructor
        // ══════════════════════════════════════════════════════════════════════
        public AnimationHexEditorForm()
        {
            InitializeComponent();
            _tooltipTimer.Tick += TooltipTimer_Tick;

            var blinkTimer = new System.Windows.Forms.Timer { Interval = 530 };
            blinkTimer.Tick += (s, e) =>
            {
                _cursorVisible = !_cursorVisible;
                hexPanel.Invalidate(GetByteRect(_cursor));
            };
            blinkTimer.Start();

            // ── NearestNeighbor für picPreview ────────────────────────────────
            // Verhindert verschwommene Darstellung bei Pixel-Art / Sprites.
            // PictureBox.SizeMode = Zoom + eigenes Paint = scharfe Pixel.
            picPreview.SizeMode = PictureBoxSizeMode.Normal; // wir zeichnen selbst
            picPreview.Paint += PicPreview_Paint;
        }

        // ── Eigenes Paint für picPreview mit NearestNeighbor ─────────────────

        private void PicPreview_Paint(object sender, PaintEventArgs e)
        {
            var pb = (PictureBox)sender;
            if (pb.Image == null)
            {
                e.Graphics.Clear(Color.FromArgb(38, 38, 48));
                return;
            }

            e.Graphics.Clear(Color.FromArgb(38, 38, 48));
            e.Graphics.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode =
                System.Drawing.Drawing2D.PixelOffsetMode.Half;

            // Bild proportional in die PictureBox einpassen (wie Zoom-Modus)
            int imgW = pb.Image.Width;
            int imgH = pb.Image.Height;
            int boxW = pb.ClientSize.Width;
            int boxH = pb.ClientSize.Height;

            float scale = Math.Min((float)boxW / imgW, (float)boxH / imgH);
            int drawW = Math.Max(1, (int)(imgW * scale));
            int drawH = Math.Max(1, (int)(imgH * scale));
            int drawX = (boxW - drawW) / 2;
            int drawY = (boxH - drawH) / 2;

            e.Graphics.DrawImage(pb.Image,
                new Rectangle(drawX, drawY, drawW, drawH),
                new Rectangle(0, 0, imgW, imgH),
                GraphicsUnit.Pixel);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Public API
        // ══════════════════════════════════════════════════════════════════════

        public void LoadMulAnimation(byte[] rawData, long fileOffset, string filePath,
            int bodyId, int actionId, int directionId, int frameId, List<HexRegion> regions)
        {
            _data = rawData != null ? (byte[])rawData.Clone() : null;
            _dataOffset = fileOffset;
            _sourceFile = filePath;
            _bodyId = bodyId;
            _actionId = actionId;
            _directionId = directionId;
            _frameId = frameId;
            _isUop = false;
            ResetState();
            ApplyRegions(regions);
            UpdateTitle();
            ScrollToPrimary();
            Refresh();
        }

        public void LoadUopAnimation(byte[] rawData, long fileOffset, string filePath,
            int bodyId, int actionId, int directionId, int sequenceId, List<HexRegion> regions)
        {
            _data = rawData != null ? (byte[])rawData.Clone() : null;
            _dataOffset = fileOffset;
            _sourceFile = filePath;
            _bodyId = bodyId;
            _actionId = actionId;
            _directionId = directionId;
            _sequenceId = sequenceId;
            _isUop = true;
            ResetState();
            ApplyRegions(regions);
            UpdateTitle();
            ScrollToPrimary();
            Refresh();
        }

        public void UpdateSelection(int directionId, int frameId, int sequenceId,
            List<HexRegion> regions, Bitmap framePreview)
        {
            _directionId = directionId;
            _frameId = frameId;
            _sequenceId = sequenceId;
            ApplyRegions(regions);
            if (framePreview != null) SetPreviewImage(framePreview, null);
            ScrollToPrimary();
            hexPanel.Invalidate();
            UpdateStatus();
        }

        public void SetPreviewImage(Bitmap bmp, HexRegion region)
        {
            picPreview.Image = bmp;
            picPreview.Invalidate(); // NearestNeighbor Paint neu auslösen
            lblPreviewInfo.Text =
                $"Body:{_bodyId}  Action:{_actionId}  Dir:{_directionId}" +
                (_isUop ? $"  Seq:{_sequenceId}" : $"  Frame:{_frameId}") +
                (region != null
                    ? $"\nOffset: 0x{(_dataOffset + region.Offset):X8}  ({region.Length} B)"
                    : "");
        }

        private void ResetState()
        {
            _cursor = 0;
            _selStart = -1;
            _selEnd = -1;
            _selAnchor = -1;
            _scrollOffset = 0;
            _undoStack.Clear();
            _redoStack.Clear();
            _modifiedBytes.Clear();
            _nibbleMode = false;
            _nibbleFirst = -1;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PAINT
        // ══════════════════════════════════════════════════════════════════════

        private void hexPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var clip = e.ClipRectangle;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(ColBg);

            if (_data == null || _data.Length == 0)
            {
                using var nr = new SolidBrush(ColHeaderFg);
                g.DrawString("No data loaded. Select an animation in the tree.",
                    _labelFont, nr, new PointF(20, HeaderHeight + 14));
                DrawHeader(g);
                return;
            }

            _visibleRows = Math.Max(1, (hexPanel.Height - HeaderHeight) / RowHeight);

            if (_diffMode && _diffData != null)
            { DrawDiffMode(g, clip); return; }

            long maxScr = Math.Max(0,
                ((_data.Length + BytesPerRow - 1) / BytesPerRow - _visibleRows) * (long)BytesPerRow);
            if (_scrollOffset > maxScr) _scrollOffset = maxScr;
            if (_scrollOffset < 0) _scrollOffset = 0;
            SyncScrollBar();

            int hexX = OffsetColWidth;
            int ascX = hexX + BytesPerRow * HexCellWidth + GutterWidth;

            for (int row = 0; row < _visibleRows + 1; row++)
            {
                long bi = _scrollOffset + row * BytesPerRow;
                if (bi >= _data.Length) break;
                int y = HeaderHeight + row * RowHeight;
                if (y + RowHeight < clip.Top || y > clip.Bottom) continue;
                DrawRow(g, row, bi, y, hexX, ascX, _data);
            }

            using (var gp = new Pen(ColGridLine))
            {
                g.DrawLine(gp, OffsetColWidth - 2, 0, OffsetColWidth - 2, hexPanel.Height);
                g.DrawLine(gp, ascX - GutterWidth / 2, HeaderHeight, ascX - GutterWidth / 2, hexPanel.Height);
            }

            DrawHeader(g);
        }

        // ── Diff side-by-side ─────────────────────────────────────────────────
        private void DrawDiffMode(Graphics g, Rectangle clip)
        {
            int halfW = hexPanel.Width / 2 - 4;
            int hx = OffsetColWidth;
            int ax = hx + BytesPerRow * HexCellWidth + GutterWidth;
            int pbx = halfW + 8;

            for (int row = 0; row < _visibleRows + 1; row++)
            {
                long bi = _scrollOffset + row * BytesPerRow;
                int y = HeaderHeight + row * RowHeight;
                if (y + RowHeight < clip.Top || y > clip.Bottom) continue;

                if (bi < _data.Length)
                    DrawRow(g, row, bi, y, hx, ax, _data, _diffData);

                g.TranslateTransform(pbx, 0);
                if (bi < _diffData.Length)
                    DrawRow(g, row, bi, y, hx, ax, _diffData, _data);
                g.TranslateTransform(-pbx, 0);
            }

            using (var dp = new Pen(Color.FromArgb(80, 80, 100), 2f))
                g.DrawLine(dp, halfW + 4, 0, halfW + 4, hexPanel.Height);

            using var lb = new SolidBrush(Color.FromArgb(140, 190, 255));
            string nA = _sourceFile != null ? Path.GetFileName(_sourceFile) : "Buffer A";
            string nB = _diffLabel != "" ? _diffLabel : "Buffer B";
            g.DrawString($"A: {nA}", _labelFont, lb, new PointF(hx, 5));
            g.DrawString($"B: {nB}", _labelFont, lb, new PointF(pbx + hx, 5));

            DrawHeader(g);
        }

        // ── Single row ────────────────────────────────────────────────────────
        private void DrawRow(Graphics g, int row, long byteIndex, int y,
            int hexAreaX, int asciiAreaX, byte[] data, byte[] diffOther = null)
        {
            if (row % 2 == 1)
            {
                using var ab = new SolidBrush(ColAltRow);
                g.FillRectangle(ab, 0, y,
                    asciiAreaX + BytesPerRow * AsciiCellWidth + 8, RowHeight);
            }

            using (var ob = new SolidBrush(ColOffsetFg))
                g.DrawString($"0x{(_dataOffset + byteIndex):X8}",
                    _monoFont, ob, new PointF(4, y + 1));

            int count = (int)Math.Min(BytesPerRow, data.Length - byteIndex);

            for (int col = 0; col < BytesPerRow; col++)
            {
                long bIdx = byteIndex + col;
                int hx = hexAreaX + col * HexCellWidth;
                int ax = asciiAreaX + col * AsciiCellWidth;

                // diff
                bool isDiff = diffOther != null && col < count
                              && bIdx < diffOther.Length && diffOther[bIdx] != data[bIdx];

                // region background
                HexRegion region = GetRegionAt(bIdx);
                if (region != null && col < count)
                {
                    using var rb = new SolidBrush(region.HighlightColor);
                    g.FillRectangle(rb, hx, y, HexCellWidth - 1, RowHeight);
                    g.FillRectangle(rb, ax, y, AsciiCellWidth, RowHeight);
                    if (bIdx == region.Offset)
                    {
                        using var bp = new Pen(region.HighlightColor.Blend(Color.White, 0.6f), 2f);
                        g.DrawLine(bp, hx, y, hx, y + RowHeight);
                        g.DrawLine(bp, ax, y, ax, y + RowHeight);
                    }
                }

                // hover
                if (region == _hoveredRegion && _hoveredRegion != null && col < count)
                {
                    using var hb = new SolidBrush(ColHover);
                    g.FillRectangle(hb, hx, y, HexCellWidth - 1, RowHeight);
                    g.FillRectangle(hb, ax, y, AsciiCellWidth, RowHeight);
                }

                // modified
                if (_modifiedBytes.Contains(bIdx) && col < count)
                {
                    using var mb = new SolidBrush(ColModified);
                    g.FillRectangle(mb, hx, y, HexCellWidth - 1, RowHeight);
                    g.FillRectangle(mb, ax, y, AsciiCellWidth, RowHeight);
                }

                // diff highlight
                if (isDiff)
                {
                    bool isMine = (data == _data);
                    using var db = new SolidBrush(isMine ? ColDiffA : ColDiffB);
                    g.FillRectangle(db, hx, y, HexCellWidth - 1, RowHeight);
                    g.FillRectangle(db, ax, y, AsciiCellWidth, RowHeight);
                }

                // selection
                bool sel = IsSelected(bIdx) && data == _data;
                if (sel && col < count)
                {
                    using var sb = new SolidBrush(ColSelection);
                    g.FillRectangle(sb, hx, y, HexCellWidth - 1, RowHeight);
                    g.FillRectangle(sb, ax, y, AsciiCellWidth, RowHeight);
                }

                if (col >= count) continue;
                byte b = data[bIdx];

                // hex text
                Color hc = sel ? ColSelText :
                           isDiff ? Color.White :
                           b == 0 ? ColHexZero : ColHexNormal;
                using (var hxb = new SolidBrush(hc))
                    g.DrawString(b.ToString("X2"), _monoFont, hxb, new PointF(hx + 2, y + 1));

                // cursor (primary buffer only)
                if (data == _data && bIdx == _cursor && _cursorVisible)
                {
                    using var cp = new Pen(ColCursor, 2f);
                    g.DrawRectangle(cp, hx + 1, y + 1, HexCellWidth - 3, RowHeight - 3);
                }

                // bookmark triangle
                if (_bookmarks.Any(bm => bm.Offset == bIdx))
                {
                    using var bmb = new SolidBrush(Color.FromArgb(220, 255, 100, 0));
                    g.FillPolygon(bmb, new[]
                    {
                        new Point(hx,   y),
                        new Point(hx+7, y),
                        new Point(hx,   y+9)
                    });
                }

                // ascii
                char c = (b >= 32 && b < 127) ? (char)b : '.';
                Color ac = sel ? ColSelText : isDiff ? Color.White : ColAsciiNormal;
                using (var acb = new SolidBrush(ac))
                    g.DrawString(c.ToString(), _monoFont, acb, new PointF(ax + 1, y + 1));
            }

            DrawRegionLabel(g, byteIndex, count, y, hexAreaX);
        }

        private void DrawRegionLabel(Graphics g, long byteIndex, int count,
            int y, int hexAreaX)
        {
            foreach (var region in _regions)
            {
                if (region.Label == null) continue;
                if (region.Offset < byteIndex || region.Offset >= byteIndex + BytesPerRow) continue;
                int col = (int)(region.Offset - byteIndex);
                using var lb = new SolidBrush(
                    Color.FromArgb(190, region.HighlightColor.Blend(Color.White, 0.7f)));
                g.DrawString(region.Label, _labelFont, lb,
                    new PointF(hexAreaX + col * HexCellWidth + 1, y - 1));
            }
        }

        private void DrawHeader(Graphics g)
        {
            using var hb = new SolidBrush(ColHeaderBg);
            g.FillRectangle(hb, 0, 0, hexPanel.Width, HeaderHeight);

            using var fg = new SolidBrush(ColHeaderFg);
            g.DrawString("Offset    ", _headerFont, fg, new PointF(4, 4));

            int hx = OffsetColWidth;
            int ax = hx + BytesPerRow * HexCellWidth + GutterWidth;

            for (int col = 0; col < BytesPerRow; col++)
            {
                int x = hx + col * HexCellWidth;
                using var gp = new Pen(ColGridLine);
                g.DrawLine(gp, x, 0, x, HeaderHeight);
                g.DrawString(col.ToString("X2"), _headerFont, fg, new PointF(x + 4, 4));
            }
            g.DrawString("ASCII", _headerFont, fg, new PointF(ax + 2, 4));

            if (_writeMode)
            {
                using var wb = new SolidBrush(Color.FromArgb(200, 255, 80, 30));
                g.DrawString("✎ WRITE", _headerFont, wb,
                    new PointF(hexPanel.Width - 75, 5));
            }
            if (_diffMode)
            {
                using var db = new SolidBrush(Color.FromArgb(200, 100, 220, 255));
                g.DrawString("⬛ DIFF", _headerFont, db,
                    new PointF(hexPanel.Width - (_writeMode ? 145 : 75), 5));
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Coordinate Helpers
        // ══════════════════════════════════════════════════════════════════════

        private Rectangle GetByteRect(long byteIndex)
        {
            if (_data == null || byteIndex < _scrollOffset) return Rectangle.Empty;
            long rel = byteIndex - _scrollOffset;
            int row = (int)(rel / BytesPerRow);
            int col = (int)(rel % BytesPerRow);
            if (row >= _visibleRows) return Rectangle.Empty;
            return new Rectangle(
                OffsetColWidth + col * HexCellWidth,
                HeaderHeight + row * RowHeight,
                HexCellWidth, RowHeight);
        }

        private long HitTest(Point pt, out bool inAscii)
        {
            inAscii = false;
            if (_data == null || pt.Y < HeaderHeight) return -1;
            int row = (pt.Y - HeaderHeight) / RowHeight;
            if (row < 0 || row >= _visibleRows) return -1;
            long base_ = _scrollOffset + row * BytesPerRow;
            int hx = OffsetColWidth;
            int ax = hx + BytesPerRow * HexCellWidth + GutterWidth;

            if (pt.X >= ax)
            {
                inAscii = true;
                int c = (pt.X - ax) / AsciiCellWidth;
                if (c < 0 || c >= BytesPerRow) return -1;
                long i = base_ + c;
                return i < _data.Length ? i : -1;
            }
            if (pt.X >= hx)
            {
                int c = (pt.X - hx) / HexCellWidth;
                if (c < 0 || c >= BytesPerRow) return -1;
                long i = base_ + c;
                return i < _data.Length ? i : -1;
            }
            return -1;
        }

        private HexRegion GetRegionAt(long byteIndex)
        {
            foreach (var r in _regions)
                if (byteIndex >= r.Offset && byteIndex < r.Offset + r.Length) return r;
            return null;
        }

        private bool IsSelected(long byteIndex)
        {
            if (_selStart < 0 || _selEnd < 0) return false;
            long lo = Math.Min(_selStart, _selEnd);
            long hi = Math.Max(_selStart, _selEnd);
            return byteIndex >= lo && byteIndex <= hi;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Mouse
        // ══════════════════════════════════════════════════════════════════════

        private void hexPanel_MouseDown(object sender, MouseEventArgs e)
        {
            hexPanel.Focus();
            long idx = HitTest(e.Location, out bool ia);
            if (idx < 0) return;
            _inAscii = ia;
            _cursor = idx;

            if (e.Button == MouseButtons.Left)
            {
                if ((ModifierKeys & Keys.Shift) != 0 && _selAnchor >= 0)
                { _selStart = _selAnchor; _selEnd = idx; }
                else
                { _selAnchor = idx; _selStart = idx; _selEnd = idx; _selecting = true; }
            }
            else if (e.Button == MouseButtons.Right)
                ShowContextMenu(e.Location, idx);

            hexPanel.Invalidate();
            UpdateStatus();
        }

        private void hexPanel_MouseMove(object sender, MouseEventArgs e)
        {
            long idx = HitTest(e.Location, out _);
            HexRegion hov = idx >= 0 ? GetRegionAt(idx) : null;

            if (hov != _hoveredRegion) { _hoveredRegion = hov; hexPanel.Invalidate(); }

            if (_selecting && e.Button == MouseButtons.Left && idx >= 0)
            { _selEnd = idx; _cursor = idx; hexPanel.Invalidate(); UpdateStatus(); }

            _lastMousePos = e.Location;
            _pendingTooltip = hov?.Tooltip;
            _tooltipTimer.Stop();
            toolTip1.Hide(hexPanel);
            if (_pendingTooltip != null) _tooltipTimer.Start();
        }

        private void hexPanel_MouseUp(object sender, MouseEventArgs e)
            => _selecting = false;

        private void hexPanel_MouseClick(object sender, MouseEventArgs e) { }

        private void hexPanel_DoubleClick(object sender, EventArgs e)
        {
            long idx = HitTest(_lastMousePos, out _);
            HexRegion r = idx >= 0 ? GetRegionAt(idx) : null;
            if (r == null) return;
            _selStart = r.Offset; _selEnd = r.Offset + r.Length - 1;
            _cursor = r.Offset; _selAnchor = r.Offset;
            ScrollToOffset(r.Offset);
            SelectRegionInList(r);
            hexPanel.Invalidate();
            UpdateStatus();
        }

        private void hexPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            _scrollOffset += (e.Delta > 0 ? -3 : 3) * BytesPerRow;
            ClampScroll(); SyncScrollBar(); hexPanel.Invalidate();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Keyboard
        // ══════════════════════════════════════════════════════════════════════

        private void hexPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (_data == null) return;
            bool shift = (e.Modifiers & Keys.Shift) != 0;
            bool ctrl = (e.Modifiers & Keys.Control) != 0;

            // write mode nibble input
            if (_writeMode && !ctrl && !shift)
            {
                int nib = KeyToNibble(e.KeyCode);
                if (nib >= 0)
                {
                    if (!_nibbleMode) { _nibbleFirst = nib; _nibbleMode = true; }
                    else
                    {
                        WriteByte(_cursor, (byte)((_nibbleFirst << 4) | nib));
                        _nibbleMode = false;
                        _cursor = Math.Min(_cursor + 1, _data.Length - 1);
                        EnsureCursorVisible();
                        hexPanel.Invalidate(); UpdateStatus();
                    }
                    e.Handled = true; return;
                }
                _nibbleMode = false;
            }

            long old = _cursor;

            switch (e.KeyCode)
            {
                case Keys.Right: _cursor = Math.Min(_cursor + 1, _data.Length - 1); break;
                case Keys.Left: _cursor = Math.Max(_cursor - 1, 0); break;
                case Keys.Down: _cursor = Math.Min(_cursor + BytesPerRow, _data.Length - 1); break;
                case Keys.Up: _cursor = Math.Max(_cursor - BytesPerRow, 0); break;
                case Keys.Home: _cursor = ctrl ? 0 : (_cursor / BytesPerRow) * BytesPerRow; break;
                case Keys.End:
                    _cursor = ctrl ? _data.Length - 1 :
                                      Math.Min((_cursor / BytesPerRow + 1) * BytesPerRow - 1, _data.Length - 1); break;
                case Keys.PageDown: _cursor = Math.Min(_cursor + _visibleRows * BytesPerRow, _data.Length - 1); break;
                case Keys.PageUp: _cursor = Math.Max(_cursor - _visibleRows * BytesPerRow, 0); break;

                case Keys.C when ctrl: CopySelectedHex(); e.Handled = true; return;
                case Keys.A when ctrl: SelectAll(); e.Handled = true; return;
                case Keys.F when ctrl: OpenSearchDialog(); e.Handled = true; return;
                case Keys.G when ctrl: GotoDialog(); e.Handled = true; return;
                case Keys.I when ctrl: ShowByteInspector(); e.Handled = true; return;
                case Keys.B when ctrl && !shift: AddBookmark(); e.Handled = true; return;
                case Keys.B when ctrl && shift: ShowBookmarks(); e.Handled = true; return;
                case Keys.T when ctrl: ShowStatistics(); e.Handled = true; return;
                case Keys.E when ctrl: ExportSelection(); e.Handled = true; return;
                case Keys.Z when ctrl && !shift: Undo(); e.Handled = true; return;
                case Keys.Z when ctrl && shift: Redo(); e.Handled = true; return;
                case Keys.Y when ctrl: Redo(); e.Handled = true; return;
                case Keys.W when ctrl: ToggleWriteMode(); e.Handled = true; return;
                case Keys.D when ctrl: OpenDiffDialog(); e.Handled = true; return;
                case Keys.P when ctrl: ShowPalettePreview(); e.Handled = true; return;
                case Keys.S when ctrl: ShowStructureTree(); e.Handled = true; return;
                case Keys.F1: ShowCommandOverview(); e.Handled = true; return;
                case Keys.F3: FindNext(_lastSearchHit + 1); e.Handled = true; return;

                default: return;
            }

            if (shift)
            {
                if (_selAnchor < 0) _selAnchor = old;
                _selStart = _selAnchor; _selEnd = _cursor;
            }
            else
            { _selAnchor = -1; _selStart = _cursor; _selEnd = _cursor; }

            EnsureCursorVisible();
            hexPanel.Invalidate();
            UpdateStatus();
            e.Handled = true;
        }

        private static int KeyToNibble(Keys k)
        {
            if (k >= Keys.D0 && k <= Keys.D9) return k - Keys.D0;
            if (k >= Keys.A && k <= Keys.F) return 10 + (k - Keys.A);
            if (k >= Keys.NumPad0 && k <= Keys.NumPad9) return k - Keys.NumPad0;
            return -1;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Resize & Scroll
        // ══════════════════════════════════════════════════════════════════════

        private void hexPanel_Resize(object sender, EventArgs e)
        {
            _visibleRows = Math.Max(1, (hexPanel.Height - HeaderHeight) / RowHeight);
            SyncScrollBar(); hexPanel.Invalidate();
        }

        private void vScroll_Scroll(object sender, ScrollEventArgs e)
        {
            _scrollOffset = (long)e.NewValue * BytesPerRow;
            hexPanel.Invalidate();
        }

        private void SyncScrollBar()
        {
            if (_data == null) { vScroll.Maximum = 0; return; }
            int total = (_data.Length + BytesPerRow - 1) / BytesPerRow;
            int vis = Math.Max(1, _visibleRows);
            vScroll.Minimum = 0;
            vScroll.Maximum = Math.Max(0, total - 1);
            vScroll.LargeChange = vis;
            vScroll.SmallChange = 1;
            vScroll.Value = (int)Math.Min(_scrollOffset / BytesPerRow,
                              Math.Max(0, vScroll.Maximum));
        }

        private void ClampScroll()
        {
            if (_data == null) return;
            long max = Math.Max(0,
                ((_data.Length + BytesPerRow - 1) / BytesPerRow - _visibleRows) * (long)BytesPerRow);
            _scrollOffset = Math.Max(0, Math.Min(_scrollOffset, max));
        }

        private void EnsureCursorVisible()
        {
            long cr = _cursor / BytesPerRow;
            long tr = _scrollOffset / BytesPerRow;
            long br = tr + _visibleRows - 1;
            if (cr < tr) _scrollOffset = cr * BytesPerRow;
            else if (cr > br) _scrollOffset = (cr - _visibleRows + 1) * BytesPerRow;
            ClampScroll(); SyncScrollBar();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Toolbar Buttons
        // ══════════════════════════════════════════════════════════════════════

        private void btnCopyHex_Click(object sender, EventArgs e) => CopySelectedHex();
        private void btnFindNext_Click(object sender, EventArgs e) => FindNext(_lastSearchHit + 1);
        private void btnGoto_Click(object sender, EventArgs e) => GotoDialog();
        private void btnSearch_Click(object sender, EventArgs e) => OpenSearchDialog();

        private void btnCopyOffset_Click(object sender, EventArgs e)
        {
            if (_data == null) return;
            Clipboard.SetText($"0x{(_dataOffset + _cursor):X8}");
            lblStatus.Text = $"Offset 0x{(_dataOffset + _cursor):X8} copied.";
        }

        private void btnScreenshot_Click(object sender, EventArgs e)
        {
            using var bmpForm = new Bitmap(Width, Height);
            using var bmpHex = new Bitmap(hexPanel.Width, hexPanel.Height);
            DrawToBitmap(bmpForm, new Rectangle(0, 0, bmpForm.Width, bmpForm.Height));
            hexPanel.DrawToBitmap(bmpHex, new Rectangle(0, 0, bmpHex.Width, bmpHex.Height));

            using var menu = DarkDialog("Screenshot", 340, 190);
            menu.Controls.Add(MakeLabel("What do you want to do?",
                new Point(14, 14), Color.FromArgb(160, 200, 255), bold: true));
            menu.Controls.Add(MakeButton("📋  Hex Panel → Clipboard", new Point(14, 44), 300, Color.FromArgb(0, 100, 170),
                (s, _) => { Clipboard.SetImage(bmpHex); lblStatus.Text = "Hex panel copied."; menu.Close(); }));
            menu.Controls.Add(MakeButton("📋  Entire Form → Clipboard", new Point(14, 80), 300, Color.FromArgb(0, 80, 130),
                (s, _) => { Clipboard.SetImage(bmpForm); lblStatus.Text = "Form copied."; menu.Close(); }));
            menu.Controls.Add(MakeButton("💾  Save as PNG file...", new Point(14, 116), 300, Color.FromArgb(55, 55, 55),
                (s, _) =>
                {
                    menu.Close();
                    using var sfd = new SaveFileDialog
                    {
                        Filter = "PNG Image (*.png)|*.png",
                        FileName = $"HexEditor_{_bodyId}_{_actionId}_Dir{_directionId}.png"
                    };
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        bmpForm.Save(sfd.FileName, ImageFormat.Png);
                        lblStatus.Text = $"Screenshot saved: {Path.GetFileName(sfd.FileName)}";
                    }
                }));
            menu.ShowDialog(this);
        }

        private void txtQuickSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            { ParseAndSearch(txtQuickSearch.Text.Trim()); e.Handled = true; }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Region List
        // ══════════════════════════════════════════════════════════════════════

        private void listRegions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listRegions.SelectedItems.Count == 0) return;
            var r = listRegions.SelectedItems[0].Tag as HexRegion;
            if (r == null) return;
            _primaryRegion = r;
            _selStart = r.Offset; _selEnd = r.Offset + r.Length - 1;
            _cursor = r.Offset; _selAnchor = r.Offset;
            ScrollToOffset(r.Offset);
            hexPanel.Invalidate(); UpdateStatus();
            if (r.PreviewImage != null) SetPreviewImage(r.PreviewImage, r);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Context Menu
        // ══════════════════════════════════════════════════════════════════════

        private void ShowContextMenu(Point loc, long byteIndex)
        {
            var m = new ContextMenuStrip();
            m.BackColor = Color.FromArgb(38, 38, 48);
            m.ForeColor = Color.White;
            m.Renderer = new DarkMenuRenderer();

            Cm(m, "Copy Hex (selection)  Ctrl+C", (s, e) => CopySelectedHex());
            Cm(m, "Copy Offset", (s, e) => { Clipboard.SetText($"0x{(_dataOffset + byteIndex):X8}"); });
            m.Items.Add(new ToolStripSeparator());

            HexRegion r = GetRegionAt(byteIndex);
            if (r != null)
            {
                Cm(m, $"Select region: {r.Label}", (s, e) => { _selStart = r.Offset; _selEnd = r.Offset + r.Length - 1; _cursor = r.Offset; hexPanel.Invalidate(); UpdateStatus(); });
                Cm(m, $"Copy region hex: {r.Label}", (s, e) => CopyRangeHex(r.Offset, r.Offset + r.Length - 1));
                m.Items.Add(new ToolStripSeparator());
            }

            Cm(m, "Select All  Ctrl+A", (s, e) => SelectAll());
            Cm(m, "Find…  Ctrl+F", (s, e) => OpenSearchDialog());
            m.Items.Add(new ToolStripSeparator());
            Cm(m, "🔬  Byte Inspector  Ctrl+I", (s, e) => ShowByteInspector());
            Cm(m, "📌  Add Bookmark  Ctrl+B", (s, e) => AddBookmark());
            Cm(m, "📋  Show Bookmarks  Ctrl+Shift+B", (s, e) => ShowBookmarks());
            Cm(m, "📊  Statistics  Ctrl+T", (s, e) => ShowStatistics());
            Cm(m, "💾  Export Selection  Ctrl+E", (s, e) => ExportSelection());
            m.Items.Add(new ToolStripSeparator());
            Cm(m, "✎   Toggle Write Mode  Ctrl+W", (s, e) => ToggleWriteMode());
            Cm(m, "↩   Undo  Ctrl+Z", (s, e) => Undo());
            Cm(m, "↪   Redo  Ctrl+Y", (s, e) => Redo());
            m.Items.Add(new ToolStripSeparator());
            Cm(m, "⬛  Diff Mode  Ctrl+D", (s, e) => OpenDiffDialog());
            Cm(m, "🎨  Palette Preview  Ctrl+P", (s, e) => ShowPalettePreview());
            Cm(m, "🌳  Structure Tree  Ctrl+S", (s, e) => ShowStructureTree());
            m.Items.Add(new ToolStripSeparator());
            Cm(m, "❓  Command Overview  F1", (s, e) => ShowCommandOverview());

            m.Show(hexPanel, loc);
        }

        private static void Cm(ContextMenuStrip m, string text, EventHandler h)
        {
            var it = new ToolStripMenuItem(text);
            it.Click += h;
            it.BackColor = Color.FromArgb(38, 38, 48);
            it.ForeColor = Color.White;
            m.Items.Add(it);
        }

        // dark renderer for context menu
        private class DarkMenuRenderer : ToolStripProfessionalRenderer
        {
            public DarkMenuRenderer() : base(new DarkColors()) { }
            private class DarkColors : ProfessionalColorTable
            {
                public override Color MenuItemSelected => Color.FromArgb(55, 100, 160);
                public override Color MenuItemBorder => Color.FromArgb(70, 70, 90);
                public override Color MenuBorder => Color.FromArgb(60, 60, 80);
                public override Color ToolStripDropDownBackground => Color.FromArgb(38, 38, 48);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Search
        // ══════════════════════════════════════════════════════════════════════

        private void OpenSearchDialog()
        {
            using var frm = DarkDialog("Hex Search", 430, 140);
            frm.Controls.Add(MakeLabel("Hex bytes (e.g.  FF 00 A3  or  FF00A3):",
                new Point(10, 12), Color.FromArgb(160, 200, 255)));
            var txt = MakeMono(new Point(10, 34), 400, txtQuickSearch.Text);
            var ok = MakeButton("Search", new Point(240, 68), 80, Color.FromArgb(0, 90, 160), null, DialogResult.OK);
            var can = MakeButton("Cancel", new Point(328, 68), 80, Color.FromArgb(70, 70, 70), null, DialogResult.Cancel);
            frm.Controls.AddRange(new Control[] { txt, ok, can });
            frm.AcceptButton = ok; frm.CancelButton = can;
            if (frm.ShowDialog(this) == DialogResult.OK)
            { txtQuickSearch.Text = txt.Text; ParseAndSearch(txt.Text.Trim()); }
        }

        private void ParseAndSearch(string hexStr)
        {
            if (string.IsNullOrWhiteSpace(hexStr)) return;
            hexStr = hexStr.Replace(" ", "").Replace("-", "");
            if (hexStr.Length % 2 != 0) { lblStatus.Text = "Invalid hex pattern (odd length)."; return; }
            try
            {
                var bytes = new byte[hexStr.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                    bytes[i] = Convert.ToByte(hexStr.Substring(i * 2, 2), 16);
                _searchPattern = bytes;
                FindNext(0);
            }
            catch { lblStatus.Text = "Invalid hex pattern."; }
        }

        private void FindNext(long startFrom)
        {
            if (_data == null || _searchPattern == null || _searchPattern.Length == 0) return;
            long pos = Math.Max(0, startFrom);

            for (long i = pos; i <= _data.Length - _searchPattern.Length; i++)
                if (IsMatch(i)) { HitFound(i); return; }

            for (long i = 0; i < Math.Min(startFrom, _data.Length - _searchPattern.Length); i++)
                if (IsMatch(i)) { HitFound(i, wrap: true); return; }

            lblStatus.Text = $"No match for {BitConverter.ToString(_searchPattern)}.";
        }

        private bool IsMatch(long pos)
        {
            for (int j = 0; j < _searchPattern.Length; j++)
                if (_data[pos + j] != _searchPattern[j]) return false;
            return true;
        }

        private void HitFound(long pos, bool wrap = false)
        {
            _lastSearchHit = pos;
            _selStart = pos; _selEnd = pos + _searchPattern.Length - 1;
            _cursor = pos; _selAnchor = pos;
            ScrollToOffset(pos);
            hexPanel.Invalidate(); UpdateStatus();
            lblStatus.Text += $"  │  {(wrap ? "Wrap: " : "")}Hit at 0x{(_dataOffset + pos):X8}";
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Goto
        // ══════════════════════════════════════════════════════════════════════

        private void GotoDialog()
        {
            using var frm = DarkDialog("Go to Offset", 360, 120);
            frm.Controls.Add(MakeLabel("Offset (hex, e.g. 0x1A4 or 1A4):",
                new Point(10, 12), Color.FromArgb(160, 200, 255)));
            var txt = MakeMono(new Point(10, 34), 330, "");
            var ok = MakeButton("OK", new Point(170, 68), 75, Color.FromArgb(0, 90, 160), null, DialogResult.OK);
            var can = MakeButton("Cancel", new Point(252, 68), 80, Color.FromArgb(70, 70, 70), null, DialogResult.Cancel);
            frm.Controls.AddRange(new Control[] { txt, ok, can });
            frm.AcceptButton = ok; frm.CancelButton = can;
            if (frm.ShowDialog(this) != DialogResult.OK) return;

            string raw = txt.Text.Trim();
            if (raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) raw = raw.Substring(2);
            if (!long.TryParse(raw, System.Globalization.NumberStyles.HexNumber, null, out long offset))
            { lblStatus.Text = "Invalid offset."; return; }

            long rel = offset >= _dataOffset ? offset - _dataOffset : offset;
            rel = Math.Max(0, Math.Min(rel, _data.Length - 1));
            _cursor = rel; _selStart = rel; _selEnd = rel; _selAnchor = rel;
            ScrollToOffset(rel); hexPanel.Invalidate(); UpdateStatus();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Clipboard / Selection
        // ══════════════════════════════════════════════════════════════════════

        private void SelectAll()
        {
            if (_data == null) return;
            _selStart = 0; _selEnd = _data.Length - 1; _selAnchor = 0;
            hexPanel.Invalidate(); UpdateStatus();
        }

        private void CopySelectedHex()
        {
            if (_data == null || _selStart < 0 || _selEnd < _selStart) return;
            CopyRangeHex(_selStart, _selEnd);
        }

        private void CopyRangeHex(long from, long to)
        {
            if (_data == null) return;
            from = Math.Max(0, from);
            to = Math.Min(_data.Length - 1, to);
            var sb = new StringBuilder();
            for (long i = from; i <= to; i++) { if (i > from) sb.Append(' '); sb.Append(_data[i].ToString("X2")); }
            Clipboard.SetText(sb.ToString());
            lblStatus.Text = $"{to - from + 1} bytes copied.";
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Write Mode & Undo
        // ══════════════════════════════════════════════════════════════════════

        private void ToggleWriteMode()
        {
            _writeMode = !_writeMode;
            _nibbleMode = false;
            lblStatus.Text = _writeMode
                ? "✎ WRITE MODE ON — type two hex digits to overwrite a byte. Ctrl+Z = undo."
                : "Write mode OFF.";
            hexPanel.Invalidate();
        }

        private void WriteByte(long offset, byte newValue)
        {
            if (offset < 0 || offset >= _data.Length) return;
            byte old = _data[offset];
            if (old == newValue) return;
            _data[offset] = newValue;
            _modifiedBytes.Add(offset);
            _undoStack.Push(new UndoEntry
            {
                Offset = offset,
                OldValue = old,
                NewValue = newValue,
                Description = $"Write 0x{newValue:X2} @ 0x{(_dataOffset + offset):X8}"
            });
            _redoStack.Clear();
        }

        private void Undo()
        {
            if (_undoStack.Count == 0) { lblStatus.Text = "Nothing to undo."; return; }
            var u = _undoStack.Pop();
            _data[u.Offset] = u.OldValue;
            _modifiedBytes.Remove(u.Offset);
            _redoStack.Push(u);
            _cursor = u.Offset; ScrollToOffset(u.Offset);
            hexPanel.Invalidate(); UpdateStatus();
            lblStatus.Text = $"Undo: {u.Description}";
        }

        private void Redo()
        {
            if (_redoStack.Count == 0) { lblStatus.Text = "Nothing to redo."; return; }
            var u = _redoStack.Pop();
            _data[u.Offset] = u.NewValue;
            _modifiedBytes.Add(u.Offset);
            _undoStack.Push(u);
            _cursor = u.Offset; ScrollToOffset(u.Offset);
            hexPanel.Invalidate(); UpdateStatus();
            lblStatus.Text = $"Redo: {u.Description}";
        }

        // ══════════════════════════════════════════════════════════════════════
        //  DIFF MODE
        // ══════════════════════════════════════════════════════════════════════

        private void OpenDiffDialog()
        {
            if (_data == null) { MessageBox.Show("Load a buffer first.", "Diff"); return; }

            if (_diffMode)
            {
                _diffMode = false;
                _diffData = null;
                _diffLabel = "";
                lblStatus.Text = "Diff mode off.";
                hexPanel.Invalidate();
                return;
            }

            using var ofd = new OpenFileDialog
            {
                Title = "Select second file for comparison",
                Filter = "Binary files (*.bin;*.mul;*.uop)|*.bin;*.mul;*.uop|All files (*.*)|*.*"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                _diffData = File.ReadAllBytes(ofd.FileName);
                _diffLabel = Path.GetFileName(ofd.FileName);
                _diffMode = true;
                lblStatus.Text = $"Diff ON — {_diffLabel}  ({CountDiffBytes()} different bytes)";
                hexPanel.Invalidate();
            }
            catch (Exception ex)
            { MessageBox.Show($"Could not load file:\n{ex.Message}", "Diff Error"); }
        }

        private int CountDiffBytes()
        {
            if (_diffData == null) return 0;
            long len = Math.Min(_data.Length, _diffData.Length);
            int cnt = 0;
            for (long i = 0; i < len; i++) if (_data[i] != _diffData[i]) cnt++;
            cnt += (int)Math.Abs(_data.Length - _diffData.Length);
            return cnt;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  BYTE INSPECTOR
        // ══════════════════════════════════════════════════════════════════════

        public void ShowByteInspector()
        {
            if (_data == null || _cursor < 0 || _cursor >= _data.Length) return;
            long pos = _cursor;
            var sb = new StringBuilder();
            sb.AppendLine($"─── Byte Inspector @ 0x{(_dataOffset + pos):X8} ───");
            sb.AppendLine();
            byte b = _data[pos];
            sb.AppendLine($"  BYTE   uint8    {b,3}   0x{b:X2}   bin: {Convert.ToString(b, 2).PadLeft(8, '0')}");
            sb.AppendLine($"  SBYTE  int8     {(sbyte)b,4}");
            sb.AppendLine();
            if (pos + 1 < _data.Length)
            {
                ushort u16 = (ushort)(_data[pos] | (_data[pos + 1] << 8));
                sb.AppendLine($"  WORD   uint16   {u16,6}   0x{u16:X4}");
                sb.AppendLine($"  SWORD  int16    {(short)u16,6}");
                sb.AppendLine();
            }
            if (pos + 3 < _data.Length)
            {
                uint u32 = (uint)(_data[pos] | (_data[pos + 1] << 8) | (_data[pos + 2] << 16) | (_data[pos + 3] << 24));
                float f32 = BitConverter.ToSingle(_data, (int)pos);
                sb.AppendLine($"  DWORD  uint32   {u32,12}   0x{u32:X8}");
                sb.AppendLine($"  SDWORD int32    {(int)u32,12}");
                sb.AppendLine($"  FLOAT  32-bit   {f32,16:G8}");
                sb.AppendLine();
            }
            if (pos + 7 < _data.Length)
            {
                ulong u64 = BitConverter.ToUInt64(_data, (int)pos);
                double d64 = BitConverter.ToDouble(_data, (int)pos);
                sb.AppendLine($"  QWORD  uint64   {u64}");
                sb.AppendLine($"  SQWORD int64    {(long)u64}");
                sb.AppendLine($"  DOUBLE 64-bit   {d64:G12}");
            }
            HexRegion r = GetRegionAt(pos);
            if (r != null)
            {
                sb.AppendLine(); sb.AppendLine("─── Region ───────────────────────────────");
                sb.AppendLine($"  Name    {r.Label}");
                sb.AppendLine($"  Start   0x{(_dataOffset + r.Offset):X8}");
                sb.AppendLine($"  Length  {r.Length} bytes");
                sb.AppendLine($"  Pos.    +{pos - r.Offset} within region");
                if (r.Tooltip != null) sb.AppendLine($"  Info    {r.Tooltip}");
            }
            ShowMonoDialog($"Byte Inspector @ 0x{(_dataOffset + pos):X8}", sb.ToString(), 480, 370);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  BOOKMARKS
        // ══════════════════════════════════════════════════════════════════════

        public void AddBookmark()
        {
            if (_data == null) return;
            string def = $"Mark {_bookmarks.Count + 1} @ 0x{(_dataOffset + _cursor):X8}";
            using var frm = DarkDialog("Add Bookmark", 360, 110);
            var txt = MakeMono(new Point(10, 12), 325, def);
            var ok = MakeButton("Add", new Point(190, 44), 90, Color.FromArgb(0, 100, 160), null, DialogResult.OK);
            var can = MakeButton("Cancel", new Point(286, 44), 60, Color.FromArgb(70, 70, 70), null, DialogResult.Cancel);
            frm.Controls.AddRange(new Control[] { txt, ok, can });
            frm.AcceptButton = ok;
            if (frm.ShowDialog(this) == DialogResult.OK)
            { _bookmarks.Add((_cursor, txt.Text.Trim())); lblStatus.Text = $"Bookmark: {txt.Text.Trim()}"; hexPanel.Invalidate(); }
        }

        public void ShowBookmarks()
        {
            if (_bookmarks.Count == 0)
            { MessageBox.Show("No bookmarks.\nCtrl+B to add one.", "Bookmarks"); return; }

            using var frm = new Form
            {
                Text = "Bookmarks",
                Size = new Size(460, 310),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(38, 38, 48),
                ForeColor = Color.White
            };
            var lv = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                BackColor = Color.FromArgb(35, 35, 45),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9f),
                BorderStyle = BorderStyle.None
            };
            lv.Columns.Add("Offset", 130);
            lv.Columns.Add("Label", 290);

            foreach (var (off, lbl) in _bookmarks)
            {
                var it = new ListViewItem($"0x{(_dataOffset + off):X8}") { Tag = off };
                it.SubItems.Add(lbl);
                it.BackColor = Color.FromArgb(35, 35, 45);
                it.ForeColor = Color.White;
                lv.Items.Add(it);
            }

            lv.DoubleClick += (s, e) =>
            {
                if (lv.SelectedItems.Count == 0) return;
                long off = (long)lv.SelectedItems[0].Tag;
                _cursor = off; ScrollToOffset(off); hexPanel.Invalidate(); UpdateStatus();
                frm.Close();
            };

            var del = MakeButton("Remove Selected", new Point(0, 0), 0,
                Color.FromArgb(140, 40, 40), (s, e) =>
                {
                    if (lv.SelectedItems.Count == 0) return;
                    long off = (long)lv.SelectedItems[0].Tag;
                    _bookmarks.RemoveAll(bm => bm.Offset == off);
                    lv.SelectedItems[0].Remove();
                    hexPanel.Invalidate();
                });
            del.Dock = DockStyle.Bottom; del.Height = 28;
            frm.Controls.Add(lv); frm.Controls.Add(del);
            frm.ShowDialog(this);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  STATISTICS
        // ══════════════════════════════════════════════════════════════════════

        public void ShowStatistics()
        {
            if (_data == null) return;
            long from, to;
            string scope;
            if (_selStart >= 0 && _selEnd > _selStart)
            { from = Math.Min(_selStart, _selEnd); to = Math.Max(_selStart, _selEnd); scope = $"Selection 0x{(_dataOffset + from):X8}–0x{(_dataOffset + to):X8}"; }
            else
            { from = 0; to = _data.Length - 1; scope = "Entire block"; }

            long count = to - from + 1;
            var freq = new int[256];
            long zeros = 0;
            for (long i = from; i <= to; i++) { freq[_data[i]]++; if (_data[i] == 0) zeros++; }
            var top5 = freq.Select((f, i) => (B: i, F: f)).OrderByDescending(x => x.F).Take(5).ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"─── Statistics: {scope} ───");
            sb.AppendLine($"  Total bytes    : {count:N0}");
            sb.AppendLine($"  Zero bytes     : {zeros:N0}  ({zeros * 100.0 / count:F1} %)");
            sb.AppendLine($"  Non-zero       : {count - zeros:N0}  ({(count - zeros) * 100.0 / count:F1} %)");
            sb.AppendLine($"  Unique values  : {freq.Count(f => f > 0)}  of 256");
            sb.AppendLine(); sb.AppendLine("  Top 5 most frequent bytes:");
            foreach (var (B, F) in top5)
            {
                if (F == 0) break;
                char c = (B >= 32 && B < 127) ? (char)B : '.';
                sb.AppendLine($"    0x{B:X2} ({B,3})  '{c}'  {F:N0}×  ({F * 100.0 / count:F1} %)");
            }
            double ent = 0;
            foreach (int f in freq) { if (f <= 0) continue; double p = (double)f / count; ent -= p * Math.Log(p, 2); }
            sb.AppendLine(); sb.AppendLine($"  Shannon entropy : {ent:F4} bit/byte");
            sb.AppendLine($"  (8.0 = random/compressed,  ~0 = repetitive)");
            if (_diffMode && _diffData != null)
            {
                int diffs = CountDiffBytes();
                long dlen = Math.Max(_data.Length, _diffData.Length);
                sb.AppendLine(); sb.AppendLine("─── Diff Summary ──────────────────────────");
                sb.AppendLine($"  Buffer A size  : {_data.Length:N0} bytes");
                sb.AppendLine($"  Buffer B size  : {_diffData.Length:N0} bytes");
                sb.AppendLine($"  Different bytes: {diffs:N0}  ({diffs * 100.0 / dlen:F1} %)");
                sb.AppendLine($"  Identical      : {dlen - diffs:N0}  ({(dlen - diffs) * 100.0 / dlen:F1} %)");
            }
            ShowMonoDialog("Block Statistics", sb.ToString(), 520, 360);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  EXPORT SELECTION
        // ══════════════════════════════════════════════════════════════════════

        public void ExportSelection()
        {
            if (_data == null) return;
            long from = _selStart >= 0 && _selEnd >= _selStart ? Math.Min(_selStart, _selEnd) : 0;
            long to = _selStart >= 0 && _selEnd >= _selStart ? Math.Max(_selStart, _selEnd) : _data.Length - 1;
            using var sfd = new SaveFileDialog
            {
                Filter = "Binary file (*.bin)|*.bin|All files (*.*)|*.*",
                FileName = $"export_0x{(_dataOffset + from):X8}_{to - from + 1}bytes.bin"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            var buf = new byte[to - from + 1];
            Array.Copy(_data, from, buf, 0, buf.Length);
            File.WriteAllBytes(sfd.FileName, buf);
            lblStatus.Text = $"{buf.Length:N0} bytes exported → {Path.GetFileName(sfd.FileName)}";
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PALETTE PREVIEW
        // ══════════════════════════════════════════════════════════════════════

        public void ShowPalettePreview()
        {
            if (_data == null) return;
            long palOff = 0;
            var pr = _regions.FirstOrDefault(r => r.Label != null && r.Label.ToLower().Contains("palette"));
            if (pr != null) palOff = pr.Offset;
            if (palOff + 512 > _data.Length)
            { MessageBox.Show("No palette data found (need 512 bytes of ARGB1555 at expected offset).", "Palette Preview"); return; }

            var colors = new Color[256];
            for (int i = 0; i < 256; i++)
            {
                long p = palOff + i * 2;
                ushort v = (ushort)(_data[p] | (_data[p + 1] << 8));
                int r5 = (v >> 10) & 0x1F, g5 = (v >> 5) & 0x1F, b5 = v & 0x1F;
                colors[i] = Color.FromArgb(
                    (v & 0x8000) != 0 ? 255 : 0,
                    r5 * 255 / 31, g5 * 255 / 31, b5 * 255 / 31);
            }

            const int cellSize = 22, cols = 16, rows = 16;
            int bmpW = cols * cellSize + 1;
            int bmpH = rows * cellSize + 28;

            using var frm = new Form
            {
                Text = $"Palette Preview @ 0x{(_dataOffset + palOff):X8}",
                Size = new Size(bmpW + 230, bmpH + 50),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(28, 28, 32),
                ForeColor = Color.White,
                MinimumSize = new Size(420, 300)
            };

            var canvas = new Panel { Location = new Point(8, 8), Size = new Size(bmpW, bmpH), BackColor = Color.FromArgb(28, 28, 32) };
            var infoLbl = new Label { Location = new Point(bmpW + 16, 8), Size = new Size(200, bmpH), ForeColor = Color.FromArgb(180, 220, 255), Font = new Font("Consolas", 8.5f) };

            int hovered = -1;

            canvas.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                g.Clear(Color.FromArgb(28, 28, 32));
                using var tb = new SolidBrush(Color.FromArgb(140, 190, 255));
                g.DrawString($"256-color ARGB1555 palette  (offset 0x{(_dataOffset + palOff):X8})",
                    new Font("Segoe UI", 8f), tb, new PointF(2, 2));
                int top = 20;
                for (int i = 0; i < 256; i++)
                {
                    int cx = (i % cols) * cellSize, cy = top + (i / cols) * cellSize;
                    using var br = new SolidBrush(colors[i].A == 0 ? Color.FromArgb(20, 20, 25) : colors[i]);
                    g.FillRectangle(br, cx + 1, cy + 1, cellSize - 2, cellSize - 2);
                    using var gp = new Pen(i == hovered ? Color.White : Color.FromArgb(50, 50, 55), i == hovered ? 2f : 1f);
                    g.DrawRectangle(gp, cx, cy, cellSize, cellSize);
                    if (i % 16 == 0)
                    {
                        using var ib = new SolidBrush(Color.FromArgb(90, 90, 110));
                        g.DrawString(i.ToString(), new Font("Consolas", 6f), ib, new PointF(cx + 1, cy + 1));
                    }
                }
            };

            canvas.MouseMove += (s, e) =>
            {
                int top = 20; if (e.Y < top) { hovered = -1; infoLbl.Text = ""; canvas.Invalidate(); return; }
                int col = e.X / cellSize, row = (e.Y - top) / cellSize;
                if (col >= cols || row >= rows) { hovered = -1; infoLbl.Text = ""; canvas.Invalidate(); return; }
                int idx = row * cols + col; if (idx < 0 || idx >= 256) return;
                hovered = idx; canvas.Invalidate();
                long p = palOff + idx * 2;
                ushort v = (ushort)(_data[p] | (_data[p + 1] << 8));
                Color c = colors[idx];
                infoLbl.Text =
                    $"Index   : {idx}\n" +
                    $"Raw     : 0x{v:X4}\n" +
                    $"File off: 0x{(_dataOffset + p):X8}\n" +
                    $"─────────────────\n" +
                    $"R5 : {(v >> 10) & 0x1F}\n" +
                    $"G5 : {(v >> 5) & 0x1F}\n" +
                    $"B5 : {v & 0x1F}\n" +
                    $"─────────────────\n" +
                    $"R  : {c.R}\n" +
                    $"G  : {c.G}\n" +
                    $"B  : {c.B}\n" +
                    $"Alpha: {(c.A == 0 ? "transparent" : "opaque")}\n" +
                    $"─────────────────\n" +
                    $"HTML: #{c.R:X2}{c.G:X2}{c.B:X2}";
            };
            canvas.MouseLeave += (s, e) => { hovered = -1; infoLbl.Text = ""; canvas.Invalidate(); };
            canvas.Click += (s, e) => { _cursor = palOff; ScrollToOffset(palOff); hexPanel.Invalidate(); UpdateStatus(); };

            frm.Controls.Add(canvas); frm.Controls.Add(infoLbl);
            frm.ShowDialog(this);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  STRUCTURE TREE
        // ══════════════════════════════════════════════════════════════════════

        private sealed class StructField { public long Offset; public int Length; public string Description; }

        public void ShowStructureTree()
        {
            if (_data == null) { MessageBox.Show("No data loaded.", "Structure Tree"); return; }

            using var frm = new Form
            {
                Text = "Structure Tree",
                Size = new Size(720, 530),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(28, 28, 32),
                ForeColor = Color.White
            };

            var tree = new TreeView
            {
                Dock = DockStyle.Left,
                Width = 320,
                BackColor = Color.FromArgb(33, 33, 42),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9f),
                BorderStyle = BorderStyle.None,
                LineColor = Color.FromArgb(80, 80, 100)
            };

            var detail = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(28, 28, 38),
                ForeColor = Color.FromArgb(180, 220, 255),
                Font = new Font("Consolas", 9.5f),
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Vertical,
                Text = "Click a field to see details."
            };

            if (_isUop) BuildUopTree(tree, _data);
            else BuildMulTree(tree, _data);

            tree.AfterSelect += (s, e) =>
            {
                if (e.Node?.Tag is StructField sf)
                {
                    detail.Text = sf.Description;
                    if (sf.Offset >= 0 && sf.Offset < _data.Length)
                    {
                        _cursor = sf.Offset; _selStart = sf.Offset;
                        _selEnd = sf.Offset + sf.Length - 1; _selAnchor = sf.Offset;
                        ScrollToOffset(sf.Offset); hexPanel.Invalidate(); UpdateStatus();
                    }
                }
            };

            tree.ExpandAll();
            frm.Controls.Add(detail); frm.Controls.Add(tree);
            frm.ShowDialog(this);
        }

        private void BuildUopTree(TreeView tree, byte[] data)
        {
            var root = new TreeNode("UOP Animation Block") { ForeColor = Color.FromArgb(255, 215, 0) };
            if (data.Length < 4) { root.Nodes.Add("(too short)"); tree.Nodes.Add(root); return; }
            SF(root, data, 0, 4, "Magic / FileType", "4-byte format identifier.");
            SF(root, data, 4, 4, "Body ID", "Animation body ID.");
            SF(root, data, 8, 4, "Frame Count", "Total frames in this direction block.");
            SF(root, data, 12, 4, "Action Index", "Action/group index (0=walk, 1=idle …).");
            SF(root, data, 16, 4, "Direction", "Direction 0–4.");
            SF(root, data, 20, 4, "Flags", "Reserved flags.");

            int fc = data.Length >= 12
                ? Math.Min((int)((uint)(data[8] | (data[9] << 8) | (data[10] << 16) | (data[11] << 24))), 256) : 0;
            if (fc > 0 && 24 + fc * 12 <= data.Length)
            {
                var tn = new TreeNode($"Frame Table  ({fc} entries)") { ForeColor = Color.FromArgb(100, 200, 255) };
                for (int i = 0; i < Math.Min(fc, 32); i++)
                {
                    long tp = 24 + i * 12;
                    int fo = data.Length > tp + 3 ? data[tp] | (data[tp + 1] << 8) | (data[tp + 2] << 16) | (data[tp + 3] << 24) : 0;
                    int fl = data.Length > tp + 7 ? data[tp + 4] | (data[tp + 5] << 8) | (data[tp + 6] << 16) | (data[tp + 7] << 24) : 0;
                    int fx = data.Length > tp + 11 ? data[tp + 8] | (data[tp + 9] << 8) | (data[tp + 10] << 16) | (data[tp + 11] << 24) : 0;
                    tn.Nodes.Add(new TreeNode($"Frame {i}  → 0x{fo:X}  len {fl}")
                    {
                        ForeColor = Color.FromArgb(180, 220, 180),
                        Tag = new StructField
                        {
                            Offset = tp,
                            Length = 12,
                            Description = $"Frame Table Entry {i}\n────────────────────────\nEntry offset : 0x{(_dataOffset + tp):X8}\nData offset  : 0x{(_dataOffset + fo):X8}  ({fo})\nData length  : {fl} bytes\nExtra/Flags  : 0x{fx:X8}"
                        }
                    });
                }
                if (fc > 32) tn.Nodes.Add(new TreeNode($"… {fc - 32} more"));
                root.Nodes.Add(tn);
            }
            tree.Nodes.Add(root);
        }

        private void BuildMulTree(TreeView tree, byte[] data)
        {
            var root = new TreeNode("MUL Animation Block") { ForeColor = Color.FromArgb(255, 215, 0) };
            if (data.Length < 514) { root.Nodes.Add("(too short)"); tree.Nodes.Add(root); return; }

            var pn = new TreeNode("Palette  [0x000–0x1FF]  512 bytes") { ForeColor = Color.FromArgb(255, 180, 60) };
            for (int i = 0; i < 16; i++)
            {
                long p = i * 2; ushort v = (ushort)(data[p] | (data[p + 1] << 8));
                pn.Nodes.Add(new TreeNode($"[{i:D3}]  0x{v:X4}")
                {
                    ForeColor = Color.FromArgb(180, 200, 180),
                    Tag = new StructField
                    {
                        Offset = p,
                        Length = 2,
                        Description = $"Palette entry {i}\nRaw  : 0x{v:X4}\nR5   : {(v >> 10) & 0x1F}\nG5   : {(v >> 5) & 0x1F}\nB5   : {v & 0x1F}\nAlpha: {((v & 0x8000) != 0 ? "opaque" : "transparent")}"
                    }
                });
            }
            pn.Nodes.Add(new TreeNode("… 240 more entries"));
            root.Nodes.Add(pn);

            int fc = data.Length >= 514 ? data[512] | (data[513] << 8) : 0; fc = Math.Min(fc, 256);
            SF(root, data, 512, 2, "Frame Count", $"Frames in this direction. Value: {fc}");

            if (fc > 0)
            {
                var tn = new TreeNode($"Frame Lookup  ({fc} offsets @ 0x202)") { ForeColor = Color.FromArgb(100, 200, 255) };
                for (int i = 0; i < Math.Min(fc, 32); i++)
                {
                    long lp = 514 + i * 4;
                    if (lp + 4 > data.Length) break;
                    int off = data[lp] | (data[lp + 1] << 8) | (data[lp + 2] << 16) | (data[lp + 3] << 24);
                    tn.Nodes.Add(new TreeNode($"[{i}] → 0x{off:X}")
                    {
                        ForeColor = Color.FromArgb(180, 200, 180),
                        Tag = new StructField
                        {
                            Offset = lp,
                            Length = 4,
                            Description = $"Frame {i} offset : 0x{(_dataOffset + off):X8}  ({off})"
                        }
                    });
                }
                root.Nodes.Add(tn);
            }
            tree.Nodes.Add(root);
        }

        // helper: add struct field node
        private void SF(TreeNode parent, byte[] data, long offset, int len, string name, string desc)
        {
            string vs = "";
            if (offset + len <= data.Length)
            {
                uint v = 0;
                for (int i = len - 1; i >= 0; i--) v = (v << 8) | data[offset + i];
                vs = $" = 0x{v:X}  ({v})";
            }
            parent.Nodes.Add(new TreeNode($"{name}{vs}")
            {
                ForeColor = Color.FromArgb(200, 200, 220),
                Tag = new StructField
                {
                    Offset = offset,
                    Length = len,
                    Description = $"{name}\n────────────────────────\nFile offset : 0x{(_dataOffset + offset):X8}\nSize        : {len} byte(s)\nValue       : {vs.Trim()}\n\n{desc}"
                }
            });
        }

        // ══════════════════════════════════════════════════════════════════════
        //  COMMAND OVERVIEW  (F1)
        // ══════════════════════════════════════════════════════════════════════

        public void ShowCommandOverview()
        {
            const string txt =
@"════════════════════════════════════════════════════════
  ANIMATION HEX EDITOR  —  Keyboard & Mouse Reference
════════════════════════════════════════════════════════

  NAVIGATION
  ──────────────────────────────────────────────────────
  Arrow keys             Move cursor one byte / one row
  Home                   Start of current row
  End                    End of current row
  Ctrl+Home              Jump to first byte
  Ctrl+End               Jump to last byte
  Page Up / Page Down    Scroll one visible page
  Mouse wheel            Scroll 3 rows
  Mouse click            Set cursor

  SELECTION
  ──────────────────────────────────────────────────────
  Shift + arrows         Extend selection
  Shift + click          Extend selection to byte
  Ctrl+A                 Select all bytes
  Double-click on byte   Select entire region

  CLIPBOARD
  ──────────────────────────────────────────────────────
  Ctrl+C                 Copy selected bytes as hex string
  Toolbar 'Copy Offset'  Copy cursor file offset

  SEARCH & GOTO
  ──────────────────────────────────────────────────────
  Ctrl+F                 Open search dialog (hex bytes)
  F3                     Find next match (wraps)
  Ctrl+G                 Go to offset

  WRITE MODE & UNDO
  ──────────────────────────────────────────────────────
  Ctrl+W                 Toggle Write Mode (✎ in header)
  0-9, A-F               Type 2 hex digits → overwrite byte
                         (Write Mode only)
  Ctrl+Z                 Undo last byte change
  Ctrl+Shift+Z  /  Ctrl+Y  Redo

  TOOLS
  ──────────────────────────────────────────────────────
  Ctrl+I                 Byte Inspector
                           BYTE/WORD/DWORD/FLOAT/
                           QWORD/DOUBLE at cursor
  Ctrl+B                 Add bookmark at cursor
  Ctrl+Shift+B           Show bookmarks list
  Ctrl+T                 Block statistics
                           (entropy, zero %, top bytes)
  Ctrl+E                 Export selection to .bin file

  VISUAL / ANALYSIS
  ──────────────────────────────────────────────────────
  Ctrl+D                 Diff Mode
                           Load a second file.
                           Red   = byte differs (buffer A)
                           Green = byte differs (buffer B)
                           Press Ctrl+D again to exit.
  Ctrl+P                 Palette Preview
                           Shows 256-color ARGB1555 swatch.
                           Hover = R/G/B details.
                           Click = jump to palette offset.
  Ctrl+S                 Structure Tree
                           Parses UOP/MUL header fields.
                           Click field → jump + select.

  SCREENSHOT
  ──────────────────────────────────────────────────────
  Toolbar 'Screenshot':
    • Copy hex panel to clipboard
    • Copy entire form to clipboard
    • Save as PNG file

  CONTEXT MENU  (right-click anywhere)
  ──────────────────────────────────────────────────────
  All tools above + region-specific actions.

  COLOR CODING
  ──────────────────────────────────────────────────────
  Gold cursor         Current cursor position
  Blue background     Active selection
  Colored background  Named region (palette/header/frame)
  Orange triangle     Bookmark marker (top-left of byte)
  Yellow background   Modified byte (write mode)
  Dark gray text      Zero byte (0x00)
  Red background      Diff byte (buffer A side)
  Green background    Diff byte (buffer B side)

════════════════════════════════════════════════════════";

            ShowMonoDialog("Command Overview  (F1)", txt, 690, 700);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Internal Helpers
        // ══════════════════════════════════════════════════════════════════════

        private void ApplyRegions(List<HexRegion> regions)
        {
            _regions.Clear(); _primaryRegion = null;
            if (regions != null)
            {
                _regions.AddRange(regions);
                _primaryRegion = _regions.Find(r => r.IsSequenceStart)
                                 ?? (_regions.Count > 0 ? _regions[0] : null);
            }
            RefreshRegionList();
        }

        private void RefreshRegionList()
        {
            listRegions.BeginUpdate(); listRegions.Items.Clear();
            foreach (var r in _regions)
            {
                var it = new ListViewItem(r.Label ?? "Region");
                it.SubItems.Add($"0x{(_dataOffset + r.Offset):X8}");
                it.SubItems.Add(r.Length.ToString("N0"));
                it.SubItems.Add(r.Tooltip ?? "");
                it.Tag = r;
                it.BackColor = r == _primaryRegion ? Color.FromArgb(0, 80, 140) : Color.FromArgb(45, 45, 45);
                it.ForeColor = Color.White;
                listRegions.Items.Add(it);
            }
            listRegions.EndUpdate();
        }

        private void SelectRegionInList(HexRegion region)
        {
            foreach (ListViewItem it in listRegions.Items)
                if (it.Tag == region) { it.Selected = true; it.EnsureVisible(); break; }
        }

        private void ScrollToPrimary()
        { if (_primaryRegion != null) ScrollToOffset(_primaryRegion.Offset); }

        private void ScrollToOffset(long byteOffset)
        {
            if (_data == null) return;
            long row = byteOffset / BytesPerRow;
            long margin = Math.Max(0, _visibleRows / 4);
            long topRow = Math.Max(0, row - margin);
            long maxRow = Math.Max(0, (_data.Length + BytesPerRow - 1) / BytesPerRow - Math.Max(1, _visibleRows));
            _scrollOffset = Math.Min(topRow, maxRow) * BytesPerRow;
            SyncScrollBar();
        }

        private void UpdateTitle()
        {
            if (_data == null) { Text = "Animation Hex Editor"; return; }
            string fn = _sourceFile != null ? Path.GetFileName(_sourceFile) : "(memory)";
            Text = $"Hex Editor  –  {fn}  │  Body:{_bodyId}  Action:{_actionId}  Dir:{_directionId}  [{_data.Length:N0} B]";
        }

        private void UpdateStatus()
        {
            if (_data == null) { lblStatus.Text = "No data loaded."; return; }
            long lo = _selStart >= 0 ? Math.Min(_selStart, _selEnd) : _cursor;
            long hi = _selStart >= 0 ? Math.Max(_selStart, _selEnd) : _cursor;
            long len = _selStart >= 0 && _selEnd >= _selStart ? hi - lo + 1 : 0;
            HexRegion r = GetRegionAt(_cursor);
            string reg = r != null ? $"  │  {r.Label}" : "";
            string bv = _cursor >= 0 && _cursor < _data.Length ? $"  │  0x{_data[_cursor]:X2} ({_data[_cursor]})" : "";
            string sel = len > 1
                ? $"  │  Sel: 0x{(_dataOffset + lo):X8}–0x{(_dataOffset + hi):X8}  ({len:N0} B)"
                : $"  │  0x{(_dataOffset + _cursor):X8}";
            string mod = _modifiedBytes.Count > 0 ? $"  │  {_modifiedBytes.Count} mod" : "";
            string diff = _diffMode ? $"  │  DIFF" : "";
            lblStatus.Text = $"{Path.GetFileName(_sourceFile ?? "?")}  │  {_data.Length:N0} B{sel}{bv}{reg}{mod}{diff}";
        }

        private void TooltipTimer_Tick(object sender, EventArgs e)
        {
            _tooltipTimer.Stop();
            if (_pendingTooltip == null) return;
            toolTip1.Show(_pendingTooltip, hexPanel, _lastMousePos.X + 12, _lastMousePos.Y + 12, 4000);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  UI Factory Helpers
        // ══════════════════════════════════════════════════════════════════════

        private static Form DarkDialog(string title, int w, int h) =>
            new Form
            {
                Text = title,
                Size = new Size(w, h),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                MaximizeBox = false,
                MinimizeBox = false
            };

        private static Label MakeLabel(string text, Point loc, Color fg, bool bold = false) =>
            new Label
            {
                Text = text,
                Location = loc,
                AutoSize = true,
                ForeColor = fg,
                Font = bold ? new Font("Segoe UI", 9f, FontStyle.Bold) : new Font("Segoe UI", 9f)
            };

        private static TextBox MakeMono(Point loc, int w, string text) =>
            new TextBox
            {
                Location = loc,
                Width = w,
                Text = text,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9.5f),
                BorderStyle = BorderStyle.FixedSingle
            };

        private static Button MakeButton(string text, Point loc, int w, Color bg,
            EventHandler onClick, DialogResult dr = DialogResult.None)
        {
            var b = new Button
            {
                Text = text,
                Location = loc,
                Width = w > 0 ? w : 100,
                Height = 28,
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = dr
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            if (onClick != null) b.Click += onClick;
            return b;
        }

        private void ShowMonoDialog(string title, string content, int w, int h)
        {
            var frm = new Form
            {
                Text = title,
                Size = new Size(w, h),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(28, 28, 32),
                ForeColor = Color.White
            };
            frm.Controls.Add(new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(28, 28, 38),
                ForeColor = Color.FromArgb(200, 220, 255),
                Font = new Font("Consolas", 9f),
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Vertical,
                Text = content
            });
            frm.ShowDialog(this);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Form Closing
        // ══════════════════════════════════════════════════════════════════════

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; Hide(); }
            base.OnFormClosing(e);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Color Blend Extension
    // ══════════════════════════════════════════════════════════════════════════

    internal static class ColorExtensions
    {
        public static Color Blend(this Color color, Color target, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return Color.FromArgb(color.A,
                (int)(color.R + (target.R - color.R) * t),
                (int)(color.G + (target.G - color.G) * t),
                (int)(color.B + (target.B - color.B) * t));
        }
    }
}
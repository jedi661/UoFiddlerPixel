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

        // ═══════════════════════════════════════════════════════════
        //  Frame Compare State
        // ═══════════════════════════════════════════════════════════
        private int _compareFrameA = -1;

        // ────────────────────────────────────────────────────────────
        // ── Pin events — fired when user presses Ctrl+1 (Pin A) or Ctrl+2 (Pin B)
        // -───────────────────────────────────────────────────────────
        public event Action<HexCompareBuffer> OnPinAsA;
        public event Action<HexCompareBuffer> OnPinAsB;

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

            // ── NearestNeighbor for picPreview ────────────────────────────────
            // Prevents blurry rendering in pixel art / sprites.
            // PictureBox.SizeMode = Zoom + custom paint = sharp pixels.
            picPreview.SizeMode = PictureBoxSizeMode.Normal;
            picPreview.Paint += PicPreview_Paint;
        }

        // ── Custom Paint for picPreview with NearestNeighbor ─────────────────

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

            // Fit image proportionally into PictureBox (like zoom mode)
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
            picPreview.Invalidate(); // NearestNeighbor Retrigger Paint
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
            if (_diffData == null) return;

            // Each side will have exactly the same width.
            int sideW = (hexPanel.Width - 6) / 2;
            int hx = OffsetColWidth;
            int ax = hx + BytesPerRow * HexCellWidth + GutterWidth;

            // Both sides together from the same _scrollOffset —
            // No separate scroll for Side B, both scroll synchronously

            for (int row = 0; row <= _visibleRows; row++)
            {
                long bi = _scrollOffset + row * BytesPerRow;
                int y = HeaderHeight + row * RowHeight;
                if (y + RowHeight < clip.Top || y > clip.Bottom) continue;

                // ── Side A (links) ────────────────────────────────────────────────
                if (bi < _data.Length)
                {
                    // Clip to the left half so that rows don't extend into the right side.
                    var stateA = g.Save();
                    g.SetClip(new Rectangle(0, 0, sideW, hexPanel.Height));
                    DrawRow(g, row, bi, y, hx, ax, _data, _diffData);
                    g.Restore(stateA);
                }

                // ── Side B (rechts) ───────────────────────────────────────────────
                if (bi < _diffData.Length)
                {
                    int offsetX = sideW + 6;
                    var stateB = g.Save();
                    g.TranslateTransform(offsetX, 0);
                    g.SetClip(new Rectangle(0, 0, sideW, hexPanel.Height));
                    DrawRow(g, row, bi, y, hx, ax, _diffData, _data);
                    g.Restore(stateB);
                }
            }

            // ── dividing line ────────────────────────────────────────────────────────
            using (var dp = new Pen(Color.FromArgb(80, 80, 100), 2f))
                g.DrawLine(dp, sideW + 3, 0, sideW + 3, hexPanel.Height);

            // ── labels ────────────────────────────────────────────────────────────
            using var lb = new SolidBrush(Color.FromArgb(140, 190, 255));
            string nA = _sourceFile != null ? Path.GetFileName(_sourceFile) : "Buffer A";
            string nB = _diffLabel != "" ? _diffLabel : "Buffer B";

            // Show length difference
            string lenInfo = _data.Length != _diffData.Length
                ? $"  ⚠ A={_data.Length:N0} B={_diffData.Length:N0} bytes"
                : $"  both {_data.Length:N0} bytes";

            g.DrawString($"A: {nA}{lenInfo}", _labelFont, lb, new PointF(hx, 5));
            g.DrawString($"B: {nB}", _labelFont, lb,
                new PointF(sideW + 6 + hx, 5));

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

            // ── Write mode nibble input ───────────────────────────────────────
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
                        hexPanel.Invalidate();
                        UpdateStatus();
                    }
                    e.Handled = true;
                    return;
                }
                _nibbleMode = false;
            }

            // ── Ctrl + Arrow keys: Field-aware navigation ─────────────────────
            if (ctrl && (e.KeyCode == Keys.Right || e.KeyCode == Keys.Left ||
                         e.KeyCode == Keys.Down || e.KeyCode == Keys.Up))
            {
                long oldPos = _cursor;

                switch (e.KeyCode)
                {
                    case Keys.Right:
                        {
                            var field = ParseFieldAtCursor(_cursor);
                            if (field != null)
                                _cursor = Math.Min(field.Offset + field.Size, _data.Length - 1);
                            else
                                _cursor = Math.Min(_cursor + 1, _data.Length - 1);
                            break;
                        }
                    case Keys.Left:
                        {
                            long probe = Math.Max(_cursor - 1, 0);
                            var field = ParseFieldAtCursor(probe);
                            if (field != null)
                                _cursor = field.Offset;
                            else
                                _cursor = probe;
                            break;
                        }
                    case Keys.Down:
                        {
                            long pos = _cursor;
                            for (int i = 0; i < 4; i++)
                            {
                                var field = ParseFieldAtCursor(pos);
                                if (field != null)
                                    pos = Math.Min(field.Offset + field.Size, _data.Length - 1);
                                else
                                    pos = Math.Min(pos + 1, _data.Length - 1);
                                if (pos >= _data.Length - 1) break;
                            }
                            _cursor = pos;
                            break;
                        }
                    case Keys.Up:
                        {
                            long pos = _cursor;
                            for (int i = 0; i < 4; i++)
                            {
                                long probe = Math.Max(pos - 1, 0);
                                var field = ParseFieldAtCursor(probe);
                                if (field != null)
                                    pos = field.Offset;
                                else
                                    pos = probe;
                                if (pos <= 0) break;
                            }
                            _cursor = pos;
                            break;
                        }
                }

                if (shift)
                {
                    if (_selAnchor < 0) _selAnchor = oldPos;
                    _selStart = _selAnchor;
                    _selEnd = _cursor;
                }
                else
                {
                    _selAnchor = -1;
                    _selStart = _cursor;
                    _selEnd = _cursor;
                }

                EnsureCursorVisible();
                hexPanel.Invalidate();
                UpdateStatus();
                e.Handled = true;
                return;
            }

            // ── Normal navigation & commands ──────────────────────────────────
            long old = _cursor;

            switch (e.KeyCode)
            {
                case Keys.Right: _cursor = Math.Min(_cursor + 1, _data.Length - 1); break;
                case Keys.Left: _cursor = Math.Max(_cursor - 1, 0); break;
                case Keys.Down: _cursor = Math.Min(_cursor + BytesPerRow, _data.Length - 1); break;
                case Keys.Up: _cursor = Math.Max(_cursor - BytesPerRow, 0); break;
                case Keys.Home: _cursor = ctrl ? 0 : (_cursor / BytesPerRow) * BytesPerRow; break;
                case Keys.End:
                    _cursor = ctrl
                        ? _data.Length - 1
                        : Math.Min((_cursor / BytesPerRow + 1) * BytesPerRow - 1, _data.Length - 1);
                    break;
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
                case Keys.K when ctrl && shift:
                    _compareFrameA = -1;
                    lblStatus.Text = "Frame A selection cleared.";
                    e.Handled = true;
                    return;
                case Keys.K when ctrl: ShowFrameCompare(); e.Handled = true; return;
                case Keys.R when ctrl: ShowRleView(); e.Handled = true; return;
                case Keys.N when ctrl: JumpToNextFrame(); e.Handled = true; return;
                case Keys.Oemcomma when ctrl: JumpToPrevFrame(); e.Handled = true; return;
                case Keys.Z when ctrl && !shift: Undo(); e.Handled = true; return;
                case Keys.Z when ctrl && shift: Redo(); e.Handled = true; return;
                case Keys.Y when ctrl: Redo(); e.Handled = true; return;
                case Keys.W when ctrl: ToggleWriteMode(); e.Handled = true; return;
                case Keys.D when ctrl: OpenDiffDialog(); e.Handled = true; return;
                case Keys.P when ctrl: ShowPalettePreview(); e.Handled = true; return;
                case Keys.S when ctrl: ShowStructureTree(); e.Handled = true; return;
                case Keys.F1: ShowCommandOverview(); e.Handled = true; return;
                case Keys.F3: FindNext(_lastSearchHit + 1); e.Handled = true; return;
                case Keys.D1 when ctrl: PinAsA(); e.Handled = true; return;
                case Keys.D2 when ctrl: PinAsB(); e.Handled = true; return;

                default: return;
            }

            if (shift)
            {
                if (_selAnchor < 0) _selAnchor = old;
                _selStart = _selAnchor;
                _selEnd = _cursor;
            }
            else
            {
                _selAnchor = -1;
                _selStart = _cursor;
                _selEnd = _cursor;
            }

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
        //  FRAME NAVIGATION & COMPARE & RLE
        // ══════════════════════════════════════════════════════════════════════

        private void JumpToNextFrame()
        {
            if (_data == null) return;
            long[] headers = GetAllFrameHeaderOffsets();
            if (headers.Length == 0) return;
            long next = headers.FirstOrDefault(h => h > _cursor);
            if (next == 0) next = headers[0];
            _cursor = next;
            _selStart = next; _selEnd = next; _selAnchor = next;
            EnsureCursorVisible();
            hexPanel.Invalidate();
            UpdateStatus();
            lblStatus.Text += $"  │  Frame header at 0x{(_dataOffset + next):X8}";
        }

        private void JumpToPrevFrame()
        {
            if (_data == null) return;
            long[] headers = GetAllFrameHeaderOffsets();
            if (headers.Length == 0) return;

            // Last header located BEFORE the cursor
            long prev = headers.LastOrDefault(h => h < _cursor);
            if (prev == 0 && headers[headers.Length - 1] < _cursor)
                prev = headers[headers.Length - 1];
            else if (prev == 0)
                prev = headers[headers.Length - 1]; // wrap around

            _cursor = prev;
            _selStart = prev; _selEnd = prev; _selAnchor = prev;
            EnsureCursorVisible();
            hexPanel.Invalidate();
            UpdateStatus();
            lblStatus.Text += $"  │  Frame header at 0x{(_dataOffset + prev):X8}";
        }       

        private long[] GetAllFrameHeaderOffsets()
        {
            if (_data == null) return Array.Empty<long>();
            var result = new List<long>();

            if (_isUop)
            {
                if (_data.Length < 12) return Array.Empty<long>();
                int fc = (int)ReadUInt32(_data, 8);
                fc = Math.Min(fc, 256);
                for (int i = 0; i < fc; i++)
                {
                    long lp = 24 + i * 12;
                    if (lp + 4 > _data.Length) break;
                    int fOff = (int)ReadUInt32(_data, lp);
                    if (fOff > 0 && fOff < _data.Length)
                        result.Add(fOff);
                }
            }
            else
            {
                if (_data.Length < 514) return Array.Empty<long>();

                ushort fc = (ushort)(_data[512] | (_data[513] << 8));
                fc = Math.Min(fc, (ushort)256);
                
                long minValid = 514 + fc * 4;
                long lastOff = -1;

                for (int i = 0; i < fc; i++)
                {
                    long lp = 514 + i * 4;
                    if (lp + 4 > _data.Length) break;

                    int relOff = _data[lp + 2] | (_data[lp + 3] << 8);
                    long absOff = 512 + relOff;
                    
                    if (absOff < minValid) break;
                    
                    if (absOff <= lastOff) break;
                    
                    if (absOff + 8 > _data.Length) break;
                    
                    ushort w = (ushort)(_data[absOff + 4] | (_data[absOff + 5] << 8));
                    ushort h = (ushort)(_data[absOff + 6] | (_data[absOff + 7] << 8));
                    if (w > 2048 || h > 2048) break;

                    result.Add(absOff);
                    lastOff = absOff;
                }
            }

            return result.ToArray();
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
            Cm(m, "⚖  Compare two frames  Ctrl+K", (s, e) => ShowFrameCompare());
            Cm(m, "📺  RLE Decoder View  Ctrl+R", (s, e) => ShowRleView());
            Cm(m, "⏭  Jump to next frame  Ctrl+N", (s, e) => JumpToNextFrame());
            Cm(m, "⏮  Jump to prev frame  Ctrl+,", (s, e) => JumpToPrevFrame());
            m.Items.Add(new ToolStripSeparator());
            Cm(m, "📌  Pin as A  Ctrl+1", (s, e) => PinAsA());
            Cm(m, "📌  Pin as B  Ctrl+2", (s, e) => PinAsB());
            Cm(m, "❓  Command Overview  F1", (s, e) => ShowCommandOverview());
            m.Items.Add(new ToolStripSeparator());
            var debugItem = new ToolStripMenuItem("🛠 Debug: Copy RLE information (clipboard)");
            debugItem.Click += (s, e) => CopyRleDebugInfoToClipboard();
            debugItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.D;
            debugItem.ShowShortcutKeys = true;
            m.Items.Add(debugItem);

            m.Show(hexPanel, loc);

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

            // ── Backpropagation: Reinterpret field and fire callback ─
            PropagateByteChangeToModel(offset);
        }

        /// <summary>
        /// Notifies the AnimationEditForm that a byte has changed.
        /// Called as an event or delegate.
        /// </summary>
        public event Action<long, byte> OnByteWritten;

        private void PropagateByteChangeToModel(long offset)
        {
            OnByteWritten?.Invoke(_dataOffset + offset, _data[offset]);
        }

        private void PinAsA()
        {
            var buf = BuildCompareBuffer();
            if (buf == null) { lblStatus.Text = "Pin A: no data to pin."; return; }
            OnPinAsA?.Invoke(buf);
            lblStatus.Text = $"Pinned as A: {buf.Label}  —  open Compare to view.";
        }

        private void PinAsB()
        {
            var buf = BuildCompareBuffer();
            if (buf == null) { lblStatus.Text = "Pin B: no data to pin."; return; }
            OnPinAsB?.Invoke(buf);
            lblStatus.Text = $"Pinned as B: {buf.Label}  —  open Compare to view.";
        }

        private HexCompareBuffer BuildCompareBuffer()
        {
            if (_data == null) return null;
            return new HexCompareBuffer
            {
                Data = (byte[])_data.Clone(),
                FileOffset = _dataOffset,
                FilePath = _sourceFile,
                BodyId = _bodyId,
                ActionId = _actionId,
                DirectionId = _directionId,
                IsUop = _isUop,
                Preview = picPreview.Image as System.Drawing.Bitmap
            };
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

        private void ShowFrameCompare()
        {
            if (_data == null)
            {
                lblStatus.Text = "Frame Compare: no data loaded.";
                return;
            }

            long[] headers = GetAllFrameHeaderOffsets();

            // Determine the current frame index
            int currentFrame = 0;
            for (int i = headers.Length - 1; i >= 0; i--)
                if (headers[i] <= _cursor) { currentFrame = i; break; }

            // Erster Druck: Frame A setzen
            if (_compareFrameA < 0)
            {
                if (headers.Length == 0)
                {
                    lblStatus.Text = "Frame Compare: no frame headers found in this data.";
                    return;
                }

                _compareFrameA = currentFrame;
                lblStatus.Text =
                    $"Frame A marked: Frame {_compareFrameA}" +
                    $" @ 0x{(_dataOffset + headers[_compareFrameA]):X8}" +
                    $"  —  navigate to Frame B and press Ctrl+K again." +
                    $"  Ctrl+Shift+K to cancel.";
                hexPanel.Invalidate();
                return;
            }

            // Second print: compare
            if (headers.Length < 2)
            {
                lblStatus.Text = "Frame Compare: only one frame available, cannot compare.";
                _compareFrameA = -1;
                return;
            }

            int idxA = _compareFrameA;
            int idxB = currentFrame;
            _compareFrameA = -1;

            // Same frame → take the next one
            if (idxA == idxB)
                idxB = (idxA + 1) % headers.Length;

            long offA = headers[idxA];
            long offB = headers[idxB];
            int lenA = GetFrameLength(idxA, headers);
            int lenB = GetFrameLength(idxB, headers);

            var compareForm = new Form
            {
                Text = $"Frame Compare  —  Frame {idxA} vs Frame {idxB}",
                Size = new Size(1100, 600),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(28, 28, 32),
                ForeColor = Color.White
            };

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                BackColor = Color.FromArgb(28, 28, 32),
                Panel1MinSize = 200,
                Panel2MinSize = 200
            };

            FillFrameComparePanel(split.Panel1, idxA, offA, lenA,
                $"Frame {idxA}  @ 0x{(_dataOffset + offA):X8}  ({lenA} B)  ← A");
            FillFrameComparePanel(split.Panel2, idxB, offB, lenB,
                $"Frame {idxB}  @ 0x{(_dataOffset + offB):X8}  ({lenB} B)  ← B");

            compareForm.Controls.Add(split);
            compareForm.ShowDialog(this);
        }

        private int GetFrameLength(int frameIndex, long[] headers)
        {
            if (_isUop)
            {
                long lp = 24 + frameIndex * 12;
                if (lp + 8 > _data.Length) return 0;
                return (int)ReadUInt32(_data, lp + 4); // DataLength aus Frame-Table
            }
            else
            {
                if (frameIndex + 1 < headers.Length)
                    return (int)(headers[frameIndex + 1] - headers[frameIndex]);
                return (int)(_data.Length - headers[frameIndex]);
            }
        }

        /// <summary>
        /// Populates the specified panel with a labeled hex dump and header information for a frame segment.
        /// </summary>
        /// <param name="panel">The panel to populate with the frame comparison content.</param>
        /// <param name="frameIdx">The index of the frame to display.</param>
        /// <param name="offset">The byte offset of the frame data within the source.</param>
        /// <param name="length">The length, in bytes, of the frame data to display.</param>
        /// <param name="title">The title to display above the frame data.</param>
        private void FillFrameComparePanel(SplitterPanel panel, int frameIdx,
            long offset, int length, string title)
        {
            var lbl = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 22,
                ForeColor = Color.FromArgb(140, 190, 255),
                Font = new Font("Consolas", 8.5f),
                BackColor = Color.FromArgb(38, 38, 50),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 0, 0)
            };

            var txt = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(28, 28, 38),
                ForeColor = Color.FromArgb(200, 220, 255),
                Font = new Font("Consolas", 8.5f),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Both,
                WordWrap = false
            };

            // Build a hex dump of the frame
            var sb = new StringBuilder();

            // Decoding header fields
            if (offset + 8 <= _data.Length)
            {
                short cx = (short)(_data[offset] | (_data[offset + 1] << 8));
                short cy = (short)(_data[offset + 2] | (_data[offset + 3] << 8));
                ushort w = (ushort)(_data[offset + 4] | (_data[offset + 5] << 8));
                ushort h = (ushort)(_data[offset + 6] | (_data[offset + 7] << 8));
                sb.AppendLine($"CenterX : {cx,6}  (0x{(ushort)cx:X4})");
                sb.AppendLine($"CenterY : {cy,6}  (0x{(ushort)cy:X4})");
                sb.AppendLine($"Width   : {w,6}  (0x{w:X4})");
                sb.AppendLine($"Height  : {h,6}  (0x{h:X4})");
                sb.AppendLine($"PixelData: {Math.Max(0, length - 8)} bytes");
                sb.AppendLine(new string('─', 42));
            }

            // Hex dump (first 256 bytes of the frame)
            int dumpLen = Math.Min(length, 256);
            for (int row = 0; row < (dumpLen + 15) / 16; row++)
            {
                sb.Append($"0x{(_dataOffset + offset + row * 16):X8}  ");
                for (int col = 0; col < 16; col++)
                {
                    long bi = offset + row * 16 + col;
                    if (bi - offset < dumpLen && bi < _data.Length)
                        sb.Append($"{_data[bi]:X2} ");
                    else
                        sb.Append("   ");
                    if (col == 7) sb.Append(' ');
                }
                sb.Append("  ");
                for (int col = 0; col < 16; col++)
                {
                    long bi = offset + row * 16 + col;
                    if (bi - offset < dumpLen && bi < _data.Length)
                    {
                        byte b = _data[bi];
                        sb.Append(b >= 32 && b < 127 ? (char)b : '.');
                    }
                }
                sb.AppendLine();
            }
            if (length > 256)
                sb.AppendLine($"  … {length - 256} more bytes");

            txt.Text = sb.ToString();

            panel.Controls.Add(txt);
            panel.Controls.Add(lbl);
        }

        #region [ ShowRLEView: RLE Decoder View for the current frame ]

        // ────────────────────────────────────────────────────────────────────
        // Entry point — koordiniert nur, tut selbst nichts
        // ────────────────────────────────────────────────────────────────────
        private void ShowRleView()
        {
            if (_data == null) { lblStatus.Text = "RLE View: no data loaded."; return; }

            long[] headers = GetAllFrameHeaderOffsets();
            if (headers.Length == 0)
            { lblStatus.Text = "RLE View: no frame headers found."; return; }

            int frameIdx = RleResolveFrameIndex(headers);
            long frameOff = headers[frameIdx];
            int frameLen = GetFrameLength(frameIdx, headers);

            if (frameOff + 8 > _data.Length)
            { lblStatus.Text = "RLE View: frame data too short."; return; }

            var pal = RleBuildPalette();
            var decoded = RleDecodeFrame(frameOff, frameLen, pal);
            var diagTxt = RleBuildDiagnostic(headers, frameIdx, frameOff, decoded);

            RleShowWindow(frameIdx, frameOff, decoded, diagTxt);
        }

        // ────────────────────────────────────────────────────────────────────
        // Step 1 — welcher Frame-Index?
        // ────────────────────────────────────────────────────────────────────
        private int RleResolveFrameIndex(long[] headers)
        {
            if (_frameId >= 0 && _frameId < headers.Length)
                return _frameId;
            int fallback = 0;
            for (int i = headers.Length - 1; i >= 0; i--)
                if (headers[i] <= _cursor) { fallback = i; break; }
            return fallback;
        }

        // ────────────────────────────────────────────────────────────────────
        // Step 2 — Palette aus _data[0..511] lesen
        // UO MUL ARGB1555: Bits 14-10 = R, 9-5 = G, 4-0 = B
        // Bestätigt durch Debug-Log: 0x0400 -> R=8, G=0, B=0
        // ────────────────────────────────────────────────────────────────────
        private Color[] RleBuildPalette()
        {
            var pal = new Color[256];
            for (int i = 0; i < 256; i++)
            {
                long p = i * 2;
                if (p + 1 >= _data.Length) break;
                ushort v = (ushort)(_data[p] | (_data[p + 1] << 8));
                int r = ((v >> 10) & 0x1F) * 255 / 31;
                int g = ((v >> 5) & 0x1F) * 255 / 31;
                int b = (v & 0x1F) * 255 / 31;
                pal[i] = (i == 0 || (r == 0 && g == 0 && b == 0))
                    ? Color.Transparent
                    : Color.FromArgb(255, r, g, b);
            }
            return pal;
        }

        // ────────────────────────────────────────────────────────────────────
        // Step 3 — RLE-Stream dekodieren -> pixBuf + Run-Text
        // UO MUL uint32 Run-Header (LE):
        //   Bits  0-11 : runLength
        //   Bits 12-21 : yRel  (signed 10-bit, relativ zu CenterY+Height)
        //   Bits 22-31 : xRel  (signed 10-bit, relativ zu CenterX)
        // Ende-Marker: 0x7FFF7FFF
        // ────────────────────────────────────────────────────────────────────
        private struct RleDecoded
        {
            public short Cx, Cy;
            public ushort W, H;
            public int Pw, Ph;
            public uint[] PixBuf;
            public string RunText;
            public int TotalRuns, TotalPixels;
        }

        private RleDecoded RleDecodeFrame(long frameOff, int frameLen, Color[] pal)
        {
            short cx = (short)(_data[frameOff + 0] | (_data[frameOff + 1] << 8));
            short cy = (short)(_data[frameOff + 2] | (_data[frameOff + 3] << 8));
            ushort w = (ushort)(_data[frameOff + 4] | (_data[frameOff + 5] << 8));
            ushort h = (ushort)(_data[frameOff + 6] | (_data[frameOff + 7] << 8));

            int pw = Math.Min((int)w, 512);
            int ph = Math.Min((int)h, 512);
            var pixBuf = new uint[Math.Max(1, ph) * Math.Max(1, pw)];

            var sb = new StringBuilder();
            long pos = frameOff + 8;
            long end = frameOff + frameLen;
            int totalRuns = 0;
            int totalPixels = 0;

            sb.AppendLine($"Frame  CX={cx}  CY={cy}  W={w}  H={h}");
            sb.AppendLine($"Pixel data: {Math.Max(0, frameLen - 8)} bytes");
            sb.AppendLine(new string('─', 72));

            while (pos + 3 < end && pos + 3 < _data.Length)
            {
                uint hdr = (uint)(_data[pos]
                         | (_data[pos + 1] << 8)
                         | (_data[pos + 2] << 16)
                         | (_data[pos + 3] << 24));
                pos += 4;

                if (hdr == 0x7FFF7FFF) break;

                int runLen = (int)(hdr & 0x0FFF);

                int xRel = (int)((hdr >> 22) & 0x03FF);
                if ((xRel & 0x0200) != 0) xRel |= unchecked((int)0xFFFFFE00);

                int yRel = (int)((hdr >> 12) & 0x03FF);
                if ((yRel & 0x0200) != 0) yRel |= unchecked((int)0xFFFFFE00);

                int xAbs = xRel + cx;
                int yAbs = yRel + cy + h;

                sb.Append($"  Run {totalRuns,4}: y={yAbs,4} x={xAbs,4} len={runLen,4}  idx=");

                for (int k = 0; k < runLen; k++)
                {
                    if (pos >= end || pos >= _data.Length) break;
                    byte palIdx = _data[pos++];

                    if (k < 8) sb.Append($"{palIdx:D3} ");
                    else if (k == 8) sb.Append("...");

                    if (yAbs >= 0 && yAbs < ph && xAbs + k >= 0 && xAbs + k < pw)
                    {
                        var col = pal[palIdx];
                        if (col.A > 0)
                            pixBuf[yAbs * pw + xAbs + k] =
                                (uint)((col.A << 24) | (col.R << 16) | (col.G << 8) | col.B);
                    }
                    totalPixels++;
                }

                sb.AppendLine();
                totalRuns++;
                if (totalRuns > 2000) { sb.AppendLine("  … truncated at 2000 runs"); break; }
            }

            sb.AppendLine(new string('─', 72));
            sb.AppendLine($"Total: {totalRuns} runs  {totalPixels} pixels");
            if (w > 0 && h > 0)
            {
                double cov = (double)totalPixels / (w * h) * 100.0;
                sb.AppendLine($"Coverage: {cov:F1}%  " +
                    (cov > 50 ? "OK" : cov > 10 ? "sparse" : "possible decode error"));
            }

            return new RleDecoded
            {
                Cx = cx,
                Cy = cy,
                W = w,
                H = h,
                Pw = pw,
                Ph = ph,
                PixBuf = pixBuf,
                RunText = sb.ToString(),
                TotalRuns = totalRuns,
                TotalPixels = totalPixels
            };
        }

        // ────────────────────────────────────────────────────────────────────
        // Step 4 — Diagnose-Text aufbauen
        // Enthält Palette-Probe: wenn die Farben hier nicht zum Body passen,
        // ist _data der falsche Block.
        // ────────────────────────────────────────────────────────────────────
        private string RleBuildDiagnostic(long[] headers, int frameIdx,
            long frameOff, RleDecoded d)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Body:" + _bodyId + "  Action:" + _actionId
                        + "  Dir:" + _directionId + "  _frameId=" + _frameId
                        + "  (= FramesTrackBar.Value)");
            sb.AppendLine("_data.Length=" + _data.Length.ToString("N0")
                        + "  _dataOffset=0x" + _dataOffset.ToString("X8"));

            // Palette-Probe: erste 8 Farben zeigen ob der richtige Block geladen ist
            sb.Append("Palette[1..8]: ");
            for (int i = 1; i <= 8 && i * 2 + 1 < _data.Length; i++)
            {
                ushort v = (ushort)(_data[i * 2] | (_data[i * 2 + 1] << 8));
                int r = ((v >> 10) & 0x1F) * 255 / 31;
                int g = ((v >> 5) & 0x1F) * 255 / 31;
                int bv = (v & 0x1F) * 255 / 31;
                sb.Append("[" + i + "]R=" + r + " G=" + g + " B=" + bv + "  ");
            }
            sb.AppendLine();

            // Frame-Headers Liste
            sb.AppendLine("Headers (" + headers.Length + "):");
            for (int i = 0; i < headers.Length; i++)
            {
                long off = headers[i];
                short hCx = (short)(_data[off + 0] | (_data[off + 1] << 8));
                short hCy = (short)(_data[off + 2] | (_data[off + 3] << 8));
                ushort hW = (ushort)(_data[off + 4] | (_data[off + 5] << 8));
                ushort hH = (ushort)(_data[off + 6] | (_data[off + 7] << 8));
                string mark = (i == frameIdx) ? " <<< DECODING" : "";
                sb.AppendLine("  [" + i + "] 0x" + off.ToString("X6")
                            + "  CX=" + hCx + " CY=" + hCy
                            + " W=" + hW + " H=" + hH + mark);
            }

            sb.AppendLine("frameIdx=" + frameIdx
                        + "  frameOff=0x" + frameOff.ToString("X6")
                        + "  abs=0x" + (_dataOffset + frameOff).ToString("X8"));
            sb.AppendLine("Decoded: CX=" + d.Cx + " CY=" + d.Cy
                        + " W=" + d.W + " H=" + d.H
                        + "  Runs=" + d.TotalRuns + "  Pixels=" + d.TotalPixels);
            return sb.ToString();
        }

        // ────────────────────────────────────────────────────────────────────
        // Step 5 — Fenster anzeigen (dein altes Design + Toggle-Button)
        // ────────────────────────────────────────────────────────────────────
        private void RleShowWindow(int frameIdx, long frameOff,
            RleDecoded d, string diagText)
        {
            int pw = d.Pw, ph = d.Ph;
            var pixBuf = d.PixBuf;

            var frm = new Form
            {
                Text = $"RLE Decoder — Frame {frameIdx}  ({d.W}×{d.H})" +
                       $"  @ 0x{(_dataOffset + frameOff):X8}",
                Size = new Size(1020, 820),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(28, 28, 32),
                ForeColor = Color.White,
                MinimumSize = new Size(600, 500)
            };

            // ── Toggle-Button ─────────────────────────────────────────────────
            var btnToggle = new Button
            {
                Text = "Diagnose einblenden",
                Dock = DockStyle.Top,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 90, 150),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f)
            };
            btnToggle.FlatAppearance.BorderColor = Color.FromArgb(0, 130, 200);

            // ── Diagnose-Panel (anfangs versteckt) ────────────────────────────
            var diagPanel = new TextBox
            {
                Dock = DockStyle.Top,
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(20, 40, 20),
                ForeColor = Color.FromArgb(100, 255, 120),
                Font = new Font("Consolas", 8.5f),
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Vertical,
                WordWrap = false,
                Text = diagText,
                Height = Math.Min((diagText.Split('\n').Length + 1) * 16, 280),
                Visible = false
            };

            bool diagVisible = false;
            btnToggle.Click += (s, e) =>
            {
                diagVisible = !diagVisible;
                diagPanel.Visible = diagVisible;
                btnToggle.Text = diagVisible ? "Diagnose ausblenden" : "Diagnose einblenden";
                btnToggle.BackColor = diagVisible
                    ? Color.FromArgb(0, 120, 60)
                    : Color.FromArgb(0, 90, 150);
            };

            // ── Split: links Vorschau, rechts Run-Text ────────────────────────
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 350,
                SplitterWidth = 6
            };

            // ── Vorschau (links) ──────────────────────────────────────────────
            var previewPanel = new HexPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(28, 28, 38)
            };

            previewPanel.Paint += (s, e) =>
            {
                var g2 = e.Graphics;
                g2.Clear(Color.FromArgb(28, 28, 38));
                if (pw <= 0 || ph <= 0) return;

                int panW = previewPanel.ClientSize.Width - 16;
                int panH = previewPanel.ClientSize.Height - 40;
                float scale = Math.Max(1f, Math.Min((float)panW / pw, (float)panH / ph));
                int drawW = (int)(pw * scale);
                int drawH = (int)(ph * scale);
                int drawX = (previewPanel.ClientSize.Width - drawW) / 2;
                int drawY = 28 + (panH - drawH) / 2;

                using var tb1 = new SolidBrush(Color.FromArgb(50, 50, 60));
                using var tb2 = new SolidBrush(Color.FromArgb(70, 70, 80));
                int cs = Math.Max(4, (int)scale);
                for (int py = 0; py < drawH; py += cs)
                    for (int px2 = 0; px2 < drawW; px2 += cs)
                        g2.FillRectangle(((px2 / cs + py / cs) % 2 == 0) ? tb1 : tb2,
                            drawX + px2, drawY + py,
                            Math.Min(cs, drawW - px2), Math.Min(cs, drawH - py));

                g2.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g2.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                for (int py = 0; py < ph; py++)
                    for (int px2 = 0; px2 < pw; px2++)
                    {
                        uint val = pixBuf[py * pw + px2];
                        if (val == 0) continue;
                        int ra = (int)((val >> 24) & 0xFF);
                        int rr = (int)((val >> 16) & 0xFF);
                        int rg = (int)((val >> 8) & 0xFF);
                        int rb = (int)(val & 0xFF);
                        if (ra == 0) continue;
                        using var pb = new SolidBrush(Color.FromArgb(ra, rr, rg, rb));
                        g2.FillRectangle(pb,
                            drawX + (int)(px2 * scale),
                            drawY + (int)(py * scale),
                            Math.Max(1, (int)scale),
                            Math.Max(1, (int)scale));
                    }

                using var ib = new SolidBrush(Color.FromArgb(140, 190, 255));
                g2.DrawString(
                    $"Frame {frameIdx}  {d.W}×{d.H}  CX={d.Cx} CY={d.Cy}  (scale {scale:F1}×)",
                    new Font("Consolas", 8f), ib, new PointF(4, 4));
            };

            previewPanel.Resize += (s, e) => previewPanel.Invalidate();

            // ── Run-Text (rechts) ─────────────────────────────────────────────
            var txtBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(28, 28, 38),
                ForeColor = Color.FromArgb(200, 220, 255),
                Font = new Font("Consolas", 8.5f),
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Text = d.RunText
            };

            split.Panel1.Controls.Add(previewPanel);
            split.Panel2.Controls.Add(txtBox);

            // Dock-Reihenfolge: Fill zuerst, dann Top (umgekehrte Add-Reihenfolge)
            frm.Controls.Add(split);
            frm.Controls.Add(diagPanel);
            frm.Controls.Add(btnToggle);

            frm.ShowDialog(this);
        }

        #endregion


        #region [ CopyRleDebugInfoToClipboard() : Copy detailed RLE debug information to clipboard ]
        // ══════════════════════════════════════════════════════════════════════
        //  RLE Debug Info
        // ═════════════════════════════════════════════════════════════════════
        private void CopyRleDebugInfoToClipboard()
        {
            if (_data == null || _data.Length == 0)
            {
                lblStatus.Text = "No data — nothing to copy.";
                return;
            }

            _cursor = 0;
            _selStart = _selEnd = _selAnchor = -1;
            ScrollToOffset(0);
            hexPanel.Invalidate();

            var sb = new StringBuilder();
            sb.AppendLine("=== RLE Debug Info ===");
            sb.AppendLine($"Time:          {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"File:          {_sourceFile ?? "(memory)"}");
            sb.AppendLine($"Format:        {(_isUop ? "UOP" : "MUL")}");
            sb.AppendLine($"Body:          {_bodyId}   Action: {_actionId}   Dir: {_directionId}" +
                          (_isUop ? $"   Seq: {_sequenceId}" : $"   Frame: {_frameId}"));
            sb.AppendLine($"Block size:    {_data.Length:N0} bytes  (0x{_data.Length:X})");
            sb.AppendLine($"DataOffset:    0x{_dataOffset:X8}  (absolute file pos of block start)");
            sb.AppendLine($"IsUop:         {_isUop}");
            sb.AppendLine();

            // ── Erste 128 Bytes Hex + ASCII ───────────────────────────────────────
            sb.AppendLine("First 128 bytes (Hex + ASCII):");
            int dumpLen = Math.Min(128, _data.Length);
            for (int i = 0; i < dumpLen; i += 16)
            {
                sb.Append($"  {i,4:X4}  ");
                for (int j = 0; j < 16; j++)
                {
                    if (i + j < dumpLen) sb.Append($"{_data[i + j]:X2} ");
                    else sb.Append("   ");
                    if (j == 7) sb.Append(" ");
                }
                sb.Append("  ");
                for (int j = 0; j < 16 && i + j < dumpLen; j++)
                {
                    byte b = _data[i + j];
                    sb.Append(b >= 32 && b < 127 ? (char)b : '.');
                }
                sb.AppendLine();
            }
            sb.AppendLine();

            // ── MUL spezifisch ────────────────────────────────────────────────────
            if (!_isUop && _data.Length >= 514)
            {
                ushort fc = (ushort)(_data[512] | (_data[513] << 8));
                long dataStart = 514 + (long)fc * 4;

                sb.AppendLine($"MUL — FrameCount @ 0x200-0x201 = {fc}");
                sb.AppendLine($"Lookup table   : offset 0x202..0x{(514 + fc * 4 - 1):X}  ({fc} × 4 bytes)");
                sb.AppendLine($"Data area start: 0x{dataStart:X} ({dataStart})");
                sb.AppendLine($"Block end      : 0x{_data.Length:X} ({_data.Length})");
                sb.AppendLine($"Remaining data : {_data.Length - dataStart} bytes");
                sb.AppendLine();

                // Palette erste 16 Einträge
                sb.AppendLine("Palette[0..15]:");
                for (int i = 0; i < Math.Min(16, 256); i++)
                {
                    long p = i * 2;
                    ushort v = (ushort)(_data[p] | (_data[p + 1] << 8));
                    int r5 = (v >> 10) & 0x1F;
                    int g5 = (v >> 5) & 0x1F;
                    int b5 = v & 0x1F;
                    sb.AppendLine($"  [{i:D3}]  0x{v:X4}  " +
                                  $"R={r5 * 255 / 31,3} G={g5 * 255 / 31,3} B={b5 * 255 / 31,3}  " +
                                  $"{((v & 0x8000) != 0 ? "opaque" : "transparent")}");
                }
                sb.AppendLine();

                // Lookup-Rohdaten
                sb.AppendLine($"Lookup table raw bytes (514..{514 + fc * 4 - 1}):");
                for (int i = 0; i < Math.Min((int)fc, 32); i++)
                {
                    long lp = 514 + i * 4;
                    if (lp + 4 > _data.Length) break;
                    byte b0 = _data[lp], b1 = _data[lp + 1],
                         b2 = _data[lp + 2], b3 = _data[lp + 3];
                    sb.AppendLine($"  [{i:D2}] @ 0x{lp:X4}  raw: {b0:X2} {b1:X2} {b2:X2} {b3:X2}");
                }
                sb.AppendLine();

                // Lookup-Tabelle — alle Interpretationen
                sb.AppendLine("Frame Lookup — alle Interpretationen + Header-Check:");
                sb.AppendLine("  Idx  raw            A=b2|b3<<8  B=b0|b1<<8  C=uint32LE    absA   absB   absC   hdrA                        hdrB");
                sb.AppendLine(new string('─', 160));

                for (int i = 0; i < Math.Min((int)fc, 32); i++)
                {
                    long lp = 514 + i * 4;
                    if (lp + 4 > _data.Length) break;
                    byte b0 = _data[lp], b1 = _data[lp + 1],
                         b2 = _data[lp + 2], b3 = _data[lp + 3];

                    int relA = b2 | (b3 << 8);
                    int relB = b0 | (b1 << 8);
                    int relC = b0 | (b1 << 8) | (b2 << 16) | (b3 << 24);

                    long absA = 512 + relA;
                    long absB = 512 + relB;
                    long absC = 512 + relC;

                    string HdrStr(long abs)
                    {
                        if (abs < 0 || abs + 8 > _data.Length) return "✗ OOB";
                        short cx = (short)(_data[abs] | (_data[abs + 1] << 8));
                        short cy = (short)(_data[abs + 2] | (_data[abs + 3] << 8));
                        ushort w = (ushort)(_data[abs + 4] | (_data[abs + 5] << 8));
                        ushort h = (ushort)(_data[abs + 6] | (_data[abs + 7] << 8));
                        bool ok = w > 0 && w <= 512 && h > 0 && h <= 512;
                        return $"CX={cx,5} CY={cy,5} W={w,4} H={h,4}{(ok ? " ✓" : " ?")}";
                    }

                    sb.AppendLine($"  [{i:D2}]  {b0:X2}{b1:X2}{b2:X2}{b3:X2}" +
                                  $"  A={relA,6}  B={relB,6}  C={relC,10}" +
                                  $"  {absA,6}  {absB,6}  {absC,10}" +
                                  $"  A:[{HdrStr(absA)}]" +
                                  $"  B:[{HdrStr(absB)}]");
                }
                if (fc > 32) sb.AppendLine($"  … {fc - 32} more entries");
                sb.AppendLine();

                // 16 Bytes an dataStart dumpen
                sb.AppendLine($"16 bytes at dataStart (0x{dataStart:X}):");
                if (dataStart + 16 <= _data.Length)
                {
                    sb.Append("  ");
                    for (int i = 0; i < 16; i++)
                        sb.Append($"{_data[dataStart + i]:X2} ");
                    sb.AppendLine();
                }
                sb.AppendLine();
            }
            else if (_isUop && _data.Length >= 12)
            {
                uint fc = ReadUInt32(_data, 8);
                sb.AppendLine($"UOP — FrameCount @ 0x08: {fc}");
                sb.AppendLine($"Frame table: offset 0x18, {fc} × 12 bytes");
                sb.AppendLine();
                for (int i = 0; i < Math.Min((int)fc, 32); i++)
                {
                    long lp = 24 + i * 12;
                    if (lp + 12 > _data.Length) break;
                    int fOff = (int)ReadUInt32(_data, lp);
                    int fLen = (int)ReadUInt32(_data, lp + 4);
                    int fExt = (int)ReadUInt32(_data, lp + 8);
                    bool ok = fOff > 0 && fOff < _data.Length;
                    string hdr = "";
                    if (ok && fOff + 8 <= _data.Length)
                    {
                        short cx = (short)(_data[fOff] | (_data[fOff + 1] << 8));
                        short cy = (short)(_data[fOff + 2] | (_data[fOff + 3] << 8));
                        ushort w = (ushort)(_data[fOff + 4] | (_data[fOff + 5] << 8));
                        ushort h = (ushort)(_data[fOff + 6] | (_data[fOff + 7] << 8));
                        hdr = $"  CX={cx,5} CY={cy,5} W={w,4} H={h,4}";
                    }
                    sb.AppendLine($"  Frame[{i,2}]  off=0x{fOff:X6}  len={fLen,6}  ext=0x{fExt:X8}" +
                                  $"  {(ok ? "✓" : "✗ OOB")}{hdr}");
                }
                if (fc > 32) sb.AppendLine($"  … {fc - 32} more entries");
                sb.AppendLine();
            }

            // ── Live Field Analysis ───────────────────────────────────────────────
            sb.AppendLine("Live field analysis (first 12 fields from offset 0):");
            long scanPos = 0;
            for (int fi = 0; fi < 12 && scanPos < _data.Length; fi++)
            {
                var field = ParseFieldAtCursor(scanPos);
                if (field == null) break;
                sb.AppendLine($"  0x{scanPos:X6}  {field.TypeName,-16} {field.Name,-32} = {field.Value}");
                long next = field.Offset + field.Size;
                if (next <= scanPos) break;
                scanPos = next;
            }
            sb.AppendLine();

            // ── GetAllFrameHeaderOffsets Ergebnis ─────────────────────────────────
            var headers = GetAllFrameHeaderOffsets();
            sb.AppendLine($"GetAllFrameHeaderOffsets() → {headers.Length} headers found:");
            for (int i = 0; i < Math.Min(headers.Length, 20); i++)
            {
                long h = headers[i];
                string hdr = "";
                if (h + 8 <= _data.Length)
                {
                    short cx = (short)(_data[h] | (_data[h + 1] << 8));
                    short cy = (short)(_data[h + 2] | (_data[h + 3] << 8));
                    ushort w = (ushort)(_data[h + 4] | (_data[h + 5] << 8));
                    ushort hh = (ushort)(_data[h + 6] | (_data[h + 7] << 8));
                    hdr = $"  CX={cx,5} CY={cy,5} W={w,4} H={hh,4}" +
                          (w > 0 && w <= 512 && hh > 0 && hh <= 512 ? "  ✓" : "  ?");
                }
                sb.AppendLine($"  [{i:D2}]  abs=0x{h:X6} ({h,6}){hdr}");
            }
            sb.AppendLine();

            // ── ShowRleView Frame-Auswahl Diagnose ────────────────────────────────────
            sb.AppendLine("=== ShowRleView Frame-Auswahl Diagnose ===");
            sb.AppendLine($"  _frameId  = {_frameId}   (von AnimationEditForm übergeben)");
            {
                bool frameIdOk = _frameId >= 0 && _frameId < headers.Length;
                int cursorFrame2 = 0;
                for (int i2 = headers.Length - 1; i2 >= 0; i2--)
                    if (headers[i2] <= _cursor) { cursorFrame2 = i2; break; }
                sb.AppendLine($"  cursor → frame = {cursorFrame2}");
                sb.AppendLine($"  _frameId gültig? {(frameIdOk ? "JA" : "NEIN")}  Range: 0..{headers.Length - 1}");
                sb.AppendLine($"  ShowRleView zeigt: Frame {(frameIdOk ? _frameId : cursorFrame2)}");
                if (frameIdOk)
                {
                    long fo2 = headers[_frameId];
                    if (fo2 + 8 <= _data.Length)
                    {
                        short fcx2 = (short)(_data[fo2] | (_data[fo2 + 1] << 8));
                        short fcy2 = (short)(_data[fo2 + 2] | (_data[fo2 + 3] << 8));
                        ushort fw2 = (ushort)(_data[fo2 + 4] | (_data[fo2 + 5] << 8));
                        ushort fh2 = (ushort)(_data[fo2 + 6] | (_data[fo2 + 7] << 8));
                        sb.AppendLine($"  Frame[{_frameId}] Header: CX={fcx2} CY={fcy2} W={fw2} H={fh2}");
                    }
                }
            }
            sb.AppendLine();


            // ═══════════════════════════════════════════════════════════════════════
            // ── NEU: Frame Pixel-Dump — RLE-Bit-Layout Analyse ───────────────────
            // ═══════════════════════════════════════════════════════════════════════
            sb.AppendLine("=== Frame Pixel-Dump (RLE Bit-Layout Analyse) ===");
            sb.AppendLine("Zeigt die ersten 16 RLE-Words nach dem 8-Byte-Header jedes Frames.");
            sb.AppendLine("Ziel: korrektes Bit-Layout bestimmen (count-Bits, xOffset-Bits, transparent-Flag).");
            sb.AppendLine();

            // Alle 4 möglichen Bit-Layouts die in UO-Animationen vorkommen:
            // Layout A: bit15=transp, bits14-12=count(3bit), bits11-0=xOffset(12bit)  [classic UO]
            // Layout B: bit15=transp, bits14-12=count(3bit), bits10-0=xOffset(11bit)  [variant]
            // Layout C: bit15=transp, bits14-11=count(4bit), bits10-0=xOffset(11bit)  [animX.mul]
            // Layout D: bit15=transp, bits11-8=count(4bit),  bits7-0=xOffset(8bit)    [compact]

            sb.AppendLine("Layout-Legende:");
            sb.AppendLine("  A: T | count(3) | xOff(12)   B: T | count(3) | xOff(11)");
            sb.AppendLine("  C: T | count(4) | xOff(11)   D: T | count(4) | xOff(8)");
            sb.AppendLine();

            int framesToDump = Math.Min(headers.Length, 5); // erste 5 Frames
            for (int fi = 0; fi < framesToDump; fi++)
            {
                long frameOff = headers[fi];
                if (frameOff + 8 >= _data.Length) continue;

                short fcx = (short)(_data[frameOff] | (_data[frameOff + 1] << 8));
                short fcy = (short)(_data[frameOff + 2] | (_data[frameOff + 3] << 8));
                ushort fw = (ushort)(_data[frameOff + 4] | (_data[frameOff + 5] << 8));
                ushort fh = (ushort)(_data[frameOff + 6] | (_data[frameOff + 7] << 8));

                sb.AppendLine($"── Frame {fi}  @ 0x{(frameOff):X6}  " +
                              $"CX={fcx} CY={fcy} W={fw} H={fh}  " +
                              $"(abs 0x{(_dataOffset + frameOff):X8})");
                sb.AppendLine($"   {"Word":>6}  {"Raw":>6}  " +
                              $"{"A:T|cnt3|x12":>14}  " +
                              $"{"B:T|cnt3|x11":>14}  " +
                              $"{"C:T|cnt4|x11":>14}  " +
                              $"{"D:T|cnt4|x8":>13}  " +
                              $"Bytes(hex)");
                sb.AppendLine("   " + new string('─', 100));

                long pos = frameOff + 8;
                long end = Math.Min(pos + 32, _data.Length - 1); // 16 Words = 32 Bytes
                int wordIdx = 0;

                while (pos + 1 <= end && wordIdx < 16)
                {
                    ushort word = (ushort)(_data[pos] | (_data[pos + 1] << 8));
                    byte lo = _data[pos];
                    byte hi = _data[pos + 1];

                    // Layout A: T=bit15, count=bits14-12 (3bit), xOff=bits11-0 (12bit)
                    bool tA = (word & 0x8000) != 0;
                    int cA = (word >> 12) & 0x07;
                    int xA = word & 0x0FFF;

                    // Layout B: T=bit15, count=bits14-12 (3bit), xOff=bits10-0 (11bit)
                    bool tB = (word & 0x8000) != 0;
                    int cB = (word >> 11) & 0x07;
                    int xB = word & 0x07FF;

                    // Layout C: T=bit15, count=bits14-11 (4bit), xOff=bits10-0 (11bit)
                    bool tC = (word & 0x8000) != 0;
                    int cC = (word >> 11) & 0x0F;
                    int xC = word & 0x07FF;

                    // Layout D: T=bit15, count=bits11-8 (4bit), xOff=bits7-0 (8bit)
                    bool tD = (word & 0x8000) != 0;
                    int cD = (word >> 8) & 0x0F;
                    int xD = word & 0x00FF;

                    // Plausibilität: xOff muss < Bildbreite, count muss > 0 (ausser EOL)
                    bool plausA = word == 0 || (xA < fw && cA <= fw);
                    bool plausB = word == 0 || (xB < fw && cB <= fw);
                    bool plausC = word == 0 || (xC < fw && cC <= fw);
                    bool plausD = word == 0 || (xD < fw && cD <= fw);

                    string FmtLayout(bool t, int cnt, int x, bool plaus) =>
                        $"{(t ? "T" : ".")}{cnt,2}|x={x,4}{(plaus ? " ✓" : " ✗")}";

                    sb.AppendLine($"   {wordIdx,5}  0x{word:X4}  " +
                                  $"{FmtLayout(tA, cA, xA, plausA),14}  " +
                                  $"{FmtLayout(tB, cB, xB, plausB),14}  " +
                                  $"{FmtLayout(tC, cC, xC, plausC),14}  " +
                                  $"{FmtLayout(tD, cD, xD, plausD),13}  " +
                                  $"{lo:X2} {hi:X2}");

                    pos += 2;
                    wordIdx++;
                }

                // Auswertung: welches Layout hat die meisten plausiblen Words?
                pos = frameOff + 8;
                end = Math.Min(pos + 32, _data.Length - 1);
                int okA = 0, okB = 0, okC = 0, okD = 0, total = 0;
                while (pos + 1 <= end)
                {
                    ushort word = (ushort)(_data[pos] | (_data[pos + 1] << 8));
                    if (word == 0) { pos += 2; total++; okA++; okB++; okC++; okD++; continue; }
                    if ((word & 0x8000) == 0) // opaque run
                    {
                        if (((word >> 12) & 0x07) > 0 && (word & 0x0FFF) < fw) okA++;
                        if (((word >> 11) & 0x07) > 0 && (word & 0x07FF) < fw) okB++;
                        if (((word >> 11) & 0x0F) > 0 && (word & 0x07FF) < fw) okC++;
                        if (((word >> 8) & 0x0F) > 0 && (word & 0x00FF) < fw) okD++;
                    }
                    else // transparent run
                    {
                        if ((word & 0x0FFF) < fw) okA++;
                        if ((word & 0x07FF) < fw) okB++;
                        if ((word & 0x07FF) < fw) okC++;
                        if ((word & 0x00FF) < fw) okD++;
                    }
                    pos += 2;
                    total++;
                }

                sb.AppendLine();
                sb.AppendLine($"   Plausibilität (xOff < W={fw}, count > 0) über {total} Words:");
                sb.AppendLine($"   Layout A (T|cnt3|x12): {okA}/{total}  " +
                              $"B (T|cnt3|x11): {okB}/{total}  " +
                              $"C (T|cnt4|x11): {okC}/{total}  " +
                              $"D (T|cnt4|x8):  {okD}/{total}");

                int best = Math.Max(okA, Math.Max(okB, Math.Max(okC, okD)));
                string bestName = okA == best ? "A" :
                                  okB == best ? "B" :
                                  okC == best ? "C" : "D";
                sb.AppendLine($"   → Wahrscheinlichstes Layout: {bestName}");
                sb.AppendLine();
            }

            // ── Rohbytes der ersten 64 Bytes nach Frame-0-Header ─────────────────
            if (headers.Length > 0)
            {
                long f0 = headers[0];
                long pixStart = f0 + 8;
                int rawCount = (int)Math.Min(64, _data.Length - pixStart);
                sb.AppendLine($"Raw pixel bytes Frame 0 (0x{pixStart:X6}, erste {rawCount} Bytes):");
                sb.Append("  ");
                for (int i = 0; i < rawCount; i++)
                {
                    sb.Append($"{_data[pixStart + i]:X2} ");
                    if ((i + 1) % 16 == 0) sb.Append("\n  ");
                }
                sb.AppendLine();
                sb.AppendLine();
            }
            // ═══════════════════════════════════════════════════════════════════════

            sb.AppendLine("=== End ===");

            string text = sb.ToString();

            bool copied = false;
            try { Clipboard.SetText(text); copied = true; }
            catch { /* ignore */ }

            var frm = new Form
            {
                Text = $"RLE Debug Info — {(_isUop ? "UOP" : "MUL")}  " +
                                  $"Body:{_bodyId} Action:{_actionId} Dir:{_directionId}",
                Size = new Size(1100, 720),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(28, 28, 32),
                ForeColor = Color.White,
                MinimumSize = new Size(600, 400)
            };

            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.FromArgb(38, 38, 50)
            };

            var btnCopy = new Button
            {
                Text = copied ? "📋  Copied to clipboard ✓" : "📋  Copy to clipboard",
                Location = new Point(8, 4),
                Width = 210,
                Height = 24,
                BackColor = copied ? Color.FromArgb(0, 100, 60) : Color.FromArgb(0, 80, 140),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCopy.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            btnCopy.Click += (s, e) =>
            {
                try
                {
                    Clipboard.SetText(text);
                    btnCopy.Text = "📋  Copied ✓";
                    btnCopy.BackColor = Color.FromArgb(0, 100, 60);
                }
                catch { btnCopy.Text = "✗  Copy failed"; }
            };

            var lblInfo = new Label
            {
                Text = $"{_data.Length:N0} bytes  |  {(_isUop ? "UOP" : "MUL")}  |  " +
                            $"Body {_bodyId}  Action {_actionId}  Dir {_directionId}  |  " +
                            $"{headers.Length} frames found",
                Location = new Point(228, 8),
                AutoSize = true,
                ForeColor = Color.FromArgb(140, 190, 255),
                Font = new Font("Consolas", 8.5f)
            };

            toolbar.Controls.Add(btnCopy);
            toolbar.Controls.Add(lblInfo);

            var txtBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(28, 28, 38),
                ForeColor = Color.FromArgb(200, 220, 255),
                Font = new Font("Consolas", 8.5f),
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Text = text
            };

            frm.Controls.Add(txtBox);
            frm.Controls.Add(toolbar);

            lblStatus.Text = copied
                ? $"Debug info copied — {headers.Length} frames found."
                : "Debug info ready — copy failed, use button in window.";

            frm.ShowDialog(this);
        }
        #endregion

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
                long p = i * 2;
                ushort v = (ushort)(data[p] | (data[p + 1] << 8));
                pn.Nodes.Add(new TreeNode($"[{i:D3}]  0x{v:X4}")
                {
                    ForeColor = Color.FromArgb(180, 200, 180),
                    Tag = new StructField
                    {
                        Offset = p,
                        Length = 2,
                        Description = $"Palette entry {i}\nRaw  : 0x{v:X4}\n" +
                                      $"R5   : {(v >> 10) & 0x1F}\nG5   : {(v >> 5) & 0x1F}\n" +
                                      $"B5   : {v & 0x1F}\n" +
                                      $"Alpha: {((v & 0x8000) != 0 ? "opaque" : "transparent")}"
                    }
                });
            }
            pn.Nodes.Add(new TreeNode("… 240 more entries"));
            root.Nodes.Add(pn);

            int fc = data.Length >= 514 ? data[512] | (data[513] << 8) : 0;
            fc = Math.Min(fc, 256);
            SF(root, data, 512, 2, "Frame Count", $"Frames in this direction. Value: {fc}");

            if (fc > 0)
            {
                var tn = new TreeNode($"Frame Lookup  ({fc} offsets @ 0x202)")
                { ForeColor = Color.FromArgb(100, 200, 255) };
                for (int i = 0; i < Math.Min(fc, 32); i++)
                {
                    long lp = 514 + i * 4;
                    if (lp + 4 > data.Length) break;
                    int relOff = data[lp + 2] | (data[lp + 3] << 8);
                    long absOff = 512 + relOff;   // ✓
                    tn.Nodes.Add(new TreeNode($"[{i}]  rel=0x{relOff:X}  abs=0x{absOff:X}")
                    {
                        ForeColor = Color.FromArgb(180, 200, 180),
                        Tag = new StructField
                        {
                            Offset = lp,
                            Length = 4,
                            Description = $"Frame {i} lookup entry\n" +
                                          $"Raw bytes  : {data[lp]:X2} {data[lp + 1]:X2} {data[lp + 2]:X2} {data[lp + 3]:X2}\n" +
                                          $"Rel offset : {relOff}  (0x{relOff:X4})\n" +
                                          $"Abs offset : 0x{(_dataOffset + absOff):X8}  ({absOff})"
                        }
                    });
                }
                if (fc > 32) tn.Nodes.Add(new TreeNode($"… {fc - 32} more"));
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
                    Description = $"{name}\n────────────────────────\n" +
                                  $"File offset : 0x{(_dataOffset + offset):X8}\n" +
                                  $"Size        : {len} byte(s)\n" +
                                  $"Value       : {vs.Trim()}\n\n{desc}"
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

  FIELD-AWARE NAVIGATION
  ──────────────────────────────────────────────────────
  Ctrl+→                 Jump to start of next field
                         (e.g. CenterX → CenterY → Width)
  Ctrl+←                 Jump to start of current field,
                         next press: jump to previous field
  Ctrl+↓                 4 fields forward
                         (e.g. skip entire frame header)
  Ctrl+↑                 4 fields backward
  Ctrl+Shift+→           Next field + extend selection
  Ctrl+Shift+←           Previous field + extend selection
                         → select exactly one field & copy

  FRAME NAVIGATION
  ──────────────────────────────────────────────────────
  Ctrl+N                 Jump to next frame header
  Ctrl+,                 Jump to previous frame header
                         (works from any position in the data)

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
  Ctrl+K                 Frame compare — step 1: set Frame A
                           Navigate to a frame, press Ctrl+K
                           Status shows: Frame A set @ 0x...
                           Then navigate to Frame B and press
                           Ctrl+K again → split view opens.
  Ctrl+Shift+K           Clear Frame A selection
  Ctrl+R                 RLE decoder view for current frame
                           Decodes pixel runs as:
                           [xOffset × count px  col=0xXXXX]
                           Shows full line-by-line breakdown.
  Ctrl+1                 Pin current animation as Compare A
  Ctrl+2                 Pin current animation as Compare B
                           Navigate to another animation,
                           pin it as B → Compare window opens
                           with both side by side.
                           Red  = byte differs (side A)
                           Green = byte differs (side B)
                           Sync scroll toggle keeps both
                           panels in lockstep.

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

  LIVE FIELD ANALYSIS
  ──────────────────────────────────────────────────────
  Status bar             Shows on every cursor move:
                           [FieldType] FieldName = Value
                           e.g. [int16-LE] Frame[2].CenterX = -22
  Hover tooltip          After 600ms: full field details
                           Name / Type / Offset / Size / Value
  MUL fields:
    Offset   0–511       Palette[n]  (256 × ARGB1555)
    Offset 512–513       FrameCount  (uint16)
    Offset 514+          FrameLookup[n]  (uint32 per entry)
    From lookup target   Frame[n].CenterX / CenterY (int16)
                         Frame[n].Width / Height (uint16)
                         Frame[n].PixelData (RLE-stream)
  UOP fields:
    Offset   0–23        Header (Magic / BodyId / FrameCount /
                         ActionIdx / Direction / Flags)
    Offset  24+          Frame table (Offset / Length / Extra
                         12 bytes per entry)
    From data offset     Frame[n].CenterX / CenterY (int16)
                         Frame[n].Width / Height (uint16)
                         Frame[n].PixelData (RLE-stream)

  SCREENSHOT
  ──────────────────────────────────────────────────────
  Toolbar 'Screenshot':
    • Copy hex panel to clipboard
    • Copy entire form to clipboard
    • Save as PNG file

  CONTEXT MENU  (right-click anywhere)
  ──────────────────────────────────────────────────────
  All tools above + region-specific actions.
  Right-click also shows:
    • Select / copy region by name
    • Compare two frames  (Ctrl+K)
    • RLE decoder view    (Ctrl+R)

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

            ShowMonoDialog("Command Overview  (F1)", txt, 690, 960);
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
            string bv = _cursor >= 0 && _cursor < _data.Length
                         ? $"  │  0x{_data[_cursor]:X2} ({_data[_cursor]})" : "";
            string sel = len > 1
                         ? $"  │  Sel: 0x{(_dataOffset + lo):X8}–0x{(_dataOffset + hi):X8}  ({len:N0} B)"
                         : $"  │  0x{(_dataOffset + _cursor):X8}";
            string mod = _modifiedBytes.Count > 0 ? $"  │  {_modifiedBytes.Count} mod" : "";
            string diff = _diffMode ? $"  │  DIFF" : "";

            // ── Live Field Analysis ──────────────────────────────────────────
            var field = ParseFieldAtCursor(_cursor);
            string fieldInfo = field != null
                ? $"  │  [{field.TypeName}] {field.Name} = {field.Value}"
                : "";

            lblStatus.Text =
                $"{Path.GetFileName(_sourceFile ?? "?")}  │  {_data.Length:N0} B" +
                $"{sel}{bv}{reg}{fieldInfo}{mod}{diff}";

            // ── Tooltip with details (overwrites old tooltip)─────────────
            if (field != null && !string.IsNullOrEmpty(field.Description))
            {
                _pendingTooltip = $"{field.Name}\n" +
                                  $"Type:   {field.TypeName}\n" +
                                  $"Offset: 0x{(_dataOffset + field.Offset):X8}\n" +
                                  $"Size:   {field.Size} byte(s)\n" +
                                  $"Value:  {field.Value}" +
                                  (field.Description.Length > 0 ? $"\n\n{field.Description}" : "");
            }
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

        private ParsedField ParseFieldAtCursor(long cursor)
        {
            if (_data == null || cursor < 0 || cursor >= _data.Length)
                return null;

            // ── UOP ──────────────────────────────────────────────────────────
            if (_isUop)
                return ParseFieldUop(cursor);

            // ── MUL ──────────────────────────────────────────────────────────
            return ParseFieldMul(cursor);
        }

        private ParsedField ParseFieldUop(long pos)
        {
            if (pos < 4)
                return MakeField("Magic / FileType", "uint32-LE", 0, 4, pos);
            if (pos < 8)
                return MakeField("BodyId", "uint32-LE", 4, 4, pos);
            if (pos < 12)
                return MakeField("FrameCount", "uint32-LE", 8, 4, pos);
            if (pos < 16)
                return MakeField("Action Index", "uint32-LE", 12, 4, pos);
            if (pos < 20)
                return MakeField("Direction", "uint32-LE", 16, 4, pos);
            if (pos < 24)
                return MakeField("Flags", "uint32-LE", 20, 4, pos);

            // Frame-Table (ab Offset 24, je 12 Bytes)
            int fc = _data.Length >= 12
                ? (int)ReadUInt32(_data, 8) : 0;
            long tableEnd = 24 + fc * 12L;

            if (pos >= 24 && pos < tableEnd)
            {
                long entry = (pos - 24) / 12;
                long entryStart = 24 + entry * 12;
                long within = pos - entryStart;

                if (within < 4) return MakeField($"Frame[{entry}].DataOffset", "int32-LE", entryStart, 4, pos);
                if (within < 8) return MakeField($"Frame[{entry}].DataLength", "int32-LE", entryStart + 4, 4, pos);
                return MakeField($"Frame[{entry}].Extra", "int32-LE", entryStart + 8, 4, pos);
            }

            // Frame data: search to which frame this offset belongs
            for (int i = 0; i < Math.Min(fc, 256); i++)
            {
                long lp = 24 + i * 12;
                if (lp + 8 > _data.Length) break;
                int fOff = (int)ReadUInt32(_data, lp);
                int fLen = (int)ReadUInt32(_data, lp + 4);
                if (fOff <= 0 || fLen <= 0) continue;
                if (pos < fOff || pos >= fOff + fLen) continue;

                long within = pos - fOff;
                if (within < 2) return MakeField($"Frame[{i}].CenterX", "int16-LE", fOff, 2, pos);
                if (within < 4) return MakeField($"Frame[{i}].CenterY", "int16-LE", fOff + 2, 2, pos);
                if (within < 6) return MakeField($"Frame[{i}].Width", "uint16-LE", fOff + 4, 2, pos);
                if (within < 8) return MakeField($"Frame[{i}].Height", "uint16-LE", fOff + 6, 2, pos);
                return MakeField($"Frame[{i}].PixelData", "RLE-stream", fOff + 8,
                                         fLen - 8, pos, $"Run-Length-Encoded pixel data ({fLen - 8} Bytes)");
            }

            return MakeField("Unknown", "byte", pos, 1, pos);
        }

        private ParsedField ParseFieldMul(long pos)
        {
            if (pos < 512)
            {
                long entry = pos / 2;
                long entryOff = entry * 2;
                ushort v = (ushort)(_data[entryOff] | (_data[entryOff + 1] << 8));
                int r5 = (v >> 10) & 0x1F, g5 = (v >> 5) & 0x1F, b5 = v & 0x1F;
                bool alpha = (v & 0x8000) != 0;
                string extra = $"R={r5 * 255 / 31}  G={g5 * 255 / 31}  B={b5 * 255 / 31}" +
                               $"  Alpha={(alpha ? "opaque" : "transparent")}";
                return MakeField($"Palette[{entry}]", "ARGB1555", entryOff, 2, pos, extra);
            }

            if (pos < 514)
            {
                ushort fc = (ushort)(_data[512] | (_data[513] << 8));
                return MakeField("FrameCount", "uint16-LE", 512, 2, pos,
                                 $"Number of frames in this direction: {fc}");
            }

            ushort frameCount = _data.Length >= 514
                ? (ushort)(_data[512] | (_data[513] << 8)) : (ushort)0;
            long dataStart = 514 + frameCount * 4L;
            long lookupEnd = dataStart;

            // ── Lookup-Tabelle ────────────────────────────────────────────────────
            if (pos >= 514 && pos < lookupEnd)
            {
                long fi = (pos - 514) / 4;
                long fp = 514 + fi * 4;
                int relOff = _data.Length >= fp + 4
                    ? (_data[fp + 2] | (_data[fp + 3] << 8)) : 0;
                long absOff = 512 + relOff;   // ✓
                return MakeField($"FrameLookup[{fi}]", "uint16-in-uint32", fp, 4, pos,
                                 $"Relative offset to frame {fi}: {relOff}" +
                                 $"  →  absolute 0x{(_dataOffset + absOff):X8}");
            }

            // ── Frame-Daten ───────────────────────────────────────────────────────
            for (int i = 0; i < frameCount && i < 256; i++)
            {
                long lp = 514 + i * 4;
                if (lp + 4 > _data.Length) break;

                int relOff = _data[lp + 2] | (_data[lp + 3] << 8);
                long fOff = 512 + relOff;   // ✓
                if (fOff < 0 || fOff >= _data.Length) continue;

                long nextFOff = _data.Length;
                if (i + 1 < frameCount)
                {
                    long nlp = 514 + (i + 1) * 4;
                    if (nlp + 4 <= _data.Length)
                    {
                        int nRel = _data[nlp + 2] | (_data[nlp + 3] << 8);
                        long nAbs = 512 + nRel;   // ✓
                        if (nAbs > fOff && nAbs <= _data.Length)
                            nextFOff = nAbs;
                    }
                }

                if (pos < fOff || pos >= nextFOff) continue;

                long within = pos - fOff;
                if (within < 2) return MakeField($"Frame[{i}].CenterX", "int16-LE", fOff, 2, pos);
                if (within < 4) return MakeField($"Frame[{i}].CenterY", "int16-LE", fOff + 2, 2, pos);
                if (within < 6) return MakeField($"Frame[{i}].Width", "uint16-LE", fOff + 4, 2, pos);
                if (within < 8) return MakeField($"Frame[{i}].Height", "uint16-LE", fOff + 6, 2, pos);
                return MakeField($"Frame[{i}].PixelData", "RLE-stream", fOff + 8,
                                 nextFOff - fOff - 8, pos,
                                 $"Compressed pixel data for frame {i}");
            }
            return MakeField("Unknown", "byte", pos, 1, pos);
        }

        private ParsedField MakeField(string name, string type, long offset, long size,
                               long cursor, string desc = null)
        {
            if (_data == null || offset < 0 || offset >= _data.Length)
                return new ParsedField
                {
                    Name = name,
                    TypeName = type,
                    Offset = offset,
                    Size = (int)Math.Min(size, 8),
                    Value = "?",
                    Description = desc ?? ""
                };

            string val = FormatFieldValue(type, offset, size);
            return new ParsedField
            {
                Name = name,
                TypeName = type,
                Offset = offset,
                Size = (int)Math.Min(size, int.MaxValue),
                Value = val,
                Description = desc ?? ""
            };
        }

        private string FormatFieldValue(string type, long offset, long size)
        {
            if (_data == null || offset < 0) return "?";
            try
            {
                switch (type)
                {
                    case "uint8":
                        return $"{_data[offset]}  (0x{_data[offset]:X2})";
                    case "int16-LE":
                        {
                            if (offset + 2 > _data.Length) return "?";
                            short v = (short)(_data[offset] | (_data[offset + 1] << 8));
                            return $"{v}  (0x{(ushort)v:X4})";
                        }
                    case "uint16-LE":
                        {
                            if (offset + 2 > _data.Length) return "?";
                            ushort v = (ushort)(_data[offset] | (_data[offset + 1] << 8));
                            return $"{v}  (0x{v:X4})";
                        }
                    case "int32-LE":
                    case "uint32-LE":
                        {
                            if (offset + 4 > _data.Length) return "?";
                            uint v = ReadUInt32(_data, offset);
                            return type == "int32-LE"
                                ? $"{(int)v}  (0x{v:X8})"
                                : $"{v}  (0x{v:X8})";
                        }
                    case "ARGB1555":
                        {
                            if (offset + 2 > _data.Length) return "?";
                            ushort v = (ushort)(_data[offset] | (_data[offset + 1] << 8));
                            int r5 = (v >> 10) & 0x1F, g5 = (v >> 5) & 0x1F, b5 = v & 0x1F;
                            return $"0x{v:X4}  R={r5 * 255 / 31} G={g5 * 255 / 31} B={b5 * 255 / 31}";
                        }
                    default:
                        return $"{size} bytes";
                }
            }
            catch { return "?"; }
        }

        private static uint ReadUInt32(byte[] data, long offset)
        {
            return (uint)(data[offset] | (data[offset + 1] << 8) |
                          (data[offset + 2] << 16) | (data[offset + 3] << 24));
        }

        // ══════════════════════════════════════════════════════════════════════
        //  DYNAMISCHE ANALYSE
        // ══════════════════════════════════════════════════════════════════════

        private sealed class ParsedField
        {
            public string Name;
            public string TypeName;
            public long Offset;
            public int Size;
            public string Value;
            public string Description;
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

    // ==========================================================================
    //  RleDecoderWindow - standalone RLE decoder display window
    //  Diagnose panel toggle, proper BGR->RGB palette, clean layout
    // ==========================================================================
    internal static class RleDecoderWindow
    {
        public static void Show(Form owner,
            int frameIdx, short cx, short cy, ushort w, ushort h,
            long absoluteAddr,
            int pw, int ph, uint[] pixBuf,
            string runText, string diagText)
        {
            var frm = new Form
            {
                Text = string.Format("RLE Decoder - Frame {0}  ({1}x{2})  @ 0x{3:X8}",
                    frameIdx, w, h, absoluteAddr),
                Size = new Size(1100, 700),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(28, 28, 32),
                ForeColor = Color.White,
                MinimumSize = new Size(640, 420)
            };

            // ── Toolbar ────────────────────────────────────────────────────────
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.FromArgb(38, 38, 50),
            };

            var btnDiag = new Button
            {
                Text = "Diagnose einblenden",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 90, 150),
                ForeColor = Color.White,
                Height = 24,
                Width = 180,
                Left = 4,
                Top = 4,
                Font = new Font("Segoe UI", 8.5f)
            };
            btnDiag.FlatAppearance.BorderColor = Color.FromArgb(0, 130, 200);

            var lblInfo = new Label
            {
                Text = string.Format("Frame:{0}  {1}x{2}  CX={3} CY={4}  @ 0x{5:X8}",
                    frameIdx, w, h, cx, cy, absoluteAddr),
                ForeColor = Color.FromArgb(140, 190, 255),
                Font = new Font("Consolas", 8.5f),
                AutoSize = true,
                Left = 192,
                Top = 8
            };

            toolbar.Controls.Add(btnDiag);
            toolbar.Controls.Add(lblInfo);

            // ── Diagnose-Panel (hidden by default) ─────────────────────────────
            int diagLineCount = 0;
            foreach (char c in diagText)
                if (c == '\n') diagLineCount++;
            diagLineCount += 2;

            var diagBox = new TextBox
            {
                Dock = DockStyle.Top,
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(15, 35, 15),
                ForeColor = Color.FromArgb(80, 220, 100),
                Font = new Font("Consolas", 8.5f),
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Vertical,
                WordWrap = false,
                Text = diagText,
                Height = Math.Min(diagLineCount * 16 + 8, 300),
                Visible = false
            };

            bool diagVisible = false;
            btnDiag.Click += (s, e) =>
            {
                diagVisible = !diagVisible;
                diagBox.Visible = diagVisible;
                btnDiag.Text = diagVisible ? "Diagnose ausblenden" : "Diagnose einblenden";
                btnDiag.BackColor = diagVisible
                    ? Color.FromArgb(0, 130, 60)
                    : Color.FromArgb(0, 90, 150);
            };

            // ── Main split: left=preview, right=run text ───────────────────────
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 420,
                SplitterWidth = 5,
                BackColor = Color.FromArgb(50, 50, 60)
            };

            // ── Preview panel (left) ───────────────────────────────────────────
            var previewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(28, 28, 38)
            };

            // Build bitmap once from pixBuf
            System.Drawing.Bitmap frameBmp = null;
            if (pw > 0 && ph > 0)
            {
                frameBmp = new System.Drawing.Bitmap(pw, ph,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                for (int py2 = 0; py2 < ph; py2++)
                {
                    for (int px2 = 0; px2 < pw; px2++)
                    {
                        uint val = pixBuf[py2 * pw + px2];
                        if (val == 0) continue;
                        int a2 = (int)((val >> 24) & 0xFF);
                        int r2 = (int)((val >> 16) & 0xFF);
                        int g2 = (int)((val >> 8) & 0xFF);
                        int b2 = (int)(val & 0xFF);
                        frameBmp.SetPixel(px2, py2, Color.FromArgb(a2, r2, g2, b2));
                    }
                }
            }

            previewPanel.Paint += (s, e) =>
            {
                var g2 = e.Graphics;
                g2.Clear(Color.FromArgb(28, 28, 38));
                if (frameBmp == null) return;

                int panW = previewPanel.ClientSize.Width - 8;
                int panH = previewPanel.ClientSize.Height - 32;
                if (panW <= 0 || panH <= 0) return;

                float scale = Math.Max(1f, Math.Min((float)panW / pw, (float)panH / ph));
                int drawW = (int)(pw * scale);
                int drawH = (int)(ph * scale);
                int drawX = (previewPanel.ClientSize.Width - drawW) / 2;
                int drawY = 24 + (panH - drawH) / 2;

                // Checkerboard background for transparency
                int cs = Math.Max(6, (int)(scale * 0.8f));
                using (var tb1 = new SolidBrush(Color.FromArgb(55, 55, 65)))
                using (var tb2 = new SolidBrush(Color.FromArgb(75, 75, 85)))
                {
                    for (int py2 = 0; py2 < drawH; py2 += cs)
                        for (int px2 = 0; px2 < drawW; px2 += cs)
                            g2.FillRectangle(
                                ((px2 / cs + py2 / cs) % 2 == 0) ? tb1 : tb2,
                                drawX + px2, drawY + py2,
                                Math.Min(cs, drawW - px2),
                                Math.Min(cs, drawH - py2));
                }

                g2.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g2.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g2.DrawImage(frameBmp,
                    new Rectangle(drawX, drawY, drawW, drawH),
                    new Rectangle(0, 0, pw, ph),
                    GraphicsUnit.Pixel);

                using (var ib = new SolidBrush(Color.FromArgb(140, 190, 255)))
                {
                    g2.DrawString(
                        string.Format("Frame {0}  {1}x{2}  CX={3} CY={4}  (scale {5:F1}x)",
                            frameIdx, w, h, cx, cy, scale),
                        new Font("Consolas", 8f), ib, new PointF(4, 4));
                }
            };

            previewPanel.Resize += (s, e) => previewPanel.Invalidate();
            frm.FormClosed += (s, e) => { frameBmp?.Dispose(); };

            // ── Run text (right) ───────────────────────────────────────────────
            var txtBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(28, 28, 38),
                ForeColor = Color.FromArgb(200, 220, 255),
                Font = new Font("Consolas", 8.5f),
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Text = runText
            };

            split.Panel1.Controls.Add(previewPanel);
            split.Panel2.Controls.Add(txtBox);

            // Add in reverse dock order: Fill first, then Top panels
            frm.Controls.Add(split);
            frm.Controls.Add(diagBox);
            frm.Controls.Add(toolbar);

            frm.ShowDialog(owner);
        }
    }
}
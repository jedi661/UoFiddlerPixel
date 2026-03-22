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
using System.Text;
using System.Windows.Forms;
namespace UoFiddler.Controls.Forms
{
    // ══════════════════════════════════════════════════════════════════════════
    //  Data container passed from AnimationEditForm to HexCompareForm
    // ══════════════════════════════════════════════════════════════════════════
    public sealed class HexCompareBuffer
    {
        public byte[] Data;
        public long FileOffset;
        public string FilePath;
        public int BodyId;
        public int ActionId;
        public int DirectionId;
        public bool IsUop;
        public Bitmap Preview;
        public string Label =>
            $"{System.IO.Path.GetFileName(FilePath ?? "?")}  " +
            $"Body:{BodyId}  Action:{ActionId}  Dir:{DirectionId}" +
            (IsUop ? "  [UOP]" : "  [MUL]");
    }
    // ══════════════════════════════════════════════════════════════════════════
    //  HexCompareForm
    // ══════════════════════════════════════════════════════════════════════════
    public class HexCompareForm : Form
    {
        // ── Double-buffered Panel ─────────────────────────────────────────────
        // SetStyle() is protected — must be called from within the derived class.
        private sealed class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer, true);
                UpdateStyles();
            }
        }
        // ── Layout ────────────────────────────────────────────────────────────
        private const int BytesPerRow = 16;
        private const int OffsetColW = 90;
        private const int HexCellW = 26;
        private const int AsciiCellW = 10;
        private const int RowH = 18;
        private const int HeaderH = 24;
        private const int GutterW = 12;
        // ── Colors ────────────────────────────────────────────────────────────
        private static readonly Color CoBg = Color.FromArgb(28, 28, 32);
        private static readonly Color CoAltRow = Color.FromArgb(33, 33, 40);
        private static readonly Color CoOffsetFg = Color.FromArgb(100, 180, 255);
        private static readonly Color CoHexNorm = Color.FromArgb(220, 220, 220);
        private static readonly Color CoHexZero = Color.FromArgb(80, 80, 90);
        private static readonly Color CoAscii = Color.FromArgb(160, 160, 160);
        private static readonly Color CoHeaderBg = Color.FromArgb(38, 38, 50);
        private static readonly Color CoHeaderFg = Color.FromArgb(140, 140, 160);
        private static readonly Color CoGridLine = Color.FromArgb(50, 50, 60);
        private static readonly Color CoDiffA = Color.FromArgb(130, 200, 60, 60);
        private static readonly Color CoDiffB = Color.FromArgb(130, 60, 160, 60);
        private static readonly Color CoCursor = Color.FromArgb(255, 200, 50);
        private static readonly Color CoSel = Color.FromArgb(0, 100, 200);
        private static readonly Color CoSelTxt = Color.White;
        // ── Fonts ─────────────────────────────────────────────────────────────
        private readonly Font _mono = new("Consolas", 9f);
        private readonly Font _hdr = new("Consolas", 8f, FontStyle.Bold);
        private readonly Font _lbl = new("Segoe UI", 8f);
        // ── Buffers ───────────────────────────────────────────────────────────
        private HexCompareBuffer _bufA;
        private HexCompareBuffer _bufB;
        // ── Per-panel state ───────────────────────────────────────────────────
        private long _scrollA = 0, _scrollB = 0;
        private long _cursorA = 0, _cursorB = 0;
        private long _selStartA = -1, _selEndA = -1;
        private long _selStartB = -1, _selEndB = -1;
        private bool _syncScroll = true;
        private int _visibleRows = 1;
        // ── Controls — typed as DoubleBufferedPanel ───────────────────────────
        private DoubleBufferedPanel _panelA, _panelB;
        private VScrollBar _scrollBarA, _scrollBarB;
        private Label _lblInfoA, _lblInfoB;
        private Label _lblStatusA, _lblStatusB;
        private PictureBox _picA, _picB;
        private CheckBox _chkSync;
        private Label _lblDiffCount;
        // ══════════════════════════════════════════════════════════════════════
        //  Constructor
        // ══════════════════════════════════════════════════════════════════════
        public HexCompareForm()
        {
            InitLayout();
        }
        // ══════════════════════════════════════════════════════════════════════
        //  Public API  — called from AnimationEditForm
        // ══════════════════════════════════════════════════════════════════════
        public void LoadBufferA(HexCompareBuffer buf)
        {
            _bufA = buf;
            _scrollA = 0; _cursorA = 0;
            _selStartA = -1; _selEndA = -1;
            UpdateInfoLabel(_lblInfoA, buf);
            UpdateScrollBar(_scrollBarA, buf?.Data, _visibleRows);
            if (buf?.Preview != null) _picA.Image = buf.Preview;
            RefreshDiffCount();
            _panelA.Invalidate();
            UpdateStatusA();
        }
        public void LoadBufferB(HexCompareBuffer buf)
        {
            _bufB = buf;
            _scrollB = 0; _cursorB = 0;
            _selStartB = -1; _selEndB = -1;
            UpdateInfoLabel(_lblInfoB, buf);
            UpdateScrollBar(_scrollBarB, buf?.Data, _visibleRows);
            if (buf?.Preview != null) _picB.Image = buf.Preview;
            RefreshDiffCount();
            _panelB.Invalidate();
            UpdateStatusB();
        }
        // ══════════════════════════════════════════════════════════════════════
        //  Layout builder
        // ══════════════════════════════════════════════════════════════════════
        private void InitLayout()
        {
            Text = "Animation Hex Compare";
            Size = new Size(1300, 720);
            MinimumSize = new Size(900, 500);
            BackColor = Color.FromArgb(28, 28, 32);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterScreen;
            // ── Top toolbar ──────────────────────────────────────────────────
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(38, 38, 50)
            };
            _chkSync = new CheckBox
            {
                Text = "Sync scroll",
                Checked = true,
                ForeColor = Color.FromArgb(180, 220, 255),
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            _chkSync.CheckedChanged += (s, e) =>
            {
                _syncScroll = _chkSync.Checked;
                if (_syncScroll) { _scrollB = _scrollA; _panelB.Invalidate(); }
            };
            _lblDiffCount = new Label
            {
                ForeColor = Color.FromArgb(255, 160, 80),
                Font = new Font("Consolas", 8.5f),
                AutoSize = true,
                Location = new Point(140, 11)
            };
            var btnSwap = MakeButton("⇄ Swap A/B", new Point(340, 5), 90,
                Color.FromArgb(60, 60, 80), (s, e) =>
                {
                    var tmp = _bufA; _bufA = _bufB; _bufB = tmp;
                    UpdateInfoLabel(_lblInfoA, _bufA);
                    UpdateInfoLabel(_lblInfoB, _bufB);
                    RefreshDiffCount();
                    _panelA.Invalidate(); _panelB.Invalidate();
                    UpdateStatusA(); UpdateStatusB();
                });
            toolbar.Controls.AddRange(new Control[] { _chkSync, _lblDiffCount, btnSwap });
            // ── Main split area ───────────────────────────────────────────────
            var mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                BackColor = Color.FromArgb(28, 28, 32),
                Panel1MinSize = 300,
                Panel2MinSize = 300
            };
            BuildSideA(mainSplit.Panel1);
            BuildSideB(mainSplit.Panel2);
            Controls.Add(mainSplit);
            Controls.Add(toolbar);
            Resize += (s, e) => RecalcLayout();
        }
        // ── Side builder ─────────────────────────────────────────────────────
        private void BuildSideA(SplitterPanel parent)
        {
            _lblInfoA = MakeInfoLabel("A: (no data loaded)");
            _lblStatusA = MakeStatusLabel();
            _picA = MakePreviewBox();
            _scrollBarA = MakeScrollBar();
            _panelA = MakeHexPanel();
            _panelA.Paint += (s, e) => PaintPanel(e.Graphics, e.ClipRectangle, _bufA, _scrollA, _cursorA, _selStartA, _selEndA, _bufB);
            _panelA.MouseDown += (s, e) => OnMouseDownA(e);
            _panelA.MouseWheel += (s, e) => OnWheelA(e);
            _panelA.KeyDown += (s, e) => OnKeyDownA(e);
            _panelA.Resize += (s, e) => { RecalcVisibleRows(); UpdateScrollBar(_scrollBarA, _bufA?.Data, _visibleRows); };
            _scrollBarA.Scroll += (s, e) => { _scrollA = (long)e.NewValue * BytesPerRow; _panelA.Invalidate(); if (_syncScroll) SyncBtoA(); };
            parent.Controls.Add(_scrollBarA);
            parent.Controls.Add(_panelA);
            parent.Controls.Add(_picA);
            parent.Controls.Add(_lblStatusA);
            parent.Controls.Add(_lblInfoA);
        }
        private void BuildSideB(SplitterPanel parent)
        {
            _lblInfoB = MakeInfoLabel("B: (no data loaded)");
            _lblStatusB = MakeStatusLabel();
            _picB = MakePreviewBox();
            _scrollBarB = MakeScrollBar();
            _panelB = MakeHexPanel();
            _panelB.Paint += (s, e) => PaintPanel(e.Graphics, e.ClipRectangle, _bufB, _scrollB, _cursorB, _selStartB, _selEndB, _bufA);
            _panelB.MouseDown += (s, e) => OnMouseDownB(e);
            _panelB.MouseWheel += (s, e) => OnWheelB(e);
            _panelB.KeyDown += (s, e) => OnKeyDownB(e);
            _panelB.Resize += (s, e) => { RecalcVisibleRows(); UpdateScrollBar(_scrollBarB, _bufB?.Data, _visibleRows); };
            _scrollBarB.Scroll += (s, e) => { _scrollB = (long)e.NewValue * BytesPerRow; _panelB.Invalidate(); if (_syncScroll) SyncAtoB(); };
            parent.Controls.Add(_scrollBarB);
            parent.Controls.Add(_panelB);
            parent.Controls.Add(_picB);
            parent.Controls.Add(_lblStatusB);
            parent.Controls.Add(_lblInfoB);
        }
        private void RecalcLayout()
        {
            foreach (var side in new[] {
                (_panelA, _scrollBarA, _lblInfoA, _lblStatusA, _picA),
                (_panelB, _scrollBarB, _lblInfoB, _lblStatusB, _picB) })
            {
                var (panel, scroll, info, status, pic) = side;
                if (panel.Parent == null) continue;
                int w = panel.Parent.Width;
                int h = panel.Parent.Height;
                int previewH = 80;
                int infoH = 22;
                int statusH = 20;
                int scrollW = 17;
                info.SetBounds(0, 0, w, infoH);
                pic.SetBounds(0, infoH, w, previewH);
                panel.SetBounds(0, infoH + previewH, w - scrollW, h - infoH - previewH - statusH);
                scroll.SetBounds(w - scrollW, infoH + previewH, scrollW, h - infoH - previewH - statusH);
                status.SetBounds(0, h - statusH, w, statusH);
            }
            RecalcVisibleRows();
        }
        private void RecalcVisibleRows()
        {
            _visibleRows = Math.Max(1, (_panelA.Height - HeaderH) / RowH);
        }
        // ── Control factories ─────────────────────────────────────────────────
        private DoubleBufferedPanel MakeHexPanel() =>
            new DoubleBufferedPanel { BackColor = CoBg, TabStop = true };
        private Label MakeInfoLabel(string text) => new Label
        {
            Text = text,
            Height = 22,
            BackColor = Color.FromArgb(38, 38, 50),
            ForeColor = Color.FromArgb(140, 190, 255),
            Font = new Font("Consolas", 8.5f),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(6, 0, 0, 0)
        };
        private Label MakeStatusLabel() => new Label
        {
            Height = 20,
            BackColor = Color.FromArgb(22, 22, 28),
            ForeColor = Color.FromArgb(160, 200, 255),
            Font = new Font("Consolas", 8f),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 0, 0)
        };
        private PictureBox MakePreviewBox() => new PictureBox
        {
            Height = 80,
            BackColor = Color.FromArgb(38, 38, 48),
            SizeMode = PictureBoxSizeMode.Zoom
        };
        private VScrollBar MakeScrollBar() => new VScrollBar
        {
            Width = 17,
            Minimum = 0,
            Maximum = 0,
            SmallChange = 1,
            LargeChange = 10
        };
        private static Button MakeButton(string text, Point loc, int w, Color bg, EventHandler onClick)
        {
            var b = new Button
            {
                Text = text,
                Location = loc,
                Width = w,
                Height = 26,
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            b.Click += onClick;
            return b;
        }
        // ══════════════════════════════════════════════════════════════════════
        //  PAINT
        // ══════════════════════════════════════════════════════════════════════
        private void PaintPanel(Graphics g, Rectangle clip,
            HexCompareBuffer buf, long scroll, long cursor,
            long selStart, long selEnd,
            HexCompareBuffer other)
        {
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(CoBg);
            if (buf?.Data == null || buf.Data.Length == 0)
            {
                using var nb = new SolidBrush(CoHeaderFg);
                g.DrawString("No data loaded.  Use 'Pin as A' or 'Pin as B' in the Hex Editor.",
                    _lbl, nb, new PointF(16, HeaderH + 14));
                DrawColumnHeader(g, _panelA.Width);
                return;
            }
            byte[] data = buf.Data;
            byte[] odata = other?.Data;
            int panW = (buf == _bufA ? _panelA : _panelB).Width;
            long maxScr = Math.Max(0,
                ((data.Length + BytesPerRow - 1) / BytesPerRow - _visibleRows) * (long)BytesPerRow);
            scroll = Math.Max(0, Math.Min(scroll, maxScr));
            int hexX = OffsetColW;
            int ascX = hexX + BytesPerRow * HexCellW + GutterW;
            for (int row = 0; row <= _visibleRows; row++)
            {
                long bi = scroll + row * BytesPerRow;
                if (bi >= data.Length) break;
                int y = HeaderH + row * RowH;
                if (y + RowH < clip.Top || y > clip.Bottom) continue;
                // alt row
                if (row % 2 == 1)
                {
                    using var ab = new SolidBrush(CoAltRow);
                    g.FillRectangle(ab, 0, y, ascX + BytesPerRow * AsciiCellW + 8, RowH);
                }
                // offset
                long absOff = buf.FileOffset + bi;
                using (var ob = new SolidBrush(CoOffsetFg))
                    g.DrawString($"0x{absOff:X8}", _mono, ob, new PointF(4, y + 1));
                int count = (int)Math.Min(BytesPerRow, data.Length - bi);
                for (int col = 0; col < BytesPerRow; col++)
                {
                    long bIdx = bi + col;
                    int hx = hexX + col * HexCellW;
                    int ax = ascX + col * AsciiCellW;
                    if (col >= count) continue;
                    byte b = data[bIdx];
                    bool isDiff = odata != null && bIdx < odata.Length && odata[bIdx] != b;
                    bool isSel = IsInSel(bIdx, selStart, selEnd);
                    // diff highlight
                    if (isDiff && !isSel)
                    {
                        bool isA = (buf == _bufA);
                        using var db = new SolidBrush(isA ? CoDiffA : CoDiffB);
                        g.FillRectangle(db, hx, y, HexCellW - 1, RowH);
                        g.FillRectangle(db, ax, y, AsciiCellW, RowH);
                    }
                    // selection
                    if (isSel)
                    {
                        using var sb = new SolidBrush(CoSel);
                        g.FillRectangle(sb, hx, y, HexCellW - 1, RowH);
                        g.FillRectangle(sb, ax, y, AsciiCellW, RowH);
                    }
                    // hex text
                    Color hc = isSel ? CoSelTxt :
                               isDiff ? Color.White :
                               b == 0 ? CoHexZero : CoHexNorm;
                    using (var hb = new SolidBrush(hc))
                        g.DrawString(b.ToString("X2"), _mono, hb, new PointF(hx + 2, y + 1));
                    // cursor
                    if (bIdx == cursor)
                    {
                        using var cp = new Pen(CoCursor, 2f);
                        g.DrawRectangle(cp, hx + 1, y + 1, HexCellW - 3, RowH - 3);
                    }
                    // ascii
                    char c = (b >= 32 && b < 127) ? (char)b : '.';
                    Color ac = isSel ? CoSelTxt : isDiff ? Color.White : CoAscii;
                    using (var acb = new SolidBrush(ac))
                        g.DrawString(c.ToString(), _mono, acb, new PointF(ax + 1, y + 1));
                }
            }
            // grid lines
            using (var gp = new Pen(CoGridLine))
            {
                g.DrawLine(gp, OffsetColW - 2, 0, OffsetColW - 2, _panelA.Height);
                g.DrawLine(gp, ascX - GutterW / 2, HeaderH, ascX - GutterW / 2, _panelA.Height);
            }
            DrawColumnHeader(g, panW);
        }
        private void DrawColumnHeader(Graphics g, int panelW)
        {
            using var hb = new SolidBrush(CoHeaderBg);
            g.FillRectangle(hb, 0, 0, panelW, HeaderH);
            using var fg = new SolidBrush(CoHeaderFg);
            g.DrawString("Offset    ", _hdr, fg, new PointF(4, 4));
            int hx = OffsetColW;
            for (int col = 0; col < BytesPerRow; col++)
            {
                int x = hx + col * HexCellW;
                using var gp = new Pen(CoGridLine);
                g.DrawLine(gp, x, 0, x, HeaderH);
                g.DrawString(col.ToString("X2"), _hdr, fg, new PointF(x + 4, 4));
            }
            int ax = hx + BytesPerRow * HexCellW + GutterW;
            g.DrawString("ASCII", _hdr, fg, new PointF(ax + 2, 4));
        }
        // ══════════════════════════════════════════════════════════════════════
        //  Mouse & Keyboard — Side A
        // ══════════════════════════════════════════════════════════════════════
        private void OnMouseDownA(MouseEventArgs e)
        {
            _panelA.Focus();
            long idx = HitTest(e.Location, _scrollA);
            if (idx < 0 || _bufA?.Data == null || idx >= _bufA.Data.Length) return;
            _cursorA = idx;
            _selStartA = idx; _selEndA = idx;
            _panelA.Invalidate();
            UpdateStatusA();
        }
        private void OnWheelA(MouseEventArgs e)
        {
            _scrollA += (e.Delta > 0 ? -3 : 3) * BytesPerRow;
            ClampScroll(ref _scrollA, _bufA?.Data);
            UpdateScrollBar(_scrollBarA, _bufA?.Data, _visibleRows);
            _panelA.Invalidate();
            if (_syncScroll) SyncBtoA();
        }
        private void OnKeyDownA(KeyEventArgs e)
        {
            if (_bufA?.Data == null) return;
            MoveCursor(ref _cursorA, ref _scrollA, e, _bufA.Data);
            UpdateScrollBar(_scrollBarA, _bufA.Data, _visibleRows);
            _panelA.Invalidate();
            UpdateStatusA();
            if (_syncScroll) SyncBtoA();
            e.Handled = true;
        }
        // ══════════════════════════════════════════════════════════════════════
        //  Mouse & Keyboard — Side B
        // ══════════════════════════════════════════════════════════════════════
        private void OnMouseDownB(MouseEventArgs e)
        {
            _panelB.Focus();
            long idx = HitTest(e.Location, _scrollB);
            if (idx < 0 || _bufB?.Data == null || idx >= _bufB.Data.Length) return;
            _cursorB = idx;
            _selStartB = idx; _selEndB = idx;
            _panelB.Invalidate();
            UpdateStatusB();
        }
        private void OnWheelB(MouseEventArgs e)
        {
            _scrollB += (e.Delta > 0 ? -3 : 3) * BytesPerRow;
            ClampScroll(ref _scrollB, _bufB?.Data);
            UpdateScrollBar(_scrollBarB, _bufB?.Data, _visibleRows);
            _panelB.Invalidate();
            if (_syncScroll) SyncAtoB();
        }
        private void OnKeyDownB(KeyEventArgs e)
        {
            if (_bufB?.Data == null) return;
            MoveCursor(ref _cursorB, ref _scrollB, e, _bufB.Data);
            UpdateScrollBar(_scrollBarB, _bufB.Data, _visibleRows);
            _panelB.Invalidate();
            UpdateStatusB();
            if (_syncScroll) SyncAtoB();
            e.Handled = true;
        }
        // ══════════════════════════════════════════════════════════════════════
        //  Helpers
        // ══════════════════════════════════════════════════════════════════════
        private long HitTest(Point pt, long scroll)
        {
            if (pt.Y < HeaderH) return -1;
            int row = (pt.Y - HeaderH) / RowH;
            int hx = OffsetColW;
            int ax = hx + BytesPerRow * HexCellW + GutterW;
            if (pt.X >= hx && pt.X < ax)
            {
                int col = (pt.X - hx) / HexCellW;
                if (col < 0 || col >= BytesPerRow) return -1;
                return scroll + row * BytesPerRow + col;
            }
            if (pt.X >= ax)
            {
                int col = (pt.X - ax) / AsciiCellW;
                if (col < 0 || col >= BytesPerRow) return -1;
                return scroll + row * BytesPerRow + col;
            }
            return -1;
        }
        private void MoveCursor(ref long cursor, ref long scroll, KeyEventArgs e, byte[] data)
        {
            bool ctrl = (e.Modifiers & Keys.Control) != 0;
            switch (e.KeyCode)
            {
                case Keys.Right: cursor = Math.Min(cursor + 1, data.Length - 1); break;
                case Keys.Left: cursor = Math.Max(cursor - 1, 0); break;
                case Keys.Down: cursor = Math.Min(cursor + BytesPerRow, data.Length - 1); break;
                case Keys.Up: cursor = Math.Max(cursor - BytesPerRow, 0); break;
                case Keys.PageDown: cursor = Math.Min(cursor + _visibleRows * BytesPerRow, data.Length - 1); break;
                case Keys.PageUp: cursor = Math.Max(cursor - _visibleRows * BytesPerRow, 0); break;
                case Keys.Home: cursor = ctrl ? 0 : (cursor / BytesPerRow) * BytesPerRow; break;
                case Keys.End:
                    cursor = ctrl ? data.Length - 1
                        : Math.Min((cursor / BytesPerRow + 1) * BytesPerRow - 1, data.Length - 1);
                    break;
            }
            EnsureCursorVisible(ref scroll, cursor);
        }
        private void EnsureCursorVisible(ref long scroll, long cursor)
        {
            long cr = cursor / BytesPerRow;
            long tr = scroll / BytesPerRow;
            long br = tr + _visibleRows - 1;
            if (cr < tr) scroll = cr * BytesPerRow;
            else if (cr > br) scroll = (cr - _visibleRows + 1) * BytesPerRow;
        }
        private void ClampScroll(ref long scroll, byte[] data)
        {
            if (data == null) { scroll = 0; return; }
            long max = Math.Max(0,
                ((data.Length + BytesPerRow - 1) / BytesPerRow - _visibleRows) * (long)BytesPerRow);
            scroll = Math.Max(0, Math.Min(scroll, max));
        }
        private void UpdateScrollBar(VScrollBar bar, byte[] data, int visRows)
        {
            if (data == null) { bar.Maximum = 0; return; }
            int total = (data.Length + BytesPerRow - 1) / BytesPerRow;
            bar.Minimum = 0;
            bar.Maximum = Math.Max(0, total - 1);
            bar.LargeChange = Math.Max(1, visRows);
            bar.SmallChange = 1;
        }
        private void SyncBtoA()
        {
            _scrollB = _scrollA;
            ClampScroll(ref _scrollB, _bufB?.Data);
            UpdateScrollBar(_scrollBarB, _bufB?.Data, _visibleRows);
            _panelB.Invalidate();
        }
        private void SyncAtoB()
        {
            _scrollA = _scrollB;
            ClampScroll(ref _scrollA, _bufA?.Data);
            UpdateScrollBar(_scrollBarA, _bufA?.Data, _visibleRows);
            _panelA.Invalidate();
        }
        private static bool IsInSel(long idx, long start, long end)
        {
            if (start < 0 || end < 0) return false;
            long lo = Math.Min(start, end);
            long hi = Math.Max(start, end);
            return idx >= lo && idx <= hi;
        }
        private void UpdateInfoLabel(Label lbl, HexCompareBuffer buf)
        {
            lbl.Text = buf != null
                ? (lbl == _lblInfoA ? "A: " : "B: ") + buf.Label
                : (lbl == _lblInfoA ? "A: (no data)" : "B: (no data)");
        }
        private void UpdateStatusA()
        {
            if (_bufA?.Data == null) { _lblStatusA.Text = "No data."; return; }
            byte b = _bufA.Data[Math.Min(_cursorA, _bufA.Data.Length - 1)];
            long abs = _bufA.FileOffset + _cursorA;
            _lblStatusA.Text =
                $"0x{abs:X8}  │  0x{b:X2} ({b})" +
                $"  │  {_bufA.Data.Length:N0} B total";
        }
        private void UpdateStatusB()
        {
            if (_bufB?.Data == null) { _lblStatusB.Text = "No data."; return; }
            byte b = _bufB.Data[Math.Min(_cursorB, _bufB.Data.Length - 1)];
            long abs = _bufB.FileOffset + _cursorB;
            _lblStatusB.Text =
                $"0x{abs:X8}  │  0x{b:X2} ({b})" +
                $"  │  {_bufB.Data.Length:N0} B total";
        }
        private void RefreshDiffCount()
        {
            if (_bufA?.Data == null || _bufB?.Data == null)
            { _lblDiffCount.Text = ""; return; }
            long len = Math.Min(_bufA.Data.Length, _bufB.Data.Length);
            int cnt = 0;
            for (long i = 0; i < len; i++)
                if (_bufA.Data[i] != _bufB.Data[i]) cnt++;
            cnt += (int)Math.Abs(_bufA.Data.Length - _bufB.Data.Length);
            _lblDiffCount.Text =
                $"{cnt:N0} differing bytes" +
                $"  (A: {_bufA.Data.Length:N0} B  /  B: {_bufB.Data.Length:N0} B)";
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; Hide(); }
            base.OnFormClosing(e);
        }
    }
}
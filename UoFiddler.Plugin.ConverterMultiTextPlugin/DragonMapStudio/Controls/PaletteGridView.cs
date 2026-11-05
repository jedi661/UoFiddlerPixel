// /***************************************************************************
//  *
//  * Dragon Map Studio – 16×16 palette grid
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls
{
    public sealed class PaletteIndexEventArgs : EventArgs
    {
        public PaletteIndexEventArgs(int index, MouseButtons button)
        {
            Index = index;
            Button = button;
        }

        public int Index { get; }
        public MouseButtons Button { get; }
    }

    /// <summary>
    /// Shows 256 palette colours (one page); left click = colour 1, right click = colour 2.
    /// 16-bit rule sets have several pages (<see cref="Page"/>, mouse wheel).
    /// </summary>
    [ToolboxItem(true)]
    public class PaletteGridView : Control
    {
        private Color[] _colors = new Color[256];
        private bool[] _defined = new bool[256];
        private string[] _names = new string[256];
        private int _primary;
        private int _secondary = -1;
        private int _hover = -1;
        private int _page;
        private readonly ToolTip _toolTip = new ToolTip();

        public PaletteGridView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            for (int i = 0; i < 256; i++)
            {
                _colors[i] = Color.FromArgb(i, i, i);
                _names[i] = string.Empty;
            }
            BackColor = SystemColors.ControlDark;
        }

        public event EventHandler<PaletteIndexEventArgs> IndexClicked;
        public event EventHandler<PaletteIndexEventArgs> IndexDoubleClicked;

        /// <summary>The mouse moved onto another colour (-1 = none).</summary>
        public event EventHandler<PaletteIndexEventArgs> IndexHovered;

        /// <summary>Height of one colour cell in pixels.</summary>
        [Browsable(false)]
        public int CellHeight => Math.Max(1, (Height - 1) / 16);

        /// <summary>The page (256 colours each) changed.</summary>
        public event EventHandler PageChanged;

        /// <summary>Number of pages of 256 colours.</summary>
        [Browsable(false)]
        public int PageCount => Math.Max(1, (_colors.Length + 255) / 256);

        /// <summary>Shown page: colours Page*256 .. Page*256+255.</summary>
        [Category("Dragon"), DefaultValue(0)]
        public int Page
        {
            get => _page;
            set
            {
                int p = Math.Clamp(value, 0, PageCount - 1);
                if (p != _page)
                {
                    _page = p;
                    _hover = -1;
                    Invalidate();
                    PageChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private int First => _page * 256;

        private bool OnPage(int index) => index >= First && index < First + 256 && index < _colors.Length;

        [Category("Dragon"), DefaultValue(0)]
        public int PrimaryIndex
        {
            get => _primary;
            set
            {
                _primary = value;
                Invalidate();
            }
        }

        [Category("Dragon"), DefaultValue(-1)]
        public int SecondaryIndex
        {
            get => _secondary;
            set
            {
                _secondary = value;
                Invalidate();
            }
        }

        /// <summary>Show a cross over colours that are not defined in maptrans.</summary>
        [Category("Dragon"), DefaultValue(true)]
        public bool MarkUndefined { get; set; } = true;

        public void SetPalette(Color[] colors, bool[] defined, string[] names)
        {
            _colors = colors ?? _colors;
            _defined = defined ?? _defined;
            _names = names ?? _names;
            if (_page >= PageCount)
            {
                _page = PageCount - 1;
                PageChanged?.Invoke(this, EventArgs.Empty);
            }
            Invalidate();
        }

        private RectangleF CellRect(int index)
        {
            index -= First;
            float cw = (Width - 1) / 16f, ch = (Height - 1) / 16f;
            return new RectangleF(index % 16 * cw, index / 16 * ch, cw, ch);
        }

        private int HitTest(Point p)
        {
            float cw = (Width - 1) / 16f, ch = (Height - 1) / 16f;
            if (cw <= 0 || ch <= 0)
            {
                return -1;
            }
            int cx = (int)(p.X / cw), cy = (int)(p.Y / ch);
            int idx = cx < 0 || cy < 0 || cx > 15 || cy > 15 ? -1 : First + cy * 16 + cx;
            return idx >= _colors.Length ? -1 : idx;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            for (int i = First; i < First + 256 && i < _colors.Length; i++)
            {
                RectangleF r = CellRect(i);
                using (var b = new SolidBrush(_colors[i]))
                {
                    g.FillRectangle(b, r.X + 1, r.Y + 1, r.Width - 1, r.Height - 1);
                }
                if (MarkUndefined && !_defined[i])
                {
                    Color c = _colors[i].GetBrightness() > 0.5f ? Color.Black : Color.White;
                    using var pen = new Pen(Color.FromArgb(150, c));
                    g.DrawLine(pen, r.X + 2, r.Y + 2, r.Right - 2, r.Bottom - 2);
                }
            }
            DrawMarker(g, _secondary, Color.White, Color.Black, "2");
            DrawMarker(g, _primary, Color.Black, Color.Yellow, "1");
            if (_hover >= 0)
            {
                RectangleF r = CellRect(_hover);
                using var pen = new Pen(Color.Orange, 1);
                g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
            }
        }

        private void DrawMarker(Graphics g, int index, Color outer, Color inner, string text)
        {
            if (!OnPage(index))
            {
                return;
            }
            RectangleF r = CellRect(index);
            using (var p1 = new Pen(outer, 3))
            {
                g.DrawRectangle(p1, r.X + 1, r.Y + 1, r.Width - 2, r.Height - 2);
            }
            using (var p2 = new Pen(inner, 1))
            {
                g.DrawRectangle(p2, r.X + 1, r.Y + 1, r.Width - 2, r.Height - 2);
            }
            if (r.Width >= 14)
            {
                using var f = new Font(Font.FontFamily, Math.Max(6f, r.Height * 0.35f), FontStyle.Bold);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawString(text, f, Brushes.Black, r.X + 3, r.Y + 2);
                g.DrawString(text, f, Brushes.White, r.X + 2, r.Y + 1);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int idx = HitTest(e.Location);
            if (idx != _hover)
            {
                _hover = idx;
                Invalidate();
                IndexHovered?.Invoke(this, new PaletteIndexEventArgs(idx, MouseButtons.None));
                if (idx >= 0)
                {
                    string def = _defined[idx] ? string.Empty : " (nicht definiert)";
                    _toolTip.SetToolTip(this, $"{(idx > 0xFF ? idx.ToString("x4") : idx.ToString("x2"))} / {idx} – {_names[idx]}{def}");
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Page += e.Delta > 0 ? -1 : 1;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = -1;
            IndexHovered?.Invoke(this, new PaletteIndexEventArgs(-1, MouseButtons.None));
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int idx = HitTest(e.Location);
            if (idx >= 0)
            {
                IndexClicked?.Invoke(this, new PaletteIndexEventArgs(idx, e.Button));
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            int idx = HitTest(e.Location);
            if (idx >= 0)
            {
                IndexDoubleClicked?.Invoke(this, new PaletteIndexEventArgs(idx, e.Button));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toolTip.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

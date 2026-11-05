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

    /// <summary>Shows the 256 palette colours; left click = colour 1, right click = colour 2.</summary>
    [ToolboxItem(true)]
    public class PaletteGridView : Control
    {
        private Color[] _colors = new Color[256];
        private bool[] _defined = new bool[256];
        private string[] _names = new string[256];
        private int _primary;
        private int _secondary = -1;
        private int _hover = -1;
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
            Invalidate();
        }

        private RectangleF CellRect(int index)
        {
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
            return cx < 0 || cy < 0 || cx > 15 || cy > 15 ? -1 : cy * 16 + cx;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            for (int i = 0; i < 256; i++)
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
            if (index < 0 || index > 255)
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
                if (idx >= 0)
                {
                    string def = _defined[idx] ? string.Empty : " (nicht definiert)";
                    _toolTip.SetToolTip(this, $"{idx:x2} / {idx} – {_names[idx]}{def}");
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = -1;
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

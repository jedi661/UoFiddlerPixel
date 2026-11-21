// /***************************************************************************
//  *
//  * Dragon Map Studio – section with a coloured header that folds open / closed
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
    /// <summary>
    /// Header (colour stripe, arrow, title, optional small picture and header buttons) + body.
    /// Click on the header folds the body. Height = header + body height when open.
    /// </summary>
    [ToolboxItem(false)]
    public class CollapsibleSection : Panel
    {
        public const int HeaderHeight = 26;

        private readonly Panel _header;
        private readonly FlowLayoutPanel _headerButtons;
        private bool _collapsed;
        private int _bodyHeight;

        public CollapsibleSection(string title, Color accent, int bodyHeight)
        {
            Title = title;
            Accent = accent;
            _bodyHeight = bodyHeight;
            Margin = new Padding(0, 0, 0, 4);
            Body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6, 4, 6, 4), BackColor = Color.FromArgb(250, 250, 252) };
            _header = new DoubleBufferedPanel { Dock = DockStyle.Top, Height = HeaderHeight, Cursor = Cursors.Hand };
            _header.Paint += Header_Paint;
            _header.MouseClick += (s, e) => Collapsed = !Collapsed;
            _headerButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 2, 2, 0)
            };
            _header.Controls.Add(_headerButtons);
            Controls.Add(Body);
            Controls.Add(_header);
            Height = HeaderHeight + bodyHeight;
        }

        public string Title { get; set; }

        public Color Accent { get; set; }

        /// <summary>Small picture left of the title (e.g. land art), optional.</summary>
        public Image Icon { get; set; }

        public Panel Body { get; }

        public event EventHandler CollapsedChanged;

        public int BodyHeight
        {
            get => _bodyHeight;
            set
            {
                _bodyHeight = Math.Max(0, value);
                if (!_collapsed)
                {
                    Height = HeaderHeight + _bodyHeight;
                }
            }
        }

        public bool Collapsed
        {
            get => _collapsed;
            set
            {
                if (_collapsed == value)
                {
                    return;
                }
                _collapsed = value;
                Body.Visible = !value;
                Height = HeaderHeight + (value ? 0 : _bodyHeight);
                _header.Invalidate();
                CollapsedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>Small flat button in the header (e.g. "⧉" undock, "+" zoom).</summary>
        public Button AddHeaderButton(string text, string tooltip, EventHandler click)
        {
            var b = new Button
            {
                Text = text,
                Width = 24,
                Height = 20,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(1, 0, 1, 0),
                BackColor = Color.FromArgb(70, Color.White),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += click;
            new ToolTip().SetToolTip(b, tooltip);
            _headerButtons.Controls.Add(b);
            return b;
        }

        private void Header_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, _header.Width, _header.Height);
            using (var back = new LinearGradientBrush(r, Accent, ControlPaint.Dark(Accent, 0.15f), LinearGradientMode.Horizontal))
            {
                g.FillRectangle(back, r);
            }
            // arrow
            int cx = 12, cy = HeaderHeight / 2;
            Point[] arrow = _collapsed
                ? new[] { new Point(cx - 3, cy - 5), new Point(cx + 3, cy), new Point(cx - 3, cy + 5) }
                : new[] { new Point(cx - 5, cy - 3), new Point(cx + 5, cy - 3), new Point(cx, cy + 3) };
            g.FillPolygon(Brushes.White, arrow);
            int x = 22;
            if (Icon != null)
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(Icon, new Rectangle(x, 3, 20, 20));
                x += 24;
            }
            using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
            TextRenderer.DrawText(g, Title, font, new Rectangle(x, 0, _header.Width - x - _headerButtons.Width - 4, HeaderHeight), Color.White,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        private sealed class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }
        }
    }
}

/***************************************************************************
 *
 *  * Dragon Map Studio – tool palette like Photoshop: a narrow column of tool pictures at the left of the map,
 *  * groups with a thin line, tool tip with name, key and help; ⧉ undocks it into its own window
 *  * (closing the window docks it back at the left)
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls
{
    /// <summary>One tool of a <see cref="ToolPalette"/>.</summary>
    public sealed class ToolPaletteEntry
    {
        /// <summary>Picture key of <see cref="ToolIcons"/> and key of the help topic.</summary>
        public string Key { get; set; }

        public string Name { get; set; }

        /// <summary>Key on the keyboard (shown in the tool tip), may be empty.</summary>
        public string Shortcut { get; set; }

        /// <summary>Short help: mouse buttons and keys of the tool.</summary>
        public string Help { get; set; }

        public Color Accent { get; set; } = Color.FromArgb(37, 99, 235);

        public Func<bool> IsChecked { get; set; }

        public Func<bool> IsEnabled { get; set; }

        public Action Click { get; set; }

        /// <summary>Own picture instead of the drawn one (null = <see cref="ToolIcons"/>).</summary>
        public Func<Image> Icon { get; set; }

        internal Rectangle Bounds;
        internal bool Separator;
    }

    /// <summary>Column(s) of tool buttons; the number of columns follows the width.</summary>
    [ToolboxItem(false)]
    public sealed class ToolPalette : Control
    {
        public const int Cell = 34;
        public const int HeaderHeight = 22;
        private const int Pad = 3;

        private readonly List<ToolPaletteEntry> _entries = new List<ToolPaletteEntry>();
        private readonly ToolTip _tip = new ToolTip { InitialDelay = 350, ReshowDelay = 100, AutoPopDelay = 15000 };
        private ToolPaletteEntry _hover;
        private bool _hoverDock;
        private Rectangle _dockButton;

        public ToolPalette(string title)
        {
            Title = title;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.FromArgb(226, 232, 240);
            Width = PreferredColumnsWidth(2);
            Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
        }

        public string Title { get; set; }

        /// <summary>Translation of the (German) names and help texts at the moment they are shown.</summary>
        public static Func<string, string> Translate { get; set; } = t => t;

        private static string Tr(string t) => string.IsNullOrEmpty(t) ? t : Translate(t);

        /// <summary>True while the palette sits in its own window (the ⧉ button then docks it back).</summary>
        public bool Floating { get; set; }

        /// <summary>⧉ clicked: undock into a window, or dock back when floating.</summary>
        public event EventHandler DockToggleRequested;

        /// <summary>Right click on a tool: help for it.</summary>
        public event EventHandler<ToolPaletteEntry> ToolHelpRequested;

        /// <summary>Width for a number of columns.</summary>
        public static int PreferredColumnsWidth(int columns) => columns * Cell + 2 * Pad + 2;

        public IReadOnlyList<ToolPaletteEntry> Entries => _entries;

        public ToolPaletteEntry Add(ToolPaletteEntry e)
        {
            _entries.Add(e);
            PerformLayout();
            Invalidate();
            return e;
        }

        /// <summary>Thin line: starts a new group in a new row.</summary>
        public void AddSeparator()
        {
            _entries.Add(new ToolPaletteEntry { Separator = true });
        }

        /// <summary>Checked / enabled state changed (tool chosen somewhere else).</summary>
        public void RefreshState() => Invalidate();

        /// <summary>Height that shows all tools at the current width.</summary>
        public int NeededHeight
        {
            get
            {
                LayoutEntries();
                int h = HeaderHeight;
                foreach (ToolPaletteEntry e in _entries)
                {
                    if (!e.Separator)
                    {
                        h = Math.Max(h, e.Bounds.Bottom);
                    }
                }
                return h + Pad + 2;
            }
        }

        private int Columns => Math.Max(1, (ClientSize.Width - 2 * Pad) / Cell);

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            LayoutEntries();
        }

        private void LayoutEntries()
        {
            int cols = Columns, col = 0;
            int left = Math.Max(Pad, (ClientSize.Width - cols * Cell) / 2);
            int y = HeaderHeight + Pad;
            foreach (ToolPaletteEntry e in _entries)
            {
                if (e.Separator)
                {
                    if (col > 0)
                    {
                        y += Cell;
                        col = 0;
                    }
                    e.Bounds = new Rectangle(left, y + 2, cols * Cell, 3);
                    y += 7;
                    continue;
                }
                e.Bounds = new Rectangle(left + col * Cell, y, Cell, Cell);
                col++;
                if (col >= cols)
                {
                    col = 0;
                    y += Cell;
                }
            }
            _dockButton = new Rectangle(ClientSize.Width - 20, 2, 18, HeaderHeight - 4);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            // header: title + dock button
            using (var head = new SolidBrush(Color.FromArgb(51, 65, 85)))
            {
                g.FillRectangle(head, 0, 0, ClientSize.Width, HeaderHeight);
            }
            string title = ClientSize.Width >= 90 ? Tr(Title) : string.Empty;
            TextRenderer.DrawText(g, title, Font, new Rectangle(4, 0, ClientSize.Width - 26, HeaderHeight), Color.White,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            using (var hb = new SolidBrush(_hoverDock ? Color.FromArgb(100, 116, 139) : Color.FromArgb(51, 65, 85)))
            {
                g.FillRectangle(hb, _dockButton);
            }
            TextRenderer.DrawText(g, Floating ? "⇤" : "⧉", Font, _dockButton, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            foreach (ToolPaletteEntry t in _entries)
            {
                if (t.Separator)
                {
                    using var line = new Pen(Color.FromArgb(148, 163, 184));
                    g.DrawLine(line, t.Bounds.Left + 3, t.Bounds.Top + 1, t.Bounds.Right - 3, t.Bounds.Top + 1);
                    continue;
                }
                bool on = SafeBool(t.IsChecked, false);
                bool enabled = SafeBool(t.IsEnabled, true);
                Rectangle r = Rectangle.Inflate(t.Bounds, -2, -2);
                if (on || t == _hover)
                {
                    using var path = Round(r, 5);
                    using var fill = new SolidBrush(on ? Color.FromArgb(70, t.Accent) : Color.FromArgb(40, 255, 255, 255));
                    g.FillPath(fill, path);
                    using var border = new Pen(on ? t.Accent : Color.FromArgb(148, 163, 184), on ? 2f : 1f);
                    g.DrawPath(border, path);
                }
                Image img = null;
                try
                {
                    img = t.Icon?.Invoke();
                }
                catch (Exception)
                {
                    img = null;
                }
                img ??= ToolIcons.Get(t.Key ?? t.Name ?? "?");
                var dest = new Rectangle(r.X + (r.Width - ToolIcons.Size) / 2, r.Y + (r.Height - ToolIcons.Size) / 2, ToolIcons.Size, ToolIcons.Size);
                if (enabled)
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(img, dest);
                }
                else
                {
                    ControlPaint.DrawImageDisabled(g, img, dest.X, dest.Y, BackColor);
                }
                if (!string.IsNullOrEmpty(t.Shortcut) && t.Shortcut.Length == 1)
                {
                    using var small = new Font("Segoe UI", 6f, FontStyle.Bold);
                    TextRenderer.DrawText(g, t.Shortcut, small, new Rectangle(r.Right - 9, r.Bottom - 10, 9, 10), Color.FromArgb(71, 85, 105),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
            }
        }

        private static bool SafeBool(Func<bool> f, bool fallback)
        {
            try
            {
                return f?.Invoke() ?? fallback;
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        private static GraphicsPath Round(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = radius * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        private ToolPaletteEntry EntryAt(Point p)
        {
            foreach (ToolPaletteEntry e in _entries)
            {
                if (!e.Separator && e.Bounds.Contains(p))
                {
                    return e;
                }
            }
            return null;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            ToolPaletteEntry h = EntryAt(e.Location);
            bool dock = _dockButton.Contains(e.Location);
            if (h != _hover || dock != _hoverDock)
            {
                _hover = h;
                _hoverDock = dock;
                Invalidate();
                if (h != null)
                {
                    string text = Tr(h.Name) + (string.IsNullOrEmpty(h.Shortcut) ? string.Empty : "  (" + h.Shortcut + ")") +
                                  (string.IsNullOrEmpty(h.Help) ? string.Empty : Environment.NewLine + Tr(h.Help)) +
                                  Environment.NewLine + Tr("Rechtsklick = Hilfe");
                    _tip.Show(text, this, h.Bounds.Right + 4, h.Bounds.Top, 12000);
                }
                else if (dock)
                {
                    _tip.Show(Tr(Floating ? DockBackText : UndockText), this, _dockButton.Left, _dockButton.Bottom + 2, 4000);
                }
                else
                {
                    _tip.Hide(this);
                }
            }
            Cursor = h != null || dock ? Cursors.Hand : Cursors.Default;
        }

        /// <summary>Tool tip of ⧉ (set by the form, translated).</summary>
        public string UndockText { get; set; } = "Werkzeuge in einem eigenen Fenster (abdocken)";

        public string DockBackText { get; set; } = "Wieder links andocken";

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = null;
            _hoverDock = false;
            _tip.Hide(this);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_dockButton.Contains(e.Location) && e.Button == MouseButtons.Left)
            {
                DockToggleRequested?.Invoke(this, EventArgs.Empty);
                return;
            }
            ToolPaletteEntry t = EntryAt(e.Location);
            if (t == null)
            {
                return;
            }
            if (e.Button == MouseButtons.Right)
            {
                ToolHelpRequested?.Invoke(this, t);
                return;
            }
            if (e.Button == MouseButtons.Left && SafeBool(t.IsEnabled, true))
            {
                _tip.Hide(this);
                t.Click?.Invoke();
                Invalidate();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tip.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Keeps a palette at the left of a host control or in its own small window. Closing the window docks it back.
    /// </summary>
    public sealed class ToolPaletteDock
    {
        private readonly ToolPalette _palette;
        private readonly Control _host;
        private readonly Form _owner;
        private Form _window;

        public ToolPaletteDock(ToolPalette palette, Control host, Form owner)
        {
            _palette = palette;
            _host = host;
            _owner = owner;
            palette.DockToggleRequested += (s, e) =>
            {
                if (_window != null)
                {
                    _window.Close();
                }
                else
                {
                    Undock(null);
                }
            };
            DockBack();
        }

        public bool Floating => _window != null;

        /// <summary>Window position while floating (for the settings), empty when docked.</summary>
        public Rectangle WindowBounds => _window?.Bounds ?? Rectangle.Empty;

        /// <summary>Floating state changed (to remember it).</summary>
        public event EventHandler Changed;

        public void Undock(Rectangle? bounds)
        {
            if (_window != null)
            {
                return;
            }
            int width = ToolPalette.PreferredColumnsWidth(2);
            var f = new Form
            {
                Text = ToolPalette.Translate(_palette.Title ?? string.Empty),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                MinimizeBox = false,
                MaximizeBox = false,
                Owner = _owner
            };
            _palette.Parent = null;
            _palette.Dock = DockStyle.Fill;
            _palette.Floating = true;
            f.Controls.Add(_palette);
            Point screen = _host.PointToScreen(Point.Empty);
            Rectangle want = bounds ?? new Rectangle(screen.X + 8, screen.Y + 40, width + 16, _palette.NeededHeight + 40);
            // only on a visible screen
            if (!IsOnScreen(want))
            {
                want = new Rectangle(screen.X + 8, screen.Y + 40, width + 16, _palette.NeededHeight + 40);
            }
            f.Bounds = want;
            bool ownerClosing = false;
            f.FormClosing += (o, e) => ownerClosing = e.CloseReason == CloseReason.FormOwnerClosing || e.CloseReason == CloseReason.ApplicationExitCall;
            f.FormClosed += (o, e) =>
            {
                _window = null;
                if (ownerClosing)
                {
                    return; // the studio closes: the window place stays remembered for the next start
                }
                DockBack();
                Changed?.Invoke(this, EventArgs.Empty);
            };
            f.ResizeEnd += (o, e) => Changed?.Invoke(this, EventArgs.Empty);
            f.Move += (o, e) => Changed?.Invoke(this, EventArgs.Empty);
            _window = f;
            f.Show(_owner);
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private static bool IsOnScreen(Rectangle r)
        {
            foreach (Screen s in Screen.AllScreens)
            {
                if (s.WorkingArea.IntersectsWith(r))
                {
                    return true;
                }
            }
            return false;
        }

        private void DockBack()
        {
            _palette.Parent = null;
            _palette.Floating = false;
            _palette.Dock = DockStyle.Left;
            _palette.Width = ToolPalette.PreferredColumnsWidth(2);
            _host.Controls.Add(_palette);
            // the palette is the outermost left control: behind (= docked before) the map
            _palette.SendToBack();
            _palette.Invalidate();
        }
    }
}

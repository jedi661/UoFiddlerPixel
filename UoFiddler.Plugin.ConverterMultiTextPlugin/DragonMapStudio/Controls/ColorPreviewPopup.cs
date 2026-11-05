// /***************************************************************************
//  *
//  * Dragon Map Studio – preview of a colour: land tiles / textures, items and statics it stands for
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
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls
{
    /// <summary>
    /// Small window next to the mouse: what a colour address creates in the map – land tiles (with texture),
    /// the items of its group and the statics of its static transitions. Shown below the hovered row, so the row stays visible.
    /// </summary>
    public sealed class ColorPreviewPopup : Form
    {
        private const int Thumb = 44;
        private const int MaxPerRow = 8;
        private DragonRuleSet _rules;
        private int _index = -1;
        private List<ushort> _tiles = new List<ushort>();
        private List<ushort> _items = new List<ushort>();
        private List<ushort> _transitionStatics = new List<ushort>();
        private readonly Font _bold = new Font("Segoe UI", 9f, FontStyle.Bold);
        private readonly Font _small = new Font("Segoe UI", 7.5f);

        public ColorPreviewPopup()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            BackColor = Color.FromArgb(32, 32, 36);
            ForeColor = Color.Gainsboro;
            DoubleBuffered = true;
            Size = new Size(380, 120);
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000 | 0x00000080; // WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW
                return cp;
            }
        }

        public int Index => _index;

        /// <summary>
        /// Shows the preview of a colour. <paramref name="cursor"/> = mouse position (screen), <paramref name="rowHeight"/> = height of the
        /// hovered row – the window appears below it (or above, when there is no room).
        /// </summary>
        public void ShowFor(DragonRuleSet rules, int index, Point cursor, int rowHeight)
        {
            if (rules == null || index < 0 || index >= rules.Palette.Count)
            {
                HidePreview();
                return;
            }
            if (index != _index || rules != _rules)
            {
                _rules = rules;
                _index = index;
                DragonPaletteEntry e = rules.Palette[index];
                _tiles = e.GetTileIds().Distinct().Take(MaxPerRow * 2).ToList();
                _items = e.Group >= 0 ? RuleSetTools.ItemIdsOfGroup(rules, e.Group, MaxPerRow * 2) : new List<ushort>();
                _transitionStatics = e.Group >= 0 ? RuleSetTools.TransitionStaticIdsOfGroup(rules, e.Group, MaxPerRow) : new List<ushort>();
                int rows = 0;
                rows += (_tiles.Count + MaxPerRow - 1) / MaxPerRow;
                rows += (_items.Count + MaxPerRow - 1) / MaxPerRow;
                rows += (_transitionStatics.Count + MaxPerRow - 1) / MaxPerRow;
                int sections = (_tiles.Count > 0 ? 1 : 0) + (_items.Count > 0 ? 1 : 0) + (_transitionStatics.Count > 0 ? 1 : 0);
                int width = 12 + MaxPerRow * (Thumb + 4);
                int height = 58 + rows * (Thumb + 16) + sections * 18;
                Size = new Size(width, Math.Max(70, height));
                Invalidate();
            }
            Rectangle screen = Screen.FromPoint(cursor).WorkingArea;
            int x = Math.Min(cursor.X + 18, screen.Right - Width - 4);
            int y = cursor.Y + Math.Max(16, rowHeight) + 6;
            if (y + Height > screen.Bottom)
            {
                y = cursor.Y - Math.Max(16, rowHeight) - Height - 6;
            }
            Location = new Point(Math.Max(screen.Left, x), Math.Max(screen.Top, y));
            if (!Visible)
            {
                Show();
            }
        }

        public void HidePreview()
        {
            _index = -1;
            if (Visible)
            {
                Hide();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            using (var border = new Pen(Color.FromArgb(90, 90, 100)))
            {
                g.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
            }
            if (_rules == null || _index < 0)
            {
                return;
            }
            DragonPaletteEntry p = _rules.Palette[_index];
            using (var b = new SolidBrush(p.Color))
            {
                g.FillRectangle(b, 8, 8, 34, 34);
            }
            g.DrawRectangle(Pens.Black, 8, 8, 34, 34);
            string group = p.Group >= 0 ? $"{_rules.HexGroup(p.Group)} {_rules.GroupName(p.Group)}" : L.T("keine Gruppe");
            TextRenderer.DrawText(g, $"{_rules.HexIndex(_index)}  {p.Name}", _bold, new Point(50, 6), Color.White);
            TextRenderer.DrawText(g, $"{L.T("Gruppe")}: {group}  ·  Z {p.Altitude}  ·  {p.HtmlColor}" + (p.Defined ? string.Empty : "  ·  " + L.T("nicht definiert")),
                _small, new Point(50, 26), ForeColor);
            int y = 50;
            y = DrawSection(g, y, L.T("Land-Tiles / Texturen"), _tiles, true);
            y = DrawSection(g, y, L.T("Items der Gruppe"), _items, false);
            DrawSection(g, y, L.T("Statics der Übergänge"), _transitionStatics, false);
        }

        private int DrawSection(Graphics g, int y, string title, List<ushort> ids, bool land)
        {
            if (ids.Count == 0)
            {
                return y;
            }
            TextRenderer.DrawText(g, title, _small, new Point(8, y), Color.FromArgb(255, 200, 120));
            y += 16;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            for (int i = 0; i < ids.Count; i++)
            {
                int col = i % MaxPerRow, row = i / MaxPerRow;
                var cell = new Rectangle(8 + col * (Thumb + 4), y + row * (Thumb + 16), Thumb, Thumb);
                using (var bg = new SolidBrush(Color.FromArgb(48, 48, 54)))
                {
                    g.FillRectangle(bg, cell);
                }
                Image img = null;
                try
                {
                    img = land ? (Image)UoArtCache.Land(ids[i]) ?? UoArtCache.Texture(ids[i]) : UoArtCache.Static(ids[i], 0);
                }
                catch (Exception)
                {
                    img = null;
                }
                if (img != null)
                {
                    float k = Math.Min(1f, Math.Min(Thumb / (float)img.Width, Thumb / (float)img.Height));
                    int w = Math.Max(1, (int)(img.Width * k)), h = Math.Max(1, (int)(img.Height * k));
                    g.DrawImage(img, new Rectangle(cell.X + (Thumb - w) / 2, cell.Bottom - h, w, h));
                }
                TextRenderer.DrawText(g, ids[i].ToString("x4"), _small, new Rectangle(cell.X - 2, cell.Bottom, Thumb + 4, 14), ForeColor, TextFormatFlags.HorizontalCenter);
            }
            return y + (ids.Count + MaxPerRow - 1) / MaxPerRow * (Thumb + 16) + 2;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bold.Dispose();
                _small.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

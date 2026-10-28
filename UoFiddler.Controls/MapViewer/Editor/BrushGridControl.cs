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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Controls.MapViewer.Build;
using UoFiddler.Controls.MapViewer.Editor.Tools;

namespace UoFiddler.Controls.MapViewer.Editor
{
    /// <summary>
    /// The brush as a grid (top view, x right, y down), like it is painted on the map:
    /// which tile lies at which position, numbers per cell, cells outside of a round brush grey.
    /// Left mouse = palette tile(s) into a cell, right = empty, Ctrl = take into the palette,
    /// <see cref="MoveMode"/> or Shift + drag = move a cell (swap), drag &amp; drop from the palette.
    /// </summary>
    public class BrushGridControl : Control
    {
        private Point? _moveFrom;
        private Point? _hover;
        private Point _lastPaint = new Point(-1, -1);

        public BrushGridControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.FromArgb(16, 16, 20);
            AllowDrop = true;
        }

        /// <summary>the edited pattern (null = show <see cref="RandomTiles"/> as example)</summary>
        public BrushPattern Pattern { get; set; }

        /// <summary>textures of a random brush (only shown, an example of the distribution)</summary>
        public IReadOnlyList<ushort> RandomTiles { get; set; }

        public int BrushSize { get; set; } = 3;
        public BrushShape Shape { get; set; } = BrushShape.Square;
        public bool ShowNumbers { get; set; } = true;
        public bool ShowIds { get; set; }
        public bool MoveMode { get; set; }
        public bool ReadOnly { get; set; }
        public int MaxCellSize { get; set; } = 64;

        public Func<IReadOnlyList<ushort>> Selection { get; set; }
        public Func<ushort, Bitmap> Image { get; set; }

        public event EventHandler PatternChanged;

        /// <summary>Ctrl+click: the tiles of a cell</summary>
        public event EventHandler<CellTilesEventArgs> CellPicked;

        private int Columns => Pattern?.Width ?? Math.Max(1, BrushSize);
        private int Rows => Pattern?.Height ?? Math.Max(1, BrushSize);

        private int CellSize => Math.Max(8, Math.Min(MaxCellSize, Math.Min((ClientSize.Width - 8) / Columns, (ClientSize.Height - 8) / Rows)));

        private Point Origin
        {
            get
            {
                int cs = CellSize;
                return new Point(Math.Max(4, (ClientSize.Width - (cs * Columns)) / 2), Math.Max(4, (ClientSize.Height - (cs * Rows)) / 2));
            }
        }

        /// <summary>cells that the brush paints (round brushes leave out the corners)</summary>
        private HashSet<Point> Mask()
        {
            if (Pattern != null && (Pattern.Width != BrushSize || Pattern.Height != BrushSize))
            {
                // the pattern is repeated / cut by the brush: every cell can be painted
                return null;
            }

            int size = Math.Max(1, BrushSize);
            return new HashSet<Point>(MapBrush.Cells((size - 1) / 2, (size - 1) / 2, size, Shape, size, size));
        }

        private Point? CellAt(Point pt)
        {
            int cs = CellSize;
            Point o = Origin;
            int x = (pt.X - o.X) / cs, y = (pt.Y - o.Y) / cs;
            if (pt.X < o.X || pt.Y < o.Y || x >= Columns || y >= Rows)
            {
                return null;
            }

            return new Point(x, y);
        }

        #region Paint

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (Pattern == null && (RandomTiles == null || RandomTiles.Count == 0))
            {
                g.DrawString("Empty brush: click tiles in the palette.", Font, Brushes.Gray, 6, 6);
                return;
            }

            int cs = CellSize;
            Point o = Origin;
            HashSet<Point> mask = Mask();
            HashSet<Point> statics = Pattern == null ? new HashSet<Point>() : new HashSet<Point>(Pattern.Statics.Select(s => new Point(s.X, s.Y)));
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;

            using (var font = new Font("Segoe UI", Math.Max(6.5f, Math.Min(10f, cs / 5f)), FontStyle.Bold))
            using (var empty = new SolidBrush(Color.FromArgb(32, 36, 60)))
            using (var outside = new HatchBrush(HatchStyle.BackwardDiagonal, Color.FromArgb(60, 60, 60), Color.FromArgb(22, 22, 26)))
            using (var shade = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
            using (var border = new Pen(Color.FromArgb(110, Color.Goldenrod)))
            using (var textBack = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
            {
                for (int y = 0; y < Rows; y++)
                {
                    for (int x = 0; x < Columns; x++)
                    {
                        var r = new Rectangle(o.X + (x * cs), o.Y + (y * cs), cs, cs);
                        bool inBrush = mask == null || mask.Contains(new Point(x, y));
                        ushort? id = Pattern != null ? Pattern.First(x, y) : Transitions.TransitionEngine.Pick(RandomTiles, x, y);
                        Bitmap bmp = id.HasValue ? Image?.Invoke(id.Value) : null;
                        if (!inBrush)
                        {
                            g.FillRectangle(outside, r);
                        }
                        else if (bmp != null)
                        {
                            g.DrawImage(bmp, r);
                        }
                        else
                        {
                            g.FillRectangle(empty, r);
                        }

                        if (!inBrush && bmp != null)
                        {
                            // painted only when the pattern is larger than the brush
                            g.DrawImage(bmp, r);
                            g.FillRectangle(shade, r);
                        }

                        g.DrawRectangle(border, r);

                        if (ShowNumbers && cs >= 16)
                        {
                            string n = ((y * Columns) + x + 1).ToString();
                            SizeF size = g.MeasureString(n, font);
                            g.FillRectangle(textBack, r.X + 1, r.Y + 1, size.Width, size.Height);
                            g.DrawString(n, font, Brushes.White, r.X + 1, r.Y + 1);
                        }

                        if (ShowIds && id.HasValue && cs >= 28)
                        {
                            string text = $"{id.Value:X}";
                            SizeF size = g.MeasureString(text, font);
                            g.FillRectangle(textBack, r.X + 1, r.Bottom - size.Height - 1, size.Width, size.Height);
                            g.DrawString(text, font, Brushes.Khaki, r.X + 1, r.Bottom - size.Height - 1);
                        }

                        if (Pattern != null)
                        {
                            int variants = Pattern.GetIds(x, y).Count;
                            if (variants > 1 && cs >= 20)
                            {
                                g.DrawString($"x{variants}", font, Brushes.DeepSkyBlue, r.Right - g.MeasureString($"x{variants}", font).Width - 1, r.Y + 1);
                            }

                            if (Pattern.GetZ(x, y) != 0 && cs >= 24 && !ShowIds)
                            {
                                g.DrawString($"z{Pattern.GetZ(x, y)}", font, Brushes.Khaki, r.X + 1, r.Bottom - font.Height - 1);
                            }

                            if (statics.Contains(new Point(x, y)))
                            {
                                g.FillEllipse(Brushes.OrangeRed, r.Right - 9, r.Bottom - 9, 6, 6);
                            }
                        }
                    }
                }

                // cell under the mouse when the stroke starts
                int cx = (Columns - 1) / 2, cy = (Rows - 1) / 2;
                using (var center = new Pen(Color.DeepSkyBlue, 2))
                {
                    g.DrawRectangle(center, o.X + (cx * cs) + 1, o.Y + (cy * cs) + 1, cs - 2, cs - 2);
                }

                if (_moveFrom.HasValue)
                {
                    using (var from = new Pen(Color.Orange, 3))
                    {
                        g.DrawRectangle(from, o.X + (_moveFrom.Value.X * cs) + 1, o.Y + (_moveFrom.Value.Y * cs) + 1, cs - 2, cs - 2);
                    }

                    if (_hover.HasValue && _hover != _moveFrom)
                    {
                        using (var to = new Pen(Color.Lime, 3) { DashStyle = DashStyle.Dash })
                        {
                            g.DrawRectangle(to, o.X + (_hover.Value.X * cs) + 1, o.Y + (_hover.Value.Y * cs) + 1, cs - 2, cs - 2);
                        }
                    }
                }

                if (Pattern == null)
                {
                    g.DrawString("random - example", font, Brushes.Khaki, o.X, Math.Max(0, o.Y - font.Height - 1));
                }
            }
        }

        #endregion

        #region Mouse

        private bool Editable => Pattern != null && !ReadOnly;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            Point? cell = CellAt(e.Location);
            if (!Editable || cell == null)
            {
                return;
            }

            Point c = cell.Value;
            if ((ModifierKeys & Keys.Control) != 0)
            {
                CellPicked?.Invoke(this, new CellTilesEventArgs(c, Pattern.GetIds(c.X, c.Y).ToList()));
                return;
            }

            if (e.Button == MouseButtons.Left && (MoveMode || (ModifierKeys & Keys.Shift) != 0))
            {
                if (!Pattern.IsEmpty(c.X, c.Y) || Pattern.StaticsAt(c.X, c.Y).Count > 0)
                {
                    _moveFrom = c;
                    Invalidate();
                }

                return;
            }

            _lastPaint = c;
            EditCell(c, e.Button);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Point? cell = CellAt(e.Location);
            if (cell != _hover)
            {
                _hover = cell;
                if (_moveFrom.HasValue)
                {
                    Invalidate();
                }
            }

            if (!Editable || cell == null || _moveFrom.HasValue || e.Button == MouseButtons.None || (ModifierKeys & Keys.Control) != 0)
            {
                return;
            }

            if (cell.Value != _lastPaint)
            {
                _lastPaint = cell.Value;
                EditCell(cell.Value, e.Button);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_moveFrom.HasValue)
            {
                Point? to = CellAt(e.Location);
                if (to.HasValue && to.Value != _moveFrom.Value && Editable)
                {
                    Pattern.Swap(_moveFrom.Value.X, _moveFrom.Value.Y, to.Value.X, to.Value.Y);
                    PatternChanged?.Invoke(this, EventArgs.Empty);
                }

                _moveFrom = null;
                Invalidate();
            }

            _lastPaint = new Point(-1, -1);
        }

        private void EditCell(Point c, MouseButtons button)
        {
            if (button == MouseButtons.Right)
            {
                Pattern.Clear(c.X, c.Y);
            }
            else if (button == MouseButtons.Left)
            {
                IReadOnlyList<ushort> sel = Selection?.Invoke();
                if (sel == null || sel.Count == 0)
                {
                    return;
                }

                Pattern.SetIds(c.X, c.Y, sel);
            }
            else
            {
                return;
            }

            Invalidate();
            PatternChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = null;
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            e.Effect = Editable && LandPaletteControl.TryGetDragId(e.Data, out _) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            Point? c = CellAt(PointToClient(new Point(e.X, e.Y)));
            if (Editable && c.HasValue && LandPaletteControl.TryGetDragId(e.Data, out ushort id))
            {
                Pattern.SetId(c.Value.X, c.Value.Y, id);
                Invalidate();
                PatternChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Iso picture

        /// <summary>
        /// The pattern like on the map (diamonds, with heights and statics), fitted into <paramref name="size"/>,
        /// optionally with the cell numbers.
        /// </summary>
        public static Bitmap RenderIso(BrushPattern pattern, ArtPixelCache art, Size size, Color background, bool numbers, HashSet<Point> mask)
        {
            var bmp = new Bitmap(Math.Max(1, size.Width), Math.Max(1, size.Height));
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(background);
                if (pattern == null)
                {
                    return bmp;
                }

                MapClip clip = pattern.ToClip();
                if (mask != null)
                {
                    for (int y = 0; y < pattern.Height; y++)
                    {
                        for (int x = 0; x < pattern.Width; x++)
                        {
                            if (!mask.Contains(new Point(x, y)))
                            {
                                clip.SetLand(x, y, 2, clip.GetLandZ(x, y));
                            }
                        }
                    }
                }

                using (MultiImage image = ClipPreview.Render(clip, art, null))
                {
                    if (image?.Bitmap == null)
                    {
                        g.DrawString(pattern.FilledCells == 0 ? "empty" : "too large", SystemFonts.DefaultFont, Brushes.Gray, 6, 6);
                        return bmp;
                    }

                    float scale = Math.Min(2f, Math.Min((size.Width - 8) / (float)image.Bitmap.Width, (size.Height - 8) / (float)image.Bitmap.Height));
                    float w = image.Bitmap.Width * scale, h = image.Bitmap.Height * scale;
                    float ox = (size.Width - w) / 2, oy = (size.Height - h) / 2;
                    g.InterpolationMode = scale >= 1 ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
                    g.DrawImage(image.Bitmap, ox, oy, w, h);

                    if (numbers)
                    {
                        using (var font = new Font("Segoe UI", Math.Max(6.5f, Math.Min(10f, 22 * scale / 2.5f)), FontStyle.Bold))
                        using (var back = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
                        {
                            for (int y = 0; y < pattern.Height; y++)
                            {
                                for (int x = 0; x < pattern.Width; x++)
                                {
                                    if (mask != null && !mask.Contains(new Point(x, y)))
                                    {
                                        continue;
                                    }

                                    // middle of the diamond of the cell
                                    float cx = ox + ((((x - y) * RegionData.TileHalf) + image.OriginX) * scale);
                                    float cy = oy + ((((x + y) * RegionData.TileHalf) + RegionData.TileHalf - (pattern.GetZ(x, y) * RegionData.ZScale) + image.OriginY) * scale);
                                    string n = ((y * pattern.Width) + x + 1).ToString();
                                    SizeF ts = g.MeasureString(n, font);
                                    g.FillRectangle(back, cx - (ts.Width / 2), cy - (ts.Height / 2), ts.Width, ts.Height);
                                    g.DrawString(n, font, Brushes.White, cx - (ts.Width / 2), cy - (ts.Height / 2));
                                }
                            }
                        }
                    }
                }
            }

            return bmp;
        }

        #endregion
    }

    public sealed class CellTilesEventArgs : EventArgs
    {
        public CellTilesEventArgs(Point cell, IReadOnlyList<ushort> tiles)
        {
            Cell = cell;
            Tiles = tiles;
        }

        public Point Cell { get; }
        public IReadOnlyList<ushort> Tiles { get; }
    }
}

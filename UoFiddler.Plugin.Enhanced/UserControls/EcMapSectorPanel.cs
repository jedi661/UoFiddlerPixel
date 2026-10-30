/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Imaging;
using UoFiddler.Plugin.Enhanced.EcUltima.Maps;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// One 64 x 64 sector of an Enhanced Client map, drawn with the radar colors; click / arrow keys select a tile.
    /// </summary>
    public sealed class EcMapSectorPanel : Control
    {
        public EcSector Sector;
        public byte[] Colors;           // BGRA 64 x 64 from the radar image (may be null)
        public int CellSize = 9;
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Point Selected { get; set; } = new Point(-1, -1);
        public bool Shading = true;
        public event Action<int, int> TileClicked;
        public event Action<int, int> TileHover;

        public EcMapSectorPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.DimGray;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (Sector == null)
            {
                TextRenderer.DrawText(g, "No sector (click on the map).", Font, new Point(10, 10), Color.White);
                return;
            }

            using (var brush = new SolidBrush(Color.Black))
            using (var staticBrush = new SolidBrush(Color.FromArgb(220, 255, 60, 30)))
            {
                for (int x = 0; x < EcSector.Size; x++)
                {
                    for (int y = 0; y < EcSector.Size; y++)
                    {
                        EcMapTile tile = Sector[x, y];
                        Color c = TileColor(x, y, tile);
                        brush.Color = c;
                        g.FillRectangle(brush, x * CellSize, y * CellSize, CellSize, CellSize);
                        if (tile.Statics.Count > 0 && CellSize >= 4)
                        {
                            int s = Math.Max(2, CellSize / 3);
                            g.FillRectangle(staticBrush, (x * CellSize) + ((CellSize - s) / 2), (y * CellSize) + ((CellSize - s) / 2), s, s);
                        }
                    }
                }
            }

            if (Selected.X >= 0)
            {
                using (var pen = new Pen(Color.Yellow, 2))
                {
                    g.DrawRectangle(pen, Selected.X * CellSize, Selected.Y * CellSize, CellSize, CellSize);
                }
            }
        }

        private Color TileColor(int x, int y, EcMapTile tile)
        {
            int r, gr, b;
            if (Colors != null)
            {
                int o = ((y * EcSector.Size) + x) * 4;
                b = Colors[o];
                gr = Colors[o + 1];
                r = Colors[o + 2];
            }
            else
            {
                // no radar image: a color per land id
                int h = (int)((tile.LandId * 2654435761u) >> 8);
                r = 60 + (h & 0x7F);
                gr = 60 + ((h >> 7) & 0x7F);
                b = 60 + ((h >> 14) & 0x7F);
            }

            if (Shading)
            {
                // lighter = higher
                double f = 1.0 + (tile.Z / 160.0);
                r = Math.Max(0, Math.Min(255, (int)(r * f)));
                gr = Math.Max(0, Math.Min(255, (int)(gr * f)));
                b = Math.Max(0, Math.Min(255, (int)(b * f)));
            }
            return Color.FromArgb(r, gr, b);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int x = e.X / CellSize, y = e.Y / CellSize;
            if (Sector != null && x >= 0 && y >= 0 && x < EcSector.Size && y < EcSector.Size)
            {
                Selected = new Point(x, y);
                Invalidate();
                TileClicked?.Invoke(x, y);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int x = e.X / CellSize, y = e.Y / CellSize;
            if (x >= 0 && y >= 0 && x < EcSector.Size && y < EcSector.Size)
            {
                TileHover?.Invoke(x, y);
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            Keys k = keyData & Keys.KeyCode;
            return k == Keys.Left || k == Keys.Right || k == Keys.Up || k == Keys.Down || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (Sector == null || Selected.X < 0)
            {
                return;
            }
            int x = Selected.X, y = Selected.Y;
            switch (e.KeyCode)
            {
                case Keys.Left: x--; break;
                case Keys.Right: x++; break;
                case Keys.Up: y--; break;
                case Keys.Down: y++; break;
                default: return;
            }
            x = Math.Max(0, Math.Min(63, x));
            y = Math.Max(0, Math.Min(63, y));
            Selected = new Point(x, y);
            Invalidate();
            TileClicked?.Invoke(x, y);
        }
    }
}

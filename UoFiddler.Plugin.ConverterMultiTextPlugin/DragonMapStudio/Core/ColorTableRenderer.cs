// /***************************************************************************
//  *
//  * Dragon Map Studio – draws a colour table picture from the rule set
//  * (like IMod13_Colortable.bmp, but always up to date with your own colours)
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
using System.Drawing.Text;
using System.Linq;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public static class ColorTableRenderer
    {
        private const int Cell = 26;
        private const int LabelWidth = 190;
        private const int RowHeight = 30;
        private const int MaxCells = 20;

        /// <summary>
        /// One row per group: name + one field per colour with its altitude. Colours without a group are listed at the end.
        /// <paramref name="indices"/> gets the palette index of every pixel (-1 = label / background).
        /// </summary>
        public static Bitmap Render(DragonRuleSet rules, out int[] indices)
        {
            var rows = new List<(string name, List<DragonPaletteEntry> entries)>();
            foreach (IGrouping<int, DragonPaletteEntry> g in rules.Palette.Where(p => p.Group >= 0 && (p.Defined || p.Group >= 0)).GroupBy(p => p.Group).OrderBy(g => g.Key))
            {
                List<DragonPaletteEntry> sorted = g.OrderBy(p => p.Defined ? 0 : 1).ThenBy(p => p.Altitude).ThenBy(p => p.Index).ToList();
                for (int k = 0; k < sorted.Count; k += MaxCells)
                {
                    rows.Add(($"{rules.HexGroup(g.Key)} {rules.GroupName(g.Key)}", sorted.Skip(k).Take(MaxCells).ToList()));
                }
            }
            // 16 bit: thousands of free addresses – only the next free ones are shown
            List<DragonPaletteEntry> free = rules.Palette.Where(p => p.Group < 0).Take(rules.ColorBits == 16 ? MaxCells * 2 : int.MaxValue).ToList();
            for (int k = 0; k < free.Count; k += MaxCells)
            {
                rows.Add(("ohne Gruppe / frei", free.Skip(k).Take(MaxCells).ToList()));
            }

            int width = LabelWidth + MaxCells * Cell + 8;
            int height = Math.Max(1, rows.Count) * RowHeight + 8;
            indices = new int[width * height];
            Array.Fill(indices, -1);
            var bmp = new Bitmap(width, height);
            using Graphics gr = Graphics.FromImage(bmp);
            gr.Clear(Color.FromArgb(235, 228, 210));
            gr.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            using var labelFont = new Font("Segoe UI", 9f, FontStyle.Bold);
            using var cellFont = new Font("Segoe UI", 7f, FontStyle.Bold);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            for (int r = 0; r < rows.Count; r++)
            {
                int y = 4 + r * RowHeight;
                gr.DrawString(rows[r].name, labelFont, Brushes.Black, new RectangleF(4, y, LabelWidth - 8, RowHeight), new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });
                for (int c = 0; c < rows[r].entries.Count; c++)
                {
                    DragonPaletteEntry e = rows[r].entries[c];
                    var rect = new Rectangle(LabelWidth + c * Cell, y + 2, Cell - 2, RowHeight - 4);
                    using (var b = new SolidBrush(e.Color))
                    {
                        gr.FillRectangle(b, rect);
                    }
                    gr.DrawRectangle(Pens.Black, rect);
                    Brush text = e.Color.GetBrightness() > 0.55f ? Brushes.Black : Brushes.White;
                    gr.DrawString(e.Defined ? e.Altitude.ToString() : "–", cellFont, text, rect, sf);
                    for (int py = rect.Top; py <= rect.Bottom; py++)
                    {
                        for (int px = rect.Left; px <= rect.Right; px++)
                        {
                            indices[py * width + px] = e.Index;
                        }
                    }
                }
            }
            return bmp;
        }
    }
}

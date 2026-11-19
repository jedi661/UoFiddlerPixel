using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace UoFiddler.Controls.Classes
{
    /// <summary>Description of one sprite sheet (stored next to the PNG as .json).</summary>
    public sealed class SpriteSheetInfo
    {
        public string Format { get; set; } = "UOFiddler-SpriteSheet";
        public int Version { get; set; } = 1;
        public string Name { get; set; } = string.Empty;
        public int Columns { get; set; }
        public int Rows { get; set; }
        public int CellWidth { get; set; }
        public int CellHeight { get; set; }
        public int Separator { get; set; } = 1;
        public string SeparatorColor { get; set; } = "#FF00FF";
        public string TransparentColor { get; set; } = "#000000";
        public string Anchor { get; set; } = "Each sprite stands bottom-centred in its cell: the middle pixel of its lowest row = middle pixel of the lowest cell row (UO fix point).";
        public List<SpriteSheetCell> Cells { get; set; } = new List<SpriteSheetCell>();
    }

    public sealed class SpriteSheetCell
    {
        public int Index { get; set; }
        public string Label { get; set; } = string.Empty;   // e.g. North, East, South, West, Source
        public int ItemId { get; set; } = -1;
        public int Column { get; set; }
        public int Row { get; set; }

        /// <summary>Inner area of the cell in the sheet (without separator lines).</summary>
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>The original frame of the sprite inside the cell (its full size defines the UO position).</summary>
        public int SpriteX { get; set; }
        public int SpriteY { get; set; }
        public int SpriteWidth { get; set; }
        public int SpriteHeight { get; set; }
    }

    /// <summary>
    /// Sprite sheet for sending art to somebody (or an AI) and getting it back: all sprites in one picture,
    /// every sprite in its own cell, cells separated by 1 px magenta lines (#FF00FF), black = transparent,
    /// each sprite bottom-centred (UO fix point = middle pixel of the lowest row). A .json next to the PNG
    /// describes every cell; without it the cells are found again by the magenta lines.
    /// When importing, each cell becomes a graphic again: cut to its content, keeping the fix point column
    /// (cell centre) and the ground row (cell bottom), so it stands in the UO field like before.
    /// </summary>
    public static class ArtSpriteSheet
    {
        /// <summary>Opaque magenta in ARGB1555 (separator lines).</summary>
        public const ushort SeparatorPixel = 0x8000 | (31 << 10) | 31;

        public static ArtPixelImage Compose(IReadOnlyList<(string Label, int ItemId, ArtPixelImage Image)> sprites, int columns,
            int padding, string name, out SpriteSheetInfo info)
        {
            columns = Math.Max(1, Math.Min(columns, Math.Max(1, sprites.Count)));
            int rows = Math.Max(1, (sprites.Count + columns - 1) / columns);
            int maxW = Math.Max(1, sprites.Max(s => s.Image?.Width ?? 1));
            int maxH = Math.Max(1, sprites.Max(s => s.Image?.Height ?? 1));
            int cw = maxW + (2 * padding), ch = maxH + padding;   // no padding below: the ground row is the cell bottom
            int sheetW = (columns * cw) + columns + 1, sheetH = (rows * ch) + rows + 1;

            var sheet = new ArtPixelImage(sheetW, sheetH);
            for (int x = 0; x < sheetW; ++x)
            {
                for (int r = 0; r <= rows; ++r)
                {
                    sheet[x, r * (ch + 1)] = SeparatorPixel;
                }
            }

            for (int y = 0; y < sheetH; ++y)
            {
                for (int c = 0; c <= columns; ++c)
                {
                    sheet[c * (cw + 1), y] = SeparatorPixel;
                }
            }

            info = new SpriteSheetInfo { Name = name ?? string.Empty, Columns = columns, Rows = rows, CellWidth = cw, CellHeight = ch };
            for (int i = 0; i < sprites.Count; ++i)
            {
                int col = i % columns, row = i / columns;
                int cx = (col * (cw + 1)) + 1, cy = (row * (ch + 1)) + 1;
                ArtPixelImage img = sprites[i].Image;
                var cell = new SpriteSheetCell
                {
                    Index = i,
                    Label = sprites[i].Label,
                    ItemId = sprites[i].ItemId,
                    Column = col,
                    Row = row,
                    X = cx,
                    Y = cy,
                    Width = cw,
                    Height = ch
                };

                if (img != null)
                {
                    // fix point of the sprite (floor(w/2), lowest row) on the fix point of the cell (floor(cw/2), lowest row)
                    int sx = cx + (cw / 2) - (img.Width / 2), sy = cy + ch - img.Height;
                    for (int y = 0; y < img.Height; ++y)
                    {
                        for (int x = 0; x < img.Width; ++x)
                        {
                            ushort p = img[x, y];
                            if (p != 0)
                            {
                                sheet[sx + x, sy + y] = p == SeparatorPixel ? (ushort)(SeparatorPixel - 1) : p; // never draw the key colour
                            }
                        }
                    }

                    cell.SpriteX = sx;
                    cell.SpriteY = sy;
                    cell.SpriteWidth = img.Width;
                    cell.SpriteHeight = img.Height;
                }

                info.Cells.Add(cell);
            }

            return sheet;
        }

        /// <summary>
        /// Cuts a sheet into its sprites. With <paramref name="info"/> the cells of the JSON are used (if the picture
        /// still has that size), otherwise the cells are found by the magenta separator lines.
        /// </summary>
        public static List<(SpriteSheetCell Cell, ArtPixelImage Image)> Slice(ArtPixelImage sheet, SpriteSheetInfo info)
        {
            List<SpriteSheetCell> cells = info != null && info.Cells.Count > 0 && FitsSheet(sheet, info)
                ? info.Cells
                : DetectCells(sheet);

            var result = new List<(SpriteSheetCell, ArtPixelImage)>();
            foreach (SpriteSheetCell cell in cells)
            {
                result.Add((cell, CutCell(sheet, cell)));
            }

            return result;
        }

        private static bool FitsSheet(ArtPixelImage sheet, SpriteSheetInfo info)
        {
            return info.Cells.All(c => c.X >= 0 && c.Y >= 0 && c.X + c.Width <= sheet.Width && c.Y + c.Height <= sheet.Height);
        }

        /// <summary>Cells between magenta separator lines (columns / rows that are completely magenta).</summary>
        public static List<SpriteSheetCell> DetectCells(ArtPixelImage sheet)
        {
            var sepCols = new List<int>();
            for (int x = 0; x < sheet.Width; ++x)
            {
                if (Enumerable.Range(0, sheet.Height).Count(y => IsSeparator(sheet[x, y])) >= sheet.Height * 0.9)
                {
                    sepCols.Add(x);
                }
            }

            var sepRows = new List<int>();
            for (int y = 0; y < sheet.Height; ++y)
            {
                if (Enumerable.Range(0, sheet.Width).Count(x => IsSeparator(sheet[x, y])) >= sheet.Width * 0.9)
                {
                    sepRows.Add(y);
                }
            }

            List<(int Start, int End)> Spans(List<int> seps, int size)
            {
                var spans = new List<(int, int)>();
                int prev = -1;
                foreach (int s in seps.Append(size))
                {
                    if (s - prev > 1)
                    {
                        spans.Add((prev + 1, s - 1));
                    }

                    prev = s;
                }

                return spans;
            }

            var cells = new List<SpriteSheetCell>();
            List<(int Start, int End)> rows = Spans(sepRows, sheet.Height), cols = Spans(sepCols, sheet.Width);
            for (int r = 0; r < rows.Count; ++r)
            {
                for (int c = 0; c < cols.Count; ++c)
                {
                    cells.Add(new SpriteSheetCell
                    {
                        Index = cells.Count,
                        Column = c,
                        Row = r,
                        X = cols[c].Start,
                        Y = rows[r].Start,
                        Width = cols[c].End - cols[c].Start + 1,
                        Height = rows[r].End - rows[r].Start + 1
                    });
                }
            }

            return cells;
        }

        private static bool IsSeparator(ushort p)
        {
            // magenta, tolerant to small colour changes by image programs
            return p != 0 && ArtDirectionGenerator.R5(p) >= 28 && ArtDirectionGenerator.B5(p) >= 28 && ArtDirectionGenerator.G5(p) <= 3;
        }

        /// <summary>
        /// The content of a cell as UO graphic: frame = smallest area around the content that keeps the cell's centre
        /// column as fix point column and the cell's bottom row as ground row (no padding left).
        /// </summary>
        public static ArtPixelImage CutCell(ArtPixelImage sheet, SpriteSheetCell cell)
        {
            int c = cell.X + (cell.Width / 2);
            int bottom = cell.Y + cell.Height - 1;
            int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue;
            for (int y = cell.Y; y <= bottom; ++y)
            {
                for (int x = cell.X; x < cell.X + cell.Width; ++x)
                {
                    ushort p = sheet[x, y];
                    if (p != 0 && (p & 0x7FFF) != (SeparatorPixel & 0x7FFF)) // inside a cell only the exact key colour is a line
                    {
                        minX = Math.Min(minX, x);
                        maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y);
                    }
                }
            }

            if (minX == int.MaxValue)
            {
                return null; // empty cell
            }

            // smallest frame width (at least the original one) whose fix point column floor(w/2) is the cell
            // centre and that still contains all content -> exact round trip, also for odd widths
            int w = Math.Max(1, cell.SpriteWidth);
            while (c - (w / 2) > minX || c - (w / 2) + w - 1 < maxX)
            {
                ++w;
            }
            int h = Math.Max(bottom - minY + 1, cell.SpriteHeight > 0 && cell.SpriteHeight <= cell.Height ? cell.SpriteHeight : 0);
            var img = new ArtPixelImage(w, h);
            int ox = c - (w / 2), oy = bottom - h + 1;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    int sx = ox + x, sy = oy + y;
                    if (sx >= cell.X && sx < cell.X + cell.Width && sy >= cell.Y && sy <= bottom)
                    {
                        ushort p = sheet[sx, sy];
                        if (p != 0 && (p & 0x7FFF) != (SeparatorPixel & 0x7FFF)) // inside a cell only the exact key colour is a line
                        {
                            img[x, y] = p;
                        }
                    }
                }
            }

            return img;
        }

        public static string ToJson(SpriteSheetInfo info) => JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });

        public static SpriteSheetInfo FromJson(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<SpriteSheetInfo>(json);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Instructions for a person or an AI that edits the sheet and sends it back.</summary>
        public static string AiPrompt(SpriteSheetInfo info)
        {
            string cells = info == null ? string.Empty : string.Join(Environment.NewLine, info.Cells.Select(c =>
                $"  - cell {c.Index} (column {c.Column}, row {c.Row}): {c.Label}{(c.ItemId >= 0 ? $", item 0x{c.ItemId:X4}" : string.Empty)}" +
                $", cell area x {c.X} y {c.Y} w {c.Width} h {c.Height}" +
                (c.SpriteWidth > 0 ? $", sprite frame {c.SpriteWidth}x{c.SpriteHeight}" : ", empty")));

            return
$@"You get a sprite sheet with Ultima Online item graphics (pixel art). Please edit it and send it back in EXACTLY the same layout.

SHEET LAYOUT (must not change)
- Picture size {(info == null ? "as received" : $"{(info.Columns * (info.CellWidth + 1)) + 1} x {(info.Rows * (info.CellHeight + 1)) + 1} px")}, {(info == null ? "cells" : $"{info.Columns} columns x {info.Rows} rows of cells, each cell {info.CellWidth} x {info.CellHeight} px")}.
- Cells are separated by 1 px lines in pure magenta #FF00FF. Do not move, recolour or remove these lines and never use #FF00FF anywhere else.
- Pure black #000000 means transparent. Do not use pure black for visible pixels (use e.g. #080808).

UO ANCHOR (most important)
- Every sprite stands bottom-centred in its cell: the middle pixel of the lowest row of the sprite is the middle pixel of the lowest row of the cell. This point is the bottom corner of the 44x44 UO tile diamond (45 degree edges) the item stands on.
- Keep this: do not move sprites up/down or sideways unless asked; the lowest row of the cell is the ground.
- Directions: South = front to the lower left, East = lower right, North = upper right, West = upper left.

PIXEL RULES
- Pixel art: no anti-aliasing, no blur, no soft shadows, no semi-transparency, no resampling/scaling of the whole sheet.
- Colours are 15-bit in the game (5 bits per channel); keep the existing palette where possible.
- Light comes from the upper left (faces to the lower left are brighter than faces to the lower right).

CELLS
{cells}

Return the edited sheet as PNG with the same size and the same cells (and this cell list unchanged). Empty cells may be filled with new sprites following the same anchor rule.";
        }
    }
}

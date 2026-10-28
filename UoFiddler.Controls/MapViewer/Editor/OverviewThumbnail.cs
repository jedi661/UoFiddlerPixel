/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Ultima;

namespace UoFiddler.Controls.MapViewer.Editor
{
    /// <summary>
    /// Small picture of a whole map (1 pixel = 4 x 4 tiles, averaged radar colors).
    /// It is built step by step (<see cref="Step"/>, called by a timer) so the window stays usable,
    /// shared by all overview controls of a map and kept as long as the map exists.
    /// After a change only the changed block rows are rendered again (<see cref="MarkDirty"/>).
    /// </summary>
    public sealed class OverviewThumbnail
    {
        /// <summary>tiles per thumbnail pixel</summary>
        public const int Scale = 4;

        private const int PixelsPerBlock = 8 / Scale;

        private static readonly ConditionalWeakTable<Map, OverviewThumbnail> _withStatics = new ConditionalWeakTable<Map, OverviewThumbnail>();
        private static readonly ConditionalWeakTable<Map, OverviewThumbnail> _withoutStatics = new ConditionalWeakTable<Map, OverviewThumbnail>();

        private readonly Map _map;
        private readonly bool _statics;
        private readonly int _blocksW;
        private readonly int _blocksH;
        private readonly int[] _pixels;
        private readonly bool[] _rowDone;
        private int _done;
        private int _next;
        private Bitmap _bitmap;
        private bool _bitmapDirty = true;

        private OverviewThumbnail(Map map, bool statics)
        {
            _map = map;
            _statics = statics;
            _blocksW = Math.Max(1, map.Width >> 3);
            _blocksH = Math.Max(1, map.Height >> 3);
            Width = _blocksW * PixelsPerBlock;
            Height = _blocksH * PixelsPerBlock;
            _pixels = new int[Width * Height];
            _rowDone = new bool[_blocksH];
        }

        public static OverviewThumbnail Get(Map map, bool statics)
        {
            ConditionalWeakTable<Map, OverviewThumbnail> table = statics ? _withStatics : _withoutStatics;
            return table.GetValue(map, m => new OverviewThumbnail(m, statics));
        }

        /// <summary>Marks the rows of an area of a map as changed (both variants, only if they exist)</summary>
        public static void Invalidate(Map map, Rectangle? area)
        {
            foreach (ConditionalWeakTable<Map, OverviewThumbnail> table in new[] { _withStatics, _withoutStatics })
            {
                if (table.TryGetValue(map, out OverviewThumbnail thumb))
                {
                    thumb.MarkDirty(area);
                }
            }
        }

        public int Width { get; }
        public int Height { get; }

        public bool IsComplete => _done >= _blocksH;

        public double Progress => _done / (double)_blocksH;

        public string Error { get; private set; }

        public void MarkDirty(Rectangle? area)
        {
            int r0 = 0, r1 = _blocksH - 1;
            if (area.HasValue)
            {
                r0 = Math.Max(0, area.Value.Top >> 3);
                r1 = Math.Min(_blocksH - 1, (area.Value.Bottom - 1) >> 3);
            }

            for (int r = r0; r <= r1; r++)
            {
                if (_rowDone[r])
                {
                    _rowDone[r] = false;
                    _done--;
                }
            }

            _next = Math.Min(_next, r0);
            Error = null;
        }

        /// <summary>Renders rows until the time is used up. Returns false when nothing is left.</summary>
        public bool Step(long budgetMs)
        {
            if (IsComplete || Error != null)
            {
                return false;
            }

            Stopwatch watch = Stopwatch.StartNew();
            while (!IsComplete && watch.ElapsedMilliseconds < budgetMs)
            {
                int row = FindNextRow();
                if (row < 0)
                {
                    break;
                }

                try
                {
                    RenderRow(row);
                }
                catch (Exception ex)
                {
                    Error = $"The map can not be read:\n{ex.GetType().Name}: {ex.Message}";
                    _map.ResetCache();
                    return false;
                }

                _rowDone[row] = true;
                _done++;
                _next = row + 1;
                _bitmapDirty = true;
            }

            return !IsComplete;
        }

        private int FindNextRow()
        {
            for (int i = 0; i < _blocksH; i++)
            {
                int r = (_next + i) % _blocksH;
                if (!_rowDone[r])
                {
                    return r;
                }
            }

            return -1;
        }

        private void RenderRow(int by)
        {
            using (Bitmap strip = _map.GetImage(0, by, _blocksW, 1, _statics))
            {
                int w = _blocksW << 3;
                BitmapData bd = strip.LockBits(new Rectangle(0, 0, w, 8), ImageLockMode.ReadOnly, PixelFormat.Format16bppRgb555);
                try
                {
                    var row = new short[w * 8];
                    for (int y = 0; y < 8; y++)
                    {
                        Marshal.Copy(bd.Scan0 + (y * bd.Stride), row, y * w, w);
                    }

                    // average of every Scale x Scale tile square
                    for (int ty = 0; ty < PixelsPerBlock; ty++)
                    {
                        int dst = ((by * PixelsPerBlock) + ty) * Width;
                        for (int tx = 0; tx < Width; tx++)
                        {
                            int r = 0, g = 0, b = 0;
                            for (int sy = 0; sy < Scale; sy++)
                            {
                                int src = (((ty * Scale) + sy) * w) + (tx * Scale);
                                for (int sx = 0; sx < Scale; sx++)
                                {
                                    int c = row[src + sx];
                                    r += (c >> 10) & 0x1F;
                                    g += (c >> 5) & 0x1F;
                                    b += c & 0x1F;
                                }
                            }

                            const int n = Scale * Scale;
                            r = (r * 255) / (31 * n);
                            g = (g * 255) / (31 * n);
                            b = (b * 255) / (31 * n);
                            _pixels[dst + tx] = unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
                        }
                    }
                }
                finally
                {
                    strip.UnlockBits(bd);
                }
            }
        }

        /// <summary>Current picture (rows that are not rendered yet are black)</summary>
        public Bitmap GetBitmap()
        {
            if (_bitmap == null)
            {
                _bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
                _bitmapDirty = true;
            }

            if (_bitmapDirty)
            {
                BitmapData bd = _bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                try
                {
                    for (int y = 0; y < Height; y++)
                    {
                        Marshal.Copy(_pixels, y * Width, bd.Scan0 + (y * bd.Stride), Width);
                    }
                }
                finally
                {
                    _bitmap.UnlockBits(bd);
                }

                _bitmapDirty = false;
            }

            return _bitmap;
        }
    }
}

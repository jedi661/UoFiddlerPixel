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
using System.Linq;

namespace UoFiddler.Controls.MapViewer.Editor.Tools
{
    /// <summary>
    /// The pattern of a brush: a grid of cells. Every cell has no tile (the ground stays), one tile
    /// or several tiles (one of them is chosen by position), a height and optional statics.
    /// Made by hand in the brush editor, copied from a map or imported from a picture.
    /// </summary>
    public sealed class BrushPattern
    {
        private readonly List<ushort>[] _ids;
        private readonly sbyte[] _z;
        private Dictionary<Point, List<ClipStatic>> _staticsByCell;

        public BrushPattern(int width, int height)
        {
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            _ids = new List<ushort>[Width * Height];
            _z = new sbyte[Width * Height];
            for (int i = 0; i < _ids.Length; i++)
            {
                _ids[i] = new List<ushort>();
            }
        }

        public int Width { get; }
        public int Height { get; }

        /// <summary>statics, position relative to the pattern, z absolute (like <see cref="MapClip"/>)</summary>
        public List<ClipStatic> Statics { get; } = new List<ClipStatic>();

        public string SourceName { get; set; } = string.Empty;

        public IReadOnlyList<ushort> GetIds(int x, int y) => _ids[(y * Width) + x];

        /// <summary>true = the cell paints nothing (the ground stays)</summary>
        public bool IsEmpty(int x, int y) => _ids[(y * Width) + x].Count == 0;

        public void SetIds(int x, int y, IEnumerable<ushort> ids)
        {
            List<ushort> list = _ids[(y * Width) + x];
            list.Clear();
            if (ids != null)
            {
                list.AddRange(ids);
            }
        }

        public void SetId(int x, int y, ushort id) => SetIds(x, y, new[] { id });

        public void Clear(int x, int y) => _ids[(y * Width) + x].Clear();

        public sbyte GetZ(int x, int y) => _z[(y * Width) + x];

        public void SetZ(int x, int y, sbyte z) => _z[(y * Width) + x] = z;

        /// <summary>the tile painted at a map position (variants by position), null = nothing</summary>
        public ushort? Pick(int x, int y, int mapX, int mapY)
        {
            List<ushort> list = _ids[(y * Width) + x];
            return list.Count == 0 ? (ushort?)null : Transitions.TransitionEngine.Pick(list, mapX, mapY);
        }

        /// <summary>first tile of a cell (for pictures), null = empty</summary>
        public ushort? First(int x, int y)
        {
            List<ushort> list = _ids[(y * Width) + x];
            return list.Count == 0 ? (ushort?)null : list[0];
        }

        public int MinZ => _z.Length == 0 ? 0 : _z.Min(z => (int)z);
        public int MaxZ => _z.Length == 0 ? 0 : _z.Max(z => (int)z);

        public int FilledCells => _ids.Count(l => l.Count > 0);

        public IReadOnlyList<ClipStatic> StaticsAt(int x, int y)
        {
            if (_staticsByCell == null)
            {
                _staticsByCell = new Dictionary<Point, List<ClipStatic>>();
                foreach (ClipStatic s in Statics)
                {
                    var key = new Point(s.X, s.Y);
                    if (!_staticsByCell.TryGetValue(key, out List<ClipStatic> list))
                    {
                        _staticsByCell[key] = list = new List<ClipStatic>();
                    }

                    list.Add(s);
                }
            }

            return _staticsByCell.TryGetValue(new Point(x, y), out List<ClipStatic> l) ? l : (IReadOnlyList<ClipStatic>)Array.Empty<ClipStatic>();
        }

        /// <summary>call after <see cref="Statics"/> was changed</summary>
        public void StaticsChanged() => _staticsByCell = null;

        #region Create / transform

        public static BrushPattern FromClip(MapClip clip)
        {
            var p = new BrushPattern(clip.Width, clip.Height) { SourceName = clip.SourceName };
            for (int y = 0; y < clip.Height; y++)
            {
                for (int x = 0; x < clip.Width; x++)
                {
                    p.SetId(x, y, clip.GetLandId(x, y));
                    p.SetZ(x, y, clip.GetLandZ(x, y));
                }
            }

            p.Statics.AddRange(clip.Statics);
            return p;
        }

        /// <summary>Pattern for pictures/previews: empty cells get the given tile (2 = nodraw, not drawn)</summary>
        public MapClip ToClip(ushort emptyId = 2)
        {
            var clip = new MapClip(Width, Height) { SourceName = SourceName };
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    clip.SetLand(x, y, First(x, y) ?? emptyId, GetZ(x, y));
                }
            }

            clip.Statics.AddRange(Statics);
            return clip;
        }

        public BrushPattern Clone()
        {
            var p = new BrushPattern(Width, Height) { SourceName = SourceName };
            for (int i = 0; i < _ids.Length; i++)
            {
                p._ids[i].AddRange(_ids[i]);
                p._z[i] = _z[i];
            }

            p.Statics.AddRange(Statics);
            return p;
        }

        /// <summary>New size, the content stays at the upper left corner</summary>
        public BrushPattern Resize(int width, int height)
        {
            var p = new BrushPattern(width, height) { SourceName = SourceName };
            for (int y = 0; y < Math.Min(Height, p.Height); y++)
            {
                for (int x = 0; x < Math.Min(Width, p.Width); x++)
                {
                    p.SetIds(x, y, GetIds(x, y));
                    p.SetZ(x, y, GetZ(x, y));
                }
            }

            p.Statics.AddRange(Statics.Where(s => s.X < p.Width && s.Y < p.Height));
            return p;
        }

        /// <summary>New size, the content stays in the middle (cells outside are dropped)</summary>
        public BrushPattern ResizeCentered(int width, int height)
        {
            var p = new BrushPattern(width, height) { SourceName = SourceName };
            int ox = ((p.Width - 1) / 2) - ((Width - 1) / 2), oy = ((p.Height - 1) / 2) - ((Height - 1) / 2);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int nx = x + ox, ny = y + oy;
                    if (nx >= 0 && ny >= 0 && nx < p.Width && ny < p.Height)
                    {
                        p.SetIds(nx, ny, GetIds(x, y));
                        p.SetZ(nx, ny, GetZ(x, y));
                    }
                }
            }

            p.Statics.AddRange(Statics.Select(s => new ClipStatic(s.X + ox, s.Y + oy, s.Id, s.Z, s.Hue))
                .Where(s => s.X >= 0 && s.Y >= 0 && s.X < p.Width && s.Y < p.Height));
            return p;
        }

        /// <summary>Moves all cells (what leaves on one side comes back on the other side)</summary>
        public BrushPattern Shift(int dx, int dy)
        {
            int Mod(int a, int m) => ((a % m) + m) % m;
            return Map(Width, Height, (x, y) => new Point(Mod(x + dx, Width), Mod(y + dy, Height)));
        }

        /// <summary>Exchanges two cells (tiles, height and statics)</summary>
        public void Swap(int x1, int y1, int x2, int y2)
        {
            if (x1 == x2 && y1 == y2)
            {
                return;
            }

            var ids1 = GetIds(x1, y1).ToList();
            var ids2 = GetIds(x2, y2).ToList();
            sbyte z1 = GetZ(x1, y1), z2 = GetZ(x2, y2);
            SetIds(x1, y1, ids2);
            SetIds(x2, y2, ids1);
            SetZ(x1, y1, z2);
            SetZ(x2, y2, z1);
            for (int i = 0; i < Statics.Count; i++)
            {
                ClipStatic s = Statics[i];
                if (s.X == x1 && s.Y == y1)
                {
                    Statics[i] = new ClipStatic(x2, y2, s.Id, s.Z, s.Hue);
                }
                else if (s.X == x2 && s.Y == y2)
                {
                    Statics[i] = new ClipStatic(x1, y1, s.Id, s.Z, s.Hue);
                }
            }

            StaticsChanged();
        }

        /// <summary>
        /// Puts textures at fixed positions: <paramref name="random"/> = like the random brush would do it,
        /// otherwise in the given order row by row (repeated). Cells outside of a round brush stay empty.
        /// </summary>
        public static BrushPattern FromTextures(IReadOnlyList<ushort> tiles, int size, BrushShape shape, bool random)
        {
            var p = new BrushPattern(size, size);
            if (tiles == null || tiles.Count == 0)
            {
                return p;
            }

            var inside = new HashSet<Point>(MapBrush.Cells((size - 1) / 2, (size - 1) / 2, size, shape, size, size));
            int n = 0;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (!inside.Contains(new Point(x, y)))
                    {
                        continue;
                    }

                    p.SetId(x, y, random ? Transitions.TransitionEngine.Pick(tiles, x, y) : tiles[n++ % tiles.Count]);
                }
            }

            return p;
        }

        private BrushPattern Map(int newW, int newH, Func<int, int, Point> pos)
        {
            var p = new BrushPattern(newW, newH) { SourceName = SourceName };
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Point n = pos(x, y);
                    p.SetIds(n.X, n.Y, GetIds(x, y));
                    p.SetZ(n.X, n.Y, GetZ(x, y));
                }
            }

            foreach (ClipStatic s in Statics)
            {
                Point n = pos(s.X, s.Y);
                p.Statics.Add(new ClipStatic(n.X, n.Y, s.Id, s.Z, s.Hue));
            }

            return p;
        }

        public BrushPattern Rotate90() => Map(Height, Width, (x, y) => new Point(Height - 1 - y, x));

        public BrushPattern MirrorX() => Map(Width, Height, (x, y) => new Point(Width - 1 - x, y));

        public BrushPattern MirrorY() => Map(Width, Height, (x, y) => new Point(x, Height - 1 - y));

        #endregion
    }
}

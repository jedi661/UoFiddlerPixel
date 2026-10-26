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
using Ultima;

namespace UoFiddler.Controls.MapViewer
{
    /// <summary>
    /// A static item inside a <see cref="RegionCell"/>
    /// </summary>
    public struct RegionStatic
    {
        public ushort Id;
        public sbyte Z;
        public short Hue;

        /// <summary>true if the static is only pending (frozen in memory, not yet in the statics file)</summary>
        public bool Pending;

        public int Height => Id < TileData.ItemTable.Length ? TileData.ItemTable[Id].Height : 0;

        public TileFlag Flags => Id < TileData.ItemTable.Length ? TileData.ItemTable[Id].Flags : TileFlag.None;

        public string Name => Id < TileData.ItemTable.Length ? TileData.ItemTable[Id].Name : string.Empty;
    }

    /// <summary>
    /// One map position of the region with land and statics
    /// </summary>
    public sealed class RegionCell
    {
        public int X;
        public int Y;
        public ushort LandId;

        /// <summary>z of the land tile (= upper corner of the diamond)</summary>
        public sbyte LandZ;

        /// <summary>z of the right corner (x+1, y)</summary>
        public sbyte ZRight;

        /// <summary>z of the lower corner (x+1, y+1)</summary>
        public sbyte ZBottom;

        /// <summary>z of the left corner (x, y+1)</summary>
        public sbyte ZLeft;

        public List<RegionStatic> Statics = new List<RegionStatic>();

        public bool IsFlat => LandZ == ZRight && LandZ == ZBottom && LandZ == ZLeft;

        public int MinCornerZ => Math.Min(Math.Min(LandZ, ZRight), Math.Min(ZBottom, ZLeft));

        public int AverageZ => (LandZ + ZRight + ZBottom + ZLeft) / 4;

        public string LandName => LandId < TileData.LandTable.Length ? TileData.LandTable[LandId].Name : string.Empty;

        public int TextureId => LandId < TileData.LandTable.Length ? TileData.LandTable[LandId].TextureId : 0;
    }

    public enum DrawableKind : byte
    {
        Land = 0,
        Static = 1
    }

    /// <summary>
    /// One thing to draw (land tile or static) in canvas coordinates
    /// </summary>
    public struct Drawable
    {
        public DrawableKind Kind;
        public int X;
        public int Y;

        /// <summary>index into <see cref="RegionCell.Statics"/></summary>
        public int StaticIndex;

        public int SortDiagonal;
        public int SortZ;
        public int Seq;

        /// <summary>bounds in canvas coordinates (union of flat and stretched land)</summary>
        public Rectangle Bounds;
    }

    /// <summary>
    /// Holds land + statics of a rectangular map region and the isometric layout (client projection:
    /// 44x44 diamonds, 4 pixel per z). The data is independent of the renderer, a later 3D renderer can
    /// use <see cref="Cells"/> directly (height mesh from the corner z values + statics).
    /// </summary>
    public sealed class RegionData
    {
        public const int TileHalf = 22;
        public const int TileSize = 44;
        public const int ZScale = 4;
        public const int ChunkSize = 256;
        private const int Margin = 32;

        private RegionCell[] _cells;
        private Drawable[] _drawables = Array.Empty<Drawable>();
        private List<int>[] _buckets = Array.Empty<List<int>>();

        public RegionData(MapDefinition definition, Rectangle area)
        {
            Definition = definition;
            Map = definition.GetMap();
            area.Intersect(new Rectangle(0, 0, Map.Width, Map.Height));
            Area = area;
        }

        public MapDefinition Definition { get; }
        public Map Map { get; }

        /// <summary>region in map coordinates</summary>
        public Rectangle Area { get; }

        public int OriginX { get; private set; }
        public int OriginY { get; private set; }
        public Size CanvasSize { get; private set; }
        public int ChunkColumns { get; private set; }
        public int ChunkRows { get; private set; }

        public IReadOnlyList<Drawable> Drawables => _drawables;

        public int StaticCount { get; private set; }

        public IEnumerable<RegionCell> Cells
        {
            get
            {
                foreach (RegionCell cell in _cells)
                {
                    yield return cell;
                }
            }
        }

        public bool Contains(int x, int y)
        {
            return Area.Contains(x, y);
        }

        public RegionCell GetCell(int x, int y)
        {
            if (!Area.Contains(x, y))
            {
                return null;
            }

            return _cells[((y - Area.Y) * Area.Width) + (x - Area.X)];
        }

        #region Load

        /// <summary>
        /// (Re)reads all cells from the map and rebuilds the layout.
        /// </summary>
        public void Load(ArtPixelCache art)
        {
            TileMatrix tiles = Map.Tiles;
            _cells = new RegionCell[Area.Width * Area.Height];
            StaticCount = 0;

            for (int y = Area.Y; y < Area.Bottom; y++)
            {
                for (int x = Area.X; x < Area.Right; x++)
                {
                    Tile land = tiles.GetLandTile(x, y);
                    var cell = new RegionCell
                    {
                        X = x,
                        Y = y,
                        LandId = land.Id,
                        LandZ = land.Z,
                        ZRight = GetZ(tiles, x + 1, y, land.Z),
                        ZBottom = GetZ(tiles, x + 1, y + 1, land.Z),
                        ZLeft = GetZ(tiles, x, y + 1, land.Z)
                    };

                    foreach (HuedTile s in tiles.GetStaticTiles(x, y))
                    {
                        cell.Statics.Add(new RegionStatic { Id = s.Id, Z = s.Z, Hue = (short)s.Hue });
                    }

                    StaticTile[] pending = tiles.GetPendingStatics(x >> 3, y >> 3);
                    if (pending != null)
                    {
                        foreach (StaticTile s in pending)
                        {
                            if (s.X == (x & 0x7) && s.Y == (y & 0x7))
                            {
                                cell.Statics.Add(new RegionStatic { Id = s.Id, Z = s.Z, Hue = s.Hue, Pending = true });
                            }
                        }
                    }

                    StaticCount += cell.Statics.Count;
                    _cells[((y - Area.Y) * Area.Width) + (x - Area.X)] = cell;
                }
            }

            tiles.CloseStreams();
            BuildLayout(art);
        }

        private sbyte GetZ(TileMatrix tiles, int x, int y, sbyte fallback)
        {
            if (x >= Map.Width || y >= Map.Height)
            {
                return fallback;
            }

            return tiles.GetLandTile(x, y).Z;
        }

        #endregion

        #region Projection

        /// <summary>Canvas x of the center line of a cell</summary>
        public int CanvasX(int x, int y)
        {
            return (((x - Area.X) - (y - Area.Y)) * TileHalf) + OriginX;
        }

        /// <summary>Canvas y of the upper corner of a cell at the given z</summary>
        public int CanvasY(int x, int y, int z)
        {
            return (((x - Area.X) + (y - Area.Y)) * TileHalf) - (z * ZScale) + OriginY;
        }

        /// <summary>
        /// The four corners (top, right, bottom, left) of the land diamond of a cell in canvas coordinates
        /// </summary>
        public Point[] GetLandCorners(RegionCell cell, bool stretched)
        {
            int cx = CanvasX(cell.X, cell.Y);
            if (!stretched)
            {
                int cy = CanvasY(cell.X, cell.Y, cell.LandZ);
                return new[]
                {
                    new Point(cx, cy),
                    new Point(cx + TileHalf, cy + TileHalf),
                    new Point(cx, cy + TileSize),
                    new Point(cx - TileHalf, cy + TileHalf)
                };
            }

            return new[]
            {
                new Point(cx, CanvasY(cell.X, cell.Y, cell.LandZ)),
                new Point(cx + TileHalf, CanvasY(cell.X, cell.Y, cell.ZRight) + TileHalf),
                new Point(cx, CanvasY(cell.X, cell.Y, cell.ZBottom) + TileSize),
                new Point(cx - TileHalf, CanvasY(cell.X, cell.Y, cell.ZLeft) + TileHalf)
            };
        }

        /// <summary>
        /// Upper left canvas position of a static image
        /// </summary>
        public Point GetStaticPosition(int x, int y, int z, int imageWidth, int imageHeight)
        {
            return new Point(CanvasX(x, y) - (imageWidth / 2), CanvasY(x, y, z) + TileSize - imageHeight);
        }

        #endregion

        #region Layout

        private void BuildLayout(ArtPixelCache art)
        {
            var list = new List<Drawable>(_cells.Length + StaticCount);
            int seq = 0;

            // origin 0 first, the bounds are shifted afterwards
            OriginX = 0;
            OriginY = 0;

            foreach (RegionCell cell in _cells)
            {
                int diagonal = cell.X + cell.Y;

                Rectangle landBounds = GetBounds(GetLandCorners(cell, false));
                landBounds = Rectangle.Union(landBounds, GetBounds(GetLandCorners(cell, true)));

                list.Add(new Drawable
                {
                    Kind = DrawableKind.Land,
                    X = cell.X,
                    Y = cell.Y,
                    StaticIndex = -1,
                    SortDiagonal = diagonal,
                    SortZ = (cell.IsFlat ? cell.LandZ : cell.MinCornerZ) - 1,
                    Seq = seq++,
                    Bounds = landBounds
                });

                for (int i = 0; i < cell.Statics.Count; i++)
                {
                    RegionStatic s = cell.Statics[i];
                    ArtImage image = art.GetStatic(s.Id, 0);
                    int w = image?.Width ?? TileSize;
                    int h = image?.Height ?? TileSize;
                    Point p = GetStaticPosition(cell.X, cell.Y, s.Z, w, h);

                    TileFlag flags = s.Flags;
                    int sortZ = s.Z + ((flags & TileFlag.Background) != 0 ? 0 : 1) + (s.Height > 0 ? 1 : 0);

                    list.Add(new Drawable
                    {
                        Kind = DrawableKind.Static,
                        X = cell.X,
                        Y = cell.Y,
                        StaticIndex = i,
                        SortDiagonal = diagonal,
                        SortZ = sortZ,
                        Seq = seq++,
                        Bounds = new Rectangle(p.X, p.Y, w, h)
                    });
                }
            }

            list.Sort(CompareDrawables);

            // shift everything into positive canvas coordinates
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (Drawable d in list)
            {
                minX = Math.Min(minX, d.Bounds.Left);
                minY = Math.Min(minY, d.Bounds.Top);
                maxX = Math.Max(maxX, d.Bounds.Right);
                maxY = Math.Max(maxY, d.Bounds.Bottom);
            }

            if (list.Count == 0)
            {
                minX = minY = 0;
                maxX = maxY = 1;
            }

            OriginX = Margin - minX;
            OriginY = Margin - minY;
            CanvasSize = new Size((maxX - minX) + (2 * Margin), (maxY - minY) + (2 * Margin));

            _drawables = list.ToArray();
            for (int i = 0; i < _drawables.Length; i++)
            {
                _drawables[i].Bounds.Offset(OriginX, OriginY);
            }

            BuildBuckets();
        }

        private static int CompareDrawables(Drawable a, Drawable b)
        {
            int c = a.SortDiagonal.CompareTo(b.SortDiagonal);
            if (c != 0)
            {
                return c;
            }

            c = a.SortZ.CompareTo(b.SortZ);
            if (c != 0)
            {
                return c;
            }

            c = a.Kind.CompareTo(b.Kind);
            return c != 0 ? c : a.Seq.CompareTo(b.Seq);
        }

        private static Rectangle GetBounds(Point[] points)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (Point p in points)
            {
                minX = Math.Min(minX, p.X);
                minY = Math.Min(minY, p.Y);
                maxX = Math.Max(maxX, p.X);
                maxY = Math.Max(maxY, p.Y);
            }

            return Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
        }

        private void BuildBuckets()
        {
            ChunkColumns = Math.Max(1, (CanvasSize.Width + ChunkSize - 1) / ChunkSize);
            ChunkRows = Math.Max(1, (CanvasSize.Height + ChunkSize - 1) / ChunkSize);
            _buckets = new List<int>[ChunkColumns * ChunkRows];

            for (int i = 0; i < _drawables.Length; i++)
            {
                Rectangle b = _drawables[i].Bounds;
                int c0 = Math.Max(0, b.Left / ChunkSize);
                int c1 = Math.Min(ChunkColumns - 1, (b.Right - 1) / ChunkSize);
                int r0 = Math.Max(0, b.Top / ChunkSize);
                int r1 = Math.Min(ChunkRows - 1, (b.Bottom - 1) / ChunkSize);

                for (int r = r0; r <= r1; r++)
                {
                    for (int c = c0; c <= c1; c++)
                    {
                        (_buckets[(r * ChunkColumns) + c] ??= new List<int>()).Add(i);
                    }
                }
            }
        }

        /// <summary>
        /// Indices of the drawables touching a chunk (in drawing order)
        /// </summary>
        public List<int> GetChunkDrawables(int column, int row)
        {
            if (column < 0 || row < 0 || column >= ChunkColumns || row >= ChunkRows)
            {
                return null;
            }

            return _buckets[(row * ChunkColumns) + column];
        }

        public RegionStatic GetStatic(Drawable d)
        {
            return GetCell(d.X, d.Y).Statics[d.StaticIndex];
        }

        #endregion
    }
}

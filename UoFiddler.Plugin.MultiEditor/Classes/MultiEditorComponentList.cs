/***************************************************************************
 *
 * $Author: MuadDib & Turley
 * Extended (undo/redo, hues, origin, bulk tools): Nikodemus
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.MultiEditor.UserControls;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    internal class MultiEditorComponentList
    {
        private const int _undoListMaxSize = 200;

        private bool _modified;
        private static MultiEditorControl _parent;
        private static Rectangle _drawDestRectangle;

        private readonly List<UndoStruct> _undo = new List<UndoStruct>();
        private readonly List<UndoStruct> _redo = new List<UndoStruct>();

        public const int GapXMod = 44;
        public const int GapYMod = 22;

        public int WalkableCount;
        public int DoubleSurfaceCount;

        /// <summary>
        /// Create a blank ComponentList
        /// </summary>
        public MultiEditorComponentList(int width, int height, MultiEditorControl parent)
        {
            _parent = parent;
            Width = width;
            Height = height;
            Tiles = new List<MultiTile>();
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    Tiles.Add(new FloorTile(x, y, _parent.DrawFloorZ));
                }
            }

            SetDefaultOrigin();
            _modified = true;
            RecalculateMinMax();
        }

        /// <summary>
        /// Create a ComponentList from UltimaSDK
        /// </summary>
        public MultiEditorComponentList(MultiComponentList list, MultiEditorControl parent)
        {
            _parent = parent;
            Width = list.Width;
            Height = list.Height;
            OriginX = list.Center.X;
            OriginY = list.Center.Y;
            Tiles = new List<MultiTile>();
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    for (int i = 0; i < list.Tiles[x][y].Length; ++i)
                    {
                        Tiles.Add(new MultiTile(
                            list.Tiles[x][y][i].Id,
                            x,
                            y,
                            list.Tiles[x][y][i].Z,
                            list.Tiles[x][y][i].Flag));
                    }

                    Tiles.Add(new FloorTile(x, y, _parent.DrawFloorZ));
                }
            }

            CalcSolver();
            Tiles.Sort();
            _modified = true;
            RecalculateMinMax();
        }

        /// <summary>
        /// Create a ComponentList from imported tiles (offsets relative to the multi center, hues are kept)
        /// </summary>
        public MultiEditorComponentList(IList<ImportTile> tiles, MultiEditorControl parent)
        {
            _parent = parent;
            Tiles = new List<MultiTile>();

            if (tiles == null || tiles.Count == 0)
            {
                Width = 1;
                Height = 1;
                OriginX = 0;
                OriginY = 0;
            }
            else
            {
                int minX = tiles.Min(t => t.X);
                int minY = tiles.Min(t => t.Y);
                int maxX = tiles.Max(t => t.X);
                int maxY = tiles.Max(t => t.Y);
                Width = maxX - minX + 1;
                Height = maxY - minY + 1;
                OriginX = -minX;
                OriginY = -minY;

                foreach (ImportTile t in tiles)
                {
                    Tiles.Add(new MultiTile(t.Id, t.X + OriginX, t.Y + OriginY, Clamp(t.Z), t.Flag) { Hue = t.Hue, IsDeco = t.Deco });
                }
            }

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    Tiles.Add(new FloorTile(x, y, _parent.DrawFloorZ));
                }
            }

            CalcSolver();
            Tiles.Sort();
            _modified = true;
            RecalculateMinMax();
        }

        /// <summary>
        /// Create a ComponentList with fixed size, tiles in cell coordinates (project files)
        /// </summary>
        public MultiEditorComponentList(IList<ImportTile> cellTiles, int width, int height, int originX, int originY, MultiEditorControl parent)
        {
            _parent = parent;
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            OriginX = originX;
            OriginY = originY;
            Tiles = new List<MultiTile>();
            foreach (ImportTile t in cellTiles)
            {
                if (t.X < 0 || t.Y < 0 || t.X >= Width || t.Y >= Height)
                {
                    continue;
                }

                Tiles.Add(new MultiTile(t.Id, t.X, t.Y, Clamp(t.Z), t.Flag) { Hue = t.Hue, IsDeco = t.Deco });
            }

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    Tiles.Add(new FloorTile(x, y, _parent.DrawFloorZ));
                }
            }

            CalcSolver();
            Tiles.Sort();
            _modified = true;
            RecalculateMinMax();
        }

        public int Height { get; private set; }

        /// <summary>
        /// Extra pixels around the multi (background margin)
        /// </summary>
        public int ViewMargin { get; set; }

        public List<MultiTile> Tiles { get; private set; }

        /// <summary>
        /// Undo history, index 0 = last action
        /// </summary>
        public IReadOnlyList<UndoStruct> UndoList => _undo;

        public IReadOnlyList<UndoStruct> RedoList => _redo;

        public bool CanUndo => _undo.Count > 0;

        public bool CanRedo => _redo.Count > 0;

        public int Width { get; private set; }

        /// <summary>
        /// Cell that is the multi center (offset 0,0) when written to multi.mul / scripts
        /// </summary>
        public int OriginX { get; set; }

        public int OriginY { get; set; }

        public int XMax { get; private set; }

        private int XMaxOrg { get; set; }

        public int XMin { get; private set; }

        private int XMinOrg { get; set; }

        public int YMax { get; private set; }

        public int YMaxOrg { get; private set; }

        public int YMin { get; private set; }

        public int YMinOrg { get; private set; }

        public int ZMax { get; private set; }

        public int ZMin { get; private set; }

        public int RealTileCount => Tiles.Count(t => !t.IsVirtualFloor);

        public IEnumerable<MultiTile> RealTiles => Tiles.Where(t => !t.IsVirtualFloor);

        private static int Clamp(int z)
        {
            return Math.Max(-128, Math.Min(127, z));
        }

        public void SetDefaultOrigin()
        {
            OriginX = (int)Math.Round(Width / 2.0) - 1;
            OriginY = (int)Math.Round(Height / 2.0) - 1;
            if (OriginX < 0)
            {
                OriginX = Width / 2;
            }

            if (OriginY < 0)
            {
                OriginY = Height / 2;
            }
        }

        /// <summary>
        /// Export to given multi id
        /// </summary>
        public void AddToSdkComponentList(int id)
        {
            Multis.Add(id, ConvertToSdk());
            Options.ChangedUltimaClass["Multis"] = true;
            ControlEvents.FireMultiChangeEvent(this, id);
        }

        /// <summary>
        /// Number of 90° view rotations (the data is stored rotated, exports rotate back)
        /// </summary>
        public int ViewRotation { get; private set; }

        private IDictionary<ushort, ushort> _viewSwapMap;

        private static void RotateCellsCw(List<ImportTile> cells, ref int w, ref int h, ref int ox, ref int oy, IDictionary<ushort, ushort> swap)
        {
            foreach (ImportTile t in cells)
            {
                int nx = h - 1 - t.Y;
                t.Y = t.X;
                t.X = nx;
                if (swap != null && swap.TryGetValue(t.Id, out ushort n))
                {
                    t.Id = n;
                }
            }

            int nox = h - 1 - oy;
            oy = ox;
            ox = nox;
            (w, h) = (h, w);
        }

        /// <summary>
        /// All real tiles in cell coordinates in the original (not view rotated) orientation
        /// </summary>
        public List<ImportTile> GetCanonicalCells(out int width, out int height, out int originX, out int originY)
        {
            var cells = new List<ImportTile>();
            foreach (MultiTile tile in Tiles)
            {
                if (tile.IsVirtualFloor)
                {
                    continue;
                }

                cells.Add(new ImportTile { Id = tile.Id, X = tile.X, Y = tile.Y, Z = tile.Z, Flag = tile.Flag, Hue = tile.Hue, Deco = tile.IsDeco });
            }

            width = Width;
            height = Height;
            originX = OriginX;
            originY = OriginY;
            for (int i = 0; i < (4 - ViewRotation) % 4; ++i)
            {
                RotateCellsCw(cells, ref width, ref height, ref originX, ref originY, _viewSwapMap);
            }

            return cells;
        }

        /// <summary>
        /// View cell -> cell in original orientation (for map coordinates)
        /// </summary>
        public Point ViewToCanonical(int x, int y)
        {
            int w = Width;
            int h = Height;
            for (int i = 0; i < (4 - ViewRotation) % 4; ++i)
            {
                int nx = h - 1 - y;
                y = x;
                x = nx;
                (w, h) = (h, w);
            }

            return new Point(x, y);
        }

        /// <summary>
        /// Returns all real tiles as import tiles (offsets relative to the origin, original orientation)
        /// </summary>
        public List<ImportTile> ToImportTiles(bool includeDeco = true)
        {
            List<ImportTile> cells = GetCanonicalCells(out _, out _, out int ox, out int oy);
            var list = new List<ImportTile>(cells.Count);
            foreach (ImportTile t in cells)
            {
                if (!includeDeco && t.Deco)
                {
                    continue;
                }

                t.X -= ox;
                t.Y -= oy;
                list.Add(t);
            }

            return list;
        }

        /// <summary>
        /// Multi for multi.mul (decoration tiles are not included)
        /// </summary>
        public MultiComponentList ConvertToSdk()
        {
            var entries = new List<MultiComponentList.MultiTileEntry>();

            foreach (ImportTile t in ToImportTiles(false))
            {
                entries.Add(new MultiComponentList.MultiTileEntry
                {
                    ItemId = t.Id,
                    OffsetX = (short)t.X,
                    OffsetY = (short)t.Y,
                    OffsetZ = (short)t.Z,
                    Flags = t.Flag,
                    Unk1 = 0
                });
            }

            if (entries.Count == 0)
            {
                // the client needs at least one entry, use the invisible center item
                entries.Add(new MultiComponentList.MultiTileEntry { ItemId = 0x1, Flags = 0 });
            }

            return new MultiComponentList(entries);
        }

        public int DecoCount => Tiles.Count(t => !t.IsVirtualFloor && t.IsDeco);

        /// <summary>
        /// Sets XMin/XMax/YMin/YMax for drawing (with or without the virtual floor)
        /// </summary>
        public void PrepareBounds(bool drawFloor)
        {
            if (_modified)
            {
                RecalculateMinMax();
            }

            XMin = XMinOrg - ViewMargin;
            XMax = XMaxOrg + ViewMargin;
            YMin = YMinOrg - ViewMargin;
            YMax = YMaxOrg + ViewMargin;

            if (!drawFloor)
            {
                return;
            }

            int floorZMod = (-_parent.DrawFloorZ * 4) - 44 - ViewMargin;
            if (YMin > floorZMod)
            {
                YMin = floorZMod;
            }

            floorZMod = ((Width + Height) * 22) - (_parent.DrawFloorZ * 4) + ViewMargin;
            if (YMax < floorZMod)
            {
                YMax = floorZMod;
            }
        }

        /// <summary>
        /// Gets Bitmap of Multi and sets HoverTile
        /// </summary>
        public void GetImage(Graphics gfx, int xOff, int yOff, int maxHeight, Point mouseLoc, bool drawFloor)
        {
            if (Width == 0 || Height == 0)
            {
                return;
            }

            PrepareBounds(drawFloor);

            _parent.HoverTile = GetSelected(mouseLoc, maxHeight, drawFloor);

            Rectangle clipBounds = Rectangle.Round(gfx.ClipBounds);
            int minZ = _parent.MinVisibleZ;

            foreach (MultiTile tile in Tiles)
            {
                if (!tile.IsVirtualFloor && (tile.Z > maxHeight || tile.Z < minZ || _parent.IsTileHidden(tile)))
                {
                    continue;
                }

                Bitmap bmp = tile.GetBitmap();
                if (bmp == null)
                {
                    continue;
                }

                _drawDestRectangle.X = tile.XMod;
                _drawDestRectangle.Y = tile.YMod;
                _drawDestRectangle.X -= XMin + xOff;
                _drawDestRectangle.Y -= YMin + yOff;
                _drawDestRectangle.Width = bmp.Width;
                _drawDestRectangle.Height = bmp.Height;

                if (!clipBounds.IntersectsWith(_drawDestRectangle))
                {
                    continue;
                }

                if (tile.IsVirtualFloor)
                {
                    if (drawFloor)
                    {
                        gfx.DrawImageUnscaled(bmp, _drawDestRectangle);
                    }
                }
                else
                {
                    if (_parent.HoverTile != null && _parent.HoverTile == tile)
                    {
                        gfx.DrawImage(bmp, _drawDestRectangle, 0, 0, _drawDestRectangle.Width,
                            _drawDestRectangle.Height, GraphicsUnit.Pixel, MultiTile.HoverColor);
                    }
                    else if ((_parent.SelectedTile != null && _parent.SelectedTile == tile) || _parent.IsInSelection(tile))
                    {
                        gfx.DrawImage(bmp, _drawDestRectangle, 0, 0, _drawDestRectangle.Width,
                            _drawDestRectangle.Height, GraphicsUnit.Pixel, MultiTile.SelectedColor);
                    }
                    else if (tile.Transparent || _parent.IsTileGhost(tile))
                    {
                        gfx.DrawImage(bmp, _drawDestRectangle, 0, 0, _drawDestRectangle.Width,
                            _drawDestRectangle.Height, GraphicsUnit.Pixel, MultiTile.TransColor);
                    }
                    else if (_parent.ShowWalkables && tile.Walkable)
                    {
                        gfx.DrawImage(bmp, _drawDestRectangle, 0, 0, _drawDestRectangle.Width,
                            _drawDestRectangle.Height, GraphicsUnit.Pixel, MultiTile.StandableColor);
                    }
                    else if (_parent.ShowDoubleSurface && tile.DoubleSurface)
                    {
                        gfx.DrawImage(bmp, _drawDestRectangle, 0, 0, _drawDestRectangle.Width,
                            _drawDestRectangle.Height, GraphicsUnit.Pixel, MultiTile.StandableColor);
                    }
                    else if (_parent.ShowDecoMarked && tile.IsDeco)
                    {
                        gfx.DrawImage(bmp, _drawDestRectangle, 0, 0, _drawDestRectangle.Width,
                            _drawDestRectangle.Height, GraphicsUnit.Pixel, MultiTile.DecoColor);
                    }
                    else if (_parent.ShowInvisibleMarked && tile.Invisible)
                    {
                        gfx.DrawImage(bmp, _drawDestRectangle, 0, 0, _drawDestRectangle.Width,
                            _drawDestRectangle.Height, GraphicsUnit.Pixel, MultiTile.InvisibleColor);
                    }
                    else
                    {
                        gfx.DrawImageUnscaled(bmp, _drawDestRectangle);
                    }
                }
            }
        }

        /// <summary>
        /// Gets <see cref="MultiTile"/> from given Pixel Location
        /// </summary>
        private MultiTile GetSelected(Point mouseLoc, int maxHeight, bool drawFloor)
        {
            if (Width == 0 || Height == 0)
            {
                return null;
            }

            if (mouseLoc == Point.Empty)
            {
                return null;
            }

            int minZ = _parent.MinVisibleZ;

            MultiTile selected = null;
            for (int i = Tiles.Count - 1; i >= 0; --i) // inverse for speedup
            {
                MultiTile tile = Tiles[i];
                if (tile.IsVirtualFloor)
                {
                    continue;
                }

                if (tile.Z > maxHeight || tile.Z < minZ || _parent.IsTileHidden(tile) || _parent.IsTileLocked(tile))
                {
                    continue;
                }

                if (drawFloor && _parent.DrawFloorZ > tile.Z)
                {
                    continue;
                }

                Bitmap bmp = tile.GetBitmap();
                if (bmp == null)
                {
                    continue;
                }

                int px = tile.XMod;
                int py = tile.YMod;
                px -= XMin;
                py -= YMin;

                if (mouseLoc.X <= px || mouseLoc.X >= px + bmp.Width || mouseLoc.Y <= py || mouseLoc.Y >= py + bmp.Height)
                {
                    continue;
                }

                // Check for transparent part
                Color p = bmp.GetPixel(mouseLoc.X - px, mouseLoc.Y - py);
                if (p.A == 0 || (p.R == 0 && p.G == 0 && p.B == 0))
                {
                    continue;
                }

                selected = tile;
                break;
            }

            return selected;
        }

        /// <summary>
        /// Resizes Multi size
        /// </summary>
        public void Resize(int width, int height)
        {
            AddToUndoList("Resize");
            ResizeInternal(width, height);
        }

        private void ResizeInternal(int width, int height)
        {
            if (Width > width || Height > height)
            {
                for (int i = Tiles.Count - 1; i >= 0; --i)
                {
                    MultiTile tile = Tiles[i];
                    if (tile.X >= width || tile.Y >= height)
                    {
                        Tiles.RemoveAt(i);
                    }
                }
            }

            if (Width < width || Height < height)
            {
                for (int x = 0; x < width; ++x)
                {
                    for (int y = 0; y < height; ++y)
                    {
                        if (x < Width && y < Height)
                        {
                            continue;
                        }

                        Tiles.Add(new FloorTile(x, y, _parent.DrawFloorZ));
                    }
                }
            }

            Width = width;
            Height = height;
            Tiles.Sort();
            _modified = true;
            RecalculateMinMax();
        }

        /// <summary>
        /// Moves all tiles so that the used area starts at 0,0 and shrinks the multi to the used size (+border)
        /// </summary>
        public void Crop(int border)
        {
            List<MultiTile> real = RealTiles.ToList();
            if (real.Count == 0)
            {
                return;
            }

            AddToUndoList("Crop");
            int minX = real.Min(t => t.X) - border;
            int minY = real.Min(t => t.Y) - border;
            int maxX = real.Max(t => t.X) + border;
            int maxY = real.Max(t => t.Y) + border;

            Tiles.RemoveAll(t => t.IsVirtualFloor);
            foreach (MultiTile t in Tiles)
            {
                t.X -= minX;
                t.Y -= minY;
            }

            OriginX -= minX;
            OriginY -= minY;
            Width = 0;
            Height = 0;
            ResizeInternal(maxX - minX + 1, maxY - minY + 1);
            CalcSolver();
            Tiles.Sort();
        }

        public void SetFloorZ(int z)
        {
            foreach (MultiTile tile in Tiles)
            {
                if (tile.IsVirtualFloor)
                {
                    tile.Z = z;
                }
            }

            Tiles.Sort();
        }

        private bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        /// <summary>
        /// Adds an <see cref="MultiTile"/> to specific location
        /// </summary>
        public void TileAdd(int x, int y, int z, ushort id)
        {
            TileAdd(x, y, z, id, 0);
        }

        public void TileAdd(int x, int y, int z, ushort id, int hue, bool deco = false)
        {
            if (!InBounds(x, y))
            {
                return;
            }

            AddToUndoList("Add Tile");
            MultiTile tile = new MultiTile(id, x, y, Clamp(z), 1) { Hue = hue, IsDeco = deco };
            Tiles.Add(tile);
            CalcSolver(x, y);
            _modified = true;
            Tiles.Sort();
            RecalculateMinMax(tile);
        }

        /// <summary>
        /// Adds many tiles as one undo step. Tiles outside the multi are skipped.
        /// </summary>
        public int TilesAdd(IEnumerable<ImportTile> tiles, string action, bool relativeToOrigin = false)
        {
            List<ImportTile> list = tiles.ToList();
            if (list.Count == 0)
            {
                return 0;
            }

            AddToUndoList(action);
            int count = 0;
            foreach (ImportTile t in list)
            {
                int x = relativeToOrigin ? t.X + OriginX : t.X;
                int y = relativeToOrigin ? t.Y + OriginY : t.Y;
                if (!InBounds(x, y))
                {
                    continue;
                }

                Tiles.Add(new MultiTile(t.Id, x, y, Clamp(t.Z), t.Flag) { Hue = t.Hue, IsDeco = t.Deco });
                ++count;
            }

            CalcSolver();
            Tiles.Sort();
            _modified = true;
            RecalculateMinMax();
            return count;
        }

        /// <summary>
        /// Moves given <see cref="MultiTile"/>
        /// </summary>
        public void TileMove(MultiTile tile, int newX, int newY)
        {
            if (Width == 0 || Height == 0 || tile == null)
            {
                return;
            }

            AddToUndoList("Move Tile");

            tile.X = newX;
            tile.Y = newY;
            CalcSolver(newX, newY);
            Tiles.Sort();
            _modified = true;
            RecalculateMinMax(tile);
        }

        /// <summary>
        /// Moves tiles by an offset (one undo step). Tiles leaving the multi are clamped to the border.
        /// </summary>
        public void TilesMove(ICollection<MultiTile> tiles, int dx, int dy, int dz)
        {
            if (tiles == null || tiles.Count == 0)
            {
                return;
            }

            AddToUndoList("Move Tiles");
            foreach (MultiTile tile in tiles)
            {
                tile.X = Math.Max(0, Math.Min(Width - 1, tile.X + dx));
                tile.Y = Math.Max(0, Math.Min(Height - 1, tile.Y + dy));
                tile.Z = Clamp(tile.Z + dz);
            }

            CalcSolver();
            Tiles.Sort();
            _modified = true;
            RecalculateMinMax();
        }

        /// <summary>
        /// Removes specific <see cref="MultiTile"/>
        /// </summary>
        public void TileRemove(MultiTile tile)
        {
            if (Width == 0 || Height == 0 || tile == null)
            {
                return;
            }

            AddToUndoList("Remove Tile");

            Tiles.Remove(tile);
            _modified = true;
            RecalculateMinMax();
        }

        public void TilesRemove(ICollection<MultiTile> tiles, string action = "Remove Tiles")
        {
            if (tiles == null || tiles.Count == 0)
            {
                return;
            }

            AddToUndoList(action);
            var set = new HashSet<MultiTile>(tiles);
            Tiles.RemoveAll(t => !t.IsVirtualFloor && set.Contains(t));
            _modified = true;
            RecalculateMinMax();
        }

        /// <summary>
        /// Alters Z level for given <see cref="MultiTile"/>
        /// </summary>
        public void TileZMod(MultiTile tile, int modZ)
        {
            AddToUndoList("Modify Z");
            tile.Z = Clamp(tile.Z + modZ);

            CalcSolver(tile.X, tile.Y);
            Tiles.Sort();
            _modified = true;
            RecalculateMinMax(tile);
        }

        /// <summary>
        /// Sets Z value of given <see cref="MultiTile"/>
        /// </summary>
        public void TileZSet(MultiTile tile, int setZ)
        {
            AddToUndoList("Set Z");
            tile.Z = Clamp(setZ);

            CalcSolver(tile.X, tile.Y);
            Tiles.Sort();
            _modified = true;
            RecalculateMinMax(tile);
        }

        public void TilesSetHue(ICollection<MultiTile> tiles, int hue)
        {
            if (tiles == null || tiles.Count == 0)
            {
                return;
            }

            AddToUndoList("Set Hue");
            foreach (MultiTile t in tiles)
            {
                t.Hue = hue;
            }

            _modified = true;
        }

        public void TilesSetDeco(ICollection<MultiTile> tiles, bool deco)
        {
            if (tiles == null || tiles.Count == 0)
            {
                return;
            }

            AddToUndoList(deco ? "Set furniture" : "Remove furniture flag");
            foreach (MultiTile t in tiles)
            {
                t.IsDeco = deco;
            }
        }

        public void TilesSetInvisible(ICollection<MultiTile> tiles, bool invisible)
        {
            if (tiles == null || tiles.Count == 0)
            {
                return;
            }

            AddToUndoList(invisible ? "Set Invisible" : "Set Visible");
            foreach (MultiTile t in tiles)
            {
                t.Invisible = invisible;
            }
        }

        /// <summary>
        /// Replaces the item id of tiles (all tiles with id oldId when tiles is null)
        /// </summary>
        public int ReplaceId(ICollection<MultiTile> tiles, int oldId, ushort newId)
        {
            List<MultiTile> list = tiles != null
                ? tiles.ToList()
                : Tiles.Where(t => !t.IsVirtualFloor && t.Id == oldId).ToList();

            if (list.Count == 0)
            {
                return 0;
            }

            AddToUndoList("Replace Id");
            foreach (MultiTile t in list)
            {
                t.Set(newId, t.Z);
            }

            CalcSolver();
            Tiles.Sort();
            _modified = true;
            RecalculateMinMax();
            return list.Count;
        }

        /// <summary>
        /// Replaces ids using a map (style transfer). Returns number of replaced tiles.
        /// </summary>
        public int ReplaceIds(IDictionary<ushort, ushort> map, string action)
        {
            List<MultiTile> list = Tiles.Where(t => !t.IsVirtualFloor && map.ContainsKey(t.Id)).ToList();
            if (list.Count == 0)
            {
                return 0;
            }

            AddToUndoList(action);
            foreach (MultiTile t in list)
            {
                t.Set(map[t.Id], t.Z);
            }

            CalcSolver();
            Tiles.Sort();
            _modified = true;
            RecalculateMinMax();
            return list.Count;
        }

        /// <summary>
        /// Rotates the whole multi 90° clockwise. Optional id map swaps wall orientations.
        /// </summary>
        public void Rotate90(IDictionary<ushort, ushort> swapMap, bool viewOnly = false)
        {
            AddToUndoList(viewOnly ? "Rotate view" : "Rotate 90°");
            if (viewOnly)
            {
                ViewRotation = (ViewRotation + 1) % 4;
                _viewSwapMap = swapMap;
            }

            int oldHeight = Height;
            int oldWidth = Width;
            Tiles.RemoveAll(t => t.IsVirtualFloor);
            foreach (MultiTile t in Tiles)
            {
                int nx = oldHeight - 1 - t.Y;
                int ny = t.X;
                t.X = nx;
                t.Y = ny;
                if (swapMap != null && swapMap.TryGetValue(t.Id, out ushort n))
                {
                    t.Set(n, t.Z);
                }
            }

            int ox = oldHeight - 1 - OriginY;
            OriginY = OriginX;
            OriginX = ox;
            Width = 0;
            Height = 0;
            ResizeInternal(oldHeight, oldWidth);
            CalcSolver();
            Tiles.Sort();
        }

        /// <summary>
        /// Mirrors the multi (horizontal = along X axis)
        /// </summary>
        public void Mirror(bool alongX, IDictionary<ushort, ushort> swapMap)
        {
            AddToUndoList(alongX ? "Mirror X" : "Mirror Y");
            foreach (MultiTile t in Tiles)
            {
                if (t.IsVirtualFloor)
                {
                    continue;
                }

                if (alongX)
                {
                    t.X = Width - 1 - t.X;
                }
                else
                {
                    t.Y = Height - 1 - t.Y;
                }

                if (swapMap != null && swapMap.TryGetValue(t.Id, out ushort n))
                {
                    t.Set(n, t.Z);
                }
            }

            if (alongX)
            {
                OriginX = Width - 1 - OriginX;
            }
            else
            {
                OriginY = Height - 1 - OriginY;
            }

            CalcSolver();
            Tiles.Sort();
            _modified = true;
            RecalculateMinMax();
        }

        public void ClearAll()
        {
            AddToUndoList("Clear");
            Tiles.RemoveAll(t => !t.IsVirtualFloor);
            _modified = true;
            RecalculateMinMax();
        }

        public void Undo(int index)
        {
            for (int i = 0; i <= index && _undo.Count > 0; ++i)
            {
                Undo();
            }
        }

        public bool Undo()
        {
            if (_undo.Count == 0)
            {
                return false;
            }

            UndoStruct entry = _undo[0];
            _undo.RemoveAt(0);
            _redo.Insert(0, CreateSnapshot(entry.Action));
            Restore(entry);
            return true;
        }

        public bool Redo()
        {
            if (_redo.Count == 0)
            {
                return false;
            }

            UndoStruct entry = _redo[0];
            _redo.RemoveAt(0);
            _undo.Insert(0, CreateSnapshot(entry.Action));
            Restore(entry);
            return true;
        }

        private void Restore(UndoStruct entry)
        {
            Width = entry.Width;
            Height = entry.Height;
            OriginX = entry.OriginX;
            OriginY = entry.OriginY;
            ViewRotation = entry.ViewRotation;
            Tiles = new List<MultiTile>();
            foreach (MultiTile tile in entry.Tiles)
            {
                Tiles.Add(tile.Clone());
            }

            CalcSolver();
            Tiles.Sort();
            _modified = true;
            RecalculateMinMax();
        }

        public void UndoClear()
        {
            _undo.Clear();
            _redo.Clear();
        }

        public void CalcWalkable()
        {
            WalkableCount = 0;
            foreach (MultiTile tile in Tiles)
            {
                if (tile.IsVirtualFloor)
                {
                    continue;
                }

                ItemData itemData = TileData.ItemTable[tile.Id];
                if ((itemData.Flags & TileFlag.Surface) == 0)
                {
                    continue;
                }

                List<MultiTile> xyArray = GetMultiTileLitAtCoordinate(tile.X, tile.Y);

                int start = tile.Z + itemData.CalcHeight;
                if (tile.IsWalkable(start, xyArray))
                {
                    ++WalkableCount;
                }
            }
        }

        public void CalcDoubleSurface()
        {
            DoubleSurfaceCount = 0;
            foreach (MultiTile tile in Tiles)
            {
                if (tile.IsVirtualFloor)
                {
                    continue;
                }

                ItemData itemData = TileData.ItemTable[tile.Id];
                if ((itemData.Flags & TileFlag.Surface) == 0)
                {
                    continue;
                }

                List<MultiTile> xyArray = GetMultiTileLitAtCoordinate(tile.X, tile.Y);

                if (tile.IsDoubleSurface(xyArray))
                {
                    ++DoubleSurfaceCount;
                }
            }
        }

        private void CalcSolver()
        {
            var counter = new Dictionary<int, int>();
            foreach (MultiTile tile in Tiles)
            {
                if (tile.IsVirtualFloor)
                {
                    continue;
                }

                int key = (tile.X << 16) | (tile.Y & 0xFFFF);
                counter.TryGetValue(key, out int n);
                tile.Solver = n;
                counter[key] = n + 1;
            }
        }

        private void CalcSolver(int x, int y)
        {
            List<MultiTile> multiTiles = GetMultiTileLitAtCoordinate(x, y);
            for (int i = 0; i < multiTiles.Count; ++i)
            {
                multiTiles[i].Solver = i;
            }
        }

        public List<MultiTile> GetMultiTileLitAtCoordinate(int x, int y)
        {
            List<MultiTile> multiTiles = new List<MultiTile>();
            foreach (MultiTile tile in Tiles)
            {
                if (tile.IsVirtualFloor)
                {
                    continue;
                }

                if (tile.X != x || tile.Y != y)
                {
                    continue;
                }

                multiTiles.Add(tile);
            }

            return multiTiles;
        }

        public List<MultiTile> GetTilesInRect(int x1, int y1, int x2, int y2)
        {
            int minX = Math.Min(x1, x2);
            int maxX = Math.Max(x1, x2);
            int minY = Math.Min(y1, y2);
            int maxY = Math.Max(y1, y2);
            return Tiles.Where(t => !t.IsVirtualFloor && t.X >= minX && t.X <= maxX && t.Y >= minY && t.Y <= maxY).ToList();
        }

        private UndoStruct CreateSnapshot(string action)
        {
            var entry = new UndoStruct
            {
                Action = action,
                Tiles = new List<MultiTile>(Tiles.Count),
                Width = Width,
                Height = Height,
                OriginX = OriginX,
                OriginY = OriginY,
                ViewRotation = ViewRotation
            };

            foreach (MultiTile tile in Tiles)
            {
                entry.Tiles.Add(tile.Clone());
            }

            return entry;
        }

        public void AddToUndoList(string action)
        {
            _undo.Insert(0, CreateSnapshot(action));
            if (_undo.Count > _undoListMaxSize)
            {
                _undo.RemoveAt(_undo.Count - 1);
            }

            _redo.Clear();
        }

        /// <summary>
        /// Recalculates bitmap size
        /// </summary>
        private void RecalculateMinMax()
        {
            // CalcEdgeTiles
            YMin = -44; // 0,0
            YMax = (Width + Height) * 22; // width,height
            XMin = (-Height * 22) - 22; // 0,height
            XMax = (Width * 22) + 22; // width,0
            ZMin = 127;
            ZMax = -128;

            foreach (MultiTile tile in Tiles)
            {
                if (tile.IsVirtualFloor)
                {
                    continue;
                }

                Bitmap bmp = tile.GetBitmap();
                if (bmp == null)
                {
                    continue;
                }

                int px = tile.XMod - GapXMod;
                int py = tile.YMod - GapYMod;

                if (px < XMin)
                {
                    XMin = px;
                }

                if (py < YMin)
                {
                    YMin = py;
                }

                px += bmp.Width;
                py += bmp.Height;

                if (px > XMax)
                {
                    XMax = px;
                }

                if (py > YMax)
                {
                    YMax = py;
                }

                if (tile.Z > ZMax)
                {
                    ZMax = tile.Z;
                }

                if (tile.Z < ZMin)
                {
                    ZMin = tile.Z;
                }
            }

            if (ZMin > ZMax)
            {
                ZMin = 0;
                ZMax = 0;
            }

            _modified = false;
            XMinOrg = XMin;
            XMaxOrg = XMax;
            YMinOrg = YMin;
            YMaxOrg = YMax;

            if (_parent.ShowWalkables)
            {
                CalcWalkable();
            }

            if (_parent.ShowDoubleSurface)
            {
                CalcDoubleSurface();
            }
        }

        private void RecalculateMinMax(MultiTile tile)
        {
            if (tile.IsVirtualFloor)
            {
                return;
            }

            Bitmap bmp = tile.GetBitmap();
            if (bmp == null)
            {
                return;
            }

            XMin = XMinOrg;
            XMax = XMaxOrg;
            YMin = YMinOrg;
            YMax = YMaxOrg;

            int px = tile.XMod - GapXMod;
            int py = tile.YMod - GapYMod;

            if (px < XMin)
            {
                XMin = px;
            }

            if (py < YMin)
            {
                YMin = py;
            }

            px += bmp.Width;
            py += bmp.Height;

            if (px > XMax)
            {
                XMax = px;
            }

            if (py > YMax)
            {
                YMax = py;
            }

            if (tile.Z > ZMax)
            {
                ZMax = tile.Z;
            }

            if (tile.Z < ZMin)
            {
                ZMin = tile.Z;
            }

            _modified = false;

            XMinOrg = XMin;
            XMaxOrg = XMax;
            YMinOrg = YMin;
            YMaxOrg = YMax;

            if (_parent.ShowWalkables)
            {
                CalcWalkable();
            }

            if (_parent.ShowDoubleSurface)
            {
                CalcDoubleSurface();
            }
        }

        public struct UndoStruct
        {
            public string Action;
            public int Height;
            public List<MultiTile> Tiles;
            public int Width;
            public int OriginX;
            public int OriginY;
            public int ViewRotation;
        }
    }

    public class FloorTile : MultiTile
    {
        private static Bitmap _floorBmp;

        public FloorTile(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override bool IsVirtualFloor => true;

        public override MultiTile Clone()
        {
            return new FloorTile(X, Y, Z);
        }

        public override Bitmap GetBitmap()
        {
            if (_floorBmp != null)
            {
                return _floorBmp;
            }

            _floorBmp = new Bitmap(44, 44);

            using (Graphics g = Graphics.FromImage(_floorBmp))
            {
                using (Brush floorBrush = new SolidBrush(Color.FromArgb(96, 32, 192, 32)))
                {
                    Point[] drawFloorPoint = new Point[4];
                    drawFloorPoint[0].X = 22;
                    drawFloorPoint[0].Y = 0;
                    drawFloorPoint[1].X = 44;
                    drawFloorPoint[1].Y = 22;
                    drawFloorPoint[2].X = 22;
                    drawFloorPoint[2].Y = 44;
                    drawFloorPoint[3].X = 0;
                    drawFloorPoint[3].Y = 22;

                    g.FillPolygon(floorBrush, drawFloorPoint);
                    g.DrawPolygon(Pens.White, drawFloorPoint);
                }
            }

            return _floorBmp;
        }
    }

    public class MultiTile : IComparable
    {
        private static readonly ColorMatrix _drawMatrix = new ColorMatrix(new float[][]
        {
            new float[] {0, 0, 0, 0, 0},
            new float[] {0, 0, 0, 0, 0},
            new float[] {0, 0, 0, 0, 0},
            new float[] {0, 0, 0, .5f, 0},
            new float[] {0, 0, 0, 0, 1}
        });

        private static readonly ColorMatrix _hoverMatrix = new ColorMatrix(new float[][]
        {
            new float[] {1, 0, 0, 0, 0},
            new float[] {0, 1, 0, 0, 0},
            new float[] {0, 0, 1, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {0, 0, .8f, 0, 1}
        });

        private static readonly ColorMatrix _selectedMatrix = new ColorMatrix(new float[][]
        {
            new float[] {0, 1, 0, 0, 0},
            new float[] {0, 0, 1, 0, 0},
            new float[] {1, 0, 0, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {.8f, 0, 0, 0, 1}
        });

        private static readonly ColorMatrix _transMatrix = new ColorMatrix(new float[][]
        {
            new float[] {1, 0, 0, 0, 0},
            new float[] {0, 1, 0, 0, 0},
            new float[] {0, 0, 1, 0, 0},
            new float[] {0, 0, 0, .3f, 0},
            new float[] {0, 0, 0, 0, 1}
        });

        private static readonly ColorMatrix _standableMatrix = new ColorMatrix(new float[][]
        {
            new float[] {0, 0, .5f, 0, 0},
            new float[] {0, .8f, 0, .8f, 0},
            new float[] {.8f, 0, 0, 0, .8f},
            new float[] {0, .8f, 0, .8f, 0},
            new float[] {0, 0, .5f, 0, 0}
        });

        private static readonly ColorMatrix _invisibleMatrix = new ColorMatrix(new float[][]
        {
            new float[] {.6f, 0, 0, 0, 0},
            new float[] {0, .6f, 0, 0, 0},
            new float[] {0, 0, .6f, 0, 0},
            new float[] {0, 0, 0, .6f, 0},
            new float[] {.4f, 0, .4f, 0, 1}
        });

        private static readonly ColorMatrix _decoMatrix = new ColorMatrix(new float[][]
        {
            new float[] {.85f, 0, 0, 0, 0},
            new float[] {0, .85f, 0, 0, 0},
            new float[] {0, 0, .85f, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {.15f, .12f, 0, 0, 1}
        });

        private static readonly Dictionary<long, Bitmap> _huedCache = new Dictionary<long, Bitmap>();

        private int _x;
        private int _y;
        private int _z;
        private int _hue;

        public MultiTile(ushort id, int x, int y, int z, int flag)
        {
            Id = id;
            X = x;
            Y = y;
            Z = z;
            Flag = flag;
            Solver = 0;
        }

        public MultiTile(ushort id, int x, int y, int z, bool flag)
        {
            Id = id;
            X = x;
            Y = y;
            Z = z;
            Invisible = flag;
            Solver = 0;
        }

        public MultiTile()
        {
            Id = 0xFFFF;
            Flag = 1;
        }

        static MultiTile()
        {
            HoverColor = new ImageAttributes();
            HoverColor.SetColorMatrix(_hoverMatrix);

            SelectedColor = new ImageAttributes();
            SelectedColor.SetColorMatrix(_selectedMatrix);

            DrawColor = new ImageAttributes();
            DrawColor.SetColorMatrix(_drawMatrix);

            TransColor = new ImageAttributes();
            TransColor.SetColorMatrix(_transMatrix);

            StandableColor = new ImageAttributes();
            StandableColor.SetColorMatrix(_standableMatrix);

            InvisibleColor = new ImageAttributes();
            InvisibleColor.SetColorMatrix(_invisibleMatrix);

            DecoColor = new ImageAttributes();
            DecoColor.SetColorMatrix(_decoMatrix);
        }

        public static ImageAttributes DrawColor { get; }

        public int Height => TileData.ItemTable[Id].Height;

        public static ImageAttributes HoverColor { get; }

        public ushort Id { get; private set; }

        public virtual bool IsVirtualFloor => false;

        public static ImageAttributes SelectedColor { get; }

        public static ImageAttributes TransColor { get; }

        public static ImageAttributes StandableColor { get; }

        public static ImageAttributes InvisibleColor { get; }

        public static ImageAttributes DecoColor { get; }

        public int X
        {
            get => _x;
            set
            {
                _x = value;
                RecalculateMod();
            }
        }

        public int XMod { get; private set; }

        public int Y
        {
            get => _y;
            set
            {
                _y = value;
                RecalculateMod();
            }
        }

        public int YMod { get; private set; }

        public int Z
        {
            get => _z;
            set
            {
                _z = value;
                RecalculateMod();
            }
        }

        /// <summary>
        /// Raw multi flag (1 = visible/normal, 0 = invisible/dynamic). Other values are kept as they are.
        /// </summary>
        public int Flag { get; set; } = 1;

        public bool Invisible
        {
            get => Flag == 0;
            set => Flag = value ? 0 : (Flag == 0 ? 1 : Flag);
        }

        /// <summary>
        /// Hue (only used for previews and ServUO addon scripts, multi.mul has no hues)
        /// </summary>
        public int Hue
        {
            get => _hue;
            set
            {
                _hue = Math.Max(0, value);
                RecalculateMod();
            }
        }

        /// <summary>
        /// Furniture / decoration: not written into multi.mul, only exported as addon
        /// </summary>
        public bool IsDeco { get; set; }

        public bool Walkable { get; private set; }

        public bool DoubleSurface { get; private set; }

        public bool Transparent { get; set; }

        public int Solver { get; set; }

        public virtual MultiTile Clone()
        {
            return new MultiTile(Id, X, Y, Z, Flag) { Hue = Hue, Solver = Solver, IsDeco = IsDeco };
        }

        public static void ClearHueCache()
        {
            foreach (Bitmap b in _huedCache.Values)
            {
                b.Dispose();
            }

            _huedCache.Clear();
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (obj is not MultiTile multiTile)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (X > multiTile.X)
            {
                return 1;
            }

            if (X < multiTile.X)
            {
                return -1;
            }

            if (Y > multiTile.Y)
            {
                return 1;
            }

            if (Y < multiTile.Y)
            {
                return -1;
            }

            if (!multiTile.IsVirtualFloor && !IsVirtualFloor)
            {
                ItemData ourData = TileData.ItemTable[Id];
                ItemData theirData = TileData.ItemTable[multiTile.Id];

                int ourThreshold = 0;
                if (ourData.Height > 0)
                {
                    ++ourThreshold;
                }

                if (!ourData.Background)
                {
                    ++ourThreshold;
                }

                int ourZ = Z;

                int theirThreshold = 0;
                if (theirData.Height > 0)
                {
                    ++theirThreshold;
                }

                if (!theirData.Background)
                {
                    ++theirThreshold;
                }

                int theirZ = multiTile.Z;

                ourZ += ourThreshold;
                theirZ += theirThreshold;

                int res = ourZ - theirZ;
                if (res == 0)
                {
                    res = ourThreshold - theirThreshold;
                }

                if (res == 0)
                {
                    res = Solver - multiTile.Solver;
                }

                return res;
            }
            else
            {
                if (Z > multiTile.Z)
                {
                    return 1;
                }

                if (multiTile.Z > Z)
                {
                    return -1;
                }

                if (multiTile.IsVirtualFloor && IsVirtualFloor)
                {
                    return 0;
                }

                if (multiTile.IsVirtualFloor)
                {
                    return 1;
                }

                if (IsVirtualFloor)
                {
                    return -1;
                }
            }

            return 0;
        }

        public virtual Bitmap GetBitmap()
        {
            if (Id == 0xFFFF)
            {
                return null;
            }

            Bitmap bmp = Art.GetStatic(Id);
            if (bmp == null || _hue <= 0)
            {
                return bmp;
            }

            return GetHuedBitmap(Id, _hue, bmp);
        }

        public static Bitmap GetHuedBitmap(ushort id, int hue, Bitmap source)
        {
            long key = ((long)id << 16) | (uint)(hue & 0xFFFF);
            if (_huedCache.TryGetValue(key, out Bitmap cached))
            {
                return cached;
            }

            try
            {
                Bitmap copy = source.Clone(new Rectangle(0, 0, source.Width, source.Height), PixelFormat.Format16bppArgb1555);
                bool partial = (TileData.ItemTable[id].Flags & TileFlag.PartialHue) != 0;
                Hues.GetHue((hue & 0x3FFF) - 1).ApplyTo(copy, partial);
                _huedCache[key] = copy;
                return copy;
            }
            catch
            {
                _huedCache[key] = source;
                return source;
            }
        }

        public void Set(ushort id, int z)
        {
            Id = id;
            Z = z;

            RecalculateMod();
        }

        private void RecalculateMod()
        {
            Bitmap bmp = GetBitmap();
            if (bmp != null)
            {
                XMod = (_x - _y) * 22;
                XMod -= bmp.Width / 2;
                XMod += MultiEditorComponentList.GapXMod;
                YMod = (_x + _y) * 22;
                YMod -= _z * 4;
                YMod -= bmp.Height;
                YMod += MultiEditorComponentList.GapYMod;
            }
            else if (IsVirtualFloor)
            {
                XMod = (_x - _y) * 22;
                XMod -= 44 / 2;
                XMod += MultiEditorComponentList.GapXMod;
                YMod = (_x + _y) * 22;
                YMod -= _z * 4;
                YMod -= 44;
                YMod += MultiEditorComponentList.GapYMod;
            }
        }

        private const TileFlag _impassableOrSurface = TileFlag.Impassable | TileFlag.Surface;

        public bool IsWalkable(int z, List<MultiTile> tiles)
        {
            int top = z + 16; // Player Height

            foreach (MultiTile tile in tiles)
            {
                ItemData itemData = TileData.ItemTable[tile.Id];
                if ((itemData.Flags & _impassableOrSurface) == 0)
                {
                    continue;
                }

                if ((itemData.Flags & TileFlag.Door) != 0)
                {
                    continue;
                }

                int checkTop = tile.Z + itemData.CalcHeight;
                if (checkTop <= z || tile.Z >= top)
                {
                    continue;
                }

                Walkable = false;
                return false;
            }

            Walkable = true;
            return true;
        }

        public bool IsDoubleSurface(List<MultiTile> tiles)
        {
            foreach (MultiTile tile in tiles)
            {
                if (tile.Z != Z)
                {
                    continue;
                }

                ItemData itemData = TileData.ItemTable[tile.Id];
                if ((itemData.Flags & TileFlag.Surface) == 0)
                {
                    continue;
                }

                if (tile == this)
                {
                    continue;
                }

                DoubleSurface = true;
                return true;
            }

            DoubleSurface = false;
            return false;
        }
    }
}

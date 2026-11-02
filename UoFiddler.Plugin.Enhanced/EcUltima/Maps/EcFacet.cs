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
using System.Globalization;
using System.IO;
using System.Linq;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Maps
{
    /// <summary>
    /// One map of the Enhanced Client (facetN.uop or facetNx.uop). The file holds one entry per 64 x 64 sector
    /// ("build/sectors/facet_{N:D2}/{index:D8}.bin") and one 8 byte entry with the map size (uint height, uint width).
    /// The radar image of the map is the file facet{N:D2}.dds in the client folder (one pixel per tile).
    /// </summary>
    public sealed class EcFacet
    {
        public const string SectorPattern = "build/sectors/facet_{0:D2}/{1:D8}.bin";

        private readonly Dictionary<int, EcSector> _cache = new Dictionary<int, EcSector>();
        private readonly HashSet<int> _modified = new HashSet<int>();

        public EcUopPackage Package { get; }
        public string FileName { get; }
        public int FacetId { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int SectorsX => (Width + EcSector.Size - 1) / EcSector.Size;
        public int SectorsY => (Height + EcSector.Size - 1) / EcSector.Size;
        public bool IsModified => _modified.Count > 0;

        /// <summary>Known map sizes (used when the size entry is missing).</summary>
        private static readonly (int Width, int Height)[] _defaultSizes =
        {
            (7168, 4096), (7168, 4096), (2304, 1600), (2560, 2048), (1448, 1448), (1280, 4096)
        };

        /// <summary>
        /// Opens the map with a given size (for maps without the size entry data.map, e.g. custom maps);
        /// 0 = take the size from the file.
        /// </summary>
        public EcFacet(EcUopPackage package, string fileName, int width, int height)
            : this(package, fileName)
        {
            if (width > 0 && height > 0 && (width != Width || height != Height))
            {
                Width = width;
                Height = height;
                package.ResolveNames(string.Format(CultureInfo.InvariantCulture, "build/sectors/facet_{0:D2}/", FacetId) + "{0:D8}.bin", 0, Math.Max(0, (SectorsX * SectorsY) - 1));
            }
        }

        /// <summary>True when the size comes from the size entry of the file (data.map).</summary>
        public bool HasSizeEntry { get; private set; }

        public EcFacet(EcUopPackage package, string fileName)
        {
            Package = package;
            FileName = fileName;
            FacetId = ParseFacetId(fileName);

            // the size entry "build/sectors/facet_NN/data.map" (uint height, uint width); older files: the only 8 byte entry
            package.ResolveNames(string.Format(CultureInfo.InvariantCulture, EcMapConverter.SizeEntryPattern, FacetId), 0, 0);
            EcUopEntry sizeEntry = package.GetEntry(string.Format(CultureInfo.InvariantCulture, EcMapConverter.SizeEntryPattern, FacetId))
                                   ?? package.Entries.FirstOrDefault(e => e.DecompressedSize == 8);
            if (sizeEntry != null)
            {
                byte[] size = package.ReadData(sizeEntry);
                Height = BitConverter.ToInt32(size, 0);
                Width = BitConverter.ToInt32(size, 4);
                HasSizeEntry = true;
            }
            else if (FacetId >= 0 && FacetId < _defaultSizes.Length)
            {
                (Width, Height) = _defaultSizes[FacetId];
            }

            package.ResolveNames(string.Format(CultureInfo.InvariantCulture, "build/sectors/facet_{0:D2}/", FacetId) + "{0:D8}.bin", 0, Math.Max(0, (SectorsX * SectorsY) - 1));
        }

        /// <summary>"facet3x.uop" -> 3.</summary>
        public static int ParseFacetId(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName).ToLowerInvariant();
            string digits = new string(name.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) ? id : -1;
        }

        public static bool IsFacetFile(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName).ToLowerInvariant();
            return name.StartsWith("facet", StringComparison.Ordinal) && name != "facets" && ParseFacetId(fileName) >= 0;
        }

        public string SectorName(int index) => string.Format(CultureInfo.InvariantCulture, SectorPattern, FacetId, index);

        public int SectorIndex(int sectorX, int sectorY) => (sectorX * SectorsY) + sectorY;

        public bool HasSector(int sectorX, int sectorY) => Package.GetEntry(SectorName(SectorIndex(sectorX, sectorY))) != null;

        /// <summary>Sector at the sector position, or null if it doesn't exist.</summary>
        public EcSector GetSector(int sectorX, int sectorY)
        {
            if (sectorX < 0 || sectorY < 0 || sectorX >= SectorsX || sectorY >= SectorsY)
            {
                return null;
            }

            int index = SectorIndex(sectorX, sectorY);
            if (_cache.TryGetValue(index, out EcSector cached))
            {
                return cached;
            }

            EcUopEntry entry = Package.GetEntry(SectorName(index));
            if (entry == null)
            {
                return null;
            }

            EcSector sector = EcSector.Read(Package.ReadData(entry));
            if (_cache.Count > 400)
            {
                // keep changed sectors, drop the others
                foreach (int key in _cache.Keys.Where(k => !_modified.Contains(k)).ToList())
                {
                    _cache.Remove(key);
                }
            }
            _cache[index] = sector;
            return sector;
        }

        /// <summary>Tile at map coordinates.</summary>
        public EcMapTile GetTile(int x, int y)
        {
            EcSector sector = GetSector(x / EcSector.Size, y / EcSector.Size);
            return sector?[x % EcSector.Size, y % EcSector.Size];
        }

        /// <summary>Releases a read sector from memory (not when it was changed).</summary>
        public void Forget(int sectorX, int sectorY)
        {
            int index = SectorIndex(sectorX, sectorY);
            if (!_modified.Contains(index))
            {
                _cache.Remove(index);
            }
        }

        public void MarkModified(EcSector sector)
        {
            _modified.Add(sector.Index);
            _cache[sector.Index] = sector;
        }

        /// <summary>
        /// Writes the changed sectors into the package (the package still needs to be saved).
        /// </summary>
        public void Apply()
        {
            foreach (int index in _modified)
            {
                if (_cache.TryGetValue(index, out EcSector sector))
                {
                    Package.AddOrReplace(SectorName(index), sector.Write(), EcUopCompression.Zlib);
                }
            }
            _modified.Clear();
        }

        /// <summary>The map area clipped to the size of the map.</summary>
        public System.Drawing.Rectangle Clip(System.Drawing.Rectangle area)
        {
            area.Intersect(new System.Drawing.Rectangle(0, 0, Width, Height));
            return area;
        }

        /// <summary>All statics of an area (map coordinates), sorted by x, y.</summary>
        public List<(int X, int Y, EcStatic Static)> GetStatics(System.Drawing.Rectangle area, int limit = int.MaxValue)
        {
            var result = new List<(int, int, EcStatic)>();
            area = Clip(area);
            for (int x = area.Left; x < area.Right && result.Count < limit; x++)
            {
                for (int y = area.Top; y < area.Bottom && result.Count < limit; y++)
                {
                    EcMapTile tile = GetTile(x, y);
                    if (tile == null)
                    {
                        continue;
                    }
                    foreach (EcStatic st in tile.Statics)
                    {
                        result.Add((x, y, st));
                    }
                }
            }
            return result;
        }

        /// <summary>Adds a static at map coordinates (the package still needs to be saved). False outside the map.</summary>
        public bool AddStatic(int x, int y, EcStatic st)
        {
            EcSector sector = x >= 0 && y >= 0 && x < Width && y < Height ? GetSector(x / EcSector.Size, y / EcSector.Size) : null;
            if (sector == null)
            {
                return false;
            }
            sector[x % EcSector.Size, y % EcSector.Size].Statics.Add(st);
            MarkModified(sector);
            return true;
        }

        /// <summary>Removes a static at map coordinates.</summary>
        public bool RemoveStatic(int x, int y, EcStatic st)
        {
            EcSector sector = x >= 0 && y >= 0 && x < Width && y < Height ? GetSector(x / EcSector.Size, y / EcSector.Size) : null;
            if (sector == null || !sector[x % EcSector.Size, y % EcSector.Size].Statics.Remove(st))
            {
                return false;
            }
            MarkModified(sector);
            return true;
        }

        /// <summary>Marks the sector of map coordinates as changed (after a static was edited in place).</summary>
        public void MarkModified(int x, int y)
        {
            EcSector sector = x >= 0 && y >= 0 && x < Width && y < Height ? GetSector(x / EcSector.Size, y / EcSector.Size) : null;
            if (sector != null)
            {
                MarkModified(sector);
            }
        }

        /// <summary>Moves a static to another tile (keeps its z). False when the static or the target is not there.</summary>
        public bool MoveStatic(int x, int y, EcStatic st, int newX, int newY)
        {
            if (newX < 0 || newY < 0 || newX >= Width || newY >= Height || GetTile(newX, newY) == null)
            {
                return false;
            }
            return RemoveStatic(x, y, st) && AddStatic(newX, newY, st);
        }

        /// <summary>
        /// Sets the land tile (and optionally the z) of a tile. Call <see cref="RefreshBorders"/> for the changed area
        /// afterwards, so the neighbour sectors know the new land (terrain blending).
        /// </summary>
        public bool SetLand(int x, int y, ushort landId, sbyte? z)
        {
            EcMapTile tile = x >= 0 && y >= 0 && x < Width && y < Height ? GetTile(x, y) : null;
            if (tile == null)
            {
                return false;
            }
            tile.LandId = landId;
            if (z.HasValue)
            {
                tile.Z = z.Value;
            }
            MarkModified(x, y);
            return true;
        }

        /// <summary>
        /// Builds the border layers (land of the neighbour tiles in the next sector) again for every border tile
        /// in the area and one tile around it.
        /// </summary>
        public int RefreshBorders(System.Drawing.Rectangle area)
        {
            area = Clip(System.Drawing.Rectangle.Inflate(area, 1, 1));
            int count = 0;
            for (int x = area.Left; x < area.Right; x++)
            {
                for (int y = area.Top; y < area.Bottom; y++)
                {
                    int lx = x % EcSector.Size, ly = y % EcSector.Size;
                    if (lx != 0 && ly != 0 && lx != EcSector.Size - 1 && ly != EcSector.Size - 1)
                    {
                        continue;
                    }
                    EcMapTile tile = GetTile(x, y);
                    if (tile == null)
                    {
                        continue;
                    }
                    List<EcLandLayer> layers = EcMapConverter.BorderLayers(x, y, Width, Height, (nx, ny) =>
                    {
                        EcMapTile n = GetTile(nx, ny);
                        return n == null ? ((ushort)0, (sbyte)0) : (n.LandId, n.Z);
                    });
                    tile.Layers.Clear();
                    tile.Layers.AddRange(layers);
                    MarkModified(x, y);
                    count++;
                }
            }
            return count;
        }

        /// <summary>Removes all statics of an area; returns the number removed.</summary>
        public int ClearStatics(System.Drawing.Rectangle area)
        {
            int count = 0;
            area = Clip(area);
            for (int x = area.Left; x < area.Right; x++)
            {
                for (int y = area.Top; y < area.Bottom; y++)
                {
                    EcMapTile tile = GetTile(x, y);
                    if (tile != null && tile.Statics.Count > 0)
                    {
                        count += tile.Statics.Count;
                        tile.Statics.Clear();
                        MarkModified(x, y);
                    }
                }
            }
            return count;
        }

        /// <summary>Path of the radar image (facet{N:D2}.dds) in the folder, or null.</summary>
        public static string FindRadarImage(string folder, int facetId)
        {
            string path = Path.Combine(folder, $"facet{facetId:D2}.dds");
            return File.Exists(path) ? path : null;
        }
    }
}

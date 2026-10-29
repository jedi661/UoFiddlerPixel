/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
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
        public int Width { get; }
        public int Height { get; }
        public int SectorsX => (Width + EcSector.Size - 1) / EcSector.Size;
        public int SectorsY => (Height + EcSector.Size - 1) / EcSector.Size;
        public bool IsModified => _modified.Count > 0;

        /// <summary>Known map sizes (used when the size entry is missing).</summary>
        private static readonly (int Width, int Height)[] _defaultSizes =
        {
            (7168, 4096), (7168, 4096), (2304, 1600), (2560, 2048), (1448, 1448), (1280, 4096)
        };

        public EcFacet(EcUopPackage package, string fileName)
        {
            Package = package;
            FileName = fileName;
            FacetId = ParseFacetId(fileName);

            // the size entry is the only entry with 8 bytes
            EcUopEntry sizeEntry = package.Entries.FirstOrDefault(e => e.DecompressedSize == 8);
            if (sizeEntry != null)
            {
                byte[] size = package.ReadData(sizeEntry);
                Height = BitConverter.ToInt32(size, 0);
                Width = BitConverter.ToInt32(size, 4);
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

        /// <summary>Path of the radar image (facet{N:D2}.dds) in the folder, or null.</summary>
        public static string FindRadarImage(string folder, int facetId)
        {
            string path = Path.Combine(folder, $"facet{facetId:D2}.dds");
            return File.Exists(path) ? path : null;
        }
    }
}

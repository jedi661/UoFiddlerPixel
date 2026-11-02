/***************************************************************************
 *
 * $Author: Turley
 * Advanced Nikodemus
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace UoFiddler.Plugin.UopPacker.Classes
{
    /// <summary>
    /// One set of files that belong together: the UOP file and its MUL / IDX files (e.g. artLegacyMUL.uop ↔ art.mul + artidx.mul).
    /// </summary>
    public sealed class UopFileSet
    {
        public string Title { get; init; }
        public FileType Type { get; init; }
        public int TypeIndex { get; init; }
        public string Uop { get; init; }
        public string Mul { get; init; }
        public string Idx { get; init; }

        /// <summary>Extra file of the set (housing.bin of MultiCollection.uop).</summary>
        public string Extra { get; init; }

        public override string ToString() => Title;
    }

    /// <summary>
    /// Everything the converter needs to know about the file types: packed names (hash formats), id limits, default file names.
    /// </summary>
    public static class UopFileTypes
    {
        /// <summary>Packed name of the extra file of MultiCollection.uop.</summary>
        public const string HousingBinName = "build/multicollection/housing.bin";

        /// <summary>Size of one map block group in the UOP (0xC4000 bytes = 4096 blocks of 196 bytes).</summary>
        public const int MapChunkSize = 0xC4000;

        /// <summary>Size of one map block in map*.mul (4 bytes header + 64 cells of 3 bytes).</summary>
        public const int MapBlockSize = 196;

        /// <summary>
        /// Name formats of the packed files (lower case) and the highest id used for the lookup table.
        /// The second format is an alternative (gump 9834 uses 7 digits in some clients).
        /// </summary>
        public static string[] GetHashFormat(FileType type, int typeIndex, out int maxId)
        {
            maxId = 0x7FFFF;
            switch (type)
            {
                case FileType.ArtLegacyMul:
                    maxId = 0x13FDC; // UOFiddler requires this exact index length to recognize UOHS art files
                    return new[] { "build/artlegacymul/{0:00000000}.tga", string.Empty };
                case FileType.GumpartLegacyMul:
                    return new[] { "build/gumpartlegacymul/{0:00000000}.tga", "build/gumpartlegacymul/{0:0000000}.tga" };
                case FileType.MapLegacyMul:
                    maxId = 0x400; // map blocks groups (0x71 for Fel / Tram); bigger custom maps fit too
                    return new[] { string.Concat("build/map", typeIndex, "legacymul/{0:00000000}.dat"), string.Empty };
                case FileType.SoundLegacyMul:
                    return new[] { "build/soundlegacymul/{0:00000000}.dat", string.Empty };
                case FileType.MultiCollection:
                    maxId = 0x2200; // reasonable limit for multis
                    return new[] { "build/multicollection/{0:000000}.bin", string.Empty };
                default:
                    throw new ArgumentException("Unknown file type!");
            }
        }

        /// <summary>The file sets of a client folder in the order of the "Every file" list.</summary>
        public static List<UopFileSet> AllSets()
        {
            var sets = new List<UopFileSet>
            {
                new UopFileSet { Title = "Art (artLegacyMUL.uop)", Type = FileType.ArtLegacyMul, Uop = "artLegacyMUL.uop", Mul = "art.mul", Idx = "artidx.mul" },
                new UopFileSet { Title = "Gumps (gumpartLegacyMUL.uop)", Type = FileType.GumpartLegacyMul, Uop = "gumpartLegacyMUL.uop", Mul = "gumpart.mul", Idx = "gumpidx.mul" },
                new UopFileSet { Title = "Sounds (soundLegacyMUL.uop)", Type = FileType.SoundLegacyMul, Uop = "soundLegacyMUL.uop", Mul = "sound.mul", Idx = "soundidx.mul" },
                new UopFileSet { Title = "Multis (MultiCollection.uop)", Type = FileType.MultiCollection, Uop = "MultiCollection.uop", Mul = "multi.mul", Idx = "multi.idx", Extra = "housing.bin" },
            };
            for (int i = 0; i <= 5; i++)
            {
                sets.Add(new UopFileSet { Title = $"Map {i} (map{i}LegacyMUL.uop)", Type = FileType.MapLegacyMul, TypeIndex = i, Uop = $"map{i}LegacyMUL.uop", Mul = $"map{i}.mul" });
                sets.Add(new UopFileSet { Title = $"Map {i}x (map{i}xLegacyMUL.uop)", Type = FileType.MapLegacyMul, TypeIndex = i, Uop = $"map{i}xLegacyMUL.uop", Mul = $"map{i}x.mul" });
            }
            return sets;
        }

        private static readonly Regex _mapRegex = new Regex(@"^map(\d)(x?)legacymul\.uop$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex _mapMulRegex = new Regex(@"^map(\d)(x?)\.mul$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Finds the file set of a UOP or MUL / IDX file name (case does not matter), e.g. "map1xLegacyMUL.uop" → map 1, "gumpidx.mul" → gumps.
        /// </summary>
        public static UopFileSet Detect(string path)
        {
            string name = Path.GetFileName(path ?? string.Empty);
            Match m = _mapRegex.Match(name);
            if (!m.Success)
            {
                m = _mapMulRegex.Match(name);
            }
            if (m.Success)
            {
                int index = int.Parse(m.Groups[1].Value);
                string x = m.Groups[2].Value.Length > 0 ? "x" : string.Empty;
                return new UopFileSet { Title = $"Map {index}{x}", Type = FileType.MapLegacyMul, TypeIndex = index, Uop = $"map{index}{x}LegacyMUL.uop", Mul = $"map{index}{x}.mul" };
            }
            foreach (UopFileSet set in AllSets())
            {
                if (string.Equals(name, set.Uop, StringComparison.OrdinalIgnoreCase) || string.Equals(name, set.Mul, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, set.Idx, StringComparison.OrdinalIgnoreCase))
                {
                    return set;
                }
            }
            // names written by older versions of this tab
            if (name.Equals("multi-unpacked.mul", StringComparison.OrdinalIgnoreCase) || name.Equals("multi-unpacked.idx", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("multiidx.mul", StringComparison.OrdinalIgnoreCase))
            {
                return AllSets().Find(s => s.Type == FileType.MultiCollection);
            }
            return null;
        }

        /// <summary>Known map sizes in bytes (classic maps); 0 = unknown.</summary>
        public static long ExpectedMapFileSize(int typeIndex)
        {
            return typeIndex switch
            {
                0 => 89_915_392,
                1 => 89_915_392,
                2 => 11_289_600,
                3 => 16_056_320,
                4 => 6_421_156,
                5 => 16_056_320,
                _ => 0
            };
        }
    }
}

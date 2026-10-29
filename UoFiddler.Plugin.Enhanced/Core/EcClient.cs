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
using System.Text.RegularExpressions;
using UoFiddler.Plugin.Enhanced.Core.StringDictionary;
using UoFiddler.Plugin.Enhanced.Core.TileArt;
using UoFiddler.Plugin.Enhanced.Core.Uop;

namespace UoFiddler.Plugin.Enhanced.Core
{
    /// <summary>
    /// A kind of image stored in the Enhanced Client files.
    /// </summary>
    public sealed class EcArtSource
    {
        public string Title { get; }

        /// <summary>Packed file name pattern, e.g. "build/worldart/{0:D8}.dds".</summary>
        public string Pattern { get; }

        /// <summary>UOP files that may contain this kind of image (first existing file with hits wins).</summary>
        public string[] FileNames { get; }

        /// <summary>Folder name used in string_dictionary.uop (e.g. "WorldArt"), used to find ids above the default range.</summary>
        public string DictionaryFolder { get; }

        public int DefaultLastId { get; }

        public EcArtSource(string title, string pattern, string dictionaryFolder, int defaultLastId, params string[] fileNames)
        {
            Title = title;
            Pattern = pattern;
            DictionaryFolder = dictionaryFolder;
            DefaultLastId = defaultLastId;
            FileNames = fileNames;
        }

        public string GetName(int id) => string.Format(CultureInfo.InvariantCulture, Pattern, id);

        public override string ToString() => Title;
    }

    /// <summary>
    /// Known packed file name patterns per UOP file (used by the UOP browser to show names instead of hashes).
    /// </summary>
    public sealed class EcNamePattern
    {
        public string FileName { get; }
        public string Pattern { get; }
        public int FirstId { get; }
        public int LastId { get; }

        public EcNamePattern(string fileName, string pattern, int firstId, int lastId)
        {
            FileName = fileName;
            Pattern = pattern;
            FirstId = firstId;
            LastId = lastId;
        }
    }

    /// <summary>
    /// The Enhanced Client folder: opens the UOP files on demand and shares them between the editors.
    /// </summary>
    public sealed class EcClient : IDisposable
    {
        public const string TileArtFileName = "tileart.uop";
        public const string StringDictionaryFileName = "string_dictionary.uop";
        public const string TextureFileName = "Texture.uop";
        public const string LegacyTextureFileName = "LegacyTexture.uop";

        public static readonly EcArtSource[] ArtSources =
        {
            new EcArtSource("World Art (EC items)", "build/worldart/{0:D8}.dds", "WorldArt", 0x20000, TextureFileName, LegacyTextureFileName),
            new EcArtSource("Tile Art Legacy (CC art for EC)", "build/tileartlegacy/{0:D8}.dds", "TileArtLegacy", 0x20000, LegacyTextureFileName, TextureFileName),
            new EcArtSource("Tile Art Enhanced", "build/tileartenhanced/{0:D8}.dds", "TileArtEnhanced", 0x20000, TextureFileName, LegacyTextureFileName),
        };

        public static readonly EcNamePattern[] KnownPatterns =
        {
            new EcNamePattern(TileArtFileName, TileArt.TileArtFile.NamePattern, 0, 0x20000),
            new EcNamePattern(TextureFileName, "build/worldart/{0:D8}.dds", 0, 0x20000),
            new EcNamePattern(TextureFileName, "build/tileartenhanced/{0:D8}.dds", 0, 0x20000),
            new EcNamePattern(TextureFileName, "build/tileartlegacy/{0:D8}.dds", 0, 0x20000),
            new EcNamePattern(LegacyTextureFileName, "build/tileartlegacy/{0:D8}.dds", 0, 0x20000),
            new EcNamePattern(LegacyTextureFileName, "build/tileartenhanced/{0:D8}.dds", 0, 0x20000),
            new EcNamePattern("artLegacyMUL.uop", "build/artlegacymul/{0:D8}.tga", 0, 0x20000),
            new EcNamePattern("gumpartLegacyMUL.uop", "build/gumpartlegacymul/{0:D8}.tga", 0, 0x10000),
            new EcNamePattern("soundLegacyMUL.uop", "build/soundlegacymul/{0:D8}.dat", 0, 0x10000),
            new EcNamePattern("MultiCollection.uop", "build/multicollection/{0:D6}.bin", 0, 0x10000),
        };

        private readonly Dictionary<string, EcUopPackage> _packages = new Dictionary<string, EcUopPackage>(StringComparer.OrdinalIgnoreCase);
        private EcStringDictionary _stringDictionary;
        private TileArtFile _tileArt;

        public string Folder { get; }

        public EcClient(string folder)
        {
            if (!Directory.Exists(folder))
            {
                throw new DirectoryNotFoundException(folder);
            }
            Folder = folder;
        }

        public IEnumerable<string> GetUopFiles()
        {
            return Directory.EnumerateFiles(Folder, "*.uop").OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Full path of a file in the client folder (case insensitive), or null.
        /// </summary>
        public string FindFile(string fileName)
        {
            string direct = Path.Combine(Folder, fileName);
            if (File.Exists(direct))
            {
                return direct;
            }

            return Directory.EnumerateFiles(Folder).FirstOrDefault(f => string.Equals(Path.GetFileName(f), fileName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Opens (once) and returns the package, or null if the file doesn't exist.
        /// </summary>
        public EcUopPackage GetPackage(string fileName)
        {
            if (_packages.TryGetValue(fileName, out EcUopPackage package))
            {
                return package;
            }

            string path = FindFile(fileName);
            if (path == null)
            {
                return null;
            }

            package = EcUopPackage.Open(path);
            _packages[fileName] = package;
            ResolveKnownNames(package, Path.GetFileName(path));
            return package;
        }

        public bool IsOpen(string fileName) => _packages.ContainsKey(fileName);

        public IEnumerable<EcUopPackage> OpenPackages => _packages.Values;

        /// <summary>
        /// Gives the known packed file names to the entries (only for patterns registered for this file name).
        /// </summary>
        public void ResolveKnownNames(EcUopPackage package, string fileName)
        {
            foreach (EcNamePattern pattern in KnownPatterns.Where(p => string.Equals(p.FileName, fileName, StringComparison.OrdinalIgnoreCase)))
            {
                package.ResolveNames(pattern.Pattern, pattern.FirstId, pattern.LastId);
            }
        }

        public EcStringDictionary StringDictionary
        {
            get
            {
                if (_stringDictionary == null)
                {
                    EcUopPackage package = GetPackage(StringDictionaryFileName);
                    if (package != null)
                    {
                        _stringDictionary = EcStringDictionary.Load(package);
                    }
                }
                return _stringDictionary;
            }
        }

        public bool HasStringDictionary => FindFile(StringDictionaryFileName) != null;

        public TileArtFile GetTileArt(IProgress<int> progress = null)
        {
            if (_tileArt == null)
            {
                EcUopPackage package = GetPackage(TileArtFileName);
                if (package != null)
                {
                    _tileArt = TileArtFile.Load(package, progress);
                }
            }
            return _tileArt;
        }

        public TileArtFile TileArtIfLoaded => _tileArt;

        /// <summary>
        /// Ids mentioned in string_dictionary.uop for a folder like "WorldArt" ("Data\WorldArt\01000045_noise.tga" -> 1000045).
        /// </summary>
        public IEnumerable<int> GetDictionaryIds(string folder)
        {
            EcStringDictionary dict = HasStringDictionary ? StringDictionary : null;
            if (dict == null || string.IsNullOrEmpty(folder))
            {
                yield break;
            }

            string prefix = "Data\\" + folder + "\\";
            var regex = new Regex(@"^(\d+)", RegexOptions.CultureInvariant);
            var seen = new HashSet<int>();
            foreach (string s in dict.Strings)
            {
                if (!s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Match m = regex.Match(s.Substring(prefix.Length));
                if (m.Success && int.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) && seen.Add(id))
                {
                    yield return id;
                }
            }
        }

        /// <summary>
        /// Id from an image path of the string dictionary, e.g. "Data\WorldArt\00000002_ankh.tga" -> (WorldArt source, 2).
        /// </summary>
        public static bool TryParseTexturePath(string path, out EcArtSource source, out int id)
        {
            source = null;
            id = -1;
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            foreach (EcArtSource s in ArtSources)
            {
                string prefix = "Data\\" + s.DictionaryFolder + "\\";
                if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Match m = Regex.Match(path.Substring(prefix.Length), @"^(\d+)");
                if (m.Success && int.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
                {
                    source = s;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Id part of a packed file name ("build/worldart/00000002.dds" -> 2).
        /// </summary>
        public static bool TryGetIdFromName(string name, out int id)
        {
            id = -1;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            return int.TryParse(Path.GetFileNameWithoutExtension(name), NumberStyles.Integer, CultureInfo.InvariantCulture, out id);
        }

        /// <summary>
        /// Writes pending tileart / string dictionary changes into their packages.
        /// </summary>
        public void ApplyPendingChanges()
        {
            _tileArt?.Apply();
            _stringDictionary?.Apply();
        }

        public bool HasUnsavedChanges =>
            _packages.Values.Any(p => p.IsDirty) || (_tileArt?.IsModified ?? false) || (_stringDictionary?.IsModified ?? false);

        public void Dispose()
        {
            foreach (EcUopPackage package in _packages.Values)
            {
                package.Dispose();
            }
            _packages.Clear();
        }
    }
}

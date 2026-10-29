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
using System.Text;
using System.Text.RegularExpressions;
using UoFiddler.Plugin.Enhanced.EcUltima.Animations;
using UoFiddler.Plugin.Enhanced.EcUltima.Strings;
using UoFiddler.Plugin.Enhanced.EcUltima.TileArt;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.EcUltima
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

        /// <summary>Folder part of the packed names, e.g. "build/worldart/".</summary>
        public string Prefix { get; }

        /// <summary>
        /// True for images that are stored by file name instead of a numbered pattern (effect textures).
        /// </summary>
        public bool IsNameBased => Pattern == null;

        public EcArtSource(string title, string pattern, string dictionaryFolder, int defaultLastId, params string[] fileNames)
        {
            Title = title;
            Pattern = pattern;
            DictionaryFolder = dictionaryFolder;
            DefaultLastId = defaultLastId;
            FileNames = fileNames;
            Prefix = pattern.Substring(0, pattern.LastIndexOf('/') + 1);
        }

        /// <summary>
        /// Source for images stored by name below <paramref name="prefix"/> (e.g. "data/effects/").
        /// </summary>
        public EcArtSource(string title, string prefix, params string[] fileNames)
        {
            Title = title;
            Prefix = prefix;
            FileNames = fileNames;
        }

        public string GetName(int id) => string.Format(CultureInfo.InvariantCulture, Pattern, id);

        /// <summary>
        /// True when the packed name belongs to this source and is an image.
        /// </summary>
        public bool Matches(string name)
        {
            if (name == null || !name.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (!IsNameBased)
            {
                return true;
            }
            string ext = Path.GetExtension(name).ToLowerInvariant();
            return ext == ".tga" || ext == ".dds" || ext == ".png" || ext == ".bmp";
        }

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

        /// <summary>Last value of the second placeholder {1} (0 = pattern has only {0}).</summary>
        public int LastSecond { get; }

        public EcNamePattern(string fileName, string pattern, int firstId, int lastId, int lastSecond = 0)
        {
            LastSecond = lastSecond;
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
        public const string AnimationDefinitionFileName = "AnimationDefinition.uop";
        public const string EffectTextureFileName = "EffectTexture.uop";

        public static readonly EcArtSource[] ArtSources =
        {
            new EcArtSource("World Art (EC items)", "build/worldart/{0:D8}.dds", "WorldArt", 0x20000, TextureFileName, LegacyTextureFileName),
            new EcArtSource("Tile Art Legacy (CC art for EC)", "build/tileartlegacy/{0:D8}.dds", "TileArtLegacy", 0x20000, LegacyTextureFileName, TextureFileName),
            new EcArtSource("Tile Art Enhanced", "build/tileartenhanced/{0:D8}.dds", "TileArtEnhanced", 0x20000, TextureFileName, LegacyTextureFileName),
            new EcArtSource("Effect textures", "data/effects/", EffectTextureFileName),
            new EcArtSource("Terrain textures (EC land)", "build/terraintexture/{0:D8}.dds", null, 0x1000, "TerrainTexture.uop"),
        };

        public static readonly EcNamePattern[] KnownPatterns =
        {
            new EcNamePattern(TileArtFileName, TileArtFile.NamePattern, 0, 0x20000),
            new EcNamePattern(TextureFileName, "build/worldart/{0:D8}.dds", 0, 0x20000),
            new EcNamePattern(TextureFileName, "build/tileartenhanced/{0:D8}.dds", 0, 0x20000),
            new EcNamePattern(TextureFileName, "build/tileartlegacy/{0:D8}.dds", 0, 0x20000),
            new EcNamePattern(LegacyTextureFileName, "build/tileartlegacy/{0:D8}.dds", 0, 0x20000),
            new EcNamePattern(LegacyTextureFileName, "build/tileartenhanced/{0:D8}.dds", 0, 0x20000),
            new EcNamePattern("artLegacyMUL.uop", "build/artlegacymul/{0:D8}.tga", 0, 0x20000),
            new EcNamePattern("gumpartLegacyMUL.uop", "build/gumpartlegacymul/{0:D8}.tga", 0, 0x10000),
            new EcNamePattern("soundLegacyMUL.uop", "build/soundlegacymul/{0:D8}.dat", 0, 0x10000),
            new EcNamePattern("MultiCollection.uop", "build/multicollection/{0:D6}.bin", 0, 0x10000),
            new EcNamePattern(AnimationDefinitionFileName, EcAnimDefinitionFile.NamePattern, 0, 0x40000),
            new EcNamePattern("AnimationSequence.uop", "build/animationsequence/{0:D8}.bin", 0, 0x10000),
            new EcNamePattern("EffectDefinitionCollection.uop", "build/effectdefinitioncollection/{0:D8}.bin", 0, 0x40000),
            new EcNamePattern("facets.uop", "build/facetdefinition/{0:D8}.bin", 0, 0x100),
            new EcNamePattern("TerrainTexture.uop", "build/terraintexture/{0:D8}.dds", 0, 0x10000),
            new EcNamePattern("TerrainDefinition.uop", "build/terraindefinition/{0}.bin", 0, 0x10000),
            new EcNamePattern("AnimationFrame1.uop", AnimationFramePattern, 0, 4095, 99),
            new EcNamePattern("AnimationFrame2.uop", AnimationFramePattern, 0, 4095, 99),
            new EcNamePattern("AnimationFrame3.uop", AnimationFramePattern, 0, 4095, 99),
            new EcNamePattern("AnimationFrame4.uop", AnimationFramePattern, 0, 4095, 99),
            new EcNamePattern("AnimationFrame5.uop", AnimationFramePattern, 0, 4095, 99),
            new EcNamePattern("AnimationFrame6.uop", AnimationFramePattern, 0, 4095, 99),
            new EcNamePattern("Paperdoll.uop", "build/paperdoll/{0:D6}/{1:D2}.bin", 0, 999, 99),
            new EcNamePattern("facet0.uop", "build/sectors/facet_00/{0:D8}.bin", 0, 0x2000),
            new EcNamePattern("facet0x.uop", "build/sectors/facet_00/{0:D8}.bin", 0, 0x2000),
            new EcNamePattern("facet1.uop", "build/sectors/facet_01/{0:D8}.bin", 0, 0x2000),
            new EcNamePattern("facet1x.uop", "build/sectors/facet_01/{0:D8}.bin", 0, 0x2000),
            new EcNamePattern("facet2.uop", "build/sectors/facet_02/{0:D8}.bin", 0, 0x2000),
            new EcNamePattern("facet2x.uop", "build/sectors/facet_02/{0:D8}.bin", 0, 0x2000),
            new EcNamePattern("facet3.uop", "build/sectors/facet_03/{0:D8}.bin", 0, 0x2000),
            new EcNamePattern("facet3x.uop", "build/sectors/facet_03/{0:D8}.bin", 0, 0x2000),
            new EcNamePattern("facet4.uop", "build/sectors/facet_04/{0:D8}.bin", 0, 0x2000),
            new EcNamePattern("facet4x.uop", "build/sectors/facet_04/{0:D8}.bin", 0, 0x2000),
            new EcNamePattern("facet5.uop", "build/sectors/facet_05/{0:D8}.bin", 0, 0x2000),
            new EcNamePattern("facet5x.uop", "build/sectors/facet_05/{0:D8}.bin", 0, 0x2000),
            new EcNamePattern("Interface.uop", "data/interface/default/textures/gumpart/{0:D8}.tga", 0, 0x20000),
            new EcNamePattern("Hues.uop", "data/definitions/hues/hue{0:D4}.bmp", 0, 9999),
            new EcNamePattern("Hues.uop", "build/hues/hues.dds", 0, 0),
            new EcNamePattern("LocalizedStrings.uop", "data/localizedstrings/{0:D3}.cliloc", 0, 999),
            new EcNamePattern(StringDictionaryFileName, "build/stringdictionary/string_dictionary.bin", 0, 0),
            new EcNamePattern("waypoint.uop", "build/sectors/waypoint.bin", 0, 0),
            new EcNamePattern("MainMisc.uop", "data/wearables.xml", 0, 0),
            new EcNamePattern("MainMisc.uop", "data/assetmap.xml", 0, 0),
            new EcNamePattern("MainMisc.uop", "data/speech.csv", 0, 0),
            new EcNamePattern("MainMisc.uop", "data/precachelist.dat", 0, 0),
            new EcNamePattern("GameData.uop", "data/speech.csv", 0, 0),
            new EcNamePattern("GameData.uop", "data/precachelist.dat", 0, 0),
            new EcNamePattern("MultiCollection.uop", "build/multicollection/housing.bin", 0, 0),
            new EcNamePattern("SystemTextures.uop", "data/systemtextures/missing.tga", 0, 0),
            new EcNamePattern("Audio.uop", "data/audio/audio_sounds.csv", 0, 0),
            new EcNamePattern("Audio.uop", "data/audio/audio_music.csv", 0, 0),
        };

        /// <summary>
        /// Optional list of known names (e.g. the Dictionary.dic of the Mythic Package Editor), used for every file.
        /// </summary>
        public static EcNameDictionary NameDictionary { get; set; }

        /// <summary>Animation frames: body id, action.</summary>
        public const string AnimationFramePattern = "build/animationframe/{0:D6}/{1:D2}.bin";

        private readonly Dictionary<string, EcUopPackage> _packages = new Dictionary<string, EcUopPackage>(StringComparer.OrdinalIgnoreCase);
        private EcStringDictionary _stringDictionary;
        private TileArtFile _tileArt;
        private EcAnimDefinitionFile _animDefinitions;

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
                if (pattern.LastSecond > 0)
                {
                    package.ResolveNames2D(pattern.Pattern, pattern.FirstId, pattern.LastId, pattern.LastSecond);
                }
                else
                {
                    package.ResolveNames(pattern.Pattern, pattern.FirstId, pattern.LastId);
                }
            }

            if (string.Equals(fileName, EffectTextureFileName, StringComparison.OrdinalIgnoreCase))
            {
                ResolveEffectNames(package);
            }

            NameDictionary?.Apply(package);

            if (EcContentNameResolver.Supports(fileName))
            {
                EcContentNameResolver.Resolve(package, fileName);
            }
        }

        /// <summary>
        /// EffectTexture.uop stores "data/effects/{file name}" (nif models, ems particle scripts, tga/dds textures).
        /// The names come from string_dictionary.uop and from the texture names inside the particle scripts.
        /// </summary>
        public void ResolveEffectNames(EcUopPackage package)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var fileRegex = new Regex(@"[\w\-\. ]{3,80}\.(?:tga|dds|nif|ems|kf|psd)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (HasStringDictionary)
            {
                foreach (string s in StringDictionary.Strings)
                {
                    if (fileRegex.IsMatch(s) && s.IndexOf("WorldArt", StringComparison.OrdinalIgnoreCase) < 0 && s.IndexOf("TileArt", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        names.Add(s.Replace('\\', '/').Split('/').Last());
                    }
                }
            }

            foreach (EcUopEntry entry in package.Entries)
            {
                if (entry.DecompressedSize > 200_000)
                {
                    continue;
                }
                try
                {
                    byte[] data = package.ReadData(entry);
                    if (data.Length > 14 && (Encoding.ASCII.GetString(data, 0, 14).StartsWith("particlesystem", StringComparison.Ordinal) ||
                                             Encoding.ASCII.GetString(data, 0, 8) == "Gamebryo"))
                    {
                        foreach (Match m in fileRegex.Matches(Encoding.Latin1.GetString(data)))
                        {
                            names.Add(m.Value.Trim());
                        }
                    }
                }
                catch (Exception)
                {
                    // unknown compression – skip
                }
            }

            foreach (string name in names)
            {
                int dot = name.LastIndexOf('.');
                string baseName = dot > 0 ? name.Substring(0, dot) : name;
                foreach (string candidate in new[] { name, baseName + ".tga", baseName + ".dds", baseName + ".nif", baseName + ".ems", baseName + ".kf", baseName + ".psd" })
                {
                    package.ResolveName("data/effects/" + candidate);
                }
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

        public EcAnimDefinitionFile GetAnimDefinitions()
        {
            if (_animDefinitions == null)
            {
                EcUopPackage package = GetPackage(AnimationDefinitionFileName);
                if (package != null)
                {
                    _animDefinitions = EcAnimDefinitionFile.Load(package);
                }
            }
            return _animDefinitions;
        }

        /// <summary>
        /// Finds the image data (DDS) of an item: first through the texture references of tileart
        /// (world art, then legacy), then by the same id in the world art / legacy files.
        /// </summary>
        public byte[] GetItemImage(uint tileId, out string description)
        {
            description = null;
            var candidates = new List<(EcArtSource Source, int Id)>();

            TileArtFile tileArt = TileArtIfLoaded ?? (FindFile(TileArtFileName) != null ? GetTileArt() : null);
            TileArtRecord record = tileArt?.Get(tileId);
            if (record != null && HasStringDictionary)
            {
                foreach (TileArtTexture texture in record.Entry.Textures)
                {
                    foreach (TileArtTextureImage image in texture.Images)
                    {
                        if (TryParseTexturePath(StringDictionary.Get(image.NameIndex), out EcArtSource source, out int id) && id != 1000045)
                        {
                            candidates.Add((source, id));
                        }
                    }
                }
            }
            candidates.Add((ArtSources[0], (int)tileId));
            candidates.Add((ArtSources[1], (int)tileId));

            foreach (var (source, id) in candidates)
            {
                foreach (string fileName in source.FileNames)
                {
                    try
                    {
                        EcUopPackage package = GetPackage(fileName);
                        EcUopEntry entry = package?.GetEntry(source.GetName(id));
                        if (entry != null)
                        {
                            description = $"{fileName}: {entry.Name}";
                            return package.ReadData(entry);
                        }
                    }
                    catch (Exception)
                    {
                        // try the next one
                    }
                }
            }
            return null;
        }

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

            foreach (EcArtSource s in ArtSources.Where(a => a.DictionaryFolder != null))
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
            _animDefinitions?.Apply();
            _stringDictionary?.Apply();
        }

        public bool HasUnsavedChanges =>
            _packages.Values.Any(p => p.IsDirty) || (_tileArt?.IsModified ?? false) || (_animDefinitions?.IsModified ?? false) || (_stringDictionary?.IsModified ?? false);

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

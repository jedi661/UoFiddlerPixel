using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Ultima
{
    /// <summary>
    /// Map file found by <see cref="MapRegistry.DetectMapFiles"/>.
    /// </summary>
    public sealed class DetectedMapFile
    {
        public int FileIndex { get; set; }
        public string FilePath { get; set; }
        public bool IsUop { get; set; }
        public long BlockCount { get; set; }
        public int SuggestedWidth { get; set; }
        public int SuggestedHeight { get; set; }
        public bool HasStatics { get; set; }

        public override string ToString()
        {
            return $"{Path.GetFileName(FilePath)} ({SuggestedWidth}x{SuggestedHeight})";
        }
    }

    /// <summary>
    /// List of all maps known to UoFiddler. Contains the 9 built-in maps and any number of user defined maps.
    /// </summary>
    public static class MapRegistry
    {
        private static readonly List<MapDefinition> _definitions = new List<MapDefinition>();

        /// <summary>
        /// Default names of the built-in maps
        /// </summary>
        public static readonly string[] BuiltInNames =
        {
            "Felucca", "Trammel", "Ilshenar", "Malas", "Tokuno", "Ter Mur", "Forell", "Dragon", "Intermediate World"
        };

        /// <summary>
        /// Raised after maps were added, removed or changed.
        /// </summary>
        public static event EventHandler Changed;

        /// <summary>
        /// All maps ordered by id
        /// </summary>
        public static IReadOnlyList<MapDefinition> Definitions
        {
            get
            {
                EnsureBuiltIns(null);
                return _definitions;
            }
        }

        /// <summary>
        /// Creates the built-in definitions if missing. Names are taken from <paramref name="names"/> when given.
        /// </summary>
        public static void EnsureBuiltIns(string[] names)
        {
            bool added = false;
            for (int id = 0; id < Map.BuiltInCount; id++)
            {
                MapDefinition def = _definitions.FirstOrDefault(d => d.Id == id);
                string name = names != null && id < names.Length && !string.IsNullOrWhiteSpace(names[id])
                    ? names[id]
                    : BuiltInNames[id];

                if (def == null)
                {
                    _definitions.Add(new MapDefinition
                    {
                        Id = id,
                        Name = name,
                        FileIndex = Map.GetBuiltIn(id)?.FileIndex ?? id,
                        IsBuiltIn = true
                    });
                    added = true;
                }
                else
                {
                    def.IsBuiltIn = true;
                    if (names != null)
                    {
                        def.Name = name;
                    }
                }
            }

            if (added)
            {
                Sort();
            }
        }

        public static MapDefinition GetById(int id)
        {
            return Definitions.FirstOrDefault(d => d.Id == id);
        }

        public static string GetName(int id)
        {
            return GetById(id)?.Name ?? $"Map {id}";
        }

        public static int NextFreeId()
        {
            int id = Map.BuiltInCount;
            while (_definitions.Any(d => d.Id == id))
            {
                id++;
            }

            return id;
        }

        public static void Add(MapDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (_definitions.Any(d => d.Id == definition.Id))
            {
                throw new ArgumentException($"A map with id {definition.Id} already exists.");
            }

            definition.IsBuiltIn = false;
            _definitions.Add(definition);
            Sort();
            OnChanged();
        }

        /// <summary>
        /// Removes a user map. Built-in maps can not be removed (hide them instead).
        /// </summary>
        public static bool Remove(MapDefinition definition)
        {
            if (definition == null || definition.IsBuiltIn)
            {
                return false;
            }

            definition.Reset();
            bool removed = _definitions.Remove(definition);
            if (removed)
            {
                OnChanged();
            }

            return removed;
        }

        /// <summary>
        /// Call after a definition was edited (closes the old files, raises <see cref="Changed"/>).
        /// </summary>
        public static void Update(MapDefinition definition)
        {
            definition?.Reset();
            Sort();
            OnChanged();
        }

        /// <summary>
        /// Resets the map instances of all user maps (called by <see cref="Map.Reload"/>).
        /// </summary>
        public static void ResetCustomMaps()
        {
            foreach (MapDefinition def in _definitions.Where(d => !d.IsBuiltIn).Concat(_temporary))
            {
                def.ResetFiles();
            }
        }

        /// <summary>
        /// Map instances of user maps that were already created
        /// </summary>
        public static IEnumerable<Map> LoadedCustomMaps()
        {
            foreach (MapDefinition def in _definitions.Where(d => !d.IsBuiltIn).Concat(_temporary).ToList())
            {
                Map map = def.LoadedMap;
                if (map != null)
                {
                    yield return map;
                }
            }
        }

        #region Temporary (external) maps

        private static readonly List<MapDefinition> _temporary = new List<MapDefinition>();
        private static int _nextTemporaryId = -1000;

        /// <summary>
        /// External maps opened in the map editor. They are not saved and not shown in the map list,
        /// but their files are closed/reloaded like the other maps when files are replaced.
        /// </summary>
        public static IReadOnlyList<MapDefinition> TemporaryDefinitions => _temporary;

        /// <summary>
        /// Opens a map of another directory for this session only (negative id)
        /// </summary>
        public static MapDefinition AddTemporary(string directory, int fileIndex, int width, int height, string name)
        {
            MapDefinition existing = _temporary.FirstOrDefault(d => d.FileIndex == fileIndex &&
                string.Equals(System.IO.Path.GetFullPath(d.CustomPath ?? string.Empty).TrimEnd('\\', '/'),
                    System.IO.Path.GetFullPath(directory).TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                if (existing.Width != width || existing.Height != height)
                {
                    existing.Width = width;
                    existing.Height = height;
                    existing.Reset();
                }

                return existing;
            }

            var def = new MapDefinition
            {
                Id = _nextTemporaryId--,
                Name = name,
                FileIndex = fileIndex,
                Width = width,
                Height = height,
                CustomPath = directory,
                Visible = false,
                IsBuiltIn = false
            };
            _temporary.Add(def);
            return def;
        }

        public static void RemoveTemporary(MapDefinition def)
        {
            if (def != null && _temporary.Remove(def))
            {
                def.Reset();
            }
        }

        /// <summary>
        /// Makes a temporary map a normal (saved) entry of the map list
        /// </summary>
        public static MapDefinition MakePermanent(MapDefinition def)
        {
            if (def == null || !_temporary.Contains(def))
            {
                return def;
            }

            _temporary.Remove(def);
            def.Id = NextFreeId();
            def.Visible = true;
            Add(def);
            return def;
        }

        public static bool IsTemporary(MapDefinition def) => def != null && _temporary.Contains(def);

        #endregion

        public static void OnChanged()
        {
            Changed?.Invoke(null, EventArgs.Empty);
        }

        private static void Sort()
        {
            _definitions.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        #region Load / Save

        /// <summary>
        /// Loads the definitions from xml. Built-in maps are always present.
        /// </summary>
        public static void Load(string fileName, string[] defaultNames)
        {
            foreach (MapDefinition def in _definitions)
            {
                def.Reset();
            }

            _definitions.Clear();
            EnsureBuiltIns(defaultNames);

            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return;
            }

            var dom = new XmlDocument();
            dom.Load(fileName);

            XmlElement root = dom["MapDefinitions"];
            if (root == null)
            {
                return;
            }

            foreach (XmlElement element in root.SelectNodes("Map"))
            {
                if (!TryGetInt(element, "id", out int id))
                {
                    continue;
                }

                string name = element.GetAttribute("name");
                bool visible = !bool.TryParse(element.GetAttribute("visible"), out bool v) || v;

                MapDefinition def = _definitions.FirstOrDefault(d => d.Id == id);
                if (def != null && def.IsBuiltIn)
                {
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        def.Name = name;
                    }

                    def.Visible = visible;
                    continue;
                }

                if (def != null)
                {
                    continue; // duplicate id
                }

                TryGetInt(element, "file", out int fileIndex);
                TryGetInt(element, "width", out int width);
                TryGetInt(element, "height", out int height);

                if (width <= 0 || height <= 0)
                {
                    continue;
                }

                _definitions.Add(new MapDefinition
                {
                    Id = id,
                    Name = string.IsNullOrWhiteSpace(name) ? $"Map {id}" : name,
                    FileIndex = fileIndex,
                    Width = width,
                    Height = height,
                    CustomPath = element.GetAttribute("path"),
                    Visible = visible,
                    IsBuiltIn = false
                });
            }

            Sort();
        }

        public static void Save(string fileName)
        {
            var dom = new XmlDocument();
            dom.AppendChild(dom.CreateXmlDeclaration("1.0", "utf-8", null));
            XmlElement root = dom.CreateElement("MapDefinitions");
            dom.AppendChild(root);

            foreach (MapDefinition def in Definitions)
            {
                XmlElement elem = dom.CreateElement("Map");
                elem.SetAttribute("id", def.Id.ToString(CultureInfo.InvariantCulture));
                elem.SetAttribute("name", def.Name ?? string.Empty);
                elem.SetAttribute("visible", def.Visible.ToString());
                elem.SetAttribute("builtin", def.IsBuiltIn.ToString());

                if (!def.IsBuiltIn)
                {
                    elem.SetAttribute("file", def.FileIndex.ToString(CultureInfo.InvariantCulture));
                    elem.SetAttribute("width", def.Width.ToString(CultureInfo.InvariantCulture));
                    elem.SetAttribute("height", def.Height.ToString(CultureInfo.InvariantCulture));
                    elem.SetAttribute("path", def.CustomPath ?? string.Empty);
                }

                root.AppendChild(elem);
            }

            string dir = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            dom.Save(fileName);
        }

        private static bool TryGetInt(XmlElement element, string attribute, out int value)
        {
            return int.TryParse(element.GetAttribute(attribute), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        #endregion

        #region Detection

        private static readonly Regex _mapFileRegex = new Regex(@"^map(\d+)(legacymul\.uop|\.mul)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Searches a directory for map files (mapX.mul / mapXLegacyMUL.uop) and suggests the map size.
        /// </summary>
        public static List<DetectedMapFile> DetectMapFiles(string directory)
        {
            var result = new List<DetectedMapFile>();
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                return result;
            }

            foreach (string file in Directory.GetFiles(directory))
            {
                Match match = _mapFileRegex.Match(Path.GetFileName(file));
                if (!match.Success)
                {
                    continue;
                }

                int index = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                bool isUop = file.EndsWith(".uop", StringComparison.OrdinalIgnoreCase);

                // prefer mul if both exist
                DetectedMapFile existing = result.FirstOrDefault(r => r.FileIndex == index);
                if (existing != null)
                {
                    if (existing.IsUop && !isUop)
                    {
                        result.Remove(existing);
                    }
                    else
                    {
                        continue;
                    }
                }

                long blocks;
                try
                {
                    blocks = isUop ? GetUopBlockCount(file) : new FileInfo(file).Length / 196;
                }
                catch (Exception)
                {
                    blocks = 0;
                }

                GuessSize(index, blocks, out int width, out int height);

                result.Add(new DetectedMapFile
                {
                    FileIndex = index,
                    FilePath = file,
                    IsUop = isUop,
                    BlockCount = blocks,
                    SuggestedWidth = width,
                    SuggestedHeight = height,
                    HasStatics = File.Exists(Path.Combine(directory, $"statics{index}.mul")) &&
                                 File.Exists(Path.Combine(directory, $"staidx{index}.mul"))
                });
            }

            result.Sort((a, b) => a.FileIndex.CompareTo(b.FileIndex));
            return result;
        }

        /// <summary>
        /// Suggests width/height for a map with the given number of 8x8 blocks.
        /// </summary>
        public static void GuessSize(int fileIndex, long blocks, out int width, out int height)
        {
            // known sizes first
            (int w, int h)[] known = fileIndex switch
            {
                0 or 1 => new[] { (7168, 4096), (6144, 4096) },
                2 => new[] { (2304, 1600) },
                3 => new[] { (2560, 2048) },
                4 => new[] { (1448, 1448) },
                5 => new[] { (1280, 4096) },
                _ => Array.Empty<(int, int)>()
            };

            foreach ((int w, int h) in known)
            {
                if ((long)(w >> 3) * (h >> 3) == blocks)
                {
                    width = w;
                    height = h;
                    return;
                }
            }

            int[] heights = { 4096, 2048, 1600, 1448, 1024, 512, 256 };
            foreach (int h in heights)
            {
                int bh = h >> 3;
                if (blocks > 0 && blocks % bh == 0)
                {
                    width = (int)(blocks / bh) << 3;
                    height = h;
                    return;
                }
            }

            // fallback: assume height 4096 and round the width down
            height = 4096;
            width = Math.Max(8, (int)(blocks / (height >> 3)) << 3);
        }

        private static long GetUopBlockCount(string file)
        {
            using (var reader = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                if (reader.ReadInt32() != 0x50594D)
                {
                    return 0;
                }

                reader.ReadInt64(); // version + signature
                long nextBlock = reader.ReadInt64();
                reader.ReadInt32(); // block capacity
                reader.ReadInt32(); // count

                long total = 0;
                while (nextBlock != 0 && nextBlock < reader.BaseStream.Length)
                {
                    reader.BaseStream.Seek(nextBlock, SeekOrigin.Begin);
                    int filesCount = reader.ReadInt32();
                    nextBlock = reader.ReadInt64();

                    for (int i = 0; i < filesCount; i++)
                    {
                        long offset = reader.ReadInt64();
                        reader.ReadInt32(); // header length
                        reader.ReadInt32(); // compressed length
                        int decompressedLength = reader.ReadInt32();
                        reader.ReadUInt64(); // hash
                        reader.ReadUInt32(); // adler32
                        reader.ReadInt16(); // flag

                        if (offset != 0)
                        {
                            total += decompressedLength;
                        }
                    }
                }

                return total / 196;
            }
        }

        #endregion
    }
}

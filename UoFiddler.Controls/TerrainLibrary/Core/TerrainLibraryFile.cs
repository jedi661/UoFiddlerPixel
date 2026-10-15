// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.IO;
using UoFiddler.Controls.ArtLibrary.Core;
using UoFiddler.Controls.TerrainLibrary.Metadata;

namespace UoFiddler.Controls.TerrainLibrary.Core
{
    /// <summary>Raised when a slot of a layer changed; Id = -1 means "many / all".</summary>
    public sealed class TerrainSlotChangedEventArgs : EventArgs
    {
        public TerrainSlotChangedEventArgs(TerrainLayer layer, int id)
        {
            Layer = layer;
            Id = id;
        }

        public TerrainLayer Layer { get; }

        public int Id { get; }

        public bool IsAll => Id < 0;
    }

    /// <summary>
    /// A terrain library: land tiles and textures in separate files but with the same slot numbers,
    /// plus the slot settings (tiledata values, radar color), names, categories, tags and templates.
    /// <code>
    /// Name.landlib.idx / .mul   land tiles  (44x44)
    /// Name.texlib.idx  / .mul   textures    (64x64 / 128x128)
    /// Name.terrain.json         settings, names, categories, tags, templates
    /// </code>
    /// Both layers always have the same capacity.
    /// </summary>
    public sealed class TerrainLibraryFile : IDisposable
    {
        public const string MetadataExtension = ".terrain.json";
        public const int DefaultCapacity = 1_000_000;

        private bool _disposed;

        private TerrainLibraryFile(string basePath, TerrainLayerStorage land, TerrainLayerStorage textures, TerrainMetadata metadata)
        {
            BasePath = basePath;
            Land = land;
            Textures = textures;
            Metadata = metadata;
            Land.EntryChanged += (s, e) => SlotChanged?.Invoke(this, new TerrainSlotChangedEventArgs(TerrainLayer.LandTile, e.Id));
            Textures.EntryChanged += (s, e) => SlotChanged?.Invoke(this, new TerrainSlotChangedEventArgs(TerrainLayer.Texture, e.Id));
        }

        /// <summary>Raised after a land tile or texture of a slot changed.</summary>
        public event EventHandler<TerrainSlotChangedEventArgs> SlotChanged;

        public string BasePath { get; }

        public string FileName => Path.GetFileName(BasePath);

        public string Name => Land.Name;

        public string MetadataPath => BasePath + MetadataExtension;

        /// <summary>Path that identifies the library (the land tile index file).</summary>
        public string MainPath => Land.IdxPath;

        public TerrainLayerStorage Land { get; }

        public TerrainLayerStorage Textures { get; }

        public TerrainMetadata Metadata { get; private set; }

        public int Capacity => Math.Min(Land.Capacity, Textures.Capacity);

        public int MaxId => Capacity - 1;

        public bool IsValidId(int id) => id >= 0 && id < Capacity;

        public TerrainLayerStorage this[TerrainLayer layer] => layer == TerrainLayer.LandTile ? Land : Textures;

        public bool AutoSave
        {
            get => Land.AutoSave;
            set
            {
                Land.AutoSave = value;
                Textures.AutoSave = value;
            }
        }

        public bool HasUnsavedGraphics => Land.HasUnsavedChanges || Textures.HasUnsavedChanges;

        public int UnsavedCount => Land.UnsavedCount + Textures.UnsavedCount;

        #region [ Paths ]

        /// <summary>Base path (without extension) for any file of a terrain library or the base path itself.</summary>
        public static string GetBasePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path is empty.", nameof(path));
            }

            path = Path.GetFullPath(path);
            foreach (string extension in new[]
                     {
                         TerrainLayer.LandTile.IdxExtension(), TerrainLayer.LandTile.MulExtension(),
                         TerrainLayer.Texture.IdxExtension(), TerrainLayer.Texture.MulExtension(), MetadataExtension
                     })
            {
                if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    return path.Substring(0, path.Length - extension.Length);
                }
            }

            return path;
        }

        public static bool Exists(string directory, string fileName)
        {
            string basePath = Path.Combine(directory, fileName);
            return File.Exists(basePath + TerrainLayer.LandTile.IdxExtension())
                   || File.Exists(basePath + TerrainLayer.Texture.IdxExtension())
                   || File.Exists(basePath + MetadataExtension);
        }

        public static bool IsValidFileName(string fileName) => ArtLibraryFile.IsValidFileName(fileName);

        #endregion

        #region [ Create / Open ]

        public static TerrainLibraryFile Create(string directory, string fileName, int capacity = DefaultCapacity)
        {
            if (!IsValidFileName(fileName))
            {
                throw new ArgumentException($"\"{fileName}\" ist kein gültiger Name.", nameof(fileName));
            }

            Directory.CreateDirectory(directory);
            if (Exists(directory, fileName))
            {
                throw new IOException($"Im Ordner gibt es bereits eine Terrain-Bibliothek \"{fileName}\".");
            }

            string basePath = Path.GetFullPath(Path.Combine(directory, fileName));
            TerrainLayerStorage land = null;
            TerrainLayerStorage textures = null;
            try
            {
                land = TerrainLayerStorage.Create(TerrainLayer.LandTile, basePath, fileName, capacity);
                textures = TerrainLayerStorage.Create(TerrainLayer.Texture, basePath, fileName, capacity);
                TerrainMetadata metadata = TerrainMetadata.Load(basePath + MetadataExtension);
                metadata.Save();
                return new TerrainLibraryFile(basePath, land, textures, metadata);
            }
            catch
            {
                land?.Dispose();
                textures?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Opens a terrain library; <paramref name="path"/> may be any of its files or the base path.
        /// A missing texture part (e.g. only land tiles copied) is created empty with the same capacity.
        /// </summary>
        public static TerrainLibraryFile Open(string path)
        {
            string basePath = GetBasePath(path);
            TerrainLayerStorage land = null;
            TerrainLayerStorage textures = null;
            try
            {
                land = File.Exists(basePath + TerrainLayer.LandTile.IdxExtension())
                    ? TerrainLayerStorage.Open(TerrainLayer.LandTile, basePath)
                    : null;
                textures = File.Exists(basePath + TerrainLayer.Texture.IdxExtension())
                    ? TerrainLayerStorage.Open(TerrainLayer.Texture, basePath)
                    : null;

                if (land == null && textures == null)
                {
                    throw new FileNotFoundException("Keine Terrain-Bibliothek gefunden.", basePath + TerrainLayer.LandTile.IdxExtension());
                }

                string name = Path.GetFileName(basePath);
                land ??= TerrainLayerStorage.Create(TerrainLayer.LandTile, basePath, name, textures.Capacity);
                textures ??= TerrainLayerStorage.Create(TerrainLayer.Texture, basePath, name, land.Capacity);

                // keep both layers at the same number of slots
                if (land.Capacity < textures.Capacity)
                {
                    land.Grow(textures.Capacity);
                }
                else if (textures.Capacity < land.Capacity)
                {
                    textures.Grow(land.Capacity);
                }

                TerrainMetadata metadata = TerrainMetadata.Load(basePath + MetadataExtension);
                return new TerrainLibraryFile(basePath, land, textures, metadata);
            }
            catch
            {
                land?.Dispose();
                textures?.Dispose();
                throw;
            }
        }

        #endregion

        #region [ Operations ]

        /// <summary>Writes pending graphic changes and the metadata.</summary>
        public void Save()
        {
            Land.Save();
            Textures.Save();
            Metadata.Save();
        }

        /// <summary>Saves only the metadata (json).</summary>
        public void SaveMetadata()
        {
            Metadata.Save();
        }

        public void Grow(int newCapacity)
        {
            Land.Grow(newCapacity);
            Textures.Grow(newCapacity);
        }

        /// <summary>Compacts both data files, returns the bytes saved.</summary>
        public long Compact()
        {
            return Land.Compact() + Textures.Compact();
        }

        public long GetUnusedBytes() => Land.GetUnusedBytes() + Textures.GetUnusedBytes();

        /// <summary>True when the slot holds a land tile, a texture or settings.</summary>
        public bool IsSlotUsed(int id)
        {
            return Land.IsOccupied(id) || Textures.IsOccupied(id) || (Metadata.GetSlot(id)?.HasSettings ?? false);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Land.Dispose();
            Textures.Dispose();
        }

        #endregion
    }
}

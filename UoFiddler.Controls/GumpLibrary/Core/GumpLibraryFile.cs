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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UoFiddler.Controls.ArtLibrary.Core;
using UoFiddler.Controls.GumpLibrary.Metadata;

namespace UoFiddler.Controls.GumpLibrary.Core
{
    /// <summary>
    /// A gump library: gump graphics, own preview images and metadata under one name.
    /// <code>
    /// Name.gumplib.idx / Name.gumplib.mul     gumps
    /// Name.gumpprev.idx / Name.gumpprev.mul   own preview images (optional per gump)
    /// Name.gumplib.json                       names, descriptions, categories, tags, sort order
    /// </code>
    /// </summary>
    public sealed class GumpLibraryFile : IDisposable
    {
        public const string MetadataExtension = ".gumplib.json";
        public const int DefaultCapacity = 1_000_000;

        private bool _disposed;

        private GumpLibraryFile(string basePath, GumpLibraryStorage gumps, GumpLibraryStorage previews, GumpLibraryMetadata metadata)
        {
            BasePath = basePath;
            Gumps = gumps;
            Previews = previews;
            Metadata = metadata;
            Gumps.EntryChanged += (s, e) => EntryChanged?.Invoke(this, e);
            Previews.EntryChanged += (s, e) => PreviewChanged?.Invoke(this, e);
        }

        /// <summary>A gump was written, removed or the whole library changed.</summary>
        public event EventHandler<ArtLibraryChangedEventArgs> EntryChanged;

        /// <summary>A preview image was written or removed.</summary>
        public event EventHandler<ArtLibraryChangedEventArgs> PreviewChanged;

        public string BasePath { get; }

        public string FileName => Path.GetFileName(BasePath);

        public string Name => Gumps.Name;

        public string MetadataPath => BasePath + MetadataExtension;

        /// <summary>The file shown in recent lists (the gump index).</summary>
        public string MainPath => Gumps.IdxPath;

        public GumpLibraryStorage Gumps { get; }

        public GumpLibraryStorage Previews { get; }

        public GumpLibraryMetadata Metadata { get; }

        public int Capacity => Gumps.Capacity;

        public int MaxId => Capacity - 1;

        public int OccupiedCount => Gumps.OccupiedCount;

        public bool IsValidId(int id) => id >= 0 && id < Capacity;

        public bool AutoSave
        {
            get => Gumps.AutoSave;
            set
            {
                Gumps.AutoSave = value;
                Previews.AutoSave = value;
            }
        }

        public bool HasUnsavedGraphics => Gumps.HasUnsavedChanges || Previews.HasUnsavedChanges;

        public int UnsavedCount => Gumps.UnsavedCount + Previews.UnsavedCount;

        public bool HasUnsavedChanges => HasUnsavedGraphics || Metadata.IsDirty;

        #region [ Paths ]

        public static string GetBasePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path is empty.", nameof(path));
            }

            path = Path.GetFullPath(path);
            foreach (string extension in new[]
                     {
                         GumpStorageKind.Gumps.IdxExtension(), GumpStorageKind.Gumps.MulExtension(),
                         GumpStorageKind.Previews.IdxExtension(), GumpStorageKind.Previews.MulExtension(),
                         MetadataExtension
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
            return File.Exists(basePath + GumpStorageKind.Gumps.IdxExtension()) || File.Exists(basePath + GumpStorageKind.Gumps.MulExtension());
        }

        public static bool IsValidFileName(string fileName) => ArtLibraryFile.IsValidFileName(fileName);

        #endregion

        #region [ Create / Open ]

        public static GumpLibraryFile Create(string directory, string fileName, int capacity = DefaultCapacity)
        {
            if (!IsValidFileName(fileName))
            {
                throw new ArgumentException($"\"{fileName}\" ist kein gültiger Name.", nameof(fileName));
            }

            Directory.CreateDirectory(directory);
            if (Exists(directory, fileName))
            {
                throw new IOException($"Im Ordner gibt es bereits eine Gump-Bibliothek \"{fileName}\".");
            }

            string basePath = Path.GetFullPath(Path.Combine(directory, fileName));
            GumpLibraryStorage gumps = null;
            GumpLibraryStorage previews = null;
            try
            {
                gumps = GumpLibraryStorage.Create(GumpStorageKind.Gumps, basePath, fileName, capacity);
                previews = File.Exists(basePath + GumpStorageKind.Previews.IdxExtension())
                    ? GumpLibraryStorage.Open(GumpStorageKind.Previews, basePath)
                    : GumpLibraryStorage.Create(GumpStorageKind.Previews, basePath, fileName, capacity);
                GumpLibraryMetadata metadata = GumpLibraryMetadata.Load(basePath + MetadataExtension);
                metadata.Save();
                return new GumpLibraryFile(basePath, gumps, previews, metadata);
            }
            catch
            {
                gumps?.Dispose();
                previews?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Opens a gump library; <paramref name="path"/> may be any of its files or the base path.
        /// A missing preview part is created empty.
        /// </summary>
        public static GumpLibraryFile Open(string path)
        {
            string basePath = GetBasePath(path);
            GumpLibraryStorage gumps = null;
            GumpLibraryStorage previews = null;
            try
            {
                if (!File.Exists(basePath + GumpStorageKind.Gumps.IdxExtension()))
                {
                    throw new FileNotFoundException("Keine Gump-Bibliothek gefunden.", basePath + GumpStorageKind.Gumps.IdxExtension());
                }

                gumps = GumpLibraryStorage.Open(GumpStorageKind.Gumps, basePath);
                previews = File.Exists(basePath + GumpStorageKind.Previews.IdxExtension())
                    ? GumpLibraryStorage.Open(GumpStorageKind.Previews, basePath)
                    : GumpLibraryStorage.Create(GumpStorageKind.Previews, basePath, gumps.Name, gumps.Capacity);
                if (previews.Capacity < gumps.Capacity)
                {
                    previews.Grow(gumps.Capacity);
                }

                GumpLibraryMetadata metadata = GumpLibraryMetadata.Load(basePath + MetadataExtension);
                return new GumpLibraryFile(basePath, gumps, previews, metadata);
            }
            catch
            {
                gumps?.Dispose();
                previews?.Dispose();
                throw;
            }
        }

        #endregion

        #region [ Graphics ]

        public bool IsOccupied(int id) => Gumps.IsOccupied(id);

        public bool HasPreview(int id) => Previews.IsOccupied(id);

        /// <summary>Gump or preview changed and not saved yet (AutoSave off).</summary>
        public bool IsPending(int id) => Gumps.IsPending(id) || Previews.IsPending(id);

        /// <summary>Gump graphic (shared, do not modify or dispose) or null.</summary>
        public Bitmap GetGump(int id) => Gumps.GetBitmap(id);

        /// <summary>Own preview image (shared) or null.</summary>
        public Bitmap GetPreview(int id) => Previews.GetBitmap(id);

        /// <summary>The image for lists: own preview if there is one, otherwise the gump.</summary>
        public Bitmap GetThumbnailSource(int id) => Previews.IsOccupied(id) ? Previews.GetBitmap(id) : Gumps.GetBitmap(id);

        public bool TryGetSize(int id, out int width, out int height) => Gumps.TryGetSize(id, out width, out height);

        public List<int> GetOccupiedIds() => Gumps.GetOccupiedIds();

        public int FindNextFree(int startId = 0) => Gumps.FindNextFree(startId);

        /// <summary>Stores a gump; new entries get a metadata entry with the next sort position.</summary>
        public void SetGump(int id, Bitmap bitmap)
        {
            bool isNew = !Gumps.IsOccupied(id);
            Gumps.SetBitmap(id, bitmap);
            GumpLibraryItemInfo info = Metadata.GetOrCreate(id);
            if (isNew && info.Added == default)
            {
                info.Added = DateTime.Now;
            }

            info.Modified = DateTime.Now;
            Metadata.MarkDirty();
        }

        public void SetPreview(int id, Bitmap bitmap)
        {
            Previews.SetBitmap(id, bitmap);
            Metadata.GetOrCreate(id).Modified = DateTime.Now;
            Metadata.MarkDirty();
        }

        public void RemovePreview(int id)
        {
            if (Previews.IsOccupied(id))
            {
                Previews.Remove(id);
            }
        }

        /// <summary>Removes gump, preview and metadata of the slot.</summary>
        public void Remove(int id)
        {
            if (Gumps.IsOccupied(id))
            {
                Gumps.Remove(id);
            }

            RemovePreview(id);
            Metadata.Remove(id);
        }

        /// <summary>Copies gump, preview and metadata to another slot (target is replaced).</summary>
        public void Copy(int sourceId, int targetId)
        {
            if (sourceId == targetId)
            {
                return;
            }

            Gumps.Copy(sourceId, targetId);
            if (Previews.IsOccupied(sourceId))
            {
                Previews.Copy(sourceId, targetId);
            }
            else
            {
                RemovePreview(targetId);
            }

            Metadata.Copy(sourceId, targetId);
        }

        public void Move(int sourceId, int targetId)
        {
            if (sourceId == targetId)
            {
                return;
            }

            Gumps.Move(sourceId, targetId);
            if (Previews.IsOccupied(sourceId))
            {
                Previews.Move(sourceId, targetId);
            }
            else
            {
                RemovePreview(targetId);
            }

            Metadata.Move(sourceId, targetId);
        }

        public void Swap(int firstId, int secondId)
        {
            if (firstId == secondId)
            {
                return;
            }

            Gumps.Swap(firstId, secondId);
            Previews.Swap(firstId, secondId);
            Metadata.Swap(firstId, secondId);
        }

        #endregion

        #region [ Operations ]

        /// <summary>Writes pending graphic changes and the metadata.</summary>
        public void Save()
        {
            Gumps.Save();
            Previews.Save();
            Metadata.Save();
        }

        public void SaveMetadata() => Metadata.Save();

        public void Grow(int newCapacity)
        {
            Gumps.Grow(newCapacity);
            Previews.Grow(newCapacity);
        }

        public long Compact() => Gumps.Compact() + Previews.Compact();

        public long GetUnusedBytes() => Gumps.GetUnusedBytes() + Previews.GetUnusedBytes();

        public long DataLength => Gumps.MulLength + Previews.MulLength;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Gumps.Dispose();
            Previews.Dispose();
        }

        #endregion

        public override string ToString() => $"{Name} ({OccupiedCount:N0} / {Capacity:N0})";
    }
}

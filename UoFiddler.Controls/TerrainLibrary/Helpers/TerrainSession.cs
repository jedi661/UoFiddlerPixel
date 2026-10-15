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
using UoFiddler.Controls.TerrainLibrary.Core;

namespace UoFiddler.Controls.TerrainLibrary.Helpers
{
    /// <summary>
    /// The terrain library that is open right now. Both windows (land tiles and textures) work on this
    /// one instance, so a change in one window is visible in the other immediately and the cache is shared.
    /// </summary>
    public static class TerrainSession
    {
        private static int _batchDepth;

        static TerrainSession()
        {
            Settings = TerrainLibrarySettings.Load();
        }

        public static TerrainLibrarySettings Settings { get; }

        public static TerrainLibraryFile Library { get; private set; }

        public static DateTime? LastSaved { get; private set; }

        /// <summary>A library was opened, replaced or closed.</summary>
        public static event EventHandler LibraryChanged;

        /// <summary>A land tile or texture changed (not raised while a batch runs).</summary>
        public static event EventHandler<TerrainSlotChangedEventArgs> SlotChanged;

        /// <summary>Settings, names, categories, tags or templates changed (sender = window that changed them).</summary>
        public static event EventHandler MetadataChanged;

        /// <summary>Save state changed (saved / unsaved).</summary>
        public static event EventHandler SaveStateChanged;

        /// <summary>A batch ended; windows rebuild their lists. Argument = slot to select or -1.</summary>
        public static event EventHandler<int> BatchCompleted;

        /// <summary>The selected slot of one window changed (for synchronous selection).</summary>
        public static event EventHandler<int> SlotSelected;

        public static bool InBatch => _batchDepth > 0;

        public static bool HasUnsavedChanges =>
            Library != null && (Library.HasUnsavedGraphics || (!Settings.AutoSave && Library.Metadata.IsDirty));

        #region [ Open / Close ]

        public static void SetLibrary(TerrainLibraryFile library)
        {
            CloseLibrary();

            Library = library;
            Library.AutoSave = Settings.AutoSave;
            Library.SlotChanged += OnSlotChanged;
            LastSaved = null;

            Settings.AddRecent(Library.MainPath);
            Settings.LastLibrary = Library.MainPath;
            Settings.LastDirectory = Path.GetDirectoryName(Library.BasePath);
            Settings.Save();

            LibraryChanged?.Invoke(null, EventArgs.Empty);
            SaveStateChanged?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Closes the library. With auto save on the metadata is saved; with auto save off unsaved
        /// changes are discarded - ask the user before.
        /// </summary>
        public static void CloseLibrary()
        {
            if (Library == null)
            {
                return;
            }

            if (Settings.AutoSave)
            {
                TrySaveMetadata();
            }

            Library.SlotChanged -= OnSlotChanged;
            Library.Dispose();
            Library = null;
            LibraryChanged?.Invoke(null, EventArgs.Empty);
            SaveStateChanged?.Invoke(null, EventArgs.Empty);
        }

        #endregion

        #region [ Save ]

        /// <summary>Saves graphics and metadata.</summary>
        public static void Save()
        {
            if (Library == null)
            {
                return;
            }

            Library.Save();
            LastSaved = DateTime.Now;
            SaveStateChanged?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>Saves the metadata when auto save is on (called after every metadata change and by a timer).</summary>
        public static void SaveMetadataIfAuto()
        {
            if (Library != null && Settings.AutoSave && Library.Metadata.IsDirty && TrySaveMetadata())
            {
                LastSaved = DateTime.Now;
                SaveStateChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        private static bool TrySaveMetadata()
        {
            try
            {
                Library.SaveMetadata();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SetAutoSave(bool value)
        {
            Settings.AutoSave = value;
            Settings.Save();
            if (Library != null)
            {
                Library.AutoSave = value; // switching on writes pending graphic changes
                if (value)
                {
                    TrySaveMetadata();
                    LastSaved = DateTime.Now;
                }
            }

            SaveStateChanged?.Invoke(null, EventArgs.Empty);
        }

        #endregion

        #region [ Notifications ]

        public static void NotifyMetadataChanged(object sender)
        {
            Library?.Metadata.MarkDirty();
            MetadataChanged?.Invoke(sender, EventArgs.Empty);
            SaveMetadataIfAuto();
            SaveStateChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void NotifySlotSelected(object sender, int id)
        {
            SlotSelected?.Invoke(sender, id);
        }

        /// <summary>Many changes at once: windows update once at the end.</summary>
        public static void BeginBatch()
        {
            ++_batchDepth;
        }

        public static void EndBatch(int selectId = -1)
        {
            if (--_batchDepth > 0)
            {
                return;
            }

            _batchDepth = 0;
            MarkChanged();
            BatchCompleted?.Invoke(null, selectId);
            SaveMetadataIfAuto();
        }

        private static void OnSlotChanged(object sender, TerrainSlotChangedEventArgs e)
        {
            MarkChanged();
            if (!InBatch)
            {
                SlotChanged?.Invoke(null, e);
            }
        }

        private static void MarkChanged()
        {
            if (Settings.AutoSave)
            {
                LastSaved = DateTime.Now;
            }

            SaveStateChanged?.Invoke(null, EventArgs.Empty);
        }

        #endregion
    }
}

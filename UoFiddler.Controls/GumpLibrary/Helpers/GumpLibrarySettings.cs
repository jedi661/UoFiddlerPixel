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
using System.IO;
using System.Linq;
using System.Text.Json;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.GumpLibrary.Metadata;

namespace UoFiddler.Controls.GumpLibrary.Helpers
{
    /// <summary>
    /// Settings of the gump library window, stored as GumpLibrarySettings.json in the UoFiddler AppData folder.
    /// </summary>
    public sealed class GumpLibrarySettings
    {
        public const int MaxRecentLibraries = 15;
        private const string FileName = "GumpLibrarySettings.json";

        public List<string> RecentLibraries { get; set; } = new List<string>();

        public string LastLibrary { get; set; }

        public string LastDirectory { get; set; }

        public string LastImportDirectory { get; set; }

        public string LastExportDirectory { get; set; }

        /// <summary>Write every change immediately (on) or only when "Speichern" is clicked (off).</summary>
        public bool AutoSave { get; set; } = true;

        public bool ShowFreeSlots { get; set; }

        public GumpSortMode SortMode { get; set; } = GumpSortMode.Id;

        public bool SortDescending { get; set; }

        /// <summary>Use the own preview image in the list (on) or always the gump (off).</summary>
        public bool UsePreviewInList { get; set; } = true;

        /// <summary>When gumps are taken from the original, take the name from IDGumpNames.xml.</summary>
        public bool TakeNamesFromXml { get; set; } = true;

        /// <summary>Show the detail panel (name, categories, tags, description) on the right.</summary>
        public bool ShowDetails { get; set; } = true;

        /// <summary>Background colors black/white/#D3D3D3 become transparent on import.</summary>
        public bool ImportBlackTransparent { get; set; } = true;

        public bool ImportWhiteTransparent { get; set; } = true;

        public bool ImportLightGrayTransparent { get; set; } = true;

        /// <summary>Largest side of a generated preview image.</summary>
        public int PreviewMaxSize { get; set; } = 100;

        /// <summary>Background of the big picture (ARGB), null = window default.</summary>
        public int? BackgroundArgb { get; set; }

        public int ListSplitterDistance { get; set; }

        public int DetailSplitterDistance { get; set; }

        public static string SettingsPath
        {
            get
            {
                string folder = string.IsNullOrEmpty(Options.AppDataPath)
                    ? AppDomain.CurrentDomain.BaseDirectory
                    : Options.AppDataPath;
                return Path.Combine(folder, FileName);
            }
        }

        public static GumpLibrarySettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var settings = JsonSerializer.Deserialize<GumpLibrarySettings>(File.ReadAllText(SettingsPath));
                    if (settings != null)
                    {
                        settings.RecentLibraries ??= new List<string>();
                        if (settings.PreviewMaxSize < 16 || settings.PreviewMaxSize > 1024)
                        {
                            settings.PreviewMaxSize = 100;
                        }

                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                Options.Logger?.Warning(ex, "GumpLibrary: could not read settings {Path}", SettingsPath);
            }

            return new GumpLibrarySettings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
                File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                Options.Logger?.Warning(ex, "GumpLibrary: could not write settings {Path}", SettingsPath);
            }
        }

        public void AddRecent(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            RecentLibraries.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
            RecentLibraries.Insert(0, path);
            if (RecentLibraries.Count > MaxRecentLibraries)
            {
                RecentLibraries = RecentLibraries.Take(MaxRecentLibraries).ToList();
            }
        }

        public void RemoveRecent(string path)
        {
            RecentLibraries.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
        }
    }
}

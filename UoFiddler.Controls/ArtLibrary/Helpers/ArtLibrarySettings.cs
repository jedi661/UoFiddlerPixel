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

namespace UoFiddler.Controls.ArtLibrary.Helpers
{
    /// <summary>
    /// User settings of the art library window, stored as ArtLibrarySettings.json in the UoFiddler AppData folder.
    /// </summary>
    public sealed class ArtLibrarySettings
    {
        public const int MaxRecentLibraries = 15;
        private const string FileName = "ArtLibrarySettings.json";

        /// <summary>Recently used libraries (base path or .artlib.idx path), newest first.</summary>
        public List<string> RecentLibraries { get; set; } = new List<string>();

        /// <summary>Library that was open when the window was closed.</summary>
        public string LastLibrary { get; set; }

        /// <summary>Folder last used for creating/opening libraries.</summary>
        public string LastDirectory { get; set; }

        public bool ShowOnlyOccupied { get; set; } = true;

        /// <summary>Write every change immediately (on) or only when "Speichern" is clicked (off).</summary>
        public bool AutoSave { get; set; } = true;

        /// <summary>Pure black becomes transparent when importing (clipboard, files) - like the items tab.</summary>
        public bool ImportBlackTransparent { get; set; } = true;

        /// <summary>Pure white (and #D3D3D3) becomes transparent when importing - like the items tab.</summary>
        public bool ImportWhiteTransparent { get; set; } = true;

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

        public static ArtLibrarySettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var settings = JsonSerializer.Deserialize<ArtLibrarySettings>(File.ReadAllText(SettingsPath));
                    if (settings != null)
                    {
                        settings.RecentLibraries ??= new List<string>();
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                Options.Logger?.Warning(ex, "ArtLibrary: could not read settings {Path}", SettingsPath);
            }

            return new ArtLibrarySettings();
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
                Options.Logger?.Warning(ex, "ArtLibrary: could not write settings {Path}", SettingsPath);
            }
        }

        /// <summary>
        /// Moves <paramref name="path"/> to the top of the recent list.
        /// </summary>
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

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

namespace UoFiddler.Controls.TerrainLibrary.Helpers
{
    /// <summary>
    /// Settings of the terrain library windows, stored as TerrainLibrarySettings.json in the UoFiddler AppData folder.
    /// </summary>
    public sealed class TerrainLibrarySettings
    {
        public const int MaxRecentLibraries = 15;
        private const string FileName = "TerrainLibrarySettings.json";

        public List<string> RecentLibraries { get; set; } = new List<string>();

        public string LastLibrary { get; set; }

        public string LastDirectory { get; set; }

        /// <summary>Show free slots in the land tile window.</summary>
        public bool LandShowFreeSlots { get; set; }

        /// <summary>Show free slots in the texture window.</summary>
        public bool TextureShowFreeSlots { get; set; }

        /// <summary>Write every change immediately (on) or only when "Speichern" is clicked (off).</summary>
        public bool AutoSave { get; set; } = true;

        /// <summary>Scale imported images to 44x44 / 64x64 / 128x128 (on) or reject other sizes (off).</summary>
        public bool AutoScale { get; set; } = true;

        /// <summary>When a land tile is taken from the original, also take its tiledata values (name, flags, texture id).</summary>
        public bool AutoTileData { get; set; } = true;

        /// <summary>When a land tile is taken from the original, also take its texture (tiledata texture id) into the same slot.</summary>
        public bool FetchLinkedTexture { get; set; } = true;

        /// <summary>When a land tile is taken from the original, also take its radar color.</summary>
        public bool AutoRadarColor { get; set; } = true;

        /// <summary>Also write the slot's radar color into radarcol.mul when a land tile is written to the original.</summary>
        public bool WriteRadarColorToOriginal { get; set; }

        /// <summary>Show the built-in templates (Standardvorlagen) next to the own ones.</summary>
        public bool UseBuiltInPresets { get; set; } = true;

        public LandConversionMode LandConversion { get; set; } = LandConversionMode.Isometric;

        /// <summary>Selecting a slot in one window selects the same slot in the other window.</summary>
        public bool SyncSelection { get; set; } = true;

        /// <summary>Texture window docked to the right side of the land tile window.</summary>
        public bool Docked { get; set; } = true;

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

        public static TerrainLibrarySettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var settings = JsonSerializer.Deserialize<TerrainLibrarySettings>(File.ReadAllText(SettingsPath));
                    if (settings != null)
                    {
                        settings.RecentLibraries ??= new List<string>();
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                Options.Logger?.Warning(ex, "TerrainLibrary: could not read settings {Path}", SettingsPath);
            }

            return new TerrainLibrarySettings();
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
                Options.Logger?.Warning(ex, "TerrainLibrary: could not write settings {Path}", SettingsPath);
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

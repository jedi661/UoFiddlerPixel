/***************************************************************************
 *
 * MultiEditor – settings and language helper (DE / EN)
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    public sealed class EditorSettings
    {
        public bool German { get; set; } = true;
        public string LastImportDir { get; set; } = string.Empty;
        public string LastExportDir { get; set; } = string.Empty;
        public List<string> RecentFiles { get; set; } = new List<string>();
        public string ServUONamespace { get; set; } = "Server.Items";
        public bool AddonHexIds { get; set; } = true;
        public int AddonStyle { get; set; }
        public bool ShowGrid { get; set; }
        public bool ShowHeights { get; set; }
        public bool ShowInvisible { get; set; } = true;
        public bool AutoBackup { get; set; } = true;
        public string LastMulDir { get; set; } = string.Empty;
        public List<int> RecentTiles { get; set; } = new List<int>();

        private static EditorSettings _instance;

        public static EditorSettings Instance => _instance ??= Load();

        private static string FilePath
        {
            get
            {
                string dir = Options.AppDataPath ?? AppContext.BaseDirectory;
                return Path.Combine(dir, "plugins", "MultiEditorSettings.json");
            }
        }

        private static EditorSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    return JsonSerializer.Deserialize<EditorSettings>(File.ReadAllText(FilePath)) ?? new EditorSettings();
                }
            }
            catch
            {
                // ignore broken settings
            }

            return new EditorSettings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch
            {
                // ignore
            }
        }

        public void AddRecentFile(string file)
        {
            RecentFiles.Remove(file);
            RecentFiles.Insert(0, file);
            while (RecentFiles.Count > 12)
            {
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
            }

            Save();
        }

        public void AddRecentTile(int id)
        {
            RecentTiles.Remove(id);
            RecentTiles.Insert(0, id);
            while (RecentTiles.Count > 60)
            {
                RecentTiles.RemoveAt(RecentTiles.Count - 1);
            }
        }
    }

    /// <summary>
    /// Tiny DE/EN helper: L.T("Deutsch", "English")
    /// </summary>
    public static class L
    {
        public static bool German => EditorSettings.Instance.German;

        public static string T(string de, string en)
        {
            return German ? de : en;
        }
    }
}

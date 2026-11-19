using System;
using System.IO;
using System.Text.Json;

namespace UoFiddler.Controls.Classes
{
    /// <summary>Remembered settings of the direction window (AppData\ArtDirectionSettings.json).</summary>
    public sealed class ArtDirectionSettings
    {
        public string ExportFolder { get; set; }

        private static string FilePath => Path.Combine(Options.AppDataPath ?? Path.GetTempPath(), "ArtDirectionSettings.json");

        public static ArtDirectionSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    return JsonSerializer.Deserialize<ArtDirectionSettings>(File.ReadAllText(FilePath)) ?? new ArtDirectionSettings();
                }
            }
            catch (Exception)
            {
                // damaged file: defaults
            }

            return new ArtDirectionSettings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception)
            {
                // not remembered - no harm
            }
        }
    }
}

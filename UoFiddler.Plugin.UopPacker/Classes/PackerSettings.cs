/***************************************************************************
 *
 * $Author: Turley
 * Advanced Nikodemus
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.IO;
using System.Xml.Serialization;

namespace UoFiddler.Plugin.UopPacker.Classes
{
    public enum BackupLocation
    {
        /// <summary>"UoFiddlerBackups\UopPacker" in the folder of the written file.</summary>
        NextToFile,

        /// <summary>One folder chosen by the user.</summary>
        FixedFolder
    }

    /// <summary>
    /// Settings of the UOP packer tab, stored as UopPackerSettings.xml in the UoFiddler settings folder.
    /// </summary>
    public sealed class PackerSettings
    {
        public bool OverwriteUop { get; set; } = true;
        public bool OverwriteMul { get; set; }
        public bool AutoFill { get; set; } = true;
        public string Compression { get; set; } = "None";
        public string LastFolder { get; set; } = string.Empty;

        public bool BackupEnabled { get; set; } = true;
        public BackupLocation BackupLocation { get; set; } = BackupLocation.NextToFile;
        public string BackupFolder { get; set; } = string.Empty;

        /// <summary>Also copy the new files into the backup folder ("new"), so the old and the new version are in one place.</summary>
        public bool BackupCopyNew { get; set; } = true;

        /// <summary>"en" or "de".</summary>
        public string HelpLanguage { get; set; } = "en";

        public static string FilePath
        {
            get
            {
                string folder = null;
                try
                {
                    folder = UoFiddler.Controls.Classes.Options.AppDataPath;
                }
                catch (Exception)
                {
                    // not set
                }
                if (string.IsNullOrEmpty(folder))
                {
                    folder = AppDomain.CurrentDomain.BaseDirectory;
                }
                return Path.Combine(folder, "UopPackerSettings.xml");
            }
        }

        public static PackerSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    using (var stream = File.OpenRead(FilePath))
                    {
                        return (PackerSettings)new XmlSerializer(typeof(PackerSettings)).Deserialize(stream) ?? new PackerSettings();
                    }
                }
            }
            catch (Exception)
            {
                // broken file: defaults
            }
            return new PackerSettings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                using (var stream = File.Create(FilePath))
                {
                    new XmlSerializer(typeof(PackerSettings)).Serialize(stream, this);
                }
            }
            catch (Exception)
            {
                // settings are not important enough to show an error
            }
        }
    }
}

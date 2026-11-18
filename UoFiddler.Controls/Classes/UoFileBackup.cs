using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Ultima;

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// Copies the original client files that the extended format touches (art + tiledata, legacy and extended)
    /// into  &lt;OutputPath&gt;\UO_Backup\yyyy-MM-dd_HH-mm-ss\  before anything is created or saved.
    /// Only copies - the files in the client directory are never changed or moved.
    /// </summary>
    public static class UoFileBackup
    {
        public const string BackupFolderName = "UO_Backup";

        /// <summary>Client files that belong to art / tiledata (missing ones are skipped).</summary>
        public static readonly string[] Files =
        {
            "art.mul",
            "artidx.mul",
            "artlegacymul.uop",
            "tiledata.mul",
            "verdata.mul",
            ExtendedFormat.ArtIndexFileName,
            ExtendedFormat.ArtDataFileName,
            ExtendedFormat.TileDataFileName
        };

        /// <summary>Folder of the last backup made in this session (null if none yet).</summary>
        public static string LastBackupFolder { get; private set; }

        public static string GetBackupRoot(string outputPath) => Path.Combine(outputPath, BackupFolderName);

        /// <summary>Creates a new dated backup folder and copies all existing files into it.</summary>
        /// <returns>The backup folder; <paramref name="copied"/> lists the copied file names.</returns>
        public static string CreateBackup(string outputPath, out List<string> copied)
        {
            if (string.IsNullOrEmpty(outputPath))
            {
                throw new InvalidOperationException("No output path set (Options -> output path).");
            }

            string folder = Path.Combine(GetBackupRoot(outputPath), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            // two backups in the same second: add a counter instead of overwriting
            string unique = folder;
            for (int i = 2; Directory.Exists(unique); ++i)
            {
                unique = $"{folder}_{i}";
            }

            Directory.CreateDirectory(unique);

            copied = new List<string>();
            var info = new List<string>
            {
                $"UOFiddler backup {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                $"Ultima Online directory: {Ultima.Files.RootDir}",
                string.Empty
            };

            foreach (string name in Files)
            {
                string source = Ultima.Files.GetFilePath(name);
                if (source == null || !File.Exists(source))
                {
                    continue;
                }

                string target = Path.Combine(unique, Path.GetFileName(source));
                File.Copy(source, target, false);
                copied.Add(Path.GetFileName(source));
                info.Add($"{Path.GetFileName(source),-20} {new FileInfo(source).Length,15:N0} bytes   from {source}");
            }

            File.WriteAllLines(Path.Combine(unique, "backup_info.txt"), info);

            LastBackupFolder = unique;
            return unique;
        }

        /// <summary>Opens a folder in the Windows Explorer (created if it does not exist yet).</summary>
        public static void OpenFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("The folder is not set.");
            }

            Directory.CreateDirectory(path);
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
    }
}

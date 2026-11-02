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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace UoFiddler.Plugin.UopPacker.Classes
{
    /// <summary>One file in the backup folder.</summary>
    public sealed class BackupEntry
    {
        public string FileName { get; set; }
        public string OriginalPath { get; set; }
        public DateTime Time { get; set; }
        public string OldCopy { get; set; }
        public string NewCopy { get; set; }
    }

    /// <summary>
    /// Automatic backup: before a file is replaced, the old file is copied to "&lt;backup folder&gt;\old\&lt;name&gt;";
    /// optionally the new file is copied to "&lt;backup folder&gt;\new\&lt;name&gt;". Files with the same name are overwritten
    /// (one backup per file). "backup.txt" in the folder lists where each file came from.
    /// </summary>
    public sealed class PackerBackup
    {
        public const string DefaultSubFolder = "UoFiddlerBackups\\UopPacker";
        private const string ListFile = "backup.txt";

        private readonly PackerSettings _settings;

        /// <summary>Raised with a line for the log.</summary>
        public event Action<string> Logged;

        public PackerBackup(PackerSettings settings)
        {
            _settings = settings;
        }

        public bool Enabled => _settings.BackupEnabled;

        /// <summary>Backup folder for a written file.</summary>
        public string FolderFor(string target)
        {
            if (_settings.BackupLocation == BackupLocation.FixedFolder && !string.IsNullOrWhiteSpace(_settings.BackupFolder))
            {
                return _settings.BackupFolder;
            }
            string dir = Path.GetDirectoryName(Path.GetFullPath(target)) ?? string.Empty;
            return Path.Combine(dir, DefaultSubFolder.Replace('\\', Path.DirectorySeparatorChar));
        }

        /// <summary>Copies the existing target to "old" before it is replaced.</summary>
        public void BackupExisting(string target)
        {
            if (!Enabled || string.IsNullOrWhiteSpace(target) || !File.Exists(target))
            {
                return;
            }
            string folder = FolderFor(target);
            string old = Path.Combine(folder, "old", Path.GetFileName(target));
            Directory.CreateDirectory(Path.GetDirectoryName(old));
            File.Copy(target, old, true);
            Remember(folder, target);
            Logged?.Invoke($"Backup: {target} → {old}");
        }

        /// <summary>Copies the new file to "new" (if chosen).</summary>
        public void CopyNew(string target)
        {
            if (!Enabled || !_settings.BackupCopyNew || string.IsNullOrWhiteSpace(target) || !File.Exists(target))
            {
                return;
            }
            string folder = FolderFor(target);
            string copy = Path.Combine(folder, "new", Path.GetFileName(target));
            Directory.CreateDirectory(Path.GetDirectoryName(copy));
            File.Copy(target, copy, true);
            Remember(folder, target);
        }

        private static void Remember(string folder, string target)
        {
            string list = Path.Combine(folder, ListFile);
            var lines = File.Exists(list) ? File.ReadAllLines(list).ToList() : new List<string>();
            string name = Path.GetFileName(target);
            lines.RemoveAll(l => l.StartsWith(name + "|", StringComparison.OrdinalIgnoreCase));
            lines.Add($"{name}|{Path.GetFullPath(target)}|{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}");
            File.WriteAllLines(list, lines);
        }

        /// <summary>The files of a backup folder.</summary>
        public static List<BackupEntry> List(string folder)
        {
            var result = new List<BackupEntry>();
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                return result;
            }
            var known = new Dictionary<string, BackupEntry>(StringComparer.OrdinalIgnoreCase);
            string list = Path.Combine(folder, ListFile);
            if (File.Exists(list))
            {
                foreach (string line in File.ReadAllLines(list))
                {
                    string[] p = line.Split('|');
                    if (p.Length >= 3)
                    {
                        DateTime.TryParseExact(p[2], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime time);
                        known[p[0]] = new BackupEntry { FileName = p[0], OriginalPath = p[1], Time = time };
                    }
                }
            }
            foreach (string sub in new[] { "old", "new" })
            {
                string dir = Path.Combine(folder, sub);
                if (!Directory.Exists(dir))
                {
                    continue;
                }
                foreach (string file in Directory.GetFiles(dir))
                {
                    string name = Path.GetFileName(file);
                    if (!known.TryGetValue(name, out BackupEntry entry))
                    {
                        entry = new BackupEntry { FileName = name, Time = File.GetLastWriteTime(file) };
                        known[name] = entry;
                    }
                    if (sub == "old")
                    {
                        entry.OldCopy = file;
                    }
                    else
                    {
                        entry.NewCopy = file;
                    }
                }
            }
            result.AddRange(known.Values.Where(e => e.OldCopy != null || e.NewCopy != null).OrderBy(e => e.FileName, StringComparer.OrdinalIgnoreCase));
            return result;
        }

        /// <summary>Copies the old version back to where it came from (or to <paramref name="targetPath"/>).</summary>
        public static void Restore(BackupEntry entry, string targetPath)
        {
            if (entry?.OldCopy == null)
            {
                throw new FileNotFoundException("There is no old copy of this file.");
            }
            File.Copy(entry.OldCopy, targetPath, true);
        }
    }
}

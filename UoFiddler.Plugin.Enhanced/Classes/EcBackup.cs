/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace UoFiddler.Plugin.Enhanced.Classes
{
    /// <summary>
    /// Backup before a file is replaced: the old file is copied to "&lt;folder&gt;\old\&lt;name&gt;", the new one to
    /// "&lt;folder&gt;\new\&lt;name&gt;". Files with the same name are overwritten (one backup per file);
    /// "backup.txt" lists where each file came from.
    /// </summary>
    public sealed class EcBackup
    {
        private const string ListFile = "backup.txt";

        /// <summary>Folder (empty = "UoFiddlerBackups\&lt;sub folder&gt;" next to the written file).</summary>
        public string Folder { get; set; }

        public string SubFolder { get; }

        public bool Enabled { get; set; } = true;

        public EcBackup(string subFolder)
        {
            SubFolder = subFolder;
        }

        public string FolderFor(string target)
        {
            if (!string.IsNullOrWhiteSpace(Folder))
            {
                return Folder;
            }
            string dir = Path.GetDirectoryName(Path.GetFullPath(target)) ?? string.Empty;
            return Path.Combine(dir, "UoFiddlerBackups", SubFolder);
        }

        /// <summary>Copies the existing file to "old" before it is replaced; returns the copy or null.</summary>
        public string BackupExisting(string target)
        {
            if (!Enabled || string.IsNullOrWhiteSpace(target) || !File.Exists(target))
            {
                return null;
            }
            string folder = FolderFor(target);
            string old = Path.Combine(folder, "old", Path.GetFileName(target));
            Directory.CreateDirectory(Path.GetDirectoryName(old));
            File.Copy(target, old, true);
            Remember(folder, target);
            return old;
        }

        /// <summary>Copies the written file to "new"; returns the copy or null.</summary>
        public string CopyNew(string target)
        {
            if (!Enabled || string.IsNullOrWhiteSpace(target) || !File.Exists(target))
            {
                return null;
            }
            string folder = FolderFor(target);
            string copy = Path.Combine(folder, "new", Path.GetFileName(target));
            Directory.CreateDirectory(Path.GetDirectoryName(copy));
            File.Copy(target, copy, true);
            Remember(folder, target);
            return copy;
        }

        private static void Remember(string folder, string target)
        {
            string list = Path.Combine(folder, ListFile);
            List<string> lines = File.Exists(list) ? File.ReadAllLines(list).ToList() : new List<string>();
            string name = Path.GetFileName(target);
            lines.RemoveAll(l => l.StartsWith(name + "|", StringComparison.OrdinalIgnoreCase));
            lines.Add($"{name}|{Path.GetFullPath(target)}|{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}");
            File.WriteAllLines(list, lines);
        }
    }
}

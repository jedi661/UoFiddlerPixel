/***************************************************************************
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
using System.Xml;
using Ultima;

namespace UoFiddler.Controls.MapViewer
{
    /// <summary>
    /// A backup folder created by <see cref="MapSaveService"/>
    /// </summary>
    public sealed class MapBackupInfo
    {
        public string Directory { get; set; }
        public string TargetDirectory { get; set; }
        public string MapName { get; set; }
        public int FileIndex { get; set; }
        public DateTime Created { get; set; }
        public List<string> Files { get; } = new List<string>();
        public string Comment { get; set; }

        public override string ToString()
        {
            return $"{Created:yyyy-MM-dd HH:mm:ss}  {MapName} (map{FileIndex})  {string.Join(", ", Files)}";
        }
    }

    /// <summary>
    /// Writes the map files of an edit session into a directory. Files are written into a temp folder
    /// first, existing files are copied into a backup folder and then replaced. If the original files of a
    /// loaded map were replaced, the maps using them are reloaded.
    /// </summary>
    public static class MapSaveService
    {
        public const string BackupFolderName = "UoFiddlerBackups";
        private const string BackupInfoFile = "backup.xml";

        /// <summary>Directory of the files of a map (where map/staidx/statics are read from)</summary>
        public static string GetSourceDirectory(Map map)
        {
            TileMatrix tiles = map.Tiles;
            string file = tiles.MapPath ?? tiles.IndexPath ?? tiles.StaticsPath;
            return file == null ? null : Path.GetDirectoryName(Path.GetFullPath(file));
        }

        /// <summary>Names of the files that would be written</summary>
        public static List<string> GetFilesToWrite(Map map)
        {
            var files = new List<string>();
            TileMatrix tiles = map.Tiles;
            if (tiles.HasModifiedLand)
            {
                files.Add($"map{map.FileIndex}.mul");
            }

            if (tiles.HasStaticOverrides)
            {
                files.Add($"staidx{map.FileIndex}.mul");
                files.Add($"statics{map.FileIndex}.mul");
            }

            return files;
        }

        public static bool IsSameDirectory(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            {
                return false;
            }

            return string.Equals(Path.GetFullPath(a).TrimEnd('\\', '/'), Path.GetFullPath(b).TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Saves the changed files. Returns the written files and the created backup (or null).
        /// </summary>
        public static List<string> Save(Map map, string mapName, string targetDirectory, bool createBackup, out MapBackupInfo backup)
        {
            backup = null;
            Directory.CreateDirectory(targetDirectory);

            List<string> names = GetFilesToWrite(map);
            if (names.Count == 0)
            {
                return new List<string>();
            }

            string temp = Path.Combine(targetDirectory, ".uofiddler_tmp_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temp);
            try
            {
                // 1. write into the temp folder (the sources are still read here)
                TileMatrix tiles = map.Tiles;
                if (tiles.HasModifiedLand)
                {
                    map.SaveLand(temp);
                }

                if (tiles.HasStaticOverrides)
                {
                    Map.DefragStatics(temp, map, map.Width, map.Height, false);
                }

                // 2. release all file handles before files are replaced
                Map.CloseAllStreams();

                List<string> targets = names.Select(n => Path.Combine(targetDirectory, n)).ToList();

                // 3. backup of the files that will be replaced
                List<string> existing = targets.Where(File.Exists).ToList();
                if (createBackup && existing.Count > 0)
                {
                    backup = CreateBackup(targetDirectory, mapName, map.FileIndex, existing, "before save");
                }

                // 4. replace
                foreach (string name in names)
                {
                    File.Move(Path.Combine(temp, name), Path.Combine(targetDirectory, name), true);
                }

                // 5. maps that read the replaced files have to read them again
                ReloadMapsUsing(targets);
                return targets;
            }
            finally
            {
                try
                {
                    if (Directory.Exists(temp))
                    {
                        Directory.Delete(temp, true);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// True if one of the given files is used by a loaded map
        /// </summary>
        public static bool IsUsedByAMap(IEnumerable<string> files)
        {
            List<string> list = files.ToList();
            return Map.AllMaps().Any(m => list.Any(m.UsesFile));
        }

        private static void ReloadMapsUsing(List<string> files)
        {
            foreach (Map m in Map.AllMaps().ToList())
            {
                if (files.Any(m.UsesFile))
                {
                    m.ResetAll();
                }
            }
        }

        #region Backup

        /// <summary>
        /// Copies files into a new folder below <c>targetDirectory\UoFiddlerBackups</c>
        /// </summary>
        public static MapBackupInfo CreateBackup(string targetDirectory, string mapName, int fileIndex, IEnumerable<string> files, string comment)
        {
            var info = new MapBackupInfo
            {
                TargetDirectory = Path.GetFullPath(targetDirectory),
                MapName = mapName,
                FileIndex = fileIndex,
                Created = DateTime.Now,
                Comment = comment
            };

            string safeName = string.Concat((mapName ?? "map").Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
            string baseName = Path.Combine(targetDirectory, BackupFolderName, $"{info.Created:yyyyMMdd_HHmmss}_{safeName}_map{fileIndex}");

            // never reuse a folder (two backups in the same second, e.g. "before restore")
            info.Directory = baseName;
            for (int i = 2; Directory.Exists(info.Directory); i++)
            {
                info.Directory = $"{baseName}_{i}";
            }

            Directory.CreateDirectory(info.Directory);

            foreach (string file in files)
            {
                File.Copy(file, Path.Combine(info.Directory, Path.GetFileName(file)), true);
                info.Files.Add(Path.GetFileName(file));
            }

            WriteInfo(info);
            return info;
        }

        private static void WriteInfo(MapBackupInfo info)
        {
            var dom = new XmlDocument();
            dom.AppendChild(dom.CreateXmlDeclaration("1.0", "utf-8", null));
            XmlElement root = dom.CreateElement("Backup");
            root.SetAttribute("map", info.MapName ?? string.Empty);
            root.SetAttribute("fileIndex", info.FileIndex.ToString(CultureInfo.InvariantCulture));
            root.SetAttribute("created", info.Created.ToString("s", CultureInfo.InvariantCulture));
            root.SetAttribute("target", info.TargetDirectory);
            root.SetAttribute("comment", info.Comment ?? string.Empty);
            foreach (string file in info.Files)
            {
                XmlElement e = dom.CreateElement("File");
                e.SetAttribute("name", file);
                root.AppendChild(e);
            }

            dom.AppendChild(root);
            dom.Save(Path.Combine(info.Directory, BackupInfoFile));
        }

        /// <summary>
        /// All backups below <c>directory\UoFiddlerBackups</c>, newest first
        /// </summary>
        public static List<MapBackupInfo> ListBackups(string directory)
        {
            var result = new List<MapBackupInfo>();
            string root = Path.Combine(directory ?? string.Empty, BackupFolderName);
            if (!Directory.Exists(root))
            {
                return result;
            }

            foreach (string dir in Directory.GetDirectories(root))
            {
                string infoFile = Path.Combine(dir, BackupInfoFile);
                if (!File.Exists(infoFile))
                {
                    continue;
                }

                try
                {
                    var dom = new XmlDocument();
                    dom.Load(infoFile);
                    XmlElement e = dom["Backup"];
                    if (e == null)
                    {
                        continue;
                    }

                    var info = new MapBackupInfo
                    {
                        Directory = dir,
                        MapName = e.GetAttribute("map"),
                        TargetDirectory = string.IsNullOrEmpty(e.GetAttribute("target")) ? directory : e.GetAttribute("target"),
                        Comment = e.GetAttribute("comment")
                    };
                    int.TryParse(e.GetAttribute("fileIndex"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int idx);
                    info.FileIndex = idx;
                    DateTime.TryParse(e.GetAttribute("created"), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime created);
                    info.Created = created;
                    foreach (XmlElement f in e.SelectNodes("File"))
                    {
                        info.Files.Add(f.GetAttribute("name"));
                    }

                    result.Add(info);
                }
                catch (Exception)
                {
                    // broken info file, skip
                }
            }

            return result.OrderByDescending(b => b.Created).ToList();
        }

        /// <summary>
        /// Copies the files of a backup back into the target directory. The current files can be backed up first.
        /// </summary>
        public static List<string> Restore(MapBackupInfo backup, bool backupCurrentFiles)
        {
            Map.CloseAllStreams();

            List<string> targets = backup.Files.Select(f => Path.Combine(backup.TargetDirectory, f)).ToList();
            List<string> existing = targets.Where(File.Exists).ToList();
            if (backupCurrentFiles && existing.Count > 0)
            {
                CreateBackup(backup.TargetDirectory, backup.MapName, backup.FileIndex, existing, "before restore");
            }

            foreach (string file in backup.Files)
            {
                File.Copy(Path.Combine(backup.Directory, file), Path.Combine(backup.TargetDirectory, file), true);
            }

            ReloadMapsUsing(targets);
            return targets;
        }

        #endregion
    }
}

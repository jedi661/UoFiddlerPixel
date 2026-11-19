using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ultima;

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// Compares the extended items in memory with what is really written in artextidx.mul / artext.mul /
    /// tiledataext.mul on disk (loaded folder, Ultima Online folder, output folder).
    /// </summary>
    public static class ExtendedCheck
    {
        public sealed class Row
        {
            public int Id;
            public bool InMemory;
            public bool Unsaved;
            public Dictionary<string, bool> OnDisk = new Dictionary<string, bool>();
        }

        /// <summary>The folders worth checking, without duplicates (label -> folder).</summary>
        public static List<(string Label, string Folder)> GetFolders()
        {
            var list = new List<(string Label, string Folder)>();
            void Add(string label, string folder)
            {
                if (string.IsNullOrEmpty(folder))
                {
                    return;
                }

                string full;
                try
                {
                    full = Path.GetFullPath(folder).TrimEnd('\\', '/');
                }
                catch (Exception)
                {
                    return;
                }

                int existing = list.FindIndex(f => string.Equals(f.Folder, full, StringComparison.OrdinalIgnoreCase));
                if (existing >= 0)
                {
                    list[existing] = (list[existing].Label + " = " + label, full);
                }
                else
                {
                    list.Add((label, full));
                }
            }

            Add("loaded from", Art.ExtendedDataPath != null ? Path.GetDirectoryName(Art.ExtendedDataPath) : null);
            Add("Ultima Online folder", Files.RootDir);
            Add("output folder", Options.OutputPath);
            Add("last save", Art.LastExtensionSaveFolder);
            return list;
        }

        /// <summary>Full report as text plus one row per extended id (memory and every folder).</summary>
        public static string BuildReport(out List<Row> rows)
        {
            var sb = new StringBuilder();
            List<int> memory = Art.HasExtension ? Art.GetUsedExtendedIds() : new List<int>();
            List<int> unsaved = Art.HasExtension ? Art.GetUnsavedExtendedIds() : new List<int>();
            var memorySet = new HashSet<int>(memory);
            var unsavedSet = new HashSet<int>(unsaved);

            sb.AppendLine("=== MEMORY (what UOFiddler has loaded right now) ===");
            if (!Art.HasExtension)
            {
                sb.AppendLine("No extension loaded.");
            }
            else
            {
                sb.AppendLine($"Loaded from : {Art.ExtendedDataPath ?? "(created in memory, never saved)"}");
                sb.AppendLine($"Slots       : {Art.ExtendedCount:N0}  (0x{ExtendedFormat.BaseId:X} - 0x{Art.GetMaxExtendedItemId():X})");
                sb.AppendLine($"Items       : {memory.Count:N0}  {ExtendedFormat.FormatIds(memory)}");
                sb.AppendLine($"Not saved   : {unsaved.Count:N0}  {ExtendedFormat.FormatIds(unsaved)}");
                sb.AppendLine($"TileData    : {TileData.ExtendedCount:N0} extended slots");
            }

            var reports = new List<(string Label, ExtendedFormat.FolderReport Report)>();
            foreach ((string label, string folder) in GetFolders())
            {
                ExtendedFormat.FolderReport r = ExtendedFormat.Inspect(folder);
                reports.Add((label, r));

                sb.AppendLine();
                sb.AppendLine($"=== DISK: {label} ===");
                sb.AppendLine(folder);
                if (r.Error != null && !r.HasFiles)
                {
                    sb.AppendLine($"  {r.Error}");
                    continue;
                }

                sb.AppendLine(FileLine(ExtendedFormat.ArtIndexFileName, r.IndexExists, r.IndexSize, r.IndexTime));
                sb.AppendLine(FileLine(ExtendedFormat.ArtDataFileName, r.DataExists, r.DataSize, r.DataTime));
                sb.AppendLine(FileLine(ExtendedFormat.TileDataFileName, r.TileDataExists, r.TileDataSize, r.TileDataTime));
                if (r.Error != null)
                {
                    sb.AppendLine($"  ERROR: {r.Error}");
                }

                if (r.Slots >= 0)
                {
                    sb.AppendLine($"  Slots {r.Slots:N0}, items with image: {r.UsedIds.Count:N0}  {ExtendedFormat.FormatIds(r.UsedIds)}");
                }

                if (r.BrokenIds.Count > 0)
                {
                    sb.AppendLine($"  BROKEN entries: {r.BrokenIds.Count:N0}  {ExtendedFormat.FormatIds(r.BrokenIds)}");
                }

                if (r.TileSlots >= 0)
                {
                    sb.AppendLine($"  TileData slots {r.TileSlots:N0}, with name/flags: {r.NamedTileIds.Count:N0}  {ExtendedFormat.FormatIds(r.NamedTileIds)}");
                }

                if (Art.HasExtension && r.Slots >= 0)
                {
                    var disk = new HashSet<int>(r.UsedIds);
                    List<int> missing = memory.Where(id => !disk.Contains(id)).ToList();
                    List<int> extra = r.UsedIds.Where(id => !memorySet.Contains(id)).ToList();
                    if (missing.Count == 0 && extra.Count == 0)
                    {
                        sb.AppendLine($"  -> MATCHES memory ({memory.Count:N0} items)");
                    }
                    else
                    {
                        if (missing.Count > 0)
                        {
                            sb.AppendLine($"  -> in memory but NOT in this folder: {missing.Count:N0}  {ExtendedFormat.FormatIds(missing)}");
                        }

                        if (extra.Count > 0)
                        {
                            sb.AppendLine($"  -> in this folder but NOT in memory: {extra.Count:N0}  {ExtendedFormat.FormatIds(extra)}");
                        }
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("=== ALL EXTENSIONS IN MEMORY ===");
            sb.Append(UoExtensions.MemorySummary());
            foreach ((string label, string folder) in GetFolders())
            {
                sb.AppendLine();
                sb.AppendLine($"=== ALL SIDE-CAR FILES: {label} ===");
                sb.AppendLine(folder);
                if (Directory.Exists(folder))
                {
                    sb.Append(UoExtensions.DiskSummary(folder));
                }
                else
                {
                    sb.AppendLine("  folder does not exist");
                }
            }

            sb.AppendLine();
            sb.AppendLine("=== HINT ===");
            sb.AppendLine("Save writes into the output folder. UOFiddler loads from the 'loaded from' folder.");
            sb.AppendLine("If they differ, copy the files or use 'Load extension from folder...'.");

            // rows: every id that exists anywhere
            var all = new SortedSet<int>(memory);
            foreach ((string _, ExtendedFormat.FolderReport r) in reports)
            {
                all.UnionWith(r.UsedIds);
                all.UnionWith(r.BrokenIds);
            }

            rows = new List<Row>();
            foreach (int id in all)
            {
                var row = new Row { Id = id, InMemory = memorySet.Contains(id), Unsaved = unsavedSet.Contains(id) };
                foreach ((string label, ExtendedFormat.FolderReport r) in reports)
                {
                    if (r.Slots >= 0)
                    {
                        row.OnDisk[label] = r.UsedIds.Contains(id);
                    }
                }

                rows.Add(row);
            }

            return sb.ToString();
        }

        /// <summary>All side-car files (items, land tiles, textures, hues, radar colours).</summary>
        public static readonly string[] FileNames = ExtendedFormat.AllFileNames;

        /// <summary>Points the extended paths to <paramref name="folder"/> and loads the extension from there.</summary>
        /// <returns>The file names that were found in the folder.</returns>
        public static List<string> LoadFrom(string folder)
        {
            var found = new List<string>();
            foreach (string name in FileNames)
            {
                string path = Path.Combine(folder, name);
                if (File.Exists(path))
                {
                    Files.SetMulPath(path, name);
                    found.Add(name);
                }
            }

            if (found.Count > 0)
            {
                UoExtensions.ReloadAll();
            }

            return found;
        }

        /// <summary>
        /// Copies the extended files from <paramref name="sourceFolder"/> (the output folder) into the Ultima Online
        /// folder - after a dated backup of the files there - and loads them from the Ultima Online folder.
        /// Only the extended side-car files are copied, art.mul / tiledata.mul / texmaps.mul / hues.mul / radarcol.mul
        /// are not touched.
        /// </summary>
        public static string CopyToClientFolder(string sourceFolder, out List<string> copied)
        {
            copied = new List<string>();
            string target = Files.RootDir;
            if (string.IsNullOrEmpty(target) || !Directory.Exists(target))
            {
                throw new InvalidOperationException("The Ultima Online folder is not set.");
            }

            if (string.Equals(Path.GetFullPath(sourceFolder).TrimEnd('\\', '/'), Path.GetFullPath(target).TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The output folder is the Ultima Online folder - nothing to copy.");
            }

            // the backup must contain the files that are in the Ultima Online folder now (not the loaded ones)
            foreach (string name in FileNames)
            {
                string existing = Path.Combine(target, name);
                if (File.Exists(existing))
                {
                    Files.SetMulPath(existing, name);
                }
            }

            string backup = UoFileBackup.CreateBackup(Options.OutputPath, out _);

            UoExtensions.CloseAll(); // artext.mul / artlandext.mul / texmapsext.mul may be open for reading
            foreach (string name in FileNames)
            {
                string source = Path.Combine(sourceFolder, name);
                if (!File.Exists(source))
                {
                    continue;
                }

                string dest = Path.Combine(target, name);
                File.Copy(source, dest, true);
                Files.SetMulPath(dest, name);
                copied.Add(name);
            }

            UoExtensions.ReloadAll();
            return backup;
        }

        /// <summary>Short check right after saving into <paramref name="folder"/>.</summary>
        /// <returns>true if every extended item in memory is on disk there.</returns>
        public static bool VerifySaved(string folder, out string text)
        {
            if (!Art.HasExtension)
            {
                text = string.Empty;
                return true;
            }

            List<int> memory = Art.GetUsedExtendedIds();
            ExtendedFormat.FolderReport r = ExtendedFormat.Inspect(folder);
            if (r.Slots < 0)
            {
                text = $"CHECK FAILED: {ExtendedFormat.ArtIndexFileName} could not be read in {folder} ({r.Error ?? "missing"}).";
                return false;
            }

            var disk = new HashSet<int>(r.UsedIds);
            List<int> missing = memory.Where(id => !disk.Contains(id)).ToList();
            if (missing.Count == 0 && r.BrokenIds.Count == 0)
            {
                text = $"Check OK: {r.UsedIds.Count:N0} extended item(s) are on disk ({ExtendedFormat.FormatIds(r.UsedIds, 10)}).";
                return true;
            }

            text = $"CHECK PROBLEM: {missing.Count:N0} item(s) missing on disk {ExtendedFormat.FormatIds(missing, 10)}" +
                   (r.BrokenIds.Count > 0 ? $", {r.BrokenIds.Count:N0} broken entries {ExtendedFormat.FormatIds(r.BrokenIds, 10)}" : string.Empty) + ".";
            return false;
        }

        private static string FileLine(string name, bool exists, long size, DateTime time)
        {
            return exists
                ? $"  {name,-16} {size,14:N0} bytes   {time:yyyy-MM-dd HH:mm:ss}"
                : $"  {name,-16} missing";
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ultima
{
    public enum ExtIssueSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    /// <summary>What kind of extension an issue belongs to (for filtering and "go to").</summary>
    public enum ExtIssueArea
    {
        General = 0,
        Items = 1,
        LandTiles = 2,
        Textures = 3,
        Hues = 4,
        RadarColors = 5
    }

    /// <summary>Things the tool can repair automatically.</summary>
    public enum ExtIssueFix
    {
        None = 0,

        /// <summary>Sizes of art / tiledata / radar colours differ: make them as big as the art extension.</summary>
        SyncSizes = 1,

        /// <summary>Broken entry (outside the data file, bad header): remove it in memory, then save.</summary>
        RemoveEntry = 2,

        /// <summary>Unsaved changes: save the extension.</summary>
        Save = 3,

        /// <summary>Radar colour missing: take the average colour of the graphic.</summary>
        RadarFromGraphic = 4
    }

    public sealed class ExtIssue
    {
        public ExtIssueSeverity Severity;
        public ExtIssueArea Area;
        public int Id = -1;
        public string File;
        public string Message;
        public ExtIssueFix Fix;

        public override string ToString()
        {
            return $"{Severity,-7} {Area,-11} {(Id >= 0 ? $"0x{Id:X}" : "-"),-10} {File ?? "-",-20} {Message}";
        }
    }

    /// <summary>
    /// Checks the side-car files in a folder directly on disk (no GDI, no loaded data - can run in a background task):
    /// headers, pairs of index + data, every index entry (inside the data file, sane content), size consistency of
    /// art / tiledata / radar colours and texture references of the land tiledata.
    /// </summary>
    public static class ExtendedValidator
    {
        /// <summary>Max. issues per check and file that are listed one by one (the rest is summed up).</summary>
        public const int MaxListedPerCheck = 300;

        private sealed class FileSpec
        {
            public string Name;
            public string Magic;
            public int BaseId;
            public int RecordSize;
            public ExtIssueArea Area;
        }

        private static readonly FileSpec[] _specs =
        {
            new FileSpec { Name = ExtendedFormat.ArtIndexFileName, Magic = "UOAX", BaseId = ExtendedFormat.BaseId, RecordSize = ExtendedFormat.ArtIndexRecordSize, Area = ExtIssueArea.Items },
            new FileSpec { Name = ExtendedFormat.TileDataFileName, Magic = "UOTX", BaseId = ExtendedFormat.BaseId, RecordSize = ExtendedFormat.TileDataRecordSize, Area = ExtIssueArea.Items },
            new FileSpec { Name = ExtendedFormat.LandIndexFileName, Magic = ExtendedFormat.LandIndexMagic, BaseId = ExtendedFormat.LandBaseId, RecordSize = ExtendedFormat.ArtIndexRecordSize, Area = ExtIssueArea.LandTiles },
            new FileSpec { Name = ExtendedFormat.LandTileDataFileName, Magic = ExtendedFormat.LandTileMagic, BaseId = ExtendedFormat.LandBaseId, RecordSize = ExtendedFormat.LandTileRecordSize, Area = ExtIssueArea.LandTiles },
            new FileSpec { Name = ExtendedFormat.TextureIndexFileName, Magic = ExtendedFormat.TextureIndexMagic, BaseId = ExtendedFormat.TextureBaseId, RecordSize = ExtendedFormat.ArtIndexRecordSize, Area = ExtIssueArea.Textures },
            new FileSpec { Name = ExtendedFormat.HueFileName, Magic = ExtendedFormat.HueMagic, BaseId = ExtendedFormat.HueBaseId, RecordSize = ExtendedFormat.HueRecordSize, Area = ExtIssueArea.Hues },
            new FileSpec { Name = ExtendedFormat.RadarItemFileName, Magic = ExtendedFormat.RadarItemMagic, BaseId = ExtendedFormat.BaseId, RecordSize = ExtendedFormat.RadarRecordSize, Area = ExtIssueArea.RadarColors },
            new FileSpec { Name = ExtendedFormat.RadarLandFileName, Magic = ExtendedFormat.RadarLandMagic, BaseId = ExtendedFormat.LandBaseId, RecordSize = ExtendedFormat.RadarRecordSize, Area = ExtIssueArea.RadarColors }
        };

        /// <summary>
        /// Checks every side-car file in <paramref name="folder"/>. <paramref name="progress"/> gets a short text,
        /// <paramref name="cancel"/> is polled regularly.
        /// </summary>
        public static List<ExtIssue> ValidateFolder(string folder, Action<string> progress = null, Func<bool> cancel = null)
        {
            var issues = new List<ExtIssue>();
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                issues.Add(new ExtIssue { Severity = ExtIssueSeverity.Error, Message = $"Folder does not exist: {folder}" });
                return issues;
            }

            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int found = 0;

            // 1. headers
            foreach (FileSpec spec in _specs)
            {
                string path = Path.Combine(folder, spec.Name);
                if (!File.Exists(path))
                {
                    continue;
                }

                ++found;
                progress?.Invoke($"Header {spec.Name}");
                bool ok = ExtendedFormat.TryReadHeader(path, out string magic, out int baseId, out int count, out int recordSize, out string error);
                if (!ok && magic == null)
                {
                    Add(issues, ExtIssueSeverity.Error, spec.Area, spec.Name, $"Header unreadable: {error}");
                    continue;
                }

                if (magic != spec.Magic)
                {
                    Add(issues, ExtIssueSeverity.Error, spec.Area, spec.Name, $"Wrong magic '{magic}' (expected '{spec.Magic}') - the file is ignored when loading.");
                    continue;
                }

                if (baseId != spec.BaseId)
                {
                    Add(issues, ExtIssueSeverity.Error, spec.Area, spec.Name, $"Wrong base id 0x{baseId:X} (expected 0x{spec.BaseId:X}) - the file is ignored when loading.");
                    continue;
                }

                if (recordSize != spec.RecordSize)
                {
                    Add(issues, ExtIssueSeverity.Error, spec.Area, spec.Name, $"Wrong record size {recordSize} (expected {spec.RecordSize}) - the file is ignored when loading.");
                    continue;
                }

                if (!ok)
                {
                    Add(issues, ExtIssueSeverity.Error, spec.Area, spec.Name, $"Invalid header ({error}) - the file is ignored when loading.");
                    continue;
                }

                if (error != null)
                {
                    // header ok, but the file is shorter than count * recordSize
                    Add(issues, ExtIssueSeverity.Warning, spec.Area, spec.Name, error);
                }

                counts[spec.Name] = count;
            }

            if (found == 0)
            {
                Add(issues, ExtIssueSeverity.Info, ExtIssueArea.General, null, "No extension files in this folder.");
                return issues;
            }

            // 2. pairs (index without data / data without index)
            Pair(issues, folder, ExtendedFormat.ArtIndexFileName, ExtendedFormat.ArtDataFileName, ExtIssueArea.Items);
            Pair(issues, folder, ExtendedFormat.LandIndexFileName, ExtendedFormat.LandDataFileName, ExtIssueArea.LandTiles);
            Pair(issues, folder, ExtendedFormat.TextureIndexFileName, ExtendedFormat.TextureDataFileName, ExtIssueArea.Textures);

            // 3. every index entry
            if (counts.ContainsKey(ExtendedFormat.ArtIndexFileName))
            {
                CheckIndex(issues, folder, ExtendedFormat.ArtIndexFileName, ExtendedFormat.ArtDataFileName, "UOAX", ExtendedFormat.BaseId,
                    ExtIssueArea.Items, CheckStaticEntry, progress, cancel);
            }

            if (counts.ContainsKey(ExtendedFormat.LandIndexFileName))
            {
                CheckIndex(issues, folder, ExtendedFormat.LandIndexFileName, ExtendedFormat.LandDataFileName, ExtendedFormat.LandIndexMagic, ExtendedFormat.LandBaseId,
                    ExtIssueArea.LandTiles, CheckLandEntry, progress, cancel);
            }

            if (counts.ContainsKey(ExtendedFormat.TextureIndexFileName))
            {
                CheckIndex(issues, folder, ExtendedFormat.TextureIndexFileName, ExtendedFormat.TextureDataFileName, ExtendedFormat.TextureIndexMagic, ExtendedFormat.TextureBaseId,
                    ExtIssueArea.Textures, CheckTextureEntry, progress, cancel);
            }

            // 4. sizes that belong together
            SameSize(issues, counts, ExtIssueArea.Items, ExtendedFormat.ArtIndexFileName, ExtendedFormat.TileDataFileName, ExtendedFormat.RadarItemFileName);
            SameSize(issues, counts, ExtIssueArea.LandTiles, ExtendedFormat.LandIndexFileName, ExtendedFormat.LandTileDataFileName, ExtendedFormat.RadarLandFileName);

            // 5. land tiledata: texture ids must exist
            if (counts.ContainsKey(ExtendedFormat.LandTileDataFileName) && cancel?.Invoke() != true)
            {
                CheckLandTextures(issues, folder, counts, progress);
            }

            return issues;
        }

        private static void Add(List<ExtIssue> issues, ExtIssueSeverity severity, ExtIssueArea area, string file, string message,
            int id = -1, ExtIssueFix fix = ExtIssueFix.None)
        {
            issues.Add(new ExtIssue { Severity = severity, Area = area, File = file, Message = message, Id = id, Fix = fix });
        }

        private static void Pair(List<ExtIssue> issues, string folder, string index, string data, ExtIssueArea area)
        {
            bool i = File.Exists(Path.Combine(folder, index));
            bool d = File.Exists(Path.Combine(folder, data));
            if (i && !d)
            {
                Add(issues, ExtIssueSeverity.Error, area, data, $"{data} is missing ({index} exists) - no graphics can be loaded.");
            }
            else if (d && !i)
            {
                Add(issues, ExtIssueSeverity.Error, area, index, $"{index} is missing ({data} exists) - the data can not be found without the index.");
            }
        }

        private static void SameSize(List<ExtIssue> issues, Dictionary<string, int> counts, ExtIssueArea area, string main, params string[] others)
        {
            if (!counts.TryGetValue(main, out int mainCount))
            {
                return;
            }

            foreach (string other in others)
            {
                if (counts.TryGetValue(other, out int c) && c != mainCount)
                {
                    Add(issues, ExtIssueSeverity.Warning, area, other,
                        $"{c:N0} slots, but {main} has {mainCount:N0} - entries above the smaller size have no data. Fix: sync sizes and save.",
                        fix: ExtIssueFix.SyncSizes);
                }
            }
        }

        /// <summary>Checks one entry's bytes; returns null if fine, otherwise the problem.</summary>
        private delegate string EntryCheck(FileStream data, ExtendedFormat.ArtIndexEntry e, byte[] buffer);

        private static void CheckIndex(List<ExtIssue> issues, string folder, string indexName, string dataName, string magic, int baseId,
            ExtIssueArea area, EntryCheck check, Action<string> progress, Func<bool> cancel)
        {
            ExtendedFormat.ArtIndexEntry[] entries;
            try
            {
                entries = ExtendedFormat.ReadIndex(Path.Combine(folder, indexName), magic, baseId);
            }
            catch (Exception ex)
            {
                Add(issues, ExtIssueSeverity.Error, area, indexName, $"Index unreadable: {ex.Message}");
                return;
            }

            string dataPath = Path.Combine(folder, dataName);
            if (!File.Exists(dataPath))
            {
                return; // reported by Pair
            }

            int broken = 0, used = 0;
            var buffer = new byte[8];
            var offsets = new Dictionary<long, int>();
            int shared = 0;
            using var data = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1 << 16);
            long dataLength = data.Length;
            for (int k = 0; k < entries.Length; ++k)
            {
                if ((k & 0xFFFF) == 0)
                {
                    progress?.Invoke($"{indexName}: {k:N0} / {entries.Length:N0}");
                    if (cancel?.Invoke() == true)
                    {
                        Add(issues, ExtIssueSeverity.Info, area, indexName, $"Check cancelled at slot {k:N0}.");
                        return;
                    }
                }

                ExtendedFormat.ArtIndexEntry e = entries[k];
                if (e.Offset < 0 && e.Length == 0)
                {
                    continue; // empty slot
                }

                string problem = null;
                if (e.Offset < 0 || e.Length <= 0)
                {
                    problem = $"invalid entry (offset {e.Offset}, length {e.Length})";
                }
                else if (e.Offset + e.Length > dataLength)
                {
                    problem = $"points outside {dataName} (offset {e.Offset:N0} + {e.Length:N0} > {dataLength:N0} bytes)";
                }
                else
                {
                    problem = check(data, e, buffer);
                }

                if (problem != null)
                {
                    if (++broken <= MaxListedPerCheck)
                    {
                        Add(issues, ExtIssueSeverity.Error, area, indexName, problem, baseId + k, ExtIssueFix.RemoveEntry);
                    }

                    continue;
                }

                ++used;
                if (offsets.TryGetValue(e.Offset, out int _))
                {
                    ++shared; // identical data stored once - fine, only counted
                }
                else
                {
                    offsets[e.Offset] = k;
                }
            }

            if (broken > MaxListedPerCheck)
            {
                Add(issues, ExtIssueSeverity.Error, area, indexName, $"... and {broken - MaxListedPerCheck:N0} more broken entries.");
            }

            Add(issues, ExtIssueSeverity.Info, area, indexName,
                $"{entries.Length:N0} slots, {used:N0} used, {broken:N0} broken" + (shared > 0 ? $", {shared:N0} share their data with an identical entry" : string.Empty) + ".");
        }

        private static string ReadHead(FileStream data, ExtendedFormat.ArtIndexEntry e, byte[] buffer, int bytes)
        {
            data.Seek(e.Offset, SeekOrigin.Begin);
            int read = 0;
            while (read < bytes)
            {
                int n = data.Read(buffer, read, bytes - read);
                if (n <= 0)
                {
                    return "data file ends early";
                }

                read += n;
            }

            return null;
        }

        private static string CheckStaticEntry(FileStream data, ExtendedFormat.ArtIndexEntry e, byte[] buffer)
        {
            if (e.Length < 8)
            {
                return $"too short for a static image ({e.Length} bytes)";
            }

            string err = ReadHead(data, e, buffer, 8);
            if (err != null)
            {
                return err;
            }

            int width = BitConverter.ToUInt16(buffer, 4);
            int height = BitConverter.ToUInt16(buffer, 6);
            if (width < 1 || height < 1 || width > 4096 || height > 4096)
            {
                return $"bad image size {width}x{height}";
            }

            if (e.Length < 8 + (height * 2))
            {
                return $"too short for {height} rows ({e.Length} bytes)";
            }

            return null;
        }

        private static string CheckLandEntry(FileStream data, ExtendedFormat.ArtIndexEntry e, byte[] buffer)
        {
            const int landBytes = 2024; // 1012 pixels * 2
            return e.Length < landBytes ? $"land tile too short ({e.Length} bytes, expected {landBytes})" : null;
        }

        private static string CheckTextureEntry(FileStream data, ExtendedFormat.ArtIndexEntry e, byte[] buffer)
        {
            if (e.Length != 0x2000 && e.Length != 0x8000)
            {
                return $"texture length {e.Length} bytes (expected 8,192 for 64x64 or 32,768 for 128x128)";
            }

            if ((e.Length == 0x8000) != (e.Extra == 1))
            {
                return $"size flag 'extra' = {e.Extra} does not fit the length {e.Length} (0 = 64x64, 1 = 128x128)";
            }

            return null;
        }

        private static void CheckLandTextures(List<ExtIssue> issues, string folder, Dictionary<string, int> counts, Action<string> progress)
        {
            progress?.Invoke("Texture ids of the land tiledata");
            byte[] records;
            int count;
            try
            {
                records = ExtendedFormat.ReadRecords(Path.Combine(folder, ExtendedFormat.LandTileDataFileName), ExtendedFormat.LandTileMagic,
                    ExtendedFormat.LandBaseId, ExtendedFormat.LandTileRecordSize, out count);
            }
            catch (Exception ex)
            {
                Add(issues, ExtIssueSeverity.Error, ExtIssueArea.LandTiles, ExtendedFormat.LandTileDataFileName, $"Unreadable: {ex.Message}");
                return;
            }

            // highest texture id that can exist: texidx.mul in this folder (or 0x3FFF) + texture extension
            int legacyTextures = 0x4000;
            string texIdx = Path.Combine(folder, "texidx.mul");
            if (File.Exists(texIdx))
            {
                legacyTextures = (int)Math.Min(0x4000, new FileInfo(texIdx).Length / 12);
            }

            int maxTexture = counts.TryGetValue(ExtendedFormat.TextureIndexFileName, out int extTex) && extTex > 0
                ? ExtendedFormat.TextureBaseId + extTex - 1
                : legacyTextures - 1;

            int bad = 0;
            for (int k = 0; k < count; ++k)
            {
                int tex = BitConverter.ToInt32(records, (k * ExtendedFormat.LandTileRecordSize) + 8);
                bool missing = tex < 0 || tex > maxTexture || (tex >= legacyTextures && tex < ExtendedFormat.TextureBaseId);
                if (!missing)
                {
                    continue;
                }

                if (++bad <= MaxListedPerCheck)
                {
                    Add(issues, ExtIssueSeverity.Warning, ExtIssueArea.LandTiles, ExtendedFormat.LandTileDataFileName,
                        $"texture id 0x{tex:X} does not exist (highest: 0x{maxTexture:X}) - the tile is drawn without texture.", ExtendedFormat.LandBaseId + k);
                }
            }

            if (bad > MaxListedPerCheck)
            {
                Add(issues, ExtIssueSeverity.Warning, ExtIssueArea.LandTiles, ExtendedFormat.LandTileDataFileName, $"... and {bad - MaxListedPerCheck:N0} more missing textures.");
            }
        }

        /// <summary>Text report (for the clipboard / a file).</summary>
        public static string ToText(IEnumerable<ExtIssue> issues, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(title);
            sb.AppendLine(new string('=', Math.Min(100, title.Length)));
            foreach (ExtIssue i in issues)
            {
                sb.AppendLine(i.ToString());
            }

            return sb.ToString();
        }
    }
}

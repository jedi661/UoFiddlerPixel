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
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Uop
{
    public enum EcPatchAction
    {
        /// <summary>Add the entry or replace the one with the same hash.</summary>
        Replace,

        /// <summary>Remove the entry with the hash.</summary>
        Remove
    }

    /// <summary>One change of a patch: an entry of one UOP file.</summary>
    public sealed class EcPatchEntry
    {
        /// <summary>UOP file name (e.g. "tileart.uop").</summary>
        public string File { get; set; }

        public ulong Hash { get; set; }

        /// <summary>Packed name if known (only for display; the hash decides).</summary>
        public string Name { get; set; }

        public EcPatchAction Action { get; set; }
        public EcUopCompression Compression { get; set; } = EcUopCompression.Zlib;

        /// <summary>Uncompressed data (null for <see cref="EcPatchAction.Remove"/>).</summary>
        public byte[] Data { get; set; }

        public string DisplayName => Name ?? $"0x{Hash:X16}";

        internal string DataPath => $"data/{File}/{Hash:X16}.bin";
    }

    /// <summary>
    /// A patch: entries of UOP files that are added, replaced or removed in a client folder.
    /// Stored as a zip (".ecpatch"): "patch.txt" (description), "manifest.txt" (one line per entry:
    /// action|file|hash|compression|name) and the data of every entry under "data/&lt;file&gt;/&lt;hash&gt;.bin".
    /// </summary>
    public sealed class EcPatch
    {
        public const string Extension = ".ecpatch";
        private const string ManifestName = "manifest.txt";
        private const string DescriptionName = "patch.txt";

        public string Title { get; set; } = "Patch";
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.Now;

        public List<EcPatchEntry> Entries { get; } = new List<EcPatchEntry>();

        public IEnumerable<string> Files => Entries.Select(e => e.File).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(f => f, StringComparer.OrdinalIgnoreCase);

        /// <summary>Adds or replaces the entry with the same file and hash.</summary>
        public void Set(EcPatchEntry entry)
        {
            Entries.RemoveAll(e => e.Hash == entry.Hash && string.Equals(e.File, entry.File, StringComparison.OrdinalIgnoreCase));
            Entries.Add(entry);
        }

        /// <summary>Adds every new, changed and removed entry of an open package.</summary>
        public int AddChanges(EcUopPackage package, string fileName)
        {
            int count = 0;
            foreach (EcUopEntry entry in package.Entries.Where(e => e.IsModified || e.IsNew).ToList())
            {
                Set(new EcPatchEntry
                {
                    File = fileName,
                    Hash = entry.Hash,
                    Name = entry.Name,
                    Action = EcPatchAction.Replace,
                    Compression = entry.Flag == (ushort)EcUopCompression.None ? EcUopCompression.None : EcUopCompression.Zlib,
                    Data = package.ReadData(entry)
                });
                count++;
            }
            foreach (ulong hash in package.RemovedHashes.ToList())
            {
                Set(new EcPatchEntry { File = fileName, Hash = hash, Action = EcPatchAction.Remove });
                count++;
            }
            return count;
        }

        // ------------------------------------------------------------------ file

        public void Save(string path)
        {
            string temp = path + ".tmp";
            using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write))
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                var text = new StringBuilder();
                text.AppendLine("title=" + OneLine(Title));
                text.AppendLine("author=" + OneLine(Author));
                text.AppendLine("created=" + Created.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                text.AppendLine("description=" + (Description ?? string.Empty).Replace("\r\n", "\\n").Replace("\n", "\\n"));
                WriteText(zip, DescriptionName, text.ToString());

                var manifest = new StringBuilder();
                foreach (EcPatchEntry e in Entries)
                {
                    manifest.AppendLine(string.Join("|", e.Action, e.File, e.Hash.ToString("X16", CultureInfo.InvariantCulture), (int)e.Compression, e.Name ?? string.Empty));
                    if (e.Action == EcPatchAction.Replace)
                    {
                        ZipArchiveEntry z = zip.CreateEntry(e.DataPath, CompressionLevel.Optimal);
                        using (Stream s = z.Open())
                        {
                            s.Write(e.Data, 0, e.Data.Length);
                        }
                    }
                }
                WriteText(zip, ManifestName, manifest.ToString());
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(temp, path);
        }

        public static EcPatch Load(string path)
        {
            var patch = new EcPatch();
            using (ZipArchive zip = ZipFile.OpenRead(path))
            {
                string text = ReadText(zip, DescriptionName) ?? string.Empty;
                foreach (string line in text.Split('\n').Select(l => l.TrimEnd('\r')))
                {
                    int eq = line.IndexOf('=');
                    if (eq <= 0)
                    {
                        continue;
                    }
                    string key = line.Substring(0, eq), value = line.Substring(eq + 1);
                    switch (key)
                    {
                        case "title": patch.Title = value; break;
                        case "author": patch.Author = value; break;
                        case "created":
                            if (DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime created))
                            {
                                patch.Created = created;
                            }
                            break;
                        case "description": patch.Description = value.Replace("\\n", Environment.NewLine); break;
                    }
                }

                string manifest = ReadText(zip, ManifestName) ?? throw new InvalidDataException("The patch has no manifest.txt.");
                foreach (string line in manifest.Split('\n').Select(l => l.TrimEnd('\r')).Where(l => l.Length > 0))
                {
                    string[] p = line.Split(new[] { '|' }, 5);
                    if (p.Length < 4)
                    {
                        continue;
                    }
                    var e = new EcPatchEntry
                    {
                        Action = (EcPatchAction)Enum.Parse(typeof(EcPatchAction), p[0]),
                        File = p[1],
                        Hash = ulong.Parse(p[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                        Compression = (EcUopCompression)int.Parse(p[3], CultureInfo.InvariantCulture),
                        Name = p.Length > 4 && p[4].Length > 0 ? p[4] : null
                    };
                    if (e.Action == EcPatchAction.Replace)
                    {
                        ZipArchiveEntry z = zip.GetEntry(e.DataPath) ?? throw new InvalidDataException($"Data of {e.File} {e.DisplayName} is missing.");
                        using (Stream s = z.Open())
                        using (var ms = new MemoryStream())
                        {
                            s.CopyTo(ms);
                            e.Data = ms.ToArray();
                        }
                    }
                    patch.Entries.Add(e);
                }
            }
            return patch;
        }

        private static string OneLine(string text) => (text ?? string.Empty).Replace("\r", " ").Replace("\n", " ");

        private static void WriteText(ZipArchive zip, string name, string text)
        {
            using (var w = new StreamWriter(zip.CreateEntry(name).Open(), new UTF8Encoding(false)))
            {
                w.Write(text);
            }
        }

        private static string ReadText(ZipArchive zip, string name)
        {
            ZipArchiveEntry e = zip.GetEntry(name);
            if (e == null)
            {
                return null;
            }
            using (var r = new StreamReader(e.Open(), Encoding.UTF8))
            {
                return r.ReadToEnd();
            }
        }

        // ------------------------------------------------------------------ apply

        /// <summary>
        /// Applies the patch to the UOP files of a folder. <paramref name="beforeWrite"/> is called with the path of
        /// every file before it is replaced (backup), <paramref name="afterWrite"/> after it was written.
        /// Files that don't exist are created. Returns a report.
        /// </summary>
        public string Apply(string folder, Action<string> beforeWrite = null, Action<string> afterWrite = null, IProgress<int> progress = null)
        {
            var report = new StringBuilder();
            List<string> files = Files.ToList();
            int done = 0;
            foreach (string file in files)
            {
                progress?.Report(done++ * 100 / Math.Max(1, files.Count));
                string path = Path.Combine(folder, file);
                bool exists = File.Exists(path);
                int replaced = 0, added = 0, removed = 0, missing = 0;
                using (EcUopPackage package = exists ? EcUopPackage.Open(path) : EcUopPackage.CreateEmpty())
                {
                    foreach (EcPatchEntry e in Entries.Where(x => string.Equals(x.File, file, StringComparison.OrdinalIgnoreCase)))
                    {
                        EcUopEntry current = package.GetEntry(e.Hash);
                        if (e.Action == EcPatchAction.Remove)
                        {
                            if (current != null && package.Remove(current))
                            {
                                removed++;
                            }
                            else
                            {
                                missing++;
                            }
                            continue;
                        }
                        if (current != null)
                        {
                            replaced++;
                        }
                        else
                        {
                            added++;
                        }
                        package.AddOrReplace(e.Hash, e.Name, e.Data, e.Compression);
                    }
                    if (exists)
                    {
                        beforeWrite?.Invoke(path);
                    }
                    package.Save(path, false);
                }
                afterWrite?.Invoke(path);
                report.AppendLine($"{file}: {added} added, {replaced} replaced, {removed} removed" + (missing > 0 ? $", {missing} to remove were not there" : string.Empty) + (exists ? string.Empty : " (new file)"));
            }
            progress?.Report(100);
            return report.ToString();
        }
    }
}

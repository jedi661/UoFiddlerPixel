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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UoFiddler.Plugin.Enhanced.EcUltima.Imaging;
using UoFiddler.Plugin.Enhanced.EcUltima.Strings;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Analysis
{
    public sealed class EcAnalyzerOptions
    {
        /// <summary>Entries per file whose data is read to find out the content types (spread over the file).</summary>
        public int ClassifyEntriesPerFile { get; set; } = 1500;

        /// <summary>Sample entries per file and content type copied into the zip.</summary>
        public int SamplesPerType { get; set; } = 4;

        /// <summary>Maximum size of one sample.</summary>
        public int MaxSampleBytes { get; set; } = 512 * 1024;

        /// <summary>Maximum size of all samples of one UOP file.</summary>
        public long MaxSampleBytesPerFile { get; set; } = 3 * 1024 * 1024;

        /// <summary>Also test name patterns (takes most of the time).</summary>
        public bool GuessNames { get; set; } = true;

        /// <summary>Only these files (null = every *.uop of the folder).</summary>
        public IList<string> Files { get; set; }
    }

    /// <summary>
    /// Writes a report (text + sample entries, as zip) about every UOP file of an Enhanced Client folder.
    /// The report is meant to be sent to the developer to add editors for unknown files.
    /// </summary>
    public sealed class EcFolderAnalyzer
    {
        private static readonly string[] _extensions = { "bin", "dds", "tga", "dat", "xml", "nif", "txt", "lua", "wav", "ogg", "mp3", "png" };
        private static readonly string[] _formats = { "{0}", "{0:D2}", "{0:D3}", "{0:D4}", "{0:D5}", "{0:D6}", "{0:D7}", "{0:D8}" };

        private readonly string _folder;
        private readonly EcAnalyzerOptions _options;
        private readonly StringBuilder _report = new StringBuilder();
        private EcStringDictionary _strings;

        public EcFolderAnalyzer(string folder, EcAnalyzerOptions options = null)
        {
            _folder = folder;
            _options = options ?? new EcAnalyzerOptions();
        }

        /// <summary>
        /// Analyzes the folder and writes the zip. Progress reports "percent|text".
        /// </summary>
        public void Run(string zipPath, IProgress<string> progress = null, CancellationToken cancel = default)
        {
            using (var zipStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                Line("Enhanced Client folder analysis");
                Line($"Created: {DateTime.Now:yyyy-MM-dd HH:mm}   Plugin: UoFiddler.Plugin.Enhanced");
                Line($"Folder:  {_folder}");
                Line();

                List<string> files = (_options.Files ?? Directory.EnumerateFiles(_folder).Where(f => f.EndsWith(".uop", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".myp", StringComparison.OrdinalIgnoreCase)).Select(Path.GetFileName).ToList())
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToList();

                LoadStringDictionary();
                WriteFolderOverview(zip);

                for (int i = 0; i < files.Count; i++)
                {
                    cancel.ThrowIfCancellationRequested();
                    progress?.Report($"{i * 100 / Math.Max(1, files.Count)}|{files[i]} ({i + 1}/{files.Count})");
                    try
                    {
                        AnalyzeFile(files[i], zip, cancel);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Line($"!! {files[i]}: {ex.GetType().Name}: {ex.Message}");
                        Line();
                    }
                }

                progress?.Report("100|writing report");
                ZipArchiveEntry reportEntry = zip.CreateEntry("report.txt", CompressionLevel.Optimal);
                using (var writer = new StreamWriter(reportEntry.Open(), new UTF8Encoding(false)))
                {
                    writer.Write(_report.ToString());
                }
            }
        }

        public string ReportText => _report.ToString();

        private void Line(string text = "") => _report.AppendLine(text);

        private void LoadStringDictionary()
        {
            string path = Path.Combine(_folder, "string_dictionary.uop");
            if (!File.Exists(path))
            {
                return;
            }
            try
            {
                using (EcUopPackage package = EcUopPackage.Open(path))
                {
                    _strings = EcStringDictionary.Load(package);
                }
            }
            catch (Exception)
            {
                _strings = null;
            }
        }

        // ------------------------------------------------------------------ other files

        private void WriteFolderOverview(ZipArchive zip)
        {
            Line("=== Files in the folder ===");
            var other = new List<string>();
            foreach (string file in Directory.EnumerateFiles(_folder).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                var info = new FileInfo(file);
                string kind = Path.GetExtension(file).ToLowerInvariant();
                Line($"  {info.Name,-40} {info.Length,14:N0} bytes  {info.LastWriteTime:yyyy-MM-dd}");
                if (kind != ".uop" && kind != ".myp")
                {
                    other.Add(file);
                }
            }
            Line();

            if (other.Count == 0)
            {
                return;
            }

            Line("=== Other files (not UOP) ===");
            foreach (string file in other)
            {
                var info = new FileInfo(file);
                string kind = Path.GetExtension(file).ToLowerInvariant();
                string description;
                try
                {
                    byte[] head = ReadHead(file, 256);
                    description = Describe(head, info.Name);
                    if (DdsImage.IsDds(head))
                    {
                        DdsImage.TryReadInfo(head, out int w, out int h, out string format, out int mips);
                        description = $"DDS {w} x {h} {format}, mips {mips}";
                    }
                    Line($"  {info.Name}: {description}");
                    Line($"    first bytes: {Hex(head, 0, 48)}");

                    // small non-UOP files (and dds previews) go into the zip
                    bool program = kind == ".exe" || kind == ".dll" || (head.Length > 2 && head[0] == 'M' && head[1] == 'Z');
                    if (!program && info.Length <= _options.MaxSampleBytes * 4L)
                    {
                        zip.CreateEntryFromFile(file, "other_files/" + info.Name, CompressionLevel.Optimal);
                        Line("    (copied into other_files/)");
                    }
                }
                catch (Exception ex)
                {
                    Line($"  {info.Name}: {ex.Message}");
                }
            }
            Line();
        }

        private static byte[] ReadHead(string file, int count)
        {
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[Math.Min(count, fs.Length)];
                int read = 0;
                while (read < buffer.Length)
                {
                    int n = fs.Read(buffer, read, buffer.Length - read);
                    if (n <= 0)
                    {
                        break;
                    }
                    read += n;
                }
                return buffer;
            }
        }

        // ------------------------------------------------------------------ one UOP file

        private sealed class Sample
        {
            public EcUopEntry Entry;
            public byte[] Data;
            public string Type;
        }

        private void AnalyzeFile(string fileName, ZipArchive zip, CancellationToken cancel)
        {
            string path = Path.Combine(_folder, fileName);
            var info = new FileInfo(path);

            Line(new string('=', 100));
            Line($"FILE {fileName}   {info.Length:N0} bytes");

            using (EcUopPackage package = EcUopPackage.Open(path))
            {
                IReadOnlyList<EcUopEntry> entries = package.Entries;
                Line($"  MYP version {package.Version}, signature 0x{package.Signature:X8}, block capacity {package.BlockCapacity}, " +
                     $"files in header {package.DeclaredFileCount}, entries read {entries.Count}");

                if (entries.Count == 0)
                {
                    Line();
                    return;
                }

                if (package.HasSignedEntries)
                {
                    Line("  NOTE: MYP version 5 – entries carry a large metadata block (probably a signature); changed entries can't be signed.");
                }

                // ---- table of contents statistics
                var flags = entries.GroupBy(e => e.Flag).OrderBy(g => g.Key).Select(g => $"flag {g.Key}: {g.Count()}");
                Line($"  Compression: {string.Join(", ", flags)}   (0 = none, 1 = zlib, other = unknown)");
                var sizes = entries.Select(e => (long)e.DecompressedSize).OrderBy(s => s).ToList();
                Line($"  Entry size: min {sizes[0]:N0}, median {sizes[sizes.Count / 2]:N0}, max {sizes[sizes.Count - 1]:N0}, total {sizes.Sum():N0} bytes");
                var headerSizes = entries.GroupBy(e => e.HeaderSize).Select(g => $"{g.Key} ({g.Count()})");
                Line($"  Metadata header size: {string.Join(", ", headerSizes)}");
                EcUopEntry firstWithHeader = entries.FirstOrDefault(e => e.HeaderSize > 0);
                if (firstWithHeader != null)
                {
                    byte[] meta = ReadMetadata(path, firstWithHeader);
                    Line($"  Metadata header example: {Hex(meta, 0, meta.Length)}");
                }

                // ---- names
                var knownHits = new List<string>();
                foreach (EcNamePattern pattern in EcClient.KnownPatterns.Where(p => string.Equals(p.FileName, fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    int hits = pattern.LastSecond > 0
                        ? package.ResolveNames2D(pattern.Pattern, pattern.FirstId, pattern.LastId, pattern.LastSecond)
                        : package.ResolveNames(pattern.Pattern, pattern.FirstId, pattern.LastId);
                    knownHits.Add($"{pattern.Pattern}: {hits}");
                }

                if (string.Equals(fileName, EcClient.EffectTextureFileName, StringComparison.OrdinalIgnoreCase))
                {
                    using (var client = new EcClient(_folder))
                    {
                        client.ResolveEffectNames(package);
                    }
                    knownHits.Add("data/effects/<names from string_dictionary and particle scripts>");
                }

                // ---- read a spread of entries
                int step = Math.Max(1, entries.Count / Math.Max(1, _options.ClassifyEntriesPerFile));
                var samples = new List<Sample>();
                int unreadable = 0;
                for (int i = 0; i < entries.Count; i += step)
                {
                    cancel.ThrowIfCancellationRequested();
                    EcUopEntry entry = entries[i];
                    if (!entry.IsSupported)
                    {
                        unreadable++;
                        continue;
                    }
                    try
                    {
                        byte[] data = package.ReadData(entry);
                        samples.Add(new Sample { Entry = entry, Data = data, Type = Describe(data, entry.Name) });
                    }
                    catch (Exception)
                    {
                        unreadable++;
                    }
                }

                int fromDictionary = EcClient.NameDictionary?.Apply(package) ?? 0;
                if (fromDictionary > 0)
                {
                    knownHits.Add($"names from the loaded name dictionary: {fromDictionary}");
                }

                if (_options.GuessNames)
                {
                    GuessNamePatterns(package, fileName, samples, knownHits, cancel);
                    int fromContents = EcContentNameResolver.Resolve(package, fileName, cancel);
                    knownHits.Add($"names from the contents (paths inside xml/mod/lua/csv/text entries): {fromContents}");
                }

                int named = entries.Count(e => e.Name != null);
                Line($"  Names found: {named} of {entries.Count}");
                foreach (string hit in knownHits)
                {
                    Line($"    pattern {hit}");
                }
                foreach (string example in entries.Where(e => e.Name != null).Select(e => e.Name).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).Take(8))
                {
                    Line($"    e.g. {example}");
                }

                // ---- content types
                Line($"  Content (read {samples.Count} of {entries.Count} entries" + (unreadable > 0 ? $", {unreadable} unreadable" : string.Empty) + "):");
                foreach (var group in samples.GroupBy(s => TypeKey(s.Type)).OrderByDescending(g => g.Count()))
                {
                    Line($"    {group.Count(),6} x {group.Key}");
                    foreach (string detail in group.Select(s => s.Type).Distinct().Take(6).Where(t => t != group.Key))
                    {
                        Line($"             {detail}");
                    }
                }

                if (unreadable > 0)
                {
                    EcUopEntry bad = entries.FirstOrDefault(e => !e.IsSupported) ?? entries[0];
                    Line($"  Unknown compression example: flag {bad.Flag}, packed {bad.CompressedSize}, size {bad.DecompressedSize}, raw: {Hex(package.ReadRaw(bad), 0, 48)}");
                }

                // ---- binary structure hints
                List<Sample> binaries = samples.Where(s => TypeKey(s.Type) == "binary").ToList();
                if (binaries.Count > 0)
                {
                    WriteBinaryHints(binaries);
                }

                // ---- string dictionary references
                if (_strings != null && binaries.Count > 0)
                {
                    WriteDictionaryHints(binaries);
                }

                // ---- samples and the complete table of contents into the zip
                WriteSamples(fileName, samples, zip);
                WriteToc(fileName, package, zip);
            }

            Line();
        }

        private static byte[] ReadMetadata(string path, EcUopEntry entry)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fs.Seek(entry.Offset, SeekOrigin.Begin);
                var buffer = new byte[Math.Min(64, entry.HeaderSize)];
                fs.ReadExactly(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        private void GuessNamePatterns(EcUopPackage package, string fileName, List<Sample> samples, List<string> hits, CancellationToken cancel)
        {
            if (package.Entries.All(e => e.Name != null))
            {
                return;
            }

            // folder candidates from the file name: "gumpartLegacyMUL.uop" -> gumpartlegacymul, gumpart, ...
            string baseName = Path.GetFileNameWithoutExtension(fileName).ToLowerInvariant();
            var folders = new HashSet<string> { baseName };
            string stripped = Regex.Replace(baseName, "(legacymul|legacy|collection|definition|\\d+x?)$", string.Empty);
            if (stripped.Length > 0)
            {
                folders.Add(stripped);
                folders.Add(stripped + "s");
                folders.Add(stripped + "legacy");
                folders.Add(stripped + "definition");
                folders.Add(stripped + "collection");
            }
            if (baseName.EndsWith("s"))
            {
                folders.Add(baseName.TrimEnd('s'));
            }

            var keys = new HashSet<ulong>(package.Entries.Where(e => e.Name == null).Select(e => e.Hash));
            var candidates = new List<string>();
            foreach (string folder in folders)
            {
                foreach (string format in _formats)
                {
                    foreach (string ext in _extensions)
                    {
                        candidates.Add($"build/{folder}/{format}.{ext}");
                    }
                }
            }

            // quick test with small ids, full range only for patterns that hit
            var found = new List<string>();
            Parallel.ForEach(candidates, new ParallelOptions { CancellationToken = cancel }, pattern =>
            {
                for (int id = 0; id < 3000; id++)
                {
                    if (keys.Contains(EcUopHash.HashFileName(string.Format(CultureInfo.InvariantCulture, pattern, id))))
                    {
                        lock (found)
                        {
                            found.Add(pattern);
                        }
                        return;
                    }
                }
            });

            // ids stored at the start of the data are tried as well
            var dataIds = new HashSet<int>();
            foreach (Sample s in samples.Where(s => s.Data.Length >= 4))
            {
                dataIds.Add(BitConverter.ToInt32(s.Data, 0) & int.MaxValue);
            }

            foreach (string pattern in found)
            {
                int count = package.ResolveNames(pattern, 0, 0x40000) + package.ResolveNames(pattern, dataIds.Where(i => i > 0x40000));
                hits.Add($"{pattern}: {count} (guessed)");
            }
        }

        private void WriteBinaryHints(List<Sample> binaries)
        {
            Line("  Binary data hints:");
            var lengths = binaries.GroupBy(s => s.Data.Length).OrderByDescending(g => g.Count()).Take(8).Select(g => $"{g.Key} ({g.Count()})");
            Line($"    most common lengths: {string.Join(", ", lengths)}");
            var heads = binaries.Where(s => s.Data.Length >= 4).GroupBy(s => Hex(s.Data, 0, 4)).OrderByDescending(g => g.Count()).Take(6).Select(g => $"{g.Key} ({g.Count()})");
            Line($"    most common first 4 bytes: {string.Join(", ", heads)}");

            // does the first uint match the id of the name?
            int withId = 0, matching = 0;
            foreach (Sample s in binaries.Where(s => s.Data.Length >= 4 && s.Entry.Name != null))
            {
                if (EcClient.TryGetIdFromName(s.Entry.Name, out int id))
                {
                    withId++;
                    if (BitConverter.ToInt32(s.Data, 0) == id)
                    {
                        matching++;
                    }
                }
            }
            if (withId > 0)
            {
                Line($"    first uint = id of the name: {matching} of {withId}");
            }

            // constant / varying bytes over the first 64 bytes of entries with the most common length
            IGrouping<int, Sample> common = binaries.GroupBy(s => s.Data.Length).OrderByDescending(g => g.Count()).First();
            if (common.Count() >= 3)
            {
                int length = Math.Min(64, common.Key);
                var map = new StringBuilder();
                for (int i = 0; i < length; i++)
                {
                    byte first = common.First().Data[i];
                    map.Append(common.All(s => s.Data[i] == first) ? first.ToString("X2") : "..");
                    map.Append(i % 4 == 3 ? "  " : " ");
                }
                Line($"    entries with length {common.Key}: constant bytes (.. = varies): {map}");
            }

            Line("    examples:");
            foreach (Sample s in binaries.OrderBy(s => s.Data.Length).Where((s, i) => i % Math.Max(1, binaries.Count / 5) == 0).Take(6))
            {
                Line($"      {s.Entry.DisplayName} ({s.Data.Length} bytes): {Hex(s.Data, 0, 64)}");
            }
        }

        /// <summary>
        /// Kind of a dictionary text: file extension (".nif", ".tga", ...) or "name".
        /// </summary>
        private static string TextKind(string text)
        {
            int dot = text.LastIndexOf('.');
            if (dot > 0 && text.Length - dot <= 5)
            {
                return text.Substring(dot).ToLowerInvariant();
            }
            return "name";
        }

        private void WriteDictionaryHints(List<Sample> binaries)
        {
            // How the texts of the dictionary are distributed (most are .tga paths, so random numbers mostly hit those).
            var global = _strings.Strings.GroupBy(TextKind).ToDictionary(g => g.Key, g => (double)g.Count() / _strings.Count);

            List<Sample> checkedSamples = binaries.Take(300).ToList();
            var results = new List<string>();
            int maxOffset = Math.Min(128, checkedSamples.Min(s => s.Data.Length));
            for (int offset = 0; offset + 4 <= maxOffset; offset++)
            {
                var values = checkedSamples.Select(s => BitConverter.ToUInt32(s.Data, offset)).ToList();
                var valid = values.Where(v => v >= 1 && v <= _strings.Count).ToList();
                if (valid.Count < Math.Max(3, values.Count * 0.8) || valid.Distinct().Count() < valid.Count * 0.3)
                {
                    continue;
                }

                // the texts must be of one kind that is rare in the dictionary
                var kinds = valid.GroupBy(v => TextKind(_strings.Get(v))).OrderByDescending(g => g.Count()).First();
                double share = (double)kinds.Count() / valid.Count;
                double globalShare = global.TryGetValue(kinds.Key, out double g) ? g : 0;
                if (share >= 0.8 && globalShare < 0.3)
                {
                    uint example = kinds.First();
                    results.Add($"offset {offset}: {valid.Count} of {values.Count} entries, kind '{kinds.Key}', e.g. #{example} = \"{_strings.Get(example)}\"");
                }
            }

            if (results.Count > 0)
            {
                Line("  Offsets that look like string_dictionary references:");
                foreach (string r in results.Take(8))
                {
                    Line($"    {r}");
                }
            }
        }

        /// <summary>
        /// All entries (hash, name, sizes) as csv, so file names can be searched for without the file itself.
        /// </summary>
        private void WriteToc(string fileName, EcUopPackage package, ZipArchive zip)
        {
            ZipArchiveEntry entry = zip.CreateEntry($"toc/{fileName}.csv", CompressionLevel.Optimal);
            using (var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
            {
                writer.WriteLine("hash;name;flag;size;packed;header");
                foreach (EcUopEntry e in package.Entries)
                {
                    writer.WriteLine($"{e.Hash:X16};{e.Name};{e.Flag};{e.DecompressedSize};{e.CompressedSize};{e.HeaderSize}");
                }
            }
            Line($"  Table of contents: toc/{fileName}.csv ({package.Count} entries)");
        }

        private void WriteSamples(string fileName, List<Sample> samples, ZipArchive zip)
        {
            long written = 0;
            int count = 0;
            foreach (var group in samples.GroupBy(s => TypeKey(s.Type)))
            {
                // smallest first, plus one medium sized entry
                List<Sample> ordered = group.OrderBy(s => s.Data.Length).ToList();
                var chosen = ordered.Take(Math.Max(1, _options.SamplesPerType - 1)).ToList();
                if (ordered.Count > chosen.Count)
                {
                    chosen.Add(ordered[ordered.Count / 2]);
                }

                foreach (Sample s in chosen.Distinct())
                {
                    if (s.Data.Length > _options.MaxSampleBytes || written + s.Data.Length > _options.MaxSampleBytesPerFile)
                    {
                        continue;
                    }

                    string name = s.Entry.Name != null
                        ? s.Entry.Name.Replace('\\', '/')
                        : $"unknown/{s.Entry.Hash:X16}{GuessExtension(s.Data)}";
                    ZipArchiveEntry zipEntry = zip.CreateEntry($"samples/{fileName}/{name}", CompressionLevel.Optimal);
                    using (Stream stream = zipEntry.Open())
                    {
                        stream.Write(s.Data, 0, s.Data.Length);
                    }
                    written += s.Data.Length;
                    count++;
                }
            }
            Line($"  Samples in zip: {count} entries, {written:N0} bytes (samples/{fileName}/)");
        }

        // ------------------------------------------------------------------ content detection

        private static string TypeKey(string description)
        {
            int p = description.IndexOf(' ');
            string first = p > 0 ? description.Substring(0, p) : description;
            return first switch
            {
                "DDS" => "DDS image",
                "TGA" => "TGA image",
                _ => description.StartsWith("text", StringComparison.Ordinal) ? "text" : description
            };
        }

        public static string Describe(byte[] data, string name = null)
        {
            if (data == null || data.Length == 0)
            {
                return "empty";
            }

            string Ascii(int count) => Encoding.ASCII.GetString(data, 0, Math.Min(count, data.Length));

            if (DdsImage.IsDds(data))
            {
                DdsImage.TryReadInfo(data, out int w, out int h, out string format, out int mips);
                return $"DDS {format} {w}x{h} mips {mips}";
            }
            if (data.Length > 8 && data[0] == 0x89 && Ascii(4).EndsWith("PNG", StringComparison.Ordinal))
            {
                return "PNG image";
            }
            if (data.Length > 2 && data[0] == 0xFF && data[1] == 0xD8)
            {
                return "JPEG image";
            }
            if (Ascii(2) == "BM")
            {
                return "BMP image";
            }
            if (Ascii(8) == "Gamebryo")
            {
                return "Gamebryo NIF (3D model)";
            }
            if (Ascii(4) == "8BPS")
            {
                return "PSD image";
            }
            if (Ascii(4) == "RIFF")
            {
                return "RIFF " + Ascii(12).Substring(Math.Min(8, data.Length));
            }
            if (Ascii(4) == "OggS")
            {
                return "OGG audio";
            }
            if (Ascii(3) == "ID3")
            {
                return "MP3 audio";
            }
            if (data.Length > 4 && data[0] == 0x1B && Ascii(4).EndsWith("Lua", StringComparison.Ordinal))
            {
                return "compiled Lua";
            }
            if (data.Length > 2 && data[0] == 0x78 && (data[1] == 0x9C || data[1] == 0xDA || data[1] == 0x01))
            {
                return "zlib stream (compressed again)";
            }
            if (Ascii(2) == "PK")
            {
                return "ZIP archive";
            }
            if (name?.EndsWith(".tga", StringComparison.OrdinalIgnoreCase) == true ? TgaImage.LooksLikeTga(data) : TgaImage.IsLikelyTga(data))
            {
                return $"TGA {BitConverter.ToUInt16(data, 12)}x{BitConverter.ToUInt16(data, 14)} {data[16]} bit type {data[2]}";
            }
            if (IsText(data))
            {
                string head = Ascii(40).Replace("\r", " ").Replace("\n", " ").Replace("\t", " ").Trim();
                if (head.StartsWith("<", StringComparison.Ordinal))
                {
                    return "text (XML) " + head;
                }
                return "text " + head;
            }
            return "binary";
        }

        private static bool IsText(byte[] data)
        {
            int length = Math.Min(data.Length, 4096);
            int printable = 0;
            for (int i = 0; i < length; i++)
            {
                byte b = data[i];
                if (b == 0)
                {
                    return false;
                }
                if (b == 9 || b == 10 || b == 13 || (b >= 32 && b < 127) || b >= 160)
                {
                    printable++;
                }
            }
            return length > 0 && printable >= length * 0.95;
        }

        private static string GuessExtension(byte[] data)
        {
            string d = Describe(data);
            if (d.StartsWith("DDS", StringComparison.Ordinal)) return ".dds";
            if (d.StartsWith("TGA", StringComparison.Ordinal)) return ".tga";
            if (d.StartsWith("PNG", StringComparison.Ordinal)) return ".png";
            if (d.StartsWith("Gamebryo", StringComparison.Ordinal)) return ".nif";
            if (d.StartsWith("text (XML)", StringComparison.Ordinal)) return ".xml";
            if (d.StartsWith("text", StringComparison.Ordinal)) return ".txt";
            if (d.StartsWith("RIFF", StringComparison.Ordinal)) return ".wav";
            if (d.StartsWith("OGG", StringComparison.Ordinal)) return ".ogg";
            if (d.StartsWith("MP3", StringComparison.Ordinal)) return ".mp3";
            return ".bin";
        }

        private static string Hex(byte[] data, int offset, int count)
        {
            var sb = new StringBuilder();
            for (int i = offset; i < Math.Min(data.Length, offset + count); i++)
            {
                sb.Append(data[i].ToString("X2"));
                sb.Append((i - offset) % 4 == 3 ? "  " : " ");
            }
            return sb.ToString().TrimEnd();
        }
    }
}

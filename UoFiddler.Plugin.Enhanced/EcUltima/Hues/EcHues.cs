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
using System.Text;
using UoFiddler.Plugin.Enhanced.EcUltima.Imaging;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Hues
{
    public sealed class EcHue
    {
        public int Number { get; internal set; }
        public EcUopEntry Entry { get; internal set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Hues.uop:
    /// <list type="bullet">
    /// <item>data/definitions/hues/hue####.bmp – 256 x 2 pixel 32 bit ramp per hue (animations, gumps, hue picker)</item>
    /// <item>data/definitions/hues/huenames.csv – "number,name"</item>
    /// <item>build/hues/hues.dds – one row per hue (colors the art tiles)</item>
    /// <item>unnamed entry 0xFA5C6A1BC0D8B01B – 64 x 64 overview, one pixel per hue</item>
    /// <item>build/hues/classichues.bin – optional, written by this plugin: the exact classic colors (for a lossless way back)</item>
    /// </list>
    /// Every change of a ramp also updates hues.dds and the overview, so items, animations and the picker always show the same hue.
    /// </summary>
    public sealed class EcHues
    {
        public const string NamePattern = "data/definitions/hues/hue{0:D4}.bmp";
        public const string NamesName = "data/definitions/hues/huenames.csv";
        public const string DdsName = "build/hues/hues.dds";
        public const ulong OverviewHash = 0xFA5C6A1BC0D8B01B;
        public const int DdsSize = 1024;
        public const int OverviewSize = 64;

        /// <summary>Highest hue that has a row in hues.dds (4 columns of 1024 rows, row 0 of column 0 is "no hue").</summary>
        public const int MaxAtlasHue = ((DdsSize / EcHueRender.RampWidth) * DdsSize) - 1;

        private readonly SortedDictionary<int, EcHue> _hues = new SortedDictionary<int, EcHue>();
        private readonly Dictionary<int, HueDecodeResult> _decoded = new Dictionary<int, HueDecodeResult>();
        private readonly Dictionary<int, (byte[] Row, byte[] Pixel)> _atlasChanges = new Dictionary<int, (byte[], byte[])>();
        private EcUopEntry _nameEntry;
        private bool _namesChanged;
        private bool _namesEndWithNewLine = true;
        private readonly SortedDictionary<int, string> _nameLines = new SortedDictionary<int, string>();
        private readonly HashSet<int> _changedNames = new HashSet<int>();
        private bool _exactChanged;
        private string _newLine = "\r\n";
        private EcHueExactData _exact;
        private HueRenderOptions _options;
        private DdsImage _dds;
        private bool _ddsRead;

        public EcUopPackage Package { get; }
        public IEnumerable<EcHue> Hues => _hues.Values;
        public int Count => _hues.Count;

        public EcUopEntry DdsEntry => Package.GetEntry(DdsName);
        public EcUopEntry OverviewEntry => Package.GetEntry(OverviewHash);
        public EcUopEntry ExactEntry => Package.GetEntry(EcHueExactData.EntryName);
        public EcUopEntry NamesEntry => _nameEntry;

        /// <summary>True when the exact classic colors are stored in the package (and kept up to date).</summary>
        public bool StoreExactData { get; set; }

        /// <summary>True when the settings come from stored data or were found on the hue bitmaps.</summary>
        public bool OptionsDetected { get; private set; }

        /// <summary>Number of samples that were drawn back pixel exact while detecting the settings.</summary>
        public int DetectedExactSamples { get; private set; }

        public int DetectedSamples { get; private set; }

        public EcHues(EcUopPackage package)
        {
            Package = package;
            package.ResolveNames(NamePattern, 0, 9999);
            package.ResolveName(NamesName);
            package.ResolveName(DdsName);
            package.ResolveName(EcHueExactData.EntryName);

            foreach (EcUopEntry entry in package.Entries)
            {
                if (entry.Name != null && entry.Name.StartsWith("data/definitions/hues/hue", StringComparison.OrdinalIgnoreCase) &&
                    entry.Name.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(Path.GetFileNameWithoutExtension(entry.Name).Substring(3), NumberStyles.Integer, CultureInfo.InvariantCulture, out int number))
                {
                    _hues[number] = new EcHue { Number = number, Entry = entry };
                }
            }

            // the name list is the text entry starting with "1,"
            _nameEntry = package.GetEntry(NamesName);
            if (_nameEntry == null)
            {
                foreach (EcUopEntry entry in package.Entries.Where(e => e.DecompressedSize < 1_000_000 && (e.Name == null || e.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || e.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))))
                {
                    byte[] head = package.ReadDataHead(entry, 16);
                    if (head.Length > 2 && head[0] == (byte)'1' && head[1] == (byte)',')
                    {
                        _nameEntry = entry;
                        break;
                    }
                }
            }
            if (_nameEntry != null)
            {
                string names = Encoding.UTF8.GetString(package.ReadData(_nameEntry));
                _newLine = names.Contains("\r\n") ? "\r\n" : "\n";
                ReadNames(names);
            }

            EcUopEntry exact = ExactEntry;
            if (exact != null)
            {
                _exact = EcHueExactData.FromBytes(package.ReadData(exact));
                StoreExactData = _exact != null;
            }
        }

        private void ReadNames(string text)
        {
            _namesEndWithNewLine = text.EndsWith("\n", StringComparison.Ordinal);
            foreach (string line in text.Split('\n'))
            {
                string l = line.TrimEnd('\r');
                if (l.Length == 0)
                {
                    continue;
                }
                int comma = l.IndexOf(',');
                string numberText = comma < 0 ? l : l.Substring(0, comma);
                if (!int.TryParse(numberText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number))
                {
                    continue;
                }
                _nameLines[number] = l;
                if (comma > 0 && _hues.TryGetValue(number, out EcHue hue))
                {
                    hue.Name = l.Substring(comma + 1);
                }
            }
        }

        public EcHue Get(int number) => _hues.TryGetValue(number, out EcHue hue) ? hue : null;

        public int MaxNumber => _hues.Count == 0 ? 0 : _hues.Keys.Max();

        // ------------------------------------------------------------------ drawing settings

        /// <summary>
        /// The settings the ramps of this file are drawn with: from the stored classic data, else found on the bitmaps
        /// (the first call checks 64 hues), else the huesmul2uop defaults.
        /// </summary>
        public HueRenderOptions Options
        {
            get
            {
                if (_options != null)
                {
                    return _options;
                }
                if (_exact != null)
                {
                    _options = _exact.Options.Clone();
                    OptionsDetected = true;
                    return _options;
                }
                var samples = new List<(byte[], byte[])>();
                List<EcHue> list = _hues.Values.ToList();
                int step = Math.Max(1, list.Count / 64);
                for (int i = 0; i < list.Count && samples.Count < 64; i += step)
                {
                    byte[] px = GetRamp(list[i], out int w, out int h);
                    if (w == EcHueRender.RampWidth && h >= 2)
                    {
                        samples.Add((px, GetDdsRow(list[i].Number)));
                    }
                }
                DetectedSamples = samples.Count;
                _options = EcHueDecoder.Detect(samples, out int exactSamples);
                DetectedExactSamples = exactSamples;
                OptionsDetected = samples.Count > 0;
                return _options;
            }
        }

        /// <summary>Sets the settings (for a new or completely rebuilt file).</summary>
        public void SetOptions(HueRenderOptions options)
        {
            _options = options.Clone();
            OptionsDetected = true;
            _decoded.Clear();
            if (_exact != null)
            {
                _exact.Options = _options.Clone();
                _exactChanged = true;
            }
        }

        // ------------------------------------------------------------------ reading

        /// <summary>The ramp pixels (BGRA, rows top to bottom).</summary>
        public byte[] GetRamp(EcHue hue, out int width, out int height)
        {
            byte[] bmp = Package.ReadData(hue.Entry);
            return ReadBmp(bmp, out width, out height);
        }

        /// <summary>The row of the hue in hues.dds (256 BGRA pixels), null when there is none.</summary>
        public byte[] GetDdsRow(int number)
        {
            if (_atlasChanges.TryGetValue(number, out (byte[] Row, byte[] Pixel) change))
            {
                return change.Row;
            }
            DdsImage dds = ReadDds();
            if (dds == null || dds.Width != DdsSize || dds.Height != DdsSize || number < 0 || number > MaxAtlasHue)
            {
                return null;
            }
            var row = new byte[EcHueRender.RampWidth * 4];
            int x0 = number / DdsSize * EcHueRender.RampWidth;
            int y = number % DdsSize;
            Buffer.BlockCopy(dds.Pixels, ((y * DdsSize) + x0) * 4, row, 0, row.Length);
            return row;
        }

        /// <summary>The pixel of the hue in the 64 x 64 overview (BGRA), null when there is none.</summary>
        public byte[] GetOverviewPixel(int number)
        {
            if (_atlasChanges.TryGetValue(number, out (byte[] Row, byte[] Pixel) change))
            {
                return change.Pixel;
            }
            if (!_overviewRead)
            {
                _overviewRead = true;
                try
                {
                    EcUopEntry entry = OverviewEntry;
                    _overview = entry == null ? null : DdsImage.Decode(Package.ReadData(entry));
                }
                catch (Exception)
                {
                    _overview = null;
                }
            }
            if (_overview == null || _overview.Width != OverviewSize || _overview.Height != OverviewSize || number < 0 || number >= OverviewSize * OverviewSize)
            {
                return null;
            }
            var pixel = new byte[4];
            Buffer.BlockCopy(_overview.Pixels, (((number % OverviewSize) * OverviewSize) + (number / OverviewSize)) * 4, pixel, 0, 4);
            return pixel;
        }

        /// <summary>Size and format of hues.dds, null when missing.</summary>
        public string DdsInfo
        {
            get
            {
                DdsImage dds = ReadDds();
                return dds == null ? null : $"{dds.Width} x {dds.Height} {dds.FormatName}";
            }
        }

        private DdsImage _overview;
        private bool _overviewRead;

        private DdsImage ReadDds()
        {
            if (!_ddsRead)
            {
                _ddsRead = true;
                EcUopEntry entry = DdsEntry;
                try
                {
                    _dds = entry == null ? null : DdsImage.Decode(Package.ReadData(entry));
                }
                catch (Exception)
                {
                    _dds = null;
                }
            }
            return _dds;
        }

        /// <summary>
        /// The 32 classic colors of the hue: the stored exact colors when they still fit the bitmap, otherwise calculated back.
        /// </summary>
        public HueDecodeResult GetColors(EcHue hue)
        {
            if (_decoded.TryGetValue(hue.Number, out HueDecodeResult cached))
            {
                return cached;
            }
            byte[] px = GetRamp(hue, out int w, out int h);
            HueDecodeResult result = null;
            ClassicHue stored = _exact?.Get(hue.Number);
            if (stored != null && w == EcHueRender.RampWidth && h >= 2)
            {
                byte[] drawn = EcHueRender.RenderRampPixels(stored.Colors, Options);
                if (drawn.AsSpan().SequenceEqual(px.AsSpan(0, drawn.Length)))
                {
                    result = new HueDecodeResult { Colors = (ushort[])stored.Colors.Clone(), Quality = HueDecodeQuality.Exact };
                }
            }
            result ??= new EcHueDecoder(Options).Decode(px, w, h, GetDdsRow(hue.Number));
            _decoded[hue.Number] = result;
            return result;
        }

        /// <summary>Group header of the stored classic file (0 when nothing is stored).</summary>
        public int StoredGroupHeader(int number) => _exact?.GroupHeader(number) ?? 0;

        /// <summary>Keeps the group headers of the classic file in the stored data.</summary>
        public void SetClassicGroupHeaders(ClassicHueFile classic)
        {
            if (!StoreExactData)
            {
                return;
            }
            _exact ??= new EcHueExactData { Options = Options.Clone() };
            _exact.SetGroupHeaders(classic);
            _exactChanged = true;
        }

        /// <summary>The stored classic hue (table values and name bytes), null if not stored.</summary>
        public ClassicHue GetStoredClassic(int number) => _exact?.Get(number);

        // ------------------------------------------------------------------ changing

        public void SetName(EcHue hue, string name)
        {
            hue.Name = name;
            _namesChanged = true;
            _changedNames.Add(hue.Number);
        }

        /// <summary>Draws the ramp from 32 classic colors (bitmap, hues.dds row, overview pixel and stored colors).</summary>
        public void SetColors(EcHue hue, ushort[] colors, ClassicHue source = null)
        {
            byte[] pixels = EcHueRender.RenderRampPixels(colors, Options);
            byte[] original = Package.ReadData(hue.Entry);
            Package.SetData(hue.Entry, PatchBmp(original, pixels, EcHueRender.RampWidth, 2) ?? WriteBmp(pixels, EcHueRender.RampWidth, 2));
            AfterColors(hue.Number, colors, source);
        }

        private void AfterColors(int number, ushort[] colors, ClassicHue source)
        {
            var pixel = new byte[4];
            EcHueRender.ToBgra(colors[31], Options.Scale, pixel, 0);
            _atlasChanges[number] = (EcHueRender.RenderDdsRow(colors, Options), pixel);
            _decoded[number] = new HueDecodeResult { Colors = (ushort[])colors.Clone(), Quality = StoreExactData ? HueDecodeQuality.Exact : HueDecodeQuality.Verified };
            if (StoreExactData)
            {
                _exact ??= new EcHueExactData { Options = Options.Clone() };
                ClassicHue stored = source?.Clone(number) ?? _exact.Get(number)?.Clone(number) ?? new ClassicHue(number);
                stored.SetColors(colors);
                if (source == null)
                {
                    stored.SetDefaultTable();
                }
                _exact.Set(stored);
                _exactChanged = true;
            }
        }

        /// <summary>Replaces the ramp with BGRA pixels (any width; stored as a 32 bit BMP). hues.dds and the overview follow.</summary>
        public void SetRamp(EcHue hue, byte[] bgra, int width, int height)
        {
            byte[] original = Package.ReadData(hue.Entry);
            byte[] patched = PatchBmp(original, bgra, width, height);
            Package.SetData(hue.Entry, patched ?? WriteBmp(bgra, width, height));
            _decoded.Remove(hue.Number);
            QueueAtlasFromRamp(hue.Number, bgra, width, height);
            if (_exact?.Remove(hue.Number) == true)
            {
                _exactChanged = true;
            }
        }

        private void QueueAtlasFromRamp(int number, byte[] bgra, int width, int height)
        {
            byte[] row;
            var pixel = new byte[4];
            HueDecodeResult decoded = width == EcHueRender.RampWidth && height >= 2 ? new EcHueDecoder(Options).Decode(bgra, width, height) : null;
            if (decoded != null && decoded.Quality <= HueDecodeQuality.Verified)
            {
                // made from 32 colors: draw the dds row the way the client files have it
                row = EcHueRender.RenderDdsRow(decoded.Colors, Options);
            }
            else
            {
                row = EcHueRender.ResampleRow(bgra, width, 0);
            }
            int y = height >= 2 ? 1 : 0;
            Buffer.BlockCopy(bgra, ((y * width) + (width / 2)) * 4, pixel, 0, 4);
            pixel[3] = 255;
            _atlasChanges[number] = (row, pixel);
        }

        /// <summary>Adds a new hue from pixels (copy of an existing ramp or an image).</summary>
        public EcHue Add(int number, byte[] bgra, int width, int height, string name)
        {
            EcUopEntry entry = Package.AddOrReplace(string.Format(CultureInfo.InvariantCulture, NamePattern, number), WriteBmp(bgra, width, height), Package.PreferredCompression);
            var hue = new EcHue { Number = number, Entry = entry, Name = name };
            _hues[number] = hue;
            _namesChanged = true;
            _changedNames.Add(number);
            _decoded.Remove(number);
            QueueAtlasFromRamp(number, bgra, width, height);
            if (_exact?.Remove(number) == true)
            {
                _exactChanged = true;
            }
            return hue;
        }

        /// <summary>Adds (or replaces) a hue drawn from 32 classic colors.</summary>
        public EcHue Add(int number, ushort[] colors, string name, ClassicHue source = null, EcUopCompression? compression = null)
        {
            byte[] bmp = EcHueRender.RenderRampBmp(colors, Options);
            EcUopEntry entry = Package.AddOrReplace(string.Format(CultureInfo.InvariantCulture, NamePattern, number), bmp, compression ?? Package.PreferredCompression);
            if (!_hues.TryGetValue(number, out EcHue hue))
            {
                hue = new EcHue { Number = number, Entry = entry };
                _hues[number] = hue;
            }
            hue.Entry = entry;
            if (hue.Name != name || !_nameLines.ContainsKey(number))
            {
                hue.Name = name;
                _namesChanged = true;
                _changedNames.Add(number);
            }
            AfterColors(number, colors, source);
            return hue;
        }

        public void Remove(EcHue hue)
        {
            Package.Remove(hue.Entry);
            _hues.Remove(hue.Number);
            _decoded.Remove(hue.Number);
            _namesChanged = true;
            _changedNames.Add(hue.Number);
            _atlasChanges[hue.Number] = (new byte[EcHueRender.RampWidth * 4], new byte[4]);
            if (_exact?.Remove(hue.Number) == true)
            {
                _exactChanged = true;
            }
        }

        /// <summary>Draws hues.dds and the overview again from all hue bitmaps (repairs files that don't match).</summary>
        public void RebuildAtlases()
        {
            _atlasChanges.Clear();
            for (int n = 1; n <= MaxAtlasHue; n++)
            {
                _atlasChanges[n] = (new byte[EcHueRender.RampWidth * 4], new byte[4]);
            }
            foreach (EcHue hue in _hues.Values)
            {
                HueDecodeResult colors = GetColors(hue);
                if (colors.Quality <= HueDecodeQuality.Verified)
                {
                    var pixel = new byte[4];
                    EcHueRender.ToBgra(colors.Colors[31], Options.Scale, pixel, 0);
                    _atlasChanges[hue.Number] = (EcHueRender.RenderDdsRow(colors.Colors, Options), pixel);
                }
                else
                {
                    byte[] px = GetRamp(hue, out int w, out int h);
                    QueueAtlasFromRamp(hue.Number, px, w, h);
                }
            }
            _rebuildAll = true;
        }

        private bool _rebuildAll;

        /// <summary>True when changes are waiting for <see cref="Apply"/>.</summary>
        public bool HasPendingChanges => _namesChanged || _exactChanged || _atlasChanges.Count > 0;

        /// <summary>Hues above <see cref="MaxAtlasHue"/> (they have no place in hues.dds).</summary>
        public IEnumerable<int> HuesOutsideAtlas => _hues.Keys.Where(n => n > MaxAtlasHue);

        /// <summary>Writes names, hues.dds, overview and the stored classic colors into the package.</summary>
        public void Apply(EcUopCompression? compression = null)
        {
            EcUopCompression comp = compression ?? Package.PreferredCompression;
            if (_namesChanged)
            {
                // unchanged lines are written as they were (the client files have lines like "2977" without a comma)
                var sb = new StringBuilder();
                int max = Math.Max(MaxNumber, _nameLines.Count == 0 ? 0 : _nameLines.Keys.Max());
                for (int i = 1; i <= max; i++)
                {
                    _hues.TryGetValue(i, out EcHue hue);
                    if (!_changedNames.Contains(i) && _nameLines.TryGetValue(i, out string raw))
                    {
                        sb.Append(raw);
                    }
                    else
                    {
                        sb.Append(i.ToString(CultureInfo.InvariantCulture)).Append(',').Append(hue?.Name ?? string.Empty);
                    }
                    if (i < max || _namesEndWithNewLine)
                    {
                        sb.Append(_newLine);
                    }
                }
                byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
                if (_nameEntry == null)
                {
                    _nameEntry = Package.AddOrReplace(NamesName, data, comp);
                }
                else
                {
                    Package.SetData(_nameEntry, data);
                }
                foreach (int n in _changedNames)
                {
                    _nameLines[n] = n.ToString(CultureInfo.InvariantCulture) + "," + (Get(n)?.Name ?? string.Empty);
                }
                _changedNames.Clear();
                _namesChanged = false;
            }

            if (_atlasChanges.Count > 0)
            {
                WriteAtlases(comp);
                _atlasChanges.Clear();
                _rebuildAll = false;
            }

            if (_exactChanged && _exact != null)
            {
                if (StoreExactData)
                {
                    _exact.Options = Options.Clone();
                    Package.AddOrReplace(EcHueExactData.EntryName, _exact.ToBytes(), comp);
                }
                else if (ExactEntry != null)
                {
                    Package.Remove(ExactEntry);
                }
                _exactChanged = false;
            }
            else if (!StoreExactData && ExactEntry != null)
            {
                Package.Remove(ExactEntry);
                _exact = null;
            }
        }

        private void WriteAtlases(EcUopCompression comp)
        {
            // hues.dds: keep the format of the existing file
            DdsImage old = _rebuildAll ? null : ReadDds();
            bool reuse = old != null && old.Width == DdsSize && old.Height == DdsSize;
            byte[] pixels = reuse ? (byte[])old.Pixels.Clone() : new byte[DdsSize * DdsSize * 4];
            DdsSaveFormat format = DdsSaveFormat.A8R8G8B8;
            if (DdsEntry != null && DdsImage.TryReadInfo(Package.ReadDataHead(DdsEntry, 128), out _, out _, out string fmt, out _) && fmt != null)
            {
                format = fmt.Contains("DXT1") ? DdsSaveFormat.Dxt1 : fmt.Contains("DXT3") ? DdsSaveFormat.Dxt3 : fmt.Contains("DXT5") ? DdsSaveFormat.Dxt5 : DdsSaveFormat.A8R8G8B8;
            }
            foreach (KeyValuePair<int, (byte[] Row, byte[] Pixel)> change in _atlasChanges)
            {
                int n = change.Key;
                if (n < 1 || n > MaxAtlasHue)
                {
                    continue;
                }
                int x0 = n / DdsSize * EcHueRender.RampWidth;
                int y = n % DdsSize;
                Buffer.BlockCopy(change.Value.Row, 0, pixels, ((y * DdsSize) + x0) * 4, EcHueRender.RampWidth * 4);
            }
            byte[] oldBytes = DdsEntry == null || _rebuildAll ? null : Package.ReadData(DdsEntry);
            byte[] dds = PatchUncompressedDds(oldBytes, pixels, DdsSize, DdsSize) ?? DdsImage.Encode(pixels, DdsSize, DdsSize, format, 1);
            Package.AddOrReplace(DdsName, dds, comp);
            _dds = new DdsImage(DdsSize, DdsSize, pixels, format.ToString(), 1, format);
            _ddsRead = true;

            // overview 64 x 64
            byte[] overview = new byte[OverviewSize * OverviewSize * 4];
            DdsSaveFormat overviewFormat = DdsSaveFormat.A8R8G8B8;
            EcUopEntry overviewEntry = OverviewEntry;
            byte[] overviewOld = null;
            if (overviewEntry != null && !_rebuildAll)
            {
                try
                {
                    overviewOld = Package.ReadData(overviewEntry);
                    DdsImage image = DdsImage.Decode(overviewOld);
                    if (image.Width == OverviewSize && image.Height == OverviewSize)
                    {
                        overview = (byte[])image.Pixels.Clone();
                        overviewFormat = image.SuggestedSaveFormat;
                    }
                }
                catch (Exception)
                {
                    // drawn new
                }
            }
            foreach (KeyValuePair<int, (byte[] Row, byte[] Pixel)> change in _atlasChanges)
            {
                int n = change.Key;
                if (n < 1 || n >= OverviewSize * OverviewSize)
                {
                    continue;
                }
                int x = n / OverviewSize;
                int y = n % OverviewSize;
                Buffer.BlockCopy(change.Value.Pixel, 0, overview, ((y * OverviewSize) + x) * 4, 4);
            }
            byte[] overviewDds = PatchUncompressedDds(overviewOld, overview, OverviewSize, OverviewSize) ?? DdsImage.Encode(overview, OverviewSize, OverviewSize, overviewFormat, 1);
            Package.AddOrReplace(OverviewHash, null, overviewDds, comp);
            _overview = new DdsImage(OverviewSize, OverviewSize, overview, "A8R8G8B8", 1, DdsSaveFormat.A8R8G8B8);
            _overviewRead = true;
        }

        /// <summary>
        /// Writes the pixels into an uncompressed 32 bit BGRA DDS of the same size (keeps the header byte by byte), null if not possible.
        /// </summary>
        public static byte[] PatchUncompressedDds(byte[] dds, byte[] bgra, int width, int height)
        {
            if (dds == null || dds.Length < 128 + (width * height * 4) || BitConverter.ToUInt32(dds, 0) != 0x20534444 ||
                BitConverter.ToInt32(dds, 12) != height || BitConverter.ToInt32(dds, 16) != width)
            {
                return null;
            }
            uint pfFlags = BitConverter.ToUInt32(dds, 80);
            uint fourCc = BitConverter.ToUInt32(dds, 84);
            uint bits = BitConverter.ToUInt32(dds, 88);
            if ((pfFlags & 0x40) == 0 || fourCc != 0 || bits != 32 || BitConverter.ToUInt32(dds, 92) != 0xFF0000 ||
                BitConverter.ToUInt32(dds, 96) != 0xFF00 || BitConverter.ToUInt32(dds, 100) != 0xFF)
            {
                return null;
            }
            byte[] copy = (byte[])dds.Clone();
            Buffer.BlockCopy(bgra, 0, copy, 128, width * height * 4);
            return copy;
        }

        // ------------------------------------------------------------------ bmp

        /// <summary>
        /// Writes the pixels into the original bitmap when the size is the same (keeps the original header byte by byte).
        /// </summary>
        public static byte[] PatchBmp(byte[] bmp, byte[] bgra, int width, int height)
        {
            if (bmp == null || bmp.Length < 54 || BitConverter.ToUInt16(bmp, 28) != 32 ||
                BitConverter.ToInt32(bmp, 18) != width || Math.Abs(BitConverter.ToInt32(bmp, 22)) != height)
            {
                return null;
            }

            byte[] copy = (byte[])bmp.Clone();
            int offset = BitConverter.ToInt32(bmp, 10);
            bool bottomUp = BitConverter.ToInt32(bmp, 22) > 0;
            for (int y = 0; y < height; y++)
            {
                int row = bottomUp ? height - 1 - y : y;
                int dst = offset + (row * width * 4);
                if (dst + (width * 4) > copy.Length)
                {
                    return null;
                }
                Buffer.BlockCopy(bgra, y * width * 4, copy, dst, width * 4);
            }
            return copy;
        }

        public static byte[] ReadBmp(byte[] bmp, out int width, out int height)
        {
            int offset = BitConverter.ToInt32(bmp, 10);
            width = BitConverter.ToInt32(bmp, 18);
            int rawHeight = BitConverter.ToInt32(bmp, 22);
            int bits = BitConverter.ToUInt16(bmp, 28);
            height = Math.Abs(rawHeight);
            bool bottomUp = rawHeight > 0;
            int bytesPerPixel = bits / 8;
            int stride = ((width * bits) + 31) / 32 * 4;

            var pixels = new byte[width * height * 4];
            for (int y = 0; y < height; y++)
            {
                int row = bottomUp ? height - 1 - y : y;
                for (int x = 0; x < width; x++)
                {
                    int src = offset + (row * stride) + (x * bytesPerPixel);
                    int dst = ((y * width) + x) * 4;
                    if (src + bytesPerPixel > bmp.Length)
                    {
                        continue;
                    }
                    pixels[dst] = bmp[src];
                    pixels[dst + 1] = bmp[src + 1];
                    pixels[dst + 2] = bmp[src + 2];
                    pixels[dst + 3] = bytesPerPixel == 4 ? bmp[src + 3] : (byte)255;
                }
            }
            return pixels;
        }

        /// <summary>
        /// 32 bit BMP, bottom up, 54 byte header with image size and resolution 0. The size field holds the
        /// file size / 4, exactly like the bitmaps of the original client (2102 bytes → 525).
        /// </summary>
        public static byte[] WriteBmp(byte[] bgra, int width, int height)
        {
            int size = 54 + (width * height * 4);
            var bmp = new byte[size];
            bmp[0] = (byte)'B';
            bmp[1] = (byte)'M';
            BitConverter.GetBytes(size / 4).CopyTo(bmp, 2);
            BitConverter.GetBytes(54).CopyTo(bmp, 10);
            BitConverter.GetBytes(40).CopyTo(bmp, 14);
            BitConverter.GetBytes(width).CopyTo(bmp, 18);
            BitConverter.GetBytes(height).CopyTo(bmp, 22);
            BitConverter.GetBytes((ushort)1).CopyTo(bmp, 26);
            BitConverter.GetBytes((ushort)32).CopyTo(bmp, 28);
            for (int y = 0; y < height; y++)
            {
                Buffer.BlockCopy(bgra, ((height - 1 - y) * width) * 4, bmp, 54 + (y * width * 4), width * 4);
            }
            return bmp;
        }
    }
}

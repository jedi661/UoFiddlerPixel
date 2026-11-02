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
using System.Linq;
using System.Text;
using System.Threading;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Hues
{
    /// <summary>Lines and counters of a hue job.</summary>
    public sealed class HueReport
    {
        private readonly StringBuilder _text = new StringBuilder();

        public int Written { get; set; }
        public int Skipped { get; set; }
        public int Problems { get; set; }
        public Dictionary<HueDecodeQuality, int> Qualities { get; } = new Dictionary<HueDecodeQuality, int>();
        public List<int> NotExact { get; } = new List<int>();

        public void Line(string text) => _text.AppendLine(text);

        public void Problem(string text)
        {
            Problems++;
            _text.AppendLine("! " + text);
        }

        public void Count(HueDecodeQuality quality, int number)
        {
            Qualities[quality] = Qualities.TryGetValue(quality, out int c) ? c + 1 : 1;
            if (quality > HueDecodeQuality.Verified)
            {
                NotExact.Add(number);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(_text.ToString());
            if (Qualities.Count > 0)
            {
                sb.AppendLine("Colors: " + string.Join(", ", Qualities.OrderBy(q => q.Key).Select(q => $"{HueDecodeResult.Text(q.Key)} {q.Value}")));
            }
            if (NotExact.Count > 0)
            {
                sb.AppendLine("Not exact (bitmap not drawn from 32 classic colors, nearest colors used): " + string.Join(", ", NotExact.Take(40)) + (NotExact.Count > 40 ? $" ... (+{NotExact.Count - 40})" : string.Empty));
            }
            return sb.ToString();
        }
    }

    /// <summary>Which hues a job works on.</summary>
    public class HueSelection
    {
        public int First { get; set; } = 1;
        public int Last { get; set; } = int.MaxValue;

        /// <summary>Only these numbers (e.g. the marked rows), null = all from First to Last.</summary>
        public ISet<int> Only { get; set; }

        public bool Names { get; set; } = true;

        public bool Includes(int number) => number >= First && number <= Last && (Only == null || Only.Contains(number));
    }

    public sealed class ClassicToEcOptions : HueSelection
    {
        /// <summary>Hues whose colors (and name) are the same already are not written again.</summary>
        public bool OnlyChanged { get; set; } = true;

        /// <summary>Unused classic slots (all colors 0, no name) are not written.</summary>
        public bool SkipEmpty { get; set; }

        /// <summary>Store the exact classic data in the package (lossless way back).</summary>
        public bool StoreExact { get; set; } = true;
    }

    public sealed class EcToClassicOptions : HueSelection
    {
        /// <summary>Keep table start / end of the base file when the colors did not change.</summary>
        public bool KeepTable { get; set; } = true;
    }

    public enum HueCompareStatus
    {
        Same,
        ColorsDiffer,
        NameDiffers,
        OnlyClassic,
        OnlyEc
    }

    public sealed class HueCompareRow
    {
        public int Number { get; set; }
        public ClassicHue Classic { get; set; }
        public EcHue Ec { get; set; }
        public HueDecodeResult Decoded { get; set; }

        /// <summary>Number of the 32 colors that differ.</summary>
        public int DifferentColors { get; set; }

        /// <summary>Largest difference of one channel (0..255).</summary>
        public int MaxDiff { get; set; }

        public bool NameDiffers { get; set; }
        public HueCompareStatus Status { get; set; }

        public string StatusText => Status switch
        {
            HueCompareStatus.Same => "same",
            HueCompareStatus.ColorsDiffer => $"{DifferentColors} colors differ (max {MaxDiff})" + (NameDiffers ? ", name" : string.Empty),
            HueCompareStatus.NameDiffers => "name differs",
            HueCompareStatus.OnlyClassic => "only classic",
            _ => "only EC"
        };
    }

    /// <summary>
    /// Classic hues.mul ↔ Enhanced Client Hues.uop (huesmul2uop, improved: update instead of rebuild,
    /// any number of hues, the way back without loss, compare).
    /// </summary>
    public static class EcHuesConverter
    {
        /// <summary>An empty Hues.uop package with the given drawing settings.</summary>
        public static EcHues CreatePackage(HueRenderOptions options)
        {
            var hues = new EcHues(EcUopPackage.CreateEmpty());
            hues.SetOptions(options);
            return hues;
        }

        /// <summary>
        /// Writes classic hues into the EC package: bitmap, hues.dds row, overview pixel, name and (optional) the exact data.
        /// Call <see cref="EcHues.Apply"/> is done at the end.
        /// </summary>
        public static HueReport ClassicToEc(ClassicHueFile classic, EcHues target, ClassicToEcOptions options, IProgress<int> progress = null, CancellationToken token = default)
        {
            var report = new HueReport();
            bool isNew = target.Count == 0;
            EcUopCompression compression = isNew ? EcUopCompression.Zlib : target.Package.PreferredCompression;
            target.StoreExactData = options.StoreExact;
            report.Line($"Drawing settings: {target.Options}");
            if (options.StoreExact)
            {
                target.SetClassicGroupHeaders(classic);
            }

            int last = Math.Min(options.Last, classic.Count);
            int total = Math.Max(1, last - options.First + 1);
            for (int n = Math.Max(1, options.First); n <= last; n++)
            {
                token.ThrowIfCancellationRequested();
                if (((n - options.First) & 63) == 0)
                {
                    progress?.Report((n - options.First) * 90 / total);
                }
                if (!options.Includes(n))
                {
                    continue;
                }
                ClassicHue hue = classic.Get(n);
                EcHue existing = target.Get(n);
                if (options.SkipEmpty && hue.IsEmpty && existing == null)
                {
                    report.Skipped++;
                    continue;
                }
                string name = options.Names ? hue.Name : existing?.Name ?? hue.Name;
                if (existing != null && options.OnlyChanged)
                {
                    HueDecodeResult current = target.GetColors(existing);
                    bool sameColors = current.Quality <= HueDecodeQuality.Verified && hue.SameColors(current.Colors);
                    bool sameName = !options.Names || string.Equals(existing.Name ?? string.Empty, hue.Name, StringComparison.Ordinal);
                    bool storedOk = !options.StoreExact || current.Quality == HueDecodeQuality.Exact;
                    if (sameColors && sameName && storedOk)
                    {
                        report.Skipped++;
                        continue;
                    }
                    if (sameColors && !sameName)
                    {
                        target.SetName(existing, name);
                        report.Written++;
                        continue;
                    }
                }
                target.Add(n, hue.Colors, name, hue, compression);
                report.Written++;
            }

            progress?.Report(92);
            target.Apply(compression);
            progress?.Report(100);

            report.Line($"{report.Written} hues written, {report.Skipped} unchanged / skipped.");
            int outside = target.HuesOutsideAtlas.Count();
            if (outside > 0)
            {
                report.Problem($"{outside} hues are above {EcHues.MaxAtlasHue}: they have no row in hues.dds (items can't use them).");
            }
            if (options.StoreExact)
            {
                report.Line($"Exact classic data stored in {EcHueExactData.EntryName} (lossless way back).");
            }
            return report;
        }

        /// <summary>
        /// Builds a classic hues.mul from the EC hues. <paramref name="baseFile"/> (optional) keeps headers,
        /// table values and every hue outside the selection.
        /// </summary>
        public static ClassicHueFile EcToClassic(EcHues source, ClassicHueFile baseFile, EcToClassicOptions options, out HueReport report, IProgress<int> progress = null, CancellationToken token = default)
        {
            report = new HueReport();
            report.Line($"Drawing settings of Hues.uop: {source.Options}" + (source.ExactEntry != null ? " (stored)" : source.OptionsDetected ? $" (found, {source.DetectedExactSamples} of {source.DetectedSamples} samples exact)" : " (default)"));

            int max = source.MaxNumber;
            ClassicHueFile result = baseFile?.Clone() ?? ClassicHueFile.Create(0);
            int needed = Math.Max(baseFile == null ? ClassicHueFile.DefaultHueCount : 0, Math.Min(max, options.Last));
            result.EnsureCount(needed);
            if (baseFile == null)
            {
                for (int n = 1; n <= result.Count; n += ClassicHueFile.HuesPerGroup)
                {
                    result.SetGroupHeader(n, source.StoredGroupHeader(n));
                }
            }

            List<EcHue> hues = source.Hues.Where(h => options.Includes(h.Number)).ToList();
            int done = 0;
            var truncated = new List<int>();
            foreach (EcHue hue in hues)
            {
                token.ThrowIfCancellationRequested();
                if ((done++ & 63) == 0)
                {
                    progress?.Report(done * 100 / Math.Max(1, hues.Count));
                }

                HueDecodeResult decoded = source.GetColors(hue);
                report.Count(decoded.Quality, hue.Number);
                ClassicHue target = result.Get(hue.Number);
                ClassicHue stored = decoded.Quality == HueDecodeQuality.Exact ? source.GetStoredClassic(hue.Number) : null;
                bool colorsChanged = !target.SameColors(decoded.Colors);
                bool fresh = baseFile == null || hue.Number > baseFile.Count;
                target.SetColors(decoded.Colors);
                if (stored != null && (fresh || colorsChanged || !options.KeepTable))
                {
                    // original table values of the classic file
                    target.TableStart = stored.TableStart;
                    target.TableEnd = stored.TableEnd;
                }
                else if (fresh || colorsChanged || !options.KeepTable)
                {
                    target.SetDefaultTable();
                }

                if (options.Names)
                {
                    string name = hue.Name ?? string.Empty;
                    if (stored != null && stored.Name == name)
                    {
                        target.SetRawName((byte[])stored.RawName.Clone());
                    }
                    else
                    {
                        string ascii = ToAscii(name);
                        if (ascii.Length > ClassicHue.NameLength)
                        {
                            truncated.Add(hue.Number);
                        }
                        target.Name = ascii.Length > ClassicHue.NameLength ? ascii.Substring(0, ClassicHue.NameLength) : ascii;
                    }
                }
                report.Written++;
            }
            progress?.Report(100);

            report.Line($"{report.Written} hues written into a hues.mul with {result.Count} hues ({result.Count / ClassicHueFile.HuesPerGroup} groups).");
            if (truncated.Count > 0)
            {
                report.Problem($"{truncated.Count} names are longer than 20 characters and were cut: " + string.Join(", ", truncated.Take(30)) + (truncated.Count > 30 ? " ..." : string.Empty));
            }
            if (max > result.Count)
            {
                report.Problem($"Hues above {result.Count} were not written (range).");
            }
            return result;
        }

        /// <summary>Compares every hue of both files.</summary>
        public static List<HueCompareRow> Compare(ClassicHueFile classic, EcHues ec, IProgress<int> progress = null, CancellationToken token = default)
        {
            var rows = new List<HueCompareRow>();
            int max = Math.Max(classic?.Count ?? 0, ec?.MaxNumber ?? 0);
            for (int n = 1; n <= max; n++)
            {
                token.ThrowIfCancellationRequested();
                if ((n & 63) == 0)
                {
                    progress?.Report(n * 100 / max);
                }
                ClassicHue c = classic?.Get(n);
                EcHue e = ec?.Get(n);
                if (e == null && (c == null || c.IsEmpty))
                {
                    continue;
                }
                var row = new HueCompareRow { Number = n, Classic = c, Ec = e };
                if (e != null)
                {
                    row.Decoded = ec.GetColors(e);
                }
                if (c == null)
                {
                    row.Status = HueCompareStatus.OnlyEc;
                }
                else if (e == null)
                {
                    row.Status = HueCompareStatus.OnlyClassic;
                }
                else
                {
                    HueColorScale scale = ec.Options.Scale;
                    for (int i = 0; i < 32; i++)
                    {
                        ushort a = c.Colors[i];
                        ushort b = row.Decoded.Colors[i];
                        if ((a & 0x7FFF) != (b & 0x7FFF))
                        {
                            row.DifferentColors++;
                            for (int ch = 0; ch < 3; ch++)
                            {
                                row.MaxDiff = Math.Max(row.MaxDiff, Math.Abs(EcHueRender.To8(EcHueRender.Channel(a, ch), scale) - EcHueRender.To8(EcHueRender.Channel(b, ch), scale)));
                            }
                        }
                    }
                    string ecName = e.Name ?? string.Empty;
                    row.NameDiffers = !string.Equals(c.Name, ecName, StringComparison.Ordinal) &&
                                      !(ecName.Length > ClassicHue.NameLength && string.Equals(c.Name, ToAscii(ecName).Substring(0, ClassicHue.NameLength), StringComparison.Ordinal));
                    row.Status = row.DifferentColors > 0 ? HueCompareStatus.ColorsDiffer : row.NameDiffers ? HueCompareStatus.NameDiffers : HueCompareStatus.Same;
                }
                rows.Add(row);
            }
            progress?.Report(100);
            return rows;
        }

        /// <summary>Checks Hues.uop: entries, drawing style, how exact the colors are, whether hues.dds and the overview fit the bitmaps.</summary>
        public static HueReport Analyze(EcHues hues, IProgress<int> progress = null, CancellationToken token = default)
        {
            var report = new HueReport();
            List<EcHue> list = hues.Hues.ToList();
            int max = hues.MaxNumber;
            report.Line($"{list.Count} hue bitmaps, numbers 1 to {max}" + (list.Count < max ? $" ({max - list.Count} numbers without bitmap)" : string.Empty) + ".");
            report.Line("Names: " + (hues.NamesEntry != null ? hues.NamesEntry.DisplayName : "missing"));
            report.Line("hues.dds: " + (hues.DdsInfo ?? "missing"));
            report.Line("Overview 0xFA5C6A1BC0D8B01B: " + (hues.OverviewEntry != null ? "yes" : "missing"));
            report.Line("Exact classic data: " + (hues.ExactEntry != null ? "yes (" + EcHueExactData.EntryName + ")" : "no"));
            HueRenderOptions options = hues.Options;
            report.Line($"Drawing settings: {options}" + (hues.ExactEntry != null ? " (stored)" : hues.OptionsDetected ? $" (found, {hues.DetectedExactSamples} of {hues.DetectedSamples} samples exact)" : " (default)"));
            if (hues.NamesEntry == null)
            {
                report.Problem("The name list is missing.");
            }
            if (hues.DdsInfo == null)
            {
                report.Problem("build/hues/hues.dds is missing – items can't be hued. Use \"Rebuild hues.dds + overview\".");
            }

            int ddsDiff = 0, overviewDiff = 0, odd = 0, done = 0;
            var ddsList = new List<int>();
            var overviewList = new List<int>();
            foreach (EcHue hue in list)
            {
                token.ThrowIfCancellationRequested();
                if ((done++ & 63) == 0)
                {
                    progress?.Report(done * 100 / Math.Max(1, list.Count));
                }
                byte[] px = hues.GetRamp(hue, out int w, out int h);
                if (w != EcHueRender.RampWidth || h != 2)
                {
                    odd++;
                }
                HueDecodeResult decoded = hues.GetColors(hue);
                report.Count(decoded.Quality, hue.Number);

                if (hue.Number <= EcHues.MaxAtlasHue)
                {
                    byte[] row = hues.GetDdsRow(hue.Number);
                    byte[] expected = decoded.Quality <= HueDecodeQuality.Verified ? EcHueRender.RenderDdsRow(decoded.Colors, options) : null;
                    if (row != null && expected != null && Differs(row, expected))
                    {
                        ddsDiff++;
                        ddsList.Add(hue.Number);
                    }
                    byte[] pixel = hues.GetOverviewPixel(hue.Number);
                    if (pixel != null && h >= 2)
                    {
                        int offset = (w + (w / 2)) * 4;
                        if (Math.Abs(pixel[0] - px[offset]) + Math.Abs(pixel[1] - px[offset + 1]) + Math.Abs(pixel[2] - px[offset + 2]) > 6)
                        {
                            overviewDiff++;
                            overviewList.Add(hue.Number);
                        }
                    }
                }
            }
            progress?.Report(100);
            if (odd > 0)
            {
                report.Problem($"{odd} bitmaps are not 256 x 2 pixels.");
            }
            if (ddsDiff > 0)
            {
                report.Problem($"{ddsDiff} rows of hues.dds don't fit the bitmap (items look different from animations): " + string.Join(", ", ddsList.Take(30)) + (ddsList.Count > 30 ? " ..." : string.Empty) + " – \"Rebuild hues.dds + overview\" repairs it.");
            }
            if (overviewDiff > 0)
            {
                report.Problem($"{overviewDiff} pixels of the overview don't fit the bitmap: " + string.Join(", ", overviewList.Take(30)) + (overviewList.Count > 30 ? " ..." : string.Empty));
            }
            int outside = hues.HuesOutsideAtlas.Count();
            if (outside > 0)
            {
                report.Problem($"{outside} hues are above {EcHues.MaxAtlasHue} and have no row in hues.dds.");
            }
            if (report.Problems == 0)
            {
                report.Line("No problems found.");
            }
            return report;
        }

        private static bool Differs(byte[] a, byte[] b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if ((i & 3) != 3 && a[i] != b[i])
                {
                    return true;
                }
            }
            return false;
        }

        private static string ToAscii(string text)
        {
            var sb = new StringBuilder(text.Length);
            foreach (char ch in text)
            {
                sb.Append(ch >= 32 && ch < 127 ? ch : '?');
            }
            return sb.ToString();
        }
    }
}

/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Helpers for hues: undo/redo, analysis (empty, colour family, similar hues,
 * colour -> hue), palette export (PNG/ACT/GPL), text range files, writing a
 * hue list into a hues.mul file.
 *
 * Hue numbers: the game/server uses hue = file index + 1 (hue 0 = no hue).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Ultima;

namespace UoFiddler.Controls.Classes
{
    #region HueState

    /// <summary>Copy of the data of one hue (colours, table values, name)</summary>
    public sealed class HueState
    {
        public ushort[] Colors { get; } = new ushort[32];
        public ushort TableStart { get; set; }
        public ushort TableEnd { get; set; }
        public string Name { get; set; } = string.Empty;

        public static HueState Capture(Hue hue)
        {
            var s = new HueState
            {
                TableStart = hue.TableStart,
                TableEnd = hue.TableEnd,
                Name = hue.Name ?? string.Empty
            };
            Array.Copy(hue.Colors, s.Colors, Math.Min(32, hue.Colors.Length));
            return s;
        }

        public static HueState FromColors(ushort[] colors, string name)
        {
            var s = new HueState { Name = name ?? string.Empty };
            Array.Copy(colors, s.Colors, Math.Min(32, colors.Length));
            s.TableStart = s.Colors[0];
            s.TableEnd = s.Colors[31];
            return s;
        }

        public HueState Clone()
        {
            var s = new HueState { TableStart = TableStart, TableEnd = TableEnd, Name = Name };
            Array.Copy(Colors, s.Colors, 32);
            return s;
        }

        public void ApplyTo(Hue hue, bool colors = true, bool name = true, bool table = true)
        {
            if (colors)
            {
                Array.Copy(Colors, hue.Colors, Math.Min(32, hue.Colors.Length));
            }

            if (table)
            {
                hue.TableStart = TableStart;
                hue.TableEnd = TableEnd;
            }

            if (name)
            {
                hue.Name = Name;
            }
        }

        public bool SameAs(Hue hue)
        {
            if (hue == null || hue.TableStart != TableStart || hue.TableEnd != TableEnd || (hue.Name ?? string.Empty) != Name)
            {
                return false;
            }

            for (int i = 0; i < 32; i++)
            {
                if (hue.Colors[i] != Colors[i])
                {
                    return false;
                }
            }

            return true;
        }
    }

    #endregion

    #region HueHistory (undo / redo)

    /// <summary>
    /// Undo/redo for a hue list. Every change goes through <see cref="Record"/>.
    /// </summary>
    public sealed class HueHistory
    {
        private sealed class Step
        {
            public string Name;
            public Dictionary<int, HueState> Before = new Dictionary<int, HueState>();
            public Dictionary<int, HueState> After = new Dictionary<int, HueState>();
        }

        private readonly Func<Hue[]> _list;
        private readonly bool _isMain;
        private readonly List<Step> _undo = new List<Step>();
        private readonly List<Step> _redo = new List<Step>();
        private const int MaxSteps = 200;

        public HueHistory(Func<Hue[]> list, bool isMain = false)
        {
            _list = list;
            _isMain = isMain;
        }

        /// <summary>History of <see cref="Hues.List"/> (hues tab, hue editor, compare)</summary>
        public static HueHistory Main { get; } = new HueHistory(() => Hues.List, true);

        /// <summary>Raised after a change, undo or redo</summary>
        public event EventHandler Changed;

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;
        public string UndoName => _undo.Count > 0 ? _undo[_undo.Count - 1].Name : null;
        public string RedoName => _redo.Count > 0 ? _redo[_redo.Count - 1].Name : null;

        /// <summary>Clears the history (e.g. after the file was loaded again)</summary>
        public void Clear()
        {
            _undo.Clear();
            _redo.Clear();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Saves the state of the hues, runs the change and stores it as one undo step</summary>
        public void Record(string name, IEnumerable<int> indices, Action change)
        {
            Hue[] list = _list();
            if (list == null)
            {
                return;
            }

            var step = new Step { Name = name };
            foreach (int i in indices.Distinct())
            {
                if (i >= 0 && i < list.Length && list[i] != null)
                {
                    step.Before[i] = HueState.Capture(list[i]);
                }
            }

            change();

            foreach (int i in step.Before.Keys)
            {
                step.After[i] = HueState.Capture(list[i]);
            }

            // drop entries that did not change
            foreach (int i in step.Before.Keys.ToList())
            {
                if (SameState(step.Before[i], step.After[i]))
                {
                    step.Before.Remove(i);
                    step.After.Remove(i);
                }
            }

            if (step.Before.Count == 0)
            {
                return;
            }

            _undo.Add(step);
            if (_undo.Count > MaxSteps)
            {
                _undo.RemoveAt(0);
            }

            _redo.Clear();
            AfterChange();
        }

        public string Undo()
        {
            if (_undo.Count == 0)
            {
                return null;
            }

            Step step = _undo[_undo.Count - 1];
            _undo.RemoveAt(_undo.Count - 1);
            Restore(step.Before);
            _redo.Add(step);
            AfterChange();
            return step.Name;
        }

        public string Redo()
        {
            if (_redo.Count == 0)
            {
                return null;
            }

            Step step = _redo[_redo.Count - 1];
            _redo.RemoveAt(_redo.Count - 1);
            Restore(step.After);
            _undo.Add(step);
            AfterChange();
            return step.Name;
        }

        private void Restore(Dictionary<int, HueState> states)
        {
            Hue[] list = _list();
            foreach (KeyValuePair<int, HueState> pair in states)
            {
                if (pair.Key < list.Length && list[pair.Key] != null)
                {
                    pair.Value.ApplyTo(list[pair.Key]);
                }
            }
        }

        private void AfterChange()
        {
            if (_isMain)
            {
                Options.ChangedUltimaClass["Hues"] = true;
                ControlEvents.FireHueChangeEvent();
            }

            Changed?.Invoke(this, EventArgs.Empty);
        }

        private static bool SameState(HueState a, HueState b)
        {
            return a.Name == b.Name && a.TableStart == b.TableStart && a.TableEnd == b.TableEnd && a.Colors.SequenceEqual(b.Colors);
        }
    }

    #endregion

    #region Analysis

    public enum HueFamily
    {
        None,
        Red,
        Orange,
        Yellow,
        Green,
        Cyan,
        Blue,
        Purple,
        Pink,
        Gray
    }

    public static class HueAnalysis
    {
        /// <summary>5 bit hue colour -> RGB (0..255, same as the client: value * 8)</summary>
        public static Color ToColor(ushort c)
        {
            return Color.FromArgb(((c >> 10) & 0x1F) * 255 / 31, ((c >> 5) & 0x1F) * 255 / 31, (c & 0x1F) * 255 / 31);
        }

        public static ushort FromColor(Color color)
        {
            int r = (int)Math.Round(color.R * 31 / 255.0);
            int g = (int)Math.Round(color.G * 31 / 255.0);
            int b = (int)Math.Round(color.B * 31 / 255.0);
            return (ushort)((r << 10) | (g << 5) | b);
        }

        public static string ToHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        /// <summary>Draws the 32 colours of a hue as a strip; marked steps get a red triangle</summary>
        public static void DrawStrip(Graphics g, Rectangle rect, ushort[] colors, ICollection<int> marked = null)
        {
            if (colors == null)
            {
                g.FillRectangle(Brushes.Gainsboro, rect);
                return;
            }

            float size = rect.Width / 32f;
            for (int i = 0; i < 32; i++)
            {
                var cell = new RectangleF(rect.X + (i * size), rect.Y, size + 1, rect.Height);
                using (var brush = new SolidBrush(ToColor(colors[i])))
                {
                    g.FillRectangle(brush, cell);
                }

                if (marked != null && marked.Contains(i))
                {
                    PointF[] tri =
                    {
                        new PointF(cell.X, rect.Y), new PointF(cell.X + size, rect.Y), new PointF(cell.X + (size / 2), rect.Y + Math.Min(8, rect.Height / 2f))
                    };
                    g.FillPolygon(Brushes.Red, tri);
                }
            }
        }

        public static bool TryParseColor(string text, out Color color)
        {
            color = Color.Empty;
            text = (text ?? string.Empty).Trim();
            if (text.StartsWith("#"))
            {
                text = text.Substring(1);
            }
            else if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                text = text.Substring(2);
            }

            if (text.Length == 6 && int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int rgb))
            {
                color = Color.FromArgb((rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
                return true;
            }

            string[] parts = text.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3 && parts.All(p => int.TryParse(p, out int v) && v >= 0 && v <= 255))
            {
                color = Color.FromArgb(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
                return true;
            }

            return false;
        }

        /// <summary>true if all 32 colours are black (unused entry)</summary>
        public static bool IsEmpty(Hue hue) => hue == null || hue.Colors.All(c => (c & 0x7FFF) == 0);

        public static bool IsUnnamed(Hue hue)
        {
            string name = hue?.Name?.Trim() ?? string.Empty;
            return name.Length == 0 || name.Equals("Null", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>average brightness 0..100 %</summary>
        public static int Brightness(Hue hue)
        {
            if (hue == null)
            {
                return 0;
            }

            double sum = 0;
            foreach (ushort c in hue.Colors)
            {
                Color col = ToColor(c);
                sum += (0.299 * col.R) + (0.587 * col.G) + (0.114 * col.B);
            }

            return (int)Math.Round(sum / hue.Colors.Length / 255 * 100);
        }

        /// <summary>colour of the middle steps (representative colour)</summary>
        public static Color MainColor(Hue hue)
        {
            int r = 0, g = 0, b = 0, n = 0;
            for (int i = 12; i < 28; i++)
            {
                Color c = ToColor(hue.Colors[i]);
                r += c.R;
                g += c.G;
                b += c.B;
                n++;
            }

            return Color.FromArgb(r / n, g / n, b / n);
        }

        public static HueFamily Family(Hue hue)
        {
            if (IsEmpty(hue))
            {
                return HueFamily.None;
            }

            Color c = MainColor(hue);
            float sat = c.GetSaturation();
            float bri = c.GetBrightness();
            if (sat < 0.15f || bri < 0.06f)
            {
                return HueFamily.Gray;
            }

            float h = c.GetHue();
            if (h < 15 || h >= 345)
            {
                return HueFamily.Red;
            }

            if (h < 45)
            {
                return HueFamily.Orange;
            }

            if (h < 70)
            {
                return HueFamily.Yellow;
            }

            if (h < 165)
            {
                return HueFamily.Green;
            }

            if (h < 200)
            {
                return HueFamily.Cyan;
            }

            if (h < 255)
            {
                return HueFamily.Blue;
            }

            if (h < 290)
            {
                return HueFamily.Purple;
            }

            return HueFamily.Pink;
        }

        private static int Dist(ushort a, ushort b)
        {
            int dr = ((a >> 10) & 0x1F) - ((b >> 10) & 0x1F);
            int dg = ((a >> 5) & 0x1F) - ((b >> 5) & 0x1F);
            int db = (a & 0x1F) - (b & 0x1F);
            return (3 * dr * dr) + (4 * dg * dg) + (2 * db * db);
        }

        /// <summary>difference of two hues (0 = equal)</summary>
        public static int Distance(Hue a, Hue b)
        {
            int sum = 0;
            for (int i = 0; i < 32; i++)
            {
                sum += Dist(a.Colors[i], b.Colors[i]);
            }

            return sum;
        }

        /// <summary>the most similar hues of a list (without the hue itself and empty hues)</summary>
        public static List<(int Index, int Score)> Similar(Hue[] list, int index, int count)
        {
            Hue reference = list[index];
            return list.Where(h => h != null && h.Index != index && !IsEmpty(h))
                .Select(h => (h.Index, Distance(reference, h)))
                .OrderBy(t => t.Item2)
                .Take(count)
                .ToList();
        }

        /// <summary>hues that contain a colour (best step); Step = index of the colour step</summary>
        public static List<(int Index, int Step, int Score)> FindByColor(Hue[] list, Color color, int count)
        {
            ushort target = FromColor(color);
            var result = new List<(int, int, int)>();
            foreach (Hue h in list)
            {
                if (h == null || IsEmpty(h))
                {
                    continue;
                }

                int best = int.MaxValue, bestStep = 0;
                for (int i = 0; i < 32; i++)
                {
                    int d = Dist(h.Colors[i], target);
                    if (d < best)
                    {
                        best = d;
                        bestStep = i;
                    }
                }

                result.Add((h.Index, bestStep, best));
            }

            return result.OrderBy(t => t.Item3).ThenBy(t => Math.Abs(t.Item2 - 20)).Take(count).ToList();
        }

        /// <summary>32 colours from dark to bright through 2 or 3 stops</summary>
        public static ushort[] Gradient(Color dark, Color? middle, Color bright, double gamma = 1.0)
        {
            var result = new ushort[32];
            for (int i = 0; i < 32; i++)
            {
                double t = Math.Pow(i / 31.0, gamma);
                Color c;
                if (middle.HasValue)
                {
                    c = t < 0.5 ? Lerp(dark, middle.Value, t * 2) : Lerp(middle.Value, bright, (t - 0.5) * 2);
                }
                else
                {
                    c = Lerp(dark, bright, t);
                }

                result[i] = FromColor(c);
            }

            return result;
        }

        private static Color Lerp(Color a, Color b, double t)
        {
            return Color.FromArgb(
                (int)Math.Round(a.R + ((b.R - a.R) * t)),
                (int)Math.Round(a.G + ((b.G - a.G) * t)),
                (int)Math.Round(a.B + ((b.B - a.B) * t)));
        }

        /// <summary>32 colours from the pixels of an image area: sorted by brightness, averaged in 32 groups</summary>
        public static ushort[] FromPixels(IList<Color> pixels, int smooth)
        {
            var result = new ushort[32];
            if (pixels.Count == 0)
            {
                return result;
            }

            List<Color> sorted = pixels.OrderBy(c => (0.299 * c.R) + (0.587 * c.G) + (0.114 * c.B)).ToList();
            var colors = new Color[32];
            for (int i = 0; i < 32; i++)
            {
                int from = (int)((long)i * sorted.Count / 32);
                int to = Math.Max(from + 1, (int)((long)(i + 1) * sorted.Count / 32));
                to = Math.Min(to, sorted.Count);
                from = Math.Min(from, sorted.Count - 1);
                long r = 0, g = 0, b = 0;
                for (int p = from; p < to; p++)
                {
                    r += sorted[p].R;
                    g += sorted[p].G;
                    b += sorted[p].B;
                }

                int n = Math.Max(1, to - from);
                colors[i] = Color.FromArgb((int)(r / n), (int)(g / n), (int)(b / n));
            }

            for (int pass = 0; pass < smooth; pass++)
            {
                var copy = (Color[])colors.Clone();
                for (int i = 1; i < 31; i++)
                {
                    colors[i] = Color.FromArgb(
                        (copy[i - 1].R + (2 * copy[i].R) + copy[i + 1].R) / 4,
                        (copy[i - 1].G + (2 * copy[i].G) + copy[i + 1].G) / 4,
                        (copy[i - 1].B + (2 * copy[i].B) + copy[i + 1].B) / 4);
                }
            }

            for (int i = 0; i < 32; i++)
            {
                result[i] = FromColor(colors[i]);
            }

            return result;
        }
    }

    #endregion

    #region Files

    public static class HueFiles
    {
        /// <summary>Text for ServUO: "Hue = 0x4D2; // name"</summary>
        public static string ServUOText(Hue hue)
        {
            int number = hue.Index + 1;
            return $"Hue = 0x{number:X}; // {number}" + (HueAnalysis.IsUnnamed(hue) ? string.Empty : $" {hue.Name}");
        }

        /// <summary>PNG: one row of 32 squares per hue</summary>
        public static void ExportPng(IEnumerable<Hue> hues, string fileName, int cell = 16)
        {
            List<Hue> list = hues.ToList();
            using (var bmp = new Bitmap(32 * cell, Math.Max(1, list.Count) * cell))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                for (int row = 0; row < list.Count; row++)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        using (var brush = new SolidBrush(HueAnalysis.ToColor(list[row].Colors[i])))
                        {
                            g.FillRectangle(brush, i * cell, row * cell, cell, cell);
                        }
                    }
                }

                bmp.Save(fileName, ImageFormat.Png);
            }
        }

        /// <summary>GIMP palette (.gpl), 32 columns</summary>
        public static void ExportGpl(IEnumerable<Hue> hues, string fileName, string name)
        {
            var sb = new StringBuilder();
            sb.AppendLine("GIMP Palette");
            sb.AppendLine($"Name: {name}");
            sb.AppendLine("Columns: 32");
            sb.AppendLine("#");
            foreach (Hue hue in hues)
            {
                for (int i = 0; i < 32; i++)
                {
                    Color c = HueAnalysis.ToColor(hue.Colors[i]);
                    sb.AppendLine($"{c.R,3} {c.G,3} {c.B,3}\tHue {hue.Index + 1} ({hue.Name}) step {i}");
                }
            }

            File.WriteAllText(fileName, sb.ToString());
        }

        /// <summary>Adobe colour table (.act): max. 256 colours = 8 hues. Returns the number of written hues.</summary>
        public static int ExportAct(IEnumerable<Hue> hues, string fileName)
        {
            List<Hue> list = hues.Take(8).ToList();
            var data = new byte[772];
            int n = 0;
            foreach (Hue hue in list)
            {
                for (int i = 0; i < 32; i++, n++)
                {
                    Color c = HueAnalysis.ToColor(hue.Colors[i]);
                    data[n * 3] = c.R;
                    data[(n * 3) + 1] = c.G;
                    data[(n * 3) + 2] = c.B;
                }
            }

            data[768] = (byte)(n >> 8);
            data[769] = (byte)(n & 0xFF);
            data[770] = 0xFF; // no transparent colour
            data[771] = 0xFF;
            File.WriteAllBytes(fileName, data);
            return list.Count;
        }

        /// <summary>Text file with several hues ("UOHUES 1" + per hue: number, name, table start/end, 32 colours)</summary>
        public static void ExportRange(IEnumerable<Hue> hues, string fileName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("UOHUES 1");
            foreach (Hue h in hues)
            {
                sb.AppendLine($"hue {h.Index + 1}");
                sb.AppendLine(h.Name ?? string.Empty);
                sb.AppendLine(h.TableStart.ToString());
                sb.AppendLine(h.TableEnd.ToString());
                sb.AppendLine(string.Join(" ", h.Colors.Select(c => c.ToString())));
            }

            File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>Reads a file of <see cref="ExportRange"/> (or a single hue export of the hues tab)</summary>
        public static List<(int Number, HueState State)> ImportRange(string fileName)
        {
            var result = new List<(int, HueState)>();
            string[] lines = File.ReadAllLines(fileName);
            if (lines.Length > 0 && lines[0].Trim() == "UOHUES 1")
            {
                for (int i = 1; i + 4 < lines.Length; i++)
                {
                    string head = lines[i].Trim();
                    if (!head.StartsWith("hue ", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    int.TryParse(head.Substring(4), out int number);
                    var state = new HueState { Name = lines[i + 1] };
                    ushort.TryParse(lines[i + 2].Trim(), out ushort ts);
                    ushort.TryParse(lines[i + 3].Trim(), out ushort te);
                    state.TableStart = ts;
                    state.TableEnd = te;
                    string[] values = lines[i + 4].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int c = 0; c < 32 && c < values.Length; c++)
                    {
                        ushort.TryParse(values[c], out state.Colors[c]);
                    }

                    result.Add((number, state));
                    i += 4;
                }

                return result;
            }

            // single hue file of "Export.." (name, table start, table end, 32 lines)
            if (lines.Length >= 35)
            {
                var state = new HueState { Name = lines[0].Trim() };
                ushort.TryParse(lines[1].Trim(), out ushort ts);
                ushort.TryParse(lines[2].Trim(), out ushort te);
                state.TableStart = ts;
                state.TableEnd = te;
                for (int c = 0; c < 32; c++)
                {
                    ushort.TryParse(lines[3 + c].Trim(), out state.Colors[c]);
                }

                result.Add((0, state));
            }

            return result;
        }

        /// <summary>Writes a hue list as hues.mul (8 hues per block, 375 blocks for 3000 hues)</summary>
        public static void SaveMul(Hue[] list, string fileName)
        {
            int blocks = (list.Length + 7) / 8;
            string temp = fileName + ".tmp";
            using (var fs = new FileStream(temp, FileMode.Create, FileAccess.Write))
            using (var bin = new BinaryWriter(fs))
            {
                for (int b = 0; b < blocks; b++)
                {
                    bin.Write(0); // block header (not used by the client)
                    for (int j = 0; j < 8; j++)
                    {
                        int index = (b * 8) + j;
                        Hue hue = index < list.Length ? list[index] : null;
                        for (int c = 0; c < 32; c++)
                        {
                            bin.Write(hue?.Colors[c] ?? (ushort)0);
                        }

                        bin.Write(hue?.TableStart ?? (ushort)0);
                        bin.Write(hue?.TableEnd ?? (ushort)0);

                        var name = new byte[20];
                        if (hue?.Name != null)
                        {
                            byte[] bytes = Encoding.ASCII.GetBytes(hue.Name);
                            Array.Copy(bytes, name, Math.Min(20, bytes.Length));
                        }

                        bin.Write(name);
                    }
                }
            }

            if (File.Exists(fileName))
            {
                File.Copy(fileName, fileName + ".bak", true);
            }

            File.Move(temp, fileName, true);
        }

        /// <summary>Number of hues stored in a hues.mul (8 per 708 byte block)</summary>
        public static int CountInFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return 0;
            }

            long blocks = Math.Min(375, new FileInfo(fileName).Length / 708);
            return (int)(blocks * 8);
        }
    }

    #endregion
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Ultima;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>State of the test preview (pressed button, ticked boxes, typed text).</summary>
    public sealed class PreviewState
    {
        public GumpElement Pressed;
        public GumpElement Hover;
        public readonly Dictionary<GumpElement, bool> Switches = new Dictionary<GumpElement, bool>();
        public readonly Dictionary<GumpElement, string> Texts = new Dictionary<GumpElement, string>();

        public bool IsOn(GumpElement e) => Switches.TryGetValue(e, out bool on) ? on : e.Checked;

        public string TextOf(GumpElement e) => Texts.TryGetValue(e, out string t) ? t : e.Text;
    }

    /// <summary>Draws a gump like the client: graphics from gumpart / art, hues, the client's unicode font.</summary>
    public static class GumpRenderer
    {
        private static readonly Dictionary<long, Bitmap> GumpCache = new Dictionary<long, Bitmap>();
        private static readonly Dictionary<long, Bitmap> ItemCache = new Dictionary<long, Bitmap>();
        private static readonly Dictionary<string, Bitmap> TextCache = new Dictionary<string, Bitmap>();
        private static StringList _cliloc;
        public static readonly object Lock = new object();

        public static void ClearCache()
        {
            lock (Lock)
            {
                foreach (Bitmap b in GumpCache.Values.Concat(ItemCache.Values).Concat(TextCache.Values))
                {
                    b?.Dispose();
                }
                GumpCache.Clear();
                ItemCache.Clear();
                TextCache.Clear();
            }
        }

        // ------------------------------------------------------------------ sources

        /// <summary>Gump picture (32 bit copy, cached) or null.</summary>
        public static Bitmap Gump(int id, int hue = 0)
        {
            if (id < 0 || id > 0xFFFFF)
            {
                return null;
            }
            long key = ((long)id << 16) | (uint)(hue & 0xFFFF);
            lock (Lock)
            {
                if (GumpCache.TryGetValue(key, out Bitmap cached))
                {
                    return cached;
                }
                if (GumpCache.Count > 6000)
                {
                    // the browser can touch thousands of gumps – start over (bitmaps are only drawn on the UI thread)
                    foreach (Bitmap b in GumpCache.Values)
                    {
                        b?.Dispose();
                    }
                    GumpCache.Clear();
                }
                Bitmap result = null;
                try
                {
                    if (Gumps.IsValidIndex(id))
                    {
                        Bitmap src = Gumps.GetGump(id);
                        if (src != null)
                        {
                            result = To32(src);
                            int h = hue & 0x3FFF;
                            if (h > 0 && Hues.List != null && h <= Hues.List.Length)
                            {
                                result = ApplyHue(result, Hues.List[h - 1], false);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    result = null;
                }
                GumpCache[key] = result;
                return result;
            }
        }

        public static Size GumpSize(int id)
        {
            Bitmap b = Gump(id);
            return b?.Size ?? new Size(20, 20);
        }

        public static Bitmap Item(int id, int hue = 0)
        {
            if (id < 0 || id > 0xFFFF)
            {
                return null;
            }
            long key = ((long)id << 16) | (uint)(hue & 0xFFFF);
            lock (Lock)
            {
                if (ItemCache.TryGetValue(key, out Bitmap cached))
                {
                    return cached;
                }
                Bitmap result = null;
                try
                {
                    if (Art.IsValidStatic(id))
                    {
                        Bitmap src = Art.GetStatic(id);
                        if (src != null)
                        {
                            result = To32(src);
                            int h = hue & 0x3FFF;
                            if (h > 0 && Hues.List != null && h <= Hues.List.Length)
                            {
                                bool partial = id < TileData.ItemTable.Length && (TileData.ItemTable[id].Flags & TileFlag.PartialHue) != 0;
                                result = ApplyHue(result, Hues.List[h - 1], partial || (hue & 0x8000) != 0);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    result = null;
                }
                ItemCache[key] = result;
                return result;
            }
        }

        private static Bitmap To32(Bitmap src)
        {
            var b = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.DrawImage(src, new Rectangle(0, 0, src.Width, src.Height));
            }
            return b;
        }

        /// <summary>Hues a 32 bit bitmap in place like the client (gray level → hue color).</summary>
        private static Bitmap ApplyHue(Bitmap b, Hue hue, bool onlyGray)
        {
            BitmapData bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                var px = new int[b.Width * b.Height];
                Marshal.Copy(bd.Scan0, px, 0, px.Length);
                for (int i = 0; i < px.Length; i++)
                {
                    int p = px[i];
                    if (((p >> 24) & 0xFF) == 0)
                    {
                        continue;
                    }
                    int r = (p >> 19) & 0x1F, g = (p >> 11) & 0x1F, bl = (p >> 3) & 0x1F;
                    if (onlyGray && !(r == g && g == bl))
                    {
                        continue;
                    }
                    int c = hue.Colors[r];
                    int rr = (c >> 10) & 0x1F, gg = (c >> 5) & 0x1F, bb = c & 0x1F;
                    px[i] = (p & unchecked((int)0xFF000000)) | (((rr << 3) | (rr >> 2)) << 16) | (((gg << 3) | (gg >> 2)) << 8) | ((bb << 3) | (bb >> 2));
                }
                Marshal.Copy(px, 0, bd.Scan0, px.Length);
            }
            finally
            {
                b.UnlockBits(bd);
            }
            return b;
        }

        /// <summary>Color of text with a hue (0 = the dark default text color).</summary>
        public static Color HueColor(int hue)
        {
            int h = hue & 0x3FFF;
            try
            {
                if (h > 0 && Hues.List != null && h <= Hues.List.Length)
                {
                    int c = Hues.List[h - 1].Colors[24];
                    int r = (c >> 10) & 0x1F, g = (c >> 5) & 0x1F, b = c & 0x1F;
                    return Color.FromArgb((r << 3) | (r >> 2), (g << 3) | (g >> 2), (b << 3) | (b >> 2));
                }
            }
            catch (Exception)
            {
                // hues missing
            }
            return Color.FromArgb(20, 20, 20);
        }

        /// <summary>15 bit color (AddHtmlLocalized color) → RGB.</summary>
        public static Color Color555(int c)
        {
            if (c <= 0)
            {
                return Color.FromArgb(20, 20, 20);
            }
            int r = (c >> 10) & 0x1F, g = (c >> 5) & 0x1F, b = c & 0x1F;
            return Color.FromArgb((r << 3) | (r >> 2), (g << 3) | (g >> 2), (b << 3) | (b >> 2));
        }

        public static string ClilocText(int number)
        {
            lock (Lock)
            {
                try
                {
                    if (_cliloc == null)
                    {
                        _cliloc = new StringList("enu", UoFiddler.Controls.Classes.Options.NewClilocFormat);
                    }
                    StringEntry e = _cliloc.Entries.FirstOrDefault(x => x.Number == number);
                    return e?.Text ?? $"[cliloc {number}]";
                }
                catch (Exception)
                {
                    return $"[cliloc {number}]";
                }
            }
        }

        // ------------------------------------------------------------------ text

        /// <summary>Text in the client's unicode font 1 (like gump labels), colored; falls back to Tahoma.</summary>
        public static Bitmap Text(string text, Color color, bool bold = false)
        {
            text ??= string.Empty;
            string key = $"{color.ToArgb()}|{bold}|{text}";
            lock (Lock)
            {
                if (TextCache.TryGetValue(key, out Bitmap cached))
                {
                    return cached;
                }
                if (TextCache.Count > 2000)
                {
                    foreach (Bitmap b in TextCache.Values)
                    {
                        b.Dispose();
                    }
                    TextCache.Clear();
                }
                Bitmap result = RenderUnicode(text, color, bold) ?? RenderGdi(text, color, bold);
                TextCache[key] = result;
                return result;
            }
        }

        private static Bitmap RenderUnicode(string text, Color color, bool bold)
        {
            try
            {
                UnicodeFont font = UnicodeFonts.Fonts.Length > 1 ? UnicodeFonts.Fonts[1] : null;
                if (font == null)
                {
                    return null;
                }
                int width = 2, height = 2;
                foreach (char ch in text)
                {
                    UnicodeChar uc = font.Chars[ch];
                    if (uc == null)
                    {
                        width += 4;
                        continue;
                    }
                    width += uc.Width == 0 ? 4 : uc.Width + uc.XOffset + 1;
                    height = Math.Max(height, uc.Height + uc.YOffset + 2);
                }
                height = Math.Max(height, 16);
                var bmp = new Bitmap(Math.Max(1, width + (bold ? 1 : 0)), height, PixelFormat.Format32bppArgb);
                int argb = color.ToArgb();
                BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                var px = new int[bmp.Width * bmp.Height];
                int dx = 0;
                foreach (char ch in text)
                {
                    UnicodeChar uc = font.Chars[ch];
                    if (uc == null || uc.Width == 0 || uc.Bytes == null)
                    {
                        dx += 4;
                        continue;
                    }
                    dx += uc.XOffset;
                    int stride = (uc.Width - 1) / 8 + 1;
                    for (int y = 0; y < uc.Height; y++)
                    {
                        for (int x = 0; x < uc.Width; x++)
                        {
                            int offset = x / 8 + y * stride;
                            if (offset < uc.Bytes.Length && (uc.Bytes[offset] & (1 << (7 - x % 8))) != 0)
                            {
                                int tx = dx + x, ty = uc.YOffset + y;
                                if (tx >= 0 && ty >= 0 && tx < bmp.Width && ty < bmp.Height)
                                {
                                    px[ty * bmp.Width + tx] = argb;
                                    if (bold && tx + 1 < bmp.Width)
                                    {
                                        px[ty * bmp.Width + tx + 1] = argb;
                                    }
                                }
                            }
                        }
                    }
                    dx += uc.Width + 1;
                }
                Marshal.Copy(px, 0, bd.Scan0, px.Length);
                bmp.UnlockBits(bd);
                return bmp;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static Bitmap RenderGdi(string text, Color color, bool bold)
        {
            using (var font = new Font("Tahoma", 8.5f, bold ? FontStyle.Bold : FontStyle.Regular))
            {
                Size size = System.Windows.Forms.TextRenderer.MeasureText(text.Length == 0 ? " " : text, font, Size.Empty, System.Windows.Forms.TextFormatFlags.NoPadding);
                var bmp = new Bitmap(Math.Max(1, size.Width), Math.Max(14, size.Height), PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                    using (var b = new SolidBrush(color))
                    {
                        g.DrawString(text, font, b, 0, 0, StringFormat.GenericTypographic);
                    }
                }
                return bmp;
            }
        }

        // ------------------------------------------------------------------ html

        private sealed class Run
        {
            public string Text;
            public Color Color;
            public bool Bold;
            public bool Underline;
            public bool NewLine;
            public int Align; // 0 left, 1 center, 2 right
        }

        private static readonly Dictionary<string, Color> NamedColors = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
        {
            ["WHITE"] = Color.White, ["BLACK"] = Color.Black, ["RED"] = Color.Red, ["GREEN"] = Color.Lime, ["BLUE"] = Color.Blue,
            ["YELLOW"] = Color.Yellow, ["GRAY"] = Color.Gray, ["GREY"] = Color.Gray, ["ORANGE"] = Color.Orange, ["PURPLE"] = Color.Purple
        };

        private static List<Run> ParseHtml(string html, Color baseColor)
        {
            var runs = new List<Run>();
            var colors = new Stack<Color>();
            Color color = baseColor;
            bool bold = false, underline = false;
            int align = 0;
            foreach (Match m in Regex.Matches(html ?? string.Empty, @"<[^>]*>|[^<]+"))
            {
                string t = m.Value;
                if (!t.StartsWith("<", StringComparison.Ordinal))
                {
                    string text = System.Net.WebUtility.HtmlDecode(t.Replace("\r", string.Empty).Replace("\n", " "));
                    foreach (string word in Regex.Split(text, @"(?<= )"))
                    {
                        if (word.Length > 0)
                        {
                            runs.Add(new Run { Text = word, Color = color, Bold = bold, Underline = underline, Align = align });
                        }
                    }
                    continue;
                }
                string tag = t.Trim('<', '>', ' ').ToUpperInvariant();
                if (tag.StartsWith("BASEFONT", StringComparison.Ordinal) || tag.StartsWith("FONT", StringComparison.Ordinal))
                {
                    Match c = Regex.Match(tag, @"COLOR\s*=\s*[""']?#?([0-9A-F]{6}|\w+)");
                    if (c.Success)
                    {
                        colors.Push(color);
                        string v = c.Groups[1].Value;
                        color = v.Length == 6 && int.TryParse(v, System.Globalization.NumberStyles.HexNumber, null, out int rgb)
                            ? Color.FromArgb(255, Color.FromArgb(rgb))
                            : NamedColors.TryGetValue(v, out Color named) ? named : color;
                    }
                }
                else if (tag.StartsWith("/BASEFONT", StringComparison.Ordinal) || tag.StartsWith("/FONT", StringComparison.Ordinal))
                {
                    if (colors.Count > 0)
                    {
                        color = colors.Pop();
                    }
                }
                else if (tag == "BR" || tag == "BR/" || tag == "P" || tag == "/P" || tag == "/DIV")
                {
                    runs.Add(new Run { NewLine = true, Align = align });
                    if (tag == "/DIV")
                    {
                        align = 0;
                    }
                }
                else if (tag == "B" || tag == "STRONG")
                {
                    bold = true;
                }
                else if (tag == "/B" || tag == "/STRONG")
                {
                    bold = false;
                }
                else if (tag == "U" || tag.StartsWith("A ", StringComparison.Ordinal))
                {
                    underline = true;
                }
                else if (tag == "/U" || tag == "/A")
                {
                    underline = false;
                }
                else if (tag == "CENTER")
                {
                    align = 1;
                }
                else if (tag == "/CENTER")
                {
                    runs.Add(new Run { NewLine = true, Align = 1 });
                    align = 0;
                }
                else if (tag.StartsWith("DIV", StringComparison.Ordinal) && tag.Contains("RIGHT"))
                {
                    align = 2;
                }
                else if (tag.StartsWith("DIV", StringComparison.Ordinal) && tag.Contains("CENTER"))
                {
                    align = 1;
                }
            }
            return runs;
        }

        /// <summary>Simple HTML layout of the client: words wrapped at the width, BASEFONT colors, CENTER, BR, B, U.</summary>
        public static void DrawHtml(Graphics g, Rectangle r, string html, Color baseColor)
        {
            List<Run> runs = ParseHtml(html, baseColor);
            var lines = new List<List<(Bitmap Bmp, Run Run)>>();
            var line = new List<(Bitmap, Run)>();
            var aligns = new List<int>();
            int lineWidth = 0, maxW = Math.Max(10, r.Width - 4);
            int currentAlign = 0;
            foreach (Run run in runs)
            {
                if (run.NewLine)
                {
                    lines.Add(line);
                    aligns.Add(line.Count > 0 ? line[0].Item2.Align : run.Align);
                    line = new List<(Bitmap, Run)>();
                    lineWidth = 0;
                    continue;
                }
                Bitmap bmp = Text(run.Text, run.Color, run.Bold);
                if (lineWidth + bmp.Width > maxW && line.Count > 0)
                {
                    lines.Add(line);
                    aligns.Add(line[0].Item2.Align);
                    line = new List<(Bitmap, Run)>();
                    lineWidth = 0;
                }
                line.Add((bmp, run));
                lineWidth += bmp.Width;
                currentAlign = run.Align;
            }
            if (line.Count > 0)
            {
                lines.Add(line);
                aligns.Add(line[0].Item2.Align);
            }
            GraphicsState state = g.Save();
            g.SetClip(r, CombineMode.Intersect);
            int y = r.Y + 2;
            for (int i = 0; i < lines.Count; i++)
            {
                int w = lines[i].Sum(x => x.Bmp.Width);
                int x = aligns[i] == 1 ? r.X + (r.Width - w) / 2 : aligns[i] == 2 ? r.Right - w - 2 : r.X + 2;
                int h = 16;
                foreach ((Bitmap bmp, Run run) in lines[i])
                {
                    g.DrawImageUnscaled(bmp, x, y);
                    if (run.Underline)
                    {
                        using (var pen = new Pen(run.Color))
                        {
                            g.DrawLine(pen, x, y + bmp.Height - 2, x + bmp.Width, y + bmp.Height - 2);
                        }
                    }
                    x += bmp.Width;
                    h = Math.Max(h, bmp.Height);
                }
                y += h;
                if (y > r.Bottom)
                {
                    break;
                }
            }
            g.Restore(state);
        }

        // ------------------------------------------------------------------ elements

        private static void DrawTiled(Graphics g, Bitmap tile, Rectangle area)
        {
            if (tile == null || area.Width <= 0 || area.Height <= 0)
            {
                return;
            }
            using (var brush = new TextureBrush(tile, WrapMode.Tile))
            {
                brush.TranslateTransform(area.X, area.Y);
                g.FillRectangle(brush, area);
            }
        }

        /// <summary>The 9 part frame: corners, tiled edges and center (gump IDs id … id + 8).</summary>
        public static void DrawBackground(Graphics g, int id, Rectangle r)
        {
            Bitmap[] p = Enumerable.Range(0, 9).Select(i => Gump(id + i)).ToArray();
            if (p.All(b => b == null))
            {
                DrawMissing(g, r, $"background {id}");
                return;
            }
            Size S(int i) => p[i]?.Size ?? Size.Empty;
            int left = Math.Max(S(0).Width, Math.Max(S(3).Width, S(6).Width));
            int right = Math.Max(S(2).Width, Math.Max(S(5).Width, S(8).Width));
            int top = Math.Max(S(0).Height, Math.Max(S(1).Height, S(2).Height));
            int bottom = Math.Max(S(6).Height, Math.Max(S(7).Height, S(8).Height));
            GraphicsState st = g.Save();
            g.SetClip(r, CombineMode.Intersect);
            DrawTiled(g, p[4], new Rectangle(r.X + S(3).Width, r.Y + S(1).Height, r.Width - S(3).Width - S(5).Width, r.Height - S(1).Height - S(7).Height));
            DrawTiled(g, p[1], new Rectangle(r.X + S(0).Width, r.Y, r.Width - S(0).Width - S(2).Width, S(1).Height));
            DrawTiled(g, p[7], new Rectangle(r.X + S(6).Width, r.Bottom - S(7).Height, r.Width - S(6).Width - S(8).Width, S(7).Height));
            DrawTiled(g, p[3], new Rectangle(r.X, r.Y + S(0).Height, S(3).Width, r.Height - S(0).Height - S(6).Height));
            DrawTiled(g, p[5], new Rectangle(r.Right - S(5).Width, r.Y + S(2).Height, S(5).Width, r.Height - S(2).Height - S(8).Height));
            if (p[0] != null) g.DrawImageUnscaled(p[0], r.X, r.Y);
            if (p[2] != null) g.DrawImageUnscaled(p[2], r.Right - S(2).Width, r.Y);
            if (p[6] != null) g.DrawImageUnscaled(p[6], r.X, r.Bottom - S(6).Height);
            if (p[8] != null) g.DrawImageUnscaled(p[8], r.Right - S(8).Width, r.Bottom - S(8).Height);
            g.Restore(st);
            _ = left + right + top + bottom;
        }

        private static void DrawMissing(Graphics g, Rectangle r, string text)
        {
            using (var b = new HatchBrush(HatchStyle.WideUpwardDiagonal, Color.FromArgb(160, 255, 0, 255), Color.FromArgb(60, 255, 0, 255)))
            {
                g.FillRectangle(b, r);
            }
            using (var f = new Font("Segoe UI", 7f))
            {
                g.DrawString(text, f, Brushes.White, r.X + 2, r.Y + 2);
            }
        }

        /// <summary>Size of an element on screen (for selection and hit tests).</summary>
        public static Rectangle Measure(GumpElement e)
        {
            switch (e.Type)
            {
                case ElementType.Image:
                    return new Rectangle(new Point(e.X, e.Y), GumpSize(e.GumpId));
                case ElementType.Button:
                case ElementType.Check:
                case ElementType.Radio:
                {
                    Size a = GumpSize(e.GumpId), b = GumpSize(e.GumpId2);
                    return new Rectangle(e.X, e.Y, Math.Max(a.Width, b.Width), Math.Max(a.Height, b.Height));
                }
                case ElementType.Label:
                {
                    Bitmap t = Text(string.IsNullOrEmpty(e.Text) ? " " : e.Text, HueColor(e.Hue));
                    return new Rectangle(e.X, e.Y, Math.Max(8, t.Width), Math.Max(14, t.Height));
                }
                case ElementType.Item:
                {
                    Bitmap i = Item(e.ItemId);
                    return new Rectangle(e.X, e.Y, i?.Width ?? 20, i?.Height ?? 20);
                }
                default:
                    return new Rectangle(e.X, e.Y, Math.Max(4, e.Width), Math.Max(4, e.Height));
            }
        }

        /// <summary>Draws one element; designer = helper outlines for invisible things (text entries, alpha regions …).</summary>
        public static void DrawElement(Graphics g, GumpElement e, bool designer, PreviewState state)
        {
            Rectangle r = Measure(e);
            e.Bounds = r;
            switch (e.Type)
            {
                case ElementType.Background:
                    DrawBackground(g, e.GumpId, r);
                    break;
                case ElementType.Image:
                {
                    Bitmap b = Gump(e.GumpId, e.Hue);
                    if (b != null) g.DrawImageUnscaled(b, e.X, e.Y);
                    else DrawMissing(g, r, $"gump {e.GumpId}");
                    break;
                }
                case ElementType.ImageTiled:
                {
                    Bitmap b = Gump(e.GumpId);
                    if (b != null) DrawTiled(g, b, r);
                    else DrawMissing(g, r, $"gump {e.GumpId}");
                    break;
                }
                case ElementType.SpriteImage:
                {
                    Bitmap b = Gump(e.GumpId);
                    if (b != null) g.DrawImage(b, r, new Rectangle(e.SourceX, e.SourceY, e.Width, e.Height), GraphicsUnit.Pixel);
                    else DrawMissing(g, r, $"gump {e.GumpId}");
                    break;
                }
                case ElementType.AlphaRegion:
                    using (var b = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                    {
                        g.FillRectangle(b, r);
                    }
                    if (designer)
                    {
                        using (var pen = new Pen(Color.FromArgb(120, 0, 200, 255)) { DashStyle = DashStyle.Dot })
                        {
                            g.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
                        }
                    }
                    break;
                case ElementType.Label:
                    g.DrawImageUnscaled(Text(e.Text ?? string.Empty, HueColor(e.Hue)), e.X, e.Y);
                    break;
                case ElementType.LabelCropped:
                {
                    GraphicsState st = g.Save();
                    g.SetClip(r, CombineMode.Intersect);
                    g.DrawImageUnscaled(Text(e.Text ?? string.Empty, HueColor(e.Hue)), e.X, e.Y);
                    g.Restore(st);
                    break;
                }
                case ElementType.Html:
                case ElementType.HtmlLocalized:
                {
                    Rectangle textArea = r;
                    if (e.HtmlBackground)
                    {
                        DrawBackground(g, 3000, r);
                    }
                    if (e.Scrollbar)
                    {
                        textArea.Width -= 16;
                        using (var b = new SolidBrush(Color.FromArgb(150, 120, 100, 70)))
                        {
                            g.FillRectangle(b, r.Right - 15, r.Y, 15, r.Height);
                        }
                        g.DrawRectangle(Pens.DimGray, r.Right - 15, r.Y, 14, r.Height - 1);
                    }
                    string html = e.Type == ElementType.Html ? e.Text : ClilocText(e.Cliloc);
                    Color baseColor = e.Type == ElementType.HtmlLocalized ? Color555(e.Hue) : Color.FromArgb(20, 20, 20);
                    DrawHtml(g, textArea, html, baseColor);
                    if (designer)
                    {
                        using (var pen = new Pen(Color.FromArgb(90, 255, 255, 0)) { DashStyle = DashStyle.Dot })
                        {
                            g.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
                        }
                    }
                    break;
                }
                case ElementType.Button:
                {
                    bool down = state != null && (state.Pressed == e || state.Hover == e && state.Pressed == e);
                    Bitmap b = Gump(down ? e.GumpId2 : e.GumpId) ?? Gump(e.GumpId);
                    if (b != null) g.DrawImageUnscaled(b, e.X, e.Y);
                    else DrawMissing(g, r, $"button {e.GumpId}");
                    break;
                }
                case ElementType.ItemButton:
                {
                    bool down = state != null && state.Pressed == e;
                    Bitmap b = Gump(down ? e.GumpId2 : e.GumpId) ?? Gump(e.GumpId);
                    if (b != null) DrawTiled(g, b, r);
                    Bitmap item = Item(e.ItemId, e.Hue);
                    if (item != null)
                    {
                        g.DrawImageUnscaled(item, r.X + (r.Width - item.Width) / 2, r.Y + (r.Height - item.Height) / 2);
                    }
                    break;
                }
                case ElementType.Check:
                case ElementType.Radio:
                {
                    bool on = state?.IsOn(e) ?? e.Checked;
                    Bitmap b = Gump(on ? e.GumpId2 : e.GumpId);
                    if (b != null) g.DrawImageUnscaled(b, e.X, e.Y);
                    else DrawMissing(g, r, e.Type == ElementType.Check ? "check" : "radio");
                    break;
                }
                case ElementType.TextEntry:
                {
                    string text = state?.TextOf(e) ?? e.Text;
                    GraphicsState st = g.Save();
                    g.SetClip(r, CombineMode.Intersect);
                    g.DrawImageUnscaled(Text(text ?? string.Empty, HueColor(e.Hue)), e.X, e.Y);
                    g.Restore(st);
                    if (designer || state != null)
                    {
                        using (var pen = new Pen(Color.FromArgb(140, 255, 160, 0)) { DashStyle = DashStyle.Dash })
                        {
                            g.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
                        }
                    }
                    break;
                }
                case ElementType.Item:
                {
                    Bitmap b = Item(e.ItemId, e.Hue);
                    if (b != null) g.DrawImageUnscaled(b, e.X, e.Y);
                    else DrawMissing(g, r, $"item {e.ItemId:X}");
                    break;
                }
            }
        }

        /// <summary>Elements of a page in draw order (page 0 = on every page).</summary>
        public static IEnumerable<GumpElement> Visible(GumpDesign d, int page, bool designer) =>
            d.Elements.Where(e => (e.Page == 0 || e.Page == page) && (!designer || !e.Hidden));

        public static void DrawDesign(Graphics g, GumpDesign d, int page, bool designer, PreviewState state)
        {
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            foreach (GumpElement e in Visible(d, page, designer))
            {
                DrawElement(g, e, designer, state);
            }
        }

        /// <summary>Picture of the gump (cut to the used area) – for PNG export.</summary>
        public static Bitmap RenderImage(GumpDesign d, int page)
        {
            List<GumpElement> els = Visible(d, page, false).ToList();
            if (els.Count == 0)
            {
                return null;
            }
            Rectangle all = els.Select(Measure).Aggregate(Rectangle.Union);
            var bmp = new Bitmap(Math.Max(1, all.Right), Math.Max(1, all.Bottom), PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                DrawDesign(g, d, page, false, null);
            }
            if (all.X <= 0 && all.Y <= 0)
            {
                return bmp;
            }
            Bitmap cut = bmp.Clone(Rectangle.Intersect(all, new Rectangle(0, 0, bmp.Width, bmp.Height)), PixelFormat.Format32bppArgb);
            bmp.Dispose();
            return cut;
        }
    }
}

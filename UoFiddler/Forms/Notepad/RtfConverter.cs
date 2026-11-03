/***************************************************************************
 *
 * $Author: Nikodemus
 * Coder: Nikodemus
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace UoFiddler.Forms.Notepad
{
    /// <summary>
    /// Converts note content (RTF) to HTML / Markdown and imports Markdown.
    /// </summary>
    internal static class RtfConverter
    {
        public const float H1Size = 20f;
        public const float H2Size = 16f;
        public const float H3Size = 13f;

        private const uint RunMask = RichEditNative.CFM_BOLD | RichEditNative.CFM_ITALIC | RichEditNative.CFM_UNDERLINE |
                                     RichEditNative.CFM_STRIKEOUT | RichEditNative.CFM_COLOR | RichEditNative.CFM_SIZE |
                                     RichEditNative.CFM_FACE | RichEditNative.CFM_BACKCOLOR;

        private sealed class Run
        {
            public string Text;
            public RichEditNative.CHARFORMAT2 Format;
            public bool Bold => (Format.dwEffects & RichEditNative.CFM_BOLD) != 0;
            public bool Italic => (Format.dwEffects & RichEditNative.CFM_ITALIC) != 0;
            public bool Underline => (Format.dwEffects & RichEditNative.CFM_UNDERLINE) != 0;
            public bool Strike => (Format.dwEffects & RichEditNative.CFM_STRIKEOUT) != 0;
            public float Size => Format.yHeight / 20f;
        }

        private sealed class Paragraph
        {
            public List<Run> Runs = new();
            public HorizontalAlignment Alignment;
            public short Numbering;
            public string Text => string.Concat(Runs.ConvertAll(r => r.Text));
        }

        private static List<Paragraph> Read(string rtf, string plain)
        {
            using var box = new RichTextBox();
            box.CreateControl();
            _ = box.Handle;
            try
            {
                box.Rtf = rtf;
            }
            catch (ArgumentException)
            {
                box.Text = plain ?? "";
            }

            var result = new List<Paragraph>();
            string text = box.Text;
            int pos = 0;
            while (pos <= text.Length)
            {
                int end = text.IndexOf('\n', pos);
                if (end < 0)
                {
                    end = text.Length;
                }

                var para = new Paragraph();
                box.Select(pos, 0);
                para.Alignment = box.SelectionAlignment;
                para.Numbering = RichEditNative.GetNumbering(box);

                int s = pos;
                while (s < end)
                {
                    int e = RunEnd(box, s, end);
                    box.Select(s, e - s);
                    para.Runs.Add(new Run { Text = text.Substring(s, e - s), Format = RichEditNative.GetCharFormat(box) });
                    s = e;
                }

                result.Add(para);
                if (end >= text.Length)
                {
                    break;
                }

                pos = end + 1;
            }

            return result;
        }

        /// <summary>
        /// End of the run starting at start: largest end where the whole range has one format.
        /// </summary>
        private static int RunEnd(RichTextBox box, int start, int limit)
        {
            bool Same(int end)
            {
                box.Select(start, end - start);
                return (RichEditNative.GetCharFormat(box).dwMask & RunMask) == RunMask;
            }

            int good = start + 1, step = 1;
            while (good + step <= limit && Same(good + step))
            {
                good += step;
                step *= 2;
            }

            int lo = good, hi = Math.Min(limit, good + step);
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (Same(mid))
                {
                    lo = mid;
                }
                else
                {
                    hi = mid - 1;
                }
            }

            return lo;
        }

        private static string Css(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        #region HTML

        public static string ToHtml(Note note)
        {
            List<Paragraph> paragraphs = Read(note.Rtf, note.PlainText);
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset=\"utf-8\">");
            sb.AppendLine($"<title>{WebUtility.HtmlEncode(note.Title)}</title>");
            sb.AppendLine("<style>body{font-family:'Segoe UI',Arial,sans-serif;max-width:900px;margin:32px auto;padding:0 16px;line-height:1.45;color:#222}" +
                          "p{margin:0;min-height:1em}ul,ol{margin:0 0 0 1.4em;padding:0}.meta{color:#888;font-size:9pt;margin-bottom:18px}" +
                          (note.CodeMode ? "body{font-family:Consolas,monospace;white-space:pre}" : "") + "</style>");
            sb.AppendLine("</head><body>");
            sb.AppendLine($"<h1>{WebUtility.HtmlEncode(note.Title)}</h1>");
            string meta = $"{note.Modified:g}" + (note.Category.Length > 0 ? " · " + note.Category : "") +
                          (note.Tags.Count > 0 ? " · " + note.TagText : "");
            sb.AppendLine($"<div class=\"meta\">{WebUtility.HtmlEncode(meta)}</div>");

            string openList = null;
            foreach (Paragraph p in paragraphs)
            {
                string list = p.Numbering == RichEditNative.PFN_BULLET ? "ul" : p.Numbering == RichEditNative.PFN_ARABIC ? "ol" : null;
                if (list != openList)
                {
                    if (openList != null)
                    {
                        sb.AppendLine($"</{openList}>");
                    }

                    if (list != null)
                    {
                        sb.AppendLine($"<{list}>");
                    }

                    openList = list;
                }

                string align = p.Alignment switch
                {
                    HorizontalAlignment.Center => " style=\"text-align:center\"",
                    HorizontalAlignment.Right => " style=\"text-align:right\"",
                    _ => ""
                };
                string tag = list != null ? "li" : "p";
                sb.Append($"<{tag}{align}>");
                if (p.Runs.Count == 0)
                {
                    sb.Append("&nbsp;");
                }

                foreach (Run r in p.Runs)
                {
                    sb.Append(RunToHtml(r, note.CodeMode));
                }

                sb.AppendLine($"</{tag}>");
            }

            if (openList != null)
            {
                sb.AppendLine($"</{openList}>");
            }

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static string RunToHtml(Run r, bool code)
        {
            string text = WebUtility.HtmlEncode(r.Text).Replace("\t", "&emsp;").Replace("￼", "[image]");
            var style = new List<string>();
            if (!code && !string.IsNullOrEmpty(r.Format.szFaceName))
            {
                style.Add($"font-family:'{r.Format.szFaceName}'");
            }

            if (r.Format.yHeight > 0)
            {
                style.Add($"font-size:{r.Size.ToString("0.#", CultureInfo.InvariantCulture)}pt");
            }

            if ((r.Format.dwEffects & RichEditNative.CFE_AUTOCOLOR) == 0)
            {
                Color c = ColorTranslator.FromWin32(r.Format.crTextColor);
                if (c.ToArgb() != Color.Black.ToArgb())
                {
                    style.Add("color:" + Css(c));
                }
            }

            if ((r.Format.dwEffects & RichEditNative.CFE_AUTOBACKCOLOR) == 0)
            {
                Color c = ColorTranslator.FromWin32(r.Format.crBackColor);
                if (c.ToArgb() != Color.White.ToArgb())
                {
                    style.Add("background:" + Css(c));
                }
            }

            if (r.Bold)
            {
                text = $"<b>{text}</b>";
            }

            if (r.Italic)
            {
                text = $"<i>{text}</i>";
            }

            if (r.Underline)
            {
                text = $"<u>{text}</u>";
            }

            if (r.Strike)
            {
                text = $"<s>{text}</s>";
            }

            return style.Count > 0 ? $"<span style=\"{string.Join(";", style)}\">{text}</span>" : text;
        }

        #endregion

        #region Markdown

        public static string ToMarkdown(Note note)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# " + note.Title);
            sb.AppendLine();

            if (note.CodeMode)
            {
                sb.AppendLine("```csharp");
                sb.AppendLine((note.PlainText ?? "").Replace("\n", Environment.NewLine));
                sb.AppendLine("```");
                return sb.ToString();
            }

            int number = 0;
            foreach (Paragraph p in Read(note.Rtf, note.PlainText))
            {
                string plain = p.Text;
                string prefix = "";
                bool heading = false;
                float size = p.Runs.Count > 0 ? p.Runs[0].Size : 0;

                if (p.Numbering == RichEditNative.PFN_ARABIC)
                {
                    prefix = $"{++number}. ";
                }
                else
                {
                    number = 0;
                    if (p.Numbering == RichEditNative.PFN_BULLET)
                    {
                        prefix = "- ";
                    }
                }

                if (prefix.Length == 0 && plain.Trim().Length > 0)
                {
                    if (size >= H1Size - 0.5f)
                    {
                        prefix = "## ";
                        heading = true;
                    }
                    else if (size >= H2Size - 0.5f)
                    {
                        prefix = "### ";
                        heading = true;
                    }
                    else if (size >= H3Size - 0.5f && p.Runs.TrueForAll(r => r.Bold || r.Text.Trim().Length == 0))
                    {
                        prefix = "#### ";
                        heading = true;
                    }
                }

                var line = new StringBuilder();
                foreach (Run r in p.Runs)
                {
                    string t = r.Text.Replace("￼", "[image]");
                    if (t.Trim().Length == 0 || heading)
                    {
                        line.Append(t);
                        continue;
                    }

                    string lead = t.Substring(0, t.Length - t.TrimStart().Length);
                    string trail = t.Substring(t.TrimEnd().Length);
                    string core = t.Trim();
                    if (r.Strike)
                    {
                        core = $"~~{core}~~";
                    }

                    if (r.Italic)
                    {
                        core = $"*{core}*";
                    }

                    if (r.Bold)
                    {
                        core = $"**{core}**";
                    }

                    line.Append(lead).Append(core).Append(trail);
                }

                string result = line.ToString();
                result = result.StartsWith(NoteRichTextBox.Unchecked + " ") ? "- [ ] " + result.Substring(2)
                    : result.StartsWith(NoteRichTextBox.Checked + " ") ? "- [x] " + result.Substring(2)
                    : result;
                sb.AppendLine(prefix + result);
            }

            return sb.ToString();
        }

        private static readonly Regex _inline = new(@"(\*\*(.+?)\*\*|__(.+?)__|~~(.+?)~~|\*(.+?)\*|_(.+?)_|`(.+?)`)", RegexOptions.Compiled);

        /// <summary>
        /// Builds formatted content from Markdown (headings, lists, checklists, bold, italic, strike, code).
        /// </summary>
        public static string MarkdownToRtf(string markdown, string fontName, float fontSize)
        {
            using var box = new RichTextBox();
            box.CreateControl();
            _ = box.Handle;
            box.Font = new Font(fontName, fontSize);

            bool inCode = false;
            string[] lines = markdown.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                bool last = i == lines.Length - 1;

                if (line.TrimStart().StartsWith("```"))
                {
                    inCode = !inCode;
                    continue;
                }

                if (inCode)
                {
                    Append(box, line, fontSize, 0, "Consolas");
                    if (!last)
                    {
                        Append(box, "\n", fontSize, 0, null);
                    }

                    continue;
                }

                int lineStart = box.TextLength;
                short numbering = 0;
                float size = fontSize;
                bool bold = false;

                Match h = Regex.Match(line, @"^(#{1,6})\s+(.*)$");
                if (h.Success)
                {
                    int level = h.Groups[1].Length;
                    size = level <= 1 ? H1Size : level == 2 ? H1Size : level == 3 ? H2Size : H3Size;
                    bold = true;
                    line = h.Groups[2].Value;
                }
                else if (Regex.IsMatch(line, @"^\s*[-*+]\s+\[[ xX]\]\s"))
                {
                    bool done = Regex.IsMatch(line, @"^\s*[-*+]\s+\[[xX]\]");
                    line = (done ? NoteRichTextBox.Checked : NoteRichTextBox.Unchecked) + " " + Regex.Replace(line, @"^\s*[-*+]\s+\[[ xX]\]\s", "");
                }
                else if (Regex.IsMatch(line, @"^\s*[-*+]\s+"))
                {
                    numbering = RichEditNative.PFN_BULLET;
                    line = Regex.Replace(line, @"^\s*[-*+]\s+", "");
                }
                else if (Regex.IsMatch(line, @"^\s*\d+[.)]\s+"))
                {
                    numbering = RichEditNative.PFN_ARABIC;
                    line = Regex.Replace(line, @"^\s*\d+[.)]\s+", "");
                }

                int pos = 0;
                foreach (Match m in _inline.Matches(line))
                {
                    Append(box, line.Substring(pos, m.Index - pos), size, bold ? RichEditNative.CFM_BOLD : 0, null);
                    if (m.Groups[2].Success || m.Groups[3].Success)
                    {
                        Append(box, m.Groups[2].Success ? m.Groups[2].Value : m.Groups[3].Value, size, RichEditNative.CFM_BOLD, null);
                    }
                    else if (m.Groups[4].Success)
                    {
                        Append(box, m.Groups[4].Value, size, RichEditNative.CFM_STRIKEOUT | (bold ? RichEditNative.CFM_BOLD : 0), null);
                    }
                    else if (m.Groups[5].Success || m.Groups[6].Success)
                    {
                        Append(box, m.Groups[5].Success ? m.Groups[5].Value : m.Groups[6].Value, size, RichEditNative.CFM_ITALIC | (bold ? RichEditNative.CFM_BOLD : 0), null);
                    }
                    else
                    {
                        Append(box, m.Groups[7].Value, size, 0, "Consolas");
                    }

                    pos = m.Index + m.Length;
                }

                Append(box, line.Substring(pos), size, bold ? RichEditNative.CFM_BOLD : 0, null);

                if (numbering != 0)
                {
                    box.Select(lineStart, 0);
                    RichEditNative.SetNumbering(box, numbering);
                }

                if (!last)
                {
                    Append(box, "\n", fontSize, 0, null);
                }
            }

            return box.Rtf;
        }

        private static void Append(RichTextBox box, string text, float size, uint effects, string face)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            int start = box.TextLength;
            box.Select(start, 0);
            box.SelectedText = text;
            box.Select(start, text.Length);
            RichEditNative.SetFontSize(box, size);
            RichEditNative.SetEffect(box, RichEditNative.CFM_BOLD | RichEditNative.CFM_ITALIC | RichEditNative.CFM_STRIKEOUT, false);
            if (effects != 0)
            {
                RichEditNative.SetEffect(box, effects, true);
            }

            RichEditNative.SetFontFace(box, face ?? box.Font.Name);
            box.Select(box.TextLength, 0);
        }

        #endregion
    }
}

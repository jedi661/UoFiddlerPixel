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
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace UoFiddler.Forms.Notepad
{
    /// <summary>
    /// RichTextBox for notes: clickable checklist boxes, detection of UO ids (0x1234) under the mouse,
    /// plain text paste (Ctrl+Shift+V) and a scroll event for the line numbers.
    /// </summary>
    public class NoteRichTextBox : RichTextBox
    {
        public const char Unchecked = '☐';
        public const char Checked = '☑';

        private const int WM_VSCROLL = 0x115;
        private const int WM_MOUSEWHEEL = 0x20A;

        private int _lastHoverId = -2;

        /// <summary>
        /// Raised when the visible area changes (scroll, resize, text).
        /// </summary>
        public event EventHandler ViewChanged;

        /// <summary>
        /// Raised when the mouse moves onto (id &gt;= 0) or away from (id = -1) a hex id like 0x1A2B.
        /// </summary>
        public event Action<int, Point> HoverIdChanged;

        public NoteRichTextBox()
        {
            AcceptsTab = true;
            HideSelection = false;
            DetectUrls = true;
            BorderStyle = BorderStyle.None;
            ScrollBars = RichTextBoxScrollBars.Both;
            ShortcutsEnabled = true;
            AllowDrop = true;
            EnableAutoDragDrop = false;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_VSCROLL || m.Msg == WM_MOUSEWHEEL)
            {
                ViewChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnVScroll(EventArgs e)
        {
            base.OnVScroll(e);
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control && e.Shift && e.KeyCode == Keys.V)
            {
                PastePlainText();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            base.OnKeyDown(e);
        }

        public void PastePlainText()
        {
            if (Clipboard.ContainsText())
            {
                SelectedText = Clipboard.GetText().Replace("\r\n", "\n");
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left || SelectionLength > 0 || ReadOnly)
            {
                return;
            }

            // click on a checklist box toggles it
            int index = GetCharIndexFromPosition(e.Location);
            foreach (int i in new[] { index, index - 1 })
            {
                if (i < 0 || i >= TextLength)
                {
                    continue;
                }

                char c = Text[i];
                if (c != Unchecked && c != Checked)
                {
                    continue;
                }

                Point p = GetPositionFromCharIndex(i);
                int width = (int)(Font.Size * 1.6f * ZoomFactor) + 6;
                if (e.X < p.X - 2 || e.X > p.X + width)
                {
                    continue;
                }

                ToggleCheckAt(i);
                return;
            }
        }

        public void ToggleCheckAt(int index)
        {
            int keep = SelectionStart;
            Select(index, 1);
            bool nowChecked = SelectedText[0] == Unchecked;
            SelectedText = nowChecked ? Checked.ToString() : Unchecked.ToString();
            Select(index, 1);
            SelectionColor = nowChecked ? Color.SeaGreen : ForeColor;
            Select(keep, 0);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int id = IdAt(GetCharIndexFromPosition(e.Location), out _, out _);
            if (id >= 0)
            {
                // GetCharIndexFromPosition returns the nearest char – make sure the mouse is really on the text line
                Point p = GetPositionFromCharIndex(GetCharIndexFromPosition(e.Location));
                if (Math.Abs(e.Y - p.Y) > Font.Height * ZoomFactor * 2)
                {
                    id = -1;
                }
            }

            if (id != _lastHoverId)
            {
                _lastHoverId = id;
                HoverIdChanged?.Invoke(id, PointToScreen(new Point(e.X + 16, e.Y + 18)));
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_lastHoverId != -1)
            {
                _lastHoverId = -1;
                HoverIdChanged?.Invoke(-1, Point.Empty);
            }
        }

        /// <summary>
        /// Returns the hex id (0x1A2B) at the char index, or -1.
        /// </summary>
        public int IdAt(int index, out int start, out int length)
        {
            start = length = 0;
            string text = Text;
            if (index < 0 || index >= text.Length)
            {
                return -1;
            }

            static bool IsPart(char c) => char.IsAsciiHexDigit(c) || c == 'x' || c == 'X';

            int s = index, e = index;
            while (s > 0 && IsPart(text[s - 1]))
            {
                s--;
            }

            while (e < text.Length && IsPart(text[e]))
            {
                e++;
            }

            string token = text.Substring(s, e - s);
            int x = token.IndexOf("0x", StringComparison.OrdinalIgnoreCase);
            if (x < 0)
            {
                return -1;
            }

            string digits = token.Substring(x + 2);
            if (digits.Length is < 1 or > 6 ||
                !int.TryParse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int id))
            {
                return -1;
            }

            start = s + x;
            length = digits.Length + 2;
            return id;
        }
    }

    /// <summary>
    /// Line numbers next to a RichTextBox (logical lines, wrapped lines are not counted).
    /// </summary>
    public class LineNumberGutter : Control
    {
        private readonly NoteRichTextBox _box;

        public LineNumberGutter(NoteRichTextBox box)
        {
            _box = box;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.FromArgb(245, 246, 248);
            ForeColor = Color.FromArgb(150, 155, 165);
            Width = 40;
            _box.ViewChanged += (_, _) => UpdateWidthAndRedraw();
            _box.SelectionChanged += (_, _) => Invalidate();
        }

        private void UpdateWidthAndRedraw()
        {
            int lines = 1;
            foreach (char c in _box.Text)
            {
                if (c == '\n')
                {
                    lines++;
                }
            }

            using Font f = NumberFont();
            int width = TextRenderer.MeasureText(new string('9', Math.Max(2, lines.ToString(CultureInfo.InvariantCulture).Length)), f).Width + 14;
            if (Width != width)
            {
                Width = width;
            }

            Invalidate();
        }

        private Font NumberFont() => new("Consolas", Math.Max(7f, _box.Font.Size * _box.ZoomFactor * 0.85f));

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            using (var border = new Pen(Color.FromArgb(225, 227, 232)))
            {
                g.DrawLine(border, Width - 1, 0, Width - 1, Height);
            }

            if (!_box.IsHandleCreated)
            {
                return;
            }

            string text = _box.Text;
            int firstChar = _box.GetCharIndexFromPosition(new Point(1, 1));
            int firstLine = _box.GetLineFromCharIndex(firstChar);
            int lastLine = _box.GetLineFromCharIndex(text.Length);
            int caretLogical = CountNewlines(text, 0, Math.Min(_box.SelectionStart, text.Length)) + 1;

            int scanned = 0;
            int number = 1;
            using Font f = NumberFont();
            using var currentBrush = new SolidBrush(Color.FromArgb(60, 64, 72));

            for (int line = firstLine; line <= lastLine; line++)
            {
                int ci = _box.GetFirstCharIndexFromLine(line);
                if (ci < 0)
                {
                    break;
                }

                number += CountNewlines(text, scanned, ci);
                scanned = ci;

                int y = _box.GetPositionFromCharIndex(ci).Y;
                if (y > Height)
                {
                    break;
                }

                bool logicalStart = ci == 0 || (ci <= text.Length && text[ci - 1] == '\n');
                if (!logicalStart)
                {
                    continue;
                }

                string s = number.ToString(CultureInfo.InvariantCulture);
                Color color = number == caretLogical ? currentBrush.Color : ForeColor;
                TextRenderer.DrawText(g, s, f, new Rectangle(0, y + 1, Width - 6, f.Height + 2), color,
                    TextFormatFlags.Right | TextFormatFlags.NoPadding);
            }
        }

        private static int CountNewlines(string text, int from, int to)
        {
            int count = 0;
            to = Math.Min(to, text.Length);
            for (int i = Math.Max(0, from); i < to; i++)
            {
                if (text[i] == '\n')
                {
                    count++;
                }
            }

            return count;
        }
    }
}

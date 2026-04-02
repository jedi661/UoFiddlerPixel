// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  * Advanced:
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace UoFiddler.Controls.Forms
{
    public partial class EditorXML : Form
    {
        // ── fields ────────────────────────────────────────────────────────────
        private readonly string _outputPath;
        private string _currentFilePath;
        private bool _isDarkMode = false;
        private bool _isManualChange = true;

        // Search
        private int _lastSearchIndex = -1;
        private string _searchText = string.Empty;
        private int _foundCount = 0;
        private bool _caseSensitive = false;
        private bool _useRegex = false;

        // Undo/Redo – based only on snapshots, not per keystroke
        private readonly Stack<string> _undoStack = new Stack<string>();
        private readonly Stack<string> _redoStack = new Stack<string>();
        private string _lastSavedSnapshot = string.Empty;
        private System.Threading.Timer _undoDebounceTimer;
        private const int UndoDebounceMs = 800; // Snapshot nach 800 ms Pause

        // bookmark
        private readonly HashSet<int> _bookmarks = new HashSet<int>();

        // AutoSave
        private Timer _autoSaveTimer;

        // ── constructor ───────────────────────────────────────────────────────
        public EditorXML(string outputPath)
        {
            InitializeComponent();
            _outputPath = outputPath;
            KeyPreview = true;

            InitializeAutoSave();
            InitializeLineNumbers();
            ApplyLightTheme(); // Default: light theme
        }

        // ════════════════════════════════════════════════════════════════════
        #region AutoSave
        // ════════════════════════════════════════════════════════════════════

        private void InitializeAutoSave()
        {
            _autoSaveTimer = new Timer { Interval = 1_800_000 }; // 30 Min
            _autoSaveTimer.Tag = DateTime.Now;
            _autoSaveTimer.Tick += AutoSaveTimer_Tick;
        }

        private void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            _autoSaveTimer.Tag = DateTime.Now;
            if (!string.IsNullOrEmpty(_currentFilePath))
            {
                File.WriteAllText(_currentFilePath, richTextBoxXmlContent.Text);
                SetStatus("Auto-saved at " + DateTime.Now.ToString("HH:mm:ss"));
            }
        }

        private void checkBoxAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAutoSave.Checked)
                _autoSaveTimer.Start();
            else
                _autoSaveTimer.Stop();

            SetStatus(checkBoxAutoSave.Checked ? "Auto-save enabled." : "Auto-save disabled.");
        }

        private void ShowAutoSaveInformation()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                MessageBox.Show("No file loaded.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var fi = new FileInfo(_currentFilePath);
            var elapsed = DateTime.Now - (DateTime)_autoSaveTimer.Tag;
            int remaining = Math.Max(0, _autoSaveTimer.Interval / 1000 - (int)elapsed.TotalSeconds);

            MessageBox.Show(
                $"Auto-Save Information:\n\n" +
                $"Directory  : {fi.DirectoryName}\n" +
                $"Filename   : {fi.Name}\n" +
                $"Size       : {fi.Length / 1024} KB\n" +
                $"Changed    : {fi.LastWriteTime}\n" +
                $"Remaining  : {remaining / 60} Min {remaining % 60} Sek",
                "Auto-Save Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region Undo / Redo  (Debounced Snapshots)
        // ════════════════════════════════════════════════════════════════════

        /// <summary>Called after UndoDebounceM's pause to save a snapshot.</summary>
        private void ScheduleUndoSnapshot()
        {
            _undoDebounceTimer?.Dispose();
            _undoDebounceTimer = new System.Threading.Timer(_ =>
            {
                Invoke((Action)TakeUndoSnapshot);
            }, null, UndoDebounceMs, System.Threading.Timeout.Infinite);
        }

        private void TakeUndoSnapshot()
        {
            string current = richTextBoxXmlContent.Text;
            if (current == _lastSavedSnapshot) return;

            _undoStack.Push(_lastSavedSnapshot);
            _redoStack.Clear();
            _lastSavedSnapshot = current;
            UpdateUndoRedoMenu();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_undoStack.Count == 0) return;

            _undoDebounceTimer?.Dispose(); // cancel the running timer
            _isManualChange = false;
            _redoStack.Push(_lastSavedSnapshot);
            _lastSavedSnapshot = _undoStack.Pop();
            richTextBoxXmlContent.Text = _lastSavedSnapshot;
            _isManualChange = true;
            UpdateUndoRedoMenu();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_redoStack.Count == 0) return;

            _undoDebounceTimer?.Dispose();
            _isManualChange = false;
            _undoStack.Push(_lastSavedSnapshot);
            _lastSavedSnapshot = _redoStack.Pop();
            richTextBoxXmlContent.Text = _lastSavedSnapshot;
            _isManualChange = true;
            UpdateUndoRedoMenu();
        }

        private void UpdateUndoRedoMenu()
        {
            undoToolStripMenuItem.Enabled = _undoStack.Count > 0;
            redoToolStripMenuItem.Enabled = _redoStack.Count > 0;
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region Line numbers
        // ════════════════════════════════════════════════════════════════════

        private void InitializeLineNumbers()
        {
            richTextBoxXmlContent.SelectionChanged += (s, e) => UpdateLineHighlight();
            richTextBoxXmlContent.VScroll += (s, e) => lineNumberPanel.Invalidate();
        }

        private void lineNumberPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(_isDarkMode ? Color.FromArgb(40, 40, 40) : Color.FromArgb(240, 240, 240));

            int firstChar = richTextBoxXmlContent.GetCharIndexFromPosition(new Point(0, 0));
            int lastChar = richTextBoxXmlContent.GetCharIndexFromPosition(
                new Point(0, richTextBoxXmlContent.ClientSize.Height - 1));

            int firstLine = richTextBoxXmlContent.GetLineFromCharIndex(firstChar);
            int lastLine = richTextBoxXmlContent.GetLineFromCharIndex(lastChar);

            using var font = new Font("Consolas", 9f);
            using var brush = new SolidBrush(_isDarkMode ? Color.FromArgb(130, 130, 130) : Color.FromArgb(100, 100, 100));
            using var highlightBrush = new SolidBrush(_isDarkMode ? Color.FromArgb(60, 100, 140) : Color.FromArgb(200, 220, 255));

            int currentLine = richTextBoxXmlContent.GetLineFromCharIndex(richTextBoxXmlContent.SelectionStart);

            for (int i = firstLine; i <= lastLine && i < richTextBoxXmlContent.Lines.Length; i++)
            {
                int charIdx = richTextBoxXmlContent.GetFirstCharIndexFromLine(i);
                Point pos = richTextBoxXmlContent.GetPositionFromCharIndex(charIdx);

                if (i == currentLine)
                    e.Graphics.FillRectangle(highlightBrush, 0, pos.Y, lineNumberPanel.Width, font.Height + 2);

                // Bookmark-Markierung
                if (_bookmarks.Contains(i + 1))
                {
                    using var bmBrush = new SolidBrush(Color.FromArgb(255, 180, 0));
                    e.Graphics.FillEllipse(bmBrush, 2, pos.Y + 2, 8, 8);
                }

                string lineNum = (i + 1).ToString();
                var size = e.Graphics.MeasureString(lineNum, font);
                e.Graphics.DrawString(lineNum, font, brush,
                    lineNumberPanel.Width - size.Width - 3, pos.Y);
            }
        }

        private void UpdateLineHighlight()
        {
            lineNumberPanel.Invalidate();
            int line = richTextBoxXmlContent.GetLineFromCharIndex(richTextBoxXmlContent.SelectionStart) + 1;
            toolStripStatusLabelLine.Text = $"Line: {line}";
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region Select line, edit, insert, jump
        // ════════════════════════════════════════════════════════════════════

        /// <summary>Select the entire current line (double-click on the line number).</summary>
        private void lineNumberPanel_MouseClick(object sender, MouseEventArgs e)
        {
            Point pt = new Point(0, e.Y);
            int charIdx = richTextBoxXmlContent.GetCharIndexFromPosition(pt);
            int lineIdx = richTextBoxXmlContent.GetLineFromCharIndex(charIdx);

            if (e.Button == MouseButtons.Left)
                SelectLine(lineIdx);
            else if (e.Button == MouseButtons.Right)
                ShowLineContextMenu(lineIdx, e.Location);
        }

        private void SelectLine(int lineIndex)
        {
            if (lineIndex < 0 || lineIndex >= richTextBoxXmlContent.Lines.Length) return;

            int start = richTextBoxXmlContent.GetFirstCharIndexFromLine(lineIndex);
            int length = richTextBoxXmlContent.Lines[lineIndex].Length;
            richTextBoxXmlContent.Select(start, length);
            richTextBoxXmlContent.Focus();
        }

        private void ShowLineContextMenu(int lineIndex, Point panelPos)
        {
            var menu = new ContextMenuStrip();

            menu.Items.Add("Select line", null, (s, e) => SelectLine(lineIndex));
            menu.Items.Add("Edit line...", null, (s, e) => EditLineDialog(lineIndex));
            menu.Items.Add("Insert line before", null, (s, e) => InsertLineAt(lineIndex, before: true));
            menu.Items.Add("Insert line after that", null, (s, e) => InsertLineAt(lineIndex, before: false));
            menu.Items.Add(new ToolStripSeparator());

            bool hasBookmark = _bookmarks.Contains(lineIndex + 1);
            menu.Items.Add(hasBookmark ? "Remove bookmarks" : "Bookmark", null, (s, e) =>
            {
                if (hasBookmark) _bookmarks.Remove(lineIndex + 1);
                else _bookmarks.Add(lineIndex + 1);
                lineNumberPanel.Invalidate();
            });

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Delete line", null, (s, e) => DeleteLine(lineIndex));

            menu.Show(lineNumberPanel, panelPos);
        }

        private void EditLineDialog(int lineIndex)
        {
            if (lineIndex < 0 || lineIndex >= richTextBoxXmlContent.Lines.Length) return;

            string originalLine = richTextBoxXmlContent.Lines[lineIndex];

            using var form = new Form
            {
                Text = $"Line {lineIndex + 1} edit",
                Width = 650,
                Height = 140,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent
            };

            var tb = new TextBox
            {
                Text = originalLine,
                Dock = DockStyle.Top,
                Font = new Font("Consolas", 10f),
                Margin = new Padding(8)
            };

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 40
            };

            var btnOk = new Button { Text = "take over", Width = 110, Height = 28, DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Cancel", Width = 90, Height = 28, DialogResult = DialogResult.Cancel };

            panel.Controls.Add(btnOk);
            panel.Controls.Add(btnCancel);
            form.Controls.Add(tb);
            form.Controls.Add(panel);
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;
            tb.SelectAll();
            tb.Focus();

            if (form.ShowDialog() != DialogResult.OK) return;

            var lines = richTextBoxXmlContent.Lines.ToList();
            lines[lineIndex] = tb.Text;
            SetTextPreserveUndo(string.Join("\n", lines));
        }

        private void InsertLineAt(int lineIndex, bool before)
        {
            using var form = new Form
            {
                Text = before ? "Insert new line (before)" : "Insert new line (afterwards)",
                Width = 650,
                Height = 140,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent
            };

            var tb = new TextBox
            {
                Dock = DockStyle.Top,
                Font = new Font("Consolas", 10f),
                PlaceholderText = "Enter XML line...."
            };

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 40
            };

            var btnOk = new Button { Text = "Insert", Width = 90, Height = 28, DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Cancel", Width = 90, Height = 28, DialogResult = DialogResult.Cancel };

            panel.Controls.Add(btnOk);
            panel.Controls.Add(btnCancel);
            form.Controls.Add(tb);
            form.Controls.Add(panel);
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;
            tb.Focus();

            if (form.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(tb.Text)) return;

            var lines = richTextBoxXmlContent.Lines.ToList();
            int insertAt = before ? lineIndex : lineIndex + 1;
            lines.Insert(insertAt, tb.Text);
            SetTextPreserveUndo(string.Join("\n", lines));
        }

        private void DeleteLine(int lineIndex)
        {
            if (lineIndex < 0 || lineIndex >= richTextBoxXmlContent.Lines.Length) return;
            if (MessageBox.Show($"Line {lineIndex + 1} delete?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            var lines = richTextBoxXmlContent.Lines.ToList();
            lines.RemoveAt(lineIndex);
            SetTextPreserveUndo(string.Join("\n", lines));
        }

        private void SetTextPreserveUndo(string newText)
        {
            TakeUndoSnapshot(); // Immediate snapshot before the change
            _isManualChange = false;
            richTextBoxXmlContent.Text = newText;
            _lastSavedSnapshot = newText;
            _isManualChange = true;
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region Go to row
        // ════════════════════════════════════════════════════════════════════

        private void goToLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var form = new Form
            {
                Text = "Go to row",
                Width = 300,
                Height = 110,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent
            };

            var lbl = new Label { Text = $"Line (1–{richTextBoxXmlContent.Lines.Length}):", Top = 15, Left = 10, Width = 180 };
            var tb = new TextBox { Top = 12, Left = 195, Width = 80 };
            var btnOk = new Button { Text = "OK", Top = 45, Left = 100, Width = 80, DialogResult = DialogResult.OK };
            form.Controls.AddRange(new Control[] { lbl, tb, btnOk });
            form.AcceptButton = btnOk;
            tb.Focus();

            if (form.ShowDialog() != DialogResult.OK) return;
            if (!int.TryParse(tb.Text, out int lineNum)) return;

            lineNum = Math.Max(1, Math.Min(lineNum, richTextBoxXmlContent.Lines.Length));
            int charIdx = richTextBoxXmlContent.GetFirstCharIndexFromLine(lineNum - 1);
            richTextBoxXmlContent.SelectionStart = charIdx;
            richTextBoxXmlContent.SelectionLength = 0;
            richTextBoxXmlContent.ScrollToCaret();
            richTextBoxXmlContent.Focus();
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region Bookmarks
        // ════════════════════════════════════════════════════════════════════

        private void toggleBookmarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int line = richTextBoxXmlContent.GetLineFromCharIndex(richTextBoxXmlContent.SelectionStart) + 1;
            if (_bookmarks.Contains(line)) _bookmarks.Remove(line);
            else _bookmarks.Add(line);
            lineNumberPanel.Invalidate();
            SetStatus(_bookmarks.Contains(line) ? $"Bookmarked: line {line}" : $"Bookmark removed: line {line}");
        }

        private void nextBookmarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_bookmarks.Count == 0) { SetStatus("No bookmarks set."); return; }
            int current = richTextBoxXmlContent.GetLineFromCharIndex(richTextBoxXmlContent.SelectionStart) + 1;
            int next = _bookmarks.Where(b => b > current).DefaultIfEmpty(_bookmarks.Min()).Min();
            int charIdx = richTextBoxXmlContent.GetFirstCharIndexFromLine(next - 1);
            richTextBoxXmlContent.SelectionStart = charIdx;
            richTextBoxXmlContent.ScrollToCaret();
        }

        private void prevBookmarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_bookmarks.Count == 0) { SetStatus("No bookmarks set."); return; }
            int current = richTextBoxXmlContent.GetLineFromCharIndex(richTextBoxXmlContent.SelectionStart) + 1;
            int prev = _bookmarks.Where(b => b < current).DefaultIfEmpty(_bookmarks.Max()).Max();
            int charIdx = richTextBoxXmlContent.GetFirstCharIndexFromLine(prev - 1);
            richTextBoxXmlContent.SelectionStart = charIdx;
            richTextBoxXmlContent.ScrollToCaret();
        }

        private void clearBookmarksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _bookmarks.Clear();
            lineNumberPanel.Invalidate();
            SetStatus("All bookmarks deleted.");
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region XML Validierung
        // ════════════════════════════════════════════════════════════════════

        private void validateXmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(richTextBoxXmlContent.Text))
            {
                MessageBox.Show("No content to validate.", "XML validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                XDocument.Parse(richTextBoxXmlContent.Text);
                SetStatus("XML is valid.");
                MessageBox.Show("XML is well-formed and valid.", "XML validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (XmlException ex)
            {
                string msg = $"XML error in line {ex.LineNumber}, Line {ex.LinePosition}:\n{ex.Message}";
                SetStatus($"XML error: Line {ex.LineNumber}");
                MessageBox.Show(msg, "XML validation failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Direkt zur Fehlerzeile springen
                if (ex.LineNumber > 0 && ex.LineNumber <= richTextBoxXmlContent.Lines.Length)
                {
                    int charIdx = richTextBoxXmlContent.GetFirstCharIndexFromLine(ex.LineNumber - 1);
                    richTextBoxXmlContent.SelectionStart = charIdx;
                    richTextBoxXmlContent.ScrollToCaret();
                }
            }
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region XML Pretty Print / Minify
        // ════════════════════════════════════════════════════════════════════

        private void prettyPrintToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var xdoc = XDocument.Parse(richTextBoxXmlContent.Text);
                var sb = new StringBuilder();
                using var writer = new StringWriter(sb);
                xdoc.Save(writer);
                SetTextPreserveUndo(sb.ToString());
                SetStatus("XML formatted (Pretty Print).");
            }
            catch (XmlException ex)
            {
                MessageBox.Show($"Formatting failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void minifyXmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var xdoc = XDocument.Parse(richTextBoxXmlContent.Text);
                var sb = new StringBuilder();
                var settings = new XmlWriterSettings { Indent = false, NewLineHandling = NewLineHandling.None };
                using var writer = XmlWriter.Create(sb, settings);
                xdoc.WriteTo(writer);
                SetTextPreserveUndo(sb.ToString());
                SetStatus("XML minimized (Minify).");
            }
            catch (XmlException ex)
            {
                MessageBox.Show($"Minify failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region Syntax-Highlighting
        // ════════════════════════════════════════════════════════════════════

        private bool _highlightingInProgress = false;

        private void ApplySyntaxHighlighting()
        {
            if (_highlightingInProgress) return;
            _highlightingInProgress = true;
            _isManualChange = false;

            int savedSelStart = richTextBoxXmlContent.SelectionStart;
            int savedSelLen = richTextBoxXmlContent.SelectionLength;

            richTextBoxXmlContent.SuspendLayout();
            richTextBoxXmlContent.SelectAll();
            richTextBoxXmlContent.SelectionColor = _isDarkMode ? Color.FromArgb(220, 220, 220) : Color.Black;

            string text = richTextBoxXmlContent.Text;

            // Farben je nach Theme
            Color colorTag = _isDarkMode ? Color.FromArgb(86, 156, 214) : Color.FromArgb(0, 0, 200);
            Color colorAttrName = _isDarkMode ? Color.FromArgb(156, 220, 254) : Color.FromArgb(120, 0, 0);
            Color colorAttrValue = _isDarkMode ? Color.FromArgb(206, 145, 120) : Color.FromArgb(0, 128, 0);
            Color colorComment = _isDarkMode ? Color.FromArgb(106, 153, 85) : Color.FromArgb(0, 128, 0);
            Color colorDecl = _isDarkMode ? Color.FromArgb(200, 200, 100) : Color.FromArgb(128, 128, 0);

            HighlightMatches(text, @"<!--[\s\S]*?-->", colorComment);
            HighlightMatches(text, @"<\?[\s\S]*?\?>", colorDecl);
            HighlightMatches(text, @"</?[\w:.]+", colorTag);
            HighlightMatches(text, @"\b[\w:]+(?=\s*=)", colorAttrName);
            HighlightMatches(text, @"""[^""]*""", colorAttrValue);
            HighlightMatches(text, @"[<>/]", colorTag);

            richTextBoxXmlContent.SelectionStart = savedSelStart;
            richTextBoxXmlContent.SelectionLength = savedSelLen;
            richTextBoxXmlContent.ResumeLayout();

            _isManualChange = true;
            _highlightingInProgress = false;
        }

        private void HighlightMatches(string text, string pattern, Color color)
        {
            foreach (Match m in Regex.Matches(text, pattern, RegexOptions.Singleline))
            {
                richTextBoxXmlContent.Select(m.Index, m.Length);
                richTextBoxXmlContent.SelectionColor = color;
            }
        }

        private void checkBoxSyntaxHighlight_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSyntaxHighlight.Checked)
                ApplySyntaxHighlighting();
            else
                ResetTextColor();
        }

        private void ResetTextColor()
        {
            _isManualChange = false;
            richTextBoxXmlContent.SelectAll();
            richTextBoxXmlContent.SelectionColor = _isDarkMode ? Color.FromArgb(220, 220, 220) : Color.Black;
            richTextBoxXmlContent.SelectionStart = 0;
            richTextBoxXmlContent.SelectionLength = 0;
            _isManualChange = true;
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region Theme (Hell / Dunkel)
        // ════════════════════════════════════════════════════════════════════

        private void ApplyLightTheme()
        {
            _isDarkMode = false;
            BackColor = SystemColors.Control;
            richTextBoxXmlContent.BackColor = Color.White;
            richTextBoxXmlContent.ForeColor = Color.Black;
            lineNumberPanel.BackColor = Color.FromArgb(240, 240, 240);
            menuStripXMLEdit.BackColor = SystemColors.Control;
            menuStripXMLEdit.ForeColor = SystemColors.ControlText;
            statusStripXMLEditor.BackColor = SystemColors.Control;
            contextMenuStripXMLEdit.BackColor = SystemColors.Control;
            contextMenuStripXMLEdit.ForeColor = SystemColors.ControlText;
            darkModeToolStripMenuItem.Text = "Dark Mode";
            if (checkBoxSyntaxHighlight?.Checked == true) ApplySyntaxHighlighting();
            lineNumberPanel.Invalidate();
        }

        private void ApplyDarkTheme()
        {
            _isDarkMode = true;
            BackColor = Color.FromArgb(41, 44, 51);
            richTextBoxXmlContent.BackColor = Color.FromArgb(30, 30, 30);
            richTextBoxXmlContent.ForeColor = Color.FromArgb(220, 220, 220);
            lineNumberPanel.BackColor = Color.FromArgb(40, 40, 40);
            menuStripXMLEdit.BackColor = Color.FromArgb(45, 45, 48);
            menuStripXMLEdit.ForeColor = Color.FromArgb(220, 220, 220);
            statusStripXMLEditor.BackColor = Color.FromArgb(45, 45, 48);
            contextMenuStripXMLEdit.BackColor = Color.FromArgb(30, 30, 30);
            contextMenuStripXMLEdit.ForeColor = Color.White;
            darkModeToolStripMenuItem.Text = "Light Mode";
            if (checkBoxSyntaxHighlight?.Checked == true) ApplySyntaxHighlighting();
            lineNumberPanel.Invalidate();
        }

        private void designToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_isDarkMode) ApplyLightTheme();
            else ApplyDarkTheme();
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region TextChanged & Statistik
        // ════════════════════════════════════════════════════════════════════

        private void richTextBoxXmlContent_TextChanged(object sender, EventArgs e)
        {
            if (!_isManualChange) return;
            ScheduleUndoSnapshot();
            UpdateTextStatistics();
            if (checkBoxSyntaxHighlight.Checked)
                ApplySyntaxHighlighting();
        }

        private void UpdateTextStatistics()
        {
            int words = richTextBoxXmlContent.Text
                .Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
            int chars = richTextBoxXmlContent.Text.Length;
            int lines = richTextBoxXmlContent.Lines.Length;
            int tags = Regex.Matches(richTextBoxXmlContent.Text, @"<[^>]+>").Count;
            toolStripStatusLabelTextStatistics.Text =
                $"Words: {words}  Sign: {chars}  lines: {lines}  XML-Tags: {tags}";
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region Load / save / print file
        // ════════════════════════════════════════════════════════════════════

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string initDir = checkBoxOutputPath.Checked
                ? _outputPath
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UoFiddler");

            using var dlg = new OpenFileDialog { Filter = "XML files|*.xml", Title = "XML-files open", InitialDirectory = initDir };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                _currentFilePath = dlg.FileName;
                string content = File.ReadAllText(dlg.FileName);
                _lastSavedSnapshot = content;
                _undoStack.Clear();
                _redoStack.Clear();

                _isManualChange = false;
                if (checkBoxXMLFormatted.Checked)
                    richTextBoxXmlContent.Rtf = ConvertXmlToRtf(content);
                else
                    richTextBoxXmlContent.Text = content;
                _isManualChange = true;

                UpdateUndoRedoMenu();
                SetStatus($"Geladen: {Path.GetFileName(dlg.FileName)}");
                UpdateTextStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentFilePath))
            {
                SaveToPath(_currentFilePath);
                return;
            }
            saveAsToolStripMenuItem_Click(sender, e);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string initDir = checkBoxOutputPath.Checked
                ? _outputPath
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UoFiddler");

            using var dlg = new SaveFileDialog { Filter = "XML files|*.xml", Title = "Save XML file", InitialDirectory = initDir };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            _currentFilePath = dlg.FileName;
            SaveToPath(_currentFilePath);
        }

        private void SaveToPath(string path)
        {
            try
            {
                File.WriteAllText(path, richTextBoxXmlContent.Text);
                SetStatus($"Saved: {Path.GetFileName(path)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var doc = new PrintDocument();
            doc.PrintPage += (s, ev) =>
                ev.Graphics.DrawString(richTextBoxXmlContent.Text,
                    new Font("Consolas", 9), Brushes.Black,
                    new RectangleF(0, 0, ev.PageBounds.Width, ev.PageBounds.Height));

            using var dlg = new PrintDialog { Document = doc };
            if (dlg.ShowDialog() == DialogResult.OK) doc.Print();
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region Find & Replace
        // ════════════════════════════════════════════════════════════════════

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string query = toolStripTextBoxFind.Text.Trim();
            if (string.IsNullOrEmpty(query)) return;

            if (_searchText != query)
            {
                ResetSearch();
                _searchText = query;
                CountOccurrences();
            }

            int start = _lastSearchIndex + 1;
            StringComparison cmp = _caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            if (_useRegex)
            {
                try
                {
                    var rx = new Regex(_searchText, _caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                    var m = rx.Match(richTextBoxXmlContent.Text, start);
                    if (m.Success) { _lastSearchIndex = m.Index; HighlightFound(_lastSearchIndex, m.Length); }
                    else { WrapAround(); }
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show($"Invalid regex:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                _lastSearchIndex = richTextBoxXmlContent.Text.IndexOf(_searchText, start, cmp);
                if (_lastSearchIndex != -1)
                    HighlightFound(_lastSearchIndex, _searchText.Length);
                else
                    WrapAround();
            }
        }

        private void WrapAround()
        {
            if (MessageBox.Show("End of document. Start over.?",
                "Search", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _lastSearchIndex = -1;
                findToolStripMenuItem_Click(null, EventArgs.Empty);
            }
            else ResetSearch();
        }

        private void HighlightFound(int index, int length)
        {
            richTextBoxXmlContent.Select(index, length);
            richTextBoxXmlContent.SelectionBackColor = Color.Yellow;
            richTextBoxXmlContent.ScrollToCaret();
            SetStatus($"'{_searchText}' found in position {index + 1}");
            toolStripStatusWord.Text = $"In total: {_foundCount}";
        }

        private void ResetSearch()
        {
            ResetHighlighting();
            _lastSearchIndex = -1;
            _foundCount = 0;
            _searchText = string.Empty;
            toolStripStatusWord.Text = string.Empty;
        }

        private void ResetHighlighting()
        {
            _isManualChange = false;
            richTextBoxXmlContent.SelectAll();
            richTextBoxXmlContent.SelectionBackColor = _isDarkMode ? Color.FromArgb(30, 30, 30) : Color.White;
            richTextBoxXmlContent.SelectionLength = 0;
            _isManualChange = true;
        }

        private void CountOccurrences()
        {
            ResetHighlighting();
            int idx = 0; _foundCount = 0;
            while ((idx = richTextBoxXmlContent.Text.IndexOf(_searchText, idx, StringComparison.OrdinalIgnoreCase)) != -1)
            { idx += _searchText.Length; _foundCount++; }
            SetStatus($"'{_searchText}': {_foundCount} Hit");
        }

        private void toolStripTextBoxFind_TextChanged(object sender, EventArgs e)
        {
            ResetSearch();
            if (!string.IsNullOrWhiteSpace(toolStripTextBoxFind.Text))
            {
                _searchText = toolStripTextBoxFind.Text.Trim();
                CountOccurrences();
            }
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetSearch();
            toolStripTextBoxFind.Text = string.Empty;
            SetStatus("Search reset.");
        }

        private void searchAndReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var frm = new SearchReplaceDialog(richTextBoxXmlContent);
            frm.ShowDialog(this);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region Add a row (UO-specific)
        // ════════════════════════════════════════════════════════════════════

        private void addNewLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var form = new Form
            {
                Text = "Add new mob / equipment",
                Width = 420,
                Height = 230,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent
            };

            int y = 15;
            Control Lbl(string t, int top) => new Label { Text = t, Top = top, Left = 10, Width = 110 };
            Control Tb(int top) { var tb = new TextBox { Top = top, Left = 125, Width = 270 }; return tb; }

            var tbName = (TextBox)Tb(y); form.Controls.Add(Lbl("Mob Name:", y)); form.Controls.Add(tbName); y += 35;
            var tbBody = (TextBox)Tb(y); form.Controls.Add(Lbl("Body ID:", y)); form.Controls.Add(tbBody); y += 35;

            var cbType = new ComboBox { Top = y, Left = 125, Width = 270, DropDownStyle = ComboBoxStyle.DropDownList };
            cbType.Items.AddRange(new object[] { "Monster (0)", "Sea (1)", "Animal (2)", "Human/Equip (3)" });
            cbType.SelectedIndex = 0;
            form.Controls.Add(Lbl("Typ:", y)); form.Controls.Add(cbType); y += 35;

            var lblInfo = new Label { Text = "0=Monster  1=Sea  2=Animal  3=Human/Equip", Top = y, Left = 10, Width = 380, ForeColor = Color.Gray };
            form.Controls.Add(lblInfo); y += 25;

            var btnAdd = new Button { Text = "Add", Top = y, Left = 155, Width = 100, Height = 28 };
            form.Controls.Add(btnAdd);

            btnAdd.Click += (s, _) =>
            {
                if (!int.TryParse(tbBody.Text, out int bodyId))
                {
                    MessageBox.Show("Invalid Body ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                int type = cbType.SelectedIndex;
                string tag = type == 3 ? "Equip" : "Mob";

                if (IsBodyIdTaken(bodyId, tag))
                {
                    MessageBox.Show($"Body ID {bodyId} is in <{tag}> already taken.", "Duplikat",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string newLine = $"  <{tag} name=\"{tbName.Text} ({bodyId})\" body=\"{bodyId}\" type=\"{type}\" />";
                AddLineToXml(newLine, bodyId, tag);
                form.Close();
            };

            form.ShowDialog();
        }

        private void AddLineToXml(string newLine, int bodyId, string tagName)
        {
            var lines = richTextBoxXmlContent.Lines.ToList();
            bool added = false;
            for (int i = 0; i < lines.Count; i++)
            {
                if (TryGetBodyIdAndTagFromLine(lines[i], out int cur, out string curTag) &&
                    curTag == tagName && cur > bodyId)
                {
                    lines.Insert(i, newLine);
                    added = true;
                    break;
                }
            }
            if (!added) lines.Add(newLine);
            SetTextPreserveUndo(string.Join("\n", lines));
        }

        private bool TryGetBodyIdAndTagFromLine(string line, out int bodyId, out string tagName)
        {
            var m = Regex.Match(line, @"<(?<tag>\w+) name=""[^""]*"" body=""(?<body>\d+)""");
            if (m.Success)
            {
                bodyId = int.Parse(m.Groups["body"].Value);
                tagName = m.Groups["tag"].Value;
                return true;
            }
            bodyId = 0; tagName = null; return false;
        }

        private bool IsBodyIdTaken(int bodyId, string tagName) =>
            richTextBoxXmlContent.Text.Contains($"<{tagName} ") &&
            richTextBoxXmlContent.Text.Contains($"body=\"{bodyId}\"");

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region Clipboard / Drag-Drop
        // ════════════════════════════════════════════════════════════════════

        private void copyClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(richTextBoxXmlContent.Text))
            {
                MessageBox.Show("No text available.", "Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Clipboard.SetText(richTextBoxXmlContent.Text);
            SetStatus("Text copied to clipboard.");
        }

        private void EditorXML_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                e.Effect = (files.Length == 1 &&
                    Path.GetExtension(files[0]).Equals(".xml", StringComparison.OrdinalIgnoreCase))
                    ? DragDropEffects.Copy : DragDropEffects.None;
            }
        }

        private void EditorXML_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                _currentFilePath = path;
                _isManualChange = false;
                richTextBoxXmlContent.Text = File.ReadAllText(path);
                _lastSavedSnapshot = richTextBoxXmlContent.Text;
                _isManualChange = true;
                SetStatus($"Geladen: {Path.GetFileName(path)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region Keyboard shortcuts
        // ════════════════════════════════════════════════════════════════════

        private void EditorXML_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F3: e.Handled = e.SuppressKeyPress = true; findToolStripMenuItem_Click(sender, EventArgs.Empty); break;
                case Keys.F5: e.Handled = e.SuppressKeyPress = true; resetToolStripMenuItem_Click(sender, EventArgs.Empty); break;
                case Keys.F6: e.Handled = e.SuppressKeyPress = true; designToolStripMenuItem_Click(sender, EventArgs.Empty); break;
                case Keys.F7: e.Handled = e.SuppressKeyPress = true; printToolStripMenuItem_Click(sender, EventArgs.Empty); break;
                case Keys.F8: e.Handled = e.SuppressKeyPress = true; saveToolStripMenuItem_Click(sender, EventArgs.Empty); break;
                case Keys.F10: e.Handled = e.SuppressKeyPress = true; ShowAutoSaveInformation(); break;
            }

            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.Z: e.SuppressKeyPress = true; undoToolStripMenuItem_Click(sender, e); break;
                    case Keys.Y: e.SuppressKeyPress = true; redoToolStripMenuItem_Click(sender, e); break;
                    case Keys.S: e.SuppressKeyPress = true; saveToolStripMenuItem_Click(sender, EventArgs.Empty); break;
                    case Keys.G: e.SuppressKeyPress = true; goToLineToolStripMenuItem_Click(sender, EventArgs.Empty); break;
                    case Keys.B: e.SuppressKeyPress = true; toggleBookmarkToolStripMenuItem_Click(sender, EventArgs.Empty); break;
                }
            }

            if (e.KeyCode == Keys.F2 && !e.Control && !e.Shift)
            {
                e.Handled = e.SuppressKeyPress = true;
                nextBookmarkToolStripMenuItem_Click(sender, EventArgs.Empty);
            }
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region Hilfe / Key-Bindings
        // ════════════════════════════════════════════════════════════════════

        private void keyDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Tastatur-Shortcuts:\n\n" +
                "F3          Search\n" +
                "F5          Reset search\n" +
                "F6          Switch theme (light/dark)\n" +
                "F7          Print\n" +
                "F8          Speichern (Ctrl+S)\n" +
                "F10         Auto-Save Info\n" +
                "F2          Next bookmark\n" +
                "Ctrl+Z      Undo\n" +
                "Ctrl+Y      Restore\n" +
                "Ctrl+G      Go to row\n" +
                "Ctrl+B      Add/remove bookmarks\n" +
                "\n" +
                "Right-click on line number:\n" +
                "  Select/edit line\n" +
                "  Insert line before/after\n" +
                "  Delete line/bookmark",
                "Keyboard shortcuts", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region RTF Syntax Converter (for checkBoxXMLFormatted)
        // ════════════════════════════════════════════════════════════════════

        private string ConvertXmlToRtf(string xmlContent)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi\deff0{\fonttbl{\f0 Courier New;}}");
            sb.Append(@"{\colortbl ;\red0\green0\blue0;\red0\green0\blue200;\red180\green0\blue0;\red0\green128\blue0;\red128\green128\blue0;}");
            sb.Append(@"\viewkind4\uc1\pard\f0\fs20 ");

            var declMatch = Regex.Match(xmlContent, @"<\?xml[^>]+>");
            if (declMatch.Success)
            {
                sb.Append(@"\cf5 ").Append(EscapeRtf(declMatch.Value)).Append(@"\par\cf1 ");
                xmlContent = xmlContent.Replace(declMatch.Value, "");
            }

            foreach (Match m in Regex.Matches(xmlContent, @"<[^>]+>|<!--.*?-->|[^<]+", RegexOptions.Singleline))
            {
                string f = m.Value;
                if (f.StartsWith("<!--"))
                    sb.Append(@"\cf4 ").Append(EscapeRtf(f)).Append(@"\cf1 ");
                else if (f.StartsWith("</"))
                    sb.Append(@"\cf2 ").Append(EscapeRtf(f)).Append(@"\cf1 ");
                else if (f.StartsWith("<"))
                {
                    var tagName = Regex.Match(f, @"<\s*[^!?/\s>]+");
                    if (tagName.Success) sb.Append(@"\cf2 ").Append(EscapeRtf(tagName.Value));
                    foreach (Match attr in Regex.Matches(f, @"\b\w+\s*=\s*""[^""]*"""))
                        sb.Append(@" \cf3 ").Append(EscapeRtf(attr.Value));
                    sb.Append(@"\cf2 ").Append(f.EndsWith("/>") ? " />" : ">").Append(@"\cf1 ");
                }
                else
                    sb.Append(EscapeRtf(f));
            }

            sb.Append(@"}");
            return sb.ToString();
        }

        private static string EscapeRtf(string t) =>
            t.Replace(@"\", @"\\").Replace("{", @"\{").Replace("}", @"\}")
             .Replace("\t", @"\tab ").Replace("\n", @"\par ");

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region Auxiliary methods
        // ════════════════════════════════════════════════════════════════════

        private void SetStatus(string message)
        {
            toolStripStatusLabelInfo.Text = message;
        }

        #endregion
    }
}
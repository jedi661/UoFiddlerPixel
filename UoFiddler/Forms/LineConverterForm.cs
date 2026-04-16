// ***************************************************************************
//
// $Author: Nikodemus / Advanced Nikodemus
//
// "THE BEER-WINE-WARE LICENSE"
// As long as you retain this notice you can do whatever you want with
// this stuff. If we meet some day, and you think this stuff is worth it,
// you can buy me a beer and wine in return.
//
// ***************************************************************************
//
// PURPOSE:
//   This tool compresses multi-line source code or text into compact,
//   single-line or block-chunked format so it can be pasted into AI chat
//   interfaces that have strict character-per-message limits (e.g. 4000,
//   8000 or custom sizes).  It also lets you step through those chunks
//   one-by-one, always showing how many characters remain, so you never
//   lose track of where you are in a large paste session.
//
// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace UoFiddler.Forms
{
    // =========================================================================
    // LineConverterForm
    // =========================================================================
    // Main form for the "Paragraph-to-Line Converter / AI Character Shrinker".
    // Responsibilities:
    //   - Accept free-form text or source code via paste, drag-drop or file load.
    //   - Offer multiple conversion strategies (with/without comment blocks,
    //     with/without spacing around operators, etc.).
    //   - Split the converted result into user-defined chunks and let the user
    //     copy them to the clipboard one chunk at a time while always displaying
    //     the remaining character count.
    //   - Restore the original unmodified text at any time.
    //   - Provide search & replace functionality inside the text area.
    // =========================================================================
    public partial class LineConverterForm : Form
    {
        // -------------------------------------------------------------------------
        // Fields
        // -------------------------------------------------------------------------

        private ToolTip _toolTip = new ToolTip();
        private int _lastSearchIndex = 0;
        private readonly Stack<string> _undoStack = new Stack<string>();
        private readonly Stack<string> _redoStack = new Stack<string>();
        private bool _suppressSnapshot = false;

        private bool _darkMode = false;

        private readonly Color DarkBack = Color.FromArgb(32, 32, 32);
        private readonly Color DarkPanel = Color.FromArgb(45, 45, 45);
        private readonly Color DarkText = Color.FromArgb(220, 220, 220);
        private readonly Color DarkBorder = Color.FromArgb(70, 70, 70);

        private readonly Color LightBack = Color.White;
        private readonly Color LightPanel = Color.FromArgb(245, 245, 245);
        private readonly Color LightText = Color.Black;

        /// <summary>
        /// Stores the text as it was before the most recent conversion so the
        /// user can revert to it via the Restore button.
        /// </summary>
        private string _originalText = string.Empty;

        /// <summary>
        /// When the user works through a multi-chunk copy session this list
        /// holds all chunks produced by the last "Build Chunks" operation.
        /// </summary>
        private List<string> _chunks = new List<string>();

        /// <summary>
        /// Zero-based index of the chunk that will be copied on the next call
        /// to CopyNextChunk().
        /// </summary>
        private int _chunkIndex = 0;

        /// <summary>
        /// Starting index used by the incremental single-replace feature so
        /// repeated clicks advance through the text rather than always hitting
        /// the first occurrence.
        /// </summary>
        private int _searchReplaceStartIndex = 0;

        // =========================================================================
        // Constructor
        // =========================================================================

        /// <summary>
        /// Initialises the form, wires up all dynamic event handlers and sets
        /// sensible default values for every control.
        /// </summary>
        public LineConverterForm()
        {
            InitializeComponent();
            WireUpEvents();
            SetDefaults();
            ApplyTooltips();
        }

        // =========================================================================
        // Initialisation helpers
        // =========================================================================

        #region WireUpEvents
        /// <summary>
        /// Attaches all event handlers that cannot be set in the designer because
        /// they depend on runtime state (e.g. closures that capture local
        /// variables).  Keeping them here rather than scattered through the code
        /// makes it easy to see the complete event map at a glance.
        /// </summary>
        private void WireUpEvents()
        {
            TextBoxInputOutput.TextChanged += TextBoxInputOutput_TextChanged;
            NumBlockSize.ValueChanged += NumBlockSize_ValueChanged;
            BtnUndo.Click += (s, e) => Undo();
            BtnRedo.Click += (s, e) => Redo();
        }
        #endregion

        #region SetDefaults
        /// <summary>
        /// Applies default values to controls after InitializeComponent() has
        /// run.  This keeps the designer file clean and groups all "startup
        /// configuration" in one readable place.
        /// </summary>
        private void SetDefaults()
        {
            NumBlockSize.Minimum = 100;
            NumBlockSize.Maximum = 100000;
            NumBlockSize.Value = 4000;

            UpdateStatusBar();
        }
        #endregion

        #region AI Limits
        private readonly Dictionary<string, int> _aiLimits = new Dictionary<string, int>
        {
            { "ChatGPT", 7000 },
            { "Claude", 180000 },
            { "Gemini", 32000 },
            { "DeepSeek", 16000 }
        };
        #endregion

        // =========================================================================
        // Conversion Methods
        // =========================================================================

        #region ConvertParagraphsToLines  (Mode A – with comment preservation)
        /// <summary>
        /// Joins all lines into a single continuous string.
        /// Lines that start with "//" or end with ";", "{" or "}" receive a
        /// trailing space so the resulting string stays parsable.
        /// When <paramref name="addSpaces"/> is true the same spacing rule is
        /// applied to ALL non-empty lines, which keeps the output slightly more
        /// readable at the cost of a few extra characters.
        /// </summary>
        /// <param name="input">The raw multi-line text to compress.</param>
        /// <param name="addSpaces">
        ///   If true, every trimmed non-empty line gets a trailing space.
        ///   If false, only syntactically significant lines do.
        /// </param>
        /// <returns>A single-line compressed string.</returns>
        private static string ConvertParagraphsToLines(string input, bool addSpaces)
        {
            var lines = input.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.None);
            var result = new StringBuilder(input.Length);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.Length == 0)
                    continue;

                result.Append(trimmed);

                bool needsSpace = addSpaces
                    || trimmed.StartsWith("//")
                    || trimmed.EndsWith(";")
                    || trimmed.EndsWith("{")
                    || trimmed.EndsWith("}");

                if (needsSpace)
                    result.Append(' ');
            }

            return result.ToString();
        }
        #endregion

        #region ConvertWordByWord  (Mode B – maximum compression)
        /// <summary>
        /// Splits every line into individual words and re-concatenates them.
        /// A single space is appended only after words that end with ";", "{"
        /// or "}".  All other whitespace (indentation, blank lines) is
        /// discarded entirely, yielding the most compact possible output.
        /// </summary>
        /// <param name="input">The raw multi-line text to compress.</param>
        /// <returns>A maximally compressed single-line string.</returns>
        private static string ConvertWordByWord(string input)
        {
            var lines = input.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.None);
            var result = new StringBuilder(input.Length);

            foreach (var line in lines)
            {
                var words = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    result.Append(word);
                    if (word.EndsWith(";") || word.EndsWith("{") || word.EndsWith("}"))
                        result.Append(' ');
                }
            }

            return result.ToString();
        }
        #endregion

        #region ConvertWordByWordSkipCommentBlocks  (Mode C – strip header comments)
        /// <summary>
        /// Identical to <see cref="ConvertWordByWord"/> except that any block
        /// delimited by a line that matches the UoFiddler license header pattern
        /// ("// /****…" … "*****/") is omitted from the output entirely.
        /// This is useful when sending code to an AI that does not need to see
        /// the copyright notice.
        /// </summary>
        /// <param name="input">The raw multi-line text to compress.</param>
        /// <returns>
        ///   A compressed single-line string with comment block(s) removed.
        /// </returns>
        private static string ConvertWordByWordSkipCommentBlocks(string input)
        {
            var lines = input.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.None);
            var result = new StringBuilder(input.Length);
            bool inCommentBlock = false;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                // Detect the START of a license / banner comment block.
                if (trimmed.StartsWith("// /*****") || trimmed.StartsWith("///*****"))
                    inCommentBlock = true;

                if (!inCommentBlock)
                {
                    var words = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in words)
                    {
                        result.Append(word);
                        if (word.EndsWith(";") || word.EndsWith("{") || word.EndsWith("}"))
                            result.Append(' ');
                    }
                }

                // Detect the END of the comment block.
                if (trimmed.EndsWith("***************************************************************************/"))
                    inCommentBlock = false;
            }

            return result.ToString();
        }
        #endregion

        #region NormaliseAndClean  (Mode D – operator spacing + whitespace normalisation)
        /// <summary>
        /// Applies a set of regex-based cleanup rules to the input:
        ///   1. Collapses all runs of whitespace (including newlines) into a
        ///      single space.
        ///   2. Ensures exactly one space surrounds common binary operators
        ///      (=, +, -, *, /, %, &lt;, &gt;, !, &amp;, |).
        ///   3. Normalises comma spacing to ", ".
        /// The result is a single line with consistent formatting – handy when
        /// pasting code snippets into an AI prompt for analysis.
        /// </summary>
        /// <param name="input">The raw multi-line text to normalise.</param>
        /// <returns>A cleaned, single-line string.</returns>
        private static string NormaliseAndClean(string input)
        {
            // Step 1 – collapse all whitespace runs into a single space.
            string text = Regex.Replace(input, @"\s+", " ");

            // Step 2 – pad binary operators that currently lack surrounding spaces.
            text = Regex.Replace(text, @"([^\s=+\-*/%<>!&|])([=+\-*/%<>!&|])([^\s=+\-*/%<>!&|])", "$1 $2 $3");

            // Step 3 – normalise comma spacing.
            text = Regex.Replace(text, @",\s*", ", ");

            return text.Trim();
        }
        #endregion

        // =========================================================================
        // Chunk Management
        // =========================================================================

        #region BuildChunks
        /// <summary>
        /// Splits the current text-box content into equally-sized chunks whose
        /// maximum length is governed by <see cref="NumBlockSize"/>.
        /// The chunks are stored in <see cref="_chunks"/> and the chunk index is
        /// reset to 0.  After building, the UI is refreshed so the user can see
        /// how many chunks were created and copy the first one immediately.
        /// </summary>
        private void BuildChunks()
        {
            _chunks.Clear();
            _chunkIndex = 0;

            string text = TextBoxInputOutput.Text;
            int blockSize = (int)NumBlockSize.Value;

            if (text.Length == 0 || blockSize <= 0)
            {
                UpdateChunkUI();
                return;
            }

            for (int i = 0; i < text.Length; i += blockSize)
            {
                int len = Math.Min(blockSize, text.Length - i);
                _chunks.Add(text.Substring(i, len));
            }

            UpdateChunkUI();
        }
        #endregion

        #region CopyNextChunk
        /// <summary>
        /// Copies the next pending chunk to the system clipboard, removes it
        /// from the working list, and updates all chunk-related UI labels so
        /// the user always knows how many characters and chunks remain.
        /// If no chunks are available a descriptive message box is shown.
        /// </summary>
        private void CopyNextChunk()
        {
            if (_chunks.Count == 0 || _chunkIndex >= _chunks.Count)
            {
                MessageBox.Show(
                    "No chunks available. Please build chunks first using the 'Build & Copy Chunks' panel.",
                    "No Chunks",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            string chunk = _chunks[_chunkIndex];
            Clipboard.SetText(chunk);

            // Highlight the copied portion in the text box so the user has
            // visual feedback about exactly what was transferred.
            int startPos = 0;
            for (int i = 0; i < _chunkIndex; i++)
                startPos += _chunks[i].Length;

            TextBoxInputOutput.Select(startPos, chunk.Length);
            TextBoxInputOutput.ScrollToCaret();

            _chunkIndex++;
            UpdateChunkUI();

            if (_chunkIndex >= _chunks.Count)
            {
                LblChunkStatus.Text = "✔  All chunks copied!";
                LblChunkStatus.ForeColor = Color.DarkGreen;
            }
        }
        #endregion

        #region UpdateChunkUI
        /// <summary>
        /// Recalculates and refreshes all chunk-related labels:
        ///   - Total chunk count.
        ///   - Current chunk index (1-based for display).
        ///   - Characters already copied.
        ///   - Characters still remaining to be copied.
        /// This method is called after every chunk operation so the display
        /// is never stale.
        /// </summary>
        private void UpdateChunkUI()
        {
            int total = _chunks.Count;
            int remaining = total - _chunkIndex;

            int charsCopied = 0;
            for (int i = 0; i < _chunkIndex; i++)
                charsCopied += _chunks[i].Length;

            int charsRemaining = 0;
            for (int i = _chunkIndex; i < _chunks.Count; i++)
                charsRemaining += _chunks[i].Length;

            LblTotalChunks.Text = $"Total chunks: {total}";
            LblChunksLeft.Text = $"Remaining: {remaining} chunk(s)";
            LblCharsCopied.Text = $"Copied: {charsCopied:N0} chars";
            LblCharsRemaining.Text = $"Remaining: {charsRemaining:N0} chars";

            BtnCopyNextChunk.Enabled = total > 0 && _chunkIndex < total;

            if (total > 0 && _chunkIndex < total)
            {
                LblChunkStatus.Text = $"Next → Chunk {_chunkIndex + 1} of {total}  ({_chunks[_chunkIndex].Length:N0} chars)";
                LblChunkStatus.ForeColor = Color.FromArgb(0, 90, 160);
            }
            else if (total == 0)
            {
                LblChunkStatus.Text = "No chunks built yet.";
                LblChunkStatus.ForeColor = Color.Gray;
            }
        }
        #endregion

        // =========================================================================
        // Button Click Handlers – Conversion
        // =========================================================================

        #region BtnConvert_Click  (Mode A)
        /// <summary>
        /// Saves the current text as the restoration snapshot, then applies
        /// Mode A conversion (join lines, preserve comment spacing).
        /// The "Add spaces" checkbox controls whether ALL lines receive a
        /// trailing space or only syntactically significant ones.
        /// </summary>
        private void BtnConvert_Click(object sender, EventArgs e)
        {
            SaveSnapshot();
            TextBoxInputOutput.Text = ConvertParagraphsToLines(
                TextBoxInputOutput.Text, ChkAddSpaces.Checked);
            UpdateStatusBar();
        }
        #endregion

        #region BtnConvert2_Click  (Mode B)
        /// <summary>
        /// Saves the current text as the restoration snapshot, then applies
        /// Mode B conversion (word-by-word maximum compression).
        /// </summary>
        private void BtnConvert2_Click(object sender, EventArgs e)
        {
            SaveSnapshot();
            TextBoxInputOutput.Text = ConvertWordByWord(TextBoxInputOutput.Text);
            UpdateStatusBar();
        }
        #endregion

        #region BtnConvertSkipComments_Click  (Mode C)
        /// <summary>
        /// Saves the current text as the restoration snapshot, then applies
        /// Mode C conversion (word-by-word, skipping license comment blocks).
        /// </summary>
        private void BtnConvertSkipComments_Click(object sender, EventArgs e)
        {
            SaveSnapshot();
            TextBoxInputOutput.Text = ConvertWordByWordSkipCommentBlocks(TextBoxInputOutput.Text);
            UpdateStatusBar();
        }
        #endregion

        #region BtnNormalise_Click  (Mode D)
        /// <summary>
        /// Saves the current text as the restoration snapshot, then applies
        /// Mode D (regex-based whitespace and operator normalisation).
        /// </summary>
        private void BtnNormalise_Click(object sender, EventArgs e)
        {
            SaveSnapshot();
            TextBoxInputOutput.Text = NormaliseAndClean(TextBoxInputOutput.Text);
            UpdateStatusBar();
        }
        #endregion

        // =========================================================================
        // Button Click Handlers – Chunk Panel
        // =========================================================================

        #region BtnBuildChunks_Click
        /// <summary>
        /// Triggers a fresh chunk-build operation based on the current text and
        /// the block-size spinner value.  Automatically applies Mode C
        /// (skip comment blocks) before splitting so the AI receives only the
        /// relevant code.
        /// </summary>
        private void BtnBuildChunks_Click(object sender, EventArgs e)
        {
            SaveSnapshot();
            TextBoxInputOutput.Text = ConvertWordByWordSkipCommentBlocks(TextBoxInputOutput.Text);
            BuildChunks();
            UpdateStatusBar();
        }
        #endregion

        #region BtnCopyNextChunk_Click
        /// <summary>
        /// Delegates to <see cref="CopyNextChunk"/> which copies the next
        /// pending chunk and advances the internal index.
        /// </summary>
        private void BtnCopyNextChunk_Click(object sender, EventArgs e)
        {
            CopyNextChunk();
        }
        #endregion

        #region BtnResetChunks_Click
        /// <summary>
        /// Discards all built chunks and resets the chunk index to 0.
        /// The text-box content is NOT changed – only the chunk session state
        /// is cleared.  Use this if you want to rebuild chunks after editing
        /// the text or changing the block size.
        /// </summary>
        private void BtnResetChunks_Click(object sender, EventArgs e)
        {
            _chunks.Clear();
            _chunkIndex = 0;
            UpdateChunkUI();
            LblChunkStatus.Text = "Chunk session reset.";
            LblChunkStatus.ForeColor = Color.Gray;
        }
        #endregion

        #region NumBlockSize_ValueChanged
        /// <summary>
        /// Fires whenever the block-size spinner value changes.  If a chunk
        /// session is already active it is automatically invalidated and the
        /// user is told to rebuild, preventing silent inconsistencies between
        /// the displayed chunk info and the actual chunks.
        /// </summary>
        private void NumBlockSize_ValueChanged(object sender, EventArgs e)
        {
            if (_chunks.Count > 0)
            {
                _chunks.Clear();
                _chunkIndex = 0;
                LblChunkStatus.Text = "Block size changed – please rebuild chunks.";
                LblChunkStatus.ForeColor = Color.OrangeRed;
                UpdateChunkUI();
            }
        }
        #endregion

        // =========================================================================
        // Button Click Handlers – Utility
        // =========================================================================

        #region BtnClear_Click
        /// <summary>
        /// Clears the text-box and resets all counters and chunk state.
        /// A snapshot is saved first so the user can still restore via BtnRestore
        /// if the clear was accidental.
        /// </summary>
        private void BtnClear_Click(object sender, EventArgs e)
        {
            SaveSnapshot();
            TextBoxInputOutput.Clear();
            _chunks.Clear();
            _chunkIndex = 0;
            UpdateStatusBar();
            UpdateChunkUI();
        }
        #endregion

        #region BtnCopyAll_Click
        /// <summary>
        /// Copies the entire current text-box content to the clipboard in one
        /// operation.  Useful for small texts that fit within the AI's limit
        /// without chunking.
        /// </summary>
        private void BtnCopyAll_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBoxInputOutput.Text))
                Clipboard.SetText(TextBoxInputOutput.Text);
        }
        #endregion

        #region BtnRestore_Click
        /// <summary>
        /// Restores the text to the state saved by the most recent call to
        /// <see cref="SaveSnapshot"/>.  Also clears any active chunk session
        /// because the restored text may differ from what the chunks were built
        /// from.
        /// </summary>
        private void BtnRestore_Click(object sender, EventArgs e)
        {
            if (_originalText == null) return;
            TextBoxInputOutput.Text = _originalText;
            _chunks.Clear();
            _chunkIndex = 0;
            LblChunkStatus.Text = "Text restored to original snapshot.";
            LblChunkStatus.ForeColor = Color.DarkGreen;
            UpdateStatusBar();
            UpdateChunkUI();
        }
        #endregion

        // =========================================================================
        // Search & Replace
        // =========================================================================

        #region ToolStripTextBoxSearch_TextChanged
        /// <summary>
        /// Fires on every keystroke in the toolbar search box.
        /// Performs a case-insensitive search from the beginning of the text
        /// and scrolls to + selects the first match found.
        /// </summary>
        private void ToolStripTextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            string query = ToolStripTextBoxSearch.Text;
            if (string.IsNullOrEmpty(query))
                return;

            string text = TextBoxInputOutput.Text;
            if (string.IsNullOrEmpty(text))
                return;
            
            if (_lastSearchIndex >= text.Length ||
                !text.Substring(_lastSearchIndex)
                     .Contains(query, StringComparison.CurrentCultureIgnoreCase))
            {
                _lastSearchIndex = 0;
            }

            int idx = text.IndexOf(query, _lastSearchIndex, StringComparison.CurrentCultureIgnoreCase);
            if (idx >= 0)
            {
                TextBoxInputOutput.Select(idx, query.Length);
                TextBoxInputOutput.ScrollToCaret();
                _lastSearchIndex = idx + query.Length;
            }
        }
        #endregion

        #region TextReplacementToolStripMenuItem_Click
        /// <summary>
        /// Opens a floating Search & Replace dialog.  The dialog supports:
        ///   - Replace Next: replaces the next occurrence starting from the
        ///     last replaced position, then advances the pointer.
        ///   - Replace All: replaces every occurrence at once and reports the
        ///     count.
        ///   - Reset: resets the incremental search pointer to the start so
        ///     the user can restart a Replace-Next session.
        ///   - Close: dismisses the dialog without further changes.
        /// The dialog is non-modal so the user can still edit the text while
        /// it is open.
        /// </summary>
        private void TextReplacementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxInputOutput.Text))
            {
                MessageBox.Show("The text area is empty. Please enter some text first.",
                    "Nothing to Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // --- Build the dialog form ---
            var dlg = new Form
            {
                Text = "Search & Replace",
                Size = new Size(340, 220),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowIcon = false,
                TopMost = true,
                StartPosition = FormStartPosition.CenterParent,
                Font = new Font("Segoe UI", 9f)
            };

            var lblFind = new Label { Text = "Find:", Left = 12, Top = 14, AutoSize = true };
            var lblReplace = new Label { Text = "Replace with:", Left = 12, Top = 44, AutoSize = true };
            var txtFind = new TextBox { Left = 105, Top = 10, Width = 200 };
            var txtReplace = new TextBox { Left = 105, Top = 40, Width = 200 };
            var lblStatus = new Label { Left = 12, Top = 75, Width = 290, Height = 18, ForeColor = Color.DimGray };

            var btnNext = new Button { Text = "Replace Next", Left = 12, Top = 100, Width = 110 };
            var btnAll = new Button { Text = "Replace All", Left = 128, Top = 100, Width = 90 };
            var btnReset = new Button { Text = "Reset", Left = 224, Top = 100, Width = 60 };
            var btnClose = new Button { Text = "Close", Left = 224, Top = 130, Width = 60 };

            dlg.Controls.AddRange(new Control[]
                { lblFind, lblReplace, txtFind, txtReplace,
                  lblStatus, btnNext, btnAll, btnReset, btnClose });

            // Replace Next
            btnNext.Click += (s, ea) =>
            {
                string find = txtFind.Text;
                string replace = txtReplace.Text;
                if (string.IsNullOrEmpty(find)) return;

                if (_searchReplaceStartIndex >= TextBoxInputOutput.Text.Length)
                    _searchReplaceStartIndex = 0;

                int idx = TextBoxInputOutput.Text.IndexOf(find, _searchReplaceStartIndex,
                              StringComparison.CurrentCultureIgnoreCase);
                if (idx < 0)
                {
                    lblStatus.Text = "No more occurrences found.";
                    lblStatus.ForeColor = Color.OrangeRed;
                    _searchReplaceStartIndex = 0;
                    return;
                }

                TextBoxInputOutput.Select(idx, find.Length);
                TextBoxInputOutput.SelectedText = replace;
                _searchReplaceStartIndex = idx + replace.Length;
                lblStatus.Text = $"Replaced at position {idx}.";
                lblStatus.ForeColor = Color.DarkGreen;
            };

            // Replace All
            btnAll.Click += (s, ea) =>
            {
                string find = txtFind.Text;
                string replace = txtReplace.Text;
                if (string.IsNullOrEmpty(find)) return;

                int count = 0;
                int idx = 0;
                while ((idx = TextBoxInputOutput.Text.IndexOf(find, idx,
                           StringComparison.CurrentCultureIgnoreCase)) >= 0)
                {
                    TextBoxInputOutput.Select(idx, find.Length);
                    TextBoxInputOutput.SelectedText = replace;
                    idx += replace.Length;
                    count++;
                }
                _searchReplaceStartIndex = 0;
                lblStatus.Text = $"Replaced {count} occurrence(s).";
                lblStatus.ForeColor = count > 0 ? Color.DarkGreen : Color.OrangeRed;
            };

            // Reset pointer
            btnReset.Click += (s, ea) =>
            {
                _searchReplaceStartIndex = 0;
                lblStatus.Text = "Search position reset to start.";
                lblStatus.ForeColor = Color.DimGray;
            };

            btnClose.Click += (s, ea) => dlg.Close();

            dlg.Show(this);
        }
        #endregion

        // =========================================================================
        // File I/O (via context menu)
        // =========================================================================

        #region LoadToolStripMenuItem_Click
        /// <summary>
        /// Opens a standard file-open dialog filtered to .txt, .cs and .xml
        /// files and loads the chosen file into the text box.  A snapshot is
        /// saved so the user can restore to the loaded state before any
        /// conversion is applied.
        /// </summary>
        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Text files (*.txt)|*.txt|C# files (*.cs)|*.cs|XML files (*.xml)|*.xml|All files (*.*)|*.*";
                ofd.FilterIndex = 4;
                ofd.RestoreDirectory = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    TextBoxInputOutput.Text = System.IO.File.ReadAllText(ofd.FileName);
                    SaveSnapshot();
                    UpdateStatusBar();
                }
            }
        }
        #endregion

        #region SaveToolStripMenuItem_Click
        /// <summary>
        /// Opens a standard save dialog and writes the current text-box content
        /// to the chosen file.  Supports .txt, .cs and .xml formats.
        /// </summary>
        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text files (*.txt)|*.txt|C# files (*.cs)|*.cs|XML files (*.xml)|*.xml|All files (*.*)|*.*";
                sfd.FilterIndex = 1;
                sfd.RestoreDirectory = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                    System.IO.File.WriteAllText(sfd.FileName, TextBoxInputOutput.Text);
            }
        }
        #endregion

        // =========================================================================
        // Internal Helpers
        // =========================================================================

        #region SaveSnapshot
        /// <summary>
        /// Captures the current text-box content into <see cref="_originalText"/>
        /// so it can be restored later.  Should be called at the start of every
        /// operation that modifies the text.
        /// </summary>
        private void SaveSnapshot()
        {
            _originalText = TextBoxInputOutput.Text;
            _undoStack.Push(TextBoxInputOutput.Text);
            _redoStack.Clear();
        }
        #endregion

        #region UpdateStatusBar
        /// <summary>
        /// Refreshes the character-count label in the status bar to reflect the
        /// current length of the text-box content.  Should be called after every
        /// operation that changes the text.
        /// </summary>
        private void UpdateStatusBar()
        {
            string text = TextBoxInputOutput.Text;

            int charCount = text.Length;

            int charNoWs = 0;
            foreach (char c in text)
            {
                if (!char.IsWhiteSpace(c))
                    charNoWs++;
            }

            int wordCount = 0;
            if (!string.IsNullOrWhiteSpace(text))
            {
                var words = System.Text.RegularExpressions.Regex.Matches(text, @"\b\w+\b");
                wordCount = words.Count;
            }

            int lineCount = 0;
            if (text.Length == 0)
            {
                lineCount = 0;
            }
            else
            {
                lineCount = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Length;
            }

            LblCharCount.Text =
                $"Chars: {charCount:N0}   |   No-WS: {charNoWs:N0}   |   Words: {wordCount:N0}   |   Lines: {lineCount:N0}";
        }

        #endregion

        #region TextBoxInputOutput_TextChanged
        /// <summary>
        /// Fires on every character change in the main text area.
        /// Updates the live character counter in the status strip so the user
        /// always has an up-to-date view of the text length without having to
        /// trigger a conversion.
        /// </summary>
        private void TextBoxInputOutput_TextChanged(object sender, EventArgs e)
        {
            if (!_suppressSnapshot)
                SaveSnapshot();

            UpdateStatusBar();
        }
        #endregion

        #region ApplyTooltips
        private void ApplyTooltips()
        {
            _toolTip.SetToolTip(BtnConvert, "Mode A – Join Lines");
            _toolTip.SetToolTip(BtnConvert2, "Mode B – Max Compress");
            _toolTip.SetToolTip(BtnConvertSkipComments, "Mode C – Skip Comments");
            _toolTip.SetToolTip(BtnNormalise, "Mode D – Normalise");
            _toolTip.SetToolTip(BtnCopyAll, "Copy all text to clipboard");
            _toolTip.SetToolTip(BtnRestore, "Restore original text");
            _toolTip.SetToolTip(BtnClear, "Clear text");
            _toolTip.SetToolTip(BtnBuildChunks, "Build chunks from the current text");
            _toolTip.SetToolTip(BtnCopyNextChunk, "Copy the next chunk to clipboard");
            _toolTip.SetToolTip(BtnResetChunks, "Reset the chunk session");
            _toolTip.SetToolTip(BtnRemoveEmptyLines, "Remove all empty lines from the text");
            _toolTip.SetToolTip(BtnRemoveDuplicateLines, "Remove duplicate lines while keeping order");
            _toolTip.SetToolTip(CmbAiModel, "Select AI model to auto‑set chunk size");
            _toolTip.SetToolTip(BtnAiExplain, "Prepends an AI analysis prompt to the text");
            _toolTip.SetToolTip(BtnUndo, "Undo last change");
            _toolTip.SetToolTip(BtnRedo, "Redo last undone change");
            _toolTip.SetToolTip(BtnDarkMode, "Toggle Dark/Light Mode");
            _toolTip.SetToolTip(BtnAiSafeMode, "Strips comments and empty lines for a cleaner AI input");
            
        }
        #endregion

        #region ToolStripTextBoxSearch_KeyDown
        private void ToolStripTextBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            string query = ToolStripTextBoxSearch.Text;
            if (string.IsNullOrEmpty(query))
                return;

            string text = TextBoxInputOutput.Text;
            if (string.IsNullOrEmpty(text))
                return;

            bool backwards = e.Shift;

            if (backwards)
            {
                int idx = text.LastIndexOf(query, _lastSearchIndex - query.Length,
                    StringComparison.CurrentCultureIgnoreCase);

                if (idx >= 0)
                {
                    TextBoxInputOutput.Select(idx, query.Length);
                    TextBoxInputOutput.ScrollToCaret();
                    _lastSearchIndex = idx;
                }
                else
                {                    
                    int last = text.LastIndexOf(query, StringComparison.CurrentCultureIgnoreCase);
                    if (last >= 0)
                    {
                        TextBoxInputOutput.Select(last, query.Length);
                        TextBoxInputOutput.ScrollToCaret();
                        _lastSearchIndex = last;
                    }
                }
            }
            else
            {
                int idx = text.IndexOf(query, _lastSearchIndex,
                    StringComparison.CurrentCultureIgnoreCase);

                if (idx >= 0)
                {
                    TextBoxInputOutput.Select(idx, query.Length);
                    TextBoxInputOutput.ScrollToCaret();
                    _lastSearchIndex = idx + query.Length;
                }
                else
                {
                    int first = text.IndexOf(query, StringComparison.CurrentCultureIgnoreCase);
                    if (first >= 0)
                    {
                        TextBoxInputOutput.Select(first, query.Length);
                        TextBoxInputOutput.ScrollToCaret();
                        _lastSearchIndex = first + query.Length;
                    }
                }
            }

            e.SuppressKeyPress = true;
        }
        #endregion

        private void BtnRemoveEmptyLines_Click(object sender, EventArgs e)
        {
            SaveSnapshot();

            var lines = TextBoxInputOutput.Text
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            var cleaned = new List<string>();

            foreach (var line in lines)
            {
                if (line.Trim().Length > 0)
                    cleaned.Add(line);
            }

            TextBoxInputOutput.Text = string.Join(Environment.NewLine, cleaned);

            UpdateStatusBar();
        }

        private void BtnRemoveDuplicateLines_Click(object sender, EventArgs e)
        {
            SaveSnapshot();

            var lines = TextBoxInputOutput.Text
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            var seen = new HashSet<string>();
            var unique = new List<string>();

            foreach (var line in lines)
            {
                if (!seen.Contains(line))
                {
                    seen.Add(line);
                    unique.Add(line);
                }
            }

            TextBoxInputOutput.Text = string.Join(Environment.NewLine, unique);

            UpdateStatusBar();
        }

        #region CmbAiModel_SelectedIndexChanged
        private void CmbAiModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            string model = CmbAiModel.SelectedItem.ToString();

            if (_aiLimits.TryGetValue(model, out int limit))
            {
                NumBlockSize.Value = Math.Min(limit, NumBlockSize.Maximum);

                LblChunkStatus.Text = $"AI‑Limit set for {model}: {limit:N0} chars";
                LblChunkStatus.ForeColor = Color.FromArgb(0, 90, 160);
            }
        }
        #endregion

        #region BtnAiSafeMode_Click // Strips comments and empty lines for a cleaner AI input
        private void BtnAiSafeMode_Click(object sender, EventArgs e)
        {
            SaveSnapshot();

            string text = TextBoxInputOutput.Text;

            // Remove /* ... */ block comments
            text = Regex.Replace(text, @"/\*.*?\*/", "", RegexOptions.Singleline);

            // Remove // single-line comments
            text = Regex.Replace(text, @"//.*?$", "", RegexOptions.Multiline);

            // Remove XML comments /// 
            text = Regex.Replace(text, @"///.*?$", "", RegexOptions.Multiline);

            // Remove #region ... #endregion
            text = Regex.Replace(text, @"#region.*?#endregion", "", RegexOptions.Singleline);

            // Remove debug blocks
            text = Regex.Replace(text, @"#if\s+DEBUG.*?#endif", "", RegexOptions.Singleline);

            // Remove banner/license blocks (****)
            text = Regex.Replace(text, @"\*{5,}.*?\*{5,}", "", RegexOptions.Singleline);

            // Remove empty lines
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var cleaned = new List<string>();
            foreach (var line in lines)
            {
                if (line.Trim().Length > 0)
                    cleaned.Add(line);
            }

            TextBoxInputOutput.Text = string.Join(Environment.NewLine, cleaned);

            UpdateStatusBar();
        }
        #endregion

        #region BtnAiExplain_Click // Prepends an AI analysis prompt to the text
        private void BtnAiExplain_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxInputOutput.Text))
            {
                MessageBox.Show("The text area is empty. Please enter some text first.",
                    "Nothing to Explain", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveSnapshot();

            string prompt =
                "Please analyze the following code and explain the logic step by step.\n" +
                "Ignore comments and focus on functionality.\n\n";

            TextBoxInputOutput.Text = prompt + TextBoxInputOutput.Text;

            LblChunkStatus.Text = "AI‑Explain prompt added.";
            LblChunkStatus.ForeColor = Color.FromArgb(0, 90, 160);

            UpdateStatusBar();
        }
        #endregion

        #region Undo/Redo Functionality
        private void Undo()
        {
            if (_undoStack.Count == 0)
                return;

            _suppressSnapshot = true;
            _redoStack.Push(TextBoxInputOutput.Text);
            TextBoxInputOutput.Text = _undoStack.Pop();
            _suppressSnapshot = false;

            UpdateStatusBar();
            UpdateChunkUI();
        }

        private void Redo()
        {
            if (_redoStack.Count == 0)
                return;

            _suppressSnapshot = true;
            _undoStack.Push(TextBoxInputOutput.Text);
            TextBoxInputOutput.Text = _redoStack.Pop();
            _suppressSnapshot = false;

            UpdateStatusBar();
            UpdateChunkUI();
        }
        #endregion

        #region BtnDarkMode_Click // Toggles between dark and light themes
        private void BtnDarkMode_Click(object sender, EventArgs e)
        {
            _darkMode = !_darkMode;
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            Color back = _darkMode ? DarkBack : LightBack;
            Color panel = _darkMode ? DarkPanel : LightPanel;
            Color text = _darkMode ? DarkText : LightText;

            // Form
            this.BackColor = back;

            // Panels
            PnlHeader.BackColor = panel;
            PnlToolbar.BackColor = panel;
            PnlChunks.BackColor = panel;

            // Textbox
            TextBoxInputOutput.BackColor = back;
            TextBoxInputOutput.ForeColor = text;
            TextBoxInputOutput.BorderStyle = BorderStyle.FixedSingle;

            // Labels
            foreach (Control c in this.Controls)
                ApplyThemeToControl(c, back, panel, text);

            // StatusStrip
            StatusStrip1.BackColor = panel;
            foreach (ToolStripItem item in StatusStrip1.Items)
                item.ForeColor = text;

            // Buttons
            StyleButtonsForTheme(text, panel);
        }

        private void ApplyThemeToControl(Control c, Color back, Color panel, Color text)
        {
            if (c is Label)
                c.ForeColor = text;

            if (c is Panel)
                c.BackColor = panel;

            if (c is NumericUpDown nud)
            {
                nud.BackColor = back;
                nud.ForeColor = text;
            }

            if (c is ComboBox cb)
            {
                cb.BackColor = back;
                cb.ForeColor = text;
            }

            if (c is TextBox tb)
            {
                tb.BackColor = back;
                tb.ForeColor = text;
            }

            foreach (Control child in c.Controls)
                ApplyThemeToControl(child, back, panel, text);
        }

        private void StyleButtonsForTheme(Color text, Color panel)
        {
            foreach (Control c in this.Controls)
                StyleButtonsRecursive(c, text, panel);
        }

        private void StyleButtonsRecursive(Control parent, Color text, Color panel)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Button b)
                {
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderColor = _darkMode ? DarkBorder : Color.Gray;
                    b.BackColor = panel;
                    b.ForeColor = text;
                }

                StyleButtonsRecursive(c, text, panel);
            }
        }
        #endregion
    }
}
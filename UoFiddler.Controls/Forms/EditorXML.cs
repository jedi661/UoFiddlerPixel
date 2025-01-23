// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  * Advanced:
//  * 
//  * \"THE BEER-WINE-WARE LICENSE\"
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
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace UoFiddler.Controls.Forms
{
    public partial class EditorXML : Form
    {
        private readonly string outputPath;
        private int _lastIndex = -1;
        private string _searchText = string.Empty;
        private int _foundCount = 0;
        private bool _isDarkMode = false;
        private Timer autoSaveTimer;
        private string currentFilePath;
        private bool _caseSensitive = false;
        private bool _useRegex = false;
        private Stack<string> undoStack = new Stack<string>();
        private Stack<string> redoStack = new Stack<string>();
        private bool isManualChange = true;

        public EditorXML(string outputPath)
        {
            InitializeComponent();
            this.outputPath = outputPath;
            this.KeyPreview = true;  // Add KeyDown event for F5, F7, F8 functionality
            InitializeAutoSave();

            // Save initial state if text exists
            if (!string.IsNullOrEmpty(richTextBoxXmlContent.Text))
            {
                SaveCurrentState();
            }
        }

        #region [ AutoSaveTimer ]
        private void InitializeAutoSave()
        {
            autoSaveTimer = new Timer
            {
                Interval = 1800000 // 30 Min
            };
            autoSaveTimer.Tick += AutoSaveTimer_Tick;
            autoSaveTimer.Tag = DateTime.Now;
        }

        private void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            autoSaveTimer.Tag = DateTime.Now;

            if (!string.IsNullOrEmpty(currentFilePath))
            {
                File.WriteAllText(currentFilePath, richTextBoxXmlContent.Text);
                UpdateStatus("Auto-saved at " + DateTime.Now.ToString("HH:mm:ss"));
                MessageBox.Show("Auto-save completed!", "Auto-Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No file loaded to auto-save.", "Auto-Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region [ checkBoxAutoSave_CheckedChanged ]
        private void checkBoxAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAutoSave.Checked)
            {
                autoSaveTimer.Start();
                MessageBox.Show("Auto-save has been enabled.", "Auto-Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                autoSaveTimer.Stop();
                MessageBox.Show("Auto-save has been disabled.", "Auto-Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region [ KeyDown Handler for Function Keys ]
        private void EditorXML_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                findToolStripMenuItem_Click(sender, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                resetToolStripMenuItem_Click(sender, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.F6)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                designToolStripMenuItem_Click(sender, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.F7)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                printToolStripMenuItem_Click(sender, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.F8)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                saveToolStripMenuItem_Click(sender, EventArgs.Empty);
            }
            if (e.KeyCode == Keys.F10)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                ShowAutoSaveInformation();
            }
            if (e.Control && e.KeyCode == Keys.Z)
            {
                e.SuppressKeyPress = true; // Suppress the standard sound
                undoToolStripMenuItem_Click(sender, e);
            }
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                e.SuppressKeyPress = true;
                redoToolStripMenuItem_Click(sender, e);
            }
        }
        #endregion

        #region [ resetToolStripMenuItem_Click ]
        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetState();
            _searchText = string.Empty;
            toolStripTextBoxFind.Text = string.Empty;
            UpdateStatus("Reset complete. Ready for new search.");
        }
        #endregion

        #region [ loadToolStripMenuItem_Click ]
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string initialDirectory = checkBoxOutputPath.Checked ? outputPath : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\UoFiddler";
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XML Files|*.xml",
                Title = "Select an XML File",
                InitialDirectory = initialDirectory
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    currentFilePath = openFileDialog.FileName;
                    string xmlContent = File.ReadAllText(openFileDialog.FileName);

                    if (checkBoxXMLFormatted.Checked)
                    {
                        richTextBoxXmlContent.Rtf = ConvertXmlToRtf(xmlContent);
                    }
                    else
                    {
                        richTextBoxXmlContent.Text = xmlContent; // Show original content without conversion
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region [ saveToolStripMenuItem_Click ]
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string initialDirectory = checkBoxOutputPath.Checked ? outputPath : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\UoFiddler";
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "XML Files|*.xml",
                Title = "Save XML File",
                InitialDirectory = initialDirectory
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, richTextBoxXmlContent.Text);
                    currentFilePath = saveFileDialog.FileName;
                    MessageBox.Show("File saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region [ ShowSaveInformation ]
        private void ShowAutoSaveInformation()
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                MessageBox.Show("No file is currently loaded.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            FileInfo fileInfo = new FileInfo(currentFilePath);
            DateTime timerStart = (DateTime)autoSaveTimer.Tag;
            TimeSpan timeElapsed = DateTime.Now - timerStart;
            int timeRemainingInSeconds = autoSaveTimer.Interval / 1000 - (int)timeElapsed.TotalSeconds;

            if (timeRemainingInSeconds < 0) timeRemainingInSeconds = 0;
            int minutesRemaining = timeRemainingInSeconds / 60;
            int secondsRemaining = timeRemainingInSeconds % 60;

            MessageBox.Show(
                $"Auto-Save Information:\n\n" +
                $"- Directory: {fileInfo.DirectoryName}\n" +
                $"- File Name: {fileInfo.Name}\n" +
                $"- File Size: {fileInfo.Length / 1024} KB\n" +
                $"- Last Modified: {fileInfo.LastWriteTime}\n" +
                $"- Time Remaining: {minutesRemaining} minutes and {secondsRemaining} seconds\n",
                "Auto-Save Status",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        #endregion

        #region [ printToolStripMenuItem_Click ]
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += printDocument_PrintPage;

            PrintDialog printDialog = new PrintDialog
            {
                Document = printDocument
            };

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    printDocument.Print();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error printing document: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void printDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawString(richTextBoxXmlContent.Text, new Font("Arial", 12), Brushes.Black, new RectangleF(0, 0, e.PageBounds.Width, e.PageBounds.Height));
        }
        #endregion

        #region [ findToolStripMenuItem_Click ]
        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(toolStripTextBoxFind.Text))
                return;

            if (_searchText != toolStripTextBoxFind.Text.Trim())
            {
                ResetState();
                _searchText = toolStripTextBoxFind.Text.Trim();
                CountOccurrences();
            }

            int searchStartIndex = _lastIndex + 1;
            StringComparison comparison = _caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            if (_useRegex)
            {
                try
                {
                    Regex regex = new Regex(_searchText, _caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                    Match match = regex.Match(richTextBoxXmlContent.Text, searchStartIndex);
                    if (match.Success)
                    {
                        _lastIndex = match.Index;
                        HighlightWord(_lastIndex, match.Length);
                        UpdateStatus($"Regex match found at position: {_lastIndex + 1}");
                    }
                    else
                    {
                        MessageBox.Show("End of document reached. Start over?", "End of Search", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        ResetState();
                    }
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show($"Invalid regex: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                _lastIndex = richTextBoxXmlContent.Text.IndexOf(_searchText, searchStartIndex, comparison);
                if (_lastIndex != -1)
                {
                    HighlightWord(_lastIndex, _searchText.Length);
                    UpdateStatus($"Word '{_searchText}' found at position: {_lastIndex + 1}");
                }
                else
                {
                    MessageBox.Show("End of document reached. Start over?", "End of Search", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    ResetState();
                }
            }
        }

        private void HighlightWord(int index, int length)
        {
            richTextBoxXmlContent.Select(index, length);
            richTextBoxXmlContent.SelectionBackColor = Color.Yellow;
            richTextBoxXmlContent.ScrollToCaret();
        }

        private void UpdateStatus(string message)
        {
            toolStripStatusLabelInfo.Text = message;
            toolStripStatusWord.Text = $"Total {_searchText}: {_foundCount}";
        }
        #endregion

        #region [ ResetHighlighting ]
        private void ResetHighlighting()
        {
            richTextBoxXmlContent.SelectAll();
            richTextBoxXmlContent.SelectionBackColor = Color.White;
            richTextBoxXmlContent.SelectionLength = 0;
        }
        #endregion

        #region [ CountOccurrences ]
        private void CountOccurrences()
        {
            ResetHighlighting();
            int startIndex = 0;
            _foundCount = 0;
            while (startIndex < richTextBoxXmlContent.Text.Length && (startIndex = richTextBoxXmlContent.Text.IndexOf(_searchText, startIndex, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                startIndex += _searchText.Length;
                _foundCount++;
            }
            UpdateStatus($"Total occurrences of '{_searchText}': {_foundCount}");
        }
        #endregion

        #region [ toolStripTextBoxFind_TextChanged ]
        private void toolStripTextBoxFind_TextChanged(object sender, EventArgs e)
        {
            ResetState();
            _searchText = toolStripTextBoxFind.Text.Trim();
            CountOccurrences();
        }

        private void ResetState()
        {
            ResetHighlighting();
            _foundCount = 0;
            toolStripStatusWord.Text = "Total: 0";
            _lastIndex = -1;
        }
        #endregion

        #region [ richTextBoxXmlContent_TextChanged ]
        private void richTextBoxXmlContent_TextChanged(object sender, EventArgs e)
        {
            // Prevent Undo/Redo from triggering the event
            if (isManualChange)
            {
                SaveCurrentState();
            }
            UpdateTextStatistics();
        }
        #endregion

        #region [ SaveInitialState ]
        private void SaveInitialState()
        {
            if (isManualChange && undoStack.Count == 0)
            {
                undoStack.Push(richTextBoxXmlContent.Text);
            }
        }
        #endregion

        #region [ UpdateTextStatistics ]
        private void UpdateTextStatistics()
        {
            int wordCount = richTextBoxXmlContent.Text.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
            int characterCount = richTextBoxXmlContent.Text.Length;
            int lineCount = richTextBoxXmlContent.Lines.Length;
            int xmlTagCount = Regex.Matches(richTextBoxXmlContent.Text, @"<[^>]+>").Count;

            toolStripStatusLabelTextStatistics.Text = $"Words: {wordCount}, Characters: {characterCount}, Lines: {lineCount}, XML Tags: {xmlTagCount}";
        }
        #endregion

        #region [ designToolStripMenuItem ]
        private void ToggleDarkMode()
        {
            if (_isDarkMode)
            {
                // Light Mode
                this.BackColor = SystemColors.Control;
                richTextBoxXmlContent.BackColor = Color.White;
                richTextBoxXmlContent.ForeColor = Color.Black;
                contextMenuStripXMLEdit.BackColor = SystemColors.Control;
                contextMenuStripXMLEdit.ForeColor = SystemColors.ControlText;
            }
            else
            {
                // Dark Mode
                this.BackColor = Color.FromArgb(41, 44, 51);
                richTextBoxXmlContent.BackColor = Color.FromArgb(30, 30, 30);
                richTextBoxXmlContent.ForeColor = Color.FromArgb(230, 230, 230);
                contextMenuStripXMLEdit.BackColor = Color.FromArgb(30, 30, 30);
                contextMenuStripXMLEdit.ForeColor = Color.White;
            }
            _isDarkMode = !_isDarkMode;
        }

        private void designToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleDarkMode();
        }
        #endregion

        #region [ SaveCurrentState ]
        private void SaveCurrentState()
        {
            if (isManualChange)
            {
                undoStack.Push(richTextBoxXmlContent.Text);
                redoStack.Clear(); // Empty redo stack when a new change is made
                UpdateMenuState();
            }
        }
        #endregion

        #region [ undoToolStripMenuItem_Click ]
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (undoStack.Count > 0)
            {
                isManualChange = false;
                redoStack.Push(richTextBoxXmlContent.Text);
                richTextBoxXmlContent.Text = undoStack.Pop();
                UpdateMenuState();
                isManualChange = true;
            }
        }
        #endregion

        #region [ redoToolStripMenuItem_Click ]
        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (redoStack.Count > 0)
            {
                isManualChange = false;
                undoStack.Push(richTextBoxXmlContent.Text);
                richTextBoxXmlContent.Text = redoStack.Pop();
                UpdateMenuState();
                isManualChange = true;
            }
        }
        #endregion

        #region [ UpdateMenuState ]
        private void UpdateMenuState()
        {
            undoToolStripMenuItem.Enabled = undoStack.Count > 0;
            redoToolStripMenuItem.Enabled = redoStack.Count > 0;
        }
        #endregion

        #region [ ConvertXmlToRtf ]
        private string ConvertXmlToRtf(string xmlContent)
        {
            var rtfBuilder = new System.Text.StringBuilder();
            rtfBuilder.Append(@"{\rtf1\ansi\deff0{\fonttbl{\f0 Courier New;}}");
            rtfBuilder.Append(@"{\colortbl ;\red0\green0\blue0;\red0\green0\blue255;\red255\green0\blue0;\red0\green128\blue0;\red128\green128\blue128;}");
            rtfBuilder.Append(@"\viewkind4\uc1\pard\f0\fs20 ");

            // Process XML declaration once
            Match xmlDeclarationMatch = Regex.Match(xmlContent, @"<\?xml[^>]+>");
            if (xmlDeclarationMatch.Success)
            {
                rtfBuilder.Append(@"\cf5 "); // Comment color
                rtfBuilder.Append(EscapeRtf(xmlDeclarationMatch.Value));
                rtfBuilder.Append(@"\par\cf1 ");
                xmlContent = xmlContent.Replace(xmlDeclarationMatch.Value, ""); // Remove declaration from main content
            }

            // Main content of the XML
            foreach (Match match in Regex.Matches(xmlContent, @"<[^>]+>|<!--.*?-->|[^<]+"))
            {
                string fragment = match.Value;

                if (fragment.StartsWith("<"))
                {
                    if (fragment.StartsWith("<!--")) // Comments
                    {
                        rtfBuilder.Append(@"\cf5 "); // Comment color
                        rtfBuilder.Append(EscapeRtf(fragment));
                        rtfBuilder.Append(@"\cf1 ");
                    }
                    else if (fragment.StartsWith("</")) // Closing tags
                    {
                        rtfBuilder.Append(@"\cf2 "); // Tag color
                        rtfBuilder.Append(EscapeRtf(fragment));
                        rtfBuilder.Append(@"\cf1 ");
                    }
                    else // Opening tags
                    {
                        Match tagNameMatch = Regex.Match(fragment, @"<\s*[^!?/\s>]+");
                        if (tagNameMatch.Success)
                        {
                            rtfBuilder.Append(@"\cf2 "); // Tag color
                            rtfBuilder.Append(EscapeRtf(tagNameMatch.Value));
                        }

                        // Process attributes within the tag
                        foreach (Match attrMatch in Regex.Matches(fragment, @"\b\w+\s*=\s*""[^""]*"""))
                        {
                            rtfBuilder.Append(@" "); // Spaces between attributes
                            rtfBuilder.Append(@"\cf3 "); // Attribute color
                            rtfBuilder.Append(EscapeRtf(attrMatch.Value));
                        }

                        // Closing characters of the tag
                        if (fragment.EndsWith("/>") || fragment.EndsWith(">"))
                        {
                            rtfBuilder.Append(@"\cf2 ");
                            rtfBuilder.Append(EscapeRtf(fragment.EndsWith("/>") ? " />" : ">"));
                            rtfBuilder.Append(@"\cf1 ");
                        }
                    }
                }
                else // Text outside tags
                {
                    rtfBuilder.Append(EscapeRtf(fragment));
                }
            }

            rtfBuilder.Append(@"}");
            return rtfBuilder.ToString();
        }

        private string EscapeRtf(string text)
        {
            return text.Replace(@"\", @"\\")
                       .Replace("{", @"\{")
                       .Replace("}", @"\}")
                       .Replace("\t", @"\tab ")
                       .Replace("\n", @"\par ");
        }
        #endregion

        #region [ copyClipboardToolStripMenuItem_Click ]
        private void copyClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Überprüfen, ob es Text in der RichTextBox gibt
                if (!string.IsNullOrEmpty(richTextBoxXmlContent.Text))
                {
                    // Kopieren des Inhalts in den Zwischenspeicher
                    Clipboard.SetText(richTextBoxXmlContent.Text);
                    MessageBox.Show("Text copied to clipboard.", "Copy to Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("There is no text to copy.", "Copy to Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying text to clipboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}

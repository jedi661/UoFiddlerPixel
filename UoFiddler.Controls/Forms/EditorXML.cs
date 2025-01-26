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
using System.Linq;

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

        #region [ addNewLineToolStripMenuItem_Click ]        
        private void addNewLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form addForm = new Form
            {
                Text = "Add New Line",
                Width = 400,
                Height = 280, // increased height to accommodate the info label
                FormBorderStyle = FormBorderStyle.FixedDialog, // Fixed border style
                MaximizeBox = false, // Disable maximize button
                MinimizeBox = false  // Disable minimize button
            };

            Label labelName = new Label { Text = "Mob Name:", Top = 20, Left = 10, Width = 100 };
            TextBox textBoxMobName = new TextBox { Top = 20, Left = 120, Width = 250 };

            Label labelBody = new Label { Text = "Body ID:", Top = 60, Left = 10, Width = 100 };
            TextBox textBoxBody = new TextBox { Top = 60, Left = 120, Width = 250 };

            Label labelType = new Label { Text = "Type:", Top = 100, Left = 10, Width = 100 };
            ComboBox comboBoxType = new ComboBox
            {
                Top = 100,
                Left = 120,
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboBoxType.Items.AddRange(new object[] { "Monster", "Sea", "Animal", "Human/Equipment" });
            comboBoxType.SelectedIndex = 0;

            Label labelInfo = new Label
            {
                Text = "0 = Monster, 1 = Sea, 2 = Animal, 3 = Human - Equipment",
                Top = 140,
                Left = 10,
                Width = 320
            };

            Button buttonAdd = new Button { Text = "Add", Top = 180, Left = 150, Width = 100 };
            buttonAdd.Click += (formSender, formE) =>
            {
                if (int.TryParse(textBoxBody.Text, out int bodyId))
                {
                    int type = comboBoxType.SelectedIndex;
                    string tagName = type == 3 ? "Equip" : "Mob";

                    if (IsBodyIdTaken(bodyId, tagName))
                    {
                        MessageBox.Show($"The Body ID is already taken in {tagName}. Please choose a different ID.", "Duplicate Body ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string mobName = $"{textBoxMobName.Text} ({bodyId})";
                    string newMobLine = $"  <{tagName} name=\"{mobName}\" body=\"{bodyId}\" type=\"{type}\" />"; // Add leading spaces
                    AddLineToXml(newMobLine, bodyId, tagName);
                    addForm.Close();
                }
                else
                {
                    MessageBox.Show("Please enter a valid Body ID.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            addForm.Controls.Add(labelName);
            addForm.Controls.Add(textBoxMobName);
            addForm.Controls.Add(labelBody);
            addForm.Controls.Add(textBoxBody);
            addForm.Controls.Add(labelType);
            addForm.Controls.Add(comboBoxType);
            addForm.Controls.Add(labelInfo);
            addForm.Controls.Add(buttonAdd);
            addForm.ShowDialog();
        }

        private void AddLineToXml(string newLine, int bodyId, string tagName)
        {
            var lines = richTextBoxXmlContent.Lines.ToList();
            bool lineAdded = false;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                int currentBodyId;
                string currentTag;

                if (TryGetBodyIdAndTagFromLine(line, out currentBodyId, out currentTag) && currentTag == tagName && currentBodyId > bodyId)
                {
                    lines.Insert(i, newLine);
                    lineAdded = true;
                    break;
                }
            }

            if (!lineAdded)
            {
                lines.Add(newLine); // Add at the end if no appropriate place is found
            }

            richTextBoxXmlContent.Lines = lines.ToArray();
        }

        private bool TryGetBodyIdAndTagFromLine(string line, out int bodyId, out string tagName)
        {
            var match = Regex.Match(line, @"<(?<tag>\w+) name=""[^""]*"" body=""(?<body>\d+)""");
            if (match.Success)
            {
                bodyId = int.Parse(match.Groups["body"].Value);
                tagName = match.Groups["tag"].Value;
                return true;
            }

            bodyId = 0;
            tagName = null;
            return false;
        }

        private bool IsBodyIdTaken(int bodyId, string tagName)
        {
            return richTextBoxXmlContent.Text.Contains($"<{tagName} name=\"") && richTextBoxXmlContent.Text.Contains($"body=\"{bodyId}\"");
        }
        #endregion

        #region [ keyDownToolStripMenuItem_Click ]
        private void keyDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form infoForm = new Form
            {
                Text = "Key Bindings",
                Width = 400,
                Height = 400,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label labelInfo = new Label
            {
                Text = GetEnglishKeyBindings(),
                Top = 20,
                Left = 20,
                Width = 360,
                Height = 300
            };

            Button buttonSwitchLanguage = new Button
            {
                Text = "Switch to German",
                Top = 330,
                Left = 150,
                Width = 100
            };

            buttonSwitchLanguage.Click += (btnSender, btnE) =>
            {
                if (labelInfo.Text == GetEnglishKeyBindings())
                {
                    labelInfo.Text = GetGermanKeyBindings();
                    buttonSwitchLanguage.Text = "Wechseln zu Englisch";
                }
                else
                {
                    labelInfo.Text = GetEnglishKeyBindings();
                    buttonSwitchLanguage.Text = "Switch to German";
                }
            };

            infoForm.Controls.Add(labelInfo);
            infoForm.Controls.Add(buttonSwitchLanguage);
            infoForm.ShowDialog();
        }

        private string GetEnglishKeyBindings()
        {
            return
            "F3: Search\n" +
            "F5: Reset\n" +
            "F6: Toggle Design\n" +
            "F7: Print\n" +
            "F8: Save\n" +
            "F10: Show AutoSave Information\n" +
            "Ctrl + Z: Undo\n" +
            "Ctrl + Y: Redo";
        }

        private string GetGermanKeyBindings()
        {
            return
            "F3: Suchen\n" +
            "F5: Zurücksetzen\n" +
            "F6: Design wechseln\n" +
            "F7: Drucken\n" +
            "F8: Speichern\n" +
            "F10: Autosave-Informationen anzeigen\n" +
            "Strg + Z: Rückgängig\n" +
            "Strg + Y: Wiederherstellen";
        }
        #endregion

        #region [ searchAndReplaceToolStripMenuItem_Click ]
        private void searchAndReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Stack for storing previous states
            Stack<string> undoStack = new Stack<string>();

            // Create shape
            Form searchReplaceForm = new Form
            {
                Text = "Search and Replace",
                Width = 500,
                Height = 450,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            // search text label
            Label labelSearch = new Label
            {
                Text = "Search:",
                Top = 20,
                Left = 10,
                Width = 100
            };
            TextBox textBoxSearch = new TextBox
            {
                Top = 20,
                Left = 120,
                Width = 300
            };

            // label for replacement text
            Label labelReplace = new Label
            {
                Text = "Replace with:",
                Top = 60,
                Left = 10,
                Width = 100
            };
            TextBox textBoxReplace = new TextBox
            {
                Top = 60,
                Left = 120,
                Width = 300
            };

            // checkbox for case sensitivity
            CheckBox checkBoxCaseSensitive = new CheckBox
            {
                Text = "Case Sensitive",
                Top = 100,
                Left = 120,
                Width = 150
            };

            // checkbox for step-by-step replacement
            CheckBox checkBoxStepByStep = new CheckBox
            {
                Text = "Step by Step Replace",
                Top = 130,
                Left = 120,
                Width = 200
            };

            // Label for number of results found
            Label labelMatches = new Label
            {
                Text = "Matches Found: 0",
                Top = 170,
                Left = 10,
                Width = 200
            };

            // Label for number of replaced results
            Label labelReplaced = new Label
            {
                Text = "Replaced: 0",
                Top = 200,
                Left = 10,
                Width = 200
            };

            // Buttons
            Button buttonFindMatches = new Button
            {
                Text = "Find Matches",
                Top = 240,
                Left = 50,
                Width = 150
            };
            Button buttonExecuteReplace = new Button
            {
                Text = "Replace",
                Top = 240,
                Left = 250,
                Width = 150
            };
            Button buttonUndo = new Button
            {
                Text = "Undo",
                Top = 300,
                Left = 170,
                Width = 150,
                Enabled = false // Disabled if no state is available
            };

            // "Find Matches" button - event handler
            buttonFindMatches.Click += (formSender, formE) =>
            {
                string searchText = textBoxSearch.Text;
                if (string.IsNullOrEmpty(searchText))
                {
                    MessageBox.Show("Please enter a search term.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                StringComparison comparison = checkBoxCaseSensitive.Checked
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;

                int matches = 0;
                int startIndex = 0;
                string content = richTextBoxXmlContent.Text;

                while ((startIndex = content.IndexOf(searchText, startIndex, comparison)) != -1)
                {
                    matches++;
                    startIndex += searchText.Length;
                }

                labelMatches.Text = $"Matches Found: {matches}";
            };

            // "Replace"-Button-Logik
            buttonExecuteReplace.Click += (formSender, formE) =>
            {
                string searchText = textBoxSearch.Text;
                string replaceText = textBoxReplace.Text;
                if (string.IsNullOrEmpty(searchText))
                {
                    MessageBox.Show("Please enter a search term.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                StringComparison comparison = checkBoxCaseSensitive.Checked
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;

                int matches = 0;
                int replacements = 0;
                string content = richTextBoxXmlContent.Text;

                // Save the current state for "Undo"
                undoStack.Push(content);
                buttonUndo.Enabled = true; // Activate "Undo" button

                // Step-by-step replacement
                if (checkBoxStepByStep.Checked)
                {
                    int startIndex = 0;
                    while ((startIndex = content.IndexOf(searchText, startIndex, comparison)) != -1)
                    {
                        matches++;
                        richTextBoxXmlContent.Select(startIndex, searchText.Length);
                        richTextBoxXmlContent.ScrollToCaret();

                        DialogResult result = MessageBox.Show(
                            $"Match found at position {startIndex + 1}.\nReplace this occurrence?",
                            "Replace Confirmation",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            content = content.Remove(startIndex, searchText.Length)
                                             .Insert(startIndex, replaceText);
                            replacements++;
                            startIndex += replaceText.Length;
                        }
                        else if (result == DialogResult.No)
                        {
                            startIndex += searchText.Length;
                        }
                        else if (result == DialogResult.Cancel)
                        {
                            break;
                        }
                    }

                    // Asks whether all remaining deposits should be replaced
                    if (matches > replacements)
                    {
                        DialogResult replaceAll = MessageBox.Show(
                            $"There are {matches - replacements} matches left.\nReplace all remaining occurrences?",
                            "Replace All Confirmation",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (replaceAll == DialogResult.Yes)
                        {
                            content = content.Replace(searchText, replaceText);
                            replacements = matches;
                        }
                    }
                }
                else // Automatic replacement of all occurrences
                {
                    matches = Regex.Matches(content, Regex.Escape(searchText),
                        checkBoxCaseSensitive.Checked ? RegexOptions.None : RegexOptions.IgnoreCase).Count;

                    content = checkBoxCaseSensitive.Checked
                        ? content.Replace(searchText, replaceText)
                        : Regex.Replace(content, Regex.Escape(searchText), replaceText, RegexOptions.IgnoreCase);

                    replacements = matches;
                }

                richTextBoxXmlContent.Text = content;
                labelMatches.Text = $"Matches Found: {matches}";
                labelReplaced.Text = $"Replaced: {replacements}";

                MessageBox.Show($"Replaced {replacements} occurrences of '{searchText}' with '{replaceText}'.",
                    "Replace Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            };

            // "Undo"-Button-Logik
            buttonUndo.Click += (formSender, formE) =>
            {
                if (undoStack.Count > 0)
                {
                    richTextBoxXmlContent.Text = undoStack.Pop();
                    buttonUndo.Enabled = undoStack.Count > 0; // Disable button if no other states exist
                }
            };

            // Add the controls to the form
            searchReplaceForm.Controls.Add(labelSearch);
            searchReplaceForm.Controls.Add(textBoxSearch);
            searchReplaceForm.Controls.Add(labelReplace);
            searchReplaceForm.Controls.Add(textBoxReplace);
            searchReplaceForm.Controls.Add(checkBoxCaseSensitive);
            searchReplaceForm.Controls.Add(checkBoxStepByStep);
            searchReplaceForm.Controls.Add(labelMatches);
            searchReplaceForm.Controls.Add(labelReplaced);
            searchReplaceForm.Controls.Add(buttonFindMatches);
            searchReplaceForm.Controls.Add(buttonExecuteReplace);
            searchReplaceForm.Controls.Add(buttonUndo);

            searchReplaceForm.ShowDialog();
        }
        #endregion
    }
}

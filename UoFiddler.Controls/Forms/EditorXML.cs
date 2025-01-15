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

        public EditorXML(string outputPath)
        {
            InitializeComponent();
            this.outputPath = outputPath;

            // Add KeyDown event for F5, F7, F8 functionality
            this.KeyPreview = true;

            InitializeAutoSave();
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
                    richTextBoxXmlContent.Text = xmlContent;
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

            // Get directory and file name information
            string directory = Path.GetDirectoryName(currentFilePath);
            string fileName = Path.GetFileName(currentFilePath);

            // Calculate the remaining time until the next auto-save
            DateTime timerStart = (DateTime)autoSaveTimer.Tag; // Ensure Tag is a DateTime
            TimeSpan timeElapsed = DateTime.Now - timerStart;
            int timeRemainingInSeconds = autoSaveTimer.Interval / 1000 - (int)timeElapsed.TotalSeconds;

            if (timeRemainingInSeconds < 0) timeRemainingInSeconds = 0; // Ensure no negative values
            
            int minutesRemaining = timeRemainingInSeconds / 60;
            int secondsRemaining = timeRemainingInSeconds % 60;

            // Display the information in a MessageBox
            MessageBox.Show(
                $"Auto-Save Information:\n\n" +
                $"- Directory: {directory}\n" +
                $"- File Name: {fileName}\n" +
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
            _lastIndex = richTextBoxXmlContent.Text.IndexOf(_searchText, searchStartIndex, StringComparison.OrdinalIgnoreCase);

            if (_lastIndex != -1)
            {
                HighlightWord(_lastIndex, _searchText.Length);
                UpdateStatus($"Word '{_searchText}' found at position: {_lastIndex + 1}");
            }
            else
            {
                MessageBox.Show("End of document reached. Start over?", "End of Search", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                ResetState();
                CountOccurrences();
                _lastIndex = -1;
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
            UpdateTextStatistics();
        }
        #endregion

        #region [ UpdateTextStatistics ]
        private void UpdateTextStatistics()
        {
            int wordCount = richTextBoxXmlContent.Text.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
            int characterCount = richTextBoxXmlContent.Text.Length;
            int lineCount = richTextBoxXmlContent.Lines.Length;

            toolStripStatusLabelTextStatistics.Text = $"Words: {wordCount}, Characters: {characterCount}, Lines: {lineCount}";
        }
        #endregion

        #region [ designToolStripMenuItem ]
        private void ToggleDarkMode()
        {
            if (_isDarkMode)
            {
                // Setze helle Farben
                this.BackColor = SystemColors.Control;
                richTextBoxXmlContent.BackColor = Color.White;
                richTextBoxXmlContent.ForeColor = Color.Black;
            }
            else
            {
                // Setze dunkle Farben
                this.BackColor = Color.FromArgb(41, 44, 51);
                richTextBoxXmlContent.BackColor = Color.FromArgb(30, 30, 30);
                richTextBoxXmlContent.ForeColor = Color.FromArgb(230, 230, 230);
            }

            _isDarkMode = !_isDarkMode;
        }

        private void designToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleDarkMode();
        }
        #endregion
    }
}

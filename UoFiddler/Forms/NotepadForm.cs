// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  * 
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Windows.Documents;
using System.Drawing.Printing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Ultima;

namespace UoFiddler.Forms
{

    public partial class NotepadForm : Form
    {
        private XmlDocument doc = new XmlDocument();
        // Bold Color
        private bool isBoldActive = false;
        // Underline Color
        private bool isUnderlineActive = false;
        // Italic
        private bool isItalicActive = false;
        // Bullet
        private bool isBulletActive = false;
        // Line Number
        bool isLineNumberActive = false;

        // Create a new DateTimePicker
        DateTimePicker dateTimePicker = new DateTimePicker();

        // Global variable to save the form
        private System.Windows.Forms.Form replaceForm = null;

        public NotepadForm()
        {
            InitializeComponent();

            // Add the event handler
            richTextBoxNotPad.TextChanged += new EventHandler(richTextBoxNotPad_TextChanged);
            listBoxLineNumbers.SelectedIndexChanged += new EventHandler(listBoxLineNumbers_SelectedIndexChanged);

            // Add the event handler
            richTextBoxNotPad.TextChanged += new EventHandler(richTextBoxNotPad_TextChanged);

            // Create an instance of ToolTip
            System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();

            // Set the tooltip for the button
            ToolTip1.SetToolTip(this.btEdit, "For editing entries that re-save the selected ID");
            ToolTip1.SetToolTip(this.btDelete, "To delete entries with the registered ID");
            ToolTip1.SetToolTip(this.btSaveText, "To save the current entry");
            ToolTip1.SetToolTip(this.btBold, "To toggle the bold style");
            ToolTip1.SetToolTip(this.BtUnderline, "To toggle the underline style");
            ToolTip1.SetToolTip(this.BtItalic, "To toggle the italic style");
            ToolTip1.SetToolTip(this.btLoad, "To load texts");
            ToolTip1.SetToolTip(this.btFont, "Changes the font");
            ToolTip1.SetToolTip(this.btFontColor, "Changes the color font");
            ToolTip1.SetToolTip(this.colorButton, "Changes the background color");
            ToolTip1.SetToolTip(this.btnBullet, "To toggle the bullet style");
            ToolTip1.SetToolTip(this.btnLineNumber, "To toggle the line number style");
            ToolTip1.SetToolTip(this.btnIncreaseIndent, "To increase the indentation");
            ToolTip1.SetToolTip(this.btnDecreaseIndent, "To decrease the indentation");
            ToolTip1.SetToolTip(this.btnAlignLeft, "To align the text to the left");
            ToolTip1.SetToolTip(this.btnAlignCenter, "To align the text to the center");
            ToolTip1.SetToolTip(this.btnAlignRight, "To align the text to the right");
            ToolTip1.SetToolTip(this.btSaveAs, "Save the text in the relevant format in the target directory");
            //ToolTip1.SetToolTip(this.btPrint, "Print the text");

            // Set the text of the button to the cursive "i" symbol
            string italicIcon = "\uD835\uDC56";
            BtItalic.Text = italicIcon;

            // Check if the file exists
            if (!File.Exists("NotepadMessage.xml"))
            {
                // If the file does not exist, create a new one
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", null, null);
                doc.AppendChild(dec);

                // Create the main element
                XmlElement mainElem = doc.CreateElement("Notes");
                doc.AppendChild(mainElem);

                // Save the blank document
                doc.Save("NotepadMessage.xml");
            }

            // Load the existing notes when starting the application
            doc.Load("NotepadMessage.xml");
            int maxId = 0;
            foreach (XmlElement noteElement in doc.GetElementsByTagName("Note"))
            {
                cBTextListing.Items.Add(noteElement.GetAttribute("id"));
                int id = int.Parse(noteElement.GetAttribute("id"));
                if (id > maxId)
                    maxId = id;
            }
            noteId.Text = (maxId + 1).ToString();

            // Load the RTF text for the selected note
            if (cBTextListing.Items.Count > 0)
            {
                cBTextListing.SelectedIndex = 0;
                string selectedId = cBTextListing.SelectedItem.ToString();
                foreach (XmlElement noteElement in doc.GetElementsByTagName("Note"))
                {
                    if (noteElement.GetAttribute("id") == selectedId)
                    {
                        richTextBoxNotPad.Rtf = noteElement.GetAttribute("rtfText");
                        break;
                    }
                }
            }

            listBoxLineNumbers.SelectionMode = System.Windows.Forms.SelectionMode.One;

        }

        #region Bold
        private void BtBold_Click(object sender, EventArgs e)
        {
            Font currentFont = richTextBoxNotPad.SelectionFont;
            FontStyle newFontStyle;

            if (richTextBoxNotPad.SelectionFont.Bold)
            {
                newFontStyle = FontStyle.Regular;
                btBold.BackColor = SystemColors.Control; // Default background color
            }
            else
            {
                newFontStyle = FontStyle.Bold;
                btBold.BackColor = Color.LightBlue; // Color background color
            }

            richTextBoxNotPad.SelectionFont = new Font(
               currentFont.FontFamily,
               currentFont.Size,
               newFontStyle
           );

            isBoldActive = !isBoldActive; // Toggle the state
        }
        #endregion

        #region Font Size
        private void CbFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            float newSize;

            if (float.TryParse(cbFontSize.SelectedItem.ToString(), out newSize))
            {
                Font currentFont = richTextBoxNotPad.SelectionFont;
                richTextBoxNotPad.SelectionFont = new Font(
                   currentFont.FontFamily,
                   newSize,
                   currentFont.Style
               );
            }
        }
        #endregion
        #region Save
        private void btSaveText_Click(object sender, EventArgs e)
        {
            // Create a new item for the note
            XmlElement noteElement = doc.CreateElement("Note");
            noteElement.SetAttribute("id", noteId.Text);
            noteElement.SetAttribute("headline", textBoxHeadLine.Text);

            // Save the RTF text instead of the normal text
            noteElement.SetAttribute("rtfText", richTextBoxNotPad.Rtf);

            // Add the element to the XmlDocument
            doc.DocumentElement.AppendChild(noteElement);

            // Save the XmlDocument to a file
            doc.Save("NotepadMessage.xml");

            // Add the ID to the ComboBox
            cBTextListing.Items.Add(noteId.Text);

            // Increment the ID for the next note
            noteId.Text = (int.Parse(noteId.Text) + 1).ToString();
        }
        #endregion
        #region RichTextBox Events
        private void cBTextListing_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedId = cBTextListing.SelectedItem.ToString();

            foreach (XmlElement noteElement in doc.GetElementsByTagName("Note"))
            {
                if (noteElement.GetAttribute("id") == selectedId)
                {
                    textBoxHeadLine.Text = noteElement.GetAttribute("headline");
                    // Load the RTF text instead of the normal text
                    richTextBoxNotPad.Rtf = noteElement.GetAttribute("rtfText");
                    break;
                }
            }
        }
        #endregion

        #region Delete Button
        private void btDelete_Click(object sender, EventArgs e)
        {
            string selectedId = noteId.Text;
            XmlElement toDelete = null;

            // Find the item to be deleted
            foreach (XmlElement noteElement in doc.GetElementsByTagName("Note"))
            {
                if (noteElement.GetAttribute("id") == selectedId)
                {
                    toDelete = noteElement;
                    break;
                }
            }

            // If the item was found
            if (toDelete != null)
            {
                // Show confirmation dialog
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this note?", "Confirmation", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    // Delete item
                    doc.DocumentElement.RemoveChild(toDelete);

                    // Reorder IDs
                    int i = 1;
                    foreach (XmlElement noteElement in doc.GetElementsByTagName("Note"))
                    {
                        noteElement.SetAttribute("id", i.ToString());
                        i++;
                    }

                    // Save XmlDocument
                    doc.Save("NotepadMessage.xml");

                    // Update ComboBox
                    cBTextListing.Items.Clear();
                    foreach (XmlElement noteElement in doc.GetElementsByTagName("Note"))
                    {
                        cBTextListing.Items.Add(noteElement.GetAttribute("id"));
                    }
                }
            }
            // Update noteId to the ID of the last note
            noteId.Text = (cBTextListing.Items.Count > 0 ? cBTextListing.Items[cBTextListing.Items.Count - 1] : "1").ToString();
        }
        #endregion
        #region checkBox Events
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                // With the CheckBox selected, set the text to bold and change the background color
                richTextBoxNotPad.SelectionFont = new Font(richTextBoxNotPad.Font, FontStyle.Bold);
                checkBox1.BackColor = Color.LightBlue; // background color
            }
            else
            {
                // With the CheckBox selected, set the text to bold and change the background color
                richTextBoxNotPad.SelectionFont = new Font(richTextBoxNotPad.Font, FontStyle.Regular);
                checkBox1.BackColor = SystemColors.Control; // Default background color
            }
        }
        #endregion

        #region Underline
        private void BtUnderline_Click(object sender, EventArgs e)
        {
            Font currentFont = richTextBoxNotPad.SelectionFont;
            FontStyle newFontStyle;

            if (richTextBoxNotPad.SelectionFont.Underline)
            {
                newFontStyle = FontStyle.Regular;
                BtUnderline.BackColor = SystemColors.Control; // Default background color
            }
            else
            {
                newFontStyle = FontStyle.Underline;
                BtUnderline.BackColor = Color.LightBlue; // Background color
            }

            richTextBoxNotPad.SelectionFont = new Font(
               currentFont.FontFamily,
               currentFont.Size,
               newFontStyle
           );

            isUnderlineActive = !isUnderlineActive; // Toggle the state
        }
        #endregion

        #region DateTime
        private void dateTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the current date and time
            string dateTimeNow = DateTime.Now.ToString();

            // Insert the date and time at the cursor position
            richTextBoxNotPad.SelectedText = dateTimeNow;
        }
        #endregion

        #region Italic
        private void BtItalic_Click(object sender, EventArgs e)
        {

            Font currentFont = richTextBoxNotPad.SelectionFont;
            FontStyle newFontStyle;

            if (richTextBoxNotPad.SelectionFont.Italic)
            {
                newFontStyle = FontStyle.Regular;
                BtItalic.BackColor = SystemColors.Control; // Default background color
            }
            else
            {
                newFontStyle = FontStyle.Italic;
                BtItalic.BackColor = Color.LightBlue; // Background color
            }

            richTextBoxNotPad.SelectionFont = new Font(
               currentFont.FontFamily,
               currentFont.Size,
               newFontStyle
           );

            isItalicActive = !isItalicActive; // Toggle the state
        }
        #endregion
        #region Search
        private void toolStripTextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            // Get all text
            string text = richTextBoxNotPad.Text;

            // Retrieve the current search query
            string query = toolStripTextBoxSearch.Text;

            // Reset the text and remove the formatting
            richTextBoxNotPad.Text = text;

            // If the search query is not empty
            if (!string.IsNullOrEmpty(query))
            {
                // Loop through the text and find all occurrences of the search query
                int index = 0;
                while ((index = text.IndexOf(query, index)) != -1)
                {
                    // Select and highlight the text found
                    richTextBoxNotPad.Select(index, query.Length);
                    richTextBoxNotPad.SelectionBackColor = Color.Yellow; // Change this to the highlight color you want

                    // Go to next occurrence
                    index += query.Length;
                }
            }
        }
        #endregion

        #region Edit Button
        private void btEdit_Click(object sender, EventArgs e)
        {
            // Get the selected ID
            string selectedId = noteId.Text;

            // Find the item to edit
            XmlElement toEdit = null;
            foreach (XmlElement noteElement in doc.GetElementsByTagName("Note"))
            {
                if (noteElement.GetAttribute("id") == selectedId)
                {
                    toEdit = noteElement;
                    break;
                }
            }

            // If the item was found
            if (toEdit != null)
            {
                // Update the element's attributes
                toEdit.SetAttribute("headline", textBoxHeadLine.Text);
                toEdit.SetAttribute("rtfText", richTextBoxNotPad.Rtf);

                // Save XmlDocument
                doc.Save("NotepadMessage.xml");
            }
            // Update noteId to the ID of the last note
            noteId.Text = (cBTextListing.Items.Count > 0 ? cBTextListing.Items[cBTextListing.Items.Count - 1] : "1").ToString();

        }
        #endregion

        #region Font
        private void btFont_Click(object sender, EventArgs e)
        {
            // Create a new FontDialog
            FontDialog fontDialog = new FontDialog();

            // Set the current font as the selected font in the dialog
            fontDialog.Font = richTextBoxNotPad.SelectionFont;

            // Display the dialog and verify that the user clicked OK
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                // Set the selected font as the font for the selected text
                richTextBoxNotPad.SelectionFont = fontDialog.Font;
            }
        }
        #endregion

        #region Delete
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Make sure text is selected
            if (!string.IsNullOrEmpty(richTextBoxNotPad.SelectedText))
            {
                // Remove the selected text
                richTextBoxNotPad.SelectedText = string.Empty;
            }
        }
        #endregion

        #region Undo
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check whether an action can be undone
            if (richTextBoxNotPad.CanUndo)
            {
                // Undo the last action
                richTextBoxNotPad.Undo();
            }
        }
        #endregion

        #region Line counter
        private void richTextBoxNotPad_TextChanged(object sender, EventArgs e)
        {
            // Update line numbers as text changes only if checkBoxLines is checked
            if (checkBoxLines.Checked)
            {
                UpdateLineNumbers();
            }
        }

        private void richTextBoxNotPad_VScroll(object sender, EventArgs e)
        {
            // Update line numbers as user scrolls only if checkBoxLines is checked
            if (checkBoxLines.Checked)
            {
                UpdateLineNumbers();
            }
        }        
        private void UpdateLineNumbers()
        {
            // Save the currently selected line number
            int selectedIndex = listBoxLineNumbers.SelectedIndex;

            // Count the number of lines in the richTextBoxNotPad
            int lineCount = richTextBoxNotPad.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Length;

            // Check if rows have been added
            while (lineCount > listBoxLineNumbers.Items.Count)
            {
                listBoxLineNumbers.Items.Add((listBoxLineNumbers.Items.Count + 1).ToString());
            }

            // Check whether rows have been removed
            while (lineCount < listBoxLineNumbers.Items.Count)
            {
                listBoxLineNumbers.Items.RemoveAt(listBoxLineNumbers.Items.Count - 1);
            }

            // Restore the selected line number
            if (selectedIndex < listBoxLineNumbers.Items.Count)
            {
                listBoxLineNumbers.SelectedIndex = selectedIndex;
            }
        }

        #endregion

        #region CheckBox Lines
        private void checkBoxLines_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the checkbox is checked
            if (checkBoxLines.Checked)
            {
                // Update the line numbers
                UpdateLineNumbers();
            }
            else
            {
                // Delete the line numbers
                listBoxLineNumbers.Items.Clear();
            }
        }
        #endregion

        #region Status Line     

        private void richTextBoxNotPad_SelectionChanged(object sender, EventArgs e)
        {
            // Calculate the current row and column
            int index = richTextBoxNotPad.SelectionStart;
            int line = richTextBoxNotPad.GetLineFromCharIndex(index);
            int firstChar = richTextBoxNotPad.GetFirstCharIndexFromLine(line);
            int column = index - firstChar;

            // Count the number of lines in the text
            int lineCount = richTextBoxNotPad.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Length;

            // Show the row and column in the status bar
            toolStripStatusLabel1.Text = $"Line: {lineCount}, Split: {column + 1}";

            // Synchronize listBoxLineNumbers with richTextBoxNotPad
            if (listBoxLineNumbers.Items.Count > line)
            {
                listBoxLineNumbers.SelectedIndex = line;
            }
        }

        private bool IsUtf8(string text)
        {
            // Check if the text is UTF-8
            // This is a simple test and may not be 100% accurate
            var utf8 = Encoding.UTF8;
            byte[] encoded = utf8.GetBytes(text);
            string decoded = utf8.GetString(encoded);
            return text == decoded;
        }

        private void richTextBoxNotPad_TextChanged_1(object sender, EventArgs e)
        {
            // Check the text format
            bool isUtf8 = IsUtf8(richTextBoxNotPad.Text);

            // Show text format in status bar
            toolStripStatusLabel1.Text += $", Format: {(isUtf8 ? "UTF-8" : "Not-UTF-8")}";
        }
        #endregion

        #region Load
        private void btLoad_Click(object sender, EventArgs e)
        {
            // Create a new OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set filters for the file types the user can open
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            // Display the dialog and verify that the user clicked OK
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Read the contents of the selected file
                string fileContent = File.ReadAllText(openFileDialog.FileName);

                // Display the contents of the file in the RichTextBox
                richTextBoxNotPad.Text = fileContent;
            }
        }
        #endregion

        #region ListBoxLineNumbers
        private void listBoxLineNumbers_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check if a row is selected
            if (listBoxLineNumbers.SelectedIndex != -1)
            {
                // Calculate the index of the first character of the selected line
                int line = listBoxLineNumbers.SelectedIndex;
                int index = richTextBoxNotPad.GetFirstCharIndexFromLine(line);

                // Move the cursor to the beginning of the selected line
                richTextBoxNotPad.SelectionStart = index;
                richTextBoxNotPad.SelectionLength = 0;

                // Set focus on the RichTextBox so the user can type immediately
                richTextBoxNotPad.Focus();
            }
        }
        #endregion

        #region Date Time Picker
        private void dateTimePicker_CloseUp(object sender, EventArgs e)
        {
            // Format the selected date as a string
            string dateString = dateTimePicker1.Value.ToString("dd.MM.yyyy");

            // Insert the date into the RichTextBox at the current cursor position
            richTextBoxNotPad.SelectedText = dateString;
        }
        #endregion

        #region Replace search        
        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if the form is already open
            if (replaceForm != null)
            {
                // The form is already open, so do nothing
                return;
            }
            // Create a new form
            replaceForm = new System.Windows.Forms.Form();
            replaceForm.TopMost = true;
            replaceForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            replaceForm.MaximizeBox = false;


            // Create the controls
            System.Windows.Forms.Label lblSearch = new System.Windows.Forms.Label() { Text = "Search for:", Location = new System.Drawing.Point(10, 10) };
            System.Windows.Forms.TextBox txtSearch = new System.Windows.Forms.TextBox() { Location = new System.Drawing.Point(10, 30) };

            System.Windows.Forms.Label lblReplace = new System.Windows.Forms.Label() { Text = "Replace by:", Location = new System.Drawing.Point(10, 60) };
            System.Windows.Forms.TextBox txtReplace = new System.Windows.Forms.TextBox() { Location = new System.Drawing.Point(10, 80) };

            System.Windows.Forms.Button btnSearchNext = new System.Windows.Forms.Button() { Text = "Search", Location = new System.Drawing.Point(10, 110) };

            System.Windows.Forms.Button btnReplaceOne = new System.Windows.Forms.Button() { Text = "Replace", Location = new System.Drawing.Point(100, 110) };

            System.Windows.Forms.Button btnReplaceAll = new System.Windows.Forms.Button() { Text = "Replace all", Location = new System.Drawing.Point(190, 110) };

            System.Windows.Forms.Button btnCancel = new System.Windows.Forms.Button() { Text = "Cancel", Location = new System.Drawing.Point(190, 140) };

            System.Windows.Forms.CheckBox chkMatchCase = new System.Windows.Forms.CheckBox() { Text = "up/lower", Location = new System.Drawing.Point(10, 140) };

            System.Windows.Forms.CheckBox chkWrapAround = new System.Windows.Forms.CheckBox() { Text = "Enclose", Location = new System.Drawing.Point(10, 160) };

            // Create the text box
            System.Windows.Forms.TextBox txtCount = new System.Windows.Forms.TextBox()
            {
                Location = new System.Drawing.Point(120, 80),
                ReadOnly = true,
                MaxLength = 3,
                Width = 20 // 20 Pixel
            };

            // Adds the controls to the form
            replaceForm.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblSearch,
                txtSearch,
                lblReplace,
                txtReplace,
                btnSearchNext,
                btnReplaceOne,
                btnReplaceAll,
                btnCancel,
                chkMatchCase,
                chkWrapAround,
                txtCount
            });

            // Adds the event handler for the buttons
            btnSearchNext.Click += (s, ev) =>
            {
                if (richTextBoxNotPad.InvokeRequired)
                {
                    richTextBoxNotPad.Invoke(new Action(() =>
                    {
                        string searchText = txtSearch.Text;
                        int startIndex = richTextBoxNotPad.SelectionStart + richTextBoxNotPad.SelectionLength;
                        int index = richTextBoxNotPad.Text.IndexOf(searchText, startIndex, chkMatchCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                        if (index != -1)
                        {
                            richTextBoxNotPad.Select(index, searchText.Length);
                        }
                        else
                        {
                            // If the search text was not found, set the starting index to 0 and start the search again
                            index = richTextBoxNotPad.Text.IndexOf(searchText, 0, chkMatchCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                            if (index != -1)
                            {
                                richTextBoxNotPad.Select(index, searchText.Length);
                            }
                        }
                    }));
                }
                else
                {
                    string searchText = txtSearch.Text;
                    int startIndex = richTextBoxNotPad.SelectionStart + richTextBoxNotPad.SelectionLength;
                    int index = richTextBoxNotPad.Text.IndexOf(searchText, startIndex, chkMatchCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                    if (index != -1)
                    {
                        richTextBoxNotPad.Select(index, searchText.Length);
                    }
                    else
                    {
                        // If the search text was not found, set the starting index to 0 and start the search again
                        index = richTextBoxNotPad.Text.IndexOf(searchText, 0, chkMatchCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                        if (index != -1)
                        {
                            richTextBoxNotPad.Select(index, searchText.Length);
                        }
                    }
                }
                // Updates the textbox every time the search text or the text in the richTextBoxNotPad changes
                txtSearch.TextChanged += (s, ev) => UpdateCount();
                richTextBoxNotPad.TextChanged += (s, ev) => UpdateCount();
            };

            // A method to update the textbox
            void UpdateCount()
            {
                if (richTextBoxNotPad.InvokeRequired)
                {
                    richTextBoxNotPad.Invoke(new Action(() => txtCount.Text = CountOccurrences(richTextBoxNotPad.Text, txtSearch.Text).ToString()));
                }
                else
                {
                    txtCount.Text = CountOccurrences(richTextBoxNotPad.Text, txtSearch.Text).ToString();
                }
            }

            // A method for counting the occurrences of a word in a text
            int CountOccurrences(string text, string word)
            {
                int count = 0;
                int startIndex = 0;
                while ((startIndex = text.IndexOf(word, startIndex)) != -1)
                {
                    count++;
                    startIndex += word.Length;
                }
                return count;
            }

            btnReplaceOne.Click += (s, ev) =>
            {
                if (richTextBoxNotPad.InvokeRequired)
                {
                    richTextBoxNotPad.Invoke(new Action(() =>
                    {
                        if (richTextBoxNotPad.SelectionLength > 0)
                        {
                            richTextBoxNotPad.SelectedText = txtReplace.Text;
                        }
                    }));
                }
                else
                {
                    if (richTextBoxNotPad.SelectionLength > 0)
                    {
                        richTextBoxNotPad.SelectedText = txtReplace.Text;
                    }
                }
            };

            btnReplaceAll.Click += (s, ev) =>
            {
                string searchText = txtSearch.Text;
                string replaceText = txtReplace.Text;
                if (richTextBoxNotPad.InvokeRequired)
                {
                    richTextBoxNotPad.Invoke(new Action(() => richTextBoxNotPad.Text = richTextBoxNotPad.Text.Replace(searchText, replaceText)));
                }
                else
                {
                    richTextBoxNotPad.Text = richTextBoxNotPad.Text.Replace(searchText, replaceText);
                }
            };

            btnCancel.Click += (s, ev) =>
            {
                replaceForm.Close();
            };

            // Adds an event handler to set the global variable to null when the form closes
            replaceForm.FormClosed += (s, ev) => { replaceForm = null; };
           
            replaceForm.Show();
        }

        #endregion

        #region Select All
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Select all text in the RichTextBox
            richTextBoxNotPad.SelectAll();
        }
        #endregion

        #region Align Left
        private void btnAlignLeft_Click(object sender, EventArgs e)
        {
            richTextBoxNotPad.SelectionAlignment = HorizontalAlignment.Left;
        }
        #endregion
        #region Align Center
        private void btnAlignCenter_Click(object sender, EventArgs e)
        {
            richTextBoxNotPad.SelectionAlignment = HorizontalAlignment.Center;
        }
        #endregion
        #region Align Right      
        private void btnAlignRight_Click(object sender, EventArgs e)
        {
            richTextBoxNotPad.SelectionAlignment = HorizontalAlignment.Right;
        }
        #endregion
        #region Bullet List
        private void btnBullet_Click(object sender, EventArgs e)
        {
            isBulletActive = !isBulletActive;
            btnBullet.BackColor = isBulletActive ? Color.LightBlue : SystemColors.Control;

            if (isBulletActive)
            {
                // Insert a bullet point before each line of selected text
                string[] lines = richTextBoxNotPad.SelectedText.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = "\u2022 " + lines[i];
                }
                richTextBoxNotPad.SelectedText = string.Join("\n", lines);
            }
        }
        #endregion

        #region Keypress
        private void richTextBoxNotPad_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (isBulletActive && e.KeyChar == (char)Keys.Return)
            {
                // Insert a bullet point at the beginning of the new line
                richTextBoxNotPad.AppendText("\u2022 ");

                // Place the cursor after the bullet
                richTextBoxNotPad.SelectionStart = richTextBoxNotPad.Text.Length;

                // Prevent the default behavior of the Return key
                e.Handled = true;
            }
            else if (isLineNumberActive && e.KeyChar == (char)Keys.Return)
            {
                // Insert a line number at the beginning of the new line
                richTextBoxNotPad.AppendText($"{lineNumber++} ");

                // Place the cursor after the line number
                richTextBoxNotPad.SelectionStart = richTextBoxNotPad.Text.Length;

                // Prevent the default behavior of the Return key
                e.Handled = true;
            }

        }
        #endregion

        #region Line Numbers
        int lineNumber = 1;
        private void btnLineNumber_Click(object sender, EventArgs e)
        {
            btnLineNumber.Click += (s, ev) =>
            {
                // Toggle the state of line numbering
                isLineNumberActive = !isLineNumberActive;

                // Change the color of the button according to the status
                btnLineNumber.BackColor = isLineNumberActive ? Color.LightBlue : SystemColors.Control;

                if (isLineNumberActive)
                {
                    // Divide the selected text into lines
                    string[] lines = richTextBoxNotPad.SelectedText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                    // Add a line number before each line
                    for (int i = 0; i < lines.Length; i++)
                    {
                        lines[i] = (lineNumber++).ToString() + " " + lines[i];
                    }

                    // Replace the highlighted text with the numbered text
                    richTextBoxNotPad.SelectedText = string.Join("\r\n", lines);
                }
                else
                {
                    // Reset the line number if line numbering is disabled
                    lineNumber = 1;
                }
            };
        }
        #endregion

        #region Increase Indent and decrease Indent

        private void btnDecreaseIndent_Click(object sender, EventArgs e)
        {
            // Increase the indent of the current line or the selected text
            richTextBoxNotPad.SelectionIndent += 20;
        }

        private void btnIncreaseIndent_Click(object sender, EventArgs e)
        {
            // Decrease the indent of the current line or the selected text
            richTextBoxNotPad.SelectionIndent = Math.Max(0, richTextBoxNotPad.SelectionIndent - 20);
        }
        #endregion

        #region Color Background
        private void colorButton_Click(object sender, EventArgs e)
        {
            // Create a new ColorDialog
            ColorDialog colorDialog = new ColorDialog();

            // Display the dialog and verify that the user clicked OK
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                // Set the selected color as the background color for the selected text
                richTextBoxNotPad.SelectionBackColor = colorDialog.Color;
            }
        }
        #endregion

        #region Button Font Color
        private void btFontColor_Click(object sender, EventArgs e)
        {
            // Create a new ColorDialog
            ColorDialog colorDialog = new ColorDialog();

            // Display the dialog and verify that the user clicked OK
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                // Set the selected color as the color for the selected text
                richTextBoxNotPad.SelectionColor = colorDialog.Color;
            }
        }
        #endregion

        #region Button Save
        private void btSaveAs_Click(object sender, EventArgs e)
        {
            // Create a new SaveFileDialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // Set filters for the file types the user can save
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|Rich Text Format (*.rtf)|*.rtf|C#-files (*.cs)|*.cs|XML-files (*.xml)|*.xml|SCP-files (*.scp)|*.scp|INI-files (*.ini)|*.ini|All files (*.*)|*.*";

            // Display the dialog and check if the user clicked OK
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Save the contents of the RichTextBox to the selected file
                if (saveFileDialog.FilterIndex == 2) // RTF-Format
                {
                    richTextBoxNotPad.SaveFile(saveFileDialog.FileName, RichTextBoxStreamType.RichText);
                }
                else // All other formats
                {
                    richTextBoxNotPad.SaveFile(saveFileDialog.FileName, RichTextBoxStreamType.PlainText);
                }
            }
        }
        #endregion

        #region Load StripMenuItem
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Create a new OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set filters for the file types the user can open
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            // Display the dialog and verify that the user clicked OK
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Read the contents of the selected file
                string fileContent = File.ReadAllText(openFileDialog.FileName);

                // Display the contents of the file in the RichTextBox
                richTextBoxNotPad.Text = fileContent;
            }
        }
        #endregion

        #region Save ToolStripMenuItem
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Create a new SaveFileDialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // Set filters for the file types the user can save
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|Rich Text Format (*.rtf)|*.rtf|C#-files (*.cs)|*.cs|XML-files (*.xml)|*.xml|SCP-files (*.scp)|*.scp|INI-files (*.ini)|*.ini|All files (*.*)|*.*";

            // Display the dialog and verify that the user clicked OK
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Save the contents of the RichTextBox to the selected file
                if (saveFileDialog.FilterIndex == 2) // RTF-Format
                {
                    richTextBoxNotPad.SaveFile(saveFileDialog.FileName, RichTextBoxStreamType.RichText);
                }
                else // All other formats
                {
                    richTextBoxNotPad.SaveFile(saveFileDialog.FileName, RichTextBoxStreamType.PlainText);
                }
            }
        }
        #endregion

        #region Print
        private void PrintToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Create a new instance of PrintDocument
            PrintDocument printDocument = new PrintDocument();

            // Add the event handler for the PrintPage event
            printDocument.PrintPage += new PrintPageEventHandler(PrintDocument_PrintPage);

            // Create a new PrintDialog
            System.Windows.Forms.PrintDialog printDialog = new System.Windows.Forms.PrintDialog();

            // Set the PrintDocument for the dialog
            printDialog.Document = printDocument;

            // Display the dialog and verify that the user clicked OK
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                // Print the document
                printDialog.Document.Print();
            }
        }

        // Event handler method for the PrintPage event
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            // Draw the RichTextBox's text onto the page
            e.Graphics.DrawString(richTextBoxNotPad.Text, richTextBoxNotPad.Font, Brushes.Black, 10, 10);
        }
        #endregion

        #region Copy Clipboard
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Copy all the text in the RichTextBox to the clipboard
            richTextBoxNotPad.SelectAll();
            richTextBoxNotPad.Copy();
        }
        #endregion

        #region Mouse Click
        private void listBoxLineNumbers_MouseClick(object sender, MouseEventArgs e)
        {
            // Check whether a line number has been selected
            if (listBoxLineNumbers.SelectedIndex != -1)
            {
                // Get the selected line number
                int lineNumber = listBoxLineNumbers.SelectedIndex;

                // Jump to the corresponding line in richTextBoxNotPad
                richTextBoxNotPad.SelectionStart = richTextBoxNotPad.GetFirstCharIndexFromLine(lineNumber);
                richTextBoxNotPad.ScrollToCaret();

                // Set the focus to richTextBoxNotPad
                richTextBoxNotPad.Focus();
            }
        }
        #endregion
    }
}

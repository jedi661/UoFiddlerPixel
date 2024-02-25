// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  * 
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
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
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;

namespace UoFiddler.Controls.Forms
{
    public partial class EditUoBodyconvMobtypes : Form
    {
        private int searchStartIndex = 0;
        private string currentFilePath;  // Saves the path of the last loaded file
        public EditUoBodyconvMobtypes()
        {
            InitializeComponent();

            textBoxPfad.Text = Properties.Settings.Default.LastPath; // Save Last Path
        }

        #region btLoadBodyconv_Click
        private void btLoadBodyconv_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfad.Text, "Bodyconv.def");
            if (File.Exists(path))
            {
                richTextBoxEdit.Text = File.ReadAllText(path);
                currentFilePath = path;  // Updates the path of the last loaded file
                lbFileName.Text = Path.GetFileName(path);  // Displays the file name in lbFileName
            }
            else
            {
                MessageBox.Show("The Bodyconv.def file could not be found.");
            }
        }
        #endregion

        #region btLoadPfad_Click
        private void btLoadPfad_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxPfad.Text = folderBrowserDialog.SelectedPath;
                // Save the selected path in Settings
                Properties.Settings.Default.LastPath = folderBrowserDialog.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }
        #endregion

        #region btmobtypes_Click
        private void btmobtypes_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfad.Text, "mobtypes.txt");
            if (File.Exists(path))
            {
                richTextBoxEdit.Text = File.ReadAllText(path);
                currentFilePath = path;  // Updates the path of the last loaded file
                lbFileName.Text = Path.GetFileName(path);  // Displays the file name in lbFileName
            }
            else
            {
                MessageBox.Show("The file mobtypes.txt could not be found.");
            }
        }
        #endregion

        #region searchToolStripMenuItem
        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string searchText = toolStripTextBoxSearch.Text;
            if (!string.IsNullOrEmpty(searchText))
            {
                // Counts the number of matches and displays them in lbSearchCount
                int count = Regex.Matches(richTextBoxEdit.Text, searchText).Count;
                lbSearchCount.Text = $"Number of matches: {count}";

                int index = richTextBoxEdit.Find(searchText, searchStartIndex, RichTextBoxFinds.None);
                if (index != -1)
                {
                    richTextBoxEdit.Select(index, searchText.Length);
                    richTextBoxEdit.ScrollToCaret();  // Scrolls to the cursor position
                    richTextBoxEdit.Focus();  // Sets the focus on the RichTextBox
                    searchStartIndex = index + searchText.Length;
                }
                else
                {
                    MessageBox.Show("No further matches found.");
                    searchStartIndex = 0;
                }
            }
        }
        #endregion

        #region btSaveFile_Click
        private void btSaveFile_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                File.WriteAllText(currentFilePath, richTextBoxEdit.Text);
            }
            else
            {
                MessageBox.Show("No file was selected to save.");
            }
        }
        #endregion

        #region btBackwardText_Click
        private void btBackwardText_Click(object sender, EventArgs e)
        {
            if (richTextBoxEdit.CanUndo)
            {
                richTextBoxEdit.Undo();
            }
            else
            {
                MessageBox.Show("No further reversals possible.");
            }
        }
        #endregion

        #region string TextBoxID
        public string TextBoxID
        {
            set { textBoxID.Text = value; }
        }
        #endregion

        #region string TextBoxBody
        public string TextBoxBody
        {
            set { textBoxBody.Text = value; }
        }
        #endregion
    }
}

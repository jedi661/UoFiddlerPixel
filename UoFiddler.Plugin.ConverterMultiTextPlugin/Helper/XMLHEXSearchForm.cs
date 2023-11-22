// /***************************************************************************
//  *
//  * $Author: Nikodemus 
//  * 
//  * "THE WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a Wine in return.
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
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Helper
{
    public partial class XMLHEXSearchForm : Form
    {
        public XMLHEXSearchForm()
        {
            InitializeComponent();
        }

        #region Load
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.richTextBox.Text = File.ReadAllText(openFileDialog.FileName);
                    this.toolStripLabelName.Text = "Name: " + Path.GetFileName(openFileDialog.FileName); // Display the file name in the toolStripLabelName
                }
            }
        }
        #endregion

        #region Hex Search
        private void btHexSearch_Click(object sender, EventArgs e)
        {
            string text = richTextBox.Text;
            string pattern = @"TileID=\""(?!0x)[a-fA-F]{2,}\"""; // Regex pattern to match hexadecimal values not starting with 0x and containing at least one letter

            int errorCount = 0; // Counter for incorrect hex addresses

            foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(text, pattern))
            {
                int startIndex = match.Index;
                int length = match.Length;

                // Highlight the incorrect hex value in the RichTextBox
                richTextBox.Select(startIndex, length);
                richTextBox.SelectionBackColor = Color.Yellow; // Change this to the color you want

                errorCount++; // Increment the counter for each incorrect hex address found
            }

            richTextBox.DeselectAll(); // Deselect all text

            // Display the number of incorrect hex addresses in the toolStripLabelHexError
            this.toolStripLabelHexError.Text = "Incorrect hex addresses: " + errorCount + " recognized";
        }
        #endregion

        #region Correvt
        private void btCorrectHex_Click(object sender, EventArgs e)
        {
            string text = richTextBox.Text;
            string pattern = @"TileID=\""(?!0x)[a-fA-F]{2,}\"""; // Regex pattern to match hexadecimal values not starting with 0x and containing at least one letter

            // Replace incorrect hex values with corrected ones
            string correctedText = System.Text.RegularExpressions.Regex.Replace(text, pattern, m => m.Value.Insert(8, "0x"));

            richTextBox.Text = correctedText; // Update the RichTextBox with the corrected text
        }
        #endregion

        #region Save
        private void saveXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "XML Files (*.xml)|*.xml";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, richTextBox.Text);
                }
            }
        }
        #endregion

        #region Clipboard
        private void btXMLCliboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBox.Text);
        }
        #endregion
    }
}

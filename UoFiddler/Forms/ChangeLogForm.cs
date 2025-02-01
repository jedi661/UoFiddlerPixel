// /***************************************************************************
//  *
//  * $Author: Turley
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
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UoFiddler.Forms
{
    public partial class ChangeLogForm : Form
    {
        #region [ ChangeLogForm ]
        public ChangeLogForm()
        {
            InitializeComponent();

            // Add an event handler for the KeyDown event of the ToolStripTextBox
            toolStripTextBox1.KeyDown += (s, args) =>
            {
                if (args.KeyCode == Keys.Enter)
                {
                    args.SuppressKeyPress = true; // Prevents the "ding" sound when pressing Enter

                    string searchText = toolStripTextBox1.Text;
                    if (string.IsNullOrWhiteSpace(searchText)) return; // Prevents empty searches

                    // Remove previous search highlighting
                    ResetHighlighting();

                    string richText = richTextBox1.Text;
                    int index = 0;

                    // Search the text for all occurrences of the search term (case-insensitive)
                    while ((index = richText.IndexOf(searchText, index, StringComparison.OrdinalIgnoreCase)) != -1)
                    {
                        // Highlight the current occurrence of the search term
                        richTextBox1.Select(index, searchText.Length);
                        richTextBox1.SelectionBackColor = Color.Yellow;

                        // Move the index forward to continue searching
                        index += searchText.Length;
                    }

                    // If the search term is found, set focus on the RichTextBox
                    if (richText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        richTextBox1.Focus();
                    }
                }
            };
        }
        #endregion

        #region [ Reset Highlighting ]
        private void ResetHighlighting()
        {
            richTextBox1.SelectAll();
            richTextBox1.SelectionBackColor = richTextBox1.BackColor; // Resets the background color
        }
        #endregion

        #region [ ChangeLogForm_Load ]
        private void ChangeLogForm_Load(object sender, EventArgs e)
        {
            // Read the contents of the "Changelog.txt" file into a string
            string changelogText = File.ReadAllText("Changelog.txt", Encoding.UTF8);

            // Split the text into an array of lines
            string[] lines = changelogText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            // Loop through each line
            for (int i = 0; i < lines.Length; i++)
            {
                // Check if the line starts with a "-"
                if (lines[i].StartsWith("-"))
                {
                    // Replace the "-" with a "•"
                    lines[i] = "•" + lines[i].Substring(1);
                }
            }

            // Join the lines back into a single string
            changelogText = string.Join(Environment.NewLine, lines);

            // Replace some specific strings with other strings
            changelogText = changelogText.Replace("#XG1", "Nikodemus");
            changelogText = changelogText.Replace("#XG2", "AsYlum");
            changelogText = changelogText.Replace("#XG3", "Alathair");

            // Convert the text to RTF format
            string changelogRtf = ConvertToRtf(changelogText);
            // Set the RTF property of the richTextBox1 to the RTF text
            richTextBox1.Rtf = changelogRtf;
        }
        #endregion

        /***********************************************************************
        * The ConvertToRtf method takes a plain text string as input and       *
        * returns an RTF-formatted string. The method uses a StringBuilder to  *
        * build the RTF text. It starts by appending the RTF header and color  *
        * table information to the StringBuilder. The color table defines the  *
        * colors for the #XC1 to #XC7 tags.                                    *
        *                                                                      *
        * The color abbreviations for #XC1 to #XC7 are:                         *
        * #XC1 = Orange                                                         *
        * #XC2 = Red                                                            *
        * #XC3 = Blue                                                           *
        * #XC4 = Yellow                                                         *
        * #XC5 = Green                                                          *
        * #XC6 = Purple                                                         *
        * #XC7 = Pink                                                           *
        *                                                                      *
        * The method then splits the input text into an array of lines and     *
        * loops through each line. If a line starts with "Version :", it       *
        * appends the RTF code to make the text bold, appends the line, and    *
        * then appends the RTF code to end the bold text. Otherwise, it splits *
        * the line into an array of words and loops through each word. If a    *
        * word starts with "#XC", it extracts the number after "#XC" and       *
        * checks if it is between 1 and 7. If it is, it appends the RTF code   *
        * to set the text color to the corresponding color in the color table, *
        * appends the next word, and then appends the RTF code to reset the    *
        * text color to the default color. It also appends the RTF code to     *
        * make the colored word bold.                                          *
        *                                                                      *
        * After processing all lines, the method appends the RTF footer and    *
        * returns the RTF text as a string.                                    *
        ***********************************************************************/

        #region [ ConvertToRtf ]
        private string ConvertToRtf(string text)
        {
            // Create a new StringBuilder to build the RTF text
            var rtf = new StringBuilder();
            // Append the RTF header and color table
            rtf.Append(@"{\rtf1\ansi\deff0{\colortbl;\red255\green165\blue0;\red255\green0\blue0;\red0\green0\blue255;\red255\green255\blue0;\red0\green128\blue0;\red128\green0\blue128;\red255\green192\blue203;\red0\green255\blue255;\red255\green105\blue180;}");
            // Split the input text into an array of lines
            string[] lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            // Loop through each line
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                // Check if the line starts with "Version :"
                if (line.StartsWith("Version :"))
                {
                    // Append the RTF code to make the text bold
                    rtf.Append(@"\b ");
                    // Append the line
                    rtf.Append(line);
                    // Append the RTF code to end the bold text
                    rtf.Append(@"\b0 ");
                }
                else
                {
                    string[] words = line.Split(' ');
                    for (int j = 0; j < words.Length; j++)
                    {
                        string word = words[j];
                        if (word.StartsWith("#XC"))
                        {
                            int colorIndex = 0;
                            if (int.TryParse(word.Substring(3), out colorIndex) && colorIndex >= 1 && colorIndex <= 7)
                            {
                                rtf.Append(@"\cf" + colorIndex + " ");
                                rtf.Append(@"\b ");
                                if (words.Length > j + 1)
                                {
                                    rtf.Append(words[j + 1]);
                                    rtf.Append(@"\b0 ");
                                    rtf.Append(@"\cf0 ");
                                    j++;
                                }
                            }
                            else
                            {
                                rtf.Append(word);
                            }
                        }
                        else
                        {
                            rtf.Append(word);
                        }
                        if (j < words.Length - 1)
                        {
                            rtf.Append(" ");
                        }
                    }
                }
                // If this is not the last line
                if (i < lines.Length - 1)
                {
                    // Append the RTF code for a new line
                    rtf.Append(@"\line ");
                }
            }
            // Append the RTF footer
            rtf.Append("}");
            // Return the RTF text as a string
            return rtf.ToString();
        }
        #endregion

        #region [ searchToolStripMenuItem_Click ]
        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Set focus on the ToolStripTextBox.
            toolStripTextBox1.Focus();
        }
        #endregion

        #region [ richTextBox1_MouseDown ]
        private void richTextBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // Check if the right mouse button was pressed.
            if (e.Button == MouseButtons.Right)
            {
                // Show the context menu at the current mouse position.
                contextMenuStrip1.Show(richTextBox1, e.Location);
            }
        }
        #endregion
    }
}

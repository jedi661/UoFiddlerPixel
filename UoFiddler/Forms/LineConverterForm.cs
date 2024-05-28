// /***************************************************************************
//  *
//  * $Author: Nikodemus
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
using System.Windows.Forms;

namespace UoFiddler.Forms
{
    public partial class LineConverterForm : Form
    {
        private string _originalText;

        public LineConverterForm()
        {
            InitializeComponent();
        }

        #region BtnConvert
        private void BtnConvert_Click(object sender, EventArgs e)
        {
            _originalText = TextBoxInputOutput.Text;
            if (chkAddSpaces.Checked)
            {
                TextBoxInputOutput.Text = ConvertParagraphsToLinesWithSpaces(TextBoxInputOutput.Text);
            }
            else
            {
                TextBoxInputOutput.Text = ConvertParagraphsToLines(TextBoxInputOutput.Text);
            }
            lbCounter.Text = TextBoxInputOutput.Text.Length.ToString();
        }
        #endregion

        #region BtnConvert2
        private void BtnConvert2_Click(object sender, EventArgs e)
        {
            _originalText = TextBoxInputOutput.Text;
            TextBoxInputOutput.Text = ConvertParagraphsToLines2(TextBoxInputOutput.Text);
            lbCounter.Text = TextBoxInputOutput.Text.Length.ToString();
        }
        #endregion

        private void BtnConvertParagraphsToLines2WithoutComments_Click(object sender, EventArgs e)
        {
            _originalText = TextBoxInputOutput.Text;
            TextBoxInputOutput.Text = ConvertParagraphsToLines2WithoutComments(TextBoxInputOutput.Text);
            lbCounter.Text = TextBoxInputOutput.Text.Length.ToString();
        }

        #region ConvertParagraphsToLines
        private static string ConvertParagraphsToLines(string input)
        {
            var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var result = new StringBuilder();
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("//"))
                {
                    result.Append(trimmedLine + " ");
                }
                else if (trimmedLine.EndsWith(";") || trimmedLine.EndsWith("{") || trimmedLine.EndsWith("}"))
                {
                    result.Append(trimmedLine + " ");
                }
                else
                {
                    result.Append(trimmedLine);
                }
            }
            return result.ToString();
        }
        #endregion

        #region ConvertParagraphsToLines2
        private static string ConvertParagraphsToLines2(string input)
        {
            var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var result = new StringBuilder();
            foreach (var line in lines)
            {
                var words = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    result.Append(word);
                    if (word.EndsWith(";") || word.EndsWith("{") || word.EndsWith("}"))
                    {
                        result.Append(" ");
                    }
                }
            }
            return result.ToString();
        }
        #endregion

        #region ConvertParagraphsToLinesWithSpaces
        private static string ConvertParagraphsToLinesWithSpaces(string input)
        {
            var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var result = new StringBuilder();
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.EndsWith(";") || trimmedLine.EndsWith("{") || trimmedLine.EndsWith("}"))
                {
                    result.Append(trimmedLine + " ");
                }
                else
                {
                    result.Append(trimmedLine);
                }
            }
            return result.ToString();
        }
        #endregion

        #region ConvertParagraphsToLines2WithoutComments
        private static string ConvertParagraphsToLines2WithoutComments(string input)
        {
            var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var result = new StringBuilder();
            bool isCommentBlock = false;

            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("///***************************************************************************") || line.Trim().StartsWith("// /***************************************************************************"))
                {
                    isCommentBlock = true;
                }

                if (!isCommentBlock)
                {
                    var words = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in words)
                    {
                        result.Append(word);
                        if (word.EndsWith(";") || word.EndsWith("{") || word.EndsWith("}"))
                        {
                            result.Append(" ");
                        }
                    }
                }

                if (line.Trim().EndsWith("***************************************************************************/"))
                {
                    isCommentBlock = false;
                }
            }

            return result.ToString();
        }
        #endregion

        #region BtnConvertWithBlocks
        private void BtnConvertWithBlocks_Click(object sender, EventArgs e)
        {
            _originalText = TextBoxInputOutput.Text;
            string convertedText = ConvertParagraphsToLines2WithoutComments(TextBoxInputOutput.Text);

            // Verify that the input to TBBlockCount is a valid number and greater than 500
            if (!int.TryParse(TBBlockCount.Text, out int blockSize) || blockSize < 500)
            {
                // If TBBlockCount does not contain a valid block size, use the checkboxes
                blockSize = chkBlockSize4000.Checked ? 4000 : 8000;
            }

            StringBuilder sb = new StringBuilder();
            int blockCount = 0;
            for (int i = 0; i < convertedText.Length; i += blockSize)
            {
                int length = Math.Min(blockSize, convertedText.Length - i);
                sb.AppendLine(convertedText.Substring(i, length));
                sb.AppendLine();
                blockCount++;
            }

            TextBoxInputOutput.Text = sb.ToString();
            lbCounter.Text = TextBoxInputOutput.Text.Length.ToString();
            lblBlockCount.Text = "Block Count: " + blockCount.ToString();
        }
        #endregion

        #region BtnClear
        private void BtnClear_Click(object sender, EventArgs e)
        {
            // Clears the contents of the TextBox
            TextBoxInputOutput.Clear();
            lbCounter.Text = "0";
        }
        #endregion

        #region BtnCopy
        private void BtnCopy_Click(object sender, EventArgs e)
        {
            // Copies the contents of the TextBox to the clipboard
            Clipboard.SetText(TextBoxInputOutput.Text);
        }
        #endregion

        #region BtnRestore
        private void BtnRestore_Click(object sender, EventArgs e)
        {
            // Restores the original text
            TextBoxInputOutput.Text = _originalText;
            lbCounter.Text = _originalText.Length.ToString();
        }
        #endregion

        #region ToolStripTextBoxSearch
        private void ToolStripTextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            // Get the searched text from the ToolStripTextBoxSearch
            string searchText = ToolStripTextBoxSearch.Text;

            // If the search text is not empty, search and navigate
            if (!string.IsNullOrEmpty(searchText))
            {
                int startIndex = TextBoxInputOutput.Text.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase);
                if (startIndex != -1)
                {
                    TextBoxInputOutput.Select(startIndex, searchText.Length);
                    TextBoxInputOutput.ScrollToCaret();
                }
            }
        }
        #endregion

        #region BtnBullBlockSize
        private void BtnBullBlockSize_Click(object sender, EventArgs e)
        {
            int blockSize;
            if (int.TryParse(TBBlockCount.Text, out int inputBlockSize) && inputBlockSize >= 500)
            {
                blockSize = inputBlockSize; // Sets the block size to the value in TBBlockCount if it is valid and greater than or equal to 500
            }
            else
            {
                blockSize = chkBlockSize4000.Checked ? 4000 : 8000; // Sets the block size to 4000 if the checkbox is activated, otherwise to 8000
            }

            if (TextBoxInputOutput.Text.Length > 0)
            {
                int currentBlockSize = Math.Min(blockSize, TextBoxInputOutput.Text.Length); // Takes the smaller characters from 'blockSize' or the remaining length of the text

                string textToTransfer = TextBoxInputOutput.Text.Substring(0, currentBlockSize); // Takes the first 'currentBlockSize' characters from the TextBox

                Clipboard.SetText(textToTransfer); // Copies the text to the clipboard

                TextBoxInputOutput.Text = TextBoxInputOutput.Text.Remove(0, currentBlockSize); // Removes the first 'currentBlockSize' characters from the TextBox

                System.Threading.Thread.Sleep(5000); // Wait 5 seconds
            }
        }
        #endregion
    }
}
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
            int blockSize = chkBlockSize4000.Checked ? 4000 : 8000;

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
            // Stellt den ursprünglichen Text wieder her
            TextBoxInputOutput.Text = _originalText;
            lbCounter.Text = _originalText.Length.ToString();
        }
        #endregion
    }
}
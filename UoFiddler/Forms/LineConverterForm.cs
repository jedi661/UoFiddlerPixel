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
using System.Windows.Forms;

namespace UoFiddler.Forms
{
    public partial class LineConverterForm : Form
    {
        private string originalText;

        public LineConverterForm()
        {
            InitializeComponent();
        }

        #region BtnConvert
        private void BtnConvert_Click(object sender, EventArgs e)
        {
            originalText = TextBoxInputOutput.Text;
            TextBoxInputOutput.Text = ConvertParagraphsToLines(TextBoxInputOutput.Text);
            lbCounter.Text = TextBoxInputOutput.Text.Length.ToString();
        }
        #endregion

        #region BtnConvert2
        private void BtnConvert2_Click(object sender, EventArgs e)
        {
            originalText = TextBoxInputOutput.Text;
            TextBoxInputOutput.Text = ConvertParagraphsToLines2(TextBoxInputOutput.Text);
            lbCounter.Text = TextBoxInputOutput.Text.Length.ToString();
        }
        #endregion

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
            TextBoxInputOutput.Text = originalText;
            lbCounter.Text = originalText.Length.ToString();
        }
        #endregion
    }
}
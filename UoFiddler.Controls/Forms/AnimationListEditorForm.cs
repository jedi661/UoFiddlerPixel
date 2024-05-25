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
using System.IO;

namespace UoFiddler.Controls.Forms
{
    public partial class AnimationListEditorForm : Form
    {
        private string fileName;

        public AnimationListEditorForm(string fileName)
        {
            InitializeComponent();

            this.fileName = fileName;

            LoadXml(); // Load the XML file
        }

        #region LoadXml
        private void LoadXml()
        {
            if (string.IsNullOrEmpty(fileName))
            {
                MessageBox.Show("The file name is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(fileName))
            {
                MessageBox.Show($"The file {fileName} does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            richTextBox.Text = File.ReadAllText(fileName);
        }
        #endregion

        #region SaveButton
        private void SaveButton_Click(object sender, EventArgs e)
        {
            File.WriteAllText(fileName, richTextBox.Text);
            MessageBox.Show("The XML file has been saved.", "saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region SearchButton
        private void SearchButton_Click(object sender, EventArgs e)
        {
            int index = richTextBox.Text.IndexOf(searchTextBox.Text, StringComparison.CurrentCultureIgnoreCase);
            if (index != -1)
            {
                richTextBox.Select(index, searchTextBox.Text.Length);
                richTextBox.ScrollToCaret();
            }
        }
        #endregion
    }
}

/***************************************************************************
 *
 * $Author: Turley
 * $Advanced: Nikodemus
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class MultiImportForm : Form
    {
        private readonly int _id;
        private readonly Action<int, MultiComponentList> _changeMultiAction;
        private bool isConverted = false;

        public MultiImportForm(int id, Action<int, MultiComponentList> changeMultiAction)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            _id = id;
            _changeMultiAction = changeMultiAction;

            importTypeComboBox.SelectedIndex = 0;
        }

        #region OnClickBrowse 
        private void OnClickBrowse(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog { Multiselect = false })
            {
                string type = "txt";

                switch (importTypeComboBox.SelectedIndex)
                {
                    case 0:
                        type = "txt";
                        break;
                    case 1:
                        type = "uoa";
                        break;
                    case 2:
                        type = "uoab";
                        break;
                    case 3:
                        type = "wsc";
                        break;
                    case 4:
                        type = "csv";
                        break;
                }

                dialog.Title = $"Choose {type} file to import";
                dialog.CheckFileExists = true;
                dialog.Filter = type == "uoab" ? "{0} file (*.uoa)|*.uoa" : string.Format("{0} file (*.{0})|*.{0}", type);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    filenameTextBox.Text = dialog.FileName;

                    // Check if the selected file is a .uoa file
                    if (type == "uoab")
                    {
                        // Display the content of the .uoa file in the RichTextBox
                        // Pass 'false' as the second parameter to display the file as hexadecimal
                        DisplayUoaFile(dialog.FileName, false);
                    }
                    else
                    {
                        // Load the contents of the file
                        string fileContent = File.ReadAllText(dialog.FileName);

                        // Display the content in the RichTextBox
                        richTextBoxFile.Text = fileContent;
                    }
                }

                // Set isConverted to false because no conversion has been made yet
                isConverted = false;
            }
        }
        #endregion

        #region  OnClickImport
        private void OnClickImport(object sender, EventArgs e)
        {
            Multis.ImportType type = (Multis.ImportType)importTypeComboBox.SelectedIndex;
            MultiComponentList multi;

            if (isConverted)
            {
                // Write the contents of the richTextBox to a temporary file
                string tempFile = Path.GetTempFileName();
                File.WriteAllText(tempFile, richTextBoxFile.Text);

                // Import from the temporary file
                multi = Multis.ImportFromFile(_id, tempFile, type);

                // Delete the temporary file
                File.Delete(tempFile);
            }
            else
            {
                // Check whether the file exists
                if (!File.Exists(filenameTextBox.Text))
                {
                    return;
                }

                // Import from file
                multi = Multis.ImportFromFile(_id, filenameTextBox.Text, type);
            }

            _changeMultiAction(_id, multi);
            Options.ChangedUltimaClass["Multis"] = true;
            Close();
        }
        #endregion

        #region OnClickConvert
        private void OnClickConvert(object sender, EventArgs e)
        {
            // Get the lines from the richTextBox
            string[] lines = richTextBoxFile.Text.Split('\n');

            // Remove the first 4 lines
            lines = lines.Skip(4).ToArray();

            // Convert the numbers to hexadecimal
            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(' ');
                if (parts.Length > 0 && int.TryParse(parts[0], out int number))
                {
                    parts[0] = $"0x{number:X}";
                    lines[i] = string.Join(" ", parts);
                }
            }

            // Update the richTextBox with the converted text
            richTextBoxFile.Text = string.Join("\n", lines);

            // Set isConverted to true since a conversion has been made
            isConverted = true;
        }
        #endregion
               
        #region DisplayUoaFile
        private void DisplayUoaFile(string filePath, bool showAsAscii)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                long offset = 0;
                StringBuilder sb = new StringBuilder();

                int byteRead;
                while ((byteRead = stream.ReadByte()) != -1)
                {
                    if (showAsAscii)
                    {
                        // ASCII representation
                        if (byteRead == 10) // '\n' in ASCII
                        {
                            sb.Append("\n");
                        }
                        else if (byteRead >= 32 && byteRead <= 126)
                        {
                            sb.Append((char)byteRead);
                        }
                    }
                    else
                    {
                        // Hexadecimal representation
                        if (offset % 16 == 0)
                        {
                            sb.AppendFormat("{0:x8}  ", offset);
                        }

                        sb.AppendFormat("{0:x2} ", byteRead);

                        if (offset % 16 == 7)
                        {
                            sb.Append(" ");
                        }
                        else if (offset % 16 == 15)
                        {
                            sb.Append("\n");
                        }

                        offset++;
                    }
                }

                richTextBoxFile.Text = sb.ToString();
            }
        }
        #endregion

        #region checkBoxShowASCII_CheckedChanged
        private void checkBoxShowASCII_CheckedChanged(object sender, EventArgs e)
        {
            // Check if a file is selected
            if (!string.IsNullOrEmpty(filenameTextBox.Text))
            {
                // Display the file according to the state of the checkbox
                DisplayUoaFile(filenameTextBox.Text, checkBoxShowASCII.Checked);
            }
        }
        #endregion
    }
}

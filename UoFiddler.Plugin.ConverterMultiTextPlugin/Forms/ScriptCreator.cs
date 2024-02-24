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

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class ScriptCreator : Form
    {
        public ScriptCreator()
        {
            InitializeComponent();

            this.textBoxBookTitle.TextChanged += new System.EventHandler(this.textBoxBookTitle_TextChanged);
            this.textBoxBookAuthor.TextChanged += new System.EventHandler(this.textBoxBookAuthor_TextChanged);

            // Create an ImageList and add your images
            ImageList imageList = new ImageList();
            imageList.Images.Add("0x1C11", Properties.Resources._0x1C11); // 1 Picture
            imageList.Images.Add("0x1C13", Properties.Resources._0x1C13); // 2 ...
            imageList.Images.Add("0x1E20", Properties.Resources._0x1E20); // 3 ..
            imageList.Images.Add("0x2D9D", Properties.Resources._0x2D9D); // 4
            imageList.Images.Add("0x22C5", Properties.Resources._0x22C5); // 5
            imageList.Images.Add("0x23A0", Properties.Resources._0x23A0); // 6
            imageList.Images.Add("0x225A", Properties.Resources._0x225A); // 7
            imageList.Images.Add("0x238C", Properties.Resources._0x238C); // 8
            imageList.Images.Add("0x2252", Properties.Resources._0x2252); // 9
            imageList.Images.Add("0x2253", Properties.Resources._0x2253); // 10
            imageList.Images.Add("0x2254", Properties.Resources._0x2254); // 11
            imageList.Images.Add("0x2259", Properties.Resources._0x2259); // 12
            imageList.Images.Add("0xE3B", Properties.Resources._0xE3B); // 13
            imageList.Images.Add("0xFBE", Properties.Resources._0xFBE); // 14
            imageList.Images.Add("0xFEF", Properties.Resources._0xFEF); // 15
            imageList.Images.Add("0xFF0", Properties.Resources._0xFF0); // 16
            imageList.Images.Add("0xFF1", Properties.Resources._0xFF1); // 17
            imageList.Images.Add("0xFF2", Properties.Resources._0xFF2); // 18
            imageList.Images.Add("0xFF3", Properties.Resources._0xFF3); // 19
            imageList.Images.Add("0xFF4", Properties.Resources._0xFF4); // 20


            // Add more pictures here...

            // Fill the comboBoxBookGrafic with the images
            foreach (string key in imageList.Images.Keys)
            {
                comboBoxBookGrafic.Items.Add(key);
            }

            // Set the DrawItem event
            comboBoxBookGrafic.DrawMode = DrawMode.OwnerDrawFixed;
            comboBoxBookGrafic.DrawItem += (sender, e) =>
            {
                e.DrawBackground();
                e.Graphics.DrawImage(imageList.Images[e.Index], e.Bounds.Left, e.Bounds.Top);
                e.Graphics.DrawString(comboBoxBookGrafic.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left + imageList.ImageSize.Width, e.Bounds.Top);
                e.DrawFocusRectangle();
            };

            // Set the SelectedIndexChanged event
            this.comboBoxBookGrafic.SelectedIndexChanged += new System.EventHandler(this.comboBoxBookGrafic_SelectedIndexChanged);

            // Fill the comboBoxEnableAutoformat with the options
            comboBoxEnableAutoformat.Items.Add("Enable Autoformat");
            comboBoxEnableAutoformat.Items.Add("Disable Autoformat");

            // Select “Disable Autoformat” by default
            comboBoxEnableAutoformat.SelectedItem = "Disable Autoformat";

            // Set the SelectedIndexChanged event
            this.comboBoxEnableAutoformat.SelectedIndexChanged += new System.EventHandler(this.comboBoxEnableAutoformat_SelectedIndexChanged);
        }

        #region buttonCreateBookScript_Click
        private void buttonCreateBookScript_Click(object sender, EventArgs e)
        {
            string bookTitle = textBoxBookTitle.Text;
            string bookAuthor = textBoxBookAuthor.Text;
            string bookFilename = textBoxBookFilename.Text.Replace(" ", ""); // Removes spaces from the class name
            string grafikID = textBoxGrafikID.Text;
            string coverHue = textBoxCoverHue.Text;            
            bool enableAutoFormat = comboBoxEnableAutoformat.SelectedItem != null ? comboBoxEnableAutoformat.SelectedItem.ToString() == "Enable Autoformat" : false;
            string bookContent = textBoxBookContent.Text;

            // Check that all required fields are filled out
            if (string.IsNullOrEmpty(textBoxBookTitle.Text) ||
                string.IsNullOrEmpty(textBoxBookAuthor.Text) ||
                string.IsNullOrEmpty(textBoxBookFilename.Text) ||
                string.IsNullOrEmpty(textBoxGrafikID.Text) ||
                string.IsNullOrEmpty(textBoxCoverHue.Text) ||
                comboBoxEnableAutoformat.SelectedItem == null)
            {
                MessageBox.Show("Please fill out all fields before creating the script.");
                return;
            }


            // Autoformat content if enabled
            if (enableAutoFormat)
            {
                // Replace all line breaks with a space
                bookContent = bookContent.Replace("\n", " ").Replace("\r", " ");

                // Perform a recursive scan to remove multiple spaces
                while (bookContent.Contains("  "))
                {
                    bookContent = bookContent.Replace("  ", " ");
                }

                // Remove newlines and spaces at the beginning
                bookContent = bookContent.TrimStart();
            }

            // Division of content into pages
            List<string> pageContents = SplitContentIntoPages(bookContent, 8); // Say 8 lines per page

            // Splitting content into pages based on characters
            pageContents = SplitContentIntoPagesByCharacters(bookContent, 90); // Assume 90 characters per page


            // Creation of the BookPageInfo objects for each page
            StringBuilder bookPageInfos = new StringBuilder();
            foreach (string pageContent in pageContents)
            {
                List<string> lines = SplitContentIntoLines(pageContent, 30); // Assume 30 characters per line
                string formattedPageContent = FormatPageContentForBookPageInfo(string.Join(",\n", lines));
                bookPageInfos.AppendLine(formattedPageContent);
            }


            // Generation of the script with the BookPageInfo objects
            string script = GenerateBookScript(bookTitle, bookAuthor, bookFilename, grafikID, coverHue, bookPageInfos.ToString());
            textBoxScriptResult.Text = script;
        }
        #endregion

        #region SplitContentIntoPages
        private List<string> SplitContentIntoPages(string content, int maxLinesPerPage)
        {
            string[] lines = content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            List<string> pages = new List<string>();
            StringBuilder currentPage = new StringBuilder();
            int lineCount = 0;

            foreach (string line in lines)
            {
                currentPage.AppendLine(line);
                lineCount++;

                if (lineCount == maxLinesPerPage)
                {
                    pages.Add(currentPage.ToString());
                    currentPage.Clear();
                    lineCount = 0;
                }
            }

            if (currentPage.Length > 0) // Add the last page if there is one
            {
                pages.Add(currentPage.ToString());
            }

            return pages;
        }
        #endregion

        #region SplitContentIntoPagesByCharacters
        private List<string> SplitContentIntoPagesByCharacters(string content, int maxCharactersPerPage)
        {
            List<string> pages = new List<string>();
            for (int i = 0; i < content.Length; i += maxCharactersPerPage)
            {
                int length = Math.Min(maxCharactersPerPage, content.Length - i);
                pages.Add(content.Substring(i, length));
            }
            return pages;
        }
        #endregion

        #region SplitContentIntoLines
        private List<string> SplitContentIntoLines(string content, int maxCharactersPerLine)
        {
            List<string> lines = new List<string>();
            for (int i = 0; i < content.Length; i += maxCharactersPerLine)
            {
                int length = Math.Min(maxCharactersPerLine, content.Length - i);
                lines.Add(content.Substring(i, length));
            }
            return lines;
        }
        #endregion

        #region FormatPageContentForBookPageInfo
        private string FormatPageContentForBookPageInfo(string pageContent)
        {
            string[] lines = pageContent.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string formattedLines = string.Join(",\n                ", lines.Select(line => $"\"{line.Trim()}\""));
            return $@"new BookPageInfo
            (
                {formattedLines}
            ),";
        }
        #endregion

        #region GenerateBookScript
        private string GenerateBookScript(string title, string author, string filename, string graphicID, string coverHue, string pageInfos)
        {
            return $@"
            using System;
            using Server;            

            namespace Server.Items
            {{
                public class {filename} : BaseBook
            {{
                public static readonly BookContent Content = new BookContent
                (
                    ""{title}"", ""{author}"",
                    {pageInfos.TrimEnd(',', '\n', '\r')}
                );

                public override BookContent DefaultContent{{ get{{ return Content; }} }}

                [Constructable]
                public {filename}() : base( {graphicID}, false )
                {{
                    Hue = {coverHue};
                }}

                public {filename}(Serial serial) : base(serial)
                {{
                }}

                public override void Serialize(GenericWriter writer)
                {{
                    base.Serialize(writer);
                    writer.WriteEncodedInt((int)0);
                }}

                public override void Deserialize(GenericReader reader)
                {{
                    base.Deserialize(reader);
                    int version = reader.ReadEncodedInt();
                }}
            }}
        }}".Trim();
        }
        #endregion

        #region textBoxBookTitle_TextChanged
        private void textBoxBookTitle_TextChanged(object sender, EventArgs e)
        {
            UpdateBookFilename();
        }
        #endregion

        #region textBoxBookAuthor_TextChanged
        private void textBoxBookAuthor_TextChanged(object sender, EventArgs e)
        {
            UpdateBookFilename();
        }
        #endregion

        #region UpdateBookFilename
        private void UpdateBookFilename()
        {
            string bookTitle = textBoxBookTitle.Text.Replace(" ", "_"); // Replaces spaces with underscores
            string bookAuthor = textBoxBookAuthor.Text.Replace(" ", "_"); // Replaces spaces with underscores

            // Create the class name based on the book title and author
            string className = "CBook" + bookTitle + bookAuthor;

            // Update the textBoxBookFilename
            textBoxBookFilename.Text = className;
        }
        #endregion

        #region comboBoxBookGrafic_SelectedIndexChanged
        private void comboBoxBookGrafic_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Set the contents of the textBoxGrafikID to the selected hex address
            textBoxGrafikID.Text = comboBoxBookGrafic.SelectedItem.ToString();
        }
        #endregion

        #region comboBoxEnableAutoformat_SelectedIndexChanged
        private void comboBoxEnableAutoformat_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check if Autoformat is enabled
            bool enableAutoFormat = comboBoxEnableAutoformat.SelectedItem.ToString() == "Enable Autoformat" ? true : false;

            if (enableAutoFormat)
            {
                // Replace all line breaks with a space
                textBoxBookContent.Text = textBoxBookContent.Text.Replace("\n", " ").Replace("\r", " ");

                // Perform a recursive scan to remove multiple spaces
                while (textBoxBookContent.Text.Contains("  "))
                {
                    textBoxBookContent.Text = textBoxBookContent.Text.Replace("  ", " ");
                }

                // Remove newlines and spaces at the beginning
                textBoxBookContent.Text = textBoxBookContent.Text.TrimStart();
            }
        }
        #endregion

        #region buttonTest_Click
        private void buttonTest_Click(object sender, EventArgs e)
        {
            // Fill the TextBoxes and ComboBoxes with test data
            textBoxBookTitle.Text = "TestTitle";
            textBoxBookAuthor.Text = "TestAuthor";
            textBoxBookFilename.Text = "CBookTestTitleTestAuthor";
            comboBoxBookGrafic.SelectedIndex = 0; // Select the first item in the ComboBox
            textBoxGrafikID.Text = "0x1C11";
            textBoxCoverHue.Text = "684";
            comboBoxEnableAutoformat.SelectedIndex = 0; // Select the first item in the ComboBox
            textBoxBookContent.Text = "This is test content.";
        }
        #endregion

        #region DataGridViewColorCell
        public class DataGridViewColorCell : DataGridViewTextBoxCell
        {
            protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
            {
                Color color = Color.White;
                if (value is Color)
                {
                    color = (Color)value;
                }

                using (Brush brush = new SolidBrush(color))
                {
                    graphics.FillRectangle(brush, cellBounds);
                }
            }
        }
        #endregion

        #region btColorSet_Click        
        private void btColorSet_Click(object sender, EventArgs e)
        {
            // Create a new form
            Form colorForm = new Form();
            colorForm.Text = "Choose a color";
            colorForm.Size = new Size(1000, 600);  // Set the size of the form
            colorForm.FormBorderStyle = FormBorderStyle.FixedDialog;  // Disable form resizing

            // Create a new DataGridView
            DataGridView dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.EnableHeadersVisualStyles = false;  // Add this line
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;  // Adjust the column width to fill the available space
            colorForm.Controls.Add(dgv);

            // Add columns for the Hue values, colors, and Photoshop color codes
            dgv.Columns.Add("Hue", "Hue");
            dgv.Columns.Add("Color", "Color");
            dgv.Columns.Add("PSColor", "PS Color");

            // Add a color preview column
            DataGridViewColumn colorPreviewColumn = new DataGridViewColumn(new DataGridViewColorCell());
            colorPreviewColumn.Name = "ColorPreview";
            dgv.Columns.Add(colorPreviewColumn);

            // Fill the DataGridView with all colors in the 16-bit 555 color scheme
            for (int hue = 1; hue <= 32768; hue++)
            {
                Color color = HueToRGB555(hue);
                string psColor = RGBToPhotoshopColorCode(color);
                dgv.Rows.Add(hue, color.Name, psColor, color);
            }

            // When the user clicks on a row, copy the Hue value to the textBoxCoverHue
            dgv.CellClick += (s, args) =>
            {
                if (args.RowIndex >= 0)
                {
                    textBoxCoverHue.Text = dgv.Rows[args.RowIndex].Cells["Hue"].Value.ToString();
                    int hue = int.Parse(dgv.Rows[args.RowIndex].Cells["Hue"].Value.ToString());
                    textBoxCoverHue.BackColor = HueToRGB555(hue);
                }
            };

            // View the form
            colorForm.ShowDialog();
        }

        // Convert a .NET Color to a Photoshop color code
        private string RGBToPhotoshopColorCode(Color color)
        {
            // Photoshop uses the hexadecimal RGB color code
            return color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        #endregion

        #region Color HueToRGB555
        private Color HueToRGB555(int hue)
        {
            // Divide the Hue value into three parts
            int r = (hue >> 10) & 0x1F;
            int g = (hue >> 5) & 0x1F;
            int b = hue & 0x1F;

            // Scale the values to the range 0 to 255
            r = (r * 255) / 31;
            g = (g * 255) / 31;
            b = (b * 255) / 31;

            return Color.FromArgb(r, g, b);
        }

        #endregion

        #region textBoxCoverHue_TextChanged
        private void textBoxCoverHue_TextChanged(object sender, EventArgs e)
        {
            // Check whether textBoxCoverHue.Text contains a value
            if (!string.IsNullOrEmpty(textBoxCoverHue.Text))
            {
                // Try parsing the value
                int hue;
                if (int.TryParse(textBoxCoverHue.Text, out hue))
                {
                    // Convert the Hue value to a color
                    Color color = HueToRGB555(hue);

                    // Color the background of the textBoxCoverHue accordingly
                    textBoxCoverHue.BackColor = color;
                }
                else
                {
                    // The value could not be parsed
                    MessageBox.Show("Please enter a valid number.");
                }
            }
            else
            {
                // textBoxCoverHue.Text is empty
                textBoxCoverHue.BackColor = SystemColors.Window; // Reset the background to the default color
            }
        }
        #endregion

        #region btClipBoard_Click
        private void btClipBoard_Click(object sender, EventArgs e)
        {
            // Check if textBoxScriptResult.Text has a value
            if (!string.IsNullOrEmpty(textBoxScriptResult.Text))
            {
                // Copy the contents of textBoxScriptResult to the clipboard
                Clipboard.SetText(textBoxScriptResult.Text);
            }
            else
            {
                // Show a message if textBoxScriptResult.Text is empty
                MessageBox.Show("There is no text to copy to the clipboard.");
            }
        }
        #endregion

        #region btSaveBookScript_Click
        private void btSaveBookScript_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "C# Dateien (*.cs)|*.cs";
            saveFileDialog.FileName = textBoxBookFilename.Text;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(saveFileDialog.FileName, textBoxScriptResult.Text);
            }
        }
        #endregion
    }
}

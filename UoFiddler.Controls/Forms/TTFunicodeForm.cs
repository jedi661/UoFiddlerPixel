// /***************************************************************************
//  *
//  * $Author: Prapilk
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
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UoFiddler.Controls.Forms
{
    public partial class TTFunicodeForm : Form
    {        
        private static TTFunicodeForm currentInstance = null; // Static variable to store the current instance of the shape
        private string importedTTFFilePath;
        private float selectedFontSize = 12; // Default font size
        private FontStyle selectedFontStyle = FontStyle.Regular; // Default font style                                                                 
        private string lastImportedDirectory; // Variable to store the path of the last imported directory

        public TTFunicodeForm()
        {
            InitializeComponent();
            InitializeControls();
        }

        #region InitializeControls
        private void InitializeControls()
        {
            // Add different font sizes to the ComboBoxFontSize
            comboBoxFontSize.Items.AddRange(new object[] { 10, 12, 14, 16, 18, 20 });
            comboBoxFontSize.SelectedIndex = 1; // Select the default font size

            // Add different font styles to the ComboBoxFontStyle
            comboBoxFontStyle.Items.AddRange(Enum.GetValues(typeof(FontStyle)).Cast<object>().ToArray());
            comboBoxFontStyle.SelectedIndex = 0; // Select the default font style

            // Associate the events with the ComboBoxes
            comboBoxFontSize.SelectedIndexChanged += ComboBoxFontSize_SelectedIndexChanged;
            comboBoxFontStyle.SelectedIndexChanged += ComboBoxFontStyle_SelectedIndexChanged;
        }
        #endregion

        #region TFunicodeForm GetCurrentInstance
        // Static method to get the current instance of the shape
        public static TTFunicodeForm GetCurrentInstance()
        {
            if (currentInstance == null || currentInstance.IsDisposed)
            {
                currentInstance = new TTFunicodeForm();
            }
            return currentInstance;
        }
        #endregion

        #region ButtonImportTTF
        private void ButtonImportTTF_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Font files (*.ttf)|*.ttf|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Save the directory of the selected file
                lastImportedDirectory = Path.GetDirectoryName(openFileDialog.FileName);

                importedTTFFilePath = openFileDialog.FileName;
                DisplayGlyphPreview(importedTTFFilePath);
                MessageBox.Show("Import completed.");
            }
        }
        #endregion

        #region ButtonExport
        private void ButtonExport_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(importedTTFFilePath))
            {
                string directoryPath = Path.GetDirectoryName(importedTTFFilePath);
                string fontFileNameWithoutExtension = Path.GetFileNameWithoutExtension(importedTTFFilePath);
                try
                {
                    ExportGlyphsAsBitmaps(importedTTFFilePath, directoryPath, fontFileNameWithoutExtension);
                    MessageBox.Show("Export completed successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error during the export of characters in .bmp format: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Please import a TTF file first.");
            }
        }
        #endregion

        #region DisplayGlyphPreview
        private void DisplayGlyphPreview(string ttfFilePath)
        {
            var fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile(ttfFilePath);
            var fontFamily = fontCollection.Families[0];

            // Base Unicode text to display
            string unicodeText = "abcdefghijklm nopqrstuvw xyz A BCD EFG H IJK L M N O PQ R ST U V W X Y Z + */ û1234567890.:,;'" + "(!?)-=éèàçùê â";

            // Create an image containing the rendered text
            Bitmap bmp = new Bitmap(pictureBoxGlyphPreview.Width, pictureBoxGlyphPreview.Height);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                // Clear the image with a white background
                graphics.Clear(Color.White);

                // Render the text on the image with the selected font size and style
                using (var font = new Font(fontFamily, selectedFontSize, selectedFontStyle))
                {
                    graphics.DrawString(unicodeText, font, Brushes.Black, PointF.Empty);
                }
            }

            // Display the image in the PictureBox
            pictureBoxGlyphPreview.Image = bmp;
        }
        #endregion

        #region ExportGlyphsAsBitmaps
        private void ExportGlyphsAsBitmaps(string ttfFilePath, string outputDirectory, string fontFileNameWithoutExtension)
        {
            // Load the TrueType font using PrivateFontCollection
            using (PrivateFontCollection privateFontCollection = new PrivateFontCollection())
            {
                privateFontCollection.AddFontFile(ttfFilePath);

                // Create a Font object from the loaded font
                using (Font font = new Font(privateFontCollection.Families[0], selectedFontSize, selectedFontStyle))
                {
                    // Create an array of standard Unicode characters to export
                    int startUnicode = 32; // Start of the standard Unicode character range
                    int endUnicode = 126; // End of the standard Unicode character range

                    // Iterate over each standard Unicode character to export
                    for (int charCode = startUnicode; charCode <= endUnicode; charCode++)
                    {
                        char character = (char)charCode;

                        // Create a Bitmap for each character and draw it
                        Bitmap bitmap = new Bitmap(1, 1); // Start with a small size
                        using (Graphics graphics = Graphics.FromImage(bitmap))
                        {
                            SizeF size = graphics.MeasureString(character.ToString(), font);

                            // Adjust the size of the bitmap based on the size of the text
                            bitmap = new Bitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));

                            // Redraw the text on the resized bitmap
                            using (Graphics g = Graphics.FromImage(bitmap))
                            {
                                g.Clear(Color.White); // White background
                                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                                g.DrawString(character.ToString(), font, Brushes.Black, new PointF(0, 0));
                            }
                        }

                        // Save the Bitmap in BMP format
                        string bmpFileName = $"{outputDirectory}\\{fontFileNameWithoutExtension}_char_{charCode}.bmp";
                        bitmap.Save(bmpFileName, ImageFormat.Bmp);
                    }
                }
            }
        }
        #endregion

        #region ComboBoxFontSize_SelectedIndexChanged
        private void ComboBoxFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedFontSize = Convert.ToSingle(comboBoxFontSize.SelectedItem);
            // Refresh the glyph preview with the new font size
            DisplayGlyphPreview(importedTTFFilePath);
        }
        #endregion

        #region ComboBoxFontStyle_SelectedIndexChanged
        private void ComboBoxFontStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update the selected font style
            selectedFontStyle = (FontStyle)comboBoxFontStyle.SelectedItem;
            // Refresh the glyph preview with the new font style
            DisplayGlyphPreview(importedTTFFilePath);
        }
        #endregion

        #region btOpenDir
        private void btOpenDir_Click(object sender, EventArgs e)
        {
            // Check if a directory was imported
            if (!string.IsNullOrEmpty(lastImportedDirectory))
            {
                // Open the directory in Windows Explorer
                System.Diagnostics.Process.Start("explorer.exe", lastImportedDirectory);
            }
            else
            {
                MessageBox.Show("Please import a file first.");
            }
        }
        #endregion
    }
}


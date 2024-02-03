/***************************************************************************
 *
 * $Author: Bittiez
 * Advanced Nikodemus
 * 
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

using UoFiddler.Plugin.ConverterMultiTextPlugin.Forms;
using System.Diagnostics;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class IsoTiloSlicer : Form
    {
        private ImageHandler imageHandler;

        public IsoTiloSlicer()
        {
            InitializeComponent();
            imageHandler = new ImageHandler();
        }

        private void BtnSelectImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtImagePath.Text = openFileDialog.FileName;
                    Image image = Image.FromFile(openFileDialog.FileName);
                    picImagePreview.Image = image;
                    imageHandler.ImagePath = openFileDialog.FileName;

                    // Zeigen Sie die Größe des Bildes im lbImageSize Label an
                    lbImageSize.Text = $"Bildgröße: {image.Width} x {image.Height}";
                }
            }
        }


        private void CmbCommands_SelectedIndexChanged(object sender, EventArgs e)
        {
            string command = cmbCommands.SelectedItem.ToString();

            switch (command)
            {
                case "--image path":
                    imageHandler.ImagePath = txtImagePath.Text;
                    break;
                case "--tilesize 44":
                    imageHandler.TileWidth = 44;
                    imageHandler.TileHeight = 44;
                    break;
                case "--offset 1":
                    imageHandler.Offset = 1;
                    break;
                case "--output out":
                    imageHandler.OutputDirectory = Path.GetDirectoryName(txtImagePath.Text);
                    break;
                case "--filename {0}":
                    imageHandler.FileNameFormat = "{0}";
                    break;
                case "--startingnumber 0":
                    imageHandler.StartingFileNumber = 0;
                    break;
            }
        }


        private void BtnRun_Click(object sender, EventArgs e)
        {
            // Get the path to the program directory
            string programDirectory = Application.StartupPath;

            // Define the path to the temporary directory in the program directory
            string directory = Path.Combine(programDirectory, "tempGrafic");

            // Set the output directory of the image handler
            imageHandler.OutputDirectory = directory;

            // Process the image
            imageHandler.Process();
        }

        private void buttonOpenTempGrafic_Click(object sender, EventArgs e)
        {
            // Get the path to the program directory
            string programDirectory = Application.StartupPath;

            // Define the path to the temporary directory in the program directory
            string directory = Path.Combine(programDirectory, "tempGrafic");

            // Check if the directory exists
            if (Directory.Exists(directory))
            {
                // Open the directory in the file explorer
                Process.Start("explorer.exe", directory);
            }
            else
            {
                // Display a message to the user indicating that the directory does not exist
                MessageBox.Show("The directory tempGraphic does not exist.");
            }
        }
    }
}

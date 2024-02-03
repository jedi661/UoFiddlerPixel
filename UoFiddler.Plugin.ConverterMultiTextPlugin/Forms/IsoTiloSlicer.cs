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

        #region BtnSelectImage
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

                    // Display the size of the image in the lbImageSize label
                    lbImageSize.Text = $"Image size: {image.Width} x {image.Height}";
                }
            }
        }
        #endregion

        #region CmbCommands
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
        #endregion

        #region BtnRun
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
        #endregion

        #region buttonOpenTempGrafic
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
        #endregion

        #region LoadToolStripMenuItem
        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Image image = new Bitmap(openFileDialog.FileName);
                    picImagePreview.Image = image;

                    // Display the size of the image in the lbImageSize label
                    lbImageSize.Text = $"Image size: {image.Width} x {image.Height}";
                }
            }
        }
        #endregion

        #region runClipbordToolStripMenuItem
        private void runClipbordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if there is an image in the PictureBox
            if (picImagePreview.Image == null)
            {
                MessageBox.Show("Please insert a picture into the PictureBox.");
                return;
            }

            // Verify that a selection was made using cmbCommands
            if (cmbCommands.SelectedItem == null)
            {
                MessageBox.Show("Please make a selection using cmbCommands.");
                return;
            }

            // Get the path to the program directory
            string programDirectory = Application.StartupPath;

            // Define the path to the temporary directory in the program directory
            string directory = Path.Combine(programDirectory, "tempGrafic");

            // Set the output directory of the image handler
            imageHandler.OutputDirectory = directory;

            // Convert the Image in the PictureBox to a Bitmap and save it to a temporary file
            Bitmap bmp = new Bitmap(picImagePreview.Image);
            string tempFilePath = Path.Combine(directory, "temp.bmp");
            bmp.Save(tempFilePath);

            // Set the ImagePath of the image handler to the temporary file
            imageHandler.ImagePath = tempFilePath;

            // Process the image
            imageHandler.Process();
        }
        #endregion

        #region importToolStripMenuItem
        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                Image image = Clipboard.GetImage();
                picImagePreview.Image = image;

                // Display the size of the image in the lbImageSize label
                lbImageSize.Text = $"Image size: {image.Width} x {image.Height}";
            }
            else
            {
                MessageBox.Show("The clipboard does not contain an image.");
            }
        }
        #endregion

        #region Mirror
        private void mirrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (picImagePreview.Image != null)
            {
                Bitmap bmp = new Bitmap(picImagePreview.Image);
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                picImagePreview.Image = bmp;
            }
            else
            {
                MessageBox.Show("There is no image to mirror in the PictureBox.");
            }
        }
        #endregion
    }
}

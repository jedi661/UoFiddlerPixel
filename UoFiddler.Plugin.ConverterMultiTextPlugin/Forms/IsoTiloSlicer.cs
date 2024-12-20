﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Forms;
using System.Diagnostics;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class IsoTiloSlicer : Form
    {
        private ImageHandler1 imageHandler1;
        private ImageHandler2 imageHandler2;

        public IsoTiloSlicer()
        {
            InitializeComponent();
            imageHandler1 = new ImageHandler1();
            imageHandler2 = new ImageHandler2();
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
                    imageHandler1.ImagePath = openFileDialog.FileName;
                    imageHandler2.ImagePath = openFileDialog.FileName;

                    // Display the size of the image in the lbImageSize label
                    lbImageSize.Text = $"Bildgröße: {image.Width} x {image.Height}";
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
                    imageHandler1.ImagePath = txtImagePath.Text;
                    imageHandler2.ImagePath = txtImagePath.Text;
                    break;
                case "--tilesize 44":
                    imageHandler1.TileWidth = 44;
                    imageHandler1.TileHeight = 44;
                    imageHandler2.TileWidth = 44;
                    imageHandler2.TileHeight = 44;
                    break;
                case "--offset 1":
                    imageHandler1.Offset = 1;
                    imageHandler2.Offset = 1;
                    break;
                case "--output out":
                    imageHandler1.OutputDirectory = Path.GetDirectoryName(txtImagePath.Text);
                    imageHandler2.OutputDirectory = Path.GetDirectoryName(txtImagePath.Text);
                    break;
                case "--filename {0}":
                    imageHandler1.FileNameFormat = "{0}";
                    imageHandler2.FileNameFormat = "{0}";
                    break;
                case "--startingnumber 0":
                    imageHandler1.StartingFileNumber = 0;
                    imageHandler2.StartingFileNumber = 0;
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
            imageHandler1.OutputDirectory = directory;

            // Process the image with ImageHandler1
            if (!imageHandler1.Process())
            {
                MessageBox.Show(imageHandler1.LastErrorMessage);
                return;
            }

            MessageBox.Show("Image processed with ImageHandler1.");
        }
        #endregion

        #region BtnRun2
        private void BtnRun2_Click(object sender, EventArgs e)
        {
            // Get the path to the program directory
            string programDirectory = Application.StartupPath;

            // Define the path to the temporary directory in the program directory
            string directory = Path.Combine(programDirectory, "tempGrafic");

            // Set the output directory of the image handler
            imageHandler2.OutputDirectory = directory;

            // Process the image with ImageHandler2
            if (!imageHandler2.Process())
            {
                MessageBox.Show(imageHandler2.LastErrorMessage);
                return;
            }

            MessageBox.Show("Image processed with ImageHandler2.");
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
                MessageBox.Show("Das Verzeichnis tempGrafic existiert nicht.");
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
                    lbImageSize.Text = $"Bildgröße: {image.Width} x {image.Height}";
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
                MessageBox.Show("Bitte ein Bild in die PictureBox einfügen.");
                return;
            }

            // Verify that a selection was made using cmbCommands
            if (cmbCommands.SelectedItem == null)
            {
                MessageBox.Show("Bitte eine Auswahl mit cmbCommands treffen.");
                return;
            }

            // Get the path to the program directory
            string programDirectory = Application.StartupPath;

            // Define the path to the temporary directory in the program directory
            string directory = Path.Combine(programDirectory, "tempGrafic");

            // Set the output directory of the image handler
            imageHandler1.OutputDirectory = directory;

            // Convert the Image in the PictureBox to a Bitmap and save it to a temporary file
            Bitmap bmp = new Bitmap(picImagePreview.Image);
            string tempFilePath = Path.Combine(directory, "temp.bmp");
            bmp.Save(tempFilePath);

            // Set the ImagePath of the image handler to the temporary file
            imageHandler1.ImagePath = tempFilePath;

            // Process the image with ImageHandler1
            if (!imageHandler1.Process())
            {
                MessageBox.Show(imageHandler1.LastErrorMessage);
                return;
            }

            MessageBox.Show("Image processed with ImageHandler1.");
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
                lbImageSize.Text = $"Bildgröße: {image.Width} x {image.Height}";
            }
            else
            {
                MessageBox.Show("Die Zwischenablage enthält kein Bild.");
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
                MessageBox.Show("Es gibt kein Bild in der PictureBox, das gespiegelt werden kann.");
            }
        }
        #endregion
    }
}

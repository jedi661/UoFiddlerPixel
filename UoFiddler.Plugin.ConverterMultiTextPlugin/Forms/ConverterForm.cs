﻿// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  * 
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;


namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class ConverterForm : Form
    {
        public ConverterForm()
        {
            InitializeComponent();
        }

        #region BtConverterBlack
        private void BtConverterBlack_Click(object sender, EventArgs e)
        {
            ConvertColor(Color.White, Color.Black, "black");
        }
        #endregion

        #region btConverterWhite
        private void BtConverterWhite_Click(object sender, EventArgs e)
        {
            ConvertColor(Color.Black, Color.White, "white");
        }
        #endregion

        #region BtConverterCustom
        private void BtConverterCustom_Click(object sender, EventArgs e)
        {
            using var colorDialog = new ColorDialog();
            // Load the custom colors from the application settings
            string customColorsSetting = Properties.Settings.Default.CustomColors;
            if (!string.IsNullOrEmpty(customColorsSetting))
            {
                int[] customColors = customColorsSetting.Split(',').Select(int.Parse).ToArray();
                colorDialog.CustomColors = customColors;
            }

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Color newColor = colorDialog.Color;
                string folderName = $"custom_{newColor.R}_{newColor.G}_{newColor.B}";

                ConvertColor(Color.Black, newColor, folderName);
                ConvertColor(Color.White, newColor, folderName);

                // Save the custom colors in the application settings
                customColorsSetting = string.Join(",", colorDialog.CustomColors);
                Properties.Settings.Default.CustomColors = customColorsSetting;
                Properties.Settings.Default.Save();
            }
        }
        #endregion

        #region ConvertColor
        private static void ConvertColor(Color fromColor, Color toColor, string folderName)
        {
            using var fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                string directoryPath = fbd.SelectedPath;
                string newDirectoryPath = Path.Combine(directoryPath, folderName);
                // Creates the new directory if it does not exist
                if (!Directory.Exists(newDirectoryPath))
                {
                    Directory.CreateDirectory(newDirectoryPath);
                }

                int count = 0; // Counter for processed images

                foreach (var filePath in Directory.GetFiles(directoryPath))
                {
                    string extension = Path.GetExtension(filePath).ToLower();
                    if (extension == ".bmp" || extension == ".png" || extension == ".jpg" || extension == ".tiff")
                    {
                        using var img = Image.FromFile(filePath);
                        for (int y = 0; y < img.Height; y++)
                        {
                            for (int x = 0; x < img.Width; x++)
                            {
                                Color pixelColor = ((Bitmap)img).GetPixel(x, y);
                                if (pixelColor.R == fromColor.R && pixelColor.G == fromColor.G && pixelColor.B == fromColor.B)
                                {
                                    ((Bitmap)img).SetPixel(x, y, toColor);
                                }
                            }
                        }
                        // Saves the image in the new directory
                        string newFilePath = Path.Combine(newDirectoryPath, Path.GetFileName(filePath));
                        img.Save(newFilePath);
                        count++; // Increment the counter
                    }
                }
                MessageBox.Show($"{count} images have been successfully processed!");
            }
        }

        #endregion

        #region btnOpenColorDialog
        private void BtnOpenColorDialog_Click(object sender, EventArgs e)
        {
            using var colorDialog = new ColorDialog();
            // Load the custom colors from the application settings
            string customColorsSetting = Properties.Settings.Default.CustomColors;
            if (!string.IsNullOrEmpty(customColorsSetting))
            {
                int[] customColors = customColorsSetting.Split(',').Select(int.Parse).ToArray();
                colorDialog.CustomColors = customColors;
            }

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                // Save the custom colors in the application settings
                customColorsSetting = string.Join(",", colorDialog.CustomColors);
                Properties.Settings.Default.CustomColors = customColorsSetting;
                Properties.Settings.Default.Save();
            }
        }
        #endregion

        #region btMirrorImages
        private void BtMirrorImages_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                string directoryPath = fbd.SelectedPath;
                string newDirectoryPath = Path.Combine(directoryPath, "mirror");

                // Creates the new directory if it does not exist
                if (!Directory.Exists(newDirectoryPath))
                {
                    Directory.CreateDirectory(newDirectoryPath);
                }

                int count = 0; // Counter for processed images

                foreach (var filePath in Directory.GetFiles(directoryPath))
                {
                    string extension = Path.GetExtension(filePath).ToLower();

                    if (extension == ".bmp" || extension == ".png" || extension == ".jpg" || extension == ".tiff")
                    {
                        using var img = (Bitmap)Image.FromFile(filePath);
                        img.RotateFlip(RotateFlipType.RotateNoneFlipX);

                        // Saves the mirrored image in the new directory
                        string newFilePath = Path.Combine(newDirectoryPath, Path.GetFileName(filePath));
                        img.Save(newFilePath);

                        count++; // Increment the counter
                    }
                }

                MessageBox.Show($"{count} images have been successfully mirrored!");
            }
        }
        #endregion

        #region btConverterTransparent
        private void BtConverterTransparent_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                string directoryPath = fbd.SelectedPath;
                string newDirectoryPath = Path.Combine(directoryPath, "transparent");

                // Creates the new directory if it does not exist
                if (!Directory.Exists(newDirectoryPath))
                {
                    Directory.CreateDirectory(newDirectoryPath);
                }

                ConvertColorToTransparent(directoryPath, newDirectoryPath, Color.Black);
                ConvertColorToTransparent(directoryPath, newDirectoryPath, Color.White);
            }
        }
        #endregion

        #region ConvertColorToTransparent
        private static void ConvertColorToTransparent(string directoryPath, string newDirectoryPath, Color fromColor)
        {
            int count = 0; // Counter for processed images

            foreach (var filePath in Directory.GetFiles(directoryPath))
            {
                string extension = Path.GetExtension(filePath).ToLower();

                if (extension == ".bmp" || extension == ".png" || extension == ".jpg" || extension == ".tiff")
                {
                    using var img = Image.FromFile(filePath);
                    Bitmap bitmap = new(img);

                    bitmap.MakeTransparent(fromColor);

                    // Saves the image in the new directory
                    string newFilePath = Path.Combine(newDirectoryPath, Path.GetFileName(filePath));
                    bitmap.Save(newFilePath);

                    count++; // Increment the counter
                }
            }

            MessageBox.Show($"{count} images were processed successfully!");
        }

        #endregion

        #region btRotateImages
        private void BtRotateImages_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                string directoryPath = fbd.SelectedPath;
                RotateImages(directoryPath);
            }
        }
        #endregion

        #region RotateImages
        private static void RotateImages(string directoryPath)
        {
            int count = 0; // Counter for processed images

            foreach (var filePath in Directory.GetFiles(directoryPath))
            {
                string extension = Path.GetExtension(filePath).ToLower();

                if (extension == ".bmp" || extension == ".png" || extension == ".jpg" || extension == ".tiff")
                {
                    using var img = Image.FromFile(filePath);
                    // Rotate the image 90 degrees to the left
                    img.RotateFlip(RotateFlipType.Rotate270FlipNone);

                    // Save the rotated image
                    img.Save(filePath);

                    count++; // Increment the counter
                }
            }

            MessageBox.Show($"{count} images have been successfully rotated!");
        }
        #endregion

        #region btConvert
        private void BtConvert_Click(object sender, EventArgs e)
        {
            if (comboBoxFileType.SelectedItem == null)
            {
                MessageBox.Show("Please select a file type from the drop down list.");
                return;
            }

            string selectedFileType = comboBoxFileType.SelectedItem.ToString();

            using var fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                string directoryPath = fbd.SelectedPath;
                string newDirectoryPath = Path.Combine(directoryPath, selectedFileType);

                // Creates the new directory if it does not exist
                if (!Directory.Exists(newDirectoryPath))
                {
                    Directory.CreateDirectory(newDirectoryPath);
                }

                int count = 0; // Counter for processed images

                foreach (var filePath in Directory.GetFiles(directoryPath))
                {
                    string extension = Path.GetExtension(filePath).ToLower();

                    if (extension == ".bmp" || extension == ".png" || extension == ".jpg" || extension == ".tiff")
                    {
                        using var img = Image.FromFile(filePath);
                        // Saves the image in the new directory with selected format
                        string newFilePath = Path.Combine(newDirectoryPath, Path.GetFileNameWithoutExtension(filePath) + $".{selectedFileType}");
                        img.Save(newFilePath, GetImageFormat(selectedFileType));

                        count++; // Increment the counter
                    }
                }

                MessageBox.Show($"{count} images have been successfully converted to .{selectedFileType} format!");
            }
        }
        #endregion

        #region ImageFormat
        private static ImageFormat GetImageFormat(string fileType)
        {
            return fileType.ToLower() switch
            {
                "bmp" => ImageFormat.Bmp,
                "png" => ImageFormat.Png,
                "jpg" => ImageFormat.Jpeg,
                "tiff" => ImageFormat.Tiff,
                _ => ImageFormat.Png,
            };
        }
        #endregion
    }
}
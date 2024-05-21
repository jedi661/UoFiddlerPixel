// /***************************************************************************
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

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class ConverterForm : Form
    {
        public ConverterForm()
        {
            InitializeComponent();
        }

        #region btConverterBlack
        private void btConverterBlack_Click(object sender, EventArgs e)
        {
            ConvertColor(Color.White, Color.Black, "black");
        }
        #endregion

        #region btConverterWhite
        private void btConverterWhite_Click(object sender, EventArgs e)
        {
            ConvertColor(Color.Black, Color.White, "white");
        }
        #endregion

        #region btConverterCustom
        private void btConverterCustom_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog())
            {
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
        }
        #endregion

        #region ConvertColor
        private void ConvertColor(Color fromColor, Color toColor, string folderName)
        {
            using (var fbd = new FolderBrowserDialog())
            {
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

                    foreach (var filePath in Directory.GetFiles(directoryPath))
                    {
                        string extension = Path.GetExtension(filePath).ToLower();

                        if (extension == ".bmp" || extension == ".png" || extension == ".jpg" || extension == ".tiff")
                        {
                            using (var img = Image.FromFile(filePath))
                            {
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
                            }
                        }
                    }

                    MessageBox.Show("All images have been successfully processed!");
                }
            }
        }
        #endregion

        #region btnOpenColorDialog
        private void btnOpenColorDialog_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog())
            {
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
        }
        #endregion

        #region btMirrorImages
        private void btMirrorImages_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
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

                    foreach (var filePath in Directory.GetFiles(directoryPath))
                    {
                        string extension = Path.GetExtension(filePath).ToLower();

                        if (extension == ".bmp" || extension == ".png" || extension == ".jpg" || extension == ".tiff")
                        {
                            using (var img = (Bitmap)Image.FromFile(filePath))
                            {
                                img.RotateFlip(RotateFlipType.RotateNoneFlipX);

                                // Saves the mirrored image in the new directory
                                string newFilePath = Path.Combine(newDirectoryPath, Path.GetFileName(filePath));
                                img.Save(newFilePath);
                            }
                        }
                    }

                    MessageBox.Show("All images have been successfully mirrored!");
                }
            }
        }
        #endregion
    }
}
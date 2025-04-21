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
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;


namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class ParticleGrayForm : Form
    {
        public ParticleGrayForm()
        {
            InitializeComponent();
        }

        #region [ LoadImageToolStripMenuItem_Click ]
        private void LoadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Alle Dateien (*.*)|*.*";
                openFileDialog.Title = "Select image";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        pictureBoxParticleGray.Image = Image.FromFile(openFileDialog.FileName);
                        pictureBoxParticleGray.SizeMode = PictureBoxSizeMode.CenterImage;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading images: {ex.Message}", "Mistake", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        #endregion

        #region [ ClipboardImageToolStripMenuItem_Click ]
        private void ClipboardImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                try
                {
                    pictureBoxParticleGray.Image = Clipboard.GetImage();
                    pictureBoxParticleGray.SizeMode = PictureBoxSizeMode.CenterImage; // Optional: Adjust image size
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image from clipboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("The clipboard does not contain an image.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        private void ConvertParticleGrayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxParticleGray.Image != null)
            {
                try
                {
                    // Convert the image to grayscale
                    Bitmap originalBitmap = new Bitmap(pictureBoxParticleGray.Image);
                    Bitmap grayscaleBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);

                    for (int y = 0; y < originalBitmap.Height; y++)
                    {
                        for (int x = 0; x < originalBitmap.Width; x++)
                        {
                            Color originalColor = originalBitmap.GetPixel(x, y);
                            int grayValue = (int)(originalColor.R * 0.3 + originalColor.G * 0.59 + originalColor.B * 0.11); // Grayscale formula
                            Color grayColor = Color.FromArgb(grayValue, grayValue, grayValue);
                            grayscaleBitmap.SetPixel(x, y, grayColor);
                        }
                    }

                    // Update the PictureBox with the grayscale image
                    pictureBoxParticleGray.Image = grayscaleBitmap;
                    MessageBox.Show("Image successfully converted to grayscale!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    // Show error if something goes wrong
                    MessageBox.Show($"Error converting image to grayscale: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No image found in the PictureBox!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CopyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxParticleGray.Image != null)
            {
                try
                {
                    Clipboard.SetImage(pictureBoxParticleGray.Image);
                    MessageBox.Show("Image copied to clipboard successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error copying image to clipboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No image found in the PictureBox to copy!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SaveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxParticleGray.Image != null)
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Title = "Save Image";
                    saveFileDialog.Filter = "Bitmap Image (*.bmp)|*.bmp|JPEG Image (*.jpg)|*.jpg|PNG Image (*.png)|*.png|TIFF Image (*.tiff)|*.tiff";
                    saveFileDialog.DefaultExt = "bmp";
                    saveFileDialog.AddExtension = true;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            // Determine the image format based on selected file extension
                            ImageFormat imageFormat = ImageFormat.Bmp; // Default to BMP
                            string fileExtension = Path.GetExtension(saveFileDialog.FileName).ToLower();

                            switch (fileExtension)
                            {
                                case ".jpg":
                                    imageFormat = ImageFormat.Jpeg;
                                    break;
                                case ".png":
                                    imageFormat = ImageFormat.Png;
                                    break;
                                case ".tiff":
                                    imageFormat = ImageFormat.Tiff;
                                    break;
                                case ".bmp":
                                default:
                                    imageFormat = ImageFormat.Bmp;
                                    break;
                            }

                            // Save the image
                            pictureBoxParticleGray.Image.Save(saveFileDialog.FileName, imageFormat);
                            MessageBox.Show("Image saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            // Handle any errors during the save process
                            MessageBox.Show($"Error saving image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                // Notify the user if there's no image to save
                MessageBox.Show("No image found in the PictureBox to save!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ButtonPixel_Click(object sender, EventArgs e)
        {
            Form colorForm = new Form
            {
                Text = "Pixel color analysis",
                Width = 1000,
                Height = 970,
                ShowIcon = false
            };

            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical
            };
            colorForm.Controls.Add(splitContainer);

            PictureBox pictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.AutoSize
            };
            Panel picturePanel = new Panel
            {
                AutoScroll = true,
                Dock = DockStyle.Fill
            };
            picturePanel.Controls.Add(pictureBox);
            splitContainer.Panel1.Controls.Add(picturePanel);

            Panel controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30
            };
            ComboBox modeSelector = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 200
            };
            modeSelector.Items.AddRange(new string[] { "Color + Gray", "Just colors", "Grayscale only" });
            modeSelector.SelectedIndex = 0;
            controlPanel.Controls.Add(modeSelector);

            RichTextBox colorBox = new RichTextBox
            {
                Dock = DockStyle.Fill
            };
            splitContainer.Panel2.Controls.Add(colorBox);
            splitContainer.Panel2.Controls.Add(controlPanel);

            Bitmap selectedImage = pictureBoxParticleGray.Image as Bitmap;
            if (selectedImage == null)
            {
                MessageBox.Show("No image found in pictureBoxParticleGray.");
                return;
            }

            int zoomFactor = 10;
            int zoomedWidth = selectedImage.Width * zoomFactor;
            int zoomedHeight = selectedImage.Height * zoomFactor;

            Bitmap zoomedImage = new Bitmap(zoomedWidth, zoomedHeight);
            using (Graphics g = Graphics.FromImage(zoomedImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(selectedImage, new Rectangle(0, 0, zoomedWidth, zoomedHeight));
            }

            pictureBox.Image = zoomedImage;

            StringBuilder colorTextBuilder = new StringBuilder();
            int[,] lineIndices = new int[selectedImage.Width, selectedImage.Height];

            void RenderPixelData()
            {
                colorBox.Invoke(() => colorBox.Clear());

                int currentLineIndex = 0;
                for (int y = 0; y < selectedImage.Height; y++)
                {
                    for (int x = 0; x < selectedImage.Width; x++)
                    {
                        Color pixelColor = selectedImage.GetPixel(x, y);
                        string hexColor = ColorTranslator.ToHtml(pixelColor);

                        bool isGray = pixelColor.R == pixelColor.G && pixelColor.G == pixelColor.B;
                        bool include = true;

                        switch (modeSelector.SelectedItem.ToString())
                        {
                            case "Just colors":
                                include = !isGray;
                                break;
                            case "Grayscale only":
                                include = isGray;
                                break;
                        }

                        if (!include)
                        {
                            lineIndices[x, y] = -1;
                            continue;
                        }

                        string colorText = $"Pixel ({x}, {y}): Color [R={pixelColor.R}, G={pixelColor.G}, B={pixelColor.B}], Hex: {hexColor}";
                        int localLineIndex = currentLineIndex++;
                        lineIndices[x, y] = localLineIndex;

                        colorBox.Invoke((Action)(() =>
                        {
                            int start = colorBox.Text.Length;
                            colorBox.AppendText(colorText);
                            colorBox.AppendText("     ");
                            int colorStart = colorBox.Text.Length;
                            colorBox.AppendText("     ");
                            colorBox.Select(colorStart, 5);
                            colorBox.SelectionBackColor = pixelColor;
                            colorBox.AppendText("\n");
                        }));
                    }
                }
            }

            Task.Run(RenderPixelData);

            modeSelector.SelectedIndexChanged += (s, e) =>
            {
                Task.Run(RenderPixelData);
            };

            int previousLineIndex = -1;
            pictureBox.MouseClick += (s, evt) =>
            {
                int x = evt.X / zoomFactor;
                int y = evt.Y / zoomFactor;

                if (x >= 0 && x < selectedImage.Width && y >= 0 && y < selectedImage.Height)
                {
                    int lineIndex = lineIndices[x, y];

                    if (lineIndex >= 0 && lineIndex < colorBox.Lines.Length)
                    {
                        if (previousLineIndex >= 0 && previousLineIndex < colorBox.Lines.Length)
                        {
                            int prevStart = colorBox.GetFirstCharIndexFromLine(previousLineIndex);
                            int prevLength = colorBox.Lines[previousLineIndex].IndexOf("Hex:") + 10;
                            colorBox.Select(prevStart, prevLength);
                            colorBox.SelectionBackColor = colorBox.BackColor;
                        }

                        int start = colorBox.GetFirstCharIndexFromLine(lineIndex);
                        int length = colorBox.Lines[lineIndex].IndexOf("Hex:") + 10;
                        colorBox.Select(start, length);
                        colorBox.SelectionBackColor = Color.LightGray;
                        colorBox.ScrollToCaret();

                        string lineText = colorBox.Lines[lineIndex];
                        Match match = Regex.Match(lineText, @"Hex: (#?[0-9A-Fa-f]{6})");
                        if (match.Success)
                        {
                            Clipboard.SetText(match.Groups[1].Value);
                        }

                        previousLineIndex = lineIndex;
                    }
                    else
                    {
                        Color pixelColor = selectedImage.GetPixel(x, y);
                        string hexColor = ColorTranslator.ToHtml(pixelColor);
                        Clipboard.SetText(hexColor);

                        string extraLine = $"[Unlisted] Pixel ({x}, {y}): Color [R={pixelColor.R}, G={pixelColor.G}, B={pixelColor.B}], Hex: {hexColor}";
                        colorBox.Invoke((Action)(() =>
                        {
                            colorBox.AppendText(extraLine);
                            colorBox.AppendText("     ");
                            int colorStart = colorBox.Text.Length;
                            colorBox.AppendText("     ");
                            colorBox.Select(colorStart, 5);
                            colorBox.SelectionBackColor = pixelColor;
                            colorBox.AppendText("\n");
                            colorBox.ScrollToCaret();
                        }));
                    }
                }
            };

            colorForm.Show();
        }




    }
}

// /***************************************************************************
//  *
//  * $Author: Nikodemus 
//  * 
//  * "THE BEER-WINE-WARE LICENSE"
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
using System.Drawing.Drawing2D;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Helpers;


namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class ParticleGrayForm : Form
    {
        private bool isDrawing = false;
        private List<Point> maskPoints = new List<Point>();
        private Bitmap originalImage;
        private int zoomLevel = 0; // 0 = original, max +2, min -2
        private const int MaxZoom = 2;
        private const int MinZoom = 0;
        private Bitmap loadedImageOriginal;
        private List<Point> correctionPoints = new List<Point>(); // Yellow correction mask
        private bool correctionMode = false;

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

                        // Save image for later editing
                        originalImage = new Bitmap(pictureBoxParticleGray.Image);
                        loadedImageOriginal = new Bitmap(originalImage); // Secure original condition
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

                    // Save image for later editing
                    originalImage = new Bitmap(pictureBoxParticleGray.Image);
                    loadedImageOriginal = new Bitmap(originalImage); // Secure original condition
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

        #region [ ConvertParticleGrayToolStripMenuItem_Click ]
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
        #endregion

        #region [ CopyToClipboardToolStripMenuItem_Click ]
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
        #endregion

        #region [ SaveImageToolStripMenuItem_Click ]
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
        #endregion

        #region [ ButtonPixel_Click ] // Pixel color analysis
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
        #endregion

        #region [ pictureBoxParticleGray_MouseDown ]
        private void pictureBoxParticleGray_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDrawing = true;

                if (checkBoxCorrection.Checked)
                {
                    correctionPoints.Clear();
                    correctionPoints.Add(e.Location);
                }
                else
                {
                    maskPoints.Clear();
                    maskPoints.Add(e.Location);
                }
            }
        }
        #endregion

        #region [ pictureBoxParticleGray_MouseMove ]
        private void pictureBoxParticleGray_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                if (checkBoxCorrection.Checked)
                    correctionPoints.Add(e.Location);
                else
                    maskPoints.Add(e.Location);

                pictureBoxParticleGray.Invalidate();
            }
        }
        #endregion

        #region [ pictureBoxParticleGray_MouseUp ]        
        private void pictureBoxParticleGray_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDrawing = false;
                pictureBoxParticleGray.Invalidate();
            }
        }
        #endregion

        #region [ pictureBoxParticleGray_Paint ]
        private void pictureBoxParticleGray_Paint(object sender, PaintEventArgs e)
        {
            // Rote Maske
            if (maskPoints.Count > 1)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawLines(pen, maskPoints.ToArray());
                }

                using (Brush brush = new SolidBrush(Color.FromArgb(80, Color.Red)))
                {
                    e.Graphics.FillPolygon(brush, maskPoints.ToArray());
                }
            }

            // Gelbe Korrekturmaske
            if (correctionPoints.Count > 1)
            {
                using (Pen pen = new Pen(Color.Yellow, 2))
                {
                    e.Graphics.DrawLines(pen, correctionPoints.ToArray());
                }

                using (Brush brush = new SolidBrush(Color.FromArgb(80, Color.Yellow)))
                {
                    e.Graphics.FillPolygon(brush, correctionPoints.ToArray());
                }
            }
        }
        #endregion

        #region [ ButtonConvertMask_Click ]
        private void ButtonConvertMask_Click(object sender, EventArgs e)
        {
            if (originalImage == null || maskPoints.Count < 3)
            {
                MessageBox.Show("Kein Bild geladen oder keine gültige Maske gezeichnet.");
                return;
            }

            // Transform mask into image coordinates
            List<Point> translatedPoints = maskPoints.Select(p => TranslateToImageCoordinates(p)).ToList();

            // Create a new edited image
            Bitmap updatedImage = new Bitmap(originalImage);
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddPolygon(translatedPoints.ToArray());

                // Convert mask pixel by pixel to grayscale
                for (int y = 0; y < updatedImage.Height; y++)
                {
                    for (int x = 0; x < updatedImage.Width; x++)
                    {
                        if (path.IsVisible(x, y))
                        {
                            Color c = updatedImage.GetPixel(x, y);
                            int gray = (int)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);
                            Color grayColor = Color.FromArgb(gray, gray, gray);
                            updatedImage.SetPixel(x, y, grayColor);
                        }
                    }
                }
            }

            // Release previous image correctly
            originalImage.Dispose();
            originalImage = updatedImage;

            // Refresh the image display
            pictureBoxParticleGray.Image = new Bitmap(originalImage);
            pictureBoxParticleGray.Invalidate();

            // Reset and apply zoom
            zoomLevel = 0;
            ApplyZoom();

            // Delete mask
            maskPoints.Clear();
        }
        #endregion

        #region [ Paint TranslateToImageCoordinates ]        
        private Point TranslateToImageCoordinates(Point p)
        {
            if (pictureBoxParticleGray.Image == null)
                return p;

            float scale = 1f + zoomLevel * 0.5f;

            PictureBoxSizeMode sizeMode = pictureBoxParticleGray.SizeMode;
            Image image = pictureBoxParticleGray.Image;

            int imgWidth = image.Width;
            int imgHeight = image.Height;
            int pbWidth = pictureBoxParticleGray.Width;
            int pbHeight = pictureBoxParticleGray.Height;

            int offsetX = (pbWidth - imgWidth) / 2;
            int offsetY = (pbHeight - imgHeight) / 2;

            // Calculate mouse position back to image coordinates (including zoom)
            float imageX = (p.X - offsetX) / scale;
            float imageY = (p.Y - offsetY) / scale;

            return new Point((int)imageX, (int)imageY);
        }
        #endregion

        #region [ ApplyZoom ]
        private void ApplyZoom()
        {
            if (originalImage == null)
                return;

            // Calculate scaling factor
            float scale = 1f + zoomLevel * 0.5f; // z. B. 1.5f at ZoomLevel 1

            int newWidth = (int)(originalImage.Width * scale);
            int newHeight = (int)(originalImage.Height * scale);

            Bitmap zoomedImage = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(zoomedImage))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.None;
                g.DrawImage(originalImage, new Rectangle(0, 0, newWidth, newHeight));
            }

            pictureBoxParticleGray.Image = zoomedImage;
            pictureBoxParticleGray.SizeMode = PictureBoxSizeMode.CenterImage;
        }
        #endregion

        #region [ ButtonZoomIn_Click ]
        private void ButtonZoomIn_Click(object sender, EventArgs e)
        {
            if (zoomLevel < MaxZoom)
            {
                zoomLevel++;
                ApplyZoom();
            }
            else
            {
                MessageBox.Show("Maximum zoom reached.");
            }
        }
        #endregion

        #region [ ButtonZoomOut_Click ]
        private void ButtonZoomOut_Click(object sender, EventArgs e)
        {
            if (zoomLevel > MinZoom)
            {
                zoomLevel--;
                ApplyZoom();
            }
            else
            {
                MessageBox.Show("Already in original size.");
            }
        }
        #endregion

        #region [ ButtonZoomReset_Click ]
        private void ButtonResetImage_Click(object sender, EventArgs e)
        {
            if (loadedImageOriginal == null)
            {
                MessageBox.Show("No original image available.");
                return;
            }

            // Share old image
            originalImage?.Dispose();

            // Restore original
            originalImage = new Bitmap(loadedImageOriginal);

            // Reset zoom
            zoomLevel = 0;
            ApplyZoom();

            // Delete mask
            maskPoints.Clear();

            // Redraw
            pictureBoxParticleGray.Invalidate();
        }
        #endregion

        #region [ CheckBoxCorrection_CheckedChanged ]
        private void checkBoxCorrection_CheckedChanged(object sender, EventArgs e)
        {
            correctionMode = checkBoxCorrection.Checked;
        }
        #endregion

        #region [ ButtonApplyCorrection_Click ]
        private void ButtonApplyCorrection_Click(object sender, EventArgs e)
        {
            if (maskPoints.Count < 3 || correctionPoints.Count < 3)
            {
                MessageBox.Show("Mask or correction not valid.");
                return;
            }

            // Convert mask and correction into image coordinates
            List<Point> maskPoly = maskPoints.Select(p => TranslateToImageCoordinates(p)).ToList();
            List<Point> correctionPoly = correctionPoints.Select(p => TranslateToImageCoordinates(p)).ToList();

            // Create mask regions
            using (GraphicsPath maskPath = new GraphicsPath())
            using (GraphicsPath correctionPath = new GraphicsPath())
            {
                maskPath.AddPolygon(maskPoly.ToArray());
                correctionPath.AddPolygon(correctionPoly.ToArray());

                Region maskRegion = new Region(maskPath);
                maskRegion.Exclude(correctionPath); // Subtract yellow area

                // Extract new mask points from the region via bitmap
                List<Point> newMask = new List<Point>();
                using (Bitmap tempBmp = new Bitmap(originalImage.Width, originalImage.Height))
                using (Graphics g = Graphics.FromImage(tempBmp))
                {
                    g.Clear(Color.Black);
                    g.FillRegion(Brushes.White, maskRegion);

                    for (int y = 0; y < tempBmp.Height; y++)
                    {
                        for (int x = 0; x < tempBmp.Width; x++)
                        {
                            if (tempBmp.GetPixel(x, y).R > 0)
                            {
                                newMask.Add(new Point(x, y));
                            }
                        }
                    }
                }

                // Update maskPoints
                maskPoints = newMask.Select(p => TranslateFromImageCoordinates(p)).ToList();
            }

            // Reset correction mask
            correctionPoints.Clear();

            // Initiate redrawing
            pictureBoxParticleGray.Invalidate();
        }
        #endregion

        #region [ Point TranslateFromImageCoordinates ]
        private Point TranslateFromImageCoordinates(Point p)
        {
            if (pictureBoxParticleGray.Image == null)
                return p;

            float scale = 1f + zoomLevel * 0.5f;

            int imgWidth = pictureBoxParticleGray.Image.Width;
            int imgHeight = pictureBoxParticleGray.Image.Height;
            int pbWidth = pictureBoxParticleGray.Width;
            int pbHeight = pictureBoxParticleGray.Height;

            int offsetX = (pbWidth - imgWidth) / 2;
            int offsetY = (pbHeight - imgHeight) / 2;

            float screenX = p.X * scale + offsetX;
            float screenY = p.Y * scale + offsetY;

            return new Point((int)screenX, (int)screenY);
        }
        #endregion

        #region [ ButtonRestoreColorFromMask_Click ]
        private void ButtonRestoreColorFromMask_Click(object sender, EventArgs e)
        {
            if (loadedImageOriginal == null || originalImage == null || maskPoints.Count < 3)
            {
                MessageBox.Show("No valid image or mask found.");
                return;
            }

            List<Point> translatedPoints = maskPoints.Select(p => TranslateToImageCoordinates(p)).ToList();

            Bitmap updatedImage = new Bitmap(originalImage);
            GraphicsPath maskPath = new GraphicsPath();
            maskPath.AddPolygon(translatedPoints.ToArray());

            for (int y = 0; y < updatedImage.Height; y++)
            {
                for (int x = 0; x < updatedImage.Width; x++)
                {
                    if (maskPath.IsVisible(x, y))
                    {
                        Color originalColor = loadedImageOriginal.GetPixel(x, y);
                        updatedImage.SetPixel(x, y, originalColor);
                    }
                }
            }

            originalImage.Dispose();
            originalImage = updatedImage;

            // Update image correctly
            ApplyZoom();

            // Delete mask (optional)
            maskPoints.Clear();

            // Redraw
            pictureBoxParticleGray.Invalidate();
        }
        #endregion

        #region [ ButtonColorizeGrayArea_Click ]
        private void ButtonColorizeGrayArea_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("No image loaded.");
                return;
            }

            string hex = textBoxColorHex.Text.Trim();
            if (!Regex.IsMatch(hex, @"^#?([0-9A-Fa-f]{6})$"))
            {
                MessageBox.Show("Invalid color code. Use, for example, #FF9933.");
                return;
            }

            if (!hex.StartsWith("#"))
                hex = "#" + hex;

            Color targetColor = ColorTranslator.FromHtml(hex);

            Bitmap updatedImage = new Bitmap(originalImage);

            for (int y = 0; y < updatedImage.Height; y++)
            {
                for (int x = 0; x < updatedImage.Width; x++)
                {
                    Color px = updatedImage.GetPixel(x, y);

                    // Only gray areas (R = G = B)
                    if (px.R == px.G && px.G == px.B)
                    {
                        float intensity = px.R / 255f; // Gray brightness
                        int r = (int)(targetColor.R * intensity);
                        int g = (int)(targetColor.G * intensity);
                        int b = (int)(targetColor.B * intensity);

                        updatedImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
            }

            originalImage.Dispose();
            originalImage = updatedImage;

            ApplyZoom();
            pictureBoxParticleGray.Invalidate();
        }
        #endregion

        #region [ RGB color mixer ]
        #region [ ButtonColorfind_Click ]
        private void ButtonColorfind_Click(object sender, EventArgs e)
        {
            using (FormRgbMixer mixer = new FormRgbMixer())
            {
                if (mixer.ShowDialog() == DialogResult.OK)
                {
                    textBoxColorHex.Text = mixer.SelectedHexColor;
                }
            }
        }
        #endregion

        #region [ Clas FormRgbMixer ]
        public class FormRgbMixer : Form
        {
            private TrackBar trackBarR;
            private TrackBar trackBarG;
            private TrackBar trackBarB;
            private Label labelR;
            private Label labelG;
            private Label labelB;
            private PictureBox pictureBoxPreview;
            private TextBox textBoxHex;
            private Button buttonOk;

            public string SelectedHexColor { get; private set; }

            public FormRgbMixer()
            {
                Text = "RGB color mixer";
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ClientSize = new Size(300, 300);
                StartPosition = FormStartPosition.CenterParent;

                trackBarR = new TrackBar { Minimum = 0, Maximum = 255, TickFrequency = 5, Left = 20, Top = 30, Width = 200 };
                trackBarG = new TrackBar { Minimum = 0, Maximum = 255, TickFrequency = 5, Left = 20, Top = 80, Width = 200 };
                trackBarB = new TrackBar { Minimum = 0, Maximum = 255, TickFrequency = 5, Left = 20, Top = 130, Width = 200 };

                labelR = new Label { Left = 230, Top = 30, Width = 40, Text = "0" };
                labelG = new Label { Left = 230, Top = 80, Width = 40, Text = "0" };
                labelB = new Label { Left = 230, Top = 130, Width = 40, Text = "0" };

                pictureBoxPreview = new PictureBox { Left = 20, Top = 180, Width = 100, Height = 50, BorderStyle = BorderStyle.FixedSingle };
                textBoxHex = new TextBox { Left = 130, Top = 190, Width = 130 };
                buttonOk = new Button { Text = "OK", Left = 100, Top = 240, Width = 80 };

                trackBarR.Scroll += (s, e) => UpdateColor();
                trackBarG.Scroll += (s, e) => UpdateColor();
                trackBarB.Scroll += (s, e) => UpdateColor();

                buttonOk.Click += (s, e) =>
                {
                    SelectedHexColor = textBoxHex.Text;
                    DialogResult = DialogResult.OK;
                    Close();
                };

                Controls.Add(trackBarR);
                Controls.Add(trackBarG);
                Controls.Add(trackBarB);
                Controls.Add(labelR);
                Controls.Add(labelG);
                Controls.Add(labelB);
                Controls.Add(pictureBoxPreview);
                Controls.Add(textBoxHex);
                Controls.Add(buttonOk);

                UpdateColor(); // Initial
            }
            #endregion

            #region [ UpdateColor ]
            private void UpdateColor()
            {
                int r = trackBarR.Value;
                int g = trackBarG.Value;
                int b = trackBarB.Value;

                labelR.Text = r.ToString();
                labelG.Text = g.ToString();
                labelB.Text = b.ToString();

                Color color = Color.FromArgb(r, g, b);
                pictureBoxPreview.BackColor = color;

                textBoxHex.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
        }
        #endregion
        #endregion

        #region [ ButtonPixelFind_Click ]
        private void ButtonPixelFind_Click(object sender, EventArgs e)
        {
            if (pictureBoxParticleGray.Image is not Bitmap bitmap)
            {
                MessageBox.Show("No valid image found.");
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Save gray pixel data";
                saveFileDialog.Filter = "Text files (*.txt)|*.txt";
                saveFileDialog.DefaultExt = "txt";
                saveFileDialog.FileName = "Graypixel_analysis.txt";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string header = $"Analysis from {DateTime.Now:G}";
                        GrayPixelAnalyzer.AnalyzeAndSave(new Bitmap(bitmap), saveFileDialog.FileName, header);
                        MessageBox.Show("Analysis saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        #endregion

        #region [ ButtonPixelFindInfo_Click ]
        private void ButtonPixelFindInfo_Click(object sender, EventArgs e)
        {
            if (pictureBoxParticleGray.Image is not Bitmap bitmap)
            {
                MessageBox.Show("No image found.");
                return;
            }

            GrayPixelAnalyzer.ShowInForm(new Bitmap(bitmap), $"Analysis from {DateTime.Now:G}");
        }
        #endregion
    }
}

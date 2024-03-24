using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class TransitionsForm : Form
    {
        private List<Image> textures1 = new List<Image>();
        private List<Image> textures2 = new List<Image>();
        private List<Image> alphaImages = new List<Image>();
        private double gamma = 0.5; // Default value for gamma
        private double blurValue = 0.5; // Default value for blur
        private FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

        public TransitionsForm()
        {
            InitializeComponent();

        }

        #region UpdatePictureBoxes
        private void UpdatePictureBoxes()
        {
            // First clear all controls from FlowLayoutPanels
            flowLayoutPanelTextures1.Controls.Clear();
            flowLayoutPanelTextures2.Controls.Clear();
            flowLayoutPanelAlphaImages.Controls.Clear();

            // Show textures 1
            foreach (Image texture in textures1)
            {
                AddPictureBoxToFlowLayout(flowLayoutPanelTextures1, texture, 128, 128);
            }

            // Show textures 2
            foreach (Image texture in textures2)
            {
                AddPictureBoxToFlowLayout(flowLayoutPanelTextures2, texture, 128, 128);
            }

            // Show alphaImages
            foreach (Image alphaImage in alphaImages)
            {
                AddPictureBoxToFlowLayout(flowLayoutPanelAlphaImages, alphaImage, 128, 128);
            }
        }
        #endregion

        #region AddPictureBoxToFlowLayout
        private void AddPictureBoxToFlowLayout(FlowLayoutPanel flowLayoutPanel, Image image, int width, int height)
        {
            PictureBox pictureBox = new PictureBox();
            pictureBox.Image = image;
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.Size = new Size(width, height);

            flowLayoutPanel.Controls.Add(pictureBox);
        }
        #endregion

        #region RotateAndResizeImageForPreview
        private Image RotateAndResizeImageForPreview(Image image, double angle)
        {
            // Create a new image with the specified size
            Bitmap rotatedImage = new Bitmap(46, 45);

            // Create a transformation matrix for rotation
            Matrix matrix = new Matrix();
            matrix.Translate(23, 22); // Move the origin slightly to the right to compensate for the adjustment
            matrix.Rotate((float)angle); // Rotation
            matrix.Translate(-16, -16); // Adjustment after rotation

            // Draw the original rotated image onto the new image
            using (Graphics graphics = Graphics.FromImage(rotatedImage))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.Transform = matrix;

                // Draw the original image onto the new image, adjusting the size so that it measures exactly 46x45 pixels
                graphics.DrawImage(image, -1, 0, 34, 33);
            }

            return rotatedImage;
        }
        #endregion

        #region UpdatePreview
        private void UpdatePreview()
        {
            if (alphaImages.Count > 0 && textures1.Count > 0 && textures2.Count > 0)
            {
                Image texture1 = textures1[0];
                Image texture2 = textures2[0];
                Image alphaImage = alphaImages[0];

                // Generate the first transition image
                Bitmap transitionImage = GenerateTransition(texture1, texture2, alphaImage);

                // Show first transition image in pictureBoxPreview
                pictureBoxPreview.Image = transitionImage;

                // Resize and rotate image for preview
                Image previewImage = RotateAndResizeImageForPreview(transitionImage, 45);

                // Show preview in pictureBoxLandtile
                pictureBoxLandtile.Image = previewImage;
            }
        }
        #endregion

        #region trackBarContrast
        private void trackBarContrast_Scroll(object sender, EventArgs e)
        {
            gamma = (double)trackBarContrast.Value;
            UpdatePreview();
        }
        #endregion

        #region trackBarFlou_Scroll
        private void trackBarFlou_Scroll(object sender, EventArgs e)
        {
            blurValue = (double)trackBarFlou.Value / 10.0 * 2;
            UpdatePreview();
        }
        #endregion

        #region Bitmap GenerateTransition
        private Bitmap GenerateTransition(Image texture1, Image texture2, Image alphaImage)
        {
            Bitmap blurredAlphaImage = ApplyGamma(ApplyGaussianBlur((Bitmap)alphaImage, blurValue), gamma);
            Bitmap transitionImage = new Bitmap(texture1.Width, texture1.Height);

            for (int x = 0; x < texture1.Width; x++)
            {
                for (int y = 0; y < texture1.Height; y++)
                {
                    Color alphaPixel = blurredAlphaImage.GetPixel(x, y);
                    float alpha = alphaPixel.GetBrightness();

                    Color texture1Pixel = ((Bitmap)texture1).GetPixel(x, y);
                    Color texture2Pixel = ((Bitmap)texture2).GetPixel(x, y);

                    int newRed = (int)(alpha * texture1Pixel.R + (1 - alpha) * texture2Pixel.R);
                    int newGreen = (int)(alpha * texture1Pixel.G + (1 - alpha) * texture2Pixel.G);
                    int newBlue = (int)(alpha * texture1Pixel.B + (1 - alpha) * texture2Pixel.B);

                    newRed = Math.Max(0, Math.Min(255, newRed));
                    newGreen = Math.Max(0, Math.Min(255, newGreen));
                    newBlue = Math.Max(0, Math.Min(255, newBlue));

                    transitionImage.SetPixel(x, y, Color.FromArgb(newRed, newGreen, newBlue));
                }
            }

            return transitionImage;
        }
        #endregion

        #region Bitmap ApplyGaussianBlur
        private Bitmap ApplyGaussianBlur(Bitmap image, double blurValue)
        {
            BlurFilter filter = new BlurFilter();
            filter.Sigma = blurValue;
            return filter.ProcessImage(image);
        }
        #endregion

        #region Bitmap ApplyGamma
        private Bitmap ApplyGamma(Bitmap image, double gamma)
        {
            Bitmap adjustedImage = new Bitmap(image.Width, image.Height);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color originalPixel = image.GetPixel(x, y);
                    int newRed = (int)(255 * Math.Pow(originalPixel.R / 255.0, gamma));
                    int newGreen = (int)(255 * Math.Pow(originalPixel.G / 255.0, gamma));
                    int newBlue = (int)(255 * Math.Pow(originalPixel.B / 255.0, gamma));

                    newRed = Math.Max(0, Math.Min(255, newRed));
                    newGreen = Math.Max(0, Math.Min(255, newGreen));
                    newBlue = Math.Max(0, Math.Min(255, newBlue));

                    adjustedImage.SetPixel(x, y, Color.FromArgb(newRed, newGreen, newBlue));
                }
            }

            return adjustedImage;
        }
        #endregion

        #region btnSelectTexture1
        private void btnSelectTexture1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textures1.Clear();
                foreach (string imagePath in openFileDialog.FileNames)
                {
                    try
                    {
                        Image texture1 = Image.FromFile(imagePath);
                        textures1.Add(texture1);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while loading the image : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                UpdatePictureBoxes();
                UpdatePreview();
            }
        }
        #endregion

        #region btnSelectTexture2
        private void btnSelectTexture2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textures2.Clear();
                foreach (string imagePath in openFileDialog.FileNames)
                {
                    try
                    {
                        Image texture2 = Image.FromFile(imagePath);
                        textures2.Add(texture2);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while loading the image : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                UpdatePictureBoxes();
                UpdatePreview();
            }
        }
        #endregion

        #region btnSelectAlpha
        private void btnSelectAlpha_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.Description = "Select the folder containing the alpha images";

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string folderPath = folderBrowserDialog.SelectedPath;

                try
                {
                    alphaImages.Clear();
                    string[] alphaFiles = Directory.GetFiles(folderPath, "*.png");

                    foreach (string filePath in alphaFiles)
                    {
                        Image alphaImage = Image.FromFile(filePath);
                        alphaImages.Add(alphaImage);
                    }

                    MessageBox.Show($"Loading successful : {alphaImages.Count} alpha images loaded.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading alpha images : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                UpdatePictureBoxes();
                UpdatePreview();
            }
        }
        #endregion

        #region btnGenerateTransition
        private void btnGenerateTransition_Click(object sender, EventArgs e)
        {
            if (textures1.Count == 0 || textures2.Count == 0 || alphaImages.Count == 0)
            {
                MessageBox.Show("Please select textures and alpha images before generating the transition.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                int alphaIndex = 0;

                foreach (Image alphaImage in alphaImages)
                {
                    Image texture1 = textures1[alphaIndex % textures1.Count];
                    Image texture2 = textures2[alphaIndex % textures2.Count];
                    Bitmap transitionImage = GenerateTransition(texture1, texture2, alphaImage);

                    // Save images without rotation
                    string transitionFileName = Path.Combine(folderBrowserDialog.SelectedPath, $"Transition_{alphaIndex + 1}.bmp");
                    transitionImage.Save(transitionFileName, ImageFormat.Bmp);

                    // Preview the transition
                    Image previewImage = RotateAndResizeImageForPreview(transitionImage, 45);

                    // Save images with 45 degree rotation and resized
                    string rotatedTransitionFileName = Path.Combine(folderBrowserDialog.SelectedPath, $"RotatedTransition_{alphaIndex + 1}.bmp");
                    previewImage.Save(rotatedTransitionFileName, ImageFormat.Bmp);

                    alphaIndex++;
                }

                MessageBox.Show("Generation complete.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Empty lists
                textures1.Clear();
                textures2.Clear();
                alphaImages.Clear();

                // Clear PictureBoxes
                pictureBoxTexture1.Image = null;
                pictureBoxTexture2.Image = null;
                pictureBoxAlpha1.Image = null;
                pictureBoxPreview.Image = null;
                pictureBoxLandtile.Image = null;

                // Clear controls from FlowLayoutPanels
                flowLayoutPanelTextures1.Controls.Clear();
                flowLayoutPanelTextures2.Controls.Clear();
                flowLayoutPanelAlphaImages.Controls.Clear();
            }
        }
        #endregion

        #region BlurFilter Class
        // Definition of BlurFilter class for Gaussian blur
        public class BlurFilter
        {
            private double[] kernel;
            private int kernelSize;

            public double Sigma { get; set; }

            public BlurFilter()
            {
                Sigma = 1.0;
                kernelSize = 1;
                kernel = GenerateGaussianKernel(Sigma, kernelSize);
            }

            #region Bitmap ProcessImage
            public Bitmap ProcessImage(Bitmap image)
            {
                Bitmap result = new Bitmap(image.Width, image.Height);

                int k = kernelSize / 2;
                int r, g, b;

                for (int x = k; x < image.Width - k; x++)
                {
                    for (int y = k; y < image.Height - k; y++)
                    {
                        r = g = b = 0;

                        for (int i = -k; i <= k; i++)
                        {
                            for (int j = -k; j <= k; j++)
                            {
                                Color c = image.GetPixel(x + i, y + j);
                                double w = kernel[i + k] * kernel[j + k];
                                r += (int)(c.R * w);
                                g += (int)(c.G * w);
                                b += (int)(c.B * w);
                            }
                        }

                        r = Math.Min(255, Math.Max(0, r));
                        g = Math.Min(255, Math.Max(0, g));
                        b = Math.Min(255, Math.Max(0, b));

                        result.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }

                return result;
            }
            #endregion

            #region double Gaussian
            private double Gaussian(double x, double sigma)
            {
                return Math.Exp(-(x * x) / (2 * sigma * sigma)) / (Math.Sqrt(2 * Math.PI) * sigma);
            }
            #endregion

            #region GenerateGaussianKernel
            private double[] GenerateGaussianKernel(double sigma, int size)
            {
                double[] kernel = new double[size];
                int center = size / 2;

                for (int i = 0; i < size; i++)
                {
                    kernel[i] = Gaussian(i - center, sigma);
                }

                // Normalize the kernel
                double sum = 0;
                foreach (double value in kernel)
                {
                    sum += value;
                }

                for (int i = 0; i < size; i++)
                {
                    kernel[i] /= sum;
                }

                return kernel;
            }
            #endregion
        }
        #endregion
    }
}

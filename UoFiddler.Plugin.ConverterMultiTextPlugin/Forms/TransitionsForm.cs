/***************************************************************************
 *
 * $Author: Prapilk
 * Built-in : Nikodemus
 * 
 * "THE WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Runtime.Intrinsics.X86;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class;
using System.Diagnostics;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class TransitionsForm : Form
    {
        // Declare textBoxValue as a member variable
        private XMLgenerator xmlGenerator = new XMLgenerator();
        private List<Image> textures1 = new List<Image>();
        private List<Image> textures2 = new List<Image>();
        private List<Image> alphaImages = new List<Image>();
        private List<string> alphaImageFileNames = new List<string>();
        private List<string> texture1FilePaths = new List<string>();
        private List<string> texture2FilePaths = new List<string>();
        private double gamma = 0.5; // Default value for gamma
        private double blurValue = 0.5; // Default value for blur
        private FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
        // Add these member variables to track the index of the currently displayed alpha image
        private int currentAlphaIndex = 0;
        private int totalAlphaImages = 0;
        private string nameTextureA;
        private string brushIdA;
        private string nameTextureB;
        private string brushIdB;

        public TransitionsForm()
        {
            InitializeComponent();

            // Set the SizeMode property to Zoom when the form is initialized
            pictureBoxPreview.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxLandtile.SizeMode = PictureBoxSizeMode.Zoom;
            totalAlphaImages = alphaImages.Count;
            xmlGenerator = new XMLgenerator();
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


        // Event handler for the "Back" button
        private void btnPrevious_Click(object sender, EventArgs e)
        {
            // Decrement the index of the currently displayed alpha image
            currentAlphaIndex--;

            // Check if index is less than zero, revert to last frame if so
            if (currentAlphaIndex < 0)
            {
                currentAlphaIndex = alphaImages.Count - 1;
            }

            // Update preview with matching alpha image
            UpdatePreview();
            UpdateAlphaNameLabel(); // Update alpha name label
            // Update the “counter” label
            UpdateCounterLabel();

        }

        // Event handler for the "Next" button
        private void btnNext_Click(object sender, EventArgs e)
        {
            // Increment index of currently displayed alpha image
            currentAlphaIndex++;

            // Check if index exceeds total images
            if (currentAlphaIndex >= alphaImages.Count)
            {
                currentAlphaIndex = 0; // Return to top of list
            }

            // Update preview with matching alpha image
            UpdatePreview();
            UpdateAlphaNameLabel(); // Update alpha name label
            // Update the “counter” label
            UpdateCounterLabel();
        }


        // Method to update the label with the name of the alpha corresponding to the current index
        private void UpdateAlphaNameLabel()
        {
            // Check if the index is valid
            if (currentAlphaIndex >= 0 && currentAlphaIndex < alphaImageFileNames.Count)
            {
                // Get alpha file name from list
                string alphaFileName = Path.GetFileName(alphaImageFileNames[currentAlphaIndex]);

                // Show alpha name in label
                lblAlphaName.Text = alphaFileName;
            }
        }

        #region UpdatePreview
        // Update preview with alpha image corresponding to current index
        private void UpdatePreview()
        {
            // Check if alpha images are available
            if (alphaImages.Count > 0)
            {
                // Get the alpha image corresponding to the current index
                Image alphaImage = alphaImages[currentAlphaIndex];

                // Generate the first transition image with the selected alpha image
                // Also use the other textures as you do now
                Image texture1 = textures1[0];
                Image texture2 = textures2[0];
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
                texture1FilePaths.Clear(); // Clear the list before adding new paths

                foreach (string imagePath in openFileDialog.FileNames)
                {
                    try
                    {
                        Image texture1 = Image.FromFile(imagePath);
                        textures1.Add(texture1);
                        texture1FilePaths.Add(imagePath); // Add the file path to the list
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
                texture2FilePaths.Clear(); // Clear the list before adding new paths

                foreach (string imagePath in openFileDialog.FileNames)
                {
                    try
                    {
                        Image texture2 = Image.FromFile(imagePath);
                        textures2.Add(texture2);
                        texture2FilePaths.Add(imagePath); // Add the file path to the list
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
                    alphaImageFileNames.Clear(); // Added to empty filename list

                    string[] alphaFiles = Directory.GetFiles(folderPath, "*.png");

                    foreach (string filePath in alphaFiles)
                    {
                        Image alphaImage = Image.FromFile(filePath);
                        alphaImages.Add(alphaImage);

                        // Add file name to list
                        alphaImageFileNames.Add(filePath);
                    }

                    // Reset the index of the currently displayed alpha image to zero
                    currentAlphaIndex = 0;

                    MessageBox.Show($"Loading successful : {alphaImages.Count} alpha images loaded.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading alpha images : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                UpdatePictureBoxes();
                UpdatePreview();
                UpdateAlphaNameLabel(); // Update alpha name label
                UpdateCounterLabel();   // Update counter
            }
        }
        #endregion

        // Method to update the "counter" label with the current number of the alpha image
        private void UpdateCounterLabel()
        {
            int currentNumber = currentAlphaIndex + 1; // Add 1 because indices start at 0
            int totalNumber = alphaImages.Count;
            Compteur.Text = $"{currentNumber}/{totalNumber}";
        }

        #region btnGenerateTransition

        private void btnGenerateTransition_Click(object sender, EventArgs e)
        {
            if (textures1.Count == 0 || textures2.Count == 0 || alphaImages.Count == 0)
            {
                MessageBox.Show("Please select textures and alpha images before generating the transitions.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get the path to the program directory
            string programDirectory = Application.StartupPath;

            // Define the path to the temporary directory in the program directory
            string defaultDirectory = Path.Combine(programDirectory, "tempGrafic");

            // Use the directory from tbDir if it's not empty, otherwise use the default directory
            string directory = string.IsNullOrEmpty(tbDir.Text) ? defaultDirectory : tbDir.Text;

            // Check if the directory exists
            if (!Directory.Exists(directory))
            {
                // Create the directory
                Directory.CreateDirectory(directory);
            }

            // Use the directory as the output path
            string outputPath = directory;

            int alphaIndex = 0;

            // Convert initial ID to integer
            int initialID = Convert.ToInt32(XMLgenerator.InitialLandTypeId, 16);

            foreach (Image alphaImage in alphaImages)
            {
                Image texture1 = textures1[alphaIndex % textures1.Count];
                Image texture2 = textures2[alphaIndex % textures2.Count];
                Bitmap transitionImage = GenerateTransition(texture1, texture2, alphaImage);

                // Save images without rotation
                string transitionFileName = Path.Combine(outputPath, $"0x{initialID.ToString("X")}.bmp");
                transitionImage.Save(transitionFileName, ImageFormat.Bmp);

                // Preview the transition
                Image previewImage = RotateAndResizeImageForPreview(transitionImage, 45);

                // Save images with 45 degree rotation and resized
                string rotatedTransitionFileName = Path.Combine(outputPath, $"RotatedTransition_{alphaIndex + 1}.bmp");
                previewImage.Save(rotatedTransitionFileName, ImageFormat.Bmp);

                // Increment initial ID for next transition
                initialID++;

                // Increment alpha image index
                alphaIndex++;
            }

            // Create an instance of the XMLgenerator class
            XMLgenerator xmlGenerator = new XMLgenerator();

            // Set the InitialLandTypeId property to tbStartHexDec.Text
            XMLgenerator.InitialLandTypeId = tbStartHexDec.Text;

            // Retrieve TextBox values
            string nameTextureA = textBoxNameTextureA.Text;
            string nameTextureB = textBoxNameTextureB.Text;
            string brushIdA = textBoxBrushNumberA.Text;
            string brushIdB = textBoxBrushNumberB.Text;

            // Call GenerateXML with all necessary values
            xmlGenerator.GenerateXML(texture1FilePaths, texture2FilePaths, alphaImageFileNames, outputPath, nameTextureA, nameTextureB, brushIdA, brushIdB);

            MessageBox.Show("Generation completed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Reset lists and controls
            textures1.Clear();
            textures2.Clear();
            alphaImages.Clear();

            pictureBoxTexture1.Image = null;
            pictureBoxTexture2.Image = null;
            pictureBoxAlpha1.Image = null;
            pictureBoxPreview.Image = null;
            pictureBoxLandtile.Image = null;
            tbStartHexDec.Text = null;

            flowLayoutPanelTextures1.Controls.Clear();
            flowLayoutPanelTextures2.Controls.Clear();
            flowLayoutPanelAlphaImages.Controls.Clear();
        }

        #endregion

        private void StartHexDec_TextChanged(object sender, EventArgs e)
        {
            string newText = tbStartHexDec.Text.Trim(); // Get the text from TextBox1 by removing whitespace at the beginning and end
            if (!string.IsNullOrEmpty(newText))
            {
                // Update the ID of the Land types rows in the XML by accessing the InitialLandTypeId property of the XMLgenerator instance
                XMLgenerator.InitialLandTypeId = newText;
            }
        }

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

        #region textBoxNameTextureA_TextChanged
        private void textBoxNameTextureA_TextChanged(object sender, EventArgs e)
        {
            // Get the text from the TextBox and assign it to nameTextureA
            nameTextureA = textBoxNameTextureA.Text;
        }
        #endregion

        #region textBoxNameTextureB_TextChanged
        private void textBoxNameTextureB_TextChanged(object sender, EventArgs e)
        {
            // Get the text from the TextBox and assign it to nameTextureB
            nameTextureB = textBoxNameTextureB.Text;
        }
        #endregion

        #region textBoxBrushNumberA_TextChanged
        private void textBoxBrushNumberA_TextChanged(object sender, EventArgs e)
        {
            // Get the text from the TextBox and assign it to brushIdA
            brushIdA = textBoxBrushNumberA.Text;
        }
        #endregion

        #region textBoxBrushNumberB_TextChanged
        private void textBoxBrushNumberB_TextChanged(object sender, EventArgs e)
        {
            // Get the text from the TextBox and assign it to brushIdB
            brushIdB = textBoxBrushNumberB.Text;
        }
        #endregion

        #region checkBoxZoom_CheckedChanged
        private void checkBoxZoom_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxZoom.Checked)
            {
                // Set the SizeMode property to Zoom if the checkbox is checked
                pictureBoxPreview.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBoxLandtile.SizeMode = PictureBoxSizeMode.Zoom;
            }
            else
            {
                // Set the SizeMode property to Normal if the checkbox is unchecked
                pictureBoxPreview.SizeMode = PictureBoxSizeMode.CenterImage;
                pictureBoxLandtile.SizeMode = PictureBoxSizeMode.CenterImage;
            }
        }
        #endregion

        #region buttonOpenTempGrafic
        private void buttonOpenTempGrafic_Click(object sender, EventArgs e)
        {
            // Get the path to the program directory
            string programDirectory = Application.StartupPath;

            // Define the path to the temporary directory in the program directory
            string defaultDirectory = Path.Combine(programDirectory, "tempGrafic");

            // Use the directory from tbDir if it's not empty, otherwise use the default directory
            string directory = string.IsNullOrEmpty(tbDir.Text) ? defaultDirectory : tbDir.Text;

            // Check if the directory exists
            if (Directory.Exists(directory))
            {
                // Open the directory in the file explorer
                Process.Start("explorer.exe", directory);
            }
            else
            {
                // Display a message to the user indicating that the directory does not exist
                MessageBox.Show($"The directory {directory} does not exist.");
            }
        }
        #endregion

        #region btDir_Click
        private void btDir_Click(object sender, EventArgs e)
        {
            // Create a new instance of FolderBrowserDialog
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            // Display the dialog and verify that the user clicked OK
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                // Set the text of tbDir to the selected path
                tbDir.Text = folderBrowserDialog.SelectedPath;
            }
        }
        #endregion
    }
}

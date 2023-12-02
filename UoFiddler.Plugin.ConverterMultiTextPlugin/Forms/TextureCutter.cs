using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Media;
using Microsoft.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;


namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class TextureCutter : Form
    {
        private Point originalImageLocation = new Point(0, 0); //Determines the position in the picture box
        private Bitmap resizedImage;
        private bool isPickingColor = false;

        // Save OrginalImage
        private Bitmap originalImage;
        // Save Orginal Size
        private Size originalSize;
        // Define a variable to store the custom colors
        private int[] customColors;
        // Draw
        private bool isDrawing = false; // Variable to track drawing mode
        private Point lastPoint;
        // Delete
        private bool isErasing = false;
        //PictureBox1
        private Point startPoint;
        private Rectangle cropArea;
        private bool isDragging = false;
        private bool showGrid2 = false;
        private Rectangle selectedRectangle; // The selected range.
        private List<Point> points = new List<Point>();

        public TextureCutter()
        {
            InitializeComponent();

            labelImageSize.Text = "";

            checkBoxChange.CheckedChanged += checkBoxChange_CheckedChanged;

            // Set the KeyPreview property of the form to true.
            this.KeyPreview = true;
            // Add a keyboard event handler to the form.
            this.KeyDown += Form1_KeyDown;
            //zoom
            pictureBox1.MouseWheel += pictureBox1_MouseWheel;
            // Set the initial value of the TrackBar control to the center
            trackBarTolerance.Value = trackBarTolerance.Maximum / 2;
            // Update the label with the current value of the TrackBar control
            labelTolerance.Text = trackBarTolerance.Value.ToString();
            // Update the label when the application starts.
            UpdateSharpnessLabel();

            // Set the initial value of the TrackBar
            TrackbarWhiteBalance.Value = 1;  // or whatever value you want

            // Display the initial value in the Label
            LabelWhiteBalance.Text = $"change: {TrackbarWhiteBalance.Value}%";
        }
        private void buttonLoadImage_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Filter = "image files|*.bmp;*.png;*.jpeg;*.jpg;*.tiff;*.gif|All files|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string selectedImagePath = openFileDialog1.FileName;

                // Set the image of the PictureBox to null and remove it from the Controls collection of the Panel.
                pictureBox1.Image = null;
                panel1.Controls.Remove(pictureBox1);
                pictureBox1.Dispose();

                // Create a new PictureBox and add it to the Panel.
                pictureBox1 = new PictureBox();
                pictureBox1.Location = originalImageLocation;
                pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
                pictureBox1.Image = Image.FromFile(selectedImagePath);
                panel1.Controls.Add(pictureBox1);

                // Update originalImage to point to the new image
                originalImage = new Bitmap(pictureBox1.Image);

                // Save the original size of the image
                originalSize = pictureBox1.Image.Size;

                // Resetting the scroll position.
                panel1.AutoScrollPosition = new Point(0, 0);

                // Displaying the image size in the label.
                Size imageSize = pictureBox1.Image.Size;
                labelImageSize.Text = $"Image size: {imageSize.Width} x {imageSize.Height} Pixel";

                // Make sure that the ContextMenuStrip of pictureBox1 is assigned.
                if (pictureBox1.ContextMenuStrip == null)
                {
                    pictureBox1.ContextMenuStrip = contextMenuStrip1;
                }
            }
        }
        private void buttonTextureCutter_Click(object sender, EventArgs e)
        {
            // Check if an image has been loaded into the picture box
            if (pictureBox1.Image == null)
            {
                // Display a message to the user and exit the method
                MessageBox.Show("No image was loaded.");
                return;
            }

            // Create a temporary directory to store the output images
            string directory = "tempGrafic";
            Directory.CreateDirectory(directory);

            int counter = 1;
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            // Define a dictionary of tile sizes and their corresponding checkbox names
            Dictionary<string, Size> tileSizes = new Dictionary<string, Size>
            {
                { "checkBox33x33", new Size(33, 33) },
                { "checkBox44x44", new Size(44, 44) },
                { "checkBox64x64", new Size(64, 64) },
                { "checkBox128x128", new Size(128, 128) },
                { "checkBox256x256", new Size(256, 256) }
            };

            // Iterate through the dictionary
            foreach (var kvp in tileSizes)
            {
                // Find the corresponding checkbox control
                CheckBox checkBox = Controls.Find(kvp.Key, true).FirstOrDefault() as CheckBox;

                // Check if the checkbox is not null and is checked
                if (checkBox != null && checkBox.Checked)
                {
                    // Get the tile size from the dictionary value
                    Size tileSize = kvp.Value;

                    // Calculate the number of tiles needed to cover the image in both the x and y directions
                    int tileCountX = (int)Math.Ceiling((double)pictureBox1.Image.Width / tileSize.Width);
                    int tileCountY = (int)Math.Ceiling((double)pictureBox1.Image.Height / tileSize.Height);

                    // Use nested loops to iterate through each tile position
                    for (int y = 0; y < tileCountY; y++)
                    {
                        for (int x = 0; x < tileCountX; x++)
                        {
                            // Define the output file name and path
                            string outputFileName = $"TextureImage{counter:D2}_{timestamp}.bmp";
                            string outputPath = Path.Combine(directory, outputFileName);

                            // Call the CropImage method to crop the image and save it to the temporary directory
                            CropImage(pictureBox1.Image, tileSize.Width, tileSize.Height, outputPath, x * tileSize.Width, y * tileSize.Height);
                            counter++;
                        }
                    }
                }
            }

            int customTileSizeW, customTileSizeH;

            // Check if custom tile sizes have been entered by the user
            if (int.TryParse(textBoxSizeW.Text, out customTileSizeW) && int.TryParse(textBoxSizeH.Text, out customTileSizeH))
            {
                // Ensure that the custom tile size does not exceed the size of the image
                customTileSizeW = Math.Min(customTileSizeW, pictureBox1.Image.Width);
                customTileSizeH = Math.Min(customTileSizeH, pictureBox1.Image.Height);

                // Find the checkBox1 control
                CheckBox checkBox1 = Controls.Find("checkBox1", true).FirstOrDefault() as CheckBox;

                // Check if checkBox1 is not checked
                if (checkBox1 == null || !checkBox1.Checked)
                {
                    // Define a list of disallowed custom tile sizes
                    List<Size> disallowedCustomTileSizes = new List<Size>
            {
                new Size(1, 1),
                new Size(1, 2),
                new Size(2, 1),
                new Size(2, 2),
                new Size(3, 3),
                new Size(3, 4),
                new Size(4, 4),
                new Size(5, 5)
            };

                    // Check if the custom tile size is in the list of disallowed custom tile sizes
                    if (disallowedCustomTileSizes.Contains(new Size(customTileSizeW, customTileSizeH)))
                    {
                        // Display a message to the user and exit the method
                        MessageBox.Show("This tile size is only allowed at your own risk by enabling checkBox.");
                        return;
                    }
                }

                // Calculate the number of tiles needed to cover the image in both the x and y directions based on the custom tile size
                int customTileCountX = (int)Math.Ceiling((double)pictureBox1.Image.Width / customTileSizeW);
                int customTileCountY = (int)Math.Ceiling((double)pictureBox1.Image.Height / customTileSizeH);

                // Use nested loops to iterate through each tile position
                for (int y = 0; y < customTileCountY; y++)
                {
                    for (int x = 0; x < customTileCountX; x++)
                    {
                        // Define the output file name and path
                        string outputFileName = $"TextureImageCustom{counter:D2}_{timestamp}.bmp";
                        string outputPath = Path.Combine(directory, outputFileName);

                        // Call the CropImage method to crop the image and save it to the temporary directory
                        CropImage(pictureBox1.Image, customTileSizeW, customTileSizeH, outputPath, x * customTileSizeW, y * customTileSizeH);
                        counter++;
                    }
                }
            }

            // Display a message to the user indicating that the graphic was successfully cropped and saved in the temporary directory.
            MessageBox.Show("The graphic has been successfully cropped and saved in the temporary directory.");
        }

        private void CropImage(Image image, int width, int height, string outputPath, int x, int y)
        {
            using (Bitmap bitmap = new Bitmap(width, height))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.DrawImage(image, new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height), GraphicsUnit.Pixel);
                }

                bitmap.Save(outputPath, ImageFormat.Bmp);
            }
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

        #region Relesution
        private void ButtonShrinkTexture_Click(object sender, EventArgs e)
        {
            // Check if an image is loaded in the PictureBox.
            if (pictureBox1.Image == null)
            {
                // Display a message and exit the method.
                MessageBox.Show("No image was loaded..");
                return;
            }

            // Dictionary for the sizes and their corresponding checkbox names.
            Dictionary<string, Size> tileSizes = new Dictionary<string, Size>
            {
                { "checkBox33x33", new Size(33, 33) },
                { "checkBox44x44", new Size(44, 44) },
                { "checkBox64x64", new Size(64, 64) },
                { "checkBox128x128", new Size(128, 128) },
                { "checkBox256x256", new Size(256, 256) }
            };

            // Iterating through the dictionary
            foreach (var kvp in tileSizes)
            {
                // Finding the CheckBox control based on the key
                CheckBox checkBox = Controls.Find(kvp.Key, true).FirstOrDefault() as CheckBox;

                // Check if the CheckBox is selected
                if (checkBox != null && checkBox.Checked)
                {
                    // Retrieve target size from the dictionary value.
                    Size targetSize = kvp.Value;

                    // Copy the original graphic.
                    Bitmap originalImage = new Bitmap(pictureBox1.Image);

                    try
                    {
                        // Improve the image using a super-resolution algorithm.
                        Image improvedImage = ImproveImageUsingSuperResolution(originalImage);

                        // Convert the improved image to a bitmap format.
                        Bitmap improvedBitmap = improvedImage as Bitmap;

                        // Resize the improved image to the target size.
                        Bitmap resizedImage = new Bitmap(improvedBitmap, targetSize.Width, targetSize.Height);

                        // Remove the current image from the PictureBox and set the improved image.
                        pictureBox1.Image = null;
                        pictureBox1.Image = resizedImage;

                        // Update the display.
                        Size imageSize = resizedImage.Size;
                        labelImageSize.Text = $"Image size.: {imageSize.Width} x {imageSize.Height} Pixel";
                    }
                    catch (Exception ex)
                    {
                        // An error has occurred - display an error message and proceed to the next step.
                        MessageBox.Show($"Error occurred while improving the image: {ex.Message}");
                    }
                }
            }
        }

        private Image ImproveImageUsingSuperResolution(Image image)
        {
            try
            {
                // Set the new width and height of the image.
                int newWidth = image.Width * 2;
                int newHeight = image.Height * 2;

                // Create a new bitmap with the new size.
                Bitmap result = new Bitmap(newWidth, newHeight);

                // Draw the original image onto the new bitmap with bicubic interpolation.
                using (Graphics graphics = Graphics.FromImage(result))
                {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bicubic;
                    graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                }

                // Return the improved image.
                return result;
            }
            catch (Exception ex)
            {
                // Log the error message and return the original image.
                Console.WriteLine($"Error occurred while improving the image: {ex.Message}");
                return image;
            }
        }
        #endregion

        #region Sharp
        private Bitmap SharpenImage(Bitmap image, double sharpenValue)
        {
            // Make a copy of the image.
            Bitmap sharpenedImage = (Bitmap)image.Clone();

            // Create a sharpening matrix based on the sharpness value.
            double[,] sharpenMatrix = new double[,]
            {
        { -sharpenValue, -sharpenValue, -sharpenValue },
        { -sharpenValue, 1 + (8 * sharpenValue), -sharpenValue },
        { -sharpenValue, -sharpenValue, -sharpenValue }
            };

            // Apply the sharpening matrix to the image.
            for (int x = 1; x < image.Width - 1; x++)
            {
                for (int y = 1; y < image.Height - 1; y++)
                {
                    Color pixel = image.GetPixel(x, y);

                    // Skip the white and black pixel sharpening process.
                    if (pixel.ToArgb() == Color.White.ToArgb() || pixel.ToArgb() == Color.Black.ToArgb())
                    {
                        continue;
                    }

                    // Apply the sharpening process.
                    ApplySharpening(sharpenedImage, x, y, pixel, sharpenMatrix);
                }
            }

            // Return the sharpened image.
            return sharpenedImage;
        }
        private void ApplySharpening(Bitmap image, int x, int y, Color pixel, double[,] matrix)
        {
            double r = 0;
            double g = 0;
            double b = 0;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int offsetX = x + i - 1;
                    int offsetY = y + j - 1;

                    Color neighborPixel = image.GetPixel(offsetX, offsetY);

                    r += neighborPixel.R * matrix[i, j];
                    g += neighborPixel.G * matrix[i, j];
                    b += neighborPixel.B * matrix[i, j];
                }
            }

            r = Math.Max(0, Math.Min(255, r));
            g = Math.Max(0, Math.Min(255, g));
            b = Math.Max(0, Math.Min(255, b));

            Color newPixel = Color.FromArgb((int)r, (int)g, (int)b);
            image.SetPixel(x, y, newPixel);
        }

        private void buttonsharp_Click(object sender, EventArgs e)
        {
            // Check if an image is loaded in the PictureBox.
            if (pictureBox1.Image == null)
            {
                // Display a message and exit the method.
                MessageBox.Show("Es wurde kein Bild geladen.");
                return;
            }

            // Copy the original graphic.
            Bitmap originalImage = new Bitmap(pictureBox1.Image);

            try
            {
                // Initialize the sharpened image variable.
                Bitmap sharpenedImage;

                // Read the value of the slider.
                double sharpenValue = trackBarSharpness.Value / 100.0;

                // Sharpen the image based on the value of the slider.
                sharpenedImage = SharpenImage(originalImage, sharpenValue);

                // Remove the current image from the PictureBox and insert the sharpened image.
                pictureBox1.Image = null;
                pictureBox1.Image = sharpenedImage;

                // Refresh the display.
                Size imageSize = sharpenedImage.Size;
                labelImageSize.Text = $"Image size: {imageSize.Width} x {imageSize.Height} Pixel";
            }
            catch (Exception ex)
            {
                // An error occurred - display an error message and go to the next step.
                MessageBox.Show($"An error occurred while sharpening the image.: {ex.Message}");
            }
        }
        private void UpdateSharpnessLabel()
        {
            // Update the label with the current value of the TrackBar.
            labelSharpnessValue.Text = $"Level of spiciness: {trackBarSharpness.Value}%";
        }

        private void trackBarSharpness_Scroll(object sender, EventArgs e)
        {
            // Update the label when the user moves the slider.
            UpdateSharpnessLabel();
        }
        #endregion
        private void buttonSaveImage_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image has been loaded..");
                return;
            }

            // Create the filename with the current time.
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = $"TextureSingleImage_{timestamp}.bmp";

            string directory = Path.Combine(Application.StartupPath, "tempGrafic");
            Directory.CreateDirectory(directory);
            string filePath = Path.Combine(directory, fileName);

            // Save the image to the specified path
            pictureBox1.Image.Save(filePath, ImageFormat.Bmp);

            MessageBox.Show("The graphic was saved successfully.");
        }
        private Bitmap ResizeImageWithFixedBorder(Bitmap image, int borderWidth)
        {
            // Calculate the new width and height of the inner part of the image
            int newInnerWidth = (image.Width - borderWidth * 2) * 2;
            int newInnerHeight = (image.Height - borderWidth * 2) * 2;

            // Calculate the new overall width and height of the image
            int newWidth = newInnerWidth + borderWidth * 2;
            int newHeight = newInnerHeight + borderWidth * 2;

            // Create a new bitmap with the new size
            Bitmap result = new Bitmap(newWidth, newHeight);

            // Draw the border of the original image onto the new bitmap
            using (Graphics graphics = Graphics.FromImage(result))
            {
                // Draw the top border
                graphics.DrawImage(image, 0, 0, new Rectangle(0, 0, image.Width, borderWidth), GraphicsUnit.Pixel);

                // Draw the bottom edge
                graphics.DrawImage(image, 0, newHeight - borderWidth, new Rectangle(0, image.Height - borderWidth, image.Width, borderWidth), GraphicsUnit.Pixel);

                // Draw the left border
                graphics.DrawImage(image, 0, borderWidth, new Rectangle(0, borderWidth, borderWidth, image.Height - borderWidth * 2), GraphicsUnit.Pixel);

                // Draw the right edge
                graphics.DrawImage(image, newWidth - borderWidth, borderWidth, new Rectangle(image.Width - borderWidth, borderWidth, borderWidth, image.Height - borderWidth * 2), GraphicsUnit.Pixel);
            }

            // Cut out the inner part of the picture
            Rectangle innerRect = new Rectangle(borderWidth, borderWidth, image.Width - borderWidth * 2, image.Height - borderWidth * 2);
            Bitmap innerImage = image.Clone(innerRect, image.PixelFormat);

            // Resize the inner part of the image with Bicubic Interpolation
            Bitmap resizedInnerImage = new Bitmap(newInnerWidth, newInnerHeight);
            using (Graphics graphics = Graphics.FromImage(resizedInnerImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bicubic;
                graphics.DrawImage(innerImage, 0, 0, newInnerWidth, newInnerHeight);
            }

            // Draw the enlarged inner part of the image onto the new bitmap
            using (Graphics graphics = Graphics.FromImage(result))
            {
                graphics.DrawImage(resizedInnerImage, borderWidth, borderWidth);
            }

            // Return the resulting image
            return result;
        }
        #region Optimize
        private void buttonOptimize_Click(object sender, EventArgs e)
        {
            // Save the current image as the new original image
            originalImage = new Bitmap(pictureBox1.Image);

            // Set the TrackBar control back to the center
            trackBarTolerance.Value = trackBarTolerance.Maximum / 2;

            // Update the label with the current value of the TrackBar control
            labelTolerance.Text = trackBarTolerance.Value.ToString();

            // Set focus to trackBarTolerance
            trackBarTolerance.Focus();
        }

        private void trackBarTolerance_Scroll(object sender, EventArgs e)
        {
            // Update the label with the current value of the TrackBar control
            labelTolerance.Text = trackBarTolerance.Value.ToString();

            // Check if an image is loaded in the PictureBox
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image was loaded.");
                return;
            }

            // Optimize the image using the tolerance from the TrackBar
            Bitmap optimizedImage = OptimizeImage(originalImage, trackBarTolerance.Value);

            // If the RGB checkbox is checked, optimize the colors
            if (checkBoxRGB.Checked)
            {
                optimizedImage = OptimizeColors(optimizedImage, trackBarTolerance.Value);
            }

            // Remove the current image in the PictureBox and put the optimized image
            pictureBox1.Image = null;
            pictureBox1.Image = optimizedImage;
            // textBoxTrackBarTolerance.Text = trackBarTolerance.Value.ToString();
        }
        private Bitmap OptimizeImage(Bitmap originalImage, int tolerance)
        {
            // Make a copy of the original image
            Bitmap optimizedImage = new Bitmap(originalImage);

            // Set values ​​for brightness, contrast and gamma
            float brightness = 1.0f; // no change in brightness
            float contrast = 1.0f; // no contrast
            float gamma = 1.0f; // no change in gamma

            // Adjust brightness, contrast and gamma based on tolerance
            brightness += (tolerance - 5) / 100.0f;
            contrast += (tolerance - 5) / 100.0f;
            gamma += (tolerance - 5) / 100.0f;

            float adjustedBrightness = brightness - 1.0f;

            // Create a matrix that brightens the image and increases contrast
            float[][] ptsArray =
            {
        new float[] {contrast, 0, 0, 0, adjustedBrightness}, // Scale and lighten red
        new float[] {0, contrast, 0, 0, adjustedBrightness}, // Scale and lighten green
        new float[] {0, 0, contrast, 0, adjustedBrightness}, // Scale and lighten blue
        new float[] {0, 0, 0, gamma, 0}, // Adjust gamma
        new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, brightness} // Adjust brightness
    };

            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default,
                ColorAdjustType.Bitmap);

            using (Graphics g = Graphics.FromImage(optimizedImage))
            {
                for (int y = 0; y < optimizedImage.Height; y++)
                {
                    for (int x = 0; x < optimizedImage.Width; x++)
                    {
                        Color pixelColor = originalImage.GetPixel(x, y);

                        // Check that the color is not #ffffff or #000000
                        if (pixelColor.ToArgb() != Color.White.ToArgb() && pixelColor.ToArgb() != Color.Black.ToArgb())
                        {
                            // Draw the optimized pixel on the optimized image
                            g.DrawImage(originalImage,
                                new Rectangle(x, y, 1, 1),
                                x,
                                y,
                                1,
                                1,
                                GraphicsUnit.Pixel,
                                imageAttributes);
                        }
                    }
                }
            }

            return optimizedImage;
        }
        private Bitmap OptimizeColors(Bitmap originalImage, int tolerance)
        {
            // Make a copy of the original image
            Bitmap optimizedImage = new Bitmap(originalImage);

            // Adjust color based on tolerance
            float colorAdjustment = (tolerance - 5) / 100.0f;

            using (Graphics g1 = Graphics.FromImage(optimizedImage))
            {
                for (int y = 0; y < optimizedImage.Height; y++)
                {
                    for (int x = 0; x < optimizedImage.Width; x++)
                    {
                        Color pixelColor = originalImage.GetPixel(x, y);

                        // Check that the color is not #ffffff or #000000
                        if (pixelColor.ToArgb() != Color.White.ToArgb() && pixelColor.ToArgb() != Color.Black.ToArgb())
                        {
                            // Adjust the RGB values of the pixel
                            int r = Math.Min(Math.Max((int)(pixelColor.R + colorAdjustment * 255), 0), 255);
                            int g = Math.Min(Math.Max((int)(pixelColor.G + colorAdjustment * 255), 0), 255);
                            int b = Math.Min(Math.Max((int)(pixelColor.B + colorAdjustment * 255), 0), 255);

                            Color optimizedColor = Color.FromArgb(r, g, b);

                            // Set the optimized pixel on the optimized image
                            optimizedImage.SetPixel(x, y, optimizedColor);
                        }
                    }
                }
            }

            return optimizedImage;
        }
        #endregion
        #region Rotate
        private void ButtonRotateTexture_Click(object sender, EventArgs e)
        {
            // Check if an image is loaded in the PictureBox
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image was loaded.");
                return;
            }

            // Enhance image automatically
            Image enhancedImage = RotateeImage(pictureBox1.Image);

            // Replace the current image in the PictureBox with the improved image
            pictureBox1.Image = enhancedImage;

            // update display
            Size imageSize = enhancedImage.Size;
            labelImageSize.Text = $"image size: {imageSize.Width} x {imageSize.Height} Pixel";
        }

        private Image RotateeImage(Image image)
        {
            // Example: Only the image is rotated here
            Bitmap enhancedImage = new Bitmap(image);
            enhancedImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
            return enhancedImage;
        }
        #endregion
        #region AutoTexture
        private void ButtonAutoTexture_Click(object sender, EventArgs e)
        {
            // Check if an image is loaded in the PictureBox
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image was loaded.");
                return;
            }

            // Enhance image automatically
            Image enhancedImage = AutoEnhanceImage(pictureBox1.Image);

            // Replace the current image in the PictureBox with the improved image
            pictureBox1.Image = enhancedImage;

            // update display
            Size imageSize = enhancedImage.Size;
            labelImageSize.Text = $"image size: {imageSize.Width} x {imageSize.Height} Pixel";

        }

        private Image AutoEnhanceImage(Image image)
        {
            // color enhancement
            Bitmap enhancedImage = AdjustColors(image);

            return enhancedImage;
        }

        private Bitmap AdjustColors(Image image)
        {
            Bitmap bitmap = new Bitmap(image);

            // Implement color enhancement algorithm here

            // Example: Increase color saturation by 20%
            float saturationFactor = 1.2f;
            ImageAttributes attributes = new ImageAttributes();
            ColorMatrix colorMatrix = new ColorMatrix(new float[][] {
        new float[] { saturationFactor, 0, 0, 0, 0 },
        new float[] { 0, saturationFactor, 0, 0, 0 },
        new float[] { 0, 0, saturationFactor, 0, 0 },
        new float[] { 0, 0, 0, 1, 0 },
        new float[] { 0, 0, 0, 0, 1 }
    });
            attributes.SetColorMatrix(colorMatrix);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.DrawImage(image, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, attributes);

            return bitmap;
        }
        #endregion
        #region WhiteBalace
        private void buttonWhite_Click(object sender, EventArgs e)
        {
            // Check if an image is loaded in the PictureBox
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image was loaded.");
                return;
            }

            // Enhance image automatically
            Bitmap image = new Bitmap(pictureBox1.Image);
            WhiteBalance(image);

            // Display the enhanced image in the PictureBox
            pictureBox1.Image = image;
            pictureBox1.Refresh();  // Refresh the PictureBox

            // update display
            Size imageSize = image.Size;
            labelImageSize.Text = $"Image Size: {imageSize.Width} x {imageSize.Height} Pixel";
        }

        private void WhiteBalance(Bitmap bitmap)
        {
            // White balance calculation code here

            // Example: white balance with dynamic values
            int red = CalculateMaxRedValue(bitmap); // valid value between 0 and 255
            int green = CalculateMaxGreenValue(bitmap); // valid value between 0 and 255
            int blue = CalculateMaxBlueValue(bitmap); // valid value between 0 and 255

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);

                    // Skip white and black pixels
                    if (pixelColor.R == 255 && pixelColor.G == 255 && pixelColor.B == 255)
                        continue; // Skip white pixels

                    if (pixelColor.R == 0 && pixelColor.G == 0 && pixelColor.B == 0)
                        continue; // Skip black pixels

                    // Calculate new RGB values ​​after white balance
                    int newRed, newGreen, newBlue;
                    if (checkboxDarken.Checked)
                    {
                        newRed = pixelColor.R - ((red - pixelColor.R) * TrackbarWhiteBalance.Value / 100);
                        newRed = Math.Max(newRed, 0);  // Ensure the value is not less than 0

                        newGreen = pixelColor.G - ((green - pixelColor.G) * TrackbarWhiteBalance.Value / 100);
                        newGreen = Math.Max(newGreen, 0);  // Ensure the value is not less than 0

                        newBlue = pixelColor.B - ((blue - pixelColor.B) * TrackbarWhiteBalance.Value / 100);
                        newBlue = Math.Max(newBlue, 0);  // Ensure the value is not less than 0
                    }
                    else
                    {
                        newRed = pixelColor.R + ((red - pixelColor.R) * TrackbarWhiteBalance.Value / 100);
                        newGreen = pixelColor.G + ((green - pixelColor.G) * TrackbarWhiteBalance.Value / 100);
                        newBlue = pixelColor.B + ((blue - pixelColor.B) * TrackbarWhiteBalance.Value / 100);
                    }

                    // Update pixels with the new RGB values
                    bitmap.SetPixel(x, y, Color.FromArgb(newRed, newGreen, newBlue));
                }
            }
        }

        // Method to calculate the max red value based on the properties of the image.
        private int CalculateMaxRedValue(Bitmap bitmap)
        {
            int max = 0;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    if (pixelColor.R > max)
                    {
                        max = pixelColor.R;
                    }
                }
            }

            return max;
        }

        // Method to calculate the max green value based on the properties of the image.
        private int CalculateMaxGreenValue(Bitmap bitmap)
        {
            int max = 0;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    if (pixelColor.G > max)
                    {
                        max = pixelColor.G;
                    }
                }
            }

            return max;
        }

        // Method to calculate the max blue value based on the properties of the image.
        private int CalculateMaxBlueValue(Bitmap bitmap)
        {
            int max = 0;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    if (pixelColor.B > max)
                    {
                        max = pixelColor.B;
                    }
                }
            }

            return max;
        }

        private void TrackbarWhiteBalance_ValueChanged(object sender, EventArgs e)
        {
            LabelWhiteBalance.Text = $"change: {TrackbarWhiteBalance.Value}%";
        }
        #endregion
        #region Rotate 45 Degress
        private void button45Degrees_Click(object sender, EventArgs e)
        {
            // Check if an image is loaded in the PictureBox
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image was loaded.");
                return;
            }

            // Resize the image to 33x33 pixels if it hasn't been resized yet
            if (resizedImage == null)
            {
                resizedImage = new Bitmap(pictureBox1.Image, new Size(33, 33));
            }

            // Rotate the resized image by -45 degrees
            Image rotatedImage = RotateImageByAngle(resizedImage, -45);

            // Create a new Bitmap with size 44x44 and black background
            Bitmap newBackground = new Bitmap(44, 44);
            using (Graphics graphics = Graphics.FromImage(newBackground))
            {
                graphics.Clear(Color.Black);

                // Calculate the coordinates to center the rotated image within the 44x44 frame
                int x = (newBackground.Width - rotatedImage.Width) / 2;
                int y = (newBackground.Height - rotatedImage.Height) / 2;

                // Draw the rotated image onto the new background graphic
                graphics.DrawImage(rotatedImage, x, y);
            }

            // Display the new image in the PictureBox
            pictureBox1.Image = newBackground;

            // Free the previous image (optional to free up memory)
            resizedImage.Dispose();

            // Reset the resized image as a new image has now been loaded
            resizedImage = null;

            // Refresh the display
            Size imageSize = newBackground.Size;
            labelImageSize.Text = $"image size: {imageSize.Width} x {imageSize.Height} Pixel";
        }
        private Image RotateImageByAngle(Image image, float angle)
        {
            // Calculate the size of the new Bitmap
            int bitmapSize = (int)Math.Ceiling(Math.Sqrt(image.Width * image.Width + image.Height * image.Height));

            // Create a new Bitmap with the calculated size
            Bitmap rotatedImage = new Bitmap(bitmapSize, bitmapSize);

            // Create a Graphics instance to draw the image
            using (Graphics graphics = Graphics.FromImage(rotatedImage))
            {
                // Set the rotation
                graphics.TranslateTransform(rotatedImage.Width / 2, rotatedImage.Height / 2);
                graphics.RotateTransform(angle);
                graphics.TranslateTransform(-rotatedImage.Width / 2, -rotatedImage.Height / 2);

                // Draw the image centered within the Bitmap
                graphics.DrawImage(image, (bitmapSize - image.Width) / 2, (bitmapSize - image.Height) / 2);
            }

            return rotatedImage;
        }
        #endregion
        #region Copy to Clipbord Image        

        private void copyClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                // Make a copy of the picture in pictureBox1
                Bitmap image = new Bitmap(pictureBox1.Image);

                // Verify that valid color values ​​were entered in the textBoxColorAdress and textBoxColorToAdress
                if (IsValidColor(textBoxColorAdress.Text) && IsValidColor(textBoxColorToAdress.Text))
                {
                    // Add the # to the color code if it's not already there
                    string fromColorCode = textBoxColorAdress.Text;
                    if (!fromColorCode.StartsWith("#"))
                    {
                        fromColorCode = "#" + fromColorCode;
                    }

                    string toColorCode = textBoxColorToAdress.Text;
                    if (!toColorCode.StartsWith("#"))
                    {
                        toColorCode = "#" + toColorCode;
                    }

                    // Convert the entered color values ​​into Color objects
                    Color fromColor = ColorTranslator.FromHtml(fromColorCode);
                    Color toColor = ColorTranslator.FromHtml(toColorCode);

                    // Replace the color value in the image
                    ReplaceColor(image, fromColor, toColor);
                }

                // Verify that a valid color value was entered in the textBoxColorErase
                if (IsValidColor(textBoxColorErase.Text))
                {
                    // Add the # to the color code if it's not already there
                    string eraseColorCode = textBoxColorErase.Text;
                    if (!eraseColorCode.StartsWith("#"))
                    {
                        eraseColorCode = "#" + eraseColorCode;
                    }

                    // Convert the entered color value to a Color object
                    Color eraseColor = ColorTranslator.FromHtml(eraseColorCode);

                    // Replace the color value in the image with white
                    ReplaceWithWhite(image, eraseColor);
                }

                // Copy the image to the clipboard
                Clipboard.SetImage(image);
            }
        }
        private void importClipbordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if there is an image on the clipboard
            if (Clipboard.ContainsImage())
            {
                // Paste the image from the clipboard into the PictureBox
                Image image = Clipboard.GetImage();
                pictureBox1.Image = image;

                // Adjust the size of the PictureBox to the size of the image
                pictureBox1.Size = image.Size;

                // Set the SizeMode property to Normal to maintain the original size of the image
                pictureBox1.SizeMode = PictureBoxSizeMode.Normal; // This line has been added

                // Save the original image and its size
                originalImage = new Bitmap(pictureBox1.Image);
                originalSize = pictureBox1.Image.Size;

                // Set the text of the label to the size of the image
                labelImageSize.Text = "Image Size: " + image.Width + " x " + image.Height;
            }
        }

        //Old function that sets to transparent not active
        private void MakeTransparent(Bitmap image, Color color)
        {
            // Loop through each pixel in the image
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    // Check if the pixel is the color you want to erase
                    if (image.GetPixel(x, y) == color)
                    {
                        // Set the pixel to transparent
                        image.SetPixel(x, y, Color.Transparent);
                    }
                }
            }
        }
        //replaces the color with the given one
        private void ReplaceColor(Bitmap image, Color fromColor, Color toColor)
        {
            // Loop through each pixel in the image
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    // Verify that the pixel is the color to be replaced
                    if (image.GetPixel(x, y) == fromColor)
                    {
                        // Set the pixel to the new color
                        image.SetPixel(x, y, toColor);
                    }
                }
            }
        }

        //Replaces the color with ffffff
        private void ReplaceWithWhite(Bitmap image, Color color)
        {
            // Loop through each pixel in the image
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    // Verify that the pixel is the color to be replaced
                    if (image.GetPixel(x, y) == color)
                    {
                        // Set the pixel to white
                        image.SetPixel(x, y, Color.White);
                    }
                }
            }
        }
        #endregion

        #region Textbox Color

        private void textBoxColorAdress_TextChanged(object sender, EventArgs e)
        {
            // Verify that the text you entered is a valid color value
            if (textBoxColorAdress.Text.Length == 7 && !IsValidColor(textBoxColorAdress.Text))
            {
                // The text you entered is not a valid color value - display an error message
                MessageBox.Show("Please enter a valid color value.");
            }
            else
            {
                // Check if the text in the TextBox is a valid color code
                try
                {
                    if (ColorTranslator.FromHtml(textBoxColorAdress.Text) is Color color)
                    {
                        // Set the background color of panelIsPickingColor to the color value
                        panelIsPickingColor.BackColor = color;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Please enter a valid color value.");
                }
            }
            // Wenn die Checkbox aktiviert ist, kopieren Sie den Text aus textBoxColorAdress in textBoxColorToAdress
            if (checkBoxToTextboxColor.Checked)
            {
                textBoxColorToAdress.Text = textBoxColorAdress.Text;
            }
        }

        private void checkBoxToTextboxColor_CheckedChanged(object sender, EventArgs e)
        {
            // Wenn die Checkbox aktiviert ist, kopieren Sie den Text aus textBoxColorAdress in textBoxColorToAdress
            if (checkBoxToTextboxColor.Checked)
            {
                textBoxColorToAdress.Text = textBoxColorAdress.Text;
            }
        }

        private void textBoxColorToAdress_TextChanged(object sender, EventArgs e)
        {
            // Verify that the text you entered is a valid color value
            if (textBoxColorToAdress.Text.Length == 7 && !IsValidColor(textBoxColorToAdress.Text))
            {
                // The text you entered is not a valid color value - display an error message
                MessageBox.Show("Please enter a valid color value.");
            }

            // Wenn die Checkbox aktiviert ist, kopieren Sie den Text aus textBoxColorAdress in textBoxColorToAdress
            if (checkBoxToTextboxColor.Checked)
            {
                textBoxColorToAdress.Text = textBoxColorAdress.Text;
            }
        }

        private void textBoxColorErase_TextChanged(object sender, EventArgs e)
        {
            // Verify that the text you entered is a valid color value
            if (textBoxColorErase.Text.Length == 7 && !IsValidColor(textBoxColorErase.Text))
            {
                // The text you entered is not a valid color value - display an error message
                MessageBox.Show("Please enter a valid color value.");
            }
        }
        private bool IsValidColor(string color)
        {
            // Check if the color value starts with a #
            if (color.StartsWith("#"))
            {
                // Remove the # from the beginning of the color value
                color = color.Substring(1);
            }

            // Verify that the color value is 6 characters long
            if (color.Length == 6)
            {
                // Verify that all characters are valid hexadecimal characters
                for (int i = 0; i < color.Length; i++)
                {
                    if (!IsHexChar(color[i]))
                    {
                        return false;
                    }
                }

                // The color value is valid
                return true;
            }

            // The color value is invalid
            return false;
        }
        private bool IsHexChar(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
        }
        #endregion

        #region Checkbox
        private void checkBoxChange_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the checkBoxChange is checked
            if (checkBoxChange.Checked)
            {
                // Transfer the value of comboBoxColorValue to textBoxColorToAdress
                textBoxColorToAdress.Text = comboBoxColorValue.Text;
            }
        }
        private void comboBoxColorValue_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check if the checkBoxChange is checked
            if (checkBoxChange.Checked)
            {
                // Transfer the value of comboBoxColorValue to textBoxColorToAdress
                textBoxColorToAdress.Text = comboBoxColorValue.Text;
            }

            // Check if the checkBoxChange is checked
            if (checkBoxDelete.Checked)
            {
                // Transfer the value of comboBoxColorValue to textBoxColorToAdress
                textBoxColorErase.Text = comboBoxColorValue.Text;
            }
        }
        #endregion

        #region To Update
        private void btToUpdate_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                // Create a copy of the image in pictureBox1.
                Bitmap image = new Bitmap(pictureBox1.Image);

                // Check if valid color values have been entered in both textBoxColorAdress and textBoxColorToAdress.
                if (IsValidColor(textBoxColorAdress.Text) && IsValidColor(textBoxColorToAdress.Text))
                {
                    // Add the "#" symbol to the color code if it's not already present.
                    string fromColorCode = textBoxColorAdress.Text;
                    if (!fromColorCode.StartsWith("#"))
                    {
                        fromColorCode = "#" + fromColorCode;
                    }

                    string toColorCode = textBoxColorToAdress.Text;
                    if (!toColorCode.StartsWith("#"))
                    {
                        toColorCode = "#" + toColorCode;
                    }

                    // Convert the entered color values into Color objects.
                    Color fromColor = ColorTranslator.FromHtml(fromColorCode);
                    Color toColor = ColorTranslator.FromHtml(toColorCode);


                    int tolerance = 0;
                    if (checkBoxtoleranz5.Checked) tolerance = 13; // 5% von 255
                    else if (checkBoxtoleranz10.Checked) tolerance = 26; // 10% von 255
                    else if (checkBoxtoleranz15.Checked) tolerance = 38; // 15% von 255
                    else if (checkBoxtoleranz25.Checked) tolerance = 64; // 25% von 255
                    else if (checkBoxtoleranz30.Checked) tolerance = 77; // 30% von 255
                    else if (checkBoxtoleranz35.Checked) tolerance = 89; // 35% von 255
                    else if (checkBoxtoleranz50.Checked) tolerance = 127; // 50% von 255
                    else if (checkBoxtoleranz75.Checked) tolerance = 191; // 75% von 255
                    else if (checkBoxtoleranz100.Checked) tolerance = 255; // 100% von 255
                    // Replace the color value in the image.
                    ReplaceColorWithTolerance(image, fromColor, toColor, tolerance);
                }

                // Check if a valid color value has been entered in the textBoxColorErase.
                if (IsValidColor(textBoxColorErase.Text))
                {
                    // Add the "#" symbol to the color code if it's not already present.
                    string eraseColorCode = textBoxColorErase.Text;
                    if (!eraseColorCode.StartsWith("#"))
                    {
                        eraseColorCode = "#" + eraseColorCode;
                    }

                    // Convert the entered color value into a Color object.
                    Color eraseColor = ColorTranslator.FromHtml(eraseColorCode);

                    // Replace the color value in the image with white.
                    ReplaceWithWhite(image, eraseColor);
                }

                // Update the image in pictureBox1.
                pictureBox1.Image = image;

                // Update the originalImage object
                originalImage = new Bitmap(image);
            }
        }
        #endregion

        #region Keydown
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if Ctrl+V was pressed.
            if (e.Control && e.KeyCode == Keys.X)
            {
                // Invoke the copyClipboardToolStripMenuItem_Click method.
                copyClipboardToolStripMenuItem_Click(sender, e);
            }
            if (e.Control && e.KeyCode == Keys.V)
            {
                // Invoke the importClipbordToolStripMenuItem_Click method.
                importClipbordToolStripMenuItem_Click(sender, e);
            }
        }
        #endregion

        #region Pipette
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            // Check if the dropper mode is activated.
            if (isPickingColor)
            {
                // Check if there is an image in pictureBox1.
                if (pictureBox1.Image != null)
                {
                    // Convert the mouse coordinates to image coordinates.
                    int x = e.X * pictureBox1.Image.Width / pictureBox1.Width;
                    int y = e.Y * pictureBox1.Image.Height / pictureBox1.Height;

                    // Create a copy of the image in pictureBox1.
                    Bitmap image = new Bitmap(pictureBox1.Image);

                    // Check if the coordinates are within the image bounds.
                    if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                    {
                        // Retrieve the color value of the pixel at the specified coordinates.
                        Color color = image.GetPixel(x, y);

                        // Convert the color value to a hexadecimal code.
                        string colorCode = "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");

                        // Set the color code in the textBoxColorAdress.
                        textBoxColorAdress.Text = colorCode;
                    }
                }

                // Disable eyedropper mode
                isPickingColor = false;
            }
        }

        private void btPickColor_Click(object sender, EventArgs e)
        {
            // Activate eyedropper mode
            isPickingColor = true;
        }
        #endregion
        #region Mirror
        private void BtMirroImage_Click(object sender, EventArgs e)
        {
            // Check if there is an image in pictureBox1.
            if (pictureBox1.Image != null)
            {
                // Create a copy of the image in pictureBox1.
                Bitmap image = new Bitmap(pictureBox1.Image);

                // Mirror the image horizontally.
                image.RotateFlip(RotateFlipType.RotateNoneFlipX);

                // Update the image in pictureBox1.
                pictureBox1.Image = image;
            }
        }
        #endregion

        #region Zoom
        private void ZoomImage(int change)
        {
            // Calculate the new size based on the current image size
            int newWidth = pictureBox1.Image.Width + change;
            int newHeight = pictureBox1.Image.Height + change;

            // If the new size is less than half of the original size or greater than the size of the PictureBox,
            // adjust it to be within these bounds
            newWidth = Math.Max(Math.Min(newWidth, panel1.Width), originalImage.Width / 2);
            newHeight = Math.Max(Math.Min(newHeight, panel1.Height), originalImage.Height / 2);

            // Calculate the zoom percentage
            double zoomPercentage = (double)newWidth / originalImage.Width * 100;

            // If the zoom percentage is close to 100%, restore the original image
            if (zoomPercentage >= 98.8 && zoomPercentage <= 102.7)
            {
                pictureBox1.Image = new Bitmap(originalImage);
                zoomLabel.Text = "Original size reached";
                return;
            }

            // Create a new Bitmap object with the new size
            Bitmap newImage = new Bitmap(originalImage, new Size(newWidth, newHeight));

            // Dispose of the old image to free up memory
            pictureBox1.Image.Dispose();

            // Assign the new image to the PictureBox
            pictureBox1.Image = newImage;

            // Update the zoom label
            zoomLabel.Text = $"Zoom: {Math.Round(zoomPercentage, 1)}%";
        }
        private void zoomInButton_Click(object sender, EventArgs e)
        {
            // Check that the Image object is not null
            if (pictureBox1.Image != null)
            {
                // Enlarge
                ZoomImage(10);
            }
        }
        private void zoomOutButton_Click(object sender, EventArgs e)
        {
            // Check that the Image object is not null and the new size is greater than half of the original size
            if (pictureBox1.Image != null && pictureBox1.Image.Width > originalImage.Width / 2 && pictureBox1.Image.Height > originalImage.Height / 2)
            {
                // Shrink
                ZoomImage(-10);
            }
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            // Check whether the mouse wheel has been rotated up or down
            /*if (e.Delta > 0)
            {
                // Check that the Image object is not null
                if (pictureBox1.Image != null)
                {
                    // Enlarge
                    ZoomImage(10);
                }
            }
            else
            {
                // Check that the Image object is not null and the new size is greater than half of the original size
                if (pictureBox1.Image != null && pictureBox1.Image.Width > originalImage.Width / 2 && pictureBox1.Image.Height > originalImage.Height / 2)
                {
                    // Shrink
                    ZoomImage(-10);
                }
            }*/
        }
        private void resetButton_Click(object sender, EventArgs e)
        {
            // Resize the image in the PictureBox to its original size
            pictureBox1.Image = new Bitmap(originalImage, originalSize);
        }
        #endregion

        #region Color Dialog
        private void selectColorButton_Click(object sender, EventArgs e)
        {
            // Create a new ColorDialog object
            ColorDialog colorDialog = new ColorDialog();

            // Restore the custom colors, if any
            if (customColors != null)
            {
                colorDialog.CustomColors = customColors;
            }

            // Display the ColorDialog and verify that the user clicked OK
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                // Convert the selected color to a hexadecimal code
                string colorCode = "#" + colorDialog.Color.R.ToString("X2") + colorDialog.Color.G.ToString("X2") + colorDialog.Color.B.ToString("X2");

                // Put the color code in the textBoxColorToAdress
                textBoxColorToAdress.Text = colorCode;

                // Save the custom colors
                customColors = colorDialog.CustomColors;
            }
        }

        private void selectColorButton2_Click(object sender, EventArgs e)
        {
            // Create a new ColorDialog object
            ColorDialog colorDialog = new ColorDialog();

            // Restore the custom colors, if any
            if (customColors != null)
            {
                colorDialog.CustomColors = customColors;
            }

            // Display the ColorDialog and verify that the user clicked OK
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                // Convert the selected color to a hexadecimal code
                string colorCode = "#" + colorDialog.Color.R.ToString("X2") + colorDialog.Color.G.ToString("X2") + colorDialog.Color.B.ToString("X2");

                // Put the color code in the textBoxColorToAdress
                textBoxColorToAdress2.Text = colorCode;

                // Save the custom colors
                customColors = colorDialog.CustomColors;
            }
        }
        #endregion
        #region Coordinates of the mouse cursor.        

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                // Converting the mouse coordinates to image coordinates.
                // int x = e.X * pictureBox1.Image.Width / pictureBox1.Width;
                // int y = e.Y * pictureBox1.Image.Height / pictureBox1.Height;

                // Adjust the mouse coordinates for the zoom factor
                int zoomFactor = (int)Math.Pow(2, zoomCounter);
                int x = (e.X * pictureBox1.Image.Width / pictureBox1.Width) / zoomFactor;
                int y = (e.Y * pictureBox1.Image.Height / pictureBox1.Height) / zoomFactor;

                // Checking if the coordinates are within the image boundaries.
                if (x >= 0 && x < pictureBox1.Image.Width && y >= 0 && y < pictureBox1.Image.Height)
                {
                    // Creating a copy of the image in pictureBox1.
                    Bitmap image = new Bitmap(pictureBox1.Image);

                    // Adjust the coordinates for the zoom factor
                    int adjustedX = x * zoomFactor;
                    int adjustedY = y * zoomFactor;

                    // Retrieving the color value of the pixel at the adjusted coordinates.
                    Color color = image.GetPixel(adjustedX, adjustedY);

                    // Converting the color value to a hexadecimal code.
                    string colorCode = "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");

                    // Setting the color code in the label control.
                    colorLabel.Text = colorCode;

                    // Displaying the coordinates of the mouse cursor in the label.
                    coordinatesLabel.Text = $"X: {adjustedX}, Y: {adjustedY}";

                    // Set the background color of the color display panel to the retrieved color value.
                    panelColorHex.BackColor = color;
                }
                else
                {
                    // Mouse is outside of the image, no color display
                    colorLabel.Text = "";
                    coordinatesLabel.Text = "";
                    panelColorHex.BackColor = Color.Transparent;
                }
            }
            else
            {
                // PictureBox is empty, no color display
                colorLabel.Text = "";
                coordinatesLabel.Text = "";
            }

            // Check if erasing mode is enabled and if left mouse button is pressed
            if (isErasing && e.Button == MouseButtons.Left)
            {
                // Check if pictureBox1.Image is not null
                if (pictureBox1.Image != null)
                {
                    // Create a copy of the image in pictureBox1
                    Bitmap image = new Bitmap(pictureBox1.Image);

                    // Create a Graphics object from the image
                    using (Graphics g = Graphics.FromImage(image))
                    {
                        // Erase an area around the current mouse position
                        g.FillEllipse(Brushes.White, e.X - 5, e.Y - 5, 10, 10);
                    }

                    // Update the image in pictureBox1
                    pictureBox1.Image = image;
                }
            }
            else if (isDrawing && e.Button == MouseButtons.Left)
            {
                // Check if pictureBox1.Image is not null
                if (pictureBox1.Image != null)
                {
                    // Create a copy of the image in pictureBox1
                    Bitmap image = new Bitmap(pictureBox1.Image);

                    // Create a Graphics object from the image
                    using (Graphics g = Graphics.FromImage(image))
                    {
                        // Check if textBoxColorToAdress is not empty
                        if (!string.IsNullOrEmpty(textBoxColorToAdress.Text))
                        {
                            // Convert the text in textBoxColorToAdress into a color value
                            string colorCode = textBoxColorToAdress.Text;
                            if (!colorCode.StartsWith("#"))
                            {
                                colorCode = "#" + colorCode;
                            }
                            Color penColor = ColorTranslator.FromHtml(colorCode);

                            // Create a pen with the specified color
                            Pen pen = new Pen(penColor);

                            // Adjust the mouse coordinates for the zoom factor
                            int zoomFactor = (int)Math.Pow(2, zoomCounter);
                            Point adjustedLastPoint = new Point(lastPoint.X / zoomFactor, lastPoint.Y / zoomFactor);
                            Point adjustedCurrentPoint = new Point(e.X / zoomFactor, e.Y / zoomFactor);

                            // Draw a line from the adjusted last position to the adjusted current position
                            g.DrawLine(pen, adjustedLastPoint.X, adjustedLastPoint.Y, adjustedCurrentPoint.X, adjustedCurrentPoint.Y);

                            if (checkBox2Colors.Checked && !string.IsNullOrEmpty(textBoxColorToAdress2.Text))
                            {
                                string colorCode2 = textBoxColorToAdress2.Text;
                                if (!colorCode2.StartsWith("#")) { colorCode2 = "#" + colorCode2; }
                                Color penColor2;
                                try { penColor2 = ColorTranslator.FromHtml(colorCode2); }
                                catch { penColor2 = Color.Black; }  // Default to black if the color code is invalid

                                Pen pen2 = new Pen(penColor2);

                                // Adjust the coordinates for the second color
                                Point adjustedCurrentPoint2 = new Point((e.X + 1) / zoomFactor, e.Y / zoomFactor);

                                g.DrawRectangle(pen2, adjustedCurrentPoint2.X, adjustedCurrentPoint2.Y, 1, 1);
                            }

                        }
                    }

                    // Update the image in pictureBox1
                    pictureBox1.Image = image;

                    // Set the last point to the current position
                    lastPoint = e.Location;
                }
            }

            else if (checkBoxFreehand.Checked && e.Button == MouseButtons.Left)
            {
                points.Add(e.Location);
                pictureBox1.Invalidate();
            }
            else if (e.Button == MouseButtons.Left && isDragging)
            {
                int width = e.X - startPoint.X;
                int height = e.Y - startPoint.Y;
                cropArea = new Rectangle(startPoint.X, startPoint.Y, width, height);
                pictureBox1.Invalidate();
            }
            else if (e.Button == MouseButtons.None)
            {
                // Set the last point to the current position
                lastPoint = e.Location;
            }

            if (pictureBox1.Image != null && checkBoxLines.Checked && e.Button == MouseButtons.Left)
            {
                points.Add(e.Location);
                pictureBox1.Invalidate();
            }
        }
        #endregion

        #region indexed colors
        private void convertToIndexedButton_Click(object sender, EventArgs e)
        {
            // Check if there is an image on the clipboard
            if (Clipboard.ContainsImage())
            {
                // Paste the image from the clipboard into the PictureBox
                Bitmap source = new Bitmap(Clipboard.GetImage());
                pictureBox1.Image = source;

                // Convert the image to indexed colors
                Bitmap result = source.Clone(new Rectangle(0, 0, source.Width, source.Height), PixelFormat.Format8bppIndexed);

                // Update the image in the PictureBox
                pictureBox1.Image = result;
            }
        }

        private ToolTip toolTip = new ToolTip();
        private void drawButton_Click(object sender, EventArgs e)
        {
            if (isDrawing)
            {
                // Switch off drawing mode
                isDrawing = false;
                drawButton.FlatAppearance.MouseDownBackColor = Color.FromName("Control"); // Reset the button color
                toolTip.SetToolTip(drawButton, "Draw mode is deactivated");
            }
            else
            {
                // Switch to drawing mode
                isDrawing = true;

                // Disable delete mode
                isErasing = false;

                drawButton.FlatAppearance.MouseDownBackColor = Color.Red;
                toolTip.SetToolTip(drawButton, "Draw mode is activated");
            }
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // Check whether drawing mode is activated
            if (isDrawing)
            {
                // Set the last point to the current mouse position
                lastPoint = e.Location;
            }
        }
        private void eraseButton_Click(object sender, EventArgs e)
        {
            if (isErasing)
            {
                // Switch off erase mode
                isErasing = false;
                eraseButton.FlatAppearance.MouseDownBackColor = Color.FromName("Control"); // Reset the button color
                toolTip.SetToolTip(eraseButton, "Erase mode is deactivated");
            }
            else
            {
                // Enter erase mode
                isErasing = true;

                // Disable drawing mode
                isDrawing = false;

                eraseButton.FlatAppearance.MouseDownBackColor = Color.Red;
                toolTip.SetToolTip(eraseButton, "Erase mode is activated");
            }
        }
        #endregion

        #region ColorWithTolerance
        private void ReplaceColorWithTolerance(Bitmap image, Color fromColor, Color toColor, int tolerance)
        {
            // Loop through every pixel in the image
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    // Check whether the pixel is within the tolerance range of the color to be replaced
                    Color pixelColor = image.GetPixel(x, y);
                    if (IsWithinTolerance(pixelColor, fromColor, tolerance))
                    {
                        // Set the pixel to the new color
                        image.SetPixel(x, y, toColor);
                    }
                }
            }
        }

        private bool IsWithinTolerance(Color color1, Color color2, int tolerance)
        {
            int rDiff = Math.Abs(color1.R - color2.R);
            int gDiff = Math.Abs(color1.G - color2.G);
            int bDiff = Math.Abs(color1.B - color2.B);

            // Check whether the difference in each color channel is within tolerance
            return rDiff <= tolerance && gDiff <= tolerance && bDiff <= tolerance;
        }
        #endregion

        #region Color Evaluate
        private Form colorForm = null;
        private Label mostCommonColorLabel = null;

        private void btEvaluateColor_Click(object sender, EventArgs e)
        {
            if (colorForm != null && !colorForm.IsDisposed)
            {
                colorForm.Close();
            }

            if (pictureBox1.Image != null)
            {
                Bitmap image = new Bitmap(pictureBox1.Image);
                Dictionary<string, int> colors = new Dictionary<string, int>();
                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        Color color = image.GetPixel(x, y);
                        string colorCode = "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
                        if (colorCode == "#FFFFFF" || colorCode == "#000000") continue;
                        if (colors.ContainsKey(colorCode))
                        {
                            colors[colorCode]++;
                        }
                        else
                        {
                            colors.Add(colorCode, 1);
                        }
                    }
                }
                var sortedColors = from pair in colors orderby pair.Value descending select pair;
                int totalPixels = image.Width * image.Height;
                colorForm = new Form();
                colorForm.Text = "Evaluate Color";

                colorForm.ShowIcon = false;

                mostCommonColorLabel = new Label();
                mostCommonColorLabel.Dock = DockStyle.Top;

                Button refreshButton = new Button();
                refreshButton.Text = "Refresh";
                refreshButton.Dock = DockStyle.Top;
                refreshButton.Click += btEvaluateColor_Click;

                TreeView treeView = new TreeView();
                treeView.Dock = DockStyle.Fill;

                // Setzen Sie DrawMode auf OwnerDrawAll
                treeView.DrawMode = TreeViewDrawMode.OwnerDrawAll;

                treeView.DrawNode += (s, evt) =>
                {
                    evt.DrawDefault = false;
                    using (var brush = new SolidBrush(ColorTranslator.FromHtml(evt.Node.Text.Split(' ')[0])))
                    {
                        var rect = new Rectangle(evt.Bounds.Left + 2, evt.Bounds.Top + 2, evt.Bounds.Height - 4, evt.Bounds.Height - 4);
                        evt.Graphics.FillRectangle(brush, rect);
                    }

                    // Set DrawMode to OwnerDrawAll
                    Color textColor = Color.Black;

                    // With the node selected, change the background color
                    if ((evt.State & TreeNodeStates.Selected) != 0)
                    {
                        using (var brush = new SolidBrush(Color.FromArgb(128, Color.LightBlue))) // Change this to the highlight color you want
                        {
                            evt.Graphics.FillRectangle(brush, new Rectangle(0, evt.Node.Bounds.Top, treeView.Width, evt.Node.Bounds.Height));
                        }
                    }

                    TextRenderer.DrawText(evt.Graphics, evt.Node.Text, evt.Node.TreeView.Font,
                        new Rectangle(evt.Bounds.Left + evt.Bounds.Height, evt.Bounds.Top, evt.Bounds.Width - evt.Bounds.Height, evt.Bounds.Height),
                        textColor);
                };


                treeView.NodeMouseClick += (s, evt) =>
                {
                    string hexCode = evt.Node.Text.Split(' ')[0];
                    textBoxColorAdress.Text = hexCode;
                    Clipboard.SetText(hexCode);
                };

                int count = 0;
                foreach (KeyValuePair<string, int> pair in sortedColors)
                {
                    if (count >= 20) break;
                    double percentage = (double)pair.Value / totalPixels * 100;
                    string nodeText = pair.Key + " - " + percentage.ToString("F2") + "%";
                    TreeNode colorNode = new TreeNode(nodeText);
                    treeView.Nodes.Add(colorNode);

                    if (count == 0)
                    {
                        mostCommonColorLabel.Text = "Most common color: " + pair.Key;
                    }

                    count++;
                }

                colorForm.Controls.Add(treeView);
                colorForm.Controls.Add(refreshButton);
                colorForm.Controls.Add(mostCommonColorLabel);

                colorForm.Show();
            }
        }
        #endregion
        #region Color List
        // Global variable to store the form
        Form colorListForm = null;

        private void btcolorlistimage_Click(object sender, EventArgs e)
        {
            // Check if the form is already open
            if (colorListForm != null)
            {
                // The form is already open, so just return
                return;
            }

            // Check whether an image has been loaded in the PictureBox
            if (pictureBox1.Image != null)
            {
                // Create a new shape
                colorListForm = new Form();
                colorListForm.Text = "Color List";
                colorListForm.FormClosed += (s, e) => { colorListForm = null; };

                // Create a TreeView and add it to the shape
                TreeView treeView = new TreeView();
                treeView.Dock = DockStyle.Fill;
                colorListForm.Controls.Add(treeView);

                // Create a label to display the total number of colors
                Label label = new Label();
                label.Dock = DockStyle.Top;
                colorListForm.Controls.Add(label);

                // Function to load the colors from the image
                Action loadColors = () =>
                {
                    // Delete all existing nodes in the TreeView
                    treeView.Nodes.Clear();

                    // Make a copy of the image in pictureBox1
                    Bitmap image = new Bitmap(pictureBox1.Image);

                    // Create a HashSet to store the unique colors in the image
                    HashSet<string> uniqueColors = new HashSet<string>();

                    // Create a list to store the colors and their brightness values
                    List<Tuple<string, float>> colors = new List<Tuple<string, float>>();

                    // Loop through every pixel in the image
                    for (int x = 0; x < image.Width; x++)
                    {
                        for (int y = 0; y < image.Height; y++)
                        {
                            // Get the color value of the pixel at the specified coordinates
                            Color color = image.GetPixel(x, y);

                            // Convert the color value to a hexadecimal code
                            string colorCode = "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");

                            // Check if the color code already exists in the HashSet
                            if (!uniqueColors.Contains(colorCode))
                            {
                                // If not, add it to the HashSet and List
                                uniqueColors.Add(colorCode);

                                // Convert the RGB color value to an HSL value and get the brightness
                                float brightness = color.GetBrightness();

                                // Add the color code and brightness to the list
                                colors.Add(new Tuple<string, float>(colorCode, brightness));
                            }
                        }
                    }

                    // Sort the list by the brightness of the colors in reverse order
                    colors.Sort((a, b) => b.Item2.CompareTo(a.Item2));

                    // Add all color codes to the TreeView
                    foreach (var color in colors)
                    {
                        TreeNode node = new TreeNode();
                        node.Text = color.Item1;
                        node.BackColor = ColorTranslator.FromHtml(color.Item1);
                        treeView.Nodes.Add(node);
                    }

                    // Update the label with the total number of colors
                    label.Text = "Total unique colors: " + colors.Count;
                };

                // Load the colors when you first open the form
                loadColors();

                // Highlight the node when hovering over it with the mouse
                treeView.NodeMouseHover += (s, e) =>
                {
                    treeView.SelectedNode = e.Node;
                };

                // Create a button and add it to the shape
                Button buttonSendToTextBoxColorAdress = new Button();
                buttonSendToTextBoxColorAdress.Text = "Send ColorAdress";
                buttonSendToTextBoxColorAdress.Dock = DockStyle.Bottom;

                buttonSendToTextBoxColorAdress.Click += (s, e) =>
                {
                    // Check if an item is selected
                    if (treeView.SelectedNode != null)
                    {
                        // Take the selected text from the TreeView
                        string selectedColor = treeView.SelectedNode.Text;

                        // Put the text in textBoxColorAdress
                        textBoxColorAdress.Text = selectedColor;
                    }
                };

                colorListForm.Controls.Add(buttonSendToTextBoxColorAdress);

                // Create a btToUpdate button and add it to the shape
                Button buttonBtToUpdate = new Button();
                buttonBtToUpdate.Text = "Update";
                buttonBtToUpdate.Dock = DockStyle.Bottom;

                buttonBtToUpdate.Click += btToUpdate_Click;

                colorListForm.Controls.Add(buttonBtToUpdate);

                // Create an update button and add it to the shape
                Button buttonRefresh = new Button();
                buttonRefresh.Text = "Refresh List";
                buttonRefresh.Dock = DockStyle.Bottom;

                buttonRefresh.Click += (s, e) =>
                {
                    loadColors();
                };

                colorListForm.Controls.Add(buttonRefresh);

                // Display the shape
                colorListForm.Show();
            }
        }
        #endregion
        #region Modus

        private Form colorChangeForm; // Instance variable for the shape
        private void btModusColorChange_Click(object sender, EventArgs e)
        {
            // Check if the shape already exists
            if (colorChangeForm != null && colorChangeForm.Visible)
            {
                // The shape is already open, so bring it to the foreground
                colorChangeForm.BringToFront();
            }
            else
            {
                // The form does not exist or has been closed, so create a new one
                colorChangeForm = new Form();

                colorChangeForm.Size = new Size(250, 400); // Default is 800 pixels wide and 600 pixels high

                // Set the title of the form
                colorChangeForm.Text = "Modus";

                // Remove the icon
                colorChangeForm.ShowIcon = false;

                // Add the FormClosing event handler
                colorChangeForm.FormClosing += (s, e) =>
                {
                    e.Cancel = true;  // Prevents the mold from closing
                    colorChangeForm.Hide();  // Instead, hides the shape
                };

                // Create the checkbox
                RadioButton grayscaleCheckbox = new RadioButton();
                grayscaleCheckbox.Text = "Grayscale";
                grayscaleCheckbox.Location = new Point(10, 10); // Set position

                // Create the checkbox
                CheckBox protectColorsCheckbox = new CheckBox();
                protectColorsCheckbox.Text = "Protect Colors";
                protectColorsCheckbox.Location = new Point(120, 10); // Set the position
                protectColorsCheckbox.Checked = false;

                // Create the TrackBar to change brightness
                TrackBar brightnessTrackBar = new TrackBar();
                brightnessTrackBar.Location = new Point(10, 40); // Set position
                brightnessTrackBar.Minimum = -40;
                brightnessTrackBar.Maximum = 40;

                // Create the TrackBar to change contrast
                TrackBar contrastTrackBar = new TrackBar();
                contrastTrackBar.Location = new Point(10, 85); // Set position
                contrastTrackBar.Minimum = -100;
                contrastTrackBar.Maximum = 100;
                contrastTrackBar.Value = 0; // Set the initial value to 0

                // Create the TrackBar for gamma correction
                TrackBar gammaTrackBar = new TrackBar();
                gammaTrackBar.Location = new Point(10, 130); // Set position
                gammaTrackBar.Minimum = -97;
                gammaTrackBar.Maximum = 400;
                gammaTrackBar.Value = 0; // Set the initial value to 0

                // Create the Saturation TrackBar
                TrackBar saturationTrackBar = new TrackBar();
                saturationTrackBar.Location = new Point(10, 175); // Set position
                saturationTrackBar.Minimum = -100;
                saturationTrackBar.Maximum = 100;
                saturationTrackBar.Value = 0; // Set the initial value to 0

                // Create the TrackBar for color change
                TrackBar colorTrackBar = new TrackBar();
                colorTrackBar.Location = new Point(10, 220); // Set position
                colorTrackBar.Minimum = -180;
                colorTrackBar.Maximum = 180;
                colorTrackBar.Value = 0; // Set the initial value to 0

                // Create the label
                Label updateLabel = new Label();
                updateLabel.Location = new Point(10, 280); // Set the position
                updateLabel.Text = ""; // Put the initial text

                // Create the label to display the current value of the TrackBar
                Label brightnessLabel = new Label();
                brightnessLabel.Location = new Point(120, 40); // Set position 40 is height 120 from the right

                // Create the label to display the current value of the TrackBar
                Label contrastLabel = new Label();
                contrastLabel.Location = new Point(120, 85); // Set position

                // Create the label to display the current value of the TrackBar
                Label gammaLabel = new Label();
                gammaLabel.Location = new Point(120, 130); // Set position

                // Create the label to display the current value of the TrackBar
                Label saturationLabel = new Label();
                saturationLabel.Location = new Point(120, 175); // Set position

                // Create the label to display the current value of the TrackBar
                Label colorLabel = new Label();
                colorLabel.Location = new Point(120, 220); // Set position

                // Add an event handler to change brightness in real time

                // Add an event handler to change brightness in real time
                brightnessTrackBar.Scroll += (s, e) =>
                {
                    if (originalImage != null)
                    {
                        Bitmap image = new Bitmap(originalImage);

                        // Change the brightness of the image
                        float brightness = brightnessTrackBar.Value * 0.01f; //Increases the gradual brightness factor example 0.001f
                        float[][] ptsArray ={
                            new float[] {1, 0, 0, 0, 0},
                            new float[] {0, 1, 0, 0, 0},
                            new float[] {0, 0, 1, 0, 0},
                            new float[] {0, 0, 0, 1.0f, 0},
                            new float[] {brightness, brightness, brightness, 1.0f, 1}
                        };
                        ImageAttributes attributes = new ImageAttributes();
                        attributes.ClearColorMatrix();
                        attributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                        Graphics g = Graphics.FromImage(image);
                        g.DrawImage(image,
                            new Rectangle(0, 0, image.Width, image.Height),
                            0,
                            0,
                            image.Width,
                            image.Height,
                            GraphicsUnit.Pixel,
                            attributes);

                        pictureBox1.Image = image;

                        // Update the label with the current value of the TrackBar
                        brightnessLabel.Text = "Brightness: " + brightnessTrackBar.Value.ToString();
                    }
                };

                // Add an event handler to change the contrast in real time
                contrastTrackBar.Scroll += (s, e) =>
                {
                    if (originalImage != null)
                    {
                        Bitmap image = new Bitmap(originalImage);
                        // Change the contrast of the image
                        float contrast = (contrastTrackBar.Value + 100) * 0.01f;
                        float[][] ptsArray ={
                    new float[] {contrast, 0, 0, 0, 0},
                    new float[] {0, contrast, 0, 0, 0},
                    new float[] {0, 0, contrast, 0, 0},
                    new float[] {0, 0, 0, 1.0f, 0},
                    new float[] {0.001f, 0.001f, 0.001f, 1.0f, 1}
                };
                        ImageAttributes attributes = new ImageAttributes();
                        attributes.ClearColorMatrix();
                        attributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                        Graphics g = Graphics.FromImage(image);
                        g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0,
                            image.Width, image.Height,
                            GraphicsUnit.Pixel,
                            attributes);
                        pictureBox1.Image = image;

                        // Update the label with the current value of the TrackBar
                        contrastLabel.Text = "Contrast: " + contrastTrackBar.Value.ToString();

                    }
                };

                // Method of adjusting the gamma value of an image
                Bitmap AdjustGamma(Image image, float gamma)
                {
                    Bitmap adjustedImage = new Bitmap(image.Width, image.Height);
                    Graphics g = Graphics.FromImage(adjustedImage);
                    ImageAttributes attributes = new ImageAttributes();
                    attributes.SetGamma(gamma);
                    g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                                0, 0, image.Width, image.Height,
                                GraphicsUnit.Pixel, attributes);
                    return adjustedImage;
                }

                // Add an event handler to change gamma correction in real time
                gammaTrackBar.Scroll += (s, e) =>
                {
                    if (originalImage != null)
                    {
                        Bitmap image = new Bitmap(originalImage);
                        // Change the gamma of the image
                        float gamma = 1.0f + gammaTrackBar.Value * 0.01f; // Adjust gamma value
                        Bitmap adjustedImage = AdjustGamma(image, gamma);
                        pictureBox1.Image = adjustedImage;
                        // Update the label with the current value of the TrackBar
                        gammaLabel.Text = "Gamma: " + gammaTrackBar.Value.ToString();
                    }
                };

                // Add an event handler to change saturation in real time
                saturationTrackBar.Scroll += (s, e) =>
                {
                    if (originalImage != null)
                    {
                        Bitmap image = new Bitmap(originalImage);
                        // Change the saturation of the image
                        float saturation = 1.0f + saturationTrackBar.Value * 0.01f; // Adjust saturation value
                        for (int y = 0; y < image.Height; y++)
                        {
                            for (int x = 0; x < image.Width; x++)
                            {
                                Color originalColor = image.GetPixel(x, y);
                                float grayScale = (originalColor.R * 0.3f) + (originalColor.G * 0.59f) + (originalColor.B * 0.11f);
                                int r = Clamp((int)(grayScale + saturation * (originalColor.R - grayScale)));
                                int g = Clamp((int)(grayScale + saturation * (originalColor.G - grayScale)));
                                int b = Clamp((int)(grayScale + saturation * (originalColor.B - grayScale)));
                                Color newColor = Color.FromArgb(r, g, b);
                                image.SetPixel(x, y, newColor);
                            }
                        }
                        pictureBox1.Image = image;
                        // Update the label with the current value of the TrackBar
                        saturationLabel.Text = "Saturation: " + saturationTrackBar.Value.ToString();
                    }

                    // Method for limiting values ​​between 0 and 255
                    int Clamp(int value)
                    {
                        if (value < 0) return 0;
                        if (value > 255) return 255;
                        return value;
                    }
                };

                // Add an event handler to change the color in real time
                colorTrackBar.Scroll += (sender, eventArgs) =>
                {
                    if (originalImage != null)
                    {
                        Bitmap image = new Bitmap(originalImage);
                        // Change the color of the image
                        float hueChange = colorTrackBar.Value / 360.0f; // Adjust hue change value

                        // Local function to limit values ​​between 0 and 255
                        int Clamp(int value)
                        {
                            if (value < 0) return 0;
                            if (value > 255) return 255;
                            return value;
                        }

                        // Local function to convert RGB to HSL
                        void RgbToHsl(int r, int g, int b, out double h, out double s, out double l)
                        {
                            double r_ = r / 255.0;
                            double g_ = g / 255.0;
                            double b_ = b / 255.0;

                            double max = Math.Max(r_, Math.Max(g_, b_));
                            double min = Math.Min(r_, Math.Min(g_, b_));

                            h = (max + min) / 2.0;
                            l = h;
                            s = h;

                            if (max == min)
                            {
                                h = s = 0; // achromatic
                            }
                            else
                            {
                                double d = max - min;
                                s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);
                                if (max == r_)
                                {
                                    h = (g_ - b_) / d + (g_ < b_ ? 6 : 0);
                                }
                                else if (max == g_)
                                {
                                    h = (b_ - r_) / d + 2;
                                }
                                else if (max == b_)
                                {
                                    h = (r_ - g_) / d + 4;
                                }
                                h /= 6;
                            }
                        }

                        // Local function to convert HSL to RGB
                        void HslToRgb(double h, double s, double l, out int r, out int g, out int b)
                        {
                            double r_, g_, b_;

                            if (s == 0)
                            {
                                r_ = g_ = b_ = l; // achromatic
                            }
                            else
                            {
                                Func<double, double, double, double> hue2rgb = (p, q, t) =>
                                {
                                    if (t < 0) t += 1;
                                    if (t > 1) t -= 1;
                                    if (t < 1 / 6.0) return p + (q - p) * 6 * t;
                                    if (t < 1 / 2.0) return q;
                                    if (t < 2 / 3.0) return p + (q - p) * (2 / 3.0 - t) * 6;
                                    return p;
                                };

                                var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                                var p = 2 * l - q;
                                r_ = hue2rgb(p, q, h + hueChange);
                                g_ = hue2rgb(p, q, h);
                                b_ = hue2rgb(p, q, h - hueChange);
                            }

                            r = Clamp((int)(r_ * 255.0));
                            g = Clamp((int)(g_ * 255.0));
                            b = Clamp((int)(b_ * 255.0));
                        }

                        for (int y = 0; y < image.Height; y++)
                        {
                            for (int x = 0; x < image.Width; x++)
                            {
                                Color originalColor = image.GetPixel(x, y);
                                // Convert the color to HSL color space
                                double h;
                                double s;
                                double l;
                                RgbToHsl(originalColor.R, originalColor.G, originalColor.B, out h, out s, out l);

                                // Convert the color back to the RGB color space
                                int r;
                                int g;
                                int b;
                                HslToRgb(h, s, l, out r, out g, out b);

                                Color newColor = Color.FromArgb(r, g, b);
                                image.SetPixel(x, y, newColor);
                            }
                        }
                        pictureBox1.Image = image;
                        // Update the label with the current value of the TrackBar
                        pictureBox1.Refresh();  // Update the PictureBox
                        colorLabel.Text = "Color: " + colorTrackBar.Value.ToString();
                    }
                };
                // Colors except 000000 or ffffff
                // Add the event handler
                protectColorsCheckbox.CheckedChanged += (s, e) =>
                {
                    // Check whether the checkbox is activated
                    if (protectColorsCheckbox.Checked)
                    {
                        // Change the brightness change function
                        brightnessTrackBar.Scroll += (senderBrightness, e) =>
                        {
                            if (originalImage != null)
                            {
                                Bitmap image = new Bitmap(originalImage);
                                float brightness = brightnessTrackBar.Value * 0.01f; // Scale the value so that it is between -1 and 1
                                for (int y = 0; y < image.Height; y++)
                                {
                                    for (int x = 0; x < image.Width; x++)
                                    {
                                        Color originalColor = image.GetPixel(x, y);
                                        // Check if the color is 000000 or ffffff
                                        if (originalColor.ToArgb() != Color.Black.ToArgb() && originalColor.ToArgb() != Color.White.ToArgb())
                                        {
                                            // Change the brightness of the color
                                            int r = Clamp((int)(originalColor.R + brightness * 255)); // Brightness range adjustment
                                            int g = Clamp((int)(originalColor.G + brightness * 255)); // Brightness range adjustment
                                            int b = Clamp((int)(originalColor.B + brightness * 255)); // Brightness range adjustment
                                            Color newColor = Color.FromArgb(r, g, b);
                                            image.SetPixel(x, y, newColor);
                                        }
                                    }
                                }
                                pictureBox1.Image = image;
                                brightnessLabel.Text = "Brightness: " + brightnessTrackBar.Value.ToString();
                            }
                        };

                        // Change the contrast change function
                        contrastTrackBar.Scroll += (s, e) =>
                        {
                            if (originalImage != null)
                            {
                                Bitmap image = new Bitmap(originalImage);
                                float contrast = (contrastTrackBar.Value + 100) * 0.01f;
                                for (int y = 0; y < image.Height; y++)
                                {
                                    for (int x = 0; x < image.Width; x++)
                                    {
                                        Color originalColor = image.GetPixel(x, y);
                                        // Check if the color is 000000 or ffffff
                                        if (originalColor.ToArgb() != Color.Black.ToArgb() && originalColor.ToArgb() != Color.White.ToArgb())
                                        {
                                            // Change the contrast of the color
                                            int r = Clamp((int)(originalColor.R * contrast));
                                            int g = Clamp((int)(originalColor.G * contrast));
                                            int b = Clamp((int)(originalColor.B * contrast));
                                            Color newColor = Color.FromArgb(r, g, b);
                                            image.SetPixel(x, y, newColor);
                                        }
                                    }
                                }
                                pictureBox1.Image = image;
                                contrastLabel.Text = "Contrast: " + contrastTrackBar.Value.ToString();
                            }
                        };

                        // Change the gamma correction function
                        gammaTrackBar.Scroll += (s, e) =>
                        {
                            if (originalImage != null)
                            {
                                Bitmap image = new Bitmap(originalImage);
                                float gamma = 1.0f + gammaTrackBar.Value * 0.01f;
                                for (int y = 0; y < image.Height; y++)
                                {
                                    for (int x = 0; x < image.Width; x++)
                                    {
                                        Color originalColor = image.GetPixel(x, y);
                                        // Check if the color is 000000 or ffffff
                                        if (originalColor.ToArgb() != Color.Black.ToArgb() && originalColor.ToArgb() != Color.White.ToArgb())
                                        {
                                            // Change the gamma of the color
                                            int r = Clamp((int)(Math.Pow(originalColor.R / 255.0, gamma) * 255));
                                            int g = Clamp((int)(Math.Pow(originalColor.G / 255.0, gamma) * 255));
                                            int b = Clamp((int)(Math.Pow(originalColor.B / 255.0, gamma) * 255));
                                            Color newColor = Color.FromArgb(r, g, b);
                                            image.SetPixel(x, y, newColor);
                                        }
                                    }
                                }
                                pictureBox1.Image = image;
                                gammaLabel.Text = "Gamma: " + gammaTrackBar.Value.ToString();
                            }
                        };

                        // Change the saturation change function
                        saturationTrackBar.Scroll += (s, e) =>
                        {
                            if (originalImage != null)
                            {
                                Bitmap image = new Bitmap(originalImage);
                                float saturation = 1.0f + saturationTrackBar.Value * 0.01f;
                                for (int y = 0; y < image.Height; y++)
                                {
                                    for (int x = 0; x < image.Width; x++)
                                    {
                                        Color originalColor = image.GetPixel(x, y);
                                        // Check if the color is 000000 or ffffff
                                        if (originalColor.ToArgb() != Color.Black.ToArgb() && originalColor.ToArgb() != Color.White.ToArgb())
                                        {
                                            // Change the saturation of the color
                                            float grayScale = (originalColor.R * 0.3f) + (originalColor.G * 0.59f) + (originalColor.B * 0.11f);
                                            int r = Clamp((int)(grayScale + saturation * (originalColor.R - grayScale)));
                                            int g = Clamp((int)(grayScale + saturation * (originalColor.G - grayScale)));
                                            int b = Clamp((int)(grayScale + saturation * (originalColor.B - grayScale)));
                                            Color newColor = Color.FromArgb(r, g, b);
                                            image.SetPixel(x, y, newColor);
                                        }
                                    }
                                }
                                pictureBox1.Image = image;
                                saturationLabel.Text = "Saturation: " + saturationTrackBar.Value.ToString();
                            }
                        };

                        int Clamp(int value, int min = 0, int max = 255)
                        {
                            if (value < min) return min;
                            if (value > max) return max;
                            return value;
                        }

                        void RgbToHsl(int r, int g, int b, out double h, out double s, out double l)
                        {
                            double r_ = r / 255.0;
                            double g_ = g / 255.0;
                            double b_ = b / 255.0;

                            double max = Math.Max(r_, Math.Max(g_, b_));
                            double min = Math.Min(r_, Math.Min(g_, b_));

                            h = (max + min) / 2.0;
                            l = h;
                            s = h;

                            if (max == min)
                            {
                                h = s = 0; // achromatic
                            }
                            else
                            {
                                double d = max - min;
                                s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);
                                if (max == r_)
                                {
                                    h = (g_ - b_) / d + (g_ < b_ ? 6 : 0);
                                }
                                else if (max == g_)
                                {
                                    h = (b_ - r_) / d + 2;
                                }
                                else if (max == b_)
                                {
                                    h = (r_ - g_) / d + 4;
                                }
                                h /= 6;
                            }
                        }

                        void HslToRgb(double h, double s, double l, out int r, out int g, out int b)
                        {
                            double r_, g_, b_;

                            if (s == 0)
                            {
                                r_ = g_ = b_ = l; // achromatic
                            }
                            else
                            {
                                Func<double, double, double, double> hue2rgb = (p, q, t) =>
                                {
                                    if (t < 0) t += 1;
                                    if (t > 1) t -= 1;
                                    if (t < 1 / 6.0) return p + (q - p) * 6 * t;
                                    if (t < 1 / 2.0) return q;
                                    if (t < 2 / 3.0) return p + (q - p) * (2 / 3.0 - t) * 6;
                                    return p;
                                };

                                var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                                var p = 2 * l - q;

                                r_ = hue2rgb(p, q, h + 1 / 3.0);
                                g_ = hue2rgb(p, q, h);
                                b_ = hue2rgb(p, q, h - 1 / 3.0);
                            }

                            r = (int)(r_ * 255.0);
                            g = (int)(g_ * 255.0);
                            b = (int)(b_ * 255.0);
                        }

                        // Change color change function
                        colorTrackBar.Scroll += (s, e) =>
                        {
                            if (originalImage != null)
                            {
                                Bitmap image = new Bitmap(originalImage);
                                float hueChange = colorTrackBar.Value / 360.0f;
                                for (int y = 0; y < image.Height; y++)
                                {
                                    for (int x = 0; x < image.Width; x++)
                                    {
                                        Color originalColor = image.GetPixel(x, y);
                                        // Check if the color is 000000 or ffffff
                                        if (originalColor.ToArgb() != Color.Black.ToArgb() && originalColor.ToArgb() != Color.White.ToArgb())
                                        {
                                            // Change the hue of the paint
                                            double h;
                                            double saturation;
                                            double l;
                                            RgbToHsl(originalColor.R, originalColor.G, originalColor.B, out h, out saturation, out l);
                                            int r;
                                            int g;
                                            int b;
                                            HslToRgb(h + hueChange, saturation, l, out r, out g, out b);
                                            Color newColor = Color.FromArgb(r, g, b);
                                            image.SetPixel(x, y, newColor);
                                        }
                                    }
                                }
                                pictureBox1.Image = image;
                                colorLabel.Text = "Color: " + colorTrackBar.Value.ToString();
                            }
                        };
                    }
                };

                // Create the button to apply the grayscale changes
                Button applyButton = new Button();
                applyButton.Text = "Apply";
                applyButton.Location = new Point(10, 320); // Set position
                applyButton.Click += (s, e) =>
                {
                    if (pictureBox1.Image != null)
                    {
                        Bitmap image = new Bitmap(pictureBox1.Image); // Zugriff auf das Bild in der PictureBox

                        // Convert the image to grayscale only if grayscaleCheckbox is checked
                        if (grayscaleCheckbox.Checked)
                        {
                            for (int y = 0; y < image.Height; y++)
                            {
                                for (int x = 0; x < image.Width; x++)
                                {
                                    Color originalColor = image.GetPixel(x, y);
                                    int grayScale = (int)((originalColor.R * .3) + (originalColor.G * .59) + (originalColor.B * .11));
                                    Color newColor = Color.FromArgb(grayScale, grayScale, grayScale);
                                    image.SetPixel(x, y, newColor);
                                }
                            }
                        }

                        pictureBox1.Image = image; // Update the image in the PictureBox
                        pictureBox1.Refresh();  // Refresh the PictureBox

                        // Update the text of the label
                        updateLabel.Text = "Image updated.";
                    }
                };

                // Create the button
                Button resetButton = new Button();
                resetButton.Text = "Reset";
                resetButton.Location = new Point(80, 320); // Set the position

                // Add the event handler
                resetButton.Click += (s, e) =>
                {
                    // Reset the values ​​of all TrackBars to 0
                    brightnessTrackBar.Value = 0;
                    contrastTrackBar.Value = 0;
                    gammaTrackBar.Value = 0;
                    saturationTrackBar.Value = 0;
                    colorTrackBar.Value = 0;

                    // Update the labels
                    brightnessLabel.Text = "Brightness: 0";
                    contrastLabel.Text = "Contrast: 0";
                    gammaLabel.Text = "Gamma: 0";
                    saturationLabel.Text = "Saturation: 0";
                    colorLabel.Text = "Color: 0";
                };

                // Add the checkbox and button to the shape
                colorChangeForm.Controls.Add(grayscaleCheckbox);
                colorChangeForm.Controls.Add(brightnessTrackBar);
                colorChangeForm.Controls.Add(brightnessLabel);
                colorChangeForm.Controls.Add(applyButton);
                // Add the new TrackBar and Label to the shape
                colorChangeForm.Controls.Add(contrastTrackBar);
                colorChangeForm.Controls.Add(contrastLabel);
                // Add the new TrackBar and Label to the shape
                colorChangeForm.Controls.Add(gammaTrackBar);
                colorChangeForm.Controls.Add(gammaLabel);
                // Add the new TrackBar and Label to the shape
                colorChangeForm.Controls.Add(saturationTrackBar);
                colorChangeForm.Controls.Add(saturationLabel);
                // Add the new TrackBar and Label to the shape
                colorChangeForm.Controls.Add(colorTrackBar);
                colorChangeForm.Controls.Add(colorLabel);
                // Add the checkbox to the form
                colorChangeForm.Controls.Add(protectColorsCheckbox);
                // Add the button to the form
                colorChangeForm.Controls.Add(resetButton);
                // Add the label to the shape
                colorChangeForm.Controls.Add(updateLabel);

                // Display the shape
                colorChangeForm.Show();
            }
        }
        #endregion
        #region FillTeture
        private void fillTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image has been loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if ((cropArea.Width <= 0 || cropArea.Height <= 0) && !checkBoxFreehand.Checked)
            {
                MessageBox.Show("No area has been selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (System.Windows.Forms.OpenFileDialog openFileDialog2 = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog2.Filter = "Image files|*.bmp;*.jpg;*.jpeg;*.png";

                if (openFileDialog2.ShowDialog() == DialogResult.OK)
                {
                    Bitmap newTexture = new Bitmap(openFileDialog2.FileName);
                    Bitmap imageCopy = (Bitmap)pictureBox1.Image.Clone();

                    using (Graphics g = Graphics.FromImage(imageCopy))
                    {
                        // Adjust cropArea and points based on zoom level
                        Rectangle adjustedCropArea = new Rectangle(cropArea.X / (int)Math.Pow(2, zoomCounter), cropArea.Y / (int)Math.Pow(2, zoomCounter), cropArea.Width / (int)Math.Pow(2, zoomCounter), cropArea.Height / (int)Math.Pow(2, zoomCounter));
                        List<Point> adjustedPoints = points.Select(p => new Point(p.X / (int)Math.Pow(2, zoomCounter), p.Y / (int)Math.Pow(2, zoomCounter))).ToList();

                        if (checkBoxCircle.Checked)
                        {
                            // Create a mask for the circle area
                            using (Bitmap mask = new Bitmap(adjustedCropArea.Width, adjustedCropArea.Height))
                            {
                                using (Graphics maskGraphics = Graphics.FromImage(mask))
                                {
                                    maskGraphics.Clear(Color.White);
                                    maskGraphics.FillEllipse(Brushes.Black, 0, 0, adjustedCropArea.Width, adjustedCropArea.Height);
                                }

                                // Scale the texture to the size of the mask
                                newTexture = new Bitmap(newTexture, mask.Size);

                                // Apply the mask to the new texture image
                                for (int x = 0; x < mask.Width; x++)
                                {
                                    for (int y = 0; y < mask.Height; y++)
                                    {
                                        if (mask.GetPixel(x, y).R != 0)
                                        {
                                            newTexture.SetPixel(x, y, Color.Transparent);
                                        }
                                    }
                                }
                            }

                            // Draw the texture on the image inside the circle
                            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                            g.DrawImage(newTexture, adjustedCropArea);
                        }
                        else if (checkBoxFreehand.Checked && adjustedPoints.Count > 1)
                        {
                            // Create a mask for the freehand area
                            using (Bitmap mask = new Bitmap(imageCopy.Width, imageCopy.Height))
                            {
                                using (Graphics maskGraphics = Graphics.FromImage(mask))
                                {
                                    maskGraphics.Clear(Color.White);
                                    maskGraphics.FillPolygon(Brushes.Black, adjustedPoints.ToArray());
                                }

                                // Scale the texture to the size of the mask
                                newTexture = new Bitmap(newTexture, mask.Size);

                                // Apply the mask to the new texture image
                                for (int x = 0; x < mask.Width; x++)
                                {
                                    for (int y = 0; y < mask.Height; y++)
                                    {
                                        if (mask.GetPixel(x, y).R != 0)
                                        {
                                            newTexture.SetPixel(x, y, Color.Transparent);
                                        }
                                    }
                                }
                            }

                            // Draw the texture onto the image inside the freehand shape
                            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                            g.DrawImage(newTexture, new Rectangle(0, 0, imageCopy.Width, imageCopy.Height));
                        }
                        else if (checkBoxLines.Checked && adjustedPoints.Count > 1)
                        {
                            // Create a mask for the line area
                            using (Bitmap mask = new Bitmap(imageCopy.Width, imageCopy.Height))
                            {
                                using (Graphics maskGraphics = Graphics.FromImage(mask))
                                {
                                    maskGraphics.Clear(Color.White);
                                    maskGraphics.FillPolygon(Brushes.Black, adjustedPoints.ToArray());
                                }

                                // Scale the texture to the size of the mask
                                newTexture = new Bitmap(newTexture, mask.Size);

                                // Apply the mask to the new texture image
                                for (int x = 0; x < mask.Width; x++)
                                {
                                    for (int y = 0; y < mask.Height; y++)
                                    {
                                        if (mask.GetPixel(x, y).R != 0)
                                        {
                                            newTexture.SetPixel(x, y, Color.Transparent);
                                        }
                                    }
                                }
                            }

                            // Draw the texture onto the image inside the line
                            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                            g.DrawImage(newTexture, new Rectangle(0, 0, imageCopy.Width, imageCopy.Height));
                        }
                        else
                        {
                            // Draw the texture on the image
                            g.DrawImage(newTexture, adjustedCropArea);
                        }

                    }

                    pictureBox1.Image = imageCopy;
                    pictureBox1.Refresh();
                }
            }
        }

        private bool shouldDrawRectangle = false; //??

        /*private void pictureBox1_MouseDown2(object sender, MouseEventArgs e)
        {
            if (checkBoxFreehand.Checked && e.Button == MouseButtons.Left)
            {
                points.Clear();
                points.Add(e.Location);
            }
            else if (e.Button == MouseButtons.Left)
            {
                startPoint = e.Location;
                isDragging = true;
                cropArea = new Rectangle(startPoint.X, startPoint.Y, 0, 0);
            }
        }*/

        private void pictureBox1_MouseDown2(object sender, MouseEventArgs e)
        {
            if (checkBoxFreehand.Checked && e.Button == MouseButtons.Left)
            {
                points.Clear();
                points.Add(e.Location);
            }
            else if (e.Button == MouseButtons.Left)
            {
                startPoint = e.Location;
                isDragging = true;
                cropArea = new Rectangle(startPoint.X, startPoint.Y, 0, 0);

                if (checkBox2Colors.Checked)
                {
                    // Create a copy of the image in pictureBox1
                    Bitmap image = new Bitmap(pictureBox1.Image);

                    // Create a Graphics object from the image
                    using (Graphics g = Graphics.FromImage(image))
                    {
                        // Convert the text in textBoxColorToAdress and textBoxColorToAdress2 into color values
                        string colorCode1 = textBoxColorToAdress.Text;
                        if (!colorCode1.StartsWith("#")) { colorCode1 = "#" + colorCode1; }
                        Color penColor1;
                        try { penColor1 = ColorTranslator.FromHtml(colorCode1); }
                        catch { penColor1 = Color.Black; }  // Default to black if the color code is invalid

                        string colorCode2 = textBoxColorToAdress2.Text;
                        if (!colorCode2.StartsWith("#")) { colorCode2 = "#" + colorCode2; }
                        Color penColor2;
                        try { penColor2 = ColorTranslator.FromHtml(colorCode2); }
                        catch { penColor2 = Color.Black; }  // Default to black if the color code is invalid

                        // If one of the text boxes is empty, use the color from the other text box
                        if (string.IsNullOrEmpty(textBoxColorToAdress.Text)) { penColor1 = penColor2; }
                        if (string.IsNullOrEmpty(textBoxColorToAdress2.Text)) { penColor2 = penColor1; }

                        // Create pens with the specified colors
                        Pen pen1 = new Pen(penColor1);
                        Pen pen2 = new Pen(penColor2);

                        // Draw a pixel at the current position with the first color
                        g.DrawRectangle(pen1, startPoint.X, startPoint.Y, 1, 1);

                        // Draw a pixel next to the current position with the second color
                        g.DrawRectangle(pen2, startPoint.X + 1, startPoint.Y, 1, 1);
                    }

                    // Update the image in pictureBox1
                    pictureBox1.Image = image;
                }
            }
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            // Cast the sender to a CheckBox
            CheckBox checkBox = (CheckBox)sender;

            // If the CheckBox is checked, uncheck all other CheckBoxes
            if (checkBox.Checked)
            {
                foreach (Control control in panel3.Controls) // Replace "panel3" with the name of your panel
                {
                    if (control is CheckBox && control != checkBox)
                    {
                        ((CheckBox)control).Checked = false;
                    }
                }
            }
        }
        #endregion


        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (checkBoxFreehand.Checked && e.Button == MouseButtons.Left)
            {
                points.Add(points[0]);  // Connect the end to the beginning
                pictureBox1.Invalidate();
            }
            else if (checkBoxLines.Checked && e.Button == MouseButtons.Left && points.Count > 0)
            {
                // Don't connect the end to the beginning for lines
                pictureBox1.Invalidate();

                // Calculate the bounding box of the points
                int minX = points.Min(p => p.X);
                int minY = points.Min(p => p.Y);
                int maxX = points.Max(p => p.X);
                int maxY = points.Max(p => p.Y);

                // Set the cropArea to the bounding box
                cropArea = new Rectangle(minX, minY, maxX - minX, maxY - minY);
            }
            else if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }



        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.Yellow))
            {
                pen.DashStyle = DashStyle.Dash;

                if (checkBoxCircle.Checked)
                {
                    // Draw a circle
                    e.Graphics.DrawEllipse(pen, cropArea);
                }
                else
                {
                    // Draw a rectangle
                    e.Graphics.DrawRectangle(pen, cropArea);
                }

                if (checkBoxFreehand.Checked && points.Count > 1)
                {
                    // Draw freehand
                    e.Graphics.DrawLines(Pens.Yellow, points.ToArray());
                }

                if (checkBoxLines.Checked)
                {
                    // Draw lines from point to point
                    DrawLines(e.Graphics, points);
                }
            }
        }
        #region Zoom
        private int zoomCounter = 0;
        private void zoomButton_Click(object sender, EventArgs e)
        {
            // Check if the zoom has already been applied twice
            if (zoomCounter >= 2)
            {
                return;
            }

            // Set the SizeMode property to Zoom
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            // Resize the PictureBox to increase the zoom effect
            pictureBox1.Width *= 2;
            pictureBox1.Height *= 2;

            // Increase the zoom counter
            zoomCounter++;
        }
        #endregion
        #region Reset
        private void resetButton2_Click(object sender, EventArgs e)
        {
            // Reset the SizeMode property to Normal
            pictureBox1.SizeMode = PictureBoxSizeMode.Normal;

            // Reset the size of the PictureBox to its original size
            pictureBox1.Width /= (int)Math.Pow(2, zoomCounter);
            pictureBox1.Height /= (int)Math.Pow(2, zoomCounter);

            // Reset the zoom counter
            zoomCounter = 0;

            // Clear the list of points
            points.Clear();

            // Reset the cropArea
            cropArea = new Rectangle();

            // Update the PictureBox to reflect the changes
            pictureBox1.Invalidate();
        }
        #endregion
        # region Normalization
        private void btColorNormalization_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                Image clipboardImage = Clipboard.GetImage();
                Bitmap bitmapImage = new Bitmap(clipboardImage);
                Bitmap normalizedImage = new Bitmap(bitmapImage.Width, bitmapImage.Height);

                int minR = 255, minG = 255, minB = 255;
                int maxR = 0, maxG = 0, maxB = 0;

                for (int y = 0; y < bitmapImage.Height; y++)
                {
                    for (int x = 0; x < bitmapImage.Width; x++)
                    {
                        Color pixelColor = bitmapImage.GetPixel(x, y);
                        minR = Math.Min(minR, pixelColor.R);
                        minG = Math.Min(minG, pixelColor.G);
                        minB = Math.Min(minB, pixelColor.B);
                        maxR = Math.Max(maxR, pixelColor.R);
                        maxG = Math.Max(maxG, pixelColor.G);
                        maxB = Math.Max(maxB, pixelColor.B);
                    }
                }

                for (int y = 0; y < bitmapImage.Height; y++)
                {
                    for (int x = 0; x < bitmapImage.Width; x++)
                    {
                        Color oldColor = bitmapImage.GetPixel(x, y);
                        int newR = ((oldColor.R - minR) * 255) / (maxR - minR);
                        int newG = ((oldColor.G - minG) * 255) / (maxG - minG);
                        int newB = ((oldColor.B - minB) * 255) / (maxB - minB);

                        Color newColor = Color.FromArgb(newR, newG, newB);
                        normalizedImage.SetPixel(x, y, newColor);
                    }
                }

                pictureBox1.Image = normalizedImage;
            }
            else
            {
                MessageBox.Show("The clipboard does not contain an image.");
            }
        }
        #endregion

        #region Color mode
        private int colorMode = 0;

        private void buttonRedToBlueColors_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                // Retrieve the image from the clipboard
                Image clipboardImage = Clipboard.GetImage();

                // Create a bitmap object
                Bitmap bitmapImage = new Bitmap(clipboardImage);

                // Create a new Bitmap object with the same dimensions as the original image
                Bitmap newImage = new Bitmap(bitmapImage.Width, bitmapImage.Height);

                // Loop to process each pixel in the image
                for (int y = 0; y < bitmapImage.Height; y++)
                {
                    for (int x = 0; x < bitmapImage.Width; x++)
                    {
                        // Get color of current pixel
                        Color pixelColor = bitmapImage.GetPixel(x, y);

                        // Depending on the color mode, perform the color conversion
                        Color newColor;
                        switch (colorMode)
                        {
                            case 0: // Red mode (default)
                                if (pixelColor.R > pixelColor.G && pixelColor.R > pixelColor.B)
                                {
                                    newColor = Color.FromArgb(pixelColor.A, pixelColor.B, pixelColor.G, pixelColor.R);
                                }
                                else
                                {
                                    newColor = pixelColor;
                                }
                                break;
                            case 1: // Green mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.G, pixelColor.R, pixelColor.B);
                                break;
                            case 2: // Yellow mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.G, pixelColor.G, pixelColor.R);
                                break;
                            case 3: // Brown mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R, pixelColor.G / 2, pixelColor.B / 2);
                                break;
                            case 4: // Orange
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R, pixelColor.G / 2, 0);
                                break;
                            case 5: // Violet
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, 0, pixelColor.B);
                                break;
                            case 6: // Green
                                newColor = Color.FromArgb(pixelColor.A, 0, pixelColor.G, 0);
                                break;
                            case 7: // Turquoise
                                newColor = Color.FromArgb(pixelColor.A, 0, pixelColor.G, pixelColor.B);
                                break;
                            case 8: // Pink
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R, 0, pixelColor.B);
                                break;
                            case 9: // Light Blue
                                newColor = Color.FromArgb(pixelColor.A, 0, pixelColor.G, pixelColor.R);
                                break;
                            case 10: // Dark red
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, 0, 0);
                                break;
                            case 11: // Purple
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, 0, pixelColor.B / 2);
                                break;
                            case 12: // Yellow-green
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, pixelColor.G / 2, 0);
                                break;
                            case 13: // magenta
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R, 0, pixelColor.B / 2);
                                break;
                            case 14: // Cyan
                                newColor = Color.FromArgb(pixelColor.A, 0, pixelColor.G / 2, pixelColor.B);
                                break;
                            case 15: // Dark green
                                newColor = Color.FromArgb(pixelColor.A, 0, pixelColor.G / 2, 0);
                                break;
                            case 16: // olive green
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, pixelColor.G / 2, 0);
                                break;
                            case 17: // Pink
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R, 0, pixelColor.B / 2);
                                break;
                            case 18: // Turquoise blue
                                newColor = Color.FromArgb(pixelColor.A, 0, pixelColor.G / 2, pixelColor.B / 2);
                                break;
                            case 19: // Golden yellow
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, pixelColor.G / 2, 0);
                                break;
                            case 20: // Gray
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, pixelColor.G / 2, pixelColor.B / 2);
                                break;
                            case 21: // Bright red
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R, 0, 0);
                                break;
                            case 22: // Light green
                                newColor = Color.FromArgb(pixelColor.A, 0, pixelColor.G, 0);
                                break;
                            case 23: // Light Blue
                                newColor = Color.FromArgb(pixelColor.A, 0, 0, pixelColor.B);
                                break;
                            case 24: // Brown
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, pixelColor.G / 2, pixelColor.B / 2);
                                break;
                            case 25: // Dark brown
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, pixelColor.G / 4, pixelColor.B / 4); // Dunkelbraun 
                                break;
                            case 26: // Gray
                                int grayValue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                                newColor = Color.FromArgb(pixelColor.A, grayValue, grayValue, grayValue);
                                break;
                            case 27: // Dark gray
                                int darkGrayValue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                                darkGrayValue = Math.Max(0, Math.Min(255, darkGrayValue - 64)); // Dunkelgrau
                                newColor = Color.FromArgb(pixelColor.A, darkGrayValue, darkGrayValue, darkGrayValue);
                                break;
                            case 28: // Light Blue
                                newColor = Color.FromArgb(pixelColor.A, 0, 0, pixelColor.B); // Hellblau 
                                break;
                            case 29: // Green 
                                newColor = Color.FromArgb(pixelColor.A, 0, pixelColor.G, 0); // Grün 
                                break;
                            case 30: // Gold 
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R, pixelColor.G / 2, 0); // Gold 
                                break;
                            case 31: // Silber 
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, pixelColor.G / 2, pixelColor.B / 2); // Silber 
                                break;
                            case 32: // Bronze 
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, pixelColor.G / 4, 0); // Bronze 
                                break;
                            case 33: // copper
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R, pixelColor.G / 3, 0); // Kupfer 
                                break;
                            default: // Default to red mode 
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.B, pixelColor.G, pixelColor.R);
                                colorMode = 0; //Reset the color mode to the default (red).
                                break;
                        }

                        // Save the changed pixel in the new image
                        newImage.SetPixel(x, y, newColor);
                    }
                }

                // Show the edited image in the PictureBox
                pictureBox1.Image = newImage;

                // Increase the color mode for the next click
                colorMode++;
            }
            else
            {
                // Show message if clipboard does not contain image
                MessageBox.Show("The clipboard does not contain an image.");
            }
        }
        private void btphotorandomColor_Click(object sender, EventArgs e)
        {
            Random rand = new Random();

            if (Clipboard.ContainsImage())
            {
                Image clipboardImage = Clipboard.GetImage();
                Bitmap bitmapImage = new Bitmap(clipboardImage);
                Bitmap newImage = new Bitmap(bitmapImage.Width, bitmapImage.Height);

                for (int y = 0; y < bitmapImage.Height; y++)
                {
                    for (int x = 0; x < bitmapImage.Width; x++)
                    {
                        Color pixelColor = bitmapImage.GetPixel(x, y);

                        // Check whether the color of the pixel is black or white
                        if (pixelColor.R == 0 && pixelColor.G == 0 && pixelColor.B == 0 ||
                            pixelColor.R == 255 && pixelColor.G == 255 && pixelColor.B == 255)
                        {
                            // If the pixel is black or white, skip the color change
                            newImage.SetPixel(x, y, pixelColor);
                            continue;
                        }

                        Color newColor;

                        int maxColorValue = Math.Max(pixelColor.R, Math.Max(pixelColor.G, pixelColor.B));
                        int minColorValue = Math.Min(pixelColor.R, Math.Min(pixelColor.G, pixelColor.B));
                        int midColorValue = pixelColor.R + pixelColor.G + pixelColor.B - maxColorValue - minColorValue;

                        switch (colorMode % 34) // 
                        {
                            case 0:
                                newColor = Color.FromArgb(pixelColor.A, maxColorValue, midColorValue, minColorValue);
                                break;
                            case 1:
                                newColor = Color.FromArgb(pixelColor.A, maxColorValue, minColorValue, midColorValue);
                                break;
                            case 2:
                                newColor = Color.FromArgb(pixelColor.A, midColorValue, maxColorValue, minColorValue);
                                break;
                            case 3:
                                newColor = Color.FromArgb(pixelColor.A, midColorValue, minColorValue, maxColorValue);
                                break;
                            case 4:
                                newColor = Color.FromArgb(pixelColor.A, minColorValue, maxColorValue, midColorValue);
                                break;
                            case 5: // Magenta mode
                                newColor = Color.FromArgb(pixelColor.A, 255, pixelColor.G, 255);
                                break;
                            case 6: // Brown mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R, pixelColor.G / 2, pixelColor.B / 2);
                                break;
                            case 7: // Orange Modus
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R, pixelColor.G / 2, 0);
                                break;
                            case 8: // Violet mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, 0, pixelColor.B);
                                break;
                            case 9: // Green mode
                                newColor = Color.FromArgb(pixelColor.A, 0, pixelColor.G, 0);
                                break;
                            case 10: // Turquoise mode
                                newColor = Color.FromArgb(pixelColor.A, 0, pixelColor.G, pixelColor.B);
                                break;
                            case 11: // Pink mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R, 0, pixelColor.B);
                                break;
                            case 12: // Light blue mode
                                newColor = Color.FromArgb(pixelColor.A, 0, pixelColor.G, pixelColor.R);
                                break;
                            case 13: // Dark red mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, 0, 0);
                                break;
                            case 14: // Purple mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, 0, pixelColor.B / 2);
                                break;
                            case 15: // Yellow-green mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, pixelColor.G / 2, 0);
                                break;
                            case 16: // Cyan mode
                                newColor = Color.FromArgb(pixelColor.A, 0, pixelColor.G / 2, pixelColor.B);
                                break;
                            case 17: // Dark green mode
                                newColor = Color.FromArgb(pixelColor.A, 0, pixelColor.G / 2, 0);
                                break;
                            case 18: // Olive green mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, pixelColor.G / 2, 0);
                                break;
                            case 19: // Rose red mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R, 0, pixelColor.B / 2);
                                break;
                            case 20: // Turquoise blue mode
                                newColor = Color.FromArgb(pixelColor.A, 0, pixelColor.G / 2, pixelColor.B / 2);
                                break;
                            case 21: // Golden yellow mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, pixelColor.G / 2, 0);
                                break;
                            case 22: // Gray mode
                                int grayValue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                                newColor = Color.FromArgb(pixelColor.A, grayValue, grayValue, grayValue);
                                break;
                            case 23: // Bright red mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R, 0, 0);
                                break;
                            case 24: // Light green mode
                                newColor = Color.FromArgb(pixelColor.A, 0, pixelColor.G, 0);
                                break;
                            case 25: // Light blue mode
                                newColor = Color.FromArgb(pixelColor.A, 0, 0, pixelColor.B);
                                break;
                            case 26: // Light blue mode with variation
                                int variation = rand.Next(-50, 50); // Generates a random number between -50 and 50
                                newColor = Color.FromArgb(pixelColor.A,
                                                          Math.Max(0, Math.Min(255, 0 + variation)),
                                                          Math.Max(0, Math.Min(255, 0 + variation)),
                                                          Math.Max(0, Math.Min(255, pixelColor.B + variation)));
                                break;
                            case 27: // Brown mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, pixelColor.G / 2, pixelColor.B / 2);
                                break;
                            case 28: // Dark brown mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, pixelColor.G / 4, pixelColor.B / 4);
                                break;
                            case 29: // Dark gray mode
                                int darkGrayValue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                                darkGrayValue = Math.Max(0, Math.Min(255, darkGrayValue - 64));
                                newColor = Color.FromArgb(pixelColor.A, darkGrayValue, darkGrayValue, darkGrayValue);
                                break;
                            case 30: // Gold mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, pixelColor.G / 2, 0);
                                break;
                            case 31: // Silver mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, pixelColor.G / 2, pixelColor.B / 2);
                                break;
                            case 32: // Bronze mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R / 2, pixelColor.G / 4, 0);
                                break;
                            case 33: // Copper mode
                                newColor = Color.FromArgb(pixelColor.A, pixelColor.R, pixelColor.G / 3, 0);
                                break;
                            default:
                                newColor = Color.FromArgb(pixelColor.A, minColorValue, midColorValue, maxColorValue);
                                break;
                        }
                        newImage.SetPixel(x, y, newColor);
                    }
                }

                pictureBox1.Image = newImage;
                colorMode++;
            }
            else
            {
                MessageBox.Show("The clipboard does not contain an image.");
            }
            #endregion
        }

        #region PaintBox and Mirror Drawing and Lines
        private static Form PaintBoxForm = null;

        private void btPaintBox_Click(object sender, EventArgs e)
        {
            bool isLineDrawingActive = false;
            bool isDrawingActive = true;
            Point startPoint = Point.Empty;
            Point endPoint = Point.Empty; // Add this line at the beginning of your code
            Point lastPoint = Point.Empty;
            List<Point> points = new List<Point>();
            // Declare imageRect outside the event
            //Rectangle imageRect = new Rectangle();
            bool drawing = false;
            bool mirrorDrawing = false;
            Rectangle imageRect = Rectangle.Empty;

            // Create a bitmap to store the drawing
            Bitmap bitmap = null;

            Stack<Bitmap> bitmapHistory = new Stack<Bitmap>();
            // Add the current bitmap to the gradient after you draw
            if (bitmap != null)
            {
                bitmapHistory.Push((Bitmap)bitmap.Clone());
            }

            // Declare imageRect outside the event
            if (PaintBoxForm != null)
            {
                //MessageBox.Show("The form is already opened.");
                return;
            }

            PaintBoxForm = new Form
            {
                Text = "Paint Box",
                Size = new Size(900, 900),
                KeyPreview = true
            };

            // Set the icon of the Form
            if (Properties.Resources.paint_box_colors != null)
            {
                using (Bitmap bmp = Properties.Resources.paint_box_colors)
                {
                    IntPtr hIcon = bmp.GetHicon(); // Create an icon handle from the bitmap
                    Icon icon = Icon.FromHandle(hIcon); // Create an Icon from the handle
                    PaintBoxForm.Icon = icon;
                }
            }

            // Create a new picture box
            PictureBox pictureBox = new PictureBox();
            pictureBox.Location = new Point(50, 50);

            // Add the picture box to the form
            PaintBoxForm.Controls.Add(pictureBox);

            // Create a color dialog
            ColorDialog colorDialog = new ColorDialog();

            // Create a new label
            Label pixelLabel = new Label();
            pixelLabel.Text = "Pixel";
            pixelLabel.Location = new Point(464, 775); // Position it above the TextBox

            // Add the label to the shape
            PaintBoxForm.Controls.Add(pixelLabel);

            // Create a button to draw on the picture box
            Button drawButton = new Button();
            drawButton.Text = "Draw Line";
            drawButton.Location = new Point(50, 800);
            drawButton.Click += (s, e) =>
            // Toggle function for drawing lines
            drawButton.Click += (s, e) =>
            {
                isLineDrawingActive = !isLineDrawingActive;
                drawButton.Text = isLineDrawingActive ? "Stop Drawing Line" : "Draw Line";
                drawButton.BackColor = colorDialog.Color; // Sets the background color of the button to the selected color

                // Change the text color of the button to ensure it's readable
                drawButton.ForeColor = (colorDialog.Color.R * 0.299 + colorDialog.Color.G * 0.587 + colorDialog.Color.B * 0.114) > 186 ? Color.Black : Color.White;

                // Make isDrawingActive always the opposite of isLineDrawingActive
                isDrawingActive = !isLineDrawingActive;
            };

            // Create a button to paste an image from the clipboard to the picture box
            Button pasteButton = new Button();
            pasteButton.Text = "Paste";
            pasteButton.Location = new Point(132, 800);
            pasteButton.Click += (s, e) =>
            {
                if (Clipboard.ContainsImage())
                {
                    Image img = Clipboard.GetImage();
                    bitmap = new Bitmap(img);
                    pictureBox.Size = img.Size;

                    // Update imageRect within the event
                    imageRect.Size = img.Size;  // Update the size of the imageRect

                    // Update the picture box
                    pictureBox.Image = bitmap;

                    // Add the inserted bitmap to the gradient
                    if (bitmap != null)
                    {
                        bitmapHistory.Push((Bitmap)bitmap.Clone());
                    }
                }
                else
                {
                    MessageBox.Show("No image found on clipboard.");
                }
            };

            // Create a button to choose a color
            Button colorButton = new Button();
            colorButton.Text = "Choose Color";
            colorButton.Location = new Point(296, 800);
            colorButton.Click += (s, e) =>
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    drawButton.ForeColor = colorDialog.Color;
                }
            };

            // Create a button to copy the image to the clipboard
            Button copyButton = new Button();
            copyButton.Text = "Copy";
            copyButton.Location = new Point(214, 800);
            copyButton.Click += (s, e) =>
            {
                if (pictureBox.Image != null)
                {
                    Clipboard.SetImage(bitmap);
                }
                else
                {
                    MessageBox.Show("No image available to copy.");
                }
            };

            // Create a button for mirror drawing
            Button mirrorDrawButton = new Button();
            mirrorDrawButton.Text = "Mirror Draw";
            mirrorDrawButton.Location = new Point(378, 800);

            // Create a textbox to set the pen width
            TextBox penWidthTextBox = new TextBox();
            penWidthTextBox.Text = "1";
            penWidthTextBox.Location = new Point(464, 800);
            penWidthTextBox.Size = new Size(50, penWidthTextBox.Height); // Sets the width to 50 pixels

            // Create an undo button
            Button undoButton = new Button();
            undoButton.Text = "Undo";
            undoButton.Location = new Point(522, 800); // Position it next to the other buttons
            undoButton.Click += (s, e) =>
            {
                if (bitmapHistory.Count > 0)
                {
                    bitmap = bitmapHistory.Pop();
                    pictureBox.Image = bitmap;
                }
            };

            // Add the buttons and textbox to the form
            PaintBoxForm.Controls.Add(drawButton);
            PaintBoxForm.Controls.Add(pasteButton);
            PaintBoxForm.Controls.Add(colorButton);
            PaintBoxForm.Controls.Add(copyButton);
            PaintBoxForm.Controls.Add(mirrorDrawButton);
            PaintBoxForm.Controls.Add(penWidthTextBox);
            PaintBoxForm.Controls.Add(undoButton);

            // Add mouse down event to start drawing
            pictureBox.MouseDown += (s, e) =>
            {
                // Set the starting point by pressing the mouse button                
                if (isLineDrawingActive && imageRect.Contains(e.Location))
                {
                    drawing = true;
                    startPoint = e.Location;
                }
                else if (isDrawingActive)
                {
                    // Set the starting point by pressing the mouse button
                    if (imageRect.Contains(e.Location))
                    {
                        drawing = true;
                    }
                }
            };

            // Add mouse up event to stop drawing
            pictureBox.MouseUp += (s, e) =>
            {
                drawing = false;

                if (isLineDrawingActive && bitmap != null && startPoint != Point.Empty)
                {
                    drawing = false;

                    // Draw the final line on the original image
                    Graphics graphics = Graphics.FromImage(bitmap);
                    float penWidth = float.Parse(penWidthTextBox.Text);
                    graphics.DrawLine(new Pen(colorDialog.Color, penWidth), startPoint, e.Location);

                    // Update the picture box
                    pictureBox.Image = bitmap;

                    // Reset the start point
                    startPoint = Point.Empty;

                    // Add the current bitmap to the gradient after you draw
                    bitmapHistory.Push((Bitmap)bitmap.Clone());
                }
                else if (isDrawingActive && bitmap != null && imageRect.Contains(e.Location))
                {
                    // Reset the last point when the mouse is released
                    lastPoint = Point.Empty;
                    drawing = false;
                    bitmapHistory.Push((Bitmap)bitmap.Clone()); // Remove the last image from the history
                }
            };

            // Add mouse move event to draw when mouse is down
            pictureBox.MouseMove += (s, e) =>
            {
                //if (isLineDrawingActive && bitmap != null)
                if (isLineDrawingActive && drawing && imageRect.Contains(e.Location))
                {
                    ;
                    pictureBox.Invalidate();

                    if (startPoint != Point.Empty)
                    {
                        // Draw the line on the picture box, not the bitmap
                        Graphics graphics = pictureBox.CreateGraphics();
                        float penWidth = float.Parse(penWidthTextBox.Text);
                        graphics.DrawLine(new Pen(colorDialog.Color, penWidth), startPoint, e.Location);
                    }
                }
                else if (isDrawingActive)
                {
                    // Draw image when mouse is down
                    if (drawing && bitmap != null)
                    {
                        Graphics graphics = Graphics.FromImage(bitmap);
                        float penWidth = float.Parse(penWidthTextBox.Text);
                        graphics.FillEllipse(new SolidBrush(colorDialog.Color), e.X - penWidth / 2, e.Y - penWidth / 2, penWidth, penWidth);

                        if (mirrorDrawing)
                        {
                            int mirrorX = bitmap.Width - e.X;
                            graphics.FillEllipse(new SolidBrush(colorDialog.Color), mirrorX - penWidth / 2, e.Y - penWidth / 2, penWidth, penWidth);
                        }
                        // Aktualisieren Sie den letzten Punkt
                        lastPoint = e.Location;

                        // Update the picture box
                        pictureBox.Image = bitmap;
                    }
                }
            };

            mirrorDrawButton.Click += (s, e) =>
            {
                mirrorDrawing = !mirrorDrawing;
                mirrorDrawButton.Text = mirrorDrawing ? "Stop Mirror Draw" : "Mirror Draw";
                if (bitmap != null)
                {
                    bitmapHistory.Push((Bitmap)bitmap.Clone()); // Remove last image from history
                }
            };

            // Add keydown event to handle Ctrl+V and Ctrl+X
            PaintBoxForm.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.V) // Ctrl+V
                {
                    if (Clipboard.ContainsImage())
                    {
                        Image img = Clipboard.GetImage();
                        bitmap = new Bitmap(img);
                        pictureBox.Size = img.Size;
                        imageRect.Size = img.Size;

                        // Update the picture box
                        pictureBox.Image = bitmap;

                        // Fügen Sie das eingefügte Bitmap zum Verlauf hinzu
                        if (bitmap != null)
                        {
                            bitmapHistory.Push((Bitmap)bitmap.Clone());
                        }
                    }
                    else
                    {
                        MessageBox.Show("No image found on clipboard.");
                    }
                }
                else if (e.Control && e.KeyCode == Keys.X) // Ctrl+X
                {
                    if (pictureBox.Image != null)
                    {
                        Clipboard.SetImage(bitmap);
                        pictureBox.Image = bitmap;
                    }
                    else
                    {
                        MessageBox.Show("No image found on clipboard..");
                    }
                }
            };

            // Create a timer
            Timer timer = new Timer();
            timer.Interval = 100; // Set the interval to 100 milliseconds

            Bitmap tempBitmap = null; // Temporary bitmap for the line preview

            timer.Tick += (s, e) =>
            {
                if (isLineDrawingActive && bitmap != null && drawing)
                {
                    // Get the current position of the mouse cursor
                    Point currentPosition = pictureBox.PointToClient(Cursor.Position);

                    // Make a copy of the current image
                    tempBitmap = new Bitmap(bitmap);

                    // Draw on the temporary image
                    Graphics graphics = Graphics.FromImage(tempBitmap);
                    float penWidth = float.Parse(penWidthTextBox.Text);
                    graphics.DrawLine(new Pen(colorDialog.Color, penWidth), startPoint, currentPosition);

                    // Update the PictureBox with the temporary image
                    pictureBox.Image = tempBitmap;
                }
            };

            // Start the timer
            timer.Start();

            // Add an event to reset the static variable when the form closes
            PaintBoxForm.FormClosed += (s, e) => { PaintBoxForm = null; };

            // Show the form
            PaintBoxForm.Show();
        }
        #endregion


        //Saves the position.
        private void TextureCutter_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.FormTextureCutter = this.Location;
            Properties.Settings.Default.Save();
        }

        //Load position.
        private void TextureCutterForm_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.FormTextureCutter != Point.Empty)
            {
                this.Location = Properties.Settings.Default.FormTextureCutter;
            }
        }

        #region Lines
        private void DrawLines(Graphics g, List<Point> points)
        {
            if (points.Count > 1)
            {
                g.DrawLines(Pens.Yellow, points.ToArray());
            }
        }
        private Point lastPointLines = Point.Empty; // Add this field to keep track of the last point

        private void pictureBox1_MouseDownLines(object sender, MouseEventArgs e)
        {
            if (checkBoxLines.Checked && e.Button == MouseButtons.Left)
            {
                if (lastPointLines.IsEmpty) // If this is the first point
                {
                    points.Clear();
                    points.Add(e.Location);
                    lastPointLines = e.Location;
                }
                else // If this is not the first point
                {
                    points.Add(e.Location);
                }
            }
        }

        #endregion
    }
}

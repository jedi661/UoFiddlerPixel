// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  * 
//  * "THE WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a Wine in return.
//  *
//  ***************************************************************************/

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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace UoFiddler.Controls.Forms
{
    public partial class BildFusionForm : Form
    {
        public BildFusionForm()
        {
            InitializeComponent();            
        }

        // Global variables to store the background image and overlay image
        private Bitmap originalBackgroundImage;
        private Bitmap originalOverlayImage;
        private Bitmap displayedImage;
        private PictureBox currentPictureBox;

        //not yet awarded
        private Bitmap originalBackgroundImage64;
        private Bitmap originalBackgroundImage128;
        private Bitmap originalBackgroundImage256;
        private Bitmap originalOverlayImage64;
        private Bitmap originalOverlayImage128;
        private Bitmap originalOverlayImage256;
        //

        private Bitmap rotatedBackgroundImage64;
        private Bitmap rotatedBackgroundImage128;
        private Bitmap rotatedBackgroundImage256;
        private Bitmap rotatedOverlayImage64;
        private Bitmap rotatedOverlayImage128;
        private Bitmap rotatedOverlayImage256;

        private Bitmap originalForegroundImage;

        #region btLoad
        private void btLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Images|*.bmp;*.png;*.jpg;*.jpeg;*.gif";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Load the background image
                originalBackgroundImage = new Bitmap(openFileDialog1.FileName);

                // Display a message
                MessageBox.Show("Please select the second image.");

                OpenFileDialog openFileDialog2 = new OpenFileDialog();
                openFileDialog2.Filter = "Images|*.bmp;*.png;*.jpg;*.jpeg;*.gif";

                if (openFileDialog2.ShowDialog() == DialogResult.OK)
                {
                    // Load the second image
                    originalOverlayImage = new Bitmap(openFileDialog2.FileName);

                    // Make all #000000 (black) pixels in the second image transparent
                    originalOverlayImage.MakeTransparent(Color.Black);

                    // Draw the overlay image onto the background image
                    DrawOverlayOnBackground();

                    // Put the resulting image into your PictureBox
                    pictureBox64x64.Image = displayedImage;
                }
            }
        }
        #endregion

        #region DrawOverlayOnBackground
        private void DrawOverlayOnBackground()
        {
            // Make a copy of the background image
            displayedImage = (Bitmap)originalBackgroundImage.Clone();

            // Create a Graphics object from the background image
            Graphics g = Graphics.FromImage(displayedImage);

            // Draw the overlay image onto the background image
            g.DrawImage(originalOverlayImage, new Rectangle(0, 0, GetSelectedSize().Width, GetSelectedSize().Height));

            // Release resources
            g.Dispose();

            // Set the image in the PictureBox to null
            GetCurrentPictureBox().Image = null;

            // Put the resulting image into your PictureBox
            GetCurrentPictureBox().Image = displayedImage;
        }
        #endregion

        #region btLeftBackgroundImage
        private void btLeftBackgroundImage_Click(object sender, EventArgs e)
        {
            // Check if there is a background image
            if (originalBackgroundImage != null)
            {
                // Rotate the background image 90 degrees to the left
                originalBackgroundImage.RotateFlip(RotateFlipType.Rotate270FlipNone);

                // Save the rotated state
                if (checkBox64x64.Checked)
                {
                    rotatedBackgroundImage64 = originalBackgroundImage;
                }
                else if (checkBox128x128.Checked)
                {
                    rotatedBackgroundImage128 = originalBackgroundImage;
                }
                else if (checkBox256x256.Checked)
                {
                    rotatedBackgroundImage256 = originalBackgroundImage;
                }

                // Draw the overlay image onto the rotated background image
                DrawOverlayOnBackground();

                // Update the PictureBox
                pictureBox64x64.Refresh();
            }
        }
        #endregion

        #region btRightBackgroundImage
        private void btRightBackgroundImage_Click(object sender, EventArgs e)
        {
            // Check if there is a background image
            if (originalBackgroundImage != null)
            {
                // Rotate the wallpaper 90 degrees to the right
                originalBackgroundImage.RotateFlip(RotateFlipType.Rotate90FlipNone);

                // Save the rotated state
                if (checkBox64x64.Checked)
                {
                    rotatedBackgroundImage64 = originalBackgroundImage;
                }
                else if (checkBox128x128.Checked)
                {
                    rotatedBackgroundImage128 = originalBackgroundImage;
                }
                else if (checkBox256x256.Checked)
                {
                    rotatedBackgroundImage256 = originalBackgroundImage;
                }

                // Draw the overlay image onto the rotated background image
                DrawOverlayOnBackground();

                // Update the PictureBox
                pictureBox64x64.Refresh();
            }
        }
        #endregion

        #region btLeftOverlayImage
        private void btLeftOverlayImage_Click(object sender, EventArgs e)
        {
            // Check if there is an overlay image
            if (originalOverlayImage != null)
            {
                // Rotate the overlay image 90 degrees to the left
                originalOverlayImage.RotateFlip(RotateFlipType.Rotate270FlipNone);

                // Save the rotated state
                if (checkBox64x64.Checked)
                {
                    rotatedOverlayImage64 = originalOverlayImage;
                }
                else if (checkBox128x128.Checked)
                {
                    rotatedOverlayImage128 = originalOverlayImage;
                }
                else if (checkBox256x256.Checked)
                {
                    rotatedOverlayImage256 = originalOverlayImage;
                }

                // Draw the rotated overlay image onto the background image
                DrawOverlayOnBackground();

                // Update the PictureBox
                pictureBox64x64.Refresh();
            }
        }
        #endregion

        #region btRightOverlayImage
        private void btRightOverlayImage_Click(object sender, EventArgs e)
        {
            // Check if there is an overlay image
            if (originalOverlayImage != null)
            {
                // Rotate the overlay image 90 degrees to the right
                originalOverlayImage.RotateFlip(RotateFlipType.Rotate90FlipNone);

                // Save the rotated state
                if (checkBox64x64.Checked)
                {
                    rotatedOverlayImage64 = originalOverlayImage;
                }
                else if (checkBox128x128.Checked)
                {
                    rotatedOverlayImage128 = originalOverlayImage;
                }
                else if (checkBox256x256.Checked)
                {
                    rotatedOverlayImage256 = originalOverlayImage;
                }

                // Draw the rotated overlay image onto the background image
                DrawOverlayOnBackground();

                // Update the PictureBox
                pictureBox64x64.Refresh();
            }
        }
        #endregion

        #region  Save
        private void btSave_Click(object sender, EventArgs e)
        {
            PictureBox currentBox;
            if (checkBox64x64.Checked)
            {
                currentBox = pictureBox64x64;
            }
            else if (checkBox128x128.Checked)
            {
                currentBox = pictureBox128x128;
            }
            else if (checkBox256x256.Checked)
            {
                currentBox = pictureBox256x256;
            }
            else
            {
                // Standard PictureBox
                currentBox = pictureBox64x64;
            }

            // Check if there is an image in the PictureBox
            if (currentBox.Image != null)
            {
                string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string directory = Path.Combine(programDirectory, "tempGrafic");

                // Generate file name with "TextureTile", date and time
                string dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string filename = Path.Combine(directory, $"TextureTile_{dateTime}.bmp");

                // Make sure the directory exists
                Directory.CreateDirectory(directory);

                // Save the image as a BMP file
                currentBox.Image.Save(filename, System.Drawing.Imaging.ImageFormat.Bmp);

                // Play the sound
                string soundFilePath = Path.Combine(programDirectory, "Sound.wav");
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(soundFilePath);
                player.Play();
            }
            else
            {
                MessageBox.Show("There is no image to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region DrawBackgroundOnImage
        private void DrawBackgroundOnImage()
        {
            // Make a copy of the background image
            displayedImage = (Bitmap)originalBackgroundImage.Clone();

            // Check which CheckBox is selected and load the image into the corresponding PictureBox
            if (checkBox64x64.Checked)
            {
                pictureBox64x64.Image = new Bitmap(displayedImage, new Size(64, 64));
            }
            else if (checkBox128x128.Checked)
            {
                pictureBox128x128.Image = new Bitmap(displayedImage, new Size(128, 128));
            }
            else if (checkBox256x256.Checked)
            {
                pictureBox256x256.Image = new Bitmap(displayedImage, new Size(256, 256));
            }
            else
            {
                pictureBox64x64.Image = displayedImage;
            }
        }
        #endregion

        #region DrawOverlayOnImag
        private void DrawOverlayOnImage()
        {
            // Make sure the wallpaper has loaded
            if (displayedImage == null)
            {
                MessageBox.Show("Please load a background image first.");
                return;
            }

            // Create a Graphics object from the background image
            Graphics g = Graphics.FromImage(displayedImage);

            // Determine the size of the rectangle based on the selected CheckBox
            Size size = GetSelectedSize();

            // Draw the overlay image onto the background image
            g.DrawImage(originalOverlayImage, new Rectangle(0, 0, size.Width, size.Height));

            // Release resources
            g.Dispose();

            // Set the image in the PictureBox to null
            GetCurrentPictureBox().Image = null;

            // Put the resulting image into your PictureBox
            GetCurrentPictureBox().Image = displayedImage;
        }
        #endregion

        #region GetSelectedSize
        private Size GetSelectedSize()
        {
            if (checkBox64x64.Checked)
            {
                return new Size(64, 64);
            }
            else if (checkBox128x128.Checked)
            {
                return new Size(128, 128);
            }
            else if (checkBox256x256.Checked)
            {
                return new Size(256, 256);
            }
            else
            {
                return new Size(64, 64); // Standard size
            }
        }
        #endregion

        #region PictureBox GetCurrentPictureBox
        private PictureBox GetCurrentPictureBox()
        {
            if (checkBox64x64.Checked)
            {
                return pictureBox64x64;
            }
            else if (checkBox128x128.Checked)
            {
                return pictureBox128x128;
            }
            else if (checkBox256x256.Checked)
            {
                return pictureBox256x256;
            }
            else
            {
                return pictureBox64x64; // Standard PictureBox
            }
        }
        #endregion

        #region btLoadSingleBackground
        private void btLoadSingleBackground_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Images|*.bmp;*.png;*.jpg;*.jpeg;*.gif";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Load the background image
                originalBackgroundImage = new Bitmap(openFileDialog.FileName);

                // Draw the background image on the image
                DrawBackgroundOnImage();

                // Check which CheckBox is selected and load the image into the corresponding PictureBox
                if (checkBox64x64.Checked)
                {
                    pictureBox64x64.Image = new Bitmap(displayedImage, new Size(64, 64));
                }
                else if (checkBox128x128.Checked)
                {
                    pictureBox128x128.Image = new Bitmap(displayedImage, new Size(128, 128));
                }
                else if (checkBox256x256.Checked)
                {
                    pictureBox256x256.Image = new Bitmap(displayedImage, new Size(256, 256));
                }
            }
        }
        #endregion

        #region btLoadSingleForeground
        private void btLoadSingleForeground_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Images|*.bmp;*.png;*.jpg;*.jpeg;*.gif";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Load the foreground image
                originalOverlayImage = new Bitmap(openFileDialog.FileName);

                // Make all #000000 (black) pixels in the foreground image transparent
                originalOverlayImage.MakeTransparent(Color.Black);

                // Make sure the wallpaper has loaded
                if (originalBackgroundImage == null)
                {
                    MessageBox.Show("Please load a background image first.");
                    return;
                }

                // Draw the foreground image onto the background image
                DrawOverlayOnImage();

                // Check which CheckBox is selected and load the image into the corresponding PictureBox
                if (checkBox64x64.Checked)
                {
                    pictureBox64x64.Image = new Bitmap(displayedImage, new Size(64, 64));
                }
                else if (checkBox128x128.Checked)
                {
                    pictureBox128x128.Image = new Bitmap(displayedImage, new Size(128, 128));
                }
                else if (checkBox256x256.Checked)
                {
                    pictureBox256x256.Image = new Bitmap(displayedImage, new Size(256, 256));
                }
            }
        }
        #endregion

        #region Checkboxen checkBox64x64 checkBox128x128 checkBox256x256
        private void checkBox64x64_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox64x64.Checked)
            {
                //originalBackgroundImage = originalBackgroundImage64;
                //originalOverlayImage = originalOverlayImage64;

                // Laden Sie den gespeicherten gedrehten Zustand
                originalBackgroundImage = rotatedBackgroundImage64;
                originalOverlayImage = rotatedOverlayImage64;

                checkBox128x128.CheckedChanged -= checkBox128x128_CheckedChanged;
                checkBox256x256.CheckedChanged -= checkBox256x256_CheckedChanged;
                checkBox128x128.Checked = false;
                checkBox256x256.Checked = false;
                checkBox128x128.CheckedChanged += checkBox128x128_CheckedChanged;
                checkBox256x256.CheckedChanged += checkBox256x256_CheckedChanged;

                if (currentPictureBox != null)
                {
                    UpdatePictureBoxState(currentPictureBox, currentPictureBox.Image);
                }

                currentPictureBox = pictureBox64x64;
                PictureBoxState state = GetStoredPictureBoxState(currentPictureBox);

                if (state != null)
                {
                    currentPictureBox.Image = state.Image;
                    currentPictureBox.Image.RotateFlip(state.Rotation);
                }
            }
        }
        private void checkBox128x128_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox128x128.Checked)
            {
                //originalBackgroundImage = originalBackgroundImage128; // Wieder raus genommen
                //originalOverlayImage = originalOverlayImage128;

                originalBackgroundImage = rotatedBackgroundImage128;
                originalOverlayImage = rotatedOverlayImage128;

                checkBox64x64.CheckedChanged -= checkBox64x64_CheckedChanged;
                checkBox256x256.CheckedChanged -= checkBox256x256_CheckedChanged;
                checkBox64x64.Checked = false;
                checkBox256x256.Checked = false;
                checkBox64x64.CheckedChanged += checkBox64x64_CheckedChanged;
                checkBox256x256.CheckedChanged += checkBox256x256_CheckedChanged;

                if (currentPictureBox != null)
                {
                    UpdatePictureBoxState(currentPictureBox, currentPictureBox.Image);
                }

                currentPictureBox = pictureBox128x128;
                PictureBoxState state = GetStoredPictureBoxState(currentPictureBox);

                if (state != null)
                {
                    currentPictureBox.Image = state.Image;
                    currentPictureBox.Image.RotateFlip(state.Rotation);
                }
            }
        }
        private void checkBox256x256_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox256x256.Checked)
            {
                //originalBackgroundImage = originalBackgroundImage256;
                //originalOverlayImage = originalOverlayImage256;

                originalBackgroundImage = rotatedBackgroundImage256;
                originalOverlayImage = rotatedOverlayImage256;

                checkBox64x64.CheckedChanged -= checkBox64x64_CheckedChanged;
                checkBox128x128.CheckedChanged -= checkBox128x128_CheckedChanged;
                checkBox64x64.Checked = false;
                checkBox128x128.Checked = false;
                checkBox64x64.CheckedChanged += checkBox64x64_CheckedChanged;
                checkBox128x128.CheckedChanged += checkBox128x128_CheckedChanged;

                if (currentPictureBox != null)
                {
                    UpdatePictureBoxState(currentPictureBox, currentPictureBox.Image);
                }

                currentPictureBox = pictureBox256x256;
                PictureBoxState state = GetStoredPictureBoxState(currentPictureBox);

                if (state != null)
                {
                    currentPictureBox.Image = state.Image;
                    currentPictureBox.Image.RotateFlip(state.Rotation);
                }
            }
        }
        #endregion

        #region class PictureBoxState
        public class PictureBoxState
        {
            public Image Image { get; set; }
            public RotateFlipType Rotation { get; set; }
        }

        private Dictionary<PictureBox, PictureBoxState> pictureBoxStates = new Dictionary<PictureBox, PictureBoxState>();

        private void UpdatePictureBoxState(PictureBox pictureBox, Image image, RotateFlipType rotation = RotateFlipType.RotateNoneFlipNone)
        {
            PictureBoxState state = new PictureBoxState { Image = image, Rotation = rotation };
            if (pictureBoxStates.ContainsKey(pictureBox))
            {
                pictureBoxStates[pictureBox] = state;
            }
            else
            {
                pictureBoxStates.Add(pictureBox, state);
            }
        }
        private PictureBoxState GetStoredPictureBoxState(PictureBox pictureBox)
        {
            if (pictureBoxStates.ContainsKey(pictureBox))
            {
                return pictureBoxStates[pictureBox];
            }
            else
            {
                return null;
            }
        }

        private void RotateAndSaveImageState(PictureBox pictureBox, RotateFlipType rotation)
        {
            pictureBox.Image.RotateFlip(rotation);
            UpdatePictureBoxState(pictureBox, pictureBox.Image, rotation);
        }
        #endregion

        #region Load comboBox
        // Create an ImageList object
        private ImageList imageList = new ImageList();

        private void btLoadRubberStamp_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string[] files = Directory.GetFiles(fbd.SelectedPath);

                    tbFileDir.Text = fbd.SelectedPath;

                    comboBoxRubberStamp.Items.Clear();
                    imageList.Images.Clear();

                    foreach (var file in files)
                    {
                        string extension = Path.GetExtension(file).ToLower();
                        if (extension == ".bmp" || extension == ".png" || extension == ".tiff" || extension == ".jpg")
                        {
                            // Add the image to the ImageList
                            imageList.Images.Add(Image.FromFile(file));
                            comboBoxRubberStamp.Items.Add(Path.GetFileName(file));
                        }
                    }
                }
            }
        }
        #endregion

        #region btViewLoad
        private void btViewLoad_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = "C:\\"; // Set your default directory path here
                ofd.Filter = "Images|*.bmp;*.png;*.jpg;*.jpeg;*.gif";
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    tbFileDir.Text = Path.GetDirectoryName(ofd.FileNames[0]);

                    comboBoxRubberStamp.Items.Clear();
                    imageList.Images.Clear();

                    foreach (var file in ofd.FileNames)
                    {
                        // Add the image to the ImageList
                        imageList.Images.Add(Image.FromFile(file));
                        comboBoxRubberStamp.Items.Add(Path.GetFileName(file));
                    }
                }
            }
        }
        #endregion

        #region comboBoxRubberStamp_DrawItem
        // Override DrawItem event handling
        private void comboBoxRubberStamp_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return; // When no item is selected

            // Draw the image and text for each element
            e.DrawBackground();
            e.Graphics.DrawImage(imageList.Images[e.Index], e.Bounds.Left, e.Bounds.Top);
            e.Graphics.DrawString(comboBoxRubberStamp.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left + imageList.ImageSize.Width, e.Bounds.Top);
            e.DrawFocusRectangle();
        }
        #endregion

        #region comboBoxRubberStamp_SelectedIndexChanged
        private void comboBoxRubberStamp_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedFile = Path.Combine(tbFileDir.Text, comboBoxRubberStamp.SelectedItem.ToString());
            originalOverlayImage = new Bitmap(selectedFile);

            // Make all #000000 (black) pixels in the image transparent
            originalOverlayImage.MakeTransparent(Color.Black);

            // Draw the overlay image onto the background image
            DrawOverlayOnImage();

            // Check which CheckBox is selected and load the image into the corresponding PictureBox
            if (checkBox64x64.Checked)
            {
                pictureBox64x64.Image = new Bitmap(displayedImage, new Size(64, 64));
            }
            else if (checkBox128x128.Checked)
            {
                pictureBox128x128.Image = new Bitmap(displayedImage, new Size(128, 128));
            }
            else if (checkBox256x256.Checked)
            {
                pictureBox256x256.Image = new Bitmap(displayedImage, new Size(256, 256));
            }
        }
        #endregion

        #region trackBarFading_Scroll
        private void trackBarFading_Scroll(object sender, EventArgs e)
        {
            // Check if there is an overlay image
            if (originalOverlayImage != null && originalBackgroundImage != null)
            {
                // Create a temporary copy of the overlay image
                Bitmap tempOverlayImage = new Bitmap(originalOverlayImage);

                // Invert the value of the TrackBar to calculate transparency
                int transparency = 255 - trackBarFading.Value;

                // Update the label with the current TrackBar value
                lbNr.Text = trackBarFading.Value.ToString();

                // Go through each pixel in the image
                for (int y = 0; y < tempOverlayImage.Height; y++)
                {
                    for (int x = 0; x < tempOverlayImage.Width; x++)
                    {
                        // Get the original color of the pixel
                        Color originalColor = tempOverlayImage.GetPixel(x, y);

                        // Check if the original color is black
                        if (originalColor.R == 0 && originalColor.G == 0 && originalColor.B == 0)
                        {
                            // Set the pixel to transparent
                            tempOverlayImage.SetPixel(x, y, Color.Transparent);
                        }
                        else
                        {
                            // Create a new color with the original color and the calculated transparency
                            Color newColor = Color.FromArgb(transparency, originalColor.R, originalColor.G, originalColor.B);

                            // Set the pixel to the new color
                            tempOverlayImage.SetPixel(x, y, newColor);
                        }
                    }
                }

                // Make a copy of the background image
                Bitmap combinedImage = new Bitmap(originalBackgroundImage);

                // Create a Graphics object from the combined image
                using (Graphics g = Graphics.FromImage(combinedImage))
                {
                    // Draw the modified foreground image onto the combined image
                    g.DrawImage(tempOverlayImage, new Rectangle(0, 0, tempOverlayImage.Width, tempOverlayImage.Height));
                }

                // Update the originalOverlayImage with the changed image
                //originalOverlayImage = tempOverlayImage;

                // Load the combined image into the PictureBox
                GetCurrentPictureBox().Image = combinedImage;
            }
        }
        #endregion

        #region TrackBarColor  
        private void trackBarColor_Scroll(object sender, EventArgs e)
        {
            if (originalOverlayImage != null && originalBackgroundImage != null)
            {
                Color newColor = Color.Empty;

                for (int y = 0; y < originalOverlayImage.Height; y++)
                {
                    for (int x = 0; x < originalOverlayImage.Width; x++)
                    {
                        Color originalColor = originalOverlayImage.GetPixel(x, y);

                        if (originalColor.R == 0 && originalColor.G == 0 && originalColor.B == 0)
                        {
                            continue;
                        }

                        double hue, saturation, lightness;
                        RgbToHsl(originalColor, out hue, out saturation, out lightness);

                        // Change the color tone based on the inverted value of the TrackBar
                        double hueChange = (255 - trackBarColor.Value) / 255.0; // This will now be a value between 0 and 1
                        hue = (hue + hueChange) % 1; // The modulo operator ensures that the hue is always between 0 and 1

                        newColor = HslToRgb(hue, saturation, lightness);

                        originalOverlayImage.SetPixel(x, y, newColor);
                    }
                }

                trackBarLabel.Text = $"Color position: {trackBarColor.Value}, R = {newColor.R}, G = {newColor.G}, B = {newColor.B}";

                Bitmap combinedImage = new Bitmap(originalBackgroundImage);

                using (Graphics g = Graphics.FromImage(combinedImage))
                {
                    g.DrawImage(originalOverlayImage, new Rectangle(0, 0, originalOverlayImage.Width, originalOverlayImage.Height));
                }

                GetCurrentPictureBox().Image = combinedImage;
            }
        }

        #region RgbToHsl
        private void RgbToHsl(Color rgb, out double h, out double s, out double l)
        {
            // Normalize the RGB values
            double r = rgb.R / 255.0;
            double g = rgb.G / 255.0;
            double b = rgb.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            // Calculate the brightness
            l = (max + min) / 2.0;

            // Calculate saturation
            if (max == min)
            {
                s = 0;
                h = 0; // It's actually undefined
            }
            else
            {
                double diff = max - min;
                s = (l > 0.5) ? diff / (2.0 - max - min) : diff / (max + min);

                // Calculate the color tone
                if (max == r)
                {
                    h = (g - b) / diff + (g < b ? 6 : 0);
                }
                else if (max == g)
                {
                    h = (b - r) / diff + 2;
                }
                else
                {
                    h = (r - g) / diff + 4;
                }

                h /= 6;
            }
        }
        #endregion

        #region HslToRgb
        private Color HslToRgb(double h, double s, double l)
        {
            double r, g, b;

            if (s == 0)
            {
                r = g = b = l; // Grayscale
            }
            else
            {
                Func<double, double, double, double> hueToRgb = (p, q, t) =>
                {
                    if (t < 0) t += 1;
                    if (t > 1) t -= 1;
                    if (t < 1 / 6.0) return p + (q - p) * 6 * t;
                    if (t < 1 / 2.0) return q;
                    if (t < 2 / 3.0) return p + (q - p) * (2 / 3.0 - t) * 6;
                    return p;
                };

                double q = (l < 0.5) ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;

                r = hueToRgb(p, q, h + 1 / 3.0);
                g = hueToRgb(p, q, h);
                b = hueToRgb(p, q, h - 1 / 3.0);
            }

            return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }
        #endregion
        #endregion

        #region btMirror
        private void btMirror_Click(object sender, EventArgs e)
        {
            // Check if there is an overlay image
            if (originalOverlayImage != null)
            {
                // Flip the overlay image horizontally
                originalOverlayImage.RotateFlip(RotateFlipType.RotateNoneFlipX);

                // Draw the mirrored overlay image onto the background image
                DrawOverlayOnImage();

                // Check which CheckBox is selected and load the image into the corresponding PictureBox
                if (checkBox64x64.Checked)
                {
                    pictureBox64x64.Image = new Bitmap(displayedImage, new Size(64, 64));
                }
                else if (checkBox128x128.Checked)
                {
                    pictureBox128x128.Image = new Bitmap(displayedImage, new Size(128, 128));
                }
                else if (checkBox256x256.Checked)
                {
                    pictureBox256x256.Image = new Bitmap(displayedImage, new Size(256, 256));
                }
            }
        }
        #endregion

        #region btBackgroundImageLoad
        // Create a second ImageList for the background images
        private ImageList backgroundImageList = new ImageList();

        private void btBackgroundImageLoad_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string[] files = Directory.GetFiles(fbd.SelectedPath);
                    tbDirBackgroundImage.Text = fbd.SelectedPath;
                    comboBoxBackgroundImage.Items.Clear();
                    backgroundImageList.Images.Clear(); // Use the new ImageList

                    foreach (var file in files)
                    {
                        string extension = System.IO.Path.GetExtension(file).ToLower();
                        if (extension == ".bmp" || extension == ".png" || extension == ".tiff" || extension == ".jpg")
                        {
                            backgroundImageList.Images.Add(Image.FromFile(file)); // Add the image to the new ImageList
                            comboBoxBackgroundImage.Items.Add(System.IO.Path.GetFileName(file));
                        }
                    }
                }
            }
        }
        #endregion

        #region comboBoxBackgroundImage_DrawItem
        private void comboBoxBackgroundImage_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return; // When no item is selected

            // Draw the image and text for each element
            e.DrawBackground();
            e.Graphics.DrawImage(backgroundImageList.Images[e.Index], e.Bounds.Left, e.Bounds.Top);
            e.Graphics.DrawString(comboBoxBackgroundImage.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left + backgroundImageList.ImageSize.Width, e.Bounds.Top);
            e.DrawFocusRectangle();
        }
        #endregion

        #region comboBoxBackgroundImage
        private void comboBoxBackgroundImage_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedFile = Path.Combine(tbDirBackgroundImage.Text, comboBoxBackgroundImage.SelectedItem.ToString());
            originalBackgroundImage = new Bitmap(selectedFile);
            displayedImage = new Bitmap(originalBackgroundImage); // Update the displayedImage variable

            if (checkBox64x64.Checked)
            {
                pictureBox64x64.Image = new Bitmap(originalBackgroundImage, new Size(64, 64));
            }
            else if (checkBox128x128.Checked)
            {
                pictureBox128x128.Image = new Bitmap(originalBackgroundImage, new Size(128, 128));
            }
            else if (checkBox256x256.Checked)
            {
                pictureBox256x256.Image = new Bitmap(originalBackgroundImage, new Size(256, 256));
            }
        }
        #endregion

        #region btViewLoadBackground
        private void btViewLoadBackground_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = "C:\\"; // Set your default directory path here
                ofd.Filter = "Images|*.bmp;*.png;*.jpg;*.jpeg;*.gif";
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    tbDirBackgroundImage.Text = System.IO.Path.GetDirectoryName(ofd.FileNames[0]);

                    comboBoxBackgroundImage.Items.Clear();
                    backgroundImageList.Images.Clear();

                    foreach (var file in ofd.FileNames)
                    {
                        // Add the image to the ImageList
                        backgroundImageList.Images.Add(Image.FromFile(file));
                        comboBoxBackgroundImage.Items.Add(System.IO.Path.GetFileName(file));
                    }
                }
            }
        }
        #endregion

        #region btDirSaveOrder
        private void btDirSaveOrder_Click(object sender, EventArgs e)
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

        #region trackBarSharp_ValueChanged
        private void trackBarSharp_ValueChanged(object sender, EventArgs e)
        {
            // Check if there is an overlay image
            if (originalOverlayImage != null)
            {
                // Check if the value of the TrackBar is 1
                if (trackBarSharp.Value == 1)
                {
                    // Restore the original foreground image
                    originalOverlayImage = new Bitmap(originalForegroundImage);
                }
                else
                {
                    // Save the original foreground image before making any changes to it
                    originalForegroundImage = new Bitmap(originalOverlayImage);

                    // Apply the sharpening function to the originalOverlayImage
                    // Use the TrackBar value as the sharpening level
                    SharpenImage(originalOverlayImage, trackBarSharp.Value);
                }

                // Draw the sharpened overlay image onto the background image
                DrawOverlayOnImage();

                // Check which CheckBox is selected and load the image into the corresponding PictureBox
                if (checkBox64x64.Checked)
                {
                    pictureBox64x64.Image = new Bitmap(displayedImage, new Size(64, 64));
                }
                else if (checkBox128x128.Checked)
                {
                    pictureBox128x128.Image = new Bitmap(displayedImage, new Size(128, 128));
                }
                else if (checkBox256x256.Checked)
                {
                    pictureBox256x256.Image = new Bitmap(displayedImage, new Size(256, 256));
                }

                // Update the PictureBox object to reflect the changes
                GetCurrentPictureBox().Refresh();
            }
        }
        #endregion

        #region SharpenImage
        private void SharpenImage(Bitmap image, int sharpness)
        {
            // Define the Laplace filter
            double[,] laplaceFilter = new double[,]
            {
                { -1, -1, -1 },
                { -1,  8, -1 },
                { -1, -1, -1 }
            };

            // Scale the filter based on sharpening level
            double scale = sharpness / 255.0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    laplaceFilter[i, j] *= scale;
                }
            }

            // Apply the filter to the image
            ApplyFilter(image, laplaceFilter, scale);
        }
        #endregion

        #region ApplyFilter
        private void ApplyFilter(Bitmap image, double[,] filter, double alpha)
        {
            int width = image.Width;
            int height = image.Height;

            // Make a copy of the image
            Bitmap copy = new Bitmap(image);
            Bitmap sharpenedImage = new Bitmap(image);

            // Loop through every pixel in the image
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    double red = 0.0, green = 0.0, blue = 0.0;

                    // Apply the filter to the surrounding pixels
                    for (int filterX = 0; filterX < 3; filterX++)
                    {
                        for (int filterY = 0; filterY < 3; filterY++)
                        {
                            Color pixelColor = copy.GetPixel(x + filterX - 1, y + filterY - 1);

                            // Check if the pixel is black
                            if (pixelColor.R == 0 && pixelColor.G == 0 && pixelColor.B == 0)
                            {
                                continue;
                            }

                            red += pixelColor.R * filter[filterX, filterY];
                            green += pixelColor.G * filter[filterX, filterY];
                            blue += pixelColor.B * filter[filterX, filterY];
                        }
                    }

                    // Limit the color values ​​to the range 0-255
                    red = Math.Min(Math.Max(red, 0), 255);
                    green = Math.Min(Math.Max(green, 0), 255);
                    blue = Math.Min(Math.Max(blue, 0), 255);

                    // Blend the sharpened image with the original image
                    Color originalColor = image.GetPixel(x, y);
                    red = Math.Max(alpha * red + (1 - alpha) * originalColor.R, originalColor.R);
                    green = Math.Max(alpha * green + (1 - alpha) * originalColor.G, originalColor.G);
                    blue = Math.Max(alpha * blue + (1 - alpha) * originalColor.B, originalColor.B);


                    // Check whether the original pixel is black before setting it
                    if (originalColor.R != 0 || originalColor.G != 0 || originalColor.B != 0)
                    {
                        sharpenedImage.SetPixel(x, y, Color.FromArgb((int)red, (int)green, (int)blue));
                    }
                }
            }

            // Replace the non-black pixels of the original image with the corresponding pixels of the sharpened image
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color originalColor = image.GetPixel(x, y);

                    // Check if the pixel is black
                    if (originalColor.R != 0 || originalColor.G != 0 || originalColor.B != 0)
                    {
                        image.SetPixel(x, y, sharpenedImage.GetPixel(x, y));
                    }
                }
            }
        }
        #endregion

        #region btClipBoard
        private void btClipboard_Click(object sender, EventArgs e)
        {
            // Check whether an image is displayed in the PictureBox
            if (GetCurrentPictureBox().Image != null)
            {
                // Copy the image to the clipboard
                Clipboard.SetImage(GetCurrentPictureBox().Image);
            }
        }
        #endregion

        #region BtTextureCut
        private void BtTextureCut_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bild Dateien|*.bmp;*.jpg;*.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Load the texture
                Image texture = Image.FromFile(openFileDialog.FileName);

                // Put the texture in the PictureBoxes
                SetImageToBox(pictureBox64x64, texture);
                SetImageToBox(pictureBox128x128, texture);
                SetImageToBox(pictureBox256x256, texture);
            }
            else
            {
                MessageBox.Show("No image selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region SetImageToBox
        private void SetImageToBox(PictureBox box, Image texture)
        {
            int size = Math.Min(box.Width, box.Height);
            int x = (texture.Width - size) / 2;
            int y = (texture.Height - size) / 2;
            Rectangle section = new Rectangle(x, y, size, size);

            Bitmap bmp = new Bitmap(texture);
            Image croppedImage = bmp.Clone(section, bmp.PixelFormat);

            box.Image = croppedImage;
            SaveImageFromBox(box);
        }
        #endregion

        #region SaveImageFromBox
        private void SaveImageFromBox(PictureBox box)
        {
            if (box.Image != null)
            {
                string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string directory = Path.Combine(programDirectory, "tempGrafic");

                // Generate file name with "TextureTile", date and time
                string dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string size = box.Width.ToString() + "x" + box.Height.ToString();
                string filename = Path.Combine(directory, $"TextureTile_{size}_{dateTime}.bmp");

                // Make sure the directory exists
                Directory.CreateDirectory(directory);

                // Save the image as a BMP file
                box.Image.Save(filename, System.Drawing.Imaging.ImageFormat.Bmp);

                // Play the sound
                string soundFilePath = Path.Combine(programDirectory, "Sound.wav");
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(soundFilePath);
                player.Play();
            }
            else
            {
                MessageBox.Show("There is no image to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region btLoadTexture
        private void btLoadTexture_Click(object sender, EventArgs e)
        {
            // Load the background image
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files|*.bmp;*.jpg;*.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Load the texture
                Image texture = Image.FromFile(openFileDialog.FileName);

                // Load the image into the PictureBoxes
                LoadImageToBox(pictureBox64x64, texture, true);
                LoadImageToBox(pictureBox128x128, texture, true);
                LoadImageToBox(pictureBox256x256, texture, true);
            }
            else
            {
                MessageBox.Show("No image selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region LoadImageToBox
        private void LoadImageToBox(PictureBox box, Image texture, bool isBackground = false)
        {
            // Put the picture in the PictureBox
            if (isBackground)
            {
                box.BackgroundImage = texture;
            }
            else
            {
                box.Image = texture;
            }
        }
        #endregion

        #region btLoadForeground
        private void btLoadForeground_Click(object sender, EventArgs e)
        {
            // Load the foreground image
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files|*.bmp;*.jpg;*.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Load the image
                Image image = Image.FromFile(openFileDialog.FileName);

                // Create a bitmap from the image
                Bitmap bitmap = new Bitmap(image);

                // Make white transparent
                MakeWhiteTransparent(bitmap);

                // Put the edited image into the selected PictureBox
                if (checkBox64x64.Checked)
                {
                    LoadImageToBox(pictureBox64x64, bitmap);
                }
                else if (checkBox128x128.Checked)
                {
                    LoadImageToBox(pictureBox128x128, bitmap);
                }
                else if (checkBox256x256.Checked)
                {
                    LoadImageToBox(pictureBox256x256, bitmap);
                }
            }
            else
            {
                MessageBox.Show("No image selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region MakeWhiteTransparent
        public void MakeWhiteTransparent(Bitmap bitmap)
        {
            // Loop through every pixel in the image
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    // Get the color of the pixel
                    Color pixelColor = bitmap.GetPixel(x, y);

                    // Check if the color is white
                    if (pixelColor.R == 255 && pixelColor.G == 255 && pixelColor.B == 255)
                    {
                        // Set the pixel to transparent
                        bitmap.SetPixel(x, y, Color.Transparent);
                    }
                }
            }
        }
        #endregion

        #region btCutTextur
        private void btCutTexture_Click(object sender, EventArgs e)
        {
            // Check the checkboxes and cut the texture
            if (checkBox64x64.Checked)
            {
                SaveMergedImageFromBox(pictureBox64x64);
            }
            else if (checkBox128x128.Checked)
            {
                SaveMergedImageFromBox(pictureBox128x128);
            }
            else if (checkBox256x256.Checked)
            {
                SaveMergedImageFromBox(pictureBox256x256);
            }
        }
        #endregion

        #region SaveBitmap
        private void SaveBitmap(Bitmap bitmap)
        {
            string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string directory = Path.Combine(programDirectory, "tempGrafic");

            // Generate file name with "TextureTile", date and time
            string dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string size = bitmap.Width.ToString() + "x" + bitmap.Height.ToString();
            string filename = Path.Combine(directory, $"TextureTile_{size}_{dateTime}.bmp");

            // Make sure the directory exists
            Directory.CreateDirectory(directory);

            // Save the image as a BMP file
            bitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Bmp);

            // Play the sound
            string soundFilePath = Path.Combine(programDirectory, "Sound.wav");
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(soundFilePath);
            player.Play();
        }
        #endregion

        #region SaveMergedImageFromBox
        private void SaveMergedImageFromBox(PictureBox box)
        {
            if (box.Image != null && box.BackgroundImage != null)
            {
                // Create a new bitmap the size of the PictureBox
                Bitmap bitmap = new Bitmap(box.Width, box.Height);

                // Draw both images onto the new bitmap
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    // Draw the background image in the center of the bitmap
                    int x = (bitmap.Width - box.BackgroundImage.Width) / 2;
                    int y = (bitmap.Height - box.BackgroundImage.Height) / 2;
                    g.DrawImage(box.BackgroundImage, x, y);

                    // Draw the foreground image in the center of the bitmap
                    x = (bitmap.Width - box.Image.Width) / 2;
                    y = (bitmap.Height - box.Image.Height) / 2;
                    g.DrawImage(box.Image, x, y);
                }

                // Save the merged image
                SaveBitmap(bitmap);
            }
            else
            {
                MessageBox.Show("There is no image to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion 

        #region btLoadTilesIntoTiles
        private void btLoadTilesIntoTiles_Click(object sender, EventArgs e)
        {
            // Create an OpenFileDialog to select the images
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Images|*.bmp;*.jpg;*.jpeg;*.png",
                Multiselect = false
            };

            // Load the images
            Image[] images = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    images[i] = Image.FromFile(openFileDialog.FileName);
                }
                else
                {
                    MessageBox.Show("Please select an image.");
                    return;
                }
            }

            // Create a new bitmap for each image
            Bitmap[] bitmaps = new Bitmap[3];
            for (int i = 0; i < 3; i++)
            {
                bitmaps[i] = new Bitmap(images[i]);
            }

            // Make #000000 transparent in the images
            MakeColorSemiTransparent(bitmaps[1], "#000000", 0); // 100% transparent

            // Define your colors
            string[] colors = new string[100];
            for (int i = 0; i < 100; i++)
            {
                int value = 255 - (int)(2.55 * i); // Produces a value from 255 to 156
                string hexValue = value.ToString("X2"); // Converts the value to hexadecimal
                colors[i] = "#" + hexValue + hexValue + hexValue; // Generates a hex color code
            }

            // Make each color in the middle image transparent with decreasing transparency
            for (int t = 0; t < 100; t++)
            {
                MakeColorSemiTransparent(bitmaps[1], colors[t], 255 - (int)(2.55 * t)); // t% opaque
            }

            // Apply the transparency mask to the foreground image
            ApplyTransparencyMask(bitmaps[2], bitmaps[1]);

            // Combine the images
            // Combine the images
            Image combinedImage = CombineImages(bitmaps[0], bitmaps[1], bitmaps[2]);

            // Check which checkbox is checked and set the corresponding PictureBox
            if (checkBox64x64.Checked)
            {
                pictureBox64x64.Image = new Bitmap(combinedImage, pictureBox64x64.Size);
            }
            else if (checkBox128x128.Checked)
            {
                pictureBox128x128.Image = new Bitmap(combinedImage, pictureBox128x128.Size);
            }
            else if (checkBox256x256.Checked)
            {
                pictureBox256x256.Image = new Bitmap(combinedImage, pictureBox256x256.Size);
            }
            else
            {
                MessageBox.Show("Please select a CheckBox.");
            }

        }
        #endregion

        #region MakeColorSemiTransparent
        private static void MakeColorSemiTransparent(Bitmap bitmap, string hexColor, int transparency)
        {
            Color color = ColorTranslator.FromHtml(hexColor);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y) == color)
                    {
                        bitmap.SetPixel(x, y, Color.FromArgb(transparency, color.R, color.G, color.B));
                    }
                }
            }
        }
        #endregion

        #region ApplyTransparencyMas
        private static void ApplyTransparencyMask(Bitmap foreground, Bitmap mask)
        {
            for (int y = 0; y < foreground.Height; y++)
            {
                for (int x = 0; x < foreground.Width; x++)
                {
                    Color maskColor = mask.GetPixel(x, y);
                    int maskBrightness = (int)(maskColor.GetBrightness() * 255);
                    Color foregroundColor = foreground.GetPixel(x, y);
                    foreground.SetPixel(x, y, Color.FromArgb(maskBrightness, foregroundColor.R, foregroundColor.G, foregroundColor.B));
                }
            }
        }
        #endregion

        #region Bitmap CombineImages
        private static Bitmap CombineImages(Image image1, Image image2, Image image3)
        {
            Bitmap result = new Bitmap(64, 64);

            using (Graphics graphics = Graphics.FromImage(result))
            {
                graphics.DrawImage(image1, 0, 0);
                graphics.DrawImage(image2, 0, 0);
                graphics.DrawImage(image3, 0, 0);
            }

            return result;
        }
        #endregion

        #region btnGenerateColorCodes
        private void btnGenerateColorCodes_Click(object sender, EventArgs e)
        {
            // Create a list of color codes and their corresponding opacity
            string[] colors = new string[]
            {
                "#000000 - 100% opacity",
                "#FFFFFF - 100% opacity",
                "#FEFEFE - 99% opacity",
                "#FDFDFD - 98% opacity",
                "#FCFCFC - 97% opacity",
                "#FBFBFB - 96% opacity",
                "#FAFAFA - 95% opacity",
                "#F9F9F9 - 94% opacity",
                "#F8F8F8 - 93% opacity",
                "#F7F7F7 - 92% opacity",
                "#F6F6F6 - 91% opacity",
                "#F5F5F5 - 90% opacity",
                "#F4F4F4 - 89% opacity",
                "#F3F3F3 - 88% opacity",
                "#F2F2F2 - 87% opacity",
                "#F1F1F1 - 86% opacity",
                "#F0F0F0 - 85% opacity",
                "#EFEFEF - 84% opacity",
                "#EEEEEE - 83% opacity",
                "#EDEDED - 82% opacity",
                "#ECECEC - 81% opacity",
                "#EBEBEB - 80% opacity",
                "#EAEAEA - 79% opacity",
                "#E9E9E9 - 78% opacity",
                "#E8E8E8 - 77% opacity",
                "#E7E7E7 - 76% opacity",
                "#E6E6E6 - 75% opacity",
                "#E5E5E5 - 74% opacity",
                "#E4E4E4 - 73% opacity",
                "#E3E3E3 - 72% opacity",
                "#E2E2E2 - 71% opacity",
                "#E1E1E1 - 70% opacity",
                "#E0E0E0 - 69% opacity",
                "#DFDFDF - 68% opacity",
                "#DEDEDE - 67% opacity",
                "#DDDDDD - 66% opacity",
                "#DCDCDC - 65% opacity",
                "#DBDBDB - 64% opacity",
                "#DADADA - 63% opacity",
                "#D9D9D9 - 62% opacity",
                "#D8D8D8 - 61% opacity",
                "#D7D7D7 - 60% opacity",
                "#D6D6D6 - 59% opacity",
                "#D5D5D5 - 58% opacity",
                "#D4D4D4 - 57% opacity",
                "#D3D3D3 - 56% opacity",
                "#D2D2D2 - 55% opacity",
                "#D1D1D1 - 54% opacity",
                "#D0D0D0 - 53% opacity",
                "#CFCFCF - 52% opacity",
                "#CECECE - 51% opacity",
                "#CDCDCD - 50% opacity",
                "#CCCCCC - 49% opacity",
                "#CBCBCB - 48% opacity",
                "#CACACA - 47% opacity",
                "#C9C9C9 - 46% opacity",
                "#C8C8C8 - 45% opacity",
                "#C7C7C7 - 44% opacity",
                "#C6C6C6 - 43% opacity",
                "#C5C5C5 - 42% opacity",
                "#C4C4C4 - 41% opacity",
                "#C3C3C3 - 40% opacity",
                "#C2C2C2 - 39% opacity",
                "#C1C1C1 - 38% opacity",
                "#C0C0C0 - 37% opacity",
                "#BFBFBF - 36% opacity",
                "#BEBEBE - 35% opacity",
                "#BDBDBD - 34% opacity",
                "#BCBCBC - 33% opacity",
                "#BBBBBB - 32% opacity",
                "#BABABA - 31% opacity",
                "#B9B9B9 - 30% opacity",
                "#B8B8B8 - 29% opacity",
                "#B7B7B7 - 28% opacity",
                "#B6B6B6 - 27% opacity",
                "#B5B5B5 - 26% opacity",
                "#B4B4B4 - 25% opacity",
                "#B3B3B3 - 24% opacity",
                "#B2B2B2 - 23% opacity",
                "#B1B1B1 - 22% opacity",
                "#B0B0B0 - 21% opacity",
                "#AFAFAF - 20% opacity",
                "#AEAEAE - 19% opacity",
                "#ADADAD - 18% opacity",
                "#ACACAC - 17% opacity",
                "#ABABAB - 16% opacity",
                "#AAAAAA - 15% opacity",
                "#A9A9A9 - 14% opacity",
                "#A8A8A8 - 13% opacity",
                "#A7A7A7 - 12% opacity",
                "#A6A6A6 - 11% opacity",
                "#A5A5A5 - 10% opacity",
                "#A4A4A4 - 9% opacity",
                "#A3A3A3 - 8% opacity",
                "#A2A2A2 - 7% opacity",
                "#A1A1A1 - 6% opacity",
                "#A0A0A0 - 5% opacity",
                "#9F9F9F - 4% opacity",
                "#9E9E9E - 3% opacity",
                "#9D9D9D - 2% opacity",
                "#9C9C9C - 1% opacity",                
            };

            // Display the color codes in the RichTextBox
            richTextBox1.Text = string.Join("\n", colors);
        }
        #endregion

        #region richTextBox1_MouseUp        
        private void richTextBox1_MouseUp(object sender, MouseEventArgs e)
        {
            // Überprüfen Sie, ob der ausgewählte Text null oder leer ist
            if (!string.IsNullOrEmpty(richTextBox1.SelectedText))
            {
                // Wenn der ausgewählte Text ein Hex-Farbcode ist, kopieren Sie ihn in die Zwischenablage
                if (richTextBox1.SelectedText.StartsWith("#"))
                {
                    Clipboard.SetText(richTextBox1.SelectedText);
                }
            }
        }
        #endregion
    }
}
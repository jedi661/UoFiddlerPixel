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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        #region btLoad
        private void btLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Images|*.bmp;*.png;*.jpg;*.jpeg;*.gif";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Laden Sie das Hintergrundbild
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

        #region  btSave64x64_Click
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
                ofd.Filter = "Bilder|*.bmp;*.png;*.jpg;*.jpeg;*.gif";
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
    }
}
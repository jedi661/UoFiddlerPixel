using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using UoFiddler.Controls.UserControls;
using System.Diagnostics;

namespace UoFiddler.Controls.Forms
{
    public partial class TextureWindowForm : Form
    {
        private int _currentId;
        private TexturesControl _texturesControl;

        public TextureWindowForm(TexturesControl texturesControl)
        {
            InitializeComponent();
            _texturesControl = texturesControl;
            ShowTexture(_texturesControl.GetSelectedTextureId());
        }

        #region ShowTexture
        public void ShowTexture(int id)
        {
            _currentId = id;
            pictureBoxTexture.Image = _texturesControl.GetTexture(id);

            if (pictureBoxTexture.Image != null)
            {
                // Store the current image
                _originalImage = (Image)pictureBoxTexture.Image.Clone();

                Size size = pictureBoxTexture.Image.Size;
                lbTextureSize.Text = $"Image size: {size.Width} x {size.Height}";

                // Convert the ID to a hexadecimal address
                string hexAddress = "0x" + id.ToString("X4");

                // Update the text of the label lbIDNr
                lbIDNr.Text = $"ID: {id}, Hex-Adresse: {hexAddress}";
            }
        }
        #endregion

        #region btBackward
        private void btBackward_Click(object sender, EventArgs e)
        {
            if (_currentId > 0)
            {
                _currentId--;
                ShowTexture(_currentId);
            }

            // Always store the current image
            if (pictureBoxTexture.Image != null)
            {
                _originalImage = (Image)pictureBoxTexture.Image.Clone();
            }
        }
        #endregion

        #region btForward
        private void btForward_Click(object sender, EventArgs e)
        {
            if (_currentId < _texturesControl.GetIdxLength() - 1)
            {
                _currentId++;
                ShowTexture(_currentId);
            }

            // Always store the current image
            if (pictureBoxTexture.Image != null)
            {
                _originalImage = (Image)pictureBoxTexture.Image.Clone();
            }
        }
        #endregion

        #region clipboardToolStripMenuItem
        private void clipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxTexture.Image != null)
            {
                Clipboard.SetImage(pictureBoxTexture.Image);
            }
        }
        #endregion

        #region btMakeTile
        // Store the original image
        private Image _originalImage;
        // Add this variable to save the current transform state
        private string _currentTransformation;

        private void btMakeTile_Click(object sender, EventArgs e)
        {
            if (_originalImage != null)
            {
                // Check the current transformation state and apply the appropriate transformation
                float angle = _currentTransformation == "right" ? 45 : -45;

                // Rotate the original image by the determined angle
                Image rotatedImage = RotateImageByAngle(_originalImage, angle);

                // Calculate the size for the rotated image to have 22 pixel sides after cropping
                int size = (int)(31 * Math.Sqrt(2));

                // Crop the rotated image to create a diamond
                Bitmap diamondImage = CropToDiamond(rotatedImage);

                // Resize the diamond image to the calculated size plus 2 pixels
                Bitmap resizedImage = new Bitmap(diamondImage, new Size(size + 2, size + 2));

                // Create a new Bitmap with size 44x44 and black background
                Bitmap newBackground = new Bitmap(44, 44);
                using (Graphics graphics = Graphics.FromImage(newBackground))
                {
                    graphics.Clear(Color.Black);

                    // Calculate the coordinates to center the resized image within the 44x44 frame
                    int x = (newBackground.Width - resizedImage.Width) / 2 - 1; // Shift 1 pixel to the left
                    int y = (newBackground.Height - resizedImage.Height) / 2 - 1; // Shift 1 pixel up

                    // Draw the resized image onto the new background graphic
                    graphics.DrawImage(resizedImage, x, y);
                }

                // Display the new image in the PictureBox
                pictureBoxTexture.Image = newBackground;

                // Free the previous images (optional to free up memory)
                rotatedImage.Dispose();
                diamondImage.Dispose();
                resizedImage.Dispose();
            }
        }
        #endregion

        #region CropToDiamond
        private Bitmap CropToDiamond(Image image)
        {
            // Create a new Bitmap with the same size as the image
            Bitmap diamondImage = new Bitmap(image.Width, image.Height);

            using (Graphics graphics = Graphics.FromImage(diamondImage))
            {
                // Create a new GraphicsPath
                using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    // Add a diamond shape to the path
                    path.AddPolygon(new Point[] {
                new Point(image.Width / 2, 0),
                new Point(image.Width, image.Height / 2),
                new Point(image.Width / 2, image.Height),
                new Point(0, image.Height / 2)
            });

                    // Set the GraphicsPath as the clipping region
                    graphics.SetClip(path);

                    // Draw the image onto the new Bitmap
                    graphics.DrawImage(image, 0, 0);
                }
            }

            return diamondImage;
        }
        #endregion

        #region RotateImageByAngle
        private Image RotateImageByAngle(Image image, float angle)
        {
            // Calculate the size of the new Bitmap
            int bitmapSize = (int)Math.Ceiling(Math.Sqrt(image.Width * image.Width + image.Height * image.Height));

            // Create a new Bitmap with the calculated size
            Bitmap rotatedImage = new Bitmap(bitmapSize, bitmapSize);

            // Create a Graphics instance to draw the image
            using (Graphics graphics = Graphics.FromImage(rotatedImage))
            {
                // Set the InterpolationMode based on the state of the checkbox
                graphics.InterpolationMode = checkBoxAntiAliasing.Checked ? System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic : System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

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

        #region checkBoxLeft        
        private void checkBoxLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxLeft.Checked)
            {
                checkBoxRight.Checked = false;
                _currentTransformation = "left";
            }
        }
        #endregion

        #region checkBoxRight
        private void checkBoxRight_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxRight.Checked)
            {
                checkBoxLeft.Checked = false;
                _currentTransformation = "right";
            }
        }
        #endregion

        #region importToolStripMenuItem
        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                // Get the image from the clipboard
                Image clipboardImage = Clipboard.GetImage();

                // Display the clipboard image in the PictureBox
                pictureBoxTexture.Image = clipboardImage;

                // Store the clipboard image as the original image
                _originalImage = (Image)clipboardImage.Clone();

                // Display the size of the image in the label
                lbTextureSize.Text = $"Size:  {clipboardImage.Width} x {clipboardImage.Height} Pixel.";
            }
            else
            {
                MessageBox.Show("The clipboard does not contain an image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region saveToolStripMenuItem
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxTexture.Image != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Bitmap Image|*.bmp|PNG Image|*.png|TIFF Image|*.tiff|JPEG Image|*.jpg";
                saveFileDialog.Title = "Save the image as...";
                saveFileDialog.ShowDialog();

                // If the file name is not an empty string open it for saving.
                if (saveFileDialog.FileName != "")
                {
                    // Saves the Image via a FileStream created by the OpenFile method.
                    System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog.OpenFile();

                    // Save the image based on the format selected in the save file dialog
                    switch (saveFileDialog.FilterIndex)
                    {
                        case 1:
                            pictureBoxTexture.Image.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
                            break;

                        case 2:
                            pictureBoxTexture.Image.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                            break;

                        case 3:
                            pictureBoxTexture.Image.Save(fs, System.Drawing.Imaging.ImageFormat.Tiff);
                            break;

                        case 4:
                            pictureBoxTexture.Image.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                            break;
                    }

                    fs.Close();
                }
            }
        }
        #endregion

        #region toolStripButtonSave
        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            // Check if there is an image in the PictureBox
            if (pictureBoxTexture.Image != null)
            {
                string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string directory = Path.Combine(programDirectory, "tempGrafic");

                // Generate file name with "TextureTile", date and time
                string dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string filename = Path.Combine(directory, $"TextureTile_{dateTime}.bmp");

                // Make sure the directory exists
                Directory.CreateDirectory(directory);

                // Save the image as a BMP file
                pictureBoxTexture.Image.Save(filename, System.Drawing.Imaging.ImageFormat.Bmp);

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

        #endregion
    }
}

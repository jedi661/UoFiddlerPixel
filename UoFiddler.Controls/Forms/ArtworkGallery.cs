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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UoFiddler.Controls.UserControls;
using Ultima;
using System.Media;


namespace UoFiddler.Controls.Forms
{
    public partial class ArtworkGallery : Form
    {
        private List<int> itemList; // List of artworks
        private int currentIndex = -1; // Current index
        private Bitmap secondImage = null; // Second image (optional)
        private bool useSecondImage => checkBoxSecondArtwork.Checked;
        // Indicates if Rhombus Drawing for ArtworkGallery is active
        private bool isDrawingRhombusArtworkGallery = false;

        // --- [ Movement Settings for Second Image ] ---
        private Point secondImageOffset = Point.Empty; // Offset for moving the overlay
        private const int moveStep = 1; // Step size in pixels (here 1, can be changed)
        private Timer moveTimer; // Timer for held keys
        private Keys currentMoveKey = Keys.None; // Currently pressed movement key
        private bool IsOverlayActive => (useSecondImage && secondImage != null) || animatedGif != null; // Indicates if an overlay is active





        public ArtworkGallery()
        {
            InitializeComponent();
            this.Load += ArtworkGallery_Load;

            // Initialize timer for held movement keys
            moveTimer = new Timer();
            moveTimer.Interval = 50; // 50 ms repetition
            moveTimer.Tick += MoveTimer_Tick;
            this.KeyPreview = true; // For the form to receive keystrokes
            this.KeyDown += ArtworkGallery_KeyDown;
            this.KeyUp += ArtworkGallery_KeyUp;

            this.ActiveControl = null; // No control gets the focus
            this.Focus(); // Focuses on the form itself

        }

        #region [ ArtworkGallery_Load ]
        private void ArtworkGallery_Load(object sender, EventArgs e)
        {
            UpdateImageInfoLabel();
        }
        #endregion

        #region [ InitializeGallery ]
        public void InitializeGallery(int selectedGraphicId)
        {
            itemList = new List<int>(ItemsControl.RefMarker.ItemList);

            if (itemList == null || itemList.Count == 0)
            {
                currentIndex = -1;
                return;
            }

            currentIndex = itemList.IndexOf(selectedGraphicId);
            if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            LoadArtwork();
        }
        #endregion

        #region [ LoadArtwork ]
        private void LoadArtwork()
        {
            if (currentIndex < 0 || currentIndex >= itemList.Count)
                return;

            int graphicId = itemList[currentIndex];
            Bitmap bitmap = Art.GetStatic(graphicId);

            if (bitmap == null)
            {
                pictureBoxArtworkGallery.Image?.Dispose();
                pictureBoxArtworkGallery.Image = null;
                this.Text = $"Artwork: 0x{graphicId:X4} (No Image)";
                return;
            }

            Bitmap finalImage;

            // If no second image is active, load normal image
            if (!useSecondImage || secondImage == null)
            {
                finalImage = new Bitmap(bitmap);
            }
            else
            {
                // If second image active: Load combined image
                Bitmap baseImage = new Bitmap(bitmap);
                finalImage = CombineImages(baseImage, secondImage);
                baseImage.Dispose();
            }

            // --- HIER: Rhombus draufzeichnen, wenn aktiv ---
            if (isDrawingRhombusArtworkGallery)
            {
                using (Graphics g = Graphics.FromImage(finalImage))
                {
                    DrawRhombusArtworkGallery(g);
                }
            }

            pictureBoxArtworkGallery.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBoxArtworkGallery.Image?.Dispose();
            pictureBoxArtworkGallery.Image = finalImage;

            this.Text = $"Artwork: 0x{graphicId:X4}";
        }

        #endregion

        #region [ Bitmap CombineImages ]
        private Bitmap CombineImages(Bitmap baseImage, Bitmap overlayImage)
        {
            Bitmap combined = new Bitmap(baseImage.Width, baseImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(combined))
            {
                g.DrawImage(baseImage, 0, 0);

                int x = (baseImage.Width - overlayImage.Width) / 2 + secondImageOffset.X; // Centering the overlay image
                int y = (baseImage.Height - overlayImage.Height) / 2 + secondImageOffset.Y; // Centering the overlay image


                g.DrawImage(overlayImage, x, y);
            }

            return combined;
        }
        #endregion

        #region [SetArtworkImage]
        public void SetArtworkImage(Bitmap artworkImage, int index)
        {
            if (artworkImage == null)
            {
                pictureBoxArtworkGallery.Image = null;
                return;
            }

            pictureBoxArtworkGallery.Image?.Dispose();
            pictureBoxArtworkGallery.Image = new Bitmap(artworkImage);
            currentIndex = index; // Set current position
        }
        #endregion

        #region [ButtonLeft_Click]
        private void ButtonLeft_Click(object sender, EventArgs e)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                LoadArtwork();
                UpdateImageInfoLabel();
            }
        }
        #endregion

        #region [ ButtonRight_Click ]
        private void ButtonRight_Click(object sender, EventArgs e)
        {
            if (currentIndex < itemList.Count - 1)
            {
                currentIndex++;
                LoadArtwork();
                UpdateImageInfoLabel();
            }
        }
        #endregion

        // not active for later
        private void UpdateArtwork()
        {
            if (currentIndex < 0 || currentIndex >= ItemsControl.RefMarker.ItemList.Count)
                return;

            int graphicId = ItemsControl.RefMarker.ItemList[currentIndex];
            Bitmap bitmap = Art.GetStatic(graphicId);

            if (bitmap != null)
            {
                pictureBoxArtworkGallery.Image?.Dispose();
                pictureBoxArtworkGallery.Image = new Bitmap(bitmap);
            }
            else
            {
                pictureBoxArtworkGallery.Image?.Dispose();
                pictureBoxArtworkGallery.Image = null;
            }
        }

        #region [ loadSecondImageToolStripMenuItem_Click ]
        private void LoadSecondImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select an image to overlay";
                openFileDialog.Filter = "Image Files (*.bmp; *.png; *.jpg; *.jpeg; *.tiff)|*.bmp;*.png;*.jpg;*.jpeg;*.tiff";
                openFileDialog.DefaultExt = "bmp";

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                if (pictureBoxArtworkGallery.Image == null)
                {
                    MessageBox.Show("There is no base image loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Clean up old SecondImage, if available
                secondImage?.Dispose();
                secondImage = null;

                using (Bitmap loadedOverlay = new Bitmap(openFileDialog.FileName))
                {
                    // Prepare new overlay with transparency
                    Bitmap preparedOverlay = new Bitmap(loadedOverlay.Width, loadedOverlay.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    for (int y = 0; y < loadedOverlay.Height; y++)
                    {
                        for (int x = 0; x < loadedOverlay.Width; x++)
                        {
                            Color pixel = loadedOverlay.GetPixel(x, y);

                            if ((pixel.R == 0 && pixel.G == 0 && pixel.B == 0) || (pixel.R == 255 && pixel.G == 255 && pixel.B == 255))
                            {
                                preparedOverlay.SetPixel(x, y, Color.Transparent); // Black or white becomes transparent
                            }
                            else
                            {
                                preparedOverlay.SetPixel(x, y, pixel); // Other colors remain
                            }
                        }
                    }

                    // Save preparedOverlay as new secondImage
                    secondImage = preparedOverlay;
                }

                // Rebuild image
                LoadArtwork();
                UpdateImageInfoLabel();
            }
        }
        #endregion

        #region [ ChecboxSecondArtwork_CheckedChanged ]
        private void CheckBoxSecondArtwork_CheckedChanged(object sender, EventArgs e)
        {
            LoadArtwork();
        }
        #endregion

        #region [ ButtonDrawRhombus_Click ]
        private void ButtonDrawRhombus_Click(object sender, EventArgs e)
        {
            isDrawingRhombusArtworkGallery = !isDrawingRhombusArtworkGallery;

            // Change button visually
            ButtonDrawRhombus.BackColor = isDrawingRhombusArtworkGallery ? Color.LightGreen : SystemColors.Control;

            // Play sound
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = "sound.wav";
            player.Play();

            // Redraw image
            pictureBoxArtworkGallery.Invalidate(); // Re-triggers `Paint`
        }
        #endregion

        #region [ DrawRhombusArtworkGallery ]
        private void DrawRhombusArtworkGallery(Graphics g)
        {
            if (pictureBoxArtworkGallery.Image == null)
            {
                return;
            }

            // **Set fixed values ​​for the rhombus size and position**
            int centerX = pictureBoxArtworkGallery.Width / 2; // Fixed center of the PictureBox
            int baseY = pictureBoxArtworkGallery.Height - 170; // Fixed base independent of the image

            // **Upper rhombus half (remains constant)**
            Point[] pointsUpper = new Point[4];
            pointsUpper[0] = new Point(centerX, baseY - 133);
            pointsUpper[1] = new Point(centerX + 22, baseY - 111);
            pointsUpper[2] = new Point(centerX, baseY - 89);
            pointsUpper[3] = new Point(centerX - 22, baseY - 111);

            g.DrawPolygon(Pens.Black, pointsUpper);

            // **Line exactly below remains unchanged**
            int lineWidth = 100;
            int lineStartX = centerX - lineWidth / 2;
            int lineEndX = lineStartX + lineWidth;
            g.DrawLine(Pens.Black, new Point(lineStartX, baseY), new Point(lineEndX, baseY));

            // **Lower rhombus half (remains constant)**
            Point[] pointsLower = new Point[4];
            pointsLower[0] = new Point(centerX, baseY);
            pointsLower[1] = new Point(centerX + 22, baseY - 22);
            pointsLower[2] = new Point(centerX, baseY - 44);
            pointsLower[3] = new Point(centerX - 22, baseY - 22);

            g.DrawPolygon(Pens.Black, pointsLower);

            // **Connecting lines**
            g.DrawLine(Pens.Black, pointsUpper[0], pointsLower[0]);
            g.DrawLine(Pens.Black, pointsUpper[1], pointsLower[1]);
            g.DrawLine(Pens.Black, pointsUpper[3], pointsLower[3]);
        }

        #endregion

        #region [ PictureBoxArtworkGallery_Paint ]
        private void PictureBoxArtworkGallery_Paint(object sender, PaintEventArgs e)
        {
            if (isDrawingRhombusArtworkGallery)
            {
                DrawRhombusArtworkGallery(e.Graphics);
            }

            if (animatedGif != null)
            {
                // GIF frame updates
                ImageAnimator.UpdateFrames(animatedGif);

                // Calculate position for centering
                int x = (pictureBoxArtworkGallery.Width - animatedGif.Width) / 2 + secondImageOffset.X;
                int y = (pictureBoxArtworkGallery.Height - animatedGif.Height) / 2 + secondImageOffset.Y;

                // Draw GIF
                e.Graphics.DrawImage(animatedGif, x, y);
            }
        }
        #endregion

        #region [ LoadClipboardToolStripMenuItem_Click ]
        private void loadClipboradToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show("The clipboard does not contain an image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (pictureBoxArtworkGallery.Image == null)
            {
                MessageBox.Show("There is no base image loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Clean up old SecondImage, if available
            secondImage?.Dispose();
            secondImage = null;

            using (Bitmap loadedOverlay = new Bitmap(Clipboard.GetImage()))
            {
                // Prepare new overlay with transparency
                Bitmap preparedOverlay = new Bitmap(loadedOverlay.Width, loadedOverlay.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                for (int y = 0; y < loadedOverlay.Height; y++)
                {
                    for (int x = 0; x < loadedOverlay.Width; x++)
                    {
                        Color pixel = loadedOverlay.GetPixel(x, y);

                        if ((pixel.R == 0 && pixel.G == 0 && pixel.B == 0) || (pixel.R == 255 && pixel.G == 255 && pixel.B == 255))
                        {
                            preparedOverlay.SetPixel(x, y, Color.Transparent); // Black or white becomes transparent
                        }
                        else
                        {
                            preparedOverlay.SetPixel(x, y, pixel); // Other colors remain
                        }
                    }
                }

                // Save preparedOverlay as new secondImage
                secondImage = preparedOverlay;
            }

            // Rebuild image
            LoadArtwork();
            UpdateImageInfoLabel();
        }
        #endregion

        #region [ UpdateImageInfoLabel ]
        private void UpdateImageInfoLabel()
        {
            if (pictureBoxArtworkGallery.Image == null)
            {
                label1ImageInfo.Text = "No base image loaded.";
                return;
            }

            string baseImageSize = $"{pictureBoxArtworkGallery.Image.Width}x{pictureBoxArtworkGallery.Image.Height}";

            string secondImageSize = secondImage != null
                ? $"{secondImage.Width}x{secondImage.Height}"
                : "No overlay";

            label1ImageInfo.Text = $"Base: {baseImageSize} | Overlay: {secondImageSize}";
        }
        #endregion

        #region [ LoadGifToolStripMenuItem_Click ]
        private Image animatedGif = null;

        private void loadGifToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select a GIF to overlay";
                openFileDialog.Filter = "GIF Files (*.gif)|*.gif";
                openFileDialog.DefaultExt = "gif";

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                if (pictureBoxArtworkGallery.Image == null)
                {
                    MessageBox.Show("There is no base image loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Clean up old second image if available
                animatedGif?.Dispose();
                animatedGif = null;

                // Load the animated GIF
                animatedGif = Image.FromFile(openFileDialog.FileName);

                // Start animating the GIF
                if (ImageAnimator.CanAnimate(animatedGif))
                {
                    ImageAnimator.Animate(animatedGif, new EventHandler(this.OnFrameChanged));
                }

                // Rebuild artwork to show the first frame
                LoadArtwork();
                UpdateImageInfoLabel();
            }
        }
        #endregion

        #region [ OnFrameChanged ]
        private void OnFrameChanged(object sender, EventArgs e)
        {
            if (animatedGif != null)
            {
                ImageAnimator.UpdateFrames(animatedGif);
                pictureBoxArtworkGallery.Invalidate(); // Redraw
            }
        }
        #endregion

        #region [ ButtonRemoveGif_Click ]
        private void ButtonRemoveGif_Click(object sender, EventArgs e)
        {
            if (animatedGif != null)
            {
                ImageAnimator.StopAnimate(animatedGif, new EventHandler(this.OnFrameChanged));
                animatedGif.Dispose();
                animatedGif = null;

                pictureBoxArtworkGallery.Invalidate();
            }
        }
        #endregion

        #region [ ButtonRemoveSecondImage_Click ]
        private void ButtonRemoveSecondImage_Click(object sender, EventArgs e)
        {
            if (secondImage != null)
            {
                secondImage.Dispose();
                secondImage = null;

                LoadArtwork();
                pictureBoxArtworkGallery.Invalidate();
            }
        }
        #endregion

        #region [ ButtonClearAll_Click ]
        private void ButtonClearAll_Click_Click(object sender, EventArgs e)
        {
            if (animatedGif != null)
            {
                ImageAnimator.StopAnimate(animatedGif, new EventHandler(this.OnFrameChanged));
                animatedGif.Dispose();
                animatedGif = null;
            }

            if (secondImage != null)
            {
                secondImage.Dispose();
                secondImage = null;
            }

            if (pictureBoxArtworkGallery.Image != null)
            {
                pictureBoxArtworkGallery.Image.Dispose();
                pictureBoxArtworkGallery.Image = null;
            }

            pictureBoxArtworkGallery.Invalidate();
            UpdateImageInfoLabel();
        }
        #endregion

        #region [ ArtworkGallery_KeyDown ]
        private void ArtworkGallery_KeyDown(object sender, KeyEventArgs e)
        {
            if (!IsOverlayActive)
                return;

            if (currentMoveKey != e.KeyCode)
            {
                currentMoveKey = e.KeyCode;
                MoveSecondImage(e.KeyCode);
                moveTimer.Start();
            }
        }
        #endregion

        #region [ ArtworkGallery_KeyUp ]
        private void ArtworkGallery_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == currentMoveKey)
            {
                moveTimer.Stop();
                currentMoveKey = Keys.None;
            }
        }
        #endregion

        #region [ MoveTimer_Tick ]
        private void MoveTimer_Tick(object sender, EventArgs e)
        {
            if (currentMoveKey != Keys.None)
            {
                MoveSecondImage(currentMoveKey);
            }
        }
        #endregion

        #region [ MoveSecondImage ]
        private void MoveSecondImage(Keys key)
        {
            switch (key)
            {
                case Keys.Left:
                    secondImageOffset.X -= moveStep;
                    break;
                case Keys.Right:
                    secondImageOffset.X += moveStep;
                    break;
                case Keys.Up:
                    secondImageOffset.Y -= moveStep;
                    break;
                case Keys.Down:
                    secondImageOffset.Y += moveStep;
                    break;
                default:
                    return;
            }

            if (animatedGif != null)
            {
                pictureBoxArtworkGallery.Invalidate(); // GIF muss neu gezeichnet werden
            }
            else
            {
                LoadArtwork(); // Für secondImage
            }
        }
        #endregion

        #region [ IsInputKey ]
        protected override bool IsInputKey(Keys keyData)
        {
            // These keys are considered "input keys" and should NOT be used for focus navigation
            if (keyData == Keys.Left || keyData == Keys.Right || keyData == Keys.Up || keyData == Keys.Down)
                return true;

            return base.IsInputKey(keyData);
        }
        #endregion

        #region [ ProcessCmdKey ]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (IsOverlayActive)
            {
                if (keyData == Keys.Left || keyData == Keys.Right || keyData == Keys.Up || keyData == Keys.Down)
                {
                    MoveSecondImage(keyData); // Trigger movement immediately
                    currentMoveKey = keyData; // Set currently pressed key
                    moveTimer.Start(); // Activate repeat motion
                    return true; // IMPORTANT: Mark event as processed -> DO NOT pass on to buttons!
                }
            }
            return base.ProcessCmdKey(ref msg, keyData); // Maintain default behavior for other keys
        }
        #endregion

        #region [ ButtonCrop_Click ]
        private void ButtonCrop_Click(object sender, EventArgs e)
        {
            if (secondImage == null)
            {
                MessageBox.Show("No second image loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(textBoxWidth.Text, out int newWidth) || newWidth <= 0 ||
                !int.TryParse(textBoxHeight.Text, out int newHeight) || newHeight <= 0)
            {
                MessageBox.Show("Please enter valid positive numbers for width and height.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int origWidth = secondImage.Width;
            int origHeight = secondImage.Height;

            // Limit: New values ​​must not exceed original size
            if (newWidth > origWidth || newHeight > origHeight)
            {
                MessageBox.Show("Crop size exceeds original image size.", "Invalid Crop", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Determine center point
            int centerX = origWidth / 2;
            int centerY = origHeight / 2;

            // Calculate crop rectangle (centered)
            int cropX = centerX - newWidth / 2;
            int cropY = centerY - newHeight / 2;
            Rectangle cropArea = new Rectangle(cropX, cropY, newWidth, newHeight);

            // Create a new bitmap with the section
            Bitmap cropped = new Bitmap(newWidth, newHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(cropped))
            {
                g.DrawImage(secondImage, new Rectangle(0, 0, newWidth, newHeight), cropArea, GraphicsUnit.Pixel);
            }

            // Request storage path from user
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Title = "Save cropped overlay image";
                saveDialog.Filter = "BMP Image (*.bmp)|*.bmp|PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg|TIFF Image (*.tiff)|*.tiff";
                saveDialog.DefaultExt = "bmp"; // Default is BMP
                saveDialog.FileName = "cropped_overlay.bmp"; // Suggestion name

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Determine file format based on the extension
                        System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Bmp;

                        string fileName = saveDialog.FileName.ToLower();

                        if (fileName.EndsWith(".png"))
                            format = System.Drawing.Imaging.ImageFormat.Png;
                        else if (fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg"))
                            format = System.Drawing.Imaging.ImageFormat.Jpeg;
                        else if (fileName.EndsWith(".tiff"))
                            format = System.Drawing.Imaging.ImageFormat.Tiff;

                        cropped.Save(saveDialog.FileName, format);

                        // Success message (optional)
                        MessageBox.Show("Image successfully saved.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving image:\n{ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return; // If error occurs: no update
                    }
                }
                else
                {
                    // Save aborts, nothing updated
                    return;
                }
            }

            // Replace and display secondImage
            secondImage.Dispose();
            secondImage = cropped;

            secondImageOffset = Point.Empty; // Reset offset

            LoadArtwork(); // Redraw
            UpdateImageInfoLabel(); // Update info
        }
        #endregion

        #region [ CenterOverlayToolStripMenuItem_Click ]
        private void CenterOverlayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (secondImage == null && animatedGif == null)
            {
                MessageBox.Show("No overlay (second image or animated GIF) loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            secondImageOffset = Point.Empty; // Position zurücksetzen

            if (animatedGif != null)
            {
                pictureBoxArtworkGallery.Invalidate(); // GIF muss neu gezeichnet werden
            }
            else
            {
                LoadArtwork(); // Für statisches Overlay
            }

            UpdateImageInfoLabel(); // Info aktualisieren
        }
        #endregion

        #region [ FlipVerticalToolStripMenuItem_Click ]
        private void FlipVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (secondImage == null)
            {
                MessageBox.Show("No second image loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            secondImage.RotateFlip(RotateFlipType.RotateNoneFlipY); // Flip vertically
            LoadArtwork();
        }
        #endregion

        #region [ FlipHorizontalToolStripMenuItem_Click ]
        private void FlipHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (secondImage == null)
            {
                MessageBox.Show("No second image loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            secondImage.RotateFlip(RotateFlipType.RotateNoneFlipX); // Flip horizontally
            LoadArtwork();
        }
        #endregion
    }
}

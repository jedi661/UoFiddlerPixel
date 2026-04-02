using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.Forms
{
    public partial class TextureWindowForm : Form
    {
        // ── State ──────────────────────────────────────────────────────────
        private int _currentId;
        private readonly TexturesControl _texturesControl;

        private int _landTile = 0x3;
        private int _currentTile = 0;
        private const int MaxTileIndex = 0x3FFF;

        private Image _originalImage;
        private string _currentTransformation = "left";

        // Background color outside the diamond: false = black, true = white
        private bool _tileBackgroundWhite = false;

        private bool _showIsoGrid = true;   // Grid turned on by default
        private float _currentContrast = 1.0f;   // 1.0 = neutral

        // ── Constructor ────────────────────────────────────────────────────
        public TextureWindowForm(TexturesControl texturesControl)
        {
            InitializeComponent();
            _texturesControl = texturesControl;
            ShowTexture(_texturesControl.GetSelectedTextureId());
        }

        #region [ GetDiamondRowBounds ] 
        // ═══════════════════════════════════════════════════════════════════
        //  PIXEL-PRECISE DIAMOND MASK (from reference graphic)
        //  For each of the 44 lines: StartX and EndX (inclusive).
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// For row y, this gives the first and last X-position within
        /// the UO tile diamond back (both inclusive).
        /// Exactly matches the pixel layout of the reference graphic (44×44).
        /// </summary>
        private static void GetDiamondRowBounds(int y, out int startX, out int endX)
        {
            // The rhombus is symmetrical about y=21/22.
            // Upper half (y=0..21): startX decreases, endX increases.
            // Lower half (y=22..43): reversed.
            //
            // Formula from pixel analysis:
            //   Für y = 0..21:  startX = 21 - y,  endX = 22 + y
            //   Für y = 22..43: distFromBottom = 43 - y
            //                   startX = 21 - distFromBottom
            //                   endX   = 22 + distFromBottom

            if (y <= 21)
            {
                startX = 21 - y;
                endX = 22 + y;
            }
            else
            {
                int d = 43 - y;
                startX = 21 - d;
                endX = 22 + d;
            }
        }
        #endregion

        // ═══════════════════════════════════════════════════════════════════
        //  MAKE TILE – Pixel-precise diamond conversion
        // ═══════════════════════════════════════════════════════════════════

        #region [ BtMakeTile_Click ]
        /// <summary>
        /// Converts the source texture into a pixel-perfect 44×44 UO land tile.
        ///
        /// Methode:
        ///   The width of the rhombus is calculated for each row of the 44×44 rhombus.
        ///   The source texture is sampled proportionally to this width. –
        ///   No rotation, direct pixel mapping.
        ///   Outside the diamond: the chosen background color (black or white).
        /// </summary>
        private void BtMakeTile_Click(object sender, EventArgs e)
        {
            if (_originalImage == null)
            {
                MessageBox.Show("No source image loaded.", "Make Tile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Apply contrast if changed
            Bitmap sourceForTile = _currentContrast == 1.0f
                ? new Bitmap(_originalImage)
                : ApplyContrast(_originalImage, _currentContrast);

            Color bgColor = _tileBackgroundWhite ? Color.White : Color.Black;
            Bitmap tile = new Bitmap(44, 44, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(tile))
                g.Clear(bgColor);

            int srcW = sourceForTile.Width;
            int srcH = sourceForTile.Height;

            for (int tileY = 0; tileY < 44; tileY++)
            {
                GetDiamondRowBounds(tileY, out int startX, out int endX);
                int rowWidth = endX - startX + 1;

                float srcY_f = (tileY / 43.0f) * (srcH - 1);

                for (int tileX = startX; tileX <= endX; tileX++)
                {
                    float posInRow = (float)(tileX - startX) / Math.Max(rowWidth - 1, 1);
                    float srcX_f = posInRow * (srcW - 1);

                    Color pixel = checkBoxAntiAliasing.Checked
                        ? BilinearSample(sourceForTile, srcX_f, srcY_f)
                        : sourceForTile.GetPixel((int)Math.Round(srcX_f), (int)Math.Round(srcY_f));

                    tile.SetPixel(tileX, tileY, pixel);
                }
            }

            sourceForTile.Dispose();

            pictureBoxTexture.Image = tile;
            pictureBoxTexture.SizeMode = PictureBoxSizeMode.CenterImage;   // mittig + Originalgröße

            lbTextureSize.Text = "Image size: 44 x 44 px (UO Tile)";
        }
        #endregion        

        #region [ BilinearSample ]
        /// <summary>
        /// Bilinear interpolation for softer colors without blurring of edges.
        /// </summary>
        private static Color BilinearSample(Bitmap bmp, float fx, float fy)
        {
            int x0 = (int)fx;
            int y0 = (int)fy;
            int x1 = Math.Min(x0 + 1, bmp.Width - 1);
            int y1 = Math.Min(y0 + 1, bmp.Height - 1);
            x0 = Math.Max(x0, 0);
            y0 = Math.Max(y0, 0);

            float tx = fx - x0;
            float ty = fy - y0;

            Color c00 = bmp.GetPixel(x0, y0);
            Color c10 = bmp.GetPixel(x1, y0);
            Color c01 = bmp.GetPixel(x0, y1);
            Color c11 = bmp.GetPixel(x1, y1);

            int r = (int)(c00.R * (1 - tx) * (1 - ty) + c10.R * tx * (1 - ty)
                        + c01.R * (1 - tx) * ty + c11.R * tx * ty);
            int g = (int)(c00.G * (1 - tx) * (1 - ty) + c10.G * tx * (1 - ty)
                        + c01.G * (1 - tx) * ty + c11.G * tx * ty);
            int b = (int)(c00.B * (1 - tx) * (1 - ty) + c10.B * tx * (1 - ty)
                        + c01.B * (1 - tx) * ty + c11.B * tx * ty);

            return Color.FromArgb(
                Math.Max(0, Math.Min(255, r)),
                Math.Max(0, Math.Min(255, g)),
                Math.Max(0, Math.Min(255, b)));
        }
        #endregion

        // ═══════════════════════════════════════════════════════════════════
        //  TEXTURE DISPLAY
        // ═══════════════════════════════════════════════════════════════════

        #region [ ShowTexture ]
        public void ShowTexture(int id)
        {
            _currentId = id;
            Image newTexture = _texturesControl.GetTexture(id);
            pictureBoxTexture.Image = newTexture;

            if (newTexture == null)
                return;

            ReplaceOriginalImage((Image)newTexture.Clone());

            Size size = newTexture.Size;
            lbTextureSize.Text = $"Image size: {size.Width} x {size.Height} px";

            string hexAddress = "0x" + id.ToString("X4");
            lbIDNr.Text = $"ID: {id}  |  Hex: {hexAddress}";
        }
        #endregion

        // ═══════════════════════════════════════════════════════════════════
        //  NAVIGATION
        // ═══════════════════════════════════════════════════════════════════

        #region [ BtBackward_Click ]
        private void BtBackward_Click(object sender, EventArgs e)
        {
            if (_currentId <= 0) return;
            _currentId--;
            ShowTexture(_currentId);
        }
        #endregion

        #region [ BtForward_Click ]
        private void BtForward_Click(object sender, EventArgs e)
        {
            if (_currentId >= _texturesControl.GetIdxLength() - 1) return;
            _currentId++;
            ShowTexture(_currentId);
        }
        #endregion

        // ═══════════════════════════════════════════════════════════════════
        //  HINTERGRUNDFARBE AUSSERHALB DER RAUTE
        // ═══════════════════════════════════════════════════════════════════

        #region [ RadioButtonTileBgBlack_CheckedChanged ]
        private void RadioButtonTileBgBlack_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonTileBgBlack.Checked)
                _tileBackgroundWhite = false;
        }
        #endregion

        #region [ RadioButtonTileBgWhite_CheckedChanged ]
        private void RadioButtonTileBgWhite_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonTileBgWhite.Checked)
                _tileBackgroundWhite = true;
        }
        #endregion

        // ═══════════════════════════════════════════════════════════════════
        //  ROTATION DIRECTION CHECKBOXES
        // ═══════════════════════════════════════════════════════════════════

        #region checkBoxLeft_CheckedChanged
        private void CheckBoxLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBoxLeft.Checked) return;
            checkBoxRight.Checked = false;
            _currentTransformation = "left";
        }
        #endregion

        #region [ CheckBoxRight_CheckedChanged ]
        private void CheckBoxRight_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBoxRight.Checked) return;
            checkBoxLeft.Checked = false;
            _currentTransformation = "right";
        }
        #endregion

        // ═══════════════════════════════════════════════════════════════════
        //  CLIPBOARD
        // ═══════════════════════════════════════════════════════════════════

        #region [ ClipboardToolStripMenuItem_Click ]
        private void ClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxTexture.Image == null)
            {
                MessageBox.Show("No image to copy..", "Clipboard",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Clipboard.SetImage(pictureBoxTexture.Image);
        }
        #endregion

        #region [ ImportToolStripMenuItem_Click ]
        private void ImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show("The clipboard contains no image.", "Import",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Image clipImage = Clipboard.GetImage();
            pictureBoxTexture.Image = clipImage;
            ReplaceOriginalImage((Image)clipImage.Clone());
            lbTextureSize.Text = $"Size: {clipImage.Width} x {clipImage.Height} px";
        }
        #endregion

        // ═══════════════════════════════════════════════════════════════════
        //  SAVE / LOAD
        // ═══════════════════════════════════════════════════════════════════

        #region [ saveToolStripMenuItem_Click ]
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxTexture.Image == null)
            {
                MessageBox.Show("No image to save.", "Save",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "Bitmap Image|*.bmp|PNG Image|*.png|TIFF Image|*.tiff|JPEG Image|*.jpg";
                dlg.Title = "Save image as…";
                if (dlg.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(dlg.FileName))
                    return;

                ImageFormat fmt = dlg.FilterIndex switch
                {
                    1 => ImageFormat.Bmp,
                    2 => ImageFormat.Png,
                    3 => ImageFormat.Tiff,
                    4 => ImageFormat.Jpeg,
                    _ => ImageFormat.Bmp
                };

                using (FileStream fs = (FileStream)dlg.OpenFile())
                    pictureBoxTexture.Image.Save(fs, fmt);
            }
        }
        #endregion

        #region [ toolStripButtonSave_Click ]
        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            if (pictureBoxTexture.Image == null)
            {
                MessageBox.Show("No image to save.", "Quick Save",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string programDir = AppDomain.CurrentDomain.BaseDirectory;
            string outputDir = Path.Combine(programDir, "tempGrafic");
            Directory.CreateDirectory(outputDir);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = Path.Combine(outputDir, $"TextureTile_{timestamp}.bmp");

            pictureBoxTexture.Image.Save(filePath, ImageFormat.Bmp);

            string soundPath = Path.Combine(programDir, "Sound.wav");
            if (File.Exists(soundPath))
            {
                using (System.Media.SoundPlayer player = new System.Media.SoundPlayer(soundPath))
                    player.Play();
            }
        }
        #endregion

        #region [ toolStripButtonImageLoad_Click ] – Loads an image from the file system, displays it in the picture box, and updates the original image reference and size label for further processing.
        private void toolStripButtonImageLoad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Images|*.bmp;*.png;*.jpeg;*.jpg;*.tiff;*.tif";
                dlg.Title = "Load image…";
                if (dlg.ShowDialog() != DialogResult.OK) return;

                Image loaded = Image.FromFile(dlg.FileName);
                pictureBoxTexture.Image = loaded;
                ReplaceOriginalImage((Image)loaded.Clone());
                lbTextureSize.Text = $"Size: {loaded.Width} x {loaded.Height} px";
            }
        }
        #endregion

        #region [ ButtonOpenTempGrafic_Click ] – Opens the "tempGrafic" folder in the file explorer, where quick-saved images are stored, and shows a message if the folder doesn't exist yet.
        private void ButtonOpenTempGrafic_Click(object sender, EventArgs e)
        {
            string directory = Path.Combine(Application.StartupPath, "tempGrafic");
            if (Directory.Exists(directory))
                Process.Start("explorer.exe", directory);
            else
                MessageBox.Show("The folder 'tempGrafic' doesn't exist yet.\nFirst save a tile.", "Open folder",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        // ═══════════════════════════════════════════════════════════════════
        //  PREVIEW PANEL – tiled floor rendering
        // ═══════════════════════════════════════════════════════════════════

        #region [ GetImage ] – Renders a tiled floor preview using the current land tile, filling the canvas with repeated tiles and optionally overlaying an isometric grid for better visualization.
        private Bitmap GetImage()
        {
            Bitmap canvas = new Bitmap(pictureBoxPreview.Width, pictureBoxPreview.Height);
            using (Graphics g = Graphics.FromImage(canvas))
            {
                Bitmap tile = Ultima.Art.GetLand(_landTile);
                if (tile == null) return canvas;

                int row = 0;
                for (int y = -22; y <= canvas.Height; y += 22)
                {
                    int startX = (row % 2 == 0) ? 0 : -22;
                    for (int x = startX; x <= canvas.Width; x += 44)
                        g.DrawImage(tile, x, y);
                    row++;
                }

                if (_showIsoGrid)
                    DrawIsoGrid(g, canvas.Width, canvas.Height);
            }
            return canvas;
        }
        #endregion

        #region [ GetImageFromImport ]
        private Bitmap GetImageFromImport(Image importedImage)
        {
            if (importedImage == null) return null;

            Bitmap diamondImage = CropToDiamond(importedImage);
            Bitmap scaledImage = new Bitmap(diamondImage, new Size(44, 44));
            diamondImage.Dispose();

            Bitmap canvas = new Bitmap(pictureBoxPreview.Width, pictureBoxPreview.Height);
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.Transparent);

                int row = 0;
                for (int y = -22; y <= canvas.Height; y += 22)
                {
                    int startX = (row % 2 == 0) ? 0 : -22;
                    for (int x = startX; x <= canvas.Width; x += 44)
                        g.DrawImage(scaledImage, x, y);
                    row++;
                }

                if (_showIsoGrid)
                    DrawIsoGrid(g, canvas.Width, canvas.Height);
            }

            scaledImage.Dispose();

            // Make black pixels transparent (as before)
            for (int x = 0; x < canvas.Width; x++)
                for (int y = 0; y < canvas.Height; y++)
                {
                    Color c = canvas.GetPixel(x, y);
                    if (c.R == 0 && c.G == 0 && c.B == 0)
                        canvas.SetPixel(x, y, Color.Transparent);
                }

            return canvas;
        }
        #endregion

        #region [ CropToDiamond ] – Crops the input image to a diamond shape by defining a polygonal clipping path that matches the UO tile's diamond layout, ensuring that only the pixels within the diamond are retained in the resulting bitmap.
        private Bitmap CropToDiamond(Image image)
        {
            Bitmap diamond = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(diamond))
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddPolygon(new[]
                {
                    new Point(image.Width / 2, 0),
                    new Point(image.Width, image.Height / 2),
                    new Point(image.Width / 2, image.Height),
                    new Point(0, image.Height / 2)
                });

                g.SetClip(path);
                g.DrawImage(image, 0, 0);
            }
            return diamond;
        }
        #endregion

        #region [ BuildTileFromImage]
        /// <summary>
        /// Creates a pixel-perfect 44×44 UO tile from any source image.        
        /// </summary>
        private Bitmap BuildTileFromImage(Image source, Color bgColor)
        {
            Bitmap src = new Bitmap(source);
            Bitmap tile = new Bitmap(44, 44, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(tile))
                g.Clear(bgColor);

            int srcW = src.Width;
            int srcH = src.Height;

            for (int tileY = 0; tileY < 44; tileY++)
            {
                GetDiamondRowBounds(tileY, out int startX, out int endX);
                int rowWidth = endX - startX + 1;

                float srcY_f = (tileY / 43.0f) * (srcH - 1);

                for (int tileX = startX; tileX <= endX; tileX++)
                {
                    float posInRow = (float)(tileX - startX) / Math.Max(rowWidth - 1, 1);
                    float srcX_f = posInRow * (srcW - 1);

                    Color pixel = checkBoxAntiAliasing.Checked
                        ? BilinearSample(src, srcX_f, srcY_f)
                        : src.GetPixel((int)Math.Round(srcX_f), (int)Math.Round(srcY_f));

                    tile.SetPixel(tileX, tileY, pixel);
                }
            }

            src.Dispose();
            return tile;
        }
        #endregion

        #region [ IgPreviewClicked_Click ] – Updates the preview with the current land tile to enable navigation through the tiles.
        private void IgPreviewClicked_Click(object sender, EventArgs e)
        {
            Bitmap image = GetImage();
            if (image != null) pictureBoxPreview.Image = image;
        }
        #endregion

        #region [ previousButton_Click ]  / NextButton_Click – Navigating through the tiles
        private void previousButton_Click(object sender, EventArgs e)
        {
            if (_currentTile <= 0) return;
            _currentTile--;
            ShowTile();
        }
        #endregion

        #region [ NextButton_Click ] – Navigates to the next tile in the sequence, ensuring it does not exceed the maximum tile index, and updates the preview accordingly.
        private void NextButton_Click(object sender, EventArgs e)
        {
            if (_currentTile >= MaxTileIndex) return;
            _currentTile++;
            ShowTile();
        }
        #endregion

        #region [ ShowTile ] – Loads the tile with the current ID and displays it in the preview to enable navigation through the tiles.
        private void ShowTile()
        {
            _landTile = _currentTile;
            pictureBoxPreview.Image = GetImage();
        }
        #endregion

        #region [ importToPrewiewToolStripMenuItem_Click ] – Imports an image from the clipboard, applies the diamond cropping and scaling, and sets it as the preview background.
        private void importToPrewiewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show("The clipboard contains no image.", "Import to Preview",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Image clipImage = Clipboard.GetImage();
            Bitmap tiled = GetImageFromImport(clipImage);
            if (tiled != null) pictureBoxPreview.Image = tiled;
        }
        #endregion

        // ═══════════════════════════════════════════════════════════════════
        //  PREVIEW HINTERGRUND CYCLE
        // ═══════════════════════════════════════════════════════════════════

        #region [ BtBackground_Click ] – Cycles the preview background color between standard, black, and white to help visualize different tile designs.
        private int _backgroundCycle = 0;

        private void BtBackground_Click(object sender, EventArgs e)
        {
            _backgroundCycle = (_backgroundCycle + 1) % 3;
            switch (_backgroundCycle)
            {
                case 0:
                    pictureBoxPreview.BackColor = SystemColors.Control;
                    btBackground.Text = "Standard";
                    break;
                case 1:
                    pictureBoxPreview.BackColor = Color.Black;
                    btBackground.Text = "Black";
                    break;
                case 2:
                    pictureBoxPreview.BackColor = Color.White;
                    btBackground.Text = "White";
                    break;
            }
        }
        #endregion

        // ═══════════════════════════════════════════════════════════════════
        //  ROTATE / MIRROR
        // ═══════════════════════════════════════════════════════════════════

        #region btImageLeft_Click / btImageRight_Click / mirrorToolStripMenuItem_Click
        private void btImageLeft_Click(object sender, EventArgs e)
        {
            if (pictureBoxTexture.Image == null)
            {
                MessageBox.Show("Kein Bild zum Drehen.", "Rotate",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            pictureBoxTexture.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            pictureBoxTexture.Refresh();
        }
        #endregion

        #region [ btImageRight_Click ] – Rotates the image 90 degrees to the right using the RotateFlip method with Rotate90FlipNone.
        private void BtImageRight_Click(object sender, EventArgs e)
        {
            if (pictureBoxTexture.Image == null)
            {
                MessageBox.Show("Kein Bild zum Drehen.", "Rotate",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            pictureBoxTexture.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            pictureBoxTexture.Refresh();
        }
        #endregion

        #region [ mirrorToolStripMenuItem_Click ] – Flip the image horizontally using the RotateFlip method with RotateNoneFlipX.
        private void mirrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxTexture.Image == null)
            {
                MessageBox.Show("No image to mirror.", "Mirror",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            pictureBoxTexture.Image.RotateFlip(RotateFlipType.RotateNoneFlipX);
            pictureBoxTexture.Refresh();
        }
        #endregion

        // ═══════════════════════════════════════════════════════════════════
        //  TRIANGLE MASK
        // ═══════════════════════════════════════════════════════════════════

        #region [ triangleToolStripMenuItem_Click ] – Applies a triangle mask to the image, filling the lower half with black to create a simple geometric effect.
        private void triangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxTexture.Image == null)
            {
                MessageBox.Show("No image available for editing.", "Triangle Mask",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Bitmap bmp = new Bitmap(pictureBoxTexture.Image);
            using (Graphics g = Graphics.FromImage(bmp))
            using (SolidBrush brush = new SolidBrush(Color.Black))
            {
                Rectangle lowerHalf = new Rectangle(0, bmp.Height / 2, bmp.Width, bmp.Height / 2);
                g.FillRectangle(brush, lowerHalf);
            }

            pictureBoxTexture.Image = bmp;
        }
        #endregion

        // ═══════════════════════════════════════════════════════════════════
        //  CONTRAST TRACKBAR
        // ═══════════════════════════════════════════════════════════════════

        #region [ trackBarContrast_ValueChanged ] – Adjusts the contrast of the displayed image based on the trackbar value, with real-time preview.
        private void trackBarContrast_ValueChanged(object sender, EventArgs e)
        {
            _currentContrast = (float)Math.Pow((100.0 + trackBarColor.Value) / 100.0, 2);
            labelContrastValue.Text = $"Contrast: {trackBarColor.Value}";

            if (_originalImage == null) return;

            if (trackBarColor.Value == 0)
            {
                pictureBoxTexture.Image = (Image)_originalImage.Clone();
                return;
            }

            // Apply contrast to the displayed image (as before)
            Bitmap bmp = new Bitmap(_originalImage);
            ColorMatrix cm = new ColorMatrix(new[]
            {
                new[] { _currentContrast, 0f, 0f, 0f, 0f },
                new[] { 0f, _currentContrast, 0f, 0f, 0f },
                new[] { 0f, 0f, _currentContrast, 0f, 0f },
                new[] { 0f, 0f, 0f, 1f, 0f },
                new[] { 0f, 0f, 0f, 0f, 1f }
            });

            using (ImageAttributes ia = new ImageAttributes())
            {
                ia.SetColorMatrix(cm);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(_originalImage, new Rectangle(0, 0, bmp.Width, bmp.Height),
                        0, 0, _originalImage.Width, _originalImage.Height, GraphicsUnit.Pixel, ia);
                }
            }
            pictureBoxTexture.Image = bmp;
        }
        #endregion

        // ═══════════════════════════════════════════════════════════════════
        //  CREATE TEXTURE (64×64 / 128×128)
        // ═══════════════════════════════════════════════════════════════════

        #region [ BtCreateTexture_Click ] – Scales the current texture to 64×64 or 128×128 pixels, based on the selected checkboxes.
        private void BtCreateTexture_Click(object sender, EventArgs e)
        {
            if (pictureBoxTexture.Image == null)
            {
                MessageBox.Show("No image to scale.", "Create Texture",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Size targetSize;
            if (checkBox64x64.Checked) targetSize = new Size(64, 64);
            else if (checkBox128x128.Checked) targetSize = new Size(128, 128);
            else
            {
                MessageBox.Show("Please select a target size (64×64 or 128×128).",
                    "Create Texture", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Bitmap resized = new Bitmap(pictureBoxTexture.Image, targetSize);
            pictureBoxTexture.Image = resized;
            lbTextureSize.Text = $"Image size: {targetSize.Width} x {targetSize.Height} px";
        }
        #endregion

        #region checkBox64x64_CheckedChanged / checkBox128x128_CheckedChanged
        private void checkBox64x64_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox64x64.Checked) checkBox128x128.Checked = false;
        }

        private void checkBox128x128_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox128x128.Checked) checkBox64x64.Checked = false;
        }
        #endregion

        // ═══════════════════════════════════════════════════════════════════
        //  COLOR ANALYSIS
        // ═══════════════════════════════════════════════════════════════════

        #region [ BtColorHex_Click ] – Analyzes the colors in the image, lists them with hex codes, and allows the codes to be copied.
        private void BtColorHex_Click(object sender, EventArgs e)
        {
            if (pictureBoxTexture.Image == null)
            {
                MessageBox.Show("No image to analyze.", "Color Analysis",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Bitmap bmp = new Bitmap(pictureBoxTexture.Image);
            var colors = new Dictionary<string, Color>();

            for (int x = 0; x < bmp.Width; x++)
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color c = bmp.GetPixel(x, y);
                    string hex = c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
                    colors.TryAdd(hex, c);
                }

            rtBoxInfo.Clear();
            foreach (var entry in colors)
            {
                rtBoxInfo.SelectionStart = rtBoxInfo.TextLength;
                rtBoxInfo.SelectionLength = 0;
                rtBoxInfo.SelectionBackColor = entry.Value;
                rtBoxInfo.AppendText("    ");
                rtBoxInfo.SelectionBackColor = rtBoxInfo.BackColor;
                rtBoxInfo.AppendText("  #" + entry.Key + Environment.NewLine);
            }

            rtBoxInfo.MouseClick += (s, ev) =>
            {
                string selected = rtBoxInfo.SelectedText.Trim();
                if (selected.Length == 0) return;
                tBoxInfoColor.Text = selected.TrimStart('#');
                Clipboard.SetText(selected);
            };
        }
        #endregion

        // ═══════════════════════════════════════════════════════════════════
        //  COLOR REPLACEMENT
        // ═══════════════════════════════════════════════════════════════════

        #region [ BtReplaceColor_Click ] – Replaces all pixels of a specific color with a new color, based on the hex codes in the text fields.
        private void BtReplaceColor_Click(object sender, EventArgs e)
        {
            if (pictureBoxTexture.Image == null)
            {
                MessageBox.Show("No image available for editing.", "Replace Color",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string oldCode = NormalizeHex(tBoxInfoColor.Text);
            string newCode = NormalizeHex(tbColorSet.Text);

            if (!IsHexColor(oldCode) || !IsHexColor(newCode))
            {
                MessageBox.Show("Invalid hex color code.\nPlease enter a 3- or 6-digit hex value., z.B. FF8800 oder #FF8800.",
                    "Replace Color", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Color oldColor = ColorTranslator.FromHtml(oldCode);
            Color newColor = ColorTranslator.FromHtml(newCode);

            Bitmap bmp = new Bitmap(pictureBoxTexture.Image);
            for (int x = 0; x < bmp.Width; x++)
                for (int y = 0; y < bmp.Height; y++)
                {
                    if (bmp.GetPixel(x, y) == oldColor)
                        bmp.SetPixel(x, y, newColor);
                }

            pictureBoxTexture.Image = bmp;
        }
        #endregion        
        private static bool IsHexColor(string colorCode)
            => Regex.IsMatch(colorCode, @"^#(?:[0-9a-fA-F]{3}){1,2}$"); // Validates that the input string is a valid hex color code, allowing for both 3-digit and 6-digit formats, with an optional leading '#'.

        #region [ NormalizeHex ] – Removes unnecessary spaces and adds missing '#' to ensure consistent formatting of hex color codes.
        private static string NormalizeHex(string raw)
        {
            raw = raw?.Trim() ?? string.Empty;
            return raw.StartsWith("#") ? raw : "#" + raw;
        }
        #endregion

        #region [ BtColorDialog_Click ] – Open a color picker dialog and set the selected color as the new color code in the text field.
        private void BtColorDialog_Click(object sender, EventArgs e)
        {
            using (ColorDialog dlg = new ColorDialog())
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                tbColorSet.Text = $"#{dlg.Color.R:X2}{dlg.Color.G:X2}{dlg.Color.B:X2}";
            }
        }
        #endregion

        #region [ BtCopyColorCode_Click ] – Copies the hex color values ​​from the info text field to the clipboard.
        private void BtCopyColorCode_Click(object sender, EventArgs e)
        {
            string text = rtBoxInfo.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                MessageBox.Show("No color codes to copy..\nFirst run 'Analyze'.",
                    "Copy Colors", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length > 0 && !line.StartsWith("#"))
                    lines[i] = "#" + line;
                else
                    lines[i] = line;
            }

            string result = string.Join(Environment.NewLine, lines).Trim();
            Clipboard.SetText(result);
            MessageBox.Show($"{lines.Length} Color code(s) copied to clipboard.",
                "Copy Colors", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        // ═══════════════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ═══════════════════════════════════════════════════════════════════

        #region ReplaceOriginalImage
        private void ReplaceOriginalImage(Image newImage)
        {
            _originalImage?.Dispose();
            _originalImage = newImage;
        }
        #endregion

        #region Form Dispose override
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _originalImage?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion

        #region [ BtToggleGrid_Click ] – Toggle visibility of the isometric grid overlay in the preview
        private void BtToggleGrid_Click(object sender, EventArgs e)
        {
            _showIsoGrid = !_showIsoGrid;
            btToggleGrid.Text = _showIsoGrid ? "Hide grid" : "Show grid";

            // Update preview immediately
            if (pictureBoxPreview.Image != null)
            {
                // Simply regenerate the current preview
                if (pictureBoxPreview.Tag is Image imported)   // if currently imported
                    pictureBoxPreview.Image = GetImageFromImport(imported);
                else
                    pictureBoxPreview.Image = GetImage();
            }
        }
        #endregion

        #region [ DrawIsoGrid ] – thin diamond grid as a guideline
        private void DrawIsoGrid(Graphics g, int width, int height)
        {
            using Pen pen = new Pen(Color.FromArgb(120, 255, 255, 255), 1f); // semi-transparent white

            for (int y = -22; y <= height; y += 22)
            {
                int startX = (y / 22 % 2 == 0) ? 0 : -22;
                for (int x = startX; x <= width; x += 44)
                {
                    // Raute zeichnen
                    g.DrawLine(pen, x + 22, y, x + 44, y + 22); // right edge
                    g.DrawLine(pen, x + 22, y, x, y + 22); // left edge
                    g.DrawLine(pen, x, y + 22, x + 22, y + 44); // lower left
                    g.DrawLine(pen, x + 44, y + 22, x + 22, y + 44); // lower right
                }
            }
        }
        #endregion

        #region [ ApplyContrast ] – applies contrast adjustment to an image using a ColorMatrix
        private Bitmap ApplyContrast(Image image, float contrastFactor)
        {
            Bitmap bmp = new Bitmap(image);
            ColorMatrix cm = new ColorMatrix(new[]
            {
                new[] { contrastFactor, 0f, 0f, 0f, 0f },
                new[] { 0f, contrastFactor, 0f, 0f, 0f },
                new[] { 0f, 0f, contrastFactor, 0f, 0f },
                new[] { 0f, 0f, 0f, 1f, 0f },
                new[] { 0f, 0f, 0f, 0f, 1f }
            });

            using (ImageAttributes ia = new ImageAttributes())
            {
                ia.SetColorMatrix(cm);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height),
                        0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ia);
                }
            }
            return bmp;
        }
        #endregion
    }
}
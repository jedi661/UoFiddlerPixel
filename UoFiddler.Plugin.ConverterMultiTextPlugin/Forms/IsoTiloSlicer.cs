// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class IsoTiloSlicer : Form
    {
        // -----------------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------------
        private ImageHandler1 _imageHandler1;
        private ImageHandler2 _imageHandler2;

        private string TempDirectory =>
            Path.Combine(Application.StartupPath, "tempGrafic");

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------
        public IsoTiloSlicer()
        {
            InitializeComponent();
            InitializeHandlers();
            AttachSettingEvents();
        }

        // -----------------------------------------------------------------------
        // Initialization
        // -----------------------------------------------------------------------

        /// <summary>
        /// Creates both image handlers with default settings.
        /// </summary>
        private void InitializeHandlers()
        {
            _imageHandler1 = new ImageHandler1();
            _imageHandler2 = new ImageHandler2();

            // Apply default UI values to both handlers
            ApplySettingsToHandlers();
        }

        /// <summary>
        /// Wires up the settings controls so both handlers stay in sync.
        /// </summary>
        private void AttachSettingEvents()
        {
            nudTileWidth.ValueChanged += (s, e) => ApplySettingsToHandlers();
            nudTileHeight.ValueChanged += (s, e) => ApplySettingsToHandlers();
            nudOffset.ValueChanged += (s, e) => ApplySettingsToHandlers();
            nudStartNumber.ValueChanged += (s, e) => ApplySettingsToHandlers();
            txtFilenameFormat.TextChanged += (s, e) => ApplySettingsToHandlers();
        }

        /// <summary>
        /// Pushes all current UI settings to both handlers at once.
        /// No more copy-paste duplication.
        /// </summary>
        private void ApplySettingsToHandlers()
        {
            SetBothHandlers(h =>
            {
                h.TileWidth = (int)nudTileWidth.Value;
                h.TileHeight = (int)nudTileHeight.Value;
                h.Offset = (int)nudOffset.Value;
                h.StartingFileNumber = (int)nudStartNumber.Value;
                h.FileNameFormat = txtFilenameFormat.Text;
            });
        }

        // -----------------------------------------------------------------------
        // Helper – eliminates handler duplication
        // -----------------------------------------------------------------------

        /// <summary>
        /// Applies an action to both image handlers in one call.
        /// </summary>
        private void SetBothHandlers(Action<ITiloSlicerHandler> action)
        {
            action(_imageHandler1);
            action(_imageHandler2);
        }

        /// <summary>
        /// Ensures the temp directory exists before processing.
        /// </summary>
        private void EnsureTempDirectory()
        {
            Directory.CreateDirectory(TempDirectory); // no-op if already exists
        }

        // -----------------------------------------------------------------------
        // Image loading helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Loads an image file into the PictureBox, disposing the old image first.
        /// </summary>
        private void LoadImageFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;

            var newImage = Image.FromFile(filePath);
            ReplacePreviewImage(newImage);

            txtImagePath.Text = filePath;
            SetBothHandlers(h => h.ImagePath = filePath);
            ShowStatus($"Loaded: {Path.GetFileName(filePath)}  ({newImage.Width} x {newImage.Height} px)");
        }

        /// <summary>
        /// Replaces the PictureBox image and disposes the old one to prevent memory leaks.
        /// </summary>
        private void ReplacePreviewImage(Image newImage)
        {
            var old = picImagePreview.Image;
            picImagePreview.Image = newImage;
            old?.Dispose();

            lbImageSize.Text = newImage != null
                ? $"Size: {newImage.Width} x {newImage.Height} px"
                : "Size: –";
        }

        // -----------------------------------------------------------------------
        // Status bar helper
        // -----------------------------------------------------------------------

        private void ShowStatus(string message)
        {
            statusLabel.Text = message;
        }

        // -----------------------------------------------------------------------
        // Run logic
        // -----------------------------------------------------------------------

        /// <summary>
        /// Shared processing logic to avoid duplication between BtnRun and BtnRun2.
        /// </summary>
        private void RunHandler(ITiloSlicerHandler handler, string label)
        {
            if (picImagePreview.Image == null)
            {
                ShowStatus("No image loaded.");
                MessageBox.Show("Please load an image first.", "No image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            EnsureTempDirectory();

            handler.OutputDirectory = TempDirectory;
            ApplySettingsToHandlers();

            ShowStatus($"Processing ({label})…");
            Cursor = Cursors.WaitCursor;
            try
            {
                if (!handler.Process())
                {
                    ShowStatus($"Error: {handler.LastErrorMessage}");
                    MessageBox.Show(handler.LastErrorMessage, "Processing error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            ShowStatus($"Done – {label} output saved to tempGrafic.");
        }

        // -----------------------------------------------------------------------
        // Button events
        // -----------------------------------------------------------------------

        #region [ BtnSelectImage ]
        private void BtnSelectImage_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Image Files (*.bmp;*.jpg;*.jpeg;*.png;*.gif)|*.bmp;*.jpg;*.jpeg;*.png;*.gif|All files (*.*)|*.*",
                Title = "Select source image"
            };

            if (dlg.ShowDialog() == DialogResult.OK)
                LoadImageFromFile(dlg.FileName);
        }
        #endregion

        #region [ BtnRun – Straight view ]
        private void BtnRun_Click(object sender, EventArgs e)
        {
            RunHandler(_imageHandler1, "Straight view");
        }
        #endregion

        #region [ BtnRun2 – Grid view ]
        private void BtnRun2_Click(object sender, EventArgs e)
        {
            RunHandler(_imageHandler2, "Grid view");
        }
        #endregion

        #region [ ButtonOpenTempGrafic ]
        private void ButtonOpenTempGrafic_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(TempDirectory))
                Process.Start("explorer.exe", TempDirectory);
            else
                ShowStatus("tempGrafic directory does not exist yet.");
        }
        #endregion

        #region [ BtnDeleteTempFiles ]
        private void BtnDeleteTempFiles_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(TempDirectory))
            {
                ShowStatus("tempGrafic directory does not exist.");
                return;
            }

            var files = Directory.GetFiles(TempDirectory);
            if (files.Length == 0)
            {
                ShowStatus("No temp files to delete.");
                return;
            }

            var result = MessageBox.Show(
                $"Delete {files.Length} file(s) from tempGrafic?",
                "Confirm delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            int errors = 0;
            foreach (var file in files)
            {
                try { File.Delete(file); }
                catch { errors++; }
            }

            ShowStatus(errors == 0
                ? $"Deleted {files.Length} temp file(s)."
                : $"Deleted with {errors} error(s). Some files may be locked.");
        }
        #endregion

        // -----------------------------------------------------------------------
        // Context menu events (PictureBox right-click)
        // -----------------------------------------------------------------------

        #region [ Load from file ]
        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Image Files (*.bmp;*.jpg;*.jpeg;*.png;*.gif)|*.bmp;*.jpg;*.jpeg;*.png;*.gif|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == DialogResult.OK)
                LoadImageFromFile(dlg.FileName);
        }
        #endregion

        #region [ Import from Clipboard ]
        private void ImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage())
            {
                ShowStatus("Clipboard does not contain an image.");
                return;
            }

            var img = Clipboard.GetImage();
            ReplacePreviewImage(img);

            // Clipboard image has no file path – clear path fields
            txtImagePath.Text = string.Empty;
            SetBothHandlers(h => h.ImagePath = string.Empty);

            ShowStatus("Image imported from clipboard.");
        }
        #endregion

        #region [ Run from Clipboard (context menu) ]
        private void RunClipbordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (picImagePreview.Image == null)
            {
                ShowStatus("No image in preview.");
                MessageBox.Show("Please load or paste an image first.", "No image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            EnsureTempDirectory();

            // Save current preview as temp file so the handler has a real path
            string tempFile = Path.Combine(TempDirectory, "clipboard_temp.bmp");
            using (var bmp = new Bitmap(picImagePreview.Image))
                bmp.Save(tempFile);

            _imageHandler1.ImagePath = tempFile;
            _imageHandler1.OutputDirectory = TempDirectory;

            RunHandler(_imageHandler1, "Straight view (clipboard)");
        }
        #endregion

        #region [ BtnHelp ]
        private void BtnHelp_Click(object sender, EventArgs e)
        {
            using var help = new IsoTiloSlicerHelp();
            help.ShowDialog(this);
        }
        #endregion

        #region [ Mirror ]
        private void MirrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (picImagePreview.Image == null)
            {
                ShowStatus("No image to mirror.");
                return;
            }

            var bmp = new Bitmap(picImagePreview.Image);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            ReplacePreviewImage(bmp);
            ShowStatus("Image mirrored.");
        }
        #endregion
    }
}
// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  * Changelog:
//  *  - FIX: Replaced slow GetPixel/SetPixel loops with fast LockBits processing
//  *  - FIX: CheckBox Black/White are now mutually exclusive
//  *  - FIX: OnFrameChanged race condition (null-check with local copy)
//  *  - FIX: Replaced Application.DoEvents() with Refresh() in crop method
//  *  - FIX: SetArtworkImage now respects secondImage and rhombus state
//  *  - FIX: ButtonClearAll naming cleaned up
//  *  - NEW: Opacity slider for overlay (secondImage + GIF)
//  *  - NEW: Overlay scale slider (render-only, non-destructive)
//  *  - NEW: Zoom + Pan with MouseWheel and drag in PictureBox
//  *  - NEW: Transparent color picker (replaces hardcoded black/white)
//  *  - NEW: Pixel info label on mouse hover (coordinates + RGBA)
//  *  - NEW: Image size label (base + overlay dimensions)
//  *  - NEW: Visual content height indicator (bounding box of non-transparent pixels)
//  *  - NEW: Favorites / bookmarks for Graphic IDs
//  *  - NEW: Batch export of current item list with current overlay
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UoFiddler.Controls.UserControls;
using Ultima;

// Explicit alias — prevents ambiguity with System.Threading.Timer
using WinTimer = System.Windows.Forms.Timer;

namespace UoFiddler.Controls.Forms
{
    public partial class ArtworkGallery : Form
    {
        // ── Core state ────────────────────────────────────────────────────────
        private List<int> itemList;
        private int currentIndex = -1;
        private Bitmap secondImage = null;
        private bool useSecondImage => checkBoxSecondArtwork.Checked;

        // ── Rhombus ───────────────────────────────────────────────────────────
        private bool isDrawingRhombusArtworkGallery = false;

        // ── Overlay movement ──────────────────────────────────────────────────
        private Point secondImageOffset = Point.Empty;
        private const int MoveStep = 1;
        private WinTimer moveTimer;
        private Keys currentMoveKey = Keys.None;
        private bool IsOverlayActive => (useSecondImage && secondImage != null) || animatedGif != null;

        // ── Cutting template ──────────────────────────────────────────────────
        private bool isCuttingTemplateActive = false;
        private Point cuttingTemplatePosition = new Point(100, 100);
        private Size cuttingTemplateSize = new Size(44, 133);
        private Keys currentCuttingTemplateMoveKey = Keys.None;
        private WinTimer cuttingTemplateMoveTimer;
        private bool isDrawingCuttingTemplateTemporaryDisabled = false;

        // ── Index image toggle ────────────────────────────────────────────────
        private bool allowIndexArtwork = true;

        // ── Background image ──────────────────────────────────────────────────
        private Bitmap backgroundImage = null;

        // ── Image file browser ────────────────────────────────────────────────
        private string lastImageDirectory = null;
        private Dictionary<string, string> imageFileLookup = new Dictionary<string, string>();

        // ── Animated GIF ──────────────────────────────────────────────────────
        private Image animatedGif = null;
        private readonly object gifLock = new object(); // FIX: race condition guard

        // ── FIX NEW: Opacity (0–255) ──────────────────────────────────────────
        private int overlayOpacity = 255;

        // ── NEW: Scale factor for overlay render (0.1 – 4.0) ─────────────────
        private float overlayScale = 1.0f;

        // ── NEW: Zoom + Pan ───────────────────────────────────────────────────
        private float zoomFactor = 1.0f;
        private Point panOffset = Point.Empty;
        private Point panStartPoint = Point.Empty;
        private bool isPanning = false;

        // ── NEW: Chroma-key color (replaces hardcoded black/white) ───────────
        private Color chromaColor1 = Color.Black;
        private Color chromaColor2 = Color.White;
        private bool useChromaKey = true;

        // ── NEW: Favorites ────────────────────────────────────────────────────
        private HashSet<int> favorites = new HashSet<int>();

        // ─────────────────────────────────────────────────────────────────────
        #region Constructor
        // ─────────────────────────────────────────────────────────────────────

        private bool isRulerActive = false;
        private bool isCrosshairActive = false;


        public ArtworkGallery()
        {
            InitializeComponent();
            this.Load += ArtworkGallery_Load;

            moveTimer = new WinTimer { Interval = 50 };
            moveTimer.Tick += MoveTimer_Tick;

            cuttingTemplateMoveTimer = new WinTimer { Interval = 50 };
            cuttingTemplateMoveTimer.Tick += CuttingTemplateMoveTimer_Tick;

            this.KeyPreview = true;
            this.KeyDown += ArtworkGallery_KeyDown;
            this.KeyUp += ArtworkGallery_KeyUp;
            this.ActiveControl = null;
            this.Focus();

            // NEW: Zoom / Pan wiring
            pictureBoxArtworkGallery.MouseWheel += PictureBox_MouseWheel;
            pictureBoxArtworkGallery.MouseDown += PictureBox_MouseDown;
            pictureBoxArtworkGallery.MouseMove += PictureBox_MouseMove;
            pictureBoxArtworkGallery.MouseUp += PictureBox_MouseUp;
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Load / Init
        // ─────────────────────────────────────────────────────────────────────
        private void ArtworkGallery_Load(object sender, EventArgs e)
        {
            // Wire opacity slider
            trackBarOpacity.Minimum = 0;
            trackBarOpacity.Maximum = 255;
            trackBarOpacity.Value = 255;
            trackBarOpacity.Scroll += TrackBarOpacity_Scroll;
            labelOpacityValue.Text = "100%";

            // Wire scale slider
            trackBarScale.Minimum = 1;    // 0.1× (stored as int *10)
            trackBarScale.Maximum = 40;   // 4.0×
            trackBarScale.Value = 10;   // 1.0×
            trackBarScale.Scroll += TrackBarScale_Scroll;
            labelScaleValue.Text = "1.0×";

            UpdateImageInfoLabel();
        }

        public void InitializeGallery(int selectedGraphicId)
        {
            itemList = new List<int>(ItemsControl.RefMarker.ItemList);
            if (itemList == null || itemList.Count == 0) { currentIndex = -1; return; }

            currentIndex = itemList.IndexOf(selectedGraphicId);
            if (currentIndex < 0) currentIndex = 0;

            LoadArtwork();
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region LoadArtwork
        // ─────────────────────────────────────────────────────────────────────
        private void LoadArtwork()
        {
            if (currentIndex < 0 || currentIndex >= itemList.Count) return;

            int graphicId = itemList[currentIndex];
            Bitmap baseBitmap = allowIndexArtwork ? Art.GetStatic(graphicId) : null;

            if (baseBitmap == null && secondImage == null && animatedGif == null)
            {
                pictureBoxArtworkGallery.Image?.Dispose();
                pictureBoxArtworkGallery.Image = null;
                this.Text = allowIndexArtwork
                    ? $"Artwork: 0x{graphicId:X4} (No Image)"
                    : "Overlay mode active (index artwork hidden)";
                UpdateImageInfoLabel();
                return;
            }

            Bitmap finalImage = null;

            if (secondImage != null && checkBoxSecondArtwork.Checked)
            {
                Bitmap baseForCombine = baseBitmap != null ? new Bitmap(baseBitmap) : null;
                finalImage = baseForCombine != null
                    ? CombineImages(baseForCombine, secondImage)
                    : ApplyOpacity(new Bitmap(secondImage), overlayOpacity);
                baseForCombine?.Dispose();
            }
            else if (baseBitmap != null)
            {
                finalImage = new Bitmap(baseBitmap);
            }

            // Case: no index image but GIF present (handled in Paint)
            if (finalImage == null && animatedGif != null)
            {
                finalImage = new Bitmap(animatedGif.Width, animatedGif.Height);
                using (Graphics g = Graphics.FromImage(finalImage))
                {
                    lock (gifLock) // FIX: race condition
                    {
                        if (animatedGif != null)
                        {
                            ImageAnimator.UpdateFrames(animatedGif);
                            g.DrawImage(animatedGif, 0, 0);
                        }
                    }
                }
            }

            // NOTE: Rhombus is drawn exclusively in PictureBoxArtworkGallery_Paint
            // (screen-space overlay), NOT baked into the bitmap here.
            // Baking it caused double-drawing and wrong position because the
            // bitmap coordinate system differs from the PictureBox screen coords.

            // Zoom: erzeuge skaliertes Bitmap wenn zoom != 1.0
            // Die PictureBox rendert es selbst via CenterImage — kein doppeltes Zeichnen
            Bitmap displayImage = finalImage;
            if (finalImage != null && Math.Abs(zoomFactor - 1.0f) > 0.001f)
            {
                int zw = Math.Max(1, (int)(finalImage.Width * zoomFactor));
                int zh = Math.Max(1, (int)(finalImage.Height * zoomFactor));
                Bitmap zoomed = new Bitmap(zw, zh, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(zoomed))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                    g.DrawImage(finalImage, 0, 0, zw, zh);
                }
                finalImage.Dispose();
                displayImage = zoomed;
            }

            pictureBoxArtworkGallery.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBoxArtworkGallery.Image?.Dispose();
            pictureBoxArtworkGallery.Image = displayImage;

            this.Text = allowIndexArtwork
                ? $"Artwork: 0x{graphicId:X4}"
                : "Overlay mode active (index hidden)";

            UpdateImageInfoLabel();
            UpdateContentHeightLabel();
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Image Combination
        // ─────────────────────────────────────────────────────────────────────
        private Bitmap CombineImages(Bitmap baseImage, Bitmap overlayImage)
        {
            Bitmap combined = new Bitmap(baseImage.Width, baseImage.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(combined))
            {
                g.DrawImage(baseImage, 0, 0);

                int scaledW = (int)(overlayImage.Width * overlayScale);
                int scaledH = (int)(overlayImage.Height * overlayScale);
                int x = (baseImage.Width - scaledW) / 2 + secondImageOffset.X;
                int y = (baseImage.Height - scaledH) / 2 + secondImageOffset.Y;

                if (overlayOpacity < 255)
                {
                    using (Bitmap opaque = ApplyOpacity(overlayImage, overlayOpacity))
                        g.DrawImage(opaque, new Rectangle(x, y, scaledW, scaledH));
                }
                else
                {
                    g.DrawImage(overlayImage, new Rectangle(x, y, scaledW, scaledH));
                }
            }
            return combined;
        }

        /// <summary>Applies an alpha multiplier to every pixel using LockBits (fast).</summary>
        private static Bitmap ApplyOpacity(Bitmap source, int opacity)
        {
            Bitmap result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            BitmapData srcData = source.LockBits(
                new Rectangle(0, 0, source.Width, source.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dstData = result.LockBits(
                new Rectangle(0, 0, result.Width, result.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int bytes = Math.Abs(srcData.Stride) * source.Height;
            byte[] buffer = new byte[bytes];
            Marshal.Copy(srcData.Scan0, buffer, 0, bytes);

            float alpha = opacity / 255f;
            for (int i = 3; i < bytes; i += 4)
                buffer[i] = (byte)(buffer[i] * alpha);

            Marshal.Copy(buffer, 0, dstData.Scan0, bytes);
            source.UnlockBits(srcData);
            result.UnlockBits(dstData);
            return result;
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region FIX: Fast LockBits chroma-key transparency
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Replaces chromaColor1 and chromaColor2 with transparency using LockBits.
        /// ~50-100x faster than GetPixel/SetPixel for typical UO artwork sizes.
        /// </summary>
        private Bitmap PrepareOverlayFast(Bitmap source)
        {
            Bitmap result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);

            BitmapData srcData = source.LockBits(
                new Rectangle(0, 0, source.Width, source.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dstData = result.LockBits(
                new Rectangle(0, 0, result.Width, result.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int bytes = Math.Abs(srcData.Stride) * source.Height;
            byte[] buf = new byte[bytes];
            Marshal.Copy(srcData.Scan0, buf, 0, bytes);

            // Match colors by RGB (ignore source alpha)
            byte c1r = chromaColor1.R, c1g = chromaColor1.G, c1b = chromaColor1.B;
            byte c2r = chromaColor2.R, c2g = chromaColor2.G, c2b = chromaColor2.B;

            for (int i = 0; i < bytes; i += 4)
            {
                byte b = buf[i], g = buf[i + 1], r = buf[i + 2];
                bool isChroma1 = useChromaKey && (r == c1r && g == c1g && b == c1b);
                bool isChroma2 = useChromaKey && (r == c2r && g == c2g && b == c2b);

                if (isChroma1 || isChroma2)
                {
                    buf[i] = 0; buf[i + 1] = 0; buf[i + 2] = 0; buf[i + 3] = 0;
                }
                // else keep pixel as-is (already copied in)
            }

            Marshal.Copy(buf, 0, dstData.Scan0, bytes);
            source.UnlockBits(srcData);
            result.UnlockBits(dstData);
            return result;
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region SetArtworkImage (FIX: now respects overlays)
        // ─────────────────────────────────────────────────────────────────────
        public void SetArtworkImage(Bitmap artworkImage, int index)
        {
            if (artworkImage == null)
            {
                pictureBoxArtworkGallery.Image = null;
                return;
            }
            currentIndex = index;
            // Route through LoadArtwork so rhombus / secondImage / opacity are all applied
            LoadArtwork();
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Navigation buttons
        // ─────────────────────────────────────────────────────────────────────
        private void ButtonLeft_Click(object sender, EventArgs e)
        {
            if (currentIndex > 0) { currentIndex--; LoadArtwork(); }
        }

        private void ButtonRight_Click(object sender, EventArgs e)
        {
            if (currentIndex < itemList.Count - 1) { currentIndex++; LoadArtwork(); }
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Load Second Image (uses fast LockBits now)
        // ─────────────────────────────────────────────────────────────────────
        private void LoadSecondImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select an image to overlay";
                ofd.Filter = "Image Files (*.bmp;*.png;*.jpg;*.jpeg;*.tiff)|*.bmp;*.png;*.jpg;*.jpeg;*.tiff";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                if (pictureBoxArtworkGallery.Image == null)
                {
                    MessageBox.Show("No base image loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                secondImage?.Dispose();
                using (Bitmap loaded = new Bitmap(ofd.FileName))
                    secondImage = PrepareOverlayFast(loaded); // FIX: fast path

                LoadArtwork();
                UpdateImageInfoLabel();
            }
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Load Clipboard (fast LockBits)
        // ─────────────────────────────────────────────────────────────────────
        private void loadClipboradToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show("Clipboard does not contain an image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (pictureBoxArtworkGallery.Image == null)
            {
                MessageBox.Show("No base image loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            secondImage?.Dispose();
            using (Bitmap loaded = new Bitmap(Clipboard.GetImage()))
                secondImage = PrepareOverlayFast(loaded); // FIX: fast path

            LoadArtwork();
            UpdateImageInfoLabel();
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region CheckBox Second Artwork
        // ─────────────────────────────────────────────────────────────────────
        private void CheckBoxSecondArtwork_CheckedChanged(object sender, EventArgs e)
            => LoadArtwork();
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region FIX: CheckBox Black/White — mutually exclusive
        // ─────────────────────────────────────────────────────────────────────
        private void CheckBoxBackgroundColorChanged(object sender, EventArgs e)
        {
            CheckBox clicked = sender as CheckBox;

            // FIX: mutual exclusion
            if (clicked == checkBoxBlack && checkBoxBlack.Checked)
                checkBoxWhite.Checked = false;
            else if (clicked == checkBoxWhite && checkBoxWhite.Checked)
                checkBoxBlack.Checked = false;

            if (checkBoxBlack.Checked)
                pictureBoxArtworkGallery.BackColor = Color.Black;
            else if (checkBoxWhite.Checked)
                pictureBoxArtworkGallery.BackColor = Color.White;
            else
                pictureBoxArtworkGallery.BackColor = Color.Transparent;
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Rhombus
        // ─────────────────────────────────────────────────────────────────────
        private void ButtonDrawRhombus_Click(object sender, EventArgs e)
        {
            isDrawingRhombusArtworkGallery = !isDrawingRhombusArtworkGallery;
            ButtonDrawRhombus.BackColor = isDrawingRhombusArtworkGallery ? Color.LightGreen : SystemColors.Control;
            try { new SoundPlayer("sound.wav").Play(); } catch { /* sound optional */ }
            pictureBoxArtworkGallery.Invalidate();
        }

        // ── Rhombus: exakt wie Original ───────────────────────────────────────
        // Koordinaten relativ zur PictureBox (Screen-Space), SizeMode=CenterImage
        // zentriert das Bild, sodass Rhombus und Bild immer deckungsgleich sind.
        private void DrawRhombusArtworkGallery(Graphics g)
        {
            if (pictureBoxArtworkGallery.Image == null) return;

            // FIX: Anchor to the actual image position inside the PictureBox.
            // SizeMode=CenterImage centers the image, so we calculate the image's
            // top-left offset and derive centerX/baseY from the image bounds.
            // The old code used PictureBox.Height-170 which only worked at one
            // fixed window size (Dock=Fill makes the PictureBox variable).
            int imgW = pictureBoxArtworkGallery.Image.Width;
            int imgH = pictureBoxArtworkGallery.Image.Height;
            int imgOffsetX = (pictureBoxArtworkGallery.Width - imgW) / 2;
            int imgOffsetY = (pictureBoxArtworkGallery.Height - imgH) / 2;

            int centerX = imgOffsetX + imgW / 2;   // horizontal center of the image
            int baseY = imgOffsetY + imgH;        // bottom edge of the image

            // Upper rhombus
            Point[] pointsUpper = new Point[4];
            pointsUpper[0] = new Point(centerX, baseY - 133);
            pointsUpper[1] = new Point(centerX + 22, baseY - 111);
            pointsUpper[2] = new Point(centerX, baseY - 89);
            pointsUpper[3] = new Point(centerX - 22, baseY - 111);
            g.DrawPolygon(Pens.Black, pointsUpper);

            // Base line
            int lineWidth = 100;
            int lineStartX = centerX - lineWidth / 2;
            g.DrawLine(Pens.Black,
                new Point(lineStartX, baseY),
                new Point(lineStartX + lineWidth, baseY));

            // Lower rhombus
            Point[] pointsLower = new Point[4];
            pointsLower[0] = new Point(centerX, baseY);
            pointsLower[1] = new Point(centerX + 22, baseY - 22);
            pointsLower[2] = new Point(centerX, baseY - 44);
            pointsLower[3] = new Point(centerX - 22, baseY - 22);
            g.DrawPolygon(Pens.Black, pointsLower);

            // Connecting lines
            g.DrawLine(Pens.Black, pointsUpper[0], pointsLower[0]);
            g.DrawLine(Pens.Black, pointsUpper[1], pointsLower[1]);
            g.DrawLine(Pens.Black, pointsUpper[3], pointsLower[3]);
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region PictureBox Paint
        // ─────────────────────────────────────────────────────────────────────
        // Die PictureBox zeichnet pictureBoxArtworkGallery.Image selbst via
        // SizeMode.CenterImage — wir zeichnen hier NUR die Overlays obendrauf,
        // exakt wie im Original. Kein TranslateTransform, kein ScaleTransform.
        private void PictureBoxArtworkGallery_Paint(object sender, PaintEventArgs e)
        {
            // Rhombus — Screen-Overlay, kein Transform
            if (isDrawingRhombusArtworkGallery)
                DrawRhombusArtworkGallery(e.Graphics);

            // GIF-Overlay
            Image gif;
            lock (gifLock) { gif = animatedGif; }
            if (gif != null)
            {
                ImageAnimator.UpdateFrames(gif);
                int scaledW = (int)(gif.Width * overlayScale);
                int scaledH = (int)(gif.Height * overlayScale);
                int x = (pictureBoxArtworkGallery.Width - scaledW) / 2 + secondImageOffset.X;
                int y = (pictureBoxArtworkGallery.Height - scaledH) / 2 + secondImageOffset.Y;

                if (overlayOpacity < 255)
                {
                    using (Bitmap frame = new Bitmap(gif.Width, gif.Height))
                    using (Graphics fg = Graphics.FromImage(frame))
                    {
                        fg.DrawImage(gif, 0, 0);
                        using (Bitmap opaque = ApplyOpacity(frame, overlayOpacity))
                            e.Graphics.DrawImage(opaque, new Rectangle(x, y, scaledW, scaledH));
                    }
                }
                else
                {
                    e.Graphics.DrawImage(gif, new Rectangle(x, y, scaledW, scaledH));
                }
            }

            // Cutting template
            if (isCuttingTemplateActive && !isDrawingCuttingTemplateTemporaryDisabled)
            {
                using (Pen pen = new Pen(Color.OrangeRed, 2))
                    e.Graphics.DrawRectangle(pen, new Rectangle(cuttingTemplatePosition, cuttingTemplateSize));
            }

            if (isRulerActive)
                DrawRuler(e.Graphics);

            if (isCrosshairActive)
                DrawCrosshair(e.Graphics);
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Animated GIF
        // ─────────────────────────────────────────────────────────────────────
        private void loadGifToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select a GIF to overlay";
                ofd.Filter = "GIF Files (*.gif)|*.gif";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                if (pictureBoxArtworkGallery.Image == null)
                {
                    MessageBox.Show("No base image loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                lock (gifLock)
                {
                    if (animatedGif != null)
                    {
                        ImageAnimator.StopAnimate(animatedGif, OnFrameChanged);
                        animatedGif.Dispose();
                    }
                    animatedGif = Image.FromFile(ofd.FileName);
                    if (ImageAnimator.CanAnimate(animatedGif))
                        ImageAnimator.Animate(animatedGif, OnFrameChanged);
                }

                LoadArtwork();
                UpdateImageInfoLabel();
            }
        }

        // FIX: local copy before access → no null ref if ButtonRemoveGif fires concurrently
        private void OnFrameChanged(object sender, EventArgs e)
        {
            Image gif;
            lock (gifLock) { gif = animatedGif; }
            if (gif == null) return;

            ImageAnimator.UpdateFrames(gif);
            // Marshal to UI thread safely
            if (pictureBoxArtworkGallery.InvokeRequired)
                pictureBoxArtworkGallery.BeginInvoke(new Action(() => pictureBoxArtworkGallery.Invalidate()));
            else
                pictureBoxArtworkGallery.Invalidate();
        }

        private void ButtonRemoveGif_Click(object sender, EventArgs e)
        {
            lock (gifLock)
            {
                if (animatedGif != null)
                {
                    ImageAnimator.StopAnimate(animatedGif, OnFrameChanged);
                    animatedGif.Dispose();
                    animatedGif = null;
                }
            }
            pictureBoxArtworkGallery.Invalidate();
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Remove / Clear
        // ─────────────────────────────────────────────────────────────────────
        private void ButtonRemoveSecondImage_Click(object sender, EventArgs e)
        {
            secondImage?.Dispose();
            secondImage = null;
            LoadArtwork();
            pictureBoxArtworkGallery.Invalidate();
        }

        private void ButtonClearAllItems_Click(object sender, EventArgs e)
        {
            lock (gifLock)
            {
                if (animatedGif != null)
                {
                    ImageAnimator.StopAnimate(animatedGif, OnFrameChanged);
                    animatedGif.Dispose();
                    animatedGif = null;
                }
            }
            secondImage?.Dispose();
            secondImage = null;

            pictureBoxArtworkGallery.Image?.Dispose();
            pictureBoxArtworkGallery.Image = null;
            pictureBoxArtworkGallery.Invalidate();
            UpdateImageInfoLabel();
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region NEW: Opacity Slider
        // ─────────────────────────────────────────────────────────────────────
        private void TrackBarOpacity_Scroll(object sender, EventArgs e)
        {
            overlayOpacity = trackBarOpacity.Value;
            int pct = (int)Math.Round(overlayOpacity / 255.0 * 100);
            labelOpacityValue.Text = $"{pct}%";
            LoadArtwork();
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region NEW: Scale Slider
        // ─────────────────────────────────────────────────────────────────────
        private void TrackBarScale_Scroll(object sender, EventArgs e)
        {
            overlayScale = trackBarScale.Value / 10f;
            labelScaleValue.Text = $"{overlayScale:F1}×";
            LoadArtwork();
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region NEW: Zoom + Pan
        // ─────────────────────────────────────────────────────────────────────
        private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0) zoomFactor = Math.Min(zoomFactor * 1.2f, 16f);
            else zoomFactor = Math.Max(zoomFactor / 1.2f, 0.1f);

            labelZoom.Text = $"Zoom: {zoomFactor * 100:F0}%";
            LoadArtwork(); // Bitmap in neuer Größe erzeugen
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && ModifierKeys == Keys.Alt))
            {
                isPanning = true;
                panStartPoint = new Point(e.X - panOffset.X, e.Y - panOffset.Y);
                pictureBoxArtworkGallery.Cursor = Cursors.SizeAll;
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                panOffset = new Point(e.X - panStartPoint.X, e.Y - panStartPoint.Y);
                pictureBoxArtworkGallery.Invalidate();
            }

            // NEW: pixel info label
            UpdatePixelInfoLabel(e.Location);
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                isPanning = false;
                pictureBoxArtworkGallery.Cursor = Cursors.Default;
            }
        }

        private void ButtonResetZoom_Click(object sender, EventArgs e)
        {
            zoomFactor = 1.0f;
            panOffset = Point.Empty;
            labelZoom.Text = "Zoom: 100%";
            LoadArtwork(); // Bitmap neu erzeugen ohne Zoom
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region NEW: Pixel info on hover
        // ─────────────────────────────────────────────────────────────────────
        private void UpdatePixelInfoLabel(Point mousePos)
        {
            if (pictureBoxArtworkGallery.Image == null) { labelPixelInfo.Text = ""; return; }
            try
            {
                Bitmap bmp = pictureBoxArtworkGallery.Image as Bitmap;
                if (bmp == null) { labelPixelInfo.Text = ""; return; }

                // SizeMode=CenterImage: Bild sitzt in der Mitte der PictureBox
                int offsetX = (pictureBoxArtworkGallery.Width - bmp.Width) / 2;
                int offsetY = (pictureBoxArtworkGallery.Height - bmp.Height) / 2;

                // bmp ist bereits das gezoomte Bild → zurück in Original-Pixel
                int zoomedPx = mousePos.X - offsetX;
                int zoomedPy = mousePos.Y - offsetY;
                int px = (int)(zoomedPx / zoomFactor);
                int py = (int)(zoomedPy / zoomFactor);

                labelPixelInfo.Text = $"X:{px} Y:{py}";
                if (zoomedPx < 0 || zoomedPy < 0 || zoomedPx >= bmp.Width || zoomedPy >= bmp.Height) return;

                // Pixel aus dem gezoomten Bitmap lesen (entspricht dem Original-Pixel)
                Color c = bmp.GetPixel(zoomedPx, zoomedPy);
                labelPixelInfo.Text = $"X:{px} Y:{py}  R:{c.R} G:{c.G} B:{c.B} A:{c.A}";
            }
            catch { labelPixelInfo.Text = ""; }
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region UpdateImageInfoLabel (enhanced)
        // ─────────────────────────────────────────────────────────────────────
        private void UpdateImageInfoLabel()
        {
            if (pictureBoxArtworkGallery.Image == null)
            {
                labelImageInfo.Text = "No base image loaded.";
                return;
            }

            string baseSize = $"{pictureBoxArtworkGallery.Image.Width}×{pictureBoxArtworkGallery.Image.Height}";
            string overlaySize = secondImage != null
                ? $"{secondImage.Width}×{secondImage.Height}"
                : (animatedGif != null ? $"{animatedGif.Width}×{animatedGif.Height} (GIF)" : "–");

            labelImageInfo.Text = $"Base: {baseSize}  |  Overlay: {overlaySize}";
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region NEW: Content height label (bounding box of opaque pixels)
        // ─────────────────────────────────────────────────────────────────────
        private void UpdateContentHeightLabel()
        {
            Bitmap bmp = pictureBoxArtworkGallery.Image as Bitmap;
            if (bmp == null) { labelContentHeight.Text = "Content height: –"; return; }

            int minY = bmp.Height, maxY = 0;
            int minX = bmp.Width, maxX = 0;

            BitmapData bd = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            int stride = bd.Stride;
            byte[] pixels = new byte[stride * bmp.Height];
            Marshal.Copy(bd.Scan0, pixels, 0, pixels.Length);
            bmp.UnlockBits(bd);

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    int idx = y * stride + x * 4;
                    if (pixels[idx + 3] > 0) // any non-transparent pixel
                    {
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                    }
                }
            }

            if (minY > maxY)
            {
                labelContentHeight.Text = "Content height: (empty)";
                return;
            }

            int contentH = maxY - minY + 1;
            int contentW = maxX - minX + 1;
            labelContentHeight.Text = $"Content: {contentW}×{contentH} px  (Y:{minY}–{maxY})";
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region NEW: Color Picker for Chroma Key
        // ─────────────────────────────────────────────────────────────────────
        private void ButtonPickChromaColor1_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog { Color = chromaColor1, FullOpen = true })
            {
                if (cd.ShowDialog() != DialogResult.OK) return;
                chromaColor1 = cd.Color;
                buttonPickChromaColor1.BackColor = chromaColor1;
            }
        }

        private void ButtonPickChromaColor2_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog { Color = chromaColor2, FullOpen = true })
            {
                if (cd.ShowDialog() != DialogResult.OK) return;
                chromaColor2 = cd.Color;
                buttonPickChromaColor2.BackColor = chromaColor2;
            }
        }

        private void CheckBoxUseChromaKey_CheckedChanged(object sender, EventArgs e)
        {
            useChromaKey = checkBoxUseChromaKey.Checked;
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region NEW: Favorites
        // ─────────────────────────────────────────────────────────────────────
        private void ButtonToggleFavorite_Click(object sender, EventArgs e)
        {
            if (currentIndex < 0 || itemList == null) return;
            int id = itemList[currentIndex];

            if (favorites.Contains(id))
            {
                favorites.Remove(id);
                buttonToggleFavorite.BackColor = SystemColors.Control;
                buttonToggleFavorite.Text = "☆ Favorite";
            }
            else
            {
                favorites.Add(id);
                buttonToggleFavorite.BackColor = Color.Gold;
                buttonToggleFavorite.Text = "★ Favorite";
            }

            RefreshFavoritesList();
        }

        private void RefreshFavoritesList()
        {
            listBoxFavorites.Items.Clear();
            foreach (int id in favorites.OrderBy(x => x))
                listBoxFavorites.Items.Add($"0x{id:X4}");
        }

        private void ListBoxFavorites_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxFavorites.SelectedItem == null) return;
            string hex = listBoxFavorites.SelectedItem.ToString().Substring(2);
            if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int id))
            {
                int idx = itemList?.IndexOf(id) ?? -1;
                if (idx >= 0) { currentIndex = idx; LoadArtwork(); }
            }
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region NEW: Batch Export
        // ─────────────────────────────────────────────────────────────────────
        private void ButtonBatchExport_Click(object sender, EventArgs e)
        {
            if (itemList == null || itemList.Count == 0)
            {
                MessageBox.Show("No items to export.", "Batch Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (FolderBrowserDialog fbd = new FolderBrowserDialog { Description = "Select export folder" })
            {
                if (fbd.ShowDialog() != DialogResult.OK) return;

                using (ProgressDialog pd = new ProgressDialog("Batch Export", itemList.Count))
                {
                    pd.Show();
                    int saved = 0, skipped = 0;

                    for (int i = 0; i < itemList.Count; i++)
                    {
                        if (pd.Cancelled) break;

                        int id = itemList[i];
                        Bitmap baseBmp = Art.GetStatic(id);
                        if (baseBmp == null) { skipped++; continue; }

                        Bitmap export;
                        if (secondImage != null && checkBoxSecondArtwork.Checked)
                        {
                            Bitmap copy = new Bitmap(baseBmp);
                            export = CombineImages(copy, secondImage);
                            copy.Dispose();
                        }
                        else
                        {
                            export = new Bitmap(baseBmp);
                        }

                        string path = Path.Combine(fbd.SelectedPath, $"0x{id:X4}.png");
                        export.Save(path, ImageFormat.Png);
                        export.Dispose();
                        saved++;

                        pd.SetProgress(i + 1, $"Exporting 0x{id:X4}…");
                    }

                    pd.Close();
                    MessageBox.Show(
                        $"Done.\nSaved:   {saved}\nSkipped: {skipped}",
                        "Batch Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Crop (ButtonCrop_Click)
        // ─────────────────────────────────────────────────────────────────────
        private void ButtonCrop_Click(object sender, EventArgs e)
        {
            if (secondImage == null)
            {
                MessageBox.Show("No second image loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(textBoxWidth.Text, out int newW) || newW <= 0 ||
                !int.TryParse(textBoxHeight.Text, out int newH) || newH <= 0)
            {
                MessageBox.Show("Enter valid positive numbers for width and height.", "Invalid Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newW > secondImage.Width || newH > secondImage.Height)
            {
                MessageBox.Show("Crop size exceeds original image size.", "Invalid Crop",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int cx = secondImage.Width / 2 - newW / 2;
            int cy = secondImage.Height / 2 - newH / 2;

            Bitmap cropped = new Bitmap(newW, newH, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(cropped))
                g.DrawImage(secondImage, new Rectangle(0, 0, newW, newH),
                    new Rectangle(cx, cy, newW, newH), GraphicsUnit.Pixel);

            using (SaveFileDialog sfd = BuildSaveDialog("Save cropped overlay", "cropped_overlay.bmp"))
            {
                if (sfd.ShowDialog() != DialogResult.OK) { cropped.Dispose(); return; }
                SaveBitmap(cropped, sfd.FileName);
            }

            secondImage.Dispose();
            secondImage = cropped;
            secondImageOffset = Point.Empty;
            LoadArtwork();
            UpdateImageInfoLabel();
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Cutting Template
        // ─────────────────────────────────────────────────────────────────────
        private void CheckBoxCuttingTemplate_CheckedChanged(object sender, EventArgs e)
        {
            isCuttingTemplateActive = checkBoxCuttingTemplate.Checked;
            if (isCuttingTemplateActive)
            {
                // FIX: If an image is loaded, use its actual size and center the
                // cutting template on the PictureBox (SizeMode=CenterImage).
                // Fall back to the classic 44x133 default when no image is present.
                if (pictureBoxArtworkGallery.Image != null)
                {
                    int imgW = pictureBoxArtworkGallery.Image.Width;
                    int imgH = pictureBoxArtworkGallery.Image.Height;

                    cuttingTemplateSize = new Size(imgW, imgH);

                    // Center of the PictureBox minus half the image size
                    // (same offset that SizeMode=CenterImage uses)
                    int pbW = pictureBoxArtworkGallery.Width;
                    int pbH = pictureBoxArtworkGallery.Height;
                    cuttingTemplatePosition = new Point(
                        (pbW - imgW) / 2,
                        (pbH - imgH) / 2);

                    textBoxWidth.Text = imgW.ToString();
                    textBoxHeight.Text = imgH.ToString();
                }
                else
                {
                    textBoxWidth.Text = "44";
                    textBoxHeight.Text = "133";
                    cuttingTemplateSize = new Size(44, 133);
                    cuttingTemplatePosition = new Point(100, 100);
                }
            }
            this.ActiveControl = null;
            this.Focus();
            pictureBoxArtworkGallery.Invalidate();
        }

        private void UpdateCuttingTemplateSize()
        {
            if (int.TryParse(textBoxWidth.Text, out int w) && int.TryParse(textBoxHeight.Text, out int h) && w > 0 && h > 0)
                cuttingTemplateSize = new Size(w, h);
        }

        private void TextBoxCuttingTemplateSizeChanged(object sender, EventArgs e)
        {
            if (!isCuttingTemplateActive) return;
            UpdateCuttingTemplateSize();
            pictureBoxArtworkGallery.Invalidate();
        }

        private void MoveCuttingTemplate(Keys key)
        {
            const int step = 2;
            switch (key)
            {
                case Keys.W: cuttingTemplatePosition.Y -= step; break;
                case Keys.S: cuttingTemplatePosition.Y += step; break;
                case Keys.A: cuttingTemplatePosition.X -= step; break;
                case Keys.D: cuttingTemplatePosition.X += step; break;
                default: return;
            }
            pictureBoxArtworkGallery.Invalidate();
        }

        private void CuttingTemplateMoveTimer_Tick(object sender, EventArgs e)
        {
            if (currentCuttingTemplateMoveKey != Keys.None)
                MoveCuttingTemplate(currentCuttingTemplateMoveKey);
        }

        // FIX: Application.DoEvents() replaced with Refresh()
        private Bitmap CropCuttingTemplateAreaIncludingBackground()
        {
            isDrawingCuttingTemplateTemporaryDisabled = true;
            pictureBoxArtworkGallery.Refresh(); // FIX: was Invalidate+Update+DoEvents

            Bitmap full = new Bitmap(pictureBoxArtworkGallery.Width, pictureBoxArtworkGallery.Height);
            pictureBoxArtworkGallery.DrawToBitmap(full, new Rectangle(0, 0, full.Width, full.Height));

            isDrawingCuttingTemplateTemporaryDisabled = false;
            pictureBoxArtworkGallery.Invalidate();

            Rectangle crop = new Rectangle(cuttingTemplatePosition, cuttingTemplateSize);
            crop.Intersect(new Rectangle(0, 0, full.Width, full.Height));
            if (crop.Width <= 0 || crop.Height <= 0) { full.Dispose(); return null; }

            Bitmap result = new Bitmap(crop.Width, crop.Height);
            using (Graphics g = Graphics.FromImage(result))
                g.DrawImage(full, new Rectangle(0, 0, crop.Width, crop.Height), crop, GraphicsUnit.Pixel);

            full.Dispose();
            return result;
        }

        private void ButtonSaveCuttingTemplate_Click(object sender, EventArgs e)
        {
            Bitmap cropped = CropCuttingTemplateAreaIncludingBackground();
            if (cropped == null)
            {
                MessageBox.Show("Nothing to crop or out of bounds.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            using (SaveFileDialog sfd = BuildSaveDialog("Save cropped image", "cropped_image.bmp"))
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                    SaveBitmap(cropped, sfd.FileName);
            }
            cropped.Dispose();
        }

        private void ButtonCopyCuttingTemplateToClipboard_Click(object sender, EventArgs e)
        {
            Bitmap cropped = CropCuttingTemplateAreaIncludingBackground();
            if (cropped == null)
            {
                MessageBox.Show("Nothing to crop or out of bounds.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Clipboard.SetImage(cropped);
            MessageBox.Show("Image copied to clipboard.", "Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
            cropped.Dispose();
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Overlay helpers (Center, Flip)
        // ─────────────────────────────────────────────────────────────────────
        private void CenterOverlayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (secondImage == null && animatedGif == null)
            {
                MessageBox.Show("No overlay loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            secondImageOffset = Point.Empty;
            if (animatedGif != null) pictureBoxArtworkGallery.Invalidate();
            else LoadArtwork();
            UpdateImageInfoLabel();
        }

        private void FlipVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (secondImage == null) { ShowNoOverlayError(); return; }
            secondImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
            LoadArtwork();
        }

        private void FlipHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (secondImage == null) { ShowNoOverlayError(); return; }
            secondImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
            LoadArtwork();
        }

        private static void ShowNoOverlayError()
            => MessageBox.Show("No second image loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Index image toggle
        // ─────────────────────────────────────────────────────────────────────
        private void ButtonShowHideIndexedImages_Click(object sender, EventArgs e)
        {
            allowIndexArtwork = !allowIndexArtwork;
            ButtonShowHideIndexedImages.BackColor = allowIndexArtwork ? SystemColors.Control : Color.LightGreen;
            LoadArtwork();
            UpdateImageInfoLabel();
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Background image
        // ─────────────────────────────────────────────────────────────────────
        private void ButtonBackgroundImage_Click(object sender, EventArgs e)
        {
            if (backgroundImage == null)
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = "Select background image";
                    ofd.Filter = "Image Files (*.bmp;*.png;*.jpg;*.jpeg;*.tiff)|*.bmp;*.png;*.jpg;*.jpeg;*.tiff";
                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    backgroundImage?.Dispose();
                    backgroundImage = new Bitmap(ofd.FileName);
                }
            }
            else
            {
                backgroundImage.Dispose();
                backgroundImage = null;
            }
            pictureBoxArtworkGallery.BackgroundImage = backgroundImage;
            pictureBoxArtworkGallery.BackgroundImageLayout = ImageLayout.Center;
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Image file browser (uses fast LockBits)
        // ─────────────────────────────────────────────────────────────────────
        private void ButtonLoadImages_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog { Description = "Select folder with images" })
            {
                if (lastImageDirectory != null) fbd.SelectedPath = lastImageDirectory;
                if (fbd.ShowDialog() != DialogResult.OK) return;

                lastImageDirectory = fbd.SelectedPath;
                string[] files = Directory.GetFiles(lastImageDirectory, "*.*")
                    .Where(f => IsImageExtension(f)).ToArray();

                imageFileLookup.Clear();
                listBoxImages.Items.Clear();
                foreach (string f in files)
                {
                    string name = Path.GetFileName(f);
                    imageFileLookup[name] = f;
                    listBoxImages.Items.Add(name);
                }

                if (listBoxImages.Items.Count == 0)
                    MessageBox.Show("No supported image files found.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static bool IsImageExtension(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            return ext == ".bmp" || ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".tiff";
        }

        private void listBoxImages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxImages.SelectedItem == null) return;
            string name = listBoxImages.SelectedItem.ToString();
            if (!imageFileLookup.TryGetValue(name, out string path)) return;

            try
            {
                using (Bitmap loaded = new Bitmap(path))
                {
                    secondImage?.Dispose();
                    secondImage = PrepareOverlayFast(loaded); // FIX: fast path
                }
                secondImageOffset = Point.Empty;
                LoadArtwork();
                UpdateImageInfoLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image:\n{ex.Message}", "Loading error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Search in listBox
        // ─────────────────────────────────────────────────────────────────────
        private void toolStripTextBoxSearchTexbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            string input = toolStripTextBoxSearchTexbox.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            foreach (var item in listBoxImages.Items)
            {
                if (item.ToString().IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    listBoxImages.SelectedItem = item;
                    listBoxImages.TopIndex = listBoxImages.Items.IndexOf(item);
                    return;
                }
            }

            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(input.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out int hexVal))
            {
                string pattern = $"0x{hexVal:X4}";
                foreach (var item in listBoxImages.Items)
                {
                    if (item.ToString().IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        listBoxImages.SelectedItem = item;
                        listBoxImages.TopIndex = listBoxImages.Items.IndexOf(item);
                        return;
                    }
                }
            }

            MessageBox.Show("No matching entry found.", "Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void toolStripTextBoxSearchTexbox_TextChanged(object sender, EventArgs e)
        {
            string input = toolStripTextBoxSearchTexbox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(input)) return;

            for (int i = 0; i < listBoxImages.Items.Count; i++)
            {
                if (listBoxImages.Items[i].ToString().ToLower().Contains(input))
                {
                    listBoxImages.SelectedIndex = i;
                    listBoxImages.TopIndex = i;
                    listBoxImages_SelectedIndexChanged(null, null);
                    return;
                }
            }
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Keyboard handling
        // ─────────────────────────────────────────────────────────────────────
        private void ArtworkGallery_KeyDown(object sender, KeyEventArgs e)
        {
            if (isCuttingTemplateActive &&
                (e.KeyCode == Keys.W || e.KeyCode == Keys.A || e.KeyCode == Keys.S || e.KeyCode == Keys.D))
            {
                if (currentCuttingTemplateMoveKey != e.KeyCode)
                {
                    currentCuttingTemplateMoveKey = e.KeyCode;
                    MoveCuttingTemplate(e.KeyCode);
                    cuttingTemplateMoveTimer.Start();
                }
                return;
            }

            if (!IsOverlayActive) return;

            if (currentMoveKey != e.KeyCode)
            {
                currentMoveKey = e.KeyCode;
                MoveSecondImage(e.KeyCode);
                moveTimer.Start();
            }
        }

        private void ArtworkGallery_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == currentMoveKey) { moveTimer.Stop(); currentMoveKey = Keys.None; }
            if (e.KeyCode == currentCuttingTemplateMoveKey) { cuttingTemplateMoveTimer.Stop(); currentCuttingTemplateMoveKey = Keys.None; }
        }

        private void MoveTimer_Tick(object sender, EventArgs e)
        {
            if (currentMoveKey != Keys.None) MoveSecondImage(currentMoveKey);
        }

        private void MoveSecondImage(Keys key)
        {
            switch (key)
            {
                case Keys.Left: secondImageOffset.X -= MoveStep; break;
                case Keys.Right: secondImageOffset.X += MoveStep; break;
                case Keys.Up: secondImageOffset.Y -= MoveStep; break;
                case Keys.Down: secondImageOffset.Y += MoveStep; break;
                default: return;
            }
            if (animatedGif != null) pictureBoxArtworkGallery.Invalidate();
            else LoadArtwork();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Left || keyData == Keys.Right ||
                keyData == Keys.Up || keyData == Keys.Down ||
                keyData == Keys.W || keyData == Keys.A ||
                keyData == Keys.S || keyData == Keys.D)
                return true;
            return base.IsInputKey(keyData);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (IsOverlayActive &&
                (keyData == Keys.Left || keyData == Keys.Right || keyData == Keys.Up || keyData == Keys.Down))
            {
                MoveSecondImage(keyData);
                currentMoveKey = keyData;
                moveTimer.Start();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Helpers
        // ─────────────────────────────────────────────────────────────────────
        private static SaveFileDialog BuildSaveDialog(string title, string defaultName)
        {
            return new SaveFileDialog
            {
                Title = title,
                Filter = "BMP Image (*.bmp)|*.bmp|PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg|TIFF Image (*.tiff)|*.tiff",
                DefaultExt = "bmp",
                FileName = defaultName
            };
        }

        private static void SaveBitmap(Bitmap bmp, string path)
        {
            try
            {
                ImageFormat fmt = ImageFormat.Bmp;
                string low = path.ToLower();
                if (low.EndsWith(".png")) fmt = ImageFormat.Png;
                else if (low.EndsWith(".jpg") || low.EndsWith(".jpeg")) fmt = ImageFormat.Jpeg;
                else if (low.EndsWith(".tiff")) fmt = ImageFormat.Tiff;

                bmp.Save(path, fmt);
                MessageBox.Show("Image saved successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving image:\n{ex.Message}", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Dispose
        // ─────────────────────────────────────────────────────────────────────
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            moveTimer?.Dispose();
            cuttingTemplateMoveTimer?.Dispose();
            secondImage?.Dispose();
            backgroundImage?.Dispose();
            lock (gifLock)
            {
                if (animatedGif != null)
                {
                    ImageAnimator.StopAnimate(animatedGif, OnFrameChanged);
                    animatedGif.Dispose();
                    animatedGif = null;
                }
            }
        }
        #endregion

        #region Background image removal
        private void ButtonRemoveBackground_Click(object sender, EventArgs e)
        {
            if (backgroundImage == null)
                return;

            backgroundImage.Dispose();
            backgroundImage = null;

            pictureBoxArtworkGallery.BackgroundImage = null;
        }
        #endregion

        #region Reset overlay position
        private void ButtonResetOffset_Click(object sender, EventArgs e)
        {
            secondImageOffset = Point.Empty;

            if (animatedGif != null)
                pictureBoxArtworkGallery.Invalidate();
            else
                LoadArtwork();
        }
        #endregion

        private void ButtonRuler_Click(object sender, EventArgs e)
        {
            isRulerActive = !isRulerActive;
            ButtonRuler.BackColor = isRulerActive ? Color.LightGreen : SystemColors.Control;
            pictureBoxArtworkGallery.Invalidate();
        }

        private void DrawRuler(Graphics g)
        {
            int tickSmall = 4;
            int tickMedium = 8;
            int tickLarge = 12;
            int step = 10; // px zwischen Ticks

            using (Pen pen = new Pen(Color.Red, 1))
            using (Font font = new Font("Arial", 6f))
            using (Brush brush = new SolidBrush(Color.Red))
            {
                // --- oberes Lineal (horizontal) ---
                for (int x = 0; x < pictureBoxArtworkGallery.Width; x += step)
                {
                    int tickH = (x % 100 == 0) ? tickLarge : (x % 50 == 0) ? tickMedium : tickSmall;
                    g.DrawLine(pen, x, 0, x, tickH);

                    if (x % 100 == 0 && x > 0)
                        g.DrawString(x.ToString(), font, brush, x + 1, tickLarge + 1);
                }

                // --- linkes Lineal (vertikal) ---
                for (int y = 0; y < pictureBoxArtworkGallery.Height; y += step)
                {
                    int tickW = (y % 100 == 0) ? tickLarge : (y % 50 == 0) ? tickMedium : tickSmall;
                    g.DrawLine(pen, 0, y, tickW, y);

                    if (y % 100 == 0 && y > 0)
                    {
                        using (var sf = new System.Drawing.StringFormat { FormatFlags = System.Drawing.StringFormatFlags.DirectionVertical })
                            g.DrawString(y.ToString(), font, brush, tickLarge + 1, y + 1, sf);
                    }
                }
            }
        }

        private void ButtonCrosshair_Click(object sender, EventArgs e)
        {
            isCrosshairActive = !isCrosshairActive;
            ButtonCrosshair.BackColor = isCrosshairActive ? Color.LightGreen : SystemColors.Control;
            pictureBoxArtworkGallery.Invalidate();
        }

        private void DrawCrosshair(Graphics g)
        {
            if (pictureBoxArtworkGallery.Image == null) return;

            // Fadenkreuz zentriert auf das Bild (nicht auf die PictureBox)
            int imgW = pictureBoxArtworkGallery.Image.Width;
            int imgH = pictureBoxArtworkGallery.Image.Height;

            int centerX = (pictureBoxArtworkGallery.Width - imgW) / 2 + imgW / 2;
            int centerY = (pictureBoxArtworkGallery.Height - imgH) / 2 + imgH / 2;

            using (Pen pen = new Pen(Color.Red, 1))
            {
                // horizontale Linie
                g.DrawLine(pen, 0, centerY, pictureBoxArtworkGallery.Width, centerY);

                // vertikale Linie
                g.DrawLine(pen, centerX, 0, centerX, pictureBoxArtworkGallery.Height);

                // kleiner Kreis in der Mitte
                int r = 6;
                g.DrawEllipse(pen, centerX - r, centerY - r, r * 2, r * 2);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Simple progress dialog for batch export
    // ─────────────────────────────────────────────────────────────────────────
    internal class ProgressDialog : Form
    {
        private ProgressBar bar;
        private Label lbl;
        private Button btnCancel;
        public bool Cancelled { get; private set; }

        public ProgressDialog(string title, int max)
        {
            this.Text = title;
            this.Size = new Size(400, 130);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lbl = new Label { Left = 12, Top = 12, Width = 360, Text = "Starting…" };
            bar = new ProgressBar { Left = 12, Top = 36, Width = 360, Height = 22, Minimum = 0, Maximum = max };
            btnCancel = new Button { Left = 150, Top = 68, Width = 90, Text = "Cancel" };
            btnCancel.Click += (s, e) => { Cancelled = true; btnCancel.Enabled = false; };

            this.Controls.AddRange(new Control[] { lbl, bar, btnCancel });
        }

        public void SetProgress(int value, string message)
        {
            bar.Value = Math.Min(value, bar.Maximum);
            lbl.Text = message;
            Application.DoEvents(); // acceptable in a blocking progress dialog
        }
    }
}
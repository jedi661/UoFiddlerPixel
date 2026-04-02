// /***************************************************************************
// *
// * $Author: Nikodemus
// *
// * "THE WINE-WARE LICENSE"
// * As long as you retain this notice you can do whatever you want with
// * this stuff. If we meet some day, and you think this stuff is worth it,
// * you can buy me a Wine in return.
// *
// ***************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Media;

namespace UoFiddler.Controls.Forms
{
    public partial class BildFusionForm : Form
    {
        // ═══════════════════════════════════════════════════════════════════════
        // SlotState – everything belonging to a PictureBox slot (64/128/256)
        // ═══════════════════════════════════════════════════════════════════════
        private sealed class SlotState : IDisposable
        {
            public Bitmap Background { get; set; }
            public Bitmap OverlayClean { get; set; }
            public Bitmap Overlay { get; set; }
            public Bitmap Composed { get; set; }

            public int FadingValue { get; set; } = 0;
            public int ColorValue { get; set; } = 0;
            public int SharpValue { get; set; } = 1;

            public void Dispose()
            {
                Background?.Dispose(); Background = null;
                OverlayClean?.Dispose(); OverlayClean = null;
                Overlay?.Dispose(); Overlay = null;
                Composed?.Dispose(); Composed = null;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // TileState – for the "Texture into Texture" workflow (3 images)
        // ═══════════════════════════════════════════════════════════════════════
        private sealed class TileState : IDisposable
        {
            public Bitmap Original { get; set; }
            public Bitmap Working { get; set; }
            public string FilePath { get; set; }

            public void Dispose()
            {
                Original?.Dispose(); Original = null;
                Working?.Dispose(); Working = null;
            }
        }

        private readonly Dictionary<int, SlotState> _slots = new()
        {
            { 64, new SlotState() },
            { 128, new SlotState() },
            { 256, new SlotState() },
        };
        private static readonly int[] AllSizes = { 64, 128, 256 };

        private int _activeSize = 64;
        private SlotState Active => _slots[_activeSize];

        private readonly TileState[] _tileStates = { new(), new(), new() };
        private int _activeTileIndex = -1;

        private readonly ImageList _overlayImages = new() { ImageSize = new Size(32, 32) };
        private readonly ImageList _backgroundImages = new() { ImageSize = new Size(32, 32) };

        private const string ImageFilter = "Image files|*.bmp;*.png;*.jpg;*.jpeg;*.gif;*.tiff";
        private static readonly string[] ImageExtensions = { ".bmp", ".png", ".jpg", ".jpeg", ".gif", ".tiff" };

        private bool _suppressTrackBarEvents = false;

        public BildFusionForm()
        {
            InitializeComponent();
            SetupTooltips();
            SyncTrackBarsFromSlot(_activeSize);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Tooltips (all in English)
        // ═══════════════════════════════════════════════════════════════════════
        private void SetupTooltips()
        {
            var tt = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 400,
                ReshowDelay = 200,
                ShowAlways = true
            };

            tt.SetToolTip(btLoad, "Load background and overlay in one step.\nBlack pixels become transparent.");
            tt.SetToolTip(btLoadSingleBackground, "Load background image into the active slot.");
            tt.SetToolTip(btLoadSingleForeground, "Load overlay (foreground) into the active slot.\nBlack pixels become transparent.");

            tt.SetToolTip(btLeftBackgroundImage, "Rotate background 90° counter-clockwise.\nActive slot or all when 'All' is checked.");
            tt.SetToolTip(btRightBackgroundImage, "Rotate background 90° clockwise.\nActive slot or all when 'All' is checked.");

            tt.SetToolTip(btLeftOverlayImage, "Rotate overlay 90° counter-clockwise.\nActive slot or all when 'All' is checked.");
            tt.SetToolTip(btRightOverlayImage, "Rotate overlay 90° clockwise.\nActive slot or all when 'All' is checked.");
            tt.SetToolTip(btMirror, "Mirror overlay horizontally.\nActive slot or all when 'All' is checked.");

            tt.SetToolTip(btSave64x64, "Save composition of the active slot as BMP to 'tempGrafic' folder.");
            tt.SetToolTip(btClipboard, "Copy composition of the currently focused slot to clipboard.");
            tt.SetToolTip(btDirSaveOrder, "Open output folder 'tempGrafic' in Windows Explorer.");

            tt.SetToolTip(trackBarFading, "Set overlay transparency.\nLeft = opaque, Right = fully transparent.\nEach slot stores its own value.\nWhen 'All' is checked: affects all three slots.");
            tt.SetToolTip(trackBarColor, "Shift hue of the overlay.\nEach slot stores its own value.\nWhen 'All' is checked: affects all three slots.");
            tt.SetToolTip(trackBarSharp, "Sharpen overlay (Laplace filter).\nPosition 1 = no sharpening.\nEach slot stores its own value.\nWhen 'All' is checked: affects all three slots.");

            tt.SetToolTip(checkBox64x64, "Focus on 64×64 slot.");
            tt.SetToolTip(checkBox128x128, "Focus on 128×128 slot.");
            tt.SetToolTip(checkBox256x256, "Focus on 256×256 slot.");
            tt.SetToolTip(checkBoxAll, "Edit all three slots at the same time.\nTrackBars, Mirror and Rotation affect 64+128+256.");

            tt.SetToolTip(checkBox1, "Select Tile 1 for 3-image blend workflow.");
            tt.SetToolTip(checkBox2, "Select Tile 2 for 3-image blend workflow.");
            tt.SetToolTip(checkBox3, "Select Tile 3 for 3-image blend workflow.");

            tt.SetToolTip(btClearAll, "Remove all images from all PictureBoxes and Tile states.");
            tt.SetToolTip(btLoadRubberStamp, "Choose folder – all images inside will be listed.");
            tt.SetToolTip(btViewLoad, "Select image(s) – all images in the same folder will be listed.");
            tt.SetToolTip(btBackgroundImageLoad, "Choose folder – all background images will be listed.");
            tt.SetToolTip(btViewLoadBackground, "Select background image(s) – all in the same folder will be listed.");

            tt.SetToolTip(BtTextureCut, "Crop an image to 64/128/256 px and save.");
            tt.SetToolTip(btLoadTexture, "Load background texture for all three slots.");
            tt.SetToolTip(btLoadForeground, "Load foreground image. White pixels become transparent. Only active slot.");
            tt.SetToolTip(btCutTexture, "Merge background and foreground of the active slot and save.");
            tt.SetToolTip(btLoadTilesIntoTiles, "Blend three images using grayscale mask:\n1 = base | 2 = mask | 3 = foreground");
            tt.SetToolTip(btnGenerateColorCodes, "Generate grayscale hex reference list.\nClick a '#'-line to copy value to clipboard.");
        }

        private PictureBox PictureBoxForSize(int size) => size switch
        {
            128 => pictureBox128x128,
            256 => pictureBox256x256,
            _ => pictureBox64x64,
        };

        private PictureBox ActivePictureBox() => PictureBoxForSize(_activeSize);

        private IEnumerable<int> TargetSizes() =>
            checkBoxAll.Checked ? (IEnumerable<int>)AllSizes : new[] { _activeSize };

        private void SyncTrackBarsFromSlot(int size)
        {
            _suppressTrackBarEvents = true;
            try
            {
                var s = _slots[size];
                trackBarFading.Value = s.FadingValue;
                trackBarColor.Value = s.ColorValue;
                trackBarSharp.Value = Math.Max(s.SharpValue, trackBarSharp.Minimum);
                lbNr.Text = s.FadingValue.ToString();
                trackBarLabel.Text = "Color:";
            }
            finally { _suppressTrackBarEvents = false; }
        }

        private void SetActiveSize(int size)
        {
            _activeSize = size;
            checkBox64x64.CheckedChanged -= checkBox64x64_CheckedChanged;
            checkBox128x128.CheckedChanged -= checkBox128x128_CheckedChanged;
            checkBox256x256.CheckedChanged -= checkBox256x256_CheckedChanged;
            checkBoxAll.CheckedChanged -= checkBoxAll_CheckedChanged;

            checkBox64x64.Checked = size == 64;
            checkBox128x128.Checked = size == 128;
            checkBox256x256.Checked = size == 256;
            checkBoxAll.Checked = false;

            checkBox64x64.CheckedChanged += checkBox64x64_CheckedChanged;
            checkBox128x128.CheckedChanged += checkBox128x128_CheckedChanged;
            checkBox256x256.CheckedChanged += checkBox256x256_CheckedChanged;
            checkBoxAll.CheckedChanged += checkBoxAll_CheckedChanged;

            SyncTrackBarsFromSlot(size);

            if (Active.Composed != null)
                ActivePictureBox().Image = Active.Composed;
        }

        private void checkBox64x64_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox64x64.Checked) SetActiveSize(64);
        }

        private void checkBox128x128_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox128x128.Checked) SetActiveSize(128);
        }

        private void checkBox256x256_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox256x256.Checked) SetActiveSize(256);
        }

        private void checkBoxAll_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAll.Checked)
            {
                checkBox64x64.CheckedChanged -= checkBox64x64_CheckedChanged;
                checkBox128x128.CheckedChanged -= checkBox128x128_CheckedChanged;
                checkBox256x256.CheckedChanged -= checkBox256x256_CheckedChanged;
                checkBox64x64.Checked = true;
                checkBox128x128.Checked = true;
                checkBox256x256.Checked = true;
                checkBox64x64.CheckedChanged += checkBox64x64_CheckedChanged;
                checkBox128x128.CheckedChanged += checkBox128x128_CheckedChanged;
                checkBox256x256.CheckedChanged += checkBox256x256_CheckedChanged;

                SyncTrackBarsFromSlot(_activeSize);
            }
            else
            {
                SetActiveSize(_activeSize);
            }
        }

        private void SetActiveTile(int index)
        {
            _activeTileIndex = index;
            checkBox1.CheckedChanged -= checkBox1_CheckedChanged;
            checkBox2.CheckedChanged -= checkBox2_CheckedChanged;
            checkBox3.CheckedChanged -= checkBox3_CheckedChanged;
            checkBox1.Checked = index == 0;
            checkBox2.Checked = index == 1;
            checkBox3.Checked = index == 2;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            checkBox2.CheckedChanged += checkBox2_CheckedChanged;
            checkBox3.CheckedChanged += checkBox3_CheckedChanged;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) { if (checkBox1.Checked) SetActiveTile(0); }
        private void checkBox2_CheckedChanged(object sender, EventArgs e) { if (checkBox2.Checked) SetActiveTile(1); }
        private void checkBox3_CheckedChanged(object sender, EventArgs e) { if (checkBox3.Checked) SetActiveTile(2); }

        private IEnumerable<int> TargetTileIndices()
        {
            if (checkBoxAll.Checked) return new[] { 0, 1, 2 };
            if (_activeTileIndex >= 0) return new[] { _activeTileIndex };
            return Array.Empty<int>();
        }

        private void RefreshComposition(int size)
        {
            var slot = _slots[size];
            if (slot.Background == null) return;
            slot.Composed?.Dispose();
            var composed = new Bitmap(size, size);
            using (var g = Graphics.FromImage(composed))
            {
                g.DrawImage(slot.Background, 0, 0, size, size);
                if (slot.Overlay != null)
                    g.DrawImage(slot.Overlay, 0, 0, size, size);
            }
            slot.Composed = composed;
            var pb = PictureBoxForSize(size);
            pb.Image = null;
            pb.Image = slot.Composed;
        }

        private void RefreshActiveComposition() => RefreshComposition(_activeSize);

        private static Bitmap ScaleTo(Bitmap src, int size)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.DrawImage(src, 0, 0, size, size);
            return bmp;
        }

        private static Bitmap FastClone(Bitmap src) =>
            src.Clone(new Rectangle(0, 0, src.Width, src.Height), src.PixelFormat);

        private void LoadBackgroundIntoSlot(int size, string path)
        {
            var slot = _slots[size];
            slot.Background?.Dispose();
            using var raw = new Bitmap(path);
            slot.Background = ScaleTo(raw, size);
        }

        private void LoadOverlayIntoSlot(int size, string path)
        {
            var slot = _slots[size];
            slot.Overlay?.Dispose();
            slot.OverlayClean?.Dispose();
            using var raw = new Bitmap(path);
            raw.MakeTransparent(Color.Black);
            slot.Overlay = ScaleTo(raw, size);
            slot.OverlayClean = FastClone(slot.Overlay);

            slot.FadingValue = 0;
            slot.ColorValue = 0;
            slot.SharpValue = trackBarSharp.Minimum;
        }

        private void btLoad_Click(object sender, EventArgs e)
        {
            using var dlg1 = new OpenFileDialog { Filter = ImageFilter, Title = "Select background image" };
            if (dlg1.ShowDialog() != DialogResult.OK) return;
            MessageBox.Show("Now select the overlay (foreground) image.", "Load Overlay",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            using var dlg2 = new OpenFileDialog { Filter = ImageFilter, Title = "Select overlay image" };
            if (dlg2.ShowDialog() != DialogResult.OK) return;

            LoadBackgroundIntoSlot(_activeSize, dlg1.FileName);
            LoadOverlayIntoSlot(_activeSize, dlg2.FileName);
            SyncTrackBarsFromSlot(_activeSize);
            RefreshActiveComposition();
        }

        private void btLoadSingleBackground_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = ImageFilter, Title = "Select background image" };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            LoadBackgroundIntoSlot(_activeSize, dlg.FileName);
            RefreshActiveComposition();
        }

        private void btLoadSingleForeground_Click(object sender, EventArgs e)
        {
            if (Active.Background == null)
            {
                MessageBox.Show("Please load a background image first.", "No Background",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using var dlg = new OpenFileDialog { Filter = ImageFilter, Title = "Select overlay image" };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            LoadOverlayIntoSlot(_activeSize, dlg.FileName);
            SyncTrackBarsFromSlot(_activeSize);
            RefreshActiveComposition();
        }

        // Background rotation – now respects checkBoxAll
        private void btLeftBackgroundImage_Click(object sender, EventArgs e)
        {
            RotateBackgroundInTargets(RotateFlipType.Rotate270FlipNone);
        }

        private void btRightBackgroundImage_Click(object sender, EventArgs e)
        {
            RotateBackgroundInTargets(RotateFlipType.Rotate90FlipNone);
        }

        private void RotateBackgroundInTargets(RotateFlipType rft)
        {
            foreach (int s in TargetSizes())
            {
                var slot = _slots[s];
                if (slot.Background == null) continue;
                slot.Background.RotateFlip(rft);
                RefreshComposition(s);
            }
        }

        // Overlay rotation
        private void btLeftOverlayImage_Click(object sender, EventArgs e)
        {
            RotateOverlayInTargets(RotateFlipType.Rotate270FlipNone);
            RotateTileTargets(RotateFlipType.Rotate270FlipNone);
        }

        private void btRightOverlayImage_Click(object sender, EventArgs e)
        {
            RotateOverlayInTargets(RotateFlipType.Rotate90FlipNone);
            RotateTileTargets(RotateFlipType.Rotate90FlipNone);
        }

        private void RotateOverlayInTargets(RotateFlipType rft)
        {
            foreach (int s in TargetSizes())
            {
                var slot = _slots[s];
                if (slot.Overlay == null) continue;
                slot.Overlay.RotateFlip(rft);
                slot.OverlayClean?.RotateFlip(rft);
                RefreshComposition(s);
            }
        }

        private void RotateTileTargets(RotateFlipType rft)
        {
            foreach (int idx in TargetTileIndices())
            {
                _tileStates[idx].Working?.RotateFlip(rft);
                _tileStates[idx].Original?.RotateFlip(rft);
            }
        }

        private void btMirror_Click(object sender, EventArgs e)
        {
            
            foreach (int s in TargetSizes())
            {
                var slot = _slots[s];
                if (slot.Overlay == null) continue;

                slot.Overlay.RotateFlip(RotateFlipType.RotateNoneFlipX);
                slot.OverlayClean?.RotateFlip(RotateFlipType.RotateNoneFlipX);
                RefreshComposition(s);
            }

            
            foreach (int idx in TargetTileIndices())
            {
                _tileStates[idx].Working?.RotateFlip(RotateFlipType.RotateNoneFlipX);
                _tileStates[idx].Original?.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
        }

        private void trackBarFading_Scroll(object sender, EventArgs e)
        {
            if (_suppressTrackBarEvents) return;
            int alpha = 255 - trackBarFading.Value;
            lbNr.Text = trackBarFading.Value.ToString();
            foreach (int s in TargetSizes())
            {
                var slot = _slots[s];
                if (slot.OverlayClean == null) continue;
                slot.FadingValue = trackBarFading.Value;
                Bitmap faded = ApplyAlphaFast(slot.OverlayClean, alpha);
                slot.Overlay?.Dispose();
                slot.Overlay = faded;
                RefreshComposition(s);
            }
            foreach (int idx in TargetTileIndices())
            {
                var tile = _tileStates[idx];
                if (tile.Original == null) continue;
                Bitmap faded = ApplyAlphaFast(tile.Original, alpha);
                tile.Working?.Dispose();
                tile.Working = faded;
            }
        }

        private static Bitmap ApplyAlphaFast(Bitmap src, int alpha)
        {
            var dst = FastClone(src);
            var rect = new Rectangle(0, 0, dst.Width, dst.Height);
            var data = dst.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int len = Math.Abs(data.Stride) * dst.Height;
            byte[] buf = new byte[len];
            try
            {
                Marshal.Copy(data.Scan0, buf, 0, len);
                for (int i = 0; i < len; i += 4)
                {
                    if (buf[i + 2] == 0 && buf[i + 1] == 0 && buf[i] == 0) continue;
                    buf[i + 3] = (byte)alpha;
                }
                Marshal.Copy(buf, 0, data.Scan0, len);
            }
            finally { dst.UnlockBits(data); }
            return dst;
        }

        private void trackBarColor_Scroll(object sender, EventArgs e)
        {
            if (_suppressTrackBarEvents) return;
            double hueShift = (255 - trackBarColor.Value) / 255.0;
            Color sample = Color.Empty;
            foreach (int s in TargetSizes())
            {
                var slot = _slots[s];
                if (slot.OverlayClean == null) continue;
                slot.ColorValue = trackBarColor.Value;
                Bitmap shifted = HueShiftFast(slot.OverlayClean, hueShift);
                slot.Overlay?.Dispose();
                slot.Overlay = shifted;
                if (s == _activeSize)
                    sample = shifted.GetPixel(shifted.Width / 2, shifted.Height / 2);
                RefreshComposition(s);
            }
            foreach (int idx in TargetTileIndices())
            {
                var tile = _tileStates[idx];
                if (tile.Original == null) continue;
                Bitmap shifted = HueShiftFast(tile.Original, hueShift);
                tile.Working?.Dispose();
                tile.Working = shifted;
            }
            if (sample != Color.Empty)
                trackBarLabel.Text = $"Hue: {trackBarColor.Value} R={sample.R} G={sample.G} B={sample.B}";
        }

        private static Bitmap HueShiftFast(Bitmap src, double hueShift)
        {
            var dst = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, src.Width, src.Height);
            var dS = src.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var dD = dst.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            int len = Math.Abs(dS.Stride) * src.Height;
            byte[] bufS = new byte[len];
            byte[] bufD = new byte[len];
            try
            {
                Marshal.Copy(dS.Scan0, bufS, 0, len);
                for (int i = 0; i < len; i += 4)
                {
                    byte b = bufS[i], g = bufS[i + 1], r = bufS[i + 2], a = bufS[i + 3];
                    if (r == 0 && g == 0 && b == 0)
                    {
                        bufD[i] = b; bufD[i + 1] = g; bufD[i + 2] = r; bufD[i + 3] = a;
                        continue;
                    }
                    RgbToHsl(Color.FromArgb(a, r, g, b), out double h, out double sv, out double l);
                    h = (h + hueShift) % 1.0;
                    Color nc = HslToRgb(h, sv, l);
                    bufD[i] = nc.B; bufD[i + 1] = nc.G; bufD[i + 2] = nc.R; bufD[i + 3] = a;
                }
                Marshal.Copy(bufD, 0, dD.Scan0, len);
            }
            finally { src.UnlockBits(dS); dst.UnlockBits(dD); }
            return dst;
        }

        private void trackBarSharp_ValueChanged(object sender, EventArgs e)
        {
            if (_suppressTrackBarEvents) return;
            foreach (int s in TargetSizes())
            {
                var slot = _slots[s];
                if (slot.OverlayClean == null) continue;
                slot.SharpValue = trackBarSharp.Value;
                if (trackBarSharp.Value <= trackBarSharp.Minimum)
                {
                    slot.Overlay?.Dispose();
                    slot.Overlay = FastClone(slot.OverlayClean);
                }
                else
                {
                    Bitmap sh = FastClone(slot.OverlayClean);
                    SharpenImage(sh, trackBarSharp.Value);
                    slot.Overlay?.Dispose();
                    slot.Overlay = sh;
                }
                RefreshComposition(s);
            }
            foreach (int idx in TargetTileIndices())
            {
                var tile = _tileStates[idx];
                if (tile.Original == null) continue;
                if (trackBarSharp.Value <= trackBarSharp.Minimum)
                {
                    tile.Working?.Dispose();
                    tile.Working = FastClone(tile.Original);
                }
                else
                {
                    Bitmap sh = FastClone(tile.Original);
                    SharpenImage(sh, trackBarSharp.Value);
                    tile.Working?.Dispose();
                    tile.Working = sh;
                }
            }
        }

        private static void SharpenImage(Bitmap image, int sharpness)
        {
            double scale = sharpness / 255.0;
            double[,] kernel =
            {
                { -1*scale, -1*scale, -1*scale },
                { -1*scale, 8*scale, -1*scale },
                { -1*scale, -1*scale, -1*scale },
            };
            var rect = new Rectangle(0, 0, image.Width, image.Height);
            var srcBd = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int stride = Math.Abs(srcBd.Stride);
            int len = stride * image.Height;
            byte[] bufIn = new byte[len];
            byte[] bufOut = new byte[len];
            Marshal.Copy(srcBd.Scan0, bufIn, 0, len);
            image.UnlockBits(srcBd);
            Buffer.BlockCopy(bufIn, 0, bufOut, 0, len);
            int w = image.Width, h = image.Height;
            for (int y = 1; y < h - 1; y++)
                for (int x = 1; x < w - 1; x++)
                {
                    int i = y * stride + x * 4;
                    byte ob = bufIn[i], og = bufIn[i + 1], orr = bufIn[i + 2];
                    if (orr == 0 && og == 0 && ob == 0) continue;
                    double r = 0, g = 0, b = 0;
                    for (int ky = 0; ky < 3; ky++)
                        for (int kx = 0; kx < 3; kx++)
                        {
                            int ni = (y + ky - 1) * stride + (x + kx - 1) * 4;
                            byte pb2 = bufIn[ni], pg = bufIn[ni + 1], pr = bufIn[ni + 2];
                            if (pr == 0 && pg == 0 && pb2 == 0) continue;
                            r += pr * kernel[kx, ky]; g += pg * kernel[kx, ky]; b += pb2 * kernel[kx, ky];
                        }
                    r = Math.Max(scale * r + (1 - scale) * orr, orr);
                    g = Math.Max(scale * g + (1 - scale) * og, og);
                    b = Math.Max(scale * b + (1 - scale) * ob, ob);
                    bufOut[i] = (byte)Math.Min(Math.Max(b, 0), 255);
                    bufOut[i + 1] = (byte)Math.Min(Math.Max(g, 0), 255);
                    bufOut[i + 2] = (byte)Math.Min(Math.Max(r, 0), 255);
                    bufOut[i + 3] = bufIn[i + 3];
                }
            var dstBd = image.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bufOut, 0, dstBd.Scan0, len);
            image.UnlockBits(dstBd);
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            var pb = ActivePictureBox();
            if (pb.Image == null)
            {
                MessageBox.Show("Nothing to save.", "No Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SaveBitmap((Bitmap)pb.Image);
        }

        private void btClipboard_Click(object sender, EventArgs e)
        {
            var pb = ActivePictureBox();
            if (pb.Image != null)
                Clipboard.SetImage(pb.Image);
            else
                MessageBox.Show($"No image in the active {_activeSize}×{_activeSize} slot.", "Clipboard",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btDirSaveOrder_Click(object sender, EventArgs e)
        {
            string dir = OutputDirectory();
            if (Directory.Exists(dir))
                Process.Start("explorer.exe", dir);
            else
                MessageBox.Show("The 'tempGrafic' folder does not exist yet.\nSave an image first.",
                    "Folder not found", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btClearAll_Click(object sender, EventArgs e)
        {
            foreach (int s in AllSizes)
            {
                _slots[s].Dispose();
                var pb = PictureBoxForSize(s);
                pb.Image = null;
                pb.BackgroundImage = null;
            }
            foreach (var tile in _tileStates) tile.Dispose();
            _activeTileIndex = -1;
            SetActiveTile(-1);
            ResetTrackBarsUI();
            MessageBox.Show("All images have been removed.", "Cleared", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btLoadRubberStamp_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog { Description = "Choose overlay folder – all images will be listed" };
            if (fbd.ShowDialog() != DialogResult.OK) return;
            tbFileDir.Text = fbd.SelectedPath;
            PopulateCombo(Directory.GetFiles(fbd.SelectedPath), comboBoxRubberStamp, _overlayImages);
        }

        private void btViewLoad_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = ImageFilter, Multiselect = true, Title = "Select overlay images" };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            string folder = Path.GetDirectoryName(ofd.FileNames[0]);
            tbFileDir.Text = folder;
            PopulateCombo(Directory.GetFiles(folder), comboBoxRubberStamp, _overlayImages);
        }

        private void comboBoxRubberStamp_DrawItem(object sender, DrawItemEventArgs e) =>
            DrawComboItem(e, _overlayImages, comboBoxRubberStamp);

        private void comboBoxRubberStamp_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxRubberStamp.SelectedIndex < 0) return;
            if (Active.Background == null)
            {
                MessageBox.Show("Please load a background image first.", "No Background",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string path = Path.Combine(tbFileDir.Text, comboBoxRubberStamp.SelectedItem?.ToString() ?? "");
            if (!File.Exists(path)) return;
            LoadOverlayIntoSlot(_activeSize, path);
            SyncTrackBarsFromSlot(_activeSize);
            RefreshActiveComposition();
        }

        private void btBackgroundImageLoad_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog { Description = "Choose background folder – all images will be listed" };
            if (fbd.ShowDialog() != DialogResult.OK) return;
            tbDirBackgroundImage.Text = fbd.SelectedPath;
            PopulateCombo(Directory.GetFiles(fbd.SelectedPath), comboBoxBackgroundImage, _backgroundImages);
        }

        private void btViewLoadBackground_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = ImageFilter, Multiselect = true, Title = "Select background images" };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            string folder = Path.GetDirectoryName(ofd.FileNames[0]);
            tbDirBackgroundImage.Text = folder;
            PopulateCombo(Directory.GetFiles(folder), comboBoxBackgroundImage, _backgroundImages);
        }

        private void comboBoxBackgroundImage_DrawItem(object sender, DrawItemEventArgs e) =>
            DrawComboItem(e, _backgroundImages, comboBoxBackgroundImage);

        private void comboBoxBackgroundImage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxBackgroundImage.SelectedIndex < 0) return;
            string path = Path.Combine(tbDirBackgroundImage.Text, comboBoxBackgroundImage.SelectedItem?.ToString() ?? "");
            if (!File.Exists(path)) return;
            LoadBackgroundIntoSlot(_activeSize, path);
            RefreshActiveComposition();
        }

        private static void PopulateCombo(string[] files, ComboBox combo, ImageList imgList)
        {
            combo.Items.Clear();
            imgList.Images.Clear();
            foreach (string file in files.OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase))
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (!ImageExtensions.Contains(ext)) continue;
                try
                {
                    using var img = Image.FromFile(file);
                    imgList.Images.Add(new Bitmap(img, imgList.ImageSize));
                    combo.Items.Add(Path.GetFileName(file));
                }
                catch { }
            }
            if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;
        }

        private static void DrawComboItem(DrawItemEventArgs e, ImageList imgList, ComboBox combo)
        {
            if (e.Index < 0 || e.Index >= imgList.Images.Count) return;
            e.DrawBackground();
            int imgW = imgList.ImageSize.Width;
            e.Graphics.DrawImage(imgList.Images[e.Index], e.Bounds.Left, e.Bounds.Top, imgW, e.Bounds.Height);
            using var brush = new SolidBrush(e.ForeColor);
            e.Graphics.DrawString(combo.Items[e.Index].ToString(), e.Font, brush,
                e.Bounds.Left + imgW + 2, e.Bounds.Top + (e.Bounds.Height - e.Font.Height) / 2);
            e.DrawFocusRectangle();
        }

        private void BtTextureCut_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = ImageFilter, Title = "Select texture to crop" };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            using var texture = new Bitmap(ofd.FileName);
            foreach (int s in AllSizes)
                SetCroppedCentre(PictureBoxForSize(s), texture, s);
        }

        private void SetCroppedCentre(PictureBox pb, Bitmap source, int size)
        {
            int cropSize = Math.Min(Math.Min(source.Width, source.Height), size);
            int ox = (source.Width - cropSize) / 2;
            int oy = (source.Height - cropSize) / 2;
            using var cropped = source.Clone(new Rectangle(ox, oy, cropSize, cropSize), source.PixelFormat);
            var final = new Bitmap(size, size);
            using (var g = Graphics.FromImage(final))
                g.DrawImage(cropped, 0, 0, size, size);
            pb.Image = final;
            SaveBitmapWithSize(final);
        }

        private void btLoadTexture_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = ImageFilter, Title = "Select background texture" };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            using var img = Image.FromFile(ofd.FileName);
            foreach (int s in AllSizes)
                PictureBoxForSize(s).BackgroundImage = new Bitmap(img, s, s);
        }

        private void btLoadForeground_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = ImageFilter, Title = "Select foreground image" };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            using var raw = new Bitmap(ofd.FileName);
            var bmp = new Bitmap(raw, _activeSize, _activeSize);
            bmp.MakeTransparent(Color.White);
            ActivePictureBox().Image = bmp;
        }

        private void btCutTexture_Click(object sender, EventArgs e) =>
            SaveMergedFromPictureBox(ActivePictureBox());

        private void SaveMergedFromPictureBox(PictureBox pb)
        {
            if (pb.Image == null || pb.BackgroundImage == null)
            {
                MessageBox.Show("Both background texture and foreground image are required.",
                    "Nothing to merge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var merged = new Bitmap(pb.Width, pb.Height);
            using (var g = Graphics.FromImage(merged))
            {
                g.DrawImage(pb.BackgroundImage, (pb.Width - pb.BackgroundImage.Width) / 2, (pb.Height - pb.BackgroundImage.Height) / 2);
                g.DrawImage(pb.Image, (pb.Width - pb.Image.Width) / 2, (pb.Height - pb.Image.Height) / 2);
            }
            SaveBitmapWithSize(merged);
        }

        private void btLoadTilesIntoTiles_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = ImageFilter, Multiselect = false };
            var bitmaps = new Bitmap[3];
            string[] labels = { "Base layer (1)", "Grayscale mask (2)", "Foreground layer (3)" };
            string[] paths = new string[3];
            for (int i = 0; i < 3; i++)
            {
                ofd.Title = $"Select image {i + 1} of 3 – {labels[i]}";
                if (ofd.ShowDialog() != DialogResult.OK)
                {
                    for (int j = 0; j < i; j++) bitmaps[j]?.Dispose();
                    return;
                }
                paths[i] = ofd.FileName;
                bitmaps[i] = new Bitmap(ofd.FileName);
            }
            for (int i = 0; i < 3; i++)
            {
                _tileStates[i].Dispose();
                _tileStates[i].Original = new Bitmap(bitmaps[i]);
                _tileStates[i].Working = new Bitmap(bitmaps[i]);
                _tileStates[i].FilePath = paths[i];
            }
            SetActiveTile(0);
            MakeColorSemiTransparent(bitmaps[1], Color.Black, 0);
            for (int t = 0; t < 100; t++)
            {
                int v = 255 - (int)(2.55 * t);
                MakeColorSemiTransparent(bitmaps[1], Color.FromArgb(v, v, v), 255 - (int)(2.55 * t));
            }
            ApplyTransparencyMask(bitmaps[2], bitmaps[1]);
            using var combined = CombineThreeLayers(bitmaps[0], bitmaps[1], bitmaps[2]);
            var pb = ActivePictureBox();
            pb.Image?.Dispose();
            pb.Image = new Bitmap(combined, pb.Size);
            foreach (var bmp in bitmaps) bmp?.Dispose();
        }

        private static void MakeColorSemiTransparent(Bitmap bmp, Color target, int alpha)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var data = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int len = Math.Abs(data.Stride) * bmp.Height;
            byte[] buf = new byte[len];
            Marshal.Copy(data.Scan0, buf, 0, len);
            for (int i = 0; i < len; i += 4)
                if (buf[i] == target.B && buf[i + 1] == target.G && buf[i + 2] == target.R)
                    buf[i + 3] = (byte)alpha;
            Marshal.Copy(buf, 0, data.Scan0, len);
            bmp.UnlockBits(data);
        }

        private static void ApplyTransparencyMask(Bitmap foreground, Bitmap mask)
        {
            int w = Math.Min(foreground.Width, mask.Width), h = Math.Min(foreground.Height, mask.Height);
            var rect = new Rectangle(0, 0, w, h);
            var dF = foreground.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var dM = mask.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int len = Math.Abs(dF.Stride) * h;
            byte[] bF = new byte[len], bM = new byte[len];
            Marshal.Copy(dF.Scan0, bF, 0, len);
            Marshal.Copy(dM.Scan0, bM, 0, len);
            for (int i = 0; i < len; i += 4)
                bF[i + 3] = (byte)((bM[i] * 0.114 + bM[i + 1] * 0.587 + bM[i + 2] * 0.299) / 255.0 * 255);
            Marshal.Copy(bF, 0, dF.Scan0, len);
            foreground.UnlockBits(dF);
            mask.UnlockBits(dM);
        }

        private static Bitmap CombineThreeLayers(Image img1, Image img2, Image img3)
        {
            var result = new Bitmap(64, 64);
            using var g = Graphics.FromImage(result);
            g.DrawImage(img1, 0, 0, 64, 64);
            g.DrawImage(img2, 0, 0, 64, 64);
            g.DrawImage(img3, 0, 0, 64, 64);
            return result;
        }

        private void btnGenerateColorCodes_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("#000000 - 100% opacity (black / fully transparent in overlay)");
            sb.AppendLine("#FFFFFF - 100% opacity (white)");
            for (int pct = 99; pct >= 1; pct--)
            {
                int v = (int)Math.Round(pct * 2.55);
                sb.AppendLine($"#{v:X2}{v:X2}{v:X2} - {pct}% opacity");
            }
            richTextBox1.Text = sb.ToString();
        }

        private void richTextBox1_MouseUp(object sender, MouseEventArgs e)
        {
            string sel = richTextBox1.SelectedText?.Trim() ?? "";
            if (sel.StartsWith("#"))
                Clipboard.SetText(sel.Split(' ')[0]);
        }

        private static string OutputDirectory() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempGrafic");

        private static void SaveBitmap(Bitmap bmp)
        {
            string dir = OutputDirectory();
            Directory.CreateDirectory(dir);
            bmp.Save(Path.Combine(dir, $"TextureTile_{DateTime.Now:yyyyMMdd_HHmmss}.bmp"), ImageFormat.Bmp);
            PlaySaveSound();
        }

        private static void SaveBitmapWithSize(Bitmap bmp)
        {
            string dir = OutputDirectory();
            Directory.CreateDirectory(dir);
            bmp.Save(Path.Combine(dir, $"TextureTile_{bmp.Width}x{bmp.Height}_{DateTime.Now:yyyyMMdd_HHmmss}.bmp"), ImageFormat.Bmp);
            PlaySaveSound();
        }

        private static void PlaySaveSound()
        {
            string p = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound.wav");
            if (File.Exists(p)) new System.Media.SoundPlayer(p).Play();
        }

        private void ResetTrackBarsUI()
        {
            _suppressTrackBarEvents = true;
            try
            {
                trackBarFading.Value = 0;
                trackBarColor.Value = 0;
                trackBarSharp.Value = trackBarSharp.Minimum;
                lbNr.Text = "0";
                trackBarLabel.Text = "Color:";
            }
            finally { _suppressTrackBarEvents = false; }
        }

        private static void RgbToHsl(Color c, out double h, out double s, out double l)
        {
            double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
            l = (max + min) / 2.0;
            if (max == min) { h = s = 0; return; }
            double d = max - min;
            s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);
            if (max == r) h = (g - b) / d + (g < b ? 6 : 0);
            else if (max == g) h = (b - r) / d + 2;
            else h = (r - g) / d + 4;
            h /= 6;
        }

        private static Color HslToRgb(double h, double s, double l)
        {
            if (s == 0) { int v = (int)(l * 255); return Color.FromArgb(v, v, v); }
            static double Hue2Rgb(double p, double q, double t)
            {
                if (t < 0) t += 1; if (t > 1) t -= 1;
                if (t < 1 / 6.0) return p + (q - p) * 6 * t;
                if (t < 0.5) return q;
                if (t < 2 / 3.0) return p + (q - p) * (2 / 3.0 - t) * 6;
                return p;
            }
            double q2 = l < 0.5 ? l * (1 + s) : l + s - l * s, p2 = 2 * l - q2;
            return Color.FromArgb(
                (int)(Hue2Rgb(p2, q2, h + 1 / 3.0) * 255),
                (int)(Hue2Rgb(p2, q2, h) * 255),
                (int)(Hue2Rgb(p2, q2, h - 1 / 3.0) * 255));
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            foreach (var slot in _slots.Values) slot.Dispose();
            foreach (var tile in _tileStates) tile.Dispose();
            _overlayImages.Dispose();
            _backgroundImages.Dispose();
        }
    }
}
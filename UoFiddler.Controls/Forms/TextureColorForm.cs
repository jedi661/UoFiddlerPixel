// /***************************************************************************
//  *
//  * $Author: Nikodemus (original), refactored & extended
//  *
//  * "THE WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Ultima;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// Form for viewing, painting, and color-shifting Ultima Online textures.
    /// Supports freehand selection, shape generation (circles, squares, triangles,
    /// diamonds, stars, hexagons), hue shifting, color exchange, brightness/contrast/
    /// saturation adjustments, pattern persistence, and clipboard/file I/O.
    /// </summary>
    public partial class TextureColorForm : Form
    {
        #region Fields

        private int _currentTextureId = 0;
        private Bitmap _originalImage;
        private Bitmap _undoImage;                      // Single-level undo buffer
        private int _savedTrackBarPosition = 0;

        // Freehand selection path
        private List<Point> _points = new List<Point>();
        private bool _isSelecting = false;
        private Point _imagePosition;
        private List<Point> _savedPattern = null;

        // Shape generation
        private readonly Random _random = new Random();
        private List<List<Point>> _shapes = new List<List<Point>>();

        // Color picker
        private bool _isPickingColor = false;

        // Custom drawing cursor
        private Cursor _customCursor;
        private Color _cursorColor = Color.LimeGreen;
        private Point _cursorPixelPosition;

        #endregion

        #region Constructor

        public TextureColorForm()
        {
            InitializeComponent();

            this.KeyPreview = true;
            _customCursor = CreateCustomCursor(_cursorColor);
        }

        #endregion

        #region Cursor Helpers

        /// <summary>Creates a small solid-color cursor bitmap.</summary>
        private Cursor CreateCustomCursor(Color color)
        {
            Bitmap bmp = new Bitmap(8, 8);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.FillEllipse(new SolidBrush(color), 1, 1, 6, 6);
            }
            return new Cursor(bmp.GetHicon());
        }

        #endregion

        #region Public Interface

        /// <summary>Sets the texture ID to display.</summary>
        public void SetCurrentTextureId(int textureId)
        {
            _currentTextureId = textureId;
        }

        /// <summary>Loads and displays the current texture from the UO data files.</summary>
        public void UpdateImage()
        {
            if (!Textures.TestTexture(_currentTextureId))
                return;

            Image textureImage = Textures.GetTexture(_currentTextureId);
            PictureBoxImageColor.Image = textureImage;
            _originalImage = new Bitmap(textureImage);
            SaveUndo();

            int imageX = (PictureBoxImageColor.Width - textureImage.Width) / 2;
            int imageY = (PictureBoxImageColor.Height - textureImage.Height) / 2;
            _imagePosition = new Point(imageX, imageY);

            string hexId = "0x" + _currentTextureId.ToString("X4");
            lbIDNumber.Text =
                $"ID: {_currentTextureId}  (Hex: {hexId})  |  Size: {textureImage.Width}×{textureImage.Height}";

            ResetAdjustmentTrackBars();
        }

        #endregion

        #region Navigation Buttons

        private void ButtonPrevious_Click(object sender, EventArgs e)
        {
            do
            {
                if (_currentTextureId > 0)
                    _currentTextureId--;
            }
            while (!Textures.TestTexture(_currentTextureId) && _currentTextureId > 0);

            UpdateImage();
        }

        private void ButtonNext_Click(object sender, EventArgs e)
        {
            do
            {
                _currentTextureId++;
            }
            while (!Textures.TestTexture(_currentTextureId) &&
                   _currentTextureId < Textures.GetIdxLength());

            UpdateImage();
        }

        #endregion

        #region Color Conversion – HSL ↔ RGB

        /// <summary>Converts an RGB color to HSL (H: 0-360, S: 0-100, L: 0-100).</summary>
        public static void ColorToHSL(Color color, out double h, out double s, out double l)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            l = (max + min) / 2.0;

            if (Math.Abs(max - min) < 1e-10)
            {
                h = s = 0;
            }
            else
            {
                double d = max - min;
                s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

                if (Math.Abs(max - r) < 1e-10)
                    h = (g - b) / d + (g < b ? 6 : 0);
                else if (Math.Abs(max - g) < 1e-10)
                    h = (b - r) / d + 2;
                else
                    h = (r - g) / d + 4;

                h /= 6;
            }

            h = h * 360;
            s = s * 100;
            l = l * 100;
        }

        /// <summary>Converts HSL (H: 0-360, S: 0-100, L: 0-100) to an RGB Color.</summary>
        public static Color HSLToColor(double h, double s, double l)
        {
            h /= 360; s /= 100; l /= 100;

            double r, g, b;

            if (Math.Abs(s) < 1e-10)
            {
                r = g = b = l;
            }
            else
            {
                double Hue2Rgb(double p, double q, double t)
                {
                    if (t < 0) t += 1;
                    if (t > 1) t -= 1;
                    if (t < 1 / 6.0) return p + (q - p) * 6 * t;
                    if (t < 1 / 2.0) return q;
                    if (t < 2 / 3.0) return p + (q - p) * (2 / 3.0 - t) * 6;
                    return p;
                }

                double q2 = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p2 = 2 * l - q2;
                r = Hue2Rgb(p2, q2, h + 1 / 3.0);
                g = Hue2Rgb(p2, q2, h);
                b = Hue2Rgb(p2, q2, h - 1 / 3.0);
            }

            return Color.FromArgb(
                Clamp255(r * 255),
                Clamp255(g * 255),
                Clamp255(b * 255));
        }

        private static int Clamp255(double value) =>
            (int)Math.Max(0, Math.Min(255, value));

        #endregion

        #region Undo

        /// <summary>Saves the current PictureBox image as the undo buffer.</summary>
        private void SaveUndo()
        {
            if (PictureBoxImageColor.Image != null)
                _undoImage = new Bitmap(PictureBoxImageColor.Image);
        }

        private void BtnUndo_Click(object sender, EventArgs e)
        {
            if (_undoImage != null)
            {
                PictureBoxImageColor.Image = new Bitmap(_undoImage);
                UpdateColorStatus(Color.Empty);
            }
        }

        #endregion

        #region Reset Adjustment TrackBars

        private void ResetAdjustmentTrackBars()
        {
            if (trackBarSaturation != null) trackBarSaturation.Value = 0;
            if (trackBarBrightness != null) trackBarBrightness.Value = 0;
            if (trackBarContrast != null) trackBarContrast.Value = 0;
            TrackBarColor.Value = 0;

            if (labelSaturation != null) labelSaturation.Text = "Saturation: 0";
            if (labelBrightness != null) labelBrightness.Text = "Brightness: 0";
            if (labelContrast != null) labelContrast.Text = "Contrast:   0";
            labelColorShift.Text = "Hue Shift: 0°";
        }

        #endregion

        #region Hue Shift – TrackBar Events

        private void TrackBarColor_Scroll(object sender, EventArgs e)
        {
            labelColorShift.Text = $"Hue Shift: {TrackBarColor.Value}°";
        }

        private void TrackBarColor_MouseUp(object sender, MouseEventArgs e)
        {
            ApplyHueShift();
        }

        private void TrackBarColor_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
                ApplyHueShift();
        }

        #endregion

        #region Adjustment TrackBars (Saturation / Brightness / Contrast)

        private void TrackBarAdjust_MouseUp(object sender, MouseEventArgs e)
        {
            ApplyAdjustments();
        }

        private void TrackBarSaturation_Scroll(object sender, EventArgs e)
        {
            if (labelSaturation != null)
                labelSaturation.Text = $"Saturation: {trackBarSaturation.Value}";
        }

        private void TrackBarBrightness_Scroll(object sender, EventArgs e)
        {
            if (labelBrightness != null)
                labelBrightness.Text = $"Brightness: {trackBarBrightness.Value}";
        }

        private void TrackBarContrast_Scroll(object sender, EventArgs e)
        {
            if (labelContrast != null)
                labelContrast.Text = $"Contrast:   {trackBarContrast.Value}";
        }

        #endregion

        #region Apply Hue Shift

        /// <summary>
        /// Applies a hue-shift to every pixel inside the current selection
        /// (freehand path or shapes). Falls back to the full image when
        /// no selection exists.
        /// </summary>
        private void ApplyHueShift()
        {
            if (_originalImage == null) return;

            int hueShift = TrackBarColor.Value;

            if (hueShift == 0)
            {
                PictureBoxImageColor.Image = new Bitmap(_originalImage);
                return;
            }

            SaveUndo();
            Bitmap bmp = new Bitmap(PictureBoxImageColor.Image);
            GraphicsPath path = BuildSelectionPath(bmp.Width, bmp.Height);

            ProcessPixels(bmp, path, pixelColor =>
            {
                ColorToHSL(pixelColor, out double h, out double s, out double l);
                h = (h + hueShift + 360) % 360;
                return HSLToColor(h, s, l);
            });

            PictureBoxImageColor.Image = bmp;
            UpdateColorStatus(Color.Empty);
        }

        #endregion

        #region Apply Saturation / Brightness / Contrast

        /// <summary>
        /// Applies saturation, brightness, and contrast adjustments
        /// from their respective track bars to the original image.
        /// </summary>
        private void ApplyAdjustments()
        {
            if (_originalImage == null) return;

            int satDelta = trackBarSaturation?.Value ?? 0;
            int briDelta = trackBarBrightness?.Value ?? 0;
            int conDelta = trackBarContrast?.Value ?? 0;

            if (satDelta == 0 && briDelta == 0 && conDelta == 0)
            {
                PictureBoxImageColor.Image = new Bitmap(_originalImage);
                return;
            }

            SaveUndo();
            Bitmap bmp = new Bitmap(_originalImage);
            GraphicsPath path = BuildSelectionPath(bmp.Width, bmp.Height);

            double contrastFactor = (259.0 * (conDelta + 255)) / (255.0 * (259 - conDelta));

            ProcessPixels(bmp, path, pixelColor =>
            {
                ColorToHSL(pixelColor, out double h, out double s, out double l);

                s = Math.Max(0, Math.Min(100, s + satDelta));
                l = Math.Max(0, Math.Min(100, l + briDelta * 0.4));

                Color c = HSLToColor(h, s, l);

                int rr = Clamp255(contrastFactor * (c.R - 128) + 128);
                int gg = Clamp255(contrastFactor * (c.G - 128) + 128);
                int bb = Clamp255(contrastFactor * (c.B - 128) + 128);

                return Color.FromArgb(c.A, rr, gg, bb);
            });

            PictureBoxImageColor.Image = bmp;
            UpdateColorStatus(Color.Empty);
        }

        #endregion

        #region Pixel Processing Helper

        /// <summary>
        /// Iterates every pixel in the bitmap that lies inside <paramref name="path"/>
        /// and replaces it with the result of <paramref name="transform"/>.
        /// </summary>
        private static void ProcessPixels(Bitmap bmp, GraphicsPath path,
                                          Func<Color, Color> transform)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    if (!path.IsVisible(x, y)) continue;

                    Color src = bmp.GetPixel(x, y);
                    if (src.A == 0) continue;           // skip fully transparent pixels

                    bmp.SetPixel(x, y, transform(src));
                }
            }
        }

        /// <summary>
        /// Builds a GraphicsPath that represents the current selection.
        /// Priority: shapes → freehand path → full image rectangle.
        /// </summary>
        private GraphicsPath BuildSelectionPath(int imageWidth, int imageHeight)
        {
            GraphicsPath path = new GraphicsPath();

            if (_shapes.Count > 0)
            {
                foreach (List<Point> shape in _shapes)
                {
                    if (shape.Count >= 2)
                        path.AddPolygon(shape.ToArray());
                }
            }
            else if (_points.Count > 1)
            {
                path.AddLines(_points.ToArray());
            }
            else
            {
                path.AddRectangle(new Rectangle(0, 0, imageWidth, imageHeight));
            }

            return path;
        }

        #endregion

        #region Grayscale

        private void BtnGrayscale_Click(object sender, EventArgs e)
        {
            if (PictureBoxImageColor.Image == null) return;

            SaveUndo();
            Bitmap bmp = new Bitmap(PictureBoxImageColor.Image);
            GraphicsPath path = BuildSelectionPath(bmp.Width, bmp.Height);

            ProcessPixels(bmp, path, pixelColor =>
            {
                int gray = (int)(pixelColor.R * 0.299 +
                                 pixelColor.G * 0.587 +
                                 pixelColor.B * 0.114);
                return Color.FromArgb(pixelColor.A, gray, gray, gray);
            });

            PictureBoxImageColor.Image = bmp;
        }

        #endregion

        #region Sepia

        private void BtnSepia_Click(object sender, EventArgs e)
        {
            if (PictureBoxImageColor.Image == null) return;

            SaveUndo();
            Bitmap bmp = new Bitmap(PictureBoxImageColor.Image);
            GraphicsPath path = BuildSelectionPath(bmp.Width, bmp.Height);

            ProcessPixels(bmp, path, pixelColor =>
            {
                int r = Clamp255(pixelColor.R * 0.393 + pixelColor.G * 0.769 + pixelColor.B * 0.189);
                int g = Clamp255(pixelColor.R * 0.349 + pixelColor.G * 0.686 + pixelColor.B * 0.168);
                int b = Clamp255(pixelColor.R * 0.272 + pixelColor.G * 0.534 + pixelColor.B * 0.131);
                return Color.FromArgb(pixelColor.A, r, g, b);
            });

            PictureBoxImageColor.Image = bmp;
        }

        #endregion

        #region Invert Colors

        private void BtnInvert_Click(object sender, EventArgs e)
        {
            if (PictureBoxImageColor.Image == null) return;

            SaveUndo();
            Bitmap bmp = new Bitmap(PictureBoxImageColor.Image);
            GraphicsPath path = BuildSelectionPath(bmp.Width, bmp.Height);

            ProcessPixels(bmp, path, pixelColor =>
                Color.FromArgb(pixelColor.A,
                               255 - pixelColor.R,
                               255 - pixelColor.G,
                               255 - pixelColor.B));

            PictureBoxImageColor.Image = bmp;
        }

        #endregion

        #region Reset Image to Original

        private void BtnResetImage_Click(object sender, EventArgs e)
        {
            if (_originalImage != null)
            {
                SaveUndo();
                PictureBoxImageColor.Image = new Bitmap(_originalImage);
                ResetAdjustmentTrackBars();
            }
        }

        #endregion

        #region TrackBar – Save / Load Position

        private void ButtonSavePosition_Click(object sender, EventArgs e)
        {
            _savedTrackBarPosition = TrackBarColor.Value;
            labelColorShift.Text = $"Hue Shift: {_savedTrackBarPosition}°  [marked]";
        }

        private void ButtonLoadPosition_Click(object sender, EventArgs e)
        {
            TrackBarColor.Value = _savedTrackBarPosition;
            labelColorShift.Text = $"Hue Shift: {_savedTrackBarPosition}°";
            ApplyHueShift();
        }

        #endregion

        #region Clipboard & File I/O

        private void ButtonCopyToClipboard_Click(object sender, EventArgs e)
        {
            if (PictureBoxImageColor.Image != null)
                Clipboard.SetImage(PictureBoxImageColor.Image);
        }

        private void ButtonSaveToFile_Click(object sender, EventArgs e)
        {
            if (PictureBoxImageColor.Image == null) return;

            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "PNG Image|*.png|Bitmap Image|*.bmp|TIFF Image|*.tiff";
                dlg.Title = "Export Texture Image";
                if (dlg.ShowDialog() != DialogResult.OK || dlg.FileName == "") return;

                ImageFormat fmt = dlg.FilterIndex switch
                {
                    2 => ImageFormat.Bmp,
                    3 => ImageFormat.Tiff,
                    _ => ImageFormat.Png
                };
                PictureBoxImageColor.Image.Save(dlg.FileName, fmt);
            }
        }

        private void BtnImportClipboard_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                SaveUndo();
                Image image = Clipboard.GetImage();
                PictureBoxImageColor.Image = image;
                _originalImage = new Bitmap(image);
            }
            else
            {
                MessageBox.Show("The clipboard does not contain an image.",
                                "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnLoadImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Image Files|*.bmp;*.png;*.tiff;*.jpg;*.jpeg";
                if (dlg.ShowDialog() != DialogResult.OK) return;

                SaveUndo();
                Image image = Image.FromFile(dlg.FileName);
                PictureBoxImageColor.Image = image;
                _originalImage = new Bitmap(image);

                int imageX = (PictureBoxImageColor.Width - image.Width) / 2;
                int imageY = (PictureBoxImageColor.Height - image.Height) / 2;
                _imagePosition = new Point(imageX, imageY);
            }
        }

        #endregion

        #region Mouse – Freehand Selection

        private void PictureBoxImageColor_MouseDown(object sender, MouseEventArgs e)
        {
            if (_isPickingColor) return;

            _isSelecting = true;
            _points.Clear();
            _points.Add(ConvertMouseToImageCoordinates(e.Location));
            PictureBoxImageColor.Invalidate();
        }

        private void PictureBoxImageColor_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isSelecting)
            {
                _points.Add(ConvertMouseToImageCoordinates(e.Location));
                _cursorPixelPosition = e.Location;
                PictureBoxImageColor.Invalidate();
            }

            if (_isPickingColor && PictureBoxImageColor.Image != null)
            {
                Point imgPt = ConvertMouseToImageCoordinates(e.Location);
                Bitmap bmp = (Bitmap)PictureBoxImageColor.Image;
                if (imgPt.X >= 0 && imgPt.X < bmp.Width &&
                    imgPt.Y >= 0 && imgPt.Y < bmp.Height)
                {
                    Color c = bmp.GetPixel(imgPt.X, imgPt.Y);
                    panelColor.BackColor = c;
                    tbColorCode.Text = ColorTranslator.ToHtml(c);
                }
            }
        }

        private void PictureBoxImageColor_MouseUp(object sender, MouseEventArgs e)
        {
            _isSelecting = false;
        }

        private void PictureBoxImageColor_MouseClick(object sender, MouseEventArgs e)
        {
            if (!_isPickingColor || PictureBoxImageColor.Image == null) return;

            Point imgPt = ConvertMouseToImageCoordinates(e.Location);
            Bitmap bmp = (Bitmap)PictureBoxImageColor.Image;
            if (imgPt.X >= 0 && imgPt.X < bmp.Width &&
                imgPt.Y >= 0 && imgPt.Y < bmp.Height)
            {
                Color c = bmp.GetPixel(imgPt.X, imgPt.Y);
                panelColor.BackColor = c;
                tbColorCode.Text = ColorTranslator.ToHtml(c);
            }

            _isPickingColor = false;
            PictureBoxImageColor.Cursor = _customCursor;
            BtColorPincers.Text = "Pipette";
        }

        #endregion

        #region PictureBox – Paint

        private void PictureBoxImageColor_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (Pen pen = new Pen(_cursorColor, 1.5f))
            {
                // Freehand selection path
                if (_points.Count > 1)
                {
                    List<Point> adjusted = AdjustPointsForDisplay(_points);
                    g.DrawLines(pen, adjusted.ToArray());
                }

                // Generated shapes
                foreach (List<Point> shape in _shapes)
                {
                    if (shape.Count < 2) continue;
                    List<Point> adjusted = AdjustPointsForDisplay(shape);
                    g.DrawLines(pen, adjusted.ToArray());
                }
            }
        }

        private List<Point> AdjustPointsForDisplay(List<Point> pts)
        {
            var result = new List<Point>(pts.Count);
            foreach (Point p in pts)
                result.Add(new Point(p.X + _imagePosition.X, p.Y + _imagePosition.Y));
            return result;
        }

        #endregion

        #region Cursor Events

        private void PictureBoxImageColor_MouseEnter(object sender, EventArgs e)
        {
            PictureBoxImageColor.Cursor = _customCursor;
        }

        private void PictureBoxImageColor_MouseLeave(object sender, EventArgs e)
        {
            PictureBoxImageColor.Cursor = Cursors.Default;
        }

        private void ButtonChangeCursorColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog dlg = new ColorDialog { Color = _cursorColor })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                _cursorColor = dlg.Color;
                _customCursor?.Dispose();
                _customCursor = CreateCustomCursor(_cursorColor);
            }
        }

        #endregion

        #region Coordinate Conversion

        private Point ConvertMouseToImageCoordinates(Point mouseLocation)
        {
            if (PictureBoxImageColor.Image == null)
                return Point.Empty;

            Image image = PictureBoxImageColor.Image;
            int imageX = (PictureBoxImageColor.Width - image.Width) / 2;
            int imageY = (PictureBoxImageColor.Height - image.Height) / 2;

            int x = Math.Max(0, Math.Min(mouseLocation.X - imageX, image.Width - 1));
            int y = Math.Max(0, Math.Min(mouseLocation.Y - imageY, image.Height - 1));

            return new Point(x, y);
        }

        #endregion

        #region Pattern – Save / Restore / Clear

        private void SavePatternButton_Click(object sender, EventArgs e)
        {
            if (_points.Count > 0)
                _savedPattern = new List<Point>(_points);
        }

        private void RestorePatternButton_Click(object sender, EventArgs e)
        {
            if (_savedPattern != null)
            {
                _points = new List<Point>(_savedPattern);
                PictureBoxImageColor.Invalidate();
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            _points.Clear();
            _shapes.Clear();
            PictureBoxImageColor.Invalidate();
        }

        #endregion

        #region Pattern – File Save / Load

        private void SaveButton_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog { Filter = "Pattern Files|*.pat;*.txt" })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    SavePattern(dlg.FileName);
            }
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog { Filter = "Pattern Files|*.pat;*.txt" })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    LoadPattern(dlg.FileName);
            }
        }

        private void SavePattern(string filename)
        {
            using (StreamWriter w = new StreamWriter(filename))
            {
                w.WriteLine($"# UoFiddler Texture Pattern – {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                foreach (Point p in _points)
                    w.WriteLine($"{p.X},{p.Y}");
            }
        }

        private void LoadPattern(string filename)
        {
            if (!checkBoxKeepPreviousPattern.Checked)
                _points.Clear();

            foreach (string line in File.ReadAllLines(filename))
            {
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split(',');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int x) &&
                    int.TryParse(parts[1], out int y))
                {
                    _points.Add(new Point(x, y));
                }
            }

            PictureBoxImageColor.Invalidate();
        }

        #endregion

        #region Color Update Helper

        private void UpdateColorStatus(Color color)
        {
            if (color == Color.Empty) return;
            labelColorValues.Text = $"R: {color.R}  G: {color.G}  B: {color.B}  A: {color.A}";
            panelColor.BackColor = color;
            tbColorCode.Text = ColorTranslator.ToHtml(color);
        }

        #endregion

        #region Color Picker (Pipette)

        private void BtColorPincers_Click(object sender, EventArgs e)
        {
            _isPickingColor = !_isPickingColor;
            PictureBoxImageColor.Cursor = _isPickingColor ? Cursors.Cross : _customCursor;
            BtColorPincers.Text = _isPickingColor ? "Picking…" : "Pipette";
        }

        #endregion

        #region Exchange Selective Colors

        private void BtExchangeSelectiveColors_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbColorCode.Text)) return;

            using (ColorDialog dlg = new ColorDialog())
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    Color oldColor = ColorTranslator.FromHtml(tbColorCode.Text);
                    ExchangeColor(oldColor, dlg.Color);
                }
                catch
                {
                    MessageBox.Show("Invalid color code in the text box.",
                                    "Color Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExchangeColor(Color oldColor, Color newColor)
        {
            if (PictureBoxImageColor.Image == null) return;

            SaveUndo();
            Bitmap bmp = new Bitmap(PictureBoxImageColor.Image);
            GraphicsPath path = BuildSelectionPath(bmp.Width, bmp.Height);

            int tolerance = numericTolerance != null ? (int)numericTolerance.Value : 0;

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    if (!path.IsVisible(x, y)) continue;
                    Color px = bmp.GetPixel(x, y);
                    if (ColorDistance(px, oldColor) <= tolerance)
                        bmp.SetPixel(x, y, Color.FromArgb(px.A, newColor));
                }
            }

            PictureBoxImageColor.Image = bmp;
        }

        /// <summary>Euclidean distance in RGB space.</summary>
        private static int ColorDistance(Color a, Color b) =>
            (int)Math.Sqrt(
                Math.Pow(a.R - b.R, 2) +
                Math.Pow(a.G - b.G, 2) +
                Math.Pow(a.B - b.B, 2));

        #endregion

        #region Shape Generation – Random Pattern (Points)

        private void ButtonGeneratePattern_Click(object sender, EventArgs e)
        {
            if (PictureBoxImageColor.Image == null) return;

            int count = trackBarCount?.Value ?? 50;
            _points.Clear();

            for (int i = 0; i < count; i++)
            {
                _points.Add(new Point(
                    _random.Next(PictureBoxImageColor.Image.Width),
                    _random.Next(PictureBoxImageColor.Image.Height)));
            }

            PictureBoxImageColor.Invalidate();
        }

        #endregion

        #region Shape Generation – Circles

        private void ButtonGenerateCircles_Click(object sender, EventArgs e)
        {
            if (PictureBoxImageColor.Image == null) return;

            int count = trackBarCount?.Value ?? 5;
            int minSize = checkBoxRandomSize.Checked ? 3 : (trackBarSize?.Value ?? 5);
            int maxSize = checkBoxRandomSize.Checked ? 20 : (trackBarSize?.Value ?? 5);

            GenerateRandomCircles(count, minSize, maxSize);
        }

        private void GenerateRandomCircles(int count, int minRadius, int maxRadius)
        {
            _shapes.Clear();

            for (int i = 0; i < count; i++)
            {
                int radius = _random.Next(minRadius, Math.Max(minRadius + 1, maxRadius + 1));
                int x = 0, y = 0, attempts = 100;

                do
                {
                    x = _random.Next(radius + 3,
                        PictureBoxImageColor.Image.Width - radius - 3);
                    y = _random.Next(radius + 3,
                        PictureBoxImageColor.Image.Height - radius - 3);
                }
                while (CirclesOverlap(x, y, radius) && --attempts > 0);

                if (attempts <= 0) continue;

                var circle = new List<Point>();
                for (int angle = 0; angle <= 360; angle += 2)
                {
                    circle.Add(new Point(
                        x + (int)(radius * Math.Cos(angle * Math.PI / 180)),
                        y + (int)(radius * Math.Sin(angle * Math.PI / 180))));
                }

                _shapes.Add(circle);
            }

            PictureBoxImageColor.Invalidate();
        }

        private bool CirclesOverlap(int x, int y, int radius)
        {
            foreach (List<Point> shape in _shapes)
            {
                if (shape.Count == 0) continue;
                int cx = shape[shape.Count / 2].X;
                int cy = shape[shape.Count / 2].Y;
                double dist = Math.Sqrt((cx - x) * (cx - x) + (cy - y) * (cy - y));
                if (dist < radius * 2 + 2) return true;
            }
            return false;
        }

        #endregion

        #region Shape Generation – Squares / Rectangles

        private void ButtonGenerateSquares_Click(object sender, EventArgs e)
        {
            if (PictureBoxImageColor.Image == null) return;

            int count = trackBarCount?.Value ?? 5;
            int minSize = checkBoxRandomSize.Checked ? 8 : (trackBarSize?.Value ?? 10);
            int maxSize = checkBoxRandomSize.Checked ? 40 : (trackBarSize?.Value ?? 10);

            GenerateRandomSquares(count, minSize, maxSize);
        }

        private void GenerateRandomSquares(int count, int minSize, int maxSize)
        {
            _shapes.Clear();

            for (int i = 0; i < count; i++)
            {
                int size = 0, x = 0, y = 0, attempts = 100;

                do
                {
                    size = _random.Next(minSize, Math.Max(minSize + 1, maxSize + 1));
                    x = _random.Next(3, PictureBoxImageColor.Image.Width - size - 3);
                    y = _random.Next(3, PictureBoxImageColor.Image.Height - size - 3);
                }
                while (SquaresOverlap(x, y, size) && --attempts > 0);

                if (attempts <= 0) continue;

                _shapes.Add(BuildRectangleOutline(x, y, size, size));
            }

            PictureBoxImageColor.Invalidate();
        }

        private static List<Point> BuildRectangleOutline(int x, int y, int w, int h)
        {
            var pts = new List<Point>
            {
                new Point(x,     y),
                new Point(x + w, y),
                new Point(x + w, y + h),
                new Point(x,     y + h),
                new Point(x,     y)      // close
            };
            return pts;
        }

        private bool SquaresOverlap(int x, int y, int size)
        {
            int pad = 3;
            Rectangle nr = new Rectangle(x - pad, y - pad, size + pad * 2, size + pad * 2);

            foreach (List<Point> shape in _shapes)
            {
                if (shape.Count < 2) continue;
                int x0 = shape[0].X, y0 = shape[0].Y;
                int x1 = shape[2].X, y1 = shape[2].Y;
                if (nr.IntersectsWith(Rectangle.FromLTRB(x0, y0, x1, y1)))
                    return true;
            }
            return false;
        }

        #endregion

        #region Shape Generation – Triangles

        private void ButtonGenerateTriangles_Click(object sender, EventArgs e)
        {
            if (PictureBoxImageColor.Image == null) return;

            int count = trackBarCount?.Value ?? 5;
            int minSize = checkBoxRandomSize.Checked ? 10 : (trackBarSize?.Value ?? 15);
            int maxSize = checkBoxRandomSize.Checked ? 40 : (trackBarSize?.Value ?? 15);

            _shapes.Clear();

            for (int i = 0; i < count; i++)
            {
                int size = _random.Next(minSize, Math.Max(minSize + 1, maxSize + 1));
                int cx = _random.Next(size + 3, PictureBoxImageColor.Image.Width - size - 3);
                int cy = _random.Next(size + 3, PictureBoxImageColor.Image.Height - size - 3);

                double rot = _random.NextDouble() * Math.PI * 2;
                var pts = new List<Point>();
                for (int k = 0; k < 4; k++)
                {
                    double angle = rot + k * (2 * Math.PI / 3);
                    pts.Add(new Point(
                        cx + (int)(size * Math.Cos(angle)),
                        cy + (int)(size * Math.Sin(angle))));
                }

                _shapes.Add(pts);
            }

            PictureBoxImageColor.Invalidate();
        }

        #endregion

        #region Shape Generation – Diamonds

        private void ButtonGenerateDiamonds_Click(object sender, EventArgs e)
        {
            if (PictureBoxImageColor.Image == null) return;

            int count = trackBarCount?.Value ?? 5;
            int minSize = checkBoxRandomSize.Checked ? 8 : (trackBarSize?.Value ?? 12);
            int maxSize = checkBoxRandomSize.Checked ? 35 : (trackBarSize?.Value ?? 12);

            _shapes.Clear();

            for (int i = 0; i < count; i++)
            {
                int size = _random.Next(minSize, Math.Max(minSize + 1, maxSize + 1));
                int cx = _random.Next(size + 3, PictureBoxImageColor.Image.Width - size - 3);
                int cy = _random.Next(size + 3, PictureBoxImageColor.Image.Height - size - 3);

                _shapes.Add(new List<Point>
                {
                    new Point(cx,        cy - size),
                    new Point(cx + size, cy),
                    new Point(cx,        cy + size),
                    new Point(cx - size, cy),
                    new Point(cx,        cy - size)   // close
                });
            }

            PictureBoxImageColor.Invalidate();
        }

        #endregion

        #region Shape Generation – Stars

        private void ButtonGenerateStars_Click(object sender, EventArgs e)
        {
            if (PictureBoxImageColor.Image == null) return;

            int count = trackBarCount?.Value ?? 5;
            int minSize = checkBoxRandomSize.Checked ? 8 : (trackBarSize?.Value ?? 12);
            int maxSize = checkBoxRandomSize.Checked ? 35 : (trackBarSize?.Value ?? 12);

            _shapes.Clear();

            for (int i = 0; i < count; i++)
            {
                int outerR = _random.Next(minSize, Math.Max(minSize + 1, maxSize + 1));
                int innerR = outerR / 2;
                int cx = _random.Next(outerR + 3, PictureBoxImageColor.Image.Width - outerR - 3);
                int cy = _random.Next(outerR + 3, PictureBoxImageColor.Image.Height - outerR - 3);

                _shapes.Add(BuildStarPoints(cx, cy, outerR, innerR, 5));
            }

            PictureBoxImageColor.Invalidate();
        }

        private static List<Point> BuildStarPoints(int cx, int cy, int outerR, int innerR, int tips)
        {
            var pts = new List<Point>();
            double step = Math.PI / tips;

            for (int k = 0; k < tips * 2; k++)
            {
                double angle = k * step - Math.PI / 2;
                int r = (k % 2 == 0) ? outerR : innerR;
                pts.Add(new Point(cx + (int)(r * Math.Cos(angle)),
                                  cy + (int)(r * Math.Sin(angle))));
            }

            pts.Add(pts[0]); // close
            return pts;
        }

        #endregion

        #region Shape Generation – Hexagons

        private void ButtonGenerateHexagons_Click(object sender, EventArgs e)
        {
            if (PictureBoxImageColor.Image == null) return;

            int count = trackBarCount?.Value ?? 5;
            int minSize = checkBoxRandomSize.Checked ? 8 : (trackBarSize?.Value ?? 12);
            int maxSize = checkBoxRandomSize.Checked ? 35 : (trackBarSize?.Value ?? 12);

            _shapes.Clear();

            for (int i = 0; i < count; i++)
            {
                int r = _random.Next(minSize, Math.Max(minSize + 1, maxSize + 1));
                int cx = _random.Next(r + 3, PictureBoxImageColor.Image.Width - r - 3);
                int cy = _random.Next(r + 3, PictureBoxImageColor.Image.Height - r - 3);

                var pts = new List<Point>();
                for (int k = 0; k <= 6; k++)
                {
                    double angle = Math.PI / 180 * (60 * k - 30);
                    pts.Add(new Point(cx + (int)(r * Math.Cos(angle)),
                                      cy + (int)(r * Math.Sin(angle))));
                }

                _shapes.Add(pts);
            }

            PictureBoxImageColor.Invalidate();
        }

        #endregion

        #region TrackBar Count / Size – Scroll + MouseUp (FIX: MouseUp regenerates shapes)

        private void TrackBarCount_Scroll(object sender, EventArgs e)
        {
            if (labelCount != null)
                labelCount.Text = $"Count: {trackBarCount.Value}";
        }

        private void TrackBarSize_Scroll(object sender, EventArgs e)
        {
            if (labelSize != null)
                labelSize.Text = $"Size: {trackBarSize.Value}";
        }

        /// <summary>
        /// Regenerates shapes after the user releases the Count track bar.
        /// Detects which shape type was last used and re-generates accordingly.
        /// </summary>
        private void TrackBarCount_MouseUp(object sender, MouseEventArgs e)
        {
            RegenerateCurrentShapes();
        }

        /// <summary>
        /// Regenerates shapes after the user releases the Size track bar.
        /// </summary>
        private void TrackBarSize_MouseUp(object sender, MouseEventArgs e)
        {
            RegenerateCurrentShapes();
        }

        /// <summary>
        /// Re-generates whatever shape type is currently in _shapes by inspecting
        /// the point count of the first shape (circles have many points, rectangles 5,
        /// triangles 4, diamonds 5, stars 11, hexagons 7).
        /// Falls back to regenerating circles when the list is empty.
        /// </summary>
        private void RegenerateCurrentShapes()
        {
            if (PictureBoxImageColor.Image == null) return;

            int count = trackBarCount?.Value ?? 5;
            int size = trackBarSize?.Value ?? 15;
            int minS = checkBoxRandomSize.Checked ? size / 2 : size;
            int maxS = checkBoxRandomSize.Checked ? size * 2 : size;

            if (_shapes.Count == 0)
                return; // nothing to regenerate yet

            int pts = _shapes[0].Count;

            if (pts > 20)
                GenerateRandomCircles(count, minS, maxS);
            else if (pts == 5)
                GenerateRandomSquares(count, minS, maxS);
            else if (pts == 4)
            {
                // triangle (4 pts including close)
                _shapes.Clear();
                ButtonGenerateTriangles_Click(sender: this, e: EventArgs.Empty);
            }
            else if (pts == 7)
            {
                _shapes.Clear();
                ButtonGenerateHexagons_Click(sender: this, e: EventArgs.Empty);
            }
            else if (pts == 11)
            {
                _shapes.Clear();
                ButtonGenerateStars_Click(sender: this, e: EventArgs.Empty);
            }
            else
            {
                GenerateRandomCircles(count, minS, maxS);
            }
        }

        #endregion

        #region CheckBoxRandomSize – CheckedChanged (FIX: refresh shape size range label)

        /// <summary>
        /// Updates the size label when the Random Size checkbox is toggled
        /// so the user knows which size mode is active.
        /// </summary>
        private void CheckBoxRandomSize_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxRandomSize.Checked)
            {
                if (labelSize != null)
                    labelSize.Text = "Size: rnd";
            }
            else
            {
                if (labelSize != null)
                    labelSize.Text = $"Size: {trackBarSize?.Value ?? 15}";
            }
        }

        #endregion

        #region Flip / Rotate

        private void BtnFlipH_Click(object sender, EventArgs e) =>
            ApplyImageTransform(img => { img.RotateFlip(RotateFlipType.RotateNoneFlipX); return img; });

        private void BtnFlipV_Click(object sender, EventArgs e) =>
            ApplyImageTransform(img => { img.RotateFlip(RotateFlipType.RotateNoneFlipY); return img; });

        private void BtnRotate90_Click(object sender, EventArgs e) =>
            ApplyImageTransform(img => { img.RotateFlip(RotateFlipType.Rotate90FlipNone); return img; });

        private void ApplyImageTransform(Func<Bitmap, Bitmap> transform)
        {
            if (PictureBoxImageColor.Image == null) return;

            SaveUndo();
            Bitmap bmp = new Bitmap(PictureBoxImageColor.Image);
            PictureBoxImageColor.Image = transform(bmp);
        }

        #endregion
    }
}
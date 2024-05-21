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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Ultima;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;


namespace UoFiddler.Controls.Forms
{
    public partial class TextureColorForm : Form
    {
        private int _currentTextureId = 0; // Start with texture ID 0
        private Bitmap _originalImage; // To store the original image
        private int _savedTrackBarPosition; // To store the position of the track bar
        private List<Point> _points = new List<Point>(); // To save the path of the mouse
        private bool _isSelecting = false; // To track whether the user is currently making a selection
        private Point _imagePosition; // To save the position of the image
        private List<Point> _savedPattern = null;
        private Random _random = null; // Generator Line Circle Shapes 
        private List<List<Point>> _circles = new List<List<Point>>(); // Point circles
        private List<List<Point>> _shapes = new List<List<Point>>(); // shapes
        private bool _isPickingColor = false; // To track the color selection mode


        private Cursor CustomCursor { get; set; }
        private Color CursorPixelColor { get; set; }
        private Point CursorPixelPosition { get; set; }


        public TextureColorForm()
        {
            InitializeComponent();

            this.KeyPreview = true;

            // Set the initial color of the cursor pixel
            this.CursorPixelColor = Color.Pink;

            // Create a custom cursor
            this.CustomCursor = CreateCustomCursor();
        }

        #region CreateTransparentCursor
        private Cursor CreateTransparentCursor()
        {
            Bitmap bitmap = new Bitmap(1, 1);
            bitmap.MakeTransparent();
            return new Cursor(bitmap.GetHicon());
        }
        #endregion

        #region CreateCustomCursor
        private Cursor CreateCustomCursor()
        {
            // Create a bitmap for the cursor
            Bitmap bitmap = new Bitmap(3, 3);

            // Fill the bitmap with the cursor color
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(this.CursorPixelColor);
            }

            // Create a cursor from the bitmap
            return new Cursor(bitmap.GetHicon());
        }
        #endregion

        #region SetCurrentTextureId
        public void SetCurrentTextureId(int textureId)
        {
            _currentTextureId = textureId;
        }
        #endregion

        #region UpdateImage
        public void UpdateImage()
        {
            // Check whether the texture exists before displaying it
            if (Textures.TestTexture(_currentTextureId))
            {
                Image textureImage = Textures.GetTexture(_currentTextureId);
                PictureBoxImageColor.Image = textureImage;
                _originalImage = new Bitmap(textureImage); // Save a copy of the original image

                // Calculate the position of the image within the PictureBox
                int imageX = (PictureBoxImageColor.Width - textureImage.Width) / 2;
                int imageY = (PictureBoxImageColor.Height - textureImage.Height) / 2;

                // Store the position for later use                
                this._imagePosition = new Point(imageX, imageY);

                // Convert the ID to a hexadecimal string
                string hexId = "0x" + _currentTextureId.ToString("X4");

                // Update the label with the ID, the size and the Hex address of the image
                lbIDNumber.Text = $"ID: {_currentTextureId} (Hex: {hexId}), Size: {PictureBoxImageColor.Image.Size}";
            }
        }
        #endregion

        private string GetHexAddress(Image image)
        {
            // Convert the image to a byte array
            ImageConverter converter = new ImageConverter();
            byte[] imageBytes = (byte[])converter.ConvertTo(image, typeof(byte[]));

            // Convert the byte array to a hex string
            StringBuilder hex = new StringBuilder(imageBytes.Length * 2);
            foreach (byte b in imageBytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        #region buttonPrevious
        private void ButtonPrevious_Click(object sender, EventArgs e)
        {
            // Decrease the texture ID but stop at 0
            do
            {
                if (_currentTextureId > 0)
                {
                    _currentTextureId--;
                }
            } while (!Textures.TestTexture(_currentTextureId) && _currentTextureId > 0);

            UpdateImage();
        }
        #endregion

        #region buttonNext
        private void ButtonNext_Click(object sender, EventArgs e)
        {
            // Increase the texture ID and refresh the image
            do
            {
                _currentTextureId++;
            } while (!Textures.TestTexture(_currentTextureId) && _currentTextureId < Textures.GetIdxLength());

            UpdateImage();
        }
        #endregion

        #region ColorToHSL
        public static void ColorToHSL(Color color, out double h, out double s, out double l)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            h = (max + min) / 2.0;
            l = (max + min) / 2.0;

            if (max == min)
            {
                h = s = 0; // achromatic
            }
            else
            {
                double d = max - min;
                s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);
                if (max == r)
                {
                    h = (g - b) / d + (g < b ? 6 : 0);
                }
                else if (max == g)
                {
                    h = (b - r) / d + 2;
                }
                else if (max == b)
                {
                    h = (r - g) / d + 4;
                }
                h /= 6;
            }

            h = h * 360;
            s = s * 100;
            l = l * 100;
        }
        #endregion

        #region Color HSLToColor
        public static Color HSLToColor(double h, double s, double l)
        {
            h = h / 360;
            s = s / 100;
            l = l / 100;

            double r, g, b;

            if (s == 0)
            {
                r = g = b = l; // achromatic
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

                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;
                r = hue2rgb(p, q, h + 1 / 3.0);
                g = hue2rgb(p, q, h);
                b = hue2rgb(p, q, h - 1 / 3.0);
            }

            return Color.FromArgb(
                (int)(Math.Max(Math.Min(r * 255, 255), 0)),
                (int)(Math.Max(Math.Min(g * 255, 255), 0)),
                (int)(Math.Max(Math.Min(b * 255, 255), 0)));
        }
        #endregion

        #region trackBarColor_MouseUp
        private void TrackBarColor_MouseUp(object sender, MouseEventArgs e)
        {
            // Check if there are any shapes
            if (_shapes.Count > 0)
            {
                // If there are shapes, call the ChangeImageColorCircleSquares method
                ChangeImageColorCircleSquares();
            }
            else
            {
                // If there are no shapes, call the ChangeImageColor method
                ChangeImageColor();
            }
        }
        #endregion

        #region ChangeImageColor
        private void ChangeImageColor()
        {
            // Get the current value of the track bar
            int hueShift = TrackBarColor.Value;

            // If the value is 0, reset the image to the original
            if (hueShift == 0)
            {
                PictureBoxImageColor.Image = new Bitmap(_originalImage);
                return;
            }

            // Get the current image from the PictureBox
            Bitmap bmp = new Bitmap(PictureBoxImageColor.Image);

            // Create a GraphicsPath from the points
            GraphicsPath path = new GraphicsPath();
            if (_points.Count > 1)
            {
                path.AddLines(_points.ToArray());
            }
            else
            {
                // If no points were drawn, set the path to the entire image
                path.AddRectangle(new Rectangle(0, 0, bmp.Width, bmp.Height));
            }

            // Iterate over the pixels in the image
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    // Check if the pixel is within the selected area
                    if (path.IsVisible(x, y))
                    {
                        // Get the current pixel color
                        Color pixelColor = bmp.GetPixel(x, y);

                        // Convert the color to HSL
                        double h;
                        double s;
                        double l;
                        ColorToHSL(pixelColor, out h, out s, out l);

                        // Shift the hue value
                        h = (h + hueShift) % 360;

                        // Convert back to RGB
                        Color newColor = HSLToColor(h, s, l);

                        // Set the new pixel color
                        bmp.SetPixel(x, y, newColor);

                        // Update the label with the current color values
                        labelColorValues.Text = $"R: {newColor.R}, G: {newColor.G}, B: {newColor.B}";
                    }
                }
            }

            // Update the PictureBox with the new image
            PictureBoxImageColor.Image = bmp;

            // Update the color display
            UpdateColorDisplay();
        }
        #endregion

        #region ChangeImageColorCircleSquares
        private void ChangeImageColorCircleSquares()
        {
            // Get the current value of the track bar
            int hueShift = TrackBarColor.Value;

            // If the value is 0, reset the image to the original
            if (hueShift == 0)
            {
                PictureBoxImageColor.Image = new Bitmap(_originalImage);
                return;
            }

            // Get the current image from the PictureBox
            Bitmap bmp = new Bitmap(PictureBoxImageColor.Image);

            // Check if there are any shapes
            if (_shapes.Count > 0)
            {
                // Iterate over each shape
                foreach (List<Point> shape in _shapes)
                {
                    // Create a GraphicsPath from the shape
                    GraphicsPath shapePath = new GraphicsPath();
                    shapePath.AddPolygon(shape.ToArray());

                    // Iterate over the pixels in the image
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        for (int y = 0; y < bmp.Height; y++)
                        {
                            // Check if the pixel is within the shape
                            if (shapePath.IsVisible(x, y))
                            {
                                // Get the current pixel color
                                Color pixelColor = bmp.GetPixel(x, y);

                                // Convert the color to HSL
                                double h;
                                double s;
                                double l;
                                ColorToHSL(pixelColor, out h, out s, out l);

                                // Shift the hue value
                                h = (h + hueShift) % 360;

                                // Convert back to RGB
                                Color newColor = HSLToColor(h, s, l);

                                // Set the new pixel color
                                bmp.SetPixel(x, y, newColor);

                                // Update the label with the current color values
                                labelColorValues.Text = $"R: {newColor.R}, G: {newColor.G}, B: {newColor.B}";
                            }
                        }
                    }
                }
            }
            else
            {
                // If there are no shapes, change the color of all pixels
                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        // Get the current pixel color
                        Color pixelColor = bmp.GetPixel(x, y);

                        // Convert the color to HSL
                        double h;
                        double s;
                        double l;
                        ColorToHSL(pixelColor, out h, out s, out l);

                        // Shift the hue value
                        h = (h + hueShift) % 360;

                        // Convert back to RGB
                        Color newColor = HSLToColor(h, s, l);

                        // Set the new pixel color
                        bmp.SetPixel(x, y, newColor);

                        // Update the label with the current color values
                        labelColorValues.Text = $"R: {newColor.R}, G: {newColor.G}, B: {newColor.B}";
                    }
                }
            }

            // Update the PictureBox with the new image
            PictureBoxImageColor.Image = bmp;

            // Update the color display
            UpdateColorDisplay();
        }
        #endregion

        #region trackBarColor_Scroll
        private void TrackBarColor_Scroll(object sender, EventArgs e)
        {
            // Get the current value of the track bar
            int colorShift = TrackBarColor.Value;

            // Update the label with the current value
            labelColorShift.Text = $"Color shift: {colorShift}";
        }
        #endregion

        #region trackBarColor_KeyUp
        private void TrackBarColor_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                // Check if there are any shapes
                if (_shapes.Count > 0)
                {
                    // If there are shapes, call the ChangeImageColorCircleSquares method
                    ChangeImageColorCircleSquares();
                }
                else
                {
                    // If there are no shapes, call the ChangeImageColor method
                    ChangeImageColor();
                }
            }
        }
        #endregion

        #region buttonSavePosition
        private void ButtonSavePosition_Click(object sender, EventArgs e)
        {
            // Save the current position of the track bar
            _savedTrackBarPosition = TrackBarColor.Value;
        }
        #endregion

        #region buttonLoadPosition
        private void ButtonLoadPosition_Click(object sender, EventArgs e)
        {
            // Set the position of the track bar to the saved value
            TrackBarColor.Value = _savedTrackBarPosition;

            // Update the label with the current value
            labelColorShift.Text = $"Color shift: {_savedTrackBarPosition}";

            // Update the image color
            ChangeImageColor();
        }
        #endregion

        #region buttonCopyToClipboard
        private void ButtonCopyToClipboard_Click(object sender, EventArgs e)
        {
            if (PictureBoxImageColor.Image != null)
            {
                Clipboard.SetImage(PictureBoxImageColor.Image);
            }
        }
        #endregion

        #region buttonSaveToFile_Click
        private void ButtonSaveToFile_Click(object sender, EventArgs e)
        {
            if (PictureBoxImageColor.Image != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Bitmap Image|*.bmp";
                saveFileDialog.Title = "Save an Image File";
                saveFileDialog.ShowDialog();

                if (saveFileDialog.FileName != "")
                {
                    PictureBoxImageColor.Image.Save(saveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                }
            }
        }
        #endregion

        #region PictureBoxImageColor_MouseDown
        private void PictureBoxImageColor_MouseDown(object sender, MouseEventArgs e)
        {
            // Start selecting
            _isSelecting = true;

            // Clear the list of points
            _points.Clear();

            // Calculate the mouse position relative to the PictureBox
            Point mousePosition = new Point(e.X - PictureBoxImageColor.Left + 9, e.Y - PictureBoxImageColor.Top + 9);

            // Convert the mouse coordinates to image coordinates
            Point imagePoint = ConvertMouseToImageCoordinates(mousePosition);

            // Add the current point to the list
            _points.Add(imagePoint);

            // Redraw the PictureBox
            PictureBoxImageColor.Invalidate();
        }
        #endregion

        #region PictureBoxImageColor_MouseMove
        private void PictureBoxImageColor_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isSelecting)
            {
                // Calculate the mouse position relative to the PictureBox
                Point mousePosition = new Point(e.X - PictureBoxImageColor.Left + 9, e.Y - PictureBoxImageColor.Top + 9);


                // Convert the mouse coordinates to image coordinates
                Point imagePoint = ConvertMouseToImageCoordinates(mousePosition);

                // Add the current point to the list
                _points.Add(imagePoint);

                // Update the position of the cursor pixel
                this.CursorPixelPosition = e.Location;

                // Redraw the PictureBox
                PictureBoxImageColor.Invalidate();
            }
            if (_isPickingColor)
            {
                // Calculate the mouse position relative to the PictureBox
                Point mousePosition = new Point(e.X - PictureBoxImageColor.Left + 9, e.Y - PictureBoxImageColor.Top + 9);

                // Convert the mouse coordinates to image coordinates
                Point imagePoint = ConvertMouseToImageCoordinates(mousePosition);

                // Get the color of the pixel at the current position
                Color pixelColor = ((Bitmap)PictureBoxImageColor.Image).GetPixel(imagePoint.X, imagePoint.Y);

                // Update the panelColor and tbColorCode with the new color
                panelColor.BackColor = pixelColor;
                tbColorCode.Text = ColorTranslator.ToHtml(pixelColor);
            }
        }
        #endregion

        #region PictureBoxImageColor_MouseUp
        private void PictureBoxImageColor_MouseUp(object sender, MouseEventArgs e)
        {
            // Stop selecting
            _isSelecting = false;
        }
        #endregion

        #region PictureBoxImageColor_Paint
        private void PictureBoxImageColor_Paint(object sender, PaintEventArgs e)
        {
            // Draw the path of the selection if there are enough points
            if (_points.Count > 1)
            {
                // Create a new list of points adjusted for the image position
                List<Point> adjustedPoints = new List<Point>();
                foreach (Point point in _points)
                {
                    adjustedPoints.Add(new Point(point.X + _imagePosition.X, point.Y + _imagePosition.Y));
                }

                // Draw the lines using the adjusted points
                e.Graphics.DrawLines(new Pen(this.CursorPixelColor), adjustedPoints.ToArray());

                // Get the position of the cursor relative to the PictureBox
                Point cursorPosition = PictureBoxImageColor.PointToClient(Cursor.Position);

                // Draw the cursor pixel at the current mouse position
                e.Graphics.FillRectangle(new SolidBrush(this.CursorPixelColor), cursorPosition.X, cursorPosition.Y, 1, 1);
            }

            // Draw the shapes
            foreach (List<Point> shape in _shapes)
            {
                // Create a new list of points adjusted for the image position
                List<Point> adjustedShape = new List<Point>();
                foreach (Point point in shape)
                {
                    adjustedShape.Add(new Point(point.X + _imagePosition.X, point.Y + _imagePosition.Y));
                }

                // Draw the shape using the adjusted points
                e.Graphics.DrawLines(new Pen(this.CursorPixelColor), adjustedShape.ToArray());
            }
        }
        #endregion

        #region ConvertMouseToImageCoordinates
        private Point ConvertMouseToImageCoordinates(Point mouseLocation)
        {
            // Get the image and PictureBox dimensions
            Image image = PictureBoxImageColor.Image;
            Rectangle box = PictureBoxImageColor.ClientRectangle;

            // Calculate the position of the image within the PictureBox
            int imageX = (box.Width - image.Width) / 2;
            int imageY = (box.Height - image.Height) / 2;

            // Convert the mouse coordinates from PictureBox coordinates to image coordinates
            int x = mouseLocation.X - imageX;
            int y = mouseLocation.Y - imageY;

            // Ensure the coordinates are within the bounds of the image
            x = Math.Max(Math.Min(x, image.Width - 1), 0);
            y = Math.Max(Math.Min(y, image.Height - 1), 0);

            return new Point(x, y);
        }
        #endregion

        #region PictureBoxImageColor_MouseEnter
        private void PictureBoxImageColor_MouseEnter(object sender, EventArgs e)
        {
            // Hide the cursor when the mouse enters the PictureBox
            PictureBoxImageColor.Cursor = this.CustomCursor;
        }
        #endregion

        #region PictureBoxImageColor_MouseLeave
        private void PictureBoxImageColor_MouseLeave(object sender, EventArgs e)
        {
            // Show the cursor when the mouse leaves the PictureBox
            PictureBoxImageColor.Cursor = Cursors.Default;
        }
        #endregion

        #region buttonChangeCursorColor
        private void ButtonChangeCursorColor_Click(object sender, EventArgs e)
        {
            // Open a color dialog to change the color of the cursor pixel
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    this.CursorPixelColor = colorDialog.Color;
                }
            }
        }
        #endregion

        #region SavePatternButton
        private void SavePatternButton_Click(object sender, EventArgs e)
        {
            // If there is a current pattern, save it
            if (_points.Count > 0)
            {
                _savedPattern = new List<Point>(_points);
            }
            // If there is no current pattern but there is a saved pattern, restore the saved pattern
            else if (_savedPattern != null)
            {
                _points = new List<Point>(_savedPattern);
                PictureBoxImageColor.Invalidate();  // Redraw the image to see the restored pattern
            }
        }
        #endregion

        #region RestorePatternButton
        private void RestorePatternButton_Click(object sender, EventArgs e)
        {
            // Check whether a pattern has been saved
            if (_savedPattern != null)
            {
                // Restore the saved pattern
                _points = new List<Point>(_savedPattern);

                // Redraw the image to see the restored pattern
                PictureBoxImageColor.Invalidate();
            }
        }
        #endregion

        #region btnClear_Click
        private void BtnClear_Click(object sender, EventArgs e)
        {
            // Clear the list of points
            _points.Clear();
            _shapes.Clear();

            // Redraw the image
            PictureBoxImageColor.Invalidate();
        }
        #endregion

        #region UpdateColorDisplay
        private void UpdateColorDisplay()
        {
            string[] colorParts = labelColorValues.Text.Split(new[] { ',', ':' }, StringSplitOptions.RemoveEmptyEntries);

            // Convert the parts to integers
            int r = int.Parse(colorParts[1]);
            int g = int.Parse(colorParts[3]);
            int b = int.Parse(colorParts[5]);

            // Create a Color instance from the RGB values
            Color color = Color.FromArgb(r, g, b);

            // Set the panel background color to the selected color
            panelColor.BackColor = color;
        }
        #endregion


        #region buttonGeneratePattern
        private void ButtonGeneratePattern_Click(object sender, EventArgs e)
        {
            GenerateRandomPoints(50); //Points 50 
            PictureBoxImageColor.Invalidate();
        }
        #endregion

        #region buttonGenerateCircles
        private void ButtonGenerateCircles_Click(object sender, EventArgs e)
        {
            if (_random == null)
            {
                _random = new Random();
            }

            int minSize = checkBoxRandomSize.Checked ? 3 : trackBarSize.Value;
            int maxSize = checkBoxRandomSize.Checked ? 15 : trackBarSize.Value;

            GenerateRandomCircles(trackBarCount.Value, minSize, maxSize);
            PictureBoxImageColor.Invalidate();
        }
        #endregion

        #region buttonGenerateSquares
        private void ButtonGenerateSquares_Click(object sender, EventArgs e)
        {
            if (_random == null)
            {
                _random = new Random();
            }

            int minSize = checkBoxRandomSize.Checked ? 10 : trackBarSize.Value;
            int maxSize = checkBoxRandomSize.Checked ? 31 : trackBarSize.Value;

            GenerateRandomSquares(trackBarCount.Value, minSize, maxSize);
            PictureBoxImageColor.Invalidate();
        }
        #endregion


        #region GenerateRandomPoints
        private void GenerateRandomPoints(int count)
        {
            if (_random == null)
            {
                _random = new Random();
            }

            _points.Clear();

            for (int i = 0; i < count; i++)
            {
                int x = _random.Next(PictureBoxImageColor.Image.Width);
                int y = _random.Next(PictureBoxImageColor.Image.Height);
                _points.Add(new Point(x, y));
            }
        }
        #endregion

        #region GenerateRandomCircles
        private void GenerateRandomCircles(int count, int minRadius, int maxRadius)
        {
            if (_random == null)
            {
                _random = new Random();
            }

            _shapes.Clear(); // Clear the list of shapes at the beginning

            for (int i = 0; i < count; i++)
            {
                int radius = _random.Next(minRadius, maxRadius + 1);
                int x, y;

                int maxAttempts = 100; // Maximum number of attempts to generate a non-overlapping shape
                do
                {
                    x = _random.Next(radius + 3, PictureBoxImageColor.Image.Width - radius - 3); // Add padding to x
                    y = _random.Next(radius + 3, PictureBoxImageColor.Image.Height - radius - 3); // Add padding to y
                    maxAttempts--;
                }
                while (CirclesOverlap(x, y, radius) && maxAttempts > 0);

                if (maxAttempts <= 0) continue; // When the maximum number of attempts is reached, move on to the next form

                List<Point> circle = new List<Point>();

                for (int angle = 0; angle < 360; angle++)
                {
                    int circleX = (int)(x + radius * Math.Cos(angle * Math.PI / 180));
                    int circleY = (int)(y + radius * Math.Sin(angle * Math.PI / 180));
                    circle.Add(new Point(circleX, circleY));
                }

                _shapes.Add(circle);
            }

            PictureBoxImageColor.Invalidate();
        }
        #endregion

        #region CirclesOverlap
        private bool CirclesOverlap(int x, int y, int radius)
        {
            foreach (List<Point> shape in _shapes)
            {
                int dx = shape[0].X - x;
                int dy = shape[0].Y - y;
                int distance = (int)Math.Sqrt(dx * dx + dy * dy);

                if (distance < radius * 2)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region SquaresOverlap
        private bool SquaresOverlap(int x, int y, int size)
        {
            int padding = 3; // Put the desired distance between the squares
            Rectangle newSquare = new Rectangle(x - padding, y - padding, size + 2 * padding, size + 2 * padding);

            foreach (List<Point> shape in _shapes)
            {
                int existingWidth = shape[1].X - shape[0].X;
                int existingHeight = shape[2].Y - shape[0].Y;
                Rectangle existingSquare = new Rectangle(shape[0].X, shape[0].Y, existingWidth, existingHeight);

                if (existingSquare.IntersectsWith(newSquare))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region GenerateRandomSquares
        private void GenerateRandomSquares(int count, int minSize, int maxSize)
        {
            if (_random == null)
            {
                _random = new Random();
            }

            _shapes.Clear(); // Clear the list of shapes at the beginning

            for (int i = 0; i < count; i++)
            {
                int size;
                int x;
                int y;

                // Generate squares until you find one that doesn't overlap
                int maxAttempts = 100; // Maximum number of attempts to generate a non-overlapping shape
                do
                {
                    size = _random.Next(minSize, maxSize + 1);
                    x = _random.Next(size + 3, PictureBoxImageColor.Image.Width - size - 3); // Add padding to x
                    y = _random.Next(size + 3, PictureBoxImageColor.Image.Height - size - 3); // Add padding to y
                    maxAttempts--;
                }
                while (SquaresOverlap(x, y, size) && maxAttempts > 0);

                if (maxAttempts <= 0) continue; // When the maximum number of attempts is reached, move on to the next form

                List<Point> square = new List<Point>();

                // Generate the points for the top and bottom sides of the square
                for (int dx = 0; dx < size; dx++)
                {
                    square.Add(new Point(x + dx, y));
                    square.Add(new Point(x + dx, y + size - 1));
                }

                // Generate the points for the left and right sides of the square
                for (int dy = 0; dy < size; dy++)
                {
                    square.Add(new Point(x, y + dy));
                    square.Add(new Point(x + size - 1, y + dy));
                }

                _shapes.Add(square);
            }

            PictureBoxImageColor.Invalidate();
        }
        #endregion

        #region trackBarCount_Scroll
        private void TrackBarCount_Scroll(object sender, EventArgs e)
        {
            // Update the label with the current value of trackBarCount
            labelCount.Text = $"Count: {trackBarCount.Value}";

            // Update the number of squares and circles when the user moves the TrackBar
            GenerateRandomSquares(trackBarCount.Value, trackBarSize.Value, trackBarSize.Value);
            GenerateRandomCircles(trackBarCount.Value, trackBarSize.Value, trackBarSize.Value);
        }
        #endregion

        #region trackBarSize_Scroll
        private void TrackBarSize_Scroll(object sender, EventArgs e)
        {
            // Update the label with the current value of trackBarSize
            labelSize.Text = $"Size: {trackBarSize.Value}";

            // Update the size of the squares and circles when the user moves the TrackBar
            GenerateRandomSquares(trackBarCount.Value, trackBarSize.Value, trackBarSize.Value);
            GenerateRandomCircles(trackBarCount.Value, trackBarSize.Value, trackBarSize.Value);
        }
        #endregion

        #region SaveButton
        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Open a SaveFileDialog to let the user select a file
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text Files|*.txt";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Save the points to the selected file
                    SavePattern(saveFileDialog.FileName);
                }
            }
        }
        #endregion

        #region LoadButton
        private void LoadButton_Click(object sender, EventArgs e)
        {
            // Open an OpenFileDialog to let the user select a file
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text Files|*.txt";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Load the points from the selected file
                    LoadPattern(openFileDialog.FileName);
                }
            }
        }
        #endregion

        #region SavePattern
        // Save the points to a file
        private void SavePattern(string filename)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (Point point in _points)
                {
                    writer.WriteLine($"{point.X},{point.Y}");
                }
            }
        }
        #endregion

        #region LoadPattern
        // Load the points from a file
        private void LoadPattern(string filename)
        {
            // Only delete the points if the CheckBox is not activated
            if (!checkBoxKeepPreviousPattern.Checked)
            {
                _points.Clear();
            }

            using (StreamReader reader = new StreamReader(filename))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    int x = int.Parse(parts[0]);
                    int y = int.Parse(parts[1]);
                    _points.Add(new Point(x, y));
                }
            }

            PictureBoxImageColor.Invalidate(); // Redraw the pattern
        }

        #endregion

        #region btnImportClipbord
        private void BtnImportClipbord_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                Image image = Clipboard.GetImage();
                PictureBoxImageColor.Image = image;
            }
            else
            {
                MessageBox.Show("The clipboard does not contain an image.");
            }
        }
        #endregion

        #region btnLoadImage
        private void BtnLoadImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Only .bmp, .png and .tiff files are permitted
                openFileDialog.Filter = "Image files|*.bmp;*.png;*.tiff";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Load the image from the selected file
                    Image image = Image.FromFile(openFileDialog.FileName);
                    PictureBoxImageColor.Image = image;
                }
            }
        }
        #endregion        

        #region btColorPincers
        private void BtColorPincers_Click(object sender, EventArgs e)
        {
            _isPickingColor = !_isPickingColor; // Switch mode when button is clicked
        }
        #endregion

        #region PictureBoxImageColor_MouseClick
        private void PictureBoxImageColor_MouseClick(object sender, MouseEventArgs e)
        {
            if (_isPickingColor)
            {
                // Calculate the mouse position relative to the PictureBox
                Point mousePosition = new Point(e.X - PictureBoxImageColor.Left + 9, e.Y - PictureBoxImageColor.Top + 9);

                // Convert the mouse coordinates to image coordinates
                Point imagePoint = ConvertMouseToImageCoordinates(mousePosition);

                // Get the color of the pixel at the current position
                Color pixelColor = ((Bitmap)PictureBoxImageColor.Image).GetPixel(imagePoint.X, imagePoint.Y);

                // Update the panelColor and tbColorCode with the new color
                panelColor.BackColor = pixelColor;
                tbColorCode.Text = ColorTranslator.ToHtml(pixelColor);

                // Deactivate the color picking mode
                _isPickingColor = false;
            }
        }
        #endregion

        #region BtExchangeSelectiveColors
        private void BtExchangeSelectiveColors_Click(object sender, EventArgs e)
        {
            // Open a ColorDialog to select the new color
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    // Convert the color code in tbColorCode to a Color instance
                    Color oldColor = ColorTranslator.FromHtml(tbColorCode.Text);

                    // Call the method to exchange the colors
                    ExchangeColor(oldColor, colorDialog.Color);
                }
            }
        }
        #endregion

        #region ExchangeColor
        private void ExchangeColor(Color oldColor, Color newColor)
        {
            // Get the current image from the PictureBox
            Bitmap bmp = new Bitmap(PictureBoxImageColor.Image);

            // Create a GraphicsPath from the points
            GraphicsPath path = new GraphicsPath();
            if (_points.Count > 1)
            {
                path.AddLines(_points.ToArray());
            }
            else
            {
                // If no points were drawn, set the path to the entire image
                path.AddRectangle(new Rectangle(0, 0, bmp.Width, bmp.Height));
            }

            // Iterate through each pixel in the image (or in the selection, if one exists)
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    // Check if the pixel is within the selected area
                    if (path.IsVisible(x, y))
                    {
                        // If the pixel has the old color, change it to the new color
                        if (bmp.GetPixel(x, y) == oldColor)
                        {
                            bmp.SetPixel(x, y, newColor);
                        }
                    }
                }
            }

            // Update the image in the PictureBox
            PictureBoxImageColor.Image = bmp;
        }
        #endregion
    }
}
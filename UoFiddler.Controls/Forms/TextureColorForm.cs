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
        private int currentTextureId = 0; // Start with texture ID 0
        private Bitmap originalImage; // To store the original image
        private int savedTrackBarPosition; // To store the position of the track bar
        private List<Point> points = new List<Point>(); // Um den Pfad der Maus zu speichern
        private bool isSelecting = false; // To track whether the user is currently making a selection
        private Point imagePosition; // To save the position of the image
        private List<Point> savedPattern = null;


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
            currentTextureId = textureId;
        }
        #endregion

        #region UpdateImage
        public void UpdateImage()
        {
            // Check whether the texture exists before displaying it
            if (Textures.TestTexture(currentTextureId))
            {
                Image textureImage = Textures.GetTexture(currentTextureId);
                PictureBoxImageColor.Image = textureImage;
                originalImage = new Bitmap(textureImage); // Save a copy of the original image

                // Calculate the position of the image within the PictureBox
                int imageX = (PictureBoxImageColor.Width - textureImage.Width) / 2;
                int imageY = (PictureBoxImageColor.Height - textureImage.Height) / 2;

                // Store the position for later use
                // (you would need to add these fields to your class)
                this.imagePosition = new Point(imageX, imageY);
            }
        }
        #endregion

        #region buttonPrevious
        private void buttonPrevious_Click(object sender, EventArgs e)
        {
            // Decrease the texture ID but stop at 0
            do
            {
                if (currentTextureId > 0)
                {
                    currentTextureId--;
                }
            } while (!Textures.TestTexture(currentTextureId) && currentTextureId > 0);

            UpdateImage();
        }
        #endregion

        #region buttonNext
        private void buttonNext_Click(object sender, EventArgs e)
        {
            // Increase the texture ID and refresh the image
            do
            {
                currentTextureId++;
            } while (!Textures.TestTexture(currentTextureId) && currentTextureId < Textures.GetIdxLength());

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
        private void trackBarColor_MouseUp(object sender, MouseEventArgs e)
        {
            ChangeImageColor();
        }
        #endregion

        #region ChangeImageColor
        private void ChangeImageColor()
        {
            // Get the current value of the track bar
            int hueShift = trackBarColor.Value;

            // If the value is 0, reset the image to the original
            if (hueShift == 0)
            {
                PictureBoxImageColor.Image = new Bitmap(originalImage);
                return;
            }

            // Get the current image from the PictureBox
            Bitmap bmp = new Bitmap(PictureBoxImageColor.Image);

            // Create a GraphicsPath from the points
            GraphicsPath path = new GraphicsPath();
            if (points.Count > 1)
            {
                path.AddLines(points.ToArray());
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

        #region trackBarColor_Scroll
        private void trackBarColor_Scroll(object sender, EventArgs e)
        {
            // Get the current value of the track bar
            int colorShift = trackBarColor.Value;

            // Update the label with the current value
            labelColorShift.Text = $"Color shift: {colorShift}";
        }
        #endregion

        #region trackBarColor_KeyUp
        private void trackBarColor_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                ChangeImageColor();
            }
        }
        #endregion

        #region buttonSavePosition
        private void buttonSavePosition_Click(object sender, EventArgs e)
        {
            // Save the current position of the track bar
            savedTrackBarPosition = trackBarColor.Value;
        }
        #endregion

        #region buttonLoadPosition
        private void buttonLoadPosition_Click(object sender, EventArgs e)
        {
            // Set the position of the track bar to the saved value
            trackBarColor.Value = savedTrackBarPosition;

            // Update the label with the current value
            labelColorShift.Text = $"Color shift: {savedTrackBarPosition}";

            // Update the image color
            ChangeImageColor();
        }
        #endregion

        #region buttonCopyToClipboard
        private void buttonCopyToClipboard_Click(object sender, EventArgs e)
        {
            if (PictureBoxImageColor.Image != null)
            {
                Clipboard.SetImage(PictureBoxImageColor.Image);
            }
        }
        #endregion

        #region buttonSaveToFile_Click
        private void buttonSaveToFile_Click(object sender, EventArgs e)
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
            isSelecting = true;

            // Clear the list of points
            points.Clear();

            // Calculate the mouse position relative to the PictureBox
            Point mousePosition = new Point(e.X - PictureBoxImageColor.Left + 9, e.Y - PictureBoxImageColor.Top + 9);

            // Convert the mouse coordinates to image coordinates
            Point imagePoint = ConvertMouseToImageCoordinates(mousePosition);

            // Add the current point to the list
            points.Add(imagePoint);

            // Redraw the PictureBox
            PictureBoxImageColor.Invalidate();
        }
        #endregion

        #region PictureBoxImageColor_MouseMove
        private void PictureBoxImageColor_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                // Calculate the mouse position relative to the PictureBox
                Point mousePosition = new Point(e.X - PictureBoxImageColor.Left + 9, e.Y - PictureBoxImageColor.Top + 9);


                // Convert the mouse coordinates to image coordinates
                Point imagePoint = ConvertMouseToImageCoordinates(mousePosition);

                // Add the current point to the list
                points.Add(imagePoint);

                // Update the position of the cursor pixel
                this.CursorPixelPosition = e.Location;

                // Redraw the PictureBox
                PictureBoxImageColor.Invalidate();
            }
        }
        #endregion

        #region PictureBoxImageColor_MouseUp
        private void PictureBoxImageColor_MouseUp(object sender, MouseEventArgs e)
        {
            // Stop selecting
            isSelecting = false;
        }
        #endregion

        #region PictureBoxImageColor_Paint
        private void PictureBoxImageColor_Paint(object sender, PaintEventArgs e)
        {
            // Draw the path of the selection if there are enough points
            if (points.Count > 1)
            {
                // Create a new list of points adjusted for the image position
                List<Point> adjustedPoints = new List<Point>();
                foreach (Point point in points)
                {
                    adjustedPoints.Add(new Point(point.X + imagePosition.X, point.Y + imagePosition.Y));
                }

                // Draw the lines using the adjusted points
                e.Graphics.DrawLines(new Pen(this.CursorPixelColor), adjustedPoints.ToArray());

                // Get the position of the cursor relative to the PictureBox
                Point cursorPosition = PictureBoxImageColor.PointToClient(Cursor.Position);

                // Draw the cursor pixel at the current mouse position
                e.Graphics.FillRectangle(new SolidBrush(this.CursorPixelColor), cursorPosition.X, cursorPosition.Y, 1, 1);
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
        private void buttonChangeCursorColor_Click(object sender, EventArgs e)
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
            if (points.Count > 0)
            {
                savedPattern = new List<Point>(points);
            }
            // If there is no current pattern but there is a saved pattern, restore the saved pattern
            else if (savedPattern != null)
            {
                points = new List<Point>(savedPattern);
                PictureBoxImageColor.Invalidate();  // Redraw the image to see the restored pattern
            }
        }
        #endregion

        #region RestorePatternButton
        private void RestorePatternButton_Click(object sender, EventArgs e)
        {
            // Check whether a pattern has been saved
            if (savedPattern != null)
            {
                // Restore the saved pattern
                points = new List<Point>(savedPattern);

                // Redraw the image to see the restored pattern
                PictureBoxImageColor.Invalidate();
            }
        }
        #endregion

        #region btnClear_Click
        private void btnClear_Click(object sender, EventArgs e)
        {
            // Clear the list of points
            points.Clear();

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
    }
}
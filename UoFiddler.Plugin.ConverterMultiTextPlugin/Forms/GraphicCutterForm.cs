using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Microsoft.Win32;
using Ultima;
using System.Drawing.Drawing2D;
using System.Media;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class GraphicCutterForm : Form
    {
        private Bitmap image;
        private bool showGrid = true;

        private bool showBorder = true;

        private int imageX = 0;
        private int imageY = 0;

        private int moveX = 0;
        private int moveY = 0;

        //PictureBox2
        private Point startPoint;
        private Rectangle cropArea;
        private bool isDragging = false;
        private bool showGrid2 = false;
        private Rectangle selectedRectangle; // The selected range.

        //imageColorClearToolStripMenuItem
        private Bitmap originalImage;
        private bool isImageTransparent = false; // Variable to track the current state of the image.

        private Color gridColor = Color.Red; // Set the default grid color to red PictureBox1.

        private Bitmap transparentImage; // Transparent Image
        private bool playCustomSound = true; //Sound

        // Global variable to store the state of the mirrored image
        private bool isMirrored = false;

        private List<Point> points = new List<Point>();

        private bool shouldDraw = false; // The indicator

        public GraphicCutterForm()
        {
            InitializeComponent();

            // VAssociate the MouseMove event of PictureBox1 with the event handler.
            pictureBox1.MouseMove += pictureBox2_MouseMove;

            // Set the values in the text boxes.
            textBoxWidth.Text = "44";
            textBoxHeight.Text = "105";
            textBoxStartX.Text = "174";
            textBoxStartY.Text = "247";

            // Set KeyPreview to true to allow the form to receive keyboard input
            this.KeyPreview = true;

            // Link the KeyDown event of the form to the event handler
            this.KeyDown += Form1_KeyDown;

            // Call the CropImage method
            CropImage();

            // Link the Click event of the ToolStripMenuItem to the event handler
            saveImageToolStripMenuItem.Click += saveImageToolStripMenuItem_Click;

            // Set the default selected item
            toolStripComboBox1.SelectedIndex = 0;

            label1.Text = ""; // do not show text

            // Register the Paint event handler for the PictureBox2 control.
            pictureBox2.Paint += pictureBox3_Paint;

            // Associate the SelectedIndexChanged event handler with the SelectedIndexChanged event of toolStripComboBox2
            toolStripComboBox2.SelectedIndexChanged += toolStripComboBox2_SelectedIndexChanged;

        }

        #region keydown Move and Textbox
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Check which key was pressed
            switch (e.KeyCode)
            {
                case Keys.Up:
                    // Increase the value in the textBoxStartY
                    textBoxStartY.Text = (int.Parse(textBoxStartY.Text) + 1).ToString();
                    btnUp.PerformClick(); // Simulate a click on the btnUp button
                    break;
                case Keys.Down:
                    // Decrease the value in the textBoxStartY
                    textBoxStartY.Text = (int.Parse(textBoxStartY.Text) - 1).ToString();
                    btnDown.PerformClick(); // Simulate a click on the btnDown button
                    break;
                case Keys.Left:
                    // Decrease the value in the textBoxStartX
                    textBoxStartX.Text = (int.Parse(textBoxStartX.Text) - 1).ToString();
                    btnLeft.PerformClick(); // Simulate a click on the btnLeft button
                    break;
                case Keys.Right:
                    // Increase the value in the textBoxStartX
                    textBoxStartX.Text = (int.Parse(textBoxStartX.Text) + 1).ToString();
                    btnRight.PerformClick(); // Simulate a click on the btnRight button
                    break;
                case Keys.X:
                    if (e.Control)
                    {
                        // Copy the image from pictureBox2 to clipboard
                        copyToolStripMenuItem_Click(sender, e);
                    }
                    break;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                    btnUp.PerformClick();
                    break;
                case Keys.Down:
                    btnDown.PerformClick();
                    break;
                case Keys.Left:
                    btnLeft.PerformClick();
                    break;
                case Keys.Right:
                    btnRight.PerformClick();
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        //Disables character input.
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Check if the pressed key is a number.
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                // Discard the input.
                e.Handled = true;
            }
        }

        #region Opendialog        

        private void openImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    image = new Bitmap(openFileDialog.FileName);
                    originalImage = new Bitmap(image); // Save a copy of the original image.
                    image = new Bitmap(originalImage); // Start with the original image.

                    if (checkBoxTransparent.Checked)
                    {
                        // Make colors transparent when the checkbox is checked.
                        MakeColorTransparent(image, Color.White); // Make white color transparent.
                        MakeColorTransparent(image, Color.Black); // Make black color transparent.
                    }

                    pictureBox1.Image = image;
                    pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;

                    // Display the image in PictureBox2 as well.
                    pictureBox2.Image = image;
                    pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;

                    pictureBox1.Refresh(); // Display a new image.

                    // Set the position of the image in PictureBox1.
                    SetImagePosition(0, 0);

                    // Set the values in the text boxes.
                    textBoxWidth.Text = image.Width.ToString();
                    textBoxHeight.Text = image.Height.ToString();
                    textBoxStartX.Text = ((pictureBox1.Width - image.Width) / 2).ToString();
                    textBoxStartY.Text = (pictureBox1.Height - image.Height).ToString();

                    isImageTransparent = false; // Set the state of the image to non-transparent.
                }
            }
        }
        #endregion

        #region  Picture Paint
        //Paint Grid
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // Check if the grid should be displayed. Überprüfe, ob das Gitter angezeigt werden soll.
            if (showGrid)
            {
                int kachelBreite = 32; // Tile width.
                int gridSize = 8; // Number of tiles.
                float scaleFactor = 250f / (gridSize * kachelBreite);
                Pen pen = new Pen(gridColor, 1f); // Verwende die ausgewählte Farbe für den Stift.
                Brush brush = new SolidBrush(Color.Black);

                // Calculate grid size.
                int gridSizeInPixels = gridSize * kachelBreite;

                int dx = 0; // Move the graphics 10 pixels to the right.
                int dy = -178; // Move the graphics 20 pixels upwards.

                // Move the origin to the center of the PictureBox.
                e.Graphics.TranslateTransform(pictureBox1.Width / 2 + dx, pictureBox1.Height / 2 + dy);

                // Scale the graphics.
                e.Graphics.ScaleTransform(scaleFactor, scaleFactor);

                // Drehe die Grafik um 45 Grad.
                e.Graphics.RotateTransform(45);

                for (int i = gridSize; i >= 0; i--)
                {
                    // Draw horizontal lines.
                    e.Graphics.DrawLine(pen, new Point(0, i * kachelBreite), new Point(gridSize * kachelBreite, i * kachelBreite));

                    // Draw vertical lines.
                    e.Graphics.DrawLine(pen, new Point(i * kachelBreite, 0), new Point(i * kachelBreite, gridSize * kachelBreite));
                }
            }
        }

        private void toolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            // Create a new ColorDialog control.
            ColorDialog colorDialog = new ColorDialog();

            // Set the custom colors of the ColorDialog control.
            colorDialog.CustomColors = new int[] { Color.Yellow.ToArgb(), Color.Blue.ToArgb(), Color.Orange.ToArgb(), Color.Pink.ToArgb(), Color.White.ToArgb(), Color.DarkBlue.ToArgb(), Color.DarkRed.ToArgb(), Color.Brown.ToArgb() };

            // Show the ColorDialog control and check if the user clicked OK.
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                // Set the color of the grid to the color selected by the user.
                gridColor = colorDialog.Color;

                // Update the PictureBox control to redraw the grid.
                pictureBox1.Invalidate();
            }
        }
        #endregion

        #region Move
        private void MoveImage(int dx, int dy)
        {
            if (image != null)
            {
                Bitmap canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                using (Graphics g = Graphics.FromImage(canvas))
                {
                    g.Clear(Color.Transparent);
                    imageX += dx;
                    imageY += dy;

                    // Calculate the position to center the image
                    int x = (canvas.Width - image.Width) / 2 + imageX;
                    int y = canvas.Height - image.Height + imageY;

                    // Make sure the image stays within the bounds of the picture box
                    x = Math.Max(x, 0);
                    x = Math.Min(x, canvas.Width - image.Width);
                    y = Math.Max(y, 0);
                    y = Math.Min(y, canvas.Height - image.Height);

                    // Update the imageX and imageY values
                    imageX = x - (canvas.Width - image.Width) / 2;
                    imageY = y - (canvas.Height - image.Height);

                    g.DrawImage(image, x, y);

                    // Draw a border around the image if showBorder is true
                    if (showBorder)
                    {
                        using (Pen borderPen = new Pen(Color.Red))
                        {
                            g.DrawRectangle(borderPen, x, y, image.Width, image.Height);
                        }
                    }

                    // Update the values in the text boxes
                    textBoxStartX.Text = x.ToString();
                    textBoxStartY.Text = y.ToString();

                    // Update the coordinates in the label
                    label1.Text = "X: " + x + ", Y: " + y;
                }
                pictureBox1.Image = canvas;
                pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            }
        }
        private void btnUp_Click(object sender, EventArgs e)
        {
            // Move the image in the desired direction
            MoveImage(0, -1);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            // Move the image in the desired direction
            MoveImage(0, 1);
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            // Move the image in the desired direction
            MoveImage(-1, 0);
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            // Move the image in the desired direction
            MoveImage(1, 0);
        }

        private void moveTimer_Tick(object sender, EventArgs e)
        {
            // Move the image in the desired direction
            MoveImage(moveX, moveY);
        }

        private void btnUp_MouseDown(object sender, MouseEventArgs e)
        {
            // Set the direction of movement
            moveX = 0;
            moveY = -1;
            // Start the timer
            moveTimer.Start();
        }

        private void btnUp_MouseUp(object sender, MouseEventArgs e)
        {
            // Stop the timer
            moveTimer.Stop();
        }

        private void btnDown_MouseDown(object sender, MouseEventArgs e)
        {
            // Set the direction of movement
            moveX = 0;
            moveY = 1;
            // Start the timer
            moveTimer.Start();
        }

        private void btnDown_MouseUp(object sender, MouseEventArgs e)
        {
            // Stop the timer
            moveTimer.Stop();
        }

        private void btnLeft_MouseDown(object sender, MouseEventArgs e)
        {
            // Set the direction of movement
            moveX = -1;
            moveY = 0;
            // Start the timer
            moveTimer.Start();
        }

        private void btnLeft_MouseUp(object sender, MouseEventArgs e)
        {
            // Stop the timer
            moveTimer.Stop();
        }

        private void btnRight_MouseDown(object sender, MouseEventArgs e)
        {
            // Set the direction of movement
            moveX = 1;
            moveY = 0;
            // Start the timer
            moveTimer.Start();
        }

        private void btnRight_MouseUp(object sender, MouseEventArgs e)
        {
            // Stop the timer
            moveTimer.Stop();
        }
        #endregion

        #region Position

        private void SetImagePosition(int x, int y)
        {
            if (image != null)
            {
                Bitmap canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                using (Graphics g = Graphics.FromImage(canvas))
                {
                    g.Clear(Color.Transparent);
                    imageX = x;
                    imageY = y;
                    // Calculate the position to center the image
                    int xPos = (canvas.Width - image.Width) / 2 + imageX;
                    int yPos = canvas.Height - image.Height + imageY;
                    // Make sure the image stays within the bounds of the picture box
                    xPos = Math.Max(xPos, 0);
                    xPos = Math.Min(xPos, canvas.Width - image.Width);
                    yPos = Math.Max(yPos, 0);
                    yPos = Math.Min(yPos, canvas.Height - image.Height);
                    g.DrawImage(image, xPos, yPos);
                }
                pictureBox1.Image = canvas;
                pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            }
        }

        private void MoveImage()
        {
            // Code to move the image...

            // If the image is mirrored, apply the mirroring again after moving it
            if (isMirrored)
            {
                Bitmap movedImage = new Bitmap(pictureBox2.Image);
                movedImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                pictureBox2.Image = movedImage;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            // Increase the value in the textBoxStartY
            textBoxStartY.Text = (int.Parse(textBoxStartY.Text) + 1).ToString();

            // If the image is mirrored, apply the mirroring again after moving it
            if (isMirrored)
            {
                Bitmap movedImage = new Bitmap(pictureBox2.Image);
                movedImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                pictureBox2.Image = movedImage;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Decrease the value in the textBoxStartY
            textBoxStartY.Text = (int.Parse(textBoxStartY.Text) - 1).ToString();

            // If the image is mirrored, apply the mirroring again after moving it
            if (isMirrored)
            {
                Bitmap movedImage = new Bitmap(pictureBox2.Image);
                movedImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                pictureBox2.Image = movedImage;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Decrease the value in the textBoxStartX
            textBoxStartX.Text = (int.Parse(textBoxStartX.Text) - 1).ToString();

            // If the image is mirrored, apply the mirroring again after moving it
            if (isMirrored)
            {
                Bitmap movedImage = new Bitmap(pictureBox2.Image);
                movedImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                pictureBox2.Image = movedImage;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Increase the value in the textBoxStartX
            textBoxStartX.Text = (int.Parse(textBoxStartX.Text) + 1).ToString();

            // If the image is mirrored, apply the mirroring again after moving it
            if (isMirrored)
            {
                Bitmap movedImage = new Bitmap(pictureBox2.Image);
                movedImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                pictureBox2.Image = movedImage;
            }
        }
        #endregion

        #region Paint 
        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (checkBoxFreehand.Checked && e.Button == MouseButtons.Left)
            {
                points.Clear();
                points.Add(e.Location);
            }
            else if (e.Button == MouseButtons.Left)
            {
                startPoint = e.Location;
                isDragging = true;
                cropArea = new Rectangle(startPoint.X, startPoint.Y, 0, 0);
            }
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (checkBoxFreehand.Checked && e.Button == MouseButtons.Left)
            {
                points.Add(e.Location);
                pictureBox2.Invalidate();
            }
            else if (e.Button == MouseButtons.Left && isDragging)
            {
                int width = e.X - startPoint.X;
                int height = e.Y - startPoint.Y;
                cropArea = new Rectangle(startPoint.X, startPoint.Y, width, height);
                pictureBox2.Invalidate();
            }
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            if (checkBoxFreehand.Checked && e.Button == MouseButtons.Left)
            {
                points.Add(points[0]);  // Verbinden Sie das Ende mit dem Anfang
                pictureBox2.Invalidate();
            }
            else if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }
        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.Yellow))
            {
                pen.DashStyle = DashStyle.Dash;

                if (checkBoxCircle.Checked)
                {
                    // Draw a circle
                    e.Graphics.DrawEllipse(pen, cropArea);
                }
                else
                {
                    // Draw a rectangle
                    e.Graphics.DrawRectangle(pen, cropArea);
                }

                if (checkBoxFreehand.Checked && points.Count > 1)
                {
                    // Draw freehand
                    e.Graphics.DrawLines(Pens.Yellow, points.ToArray());
                }
            }
        }

        private void checkBoxCircle_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox2.Invalidate();
        }

        #endregion

        #region funktions Menu
        //Create a screenshot image.
        private void PaintGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Hide the grid.
            // showGrid = false;

            // Update the PictureBox1.
            pictureBox1.Invalidate();
            pictureBox1.Update();

            // Check if an image is loaded in the pictureBox1
            if (pictureBox1.Image != null)
            {
                // Create a screenshot of the current pictureBox1
                Bitmap screenshot = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                pictureBox1.DrawToBitmap(screenshot, new Rectangle(0, 0, screenshot.Width, screenshot.Height));

                // Set the screenshot as the image of the pictureBox1
                pictureBox1.Image = screenshot;
            }

            // Call the CropImage method
            CropImage();
        }

        // Mouse cursor.
        private void ChangeCursorToCross()
        {
            // Change the mouse cursor to a crosshair.
            pictureBox2.Cursor = Cursors.Cross;
        }

        // Color Black and White
        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check the current background color of the PictureBox
            if (pictureBox1.BackColor == Color.FromArgb(0, 0, 0))
            {
                // If the current background color is black, change it to white.
                pictureBox1.BackColor = Color.FromArgb(255, 255, 255);
            }
            else
            {
                // Otherwise, change the background color to black.
                pictureBox1.BackColor = Color.FromArgb(0, 0, 0);
            }
        }

        //Mirror
        private void spiegelnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (image != null)
            {
                // Mirror the image horizontally.
                image.RotateFlip(RotateFlipType.RotateNoneFlipX);

                // Set the mirrored image as the new image of the PictureBox.
                pictureBox1.Image = image;

                // Keep the current position of the image.
                SetImagePosition(imageX, imageY);

                // Update the image in the PictureBox.
                pictureBox1.Refresh();
            }
        }

        // Color
        private void toolStripTextBoxColors_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Enter key was pressed.
            if (e.KeyCode == Keys.Enter)
            {
                // Try to interpret the entered text as a color code.
                try
                {
                    // Remove the '#' from the entered text, if present.
                    string colorCode = toolStripTextBoxColors.Text.TrimStart('#');

                    // Check if the color code is 6 characters long.
                    if (colorCode.Length == 6)
                    {
                        // Convert the color code to a color.
                        int argb = Int32.Parse(colorCode, System.Globalization.NumberStyles.HexNumber);
                        Color color = Color.FromArgb(255, Color.FromArgb(argb));

                        // Set the background color of the PictureBox.
                        pictureBox1.BackColor = color;
                    }
                    else
                    {
                        // Display an error message if the entered text is not a valid color code.
                        MessageBox.Show("Please enter a valid 6-digit color code. Example: FF0000 for red.", "Invalid color code.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch
                {
                    // Display an error message if the text entered is not a valid color code
                    MessageBox.Show("Please enter a valid 6-digit color code. Example: FF0000 for red.", "Invalid color code.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Grid on and off
        private void toggleGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Toggle the grid on or off.
            showGrid = !showGrid;

            // Refresh the PictureBox1.
            pictureBox1.Invalidate();
        }
        private void unloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if an image is loaded
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image was loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // PictureBox leeren
            pictureBox1.Image = null;
            pictureBox1.Refresh();
        }
        #endregion

        // Imput 

        private void textBoxWidth_TextChanged(object sender, EventArgs e)
        {
            CropImage();
        }

        private void textBoxHeight_TextChanged(object sender, EventArgs e)
        {
            CropImage();
        }

        private void textBoxStartX_TextChanged(object sender, EventArgs e)
        {
            CropImage();
        }

        private void textBoxStartY_TextChanged(object sender, EventArgs e)
        {
            CropImage();
        }
        #region CromImage        

        private void CropImage()
        {
            // Check if an image is loaded in PictureBox1.
            if (pictureBox1.Image != null && !string.IsNullOrEmpty(textBoxWidth.Text) && !string.IsNullOrEmpty(textBoxHeight.Text) && !string.IsNullOrEmpty(textBoxStartX.Text) && !string.IsNullOrEmpty(textBoxStartY.Text))
            {
                // Load the image from PictureBox1 into PictureBox2.
                pictureBox2.Image = pictureBox1.Image;
                pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;

                // Read the width and height from the text boxes.
                int width = int.Parse(textBoxWidth.Text);
                int height = int.Parse(textBoxHeight.Text);

                // Check if the width and height are greater than 0.
                if (width > 0 && height > 0)
                {
                    // Read the starting point from the text boxes.
                    int startX = int.Parse(textBoxStartX.Text);
                    int startY = int.Parse(textBoxStartY.Text);

                    // Crop the image from PictureBox2 to the specified size.
                    Bitmap croppedImage = new Bitmap(width, height);
                    using (Graphics g = Graphics.FromImage(croppedImage))
                    {
                        g.DrawImage(pictureBox2.Image, new Rectangle(0, 0, width, height), new Rectangle(startX, startY, width, height), GraphicsUnit.Pixel);
                    }

                    // Load the cropped image into PictureBox2.
                    pictureBox2.Image = croppedImage;
                    pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;
                }
            }
        }

        #endregion
        // Show Border Image und die Sichtbarkeit des Rahmens zu aktualisieren.
        private void showBorderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showBorder = !showBorder;
            MoveImage(0, 0); // Redraw the image to update the border visibility
        }
        #region Save
        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if an image is loaded in the pictureBox2
            if (pictureBox2.Image != null)
            {
                // Get the selected image format from the toolStripComboBox1
                string format = toolStripComboBox1.SelectedItem.ToString();

                // Create the directory if it doesn't exist
                string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempGrafic");
                Directory.CreateDirectory(directory);

                // Create the file name with a timestamp
                string fileName = "ArtItem_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

                // Set the file extension based on the selected format
                switch (format)
                {
                    case "BMP":
                        fileName += ".bmp";
                        break;
                    case "TIFF":
                        fileName += ".tiff";
                        break;
                    case "PNG":
                        fileName += ".png";
                        break;
                    case "JPEG":
                        fileName += ".jpeg";
                        break;
                }

                // Combine the directory and file name
                string filePath = Path.Combine(directory, fileName);

                // Save the image in the selected format
                switch (format)
                {
                    case "BMP":
                        pictureBox2.Image.Save(filePath, ImageFormat.Bmp);
                        break;
                    case "TIFF":
                        pictureBox2.Image.Save(filePath, ImageFormat.Tiff);
                        break;
                    case "PNG":
                        pictureBox2.Image.Save(filePath, ImageFormat.Png);
                        break;
                    case "JPEG":
                        pictureBox2.Image.Save(filePath, ImageFormat.Jpeg);
                        break;
                }

                // Show a message box to indicate that the image was saved
                MessageBox.Show("The image was saved in: " + filePath);
            }
        }

        private void pictureBox2_MouseClick(object sender, EventArgs e)
        {
            // Cast EventArgs to MouseEventArgs
            var me = e as MouseEventArgs;
            if (me != null)
            {
                // Check if the right mouse button was clicked
                if (me.Button == MouseButtons.Right)
                {
                    // Show the contextMenuStrip1 at the current mouse position
                    contextMenuStrip1.Show(pictureBox2, me.Location);
                }
            }
        }
        #endregion

        #region Picturebox2
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Bitmap image = new Bitmap(openFileDialog.FileName);
                    pictureBox2.Image = image;
                    pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox2.Refresh();
                }
            }

        }

        private void openToolStrip2MenuItem_Click(object sender, EventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Bitmap image = new Bitmap(openFileDialog.FileName);
                    pictureBox2.Image = image;
                    // Change the size of the PictureBox2 to match the size of the image.
                    pictureBox2.Size = pictureBox2.Image.Size;
                    pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox2.Refresh();
                }
            }
        }

        private void buttonCrop_Click(object sender, EventArgs e)
        {
            // Check if an image is loaded in the pictureBox2
            if (pictureBox2.Image != null)
            {
                // Create the directory if it doesn't exist
                string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempGrafic");
                Directory.CreateDirectory(directory);

                // Create the file name with a timestamp
                string fileName = "ArtItem_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bmp";

                // Combine the directory and file name
                string filePath = Path.Combine(directory, fileName);

                // Save the image in BMP format
                pictureBox2.Image.Save(filePath, ImageFormat.Bmp);

                // Show a message box to indicate that the image was saved
                MessageBox.Show("The image was saved in: " + filePath);
            }
        }

        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Toggle the value of the showGrid field.
            showGrid2 = !showGrid2;

            // Redraw the PictureBox2.
            pictureBox2.Invalidate();
        }

        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            // Check if the grid should be displayed.
            if (showGrid2)
            {
                int kachelBreite = 32; // Tile width.
                int gridSize = 8; // Number of tiles.
                float scaleFactor = 250f / (gridSize * kachelBreite);
                //Pen pen = new Pen(Color.Red, 1f);
                Pen pen = new Pen(gridColor, 1f); // Verwende die ausgewählte Farbe für den Stift.
                Brush brush = new SolidBrush(Color.Black);

                // Calculate grid size.
                int gridSizeInPixels = gridSize * kachelBreite;

                int dx = 0; // Move the graphics 10 pixels to the right.
                int dy = -178; // Move the graphics 20 pixels upwards.

                // Move the origin to the center of the PictureBox.
                e.Graphics.TranslateTransform(pictureBox2.Width / 2 + dx, pictureBox2.Height / 2 + dy);

                // Scale the graphics.
                e.Graphics.ScaleTransform(scaleFactor, scaleFactor);

                // Rotate the graphics by 45 degrees.
                e.Graphics.RotateTransform(45);

                for (int i = gridSize; i >= 0; i--)
                {
                    // Draw horizontal lines.
                    e.Graphics.DrawLine(pen, new Point(0, i * kachelBreite), new Point(gridSize * kachelBreite, i * kachelBreite));

                    // Draw vertical lines.
                    e.Graphics.DrawLine(pen, new Point(i * kachelBreite, 0), new Point(i * kachelBreite, gridSize * kachelBreite));
                }
            }
        }

        //Grid Color PictureBox2
        private void toolStripComboBox3_Click(object sender, EventArgs e)
        {
            // Create a new ColorDialog control.
            ColorDialog colorDialog = new ColorDialog();

            // Set the custom colors of the ColorDialog control.
            colorDialog.CustomColors = new int[] { Color.Yellow.ToArgb(), Color.Blue.ToArgb(), Color.Orange.ToArgb(), Color.Pink.ToArgb(), Color.White.ToArgb(), Color.DarkBlue.ToArgb(), Color.DarkRed.ToArgb(), Color.Brown.ToArgb() };

            // Show the ColorDialog control and check if the user clicked OK.
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                // Set the color of the grid to the color selected by the user.
                gridColor = colorDialog.Color;

                // Update the PictureBox control to redraw the grid.
                pictureBox2.Invalidate();
            }
        }

        // Mirror
        private void mirror2ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            // Check if there is an image in PictureBox2.
            if (pictureBox2.Image != null)
            {
                // Create a copy of the current image.
                Bitmap originalImage = new Bitmap(pictureBox2.Image);

                // Mirror the image horizontally.
                originalImage.RotateFlip(RotateFlipType.RotateNoneFlipX);

                // Set the mirrored state to true
                isMirrored = true;

                // Reset the mirrored image back to PictureBox2.
                pictureBox2.Image = originalImage;
            }
        }

        //Rotate 90 Degress
        private void toolStripMenuItemrotate90degrees_Click(object sender, EventArgs e)
        {
            // Check if there is an image in PictureBox2.
            if (pictureBox2.Image != null)
            {
                // Create a copy of the current image.
                Bitmap originalImage = new Bitmap(pictureBox2.Image);

                // Rotate the image 90 degrees to the left.
                originalImage.RotateFlip(RotateFlipType.Rotate270FlipNone);

                // Reset the rotated image back to PictureBox2.
                pictureBox2.Image = originalImage;
            }
        }

        //Cut Image
        private void cutImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if an image is selected.
            if (pictureBox2.Image == null)
            {
                MessageBox.Show("No image has been selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Bitmap originalImage = new Bitmap(pictureBox2.Image);
            int width = originalImage.Width;
            int height = originalImage.Height;
            int count = (int)Math.Ceiling((double)width / 44);
            int lastImageWidth = width % 44;
            Color fillColor = Color.White; // Example: White color. (FFFFFF)

            // Check if the image is narrower than 44 pixels.
            if (width < 44)
            {
                MessageBox.Show("The image is narrower than 44 pixels.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Create the directory if it doesn't exist.
            string directory = "tempGrafic";
            Directory.CreateDirectory(directory);

            // Crop and save the sub-images.
            for (int i = 0; i < count; i++)
            {
                int cropWidth = (i == count - 1 && lastImageWidth > 0) ? lastImageWidth : 44;
                int offsetX = i * 44;

                // Create a crop
                Rectangle cropRect = new Rectangle(offsetX, 0, cropWidth, height);
                Bitmap croppedImage = originalImage.Clone(cropRect, originalImage.PixelFormat);

                // Check if the last image is being cropped.
                if (i == count - 1 && lastImageWidth < 44)
                {
                    // Fill the missing area with the specified color.
                    int missingWidth = 44 - lastImageWidth;
                    Bitmap lastImage = new Bitmap(44, height);
                    using (Graphics g = Graphics.FromImage(lastImage))
                    {
                        // Draw a transparent background.
                        g.Clear(Color.Transparent);

                        // Place the last image on the left edge.
                        int x = 0;
                        g.DrawImage(croppedImage, x, 0);

                        // Fill the missing area with the specified color.
                        FillMissingArea(lastImage, missingWidth, height, fillColor);
                    }
                    croppedImage = lastImage;
                }

                // Fill the content.
                FillContent(croppedImage, fillColor);

                // Generate a file name
                string fileName = Path.Combine(directory, "art" + (i + 1) + ".bmp");

                // Save the image
                croppedImage.Save(fileName, ImageFormat.Bmp);

                // Release of resources.
                croppedImage.Dispose();
            }

            // Display a success message.
            MessageBox.Show("The image has been successfully cropped and saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Fill the missing area with the specified color.
        private void FillMissingArea(Bitmap image, int width, int height, Color color)
        {
            using (Graphics g = Graphics.FromImage(image))
            {
                Rectangle missingAreaRect = new Rectangle(image.Width - width, 0, width, height);
                using (Brush brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, missingAreaRect);
                }
            }
        }

        // Fill the image content with the specified color.
        private void FillContent(Bitmap image, Color color)
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if (image.GetPixel(x, y) == Color.Transparent || image.GetPixel(x, y).A == 0)
                    {
                        image.SetPixel(x, y, color);
                    }
                }
            }
        }

        private void fillTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null)
            {
                MessageBox.Show("No image has been loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if ((cropArea.Width <= 0 || cropArea.Height <= 0) && !checkBoxFreehand.Checked)
            {
                MessageBox.Show("No area has been selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Filter = "Bilddateien|*.bmp;*.jpg;*.jpeg;*.png";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Bitmap newTexture = new Bitmap(openFileDialog.FileName);
                    Bitmap imageCopy = (Bitmap)pictureBox2.Image.Clone();

                    using (Graphics g = Graphics.FromImage(imageCopy))
                    {
                        // Adjust cropArea and points based on zoom level
                        Rectangle adjustedCropArea = new Rectangle(cropArea.X / (int)Math.Pow(2, zoomCounter), cropArea.Y / (int)Math.Pow(2, zoomCounter), cropArea.Width / (int)Math.Pow(2, zoomCounter), cropArea.Height / (int)Math.Pow(2, zoomCounter));
                        List<Point> adjustedPoints = points.Select(p => new Point(p.X / (int)Math.Pow(2, zoomCounter), p.Y / (int)Math.Pow(2, zoomCounter))).ToList();

                        if (checkBoxCircle.Checked)
                        {
                            // Create a mask for the circle area
                            using (Bitmap mask = new Bitmap(adjustedCropArea.Width, adjustedCropArea.Height))
                            {
                                using (Graphics maskGraphics = Graphics.FromImage(mask))
                                {
                                    maskGraphics.Clear(Color.White);
                                    maskGraphics.FillEllipse(Brushes.Black, 0, 0, adjustedCropArea.Width, adjustedCropArea.Height);
                                }

                                // Skaliere die Textur auf die Größe der Maske
                                newTexture = new Bitmap(newTexture, mask.Size);

                                // Wende die Maske auf das neue Texturbild an
                                for (int x = 0; x < mask.Width; x++)
                                {
                                    for (int y = 0; y < mask.Height; y++)
                                    {
                                        if (mask.GetPixel(x, y).R != 0)
                                        {
                                            newTexture.SetPixel(x, y, Color.Transparent);
                                        }
                                    }
                                }
                            }

                            // Draw the texture on the image inside the circle
                            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                            g.DrawImage(newTexture, adjustedCropArea);
                        }
                        else if (checkBoxFreehand.Checked && adjustedPoints.Count > 1)
                        {
                            // Create a mask for the freehand area
                            using (Bitmap mask = new Bitmap(imageCopy.Width, imageCopy.Height))
                            {
                                using (Graphics maskGraphics = Graphics.FromImage(mask))
                                {
                                    maskGraphics.Clear(Color.White);
                                    maskGraphics.FillPolygon(Brushes.Black, adjustedPoints.ToArray());
                                }

                                // Scale the texture to the size of the mask
                                newTexture = new Bitmap(newTexture, mask.Size);

                                // Apply the mask to the new texture image
                                for (int x = 0; x < mask.Width; x++)
                                {
                                    for (int y = 0; y < mask.Height; y++)
                                    {
                                        if (mask.GetPixel(x, y).R != 0)
                                        {
                                            newTexture.SetPixel(x, y, Color.Transparent);
                                        }
                                    }
                                }
                            }

                            // Draw the texture onto the image inside the freehand shape
                            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                            g.DrawImage(newTexture, new Rectangle(0, 0, imageCopy.Width, imageCopy.Height));
                        }
                        else
                        {
                            // Draw the texture on the image
                            g.DrawImage(newTexture, adjustedCropArea);
                        }
                    }

                    pictureBox2.Image = imageCopy;
                    pictureBox2.Refresh();
                }
            }
        }
        private void unloadimageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if an image is loaded.
            if (pictureBox2.Image == null)
            {
                MessageBox.Show("No image has been loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Clear the PictureBox.
            pictureBox2.Image = null;
            pictureBox2.Refresh();
        }
        #endregion

        #region Background Image PictureBox1
        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripComboBox2.SelectedItem != null)
            {
                string selectedOption = toolStripComboBox2.SelectedItem.ToString();

                if (selectedOption == "green")
                {
                    pictureBox1.BackgroundImage = Properties.Resources.green; // Change the background image to "water".
                }
                if (selectedOption == "water")
                {
                    pictureBox1.BackgroundImage = Properties.Resources.water; // Change background image water
                }
                else if (selectedOption == "clear")
                {
                    pictureBox1.BackgroundImage = null; // Delete the background image.
                }
            }
        }

        private void ReplaceColors(Bitmap image, Color replacementColor, int threshold, params Color[] colorsToReplace)
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    foreach (Color colorToReplace in colorsToReplace)
                    {
                        int rDiff = Math.Abs(pixelColor.R - colorToReplace.R);
                        int gDiff = Math.Abs(pixelColor.G - colorToReplace.G);
                        int bDiff = Math.Abs(pixelColor.B - colorToReplace.B);
                        if (rDiff <= threshold && gDiff <= threshold && bDiff <= threshold)
                        {
                            image.SetPixel(x, y, replacementColor);
                            break;
                        }
                    }
                }
            }
        }

        private void imageColorClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (image != null)
            {
                if (isImageTransparent)
                {
                    image = new Bitmap(originalImage); // Restore the original image.
                    pictureBox1.Image = image;
                    pictureBox1.Refresh();
                    SetImagePosition(0, 0); // Set the position of the image.
                    isImageTransparent = false; // Set the state of the image to non-transparent.

                    pictureBox2.Image = new Bitmap(originalImage); // Restore the original image in pictureBox2.
                }
                else
                {
                    // Convert the loaded image to a PNG image.
                    using (MemoryStream stream = new MemoryStream())
                    {
                        image.Save(stream, ImageFormat.Png);
                        stream.Position = 0;
                        image = new Bitmap(stream);
                    }

                    int threshold = 10; // Colors within this threshold will be replaced.
                    ReplaceColors(image, Color.Transparent, threshold, Color.FromArgb(255, 255, 255), Color.FromArgb(0, 0, 0));
                    Bitmap croppedImage = CropImage2(image);
                    pictureBox1.Image = croppedImage;
                    pictureBox1.Refresh();
                    SetImagePosition(0, 0); // Set the position of the image.
                    isImageTransparent = true; // Set the state of the image to transparent.

                    pictureBox2.Image = new Bitmap(croppedImage); // Display the cropped image in pictureBox2.
                }
            }
        }
        private Bitmap CropImage2(Bitmap image)
        {
            int topmost = 0;
            int bottommost = image.Height;
            int leftmost = 0;
            int rightmost = image.Width;

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    if (pixelColor.A != 0)
                    {
                        if (x < leftmost) leftmost = x;
                        if (x > rightmost) rightmost = x;
                        if (y < topmost) topmost = y;
                        if (y > bottommost) bottommost = y;
                    }
                }
            }

            int width = rightmost - leftmost;
            int height = bottommost - topmost;

            return new Bitmap(image).Clone(new Rectangle(leftmost, topmost, width, height), image.PixelFormat);
        }
        #endregion

        #region Transparent and Checkbox
        private void checkBoxTransparent_CheckedChanged(object sender, EventArgs e)
        {
            if (originalImage != null) // Ensure an image has been loaded
            {
                if (checkBoxTransparent.Checked)
                {
                    // Make colors transparent when the checkbox is checked.
                    image = new Bitmap(originalImage); // Start with a copy of the original image.
                    MakeColorTransparent(image, Color.White); // Make white color transparent.
                    MakeColorTransparent(image, Color.Black); // Make black color transparent.
                }
                else
                {
                    // Restore the original image when the checkbox is unchecked.
                    image = new Bitmap(originalImage);
                }

                // Display the modified image in PictureBox.
                pictureBox1.Image = image;
                pictureBox1.Refresh(); // Refresh the PictureBox to show the new image.
            }
        }

        private void MakeColorTransparent(Bitmap image, Color color)
        {
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color pixelColor = image.GetPixel(i, j);
                    if (pixelColor.R == color.R && pixelColor.G == color.G && pixelColor.B == color.B)
                    {
                        image.SetPixel(i, j, Color.Transparent);
                    }
                }
            }
        }
        #endregion
        #region Clipbord Import Image
        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                originalImage = new Bitmap(Clipboard.GetImage()); // Save a copy of the original image.
                image = new Bitmap(originalImage); // Start with the original image.

                if (checkBoxTransparent.Checked)
                {
                    // Make colors transparent when the checkbox is checked.
                    MakeColorTransparent(image, Color.White); // Make white color transparent.
                    MakeColorTransparent(image, Color.Black); // Make black color transparent.
                }

                pictureBox1.Image = image;
                pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;

                // Display the image in PictureBox2 as well.
                pictureBox2.Image = image;
                pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;

                // Set the position of the image in PictureBox1.
                SetImagePosition(0, 0);

                // Set the values in the text boxes.
                textBoxWidth.Text = image.Width.ToString();
                textBoxHeight.Text = image.Height.ToString();
                textBoxStartX.Text = ((pictureBox1.Width - image.Width) / 2).ToString();
                textBoxStartY.Text = (pictureBox1.Height - image.Height).ToString();

                isImageTransparent = false; // Set the state of the image to non-transparent.

                pictureBox1.Invalidate(); // Redraw the grid and the image.
            }
        }
        #endregion

        #region Copy Clipbord
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                Clipboard.SetImage(pictureBox2.Image);

                // Check if the sound should be played
                if (playCustomSound)
                {
                    SoundPlayer player = new SoundPlayer();
                    player.SoundLocation = "sound.wav";
                    player.Play();
                }
            }
        }
        #endregion
        #region Reset Button
        private void resetButton_Click(object sender, EventArgs e)
        {
            // Reset the SizeMode property to Normal
            pictureBox2.SizeMode = PictureBoxSizeMode.Normal;

            // Reset the size of the PictureBox to its original size
            pictureBox2.Width /= (int)Math.Pow(2, zoomCounter);
            pictureBox2.Height /= (int)Math.Pow(2, zoomCounter);

            // Reset the zoom counter
            zoomCounter = 0;

            // Clear the list of points
            points.Clear();

            // Reset the cropArea
            cropArea = new Rectangle();

            // Update the PictureBox to reflect the changes
            pictureBox2.Invalidate();
        }
        #endregion

        #region Zoom Button
        private int zoomCounter = 0;
        private void zoomButton_Click(object sender, EventArgs e)
        {
            // Check if the zoom has already been applied twice
            if (zoomCounter >= 2)
            {
                return;
            }

            // Set the SizeMode property to Zoom
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;

            // Resize the PictureBox to increase the zoom effect
            pictureBox2.Width *= 2;
            pictureBox2.Height *= 2;

            // Increase the zoom counter
            zoomCounter++;
        }
        #endregion

        #region Show Mask
        // Add these lines to the beginning of your class
        private Bitmap originalImageMask; //Show Mask
        private Bitmap originalImageBeforeMask; // Show Mask bevor
        private bool isMaskDisplayed = false;

        private void showMaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if pictureBox2.Image is not null
            if (pictureBox2.Image != null)
            {
                // Save the original image in originalImageBeforeMask
                originalImageBeforeMask = new Bitmap(pictureBox2.Image);

                // Copy the original image to originalImageMask
                originalImageMask = new Bitmap(originalImageBeforeMask);

                // Load the image from resources
                var maskImage = Properties.Resources.showmask;

                // Create a new bitmap with the same size as pictureBox2
                var bitmap = new Bitmap(pictureBox2.Width, pictureBox2.Height);

                // Create a Graphics object from the bitmap
                using (var g = Graphics.FromImage(bitmap))
                {
                    // Draw the maskImage onto the bitmap, keeping the mask's original size
                    g.DrawImage(maskImage, new Rectangle(0, 0, maskImage.Width, maskImage.Height));
                }

                // Loop through every pixel in the image
                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        // Get the color of the pixel
                        var color = bitmap.GetPixel(x, y);

                        // If the color is black, make it transparent
                        if (color.R == 0 && color.G == 0 && color.B == 0)
                            bitmap.SetPixel(x, y, Color.Transparent);
                    }
                }

                // Combine the original image and mask
                using (var g = Graphics.FromImage(originalImageMask))
                {
                    g.DrawImage(bitmap, 0, 0);
                }

                // Set the image of the pictureBox2 to the combined image
                pictureBox2.Image = originalImageMask;

                isMaskDisplayed = true;
            }
        }
        #endregion
        #region Discharged Mask
        private void dischargedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isMaskDisplayed)
            {
                // Reset the image of pictureBox2 to the original image
                pictureBox2.Image = originalImageBeforeMask;
                isMaskDisplayed = false;

                // Check if pictureBox2.Image is set correctly
                if (pictureBox2.Image != originalImageBeforeMask)
                {
                    // Paste code here to troubleshoot or log the issue
                    Console.WriteLine("pictureBox2.Image was not correctly set to originalImageBeforeMask.");
                }
            }
        }
        #endregion
        #region Checkbox Size 44x123
        private void checkBoxOrgSize_CheckedChanged(object sender, EventArgs e)
        {
            // Check that the checkbox is selected
            if (checkBoxOrgSize.Checked)
            {
                // Update the values ​​of the text boxes
                textBoxWidth.Text = "44";
                textBoxHeight.Text = "123";
            }
        }
        #endregion

        #region Copy 
        private void CopyHighlightedAreaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if checkBoxFreehand is enabled
            if (checkBoxFreehand.Checked)
            {
                // Call the FreehandCheckCopy_Click method
                FreehandCheckCopy_Click(sender, e);
            }
            else
            {

                // Verify that an area is selected
                if (pictureBox2.Image != null && (cropArea.Width > 0 && cropArea.Height > 0))
                {
                    // Create a new Bitmap object with the dimensions of the selected area
                    Bitmap bmp = new Bitmap(cropArea.Width, cropArea.Height);

                    // Convert the image to a bitmap
                    Bitmap imageBitmap = new Bitmap(pictureBox2.Image);

                    // Create a new Graphics object from the bitmap
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        // Check whether the checkBoxCircle is activated
                        if (checkBoxCircle.Checked)
                        {
                            // Create a mask for the circle area
                            using (Bitmap mask = new Bitmap(cropArea.Width, cropArea.Height))
                            {
                                using (Graphics maskGraphics = Graphics.FromImage(mask))
                                {
                                    maskGraphics.Clear(Color.White);
                                    maskGraphics.FillEllipse(Brushes.Black, 0, 0, cropArea.Width, cropArea.Height);
                                }

                                // Apply the mask to the image and copy only the circle area
                                for (int x = 0; x < mask.Width; x++)
                                {
                                    for (int y = 0; y < mask.Height; y++)
                                    {
                                        if (mask.GetPixel(x, y).R == 0)
                                        {
                                            bmp.SetPixel(x, y, imageBitmap.GetPixel(x + cropArea.X, y + cropArea.Y));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Copy the selected area into the Graphics object
                            g.DrawImage(pictureBox2.Image, new Rectangle(0, 0, cropArea.Width, cropArea.Height), cropArea,
                                        GraphicsUnit.Pixel);
                        }
                    }

                    // Copy the image to the clipboard
                    Clipboard.SetImage(bmp);
                }
            }
        }

        private void FreehandCheckCopy_Click(object sender, EventArgs e)
        {
            // Verify that an area is selected
            if (pictureBox2.Image != null)
            {
                // Set the cropping area to the entire image
                Rectangle cropArea = new Rectangle(0, 0, pictureBox2.Image.Width, pictureBox2.Image.Height);

                if (cropArea.Width > 0 && cropArea.Height > 0)
                {
                    // Create a new Bitmap object with the dimensions of the selected area
                    Bitmap bmp = new Bitmap(cropArea.Width, cropArea.Height);

                    // Create a Graphics object from the new bitmap
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        // Set the background color to Transparent
                        g.Clear(Color.Transparent);

                        if (checkBoxFreehand.Checked && points.Count > 1)
                        {
                            // Create a GraphicsPath from the freehand points
                            GraphicsPath path = new GraphicsPath();
                            path.AddPolygon(points.ToArray());

                            // Draw the freehand area directly onto the new bitmap
                            g.SetClip(path);
                            g.DrawImage(((Bitmap)pictureBox2.Image), new Rectangle(0, 0, cropArea.Width, cropArea.Height), cropArea, GraphicsUnit.Pixel);
                        }
                    }

                    // Find the bounding rectangle of the non-transparent area
                    int minX = bmp.Width, minY = bmp.Height, maxX = 0, maxY = 0;
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        for (int y = 0; y < bmp.Height; y++)
                        {
                            if (bmp.GetPixel(x, y).A != 0)
                            {
                                minX = Math.Min(minX, x);
                                minY = Math.Min(minY, y);
                                maxX = Math.Max(maxX, x);
                                maxY = Math.Max(maxY, y);
                            }
                        }
                    }

                    // Create a new Bitmap object with the dimensions of the non-transparent area
                    Bitmap croppedBmp = new Bitmap(maxX - minX + 1, maxY - minY + 1);

                    // Copy the non-transparent area into the new bitmap object
                    using (Graphics g = Graphics.FromImage(croppedBmp))
                    {
                        g.DrawImage(bmp, new Rectangle(0, 0, croppedBmp.Width, croppedBmp.Height), new Rectangle(minX, minY, croppedBmp.Width, croppedBmp.Height), GraphicsUnit.Pixel);
                    }

                    // Copy the image to the clipboard
                    Clipboard.SetImage(croppedBmp);
                }
            }
        }
        #endregion

        #region Checkbox Only
        private void UpdateCheckBoxes(object sender, EventArgs e)
        {
            CheckBox changedCheckBox = sender as CheckBox;
            if (changedCheckBox != null)
            {
                if (changedCheckBox == checkBoxCircle && checkBoxCircle.Checked)
                {
                    checkBoxFreehand.Checked = false;
                }
                else if (changedCheckBox == checkBoxFreehand && checkBoxFreehand.Checked)
                {
                    checkBoxCircle.Checked = false;
                }
            }
        }
        #endregion

        #region Ruler
        private void customsRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            shouldDraw = !shouldDraw; // Switching the indicator when the menu item is clicked
            panel4.Invalidate(); // This triggers the Paint event
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {
            if (shouldDraw) // Check the indicator
            {
                var g = e.Graphics; // Use the Graphics object provided by the Paint event
                DrawMeasurements(g);
            }
            else
            {
                e.Graphics.Clear(panel4.BackColor); // Delete the drawing if shouldDraw is false
            }
        }
        private void DrawMeasurements(Graphics g)
        {
            // Height and width of the PictureBox object
            var h = pictureBox1.Height;
            var w = pictureBox1.Width;

            // Offset in millimeters
            var offset = 3;

            // Create a new pen with the color red
            using (var pen = new Pen(Color.Red))
            {
                // Create a new font with a smaller size
                using (var smallFont = new Font(this.Font.FontFamily, 9)) // Sets the font size to 10
                {
                    // Draw horizontal lines and measurements
                    for (int i = 0; i <= h; i += 10)
                    {
                        // Draw a line
                        g.DrawLine(pen, pictureBox1.Left - offset, pictureBox1.Bottom - i, pictureBox1.Left - offset - 5, pictureBox1.Bottom - i);

                        // Draw the measurement using the smaller font
                        g.DrawString(i.ToString(), smallFont, Brushes.Red, pictureBox1.Left - offset - 5 - g.MeasureString(i.ToString(), smallFont).Width, pictureBox1.Bottom - i - g.MeasureString(i.ToString(), smallFont).Height / 2);
                    }

                    // Draw vertical lines and measurements
                    for (int i = 0; i <= w; i += 10)
                    {
                        // Draw a line
                        g.DrawLine(pen, pictureBox1.Left + i, pictureBox1.Bottom + offset, pictureBox1.Left + i, pictureBox1.Bottom + offset + 5);

                        // Create a new StringFormat object for vertical alignment
                        var sf = new StringFormat();
                        sf.FormatFlags = StringFormatFlags.DirectionVertical;
                        sf.Alignment = StringAlignment.Far;

                        float yPosition = pictureBox1.Bottom + offset + 5 + 3; //Move the width to the right by 3 pixels

                        // Calculate the width of the string with the smaller font
                        float stringWidth = g.MeasureString(" " + i.ToString(), smallFont).Width;

                        // Check if the number is greater than or equal to 100
                        if (i >= 100)
                        {
                            // Add an additional adjustment to change the position of the number
                            stringWidth += g.MeasureString(" ", smallFont).Width;

                            // Add an additional 3 pixels of padding
                            stringWidth += 3;
                        }

                        // Draw the measurement with a space before the number just after the hyphen in the smaller font
                        g.DrawString(" " + i.ToString(), smallFont, Brushes.Red, pictureBox1.Left + i - (stringWidth / 2), yPosition + g.MeasureString(" " + i.ToString(), smallFont).Height * 2, sf);

                    }
                }
            }
        }
        #endregion
    }
}
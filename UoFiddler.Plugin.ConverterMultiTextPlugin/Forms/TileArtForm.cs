// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
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
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class TileArtForm : Form
    {
        public TileArtForm()
        {
            InitializeComponent();

            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(TileArtForm_KeyDown);

            hScrollBar1.Minimum = -pictureBoxTileArt3.Width / 2;
            hScrollBar1.Maximum = pictureBoxTileArt3.Width / 2;
        }

        #region pictureBoxTileArt_Paint
        private void pictureBoxTileArt_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Define the size of the route
            int routeSize = 44;

            // Define the number of routes in the x and y directions
            int routesX = 3;
            int routesY = 3;

            // Move the origin to the center of the PictureBox
            g.TranslateTransform(pictureBoxTileArt.Width / 2, pictureBoxTileArt.Height / 2);

            // Draw the routes
            for (int i = 0; i < routesX; i++)
            {
                for (int j = 0; j < routesY; j++)
                {
                    // Calculate the position of the route
                    int x = (int)((i - j) * routeSize / 2f) - routeSize;
                    int y = (int)((i + j) * routeSize / 2f) - routeSize;

                    // Create a new polygon for the diamond
                    Point[] diamond = new Point[]
                    {
                    new Point(x + routeSize / 2, y),
                    new Point(x + routeSize, y + routeSize / 2),
                    new Point(x + routeSize / 2, y + routeSize),
                    new Point(x, y + routeSize / 2)
                    };

                    // Draw the diamond
                    g.DrawPolygon(Pens.Black, diamond);

                    // Once the image has loaded, draw it onto the diamond
                    if (images[0] != null)
                    {
                        g.DrawImage(images[0], x, y, routeSize, routeSize);
                    }
                }
            }
        }
        #endregion

        #region MakeTransparent
        private Image MakeTransparent(Image image, Color color)
        {
            Bitmap bitmap = new Bitmap(image);

            bitmap.MakeTransparent(color);

            return bitmap;
        }
        #endregion

        #region btloadArt0All
        // Global variable to store the images
        private Image[] images = new Image[9];
        private void btloadArt0All_Click(object sender, EventArgs e)
        {
            // Create an OpenFileDialog object
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set the properties of the OpenFileDialog
            openFileDialog.Filter = "Pictures|*.jpg;*.jpeg;*.png;*.bmp";
            openFileDialog.Multiselect = false;

            // Display the dialog box and verify that the user clicked OK
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Load the image
                Image image = Image.FromFile(openFileDialog.FileName);

                // Make the colors #000000 and #FFFFFF transparent
                image = MakeTransparent(image, Color.Black);
                image = MakeTransparent(image, Color.White);

                // Save the image in the images array
                images[0] = image;

                // Redraw the PictureBox
                pictureBoxTileArt.Invalidate();
            }
        }
        #endregion

        #region pictureBoxTileArt_Paint2
        private void pictureBoxTileArt_Paint2(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Define the size of the route
            int routeSize = 44;

            // Define the number of routes in the x and y directions
            int routesX = 3;
            int routesY = 3;

            // Move the origin to the center of the PictureBox
            g.TranslateTransform(pictureBoxTileArt2.Width / 2, pictureBoxTileArt2.Height / 2);

            // Draw the routes
            for (int i = 0; i < routesX; i++)
            {
                for (int j = 0; j < routesY; j++)
                {
                    // Calculate the position of the route
                    int x = (int)((i - j) * routeSize / 2f) - routeSize;
                    int y = (int)((i + j) * routeSize / 2f) - routeSize;

                    // Create a new polygon for the diamond
                    Point[] diamond = new Point[]
                    {
                        new Point(x + routeSize / 2, y),
                        new Point(x + routeSize, y + routeSize / 2),
                        new Point(x + routeSize / 2, y + routeSize),
                        new Point(x, y + routeSize / 2)
                    };

                    // Draw the diamond
                    g.DrawPolygon(Pens.Black, diamond);

                    // Once the image has loaded, draw it onto the diamond
                    int imageIndex = i * routesY + j; // Calculate the index of the image based on the coordinates of the tile
                    if (imageIndex < images.Length && images[imageIndex] != null)
                    {
                        g.DrawImage(images[imageIndex], x, y, routeSize, routeSize);
                    }
                }
            }
        }
        #endregion

        #region btloadArt
        private void btloadArt01_Click(object sender, EventArgs e)
        {
            LoadImage(0);
        }

        private void btloadArt02_Click(object sender, EventArgs e)
        {
            LoadImage(1);
        }

        private void btloadArt03_Click(object sender, EventArgs e)
        {
            LoadImage(2);
        }

        private void btloadArt04_Click(object sender, EventArgs e)
        {
            LoadImage(3);
        }

        private void btloadArt05_Click(object sender, EventArgs e)
        {
            LoadImage(4);
        }

        private void btloadArt06_Click(object sender, EventArgs e)
        {
            LoadImage(5);
        }

        private void btloadArt07_Click(object sender, EventArgs e)
        {
            LoadImage(6);
        }

        private void btloadArt08_Click(object sender, EventArgs e)
        {
            LoadImage(7);
        }

        private void btloadArt09_Click(object sender, EventArgs e)
        {
            LoadImage(8);
        }
        #endregion

        #region LoadImage
        private void LoadImage(int index)
        {
            Image image;

            // Check whether the checkBoxClipboard is activated
            if (checkBoxClipboard.Checked)
            {
                // Load the image from the clipboard
                if (Clipboard.ContainsImage())
                {
                    image = Clipboard.GetImage();
                }
                else
                {
                    MessageBox.Show("The clipboard does not contain an image.");
                    return;
                }
            }
            else
            {
                // Create an OpenFileDialog object
                OpenFileDialog openFileDialog = new OpenFileDialog();

                // Set the properties of the OpenFileDialog
                openFileDialog.Filter = "Bilder|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.Multiselect = false;

                // Display the dialog box and verify that the user clicked OK
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Load the image
                    image = Image.FromFile(openFileDialog.FileName);
                }
                else
                {
                    return;
                }
            }

            // Make the colors #000000 and #FFFFFF transparent
            image = MakeTransparent(image, Color.Black);
            image = MakeTransparent(image, Color.White);

            // Save the image in the images array
            images[index] = image;

            // Redraw the PictureBox
            pictureBoxTileArt2.Invalidate();
        }
        #endregion

        #region array for pictureBoxTileArt3
        private Image[] images3 = new Image[64]; // A separate array for pictureBoxTileArt3
        private Point? hoverTile = null; // Saves the tile the mouse hovers over
        private const int routeSize = 44; // Define routeSize as a class variable
        private const int routesX = 8; // Define routesX as a class variable
        private const int routesY = 8; // Define routesY as a class variable
        private Dictionary<Point, Rectangle> tileRectangles = new Dictionary<Point, Rectangle>(); // Stores the pixel coordinates of each tile
        #endregion

        #region pictureBoxTileArt3_MouseMove
        private void pictureBoxTileArt3_MouseMove(object sender, MouseEventArgs e)
        {
            // Show mouse coordinates on a label
            labelMouseCoordinates.Text = $"X: {e.X}, Y: {e.Y}";

            // Find the tile the mouse is hovering over
            Point newHoverTile = MouseToTileCoordinates(e.X, e.Y);

            // Check whether the mouse has entered a new tile
            if (newHoverTile != hoverTile)
            {
                // Update the tile the mouse is hovering over
                hoverTile = newHoverTile;

                // Redraw the PictureBox
                pictureBoxTileArt3.Invalidate();
            }
        }
        #endregion

        #region pictureBoxTileArt3_MouseClick
        private void pictureBoxTileArt3_MouseClick(object sender, MouseEventArgs e)
        {
            if (hoverTile.HasValue && e.Button == MouseButtons.Right)
            {
                int imageIndex = hoverTile.Value.X * routesY + hoverTile.Value.Y;
                if (imageIndex < images3.Length)
                {
                    // Load the image
                    LoadImageFromClipboardOrFile(imageIndex);

                    // Once the image has loaded, draw it onto the tile
                    if (images3[imageIndex] != null)
                    {
                        // Draw the image on the tile
                        Graphics g = pictureBoxTileArt3.CreateGraphics();
                        Rectangle tileRectangle = tileRectangles[hoverTile.Value];
                        g.DrawImage(images3[imageIndex], tileRectangle);
                    }
                }

                // Redraw the PictureBox
                pictureBoxTileArt3.Invalidate();
            }
        }
        #endregion

        #region pictureBoxTileArt3_Paint 
        private void pictureBoxTileArt3_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Matrix matrix = new Matrix();

            if (is3DView)
            {
                //Add a rotation to rotate the diamond mesh to an isometric view
                matrix.Rotate(currentRotation);
                // Add scale to distort the diamond mesh and complete the isometric view
                matrix.Scale((float)Math.Sqrt(2), (float)Math.Sqrt(2) / 2);
                // Add a translation to move the diamond mesh based on the value of the scroll bar
                matrix.Translate(hScrollBar1.Value, 0, MatrixOrder.Append);
            }
            // Set the transformation matrix of the Graphics object
            g.Transform = matrix;
            // Move the origin to the center of the PictureBox and shift it upwards by 1.5 route sizes
            g.TranslateTransform(pictureBoxTileArt3.Width / 2, pictureBoxTileArt3.Height / 2 - (float)(routeSize * routesY / 4 + 1.5 * routeSize));

            // Draw the routes
            for (int i = 0; i < routesX; i++)
            {
                for (int j = 0; j < routesY; j++)
                {
                    int x = (int)((i - j) * routeSize / 2f);
                    int y = (int)((i + j) * routeSize / 2f);

                    // Create a new polygon for the diamond
                    Point[] diamond = new Point[]
                    {
                        new Point(x, y - routeSize / 2),
                        new Point(x + routeSize / 2, y),
                        new Point(x, y + routeSize / 2),
                        new Point(x - routeSize / 2, y)
                    };

                    // Draw the diamond only if not in 3D mode
                    if (!is3DView)
                    {
                        g.DrawPolygon(Pens.Black, diamond);
                    }

                    // Save the pixel coordinates of the tile
                    Rectangle tileRectangle = new Rectangle(x - routeSize / 2, y - routeSize / 2, routeSize, routeSize);
                    tileRectangles[new Point(i, j)] = tileRectangle;

                    // Once the image has loaded, draw it onto the diamond
                    int imageIndex = i * routesY + j; // Calculate the index of the image based on the coordinates of the tile
                    if (imageIndex < images3.Length && images3[imageIndex] != null)
                    {
                        if (is3DView)
                        {
                            // Draw the image in 3D
                            DrawIsometricImage(g, images3[imageIndex], new Point(x, y));
                        }
                        else
                        {
                            // Draw the image in 2D
                            g.DrawImage(images3[imageIndex], tileRectangle);
                        }
                    }
                }
            }

            // Draw a highlight on the tile that the mouse hovers over
            if (hoverTile.HasValue)
            {
                int i = hoverTile.Value.X;
                int j = hoverTile.Value.Y;

                // Calculate the position of the tile
                int x = (int)((i - j) * routeSize / 2f);
                int y = (int)((i + j) * routeSize / 2f);

                // Create a new polygon for the highlight
                Point[] highlight = new Point[]
                {
                    new Point(x, y - routeSize / 2),
                    new Point(x + routeSize / 2, y),
                    new Point(x, y + routeSize / 2),
                    new Point(x - routeSize / 2, y)
                };

                // Draw the highlight
                g.FillPolygon(new SolidBrush(Color.FromArgb(128, Color.Yellow)), highlight); //Color
            }
        }
        #endregion

        #region LoadImageFromClipboardOrFile
        private void LoadImageFromClipboardOrFile(int index)
        {
            Image image;

            // Check whether the checkBoxClipboard2 is activated
            if (checkBoxClipboard2.Checked)
            {
                // Load the image from the clipboard
                if (Clipboard.ContainsImage())
                {
                    image = Clipboard.GetImage();
                }
                else
                {
                    MessageBox.Show("The clipboard does not contain an image.");
                    return;
                }
            }
            else
            {
                // Create an OpenFileDialog object
                OpenFileDialog openFileDialog = new OpenFileDialog();

                // Set the properties of the OpenFileDialog
                openFileDialog.Filter = "Bilder|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.Multiselect = false;

                // Display the dialog box and verify that the user clicked OK
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Load the image
                    image = Image.FromFile(openFileDialog.FileName);
                }
                else
                {
                    return;
                }
            }

            // Make the colors #000000 and #FFFFFF transparent
            image = MakeTransparent(image, Color.Black);
            image = MakeTransparent(image, Color.White);

            // Save the image in the images3 array
            images3[index] = image;

            // Redraw the PictureBox
            pictureBoxTileArt3.Invalidate();
        }
        #endregion

        #region MouseToTileCoordinates 
        private Point MouseToTileCoordinates(int mouseX, int mouseY)
        {
            // Convert the mouse coordinates relative to the origin of the PictureBox
            int relativeX = mouseX - pictureBoxTileArt3.Width / 2;
            int relativeY = mouseY - (pictureBoxTileArt3.Height / 2 - (int)(routeSize * routesY / 4 + 1.5 * routeSize));

            // Calculate tile coordinates based on relative mouse coordinates
            int i = (int)Math.Round((relativeX / (float)routeSize + relativeY / (float)routeSize));
            int j = (int)Math.Round((relativeY / (float)routeSize - relativeX / (float)routeSize));

            // Make sure the tile coordinates are within the valid range
            i = Math.Max(0, Math.Min(i, routesX - 1));
            j = Math.Max(0, Math.Min(j, routesY - 1));

            // Find the nearest tile
            Point closestTile = new Point(i, j);
            double minDistance = double.MaxValue;

            // Check the surrounding tiles to find the closest one
            for (int di = -1; di <= 1; di++)
            {
                for (int dj = -1; dj <= 1; dj++)
                {
                    Point tile = new Point(i + di, j + dj);
                    // Check that the tile lies within the boundaries of the diamond
                    if (tile.X >= 0 && tile.X < routesX && tile.Y >= 0 && tile.Y < routesY)
                    {
                        Point tileMouseCoords = TileToMouseCoordinates(tile.X, tile.Y);
                        double distance = Math.Sqrt(Math.Pow(tileMouseCoords.X - mouseX, 2) + Math.Pow(tileMouseCoords.Y - mouseY, 2));

                        if (distance < minDistance)
                        {
                            closestTile = tile;
                            minDistance = distance;
                        }
                    }
                }
            }

            return closestTile;
        }
        #endregion

        #region Point TileToMouseCoordinates
        private Point TileToMouseCoordinates(int i, int j)
        {
            // Calculate the pixel coordinates of the tile
            int x = (int)((i - j) * routeSize / 2f);
            int y = (int)((i + j) * routeSize / 2f);

            // Convert the pixel coordinates to mouse coordinates
            int mouseX = x + pictureBoxTileArt3.Width / 2;
            int mouseY = y + (pictureBoxTileArt3.Height / 2 - (int)(routeSize * routesY / 4 + 1.5 * routeSize));

            return new Point(mouseX, mouseY);
        }
        #endregion        

        #region TileArtForm_KeyDown
        private void TileArtForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (hoverTile.HasValue)
            {
                int i = hoverTile.Value.X;
                int j = hoverTile.Value.Y;

                switch (e.KeyCode)
                {
                    case Keys.W:  // W for up
                        i = Math.Max(0, i - 1);
                        break;
                    case Keys.A:  // A for left
                        j = Math.Min(routesY - 1, j + 1);
                        break;
                    case Keys.S:  // S for down
                        i = Math.Min(routesX - 1, i + 1);
                        break;
                    case Keys.D:  // D for right
                        j = Math.Max(0, j - 1);
                        break;
                    case Keys.Space:  // Space bar to load images
                        int imageIndex = i * routesY + j;  // Calculate the index of the image based on the coordinates of the tile
                        LoadImageFromClipboardOrFile(imageIndex);  // Load the image
                        break;
                }

                hoverTile = new Point(i, j);
                pictureBoxTileArt3.Invalidate();
            }
        }
        #endregion

        #region Veriable 3d
        // Global variable to store the state of the view
        private bool is3DView = false;
        // Global variable to store the current rotation value
        private float currentRotation = 0.0f;
        #endregion

        #region DrawIsometricImage
        private void DrawIsometricImage(Graphics g, Image image, Point location)
        {
            // Create a new matrix for the transformation
            Matrix matrix = new Matrix();

            // Add rotation to rotate the image to an isometric view
            matrix.Rotate(currentRotation); // Use the current rotation value

            // Add scaling to distort the image and complete the isometric view
            matrix.Scale((float)Math.Sqrt(2), (float)Math.Sqrt(2) / 2);

            // Add a translation to move the image based on the scroll bar value
            matrix.Translate(hScrollBar1.Value, 0, MatrixOrder.Append); // NEW ADDED

            // Set the transformation matrix of the Graphics object
            g.Transform = matrix;

            // Draw the image at the specified location
            g.DrawImage(image, location);
        }

        #endregion

        #region btnToggleView_Click
        private void btnToggleView_Click(object sender, EventArgs e)
        {
            // Switch between 2D and 3D view
            is3DView = !is3DView;

            // Redraw PictureBox
            pictureBoxTileArt3.Invalidate();
        }
        #endregion

        #region trackBar1_ValueChanged
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            // Update the alignment based on the value of the slider
            currentRotation = trackBar1.Value;

            // Update the label
            labelGrad.Text = $"Orientation: {currentRotation} Degree";

            // Redraw the image with the updated orientation
            pictureBoxTileArt3.Invalidate();
        }
        #endregion

        #region hScrollBar1_Scroll
        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            // Redraw the PictureBox
            pictureBoxTileArt3.Invalidate();
        }
        #endregion

        #region btFill64Tiles_Click
        private void btFill64Tiles_Click(object sender, EventArgs e)
        {
            Image image;

            // Check whether the checkBoxClipboard2 is activated
            if (checkBoxClipboard2.Checked)
            {
                // Load the image from the clipboard
                if (Clipboard.ContainsImage())
                {
                    image = Clipboard.GetImage();
                }
                else
                {
                    MessageBox.Show("The clipboard does not contain an image.");
                    return;
                }
            }
            else
            {
                // Create an OpenFileDialog object
                OpenFileDialog openFileDialog = new OpenFileDialog();

                // Set the properties of the OpenFileDialog
                openFileDialog.Filter = "Bilder|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.Multiselect = false;

                // Display the dialog box and verify that the user clicked OK
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Load the image
                    image = Image.FromFile(openFileDialog.FileName);
                }
                else
                {
                    return;
                }
            }

            // Make the colors #000000 and #FFFFFF transparent
            image = MakeTransparent(image, Color.Black);
            image = MakeTransparent(image, Color.White);

            // Save the image in all indices of the images3 array
            for (int i = 0; i < images3.Length; i++)
            {
                images3[i] = image;
            }

            // Redraw the PictureBox
            pictureBoxTileArt3.Invalidate();
        }
        #endregion
    }
}

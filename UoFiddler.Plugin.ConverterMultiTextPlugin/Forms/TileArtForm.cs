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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Helpers;
using System.IO;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static UoFiddler.Controls.UserControls.TileView.TileViewControl;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Media;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class TileArtForm : Form
    {
        private List<int> _tileList = new List<int>(); // List for storing items Tiles
        private int _selectedGraphicId = -1;
        public bool IsLoaded { get; private set; }
        private bool _showFreeSlots = true; // or false 

        private List<string> copiedIdAddresses = new List<string>(); // Copy List Hex ID
        private List<string> copiedHexAddresses = new List<string>(); // Insert Paint Grid Hex ID
        private List<List<string>> loadedHexAddressesInTileArt2 = new List<List<string>>(); // A list of lists for each button.



        public TileArtForm()
        {
            InitializeComponent();

            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(TileArtForm_KeyDown);

            hScrollBar1.Minimum = -pictureBoxTileArt3.Width / 1;
            hScrollBar1.Maximum = pictureBoxTileArt3.Width / 1;
            hScrollBar2.Minimum = -pictureBoxTileArt4.Width / 1;
            hScrollBar2.Maximum = pictureBoxTileArt4.Width / 1;

            // Call the OnLoad method here to draw the tiles
            OnLoad(this, EventArgs.Empty);

            // Initialize loadedHexAddressesInTileArt2 with empty lists.
            for (int i = 0; i < 9; i++) // Replace "9" with the number of buttons you have.
            {
                loadedHexAddressesInTileArt2.Add(new List<string>());
            }

            // Load the image from resources
            Image image = Properties.Resources.Transestion; // Replace "Properties.Resources.Transestion" with the actual path to your image in resources

            // Set the image as the panel's background image
            panelImage.BackgroundImage = image;

            // Enter the layout of the background image so that it stretches to fill the entire panel
            panelImage.BackgroundImageLayout = ImageLayout.Stretch;

        }

        #region SelectedGraphicId
        public int SelectedGraphicId
        {
            get => _selectedGraphicId;
            set
            {
                _selectedGraphicId = value < 0 ? 0 : value;
                UpdateToolStripLabels(_selectedGraphicId);
                LandTilesTileView.FocusIndex = _tileList.IndexOf(_selectedGraphicId);
            }
        }
        #endregion

        #region UpdateToolStripLabels
        private void UpdateToolStripLabels(int graphic)
        {
            if (!IsLoaded)
            {
                return;
            }

            NameLabel.Text = $"Name: {TileData.LandTable[graphic].Name}";
            GraphicLabel.Text = string.Format("ID: 0x{0:X4} ({0})", graphic);
            FlagsLabel.Text = $"Flags: {TileData.LandTable[graphic].Flags}";
        }
        #endregion

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

            // HexIDTextBox leeren
            HexIDTextBox9.Clear();

            // ID-Adressen in der HexIDTextBox auflisten
            foreach (string idAddress in copiedIdAddresses)
            {
                HexIDTextBox9.AppendText(idAddress + Environment.NewLine);
            }

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
        // Create a dictionary to store the hex addresses and type together.
        private Dictionary<string, string> hexAddresses = new Dictionary<string, string>();

        private void LoadImage(int index)
        {
            Image image;

            // Check if the checkBoxClipboard is checked.
            if (checkBoxClipboard.Checked)
            {
                // Load the image from the clipboard
                if (Clipboard.ContainsImage())
                {
                    image = Clipboard.GetImage();
                }
                else
                {
                    MessageBox.Show("The clipboard does not contain an image..");
                    return;
                }
            }
            else
            {
                // Create an OpenFileDialog object.
                OpenFileDialog openFileDialog = new OpenFileDialog();

                // Set the properties of the OpenFileDialog.
                openFileDialog.Filter = "Picture|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.Multiselect = false;

                // Display the dialog box and check if the user clicked OK.
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Load the image.
                    image = Image.FromFile(openFileDialog.FileName);
                }
                else
                {
                    return;
                }
            }

            // Make the colors #000000 and #FFFFFF transparent.
            image = MakeTransparent(image, Color.Black);
            image = MakeTransparent(image, Color.White);

            // Save the image in the images array.
            images[index] = image;

            // Redraw the PictureBox.
            pictureBoxTileArt2.Invalidate();

            // Add the loaded hex addresses from copiedIdAddresses to the dictionary for the selected type.
            string selectedType = null;
            if (checkBoxLL.Checked)
            {
                selectedType = "LL";
            }
            else if (checkBoxUU.Checked)
            {
                selectedType = "UU";
            }
            else if (checkBoxDL.Checked)
            {
                selectedType = "DL";
            }
            else if (checkBoxDR.Checked)
            {
                selectedType = "DR";
            }
            else if (checkBoxUL.Checked)
            {
                selectedType = "UL";
            }
            else if (checkBoxUR.Checked)
            {
                selectedType = "UR";
            }

            if (selectedType != null)
            {
                foreach (string address in copiedIdAddresses)
                {
                    if (!hexAddresses.ContainsKey(address))
                    {
                        hexAddresses[address] = selectedType;
                    }
                }
            }

            // Clear TextBoxTileArt2.
            TextBoxTileArt2.Clear();

            // List the loaded hex addresses in TextBoxTileArt2.
            foreach (var pair in hexAddresses)
            {
                TextBoxTileArt2.AppendText(pair.Key + " " + pair.Value + Environment.NewLine);
            }
        }
        #endregion

        #region private Dictionary<string, List<Tuple<string, string>>> hexAddressesByType = new Dictionary<string, List<Tuple<string, string>>>
        // Create a dictionary to store the hex addresses for each type.
        private Dictionary<string, List<Tuple<string, string>>> hexAddressesByType = new Dictionary<string, List<Tuple<string, string>>>
        {
            { "LL", new List<Tuple<string, string>>() },
            { "UU", new List<Tuple<string, string>>() },
            { "DL", new List<Tuple<string, string>>() },
            { "DR", new List<Tuple<string, string>>() },
            { "UL", new List<Tuple<string, string>>() },
            { "UR", new List<Tuple<string, string>>() },
        };
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

                    // Kopierte Hexadresse zur TextBoxTileArt3 hinzufügen
                    string copiedHexAddress = copiedHexAddresses.LastOrDefault();
                    if (!string.IsNullOrEmpty(copiedHexAddress))
                    {
                        TextBoxTileArt3.AppendText(copiedHexAddress + Environment.NewLine);
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

            // Clear HexIDTextBox.
            HexIDTextBox64.Clear();

            // List ID addresses in the HexIDTextBox.
            foreach (string idAddress in copiedIdAddresses)
            {
                HexIDTextBox64.AppendText(idAddress + Environment.NewLine);
            }

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

        #region btClearTilesAll_Click
        private void btClearTilesAll_Click(object sender, EventArgs e)
        {
            // Set each element in the images3 array to null
            for (int i = 0; i < images3.Length; i++)
            {
                images3[i] = null;
            }

            // Redraw the PictureBox to show the empty diamond spaces
            pictureBoxTileArt3.Invalidate();
        }
        #endregion

        #region btSaveDrawing_Click
        private void btSaveDrawing_Click(object sender, EventArgs e)
        {
            // Create a SaveFileDialog to select the save location
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Save the current drawing as a PNG image
                Bitmap bitmap = new Bitmap(pictureBoxTileArt3.Width, pictureBoxTileArt3.Height);
                pictureBoxTileArt3.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                bitmap.Save(saveFileDialog.FileName, ImageFormat.Png);
            }
        }
        #endregion

        #region btLoadDrawing_Click
        private void btLoadDrawing_Click(object sender, EventArgs e)
        {
            // Create an OpenFileDialog to select the file to load
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PNG Image|*.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Load the image and set it as the PictureBox's background image
                Image image = Image.FromFile(openFileDialog.FileName);
                pictureBoxTileArt3.BackgroundImage = image;
                pictureBoxTileArt3.Invalidate();
            }
        }
        #endregion

        #region Veriable pictureBoxTileArt4_Paint
        private Image[] images4 = new Image[256]; // A separate array for pictureBoxTileArt4.
        private const int routesX1 = 16; // Define routesX1 as a class variable
        private const int routesY1 = 16; // Define routesY1 as a class variable
        private const int routeSize2 = 44; // Define routeSize2 as a class variable
        private Point? hoverTile2 = null; // Saves the tile the mouse hovers over
        private Dictionary<Point, Rectangle> tileRectangles2 = new Dictionary<Point, Rectangle>(); // Stores the pixel coordinates of each tile
        #endregion

        #region pictureBoxTileArt4_Paint
        private void pictureBoxTileArt4_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Matrix matrix = new Matrix();

            // Clear HexIDTextBox.
            HexIDTextBox256.Clear();

            // List ID addresses in the HexIDTextBox.
            foreach (string idAddress in copiedIdAddresses)
            {
                HexIDTextBox256.AppendText(idAddress + Environment.NewLine);
            }

            if (is3DView2)
            {
                matrix.Rotate(currentRotation);
                matrix.Scale((float)Math.Sqrt(2), (float)Math.Sqrt(2) / 2);
                matrix.Translate(hScrollBar2.Value, 0, MatrixOrder.Append);
            }

            g.Transform = matrix;
            // Move the origin pixels to the right 0 and down 175
            g.TranslateTransform(0, -175);

            g.TranslateTransform(pictureBoxTileArt4.Width / 2, pictureBoxTileArt4.Height / 2 - (float)(routeSize * routesY / 4 + 1.5 * routeSize));

            // Draw the routes
            for (int i = 0; i < routesX1; i++)
            {
                for (int j = 0; j < routesY1; j++)
                {
                    int x = (int)((i - j) * routeSize2 / 2f);
                    int y = (int)((i + j) * routeSize2 / 2f);

                    // Create a new polygon for the diamond
                    Point[] diamond = new Point[]
                    {
                        new Point(x, y - routeSize2 / 2),
                        new Point(x + routeSize2 / 2, y),
                        new Point(x, y + routeSize2 / 2),
                        new Point(x - routeSize2 / 2, y)
                    };

                    // Only draw the diamond when not in 3D mode
                    if (!is3DView)
                    {
                        g.DrawPolygon(Pens.Black, diamond);
                    }

                    // Save the pixel coordinates of the tile
                    Rectangle tileRectangle2 = new Rectangle(x - routeSize2 / 2, y - routeSize2 / 2, routeSize2, routeSize2);
                    tileRectangles2[new Point(i, j)] = tileRectangle2;

                    // Once the image loads, draw it onto the diamond
                    int imageIndex = i * routesY1 + j; // Calculate the index of the image based on the coordinates of the tile
                    if (imageIndex < images4.Length && images4[imageIndex] != null)
                    {
                        if (is3DView)
                        {
                            // Draw the image in 3D
                            DrawIsometricImage2(g, images4[imageIndex], new Point(x, y));
                        }
                        else
                        {
                            // Draw the image in 2D
                            g.DrawImage(images4[imageIndex], tileRectangle2);
                        }
                    }
                }
            }

            // Draw a highlight on the tile that the mouse is hovering over
            if (hoverTile2.HasValue)
            {
                int i = hoverTile2.Value.X;
                int j = hoverTile2.Value.Y;

                // v
                int x = (int)((i - j) * routeSize2 / 2f);
                int y = (int)((i + j) * routeSize2 / 2f);

                // Create a new polygon for the highlight
                Point[] highlight = new Point[]
                {
                    new Point(x, y - routeSize2 / 2),
                    new Point(x + routeSize2 / 2, y),
                    new Point(x, y + routeSize2 / 2),
                    new Point(x - routeSize2 / 2, y)
                };

                // Draw the highlight
                g.FillPolygon(new SolidBrush(Color.FromArgb(128, Color.Yellow)), highlight); //Color
            }
        }
        #endregion

        #region pictureBoxTileArt4_MouseMove
        private void pictureBoxTileArt4_MouseMove(object sender, MouseEventArgs e)
        {
            // Show mouse coordinates on a label
            labelMouseCoordinates2.Text = $"X: {e.X}, Y: {e.Y}";

            // Find the tile the mouse is hovering over
            Point newHoverTile2 = MouseToTileCoordinates2(e.X, e.Y);

            // Check whether the mouse has entered a new tile
            if (newHoverTile2 != hoverTile2)
            {
                // Update the tile the mouse is hovering over
                hoverTile2 = newHoverTile2;

                // Redraw the PictureBox
                pictureBoxTileArt4.Invalidate();
            }
        }
        #endregion

        #region pictureBoxTileArt4_MouseClick       

        private void pictureBoxTileArt4_MouseClick(object sender, MouseEventArgs e)
        {
            if (hoverTile2.HasValue && e.Button == MouseButtons.Right)
            {
                //int imageIndex = hoverTile2.Value.X * routesY + hoverTile2.Value.Y;
                int imageIndex = hoverTile2.Value.X * routesY1 + hoverTile2.Value.Y;

                if (imageIndex < images4.Length)
                {
                    // Load the image
                    LoadImageFromClipboardOrFile3(imageIndex);

                    // Once the image has loaded, draw it onto the tile
                    if (images4[imageIndex] != null)
                    {
                        // Draw the image on the tile
                        Graphics g = pictureBoxTileArt4.CreateGraphics();
                        Rectangle tileRectangle2 = tileRectangles2[hoverTile2.Value];
                        g.DrawImage(images4[imageIndex], tileRectangle2);
                    }

                    // Add copied hex address to TextBoxTileArt4
                    string copiedHexAddress = copiedHexAddresses.LastOrDefault();
                    if (!string.IsNullOrEmpty(copiedHexAddress))
                    {
                        TextBoxTileArt4.AppendText(copiedHexAddress + Environment.NewLine);
                    }
                }

                // Redraw the PictureBox
                pictureBoxTileArt4.Invalidate();
            }
        }
        #endregion

        #region LoadImageFromClipboardOrFile3
        private void LoadImageFromClipboardOrFile3(int index)
        {
            Image image;

            // Check whether the checkBoxClipboard3 is activated
            if (checkBoxClipboard3.Checked)
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
                openFileDialog.Filter = "Picture|*.jpg;*.jpeg;*.png;*.bmp";
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

            // Save the image in the images4 array
            images4[index] = image;

            // Redraw the PictureBox
            pictureBoxTileArt4.Invalidate();
        }
        #endregion

        #region MouseToTileCoordinates2
        private Point MouseToTileCoordinates2(int mouseX, int mouseY)
        {
            // Convert the mouse coordinates relative to the origin of the PictureBox
            int relativeX = mouseX - pictureBoxTileArt4.Width / 2;
            int relativeY = mouseY - (pictureBoxTileArt4.Height / 2 - (int)(routeSize2 * routesY1 / 4 + 1.5 * routeSize2)) + 131;

            // Calculate tile coordinates based on relative mouse coordinates
            int i = (int)Math.Round((relativeX / (float)routeSize2 + relativeY / (float)routeSize2));
            int j = (int)Math.Round((relativeY / (float)routeSize2 - relativeX / (float)routeSize2));

            // Make sure the tile coordinates are within the valid range
            i = Math.Max(0, Math.Min(i, routesX1 - 1));
            j = Math.Max(0, Math.Min(j, routesY1 - 1));

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
                    if (tile.X >= 0 && tile.X < routesX1 && tile.Y >= 0 && tile.Y < routesY1)
                    {
                        Point tileMouseCoords = TileToMouseCoordinates2(tile.X, tile.Y);
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

        #region TileToMouseCoordinates2
        private Point TileToMouseCoordinates2(int i, int j)
        {
            // Calculate the pixel coordinates of the tile
            int x = (int)((i - j) * routeSize2 / 2f);
            int y = (int)((i + j) * routeSize2 / 2f);

            // Convert the pixel coordinates to mouse coordinates
            int mouseX = x + pictureBoxTileArt4.Width / 2;
            int mouseY = y + (pictureBoxTileArt4.Height / 2 - (int)(routeSize2 * routesY1 / 4 + 1.5 * routeSize2)) - 88;

            return new Point(mouseX, mouseY);
        }
        #endregion

        #region Veriable 3d
        // Global variable to store the state of the view
        private bool is3DView2 = false;
        // Global variable to store the current rotation value
        private float currentRotation2 = 0.0f;
        #endregion

        #region DrawIsometricImage2
        private void DrawIsometricImage2(Graphics g, Image image, Point location)
        {
            // Create a new matrix for the transformation
            Matrix matrix = new Matrix();

            // Add rotation to rotate the image to an isometric view
            matrix.Rotate(currentRotation2); // Use the current rotation value

            // Add scaling to distort the image and complete the isometric view
            matrix.Scale((float)Math.Sqrt(2), (float)Math.Sqrt(2) / 2);

            // Add a translation to move the image based on the scroll bar value
            matrix.Translate(hScrollBar2.Value, 0, MatrixOrder.Append); // NEW ADDED

            // Set the transformation matrix of the Graphics object
            g.Transform = matrix;

            // Draw the image at the specified location
            g.DrawImage(image, location);
        }
        #endregion

        #region btnToggleView2_Click
        private void btnToggleView2_Click(object sender, EventArgs e)
        {
            // Switch between 2D and 3D view
            is3DView2 = !is3DView2;

            // Redraw PictureBox
            pictureBoxTileArt4.Invalidate();
        }
        #endregion

        #region hScrollBar2_Scroll
        private void hScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            // Redraw the PictureBox
            pictureBoxTileArt4.Invalidate();
        }
        #endregion

        #region  btFill256Tiles
        private void btFill256Tiles_Click(object sender, EventArgs e)
        {
            Image image;

            // Check whether the checkBoxClipboard2 is activated
            if (checkBoxClipboard3.Checked)
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
            for (int i = 0; i < images4.Length; i++)
            {
                images4[i] = image;
            }

            // Redraw the PictureBox
            pictureBoxTileArt4.Invalidate();
        }
        #endregion

        #region pictureBoxTileArt2Mirror_Paint
        private void pictureBoxTileArt2Mirror_Paint(object sender, PaintEventArgs e)
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

        #region OnLoad
        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;

            const int landTileMax = 0x4000;

            for (int i = 0; i < landTileMax; ++i)
            {
                if (Art.IsValidLand(i))
                {
                    _tileList.Add(i);
                }
            }

            LandTilesTileView.VirtualListSize = _tileList.Count;

            if (!IsLoaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.LandTileChangeEvent += OnLandTileChangeEvent;
                ControlEvents.TileDataChangeEvent += OnTileDataChangeEvent;
            }

            IsLoaded = true;

            Cursor.Current = Cursors.Default;
        }
        #endregion

        #region OnFilePathChangeEvent
        private void OnFilePathChangeEvent()
        {
            Reload();
        }
        #endregion

        #region Reload
        private void Reload()
        {
            if (!IsLoaded)
            {
                return;
            }

            _selectedGraphicId = -1;
            _tileList.Clear();

            OnLoad(this, new MyEventArgs(MyEventArgs.Types.ForceReload));
        }
        #endregion

        #region OnTileDataChangeEvent
        private void OnTileDataChangeEvent(object sender, int id)
        {
            if (!IsLoaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (id < 0 || id > 0x3FFF)
            {
                return;
            }

            if (_selectedGraphicId != id)
            {
                return;
            }

            UpdateToolStripLabels(id);
        }
        #endregion

        #region OnLandTileChangeEvent
        private void OnLandTileChangeEvent(object sender, int index)
        {
            if (!IsLoaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (Art.IsValidLand(index))
            {
                bool done = false;
                for (int i = 0; i < _tileList.Count; ++i)
                {
                    if (index < _tileList[i])
                    {
                        _tileList.Insert(i, index);
                        done = true;
                        break;
                    }

                    if (index != _tileList[i])
                    {
                        continue;
                    }

                    done = true;
                    break;
                }

                if (!done)
                {
                    _tileList.Add(index);
                }
            }
            else
            {
                if (_showFreeSlots)
                {
                    return;
                }

                _tileList.Remove(index);
            }

            LandTilesTileView.VirtualListSize = _tileList.Count;
            LandTilesTileView.Invalidate();
        }
        #endregion

        #region LandTilesTileView_DrawItem
        private void LandTilesTileView_DrawItem(object sender, DrawTileListItemEventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Point itemPoint = new Point(e.Bounds.X + LandTilesTileView.TilePadding.Left, e.Bounds.Y + LandTilesTileView.TilePadding.Top);
            const int fixedTileSize = 44;
            Size itemSize = new Size(fixedTileSize, fixedTileSize);
            Rectangle itemRec = new Rectangle(itemPoint, itemSize);

            var previousClip = e.Graphics.Clip;

            e.Graphics.Clip = new Region(itemRec);

            if (e.Index < _tileList.Count)
            {
                Bitmap bitmap = Art.GetLand(_tileList[e.Index], out bool patched);

                if (bitmap == null)
                {
                    e.Graphics.Clip = new Region(itemRec);

                    itemRec.X += 5;
                    itemRec.Y += 5;

                    itemRec.Width -= 10;
                    itemRec.Height -= 10;

                    e.Graphics.FillRectangle(Brushes.Red, itemRec);
                    e.Graphics.Clip = previousClip;
                }
                else
                {
                    if (patched)
                    {
                        // different background for verdata patched tiles
                        e.Graphics.FillRectangle(Brushes.LightCoral, itemRec);
                    }

                    e.Graphics.DrawImage(bitmap, itemRec);

                    e.Graphics.Clip = previousClip;
                }
            }
            else
            {
                // Handling the case when the index is out of range...
            }
        }
        #endregion

        #region TileArtTileView_ItemSelectionChanged
        private void TileArtTileView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
            {
                return;
            }

            if (_tileList.Count == 0)
            {
                return;
            }

            SelectedGraphicId = e.ItemIndex < 0 || e.ItemIndex > _tileList.Count
                ? _tileList[0]
                : _tileList[e.ItemIndex];
        }
        #endregion

        #region Copy clipboard - Event handler for the click event of the copyToolStripMenuItem control
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if a graphic is selected
            if (_selectedGraphicId >= 0)
            {
                // Hexadezimalwert des ausgewählten _selectedGraphicId zur Liste hinzufügen
                copiedIdAddresses.Add("0x" + _selectedGraphicId.ToString("X4"));
                string hexAddress = "0x" + _selectedGraphicId.ToString("X4");
                copiedHexAddresses.Add(hexAddress);

                // Get the bitmap of the selected graphic using the Art class
                Bitmap originalBitmap = Art.GetLand(_selectedGraphicId);
                if (originalBitmap != null)
                {
                    // Create a copy of the original image.
                    Bitmap bitmap = new Bitmap(originalBitmap);

                    // Color change function directly integrated.
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            Color pixelColor = bitmap.GetPixel(x, y);
                            if (pixelColor.R == 211 && pixelColor.G == 211 && pixelColor.B == 211) // Check if the color of the pixel is #D3D3D3
                            {
                                bitmap.SetPixel(x, y, Color.Black); // Change the color of the pixel to black
                            }
                        }
                    }

                    // Convert the image to a 24-bit color depth
                    Bitmap bmp24bit = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(bmp24bit))
                    {
                        g.DrawImage(bitmap, new Rectangle(0, 0, bmp24bit.Width, bmp24bit.Height));
                    }

                    // Copy the graphic to the clipboard
                    Clipboard.SetImage(bmp24bit);

                    // Show a message box indicating success
                    MessageBox.Show("The image has been copied to the clipboard!");
                }
                else
                {
                    // Show a message box indicating failure
                    MessageBox.Show("No image to copy!");
                }
            }
        }
        #endregion

        #region btnClearTextBoxTileArt3
        private void btnClearTextBoxTileArt3_Click(object sender, EventArgs e)
        {
            TextBoxTileArt3.Clear();
        }
        #endregion

        #region btnClearTextBoxTileArt4
        private void btnClearTextBoxTileArt4_Click(object sender, EventArgs e)
        {
            TextBoxTileArt4.Clear();
        }
        #endregion

        #region btClearTilesAll4
        private void btClearTilesAll4_Click(object sender, EventArgs e)
        {
            // Set each element in the images3 array to null
            for (int i = 0; i < images4.Length; i++)
            {
                images4[i] = null;
            }

            // Redraw the PictureBox to show the empty diamond spaces
            pictureBoxTileArt4.Invalidate();
        }
        #endregion

        #region GenerateXmlButton_Click
        private void GenerateXmlButton_Click(object sender, EventArgs e)
        {
            // Create the XML section based on the stored hex addresses.
            string xml = $"<Brush Id=\"{textBoxId.Text}\" Name=\"{textBoxName.Text}\">\n";

            // Add the hex addresses from textboxLandID.
            string[] landIDs = textboxLandID.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string id in landIDs)
            {
                xml += $"    <Land ID=\"{id}\"/>\n";
            }

            xml += $"    <Edge To=\"{textBoxEdgeTo.Text}\">\n";
            foreach (var pair in hexAddresses)
            {
                xml += $"        <Land Type=\"{pair.Value}\" ID=\"{pair.Key}\"/>\n";
            }
            xml += "    </Edge>\n";
            xml += "</Brush>";

            // Display the generated XML section.
            richTextBoxXML.Text = xml;
        }
        #endregion

        #region  checkBoxLL checkBoxUU checkBoxDL ... CheckedChanged
        private void checkBoxLL_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxLL.Checked)
            {
                checkBoxUU.Checked = false;
                checkBoxDL.Checked = false;
                checkBoxDR.Checked = false;
                checkBoxUL.Checked = false;
                checkBoxUR.Checked = false;
            }
        }

        private void checkBoxUU_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxUU.Checked)
            {
                checkBoxLL.Checked = false;
                checkBoxDL.Checked = false;
                checkBoxDR.Checked = false;
                checkBoxUL.Checked = false;
                checkBoxUR.Checked = false;
            }
        }

        private void checkBoxDL_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDL.Checked)
            {
                checkBoxLL.Checked = false;
                checkBoxUU.Checked = false;
                checkBoxDR.Checked = false;
                checkBoxUL.Checked = false;
                checkBoxUR.Checked = false;
            }
        }

        private void checkBoxDR_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDR.Checked)
            {
                checkBoxLL.Checked = false;
                checkBoxUU.Checked = false;
                checkBoxDL.Checked = false;
                checkBoxUL.Checked = false;
                checkBoxUR.Checked = false;
            }
        }

        private void checkBoxUL_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxUL.Checked)
            {
                checkBoxLL.Checked = false;
                checkBoxUU.Checked = false;
                checkBoxDL.Checked = false;
                checkBoxDR.Checked = false;
                checkBoxUR.Checked = false;
            }
        }

        private void checkBoxUR_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxUR.Checked)
            {
                checkBoxLL.Checked = false;
                checkBoxUU.Checked = false;
                checkBoxDL.Checked = false;
                checkBoxDR.Checked = false;
                checkBoxUL.Checked = false;
            }
        }
        #endregion

        #region resetButton
        private void resetButton_Click(object sender, EventArgs e)
        {
            // Reset pictureBoxTileArt2.
            for (int i = 0; i < images.Length; i++) // Access the "images" array.
            {
                images[i] = null;
            }
            pictureBoxTileArt2.Invalidate();

            // Reset TextBoxTileArt2.
            TextBoxTileArt2.Clear();

            // Reset HexIDTextBox9.
            HexIDTextBox9.Clear();

            // Fügen Sie hier weitere Reset-Logik hinzu, falls erforderlich
        }
        #endregion

        #region copyClipbordButton
        private void copyClipbordButton_Click(object sender, EventArgs e)
        {
            // Check if richTextBoxXML.Text is null.
            if (!string.IsNullOrEmpty(richTextBoxXML.Text))
            {
                // Copy the contents of richTextBoxXML to the clipboard.
                Clipboard.SetText(richTextBoxXML.Text);
            }
            else
            {
                MessageBox.Show("There is no text to copy..");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion
    }
}

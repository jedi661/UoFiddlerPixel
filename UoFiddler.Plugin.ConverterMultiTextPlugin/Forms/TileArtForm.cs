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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class TileArtForm : Form
    {
        public TileArtForm()
        {
            InitializeComponent();
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
    }
}

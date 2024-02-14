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
        private void pictureBoxTileArt_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Definieren Sie die Größe der Route
            int routeSize = 44;

            // Definieren Sie die Anzahl der Routen in der x- und y-Richtung
            int routesX = 3;
            int routesY = 3;

            // Verschieben Sie den Ursprung in die Mitte der PictureBox
            g.TranslateTransform(pictureBoxTileArt.Width / 2, pictureBoxTileArt.Height / 2);

            // Zeichnen Sie die Routen
            for (int i = 0; i < routesX; i++)
            {
                for (int j = 0; j < routesY; j++)
                {
                    // Berechnen Sie die Position der Route
                    int x = (int)((i - j) * routeSize / 2f) - routeSize;
                    int y = (int)((i + j) * routeSize / 2f) - routeSize;

                    // Erstellen Sie ein neues Polygon für die Raute
                    Point[] diamond = new Point[]
                    {
                    new Point(x + routeSize / 2, y),
                    new Point(x + routeSize, y + routeSize / 2),
                    new Point(x + routeSize / 2, y + routeSize),
                    new Point(x, y + routeSize / 2)
                    };

                    // Zeichnen Sie die Raute
                    g.DrawPolygon(Pens.Black, diamond);

                    // Zeichnen Sie das Bild auf die Raute, wenn es geladen wurde
                    if (images[0] != null)
                    {
                        g.DrawImage(images[0], x, y, routeSize, routeSize);
                    }
                }
            }
        }
        private Image MakeTransparent(Image image, Color color)
        {
            Bitmap bitmap = new Bitmap(image);

            bitmap.MakeTransparent(color);

            return bitmap;
        }

        // Globale Variable zum Speichern der Bilder
        private Image[] images = new Image[9];
        private void btloadArt0All_Click(object sender, EventArgs e)
        {
            // Erstellen Sie ein OpenFileDialog-Objekt
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Setzen Sie die Eigenschaften des OpenFileDialog
            openFileDialog.Filter = "Bilder|*.jpg;*.jpeg;*.png;*.bmp";
            openFileDialog.Multiselect = false;

            // Zeigen Sie das Dialogfeld an und prüfen Sie, ob der Benutzer auf OK geklickt hat
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Laden Sie das Bild
                Image image = Image.FromFile(openFileDialog.FileName);

                // Machen Sie die Farben #000000 und #FFFFFF transparent
                image = MakeTransparent(image, Color.Black);
                image = MakeTransparent(image, Color.White);

                // Speichern Sie das Bild in der images-Array
                images[0] = image;

                // Zeichnen Sie die PictureBox neu
                pictureBoxTileArt.Invalidate();
            }
        }

        private void pictureBoxTileArt_Paint2(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Definieren Sie die Größe der Route
            int routeSize = 44;

            // Definieren Sie die Anzahl der Routen in der x- und y-Richtung
            int routesX = 3;
            int routesY = 3;

            // Verschieben Sie den Ursprung in die Mitte der PictureBox
            g.TranslateTransform(pictureBoxTileArt2.Width / 2, pictureBoxTileArt2.Height / 2);

            // Zeichnen Sie die Routen
            for (int i = 0; i < routesX; i++)
            {
                for (int j = 0; j < routesY; j++)
                {
                    // Berechnen Sie die Position der Route
                    int x = (int)((i - j) * routeSize / 2f) - routeSize;
                    int y = (int)((i + j) * routeSize / 2f) - routeSize;

                    // Erstellen Sie ein neues Polygon für die Raute
                    Point[] diamond = new Point[]
                    {
                new Point(x + routeSize / 2, y),
                new Point(x + routeSize, y + routeSize / 2),
                new Point(x + routeSize / 2, y + routeSize),
                new Point(x, y + routeSize / 2)
                    };

                    // Zeichnen Sie die Raute
                    g.DrawPolygon(Pens.Black, diamond);

                    // Zeichnen Sie das Bild auf die Raute, wenn es geladen wurde
                    int imageIndex = i * routesY + j; // Berechnen Sie den Index des Bildes basierend auf den Koordinaten der Kachel
                    if (imageIndex < images.Length && images[imageIndex] != null)
                    {
                        g.DrawImage(images[imageIndex], x, y, routeSize, routeSize);
                    }
                }
            }
        }

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

        private void LoadImage(int index)
        {
            Image image;

            // Überprüfen Sie, ob die checkBoxClipboard aktiviert ist
            if (checkBoxClipboard.Checked)
            {
                // Laden Sie das Bild aus der Zwischenablage
                if (Clipboard.ContainsImage())
                {
                    image = Clipboard.GetImage();
                }
                else
                {
                    MessageBox.Show("Die Zwischenablage enthält kein Bild.");
                    return;
                }
            }
            else
            {
                // Erstellen Sie ein OpenFileDialog-Objekt
                OpenFileDialog openFileDialog = new OpenFileDialog();

                // Setzen Sie die Eigenschaften des OpenFileDialog
                openFileDialog.Filter = "Bilder|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.Multiselect = false;

                // Zeigen Sie das Dialogfeld an und prüfen Sie, ob der Benutzer auf OK geklickt hat
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Laden Sie das Bild
                    image = Image.FromFile(openFileDialog.FileName);
                }
                else
                {
                    return;
                }
            }

            // Machen Sie die Farben #000000 und #FFFFFF transparent
            image = MakeTransparent(image, Color.Black);
            image = MakeTransparent(image, Color.White);

            // Speichern Sie das Bild in der images-Array
            images[index] = image;

            // Zeichnen Sie die PictureBox neu
            pictureBoxTileArt2.Invalidate();
        }


    }
}

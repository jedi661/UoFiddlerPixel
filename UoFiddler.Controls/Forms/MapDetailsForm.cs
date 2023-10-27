/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class MapDetailsForm : Form
    {
        private int _currentStaticIndex = 0;
        private HuedTile[] _staticsAtPoint;

        public MapDetailsForm(Map currentMap, Point point)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();
            TopMost = true;

            Tile currentTile = currentMap.Tiles.GetLandTile(point.X, point.Y);
            richTextBox.AppendText($"X: {point.X} Y: {point.Y}\n\n");
            richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Bold);
            richTextBox.AppendText("LandTile:\n");
            richTextBox.AppendText($"{TileData.LandTable[currentTile.Id].Name}: 0x{currentTile.Id:X} Altitude: {currentTile.Z}\n\n");
            HuedTile[] staticsAtPoint = currentMap.Tiles.GetStaticTiles(point.X, point.Y);
            richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Regular);
            richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Bold);
            _staticsAtPoint = currentMap.Tiles.GetStaticTiles(point.X, point.Y);
            richTextBox.AppendText("Statics:\n");
            richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Regular);
            foreach (HuedTile @static in staticsAtPoint)
            {
                ushort id = @static.Id;
                richTextBox.AppendText($"{TileData.ItemTable[id].Name}: 0x{id:X} Hue: {@static.Hue} Altitude: {@static.Z}\n");
            }
            // Display for the textures that are being determined.
            UpdateTextureInformation(point, currentTile);

            // Display of the land tiles in pictureBox1
            Bitmap landTileBitmap = Art.GetLand(currentTile.Id);
            pictureBox1.Image = landTileBitmap;

            // Display of the current statics in pictureBox2
            if (staticsAtPoint.Length > 0)
            {
                HuedTile currentStatic = staticsAtPoint[_currentStaticIndex];
                Bitmap staticBitmap = Art.GetStatic(currentStatic.Id);
                pictureBox2.Image = staticBitmap;
            }
        }

        private void UpdateTextureInformation(Point point, Tile currentTile)
        {
            // Get Tile ID from CurrentTile
            int tileID = currentTile.Id;

            // Determine via TileData TexID
            int texID = TileData.LandTable[tileID].TextureId;

            // Load texture
            Bitmap texture = Textures.GetTexture(texID);

            if (texture != null)
            {
                // Empty line 
                richTextBox.AppendText("\n");
                // Texture heading bold
                richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Bold);
                // Details with line breaks
                richTextBox.AppendText("Texture:\n");
                richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Regular);
                richTextBox.AppendText($"Width: {texture.Width} Height: {texture.Height}\n");
                richTextBox.AppendText($"TexID: 0x{texID:X}\n");
                // Load into PictureBox
                pictureBox3.Image = texture;
            }
        }

        private void buttonNext_Click_1(object sender, System.EventArgs e)
        {
            if (_currentStaticIndex < _staticsAtPoint.Length - 1)
            {
                _currentStaticIndex++;
                HuedTile currentStatic = _staticsAtPoint[_currentStaticIndex];
                Bitmap staticBitmap = Art.GetStatic(currentStatic.Id);
                pictureBox2.Image = staticBitmap;
            }
        }

        private void buttonPrevious_Click_1(object sender, System.EventArgs e)
        {
            if (_currentStaticIndex > 0)
            {
                _currentStaticIndex--;
                HuedTile currentStatic = _staticsAtPoint[_currentStaticIndex];
                Bitmap staticBitmap = Art.GetStatic(currentStatic.Id);
                pictureBox2.Image = staticBitmap;
            }
        }
    }
}
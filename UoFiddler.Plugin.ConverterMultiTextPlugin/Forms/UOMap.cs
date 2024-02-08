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
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualBasic.Devices;
using System.Diagnostics;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{

    public partial class UOMap : Form
    {
        private byte[] radarColorBuffer;
        private byte[] mapBuffer;
        private const int BlockSize = 196;
        private const int BlockH = 8;
        private const int BlockV = 8;
        private int MapBlockH;
        private int MapBlockV;
        private const int ViewStrtH = 0;
        private const int ViewStrtV = 0;
        private int ViewSizeH;
        private int ViewSizeV;
        private OpenFileDialog openFileDialog;

        private Dictionary<string, Tuple<int, int>> mapSizes = new Dictionary<string, Tuple<int, int>>
        {
            { "map0.mul", Tuple.Create(6144, 4096) },
            { "map1.mul", Tuple.Create(6144, 4096) },
            { "map2.mul", Tuple.Create(2304, 1600) },
            { "map3.mul", Tuple.Create(2560, 2048) },
            { "map4.mul", Tuple.Create(1448, 1448) },
            { "map5.mul", Tuple.Create(1280, 4096) },
            { "map6.mul", Tuple.Create(6144, 4096) }, //Forell
            { "map7.mul", Tuple.Create(6144, 4096) }, //Dragon
        };

        public UOMap()
        {
            InitializeComponent();

            comboBoxMaps.Items.AddRange(new object[] { "map0.mul", "map1.mul", "map2.mul", "map3.mul", "map4.mul", "map5.mul", "map6.mul", "map7.mul" });
        }

        #region BtnLoad
        private void BtnLoad_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string directoryPath = folderBrowserDialog.SelectedPath;
                textBoxLoad.Text = directoryPath;

                if (comboBoxMaps.SelectedItem != null)
                {
                    string[] fileNames = { "radarcol.mul", comboBoxMaps.SelectedItem.ToString() };
                    foreach (string fileName in fileNames)
                    {
                        string filePath = Path.Combine(directoryPath, fileName);
                        if (File.Exists(filePath))
                        {
                            LoadGameFileData(filePath);
                        }
                        else
                        {
                            MessageBox.Show($"The file '{fileName}' does not exist in the selected directory.");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select a card from the drop down list.");
                }
            }
        }
        #endregion

        #region LoadGameFileData
        private void LoadGameFileData(string filePath)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            string fileName = Path.GetFileName(filePath);

            if (fileName.StartsWith("map") && fileName.EndsWith(".mul"))
            {
                mapBuffer = fileData;
                SetMapSize(fileName);
            }
            else if (fileName == "radarcol.mul")
            {
                radarColorBuffer = fileData;
            }

            MessageBox.Show($"Loaded {fileData.Length / 1024.0} KBytes of game data from '{filePath}'.");
        }
        #endregion

        #region SetMapSize
        private void SetMapSize(string fileName)
        {
            if (mapSizes.TryGetValue(fileName, out var size))
            {
                MapBlockH = size.Item1 / BlockH;
                MapBlockV = size.Item2 / BlockV;
                ViewSizeH = size.Item1 / BlockH;
                ViewSizeV = size.Item2 / BlockV;
            }
            else
            {
                throw new Exception($"Unbekannte Karte: {fileName}");
            }
        }
        #endregion

        #region BtnExecute
        private void BtnExecute_Click(object sender, EventArgs e)
        {
            // Check if a map is selected in the comboBoxMaps
            if (comboBoxMaps.SelectedItem == null)
            {
                MessageBox.Show("Please first select a card from the drop down list.");
                return;
            }

            // Initialize the progress bar
            progressBarMap.Minimum = 0;
            progressBarMap.Maximum = ViewSizeH * ViewSizeV;
            progressBarMap.Value = 0;

            // Create the bitmaps
            Bitmap blkim = new Bitmap(ViewSizeH * BlockH, ViewSizeV * BlockV);
            Bitmap zim = new Bitmap(ViewSizeH * BlockH, ViewSizeV * BlockV);

            for (int rblockv = ViewStrtV; rblockv < ViewStrtV + ViewSizeV; rblockv++)
            {
                for (int rblockh = ViewStrtH; rblockh < ViewStrtH + ViewSizeH; rblockh++)
                {
                    var (b, z) = RenderBlock(GetBlock(Tuple.Create(rblockh, rblockv)));
                    int imh = BlockH * (rblockh - ViewStrtH);
                    int imv = BlockV * (rblockv - ViewStrtV);
                    using (Graphics g = Graphics.FromImage(blkim))
                    {
                        g.DrawImage(b, imh, imv);
                    }
                    using (Graphics g = Graphics.FromImage(zim))
                    {
                        g.DrawImage(z, imh, imv);
                    }

                    // Update the progress bar
                    progressBarMap.Value++;
                }
            }

            // Get the path to the program directory
            string programDirectory = Application.StartupPath;

            // Define the path to the temporary directory in the program directory
            string directory = Path.Combine(programDirectory, "tempGrafic");

            // Create the directory if it does not exist
            Directory.CreateDirectory(directory);

            // Save the images in the specified directory
            string mapName = comboBoxMaps.SelectedItem.ToString().Replace(".mul", "");
            string format = saveAsBmpCheckBox.Checked ? ".bmp" : ".png";
            blkim.Save(Path.Combine(directory, $"{mapName}_map{format}"), ImageFormat.Bmp);
            zim.Save(Path.Combine(directory, $"{mapName}_zmap{format}"), ImageFormat.Bmp);

            // Confirm the completion in textBoxLoad
            textBoxLoad.Text = $"The images were successfully added to the directory '{directory}' saved.";
        }
        #endregion

        #region Color RadarColor2Rgb
        private Color RadarColor2Rgb(int rcode)
        {
            if (radarColorBuffer == null)
            {
                throw new Exception("radarColorBuffer has not been initialized.");
            }

            int offset = 2 * rcode;
            ushort colorVal = BitConverter.ToUInt16(radarColorBuffer, offset);
            return Color.FromArgb(((colorVal >> 10) & 0x1F) << 3, ((colorVal >> 5) & 0x1F) << 3, ((colorVal >> 0) & 0x1F) << 3);
        }
        #endregion

        #region GetBlock
        private byte[] GetBlock(Tuple<int, int> blkcoord)
        {
            int offset = BlockSize * (blkcoord.Item1 * MapBlockV + blkcoord.Item2);
            byte[] block = new byte[BlockSize];
            Array.Copy(mapBuffer, offset, block, 0, BlockSize);
            return block;
        }
        #endregion

        #region Tuple<Bitmap, Bitmap> RenderBlock
        private Tuple<Bitmap, Bitmap> RenderBlock(byte[] buffer)
        {
            Bitmap blkim = new Bitmap(BlockH, BlockV);
            Bitmap zim = new Bitmap(BlockH, BlockV);
            for (int v = 0; v < BlockV; v++)
            {
                for (int h = 0; h < BlockH; h++)
                {
                    int offset = 4 + 3 * (8 * v + h);
                    ushort trccode = BitConverter.ToUInt16(buffer, offset);
                    sbyte zval = (sbyte)buffer[offset + 2];
                    blkim.SetPixel(h, v, RadarColor2Rgb(trccode));
                    zim.SetPixel(h, v, Color.FromArgb(128 + zval, 128 + zval, 128 + zval));
                }
            }
            return Tuple.Create(blkim, zim);
        }
        #endregion

        #region Tuple<Bitmap, Bitmap> RenderMapArea
        private Tuple<Bitmap, Bitmap> RenderMapArea(Tuple<int, int> start, Tuple<int, int> size)
        {
            Bitmap blkim = new Bitmap(ViewSizeH * BlockH, ViewSizeV * BlockV);
            Bitmap zim = new Bitmap(ViewSizeH * BlockH, ViewSizeV * BlockV);
            Console.WriteLine($"Rendering {size.Item1}x{size.Item2} block area, starting at ({start.Item1},{start.Item2})");
            for (int rblockv = start.Item2; rblockv < start.Item2 + size.Item2; rblockv++)
            {
                for (int rblockh = start.Item1; rblockh < start.Item1 + size.Item1; rblockh++)
                {
                    var (b, z) = RenderBlock(GetBlock(Tuple.Create(rblockh, rblockv)));
                    int imh = BlockH * (rblockh - start.Item1);
                    int imv = BlockV * (rblockv - start.Item2);
                    using (Graphics g = Graphics.FromImage(blkim))
                    {
                        g.DrawImage(b, imh, imv);
                    }
                    using (Graphics g = Graphics.FromImage(zim))
                    {
                        g.DrawImage(z, imh, imv);
                    }
                }
            }
            return Tuple.Create(blkim, zim);
        }
        #endregion

        #region buttonOpenTempGrafic
        private void buttonOpenTempGrafic_Click(object sender, EventArgs e)
        {
            // Get the path to the program directory
            string programDirectory = Application.StartupPath;

            // Define the path to the temporary directory in the program directory
            string directory = Path.Combine(programDirectory, "tempGrafic");

            // Check if the directory exists
            if (Directory.Exists(directory))
            {
                // Open the directory in the file explorer
                Process.Start("explorer.exe", directory);
            }
            else
            {
                // Display a message to the user indicating that the directory does not exist
                MessageBox.Show("The directory tempGraphic does not exist.");
            }
        }
        #endregion

        #region comboBoxMaps_SelectedIndexChanged
        private void comboBoxMaps_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Überprüfen Sie, ob ein Verzeichnis ausgewählt wurde
            if (!string.IsNullOrEmpty(textBoxLoad.Text))
            {
                string selectedMap = comboBoxMaps.SelectedItem.ToString();
                string filePath = Path.Combine(textBoxLoad.Text, selectedMap);
                if (File.Exists(filePath))
                {
                    // Leeren Sie die Puffer und laden Sie die neue Karte
                    mapBuffer = null;
                    radarColorBuffer = null;
                    LoadGameFileData(filePath);
                    LoadGameFileData(Path.Combine(textBoxLoad.Text, "radarcol.mul"));
                }
                else
                {
                    MessageBox.Show($"The file '{selectedMap}' does not exist in the selected directory.");
                }
            }
            else
            {
                MessageBox.Show("Please select a directory first.");
            }
        }
        #endregion
    }
}

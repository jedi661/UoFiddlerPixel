﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using static UoFiddler.Plugin.ConverterMultiTextPlugin.Forms.ARTMulIDXCreator;
using UoFiddler.Controls.UserControls;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class ARTMulIDXCreator : Form
    {
        public ARTMulIDXCreator()
        {
            InitializeComponent();
        }

        #region btCreateARTIDXMul
        private void BtCreateARTIDXMul_Click(object sender, EventArgs e)
        {
            var artIndexFile = new ArtIndexFile();

            long numEntries;
            if (!long.TryParse(textBox2.Text, out numEntries))
            {
                numEntries = 65500; // Default value if textBox2 is empty
            }

            for (long i = 0; i < numEntries; i++)
            {
                // Create an empty ArtIndexEntry and add it to artIndexFile
                var indexEntry = new ArtIndexEntry(0xFFFFFFFF, 0, 0);
                artIndexFile.AddEntry(indexEntry);
            }

            string artidxPath = textBox1.Text + "\\artidx.MUL";
            artIndexFile.SaveToFile(textBox1.Text + "\\artidx.MUL");

            // Create an empty art.mul file
            string artPath = textBox1.Text + "\\art.MUL";
            using (var fs = File.Create(textBox1.Text + "\\art.MUL")) { }

            // Update the label with the path of the created files
            lbCreatedMul.Text = $"The files were successfully created in: {artidxPath} and {artPath}";
        }
        #endregion

        #region btFileOrder
        private void BtFileOrder_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private void BtnCountEntries_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    var artIndexFile = new ArtIndexFile();
                    string filePath = Path.Combine(folderBrowserDialog.SelectedPath, "artidx.MUL");
                    artIndexFile.LoadFromFile(filePath); // Suppose you have a LoadFromFile method

                    int numEntries = artIndexFile.CountEntries();

                    lblEntryCount.Text = "The number of index entries is: " + numEntries;
                }
            }
        }
        #endregion

        #region btnShowInfo
        private void BtnShowInfo_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = folderBrowserDialog.SelectedPath;

                    var artIndexFile = new ArtIndexFile();
                    string filePath = Path.Combine(folderBrowserDialog.SelectedPath, "artidx.MUL");
                    artIndexFile.LoadFromFile(filePath); // Suppose you have a LoadFromFile method

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < artIndexFile.CountEntries(); i++)
                    {
                        var entry = artIndexFile.GetEntry(i); // Suppose you have a GetEntry method
                        sb.AppendLine($"Eintrag {i}: Lookup={entry.Lookup}, Size={entry.Size}, Unknown={entry.Unknown}");
                    }

                    textBoxInfo.Text = sb.ToString(); // Assume textBoxInfo is the TextBox where you want to display the information
                }
            }
        }
        #endregion

        #region Long 
        private void BtnReadArtIdx_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = folderBrowserDialog.SelectedPath;

                    var artIndexFile = new ArtIndexFile();
                    string filePath = Path.Combine(folderBrowserDialog.SelectedPath, "artidx.MUL");
                    artIndexFile.LoadFromFile(filePath); // Suppose you have a LoadFromFile method

                    int indexToRead;
                    if (!int.TryParse(textBoxIndex.Text, out indexToRead)) // Use TryParse instead of Parse
                    {
                        indexToRead = 0; // Set a default value if textBoxIndex is empty or does not contain a valid int value
                    }

                    if (indexToRead >= 0 && indexToRead < artIndexFile.CountEntries())
                    {
                        var entry = artIndexFile.GetEntry(indexToRead); // Suppose you have a GetEntry method
                        textBoxInfo.Text = $"Eintrag {indexToRead}: Lookup={entry.Lookup}, Size={entry.Size}, Unknown={entry.Unknown}"; // Assume textBoxInfo is the TextBox where you want to display the information
                    }
                    else
                    {
                        textBoxInfo.Text = "Invalid index";
                    }
                }
            }
        }
        #endregion

        #region Unit
        private void BtCreateARTIDXMul_uint_Click(object sender, EventArgs e)
        {
            var artIndexFile = new ArtIndexFile();

            uint numEntries;
            if (!uint.TryParse(textBox2.Text, out numEntries))
            {
                numEntries = 65500; // Default value if textBox2 is empty
            }

            for (uint i = 0; i < numEntries; i++)
            {
                // Create an empty ArtIndexEntry and add it to artIndexFile
                var indexEntry = new ArtIndexEntry(0xFFFFFFFF, 0, 0);
                artIndexFile.AddEntry(indexEntry);
            }

            string artidxPath = textBox1.Text + "\\artidx.MUL";
            artIndexFile.SaveToFile(textBox1.Text + "\\artidx.MUL");

            // Create an empty art.mul file
            string artPath = textBox1.Text + "\\art.MUL";
            using (var fs = File.Create(textBox1.Text + "\\art.MUL")) { }

            // Aktualisieren Sie das Label mit dem Pfad der erstellten Dateien
            lbCreatedMul.Text = $"The files were successfully created in: {artidxPath} and {artPath}";
        }
        #endregion

        #region int
        private void BtCreateARTIDXMul_Int_Click(object sender, EventArgs e)
        {
            var artIndexFile = new ArtIndexFile();

            int numEntries;
            if (!int.TryParse(textBox2.Text, out numEntries))
            {
                numEntries = 65500; // Default value if textBox2 is empty
            }

            for (int i = 0; i < numEntries; i++)
            {
                // Create an empty ArtIndexEntry and add it to artIndexFile
                var indexEntry = new ArtIndexEntry(0xFFFFFFFF, 0, 0);
                artIndexFile.AddEntry(indexEntry);
            }

            string artidxPath = textBox1.Text + "\\artidx.MUL";
            artIndexFile.SaveToFile(textBox1.Text + "\\artidx.MUL");

            // Create an empty art.mul file
            string artPath = textBox1.Text + "\\art.MUL";
            using (var fs = File.Create(textBox1.Text + "\\art.MUL")) { }

            // Update the label with the path of the created files
            lbCreatedMul.Text = $"The files were successfully created in: {artidxPath} and {artPath}";
        }
        #endregion

        #region Ushort
        private void BtCreateARTIDXMul_Ushort_Click(object sender, EventArgs e)
        {
            var artIndexFile = new ArtIndexFile();

            ushort numEntries;
            if (!ushort.TryParse(textBox2.Text, out numEntries))
            {
                numEntries = 65500; // Default value if textBox2 is empty
            }

            for (ushort i = 0; i < numEntries; i++)
            {
                // Create an empty ArtIndexEntry and add it to artIndexFile
                var indexEntry = new ArtIndexEntry(0xFFFFFFFF, 0, 0);
                artIndexFile.AddEntry(indexEntry);
            }

            string artidxPath = textBox1.Text + "\\artidx.MUL";
            artIndexFile.SaveToFile(textBox1.Text + "\\artidx.MUL");

            // Create an empty art.mul file
            string artPath = textBox1.Text + "\\art.MUL";
            using (var fs = File.Create(textBox1.Text + "\\art.MUL")) { }

            // Update the label with the path of the created files
            lbCreatedMul.Text = $"The files were successfully created in: {artidxPath} and {artPath}";
        }
        #endregion

        #region Short
        private void BtCreateARTIDXMul_Short_Click(object sender, EventArgs e)
        {
            var artIndexFile = new ArtIndexFile();

            short numEntries;
            if (!short.TryParse(textBox2.Text, out numEntries) || numEntries > 32767)
            {
                numEntries = 32767; // Default value if textBox2 is empty or contains a value greater than 32767
                textBox2.Text = numEntries.ToString(); // Update textBox2 with default value
            }

            for (short i = 0; i < numEntries; i++)
            {
                // Create an empty ArtIndexEntry and add it to artIndexFile
                var indexEntry = new ArtIndexEntry(0xFFFFFFFF, 0, 0);
                artIndexFile.AddEntry(indexEntry);
            }
            string artidxPath = textBox1.Text + "\\artidx.MUL";
            artIndexFile.SaveToFile(textBox1.Text + "\\artidx.MUL");

            // Create an empty art.mul file
            string artPath = textBox1.Text + "\\art.MUL";
            using (var fs = File.Create(textBox1.Text + "\\art.MUL")) { }

            // Update the label with the path of the created files
            lbCreatedMul.Text = $"The files were successfully created in: {artidxPath} and {artPath}";
        }
        #endregion

        #region Byte
        private void BtCreateARTIDXMul_Byte_Click(object sender, EventArgs e)
        {
            var artIndexFile = new ArtIndexFile();

            byte numEntries;
            if (!byte.TryParse(textBox2.Text, out numEntries) || numEntries > 255)
            {
                numEntries = 255; // Default value if textBox2 is empty or contains a value greater than 255
                textBox2.Text = numEntries.ToString(); // Update textBox2 with default value
            }

            for (byte i = 0; i < numEntries; i++)
            {
                // Create an empty ArtIndexEntry and add it to artIndexFile
                var indexEntry = new ArtIndexEntry(0xFFFFFFFF, 0, 0);
                artIndexFile.AddEntry(indexEntry);
            }

            string artidxPath = textBox1.Text + "\\artidx.MUL";
            artIndexFile.SaveToFile(textBox1.Text + "\\artidx.MUL");

            // Create an empty art.mul file
            string artPath = textBox1.Text + "\\art.MUL";
            using (var fs = File.Create(textBox1.Text + "\\art.MUL")) { }

            // Update the label with the path of the created files
            lbCreatedMul.Text = $"The files were successfully created in: {artidxPath} and {artPath}";
        }
        #endregion

        #region Ulong
        private void BtCreateARTIDXMul_Ulong_Click(object sender, EventArgs e)
        {
            var artIndexFile = new ArtIndexFile();

            ulong numEntries;
            if (!ulong.TryParse(textBox2.Text, out numEntries))
            {
                numEntries = 65500; // Default value if textBox2 is empty
            }

            for (ulong i = 0; i < numEntries; i++)
            {
                // Create an empty ArtIndexEntry and add it to artIndexFile
                var indexEntry = new ArtIndexEntry(0xFFFFFFFF, 0, 0);
                artIndexFile.AddEntry(indexEntry);
            }

            string artidxPath = textBox1.Text + "\\artidx.MUL";
            artIndexFile.SaveToFile(textBox1.Text + "\\artidx.MUL");

            // Create an empty art.mul file
            string artPath = textBox1.Text + "\\art.MUL";
            using (var fs = File.Create(textBox1.Text + "\\art.MUL")) { }

            // Update the label with the path of the created files
            lbCreatedMul.Text = $"The files were successfully created in: {artidxPath} and {artPath}";
        }
        #endregion

        #region Rename
        private void ComboBoxMuls_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedValue = ComboBoxMuls.SelectedItem.ToString();

            if (selectedValue == "Texture")
            {
                string directoryPath = textBox1.Text;

                string oldArtMulPath = Path.Combine(directoryPath, "Art.mul");
                string newArtMulPath = Path.Combine(directoryPath, "texmaps.mul");

                string oldArtIdxMulPath = Path.Combine(directoryPath, "Artidx.mul");
                string newArtIdxMulPath = Path.Combine(directoryPath, "texidx.mul");

                if (File.Exists(oldArtMulPath) && File.Exists(oldArtIdxMulPath))
                {
                    File.Move(oldArtMulPath, newArtMulPath);
                    File.Move(oldArtIdxMulPath, newArtIdxMulPath);

                    lbCreatedMul.Text = $"The files have been successfully renamed: {newArtMulPath} and {newArtIdxMulPath}";
                }
                else
                {
                    lbCreatedMul.Text = "The Art.mul and Artidx.mul files could not be found.";
                }
            }
        }
        #endregion

        #region CreateTiledata
        private void BtCreateTiledata_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    tbDirTileData.Text = fbd.SelectedPath;
                    TileDataCreator creator = new TileDataCreator();

                    // Read the values ​​from the text boxes or use default values
                    int landTileGroups = string.IsNullOrWhiteSpace(tblandTileGroups.Text) ? 16383 : int.Parse(tblandTileGroups.Text);
                    int staticTileGroups = string.IsNullOrWhiteSpace(tbstaticTileGroups.Text) ? 65535 : int.Parse(tbstaticTileGroups.Text);

                    // Create the full path to the file
                    string filePath = Path.Combine(fbd.SelectedPath, "Tiledata.mul");

                    // Check whether the checkbox is activated
                    bool createEmptyFile = checkBoxTileData.Checked;

                    creator.CreateTileData(filePath, landTileGroups, staticTileGroups, createEmptyFile);

                    // Update the label to indicate that the file was created successfully
                    lbTileDataCreate.Text = $"The Tiledata.mul file was successfully created in: {filePath}";
                }
            }
        }
        #endregion

        #region  Tiledatainfo
        private void BtTiledatainfo_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    tbDirTileData.Text = fbd.SelectedPath;

                    // Create the full path to the file
                    string filePath = Path.Combine(fbd.SelectedPath, "Tiledata.mul");

                    // Check if the file exists
                    if (!File.Exists(filePath))
                    {
                        textBoxTileDataInfo.Text = "The file Tiledata.mul could not be found.";
                        return;
                    }

                    // Read the data from the file
                    using (var fs = File.OpenRead(filePath))
                    {
                        using (var reader = new BinaryReader(fs))
                        {
                            // Read the number of Land Tile Groups
                            int landTileGroups = reader.ReadInt32();

                            // Read the number of Static Tile Groups
                            int staticTileGroups = reader.ReadInt32();

                            // Display the information in the text box
                            textBoxTileDataInfo.Text = $"Land Tile Groups: {landTileGroups}\nStatic Tile Groups: {staticTileGroups}";
                        }
                    }
                }
            }
        }
        #endregion

        #region Create Button Empty
        private void BtCreateTiledataEmpty_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    tbDirTileData.Text = fbd.SelectedPath;
                    TileDataCreator creator = new TileDataCreator();

                    // Read the values ​​from the text boxes or use default values
                    int landTileGroups = string.IsNullOrWhiteSpace(tblandTileGroups.Text) ? 16383 : int.Parse(tblandTileGroups.Text);
                    int staticTileGroups = string.IsNullOrWhiteSpace(tbstaticTileGroups.Text) ? 65535 : int.Parse(tbstaticTileGroups.Text);

                    // Create the full path to the file
                    string filePath = Path.Combine(fbd.SelectedPath, "Tiledata.mul");

                    // Create an empty Tiledata.mul file
                    creator.CreateTileData(filePath, landTileGroups, staticTileGroups, true);

                    // Update the label to indicate that the file was created successfully
                    lbTileDataCreate.Text = $"The empty Tiledata.mul file has been successfully created in: {filePath}";
                }
            }
        }
        #endregion

        #region Crete Button Emtpy2
        private void BtCreateTiledataEmpty2_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    TileDataCreator creator = new TileDataCreator();

                    // Set the number of land and static tile groups to default values
                    int landTileGroups = 16383;
                    int staticTileGroups = 65535;

                    // Create the full path to the file
                    string filePath = Path.Combine(fbd.SelectedPath, "Tiledata.mul");

                    // Create an empty Tiledata.mul file
                    creator.CreateTileData(filePath, landTileGroups, staticTileGroups, true);

                    // Display a message to indicate that the file was created successfully
                    MessageBox.Show($"The empty Tiledata.mul file has been successfully created in: {filePath}");
                }
            }
        }
        #endregion

        #region ReadIndexTiledata
        private void BtReadIndexTiledata_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    TileDataCreator creator = new TileDataCreator();
                    string filePath = Path.Combine(fbd.SelectedPath, "Tiledata.mul");
                    int index = int.Parse(textBoxTiledataIndex.Text);
                    //string tileDataInfo = creator.ReadTileData(filePath, index);
                    string tileDataInfo = creator.ReadTileData(filePath, index, "Land"); // To read Land Tiles
                    string tileDataInfo2 = creator.ReadTileData(filePath, index, "Static"); // For reading static tiles

                    textBoxTileDataInfo.Text = tileDataInfo;
                }
            }
        }
        #endregion

        #region Button Hex
        public void BtTReadHexAndSelectDirectory_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string filePath = Path.Combine(fbd.SelectedPath, "Tiledata.mul");

                    // Check that the index is in hexadecimal form
                    bool isHex = textBoxTiledataIndex.Text.StartsWith("0x");
                    int index;
                    if (isHex)
                    {
                        // If the index is in hexadecimal form, convert it to decimal
                        index = Convert.ToInt32(textBoxTiledataIndex.Text.Substring(2), 16);
                    }
                    else
                    {
                        index = int.Parse(textBoxTiledataIndex.Text);
                    }

                    // Multiply the index by the size of the block to get the correct position in the file
                    index *= 836; // Replace 836 with the actual size of the block

                    using (var fs = File.OpenRead(filePath))
                    {
                        fs.Position = index;
                        byte[] buffer = new byte[836]; // Read 836 bytes from the specified position
                        fs.Read(buffer, 0, buffer.Length);
                        string hex = BitConverter.ToString(buffer).Replace("-", " "); // Convert the bytes to a hexadecimal string

                        // Convert the bytes to ASCII characters
                        string ascii = Encoding.ASCII.GetString(buffer);

                        textBoxTileDataInfo.Text = $"The bytes at the position {index} in the file {filePath} are:\n{hex}\n\nASCII:\n{ascii}";
                    }
                }
            }
        }
        #endregion

        #region Button Land
        public void BtReadLandTile_Click(object sender, EventArgs e)
        {
            ReadTileData("Land");
        }
        #endregion

        #region Button Static
        public void BtReadStaticTile_Click(object sender, EventArgs e)
        {
            ReadTileData("Static");
        }
        #endregion

        #region ReadTileData
        public void ReadTileData(string tileType)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    TileDataCreator creator = new TileDataCreator();
                    string filePath = Path.Combine(fbd.SelectedPath, "Tiledata.mul");
                    int index = int.Parse(textBoxTiledataIndex.Text);
                    //string tileDataInfo = creator.ReadTileData(filePath, index, tileType);
                    string tileDataInfo = creator.ReadTileData(filePath, index, tileType);
                    textBoxTileDataInfo.Text = tileDataInfo;
                }
            }
        }
        #endregion

        #region btnCountTileDataEntries
        private void BtnCountTileDataEntries_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    var artDataFile = new ArtDataFile();
                    string filePath = Path.Combine(folderBrowserDialog.SelectedPath, "Tiledata.mul");
                    artDataFile.LoadFromFileForCounting(filePath);
                    int numEntries = artDataFile.CountEntries();
                    lblTileDataEntryCount.Text = "The number of entries is: " + numEntries;
                }
            }
        }
        #endregion

        #region Button SimpleTiledata
        private void BtCreateSimpleTiledata_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string filePath = Path.Combine(fbd.SelectedPath, "Tiledata.mul");

                    // Check if the file already exists. If so, delete them.
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    // Create the file.
                    using (var fs = File.Create(filePath))
                    {
                        using (var writer = new BinaryWriter(fs))
                        {
                            int landTileGroups = string.IsNullOrWhiteSpace(tblandTileGroups.Text) ? 16383 : int.Parse(tblandTileGroups.Text);
                            int staticTileGroups = string.IsNullOrWhiteSpace(tbstaticTileGroups.Text) ? 65535 : int.Parse(tbstaticTileGroups.Text);

                            for (int i = 0; i < landTileGroups + staticTileGroups; i++)
                            {
                                var tileData = new SimpleTileData("Tile" + i, (uint)i, (uint)(i % 32));
                                tileData.WriteToStream(writer);
                            }
                        }
                    }

                    MessageBox.Show($"The Tiledata.mul file was successfully created in: {filePath}");
                }
            }
        }
        #endregion

        #region ReadTileFlags Button
        private void BtReadTileFlags_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    tbDirTileData.Text = fbd.SelectedPath;

                    // Create the full path to the file
                    string filePath = Path.Combine(fbd.SelectedPath, "Tiledata.mul");

                    // Check if the file exists
                    if (!File.Exists(filePath))
                    {
                        textBoxTileDataInfo.Text = "The file Tiledata.mul could not be found.";
                        return;
                    }

                    // Read the data from the file
                    using (var fs = File.OpenRead(filePath))
                    {
                        using (var reader = new BinaryReader(fs))
                        {
                            // Read the number of Land Tile Groups
                            int landTileGroups = reader.ReadInt32();

                            // Read the number of Static Tile Groups
                            int staticTileGroups = reader.ReadInt32();

                            // Create a new instance of TileDataFlags
                            TileDataFlags flags = new TileDataFlags();

                            // Read the flags for each tile
                            while (reader.BaseStream.Position != reader.BaseStream.Length)
                            {
                                // Read the flags from the file
                                ulong flagValue = reader.ReadUInt64();

                                // Interpret the flags using the TileDataFlags class
                                flags.Value = flagValue;

                                // Add the flags to a list
                                List<ulong> flagList = new List<ulong>();
                                flagList.Add(flagValue);

                                // Use the flags here...
                                foreach (ulong flag in flagList)
                                {
                                    // Process each flag...
                                }
                            }

                            // Display the information in the text box
                            textBoxTileDataInfo.Text = $"Land Tile Groups: {landTileGroups}\nStatic Tile Groups: {staticTileGroups}";
                        }
                    }
                }
            }
        }
        #endregion

        #region Class ArtIndexEntry
        public class ArtIndexEntry
        {
            public uint Lookup { get; set; }
            public uint Size { get; set; }
            public uint Unknown { get; set; }

            public Bitmap Image { get; set; }

            #region ArtIndexEntry
            public ArtIndexEntry(uint lookup, uint size, uint unknown)
            {
                Lookup = lookup;
                Size = size;
                Unknown = unknown;
            }
            #endregion

            #region WriteToStream
            public void WriteToStream(BinaryWriter writer)
            {
                writer.Write(Lookup);
                writer.Write(Size);
                writer.Write(Unknown);
            }
            #endregion
        }
        #endregion

        #region class ArtIndexFile
        public class ArtIndexFile
        {
            private List<ArtIndexEntry> _entries;

            #region
            public ArtIndexFile()
            {
                _entries = new List<ArtIndexEntry>();
            }
            #endregion

            #region ArtIndexEntry GetEntry
            public ArtIndexEntry GetEntry(int index)
            {
                if (index >= 0 && index < _entries.Count)
                {
                    return _entries[index];
                }
                else
                {
                    return null; // or throw an exception
                }
            }
            #endregion

            #region
            public void AddEntry(ArtIndexEntry entry)
            {
                _entries.Add(entry);
            }
            #endregion

            #region LoadFromFile
            public void LoadFromFile(string filename)
            {
                _entries.Clear(); // Delete all existing entries

                using (var fs = new FileStream(filename, FileMode.Open))
                {
                    using (var reader = new BinaryReader(fs))
                    {
                        while (reader.BaseStream.Position != reader.BaseStream.Length) // Read to the end of the file
                        {
                            uint lookup = reader.ReadUInt32();
                            uint size = reader.ReadUInt32();
                            uint unknown = reader.ReadUInt32();

                            var entry = new ArtIndexEntry(lookup, size, unknown);
                            _entries.Add(entry);
                        }
                    }
                }
            }
            #endregion

            #region CountEntries
            public int CountEntries()
            {
                return _entries.Count;
            }
            #endregion

            #region  SaveToFile
            public void SaveToFile(string filename)
            {
                using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        foreach (var entry in _entries)
                        {
                            entry.WriteToStream(writer);
                        }
                    }
                }
            }
            #endregion
        }
        #endregion

        #region class ArtDataEntry
        public class ArtDataEntry
        {
            public Bitmap Image { get; set; }

            public ArtDataEntry(Bitmap image)
            {
                Image = image;
            }

            #region  WriteToStream
            public void WriteToStream(BinaryWriter writer)
            {
                // Convert the image to a byte array
                byte[] imageData = ImageToByteArray(Image);

                // Write the image data to the stream
                writer.Write(imageData);
            }
            #endregion
            #region
            private byte[] ImageToByteArray(Bitmap image)
            {
                using (var stream = new MemoryStream())
                {
                    image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    return stream.ToArray();
                }
            }
            #endregion
        }
        #endregion

        #region SimpleTileData
        public class SimpleTileData
        {
            public string Name { get; set; }
            public uint Value { get; set; }
            public uint Flags { get; set; }

            public SimpleTileData(string name, uint value, uint flags)
            {
                Name = name;
                Value = value;
                Flags = flags;
            }

            public void WriteToStream(BinaryWriter writer)
            {
                // Write the Name to the stream
                writer.Write(Name);

                // Write the Value to the stream
                writer.Write(Value);

                // Write the Flags to the stream
                writer.Write(Flags);
            }
        }
        #endregion

        #region TiledataFlags
        public class TileDataFlags
        {
            private Dictionary<string, ulong> _flagNameMasks = new Dictionary<string, ulong>
            {
                {"background", 0x1},
                {"weapon", 0x2},
                {"transparent", 0x4},
                {"translucent", 0x8},
                {"wall", 0x10},
                {"damaging", 0x20},
                {"impassable", 0x40},
                {"wet", 0x80},
                {"unknown", 0x100},
                {"surface", 0x200},
                {"climbable", 0x400},
                {"stackable", 0x800},
                {"window", 0x1000},
                {"noShoot", 0x2000},
                {"articleA", 0x4000},
                {"articleAn", 0x8000},
                {"internal", 0x10000},
                {"foliage", 0x20000},
                {"partialHue", 0x40000},
                {"unknown1", 0x80000},
                {"map", 0x100000},
                {"container", 0x200000},
                {"wearable", 0x400000},
                {"lightSource", 0x800000},
                {"animation", 0x1000000},
                {"noDiagonal", 0x2000000},
                {"unknown2", 0x4000000},
                {"armor", 0x8000000},
                {"roof", 0x10000000},
                {"door", 0x20000000},
                {"stairBack", 0x40000000},
                {"stairRight", 0x80000000},
                {"noShadow", 0x100000000},
                {"pixelBleed", 0x200000000},
                {"playAnimOnce", 0x400000000},
                {"multiMovable", 0x800000000},
                {"unknown3", 0x1000000000},
                {"fullBright", 0x2000000000},
                {"unknown4", 0x4000000000},
                {"unknown5", 0x8000000000},
                {"unknown6", 0x10000000000},
                {"unknown7", 0x20000000000},
                {"unknown8", 0x40000000000},
                {"hoverOver", 0x80000000000},
                {"unknown9", 0x100000000000},
                {"unknown10", 0x200000000000},
                {"unknown11", 0x400000000000},
                {"unknown12", 0x800000000000},
                {"gumpArt", 0x1000000000000},
                {"gumpArtUsed", 0x2000000000000},
                {"unknown13", 0x4000000000000},
                {"tileArt", 0x8000000000000},
                {"tileArtUsed", 0x10000000000000},
                {"unknown14", 0x20000000000000},
                {"unknown15", 0x40000000000000},
                {"unknown16", 0x80000000000000},
                {"unknown17", 0x100000000000000},
                {"unknown18", 0x200000000000000},
                {"unknown19", 0x400000000000000},
                {"unknown20", 0x800000000000000},
                {"unknown21", 0x1000000000000000},
                {"unknown22", 0x2000000000000000},
                {"unknown23", 0x4000000000000000},
                {"unknown24", 0x8000000000000000}
            };

            private ulong _value;
            public ulong Value { get; set; }

            public TileDataFlags()
            {
                // Initialize your flags here if necessary
            }

            public TileDataFlags(ulong flagValue)
            {
                this.Value = flagValue;
            }

            public TileDataFlags(string flagValue)
            {
                // Break the input string into individual flag names
                var flagNames = flagValue.Split(',');

                // Loop through each flag name
                foreach (var flagName in flagNames)
                {
                    // Remove spaces
                    var trimmedFlagName = flagName.Trim();

                    // Check whether the flag name is valid
                    if (_flagNameMasks.ContainsKey(trimmedFlagName))
                    {
                        // If yes, set the appropriate one Bit in `value`
                        _value |= _flagNameMasks[trimmedFlagName];
                    }
                    else
                    {
                        // If not, throw an exception or handle the error appropriately
                        throw new ArgumentException("Invalid flag name: " + trimmedFlagName);
                    }
                }
            }


            public ulong MaskForName(string flagName)
            {
                if (_flagNameMasks.TryGetValue(flagName, out ulong mask))
                {
                    return mask;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Flag name: " + flagName + " not valid");
                }
            }

            public void SetBit(int bit, bool value)
            {
                ulong mask = (ulong)1 << bit;
                if (value)
                {
                    this._value |= mask;
                }
                else
                {
                    this._value &= ~mask;
                }
            }

            public bool BitValue(int bit)
            {
                ulong mask = (ulong)1 << bit;
                return (_value & mask) != 0;
            }
        }
        #endregion

        #region class ArtDataFile
        public class ArtDataFile
        {
            private List<ArtDataEntry> _entries;

            public ArtDataFile()
            {
                _entries = new List<ArtDataEntry>();
            }

            public ArtDataEntry GetEntry(int index)
            {
                if (index >= 0 && index < _entries.Count)
                {
                    return _entries[index];
                }
                else
                {
                    return null; // or throw an exception
                }
            }

            #region CountEntries
            public int CountEntries()
            {
                return _entries.Count;
            }
            #endregion           

            #region LoadFromFile
            public void LoadFromFile(string filename)
            {
                _entries.Clear(); // Delete all existing entries

                using (var fs = new FileStream(filename, FileMode.Open))
                {
                    using (var reader = new BinaryReader(fs))
                    {
                        while (reader.BaseStream.Position != reader.BaseStream.Length) // Read to the end of the file
                        {
                            // Read the metadata for the image
                            uint lookup = reader.ReadUInt32();
                            uint size = reader.ReadUInt32();
                            uint unknown = reader.ReadUInt32();

                            // Set the position of the reader to the beginning of the image
                            reader.BaseStream.Position = lookup;

                            // Read the image data from the file
                            byte[] imageData = reader.ReadBytes((int)size);

                            // Check if there is enough data for the flag
                            if (imageData.Length >= 4)
                            {
                                // Read the flag
                                uint flag = BitConverter.ToUInt32(imageData, 0);

                                Bitmap image;
                                if (flag > 0xFFFF || flag == 0)
                                {
                                    // The image is raw
                                    image = LoadRawImage(imageData);
                                }
                                else
                                {
                                    // The image is a moving image
                                    image = LoadRunImage(imageData);
                                }

                                var entry = new ArtDataEntry(image);
                                _entries.Add(entry);
                            }
                            else
                            {
                                // Handle the error
                            }
                        }
                    }
                }
            }
            #endregion            

            #region LoadRawImage
            private Bitmap LoadRawImage(byte[] imageData)
            {
                int width = 44;
                int height = 44;
                Bitmap image = new Bitmap(width, height);
                int index = 4; // skip the flag

                for (int i = 0; i < height; i++)
                {
                    int rowWidth = 2 + i;
                    if (rowWidth > width) rowWidth = width;

                    for (int j = 0; j < rowWidth; j++)
                    {
                        // Check whether the index is within the valid range
                        if (index + 2 <= imageData.Length)
                        {
                            ushort pixelData = BitConverter.ToUInt16(imageData, index);
                            Color pixelColor = Color.FromArgb(
                                ((pixelData >> 10) & 0x1F) * 0xFF / 0x1F,
                                ((pixelData >> 5) & 0x1F) * 0xFF / 0x1F,
                                (pixelData & 0x1F) * 0xFF / 0x1F);
                            image.SetPixel(j, i, pixelColor);
                            index += 2;
                        }
                        else
                        {
                            // Handle the error                            
                        }
                    }
                }

                return image;
            }
            #endregion

            #region LoadRunImage
            private Bitmap LoadRunImage(byte[] imageData)
            {
                int width = BitConverter.ToUInt16(imageData, 4);
                int height = BitConverter.ToUInt16(imageData, 6);
                Bitmap image = new Bitmap(width, height);
                int index = 8 + height * 2; // skip the flag, width, height and LStart

                int x = 0;
                int y = 0;

                while (y < height)
                {
                    ushort xOffs = BitConverter.ToUInt16(imageData, index);
                    ushort run = BitConverter.ToUInt16(imageData, index + 2);

                    index += 4;

                    if (xOffs + run != 0)
                    {
                        x += xOffs;

                        for (int i = 0; i < run; i++)
                        {
                            ushort pixelData = BitConverter.ToUInt16(imageData, index);
                            Color pixelColor = Color.FromArgb(
                                ((pixelData >> 10) & 0x1F) * 0xFF / 0x1F,
                                ((pixelData >> 5) & 0x1F) * 0xFF / 0x1F,
                                (pixelData & 0x1F) * 0xFF / 0x1F);
                            image.SetPixel(x + i, y, pixelColor);

                            index += 2;
                        }

                        x += run;
                    }
                    else
                    {
                        x = 0;
                        y++;
                        index = 8 + height * 2 + BitConverter.ToUInt16(imageData, 8 + y * 2) * 2;
                    }
                }

                return image;
            }
            #endregion

            #region AddEntry
            public void AddEntry(ArtDataEntry entry)
            {
                _entries.Add(entry);
            }
            #endregion

            #region GetImage
            public Bitmap GetImage(int index)
            {
                if (index >= 0 && index < _entries.Count)
                {
                    return _entries[index].Image;
                }
                else
                {
                    return null; // or throw an exception
                }
            }
            #endregion

            #region SaveToFile
            public void SaveToFile(string filename)
            {
                using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        foreach (var entry in _entries)
                        {
                            entry.WriteToStream(writer);
                        }
                    }
                }
            }
            #endregion

            #region LoadFromFileForCounting
            public void LoadFromFileForCounting(string filename)
            {
                _entries.Clear(); // Delete all existing entries

                using (var fs = new FileStream(filename, FileMode.Open))
                {
                    long fileSize = fs.Length;
                    int landTileGroupSize = 836; // Size of a Land Tile Group
                    int staticTileGroupSize = 1188; // Size of a Static Tile Group

                    // Calculate the number of Land Tile Groups
                    int numLandTileGroups = 0;
                    while (fs.Position + landTileGroupSize <= fileSize)
                    {
                        fs.Position += landTileGroupSize; // Skip Land Tile Group
                        _entries.Add(new ArtDataEntry(null));
                        numLandTileGroups++;
                    }

                    // Calculate the number of Static Tile Groups
                    int numStaticTileGroups = 0;
                    while (fs.Position + staticTileGroupSize <= fileSize)
                    {
                        fs.Position += staticTileGroupSize; // Skip Static Tile Group
                        _entries.Add(new ArtDataEntry(null));
                        numStaticTileGroups++;
                    }

                    Console.WriteLine("Number of Land Tile Groups: " + numLandTileGroups);
                    Console.WriteLine("Number of Static Tile Groups: " + numStaticTileGroups);
                }
                #endregion
            }
        }
        #endregion

        #region Class Tiledata   
        public class TileDataCreator
        {
            #region  CreateTileData
            public void CreateTileData(string filePath, int landTileGroups, int staticTileGroups, bool createEmptyFile)
            {
                // Check if the file already exists. If so, delete them.
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Create the file.
                using (var fs = File.Create(filePath))
                {
                    for (int i = 0; i < landTileGroups; i++)
                    {
                        // Write the Land Tile Group to the file.
                        WriteLandTileGroup(fs, createEmptyFile);
                    }

                    for (int i = 0; i < staticTileGroups; i++)
                    {
                        // Write the Static Tile Group to the file.
                        WriteStaticTileGroup(fs, createEmptyFile);
                    }
                }
            }
            #endregion

            #region WriteLandTileGroup
            private void WriteLandTileGroup(FileStream fs, bool createEmptyFile)
            {
                using (var writer = new BinaryWriter(fs, Encoding.Default, true))
                {
                    writer.Write((uint)0); // Unknown DWORD

                    for (int i = 0; i < 32; i++)
                    {
                        if (createEmptyFile)
                        {
                            writer.Write(new string('\0', 26)); // Empty Land Tile Data
                        }
                        else
                        {
                            writer.Write((uint)0); // Flags
                            writer.Write((ushort)0); // Texture ID
                            writer.Write(new string('\0', 20)); // Tile Name
                        }
                    }
                }
            }
            #endregion

            #region WriteStaticTileGroup
            private void WriteStaticTileGroup(FileStream fs, bool createEmptyFile)
            {
                using (var writer = new BinaryWriter(fs, Encoding.Default, true))
                {
                    writer.Write((uint)0); // Unknown DWORD

                    for (int i = 0; i < 32; i++)
                    {
                        if (createEmptyFile)
                        {
                            writer.Write(new string('\0', 37)); // Empty Static Tile Data
                        }
                        else
                        {
                            writer.Write((uint)0); // Flags
                            writer.Write((byte)0); // Weight
                            writer.Write((byte)0); // Quality
                            writer.Write((ushort)0); // Unknown
                            writer.Write((byte)0); // Unknown1
                            writer.Write((byte)0); // Quantity
                            writer.Write((ushort)0); // Anim ID
                            writer.Write((byte)0); // Unknown2
                            writer.Write((byte)0); // Hue
                            writer.Write((ushort)0); // Unknown3
                            writer.Write((byte)0); // Height
                            writer.Write(new string('\0', 20)); // Tile Name
                        }
                    }
                }
            }
            #endregion

            #region ReadTileData2(
            // not used
            public string ReadTileData2(string filePath, int index)
            {
                using (var fs = File.OpenRead(filePath))
                {
                    // Calculate the position of the specified index in the file
                    long position = index * 26; // Each tile is 26 bytes in size

                    // Make sure the location is within the file
                    if (position < fs.Length)
                    {
                        fs.Position = position;

                        using (var reader = new BinaryReader(fs))
                        {
                            uint flags = reader.ReadUInt32();
                            byte weight = reader.ReadByte();
                            byte quality = reader.ReadByte();
                            ushort unknown = reader.ReadUInt16();
                            byte unknown1 = reader.ReadByte();
                            byte quantity = reader.ReadByte();
                            ushort animId = reader.ReadUInt16();
                            byte unknown2 = reader.ReadByte();
                            byte hue = reader.ReadByte();
                            ushort unknown3 = reader.ReadUInt16();
                            byte height = reader.ReadByte();
                            char[] tileName = reader.ReadChars(20);

                            // Convert the flags into a sequence of bit values
                            string flagString = Convert.ToString(flags, 2).PadLeft(32, '0');

                            // Generate the output
                            StringBuilder output = new StringBuilder($"0x{index:X4};{new string(tileName)};{weight};{quality};0x{unknown:X4};{unknown1};{quantity};0x{animId:X4};{unknown2};{hue};0x{unknown3:X4};{height};{flagString}");

                            return output.ToString();
                        }
                    }
                    else
                    {
                        return $"Index {index} is out of range.";
                    }
                }
            }
            #endregion

            #region  ReadTileData
            public string ReadTileData(string filePath, int index, string tileType)
            {
                using (var fs = File.OpenRead(filePath))
                {
                    // Calculate the position of the specified index in the file
                    long position;
                    if (tileType == "Land")
                    {
                        position = index * 26; // Each country tile is 26 bytes in size
                    }
                    else // Static
                    {
                        position = index * 37; // Each static tile is 37 bytes in size
                    }

                    // Make sure the location is within the file
                    if (position < fs.Length)
                    {
                        fs.Position = position;

                        using (var reader = new BinaryReader(fs))
                        {
                            uint flags = reader.ReadUInt32();
                            // ... (remaining code for reading other attributes)...

                            // Convert the flags into a sequence of bit values
                            string flagString = Convert.ToString(flags, 2).PadLeft(32, '0');

                            // Generate the output
                            StringBuilder output = new StringBuilder($"0x{index:X4}; ... ;{flagString}");

                            return output.ToString();
                        }
                    }
                    else
                    {
                        return $"Index {index} is out of range.";
                    }
                }
            }
            #endregion

        }
        #endregion

        #region Class Tiledata2 !!
        public class TileData2
        {
            private string _directory;
            private string _filePath;

            public TileData2(string directory)
            {
                this._directory = directory;
                this._filePath = Path.Combine(this._directory, "TileData.mul");
            }

            public void CreateTileData(int landBlockCount, int staticBlockCount)
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(this._filePath, FileMode.Create)))
                {
                    for (int i = 0; i < landBlockCount; i++)
                    {
                        WriteLandBlock(writer, i);
                    }
                    for (int i = 0; i < staticBlockCount; i++)
                    {
                        WriteStaticBlock(writer);
                    }
                }
            }

            private void WriteLandBlock(BinaryWriter writer, int index)
            {
                writer.Write(index);  // header
                for (int i = 0; i < 32; i++)
                {
                    writer.Write(i);  // flags
                    writer.Write((short)i);  // TextureID
                    writer.Write(Encoding.ASCII.GetBytes("name"));  // name
                }
            }

            private void WriteStaticBlock(BinaryWriter writer)
            {
                writer.Write(0);  // header
                for (int i = 0; i < 32; i++)
                {
                    writer.Write(i);  // flags
                    writer.Write((byte)i);  // weight
                    writer.Write((byte)i);  // quality
                    writer.Write((short)i);  // unknown1
                    writer.Write((byte)i);  // unknown2
                    writer.Write((byte)i);  // quantity
                    writer.Write((short)i);  // animation
                    writer.Write((byte)i);  // unknown3
                    writer.Write((byte)i);  // hue
                    writer.Write((byte)i);  // unknown4
                    writer.Write((byte)i);  // unknown5
                    writer.Write((byte)i);  // height
                    writer.Write(Encoding.ASCII.GetBytes("name"));  // name
                }
            }
            public int CountIndices()
            {
                int count = 0;
                using (BinaryReader reader = new BinaryReader(File.Open(this._filePath, FileMode.Open)))
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        if (reader.BaseStream.Length - reader.BaseStream.Position >= 4)
                        {
                            reader.ReadInt32();  // header
                            for (int i = 0; i < 32; i++)
                            {
                                if (reader.BaseStream.Length - reader.BaseStream.Position >= 26)
                                {
                                    reader.ReadInt32();  // flags
                                    reader.ReadInt16();  // TextureID or animation
                                    reader.ReadBytes(20);  // name
                                    count++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                return count;
            }

        }
        #endregion

        #region class Texture
        public class TextureFileCreator
        {
            private string _texMapsFileName;
            private string _texIdxFileName;

            public TextureFileCreator(string texMapsFileName, string texIdxFileName)
            {
                this._texMapsFileName = texMapsFileName;
                this._texIdxFileName = texIdxFileName;
            }

            public void CreateFiles(int extra, short[] imageColors)
            {
                int width = (extra == 1 ? 128 : 64);
                int height = width;

                // Create TexMaps.mul file
                using (BinaryWriter texMapsFile = new BinaryWriter(File.Open(_texMapsFileName, FileMode.Create)))
                {
                    foreach (short color in imageColors)
                    {
                        texMapsFile.Write(color);
                    }
                }

                // Create TexIdx.mul file
                using (BinaryWriter texIdxFile = new BinaryWriter(File.Open(_texIdxFileName, FileMode.Create)))
                {
                    texIdxFile.Write(width);
                    texIdxFile.Write(height);
                    texIdxFile.Write(extra);
                }
            }
        }
        #endregion

        #region Class RadarColors
        public class RadarColors
        {
            private short[] _colors;

            public RadarColors(string filePath)
            {
                _colors = new short[0x8000];
                LoadColorsFromFile(filePath);
            }

            private void LoadColorsFromFile(string filePath)
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"The file {filePath} was not found.");
                }

                using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
                {
                    for (int i = 0; i < _colors.Length; i++)
                    {
                        _colors[i] = reader.ReadInt16();
                    }
                }
            }

            public short GetColor(int index)
            {
                if (index < 0 || index >= _colors.Length)
                {
                    throw new IndexOutOfRangeException("The index is out of range.");
                }

                return _colors[index];
            }
        }

        #endregion

        #region Class Palette
        public class Palette
        {
            public class RGBValue
            {
                public byte R;
                public byte G;
                public byte B;
            }

            public List<RGBValue> RGBValues = new List<RGBValue>();

            public void Save(string filename)
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.Create)))
                {
                    foreach (var rgb in RGBValues)
                    {
                        writer.Write(rgb.R);
                        writer.Write(rgb.G);
                        writer.Write(rgb.B);
                    }
                }
            }

            public void CreateDefaultPalette()
            {
                for (int i = 0; i < 256; i++)
                {
                    RGBValue rgb = new RGBValue();
                    // Here we set the R, G and B values ​​to i, 
                    // However, you can change these values ​​as you wish.
                    rgb.R = (byte)i;
                    rgb.G = (byte)i;
                    rgb.B = (byte)i;
                    RGBValues.Add(rgb);
                }
            }

            public void Load(string filename)
            {
                using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open)))
                {
                    for (int i = 0; i < 256; i++)
                    {
                        RGBValue rgb = new RGBValue();
                        rgb.R = reader.ReadByte();
                        rgb.G = reader.ReadByte();
                        rgb.B = reader.ReadByte();
                        RGBValues.Add(rgb);
                    }
                }
            }

            public void DrawPalette(Graphics g)
            {
                for (int i = 0; i < RGBValues.Count; i++)
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(RGBValues[i].R, RGBValues[i].G, RGBValues[i].B)))
                    {
                        g.FillRectangle(brush, i * 10, 0, 10, 10);
                    }
                }
            }
            public void AddColor(Color color)
            {
                RGBValue rgb = new RGBValue();
                rgb.R = color.R;
                rgb.G = color.G;
                rgb.B = color.B;
                RGBValues.Add(rgb);
            }

            public void DrawPalette(Graphics g, int width, int height)
            {
                int colorsPerRow = width / 10;
                for (int i = 0; i < RGBValues.Count; i++)
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(RGBValues[i].R, RGBValues[i].G, RGBValues[i].B)))
                    {
                        int x = (i % colorsPerRow) * 10;
                        int y = (i / colorsPerRow) * 10;
                        g.FillRectangle(brush, x, y, 10, 10);
                    }
                }
            }


        }
        #endregion

        #region Class Animation
        public class AnimationGroup
        {
            public ushort[] Palette { get; set; }
            public uint FrameCount { get; set; }
            public uint[] FrameOffset { get; set; }
            public Frame[] Frames { get; set; }

            public AnimationGroup()
            {
                Palette = new ushort[256];
                FrameCount = 0;
                FrameOffset = new uint[0];
                Frames = new Frame[0];
            }
        }

        public class Frame
        {
            public ushort ImageCenterX { get; set; }
            public ushort ImageCenterY { get; set; }
            public ushort Width { get; set; }
            public ushort Height { get; set; }
            public DataStream Data { get; set; }

            public Frame()
            {
                // Initialize the properties accordingly
            }
        }

        public class DataStream
        {
            public ushort RowHeader { get; set; }
            private ushort _rowOfs;

            public DataStream()
            {
                // Initialize the properties accordingly
            }

            public int RunLength
            {
                get
                {
                    return RowHeader & 0xFFF;
                }
            }

            public int LineNum
            {
                get
                {
                    return RowHeader >> 12;
                }
            }

            public int Unknown
            {
                get
                {
                    return _rowOfs & 0x3F;
                }
            }

            public int RowOfs
            {
                get
                {
                    return _rowOfs >> 6;
                }
            }
        }
        #endregion

        #region class AnimData
        public class AnimData
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 26, Pack = 1)]
            private unsafe struct OldLandData
            {
                private uint _flags;
                private ushort _texID;
                public fixed byte _Name[20]; // Change the access modifier to public
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 37, Pack = 1)]
            private unsafe struct OldItemData
            {
                private uint _flags;
                private byte _weight;
                private byte _quality;
                private ushort _miscdata;
                private byte _unk1;
                private byte _quantity;
                private ushort _animation;
                private byte _unk2;
                private byte _hue;
                private byte _stackingOff;
                private byte _value;
                private byte _height;
                public fixed byte _Name[20]; // Change the access modifier to public
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 41, Pack = 1)]
            private unsafe struct NewItemData
            {
                [MarshalAs(UnmanagedType.U8)]
                private ulong _flags;
                private byte _weight;
                private byte _quality;
                private ushort _miscdata;
                private byte _unk1;
                private byte _quantity;
                private ushort _animation;
                private byte _unk2;
                private byte _hue;
                private byte _stackingOff;
                private byte _value;
                private byte _height;
                public fixed byte _Name[20]; // Change the access modifier to public
            }

            public unsafe void ReadAndDisplayData(string filePath, System.Windows.Forms.TextBox textBox) // Add the keyword unsafe
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var reader = new BinaryReader(stream);
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        var data = new OldItemData();
                        var bytes = reader.ReadBytes(Marshal.SizeOf(typeof(OldItemData)));
                        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                        data = (OldItemData)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(OldItemData));
                        handle.Free();

                        // Suppose GetName() is a function that retrieves the name of the article from the raw data
                        string name = GetName(data._Name);
                        textBox.AppendText(name + "\n");
                    }
                }
            }

            private unsafe string GetName(byte* name) // Add the keyword unsafe
            {
                var bytes = new byte[20];
                for (int i = 0; i < 20; i++)
                {
                    bytes[i] = name[i];
                }

                return Encoding.ASCII.GetString(bytes).TrimEnd('\0');
            }
        }
        #endregion

        private BinaryReader _reader;
        private int _itemsLoaded = 0;

        #region LoadItems
        private void LoadItems()
        {
            // Empty the ListView when it first loads
            if (_itemsLoaded == 0)
            {
                listViewTileData.Items.Clear();
            }

            // Load up to 50 items
            for (int i = 0; i < 50 && _reader.BaseStream.Position < _reader.BaseStream.Length; i++)
            {
                // Read the data for each country tile group
                ulong flags = _reader.ReadUInt64();
                ushort textureId = _reader.ReadUInt16();
                string tileName = Encoding.Default.GetString(_reader.ReadBytes(20));

                // Paste the information into the ListView as a new entry
                ListViewItem item = new ListViewItem(new string[]
                {
                    _reader.BaseStream.Position.ToString("X"),
                    tileName,
                    textureId.ToString(),
                    Convert.ToString((long)flags, 2).PadLeft(33, '0')
                });

                // Add the flag values
                for (int j = 0; j < 33; j++)
                {
                    bool flagJ = (flags & (1UL << j)) != 0;
                    item.SubItems.Add(flagJ.ToString());
                }

                listViewTileData.Items.Add(item);
                _itemsLoaded++;
            }
        }
        #endregion

        #region TiledataHex Keydown
        private void TiledataHex_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the spacebar was pressed
            if (e.KeyCode == Keys.Space)
            {
                if (_landItemsLoaded < 16000)
                {
                    LoadLandTiles();
                }
                else if (_staticItemsLoaded < 65500)
                {
                    LoadStaticTiles();
                }
            }
        }
        #endregion

        #region buttonReadTileData
        private void ButtonReadTileData_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _reader = new BinaryReader(File.Open(openFileDialog.FileName, FileMode.Open));
                LoadItems();
            }
        }
        #endregion

        private int _landItemsLoaded = 0;
        private int _staticItemsLoaded = 0;

        #region LoadLandTiles
        private void LoadLandTiles()
        {
            // Load up to 50 items
            for (int i = 0; i < 50 && _reader.BaseStream.Position < _reader.BaseStream.Length; i++)
            {
                // Read the data for each country tile group
                uint flags = _reader.ReadUInt32();
                Console.WriteLine("Flags: " + flags);
                ushort textureId = _reader.ReadUInt16();
                Console.WriteLine("Texture ID: " + textureId);
                string tileName = Encoding.Default.GetString(_reader.ReadBytes(20));
                Console.WriteLine("Tile Name: " + tileName);

                // Paste the information into the ListView as a new entry
                ListViewItem item = new ListViewItem(new string[]
                {
            _reader.BaseStream.Position.ToString("X"),
            flags.ToString(),
            textureId.ToString(),
            tileName
                });

                listViewTileData.Items.Add(item);
                _landItemsLoaded++;
            }
        }
        #endregion

        #region LoadStaticTiles
        private void LoadStaticTiles()
        {
            // Load up to 50 items
            for (int i = 0; i < 50 && _reader.BaseStream.Position < _reader.BaseStream.Length; i++)
            {
                uint unknown = _reader.ReadUInt32();
                uint flags = _reader.ReadUInt32();
                byte weight = _reader.ReadByte();
                byte quality = _reader.ReadByte();
                ushort unknown1 = _reader.ReadUInt16();
                byte unknown2 = _reader.ReadByte();
                byte quantity = _reader.ReadByte();
                ushort animId = _reader.ReadUInt16();
                byte unknown3 = _reader.ReadByte();
                byte hue = _reader.ReadByte();
                ushort unknown4 = _reader.ReadUInt16();
                byte height = _reader.ReadByte();
                string tileName = Encoding.Default.GetString(_reader.ReadBytes(20));

                // Paste the information into the ListView as a new entry
                ListViewItem item = new ListViewItem(new string[]
                {
            _reader.BaseStream.Position.ToString("X"),
            unknown.ToString(),
            flags.ToString(),
            weight.ToString(),
            quality.ToString(),
            unknown1.ToString(),
            unknown2.ToString(),
            quantity.ToString(),
            animId.ToString(),
            unknown3.ToString(),
            hue.ToString(),
            unknown4.ToString(),
            height.ToString(),
            tileName
                });

                listViewTileData.Items.Add(item);
                _staticItemsLoaded++;
            }
        }
        #endregion

        #region buttonReadLandTileData
        private void ButtonReadLandTileData_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _reader = new BinaryReader(File.Open(openFileDialog.FileName, FileMode.Open));
                LoadLandTiles();
            }
        }
        #endregion

        #region buttonReadStaticTileData
        private void ButtonReadStaticTileData_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _reader = new BinaryReader(File.Open(openFileDialog.FileName, FileMode.Open));
                LoadStaticTiles();
            }
        }
        #endregion

        #region listViewTileData
        private void ListViewTileData_MouseClick(object sender, MouseEventArgs e)
        {
            if (listViewTileData.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewTileData.SelectedItems[0];
                foreach (ListViewItem item in listViewTileData.Items)
                {
                    string details = "";
                    // Iterate through all SubItems of the current item
                    foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
                    {
                        // Add the SubItem's text to the details
                        details += subItem.Text + "\n";
                    }
                    // Add the details to the TextBox
                    textBoxOutput.AppendText(details);
                }
            }
        }
        #endregion

        #region Create Tiledata
        private void CreateTiledataMul_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                int landCount = string.IsNullOrEmpty(landBlockCount.Text) ? 512 : int.Parse(landBlockCount.Text);
                int staticCount = int.Parse(staticBlockCount.Text);
                TileData2 tileData = new TileData2(folderBrowserDialog.SelectedPath);
                tileData.CreateTileData(landCount, staticCount);
                int totalIndices = landCount * 32 + staticCount * 32;
                indexCount.Text = $"Created land blocks: {landCount * 32}\n" +
                                  $"Created static blocks: {staticCount * 32}\n" +
                                  $"Total number of indexes created: {totalIndices}";
            }
        }
        #endregion

        #region Indizien
        private void CountIndices_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "mul files (*.mul)|*.mul";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                TileData2 tileData = new TileData2(Path.GetDirectoryName(openFileDialog.FileName));
                int count = tileData.CountIndices();
                indexCount.Text = $"Number of indexes: {count}";
            }
        }
        #endregion       

        #region Create Teture imagees
        //private TextureFileCreator textureFileCreator;

        private void BtCreateTextur_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select the directory that you want to use as the default.";
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string folder = dialog.SelectedPath;

                    // Determine the number of indexes to create
                    int indexCount;
                    if (string.IsNullOrEmpty(tbIndexCount.Text) || !int.TryParse(tbIndexCountTexture.Text, out indexCount))
                    {
                        indexCount = 16383;
                    }

                    // Create the BinaryWriter instances outside the loop
                    using (BinaryWriter texMapsFile = new BinaryWriter(File.Open(Path.Combine(folder, "TexMaps.mul"), FileMode.Create)))
                    using (BinaryWriter texIdxFile = new BinaryWriter(File.Open(Path.Combine(folder, "TexIdx.mul"), FileMode.Create)))
                    {
                        for (int i = 0; i < indexCount; i++)
                        {
                            // Create your imageColors array here
                            short[] imageColors;
                            int extra;
                            if (i % 2 == 0)
                            {
                                imageColors = new short[64 * 64]; // Example: a 64x64 image with all pixels black
                                extra = 0;
                            }
                            else
                            {
                                imageColors = new short[128 * 128]; // Example: a 128x128 image with all pixels black
                                extra = 1;
                            }

                            // Write the image data to the TexMaps.mul file
                            // only if the CheckBox is not activated or these are the first two images
                            if (!checkBoxTexture.Checked || i < 2)
                            {
                                foreach (short color in imageColors)
                                {
                                    texMapsFile.Write(color);
                                }
                            }

                            // Write the index data to the TexIdx.mul file
                            int width = (extra == 1 ? 128 : 64);
                            texIdxFile.Write(width); // width
                            texIdxFile.Write(width); // height
                            texIdxFile.Write(extra);  // extra
                        }
                    }

                    // Update the Label and TextBox to reflect the number of indexes created
                    lbTextureCount.Text = $"Created indexes: {indexCount}";
                    tbIndexCount.Text = indexCount.ToString();
                }
            }
        }
        #endregion

        #region Emtpy Texture
        private void BtCreateIndexes_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select the directory that you want to use as the default.";
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string folder = dialog.SelectedPath;

                    // Determine the number of indexes to create
                    int indexCount;
                    if (string.IsNullOrEmpty(tbIndexCount.Text) || !int.TryParse(tbIndexCountTexture.Text, out indexCount))
                    {
                        indexCount = 16383;
                    }

                    // Create the BinaryWriter instance for the TexIdx.mul file
                    using (BinaryWriter texIdxFile = new BinaryWriter(File.Open(Path.Combine(folder, "TexIdx.mul"), FileMode.Create)))
                    {
                        for (int i = 0; i < indexCount; i++)
                        {
                            // Write the index data to the TexIdx.mul file
                            int width = (i % 2 == 0 ? 64 : 128);
                            texIdxFile.Write(width); // width
                            texIdxFile.Write(width); // height
                            texIdxFile.Write(i % 2);  // extra
                        }
                    }

                    // Create an empty TexMaps.mul file
                    using (File.Create(Path.Combine(folder, "TexMaps.mul"))) { }

                    // Update the Label and TextBox to reflect the number of indexes created
                    lbTextureCount.Text = $"Created indexes: {indexCount}";
                    tbIndexCount.Text = indexCount.ToString();
                }
            }
        }
        #endregion

        #region Create RadarColor
        private void CreateFileButtonRadarColor_Click(object sender, EventArgs e)
        {
            // Set the default file name
            saveFileDialog.FileName = "RadarCol.mul";

            // Display the save file dialog
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Read the number of indexes from the TextBox
                int indexCount;
                if (!int.TryParse(indexCountTextBox.Text, out indexCount))
                {
                    // Display an error message if the input is invalid
                    lbRadarColorInfo.Text = "Please enter a valid number.";
                    return;
                }

                // Create the RadarCol.mul file
                CreateRadarColFile(saveFileDialog.FileName, indexCount);
            }
        }

        private void CreateRadarColFile(string filePath, int indexCount)
        {
            short[] colors = new short[indexCount];
            // Here you can adjust the colors according to your wishes.

            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    writer.Write(colors[i]);
                }
            }

            lbRadarColorInfo.Text = "The file was created successfully!";
        }
        #endregion

        #region Create Orginal Palette

        private Palette _palette = new Palette();

        private void BtCreatePalette_Click(object sender, EventArgs e)
        {
            // Create the palette and add the colors you want
            CreateDefaultPalette();

            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string filename = Path.Combine(folderBrowserDialog.SelectedPath, "Palette.mul");
                    _palette.Save(filename);
                }
            }
        }
        #endregion

        #region CreateDefaultPalette Orginal Colors
        private void CreateDefaultPalette()
        {
            _palette = new Palette();

            // Add the desired RGB values ​​here
            _palette.AddColor(Color.FromArgb(0, 0, 0)); // RGB Value 0: R=0, G=0, B=0
            _palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 1: R=255, G=0, B=255
            _palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 2: R=255, G=0, B=255
            _palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 3: R=255, G=0, B=255
            _palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 4: R=255, G=0, B=255
            _palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 5: R=255, G=0, B=255
            _palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 6: R=255, G=0, B=255
            _palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 7: R=255, G=0, B=255
            _palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 8: R=255, G=0, B=255
            _palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 9: R=255, G=0, B=255
            _palette.AddColor(Color.FromArgb(199, 75, 0)); // RGB Value 10: R=199, G=75, B=0
            _palette.AddColor(Color.FromArgb(207, 99, 0)); // RGB Value 11: R=207, G=99, B=0
            _palette.AddColor(Color.FromArgb(215, 127, 0)); // RGB Value 12: R=215, G=127, B=0
            _palette.AddColor(Color.FromArgb(227, 155, 0)); // RGB Value 13: R=227, G=155, B=0
            _palette.AddColor(Color.FromArgb(235, 187, 0)); // RGB Value 14: R=235, G=187, B=0
            _palette.AddColor(Color.FromArgb(243, 219, 0)); // RGB Value 15: R=243, G=219, B=0
            _palette.AddColor(Color.FromArgb(255, 255, 0)); // RGB Value 16: R=255, G=255, B=0
            _palette.AddColor(Color.FromArgb(255, 255, 95)); // RGB Value 17: R=255, G=255, B=95
            _palette.AddColor(Color.FromArgb(255, 255, 147)); // RGB Value 18: R=255, G=255, B=147
            _palette.AddColor(Color.FromArgb(255, 255, 195)); // RGB Value 19: R=255, G=255, B=195
            _palette.AddColor(Color.FromArgb(255, 255, 247)); // RGB Value 20: R=255, G=255, B=247
            _palette.AddColor(Color.FromArgb(59, 27, 7)); // RGB Value 21: R=59, G=27, B=7
            _palette.AddColor(Color.FromArgb(67, 31, 7)); // RGB Value 22: R=67, G=31, B=7
            _palette.AddColor(Color.FromArgb(75, 35, 11)); // RGB Value 23: R=75, G=35, B=11
            _palette.AddColor(Color.FromArgb(83, 43, 15)); // RGB Value 24: R=83, G=43, B=15
            _palette.AddColor(Color.FromArgb(91, 47, 23)); // RGB Value 25: R=91, G=47, B=23
            _palette.AddColor(Color.FromArgb(99, 55, 27)); // RGB Value 26: R=99, G=55, B=27
            _palette.AddColor(Color.FromArgb(107, 63, 31)); // RGB Value 27: R=107, G=63, B=31
            _palette.AddColor(Color.FromArgb(115, 67, 39)); // RGB Value 28: R=115, G=67, B=39
            _palette.AddColor(Color.FromArgb(123, 75, 47)); // RGB Value 29: R=123, G=75, B=47
            _palette.AddColor(Color.FromArgb(131, 83, 51)); // RGB Value 30: R=131, G=83, B=51
            _palette.AddColor(Color.FromArgb(139, 91, 59)); // RGB Value 31: R=139, G=91, B=59
            _palette.AddColor(Color.FromArgb(147, 99, 67)); // RGB Value 32: R=147, G=99, B=67
            _palette.AddColor(Color.FromArgb(155, 107, 79)); // RGB Value 33: R=155, G=107, B=79
            _palette.AddColor(Color.FromArgb(163, 115, 87)); // RGB Value 34: R=163, G=115, B=87
            _palette.AddColor(Color.FromArgb(171, 127, 95)); // RGB Value 35: R=171, G=127, B=95
            _palette.AddColor(Color.FromArgb(179, 135, 107)); // RGB Value 36: R=179, G=135, B=107
            _palette.AddColor(Color.FromArgb(187, 147, 115)); // RGB Value 37: R=187, G=147, B=115
            _palette.AddColor(Color.FromArgb(195, 155, 127)); // RGB Value 38: R=195, G=155, B=127
            _palette.AddColor(Color.FromArgb(203, 167, 139)); // RGB Value 39: R=203, G=167, B=139
            _palette.AddColor(Color.FromArgb(211, 175, 151)); // RGB Value 40: R=211, G=175, B=151
            _palette.AddColor(Color.FromArgb(219, 187, 163)); // RGB Value 41: R=219, G=187, B=163
            _palette.AddColor(Color.FromArgb(231, 199, 179)); // RGB Value 42: R=231, G=199, B=179
            _palette.AddColor(Color.FromArgb(239, 211, 191)); // RGB Value 43: R=239, G=211, B=191
            _palette.AddColor(Color.FromArgb(247, 223, 207)); // RGB Value 44: R=247, G=223, B=207
            _palette.AddColor(Color.FromArgb(59, 59, 43)); // RGB Value 45: R=59, G=59, B=43
            _palette.AddColor(Color.FromArgb(79, 79, 59)); // RGB Value 46: R=79, G=79, B=59
            _palette.AddColor(Color.FromArgb(103, 103, 79)); // RGB Value 47: R=103, G=103, B=79
            _palette.AddColor(Color.FromArgb(127, 127, 103)); // RGB Value 48: R=127, G=127, B=103
            _palette.AddColor(Color.FromArgb(147, 147, 123)); // RGB Value 49: R=147, G=147, B=123
            _palette.AddColor(Color.FromArgb(171, 171, 147)); // RGB Value 50: R=171, G=171, B=147
            _palette.AddColor(Color.FromArgb(195, 195, 171)); // RGB Value 51: R=195, G=195, B=171
            _palette.AddColor(Color.FromArgb(219, 219, 199)); // RGB Value 52: R=219, G=219, B=199
            _palette.AddColor(Color.FromArgb(39, 0, 0)); // RGB Value 53: R=39, G=0, B=0
            _palette.AddColor(Color.FromArgb(51, 0, 0)); // RGB Value 54: R=51, G=0, B=0
            _palette.AddColor(Color.FromArgb(67, 11, 7)); // RGB Value 55: R=67, G=11, B=7
            _palette.AddColor(Color.FromArgb(79, 19, 11)); // RGB Value 56: R=79, G=19, B=11
            _palette.AddColor(Color.FromArgb(95, 31, 15)); // RGB Value 57: R=95, G=31, B=15
            _palette.AddColor(Color.FromArgb(111, 43, 27)); // RGB Value 58: R=111, G=43, B=27
            _palette.AddColor(Color.FromArgb(123, 59, 35)); // RGB Value 59: R=123, G=59, B=35
            _palette.AddColor(Color.FromArgb(60, 139, 75)); // RGB Value 60 R=60, G=139, B=75
            _palette.AddColor(Color.FromArgb(151, 91, 59)); // RGB Value 61: R=151, G=91, B=59
            _palette.AddColor(Color.FromArgb(167, 111, 71)); // RGB Value 62: R=167, G=111, B=71
            _palette.AddColor(Color.FromArgb(183, 127, 87)); // RGB Value 63: R=183, G=127, B=87
            _palette.AddColor(Color.FromArgb(195, 147, 103)); // RGB Value 64: R=195, G=147, B=103
            _palette.AddColor(Color.FromArgb(211, 167, 123)); // RGB Value 65: R=211, G=167, B=123
            _palette.AddColor(Color.FromArgb(223, 187, 143)); // RGB Value 66: R=223, G=187, B=143
            _palette.AddColor(Color.FromArgb(239, 211, 163)); // RGB Value 67: R=239, G=211, B=163
            _palette.AddColor(Color.FromArgb(255, 231, 187)); // RGB Value 68: R=255, G=231, B=187
            _palette.AddColor(Color.FromArgb(23, 23, 0)); // RGB Value 69: R=23, G=23, B=0
            _palette.AddColor(Color.FromArgb(35, 35, 0)); // RGB Value 70: R=35, G=35, B=0
            _palette.AddColor(Color.FromArgb(51, 47, 0)); // RGB Value 71: R=51, G=47, B=0
            _palette.AddColor(Color.FromArgb(67, 63, 7)); // RGB Value 72: R=67, G=63, B=7
            _palette.AddColor(Color.FromArgb(83, 75, 15)); // RGB Value 73: R=83, G=75, B=15
            _palette.AddColor(Color.FromArgb(99, 87, 19)); // RGB Value 74: R=99, G=87, B=19
            _palette.AddColor(Color.FromArgb(111, 99, 27)); // RGB Value 75: R=111, G=99, B=27
            _palette.AddColor(Color.FromArgb(127, 111, 39)); // RGB Value 76: R=127, G=111, B=39
            _palette.AddColor(Color.FromArgb(143, 123, 51)); // RGB Value 77: R=143, G=123, B=51
            _palette.AddColor(Color.FromArgb(159, 135, 63)); // RGB Value 78: R=159, G=135, B=63
            _palette.AddColor(Color.FromArgb(175, 147, 79)); // RGB Value 79: R=175, G=147, B=79
            _palette.AddColor(Color.FromArgb(187, 159, 95)); // RGB Value 80: R=187, G=159, B=95
            _palette.AddColor(Color.FromArgb(203, 175, 111)); // RGB Value 81: R=203, G=175, B=111
            _palette.AddColor(Color.FromArgb(219, 187, 131)); // RGB Value 82: R=219, G=187, B=131
            _palette.AddColor(Color.FromArgb(235, 203, 147)); // RGB Value 83: R=235, G=203, B=147
            _palette.AddColor(Color.FromArgb(251, 219, 171)); // RGB Value 84: R=251, G=219, B=171
            _palette.AddColor(Color.FromArgb(0, 0, 43)); // RGB Value 85: R=0, G=0, B=43
            _palette.AddColor(Color.FromArgb(0, 0, 55)); // RGB Value 86: R=0, G=0, B=55
            _palette.AddColor(Color.FromArgb(7, 7, 71)); // RGB Value 87: R=7, G=7, B=71
            _palette.AddColor(Color.FromArgb(15, 15, 83)); // RGB Value 88: R=15, G=15, B=83
            _palette.AddColor(Color.FromArgb(23, 23, 99)); // RGB Value 89: R=23, G=23, B=99
            _palette.AddColor(Color.FromArgb(31, 31, 111)); // RGB Value 90: R=31, G=31, B=111
            _palette.AddColor(Color.FromArgb(43, 43, 127)); // RGB Value 91: R=43, G=43, B=127
            _palette.AddColor(Color.FromArgb(59, 59, 139)); // RGB Value 92: R=59, G=59, B=139
            _palette.AddColor(Color.FromArgb(75, 75, 155)); // RGB Value 93: R=75, G=75, B=155
            _palette.AddColor(Color.FromArgb(91, 91, 167)); // RGB Value 94: R=91, G=91, B=167
            _palette.AddColor(Color.FromArgb(111, 111, 183)); // RGB Value 95: R=111, G=111, B=183
            _palette.AddColor(Color.FromArgb(131, 131, 195)); // RGB Value 96: R=131, G=131, B=195
            _palette.AddColor(Color.FromArgb(155, 155, 211)); // RGB Value 97: R=155, G=155, B=211
            _palette.AddColor(Color.FromArgb(179, 179, 227)); // RGB Value 98: R=179, G=179, B=227
            _palette.AddColor(Color.FromArgb(207, 207, 239)); // RGB Value 99: R=207, G=207, B=239
            _palette.AddColor(Color.FromArgb(235, 235, 255)); // RGB Value 100: R=235, G=235, B=255
            _palette.AddColor(Color.FromArgb(27, 27, 111)); // RGB Value 101: R=27, G=27, B=111
            _palette.AddColor(Color.FromArgb(35, 39, 131)); // RGB Value 102: R=35, G=39, B=131
            _palette.AddColor(Color.FromArgb(47, 55, 151)); // RGB Value 103: R=47, G=55, B=151
            _palette.AddColor(Color.FromArgb(63, 71, 171)); // RGB Value 104: R=63, G=71, B=171
            _palette.AddColor(Color.FromArgb(79, 91, 191)); // RGB Value 105: R=79, G=91, B=191
            _palette.AddColor(Color.FromArgb(95, 111, 211)); // RGB Value 106: R=95, G=111, B=211
            _palette.AddColor(Color.FromArgb(115, 135, 231)); // RGB Value 107: R=115, G=135, B=231
            _palette.AddColor(Color.FromArgb(139, 159, 255)); // RGB Value 108: R=139, G=159, B=255
            _palette.AddColor(Color.FromArgb(27, 135, 135)); // RGB Value 109: R=27, G=135, B=135
            _palette.AddColor(Color.FromArgb(43, 151, 151)); // RGB Value 110: R=43, G=151, B=151
            _palette.AddColor(Color.FromArgb(59, 167, 167)); // RGB Value 111: R=59, G=167, B=167
            _palette.AddColor(Color.FromArgb(79, 183, 183)); // RGB Value 112: R=79, G=183, B=183
            _palette.AddColor(Color.FromArgb(107, 203, 203)); // RGB Value 113: R=107, G=203, B=203
            _palette.AddColor(Color.FromArgb(131, 219, 219)); // RGB Value 114: R=131, G=219, B=219
            _palette.AddColor(Color.FromArgb(163, 235, 235)); // RGB Value 115: R=163, G=235, B=235
            _palette.AddColor(Color.FromArgb(195, 255, 255)); // RGB Value 116: R=195, G=255, B=255
            _palette.AddColor(Color.FromArgb(67, 0, 67)); // RGB Value 117: R=67, G=0, B=67
            _palette.AddColor(Color.FromArgb(91, 11, 91)); // RGB Value 118: R=91, G=11, B=91
            _palette.AddColor(Color.FromArgb(119, 27, 119)); // RGB Value 119: R=119, G=27, B=119
            _palette.AddColor(Color.FromArgb(147, 51, 147)); // RGB Value 120: R=147, G=51, B=147
            _palette.AddColor(Color.FromArgb(171, 83, 171)); // RGB Value 121: R=171, G=83, B=171
            _palette.AddColor(Color.FromArgb(199, 123, 199)); // RGB Value 122: R=199, G=123, B=199
            _palette.AddColor(Color.FromArgb(227, 167, 227)); // RGB Value 123: R=227, G=167, B=227
            _palette.AddColor(Color.FromArgb(255, 219, 255)); // RGB Value 124: R=255, G=219, B=255
            _palette.AddColor(Color.FromArgb(55, 127, 59)); // RGB Value 125: R=55, G=127, B=59
            _palette.AddColor(Color.FromArgb(71, 143, 75)); // RGB Value 126: R=71, G=143, B=75
            _palette.AddColor(Color.FromArgb(91, 163, 91)); // RGB Value 127: R=91, G=163, B=91
            _palette.AddColor(Color.FromArgb(111, 179, 111)); // RGB Value 128: R=111, G=179, B=111
            _palette.AddColor(Color.FromArgb(131, 199, 135)); // RGB Value 129: R=131, G=199, B=135
            _palette.AddColor(Color.FromArgb(155, 215, 159)); // RGB Value 130: R=155, G=215, B=159
            _palette.AddColor(Color.FromArgb(183, 235, 183)); // RGB Value 131: R=183, G=235, B=183
            _palette.AddColor(Color.FromArgb(215, 255, 215)); // RGB Value 132: R=215, G=255, B=215
            _palette.AddColor(Color.FromArgb(31, 39, 0)); // RGB Value 133: R=31, G=39, B=0
            _palette.AddColor(Color.FromArgb(43, 51, 0)); // RGB Value 134: R=43, G=51, B=0
            _palette.AddColor(Color.FromArgb(55, 67, 7)); // RGB Value 135: R=55, G=67, B=7
            _palette.AddColor(Color.FromArgb(67, 79, 7)); // RGB Value 136: R=67, G=79, B=7
            _palette.AddColor(Color.FromArgb(83, 95, 15)); // RGB Value 137: R=83, G=95, B=15
            _palette.AddColor(Color.FromArgb(99, 111, 23)); // RGB Value 138: R=99, G=111, B=23
            _palette.AddColor(Color.FromArgb(111, 123, 31)); // RGB Value 139: R=111, G=123, B=31
            _palette.AddColor(Color.FromArgb(127, 139, 43)); // RGB Value 140: R=127, G=139, B=43
            _palette.AddColor(Color.FromArgb(143, 151, 55)); // RGB Value 141: R=143, G=151, B=55
            _palette.AddColor(Color.FromArgb(159, 167, 67)); // RGB Value 142: R=159, G=167, B=67
            _palette.AddColor(Color.FromArgb(175, 183, 79)); // RGB Value 143: R=175, G=183, B=79
            _palette.AddColor(Color.FromArgb(191, 195, 95)); // RGB Value 144: R=191, G=195, B=95
            _palette.AddColor(Color.FromArgb(207, 211, 111)); // RGB Value 145: R=207, G=211, B=111
            _palette.AddColor(Color.FromArgb(223, 223, 131)); // RGB Value 146: R=223, G=223, B=131
            _palette.AddColor(Color.FromArgb(239, 239, 151)); // RGB Value 147: R=239, G=239, B=151
            _palette.AddColor(Color.FromArgb(255, 255, 171)); // RGB Value 148: R=255, G=255, B=171
            _palette.AddColor(Color.FromArgb(0, 35, 19)); // RGB Value 149: R=0, G=35, B=19
            _palette.AddColor(Color.FromArgb(0, 47, 27)); // RGB Value 150: R=0, G=47, B=27
            _palette.AddColor(Color.FromArgb(7, 63, 39)); // RGB Value 151: R=7, G=63, B=39
            _palette.AddColor(Color.FromArgb(11, 79, 55)); // RGB Value 152: R=11, G=79, B=55
            _palette.AddColor(Color.FromArgb(19, 91, 67)); // RGB Value 153: R=19, G=91, B=67
            _palette.AddColor(Color.FromArgb(27, 107, 83)); // RGB Value 154: R=27, G=107, B=83
            _palette.AddColor(Color.FromArgb(35, 123, 99)); // RGB Value 155: R=35, G=123, B=99
            _palette.AddColor(Color.FromArgb(43, 139, 115)); // RGB Value 156: R=43, G=139, B=115
            _palette.AddColor(Color.FromArgb(59, 151, 131)); // RGB Value 157: R=59, G=151, B=131
            _palette.AddColor(Color.FromArgb(79, 167, 151)); // RGB Value 158: R=79, G=167, B=151
            _palette.AddColor(Color.FromArgb(99, 179, 167)); // RGB Value 159: R=99, G=179, B=167
            _palette.AddColor(Color.FromArgb(115, 195, 187)); // RGB Value 160: R=115, G=195, B=187
            _palette.AddColor(Color.FromArgb(139, 211, 203)); // RGB Value 161: R=139, G=211, B=203
            _palette.AddColor(Color.FromArgb(159, 223, 219)); // RGB Value 162: R=159, G=223, B=219
            _palette.AddColor(Color.FromArgb(183, 239, 235)); // RGB Value 163: R=183, G=239, B=235
            _palette.AddColor(Color.FromArgb(211, 255, 255)); // RGB Value 164: R=211, G=255, B=255
            _palette.AddColor(Color.FromArgb(0, 19, 0)); // RGB Value 165: R=0, G=19, B=0
            _palette.AddColor(Color.FromArgb(0, 35, 0)); // RGB Value 166: R=0, G=35, B=0
            _palette.AddColor(Color.FromArgb(0, 47, 7)); // RGB Value 167: R=0, G=47, B=7
            _palette.AddColor(Color.FromArgb(7, 63, 11)); // RGB Value 168: R=7, G=63, B=11
            _palette.AddColor(Color.FromArgb(15, 79, 19)); // RGB Value 169: R=15, G=79, B=19
            _palette.AddColor(Color.FromArgb(23, 95, 27)); // RGB Value 170: R=23, G=95, B=27
            _palette.AddColor(Color.FromArgb(35, 111, 39)); // RGB Value 171: R=35, G=111, B=39
            _palette.AddColor(Color.FromArgb(43, 127, 51)); // RGB Value 172: R=43, G=127, B=51
            _palette.AddColor(Color.FromArgb(59, 143, 67)); // RGB Value 173: R=59, G=143, B=67
            _palette.AddColor(Color.FromArgb(75, 159, 79)); // RGB Value 174: R=75, G=159, B=79
            _palette.AddColor(Color.FromArgb(91, 175, 99)); // RGB Value 175: R=91, G=175, B=99
            _palette.AddColor(Color.FromArgb(107, 191, 115)); // RGB Value 176: R=107, G=191, B=115
            _palette.AddColor(Color.FromArgb(127, 207, 139)); // RGB Value 177: R=127, G=207, B=139
            _palette.AddColor(Color.FromArgb(151, 223, 159)); // RGB Value 178: R=151, G=223, B=159
            _palette.AddColor(Color.FromArgb(171, 239, 183)); // RGB Value 179: R=171, G=239, B=183
            _palette.AddColor(Color.FromArgb(199, 255, 207)); // RGB Value 180: R=199, G=255, B=207
            _palette.AddColor(Color.FromArgb(131, 71, 43)); // RGB Value 181: R=131, G=71, B=43
            _palette.AddColor(Color.FromArgb(147, 83, 47)); // RGB Value 182: R=147, G=83, B=47
            _palette.AddColor(Color.FromArgb(163, 95, 55)); // RGB Value 183: R=163, G=95, B=55
            _palette.AddColor(Color.FromArgb(183, 107, 63)); // RGB Value 184: R=183, G=107, B=63
            _palette.AddColor(Color.FromArgb(199, 123, 71)); // RGB Value 185: R=199, G=123, B=71
            _palette.AddColor(Color.FromArgb(219, 139, 79)); // RGB Value 186: R=219, G=139, B=79
            _palette.AddColor(Color.FromArgb(235, 155, 87)); // RGB Value 187: R=235, G=155, B=87
            _palette.AddColor(Color.FromArgb(255, 171, 95)); // RGB Value 188: R=255, G=171, B=95
            _palette.AddColor(Color.FromArgb(175, 151, 55)); // RGB Value 189: R=175, G=151, B=55
            _palette.AddColor(Color.FromArgb(191, 171, 55)); // RGB Value 190: R=191, G=171, B=55
            _palette.AddColor(Color.FromArgb(211, 195, 55)); // RGB Value 191: R=211, G=195, B=55
            _palette.AddColor(Color.FromArgb(231, 219, 51)); // RGB Value 192: R=231, G=219, B=51
            _palette.AddColor(Color.FromArgb(251, 247, 51)); // RGB Value 193: R=251, G=247, B=51
            _palette.AddColor(Color.FromArgb(27, 7, 0)); // RGB Value 194: R=27, G=7, B=0
            _palette.AddColor(Color.FromArgb(55, 11, 0)); // RGB Value 195: R=55, G=11, B=0
            _palette.AddColor(Color.FromArgb(83, 15, 0)); // RGB Value 196: R=83, G=15, B=0
            _palette.AddColor(Color.FromArgb(111, 15, 0)); // RGB Value 197: R=111, G=15, B=0
            _palette.AddColor(Color.FromArgb(139, 15, 0)); // RGB Value 198: R=139, G=15, B=0
            _palette.AddColor(Color.FromArgb(167, 11, 0)); // RGB Value 199: R=167, G=11, B=0
            _palette.AddColor(Color.FromArgb(195, 7, 0)); // RGB Value 200: R=195, G=7, B=0
            _palette.AddColor(Color.FromArgb(223, 0, 0)); // RGB Value 201: R=223, G=0, B=0
            _palette.AddColor(Color.FromArgb(227, 23, 23)); // RGB Value 202: R=227, G=23, B=23
            _palette.AddColor(Color.FromArgb(231, 51, 51)); // RGB Value 203: R=231, G=51, B=51
            _palette.AddColor(Color.FromArgb(235, 79, 79)); // RGB Value 204: R=235, G=79, B=79
            _palette.AddColor(Color.FromArgb(239, 111, 111)); // RGB Value 205: R=239, G=111, B=111
            _palette.AddColor(Color.FromArgb(243, 139, 139)); // RGB Value 206: R=243, G=139, B=139
            _palette.AddColor(Color.FromArgb(247, 171, 171)); // RGB Value 207: R=247, G=171, B=171
            _palette.AddColor(Color.FromArgb(251, 203, 203)); // RGB Value 208: R=251, G=203, B=203
            _palette.AddColor(Color.FromArgb(255, 239, 239)); // RGB Value 209: R=255, G=239, B=239
            _palette.AddColor(Color.FromArgb(7, 7, 7)); // RGB Value 210: R=7, G=7, B=7
            _palette.AddColor(Color.FromArgb(15, 15, 15)); // RGB Value 211: R=15, G=15, B=15
            _palette.AddColor(Color.FromArgb(27, 27, 27)); // RGB Value 212: R=27, G=27, B=27
            _palette.AddColor(Color.FromArgb(39, 39, 39)); // RGB Value 213: R=39, G=39, B=39
            _palette.AddColor(Color.FromArgb(51, 51, 47)); // RGB Value 214: R=51, G=51, B=47
            _palette.AddColor(Color.FromArgb(59, 59, 59)); // RGB Value 215: R=59, G=59, B=59
            _palette.AddColor(Color.FromArgb(71, 71, 71)); // RGB Value 216: R=71, G=71, B=71
            _palette.AddColor(Color.FromArgb(83, 83, 79)); // RGB Value 217: R=83, G=83, B=79
            _palette.AddColor(Color.FromArgb(91, 91, 91)); // RGB Value 218: R=91, G=91, B=91
            _palette.AddColor(Color.FromArgb(103, 103, 99)); // RGB Value 219: R=103, G=103, B=99
            _palette.AddColor(Color.FromArgb(115, 115, 111)); // RGB Value 220: R=115, G=115, B=111
            _palette.AddColor(Color.FromArgb(127, 127, 123)); // RGB Value 221: R=127, G=127, B=123
            _palette.AddColor(Color.FromArgb(127, 127, 123)); // RGB Value 222: R=127, G=127, B=123
            _palette.AddColor(Color.FromArgb(135, 135, 131)); // RGB Value 223: R=135, G=135, B=131
            _palette.AddColor(Color.FromArgb(147, 147, 143)); // RGB Value 224: R=147, G=147, B=143
            _palette.AddColor(Color.FromArgb(159, 159, 151)); // RGB Value 225: R=159, G=159, B=151
            _palette.AddColor(Color.FromArgb(171, 171, 163)); // RGB Value 226: R=171, G=171, B=163
            _palette.AddColor(Color.FromArgb(179, 179, 171)); // RGB Value 227: R=179, G=179, B=171
            _palette.AddColor(Color.FromArgb(191, 191, 183)); // RGB Value 228: R=191, G=191, B=183
            _palette.AddColor(Color.FromArgb(203, 203, 191)); // RGB Value 229: R=203, G=203, B=191
            _palette.AddColor(Color.FromArgb(211, 211, 199)); // RGB Value 230: R=211, G=211, B=199
            _palette.AddColor(Color.FromArgb(223, 223, 211)); // RGB Value 231: R=223, G=223, B=211
            _palette.AddColor(Color.FromArgb(235, 235, 219)); // RGB Value 232: R=235, G=235, B=219
            _palette.AddColor(Color.FromArgb(247, 247, 231)); // RGB Value 233: R=247, G=247, B=231
            _palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 234: R=0, G=255, B=0
            _palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 235: R=0, G=255, B=0
            _palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 236: R=0, G=255, B=0
            _palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 237: R=0, G=255, B=0
            _palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 238: R=0, G=255, B=0
            _palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 239: R=0, G=255, B=0
            _palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 240: R=0, G=255, B=0
            _palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 241: R=0, G=255, B=0
            _palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 242: R=0, G=255, B=0
            _palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 243: R=0, G=255, B=0
            _palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 244: R=0, G=255, B=0
            _palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 245: R=0, G=255, B=0
            _palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 246: R=255, G=0, B=255
            _palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 247: R=255, G=0, B=255
            _palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 248: R=255, G=0, B=255
            _palette.AddColor(Color.FromArgb(255, 0, 247)); // RGB Value 249: R=255, G=0, B=247
            _palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 250: R=255, G=0, B=255
            _palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 251: R=255, G=0, B=255
            _palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 252: R=255, G=0, B=255
            _palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 253: R=255, G=0, B=255
            _palette.AddColor(Color.FromArgb(127, 127, 127)); // RGB Value 254: R=127, G=127, B=127
            _palette.AddColor(Color.FromArgb(255, 255, 255)); // RGB Value 255: R=255, G=255, B=255
        }
        #endregion

        #region Create Gray Palette RGB
        private void BtCreatePaletteFull_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string filename = Path.Combine(folderBrowserDialog.SelectedPath, "Palette.mul");
                    _palette.CreateDefaultPalette();
                    _palette.Save(filename);
                }
            }
        }
        #endregion

        #region Load Palette
        private void LoadPaletteButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "mul files (*.mul)|*.mul";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _palette.Load(openFileDialog.FileName);
                    DrawPalette();
                    DisplayRGBValues();
                }
            }
        }
        #endregion

        #region DrawPalette
        private void DrawPalette()
        {
            Bitmap bitmap = new Bitmap(pictureBoxPalette.Width, pictureBoxPalette.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                _palette.DrawPalette(g, pictureBoxPalette.Width, pictureBoxPalette.Height);
            }
            pictureBoxPalette.Image = bitmap;
        }
        #endregion

        #region DisplayRGBValues
        private void DisplayRGBValues()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _palette.RGBValues.Count; i++)
            {
                sb.AppendLine($"RGB Value {i}: R={_palette.RGBValues[i].R}, G={_palette.RGBValues[i].G}, B={_palette.RGBValues[i].B}");
            }
            //rgbValuesLabel.Text = sb.ToString();
            textBoxRgbValues.Text = sb.ToString();

            // Copy the RGB values ​​to the clipboard
            Clipboard.SetText(sb.ToString());
        }
        #endregion

        #region Animation
        #region btnLoadAnimationMulData
        private void BtnLoadAnimationMulData_Click(object sender, EventArgs e)
        {
            // Open the file dialog to let the user select the directory
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string animMulPath = Path.Combine(fbd.SelectedPath, "Anim.mul");
                    string animIdxPath = Path.Combine(fbd.SelectedPath, "Anim.idx");

                    // Load the animation with the selected file paths
                    AnimationGroup animation = LoadAnimation(animMulPath, animIdxPath);

                    // Display the animation in the TextBox
                    this.txtData.Text = animation.ToString();
                }
            }
        }
        #endregion

        #region LoadAnimation
        private AnimationGroup LoadAnimation(string animMulPath, string animIdxPath)
        {
            AnimationGroup animation = new AnimationGroup();

            // Read the data from the Anim.idx file
            using (var idxReader = new BinaryReader(File.OpenRead(animIdxPath)))
            {
                animation.FrameCount = (uint)(idxReader.BaseStream.Length / 12);
                animation.FrameOffset = new uint[animation.FrameCount];

                for (int i = 0; i < animation.FrameCount; i++)
                {
                    if (idxReader.BaseStream.Position < idxReader.BaseStream.Length - 12) // Check if there is enough data left to read
                    {
                        idxReader.ReadUInt32(); // Skip the lookup field
                        uint size = idxReader.ReadUInt32();
                        idxReader.ReadUInt32(); // Skip the Unknown field

                        if (size > 0)
                        {
                            animation.FrameOffset[i] = size;
                        }
                    }
                }
            }

            // Read the data from the Anim.mul file
            using (var mulReader = new BinaryReader(File.OpenRead(animMulPath)))
            {
                animation.Frames = new Frame[animation.FrameCount];

                for (int i = 0; i < animation.FrameCount; i++)
                {
                    if (animation.FrameOffset[i] > 0)
                    {
                        mulReader.BaseStream.Seek(animation.FrameOffset[i], SeekOrigin.Begin);

                        if (mulReader.BaseStream.Position < mulReader.BaseStream.Length - 8) // Check if there is enough data left to read
                        {
                            Frame frame = new Frame();
                            frame.ImageCenterX = mulReader.ReadUInt16();
                            frame.ImageCenterY = mulReader.ReadUInt16();
                            frame.Width = mulReader.ReadUInt16();
                            frame.Height = mulReader.ReadUInt16();

                            // Here you should add the code to read the pixel data

                            animation.Frames[i] = frame;
                        }
                    }
                }
            }

            return animation;
        }
        #endregion
        #endregion

        #region Arts
        #region BtnCreateArtIdx
        private void BtnCreateArtIdx_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "MUL Files|*.mul";
            saveFileDialog.Title = "Save a MUL File";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                CreateArtIdx(saveFileDialog.FileName, 1000000);
                infoARTIDXMULID.AppendText($"Created a new ARTIDX.MUL file with 1,000,000 entries at {saveFileDialog.FileName}\n");
            }
        }
        #endregion

        #region CreateArtIdx
        private void CreateArtIdx(string filename, int entries)
        {
            using (FileStream stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                // Read the existing entries into a buffer
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);

                // Create a buffer for the additional entries
                byte[] additionalBuffer = new byte[(entries - buffer.Length / 12) * 12];

                // Write the additional entries to the stream
                stream.Write(additionalBuffer, 0, additionalBuffer.Length);
            }
        }
        #endregion

        #region BtnCreateArtIdx100K
        private void BtnCreateArtIdx100K_Click(object sender, EventArgs e)
        {
            CreateArtIdxWithEntries(100000);
        }
        #endregion

        #region BtnCreateArtIdx150K
        private void BtnCreateArtIdx150K_Click(object sender, EventArgs e)
        {
            CreateArtIdxWithEntries(150000);
        }
        #endregion

        #region BtnCreateArtIdx200K
        private void BtnCreateArtIdx200K_Click(object sender, EventArgs e)
        {
            CreateArtIdxWithEntries(200000);
        }
        #endregion

        #region BtnCreateArtIdx250K
        private void BtnCreateArtIdx250K_Click(object sender, EventArgs e)
        {
            CreateArtIdxWithEntries(250000);
        }
        #endregion

        #region BtnCreateArtIdx500K
        private void BtnCreateArtIdx500K_Click(object sender, EventArgs e)
        {
            CreateArtIdxWithEntries(500000);
        }
        #endregion

        #region CreateArtIdxWithEntries
        private void CreateArtIdxWithEntries(int entries)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "MUL Files|*.mul";
            saveFileDialog.Title = "Save a MUL File";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                CreateArtIdx(saveFileDialog.FileName, entries);
                infoARTIDXMULID.AppendText($"Created a new ARTIDX.MUL file with {entries} entries at {saveFileDialog.FileName}\n");
            }
        }
        #endregion

        #region BtnReadArtIdx2
        private int _indexCount = 0;        
        private void BtnReadArtIdx2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MUL Files|*.mul";
            openFileDialog.Title = "Open a MUL File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _indexCount = 0; // Setzen Sie den Zähler zurück
                ReadArtIdx(openFileDialog.FileName);
                lblIndexCount.Text = $"Total indices read: {_indexCount}"; // Update the label
                infoARTIDXMULID.AppendText($"Finished reading {_indexCount} entries from {openFileDialog.FileName}\n");
            }
        }
        #endregion

        #region ReadArtIdx
        private void ReadArtIdx(string filename)
        {
            using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                // Calculate the number of entries
                int entries = (int)stream.Length / 12;

                // Create a buffer for the entries
                byte[] buffer = new byte[12];

                int definedCount = 0;
                int undefinedCount = 0;

                for (int i = 0; i < entries; i++)
                {
                    // Read an entry into the buffer
                    stream.Read(buffer, 0, buffer.Length);

                    // Convert the buffer to DWORDs
                    int lookup = BitConverter.ToInt32(buffer, 0);
                    int size = BitConverter.ToInt32(buffer, 4);

                    // Check if the entry is defined
                    if (lookup != -1 && size > 0)
                    {
                        definedCount++;
                    }
                    else
                    {
                        undefinedCount++;
                    }

                    _indexCount++; // Erhöhen Sie den Zähler
                }

                // Append the summary to infoARTIDXMULID
                infoARTIDXMULID.AppendText($"Total entries: {_indexCount}\n");
                infoARTIDXMULID.AppendText($"Defined entries: {definedCount}\n");
                infoARTIDXMULID.AppendText($"Undefined entries: {undefinedCount}\n");
            }
        }
        #endregion

        #region ReadArtmul2
        private void ReadArtmul2_Click(object sender, EventArgs e)
        {
            // Öffnen Sie einen FolderBrowserDialog, um die Datei auszuwählen
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string fileName = Path.Combine(fbd.SelectedPath, "artidx.mul");
                if (File.Exists(fileName))
                {
                    // Lesen Sie die Datei als Bytearray
                    byte[] fileBytes = File.ReadAllBytes(fileName);

                    // Berechnen Sie die Anzahl der Indexeinträge (jeder Eintrag hat 12 Bytes)
                    int indexCount = fileBytes.Length / 12;

                    // Zeigen Sie die Anzahl der Indexeinträge im Label an
                    lblIndexCount.Text = $"Die ARTIDX.MUL hat {indexCount} Indexeinträge.";
                }
                else
                {
                    MessageBox.Show("Die Datei 'artidx.mul' wurde nicht gefunden.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region btnCreateNewArtidx
        private void BtnCreateNewArtidx(object sender, EventArgs e)
        {
            // Open a FolderBrowserDialog to select the destination path
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string targetPath = fbd.SelectedPath;

                // Verify that a valid number of index entries has been entered
                if (int.TryParse(tbxNewIndex.Text, out int indexCount) && indexCount > 0)
                {
                    // Create a new byte array with the specified size
                    byte[] newFileBytes = new byte[indexCount * 12];

                    // Set the flag to 0xFFFFFFFF for all index entries (empty)
                    for (int i = 0; i < indexCount; i++)
                    {
                        int offset = i * 12;
                        // Write the flag (0xFFFFFFFF) to the first 4 bytes of the index entry
                        Array.Copy(BitConverter.GetBytes(0xFFFFFFFF), 0, newFileBytes, offset, 4);

                        // Initialize the next 4 bytes (Size) with 0
                        Array.Copy(BitConverter.GetBytes(0), 0, newFileBytes, offset + 4, 4);

                        // Initialize the last 4 bytes (Unknown) with 0
                        Array.Copy(BitConverter.GetBytes(0), 0, newFileBytes, offset + 8, 4);
                    }

                    // Save the new byte array as a file
                    string newFileName = Path.Combine(targetPath, "artidx.mul");
                    File.WriteAllBytes(newFileName, newFileBytes);

                    MessageBox.Show($"The new ARTIDX.MUL file with {indexCount} empty index entries has been successfully created..", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Please enter a valid positive integer for the number of index entries.", "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region btnCreateNewArtidx2
        private void BtnCreateNewArtidx2(object sender, EventArgs e)
        {
            // Open a FolderBrowserDialog to select the destination path
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string targetPath = fbd.SelectedPath;

                // Verify that valid numbers have been entered for the ranges
                if (int.TryParse(tbxNewIndex2.Text, out int totalCount) && totalCount > 0 &&
                    int.TryParse(tbxArtsCount.Text, out int artsCount) && artsCount >= 0 &&
                    int.TryParse(tbxLandTilesCount.Text, out int landTilesCount) && landTilesCount >= 0 &&
                    artsCount + landTilesCount == totalCount)
                {
                    // Create a new byte array with the specified size
                    byte[] newFileBytes = new byte[totalCount * 12];

                    // Set the flag to 0xFFFFFFFF for all index entries (empty)
                    for (int i = 0; i < totalCount; i++)
                    {
                        int offset = i * 12;
                        Array.Copy(BitConverter.GetBytes(0xFFFFFFFF), 0, newFileBytes, offset, 4); // Flag
                        Array.Copy(BitConverter.GetBytes(0), 0, newFileBytes, offset + 4, 4); // Size
                        Array.Copy(BitConverter.GetBytes(0), 0, newFileBytes, offset + 8, 4); // Unknown
                    }

                    // Speichern Sie das neue Bytearray als Datei
                    string newFileName = Path.Combine(targetPath, "artidx.mul");
                    File.WriteAllBytes(newFileName, newFileBytes);

                    MessageBox.Show($"The new ARTIDX.MUL file with a total of {totalCount} empty index entries has been successfully created..\nNumber of Arts: {artsCount}\nNumber of Land Tiles: {landTilesCount}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Please enter valid positive integers for the total number, the number of arts, and the number of land tiles, where the sum of the arts and land tiles must match the total number.", "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region btnCreateOldVersionArtidx
        private void BtnCreateOldVersionArtidx(object sender, EventArgs e)
        {
            // Open a FolderBrowserDialog to select the destination path
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string targetPath = fbd.SelectedPath;

                // Verify that a valid number of index entries has been entered
                if (int.TryParse(tbxNewIndex.Text, out int indexCount) && indexCount > 0)
                {
                    // Create a new byte array with the specified size
                    byte[] newFileBytes = new byte[indexCount * 12];

                    // Set the flag to 0xFFFFFFFF for all index entries (empty)
                    for (int i = 0; i < indexCount; i++)
                    {
                        int offset = i * 12;
                        newFileBytes[offset] = 0xFF;
                        newFileBytes[offset + 1] = 0xFF;
                        newFileBytes[offset + 2] = 0xFF;
                        newFileBytes[offset + 3] = 0xFF;
                    }

                    // Save the new byte array as a file
                    string newFileName = Path.Combine(targetPath, "artidx.mul");
                    File.WriteAllBytes(newFileName, newFileBytes);

                    MessageBox.Show($"The new ARTIDX.MUL file with {indexCount} empty index entries has been successfully created.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Please enter a valid positive integer for the number of index entries.", "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion
        #endregion

        #region btnSingleEmptyAnimMul_Click
        private void BtnSingleEmptyAnimMul_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select a directory to save the files";

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string path = fbd.SelectedPath;

                    // Create the file anim.mul
                    using (FileStream fs = File.Create(Path.Combine(path, "anim.mul")))
                    {
                        // No data is written to the anim.mul file because it is supposed to be empty
                    }
                }
            }
        }
        #endregion

        #region Constants
        // Constants for the sizes of different creature types
        const int _cHighDetail = 110;
        const int _cLowDetail = 65;
        const int _cHuman = 175;

        const int _cHighDetailOLd = 1; // Old Version
        const int _cLowDetailOld = 2; // Old Version
        const int _cHumanOld = 3; // Old Version

        private int _newIdCount = 0;
        #endregion

        #region btnBrowseClick
        private void BtnBrowseClick(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                tbfilename.Text = openFileDialog.FileName;
            }
        }
        #endregion

        #region btnSetOutputDirectoryClick
        private void BtnSetOutputDirectoryClick(object sender, EventArgs e)
        {
            // Open a FolderBrowserDialog to select the output directory
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                // Set the text of the output directory textbox to the selected path
                txtOutputDirectory.Text = folderBrowserDialog.SelectedPath;
            }
        }
        #endregion

        #region btnProcessClick
        private void BtnProcessClick(object sender, EventArgs e)
        {
            // Flush tbProcessAminidx with every new process
            tbProcessAminidx.Clear();

            try
            {
                // Check if the file exists
                string filename = tbfilename.Text;
                if (!File.Exists(filename))
                {
                    tbProcessAminidx.AppendText("Could not open anim.idx!\n");
                    return;
                }

                // Parse the creature ID from the text box
                int creatureID;
                if (!int.TryParse(txtOrigCreatureID.Text, System.Globalization.NumberStyles.HexNumber, null, out creatureID))
                {
                    tbProcessAminidx.AppendText("Enter a valid Animation ID\n");
                    return;
                }

                // Determine the number of copies based on the selected checkboxes
                int copyCount;
                if (chkHighDetail.Checked)
                {
                    copyCount = _cHighDetail;
                }
                else if (chkLowDetail.Checked)
                {
                    copyCount = _cLowDetail;
                }
                else if (chkHuman.Checked)
                {
                    copyCount = _cHuman;
                }
                else
                {
                    // If no checkbox is selected, parse the number of copies from the text box
                    if (!int.TryParse(txtNewCreatureID.Text, out copyCount))
                    {
                        tbProcessAminidx.AppendText("Enter a valid copy count\n");
                        return;
                    }
                }

                // Determine the output filename
                string outputFilename = txtOutputFilename.Text;
                if (string.IsNullOrEmpty(outputFilename))
                {
                    outputFilename = Path.Combine(txtOutputDirectory.Text, "anim.idx"); // Use "anim.idx" in the output directory if no output file is specified
                }
                else
                {
                    // Append "amin", the outputFilename, and ".idx" to the output directory
                    outputFilename = Path.Combine(txtOutputDirectory.Text, "amin" + outputFilename + ".idx");
                }

                // Copy the original file to the output file
                File.Copy(filename, outputFilename, true);

                // Execute the copy process
                // CopyAnimationData(filename, creatureID, copyCount);

                // Execute the copy process on the new file
                CopyAnimationData(outputFilename, creatureID, copyCount);
            }
            catch (Exception ex)
            {
                tbProcessAminidx.AppendText($"An error has occurred: {ex.Message}\n");
            }
        }
        #endregion

        #region CopyAnimationData
        private void CopyAnimationData(string filename, int creatureID, int copyCount)
        {
            tbProcessAminidx.AppendText("Checking if new Animation ID is in use\n");

            using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.ReadWrite))
            {
                // Determine the index offset and length of data to read based on the creature ID
                int indexOffset, readLength;
                string creatureType;
                DetermineCreatureProperties(creatureID, out indexOffset, out readLength, out creatureType);

                tbProcessAminidx.AppendText($"Creature is a {creatureType}\n");

                // Read the animation index data into a byte array
                stream.Seek(indexOffset * 12, SeekOrigin.Begin);
                byte[] buffer = new byte[readLength];
                stream.Read(buffer, 0, readLength);
                tbProcessAminidx.AppendText($"Read {readLength} bytes of index-data for cID {creatureID}\n");

                // Copy the data the specified number of times
                for (int i = 0; i < copyCount; i++)
                {
                    // Find the end of the file
                    stream.Seek(0, SeekOrigin.End);

                    // Write the data directly to the stream
                    stream.Write(buffer, 0, readLength);

                    tbProcessAminidx.AppendText($"Wrote {readLength} bytes of index-data to cID {creatureID}\n");

                    // Increment the counter for each ID created
                    _newIdCount++;
                }
                // Update the label with the number of IDs created
                lblNewIdCount.Text = $"Number of IDs created: {_newIdCount}";
            }
        }
        #endregion        

        #region ReadAnimIdx
        private async void ReadAnimIdx_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("ReadAnimIdx_Click gestartet");
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Animation Index File (ANIM.IDX)|ANIM.IDX";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                Debug.WriteLine($"Datei: {filePath}");
                try
                {
                    using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath)))
                    {
                        tbProcessAminidx.Clear();
                        StringBuilder indexEntries = new StringBuilder();
                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                        {
                            int lookup = await Task.Run(() => reader.ReadInt32());
                            int size = await Task.Run(() => reader.ReadInt32());
                            int unknown = await Task.Run(() => reader.ReadInt32());
                            indexEntries.AppendLine($"Lookup: {lookup}, Size: {size}, Unknown: {unknown}");
                        }
                        tbProcessAminidx.Text = indexEntries.ToString();
                    }
                    Debug.WriteLine("Read file completed");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region btnCountIndices
        private async void BtnCountIndices_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Animation Index File (ANIM.IDX)|ANIM.IDX";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                try
                {
                    using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath)))
                    {
                        int indexCount = 0;
                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                        {
                            await Task.Run(() => reader.ReadInt32()); // Lookup
                            await Task.Run(() => reader.ReadInt32()); // Size
                            await Task.Run(() => reader.ReadInt32()); // Unknown
                            indexCount++;
                        }
                        lblNewIdCount.Text = indexCount.ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Lesen der Datei: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region DetermineCreatureProperties
        private void DetermineCreatureProperties(int creatureID, out int indexOffset, out int readLength, out string creatureType)
        {
            if (creatureID <= 199)
            {
                indexOffset = creatureID * _cHighDetail;
                readLength = _cHighDetail * 12;
                creatureType = "High Detail Critter";
            }
            else if (creatureID > 199 && creatureID <= 399)
            {
                indexOffset = (creatureID - 200) * _cLowDetail + 22000;
                readLength = _cLowDetail * 12;
                creatureType = "Low Detail Critter";
            }
            else
            {
                indexOffset = (creatureID - 400) * _cHuman + 35000;
                readLength = _cHuman * 12;
                creatureType = "Human or an Accessoire";
            }
        }
        #endregion

        #region btnProcessClickOldVersion
        private void BtnProcessClickOldVersion(object sender, EventArgs e)
        {
            // Flush tbProcessAminidx with every new process
            tbProcessAminidx.Clear();

            try
            {
                int readLength = 0;
                int wroteLength = 0;
                string filename = tbfilename.Text;
                if (!File.Exists(filename))
                {
                    tbProcessAminidx.AppendText("Could not open anim.idx!\n");
                    return;
                }

                int creatureID;
                if (!int.TryParse(txtOrigCreatureID.Text, System.Globalization.NumberStyles.HexNumber, null, out creatureID))
                {
                    tbProcessAminidx.AppendText("Enter a valid Animation ID\n");
                    return;
                }

                int copyCount;
                if (!int.TryParse(txtNewCreatureID.Text, out copyCount))
                {
                    tbProcessAminidx.AppendText("Enter a valid copy count\n");
                    return;
                }

                tbProcessAminidx.AppendText("Checking if new Animation ID is in use\n");

                using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.ReadWrite))
                {
                    int indexOffset;
                    byte cAnimType;

                    if (creatureID <= 199)
                    {
                        indexOffset = creatureID * 110;
                        readLength = 110 * 12;
                        cAnimType = _cHighDetailOLd;
                        tbProcessAminidx.AppendText("Creature is a High Detail Critter\n");
                    }
                    else if (creatureID > 199 && creatureID <= 399)
                    {
                        indexOffset = (creatureID - 200) * 65 + 22000;
                        readLength = 65 * 12;
                        cAnimType = _cLowDetailOld;
                        tbProcessAminidx.AppendText("Creature is a Low Detail Critter\n");
                    }
                    else
                    {
                        indexOffset = (creatureID - 400) * 175 + 35000;
                        readLength = 175 * 12;
                        cAnimType = _cHumanOld;
                        tbProcessAminidx.AppendText("Creature is a Human or an Accessoire\n");
                    }

                    stream.Seek(indexOffset * 12, SeekOrigin.Begin);
                    byte[] buffer = new byte[readLength];
                    stream.Read(buffer, 0, readLength);
                    tbProcessAminidx.AppendText($"Read {readLength} bytes of index-data for cID {creatureID}\n");

                    // Copy the data the number of times specified in copyCount
                    for (int i = 0; i < copyCount; i++)
                    {
                        // Find the end of the file
                        stream.Seek(0, SeekOrigin.End);

                        // Write the data directly to the stream
                        stream.Write(buffer, 0, readLength);

                        // Update wroteLength to the value of readLength since all data has been written
                        wroteLength = readLength;
                        tbProcessAminidx.AppendText($"Wrote {wroteLength} bytes of index-data to cID {creatureID}\n");

                        // Perform different actions based on the creature type
                        switch (cAnimType)
                        {
                            case _cHighDetailOLd:
                                // Perform some action for high detail creatures
                                break;
                            case _cLowDetailOld:
                                // Perform some action for low detail creatures
                                break;
                            case _cHumanOld:
                                // Perform some action for human creatures
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                tbProcessAminidx.AppendText($"An error has occurred: {ex.Message}\n");
            }
        }
        #endregion

        #region Sound
        #region CreateOrgSoundMul
        private void CreateOrgSoundMul_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string directoryPath = folderBrowserDialog.SelectedPath;
                int indexSize = int.Parse(SoundIDXMul.Text);

                using (BinaryWriter soundIdxWriter = new BinaryWriter(File.Open(Path.Combine(directoryPath, "SoundIdx.mul"), FileMode.Create)))
                using (BinaryWriter soundWriter = new BinaryWriter(File.Open(Path.Combine(directoryPath, "Sound.mul"), FileMode.Create)))
                {
                    int currentPosition = 0;

                    for (int i = 0; i < indexSize; i++)
                    {
                        // Beispieldaten für Wellendaten
                        byte[] waveData = GenerateWaveData();

                        // Schreiben Sie die Wellendaten in die Sound.mul-Datei
                        soundWriter.Write(waveData);

                        // Schreiben Sie die Indexeinträge in die SoundIdx.mul-Datei
                        soundIdxWriter.Write(currentPosition); // Start-Position
                        soundIdxWriter.Write(waveData.Length); // Länge der Wellendaten
                        soundIdxWriter.Write((ushort)i); // Index
                        soundIdxWriter.Write((ushort)0); // Reserviert

                        currentPosition += waveData.Length;
                    }
                }
            }
        }

        private byte[] GenerateWaveData()
        {
            // Hier können Sie den Code zum Generieren von Beispiel-Wellendaten einfügen
            // oder die Wellendaten aus einer Quelldatei lesen
            byte[] waveData = new byte[1024]; // Beispiel-Wellendaten
            return waveData;
        }
        #endregion

        #region ReadIndexSize Sound
        private void ReadIndexSize_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string directoryPath = folderBrowserDialog.SelectedPath;
                string filePath = Path.Combine(directoryPath, "SoundIdx.mul");

                if (File.Exists(filePath))
                {
                    int indexCount = 0;
                    using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath)))
                    {
                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                        {
                            // Lesen Sie die Start-Position (4 Bytes)
                            int start = reader.ReadInt32();

                            // Lesen Sie die Länge der Wellendaten (4 Bytes)
                            int length = reader.ReadInt32();

                            // Lesen Sie den Index (2 Bytes)
                            ushort index = reader.ReadUInt16();

                            // Lesen Sie den reservierten Wert (2 Bytes)
                            ushort reserved = reader.ReadUInt16();

                            indexCount++;
                        }
                    }

                    IndexSizeLabel.Text = $"Indexgröße: {indexCount}";
                }
                else
                {
                    MessageBox.Show("Die Datei SoundIdx.mul wurde nicht gefunden.");
                }
            }
        }
        #endregion
        #endregion

        #region Gump
        #region CreateGumpButton
        // Event handler for the button click event to create a new Gump.
        // Parameters:
        //   - sender: The object that raised the event.
        //   - e: The event arguments.
        private void CreateGumpButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string directoryPath = folderBrowserDialog.SelectedPath;
                int indexSize;

                if (int.TryParse(IndexSizeTextBox.Text, out indexSize) && indexSize > 0)
                {
                    int width = 2500; // Actual width of the image
                    int height = 2500; // Actual height of the image

                    List<GumpRow> rows = GenerateGumpRows(width, indexSize);
                    CreateGump(directoryPath, width, height, rows);
                }
                else
                {
                    MessageBox.Show("Please enter a valid index size, which is greater than 0.");
                }
            }
        }
        #endregion

        #region ReadGumpButton
        // Event handler for the button click event to read the Gump index count and display it on a label.
        // Parameters:
        //   - sender: The object that raised the event.
        //   - e: The event arguments.

        private void ReadGumpButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string directoryPath = folderBrowserDialog.SelectedPath;
                int indexCount = GetIndexCount(directoryPath);

                gumpLabel.Text = $"Index is {indexCount}";
            }
        }
        #endregion

        #region GetIndexCount
        // Retrieves the number of entries in the Gump index file.
        // Parameters:
        //   - directoryPath: The directory path containing the Gump index file.
        // Returns:
        //   The number of entries in the Gump index, or 0 if the index file does not exist.
        private int GetIndexCount(string directoryPath)
        {
            string gumpIndexPath = Path.Combine(directoryPath, "GUMPIDX.MUL");

            if (!File.Exists(gumpIndexPath))
                return 0;

            using (BinaryReader gumpIndexReader = new BinaryReader(File.OpenRead(gumpIndexPath)))
            {
                return (int)gumpIndexReader.BaseStream.Length / 12;
            }
        }
        #endregion

        #region CreateGump
        // Creates a Gump by writing its metadata and pixel data to binary files.
        // Parameters:
        //   - directoryPath: The directory path where the Gump files will be created.
        //   - width: The width of the Gump.
        //   - height: The height of the Gump.
        //   - rows: A list of GumpRow objects representing the pixel data of the Gump.

        private void CreateGump(string directoryPath, int width, int height, List<GumpRow> rows)
        {
            string gumpIndexPath = Path.Combine(directoryPath, "GUMPIDX.MUL");
            string gumpDataPath = Path.Combine(directoryPath, "GUMPART.MUL");

            using (BinaryWriter gumpIndexWriter = new BinaryWriter(File.Open(gumpIndexPath, FileMode.Create)))
            using (BinaryWriter gumpDataWriter = new BinaryWriter(File.Open(gumpDataPath, FileMode.Create)))
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    gumpIndexWriter.Write(-1); // Lookup (still undefined)
                    gumpIndexWriter.Write(0); // Size (will be updated later)
                    gumpIndexWriter.Write((ushort)height); // Height
                    gumpIndexWriter.Write((ushort)width); // Width

                    // Write a placeholder value to the GUMPART.MUL file
                    gumpDataWriter.Write(-1);
                }
            }
        }
        #endregion

        #region ReadGump
        // Reads a Gump from binary data files.
        // Parameters:
        //   - directoryPath: The directory path containing the Gump files.
        //   - gumpIndex: The index of the Gump to read.
        // Returns:
        //   A list of GumpRow objects representing the pixels of the read Gump.

        private List<GumpRow> ReadGump(string directoryPath, int gumpIndex)
        {
            string gumpIndexPath = Path.Combine(directoryPath, "GUMPIDX.MUL");
            string gumpDataPath = Path.Combine(directoryPath, "GUMPART.MUL");

            using (BinaryReader gumpIndexReader = new BinaryReader(File.OpenRead(gumpIndexPath)))
            using (BinaryReader gumpDataReader = new BinaryReader(File.OpenRead(gumpDataPath)))
            {
                gumpIndexReader.BaseStream.Seek(gumpIndex * 12, SeekOrigin.Begin);
                int lookup = gumpIndexReader.ReadInt32();
                int size = gumpIndexReader.ReadInt32();
                ushort height = gumpIndexReader.ReadUInt16();
                ushort width = gumpIndexReader.ReadUInt16();

                List<GumpRow> rows = new List<GumpRow>();

                gumpDataReader.BaseStream.Seek(lookup, SeekOrigin.Begin);
                for (int y = 0; y < height; y++)
                {
                    GumpRow row = DecodeRow(gumpDataReader, width);
                    rows.Add(row);
                }

                return rows;
            }
        }
        #endregion

        #region GumpRun
        // Encodes a single row of pixels into runs of pixel values and their run lengths.
        // Parameters:
        //   - row: The GumpRow containing the pixel values to encode.
        // Returns:
        //   A list of GumpRun objects representing the encoded pixel runs.
        private List<GumpRun> EncodeRow(GumpRow row)
        {
            List<GumpRun> runs = new List<GumpRun>();
            ushort currentValue = row.Pixels[0];
            int currentRunLength = 1;

            for (int i = 1; i < row.Pixels.Length; i++)
            {
                ushort nextValue = row.Pixels[i];
                if (nextValue == currentValue)
                {
                    currentRunLength++;
                }
                else
                {
                    runs.Add(new GumpRun { Value = currentValue, Run = (ushort)currentRunLength });
                    currentValue = nextValue;
                    currentRunLength = 1;
                }
            }

            runs.Add(new GumpRun { Value = currentValue, Run = (ushort)currentRunLength });
            return runs;
        }
        #endregion

        #region DecodeRow
        // Decodes a single row of pixels from binary data using a BinaryReader.
        // Parameters:
        //   - reader: The BinaryReader instance used to read pixel data.
        //   - width: The width of the row.
        // Returns:
        //   A GumpRow containing the decoded pixel values.
        private GumpRow DecodeRow(BinaryReader reader, int width)
        {
            GumpRow row = new GumpRow { Pixels = new ushort[width] };
            int pixelIndex = 0;

            while (pixelIndex < width)
            {
                ushort value = reader.ReadUInt16();
                ushort run = reader.ReadUInt16();

                for (int i = 0; i < run; i++)
                {
                    row.Pixels[pixelIndex++] = value;
                }
            }

            return row;
        }
        #endregion

        #region  DisplayGumpInLabel
        // Displays the generated Gump on a label in the user interface.
        // Currently not utilized in the application.
        private void DisplayGumpInLabel(List<GumpRow> rows)
        {
            // Implement the code here to display the Gump in a label
            StringBuilder sb = new StringBuilder();
            foreach (GumpRow row in rows)
            {
                sb.AppendLine(string.Join(" ", row.Pixels));
            }
            gumpLabel.Text = sb.ToString();
        }
        #endregion

        #region GenerateGumpRows
        // Generates Gump rows based on the specified width and index size.
        // Parameters:
        //   - width: The width of each Gump row.
        //   - indexSize: The size of the Gump index.
        // Returns:
        //   A list of Gump rows containing randomly generated pixel values within the specified index size.

        private List<GumpRow> GenerateGumpRows(int width, int indexSize)
        {
            List<GumpRow> rows = new List<GumpRow>();
            Random random = new Random();
            for (int i = 0; i < indexSize; i++)
            {
                ushort[] pixels = new ushort[width];
                for (int x = 0; x < width; x++)
                {
                    pixels[x] = (ushort)random.Next(indexSize); // Random pixel values ​​within the index size
                }
                rows.Add(new GumpRow { Pixels = pixels });
            }
            return rows;
        }
        #endregion
        #region GumpRun
        // Represents a run of pixels in a Gump row, consisting of a pixel value and its run length.
        public struct GumpRun
        {
            public ushort Value;
            public ushort Run;
        }
        #endregion

        #region GumpRow
        // Represents a single row of pixels in a Gump, stored as an array of ushort values.
        public struct GumpRow
        {
            public ushort[] Pixels;
        }
        #endregion
        #endregion
    }
}
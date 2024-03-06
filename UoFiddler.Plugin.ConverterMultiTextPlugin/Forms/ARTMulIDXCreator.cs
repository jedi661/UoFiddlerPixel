
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using static UoFiddler.Plugin.ConverterMultiTextPlugin.Forms.ARTMulIDXCreator;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class ARTMulIDXCreator : Form
    {
        public ARTMulIDXCreator()
        {
            InitializeComponent();
        }

        #region btCreateARTIDXMul
        private void btCreateARTIDXMul_Click(object sender, EventArgs e)
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
        private void btFileOrder_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private void btnCountEntries_Click(object sender, EventArgs e)
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
        private void btnShowInfo_Click(object sender, EventArgs e)
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
        /*private void btnReadArtIdx_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = folderBrowserDialog.SelectedPath;

                    var artIndexFile = new ArtIndexFile();
                    string filePath = Path.Combine(folderBrowserDialog.SelectedPath, "artidx.MUL");
                    artIndexFile.LoadFromFile(filePath); // Angenommen, Sie haben eine LoadFromFile-Methode

                    int indexToRead = int.Parse(textBoxIndex.Text); // Angenommen, textBoxIndex ist die TextBox, in der Sie den zu lesenden Index angeben
                    if (indexToRead >= 0 && indexToRead < artIndexFile.CountEntries())
                    {
                        var entry = artIndexFile.GetEntry(indexToRead); // Angenommen, Sie haben eine GetEntry-Methode
                        textBoxInfo.Text = $"Eintrag {indexToRead}: Lookup={entry.Lookup}, Size={entry.Size}, Unknown={entry.Unknown}"; // Angenommen, textBoxInfo ist die TextBox, in der Sie die Informationen anzeigen möchten
                    }
                    else
                    {
                        textBoxInfo.Text = "Ungültiger Index";
                    }
                }
            }
        }*/

        private void btnReadArtIdx_Click(object sender, EventArgs e)
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
                        textBoxInfo.Text = $"Eintrag {indexToRead}: Lookup={entry.Lookup}, Size={entry.Size}, Unknown={entry.Unknown}"; // Angenommen, textBoxInfo ist die TextBox, in der Sie die Informationen anzeigen möchten
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
        private void btCreateARTIDXMul_uint_Click(object sender, EventArgs e)
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
        private void btCreateARTIDXMul_Int_Click(object sender, EventArgs e)
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
        private void btCreateARTIDXMul_Ushort_Click(object sender, EventArgs e)
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
        private void btCreateARTIDXMul_Short_Click(object sender, EventArgs e)
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
        private void btCreateARTIDXMul_Byte_Click(object sender, EventArgs e)
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
        private void btCreateARTIDXMul_Ulong_Click(object sender, EventArgs e)
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
        private void comboBoxMuls_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedValue = comboBoxMuls.SelectedItem.ToString();

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
        private void btCreateTiledata_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    tbDirTileData.Text = fbd.SelectedPath;
                    TileDataCreator creator = new TileDataCreator();

                    // Lesen Sie die Werte aus den Textboxen oder verwenden Sie Standardwerte
                    int landTileGroups = string.IsNullOrWhiteSpace(tblandTileGroups.Text) ? 16383 : int.Parse(tblandTileGroups.Text);
                    int staticTileGroups = string.IsNullOrWhiteSpace(tbstaticTileGroups.Text) ? 65535 : int.Parse(tbstaticTileGroups.Text);

                    // Erstellen Sie den vollständigen Pfad zur Datei
                    string filePath = Path.Combine(fbd.SelectedPath, "Tiledata.mul");

                    // Überprüfen Sie, ob die Checkbox aktiviert ist
                    bool createEmptyFile = checkBoxTileData.Checked;

                    creator.CreateTileData(filePath, landTileGroups, staticTileGroups, createEmptyFile);

                    // Aktualisieren Sie das Label, um anzuzeigen, dass die Datei erfolgreich erstellt wurde
                    lbTileDataCreate.Text = $"Die Tiledata.mul-Datei wurde erfolgreich erstellt in: {filePath}";
                }
            }
        }
        #endregion


        #region  Tiledatainfo
        private void btTiledatainfo_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    tbDirTileData.Text = fbd.SelectedPath;

                    // Erstellen Sie den vollständigen Pfad zur Datei
                    string filePath = Path.Combine(fbd.SelectedPath, "Tiledata.mul");

                    // Überprüfen Sie, ob die Datei existiert
                    if (!File.Exists(filePath))
                    {
                        textBoxTileDataInfo.Text = "Die Datei Tiledata.mul konnte nicht gefunden werden.";
                        return;
                    }

                    // Lesen Sie die Daten aus der Datei
                    using (var fs = File.OpenRead(filePath))
                    {
                        using (var reader = new BinaryReader(fs))
                        {
                            // Lesen Sie die Anzahl der Land Tile Groups
                            int landTileGroups = reader.ReadInt32();

                            // Lesen Sie die Anzahl der Static Tile Groups
                            int staticTileGroups = reader.ReadInt32();

                            // Zeigen Sie die Informationen in der Textbox an
                            textBoxTileDataInfo.Text = $"Land Tile Groups: {landTileGroups}\nStatic Tile Groups: {staticTileGroups}";
                        }
                    }
                }
            }
        }
        #endregion

        #region Create Button Empty
        private void btCreateTiledataEmpty_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    tbDirTileData.Text = fbd.SelectedPath;
                    TileDataCreator creator = new TileDataCreator();

                    // Lesen Sie die Werte aus den Textboxen oder verwenden Sie Standardwerte
                    int landTileGroups = string.IsNullOrWhiteSpace(tblandTileGroups.Text) ? 16383 : int.Parse(tblandTileGroups.Text);
                    int staticTileGroups = string.IsNullOrWhiteSpace(tbstaticTileGroups.Text) ? 65535 : int.Parse(tbstaticTileGroups.Text);

                    // Erstellen Sie den vollständigen Pfad zur Datei
                    string filePath = Path.Combine(fbd.SelectedPath, "Tiledata.mul");

                    // Erstellen Sie eine leere Tiledata.mul-Datei
                    creator.CreateTileData(filePath, landTileGroups, staticTileGroups, true);

                    // Aktualisieren Sie das Label, um anzuzeigen, dass die Datei erfolgreich erstellt wurde
                    lbTileDataCreate.Text = $"Die leere Tiledata.mul-Datei wurde erfolgreich erstellt in: {filePath}";
                }
            }
        }
        #endregion


        #region Crete Button Emtpy2
        private void btCreateTiledataEmpty2_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    TileDataCreator creator = new TileDataCreator();

                    // Setzen Sie die Anzahl der Land- und Static-Tile-Gruppen auf Standardwerte
                    int landTileGroups = 16383;
                    int staticTileGroups = 65535;

                    // Erstellen Sie den vollständigen Pfad zur Datei
                    string filePath = Path.Combine(fbd.SelectedPath, "Tiledata.mul");

                    // Erstellen Sie eine leere Tiledata.mul-Datei
                    creator.CreateTileData(filePath, landTileGroups, staticTileGroups, true);

                    // Zeigen Sie eine Meldung an, um anzuzeigen, dass die Datei erfolgreich erstellt wurde
                    MessageBox.Show($"Die leere Tiledata.mul-Datei wurde erfolgreich erstellt in: {filePath}");
                }
            }
        }
        #endregion

        #region ReadIndexTiledata
        private void btReadIndexTiledata_Click(object sender, EventArgs e)
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
                    string tileDataInfo = creator.ReadTileData(filePath, index, "Land"); // Zum Lesen von Land Tiles
                    string tileDataInfo2 = creator.ReadTileData(filePath, index, "Static"); // Zum Lesen von Static Tiles

                    textBoxTileDataInfo.Text = tileDataInfo;
                }
            }
        }
        #endregion

        #region Button Hex
        public void btTReadHexAndSelectDirectory_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string filePath = Path.Combine(fbd.SelectedPath, "Tiledata.mul");

                    // Überprüfen Sie, ob der Index in Hexadezimalform vorliegt
                    bool isHex = textBoxTiledataIndex.Text.StartsWith("0x");
                    int index;
                    if (isHex)
                    {
                        // Wenn der Index in Hexadezimalform vorliegt, konvertieren Sie ihn in eine Dezimalzahl
                        index = Convert.ToInt32(textBoxTiledataIndex.Text.Substring(2), 16);
                    }
                    else
                    {
                        index = int.Parse(textBoxTiledataIndex.Text);
                    }

                    // Multiplizieren Sie den Index mit der Größe des Blocks, um die richtige Position in der Datei zu erreichen
                    index *= 836; // Ersetzen Sie 836 durch die tatsächliche Größe des Blocks

                    using (var fs = File.OpenRead(filePath))
                    {
                        fs.Position = index;
                        byte[] buffer = new byte[836]; // Lesen Sie 836 Bytes ab der angegebenen Position
                        fs.Read(buffer, 0, buffer.Length);
                        string hex = BitConverter.ToString(buffer).Replace("-", " "); // Konvertieren Sie die Bytes in einen Hexadezimal-String

                        // Konvertieren Sie die Bytes in ASCII-Zeichen
                        string ascii = Encoding.ASCII.GetString(buffer);

                        textBoxTileDataInfo.Text = $"Die Bytes an der Position {index} in der Datei {filePath} sind:\n{hex}\n\nASCII:\n{ascii}";
                    }
                }
            }
        }
        #endregion

        #region Button Land
        public void btReadLandTile_Click(object sender, EventArgs e)
        {
            ReadTileData("Land");
        }
        #endregion

        #region Button Static
        public void btReadStaticTile_Click(object sender, EventArgs e)
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
        private void btnCountTileDataEntries_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    var artDataFile = new ArtDataFile();
                    string filePath = Path.Combine(folderBrowserDialog.SelectedPath, "Tiledata.mul");
                    artDataFile.LoadFromFileForCounting(filePath);
                    int numEntries = artDataFile.CountEntries();
                    lblTileDataEntryCount.Text = "Die Anzahl der Einträge ist: " + numEntries;
                }
            }
        }
        #endregion

        #region Button SimpleTiledata
        private void btCreateSimpleTiledata_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string filePath = Path.Combine(fbd.SelectedPath, "Tiledata.mul");

                    // Überprüfen Sie, ob die Datei bereits existiert. Wenn ja, löschen Sie sie.
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    // Erstellen Sie die Datei.
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

                    MessageBox.Show($"Die Tiledata.mul-Datei wurde erfolgreich erstellt in: {filePath}");
                }
            }
        }
        #endregion

        #region ReadTileFlags Button
        private void btReadTileFlags_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    tbDirTileData.Text = fbd.SelectedPath;

                    // Erstellen Sie den vollständigen Pfad zur Datei
                    string filePath = Path.Combine(fbd.SelectedPath, "Tiledata.mul");

                    // Überprüfen Sie, ob die Datei existiert
                    if (!File.Exists(filePath))
                    {
                        textBoxTileDataInfo.Text = "Die Datei Tiledata.mul konnte nicht gefunden werden.";
                        return;
                    }

                    // Lesen Sie die Daten aus der Datei
                    using (var fs = File.OpenRead(filePath))
                    {
                        using (var reader = new BinaryReader(fs))
                        {
                            // Lesen Sie die Anzahl der Land Tile Groups
                            int landTileGroups = reader.ReadInt32();

                            // Lesen Sie die Anzahl der Static Tile Groups
                            int staticTileGroups = reader.ReadInt32();

                            // Erstellen Sie eine neue Instanz von TileDataFlags
                            TileDataFlags flags = new TileDataFlags();

                            // Lesen Sie die Flags für jedes Tile
                            while (reader.BaseStream.Position != reader.BaseStream.Length)
                            {
                                // Lesen Sie die Flags aus der Datei
                                ulong flagValue = reader.ReadUInt64();

                                // Interpretieren Sie die Flags mit der TileDataFlags-Klasse
                                flags.Value = flagValue;

                                // Fügen Sie die Flags zu einer Liste hinzu
                                List<ulong> flagList = new List<ulong>();
                                flagList.Add(flagValue);

                                // Verwenden Sie die Flags hier...
                                foreach (ulong flag in flagList)
                                {
                                    // Verarbeiten Sie jedes Flag...
                                }
                            }

                            // Zeigen Sie die Informationen in der Textbox an
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
            private List<ArtIndexEntry> entries;

            #region
            public ArtIndexFile()
            {
                entries = new List<ArtIndexEntry>();
            }
            #endregion

            #region ArtIndexEntry GetEntry
            public ArtIndexEntry GetEntry(int index)
            {
                if (index >= 0 && index < entries.Count)
                {
                    return entries[index];
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
                entries.Add(entry);
            }
            #endregion

            #region LoadFromFile
            public void LoadFromFile(string filename)
            {
                entries.Clear(); // Delete all existing entries

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
                            entries.Add(entry);
                        }
                    }
                }
            }
            #endregion

            #region CountEntries
            public int CountEntries()
            {
                return entries.Count;
            }
            #endregion

            #region  SaveToFile
            public void SaveToFile(string filename)
            {
                using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        foreach (var entry in entries)
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
            private Dictionary<string, ulong> flagNameMasks = new Dictionary<string, ulong>
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

            private ulong value;
            public ulong Value { get; set; }

            public TileDataFlags()
            {
                // Initialisieren Sie Ihre Flags hier, falls erforderlich
            }

            public TileDataFlags(ulong flagValue)
            {
                this.Value = flagValue;
            }

            public TileDataFlags(string flagValue)
            {
                // Zerlegen Sie den Eingabestring in einzelne Flag-Namen
                var flagNames = flagValue.Split(',');

                // Durchlaufen Sie jeden Flag-Namen
                foreach (var flagName in flagNames)
                {
                    // Entfernen Sie Leerzeichen
                    var trimmedFlagName = flagName.Trim();

                    // Überprüfen Sie, ob der Flag-Name gültig ist
                    if (flagNameMasks.ContainsKey(trimmedFlagName))
                    {
                        // Wenn ja, setzen Sie das entsprechende Bit in `value`
                        value |= flagNameMasks[trimmedFlagName];
                    }
                    else
                    {
                        // Wenn nicht, werfen Sie eine Ausnahme oder behandeln Sie den Fehler auf geeignete Weise
                        throw new ArgumentException("Ungültiger Flag-Name: " + trimmedFlagName);
                    }
                }
            }


            public ulong MaskForName(string flagName)
            {
                if (flagNameMasks.TryGetValue(flagName, out ulong mask))
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
                    this.value |= mask;
                }
                else
                {
                    this.value &= ~mask;
                }
            }

            public bool BitValue(int bit)
            {
                ulong mask = (ulong)1 << bit;
                return (value & mask) != 0;
            }
        }
        #endregion

        #region class ArtDataFile
        public class ArtDataFile
        {
            private List<ArtDataEntry> entries;

            public ArtDataFile()
            {
                entries = new List<ArtDataEntry>();
            }

            public ArtDataEntry GetEntry(int index)
            {
                if (index >= 0 && index < entries.Count)
                {
                    return entries[index];
                }
                else
                {
                    return null; // or throw an exception
                }
            }

            #region CountEntries
            public int CountEntries()
            {
                return entries.Count;
            }
            #endregion

            /*public void LoadFromFile(string filename)
            {
                entries.Clear(); // Löschen Sie alle vorhandenen Einträge

                using (var fs = new FileStream(filename, FileMode.Open))
                {
                    using (var reader = new BinaryReader(fs))
                    {
                        while (reader.BaseStream.Position != reader.BaseStream.Length) // Lesen Sie bis zum Ende der Datei
                        {
                            // Lesen Sie die Metadaten für das Bild
                            uint lookup = reader.ReadUInt32();
                            uint size = reader.ReadUInt32();
                            uint unknown = reader.ReadUInt32();

                            // Setzen Sie die Position des Readers auf den Anfang des Bildes
                            reader.BaseStream.Position = lookup;

                            // Lesen Sie die Bilddaten aus der Datei
                            byte[] imageData = reader.ReadBytes((int)size);

                            // Lesen Sie die Flagge
                            uint flag = BitConverter.ToUInt32(imageData, 0);

                            Bitmap image;
                            if (flag > 0xFFFF || flag == 0)
                            {
                                // Das Bild ist roh
                                image = LoadRawImage(imageData);
                            }
                            else
                            {
                                // Das Bild ist ein Laufbild
                                image = LoadRunImage(imageData);
                            }

                            var entry = new ArtDataEntry(image);
                            entries.Add(entry);
                        }
                    }
                }
            }*/

            #region LoadFromFile
            public void LoadFromFile(string filename)
            {
                entries.Clear(); // Delete all existing entries

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
                                entries.Add(entry);
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


            /*private Bitmap LoadRawImage(byte[] imageData)
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
                        ushort pixelData = BitConverter.ToUInt16(imageData, index);
                        Color pixelColor = Color.FromArgb(
                            ((pixelData >> 10) & 0x1F) * 0xFF / 0x1F,
                            ((pixelData >> 5) & 0x1F) * 0xFF / 0x1F,
                            (pixelData & 0x1F) * 0xFF / 0x1F);
                        image.SetPixel(j, i, pixelColor);

                        index += 2;
                    }
                }

                return image;
            }*/

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
                        // Überprüfen Sie, ob der Index innerhalb des gültigen Bereichs liegt
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
                            // Zum Beispiel könnten Sie eine Ausnahme auslösen oder einen Standardwert zuweisen
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
                entries.Add(entry);
            }
            #endregion

            #region GetImage
            public Bitmap GetImage(int index)
            {
                if (index >= 0 && index < entries.Count)
                {
                    return entries[index].Image;
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
                        foreach (var entry in entries)
                        {
                            entry.WriteToStream(writer);
                        }
                    }
                }
            }
            #endregion

            /*public void LoadFromFileForCounting(string filename)
            {
                entries.Clear(); // Delete all existing entries

                using (var fs = new FileStream(filename, FileMode.Open))
                {
                    using (var reader = new BinaryReader(fs))
                    {
                        // Read Land Tile Groups
                        for (int i = 0; i < 512; i++)
                        {
                            if (reader.BaseStream.Position + 836 <= reader.BaseStream.Length)
                            {
                                reader.BaseStream.Position += 836; // Skip Land Tile Group
                                entries.Add(new ArtDataEntry(null));
                            }
                            else
                            {
                                break;
                            }
                        }

                        // Read Static Tile Groups until the end of the file
                        while (reader.BaseStream.Position + 1188 <= reader.BaseStream.Length)
                        {
                            reader.BaseStream.Position += 1188; // Skip Static Tile Group
                            entries.Add(new ArtDataEntry(null));
                        }
                    }
                }
            }*/
            #region LoadFromFileForCounting
            public void LoadFromFileForCounting(string filename)
            {
                entries.Clear(); // Delete all existing entries

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
                        entries.Add(new ArtDataEntry(null));
                        numLandTileGroups++;
                    }

                    // Calculate the number of Static Tile Groups
                    int numStaticTileGroups = 0;
                    while (fs.Position + staticTileGroupSize <= fileSize)
                    {
                        fs.Position += staticTileGroupSize; // Skip Static Tile Group
                        entries.Add(new ArtDataEntry(null));
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
                // Überprüfen Sie, ob die Datei bereits existiert. Wenn ja, löschen Sie sie.
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Erstellen Sie die Datei.
                using (var fs = File.Create(filePath))
                {
                    for (int i = 0; i < landTileGroups; i++)
                    {
                        // Schreiben Sie die Land Tile Group in die Datei.
                        WriteLandTileGroup(fs, createEmptyFile);
                    }

                    for (int i = 0; i < staticTileGroups; i++)
                    {
                        // Schreiben Sie die Static Tile Group in die Datei.
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
            // nicht genützt
            public string ReadTileData2(string filePath, int index)
            {
                using (var fs = File.OpenRead(filePath))
                {
                    // Berechnen Sie die Position des angegebenen Index in der Datei
                    long position = index * 26; // Jedes Tile ist 26 Bytes groß

                    // Stellen Sie sicher, dass die Position innerhalb der Datei liegt
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

                            // Konvertieren Sie die Flags in eine Sequenz von Bit-Werten
                            string flagString = Convert.ToString(flags, 2).PadLeft(32, '0');

                            // Erzeugen Sie die Ausgabe
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
                    // Berechnen Sie die Position des angegebenen Index in der Datei
                    long position;
                    if (tileType == "Land")
                    {
                        position = index * 26; // Jedes Land Tile ist 26 Bytes groß
                    }
                    else // Static
                    {
                        position = index * 37; // Jedes Static Tile ist 37 Bytes groß
                    }

                    // Stellen Sie sicher, dass die Position innerhalb der Datei liegt
                    if (position < fs.Length)
                    {
                        fs.Position = position;

                        using (var reader = new BinaryReader(fs))
                        {
                            uint flags = reader.ReadUInt32();
                            // ... (restlicher Code zum Lesen anderer Attribute) ...

                            // Konvertieren Sie die Flags in eine Sequenz von Bit-Werten
                            string flagString = Convert.ToString(flags, 2).PadLeft(32, '0');

                            // Erzeugen Sie die Ausgabe
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
            private string directory;
            private string filePath;

            public TileData2(string directory)
            {
                this.directory = directory;
                this.filePath = Path.Combine(this.directory, "TileData.mul");
            }

            public void CreateTileData(int landBlockCount, int staticBlockCount)
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(this.filePath, FileMode.Create)))
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
                using (BinaryReader reader = new BinaryReader(File.Open(this.filePath, FileMode.Open)))
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
            private string texMapsFileName;
            private string texIdxFileName;

            public TextureFileCreator(string texMapsFileName, string texIdxFileName)
            {
                this.texMapsFileName = texMapsFileName;
                this.texIdxFileName = texIdxFileName;
            }

            public void CreateFiles(int extra, short[] imageColors)
            {
                int width = (extra == 1 ? 128 : 64);
                int height = width;

                // Create TexMaps.mul file
                using (BinaryWriter texMapsFile = new BinaryWriter(File.Open(texMapsFileName, FileMode.Create)))
                {
                    foreach (short color in imageColors)
                    {
                        texMapsFile.Write(color);
                    }
                }

                // Create TexIdx.mul file
                using (BinaryWriter texIdxFile = new BinaryWriter(File.Open(texIdxFileName, FileMode.Create)))
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
            private short[] colors;

            public RadarColors(string filePath)
            {
                colors = new short[0x8000];
                LoadColorsFromFile(filePath);
            }

            private void LoadColorsFromFile(string filePath)
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Die Datei {filePath} wurde nicht gefunden.");
                }

                using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
                {
                    for (int i = 0; i < colors.Length; i++)
                    {
                        colors[i] = reader.ReadInt16();
                    }
                }
            }

            public short GetColor(int index)
            {
                if (index < 0 || index >= colors.Length)
                {
                    throw new IndexOutOfRangeException("Der Index liegt außerhalb des gültigen Bereichs.");
                }

                return colors[index];
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
                    // Hier setzen wir die R-, G- und B-Werte auf i, 
                    // Sie können diese Werte jedoch nach Belieben ändern.
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
                // Initialisieren Sie die Eigenschaften entsprechend
            }
        }

        public class DataStream
        {
            public ushort RowHeader { get; set; }
            private ushort _RowOfs;

            public DataStream()
            {
                // Initialisieren Sie die Eigenschaften entsprechend
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
                    return _RowOfs & 0x3F;
                }
            }

            public int RowOfs
            {
                get
                {
                    return _RowOfs >> 6;
                }
            }
        }
        #endregion

        public class AnimData
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 26, Pack = 1)]
            private unsafe struct OldLandData
            {
                private uint _Flags;
                private ushort _TexID;
                public fixed byte _Name[20]; // Ändern Sie den Zugriffsmodifizierer auf public
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 37, Pack = 1)]
            private unsafe struct OldItemData
            {
                private uint _Flags;
                private byte _Weight;
                private byte _Quality;
                private ushort _Miscdata;
                private byte _Unk1;
                private byte _Quantity;
                private ushort _Animation;
                private byte _Unk2;
                private byte _Hue;
                private byte _StackingOff;
                private byte _Value;
                private byte _Height;
                public fixed byte _Name[20]; // Ändern Sie den Zugriffsmodifizierer auf public
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 41, Pack = 1)]
            private unsafe struct NewItemData
            {
                [MarshalAs(UnmanagedType.U8)]
                private ulong _Flags;
                private byte _Weight;
                private byte _Quality;
                private ushort _Miscdata;
                private byte _Unk1;
                private byte _Quantity;
                private ushort _Animation;
                private byte _Unk2;
                private byte _Hue;
                private byte _StackingOff;
                private byte _Value;
                private byte _Height;
                public fixed byte _Name[20]; // Ändern Sie den Zugriffsmodifizierer auf public
            }

            public unsafe void ReadAndDisplayData(string filePath, System.Windows.Forms.TextBox textBox) // Fügen Sie das Schlüsselwort unsafe hinzu
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

                        // Angenommen, GetName() ist eine Funktion, die den Namen des Artikels aus den Rohdaten abruft
                        string name = GetName(data._Name);
                        textBox.AppendText(name + "\n");
                    }
                }
            }

            private unsafe string GetName(byte* name) // Fügen Sie das Schlüsselwort unsafe hinzu
            {
                var bytes = new byte[20];
                for (int i = 0; i < 20; i++)
                {
                    bytes[i] = name[i];
                }

                return Encoding.ASCII.GetString(bytes).TrimEnd('\0');
            }
        }


        private BinaryReader reader;
        private int itemsLoaded = 0;


        #region LoadItems
        private void LoadItems()
        {
            // Leeren Sie das ListView, wenn es das erste Mal geladen wird
            if (itemsLoaded == 0)
            {
                listViewTileData.Items.Clear();
            }

            // Laden Sie bis zu 50 Elemente
            for (int i = 0; i < 50 && reader.BaseStream.Position < reader.BaseStream.Length; i++)
            {
                // Lesen Sie die Daten für jede Land-Kachel-Gruppe
                ulong flags = reader.ReadUInt64();
                ushort textureId = reader.ReadUInt16();
                string tileName = Encoding.Default.GetString(reader.ReadBytes(20));

                // Fügen Sie die Informationen als neuen Eintrag in das ListView ein
                ListViewItem item = new ListViewItem(new string[]
                {
            reader.BaseStream.Position.ToString("X"),
            tileName,
            textureId.ToString(),
            Convert.ToString((long)flags, 2).PadLeft(33, '0')
                });

                // Füge die Flag-Werte hinzu
                for (int j = 0; j < 33; j++)
                {
                    bool flagJ = (flags & (1UL << j)) != 0;
                    item.SubItems.Add(flagJ.ToString());
                }

                listViewTileData.Items.Add(item);
                itemsLoaded++;
            }
        }
        #endregion

        #region TiledataHex Keydown
        private void TiledataHex_KeyDown(object sender, KeyEventArgs e)
        {
            // Prüfen Sie, ob die Leertaste gedrückt wurde
            if (e.KeyCode == Keys.Space)
            {
                if (landItemsLoaded < 16000)
                {
                    LoadLandTiles();
                }
                else if (staticItemsLoaded < 65500)
                {
                    LoadStaticTiles();
                }
            }
        }
        #endregion

        #region buttonReadTileData
        private void buttonReadTileData_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                reader = new BinaryReader(File.Open(openFileDialog.FileName, FileMode.Open));
                LoadItems();
            }
        }
        #endregion

        private int landItemsLoaded = 0;
        private int staticItemsLoaded = 0;

        #region LoadLandTiles
        private void LoadLandTiles()
        {
            // Laden Sie bis zu 50 Elemente
            for (int i = 0; i < 50 && reader.BaseStream.Position < reader.BaseStream.Length; i++)
            {
                // Lesen Sie die Daten für jede Land-Kachel-Gruppe
                uint flags = reader.ReadUInt32();
                Console.WriteLine("Flags: " + flags);
                ushort textureId = reader.ReadUInt16();
                Console.WriteLine("Texture ID: " + textureId);
                string tileName = Encoding.Default.GetString(reader.ReadBytes(20));
                Console.WriteLine("Tile Name: " + tileName);

                // Fügen Sie die Informationen als neuen Eintrag in das ListView ein
                ListViewItem item = new ListViewItem(new string[]
                {
            reader.BaseStream.Position.ToString("X"),
            flags.ToString(),
            textureId.ToString(),
            tileName
                });

                listViewTileData.Items.Add(item);
                landItemsLoaded++;
            }
        }
        #endregion

        #region LoadStaticTiles
        private void LoadStaticTiles()
        {
            // Laden Sie bis zu 50 Elemente
            for (int i = 0; i < 50 && reader.BaseStream.Position < reader.BaseStream.Length; i++)
            {
                uint unknown = reader.ReadUInt32();
                uint flags = reader.ReadUInt32();
                byte weight = reader.ReadByte();
                byte quality = reader.ReadByte();
                ushort unknown1 = reader.ReadUInt16();
                byte unknown2 = reader.ReadByte();
                byte quantity = reader.ReadByte();
                ushort animId = reader.ReadUInt16();
                byte unknown3 = reader.ReadByte();
                byte hue = reader.ReadByte();
                ushort unknown4 = reader.ReadUInt16();
                byte height = reader.ReadByte();
                string tileName = Encoding.Default.GetString(reader.ReadBytes(20));

                // Fügen Sie die Informationen als neuen Eintrag in das ListView ein
                ListViewItem item = new ListViewItem(new string[]
                {
            reader.BaseStream.Position.ToString("X"),
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
                staticItemsLoaded++;
            }
        }
        #endregion

        #region buttonReadLandTileData
        private void buttonReadLandTileData_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                reader = new BinaryReader(File.Open(openFileDialog.FileName, FileMode.Open));
                LoadLandTiles();
            }
        }
        #endregion

        #region buttonReadStaticTileData
        private void buttonReadStaticTileData_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                reader = new BinaryReader(File.Open(openFileDialog.FileName, FileMode.Open));
                LoadStaticTiles();
            }
        }
        #endregion

        #region listViewTileData
        private void listViewTileData_MouseClick(object sender, MouseEventArgs e)
        {
            if (listViewTileData.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewTileData.SelectedItems[0];
                foreach (ListViewItem item in listViewTileData.Items)
                {
                    string details = "";
                    // Iterieren Sie durch alle SubItems des aktuellen Elements
                    foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
                    {
                        // Fügen Sie den Text des SubItems zu den Details hinzu
                        details += subItem.Text + "\n";
                    }
                    // Fügen Sie die Details zur TextBox hinzu
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
                indexCount.Text = $"Erstellte Landblöcke: {landCount * 32}\n" +
                                  $"Erstellte statische Blöcke: {staticCount * 32}\n" +
                                  $"Gesamtzahl der erstellten Indizes: {totalIndices}";
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
                indexCount.Text = $"Anzahl der Indizes: {count}";
            }
        }
        #endregion       

        #region Create Teture imagees
        private TextureFileCreator textureFileCreator;

        private void btCreateTextur_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select the directory that you want to use as the default.";
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string folder = dialog.SelectedPath;

                    // Bestimmen Sie die Anzahl der zu erstellenden Indizes
                    int indexCount;
                    if (string.IsNullOrEmpty(tbIndexCount.Text) || !int.TryParse(tbIndexCountTexture.Text, out indexCount))
                    {
                        indexCount = 16383;
                    }

                    // Erstellen Sie die BinaryWriter Instanzen außerhalb der Schleife
                    using (BinaryWriter texMapsFile = new BinaryWriter(File.Open(Path.Combine(folder, "TexMaps.mul"), FileMode.Create)))
                    using (BinaryWriter texIdxFile = new BinaryWriter(File.Open(Path.Combine(folder, "TexIdx.mul"), FileMode.Create)))
                    {
                        for (int i = 0; i < indexCount; i++)
                        {
                            // Erstellen Sie hier Ihr imageColors Array
                            short[] imageColors;
                            int extra;
                            if (i % 2 == 0)
                            {
                                imageColors = new short[64 * 64]; // Beispiel: ein 64x64 Bild mit allen Pixeln schwarz
                                extra = 0;
                            }
                            else
                            {
                                imageColors = new short[128 * 128]; // Beispiel: ein 128x128 Bild mit allen Pixeln schwarz
                                extra = 1;
                            }

                            // Schreiben Sie die Bilddaten in die TexMaps.mul Datei
                            // nur wenn die CheckBox nicht aktiviert ist oder dies die ersten beiden Bilder sind
                            if (!checkBoxTexture.Checked || i < 2)
                            {
                                foreach (short color in imageColors)
                                {
                                    texMapsFile.Write(color);
                                }
                            }

                            // Schreiben Sie die Indexdaten in die TexIdx.mul Datei
                            int width = (extra == 1 ? 128 : 64);
                            texIdxFile.Write(width); // width
                            texIdxFile.Write(width); // height
                            texIdxFile.Write(extra);  // extra
                        }
                    }

                    // Aktualisieren Sie das Label und die TextBox, um die Anzahl der erstellten Indizes anzuzeigen
                    lbTextureCount.Text = $"Erstellte Indizes: {indexCount}";
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

                    // Bestimmen Sie die Anzahl der zu erstellenden Indizes
                    int indexCount;
                    if (string.IsNullOrEmpty(tbIndexCount.Text) || !int.TryParse(tbIndexCountTexture.Text, out indexCount))
                    {
                        indexCount = 16383;
                    }

                    // Erstellen Sie die BinaryWriter Instanz für die TexIdx.mul Datei
                    using (BinaryWriter texIdxFile = new BinaryWriter(File.Open(Path.Combine(folder, "TexIdx.mul"), FileMode.Create)))
                    {
                        for (int i = 0; i < indexCount; i++)
                        {
                            // Schreiben Sie die Indexdaten in die TexIdx.mul Datei
                            int width = (i % 2 == 0 ? 64 : 128);
                            texIdxFile.Write(width); // width
                            texIdxFile.Write(width); // height
                            texIdxFile.Write(i % 2);  // extra
                        }
                    }

                    // Erstellen Sie eine leere TexMaps.mul Datei
                    using (File.Create(Path.Combine(folder, "TexMaps.mul"))) { }

                    // Aktualisieren Sie das Label und die TextBox, um die Anzahl der erstellten Indizes anzuzeigen
                    lbTextureCount.Text = $"Erstellte Indizes: {indexCount}";
                    tbIndexCount.Text = indexCount.ToString();
                }
            }
        }
        #endregion

        #region Create RadarColor
        private void CreateFileButtonRadarColor_Click(object sender, EventArgs e)
        {
            // Setzen Sie den Standarddateinamen
            saveFileDialog.FileName = "RadarCol.mul";

            // Zeigen Sie den Dialog zum Speichern der Datei an
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Lesen Sie die Anzahl der Indizes aus der TextBox
                int indexCount;
                if (!int.TryParse(indexCountTextBox.Text, out indexCount))
                {
                    // Zeigen Sie eine Fehlermeldung an, wenn die Eingabe ungültig ist
                    lbRadarColorInfo.Text = "Bitte geben Sie eine gültige Zahl ein.";
                    return;
                }

                // Erstellen Sie die RadarCol.mul-Datei
                CreateRadarColFile(saveFileDialog.FileName, indexCount);
            }
        }

        private void CreateRadarColFile(string filePath, int indexCount)
        {
            short[] colors = new short[indexCount];
            // Hier können Sie die Farben nach Ihren Wünschen einstellen.

            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    writer.Write(colors[i]);
                }
            }

            lbRadarColorInfo.Text = "Die Datei wurde erfolgreich erstellt!";
        }
        #endregion

        #region Create Orginal Palette

        private Palette palette = new Palette();

        private void btCreatePalette_Click(object sender, EventArgs e)
        {
            // Erstellen Sie die Palette und fügen Sie die gewünschten Farben hinzu
            CreateDefaultPalette();

            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string filename = Path.Combine(folderBrowserDialog.SelectedPath, "Palette.mul");
                    palette.Save(filename);
                }
            }
        }
        #endregion

        #region CreateDefaultPalette Orginal Colors
        private void CreateDefaultPalette()
        {
            palette = new Palette();

            // Fügen Sie hier die gewünschten RGB-Werte hinzu
            palette.AddColor(Color.FromArgb(0, 0, 0)); // RGB Value 0: R=0, G=0, B=0
            palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 1: R=255, G=0, B=255
            palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 2: R=255, G=0, B=255
            palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 3: R=255, G=0, B=255
            palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 4: R=255, G=0, B=255
            palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 5: R=255, G=0, B=255
            palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 6: R=255, G=0, B=255
            palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 7: R=255, G=0, B=255
            palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 8: R=255, G=0, B=255
            palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 9: R=255, G=0, B=255
            palette.AddColor(Color.FromArgb(199, 75, 0)); // RGB Value 10: R=199, G=75, B=0
            palette.AddColor(Color.FromArgb(207, 99, 0)); // RGB Value 11: R=207, G=99, B=0
            palette.AddColor(Color.FromArgb(215, 127, 0)); // RGB Value 12: R=215, G=127, B=0
            palette.AddColor(Color.FromArgb(227, 155, 0)); // RGB Value 13: R=227, G=155, B=0
            palette.AddColor(Color.FromArgb(235, 187, 0)); // RGB Value 14: R=235, G=187, B=0
            palette.AddColor(Color.FromArgb(243, 219, 0)); // RGB Value 15: R=243, G=219, B=0
            palette.AddColor(Color.FromArgb(255, 255, 0)); // RGB Value 16: R=255, G=255, B=0
            palette.AddColor(Color.FromArgb(255, 255, 95)); // RGB Value 17: R=255, G=255, B=95
            palette.AddColor(Color.FromArgb(255, 255, 147)); // RGB Value 18: R=255, G=255, B=147
            palette.AddColor(Color.FromArgb(255, 255, 195)); // RGB Value 19: R=255, G=255, B=195
            palette.AddColor(Color.FromArgb(255, 255, 247)); // RGB Value 20: R=255, G=255, B=247
            palette.AddColor(Color.FromArgb(59, 27, 7)); // RGB Value 21: R=59, G=27, B=7
            palette.AddColor(Color.FromArgb(67, 31, 7)); // RGB Value 22: R=67, G=31, B=7
            palette.AddColor(Color.FromArgb(75, 35, 11)); // RGB Value 23: R=75, G=35, B=11
            palette.AddColor(Color.FromArgb(83, 43, 15)); // RGB Value 24: R=83, G=43, B=15
            palette.AddColor(Color.FromArgb(91, 47, 23)); // RGB Value 25: R=91, G=47, B=23
            palette.AddColor(Color.FromArgb(99, 55, 27)); // RGB Value 26: R=99, G=55, B=27
            palette.AddColor(Color.FromArgb(107, 63, 31)); // RGB Value 27: R=107, G=63, B=31
            palette.AddColor(Color.FromArgb(115, 67, 39)); // RGB Value 28: R=115, G=67, B=39
            palette.AddColor(Color.FromArgb(123, 75, 47)); // RGB Value 29: R=123, G=75, B=47
            palette.AddColor(Color.FromArgb(131, 83, 51)); // RGB Value 30: R=131, G=83, B=51
            palette.AddColor(Color.FromArgb(139, 91, 59)); // RGB Value 31: R=139, G=91, B=59
            palette.AddColor(Color.FromArgb(147, 99, 67)); // RGB Value 32: R=147, G=99, B=67
            palette.AddColor(Color.FromArgb(155, 107, 79)); // RGB Value 33: R=155, G=107, B=79
            palette.AddColor(Color.FromArgb(163, 115, 87)); // RGB Value 34: R=163, G=115, B=87
            palette.AddColor(Color.FromArgb(171, 127, 95)); // RGB Value 35: R=171, G=127, B=95
            palette.AddColor(Color.FromArgb(179, 135, 107)); // RGB Value 36: R=179, G=135, B=107
            palette.AddColor(Color.FromArgb(187, 147, 115)); // RGB Value 37: R=187, G=147, B=115
            palette.AddColor(Color.FromArgb(195, 155, 127)); // RGB Value 38: R=195, G=155, B=127
            palette.AddColor(Color.FromArgb(203, 167, 139)); // RGB Value 39: R=203, G=167, B=139
            palette.AddColor(Color.FromArgb(211, 175, 151)); // RGB Value 40: R=211, G=175, B=151
            palette.AddColor(Color.FromArgb(219, 187, 163)); // RGB Value 41: R=219, G=187, B=163
            palette.AddColor(Color.FromArgb(231, 199, 179)); // RGB Value 42: R=231, G=199, B=179
            palette.AddColor(Color.FromArgb(239, 211, 191)); // RGB Value 43: R=239, G=211, B=191
            palette.AddColor(Color.FromArgb(247, 223, 207)); // RGB Value 44: R=247, G=223, B=207
            palette.AddColor(Color.FromArgb(59, 59, 43)); // RGB Value 45: R=59, G=59, B=43
            palette.AddColor(Color.FromArgb(79, 79, 59)); // RGB Value 46: R=79, G=79, B=59
            palette.AddColor(Color.FromArgb(103, 103, 79)); // RGB Value 47: R=103, G=103, B=79
            palette.AddColor(Color.FromArgb(127, 127, 103)); // RGB Value 48: R=127, G=127, B=103
            palette.AddColor(Color.FromArgb(147, 147, 123)); // RGB Value 49: R=147, G=147, B=123
            palette.AddColor(Color.FromArgb(171, 171, 147)); // RGB Value 50: R=171, G=171, B=147
            palette.AddColor(Color.FromArgb(195, 195, 171)); // RGB Value 51: R=195, G=195, B=171
            palette.AddColor(Color.FromArgb(219, 219, 199)); // RGB Value 52: R=219, G=219, B=199
            palette.AddColor(Color.FromArgb(39, 0, 0)); // RGB Value 53: R=39, G=0, B=0
            palette.AddColor(Color.FromArgb(51, 0, 0)); // RGB Value 54: R=51, G=0, B=0
            palette.AddColor(Color.FromArgb(67, 11, 7)); // RGB Value 55: R=67, G=11, B=7
            palette.AddColor(Color.FromArgb(79, 19, 11)); // RGB Value 56: R=79, G=19, B=11
            palette.AddColor(Color.FromArgb(95, 31, 15)); // RGB Value 57: R=95, G=31, B=15
            palette.AddColor(Color.FromArgb(111, 43, 27)); // RGB Value 58: R=111, G=43, B=27
            palette.AddColor(Color.FromArgb(123, 59, 35)); // RGB Value 59: R=123, G=59, B=35
            palette.AddColor(Color.FromArgb(60, 139, 75)); // RGB Value 60 R=60, G=139, B=75
            palette.AddColor(Color.FromArgb(151, 91, 59)); // RGB Value 61: R=151, G=91, B=59
            palette.AddColor(Color.FromArgb(167, 111, 71)); // RGB Value 62: R=167, G=111, B=71
            palette.AddColor(Color.FromArgb(183, 127, 87)); // RGB Value 63: R=183, G=127, B=87
            palette.AddColor(Color.FromArgb(195, 147, 103)); // RGB Value 64: R=195, G=147, B=103
            palette.AddColor(Color.FromArgb(211, 167, 123)); // RGB Value 65: R=211, G=167, B=123
            palette.AddColor(Color.FromArgb(223, 187, 143)); // RGB Value 66: R=223, G=187, B=143
            palette.AddColor(Color.FromArgb(239, 211, 163)); // RGB Value 67: R=239, G=211, B=163
            palette.AddColor(Color.FromArgb(255, 231, 187)); // RGB Value 68: R=255, G=231, B=187
            palette.AddColor(Color.FromArgb(23, 23, 0)); // RGB Value 69: R=23, G=23, B=0
            palette.AddColor(Color.FromArgb(35, 35, 0)); // RGB Value 70: R=35, G=35, B=0
            palette.AddColor(Color.FromArgb(51, 47, 0)); // RGB Value 71: R=51, G=47, B=0
            palette.AddColor(Color.FromArgb(67, 63, 7)); // RGB Value 72: R=67, G=63, B=7
            palette.AddColor(Color.FromArgb(83, 75, 15)); // RGB Value 73: R=83, G=75, B=15
            palette.AddColor(Color.FromArgb(99, 87, 19)); // RGB Value 74: R=99, G=87, B=19
            palette.AddColor(Color.FromArgb(111, 99, 27)); // RGB Value 75: R=111, G=99, B=27
            palette.AddColor(Color.FromArgb(127, 111, 39)); // RGB Value 76: R=127, G=111, B=39
            palette.AddColor(Color.FromArgb(143, 123, 51)); // RGB Value 77: R=143, G=123, B=51
            palette.AddColor(Color.FromArgb(159, 135, 63)); // RGB Value 78: R=159, G=135, B=63
            palette.AddColor(Color.FromArgb(175, 147, 79)); // RGB Value 79: R=175, G=147, B=79
            palette.AddColor(Color.FromArgb(187, 159, 95)); // RGB Value 80: R=187, G=159, B=95
            palette.AddColor(Color.FromArgb(203, 175, 111)); // RGB Value 81: R=203, G=175, B=111
            palette.AddColor(Color.FromArgb(219, 187, 131)); // RGB Value 82: R=219, G=187, B=131
            palette.AddColor(Color.FromArgb(235, 203, 147)); // RGB Value 83: R=235, G=203, B=147
            palette.AddColor(Color.FromArgb(251, 219, 171)); // RGB Value 84: R=251, G=219, B=171
            palette.AddColor(Color.FromArgb(0, 0, 43)); // RGB Value 85: R=0, G=0, B=43
            palette.AddColor(Color.FromArgb(0, 0, 55)); // RGB Value 86: R=0, G=0, B=55
            palette.AddColor(Color.FromArgb(7, 7, 71)); // RGB Value 87: R=7, G=7, B=71
            palette.AddColor(Color.FromArgb(15, 15, 83)); // RGB Value 88: R=15, G=15, B=83
            palette.AddColor(Color.FromArgb(23, 23, 99)); // RGB Value 89: R=23, G=23, B=99
            palette.AddColor(Color.FromArgb(31, 31, 111)); // RGB Value 90: R=31, G=31, B=111
            palette.AddColor(Color.FromArgb(43, 43, 127)); // RGB Value 91: R=43, G=43, B=127
            palette.AddColor(Color.FromArgb(59, 59, 139)); // RGB Value 92: R=59, G=59, B=139
            palette.AddColor(Color.FromArgb(75, 75, 155)); // RGB Value 93: R=75, G=75, B=155
            palette.AddColor(Color.FromArgb(91, 91, 167)); // RGB Value 94: R=91, G=91, B=167
            palette.AddColor(Color.FromArgb(111, 111, 183)); // RGB Value 95: R=111, G=111, B=183
            palette.AddColor(Color.FromArgb(131, 131, 195)); // RGB Value 96: R=131, G=131, B=195
            palette.AddColor(Color.FromArgb(155, 155, 211)); // RGB Value 97: R=155, G=155, B=211
            palette.AddColor(Color.FromArgb(179, 179, 227)); // RGB Value 98: R=179, G=179, B=227
            palette.AddColor(Color.FromArgb(207, 207, 239)); // RGB Value 99: R=207, G=207, B=239
            palette.AddColor(Color.FromArgb(235, 235, 255)); // RGB Value 100: R=235, G=235, B=255
            palette.AddColor(Color.FromArgb(27, 27, 111)); // RGB Value 101: R=27, G=27, B=111
            palette.AddColor(Color.FromArgb(35, 39, 131)); // RGB Value 102: R=35, G=39, B=131
            palette.AddColor(Color.FromArgb(47, 55, 151)); // RGB Value 103: R=47, G=55, B=151
            palette.AddColor(Color.FromArgb(63, 71, 171)); // RGB Value 104: R=63, G=71, B=171
            palette.AddColor(Color.FromArgb(79, 91, 191)); // RGB Value 105: R=79, G=91, B=191
            palette.AddColor(Color.FromArgb(95, 111, 211)); // RGB Value 106: R=95, G=111, B=211
            palette.AddColor(Color.FromArgb(115, 135, 231)); // RGB Value 107: R=115, G=135, B=231
            palette.AddColor(Color.FromArgb(139, 159, 255)); // RGB Value 108: R=139, G=159, B=255
            palette.AddColor(Color.FromArgb(27, 135, 135)); // RGB Value 109: R=27, G=135, B=135
            palette.AddColor(Color.FromArgb(43, 151, 151)); // RGB Value 110: R=43, G=151, B=151
            palette.AddColor(Color.FromArgb(59, 167, 167)); // RGB Value 111: R=59, G=167, B=167
            palette.AddColor(Color.FromArgb(79, 183, 183)); // RGB Value 112: R=79, G=183, B=183
            palette.AddColor(Color.FromArgb(107, 203, 203)); // RGB Value 113: R=107, G=203, B=203
            palette.AddColor(Color.FromArgb(131, 219, 219)); // RGB Value 114: R=131, G=219, B=219
            palette.AddColor(Color.FromArgb(163, 235, 235)); // RGB Value 115: R=163, G=235, B=235
            palette.AddColor(Color.FromArgb(195, 255, 255)); // RGB Value 116: R=195, G=255, B=255
            palette.AddColor(Color.FromArgb(67, 0, 67)); // RGB Value 117: R=67, G=0, B=67
            palette.AddColor(Color.FromArgb(91, 11, 91)); // RGB Value 118: R=91, G=11, B=91
            palette.AddColor(Color.FromArgb(119, 27, 119)); // RGB Value 119: R=119, G=27, B=119
            palette.AddColor(Color.FromArgb(147, 51, 147)); // RGB Value 120: R=147, G=51, B=147
            palette.AddColor(Color.FromArgb(171, 83, 171)); // RGB Value 121: R=171, G=83, B=171
            palette.AddColor(Color.FromArgb(199, 123, 199)); // RGB Value 122: R=199, G=123, B=199
            palette.AddColor(Color.FromArgb(227, 167, 227)); // RGB Value 123: R=227, G=167, B=227
            palette.AddColor(Color.FromArgb(255, 219, 255)); // RGB Value 124: R=255, G=219, B=255
            palette.AddColor(Color.FromArgb(55, 127, 59)); // RGB Value 125: R=55, G=127, B=59
            palette.AddColor(Color.FromArgb(71, 143, 75)); // RGB Value 126: R=71, G=143, B=75
            palette.AddColor(Color.FromArgb(91, 163, 91)); // RGB Value 127: R=91, G=163, B=91
            palette.AddColor(Color.FromArgb(111, 179, 111)); // RGB Value 128: R=111, G=179, B=111
            palette.AddColor(Color.FromArgb(131, 199, 135)); // RGB Value 129: R=131, G=199, B=135
            palette.AddColor(Color.FromArgb(155, 215, 159)); // RGB Value 130: R=155, G=215, B=159
            palette.AddColor(Color.FromArgb(183, 235, 183)); // RGB Value 131: R=183, G=235, B=183
            palette.AddColor(Color.FromArgb(215, 255, 215)); // RGB Value 132: R=215, G=255, B=215
            palette.AddColor(Color.FromArgb(31, 39, 0)); // RGB Value 133: R=31, G=39, B=0
            palette.AddColor(Color.FromArgb(43, 51, 0)); // RGB Value 134: R=43, G=51, B=0
            palette.AddColor(Color.FromArgb(55, 67, 7)); // RGB Value 135: R=55, G=67, B=7
            palette.AddColor(Color.FromArgb(67, 79, 7)); // RGB Value 136: R=67, G=79, B=7
            palette.AddColor(Color.FromArgb(83, 95, 15)); // RGB Value 137: R=83, G=95, B=15
            palette.AddColor(Color.FromArgb(99, 111, 23)); // RGB Value 138: R=99, G=111, B=23
            palette.AddColor(Color.FromArgb(111, 123, 31)); // RGB Value 139: R=111, G=123, B=31
            palette.AddColor(Color.FromArgb(127, 139, 43)); // RGB Value 140: R=127, G=139, B=43
            palette.AddColor(Color.FromArgb(143, 151, 55)); // RGB Value 141: R=143, G=151, B=55
            palette.AddColor(Color.FromArgb(159, 167, 67)); // RGB Value 142: R=159, G=167, B=67
            palette.AddColor(Color.FromArgb(175, 183, 79)); // RGB Value 143: R=175, G=183, B=79
            palette.AddColor(Color.FromArgb(191, 195, 95)); // RGB Value 144: R=191, G=195, B=95
            palette.AddColor(Color.FromArgb(207, 211, 111)); // RGB Value 145: R=207, G=211, B=111
            palette.AddColor(Color.FromArgb(223, 223, 131)); // RGB Value 146: R=223, G=223, B=131
            palette.AddColor(Color.FromArgb(239, 239, 151)); // RGB Value 147: R=239, G=239, B=151
            palette.AddColor(Color.FromArgb(255, 255, 171)); // RGB Value 148: R=255, G=255, B=171
            palette.AddColor(Color.FromArgb(0, 35, 19)); // RGB Value 149: R=0, G=35, B=19
            palette.AddColor(Color.FromArgb(0, 47, 27)); // RGB Value 150: R=0, G=47, B=27
            palette.AddColor(Color.FromArgb(7, 63, 39)); // RGB Value 151: R=7, G=63, B=39
            palette.AddColor(Color.FromArgb(11, 79, 55)); // RGB Value 152: R=11, G=79, B=55
            palette.AddColor(Color.FromArgb(19, 91, 67)); // RGB Value 153: R=19, G=91, B=67
            palette.AddColor(Color.FromArgb(27, 107, 83)); // RGB Value 154: R=27, G=107, B=83
            palette.AddColor(Color.FromArgb(35, 123, 99)); // RGB Value 155: R=35, G=123, B=99
            palette.AddColor(Color.FromArgb(43, 139, 115)); // RGB Value 156: R=43, G=139, B=115
            palette.AddColor(Color.FromArgb(59, 151, 131)); // RGB Value 157: R=59, G=151, B=131
            palette.AddColor(Color.FromArgb(79, 167, 151)); // RGB Value 158: R=79, G=167, B=151
            palette.AddColor(Color.FromArgb(99, 179, 167)); // RGB Value 159: R=99, G=179, B=167
            palette.AddColor(Color.FromArgb(115, 195, 187)); // RGB Value 160: R=115, G=195, B=187
            palette.AddColor(Color.FromArgb(139, 211, 203)); // RGB Value 161: R=139, G=211, B=203
            palette.AddColor(Color.FromArgb(159, 223, 219)); // RGB Value 162: R=159, G=223, B=219
            palette.AddColor(Color.FromArgb(183, 239, 235)); // RGB Value 163: R=183, G=239, B=235
            palette.AddColor(Color.FromArgb(211, 255, 255)); // RGB Value 164: R=211, G=255, B=255
            palette.AddColor(Color.FromArgb(0, 19, 0)); // RGB Value 165: R=0, G=19, B=0
            palette.AddColor(Color.FromArgb(0, 35, 0)); // RGB Value 166: R=0, G=35, B=0
            palette.AddColor(Color.FromArgb(0, 47, 7)); // RGB Value 167: R=0, G=47, B=7
            palette.AddColor(Color.FromArgb(7, 63, 11)); // RGB Value 168: R=7, G=63, B=11
            palette.AddColor(Color.FromArgb(15, 79, 19)); // RGB Value 169: R=15, G=79, B=19
            palette.AddColor(Color.FromArgb(23, 95, 27)); // RGB Value 170: R=23, G=95, B=27
            palette.AddColor(Color.FromArgb(35, 111, 39)); // RGB Value 171: R=35, G=111, B=39
            palette.AddColor(Color.FromArgb(43, 127, 51)); // RGB Value 172: R=43, G=127, B=51
            palette.AddColor(Color.FromArgb(59, 143, 67)); // RGB Value 173: R=59, G=143, B=67
            palette.AddColor(Color.FromArgb(75, 159, 79)); // RGB Value 174: R=75, G=159, B=79
            palette.AddColor(Color.FromArgb(91, 175, 99)); // RGB Value 175: R=91, G=175, B=99
            palette.AddColor(Color.FromArgb(107, 191, 115)); // RGB Value 176: R=107, G=191, B=115
            palette.AddColor(Color.FromArgb(127, 207, 139)); // RGB Value 177: R=127, G=207, B=139
            palette.AddColor(Color.FromArgb(151, 223, 159)); // RGB Value 178: R=151, G=223, B=159
            palette.AddColor(Color.FromArgb(171, 239, 183)); // RGB Value 179: R=171, G=239, B=183
            palette.AddColor(Color.FromArgb(199, 255, 207)); // RGB Value 180: R=199, G=255, B=207
            palette.AddColor(Color.FromArgb(131, 71, 43)); // RGB Value 181: R=131, G=71, B=43
            palette.AddColor(Color.FromArgb(147, 83, 47)); // RGB Value 182: R=147, G=83, B=47
            palette.AddColor(Color.FromArgb(163, 95, 55)); // RGB Value 183: R=163, G=95, B=55
            palette.AddColor(Color.FromArgb(183, 107, 63)); // RGB Value 184: R=183, G=107, B=63
            palette.AddColor(Color.FromArgb(199, 123, 71)); // RGB Value 185: R=199, G=123, B=71
            palette.AddColor(Color.FromArgb(219, 139, 79)); // RGB Value 186: R=219, G=139, B=79
            palette.AddColor(Color.FromArgb(235, 155, 87)); // RGB Value 187: R=235, G=155, B=87
            palette.AddColor(Color.FromArgb(255, 171, 95)); // RGB Value 188: R=255, G=171, B=95
            palette.AddColor(Color.FromArgb(175, 151, 55)); // RGB Value 189: R=175, G=151, B=55
            palette.AddColor(Color.FromArgb(191, 171, 55)); // RGB Value 190: R=191, G=171, B=55
            palette.AddColor(Color.FromArgb(211, 195, 55)); // RGB Value 191: R=211, G=195, B=55
            palette.AddColor(Color.FromArgb(231, 219, 51)); // RGB Value 192: R=231, G=219, B=51
            palette.AddColor(Color.FromArgb(251, 247, 51)); // RGB Value 193: R=251, G=247, B=51
            palette.AddColor(Color.FromArgb(27, 7, 0)); // RGB Value 194: R=27, G=7, B=0
            palette.AddColor(Color.FromArgb(55, 11, 0)); // RGB Value 195: R=55, G=11, B=0
            palette.AddColor(Color.FromArgb(83, 15, 0)); // RGB Value 196: R=83, G=15, B=0
            palette.AddColor(Color.FromArgb(111, 15, 0)); // RGB Value 197: R=111, G=15, B=0
            palette.AddColor(Color.FromArgb(139, 15, 0)); // RGB Value 198: R=139, G=15, B=0
            palette.AddColor(Color.FromArgb(167, 11, 0)); // RGB Value 199: R=167, G=11, B=0
            palette.AddColor(Color.FromArgb(195, 7, 0)); // RGB Value 200: R=195, G=7, B=0
            palette.AddColor(Color.FromArgb(223, 0, 0)); // RGB Value 201: R=223, G=0, B=0
            palette.AddColor(Color.FromArgb(227, 23, 23)); // RGB Value 202: R=227, G=23, B=23
            palette.AddColor(Color.FromArgb(231, 51, 51)); // RGB Value 203: R=231, G=51, B=51
            palette.AddColor(Color.FromArgb(235, 79, 79)); // RGB Value 204: R=235, G=79, B=79
            palette.AddColor(Color.FromArgb(239, 111, 111)); // RGB Value 205: R=239, G=111, B=111
            palette.AddColor(Color.FromArgb(243, 139, 139)); // RGB Value 206: R=243, G=139, B=139
            palette.AddColor(Color.FromArgb(247, 171, 171)); // RGB Value 207: R=247, G=171, B=171
            palette.AddColor(Color.FromArgb(251, 203, 203)); // RGB Value 208: R=251, G=203, B=203
            palette.AddColor(Color.FromArgb(255, 239, 239)); // RGB Value 209: R=255, G=239, B=239
            palette.AddColor(Color.FromArgb(7, 7, 7)); // RGB Value 210: R=7, G=7, B=7
            palette.AddColor(Color.FromArgb(15, 15, 15)); // RGB Value 211: R=15, G=15, B=15
            palette.AddColor(Color.FromArgb(27, 27, 27)); // RGB Value 212: R=27, G=27, B=27
            palette.AddColor(Color.FromArgb(39, 39, 39)); // RGB Value 213: R=39, G=39, B=39
            palette.AddColor(Color.FromArgb(51, 51, 47)); // RGB Value 214: R=51, G=51, B=47
            palette.AddColor(Color.FromArgb(59, 59, 59)); // RGB Value 215: R=59, G=59, B=59
            palette.AddColor(Color.FromArgb(71, 71, 71)); // RGB Value 216: R=71, G=71, B=71
            palette.AddColor(Color.FromArgb(83, 83, 79)); // RGB Value 217: R=83, G=83, B=79
            palette.AddColor(Color.FromArgb(91, 91, 91)); // RGB Value 218: R=91, G=91, B=91
            palette.AddColor(Color.FromArgb(103, 103, 99)); // RGB Value 219: R=103, G=103, B=99
            palette.AddColor(Color.FromArgb(115, 115, 111)); // RGB Value 220: R=115, G=115, B=111
            palette.AddColor(Color.FromArgb(127, 127, 123)); // RGB Value 221: R=127, G=127, B=123
            palette.AddColor(Color.FromArgb(127, 127, 123)); // RGB Value 222: R=127, G=127, B=123
            palette.AddColor(Color.FromArgb(135, 135, 131)); // RGB Value 223: R=135, G=135, B=131
            palette.AddColor(Color.FromArgb(147, 147, 143)); // RGB Value 224: R=147, G=147, B=143
            palette.AddColor(Color.FromArgb(159, 159, 151)); // RGB Value 225: R=159, G=159, B=151
            palette.AddColor(Color.FromArgb(171, 171, 163)); // RGB Value 226: R=171, G=171, B=163
            palette.AddColor(Color.FromArgb(179, 179, 171)); // RGB Value 227: R=179, G=179, B=171
            palette.AddColor(Color.FromArgb(191, 191, 183)); // RGB Value 228: R=191, G=191, B=183
            palette.AddColor(Color.FromArgb(203, 203, 191)); // RGB Value 229: R=203, G=203, B=191
            palette.AddColor(Color.FromArgb(211, 211, 199)); // RGB Value 230: R=211, G=211, B=199
            palette.AddColor(Color.FromArgb(223, 223, 211)); // RGB Value 231: R=223, G=223, B=211
            palette.AddColor(Color.FromArgb(235, 235, 219)); // RGB Value 232: R=235, G=235, B=219
            palette.AddColor(Color.FromArgb(247, 247, 231)); // RGB Value 233: R=247, G=247, B=231
            palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 234: R=0, G=255, B=0
            palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 235: R=0, G=255, B=0
            palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 236: R=0, G=255, B=0
            palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 237: R=0, G=255, B=0
            palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 238: R=0, G=255, B=0
            palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 239: R=0, G=255, B=0
            palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 240: R=0, G=255, B=0
            palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 241: R=0, G=255, B=0
            palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 242: R=0, G=255, B=0
            palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 243: R=0, G=255, B=0
            palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 244: R=0, G=255, B=0
            palette.AddColor(Color.FromArgb(0, 255, 0)); // RGB Value 245: R=0, G=255, B=0
            palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 246: R=255, G=0, B=255
            palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 247: R=255, G=0, B=255
            palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 248: R=255, G=0, B=255
            palette.AddColor(Color.FromArgb(255, 0, 247)); // RGB Value 249: R=255, G=0, B=247
            palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 250: R=255, G=0, B=255
            palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 251: R=255, G=0, B=255
            palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 252: R=255, G=0, B=255
            palette.AddColor(Color.FromArgb(255, 0, 255)); // RGB Value 253: R=255, G=0, B=255
            palette.AddColor(Color.FromArgb(127, 127, 127)); // RGB Value 254: R=127, G=127, B=127
            palette.AddColor(Color.FromArgb(255, 255, 255)); // RGB Value 255: R=255, G=255, B=255
        }
        #endregion

        #region Create Gray Palette RGB
        private void btCreatePaletteFull_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string filename = Path.Combine(folderBrowserDialog.SelectedPath, "Palette.mul");
                    palette.CreateDefaultPalette();
                    palette.Save(filename);
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
                    palette.Load(openFileDialog.FileName);
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
                palette.DrawPalette(g, pictureBoxPalette.Width, pictureBoxPalette.Height);
            }
            pictureBoxPalette.Image = bitmap;
        }
        #endregion

        #region DisplayRGBValues
        private void DisplayRGBValues()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < palette.RGBValues.Count; i++)
            {
                sb.AppendLine($"RGB Value {i}: R={palette.RGBValues[i].R}, G={palette.RGBValues[i].G}, B={palette.RGBValues[i].B}");
            }
            //rgbValuesLabel.Text = sb.ToString();
            textBoxRgbValues.Text = sb.ToString();

            // Kopieren Sie die RGB-Werte in die Zwischenablage
            Clipboard.SetText(sb.ToString());
        }
        #endregion

        #region btnLoadAnimationMulData
        private void btnLoadAnimationMulData_Click(object sender, EventArgs e)
        {
            // Öffnen Sie den Datei-Dialog, um den Benutzer das Verzeichnis auswählen zu lassen
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string animMulPath = Path.Combine(fbd.SelectedPath, "Anim.mul");
                    string animIdxPath = Path.Combine(fbd.SelectedPath, "Anim.idx");

                    // Laden Sie die Animation mit den ausgewählten Dateipfaden
                    AnimationGroup animation = LoadAnimation(animMulPath, animIdxPath);

                    // Zeigen Sie die Animation in der TextBox an
                    this.txtData.Text = animation.ToString();
                }
            }
        }
        #endregion

        #region LoadAnimation
        private AnimationGroup LoadAnimation(string animMulPath, string animIdxPath)
        {
            AnimationGroup animation = new AnimationGroup();

            // Lesen Sie die Daten aus der Anim.idx Datei
            using (var idxReader = new BinaryReader(File.OpenRead(animIdxPath)))
            {
                animation.FrameCount = (uint)(idxReader.BaseStream.Length / 12);
                animation.FrameOffset = new uint[animation.FrameCount];

                for (int i = 0; i < animation.FrameCount; i++)
                {
                    if (idxReader.BaseStream.Position < idxReader.BaseStream.Length - 12) // Überprüfen Sie, ob noch genügend Daten zum Lesen vorhanden sind
                    {
                        idxReader.ReadUInt32(); // Überspringen Sie das Lookup-Feld
                        uint size = idxReader.ReadUInt32();
                        idxReader.ReadUInt32(); // Überspringen Sie das Unknown-Feld

                        if (size > 0)
                        {
                            animation.FrameOffset[i] = size;
                        }
                    }
                }
            }            

            // Lesen Sie die Daten aus der Anim.mul Datei
            using (var mulReader = new BinaryReader(File.OpenRead(animMulPath)))
            {
                animation.Frames = new Frame[animation.FrameCount];

                for (int i = 0; i < animation.FrameCount; i++)
                {
                    if (animation.FrameOffset[i] > 0)
                    {
                        mulReader.BaseStream.Seek(animation.FrameOffset[i], SeekOrigin.Begin);

                        if (mulReader.BaseStream.Position < mulReader.BaseStream.Length - 8) // Überprüfen Sie, ob noch genügend Daten zum Lesen vorhanden sind
                        {
                            Frame frame = new Frame();
                            frame.ImageCenterX = mulReader.ReadUInt16();
                            frame.ImageCenterY = mulReader.ReadUInt16();
                            frame.Width = mulReader.ReadUInt16();
                            frame.Height = mulReader.ReadUInt16();

                            // Hier sollten Sie den Code hinzufügen, um die Pixel-Daten zu lesen

                            animation.Frames[i] = frame;
                        }
                    }
                }
            }

            return animation;
        }
        #endregion

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

        #region ReadArtIdx

        private int _indexCount = 0;
        private void BtnReadArtIdx_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MUL Files|*.mul";
            openFileDialog.Title = "Open a MUL File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _indexCount = 0; // Setzen Sie den Zähler zurück
                ReadArtIdx(openFileDialog.FileName);
                lblIndexCount.Text = $"Total indices read: {_indexCount}"; // Aktualisieren Sie das Label
                infoARTIDXMULID.AppendText($"Finished reading {_indexCount} entries from {openFileDialog.FileName}\n");
            }
        }

        /*private void ReadArtIdx(string filename)
        {
            using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                // Calculate the number of entries
                int entries = (int)stream.Length / 12;

                // Create a buffer for the entries
                byte[] buffer = new byte[12];

                for (int i = 0; i < entries; i++)
                {
                    // Read an entry into the buffer
                    stream.Read(buffer, 0, buffer.Length);

                    // Convert the buffer to DWORDs
                    int lookup = BitConverter.ToInt32(buffer, 0);
                    int size = BitConverter.ToInt32(buffer, 4);
                    int unknown = BitConverter.ToInt32(buffer, 8);

                    _indexCount++; // Erhöhen Sie den Zähler
                }
            }
        }*/

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

        #region btnSingleEmptyAnimMul_Click
        private void btnSingleEmptyAnimMul_Click(object sender, EventArgs e)
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
        const int cHighDetail = 110;
        const int cLowDetail = 65;
        const int cHuman = 175;

        const int cHighDetailOLd = 1; // Old Version
        const int cLowDetailOld = 2; // Old Version
        const int cHumanOld = 3; // Old Version

        private int newIdCount = 0;
        #endregion

        #region btnBrowseClick
        private void btnBrowseClick(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                tbfilename.Text = openFileDialog.FileName;
            }
        }
        #endregion

        #region btnSetOutputDirectoryClick
        private void btnSetOutputDirectoryClick(object sender, EventArgs e)
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
        private void btnProcessClick(object sender, EventArgs e)
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
                    copyCount = cHighDetail;
                }
                else if (chkLowDetail.Checked)
                {
                    copyCount = cLowDetail;
                }
                else if (chkHuman.Checked)
                {
                    copyCount = cHuman;
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
                    newIdCount++;
                }
                // Update the label with the number of IDs created
                lblNewIdCount.Text = $"Number of IDs created: {newIdCount}";
            }
        }
        #endregion

        #region DetermineCreatureProperties
        private void DetermineCreatureProperties(int creatureID, out int indexOffset, out int readLength, out string creatureType)
        {
            if (creatureID <= 199)
            {
                indexOffset = creatureID * cHighDetail;
                readLength = cHighDetail * 12;
                creatureType = "High Detail Critter";
            }
            else if (creatureID > 199 && creatureID <= 399)
            {
                indexOffset = (creatureID - 200) * cLowDetail + 22000;
                readLength = cLowDetail * 12;
                creatureType = "Low Detail Critter";
            }
            else
            {
                indexOffset = (creatureID - 400) * cHuman + 35000;
                readLength = cHuman * 12;
                creatureType = "Human or an Accessoire";
            }
        }
        #endregion

        #region btnProcessClickOldVersion
        private void btnProcessClickOldVersion(object sender, EventArgs e)
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
                        cAnimType = cHighDetailOLd;
                        tbProcessAminidx.AppendText("Creature is a High Detail Critter\n");
                    }
                    else if (creatureID > 199 && creatureID <= 399)
                    {
                        indexOffset = (creatureID - 200) * 65 + 22000;
                        readLength = 65 * 12;
                        cAnimType = cLowDetailOld;
                        tbProcessAminidx.AppendText("Creature is a Low Detail Critter\n");
                    }
                    else
                    {
                        indexOffset = (creatureID - 400) * 175 + 35000;
                        readLength = 175 * 12;
                        cAnimType = cHumanOld;
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
                            case cHighDetailOLd:
                                // Perform some action for high detail creatures
                                break;
                            case cLowDetailOld:
                                // Perform some action for low detail creatures
                                break;
                            case cHumanOld:
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
    }
}
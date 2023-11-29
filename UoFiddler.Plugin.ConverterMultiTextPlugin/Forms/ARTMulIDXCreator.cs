
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

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

        private BinaryReader reader;
        private int itemsLoaded = 0;



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


        private void YourForm_KeyDown(object sender, KeyEventArgs e)
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

        private void buttonReadTileData_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                reader = new BinaryReader(File.Open(openFileDialog.FileName, FileMode.Open));
                LoadItems();
            }
        }

        private int landItemsLoaded = 0;
        private int staticItemsLoaded = 0;

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

        private void buttonReadLandTileData_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                reader = new BinaryReader(File.Open(openFileDialog.FileName, FileMode.Open));
                LoadLandTiles();
            }
        }

        private void buttonReadStaticTileData_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                reader = new BinaryReader(File.Open(openFileDialog.FileName, FileMode.Open));
                LoadStaticTiles();
            }
        }

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

    }
}
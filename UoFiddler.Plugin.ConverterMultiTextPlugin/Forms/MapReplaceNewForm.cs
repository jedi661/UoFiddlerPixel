﻿using System;
using System.IO;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class MapReplaceNewForm : Form
    {
        private Map _workingMap;
        private Map _sourceMap;
        private string _mulDirectoryPath;

        public MapReplaceNewForm()
        {
            InitializeComponent();            
            Icon = Options.GetFiddlerIcon();
            comboBoxMapID.BeginUpdate();
            comboBoxMapID.EndUpdate();
            //comboBoxMapID.SelectedIndex = 0;

            checkBoxMap0.Checked = true;

            this.Controls.Add(this.checkBoxMap0);
            this.Controls.Add(this.checkBoxMap1);
            this.Controls.Add(this.checkBoxMap2);
            this.Controls.Add(this.checkBoxMap3);
            this.Controls.Add(this.checkBoxMap4);
            this.Controls.Add(this.checkBoxMap5);
            this.Controls.Add(this.checkBoxMap6);
            this.Controls.Add(this.checkBoxMap7);
        }

        public void SetWorkingMap(Map map)
        {
            _workingMap = map; // Führen Sie hier alle weiteren Initialisierungen durch, die von der Map abhängen
        }

        #region [ OnClickBrowse ]
        private void OnClickBrowse(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                Description = "Select directory containing the map files",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = dialog.SelectedPath;
            }

            dialog.Dispose();
        }
        #endregion

        #region [ heckBoxMap_CheckedChanged ]
        private void CheckBoxMap_CheckedChanged(object sender, EventArgs e)
        {
            // Setzen der Zielkarte basierend auf der aktivierten CheckBox
            CheckBox checkBox = sender as CheckBox;

            if (checkBox == null)
                return;

            if (checkBox.Checked)
            {
                // Deaktivieren der anderen CheckBoxen
                foreach (var control in this.Controls)
                {
                    if (control is CheckBox && control != checkBox)
                    {
                        ((CheckBox)control).Checked = false;
                    }
                }

                // Setzen der _workingMap basierend auf der ausgewählten CheckBox
                int mapIndex = int.Parse(checkBox.Text.Replace("Map", "").Replace(" als Ziel", ""));
                _workingMap = new Map(mapIndex, GetMapWidth(mapIndex), GetMapHeight(mapIndex));
            }
            else
            {
                // Setzen der _workingMap auf die Standard-Map (Map 0), wenn keine Checkbox ausgewählt ist
                _workingMap = new Map(0, GetMapWidth(0), GetMapHeight(0));
            }
        }
        #endregion

        private int GetMapWidth(int mapIndex)
        {
            // Implementiere eine Methode, die die Breite der Karte basierend auf dem Index zurückgibt
            // Beispiel:
            switch (mapIndex)
            {
                case 0: return 7168; // Felucca
                case 1: return 7168; // Trammel
                case 2: return 2304; // Ilshenar
                case 3: return 2560; // Malas
                case 4: return 1448; // Tokuno
                case 5: return 1280; // TerMur
                case 6: return 6144; // Forell
                case 7: return 6144; // Dragon
                case 8: return 6144; // intermediate world
                default: return 0;
            }
        }

        private int GetMapHeight(int mapIndex)
        {
            // Implementiere eine Methode, die die Höhe der Karte basierend auf dem Index zurückgibt
            // Beispiel:
            switch (mapIndex)
            {
                case 0: return 4096; // Felucca
                case 1: return 4096; // Trammel
                case 2: return 1600; // Ilshenar
                case 3: return 2048; // Malas
                case 4: return 1448; // Tokuno
                case 5: return 4096; // TerMur
                case 6: return 4096; // Forell
                case 7: return 4096; // Dragon
                case 8: return 4096; // intermediate world
                default: return 0;
            }
        }

        #region [ OnClickCopy ]
        private void OnClickCopy(object sender, EventArgs e)
        {

            // Überprüfen Sie, ob eine Zielkarte ausgewählt wurde
            if (_workingMap == null)
            {
                MessageBox.Show("Keine Zielkarte ausgewählt!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Überprüfen Sie, ob eine Quellkarte ausgewählt wurde
            if (this._sourceMap == null)
            {
                MessageBox.Show("Keine Quellkarte ausgewählt!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Verwenden Sie _mulDirectoryPath anstelle von textBoxUltimaDir.Text
            string ultimaDirectory = _mulDirectoryPath;

            if (!Directory.Exists(ultimaDirectory))
            {
                MessageBox.Show("Directory does not exist!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Überprüfen Sie, ob das ausgewählte Element eine unterstützte Karte ist
            if (!(comboBoxMapID.SelectedItem is SupportedMaps _sourceMap))
            {
                MessageBox.Show("Invalid Map ID!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            // Holen Sie sich die ID der ausgewählten Karte
            int selectedMapId = int.Parse(_sourceMap.GetId());

            // Überprüfen Sie, ob die ausgewählte Map-ID zwischen 1 und 5 liegt
            if (selectedMapId < 1 || selectedMapId > 8) // from to
            {
                MessageBox.Show("Invalid Map ID! Please select a map between 1 and 5.", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            
            // Konstruktion der Pfadnamen für die benötigten .mul Dateien basierend auf dem ausgewählten Verzeichnis
            string mapFilePath = Path.Combine(ultimaDirectory, $"map{_workingMap.Id}.mul");
            string staticsFilePath = Path.Combine(ultimaDirectory, $"statics{_sourceMap.Id}.mul");

            // Überprüfen, ob die Dateien existieren
            if (!File.Exists(mapFilePath) || !File.Exists(staticsFilePath))
            {
                MessageBox.Show("Map files not found in the directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string path = textBox1.Text;
            if (!Directory.Exists(path))
            {
                MessageBox.Show("Path not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            int x1 = (int)numericUpDownX1.Value;
            int x2 = (int)numericUpDownX2.Value;
            int y1 = (int)numericUpDownY1.Value;
            int y2 = (int)numericUpDownY2.Value;
            int tox = (int)numericUpDownToX1.Value;
            int toy = (int)numericUpDownToY1.Value;

            if (x1 < 0 || x1 > _sourceMap.Width)
            {
                MessageBox.Show("Invalid X1 coordinate!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (x1 < 0 || x1 > _sourceMap.Width || x2 < 0 || x2 > _sourceMap.Width || y1 < 0 || y1 > _sourceMap.Height || y2 < 0 || y2 > _sourceMap.Height)
            {
                MessageBox.Show("Invalid coordinates!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (x2 < 0 || x2 > _sourceMap.Width)
            {
                MessageBox.Show("Invalid X2 coordinate!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (y1 < 0 || y1 > _sourceMap.Height)
            {
                MessageBox.Show("Invalid Y1 coordinate!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (y2 < 0 || y2 > _sourceMap.Height)
            {
                MessageBox.Show("Invalid Y2 coordinate!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (x1 > x2 || y1 > y2)
            {
                MessageBox.Show("X1 and Y1 cannot be bigger than X2 and Y2!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }


            if (tox < 0 || tox > _workingMap.Width || tox + (x2 - x1) > _workingMap.Width)
            {
                MessageBox.Show("Invalid toX coordinate!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (toy < 0 || toy > _workingMap.Height || toy + (y2 - y1) > _workingMap.Height)
            {
                MessageBox.Show("Invalid toX coordinate!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            x1 >>= 3;
            x2 >>= 3;
            y1 >>= 3;
            y2 >>= 3;

            tox >>= 3;
            toy >>= 3;

            int tox2 = x2 - x1 + tox;
            int toy2 = y2 - y1 + toy;

            int blockY = _workingMap.Height >> 3;
            int blockX = _workingMap.Width >> 3;
            int blockYReplace = _sourceMap.Height >> 3;
            // int blockxreplace = _sourceMap.Width >> 3; // TODO: unused variable?

            progressBar1.Step = 1;
            progressBar1.Value = 0;
            progressBar1.Maximum = 0;

            if (checkBoxMap.Checked)
            {
                progressBar1.Maximum += blockY * blockX;
            }

            if (checkBoxStatics.Checked)
            {
                progressBar1.Maximum += blockY * blockX;
            }

            if (checkBoxMap.Checked)
            {
                string copyMap = Path.Combine(path, $"map{_sourceMap.Id}.mul");
                if (!File.Exists(copyMap))
                {
                    MessageBox.Show("Map file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                    return;
                }

                FileStream mMapCopy = new FileStream(copyMap, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader mMapReaderCopy = new BinaryReader(mMapCopy);
                string mapPath = Files.GetFilePath($"map{_workingMap.FileIndex}.mul");

                BinaryReader mMapReader;

                if (mapPath != null)
                {
                    FileStream mMap = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    mMapReader = new BinaryReader(mMap);
                }
                else
                {
                    MessageBox.Show("Map file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                    return;
                }

                string mul = Path.Combine(Options.OutputPath, $"map{_workingMap.FileIndex}.mul");
                using (FileStream fsMul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (BinaryWriter binMul = new BinaryWriter(fsMul))
                    {
                        for (int x = 0; x < blockX; ++x)
                        {
                            for (int y = 0; y < blockY; ++y)
                            {
                                if (tox <= x && x <= tox2 && toy <= y && y <= toy2)
                                {
                                    mMapReaderCopy.BaseStream.Seek((((x - tox + x1) * blockYReplace) + (y - toy) + y1) * 196, SeekOrigin.Begin);
                                    int header = mMapReaderCopy.ReadInt32();
                                    binMul.Write(header);
                                }
                                else
                                {
                                    mMapReader.BaseStream.Seek(((x * blockY) + y) * 196, SeekOrigin.Begin);
                                    int header = mMapReader.ReadInt32();
                                    binMul.Write(header);
                                }
                                for (int i = 0; i < 64; ++i)
                                {
                                    ushort tileId;
                                    sbyte z;

                                    if (tox <= x && x <= tox2 && toy <= y && y <= toy2)
                                    {
                                        tileId = mMapReaderCopy.ReadUInt16();
                                        z = mMapReaderCopy.ReadSByte();
                                    }
                                    else
                                    {
                                        tileId = mMapReader.ReadUInt16();
                                        z = mMapReader.ReadSByte();
                                    }

                                    tileId = Art.GetLegalItemId(tileId);

                                    if (z < -128)
                                    {
                                        z = -128;
                                    }

                                    if (z > 127)
                                    {
                                        z = 127;
                                    }

                                    binMul.Write(tileId);
                                    binMul.Write(z);
                                }
                                progressBar1.PerformStep();
                            }
                        }
                    }
                }

                mMapReader.Close();
                mMapReaderCopy.Close();
            }

            if (checkBoxStatics.Checked)
            {
                string indexPath = Files.GetFilePath($"staidx{_workingMap.FileIndex}.mul");
                BinaryReader mIndexReader;

                if (indexPath != null)
                {
                    FileStream mIndex = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    mIndexReader = new BinaryReader(mIndex);
                }
                else
                {
                    MessageBox.Show("Static file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }

                string staticsPath = Files.GetFilePath($"statics{_workingMap.FileIndex}.mul");
                FileStream mStatics;
                BinaryReader mStaticsReader;

                if (staticsPath != null)
                {
                    mStatics = new FileStream(staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    mStaticsReader = new BinaryReader(mStatics);
                }
                else
                {
                    MessageBox.Show("Static file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }

                string copyIndexPath = Path.Combine(path, $"staidx{_sourceMap.Id}.mul");
                if (!File.Exists(copyIndexPath))
                {
                    MessageBox.Show("Static file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }

                FileStream mIndexCopy = new FileStream(copyIndexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader mIndexReaderCopy = new BinaryReader(mIndexCopy);

                string copyStaticsPath = Path.Combine(path, $"statics{_sourceMap.Id}.mul");
                if (!File.Exists(copyStaticsPath))
                {
                    MessageBox.Show("Static file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }

                FileStream mStaticsCopy = new FileStream(copyStaticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader mStaticsReaderCopy = new BinaryReader(mStaticsCopy);

                string idx = Path.Combine(Options.OutputPath, $"staidx{_workingMap.FileIndex}.mul");
                string mul = Path.Combine(Options.OutputPath, $"statics{_workingMap.FileIndex}.mul");
                using (FileStream fsIdx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
                                  fsMul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (BinaryWriter binidx = new BinaryWriter(fsIdx),
                                        binmul = new BinaryWriter(fsMul))
                    {
                        for (int x = 0; x < blockX; ++x)
                        {
                            for (int y = 0; y < blockY; ++y)
                            {
                                int lookup, length, extra;
                                if (tox <= x && x <= tox2 && toy <= y && y <= toy2)
                                {
                                    mIndexReaderCopy.BaseStream.Seek((((x - tox + x1) * blockYReplace) + (y - toy) + y1) * 12, SeekOrigin.Begin);
                                    lookup = mIndexReaderCopy.ReadInt32();
                                    length = mIndexReaderCopy.ReadInt32();
                                    extra = mIndexReaderCopy.ReadInt32();
                                }
                                else
                                {
                                    mIndexReader.BaseStream.Seek(((x * blockY) + y) * 12, SeekOrigin.Begin);
                                    lookup = mIndexReader.ReadInt32();
                                    length = mIndexReader.ReadInt32();
                                    extra = mIndexReader.ReadInt32();
                                }

                                if (lookup < 0 || length <= 0)
                                {
                                    binidx.Write(-1); // lookup
                                    binidx.Write(-1); // length
                                    binidx.Write(-1); // extra
                                }
                                else
                                {
                                    if (tox <= x && x <= tox2 && toy <= y && y <= toy2)
                                    {
                                        mStaticsCopy.Seek(lookup, SeekOrigin.Begin);
                                    }
                                    else
                                    {
                                        mStatics.Seek(lookup, SeekOrigin.Begin);
                                    }

                                    int fsMulLength = (int)fsMul.Position;
                                    int count = length / 7;
                                    if (RemoveDupl.Checked)
                                    {
                                        var tileList = new StaticTile[count];
                                        int j = 0;
                                        for (int i = 0; i < count; ++i)
                                        {
                                            StaticTile tile = new StaticTile();
                                            if (tox <= x && x <= tox2 && toy <= y && y <= toy2)
                                            {
                                                tile.Id = mStaticsReaderCopy.ReadUInt16();
                                                tile.X = mStaticsReaderCopy.ReadByte();
                                                tile.Y = mStaticsReaderCopy.ReadByte();
                                                tile.Z = mStaticsReaderCopy.ReadSByte();
                                                tile.Hue = mStaticsReaderCopy.ReadInt16();
                                            }
                                            else
                                            {
                                                tile.Id = mStaticsReader.ReadUInt16();
                                                tile.X = mStaticsReader.ReadByte();
                                                tile.Y = mStaticsReader.ReadByte();
                                                tile.Z = mStaticsReader.ReadSByte();
                                                tile.Hue = mStaticsReader.ReadInt16();
                                            }

                                            if (tile.Id > Art.GetMaxItemId())
                                            {
                                                continue;
                                            }

                                            if (tile.Hue < 0)
                                            {
                                                tile.Hue = 0;
                                            }

                                            bool first = true;
                                            for (int k = 0; k < j; ++k)
                                            {
                                                if (tileList[k].Id == tile.Id && tileList[k].X == tile.X && tileList[k].Y == tile.Y && tileList[k].Z == tile.Z && tileList[k].Hue == tile.Hue)
                                                {
                                                    first = false;
                                                    break;
                                                }
                                            }
                                            if (first)
                                            {
                                                tileList[j++] = tile;
                                            }
                                        }
                                        if (j > 0)
                                        {
                                            binidx.Write((int)fsMul.Position); //lookup
                                            for (int i = 0; i < j; ++i)
                                            {
                                                binmul.Write(tileList[i].Id);
                                                binmul.Write(tileList[i].X);
                                                binmul.Write(tileList[i].Y);
                                                binmul.Write(tileList[i].Z);
                                                binmul.Write(tileList[i].Hue);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        bool firstItem = true;
                                        for (int i = 0; i < count; ++i)
                                        {
                                            ushort graphic;
                                            short sHue;
                                            byte sx, sy;
                                            sbyte sz;
                                            if (tox <= x && x <= tox2 && toy <= y && y <= toy2)
                                            {
                                                graphic = mStaticsReaderCopy.ReadUInt16();
                                                sx = mStaticsReaderCopy.ReadByte();
                                                sy = mStaticsReaderCopy.ReadByte();
                                                sz = mStaticsReaderCopy.ReadSByte();
                                                sHue = mStaticsReaderCopy.ReadInt16();
                                            }
                                            else
                                            {
                                                graphic = mStaticsReader.ReadUInt16();
                                                sx = mStaticsReader.ReadByte();
                                                sy = mStaticsReader.ReadByte();
                                                sz = mStaticsReader.ReadSByte();
                                                sHue = mStaticsReader.ReadInt16();
                                            }

                                            if (graphic > Art.GetMaxItemId())
                                            {
                                                continue;
                                            }

                                            if (sHue < 0)
                                            {
                                                sHue = 0;
                                            }

                                            if (firstItem)
                                            {
                                                binidx.Write((int)fsMul.Position); // lookup
                                                firstItem = false;
                                            }
                                            binmul.Write(graphic);
                                            binmul.Write(sx);
                                            binmul.Write(sy);
                                            binmul.Write(sz);
                                            binmul.Write(sHue);
                                        }
                                    }

                                    fsMulLength = (int)fsMul.Position - fsMulLength;
                                    if (fsMulLength > 0)
                                    {
                                        binidx.Write(fsMulLength); // length
                                        binidx.Write(extra); // extra
                                    }
                                    else
                                    {
                                        binidx.Write(-1); // lookup
                                        binidx.Write(-1); // length
                                        binidx.Write(-1); // extra
                                    }
                                }

                                progressBar1.PerformStep();
                            }
                        }
                    }
                }

                mIndexReader.Close();
                mStaticsReader.Close();
                mIndexCopy.Close();
                mStaticsReaderCopy.Close();
            }

            MessageBox.Show($"Files saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region [ class SupportedMaps ]
        private class SupportedMaps
        {
            public int Id { get; }
            private string Name { get; }
            public int Height { get; }
            public int Width { get; }

            protected SupportedMaps(int id, string name, int width, int height)
            {
                Id = id;
                Name = name;
                Width = width;
                Height = height;
            }

            public string GetId()
            {
                return Id.ToString();
            }

            public override string ToString()
            {
                return $"{Id} - {Name} : {Width}x{Height}";
            }
        }
        #endregion

        #region [ class RFeluccaOld : SupportedMaps ]
        private class RFeluccaOld : SupportedMaps
        {
            public RFeluccaOld() : base(0, Options.MapNames[0] + "Old", 6144, 4096) { }
        }
        #endregion

        #region [ class RFelucca : SupportedMaps ]
        private class RFelucca : SupportedMaps
        {
            public RFelucca() : base(0, Options.MapNames[0], 7168, 4096) { }
        }
        #endregion

        #region [ class RTrammel : SupportedMaps ]
        private class RTrammel : SupportedMaps
        {
            public RTrammel() : base(1, Options.MapNames[1], 7168, 4096) { }
        }
        #endregion

        #region [ class RIlshenar : SupportedMaps ]
        private class RIlshenar : SupportedMaps
        {
            public RIlshenar() : base(2, Options.MapNames[2], 2304, 1600) { }
        }
        #endregion

        #region [ class RMalas : SupportedMaps ]
        private class RMalas : SupportedMaps
        {
            public RMalas() : base(3, Options.MapNames[3], 2560, 2048) { }
        }
        #endregion

        #region [ class RTokuno : SupportedMaps ]
        private class RTokuno : SupportedMaps
        {
            public RTokuno() : base(4, Options.MapNames[4], 1448, 1448) { }
        }
        #endregion

        #region [ class RTerMur : SupportedMaps ]
        private class RTerMur : SupportedMaps
        {
            public RTerMur() : base(5, Options.MapNames[5], 1280, 4096) { }
        }
        #endregion

        #region [ class RForell : SupportedMaps ]
        private class RForell : SupportedMaps
        {
            public RForell() : base(6, Options.MapNames[6], 6144, 4096) { }
        }
        #endregion

        #region [ class RDragon : SupportedMaps ]
        private class RDragon : SupportedMaps
        {
            public RDragon() : base(7, Options.MapNames[7], 6144, 4096) { }
        }
        #endregion

        #region [ class Rintermediateworld : SupportedMaps ]
        private class Rintermediateworld : SupportedMaps
        {
            public Rintermediateworld() : base(8, Options.MapNames[8], 6144, 4096) { }
        }
        #endregion

        #region [ btLoadUODir ]
        private void btLoadUODir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                Description = "Wählen Sie das Verzeichnis aus, das die .mul-Dateien enthält",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // Speichern Sie den Pfad in einer Eigenschaft oder Variable, auf die OnClickCopy zugreifen kann
                _mulDirectoryPath = dialog.SelectedPath;

                // Zeigen Sie den Pfad in der textBoxUltimaDir an
                textBoxUltimaDir.Text = _mulDirectoryPath;

                // Aktualisieren Sie die comboBoxMapID mit den unterstützten Karten
                UpdateMapComboBox();
            }

            dialog.Dispose();
        }
        #endregion

        #region [ UpdateMapComboBox ]
        private void UpdateMapComboBox()
        {
            // Leeren Sie die comboBoxMapID
            comboBoxMapID.Items.Clear();

            // Fügen Sie jede unterstützte Karte zur comboBoxMapID hinzu
            comboBoxMapID.Items.Add(new RFeluccaOld());
            comboBoxMapID.Items.Add(new RFelucca());
            comboBoxMapID.Items.Add(new RTrammel());
            comboBoxMapID.Items.Add(new RIlshenar());
            comboBoxMapID.Items.Add(new RMalas());
            comboBoxMapID.Items.Add(new RTokuno());
            comboBoxMapID.Items.Add(new RTerMur());
            comboBoxMapID.Items.Add(new RForell()); // Forell
            comboBoxMapID.Items.Add(new RDragon()); // Dragon
            comboBoxMapID.Items.Add(new Rintermediateworld()); // Intermediate World

            // Wählen Sie das erste Element in der comboBoxMapID aus, wenn es eines gibt
            if (comboBoxMapID.Items.Count > 0)
            {
                comboBoxMapID.SelectedIndex = 0;
            }

            // Fügen Sie einen Event-Handler für das SelectedIndexChanged-Event der ComboBox hinzu
            comboBoxMapID.SelectedIndexChanged += ComboBoxMapID_SelectedIndexChanged;
        }
        #endregion

        #region [ TestCord ]
        private void TestCord_Click(object sender, EventArgs e)
        {
            // Ersetzen Sie diese Werte durch die tatsächlichen Koordinaten, die Sie verwenden möchten
            int x1 = 0;
            int x2 = 100;
            int y1 = 0;
            int y2 = 100;

            // Setzen Sie die Value-Eigenschaften Ihrer NumericUpDown-Steuerelemente auf die Koordinaten
            numericUpDownX1.Value = x1;
            numericUpDownX2.Value = x2;
            numericUpDownY1.Value = y1;
            numericUpDownY2.Value = y2;
            numericUpDownToX1.Value = x1; // Ersetzen Sie x1 durch den tatsächlichen Wert, den Sie verwenden möchten
            numericUpDownToY1.Value = y1; // Ersetzen Sie y1 durch den tatsächlichen Wert, den Sie verwenden möchten
        }
        #endregion

        #region [ ComboBoxMapID_SelectedIndexChanged ]
        /*private void ComboBoxMapID_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Holen Sie sich die ausgewählte Karte
            SupportedMaps selectedMap = comboBoxMapID.SelectedItem as SupportedMaps;

            // Überprüfen Sie, ob eine Karte ausgewählt wurde
            if (selectedMap != null)
            {
                // Erstellen Sie eine neue Map-Instanz basierend auf der ausgewählten Karte
                Map map = new Map(selectedMap.Id, selectedMap.Width, selectedMap.Height);

                // Setzen Sie die _workingMap auf die neu erstellte Karte
                SetWorkingMap(map);

                // Setzen Sie die _sourceMap auf die neu erstellte Karte
                _sourceMap = new Map(selectedMap.Id, selectedMap.Width, selectedMap.Height);

                // Erstellen Sie den Dateinamen der Zieldatei basierend auf der ausgewählten Karte
                string targetFileName = $"map{selectedMap.GetId()}.mul";

                // Fügen Sie den Dateinamen zum Pfad des Zielverzeichnisses hinzu
                string targetFilePath = Path.Combine(_mulDirectoryPath, targetFileName);

                // Hier können Sie den Code hinzufügen, der die Datei kopiert oder erstellt
                // Zum Beispiel:
                string sourceFilePath = Path.Combine(textBox1.Text, $"map{selectedMap.GetId()}.mul");
                if (File.Exists(sourceFilePath))
                {
                    File.Copy(sourceFilePath, targetFilePath, true);
                }
                else
                {
                    MessageBox.Show($"Die Quelldatei {sourceFilePath} existiert nicht.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Aktualisieren Sie den Text des Labels, wenn das ausgewählte Element geändert wird
            string selectedMapId = ((SupportedMaps)comboBoxMapID.SelectedItem).GetId();
            string mapFilePath = Path.Combine(_mulDirectoryPath, $"map{selectedMapId}.mul");
            lbMulControl.Text = $"Ausgewähltes Verzeichnis: {_mulDirectoryPath}\nAusgewählte Datei: {mapFilePath}";
        }*/

        private void ComboBoxMapID_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Holen Sie sich die ausgewählte Karte
            SupportedMaps selectedMap = comboBoxMapID.SelectedItem as SupportedMaps;

            // Überprüfen Sie, ob eine Karte ausgewählt wurde
            if (selectedMap != null)
            {
                // Setzen Sie die _sourceMap basierend auf der ausgewählten Karte
                _sourceMap = new Map(selectedMap.Id, selectedMap.Width, selectedMap.Height);

                // Setzen Sie die _workingMap basierend auf der ausgewählten Karte
                SetWorkingMap(_sourceMap);

                // Erstellen Sie den Dateinamen der Zieldatei basierend auf der ausgewählten Karte
                string targetFileName = $"map{selectedMap.GetId()}.mul";

                // Fügen Sie den Dateinamen zum Pfad des Zielverzeichnisses hinzu
                string targetFilePath = Path.Combine(_mulDirectoryPath, targetFileName);

                // Hier können Sie den Code hinzufügen, der die Datei kopiert oder erstellt
                string sourceFilePath = Path.Combine(textBox1.Text, $"map{selectedMap.GetId()}.mul");
                if (File.Exists(sourceFilePath))
                {
                    File.Copy(sourceFilePath, targetFilePath, true);
                }
                else
                {
                    MessageBox.Show($"Die Quelldatei {sourceFilePath} existiert nicht.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Aktualisieren Sie den Text des Labels, wenn das ausgewählte Element geändert wird
            string selectedMapId = ((SupportedMaps)comboBoxMapID.SelectedItem).GetId();
            string mapFilePath = Path.Combine(_mulDirectoryPath, $"map{selectedMapId}.mul");
            lbMulControl.Text = $"Ausgewähltes Verzeichnis: {_mulDirectoryPath}\nAusgewählte Datei: {mapFilePath}";
        }

        #endregion

    }

    #region [ class Map ]
    public class Map
    {
        public int Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public int FileIndex { get; set; } // Hinzufügen der FileIndex-Eigenschaft

        public Map(int id, int width, int height)
        {
            Id = id;
            Width = width;
            Height = height;
        }
    }
    #endregion
}

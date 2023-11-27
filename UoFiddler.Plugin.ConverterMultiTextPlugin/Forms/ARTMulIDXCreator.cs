
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

        /*private void btCreateARTIDXMul_Click(object sender, EventArgs e)
        {
            var artIndexFile = new ArtIndexFile();
            var artDataFile = new ArtDataFile();

            int numEntries;
            if (!int.TryParse(textBox2.Text, out numEntries))
            {
                numEntries = 65500; // Standardwert, wenn textBox2 leer ist
            }

            for (int i = 0; i < numEntries; i++)
            {
                // Erstellen Sie einen leeren ArtIndexEntry und fügen Sie ihn zu artIndexFile hinzu
                var indexEntry = new ArtIndexEntry(0xFFFFFFFF, 0, 0);
                artIndexFile.AddEntry(indexEntry);

                // Erstellen Sie einen leeren ArtDataEntry und fügen Sie ihn zu artDataFile hinzu
                var dataEntry = new ArtDataEntry(new Bitmap(1, 1));
                artDataFile.AddEntry(dataEntry);
            }

            artIndexFile.SaveToFile(textBox1.Text + "\\artidx.MUL");
            artDataFile.SaveToFile(textBox1.Text + "\\art.MUL");
        }*/

        /*private void btCreateARTIDXMul_Click(object sender, EventArgs e)
        {
            var artIndexFile = new ArtIndexFile();
            var artDataFile = new ArtDataFile();

            long numEntries;
            if (!long.TryParse(textBox2.Text, out numEntries))
            {
                numEntries = 65500; // Standardwert, wenn textBox2 leer ist
            }

            for (long i = 0; i < numEntries; i++)
            {
                // Erstellen Sie einen leeren ArtIndexEntry und fügen Sie ihn zu artIndexFile hinzu
                var indexEntry = new ArtIndexEntry(0xFFFFFFFF, 0, 0);
                artIndexFile.AddEntry(indexEntry);

                // Erstellen Sie einen leeren ArtDataEntry und fügen Sie ihn zu artDataFile hinzu
                var dataEntry = new ArtDataEntry(new Bitmap(1, 1));
                artDataFile.AddEntry(dataEntry);
            }

            artIndexFile.SaveToFile(textBox1.Text + "\\artidx.MUL");
            artDataFile.SaveToFile(textBox1.Text + "\\art.MUL");
        }*/

        #region btCreateARTIDXMul
        private void btCreateARTIDXMul_Click(object sender, EventArgs e)
        {
            var artIndexFile = new ArtIndexFile();

            long numEntries;
            if (!long.TryParse(textBox2.Text, out numEntries))
            {
                numEntries = 65500; // Standardwert, wenn textBox2 leer ist
            }

            for (long i = 0; i < numEntries; i++)
            {
                // Erstellen Sie einen leeren ArtIndexEntry und fügen Sie ihn zu artIndexFile hinzu
                var indexEntry = new ArtIndexEntry(0xFFFFFFFF, 0, 0);
                artIndexFile.AddEntry(indexEntry);
            }

            artIndexFile.SaveToFile(textBox1.Text + "\\artidx.MUL");

            // Erstellen Sie eine leere art.mul Datei
            using (var fs = File.Create(textBox1.Text + "\\art.MUL")) { }
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
                    artIndexFile.LoadFromFile(filePath); // Angenommen, Sie haben eine LoadFromFile-Methode

                    int numEntries = artIndexFile.CountEntries();

                    lblEntryCount.Text = "Die Anzahl der Indexeinträge ist: " + numEntries;
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
                    artIndexFile.LoadFromFile(filePath); // Angenommen, Sie haben eine LoadFromFile-Methode

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < artIndexFile.CountEntries(); i++)
                    {
                        var entry = artIndexFile.GetEntry(i); // Angenommen, Sie haben eine GetEntry-Methode
                        sb.AppendLine($"Eintrag {i}: Lookup={entry.Lookup}, Size={entry.Size}, Unknown={entry.Unknown}");
                    }

                    textBoxInfo.Text = sb.ToString(); // Angenommen, textBoxInfo ist die TextBox, in der Sie die Informationen anzeigen möchten
                }
            }
        }
        #endregion

        #region btnReadArtIdx
        private void btnReadArtIdx_Click(object sender, EventArgs e)
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

        public ArtIndexEntry(uint lookup, uint size, uint unknown)
        {
            Lookup = lookup;
            Size = size;
            Unknown = unknown;
        }

        public void WriteToStream(BinaryWriter writer)
        {
            writer.Write(Lookup);
            writer.Write(Size);
            writer.Write(Unknown);
        }
    }
    #endregion

    #region class ArtIndexFile
    public class ArtIndexFile
    {
        private List<ArtIndexEntry> entries;

        public ArtIndexFile()
        {
            entries = new List<ArtIndexEntry>();
        }

        public ArtIndexEntry GetEntry(int index)
        {
            if (index >= 0 && index < entries.Count)
            {
                return entries[index];
            }
            else
            {
                return null; // oder werfen Sie eine Ausnahme
            }
        }

        public void AddEntry(ArtIndexEntry entry)
        {
            entries.Add(entry);
        }

        public void LoadFromFile(string filename)
        {
            entries.Clear(); // Löschen Sie alle vorhandenen Einträge

            using (var fs = new FileStream(filename, FileMode.Open))
            {
                using (var reader = new BinaryReader(fs))
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length) // Lesen Sie bis zum Ende der Datei
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

        public int CountEntries()
        {
            return entries.Count;
        }

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

        public void WriteToStream(BinaryWriter writer)
        {
            // Konvertieren Sie das Bild in ein Byte-Array
            byte[] imageData = ImageToByteArray(Image);

            // Schreiben Sie die Bilddaten in den Stream
            writer.Write(imageData);
        }

        private byte[] ImageToByteArray(Bitmap image)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
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
                return null; // oder werfen Sie eine Ausnahme
            }
        }

        public int CountEntries()
        {
            return entries.Count;
        }

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

        public void LoadFromFile(string filename)
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

                        // Überprüfen Sie, ob genügend Daten für die Flagge vorhanden sind
                        if (imageData.Length >= 4)
                        {
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
                        else
                        {
                            // Behandeln Sie den Fehler
                        }
                    }
                }
            }
        }


        private Bitmap LoadRawImage(byte[] imageData)
        {
            int width = 44;
            int height = 44;
            Bitmap image = new Bitmap(width, height);
            int index = 4; // überspringen Sie die Flagge

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
        }

        private Bitmap LoadRunImage(byte[] imageData)
        {
            int width = BitConverter.ToUInt16(imageData, 4);
            int height = BitConverter.ToUInt16(imageData, 6);
            Bitmap image = new Bitmap(width, height);
            int index = 8 + height * 2; // überspringen Sie die Flagge, Breite, Höhe und LStart

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

        public void AddEntry(ArtDataEntry entry)
        {
            entries.Add(entry);
        }

        public Bitmap GetImage(int index)
        {
            if (index >= 0 && index < entries.Count)
            {
                return entries[index].Image;
            }
            else
            {
                return null; // oder werfen Sie eine Ausnahme
            }
        }

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
    }
    #endregion
}


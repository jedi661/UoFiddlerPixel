// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  * 
//  * "THE WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class AnimationVDForm : Form
    {
        public AnimationVDForm()
        {
            InitializeComponent();
        }

        private void btLoadVD_Click(object sender, EventArgs e)
        {
            // Erstellen eines OpenFileDialogs
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Legen Sie die Eigenschaften des OpenFileDialogs fest
            openFileDialog.Filter = "VD-Dateien (*.vd)|*.vd|Alle Dateien (*.*)|*.*";
            openFileDialog.InitialDirectory = @"C:\Temp";
            openFileDialog.Title = "Select a VD file";

            // Rufen Sie die Methode ShowDialog() auf
            DialogResult result = openFileDialog.ShowDialog();

            // Überprüfen Sie den DialogResult-Wert
            if (result == DialogResult.OK)
            {
                // Geben Sie den Pfad der ausgewählten Datei an die Methode File.OpenRead() weiter
                Stream stream = File.OpenRead(openFileDialog.FileName);

                // Erstellen eines StreamReader-Objekts
                StreamReader reader = new StreamReader(stream);

                // Lesen des Inhalts der .vd-Datei
                string contents = reader.ReadToEnd();

                // Extrahieren der Frames aus dem Inhalt
                string[] frames = contents.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

                // Hinzufügen der Frames zu einer Liste
                List<string> frameList = new List<string>();
                foreach (string frame in frames)
                {
                    frameList.Add(frame);
                }

                // Anzeigen der Liste der Frames in listBox1
                listBox1.Items.Clear();
                listBox1.Items.AddRange(frameList.ToArray());

                // Event-Handler für das SelectedIndexChanged-Event der ListBox hinzufügen
                listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Überprüfen, ob ein Element in der ListBox ausgewählt ist
            if (listBox1.SelectedIndex != -1)
            {
                // Holen Sie sich den ausgewählten Frame aus der ListBox
                string selectedFrame = listBox1.SelectedItem.ToString();

                try
                {
                    // Konvertieren Sie den ausgewählten Frame in ein Byte-Array
                    byte[] frameBytes = Convert.FromBase64String(selectedFrame);

                    // Erstellen Sie einen MemoryStream aus dem Byte-Array
                    using (MemoryStream ms = new MemoryStream(frameBytes))
                    {
                        // Erstellen Sie eine Bitmap aus dem MemoryStream
                        Bitmap bitmap = new Bitmap(ms);

                        // Setzen Sie das Bild der PictureBox auf die erstellte Bitmap
                        pictureBox1.Image = bitmap;
                    }
                }
                catch (FormatException)
                {
                    // Der ausgewählte Frame ist kein gültiger Base64-String
                    // Überspringen Sie diesen Frame und zeigen Sie eine Fehlermeldung an
                    MessageBox.Show("Der ausgewählte Frame ist kein gültiges Bild.");
                }
            }
        }

        private void btCopyToClipboard_Click(object sender, EventArgs e)
        {
            // Erstellen Sie einen StringBuilder
            StringBuilder sb = new StringBuilder();

            // Fügen Sie jedes Element in der ListBox zum StringBuilder hinzu
            foreach (object item in listBox1.Items)
            {
                sb.AppendLine(item.ToString());
            }

            // Kopieren Sie den Inhalt des StringBuilders in die Zwischenablage
            Clipboard.SetText(sb.ToString());
        }

        private void btExtractImages_Click(object sender, EventArgs e)
        {
            // Erstellen eines OpenFileDialogs
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Legen Sie die Eigenschaften des OpenFileDialogs fest
            openFileDialog.Filter = "VD-Dateien (*.vd)|*.vd|Alle Dateien (*.*)|*.*";
            openFileDialog.InitialDirectory = @"C:\Temp";
            openFileDialog.Title = "Select a VD file";

            // Rufen Sie die Methode ShowDialog() auf
            DialogResult result = openFileDialog.ShowDialog();

            // Überprüfen Sie den DialogResult-Wert
            if (result == DialogResult.OK)
            {
                // Geben Sie den Pfad der ausgewählten Datei an die Methode File.OpenRead() weiter
                Stream stream = File.OpenRead(openFileDialog.FileName);

                // Erstellen eines StreamReader-Objekts
                StreamReader reader = new StreamReader(stream);

                // Lesen des Inhalts der .vd-Datei
                string contents = reader.ReadToEnd();

                // Extrahieren der Frames aus dem Inhalt
                string[] frames = contents.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

                // Erstellen eines FolderBrowserDialogs
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

                // Rufen Sie die Methode ShowDialog() auf
                DialogResult folderResult = folderBrowserDialog.ShowDialog();

                // Überprüfen Sie den DialogResult-Wert
                if (folderResult == DialogResult.OK)
                {
                    // Speichern Sie den Pfad des ausgewählten Verzeichnisses
                    string selectedPath = folderBrowserDialog.SelectedPath;

                    // Durchlaufen Sie jeden Frame in der .vd-Datei
                    for (int i = 0; i < frames.Length; i++)
                    {
                        try
                        {
                            // Konvertieren Sie den Frame in ein Byte-Array
                            byte[] frameBytes = Convert.FromBase64String(frames[i]);

                            // Erstellen Sie einen MemoryStream aus dem Byte-Array
                            using (MemoryStream ms = new MemoryStream(frameBytes))
                            {
                                // Erstellen Sie eine Bitmap aus dem MemoryStream
                                Bitmap bitmap = new Bitmap(ms);

                                // Speichern Sie das Bild im gewünschten Verzeichnis
                                bitmap.Save(Path.Combine(selectedPath, $"arts{i}.bmp"));
                            }
                        }
                        catch (FormatException)
                        {
                            // Der Frame ist kein gültiger Base64-String
                            // Überspringen Sie diesen Frame
                        }
                        catch (ArgumentException)
                        {
                            // Der MemoryStream enthält kein gültiges Bildformat
                            // Überspringen Sie diesen Frame
                        }
                    }
                }
            }
        }
    }
}

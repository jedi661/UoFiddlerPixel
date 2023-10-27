// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  * 
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Linq;
using Ultima;
using UoFiddler.Controls.Classes;
using System.IO;


namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class GumpsEdit : Form
    {
        private Dictionary<int, string> idNames;
        private string xmlFilePath;
        public GumpsEdit()
        {
            InitializeComponent();

            this.Load += GumpsEdit_Load; // Fügen Sie das Load-Ereignis hinzu            
            //pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IDGumpNames.xml");
            XDocument doc;

            if (!File.Exists(xmlFilePath))
            {
                doc = new XDocument(new XElement("IDNames"));
                doc.Save(xmlFilePath);
            }
            else
            {
                doc = XDocument.Load(xmlFilePath);
            }

            idNames = doc.Root.Elements("ID")
                .ToDictionary(x => (int)x.Attribute("value"), x => (string)x.Attribute("name"));

            UpdateTotalIDsLabel();
            UpdateFreeIDsLabel();
        }

        #region PopulateListBox
        private void PopulateListBox(bool showFreeSlots = false)
        {
            listBox.BeginUpdate(); // Deaktivieren Sie ListBox-Aktualisierungen

            for (int i = 0; i < Gumps.GetCount(); i++)
            {
                string hexValue = i.ToString("X"); // Konvertiert die ID in eine Hexadezimalzeichenfolge
                if (Gumps.IsValidIndex(i))
                {
                    string idName = idNames.ContainsKey(i) ? idNames[i] : "";
                    listBox.Items.Add($"ID: {i} (0x{hexValue}) - {idName}"); // Fügt sowohl die Dezimal- als auch die Hexadezimalrepräsentation der ID zur ListBox hinzu
                }
                else if (showFreeSlots)
                {
                    listBox.Items.Add($"ID: {i} (0x{hexValue}) - Freier Platz");
                }
            }

            listBox.EndUpdate(); // Aktivieren Sie ListBox-Aktualisierungen
        }
        #endregion

        #region Search
        private void SearchByIdToolStripTextBox_TextChanged(object sender, EventArgs e)
        {
            string searchText = searchByIdToolStripTextBox.Text.Trim();
            int id;

            // Versuchen Sie, den Suchtext als Ganzzahl zu interpretieren
            if (int.TryParse(searchText, out id) ||
                (searchText.StartsWith("0x") && int.TryParse(searchText.Substring(2), NumberStyles.HexNumber, null, out id)))
            {
                // Durchsuchen Sie die ListBox nach dem Element mit der entsprechenden ID
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    string item = listBox.Items[i].ToString();
                    if (item.Contains($"ID: {id}"))
                    {
                        // Wählen Sie das gefundene Element aus und beenden Sie die Suche
                        listBox.SelectedIndex = i;
                        break;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(searchText) && !searchText.All(char.IsDigit))
            {
                // Wenn der Suchtext keine Ganzzahl ist und mindestens ein Zeichen enthält, das kein Ziffer ist,
                // suchen Sie nach dem Namen in idNames
                var matchingIds = idNames.Where(kv => kv.Value.Equals(searchText)).Select(kv => kv.Key).ToList();
                if (matchingIds.Count > 0)
                {
                    // Durchsuchen Sie die ListBox nach dem Element mit dem entsprechenden Namen
                    for (int i = 0; i < listBox.Items.Count; i++)
                    {
                        string item = listBox.Items[i].ToString();
                        if (item.Contains(searchText))
                        {
                            // Wählen Sie das gefundene Element aus und beenden Sie die Suche
                            listBox.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }

        }
        #endregion

        #region Load ListBox
        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Überprüfen Sie, ob ein Element in der ListBox ausgewählt ist
            if (listBox.SelectedIndex == -1)
            {
                return;
            }

            // Holen Sie sich das ausgewählte Element in der ListBox und konvertieren Sie es in eine Zeichenfolge
            string selectedItem = listBox.Items[listBox.SelectedIndex].ToString();

            // Finden Sie den Startindex und die Länge des Ganzzahlwerts in der Zeichenfolge
            int startIdx = selectedItem.IndexOf("ID: ") + 4;
            int length = selectedItem.IndexOf(" (") - startIdx;

            // Extrahieren Sie den Ganzzahlwert aus der Zeichenfolge
            string intValueStr = selectedItem.Substring(startIdx, length);

            // Konvertieren Sie den Ganzzahlwert in eine Ganzzahl
            int i = int.Parse(intValueStr);

            // Überprüfen Sie, ob die ID gültig ist
            if (Gumps.IsValidIndex(i))
            {
                // Holen Sie sich das Gump für die ID
                Bitmap bmp = Gumps.GetGump(i);

                // Überprüfen Sie, ob das Gump vorhanden ist
                if (bmp != null)
                {
                    // Setzen Sie das Hintergrundbild der PictureBox auf das Gump
                    pictureBox.BackgroundImage = bmp;

                    // Aktualisieren Sie die Labels mit der ID und der Größe des Gumps
                    IDLabel.Text = $"ID: 0x{i:X} ({i})";
                    SizeLabel.Text = $"Size: {bmp.Width},{bmp.Height}";
                }
                else
                {
                    // Setzen Sie das Hintergrundbild der PictureBox auf null, wenn das Gump nicht vorhanden ist
                    pictureBox.BackgroundImage = null;
                }
            }
            else
            {
                // Setzen Sie das Hintergrundbild der PictureBox auf null, wenn die ID ungültig ist
                pictureBox.BackgroundImage = null;
            }

            // Aktualisieren Sie die ListBox
            listBox.Invalidate();
        }
        private void GumpsEdit_Load(object sender, EventArgs e)
        {
            PopulateListBox(); // Rufen Sie die Methode zum Füllen der ListBox auf

            // Wählen Sie den Index 0 in der ListBox aus
            if (listBox.Items.Count > 0)
            {
                listBox.SelectedIndex = 0;
            }

            // Zeigen Sie das entsprechende Bild in der PictureBox an
            if (Gumps.IsValidIndex(0))
            {
                pictureBox.BackgroundImage = Gumps.GetGump(0);
            }
        }
        #endregion

        #region Copy clipboard
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex != -1)
            {
                string selectedItem = listBox.Items[listBox.SelectedIndex].ToString();
                int startIdx = selectedItem.IndexOf("ID: ") + 4; // Startindex des Ganzzahlwerts
                int length = selectedItem.IndexOf(" (") - startIdx; // Länge des Ganzzahlwerts
                string intValueStr = selectedItem.Substring(startIdx, length); // Extrahieren des Ganzzahlwerts
                int i = int.Parse(intValueStr);

                //int i = int.Parse(listBox.Items[listBox.SelectedIndex].ToString());
                if (Gumps.IsValidIndex(i))
                {
                    Bitmap originalBmp = Gumps.GetGump(i);
                    if (originalBmp != null)
                    {
                        // Erstellen Sie eine Kopie des Originalbildes
                        Bitmap bmp = new Bitmap(originalBmp);

                        // Farbänderungsfunktion direkt eingebaut
                        for (int y = 0; y < bmp.Height; y++)
                        {
                            for (int x = 0; x < bmp.Width; x++)
                            {
                                Color pixelColor = bmp.GetPixel(x, y);
                                if (pixelColor.R == 211 && pixelColor.G == 211 && pixelColor.B == 211) // Check if the color of the pixel is #D3D3D3
                                {
                                    bmp.SetPixel(x, y, Color.Black); // Change the color of the pixel to black
                                }
                            }
                        }

                        // Convert the image to a 24-bit color depth
                        Bitmap bmp24bit = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        using (Graphics g = Graphics.FromImage(bmp24bit))
                        {
                            g.DrawImage(bmp, new Rectangle(0, 0, bmp24bit.Width, bmp24bit.Height));
                        }

                        // Copy the graphic to the clipboard
                        Clipboard.SetImage(bmp24bit);
                        MessageBox.Show("The image has been copied to the clipboard!");
                    }
                    else
                    {
                        MessageBox.Show("No image to copy!");
                    }
                }
                else
                {
                    MessageBox.Show("No image to copy!");
                }
            }
            else
            {
                MessageBox.Show("No image to copy!");
            }
        }
        #endregion

        #region Import Import clipboard - Import graphics from clipboard.
        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Überprüfen, ob die Zwischenablage ein Bild enthält
            if (Clipboard.ContainsImage())
            {
                // Retrieve the image from the clipboard
                using (Bitmap bmp = new Bitmap(Clipboard.GetImage()))
                {
                    // Überprüfen Sie, ob ein Element in der ListBox ausgewählt ist
                    if (listBox.SelectedIndex != -1)
                    {
                        // Holen Sie sich das ausgewählte Element in der ListBox und konvertieren Sie es in eine Zeichenfolge
                        string selectedItem = listBox.Items[listBox.SelectedIndex].ToString();

                        // Finden Sie den Startindex und die Länge des Ganzzahlwerts in der Zeichenfolge
                        int startIdx = selectedItem.IndexOf('(') + 3; // Startindex des Ganzzahlwerts
                        int length = selectedItem.IndexOf(')') - startIdx; // Länge des Ganzzahlwerts

                        // Extrahieren Sie den Ganzzahlwert aus der Zeichenfolge
                        string intValueStr = selectedItem.Substring(startIdx, length);

                        // Konvertieren Sie den Ganzzahlwert in eine Ganzzahl
                        int index = int.Parse(intValueStr);

                        if (index >= 0 && index < Gumps.GetCount())
                        {
                            // Create a new bitmap with the same size as the image from the clipboard
                            Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);

                            // Define the colors to ignore
                            Color[] colorsToIgnore = new Color[]
                            {
                        Color.FromArgb(211, 211, 211), // #D3D3D3
                        Color.FromArgb(0, 0, 0),       // #000000
                        Color.FromArgb(255, 255, 255)  // #FFFFFF
                            };

                            // Iterate through each pixel of the image
                            for (int x = 0; x < bmp.Width; x++)
                            {
                                for (int y = 0; y < bmp.Height; y++)
                                {
                                    // Get the color of the current pixel
                                    Color pixelColor = bmp.GetPixel(x, y);

                                    // Check if the color of the current pixel is one of the colors to ignore
                                    if (colorsToIgnore.Contains(pixelColor))
                                    {
                                        // Set the color of the current pixel to transparent
                                        newBmp.SetPixel(x, y, Color.Transparent);
                                    }
                                    else
                                    {
                                        // Set the color of the current pixel to the color of the original image
                                        newBmp.SetPixel(x, y, pixelColor);
                                    }
                                }
                            }

                            // Call the ReplaceGump method with the selected graphic ID and the new bitmap
                            Gumps.ReplaceGump(index, newBmp);
                            ControlEvents.FireGumpChangeEvent(this, index);

                            listBox.Invalidate();
                            ListBox_SelectedIndexChanged(this, EventArgs.Empty);

                            Options.ChangedUltimaClass["Gumps"] = true;
                        }
                        else
                        {
                            MessageBox.Show("Invalid index.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No image to copy!");
                    }
                }
            }
            else
            {
                MessageBox.Show("No image in the clipboard.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            UpdateTotalIDsLabel();
            UpdateFreeIDsLabel();
        }


        // Import und Export Strg+V and Strg+X
        private void GumpControl_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Ctrl+V key combination has been pressed
            if (e.Control && e.KeyCode == Keys.V)
            {
                // Calling the importToolStripMenuItem_Click method to import the graphic from the clipboard.
                importToolStripMenuItem_Click(sender, e);
            }
            // Checking if the Ctrl+X key combination has been pressed
            else if (e.Control && e.KeyCode == Keys.X)
            {
                // Calling the copyToolStripMenuItem_Click method to import the graphic from the clipboard.
                copyToolStripMenuItem_Click(sender, e);
            }
        }
        #endregion

        #region Remove       
        private void OnClickRemove(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex != -1)
            {
                string selectedItem = listBox.Items[listBox.SelectedIndex].ToString();
                int startIdx = selectedItem.IndexOf("ID: ") + 4; // Startindex des Ganzzahlwerts
                int length = selectedItem.IndexOf(" (") - startIdx; // Länge des Ganzzahlwerts
                string intValueStr = selectedItem.Substring(startIdx, length); // Extrahieren des Ganzzahlwerts

                int i = int.Parse(intValueStr);

                if (Gumps.IsValidIndex(i))
                {
                    // Entfernen Sie die Grafik
                    Gumps.RemoveGump(i);
                    ControlEvents.FireGumpChangeEvent(this, i);

                    // Entfernen Sie das Bild aus der PictureBox
                    pictureBox.Image = null;

                    listBox.Invalidate();
                    ListBox_SelectedIndexChanged(this, EventArgs.Empty);

                    Options.ChangedUltimaClass["Gumps"] = true;
                }
            }
            UpdateTotalIDsLabel();
            UpdateFreeIDsLabel();
        }
        #endregion

        #region Replace
        private void OnClickReplace(object sender, EventArgs e)
        {
            if (listBox.SelectedItems.Count != 1)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose image file to replace";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                using (var bmpTemp = new Bitmap(dialog.FileName))
                {
                    Bitmap bitmap = new Bitmap(bmpTemp);

                    if (dialog.FileName.Contains(".bmp"))
                    {
                        bitmap = Utils.ConvertBmp(bitmap);
                    }

                    string selectedItem = listBox.Items[listBox.SelectedIndex].ToString();
                    int startIdx = selectedItem.IndexOf("ID: ") + 4;
                    int length = selectedItem.IndexOf(" (") - startIdx;
                    string intValueStr = selectedItem.Substring(startIdx, length);
                    int i = int.Parse(intValueStr);

                    Gumps.ReplaceGump(i, bitmap);

                    ControlEvents.FireGumpChangeEvent(this, i);

                    listBox.Invalidate();
                    ListBox_SelectedIndexChanged(this, EventArgs.Empty);

                    Options.ChangedUltimaClass["Gumps"] = true;
                }
            }
        }
        #endregion

        #region Show Free Slots

        private bool isShowingFreeSlots = false; // Zustandsvariable hinzufügen
        private void showFreeIdsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox.Items.Clear(); // Löschen Sie zuerst alle vorhandenen Elemente in der ListBox

            if (!isShowingFreeSlots)
            {
                // Wenn der Button das erste Mal geklickt wird, zeigen Sie alle IDs an und färben Sie den Hintergrund grün
                PopulateListBox(true); // Rufen Sie dann die Methode zum Füllen der ListBox auf
                showFreeIdsToolStripMenuItem.BackColor = Color.Green;
            }
            else
            {
                // Wenn der Button erneut geklickt wird, zeigen Sie nur die gültigen IDs an und setzen Sie die Hintergrundfarbe zurück
                PopulateListBox();
                showFreeIdsToolStripMenuItem.BackColor = SystemColors.Control;
            }

            // Den Zustand umschalten
            isShowingFreeSlots = !isShowingFreeSlots;
        }
        #endregion

        #region Frind Netzt Free ID
        private void OnClickFindFree(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex != -1)
            {
                string selectedItem = listBox.Items[listBox.SelectedIndex].ToString();
                int startIdx = selectedItem.IndexOf("ID: ") + 4;
                int length = selectedItem.IndexOf(" (") - startIdx;
                string intValueStr = selectedItem.Substring(startIdx, length);
                int id = int.Parse(intValueStr);
                ++id;

                for (int i = listBox.SelectedIndex + 1; i < listBox.Items.Count; ++i, ++id)
                {
                    string item = listBox.Items[i].ToString();
                    startIdx = item.IndexOf("ID: ") + 4;
                    length = item.IndexOf(" (") - startIdx;
                    intValueStr = item.Substring(startIdx, length);
                    int itemId = int.Parse(intValueStr);

                    if (id < itemId || (isShowingFreeSlots && !Gumps.IsValidIndex(itemId)))
                    {
                        listBox.SelectedIndex = i;
                        break;
                    }
                }

                // Falls keine leere ID gefunden wurde und isShowingFreeSlots aktiviert ist,
                // wird eine neue ID am Ende der ListBox hinzugefügt
                if (listBox.SelectedIndex == -1 && isShowingFreeSlots)
                {
                    int newId = Gumps.GetCount();
                    listBox.Items.Add($"ID: {newId} (0x{newId:X})");
                    listBox.SelectedIndex = listBox.Items.Count - 1;
                }
            }
        }
        #endregion

        #region Add Names ID Gumps XML
        public void SetIDName(int id, string name)
        {
            // Fügen Sie den Namen zur ID im Dictionary hinzu oder aktualisieren Sie ihn
            idNames[id] = name;

            // Speichern Sie die Änderungen in der XML-Datei
            XDocument doc = new XDocument(
                new XElement("IDNames",
                    idNames.Select(kv => new XElement("ID", new XAttribute("value", kv.Key), new XAttribute("name", kv.Value)))
                )
            );
            doc.Save(xmlFilePath);
        }
        private void addIDNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex != -1)
            {
                string selectedItem = listBox.Items[listBox.SelectedIndex].ToString();
                int startIdx = selectedItem.IndexOf("ID: ") + 4;
                int length = selectedItem.IndexOf(" (") - startIdx;
                string intValueStr = selectedItem.Substring(startIdx, length);
                int id = int.Parse(intValueStr);

                using (var form = new System.Windows.Forms.Form())
                {
                    var idTextBox = new System.Windows.Forms.TextBox { Text = id.ToString(), Location = new Point(10, 10) };
                    var nameTextBox = new System.Windows.Forms.TextBox { Text = idNames.ContainsKey(id) ? idNames[id] : "", Location = new Point(10, 40) };
                    var okButton = new System.Windows.Forms.Button { Text = "OK", Location = new Point(10, 70) };
                    var deleteButton = new System.Windows.Forms.Button { Text = "Löschen", Location = new Point(nameTextBox.Location.X + nameTextBox.Width + 4, nameTextBox.Location.Y) };

                    deleteButton.Click += (s, e) =>
                    {
                        // Leeren Sie den Text des nameTextBox
                        nameTextBox.Text = "";
                    };

                    okButton.Click += (s, e) =>
                    {
                        // Aktualisieren Sie den Namen der ID in idNames und speichern Sie die Änderungen in der XML-Datei
                        idNames[int.Parse(idTextBox.Text)] = nameTextBox.Text;

                        XDocument doc = new XDocument(
                            new XElement("IDNames",
                                idNames.Select(kv => new XElement("ID", new XAttribute("value", kv.Key), new XAttribute("name", kv.Value)))
                            )
                        );
                        doc.Save(xmlFilePath);

                        form.DialogResult = DialogResult.OK;
                    };

                    form.Controls.Add(idTextBox);
                    form.Controls.Add(nameTextBox);
                    form.Controls.Add(okButton);
                    form.Controls.Add(deleteButton);

                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        // Aktualisieren Sie die ListBox
                        listBox.Items.Clear();
                        PopulateListBox();
                    }
                }
            }
            // Aktualisieren Sie die Labels
            UpdateTotalIDsLabel();
            UpdateFreeIDsLabel();
        }
        #endregion

        private void OnClickSave(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure? Will take a while", "Save", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Gumps.Save(Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Saved to {Options.OutputPath}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Gumps"] = false;
        }

        #region ID Name Counter
        private void UpdateTotalIDsLabel()
        {
            // Zählen Sie die Anzahl der belegten IDs
            int totalIDs = idNames.Count;

            // Aktualisieren Sie das Label
            IDLabelinortotal.Text = $"Named ID Total: {totalIDs}";
        }

        private void UpdateFreeIDsLabel()
        {
            // Berechnen Sie die Anzahl der freien IDs
            int freeIDs = Gumps.GetCount() - idNames.Count;

            // Aktualisieren Sie das Label
            LabelFreeIDs.Text = $"Total Free IDs: {freeIDs}";
        }
        #endregion
    }
}

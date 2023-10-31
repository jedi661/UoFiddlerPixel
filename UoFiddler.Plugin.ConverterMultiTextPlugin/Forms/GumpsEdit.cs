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
using DrawingImage = System.Drawing.Image;
using System.Drawing.Imaging;
using static System.Runtime.InteropServices.JavaScript.JSType;


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

            this.KeyPreview = true;
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

        #region OnClickSave
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
        #endregion

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

        #region Paint

        // Zustandsvariable hinzufügen
        private bool isDrawing = false;
        private Color drawingColor = Color.Black;
        private Stack<Bitmap> previousImages = new Stack<Bitmap>();
        private Bitmap originalImage;
        private Bitmap originalImageDraw;
        private Bitmap currentImage;
        // Erstellen Sie eine Liste, um die Linien zu speichern
        List<List<Point>> lines = new List<List<Point>>();
        // Erstellen Sie eine temporäre Liste, um die aktuelle Linie zu speichern
        List<Point> currentLine;
        // Erstellen Sie einen zweiten Stapel für die Redo-Funktion
        Stack<Bitmap> redoImages = new Stack<Bitmap>();

        #region LoadImage
        private void LoadImageIntopictureBoxDraw(System.Drawing.Image image)
        {
            tbPinselSize.Text = "2";

            // Überprüfen Sie, ob das Bild nicht null ist
            if (image != null)
            {
                // Speichern Sie das Originalbild und erstellen Sie eine veränderbare Kopie
                originalImage = new Bitmap(image);
                originalImageDraw = new Bitmap(originalImage);

                // Weisen Sie die veränderbare Kopie der pictureBoxDraw zu
                pictureBoxDraw.Image = originalImageDraw;
            }
            else
            {
                MessageBox.Show("Das Bild konnte nicht geladen werden.");
            }
        }
        #endregion

        #region MouseMove

        private void pictureBoxDraw_MouseMove(object sender, MouseEventArgs e)
        {
            // Zeichnen Sie nur, wenn die linke Maustaste gedrückt wird und die Zeichenfunktion aktiviert ist
            if (e.Button == MouseButtons.Left && isDrawing)
            {
                // Stellen Sie sicher, dass das Bild nicht null ist, bevor Sie darauf zeichnen
                if (originalImageDraw != null)
                {
                    // Holen Sie sich die Pinselgröße aus der TextBox
                    float pinselSize = float.Parse(tbPinselSize.Text);

                    // Zeichnen Sie nur, wenn die Pinselgröße größer als 0 ist
                    if (pinselSize > 0)
                    {
                        using (Graphics g = Graphics.FromImage(originalImageDraw))
                        {
                            // Berechnen Sie den Offset durch das Zentrieren des Bildes
                            int offsetX = (pictureBoxDraw.Width - originalImageDraw.Width) / 2;
                            int offsetY = (pictureBoxDraw.Height - originalImageDraw.Height) / 2;

                            // Berechnen Sie die korrekten Koordinaten auf dem Bild
                            int imageX = e.X - offsetX;
                            int imageY = e.Y - offsetY;

                            // Stellen Sie sicher, dass die Koordinaten innerhalb der Grenzen des Bildes liegen
                            if (imageX >= 0 && imageX < originalImageDraw.Width && imageY >= 0 && imageY < originalImageDraw.Height)
                            {
                                // Zeichnen Sie auf dem Graphics-Objekt mit der angegebenen Pinselgröße
                                g.FillEllipse(new SolidBrush(drawingColor), imageX, imageY, pinselSize, pinselSize);

                                // Fügen Sie den aktuellen Punkt zur aktuellen Linie hinzu
                                currentLine.Add(new Point(imageX, imageY));

                                // Aktualisieren Sie das pictureBoxDraw-Steuerelement
                                pictureBoxDraw.Invalidate();
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region MouseDown
        private void pictureBoxDraw_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isDrawing)
            {
                // Fügen Sie den aktuellen Zustand des Bildes zum Stapel hinzu, bevor Sie anfangen zu zeichnen
                previousImages.Push((Bitmap)originalImageDraw.Clone());

                // Initialisieren Sie die aktuelle Linie
                currentLine = new List<Point>();
            }
        }


        #endregion

        #region MouseUp
        private void pictureBoxDraw_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isDrawing)
            {
                // Zeichnen Sie die gesamte Linie auf dem Bild
                using (Graphics g = Graphics.FromImage(originalImageDraw))
                {
                    float pinselSize = float.Parse(tbPinselSize.Text);
                    SolidBrush brush = new SolidBrush(drawingColor);

                    foreach (Point p in currentLine)
                    {
                        g.FillEllipse(brush, p.X, p.Y, pinselSize, pinselSize);
                    }

                    pictureBoxDraw.Invalidate();
                }

                // Fügen Sie den geänderten Zustand des Bildes zum Stapel hinzu
                previousImages.Push((Bitmap)originalImageDraw.Clone());

                // Leeren Sie die aktuelle Linie
                currentLine.Clear();
            }
        }

        #endregion

        #region Textbox Color Change
        private void TextBoxColor_TextChanged(object sender, EventArgs e)
        {
            // Ändern Sie die Zeichenfarbe, wenn der Text in TextBoxColor geändert wird
            string colorText = TextBoxColor.Text.TrimStart('#');

            // Überprüfen Sie, ob der Text ein gültiger Hexadezimalwert ist
            if (int.TryParse("FF" + colorText, System.Globalization.NumberStyles.HexNumber, null, out int argb))
            {
                drawingColor = Color.FromArgb(argb);
            }
            else
            {
                MessageBox.Show("Bitte geben Sie einen gültigen Farbcode ein.");
            }
        }
        #endregion

        #region Undo
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Führen Sie die "Undo"-Aktion zweimal aus
            for (int i = 0; i < 2; i++)
            {
                // Stellen Sie das Bild auf den vorherigen Zustand zurück, wenn der Stapel nicht leer ist
                if (previousImages.Count > 0)
                {
                    // Fügen Sie den aktuellen Zustand zum Redo-Stapel hinzu
                    redoImages.Push(originalImageDraw);

                    originalImageDraw = previousImages.Pop();
                    pictureBoxDraw.Image = originalImageDraw;
                }
            }
        }

        #endregion

        #region Color Renderer
        private class MyRenderer : ToolStripProfessionalRenderer
        {
            private GumpsEdit gumpsEdit;

            public MyRenderer(GumpsEdit gumpsEdit)
            {
                this.gumpsEdit = gumpsEdit;
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (e.Item == gumpsEdit.paintToolStripMenuItem)
                {
                    e.Graphics.FillRectangle(Brushes.Green, e.Item.Bounds);
                }
                else
                {
                    base.OnRenderMenuItemBackground(e);
                }
            }
        }
        #endregion

        #region Paint
        private void paintToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Den Zustand umschalten
            isDrawing = !isDrawing;

            // Den Hintergrund ändern und eine Nachricht anzeigen
            if (isDrawing)
            {
                contextMenuStripPicturebBox.Renderer = new MyRenderer(this);
                MessageBox.Show("Sie können jetzt zeichnen.");

                // Laden Sie das Bild in die pictureBoxDraw
                LoadImageIntopictureBoxDraw(pictureBoxDraw.Image);
            }
            else
            {
                contextMenuStripPicturebBox.Renderer = new ToolStripProfessionalRenderer();
                MessageBox.Show("Zeichnen ist jetzt deaktiviert.");
            }
        }
        #endregion

        #region Import Clipbord
        private void importImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Überprüfen Sie, ob ein Bild im Zwischenspeicher vorhanden ist
            if (Clipboard.ContainsImage())
            {
                // Holen Sie das Bild aus dem Zwischenspeicher
                System.Drawing.Image clipboardImage = Clipboard.GetImage();

                // Speichern Sie das Bild und erstellen Sie eine veränderbare Kopie
                originalImage = new Bitmap(clipboardImage);
                originalImageDraw = new Bitmap(originalImage);

                // Laden Sie das Bild in die PictureBoxDraw
                pictureBoxDraw.Image = originalImageDraw;
            }
            else
            {
                MessageBox.Show("Es gibt kein Bild im Zwischenspeicher.");
            }
        }
        #endregion

        #region Color Dialog
        private void btColorDialog_Click(object sender, EventArgs e)
        {
            // Erstellen Sie ein neues ColorDialog-Objekt
            ColorDialog colorDialog = new ColorDialog();

            // Zeigen Sie den Dialog an und überprüfen Sie, ob der Benutzer auf OK geklickt hat
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                // Konvertieren Sie die ausgewählte Farbe in einen Hexadezimalwert
                string hexColor = colorDialog.Color.R.ToString("X2") + colorDialog.Color.G.ToString("X2") + colorDialog.Color.B.ToString("X2");

                // Fügen Sie den Hexadezimalwert in die TextBoxColor ein
                TextBoxColor.Text = hexColor;
            }
        }
        #endregion

        private void tbPinselSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Erlauben Sie nur die Eingabe von Zahlen
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        #region Pinsel Size
        private void tbPinselSize_TextChanged(object sender, EventArgs e)
        {
            // Stellen Sie sicher, dass der Wert nicht größer als 60 ist
            if (int.TryParse(tbPinselSize.Text, out int value))
            {
                if (value > 60)
                {
                    // Ändern Sie den Wert in der TextBox auf 60
                    tbPinselSize.Text = "60";
                }
            }
            else if (tbPinselSize.Text != string.Empty)
            {
                // Wenn der Text keine gültige Zahl ist und nicht leer ist,
                // setzen Sie ihn auf den Standardwert zurück
                tbPinselSize.Text = "2";
            }
        }
        #endregion

        #region GumpsEdit KeyDown
        private void GumpsEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Back)
            {
                undoToolStripMenuItem_Click(sender, e);
            }
            if (e.Control && e.KeyCode == Keys.R)
            {
                redoToolStripMenuItem_Click(sender, e);
            }
        }
        #endregion

        #region Copy Image PictureboxDraw Clipbord
        private void CopyImagePictureBoxDrawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxDraw.Image != null)
            {
                Clipboard.SetImage(pictureBoxDraw.Image);
            }
            else
            {
                MessageBox.Show("Es gibt kein Bild zum Kopieren.");
            }
        }
        #endregion

        #region Send Image to Paint Box
        private void sendImageToPaintBoxToolStripMenuItem_Click(object sender, EventArgs e)
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

                        // Speichern Sie das Bild und erstellen Sie eine veränderbare Kopie
                        originalImageDraw = bmp24bit;

                        // Laden Sie das Bild in die pictureBoxDraw
                        pictureBoxDraw.Image = originalImageDraw;
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

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Stellen Sie das Bild auf den nächsten Zustand vor, wenn der Redo-Stapel nicht leer ist
            if (redoImages.Count > 0)
            {
                // Fügen Sie den aktuellen Zustand zum Undo-Stapel hinzu
                previousImages.Push(originalImageDraw);

                // Stellen Sie das Bild auf den nächsten Zustand vor
                originalImageDraw = redoImages.Pop();
                pictureBoxDraw.Image = originalImageDraw;
            }
        }

        private void rotateImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Überprüfen Sie, ob ein Bild in der pictureBoxDraw vorhanden ist
            if (pictureBoxDraw.Image != null)
            {
                // Drehen Sie das Bild um 90 Grad gegen den Uhrzeigersinn
                pictureBoxDraw.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);

                // Aktualisieren Sie das pictureBoxDraw-Steuerelement
                pictureBoxDraw.Invalidate();
            }
        }

        #region trackBarBrightness

        // Zustandsvariable hinzufügen
        private bool ignoreColors = false;

        private void trackBarBrightness_Scroll(object sender, EventArgs e)
        {
            // Aktualisieren Sie das Label mit dem aktuellen Wert der TrackBar
            labelBrightnessValue.Text = trackBarBrightness.Value.ToString();

            // Erstellen Sie eine temporäre Kopie des Originalbildes
            Bitmap tempBmp = new Bitmap(originalImageDraw);

            // Erstellen Sie eine ColorMatrix und ändern Sie den Helligkeitswert
            float brightness = trackBarBrightness.Value * 0.01f; // Skalieren Sie den Wert auf einen Bereich von -0.3 bis 0.3
            float[][] matrixItems = {
        new float[] {1, 0, 0, 0, 0},
        new float[] {0, 1, 0, 0, 0},
        new float[] {0, 0, 1, 0, 0},
        new float[] {0, 0, 0, 1, 0},
        new float[] {brightness, brightness, brightness, 1, 1}
    };
            ColorMatrix colorMatrix = new ColorMatrix(matrixItems);

            // Erstellen Sie ein ImageAttributes-Objekt und setzen Sie die ColorMatrix
            ImageAttributes imageAttr = new ImageAttributes();
            imageAttr.SetColorMatrix(colorMatrix);

            // Zeichnen Sie das Bild mit den neuen ImageAttributes
            using (Graphics g = Graphics.FromImage(tempBmp))
            {
                if (checkBoxBrightness.Checked)
                {
                    for (int y = 0; y < originalImageDraw.Height; y++)
                    {
                        for (int x = 0; x < originalImageDraw.Width; x++)
                        {
                            Color pixelColor = originalImageDraw.GetPixel(x, y);
                            if (pixelColor.ToArgb() != Color.White.ToArgb() && pixelColor.ToArgb() != Color.Black.ToArgb())
                            {
                                g.DrawImage(originalImageDraw,
                                    new Rectangle(x, y, 1, 1), // Zielrechteck
                                    x, y, 1, 1,
                                    GraphicsUnit.Pixel,
                                    imageAttr);
                            }
                        }
                    }
                }
                else
                {
                    g.DrawImage(originalImageDraw,
                        new Rectangle(0, 0, originalImageDraw.Width, originalImageDraw.Height), // Zielrechteck
                        0, 0, originalImageDraw.Width, originalImageDraw.Height,
                        GraphicsUnit.Pixel,
                        imageAttr);
                }
            }

            // Setzen Sie das geänderte Bild in die PictureBoxDraw
            if (currentImage != null)
            {
                currentImage.Dispose();
            }
            currentImage = tempBmp;
            pictureBoxDraw.Image = currentImage;
        }


        // Event-Handler für die CheckBox
        private void checkBoxBrightness_CheckedChanged(object sender, EventArgs e)
        {
            ignoreColors = checkBoxBrightness.Checked;
        }
        #endregion

        #region ContrastColors
        // Zustandsvariable hinzufügen
        private bool ignoreContrastColors = false;

        private void trackBarContrast_Scroll(object sender, EventArgs e)
        {
            // Aktualisieren Sie das Label mit dem aktuellen Wert der TrackBar
            labelContrastValue.Text = trackBarContrast.Value.ToString();

            // Erstellen Sie eine temporäre Kopie des Originalbildes
            Bitmap tempBmp = new Bitmap(originalImageDraw);

            if (trackBarContrast.Value == 0)
            {
                // Setzen Sie das Bild auf das ursprüngliche Bild zurück
                pictureBoxDraw.Image = new Bitmap(originalImageDraw);
                return;
            }

            // Erstellen Sie eine ColorMatrix und ändern Sie den Kontrastwert
            float contrast = 1 + trackBarContrast.Value * 0.02f; // Skalieren Sie den Wert auf einen Bereich von 1 bis 1.6
            float translate = 0.5f * (1.0f - contrast);
            float[][] matrixItems = {
        new float[] {contrast, 0, 0, 0, 0},
        new float[] {0, contrast, 0, 0, 0},
        new float[] {0, 0, contrast, 0, 0},
        new float[] {0, 0, 0, 1, 0},
        new float[] {translate, translate, translate, 1, 1}
    };
            ColorMatrix colorMatrix = new ColorMatrix(matrixItems);

            // Erstellen Sie ein ImageAttributes-Objekt und setzen Sie die ColorMatrix
            ImageAttributes imageAttr = new ImageAttributes();
            imageAttr.SetColorMatrix(colorMatrix);

            // Zeichnen Sie das Bild mit den neuen ImageAttributes
            using (Graphics g = Graphics.FromImage(tempBmp))
            {
                if (checkBoxContrast.Checked)
                {
                    for (int y = 0; y < originalImageDraw.Height; y++)
                    {
                        for (int x = 0; x < originalImageDraw.Width; x++)
                        {
                            Color pixelColor = originalImageDraw.GetPixel(x, y);
                            if (pixelColor.ToArgb() != Color.White.ToArgb() && pixelColor.ToArgb() != Color.Black.ToArgb())
                            {
                                g.DrawImage(originalImageDraw,
                                    new Rectangle(x, y, 1, 1), // Zielrechteck
                                    x, y, 1, 1,
                                    GraphicsUnit.Pixel,
                                    imageAttr);
                            }
                        }
                    }
                }
                else
                {
                    g.DrawImage(originalImageDraw,
                        new Rectangle(0, 0, originalImageDraw.Width, originalImageDraw.Height), // Zielrechteck
                        0, 0, originalImageDraw.Width, originalImageDraw.Height,
                        GraphicsUnit.Pixel,
                        imageAttr);
                }
            }

            // Setzen Sie das geänderte Bild in die PictureBoxDraw
            if (currentImage != null)
            {
                currentImage.Dispose();
            }
            currentImage = tempBmp;
            pictureBoxDraw.Image = currentImage;
        }

        // Event-Handler für die CheckBox
        private void checkBoxContrast_CheckedChanged(object sender, EventArgs e)
        {
            ignoreContrastColors = checkBoxContrast.Checked;
        }
        #endregion

        #region Color
        // Zustandsvariable hinzufügen

        private void trackBarColorR_Scroll(object sender, EventArgs e)
        {

            // Aktualisieren Sie das Label mit dem aktuellen Wert der TrackBar
            labelColorRValue.Text = trackBarColorR.Value.ToString();

            // Erstellen Sie eine temporäre Kopie des Originalbildes
            Bitmap tempBmp = new Bitmap(originalImageDraw);

            // Erstellen Sie eine ColorMatrix und ändern Sie den Rotwert
            float colorScaleR = trackBarColorR.Value * 0.01f;
            float[][] matrixItems = {
        new float[] {1 + colorScaleR, 0, 0, 0, 0}, // Rotwert anpassen
        new float[] {0, 1, 0, 0, 0},
        new float[] {0, 0, 1, 0, 0},
        new float[] {0, 0, 0, 1, 0},
        new float[] {0, 0, 0, 1, 1}
    };
            ColorMatrix colorMatrix = new ColorMatrix(matrixItems);

            // Erstellen Sie ein ImageAttributes-Objekt und setzen Sie die ColorMatrix
            ImageAttributes imageAttr = new ImageAttributes();
            imageAttr.SetColorMatrix(colorMatrix);

            // Zeichnen Sie das Bild mit den neuen ImageAttributes
            using (Graphics g = Graphics.FromImage(tempBmp))
            {
                g.DrawImage(originalImageDraw,
                    new Rectangle(0, 0, originalImageDraw.Width, originalImageDraw.Height), // Zielrechteck
                    0, 0, originalImageDraw.Width, originalImageDraw.Height,
                    GraphicsUnit.Pixel,
                    imageAttr);
            }

            // Setzen Sie das geänderte Bild in die PictureBoxDraw
            pictureBoxDraw.Image = tempBmp;
        }

        private void trackBarColorG_Scroll(object sender, EventArgs e)
        {

            // Aktualisieren Sie das Label mit dem aktuellen Wert der TrackBar
            labelColorGValue.Text = trackBarColorG.Value.ToString();

            // Erstellen Sie eine temporäre Kopie des Originalbildes
            Bitmap tempBmp = new Bitmap(originalImageDraw);

            // Erstellen Sie eine ColorMatrix und ändern Sie den Grünwert
            float colorScaleG = trackBarColorG.Value * 0.01f;
            float[][] matrixItems = {
        new float[] {1, 0, 0, 0, 0},
        new float[] {0, 1 + colorScaleG, 0, 0, 0}, // Grünwert anpassen
        new float[] {0, 0, 1, 0, 0},
        new float[] {0, 0, 0, 1, 0},
        new float[] {0, 0, 0, 1, 1}
        };
            ColorMatrix colorMatrix = new ColorMatrix(matrixItems);

            // Erstellen Sie ein ImageAttributes-Objekt und setzen Sie die ColorMatrix
            ImageAttributes imageAttr = new ImageAttributes();
            imageAttr.SetColorMatrix(colorMatrix);

            // Zeichnen Sie das Bild mit den neuen ImageAttributes
            using (Graphics g = Graphics.FromImage(tempBmp))
            {
                g.DrawImage(originalImageDraw,
                    new Rectangle(0, 0, originalImageDraw.Width, originalImageDraw.Height), // Zielrechteck
                    0, 0, originalImageDraw.Width, originalImageDraw.Height,
                    GraphicsUnit.Pixel,
                    imageAttr);
            }

            // Setzen Sie das geänderte Bild in die PictureBoxDraw
            pictureBoxDraw.Image = tempBmp;
        }

        private void trackBarColorB_Scroll(object sender, EventArgs e)
        {
            // Update the label with the current value of the TrackBar
            labelColorBValue.Text = trackBarColorB.Value.ToString();

            // Create a temporary copy of the original image
            Bitmap tempBmp = new Bitmap(originalImageDraw);

            // Create a ColorMatrix and change the blue value
            float colorScaleB = trackBarColorB.Value * 0.01f;
            float[][] matrixItems = {
        new float[] {1, 0, 0, 0, 0},
        new float[] {0, 1 , 0 , 0 , 0 },
        new float[] {0, 0, 1 + colorScaleB, 0, 0}, // Adjust blue value
        new float[] {0, 0, 0, 1, 0},
        new float[] {0, 0, 0, 0, 1}
    };
            ColorMatrix colorMatrix = new ColorMatrix(matrixItems);

            // Create an ImageAttributes object and set the ColorMatrix
            ImageAttributes imageAttr = new ImageAttributes();
            imageAttr.SetColorMatrix(colorMatrix);

            // Draw the image with the new ImageAttributes
            using (Graphics g = Graphics.FromImage(tempBmp))
            {
                g.DrawImage(originalImageDraw,
                    new Rectangle(0, 0, originalImageDraw.Width, originalImageDraw.Height), // Destination rectangle
                    0, 0, originalImageDraw.Width, originalImageDraw.Height,
                    GraphicsUnit.Pixel,
                    imageAttr);
            }

            // Set the modified image in the PictureBoxDraw
            pictureBoxDraw.Image = tempBmp;
        }

        // Event-Handler für die CheckBox
        #endregion

        #region Reset RGB
        private void resetButtonRGB_Click(object sender, EventArgs e)
        {
            // Setzen Sie die Werte der TrackBars auf 0
            trackBarColorR.Value = 0;
            trackBarColorG.Value = 0;
            trackBarColorB.Value = 0;

            // Setzen Sie die Texte der Labels zurück
            labelColorRValue.Text = "0";
            labelColorGValue.Text = "0";
            labelColorBValue.Text = "0";

            // Lösen Sie die Scroll-Ereignisse der TrackBars aus
            trackBarColorR_Scroll(sender, e);
            trackBarColorG_Scroll(sender, e);
            trackBarColorB_Scroll(sender, e);
        }
        #endregion
    }
    #endregion
}
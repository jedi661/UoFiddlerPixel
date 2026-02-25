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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Ultima;
using UoFiddler.Controls.Classes;
using System.IO;
using DrawingImage = System.Drawing.Image;
using System.Drawing.Imaging;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class GumpsEdit : Form
    {
        // -----------------------------------------------------------------------
        //  Felder
        // -----------------------------------------------------------------------
        private Dictionary<int, string> idNames;
        private string xmlFilePath;

        // -----------------------------------------------------------------------
        //  Konstruktor
        // -----------------------------------------------------------------------
        public GumpsEdit()
        {
            InitializeComponent();

            this.Load += GumpsEdit_Load;

            // XML-Datei für ID-Namen laden oder neu anlegen
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

        // -----------------------------------------------------------------------
        //  HILFSMETHODE: Liest die aktuell gewählte Gump-ID aus der ListBox.
        //  Gibt -1 zurück, wenn nichts ausgewählt ist.
        // -----------------------------------------------------------------------
        private int GetSelectedId()
        {
            if (listBox.SelectedIndex == -1)
                return -1;

            string item = listBox.Items[listBox.SelectedIndex].ToString();
            int start = item.IndexOf("ID: ") + 4;
            int len = item.IndexOf(" (") - start;

            if (start < 4 || len <= 0)
                return -1;

            return int.TryParse(item.Substring(start, len), out int id) ? id : -1;
        }

        // -----------------------------------------------------------------------
        //  HILFSMETHODE: Setzt ein Bitmap sicher in ein PictureBox-Image-Feld
        //  und gibt das alte Bitmap zurück, damit es disposed werden kann.
        // -----------------------------------------------------------------------
        private void SetPictureBoxImage(PictureBox pb, Bitmap newBmp)
        {
            var old = pb.Image;
            pb.Image = newBmp;
            if (old != null && old != newBmp)
                old.Dispose();
        }

        // -----------------------------------------------------------------------
        //  HILFSMETHODE: Zeichnet transparente Pixel als #D3D3D3-Hintergrund
        //  (Gumps nutzen diesen Grauton als Transparenz-Marker) und konvertiert
        //  das Ergebnis auf 24-bit RGB für die Zwischenablage.
        // -----------------------------------------------------------------------
        private Bitmap ConvertGumpForClipboard(Bitmap src)
        {
            Bitmap copy = new Bitmap(src);

            for (int y = 0; y < copy.Height; y++)
                for (int x = 0; x < copy.Width; x++)
                {
                    Color px = copy.GetPixel(x, y);
                    if (px.R == 211 && px.G == 211 && px.B == 211)
                        copy.SetPixel(x, y, Color.Black);
                }

            // 24-bit-Konvertierung (Clipboard verträgt kein 32-bit ARGB)
            Bitmap bmp24 = new Bitmap(copy.Width, copy.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp24))
                g.DrawImage(copy, new Rectangle(0, 0, bmp24.Width, bmp24.Height));

            copy.Dispose();
            return bmp24;
        }

        // -----------------------------------------------------------------------
        //  HILFSMETHODE: Wendet ein ColorMatrix-Array auf ein Bitmap an
        //  (optional nur auf nicht-schwarze/weiße Pixel, wenn ignoreBlackWhite=true).
        //  Gibt ein neues Bitmap zurück – das Original bleibt unverändert.
        // -----------------------------------------------------------------------
        private Bitmap ApplyColorMatrix(Bitmap source, float[][] matrix, bool ignoreBlackWhite = false)
        {
            Bitmap result = new Bitmap(source.Width, source.Height);
            ColorMatrix cm = new ColorMatrix(matrix);
            ImageAttributes attr = new ImageAttributes();
            attr.SetColorMatrix(cm);

            using (Graphics g = Graphics.FromImage(result))
            {
                if (ignoreBlackWhite)
                {
                    // Jeden Pixel einzeln prüfen (nur farbige Pixel verändern)
                    for (int y = 0; y < source.Height; y++)
                        for (int x = 0; x < source.Width; x++)
                        {
                            Color px = source.GetPixel(x, y);
                            if (px.ToArgb() != Color.White.ToArgb() && px.ToArgb() != Color.Black.ToArgb())
                                g.DrawImage(source, new Rectangle(x, y, 1, 1), x, y, 1, 1, GraphicsUnit.Pixel, attr);
                            else
                                result.SetPixel(x, y, px); // Schwarz/Weiß unverändert übernehmen
                        }
                }
                else
                {
                    g.DrawImage(source,
                        new Rectangle(0, 0, source.Width, source.Height),
                        0, 0, source.Width, source.Height,
                        GraphicsUnit.Pixel, attr);
                }
            }

            attr.Dispose();
            return result;
        }

        // -----------------------------------------------------------------------
        //  HILFSMETHODE: Speichert den aktuellen Zeichen-Zustand (originalImageDraw)
        //  auf dem Undo-Stack und leert den Redo-Stack.
        // -----------------------------------------------------------------------
        private void PushUndo()
        {
            if (originalImageDraw == null) return;
            previousImages.Push((Bitmap)originalImageDraw.Clone());
            redoImages.Clear();
        }

        // -----------------------------------------------------------------------
        //  HILFSMETHODE: XML-Datei mit den ID-Namen speichern.
        // -----------------------------------------------------------------------
        private void SaveIdNamesXml()
        {
            XDocument doc = new XDocument(
                new XElement("IDNames",
                    idNames.Select(kv =>
                        new XElement("ID",
                            new XAttribute("value", kv.Key),
                            new XAttribute("name", kv.Value))
                    )
                )
            );
            doc.Save(xmlFilePath);
        }

        // =======================================================================
        //  LISTBOX – Befüllen
        // =======================================================================
        #region PopulateListBox
        /// <summary>
        /// Füllt die ListBox mit allen gültigen Gump-IDs.
        /// Wenn showFreeSlots=true, werden auch freie Slots angezeigt.
        /// </summary>
        private void PopulateListBox(bool showFreeSlots = false)
        {
            listBox.BeginUpdate();
            listBox.Items.Clear();

            for (int i = 0; i < Gumps.GetCount(); i++)
            {
                string hex = i.ToString("X");

                if (Gumps.IsValidIndex(i))
                {
                    string name = idNames.ContainsKey(i) ? idNames[i] : "";
                    listBox.Items.Add($"ID: {i} (0x{hex}) - {name}");
                }
                else if (showFreeSlots)
                {
                    listBox.Items.Add($"ID: {i} (0x{hex}) - Free space");
                }
            }

            listBox.EndUpdate();
        }
        #endregion

        // =======================================================================
        //  SUCHE
        // =======================================================================
        #region Search
        /// <summary>
        /// Sucht beim Tippen nach ID (dezimal/hex) oder Name (Partial-Match).
        /// </summary>
        private void SearchByIdToolStripTextBox_TextChanged(object sender, EventArgs e)
        {
            string searchText = searchByIdToolStripTextBox.Text.Trim();
            if (string.IsNullOrEmpty(searchText)) return;

            // -- Numerische Suche (dezimal oder 0x-Hex) --
            if (int.TryParse(searchText, out int id) ||
                (searchText.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
                 int.TryParse(searchText.Substring(2), NumberStyles.HexNumber, null, out id)))
            {
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    string item = listBox.Items[i].ToString();
                    if (item.Contains($"ID: {id} "))
                    {
                        listBox.SelectedIndex = i;
                        return;
                    }
                }
                return;
            }

            // -- Namens-Suche (Partial-Match, Groß-/Kleinschreibung ignorieren) --
            string lower = searchText.ToLowerInvariant();
            for (int i = 0; i < listBox.Items.Count; i++)
            {
                if (listBox.Items[i].ToString().ToLowerInvariant().Contains(lower))
                {
                    listBox.SelectedIndex = i;
                    return;
                }
            }
        }
        #endregion

        // =======================================================================
        //  LISTBOX – Auswahl geändert / Formular laden
        // =======================================================================
        #region Load ListBox
        /// <summary>
        /// Zeigt das Gump-Bild für den gewählten ListBox-Eintrag an
        /// und aktualisiert ID- und Größen-Label.
        /// </summary>
        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex == -1) return;

            int i = GetSelectedId();
            if (i < 0) return;

            if (Gumps.IsValidIndex(i))
            {
                Bitmap bmp = Gumps.GetGump(i);
                if (bmp != null)
                {
                    pictureBox.BackgroundImage = bmp;
                    IDLabel.Text = $"ID: 0x{i:X} ({i})";
                    SizeLabel.Text = $"Size: {bmp.Width},{bmp.Height}";
                }
                else
                {
                    pictureBox.BackgroundImage = null;
                }
            }
            else
            {
                pictureBox.BackgroundImage = null;
            }

            listBox.Invalidate();

            // Titelzeile mit Unsaved-Marker aktualisieren
            UpdateFormTitle();
        }

        /// <summary>
        /// Wird beim ersten Laden des Formulars aufgerufen:
        /// ListBox befüllen und erstes Element auswählen.
        /// </summary>
        private void GumpsEdit_Load(object sender, EventArgs e)
        {
            PopulateListBox();

            if (listBox.Items.Count > 0)
                listBox.SelectedIndex = 0;

            if (Gumps.IsValidIndex(0))
                pictureBox.BackgroundImage = Gumps.GetGump(0);
        }
        #endregion

        // =======================================================================
        //  TITELZEILE
        // =======================================================================
        #region Form Title
        /// <summary>
        /// Zeigt im Fenstertitel ein '*' an, wenn ungespeicherte Änderungen vorliegen.
        /// </summary>
        private void UpdateFormTitle()
        {
            bool changed = Options.ChangedUltimaClass.ContainsKey("Gumps") &&
                           Options.ChangedUltimaClass["Gumps"];
            this.Text = changed ? "Gumps Edit *" : "Gumps Edit";
        }
        #endregion

        // =======================================================================
        //  KOPIEREN (Gump → Zwischenablage)
        // =======================================================================
        #region Copy clipboard
        /// <summary>
        /// Kopiert das aktuell gewählte Gump als 24-bit-Bitmap in die Zwischenablage.
        /// Transparenz-Pixel (#D3D3D3) werden dabei schwarz gezeichnet.
        /// </summary>
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int i = GetSelectedId();
            if (i < 0) { MessageBox.Show("No image to copy!"); return; }

            if (!Gumps.IsValidIndex(i)) { MessageBox.Show("No image to copy!"); return; }

            Bitmap src = Gumps.GetGump(i);
            if (src == null) { MessageBox.Show("No image to copy!"); return; }

            using (Bitmap bmp24 = ConvertGumpForClipboard(src))
            {
                Clipboard.SetImage(bmp24);
                MessageBox.Show("The image has been copied to the clipboard!");
            }
        }
        #endregion

        // =======================================================================
        //  IMPORTIEREN (Zwischenablage → Gump)
        // =======================================================================
        #region Import clipboard
        /// <summary>
        /// Importiert ein Bild aus der Zwischenablage in den gewählten Gump-Slot.
        /// Die Farben #D3D3D3, Schwarz und Weiß werden als transparent behandelt.
        /// </summary>
        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show("No image in the clipboard.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int index = GetSelectedId();
            if (index < 0) { MessageBox.Show("No item selected!"); return; }

            if (index < 0 || index >= Gumps.GetCount())
            {
                MessageBox.Show("Invalid index.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (Bitmap bmp = new Bitmap(Clipboard.GetImage()))
            {
                Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);

                // Farben, die als transparent gelten
                Color[] ignoreColors =
                {
                    Color.FromArgb(211, 211, 211),  // #D3D3D3
                    Color.FromArgb(0,   0,   0),    // Schwarz
                    Color.FromArgb(255, 255, 255)   // Weiß
                };

                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Color px = bmp.GetPixel(x, y);
                        newBmp.SetPixel(x, y, ignoreColors.Contains(px) ? Color.Transparent : px);
                    }

                Gumps.ReplaceGump(index, newBmp);
                ControlEvents.FireGumpChangeEvent(this, index);

                listBox.Invalidate();
                ListBox_SelectedIndexChanged(this, EventArgs.Empty);

                Options.ChangedUltimaClass["Gumps"] = true;
                UpdateFormTitle();
            }

            UpdateTotalIDsLabel();
            UpdateFreeIDsLabel();
        }

        /// <summary>
        /// Tastenkürzel: Strg+V importiert aus Zwischenablage,
        ///               Strg+X kopiert in die Zwischenablage.
        /// </summary>
        private void GumpControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
                importToolStripMenuItem_Click(sender, e);
            else if (e.Control && e.KeyCode == Keys.X)
                copyToolStripMenuItem_Click(sender, e);
        }
        #endregion

        // =======================================================================
        //  ENTFERNEN
        // =======================================================================
        #region Remove
        /// <summary>
        /// Entfernt das aktuell gewählte Gump aus den Daten.
        /// </summary>
        private void OnClickRemove(object sender, EventArgs e)
        {
            int i = GetSelectedId();
            if (i < 0) return;

            if (Gumps.IsValidIndex(i))
            {
                Gumps.RemoveGump(i);
                ControlEvents.FireGumpChangeEvent(this, i);

                pictureBox.Image = null;

                listBox.Invalidate();
                ListBox_SelectedIndexChanged(this, EventArgs.Empty);

                Options.ChangedUltimaClass["Gumps"] = true;
                UpdateFormTitle();
            }

            UpdateTotalIDsLabel();
            UpdateFreeIDsLabel();
        }
        #endregion

        // =======================================================================
        //  ERSETZEN (Datei → Gump)
        // =======================================================================
        #region Replace
        /// <summary>
        /// Ersetzt das gewählte Gump durch eine Bild-Datei (TIF/BMP).
        /// </summary>
        private void OnClickReplace(object sender, EventArgs e)
        {
            if (listBox.SelectedItems.Count != 1) return;

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose image file to replace";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";

                if (dialog.ShowDialog() != DialogResult.OK) return;

                using (var bmpTemp = new Bitmap(dialog.FileName))
                {
                    Bitmap bitmap = new Bitmap(bmpTemp);

                    if (dialog.FileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                        bitmap = Utils.ConvertBmp(bitmap);

                    int i = GetSelectedId();
                    if (i < 0) return;

                    Gumps.ReplaceGump(i, bitmap);
                    ControlEvents.FireGumpChangeEvent(this, i);

                    listBox.Invalidate();
                    ListBox_SelectedIndexChanged(this, EventArgs.Empty);

                    Options.ChangedUltimaClass["Gumps"] = true;
                    UpdateFormTitle();
                }
            }
        }
        #endregion

        // =======================================================================
        //  FREIE SLOTS ANZEIGEN
        // =======================================================================
        #region Show Free Slots
        private bool isShowingFreeSlots = false;

        /// <summary>
        /// Schaltet die Anzeige freier Gump-Slots in der ListBox um.
        /// Aktiver Zustand wird durch grünen Hintergrund des Menüpunktes signalisiert.
        /// </summary>
        private void showFreeIdsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isShowingFreeSlots = !isShowingFreeSlots;
            PopulateListBox(isShowingFreeSlots);
            showFreeIdsToolStripMenuItem.BackColor =
                isShowingFreeSlots ? Color.Green : SystemColors.Control;
        }
        #endregion

        // =======================================================================
        //  NÄCHSTEN FREIEN SLOT FINDEN
        // =======================================================================
        #region Find Next Free ID
        /// <summary>
        /// Springt in der ListBox zum nächsten freien Gump-Slot nach der aktuellen Auswahl.
        /// </summary>
        private void OnClickFindFree(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex == -1) return;

            int id = GetSelectedId();
            if (id < 0) return;
            id++;

            for (int i = listBox.SelectedIndex + 1; i < listBox.Items.Count; i++, id++)
            {
                string item = listBox.Items[i].ToString();
                int start = item.IndexOf("ID: ") + 4;
                int len = item.IndexOf(" (") - start;
                if (start < 4 || len <= 0) continue;

                if (!int.TryParse(item.Substring(start, len), out int itemId)) continue;

                if (id < itemId || (isShowingFreeSlots && !Gumps.IsValidIndex(itemId)))
                {
                    listBox.SelectedIndex = i;
                    return;
                }
            }

            // Kein freier Slot gefunden – am Ende einen neuen Eintrag anfügen (nur bei aktiver Freiansicht)
            if (isShowingFreeSlots)
            {
                int newId = Gumps.GetCount();
                listBox.Items.Add($"ID: {newId} (0x{newId:X})");
                listBox.SelectedIndex = listBox.Items.Count - 1;
            }
        }
        #endregion

        // =======================================================================
        //  ID-NAMEN VERWALTEN (XML)
        // =======================================================================
        #region Add Names ID Gumps XML
        /// <summary>
        /// Weist einer Gump-ID einen Namen zu und speichert ihn in der XML-Datei.
        /// Kann auch programmatisch aufgerufen werden.
        /// </summary>
        public void SetIDName(int id, string name)
        {
            idNames[id] = name;
            SaveIdNamesXml();
        }

        /// <summary>
        /// Öffnet einen kleinen Dialog zum Bearbeiten des Namens für die gewählte ID.
        /// </summary>
        private void addIDNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int id = GetSelectedId();
            if (id < 0) return;

            using (var form = new Form
            {
                Text = "Edit ID Name",
                Width = 320,
                Height = 140,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent
            })
            {
                var idTextBox = new TextBox { Text = id.ToString(), Location = new Point(10, 10), Width = 280 };
                var nameTextBox = new TextBox { Text = idNames.ContainsKey(id) ? idNames[id] : "", Location = new Point(10, 40), Width = 230 };
                var okButton = new Button { Text = "OK", Location = new Point(10, 70), Width = 60 };
                var delButton = new Button { Text = "Delete", Location = new Point(80, 70), Width = 60 };

                delButton.Click += (s, ev) => nameTextBox.Text = "";

                okButton.Click += (s, ev) =>
                {
                    if (int.TryParse(idTextBox.Text, out int newId))
                    {
                        idNames[newId] = nameTextBox.Text;
                        SaveIdNamesXml();
                    }
                    form.DialogResult = DialogResult.OK;
                };

                form.Controls.AddRange(new Control[] { idTextBox, nameTextBox, okButton, delButton });
                form.AcceptButton = okButton;

                if (form.ShowDialog() == DialogResult.OK)
                    PopulateListBox(isShowingFreeSlots);
            }

            UpdateTotalIDsLabel();
            UpdateFreeIDsLabel();
        }
        #endregion

        // =======================================================================
        //  SPEICHERN (MUL-Dateien)
        // =======================================================================
        #region OnClickSave
        /// <summary>
        /// Speichert alle Gump-Änderungen in die MUL/UOP-Ausgabedateien.
        /// Fragt vorher nach Bestätigung.
        /// </summary>
        private void OnClickSave(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure? Will take a while", "Save",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes) return;

            Cursor.Current = Cursors.WaitCursor;
            Gumps.Save(Options.OutputPath);
            Cursor.Current = Cursors.Default;

            Options.ChangedUltimaClass["Gumps"] = false;
            UpdateFormTitle();

            MessageBox.Show($"Saved to {Options.OutputPath}", "Save",
                MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
        #endregion

        // =======================================================================
        //  LABEL-UPDATES (benannte IDs / freie IDs)
        // =======================================================================
        #region ID Name Counter
        /// <summary>
        /// Aktualisiert das Label mit der Anzahl benannter IDs.
        /// </summary>
        private void UpdateTotalIDsLabel()
        {
            IDLabelinortotal.Text = $"Named ID Total: {idNames.Count}";
        }

        /// <summary>
        /// Aktualisiert das Label mit der Anzahl freier (unbenannter) IDs.
        /// </summary>
        private void UpdateFreeIDsLabel()
        {
            int freeIDs = Gumps.GetCount() - idNames.Count;
            LabelFreeIDs.Text = $"Total Free IDs: {freeIDs}";
        }
        #endregion

        // =======================================================================
        //  PAINT – Zustandsvariablen
        // =======================================================================
        #region Paint State Variables
        private bool isDrawing = false;
        private bool isEraserActive = false;   // NEU: Radierer-Modus
        private bool isEyedropper = false;   // NEU: Farbpipette
        private Color drawingColor = Color.Black;
        private Stack<Bitmap> previousImages = new Stack<Bitmap>();
        private Stack<Bitmap> redoImages = new Stack<Bitmap>();
        private Bitmap originalImage;              // Ursprüngliches Bild (unveränderlich)
        private Bitmap baseImage;                  // NEU: Referenz-Bild für Helligkeit/Kontrast
        private Bitmap originalImageDraw;          // Aktuelles Arbeitsbild
        private Bitmap currentImage;               // Temporäres Vorschau-Bild (Trackbars)
        private List<List<Point>> lines = new List<List<Point>>();
        private List<Point> currentLine;
        private bool removeColor = false;
        private bool isSelecting = false;
        private Rectangle selectionRectangle = new Rectangle();
        private System.Drawing.Image loadedImage;
        private bool isSelectingCircle = false;
        private Rectangle selectionCircle = new Rectangle();
        private float zoomFactor = 1.0f;   // NEU: Zoom
        #endregion

        // =======================================================================
        //  BILD IN PAINT-BOX LADEN
        // =======================================================================
        #region LoadImage
        /// <summary>
        /// Lädt ein Bild in die Paint-PictureBox und initialisiert alle Arbeitskopien.
        /// Setzt Pinselgröße zurück auf 2.
        /// </summary>
        private void LoadImageIntopictureBoxDraw(System.Drawing.Image image)
        {
            tbPinselSize.Text = "2";

            if (image == null)
            {
                MessageBox.Show("The image could not be loaded.");
                return;
            }

            originalImage?.Dispose();
            originalImage = new Bitmap(image);

            baseImage?.Dispose();
            baseImage = new Bitmap(originalImage);  // NEU: separate Referenz

            originalImageDraw?.Dispose();
            originalImageDraw = new Bitmap(originalImage);

            pictureBoxDraw.Image = originalImageDraw;

            // Undo/Redo leeren beim Neuladen
            previousImages.Clear();
            redoImages.Clear();
        }
        #endregion

        // =======================================================================
        //  MAUS – BEWEGEN (Zeichnen / Rechteck / Kreis / Eyedropper)
        // =======================================================================
        #region MouseMove
        /// <summary>
        /// Verarbeitet Mausbewegungen: Zeichnen/Radieren im Drawing-Modus,
        /// Aktualisieren der Auswahl-Rechtecke/-Kreise.
        /// Zeigt außerdem die aktuelle Pixelfarbe in der Statusleiste.
        /// </summary>
        private void pictureBoxDraw_MouseMove(object sender, MouseEventArgs e)
        {
            // -- Pixel-Koordinaten auf Bild abbilden --
            if (originalImageDraw != null)
            {
                int offX = (pictureBoxDraw.Width - (int)(originalImageDraw.Width * zoomFactor)) / 2;
                int offY = (pictureBoxDraw.Height - (int)(originalImageDraw.Height * zoomFactor)) / 2;
                int imgX = (int)((e.X - offX) / zoomFactor);
                int imgY = (int)((e.Y - offY) / zoomFactor);

                // Statusleiste mit Mausposition und Pixelfarbe aktualisieren (NEU)
                if (imgX >= 0 && imgX < originalImageDraw.Width &&
                    imgY >= 0 && imgY < originalImageDraw.Height)
                {
                    Color px = originalImageDraw.GetPixel(imgX, imgY);
                    IDLabel.Text = $"X:{imgX} Y:{imgY}  #{px.R:X2}{px.G:X2}{px.B:X2}";
                }

                // -- Zeichnen / Radieren --
                if (e.Button == MouseButtons.Left && isDrawing && currentLine != null)
                {
                    if (imgX >= 0 && imgX < originalImageDraw.Width &&
                        imgY >= 0 && imgY < originalImageDraw.Height)
                    {
                        float pinselSize;
                        if (!float.TryParse(tbPinselSize.Text, out pinselSize) || pinselSize <= 0)
                            pinselSize = 2;

                        Color activeColor = isEraserActive ? Color.Transparent : drawingColor;

                        using (Graphics g = Graphics.FromImage(originalImageDraw))
                            g.FillEllipse(new SolidBrush(activeColor), imgX, imgY, pinselSize, pinselSize);

                        currentLine.Add(new Point(imgX, imgY));
                        pictureBoxDraw.Invalidate();
                    }
                }
            }

            // -- Rechteck-Auswahl aktualisieren --
            if (e.Button == MouseButtons.Left && checkBoxRectangle.Checked)
            {
                selectionRectangle.Width = e.X - selectionRectangle.X;
                selectionRectangle.Height = e.Y - selectionRectangle.Y;
                pictureBoxDraw.Invalidate();
            }

            // -- Kreis-Auswahl aktualisieren --
            if (e.Button == MouseButtons.Left && isSelectingCircle)
            {
                selectionCircle.Width = e.X - selectionCircle.X;
                selectionCircle.Height = e.Y - selectionCircle.Y;
                pictureBoxDraw.Invalidate();
            }
        }
        #endregion

        // =======================================================================
        //  MAUS – DRÜCKEN
        // =======================================================================
        #region MouseDown
        /// <summary>
        /// Initialisiert Auswahl-Rechteck/-Kreis beim Maustaste-Drücken.
        /// Im Zeichenmodus: aktuellen Zustand auf Undo-Stack schieben.
        /// Im Eyedropper-Modus: Farbe aufnehmen.
        /// </summary>
        private void pictureBoxDraw_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isEyedropper && originalImageDraw != null)
            {
                // NEU: Farbpipette – Pixelfarbe aufnehmen
                int offX = (pictureBoxDraw.Width - (int)(originalImageDraw.Width * zoomFactor)) / 2;
                int offY = (pictureBoxDraw.Height - (int)(originalImageDraw.Height * zoomFactor)) / 2;
                int imgX = (int)((e.X - offX) / zoomFactor);
                int imgY = (int)((e.Y - offY) / zoomFactor);

                if (imgX >= 0 && imgX < originalImageDraw.Width &&
                    imgY >= 0 && imgY < originalImageDraw.Height)
                {
                    Color picked = originalImageDraw.GetPixel(imgX, imgY);
                    drawingColor = picked;
                    TextBoxColor.Text = $"{picked.R:X2}{picked.G:X2}{picked.B:X2}";
                }
                return;
            }

            if (e.Button == MouseButtons.Left && checkBoxRectangle.Checked)
            {
                selectionRectangle = new Rectangle(e.X, e.Y, 0, 0);
                isSelecting = true;
            }

            if (e.Button == MouseButtons.Left && isSelectingCircle)
            {
                selectionCircle = new Rectangle(e.X, e.Y, 0, 0);
                isSelecting = true;
            }

            if (e.Button == MouseButtons.Left && isDrawing)
            {
                PushUndo();
                currentLine = new List<Point>();
            }
        }
        #endregion

        // =======================================================================
        //  MAUS – LOSLASSEN
        // =======================================================================
        #region MouseUp
        /// <summary>
        /// Schließt den aktuellen Strich ab und schiebt den Endzustand auf den Undo-Stack.
        /// Beendet die Auswahl-Geste bei Rechteck/Kreis.
        /// </summary>
        private void pictureBoxDraw_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isDrawing && currentLine != null)
            {
                float pinselSize;
                if (!float.TryParse(tbPinselSize.Text, out pinselSize) || pinselSize <= 0)
                    pinselSize = 2;

                Color activeColor = isEraserActive ? Color.Transparent : drawingColor;

                using (Graphics g = Graphics.FromImage(originalImageDraw))
                using (SolidBrush brush = new SolidBrush(activeColor))
                    foreach (Point p in currentLine)
                        g.FillEllipse(brush, p.X, p.Y, pinselSize, pinselSize);

                previousImages.Push((Bitmap)originalImageDraw.Clone());
                currentLine.Clear();
                pictureBoxDraw.Invalidate();
            }

            if (e.Button == MouseButtons.Left && isSelecting)
                isSelecting = false;
        }
        #endregion

        // =======================================================================
        //  MAUS – SCROLL (Zoom)
        // =======================================================================
        #region MouseWheel Zoom
        /// <summary>
        /// NEU: Vergrößert / verkleinert die Zeichenfläche per Mausrad (Faktor 0.1–5.0).
        /// </summary>
        private void pictureBoxDraw_MouseWheel(object sender, MouseEventArgs e)
        {
            zoomFactor += e.Delta > 0 ? 0.1f : -0.1f;
            zoomFactor = Math.Max(0.1f, Math.Min(5.0f, zoomFactor));

            if (originalImageDraw != null)
            {
                int zoomedW = (int)(originalImageDraw.Width * zoomFactor);
                int zoomedH = (int)(originalImageDraw.Height * zoomFactor);
                pictureBoxDraw.Size = new Size(
                    Math.Max(zoomedW + 20, panel2.Width),
                    Math.Max(zoomedH + 20, panel2.Height));
            }

            pictureBoxDraw.Invalidate();
        }
        #endregion

        // =======================================================================
        //  FARBE – TEXTBOX
        // =======================================================================
        #region Textbox Color Change
        /// <summary>
        /// Aktualisiert die Zeichenfarbe, wenn ein neuer Hex-Farbcode eingegeben wird.
        /// </summary>
        private void TextBoxColor_TextChanged(object sender, EventArgs e)
        {
            string colorText = TextBoxColor.Text.TrimStart('#');

            if (colorText.Length == 6 &&
                int.TryParse("FF" + colorText, NumberStyles.HexNumber, null, out int argb))
            {
                drawingColor = Color.FromArgb(argb);
            }
        }
        #endregion

        // =======================================================================
        //  UNDO / REDO
        // =======================================================================
        #region Undo
        /// <summary>
        /// Macht den letzten Zeichenschritt rückgängig (bis zu zwei Schritte auf einmal,
        /// da MouseDown und MouseUp beide einen Zustand pushen).
        /// </summary>
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 2; i++)
            {
                if (previousImages.Count > 0)
                {
                    redoImages.Push(originalImageDraw);
                    originalImageDraw = previousImages.Pop();
                    pictureBoxDraw.Image = originalImageDraw;
                }
            }
        }
        #endregion

        #region Redo
        /// <summary>
        /// Stellt einen rückgängig gemachten Zeichenschritt wieder her.
        /// </summary>
        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (redoImages.Count > 0)
            {
                previousImages.Push(originalImageDraw);
                originalImageDraw = redoImages.Pop();
                pictureBoxDraw.Image = originalImageDraw;
            }
        }
        #endregion

        // =======================================================================
        //  MENÜ-RENDERER (grüner Hintergrund wenn Paint aktiv)
        // =======================================================================
        #region Color Renderer
        private class MyRenderer : ToolStripProfessionalRenderer
        {
            private GumpsEdit _owner;
            public MyRenderer(GumpsEdit owner) { _owner = owner; }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (e.Item == _owner.paintToolStripMenuItem)
                    e.Graphics.FillRectangle(Brushes.Green, e.Item.Bounds);
                else
                    base.OnRenderMenuItemBackground(e);
            }
        }
        #endregion

        // =======================================================================
        //  PAINT-MODUS UMSCHALTEN
        // =======================================================================
        #region Paint Toggle
        /// <summary>
        /// Schaltet den Zeichenmodus ein/aus.
        /// Beim Einschalten wird das aktuelle Bild in die Zeichenfläche geladen.
        /// </summary>
        private void paintToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isDrawing = !isDrawing;

            if (isDrawing)
            {
                contextMenuStripPicturebBox.Renderer = new MyRenderer(this);
                LoadImageIntopictureBoxDraw(pictureBoxDraw.Image);
                MessageBox.Show("Drawing mode enabled. Use mouse wheel to zoom.");
            }
            else
            {
                contextMenuStripPicturebBox.Renderer = new ToolStripProfessionalRenderer();
                MessageBox.Show("Drawing mode disabled.");
            }
        }
        #endregion

        // =======================================================================
        //  NEU: RADIERER UMSCHALTEN
        // =======================================================================
        #region Eraser Toggle
        /// <summary>
        /// NEU: Schaltet den Radierer-Modus um.
        /// Im Radierer-Modus werden Pixel transparent gezeichnet.
        /// </summary>
        private void eraserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isEraserActive = !isEraserActive;
            eraserToolStripMenuItem.Checked = isEraserActive;
        }
        #endregion

        // =======================================================================
        //  NEU: FARBPIPETTE UMSCHALTEN
        // =======================================================================
        #region Eyedropper Toggle
        /// <summary>
        /// NEU: Schaltet die Farbpipette um.
        /// Ein Linksklick auf das Bild übernimmt dann die Pixelfarbe als Zeichenfarbe.
        /// </summary>
        private void eyedropperToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isEyedropper = !isEyedropper;
            eyedropperToolStripMenuItem.Checked = isEyedropper;
        }
        #endregion

        // =======================================================================
        //  BILD AUS ZWISCHENABLAGE IN PAINT-BOX
        // =======================================================================
        #region Import Clipboard to PaintBox
        /// <summary>
        /// Lädt ein Bild aus der Zwischenablage direkt in die Zeichenfläche
        /// (ohne es in den Gump-Slot zu schreiben).
        /// </summary>
        private void importImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show("There is no image in the buffer.");
                return;
            }

            System.Drawing.Image clipboardImage = Clipboard.GetImage();

            originalImage?.Dispose();
            originalImage = new Bitmap(clipboardImage);

            baseImage?.Dispose();
            baseImage = new Bitmap(originalImage);

            originalImageDraw?.Dispose();
            originalImageDraw = new Bitmap(originalImage);

            pictureBoxDraw.Image = originalImageDraw;
        }
        #endregion

        // =======================================================================
        //  FARBDIALOG
        // =======================================================================
        #region Color Dialog
        /// <summary>
        /// Öffnet den Windows-Farbdialog und übernimmt die gewählte Farbe
        /// als Hex-Code in die TextBox.
        /// </summary>
        private void btColorDialog_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog { Color = drawingColor };

            if (dlg.ShowDialog() == DialogResult.OK)
                TextBoxColor.Text = $"{dlg.Color.R:X2}{dlg.Color.G:X2}{dlg.Color.B:X2}";
        }
        #endregion

        // =======================================================================
        //  PINSELGRÖSSE – EINGABE-VALIDIERUNG
        // =======================================================================
        #region Brush Size
        /// <summary>
        /// Erlaubt in der Pinselgröße-TextBox nur Ziffern und Steuertasten.
        /// </summary>
        private void tbPinselSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        /// <summary>
        /// Begrenzt den Pinselgrößen-Wert auf maximal 60.
        /// </summary>
        private void tbPinselSize_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(tbPinselSize.Text, out int value))
            {
                if (value > 60) tbPinselSize.Text = "60";
            }
            else if (tbPinselSize.Text != string.Empty)
            {
                tbPinselSize.Text = "2";
            }
        }
        #endregion

        // =======================================================================
        //  TASTENKÜRZEL (Formular)
        // =======================================================================
        #region GumpsEdit KeyDown
        /// <summary>
        /// Strg+Backspace = Undo, Strg+R = Redo (global für das Formular).
        /// </summary>
        private void GumpsEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Back)
                undoToolStripMenuItem_Click(sender, e);
            else if (e.Control && e.KeyCode == Keys.R)
                redoToolStripMenuItem_Click(sender, e);
        }
        #endregion

        // =======================================================================
        //  PAINT-BOX BILD KOPIEREN
        // =======================================================================
        #region Copy PictureBoxDraw to Clipboard
        /// <summary>
        /// Kopiert das aktuelle Bild der Zeichenfläche in die Zwischenablage.
        /// </summary>
        private void CopyImagePictureBoxDrawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxDraw.Image != null)
                Clipboard.SetImage(pictureBoxDraw.Image);
            else
                MessageBox.Show("There is no image to copy.");
        }
        #endregion

        // =======================================================================
        //  GUMP IN PAINT-BOX SENDEN
        // =======================================================================
        #region Send Image to Paint Box
        /// <summary>
        /// Sendet das gewählte Gump (mit #D3D3D3-zu-Schwarz-Konvertierung)
        /// in die Zeichenfläche des Paint-Tabs.
        /// </summary>
        private void sendImageToPaintBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int i = GetSelectedId();
            if (i < 0) { MessageBox.Show("No image to copy!"); return; }

            if (!Gumps.IsValidIndex(i)) { MessageBox.Show("No image to copy!"); return; }

            Bitmap src = Gumps.GetGump(i);
            if (src == null) { MessageBox.Show("No image to copy!"); return; }

            Bitmap bmp24 = ConvertGumpForClipboard(src);

            baseImage?.Dispose();
            baseImage = new Bitmap(bmp24);

            originalImageDraw?.Dispose();
            originalImageDraw = bmp24;

            pictureBoxDraw.Image = originalImageDraw;

            previousImages.Clear();
            redoImages.Clear();
        }
        #endregion

        // =======================================================================
        //  BILD DREHEN
        // =======================================================================
        #region Rotate Image
        /// <summary>
        /// Dreht das Bild in der Zeichenfläche um 90° gegen den Uhrzeigersinn.
        /// </summary>
        private void rotateImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxDraw.Image == null) return;

            PushUndo();
            originalImageDraw.RotateFlip(RotateFlipType.Rotate270FlipNone);
            pictureBoxDraw.Invalidate();
        }
        #endregion

        // =======================================================================
        //  NEU: BILD HORIZONTAL SPIEGELN
        // =======================================================================
        #region Flip Horizontal
        /// <summary>
        /// NEU: Spiegelt das Bild in der Zeichenfläche horizontal.
        /// </summary>
        private void flipHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (originalImageDraw == null) return;

            PushUndo();
            originalImageDraw.RotateFlip(RotateFlipType.RotateNoneFlipX);
            pictureBoxDraw.Invalidate();
        }
        #endregion

        // =======================================================================
        //  NEU: BILD VERTIKAL SPIEGELN
        // =======================================================================
        #region Flip Vertical
        /// <summary>
        /// NEU: Spiegelt das Bild in der Zeichenfläche vertikal.
        /// </summary>
        private void flipVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (originalImageDraw == null) return;

            PushUndo();
            originalImageDraw.RotateFlip(RotateFlipType.RotateNoneFlipY);
            pictureBoxDraw.Invalidate();
        }
        #endregion

        // =======================================================================
        //  NEU: BATCH-EXPORT (alle Gumps als PNG)
        // =======================================================================
        #region Batch Export
        /// <summary>
        /// NEU: Exportiert alle gültigen Gumps als PNG-Dateien in einen gewählten Ordner.
        /// Dateiname: gump_XXXXX.png
        /// </summary>
        private void exportAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog
            { Description = "Select export folder" })
            {
                if (fbd.ShowDialog() != DialogResult.OK) return;

                Cursor.Current = Cursors.WaitCursor;
                int count = 0;

                for (int i = 0; i < Gumps.GetCount(); i++)
                {
                    if (!Gumps.IsValidIndex(i)) continue;
                    Bitmap bmp = Gumps.GetGump(i);
                    if (bmp == null) continue;

                    string path = Path.Combine(fbd.SelectedPath, $"gump_{i:D5}.png");
                    bmp.Save(path, ImageFormat.Png);
                    count++;
                }

                Cursor.Current = Cursors.Default;
                MessageBox.Show($"Exported {count} gumps to:\n{fbd.SelectedPath}",
                    "Export complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        // =======================================================================
        //  NEU: BATCH-IMPORT (Ordner → Gumps)
        // =======================================================================
        #region Batch Import
        /// <summary>
        /// NEU: Importiert alle BMP/PNG-Dateien aus einem Ordner in aufeinanderfolgende
        /// Gump-Slots, beginnend bei der aktuell gewählten ID.
        /// </summary>
        private void importFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int startId = GetSelectedId();
            if (startId < 0) { MessageBox.Show("Please select a start ID first."); return; }

            using (FolderBrowserDialog fbd = new FolderBrowserDialog
            { Description = "Select folder with images (BMP/PNG)" })
            {
                if (fbd.ShowDialog() != DialogResult.OK) return;

                var files = Directory.GetFiles(fbd.SelectedPath, "*.bmp")
                            .Concat(Directory.GetFiles(fbd.SelectedPath, "*.png"))
                            .OrderBy(f => f)
                            .ToList();

                if (files.Count == 0)
                {
                    MessageBox.Show("No BMP/PNG files found in that folder.");
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;
                int id = startId;

                foreach (string file in files)
                {
                    if (id >= Gumps.GetCount()) break;
                    using (var tmp = new Bitmap(file))
                    {
                        Gumps.ReplaceGump(id, new Bitmap(tmp));
                        ControlEvents.FireGumpChangeEvent(this, id);
                    }
                    id++;
                }

                Cursor.Current = Cursors.Default;

                Options.ChangedUltimaClass["Gumps"] = true;
                UpdateFormTitle();
                PopulateListBox(isShowingFreeSlots);

                MessageBox.Show($"Imported {id - startId} images starting at ID {startId}.",
                    "Import complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            UpdateTotalIDsLabel();
            UpdateFreeIDsLabel();
        }
        #endregion

        // =======================================================================
        //  HELLIGKEIT (TrackBar)
        // =======================================================================
        #region Brightness
        private bool ignoreColors = false;

        /// <summary>
        /// Passt die Helligkeit des Bildes per ColorMatrix an.
        /// Wenn "Ignore color" aktiv ist, werden Schwarz/Weiß nicht verändert.
        /// Arbeitet immer auf baseImage, um Qualitätsverlust durch Mehrfach-Anwendung zu vermeiden.
        /// </summary>
        private void trackBarBrightness_Scroll(object sender, EventArgs e)
        {
            if (baseImage == null) return;

            labelBrightnessValue.Text = trackBarBrightness.Value.ToString();

            float brightness = trackBarBrightness.Value * 0.01f;
            float[][] matrix =
            {
                new float[] { 1, 0, 0, 0, 0 },
                new float[] { 0, 1, 0, 0, 0 },
                new float[] { 0, 0, 1, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { brightness, brightness, brightness, 1, 1 }
            };

            Bitmap result = ApplyColorMatrix(baseImage, matrix, ignoreColors);

            currentImage?.Dispose();
            currentImage = result;
            pictureBoxDraw.Image = currentImage;
        }

        /// <summary>
        /// Steuert, ob die Helligkeitsanpassung Schwarz/Weiß-Pixel auslässt.
        /// </summary>
        private void checkBoxBrightness_CheckedChanged(object sender, EventArgs e)
        {
            ignoreColors = checkBoxBrightness.Checked;
        }
        #endregion

        // =======================================================================
        //  KONTRAST (TrackBar)
        // =======================================================================
        #region Contrast
        private bool ignoreContrastColors = false;

        /// <summary>
        /// Passt den Kontrast des Bildes per ColorMatrix an.
        /// Wenn "Ignore color" aktiv ist, werden Schwarz/Weiß-Pixel nicht verändert.
        /// Arbeitet immer auf baseImage.
        /// </summary>
        private void trackBarContrast_Scroll(object sender, EventArgs e)
        {
            if (baseImage == null) return;

            labelContrastValue.Text = trackBarContrast.Value.ToString();

            if (trackBarContrast.Value == 0)
            {
                pictureBoxDraw.Image = new Bitmap(baseImage);
                return;
            }

            float contrast = 1 + trackBarContrast.Value * 0.02f;
            float translate = 0.5f * (1.0f - contrast);
            float[][] matrix =
            {
                new float[] { contrast, 0, 0, 0, 0 },
                new float[] { 0, contrast, 0, 0, 0 },
                new float[] { 0, 0, contrast, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { translate, translate, translate, 1, 1 }
            };

            Bitmap result = ApplyColorMatrix(baseImage, matrix, ignoreContrastColors);

            currentImage?.Dispose();
            currentImage = result;
            pictureBoxDraw.Image = currentImage;
        }

        /// <summary>
        /// Steuert, ob die Kontrastanpassung Schwarz/Weiß-Pixel auslässt.
        /// </summary>
        private void checkBoxContrast_CheckedChanged(object sender, EventArgs e)
        {
            ignoreContrastColors = checkBoxContrast.Checked;
        }
        #endregion

        // =======================================================================
        //  RGB-KANÄLE (TrackBars)
        // =======================================================================
        #region RGB Color Channels

        /// <summary>
        /// Passt den Rot-Kanal per ColorMatrix an.
        /// Arbeitet auf baseImage, um akkumulierten Qualitätsverlust zu verhindern.
        /// </summary>
        private void trackBarColorR_Scroll(object sender, EventArgs e)
        {
            if (baseImage == null) return;

            labelColorRValue.Text = trackBarColorR.Value.ToString();

            float scale = trackBarColorR.Value * 0.01f;
            float[][] matrix =
            {
                new float[] { 1 + scale, 0, 0, 0, 0 },
                new float[] { 0, 1, 0, 0, 0 },
                new float[] { 0, 0, 1, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { 0, 0, 0, 0, 1 }
            };

            var old = pictureBoxDraw.Image;
            pictureBoxDraw.Image = ApplyColorMatrix(baseImage, matrix);
            if (old != null && old != baseImage && old != originalImageDraw) old.Dispose();
        }

        /// <summary>
        /// Passt den Grün-Kanal per ColorMatrix an. Arbeitet auf baseImage.
        /// </summary>
        private void trackBarColorG_Scroll(object sender, EventArgs e)
        {
            if (baseImage == null) return;

            labelColorGValue.Text = trackBarColorG.Value.ToString();

            float scale = trackBarColorG.Value * 0.01f;
            float[][] matrix =
            {
                new float[] { 1, 0, 0, 0, 0 },
                new float[] { 0, 1 + scale, 0, 0, 0 },
                new float[] { 0, 0, 1, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { 0, 0, 0, 0, 1 }
            };

            var old = pictureBoxDraw.Image;
            pictureBoxDraw.Image = ApplyColorMatrix(baseImage, matrix);
            if (old != null && old != baseImage && old != originalImageDraw) old.Dispose();
        }

        /// <summary>
        /// Passt den Blau-Kanal per ColorMatrix an. Arbeitet auf baseImage.
        /// </summary>
        private void trackBarColorB_Scroll(object sender, EventArgs e)
        {
            if (baseImage == null) return;

            labelColorBValue.Text = trackBarColorB.Value.ToString();

            float scale = trackBarColorB.Value * 0.01f;
            float[][] matrix =
            {
                new float[] { 1, 0, 0, 0, 0 },
                new float[] { 0, 1, 0, 0, 0 },
                new float[] { 0, 0, 1 + scale, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { 0, 0, 0, 0, 1 }
            };

            var old = pictureBoxDraw.Image;
            pictureBoxDraw.Image = ApplyColorMatrix(baseImage, matrix);
            if (old != null && old != baseImage && old != originalImageDraw) old.Dispose();
        }
        #endregion

        // =======================================================================
        //  RGB-RESET
        // =======================================================================
        #region Reset RGB
        /// <summary>
        /// Setzt alle drei RGB-TrackBars auf 0 zurück und stellt das Originalbild wieder her.
        /// </summary>
        private void resetButtonRGB_Click(object sender, EventArgs e)
        {
            trackBarColorR.Value = 0;
            trackBarColorG.Value = 0;
            trackBarColorB.Value = 0;

            labelColorRValue.Text = "0";
            labelColorGValue.Text = "0";
            labelColorBValue.Text = "0";

            trackBarColorR_Scroll(sender, e);
            trackBarColorG_Scroll(sender, e);
            trackBarColorB_Scroll(sender, e);
        }
        #endregion

        // =======================================================================
        //  FARBE ENTFERNEN (Schwarz/Weiß → Transparent)
        // =======================================================================
        #region Remove Color Black/White
        /// <summary>
        /// Wenn aktiv, werden alle schwarzen und weißen Pixel transparent gemacht.
        /// Nützlich, um Ränder nach einer Bearbeitung zu bereinigen.
        /// </summary>
        private void checkBoxRemoveColor_CheckedChanged(object sender, EventArgs e)
        {
            removeColor = checkBoxRemoveColor.Checked;
            if (originalImageDraw == null) return;

            Bitmap tempBmp = new Bitmap(originalImageDraw);

            if (removeColor)
            {
                for (int y = 0; y < tempBmp.Height; y++)
                    for (int x = 0; x < tempBmp.Width; x++)
                    {
                        Color px = tempBmp.GetPixel(x, y);
                        if (px.ToArgb() == Color.Black.ToArgb() ||
                            px.ToArgb() == Color.White.ToArgb())
                            tempBmp.SetPixel(x, y, Color.Transparent);
                    }
            }

            SetPictureBoxImage(pictureBoxDraw, tempBmp);
        }
        #endregion

        // =======================================================================
        //  CHECKBOXEN: RECHTECK / KREIS
        // =======================================================================
        #region Selection Shape Checkboxes
        /// <summary>
        /// Aktiviert die rechteckige Auswahlgeste (deaktiviert Kreis).
        /// </summary>
        private void checkBoxRectangle_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxRectangle.Checked)
                checkBoxDrawCircle.Checked = false;

            isSelecting = checkBoxRectangle.Checked;
        }

        /// <summary>
        /// Aktiviert die kreisförmige Auswahlgeste (deaktiviert Rechteck).
        /// </summary>
        private void checkBoxDrawCircle_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDrawCircle.Checked)
                checkBoxRectangle.Checked = false;

            isSelectingCircle = checkBoxDrawCircle.Checked;
        }
        #endregion

        // =======================================================================
        //  PAINT-EVENT: Auswahl-Overlays
        // =======================================================================
        #region PictureBox Paint
        /// <summary>
        /// Zeichnet die gestrichelten gelben Auswahl-Rahmen (Rechteck oder Kreis)
        /// über das Bild in der Zeichenfläche.
        /// </summary>
        private void pictureBoxDraw_Paint(object sender, PaintEventArgs e)
        {
            if (checkBoxRectangle.Checked)
            {
                using (Pen pen = new Pen(Color.Yellow) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                    e.Graphics.DrawRectangle(pen, selectionRectangle);
            }

            if (isSelectingCircle)
            {
                using (Pen pen = new Pen(Color.Yellow) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                    e.Graphics.DrawEllipse(pen, selectionCircle);
            }
        }
        #endregion

        // =======================================================================
        //  TEXTUR-FILL (Rechteck oder Kreis mit Bild füllen)
        // =======================================================================
        #region Texture Fill
        /// <summary>
        /// Füllt den markierten Bereich (Rechteck oder Kreis) mit einer geladenen
        /// Textur-Datei. Schwarze und weiße Pixel der Textur werden übersprungen.
        /// </summary>
        private void SelectingRectangleCircleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog
            { Filter = "Image files|*.bmp;*.jpg;*.jpeg;*.png" })
            {
                if (ofd.ShowDialog() != DialogResult.OK) return;

                loadedImage = new Bitmap(System.Drawing.Image.FromFile(ofd.FileName));

                Bitmap scaledImage = isSelectingCircle
                    ? new Bitmap(loadedImage, selectionCircle.Size)
                    : new Bitmap(loadedImage, selectionRectangle.Size);

                PushUndo();

                using (Graphics g = Graphics.FromImage(originalImageDraw))
                {
                    for (int y = 0; y < scaledImage.Height; y++)
                        for (int x = 0; x < scaledImage.Width; x++)
                        {
                            Color px = scaledImage.GetPixel(x, y);
                            if (px == Color.FromArgb(255, 0, 0, 0) ||
                                px == Color.FromArgb(255, 255, 255, 255))
                                continue;

                            int drawX = isSelectingCircle
                                ? selectionCircle.X + x
                                : selectionRectangle.X + x;
                            int drawY = isSelectingCircle
                                ? selectionCircle.Y + y
                                : selectionRectangle.Y + y;

                            // Im Kreis-Modus: nur Pixel innerhalb der Ellipse zeichnen
                            if (isSelectingCircle)
                            {
                                double dx = x - selectionCircle.Width / 2.0;
                                double dy = y - selectionCircle.Height / 2.0;
                                if (dx * dx / Math.Pow(selectionCircle.Width / 2.0, 2) +
                                    dy * dy / Math.Pow(selectionCircle.Height / 2.0, 2) > 1)
                                    continue;
                            }

                            int imgX = drawX - (pictureBoxDraw.Width - originalImageDraw.Width) / 2;
                            int imgY = drawY - (pictureBoxDraw.Height - originalImageDraw.Height) / 2;

                            if (imgX >= 0 && imgX < originalImageDraw.Width &&
                                imgY >= 0 && imgY < originalImageDraw.Height)
                                g.FillRectangle(new SolidBrush(px), imgX, imgY, 1, 1);
                        }
                }

                pictureBoxDraw.Image = originalImageDraw;
                pictureBoxDraw.Invalidate();

                if (!checkBoxRectangle.Checked) { isSelecting = false; selectionRectangle = new Rectangle(); }
                if (!checkBoxDrawCircle.Checked) { isSelectingCircle = false; selectionCircle = new Rectangle(); }
            }
        }
        #endregion

        // =======================================================================
        //  BILD SPEICHERN (aus Zeichenfläche)
        // =======================================================================
        #region Save Image from PaintBox
        /// <summary>
        /// Speichert das aktuelle Bild der Zeichenfläche als BMP, TIFF, PNG oder JPG.
        /// Standard-Format: PNG (verlustfrei).
        /// </summary>
        private void saveToolStripMenuItemPictureBoxDraw_Click(object sender, EventArgs e)
        {
            if (pictureBoxDraw.Image == null)
            {
                MessageBox.Show("There is no image to save.");
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|Bitmap Image|*.bmp|TIFF Image|*.tiff|JPEG Image|*.jpg",
                Title = "Save image as...",
                FilterIndex = 1  // PNG als Standard (verlustfrei)
            })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;

                ImageFormat fmt = sfd.FilterIndex switch
                {
                    1 => ImageFormat.Png,
                    2 => ImageFormat.Bmp,
                    3 => ImageFormat.Tiff,
                    4 => ImageFormat.Jpeg,
                    _ => ImageFormat.Png
                };

                pictureBoxDraw.Image.Save(sfd.FileName, fmt);
            }
        }
        #endregion
    }
}
// /***************************************************************************
//  *
//  * $Author: Turley
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class ScriptCreator : Form
    {
        public ScriptCreator()
        {
            InitializeComponent();

            this.textBoxBookTitle.TextChanged += new System.EventHandler(this.textBoxBookTitle_TextChanged);
            this.textBoxBookAuthor.TextChanged += new System.EventHandler(this.textBoxBookAuthor_TextChanged);

            // Erstellen Sie eine ImageList und fügen Sie Ihre Bilder hinzu
            ImageList imageList = new ImageList();
            imageList.Images.Add("0x1C11", Properties.Resources._0x1C11); // 1
            imageList.Images.Add("0x1C13", Properties.Resources._0x1C13); // 2
            imageList.Images.Add("0x1E20", Properties.Resources._0x1E20); // 3
            imageList.Images.Add("0x2D9D", Properties.Resources._0x2D9D); // 4
            imageList.Images.Add("0x22C5", Properties.Resources._0x22C5); // 5
            imageList.Images.Add("0x23A0", Properties.Resources._0x23A0); // 6
            imageList.Images.Add("0x225A", Properties.Resources._0x225A); // 7
            imageList.Images.Add("0x238C", Properties.Resources._0x238C); // 8
            imageList.Images.Add("0x2252", Properties.Resources._0x2252); // 9
            imageList.Images.Add("0x2253", Properties.Resources._0x2253); // 10
            imageList.Images.Add("0x2254", Properties.Resources._0x2254); // 11
            imageList.Images.Add("0x2259", Properties.Resources._0x2259); // 12
            imageList.Images.Add("0xE3B", Properties.Resources._0xE3B); // 13
            imageList.Images.Add("0xFBE", Properties.Resources._0xFBE); // 14
            imageList.Images.Add("0xFEF", Properties.Resources._0xFEF); // 15
            imageList.Images.Add("0xFF0", Properties.Resources._0xFF0); // 16
            imageList.Images.Add("0xFF1", Properties.Resources._0xFF1); // 17
            imageList.Images.Add("0xFF2", Properties.Resources._0xFF2); // 18
            imageList.Images.Add("0xFF3", Properties.Resources._0xFF3); // 19
            imageList.Images.Add("0xFF4", Properties.Resources._0xFF4); // 20


            // Fügen Sie hier weitere Bilder hinzu...

            // Füllen Sie die comboBoxBookGrafic mit den Bildern
            foreach (string key in imageList.Images.Keys)
            {
                comboBoxBookGrafic.Items.Add(key);
            }

            // Setzen Sie das DrawItem-Ereignis
            comboBoxBookGrafic.DrawMode = DrawMode.OwnerDrawFixed;
            comboBoxBookGrafic.DrawItem += (sender, e) =>
            {
                e.DrawBackground();
                e.Graphics.DrawImage(imageList.Images[e.Index], e.Bounds.Left, e.Bounds.Top);
                e.Graphics.DrawString(comboBoxBookGrafic.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left + imageList.ImageSize.Width, e.Bounds.Top);
                e.DrawFocusRectangle();
            };

            // Setzen Sie das SelectedIndexChanged-Ereignis
            this.comboBoxBookGrafic.SelectedIndexChanged += new System.EventHandler(this.comboBoxBookGrafic_SelectedIndexChanged);

            // Füllen Sie die comboBoxEnableAutoformat mit den Optionen
            comboBoxEnableAutoformat.Items.Add("Enable Autoformat");
            comboBoxEnableAutoformat.Items.Add("Disable Autoformat");

            // Setzen Sie das SelectedIndexChanged-Ereignis
            this.comboBoxEnableAutoformat.SelectedIndexChanged += new System.EventHandler(this.comboBoxEnableAutoformat_SelectedIndexChanged);
        }

        /*private void buttonCreateBookScript_Click(object sender, EventArgs e)
        {
            string bookTitle = textBoxBookTitle.Text;
            string bookAuthor = textBoxBookAuthor.Text;
            string bookFilename = textBoxBookFilename.Text.Replace(" ", ""); // Entfernt Leerzeichen aus dem Klassennamen
            string grafikID = textBoxGrafikID.Text;
            string coverHue = textBoxCoverHue.Text;
            bool enableAutoFormat = comboBoxEnableAutoformat.SelectedItem.ToString() == "Enable Autoformat";
            string bookContent = textBoxBookContent.Text;

            // Autoformatierung des Inhalts, falls aktiviert
            if (enableAutoFormat)
            {
                // Implementiere hier die Autoformatierung des Inhalts
            }

            // Aufteilung des Inhalts in Seiten
            List<string> pageContents = SplitContentIntoPages(bookContent, 8); // Angenommen, 8 Zeilen pro Seite

            // Erstellung der BookPageInfo Objekte für jede Seite
            StringBuilder bookPageInfos = new StringBuilder();
            foreach (string pageContent in pageContents)
            {
                string formattedPageContent = FormatPageContentForBookPageInfo(pageContent);
                bookPageInfos.AppendLine(formattedPageContent);
            }

            // Generierung des Skripts mit den BookPageInfo Objekten
            string script = GenerateBookScript(bookTitle, bookAuthor, bookFilename, grafikID, coverHue, bookPageInfos.ToString());
            textBoxScriptResult.Text = script;
        }*/

        private void buttonCreateBookScript_Click(object sender, EventArgs e)
        {
            string bookTitle = textBoxBookTitle.Text;
            string bookAuthor = textBoxBookAuthor.Text;
            string bookFilename = textBoxBookFilename.Text.Replace(" ", ""); // Entfernt Leerzeichen aus dem Klassennamen
            string grafikID = textBoxGrafikID.Text;
            string coverHue = textBoxCoverHue.Text;
            bool enableAutoFormat = comboBoxEnableAutoformat.SelectedItem.ToString() == "Enable Autoformat";
            string bookContent = textBoxBookContent.Text;

            // Autoformatierung des Inhalts, falls aktiviert
            if (enableAutoFormat)
            {
                // Ersetzen Sie alle Zeilenumbrüche durch ein Leerzeichen
                bookContent = bookContent.Replace("\n", " ").Replace("\r", " ");

                // Führen Sie einen rekursiven Scan durch, um mehrere Leerzeichen zu entfernen
                while (bookContent.Contains("  "))
                {
                    bookContent = bookContent.Replace("  ", " ");
                }

                // Entfernen Sie Zeilenumbrüche und Leerzeichen am Anfang
                bookContent = bookContent.TrimStart();
            }

            // Aufteilung des Inhalts in Seiten
            List<string> pageContents = SplitContentIntoPages(bookContent, 8); // Angenommen, 8 Zeilen pro Seite

            // Erstellung der BookPageInfo Objekte für jede Seite
            StringBuilder bookPageInfos = new StringBuilder();
            foreach (string pageContent in pageContents)
            {
                string formattedPageContent = FormatPageContentForBookPageInfo(pageContent);
                bookPageInfos.AppendLine(formattedPageContent);
            }

            // Generierung des Skripts mit den BookPageInfo Objekten
            string script = GenerateBookScript(bookTitle, bookAuthor, bookFilename, grafikID, coverHue, bookPageInfos.ToString());
            textBoxScriptResult.Text = script;
        }


        private List<string> SplitContentIntoPages(string content, int maxLinesPerPage)
        {
            string[] lines = content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            List<string> pages = new List<string>();
            StringBuilder currentPage = new StringBuilder();
            int lineCount = 0;

            foreach (string line in lines)
            {
                currentPage.AppendLine(line);
                lineCount++;

                if (lineCount == maxLinesPerPage)
                {
                    pages.Add(currentPage.ToString());
                    currentPage.Clear();
                    lineCount = 0;
                }
            }

            if (currentPage.Length > 0) // Füge die letzte Seite hinzu, falls vorhanden
            {
                pages.Add(currentPage.ToString());
            }

            return pages;
        }

        private string FormatPageContentForBookPageInfo(string pageContent)
        {
            string[] lines = pageContent.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string formattedLines = string.Join(",\n                ", lines.Select(line => $"\"{line.Trim()}\""));
            return $@"new BookPageInfo
            (
                {formattedLines}
            ),";
        }

        private string GenerateBookScript(string title, string author, string filename, string graphicID, string coverHue, string pageInfos)
        {
            return $@"
            using System;
            using Server;
            using Server.Items;

            namespace Server.Items
            {{
                public class {filename} : BaseBook
            {{
                public static readonly BookContent Content = new BookContent
                (
                    ""{title}"", ""{author}"",
                    {pageInfos.TrimEnd(',', '\n', '\r')}
                );

                public override BookContent DefaultContent{{ get{{ return Content; }} }}

                [Constructable]
                public {filename}() : base( {graphicID}, false )
                {{
                    Hue = {coverHue};
                }}

                public {filename}(Serial serial) : base(serial)
                {{
                }}

                public override void Serialize(GenericWriter writer)
                {{
                    base.Serialize(writer);
                    writer.WriteEncodedInt((int)0);
                }}

                public override void Deserialize(GenericReader reader)
                {{
                    base.Deserialize(reader);
                    int version = reader.ReadEncodedInt();
                }}
            }}
        }}";
        }

        private void textBoxBookTitle_TextChanged(object sender, EventArgs e)
        {
            UpdateBookFilename();
        }

        private void textBoxBookAuthor_TextChanged(object sender, EventArgs e)
        {
            UpdateBookFilename();
        }

        private void UpdateBookFilename()
        {
            string bookTitle = textBoxBookTitle.Text.Replace(" ", "_"); // Ersetzt Leerzeichen durch Unterstriche
            string bookAuthor = textBoxBookAuthor.Text.Replace(" ", "_"); // Ersetzt Leerzeichen durch Unterstriche

            // Erstellen Sie den Klassennamen basierend auf dem Buchtitel und dem Autor
            string className = "CBook" + bookTitle + bookAuthor;

            // Aktualisieren Sie die textBoxBookFilename
            textBoxBookFilename.Text = className;
        }


        private void comboBoxBookGrafic_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Setzen Sie den Inhalt der textBoxGrafikID auf die ausgewählte Hexadresse
            textBoxGrafikID.Text = comboBoxBookGrafic.SelectedItem.ToString();
        }

        private void comboBoxEnableAutoformat_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Überprüfen Sie, ob Autoformat aktiviert ist
            bool enableAutoFormat = comboBoxEnableAutoformat.SelectedItem.ToString() == "Enable Autoformat" ? true : false;

            if (enableAutoFormat)
            {
                // Ersetzen Sie alle Zeilenumbrüche durch ein Leerzeichen
                textBoxBookContent.Text = textBoxBookContent.Text.Replace("\n", " ").Replace("\r", " ");

                // Führen Sie einen rekursiven Scan durch, um mehrere Leerzeichen zu entfernen
                while (textBoxBookContent.Text.Contains("  "))
                {
                    textBoxBookContent.Text = textBoxBookContent.Text.Replace("  ", " ");
                }

                // Entfernen Sie Zeilenumbrüche und Leerzeichen am Anfang
                textBoxBookContent.Text = textBoxBookContent.Text.TrimStart();
            }
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            // Füllen Sie die TextBoxen und ComboBoxen mit Testdaten
            textBoxBookTitle.Text = "TestTitle";
            textBoxBookAuthor.Text = "TestAuthor";
            textBoxBookFilename.Text = "CBookTestTitleTestAuthor";
            comboBoxBookGrafic.SelectedIndex = 0; // Wählen Sie das erste Element in der ComboBox aus
            textBoxGrafikID.Text = "0x1C11";
            textBoxCoverHue.Text = "684";
            comboBoxEnableAutoformat.SelectedIndex = 0; // Wählen Sie das erste Element in der ComboBox aus
            textBoxBookContent.Text = "Das ist ein Testinhalt.";
        }

        public class DataGridViewColorCell : DataGridViewTextBoxCell
        {
            protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
            {
                Color color = Color.White;
                if (value is Color)
                {
                    color = (Color)value;
                }

                using (Brush brush = new SolidBrush(color))
                {
                    graphics.FillRectangle(brush, cellBounds);
                }
            }
        }

        private void btColorSet_Click(object sender, EventArgs e)
        {
            // Erstellen Sie ein neues Formular
            Form colorForm = new Form();
            colorForm.Text = "Wählen Sie eine Farbe";
            colorForm.Size = new Size(800, 600);  // Setzen Sie die Größe des Formulars
            colorForm.FormBorderStyle = FormBorderStyle.FixedDialog;  // Deaktivieren Sie die Größenänderung des Formulars

            // Erstellen Sie ein neues DataGridView
            DataGridView dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.EnableHeadersVisualStyles = false;  // Fügen Sie diese Zeile hinzu
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;  // Setzen Sie die Spaltenbreite so, dass sie den verfügbaren Platz ausfüllt
            colorForm.Controls.Add(dgv);

            // Fügen Sie eine Spalte für die Hue-Werte und eine für die Farben hinzu
            dgv.Columns.Add("Hue", "Hue");
            dgv.Columns.Add("Color", "Color");

            // Fügen Sie eine Spalte für die Farbvorschau hinzu
            DataGridViewColumn colorPreviewColumn = new DataGridViewColumn(new DataGridViewColorCell());
            colorPreviewColumn.Name = "ColorPreview";
            dgv.Columns.Add(colorPreviewColumn);

            // Füllen Sie das DataGridView mit allen Farben im 16-Bit-555-Farbschema
            for (int hue = 1; hue <= 32768; hue++)
            {
                Color color = HueToRGB555(hue);
                dgv.Rows.Add(hue, color.Name, color);
            }

            // Wenn der Benutzer auf eine Zeile klickt, kopieren Sie den Hue-Wert in die textBoxCoverHue
            dgv.CellClick += (s, args) =>
            {
                if (args.RowIndex >= 0)
                {
                    textBoxCoverHue.Text = dgv.Rows[args.RowIndex].Cells["Hue"].Value.ToString();
                    int hue = int.Parse(dgv.Rows[args.RowIndex].Cells["Hue"].Value.ToString());
                    textBoxCoverHue.BackColor = HueToRGB555(hue);
                }
            };

            // Zeigen Sie das Formular an
            colorForm.ShowDialog();
        }
        private Color HueToRGB555(int hue)
        {
            // Teilen Sie den Hue-Wert in drei Teile
            int r = hue % 32;
            int g = (hue / 32) % 32;
            int b = (hue / 1024) % 32;

            // Skalieren Sie die Werte auf den Bereich von 0 bis 255
            r = (r << 3) | (r >> 2);
            g = (g << 3) | (g >> 2);
            b = (b << 3) | (b >> 2);

            return Color.FromArgb(r, g, b);
        }


        private void textBoxCoverHue_TextChanged(object sender, EventArgs e)
        {
            // Überprüfen Sie, ob textBoxCoverHue.Text einen Wert enthält
            if (!string.IsNullOrEmpty(textBoxCoverHue.Text))
            {
                // Versuchen Sie, den Wert zu parsen
                int hue;
                if (int.TryParse(textBoxCoverHue.Text, out hue))
                {
                    // Konvertieren Sie den Hue-Wert in eine Farbe
                    Color color = HueToRGB555(hue);

                    // Färben Sie den Hintergrund der textBoxCoverHue entsprechend ein
                    textBoxCoverHue.BackColor = color;
                }
                else
                {
                    // Der Wert konnte nicht geparst werden
                    MessageBox.Show("Bitte geben Sie eine gültige Zahl ein.");
                }
            }
            else
            {
                // textBoxCoverHue.Text ist leer
                textBoxCoverHue.BackColor = SystemColors.Window; // Setzen Sie den Hintergrund auf die Standardfarbe zurück
            }
        }

        private void btClipBoard_Click(object sender, EventArgs e)
        {
            // Überprüfen Sie, ob textBoxScriptResult.Text einen Wert hat
            if (!string.IsNullOrEmpty(textBoxScriptResult.Text))
            {
                // Kopieren Sie den Inhalt von textBoxScriptResult in die Zwischenablage
                Clipboard.SetText(textBoxScriptResult.Text);
            }
            else
            {
                // Zeigen Sie eine Nachricht an, wenn textBoxScriptResult.Text leer ist
                MessageBox.Show("Es gibt keinen Text zum Kopieren in die Zwischenablage.");
            }
        }


        private void btSaveBookScript_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "C# Dateien (*.cs)|*.cs";
            saveFileDialog.FileName = textBoxBookFilename.Text;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(saveFileDialog.FileName, textBoxScriptResult.Text);
            }
        }

    }
}

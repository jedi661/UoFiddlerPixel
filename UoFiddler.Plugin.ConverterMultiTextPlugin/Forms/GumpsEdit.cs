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

            this.Load += GumpsEdit_Load; // Add the Load event            

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
            listBox.BeginUpdate(); // Disable ListBox updates

            for (int i = 0; i < Gumps.GetCount(); i++)
            {
                string hexValue = i.ToString("X"); // Converts the ID to a hexadecimal string
                if (Gumps.IsValidIndex(i))
                {
                    string idName = idNames.ContainsKey(i) ? idNames[i] : "";
                    listBox.Items.Add($"ID: {i} (0x{hexValue}) - {idName}"); // Adds both the decimal and hexadecimal representations of the ID to the ListBox
                }
                else if (showFreeSlots)
                {
                    listBox.Items.Add($"ID: {i} (0x{hexValue}) - Free space");
                }
            }

            listBox.EndUpdate(); // Enable ListBox updates
        }
        #endregion

        #region Search
        private void SearchByIdToolStripTextBox_TextChanged(object sender, EventArgs e)
        {
            string searchText = searchByIdToolStripTextBox.Text.Trim();
            int id;

            // Try to interpret the search text as an integer
            if (int.TryParse(searchText, out id) ||
                (searchText.StartsWith("0x") && int.TryParse(searchText.Substring(2), NumberStyles.HexNumber, null, out id)))
            {
                // Search the ListBox for the item with the corresponding ID
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    string item = listBox.Items[i].ToString();
                    if (item.Contains($"ID: {id}"))
                    {
                        // Select the found item and end the search
                        listBox.SelectedIndex = i;
                        break;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(searchText) && !searchText.All(char.IsDigit))
            {
                // If the search text is not an integer and contains at least one character that is not a digit,
                // search for the name in idNames
                var matchingIds = idNames.Where(kv => kv.Value.Equals(searchText)).Select(kv => kv.Key).ToList();
                if (matchingIds.Count > 0)
                {
                    // Search the ListBox for the item with the corresponding name
                    for (int i = 0; i < listBox.Items.Count; i++)
                    {
                        string item = listBox.Items[i].ToString();
                        if (item.Contains(searchText))
                        {
                            // Select the found item and end the search
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
            // Check if an item in the ListBox is selected
            if (listBox.SelectedIndex == -1)
            {
                return;
            }

            // Get the selected item in the ListBox and convert it to a string
            string selectedItem = listBox.Items[listBox.SelectedIndex].ToString();

            // Find the starting index and length of the integer value in the string
            int startIdx = selectedItem.IndexOf("ID: ") + 4;
            int length = selectedItem.IndexOf(" (") - startIdx;

            // Extract the integer value from the string
            string intValueStr = selectedItem.Substring(startIdx, length);

            // Convert the integer value to an integer
            int i = int.Parse(intValueStr);

            // Überprüfen Sie, ob die ID gültig ist
            if (Gumps.IsValidIndex(i))
            {
                // Get the Gump for the ID
                Bitmap bmp = Gumps.GetGump(i);

                // Check if the Gump is present
                if (bmp != null)
                {
                    // Set the PictureBox's background image to the Gump
                    pictureBox.BackgroundImage = bmp;

                    // Update the labels with the Gump's ID and size
                    IDLabel.Text = $"ID: 0x{i:X} ({i})";
                    SizeLabel.Text = $"Size: {bmp.Width},{bmp.Height}";
                }
                else
                {
                    // Set the PictureBox's background image to null if the gump is not present
                    pictureBox.BackgroundImage = null;
                }
            }
            else
            {
                // Set the PictureBox's background image to null if the ID is invalid
                pictureBox.BackgroundImage = null;
            }

            // Update the ListBox
            listBox.Invalidate();
        }
        private void GumpsEdit_Load(object sender, EventArgs e)
        {
            PopulateListBox(); // Call the method to fill the ListBox

            // Select index 0 in the ListBox
            if (listBox.Items.Count > 0)
            {
                listBox.SelectedIndex = 0;
            }

            // Display the corresponding image in the PictureBox
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
                int startIdx = selectedItem.IndexOf("ID: ") + 4; // Starting index of the integer value
                int length = selectedItem.IndexOf(" (") - startIdx; //Length of the integer value
                string intValueStr = selectedItem.Substring(startIdx, length); // Extract the integer value
                int i = int.Parse(intValueStr);

                //int i = int.Parse(listBox.Items[listBox.SelectedIndex].ToString());
                if (Gumps.IsValidIndex(i))
                {
                    Bitmap originalBmp = Gumps.GetGump(i);
                    if (originalBmp != null)
                    {
                        // Make a copy of the original image
                        Bitmap bmp = new Bitmap(originalBmp);

                        // Color change function built in
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
                    // Check if an item in the ListBox is selected
                    if (listBox.SelectedIndex != -1)
                    {
                        // Get the selected item in the ListBox and convert it to a string
                        string selectedItem = listBox.Items[listBox.SelectedIndex].ToString();

                        // Find the starting index and length of the integer value in the string
                        int startIdx = selectedItem.IndexOf('(') + 3; // Starting index of the integer value
                        int length = selectedItem.IndexOf(')') - startIdx; // Length of the integer value

                        // Extract the integer value from the string
                        string intValueStr = selectedItem.Substring(startIdx, length);

                        // Convert the integer value to an integer
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
                int startIdx = selectedItem.IndexOf("ID: ") + 4; // Starting index of the integer value
                int length = selectedItem.IndexOf(" (") - startIdx; // Length of the integer value
                string intValueStr = selectedItem.Substring(startIdx, length); // Extract the integer value

                int i = int.Parse(intValueStr);

                if (Gumps.IsValidIndex(i))
                {
                    // Remove the graphic
                    Gumps.RemoveGump(i);
                    ControlEvents.FireGumpChangeEvent(this, i);

                    // Remove the image from the PictureBox
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

        private bool isShowingFreeSlots = false; // Add state variable
        private void showFreeIdsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox.Items.Clear(); // First delete all existing items in the ListBox

            if (!isShowingFreeSlots)
            {
                // When the button is clicked for the first time, show all IDs and color the background green
                PopulateListBox(true); // Then call the method to fill the ListBox
                showFreeIdsToolStripMenuItem.BackColor = Color.Green;
            }
            else
            {
                // When the button is clicked again, show only the valid IDs and reset the background color
                PopulateListBox();
                showFreeIdsToolStripMenuItem.BackColor = SystemColors.Control;
            }

            // Toggle the state
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

                // If no empty ID was found and isShowingFreeSlots is enabled,
                // a new ID is added to the end of the ListBox
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
            // Add or update the name to the ID in the dictionary
            idNames[id] = name;

            // Save the changes to the XML file
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
                    var deleteButton = new System.Windows.Forms.Button { Text = "Delete", Location = new Point(nameTextBox.Location.X + nameTextBox.Width + 4, nameTextBox.Location.Y) };

                    deleteButton.Click += (s, e) =>
                    {
                        // Empty the text of the nameTextBox
                        nameTextBox.Text = "";
                    };

                    okButton.Click += (s, e) =>
                    {
                        // Update the name of the ID in idNames and save the changes in the XML file
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
                        // Update the ListBox
                        listBox.Items.Clear();
                        PopulateListBox();
                    }
                }
            }
            // Update the labels
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
            // Count the number of occupied IDs
            int totalIDs = idNames.Count;

            // Update the label
            IDLabelinortotal.Text = $"Named ID Total: {totalIDs}";
        }

        private void UpdateFreeIDsLabel()
        {
            // Calculate the number of free IDs
            int freeIDs = Gumps.GetCount() - idNames.Count;

            // Update the label
            LabelFreeIDs.Text = $"Total Free IDs: {freeIDs}";
        }
        #endregion

        #region Paint

        // Add state variable
        private bool isDrawing = false;
        private Color drawingColor = Color.Black;
        private Stack<Bitmap> previousImages = new Stack<Bitmap>();
        private Bitmap originalImage; // Original image
        private Bitmap originalImageDraw; // Drawing Image
        private Bitmap currentImage;
        // Create a list to store the lines
        List<List<Point>> lines = new List<List<Point>>();
        // Create a temporary list to save the current line
        List<Point> currentLine;
        // Create a second batch for the redo function
        Stack<Bitmap> redoImages = new Stack<Bitmap>();


        private bool removeColor = false; // Removes colors
        private bool isSelecting = false;
        private Rectangle selectionRectangle = new Rectangle(); //selection rectangle
        private System.Drawing.Image loadedImage; //Extra for inserting rectangle characters 

        private bool isSelectingCircle = false; // Add state variable
        private Rectangle selectionCircle = new Rectangle(); // selection circle

        #region LoadImage
        private void LoadImageIntopictureBoxDraw(System.Drawing.Image image)
        {
            tbPinselSize.Text = "2";

            // Check if the image is not null
            if (image != null)
            {
                // Save the original image and create a modifiable copy
                originalImage = new Bitmap(image);
                originalImageDraw = new Bitmap(originalImage);

                // Assign the resizable copy to the pictureBoxDraw
                pictureBoxDraw.Image = originalImageDraw;
            }
            else
            {
                MessageBox.Show("The image could not be loaded.");
            }
        }
        #endregion

        #region MouseMove
        private void pictureBoxDraw_MouseMove(object sender, MouseEventArgs e)
        {
            // Only draw when the left mouse button is pressed and the drawing function is activated
            if (e.Button == MouseButtons.Left && isDrawing)
            {
                // Make sure the image is not null before drawing on it
                if (originalImageDraw != null)
                {
                    // Get the brush size from the TextBox
                    float pinselSize = float.Parse(tbPinselSize.Text);

                    // Only draw if the brush size is greater than 0
                    if (pinselSize > 0)
                    {
                        using (Graphics g = Graphics.FromImage(originalImageDraw))
                        {
                            // Calculate the offset by centering the image
                            int offsetX = (pictureBoxDraw.Width - originalImageDraw.Width) / 2;
                            int offsetY = (pictureBoxDraw.Height - originalImageDraw.Height) / 2;

                            // Calculate the correct coordinates on the image
                            int imageX = e.X - offsetX;
                            int imageY = e.Y - offsetY;

                            // Make sure the coordinates are within the boundaries of the image
                            if (imageX >= 0 && imageX < originalImageDraw.Width && imageY >= 0 && imageY < originalImageDraw.Height)
                            {
                                // Draw on the Graphics object with the specified brush size
                                g.FillEllipse(new SolidBrush(drawingColor), imageX, imageY, pinselSize, pinselSize);

                                // Add the current point to the current line
                                currentLine.Add(new Point(imageX, imageY));

                                // Update the pictureBoxDraw control
                                pictureBoxDraw.Invalidate();
                            }
                        }
                    }
                }
            }
            if (e.Button == MouseButtons.Left && checkBoxRectangle.Checked)
            {
                // Update the size of the rectangle selection box
                selectionRectangle.Width = e.X - selectionRectangle.X;
                selectionRectangle.Height = e.Y - selectionRectangle.Y;

                // Draw the selection box
                pictureBoxDraw.Invalidate();
            }
            if (e.Button == MouseButtons.Left && isSelectingCircle)
            {
                // Update the size of the Circle selection box
                selectionCircle.Width = e.X - selectionCircle.X;
                selectionCircle.Height = e.Y - selectionCircle.Y;

                // Draw the selection box
                pictureBoxDraw.Invalidate();
            }
        }
        #endregion

        #region MouseDown
        private void pictureBoxDraw_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && checkBoxRectangle.Checked)
            {
                // Initialize the selection field
                selectionRectangle = new Rectangle(e.X, e.Y, 0, 0);
                isSelecting = true;
            }
            if (e.Button == MouseButtons.Left && isSelectingCircle)
            {
                // Initialize the selection field
                selectionCircle = new Rectangle(e.X, e.Y, 0, 0);
                isSelecting = true;
            }
            if (e.Button == MouseButtons.Left && isDrawing)
            {
                // Add the current state of the image to the stack before you start drawing
                previousImages.Push((Bitmap)originalImageDraw.Clone());

                // Initialize the current line
                currentLine = new List<Point>();
            }
        }
        #endregion

        #region MouseUp
        private void pictureBoxDraw_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isDrawing)
            {
                // Draw the entire line on the picture
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

                // Add the changed state of the image to the stack
                previousImages.Push((Bitmap)originalImageDraw.Clone());

                // Empty the current line
                currentLine.Clear();
            }
            if (e.Button == MouseButtons.Left && isSelecting)
            {
                // Complete the selection
                isSelecting = false;
            }
        }
        #endregion

        #region Textbox Color Change
        private void TextBoxColor_TextChanged(object sender, EventArgs e)
        {
            // Change character color when text is changed in TextBoxColor
            string colorText = TextBoxColor.Text.TrimStart('#');

            // Check that the text is a valid hexadecimal value
            if (int.TryParse("FF" + colorText, System.Globalization.NumberStyles.HexNumber, null, out int argb))
            {
                drawingColor = Color.FromArgb(argb);
            }
            else
            {
                MessageBox.Show("Please enter a valid color code.");
            }
        }
        #endregion

        #region Undo
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Perform the "Undo" action twice
            for (int i = 0; i < 2; i++)
            {
                // Restore the image to its previous state if the stack is not empty
                if (previousImages.Count > 0)
                {
                    // Add the current state to the redo stack
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
            // Toggle the state
            isDrawing = !isDrawing;

            // Change the background and display a message
            if (isDrawing)
            {
                contextMenuStripPicturebBox.Renderer = new MyRenderer(this);
                MessageBox.Show("You can now draw.");

                // Load the image into the pictureBoxDraw
                LoadImageIntopictureBoxDraw(pictureBoxDraw.Image);
            }
            else
            {
                contextMenuStripPicturebBox.Renderer = new ToolStripProfessionalRenderer();
                MessageBox.Show("Drawing is now disabled.");
            }
        }
        #endregion

        #region Import Clipbord
        private void importImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if there is an image in the cache
            if (Clipboard.ContainsImage())
            {
                // Get the image from the cache
                System.Drawing.Image clipboardImage = Clipboard.GetImage();

                // Save the image and create a modifiable copy
                originalImage = new Bitmap(clipboardImage);
                originalImageDraw = new Bitmap(originalImage);

                // Load the image into the PictureBoxDraw
                pictureBoxDraw.Image = originalImageDraw;
            }
            else
            {
                MessageBox.Show("There is no image in the buffer.");
            }
        }
        #endregion

        #region Color Dialog
        private void btColorDialog_Click(object sender, EventArgs e)
        {
            // Create a new ColorDialog object
            ColorDialog colorDialog = new ColorDialog();

            // Display the dialog and verify that the user clicked OK
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                // Convert the selected color to a hexadecimal value
                string hexColor = colorDialog.Color.R.ToString("X2") + colorDialog.Color.G.ToString("X2") + colorDialog.Color.B.ToString("X2");

                // Insert the hexadecimal value into the TextBoxColor
                TextBoxColor.Text = hexColor;
            }
        }
        #endregion

        private void tbPinselSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow numbers to be entered
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        #region Pinsel Size
        private void tbPinselSize_TextChanged(object sender, EventArgs e)
        {
            // Make sure the value is not greater than 60
            if (int.TryParse(tbPinselSize.Text, out int value))
            {
                if (value > 60)
                {
                    // Change the value in the TextBox to 60
                    tbPinselSize.Text = "60";
                }
            }
            else if (tbPinselSize.Text != string.Empty)
            {
                // If the text is not a valid number and is not empty,
                // reset it to the default value
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
                MessageBox.Show("There is no image to copy.");
            }
        }
        #endregion

        #region Send Image to Paint Box
        private void sendImageToPaintBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex != -1)
            {
                string selectedItem = listBox.Items[listBox.SelectedIndex].ToString();
                int startIdx = selectedItem.IndexOf("ID: ") + 4; // Starting index of the integer value
                int length = selectedItem.IndexOf(" (") - startIdx; // Length of the integer value
                string intValueStr = selectedItem.Substring(startIdx, length); // Extract the integer value
                int i = int.Parse(intValueStr);

                if (Gumps.IsValidIndex(i))
                {
                    Bitmap originalBmp = Gumps.GetGump(i);
                    if (originalBmp != null)
                    {
                        // Make a copy of the original image
                        Bitmap bmp = new Bitmap(originalBmp);

                        // Color change function built in
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

                        // Save the image and create a modifiable copy
                        originalImageDraw = bmp24bit;

                        // Load the image into the pictureBoxDraw
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
            // Advance the image to the next state if the redo stack is not empty
            if (redoImages.Count > 0)
            {
                // Add the current state to the undo stack
                previousImages.Push(originalImageDraw);

                // Advance the image to the next state
                originalImageDraw = redoImages.Pop();
                pictureBoxDraw.Image = originalImageDraw;
            }
        }

        private void rotateImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if an image exists in the pictureBoxDraw
            if (pictureBoxDraw.Image != null)
            {
                // Rotate the image 90 degrees counterclockwise
                pictureBoxDraw.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);

                // Update the pictureBoxDraw control
                pictureBoxDraw.Invalidate();
            }
        }

        #region trackBarBrightness

        // Add state variable
        private bool ignoreColors = false;

        private void trackBarBrightness_Scroll(object sender, EventArgs e)
        {
            // Update the label with the current value of the TrackBar
            labelBrightnessValue.Text = trackBarBrightness.Value.ToString();

            // Create a temporary copy of the original image
            Bitmap tempBmp = new Bitmap(originalImageDraw);

            // Create a ColorMatrix and change the brightness value
            float brightness = trackBarBrightness.Value * 0.01f; // Scale the value to a range of -0.3 to 0.3
            float[][] matrixItems = {
        new float[] {1, 0, 0, 0, 0},
        new float[] {0, 1, 0, 0, 0},
        new float[] {0, 0, 1, 0, 0},
        new float[] {0, 0, 0, 1, 0},
        new float[] {brightness, brightness, brightness, 1, 1}
    };
            ColorMatrix colorMatrix = new ColorMatrix(matrixItems);

            // Create an ImageAttributes object and set the ColorMatrix
            ImageAttributes imageAttr = new ImageAttributes();
            imageAttr.SetColorMatrix(colorMatrix);

            // Draw the image with the new ImageAttributes
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
                                    new Rectangle(x, y, 1, 1), // Target rectangle
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
                        new Rectangle(0, 0, originalImageDraw.Width, originalImageDraw.Height), // Target rectangle
                        0, 0, originalImageDraw.Width, originalImageDraw.Height,
                        GraphicsUnit.Pixel,
                        imageAttr);
                }
            }

            // Put the modified image into the PictureBoxDraw
            if (currentImage != null)
            {
                currentImage.Dispose();
            }
            currentImage = tempBmp;
            pictureBoxDraw.Image = currentImage;
        }


        // Event handler for the CheckBox
        private void checkBoxBrightness_CheckedChanged(object sender, EventArgs e)
        {
            ignoreColors = checkBoxBrightness.Checked;
        }
        #endregion

        #region ContrastColors
        // Add state variable
        private bool ignoreContrastColors = false;

        private void trackBarContrast_Scroll(object sender, EventArgs e)
        {
            // Update the label with the current value of the TrackBar
            labelContrastValue.Text = trackBarContrast.Value.ToString();

            // Create a temporary copy of the original image
            Bitmap tempBmp = new Bitmap(originalImageDraw);

            if (trackBarContrast.Value == 0)
            {
                // Reset the image to the original image
                pictureBoxDraw.Image = new Bitmap(originalImageDraw);
                return;
            }

            // Create a ColorMatrix and change the contrast value
            float contrast = 1 + trackBarContrast.Value * 0.02f; // Scale the value to a range of 1 to 1.6
            float translate = 0.5f * (1.0f - contrast);
            float[][] matrixItems = {
        new float[] {contrast, 0, 0, 0, 0},
        new float[] {0, contrast, 0, 0, 0},
        new float[] {0, 0, contrast, 0, 0},
        new float[] {0, 0, 0, 1, 0},
        new float[] {translate, translate, translate, 1, 1}
    };
            ColorMatrix colorMatrix = new ColorMatrix(matrixItems);

            // Create an ImageAttributes object and set the ColorMatrix
            ImageAttributes imageAttr = new ImageAttributes();
            imageAttr.SetColorMatrix(colorMatrix);

            // Draw the image with the new ImageAttributes
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
                                    new Rectangle(x, y, 1, 1), // Target rectangle
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
                        new Rectangle(0, 0, originalImageDraw.Width, originalImageDraw.Height), // Target rectangle
                        0, 0, originalImageDraw.Width, originalImageDraw.Height,
                        GraphicsUnit.Pixel,
                        imageAttr);
                }
            }

            // Put the modified image into the PictureBoxDraw
            if (currentImage != null)
            {
                currentImage.Dispose();
            }
            currentImage = tempBmp;
            pictureBoxDraw.Image = currentImage;
        }

        // Event handler for the CheckBox
        private void checkBoxContrast_CheckedChanged(object sender, EventArgs e)
        {
            ignoreContrastColors = checkBoxContrast.Checked;
        }
        #endregion

        #region Color
        // Add state variable

        private void trackBarColorR_Scroll(object sender, EventArgs e)
        {

            // Update the label with the current value of the TrackBar
            labelColorRValue.Text = trackBarColorR.Value.ToString();

            // Create a temporary copy of the original image
            Bitmap tempBmp = new Bitmap(originalImageDraw);

            // Create a ColorMatrix and change the red value
            float colorScaleR = trackBarColorR.Value * 0.01f;
            float[][] matrixItems = {
        new float[] {1 + colorScaleR, 0, 0, 0, 0}, // Adjust red value
        new float[] {0, 1, 0, 0, 0},
        new float[] {0, 0, 1, 0, 0},
        new float[] {0, 0, 0, 1, 0},
        new float[] {0, 0, 0, 1, 1}
    };
            ColorMatrix colorMatrix = new ColorMatrix(matrixItems);

            // Create an ImageAttributes object and set the ColorMatrix
            ImageAttributes imageAttr = new ImageAttributes();
            imageAttr.SetColorMatrix(colorMatrix);

            // Draw the image with the new ImageAttributes
            using (Graphics g = Graphics.FromImage(tempBmp))
            {
                g.DrawImage(originalImageDraw,
                    new Rectangle(0, 0, originalImageDraw.Width, originalImageDraw.Height), // Target rectangle
                    0, 0, originalImageDraw.Width, originalImageDraw.Height,
                    GraphicsUnit.Pixel,
                    imageAttr);
            }

            // Put the modified image into the PictureBoxDraw
            pictureBoxDraw.Image = tempBmp;
        }

        private void trackBarColorG_Scroll(object sender, EventArgs e)
        {

            // Update the label with the current value of the TrackBar
            labelColorGValue.Text = trackBarColorG.Value.ToString();

            // Create a temporary copy of the original image
            Bitmap tempBmp = new Bitmap(originalImageDraw);

            // Create a ColorMatrix and change the green value
            float colorScaleG = trackBarColorG.Value * 0.01f;
            float[][] matrixItems = {
        new float[] {1, 0, 0, 0, 0},
        new float[] {0, 1 + colorScaleG, 0, 0, 0}, // Adjust green value
        new float[] {0, 0, 1, 0, 0},
        new float[] {0, 0, 0, 1, 0},
        new float[] {0, 0, 0, 1, 1}
        };
            ColorMatrix colorMatrix = new ColorMatrix(matrixItems);

            // Create an ImageAttributes object and set the ColorMatrix
            ImageAttributes imageAttr = new ImageAttributes();
            imageAttr.SetColorMatrix(colorMatrix);

            // Draw the image with the new ImageAttributes
            using (Graphics g = Graphics.FromImage(tempBmp))
            {
                g.DrawImage(originalImageDraw,
                    new Rectangle(0, 0, originalImageDraw.Width, originalImageDraw.Height), // Target rectangle
                    0, 0, originalImageDraw.Width, originalImageDraw.Height,
                    GraphicsUnit.Pixel,
                    imageAttr);
            }

            // Put the modified image into the PictureBoxDraw
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

        // Event handler for the CheckBox
        #endregion

        #region Reset RGB
        private void resetButtonRGB_Click(object sender, EventArgs e)
        {
            // Set the TrackBars values ​​to 0
            trackBarColorR.Value = 0;
            trackBarColorG.Value = 0;
            trackBarColorB.Value = 0;

            // Reset the label texts
            labelColorRValue.Text = "0";
            labelColorGValue.Text = "0";
            labelColorBValue.Text = "0";

            // Trigger the TrackBars scroll events
            trackBarColorR_Scroll(sender, e);
            trackBarColorG_Scroll(sender, e);
            trackBarColorB_Scroll(sender, e);
        }
        #endregion

        #region Remove Color #000000 #ffffff
        private void checkBoxRemoveColor_CheckedChanged(object sender, EventArgs e)
        {
            // Update the state
            removeColor = checkBoxRemoveColor.Checked;

            // Create a temporary copy of the original image
            Bitmap tempBmp = new Bitmap(originalImageDraw);

            if (removeColor)
            {
                // Loop through every pixel in the image
                for (int y = 0; y < tempBmp.Height; y++)
                {
                    for (int x = 0; x < tempBmp.Width; x++)
                    {
                        // Get the color of the current pixel
                        Color pixelColor = tempBmp.GetPixel(x, y);

                        // Check whether the color of the pixel is black or white
                        if (pixelColor.ToArgb() == Color.Black.ToArgb() || pixelColor.ToArgb() == Color.White.ToArgb())
                        {
                            // Change the color of the pixel to Transparent
                            tempBmp.SetPixel(x, y, Color.Transparent);
                        }
                    }
                }
            }

            // Put the modified image into the PictureBoxDraw
            pictureBoxDraw.Image = tempBmp;
        }
        #endregion

        #region CheckBox Rechteck
        private void checkBoxRectangle_CheckedChanged(object sender, EventArgs e)
        {
            // If checkBoxRectangle is enabled, disable checkBoxDrawCircle
            if (checkBoxRectangle.Checked)
            {
                checkBoxDrawCircle.Checked = false;
            }

            // Update the state
            isSelecting = checkBoxRectangle.Checked;

        }

        private void checkBoxDrawCircle_CheckedChanged(object sender, EventArgs e)
        {
            // If checkBoxDrawCircle is enabled, disable checkBoxRectangle
            if (checkBoxDrawCircle.Checked)
            {
                checkBoxRectangle.Checked = false;
            }

            // Update the state
            isSelectingCircle = checkBoxDrawCircle.Checked;
        }
        #endregion

        #region PictureBox_Paint
        private void pictureBoxDraw_Paint(object sender, PaintEventArgs e)
        {
            if (checkBoxRectangle.Checked)
            {
                // Draw the selection box with a dashed yellow line
                using (Pen pen = new Pen(Color.Yellow))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawRectangle(pen, selectionRectangle);
                }
            }
            if (isSelectingCircle)
            {
                // Draw the selection circle with a dashed yellow line
                using (Pen pen = new Pen(Color.Yellow))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawEllipse(pen, selectionCircle);
                }
            }
        }
        #endregion

        #region ContextMenu Texture fill       

        private void SelectingRectangleCircleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files|*.bmp;*.jpg;*.jpeg;*.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Load the image and save it in loadedImage
                loadedImage = new Bitmap(System.Drawing.Image.FromFile(openFileDialog.FileName));

                // Scale the loaded image to the size of the selected rectangle or circle
                Bitmap scaledImage = isSelectingCircle ? new Bitmap(loadedImage, selectionCircle.Size) : new Bitmap(loadedImage, selectionRectangle.Size);

                // Draw the scaled image on originalImageDraw at the position of the selection box
                using (Graphics g = Graphics.FromImage(originalImageDraw))
                {
                    for (int y = 0; y < scaledImage.Height; y++)
                    {
                        for (int x = 0; x < scaledImage.Width; x++)
                        {
                            Color pixelColor = scaledImage.GetPixel(x, y);

                            // Check that the color of the pixel is neither black nor white
                            if (pixelColor != Color.FromArgb(255, 0, 0, 0) && pixelColor != Color.FromArgb(255, 255, 255, 255))
                            {
                                // Draw the pixel on originalImageDraw
                                int drawX = isSelectingCircle ? selectionCircle.X + x : selectionRectangle.X + x;
                                int drawY = isSelectingCircle ? selectionCircle.Y + y : selectionRectangle.Y + y;

                                // Check if the point is inside the circle when circle selection mode is active
                                if (!isSelectingCircle || Math.Pow(x - selectionCircle.Width / 2, 2) + Math.Pow(y - selectionCircle.Height / 2, 2) <= Math.Pow(selectionCircle.Width / 2, 2))
                                {
                                    // Calculate the position on the image based on the position and size of the image in the PictureBox
                                    int imageX = drawX - (pictureBoxDraw.Width - originalImageDraw.Width) / 2;
                                    int imageY = drawY - (pictureBoxDraw.Height - originalImageDraw.Height) / 2;

                                    // Make sure the coordinates are within the boundaries of the image
                                    if (imageX >= 0 && imageX < originalImageDraw.Width && imageY >= 0 && imageY < originalImageDraw.Height)
                                    {
                                        g.FillRectangle(new SolidBrush(pixelColor), imageX, imageY, 1, 1);
                                    }
                                }
                            }
                        }
                    }
                }

                // Update the pictureBoxDraw control with originalImageDraw
                pictureBoxDraw.Image = originalImageDraw;
                pictureBoxDraw.Invalidate();

                // Only remove the selection field if no checkbox is active
                if (!checkBoxRectangle.Checked)
                {
                    isSelecting = false;
                    selectionRectangle = new Rectangle();
                }
                if (!checkBoxDrawCircle.Checked)
                {
                    isSelectingCircle = false;
                    selectionCircle = new Rectangle();
                }
            }
        }
        #endregion

        #region Save Image
        private void saveToolStripMenuItemPictureBoxDraw_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Bitmap Image|*.bmp|TIFF Image|*.tiff|PNG Image|*.png|JPEG Image|*.jpg";
            saveFileDialog.Title = "Speichern Sie das Bild als...";
            saveFileDialog.FilterIndex = 1; // Set .bmp as the default format

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Save the image in the PictureBox in the selected format
                switch (saveFileDialog.FilterIndex)
                {
                    case 1: // .bmp
                        pictureBoxDraw.Image.Save(saveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                    case 2: // .tiff
                        pictureBoxDraw.Image.Save(saveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Tiff);
                        break;
                    case 3: // .png
                        pictureBoxDraw.Image.Save(saveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    case 4: // .jpg
                        pictureBoxDraw.Image.Save(saveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                }
            }
        }
        #endregion
    }
    #endregion
}
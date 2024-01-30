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
using Ultima;
using System.Security.Cryptography;
using UoFiddler.Controls.Classes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Xml.Linq;
using UoFiddler.Controls.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class UOArtMergeForm : Form
    {
        private readonly Dictionary<int, bool> _mCompare = new Dictionary<int, bool>();
        private readonly ImageConverter _ic = new ImageConverter();
        private readonly SHA256 _sha256 = SHA256.Create();

        private Dictionary<int, Bitmap> selectedImages = new Dictionary<int, Bitmap>();

        #region UOArtMergeForm
        public UOArtMergeForm()
        {
            InitializeComponent();
            LoadOrg();

            listBoxOrg.DrawMode = DrawMode.OwnerDrawFixed;
            listBoxOrg.DrawItem += listBoxOrg_DrawItem;

            listBoxLeft.DrawMode = DrawMode.OwnerDrawFixed;
            listBoxRight.DrawMode = DrawMode.OwnerDrawFixed;

            LoadDirectoriesIntoComboBox();
            this.Load += UOArtMergeForm_Load;
        }
        #endregion

        #region LoadOrg
        private void LoadOrg()
        {
            listBoxOrg.Items.Clear();
            listBoxOrg.BeginUpdate();
            List<object> cache = new List<object>();
            int staticsLength = Art.GetMaxItemId() + 1;
            for (int i = 0; i < staticsLength; i++)
            {
                cache.Add(i);
            }
            listBoxOrg.Items.AddRange(cache.ToArray());
            listBoxOrg.EndUpdate();
        }
        #endregion

        #region OnIndexChangedOrg
        private void OnIndexChangedOrg(object sender, EventArgs e)
        {
            if (listBoxOrg.SelectedIndex == -1 || listBoxOrg.Items.Count < 1)
            {
                return;
            }
            int i = int.Parse(listBoxOrg.Items[listBoxOrg.SelectedIndex].ToString());
            pictureBoxOrg.BackgroundImage = Art.IsValidStatic(i) ? Art.GetStatic(i) : null;
            listBoxOrg.Invalidate();

            pictureBoxOrg.BackgroundImage = Art.IsValidStatic(i)
                ? Art.GetStatic(i)
                : null;

            if (checkBoxSameHeight.Checked)
            {
                int selectedIndex = listBoxOrg.SelectedIndex;
                if (selectedIndex < listBoxLeft.Items.Count)
                {
                    listBoxLeft.SelectedIndex = selectedIndex;
                }
                if (selectedIndex < listBoxRight.Items.Count)
                {
                    listBoxRight.SelectedIndex = selectedIndex;
                }
            }

            // Set the text of the searchTextBox to the hexadecimal representation of the selected item
            searchTextBox.Text = $"0x{i:X}";

            // Check if an item in the listBoxOrg is selected
            if (listBoxOrg.SelectedIndex != -1)
            {
                int selectedIndex = listBoxOrg.SelectedIndex;
                // Convert the selected index to a hexadecimal address
                string hexAddress = $"0x{selectedIndex:X}";
                // Update lbIndex with the hexadecimal address and ID
                lbIndex.Text = $"Hex-Adresse: {hexAddress}, ID: {selectedIndex}";
            }

            // Get the total number of items (IDs) in listBoxOrg
            int totalIDs = listBoxOrg.Items.Count;
            // Update lbIndex with the total number of IDs
            lbCountOrg.Text = $"Total number IDs: {totalIDs}";
        }
        #endregion

        #region listBoxOrg_DrawItem
        private void listBoxOrg_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
            {
                return;
            }

            e.DrawBackground();

            int i = int.Parse(listBoxOrg.Items[e.Index].ToString());
            string hexValue = $"0x{i:X}";
            string displayValue = $"{hexValue} ({i})"; // Displays both the hex address and the ID address

            using (Brush brush = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(displayValue, e.Font, brush, e.Bounds);
            }
        }
        #endregion

        #region btDirLeft
        private void btDirLeft_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the art files";
                dialog.ShowNewFolderButton = false;
                if (!string.IsNullOrEmpty(textBoxLeftDir.Text))
                {
                    dialog.SelectedPath = textBoxLeftDir.Text;
                }
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxLeftDir.Text = dialog.SelectedPath;
                }
            }
        }
        #endregion

        #region btLeftLoad
        private void btLeftLoad_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxLeftDir.Text))
            {
                return;
            }

            string path = textBoxLeftDir.Text;
            string mulFile = Path.Combine(path, "art.mul");
            string idxFile = Path.Combine(path, "artidx.mul");
            if (File.Exists(mulFile) && File.Exists(idxFile))
            {
                SecondArt.SetFileIndex(idxFile, mulFile); //Load .mul file
                LoadLeft();
            }
        }
        #endregion

        #region LoadLeft
        private void LoadLeft()
        {
            listBoxLeft.Items.Clear();
            listBoxLeft.BeginUpdate();
            List<object> cache = new List<object>();
            int staticLength = SecondArt.GetMaxItemId() + 1;
            for (int i = 0; i < staticLength; i++)
            {
                cache.Add(i);
            }
            listBoxLeft.Items.AddRange(cache.ToArray());
            listBoxLeft.EndUpdate();
        }
        #endregion

        #region listBoxLeft_SelectedIndexChanged
        private void listBoxLeft_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxLeft.SelectedIndex == -1)
            {
                return;
            }

            int i = int.Parse(listBoxLeft.Items[listBoxLeft.SelectedIndex].ToString());
            pictureBoxLeft.Image = SecondArt.IsValidStatic(i) ? SecondArt.GetStatic(i) : null;

            if (checkBoxSameHeight.Checked)
            {
                int selectedIndex = listBoxLeft.SelectedIndex;
                if (selectedIndex < listBoxOrg.Items.Count)
                {
                    listBoxOrg.SelectedIndex = selectedIndex;
                }
                if (selectedIndex < listBoxLeft.Items.Count)
                {
                    listBoxLeft.SelectedIndex = selectedIndex;
                }
            }

            // Check if an item in the listBoxLeft is selected
            if (listBoxLeft.SelectedIndex != -1)
            {
                int selectedIndex = listBoxLeft.SelectedIndex;
                // Convert the selected index to a hexadecimal address
                string hexAddress = $"0x{selectedIndex:X}";
                // Update lbIndexLeft with the hexadecimal address and ID
                lbIndexLeft.Text = $"Hex-Adresse: {hexAddress}, ID: {selectedIndex}";
            }

            // Get the total number of items (IDs) in listBoxLeft
            int totalIDs = listBoxLeft.Items.Count;
            // Update lbIndex with the total number of IDs
            lbCountLeft.Text = $"Total number IDs: {totalIDs}";
        }
        #endregion

        #region listBoxLeft
        private void listBoxLeft_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
            {
                return;
            }

            e.DrawBackground();

            int i = int.Parse(listBoxLeft.Items[e.Index].ToString());
            string hexValue = $"0x{i:X}";
            string displayValue = $"{hexValue} ({i})"; // Displays both the hex address and the ID address

            using (Brush brush = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(displayValue, e.Font, brush, e.Bounds);
            }
        }
        #endregion

        #region btDirRight
        private void btDirRight_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the art files";
                dialog.ShowNewFolderButton = false;
                if (!string.IsNullOrEmpty(textBoxRightDir.Text))
                {
                    dialog.SelectedPath = textBoxRightDir.Text;
                }
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxRightDir.Text = dialog.SelectedPath;                    
                }
            }
        }
        #endregion

        #region btRightLoad
        private void btRightLoad_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxRightDir.Text))
            {
                return;
            }

            string path = textBoxRightDir.Text;
            string mulFile = Path.Combine(path, "art.mul");
            string idxFile = Path.Combine(path, "artidx.mul");
            if (File.Exists(mulFile) && File.Exists(idxFile))
            {
                SecondArt.SetFileIndex(idxFile, mulFile); //Load .mul file
                LoadRight();
            }
        }
        #endregion

        #region LoadRight
        private void LoadRight()
        {
            listBoxRight.Items.Clear();
            listBoxRight.BeginUpdate();
            List<object> cache = new List<object>();
            int staticLength = SecondArt.GetMaxItemId() + 1;
            for (int i = 0; i < staticLength; i++)
            {
                cache.Add(i);
            }
            listBoxRight.Items.AddRange(cache.ToArray());
            listBoxRight.EndUpdate();
        }
        #endregion

        #region listBoxRight_SelectedIndexChanged
        private void listBoxRight_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxRight.SelectedIndex == -1)
            {
                return;
            }

            int i = int.Parse(listBoxRight.Items[listBoxRight.SelectedIndex].ToString());
            pictureBoxRight.Image = SecondArt.IsValidStatic(i) ? SecondArt.GetStatic(i) : null;

            if (checkBoxSameHeight.Checked)
            {
                int selectedIndex = listBoxRight.SelectedIndex;
                if (selectedIndex < listBoxOrg.Items.Count)
                {
                    listBoxOrg.SelectedIndex = selectedIndex;
                }
                if (selectedIndex < listBoxRight.Items.Count)
                {
                    listBoxRight.SelectedIndex = selectedIndex;
                }
            }

            // Check if an item in the listBoxRight is selected
            if (listBoxRight.SelectedIndex != -1)
            {
                int selectedIndex = listBoxRight.SelectedIndex;
                // Convert the selected index to a hexadecimal address
                string hexAddress = $"0x{selectedIndex:X}";
                // Update lbIndexRight with the hexadecimal address and ID
                lbIndexRight.Text = $"Hex-Adresse: {hexAddress}, ID: {selectedIndex}";
            }

            // Get the total number of items (IDs) in listBoxRight
            int totalIDs = listBoxRight.Items.Count;
            // Update lbIndex with the total number of IDs
            lbCountRight.Text = $"Total number IDs: {totalIDs}";
        }
        #endregion

        #region listBoxRight_DrawItem
        private void listBoxRight_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
            {
                return;
            }

            e.DrawBackground();

            int i = int.Parse(listBoxRight.Items[e.Index].ToString());
            string hexValue = $"0x{i:X}";
            string displayValue = $"{hexValue} ({i})"; // Displays both the hex address and the ID address

            using (Brush brush = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(displayValue, e.Font, brush, e.Bounds);
            }
        }
        #endregion

        #region OnChangeShowDiff
        private void OnChangeShowDiff(object sender, EventArgs e)
        {
            // Call Compare on each item in listBoxOrg
            for (int i = 0; i < listBoxOrg.Items.Count; i++)
            {
                Compare(i);
            }

            if (_mCompare.Count < 1)
            {
                if (checkBoxOnChangeShowDiff.Checked)
                {
                    MessageBox.Show("Second Item file is not loaded!");
                    checkBoxOnChangeShowDiff.Checked = false;
                }
                return;
            }

            listBoxOrg.BeginUpdate();
            listBoxRight.BeginUpdate();
            listBoxLeft.BeginUpdate();
            listBoxOrg.Items.Clear();
            listBoxRight.Items.Clear();
            listBoxLeft.Items.Clear();
            List<object> cache = new List<object>();
            int staticLength = Math.Max(Art.GetMaxItemId(), SecondArt.GetMaxItemId());
            if (checkBoxOnChangeShowDiff.Checked)
            {
                for (int i = 0; i < staticLength; i++)
                {
                    if (!Compare(i))
                    {
                        cache.Add(i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < staticLength; i++)
                {
                    cache.Add(i);
                }
            }
            listBoxOrg.Items.AddRange(cache.ToArray());
            listBoxRight.Items.AddRange(cache.ToArray());
            listBoxLeft.Items.AddRange(cache.ToArray());
            listBoxOrg.EndUpdate();
            listBoxRight.EndUpdate();
            listBoxLeft.EndUpdate();
        }
        #endregion

        #region Compare
        private bool Compare(int index)
        {
            if (_mCompare.ContainsKey(index))
            {
                return _mCompare[index];
            }

            Bitmap bitorg = Art.GetStatic(index);
            Bitmap bitsec = SecondArt.GetStatic(index);
            if (bitorg == null && bitsec == null)
            {
                _mCompare[index] = true;
                return true;
            }
            if (bitorg == null || bitsec == null
                               || bitorg.Size != bitsec.Size)
            {
                _mCompare[index] = false;
                return false;
            }

            byte[] btImage1 = new byte[1];
            btImage1 = (byte[])_ic.ConvertTo(bitorg, btImage1.GetType());
            byte[] btImage2 = new byte[1];
            btImage2 = (byte[])_ic.ConvertTo(bitsec, btImage2.GetType());

            byte[] checksum1 = _sha256.ComputeHash(btImage1);
            byte[] checksum2 = _sha256.ComputeHash(btImage2);
            bool res = true;
            for (int j = 0; j < checksum1.Length; ++j)
            {
                if (checksum1[j] != checksum2[j])
                {
                    res = false;
                    break;
                }
            }
            _mCompare[index] = res;
            return res;
        }
        #endregion

        #region btRightMoveItem
        private void btRightMoveItem_Click(object sender, EventArgs e)
        {
            if (listBoxRight.SelectedIndex == -1)
            {
                return;
            }
            int i = int.Parse(listBoxRight.Items[listBoxRight.SelectedIndex].ToString());
            if (!SecondArt.IsValidStatic(i))
            {
                return;
            }
            Bitmap copy = new Bitmap(SecondArt.GetStatic(i));

            // If checkBoxFreeIDchoice is enabled, insert the image into listBoxOrg at the selected ID
            if (checkBoxfFreeIDchoice.Checked)
            {
                int selectedIndex = listBoxOrg.SelectedIndex;
                if (selectedIndex != -1)
                {
                    Art.ReplaceStatic(selectedIndex, copy);
                    Options.ChangedUltimaClass["Art"] = true;
                    ControlEvents.FireItemChangeEvent(this, selectedIndex);
                    _mCompare[selectedIndex] = true;

                    // Update pictureBoxOrg with the selected image
                    pictureBoxOrg.BackgroundImage = Art.GetStatic(selectedIndex);
                }
            }
            else
            {
                int staticLength = Art.GetMaxItemId() + 1;
                if (i >= staticLength)
                {
                    return;
                }
                Art.ReplaceStatic(i, copy);
                Options.ChangedUltimaClass["Art"] = true;
                ControlEvents.FireItemChangeEvent(this, i);
                _mCompare[i] = true;
                listBoxOrg.BeginUpdate();
                bool done = false;
                for (int id = 0; id < staticLength; id++)
                {
                    if (id > i)
                    {
                        listBoxOrg.Items.Insert(id, i);
                        done = true;
                        break;
                    }
                    if (id == i)
                    {
                        done = true;
                        break;
                    }
                }
                if (!done)
                {
                    listBoxOrg.Items.Add(i);
                }
                listBoxOrg.EndUpdate();
                listBoxOrg.Invalidate();
                listBoxRight.Invalidate();

                // Update pictureBoxOrg with the selected item
                pictureBoxOrg.BackgroundImage = Art.GetStatic(i);
            }
        }
        #endregion

        #region checkBoxSameHeight_CheckedChanged
        private void checkBoxSameHeight_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSameHeight.Checked)
            {
                checkBoxfFreeIDchoice.Checked = false;
            }
        }
        #endregion

        #region checkBoxfFreeIDchoice_CheckedChanged
        private void checkBoxfFreeIDchoice_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxfFreeIDchoice.Checked)
            {
                checkBoxSameHeight.Checked = false;
            }
        }
        #endregion

        #region btLeftMoveItem
        private void btLeftMoveItem_Click(object sender, EventArgs e)
        {
            if (listBoxLeft.SelectedIndex == -1)
            {
                return;
            }
            int i = int.Parse(listBoxLeft.Items[listBoxLeft.SelectedIndex].ToString());
            if (!SecondArt.IsValidStatic(i))
            {
                return;
            }
            Bitmap copy = new Bitmap(SecondArt.GetStatic(i));

            // If checkBoxFreeIDchoice is enabled, insert the image into listBoxOrg at the selected ID
            if (checkBoxfFreeIDchoice.Checked)
            {
                int selectedIndex = listBoxOrg.SelectedIndex;
                if (selectedIndex != -1)
                {
                    Art.ReplaceStatic(selectedIndex, copy);
                    Options.ChangedUltimaClass["Art"] = true;
                    ControlEvents.FireItemChangeEvent(this, selectedIndex);
                    _mCompare[selectedIndex] = true;

                    // Update pictureBoxOrg with the selected image
                    pictureBoxOrg.BackgroundImage = Art.GetStatic(selectedIndex);
                }
            }
            else
            {
                int staticLength = Art.GetMaxItemId() + 1;
                if (i >= staticLength)
                {
                    return;
                }
                Art.ReplaceStatic(i, copy);
                Options.ChangedUltimaClass["Art"] = true;
                ControlEvents.FireItemChangeEvent(this, i);
                _mCompare[i] = true;
                listBoxOrg.BeginUpdate();
                bool done = false;
                for (int id = 0; id < staticLength; id++)
                {
                    if (id > i)
                    {
                        listBoxOrg.Items.Insert(id, i);
                        done = true;
                        break;
                    }
                    if (id == i)
                    {
                        done = true;
                        break;
                    }
                }
                if (!done)
                {
                    listBoxOrg.Items.Add(i);
                }
                listBoxOrg.EndUpdate();
                listBoxOrg.Invalidate();
                listBoxLeft.Invalidate();

                // Update pictureBoxOrg with the selected item
                pictureBoxOrg.BackgroundImage = Art.GetStatic(i);
            }
        }
        #endregion

        #region btSaveXML
        private void btSaveXML_Click(object sender, EventArgs e)
        {
            // Create the directory if it doesn't exist
            string settingsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DirectoryisSettings");
            if (!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
            }

            // Create the XML file
            string xmlFilePath = Path.Combine(settingsDirectory, "XMLSaveDirUAArtMerge.xml");

            // Load the existing XML file if it exists
            XDocument doc;
            if (File.Exists(xmlFilePath))
            {
                doc = XDocument.Load(xmlFilePath);
            }
            else
            {
                doc = new XDocument(new XElement("Directories"));
            }

            // Count the number of existing directory entries
            int directoryCount = doc.Root.Elements("Directory").Count();

            // Add the directories to the XML file with a unique ID
            doc.Root.Add(
                new XElement("Directory",
                    new XAttribute("id", directoryCount + 1),
                    new XAttribute("name", "LeftDir"),
                    new XAttribute("path", textBoxLeftDir.Text)),
                new XElement("Directory",
                    new XAttribute("id", directoryCount + 2),
                    new XAttribute("name", "RightDir"),
                    new XAttribute("path", textBoxRightDir.Text))
            );
            doc.Save(xmlFilePath);

            // Update the comboBoxSaveDir
            comboBoxSaveDir.Items.Clear();
            comboBoxSaveDir.Items.Add(textBoxLeftDir.Text);
            comboBoxSaveDir.Items.Add(textBoxRightDir.Text);
        }
        #endregion

        #region LoadDirectoriesIntoComboBox
        private void LoadDirectoriesIntoComboBox()
        {
            // Create the path to the XML file
            string settingsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DirectoryisSettings");
            string xmlFilePath = Path.Combine(settingsDirectory, "XMLSaveDirUAArtMerge.xml");

            // Check if the XML file exists
            if (!File.Exists(xmlFilePath))
            {
                return;
            }

            // Read the XML file
            XDocument doc = XDocument.Load(xmlFilePath);
            var directories = doc.Root.Elements("Directory");

            // Add each path to comboBoxSaveDir and comboBoxSaveDir2
            comboBoxSaveDir.Items.Clear();
            comboBoxSaveDir2.Items.Clear();
            foreach (var directory in directories)
            {
                string path = directory.Attribute("path").Value;
                comboBoxSaveDir.Items.Add(path);
                comboBoxSaveDir2.Items.Add(path);  // Adding the paths to the comboBoxSaveDir2
            }
        }
        #endregion

        #region UOArtMergeForm_Load
        private void UOArtMergeForm_Load(object sender, EventArgs e)
        {
            LoadIDsIntoListBox();
        }
        #endregion

        #region LoadIDsIntoListBox
        private void LoadIDsIntoListBox()
        {
            // Create the path to the XML file
            string settingsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DirectoryisSettings");
            string xmlFilePath = Path.Combine(settingsDirectory, "XMLSaveDirUAArtMerge.xml");

            // Check if the XML file exists
            if (!File.Exists(xmlFilePath))
            {
                MessageBox.Show("The XML file could not be found.");
                return;
            }

            // Read the XML file
            XDocument doc;
            try
            {
                doc = XDocument.Load(xmlFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading XML file: {ex.Message}");
                return;
            }

            var directories = doc.Root.Elements("Directory");

            // Add each ID to the tbIDNr
            tbIDNr.Items.Clear();
            foreach (var directory in directories)
            {
                string id = directory.Attribute("id")?.Value;
                if (id != null)
                {
                    tbIDNr.Items.Add(id);
                }
            }
        }
        #endregion

        #region comboBoxSaveDir_SelectedIndexChanged
        private void comboBoxSaveDir_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSaveDir.SelectedItem != null)
            {
                string selectedPath = comboBoxSaveDir.SelectedItem.ToString();
                textBoxLeftDir.Text = selectedPath;
            }
        }
        #endregion

        #region comboBoxSaveDir2_SelectedIndexChanged
        private void comboBoxSaveDir2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSaveDir2.SelectedItem != null)
            {
                string selectedPath = comboBoxSaveDir2.SelectedItem.ToString();
                textBoxRightDir.Text = selectedPath;
            }
        }
        #endregion

        #region DeleteDirectoryById       

        private void DeleteDirectoryById(int id)
        {
            string settingsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DirectoryisSettings");
            string xmlFilePath = Path.Combine(settingsDirectory, "XMLSaveDirUAArtMerge.xml");

            if (!File.Exists(xmlFilePath))
            {
                MessageBox.Show("The XML file could not be found.");
                return;
            }

            XDocument doc;
            try
            {
                doc = XDocument.Load(xmlFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading XML file: {ex.Message}");
                return;
            }

            var directoryToDelete = doc.Root.Elements("Directory").FirstOrDefault(d => (int)d.Attribute("id") == id);
            if (directoryToDelete != null)
            {
                directoryToDelete.Remove();

                // Neu zuweisen der IDs
                int newId = 1;
                foreach (var directory in doc.Root.Elements("Directory"))
                {
                    directory.Attribute("id").Value = newId.ToString();
                    newId++;
                }

                doc.Save(xmlFilePath);

                // Remove the ID from the tbIDNr
                tbIDNr.Items.Remove(id.ToString());
            }
            else
            {
                MessageBox.Show($"The ID {id} could not be found.");
            }
        }

        #endregion

        #region btDelete
        private void btDelete_Click(object sender, EventArgs e)
        {
            // Read the ID from the textbox
            if (!int.TryParse(tbIDNr.Text, out int id))
            {
                MessageBox.Show("Please enter a valid ID.");
                return;
            }

            // Delete the directory with the specified ID
            DeleteDirectoryById(id);

            // Update the comboBoxSaveDir
            LoadDirectoriesIntoComboBox();
        }
        #endregion

        #region btremoveitemfromindex
        private void btremoveitemfromindex_Click(object sender, EventArgs e)
        {
            if (listBoxOrg.SelectedIndex != -1)
            {
                int selectedIndex = listBoxOrg.SelectedIndex;
                if (!Art.IsValidStatic(selectedIndex))
                {
                    return;
                }

                DialogResult result = MessageBox.Show($"Are you sure to remove 0x{selectedIndex:X}", "Save",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return;
                }

                // Remove the selected item from Art
                Art.RemoveStatic(selectedIndex);
                Options.ChangedUltimaClass["Art"] = true;
                ControlEvents.FireItemChangeEvent(this, selectedIndex);

                // Update pictureBoxOrg
                pictureBoxOrg.BackgroundImage = null;

                // Refresh listBoxOrg to reflect the changes
                listBoxOrg.Invalidate();
            }
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
            ProgressBarDialog barDialog = new ProgressBarDialog(Art.GetIdxLength(), "Save");
            Art.Save(Options.OutputPath);
            barDialog.Dispose();
            Cursor.Current = Cursors.Default;
            Options.ChangedUltimaClass["Art"] = false;
            MessageBox.Show($"Saved to {Options.OutputPath}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region Search Textbox Hex
        private void OnClickSearch_Click(object sender, EventArgs e)
        {
            string addressText = searchTextBox.Text;
            int address;
            if (addressText.StartsWith("0x") || System.Text.RegularExpressions.Regex.IsMatch(addressText, @"\A\b[0-9a-fA-F]+\b\Z"))
            {
                string hexText = addressText.StartsWith("0x") ? addressText.Substring(2) : addressText;
                if (int.TryParse(hexText, System.Globalization.NumberStyles.HexNumber, null, out address))
                {
                    int index = FindIndexByHexValue(listBoxOrg, hexText);
                    if (index != -1)
                    {
                        listBoxOrg.SelectedIndex = index;
                    }
                    else
                    {
                        MessageBox.Show("Address not found.");
                    }
                }
                else
                {
                    MessageBox.Show("Invalid address. Please enter a valid hex address.");
                }
            }
            else
            {
                MessageBox.Show("Invalid address. Please enter a valid hex address.");
            }
        }

        private int FindIndexByHexValue(ListBox listBox, string hexValue)
        {
            for (int i = 0; i < listBox.Items.Count; i++)
            {
                if (listBox.Items[i] is int intValue && intValue.ToString("X").Equals(hexValue, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }
        #endregion

        #region Mirror
        private void mirrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxOrg.SelectedIndex != -1)
            {
                int selectedIndex = listBoxOrg.SelectedIndex;
                if (!Art.IsValidStatic(selectedIndex))
                {
                    return;
                }

                // Get the current image
                Bitmap currentImage = Art.GetStatic(selectedIndex);
                if (currentImage != null)
                {
                    // Create a new image that is a mirrored copy of the current image
                    Bitmap mirroredImage = new Bitmap(currentImage);
                    mirroredImage.RotateFlip(RotateFlipType.RotateNoneFlipX);

                    // Replace the current image with the mirrored image in Art
                    Art.ReplaceStatic(selectedIndex, mirroredImage);
                    Options.ChangedUltimaClass["Art"] = true;
                    ControlEvents.FireItemChangeEvent(this, selectedIndex);

                    // Update pictureBoxOrg
                    pictureBoxOrg.BackgroundImage = mirroredImage;

                    // Refresh listBoxOrg to reflect the changes
                    listBoxOrg.Invalidate();
                }
            }
        }
        #endregion

        #region Copy Clipboard listBoxOrg
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxOrg.SelectedIndex != -1)
            {
                int selectedIndex = listBoxOrg.SelectedIndex;
                if (!Art.IsValidStatic(selectedIndex))
                {
                    return;
                }

                // Get the current image
                Bitmap currentImage = Art.GetStatic(selectedIndex);
                if (currentImage != null)
                {
                    // Copy the image to the clipboard
                    Clipboard.SetImage(currentImage);
                }
            }
        }
        #endregion

        #region listBoxRight

        private void copyToolStripMenuItemListBoxRight_Click(object sender, EventArgs e)
        {
            if (listBoxRight.SelectedIndex != -1)
            {
                int selectedIndex = listBoxRight.SelectedIndex;
                if (!Art.IsValidStatic(selectedIndex))
                {
                    return;
                }

                // Get the current image
                Bitmap currentImage = Art.GetStatic(selectedIndex);
                if (currentImage != null)
                {
                    // Copy the image to the clipboard
                    Clipboard.SetImage(currentImage);
                }
            }
        }
        #endregion

        #region listBoxLeft
        private void copyToolStripMenuItemListBoxLeft_Click(object sender, EventArgs e)
        {
            if (listBoxLeft.SelectedIndex != -1)
            {
                int selectedIndex = listBoxLeft.SelectedIndex;
                if (!Art.IsValidStatic(selectedIndex))
                {
                    return;
                }

                // Get the current image
                Bitmap currentImage = Art.GetStatic(selectedIndex);
                if (currentImage != null)
                {
                    // Copy the image to the clipboard
                    Clipboard.SetImage(currentImage);
                }
            }
        }
        #endregion

        #region importToolStripclipboardMenuItem
        private void importToolStripclipboardMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxOrg.SelectedIndex != -1)
            {
                int selectedIndex = listBoxOrg.SelectedIndex;
                if (!Art.IsValidStatic(selectedIndex))
                {
                    return;
                }

                // Check if the clipboard contains an image
                if (Clipboard.ContainsImage())
                {
                    // Retrieve the image from the clipboard
                    using (Bitmap bmp = new Bitmap(Clipboard.GetImage()))
                    {
                        // Create a new bitmap with the same size as the image from the clipboard
                        Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);

                        // Define the colors to Convert
                        Color[] colorsToConvert = new Color[]
                        {
                    Color.FromArgb(211, 211, 211), // #D3D3D3 => #000000
                    Color.FromArgb(0, 0, 0), // #000000 => #000000
                    Color.FromArgb(255, 255, 255), // #FFFFFF => #000000
                    Color.FromArgb(254, 254, 254) // #FEFEFE => #000000
                        };

                        // Iterate through each pixel of the image
                        for (int x = 0; x < bmp.Width; x++)
                        {
                            for (int y = 0; y < bmp.Height; y++)
                            {
                                // Get the color of the current pixel
                                Color pixelColor = bmp.GetPixel(x, y);
                                if (colorsToConvert.Contains(pixelColor))
                                {
                                    newBmp.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                                }
                                else
                                {
                                    newBmp.SetPixel(x, y, pixelColor);
                                }
                            }
                        }

                        // Replace the selected item with the new image in Art
                        Art.ReplaceStatic(selectedIndex, newBmp);
                        Options.ChangedUltimaClass["Art"] = true;
                        ControlEvents.FireItemChangeEvent(this, selectedIndex);

                        // Update pictureBoxOrg
                        pictureBoxOrg.BackgroundImage = newBmp;

                        // Refresh listBoxOrg to reflect the changes
                        listBoxOrg.Invalidate();
                    }
                }
                else
                {
                    MessageBox.Show("No image in the clipboard.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion
    }
}
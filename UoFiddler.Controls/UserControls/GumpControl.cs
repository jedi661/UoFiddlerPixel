/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Windows.Forms;
using System.Xml.Linq;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class GumpControl : UserControl
    {
        private Dictionary<string, string> idNames = new Dictionary<string, string>(); //XML

        #region [ GumpControl ]
        public GumpControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint,
                true);
            if (!Files.CacheData)
            {
                Preload.Visible = false;
            }

            ProgressBar.Visible = false;

            _refMarker = this;

            // Fügen Sie den listBox_KeyDown-Ereignishandler hinzu
            listBox.KeyDown += listBox_KeyDown;
        }
        #endregion

        private static GumpControl _refMarker;
        private bool _loaded;
        private bool _showFreeSlots;
        private GumpSearchForm _showForm; // _showForm
        private AddressListForm addressListForm; // To list the addresses

        #region [ Reload ]

        /// <summary>
        /// Reload when loaded (file changed)
        /// </summary> 
        private void Reload()
        {
            if (!_loaded)
            {
                return;
            }

            _loaded = false;
            OnLoad(EventArgs.Empty);
        }
        #endregion

        #region [ OnLoad ]
        protected override void OnLoad(EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (_loaded)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Gumps"] = true;
            _showFreeSlots = false;
            showFreeSlotsToolStripMenuItem.Checked = false;

            LoadIdNamesFromXml();

            PopulateListBox(true);

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.GumpChangeEvent += OnGumpChangeEvent;
            }

            _loaded = true;
            Cursor.Current = Cursors.Default;
        }
        #endregion

        #region [ LoadIdNamesFromXml ]
        private void LoadIdNamesFromXml()
        {
            // Path to XML file
            string xmlFilePath = Path.Combine(Application.StartupPath, "IDGumpNames.xml");

            XDocument doc;

            // Check if the XML file exists
            if (File.Exists(xmlFilePath))
            {
                // Load the XML file
                doc = XDocument.Load(xmlFilePath);
            }
            else
            {
                // Create a new XML file with the root element "IDNames"
                doc = new XDocument(new XElement("IDNames"));
                doc.Save(xmlFilePath);
            }

            foreach (XElement idElement in doc.Root.Elements("ID"))
            {
                string id = idElement.Attribute("value").Value;
                string name = idElement.Attribute("name").Value;

                // Add the name to the dictionary
                idNames[id] = name;
            }
        }
        #endregion

        #region [ PopulateListBox ]
        private void PopulateListBox(bool showOnlyValid)
        {
            listBox.BeginUpdate();
            listBox.Items.Clear();
            List<object> cache = new List<object>();
            int maxGumpID = 0;

            for (int i = 0; i < Gumps.GetCount(); i++)
            {
                if (Gumps.IsValidIndex(i))
                {
                    maxGumpID = i;
                }
            }

            for (int i = 0; i < maxGumpID; i++)
            {
                if (showOnlyValid && !_showFreeSlots)
                {
                    if (Gumps.IsValidIndex(i))
                    {
                        string name = idNames.TryGetValue(i.ToString(), out string idName) ? idName : "";
                        cache.Add($"{i} - {name}");
                    }
                }
                else
                {
                    string name = idNames.TryGetValue(i.ToString(), out string idName) ? idName : "";
                    cache.Add($"{i} - {name}");
                }
            }

            if (_showFreeSlots)
            {
                for (int i = maxGumpID + 1; i <= Gumps.GetCount(); i++)
                {
                    string name = idNames.TryGetValue(i.ToString(), out string idName) ? idName : "";
                    cache.Add($"{i} - {name}");
                }
            }

            listBox.Items.AddRange(cache.ToArray());
            listBox.EndUpdate();

            if (listBox.Items.Count > 0)
            {
                listBox.SelectedIndex = 0;
            }
            listBox.Refresh();
        }
        #endregion

        #region [ OnFilePathChangeEven ]
        private void OnFilePathChangeEvent()
        {
            Reload();
        }
        #endregion

        #region [ OnGumpChangeEvent ]
        private void OnGumpChangeEvent(object sender, int index)
        {
            if (!_loaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (Gumps.IsValidIndex(index))
            {
                bool done = false;
                for (int i = 0; i < listBox.Items.Count; ++i)
                {
                    int j = int.Parse(listBox.Items[i].ToString());
                    if (j > index)
                    {
                        listBox.Items.Insert(i, index);
                        listBox.SelectedIndex = i;
                        done = true;
                        break;
                    }

                    if (j == index)
                    {
                        done = true;
                        break;
                    }
                }

                if (!done)
                {
                    listBox.Items.Add(index);
                }
            }
            else
            {
                for (int i = 0; i < listBox.Items.Count; ++i)
                {
                    int j = int.Parse(listBox.Items[i].ToString());
                    if (j == index)
                    {
                        listBox.Items.RemoveAt(i);
                        break;
                    }
                }

                listBox.Invalidate();
            }
        }
        #endregion

        #region [ ListBox_DrawItem ]
        private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            Brush fontBrush = Brushes.Gray;

            string itemString = listBox.Items[e.Index].ToString();
            string idString = itemString.Split('-')[0].Trim();
            int i = int.Parse(idString);

            if (Gumps.IsValidIndex(i))
            {
                Bitmap bmp = Gumps.GetGump(i, out bool patched);

                if (bmp != null)
                {
                    int width = bmp.Width > 100 ? 100 : bmp.Width;
                    int height = bmp.Height > 54 ? 54 : bmp.Height;

                    if (listBox.SelectedIndex == e.Index)
                    {
                        e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds.X, e.Bounds.Y, 105, 60);
                    }
                    else if (patched)
                    {
                        e.Graphics.FillRectangle(Brushes.LightCoral, e.Bounds.X, e.Bounds.Y, 105, 60);
                    }

                    e.Graphics.DrawImage(bmp, new Rectangle(e.Bounds.X + 3, e.Bounds.Y + 3, width, height));
                }
                else
                {
                    fontBrush = Brushes.Red;
                }
            }
            else
            {
                if (listBox.SelectedIndex == e.Index)
                {
                    e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds.X, e.Bounds.Y, 105, 60);
                }

                fontBrush = Brushes.Red;
            }
            // Retrieving the name from the dictionary
            string name = idNames.TryGetValue(i.ToString(), out string idName) ? idName : "";

            // Drawing the ID, hex address and name
            e.Graphics.DrawString($"0x{i:X} ({i}) {name}", Font, fontBrush,
                new PointF(105,
                    e.Bounds.Y + ((e.Bounds.Height / 2) -
                                  (e.Graphics.MeasureString($"0x{i:X} ({i}) {name}", Font).Height / 2))));

            // To list the addresses
            if (e.Index == listBox.SelectedIndex)
            {
                string address = itemString.Split('-')[0].Trim();
                addressListForm?.AddAddress(address);
            }
        }
        #endregion

        #region [ SelectAddress ]
        public void SelectAddress(string address)
        {
            int addressIndex = listBox.Items.Cast<string>().ToList().FindIndex(item => item.StartsWith(address));
            if (addressIndex >= 0)
            {
                listBox.SelectedIndex = addressIndex;
                listBox.TopIndex = addressIndex;
            }
        }
        #endregion

        #region [ ListBox_MeasureItem ]
        private void ListBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 60;
        }
        #endregion

        #region [ ListBox_SelectedIndexChanged ]
        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex == -1)
            {
                return;
            }

            string itemString = listBox.Items[listBox.SelectedIndex].ToString();
            string idString = itemString.Split('-')[0].Trim();
            int i = int.Parse(idString);

            if (Gumps.IsValidIndex(i))
            {
                Bitmap bmp = Gumps.GetGump(i);

                if (bmp != null)
                {
                    pictureBox.BackgroundImage = bmp;
                    string name = idNames.TryGetValue(i.ToString(), out string idName) ? idName : "";
                    IDLabel.Text = $"ID: 0x{i:X} ({i}) {name}";
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
            JumpToMaleFemaleInvalidate();
        }
        #endregion

        #region [ JumpToMaleInvalistate ]
        private void JumpToMaleFemaleInvalidate()
        {
            if (listBox.SelectedIndex == -1)
            {
                return;
            }

            string itemString = listBox.SelectedItem.ToString();
            string idString = itemString.Split('-')[0].Trim();
            int gumpId = int.Parse(idString);
            if (gumpId >= 50000)
            {
                if (gumpId >= 60000)
                {
                    jumpToMaleFemale.Text = "Jump to Male";
                    jumpToMaleFemale.Enabled = HasGumpId(gumpId - 10000);
                }
                else
                {
                    jumpToMaleFemale.Text = "Jump to Female";
                    jumpToMaleFemale.Enabled = HasGumpId(gumpId + 10000);
                }
            }
            else
            {
                jumpToMaleFemale.Enabled = false;
                jumpToMaleFemale.Text = "Jump to Male/Female";
            }
        }
        #endregion

        #region [ OnClickReplace ]
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

                    // Split the string at the hyphen location and use only the first part
                    string itemString = listBox.Items[listBox.SelectedIndex].ToString();
                    string idString = itemString.Split('-')[0].Trim();
                    int i = int.Parse(idString);

                    Gumps.ReplaceGump(i, bitmap);

                    ControlEvents.FireGumpChangeEvent(this, i);

                    listBox.Invalidate();
                    ListBox_SelectedIndexChanged(this, EventArgs.Empty);

                    Options.ChangedUltimaClass["Gumps"] = true;

                    // Play sound if isSoundMessageActive is true
                    if (isSoundMessageActive)
                    {
                        if (playCustomSound)
                        {
                            player.Play();
                        }
                        else
                        {
                            player.SoundLocation = "sound.wav";
                            player.Play();
                        }
                    }
                }
            }
        }

        #endregion

        #region [ OnClickSave ]
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

        #region [ OnClickRemove ]
        private void OnClickRemove(object sender, EventArgs e)
        {
            // Split the string at the hyphen location and use only the first part
            string itemString = listBox.Items[listBox.SelectedIndex].ToString();
            string idString = itemString.Split('-')[0].Trim();
            int i = int.Parse(idString);

            DialogResult result = MessageBox.Show($"Are you sure to remove {i}", "Remove", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Gumps.RemoveGump(i);
            ControlEvents.FireGumpChangeEvent(this, i);
            if (!_showFreeSlots)
            {
                listBox.Items.RemoveAt(listBox.SelectedIndex);
            }

            pictureBox.BackgroundImage = null;
            listBox.Invalidate();
            Options.ChangedUltimaClass["Gumps"] = true;

            // Play sound if isSoundMessageActive is true
            if (isSoundMessageActive)
            {
                if (playCustomSound)
                {
                    player.Play();
                }
                else
                {
                    player.SoundLocation = "sound.wav";
                    player.Play();
                }
            }
        }
        #endregion

        #region [ OnClickFindFree ]
        private void OnClickFindFree(object sender, EventArgs e)
        {
            // Split the string at the hyphen location and use only the first part
            string itemString = listBox.Items[listBox.SelectedIndex].ToString();
            string idString = itemString.Split('-')[0].Trim();
            int id = int.Parse(idString);
            ++id;

            for (int i = listBox.SelectedIndex + 1; i < listBox.Items.Count; ++i, ++id)
            {
                itemString = listBox.Items[i].ToString();
                idString = itemString.Split('-')[0].Trim();
                if (id < int.Parse(idString))
                {
                    listBox.SelectedIndex = i;
                    break;
                }

                if (!_showFreeSlots)
                {
                    continue;
                }

                if (!Gumps.IsValidIndex(int.Parse(idString)))
                {
                    listBox.SelectedIndex = i;
                    break;
                }
            }

            // If no empty ID was found and _showFreeSlots is enabled,
            // a new ID is added to the end of the ListBox
            if (listBox.SelectedIndex == -1 && _showFreeSlots)
            {
                int newId = Gumps.GetCount();
                listBox.Items.Add(newId);
                listBox.SelectedIndex = listBox.Items.Count - 1;
            }

            // Play sound if isSoundMessageActive is true
            if (isSoundMessageActive)
            {
                if (playCustomSound)
                {
                    player.Play();
                }
                else
                {
                    player.SoundLocation = "sound.wav";
                    player.Play();
                }
            }
        }
        #endregion

        #region [ AddShowAllFreeSlotsButton ]
        private void AddShowAllFreeSlotsButton_Click(object sender, EventArgs e)
        {

            _showFreeSlots = !_showFreeSlots;
            PopulateListBox(!_showFreeSlots);
        }
        #endregion

        #region [ OnTextChanged_InsertAt ]
        private void OnTextChanged_InsertAt(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, Gumps.GetCount()))
            {
                InsertText.ForeColor = Gumps.IsValidIndex(index) ? Color.Red : Color.Black;
            }
            else
            {
                InsertText.ForeColor = Color.Red;
            }
        }
        #endregion

        #region [ OnKeydown_InserText ]
        private void OnKeydown_InsertText(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            // Split the string at the hyphen location and use only the first part
            string itemString = InsertText.Text;
            string idString = itemString.Split('-')[0].Trim();
            if (!Utils.ConvertStringToInt(idString, out int index, 0, Gumps.GetCount()))
            {
                return;
            }

            if (Gumps.IsValidIndex(index))
            {
                return;
            }

            contextMenuStrip.Close();
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = $"Choose image file to insert at 0x{index:X}";
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

                    Gumps.ReplaceGump(index, bitmap);

                    ControlEvents.FireGumpChangeEvent(this, index);

                    bool done = false;
                    for (int i = 0; i < listBox.Items.Count; ++i)
                    {
                        itemString = listBox.Items[i].ToString();
                        idString = itemString.Split('-')[0].Trim();
                        int j = int.Parse(idString);
                        if (j > index)
                        {
                            listBox.Items.Insert(i, index);
                            listBox.SelectedIndex = i;
                            done = true;
                            break;
                        }

                        if (!_showFreeSlots)
                        {
                            continue;
                        }

                        if (!Gumps.IsValidIndex(j))
                        {
                            listBox.SelectedIndex = i;
                            break;
                        }
                    }

                    if (!done)
                    {
                        listBox.Items.Add(index);
                        listBox.SelectedIndex = listBox.Items.Count - 1;
                    }

                    Options.ChangedUltimaClass["Gumps"] = true;
                }
            }
        }
        #endregion

        #region [ Extract_Image_ClickBmp ]
        private void Extract_Image_ClickBmp(object sender, EventArgs e)
        {
            string itemString = listBox.Items[listBox.SelectedIndex].ToString();
            string idString = itemString.Split('-')[0].Trim();
            if (Int32.TryParse(idString, out int i))
            {
                ExportGumpImage(i, ImageFormat.Bmp);
            }
        }
        #endregion

        #region [ Extract_Image_ClickTiff ]
        private void Extract_Image_ClickTiff(object sender, EventArgs e)
        {
            string itemString = listBox.Items[listBox.SelectedIndex].ToString();
            string idString = itemString.Split('-')[0].Trim();
            if (Int32.TryParse(idString, out int i))
            {
                ExportGumpImage(i, ImageFormat.Tiff);
            }
        }
        #endregion

        #region [ Extract_Image_ClickJpg ]
        private void Extract_Image_ClickJpg(object sender, EventArgs e)
        {
            string itemString = listBox.Items[listBox.SelectedIndex].ToString();
            string idString = itemString.Split('-')[0].Trim();
            if (Int32.TryParse(idString, out int i))
            {
                ExportGumpImage(i, ImageFormat.Jpeg);
            }
        }
        #endregion

        #region [ Extract_Image_ClickPng ]
        private void Extract_Image_ClickPng(object sender, EventArgs e)
        {
            string itemString = listBox.Items[listBox.SelectedIndex].ToString();
            string idString = itemString.Split('-')[0].Trim();
            if (Int32.TryParse(idString, out int i))
            {
                ExportGumpImage(i, ImageFormat.Png);
            }
        }
        #endregion

        #region [ ExportGumpImage ]
        private static void ExportGumpImage(int index, ImageFormat imageFormat)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"Gump {index}.{fileExtension}");

            using (Bitmap bit = new Bitmap(Gumps.GetGump(index)))
            {
                bit.Save(fileName, imageFormat);
            }

            MessageBox.Show(
                $"Gump saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region [ OnClick_SaveAllBmp ]
        private void OnClick_SaveAllBmp(object sender, EventArgs e)
        {
            ExportAllGumps(ImageFormat.Bmp);
        }
        #endregion

        #region [ OnClick_SaveAllTiff ]
        private void OnClick_SaveAllTiff(object sender, EventArgs e)
        {
            ExportAllGumps(ImageFormat.Tiff);
        }
        #endregion

        #region [ OnClick_SaveAllJpg ]
        private void OnClick_SaveAllJpg(object sender, EventArgs e)
        {
            ExportAllGumps(ImageFormat.Jpeg);
        }
        #endregion

        #region [ OnClick_SaveAllPng ]
        private void OnClick_SaveAllPng(object sender, EventArgs e)
        {
            ExportAllGumps(ImageFormat.Png);
        }
        #endregion

        #region [ ExportAllGumps ]
        private void ExportAllGumps(ImageFormat imageFormat)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                for (int i = 0; i < listBox.Items.Count; ++i)
                {
                    string itemString = listBox.Items[i].ToString();
                    string idString = itemString.Split('-')[0].Trim();
                    if (!Int32.TryParse(idString, out int index))
                    {
                        continue;
                    }
                    string fileName = Path.Combine(dialog.SelectedPath, $"Gump {index}.{fileExtension}");
                    using (Bitmap bit = new Bitmap(Gumps.GetGump(index)))
                    {
                        bit.Save(fileName, imageFormat);
                    }
                }
                MessageBox.Show($"All Gumps saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }
        #endregion

        #region [ OnClickShowFreeSlots ]
        private void OnClickShowFreeSlots(object sender, EventArgs e)
        {
            _showFreeSlots = !_showFreeSlots;
            PopulateListBox(!_showFreeSlots);
        }
        #endregion

        #region [ onClickPreload ]
        private void OnClickPreLoad(object sender, EventArgs e)
        {
            if (PreLoader.IsBusy)
            {
                return;
            }

            ProgressBar.Minimum = 1;
            ProgressBar.Maximum = Gumps.GetCount();
            ProgressBar.Step = 1;
            ProgressBar.Value = 1;
            ProgressBar.Visible = true;
            PreLoader.RunWorkerAsync();
        }

        private void PreLoaderDoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < Gumps.GetCount(); ++i)
            {
                Gumps.GetGump(i);
                PreLoader.ReportProgress(1);
            }
        }

        private void PreLoaderProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.PerformStep();
        }

        private void PreLoaderCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBar.Visible = false;
        }
        #endregion

        #region [ Static Void Select ]
        internal static void Select(int gumpId)
        {
            if (!_refMarker._loaded)
            {
                _refMarker.OnLoad(EventArgs.Empty);
            }

            _refMarker.Search(gumpId.ToString());
        }
        #endregion

        #region [ HasGumpId ]

        public static bool HasGumpId(int gumpId) // HasGumpId is cross-class to identify items on Gumps with it.
        {
            if (!_refMarker._loaded)
            {
                _refMarker.OnLoad(EventArgs.Empty);
            }

            return _refMarker.listBox.Items.Cast<object>().Any(id =>
            {
                if (int.TryParse(id.ToString().Split('-')[0].Trim(), out int intId))
                {
                    return intId == gumpId;
                }
                return false;
            });
        }
        #endregion

        #region [ JumpToMaleFemale_Click ]        
        private void JumpToMaleFemale_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex == -1)
            {
                return;
            }

            string itemString = listBox.SelectedItem.ToString();
            string idString = itemString.Split('-')[0].Trim();
            int gumpId = int.Parse(idString);

            gumpId = gumpId < 60000 ? (gumpId % 10000) + 60000 : (gumpId % 10000) + 50000;

            // Find the new gumpId in the listBox
            for (int i = 0; i < listBox.Items.Count; i++)
            {
                itemString = listBox.Items[i].ToString();
                idString = itemString.Split('-')[0].Trim();
                if (int.Parse(idString) == gumpId)
                {
                    listBox.SelectedIndex = i;
                    break;
                }
            }
        }
        #endregion

        #region [ JumpToMaleFemale2_Click ]
        // all in one Disabled
        // Here is a method that combines three functions but one of them is deactivated.
        // I left the function there as an example: 'JumpToMaleFemale, HasGumpId, JumpToMaleFemaleInvalidate'
        private void JumpToMaleFemale2_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex == -1)
            {
                return;
            }

            string itemString = listBox.SelectedItem.ToString();
            string idString = itemString.Split('-')[0].Trim();
            if (!int.TryParse(idString, out int gumpId))
            {
                MessageBox.Show("Invalid Gump ID format.");
                return;
            }

            gumpId = gumpId < 60000 ? (gumpId % 10000) + 60000 : (gumpId % 10000) + 50000;

            // Find the new gumpId in the listBox
            for (int i = 0; i < listBox.Items.Count; i++)
            {
                itemString = listBox.Items[i].ToString();
                idString = itemString.Split('-')[0].Trim();
                if (int.TryParse(idString, out int newGumpId) && newGumpId == gumpId)
                {
                    listBox.SelectedIndex = i;
                    break;
                }
            }

            // Update the button text and enabled state
            if (gumpId >= 50000)
            {
                if (gumpId >= 60000)
                {
                    jumpToMaleFemale.Text = "Jump to Male";
                    jumpToMaleFemale.Enabled = listBox.Items.Cast<object>().Any(id =>
                    {
                        if (int.TryParse(id.ToString(), out int intId))
                        {
                            return intId == gumpId - 10000;
                        }
                        return false;
                    });
                }
                else
                {
                    jumpToMaleFemale.Text = "Jump to Female";
                    jumpToMaleFemale.Enabled = listBox.Items.Cast<object>().Any(id =>
                    {
                        if (int.TryParse(id.ToString(), out int intId))
                        {
                            return intId == gumpId + 10000;
                        }
                        return false;
                    });
                }
            }
            else
            {
                jumpToMaleFemale.Enabled = false;
                jumpToMaleFemale.Text = "Jump to Male/Female";
            }
        }
        #endregion        

        #region [ SearchWrapper ]
        public bool SearchWrapper(int id)
        {
            return Search(id.ToString());
        }
        #endregion

        #region [ Search Click ]
        private void Search_Click(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = new GumpSearchForm(SearchWrapper) { TopMost = true };
            _showForm.Show();
        }
        #endregion

        #region [ Search ]
        public bool Search(string searchQuery)
        {
            if (!_refMarker._loaded)
            {
                _refMarker.OnLoad(EventArgs.Empty);
            }

            string searchQueryLower = searchQuery.ToLower();

            for (int i = 0; i < _refMarker.listBox.Items.Count; ++i)
            {
                string itemString = _refMarker.listBox.Items[i].ToString();
                string[] parts = itemString.Split('-');
                string idString = parts[0].Trim();
                string nameString = parts.Length > 1 ? parts[1].Trim() : "";

                // Check if the ID or name matches the search query
                if (idString.ToLower() == searchQueryLower || nameString.ToLower().Contains(searchQueryLower))
                {
                    _refMarker.listBox.SelectedIndex = i;
                    _refMarker.listBox.TopIndex = i;
                    return true;
                }

                // Check if the hex address matches the search query
                if (Int32.TryParse(idString, out int id))
                {
                    string hexId = id.ToString("x");
                    if ("0x" + hexId.ToLower() == searchQueryLower)
                    {
                        _refMarker.listBox.SelectedIndex = i;
                        _refMarker.listBox.TopIndex = i;
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion

        #region [ Gump_KeyUp ]
        private void Gump_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F || !e.Control)
            {
                return;
            }

            Search_Click(sender, e);
            e.SuppressKeyPress = true;
            e.Handled = true;
        }
        #endregion

        #region [ InsertStartingFromTb_KeyDown ]]
        private void InsertStartingFromTb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Int32.TryParse(InsertStartingFromTb.Text, out int index))
            {
                // Failed conversion, error handling here
                MessageBox.Show("Please enter a valid integer.");
                return;
            }

            contextMenuStrip.Close();
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = $"Choose image file to insert at 0x{index:X}";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var fileCount = dialog.FileNames.Length;
                if (CheckForIndexes(index, fileCount))
                {
                    for (int i = 0; i < fileCount; i++)
                    {
                        var currentIdx = index + i;
                        AddSingleGump(dialog.FileNames[i], currentIdx);
                    }

                    Search((index + (fileCount - 1)).ToString());

                }
            }

            Options.ChangedUltimaClass["Gumps"] = true;
        }
        #endregion

        #region [ CheckForIndexes ]
        /// <summary>
        /// Check if all the indexes from baseIndex to baseIndex + count are valid
        /// </summary>
        /// <param name="baseIndex">Starting Index</param>
        /// <param name="count">Number of the indexes to check.</param>
        /// <returns></returns>

        private bool CheckForIndexes(int baseIndex, int count)
        {
            for (int i = baseIndex; i < baseIndex + count; i++)
            {
                if (i >= Gumps.GetCount() || Gumps.IsValidIndex(i))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region [ AddSingleGump ]

        /// <summary>
        /// Adds a single Gump.
        /// </summary>
        /// <param name="fileName">Filename of the gump to add</param>
        /// <param name="index">Index where the gump shall be added.</param>
        private void AddSingleGump(string fileName, int index)
        {
            using (var bmpTemp = new Bitmap(fileName))
            {
                Bitmap bitmap = new Bitmap(bmpTemp);
                if (fileName.Contains(".bmp"))
                {
                    bitmap = Utils.ConvertBmp(bitmap);
                }
                Gumps.ReplaceGump(index, bitmap);
                ControlEvents.FireGumpChangeEvent(this, index);
                bool done = false;
                for (int i = 0; i < listBox.Items.Count; ++i)
                {
                    string itemString = listBox.Items[i].ToString();
                    string idString = itemString.Split('-')[0].Trim();
                    if (!Int32.TryParse(idString, out int j))
                    {
                        continue;
                    }
                    if (j > index)
                    {
                        listBox.Items.Insert(i, index);
                        listBox.SelectedIndex = i;
                        done = true;
                        break;
                    }
                    if (!_showFreeSlots)
                    {
                        continue;
                    }
                    if (j != i)
                    {
                        continue;
                    }
                    done = true;
                    break;
                }
                if (!done)
                {
                    listBox.Items.Add(index);
                    listBox.SelectedIndex = listBox.Items.Count - 1;
                }
            }
        }
        #endregion

        #region [ copyToolStripMenuItem ]
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex != -1)
            {
                string itemString = listBox.Items[listBox.SelectedIndex].ToString();
                string idString = itemString.Split('-')[0].Trim();
                if (Int32.TryParse(idString, out int i) && Gumps.IsValidIndex(i))
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

                        // Play sound if isSoundMessageActive is true
                        if (isSoundMessageActive)
                        {
                            if (playCustomSound)
                            {
                                player.Play();
                            }
                            else
                            {
                                player.SoundLocation = "sound.wav";
                                player.Play();
                            }
                        }

                        // Only show a message if isSoundMessageActive is false
                        if (!isSoundMessageActive)
                        {
                            MessageBox.Show("The image has been copied to the clipboard!");
                        }
                    }
                    else
                    {
                        // Only show a message if isSoundMessageActive is false
                        if (!isSoundMessageActive)
                        {
                            MessageBox.Show("No image to copy!");
                        }
                    }
                }
                else
                {
                    // Only show a message if isSoundMessageActive is false
                    if (!isSoundMessageActive)
                    {
                        MessageBox.Show("No image to copy!");
                    }
                }
            }
            else
            {
                // Only show a message if isSoundMessageActive is false
                if (!isSoundMessageActive)
                {
                    MessageBox.Show("No image to copy!");
                }
            }
        }
        #endregion

        #region [ importToolStripMenuItem = Import Import clipboard - Import graphics from clipboard ]
        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if the clipboard contains an image
            if (Clipboard.ContainsImage())
            {
                // Retrieve the image from the clipboard
                using (Bitmap bmp = new Bitmap(Clipboard.GetImage()))
                {
                    // Determine the position of the selected graphic in the listBox.
                    string itemString = listBox.Items[listBox.SelectedIndex].ToString();
                    string idString = itemString.Split('-')[0].Trim();
                    if (Int32.TryParse(idString, out int index) && index >= 0 && index < Gumps.GetCount())
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

                        // Play sound if isSoundMessageActive is true
                        if (isSoundMessageActive)
                        {
                            if (playCustomSound)
                            {
                                player.Play();
                            }
                            else
                            {
                                player.SoundLocation = "sound.wav";
                                player.Play();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid index.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("No image in the clipboard.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        #region [ SearchByIdToolStripTextBox_KeyUp ]
        private void SearchByIdToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            var searchQuery = searchByIdToolStripTextBox.Text;
            Search(searchQuery);
            if (e.KeyCode == Keys.Return)
            {
                _refMarker.listBox.Focus();
            }
        }

        #endregion

        #region [ addIDNamesToolStripMenuItem = Add Id Names Form ]
        private void addIDNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Create a new shape
            Form form = new Form
            {
                Width = 300,
                Height = 200,
                Text = "Add ID name"
            };

            // Create a TextBox for the ID
            TextBox idTextBox = new TextBox
            {
                Location = new Point(10, 10),
                Width = 200,
                Text = listBox.SelectedItem?.ToString().Split('-')[0].Trim() ?? "" //Set the text to the selected ID in the ListBox
            };
            // Add an event handler to ignore non-numeric input
            idTextBox.KeyPress += (sender, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                }
            };
            form.Controls.Add(idTextBox);

            // Create a TextBox for the name
            TextBox nameTextBox = new TextBox
            {
                Location = new Point(10, 40),
                Width = 200,
                Text = idNames.TryGetValue(idTextBox.Text, out string existingName) ? existingName : "" // Set the text to the existing name if one exists
            };
            form.Controls.Add(nameTextBox);

            // Create an OK button
            Button okButton = new Button
            {
                Text = "OK",
                Location = new Point(10, 70)
            };

            okButton.Click += (sender, e) =>
            {
                string id = idTextBox.Text;
                string name = nameTextBox.Text;

                // Update the XML file
                UpdateIdNameInXml(id, name);

                // Update the ListBox
                PopulateListBox(true);

                form.Close();
            };

            form.Controls.Add(okButton);

            // Create a delete button
            Button deleteButton = new Button
            {
                Text = "Delete",
                Location = new Point(okButton.Location.X + okButton.Width + 3, 70) // Position the button to the right of the OK button
            };

            deleteButton.Click += (sender, e) =>
            {
                string id = idTextBox.Text;

                // Delete the entry from the XML file
                UpdateIdNameInXml(id, "");

                form.Close();
            };

            form.Controls.Add(deleteButton);

            // Select the nameTextBox when the form is shown
            form.Shown += (sender, e) => nameTextBox.Select();

            // Display the shape
            form.ShowDialog();
        }
        #endregion

        #region [ UpdateIdNameInXml ]
        private void UpdateIdNameInXml(string id, string name)
        {
            // Pfad zur XML-Datei
            string xmlFilePath = Path.Combine(Application.StartupPath, "IDGumpNames.xml");

            XDocument doc = XDocument.Load(xmlFilePath);

            XElement idElement = doc.Root.Elements("ID")
                                        .FirstOrDefault(el => el.Attribute("value").Value == id);

            if (idElement != null)
            {
                idElement.Attribute("name").Value = name;
            }
            else
            {
                XElement newIdElement = new XElement("ID",
                    new XAttribute("value", id),
                    new XAttribute("name", name));

                doc.Root.Add(newIdElement);
            }

            // Save the changes to the XML file
            doc.Save(xmlFilePath);

            // Update the dictionary
            idNames[id] = name;
        }
        #endregion

        #region [ Sound Button ]
        private bool isSoundMessageActive = false;
        private bool playCustomSound = false; // You can use it to select any sound
        private SoundPlayer player = new SoundPlayer();

        public void toolStripButtonSoundMessage_Click(object sender, EventArgs e)
        {
            // Toggle the state of isSoundMessageActive
            isSoundMessageActive = !isSoundMessageActive;

            // Change the background color of the button based on its state
            if (isSoundMessageActive)
            {
                // Change the background color to blue when the button is active
                toolStripButtonSoundMessage.BackColor = Color.Blue;

                // Play sound
                if (playCustomSound)
                {
                    player.Play();
                }
                else
                {
                    player.SoundLocation = "sound.wav";
                    player.Play();
                }
            }
            else
            {
                // Change the background color to default when the button is not active
                toolStripButtonSoundMessage.BackColor = default(Color);
            }
        }
        #endregion

        #region [ Mark ]
        private string lastSelectedId = "50320"; // Default value

        #region listBox_MouseDouvleClick
        private void listBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Save the selected ID value when an item in the ListBox is double-clicked
            string itemString = listBox.SelectedItem.ToString();
            lastSelectedId = itemString.Split('-')[0].Trim();

            if (isSoundMessageActive)
            {
                if (playCustomSound)
                {
                    player.Play();
                }
                else
                {
                    player.SoundLocation = "sound.wav";
                    player.Play();
                }
            }
        }
        #endregion

        #region Mark
        private void markToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listBox.Items.Count; ++i)
            {
                string itemString = listBox.Items[i].ToString();
                string idString = itemString.Split('-')[0].Trim();

                if (idString == lastSelectedId)
                {
                    listBox.SelectedIndex = i;
                    break;
                }
            }

            if (isSoundMessageActive)
            {
                if (playCustomSound)
                {
                    player.Play();
                }
                else
                {
                    player.SoundLocation = "sound.wav";
                    player.Play();
                }
            }
        }
        #endregion

        #endregion

        #region [ CoustomSound ]
        private void customSoundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Wave Sound File (*.wav)|*.wav";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                player.SoundLocation = openFileDialog.FileName;
                playCustomSound = true;
            }
        }
        #endregion

        #region [ PreviewKeyDown // Pagedown and up ]
        private void listBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Next:  // PageDown
                case Keys.Prior: // PageUp
                    e.IsInputKey = true;
                    break;
            }
        }
        #endregion

        #region [ second keydown as a replacement ]
        private void listBox_KeyDown(object sender, KeyEventArgs e)
        {
            int visibleItems = listBox.ClientSize.Height / listBox.ItemHeight;

            switch (e.KeyCode)
            {
                case Keys.Next:  // PageDown
                    if (listBox.SelectedIndex + visibleItems < listBox.Items.Count)
                    {
                        listBox.SelectedIndex += visibleItems;
                    }
                    else
                    {
                        listBox.SelectedIndex = listBox.Items.Count - 1;
                    }
                    break;

                case Keys.Prior: // PageUp
                    if (listBox.SelectedIndex - visibleItems >= 0)
                    {
                        listBox.SelectedIndex -= visibleItems;
                    }
                    else
                    {
                        listBox.SelectedIndex = 0;
                    }
                    break;
            }
        }
        #endregion

        #region [ exportAllIDsToTextToolStripMenuItem_Click ]
        private void exportAllIDsToTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportAllGumpIDs();
        }
        #endregion

        #region [ ExportAllGumpIDs ]
        private void ExportAllGumpIDs()
        {
            var format = MessageBox.Show("What format do you want to export in? Decimal?", "Select format", MessageBoxButtons.YesNo) == DialogResult.Yes ? "Decimal" : "Hex";
            var includeFree = MessageBox.Show("Would you also like to export the free IDs?", "Include free IDs", MessageBoxButtons.YesNo) == DialogResult.Yes;

            // Create the default filename with "GumpExport" and current date
            string defaultFileName = $"GumpExport_{DateTime.Now:yyyyMMdd}.txt";

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                Title = "Gump IDs save",
                FileName = defaultFileName  // Set the default file name
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    int maxGumpID = Gumps.GetCount();  // Get the maximum Gump ID from the Gump mul file
                    for (int gumpId = 0; gumpId < maxGumpID; gumpId++)
                    {
                        bool isOccupied = Gumps.IsValidIndex(gumpId);  // Check whether the Gump entry is occupied

                        // Skip free IDs if the user doesn't want to include them
                        if (!isOccupied && !includeFree)
                            continue;

                        // Convert the ID to the desired format (decimal or hexadecimal)
                        string formattedID = format == "Decimal" ? gumpId.ToString() : $"0x{gumpId:X}";
                        writer.WriteLine($"{formattedID} - {(isOccupied ? "Gump Image" : "Free")}");
                    }
                }

                MessageBox.Show("The export was successfully completed!", "Export status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region [ importAllImagesFromTextToolStripMenuItem ] 
        private void importAllImagesFromTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select the directory with the text file and Gump images";

                if (folderDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string selectedDirectory = folderDialog.SelectedPath;

                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt",
                    Title = "Select the text file with the Gump addresses",
                    InitialDirectory = selectedDirectory
                };

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string textFilePath = openFileDialog.FileName;

                // Read the text file
                string[] lines = File.ReadAllLines(textFilePath);

                foreach (string line in lines)
                {
                    string[] parts = line.Split('-');
                    if (parts.Length < 1)
                    {
                        continue;
                    }

                    string gumpAddress = parts[0].Trim();
                    int gumpId;

                    // Recognize hex or decimal
                    if (gumpAddress.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!int.TryParse(gumpAddress.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out gumpId))
                        {
                            MessageBox.Show($"Invalid hex address: {gumpAddress}");
                            continue;
                        }
                    }
                    else if (!int.TryParse(gumpAddress, out gumpId))
                    {
                        MessageBox.Show($"Invalid decimal address: {gumpAddress}");
                        continue;
                    }

                    string imagePath = FindImageForGumpId(selectedDirectory, gumpAddress);
                    if (string.IsNullOrEmpty(imagePath))
                    {
                        MessageBox.Show($"No image found for Gump ID: {gumpAddress}");
                        continue;
                    }

                    // Convert to BMP if necessary
                    using (var bmpTemp = new Bitmap(imagePath))
                    {
                        Bitmap bitmap = new Bitmap(bmpTemp);

                        if (imagePath.Contains(".bmp"))
                        {
                            bitmap = Utils.ConvertBmp(bitmap);
                        }

                        // Import the image to the Gump ID
                        Gumps.ReplaceGump(gumpId, bitmap);

                        // Enable GumpChangeEvent to ensure the ListBox is updated
                        ControlEvents.FireGumpChangeEvent(this, gumpId);

                        // Update the ListBox
                        UpdateListBoxWithGump(gumpId);
                    }
                }

                PopulateListBox(false); // Here the ListBox is reloaded after the import

                // Force redraw the ListBox
                listBox.Invalidate();
                MessageBox.Show("Import complete!", "Import status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region [ FindImageForGumpId ]
        // Auxiliary method for image search
        private string FindImageForGumpId(string directory, string gumpAddress)
        {
            string[] imageFormats = { "bmp", "png", "jpg", "jpeg", "tif", "tiff" };
            foreach (var format in imageFormats)
            {
                string filePath = Path.Combine(directory, $"{gumpAddress}.{format}");
                if (File.Exists(filePath))
                {
                    return filePath;
                }
            }

            return null;
        }
        #endregion

        #region [ UpdateListBoxWithGump ]
        // ListBox update
        private void UpdateListBoxWithGump(int gumpId)
        {
            bool alreadyExists = false;
            for (int i = 0; i < listBox.Items.Count; i++)
            {
                string item = listBox.Items[i].ToString().Split('-')[0].Trim();
                if (int.TryParse(item, System.Globalization.NumberStyles.HexNumber, null, out int listGumpId) && listGumpId == gumpId)
                {
                    listBox.Items[i] = $"0x{gumpId:X} - Replaced with new Gump";
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists)
            {
                listBox.Items.Add($"0x{gumpId:X} - Replaced with new Gump");
            }

            listBox.Refresh();
        }
        #endregion

        #region [ mirrorToolStripMenuItem ]
        private void mirrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox.BackgroundImage == null)
            {
                MessageBox.Show("No image available to mirror.");
                return;
            }

            Bitmap originalImage = new Bitmap(pictureBox.BackgroundImage);

            Bitmap mirroredBitmap = new Bitmap(originalImage.Width, originalImage.Height);

            using (Graphics g = Graphics.FromImage(mirroredBitmap))
            {
                g.DrawImage(originalImage, new Rectangle(0, 0, originalImage.Width, originalImage.Height),
                    new Rectangle(originalImage.Width, 0, -originalImage.Width, originalImage.Height), GraphicsUnit.Pixel);
            }

            pictureBox.BackgroundImage = mirroredBitmap;
        }
        #endregion

        #region [ listingToolStripMenuItem_Click ]
        private void listingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (addressListForm == null || addressListForm.IsDisposed)
            {
                addressListForm = new AddressListForm();
                addressListForm.AddressSelected += AddressListForm_AddressSelected;
                addressListForm.Show();
            }
        }
        #endregion

        #region [ AddressListForm_AddressSelected ]
        private void AddressListForm_AddressSelected(object sender, string address)
        {
            SelectAddress(address);
        }
        #endregion

        #region [ class AddressListForm : Form]
        public class AddressListForm : Form
        {
            private ListBox addressListBox;
            public event EventHandler<string> AddressSelected;

            public AddressListForm()
            {
                Initialize();
            }

            private void Initialize()
            {
                addressListBox = new ListBox() { Dock = DockStyle.Fill };
                addressListBox.MouseDoubleClick += (sender, e) =>
                {
                    if (addressListBox.SelectedItem != null)
                    {
                        string selectedAddress = addressListBox.SelectedItem.ToString();
                        AddressSelected?.Invoke(this, selectedAddress);
                    }
                };
                Controls.Add(addressListBox);
            }

            public void AddAddress(string address)
            {
                if (!addressListBox.Items.Contains(address))
                {
                    addressListBox.Items.Add(address);
                }
            }
        }
        #endregion
    }
}
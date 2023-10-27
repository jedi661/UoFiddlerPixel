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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Ultima;
using Ultima.Helpers;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class RadarColorControl : UserControl
    {
        public RadarColorControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            _refMarker = this;

            // Link the event method to the AfterSelect event.
            treeViewItem.AfterSelect += treeViewItem_AfterSelect;
            treeViewLand.AfterSelect += treeViewLand_AfterSelect;

            //this.treeViewLand = new UoFiddler.Controls.UserControls.RadarColorControl.ColorTreeView();

            // Label Colorcode
            LabelColorCode.Text = "";
        }

        private int _selectedIndex = -1;
        private ushort _currentColor;
        private static RadarColorControl _refMarker;
        private bool _updating;

        public bool IsLoaded { get; private set; }

        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ushort CurrentColor
        {
            get => _currentColor;
            set
            {
                if (_currentColor == value)
                {
                    return;
                }

                _currentColor = value;
                _updating = true;
                numericUpDownShortCol.Value = _currentColor;
                Color color = HueHelpers.HueToColor(_currentColor);
                pictureBoxColor.BackColor = color;
                numericUpDownR.Value = color.R;
                numericUpDownG.Value = color.G;
                numericUpDownB.Value = color.B;
                _updating = false;
            }
        }

        public static void Select(int graphic, bool land)
        {
            if (!_refMarker.IsLoaded)
            {
                _refMarker.OnLoad(_refMarker, EventArgs.Empty);
            }

            const int index = 0;
            if (land)
            {
                for (int i = index; i < _refMarker.treeViewLand.Nodes.Count; ++i)
                {
                    TreeNode node = _refMarker.treeViewLand.Nodes[i];
                    if ((int)node.Tag != graphic)
                    {
                        continue;
                    }

                    _refMarker.tabControl2.SelectTab(1);
                    _refMarker.treeViewLand.SelectedNode = node;
                    node.EnsureVisible();
                    break;
                }
            }
            else
            {
                for (int i = index; i < _refMarker.treeViewItem.Nodes.Count; ++i)
                {
                    TreeNode node = _refMarker.treeViewItem.Nodes[i];
                    if ((int)node.Tag != graphic)
                    {
                        continue;
                    }

                    _refMarker.tabControl2.SelectTab(0);
                    _refMarker.treeViewItem.SelectedNode = node;
                    node.EnsureVisible();
                    break;
                }
            }
        }

        private void Reload()
        {
            if (IsLoaded)
            {
                OnLoad(this, new MyEventArgs(MyEventArgs.Types.ForceReload));
            }
        }

        #region Onload        
        private BackgroundWorker _worker = new BackgroundWorker();

        public void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (IsLoaded && (!(e is MyEventArgs args) || args.Type != MyEventArgs.Types.ForceReload))
            {
                return;
            }

            // Configure the background worker.
            _worker.DoWork += Worker_DoWork;
            _worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            // Start the background worker.
            _worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Load the data here.
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["RadarColor"] = true;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Update the user control here.
            treeViewItem.BeginUpdate();
            try
            {
                treeViewItem.Nodes.Clear();
                if (TileData.ItemTable != null)
                {
                    TreeNode[] nodes = new TreeNode[Art.GetMaxItemId()];
                    for (int i = 0; i < Art.GetMaxItemId(); ++i)
                    {
                        ushort color = RadarCol.GetItemColor(i);
                        Color foreColor = HueHelpers.HueToColor(color);
                        nodes[i] = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", i, TileData.ItemTable[i].Name))
                        {
                            Tag = i,
                            ForeColor = foreColor
                        };
                    }
                    treeViewItem.Nodes.AddRange(nodes);
                }
            }
            finally
            {
                treeViewItem.EndUpdate();
            }

            // Load treeViewLand when the control is loaded
            treeViewLand.BeginUpdate();
            try
            {
                treeViewLand.Nodes.Clear();
                if (TileData.LandTable != null)
                {
                    TreeNode[] nodes = new TreeNode[TileData.LandTable.Length];
                    for (int i = 0; i < TileData.LandTable.Length; ++i)
                    {
                        ushort color = RadarCol.GetLandColor(i);
                        Color foreColor = HueHelpers.HueToColor(color);
                        nodes[i] = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", i, TileData.LandTable[i].Name))
                        {
                            Tag = i,
                            ForeColor = foreColor
                        };
                    }
                    treeViewLand.Nodes.AddRange(nodes);
                }
            }
            finally
            {
                treeViewLand.EndUpdate();
            }

            if (!IsLoaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            IsLoaded = true;
        }

        //Update TreeViewItems
        private void UpdateTreeViewItem()
        {
            treeViewItem.BeginUpdate();
            try
            {
                treeViewItem.Nodes.Clear();
                if (TileData.ItemTable != null)
                {
                    TreeNode[] nodes = new TreeNode[Art.GetMaxItemId()];
                    for (int i = 0; i < Art.GetMaxItemId(); ++i)
                    {
                        ushort color = RadarCol.GetItemColor(i);
                        Color foreColor = HueHelpers.HueToColor(color);
                        nodes[i] = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", i, TileData.ItemTable[i].Name))
                        {
                            Tag = i,
                            ForeColor = foreColor
                        };
                    }
                    treeViewItem.Nodes.AddRange(nodes);
                }
            }
            finally
            {
                treeViewItem.EndUpdate();
            }
        }
        private void btupdateTreeView_Click(object sender, EventArgs e)
        {
            // Update the treeViewItem data.
            UpdateTreeViewItem();
        }
        #endregion


        #region  New ColorTree erstellt Grafikbalken
        public class ColorTreeView : TreeView
        {
            public ColorTreeView()
            {
                DrawMode = TreeViewDrawMode.OwnerDrawText;
            }

            protected override void OnDrawNode(DrawTreeNodeEventArgs e)
            {
                // Draw a colored bar.
                int colorBoxWidth = 16;
                int colorBoxHeight = 16;
                int colorBoxOffset = 2;
                Rectangle colorBoxRect = new Rectangle(e.Bounds.X + colorBoxOffset, e.Bounds.Y + colorBoxOffset, colorBoxWidth, colorBoxHeight);

                Color color = e.Node.ForeColor; // Use the predefined foreground color of the node.

                e.Graphics.FillRectangle(new SolidBrush(color), colorBoxRect);

                // Draw the node text.
                int textOffset = colorBoxWidth + 2 * colorBoxOffset;
                Rectangle textRect = new Rectangle(e.Bounds.X + textOffset, e.Bounds.Y, e.Bounds.Width - textOffset, e.Bounds.Height);
                TextRenderer.DrawText(e.Graphics, e.Node.Text, this.Font, textRect, this.ForeColor, TextFormatFlags.GlyphOverhangPadding);
            }
        }
        #endregion

        #region Click farbcode
        private void treeViewItem_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!checkBoxHexCode.Checked)
            {
                return;
            }

            // Get the selected element.
            TreeNode selectedNode = e.Node;

            // Get the color code of the selected element.
            ushort color = RadarCol.GetItemColor((int)selectedNode.Tag);
            string colorCode = $"0x{color:X4}";

            // Copy the color code to the clipboard.
            Clipboard.SetText(colorCode);
        }

        private void treeViewLand_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!checkBoxHexCode.Checked) //Checkbox checkBoxHexCode
            {
                return;
            }

            // Get the selected element.
            TreeNode selectedNode = e.Node;

            // Get the color code of the selected element.
            ushort color = RadarCol.GetLandColor((int)selectedNode.Tag);
            string colorCode = $"0x{color:X4}";

            // Copy the color code to the clipboard.
            Clipboard.SetText(colorCode);
        }
        #endregion

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void AfterSelectTreeViewItem(object sender, TreeViewEventArgs e)
        {
            _selectedIndex = (int)e.Node.Tag;

            if (Art.IsValidStatic(_selectedIndex))
            {
                Bitmap bitmap = Art.GetStatic(_selectedIndex);
                Bitmap newBitmap = new Bitmap(pictureBoxArt.Size.Width, pictureBoxArt.Size.Height);
                using (Graphics newGraphic = Graphics.FromImage(newBitmap))
                {
                    newGraphic.Clear(Color.FromArgb(-1));
                    newGraphic.DrawImage(bitmap, (pictureBoxArt.Size.Width - bitmap.Width) / 2, 1);
                }

                pictureBoxArt.Image = newBitmap;
            }
            else
            {
                pictureBoxArt.Image = new Bitmap(pictureBoxArt.Width, pictureBoxArt.Height);
            }

            CurrentColor = RadarCol.GetItemColor(_selectedIndex);
            // Update the LabelTildataNameItemsLand label with the name of the selected item.
            LabelTildataNameItemsLand.Text = TileData.ItemTable[_selectedIndex].Name;
        }

        private void AfterSelectTreeViewLand(object sender, TreeViewEventArgs e)
        {
            _selectedIndex = (int)e.Node.Tag;

            if (Art.IsValidLand(_selectedIndex))
            {
                Bitmap bitmap = Art.GetLand(_selectedIndex);
                Bitmap newBitmap = new Bitmap(pictureBoxArt.Size.Width, pictureBoxArt.Size.Height);
                using (Graphics newGraphic = Graphics.FromImage(newBitmap))
                {
                    newGraphic.Clear(Color.FromArgb(-1));
                    newGraphic.DrawImage(bitmap, (pictureBoxArt.Size.Width - bitmap.Width) / 2, 1);
                }

                pictureBoxArt.Image = newBitmap;
            }
            else
            {
                pictureBoxArt.Image = new Bitmap(pictureBoxArt.Width, pictureBoxArt.Height);
            }

            CurrentColor = RadarCol.GetLandColor(_selectedIndex);
            // Update the LabelTildataNameItemsLand label with the name of the selected item.
            LabelTildataNameItemsLand.Text = TileData.LandTable[_selectedIndex].Name;
        }

        private void OnClickMeanColor(object sender, EventArgs e)
        {
            Bitmap image = tabControl2.SelectedIndex == 0 ? Art.GetStatic(_selectedIndex) : Art.GetLand(_selectedIndex);
            if (image == null)
            {
                return;
            }

            CurrentColor = HueHelpers.ColorToHue(AverageColorFrom(image));
        }

        private void OnClickSaveFile(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "radarcol.mul");
            RadarCol.Save(fileName);
            MessageBox.Show($"RadarCol saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["RadarCol"] = false;
        }

        private void OnClickSaveColor(object sender, EventArgs e)
        {
            if (_selectedIndex < 0)
            {
                return;
            }

            if (tabControl2.SelectedIndex == 0)
            {
                RadarCol.SetItemColor(_selectedIndex, CurrentColor);
                if (treeViewItem.SelectedNode != null) //Colors the index directly
                {
                    treeViewItem.SelectedNode.ForeColor = HueHelpers.HueToColor(CurrentColor);
                }
            }
            else
            {
                RadarCol.SetLandColor(_selectedIndex, CurrentColor);
                if (treeViewLand.SelectedNode != null) //Colors the index directly
                {
                    treeViewLand.SelectedNode.ForeColor = HueHelpers.HueToColor(CurrentColor);
                }
            }

            LabelColorCode.Text = $"Index {_selectedIndex}, Color Code: 0x{CurrentColor:X4}"; // LabelColorCode

            Options.ChangedUltimaClass["RadarCol"] = true;
        }

        private void OnChangeR(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }

            Color col = Color.FromArgb((int)numericUpDownR.Value, (int)numericUpDownG.Value, (int)numericUpDownB.Value);
            CurrentColor = HueHelpers.ColorToHue(col);
        }

        private void OnChangeG(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }

            Color col = Color.FromArgb((int)numericUpDownR.Value, (int)numericUpDownG.Value, (int)numericUpDownB.Value);
            CurrentColor = HueHelpers.ColorToHue(col);
        }

        private void OnChangeB(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }

            Color col = Color.FromArgb((int)numericUpDownR.Value, (int)numericUpDownG.Value, (int)numericUpDownB.Value);
            CurrentColor = HueHelpers.ColorToHue(col);
        }

        private void OnNumericShortColChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                CurrentColor = (ushort)numericUpDownShortCol.Value;

                int value = (int)numericUpDownShortCol.Value;
                string hexValue = value.ToString("X4");
                textBoxHexCode.Text = hexValue;
            }
        }

        public class HexNumericUpDown : NumericUpDown
        {
            public HexNumericUpDown()
            {
                // Set the minimum and maximum values to meaningful values for hexadecimal values.
                Minimum = 0;
                Maximum = ushort.MaxValue;
            }

            protected override void UpdateEditText()
            {
                // Display the current value in hexadecimal format.
                Text = ((int)Value).ToString("X4");
            }

            protected override void ValidateEditText()
            {
                // Try to interpret the text as a hexadecimal value.
                if (int.TryParse(Text, System.Globalization.NumberStyles.HexNumber, null, out int value))
                {
                    // Ensure that the value is within the allowable range.
                    value = Math.Max((int)Minimum, Math.Min((int)Maximum, value));

                    // Set the value.
                    Value = value;
                }
                else
                {
                    // If the text does not represent a valid hexadecimal value, reset it to the current value.
                    UpdateEditText();
                }
            }
        }


        private void OnClickMeanColorFromTo(object sender, EventArgs e)
        {
            if (!Utils.ConvertStringToInt(textBoxMeanFrom.Text, out int from, 0, 0x4000) ||
                !Utils.ConvertStringToInt(textBoxMeanTo.Text, out int to, 0, 0x4000))
            {
                return;
            }

            if (to < from)
            {
                int temp = from;
                from = to;
                to = temp;
            }

            int gmeanr = 0;
            int gmeang = 0;
            int gmeanb = 0;

            for (int i = from; i < to; ++i)
            {
                Bitmap image = tabControl2.SelectedIndex == 0 ? Art.GetStatic(i) : Art.GetLand(i);
                if (image == null)
                {
                    continue;
                }

                unsafe
                {
                    BitmapData bd = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                    ushort* line = (ushort*)bd.Scan0;
                    int delta = bd.Stride >> 1;
                    ushort* cur = line;

                    int meanr = 0;
                    int meang = 0;
                    int meanb = 0;

                    int count = 0;
                    for (int y = 0; y < image.Height; ++y, line += delta)
                    {
                        cur = line;
                        for (int x = 0; x < image.Width; ++x)
                        {
                            if (cur[x] != 0)
                            {
                                meanr += HueHelpers.HueToColorR(cur[x]);
                                meang += HueHelpers.HueToColorG(cur[x]);
                                meanb += HueHelpers.HueToColorB(cur[x]);
                                ++count;
                            }
                        }
                    }
                    image.UnlockBits(bd);

                    meanr /= count;
                    meang /= count;
                    meanb /= count;

                    gmeanr += meanr;
                    gmeang += meang;
                    gmeanb += meanb;
                }
            }

            gmeanr /= to - from;
            gmeang /= to - from;
            gmeanb /= to - from;

            Color col = Color.FromArgb(gmeanr, gmeang, gmeanb);
            CurrentColor = HueHelpers.ColorToHue(col);
        }

        private void OnClickSelectItemsTab(object sender, EventArgs e)
        {
            if (treeViewItem.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            var found = ItemsControl.SearchGraphic(index);
            if (!found)
            {
                MessageBox.Show("You need to load Items tab first.", "Information");
            }
        }

        private void OnClickSelectItemTiledataTab(object sender, EventArgs e)
        {
            if (treeViewItem.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            TileDataControl.Select(index, false);
        }

        private void OnClickSelectLandTilesTab(object sender, EventArgs e)
        {
            if (treeViewLand.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            var found = LandTilesControl.SearchGraphic(index);
            if (!found)
            {
                MessageBox.Show("You need to load LandTiles tab first.", "Information");
            }
        }

        private void OnClickSelectLandTiledataTab(object sender, EventArgs e)
        {
            if (treeViewLand.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            TileDataControl.Select(index, true);
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Choose csv file to import",
                CheckFileExists = true,
                Filter = "csv files (*.csv)|*.csv"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Options.ChangedUltimaClass["RadarCol"] = true;
                RadarCol.ImportFromCSV(dialog.FileName);
                if (tabControl2.SelectedTab == tabControl2.TabPages[0])
                {
                    if (treeViewItem.SelectedNode != null)
                    {
                        AfterSelectTreeViewItem(this, new TreeViewEventArgs(treeViewItem.SelectedNode));
                    }
                }
                else
                {
                    if (treeViewLand.SelectedNode != null)
                    {
                        AfterSelectTreeViewLand(this, new TreeViewEventArgs(treeViewLand.SelectedNode));
                    }
                }
            }
            dialog.Dispose();
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "RadarColor.csv");
            RadarCol.ExportToCSV(fileName);
            MessageBox.Show($"RadarColor saved to {fileName}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickMeanColorAll(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Do you want to calculate and set new radar color values for all items and land tiles entries where current color is black or missing?",
                "Average All",
                MessageBoxButtons.YesNo
                );

            if (result != DialogResult.Yes)
            {
                return;
            }

            if (TileData.ItemTable != null)
            {
                int itemsLength = Art.GetMaxItemId();
                progressBar1.Maximum = itemsLength;

                for (int i = 0; i < itemsLength; ++i)
                {
                    progressBar1.Value++;
                    if (!Art.IsValidStatic(i))
                    {
                        continue;
                    }

                    if (RadarCol.GetItemColor(i) != 0)
                    {
                        continue;
                    }

                    Bitmap image = Art.GetStatic(i);
                    if (image == null)
                    {
                        continue;
                    }

                    var currentColor = HueHelpers.ColorToHue(AverageColorFrom(image));
                    RadarCol.SetItemColor(i, currentColor);
                    Options.ChangedUltimaClass["RadarCol"] = true;
                }
            }

            if (TileData.LandTable != null)
            {
                int landLength = TileData.LandTable.Length;
                progressBar2.Maximum = landLength;
                for (int i = 0; i < landLength; ++i)
                {
                    progressBar2.Value++;
                    if (!Art.IsValidLand(i))
                    {
                        continue;
                    }

                    if (RadarCol.GetLandColor(i) != 0)
                    {
                        continue;
                    }

                    Bitmap image = Art.GetLand(i);
                    if (image == null)
                    {
                        continue;
                    }

                    var currentColor = HueHelpers.ColorToHue(AverageColorFrom(image));
                    RadarCol.SetLandColor(i, currentColor);
                    Options.ChangedUltimaClass["RadarCol"] = true;
                }
            }

            MessageBox.Show("Done!", "Average All");

            progressBar1.Value = 0;
            progressBar2.Value = 0;
        }

        private unsafe Color AverageColorFrom(Bitmap image)
        {
            BitmapData bd = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;
            ushort* cur = line;

            int meanR = 0;
            int meanG = 0;
            int meanB = 0;

            int count = 0;
            for (int y = 0; y < image.Height; ++y, line += delta)
            {
                cur = line;
                for (int x = 0; x < image.Width; ++x)
                {
                    if (cur[x] == 0)
                    {
                        continue;
                    }

                    meanR += HueHelpers.HueToColorR(cur[x]);
                    meanG += HueHelpers.HueToColorG(cur[x]);
                    meanB += HueHelpers.HueToColorB(cur[x]);
                    ++count;
                }
            }
            image.UnlockBits(bd);

            if (count > 0)
            {
                meanR /= count;
                meanG /= count;
                meanB /= count;
            }

            return Color.FromArgb(meanR, meanG, meanB);
        }

        #region Hex and Photoshop
        private void textBoxHexCode_TextChanged(object sender, EventArgs e)
        {

            string hexValue = textBoxHexCode.Text;

            // Check if the entered value is a valid hexadecimal value.


            if (int.TryParse(hexValue, System.Globalization.NumberStyles.HexNumber, null, out int value))
            {
                // Ensure that the value stays within the valid range of numericUpDownShortCol.
                value = Math.Max((int)numericUpDownShortCol.Minimum, value);
                value = Math.Min((int)numericUpDownShortCol.Maximum, value);

                // Update numericUpDownShortCol.
                numericUpDownShortCol.Value = value;

                // Update the color code for Photoshop.
                UpdatePhotoshopCode();
            }
            else
            {
                // If the value is invalid, reset textBoxHexCode to the current value of numericUpDownShortCol.
                int currentValue = (int)numericUpDownShortCol.Value;
                string currentHexValue = currentValue.ToString("X4");
                textBoxHexCode.Text = currentHexValue;
            }
        }
        private void textBoxHexCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow control characters (e.g., Backspace).
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Allow only hexadecimal characters (0-9, a-f, A-F).
            if (!Uri.IsHexDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private string ConvertToHexColor(int value)
        {
            // Ensure that the value stays within the valid range of 0 to 32767.
            value = Math.Max(0, value);
            value = Math.Min(32767, value);

            // Extract the intensities of the red, green, and blue color channels.
            int red = (value >> 10) & 0x1F;
            int green = (value >> 5) & 0x1F;
            int blue = value & 0x1F;

            // Scale the intensities to the range of 0 to 255.
            red = (red * 255) / 31;
            green = (green * 255) / 31;
            blue = (blue * 255) / 31;

            // Convert the intensities to a hexadecimal value.
            string hexValue = string.Format("{0:X2}{1:X2}{2:X2}", red, green, blue);

            return hexValue;
        }
        private void UpdatePhotoshopCode()
        {
            // Get the current value from numericUpDownShortCol.
            int value = (int)numericUpDownShortCol.Value;

            // Convert the value to a valid 6-digit hexadecimal color code.
            string hexValue = ConvertToHexColor(value);

            // Display the calculated color code in the textBoxPhotoshopCode.
            textBoxPhotoshopCode.Text = hexValue;
        }

        private void textBoxPhotoshopCode_Click(object sender, EventArgs e)
        {
            // Create a new ToolTip control.
            ToolTip toolTip = new ToolTip();

            // Get the text from the textBoxPhotoshopCode.
            string text = textBoxPhotoshopCode.Text;

            // Check if the text is not empty.
            if (!string.IsNullOrEmpty(text))
            {
                // Copy the text from the textBoxPhotoshopCode to the clipboard.
                Clipboard.SetText(text);

                // Display a message using the ToolTip control.
                toolTip.Show($"Copied '{text}' to clipboard", textBoxPhotoshopCode);
            }
        }

        #endregion
    }
}

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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.ConstrainedExecution;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class TileDataControl : UserControl
    {
        public TileDataControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            // Add the presets to the ToolStripComboBox
            toolStripComboBox1.Items.Add("Wall"); //1
            toolStripComboBox1.Items.Add("Door-E-L"); //2
            toolStripComboBox1.Items.Add("Door-E-R"); //3
            toolStripComboBox1.Items.Add("Door-S-L"); //4
            toolStripComboBox1.Items.Add("Door-S-R"); //5
            toolStripComboBox1.Items.Add("Window"); //6
            toolStripComboBox1.Items.Add("Roof"); //7
            toolStripComboBox1.Items.Add("Floor"); //8
            toolStripComboBox1.Items.Add("Water"); //9
            toolStripComboBox1.Items.Add("Stairs-W"); //10
            toolStripComboBox1.Items.Add("Stairs-N"); //11
            toolStripComboBox1.Items.Add("Stairs-E"); //12
            toolStripComboBox1.Items.Add("Stairs-S"); //13
            toolStripComboBox1.Items.Add("Stairs-W-N"); //14
            toolStripComboBox1.Items.Add("Stairs-N-E"); //15
            toolStripComboBox1.Items.Add("Stairs-E-S"); //16
            toolStripComboBox1.Items.Add("Stairs-S-W"); //17
            toolStripComboBox1.Items.Add("Stairs-Block"); //18
            toolStripComboBox1.Items.Add("Container"); //19
            toolStripComboBox1.Items.Add("Lamp Post"); //20
            toolStripComboBox1.Items.Add("Fence"); //21
            toolStripComboBox1.Items.Add("Cave Wall"); //22
            toolStripComboBox1.Items.Add("Clothing"); //23
            toolStripComboBox1.Items.Add("Plant"); //24
            toolStripComboBox1.Items.Add("Chair-Small-5"); //25
            toolStripComboBox1.Items.Add("Chair-35"); //26
            toolStripComboBox1.Items.Add("Chair-Wood-8"); //27
            toolStripComboBox1.Items.Add("Chair-Throne-15"); //28
            // checkedListBox2
            toolStripComboBox1.Items.Add("LandTile Land"); //29
            toolStripComboBox1.Items.Add("LandTile Water"); //30
            toolStripComboBox1.Items.Add("LandTile Mountain"); //31
            // checkedListBox2 End
            //Tree
            toolStripComboBox1.Items.Add("Tree"); //32
            //Clear
            toolStripComboBox1.Items.Add("Clear"); //33
            // Add an event handler for toolStripComboBox1's SelectedIndexChanged event
            toolStripComboBox1.SelectedIndexChanged += ToolStripComboBox1_SelectedIndexChanged;

            _refMarker = this;

            treeViewItem.BeforeSelect += TreeViewItemOnBeforeSelect;

            saveDirectlyOnChangesToolStripMenuItem.Checked = Options.TileDataDirectlySaveOnChange;
            saveDirectlyOnChangesToolStripMenuItem.CheckedChanged += SaveDirectlyOnChangesToolStripMenuItemOnCheckedChanged;

            ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            ControlEvents.TileDataChangeEvent += OnTileDataChangeEvent;

            LabelDecimalAdress.Text = "";
        }

        private void InitLandTilesFlagsCheckBoxes()
        {
            checkedListBox2.BeginUpdate();
            try
            {
                checkedListBox2.Items.Clear();

                string[] enumNames = Enum.GetNames(typeof(TileFlag));
                int maxLength = Art.IsUOAHS() ? enumNames.Length : (enumNames.Length / 2) + 1;
                for (int i = 1; i < maxLength; ++i)
                {
                    checkedListBox2.Items.Add(enumNames[i], false);
                }

                // TODO: for now we present all flags. Needs research if landtiles have only selected flags or all of them?
                // TODO: looks like only small subset is used but it is still different then these 5 below
                //checkedListBox2.Items.Add(Enum.GetName(typeof(TileFlag), TileFlag.Damaging), false);
                //checkedListBox2.Items.Add(Enum.GetName(typeof(TileFlag), TileFlag.Wet), false);
                //checkedListBox2.Items.Add(Enum.GetName(typeof(TileFlag), TileFlag.Impassable), false);
                //checkedListBox2.Items.Add(Enum.GetName(typeof(TileFlag), TileFlag.Wall), false);
                //checkedListBox2.Items.Add(Enum.GetName(typeof(TileFlag), TileFlag.NoDiagonal), false);
            }
            finally
            {
                checkedListBox2.EndUpdate();
            }
        }
        #region ItemControl Select PiictureBox refresh
        public void RefreshPictureBoxItem()
        {
            pictureBoxItem.Refresh();
        }
        #endregion
        private void InitItemsFlagsCheckBoxes()
        {
            checkedListBox1.BeginUpdate();
            try
            {
                checkedListBox1.Items.Clear();

                string[] enumNames = Enum.GetNames(typeof(TileFlag));
                int maxLength = Art.IsUOAHS() ? enumNames.Length : (enumNames.Length / 2) + 1;
                for (int i = 1; i < maxLength; ++i)
                {
                    checkedListBox1.Items.Add(enumNames[i], false);
                }
            }
            finally
            {
                checkedListBox1.EndUpdate();
            }
        }

        private static TileDataControl _refMarker;
        private bool _changingIndex;

        public bool IsLoaded { get; private set; }

        private int? _reselectGraphic;
        private bool? _reselectGraphicLand;

        public static void Select(int graphic, bool land)
        {
            if (!_refMarker.IsLoaded)
            {
                _refMarker.OnLoad(_refMarker, EventArgs.Empty);
                _refMarker._reselectGraphic = graphic;
                _refMarker._reselectGraphicLand = land;
            }

            SearchGraphic(graphic, land);
        }

        public static bool SearchGraphic(int graphic, bool land)
        {
            const int index = 0;
            if (land)
            {
                for (int i = index; i < _refMarker.treeViewLand.Nodes.Count; ++i)
                {
                    TreeNode node = _refMarker.treeViewLand.Nodes[i];
                    if (node.Tag == null || (int)node.Tag != graphic)
                    {
                        continue;
                    }

                    _refMarker.tabcontrol.SelectTab(1);
                    _refMarker.treeViewLand.SelectedNode = node;
                    node.EnsureVisible();
                    return true;
                }
            }
            else
            {
                for (int i = index; i < _refMarker.treeViewItem.Nodes.Count; ++i)
                {
                    for (int j = 0; j < _refMarker.treeViewItem.Nodes[i].Nodes.Count; ++j)
                    {
                        TreeNode node = _refMarker.treeViewItem.Nodes[i].Nodes[j];
                        if (node.Tag == null || (int)node.Tag != graphic)
                        {
                            continue;
                        }

                        _refMarker.tabcontrol.SelectTab(0);
                        _refMarker.treeViewItem.SelectedNode = node;
                        node.EnsureVisible();
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool SearchName(string name, bool next, bool land)
        {
            int index = 0;

            var searchMethod = SearchHelper.GetSearchMethod();

            if (land)
            {
                if (next)
                {
                    if (_refMarker.treeViewLand.SelectedNode.Index >= 0)
                    {
                        index = _refMarker.treeViewLand.SelectedNode.Index + 1;
                    }

                    if (index >= _refMarker.treeViewLand.Nodes.Count)
                    {
                        index = 0;
                    }
                }

                for (int i = index; i < _refMarker.treeViewLand.Nodes.Count; ++i)
                {
                    TreeNode node = _refMarker.treeViewLand.Nodes[i];
                    if (node.Tag == null)
                    {
                        continue;
                    }

                    var searchResult = searchMethod(name, TileData.LandTable[(int)node.Tag].Name);
                    if (!searchResult.EntryFound)
                    {
                        continue;
                    }

                    _refMarker.tabcontrol.SelectTab(1);
                    _refMarker.treeViewLand.SelectedNode = node;
                    node.EnsureVisible();
                    return true;
                }
            }
            else
            {
                int sIndex = 0;
                if (next && _refMarker.treeViewItem.SelectedNode != null)
                {
                    if (_refMarker.treeViewItem.SelectedNode.Parent != null)
                    {
                        index = _refMarker.treeViewItem.SelectedNode.Parent.Index;
                        sIndex = _refMarker.treeViewItem.SelectedNode.Index + 1;
                    }
                    else
                    {
                        index = _refMarker.treeViewItem.SelectedNode.Index;
                        sIndex = 0;
                    }
                }

                for (int i = index; i < _refMarker.treeViewItem.Nodes.Count; ++i)
                {
                    for (int j = sIndex; j < _refMarker.treeViewItem.Nodes[i].Nodes.Count; ++j)
                    {
                        TreeNode node = _refMarker.treeViewItem.Nodes[i].Nodes[j];
                        if (node.Tag == null)
                        {
                            continue;
                        }

                        var searchResult = searchMethod(name, TileData.ItemTable[(int)node.Tag].Name);
                        if (!searchResult.EntryFound)
                        {
                            continue;
                        }

                        _refMarker.tabcontrol.SelectTab(0);
                        _refMarker.treeViewItem.SelectedNode = node;
                        node.EnsureVisible();
                        return true;
                    }

                    sIndex = 0;
                }
            }

            return false;
        }

        public void ApplyFilterItem(ItemData item)
        {
            treeViewItem.BeginUpdate();
            treeViewItem.Nodes.Clear();

            var nodes = new List<TreeNode>();
            var nodesSa = new List<TreeNode>();
            var nodesHsa = new List<TreeNode>();

            for (int i = 0; i < TileData.ItemTable.Length; ++i)
            {
                if (!string.IsNullOrEmpty(item.Name) && TileData.ItemTable[i].Name.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                if (item.Animation != 0 && TileData.ItemTable[i].Animation != item.Animation)
                {
                    continue;
                }

                if (item.Weight != 0 && TileData.ItemTable[i].Weight != item.Weight)
                {
                    continue;
                }

                if (item.Quality != 0 && TileData.ItemTable[i].Quality != item.Quality)
                {
                    continue;
                }

                if (item.Quantity != 0 && TileData.ItemTable[i].Quantity != item.Quantity)
                {
                    continue;
                }

                if (item.Hue != 0 && TileData.ItemTable[i].Hue != item.Hue)
                {
                    continue;
                }

                if (item.StackingOffset != 0 && TileData.ItemTable[i].StackingOffset != item.StackingOffset)
                {
                    continue;
                }

                if (item.Value != 0 && TileData.ItemTable[i].Value != item.Value)
                {
                    continue;
                }

                if (item.Height != 0 && TileData.ItemTable[i].Height != item.Height)
                {
                    continue;
                }

                if (item.MiscData != 0 && TileData.ItemTable[i].MiscData != item.MiscData)
                {
                    continue;
                }

                if (item.Unk2 != 0 && TileData.ItemTable[i].Unk2 != item.Unk2)
                {
                    continue;
                }

                if (item.Unk3 != 0 && TileData.ItemTable[i].Unk3 != item.Unk3)
                {
                    continue;
                }

                if (item.Flags != 0 && (TileData.ItemTable[i].Flags & item.Flags) == 0)
                {
                    continue;
                }

                TreeNode node = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", i, TileData.ItemTable[i].Name))
                {
                    Tag = i
                };

                if (i < 0x4000)
                {
                    nodes.Add(node);
                }
                else if (i < 0x8000)
                {
                    nodesSa.Add(node);
                }
                else
                {
                    nodesHsa.Add(node);
                }
            }

            if (nodes.Count > 0)
            {
                treeViewItem.Nodes.Add(new TreeNode("AOS - ML", nodes.ToArray()));
            }

            if (nodesSa.Count > 0)
            {
                treeViewItem.Nodes.Add(new TreeNode("Stygian Abyss", nodesSa.ToArray()));
            }

            if (nodesHsa.Count > 0)
            {
                treeViewItem.Nodes.Add(new TreeNode("Adventures High Seas", nodesHsa.ToArray()));
            }

            treeViewItem.EndUpdate();

            if (treeViewItem.Nodes.Count > 0 && _refMarker.treeViewItem.Nodes[0].Nodes.Count > 0)
            {
                treeViewItem.SelectedNode = _refMarker.treeViewItem.Nodes[0].Nodes[0];
            }
        }

        public static void ApplyFilterLand(LandData land)
        {
            _refMarker.treeViewLand.BeginUpdate();
            _refMarker.treeViewLand.Nodes.Clear();
            var nodes = new List<TreeNode>();
            for (int i = 0; i < TileData.LandTable.Length; ++i)
            {
                if (!string.IsNullOrEmpty(land.Name) && TileData.ItemTable[i].Name.IndexOf(land.Name, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                if (land.TextureId != 0 && TileData.LandTable[i].TextureId != land.TextureId)
                {
                    continue;
                }

                if (land.Flags != 0 && (TileData.LandTable[i].Flags & land.Flags) == 0)
                {
                    continue;
                }

                TreeNode node = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", i, TileData.LandTable[i].Name))
                {
                    Tag = i
                };
                nodes.Add(node);
            }

            _refMarker.treeViewLand.Nodes.AddRange(nodes.ToArray());
            _refMarker.treeViewLand.EndUpdate();

            if (_refMarker.treeViewLand.Nodes.Count > 0)
            {
                _refMarker.treeViewLand.SelectedNode = _refMarker.treeViewLand.Nodes[0];
            }
        }

        private void Reload()
        {
            if (IsLoaded)
            {
                OnLoad(this, new MyEventArgs(MyEventArgs.Types.ForceReload));
            }
        }

        public void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (_reselectGraphic != null && _reselectGraphicLand != null)
            {
                SearchGraphic(_reselectGraphic.Value, _reselectGraphicLand.Value);
                _reselectGraphic = null;
                _reselectGraphicLand = null;
            }

            if (IsLoaded && (!(e is MyEventArgs args) || args.Type != MyEventArgs.Types.ForceReload))
            {
                return;
            }

            InitItemsFlagsCheckBoxes();
            InitLandTilesFlagsCheckBoxes();

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            treeViewItem.BeginUpdate();
            treeViewItem.Nodes.Clear();
            if (TileData.ItemTable != null)
            {
                var nodes = new TreeNode[0x4000];
                for (int i = 0; i < 0x4000; ++i)
                {
                    nodes[i] = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", i, TileData.ItemTable[i].Name))
                    {
                        Tag = i
                    };
                }
                treeViewItem.Nodes.Add(new TreeNode("AOS - ML", nodes));

                if (TileData.ItemTable.Length > 0x4000) // SA
                {
                    nodes = new TreeNode[0x4000];
                    for (int i = 0; i < 0x4000; ++i)
                    {
                        int j = i + 0x4000;
                        nodes[i] = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", j, TileData.ItemTable[j].Name))
                        {
                            Tag = j
                        };
                    }
                    treeViewItem.Nodes.Add(new TreeNode("Stygian Abyss", nodes));
                }

                if (TileData.ItemTable.Length > 0x8000) // AHS
                {
                    nodes = new TreeNode[0x8000];
                    for (int i = 0; i < 0x8000; ++i)
                    {
                        int j = i + 0x8000;
                        nodes[i] = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", j, TileData.ItemTable[j].Name))
                        {
                            Tag = j
                        };
                    }
                    treeViewItem.Nodes.Add(new TreeNode("Adventures High Seas", nodes));
                }
                else
                {
                    treeViewItem.ExpandAll();
                }
            }
            treeViewItem.EndUpdate();

            treeViewLand.BeginUpdate();
            treeViewLand.Nodes.Clear();
            if (TileData.LandTable != null)
            {
                var nodes = new TreeNode[TileData.LandTable.Length];
                for (int i = 0; i < TileData.LandTable.Length; ++i)
                {
                    nodes[i] = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", i, TileData.LandTable[i].Name))
                    {
                        Tag = i
                    };
                }
                treeViewLand.Nodes.AddRange(nodes);
            }
            treeViewLand.EndUpdate();

            IsLoaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnTileDataChangeEvent(object sender, int index)
        {
            if (!IsLoaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (index > 0x3FFF) // items
            {
                if (treeViewItem.SelectedNode == null)
                {
                    return;
                }

                if ((int)treeViewItem.SelectedNode.Tag == index)
                {
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    AfterSelectTreeViewItem(this, new TreeViewEventArgs(treeViewItem.SelectedNode));
                }
                else
                {
                    foreach (TreeNode parentNode in treeViewItem.Nodes)
                    {
                        foreach (TreeNode node in parentNode.Nodes)
                        {
                            if ((int)node.Tag != index)
                            {
                                continue;
                            }

                            node.ForeColor = Color.Red;
                            break;
                        }
                    }
                }
            }
            else
            {
                if (treeViewLand.SelectedNode == null)
                {
                    return;
                }

                if ((int)treeViewLand.SelectedNode.Tag == index)
                {
                    treeViewLand.SelectedNode.ForeColor = Color.Red;
                    AfterSelectTreeViewLand(this, new TreeViewEventArgs(treeViewLand.SelectedNode));
                }
                else
                {
                    foreach (TreeNode node in treeViewLand.Nodes)
                    {
                        if ((int)node.Tag != index)
                        {
                            continue;
                        }

                        node.ForeColor = Color.Red;
                        break;
                    }
                }
            }
        }

        private void AfterSelectTreeViewItem(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag == null)
            {
                return;
            }

            int index = (int)e.Node.Tag;

            Bitmap bit = Art.GetStatic(index);
            if (bit != null)
            {
                Bitmap newBit = new Bitmap(pictureBoxItem.Size.Width, pictureBoxItem.Size.Height);
                using (Graphics newGraph = Graphics.FromImage(newBit))
                {
                    newGraph.Clear(Color.FromArgb(-1));
                    newGraph.DrawImage(bit, (pictureBoxItem.Size.Width - bit.Width) / 2, 1);
                }

                pictureBoxItem.Image?.Dispose();
                pictureBoxItem.Image = newBit;

                originalImage = pictureBoxItem.Image; // Update the original image -> Zoom
                UpdateZoomFormImage(); // Update Zoom -> Form
            }
            else
            {
                pictureBoxItem.Image = null;
            }

            ItemData data = TileData.ItemTable[index];
            _changingIndex = true;
            textBoxName.Text = data.Name;
            textBoxAnim.Text = data.Animation.ToString();
            textBoxWeight.Text = data.Weight.ToString();
            textBoxQuality.Text = data.Quality.ToString();
            textBoxQuantity.Text = data.Quantity.ToString();
            textBoxHue.Text = data.Hue.ToString();
            textBoxStackOff.Text = data.StackingOffset.ToString();
            textBoxValue.Text = data.Value.ToString();
            textBoxHeigth.Text = data.Height.ToString();
            textBoxUnk1.Text = data.MiscData.ToString();
            textBoxUnk2.Text = data.Unk2.ToString();
            textBoxUnk3.Text = data.Unk3.ToString();

            Array enumValues = Enum.GetValues(typeof(TileFlag));
            int maxLength = Art.IsUOAHS() ? enumValues.Length : (enumValues.Length / 2) + 1;
            for (int i = 1; i < maxLength; ++i)
            {
                checkedListBox1.SetItemChecked(i - 1, (data.Flags & (TileFlag)enumValues.GetValue(i)) != 0);
            }
            _changingIndex = false;
        }

        private void AfterSelectTreeViewLand(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
            {
                return;
            }

            int index = (int)e.Node.Tag;

            LabelDecimalAdress.Text = index.ToString();

            Bitmap bit = Art.GetLand(index);
            if (bit != null)
            {
                Bitmap newBit = new Bitmap(pictureBoxLand.Size.Width, pictureBoxLand.Size.Height);
                using (Graphics newGraph = Graphics.FromImage(newBit))
                {
                    newGraph.Clear(Color.FromArgb(-1));
                    newGraph.DrawImage(bit, (pictureBoxLand.Size.Width - bit.Width) / 2, 1);
                }

                pictureBoxLand.Image?.Dispose();
                pictureBoxLand.Image = newBit;
            }
            else
            {
                pictureBoxLand.Image = null;
            }

            LandData data = TileData.LandTable[index];
            _changingIndex = true;
            textBoxNameLand.Text = data.Name;
            textBoxTexID.Text = data.TextureId.ToString();

            Array enumValues = Enum.GetValues(typeof(TileFlag));
            int maxLength = Art.IsUOAHS() ? enumValues.Length : (enumValues.Length / 2) + 1;
            for (int i = 1; i < maxLength; ++i)
            {
                checkedListBox2.SetItemChecked(i - 1, (data.Flags & (TileFlag)enumValues.GetValue(i)) != 0);
            }

            _changingIndex = false;
        }

        private void OnClickSaveTiledata(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "tiledata.mul");
            TileData.SaveTileData(fileName);
            MessageBox.Show($"TileData saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["TileData"] = false;
        }

        private void OnClickSaveChanges(object sender, EventArgs e)
        {
            if (tabcontrol.SelectedIndex == 0) // items
            {
                if (treeViewItem.SelectedNode?.Tag == null)
                {
                    return;
                }

                int index = (int)treeViewItem.SelectedNode.Tag;
                ItemData item = TileData.ItemTable[index];
                string name = textBoxName.Text;
                if (name.Length > 20)
                {
                    name = name.Substring(0, 20);
                }

                item.Name = name;
                treeViewItem.SelectedNode.Text = string.Format("0x{0:X4} ({0}) {1}", index, name);
                if (short.TryParse(textBoxAnim.Text, out short shortRes))
                {
                    item.Animation = shortRes;
                }

                if (byte.TryParse(textBoxWeight.Text, out byte byteRes))
                {
                    item.Weight = byteRes;
                }

                if (byte.TryParse(textBoxQuality.Text, out byteRes))
                {
                    item.Quality = byteRes;
                }

                if (byte.TryParse(textBoxQuantity.Text, out byteRes))
                {
                    item.Quantity = byteRes;
                }

                if (byte.TryParse(textBoxHue.Text, out byteRes))
                {
                    item.Hue = byteRes;
                }

                if (byte.TryParse(textBoxStackOff.Text, out byteRes))
                {
                    item.StackingOffset = byteRes;
                }

                if (byte.TryParse(textBoxValue.Text, out byteRes))
                {
                    item.Value = byteRes;
                }

                if (byte.TryParse(textBoxHeigth.Text, out byteRes))
                {
                    item.Height = byteRes;
                }

                if (short.TryParse(textBoxUnk1.Text, out shortRes))
                {
                    item.MiscData = shortRes;
                }

                if (byte.TryParse(textBoxUnk2.Text, out byteRes))
                {
                    item.Unk2 = byteRes;
                }

                if (byte.TryParse(textBoxUnk3.Text, out byteRes))
                {
                    item.Unk3 = byteRes;
                }

                item.Flags = TileFlag.None;
                Array enumValues = Enum.GetValues(typeof(TileFlag));
                for (int i = 0; i < checkedListBox1.Items.Count; ++i)
                {
                    if (checkedListBox1.GetItemChecked(i))
                    {
                        item.Flags |= (TileFlag)enumValues.GetValue(i + 1);
                    }
                }
                TileData.ItemTable[index] = item;
                treeViewItem.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);

                if (toolStripButton7IsActive && memorySaveWarningToolStripMenuItem.Checked)
                {
                    if (playCustomSound) //Land Tiles no MessageBox 
                    {
                        SoundPlayer player = new SoundPlayer();
                        player.SoundLocation = "sound.wav";
                        player.Play();
                    }
                    else //new
                        MessageBox.Show(
                        string.Format(
                            "Edits of 0x{0:X4} ({0}) saved to memory. Click 'Save Tiledata' to write to file.", index),
                        "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }

            }
            else // land
            {
                if (treeViewLand.SelectedNode == null)
                {
                    return;
                }

                int index = (int)treeViewLand.SelectedNode.Tag;
                LandData land = TileData.LandTable[index];
                string name = textBoxNameLand.Text;
                if (name.Length > 20)
                {
                    name = name.Substring(0, 20);
                }

                land.Name = name;
                treeViewLand.SelectedNode.Text = $"0x{index:X4} {name}";
                if (ushort.TryParse(textBoxTexID.Text, out ushort shortRes))
                {
                    land.TextureId = shortRes;
                }

                land.Flags = TileFlag.None;
                Array enumValues = Enum.GetValues(typeof(TileFlag));
                for (int i = 0; i < checkedListBox2.Items.Count; ++i)
                {
                    if (checkedListBox2.GetItemChecked(i))
                    {
                        land.Flags |= (TileFlag)enumValues.GetValue(i + 1);
                    }
                }

                TileData.LandTable[index] = land;
                Options.ChangedUltimaClass["TileData"] = true;
                ControlEvents.FireTileDataChangeEvent(this, index);
                treeViewLand.SelectedNode.ForeColor = Color.Red;

                if (toolStripButton7IsActive && memorySaveWarningToolStripMenuItem.Checked)
                {
                    if (playCustomSound) //Land Tiles no MessageBox 
                    {
                        SoundPlayer player = new SoundPlayer();
                        player.SoundLocation = "sound.wav";
                        player.Play();
                    }
                    else //new
                        MessageBox.Show(
                        string.Format(
                            "Edits of 0x{0:X4} ({0}) saved to memory. Click 'Save Tiledata' to write to file.", index),
                        "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void SaveDirectlyOnChangesToolStripMenuItemOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            Options.TileDataDirectlySaveOnChange = saveDirectlyOnChangesToolStripMenuItem.Checked;
        }

        private void OnTextChangedItemAnim(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            if (!short.TryParse(textBoxAnim.Text, out short shortRes))
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            item.Animation = shortRes;
            TileData.ItemTable[index] = item;
            treeViewItem.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemName(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            string name = textBoxName.Text;
            if (name.Length == 0)
            {
                return;
            }

            if (name.Length > 20)
            {
                name = name.Substring(0, 20);
            }

            item.Name = name;

            TileData.ItemTable[index] = item;
            treeViewItem.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void TreeViewItemOnBeforeSelect(object sender, TreeViewCancelEventArgs treeViewCancelEventArgs)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];

            string itemText = string.Format("0x{0:X4} ({0}) {1}", index, item.Name);
            if (treeViewItem.SelectedNode.Text != itemText)
            {
                treeViewItem.SelectedNode.Text = string.Format("0x{0:X4} ({0}) {1}", index, item.Name);
            }
        }

        private void OnTextChangedItemWeight(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            if (!byte.TryParse(textBoxWeight.Text, out byte byteRes))
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            item.Weight = byteRes;
            TileData.ItemTable[index] = item;
            treeViewItem.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemQuality(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            if (!byte.TryParse(textBoxQuality.Text, out byte byteRes))
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            item.Quality = byteRes;
            TileData.ItemTable[index] = item;
            treeViewItem.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemQuantity(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            if (!byte.TryParse(textBoxQuantity.Text, out byte byteRes))
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            item.Quantity = byteRes;
            TileData.ItemTable[index] = item;
            treeViewItem.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemHue(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            if (!byte.TryParse(textBoxHue.Text, out byte byteRes))
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            item.Hue = byteRes;
            TileData.ItemTable[index] = item;
            treeViewItem.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemStackOff(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            if (!byte.TryParse(textBoxStackOff.Text, out byte byteRes))
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            item.StackingOffset = byteRes;
            TileData.ItemTable[index] = item;
            treeViewItem.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemValue(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            if (!byte.TryParse(textBoxValue.Text, out byte byteRes))
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            item.Value = byteRes;
            TileData.ItemTable[index] = item;
            treeViewItem.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemHeight(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            if (!byte.TryParse(textBoxHeigth.Text, out byte byteRes))
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            item.Height = byteRes;
            TileData.ItemTable[index] = item;
            treeViewItem.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemMiscData(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            if (!short.TryParse(textBoxUnk1.Text, out short shortRes))
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            item.MiscData = shortRes;
            TileData.ItemTable[index] = item;
            treeViewItem.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemUnk2(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            if (!byte.TryParse(textBoxUnk2.Text, out byte byteRes))
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            item.Unk2 = byteRes;
            TileData.ItemTable[index] = item;
            treeViewItem.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemUnk3(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            if (!byte.TryParse(textBoxUnk3.Text, out byte byteRes))
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            item.Unk3 = byteRes;
            TileData.ItemTable[index] = item;
            treeViewItem.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedLandName(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewLand.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            LandData land = TileData.LandTable[index];
            string name = textBoxNameLand.Text;
            if (name.Length == 0)
            {
                return;
            }

            if (name.Length > 20)
            {
                name = name.Substring(0, 20);
            }

            land.Name = name;
            treeViewLand.SelectedNode.Text = string.Format("0x{0:X4} ({0}) {1}", index, name);
            TileData.LandTable[index] = land;
            treeViewLand.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index);
        }

        private void OnTextChangedLandTexID(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewLand.SelectedNode == null)
            {
                return;
            }

            if (!ushort.TryParse(textBoxTexID.Text, out ushort shortRes))
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            LandData land = TileData.LandTable[index];
            land.TextureId = shortRes;
            TileData.LandTable[index] = land;
            treeViewLand.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index);
        }

        private void OnFlagItemCheckItems(object sender, ItemCheckEventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (e.CurrentValue == e.NewValue)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            Array enumValues = Enum.GetValues(typeof(TileFlag));

            TileFlag changeFlag = (TileFlag)enumValues.GetValue(e.Index + 1);

            if ((item.Flags & changeFlag) != 0) // better double check
            {
                if (e.NewValue != CheckState.Unchecked)
                {
                    return;
                }

                item.Flags ^= changeFlag;
                TileData.ItemTable[index] = item;
                treeViewItem.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
            }
            else if ((item.Flags & changeFlag) == 0)
            {
                if (e.NewValue != CheckState.Checked)
                {
                    return;
                }

                item.Flags |= changeFlag;
                TileData.ItemTable[index] = item;
                treeViewItem.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
            }
        }

        private void OnFlagItemCheckLandTiles(object sender, ItemCheckEventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (e.CurrentValue == e.NewValue)
            {
                return;
            }

            if (treeViewLand.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            LandData land = TileData.LandTable[index];
            TileFlag changeFlag;
            switch (e.Index)
            {
                case 0:
                    changeFlag = TileFlag.Damaging;
                    break;

                case 1:
                    changeFlag = TileFlag.Wet;
                    break;

                case 2:
                    changeFlag = TileFlag.Impassable;
                    break;

                case 3:
                    changeFlag = TileFlag.Wall;
                    break;

                case 4:
                    changeFlag = TileFlag.NoDiagonal;
                    break;

                default:
                    changeFlag = TileFlag.None;
                    break;
            }

            if ((land.Flags & changeFlag) != 0)
            {
                if (e.NewValue != CheckState.Unchecked)
                {
                    return;
                }

                land.Flags ^= changeFlag;
                TileData.LandTable[index] = land;
                treeViewLand.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                ControlEvents.FireTileDataChangeEvent(this, index);
            }
            else if ((land.Flags & changeFlag) == 0)
            {
                if (e.NewValue != CheckState.Checked)
                {
                    return;
                }

                land.Flags |= changeFlag;
                TileData.LandTable[index] = land;
                treeViewLand.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                ControlEvents.FireTileDataChangeEvent(this, index);
            }
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            if (tabcontrol.SelectedIndex == 0) // items
            {
                string fileName = Path.Combine(path, "ItemData.csv");
                TileData.ExportItemDataToCsv(fileName);
                MessageBox.Show($"ItemData saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else
            {
                string fileName = Path.Combine(path, "LandData.csv");
                TileData.ExportLandDataToCsv(fileName);
                MessageBox.Show($"LandData saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private TileDataSearchForm _showForm1;
        private TileDataSearchForm _showForm2;

        private void OnClickSearch(object sender, EventArgs e)
        {
            if (tabcontrol.SelectedIndex == 0) // items
            {
                if (_showForm1?.IsDisposed == false)
                {
                    return;
                }

                _showForm1 = new TileDataSearchForm(false, SearchGraphic, SearchName)
                {
                    TopMost = true
                };
                _showForm1.Show();
            }
            else // land tiles
            {
                if (_showForm2?.IsDisposed == false)
                {
                    return;
                }

                _showForm2 = new TileDataSearchForm(true, SearchGraphic, SearchName)
                {
                    TopMost = true
                };
                _showForm2.Show();
            }
        }

        private void OnClickSelectItem(object sender, EventArgs e)
        {
            if (treeViewItem.SelectedNode?.Tag == null)
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

        private void OnClickSelectInLandTiles(object sender, EventArgs e)
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

        private void OnClickSelectRadarItem(object sender, EventArgs e)
        {
            if (treeViewItem.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            RadarColorControl.Select(index, false);
        }

        private void OnClickSelectRadarLand(object sender, EventArgs e)
        {
            if (treeViewLand.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            RadarColorControl.Select(index, true);
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
                Options.ChangedUltimaClass["TileData"] = true;
                if (tabcontrol.SelectedIndex == 0) // items
                {
                    TileData.ImportItemDataFromCsv(dialog.FileName);
                    AfterSelectTreeViewItem(this, new TreeViewEventArgs(treeViewItem.SelectedNode));
                }
                else
                {
                    TileData.ImportLandDataFromCsv(dialog.FileName);
                    AfterSelectTreeViewLand(this, new TreeViewEventArgs(treeViewLand.SelectedNode));
                }
            }
            dialog.Dispose();
        }

        private TileDataFilterForm _filterFormForm;

        private void OnClickSetFilter(object sender, EventArgs e)
        {
            if (_filterFormForm?.IsDisposed == false)
            {
                return;
            }

            _filterFormForm = new TileDataFilterForm(ApplyFilterItem, ApplyFilterLand)
            {
                TopMost = true
            };
            _filterFormForm.Show();
        }

        private void OnItemDataNodeExpanded(object sender, TreeViewCancelEventArgs e)
        {
            // workaround for 65536 items microsoft bug
            if (treeViewItem.Nodes.Count == 3)
            {
                treeViewItem.CollapseAll();
            }
        }

        private void TileData_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F || !e.Control)
            {
                return;
            }

            OnClickSearch(sender, e);
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        private const int _maleGumpOffset = 50_000;
        private const int _femaleGumpOffset = 60_000;

        private static void SelectInGumpsTab(int tiledataIndex, bool female = false)
        {
            int gumpOffset = female ? _femaleGumpOffset : _maleGumpOffset;
            var animation = TileData.ItemTable[tiledataIndex].Animation;

            GumpControl.Select(animation + gumpOffset);
        }

        private void SelectInGumpsTabMaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedItemTag = treeViewItem.SelectedNode?.Tag;
            if (selectedItemTag is null || (int)selectedItemTag <= 0)
            {
                return;
            }

            SelectInGumpsTab((int)selectedItemTag);
        }

        private void SelectInGumpsTabFemaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedItemTag = treeViewItem.SelectedNode?.Tag;
            if (selectedItemTag is null || (int)selectedItemTag <= 0)
            {
                return;
            }

            SelectInGumpsTab((int)selectedItemTag, true);
        }

        private void ItemsContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var selectedItemTag = treeViewItem.SelectedNode?.Tag;
            if (selectedItemTag is null || (int)selectedItemTag <= 0)
            {
                selectInGumpsTabMaleToolStripMenuItem.Enabled = false;
                selectInGumpsTabFemaleToolStripMenuItem.Enabled = false;
            }
            else
            {
                var itemData = TileData.ItemTable[(int)selectedItemTag];

                if (itemData.Animation > 0)
                {
                    selectInGumpsTabMaleToolStripMenuItem.Enabled =
                        GumpControl.HasGumpId(itemData.Animation + _maleGumpOffset);

                    selectInGumpsTabFemaleToolStripMenuItem.Enabled =
                        GumpControl.HasGumpId(itemData.Animation + _femaleGumpOffset);
                }
                else
                {
                    selectInGumpsTabMaleToolStripMenuItem.Enabled = false;
                    selectInGumpsTabFemaleToolStripMenuItem.Enabled = false;
                }
            }
        }

        /// <summary>
        /// DoubleClick event handler on the TextBoxTexID. Sets the TexID to the Tag value of the node
        /// i.e. 0x256 (598) lava -> 598.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxTexID_DoubleClick(object sender, EventArgs e)
        {
            if (!setTextureOnDoubleClickToolStripMenuItem.Checked)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            if (!int.TryParse(textBoxTexID.Text, out int texIdValue) || texIdValue == index)
            {
                return;
            }

            textBoxTexID.Text = $"{index}";
        }

        /// <summary>
        /// Click event handler on the "Set Textures" menu item. Sets all the land tiles TextureId to their index.
        /// This is written under the assumption that LandTileID == TextureId for every LandTile.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetTextureMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Do you want to set TexID for all land tiles?\n\n" +
                "This operation assumes that land tile index value is equal to texture index value.\n\n" +
                "It will only consider land tiles where TexID is 0.\n\nContinue?",
                "Set textures",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            var updated = 0;
            for (int i = 0; i < TileData.LandTable.Length; ++i)
            {
                if (!Textures.TestTexture(i) || TileData.LandTable[i].TextureId != 0)
                {
                    continue;
                }

                TileData.LandTable[i].TextureId = (ushort)i;

                var node = treeViewLand.Nodes.OfType<TreeNode>().FirstOrDefault(x => x.Tag.Equals(i));
                if (node != null)
                {
                    node.ForeColor = Color.Red;
                }

                updated++;

                Options.ChangedUltimaClass["TileData"] = true;
            }

            MessageBox.Show(updated > 0 ? $"Updated {updated} land tile(s)." : "Nothing was updated.", "Set textures");
        }
        #region ToolStripComboBox
        private void ToolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Set all elements to unchecked
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, false);
            }

            for (int i = 0; i < checkedListBox2.Items.Count; i++)
            {
                checkedListBox2.SetItemChecked(i, false);
            }
            // Update the Checked items and the Weight and Height values ​​based on the selected preset
            if (toolStripComboBox1.SelectedItem != null)
            {
                string selectedItem = toolStripComboBox1.SelectedItem.ToString();
                switch (selectedItem)
                {
                    case "Wall":  //1
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "20";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Wall"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Impassable"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoShoot"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleA"), true);
                        break;
                    case "Door-E-L": //2
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "20";
                        textBoxQuality.Text = "1";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Wall"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Impassable"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoShoot"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleA"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Door"), true);
                        break;
                    case "Door-E-R": //3
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "20";
                        textBoxQuality.Text = "2";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Wall"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Impassable"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoShoot"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleA"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Door"), true);
                        break;
                    case "Door-S-L": //4
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "20";
                        textBoxQuality.Text = "2";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Wall"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Impassable"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoShoot"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleA"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Door"), true);
                        break;
                    case "Door-S-R": //5
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "20";
                        textBoxQuality.Text = "3";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Wall"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Impassable"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoShoot"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleA"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Door"), true);
                        break;
                    case "Window": //6
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "20";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Wall"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Impassable"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Window"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoShoot"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleA"), true);
                        break;
                    case "Roof": //7
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "3";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoShoot"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleA"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Roof"), true);
                        break;
                    case "Floor": //8
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "0";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Background"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Bridge"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Surface"), true);
                        break;
                    case "Water": //9
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "0";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Background"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Translucent"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Surface"), true);
                        break;
                    case "Stairs-W": //10
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "5";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Surface"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Bridge"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoShoot"), true);
                        break;
                    case "Stairs-N": //11
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "5";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Surface"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Bridge"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoShoot"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("StairBack"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("StairRight"), true);
                        break;
                    case "Stairs-E": //12
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "5";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Surface"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Bridge"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoShoot"), true);
                        break;
                    case "Stairs-S": //13
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "5";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Surface"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Bridge"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoShoot"), true);
                        break;
                    case "Stairs-W-N": //14
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "5";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Surface"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Bridge"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoShoot"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("StairBack"), true);
                        break;
                    case "Stairs-N-E": //15
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "5";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Surface"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Bridge"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoShoot"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("StairRight"), true);
                        break;
                    case "Stairs-E-S": //16
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "5";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Surface"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Bridge"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoShoot"), true);
                        break;
                    case "Stairs-S-W": //17
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "5";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Surface"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Bridge"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoShoot"), true);
                        break;
                    case "Stairs-Block": //18
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "10";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Surface"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Bridge"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoShoot"), true);
                        break;
                    case "Container": //19
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "5";
                        textBoxStackOff.Text = "15";
                        textBoxUnk1.Text = "1";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Impassable"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleA"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Container"), true);
                        break;
                    case "Lamp Post": //20
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "15";
                        textBoxQuality.Text = "29";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Impassable"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleA"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("LightSource"), true);
                        break;
                    case "Fence": //21
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "10";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Wall"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Impassable"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleA"), true);
                        break;
                    case "Cave Wall": //22
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "24";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Wall"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Impassable"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoShoot"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleA"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleAn"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoHouse"), true);
                        break;
                    case "Clothing": //23
                        textBoxWeight.Text = "10";
                        textBoxHeigth.Text = "1";
                        textBoxQuality.Text = "10"; //Layer
                        textBoxStackOff.Text = "6"; //StackOff
                        textBoxUnk1.Text = "582"; //MiscData
                        textBoxAnim.Text = "599"; //Anim
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Weapon"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleA"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Wearable"), true);
                        break;
                    case "Plant": //24
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "1";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Background"), true);
                        break;
                    case "Chair-Small-5": //25
                        textBoxWeight.Text = "20";
                        textBoxHeigth.Text = "1";
                        textBoxStackOff.Text = "5"; //StackOff
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleA"), true);
                        break;
                    case "Chair-35": //26
                        textBoxWeight.Text = "20";
                        textBoxHeigth.Text = "1";
                        textBoxStackOff.Text = "35"; //StackOff
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleA"), true);
                        break;
                    case "Chair-Wood-8": //27
                        textBoxWeight.Text = "20";
                        textBoxHeigth.Text = "1";
                        textBoxStackOff.Text = "8"; //StackOff
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleA"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("PartialHue"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoDiagonal"), true);
                        break;
                    case "Chair-Throne-15": //27
                        textBoxWeight.Text = "20";
                        textBoxHeigth.Text = "1";
                        textBoxStackOff.Text = "8"; //StackOff
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleA"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("PartialHue"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoDiagonal"), true);
                        break;
                    case "LandTile Land": //29
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Background"), true);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Surface"), true);
                        break;
                    case "LandTile Water": //30
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Translucent"), true);
                        break;
                    case "LandTile Mountain": //31
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Impassable"), true);
                        break;
                    case "Tree": //32
                        textBoxName.Text = "Tree";
                        textBoxWeight.Text = "255";
                        textBoxHeigth.Text = "20";
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Impassable"), true);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleAn"), true);
                        break;
                    case "Clear": //33
                        textBoxName.Text = "";
                        textBoxWeight.Text = "";
                        textBoxHeigth.Text = "";
                        textBoxHue.Text = "";
                        textBoxUnk3.Text = "";
                        textBoxQuality.Text = "";
                        textBoxStackOff.Text = "";
                        textBoxUnk1.Text = "";
                        textBoxQuantity.Text = "";
                        textBoxValue.Text = "";
                        textBoxUnk2.Text = "";
                        textBoxAnim.Text = "";
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Background"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Weapon"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Transparent"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Translucent"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Wall"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Damaging"), false);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("Impassable"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Wet"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Unknown1"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Surface"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Bridge"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Generic"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Window"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("NoShoot"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("ArticleA"), false);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("ArticleAn"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("ArticleThe"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Foliage"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("PartialHue"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("NoHouse"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Map"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Container"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Wearable"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("LightSource"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Animation"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("HoverOver"), false);
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("NoDiagonal"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Armor"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Roof"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Door"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("StairBack"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("StairRight"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("AlphaBlend"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("UseNewArt"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("ArtUsed"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("Unused8"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("NoShadow"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("PixelBleed"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("PlayAnimOnce"), false);
                        checkedListBox2.SetItemChecked(checkedListBox2.Items.IndexOf("MultiMovable"), false);
                        break;
                        // Add more presets here
                }
            }
        }
        #endregion
        #region toolStripButton6 for Sound

        private bool playCustomSound = false;

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            toolStripButton6.Checked = !toolStripButton6.Checked;
            playCustomSound = !playCustomSound;
        }
        #endregion

        #region Save last Ideas

        bool toolStripButton7IsActive = false;
        List<int> savedCheckedIndices = new List<int>();
        Dictionary<TextBox, string> savedTextBoxTexts = new Dictionary<TextBox, string>();

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (!toolStripButton7IsActive)
            {
                // Save checked indices
                foreach (int index in checkedListBox1.CheckedIndices)
                {
                    savedCheckedIndices.Add(index);
                }

                // Save text box texts
                savedTextBoxTexts.Add(textBoxName, textBoxName.Text);
                savedTextBoxTexts.Add(textBoxWeight, textBoxWeight.Text);
                savedTextBoxTexts.Add(textBoxHue, textBoxHue.Text);
                savedTextBoxTexts.Add(textBoxHeigth, textBoxHeigth.Text);
                savedTextBoxTexts.Add(textBoxUnk3, textBoxUnk3.Text);
                savedTextBoxTexts.Add(textBoxQuality, textBoxQuality.Text);
                savedTextBoxTexts.Add(textBoxStackOff, textBoxStackOff.Text);
                savedTextBoxTexts.Add(textBoxUnk1, textBoxUnk1.Text);
                savedTextBoxTexts.Add(textBoxQuantity, textBoxQuantity.Text);
                savedTextBoxTexts.Add(textBoxValue, textBoxValue.Text);
                savedTextBoxTexts.Add(textBoxUnk2, textBoxUnk2.Text);
                savedTextBoxTexts.Add(textBoxAnim, textBoxAnim.Text);

                // Update toolStripButton7 appearance
                toolStripButton7.Checked = true;

                // Update toolStripButton7IsActive
                toolStripButton7IsActive = true;
            }
            else
            {
                // Restore checked indices
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    if (savedCheckedIndices.Contains(i))
                    {
                        checkedListBox1.SetItemChecked(i, true);
                    }
                    else
                    {
                        checkedListBox1.SetItemChecked(i, false);
                    }
                }

                // Clear savedCheckedIndices
                savedCheckedIndices.Clear();

                // Restore text box texts
                foreach (KeyValuePair<TextBox, string> pair in savedTextBoxTexts)
                {
                    pair.Key.Text = pair.Value;
                }

                // Clear savedTextBoxTexts
                savedTextBoxTexts.Clear();

                // Update toolStripButton7 appearance
                toolStripButton7.Checked = false;

                // Update toolStripButton7IsActive
                toolStripButton7IsActive = false;
            }
        }
        private void toolStripPushMarkedButton8_Click(object sender, EventArgs e)
        {
            if (toolStripButton7IsActive)
            {
                // Transferring the saved settings to the selected location.

                // Transferring the checked indices.
                foreach (int index in savedCheckedIndices)
                {
                    checkedListBox1.SetItemChecked(index, true);
                }

                // Transferring the textbox texts.
                foreach (KeyValuePair<TextBox, string> pair in savedTextBoxTexts)
                {
                    pair.Key.Text = pair.Value;
                }

                // Saving the changes (without MessageBox).
                OnClickSaveChanges(null, EventArgs.Empty);
            }
        }
        #endregion
        #region Search New
        private void SearchByIdToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Utils.ConvertStringToInt(searchByIdToolStripTextBox.Text, out int indexValue, 0, Art.GetMaxItemId()))
            {
                return;
            }

            var maximumIndex = Art.GetMaxItemId();

            if (indexValue < 0)
            {
                indexValue = 0;
            }

            if (indexValue > maximumIndex)
            {
                indexValue = maximumIndex;
            }

            var landTilesSelected = tabcontrol.SelectedIndex != 0;

            SearchGraphic(indexValue, landTilesSelected);
        }

        private void SearchByNameToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            var landTilesSelected = tabcontrol.SelectedIndex != 0;

            SearchName(searchByNameToolStripTextBox.Text, false, landTilesSelected);
        }

        private void SearchByNameToolStripButton_Click(object sender, EventArgs e)
        {
            var landTilesSelected = tabcontrol.SelectedIndex != 0;

            SearchName(searchByNameToolStripTextBox.Text, true, landTilesSelected);
        }
        #endregion

        #region middle mouse button copying the settings in the tiledata

        private void TreeViewItem_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                if (treeViewItem.SelectedNode != null && toolStripButton7.Checked)
                {
                    toolStripPushMarkedButton8_Click(null, null);
                }
            }
        }

        private void TreeViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Y && treeViewItem.SelectedNode != null)
            {
                toolStripPushMarkedButton8_Click(null, null);
            }
        }
        #endregion

        #region Label Tile Decimal
        private void LabelDecimalAdress_Click(object sender, EventArgs e)
        {
            if (treeViewLand.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            textBoxTexID.Text = index.ToString();
        }
        #endregion

        #region Delete Button
        private void toolStripButton8Clear_Click(object sender, EventArgs e)
        {
            // Set the text of all TextBoxes to an empty string
            textBoxName.Text = "";
            textBoxWeight.Text = "";
            textBoxHeigth.Text = "";
            textBoxHue.Text = "";
            textBoxUnk3.Text = "";
            textBoxQuality.Text = "";
            textBoxStackOff.Text = "";
            textBoxUnk1.Text = "";
            textBoxQuantity.Text = "";
            textBoxValue.Text = "";
            textBoxUnk2.Text = "";
            textBoxAnim.Text = "";

            // Make a list of the checkboxes you want to uncheck
            List<string> itemsToUncheck = new List<string>
            {
                "Background",
                "Weapon",
                "Transparent",
                "Translucent",
                "Wall",
                "Damaging",
                "Impassable",
                "Wet",
                "Unknown1",
                "Surface",
                "Bridge",
                "Generic",
                "Window",
                "NoShoot",
                "ArticleA",
                "ArticleAn",
                "ArticleThe",
                "Foliage",
                "PartialHue",
                "NoHouse",
                "Map",
                "Container",
                "Wearable",
                "LightSource",
                "Animation",
                "HoverOver",
                "NoDiagonal",
                "Armor",
                "Roof",
                "Door",
                "StairBack",
                "StairRight",
                "AlphaBlend",
                "UseNewArt",
                "ArtUsed",
                "Unused8",
                "NoShadow",
                "PixelBleed",
                "PlayAnimOnce",
                "MultiMovable"
            };

            // Uncheck all checkboxes in the list
            foreach (string item in itemsToUncheck)
            {
                int index = checkedListBox2.Items.IndexOf(item);
                if (index != -1)
                {
                    checkedListBox2.SetItemChecked(index, false);
                }
            }

            // Uncheck all checkboxes in checkedListBox1
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, false);
            }

            // Play a sound when playCustomSound is set to true
            if (playCustomSound)
            {
                SoundPlayer player = new SoundPlayer();
                player.SoundLocation = "sound.wav";
                player.Play();
            }
        }
        #endregion

        #region Zoom

        private bool isZoomed = false; // State of the zoom
        private Image originalImage; // Original image
        private Form zoomForm; // The zoom form
        private PictureBox zoomPictureBox; // The PictureBox in zoom form

        private void zoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if an image is selected
            if (pictureBoxItem.Image == null)
            {
                MessageBox.Show("Please select an image first.");
                return;
            }

            if (originalImage == null) // If the original image has not been saved yet
            {
                originalImage = pictureBoxItem.Image; // Save the original image
            }

            if (!isZoomed) // If not zoomed
            {
                // Enlarge the image to twice its original size
                Bitmap zoomedImage = new Bitmap(originalImage, originalImage.Width * 2, originalImage.Height * 2);

                // Draw the original image onto the new bitmap, resizing it
                using (Graphics g = Graphics.FromImage(zoomedImage))
                {
                    g.DrawImage(originalImage, 0, 0, zoomedImage.Width, zoomedImage.Height);
                }

                // Create the Zoom Shape and PictureBox if they are not already created
                if (zoomForm == null)
                {
                    zoomForm = new Form();
                    zoomPictureBox = new PictureBox
                    {
                        Dock = DockStyle.Fill,
                        SizeMode = PictureBoxSizeMode.Normal // Set the SizeMode property to Normal
                    };
                    zoomForm.Controls.Add(zoomPictureBox);
                    zoomForm.FormClosed += (s, ev) => { zoomForm = null; zoomPictureBox = null; }; // Reset zoomForm and zoomPictureBox to null when the form is closed

                    // Set the size of the shape to 690, 580
                    zoomForm.Size = new Size(690, 580);
                }

                // Set the PictureBox's image in the zoom form to the new bitmap
                zoomPictureBox.Image = zoomedImage;

                // Adjust the size of the shape to the size of the PictureBox
                zoomForm.ClientSize = zoomPictureBox.Size;

                // Display the zoom shape
                if (!zoomForm.Visible)
                {
                    zoomForm.Show(this); // Display the shape as a modal dialog
                }

                isZoomed = true; // Update the zoom state

                // Add a click event handler that resizes the shape
                zoomPictureBox.Click += (s, ev) =>
                {
                    if (zoomForm.Size.Width == 690 && zoomForm.Size.Height == 580)
                    {
                        zoomForm.Size = new Size(1432, 870);
                    }
                    else
                    {
                        zoomForm.Size = new Size(690, 580);
                    }
                };
            }
            else // When zoomed
            {
                // Reset the image to its original size
                pictureBoxItem.Image = originalImage;
                isZoomed = false; // Update the zoom state

                // Close the zoom shape
                if (zoomForm != null)
                {
                    zoomForm.Close();
                }
            }
        }

        // UpdateZoomFormImage
        private void UpdateZoomFormImage()
        {
            if (zoomForm != null && zoomPictureBox != null && originalImage != null && isZoomed)
            {
                // Enlarge the image to twice its original size
                Bitmap zoomedImage = new Bitmap(originalImage, originalImage.Width * 2, originalImage.Height * 2);

                // Draw the original image onto the new bitmap, resizing it
                using (Graphics g = Graphics.FromImage(zoomedImage))
                {
                    g.DrawImage(originalImage, 0, 0, zoomedImage.Width, zoomedImage.Height);
                }

                // Set the PictureBox's image in the zoom form to the new bitmap
                zoomPictureBox.Image = zoomedImage;
            }
        }

        // ZoomImage
        private void zoomImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if an image is selected
            if (pictureBoxItem.Image == null)
            {
                MessageBox.Show("Please select an image first.");
                return;
            }

            if (originalImage == null) // If the original image has not been saved yet
            {
                originalImage = pictureBoxItem.Image; // Save the original image
            }

            if (!isZoomed) // If not zoomed
            {
                // Enlarge the image to twice its original size
                Bitmap zoomedImage = new Bitmap(originalImage.Width * 2, originalImage.Height * 2);

                // Draw the original image onto the new bitmap, resizing it
                using (Graphics g = Graphics.FromImage(zoomedImage))
                {
                    g.DrawImage(originalImage, 0, 0, zoomedImage.Width, zoomedImage.Height);
                }

                // Set the PictureBox's image to the new bitmap
                pictureBoxItem.Image = zoomedImage;

                // Zentrieren Sie die PictureBox im Panel
                pictureBoxItem.Left = (splitContainer2.Panel2.Width - pictureBoxItem.Width) / 2;
                pictureBoxItem.Top = (splitContainer2.Panel2.Height - pictureBoxItem.Height) / 2;

                isZoomed = true; // Update the zoom state
            }
            else // When zoomed
            {
                // Reset the image to its original size
                pictureBoxItem.Image = originalImage;
                isZoomed = false; // Update the zoom state
            }
        }
        #endregion
    }
}
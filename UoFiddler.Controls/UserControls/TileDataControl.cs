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
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;
using System.Xml;

namespace UoFiddler.Controls.UserControls
{
    public partial class TileDataControl : UserControl
    {
        private Image _image;        

        #region [ TileDataControl ]
        public TileDataControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            AssignToolTipsToLabels();

            toolStripComboBox1.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            toolStripComboBox1.ComboBox.DrawItem += new DrawItemEventHandler(ToolStripComboBox1_DrawItem);
            LoadImages();  // Call the method to load the images

            toolStripComboBox1.SelectedIndexChanged += ToolStripComboBox1_SelectedIndexChanged;

            _refMarker = this;

            treeViewItem.BeforeSelect += TreeViewItemOnBeforeSelect;

            saveDirectlyOnChangesToolStripMenuItem.Checked = Options.TileDataDirectlySaveOnChange;
            saveDirectlyOnChangesToolStripMenuItem.CheckedChanged += SaveDirectlyOnChangesToolStripMenuItemOnCheckedChanged;

            ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            ControlEvents.TileDataChangeEvent += OnTileDataChangeEvent;

            LabelDecimalAdress.Text = "";

            tbClassicUOPfad.Text = Properties.Settings.Default.CUOTestPath; // Load Settings path

            InitEditing();
        }
        #endregion

        #region [ InitLandTilesFlagsCheckBoxes ]
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
        #endregion

        #region ItemControl Select PiictureBox refresh
        public void RefreshPictureBoxItem()
        {
            pictureBoxItem.Refresh();
        }
        #endregion

        #region [ InitItemsFlagsCheckBoxes ]
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
        #endregion

        private Settings _copiedSettings = null; // Global variable to store the copied settings
        private List<TreeNode> _selectedNodes = new List<TreeNode>(); // Veriable for multiple selection
        private LandSettings _copiedLandSettings = null; // Global variable for the copied country settings

        private static TileDataControl _refMarker;
        private bool _changingIndex;

        public bool IsLoaded { get; private set; }

        private int? _reselectGraphic;
        private bool? _reselectGraphicLand;

        #region [ Select ]
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
        #endregion

        #region [ SearchGraphic ]
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

                TreeNode extNode = GetOrCreateExtendedNode(graphic);
                if (extNode != null)
                {
                    _refMarker.tabcontrol.SelectTab(0);
                    _refMarker.treeViewItem.SelectedNode = extNode;
                    extNode.EnsureVisible();
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region [ Extended items ]
        private const string ExtendedGroupName = "Extended (0x10000+)";

        private static TreeNode CreateExtendedItemNode(int id)
        {
            return new TreeNode(string.Format("0x{0:X5} ({0}) {1}", id, TileData.ItemTable[id].Name)) { Tag = id };
        }

        /// <summary>Finds (or inserts, sorted) the node of an extended item id so it can be edited.</summary>
        private static TreeNode GetOrCreateExtendedNode(int graphic)
        {
            if (!TileData.HasExtension || graphic < Ultima.ExtendedFormat.BaseId || graphic >= TileData.ItemTable.Length)
            {
                return null;
            }

            TreeNode group = null;
            foreach (TreeNode n in _refMarker.treeViewItem.Nodes)
            {
                if (n.Text == ExtendedGroupName)
                {
                    group = n;
                    break;
                }
            }

            if (group == null)
            {
                group = new TreeNode(ExtendedGroupName);
                _refMarker.treeViewItem.Nodes.Add(group);
            }

            int insertAt = group.Nodes.Count;
            for (int i = 0; i < group.Nodes.Count; ++i)
            {
                int tag = (int)group.Nodes[i].Tag;
                if (tag == graphic)
                {
                    return group.Nodes[i];
                }

                if (tag > graphic)
                {
                    insertAt = i;
                    break;
                }
            }

            TreeNode node = CreateExtendedItemNode(graphic);
            group.Nodes.Insert(insertAt, node);
            return node;
        }
        #endregion

        #region [ SearchName ]
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
        #endregion

        #region [ ApplyFilterItem ]
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
        #endregion

        #region [ ApplyFilterLand ]
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
        #endregion

        #region [ Reload ]
        private void Reload()
        {
            if (IsLoaded)
            {
                OnLoad(this, new MyEventArgs(MyEventArgs.Types.ForceReload));
            }
        }
        #endregion

        #region [ OnLoad ]
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

                // Extended side-car items (ids >= 0x10000, tiledataext.mul): only used slots are listed,
                // free slots are added on demand by SearchGraphic (a tree with 1M nodes would be unusable)
                if (TileData.HasExtension)
                {
                    var extNodes = new List<TreeNode>();
                    for (int j = Ultima.ExtendedFormat.BaseId; j < TileData.ItemTable.Length; ++j)
                    {
                        if (!string.IsNullOrEmpty(TileData.ItemTable[j].Name) || Art.IsValidStatic(j))
                        {
                            extNodes.Add(CreateExtendedItemNode(j));
                        }
                    }

                    treeViewItem.Nodes.Add(new TreeNode(ExtendedGroupName, extNodes.ToArray()));
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

            // the lists were built again: old selections and undo steps do not fit any more
            _selectedNodes.Clear();
            _selectedLandNodes.Clear();
            _itemAnchor = null;
            _landAnchor = null;
            TileDataHistory.Clear();
            TileDataHistory.SetBaseline();

            IsLoaded = true;
            Cursor.Current = Cursors.Default;
        }
        #endregion

        #region [ OnFilePathChangeEvent ]
        private void OnFilePathChangeEvent()
        {
            Reload();
        }
        #endregion

        #region [ OnTileDataChangeEvent ]
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
        #endregion        

        #region [ AfterSelectTreeViewItem ]
        private void AfterSelectTreeViewItem(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag == null)
            {
                return;
            }

            // Ctrl = add/remove, Shift = range, otherwise single selection (TileDataControl.Editing.cs);
            // a refresh from code (e.g. a change in another tab) keeps the selection
            if (sender == treeViewItem)
            {
                HandleItemSelection(e.Node);
            }

            // show the values of the clicked item; changed fields are remembered from here on
            ApplySettingsToNode(e.Node);
            ResetItemEdits();
        }
        #endregion

        #region ClearPreviousSelection
        private void ClearPreviousSelection()
        {
            foreach (TreeNode node in _selectedNodes)
            {
                node.BackColor = treeViewItem.BackColor; // Reset the background color to default
            }
            _selectedNodes.Clear();
        }
        #endregion

        #region [ ApplySettingsToNode ]
        private void ApplySettingsToNode(TreeNode node)
        {
            // Old logic
            int index = (int)node.Tag;

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

                _originalImage = pictureBoxItem.Image; // Update the original image -> Zoom

                // Update the image in the Zoom form
                if (_zoomForm != null && _zoomForm.Visible)
                {
                    UpdateZoomFormImage();
                }

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
        #endregion

        #region [ AfterSelectTreeViewLand ]
        private void AfterSelectTreeViewLand(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
            {
                return;
            }

            if (!_suppressLandSelection && sender == treeViewLand)
            {
                // Ctrl = add/remove, Shift = range, otherwise single selection (TileDataControl.Editing.cs)
                HandleLandSelection(e.Node);
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
            ResetLandEdits();
        }
        #endregion

        #region [ OnClickSaveTiledata ]
        private void OnClickSaveTiledata(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "tiledata.mul");
            TileData.SaveTileData(fileName);
            MessageBox.Show($"TileData saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["TileData"] = false;

            // the saved state is the new "state of the file" (changes list, red entries)
            TileDataHistory.SetBaseline();
            foreach (TreeNode node in ItemNodes())
            {
                node.ForeColor = treeViewItem.ForeColor;
            }

            foreach (TreeNode node in LandNodes())
            {
                node.ForeColor = treeViewLand.ForeColor;
            }
        }
        #endregion

        #region [ OnClickSaveChanges ] 
        private void OnClickSaveChanges(object sender, EventArgs e)
        {
            // one tile: all values of the fields; several tiles: only the changed values/flags (TileDataControl.Editing.cs)
            SaveChanges(false);
        }
        #endregion

        #region [ SaveDirectlyOnChangesToolStripMenuItemOnCheckedChanged ]
        private void SaveDirectlyOnChangesToolStripMenuItemOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            Options.TileDataDirectlySaveOnChange = saveDirectlyOnChangesToolStripMenuItem.Checked;
        }
        #endregion

        #region [ OnTextChangedItemAnim ]
        private void OnTextChangedItemAnim(object sender, EventArgs e)
        {
            // remembers the change; with "save directly" it is applied to all selected items at once
            OnItemFieldEdited(TileField.Animation);
        }
        #endregion

        #region [ OnTextChangedItemName ]
        private void OnTextChangedItemName(object sender, EventArgs e)
        {
            // remembers the change; with "save directly" it is applied to all selected items at once
            OnItemFieldEdited(TileField.Name);
        }
        #endregion

        #region [ TreeViewItemOnBeforeSelect ]
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
        #endregion

        #region [ OnTextChangedItemWeight ]
        private void OnTextChangedItemWeight(object sender, EventArgs e)
        {
            // remembers the change; with "save directly" it is applied to all selected items at once
            OnItemFieldEdited(TileField.Weight);
        }
        #endregion

        #region [ OnTextChangedItemQuality ]
        private void OnTextChangedItemQuality(object sender, EventArgs e)
        {
            // remembers the change; with "save directly" it is applied to all selected items at once
            OnItemFieldEdited(TileField.Quality);
        }
        #endregion

        #region [ OnTextChangedItemQuantity ]
        private void OnTextChangedItemQuantity(object sender, EventArgs e)
        {
            // remembers the change; with "save directly" it is applied to all selected items at once
            OnItemFieldEdited(TileField.Quantity);
        }
        #endregion

        #region [ OnTextChangedItemHue ]
        private void OnTextChangedItemHue(object sender, EventArgs e)
        {
            // remembers the change; with "save directly" it is applied to all selected items at once
            OnItemFieldEdited(TileField.Hue);
        }
        #endregion

        #region [ OnTextChangedItemStackOff ]
        private void OnTextChangedItemStackOff(object sender, EventArgs e)
        {
            // remembers the change; with "save directly" it is applied to all selected items at once
            OnItemFieldEdited(TileField.StackingOffset);
        }
        #endregion

        #region [ OnTextChangedItemValue ]
        private void OnTextChangedItemValue(object sender, EventArgs e)
        {
            // remembers the change; with "save directly" it is applied to all selected items at once
            OnItemFieldEdited(TileField.Value);
        }
        #endregion

        #region [ OnTextChangedItemHeight ]
        private void OnTextChangedItemHeight(object sender, EventArgs e)
        {
            // remembers the change; with "save directly" it is applied to all selected items at once
            OnItemFieldEdited(TileField.Height);
        }
        #endregion

        #region [ OnTextChangedItemMiscData ]
        private void OnTextChangedItemMiscData(object sender, EventArgs e)
        {
            // remembers the change; with "save directly" it is applied to all selected items at once
            OnItemFieldEdited(TileField.MiscData);
        }
        #endregion

        #region [ OnTextChangedItemUnk2 ]
        private void OnTextChangedItemUnk2(object sender, EventArgs e)
        {
            // remembers the change; with "save directly" it is applied to all selected items at once
            OnItemFieldEdited(TileField.Unk2);
        }
        #endregion

        #region [ OnTextChangedItemUnk3 ]
        private void OnTextChangedItemUnk3(object sender, EventArgs e)
        {
            // remembers the change; with "save directly" it is applied to all selected items at once
            OnItemFieldEdited(TileField.Unk3);
        }
        #endregion

        #region [ OnTextChangedLandName ]
        private void OnTextChangedLandName(object sender, EventArgs e)
        {
            OnLandFieldEdited(TileField.Name);
        }
        #endregion

        #region OnTextChangedLandTexID
        private void OnTextChangedLandTexID(object sender, EventArgs e)
        {
            OnLandFieldEdited(TileField.TextureId);
        }
        #endregion

        #region [ FlagOfListIndex / FlagsFromList ]
        /// <summary>flag of an entry of checkedListBox1 / checkedListBox2 (both list the flags in enum order without None)</summary>
        private static TileFlag FlagOfListIndex(int listIndex)
        {
            Array enumValues = Enum.GetValues(typeof(TileFlag));
            return listIndex >= 0 && listIndex + 1 < enumValues.Length ? (TileFlag)enumValues.GetValue(listIndex + 1) : TileFlag.None;
        }

        /// <summary>
        /// Flags of a checked list box. Flags that are not shown in the list (upper 32 flags of
        /// old clients) are taken from <paramref name="keep"/> so they are not lost.
        /// </summary>
        private static TileFlag FlagsFromList(CheckedListBox list, TileFlag keep)
        {
            TileFlag shown = TileFlag.None;
            TileFlag result = TileFlag.None;
            for (int i = 0; i < list.Items.Count; ++i)
            {
                TileFlag flag = FlagOfListIndex(i);
                shown |= flag;
                if (list.GetItemChecked(i))
                {
                    result |= flag;
                }
            }

            return result | (keep & ~shown);
        }
        #endregion

        #region [ OnFlagItemCheckItems ]
        private void OnFlagItemCheckItems(object sender, ItemCheckEventArgs e)
        {
            // remembers the flag; with "save directly" it is set/removed on all selected items at once
            OnItemFlagEdited(e);
        }
        #endregion

        #region [ OnFlagItemCheckLandTiles ]
        private void OnFlagItemCheckLandTiles(object sender, ItemCheckEventArgs e)
        {
            OnLandFlagEdited(e);
        }
        #endregion

        #region [ OnClickExport ]
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
        #endregion

        #region [ OnClickSearch ]
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
        #endregion

        #region [ OnClickSelectItem ]
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
        #endregion

        #region [ OnClickSelectInLandTiles ]
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
        #endregion

        #region [ OnClickSelectRadarItem ]
        private void OnClickSelectRadarItem(object sender, EventArgs e)
        {
            if (treeViewItem.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            RadarColorControl.Select(index, false);
        }
        #endregion

        #region [ OnClickSelectRadarLand ]
        private void OnClickSelectRadarLand(object sender, EventArgs e)
        {
            if (treeViewLand.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            RadarColorControl.Select(index, true);
        }
        #endregion

        #region [ OnClickImport ]
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
                    var before = (ItemData[])TileData.ItemTable.Clone();
                    TileDataHistory.Record($"CSV import {Path.GetFileName(dialog.FileName)}", Enumerable.Range(0, before.Length), null);
                    TileData.ImportItemDataFromCsv(dialog.FileName);
                    MarkItemsChanged(Enumerable.Range(0, before.Length).Where(i => !TileDataHistory.SameItem(before[i], TileData.ItemTable[i])).ToList());
                }
                else
                {
                    var before = (LandData[])TileData.LandTable.Clone();
                    TileDataHistory.Record($"CSV import {Path.GetFileName(dialog.FileName)}", null, Enumerable.Range(0, before.Length));
                    TileData.ImportLandDataFromCsv(dialog.FileName);
                    MarkLandsChanged(Enumerable.Range(0, before.Length).Where(i => !TileDataHistory.SameLand(before[i], TileData.LandTable[i])).ToList());
                }

                RefreshShownTiles();
            }
            dialog.Dispose();
        }
        #endregion

        #region [ OnClickSetFilter ]
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
        #endregion

        #region [ OnItemDataNodeExpanded ]
        private void OnItemDataNodeExpanded(object sender, TreeViewCancelEventArgs e)
        {
            // workaround for 65536 items microsoft bug
            if (treeViewItem.Nodes.Count == 3)
            {
                treeViewItem.CollapseAll();
            }
        }
        #endregion

        #region [ TileData_KeyUp ]
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
        #endregion

        #region [ SelectInGumpsTab ]
        private const int _maleGumpOffset = 50_000;
        private const int _femaleGumpOffset = 60_000;

        private static void SelectInGumpsTab(int tiledataIndex, bool female = false)
        {
            int gumpOffset = female ? _femaleGumpOffset : _maleGumpOffset;
            var animation = TileData.ItemTable[tiledataIndex].Animation;

            GumpControl.Select(animation + gumpOffset);
        }
        #endregion

        #region [ SelectInGumpsTabMaleToolStripMenuItem_Click ]
        private void SelectInGumpsTabMaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedItemTag = treeViewItem.SelectedNode?.Tag;
            if (selectedItemTag is null || (int)selectedItemTag <= 0)
            {
                return;
            }

            SelectInGumpsTab((int)selectedItemTag);
        }
        #endregion

        #region [ SelectInGumpsTabFemaleToolStripMenuItem_Click ]
        private void SelectInGumpsTabFemaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedItemTag = treeViewItem.SelectedNode?.Tag;
            if (selectedItemTag is null || (int)selectedItemTag <= 0)
            {
                return;
            }

            SelectInGumpsTab((int)selectedItemTag, true);
        }
        #endregion

        #region [ ItemsContextMenuStrip_Opening ]
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
        #endregion

        #region [ TextBoxTexID_DoubleClick ]
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
        #endregion

        #region [ SetTextureMenuItem ]
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

            TileDataHistory.Record("Set texture ids",
                null, Enumerable.Range(0, TileData.LandTable.Length).Where(i => Textures.TestTexture(i) && TileData.LandTable[i].TextureId == 0));

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
        #endregion

        #region ToolStripComboBox and More

        #region LoadImages
        void LoadImages()
        {
            if (!_icons.ContainsKey("Stairs"))
            {
                _icons.Add("Stairs-S", Properties.Resources.Stairs01);
                _icons.Add("Stairs-E", Properties.Resources.Stairs02);
                _icons.Add("Stairs-N", Properties.Resources.Stairs03);
                _icons.Add("Stairs-W", Properties.Resources.Stairs04);
                _icons.Add("Stairs-E-S", Properties.Resources.Stairs05);
                _icons.Add("Stairs-N-E", Properties.Resources.Stairs06);
                _icons.Add("Stairs-S-W", Properties.Resources.Stairs07);
                _icons.Add("Stairs-W-N", Properties.Resources.Stairs08);
            }
        }
        #endregion

        #region [ Define Dictionary with Bitmap instead of Icon ]
        // Define Dictionary with Bitmap instead of Icon
        Dictionary<string, Bitmap> _icons = new Dictionary<string, Bitmap>();
        #endregion

        #region [ toolStripComboBox1_DrawItem ]
        void ToolStripComboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            string name = toolStripComboBox1.Items[e.Index].ToString();
            // First draw the text
            e.Graphics.DrawString(name, e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left, e.Bounds.Top);
            if (_icons.TryGetValue(name, out Bitmap bitmap))  // Use TryGetValue to check and get the image
            {
                // Scale the image to the height of the line
                int width = Convert.ToInt32(e.Graphics.MeasureString(name, e.Font).Width);
                Rectangle destRect = new Rectangle(e.Bounds.Left + width, e.Bounds.Top, e.Bounds.Height, e.Bounds.Height);
                e.Graphics.DrawImage(bitmap, destRect);
            }
        }
        #endregion

        #region [ ToolStripComboBox1_SelectedIndexChanged ]
        private void ToolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Only the list the preset is for is reset: "LandTile …" presets change the land flags,
            // all others the item flags. Before both lists were cleared, so choosing an item preset
            // also removed the flags of the selected land tile (with "save directly on changes").
            bool landPreset = toolStripComboBox1.SelectedItem?.ToString().StartsWith("LandTile", StringComparison.Ordinal) == true;
            CheckedListBox target = landPreset ? checkedListBox2 : checkedListBox1;
            for (int i = 0; i < target.Items.Count; i++)
            {
                target.SetItemChecked(i, false);
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
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("StairBack"), true);
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
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf("StairRight"), true);
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
                        break;
                        // Add more presets here
                }
            }
        }
        #endregion
        #endregion

        #region [ Search and Sound ]
        private bool _playCustomSound = false;

        #region [ toolStripButton6 ]
        private void ToolStripButton6_Click(object sender, EventArgs e)
        {
            toolStripButton6.Checked = !toolStripButton6.Checked;
            _playCustomSound = !_playCustomSound;
        }
        #endregion        

        bool _toolStripButton7IsActive = false;
        List<int> _savedCheckedIndices = new List<int>();
        Dictionary<TextBox, string> _savedTextBoxTexts = new Dictionary<TextBox, string>();

        #region [ toolStripButton7 ]
        private void ToolStripButton7_Click(object sender, EventArgs e)
        {
            if (!_toolStripButton7IsActive)
            {
                // Save checked indices
                foreach (int index in checkedListBox1.CheckedIndices)
                {
                    _savedCheckedIndices.Add(index);
                }

                // Save text box texts
                _savedTextBoxTexts.Add(textBoxName, textBoxName.Text);
                _savedTextBoxTexts.Add(textBoxWeight, textBoxWeight.Text);
                _savedTextBoxTexts.Add(textBoxHue, textBoxHue.Text);
                _savedTextBoxTexts.Add(textBoxHeigth, textBoxHeigth.Text);
                _savedTextBoxTexts.Add(textBoxUnk3, textBoxUnk3.Text);
                _savedTextBoxTexts.Add(textBoxQuality, textBoxQuality.Text);
                _savedTextBoxTexts.Add(textBoxStackOff, textBoxStackOff.Text);
                _savedTextBoxTexts.Add(textBoxUnk1, textBoxUnk1.Text);
                _savedTextBoxTexts.Add(textBoxQuantity, textBoxQuantity.Text);
                _savedTextBoxTexts.Add(textBoxValue, textBoxValue.Text);
                _savedTextBoxTexts.Add(textBoxUnk2, textBoxUnk2.Text);
                _savedTextBoxTexts.Add(textBoxAnim, textBoxAnim.Text);

                // Update toolStripButton7 appearance
                toolStripButton7.Checked = true;

                // Update toolStripButton7IsActive
                _toolStripButton7IsActive = true;
            }
            else
            {
                // Restore checked indices
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    if (_savedCheckedIndices.Contains(i))
                    {
                        checkedListBox1.SetItemChecked(i, true);
                    }
                    else
                    {
                        checkedListBox1.SetItemChecked(i, false);
                    }
                }

                // Clear savedCheckedIndices
                _savedCheckedIndices.Clear();

                // Restore text box texts
                foreach (KeyValuePair<TextBox, string> pair in _savedTextBoxTexts)
                {
                    pair.Key.Text = pair.Value;
                }

                // Clear savedTextBoxTexts
                _savedTextBoxTexts.Clear();

                // Update toolStripButton7 appearance
                toolStripButton7.Checked = false;

                // Update toolStripButton7IsActive
                _toolStripButton7IsActive = false;
            }
        }
        #endregion

        #region [ toolStripPushMarkedButton8 ]
        private void ToolStripPushMarkedButton8_Click(object sender, EventArgs e)
        {
            if (_toolStripButton7IsActive)
            {
                // Transferring the saved settings to the selected location.

                // Transferring the checked indices.
                foreach (int index in _savedCheckedIndices)
                {
                    checkedListBox1.SetItemChecked(index, true);
                }

                // Transferring the textbox texts.
                foreach (KeyValuePair<TextBox, string> pair in _savedTextBoxTexts)
                {
                    pair.Key.Text = pair.Value;
                }

                // Saving the changes: all transferred values, also on several selected items
                SaveChanges(true);
            }
        }
        #endregion

        #region [ SearchByIdToolStripTextBox_KeyUp ]
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
        #endregion

        #region [ SearchByNameToolStripTextBox_KeyUp ]
        private void SearchByNameToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            var landTilesSelected = tabcontrol.SelectedIndex != 0;

            SearchName(searchByNameToolStripTextBox.Text, false, landTilesSelected);
        }
        #endregion

        #region [ SearchByNameToolStripButton_Click ]
        private void SearchByNameToolStripButton_Click(object sender, EventArgs e)
        {
            var landTilesSelected = tabcontrol.SelectedIndex != 0;

            SearchName(searchByNameToolStripTextBox.Text, true, landTilesSelected);
        }
        #endregion
        #endregion        

        #region middle mouse button copying the settings in the tiledata

        private void TreeViewItem_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                if (treeViewItem.SelectedNode != null && toolStripButton7.Checked)
                {
                    ToolStripPushMarkedButton8_Click(null, null);
                }
            }
        }

        private void TreeViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Y && treeViewItem.SelectedNode != null)
            {
                ToolStripPushMarkedButton8_Click(null, null);
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
        private void ToolStripButton8Clear_Click(object sender, EventArgs e)
        {
            if (tabcontrol.SelectedIndex != 0)
            {
                // land tab: only the land flags (the text boxes belong to the items)
                for (int i = 0; i < checkedListBox2.Items.Count; i++)
                {
                    checkedListBox2.SetItemChecked(i, false);
                }
            }
            else
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

                // Uncheck all item flags (before the land flags were cleared as well)
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    checkedListBox1.SetItemChecked(i, false);
                }
            }

            // Play a sound when playCustomSound is set to true
            if (_playCustomSound)
            {
                SoundPlayer player = new SoundPlayer();
                player.SoundLocation = "sound.wav";
                player.Play();
            }
        }
        #endregion

        #region Zoom
        private bool _isZoomed = false; // State of the zoom
        private Image _originalImage; // Original image
        private Form _zoomForm; // The zoom form
        private PictureBox _zoomPictureBox; // The PictureBox in zoom form

        private void ZoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if an image is selected
            if (pictureBoxItem.Image == null)
            {
                MessageBox.Show("Please select an image first.");
                return;
            }

            if (_originalImage == null) // If the original image has not been saved yet
            {
                _originalImage = pictureBoxItem.Image; // Save the original image
            }

            if (!_isZoomed) // If not zoomed
            {
                // Enlarge the image to twice its original size
                Bitmap zoomedImage = new Bitmap(_originalImage, _originalImage.Width * 2, _originalImage.Height * 2);

                // Draw the original image onto the new bitmap, resizing it
                using (Graphics g = Graphics.FromImage(zoomedImage))
                {
                    g.DrawImage(_originalImage, 0, 0, zoomedImage.Width, zoomedImage.Height);
                }

                // Create the Zoom Shape and PictureBox if they are not already created
                if (_zoomForm == null)
                {
                    _zoomForm = new Form();
                    _zoomPictureBox = new PictureBox
                    {
                        Dock = DockStyle.Fill,
                        SizeMode = PictureBoxSizeMode.Normal // Set the SizeMode property to Normal
                    };
                    _zoomForm.Controls.Add(_zoomPictureBox);
                    _zoomForm.FormClosed += (s, ev) => { _zoomForm = null; _zoomPictureBox = null; }; // Reset zoomForm and zoomPictureBox to null when the form is closed

                    // Set the size of the shape to 690, 580
                    _zoomForm.Size = new Size(690, 580);
                }

                // Set the PictureBox's image in the zoom form to the new bitmap
                _zoomPictureBox.Image = zoomedImage;

                // Adjust the size of the shape to the size of the PictureBox
                _zoomForm.ClientSize = _zoomPictureBox.Size;

                // Display the zoom shape
                if (!_zoomForm.Visible)
                {
                    _zoomForm.Show(this); // Display the shape as a modal dialog
                }

                _isZoomed = true; // Update the zoom state

                // Add a click event handler that resizes the shape
                _zoomPictureBox.Click += (s, ev) =>
                {
                    if (_zoomForm.Size.Width == 690 && _zoomForm.Size.Height == 580)
                    {
                        _zoomForm.Size = new Size(1432, 870);
                    }
                    else
                    {
                        _zoomForm.Size = new Size(690, 580);
                    }
                };
            }
            else // When zoomed
            {
                // Reset the image to its original size
                pictureBoxItem.Image = _originalImage;
                _isZoomed = false; // Update the zoom state

                // Close the zoom shape
                if (_zoomForm != null)
                {
                    _zoomForm.Close();
                }
            }
        }

        // UpdateZoomFormImage
        private void UpdateZoomFormImage()
        {
            if (_zoomForm != null && _zoomPictureBox != null && _originalImage != null && _isZoomed)
            {
                // Enlarge the image to twice its original size
                Bitmap zoomedImage = new Bitmap(_originalImage, _originalImage.Width * 2, _originalImage.Height * 2);

                // Draw the original image onto the new bitmap, resizing it
                using (Graphics g = Graphics.FromImage(zoomedImage))
                {
                    g.DrawImage(_originalImage, 0, 0, zoomedImage.Width, zoomedImage.Height);
                }

                // Set the PictureBox's image in the zoom form to the new bitmap
                _zoomPictureBox.Image = zoomedImage;
            }
        }

        // ZoomImage
        private void ZoomImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if an image is selected
            if (pictureBoxItem.Image == null)
            {
                MessageBox.Show("Please select an image first.");
                return;
            }

            if (_originalImage == null) // If the original image has not been saved yet
            {
                _originalImage = pictureBoxItem.Image; // Save the original image
            }

            if (!_isZoomed) // If not zoomed
            {
                // Enlarge the image to twice its original size
                Bitmap zoomedImage = new Bitmap(_originalImage.Width * 2, _originalImage.Height * 2);

                // Draw the original image onto the new bitmap, resizing it
                using (Graphics g = Graphics.FromImage(zoomedImage))
                {
                    g.DrawImage(_originalImage, 0, 0, zoomedImage.Width, zoomedImage.Height);
                }

                // Set the PictureBox's image to the new bitmap
                pictureBoxItem.Image = zoomedImage;

                // Center the PictureBox in the panel
                pictureBoxItem.Left = (splitContainer2.Panel2.Width - pictureBoxItem.Width) / 2;
                pictureBoxItem.Top = (splitContainer2.Panel2.Height - pictureBoxItem.Height) / 2;

                _isZoomed = true; // Update the zoom state
            }
            else // When zoomed
            {
                // Reset the image to its original size
                pictureBoxItem.Image = _originalImage;
                _isZoomed = false; // Update the zoom state
            }
        }
        #endregion

        #region [ buttonLoadTxt ]
        private void ButtonLoadTxt_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog { Description = "ClassicUO folder (the folder of ClassicUO.exe) or its Data\\Client folder" })
            {
                if (Directory.Exists(tbClassicUOPfad.Text))
                {
                    folderBrowserDialog.SelectedPath = tbClassicUOPfad.Text;
                }

                if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // Set the text of tbClassicUOPfad to the selected path and save it in the settings
                tbClassicUOPfad.Text = folderBrowserDialog.SelectedPath;
                Properties.Settings.Default.CUOTestPath = folderBrowserDialog.SelectedPath;
                Properties.Settings.Default.Save();
            }

            UpdateCuoStatus();
            if (comboBoxLoadText.SelectedItem != null)
            {
                LoadCuoFile();
            }
        }
        #endregion


        #region [ comboBoxLoadText_SelectedIndexChanged ]
        private void ComboBoxLoadText_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TileDataControl.Cuo.cs: folder detection, description, check, backup
            LoadCuoFile();
        }

        #endregion

        #region [ btSaveTxtCuo ]
        private void BtSaveTxtCuo_Click(object sender, EventArgs e)
        {
            SaveCuoFile();
        }
        #endregion

        #region [ lbChairInfo_Click ]
        private void LbChairInfo_Click(object sender, EventArgs e)
        {
            Image image = Properties.Resources.MakeChairsUseable;

            Form form = new Form
            {
                ClientSize = image.Size,
                ShowIcon = false
            };

            PictureBox pictureBox = new PictureBox
            {
                Image = image,
                SizeMode = PictureBoxSizeMode.AutoSize
            };

            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            panel.Controls.Add(pictureBox);

            form.Controls.Add(panel);

            form.Show();
        }
        #endregion

        #region [ findToolStripMenuItem ]
        private void FindToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string searchText = toolStripTextBoxFindText.Text;

            int index = richTextBoxEdit.Find(searchText);

            if (index != -1)
            {
                richTextBoxEdit.Select(index, searchText.Length);
                richTextBoxEdit.Focus();
            }
        }
        #endregion

        #region [ copySettingsToolStripMenuItem ]
        private void CopySettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Create a new instance of Settings and copy the values from text boxes and checked list
            _copiedSettings = new Settings
            {
                Name = textBoxName.Text,
                Weight = textBoxWeight.Text,
                Anim = textBoxAnim.Text,
                Quality = textBoxQuality.Text,
                Quantity = textBoxQuantity.Text,
                Hue = textBoxHue.Text,
                StackOff = textBoxStackOff.Text,
                Value = textBoxValue.Text,
                Height = textBoxHeigth.Text,
                Unk1 = textBoxUnk1.Text,
                Unk2 = textBoxUnk2.Text,
                Unk3 = textBoxUnk3.Text,
                CheckedList = new List<bool>()
            };

            // Save the state of the CheckedListBox1 by iterating over each item
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                // Add true if the item is checked, false otherwise
                _copiedSettings.CheckedList.Add(checkedListBox1.GetItemChecked(i));
            }

            // Optionally, display a message to indicate successful copying
            // MessageBox.Show("Settings copied successfully!");

            // Play a sound effect after settings have been copied
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = "sound.wav";
            player.Play();
        }
        #endregion

        #region [ insertSettingsToolStripMenuItem ]
        private void InsertSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if any settings were copied before attempting to insert them
            if (_copiedSettings == null)
            {
                MessageBox.Show("No settings copied yet.");
                return;
            }

            // Restore the settings by assigning values back to the text boxes
            textBoxName.Text = _copiedSettings.Name;
            textBoxWeight.Text = _copiedSettings.Weight;
            textBoxAnim.Text = _copiedSettings.Anim;
            textBoxQuality.Text = _copiedSettings.Quality;
            textBoxQuantity.Text = _copiedSettings.Quantity;
            textBoxHue.Text = _copiedSettings.Hue;
            textBoxStackOff.Text = _copiedSettings.StackOff;
            textBoxValue.Text = _copiedSettings.Value;
            textBoxHeigth.Text = _copiedSettings.Height;
            textBoxUnk1.Text = _copiedSettings.Unk1;
            textBoxUnk2.Text = _copiedSettings.Unk2;
            textBoxUnk3.Text = _copiedSettings.Unk3;

            // Restore the state of the CheckedListBox1 from the copied settings
            for (int i = 0; i < checkedListBox1.Items.Count && i < _copiedSettings.CheckedList.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, _copiedSettings.CheckedList[i]);
            }

            // Optionally, display a message to indicate successful insertion
            // MessageBox.Show("Settings inserted successfully!");

            // Play a sound effect after settings have been inserted
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = "sound.wav";
            player.Play();
        }
        #endregion

        #region [ class Settings ] 
        public class Settings // Copy for settings
        {
            public string Name { get; set; }
            public string Weight { get; set; }
            public string Anim { get; set; }
            public string Quality { get; set; }
            public string Quantity { get; set; }
            public string Hue { get; set; }
            public string StackOff { get; set; }
            public string Value { get; set; }
            public string Height { get; set; }
            public string Unk1 { get; set; }
            public string Unk2 { get; set; }
            public string Unk3 { get; set; }
            public List<bool> CheckedList { get; set; } // Speichert die CheckedListBox Werte
        }
        #endregion

        #region [ copySettingsLandToolStripMenuItem ]
        private void CopySettingsLandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _copiedLandSettings = new LandSettings
            {
                Name = textBoxNameLand.Text,
                CheckedList = new List<bool>(),
                TextureId = ushort.TryParse(textBoxTexID.Text, out ushort texId) ? texId : (ushort)0
            };

            // Save the state of CheckedListBox2 (one entry per flag, in list order)
            for (int i = 0; i < checkedListBox2.Items.Count; i++)
            {
                _copiedLandSettings.CheckedList.Add(checkedListBox2.GetItemChecked(i));
            }
            
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = "sound.wav";
            player.Play();
        }
        #endregion

        #region [ insertSettingsLandToolStripMenuItem ]
        private void InsertSettingsLandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_copiedLandSettings == null)
            {
                MessageBox.Show("No settings copied yet.");
                return;
            }

            if (treeViewLand.SelectedNode == null)
            {
                MessageBox.Show("No node selected.");
                return;
            }

            // Get the currently selected node
            TreeNode selectedNode = treeViewLand.SelectedNode;
            int index = (int)selectedNode.Tag;

            // Transfer the copied settings to the current country item
            LandData land = TileData.LandTable[index];
            land.Name = _copiedLandSettings.Name;
            land.TextureId = _copiedLandSettings.TextureId;

            // Show the copied flags (without the direct-save handler) and write them into the land tile
            _changingIndex = true;
            try
            {
                for (int i = 0; i < checkedListBox2.Items.Count && i < _copiedLandSettings.CheckedList.Count; i++)
                {
                    checkedListBox2.SetItemChecked(i, _copiedLandSettings.CheckedList[i]);
                }
            }
            finally
            {
                _changingIndex = false;
            }

            land.Flags = FlagsFromList(checkedListBox2, land.Flags);
            textBoxNameLand.Text = land.Name;
            textBoxTexID.Text = land.TextureId.ToString();

            // Visual update of the node
            selectedNode.Text = $"0x{index:X4} {land.Name}";

            // Save the changes (can be undone)
            TileDataHistory.Record($"Insert settings on land 0x{index:X4}", null, new[] { index });
            TileData.LandTable[index] = land;

            // Optional: Mark the node as changed and update the event
            selectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index);
            
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = "sound.wav";
            player.Play();
        }
        #endregion

        #region [ class LandSettings ]
        public class LandSettings
        {
            public string Name { get; set; }
            public ushort TextureId { get; set; }
            public List<bool> CheckedList { get; set; } // Stores the CheckedListBox values
        }
        #endregion

        #region [ AssignToolTipsToLabels ]
        private void AssignToolTipsToLabels()
        {
            _image = Properties.Resources.MakeChairsUseable;

            // Statics
            toolTipComponent.SetToolTip(nameLabel, GetDescription(nameLabel));
            toolTipComponent.SetToolTip(animLabel, GetDescription(animLabel));
            toolTipComponent.SetToolTip(weightLabel, GetDescription(weightLabel));
            toolTipComponent.SetToolTip(layerLabel, GetDescription(layerLabel));
            toolTipComponent.SetToolTip(quantityLabel, GetDescription(quantityLabel));
            toolTipComponent.SetToolTip(valueLabel, GetDescription(valueLabel));
            toolTipComponent.SetToolTip(stackOffLabel, GetDescription(stackOffLabel));
            toolTipComponent.SetToolTip(hueLabel, GetDescription(hueLabel));
            toolTipComponent.SetToolTip(unknown2Label, GetDescription(unknown2Label));
            toolTipComponent.SetToolTip(miscDataLabel, GetDescription(miscDataLabel));
            toolTipComponent.SetToolTip(heightLabel, GetDescription(heightLabel));
            toolTipComponent.SetToolTip(unknown3Label, GetDescription(unknown3Label));

            // Land Tiles
            toolTipComponent.SetToolTip(landNameLabel, GetDescription(landNameLabel));
            toolTipComponent.SetToolTip(landTexIdLabel, GetDescription(landTexIdLabel));

            // Edit Cou Files
            toolTipComponent.SetToolTip(lbcomboBoxLoadText, GetDescription(lbcomboBoxLoadText));
        }
        #endregion

        #region [ GetDescription ]
        private string GetDescription(object sender)
        {
            string description = string.Empty;

            if (sender == nameLabel)
            {
                description = "This field is for the name of the item, which can be a maximum of 20 characters.";
            }
            else if (sender == animLabel)
            {
                description = "This field is for the animation ID associated with the item.";
            }
            else if (sender == weightLabel)
            {
                description = "This field is for the weight of the item.";
            }
            else if (sender == layerLabel)
            {
                description = new StringBuilder()
                    .AppendLine("This field is for the layer of the item:")
                    .AppendLine("")
                    .AppendLine("1 One handed weapon")
                    .AppendLine("2 Two handed weapon, shield, or misc.")
                    .AppendLine("3 Shoes")
                    .AppendLine("4 Pants")
                    .AppendLine("5 Shirt")
                    .AppendLine("6 Helm / Line")
                    .AppendLine("7 Gloves")
                    .AppendLine("8 Ring")
                    .AppendLine("9 Talisman")
                    .AppendLine("10 Neck")
                    .AppendLine("11 Hair")
                    .AppendLine("12 Waist (half apron)")
                    .AppendLine("13 Torso (inner) (chest armor)")
                    .AppendLine("14 Bracelet")
                    .AppendLine("15 Unused (but backpackers for backpackers go to 21)")
                    .AppendLine("16 Facial Hair")
                    .AppendLine("17 Torso (middle) (surcoat, tunic, full apron, sash)")
                    .AppendLine("18 Earrings")
                    .AppendLine("19 Arms")
                    .AppendLine("20 Back (cloak)")
                    .AppendLine("21 Backpack")
                    .AppendLine("22 Torso (outer) (robe)")
                    .AppendLine("23 Legs (outer) (skirt / kilt)")
                    .AppendLine("24 Legs (inner) (leg armor)")
                    .AppendLine("25 Mount (horse, ostard, etc)")
                    .AppendLine("26 NPC Buy Restock container")
                    .AppendLine("27 NPC Buy no restock container")
                    .AppendLine("28 NPC Sell container")
                    .ToString();
            }
            else if (sender == quantityLabel)
            {
                description = "This field is for the quantity of the item.";
            }
            else if (sender == valueLabel)
            {
                description = "This field is for the value of the item.";
            }
            else if (sender == stackOffLabel)
            {
                description = new StringBuilder()
                    .AppendLine("StackOff refers to the stacking offset in pixels when multiple items are stacked.")
                    .AppendLine("A higher StackOff value means the items will appear further apart from each other within the stack.")
                    .ToString();
            }
            else if (sender == hueLabel)
            {
                description = "This field is for the hue (color) of the item.";
            }
            else if (sender == unknown2Label)
            {
                description = "This field is for the second unknown value.";
            }
            else if (sender == miscDataLabel)
            {
                description = "Old UO Demo weapon template definition";
            }
            else if (sender == heightLabel)
            {
                description = "This field is for the height of the item.";
            }
            else if (sender == unknown3Label)
            {
                description = "This field is for the third unknown value.";
            }
            else if (sender == landNameLabel)
            {
                description = "This field is for the name of the land tile, which can be a maximum of 20 characters.";
            }
            else if (sender == landTexIdLabel)
            {
                description = "This field is for the texture ID associated with the land tile.";
            }
            else if (sender == lbcomboBoxLoadText)
            {
                description = new StringBuilder()
                         .AppendLine("This is how you load the text files for editing into the rich text box to edit the text files.")
                         .AppendLine("This allows you to embed new chairs, new colors, containers, and more for the client.")
                         .ToString();
            }

            return description;
        }
        #endregion
    }
}
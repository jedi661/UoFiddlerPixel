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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using AnimatedGif;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class AnimationListControl : UserControl
    {

        public AnimationListControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        #region [ GetActionNames ]
        public string[][] GetActionNames { get; } = {
            // Monster
            new[]
            {
                "Walk",
                "Idle",
                "Die1",
                "Die2",
                "Attack1",
                "Attack2",
                "Attack3",
                "AttackBow",
                "AttackCrossBow",
                "AttackThrow",
                "GetHit",
                "Pillage",
                "Stomp",
                "Cast2",
                "Cast3",
                "BlockRight",
                "BlockLeft",
                "Idle",
                "Fidget",
                "Fly",
                "TakeOff",
                "GetHitInAir"
            },
            // Sea
            new[]
            {
                "Walk",
                "Run",
                "Idle",
                "Idle",
                "Fidget",
                "Attack1",
                "Attack2",
                "GetHit",
                "Die1"
            },
            // Animal
            new[]
            {
                "Walk",
                "Run",
                "Idle",
                "Eat",
                "Alert",
                "Attack1",
                "Attack2",
                "GetHit",
                "Die1",
                "Idle",
                "Fidget",
                "LieDown",
                "Die2"
            },
            // Human
            new[]
            {
                "Walk_01",
                "WalkStaff_01",
                "Run_01",
                "RunStaff_01",
                "Idle_01",
                "Idle_01",
                "Fidget_Yawn_Stretch_01",
                "CombatIdle1H_01",
                "CombatIdle1H_01",
                "AttackSlash1H_01",
                "AttackPierce1H_01",
                "AttackBash1H_01",
                "AttackBash2H_01",
                "AttackSlash2H_01",
                "AttackPierce2H_01",
                "CombatAdvance_1H_01",
                "Spell1",
                "Spell2",
                "AttackBow_01",
                "AttackCrossbow_01",
                "GetHit_Fr_Hi_01",
                "Die_Hard_Fwd_01",
                "Die_Hard_Back_01",
                "Horse_Walk_01",
                "Horse_Run_01",
                "Horse_Idle_01",
                "Horse_Attack1H_SlashRight_01",
                "Horse_AttackBow_01",
                "Horse_AttackCrossbow_01",
                "Horse_Attack2H_SlashRight_01",
                "Block_Shield_Hard_01",
                "Punch_Punch_Jab_01",
                "Bow_Lesser_01",
                "Salute_Armed1h_01",
                "Ingest_Eat_01"
            }
        };
        #endregion

        private Bitmap _mainPicture;
        private int _currentSelect;
        private int _currentSelectAction;
        private bool _animate;
        private int _frameIndex;
        private Bitmap[] _animationList;
        private bool _imageInvalidated = true;
        private Timer _timer;
        private AnimationFrame[] _frames;
        private int _customHue;
        private int _defHue;
        private int _facing = 1;
        private bool _sortAlpha;
        private int _displayType;
        private bool _loaded;

        #region  [ Reload ]
        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (!_loaded)
            {
                return;
            }

            _mainPicture = null;
            _currentSelect = 0;
            _currentSelectAction = 0;
            _animate = false;
            _imageInvalidated = true;
            StopAnimation();
            _frames = null;
            _customHue = 0;
            _defHue = 0;
            _facing = 1;
            _sortAlpha = false;
            _displayType = 0;
            OnLoad(this, EventArgs.Empty);
        }
        #endregion

        #region [ OnLoad ]
        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Animations"] = true;
            Options.LoadedUltimaClass["Hues"] = true;

            TreeViewMobs.TreeViewNodeSorter = new GraphicSorter();

            if (!LoadXml())
            {
                Cursor.Current = Cursors.Default;
                return;
            }

            LoadListView();
            extractAnimationToolStripMenuItem.Visible = false;
            _currentSelect = 0;
            _currentSelectAction = 0;

            if (TreeViewMobs.Nodes[0].Nodes.Count > 0)
            {
                TreeViewMobs.SelectedNode = TreeViewMobs.Nodes[0].Nodes[0];
            }

            FacingBar.Value = (_facing + 3) & 7;

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            _loaded = true;
            Cursor.Current = Cursors.Default;
        }
        #endregion

        #region  [ OnFilePathChangeEvent ]
        private void OnFilePathChangeEvent()
        {
            Reload();
        }
        #endregion

        #region [ ChangeHue ]
        /// <summary>
        /// Changes Hue of current Mob
        /// </summary>
        /// <param name="select"></param>
        public void ChangeHue(int select)
        {
            _customHue = select + 1;
            CurrentSelect = CurrentSelect;
        }
        #endregion

        #region IsAlreadyDefine
        /// <summary>
        /// Is Graphic already in TreeView
        /// </summary>
        /// <param name="graphic"></param>
        /// <returns></returns>
        public bool IsAlreadyDefined(int graphic)
        {
            return TreeViewMobs.Nodes[0].Nodes.Cast<TreeNode>().Any(node => ((int[])node.Tag)[0] == graphic) ||
                   TreeViewMobs.Nodes[1].Nodes.Cast<TreeNode>().Any(node => ((int[])node.Tag)[0] == graphic);
        }
        #endregion

        #region [ AddGraphic ]
        /// <summary>
        /// Adds Graphic with type and name to List
        /// </summary>
        /// <param name="graphic"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        public void AddGraphic(int graphic, int type, string name)
        {
            TreeViewMobs.BeginUpdate();
            TreeViewMobs.TreeViewNodeSorter = null;
            TreeNode nodeParent = new TreeNode(name)
            {
                Tag = new[] { graphic, type },
                ToolTipText = Animations.GetFileName(graphic)
            };

            if (type == 4)
            {
                TreeViewMobs.Nodes[1].Nodes.Add(nodeParent);
                type = 3;
            }
            else
            {
                TreeViewMobs.Nodes[0].Nodes.Add(nodeParent);
            }

            for (int i = 0; i < GetActionNames[type].GetLength(0); ++i)
            {
                if (!Animations.IsActionDefined(graphic, i, 0))
                {
                    continue;
                }

                TreeNode node = new TreeNode($"{i} {GetActionNames[type][i]}")
                {
                    Tag = i
                };

                nodeParent.Nodes.Add(node);
            }

            TreeViewMobs.TreeViewNodeSorter = !_sortAlpha
                ? new GraphicSorter()
                : (IComparer)new AlphaSorter();

            TreeViewMobs.Sort();
            TreeViewMobs.EndUpdate();
            LoadListView();
            TreeViewMobs.SelectedNode = nodeParent;
            nodeParent.EnsureVisible();
        }
        #endregion

        #region [ Animate ]
        private bool Animate
        {
            get => _animate;
            set
            {
                if (_animate == value)
                {
                    return;
                }

                _animate = value;
                extractAnimationToolStripMenuItem.Visible = _animate;
                StopAnimation();
                _imageInvalidated = true;
                MainPictureBox.Invalidate();
            }
        }
        #endregion

        #region [ StopAnimation ]
        private void StopAnimation()
        {
            if (_timer != null)
            {
                if (_timer.Enabled)
                {
                    _timer.Stop();
                }

                _timer.Dispose();
                _timer = null;
            }

            if (_animationList != null)
            {
                foreach (var animationBmp in _animationList)
                {
                    animationBmp?.Dispose();
                }
            }

            _animationList = null;
            _frameIndex = 0;
        }
        #endregion

        #region [ CurrentSelect ]
        private int CurrentSelect
        {
            get => _currentSelect;
            set
            {
                _currentSelect = value;
                if (_timer != null)
                {
                    if (_timer.Enabled)
                    {
                        _timer.Stop();
                    }

                    _timer.Dispose();
                    _timer = null;
                }
                SetPicture();
                MainPictureBox.Invalidate();
            }
        }
        #endregion

        #region [ SetPicture ]
        private void SetPicture()
        {
            _frames = null;
            _mainPicture?.Dispose();
            if (_currentSelect == 0)
            {
                return;
            }

            if (Animate)
            {
                _mainPicture = DoAnimation();
            }
            else
            {
                int body = _currentSelect;
                Animations.Translate(ref body);
                int hue = _customHue;
                if (hue != 0)
                {
                    _frames = Animations.GetAnimation(_currentSelect, _currentSelectAction, _facing, ref hue, true, false);
                }
                else
                {
                    _frames = Animations.GetAnimation(_currentSelect, _currentSelectAction, _facing, ref hue, false, false);
                    _defHue = hue;
                }

                if (_frames != null)
                {
                    if (_frames[0].Bitmap != null)
                    {
                        _mainPicture = new Bitmap(_frames[0].Bitmap);
                        BaseGraphicLabel.Text = $"BaseGraphic: {body} (0x{body:X})";
                        GraphicLabel.Text = $"Graphic: {_currentSelect} (0x{_currentSelect:X})";
                        HueLabel.Text = $"Hue: {hue + 1} (0x{hue + 1:X})";
                    }
                    else
                    {
                        _mainPicture = null;
                    }
                }
                else
                {
                    _mainPicture = null;
                }
            }
        }
        #endregion

        #region [ Bitmap DoAnimation ]
        private Bitmap DoAnimation()
        {
            if (_timer != null)
            {
                return _animationList[_frameIndex] != null
                    ? new Bitmap(_animationList[_frameIndex])
                    : null;
            }

            int body = _currentSelect;
            Animations.Translate(ref body);
            int hue = _customHue;
            if (hue != 0)
            {
                _frames = Animations.GetAnimation(_currentSelect, _currentSelectAction, _facing, ref hue, true, false);
            }
            else
            {
                _frames = Animations.GetAnimation(_currentSelect, _currentSelectAction, _facing, ref hue, false, false);
                _defHue = hue;
            }

            if (_frames == null)
            {
                return null;
            }

            BaseGraphicLabel.Text = $"BaseGraphic: {body} (0x{body:X})";
            GraphicLabel.Text = $"Graphic: {_currentSelect} (0x{_currentSelect:X})";
            HueLabel.Text = $"Hue: {hue + 1} (0x{hue + 1:X})";
            int count = _frames.Length;
            _animationList = new Bitmap[count];

            for (int i = 0; i < count; ++i)
            {
                _animationList[i] = _frames[i].Bitmap;
            }

            // Check invalid count with division by 0
            if (count <= 0)
            {
                count = 1;
            }

            _timer = new Timer
            {
                Interval = 1000 / count
            };
            _timer.Tick += AnimTick;
            _timer.Start();
            _frameIndex = 0;

            LoadListViewFrames(); // Reload frames

            return _animationList[0] != null ? new Bitmap(_animationList[0]) : null;
        }
        #endregion

        #region [ AnimTick ]
        private void AnimTick(object sender, EventArgs e)
        {
            ++_frameIndex;

            if (_frameIndex == _animationList.Length)
            {
                _frameIndex = 0;
            }

            _imageInvalidated = true;

            MainPictureBox.Invalidate();
        }
        #endregion

        #region [ OnPaint_MainPicture ]
        private void OnPaint_MainPicture(object sender, PaintEventArgs e)
        {
            if (_imageInvalidated)
            {
                SetPicture();
            }

            if (_mainPicture != null)
            {
                Point location = Point.Empty;
                Size size = _mainPicture.Size;
                location.X = (MainPictureBox.Width - _mainPicture.Width) / 2;
                location.Y = (MainPictureBox.Height - _mainPicture.Height) / 2;

                Rectangle destRect = new Rectangle(location, size);

                e.Graphics.DrawImage(_mainPicture, destRect, 0, 0, _mainPicture.Width, _mainPicture.Height, GraphicsUnit.Pixel);
            }
            else
            {
                _mainPicture = null;
            }
        }
        #endregion

        #region [ Load Dsiplay Frame and more listView1 ]
        private void TreeViewMobs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent != null)
            {
                if (e.Node.Parent.Name == "Mobs" || e.Node.Parent.Name == "Equipment")
                {
                    _currentSelectAction = 0;
                    CurrentSelect = ((int[])e.Node.Tag)[0];
                    if (e.Node.Parent.Name == "Mobs" && _displayType == 1)
                    {
                        _displayType = 0;
                        LoadListView();
                    }
                    else if (e.Node.Parent.Name == "Equipment" && _displayType == 0)
                    {
                        _displayType = 1;
                        LoadListView();
                    }

                    // Here, call the DoAnimation() method to initialize _animationList
                    DoAnimation();
                }
                else
                {
                    _currentSelectAction = (int)e.Node.Tag;
                    CurrentSelect = ((int[])e.Node.Parent.Tag)[0];
                    if (e.Node.Parent.Parent.Name == "Mobs" && _displayType == 1)
                    {
                        _displayType = 0;
                        LoadListView();
                    }
                    else if (e.Node.Parent.Parent.Name == "Equipment" && _displayType == 0)
                    {
                        _displayType = 1;
                        LoadListView();
                    }

                    // Here, call the DoAnimation() method to initialize _animationList
                    DoAnimation();
                }
            }
            else
            {
                if (e.Node.Name == "Mobs" && _displayType == 1)
                {
                    _displayType = 0;
                    LoadListView();
                }
                else if (e.Node.Name == "Equipment" && _displayType == 0)
                {
                    _displayType = 1;
                    LoadListView();
                }
                TreeViewMobs.SelectedNode = e.Node.Nodes[0];
            }

            // Here you can add the code to update the text of the toolStripStatusAnimLabel
            string fileName = Animations.GetFileName(_currentSelect);
            toolStripStatusAminLabel.Text = $"Source: {fileName}";
        }
        #endregion

        #region [ Animate_Click ]
        private void Animate_Click(object sender, EventArgs e)
        {
            Animate = !Animate;
        }
        #endregion

        #region [ LoadXml ]
        private bool LoadXml()
        {
            string fileName = Path.Combine(Options.AppDataPath, "Animationlist.xml");
            if (!File.Exists(fileName))
            {
                return false;
            }

            TreeViewMobs.BeginUpdate();
            try
            {
                TreeViewMobs.Nodes.Clear();

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.Load(fileName);
                }
                catch (XmlException ex)
                {
                    MessageBox.Show("There was a problem loading the XML file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                XmlElement xMobs = dom["Graphics"];
                List<TreeNode> nodes = new List<TreeNode>();
                TreeNode node;
                TreeNode typeNode;

                TreeNode rootNode = new TreeNode("Mobs")
                {
                    Name = "Mobs",
                    Tag = -1
                };
                nodes.Add(rootNode);

                foreach (XmlElement xMob in xMobs.SelectNodes("Mob"))
                {
                    string name = xMob.GetAttribute("name");
                    int value = int.Parse(xMob.GetAttribute("body"));
                    int type = int.Parse(xMob.GetAttribute("type"));
                    node = new TreeNode($"{name} (0x{value:X})")
                    {
                        Tag = new[] { value, type },
                        ToolTipText = Animations.GetFileName(value)
                    };
                    rootNode.Nodes.Add(node);

                    for (int i = 0; i < GetActionNames[type].GetLength(0); ++i)
                    {
                        if (!Animations.IsActionDefined(value, i, 0))
                        {
                            continue;
                        }

                        typeNode = new TreeNode($"{i} {GetActionNames[type][i]}")
                        {
                            Tag = i
                        };
                        node.Nodes.Add(typeNode);
                    }
                }

                rootNode = new TreeNode("Equipment")
                {
                    Name = "Equipment",
                    Tag = -2
                };
                nodes.Add(rootNode);

                foreach (XmlElement xMob in xMobs.SelectNodes("Equip"))
                {
                    string name = xMob.GetAttribute("name");
                    int value = int.Parse(xMob.GetAttribute("body"));
                    int type = int.Parse(xMob.GetAttribute("type"));
                    node = new TreeNode(name)
                    {
                        Tag = new[] { value, type },
                        ToolTipText = Animations.GetFileName(value)
                    };
                    rootNode.Nodes.Add(node);

                    for (int i = 0; i < GetActionNames[type].GetLength(0); ++i)
                    {
                        if (!Animations.IsActionDefined(value, i, 0))
                        {
                            continue;
                        }

                        typeNode = new TreeNode($"{i} {GetActionNames[type][i]}")
                        {
                            Tag = i
                        };
                        node.Nodes.Add(typeNode);
                    }
                }
                TreeViewMobs.Nodes.AddRange(nodes.ToArray());
                nodes.Clear();
            }
            finally
            {
                TreeViewMobs.EndUpdate();
            }

            return true;
        }
        #endregion

        #region [ LoadListView ]
        private void LoadListView()
        {
            listView.BeginUpdate();
            try
            {
                listView.Clear();
                foreach (TreeNode node in TreeViewMobs.Nodes[_displayType].Nodes)
                {
                    ListViewItem item = new ListViewItem($"({((int[])node.Tag)[0]})", 0)
                    {
                        Tag = ((int[])node.Tag)[0]
                    };
                    listView.Items.Add(item);
                }
            }
            finally
            {
                listView.EndUpdate();
            }
        }
        #endregion

        #region [ SelectChanged_listView ]
        private void SelectChanged_listView(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                TreeViewMobs.SelectedNode = TreeViewMobs.Nodes[_displayType].Nodes[listView.SelectedItems[0].Index];
            }
        }
        #endregion

        #region [ ListView_DoubleClick ]
        private void ListView_DoubleClick(object sender, MouseEventArgs e)
        {
            tabControl1.SelectTab(tabPage1);
        }
        #endregion

        #region [ ListViewDrawItem ]       
        private void ListViewDrawItem(object sender, DrawListViewItemEventArgs e)
        {
            int graphic = (int)e.Item.Tag;
            int hue = 0;
            _frames = Animations.GetAnimation(graphic, 0, 1, ref hue, false, true);

            if (_frames == null)
            {
                return;
            }

            Bitmap bmp = _frames[0].Bitmap;
            int width = bmp.Width;
            int height = bmp.Height;

            if (width > e.Bounds.Width)
            {
                width = e.Bounds.Width;
            }

            if (height > e.Bounds.Height)
            {
                height = e.Bounds.Height;
            }

            
            if (listView.SelectedItems.Contains(e.Item))
            {                
                e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);
            }

            e.Graphics.DrawImage(bmp, e.Bounds.X, e.Bounds.Y, width, height);
            e.DrawText(TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter);

            if (listView.SelectedItems.Contains(e.Item))
            {
                e.DrawFocusRectangle();
            }
            else
            {
                using (var pen = new Pen(Color.Gray))
                {
                    e.Graphics.DrawRectangle(pen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                }
            }
        }
        #endregion

        #region [ OnClick_Hue ]
        private HuePopUpForm _showForm;
        private void OnClick_Hue(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = _customHue == 0
                ? new HuePopUpForm(ChangeHue, _defHue + 1)
                : new HuePopUpForm(ChangeHue, _customHue - 1);

            _showForm.TopMost = true;
            _showForm.Show();
        }
        #endregion

        #region [ LoadListViewFrames ]
        private void LoadListViewFrames()
        {
            if (_animationList == null || _animationList.Length == 0)
            {
                return;
            }

            listView1.BeginUpdate();
            try
            {
                listView1.Items.Clear();
                for (int frame = 0; frame < _animationList.Length; ++frame)
                {
                    ListViewItem item = new ListViewItem(frame.ToString(), 0)
                    {
                        Tag = frame
                    };
                    listView1.Items.Add(item);
                }
            }
            finally
            {
                listView1.EndUpdate();
                listView1.Invalidate(); // Make sure the ListView is updated
            }
        }
        #endregion

        #region [ Frames_ListView_DrawItem ]
        private void Frames_ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (_animationList == null)
            {
                return;
            }

            Bitmap bmp = _animationList[(int)e.Item.Tag];
            int width = bmp.Width;
            int height = bmp.Height;

            if (width > e.Bounds.Width)
            {
                width = e.Bounds.Width;
            }
            if (height > e.Bounds.Height)
            {
                height = e.Bounds.Height;
            }

            // Verify that the current item is selected
            if (listView1.SelectedItems.Contains(e.Item))
            {
                // Change the background color of the selected item
                e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);
            }

            e.Graphics.DrawImage(bmp, e.Bounds.X, e.Bounds.Y, width, height);
            e.DrawText(TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter);

            using (var pen = new Pen(Color.Gray))
            {
                e.Graphics.DrawRectangle(pen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            }
        }
        #endregion

        #region [ OnScrollFacing ]
        private void OnScrollFacing(object sender, EventArgs e)
        {
            _facing = (FacingBar.Value - 3) & 7;
            CurrentSelect = CurrentSelect;

            // Reload frames for the currently selected element
            DoAnimation();

            if (tabControl1.SelectedTab == tabPage3)
            {
                LoadListViewFrames();
                listView1.Invalidate(); // Ensures that the ListView is updated
            }
            else
            {
                SetPicture();
                MainPictureBox.Invalidate();
            }
        }
        #endregion

        #region [ tabControl1_Selected ]
        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == tabPage3)
            {
                LoadListViewFrames();
                listView1.Invalidate();
            }
            else
            {
                SetPicture();
                MainPictureBox.Invalidate();
            }
        }
        #endregion

        #region [ OnClick_Sort ]
        private void OnClick_Sort(object sender, EventArgs e)
        {
            _sortAlpha = !_sortAlpha;

            TreeViewMobs.BeginUpdate();
            try
            {
                TreeViewMobs.TreeViewNodeSorter = !_sortAlpha ? new GraphicSorter() : (IComparer)new AlphaSorter();
                TreeViewMobs.Sort();
            }
            finally
            {
                TreeViewMobs.EndUpdate();
            }

            LoadListView();
        }
        #endregion

        #region [ OnClickRemove ]
        private void OnClickRemove(object sender, EventArgs e)
        {
            TreeNode node = TreeViewMobs.SelectedNode;
            if (node?.Parent == null)
            {
                return;
            }

            if (node.Parent.Name != "Mobs" && node.Parent.Name != "Equipment")
            {
                node = node.Parent;
            }

            node.Remove();
            LoadListView();
        }
        #endregion

        #region [ OnClickAnimationEdit ]
        private AnimationEditForm _animEditFormEntry;        
        private void OnClickAnimationEdit(object sender, EventArgs e)
        {
            if (_animEditFormEntry?.IsDisposed == false)
            {
                return;
            }

            _animEditFormEntry = new AnimationEditForm();
            //animEditEntry.TopMost = true; // TODO: should it be topMost?
            _animEditFormEntry.Show();
        }
        #endregion

        #region [ OnClickFindNewEntries ]
        private AnimationListNewEntriesForm _animNewEntryForm;
        private void OnClickFindNewEntries(object sender, EventArgs e)
        {
            if (_animNewEntryForm?.IsDisposed == false)
            {
                return;
            }

            _animNewEntryForm = new AnimationListNewEntriesForm(IsAlreadyDefined, AddGraphic, GetActionNames)
            {
                TopMost = true
            };
            _animNewEntryForm.Show();
        }
        #endregion

        #region [ RewriteXml ]
        private void RewriteXml(object sender, EventArgs e)
        {
            TreeViewMobs.BeginUpdate();
            try
            {
                TreeViewMobs.TreeViewNodeSorter = new GraphicSorter();
                TreeViewMobs.Sort();
            }
            finally
            {
                TreeViewMobs.EndUpdate();
            }

            string fileName = Path.Combine(Options.OutputPath, "Animationlist.xml"); //Correct save path

            XmlDocument dom = new XmlDocument();
            XmlDeclaration decl = dom.CreateXmlDeclaration("1.0", "utf-8", null);
            dom.AppendChild(decl);
            XmlElement sr = dom.CreateElement("Graphics");
            XmlComment comment = dom.CreateComment("Entries in Mob tab");
            sr.AppendChild(comment);
            comment = dom.CreateComment("Name=Displayed name");
            sr.AppendChild(comment);
            comment = dom.CreateComment("body=Graphic");
            sr.AppendChild(comment);
            comment = dom.CreateComment("type=0:Monster, 1:Sea, 2:Animal, 3:Human/Equipment");
            sr.AppendChild(comment);

            XmlElement elem;
            foreach (TreeNode node in TreeViewMobs.Nodes[0].Nodes)
            {
                elem = dom.CreateElement("Mob");
                elem.SetAttribute("name", node.Text);
                elem.SetAttribute("body", ((int[])node.Tag)[0].ToString());
                elem.SetAttribute("type", ((int[])node.Tag)[1].ToString());

                sr.AppendChild(elem);
            }

            foreach (TreeNode node in TreeViewMobs.Nodes[1].Nodes)
            {
                elem = dom.CreateElement("Equip");
                elem.SetAttribute("name", node.Text);
                elem.SetAttribute("body", ((int[])node.Tag)[0].ToString());
                elem.SetAttribute("type", ((int[])node.Tag)[1].ToString());
                sr.AppendChild(elem);
            }

            dom.AppendChild(sr);

            // Save the XML file in the Options.OutputPath directory
            dom.Save(fileName);

            MessageBox.Show("XML saved", "Rewrite", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region [ RewriteXml2 ]
        private void RewriteXml2(object sender, EventArgs e)
        {
            TreeViewMobs.BeginUpdate();
            try
            {
                TreeViewMobs.TreeViewNodeSorter = new GraphicSorter();
                TreeViewMobs.Sort();
            }
            finally
            {
                TreeViewMobs.EndUpdate();
            }

            string fileName = Path.Combine(Options.OutputPath, "Animationlist.xml"); // Correct save path

            string CleanNodeName(string nodeName)
            {
                // Check if the node name contains (0x1), (0x2), etc.
                // If yes, then remove them and the ID inside the parentheses
                int index;
                while ((index = nodeName.IndexOf(" (0x")) >= 0)
                {
                    nodeName = nodeName.Substring(0, index);
                }
                return nodeName;
            }

            XmlDocument dom = new XmlDocument();
            XmlDeclaration decl = dom.CreateXmlDeclaration("1.0", "utf-8", null);
            dom.AppendChild(decl);
            XmlElement sr = dom.CreateElement("Graphics");
            XmlComment comment = dom.CreateComment("Entries in Mob tab");
            sr.AppendChild(comment);
            comment = dom.CreateComment("Name=Displayed name");
            sr.AppendChild(comment);
            comment = dom.CreateComment("body=Graphic");
            sr.AppendChild(comment);
            comment = dom.CreateComment("type=0:Monster, 1:Sea, 2:Animal, 3:Human/Equipment");
            sr.AppendChild(comment);

            XmlElement elem;
            foreach (TreeNode node in TreeViewMobs.Nodes[0].Nodes)
            {
                string nodeNameCleaned = CleanNodeName(node.Text);
                int nodeId = ((int[])node.Tag)[0]; // Assuming the ID is stored in the first element of the node.Tag array
                elem = dom.CreateElement("Mob");
                elem.SetAttribute("name", $"{nodeNameCleaned}");
                elem.SetAttribute("body", nodeId.ToString());
                elem.SetAttribute("type", ((int[])node.Tag)[1].ToString());

                sr.AppendChild(elem);
            }

            foreach (TreeNode node in TreeViewMobs.Nodes[1].Nodes)
            {
                string nodeNameCleaned = CleanNodeName(node.Text);
                int nodeId = ((int[])node.Tag)[0]; // Assuming the ID is stored in the first element of the node.Tag array
                elem = dom.CreateElement("Equip");
                elem.SetAttribute("name", $"{nodeNameCleaned} ({nodeId})");
                elem.SetAttribute("body", nodeId.ToString());
                elem.SetAttribute("type", ((int[])node.Tag)[1].ToString());
                sr.AppendChild(elem);
            }

            dom.AppendChild(sr);

            // Save the XML file in the Options.OutputPath directory
            dom.Save(fileName);

            MessageBox.Show("XML saved", "Rewrite", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region [ Extract_Image_ClickBmp ]
        private void Extract_Image_ClickBmp(object sender, EventArgs e)
        {
            ExtractImage(ImageFormat.Bmp);
        }
        #endregion

        #region [ Extract_Image_ClickTiff ]
        private void Extract_Image_ClickTiff(object sender, EventArgs e)
        {
            ExtractImage(ImageFormat.Tiff);
        }
        #endregion

        #region [ Extract_Image_ClickJpg ]
        private void Extract_Image_ClickJpg(object sender, EventArgs e)
        {
            ExtractImage(ImageFormat.Jpeg);
        }
        #endregion

        #region [ Extract_Image_ClickPng ]
        private void Extract_Image_ClickPng(object sender, EventArgs e)
        {
            ExtractImage(ImageFormat.Png);
        }
        #endregion

        #region [ ExtractImage ]
        private void ExtractImage(ImageFormat imageFormat)
        {
            string what = "Mob";
            if (_displayType == 1)
            {
                what = "Equipment";
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"{what} {_currentSelect}.{fileExtension}");

            Bitmap sourceBitmap = Animate ? _animationList[0] : _mainPicture;
            using (Bitmap newBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height))
            {
                using (Graphics newGraph = Graphics.FromImage(newBitmap))
                {
                    newGraph.FillRectangle(Brushes.White, 0, 0, newBitmap.Width, newBitmap.Height);
                    newGraph.DrawImage(sourceBitmap, new Point(0, 0));
                    newGraph.Save();
                }

                newBitmap.Save(fileName, imageFormat);
            }

            MessageBox.Show($"{what} saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region [ OnClickExtractAnimBmp ]
        private void OnClickExtractAnimBmp(object sender, EventArgs e)
        {
            ExportAnimationFrames(ImageFormat.Bmp);
        }
        #endregion

        #region [ OnClickExtractAnimTiff ]
        private void OnClickExtractAnimTiff(object sender, EventArgs e)
        {
            ExportAnimationFrames(ImageFormat.Tiff);
        }
        #endregion

        #region [ OnClickExtractAnimJpg ]
        private void OnClickExtractAnimJpg(object sender, EventArgs e)
        {
            ExportAnimationFrames(ImageFormat.Jpeg);
        }
        #endregion

        #region [ OnClickExtractAnimPng ]
        private void OnClickExtractAnimPng(object sender, EventArgs e)
        {
            ExportAnimationFrames(ImageFormat.Png);
        }
        #endregion

        #region [ ExportAnimationFrames ]
        private void ExportAnimationFrames(ImageFormat imageFormat)
        {
            if (!Animate)
            {
                return;
            }

            string what = "Mob";
            if (_displayType == 1)
            {
                what = "Equipment";
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"{what} {_currentSelect}");

            for (int i = 0; i < _animationList.Length; ++i)
            {
                using (Bitmap newBitmap = new Bitmap(_animationList[i].Width, _animationList[i].Height))
                {
                    using (Graphics newGraph = Graphics.FromImage(newBitmap))
                    {
                        newGraph.FillRectangle(Brushes.White, 0, 0, newBitmap.Width, newBitmap.Height);
                        newGraph.DrawImage(_animationList[i], new Point(0, 0));
                        newGraph.Save();
                    }

                    newBitmap.Save($"{fileName}-{i}.{fileExtension}", imageFormat);
                }
            }

            MessageBox.Show($"{what} saved to '{fileName}-X.{fileExtension}'", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region [ OnClickExportFrameBmp ]
        private void OnClickExportFrameBmp(object sender, EventArgs e)
        {
            ExportSingleFrame(ImageFormat.Bmp);
        }
        #endregion

        #region [ OnClickExportFrameTiff ]
        private void OnClickExportFrameTiff(object sender, EventArgs e)
        {
            ExportSingleFrame(ImageFormat.Tiff);
        }
        #endregion

        #region [ OnClickExportFrameJpg ]
        private void OnClickExportFrameJpg(object sender, EventArgs e)
        {
            ExportSingleFrame(ImageFormat.Jpeg);
        }
        #endregion

        #region [ OnClickExportFramePng ]
        private void OnClickExportFramePng(object sender, EventArgs e)
        {
            ExportSingleFrame(ImageFormat.Png);
        }
        #endregion

        #region [ ExportSingleFrame ]
        private void ExportSingleFrame(ImageFormat imageFormat)
        {
            if (listView1.SelectedItems.Count < 1)
            {
                return;
            }

            string what = "Mob";
            if (_displayType == 1)
            {
                what = "Equipment";
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"{what} {_currentSelect}");

            Bitmap bit = _animationList[(int)listView1.SelectedItems[0].Tag];
            using (Bitmap newBitmap = new Bitmap(bit.Width, bit.Height))
            {
                using (Graphics newGraph = Graphics.FromImage(newBitmap))
                {
                    newGraph.FillRectangle(Brushes.White, 0, 0, newBitmap.Width, newBitmap.Height);
                    newGraph.DrawImage(bit, new Point(0, 0));
                    newGraph.Save();
                }

                newBitmap.Save($"{fileName}-{(int)listView1.SelectedItems[0].Tag}.{fileExtension}", imageFormat);
            }
        }
        #endregion

        #region [ OnClickCopyFrameToClipboard ]
        private void CopyFrameToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if a frame is selected in listView1
            if (listView1.SelectedItems.Count > 0)
            {
                // Copy the selected frame to the clipboard
                int selectedFrameIndex = (int)listView1.SelectedItems[0].Tag;
                if (selectedFrameIndex >= 0 && selectedFrameIndex < _animationList.Length)
                {
                    Clipboard.SetImage(_animationList[selectedFrameIndex]);

                    // Display a MessageBox to confirm that the graphic was copied
                    MessageBox.Show("The graphic was copied to the clipboard.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        #endregion

        #region [ importImage ]
        private void ImportImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if a frame is selected in listView1
            if (listView1.SelectedItems.Count > 0)
            {
                // Check if there is an image in the cache
                if (Clipboard.ContainsImage())
                {
                    int selectedFrameIndex = (int)listView1.SelectedItems[0].Tag;
                    if (selectedFrameIndex >= 0 && selectedFrameIndex < _animationList.Length)
                    {
                        // Import the graphic from the clipboard into the selected ListViewItem (frame).
                        Image imageFromClipboard = Clipboard.GetImage();
                        _animationList[selectedFrameIndex] = (Bitmap)imageFromClipboard;

                        // Here you can carry out further actions, e.g. B. View or save the updated graphic
                        // ...

                        // Refresh the ListView to show the changes
                        LoadListViewFrames();

                        // Display a MessageBox to confirm the graphic was imported
                        MessageBox.Show("The graphic has been imported and saved in the frame.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    // If there is no image in the cache, display an error message
                    MessageBox.Show("There is no image in the cache.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region [ animationlistEditToolStripMenuItem ]
        private AnimationListEditorForm _editorForm = null;

        private void AnimationlistEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = Path.Combine(Options.AppDataPath, "Animationlist.xml");

            if (_editorForm == null || _editorForm.IsDisposed)
            {
                _editorForm = new AnimationListEditorForm(fileName);
                _editorForm.Show();
            }
            else
            {
                // The shape is already open, so let's bring it to the foreground
                _editorForm.BringToFront();
            }
        }
        #endregion

        #region [ LoadFramesFromListView ]
        private void LoadFramesFromListView()
        {
            // Number of entries in the ListView
            int count = listView1.Items.Count;

            // Initialize the _frames array with the number of entries in the ListView
            _frames = new AnimationFrame[count];

            for (int i = 0; i < count; i++)
            {
                // Get the tag of the ListView item that represents the index of the frame
                int frameIndex = (int)listView1.Items[i].Tag;

                // Load the corresponding frame from _animationList
                Bitmap frameBitmap = _animationList[frameIndex];

                // Initialize the AnimationFrame with the bitmap
                _frames[i] = new AnimationFrame { Bitmap = frameBitmap };
            }
        }
        #endregion

        #region [ ExportAnimatedGif ]
        private void ExportAnimatedGif(bool looping)
        {
            // Check if the frames are loaded
            if (_frames == null || _frames.Length == 0)
            {
                MessageBox.Show("Frames not loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string baseFileName = $"{(_displayType == 1 ? "Equipment" : "Mob")} {_currentSelect}.gif";
            string outputFile = Path.Combine(Options.OutputPath, baseFileName);
            int fileIndex = 1;

            // Check whether the path exists and is writable
            if (!Directory.Exists(Options.OutputPath))
            {
                MessageBox.Show($"OutputPath {Options.OutputPath} does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Increment the filename if the file already exists
            while (File.Exists(outputFile))
            {
                outputFile = Path.Combine(Options.OutputPath, $"{(_displayType == 1 ? "Equipment" : "Mob")} {_currentSelect} ({fileIndex}).gif");
                fileIndex++;
            }

            try
            {
                var maxFrameSize = new Size(0, 0);

                foreach (var frame in _frames)
                {
                    if (frame?.Bitmap != null)
                    {
                        maxFrameSize.Width = Math.Max(maxFrameSize.Width, frame.Bitmap.Width);
                        maxFrameSize.Height = Math.Max(maxFrameSize.Height, frame.Bitmap.Height);
                    }
                }

                using (var gif = AnimatedGif.AnimatedGif.Create(outputFile, delay: 150))
                {
                    foreach (var frame in _frames)
                    {
                        if (frame?.Bitmap == null)
                        {
                            continue;
                        }

                        using (Bitmap target = new Bitmap(maxFrameSize.Width, maxFrameSize.Height))
                        {
                            using (Graphics g = Graphics.FromImage(target))
                            {
                                g.DrawImage(frame.Bitmap, 0, 0);
                            }
                            gif.AddFrame(target, delay: -1, quality: GifQuality.Bit8);
                        }
                    }
                }

                if (!looping)
                {
                    using (var stream = new FileStream(outputFile, FileMode.Open, FileAccess.Write))
                    {
                        stream.Seek(28, SeekOrigin.Begin);
                        stream.WriteByte(0);
                    }
                }

                MessageBox.Show($"InGame Anim saved in {outputFile}", "Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating GIF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region [ OnClickExtractAnimGifLooping ]
        private void OnClickExtractAnimGifLooping(object sender, EventArgs e)
        {
            // Load frames from the ListView
            LoadFramesFromListView();
            ExportAnimatedGif(true);
        }
        #endregion

        #region [ OnClickExtractAnimGifNoLooping ]
        private void OnClickExtractAnimGifNoLooping(object sender, EventArgs e)
        {            
            LoadFramesFromListView();
            ExportAnimatedGif(false);
        }
        #endregion
    }

    #region [ class AlphaSorter ]
    public class AlphaSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            if (tx.Parent == null) // don't change Mob and Equipment
            {
                return (int)tx.Tag == -1 ? -1 : 1;
            }
            if (tx.Parent.Parent != null)
            {
                return (int)tx.Tag - (int)ty.Tag;
            }

            return string.CompareOrdinal(tx.Text, ty.Text);
        }
    }
    #endregion

    #region [ class GraphicSorter ]
    public class GraphicSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            if (tx.Parent == null)
            {
                return (int)tx.Tag == -1 ? -1 : 1;
            }

            if (tx.Parent.Parent != null)
            {
                return (int)tx.Tag - (int)ty.Tag;
            }

            int[] ix = (int[])tx.Tag;
            int[] iy = (int[])ty.Tag;

            if (ix[0] == iy[0])
            {
                return 0;
            }

            if (ix[0] < iy[0])
            {
                return -1;
            }

            return 1;
        }
    }
    #endregion
}

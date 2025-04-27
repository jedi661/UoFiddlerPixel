﻿/***************************************************************************
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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AnimatedGif;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    #region [ class AnimDataControl ]
    public partial class AnimDataControl : UserControl
    {
        public AnimDataControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        private static bool _loaded;
        private Animdata.AnimdataEntry _selAnimdataEntry;
        private int _currAnim;
        private int _curFrame;
        private Timer _mTimer;
        private int _timerFrame;
        private AnimDataImportForm _importForm;
        private AnimDataExportForm _exportForm;
        private HuePopUpForm _showForm;
        private int _customHue = 0;
        private bool _hueOnlyGray = false;

        // Must be non-null for synchronization. Can contain null values for invalid tiles.
        private List<Bitmap> _huedFrames = [];

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

        #region [ CurrAnim ]
        private int CurrAnim
        {
            get => _currAnim;
            set
            {
                if (!_loaded)
                {
                    return;
                }

                _selAnimdataEntry = Animdata.AnimData[value];
                if (_currAnim != value)
                {
                    treeViewFrames.BeginUpdate();
                    treeViewFrames.Nodes.Clear();
                    for (int i = 0; i < _selAnimdataEntry.FrameCount; ++i)
                    {
                        TreeNode node = new TreeNode();
                        int frame = value + _selAnimdataEntry.FrameData[i];
                        node.Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";
                        treeViewFrames.Nodes.Add(node);
                    }
                    treeViewFrames.EndUpdate();
                }
                _currAnim = value;

                if (_mTimer == null)
                {
                    int graphic = value;
                    if (_curFrame > -1)
                    {
                        graphic += _selAnimdataEntry.FrameData[_curFrame];
                    }

                    AminDataMainPictureBox.Image = Art.GetStatic(graphic);
                }
                numericUpDownFrameDelay.Value = _selAnimdataEntry.FrameInterval;
                numericUpDownStartDelay.Value = _selAnimdataEntry.FrameStart;
            }
        }
        #endregion

        #region [ GenerateFramedHues ]
        private void GenerateFramedHues()
        {
            if (_selAnimdataEntry != null)
            {
                // In a lock, since the Timer may read values from the list
                lock (_huedFrames)
                {
                    foreach (var huedFrame in _huedFrames)
                    {
                        huedFrame?.Dispose();
                    }
                    _huedFrames.Clear();

                    for (int i = 0; i < _selAnimdataEntry.FrameCount; ++i)
                    {
                        int graphic = _currAnim + _selAnimdataEntry.FrameData[i];
                        var huedFrame = Art.GetStatic(graphic);
                        if (huedFrame != null)
                        {
                            huedFrame = new Bitmap(huedFrame);
                            Hue hueObject = Hues.List[_customHue - 1];
                            hueObject.ApplyTo(huedFrame, _hueOnlyGray);
                        }
                        _huedFrames.Add(huedFrame);
                    }
                }
            }
        }
        #endregion

        #region [ Reload ]
        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (_loaded)
            {
                OnLoad(this, EventArgs.Empty);
            }
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
            Options.LoadedUltimaClass["Animdata"] = true;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();

            treeView1.TreeViewNodeSorter = new AnimdataSorter();

            foreach (int id in Animdata.AnimData.Keys)
            {
                Animdata.AnimdataEntry animdataEntry = Animdata.AnimData[id];
                TreeNode node = new TreeNode
                {
                    Tag = id,
                    Text = $"0x{id:X4} {TileData.ItemTable[id].Name}"
                };

                if (!Art.IsValidStatic(id))
                {
                    node.ForeColor = Color.Red;
                }
                else if ((TileData.ItemTable[id].Flags & TileFlag.Animation) == 0)
                {
                    node.ForeColor = Color.Blue;
                }

                // TODO: find a better approach to this
                // we need to fix invalid entries as there cannot be more than 64 frames
                if (animdataEntry.FrameCount > 64)
                {
                    animdataEntry.FrameCount = 64;
                    Options.ChangedUltimaClass["Animdata"] = true;
                }

                treeView1.Nodes.Add(node);

                for (int i = 0; i < animdataEntry.FrameCount; ++i)
                {
                    int frame = id + animdataEntry.FrameData[i];
                    if (Art.IsValidStatic(frame))
                    {
                        TreeNode subNode = new TreeNode
                        {
                            Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}"
                        };
                        node.Nodes.Add(subNode);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            treeView1.EndUpdate();

            if (treeView1.Nodes.Count > 0)
            {
                treeView1.SelectedNode = treeView1.Nodes[0];
            }

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            _loaded = true;
            Cursor.Current = Cursors.Default;

            // After all nodes are added, select the node with hex address 0x03ae
            SelectNodeById(ConvertHexAddressToId("0x03ae"));
        }
        #endregion

        #region [ SelectNodeById ]
        private void SelectNodeById(int id)
        {
            foreach (TreeNode node in treeView1.Nodes)
            {
                if ((int)node.Tag == id)
                {
                    treeView1.SelectedNode = node;
                    break;
                }
            }
        }
        #endregion

        #region [ ConvertHexAddressToId from SelectNodeById hex to id ]
        private int ConvertHexAddressToId(string hexAddress)
        {
            return int.Parse(hexAddress.Substring(2), System.Globalization.NumberStyles.HexNumber);
        }
        #endregion

        #region [ OnFilePathChangeEvent ]
        private void OnFilePathChangeEvent()
        {
            Reload();
        }
        #endregion

        #region [ AfterNodeSelect ]
        private void AfterNodeSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }

            if (treeView1.SelectedNode.Parent == null)
            {
                _curFrame = -1;
                CurrAnim = (int)treeView1.SelectedNode.Tag;
            }
            else
            {
                _curFrame = treeView1.SelectedNode.Index;
                CurrAnim = (int)treeView1.SelectedNode.Parent.Tag;
            }

            frameCountLabel.Text = $"Frames: {treeViewFrames.Nodes.Count}";
        }
        #endregion

        #region [ AfterSelectTreeViewFrames ]
        private void AfterSelectTreeViewFrames(object sender, TreeViewEventArgs e)
        {
            _curFrame = treeViewFrames.SelectedNode.Index;
            if (_mTimer != null)
            {
                StopTimer();
            }

            CurrAnim = CurrAnim;
            
            frameCountLabel.Text = $"Frames: {treeViewFrames.Nodes.Count}";
        }
        #endregion

        #region [ BtnOnClickExport ]
        private void BtnOnClickExport_Click(object sender, EventArgs e)
        {
            if (_exportForm?.IsDisposed == false)
            {
                _exportForm.Focus();
                return;
            }

            _exportForm = new AnimDataExportForm
            {
                TopMost = true
            };
            _exportForm.Show();
        }
        #endregion

        #region [ BtnOnClickImport ]
        private void BtnOnClickImport_Click(object sender, EventArgs e)
        {
            if (_importForm?.IsDisposed == false)
            {
                _importForm.Focus();
                return;
            }

            _importForm = new AnimDataImportForm
            {
                TopMost = true,
                OnAfterImport = Reload
            };
            _importForm.Show();
        }
        #endregion

        #region [ OnClickStartStop ]
        private void OnClickStartStop(object sender, EventArgs e)
        {
            if (_selAnimdataEntry != null)
            {
                if (_mTimer != null)
                {
                    StopTimer();
                }
                else
                {
                    _mTimer = new Timer
                    {
                        Interval = (100 * _selAnimdataEntry.FrameInterval) + 1
                    };
                    _mTimer.Tick += Timer_Tick;
                    _timerFrame = 0;
                    _mTimer.Start();
                }
            }
            else
            {
                // Handle the case where _selAnimdataEntry is null.                
            }
        }
        #endregion

        #region [ Timer_Tick ]
        private void Timer_Tick(object sender, EventArgs e)
        {
            ++_timerFrame;
            if (_timerFrame >= _selAnimdataEntry.FrameCount)
            {
                _timerFrame = 0;
            }

            AminDataMainPictureBox.Image = Art.GetStatic(CurrAnim + _selAnimdataEntry.FrameData[_timerFrame]);
        }
        #endregion

        #region [ OnValueChangedStartDelay ]
        private void OnValueChangedStartDelay(object sender, EventArgs e)
        {
            if (_selAnimdataEntry == null)
            {
                return;
            }

            if (_selAnimdataEntry.FrameStart == (byte)numericUpDownStartDelay.Value)
            {
                return;
            }

            _selAnimdataEntry.FrameStart = (byte)numericUpDownStartDelay.Value;
            Options.ChangedUltimaClass["Animdata"] = true;
        }
        #endregion

        #region [ OnValueChangedFrameDelay ]
        private void OnValueChangedFrameDelay(object sender, EventArgs e)
        {
            if (_selAnimdataEntry == null)
            {
                return;
            }

            if (_selAnimdataEntry.FrameInterval == (byte)numericUpDownFrameDelay.Value)
            {
                return;
            }

            _selAnimdataEntry.FrameInterval = (byte)numericUpDownFrameDelay.Value;
            if (_mTimer != null)
            {
                _mTimer.Interval = (100 * _selAnimdataEntry.FrameInterval) + 1;
            }

            Options.ChangedUltimaClass["Animdata"] = true;
        }
        #endregion

        #region [ OnClickFrameDown ]
        private void OnClickFrameDown(object sender, EventArgs e)
        {
            if (_selAnimdataEntry == null)
            {
                return;
            }

            if (treeViewFrames.Nodes.Count <= 1)
            {
                return;
            }

            if (treeViewFrames.SelectedNode == null)
            {
                return;
            }

            int index = treeViewFrames.SelectedNode.Index;
            if (index >= _selAnimdataEntry.FrameCount - 1)
            {
                return;
            }

            sbyte temp = _selAnimdataEntry.FrameData[index];
            _selAnimdataEntry.FrameData[index] = _selAnimdataEntry.FrameData[index + 1];
            _selAnimdataEntry.FrameData[index + 1] = temp;

            TreeNode listNode = treeView1.SelectedNode.Parent ?? treeView1.SelectedNode;
            int frame = CurrAnim + _selAnimdataEntry.FrameData[index];
            treeViewFrames.Nodes[index].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";
            listNode.Nodes[index].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";

            frame = CurrAnim + _selAnimdataEntry.FrameData[index + 1];
            treeViewFrames.Nodes[index + 1].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";
            listNode.Nodes[index + 1].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";

            treeViewFrames.SelectedNode = treeViewFrames.Nodes[index + 1];
            Options.ChangedUltimaClass["Animdata"] = true;
        }
        #endregion

        #region [ OnClickFrameU ]
        private void OnClickFrameUp(object sender, EventArgs e)
        {
            if (_selAnimdataEntry == null)
            {
                return;
            }

            if (treeViewFrames.Nodes.Count <= 1)
            {
                return;
            }

            if (treeViewFrames.SelectedNode == null)
            {
                return;
            }

            int index = treeViewFrames.SelectedNode.Index;
            if (index <= 0)
            {
                return;
            }

            sbyte temp = _selAnimdataEntry.FrameData[index];
            _selAnimdataEntry.FrameData[index] = _selAnimdataEntry.FrameData[index - 1];
            _selAnimdataEntry.FrameData[index - 1] = temp;

            TreeNode listNode = treeView1.SelectedNode.Parent ?? treeView1.SelectedNode;
            int frame = CurrAnim + _selAnimdataEntry.FrameData[index];
            treeViewFrames.Nodes[index].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";
            listNode.Nodes[index].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";

            frame = CurrAnim + _selAnimdataEntry.FrameData[index - 1];
            treeViewFrames.Nodes[index - 1].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";
            listNode.Nodes[index - 1].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";
            treeViewFrames.SelectedNode = treeViewFrames.Nodes[index - 1];

            Options.ChangedUltimaClass["Animdata"] = true;
        }
        #endregion

        #region  [ OnTextChanged ]
        private void OnTextChanged(object sender, EventArgs e)
        {
            bool canDone = Utils.ConvertStringToInt(textBoxAddFrame.Text, out int index);
            if (checkBoxRelative.Checked)
            {
                index += CurrAnim;
            }

            if (index > Art.GetMaxItemId() || index < 0)
            {
                canDone = false;
            }

            if (canDone)
            {
                textBoxAddFrame.ForeColor = !Art.IsValidStatic(index) ? Color.Red : Color.Black;
            }
            else
            {
                textBoxAddFrame.ForeColor = Color.Red;
            }
        }
        #endregion

        #region [ OnCheckChange ]
        private void OnCheckChange(object sender, EventArgs e)
        {
            OnTextChanged(this, EventArgs.Empty);
        }
        #endregion

        #region [ OnClickAdd ]
        private void OnClickAdd(object sender, EventArgs e)
        {
            if (_selAnimdataEntry == null)
            {
                return;
            }

            bool canDone = Utils.ConvertStringToInt(textBoxAddFrame.Text, out int index);
            if (checkBoxRelative.Checked)
            {
                index += CurrAnim;
            }

            if (index > Art.GetMaxItemId() || index < 0)
            {
                canDone = false;
            }

            if (!canDone || !Art.IsValidStatic(index))
            {
                return;
            }

            _selAnimdataEntry.FrameData[_selAnimdataEntry.FrameCount] = (sbyte)(index - CurrAnim);
            _selAnimdataEntry.FrameCount++;

            TreeNode node = new TreeNode
            {
                Text = $"0x{index:X4} {TileData.ItemTable[index].Name}"
            };
            treeViewFrames.Nodes.Add(node);

            TreeNode subNode = new TreeNode
            {
                Tag = _selAnimdataEntry.FrameCount - 1,
                Text = $"0x{index:X4} {TileData.ItemTable[index].Name}"
            };

            if (treeView1.SelectedNode.Parent == null)
            {
                treeView1.SelectedNode.Nodes.Add(subNode);
            }
            else
            {
                treeView1.SelectedNode.Parent.Nodes.Add(subNode);
            } // Add 1 Hexadress = textBoxAddFrame
            if (Utils.ConvertStringToInt(textBoxAddFrame.Text, out int newIndex))
            {
                newIndex++;
                textBoxAddFrame.Text = $"0x{newIndex:X4}";
            }

            Options.ChangedUltimaClass["Animdata"] = true;
        }
        #endregion

        #region  [ OnClickRemove ]
        private void OnClickRemove(object sender, EventArgs e)
        {
            if (_selAnimdataEntry == null || treeViewFrames.SelectedNode == null)
            {
                return;
            }

            int index = treeViewFrames.SelectedNode.Index;
            int i;
            for (i = index; i < _selAnimdataEntry.FrameCount - 1; ++i)
            {
                _selAnimdataEntry.FrameData[i] = _selAnimdataEntry.FrameData[i + 1];
            }

            for (; i < _selAnimdataEntry.FrameData.Length; ++i)
            {
                _selAnimdataEntry.FrameData[i] = 0;
            }

            _selAnimdataEntry.FrameCount--;
            treeView1.BeginUpdate();
            treeViewFrames.BeginUpdate();
            treeViewFrames.Nodes.Clear();
            TreeNode node = treeView1.SelectedNode.Parent ?? treeView1.SelectedNode;
            node.Nodes.Clear();
            for (i = 0; i < _selAnimdataEntry.FrameCount; ++i)
            {
                int frame = CurrAnim + _selAnimdataEntry.FrameData[i];
                if (Art.IsValidStatic(frame))
                {
                    TreeNode subNode = new TreeNode
                    {
                        Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}"
                    };
                    node.Nodes.Add(subNode);
                    treeViewFrames.Nodes.Add((TreeNode)subNode.Clone());
                }
                else
                {
                    break;
                }
            }
            treeViewFrames.EndUpdate();
            treeView1.EndUpdate();
            Options.ChangedUltimaClass["Animdata"] = true;
        }
        #endregion

        #region [ OnClickSave ]
        private void OnClickSave(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Animdata.Save(Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Saved to {Options.OutputPath}",
                "Save",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Animdata"] = false;
        }
        #endregion

        #region [ OnClickRemoveAnim ]
        private void OnClickRemoveAnim(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }

            Animdata.AnimData.Remove(CurrAnim);
            Options.ChangedUltimaClass["Animdata"] = true;
            treeView1.SelectedNode.Remove();
        }
        #endregion

        #region [ OnClickNode ]
        private void OnClickNode(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = e.Node;
            }
        }
        #endregion

        #region [ OnTextChangeAdd ]
        private void OnTextChangeAdd(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(AddTextBox.Text, out int index, 0, Art.GetMaxItemId()))
            {
                AddTextBox.ForeColor = Animdata.GetAnimData(index) != null ? Color.Red : Color.Black;
            }
            else
            {
                AddTextBox.ForeColor = Color.Red;
            }
        }
        #endregion

        #region [ OnKeyDownAdd ]
        private void OnKeyDownAdd(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(AddTextBox.Text, out int index, 0, Art.GetMaxItemId()))
            {
                return;
            }

            if (Animdata.GetAnimData(index) != null)
            {
                return;
            }

            Animdata.AnimData[index] = new Animdata.AnimdataEntry(new sbyte[64], 0, 1, 0, 0);
            TreeNode node = new TreeNode
            {
                Tag = index,
                Text = $"0x{index:X4} {TileData.ItemTable[index].Name}"
            };

            if ((TileData.ItemTable[index].Flags & TileFlag.Animation) == 0)
            {
                node.ForeColor = Color.Blue;
            }
            treeView1.Nodes.Add(node);

            TreeNode subNode = new TreeNode
            {
                Text = $"0x{index:X4} {TileData.ItemTable[index].Name}"
            };
            node.Nodes.Add(subNode);
            node.EnsureVisible();
            treeView1.SelectedNode = node;

            // Set the value of textBoxAddFrame.Text to the hexadecimal address.
            textBoxAddFrame.Text = $"0x{index:X4}";

            Options.ChangedUltimaClass["Animdata"] = true;
        }
        #endregion

        #region [ StopTimer ]
        private void StopTimer()
        {
            if (_mTimer.Enabled)
            {
                _mTimer.Stop();
            }

            _mTimer.Dispose();
            _mTimer = null;
            _timerFrame = 0;
        }
        #endregion

        #region ConextMenuStriip Opering
        private void ContextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            var enabled = treeView1.SelectedNode.Parent == null;

            removeToolStripMenuItem.Enabled = enabled;
            addToolStripMenuItem.Enabled = enabled;

            // Add this code to check the clipboard contents and paste them into AddTextBox
            if (Clipboard.ContainsText())
            {
                string clipboardText = Clipboard.GetText();
                // Check whether the text is a valid hex address
                if (System.Text.RegularExpressions.Regex.IsMatch(clipboardText, @"\A\b0x[0-9a-fA-F]+\b\Z"))
                {
                    AddTextBox.Text = clipboardText;
                }
            }
        }
        #endregion

        #region [ ChangeHue ]
        public void ChangeHue(int select)
        {
            select += 1;
            var newHue = select & ~0x8000;
            var newHueOnlyGray = (select & 0x8000) != 0;

            if (_customHue != newHue || _hueOnlyGray != newHueOnlyGray)
            {
                _customHue = newHue;
                _hueOnlyGray = newHueOnlyGray;
                toolStripStatusHue.Text = $"Hue: {_customHue}";
                if (_customHue != 0)
                {
                    GenerateFramedHues();
                }
                else
                {
                    // In a lock, since the Timer may read from this list.
                    lock (_huedFrames)
                    {
                        foreach (var huedFrame in _huedFrames)
                        {
                            huedFrame?.Dispose();
                        }
                        _huedFrames.Clear();
                    }
                }

                if (_selAnimdataEntry != null && _mTimer == null)
                {
                    if (_customHue > 0)
                    {
                        var frame = Math.Max(0, _curFrame);
                        AminDataMainPictureBox.Image = _huedFrames[frame];

                    }
                    else
                    {
                        var graphic = _currAnim + (_curFrame == -1 ? 0 : _selAnimdataEntry.FrameData[_curFrame]);
                        AminDataMainPictureBox.Image = Art.GetStatic(graphic);
                    }
                }
            }
        }
        #endregion

        #region [ OnClick_Hue ]
        private void OnClick_Hue(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = _customHue == 0
                ? new HuePopUpForm(ChangeHue, 1)
                : new HuePopUpForm(ChangeHue, _customHue - 1);

            _showForm.TopMost = true;
            _showForm.Show();
        }
        #endregion

        #region [ OnClick_ExportAsGif ]
        private void OnClick_ExportAsGif(object sender, EventArgs e)
        {
            if (_selAnimdataEntry != null)
            {
                var outputFile = Path.Combine(Options.OutputPath, $"AnimData 0x{_currAnim:X}.gif");
                var delay = (100 * _selAnimdataEntry.FrameInterval) + 1;

                {
                    using var gif = AnimatedGif.AnimatedGif.Create(outputFile, delay);
                    if (_customHue > 0)
                    {
                        // Not in a lock, since event runs on main thread, which is the only place it can be written.
                        foreach (var huedFrame in _huedFrames)
                        {
                            if (huedFrame != null)
                            {
                                gif.AddFrame(huedFrame, delay: -1, quality: GifQuality.Bit8);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _selAnimdataEntry.FrameCount; ++i)
                        {
                            int graphic = _currAnim + _selAnimdataEntry.FrameData[i];
                            gif.AddFrame(Art.GetStatic(graphic), delay: -1, quality: GifQuality.Bit8);
                        }
                    }
                }

                MessageBox.Show($"Saved to {outputFile}");
            }
        }
        #endregion

        #region [ ToolStripTextBoxFindAdress_TextChanged ]
        private void ToolStripTextBoxFindAdress_TextChanged(object sender, EventArgs e)
        {
            string searchText = toolStripTextBoxFindAdress.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
                return;

            TreeNode foundNode = null;

            // 1. Try: Search as hex address
            if (searchText.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(searchText.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out int hexId))
                {
                    foreach (TreeNode node in treeView1.Nodes)
                    {
                        if ((int)node.Tag == hexId)
                        {
                            foundNode = node;
                            break;
                        }
                    }
                }
            }

            // 2. Try: Search by name
            if (foundNode == null)
            {
                foreach (TreeNode node in treeView1.Nodes)
                {
                    if (node.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        foundNode = node;
                        break;
                    }
                }
            }

            // 3. Update selection in TreeView
            if (foundNode != null)
            {
                treeView1.SelectedNode = foundNode;
                treeView1.SelectedNode.EnsureVisible();
            }
        }
        #endregion

    }
    #endregion

    #region [ class AnimdataSorter ]
    public class AnimdataSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            if (tx.Parent != null)
            {
                return 0;
            }

            int ix = (int)tx.Tag;
            int iy = (int)ty.Tag;
            if (ix == iy)
            {
                return 0;
            }
            else if (ix < iy)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }
    #endregion
}

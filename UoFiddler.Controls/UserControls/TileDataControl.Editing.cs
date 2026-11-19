/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * TileDataControl – editing of items and land tiles:
 *
 *  - multi selection in both lists: Ctrl = add/remove, Shift = range
 *  - the edit fields remember which values / flags were changed: with several
 *    selected tiles "Save changes" (and "save directly") applies only these,
 *    the other values of the tiles stay as they are
 *  - undo / redo (Ctrl+Z / Ctrl+Y) for every change, also batch edit,
 *    CSV import, presets, copy/insert settings
 *  - batch edit dialog, list of changed tiles, revert to the file state
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;

namespace UoFiddler.Controls.UserControls
{
    public partial class TileDataControl
    {
        private static readonly Color _selectionColor = Color.LightBlue;

        /// <summary>more changed tiles (CSV import, big batch) do not send an event per tile to the other tabs</summary>
        private const int MaxChangeEvents = 500;

        private readonly List<TreeNode> _selectedLandNodes = new List<TreeNode>();
        private TreeNode _itemAnchor;
        private TreeNode _landAnchor;

        // values / flags the user changed in the edit fields since the tile was shown
        private readonly HashSet<TileField> _dirtyItemFields = new HashSet<TileField>();
        private readonly Dictionary<TileFlag, bool> _itemFlagEdits = new Dictionary<TileFlag, bool>();
        private readonly HashSet<TileField> _dirtyLandFields = new HashSet<TileField>();
        private readonly Dictionary<TileFlag, bool> _landFlagEdits = new Dictionary<TileFlag, bool>();

        private TileDataChangesForm _changesForm;
        private bool _editingInitialized;

        private void InitEditing()
        {
            if (_editingInitialized)
            {
                return;
            }

            _editingInitialized = true;
            TileDataHistory.Changed += OnHistoryChanged;
            Disposed += OnDisposedEditing;
            OnHistoryChanged(this, EventArgs.Empty);
        }

        private void OnDisposedEditing(object sender, EventArgs e) => TileDataHistory.Changed -= OnHistoryChanged;

        private bool SaveDirectly => saveDirectlyOnChangesToolStripMenuItem.Checked;

        #region Node helpers

        private static string ItemNodeText(int index) => string.Format("0x{0:X4} ({0}) {1}", index, TileData.ItemTable[index].Name);

        private static string LandNodeText(int index) => string.Format("0x{0:X4} ({0}) {1}", index, TileData.LandTable[index].Name);

        /// <summary>all item nodes (the list has group nodes "AOS - ML", "Stygian Abyss" …)</summary>
        private IEnumerable<TreeNode> ItemNodes()
        {
            foreach (TreeNode node in treeViewItem.Nodes)
            {
                if (node.Tag is int)
                {
                    yield return node;
                }

                foreach (TreeNode child in node.Nodes)
                {
                    if (child.Tag is int)
                    {
                        yield return child;
                    }
                }
            }
        }

        private IEnumerable<TreeNode> LandNodes() => treeViewLand.Nodes.Cast<TreeNode>().Where(n => n.Tag is int);

        private Dictionary<int, TreeNode> ItemNodeMap()
        {
            var map = new Dictionary<int, TreeNode>();
            foreach (TreeNode n in ItemNodes())
            {
                map[(int)n.Tag] = n;
            }

            return map;
        }

        private Dictionary<int, TreeNode> LandNodeMap()
        {
            var map = new Dictionary<int, TreeNode>();
            foreach (TreeNode n in LandNodes())
            {
                map[(int)n.Tag] = n;
            }

            return map;
        }

        /// <summary>items the edit fields are applied to (selection, otherwise the shown item)</summary>
        private List<int> ItemTargets()
        {
            List<int> list = _selectedNodes.Where(n => n?.Tag is int).Select(n => (int)n.Tag).Distinct().ToList();
            if (list.Count == 0 && treeViewItem.SelectedNode?.Tag is int index)
            {
                list.Add(index);
            }

            return list;
        }

        private List<int> LandTargets()
        {
            List<int> list = _selectedLandNodes.Where(n => n?.Tag is int).Select(n => (int)n.Tag).Distinct().ToList();
            if (list.Count == 0 && treeViewLand.SelectedNode?.Tag is int index)
            {
                list.Add(index);
            }

            return list;
        }

        /// <summary>marks items as changed: node text and colour, change flag, event for other tabs</summary>
        private void MarkItemsChanged(IEnumerable<int> indices)
        {
            Dictionary<int, TreeNode> map = null;
            int fired = 0;
            foreach (int index in indices)
            {
                map = map ?? ItemNodeMap();
                if (map.TryGetValue(index, out TreeNode node))
                {
                    string text = ItemNodeText(index);
                    if (node.Text != text)
                    {
                        node.Text = text;
                    }

                    // red = differs from the file, back to normal when it is like the file again (undo, revert)
                    node.ForeColor = TileDataHistory.TryGetOriginalItem(index, out ItemData original) && TileDataHistory.SameItem(original, TileData.ItemTable[index])
                        ? treeViewItem.ForeColor
                        : Color.Red;
                }

                Options.ChangedUltimaClass["TileData"] = true;
                if (++fired <= MaxChangeEvents)
                {
                    ControlEvents.FireTileDataChangeEvent(this, ControlEvents.TileDataItemEventId(index));
                }
            }
        }

        private void MarkLandsChanged(IEnumerable<int> indices)
        {
            Dictionary<int, TreeNode> map = null;
            int fired = 0;
            foreach (int index in indices)
            {
                map = map ?? LandNodeMap();
                if (map.TryGetValue(index, out TreeNode node))
                {
                    string text = LandNodeText(index);
                    if (node.Text != text)
                    {
                        node.Text = text;
                    }

                    node.ForeColor = TileDataHistory.TryGetOriginalLand(index, out LandData original) && TileDataHistory.SameLand(original, TileData.LandTable[index])
                        ? treeViewLand.ForeColor
                        : Color.Red;
                }

                Options.ChangedUltimaClass["TileData"] = true;
                if (++fired <= MaxChangeEvents)
                {
                    ControlEvents.FireTileDataChangeEvent(this, ControlEvents.TileDataLandEventId(index));
                }
            }
        }

        /// <summary>shows the values of the shown item / land tile again (after undo, batch …)</summary>
        private void RefreshShownTiles()
        {
            if (treeViewItem.SelectedNode?.Tag is int)
            {
                ApplySettingsToNode(treeViewItem.SelectedNode);
                ResetItemEdits();
            }

            if (treeViewLand.SelectedNode?.Tag is int)
            {
                ShowLand(treeViewLand.SelectedNode);
            }
        }

        private void ShowLand(TreeNode node)
        {
            // AfterSelectTreeViewLand would change the selection, this only shows the values
            _suppressLandSelection = true;
            try
            {
                AfterSelectTreeViewLand(this, new TreeViewEventArgs(node));
            }
            finally
            {
                _suppressLandSelection = false;
            }
        }

        private bool _suppressLandSelection;

        #endregion

        #region Multi selection

        /// <summary>Ctrl = add/remove, Shift = range from the last clicked tile, otherwise single selection</summary>
        private void HandleItemSelection(TreeNode node)
        {
            bool ctrl = (ModifierKeys & Keys.Control) != 0;
            bool shift = (ModifierKeys & Keys.Shift) != 0;

            if (shift && _itemAnchor != null && _itemAnchor.Parent == node.Parent && _itemAnchor.TreeView == treeViewItem)
            {
                TreeNodeCollection siblings = node.Parent?.Nodes ?? treeViewItem.Nodes;
                if (!ctrl)
                {
                    ClearPreviousSelection();
                }

                int from = Math.Min(_itemAnchor.Index, node.Index), to = Math.Max(_itemAnchor.Index, node.Index);
                for (int i = from; i <= to; i++)
                {
                    TreeNode n = siblings[i];
                    if (!_selectedNodes.Contains(n))
                    {
                        _selectedNodes.Add(n);
                        n.BackColor = _selectionColor;
                    }
                }
            }
            else if (ctrl)
            {
                if (_selectedNodes.Contains(node))
                {
                    _selectedNodes.Remove(node);
                    node.BackColor = treeViewItem.BackColor;
                }
                else
                {
                    _selectedNodes.Add(node);
                    node.BackColor = _selectionColor;
                }

                _itemAnchor = node;
            }
            else
            {
                ClearPreviousSelection();
                _selectedNodes.Add(node);
                node.BackColor = _selectionColor;
                _itemAnchor = node;
            }

            UpdateSelectionStatus();
        }

        private void HandleLandSelection(TreeNode node)
        {
            bool ctrl = (ModifierKeys & Keys.Control) != 0;
            bool shift = (ModifierKeys & Keys.Shift) != 0;

            if (shift && _landAnchor != null && _landAnchor.TreeView == treeViewLand)
            {
                if (!ctrl)
                {
                    ClearLandSelection();
                }

                int from = Math.Min(_landAnchor.Index, node.Index), to = Math.Max(_landAnchor.Index, node.Index);
                for (int i = from; i <= to; i++)
                {
                    TreeNode n = treeViewLand.Nodes[i];
                    if (!_selectedLandNodes.Contains(n))
                    {
                        _selectedLandNodes.Add(n);
                        n.BackColor = _selectionColor;
                    }
                }
            }
            else if (ctrl)
            {
                if (_selectedLandNodes.Contains(node))
                {
                    _selectedLandNodes.Remove(node);
                    node.BackColor = treeViewLand.BackColor;
                }
                else
                {
                    _selectedLandNodes.Add(node);
                    node.BackColor = _selectionColor;
                }

                _landAnchor = node;
            }
            else
            {
                ClearLandSelection();
                _selectedLandNodes.Add(node);
                node.BackColor = _selectionColor;
                _landAnchor = node;
            }

            UpdateSelectionStatus();
        }

        private void ClearLandSelection()
        {
            foreach (TreeNode n in _selectedLandNodes)
            {
                n.BackColor = treeViewLand.BackColor;
            }

            _selectedLandNodes.Clear();
        }

        private void SelectIds(bool land, IEnumerable<int> ids, bool add)
        {
            var wanted = new HashSet<int>(ids);
            if (land)
            {
                if (!add)
                {
                    ClearLandSelection();
                }

                foreach (TreeNode n in LandNodes().Where(n => wanted.Contains((int)n.Tag) && !_selectedLandNodes.Contains(n)))
                {
                    _selectedLandNodes.Add(n);
                    n.BackColor = _selectionColor;
                }
            }
            else
            {
                if (!add)
                {
                    ClearPreviousSelection();
                }

                foreach (TreeNode n in ItemNodes().Where(n => wanted.Contains((int)n.Tag) && !_selectedNodes.Contains(n)))
                {
                    _selectedNodes.Add(n);
                    n.BackColor = _selectionColor;
                }
            }

            UpdateSelectionStatus();
        }

        /// <summary>shows the number of selected tiles in the tooltip of the save button</summary>
        private void UpdateSelectionStatus()
        {
            int items = ItemTargets().Count, lands = LandTargets().Count;
            string text = tabcontrol.SelectedIndex == 1
                ? (lands > 1 ? $"{lands} land tiles selected – only the changed values/flags are applied to all" : "Save changes")
                : (items > 1 ? $"{items} items selected – only the changed values/flags are applied to all" : "Save changes");
            toolStripButton1.ToolTipText = text;
        }

        private bool LandMenu(object sender) =>
            sender == _miLandBatch || sender == _miLandSelectRange || sender == _miLandClearSelection || sender == _miLandRevert ||
            (sender is ToolStripItem item && item.Owner == LandTilesContextMenuStrip) ||
            (!(sender is ToolStripItem i2 && i2.Owner == ItemsContextMenuStrip) && tabcontrol.SelectedIndex == 1);

        private void OnClickSelectRange(object sender, EventArgs e)
        {
            bool land = LandMenu(sender);
            List<int> current = land ? LandTargets() : ItemTargets();
            int max = land ? TileData.LandTable?.Length ?? 0 : TileData.ItemTable?.Length ?? 0;
            using (var form = new TileDataRangeForm(max, current.Count > 1 ? TileDataBatch.FormatRanges(current) : string.Empty))
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                SelectIds(land, form.Ids, form.AddToSelection);
            }
        }

        private void OnClickClearSelection(object sender, EventArgs e)
        {
            if (LandMenu(sender))
            {
                ClearLandSelection();
                if (treeViewLand.SelectedNode != null)
                {
                    _selectedLandNodes.Add(treeViewLand.SelectedNode);
                    treeViewLand.SelectedNode.BackColor = _selectionColor;
                }
            }
            else
            {
                ClearPreviousSelection();
                if (treeViewItem.SelectedNode?.Tag is int)
                {
                    _selectedNodes.Add(treeViewItem.SelectedNode);
                    treeViewItem.SelectedNode.BackColor = _selectionColor;
                }
            }

            UpdateSelectionStatus();
        }

        #endregion

        #region Edit fields (remember what was changed)

        private void ResetItemEdits()
        {
            _dirtyItemFields.Clear();
            _itemFlagEdits.Clear();
        }

        private void ResetLandEdits()
        {
            _dirtyLandFields.Clear();
            _landFlagEdits.Clear();
        }

        private TextBox ItemTextBox(TileField field)
        {
            switch (field)
            {
                case TileField.Name: return textBoxName;
                case TileField.Weight: return textBoxWeight;
                case TileField.Quality: return textBoxQuality;
                case TileField.Animation: return textBoxAnim;
                case TileField.Quantity: return textBoxQuantity;
                case TileField.Hue: return textBoxHue;
                case TileField.StackingOffset: return textBoxStackOff;
                case TileField.Value: return textBoxValue;
                case TileField.Height: return textBoxHeigth;
                case TileField.MiscData: return textBoxUnk1;
                case TileField.Unk2: return textBoxUnk2;
                case TileField.Unk3: return textBoxUnk3;
                default: return null;
            }
        }

        /// <summary>writes the value of an edit field into the item; false = the text is no valid value</summary>
        private bool TryApplyItemField(TileField field, ref ItemData item)
        {
            string text = ItemTextBox(field)?.Text ?? string.Empty;
            if (field == TileField.Name)
            {
                item.Name = text.Length > 20 ? text.Substring(0, 20) : text;
                return true;
            }

            switch (field)
            {
                case TileField.Animation:
                case TileField.MiscData:
                    {
                        if (!short.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out short v))
                        {
                            return false;
                        }

                        if (field == TileField.Animation)
                        {
                            item.Animation = v;
                        }
                        else
                        {
                            item.MiscData = v;
                        }

                        return true;
                    }

                default:
                    {
                        if (!byte.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte b))
                        {
                            return false;
                        }

                        switch (field)
                        {
                            case TileField.Weight: item.Weight = b; break;
                            case TileField.Quality: item.Quality = b; break;
                            case TileField.Quantity: item.Quantity = b; break;
                            case TileField.Hue: item.Hue = b; break;
                            case TileField.StackingOffset: item.StackingOffset = b; break;
                            case TileField.Value: item.Value = b; break;
                            case TileField.Height: item.Height = b; break;
                            case TileField.Unk2: item.Unk2 = b; break;
                            case TileField.Unk3: item.Unk3 = b; break;
                        }

                        return true;
                    }
            }
        }

        private bool TryApplyLandField(TileField field, ref LandData land)
        {
            if (field == TileField.Name)
            {
                string name = textBoxNameLand.Text;
                land.Name = name.Length > 20 ? name.Substring(0, 20) : name;
                return true;
            }

            if (!ushort.TryParse(textBoxTexID.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort tex))
            {
                return false;
            }

            land.TextureId = tex;
            return true;
        }

        /// <summary>a text box of the item tab was changed by the user</summary>
        private void OnItemFieldEdited(TileField field)
        {
            if (_changingIndex)
            {
                return;
            }

            _dirtyItemFields.Add(field);
            if (!SaveDirectly)
            {
                return;
            }

            List<int> targets = ItemTargets();
            if (targets.Count == 0 || (field == TileField.Name && textBoxName.Text.Length == 0))
            {
                return;
            }

            ItemData probe = TileData.ItemTable[targets[0]];
            if (!TryApplyItemField(field, ref probe))
            {
                return;
            }

            TileDataHistory.Record($"{field} of {Describe(targets, false)}", targets, null, $"item|{field}|{TileDataBatch.FormatRanges(targets)}");
            foreach (int index in targets)
            {
                ItemData item = TileData.ItemTable[index];
                TryApplyItemField(field, ref item);
                TileData.ItemTable[index] = item;
            }

            MarkItemsChanged(targets);
        }

        private void OnLandFieldEdited(TileField field)
        {
            if (_changingIndex)
            {
                return;
            }

            _dirtyLandFields.Add(field);
            if (!SaveDirectly)
            {
                return;
            }

            List<int> targets = LandTargets();
            if (targets.Count == 0 || (field == TileField.Name && textBoxNameLand.Text.Length == 0))
            {
                return;
            }

            LandData probe = TileData.LandTable[targets[0]];
            if (!TryApplyLandField(field, ref probe))
            {
                return;
            }

            TileDataHistory.Record($"{field} of {Describe(targets, true)}", null, targets, $"land|{field}|{TileDataBatch.FormatRanges(targets)}");
            foreach (int index in targets)
            {
                LandData land = TileData.LandTable[index];
                TryApplyLandField(field, ref land);
                TileData.LandTable[index] = land;
            }

            MarkLandsChanged(targets);
        }

        private void OnItemFlagEdited(ItemCheckEventArgs e)
        {
            if (_changingIndex || e.CurrentValue == e.NewValue)
            {
                return;
            }

            TileFlag flag = FlagOfListIndex(e.Index);
            if (flag == TileFlag.None)
            {
                return;
            }

            bool set = e.NewValue == CheckState.Checked;
            _itemFlagEdits[flag] = set;
            if (!SaveDirectly)
            {
                return;
            }

            List<int> targets = ItemTargets().Where(i => ((TileData.ItemTable[i].Flags & flag) != 0) != set).ToList();
            if (targets.Count == 0)
            {
                return;
            }

            TileDataHistory.Record($"{(set ? "+" : "-")}{flag} on {Describe(targets, false)}", targets, null);
            foreach (int index in targets)
            {
                ItemData item = TileData.ItemTable[index];
                item.Flags = set ? item.Flags | flag : item.Flags & ~flag;
                TileData.ItemTable[index] = item;
            }

            MarkItemsChanged(targets);
        }

        private void OnLandFlagEdited(ItemCheckEventArgs e)
        {
            if (_changingIndex || e.CurrentValue == e.NewValue)
            {
                return;
            }

            TileFlag flag = FlagOfListIndex(e.Index);
            if (flag == TileFlag.None)
            {
                return;
            }

            bool set = e.NewValue == CheckState.Checked;
            _landFlagEdits[flag] = set;
            if (!SaveDirectly)
            {
                return;
            }

            List<int> targets = LandTargets().Where(i => ((TileData.LandTable[i].Flags & flag) != 0) != set).ToList();
            if (targets.Count == 0)
            {
                return;
            }

            TileDataHistory.Record($"{(set ? "+" : "-")}{flag} on {Describe(targets, true)}", null, targets);
            foreach (int index in targets)
            {
                LandData land = TileData.LandTable[index];
                land.Flags = set ? land.Flags | flag : land.Flags & ~flag;
                TileData.LandTable[index] = land;
            }

            MarkLandsChanged(targets);
        }

        private static string Describe(List<int> indices, bool land) =>
            indices.Count == 1 ? $"{(land ? "land" : "item")} 0x{indices[0]:X4}" : $"{indices.Count} {(land ? "land tiles" : "items")}";

        #endregion

        #region Save changes

        /// <summary>
        /// "Save changes": one tile = all values of the edit fields; several tiles = only the
        /// changed values and flags (applyAll = all values, used by "push marked").
        /// </summary>
        private void SaveChanges(bool applyAll)
        {
            if (tabcontrol.SelectedIndex == 0)
            {
                SaveItemChanges(applyAll);
            }
            else if (tabcontrol.SelectedIndex == 1)
            {
                SaveLandChanges(applyAll);
            }
        }

        private void SaveItemChanges(bool applyAll)
        {
            List<int> targets = ItemTargets();
            if (targets.Count == 0)
            {
                MessageBox.Show("No item selected.");
                return;
            }

            bool all = applyAll || targets.Count == 1;
            if (!all && _dirtyItemFields.Count == 0 && _itemFlagEdits.Count == 0)
            {
                MessageBox.Show(
                    $"{targets.Count} items are selected.\n\nOnly the values and flags you change in the fields are applied to all of them – " +
                    "nothing was changed yet.\n\nTip: \"Batch edit…\" offers more possibilities (add/subtract, name prefix …).",
                    "Save changes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            IEnumerable<TileField> fields = all ? TileDataBatch.ItemFields : _dirtyItemFields;
            var changed = new List<int>();
            var newValues = new Dictionary<int, ItemData>();
            foreach (int index in targets)
            {
                ItemData item = TileData.ItemTable[index];
                foreach (TileField f in fields)
                {
                    ItemData copy = item;
                    if (TryApplyItemField(f, ref copy))
                    {
                        item = copy;
                    }
                }

                if (all)
                {
                    item.Flags = FlagsFromList(checkedListBox1, item.Flags);
                }
                else
                {
                    foreach (KeyValuePair<TileFlag, bool> flag in _itemFlagEdits)
                    {
                        item.Flags = flag.Value ? item.Flags | flag.Key : item.Flags & ~flag.Key;
                    }
                }

                if (!TileDataHistory.SameItem(item, TileData.ItemTable[index]))
                {
                    changed.Add(index);
                    newValues[index] = item;
                }
            }

            if (changed.Count > 0)
            {
                TileDataHistory.Record($"Save changes ({Describe(changed, false)})", changed, null);
                foreach (int index in changed)
                {
                    TileData.ItemTable[index] = newValues[index];
                }

                MarkItemsChanged(changed);
            }

            ResetItemEdits();
            NotifySaved(changed.Count, targets.Count);
        }

        private void SaveLandChanges(bool applyAll)
        {
            List<int> targets = LandTargets();
            if (targets.Count == 0)
            {
                return;
            }

            bool all = applyAll || targets.Count == 1;
            if (!all && _dirtyLandFields.Count == 0 && _landFlagEdits.Count == 0)
            {
                MessageBox.Show(
                    $"{targets.Count} land tiles are selected.\n\nOnly the values and flags you change in the fields are applied to all of them – nothing was changed yet.",
                    "Save changes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            IEnumerable<TileField> fields = all ? TileDataBatch.LandFields : _dirtyLandFields;
            var changed = new List<int>();
            var newValues = new Dictionary<int, LandData>();
            foreach (int index in targets)
            {
                LandData land = TileData.LandTable[index];
                foreach (TileField f in fields)
                {
                    LandData copy = land;
                    if (TryApplyLandField(f, ref copy))
                    {
                        land = copy;
                    }
                }

                if (all)
                {
                    land.Flags = FlagsFromList(checkedListBox2, land.Flags);
                }
                else
                {
                    foreach (KeyValuePair<TileFlag, bool> flag in _landFlagEdits)
                    {
                        land.Flags = flag.Value ? land.Flags | flag.Key : land.Flags & ~flag.Key;
                    }
                }

                if (!TileDataHistory.SameLand(land, TileData.LandTable[index]))
                {
                    changed.Add(index);
                    newValues[index] = land;
                }
            }

            if (changed.Count > 0)
            {
                TileDataHistory.Record($"Save changes ({Describe(changed, true)})", null, changed);
                foreach (int index in changed)
                {
                    TileData.LandTable[index] = newValues[index];
                }

                MarkLandsChanged(changed);
            }

            ResetLandEdits();
            NotifySaved(changed.Count, targets.Count);
        }

        /// <summary>the existing note "saved to memory" (message box or sound)</summary>
        private void NotifySaved(int changed, int targets)
        {
            if (!_toolStripButton7IsActive || !memorySaveWarningToolStripMenuItem.Checked)
            {
                return;
            }

            if (_playCustomSound)
            {
                using (var player = new SoundPlayer { SoundLocation = "sound.wav" })
                {
                    try
                    {
                        player.Play();
                    }
                    catch (Exception)
                    {
                        // no sound file
                    }
                }
            }
            else
            {
                MessageBox.Show($"{changed} of {targets} tile(s) changed in memory. Click 'Save Tiledata' to write to file.", "Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        #endregion

        #region Undo / redo

        private void OnClickTdUndo(object sender, EventArgs e) => ApplyHistoryStep(TileDataHistory.Undo());

        private void OnClickTdRedo(object sender, EventArgs e) => ApplyHistoryStep(TileDataHistory.Redo());

        private void ApplyHistoryStep(TileDataHistoryStep step)
        {
            if (step == null)
            {
                return;
            }

            MarkItemsChanged(step.ItemIndices.ToList());
            MarkLandsChanged(step.LandIndices.ToList());
            RefreshShownTiles();
        }

        private void OnHistoryChanged(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            _btnTdUndo.Enabled = TileDataHistory.CanUndo;
            _btnTdRedo.Enabled = TileDataHistory.CanRedo;
            _btnTdUndo.ToolTipText = TileDataHistory.CanUndo ? $"Undo: {TileDataHistory.UndoText} (Ctrl+Z)" : "Nothing to undo";
            _btnTdRedo.ToolTipText = TileDataHistory.CanRedo ? $"Redo: {TileDataHistory.RedoText} (Ctrl+Y)" : "Nothing to redo";
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool inText = ActiveControlDeep() is TextBoxBase;
            if (!inText && tabcontrol.SelectedIndex != 2)
            {
                if (keyData == (Keys.Control | Keys.Z))
                {
                    OnClickTdUndo(this, EventArgs.Empty);
                    return true;
                }

                // Ctrl+Y on the item list is "push marked" while the marker is active
                if ((keyData == (Keys.Control | Keys.Y) && !(_toolStripButton7IsActive && treeViewItem.Focused)) ||
                    keyData == (Keys.Control | Keys.Shift | Keys.Z))
                {
                    OnClickTdRedo(this, EventArgs.Empty);
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private Control ActiveControlDeep()
        {
            Control c = ActiveControl;
            while (c is ContainerControl cc && cc.ActiveControl != null)
            {
                c = cc.ActiveControl;
            }

            return c;
        }

        #endregion

        #region Batch edit / changes / revert / help

        private void OnClickTdBatch(object sender, EventArgs e)
        {
            if (!IsLoaded || TileData.ItemTable == null)
            {
                return;
            }

            bool land = sender == _miLandBatch || (sender != _miItemsBatch && tabcontrol.SelectedIndex == 1);
            List<int> selected = land ? LandTargets() : ItemTargets();
            List<int> visible = (land ? LandNodes() : ItemNodes()).Select(n => (int)n.Tag).ToList();
            using (var form = new TileDataBatchForm(land, selected, visible))
            {
                if (form.ShowDialog(this) != DialogResult.OK || form.ChangedIndices.Count == 0)
                {
                    return;
                }

                if (land)
                {
                    MarkLandsChanged(form.ChangedIndices);
                }
                else
                {
                    MarkItemsChanged(form.ChangedIndices);
                }

                RefreshShownTiles();
            }
        }

        private void OnClickTdChanges(object sender, EventArgs e)
        {
            if (_changesForm == null || _changesForm.IsDisposed)
            {
                _changesForm = new TileDataChangesForm(ShowTile, OnTilesReverted);
                _changesForm.Show(FindForm());
            }
            else
            {
                _changesForm.FillList();
                _changesForm.Activate();
            }
        }

        private void ShowTile(int index, bool land)
        {
            tabcontrol.SelectedIndex = land ? 1 : 0;
            SearchGraphic(index, land);
        }

        private void OnTilesReverted(List<int> items, List<int> lands)
        {
            MarkItemsChanged(items);
            MarkLandsChanged(lands);
            RefreshShownTiles();
        }

        private void OnClickRevertSelected(object sender, EventArgs e)
        {
            bool land = LandMenu(sender);
            List<int> targets = land ? LandTargets() : ItemTargets();
            if (targets.Count == 0 || !TileDataHistory.HasBaseline)
            {
                return;
            }

            int count = TileDataHistory.Revert(land ? null : targets, land ? targets : null, $"Revert {Describe(targets, land)} to the file");
            if (count > 0)
            {
                OnTilesReverted(land ? new List<int>() : targets, land ? targets : new List<int>());
            }
        }

        private void OnClickTdHelp(object sender, EventArgs e) => TileDataHelpForm.ShowHelp(FindForm());

        #endregion
    }
}

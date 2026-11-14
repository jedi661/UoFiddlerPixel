/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * AnimationListControl – Erweiterungen:
 *  - Einträge mit Namen hinzufügen / umbenennen (Dialog mit Vorschau)
 *  - neue Animationen aus dem AnimationEditForm erscheinen sofort in der Liste
 *  - ServUO-Script und Einrichtung (Client- und Server-Dateien) für eine Animation
 *
 ***************************************************************************/

using System;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;

namespace UoFiddler.Controls.UserControls
{
    public partial class AnimationListControl
    {
        private bool _suppressStoreEvent;
        private AnimationListAddForm _addForm;
        private ServUOSetupForm _servUoForm;

        private void InitAnimationListExtensions()
        {
            var addItem = new ToolStripMenuItem("Add animation…", null, (s, e) => ShowAddForm(null))
            {
                ToolTipText = "Add an animation (body id) with name and type to the list"
            };
            var editItem = new ToolStripMenuItem("Rename / change type…", null, (s, e) => ShowAddForm(SelectedEntry()));
            var servUoItem = new ToolStripMenuItem("ServUO script & setup…", null, (s, e) => ShowServUoForm(SelectedEntry()))
            {
                ToolTipText = "Create a ServUO script for the selected animation and check which client/server files need entries"
            };
            var reloadItem = new ToolStripMenuItem("Reload list", null, (s, e) => ReloadList());

            contextMenuStrip2.Items.Insert(0, new ToolStripSeparator());
            contextMenuStrip2.Items.Insert(0, servUoItem);
            contextMenuStrip2.Items.Insert(0, editItem);
            contextMenuStrip2.Items.Insert(0, addItem);
            contextMenuStrip2.Opening += (s, e) =>
            {
                bool selected = SelectedEntry() != null;
                editItem.Enabled = selected;
                servUoItem.Enabled = selected;
            };

            int index = SettingsButton.DropDownItems.IndexOf(tryToFindNewGraphicsToolStripMenuItem);
            if (index < 0)
            {
                index = SettingsButton.DropDownItems.Count;
            }

            SettingsButton.DropDownItems.Insert(index, reloadItem);
            SettingsButton.DropDownItems.Insert(index, new ToolStripMenuItem("ServUO script & setup…", null, (s, e) => ShowServUoForm(SelectedEntry())));
            SettingsButton.DropDownItems.Insert(index, new ToolStripMenuItem("Add animation…", null, (s, e) => ShowAddForm(null)));

            AnimationListStore.Changed += OnStoreChanged;
            Disposed += (s, e) => AnimationListStore.Changed -= OnStoreChanged;
        }

        #region Nodes

        /// <summary>Name of an entry without the " (0x..)" that is only shown in the tree</summary>
        private static string EntryName(TreeNode node)
        {
            return AnimationListStore.CleanName(string.IsNullOrEmpty(node.Name) ? node.Text : node.Name);
        }

        private static bool TryReadEntry(XmlElement element, bool equipment, out AnimationListEntry entry)
        {
            entry = null;
            if (!int.TryParse(element.GetAttribute("body"), out int body))
            {
                return false;
            }

            int.TryParse(element.GetAttribute("type"), out int type);
            entry = new AnimationListEntry
            {
                Body = body,
                IsEquipment = equipment,
                // type 4 (equipment of the "find new entries" dialog) or invalid values crashed the list before
                Type = AnimationListStore.NormalizeType(type),
                Name = AnimationListStore.CleanName(element.GetAttribute("name"))
            };
            return true;
        }

        private TreeNode CreateEntryNode(AnimationListEntry entry)
        {
            int type = AnimationListStore.NormalizeType(entry.Type);
            var node = new TreeNode(entry.IsEquipment ? entry.Name : $"{entry.Name} (0x{entry.Body:X})")
            {
                Name = entry.Name,
                Tag = new[] { entry.Body, type },
                ToolTipText = Animations.GetFileName(entry.Body)
            };

            AddActionNodes(node, entry.Body, type);
            return node;
        }

        private void AddActionNodes(TreeNode node, int body, int type)
        {
            node.Nodes.Clear();
            for (int i = 0; i < GetActionNames[type].GetLength(0); ++i)
            {
                if (!Animations.IsActionDefined(body, i, 0))
                {
                    continue;
                }

                node.Nodes.Add(new TreeNode($"{i} {GetActionNames[type][i]}") { Tag = i });
            }
        }

        private TreeNode FindEntryNode(int body, bool equipment)
        {
            if (TreeViewMobs.Nodes.Count < 2)
            {
                return null;
            }

            return TreeViewMobs.Nodes[equipment ? 1 : 0].Nodes.Cast<TreeNode>()
                .FirstOrDefault(n => n.Tag is int[] tag && tag[0] == body);
        }

        /// <summary>Adds or updates the node of an entry (the file is not written here)</summary>
        private void AddEntryNode(AnimationListEntry entry, bool select)
        {
            if (TreeViewMobs.Nodes.Count < 2)
            {
                return;
            }

            TreeViewMobs.BeginUpdate();
            TreeNode node;
            try
            {
                TreeViewMobs.TreeViewNodeSorter = null;
                node = FindEntryNode(entry.Body, entry.IsEquipment);
                if (node == null)
                {
                    node = CreateEntryNode(entry);
                    TreeViewMobs.Nodes[entry.IsEquipment ? 1 : 0].Nodes.Add(node);
                }
                else
                {
                    int type = AnimationListStore.NormalizeType(entry.Type);
                    node.Name = entry.Name;
                    node.Text = entry.IsEquipment ? entry.Name : $"{entry.Name} (0x{entry.Body:X})";
                    node.Tag = new[] { entry.Body, type };
                    AddActionNodes(node, entry.Body, type);
                }

                TreeViewMobs.TreeViewNodeSorter = !_sortAlpha ? new GraphicSorter() : (System.Collections.IComparer)new AlphaSorter();
                TreeViewMobs.Sort();
            }
            finally
            {
                TreeViewMobs.EndUpdate();
            }

            LoadListView();
            if (select)
            {
                TreeViewMobs.SelectedNode = node;
                node.EnsureVisible();
            }
        }

        private AnimationListEntry SelectedEntry()
        {
            TreeNode node = TreeViewMobs.SelectedNode;
            if (node?.Parent == null)
            {
                return null;
            }

            if (node.Parent.Name != "Mobs" && node.Parent.Name != "Equipment")
            {
                node = node.Parent;
            }

            if (!(node.Tag is int[] tag))
            {
                return null;
            }

            return new AnimationListEntry
            {
                Body = tag[0],
                Type = tag[1],
                Name = EntryName(node),
                IsEquipment = node.Parent?.Name == "Equipment"
            };
        }

        private void ReloadList()
        {
            if (!LoadXml())
            {
                return;
            }

            if (TreeViewMobs.Nodes.Count > _displayType)
            {
                LoadListView();
            }
        }

        #endregion

        #region Store events (entries added by other windows, e.g. the AnimationEditForm)

        private void OnStoreChanged(object sender, AnimationListChangedEventArgs e)
        {
            if (_suppressStoreEvent || IsDisposed || !_loaded)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnStoreChanged(sender, e)));
                return;
            }

            if (e.Removed)
            {
                FindEntryNode(e.Entry.Body, e.Entry.IsEquipment)?.Remove();
                LoadListView();
                return;
            }

            AddEntryNode(e.Entry, false);
        }

        #endregion

        #region Dialogs

        private void ShowAddForm(AnimationListEntry entry)
        {
            if (_addForm != null && !_addForm.IsDisposed)
            {
                if (entry != null)
                {
                    _addForm.ShowEntry(entry);
                }

                _addForm.BringToFront();
                return;
            }

            _addForm = new AnimationListAddForm(GetActionNames, null, entry);
            _addForm.Show(FindForm());
        }

        private void ShowServUoForm(AnimationListEntry entry)
        {
            if (entry == null)
            {
                MessageBox.Show("Please select an animation in the list first.", "ServUO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_servUoForm != null && !_servUoForm.IsDisposed)
            {
                _servUoForm.SetAnimation(entry.Body, entry.IsEquipment ? 4 : entry.Type, entry.Name, null);
                _servUoForm.BringToFront();
                return;
            }

            _servUoForm = new ServUOSetupForm(entry.Body, entry.IsEquipment ? 4 : entry.Type, entry.Name, null);
            _servUoForm.Show(FindForm());
        }

        #endregion
    }
}

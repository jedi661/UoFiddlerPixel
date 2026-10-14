// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Controls.ArtLibrary.Metadata;

namespace UoFiddler.Controls.ArtLibrary.Forms
{
    /// <summary>
    /// Dialog "Kategorien und Tags verwalten". Categories are shown as a tree; a category can be moved
    /// into another one by drag and drop (drop on empty space = top level).
    /// </summary>
    public partial class ArtLibraryCategoryForm : Form
    {
        private readonly ArtLibraryMetadata _metadata;

        /// <summary>Designer only.</summary>
        public ArtLibraryCategoryForm() : this(new ArtLibraryMetadata())
        {
        }

        public ArtLibraryCategoryForm(ArtLibraryMetadata metadata)
        {
            InitializeComponent();
            _metadata = metadata ?? new ArtLibraryMetadata();
            FillCategories(null);
            FillTags(null);
        }

        /// <summary>True when something was changed (caller refreshes its filters).</summary>
        public bool Changed { get; private set; }

        #region [ Categories ]

        private void FillCategories(int? selectId)
        {
            treeViewCategories.BeginUpdate();
            treeViewCategories.Nodes.Clear();

            var nodes = new Dictionary<int, TreeNode>();
            foreach (ArtLibraryCategory category in _metadata.Categories.OrderBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                nodes[category.Id] = new TreeNode(category.Name) { Tag = category.Id };
            }

            foreach (ArtLibraryCategory category in _metadata.Categories.OrderBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                TreeNode node = nodes[category.Id];
                if (category.ParentId != null && nodes.TryGetValue(category.ParentId.Value, out TreeNode parent))
                {
                    parent.Nodes.Add(node);
                }
                else
                {
                    treeViewCategories.Nodes.Add(node);
                }
            }

            treeViewCategories.ExpandAll();
            if (selectId != null && nodes.TryGetValue(selectId.Value, out TreeNode selected))
            {
                treeViewCategories.SelectedNode = selected;
            }

            treeViewCategories.EndUpdate();
        }

        private int? SelectedCategoryId => treeViewCategories.SelectedNode?.Tag as int?;

        private string CategoryNameInput()
        {
            string name = textBoxCategory.Text.Trim();
            if (name.Length == 0)
            {
                MessageBox.Show(this, "Bitte zuerst einen Namen in das Feld eingeben.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBoxCategory.Focus();
                return null;
            }

            return name;
        }

        private void ButtonAddCategory_Click(object sender, EventArgs e)
        {
            string name = CategoryNameInput();
            if (name == null)
            {
                return;
            }

            ArtLibraryCategory category = _metadata.AddCategory(name);
            Changed = true;
            textBoxCategory.Clear();
            FillCategories(category.Id);
        }

        private void ButtonAddSubCategory_Click(object sender, EventArgs e)
        {
            if (SelectedCategoryId == null)
            {
                MessageBox.Show(this, "Bitte zuerst die übergeordnete Kategorie auswählen.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string name = CategoryNameInput();
            if (name == null)
            {
                return;
            }

            ArtLibraryCategory category = _metadata.AddCategory(name, SelectedCategoryId);
            Changed = true;
            textBoxCategory.Clear();
            FillCategories(category.Id);
        }

        private void ButtonRenameCategory_Click(object sender, EventArgs e)
        {
            int? id = SelectedCategoryId;
            if (id == null)
            {
                return;
            }

            string name = CategoryNameInput();
            if (name == null)
            {
                return;
            }

            _metadata.RenameCategory(id.Value, name);
            Changed = true;
            textBoxCategory.Clear();
            FillCategories(id);
        }

        private void ButtonDeleteCategory_Click(object sender, EventArgs e)
        {
            int? id = SelectedCategoryId;
            if (id == null)
            {
                return;
            }

            if (MessageBox.Show(this,
                    $"Kategorie \"{_metadata.GetCategoryPath(id)}\" löschen?\n\nUnterkategorien wandern eine Ebene nach oben, " +
                    "die Grafiken bleiben erhalten und verlieren nur diese Kategorie.",
                    Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _metadata.DeleteCategory(id.Value);
            Changed = true;
            FillCategories(null);
        }

        private void TreeViewCategories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            textBoxCategory.Text = e.Node?.Text ?? string.Empty;
        }

        private void TreeViewCategories_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Item is TreeNode)
            {
                DoDragDrop(e.Item, DragDropEffects.Move);
            }
        }

        private void TreeViewCategories_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(TreeNode)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void TreeViewCategories_DragDrop(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(typeof(TreeNode)) is TreeNode dragged) || !(dragged.Tag is int draggedId))
            {
                return;
            }

            Point point = treeViewCategories.PointToClient(new Point(e.X, e.Y));
            TreeNode target = treeViewCategories.GetNodeAt(point);
            int? parentId = target?.Tag as int?;

            if (!_metadata.CanSetParent(draggedId, parentId))
            {
                return;
            }

            _metadata.SetCategoryParent(draggedId, parentId);
            Changed = true;
            FillCategories(draggedId);
        }

        #endregion

        #region [ Tags ]

        private void FillTags(string select)
        {
            listBoxTags.BeginUpdate();
            listBoxTags.Items.Clear();
            foreach (string tag in _metadata.Tags)
            {
                listBoxTags.Items.Add(tag);
            }

            if (select != null)
            {
                listBoxTags.SelectedItem = _metadata.Tags.FirstOrDefault(t => string.Equals(t, select, StringComparison.OrdinalIgnoreCase));
            }

            listBoxTags.EndUpdate();
        }

        private void ButtonAddTag_Click(object sender, EventArgs e)
        {
            string tag = textBoxTag.Text.Trim();
            if (tag.Length == 0)
            {
                textBoxTag.Focus();
                return;
            }

            _metadata.AddTag(tag);
            Changed = true;
            textBoxTag.Clear();
            FillTags(tag);
        }

        private void ButtonRenameTag_Click(object sender, EventArgs e)
        {
            if (!(listBoxTags.SelectedItem is string oldTag))
            {
                return;
            }

            string newTag = textBoxTag.Text.Trim();
            if (newTag.Length == 0)
            {
                textBoxTag.Focus();
                return;
            }

            _metadata.RenameTag(oldTag, newTag);
            Changed = true;
            textBoxTag.Clear();
            FillTags(newTag);
        }

        private void ButtonDeleteTag_Click(object sender, EventArgs e)
        {
            if (!(listBoxTags.SelectedItem is string tag))
            {
                return;
            }

            if (MessageBox.Show(this, $"Tag \"{tag}\" löschen? Er wird auch bei allen Grafiken entfernt.", Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _metadata.DeleteTag(tag);
            Changed = true;
            FillTags(null);
        }

        private void ListBoxTags_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxTags.SelectedItem is string tag)
            {
                textBoxTag.Text = tag;
            }
        }

        #endregion
    }
}

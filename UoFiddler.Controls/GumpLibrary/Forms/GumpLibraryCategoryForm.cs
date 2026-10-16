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
using UoFiddler.Controls.GumpLibrary.Metadata;

namespace UoFiddler.Controls.GumpLibrary.Forms
{
    /// <summary>
    /// Manages the categories (tree, order, description, color) and tags of a gump library.
    /// Works directly on the metadata; <see cref="Changed"/> tells the caller to refresh and save.
    /// </summary>
    public partial class GumpLibraryCategoryForm : Form
    {
        private readonly GumpLibraryMetadata _metadata;
        private readonly List<int> _occupiedIds;
        private int? _pendingColor;
        private bool _colorChanged;

        public GumpLibraryCategoryForm(GumpLibraryMetadata metadata, IEnumerable<int> occupiedIds)
        {
            InitializeComponent();
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _occupiedIds = (occupiedIds ?? Enumerable.Empty<int>()).ToList();
            FillTree(null);
            FillTags();
            UpdateButtons();
        }

        /// <summary>True when something was changed.</summary>
        public bool Changed { get; private set; }

        /// <summary>Opens the tags tab first.</summary>
        public bool StartWithTags
        {
            set => tabControl.SelectedTab = value ? tabPageTags : tabPageCategories;
        }

        private int? SelectedCategoryId => treeViewCategories.SelectedNode?.Tag as int?;

        private void MarkChanged()
        {
            Changed = true;
            _metadata.MarkDirty();
        }

        #region [ Categories ]

        private void FillTree(int? selectId)
        {
            Dictionary<int, int> counts = _metadata.CountPerCategory(_occupiedIds);
            treeViewCategories.BeginUpdate();
            treeViewCategories.Nodes.Clear();
            var nodes = new Dictionary<int, TreeNode>();
            foreach ((GumpLibraryCategory category, int _) in _metadata.GetCategoryTree())
            {
                counts.TryGetValue(category.Id, out int count);
                var node = new TreeNode($"{category.Name}  ({count})") { Tag = category.Id };
                if (category.Color != null)
                {
                    node.ForeColor = Color.FromArgb(category.Color.Value);
                }

                if (!string.IsNullOrEmpty(category.Description))
                {
                    node.ToolTipText = category.Description;
                }

                if (category.ParentId != null && nodes.TryGetValue(category.ParentId.Value, out TreeNode parent))
                {
                    parent.Nodes.Add(node);
                }
                else
                {
                    treeViewCategories.Nodes.Add(node);
                }

                nodes[category.Id] = node;
            }

            treeViewCategories.ShowNodeToolTips = true;
            treeViewCategories.ExpandAll();
            treeViewCategories.EndUpdate();
            if (selectId != null && nodes.TryGetValue(selectId.Value, out TreeNode selected))
            {
                treeViewCategories.SelectedNode = selected;
                selected.EnsureVisible();
            }

            ShowCategory();
        }

        private void ShowCategory()
        {
            GumpLibraryCategory category = _metadata.GetCategory(SelectedCategoryId);
            textBoxCategoryName.Text = category?.Name ?? string.Empty;
            textBoxCategoryDescription.Text = category?.Description ?? string.Empty;
            _pendingColor = category?.Color;
            _colorChanged = false;
            buttonCategoryColor.BackColor = _pendingColor == null ? SystemColors.Control : Color.FromArgb(_pendingColor.Value);
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            bool has = SelectedCategoryId != null;
            buttonAddSubCategory.Enabled = has;
            buttonDeleteCategory.Enabled = has;
            buttonCategoryUp.Enabled = has;
            buttonCategoryDown.Enabled = has;
            buttonCategoryToTop.Enabled = has && _metadata.GetCategory(SelectedCategoryId)?.ParentId != null;
            textBoxCategoryName.Enabled = has;
            textBoxCategoryDescription.Enabled = has;
            buttonCategoryColor.Enabled = has;
            buttonCategoryNoColor.Enabled = has;
            buttonCategoryApply.Enabled = has;
        }

        private void TreeViewCategories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ShowCategory();
        }

        private void AddCategory(int? parentId)
        {
            string name = GumpLibraryInputForm.Ask(this, parentId == null ? "Neue Kategorie" : "Neue Unterkategorie",
                parentId == null ? "Name der neuen Kategorie:" : $"Name der neuen Unterkategorie von \"{_metadata.GetCategoryPath(parentId.Value)}\":");
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            GumpLibraryCategory category = _metadata.AddCategory(name, parentId);
            MarkChanged();
            FillTree(category.Id);
        }

        private void ButtonAddCategory_Click(object sender, EventArgs e) => AddCategory(null);

        private void ButtonAddSubCategory_Click(object sender, EventArgs e)
        {
            if (SelectedCategoryId != null)
            {
                AddCategory(SelectedCategoryId);
            }
        }

        private void ButtonDeleteCategory_Click(object sender, EventArgs e)
        {
            GumpLibraryCategory category = _metadata.GetCategory(SelectedCategoryId);
            if (category == null)
            {
                return;
            }

            int count = _metadata.CountPerCategory(_occupiedIds).TryGetValue(category.Id, out int c) ? c : 0;
            bool hasChildren = _metadata.Categories.Any(x => x.ParentId == category.Id);
            string text = $"Kategorie \"{_metadata.GetCategoryPath(category.Id)}\" löschen?\n\n" +
                          (count > 0 ? $"{count} Gump(s) verlieren diese Zuordnung (die Gumps selbst bleiben).\n" : string.Empty) +
                          (hasChildren ? "Unterkategorien rücken eine Ebene nach oben.\n" : string.Empty);
            if (MessageBox.Show(this, text, "Kategorie löschen", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            _metadata.DeleteCategory(category.Id);
            MarkChanged();
            FillTree(category.ParentId);
        }

        private void MoveSelected(int direction)
        {
            int? id = SelectedCategoryId;
            if (id == null)
            {
                return;
            }

            _metadata.MoveCategory(id.Value, direction);
            MarkChanged();
            FillTree(id);
        }

        private void ButtonCategoryUp_Click(object sender, EventArgs e) => MoveSelected(-1);

        private void ButtonCategoryDown_Click(object sender, EventArgs e) => MoveSelected(1);

        private void ButtonCategoryToTop_Click(object sender, EventArgs e)
        {
            GumpLibraryCategory category = _metadata.GetCategory(SelectedCategoryId);
            if (category?.ParentId == null)
            {
                return;
            }

            _metadata.SetCategoryParent(category.Id, _metadata.GetCategory(category.ParentId)?.ParentId);
            MarkChanged();
            FillTree(category.Id);
        }

        private void ButtonCategoryColor_Click(object sender, EventArgs e)
        {
            if (_pendingColor != null)
            {
                colorDialog.Color = Color.FromArgb(_pendingColor.Value);
            }

            if (colorDialog.ShowDialog(this) == DialogResult.OK)
            {
                _pendingColor = colorDialog.Color.ToArgb();
                _colorChanged = true;
                buttonCategoryColor.BackColor = colorDialog.Color;
            }
        }

        private void ButtonCategoryNoColor_Click(object sender, EventArgs e)
        {
            _pendingColor = null;
            _colorChanged = true;
            buttonCategoryColor.BackColor = SystemColors.Control;
        }

        private void ButtonCategoryApply_Click(object sender, EventArgs e)
        {
            GumpLibraryCategory category = _metadata.GetCategory(SelectedCategoryId);
            if (category == null)
            {
                return;
            }

            string name = textBoxCategoryName.Text.Trim();
            if (name.Length == 0)
            {
                MessageBox.Show(this, "Der Name darf nicht leer sein.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            category.Name = name;
            category.Description = string.IsNullOrWhiteSpace(textBoxCategoryDescription.Text) ? null : textBoxCategoryDescription.Text.Trim();
            if (_colorChanged)
            {
                category.Color = _pendingColor;
            }

            MarkChanged();
            FillTree(category.Id);
        }

        private void TreeViewCategories_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Item is TreeNode node)
            {
                DoDragDrop(node, DragDropEffects.Move);
            }
        }

        private void TreeViewCategories_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if (!(e.Data.GetData(typeof(TreeNode)) is TreeNode dragged) || !(dragged.Tag is int id))
            {
                return;
            }

            TreeNode target = treeViewCategories.GetNodeAt(treeViewCategories.PointToClient(new Point(e.X, e.Y)));
            int? targetId = target?.Tag as int?;
            if (_metadata.CanSetParent(id, targetId))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void TreeViewCategories_DragDrop(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(typeof(TreeNode)) is TreeNode dragged) || !(dragged.Tag is int id))
            {
                return;
            }

            TreeNode target = treeViewCategories.GetNodeAt(treeViewCategories.PointToClient(new Point(e.X, e.Y)));
            int? targetId = target?.Tag as int?;
            if (!_metadata.CanSetParent(id, targetId))
            {
                return;
            }

            // dropped on empty space = top level
            _metadata.SetCategoryParent(id, targetId);
            MarkChanged();
            FillTree(id);
        }

        #endregion

        #region [ Tags ]

        private sealed class TagItem
        {
            public string Tag;
            public int Count;

            public override string ToString() => $"{Tag}  ({Count})";
        }

        private void FillTags(string select = null)
        {
            var occupied = new HashSet<int>(_occupiedIds);
            listBoxTags.BeginUpdate();
            listBoxTags.Items.Clear();
            foreach (string tag in _metadata.GetAllTags())
            {
                int count = _metadata.Items.Count(p => occupied.Contains(p.Key) && p.Value.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
                int index = listBoxTags.Items.Add(new TagItem { Tag = tag, Count = count });
                if (string.Equals(tag, select, StringComparison.OrdinalIgnoreCase))
                {
                    listBoxTags.SelectedIndex = index;
                }
            }

            listBoxTags.EndUpdate();
        }

        private List<string> SelectedTags => listBoxTags.SelectedItems.Cast<TagItem>().Select(t => t.Tag).ToList();

        private void ButtonAddTag_Click(object sender, EventArgs e)
        {
            string text = GumpLibraryInputForm.Ask(this, "Neuer Tag", "Neue Tags (mehrere mit Komma trennen):");
            List<string> tags = GumpLibraryMetadata.ParseTags(text);
            if (tags.Count == 0)
            {
                return;
            }

            foreach (string tag in tags)
            {
                _metadata.AddTag(tag);
            }

            MarkChanged();
            FillTags(tags[0]);
        }

        private void ButtonRenameTag_Click(object sender, EventArgs e)
        {
            List<string> selected = SelectedTags;
            if (selected.Count != 1)
            {
                MessageBox.Show(this, "Bitte genau einen Tag auswählen.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string newTag = GumpLibraryInputForm.Ask(this, "Tag umbenennen", $"Neuer Name für \"{selected[0]}\" (wird bei allen Gumps geändert):", selected[0]);
            if (string.IsNullOrWhiteSpace(newTag) || newTag == selected[0])
            {
                return;
            }

            _metadata.RenameTag(selected[0], newTag);
            MarkChanged();
            FillTags(newTag);
        }

        private void ButtonDeleteTag_Click(object sender, EventArgs e)
        {
            List<string> selected = SelectedTags;
            if (selected.Count == 0)
            {
                return;
            }

            if (MessageBox.Show(this, $"{selected.Count} Tag(s) löschen? Sie werden auch bei allen Gumps entfernt.\n\n{string.Join(", ", selected)}",
                    "Tags löschen", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            foreach (string tag in selected)
            {
                _metadata.DeleteTag(tag);
            }

            MarkChanged();
            FillTags();
        }

        private void ButtonCleanTags_Click(object sender, EventArgs e)
        {
            var used = new HashSet<string>(_metadata.Items.Values.SelectMany(i => i.Tags), StringComparer.OrdinalIgnoreCase);
            List<string> unused = _metadata.Tags.Where(t => !used.Contains(t)).ToList();
            if (unused.Count == 0)
            {
                MessageBox.Show(this, "Alle Tags werden benutzt.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (string tag in unused)
            {
                _metadata.DeleteTag(tag);
            }

            MarkChanged();
            FillTags();
        }

        #endregion
    }
}

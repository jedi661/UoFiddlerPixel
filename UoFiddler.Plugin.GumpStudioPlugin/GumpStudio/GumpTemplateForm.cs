using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    public enum TemplateAction
    {
        None,
        Open,
        Insert
    }

    /// <summary>
    /// Template library: built-in and own gump templates in categories (folders), with pictures.
    /// Open = new gump from the template, Insert = add its elements to the current gump.
    /// </summary>
    public partial class GumpTemplateForm : Form
    {
        private const string AllTag = "\u0001all";

        /// <summary>Rendered previews (key = file or built-in name), kept while UoFiddler runs.</summary>
        private static readonly Dictionary<string, (DateTime Changed, Bitmap Picture)> Previews = new Dictionary<string, (DateTime, Bitmap)>();

        private static string _lastCategory;
        private static int _lastThumbSize = 1;

        private readonly bool _canInsert;
        private List<GumpTemplateEntry> _entries = new List<GumpTemplateEntry>();
        private bool _descriptionDirty;
        private GumpTemplateEntry _shown;

        public GumpTemplateForm()
            : this(false)
        {
        }

        public GumpTemplateForm(bool canInsert)
        {
            InitializeComponent();
            _canInsert = canInsert;
            btnInsert.Visible = canInsert;
            miInsert.Visible = canInsert;
        }

        /// <summary>What the user chose (Open / Insert) and the template's design.</summary>
        public TemplateAction Action { get; private set; }

        public GumpTemplateEntry SelectedEntry => listTemplates.SelectedItems.Count > 0 ? listTemplates.SelectedItems[0].Tag as GumpTemplateEntry : null;

        public GumpDesign SelectedDesign { get; private set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode)
            {
                return;
            }
            tsThumbSize.SelectedIndex = _lastThumbSize;   // also loads the list
            Reload(_lastCategory, null);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _lastCategory = CurrentCategory;
            _lastThumbSize = tsThumbSize.SelectedIndex;
            base.OnFormClosed(e);
        }

        #region Loading

        /// <summary>Selected category ("" = all, Built-in, or a user category path).</summary>
        private string CurrentCategory => treeCategories.SelectedNode?.Tag as string ?? AllTag;

        private static bool IsUserCategory(string c) => c != null && !c.StartsWith(AllTag, StringComparison.Ordinal) && c != GumpTemplateLibrary.BuiltInCategory;

        /// <summary>Category for new templates / imports: the selected user category or "General".</summary>
        private string TargetCategory => IsUserCategory(CurrentCategory) ? CurrentCategory : GumpTemplateLibrary.DefaultCategory;

        private void Reload(string selectCategory, string selectFile)
        {
            var errors = new List<string>();
            _entries = GumpTemplateLibrary.BuiltIn().Concat(GumpTemplateLibrary.LoadUser(errors)).ToList();
            FillTree(selectCategory ?? CurrentCategory);
            FillList(selectFile);
            if (errors.Count > 0)
            {
                lblStatus.Text = $"{errors.Count} file(s) could not be read: " + string.Join("; ", errors.Take(3));
            }
        }

        private void FillTree(string select)
        {
            treeCategories.BeginUpdate();
            treeCategories.Nodes.Clear();
            int users = _entries.Count(e => !e.BuiltIn);
            treeCategories.Nodes.Add(new TreeNode($"All ({_entries.Count})") { Tag = AllTag, NodeFont = new Font(treeCategories.Font, FontStyle.Bold) });
            treeCategories.Nodes.Add(new TreeNode($"{GumpTemplateLibrary.BuiltInCategory} ({_entries.Count(e => e.BuiltIn)})") { Tag = GumpTemplateLibrary.BuiltInCategory });
            var mine = new TreeNode($"My templates ({users})") { Tag = AllTag + "mine" };
            treeCategories.Nodes.Add(mine);
            IEnumerable<string> categories = GumpTemplateLibrary.Categories().Concat(_entries.Where(e => !e.BuiltIn).Select(e => e.Category)).Distinct(StringComparer.OrdinalIgnoreCase);
            foreach (string cat in categories.OrderBy(c => c, StringComparer.CurrentCultureIgnoreCase))
            {
                TreeNodeCollection parent = mine.Nodes;
                string path = string.Empty;
                foreach (string part in cat.Split('/'))
                {
                    path = path.Length == 0 ? part : path + "/" + part;
                    TreeNode node = parent.Cast<TreeNode>().FirstOrDefault(n => string.Equals((string)n.Tag, path, StringComparison.OrdinalIgnoreCase));
                    if (node == null)
                    {
                        node = new TreeNode(part) { Tag = path };
                        parent.Add(node);
                    }
                    parent = node.Nodes;
                }
            }
            UpdateCounts(mine.Nodes);
            mine.Expand();
            TreeNode sel = Find(treeCategories.Nodes, select) ?? treeCategories.Nodes[0];
            treeCategories.EndUpdate();
            treeCategories.SelectedNode = sel;
            sel.EnsureVisible();
        }

        private void UpdateCounts(TreeNodeCollection nodes)
        {
            foreach (TreeNode n in nodes)
            {
                string cat = (string)n.Tag;
                int count = _entries.Count(e => !e.BuiltIn && InCategory(e, cat));
                n.Text = cat.Split('/').Last() + (count > 0 ? $" ({count})" : string.Empty);
                UpdateCounts(n.Nodes);
            }
        }

        private static TreeNode Find(TreeNodeCollection nodes, string tag)
        {
            foreach (TreeNode n in nodes)
            {
                if (string.Equals(n.Tag as string, tag, StringComparison.OrdinalIgnoreCase))
                {
                    return n;
                }
                TreeNode inner = Find(n.Nodes, tag);
                if (inner != null)
                {
                    return inner;
                }
            }
            return null;
        }

        private static bool InCategory(GumpTemplateEntry e, string cat)
        {
            if (cat == AllTag)
            {
                return true;
            }
            if (cat == AllTag + "mine")
            {
                return !e.BuiltIn;
            }
            return string.Equals(e.Category, cat, StringComparison.OrdinalIgnoreCase) || e.Category.StartsWith(cat + "/", StringComparison.OrdinalIgnoreCase);
        }

        private void FillList(string selectFile = null)
        {
            SaveDescriptionIfDirty();
            string search = tsSearch.Text.Trim();
            string cat = CurrentCategory;
            IEnumerable<GumpTemplateEntry> shown = search.Length > 0
                ? _entries.Where(e => Contains(e.Name, search) || Contains(e.Description, search) || Contains(e.Category, search))
                : _entries.Where(e => InCategory(e, cat));
            listTemplates.BeginUpdate();
            listTemplates.Items.Clear();
            bool showCategory = search.Length > 0 || cat == AllTag || cat == AllTag + "mine";
            foreach (GumpTemplateEntry e in shown)
            {
                string key = Key(e);
                if (!imageThumbs.Images.ContainsKey(key))
                {
                    imageThumbs.Images.Add(key, Thumb(Preview(e), imageThumbs.ImageSize));
                }
                var item = new ListViewItem(showCategory ? $"{e.Name}\n[{e.Category}]" : e.Name) { Tag = e, ImageKey = key, ToolTipText = e.Description };
                listTemplates.Items.Add(item);
                if (selectFile != null && string.Equals(e.File ?? e.Name, selectFile, StringComparison.OrdinalIgnoreCase))
                {
                    item.Selected = true;
                    item.Focused = true;
                }
            }
            listTemplates.ShowItemToolTips = true;
            listTemplates.EndUpdate();
            if (listTemplates.SelectedItems.Count > 0)
            {
                listTemplates.SelectedItems[0].EnsureVisible();
            }
            else if (listTemplates.Items.Count > 0)
            {
                listTemplates.Items[0].Selected = true;
            }
            ShowDetails();
            lblStatus.Text = $"{listTemplates.Items.Count} template(s)" + (search.Length > 0 ? $" for \"{search}\"" : string.Empty) + " · " + GumpTemplateLibrary.Folder;
        }

        private static bool Contains(string text, string search) => (text ?? string.Empty).IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0;

        private static string Key(GumpTemplateEntry e) => e.BuiltIn ? "builtin:" + e.Name : e.File;

        /// <summary>Picture of page 1 of the template (cached; null without client files).</summary>
        private static Bitmap Preview(GumpTemplateEntry e)
        {
            string key = Key(e);
            DateTime changed = e.BuiltIn ? DateTime.MinValue : e.Changed;
            if (Previews.TryGetValue(key, out (DateTime Changed, Bitmap Picture) p) && p.Changed == changed)
            {
                return p.Picture;
            }
            Bitmap bmp = null;
            try
            {
                bmp = GumpRenderer.RenderImage(e.Load(), 1);
            }
            catch (Exception)
            {
                // client files not loaded or broken template
            }
            if (Previews.TryGetValue(key, out p))
            {
                p.Picture?.Dispose();
            }
            Previews[key] = (changed, bmp);
            return bmp;
        }

        private static Bitmap Thumb(Bitmap picture, Size size)
        {
            var bmp = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(40, 40, 40));
                if (picture == null)
                {
                    TextRenderer.DrawText(g, "no picture", SystemFonts.DefaultFont, new Rectangle(Point.Empty, size), Color.Silver, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                else
                {
                    float scale = Math.Min(1f, Math.Min((size.Width - 6f) / picture.Width, (size.Height - 6f) / picture.Height));
                    int w = Math.Max(1, (int)(picture.Width * scale)), h = Math.Max(1, (int)(picture.Height * scale));
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(picture, (size.Width - w) / 2, (size.Height - h) / 2, w, h);
                }
                g.DrawRectangle(Pens.Gray, 0, 0, size.Width - 1, size.Height - 1);
            }
            return bmp;
        }

        private void ShowDetails()
        {
            GumpTemplateEntry e = SelectedEntry;
            _shown = e;
            picturePreview.Image?.Dispose();
            picturePreview.Image = null;
            bool user = e != null && !e.BuiltIn;
            btnOpen.Enabled = miOpen.Enabled = e != null;
            btnInsert.Enabled = miInsert.Enabled = e != null && _canInsert;
            tsRename.Enabled = miRename.Enabled = tsDelete.Enabled = miDelete.Enabled = miShowFile.Enabled = user;
            tsDuplicate.Enabled = miDuplicate.Enabled = tsExport.Enabled = e != null;
            txtDescription.ReadOnly = !user;
            if (e == null)
            {
                lblTitle.Text = "–";
                lblInfo.Text = string.Empty;
                txtDescription.Text = string.Empty;
            }
            else
            {
                Bitmap p = Preview(e);
                picturePreview.Image = p == null ? null : new Bitmap(p);
                lblTitle.Text = e.Name;
                lblInfo.Text = $"{e.Category} · {e.ElementCount} elements · {e.Pages} page(s)" + (e.BuiltIn ? " · built-in (copy it to change it)" : $" · {e.Changed:g}");
                txtDescription.Text = e.Description;
            }
            _descriptionDirty = false;
            btnSaveDescription.Enabled = false;
        }

        #endregion

        #region Events

        private void TreeCategories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            tsDeleteCategory.Enabled = IsUserCategory(CurrentCategory) && !CurrentCategory.StartsWith(AllTag, StringComparison.Ordinal);
            if (tsSearch.Text.Length > 0)
            {
                tsSearch.Text = string.Empty;   // FillList via TextChanged
                return;
            }
            FillList();
        }

        private void ListTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveDescriptionIfDirty();
            ShowDetails();
        }

        private void TsSearch_TextChanged(object sender, EventArgs e) => FillList();

        private void TsThumbSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            Size[] sizes = { new Size(96, 72), new Size(128, 96), new Size(200, 150) };
            imageThumbs.Images.Clear();
            imageThumbs.ImageSize = sizes[Math.Max(0, tsThumbSize.SelectedIndex)];
            if (_entries.Count > 0)
            {
                FillList(SelectedEntry?.File ?? SelectedEntry?.Name);
            }
        }

        private void TsRefresh_Click(object sender, EventArgs e) => Reload(CurrentCategory, SelectedEntry?.File);

        private void ListTemplates_DoubleClick(object sender, EventArgs e) => BtnOpen_Click(sender, e);

        private void BtnOpen_Click(object sender, EventArgs e) => Finish(TemplateAction.Open);

        private void BtnInsert_Click(object sender, EventArgs e) => Finish(TemplateAction.Insert);

        private void Finish(TemplateAction action)
        {
            GumpTemplateEntry entry = SelectedEntry;
            if (entry == null || action == TemplateAction.Insert && !_canInsert)
            {
                return;
            }
            try
            {
                SaveDescriptionIfDirty();
                SelectedDesign = entry.Load();
                Action = action;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The template could not be read:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ContextList_Opening(object sender, System.ComponentModel.CancelEventArgs e) => e.Cancel = SelectedEntry == null;

        private void ListTemplates_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && tsDelete.Enabled)
            {
                TsDelete_Click(sender, e);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F2 && tsRename.Enabled)
            {
                TsRename_Click(sender, e);
                e.Handled = true;
            }
        }

        private void GumpTemplateForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                TsRefresh_Click(sender, e);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F3 || e.Control && e.KeyCode == Keys.F)
            {
                tsSearch.Focus();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F2 && treeCategories.Focused && treeCategories.SelectedNode != null)
            {
                treeCategories.SelectedNode.BeginEdit();
                e.Handled = true;
            }
        }

        #endregion

        #region Templates

        private void TsRename_Click(object sender, EventArgs e)
        {
            GumpTemplateEntry entry = SelectedEntry;
            if (entry == null || entry.BuiltIn)
            {
                return;
            }
            SaveDescriptionIfDirty();
            using (var f = new GumpTemplateSaveForm(entry.Load(), null, entry.Name, entry.Category, entry.Description, entry.File))
            {
                if (f.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    string file = GumpTemplateLibrary.Move(entry, f.TemplateName, f.Category);
                    entry.File = file;
                    if (f.Description != entry.Description)
                    {
                        GumpTemplateLibrary.SetDescription(entry, f.Description);
                    }
                    Reload(f.Category, file);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void TsDuplicate_Click(object sender, EventArgs e)
        {
            GumpTemplateEntry entry = SelectedEntry;
            if (entry == null)
            {
                return;
            }
            try
            {
                string cat = entry.BuiltIn ? TargetCategory : entry.Category;
                string name = GumpTemplateLibrary.Unique(entry.BuiltIn ? entry.Name : entry.Name + " – copy", cat);
                GumpDesign d = entry.Load();
                string file = GumpTemplateLibrary.Save(d, name, cat, entry.BuiltIn ? string.Empty : entry.Description);
                Reload(cat, file);
                lblStatus.Text = $"Copied to {cat} / {name}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TsDelete_Click(object sender, EventArgs e)
        {
            GumpTemplateEntry entry = SelectedEntry;
            if (entry == null || entry.BuiltIn)
            {
                return;
            }
            if (MessageBox.Show(this, $"Delete the template \"{entry.Name}\" ({entry.Category})?\n\nDE: Vorlage löschen?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                GumpTemplateLibrary.Delete(entry);
                Reload(CurrentCategory, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TsExport_Click(object sender, EventArgs e)
        {
            GumpTemplateEntry entry = SelectedEntry;
            if (entry == null)
            {
                return;
            }
            using (var d = new SaveFileDialog { Filter = "Gump Studio project (*.gump.json)|*.gump.json", FileName = GumpTemplateLibrary.Safe(entry.Name) + GumpTemplateLibrary.Extension })
            {
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    File.WriteAllText(d.FileName, entry.Load().ToJson(), new System.Text.UTF8Encoding(false));
                    lblStatus.Text = "Exported to " + d.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void TxtDescription_TextChanged(object sender, EventArgs e)
        {
            if (!txtDescription.ReadOnly && _shown != null && txtDescription.Text != _shown.Description)
            {
                _descriptionDirty = true;
                btnSaveDescription.Enabled = true;
            }
        }

        private void BtnSaveDescription_Click(object sender, EventArgs e) => SaveDescriptionIfDirty();

        private void SaveDescriptionIfDirty()
        {
            GumpTemplateEntry entry = _shown;
            if (!_descriptionDirty || entry == null || entry.BuiltIn)
            {
                return;
            }
            try
            {
                GumpTemplateLibrary.SetDescription(entry, txtDescription.Text.Trim());
                lblStatus.Text = "Note saved.";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Note not saved: " + ex.Message;
            }
            _descriptionDirty = false;
            btnSaveDescription.Enabled = false;
        }

        private void MiShowFile_Click(object sender, EventArgs e)
        {
            GumpTemplateEntry entry = SelectedEntry;
            if (entry?.File != null)
            {
                try
                {
                    Process.Start("explorer.exe", "/select,\"" + entry.File + "\"");
                }
                catch (Exception ex)
                {
                    lblStatus.Text = ex.Message;
                }
            }
        }

        private void TsOpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                string dir = IsUserCategory(CurrentCategory) && !CurrentCategory.StartsWith(AllTag, StringComparison.Ordinal)
                    ? GumpTemplateLibrary.CategoryFolder(CurrentCategory)
                    : GumpTemplateLibrary.Folder;
                Directory.CreateDirectory(dir);
                Process.Start("explorer.exe", "\"" + dir + "\"");
            }
            catch (Exception ex)
            {
                lblStatus.Text = ex.Message;
            }
        }

        #endregion

        #region Adding files

        private void TsImportFiles_Click(object sender, EventArgs e)
        {
            using (var d = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Add to " + TargetCategory,
                Filter = "Gump projects and ServUO scripts (*.gump.json;*.json;*.cs)|*.gump.json;*.json;*.cs|All files|*.*"
            })
            {
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    AddFiles(d.FileNames);
                }
            }
        }

        private void AddFiles(IEnumerable<string> files)
        {
            var messages = new List<string>();
            string cat = TargetCategory;
            Cursor = Cursors.WaitCursor;
            int n;
            try
            {
                n = GumpTemplateLibrary.ImportFiles(files, cat, messages);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
            Reload(cat, null);
            lblStatus.Text = $"{n} template(s) added to {cat}." + (messages.Count > 0 ? " Problems: " + string.Join("; ", messages.Take(3)) : string.Empty);
        }

        private void ListTemplates_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void ListTemplates_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
            {
                AddFiles(files.Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)));
            }
        }

        private void ListTemplates_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Item is ListViewItem item && item.Tag is GumpTemplateEntry entry && !entry.BuiltIn)
            {
                listTemplates.DoDragDrop(entry, DragDropEffects.Move);
            }
        }

        private void TreeCategories_DragOver(object sender, DragEventArgs e)
        {
            TreeNode node = treeCategories.GetNodeAt(treeCategories.PointToClient(new Point(e.X, e.Y)));
            bool userNode = node != null && IsUserCategory(node.Tag as string) && !((string)node.Tag).StartsWith(AllTag, StringComparison.Ordinal);
            if (e.Data.GetDataPresent(typeof(GumpTemplateEntry)) && userNode)
            {
                e.Effect = DragDropEffects.Move;
                treeCategories.SelectedNode = node;
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop) && userNode)
            {
                e.Effect = DragDropEffects.Copy;
                treeCategories.SelectedNode = node;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void TreeCategories_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode node = treeCategories.GetNodeAt(treeCategories.PointToClient(new Point(e.X, e.Y)));
            if (node == null || !(node.Tag is string cat) || !IsUserCategory(cat) || cat.StartsWith(AllTag, StringComparison.Ordinal))
            {
                return;
            }
            if (e.Data.GetData(typeof(GumpTemplateEntry)) is GumpTemplateEntry entry)
            {
                try
                {
                    string file = GumpTemplateLibrary.Move(entry, entry.Name, cat);
                    Reload(cat, file);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
            {
                AddFiles(files);
            }
        }

        #endregion

        #region Categories

        private void TsNewCategory_Click(object sender, EventArgs e)
        {
            string parent = IsUserCategory(CurrentCategory) && !CurrentCategory.StartsWith(AllTag, StringComparison.Ordinal) ? CurrentCategory + "/" : string.Empty;
            string name = "New category";
            for (int i = 2; Directory.Exists(GumpTemplateLibrary.CategoryFolder(parent + name)); i++)
            {
                name = $"New category {i}";
            }
            try
            {
                GumpTemplateLibrary.CreateCategory(parent + name);
                Reload(parent + name, null);
                treeCategories.SelectedNode?.BeginEdit();
                lblStatus.Text = "Type the name of the new category. / Namen der neuen Kategorie eintippen.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TsDeleteCategory_Click(object sender, EventArgs e)
        {
            string cat = CurrentCategory;
            if (!IsUserCategory(cat) || cat.StartsWith(AllTag, StringComparison.Ordinal))
            {
                return;
            }
            try
            {
                if (!GumpTemplateLibrary.DeleteCategory(cat))
                {
                    MessageBox.Show(this, $"\"{cat}\" still holds templates – move or delete them first.\n\nDE: Die Kategorie enthält noch Vorlagen – erst verschieben oder löschen.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                Reload(AllTag + "mine", null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TreeCategories_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            string cat = e.Node.Tag as string;
            e.CancelEdit = !IsUserCategory(cat) || cat.StartsWith(AllTag, StringComparison.Ordinal);
            if (!e.CancelEdit)
            {
                e.Node.Text = cat.Split('/').Last();   // without the count
            }
        }

        private void TreeCategories_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            string old = (string)e.Node.Tag;
            e.CancelEdit = true;
            string label = GumpTemplateLibrary.Safe(e.Label ?? string.Empty);
            if (label.Length == 0 || label == old.Split('/').Last())
            {
                BeginInvoke((Action)(() => UpdateCounts(Find(treeCategories.Nodes, AllTag + "mine").Nodes)));
                return;
            }
            string parent = old.Contains("/") ? old.Substring(0, old.LastIndexOf('/') + 1) : string.Empty;
            string target = parent + label;
            try
            {
                string from = GumpTemplateLibrary.CategoryFolder(old), to = GumpTemplateLibrary.CategoryFolder(target);
                if (Directory.Exists(to) && !string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
                {
                    throw new IOException($"The category \"{target}\" already exists.");
                }
                if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
                {
                    Directory.Move(from, from + ".rename");
                    from += ".rename";
                }
                Directory.Move(from, to);
                BeginInvoke((Action)(() => Reload(target, null)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                BeginInvoke((Action)(() => Reload(old, null)));
            }
        }

        #endregion
    }
}

/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
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
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Strings;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Viewer / editor for string_dictionary.uop.
    /// </summary>
    public sealed class EcStringsControl : UserControl, IEcEditor
    {
        private readonly ToolStripTextBox _filterBox;
        private readonly ToolStripButton _saveButton;
        private readonly ListView _list;
        private readonly ToolStripStatusLabel _statusLabel;

        private EcClient _client;
        private EcStringDictionary _strings;
        private List<int> _view = new List<int>();
        private bool _loaded;

        public EcStringsControl()
        {
            Dock = DockStyle.Fill;

            var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _filterBox = new ToolStripTextBox { Width = 220, ToolTipText = "Part of the text, or #index – Enter" };
            _filterBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ApplyFilter(); } };
            _saveButton = new ToolStripButton("Save", null, (s, e) => Save(false)) { Enabled = false };

            toolStrip.Items.Add(new ToolStripLabel("Search:"));
            toolStrip.Items.Add(_filterBox);
            toolStrip.Items.Add(new ToolStripButton("Apply", null, (s, e) => ApplyFilter()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("Add...", null, (s, e) => AddString()));
            toolStrip.Items.Add(new ToolStripButton("Edit...", null, (s, e) => EditSelected()) { ToolTipText = "Change the text (every tileart entry using it changes too)" });
            toolStrip.Items.Add(new ToolStripButton("Export (txt)...", null, (s, e) => Export()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(_saveButton);
            toolStrip.Items.Add(new ToolStripButton("Save as...", null, (s, e) => Save(true)));

            _list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false, VirtualMode = true, MultiSelect = false };
            _list.Columns.Add("Index", 80, HorizontalAlignment.Right);
            _list.Columns.Add("Text", 700);
            _list.RetrieveVirtualItem += RetrieveItem;
            _list.DoubleClick += (s, e) => EditSelected();

            var menu = new ContextMenuStrip();
            menu.Items.Add("Copy text", null, (s, e) => { int i = SelectedIndex; if (i > 0) { Clipboard.SetText(_strings.Get((uint)i)); } });
            menu.Items.Add("Copy index", null, (s, e) => { int i = SelectedIndex; if (i > 0) { Clipboard.SetText(i.ToString()); } });
            _list.ContextMenuStrip = menu;

            var status = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Choose the Enhanced Client folder on top.") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            status.Items.Add(_statusLabel);

            Controls.Add(_list);
            Controls.Add(toolStrip);
            Controls.Add(status);
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            _strings = null;
            _view = new List<int>();
            _list.VirtualListSize = 0;
            _loaded = false;
            _saveButton.Enabled = false;
            if (Visible && client != null)
            {
                OnActivated();
            }
        }

        public void OnActivated()
        {
            if (_client == null)
            {
                return;
            }

            if (_loaded)
            {
                // names may have been added by the TileArt editor
                _saveButton.Enabled = _strings?.IsModified == true || (_strings?.Package.IsDirty ?? false);
                if (_strings != null && _view.Count != _strings.Count && _filterBox.Text.Length == 0)
                {
                    ApplyFilter();
                }
                return;
            }
            _loaded = true;

            try
            {
                if (!_client.HasStringDictionary)
                {
                    _statusLabel.Text = $"string_dictionary.uop was not found in {_client.Folder}.";
                    return;
                }
                _strings = _client.StringDictionary;
                ApplyFilter();
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Loading string_dictionary.uop failed.", ex);
            }
        }

        private void ApplyFilter()
        {
            if (_strings == null)
            {
                return;
            }

            string filter = _filterBox.Text.Trim();
            IEnumerable<int> indices = Enumerable.Range(1, _strings.Count);
            if (filter.StartsWith("#") && int.TryParse(filter.Substring(1), out int number))
            {
                indices = indices.Where(i => i == number);
            }
            else if (filter.Length > 0)
            {
                indices = indices.Where(i => _strings.Get((uint)i).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            _view = indices.ToList();
            _list.VirtualListSize = _view.Count;
            _list.Invalidate();
            _saveButton.Enabled = _strings.IsModified || _strings.Package.IsDirty;
            _statusLabel.Text = $"string_dictionary.uop: {_strings.Count} strings" + (filter.Length > 0 ? $", {_view.Count} found" : string.Empty);
        }

        private void RetrieveItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= _view.Count)
            {
                e.Item = new ListViewItem(string.Empty);
                return;
            }

            int index = _view[e.ItemIndex];
            var item = new ListViewItem(index.ToString());
            item.SubItems.Add(_strings.Get((uint)index));
            e.Item = item;
        }

        private int SelectedIndex => _list.SelectedIndices.Count == 1 && _list.SelectedIndices[0] < _view.Count ? _view[_list.SelectedIndices[0]] : -1;

        private void AddString()
        {
            if (_strings == null)
            {
                return;
            }

            string text = EcDialogs.Prompt(this, "Add text", "Text:", string.Empty);
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            uint existing = _strings.Find(text);
            uint index = _strings.GetOrAdd(text);
            _filterBox.Text = "#" + index;
            ApplyFilter();
            _statusLabel.Text = existing != 0 ? $"The text already exists as #{index}." : $"Added as #{index}. Save to keep it.";
        }

        private void EditSelected()
        {
            int index = SelectedIndex;
            if (index <= 0)
            {
                return;
            }

            string text = EcDialogs.Prompt(this, $"Edit #{index}", "Text:", _strings.Get((uint)index));
            if (text == null)
            {
                return;
            }

            _strings.Set((uint)index, text);
            _list.Invalidate();
            _saveButton.Enabled = true;
        }

        private void Export()
        {
            if (_strings == null)
            {
                return;
            }

            using (var dialog = new SaveFileDialog { Filter = "Text|*.txt", FileName = "string_dictionary.txt" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllLines(dialog.FileName, Enumerable.Range(1, _strings.Count).Select(i => $"{i}\t{_strings.Get((uint)i)}"));
                }
            }
        }

        private void Save(bool saveAs)
        {
            if (_strings == null)
            {
                return;
            }

            string target = _strings.Package.FilePath;
            if (saveAs)
            {
                using (var dialog = new SaveFileDialog { Filter = "UOP files (*.uop)|*.uop", FileName = Path.GetFileName(target) })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                    target = dialog.FileName;
                }
            }
            else if (!EcDialogs.Confirm(this, "Save string_dictionary.uop?\r\nThe old file is kept as .bak next to it.\r\n\r\nThe Enhanced Client must be closed."))
            {
                return;
            }

            try
            {
                _strings.Apply();
                _strings.Package.Save(target);
                _saveButton.Enabled = false;
                _statusLabel.Text = $"Saved {target}.";
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Saving failed (is the file opened by the client?).", ex);
            }
        }
    }
}

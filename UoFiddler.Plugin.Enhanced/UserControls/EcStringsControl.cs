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
    public sealed partial class EcStringsControl : UserControl, IEcEditor
    {
        private EcClient _client;
        private EcStringDictionary _strings;
        private List<int> _view = new List<int>();
        private bool _loaded;

        public EcStringsControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            _strings = null;
            _view = new List<int>();
            listViewStrings.VirtualListSize = 0;
            _loaded = false;
            toolStripButtonSave.Enabled = false;
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
                toolStripButtonSave.Enabled = _strings?.IsModified == true || (_strings?.Package.IsDirty ?? false);
                if (_strings != null && _view.Count != _strings.Count && toolStripTextBoxFilter.Text.Length == 0)
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
                    toolStripStatusLabel.Text = $"string_dictionary.uop was not found in {_client.Folder}.";
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

            string filter = toolStripTextBoxFilter.Text.Trim();
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
            listViewStrings.VirtualListSize = _view.Count;
            listViewStrings.Invalidate();
            toolStripButtonSave.Enabled = _strings.IsModified || _strings.Package.IsDirty;
            toolStripStatusLabel.Text = $"string_dictionary.uop: {_strings.Count} strings" + (filter.Length > 0 ? $", {_view.Count} found" : string.Empty);
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

        private int SelectedIndex => listViewStrings.SelectedIndices.Count == 1 && listViewStrings.SelectedIndices[0] < _view.Count ? _view[listViewStrings.SelectedIndices[0]] : -1;

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
            toolStripTextBoxFilter.Text = "#" + index;
            ApplyFilter();
            toolStripStatusLabel.Text = existing != 0 ? $"The text already exists as #{index}." : $"Added as #{index}. Save to keep it.";
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
            listViewStrings.Invalidate();
            toolStripButtonSave.Enabled = true;
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
                toolStripButtonSave.Enabled = false;
                toolStripStatusLabel.Text = $"Saved {target}.";
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Saving failed (is the file opened by the client?).", ex);
            }
        }

        // ==================================================================== event handlers (designer)

        private void OnFilterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ApplyFilter();
            }
        }

        private void OnClickApply(object sender, EventArgs e) => ApplyFilter();

        private void OnClickAdd(object sender, EventArgs e) => AddString();

        private void OnClickEdit(object sender, EventArgs e) => EditSelected();

        private void OnClickExport(object sender, EventArgs e) => Export();

        private void OnClickSave(object sender, EventArgs e) => Save(false);

        private void OnClickSaveAs(object sender, EventArgs e) => Save(true);

        private void OnClickCopyText(object sender, EventArgs e)
        {
            int i = SelectedIndex;
            if (i > 0)
            {
                Clipboard.SetText(_strings.Get((uint)i));
            }
        }

        private void OnClickCopyIndex(object sender, EventArgs e)
        {
            int i = SelectedIndex;
            if (i > 0)
            {
                Clipboard.SetText(i.ToString());
            }
        }

    }
}

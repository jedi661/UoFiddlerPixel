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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Strings;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Editor for LocalizedStrings.uop (the cliloc texts of the Enhanced Client, one file per language).
    /// </summary>
    public sealed partial class EcClilocControl : UserControl, IEcEditor
    {
        public const string FileName = "LocalizedStrings.uop";

        private EcClient _client;
        private EcUopPackage _package;
        private bool _loaded;
        private readonly Dictionary<EcUopEntry, EcCliloc> _languages = new Dictionary<EcUopEntry, EcCliloc>();
        private readonly HashSet<EcUopEntry> _changed = new HashSet<EcUopEntry>();
        private EcUopEntry _currentEntry;
        private EcCliloc _current;
        private List<EcClilocEntry> _view = new List<EcClilocEntry>();

        public EcClilocControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            _loaded = false;
            _package = null;
            _languages.Clear();
            _changed.Clear();
            _current = null;
            _view = new List<EcClilocEntry>();
            listViewCliloc.VirtualListSize = 0;
            toolStripComboBoxLanguage.Items.Clear();
            toolStripButtonSave.Enabled = false;
            if (Visible && client != null)
            {
                OnActivated();
            }
        }

        public void OnActivated()
        {
            if (_loaded || _client == null)
            {
                return;
            }
            _loaded = true;

            _package = _client.GetPackage(FileName);
            if (_package == null)
            {
                toolStripStatusLabel.Text = $"{FileName} was not found.";
                return;
            }

            _package.ResolveNames(EcCliloc.NamePattern, 0, 999);
            foreach (EcUopEntry entry in _package.Entries.OrderBy(e => e.Name ?? "~"))
            {
                int number = EcClient.TryGetIdFromName(entry.Name, out int n) ? n : -1;
                string title = number > 0 ? $"{number:D3} {EcCliloc.LanguageName(number)}" : entry.DisplayName;
                toolStripComboBoxLanguage.Items.Add(new KeyValuePair<string, EcUopEntry>(title, entry));
            }
            toolStripComboBoxLanguage.ComboBox.DisplayMember = "Key";
            if (toolStripComboBoxLanguage.Items.Count > 0)
            {
                toolStripComboBoxLanguage.SelectedIndex = 0;
            }
        }

        private void ShowLanguage()
        {
            if (toolStripComboBoxLanguage.SelectedItem is not KeyValuePair<string, EcUopEntry> item)
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                _currentEntry = item.Value;
                if (!_languages.TryGetValue(_currentEntry, out _current))
                {
                    _current = EcCliloc.Read(_package.ReadData(_currentEntry));
                    _languages[_currentEntry] = _current;
                }
                ApplyFilter();
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "The language can't be read.", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ApplyFilter()
        {
            if (_current == null)
            {
                return;
            }

            string filter = toolStripTextBoxSearch.Text.Trim();
            int jump = -1;
            if (filter.Length > 0 && int.TryParse(filter, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
            {
                _view = _current.Entries.ToList();
                jump = _view.FindIndex(e => e.Id >= id);
            }
            else if (filter.Length > 0)
            {
                _view = _current.Entries.Where(e => e.Text != null && e.Text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            else
            {
                _view = _current.Entries.ToList();
            }

            listViewCliloc.VirtualListSize = _view.Count;
            listViewCliloc.Invalidate();
            if (jump >= 0)
            {
                listViewCliloc.SelectedIndices.Clear();
                listViewCliloc.SelectedIndices.Add(jump);
                listViewCliloc.EnsureVisible(jump);
            }
            toolStripStatusLabel.Text = $"{_current.Entries.Count:N0} texts" + (filter.Length > 0 && jump < 0 ? $", {_view.Count:N0} found" : string.Empty);
        }

        private void RetrieveItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= _view.Count)
            {
                e.Item = new ListViewItem(string.Empty);
                return;
            }
            EcClilocEntry entry = _view[e.ItemIndex];
            var item = new ListViewItem(entry.Id.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(entry.Flag.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(entry.Text);
            e.Item = item;
        }

        private EcClilocEntry Selected => listViewCliloc.SelectedIndices.Count == 1 && listViewCliloc.SelectedIndices[0] < _view.Count ? _view[listViewCliloc.SelectedIndices[0]] : null;

        private void ShowSelected()
        {
            textBoxText.Text = Selected?.Text ?? string.Empty;
        }

        private void MarkChanged()
        {
            _changed.Add(_currentEntry);
            toolStripButtonSave.Enabled = true;
            listViewCliloc.Invalidate();
        }

        private void ApplyText()
        {
            EcClilocEntry entry = Selected;
            if (entry == null)
            {
                return;
            }
            entry.Text = textBoxText.Text;
            MarkChanged();
        }

        private void AddEntry()
        {
            if (_current == null)
            {
                return;
            }
            int next = _current.Entries.Count == 0 ? 1000000 : _current.Entries.Max(e => e.Id) + 1;
            string[] values = EcDialogs.Prompt(this, "Add text", new[] { "Number:", "Text:" }, new[] { next.ToString(CultureInfo.InvariantCulture), string.Empty });
            if (values == null || !int.TryParse(values[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
            {
                return;
            }
            if (_current.Find(id) != null && !EcDialogs.Confirm(this, $"{id} exists already. Replace the text?"))
            {
                return;
            }
            _current.Set(id, values[1]);
            MarkChanged();
            toolStripTextBoxSearch.Text = id.ToString(CultureInfo.InvariantCulture);
            ApplyFilter();
        }

        private void DeleteEntry()
        {
            EcClilocEntry entry = Selected;
            if (entry == null || !EcDialogs.Confirm(this, $"Delete text {entry.Id}?"))
            {
                return;
            }
            _current.Entries.Remove(entry);
            MarkChanged();
            ApplyFilter();
        }

        private void ExportText()
        {
            if (_current == null)
            {
                return;
            }
            using (var dialog = new SaveFileDialog { Filter = "Text|*.txt", FileName = "cliloc.txt" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllLines(dialog.FileName, _current.Entries.Select(e => $"{e.Id}\t{e.Text.Replace("\r", "\\r").Replace("\n", "\\n")}"), Encoding.UTF8);
                }
            }
        }

        private void ImportText()
        {
            if (_current == null)
            {
                return;
            }
            using (var dialog = new OpenFileDialog { Filter = "Text|*.txt|All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                int count = 0;
                foreach (string line in File.ReadAllLines(dialog.FileName, Encoding.UTF8))
                {
                    int tab = line.IndexOf('\t');
                    if (tab > 0 && int.TryParse(line.Substring(0, tab), NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
                    {
                        _current.Set(id, line.Substring(tab + 1).Replace("\\r", "\r").Replace("\\n", "\n"));
                        count++;
                    }
                }
                if (count > 0)
                {
                    MarkChanged();
                    ApplyFilter();
                }
                toolStripStatusLabel.Text = $"{count} texts imported.";
            }
        }

        private void Save(bool saveAs)
        {
            if (_package == null)
            {
                return;
            }
            foreach (EcUopEntry entry in _changed)
            {
                _package.SetData(entry, _languages[entry].Write());
            }
            _changed.Clear();
            if (EcPackageSaver.Save(this, new[] { _package }, saveAs))
            {
                toolStripButtonSave.Enabled = false;
                toolStripStatusLabel.Text = "Saved.";
            }
        }

        // ==================================================================== event handlers (designer)

        private void OnLanguageChanged(object sender, EventArgs e) => ShowLanguage();

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ApplyFilter();
            }
        }

        private void OnClickApply(object sender, EventArgs e) => ApplyFilter();

        private void OnClickAdd(object sender, EventArgs e) => AddEntry();

        private void OnClickDelete(object sender, EventArgs e) => DeleteEntry();

        private void OnClickExportText(object sender, EventArgs e) => ExportText();

        private void OnClickImportText(object sender, EventArgs e) => ImportText();

        private void OnClickSave(object sender, EventArgs e) => Save(false);

        private void OnClickSaveAs(object sender, EventArgs e) => Save(true);

        private void OnListSelectedIndexChanged(object sender, EventArgs e) => ShowSelected();

        private void OnClickApplyText(object sender, EventArgs e) => ApplyText();

    }
}

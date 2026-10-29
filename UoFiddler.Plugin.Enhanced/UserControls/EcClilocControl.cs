/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
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
    public sealed class EcClilocControl : UserControl, IEcEditor
    {
        public const string FileName = "LocalizedStrings.uop";

        private readonly ToolStripComboBox _languageBox;
        private readonly ToolStripTextBox _searchBox;
        private readonly ToolStripButton _saveButton;
        private readonly ListView _list;
        private readonly TextBox _textBox;
        private readonly ToolStripStatusLabel _statusLabel;

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
            Dock = DockStyle.Fill;

            var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _languageBox = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
            _languageBox.SelectedIndexChanged += (s, e) => ShowLanguage();
            _searchBox = new ToolStripTextBox { Width = 200, ToolTipText = "Number or part of the text – Enter" };
            _searchBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ApplyFilter(); } };
            _saveButton = new ToolStripButton("Save", null, (s, e) => Save(false)) { Enabled = false };

            toolStrip.Items.Add(new ToolStripLabel("Language:"));
            toolStrip.Items.Add(_languageBox);
            toolStrip.Items.Add(new ToolStripLabel("Search:"));
            toolStrip.Items.Add(_searchBox);
            toolStrip.Items.Add(new ToolStripButton("Apply", null, (s, e) => ApplyFilter()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("Add...", null, (s, e) => AddEntry()));
            toolStrip.Items.Add(new ToolStripButton("Delete", null, (s, e) => DeleteEntry()));
            var moreButton = new ToolStripDropDownButton("Import / Export");
            moreButton.DropDownItems.Add("Export language as text (id <tab> text)...", null, (s, e) => ExportText());
            moreButton.DropDownItems.Add("Import text (id <tab> text, adds / changes)...", null, (s, e) => ImportText());
            toolStrip.Items.Add(moreButton);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(_saveButton);
            toolStrip.Items.Add(new ToolStripButton("Save as...", null, (s, e) => Save(true)));

            _list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false, VirtualMode = true, MultiSelect = false };
            _list.Columns.Add("Number", 90, HorizontalAlignment.Right);
            _list.Columns.Add("Flag", 40);
            _list.Columns.Add("Text", 800);
            _list.RetrieveVirtualItem += RetrieveItem;
            _list.SelectedIndexChanged += (s, e) => ShowSelected();

            _textBox = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
            var applyButton = new Button { Text = "Apply text", Dock = DockStyle.Right, Width = 100 };
            applyButton.Click += (s, e) => ApplyText();
            var editPanel = new Panel { Dock = DockStyle.Bottom, Height = 80 };
            editPanel.Controls.Add(_textBox);
            editPanel.Controls.Add(applyButton);

            var status = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Choose the Enhanced Client folder on top.") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            status.Items.Add(_statusLabel);

            Controls.Add(_list);
            Controls.Add(editPanel);
            Controls.Add(toolStrip);
            Controls.Add(status);
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
            _list.VirtualListSize = 0;
            _languageBox.Items.Clear();
            _saveButton.Enabled = false;
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
                _statusLabel.Text = $"{FileName} was not found.";
                return;
            }

            _package.ResolveNames(EcCliloc.NamePattern, 0, 999);
            foreach (EcUopEntry entry in _package.Entries.OrderBy(e => e.Name ?? "~"))
            {
                int number = EcClient.TryGetIdFromName(entry.Name, out int n) ? n : -1;
                string title = number > 0 ? $"{number:D3} {EcCliloc.LanguageName(number)}" : entry.DisplayName;
                _languageBox.Items.Add(new KeyValuePair<string, EcUopEntry>(title, entry));
            }
            _languageBox.ComboBox.DisplayMember = "Key";
            if (_languageBox.Items.Count > 0)
            {
                _languageBox.SelectedIndex = 0;
            }
        }

        private void ShowLanguage()
        {
            if (_languageBox.SelectedItem is not KeyValuePair<string, EcUopEntry> item)
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

            string filter = _searchBox.Text.Trim();
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

            _list.VirtualListSize = _view.Count;
            _list.Invalidate();
            if (jump >= 0)
            {
                _list.SelectedIndices.Clear();
                _list.SelectedIndices.Add(jump);
                _list.EnsureVisible(jump);
            }
            _statusLabel.Text = $"{_current.Entries.Count:N0} texts" + (filter.Length > 0 && jump < 0 ? $", {_view.Count:N0} found" : string.Empty);
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

        private EcClilocEntry Selected => _list.SelectedIndices.Count == 1 && _list.SelectedIndices[0] < _view.Count ? _view[_list.SelectedIndices[0]] : null;

        private void ShowSelected()
        {
            _textBox.Text = Selected?.Text ?? string.Empty;
        }

        private void MarkChanged()
        {
            _changed.Add(_currentEntry);
            _saveButton.Enabled = true;
            _list.Invalidate();
        }

        private void ApplyText()
        {
            EcClilocEntry entry = Selected;
            if (entry == null)
            {
                return;
            }
            entry.Text = _textBox.Text;
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
            _searchBox.Text = id.ToString(CultureInfo.InvariantCulture);
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
                _statusLabel.Text = $"{count} texts imported.";
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
                _saveButton.Enabled = false;
                _statusLabel.Text = "Saved.";
            }
        }
    }
}

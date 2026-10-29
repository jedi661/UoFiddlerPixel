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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.Models;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Strings;
using UoFiddler.Plugin.Enhanced.EcUltima.TileArt;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Editor for tileart.uop (the tile data of the Enhanced Client).
    /// </summary>
    public sealed class EcTileArtControl : UserControl, IEcEditor
    {
        private static readonly TileArtFlags[] _flagValues = Enum.GetValues(typeof(TileArtFlags)).Cast<TileArtFlags>().Where(f => f != TileArtFlags.None).ToArray();

        private readonly ToolStripTextBox _filterBox;
        private readonly ToolStripComboBox _rangeBox;
        private readonly ToolStripButton _saveButton;
        private readonly ListView _list;
        private readonly PropertyGrid _propertyGrid;
        private readonly CheckedListBox _flagList;
        private readonly CheckedListBox _flag2List;
        private readonly ListView _textureList;
        private readonly EcPreviewPanel _texturePreview;
        private readonly Label _textureInfo;
        private readonly ToolStripStatusLabel _statusLabel;

        private EcClient _client;
        private TileArtFile _tileArt;
        private EcStringDictionary _strings;
        private List<TileArtRecord> _view = new List<TileArtRecord>();
        private bool _loaded;
        private bool _updatingFlags;

        /// <summary>
        /// Raised with the art source and id when the user wants to see a texture in the Art tab.
        /// </summary>
        public event Action<EcArtSource, int> ShowInArtRequested;

        public EcTileArtControl()
        {
            Dock = DockStyle.Fill;

            // ---------------- tool bar
            var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _filterBox = new ToolStripTextBox { Width = 160, ToolTipText = "Id (decimal / 0x hex) or part of the name – Enter" };
            _filterBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ApplyFilter(); } };
            _rangeBox = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
            _rangeBox.Items.AddRange(new object[] { "All entries", "0x0000 - 0x3FFF", "0x4000 - 0xFFFF", ">= 0x10000", "EC only (>= 10000000)", "Modified", "Id differs from file" });
            _rangeBox.SelectedIndex = 0;
            _rangeBox.SelectedIndexChanged += (s, e) => ApplyFilter();
            _saveButton = new ToolStripButton("Save", null, (s, e) => Save(false)) { Enabled = false, ToolTipText = "Save tileart.uop (and string_dictionary.uop if names were added)" };

            toolStrip.Items.Add(new ToolStripLabel("Search:"));
            toolStrip.Items.Add(_filterBox);
            toolStrip.Items.Add(new ToolStripButton("Apply", null, (s, e) => ApplyFilter()));
            toolStrip.Items.Add(_rangeBox);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("Duplicate...", null, (s, e) => DuplicateSelected()) { ToolTipText = "Create a new entry as copy of the selected one" });
            toolStrip.Items.Add(new ToolStripButton("Remove", null, (s, e) => RemoveSelected()));
            var moreButton = new ToolStripDropDownButton("Entry");
            moreButton.DropDownItems.Add("Export entry (.bin)...", null, (s, e) => ExportEntry());
            moreButton.DropDownItems.Add("Import entry (.bin) into selected...", null, (s, e) => ImportEntry());
            moreButton.DropDownItems.Add(new ToolStripSeparator());
            moreButton.DropDownItems.Add("Export list (csv)...", null, (s, e) => ExportCsv());
            toolStrip.Items.Add(moreButton);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(_saveButton);
            toolStrip.Items.Add(new ToolStripButton("Save as...", null, (s, e) => Save(true)) { ToolTipText = "Write tileart.uop (and string_dictionary.uop) into another folder" });

            // ---------------- list
            _list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false, VirtualMode = true, MultiSelect = false };
            _list.Columns.Add("Id", 70, HorizontalAlignment.Right);
            _list.Columns.Add("Hex", 70);
            _list.Columns.Add("Name", 200);
            _list.Columns.Add("Flags", 220);
            _list.RetrieveVirtualItem += RetrieveItem;
            _list.SelectedIndexChanged += (s, e) => ShowSelected();

            // ---------------- details
            _propertyGrid = new PropertyGrid { Dock = DockStyle.Fill, ToolbarVisible = false, PropertySort = PropertySort.Categorized };
            _propertyGrid.PropertyValueChanged += (s, e) => { RefreshFlags(); _list.Invalidate(); };

            _flagList = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, IntegralHeight = false, MultiColumn = true, ColumnWidth = 130 };
            foreach (TileArtFlags flag in _flagValues)
            {
                _flagList.Items.Add(flag.ToString());
            }
            _flagList.ItemCheck += FlagChecked;

            _flag2List = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, IntegralHeight = false, MultiColumn = true, ColumnWidth = 130 };
            foreach (TileArtFlags flag in _flagValues)
            {
                _flag2List.Items.Add(flag.ToString());
            }
            _flag2List.ItemCheck += Flag2Checked;

            _textureList = new ListView { Dock = DockStyle.Top, Height = 150, View = View.Details, FullRowSelect = true, HideSelection = false, MultiSelect = false };
            _textureList.Columns.Add("Slot", 40);
            _textureList.Columns.Add("Image (string dictionary)", 300);
            _textureList.Columns.Add("Index", 70);
            _textureList.SelectedIndexChanged += (s, e) => ShowTexture();
            _textureList.DoubleClick += (s, e) => ChangeTexture();

            var textureStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
            textureStrip.Items.Add(new ToolStripButton("Change image...", null, (s, e) => ChangeTexture()) { ToolTipText = "Point the reference to another image path, e.g. Data\\WorldArt\\00012345_name.tga" });
            textureStrip.Items.Add(new ToolStripButton("Show in Art tab", null, (s, e) => ShowTextureInArt()));

            _texturePreview = new EcPreviewPanel { Dock = DockStyle.Fill };
            _textureInfo = new Label { Dock = DockStyle.Top, Height = 44, Padding = new Padding(4), Font = EcDialogs.MonoFont(8.5f) };

            var texturePage = new TabPage("Textures");
            texturePage.Controls.Add(_texturePreview);
            texturePage.Controls.Add(_textureInfo);
            texturePage.Controls.Add(_textureList);
            texturePage.Controls.Add(textureStrip);

            var flagPage = new TabPage("Flags (EC)");
            flagPage.Controls.Add(_flagList);
            var flag2Page = new TabPage("Flags 2 (classic)");
            flag2Page.Controls.Add(_flag2List);
            flag2Page.Controls.Add(new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = "Second flag field: same bits as the flags of the classic tiledata.mul (in 2/3 of the entries both fields are equal)."
            });

            var detailTabs = new TabControl { Dock = DockStyle.Fill };
            detailTabs.TabPages.Add(texturePage);
            detailTabs.TabPages.Add(flagPage);
            detailTabs.TabPages.Add(flag2Page);

            var detailSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
            detailSplit.Panel1.Controls.Add(_propertyGrid);
            detailSplit.Panel2.Controls.Add(detailTabs);
            detailSplit.HandleCreated += (s, e) => { try { detailSplit.SplitterDistance = Math.Max(150, detailSplit.Height / 2); } catch (InvalidOperationException) { } };

            var split = new SplitContainer { Dock = DockStyle.Fill };
            split.Panel1.Controls.Add(_list);
            split.Panel2.Controls.Add(detailSplit);
            split.HandleCreated += (s, e) => { try { split.SplitterDistance = Math.Max(250, split.Width / 2 - 100); } catch (InvalidOperationException) { } };

            var status = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Choose the Enhanced Client folder on top.") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            status.Items.Add(_statusLabel);

            Controls.Add(split);
            Controls.Add(toolStrip);
            Controls.Add(status);
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            _tileArt = null;
            _strings = null;
            _view = new List<TileArtRecord>();
            _list.VirtualListSize = 0;
            _propertyGrid.SelectedObject = null;
            _textureList.Items.Clear();
            _texturePreview.Image = null;
            _loaded = false;
            _saveButton.Enabled = false;
            _statusLabel.Text = client == null ? "Choose the Enhanced Client folder on top." : "Ready.";
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

            Cursor = Cursors.WaitCursor;
            try
            {
                _statusLabel.Text = "Loading tileart.uop...";
                Application.DoEvents();

                _strings = _client.HasStringDictionary ? _client.StringDictionary : null;
                _tileArt = _client.GetTileArt();
                if (_tileArt == null)
                {
                    _statusLabel.Text = $"tileart.uop was not found in {_client.Folder}.";
                    return;
                }

                ApplyFilter();
                _statusLabel.Text = $"tileart.uop: {_tileArt.Count} entries" +
                                    (_tileArt.FailedEntries > 0 ? $", {_tileArt.FailedEntries} could not be read" : string.Empty) +
                                    (_strings == null ? " – string_dictionary.uop not found, names are not shown" : $", {_strings.Count} strings");
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Loading tileart.uop failed.", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private string NameOf(TileArtRecord r) => _strings?.Get(r.Entry.NameIndex) ?? string.Empty;

        private void ApplyFilter()
        {
            if (_tileArt == null)
            {
                return;
            }

            TileArtRecord selected = SelectedRecord;
            IEnumerable<TileArtRecord> records = _tileArt.Records;
            switch (_rangeBox.SelectedIndex)
            {
                case 1: records = records.Where(r => r.Id < 0x4000); break;
                case 2: records = records.Where(r => r.Id >= 0x4000 && r.Id < 0x10000); break;
                case 3: records = records.Where(r => r.Id >= 0x10000); break;
                case 4: records = records.Where(r => r.Id >= 10000000); break;
                case 5: records = records.Where(r => r.IsModified); break;
                case 6: records = records.Where(r => r.IdMismatch || !r.NameKnown); break;
            }

            string filter = _filterBox.Text.Trim();
            int jumpTo = -1;
            if (filter.Length > 0)
            {
                if (EcDialogs.TryParseNumber(filter, out long id))
                {
                    // numbers jump to the id instead of filtering
                    _view = records.ToList();
                    jumpTo = _view.FindIndex(r => r.Id >= id);
                }
                else
                {
                    _view = records.Where(r => NameOf(r).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }
            }
            else
            {
                _view = records.ToList();
            }

            _list.VirtualListSize = _view.Count;
            _list.Invalidate();

            int index = jumpTo >= 0 ? jumpTo : selected != null ? _view.IndexOf(selected) : -1;
            Select(index >= 0 ? index : (_view.Count > 0 ? 0 : -1));
        }

        private void Select(int index)
        {
            _list.SelectedIndices.Clear();
            if (index >= 0 && index < _view.Count)
            {
                _list.SelectedIndices.Add(index);
                _list.EnsureVisible(index);
            }
            else
            {
                ShowSelected();
            }
        }

        private void RetrieveItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= _view.Count)
            {
                e.Item = new ListViewItem(string.Empty);
                return;
            }

            TileArtRecord r = _view[e.ItemIndex];
            var item = new ListViewItem(r.Id.ToString());
            item.SubItems.Add($"0x{r.Id:X4}");
            item.SubItems.Add(NameOf(r));
            item.SubItems.Add(r.Entry.Flags == TileArtFlags.None ? string.Empty : r.Entry.Flags.ToString());
            if (r.IsModified)
            {
                item.ForeColor = Color.DarkBlue;
            }
            else if (r.IdMismatch || !r.NameKnown)
            {
                item.ForeColor = Color.DarkRed;
            }
            e.Item = item;
        }

        private TileArtRecord SelectedRecord => _list.SelectedIndices.Count == 1 && _list.SelectedIndices[0] < _view.Count ? _view[_list.SelectedIndices[0]] : null;

        private void ShowSelected()
        {
            TileArtRecord record = SelectedRecord;
            _textureList.Items.Clear();
            _texturePreview.Image = null;
            _textureInfo.Text = string.Empty;

            if (record == null)
            {
                _propertyGrid.SelectedObject = null;
                RefreshFlags();
                return;
            }

            _propertyGrid.SelectedObject = new TileArtEntryView(record, _tileArt, _strings, EntryChanged);
            RefreshFlags();

            foreach (TileArtTexture texture in record.Entry.Textures)
            {
                foreach (TileArtTextureImage image in texture.Images)
                {
                    string path = _strings?.Get(image.NameIndex) ?? $"#{image.NameIndex}";
                    var item = new ListViewItem(texture.Slot.ToString()) { Tag = image };
                    item.SubItems.Add(path);
                    item.SubItems.Add(image.NameIndex.ToString());
                    _textureList.Items.Add(item);
                }
            }

            if (!record.Entry.TexturesComplete)
            {
                _textureInfo.Text = "The texture data of this entry has an unknown layout;\r\nonly the references shown can be changed.";
            }

            if (_textureList.Items.Count > 0)
            {
                _textureList.Items[0].Selected = true;
            }

            if (record.IdMismatch)
            {
                _statusLabel.Text = $"Note: entry {record.Id} stores id {record.Entry.TileId} inside its data.";
            }
        }

        private void EntryChanged()
        {
            _saveButton.Enabled = true;
            _list.Invalidate();
        }

        private void RefreshFlags()
        {
            TileArtRecord record = SelectedRecord;
            _updatingFlags = true;
            for (int i = 0; i < _flagValues.Length; i++)
            {
                _flagList.SetItemChecked(i, record != null && (record.Entry.Flags & _flagValues[i]) != 0);
                _flag2List.SetItemChecked(i, record != null && (record.Entry.Flags2 & (ulong)_flagValues[i]) != 0);
            }
            _flagList.Enabled = record != null;
            _flag2List.Enabled = record != null;
            _updatingFlags = false;
        }

        private void FlagChecked(object sender, ItemCheckEventArgs e)
        {
            TileArtRecord record = SelectedRecord;
            if (_updatingFlags || record == null)
            {
                return;
            }

            TileArtFlags flag = _flagValues[e.Index];
            record.Entry.Flags = e.NewValue == CheckState.Checked ? record.Entry.Flags | flag : record.Entry.Flags & ~flag;
            _tileArt.MarkModified(record);
            _propertyGrid.Refresh();
            EntryChanged();
        }

        private void Flag2Checked(object sender, ItemCheckEventArgs e)
        {
            TileArtRecord record = SelectedRecord;
            if (_updatingFlags || record == null)
            {
                return;
            }

            ulong flag = (ulong)_flagValues[e.Index];
            record.Entry.Flags2 = e.NewValue == CheckState.Checked ? record.Entry.Flags2 | flag : record.Entry.Flags2 & ~flag;
            _tileArt.MarkModified(record);
            _propertyGrid.Refresh();
            EntryChanged();
        }

        // ------------------------------------------------------------------ textures

        private TileArtTextureImage SelectedTextureImage => _textureList.SelectedItems.Count == 1 ? _textureList.SelectedItems[0].Tag as TileArtTextureImage : null;

        private void ShowTexture()
        {
            TileArtTextureImage image = SelectedTextureImage;
            _texturePreview.Image = null;
            if (image == null || _client == null)
            {
                return;
            }

            string path = _strings?.Get(image.NameIndex);
            if (!EcClient.TryParseTexturePath(path, out EcArtSource source, out int id))
            {
                _textureInfo.Text = path == null ? "Unknown reference." : "No image path.";
                return;
            }

            byte[] data = FindTextureData(source, id, out string file);
            if (data == null)
            {
                _textureInfo.Text = $"{source.GetName(id)}\r\nnot found in {string.Join(" / ", source.FileNames)}";
                return;
            }

            _texturePreview.Image = EcImageHelper.TryDecode(data, out string info);
            _textureInfo.Text = $"{file}: {source.GetName(id)}\r\n{info}";
        }

        private byte[] FindTextureData(EcArtSource source, int id, out string file)
        {
            file = null;
            foreach (string fileName in source.FileNames)
            {
                try
                {
                    EcUopPackage package = _client.GetPackage(fileName);
                    EcUopEntry entry = package?.GetEntry(source.GetName(id));
                    if (entry != null)
                    {
                        file = fileName;
                        return package.ReadData(entry);
                    }
                }
                catch (Exception)
                {
                    // try the next file
                }
            }
            return null;
        }

        private void ChangeTexture()
        {
            TileArtRecord record = SelectedRecord;
            TileArtTextureImage image = SelectedTextureImage;
            if (record == null || image == null)
            {
                return;
            }

            if (_strings == null)
            {
                EcDialogs.ShowError(this, "string_dictionary.uop is needed to change image references.");
                return;
            }

            string current = _strings.Get(image.NameIndex) ?? string.Empty;
            string text = EcDialogs.Prompt(this, "Change image reference",
                "Image path (e.g. Data\\WorldArt\\00012345_name.tga):", current);
            if (string.IsNullOrWhiteSpace(text) || text == current)
            {
                return;
            }

            uint index = _strings.GetOrAdd(text.Trim());
            record.Entry.SetTextureNameIndex(image, index);
            _tileArt.MarkModified(record);
            EntryChanged();

            int selectedRow = _textureList.SelectedIndices[0];
            ShowSelected();
            if (selectedRow < _textureList.Items.Count)
            {
                _textureList.Items[selectedRow].Selected = true;
            }
        }

        private void ShowTextureInArt()
        {
            TileArtTextureImage image = SelectedTextureImage;
            if (image != null && EcClient.TryParseTexturePath(_strings?.Get(image.NameIndex), out EcArtSource source, out int id))
            {
                ShowInArtRequested?.Invoke(source, id);
            }
        }

        // ------------------------------------------------------------------ entries

        private void DuplicateSelected()
        {
            TileArtRecord record = SelectedRecord;
            if (record == null)
            {
                return;
            }

            uint suggestion = _tileArt.NextFreeId(record.Id + 1);
            string text = EcDialogs.Prompt(this, "Duplicate entry", $"New id for the copy of {record.Id} (decimal or 0x hex):", suggestion.ToString());
            if (text == null || !EcDialogs.TryParseNumber(text, out long id) || id < 0 || id > uint.MaxValue)
            {
                return;
            }

            if (_tileArt.Contains((uint)id))
            {
                EcDialogs.ShowError(this, $"Id {id} already exists.");
                return;
            }

            TileArtRecord copy = _tileArt.Duplicate(record, (uint)id);
            _saveButton.Enabled = true;
            _filterBox.Text = string.Empty;
            ApplyFilter();
            Select(_view.IndexOf(copy));
            _statusLabel.Text = $"Entry {id} created as copy of {record.Id}. Change name and textures, then save.";
        }

        private void RemoveSelected()
        {
            TileArtRecord record = SelectedRecord;
            if (record == null || !EcDialogs.Confirm(this, $"Remove tileart entry {record.Id} ({NameOf(record)})?"))
            {
                return;
            }

            int index = _list.SelectedIndices[0];
            _tileArt.Remove(record);
            _saveButton.Enabled = true;
            ApplyFilter();
            Select(Math.Min(index, _view.Count - 1));
        }

        private void ExportEntry()
        {
            TileArtRecord record = SelectedRecord;
            if (record == null)
            {
                return;
            }

            using (var dialog = new SaveFileDialog { Filter = "Tileart entry (*.bin)|*.bin", FileName = $"{record.Id:D8}.bin" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllBytes(dialog.FileName, record.Entry.Write());
                }
            }
        }

        private void ImportEntry()
        {
            TileArtRecord record = SelectedRecord;
            if (record == null)
            {
                return;
            }

            using (var dialog = new OpenFileDialog { Filter = "Tileart entry (*.bin)|*.bin|All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    TileArtEntry imported = TileArtEntry.Read(File.ReadAllBytes(dialog.FileName));
                    imported.TileId = record.Id;
                    record.Entry = imported;
                    _tileArt.MarkModified(record);
                    EntryChanged();
                    ShowSelected();
                }
                catch (Exception ex)
                {
                    EcDialogs.ShowError(this, "The file is no valid tileart entry.", ex);
                }
            }
        }

        private void ExportCsv()
        {
            if (_tileArt == null)
            {
                return;
            }

            using (var dialog = new SaveFileDialog { Filter = "CSV|*.csv", FileName = "tileart.csv" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                using (var writer = new StreamWriter(dialog.FileName))
                {
                    writer.WriteLine("Id;Hex;Name;Flags;Weight;Height;RadarColor");
                    foreach (TileArtRecord r in _view)
                    {
                        TileArtEntry e = r.Entry;
                        writer.WriteLine($"{r.Id};0x{r.Id:X};{NameOf(r).Replace(';', ',')};{e.Flags};{e.GetProperty(TileArtPropertyId.Weight)};{e.GetProperty(TileArtPropertyId.Height)};#{e.RadarR:X2}{e.RadarG:X2}{e.RadarB:X2}");
                    }
                }
            }
        }

        private void Save(bool saveAs)
        {
            if (_tileArt == null)
            {
                return;
            }

            string folder = Path.GetDirectoryName(_tileArt.Package.FilePath);
            if (saveAs)
            {
                using (var dialog = new FolderBrowserDialog { Description = "Folder for tileart.uop (and string_dictionary.uop)" })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                    folder = dialog.SelectedPath;
                }
            }
            else if (!EcDialogs.Confirm(this, "Save tileart.uop" + (_strings?.IsModified == true ? " and string_dictionary.uop" : string.Empty) +
                                              "?\r\nThe old files are kept as .bak next to them.\r\n\r\nThe Enhanced Client must be closed."))
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                bool stringsChanged = _strings?.IsModified == true || (_strings?.Package.IsDirty ?? false);
                _client.ApplyPendingChanges();
                _tileArt.Package.Save(Path.Combine(folder, Path.GetFileName(_tileArt.Package.FilePath)));
                if (stringsChanged)
                {
                    _strings.Package.Save(Path.Combine(folder, Path.GetFileName(_strings.Package.FilePath)));
                }

                _saveButton.Enabled = false;
                _list.Invalidate();
                _statusLabel.Text = $"Saved to {folder}.";
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Saving failed (is the file opened by the client?).", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
    }
}

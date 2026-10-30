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
    public sealed partial class EcTileArtControl : UserControl, IEcEditor
    {
        private static readonly TileArtFlags[] _flagValues = Enum.GetValues(typeof(TileArtFlags)).Cast<TileArtFlags>().Where(f => f != TileArtFlags.None).ToArray();

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
            InitializeComponent();
            Dock = DockStyle.Fill;

            // runtime only: flag names from the enum
            toolStripComboBoxRange.SelectedIndex = 0;
            foreach (TileArtFlags flag in _flagValues)
            {
                checkedListBoxFlags.Items.Add(flag.ToString());
                checkedListBoxFlags2.Items.Add(flag.ToString());
            }
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            _tileArt = null;
            _strings = null;
            _view = new List<TileArtRecord>();
            listViewTiles.VirtualListSize = 0;
            propertyGrid.SelectedObject = null;
            listViewTextures.Items.Clear();
            previewPanelTexture.Image = null;
            _loaded = false;
            toolStripButtonSave.Enabled = false;
            toolStripStatusLabel.Text = client == null ? "Choose the Enhanced Client folder on top." : "Ready.";
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
                toolStripStatusLabel.Text = "Loading tileart.uop...";
                Application.DoEvents();

                _strings = _client.HasStringDictionary ? _client.StringDictionary : null;
                _tileArt = _client.GetTileArt();
                if (_tileArt == null)
                {
                    toolStripStatusLabel.Text = $"tileart.uop was not found in {_client.Folder}.";
                    return;
                }

                ApplyFilter();
                toolStripStatusLabel.Text = $"tileart.uop: {_tileArt.Count} entries" +
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
            switch (toolStripComboBoxRange.SelectedIndex)
            {
                case 1: records = records.Where(r => r.Id < 0x4000); break;
                case 2: records = records.Where(r => r.Id >= 0x4000 && r.Id < 0x10000); break;
                case 3: records = records.Where(r => r.Id >= 0x10000); break;
                case 4: records = records.Where(r => r.Id >= 10000000); break;
                case 5: records = records.Where(r => r.IsModified); break;
                case 6: records = records.Where(r => r.IdMismatch || !r.NameKnown); break;
            }

            string filter = toolStripTextBoxFilter.Text.Trim();
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

            listViewTiles.VirtualListSize = _view.Count;
            listViewTiles.Invalidate();

            int index = jumpTo >= 0 ? jumpTo : selected != null ? _view.IndexOf(selected) : -1;
            Select(index >= 0 ? index : (_view.Count > 0 ? 0 : -1));
        }

        private void Select(int index)
        {
            listViewTiles.SelectedIndices.Clear();
            if (index >= 0 && index < _view.Count)
            {
                listViewTiles.SelectedIndices.Add(index);
                listViewTiles.EnsureVisible(index);
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

        private TileArtRecord SelectedRecord => listViewTiles.SelectedIndices.Count == 1 && listViewTiles.SelectedIndices[0] < _view.Count ? _view[listViewTiles.SelectedIndices[0]] : null;

        private void ShowSelected()
        {
            TileArtRecord record = SelectedRecord;
            listViewTextures.Items.Clear();
            previewPanelTexture.Image = null;
            labelTextureInfo.Text = string.Empty;

            if (record == null)
            {
                propertyGrid.SelectedObject = null;
                RefreshFlags();
                return;
            }

            propertyGrid.SelectedObject = new TileArtEntryView(record, _tileArt, _strings, EntryChanged);
            RefreshFlags();

            foreach (TileArtTexture texture in record.Entry.Textures)
            {
                foreach (TileArtTextureImage image in texture.Images)
                {
                    string path = _strings?.Get(image.NameIndex) ?? $"#{image.NameIndex}";
                    var item = new ListViewItem(texture.Slot.ToString()) { Tag = image };
                    item.SubItems.Add(path);
                    item.SubItems.Add(image.NameIndex.ToString());
                    listViewTextures.Items.Add(item);
                }
            }

            if (!record.Entry.TexturesComplete)
            {
                labelTextureInfo.Text = "The texture data of this entry has an unknown layout;\r\nonly the references shown can be changed.";
            }

            if (listViewTextures.Items.Count > 0)
            {
                listViewTextures.Items[0].Selected = true;
            }

            if (record.IdMismatch)
            {
                toolStripStatusLabel.Text = $"Note: entry {record.Id} stores id {record.Entry.TileId} inside its data.";
            }
        }

        private void EntryChanged()
        {
            toolStripButtonSave.Enabled = true;
            listViewTiles.Invalidate();
        }

        private void RefreshFlags()
        {
            TileArtRecord record = SelectedRecord;
            _updatingFlags = true;
            for (int i = 0; i < _flagValues.Length; i++)
            {
                checkedListBoxFlags.SetItemChecked(i, record != null && (record.Entry.Flags & _flagValues[i]) != 0);
                checkedListBoxFlags2.SetItemChecked(i, record != null && (record.Entry.Flags2 & (ulong)_flagValues[i]) != 0);
            }
            checkedListBoxFlags.Enabled = record != null;
            checkedListBoxFlags2.Enabled = record != null;
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
            propertyGrid.Refresh();
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
            propertyGrid.Refresh();
            EntryChanged();
        }

        // ------------------------------------------------------------------ textures

        private TileArtTextureImage SelectedTextureImage => listViewTextures.SelectedItems.Count == 1 ? listViewTextures.SelectedItems[0].Tag as TileArtTextureImage : null;

        private void ShowTexture()
        {
            TileArtTextureImage image = SelectedTextureImage;
            previewPanelTexture.Image = null;
            if (image == null || _client == null)
            {
                return;
            }

            string path = _strings?.Get(image.NameIndex);
            if (!EcClient.TryParseTexturePath(path, out EcArtSource source, out int id))
            {
                labelTextureInfo.Text = path == null ? "Unknown reference." : "No image path.";
                return;
            }

            byte[] data = FindTextureData(source, id, out string file);
            if (data == null)
            {
                labelTextureInfo.Text = $"{source.GetName(id)}\r\nnot found in {string.Join(" / ", source.FileNames)}";
                return;
            }

            previewPanelTexture.Image = EcImageHelper.TryDecode(data, out string info);
            labelTextureInfo.Text = $"{file}: {source.GetName(id)}\r\n{info}";
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

            int selectedRow = listViewTextures.SelectedIndices[0];
            ShowSelected();
            if (selectedRow < listViewTextures.Items.Count)
            {
                listViewTextures.Items[selectedRow].Selected = true;
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
            toolStripButtonSave.Enabled = true;
            toolStripTextBoxFilter.Text = string.Empty;
            ApplyFilter();
            Select(_view.IndexOf(copy));
            toolStripStatusLabel.Text = $"Entry {id} created as copy of {record.Id}. Change name and textures, then save.";
        }

        private void RemoveSelected()
        {
            TileArtRecord record = SelectedRecord;
            if (record == null || !EcDialogs.Confirm(this, $"Remove tileart entry {record.Id} ({NameOf(record)})?"))
            {
                return;
            }

            int index = listViewTiles.SelectedIndices[0];
            _tileArt.Remove(record);
            toolStripButtonSave.Enabled = true;
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

                toolStripButtonSave.Enabled = false;
                listViewTiles.Invalidate();
                toolStripStatusLabel.Text = $"Saved to {folder}.";
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

        private void OnRangeChanged(object sender, EventArgs e) => ApplyFilter();

        private void OnClickDuplicate(object sender, EventArgs e) => DuplicateSelected();

        private void OnClickRemove(object sender, EventArgs e) => RemoveSelected();

        private void OnClickExportEntry(object sender, EventArgs e) => ExportEntry();

        private void OnClickImportEntry(object sender, EventArgs e) => ImportEntry();

        private void OnClickExportCsv(object sender, EventArgs e) => ExportCsv();

        private void OnClickSave(object sender, EventArgs e) => Save(false);

        private void OnClickSaveAs(object sender, EventArgs e) => Save(true);

        private void OnListSelectedIndexChanged(object sender, EventArgs e) => ShowSelected();

        private void OnPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            RefreshFlags();
            listViewTiles.Invalidate();
        }

        private void OnTextureSelectedIndexChanged(object sender, EventArgs e) => ShowTexture();

        private void OnClickChangeTexture(object sender, EventArgs e) => ChangeTexture();

        private void OnClickShowInArt(object sender, EventArgs e) => ShowTextureInArt();

        private TileArtRecord FirstSelectedRecord =>
            listViewTiles.SelectedIndices.Count > 0 && listViewTiles.SelectedIndices[0] < _view.Count ? _view[listViewTiles.SelectedIndices[0]] : null;

        private void OnClickCopyId(object sender, EventArgs e)
        {
            TileArtRecord record = FirstSelectedRecord;
            if (record != null)
            {
                Clipboard.SetText($"0x{record.Id:X4}");
            }
        }

        private void OnClickCopyName(object sender, EventArgs e)
        {
            TileArtRecord record = FirstSelectedRecord;
            string name = record != null && _strings != null ? _strings.Get(record.Entry.NameIndex) : null;
            if (!string.IsNullOrEmpty(name))
            {
                Clipboard.SetText(name);
            }
        }

    }
}

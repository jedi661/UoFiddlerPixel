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
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Gumps;
using UoFiddler.Plugin.Enhanced.EcUltima.Imaging;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Gumps of the Enhanced Client: interface gumps (Interface.uop, TGA) and legacy gumps (gumpartLegacyMUL.uop, classic RLE).
    /// </summary>
    public sealed partial class EcGumpsControl : UserControl, IEcEditor
    {
        private sealed class Source
        {
            public string Title;
            public string File;
            public string Pattern;
            public bool Legacy;
            public override string ToString() => Title;
        }

        private static readonly Source[] _sources =
        {
            new Source { Title = "Interface gumps (Interface.uop)", File = "Interface.uop", Pattern = "data/interface/default/textures/gumpart/{0:D8}.tga" },
            new Source { Title = "Legacy gumps (gumpartLegacyMUL.uop)", File = "gumpartLegacyMUL.uop", Pattern = "build/gumpartlegacymul/{0:D8}.tga", Legacy = true },
        };

        private EcClient _client;
        private bool _loaded;
        private Source _source;
        private EcUopPackage _package;
        private List<(int Id, EcUopEntry Entry)> _items = new List<(int, EcUopEntry)>();

        public EcGumpsControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;

            // runtime only: source list, size and the image callbacks of the grid
            toolStripComboBoxSource.Items.AddRange(_sources.Cast<object>().ToArray());
            toolStripComboBoxSize.SelectedIndex = 2;
            thumbnailGrid.ImageRequested += index => index < _items.Count ? Decode(_items[index].Entry, out _) : null;
            thumbnailGrid.LabelRequested += index => index < _items.Count ? _items[index].Id + (_items[index].Entry.IsModified ? "*" : string.Empty) : string.Empty;
            thumbnailGrid.SelectionChanged += OnGridSelectionChanged;
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            _loaded = false;
            _package = null;
            _items = new List<(int, EcUopEntry)>();
            thumbnailGrid.ItemCount = 0;
            previewPanel.Image = null;
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
            if (toolStripComboBoxSource.SelectedIndex < 0)
            {
                toolStripComboBoxSource.SelectedIndex = 0;
            }
            _loaded = true;
            LoadSource();
        }

        private void LoadSource()
        {
            _source = toolStripComboBoxSource.SelectedItem as Source;
            if (_source == null)
            {
                return;
            }
            Cursor = Cursors.WaitCursor;
            try
            {
                _package = _client.GetPackage(_source.File);
                if (_package == null)
                {
                    _items = new List<(int, EcUopEntry)>();
                    thumbnailGrid.ItemCount = 0;
                    toolStripStatusLabel.Text = $"{_source.File} was not found.";
                    return;
                }
                _package.Changed -= PackageChanged;
                _package.Changed += PackageChanged;
                _package.ResolveNames(_source.Pattern, 0, 0x20000);
                BuildItems();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void PackageChanged(object sender, EventArgs e)
        {
            toolStripButtonSave.Enabled = _sources.Any(s => _client.IsOpen(s.File) && (_client.GetPackage(s.File)?.IsDirty ?? false));
        }

        private string Prefix => _source.Pattern.Substring(0, _source.Pattern.LastIndexOf('/') + 1);

        private void BuildItems()
        {
            string prefix = Prefix;
            _items = _package.Entries
                .Where(e => e.Name != null && e.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(e => (Id: EcClient.TryGetIdFromName(e.Name, out int id) ? id : -1, Entry: e))
                .Where(x => x.Id >= 0)
                .OrderBy(x => x.Id)
                .ToList();
            thumbnailGrid.ItemCount = _items.Count;
            toolStripStatusLabel.Text = $"{_source.File}: {_items.Count} gumps.";
        }

        private Bitmap Decode(EcUopEntry entry, out string info)
        {
            info = null;
            byte[] data = _package.ReadData(entry);
            if (_source.Legacy && EcLegacyGump.IsLegacyGump(data))
            {
                byte[] px = EcLegacyGump.Decode(data, out int w, out int h);
                info = $"{w} x {h}  classic gump (RGB555 runs)";
                return w > 0 && h > 0 ? EcImageHelper.ToBitmap(px, w, h) : null;
            }
            return EcImageHelper.TryDecode(data, out info, entry.Name);
        }

        private (int Id, EcUopEntry Entry)? Selected => thumbnailGrid.SelectedIndex >= 0 && thumbnailGrid.SelectedIndex < _items.Count ? _items[thumbnailGrid.SelectedIndex] : ((int, EcUopEntry)?)null;

        private void ShowSelected()
        {
            var sel = Selected;
            if (sel == null)
            {
                previewPanel.Image = null;
                labelInfo.Text = string.Empty;
                return;
            }
            try
            {
                previewPanel.Image = Decode(sel.Value.Entry, out string info);
                labelInfo.Text = $"Gump {sel.Value.Id} (0x{sel.Value.Id:X})\r\n{sel.Value.Entry.Name}\r\n{info ?? "empty"}";
            }
            catch (Exception ex)
            {
                previewPanel.Image = null;
                labelInfo.Text = ex.Message;
            }
        }

        private void Search()
        {
            if (!EcDialogs.TryParseNumber(toolStripTextBoxSearch.Text, out long id))
            {
                return;
            }
            int index = _items.FindIndex(i => i.Id >= id);
            if (index >= 0)
            {
                thumbnailGrid.SelectedIndex = index;
            }
        }

        private byte[] Encode(string file)
        {
            using (Bitmap bitmap = EcImageHelper.LoadImageFile(file))
            {
                byte[] px = EcImageHelper.ToBgra(bitmap, out int w, out int h);
                return _source.Legacy ? EcLegacyGump.Encode(px, w, h) : TgaImage.Encode(px, w, h);
            }
        }

        private static string ChooseImage(IWin32Window owner)
        {
            using (var dialog = new OpenFileDialog { Filter = "Images (*.png;*.bmp;*.gif;*.tga;*.dds)|*.png;*.bmp;*.gif;*.tga;*.dds|All files (*.*)|*.*" })
            {
                return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.FileName : null;
            }
        }

        private void Replace()
        {
            var sel = Selected;
            if (sel == null)
            {
                return;
            }
            string file = ChooseImage(this);
            if (file == null)
            {
                return;
            }
            try
            {
                _package.SetData(sel.Value.Entry, Encode(file));
                thumbnailGrid.InvalidateItem(thumbnailGrid.SelectedIndex);
                ShowSelected();
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Replacing failed.", ex);
            }
        }

        private void Insert()
        {
            if (_package == null)
            {
                return;
            }
            int next = _items.Count == 0 ? 0 : _items.Max(i => i.Id) + 1;
            string text = EcDialogs.Prompt(this, "Insert gump", "Gump id (decimal or 0x hex):", next.ToString(CultureInfo.InvariantCulture));
            if (text == null || !EcDialogs.TryParseNumber(text, out long id) || id < 0)
            {
                return;
            }
            string name = string.Format(CultureInfo.InvariantCulture, _source.Pattern, id);
            if (_package.Contains(name) && !EcDialogs.Confirm(this, $"Gump {id} exists already. Replace it?"))
            {
                return;
            }
            string file = ChooseImage(this);
            if (file == null)
            {
                return;
            }
            try
            {
                _package.AddOrReplace(name, Encode(file), _package.PreferredCompression);
                BuildItems();
                toolStripTextBoxSearch.Text = id.ToString(CultureInfo.InvariantCulture);
                Search();
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Inserting failed.", ex);
            }
        }

        private void RemoveSelected()
        {
            var sel = Selected;
            if (sel == null || !EcDialogs.Confirm(this, $"Remove gump {sel.Value.Id}?"))
            {
                return;
            }
            int index = thumbnailGrid.SelectedIndex;
            _package.Remove(sel.Value.Entry);
            BuildItems();
            thumbnailGrid.SelectedIndex = Math.Min(index, _items.Count - 1);
        }

        private void ExportSelected()
        {
            var sel = Selected;
            if (sel == null)
            {
                return;
            }
            using (var dialog = new SaveFileDialog { Filter = "PNG|*.png", FileName = $"gump_{sel.Value.Id:D8}.png" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    using (Bitmap b = Decode(sel.Value.Entry, out _))
                    {
                        b?.Save(dialog.FileName, ImageFormat.Png);
                    }
                }
            }
        }

        private void ExportAll()
        {
            using (var dialog = new FolderBrowserDialog { Description = "Folder for the PNG files" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                int done = 0;
                Cursor = Cursors.WaitCursor;
                try
                {
                    foreach (var (id, entry) in _items)
                    {
                        using (Bitmap b = Decode(entry, out _))
                        {
                            if (b != null)
                            {
                                b.Save(Path.Combine(dialog.SelectedPath, $"gump_{id:D8}.png"), ImageFormat.Png);
                                done++;
                            }
                        }
                        if (done % 200 == 0)
                        {
                            toolStripStatusLabel.Text = $"Exporting {done}...";
                            Application.DoEvents();
                        }
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
                toolStripStatusLabel.Text = $"{done} gumps exported.";
            }
        }

        private void Save(bool saveAs)
        {
            var packages = _sources.Where(s => _client != null && _client.IsOpen(s.File)).Select(s => _client.GetPackage(s.File));
            if (EcPackageSaver.Save(this, packages, saveAs))
            {
                toolStripButtonSave.Enabled = false;
                thumbnailGrid.ClearCache();
                thumbnailGrid.Invalidate();
                toolStripStatusLabel.Text = "Saved.";
            }
        }

        // ==================================================================== event handlers (designer)

        private void OnSourceChanged(object sender, EventArgs e)
        {
            if (_loaded)
            {
                LoadSource();
            }
        }

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Search();
            }
        }

        private void OnClickSearch(object sender, EventArgs e) => Search();

        private void OnSizeChanged(object sender, EventArgs e) =>
            thumbnailGrid.CellSize = int.Parse((string)toolStripComboBoxSize.SelectedItem, CultureInfo.InvariantCulture);

        private void OnClickReplace(object sender, EventArgs e) => Replace();

        private void OnClickInsert(object sender, EventArgs e) => Insert();

        private void OnClickRemove(object sender, EventArgs e) => RemoveSelected();

        private void OnClickExportSelected(object sender, EventArgs e) => ExportSelected();

        private void OnClickExportAll(object sender, EventArgs e) => ExportAll();

        private void OnClickSave(object sender, EventArgs e) => Save(false);

        private void OnClickSaveAs(object sender, EventArgs e) => Save(true);

        private void OnGridSelectionChanged(object sender, EventArgs e) => ShowSelected();

    }
}
